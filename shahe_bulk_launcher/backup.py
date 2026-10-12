import codecs
import datetime
import os
import re
import shutil
import stat
import subprocess
import sys
import threading
import time
import zipfile
from pathlib import Path, PurePosixPath, PureWindowsPath

import wx

from .bootstrap import get_app_dir, get_current_executable_path, is_packaged_app
from .common import get_update_temp_dir
from .constants import get_default_sandbox_root
from .ui.common_dialogs import SelectUsersDialog


def safe_extract_zip(zip_path, extract_dir):
    root = Path(extract_dir).resolve()
    root.mkdir(parents=True, exist_ok=True)

    with zipfile.ZipFile(zip_path, "r") as zf:
        members = []
        for info in zf.infolist():
            name = info.filename.replace("\\", "/")
            parts = PurePosixPath(name).parts
            windows_path = PureWindowsPath(info.filename)
            mode = info.external_attr >> 16
            if (
                not parts
                or name.startswith("/")
                or windows_path.drive
                or ".." in parts
                or any(":" in part for part in parts)
                or stat.S_ISLNK(mode)
            ):
                raise ValueError(f"备份文件包含不安全路径：{info.filename}")

            target = (root / Path(*parts)).resolve()
            try:
                target.relative_to(root)
            except ValueError as exc:
                raise ValueError(f"备份文件包含不安全路径：{info.filename}") from exc
            members.append((info, target, info.is_dir() or name.endswith("/")))

        for info, target, is_dir in members:
            if is_dir:
                target.mkdir(parents=True, exist_ok=True)
                continue
            target.parent.mkdir(parents=True, exist_ok=True)
            with zf.open(info, "r") as source, open(target, "wb") as destination:
                shutil.copyfileobj(source, destination)


class BackupRestoreMgr:
    def __init__(self, frame):
        self.frame = frame
        default_root = get_default_sandbox_root()
        self.sandbox_users_root = Path(
            str(getattr(self.frame, "sandbox_root", default_root) or default_root)
        )
        ini_path = ""
        if hasattr(self.frame, "_get_sandboxie_ini_path"):
            try:
                ini_path = str(self.frame._get_sandboxie_ini_path() or "").strip()
            except Exception:
                ini_path = ""
        if not ini_path:
            win_dir = os.environ.get("WINDIR", r"C:\Windows")
            ini_path = str(Path(win_dir) / "Sandboxie.ini")
        self.sandboxie_ini_path = Path(ini_path)

    def _safe_frame_call_after(self, callback, *args, **kwargs):
        frame = getattr(self, "frame", None)
        safe_call = getattr(frame, "_safe_call_after", None)
        if callable(safe_call):
            safe_call(callback, *args, **kwargs)
            return
        try:
            wx.CallAfter(callback, *args, **kwargs)
        except RuntimeError:
            pass

    def _iter_backup_user_roots(self, base_dir):
        root = Path(base_dir)
        candidates = [
            root / "SandboxUsers",
            root / "Sandbox" / str(os.environ.get("USERNAME") or "Administrator"),
            root / "Sandbox" / "Administrator",
        ]
        for candidate in candidates:
            if candidate.exists() and candidate.is_dir():
                yield candidate

    def _stop_sandboxie_services(self):
        try:
            subprocess.run(["net", "stop", "SbieSvc"], capture_output=True, text=True)
            time.sleep(1)
        except: pass
        for proc in ["Sandboxie.exe", "SandboxieRpcSs.exe", "SandboxieDcomLaunch.exe",
                     "SbieSvc.exe", "SandboxieCrypto.exe", "SandMan.exe", "Start.exe"]:
            try:
                subprocess.run(["taskkill", "/f", "/im", proc], capture_output=True, text=True)
                time.sleep(0.3)
            except: pass
        time.sleep(1)

    def _restart_sandboxie_services(self):
        try:
            subprocess.run(["net", "start", "SbieSvc"], capture_output=True, text=True)
        except: pass

    def _reload_sandboxie_config(self):
        if hasattr(self.frame, "_reload_sandboxie_config"):
            try:
                self.frame._reload_sandboxie_config()
                self._restart_sandboxie_services()
                return
            except Exception:
                pass
        for d in [r"C:\Program Files\Sandboxie", r"C:\Program Files (x86)\Sandboxie",
                  r"C:\Program Files\Sandboxie-Plus", r"C:\Program Files (x86)\Sandboxie-Plus"]:
            if os.path.exists(d):
                start_exe = os.path.join(d, "Start.exe")
                if os.path.exists(start_exe):
                    subprocess.run([start_exe, "/reload"], capture_output=True, text=True)
                else:
                    sandboxie_exe = os.path.join(d, "Sandboxie.exe")
                    if os.path.exists(sandboxie_exe):
                        subprocess.run([sandboxie_exe, "/reload"], capture_output=True, text=True)
                break
        self._restart_sandboxie_services()

    def _find_sandboxie_control_exe(self):
        base = str(getattr(self.frame, "SANDBOXIE_PATH", "") or "").strip()
        if base:
            for name in ("SandMan.exe", "SbieCtrl.exe", "Sandboxie.exe"):
                p = os.path.join(base, name)
                if os.path.isfile(p):
                    return p
        for p in [r"C:\Program Files\Sandboxie\SandMan.exe", r"C:\Program Files\Sandboxie\SbieCtrl.exe", r"C:\Program Files\Sandboxie\Sandboxie.exe",
                  r"C:\Program Files (x86)\Sandboxie\SandMan.exe", r"C:\Program Files (x86)\Sandboxie\SbieCtrl.exe", r"C:\Program Files (x86)\Sandboxie\Sandboxie.exe",
                  r"C:\Program Files\Sandboxie-Plus\SandMan.exe", r"C:\Program Files (x86)\Sandboxie-Plus\SandMan.exe"]:
            if os.path.exists(p):
                return p
        return None

    def _force_remove_dir(self, path):
        if not path.exists():
            return
        def on_rm_error(func, p, exc_info):
            try:
                os.chmod(p, 0o777)
                func(p)
            except:
                pass
        shutil.rmtree(path, ignore_errors=False, onerror=on_rm_error)

    def _merge_ini_sections(self, backup_ini_path, target_ini_path, sections_to_merge):
        if not sections_to_merge:
            return

        def _read_text_with_encoding(path_obj):
            p = Path(path_obj)
            if not p.exists():
                return "", "utf-8"
            data = p.read_bytes()
            if data.startswith(codecs.BOM_UTF8):
                return data.decode("utf-8-sig"), "utf-8-sig"
            for enc in ("utf-8", "utf-16", "gbk", "mbcs"):
                try:
                    return data.decode(enc), enc
                except Exception:
                    continue
            return data.decode("utf-8", errors="ignore"), "utf-8"

        def _split_sections(text):
            content = str(text or "")
            pattern = re.compile(r"(?m)^[ \t]*\[(?P<name>[^\]\r\n]+)\][^\r\n]*(?:\r?\n)?")
            matches = list(pattern.finditer(content))
            if not matches:
                return content, []
            preamble = content[:matches[0].start()]
            sections = []
            for i, match in enumerate(matches):
                section_name = str(match.group("name") or "").strip()
                start = match.start()
                end = matches[i + 1].start() if i + 1 < len(matches) else len(content)
                block = content[start:end]
                sections.append((section_name, block))
            return preamble, sections

        backup_path = Path(backup_ini_path)
        target_path = Path(target_ini_path)
        if not backup_path.exists():
            return

        backup_text, _ = _read_text_with_encoding(backup_path)
        target_text, target_encoding = _read_text_with_encoding(target_path)
        line_sep = "\r\n" if "\r\n" in target_text else ("\r\n" if "\r\n" in backup_text else "\n")

        _, backup_sections = _split_sections(backup_text)
        target_preamble, target_sections = _split_sections(target_text)

        backup_map = {
            str(name or "").strip().lower(): block
            for name, block in backup_sections
            if str(name or "").strip()
        }
        wanted_keys = [
            str(section or "").strip().lower()
            for section in sections_to_merge
            if str(section or "").strip()
        ]
        wanted_set = set(wanted_keys)
        if not wanted_set:
            return

        merged_sections = []
        replaced_keys = set()
        for name, block in target_sections:
            key = str(name or "").strip().lower()
            if key in wanted_set and key in backup_map:
                merged_sections.append((name, backup_map[key]))
                replaced_keys.add(key)
            else:
                merged_sections.append((name, block))

        for key in wanted_keys:
            if key in replaced_keys:
                continue
            block = backup_map.get(key, "")
            if not block:
                continue
            merged_sections.append((key, block))
            replaced_keys.add(key)

        new_text = str(target_preamble or "")
        for _, block in merged_sections:
            chunk = str(block or "")
            if chunk and not (chunk.endswith("\n") or chunk.endswith("\r")):
                chunk += line_sep
            new_text += chunk

        target_path.parent.mkdir(parents=True, exist_ok=True)
        with open(target_path, "w", encoding=target_encoding, newline="") as f:
            f.write(new_text)

    def backup(self):
        if not self.sandbox_users_root.exists():
            wx.MessageBox("未找到任何沙盒用户，请先创建沙盒。", "错误", wx.OK | wx.ICON_ERROR)
            return
        users = [d for d in os.listdir(self.sandbox_users_root) if os.path.isdir(self.sandbox_users_root / d)]
        if not users:
            wx.MessageBox("未找到任何沙盒用户。", "错误", wx.OK | wx.ICON_ERROR)
            return

        # 使用自定义复选框列表对话框
        dlg = SelectUsersDialog(self.frame, "批量备份", users)
        if dlg.ShowModal() != wx.ID_OK:
            dlg.Destroy()
            return
        selected_users = dlg.get_selected_users()
        dlg.Destroy()
        if not selected_users:
            return

        save_dlg = wx.DirDialog(self.frame, "选择备份保存目录", style=wx.DD_DEFAULT_STYLE)
        if save_dlg.ShowModal() != wx.ID_OK:
            save_dlg.Destroy()
            return
        save_dir = Path(save_dlg.GetPath())
        save_dlg.Destroy()

        timestamp = datetime.datetime.now().strftime("%Y%m%d_%H%M%S")
        final_path = save_dir / f"SandboxBackup_{timestamp}.sbx"
        temp_zip = save_dir / f"temp_{timestamp}.zip"

        self._safe_frame_call_after(self.frame.lbl_status.SetLabel, "正在备份...")

        def backup_thread():
            try:
                with zipfile.ZipFile(temp_zip, 'w', zipfile.ZIP_DEFLATED) as zf:
                    if self.sandboxie_ini_path.exists():
                        zf.write(self.sandboxie_ini_path, arcname="Sandboxie.ini")
                    for user in selected_users:
                        user_path = self.sandbox_users_root / user
                        if user_path.exists() and user_path.is_dir():
                            for current_root, dir_names, file_names in os.walk(user_path):
                                current_dir = Path(current_root)
                                relative_dir = current_dir.relative_to(user_path)
                                for file_name in file_names:
                                    file_path = current_dir / file_name
                                    arcname = Path("SandboxUsers") / user / relative_dir / file_name
                                    zf.write(file_path, arcname.as_posix())
                                if not dir_names and not file_names:
                                    empty_dir = Path("SandboxUsers") / user / relative_dir
                                    empty_dir_entry = empty_dir.as_posix().rstrip("/") + "/"
                                    zf.writestr(empty_dir_entry, "")
                temp_zip.rename(final_path)
                self._safe_frame_call_after(self.frame.lbl_status.SetLabel, "备份完成")
                self._safe_frame_call_after(
                    wx.MessageBox,
                    f"备份成功！\n备份文件保存在：\n{final_path}\n已备份用户：{', '.join(selected_users)}",
                    "备份完成",
                    wx.OK | wx.ICON_INFORMATION
                )
            except Exception as e:
                self._safe_frame_call_after(self.frame.lbl_status.SetLabel, "备份失败")
                self._safe_frame_call_after(wx.MessageBox, f"备份失败：{str(e)}", "错误", wx.OK | wx.ICON_ERROR)
                if temp_zip.exists():
                    temp_zip.unlink()
            finally:
                if temp_zip.exists():
                    temp_zip.unlink()

        threading.Thread(target=backup_thread, daemon=True).start()

    def restore(self):
        with wx.FileDialog(self.frame, "选择备份文件", "", "", "备份文件 (*.sbx)|*.sbx", wx.FD_OPEN | wx.FD_FILE_MUST_EXIST) as dlg:
            if dlg.ShowModal() != wx.ID_OK:
                return
            sbx_path = Path(dlg.GetPath())
        if str(sbx_path.suffix or "").lower() != ".sbx":
            wx.MessageBox("请选择 .sbx 格式的备份文件。", "错误", wx.OK | wx.ICON_ERROR)
            return

        temp_dir = Path(get_update_temp_dir(get_app_dir())) / "restore_temp"
        if temp_dir.exists():
            self._force_remove_dir(temp_dir)
        temp_dir.mkdir(parents=True, exist_ok=True)

        try:
            safe_extract_zip(sbx_path, temp_dir)
        except Exception as e:
            wx.MessageBox(f"无法读取备份文件：{str(e)}", "错误", wx.OK | wx.ICON_ERROR)
            if temp_dir.exists():
                shutil.rmtree(temp_dir, ignore_errors=True)
            return

        backup_users_root = None
        available_users = []
        for candidate_root in self._iter_backup_user_roots(temp_dir):
            names = [d.name for d in candidate_root.iterdir() if d.is_dir()]
            if names:
                backup_users_root = candidate_root
                available_users = names
                break
        if not available_users:
            wx.MessageBox("备份文件中没有找到任何沙盒用户数据。", "错误", wx.OK | wx.ICON_ERROR)
            shutil.rmtree(temp_dir, ignore_errors=True)
            return

        dlg = SelectUsersDialog(self.frame, "批量还原", available_users)
        if dlg.ShowModal() != wx.ID_OK:
            dlg.Destroy()
            shutil.rmtree(temp_dir, ignore_errors=True)
            return
        selected_users = dlg.get_selected_users()
        dlg.Destroy()
        if not selected_users:
            shutil.rmtree(temp_dir, ignore_errors=True)
            return

        if wx.MessageBox(f"还原将覆盖以下沙盒用户的数据：{', '.join(selected_users)}\n是否继续？", "确认还原", wx.YES_NO | wx.ICON_WARNING) != wx.YES:
            shutil.rmtree(temp_dir, ignore_errors=True)
            return

        progress = wx.ProgressDialog("还原沙盒数据", "准备...", maximum=100, parent=self.frame, style=wx.PD_APP_MODAL)
        progress.Update(0, "停止 Sandboxie 服务...")

        def restore_thread():
            try:
                self._stop_sandboxie_services()
                self._safe_frame_call_after(progress.Update, 10, "清理旧数据...")
                if not self.sandbox_users_root.exists():
                    self.sandbox_users_root.mkdir(parents=True)

                self._safe_frame_call_after(progress.Update, 30, "还原用户数据...")
                for user in selected_users:
                    src_user = backup_users_root / user
                    dst_user = self.sandbox_users_root / user
                    if dst_user.exists():
                        self._force_remove_dir(dst_user)
                    shutil.copytree(src_user, dst_user)

                self._safe_frame_call_after(progress.Update, 70, "合并配置文件...")
                backup_ini = temp_dir / "Sandboxie.ini"
                if backup_ini.exists():
                    self._merge_ini_sections(backup_ini, self.sandboxie_ini_path, selected_users)

                self._safe_frame_call_after(progress.Update, 90, "重载配置...")
                self._reload_sandboxie_config()
                time.sleep(0.5)

                self._safe_frame_call_after(progress.Update, 100, "完成")
                time.sleep(0.3)
                self._safe_frame_call_after(progress.Destroy)

                if hasattr(self.frame, "_suppress_list_refresh_status"):
                    self._safe_frame_call_after(self.frame._suppress_list_refresh_status, 2.0)
                self._safe_frame_call_after(self.frame.sandbox_list.on_refresh_sandbox_list, None)
                self._safe_frame_call_after(self.frame.lbl_status.SetLabel, "还原完成")

                control = self._find_sandboxie_control_exe()
                if control:
                    self._safe_frame_call_after(self._ask_start_control, control)
                else:
                    self._safe_frame_call_after(
                        wx.MessageBox,
                        "还原成功！\n未找到 Sandboxie Control，请手动启动。",
                        "还原完成",
                        wx.OK | wx.ICON_INFORMATION
                    )
                self._safe_frame_call_after(self._ask_restart_app)

            except Exception as e:
                self._safe_frame_call_after(progress.Destroy)
                self._safe_frame_call_after(wx.MessageBox, f"还原失败：{str(e)}", "错误", wx.OK | wx.ICON_ERROR)
            finally:
                if temp_dir.exists():
                    shutil.rmtree(temp_dir, ignore_errors=True)

        threading.Thread(target=restore_thread, daemon=True).start()

    def _ask_start_control(self, control_exe):
        if wx.MessageBox("还原成功！\n是否立即启动 Sandboxie Control？", "还原完成", wx.YES_NO | wx.ICON_QUESTION) == wx.YES:
            try:
                subprocess.Popen([control_exe])
            except Exception as e:
                wx.MessageBox(f"启动失败：{str(e)}", "提示", wx.OK | wx.ICON_INFORMATION)

    def _ask_restart_app(self):
        if wx.MessageBox("还原已完成。\n是否重启本程序？", "重启提示", wx.YES_NO | wx.ICON_QUESTION) == wx.YES:
            try:
                subprocess.Popen([sys.executable, get_current_executable_path()] if not is_packaged_app() else [get_current_executable_path()])
            except:
                pass
            self.frame.Destroy()
            os._exit(0)
