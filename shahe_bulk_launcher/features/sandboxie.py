import ctypes
import glob
import os
import re
import subprocess
import tempfile
import time
import winreg
from ctypes import wintypes

import wx

from ..common import get_hidden_subprocess_kwargs
from ..constants import (
    get_internal_sandbox_name_validation_error,
    get_sandbox_display_name_validation_error,
    is_valid_internal_sandbox_name,
)


class SandboxieMixin:
    def _is_valid_sandboxie_install_path(self, path):
        value = str(path or "").strip()
        if not value:
            return False
        return os.path.isfile(os.path.join(value, "Start.exe"))

    def first_time_scan_sandboxie(self):
        if not self._safe_set_status_label("正在扫描 C/D 盘的 Sandboxie 安装路径..."):
            return
        try:
            wx.Yield()
        except Exception:
            pass
        if self._is_window_closing():
            return
        sandboxie_path = self.find_sandboxie_path()
        if sandboxie_path:
            self.SANDBOXIE_PATH = sandboxie_path
            self.save_settings()
            message = f"已扫描到 Sandboxie 安装位置：\n{sandboxie_path}"
            if not self._safe_set_status_label(message):
                return
            self._speak_process_assist(message, event_key="launch_done")
            if self._is_window_closing():
                return
            try:
                wx.MessageBox(message, "扫描成功", wx.OK | wx.ICON_INFORMATION, self)
            except RuntimeError:
                pass
        else:
            message = "未找到 Sandboxie 安装路径，请在“校正”菜单中手动校正 Sandboxie 安装路径。"
            self._safe_set_status_label(message)
            if self._is_window_closing():
                return
            try:
                wx.MessageBox(message, "扫描结果", wx.OK | wx.ICON_INFORMATION, self)
            except RuntimeError:
                pass

    def _get_sandboxie_start_exe(self):
        sandboxie_path = getattr(self, "SANDBOXIE_PATH", "") or self.find_sandboxie_path() or ""
        if not sandboxie_path:
            return ""
        start_exe = os.path.join(sandboxie_path, "Start.exe")
        return start_exe if os.path.exists(start_exe) else ""

    def _get_sbieini_exe(self):
        sandboxie_path = getattr(self, "SANDBOXIE_PATH", "") or self.find_sandboxie_path() or ""
        if not sandboxie_path:
            return ""
        for exe_name in ("SbieIni.exe", "sbieini.exe"):
            exe_path = os.path.join(sandboxie_path, exe_name)
            if os.path.exists(exe_path):
                return exe_path
        return ""

    def _get_sbie_dll_path(self):
        sandboxie_path = getattr(self, "SANDBOXIE_PATH", "") or self.find_sandboxie_path() or ""
        candidates = []
        if sandboxie_path:
            candidates.extend([
                os.path.join(sandboxie_path, "SbieDll.dll"),
                os.path.join(sandboxie_path, "SbieDll64.dll"),
            ])
        for candidate in candidates:
            if os.path.exists(candidate):
                return candidate
        return ""

    def _get_sbie_api(self):
        dll_path = self._get_sbie_dll_path()
        if not dll_path:
            return None
        cached = getattr(self, "_sbie_api_cache", None)
        if cached and cached.get("path") == dll_path:
            return cached
        try:
            dll = ctypes.WinDLL(dll_path)
            enum_process = dll.SbieApi_EnumProcessEx
            enum_process.argtypes = [
                wintypes.LPCWSTR,
                wintypes.BOOLEAN,
                wintypes.ULONG,
                ctypes.POINTER(wintypes.ULONG),
                ctypes.POINTER(wintypes.ULONG),
            ]
            enum_process.restype = wintypes.LONG

            query_process = dll.SbieApi_QueryProcess
            query_process.argtypes = [
                wintypes.HANDLE,
                wintypes.LPWSTR,
                wintypes.LPWSTR,
                wintypes.LPWSTR,
                ctypes.POINTER(wintypes.ULONG),
            ]
            query_process.restype = wintypes.LONG

            cached = {
                "path": dll_path,
                "dll": dll,
                "enum_process": enum_process,
                "query_process": query_process,
            }
            self._sbie_api_cache = cached
            return cached
        except Exception:
            return None

    def _run_sbieini(self, args, timeout=10, cwd=None):
        sbieini_exe = self._get_sbieini_exe()
        if not sbieini_exe:
            return None, ""
        cmd = [sbieini_exe]
        for arg in list(args or []):
            value = str(arg or "").strip()
            if value:
                cmd.append(value)
        try:
            result = subprocess.run(
                cmd,
                capture_output=True,
                text=True,
                encoding="utf-8",
                errors="ignore",
                timeout=max(1.0, float(timeout)),
                cwd=cwd or None,
                **get_hidden_subprocess_kwargs()
            )
            output = "\n".join(part for part in [result.stdout, result.stderr] if part)
            return result, output
        except Exception:
            return None, ""

    def _list_enabled_sandboxes_from_sbieini(self):
        result, output = self._run_sbieini(["queryex", "/boxes", "*"], timeout=8)
        if result is None or result.returncode != 0:
            return []
        names = []
        seen = set()
        invalid_chars_pattern = re.compile(r"[\\/:*?\"<>|\[\]]")
        for raw_line in str(output or "").splitlines():
            name = str(raw_line or "").strip().strip("\"")
            if not name:
                continue
            if name.startswith("[") and name.endswith("]"):
                name = name[1:-1].strip()
            if not name:
                continue
            if self._is_reserved_sandboxie_ini_section_name(name):
                continue
            if invalid_chars_pattern.search(name):
                continue
            key = name.lower()
            if key in seen:
                continue
            seen.add(key)
            names.append(name)
        return names

    def _get_official_sandbox_name_set(self):
        return {
            str(name or "").strip().lower()
            for name in self._list_enabled_sandboxes_from_sbieini()
            if str(name or "").strip()
        }

    def _sandbox_exists_in_official_config(self, sandbox_name):
        name = str(sandbox_name or "").strip()
        if not name:
            return False
        names = self._get_official_sandbox_name_set()
        return bool(names) and name.lower() in names

    def _get_ini_setting_value_from_block(self, block, setting_name):
        target = str(setting_name or "").strip().lower()
        if not target:
            return ""
        for raw_line in str(block or "").splitlines():
            line = str(raw_line or "").strip()
            if not line or line.startswith("#") or "=" not in line:
                continue
            key, value = line.split("=", 1)
            if key.strip().lower() == target:
                return value.strip()
        return ""

    def _is_reserved_sandboxie_ini_section_name(self, section_name):
        name = str(section_name or "").strip().lower()
        return name == "globalsettings" or name.startswith("usersettings_")

    def _is_editable_sandbox_ini_section_name(self, section_name):
        name = str(section_name or "").strip()
        return bool(name) and not self._is_reserved_sandboxie_ini_section_name(name) and is_valid_internal_sandbox_name(name)

    def _is_sandbox_config_section(self, section_name, block, enabled_names_lower=None):
        name = str(section_name or "").strip()
        if not name:
            return False
        if not self._is_editable_sandbox_ini_section_name(name):
            return False
        lower_name = name.lower()
        if enabled_names_lower and lower_name in enabled_names_lower:
            return True
        enabled = self._get_ini_setting_value_from_block(block, "Enabled").lower()
        return enabled in ("y", "yes", "true", "1")

    def _get_box_alias_map_from_ini(self):
        ini_path = self._get_sandboxie_ini_path()
        text, _ = self._read_text_with_encoding(ini_path)
        _, sections = self._split_ini_sections(text)
        aliases = {}
        for section_name, block in sections:
            if not self._is_sandbox_config_section(section_name, block):
                continue
            alias = self._get_ini_setting_value_from_block(block, "BoxAlias")
            alias_disabled = self._get_ini_setting_value_from_block(block, "BoxAliasDisabled")
            effective_alias = alias or alias_disabled
            if effective_alias:
                aliases[str(section_name or "").strip().lower()] = effective_alias.strip()
        return aliases

    def _display_name_for_sandbox(self, internal_name, alias_map=None):
        name = str(internal_name or "").strip()
        if not name:
            return ""
        alias = (alias_map or {}).get(name.lower(), "")
        if alias:
            return alias
        return name.replace("_", " ")

    def _refresh_sandbox_display_maps(self):
        internal_names = []
        seen_internal = set()

        def add_internal(name):
            value = str(name or "").strip()
            if not value:
                return
            key = value.lower()
            if key in seen_internal:
                return
            seen_internal.add(key)
            internal_names.append(value)

        official_internal_names = []
        try:
            enabled_names = self._list_enabled_sandboxes_from_sbieini()
        except Exception:
            enabled_names = []
        enabled_names_lower = {
            str(name or "").strip().lower()
            for name in enabled_names
            if str(name or "").strip()
        }

        try:
            ini_path = self._get_sandboxie_ini_path()
            text, _ = self._read_text_with_encoding(ini_path)
            _, sections = self._split_ini_sections(text)
            for section_name, block in sections:
                if self._is_sandbox_config_section(section_name, block, enabled_names_lower):
                    value = str(section_name or "").strip()
                    if value:
                        official_internal_names.append(value)
        except Exception:
            pass

        if official_internal_names:
            for name in official_internal_names:
                add_internal(name)
        elif enabled_names:
            try:
                for name in enabled_names:
                    add_internal(name)
            except Exception:
                pass
        else:
            try:
                if os.path.isdir(self.sandbox_root):
                    with os.scandir(self.sandbox_root) as entries:
                        for entry in entries:
                            try:
                                if entry.is_dir() and is_valid_internal_sandbox_name(entry.name):
                                    add_internal(entry.name)
                            except Exception:
                                pass
            except Exception:
                pass

        try:
            alias_map = self._get_box_alias_map_from_ini()
        except Exception:
            alias_map = {}

        display_to_name = {}
        name_to_display = {}
        used_display = set()
        for internal_name in internal_names:
            display_name = self._display_name_for_sandbox(internal_name, alias_map)
            display_key = display_name.lower()
            if display_key in used_display:
                display_name = f"{display_name} [{internal_name}]"
                display_key = display_name.lower()
            used_display.add(display_key)
            display_to_name[display_key] = internal_name
            display_to_name[internal_name.lower()] = internal_name
            name_to_display[internal_name.lower()] = display_name

        self._sandbox_display_to_name = display_to_name
        self._sandbox_name_to_display = name_to_display
        return internal_names, name_to_display

    def _scan_sandbox_display_names(self):
        _, name_to_display = self._refresh_sandbox_display_maps()
        displays = [value for value in name_to_display.values() if str(value or "").strip()]
        return sorted(displays, key=lambda value: value.lower())

    def _scan_sandbox_names_for_worker(self):
        try:
            return self._scan_sandbox_display_names()
        except Exception:
            return []

    def _resolve_sandbox_internal_name(self, sandbox_name):
        text = str(sandbox_name or "").strip()
        if not text:
            return ""
        if not getattr(self, "_sandbox_display_to_name", None):
            try:
                self._refresh_sandbox_display_maps()
            except Exception:
                pass
        mapped = self._sandbox_display_to_name.get(text.lower()) if getattr(self, "_sandbox_display_to_name", None) else None
        return mapped or text

    def _get_sandbox_display_name(self, sandbox_name):
        internal_name = self._resolve_sandbox_internal_name(sandbox_name)
        if not internal_name:
            return ""
        if not getattr(self, "_sandbox_name_to_display", None):
            try:
                self._refresh_sandbox_display_maps()
            except Exception:
                pass
        return self._sandbox_name_to_display.get(internal_name.lower(), internal_name.replace("_", " "))

    def _get_sandbox_internal_and_display_name(self, sandbox_name):
        display_name = str(sandbox_name or "").strip()
        internal_name = self._normalize_sandbox_name(sandbox_name)
        if internal_name:
            try:
                resolved_display = self._get_sandbox_display_name(internal_name)
            except Exception:
                resolved_display = ""
            if resolved_display:
                display_name = resolved_display
            elif not display_name:
                display_name = internal_name
        return internal_name, (display_name or internal_name)

    def _get_volume_memory_key(self, sandbox_name):
        _, display_name = self._get_sandbox_internal_and_display_name(sandbox_name)
        key = str(display_name or "").strip()
        if key:
            return key
        return str(sandbox_name or "").strip()

    def _get_existing_sandbox_names(self):
        names = set()
        try:
            if hasattr(self, "sandbox_list") and self.sandbox_list is not None:
                self._refresh_sandbox_display_maps()
                names.update(self._sandbox_name_to_display.keys())
                names.update(self._sandbox_display_to_name.values())
        except Exception:
            pass
        try:
            names.update(self._list_enabled_sandboxes_from_sbieini())
        except Exception:
            pass
        return {str(name or "").strip() for name in names if str(name or "").strip()}

    def _get_existing_sandbox_display_names(self):
        try:
            self._refresh_sandbox_display_maps()
        except Exception:
            pass
        displays = set()
        try:
            displays.update(self._sandbox_name_to_display.values())
        except Exception:
            pass
        try:
            if hasattr(self, "sandbox_list") and self.sandbox_list is not None:
                displays.update(self.sandbox_list.scan_sandboxes())
        except Exception:
            pass
        return {str(name or "").strip() for name in displays if str(name or "").strip()}

    def _display_name_to_internal_candidate(self, display_name):
        return str(display_name or "").strip().replace(" ", "_")

    def _generate_available_internal_sandbox_name(self, display_name="", reserved_names=None):
        existing = {str(name or "").strip().lower() for name in self._get_existing_sandbox_names()}
        existing.update({str(name or "").strip().lower() for name in (reserved_names or []) if str(name or "").strip()})
        preferred = self._display_name_to_internal_candidate(display_name)
        if is_valid_internal_sandbox_name(preferred) and preferred.lower() not in existing:
            return preferred
        base = re.sub(r"[^A-Za-z0-9_]", "", preferred)
        base = re.sub(r"_+", "_", base).strip("_")
        base = re.sub(r"\d+$", "", base).strip("_")
        if not base:
            base = "Box"
        base = base[:24] or "Box"
        for index in range(1, 1000000):
            suffix = str(index)
            candidate = f"{base}{suffix}"
            if len(candidate) > 32:
                candidate = f"{base[:32 - len(suffix)]}{suffix}"
            if candidate.lower() not in existing and is_valid_internal_sandbox_name(candidate):
                return candidate
        return f"Box{int(time.time())}"

    def _set_box_alias_via_official(self, internal_name, alias):
        name = str(internal_name or "").strip()
        if not name:
            return False
        value = str(alias or "").strip()
        result, _ = self._run_sbieini(["set", "/drv", name, "BoxAlias", value], timeout=10)
        return result is not None and result.returncode == 0

    def _create_sandbox_via_official(self, sandbox_name):
        display_name = str(sandbox_name or "").strip()
        validation_error = get_sandbox_display_name_validation_error(display_name)
        if validation_error:
            return False, validation_error
        name = self._generate_available_internal_sandbox_name(display_name)
        internal_validation_error = get_internal_sandbox_name_validation_error(name)
        if internal_validation_error:
            return False, internal_validation_error
        alias = "" if self._display_name_for_sandbox(name, {}) == display_name else display_name

        result, output = self._run_sbieini(["set", "/drv", name, "Enabled", "y"], timeout=10)
        if result is not None and result.returncode == 0:
            self._run_sbieini(["set", "/drv", name, "ConfigLevel", "9"], timeout=10)
            if alias:
                self._set_box_alias_via_official(name, alias)
            try:
                self._reload_sandboxie_config()
            except Exception:
                pass
            try:
                os.makedirs(os.path.join(self.sandbox_root, name), exist_ok=True)
            except Exception:
                pass
            try:
                self._refresh_sandbox_display_maps()
            except Exception:
                pass
            return True, ""
        return False, str(output or "").strip() or "官方创建命令未成功"

    def _delete_sandbox_via_official(self, sandbox_name):
        name = self._normalize_sandbox_name(sandbox_name)
        if not name:
            return False, "沙盒名为空"
        validation_error = get_internal_sandbox_name_validation_error(name)
        if validation_error:
            return False, validation_error

        try:
            official_names = self._get_official_sandbox_name_set()
        except Exception:
            official_names = set()
        try:
            before_section_names_lower = {
                str(section_name or "").strip().lower()
                for section_name in self._list_ini_section_names(sandbox_only=False)
            }
        except Exception:
            before_section_names_lower = set()
        target_exists = False
        if official_names:
            target_exists = name.lower() in official_names
        if not target_exists and before_section_names_lower:
            target_exists = name.lower() in before_section_names_lower
        if (official_names or before_section_names_lower) and not target_exists:
            return False, "沙盒不存在"
        deleted_by_start = False
        if not official_names or name.lower() in official_names:
            delete_attempts = [
                [f"/box:{name}", "delete_sandbox_silent"],
                [f"/box:{name}", "delete_sandbox"],
            ]
            for args in delete_attempts:
                result, _ = self._run_sandboxie_start(args, timeout=18)
                if result is not None and result.returncode == 0:
                    deleted_by_start = True
                    break

        ini_removed = False
        result, _ = self._run_sbieini(["set", "/drv", name, "*", ""], timeout=10)
        if result is not None and result.returncode == 0:
            ini_removed = True

        try:
            self._reload_sandboxie_config()
        except Exception:
            pass

        try:
            section_names_lower = {
                str(section_name or "").strip().lower()
                for section_name in self._list_ini_section_names(sandbox_only=False)
            }
        except Exception:
            section_names_lower = {name.lower()}
        if name.lower() not in section_names_lower:
            return True, ""
        return False, "官方删除命令未成功"

    def _normalize_sandbox_name(self, sandbox_name):
        name = str(sandbox_name or "").strip()
        if not name:
            return ""
        if "\n" in name or "\r" in name:
            parts = [part.strip() for part in name.replace("\r", "\n").split("\n") if part.strip()]
            name = parts[0] if parts else ""
        if not name:
            return ""
        if hasattr(self, "sandbox_list"):
            try:
                name = self.sandbox_list.get_sandbox_name_from_item_text(name)
            except Exception:
                pass
        try:
            name = self._resolve_sandbox_internal_name(name)
        except Exception:
            pass
        return str(name or "").strip()

    def _run_sandboxie_start(self, args, timeout=10, cwd=None, hide_window=True, auto_silent=True):
        start_exe = self._get_sandboxie_start_exe()
        if not start_exe:
            return None, ""
        cmd = [start_exe]
        normalized_args = []
        for arg in list(args or []):
            value = str(arg or "").strip()
            if not value:
                continue
            if value in ("?", "/?", "-?", "--help"):
                continue
            normalized_args.append(value)
            cmd.append(value)
        if len(cmd) <= 1:
            return None, ""
        lower_args = {str(arg).strip().lower() for arg in normalized_args}
        query_or_control_args = {"/listpids", "/terminate", "/terminate_all", "/reload"}
        should_add_silent = bool(auto_silent) and not bool(lower_args & query_or_control_args)
        if should_add_silent and not any(str(arg).strip().lower() == "/silent" for arg in cmd[1:]):
            insert_at = 2 if len(cmd) > 1 and str(cmd[1]).strip().lower().startswith("/box:") else 1
            cmd.insert(insert_at, "/silent")
        run_kwargs = {}
        if hide_window:
            run_kwargs.update(get_hidden_subprocess_kwargs())
        try:
            result = subprocess.run(
                cmd,
                capture_output=True,
                text=True,
                encoding="utf-8",
                errors="ignore",
                timeout=max(1.0, float(timeout)),
                cwd=cwd or None,
                **run_kwargs
            )
            output = "\n".join(part for part in [result.stdout, result.stderr] if part)
            return result, output
        except Exception:
            return None, ""

    def _dispatch_sandboxie_start_detached(self, args, cwd=None, hide_window=False, auto_silent=False):
        start_exe = self._get_sandboxie_start_exe()
        if not start_exe:
            return False, ""
        cmd = [start_exe]
        normalized_args = []
        for arg in list(args or []):
            value = str(arg or "").strip()
            if not value:
                continue
            if value in ("?", "/?", "-?", "--help"):
                continue
            normalized_args.append(value)
            cmd.append(value)
        if len(cmd) <= 1:
            return False, ""
        lower_args = {str(arg).strip().lower() for arg in normalized_args}
        query_or_control_args = {"/listpids", "/terminate", "/terminate_all", "/reload"}
        should_add_silent = bool(auto_silent) and not bool(lower_args & query_or_control_args)
        if should_add_silent and not any(str(arg).strip().lower() == "/silent" for arg in cmd[1:]):
            insert_at = 2 if len(cmd) > 1 and str(cmd[1]).strip().lower().startswith("/box:") else 1
            cmd.insert(insert_at, "/silent")
        popen_kwargs = {}
        if hide_window:
            popen_kwargs.update(get_hidden_subprocess_kwargs())
        output_file = None
        try:
            output_file = tempfile.TemporaryFile(mode="w+t", encoding="utf-8", errors="ignore")
            process = subprocess.Popen(
                cmd,
                cwd=cwd or None,
                stdin=subprocess.DEVNULL,
                stdout=output_file,
                stderr=subprocess.STDOUT,
                close_fds=True,
                **popen_kwargs
            )
            poll = getattr(process, "poll", None)
            if callable(poll):
                time.sleep(0.08)
                return_code = poll()
                if return_code not in (None, 0):
                    try:
                        output_file.seek(0)
                        output = str(output_file.read() or "").strip()
                    except Exception:
                        output = ""
                    try:
                        output_file.close()
                    except Exception:
                        pass
                    if not output:
                        box_arg = next((arg for arg in normalized_args if str(arg).strip().lower().startswith("/box:")), "")
                        output = f"Sandboxie Start.exe 立即退出，退出码 {return_code}"
                        if box_arg:
                            output = f"{output}\n/box:{box_arg[5:]}"
                    return False, output
                try:
                    output_file.close()
                except Exception:
                    pass
            return True, ""
        except Exception as e:
            try:
                if output_file:
                    output_file.close()
            except Exception:
                pass
            return False, str(e)

    def _is_invalid_sandbox_name_output(self, output_text):
        text = str(output_text or "").lower()
        if not text:
            return False
        patterns = [
            "invalid sandbox name",
            "invalid box",
            "invalid sandbox",
            "无效的沙盒名参数",
            "无效沙盒名",
            "沙盒名参数无效",
            "参数: defaultbox",
        ]
        return any(pattern in text for pattern in patterns)

    def _parse_listpids_output(self, output):
        text = str(output or "")
        lines = [line.strip() for line in text.splitlines() if line.strip()]
        pure_numeric_values = []
        for line in lines:
            if re.fullmatch(r"\d+", line):
                pure_numeric_values.append(int(line))

        # Sandboxie normally prints one PID per line, sometimes preceded by a
        # count line.  Some localized/diagnostic builds can include labels such
        # as "PID: 123".  Parse every integer so those mixed lines are not
        # silently dropped, then remove a leading count when it matches the
        # number of remaining values.
        all_values = [int(m.group()) for m in re.finditer(r"\b\d+\b", text)]
        numeric_values = all_values or pure_numeric_values
        if not numeric_values:
            return []
        if len(numeric_values) >= 2 and numeric_values[0] == len(numeric_values) - 1:
            numeric_values = numeric_values[1:]

        result = []
        seen = set()
        for pid in numeric_values:
            if pid > 0 and pid not in seen:
                seen.add(pid)
                result.append(pid)
        return result

    def _get_sandbox_pids(self, sandbox_name, timeout=5):
        sandbox_name = self._normalize_sandbox_name(sandbox_name)
        if not sandbox_name:
            return []
        if not is_valid_internal_sandbox_name(sandbox_name):
            return []
        try:
            official_names = self._get_official_sandbox_name_set()
        except Exception:
            official_names = set()
        if official_names and sandbox_name.lower() not in official_names:
            return []

        # First try Sandboxie's API; on some VMs / Sandboxie builds the DLL can
        # load but enumeration returns an empty/incomplete map.  Do not treat
        # that as authoritative, otherwise locate/close/mute/volume for the
        # current opened process all become unavailable.  Fall back to the
        # official Start.exe /box:<name> /listpids query when the selected box
        # is missing from the API result.
        try:
            process_map = self._get_sandbox_process_map_via_api()
        except Exception:
            process_map = None
        if isinstance(process_map, dict):
            pids = sorted(set(process_map.get(sandbox_name.lower(), [])))
            if pids:
                return pids

        result, output = self._run_sandboxie_start([f"/box:{sandbox_name}", "/listpids"], timeout=timeout)
        if result is None:
            return []
        pids = self._parse_listpids_output(output)
        if pids:
            return pids
        return []

    def _get_running_sandbox_states(self, sandbox_names, timeout=1, max_workers=24):
        names = [str(name or "").strip() for name in (sandbox_names or []) if str(name or "").strip()]
        states = {name: False for name in names}
        if not names:
            return states

        if not getattr(self, "_sandbox_display_to_name", None):
            try:
                self._refresh_sandbox_display_maps()
            except Exception:
                pass

        process_map = self._get_sandbox_process_map_via_api()
        if process_map is not None:
            for display_name in names:
                internal_name = self._normalize_sandbox_name(display_name)
                if internal_name and is_valid_internal_sandbox_name(internal_name):
                    states[display_name] = bool(process_map.get(internal_name.lower()))
            return states

        targets = []
        seen_internal = set()
        for display_name in names:
            internal_name = self._normalize_sandbox_name(display_name)
            if not internal_name or not is_valid_internal_sandbox_name(internal_name):
                continue
            key = internal_name.lower()
            if key in seen_internal:
                continue
            seen_internal.add(key)
            targets.append((display_name, internal_name))

        if not targets:
            return states

        def _check_one(target):
            display_name, internal_name = target
            try:
                return display_name, bool(self._get_sandbox_pids(internal_name, timeout=timeout))
            except Exception:
                return display_name, False

        if len(targets) == 1:
            display_name, is_running = _check_one(targets[0])
            states[display_name] = is_running
            return states

        try:
            from concurrent.futures import ThreadPoolExecutor, as_completed
            workers = min(max(1, int(max_workers)), len(targets))
            with ThreadPoolExecutor(max_workers=workers) as executor:
                futures = [executor.submit(_check_one, target) for target in targets]
                for future in as_completed(futures):
                    display_name, is_running = future.result()
                    states[display_name] = is_running
        except Exception:
            for target in targets:
                display_name, is_running = _check_one(target)
                states[display_name] = is_running
        return states

    def _get_all_sandbox_pids(self, timeout=2):
        # Do not call Sandboxie Start.exe /listpids without /box.
        # On some machines Sandboxie tries the missing DefaultBox and shows
        # Avoid Sandboxie "invalid sandbox name: DefaultBox" popups when opening context menus.
        # Query the real configured boxes one by one instead.
        pids = []
        try:
            names = self._list_enabled_sandboxes_from_sbieini()
        except Exception:
            names = []
        if not names:
            try:
                names = sorted(self._get_existing_sandbox_names())
            except Exception:
                names = []
        seen_boxes = set()
        for name in names:
            box = self._normalize_sandbox_name(name)
            if not box or not is_valid_internal_sandbox_name(box):
                continue
            key = box.lower()
            if key in seen_boxes:
                continue
            seen_boxes.add(key)
            result, output = self._run_sandboxie_start([f"/box:{box}", "/listpids"], timeout=timeout)
            if result is None:
                continue
            pids.extend(self._parse_listpids_output(output))
        return sorted(set(pids))

    def _get_sandbox_process_map_via_api(self):
        api = self._get_sbie_api()
        if not api:
            return None
        enum_process = api.get("enum_process")
        query_process = api.get("query_process")
        if not enum_process or not query_process:
            return None
        try:
            count = wintypes.ULONG(0)
            ret = enum_process(None, True, 0xFFFFFFFF, None, ctypes.byref(count))
            if int(ret) != 0:
                return None
            if count.value <= 0:
                return {}
            arr = (wintypes.ULONG * int(count.value))()
            ret = enum_process(None, True, 0xFFFFFFFF, arr, ctypes.byref(count))
            if int(ret) != 0:
                return None
            process_map = {}
            for pid in list(arr[:int(count.value)]):
                try:
                    pid_value = int(pid)
                except Exception:
                    continue
                if pid_value <= 0:
                    continue
                box_name = ctypes.create_unicode_buffer(128)
                image_name = ctypes.create_unicode_buffer(260)
                sid_text = ctypes.create_unicode_buffer(260)
                session_id = wintypes.ULONG(0)
                try:
                    ret = query_process(
                        wintypes.HANDLE(pid_value),
                        box_name,
                        image_name,
                        sid_text,
                        ctypes.byref(session_id)
                    )
                except Exception:
                    continue
                if int(ret) != 0:
                    continue
                name = str(box_name.value or "").strip()
                if not name:
                    continue
                process_map.setdefault(name.lower(), []).append(pid_value)
            return process_map
        except Exception:
            return None

    def on_repair_sandboxie_control_settings(self, event):
        message = (
            "该操作会备份 Sandboxie.ini，然后清理 UserSettings 段中的 "
            "SbieCtrl_WindowCoords 窗口坐标记录。\n\n"
            "建议先关闭 Sandboxie Control 再执行。\n\n"
            "不会删除沙盒用户，也不会修改 GlobalSettings 或沙盒配置。是否继续？"
        )
        if wx.MessageBox(message, "修复 Sandboxie Control 用户设置", wx.YES_NO | wx.ICON_QUESTION, self) != wx.YES:
            return
        try:
            changed, detail = self._repair_sandboxie_control_user_settings()
        except Exception as e:
            detail = str(e) or "修复失败"
            self.lbl_status.SetLabel("修复 Sandboxie Control 用户设置失败")
            wx.MessageBox(f"修复失败：\n{detail}", "修复失败", wx.OK | wx.ICON_ERROR, self)
            return
        if changed:
            self.lbl_status.SetLabel("已修复 Sandboxie Control 用户设置")
            wx.MessageBox(detail, "修复完成", wx.OK | wx.ICON_INFORMATION, self)
        else:
            self.lbl_status.SetLabel("未发现需要修复的 Sandboxie Control 用户设置")
            wx.MessageBox(detail, "无需修复", wx.OK | wx.ICON_INFORMATION, self)

    def get_sandboxie_path_from_registry(self):
        """从多个常见注册表位置读取 Sandboxie 安装目录。"""
        registry_paths = [
            r"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Sandboxie",
            r"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Sandboxie",
            r"SOFTWARE\Sandboxie",
            r"SOFTWARE\WOW6432Node\Sandboxie",
        ]
        value_names = ("InstallLocation", "Install_Dir")
        for reg_path in registry_paths:
            key = None
            try:
                key = winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, reg_path)
                for value_name in value_names:
                    try:
                        value, _ = winreg.QueryValueEx(key, value_name)
                    except OSError:
                        continue
                    value = str(value or "").strip().strip('"')
                    if value and os.path.exists(os.path.join(value, "Start.exe")):
                        return value
            except OSError:
                pass
            finally:
                if key is not None:
                    try:
                        winreg.CloseKey(key)
                    except Exception:
                        pass
        return None

    def find_sandboxie_path(self):
        registry_path = self.get_sandboxie_path_from_registry()
        if registry_path:
            return registry_path

        candidate_roots = []
        seen = set()

        def add_candidate(path):
            path = str(path or "").strip()
            if not path:
                return
            norm = os.path.normcase(os.path.normpath(path))
            if norm in seen:
                return
            seen.add(norm)
            candidate_roots.append(path)

        for drive in "CDEFGHIJKLMNOPQRSTUVWXYZ":
            root = f"{drive}:\\"
            if not os.path.exists(root):
                continue
            for base in ("Program Files", "Program Files (x86)", ""):
                base_path = os.path.join(root, base) if base else root
                add_candidate(os.path.join(base_path, "Sandboxie"))
                add_candidate(os.path.join(base_path, "Sandboxie-Plus"))

        for path in candidate_roots:
            if os.path.exists(os.path.join(path, "Start.exe")):
                return path

        for drive in "CDEFGHIJKLMNOPQRSTUVWXYZ":
            root = f"{drive}:\\"
            if not os.path.exists(root):
                continue
            search_patterns = [
                os.path.join(root, "Program Files", "*Sandboxie*", "Start.exe"),
                os.path.join(root, "Program Files (x86)", "*Sandboxie*", "Start.exe"),
                os.path.join(root, "*Sandboxie*", "Start.exe"),
            ]
            for pattern in search_patterns:
                try:
                    matches = glob.glob(pattern, recursive=False)
                except Exception:
                    matches = []
                for match in matches:
                    path = os.path.dirname(match)
                    if os.path.exists(os.path.join(path, "Start.exe")):
                        return path
        return None

    def start_app_in_sandbox(self, sandbox_name, app_exe, app_dir):
        box_name = self._normalize_sandbox_name(sandbox_name)
        if not box_name:
            return False
        display_name = self._get_sandbox_display_name(box_name) or str(sandbox_name or "").strip() or box_name
        validation_error = get_internal_sandbox_name_validation_error(box_name)
        if validation_error:
            self._safe_call_after(self.lbl_status.SetLabel, f"启动失败：{validation_error}（{display_name}）")
            return False
        try:
            official_names = self._get_official_sandbox_name_set()
        except Exception:
            official_names = set()
        if official_names and box_name.lower() not in official_names:
            try:
                self._refresh_sandbox_display_maps()
            except Exception:
                pass
            if hasattr(self, "sandbox_list"):
                self._safe_call_after(self.sandbox_list.on_refresh_sandbox_list, None)
            self._safe_call_after(self.lbl_status.SetLabel, f"启动失败：Sandboxie 未启用沙盒 {display_name}，已刷新列表")
            return False
        app_path = str(app_exe or "").strip()
        if not app_path or not os.path.exists(app_path):
            return False
        start_exe = self._get_sandboxie_start_exe()
        if not start_exe:
            return False

        launch_cwd = str(app_dir or "").strip()
        if not launch_cwd or not os.path.isdir(launch_cwd):
            launch_cwd = None

        def _launch_once():
            # 真正启动沙盒应用时不要等待 Start.exe 退出；部分目标程序会让
            # Start.exe 保持较久，批量多开时等待会拖住启动队列并压住界面响应。
            return self._dispatch_sandboxie_start_detached(
                [f"/box:{box_name}", app_path],
                cwd=launch_cwd,
                hide_window=False,
                auto_silent=False,
            )

        result, output = _launch_once()
        if result:
            return True

        if self._is_invalid_sandbox_name_output(output):
            # 某些机器上会出现“无效的沙盒名参数”，先重载配置后再重试一次。
            try:
                self._reload_sandboxie_config()
            except Exception:
                pass
            result, output = _launch_once()
            if result:
                return True

            try:
                official_names = self._get_official_sandbox_name_set()
                if official_names and box_name.lower() not in official_names:
                    self._refresh_sandbox_display_maps()
                    if hasattr(self, "sandbox_list"):
                        self._safe_call_after(self.sandbox_list.on_refresh_sandbox_list, None)
                    self._safe_call_after(self.lbl_status.SetLabel, f"启动失败：Sandboxie 未启用沙盒 {display_name}，已刷新列表")
                    return False
            except Exception:
                pass

        if self._is_invalid_sandbox_name_output(output):
            self._safe_call_after(self.lbl_status.SetLabel, f"启动失败：无效的沙盒名参数 {display_name}")
        else:
            detail = ""
            for line in str(output or "").splitlines():
                text = str(line or "").strip()
                if text:
                    detail = text
                    break
            if detail:
                self._safe_call_after(self.lbl_status.SetLabel, f"启动失败：{display_name}，{detail}")
        return False
