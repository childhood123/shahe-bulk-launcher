import configparser
import ctypes
import json
import os
import threading
import time
from ctypes import wintypes

import wx

from ..bootstrap import get_app_dir, get_resource_path
from ..common import get_configuration_dir, parse_bool_setting
from ..ui.settings_dialogs import SpeechSettingsDialog


class SpeechMixin:
    def _get_speech_source_config_path(self):
        candidates = [
            os.path.join(getattr(self, "CONFIG_DIR", get_configuration_dir(get_app_dir())), "SpeechConfig.json"),
            os.path.join(get_app_dir(), "SpeechConfig.json"),
            get_resource_path("SpeechConfig.json"),
            os.path.join(get_app_dir(), "internal", "SpeechConfig.json"),
            os.path.join(get_resource_path("internal"), "SpeechConfig.json"),
            os.path.join(get_resource_path("Speech"), "SpeechConfig.json"),
            os.path.join(get_app_dir(), "Speech", "SpeechConfig.json"),
            os.path.join(get_app_dir(), "Speech", "SpeechConfig.json"),
            os.path.join(
                get_app_dir(),
                "py_work", "游戏", "哲隐语音组件", "Speech", "SpeechConfig.json"
            )
        ]
        for path in candidates:
            try:
                if os.path.isfile(path):
                    return path
            except Exception:
                continue
        return ""

    def _ensure_writable_dir(self, folder_path):
        try:
            os.makedirs(folder_path, exist_ok=True)
            test_path = os.path.join(folder_path, ".write_test.tmp")
            with open(test_path, "w", encoding="utf-8") as f:
                f.write("ok")
            os.remove(test_path)
            return True
        except Exception:
            return False

    def _get_speech_runtime_dir(self):
        # 用户可写配置统一放到 Configuration，旧目录仅作为读取兼容。
        folder_path = getattr(self, "CONFIG_DIR", get_configuration_dir(get_app_dir()))
        if self._ensure_writable_dir(folder_path):
            return folder_path
        return folder_path

    def _get_tolk_dll_path(self):
        candidates = [
            os.path.join(get_app_dir(), "internal", "Tolk.dll"),
            os.path.join(get_resource_path("internal"), "Tolk.dll"),
            os.path.join(get_app_dir(), "Tolk.dll"),
            get_resource_path("Tolk.dll"),
        ]
        for dll_path in candidates:
            try:
                if os.path.isfile(dll_path):
                    return os.path.abspath(dll_path)
            except Exception:
                continue
        return ""

    def _get_reader_ini_path(self, filename, for_write=False):
        name = str(filename or "").strip()
        if not name:
            return ""
        config_dir = getattr(self, "CONFIG_DIR", get_configuration_dir(get_app_dir()))
        config_path = os.path.join(config_dir, name)
        if for_write:
            self._ensure_writable_dir(config_dir)
            return config_path
        candidates = [
            config_path,
            os.path.join(get_app_dir(), "internal", name),
            os.path.join(get_resource_path("internal"), name),
            os.path.join(get_app_dir(), name),
            get_resource_path(name),
        ]
        for path in candidates:
            try:
                if os.path.isfile(path):
                    return path
            except Exception:
                continue
        if self._ensure_writable_dir(config_dir):
            return config_path
        return config_path

    def _load_reader_ini_settings(self):
        result = {
            "zdsr": {
                "type": "",
                "channelName": "",
                "keyDownInterrupt": "",
                "alwaysInterrupt": ""
            },
            "boy": {
                "Param1": "false",
                "Param2": "false",
                "Param3": "true",
                "Param4": "true"
            }
        }

        zdsr_path = self._get_reader_ini_path("ZDSRAPI.ini")
        if zdsr_path and os.path.isfile(zdsr_path):
            try:
                cp = configparser.ConfigParser()
                cp.optionxform = str
                cp.read(zdsr_path, encoding="utf-8")
                if cp.has_section("settings"):
                    section = cp["settings"]
                    result["zdsr"]["type"] = str(section.get("type", "") or "").strip()
                    result["zdsr"]["channelName"] = str(section.get("channelName", "") or "").strip()
                    result["zdsr"]["keyDownInterrupt"] = str(section.get("keyDownInterrupt", "") or "").strip()
                    result["zdsr"]["alwaysInterrupt"] = str(section.get("alwaysInterrupt", "") or "").strip()
            except Exception:
                pass

        boy_path = self._get_reader_ini_path("boyctrl.ini")
        if boy_path and os.path.isfile(boy_path):
            try:
                cp = configparser.ConfigParser()
                cp.optionxform = str
                cp.read(boy_path, encoding="utf-8")
                if cp.has_section("Config"):
                    section = cp["Config"]
                    for key in ("Param1", "Param2", "Param3", "Param4"):
                        value = str(section.get(key, result["boy"][key]) or "").strip().lower()
                        result["boy"][key] = "true" if value == "true" else "false"
            except Exception:
                pass
        return result

    def _save_reader_ini_settings(self, settings):
        try:
            data = SpeechSettingsDialog._normalize_reader_ini_settings(settings)

            zdsr_path = self._get_reader_ini_path("ZDSRAPI.ini", for_write=True)
            if zdsr_path:
                cp = configparser.ConfigParser()
                cp.optionxform = str
                cp["settings"] = {
                    "type": data["zdsr"].get("type", ""),
                    "channelName": data["zdsr"].get("channelName", ""),
                    "keyDownInterrupt": data["zdsr"].get("keyDownInterrupt", ""),
                    "alwaysInterrupt": data["zdsr"].get("alwaysInterrupt", "")
                }
                with open(zdsr_path, "w", encoding="utf-8") as f:
                    cp.write(f)

            boy_path = self._get_reader_ini_path("boyctrl.ini", for_write=True)
            if boy_path:
                cp = configparser.ConfigParser()
                cp.optionxform = str
                cp["Config"] = {
                    "Param1": data["boy"].get("Param1", "false"),
                    "Param2": data["boy"].get("Param2", "false"),
                    "Param3": data["boy"].get("Param3", "true"),
                    "Param4": data["boy"].get("Param4", "true")
                }
                with open(boy_path, "w", encoding="utf-8") as f:
                    cp.write(f)
            return True
        except Exception as e:
            self.lbl_status.SetLabel(f"读屏配置保存失败: {str(e)}")
            return False

    def _detect_screen_reader_and_match_engine(self, engine_names):
        process_name = ""
        reader_key = ""
        reader_name = ""
        names = []

        try:
            import psutil as _psutil
            for proc in _psutil.process_iter(["name"]):
                try:
                    name = str(proc.info.get("name", "") or "").strip()
                except Exception:
                    name = ""
                if name:
                    names.append(name.lower())
        except Exception:
            names = []

        patterns = [
            ("zdsr", "争渡读屏", ("zdsr", "zdsrapi", "saapi")),
            ("boy", "保益读屏", ("boyctrl", "boy")),
            ("nvda", "NVDA", ("nvda",)),
        ]
        for key, display_name, keys in patterns:
            matched = ""
            for n in names:
                if any(token in n for token in keys):
                    matched = n
                    break
            if matched:
                reader_key = key
                reader_name = display_name
                process_name = matched
                break

        engine_name = ""
        if reader_key and isinstance(engine_names, (list, tuple)):
            keyword_map = {
                "zdsr": ("争渡", "zdsr"),
                "boy": ("保益", "boy"),
                "nvda": ("nvda",),
            }
            keys = keyword_map.get(reader_key, ())
            for engine in engine_names:
                text = str(engine or "").strip()
                lower = text.lower()
                if any(k in lower for k in keys) or any(k in text for k in keys):
                    engine_name = text
                    break

        return {
            "reader_key": reader_key,
            "reader_name": reader_name,
            "process_name": process_name,
            "engine_name": engine_name
        }

    def _scan_reader_interface_dlls(self):
        return [
            {"label": "NVDA", "path": "nvda"},
            {"label": "争渡", "path": "zdsr"},
            {"label": "保益", "path": "boy"},
        ]

    def _normalize_speech_interface_value(self, value):
        text = str(value or "").strip()
        lower_text = text.lower()
        basename = os.path.basename(text).lower()
        mapping = {
            "nvda": "nvda",
            "nvdacontrollerclient.dll": "nvda",
            "nvdacontrollerclient64.dll": "nvda",
            "zdsr": "zdsr",
            "zdsrapi.dll": "zdsr",
            "zdsrapi_x64.dll": "zdsr",
            "争渡": "zdsr",
            "boy": "boy",
            "boyctrl.dll": "boy",
            "boyctrl-x64.dll": "boy",
            "保益": "boy",
        }
        return mapping.get(lower_text) or mapping.get(basename, "")

    def _is_allowed_speech_interface_dll_path(self, value):
        return bool(self._normalize_speech_interface_value(value))

    def _get_preferred_speech_interface_dll(self, choices):
        try:
            detected = self._detect_screen_reader_and_match_engine([])
            if isinstance(detected, dict):
                detected_key = str(detected.get("reader_key", "") or "").strip().lower()
                if detected_key in {"nvda", "zdsr", "boy"}:
                    return detected_key
        except Exception:
            pass
        return ""

    def _get_assist_speech_engine_dir(self):
        internal_dir = os.path.join(get_app_dir(), "internal")
        if os.path.isdir(internal_dir):
            return internal_dir
        return get_app_dir()

    def _ensure_assist_speech_runtime(self):
        if self._assist_speech_runtime is not None:
            return True
        if self._assist_speech_disabled:
            return False
        with self._assist_speech_init_lock:
            if self._assist_speech_runtime is not None:
                return True
            if self._assist_speech_disabled:
                return False
            tolk_path = self._get_tolk_dll_path()
            if not tolk_path:
                self._assist_speech_disabled = True
                return False
            try:
                engine_dir = self._get_assist_speech_engine_dir()
                if engine_dir and os.path.isdir(engine_dir):
                    if engine_dir not in os.environ.get("PATH", ""):
                        os.environ["PATH"] = engine_dir + os.pathsep + os.environ.get("PATH", "")
                    try:
                        if hasattr(os, "add_dll_directory"):
                            os.add_dll_directory(engine_dir)
                    except Exception:
                        pass

                dll = ctypes.CDLL(tolk_path)
                load_fn = dll.Tolk_Load
                load_fn.argtypes = []
                load_fn.restype = None

                unload_fn = dll.Tolk_Unload
                unload_fn.argtypes = []
                unload_fn.restype = None

                is_loaded_fn = dll.Tolk_IsLoaded
                is_loaded_fn.argtypes = []
                is_loaded_fn.restype = wintypes.BOOL

                output_fn = dll.Tolk_Output
                output_fn.argtypes = [wintypes.LPCWSTR, wintypes.BOOL]
                output_fn.restype = wintypes.BOOL

                silence_fn = dll.Tolk_Silence
                silence_fn.argtypes = []
                silence_fn.restype = wintypes.BOOL

                try_sapi_fn = dll.Tolk_TrySAPI
                try_sapi_fn.argtypes = [wintypes.BOOL]
                try_sapi_fn.restype = None

                try_sapi_fn(False)
                load_fn()
                if not bool(is_loaded_fn()):
                    self._assist_speech_disabled = True
                    return False

                self._assist_speech_runtime = {
                    "dll": dll,
                    "load": load_fn,
                    "output": output_fn,
                    "silence": silence_fn,
                    "unload": unload_fn,
                }
                return True
            except Exception:
                self._assist_speech_disabled = True
                self._assist_speech_runtime = None
                return False

    def _speak_assist_text(self, text, interrupt=True):
        content = str(text or "").strip()
        if not content:
            return
        if getattr(self, "_destroy_started", False) or self._is_window_closing():
            return
        if not parse_bool_setting(self.speech_assist_hints.get("enabled", False), False):
            return

        def _worker():
            with self._assist_speech_call_lock:
                if self._is_window_closing():
                    return
                if not self._ensure_assist_speech_runtime():
                    return
                runtime = self._assist_speech_runtime
                if not runtime:
                    return
                try:
                    if interrupt:
                        try:
                            runtime["silence"]()
                        except Exception:
                            pass
                    runtime["output"](content, bool(interrupt))
                except Exception:
                    pass

        threading.Thread(target=_worker, daemon=True).start()

    def _shutdown_assist_speech_runtime(self):
        call_lock = getattr(self, "_assist_speech_call_lock", None)
        init_lock = getattr(self, "_assist_speech_init_lock", None)

        def _detach_runtime():
            if init_lock is not None:
                with init_lock:
                    self._assist_speech_runtime = None
                    self._assist_speech_disabled = True
            else:
                self._assist_speech_runtime = None
                self._assist_speech_disabled = True

        if call_lock is not None:
            try:
                with call_lock:
                    _detach_runtime()
                    return
            except Exception:
                pass
        else:
            try:
                _detach_runtime()
                return
            except Exception:
                pass
        try:
            self._assist_speech_runtime = None
            self._assist_speech_disabled = True
        except Exception:
            pass

    def _save_speech_interface_dll(self, dll_path):
        value = self._normalize_speech_interface_value(dll_path)
        self.speech_interface_dll = value
        self.save_settings()
        return True

    def _save_speech_assist_hints(self, hints):
        data = hints if isinstance(hints, dict) else {}
        enabled = parse_bool_setting(data.get("enabled", self.speech_assist_hints.get("enabled", False)), False)
        mute_current = str(data.get("mute_current", "") or "").strip()
        mute_all = str(data.get("mute_all", "") or "").strip()
        if not mute_current:
            mute_current = str(self.speech_assist_hints.get("mute_current", "") or "").strip() or "静音当前打开的进程"
        if not mute_all:
            mute_all = str(self.speech_assist_hints.get("mute_all", "") or "").strip() or "静音全部打开的进程"
        self.speech_assist_hints = {
            "enabled": enabled,
            "mute_current": mute_current,
            "mute_all": mute_all
        }
        self.save_settings()
        return True

    def _with_assist_hint(self, hint_key, status_text):
        if not parse_bool_setting(self.speech_assist_hints.get("enabled", False), False):
            return str(status_text or "").strip()
        hint = str(self.speech_assist_hints.get(hint_key, "") or "").strip()
        text = str(status_text or "").strip()
        if hint:
            return f"{hint}，{text}"
        return text

    def _speak_process_assist(self, text, interrupt=True, event_key=""):
        key = str(event_key or "").strip().lower()
        allowed_keys = {
            "mute_current",
            "mute_all",
            "volume_current",
            "volume_all",
            "switch_output_current",
            "switch_output_all",
            "close_current",
            "close_all",
            "locate_current",
            "launch_done",
        }
        if key and key not in allowed_keys:
            return

        content = str(text or "").strip()
        if not content:
            return

        now = time.time()
        if content == self._assist_last_text and (now - self._assist_last_time) < 0.45:
            return
        self._assist_last_text = content
        self._assist_last_time = now
        self._speak_assist_text(content, interrupt=interrupt)

    def _get_speech_config_path(self):
        runtime_dir = self._get_speech_runtime_dir()
        if not runtime_dir:
            return ""
        config_path = os.path.join(runtime_dir, "SpeechConfig.json")
        if os.path.isfile(config_path):
            return config_path

        source_path = self._get_speech_source_config_path()
        if source_path:
            try:
                with open(source_path, "r", encoding="utf-8") as f:
                    source_raw = json.load(f)
                source_data = SpeechSettingsDialog.sanitize_config(source_raw)
                with open(config_path, "w", encoding="utf-8") as f:
                    json.dump(source_data, f, ensure_ascii=False, indent=2)
                return config_path
            except Exception:
                pass

        default_config = {"DefaultEngine": "", "TTSInfos": {}}
        try:
            with open(config_path, "w", encoding="utf-8") as f:
                json.dump(default_config, f, ensure_ascii=False, indent=2)
            return config_path
        except Exception:
            return ""

    def _load_speech_config(self):
        config_path = self._get_speech_config_path()
        if not config_path:
            return {"DefaultEngine": "", "TTSInfos": {}}
        try:
            with open(config_path, "r", encoding="utf-8") as f:
                raw = json.load(f)
        except Exception:
            raw = {"DefaultEngine": "", "TTSInfos": {}}
        cleaned = SpeechSettingsDialog.sanitize_config(raw)
        if cleaned != raw:
            try:
                with open(config_path, "w", encoding="utf-8") as f:
                    json.dump(cleaned, f, ensure_ascii=False, indent=2)
            except Exception:
                pass
        return cleaned

    def _save_speech_config(self, config):
        config_path = self._get_speech_config_path()
        if not config_path:
            self.lbl_status.SetLabel("朗读设置保存失败：无法创建配置目录")
            return False
        data = SpeechSettingsDialog.sanitize_config(config)
        try:
            with open(config_path, "w", encoding="utf-8") as f:
                json.dump(data, f, ensure_ascii=False, indent=2)
            self.lbl_status.SetLabel("朗读设置已保存")
            return True
        except Exception as e:
            self.lbl_status.SetLabel(f"朗读设置保存失败: {str(e)}")
            return False

    def on_speech_settings(self, event):
        dll_path = self._get_tolk_dll_path()
        if dll_path:
            self.lbl_status.SetLabel("已检测到读屏辅助接口")
        else:
            self.lbl_status.SetLabel("未检测到读屏辅助接口，请检查 internal 目录")
            wx.MessageBox(
                "未检测到读屏辅助接口。\n请确认 internal 目录中的读屏辅助文件完整。",
                "朗读设置提示",
                wx.OK | wx.ICON_WARNING,
                self
            )
        reader_ini_settings = self._load_reader_ini_settings()
        interface_choices = self._scan_reader_interface_dlls()
        selected_interface_dll = self._normalize_speech_interface_value(self.speech_interface_dll)
        dialog = SpeechSettingsDialog(
            self,
            {},
            lambda config: True,
            reader_ini_settings=reader_ini_settings,
            on_save_reader_ini_callback=self._save_reader_ini_settings,
            detect_reader_callback=self._detect_screen_reader_and_match_engine,
            interface_dll_choices=interface_choices,
            selected_interface_dll=selected_interface_dll,
            on_save_interface_dll_callback=self._save_speech_interface_dll,
            assist_hints=self.speech_assist_hints,
            on_save_assist_hints_callback=self._save_speech_assist_hints
        )
        try:
            if dialog.ShowModal() != wx.ID_OK:
                return
            self.create_menu_bar()
        finally:
            dialog.Destroy()
