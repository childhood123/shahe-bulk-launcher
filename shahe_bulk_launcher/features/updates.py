import json
import os
import shutil
import subprocess
import threading
import time
import urllib.request
from pathlib import Path

import wx

from ..bootstrap import (
    get_app_dir,
    get_current_executable_path,
    get_resource_path,
    is_packaged_app,
)
from ..common import (
    compare_versions,
    get_update_temp_dir,
    is_allowed_update_body_status,
    open_update_url,
    parse_bool_setting,
    parse_version_parts,
)
from ..constants import APP_VERSION
from ..ui.common_dialogs import QRCodeDialog

try:
    from qr_embedded import QR_EMBEDDED_IMAGES
except Exception:
    QR_EMBEDDED_IMAGES = {}


class UpdatesMixin:
    def _resolve_qr_image_path(self, filename):
        name = str(filename or "").strip()
        if not name:
            return ""
        candidates = [
            os.path.join(get_app_dir(), name),
            get_resource_path(name),
            get_resource_path(os.path.join("internal", name)),
            os.path.join(get_app_dir(), "internal", name),
        ]
        for path in candidates:
            try:
                if os.path.isfile(path):
                    return path
            except Exception:
                continue
        return ""

    def on_check_update(self, event):
        self._log_operation("手动检查更新")
        self._show_update_notice(False)
        self.lbl_status.SetLabel("正在检查更新...")
        threading.Thread(target=self._check_update_worker, args=(False, False), daemon=True).start()

    def _auto_check_update_after_startup(self):
        self._log_operation("启动后自动检查更新")
        threading.Thread(target=self._check_update_worker, args=(True, False), daemon=True).start()

    def _check_update_worker(self, silent_when_latest=False, inline_when_update=False):
        try:
            manifest = self._fetch_update_manifest()
        except Exception as e:
            self._log_operation(f"检查更新失败: {e}")
            if not silent_when_latest:
                self._safe_call_after(self._show_update_error, str(e))
            return
        self._log_operation("检查更新成功")
        self._safe_call_after(self._show_update_result, manifest, silent_when_latest, inline_when_update)

    def _fetch_update_manifest(self):
        manifests = []
        errors = []
        urls = (self._get_update_manifest_url(),)
        for base_url in urls:
            try:
                url = f"{base_url}?t={int(time.time())}"
                req = urllib.request.Request(
                    url,
                    headers={
                        "Cache-Control": "no-cache, no-store, max-age=0",
                        "Pragma": "no-cache",
                        "User-Agent": f"SandboxBatchLauncher/{APP_VERSION}",
                        "Accept": "application/json",
                    }
                )
                with open_update_url(req, timeout=8, allow_error_body=True) as response:
                    status = getattr(response, "status", 200)
                    if status is None:
                        status = 200
                    status = int(status)
                    if status >= 400 and not is_allowed_update_body_status(status, url):
                        raise RuntimeError(f"更新服务器返回 HTTP {status}")
                    data = response.read(1024 * 1024)
                text = data.decode("utf-8-sig")
                try:
                    manifest = json.loads(text)
                except json.JSONDecodeError:
                    manifest = json.loads(text, strict=False)
                if not isinstance(manifest, dict):
                    raise RuntimeError("更新清单格式不正确")
                manifest["_manifest_url"] = base_url
                manifests.append(manifest)
            except Exception as e:
                errors.append(f"{base_url}: {e}")

        if not manifests:
            raise RuntimeError("; ".join(errors) or "无法获取更新清单")

        def manifest_version(item):
            return parse_version_parts(str(item.get("latest_version") or item.get("version") or "0"))

        manifests.sort(key=manifest_version, reverse=True)
        return manifests[0]

    def _format_update_notes(self, release_notes):
        if isinstance(release_notes, list):
            lines = [str(item or "").strip() for item in release_notes if str(item or "").strip()]
            return "\n".join(f"- {line}" for line in lines[:8])
        return str(release_notes or "").strip()

    def _format_update_package_size(self, size_value):
        try:
            size = int(size_value or 0)
        except Exception:
            size = 0
        if size <= 0:
            return ""
        units = ["B", "KB", "MB", "GB"]
        value = float(size)
        for unit in units:
            if value < 1024 or unit == units[-1]:
                if unit == "B":
                    return f"{int(value)} {unit}"
                return f"{value:.1f} {unit}"
            value /= 1024.0
        return f"{size} B"

    def _normalize_update_package(self, package, kind, latest_version):
        if not isinstance(package, dict):
            return None
        url = str(package.get("url") or "").strip()
        if not url:
            return None
        size = package.get("size") or package.get("bytes") or 0
        try:
            size = max(0, int(size or 0))
        except Exception:
            size = 0
        return {
            "kind": kind,
            "kind_label": "增量补丁",
            "url": url,
            "sha256": str(package.get("sha256") or package.get("hash") or "").strip(),
            "signature": str(package.get("signature") or package.get("sig") or "").strip(),
            "encrypted": parse_bool_setting(package.get("encrypted", package.get("is_encrypted", False)), False),
            "size": size,
            "version": str(package.get("to") or package.get("to_version") or latest_version or "").strip(),
        }

    def _select_update_package(self, manifest, latest_version):
        patches = manifest.get("patches", [])
        if isinstance(patches, list):
            for patch in patches:
                if not isinstance(patch, dict):
                    continue
                from_version = str(patch.get("from") or patch.get("from_version") or "").strip()
                to_version = str(patch.get("to") or patch.get("to_version") or latest_version or "").strip()
                if from_version != APP_VERSION:
                    continue
                if to_version and compare_versions(to_version, APP_VERSION) <= 0:
                    continue
                selected = self._normalize_update_package(patch, "patch", latest_version)
                if selected:
                    return selected
        return None

    def _resolve_updater_executable(self):
        candidate = os.path.join(get_app_dir(), "updater.exe")
        try:
            if candidate and os.path.isfile(candidate):
                return candidate
        except Exception:
            pass
        return ""

    def _prepare_updater_command(self, package_info, latest_version):
        if not is_packaged_app():
            return None, "自动更新只能在打包后的发布版中使用。"
        app_dir = get_app_dir()
        updater_path = self._resolve_updater_executable()
        if not updater_path:
            return None, "未找到当前主程序，自动更新无法继续。"
        try:
            temp_dir = Path(get_update_temp_dir(app_dir))
            temp_dir.mkdir(parents=True, exist_ok=True)
            helper_dir = temp_dir / f"updater_helper_{os.getpid()}_{int(time.time())}"
            helper_dir.mkdir(parents=True, exist_ok=True)
            temp_updater = helper_dir / "updater.exe"
            shutil.copy2(updater_path, temp_updater)
            internal_source = Path(app_dir) / "internal"
            internal_target = helper_dir / "internal"
            if internal_source.is_dir():
                shutil.copytree(internal_source, internal_target, dirs_exist_ok=True)
            updater_to_run = str(temp_updater)
        except Exception as exc:
            return None, f"创建临时更新程序失败: {exc}"

        request_file = ""
        try:
            request_dir = Path(get_update_temp_dir(app_dir))
            request_dir.mkdir(parents=True, exist_ok=True)
            request_file = str(request_dir / f"update_request_{os.getpid()}_{int(time.time())}.json")
            request_data = {
                "url": str(package_info.get("url") or ""),
                "package_kind": str(package_info.get("kind") or ""),
                "current_version": APP_VERSION,
                "target_version": str(latest_version or ""),
                "sha256": str(package_info.get("sha256") or "").strip(),
                "signature": str(package_info.get("signature") or "").strip(),
                "encrypted": parse_bool_setting(package_info.get("encrypted", False), False),
                "size": str(package_info.get("size") or "0"),
                "restart": True,
                "download_workers": "4",
            }
            Path(request_file).write_text(json.dumps(request_data, ensure_ascii=False), encoding="utf-8")
        except Exception as exc:
            return None, f"\u521b\u5efa\u66f4\u65b0\u8bf7\u6c42\u6587\u4ef6\u5931\u8d25: {exc}"

        cmd = [
            updater_to_run,
            "--app-dir", app_dir,
            "--exe", get_current_executable_path(),
            "--pid", str(os.getpid()),
            "--request-file", request_file,
        ]
        return cmd, ""

    def _launch_updater(self, package_info, latest_version):
        self._log_operation(f"准备启动更新程序: {latest_version}")
        self.lbl_status.SetLabel("正在准备更新程序...")
        cmd, error = self._prepare_updater_command(package_info, latest_version)
        if not cmd:
            self._log_operation(f"自动更新不可用: {error}")
            self.lbl_status.SetLabel("自动更新不可用，请稍后重试")
            wx.MessageBox(
                f"{error}\n\n当前版本只支持通过内置更新程序安装增量补丁，请稍后重试。",
                "自动更新不可用",
                wx.OK | wx.ICON_INFORMATION,
                self
            )
            return False
        try:
            helper_dir = Path(cmd[0]).resolve().parent
            subprocess.Popen(cmd, cwd=str(helper_dir), close_fds=True)
            self._log_operation(f"更新程序已启动: {latest_version}")
            self.lbl_status.SetLabel("更新程序已启动，正在关闭主程序...")
            self.force_exit_requested = True
            self.Close()
            return True
        except Exception as e:
            self._log_operation(f"自动更新启动失败: {e}")
            self.lbl_status.SetLabel("自动更新启动失败，请稍后重试")
            wx.MessageBox(
                f"启动更新程序失败：\n{str(e)}\n\n当前版本只支持通过内置更新程序安装增量补丁，请稍后重试。",
                "自动更新失败",
                wx.OK | wx.ICON_WARNING,
                self
            )
            return False

    def _exit_without_update(self, latest_version=""):
        version_text = str(latest_version or "").strip()
        if version_text:
            self.lbl_status.SetLabel(f"发现新版本 {version_text}，已退出更新")
        else:
            self.lbl_status.SetLabel("已退出更新")
        self.force_exit_requested = True
        self._mark_window_closing()
        try:
            self.Close()
        except Exception:
            try:
                self.Destroy()
            except Exception:
                pass

    def _show_update_notice(self, show=True, latest_version="", package_info=None):
        self._pending_update_latest_version = str(latest_version or "").strip() if show else ""
        self._pending_update_package_info = package_info if show and isinstance(package_info, dict) else None
        panel = getattr(self, "update_notice_panel", None)
        label = getattr(self, "lbl_update_notice", None)
        button = getattr(self, "btn_download_update", None)
        if not panel or not label or not button:
            return
        if show and self._pending_update_latest_version and self._pending_update_package_info:
            label.SetLabel(f"检测到新版本：{self._pending_update_latest_version}")
            button.Enable(True)
            panel.Show()
        else:
            label.SetLabel("")
            button.Enable(False)
            panel.Hide()
        try:
            panel.GetParent().Layout()
        except Exception:
            pass
        try:
            self.Layout()
        except Exception:
            pass

    def on_download_update(self, event):
        package_info = getattr(self, "_pending_update_package_info", None)
        latest_version = str(getattr(self, "_pending_update_latest_version", "") or "").strip()
        if not isinstance(package_info, dict) or not latest_version:
            self.lbl_status.SetLabel("当前没有可下载的新版本")
            self._show_update_notice(False)
            return
        self._launch_updater(package_info, latest_version)

    def _show_update_error(self, message):
        detail = str(message or "").strip() or "未知错误"
        self._show_update_notice(False)
        self.lbl_status.SetLabel(f"检查更新失败: {detail}")
        wx.MessageBox(f"检查更新失败：\n{detail}", "检查更新", wx.OK | wx.ICON_WARNING, self)

    def _show_update_result(self, manifest, silent_when_latest=False, inline_when_update=False):
        latest_version = str(
            manifest.get("latest_version")
            or manifest.get("version")
            or ""
        ).strip()
        if not latest_version:
            if not silent_when_latest:
                self._show_update_error("更新清单缺少 latest_version")
            return

        force_update = parse_bool_setting(manifest.get("force_update", False), False)
        has_update = compare_versions(latest_version, APP_VERSION) > 0

        if not has_update:
            self._show_update_notice(False)
            if silent_when_latest:
                return
            self.lbl_status.SetLabel(f"沙河批量打开器 已是最新版本: {APP_VERSION}")
            wx.MessageBox(
                f"沙河批量打开器 已是最新版本。\n\n当前版本：{APP_VERSION}",
                "检查更新",
                wx.OK | wx.ICON_INFORMATION,
                self
            )
            return

        package_info = self._select_update_package(manifest, latest_version)
        if not package_info:
            self._show_update_notice(False)
            self.lbl_status.SetLabel(f"发现新版本 {latest_version}，但没有适用于当前版本的增量补丁")
            wx.MessageBox(f"发现新版本：{latest_version}\n但没有适用于当前版本的增量补丁。", "检查更新", wx.OK | wx.ICON_WARNING, self)
            return

        self._show_update_notice(False)
        prompt = f"发现新版本{latest_version}"
        style = wx.YES_NO | wx.ICON_WARNING if force_update else wx.YES_NO | wx.ICON_INFORMATION
        if wx.MessageBox(prompt, "提示", style, self) == wx.YES:
            self._launch_updater(package_info, latest_version)
        else:
            if force_update:
                self._exit_without_update(latest_version)
                return
            self._show_update_notice(True, latest_version, package_info)
            self.lbl_status.SetLabel(f"发现新版本 {latest_version}，可稍后点击“下载更新”")

    def _get_embedded_qr_bytes(self, filename):
        name = str(filename or "").strip()
        if not name:
            return b""
        data = QR_EMBEDDED_IMAGES.get(name)
        if isinstance(data, (bytes, bytearray)) and data:
            return bytes(data)
        return b""

    def on_show_wechat(self, event):
        img_name = "微信赞赏码.jpg"
        img_path = self._resolve_qr_image_path(img_name)
        embedded = b"" if img_path else self._get_embedded_qr_bytes(img_name)
        if (not embedded) and (not img_path):
            wx.MessageBox("未找到可用的微信二维码资源", "错误", wx.OK | wx.ICON_ERROR, self)
            return
        dlg = QRCodeDialog(
            self,
            "微信打赏",
            img_path=img_path if not embedded else "",
            embedded_bytes=embedded,
            copy_img_path=img_path
        )
        dlg.ShowModal()
        dlg.Destroy()

    def on_show_alipay(self, event):
        img_name = "支付宝赞赏码.jpg"
        img_path = self._resolve_qr_image_path(img_name)
        embedded = b"" if img_path else self._get_embedded_qr_bytes(img_name)
        if (not embedded) and (not img_path):
            wx.MessageBox("未找到可用的支付宝二维码资源", "错误", wx.OK | wx.ICON_ERROR, self)
            return
        dlg = QRCodeDialog(
            self,
            "支付宝打赏",
            img_path=img_path if not embedded else "",
            embedded_bytes=embedded,
            copy_img_path=img_path
        )
        dlg.ShowModal()
        dlg.Destroy()
