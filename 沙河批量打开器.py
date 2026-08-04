import sys
import os
import subprocess
import time
import winreg
import glob
import json
import shutil
import tempfile
from pathlib import Path

_INTERNAL_DLL_HANDLES = []


def _is_packaged_runtime():
    return bool(globals().get("__compiled__", False)) or os.environ.get("SHAHE_PACKAGED_APP") == "1"


def _cleanup_release_internal_dir(internal_dir):
    for obsolete_name in ("沙河批量打开器.exe", "updater.exe"):
        try:
            obsolete_path = Path(internal_dir) / obsolete_name
            if obsolete_path.is_file():
                obsolete_path.unlink()
        except Exception:
            pass


def _bootstrap_internal_runtime():
    if not _is_packaged_runtime():
        return
    try:
        executable_path = Path(str(getattr(sys, "argv", [""])[0] or "")).resolve()
    except Exception:
        executable_path = Path(__file__).resolve()
    executable_dir = executable_path.parent
    if executable_dir.name.lower() == "internal":
        app_root = executable_dir.parent
    else:
        app_root = executable_dir
    internal_dir = app_root / "internal"
    if not internal_dir.is_dir():
        return
    _cleanup_release_internal_dir(internal_dir)
    internal_text = str(internal_dir)
    if internal_text not in sys.path:
        sys.path.insert(0, internal_text)
    path_parts = [part for part in os.environ.get("PATH", "").split(os.pathsep) if part]
    if os.path.normcase(internal_text) not in {os.path.normcase(part) for part in path_parts}:
        os.environ["PATH"] = internal_text + (os.pathsep + os.environ.get("PATH", "") if os.environ.get("PATH") else "")
    if hasattr(os, "add_dll_directory"):
        try:
            _INTERNAL_DLL_HANDLES.append(os.add_dll_directory(internal_text))
        except Exception:
            pass


_bootstrap_internal_runtime()

import wx
import wx.lib.mixins.listctrl as listmix
import threading
import datetime
import zipfile
import configparser
import re
import ctypes
import codecs
import urllib.request
import urllib.error
from ctypes import wintypes
from urllib.parse import unquote

SCRIPT_DIR = Path(__file__).resolve().parent
for shared_root in (
    SCRIPT_DIR,
    SCRIPT_DIR / "MyCodex",
    SCRIPT_DIR.parent if SCRIPT_DIR.name == "shahe_bulk_launcher" else None,
):
    if shared_root and (shared_root / "shahe_bulk_launcher" / "common.py").exists():
        shared_root_path = str(shared_root)
        if shared_root_path not in sys.path:
            sys.path.insert(0, shared_root_path)
        break

if _is_packaged_runtime():
    from common import (
        ConfigStore,
        compare_versions,
        end_dialog_safely,
        get_configuration_db_path,
        get_configuration_dir,
        get_hidden_subprocess_kwargs,
        get_update_temp_dir,
        is_allowed_update_body_status,
        open_update_url,
        parse_bool_setting,
        parse_int_setting,
        parse_version_parts,
    )
else:
    from shahe_bulk_launcher.common import (
        ConfigStore,
        compare_versions,
        end_dialog_safely,
        get_configuration_db_path,
        get_configuration_dir,
        get_hidden_subprocess_kwargs,
        get_update_temp_dir,
        is_allowed_update_body_status,
        open_update_url,
        parse_bool_setting,
        parse_int_setting,
        parse_version_parts,
    )

from shahe_bulk_launcher.bootstrap import (
    append_daily_log,
    cleanup_legacy_release_artifacts,
    get_app_dir,
    get_current_executable_path,
    get_resource_path,
    is_packaged_app,
)
from shahe_bulk_launcher.constants import (
    APP_VERSION,
    FIXED_HOTKEY_LABEL_BY_ACTION,
    FIXED_HOTKEY_TEXT_BY_ACTION,
    LOG_OUTPUT_CHOICES,
    LOG_OUTPUT_DETAILED,
    LOG_OUTPUT_OFF,
    OLD_DEFAULT_SANDBOX_ROOT,
    SANDBOX_INTERNAL_NAME_PATTERN,
    UPDATE_MANIFEST_URL,
    get_default_sandbox_root,
    get_internal_sandbox_name_validation_error,
    get_sandbox_display_name_validation_error,
    is_valid_internal_sandbox_name,
    make_default_sandbox_display_prefix,
    normalize_log_output_mode,
    normalize_sandbox_root_path,
)
from shahe_bulk_launcher.single_instance import SingleInstance

from shahe_bulk_launcher.ui.controls import (
    AppPathListCtrl,
    CheckableListCtrl,
    ManagedDelayedCall,
    OperationStatusText,
    SingleColumnListCtrl,
)

from shahe_bulk_launcher.ui.common_dialogs import (
    BatchNameRuleDialog,
    GroupNameDialog,
    QRCodeDialog,
    SelectUsersDialog,
    _load_wx_image_from_bytes,
    copy_image_to_clipboard,
)
from shahe_bulk_launcher.ui.settings_dialogs import (
    HotkeySettingsDialog,
    PathMemoryManagerDialog,
    SettingsDialog,
    SpeechSettingsDialog,
)
from shahe_bulk_launcher.ui.tray import TaskBarIcon

from shahe_bulk_launcher.backup import BackupRestoreMgr
from shahe_bulk_launcher.features.lifecycle import LifecycleMixin
from shahe_bulk_launcher.features.settings import SettingsMixin





















try:
    from qr_embedded import QR_EMBEDDED_IMAGES
except Exception:
    QR_EMBEDDED_IMAGES = {}

pythoncom = None
AudioUtilities = None
AudioRouter = None

























class SandboxLauncher(
    LifecycleMixin,
    SettingsMixin,
    wx.Frame,
):
    def __init__(self):
        super().__init__(None, title="沙河批量打开器", size=(600, 500))
        default_sandbox_root = get_default_sandbox_root()
        self.SANDBOX_ROOT = default_sandbox_root
        self.APP_PATH = ""
        self.APP_DIR = ""
        self.CONFIG_DIR = get_configuration_dir(get_app_dir())
        self.CONFIG_DB_FILE = get_configuration_db_path(get_app_dir())
        self.CONFIG_FILE = os.path.join(get_app_dir(), "app_config.dat")
        self.config_store = ConfigStore(self.CONFIG_DB_FILE)
        self.settings = self.load_settings()
        self.log_output_mode = normalize_log_output_mode(self.settings.get("log_output_mode", LOG_OUTPUT_DETAILED))
        self.settings["log_output_mode"] = self.log_output_mode
        self._log_operation(f"启动软件: {APP_VERSION}")
        self.max_history_items = parse_int_setting(self.settings.get("max_history_items", 10), 10, 0, 20)
        self.hide_path = parse_bool_setting(self.settings.get("hide_path", False), False)
        self.app_history = self._normalize_app_history(
            self.settings.get("app_history", []),
            limit=self.max_history_items
        )
        self.SANDBOXIE_PATH = str(self.settings.get("sandboxie_path", "") or "").strip()
        if self.SANDBOXIE_PATH and not self._is_valid_sandboxie_install_path(self.SANDBOXIE_PATH):
            self.SANDBOXIE_PATH = ""
            self.settings["sandboxie_path"] = ""
        self.sandbox_root = normalize_sandbox_root_path(self.settings.get("sandbox_root", default_sandbox_root))
        if not self.sandbox_root:
            self.sandbox_root = default_sandbox_root
        self.settings["sandbox_root"] = self.sandbox_root
        if (
            self.sandbox_root == OLD_DEFAULT_SANDBOX_ROOT
            and default_sandbox_root != OLD_DEFAULT_SANDBOX_ROOT
            and not os.path.exists(self.sandbox_root)
        ):
            self.sandbox_root = default_sandbox_root
        self.SANDBOX_ROOT = self.sandbox_root
        self.launch_interval = parse_int_setting(self.settings.get("launch_interval", 5), 5, 0, 15)
        self.launch_concurrency_limit = parse_int_setting(self.settings.get("launch_concurrency_limit", 1), 1, 1, 32)
        self.dynamic_launch_delay_by_load = parse_bool_setting(
            self.settings.get("dynamic_launch_delay_by_load", False),
            False
        )
        self.show_tray = parse_bool_setting(self.settings.get("show_tray", True), True)
        self.auto_run = parse_bool_setting(self.settings.get("auto_run", False), False)
        self.settings.pop("auto_update_on_startup", None)
        self.settings.pop("auto_check_update_on_startup", None)
        self.auto_select_range_on_check = parse_bool_setting(
            self.settings.get("auto_select_range_on_check", False),
            False
        )
        self.list_use_left_right_nav = parse_bool_setting(self.settings.get("list_use_left_right_nav", True), True)
        self.minimize_to_tray_after_launch = False
        self.skip_running_sandbox_on_launch = parse_bool_setting(
            self.settings.get("skip_running_sandbox_on_launch", False),
            False
        )
        self.remember_volume = parse_bool_setting(self.settings.get("remember_volume", True), True)
        self.hotkeys_enabled = parse_bool_setting(self.settings.get("hotkeys_enabled", False), False)
        self.hotkey_action_specs = self._build_hotkey_action_specs()
        self.default_hotkeys = {spec["action"]: spec["default"] for spec in self.hotkey_action_specs}
        self.hotkey_event_ids = {spec["action"]: spec["event_id"] for spec in self.hotkey_action_specs}
        self.hotkeys = self._normalize_hotkey_map(self.settings.get("hotkeys", {}))
        self.sandbox_groups = self._normalize_sandbox_groups(self.settings.get("sandbox_groups", {}))
        self.pinned_sandboxes = self._normalize_sandbox_name_list(self.settings.get("pinned_sandboxes", []))
        self.sandbox_volume_memory = self._normalize_volume_memory(self.settings.get("sandbox_volume_memory", {}))
        raw_speech_interface = str(self.settings.get("speech_interface_dll", "") or "").strip()
        self.speech_interface_dll = self._normalize_speech_interface_value(raw_speech_interface)
        if raw_speech_interface and not self.speech_interface_dll:
            self.speech_interface_dll = ""
            self.settings["speech_interface_dll"] = ""
        raw_hints = self.settings.get("speech_assist_hints", {})
        self.speech_assist_hints = raw_hints if isinstance(raw_hints, dict) else {}
        self.speech_assist_hints["enabled"] = parse_bool_setting(self.speech_assist_hints.get("enabled", True), True)
        if not str(self.speech_assist_hints.get("mute_current", "") or "").strip():
            self.speech_assist_hints["mute_current"] = "静音当前打开的进程"
        if not str(self.speech_assist_hints.get("mute_all", "") or "").strip():
            self.speech_assist_hints["mute_all"] = "静音全部打开的进程"
        self._cached_mute_state_by_sandbox = {}
        self._cached_mute_state_all = None
        self._process_menu_state_cache = {}
        self._process_menu_state_cache_time = 0.0
        self._process_menu_live_state_cache = {}
        self._process_menu_live_state_refresh_token = 0
        self._process_menu_live_state_refresh_in_progress = False
        self._process_menu_live_state_pending = False
        self._process_menu_live_state_timer = None
        self._suppress_selection_status_until = 0.0
        self._suppress_list_refresh_status_until = 0.0
        self._sandbox_selection_process_state_timer = None
        self._pending_update_package_info = None
        self._pending_update_latest_version = ""
        self._sandbox_display_to_name = {}
        self._sandbox_name_to_display = {}
        self._schedule_jobs = {}
        self._schedule_seq = 0
        self.force_exit_requested = False
        self.is_launching = False
        self._launch_control_lock = threading.Lock()
        self._launch_cancel_event = threading.Event()
        self._launch_session_id = 0
        self._active_launch_targets = None
        self._window_closing = False
        self._destroy_started = False
        self._destroy_later_scheduled = False
        self._pending_call_laters = []
        self._menu_bar_rebuild_timer = None
        self._menu_bar_rebuild_pending = False
        self._locate_reinforcement_lock = threading.Lock()
        self._locate_reinforcement_event = None
        self._locate_reinforcement_thread = None
        self._manual_process_refresh_status_pending = False
        self.task_bar_icon = None
        self._assist_speech_runtime = None
        self._assist_speech_init_lock = threading.Lock()
        self._assist_speech_call_lock = threading.Lock()
        self._assist_speech_disabled = False
        self._assist_last_text = ""
        self._assist_last_time = 0.0
        self._volume_task_lock = threading.Lock()
        self._close_process_task_lock = threading.Lock()
        self.init_ui()
        try:
            # 启动时自动清洗并回写语音配置，移除无效引擎与空默认项。
            self._load_speech_config()
        except Exception:
            pass
        self.sandbox_list.set_sandbox_root(self.sandbox_root)
        self.sandbox_list.set_range_auto_check_enabled(self.auto_select_range_on_check)
        self.sandbox_list.set_left_right_navigation_enabled(self.list_use_left_right_nav)
        self.lbl_status.SetLabel("正在刷新沙盒用户...")
        self.Center()
        self.Show()
        if self.show_tray:
            self.create_task_bar_icon()
        if not self.SANDBOXIE_PATH:
            self.first_time_scan_sandboxie()
        else:
            self.lbl_status.SetLabel("就绪")
        self._safe_call_later(900, self._refresh_sandbox_list_on_startup)
        self._apply_auto_run()
        self._safe_call_later(1500, self._auto_check_update_after_startup)































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






    def add_to_history(self, app_path, refresh=True):
        if not app_path:
            return False
        app_path, _ = self.extract_path_and_note(str(app_path))
        if not app_path or not os.path.exists(app_path) or self.max_history_items == 0:
            return False
        changed = False
        for item in self.app_history:
            if item["path"] == app_path:
                self.app_history.remove(item)
                self.app_history.insert(0, item)
                changed = True
                break
        else:
            self.app_history.insert(0, {"path": app_path, "note": os.path.basename(app_path)})
            changed = True
        if len(self.app_history) > self.max_history_items:
            self.app_history = self.app_history[:self.max_history_items]
            changed = True
        if changed:
            self.save_settings()
            if refresh:
                self.update_combo_box()
        return changed

    def format_display_text(self, item):
        return item["note"] if self.hide_path else f'{item["note"]} - "{item["path"]}"'

    def _get_app_history_item_by_index(self, index):
        try:
            target_index = int(index)
        except Exception:
            return None
        if target_index < 0 or target_index >= len(self.app_history):
            return None
        try:
            item = self.app_history[target_index]
        except Exception:
            return None
        if not isinstance(item, dict):
            return None
        return item

    def _get_app_history_item_by_path(self, app_path):
        index = self._find_app_history_index_by_path(app_path)
        if index == -1:
            return None
        try:
            return self.app_history[index]
        except Exception:
            return None

    def _get_app_list_path_by_index(self, index):
        try:
            target_index = int(index)
        except Exception:
            return ""
        paths = getattr(self, "_combo_app_path_item_paths", [])
        if target_index < 0 or target_index >= len(paths):
            return ""
        return str(paths[target_index] or "").strip()

    def _find_app_history_index_by_path(self, app_path):
        target_path = str(app_path or "").strip()
        if not target_path:
            return -1
        target_key = os.path.normcase(os.path.abspath(target_path))
        for index, item in enumerate(self.app_history):
            item_path = str(item.get("path", "") or "").strip()
            if not item_path:
                continue
            try:
                item_key = os.path.normcase(os.path.abspath(item_path))
            except Exception:
                item_key = os.path.normcase(item_path)
            if item_key == target_key:
                return index
        return -1

    def _set_combo_app_path_by_history_path(self, app_path, fallback_text=""):
        target_path = str(app_path or "").strip()
        if not hasattr(self, "combo_app_path"):
            return False
        index = self._find_app_history_index_by_path(target_path)
        if index != -1:
            resolved_path = str(self.app_history[index].get("path", "") or "").strip()
            if hasattr(self.combo_app_path, "SetValueByIndex"):
                self.combo_app_path.SetValueByIndex(index)
            else:
                self.combo_app_path.SetValue(self.format_display_text(self.app_history[index]))
            if resolved_path:
                self.APP_PATH = resolved_path
                self.APP_DIR = os.path.dirname(resolved_path)
            return True
        self.combo_app_path.SetValue(fallback_text)
        return False

    def _focus_app_path_list(self):
        if not hasattr(self, "combo_app_path"):
            return
        try:
            self.combo_app_path.SetFocus()
        except Exception:
            pass
        try:
            index = self.combo_app_path.GetFirstSelected()
            if index != -1:
                self.combo_app_path.Focus(index)
                self.combo_app_path.EnsureVisible(index)
        except Exception:
            pass

    def _resolve_app_history_entry(self, text, preferred_index=None):
        item = self._get_app_history_item_by_index(preferred_index)
        if item is not None:
            return item
        current_path, current_note = self.extract_path_and_note(text)
        if not current_path:
            return None
        index = self._find_app_history_index_by_path(current_path)
        if index != -1:
            return self.app_history[index]
        return {"path": current_path, "note": current_note or os.path.basename(current_path)}

    def _resolve_current_app_path_and_note(self, text=None, preferred_index=None):
        current_text = self.combo_app_path.GetValue() if text is None and hasattr(self, "combo_app_path") else text
        current_text = str(current_text or "")
        if preferred_index is None and hasattr(self, "combo_app_path"):
            try:
                preferred_index = self.combo_app_path.GetFirstSelected()
            except Exception:
                preferred_index = -1
        current_path, current_note = self.extract_path_and_note(current_text, preferred_index=preferred_index)
        if preferred_index is not None and int(preferred_index) >= 0 and current_path:
            return current_path, current_note

        app_path = str(getattr(self, "APP_PATH", "") or "").strip()
        if app_path:
            item = self._get_app_history_item_by_path(app_path)
            if item is not None:
                return item["path"], item["note"]
            if os.path.exists(app_path):
                return app_path, os.path.basename(app_path)
        return current_path, current_note

    def update_combo_box(self):
        current = self.combo_app_path.GetValue()
        current_path = ""
        checked_paths = []
        if current:
            try:
                current_index = self.combo_app_path.GetFirstSelected() if hasattr(self, "combo_app_path") else -1
                current_path, _ = self._resolve_current_app_path_and_note(current, preferred_index=current_index)
            except Exception:
                current_path = ""
        if hasattr(self, "combo_app_path") and hasattr(self.combo_app_path, "get_checked_items"):
            try:
                for checked_index in self.combo_app_path.get_checked_items():
                    checked_path = self._get_app_list_path_by_index(checked_index)
                    if not checked_path:
                        checked_text = self.combo_app_path.GetItemText(checked_index)
                        checked_path, _ = self.extract_path_and_note(checked_text, preferred_index=checked_index)
                    checked_path = str(checked_path or "").strip()
                    if checked_path:
                        checked_paths.append(checked_path)
            except Exception:
                checked_paths = []
        if hasattr(self.combo_app_path, "begin_programmatic_update"):
            self.combo_app_path.begin_programmatic_update()
        try:
            item_texts = [self.format_display_text(item) for item in self.app_history]
            self._combo_app_path_item_paths = [str(item.get("path", "") or "").strip() for item in self.app_history]
            if hasattr(self.combo_app_path, "replace_items"):
                self.combo_app_path.replace_items(item_texts)
            else:
                self.combo_app_path.Clear()
                for text in item_texts:
                    self.combo_app_path.Append(text)
            restored_current = False
            if current_path:
                item_index = self._find_app_history_index_by_path(current_path)
                if item_index != -1:
                    item = self.app_history[item_index]
                    self._set_combo_app_path_by_history_path(current_path, self.format_display_text(item))
                    item_path = str(item.get("path", "") or "").strip()
                    self.APP_PATH = item_path
                    self.APP_DIR = os.path.dirname(item_path)
                    restored_current = True
            if (not restored_current) and current and current in item_texts:
                self.combo_app_path.SetValue(current)
                selected_index = self.combo_app_path.GetFirstSelected() if hasattr(self, "combo_app_path") else -1
                selected_path = self._get_app_list_path_by_index(selected_index)
                if selected_path:
                    self.APP_PATH = selected_path
                    self.APP_DIR = os.path.dirname(selected_path)
                restored_current = True
            if (not restored_current) and self.app_history:
                first_item = self.app_history[0]
                first_path = str(first_item.get("path", "") or "").strip()
                self._set_combo_app_path_by_history_path(first_path, self.format_display_text(first_item))
                if first_path:
                    self.APP_PATH = first_path
                    self.APP_DIR = os.path.dirname(first_path)
            elif not restored_current:
                self.combo_app_path.SetValue("")
                self.APP_PATH = ""
                self.APP_DIR = ""
            checked_keys = set()
            for checked_path in checked_paths:
                try:
                    checked_keys.add(os.path.normcase(os.path.abspath(checked_path)))
                except Exception:
                    checked_keys.add(os.path.normcase(checked_path))
            if checked_keys and hasattr(self.combo_app_path, "CheckItem"):
                for index, item_path in enumerate(self._combo_app_path_item_paths):
                    current_item_path = str(item_path or "").strip()
                    if not current_item_path:
                        continue
                    try:
                        item_key = os.path.normcase(os.path.abspath(current_item_path))
                    except Exception:
                        item_key = os.path.normcase(current_item_path)
                    if item_key in checked_keys:
                        try:
                            wx.ListCtrl.CheckItem(self.combo_app_path, index, True)
                        except Exception:
                            pass
        finally:
            if hasattr(self.combo_app_path, "end_programmatic_update"):
                self.combo_app_path.end_programmatic_update()


    def on_ctrl_o_open(self, event):
        self.on_browse_app(event)

    def _extract_exe_path_from_text(self, text):
        raw = str(text or "").strip()
        if not raw:
            return ""

        candidates = [raw]
        if "\r" in raw or "\n" in raw:
            candidates.extend([part.strip() for part in re.split(r"[\r\n]+", raw) if part.strip()])

        candidates.extend(re.findall(r'"([^"]+?\.exe)"', raw, flags=re.IGNORECASE))
        candidates.extend(re.findall(r"'([^']+?\.exe)'", raw, flags=re.IGNORECASE))
        candidates.extend(re.findall(r"([A-Za-z]:[\\/][^<>\r\n\"|]+?\.exe)", raw, flags=re.IGNORECASE))
        candidates.extend(re.findall(r"(\\\\[^<>\r\n\"|]+?\.exe)", raw, flags=re.IGNORECASE))

        seen = set()
        for candidate in candidates:
            value = str(candidate or "").strip().strip("\ufeff").strip("\x00")
            if not value:
                continue

            if value.lower().startswith("file://"):
                value = unquote(value[7:])
                if value.startswith("/") and len(value) >= 3 and value[2] == ":":
                    value = value[1:]
                value = value.replace("/", "\\")

            if (value.startswith('"') and value.endswith('"')) or (value.startswith("'") and value.endswith("'")):
                value = value[1:-1].strip()
            if not value:
                continue

            value = os.path.expandvars(os.path.expanduser(value))
            if value.lower().startswith("file://"):
                continue

            if value.startswith("\\\\"):
                value = value.replace("/", "\\")
            elif re.match(r"^[A-Za-z]:/", value):
                value = value.replace("/", "\\")

            if ".exe" in value.lower():
                value = value[:value.lower().find(".exe") + 4]

            try:
                value = os.path.abspath(os.path.normpath(value))
            except Exception:
                pass

            key = os.path.normcase(value)
            if key in seen:
                continue
            seen.add(key)

            if os.path.isfile(value) and value.lower().endswith(".exe"):
                return value
        return ""

    def _set_current_app_path(self, app_path, status_prefix="已选择"):
        path = self._extract_exe_path_from_text(app_path)
        if not path:
            return False
        self.APP_PATH = path
        self.APP_DIR = os.path.dirname(path)
        self.add_to_history(path)
        if not self._set_combo_app_path_by_history_path(path, path):
            self.combo_app_path.SetValue(path)
        self._focus_app_path_list()
        self.lbl_status.SetLabel(f"{status_prefix}: {os.path.basename(path)}")
        return True

    def on_paste_file(self, event):
        chosen_path = ""
        try:
            if not wx.TheClipboard.Open():
                self.lbl_status.SetLabel("无法访问剪贴板")
                return
            try:
                file_data = wx.FileDataObject()
                if wx.TheClipboard.GetData(file_data):
                    for file_path in file_data.GetFilenames():
                        parsed = self._extract_exe_path_from_text(file_path)
                        if parsed:
                            chosen_path = parsed
                            break
                if not chosen_path:
                    text_data = wx.TextDataObject()
                    if wx.TheClipboard.GetData(text_data):
                        chosen_path = self._extract_exe_path_from_text(text_data.GetText())
            finally:
                wx.TheClipboard.Close()
        except Exception as e:
            self.lbl_status.SetLabel(f"粘贴文件时出错: {str(e)}")
            return

        if chosen_path and self._set_current_app_path(chosen_path, "已从剪贴板粘贴应用"):
            return
        self.lbl_status.SetLabel("剪贴板没有包含有效的数据")




    def on_frame_char_hook(self, event):
        key_code = event.GetKeyCode()
        if key_code == wx.WXK_F4 and event.AltDown() and (not event.ControlDown()):
            self.on_alt_f4_exit(event)
            return
        if (
            key_code == wx.WXK_ESCAPE
            and (not event.AltDown())
            and (not event.ControlDown())
            and (not event.ShiftDown())
        ):
            self.on_esc_minimize_to_tray(event)
            return
        if (
            key_code in (ord('O'), ord('o'))
            and event.ControlDown()
            and (not event.AltDown())
            and (not event.ShiftDown())
        ):
            self.on_browse_app(event)
            return
        if (
            key_code == wx.WXK_F5
            and (not event.ControlDown())
            and (not event.AltDown())
            and (not event.ShiftDown())
        ):
            self.on_f5_refresh(event)
            return
        if (
            key_code in (ord('A'), ord('a'))
            and event.ControlDown()
            and (not event.AltDown())
            and (not event.ShiftDown())
        ):
            self.on_ctrl_a_select(event)
            return
        if (
            key_code in (ord('V'), ord('v'))
            and event.ControlDown()
            and (not event.AltDown())
            and (not event.ShiftDown())
        ):
            self.on_paste_file(event)
            return
        event.Skip()

    def on_f4_settings(self, event):
        self.on_settings(event)

    def on_f5_refresh(self, event):
        self.sandbox_list.on_fast_refresh_sandbox_list(event)

    def on_ctrl_a_select(self, event):
        self.sandbox_list.toggle_select_all()

    def on_hotkey_select_none(self, event):
        if hasattr(self, "sandbox_list") and self.sandbox_list is not None:
            self.sandbox_list.uncheck_all()

    def on_ctrl_h_memory(self, event):
        self.show_app_path_context_menu(event)

    def on_hotkey_launch_selected(self, event):
        self.on_launch(event)

    def on_hotkey_select_prev_user(self, event):
        self._select_adjacent_sandbox(-1)

    def on_hotkey_select_next_user(self, event):
        self._select_adjacent_sandbox(1)

    def on_ctrl_m_mute_all(self, event):
        self.on_mute_all_sandboxes()

    def on_hotkey_volume_up(self, event):
        self.on_adjust_volume_all_sandboxes(5)

    def on_hotkey_volume_down(self, event):
        self.on_adjust_volume_all_sandboxes(-5)

    def on_hotkey_mute_current(self, event):
        sandbox_name = self._get_selected_sandbox_name()
        if not sandbox_name:
            self.lbl_status.SetLabel("请先在用户列表中选中一个用户")
            return
        self.on_mute_current_sandbox(sandbox_name)

    def on_hotkey_volume_current_up(self, event):
        sandbox_name = self._get_selected_sandbox_name()
        if not sandbox_name:
            self.lbl_status.SetLabel("请先在用户列表中选中一个用户")
            return
        self.on_adjust_volume_current_sandbox(sandbox_name, 5)

    def on_hotkey_volume_current_down(self, event):
        sandbox_name = self._get_selected_sandbox_name()
        if not sandbox_name:
            self.lbl_status.SetLabel("请先在用户列表中选中一个用户")
            return
        self.on_adjust_volume_current_sandbox(sandbox_name, -5)

    def on_hotkey_switch_output_current(self, event):
        sandbox_name = self._get_selected_sandbox_name()
        if not sandbox_name:
            self.lbl_status.SetLabel("请先在用户列表中选中一个用户")
            return
        self.on_switch_output_device_current_sandbox(sandbox_name)

    def on_hotkey_switch_output_all(self, event):
        self.on_switch_output_device_all_sandboxes()

    def on_hotkey_close_current_process(self, event):
        sandbox_name = self._get_selected_sandbox_name()
        if not sandbox_name:
            self.lbl_status.SetLabel("请先在用户列表中选中一个用户")
            return
        self.on_close_current_sandbox(sandbox_name)

    def on_hotkey_close_all_processes(self, event):
        self.on_close_all_sandboxes()

    def on_hotkey_locate_current_process(self, event):
        sandbox_name = self._get_selected_sandbox_name()
        if not sandbox_name:
            self.lbl_status.SetLabel("请先在用户列表中选中一个用户")
            return
        self.on_locate_current_sandbox(sandbox_name)

    def on_hotkey_refresh_process_state(self, event):
        self._refresh_process_ui_after_action(refresh_list=False)
        refreshed = False
        try:
            refreshed = bool(self.sandbox_list.refresh_visible_running_states())
        except Exception:
            refreshed = False
        self._manual_process_refresh_status_pending = bool(refreshed)
        self._schedule_process_menu_live_state_refresh(delay_ms=1)
        self.lbl_status.SetLabel("正在刷新进程状态" if refreshed else "没有可刷新的进程状态")

    def on_hotkey_batch_create_sandboxes(self, event):
        self.on_batch_create_sandboxes(event)

    def on_hotkey_batch_rename_sandboxes(self, event):
        self.on_batch_rename_sandboxes(event)

    def on_hotkey_batch_delete_sandboxes(self, event):
        self.on_batch_delete_sandboxes(event)

    def on_hotkey_pin_current_user(self, event):
        sandbox_name = self._get_selected_sandbox_name()
        if not sandbox_name:
            self.lbl_status.SetLabel("请先在用户列表中选中一个用户")
            return
        self.on_toggle_pin_sandbox(sandbox_name)

    def on_hotkey_schedule_launch(self, event):
        self.on_schedule_launch_sandboxes(event)

    def on_hotkey_schedule_close(self, event):
        self.on_schedule_close_sandboxes(event)

    def on_hotkey_cancel_scheduled_tasks(self, event):
        self.on_cancel_all_schedules(event)

    def on_hotkey_create_group(self, event):
        self.on_create_group(event)

    def on_hotkey_launch_group(self, event):
        self.on_launch_group(event)

    def on_hotkey_delete_group(self, event):
        self.on_delete_group(event)

    def on_hotkey_backup_sandboxes(self, event):
        self.on_backup_sandboxes(event)

    def on_hotkey_restore_sandboxes(self, event):
        self.on_restore_sandboxes(event)

    def on_hotkey_manual_correct_sandboxie_path(self, event):
        self.on_manual_correct_sandboxie_path(event)

    def on_hotkey_select_sandbox_root(self, event):
        self.on_select_sandbox_root(event)

    def on_hotkey_speech_settings(self, event):
        self.on_speech_settings(event)

    def on_hotkey_reset_settings(self, event):
        self.on_reset_settings(event)

    def on_hotkey_memory_edit_note(self, event):
        self.on_edit_note(event)

    def on_hotkey_memory_adjust_count(self, event):
        self.on_adjust_count(event)

    def on_hotkey_memory_toggle_path_display(self, event):
        self.on_toggle_path_display(event)

    def on_hotkey_memory_delete_current(self, event):
        self.on_delete_current(event)

    def on_hotkey_memory_clear_all(self, event):
        self.on_clear_memory(event)

    def on_hotkey_memory_copy_path(self, event):
        self.on_copy_path(event)

    def _toggle_boolean_setting(
        self,
        attr_name,
        setting_key,
        enabled_text,
        disabled_text,
        after_apply=None
    ):
        current = parse_bool_setting(getattr(self, attr_name, False), False)
        new_value = not current
        setattr(self, attr_name, new_value)
        self.settings[setting_key] = new_value
        if callable(after_apply):
            try:
                after_apply(new_value)
            except Exception:
                pass
        final_value = parse_bool_setting(getattr(self, attr_name, new_value), new_value)
        setattr(self, attr_name, final_value)
        self.settings[setting_key] = final_value
        self.save_settings()
        self.create_menu_bar()
        self.lbl_status.SetLabel(enabled_text if final_value else disabled_text)

    def on_hotkey_toggle_dynamic_launch_delay(self, event):
        self._toggle_boolean_setting(
            "dynamic_launch_delay_by_load",
            "dynamic_launch_delay_by_load",
            "已开启：批量启动按系统负载动态调整延迟",
            "已关闭：批量启动按系统负载动态调整延迟"
        )

    def on_hotkey_toggle_auto_run(self, event):
        self._toggle_boolean_setting(
            "auto_run",
            "auto_run",
            "已开启：开机自动启动本程序",
            "已关闭：开机自动启动本程序",
            after_apply=lambda _: self._apply_auto_run()
        )

    def on_hotkey_toggle_show_tray(self, event):
        def _apply(value):
            if value:
                self.create_task_bar_icon()
            else:
                self._disable_tray_display()
        self._toggle_boolean_setting(
            "show_tray",
            "show_tray",
            "已开启：在托盘上显示",
            "已关闭：在托盘上显示",
            after_apply=_apply
        )

    def on_hotkey_toggle_auto_select_range(self, event):
        def _apply(value):
            if hasattr(self, "sandbox_list"):
                self.sandbox_list.set_range_auto_check_enabled(value)
        self._toggle_boolean_setting(
            "auto_select_range_on_check",
            "auto_select_range_on_check",
            "已开启：首尾两点自动勾选中间用户",
            "已关闭：首尾两点自动勾选中间用户",
            after_apply=_apply
        )

    def on_hotkey_toggle_skip_running_sandbox_on_launch(self, event):
        self._toggle_boolean_setting(
            "skip_running_sandbox_on_launch",
            "skip_running_sandbox_on_launch",
            "已开启：启动跳过已运行的沙盒",
            "已关闭：启动跳过已运行的沙盒"
        )

    def on_hotkey_toggle_remember_volume(self, event):
        self._toggle_boolean_setting(
            "remember_volume",
            "remember_volume",
            "已开启：记忆进程音量",
            "已关闭：记忆进程音量"
        )

    def on_hotkey_toggle_list_left_right_nav(self, event):
        def _apply(value):
            if hasattr(self, "sandbox_list"):
                self.sandbox_list.set_left_right_navigation_enabled(value)
        self._toggle_boolean_setting(
            "list_use_left_right_nav",
            "list_use_left_right_nav",
            "已开启：用户列表左右光标选择",
            "已关闭：用户列表左右光标选择",
            after_apply=_apply
        )

    def on_hotkey_show_wechat_qr(self, event):
        self.on_show_wechat(event)

    def on_hotkey_show_alipay_qr(self, event):
        self.on_show_alipay(event)

    def on_menu_select_prev_user(self, event):
        self.on_hotkey_select_prev_user(event)

    def on_menu_select_next_user(self, event):
        self.on_hotkey_select_next_user(event)

    def on_menu_mute_current_process(self, event):
        self.on_hotkey_mute_current(event)

    def on_menu_volume_current_up(self, event):
        self.on_hotkey_volume_current_up(event)

    def on_menu_volume_current_down(self, event):
        self.on_hotkey_volume_current_down(event)

    def on_menu_volume_all_up(self, event):
        self.on_hotkey_volume_up(event)

    def on_menu_volume_all_down(self, event):
        self.on_hotkey_volume_down(event)

    def on_menu_switch_output_current(self, event):
        self.on_hotkey_switch_output_current(event)

    def on_menu_switch_output_all(self, event):
        self.on_hotkey_switch_output_all(event)

    def on_menu_close_current_process(self, event):
        self.on_hotkey_close_current_process(event)

    def on_menu_close_all_processes(self, event):
        self.on_hotkey_close_all_processes(event)

    def on_menu_locate_current_process(self, event):
        self.on_hotkey_locate_current_process(event)

    def on_menu_refresh_process_state(self, event):
        self.on_hotkey_refresh_process_state(event)

    def on_menu_launch_selected(self, event):
        self.on_launch(event)

    def on_menu_open_app(self, event):
        self.on_browse_app(event)

    def on_menu_paste_app(self, event):
        self.on_paste_file(event)

    def on_hotkey_toggle_window(self, event):
        if self.IsIconized():
            self.Iconize(False)
            self.Show(True)
            self.Raise()
            self.lbl_status.SetLabel("已显示主窗口")
            self._speak_process_assist("已显示主窗口", event_key="launch_done")
            return
        if self.IsShown():
            moved_to_tray = self._minimize_to_tray_window(
                status_when_tray="已隐藏到托盘",
                status_when_window="当前托盘不可用，已最小化窗口"
            )
            if moved_to_tray:
                self._speak_process_assist("已隐藏窗口", event_key="launch_done")
            else:
                self._speak_process_assist("当前托盘不可用，已最小化窗口", event_key="launch_done")
            return
        self.Show(True)
        self.Raise()
        self.lbl_status.SetLabel("已显示主窗口")
        self._speak_process_assist("已显示主窗口", event_key="launch_done")

    def on_alt_f4_exit(self, event):
        self.force_exit_requested = True
        self.lbl_status.SetLabel("正在退出软件")
        self._speak_process_assist("正在退出软件", event_key="launch_done")
        self.Close()

    def _minimize_to_tray_window(self, status_when_tray="", status_when_window=""):
        if self.show_tray and (self.task_bar_icon is None or self.task_bar_icon.tb_icon is None):
            self.create_task_bar_icon()
        if self.task_bar_icon and self.task_bar_icon.tb_icon:
            self.Hide()
            if status_when_tray:
                self.lbl_status.SetLabel(status_when_tray)
            return True
        self.Iconize(True)
        if status_when_window:
            self.lbl_status.SetLabel(status_when_window)
        return False

    def on_esc_minimize_to_tray(self, event):
        moved_to_tray = self._minimize_to_tray_window(
            status_when_tray="已最小化到托盘",
            status_when_window="当前托盘不可用，已最小化窗口"
        )
        if moved_to_tray:
            self._speak_process_assist("已最小化到托盘", event_key="launch_done")
        else:
            self._speak_process_assist("当前托盘不可用，已最小化窗口", event_key="launch_done")

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

    def _ensure_audio_backend(self):
        global pythoncom, AudioUtilities
        if pythoncom is None:
            try:
                import pythoncom as _pythoncom
                pythoncom = _pythoncom
            except Exception:
                pythoncom = None
        if AudioUtilities is None:
            try:
                from pycaw.pycaw import AudioUtilities as _AudioUtilities
                AudioUtilities = _AudioUtilities
            except Exception:
                AudioUtilities = None
        return AudioUtilities is not None

    def _ensure_audio_router_backend(self):
        global AudioRouter
        if AudioRouter is None:
            try:
                import winappaudiorouter as _audio_router
                AudioRouter = _audio_router
            except Exception:
                AudioRouter = False
        return bool(AudioRouter)

    def _list_output_audio_devices(self):
        if not self._ensure_audio_router_backend():
            return []
        try:
            devices = AudioRouter.list_output_devices()
        except Exception:
            return []
        result = []
        for dev in devices or []:
            try:
                dev_id = str(getattr(dev, "id", "") or "").strip()
                dev_name = str(getattr(dev, "name", "") or "").strip()
                is_default = bool(getattr(dev, "is_default", False))
            except Exception:
                continue
            if not dev_id:
                continue
            label = dev_name or dev_id
            if is_default:
                label = f"{label}（默认）"
            result.append({
                "id": dev_id,
                "name": dev_name or dev_id,
                "label": label,
                "is_default": is_default
            })
        return result

    def _prompt_output_device_choice(self, title):
        devices = self._list_output_audio_devices()
        if not devices:
            self.lbl_status.SetLabel("未找到可用的播放设备")
            return None
        return self._choose_output_device_from_devices(title, devices)

    def _choose_output_device_from_devices(self, title, devices):
        choices = [item["label"] for item in devices]
        dlg = wx.SingleChoiceDialog(self, "请选择播放设备", title, choices)
        try:
            if dlg.ShowModal() != wx.ID_OK:
                return None
            idx = int(dlg.GetSelection())
        finally:
            dlg.Destroy()
        if idx < 0 or idx >= len(devices):
            return None
        return devices[idx]

    def _set_output_device_for_pids(self, pids, device_id):
        if not self._ensure_audio_router_backend():
            return 0, 0
        pid_list = sorted(self._normalize_pid_set(pids))
        if not pid_list:
            return 0, 0
        changed = 0
        failed = 0
        for pid in pid_list:
            try:
                result = AudioRouter.set_app_output_device(process_id=int(pid), device=str(device_id))
                if isinstance(result, dict) and int(pid) in result:
                    changed += 1
                elif result:
                    changed += 1
                else:
                    failed += 1
            except Exception:
                failed += 1
        return changed, failed

    def _get_target_pids_for_sandbox(self, sandbox_name):
        if not sandbox_name:
            return []
        return sorted(set(self._get_sandbox_pids(sandbox_name)))

    def _find_window_for_pids(self, pids):
        pid_set = self._normalize_pid_set(pids)
        if not pid_set:
            return 0
        try:
            user32 = ctypes.windll.user32
        except Exception:
            return 0

        GW_OWNER = 4
        ownerless_candidates = []
        owned_candidates = []
        enum_windows_proc = ctypes.WINFUNCTYPE(ctypes.c_bool, wintypes.HWND, wintypes.LPARAM)

        def _enum_proc(hwnd, lparam):
            try:
                if not user32.IsWindowVisible(hwnd):
                    return True
                owner = user32.GetWindow(hwnd, GW_OWNER)
                pid = wintypes.DWORD()
                user32.GetWindowThreadProcessId(hwnd, ctypes.byref(pid))
                if pid.value not in pid_set:
                    return True
                try:
                    title_len = int(user32.GetWindowTextLengthW(hwnd))
                except Exception:
                    title_len = 0
                item = (hwnd, title_len)
                if owner:
                    owned_candidates.append(item)
                else:
                    ownerless_candidates.append(item)
            except Exception:
                pass
            return True

        cb = enum_windows_proc(_enum_proc)
        try:
            user32.EnumWindows(cb, 0)
        except Exception:
            return 0

        candidates_with_title = [item for item in ownerless_candidates if item[1] > 0]
        if not candidates_with_title:
            candidates_with_title = [item for item in owned_candidates if item[1] > 0]
        if candidates_with_title:
            candidates = [item[0] for item in candidates_with_title]
        elif ownerless_candidates:
            candidates = [item[0] for item in ownerless_candidates]
        elif owned_candidates:
            candidates = [item[0] for item in owned_candidates]
        else:
            return 0

        non_minimized = [hwnd for hwnd in candidates if not user32.IsIconic(hwnd)]
        if non_minimized:
            candidates = non_minimized
        foreground = user32.GetForegroundWindow()
        if foreground in candidates:
            return int(foreground)
        return int(candidates[0])

    def _get_window_title(self, hwnd):
        if not hwnd:
            return ""
        try:
            user32 = ctypes.windll.user32
            title_len = int(user32.GetWindowTextLengthW(hwnd))
            if title_len <= 0:
                return ""
            buffer = ctypes.create_unicode_buffer(title_len + 1)
            user32.GetWindowTextW(hwnd, buffer, len(buffer))
            return str(buffer.value or "").strip()
        except Exception:
            return ""

    def _find_windows_for_pids(self, pids):
        pid_set = self._normalize_pid_set(pids)
        if not pid_set:
            return []
        try:
            user32 = ctypes.windll.user32
        except Exception:
            return []

        GW_OWNER = 4
        ownerless_candidates = []
        owned_candidates = []
        enum_windows_proc = ctypes.WINFUNCTYPE(ctypes.c_bool, wintypes.HWND, wintypes.LPARAM)

        def _enum_proc(hwnd, lparam):
            try:
                if not user32.IsWindowVisible(hwnd):
                    return True
                owner = user32.GetWindow(hwnd, GW_OWNER)
                pid = wintypes.DWORD()
                user32.GetWindowThreadProcessId(hwnd, ctypes.byref(pid))
                if pid.value not in pid_set:
                    return True
                title = self._get_window_title(hwnd)
                item = {
                    "hwnd": int(hwnd),
                    "pid": int(pid.value),
                    "title": title,
                    "owner": bool(owner),
                    "minimized": bool(user32.IsIconic(hwnd)),
                }
                if owner:
                    owned_candidates.append(item)
                else:
                    ownerless_candidates.append(item)
            except Exception:
                pass
            return True

        cb = enum_windows_proc(_enum_proc)
        try:
            user32.EnumWindows(cb, 0)
        except Exception:
            return []

        candidates = [item for item in ownerless_candidates if item.get("title")]
        if not candidates:
            candidates = [item for item in owned_candidates if item.get("title")]
        if not candidates:
            candidates = ownerless_candidates or owned_candidates
        if not candidates:
            return []

        non_minimized = [item for item in candidates if not item.get("minimized")]
        if non_minimized:
            candidates = non_minimized
        foreground = int(user32.GetForegroundWindow() or 0)
        candidates.sort(key=lambda item: (0 if item.get("hwnd") == foreground else 1, item.get("pid", 0), item.get("hwnd", 0)))
        return candidates

    def _prompt_window_choice(self, sandbox_name, windows):
        options = list(windows or [])
        if not options:
            return 0
        if len(options) == 1:
            return int(options[0].get("hwnd") or 0)
        labels = []
        for index, item in enumerate(options, 1):
            title = str(item.get("title") or "").strip() or "<无标题窗口>"
            pid = int(item.get("pid") or 0)
            labels.append(f"{index}. PID {pid} - {title}")
        dlg = wx.SingleChoiceDialog(self, f"当前用户 {sandbox_name} 检测到多个打开进程，请选择要定位的进程", "定位当前打开的进程", labels)
        try:
            if dlg.ShowModal() != wx.ID_OK:
                return 0
            idx = dlg.GetSelection()
            if idx == wx.NOT_FOUND or idx < 0 or idx >= len(options):
                return 0
            return int(options[idx].get("hwnd") or 0)
        finally:
            dlg.Destroy()

    def _activate_window(self, hwnd):
        if not hwnd:
            return False
        try:
            user32 = ctypes.windll.user32
            try:
                kernel32 = ctypes.windll.kernel32
            except Exception:
                kernel32 = None
            SW_RESTORE = 9
            SW_SHOW = 5
            SWP_NOSIZE = 0x0001
            SWP_NOMOVE = 0x0002
            SWP_SHOWWINDOW = 0x0040
            HWND_TOPMOST = -1
            HWND_NOTOPMOST = -2
            VK_MENU = 0x12
            KEYEVENTF_KEYUP = 0x0002
            hwnd = int(hwnd or 0)
            if not hwnd:
                return False
            if hasattr(user32, "IsWindow") and not user32.IsWindow(hwnd):
                return False
            try:
                requires_long_stability = bool(user32.IsIconic(hwnd))
            except Exception:
                requires_long_stability = False

            def _get_foreground_hwnd():
                try:
                    return int(user32.GetForegroundWindow() or 0)
                except Exception:
                    return 0

            def _is_foreground_target(stable_checks, interval_seconds=0.1):
                checks = max(1, int(stable_checks or 1))
                for index in range(checks):
                    if _get_foreground_hwnd() != hwnd:
                        return False
                    if index + 1 < checks:
                        time.sleep(max(0.0, float(interval_seconds or 0.0)))
                return True

            def _prime_foreground_permission():
                try:
                    user32.keybd_event(VK_MENU, 0, 0, 0)
                    user32.keybd_event(VK_MENU, 0, KEYEVENTF_KEYUP, 0)
                except Exception:
                    pass

            def _bring_to_front(use_alt_prime=False):
                try:
                    was_iconic = bool(user32.IsIconic(hwnd))
                except Exception:
                    was_iconic = False
                try:
                    if was_iconic:
                        user32.ShowWindow(hwnd, SW_RESTORE)
                        time.sleep(0.25)
                    else:
                        user32.ShowWindow(hwnd, SW_SHOW)
                except Exception:
                    pass
                if use_alt_prime:
                    _prime_foreground_permission()
                attached_pairs = []
                foreground_hwnd = _get_foreground_hwnd()
                try:
                    target_thread = int(user32.GetWindowThreadProcessId(hwnd, None) or 0)
                except Exception:
                    target_thread = 0
                try:
                    foreground_thread = int(user32.GetWindowThreadProcessId(foreground_hwnd, None) or 0) if foreground_hwnd else 0
                except Exception:
                    foreground_thread = 0
                try:
                    current_thread = int(kernel32.GetCurrentThreadId() or 0) if kernel32 else 0
                except Exception:
                    current_thread = 0
                try:
                    attach_thread_input = getattr(user32, "AttachThreadInput", None)
                    if callable(attach_thread_input):
                        for pair in (
                            (foreground_thread, target_thread),
                            (foreground_thread, current_thread),
                            (target_thread, current_thread),
                        ):
                            src, dst = pair
                            if src and dst and src != dst and pair not in attached_pairs:
                                try:
                                    attach_thread_input(src, dst, True)
                                    attached_pairs.append(pair)
                                except Exception:
                                    pass
                except Exception:
                    attached_pairs = []
                try:
                    user32.BringWindowToTop(hwnd)
                    user32.SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW)
                    user32.SetWindowPos(hwnd, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW)
                    if hasattr(user32, "SetActiveWindow"):
                        user32.SetActiveWindow(hwnd)
                    if hasattr(user32, "SetFocus"):
                        user32.SetFocus(hwnd)
                    user32.SetForegroundWindow(hwnd)
                except Exception:
                    pass
                finally:
                    if attached_pairs:
                        detach_thread_input = getattr(user32, "AttachThreadInput", None)
                        if callable(detach_thread_input):
                            for src, dst in reversed(attached_pairs):
                                try:
                                    detach_thread_input(src, dst, False)
                                except Exception:
                                    pass

            if not requires_long_stability and _is_foreground_target(1, interval_seconds=0.0):
                return True

            stable_checks = 2
            deadline = time.time() + 5.0
            attempt_index = 0
            while time.time() < deadline:
                _bring_to_front(use_alt_prime=attempt_index > 0)
                time.sleep(0.1)
                if _is_foreground_target(stable_checks):
                    return True
                attempt_index += 1
            return False
        except Exception:
            return False

    def _stabilize_activated_window(self, hwnd):
        target_hwnd = int(hwnd or 0)
        if not target_hwnd or self._is_window_closing():
            return False
        for _ in range(6):
            if self._is_window_closing():
                return False
            try:
                if self._activate_window(target_hwnd):
                    if self._has_foreground_window_streak(target_hwnd, required_streak=2, total_checks=4, interval_seconds=0.08):
                        time.sleep(0.1)
                        if self._has_foreground_window_streak(target_hwnd, required_streak=2, total_checks=3, interval_seconds=0.08):
                            return True
                elif self._has_foreground_window_streak(target_hwnd, required_streak=2, total_checks=3, interval_seconds=0.08):
                    time.sleep(0.12)
                    if self._has_foreground_window_streak(target_hwnd, required_streak=2, total_checks=3, interval_seconds=0.08):
                        return True
            except Exception:
                return False
            time.sleep(0.14)
        return False

    def _start_locate_window_reinforcement(self, hwnd, keep_long_tail=False):
        target_hwnd = int(hwnd or 0)
        if not target_hwnd or self._is_window_closing():
            return
        self._cancel_locate_window_reinforcement(wait=False)
        stop_event = threading.Event()

        def _worker():
            start_time = time.monotonic()
            try:
                offsets = [0.08, 0.22, 0.42, 0.72]
                if keep_long_tail:
                    offsets.extend((1.12, 1.52, 1.92, 2.32))
                for target_offset in offsets:
                    remaining = max(0.0, float(target_offset) - (time.monotonic() - start_time))
                    if stop_event.wait(remaining):
                        return
                    if self._is_window_closing() or stop_event.is_set():
                        return
                    try:
                        user32 = ctypes.windll.user32
                        if hasattr(user32, "IsWindow") and not user32.IsWindow(target_hwnd):
                            return
                    except Exception:
                        return
                    try:
                        self._activate_window(target_hwnd)
                    except Exception:
                        return
            finally:
                try:
                    with self._locate_reinforcement_lock:
                        if getattr(self, "_locate_reinforcement_thread", None) is threading.current_thread():
                            self._locate_reinforcement_thread = None
                            self._locate_reinforcement_event = None
                except Exception:
                    pass

        thread = threading.Thread(target=_worker, name="LocateWindowReinforcement", daemon=True)
        try:
            with self._locate_reinforcement_lock:
                self._locate_reinforcement_event = stop_event
                self._locate_reinforcement_thread = thread
        except Exception:
            return
        thread.start()

    def _has_foreground_window_streak(self, hwnd, required_streak=3, total_checks=8, interval_seconds=0.1):
        target_hwnd = int(hwnd or 0)
        if not target_hwnd:
            return False
        try:
            user32 = ctypes.windll.user32
        except Exception:
            return False
        needed = max(1, int(required_streak or 1))
        checks = max(needed, int(total_checks or needed))
        streak = 0
        for index in range(checks):
            try:
                current_foreground = int(user32.GetForegroundWindow() or 0)
            except Exception:
                current_foreground = 0
            if current_foreground == target_hwnd:
                streak += 1
                if streak >= needed:
                    return True
            else:
                streak = 0
            if index + 1 < checks:
                time.sleep(max(0.0, float(interval_seconds or 0.0)))
        return False

    def _move_focus_off_target_pids(self, pids):
        pid_set = self._normalize_pid_set(pids)
        if not pid_set:
            return False
        try:
            user32 = ctypes.windll.user32
        except Exception:
            return False

        def _get_foreground_info():
            try:
                foreground_hwnd = int(user32.GetForegroundWindow() or 0)
            except Exception:
                foreground_hwnd = 0
            foreground_pid = 0
            if foreground_hwnd:
                try:
                    pid = wintypes.DWORD()
                    user32.GetWindowThreadProcessId(foreground_hwnd, ctypes.byref(pid))
                    foreground_pid = int(pid.value or 0)
                except Exception:
                    foreground_pid = 0
            return foreground_hwnd, foreground_pid

        foreground_hwnd, foreground_pid = _get_foreground_info()
        if foreground_pid not in pid_set:
            return False

        minimize_codes = (6, 11)
        minimized_hwnds = set()
        deadline = time.time() + 1.5
        while time.time() < deadline:
            current_hwnd, current_pid = _get_foreground_info()
            if current_pid not in pid_set:
                return True

            candidate_hwnds = []
            if current_hwnd:
                candidate_hwnds.append(current_hwnd)
            try:
                for item in self._find_windows_for_pids(pid_set):
                    hwnd = int(item.get("hwnd") or 0)
                    if hwnd and hwnd not in candidate_hwnds:
                        candidate_hwnds.append(hwnd)
            except Exception:
                pass

            moved_any = False
            for hwnd in candidate_hwnds:
                if not hwnd or hwnd in minimized_hwnds:
                    continue
                minimized_hwnds.add(hwnd)
                for show_code in minimize_codes:
                    try:
                        user32.ShowWindow(hwnd, show_code)
                    except Exception:
                        pass
                moved_any = True
                time.sleep(0.12)
                current_hwnd, current_pid = _get_foreground_info()
                if current_pid not in pid_set:
                    return True
            if not moved_any:
                break
        return False

    def _get_target_pids_for_all(self):
        all_pids_fast = self._get_all_sandbox_pids()
        if all_pids_fast:
            return sorted(set(all_pids_fast))

        sandbox_names = []
        try:
            sandbox_names = self.sandbox_list.scan_sandboxes()
        except Exception:
            sandbox_names = []

        all_pids = set()
        for sandbox_name in sandbox_names:
            all_pids.update(self._get_sandbox_pids(sandbox_name))
        return sorted(all_pids)

    def _normalize_pid_set(self, pids):
        pid_set = set()
        for pid in pids or []:
            try:
                value = int(pid)
            except Exception:
                continue
            if value > 0:
                pid_set.add(value)
        return pid_set

    def _get_mute_state_for_pids(self, pids):
        pid_set = self._normalize_pid_set(pids)
        if not pid_set or not self._ensure_audio_backend():
            return None
        com_initialized = False
        try:
            if pythoncom is not None:
                pythoncom.CoInitialize()
                com_initialized = True
            states = []
            for session in AudioUtilities.GetAllSessions():
                try:
                    proc = getattr(session, "Process", None)
                    pid = getattr(proc, "pid", None)
                    if pid not in pid_set:
                        continue
                    volume = getattr(session, "SimpleAudioVolume", None)
                    if volume is None:
                        continue
                    states.append(bool(volume.GetMute()))
                except Exception:
                    continue
            if not states:
                return None
            return all(states)
        except Exception:
            return None
        finally:
            if com_initialized:
                try:
                    pythoncom.CoUninitialize()
                except Exception:
                    pass

    def _set_mute_for_pids(self, pids, mute):
        result = self._set_mute_for_pids_detailed(pids, mute)
        return int(result.get("changed_sessions", 0) or 0)

    def _set_mute_for_pids_detailed(self, pids, mute):
        pid_set = self._normalize_pid_set(pids)
        if not pid_set or not self._ensure_audio_backend():
            return {
                "changed_sessions": 0,
                "changed_pids": set(),
                "matched_pids": set(),
                "failed_pids": set(),
            }
        changed_sessions = 0
        changed_pids = set()
        matched_pids = set()
        failed_pids = set()
        com_initialized = False
        try:
            if pythoncom is not None:
                pythoncom.CoInitialize()
                com_initialized = True
            for session in AudioUtilities.GetAllSessions():
                try:
                    proc = getattr(session, "Process", None)
                    pid = getattr(proc, "pid", None)
                    if pid not in pid_set:
                        continue
                    volume = getattr(session, "SimpleAudioVolume", None)
                    if volume is None:
                        continue
                    matched_pids.add(int(pid))
                    volume.SetMute(1 if mute else 0, None)
                    changed_sessions += 1
                    changed_pids.add(int(pid))
                except Exception:
                    try:
                        if pid in pid_set:
                            failed_pids.add(int(pid))
                    except Exception:
                        pass
                    continue
        except Exception:
            return {
                "changed_sessions": int(changed_sessions),
                "changed_pids": set(changed_pids),
                "matched_pids": set(matched_pids),
                "failed_pids": set(failed_pids),
            }
        finally:
            if com_initialized:
                try:
                    pythoncom.CoUninitialize()
                except Exception:
                    pass
        return {
            "changed_sessions": int(changed_sessions),
            "changed_pids": set(changed_pids),
            "matched_pids": set(matched_pids),
            "failed_pids": set(failed_pids),
        }

    def _adjust_volume_for_pids(self, pids, delta):
        result = self._adjust_volume_for_pids_detailed(pids, delta)
        return int(result.get("changed_sessions", 0) or 0), result.get("avg_level")

    def _adjust_volume_for_pids_detailed(self, pids, delta):
        pid_set = self._normalize_pid_set(pids)
        if not pid_set or not self._ensure_audio_backend():
            return {
                "changed_sessions": 0,
                "avg_level": None,
                "changed_pids": set(),
                "matched_pids": set(),
                "failed_pids": set(),
            }

        try:
            delta = float(delta)
        except Exception:
            return {
                "changed_sessions": 0,
                "avg_level": None,
                "changed_pids": set(),
                "matched_pids": set(),
                "failed_pids": set(),
            }
        if delta == 0:
            return {
                "changed_sessions": 0,
                "avg_level": None,
                "changed_pids": set(),
                "matched_pids": set(),
                "failed_pids": set(),
            }

        changed = 0
        level_values = []
        changed_pids = set()
        matched_pids = set()
        failed_pids = set()
        com_initialized = False
        try:
            if pythoncom is not None:
                pythoncom.CoInitialize()
                com_initialized = True
            for session in AudioUtilities.GetAllSessions():
                try:
                    proc = getattr(session, "Process", None)
                    pid = getattr(proc, "pid", None)
                    if pid not in pid_set:
                        continue
                    volume = getattr(session, "SimpleAudioVolume", None)
                    if volume is None:
                        continue
                    matched_pids.add(int(pid))
                    current = float(volume.GetMasterVolume())
                    target = max(0.0, min(1.0, current + delta))
                    volume.SetMasterVolume(target, None)
                    level_values.append(target)
                    changed += 1
                    changed_pids.add(int(pid))
                except Exception:
                    try:
                        if pid in pid_set:
                            failed_pids.add(int(pid))
                    except Exception:
                        pass
                    continue
        except Exception:
            return {
                "changed_sessions": int(changed),
                "avg_level": (int(round((sum(level_values) / len(level_values)) * 100)) if level_values else None),
                "changed_pids": set(changed_pids),
                "matched_pids": set(matched_pids),
                "failed_pids": set(failed_pids),
            }
        finally:
            if com_initialized:
                try:
                    pythoncom.CoUninitialize()
                except Exception:
                    pass

        if not level_values:
            return {
                "changed_sessions": 0,
                "avg_level": None,
                "changed_pids": set(changed_pids),
                "matched_pids": set(matched_pids),
                "failed_pids": set(failed_pids),
            }
        avg_level = int(round((sum(level_values) / len(level_values)) * 100))
        return {
            "changed_sessions": int(changed),
            "avg_level": avg_level,
            "changed_pids": set(changed_pids),
            "matched_pids": set(matched_pids),
            "failed_pids": set(failed_pids),
        }

    def _set_volume_for_pids(self, pids, level_percent):
        pid_set = self._normalize_pid_set(pids)
        if not pid_set or not self._ensure_audio_backend():
            return 0, None
        try:
            target = max(0.0, min(1.0, float(level_percent) / 100.0))
        except Exception:
            return 0, None

        changed = 0
        level_values = []
        com_initialized = False
        try:
            if pythoncom is not None:
                pythoncom.CoInitialize()
                com_initialized = True
            for session in AudioUtilities.GetAllSessions():
                try:
                    proc = getattr(session, "Process", None)
                    pid = getattr(proc, "pid", None)
                    if pid not in pid_set:
                        continue
                    volume = getattr(session, "SimpleAudioVolume", None)
                    if volume is None:
                        continue
                    volume.SetMasterVolume(target, None)
                    level_values.append(target)
                    changed += 1
                except Exception:
                    continue
        except Exception:
            return 0, None
        finally:
            if com_initialized:
                try:
                    pythoncom.CoUninitialize()
                except Exception:
                    pass

        if not level_values:
            return 0, None
        avg_level = int(round((sum(level_values) / len(level_values)) * 100))
        return changed, avg_level

    def _remember_volume_for_sandbox(self, sandbox_name, level_percent):
        if not getattr(self, "remember_volume", True):
            return False
        name = self._get_volume_memory_key(sandbox_name)
        if not name:
            return False
        try:
            level = int(round(float(level_percent)))
        except Exception:
            return False
        level = max(0, min(100, level))
        if self.sandbox_volume_memory.get(name) == level:
            return False
        self.sandbox_volume_memory[name] = level
        return True

    def _remember_volume_for_all_running_sandboxes(self, level_percent):
        if not getattr(self, "remember_volume", True):
            return
        changed = False
        sandbox_names = []
        try:
            sandbox_names = self.sandbox_list.scan_sandboxes()
        except Exception:
            sandbox_names = []
        for sandbox_name in sandbox_names:
            pids = self._get_target_pids_for_sandbox(sandbox_name)
            if not pids:
                continue
            if self._remember_volume_for_sandbox(sandbox_name, level_percent):
                changed = True
        if changed:
            self.save_settings()

    def _restore_saved_volume_for_sandbox(self, sandbox_name):
        if not getattr(self, "remember_volume", True):
            return
        memory_key = self._get_volume_memory_key(sandbox_name)
        target_level = self.sandbox_volume_memory.get(memory_key)
        if target_level is None:
            return
        deadline = time.time() + 8.0
        while time.time() < deadline:
            pids = self._get_target_pids_for_sandbox(sandbox_name)
            if pids:
                changed, _ = self._set_volume_for_pids(pids, target_level)
                if changed > 0:
                    return
            time.sleep(0.45)

    def _has_volume_session_for_pids(self, pids):
        pid_set = self._normalize_pid_set(pids)
        if not pid_set or not self._ensure_audio_backend():
            return False
        com_initialized = False
        try:
            if pythoncom is not None:
                pythoncom.CoInitialize()
                com_initialized = True
            for session in AudioUtilities.GetAllSessions():
                try:
                    proc = getattr(session, "Process", None)
                    pid = getattr(proc, "pid", None)
                    if pid not in pid_set:
                        continue
                    if getattr(session, "SimpleAudioVolume", None) is not None:
                        return True
                except Exception:
                    continue
            return False
        except Exception:
            return False
        finally:
            if com_initialized:
                try:
                    pythoncom.CoUninitialize()
                except Exception:
                    pass

    def _terminate_pids(self, pids):
        pid_list = sorted(self._normalize_pid_set(pids))
        if not pid_list:
            return 0

        def _pid_exists(pid):
            try:
                os.kill(pid, 0)
                return True
            except PermissionError:
                return True
            except OSError:
                return False

        alive_before = {pid for pid in pid_list if _pid_exists(pid)}
        if not alive_before:
            return 0

        chunk_size = 60
        for i in range(0, len(pid_list), chunk_size):
            chunk = pid_list[i:i + chunk_size]
            cmd = ["taskkill", "/T", "/F"]
            for pid in chunk:
                cmd.extend(["/PID", str(pid)])
            try:
                subprocess.run(
                    cmd,
                    stdout=subprocess.DEVNULL,
                    stderr=subprocess.DEVNULL,
                    timeout=8
                )
            except Exception:
                continue

        time.sleep(0.12)
        alive_after = {pid for pid in alive_before if _pid_exists(pid)}
        return len(alive_before - alive_after)

    def _get_alive_pids(self, pids):
        pid_list = sorted(self._normalize_pid_set(pids))
        if not pid_list:
            return []

        alive = []
        for pid in pid_list:
            try:
                os.kill(pid, 0)
                alive.append(pid)
            except PermissionError:
                alive.append(pid)
            except OSError:
                continue
        return alive

    def _wait_for_pids_exit(self, pids, timeout=1.2, interval=0.12):
        alive = self._get_alive_pids(pids)
        if not alive:
            return []
        deadline = time.time() + max(0.0, float(timeout))
        sleep_interval = max(0.05, float(interval))
        while alive and time.time() < deadline:
            time.sleep(sleep_interval)
            alive = self._get_alive_pids(alive)
        return alive

    def _terminate_sandbox_via_start(self, sandbox_name):
        sandbox_name = self._normalize_sandbox_name(sandbox_name)
        if not sandbox_name:
            return False
        if not is_valid_internal_sandbox_name(sandbox_name):
            return False
        try:
            official_names = self._get_official_sandbox_name_set()
        except Exception:
            official_names = set()
        if official_names and sandbox_name.lower() not in official_names:
            return False
        result, _ = self._run_sandboxie_start([f"/box:{sandbox_name}", "/terminate"], timeout=8)
        if result is None:
            return False
        return result.returncode == 0

    def _terminate_all_via_start(self):
        result, _ = self._run_sandboxie_start(["/terminate_all"], timeout=8)
        if result is None:
            return False
        return result.returncode == 0

    def get_close_menu_state_for_all(self):
        return True if self._get_sandboxie_start_exe() else None

    def get_close_menu_state_for_sandbox(self, sandbox_name):
        return True if sandbox_name and self._get_sandboxie_start_exe() else None

    def get_volume_menu_state_for_sandbox(self, sandbox_name):
        pids = self._get_target_pids_for_sandbox(sandbox_name)
        return True if self._has_volume_session_for_pids(pids) else None

    def get_volume_menu_state_for_all(self):
        pids = self._get_target_pids_for_all()
        return True if self._has_volume_session_for_pids(pids) else None

    def get_cached_mute_state_for_sandbox(self, sandbox_name):
        """Return the real current mute state for the selected sandbox.

        The menu used to trust _cached_mute_state_by_sandbox only.  That could
        make user B show "unmute current process" after user A had been muted,
        even when B's current process was not muted.  For the current item we
        verify the audio session now, then refresh the small cache.
        """
        name = self._normalize_sandbox_name(sandbox_name)
        if not name:
            return None
        state = self.get_mute_menu_state_for_sandbox(name)
        if state is None:
            self._cached_mute_state_by_sandbox.pop(name, None)
        else:
            self._cached_mute_state_by_sandbox[name] = bool(state)
        return state

    def get_cached_mute_state_for_all(self):
        return self._cached_mute_state_all

    def get_menu_cached_mute_state_for_sandbox(self, sandbox_name):
        name = self._normalize_sandbox_name(sandbox_name)
        if not name:
            return None
        return self._cached_mute_state_by_sandbox.get(name)

    def get_mute_menu_state_for_sandbox(self, sandbox_name):
        return self._get_mute_state_for_pids(self._get_target_pids_for_sandbox(sandbox_name))

    def get_locate_menu_state_for_sandbox(self, sandbox_name):
        pids = self._get_target_pids_for_sandbox(sandbox_name)
        if not pids:
            return None
        return True if self._find_window_for_pids(pids) else None

    def get_mute_menu_state_for_all(self):
        return self._get_mute_state_for_pids(self._get_target_pids_for_all())

    def _refresh_process_ui_after_action(self, refresh_list=False):
        try:
            self._invalidate_process_menu_state_cache()
            self._invalidate_process_menu_live_state_cache(clear_values=False)
        except Exception:
            pass
        if refresh_list and hasattr(self, "sandbox_list"):
            current_status = ""
            try:
                current_status = self.lbl_status.GetLabel()
            except Exception:
                current_status = ""
            self._suppress_selection_status(8.0)
            try:
                self.sandbox_list.on_refresh_sandbox_list(None)
            except Exception:
                pass
            if current_status:
                try:
                    self.lbl_status.SetLabel(current_status)
                except Exception:
                    pass
        self._schedule_process_menu_live_state_refresh(delay_ms=1)
        self._schedule_menu_bar_rebuild(delay_ms=24)

    def _refresh_visible_sandbox_running_states(self):
        try:
            if hasattr(self, "sandbox_list"):
                return bool(self.sandbox_list.refresh_visible_running_states())
        except Exception:
            pass
        return False

    def on_mute_current_sandbox(self, sandbox_name):
        sandbox_name, display_name = self._get_sandbox_internal_and_display_name(sandbox_name)

        def _worker():
            pids = self._get_target_pids_for_sandbox(sandbox_name)
            if not pids:
                self._safe_call_after(self._finish_mute_current_sandbox, sandbox_name, display_name, None, None, "no_target")
                return
            state = self._get_mute_state_for_pids(pids)
            target_mute = False if state is True else True
            try:
                mute_result = self._set_mute_for_pids_detailed(pids, target_mute)
            except Exception:
                self._safe_call_after(self._finish_mute_current_sandbox, sandbox_name, display_name, target_mute, None, "failed")
                return
            self._safe_call_after(self._finish_mute_current_sandbox, sandbox_name, display_name, target_mute, mute_result, "")

        self._run_volume_task_async(f"正在切换静音状态: {display_name}", _worker)

    def _finish_mute_current_sandbox(self, sandbox_name, display_name, target_mute, mute_result, failure_reason=""):
        if failure_reason == "no_target":
            self._cached_mute_state_by_sandbox.pop(sandbox_name, None)
            self.lbl_status.SetLabel(
                self._with_assist_hint("mute_current", f"当前用户 {display_name} 没有可静音的已开进程")
            )
            self._refresh_process_ui_after_action(refresh_list=False)
            return
        if failure_reason == "failed" or not isinstance(mute_result, dict):
            self._cached_mute_state_by_sandbox.pop(sandbox_name, None)
            self._cached_mute_state_all = None
            action = "取消静音" if not target_mute else "静音"
            self.lbl_status.SetLabel(
                self._with_assist_hint("mute_current", f"{action}失败: {display_name}")
            )
            self._refresh_process_ui_after_action(refresh_list=False)
            return
        changed = int(mute_result.get("changed_sessions", 0) or 0)
        changed_pids = set(mute_result.get("changed_pids", set()) or set())
        matched_pids = set(mute_result.get("matched_pids", set()) or set())
        failed_pids = set(mute_result.get("failed_pids", set()) or set())
        fully_changed = bool(matched_pids) and changed_pids == matched_pids and not failed_pids
        if changed > 0 and fully_changed:
            self._cached_mute_state_by_sandbox[sandbox_name] = target_mute
            self._cached_mute_state_all = None
            self.lbl_status.SetLabel(
                self._with_assist_hint("mute_current", f"{'已取消静音' if not target_mute else '已静音'}: {display_name}")
            )
            self._speak_process_assist(
                f"{'取消静音' if not target_mute else '静音'}{display_name}进程",
                event_key="mute_current"
            )
        elif changed > 0:
            self._cached_mute_state_by_sandbox.pop(sandbox_name, None)
            self._cached_mute_state_all = None
            self.lbl_status.SetLabel(
                self._with_assist_hint(
                    "mute_current",
                    f"{'已部分取消静音' if not target_mute else '已部分静音'}: {display_name}"
                )
            )
            self._speak_process_assist(
                f"{'部分取消静音' if not target_mute else '部分静音'}{display_name}进程",
                event_key="mute_current"
            )
        elif failed_pids:
            self._cached_mute_state_by_sandbox.pop(sandbox_name, None)
            self._cached_mute_state_all = None
            self.lbl_status.SetLabel(
                self._with_assist_hint(
                    "mute_current",
                    f"{'取消静音' if not target_mute else '静音'}失败: {display_name}"
                )
            )
        else:
            self._cached_mute_state_by_sandbox.pop(sandbox_name, None)
            self.lbl_status.SetLabel(
                self._with_assist_hint("mute_current", f"未找到可静音的进程: {display_name}")
            )
        self._refresh_process_ui_after_action(refresh_list=False)

    def on_mute_all_sandboxes(self):
        def _worker():
            pids = self._get_target_pids_for_all()
            if not pids:
                self._safe_call_after(self._finish_mute_all_sandboxes, None, None, "no_target")
                return
            state = self._get_mute_state_for_pids(pids)
            target_mute = False if state is True else True
            try:
                mute_result = self._set_mute_for_pids_detailed(pids, target_mute)
            except Exception:
                self._safe_call_after(self._finish_mute_all_sandboxes, target_mute, None, "failed")
                return
            self._safe_call_after(self._finish_mute_all_sandboxes, target_mute, mute_result, "")

        self._run_volume_task_async("正在切换全部打开进程静音状态...", _worker)

    def _finish_mute_all_sandboxes(self, target_mute, mute_result, failure_reason=""):
        if failure_reason == "no_target":
            self._cached_mute_state_all = None
            self.lbl_status.SetLabel(
                self._with_assist_hint("mute_all", "当前没有可静音的全部打开进程")
            )
            self._refresh_process_ui_after_action(refresh_list=False)
            return
        if failure_reason == "failed" or not isinstance(mute_result, dict):
            self._cached_mute_state_all = None
            self._cached_mute_state_by_sandbox.clear()
            action = "取消静音" if not target_mute else "静音"
            self.lbl_status.SetLabel(
                self._with_assist_hint("mute_all", f"{action}全部打开的进程失败")
            )
            self._refresh_process_ui_after_action(refresh_list=False)
            return
        changed = int(mute_result.get("changed_sessions", 0) or 0)
        changed_pids = set(mute_result.get("changed_pids", set()) or set())
        matched_pids = set(mute_result.get("matched_pids", set()) or set())
        failed_pids = set(mute_result.get("failed_pids", set()) or set())
        fully_changed = bool(matched_pids) and changed_pids == matched_pids and not failed_pids
        if changed > 0 and fully_changed:
            self._cached_mute_state_all = target_mute
            if hasattr(self, "sandbox_list") and self.sandbox_list is not None:
                try:
                    for i in range(self.sandbox_list.GetItemCount()):
                        if self.sandbox_list._is_special_index(i):
                            continue
                        sandbox_name = self.sandbox_list.get_item_identity_text(i)
                        if not sandbox_name:
                            continue
                        self._cached_mute_state_by_sandbox[sandbox_name] = target_mute
                except Exception:
                    pass
            self.lbl_status.SetLabel(
                self._with_assist_hint("mute_all", f"{'已取消静音' if not target_mute else '已静音'}全部打开的进程")
            )
            self._speak_process_assist(
                "全部取消静音" if not target_mute else "全部静音",
                event_key="mute_all"
            )
        elif changed > 0:
            self._cached_mute_state_all = None
            self._cached_mute_state_by_sandbox.clear()
            self.lbl_status.SetLabel(
                self._with_assist_hint(
                    "mute_all",
                    f"{'已部分取消静音' if not target_mute else '已部分静音'}全部打开的进程"
                )
            )
            self._speak_process_assist(
                "全部进程部分取消静音" if not target_mute else "全部进程部分静音",
                event_key="mute_all"
            )
        elif failed_pids:
            self._cached_mute_state_all = None
            self._cached_mute_state_by_sandbox.clear()
            self.lbl_status.SetLabel(
                self._with_assist_hint(
                    "mute_all",
                    f"{'取消静音' if not target_mute else '静音'}全部打开的进程失败"
                )
            )
        else:
            self._cached_mute_state_all = None
            self.lbl_status.SetLabel(
                self._with_assist_hint("mute_all", "未找到可静音的全部打开进程")
            )
        self._refresh_process_ui_after_action(refresh_list=False)

    def on_switch_output_device_current_sandbox(self, sandbox_name):
        sandbox_name, display_name = self._get_sandbox_internal_and_display_name(sandbox_name)
        pids = self._get_target_pids_for_sandbox(sandbox_name)
        if not pids:
            self.lbl_status.SetLabel(f"当前用户 {display_name} 没有可切换设备的已开进程")
            return
        devices = self._list_output_audio_devices()
        if not devices:
            self.lbl_status.SetLabel("未找到可用的播放设备")
            return
        device = self._choose_output_device_from_devices("切换当前打开的进程播放设备", devices)
        if not device:
            return

        def _worker():
            try:
                changed, failed = self._set_output_device_for_pids(pids, device["id"])
            except Exception:
                self._safe_call_after(self._finish_switch_output_device_current_sandbox, display_name, device, 0, 1, True)
                return
            self._safe_call_after(self._finish_switch_output_device_current_sandbox, display_name, device, changed, failed, False)

        self._run_volume_task_async(f"正在切换当前打开进程播放设备: {display_name}", _worker)

    def _finish_switch_output_device_current_sandbox(self, display_name, device, changed, failed, exception_failed=False):
        if exception_failed:
            self.lbl_status.SetLabel(f"切换播放设备失败: {display_name}")
            self._refresh_process_ui_after_action(refresh_list=False)
            return
        if changed > 0 and failed <= 0:
            self.lbl_status.SetLabel(f"已切换当前打开进程播放设备: {display_name} -> {device['name']}")
            self._speak_process_assist(
                f"把{display_name}进程切换到播放设备{device['name']}",
                event_key="switch_output_current"
            )
        elif changed > 0:
            self.lbl_status.SetLabel(
                f"已部分切换当前打开进程播放设备: {display_name} -> {device['name']}（成功 {changed}，失败 {failed}）"
            )
            self._speak_process_assist(
                f"部分切换{display_name}进程到播放设备{device['name']}",
                event_key="switch_output_current"
            )
        elif failed > 0:
            self.lbl_status.SetLabel(f"切换播放设备失败: {display_name}")
        else:
            self.lbl_status.SetLabel(f"未找到可切换设备的进程: {display_name}")
        self._refresh_process_ui_after_action(refresh_list=False)

    def on_switch_output_device_all_sandboxes(self):
        pids = self._get_target_pids_for_all()
        if not pids:
            self.lbl_status.SetLabel("当前没有可切换设备的全部打开进程")
            return
        devices = self._list_output_audio_devices()
        if not devices:
            self.lbl_status.SetLabel("未找到可用的播放设备")
            return
        device = self._choose_output_device_from_devices("切换全部打开的进程播放设备", devices)
        if not device:
            return

        def _worker():
            try:
                changed, failed = self._set_output_device_for_pids(pids, device["id"])
            except Exception:
                self._safe_call_after(self._finish_switch_output_device_all_sandboxes, device, 0, 1, True)
                return
            self._safe_call_after(self._finish_switch_output_device_all_sandboxes, device, changed, failed, False)

        self._run_volume_task_async("正在切换全部打开进程播放设备...", _worker)

    def _finish_switch_output_device_all_sandboxes(self, device, changed, failed, exception_failed=False):
        if exception_failed:
            self.lbl_status.SetLabel("切换全部打开进程播放设备失败")
            self._refresh_process_ui_after_action(refresh_list=False)
            return
        if changed > 0 and failed <= 0:
            self.lbl_status.SetLabel(f"已切换全部打开进程播放设备 -> {device['name']}")
            self._speak_process_assist(
                f"把全部进程切换到播放设备{device['name']}",
                event_key="switch_output_all"
            )
        elif changed > 0:
            self.lbl_status.SetLabel(
                f"已部分切换全部打开进程播放设备 -> {device['name']}（成功 {changed}，失败 {failed}）"
            )
            self._speak_process_assist(
                f"部分进程已切换到播放设备{device['name']}",
                event_key="switch_output_all"
            )
        elif failed > 0:
            self.lbl_status.SetLabel("切换全部打开进程播放设备失败")
        else:
            self.lbl_status.SetLabel("未找到可切换设备的全部打开进程")
        self._refresh_process_ui_after_action(refresh_list=False)

    def _run_volume_task_async(self, start_status, worker):
        if not self._volume_task_lock.acquire(blocking=False):
            self.lbl_status.SetLabel("正在执行音频操作，请稍候")
            return
        self.lbl_status.SetLabel(str(start_status or "正在执行音频操作..."))

        def _task():
            status_text = ""
            try:
                result = worker()
                if isinstance(result, str) and result.strip():
                    status_text = result.strip()
            except Exception as e:
                status_text = f"音频操作失败: {str(e)}"
            finally:
                self._volume_task_lock.release()
            if status_text:
                self._safe_call_after(self.lbl_status.SetLabel, status_text)
            self._safe_call_after(self.create_menu_bar)

        threading.Thread(target=_task, daemon=True).start()

    def on_adjust_volume_all_sandboxes(self, step_percent):
        step = int(step_percent)

        def _worker():
            pids = self._get_target_pids_for_all()
            if not pids:
                return "当前没有可调节音量的全部打开进程"
            volume_result = self._adjust_volume_for_pids_detailed(pids, step / 100.0)
            changed = int(volume_result.get("changed_sessions", 0) or 0)
            avg_level = volume_result.get("avg_level")
            changed_pids = set(volume_result.get("changed_pids", set()) or set())
            matched_pids = set(volume_result.get("matched_pids", set()) or set())
            failed_pids = set(volume_result.get("failed_pids", set()) or set())
            fully_changed = bool(matched_pids) and changed_pids == matched_pids and not failed_pids
            if changed <= 0:
                if failed_pids:
                    return f"{'提高' if step > 0 else '降低'}全部打开进程音量失败"
                return "未找到可调节音量的全部打开进程"
            if fully_changed and avg_level is not None:
                self._remember_volume_for_all_running_sandboxes(avg_level)
            action = "提高" if step > 0 else "降低"
            if fully_changed:
                self._speak_process_assist(
                    f"全部进程音量{action}{abs(step)}%",
                    event_key="volume_all"
                )
                if avg_level is None:
                    return f"已将全部打开进程音量{action} {abs(step)}%"
                return f"已将全部打开进程音量{action} {abs(step)}%，当前约 {avg_level}%"
            self._speak_process_assist(
                f"全部进程音量部分{action}{abs(step)}%",
                event_key="volume_all"
            )
            return f"已部分将全部打开进程音量{action} {abs(step)}%"

        action = "提高" if step > 0 else "降低"
        self._run_volume_task_async(f"正在{action}全部打开进程音量...", _worker)

    def on_adjust_volume_current_sandbox(self, sandbox_name, step_percent=5):
        sandbox_name, display_name = self._get_sandbox_internal_and_display_name(sandbox_name)
        step = int(step_percent)

        def _worker():
            pids = self._get_target_pids_for_sandbox(sandbox_name)
            if not pids:
                return f"当前用户 {display_name} 没有可调节音量的已开进程"
            volume_result = self._adjust_volume_for_pids_detailed(pids, step / 100.0)
            changed = int(volume_result.get("changed_sessions", 0) or 0)
            avg_level = volume_result.get("avg_level")
            changed_pids = set(volume_result.get("changed_pids", set()) or set())
            matched_pids = set(volume_result.get("matched_pids", set()) or set())
            failed_pids = set(volume_result.get("failed_pids", set()) or set())
            fully_changed = bool(matched_pids) and changed_pids == matched_pids and not failed_pids
            if changed <= 0:
                if failed_pids:
                    return f"{'提高' if step > 0 else '降低'}音量失败: {display_name}"
                return f"未找到可调节音量的进程: {display_name}"
            if fully_changed and avg_level is not None and self._remember_volume_for_sandbox(sandbox_name, avg_level):
                self.save_settings()
            action = "提高" if step > 0 else "降低"
            if fully_changed:
                self._speak_process_assist(
                    f"{display_name}进程音量{action}{abs(step)}%",
                    event_key="volume_current"
                )
                if avg_level is None:
                    return f"已将 {display_name} 音量{action} {abs(step)}%"
                return f"已将 {display_name} 音量{action} {abs(step)}%，当前约 {avg_level}%"
            self._speak_process_assist(
                f"{display_name}进程音量部分{action}{abs(step)}%",
                event_key="volume_current"
            )
            return f"已部分将 {display_name} 音量{action} {abs(step)}%"

        action = "提高" if step > 0 else "降低"
        self._run_volume_task_async(f"正在{action}{display_name}进程音量...", _worker)

    def _close_sandbox_processes(self, sandbox_name, allow_pid_fallback=True):
        sandbox_name = self._normalize_sandbox_name(sandbox_name)
        result = {
            "sandbox_name": sandbox_name,
            "status": "none_closed",
            "closed": 0,
            "any_closed": False,
        }
        self._cancel_locate_window_reinforcement(wait=True)
        pids_before = self._get_target_pids_for_sandbox(sandbox_name)
        try:
            self._move_focus_off_target_pids(pids_before)
        except Exception:
            pass
        total_closed = 0
        if self._terminate_sandbox_via_start(sandbox_name):
            if pids_before:
                total_before = len(self._normalize_pid_set(pids_before))
                remaining_after_start = self._wait_for_pids_exit(pids_before)
                if not remaining_after_start:
                    result["status"] = "all_closed"
                    result["closed"] = total_before
                    result["any_closed"] = True
                    return result
                total_closed += max(0, total_before - len(self._normalize_pid_set(remaining_after_start)))
                pids = remaining_after_start
            else:
                pids = self._get_target_pids_for_sandbox(sandbox_name)
                if not pids:
                    result["status"] = "no_target"
                    return result
        else:
            pids = list(pids_before)
            if not pids:
                pids = self._get_target_pids_for_sandbox(sandbox_name)
        if not pids:
            result["status"] = "no_target"
            return result
        if not allow_pid_fallback:
            result["closed"] = total_closed
            if total_closed > 0:
                result["status"] = "partially_closed"
                result["any_closed"] = True
            return result
        tracked_pids = sorted(self._normalize_pid_set(pids))
        self._terminate_pids(tracked_pids)
        remaining_after_pid = self._wait_for_pids_exit(tracked_pids, timeout=1.0, interval=0.1)
        total_closed += max(0, len(self._normalize_pid_set(tracked_pids)) - len(self._normalize_pid_set(remaining_after_pid)))
        result["closed"] = total_closed
        if not remaining_after_pid:
            result["status"] = "all_closed"
            result["any_closed"] = total_closed > 0
        elif total_closed > 0:
            result["status"] = "partially_closed"
            result["any_closed"] = True
        return result

    def _run_close_process_task_async(self, start_status, worker):
        if not self._close_process_task_lock.acquire(blocking=False):
            self.lbl_status.SetLabel("正在关闭进程，请稍候")
            return
        self.lbl_status.SetLabel(str(start_status or "正在关闭进程..."))

        def _task():
            try:
                worker()
            except Exception as e:
                self._safe_call_after(self.lbl_status.SetLabel, f"关闭进程失败: {str(e)}")
            finally:
                self._close_process_task_lock.release()

        threading.Thread(target=_task, daemon=True).start()

    def _finish_close_current_sandbox(self, display_name, close_result):
        status = close_result.get("status")
        closed = int(close_result.get("closed", 0) or 0)
        if status == "all_closed":
            self.lbl_status.SetLabel(f"已关闭当前打开的进程: {display_name}")
            self._speak_process_assist(f"已关闭{display_name}进程", event_key="close_current")
            self._refresh_process_ui_after_action(refresh_list=True)
            return
        if status == "no_target":
            self.lbl_status.SetLabel(f"当前用户 {display_name} 没有可关闭的已开进程")
            self._refresh_process_ui_after_action(refresh_list=False)
            return
        if status == "partially_closed":
            self.lbl_status.SetLabel(f"已关闭部分进程，仍有进程未关闭: {display_name}")
        elif closed > 0:
            self.lbl_status.SetLabel(f"已关闭 {closed} 个进程: {display_name}")
            self._speak_process_assist(f"已关闭{display_name}进程", event_key="close_current")
        else:
            self.lbl_status.SetLabel(f"未关闭任何进程: {display_name}")
        self._refresh_process_ui_after_action(refresh_list=True)

    def on_close_current_sandbox(self, sandbox_name):
        sandbox_name, display_name = self._get_sandbox_internal_and_display_name(sandbox_name)

        def _worker():
            close_result = self._close_sandbox_processes(sandbox_name)
            self._safe_call_after(self._finish_close_current_sandbox, display_name, close_result)

        self._run_close_process_task_async(f"正在关闭当前打开的进程: {display_name}", _worker)

    def on_close_all_sandboxes(self):
        def _worker():
            self._cancel_active_launch()
            pids_before_all = self._get_target_pids_for_all()
            if self._terminate_all_via_start():
                if pids_before_all:
                    remaining_after_start_all = self._wait_for_pids_exit(pids_before_all)
                    if not remaining_after_start_all:
                        self._safe_call_after(self._finish_close_all_sandboxes, "all_closed", 0, False)
                        return
                    any_terminated = len(remaining_after_start_all) < len(self._normalize_pid_set(pids_before_all))
                else:
                    remaining_after_start_all = self._get_target_pids_for_all()
                    if not remaining_after_start_all:
                        self._safe_call_after(self._finish_close_all_sandboxes, "no_target", 0, False)
                        return
                    any_terminated = False
            else:
                remaining_after_start_all = list(pids_before_all)
                any_terminated = False
            sandbox_names = self._scan_sandbox_names_for_worker()
            for sandbox_name in sandbox_names:
                close_result = self._close_sandbox_processes(sandbox_name, allow_pid_fallback=False)
                if close_result.get("any_closed"):
                    any_terminated = True
            pids = self._get_target_pids_for_all()
            if not pids and remaining_after_start_all:
                pids = list(remaining_after_start_all)
            if not pids:
                self._safe_call_after(
                    self._finish_close_all_sandboxes,
                    "all_closed" if any_terminated else "no_target",
                    0,
                    any_terminated
                )
                return
            closed = self._terminate_pids(pids)
            remaining_pids = []
            if closed > 0 or any_terminated:
                try:
                    remaining_pids = self._get_target_pids_for_all()
                except Exception:
                    remaining_pids = list(pids)
            if not remaining_pids and (closed > 0 or any_terminated):
                self._safe_call_after(self._finish_close_all_sandboxes, "all_closed", closed, any_terminated)
            elif closed > 0:
                self._safe_call_after(self._finish_close_all_sandboxes, "closed_some", closed, any_terminated)
            elif any_terminated:
                self._safe_call_after(self._finish_close_all_sandboxes, "partially_closed", 0, any_terminated)
            else:
                self._safe_call_after(self._finish_close_all_sandboxes, "none_closed", 0, False)

        self._run_close_process_task_async("正在关闭全部打开进程...", _worker)

    def _finish_close_all_sandboxes(self, status, closed=0, any_terminated=False):
        closed = int(closed or 0)
        if status == "all_closed":
            self.lbl_status.SetLabel("已关闭全部打开进程")
            self._speak_process_assist("已关闭全部进程", event_key="close_all")
            self._refresh_process_ui_after_action(refresh_list=True)
            return
        if status == "no_target":
            self.lbl_status.SetLabel("当前没有可关闭的全部打开进程")
            self._refresh_process_ui_after_action(refresh_list=False)
            return
        if status == "closed_some":
            self.lbl_status.SetLabel(f"已关闭 {closed} 个全部打开进程")
            self._speak_process_assist("已关闭全部进程", event_key="close_all")
        elif status == "partially_closed" or any_terminated:
            self.lbl_status.SetLabel("已关闭部分打开进程，仍有进程未关闭")
        else:
            self.lbl_status.SetLabel("未关闭任何全部打开进程")
        self._refresh_process_ui_after_action(refresh_list=True)

    def on_locate_current_sandbox(self, sandbox_name):
        self._cancel_locate_window_reinforcement(wait=True)
        sandbox_name, display_name = self._get_sandbox_internal_and_display_name(sandbox_name)
        pids = self._get_target_pids_for_sandbox(sandbox_name)
        if not pids:
            self.lbl_status.SetLabel(f"当前用户 {display_name} 没有可定位的打开进程")
            return
        windows = self._find_windows_for_pids(pids)
        if not windows:
            self.lbl_status.SetLabel(f"当前用户 {display_name} 没有可定位的打开进程")
            return
        hwnd = self._prompt_window_choice(display_name, windows)
        if not hwnd:
            self.lbl_status.SetLabel(f"已取消定位当前打开的进程: {display_name}")
            return
        selected_window = None
        target_hwnd = int(hwnd or 0)
        for item in windows:
            try:
                if int(item.get("hwnd") or 0) == target_hwnd:
                    selected_window = item
                    break
            except Exception:
                continue
        selected_was_minimized = bool((selected_window or {}).get("minimized"))
        if not self._stabilize_activated_window(hwnd):
            self.lbl_status.SetLabel(f"定位进程失败: {display_name}")
            return
        if selected_was_minimized:
            self._start_locate_window_reinforcement(hwnd, keep_long_tail=True)
            def _finish_minimized_locate():
                if self._is_window_closing():
                    return
                self._cancel_locate_window_reinforcement(wait=True)
                if not self._stabilize_activated_window(hwnd):
                    self.lbl_status.SetLabel(f"定位进程失败: {display_name}")
                    return
                self._start_locate_window_reinforcement(hwnd, keep_long_tail=True)
                if len(windows) > 1:
                    if not self._has_foreground_window_streak(hwnd, required_streak=2, total_checks=4, interval_seconds=0.1):
                        try:
                            self._activate_window(hwnd)
                        except Exception:
                            pass
                        if not self._has_foreground_window_streak(hwnd, required_streak=2, total_checks=4, interval_seconds=0.1):
                            self._cancel_locate_window_reinforcement(wait=True)
                            self.lbl_status.SetLabel(f"定位进程失败: {display_name}")
                            return
                self.lbl_status.SetLabel(f"已定位到当前打开的进程: {display_name}")
                self._speak_process_assist(f"已定位{display_name}进程", event_key="locate_current")
            self._safe_call_later(2450, _finish_minimized_locate)
            return
        if len(windows) > 1:
            if not self._has_foreground_window_streak(hwnd, required_streak=2, total_checks=4, interval_seconds=0.1):
                try:
                    self._activate_window(hwnd)
                except Exception:
                    pass
                if not self._has_foreground_window_streak(hwnd, required_streak=2, total_checks=4, interval_seconds=0.1):
                    self._cancel_locate_window_reinforcement(wait=True)
                    self.lbl_status.SetLabel(f"定位进程失败: {display_name}")
                    return
        self.lbl_status.SetLabel(f"已定位到当前打开的进程: {display_name}")
        self._speak_process_assist(f"已定位{display_name}进程", event_key="locate_current")

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

    def create_menu_bar(self):
        menubar = wx.MenuBar()
        file_menu = wx.Menu()
        open_app_item = file_menu.Append(wx.ID_ANY, self._menu_label("选择应用", "open_app"), "选择要在沙盒中运行的应用")
        self.Bind(wx.EVT_MENU, self.on_menu_open_app, open_app_item)
        paste_app_item = file_menu.Append(wx.ID_ANY, self._menu_label("粘贴 .exe 或文件路径", "paste_app"), "从剪贴板粘贴应用路径")
        self.Bind(wx.EVT_MENU, self.on_menu_paste_app, paste_app_item)
        launch_item = file_menu.Append(wx.ID_ANY, self._menu_label("启动选中沙盒", "launch_selected"), "启动当前勾选的沙盒用户")
        self.Bind(wx.EVT_MENU, self.on_menu_launch_selected, launch_item)
        refresh_item = file_menu.Append(wx.ID_ANY, self._menu_label("刷新沙盒用户", "refresh"), "刷新沙盒用户列表")
        self.Bind(wx.EVT_MENU, self.on_f5_refresh, refresh_item)
        prev_user_item = file_menu.Append(wx.ID_ANY, self._menu_label("切换到上一个用户", "select_prev_user"), "将焦点切换到上一个用户")
        self.Bind(wx.EVT_MENU, self.on_menu_select_prev_user, prev_user_item)
        next_user_item = file_menu.Append(wx.ID_ANY, self._menu_label("切换到下一个用户", "select_next_user"), "将焦点切换到下一个用户")
        self.Bind(wx.EVT_MENU, self.on_menu_select_next_user, next_user_item)
        select_all_item = file_menu.Append(wx.ID_ANY, self._menu_label("全选", "select_all"), "勾选当前列表全部项目")
        self.Bind(wx.EVT_MENU, self.on_ctrl_a_select, select_all_item)
        file_menu.AppendSeparator()
        exit_item = file_menu.Append(wx.ID_ANY, self._menu_label("退出软件", "exit_app"), "退出软件")
        self.Bind(wx.EVT_MENU, self.on_alt_f4_exit, exit_item)
        menubar.Append(file_menu, "文件(&F)")

        process_states = self._get_process_menu_states()
        live_process_state = self._get_process_menu_live_state()
        selected_name = str(process_states.get("selected_name", "") or "").strip()
        selected_has_process = bool(process_states.get("selected_has_process", False))
        all_has_process = bool(process_states.get("all_has_process", False))
        selected_has_window = bool(selected_has_process)
        selected_mute_state = live_process_state.get("selected_mute_state")
        selected_audio_session = bool(selected_has_process)
        all_mute_state = live_process_state.get("all_mute_state")
        all_audio_session = bool(all_has_process)
        output_devices_available = bool(selected_has_process or all_has_process)
        mute_current_label = "静音当前打开的进程"
        if selected_mute_state is True:
            mute_current_label = "取消静音当前打开的进程"
        mute_all_label = "静音全部打开的进程"
        if all_mute_state is True:
            mute_all_label = "取消静音全部打开的进程"

        process_menu = wx.Menu()
        mute_current_item = process_menu.Append(
            wx.ID_ANY,
            self._menu_label_with_state(mute_current_label, "mute_current", selected_audio_session),
            "切换当前用户进程静音"
        )
        self.Bind(wx.EVT_MENU, self.on_menu_mute_current_process, mute_current_item)
        mute_current_item.Enable(selected_audio_session)

        mute_all_item = process_menu.Append(
            wx.ID_ANY,
            self._menu_label_with_state(mute_all_label, "mute_all", all_audio_session),
            "切换全部进程静音"
        )
        self.Bind(wx.EVT_MENU, self.on_ctrl_m_mute_all, mute_all_item)
        mute_all_item.Enable(all_audio_session)
        process_menu.AppendSeparator()

        volume_current_up_item = process_menu.Append(
            wx.ID_ANY,
            self._menu_label_with_state("提高当前打开的进程音量（+5%）", "volume_current_up", selected_audio_session),
            "提高当前用户进程音量"
        )
        self.Bind(wx.EVT_MENU, self.on_menu_volume_current_up, volume_current_up_item)
        volume_current_up_item.Enable(selected_audio_session)

        volume_current_down_item = process_menu.Append(
            wx.ID_ANY,
            self._menu_label_with_state("降低当前打开的进程音量（-5%）", "volume_current_down", selected_audio_session),
            "降低当前用户进程音量"
        )
        self.Bind(wx.EVT_MENU, self.on_menu_volume_current_down, volume_current_down_item)
        volume_current_down_item.Enable(selected_audio_session)

        volume_all_up_item = process_menu.Append(
            wx.ID_ANY,
            self._menu_label_with_state("提高全部打开的进程音量（+5%）", "volume_all_up", all_audio_session),
            "提高全部进程音量"
        )
        self.Bind(wx.EVT_MENU, self.on_menu_volume_all_up, volume_all_up_item)
        volume_all_up_item.Enable(all_audio_session)

        volume_all_down_item = process_menu.Append(
            wx.ID_ANY,
            self._menu_label_with_state("降低全部打开的进程音量（-5%）", "volume_all_down", all_audio_session),
            "降低全部进程音量"
        )
        self.Bind(wx.EVT_MENU, self.on_menu_volume_all_down, volume_all_down_item)
        volume_all_down_item.Enable(all_audio_session)
        process_menu.AppendSeparator()

        switch_current_output_item = process_menu.Append(
            wx.ID_ANY,
            self._menu_label_with_state("切换当前打开的进程播放设备", "switch_output_current", selected_has_process and output_devices_available),
            "切换当前用户进程播放设备"
        )
        self.Bind(wx.EVT_MENU, self.on_menu_switch_output_current, switch_current_output_item)
        switch_current_output_item.Enable(selected_has_process and output_devices_available)

        switch_all_output_item = process_menu.Append(
            wx.ID_ANY,
            self._menu_label_with_state("切换全部打开的进程播放设备", "switch_output_all", all_has_process and output_devices_available),
            "切换全部打开进程播放设备"
        )
        self.Bind(wx.EVT_MENU, self.on_menu_switch_output_all, switch_all_output_item)
        switch_all_output_item.Enable(all_has_process and output_devices_available)
        process_menu.AppendSeparator()

        close_current_item = process_menu.Append(
            wx.ID_ANY,
            self._menu_label_with_state("关闭当前打开的进程", "close_current_process", selected_has_process),
            "关闭当前用户进程"
        )
        self.Bind(wx.EVT_MENU, self.on_menu_close_current_process, close_current_item)
        close_current_item.Enable(selected_has_process)

        close_all_item = process_menu.Append(
            wx.ID_ANY,
            self._menu_label_with_state("关闭全部打开的进程", "close_all_processes", all_has_process),
            "关闭全部进程"
        )
        self.Bind(wx.EVT_MENU, self.on_menu_close_all_processes, close_all_item)
        close_all_item.Enable(all_has_process)

        locate_item = process_menu.Append(
            wx.ID_ANY,
            self._menu_label_with_state("定位当前打开的进程", "locate_current_process", selected_has_window),
            "定位当前用户进程窗口"
        )
        self.Bind(wx.EVT_MENU, self.on_menu_locate_current_process, locate_item)
        locate_item.Enable(selected_has_window)
        process_menu.AppendSeparator()
        refresh_process_item = process_menu.Append(
            wx.ID_ANY,
            self._menu_label("刷新进程状态", "refresh_process_state"),
            "刷新进程、窗口和键盘转发相关状态"
        )
        self.Bind(wx.EVT_MENU, self.on_menu_refresh_process_state, refresh_process_item)
        menubar.Append(process_menu, "进程(&P)")

        sandbox_menu = wx.Menu()
        group_submenu = wx.Menu()
        group_create_item = group_submenu.Append(wx.ID_ANY, self._menu_label("新建分组", "create_group"), "将当前勾选的沙盒用户保存为分组")
        self.Bind(wx.EVT_MENU, self.on_create_group, group_create_item)
        group_launch_item = group_submenu.Append(wx.ID_ANY, self._menu_label("启动分组", "launch_group"), "选择分组并直接启动分组内沙盒用户")
        self.Bind(wx.EVT_MENU, self.on_launch_group, group_launch_item)
        group_delete_item = group_submenu.Append(wx.ID_ANY, self._menu_label("删除分组", "delete_group"), "删除已保存的分组")
        self.Bind(wx.EVT_MENU, self.on_delete_group, group_delete_item)
        sandbox_menu.AppendSubMenu(group_submenu, "分组管理")

        user_submenu = wx.Menu()
        pin_item = user_submenu.Append(
            wx.ID_ANY,
            self._menu_label("置顶/取消置顶当前用户", "pin_current_user"),
            "置顶或取消置顶当前选中的用户"
        )
        self.Bind(wx.EVT_MENU, self.on_hotkey_pin_current_user, pin_item)

        user_submenu.AppendSeparator()
        batch_create_item = user_submenu.Append(wx.ID_ANY, self._menu_label("批量创建沙盒用户", "batch_create_sandboxes"), "按命名规则批量创建沙盒用户")
        self.Bind(wx.EVT_MENU, self.on_batch_create_sandboxes, batch_create_item)
        batch_rename_item = user_submenu.Append(wx.ID_ANY, self._menu_label("批量重命名沙盒用户", "batch_rename_sandboxes"), "将勾选的沙盒用户按命名规则批量重命名")
        self.Bind(wx.EVT_MENU, self.on_batch_rename_sandboxes, batch_rename_item)
        batch_delete_item = user_submenu.Append(wx.ID_ANY, self._menu_label("批量删除沙盒用户", "batch_delete_sandboxes"), "删除勾选的沙盒用户")
        self.Bind(wx.EVT_MENU, self.on_batch_delete_sandboxes, batch_delete_item)
        sandbox_menu.AppendSubMenu(user_submenu, "用户管理")
        menubar.Append(sandbox_menu, "沙盒管理(&G)")

        schedule_menu = wx.Menu()
        schedule_launch_item = schedule_menu.Append(wx.ID_ANY, self._menu_label("定时启动选中沙盒", "schedule_launch"), "在指定秒数后启动勾选的沙盒用户")
        self.Bind(wx.EVT_MENU, self.on_schedule_launch_sandboxes, schedule_launch_item)
        schedule_close_item = schedule_menu.Append(wx.ID_ANY, self._menu_label("定时关闭选中沙盒", "schedule_close"), "在指定秒数后关闭勾选的沙盒用户")
        self.Bind(wx.EVT_MENU, self.on_schedule_close_sandboxes, schedule_close_item)
        cancel_schedule_item = schedule_menu.Append(wx.ID_ANY, self._menu_label("取消全部定时任务", "cancel_scheduled_tasks"), "取消当前全部已设置的定时任务")
        self.Bind(wx.EVT_MENU, self.on_cancel_all_schedules, cancel_schedule_item)
        menubar.Append(schedule_menu, "定时(&D)")

        settings_menu = wx.Menu()
        hotkey_item = settings_menu.Append(wx.ID_ANY, self._menu_label("热键设置", "hotkey_settings"), "打开热键设置")
        self.Bind(wx.EVT_MENU, self.on_hotkey_settings, hotkey_item)
        speech_item = settings_menu.Append(wx.ID_ANY, self._menu_label("朗读设置", "speech_settings"), "打开朗读设置")
        self.Bind(wx.EVT_MENU, self.on_speech_settings, speech_item)
        software_settings_item = settings_menu.Append(wx.ID_ANY, self._menu_label("软件设置", "settings"), "打开软件设置")
        self.Bind(wx.EVT_MENU, self.on_settings, software_settings_item)
        menubar.Append(settings_menu, "设置(&S)")

        correct_menu = wx.Menu()
        manual_correct_item = correct_menu.Append(wx.ID_ANY, self._menu_label("手动校正 Sandboxie 安装路径", "manual_correct_sandboxie_path"), "手动指定 Sandboxie 安装目录")
        self.Bind(wx.EVT_MENU, self.on_manual_correct_sandboxie_path, manual_correct_item)
        sandbox_root_item = correct_menu.Append(wx.ID_ANY, self._menu_label("选择沙盒用户数据目录", "select_sandbox_root"), "选择沙盒用户数据目录根路径")
        self.Bind(wx.EVT_MENU, self.on_select_sandbox_root, sandbox_root_item)
        correct_menu.AppendSeparator()
        repair_control_settings_item = correct_menu.Append(
            wx.ID_ANY,
            self._menu_label("修复 Sandboxie Control 用户设置", "repair_sandboxie_control_settings"),
            "清理 Sandboxie Control 窗口坐标等用户状态记录"
        )
        self.Bind(wx.EVT_MENU, self.on_repair_sandboxie_control_settings, repair_control_settings_item)
        menubar.Append(correct_menu, "校正(&J)")

        tools_menu = wx.Menu()
        backup_item = tools_menu.Append(wx.ID_ANY, self._menu_label("备份沙盒用户", "backup_sandboxes"), "备份所选沙盒用户")
        self.Bind(wx.EVT_MENU, self.on_backup_sandboxes, backup_item)
        restore_item = tools_menu.Append(wx.ID_ANY, self._menu_label("还原沙盒用户", "restore_sandboxes"), "从备份文件还原所选沙盒用户")
        self.Bind(wx.EVT_MENU, self.on_restore_sandboxes, restore_item)
        menubar.Append(tools_menu, "工具(&T)")

        about_menu = wx.Menu()
        check_update_item = about_menu.Append(
            wx.ID_ANY,
            self._menu_label("检查更新", "check_update"),
            "检查软件是否有新版本"
        )
        self.Bind(wx.EVT_MENU, self.on_check_update, check_update_item)
        about_menu.AppendSeparator()
        donate_submenu = wx.Menu()
        wechat_item = donate_submenu.Append(
            wx.ID_ANY,
            self._menu_label("微信打赏", "show_wechat_qr"),
            "显示微信赞赏码"
        )
        alipay_item = donate_submenu.Append(
            wx.ID_ANY,
            self._menu_label("支付宝打赏", "show_alipay_qr"),
            "显示支付宝赞赏码"
        )
        self.Bind(wx.EVT_MENU, self.on_hotkey_show_wechat_qr, wechat_item)
        self.Bind(wx.EVT_MENU, self.on_hotkey_show_alipay_qr, alipay_item)
        about_menu.AppendSubMenu(donate_submenu, "请作者吃辣条", "支持作者，请吃辣条")
        menubar.Append(about_menu, "关于(&A)")
        self.SetMenuBar(menubar)

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



    def on_hotkey_settings(self, event):
        dialog = HotkeySettingsDialog(
            self,
            self.hotkey_action_specs,
            self.hotkeys,
            self.default_hotkeys,
            self._parse_hotkey_text,
            self._format_hotkey_text,
            hotkeys_enabled=self.hotkeys_enabled,
            reserved_hotkeys=self._get_reserved_hotkey_specs()
        )
        try:
            if dialog.ShowModal() != wx.ID_OK:
                return
            self.hotkeys = self._normalize_hotkey_map(dialog.result_hotkeys)
            self.hotkeys_enabled = bool(dialog.result_hotkeys_enabled)
            self._apply_hotkeys()
            self.create_menu_bar()
            self.save_settings()
            self.lbl_status.SetLabel("快捷键设置已保存")
        finally:
            dialog.Destroy()

    def _get_speech_source_config_path(self):
        candidates = [
            os.path.join(getattr(self, "CONFIG_DIR", get_configuration_dir(get_app_dir())), "SpeechConfig.json"),
            os.path.join(get_app_dir(), "SpeechConfig.json"),
            get_resource_path("SpeechConfig.json"),
            os.path.join(get_app_dir(), "internal", "SpeechConfig.json"),
            os.path.join(get_resource_path("internal"), "SpeechConfig.json"),
            os.path.join(get_resource_path("Speech"), "SpeechConfig.json"),
            os.path.join(get_app_dir(), "Speech", "SpeechConfig.json"),
            os.path.join(os.path.dirname(os.path.abspath(__file__)), "Speech", "SpeechConfig.json"),
            os.path.join(
                os.path.dirname(os.path.abspath(__file__)),
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






    def _build_hotkey_action_specs(self):
        specs = [
            {"category": "软件层面", "action": "toggle_window", "label": "显示/隐藏窗口", "default": "Ctrl+Shift+W", "event_id": 2999, "handler": self.on_hotkey_toggle_window},
            {"category": "软件层面", "action": "hotkey_settings", "label": "打开快捷键设置", "default": "Ctrl+Shift+K", "event_id": 3025, "handler": self.on_hotkey_settings},
            {"category": "软件层面", "action": "speech_settings", "label": "朗读设置", "default": "Ctrl+Shift+R", "event_id": 3033, "handler": self.on_hotkey_speech_settings},
            {"category": "软件层面", "action": "reset_settings", "label": "重置所有设置为默认值", "default": "", "event_id": 3038, "handler": self.on_hotkey_reset_settings},
            {"category": "常用操作", "action": "settings", "label": "设置", "default": "Ctrl+Shift+S", "event_id": 3000, "handler": self.on_f4_settings},
            {"category": "路径记忆", "action": "memory_manager", "label": "打开应用路径菜单", "default": "Ctrl+H", "event_id": 3003, "handler": self.on_ctrl_h_memory},
            {"category": "路径记忆", "action": "memory_edit_note", "label": "路径记忆备注", "default": "", "event_id": 3050, "handler": self.on_hotkey_memory_edit_note},
            {"category": "路径记忆", "action": "memory_adjust_count", "label": "调整记忆数", "default": "", "event_id": 3051, "handler": self.on_hotkey_memory_adjust_count},
            {"category": "路径记忆", "action": "memory_toggle_path_display", "label": "隐藏/显示路径", "default": "", "event_id": 3052, "handler": self.on_hotkey_memory_toggle_path_display},
            {"category": "路径记忆", "action": "memory_delete_current", "label": "删除当前记忆", "default": "", "event_id": 3053, "handler": self.on_hotkey_memory_delete_current},
            {"category": "路径记忆", "action": "memory_clear_all", "label": "清空所有路径记忆", "default": "", "event_id": 3054, "handler": self.on_hotkey_memory_clear_all},
            {"category": "路径记忆", "action": "memory_copy_path", "label": "复制完整路径", "default": "", "event_id": 3055, "handler": self.on_hotkey_memory_copy_path},
            {"category": "常用操作", "action": "launch_selected", "label": "启动选中沙盒", "default": "Ctrl+Enter", "event_id": 3016, "handler": self.on_hotkey_launch_selected},
            {"category": "常用操作", "action": "select_prev_user", "label": "切换到上一个用户", "default": "Alt+Up", "event_id": 3036, "handler": self.on_hotkey_select_prev_user},
            {"category": "常用操作", "action": "select_next_user", "label": "切换到下一个用户", "default": "Alt+Down", "event_id": 3037, "handler": self.on_hotkey_select_next_user},
            {"category": "常用操作", "action": "create_group", "label": "新建分组", "default": "", "event_id": 3026, "handler": self.on_hotkey_create_group},
            {"category": "常用操作", "action": "launch_group", "label": "启动分组", "default": "", "event_id": 3027, "handler": self.on_hotkey_launch_group},
            {"category": "常用操作", "action": "delete_group", "label": "删除分组", "default": "", "event_id": 3028, "handler": self.on_hotkey_delete_group},
            {"category": "进程操作", "action": "mute_current", "label": "静音当前打开的进程", "default": "Ctrl+M", "event_id": 3017, "handler": self.on_hotkey_mute_current},
            {"category": "进程操作", "action": "mute_all", "label": "静音全部打开的进程", "default": "Ctrl+Shift+M", "event_id": 3006, "handler": self.on_ctrl_m_mute_all},
            {"category": "进程操作", "action": "volume_all_up", "label": "提高全部打开的进程音量（+5%）", "default": "Ctrl+Shift+Up", "event_id": 3007, "handler": self.on_hotkey_volume_up},
            {"category": "进程操作", "action": "volume_all_down", "label": "降低全部打开的进程音量（-5%）", "default": "Ctrl+Shift+Down", "event_id": 3008, "handler": self.on_hotkey_volume_down},
            {"category": "进程操作", "action": "volume_current_up", "label": "提高当前打开的进程音量（+5%）", "default": "Ctrl+Up", "event_id": 3018, "handler": self.on_hotkey_volume_current_up},
            {"category": "进程操作", "action": "volume_current_down", "label": "降低当前打开的进程音量（-5%）", "default": "Ctrl+Down", "event_id": 3019, "handler": self.on_hotkey_volume_current_down},
            {"category": "进程操作", "action": "switch_output_current", "label": "切换当前打开的进程播放设备", "default": "", "event_id": 3034, "handler": self.on_hotkey_switch_output_current},
            {"category": "进程操作", "action": "switch_output_all", "label": "切换全部打开的进程播放设备", "default": "", "event_id": 3035, "handler": self.on_hotkey_switch_output_all},
            {"category": "进程操作", "action": "close_current_process", "label": "关闭当前打开的进程", "default": "", "event_id": 3020, "handler": self.on_hotkey_close_current_process},
            {"category": "进程操作", "action": "close_all_processes", "label": "关闭全部打开的进程", "default": "", "event_id": 3021, "handler": self.on_hotkey_close_all_processes},
            {"category": "进程操作", "action": "locate_current_process", "label": "定位当前打开的进程", "default": "Ctrl+L", "event_id": 3022, "handler": self.on_hotkey_locate_current_process},
            {"category": "进程操作", "action": "refresh_process_state", "label": "刷新进程状态", "default": "Ctrl+F5", "event_id": 3057, "handler": self.on_hotkey_refresh_process_state},
            {"category": "沙盒管理", "action": "batch_create_sandboxes", "label": "批量创建沙盒用户", "default": "", "event_id": 3009, "handler": self.on_hotkey_batch_create_sandboxes},
            {"category": "沙盒管理", "action": "batch_rename_sandboxes", "label": "批量重命名沙盒用户", "default": "", "event_id": 3010, "handler": self.on_hotkey_batch_rename_sandboxes},
            {"category": "沙盒管理", "action": "batch_delete_sandboxes", "label": "批量删除沙盒用户", "default": "", "event_id": 3011, "handler": self.on_hotkey_batch_delete_sandboxes},
            {"category": "沙盒管理", "action": "pin_current_user", "label": "置顶/取消置顶当前用户", "default": "", "event_id": 3012, "handler": self.on_hotkey_pin_current_user},
            {"category": "定时任务", "action": "schedule_launch", "label": "定时启动选中沙盒", "default": "", "event_id": 3013, "handler": self.on_hotkey_schedule_launch},
            {"category": "定时任务", "action": "schedule_close", "label": "定时关闭选中沙盒", "default": "", "event_id": 3014, "handler": self.on_hotkey_schedule_close},
            {"category": "定时任务", "action": "cancel_scheduled_tasks", "label": "取消全部定时任务", "default": "", "event_id": 3015, "handler": self.on_hotkey_cancel_scheduled_tasks},
            {"category": "工具操作", "action": "backup_sandboxes", "label": "备份沙盒用户", "default": "", "event_id": 3029, "handler": self.on_hotkey_backup_sandboxes},
            {"category": "工具操作", "action": "restore_sandboxes", "label": "还原沙盒用户", "default": "", "event_id": 3030, "handler": self.on_hotkey_restore_sandboxes},
            {"category": "目录校正", "action": "manual_correct_sandboxie_path", "label": "手动校正 Sandboxie 安装路径", "default": "", "event_id": 3031, "handler": self.on_hotkey_manual_correct_sandboxie_path},
            {"category": "目录校正", "action": "select_sandbox_root", "label": "选择沙盒用户数据目录", "default": "", "event_id": 3032, "handler": self.on_hotkey_select_sandbox_root},
            {"category": "设置开关", "action": "toggle_dynamic_launch_delay", "label": "切换：按系统负载动态调整延迟", "default": "", "event_id": 3039, "handler": self.on_hotkey_toggle_dynamic_launch_delay},
            {"category": "设置开关", "action": "toggle_auto_run", "label": "切换：开机自动启动本程序", "default": "", "event_id": 3040, "handler": self.on_hotkey_toggle_auto_run},
            {"category": "设置开关", "action": "toggle_show_tray", "label": "切换：在托盘上显示", "default": "", "event_id": 3041, "handler": self.on_hotkey_toggle_show_tray},
            {"category": "设置开关", "action": "toggle_auto_select_range", "label": "切换：首尾两点自动勾选中间用户", "default": "", "event_id": 3042, "handler": self.on_hotkey_toggle_auto_select_range},
            {"category": "设置开关", "action": "toggle_skip_running_sandbox_on_launch", "label": "切换：启动跳过已运行的沙盒", "default": "", "event_id": 3045, "handler": self.on_hotkey_toggle_skip_running_sandbox_on_launch},
            {"category": "设置开关", "action": "toggle_remember_volume", "label": "切换：记忆进程音量", "default": "", "event_id": 3046, "handler": self.on_hotkey_toggle_remember_volume},
            {"category": "设置开关", "action": "toggle_list_left_right_nav", "label": "切换：用户列表左右光标选择", "default": "", "event_id": 3047, "handler": self.on_hotkey_toggle_list_left_right_nav},
        ]
        return specs

    def _parse_hotkey_text(self, hotkey_text):
        text = str(hotkey_text or "").strip()
        if not text:
            return None
        parts = [part.strip() for part in text.replace("＋", "+").split("+") if part.strip()]
        if not parts:
            return None

        modifiers = 0
        key_token = parts[-1]
        for token in parts[:-1]:
            token_upper = token.upper()
            if token_upper in ("CTRL", "CONTROL"):
                modifiers |= wx.ACCEL_CTRL
            elif token_upper == "ALT":
                modifiers |= wx.ACCEL_ALT
            elif token_upper == "SHIFT":
                modifiers |= wx.ACCEL_SHIFT
            else:
                return None

        key_code = self._parse_hotkey_key_token(key_token)
        if key_code is None:
            return None
        return modifiers, key_code

    def _parse_hotkey_key_token(self, token):
        key = str(token or "").strip()
        if not key:
            return None
        key_upper = key.upper()
        special_map = {
            "ESC": wx.WXK_ESCAPE,
            "ESCAPE": wx.WXK_ESCAPE,
            "ENTER": wx.WXK_RETURN,
            "RETURN": wx.WXK_RETURN,
            "SPACE": wx.WXK_SPACE,
            "TAB": wx.WXK_TAB,
            "BACKSPACE": wx.WXK_BACK,
            "DELETE": wx.WXK_DELETE,
            "DEL": wx.WXK_DELETE,
            "INSERT": wx.WXK_INSERT,
            "INS": wx.WXK_INSERT,
            "HOME": wx.WXK_HOME,
            "END": wx.WXK_END,
            "PAGEUP": wx.WXK_PAGEUP,
            "PGUP": wx.WXK_PAGEUP,
            "PAGEDOWN": wx.WXK_PAGEDOWN,
            "PGDN": wx.WXK_PAGEDOWN,
            "LEFT": wx.WXK_LEFT,
            "RIGHT": wx.WXK_RIGHT,
            "UP": wx.WXK_UP,
            "DOWN": wx.WXK_DOWN,
        }
        if key_upper in special_map:
            return special_map[key_upper]
        if re.fullmatch(r"F([1-9]|1[0-9]|2[0-4])", key_upper):
            return getattr(wx, f"WXK_{key_upper}", None)
        if len(key) == 1:
            if not key.isascii() or not key.isprintable():
                return None
            return ord(key_upper)
        if re.fullmatch(r"\d", key):
            return ord(key)
        numpad_map = {
            "NUMPAD0": wx.WXK_NUMPAD0,
            "NUMPAD1": wx.WXK_NUMPAD1,
            "NUMPAD2": wx.WXK_NUMPAD2,
            "NUMPAD3": wx.WXK_NUMPAD3,
            "NUMPAD4": wx.WXK_NUMPAD4,
            "NUMPAD5": wx.WXK_NUMPAD5,
            "NUMPAD6": wx.WXK_NUMPAD6,
            "NUMPAD7": wx.WXK_NUMPAD7,
            "NUMPAD8": wx.WXK_NUMPAD8,
            "NUMPAD9": wx.WXK_NUMPAD9,
            "NUMPADENTER": wx.WXK_NUMPAD_ENTER,
        }
        if key_upper in numpad_map:
            return numpad_map[key_upper]
        return None

    def _format_hotkey_text(self, modifiers, key_code):
        if key_code is None:
            return ""
        parts = []
        if modifiers & wx.ACCEL_CTRL:
            parts.append("Ctrl")
        if modifiers & wx.ACCEL_ALT:
            parts.append("Alt")
        if modifiers & wx.ACCEL_SHIFT:
            parts.append("Shift")
        key_name = self._format_hotkey_key_code(key_code)
        if not key_name:
            return ""
        parts.append(key_name)
        return "+".join(parts)

    def _format_hotkey_key_code(self, key_code):
        if key_code is None:
            return ""
        special_map = {
            wx.WXK_ESCAPE: "Esc",
            wx.WXK_RETURN: "Enter",
            wx.WXK_NUMPAD_ENTER: "NumPadEnter",
            wx.WXK_SPACE: "Space",
            wx.WXK_TAB: "Tab",
            wx.WXK_BACK: "Backspace",
            wx.WXK_DELETE: "Delete",
            wx.WXK_INSERT: "Insert",
            wx.WXK_HOME: "Home",
            wx.WXK_END: "End",
            wx.WXK_PAGEUP: "PageUp",
            wx.WXK_PAGEDOWN: "PageDown",
            wx.WXK_LEFT: "Left",
            wx.WXK_RIGHT: "Right",
            wx.WXK_UP: "Up",
            wx.WXK_DOWN: "Down",
        }
        if key_code in special_map:
            return special_map[key_code]
        for i in range(1, 25):
            if key_code == getattr(wx, f"WXK_F{i}", None):
                return f"F{i}"
        numpad_map = {
            wx.WXK_NUMPAD0: "NumPad0",
            wx.WXK_NUMPAD1: "NumPad1",
            wx.WXK_NUMPAD2: "NumPad2",
            wx.WXK_NUMPAD3: "NumPad3",
            wx.WXK_NUMPAD4: "NumPad4",
            wx.WXK_NUMPAD5: "NumPad5",
            wx.WXK_NUMPAD6: "NumPad6",
            wx.WXK_NUMPAD7: "NumPad7",
            wx.WXK_NUMPAD8: "NumPad8",
            wx.WXK_NUMPAD9: "NumPad9",
        }
        if key_code in numpad_map:
            return numpad_map[key_code]
        if 48 <= key_code <= 57 or 65 <= key_code <= 90:
            return chr(key_code)
        return str(key_code)

    def _normalize_hotkey_map(self, hotkeys):
        raw_map = hotkeys if isinstance(hotkeys, dict) else {}
        normalized = {}
        reserved_keys = set()
        for hotkey_text in FIXED_HOTKEY_TEXT_BY_ACTION.values():
            parsed_reserved = self._parse_hotkey_text(hotkey_text)
            if parsed_reserved:
                reserved_keys.add((parsed_reserved[0], parsed_reserved[1]))
        for spec in self.hotkey_action_specs:
            action = spec["action"]
            default_value = self.default_hotkeys.get(action, "")
            raw_value = raw_map.get(action, default_value)
            value = str(raw_value or "").strip()
            if not value:
                normalized[action] = ""
                continue
            parsed = self._parse_hotkey_text(value)
            if parsed:
                if (parsed[0], parsed[1]) in reserved_keys:
                    normalized[action] = ""
                else:
                    normalized[action] = self._format_hotkey_text(*parsed)
            else:
                normalized[action] = ""
        return normalized

    def _get_hotkey_text(self, action):
        fixed_text = str(FIXED_HOTKEY_TEXT_BY_ACTION.get(action, "") or "").strip()
        if fixed_text:
            return fixed_text
        if not getattr(self, "hotkeys_enabled", True):
            return ""
        value = str(self.hotkeys.get(action, "") or "").strip()
        if not value:
            return ""
        parsed = self._parse_hotkey_text(value)
        if parsed:
            return self._format_hotkey_text(*parsed)
        return value

    def _get_reserved_hotkey_specs(self):
        return [
            {
                "action": action,
                "label": str(FIXED_HOTKEY_LABEL_BY_ACTION.get(action, action) or action),
                "hotkey": hotkey,
            }
            for action, hotkey in FIXED_HOTKEY_TEXT_BY_ACTION.items()
            if str(hotkey or "").strip()
        ]

    def _menu_label(self, label, action):
        hotkey = self._get_hotkey_text(action)
        return f"{label}\t{hotkey}" if hotkey else label

    def _menu_label_with_state(self, label, action, enabled=True):
        return self._menu_label(label, action)

    def _bind_hotkey_handlers_once(self):
        if getattr(self, "_hotkey_bindings_done", False):
            return
        for spec in self.hotkey_action_specs:
            self.Bind(wx.EVT_MENU, spec["handler"], id=spec["event_id"])
        self._hotkey_bindings_done = True

    def _apply_hotkeys(self):
        self._bind_hotkey_handlers_once()
        if not getattr(self, "hotkeys_enabled", True):
            self.accel_tbl = wx.AcceleratorTable([])
            self.SetAcceleratorTable(self.accel_tbl)
            return
        entries = []
        for spec in self.hotkey_action_specs:
            action = spec["action"]
            hotkey_text = self._get_hotkey_text(action)
            parsed = self._parse_hotkey_text(hotkey_text)
            if parsed:
                modifiers, key_code = parsed
                entries.append((modifiers, key_code, spec["event_id"]))
        self.accel_tbl = wx.AcceleratorTable(entries)
        self.SetAcceleratorTable(self.accel_tbl)

    def _get_selected_sandbox_name(self):
        if not hasattr(self, "sandbox_list"):
            return ""
        index = self.sandbox_list.GetFirstSelected()
        if index == -1:
            return ""
        try:
            item_text = self.sandbox_list.get_item_identity_text(index)
        except Exception:
            return ""
        if not item_text:
            return ""
        try:
            if self.sandbox_list._is_special_index(index):
                return ""
        except Exception:
            pass
        return item_text

    def _get_selectable_sandbox_indexes(self):
        if not hasattr(self, "sandbox_list"):
            return []
        indexes = []
        for i in range(self.sandbox_list.GetItemCount()):
            item_text = self.sandbox_list.get_item_identity_text(i)
            if not item_text:
                continue
            try:
                if self.sandbox_list._is_special_index(i):
                    continue
            except Exception:
                pass
            indexes.append(i)
        return indexes

    def _select_sandbox_list_index(self, index):
        if not hasattr(self, "sandbox_list"):
            return False
        if index < 0 or index >= self.sandbox_list.GetItemCount():
            return False
        current = self.sandbox_list.GetFirstSelected()
        if current != -1 and current != index:
            self.sandbox_list.Select(current, 0)
        self.sandbox_list.Select(index)
        if hasattr(self.sandbox_list, "context_item_index"):
            self.sandbox_list.context_item_index = index
        if hasattr(self.sandbox_list, "_current_value"):
            try:
                self.sandbox_list._current_value = self.sandbox_list.GetItemText(index)
            except Exception:
                self.sandbox_list._current_value = ""
        self.sandbox_list.Focus(index)
        self.sandbox_list.EnsureVisible(index)
        self.sandbox_list.announce_item_state(index)
        self._schedule_sandbox_selection_process_state_refresh()
        return True

    def _select_adjacent_sandbox(self, direction):
        indexes = self._get_selectable_sandbox_indexes()
        if not indexes:
            self.lbl_status.SetLabel("当前列表没有可选择的用户")
            return False
        current = self.sandbox_list.GetFirstSelected() if hasattr(self, "sandbox_list") else -1
        if current == -1 or current not in indexes:
            target = indexes[-1] if direction < 0 else indexes[0]
            return self._select_sandbox_list_index(target)

        pos = indexes.index(current)
        next_pos = (pos + (-1 if direction < 0 else 1)) % len(indexes)
        return self._select_sandbox_list_index(indexes[next_pos])

    def _invalidate_process_menu_state_cache(self):
        self._process_menu_state_cache = {}
        self._process_menu_state_cache_time = 0.0

    def _get_default_process_menu_live_state(self):
        return {
            "selected_mute_state": None,
            "all_mute_state": None,
            "selected_has_window": False,
            "output_devices_available": False,
            "selected_audio_session": False,
            "all_audio_session": False,
        }

    def _invalidate_process_menu_live_state_cache(self, clear_values=False):
        self._process_menu_live_state_refresh_token = int(getattr(self, "_process_menu_live_state_refresh_token", 0) or 0) + 1
        self._process_menu_live_state_pending = False
        timer = getattr(self, "_process_menu_live_state_timer", None)
        if timer is not None:
            try:
                timer.Stop()
            except Exception:
                pass
        self._process_menu_live_state_timer = None
        if clear_values:
            self._process_menu_live_state_cache = {}

    def _get_process_menu_live_state(self):
        state = dict(self._get_default_process_menu_live_state())
        cached = getattr(self, "_process_menu_live_state_cache", None)
        if isinstance(cached, dict):
            state.update(cached)
        return state

    def _schedule_process_menu_live_state_refresh(self, delay_ms=1):
        if self._is_window_closing():
            return False
        if getattr(self, "_process_menu_live_state_refresh_in_progress", False):
            self._process_menu_live_state_pending = True
            return True
        timer = getattr(self, "_process_menu_live_state_timer", None)
        if timer is not None:
            try:
                timer.Stop()
            except Exception:
                pass
            self._process_menu_live_state_timer = None
        token = int(getattr(self, "_process_menu_live_state_refresh_token", 0) or 0) + 1
        self._process_menu_live_state_refresh_token = token
        self._process_menu_live_state_timer = self._safe_call_later(delay_ms, self._start_process_menu_live_state_refresh, token)
        return self._process_menu_live_state_timer is not None

    def _start_process_menu_live_state_refresh(self, token):
        self._process_menu_live_state_timer = None
        if self._is_window_closing():
            return
        try:
            current_token = int(getattr(self, "_process_menu_live_state_refresh_token", 0) or 0)
        except Exception:
            current_token = 0
        if int(token or 0) != current_token:
            return
        if getattr(self, "_process_menu_live_state_refresh_in_progress", False):
            self._process_menu_live_state_pending = True
            return
        self._process_menu_live_state_refresh_in_progress = True
        threading.Thread(
            target=self._process_menu_live_state_refresh_worker,
            args=(token,),
            daemon=True
        ).start()

    def _process_menu_live_state_refresh_worker(self, token):
        result = dict(self._get_default_process_menu_live_state())
        try:
            process_states = self._get_process_menu_states()
            selected_name = str(process_states.get("selected_name", "") or "").strip()
            selected_has_process = bool(process_states.get("selected_has_process", False))
            all_has_process = bool(process_states.get("all_has_process", False))

            process_map = self._get_sandbox_process_map_via_api()
            if process_map is None:
                process_map = {}

            selected_pids = []
            if selected_has_process and selected_name:
                normalized_name = self._normalize_sandbox_name(selected_name).lower()
                selected_pids = sorted(set(process_map.get(normalized_name, [])))
                if not selected_pids:
                    selected_pids = self._get_target_pids_for_sandbox(selected_name)

            all_pids = []
            if all_has_process:
                if process_map:
                    aggregate = []
                    for pids in process_map.values():
                        aggregate.extend(pids or [])
                    all_pids = sorted(set(int(pid) for pid in aggregate if int(pid) > 0))
                if not all_pids:
                    all_pids = self._get_target_pids_for_all()

            if selected_pids:
                selected_mute_state = self._get_mute_state_for_pids(selected_pids)
                result["selected_mute_state"] = selected_mute_state
                result["selected_audio_session"] = selected_mute_state is not None
                result["selected_has_window"] = bool(self._find_window_for_pids(selected_pids))

            if all_pids:
                all_mute_state = self._get_mute_state_for_pids(all_pids)
                result["all_mute_state"] = all_mute_state
                result["all_audio_session"] = all_mute_state is not None

            if selected_pids or all_pids:
                result["output_devices_available"] = bool(self._list_output_audio_devices())
        except Exception:
            result = dict(self._get_default_process_menu_live_state())
        self._safe_call_after(self._finish_process_menu_live_state_refresh, token, result)

    def _finish_process_menu_live_state_refresh(self, token, result):
        try:
            current_token = int(getattr(self, "_process_menu_live_state_refresh_token", 0) or 0)
        except Exception:
            current_token = 0
        if int(token or 0) == current_token and isinstance(result, dict):
            self._process_menu_live_state_cache = dict(self._get_default_process_menu_live_state())
            self._process_menu_live_state_cache.update(result)
            selected_name = str(self._get_process_menu_states().get("selected_name", "") or "").strip()
            if selected_name:
                normalized_name = self._normalize_sandbox_name(selected_name)
                if normalized_name:
                    selected_mute_state = self._process_menu_live_state_cache.get("selected_mute_state")
                    if selected_mute_state is None:
                        self._cached_mute_state_by_sandbox.pop(normalized_name, None)
                    else:
                        self._cached_mute_state_by_sandbox[normalized_name] = bool(selected_mute_state)
            all_mute_state = self._process_menu_live_state_cache.get("all_mute_state")
            if all_mute_state is None:
                self._cached_mute_state_all = None
            else:
                self._cached_mute_state_all = bool(all_mute_state)
            try:
                self.create_menu_bar()
            except Exception:
                pass
        self._process_menu_live_state_refresh_in_progress = False
        if getattr(self, "_process_menu_live_state_pending", False):
            self._process_menu_live_state_pending = False
            self._schedule_process_menu_live_state_refresh(delay_ms=24)

    def _get_process_menu_states(self):
        # Fast menu state: use cached running markers in the list.  Do not call
        # Sandboxie or enumerate windows while opening menus; those calls are
        # done by the actual command handlers and by explicit refresh.
        selected_name = self._get_selected_sandbox_name()
        selected_has_process = False
        all_has_process = False
        if hasattr(self, "sandbox_list"):
            try:
                selected_index = self.sandbox_list.GetFirstSelected()
                if selected_index != -1:
                    text = self.sandbox_list.GetItemText(selected_index)
                    if not self.sandbox_list._is_special_index(selected_index):
                        selected_has_process = str(text or "").strip().endswith(self.sandbox_list.RUNNING_SUFFIX)
            except Exception:
                selected_has_process = False
            try:
                for index in range(self.sandbox_list.GetItemCount()):
                    if self.sandbox_list._is_special_index(index):
                        continue
                    text = self.sandbox_list.GetItemText(index)
                    if str(text or "").strip().endswith(self.sandbox_list.RUNNING_SUFFIX):
                        all_has_process = True
                        break
            except Exception:
                all_has_process = False
        all_has_process = self._resolve_all_has_process_for_menu_state(all_has_process)
        selected_has_window = False

        result = {
            "selected_name": selected_name,
            "selected_has_process": bool(selected_has_process),
            "all_has_process": bool(all_has_process),
            "selected_has_window": bool(selected_has_window),
            "all_has_window": bool(all_has_process),
        }
        self._process_menu_state_cache = dict(result)
        self._process_menu_state_cache_time = time.time()
        return result

    def _list_view_may_hide_sandbox_members(self):
        if not hasattr(self, "sandbox_list") or self.sandbox_list is None:
            return False
        try:
            if str(getattr(self.sandbox_list, "current_group_view", "") or "").strip():
                return True
        except Exception:
            pass
        try:
            for index in range(self.sandbox_list.GetItemCount()):
                if self.sandbox_list._is_group_index(index):
                    return True
        except Exception:
            pass
        return False

    def _has_any_running_sandbox_process(self):
        try:
            process_map = self._get_sandbox_process_map_via_api()
        except Exception:
            process_map = None
        if process_map is not None:
            return any(bool(pids) for pids in process_map.values())
        try:
            return bool(self._get_all_sandbox_pids(timeout=0.8))
        except Exception:
            return False

    def _resolve_all_has_process_for_menu_state(self, visible_all_has_process):
        if visible_all_has_process:
            return True
        if not self._list_view_may_hide_sandbox_members():
            return False
        return bool(self._has_any_running_sandbox_process())

    def _normalize_sandbox_groups(self, groups):
        normalized = {}
        valid_display_lookup = set()
        valid_internal_lookup = set()
        sandbox_source_ready = bool(
            getattr(self, "sandbox_root", "") or
            getattr(self, "SANDBOX_ROOT", "") or
            getattr(self, "sandbox_list", None)
        )
        if sandbox_source_ready:
            try:
                valid_display_lookup = {
                    str(name or "").strip().lower()
                    for name in self._get_existing_sandbox_display_names()
                    if str(name or "").strip()
                }
            except Exception:
                valid_display_lookup = set()
            try:
                valid_internal_lookup = {
                    str(name or "").strip().lower()
                    for name in self._get_existing_sandbox_names()
                    if str(name or "").strip()
                }
            except Exception:
                valid_internal_lookup = set()
        filter_invalid_members = bool(valid_display_lookup or valid_internal_lookup)
        if isinstance(groups, dict):
            for group_name, members in groups.items():
                name = str(group_name).strip()
                if not name:
                    continue
                if isinstance(members, (list, tuple, set)):
                    cleaned = []
                    for item in members:
                        item_name = str(item).strip()
                        normalized_item_name = item_name
                        if filter_invalid_members and item_name:
                            item_key = item_name.lower()
                            try:
                                internal_name = self._normalize_sandbox_name(item_name)
                            except Exception:
                                internal_name = item_name
                            internal_key = str(internal_name or "").strip().lower()
                            if item_key not in valid_display_lookup:
                                if not internal_key or internal_key not in valid_internal_lookup:
                                    continue
                            try:
                                display_name = self._get_sandbox_display_name(internal_name)
                            except Exception:
                                display_name = ""
                            normalized_item_name = str(display_name or item_name).strip()
                            if not normalized_item_name:
                                continue
                        if normalized_item_name and normalized_item_name not in cleaned:
                            cleaned.append(normalized_item_name)
                    if cleaned:
                        normalized[name] = cleaned
        return normalized

    def _normalize_sandbox_name_list(self, values):
        normalized = []
        valid_display_lookup = set()
        valid_internal_lookup = set()
        sandbox_source_ready = bool(
            getattr(self, "sandbox_root", "") or
            getattr(self, "SANDBOX_ROOT", "") or
            getattr(self, "sandbox_list", None)
        )
        if sandbox_source_ready:
            try:
                valid_display_lookup = {
                    str(name or "").strip().lower()
                    for name in self._get_existing_sandbox_display_names()
                    if str(name or "").strip()
                }
            except Exception:
                valid_display_lookup = set()
            try:
                valid_internal_lookup = {
                    str(name or "").strip().lower()
                    for name in self._get_existing_sandbox_names()
                    if str(name or "").strip()
                }
            except Exception:
                valid_internal_lookup = set()
        filter_invalid_names = bool(valid_display_lookup or valid_internal_lookup)
        if isinstance(values, (list, tuple, set)):
            for item in values:
                name = str(item or "").strip()
                normalized_name = name
                if not name:
                    continue
                if filter_invalid_names:
                    name_key = name.lower()
                    try:
                        internal_name = self._normalize_sandbox_name(name)
                    except Exception:
                        internal_name = name
                    internal_key = str(internal_name or "").strip().lower()
                    if name_key not in valid_display_lookup:
                        if not internal_key or internal_key not in valid_internal_lookup:
                            continue
                    try:
                        display_name = self._get_sandbox_display_name(internal_name)
                    except Exception:
                        display_name = ""
                    normalized_name = str(display_name or name).strip()
                    if not normalized_name:
                        continue
                if normalized_name not in normalized:
                    normalized.append(normalized_name)
        return normalized

    def _normalize_volume_memory(self, volume_memory):
        normalized = {}
        if isinstance(volume_memory, dict):
            for sandbox_name, level in volume_memory.items():
                name = self._get_volume_memory_key(sandbox_name)
                if not name:
                    continue
                try:
                    normalized_level = int(round(float(level)))
                except Exception:
                    continue
                normalized_level = max(0, min(100, normalized_level))
                normalized[name] = normalized_level
        return normalized

    def _normalize_app_history(self, app_history, limit=None):
        normalized = []
        seen = set()
        if isinstance(app_history, (list, tuple)):
            for item in app_history:
                path = ""
                note = ""
                if isinstance(item, dict):
                    path = str(item.get("path", "") or "").strip()
                    note = str(item.get("note", "") or "").strip()
                elif isinstance(item, str):
                    path = str(item or "").strip()
                if not path:
                    continue
                try:
                    path_key = os.path.normcase(os.path.abspath(path))
                except Exception:
                    path_key = os.path.normcase(path)
                if path_key in seen:
                    continue
                seen.add(path_key)
                normalized.append({
                    "path": path,
                    "note": note or os.path.basename(path)
                })
        try:
            max_items = None if limit is None else max(0, int(limit))
        except Exception:
            max_items = None
        if max_items is not None:
            normalized = normalized[:max_items]
        return normalized

    def get_group_names(self):
        return sorted(self.sandbox_groups.keys())

    def get_group_members(self, group_name):
        return list(self.sandbox_groups.get(group_name, []))

    def get_pinned_sandboxes(self):
        return list(self.pinned_sandboxes)

    def is_sandbox_pinned(self, sandbox_name):
        name = str(sandbox_name or "").strip()
        if not name:
            return False
        return name in self.pinned_sandboxes

    def on_toggle_pin_sandbox(self, sandbox_name):
        name = str(sandbox_name or "").strip()
        if not name:
            return
        if name in self.pinned_sandboxes:
            self.pinned_sandboxes = [item for item in self.pinned_sandboxes if item != name]
            self.lbl_status.SetLabel(f"已取消置顶: {name}")
        else:
            self.pinned_sandboxes = [name] + [item for item in self.pinned_sandboxes if item != name]
            self.lbl_status.SetLabel(f"已置顶: {name}")
        self.save_settings()
        if hasattr(self, "sandbox_list"):
            self._suppress_list_refresh_status(2.0)
            self.sandbox_list.on_refresh_sandbox_list(None)

    def get_grouped_members_set(self):
        grouped = set()
        for members in self.sandbox_groups.values():
            for name in members:
                if name:
                    grouped.add(name)
        return grouped

    def _prompt_group_name(self, title="分组名称", default_value=""):
        dlg = GroupNameDialog(self, title=title, default_value=default_value)
        try:
            if dlg.ShowModal() == wx.ID_OK:
                return dlg.value
            return ""
        finally:
            dlg.Destroy()

    def _prompt_group_choice(self, title, prompt, choices):
        options = list(choices or [])
        if not options:
            return ""
        dialog = wx.Dialog(self, title=title, size=(420, 360))
        panel = wx.Panel(dialog)
        sizer = wx.BoxSizer(wx.VERTICAL)
        sizer.Add(wx.StaticText(panel, label=str(prompt or "")), 0, wx.ALL, 10)
        list_box = wx.ListBox(panel, choices=options, style=wx.LB_SINGLE)
        if options:
            list_box.SetSelection(0)
        sizer.Add(list_box, 1, wx.EXPAND | wx.LEFT | wx.RIGHT, 10)
        btn_sizer = wx.BoxSizer(wx.HORIZONTAL)
        btn_sizer.AddStretchSpacer()
        btn_ok = wx.Button(panel, wx.ID_OK, "确定")
        btn_cancel = wx.Button(panel, wx.ID_CANCEL, "取消")
        btn_sizer.Add(btn_ok, 0, wx.RIGHT, 6)
        btn_sizer.Add(btn_cancel, 0)
        sizer.Add(btn_sizer, 0, wx.EXPAND | wx.ALL, 10)
        panel.SetSizer(sizer)
        dialog.Centre()
        try:
            if dialog.ShowModal() != wx.ID_OK:
                return ""
            idx = list_box.GetSelection()
            if idx == wx.NOT_FOUND:
                return ""
            return options[idx]
        finally:
            dialog.Destroy()

    def on_create_group(self, event):
        selected_entries = self.sandbox_list.get_checked_entry_texts()
        selected = self._resolve_launch_entries(selected_entries)
        if not selected:
            self.lbl_status.SetLabel("请先勾选要归类的沙盒用户")
            return
        if len(selected) < 2:
            self.lbl_status.SetLabel("请至少勾选 2 个用户再新建分组")
            return
        group_name = self._prompt_group_name("新建分组")
        if not group_name:
            return
        if group_name in self.sandbox_groups:
            self.lbl_status.SetLabel(f"分组已存在: {group_name}")
            return
        self.sandbox_groups[group_name] = selected
        self.save_settings()
        self.sandbox_list.current_group_view = ""
        self.sandbox_list.on_refresh_sandbox_list(None)
        self.lbl_status.SetLabel(f"已保存分组: {group_name}")

    def on_launch_group(self, event):
        if not self.sandbox_groups:
            self.lbl_status.SetLabel("当前没有已保存的分组")
            return
        choices = sorted(self.sandbox_groups.keys())
        group_name = self._prompt_group_choice("启动分组", "请选择要启动的分组", choices)
        if not group_name:
            return
        self._suppress_list_refresh_status(2.0)
        self._suppress_selection_status(2.0)
        sandboxes = self.sandbox_groups.get(group_name, [])
        if not sandboxes:
            self.lbl_status.SetLabel(f"分组为空: {group_name}")
            return
        valid_sandboxes = self._resolve_launch_entries(sandboxes)
        if not valid_sandboxes:
            self.lbl_status.SetLabel(f"分组内没有可用沙盒: {group_name}")
            return
        self._launch_selected_sandboxes(valid_sandboxes)

    def on_delete_group(self, event):
        if not self.sandbox_groups:
            self.lbl_status.SetLabel("当前没有已保存的分组")
            return
        choices = sorted(self.sandbox_groups.keys())
        group_name = self._prompt_group_choice("删除分组", "请选择要删除的分组", choices)
        if not group_name:
            return
        if group_name in self.sandbox_groups:
            del self.sandbox_groups[group_name]
            self.save_settings()
            if self.sandbox_list.current_group_view == group_name:
                self.sandbox_list.current_group_view = ""
            self.sandbox_list.on_refresh_sandbox_list(None)
            self.lbl_status.SetLabel(f"已删除分组: {group_name}")

    def _get_batch_target_sandboxes(self):
        entries = self.sandbox_list.get_checked_entry_texts()
        selected = self._resolve_launch_entries(entries)
        if not selected:
            current = self._get_selected_sandbox_name()
            if current:
                selected = [current]
        return selected

    def _build_batch_sandbox_names(self, prefix, start, count, digits):
        names = []
        prefix_value = str(prefix or "").strip()
        start_value = int(max(0, start))
        count_value = int(max(0, count))
        digits_value = int(max(0, digits))
        for i in range(count_value):
            index = start_value + i
            number_text = str(index).zfill(digits_value) if digits_value > 0 else str(index)
            names.append(f"{prefix_value}{number_text}")
        return names

    def _validate_batch_sandbox_names(self, names):
        errors = []
        seen = set()
        duplicates = []
        for raw_name in names:
            name = str(raw_name or "").strip()
            validation_error = get_sandbox_display_name_validation_error(name)
            if validation_error:
                errors.append(f"{validation_error}: {name or '(空)'}")
                continue
            key = name.lower()
            if key in seen and name not in duplicates:
                duplicates.append(name)
            seen.add(key)
        if duplicates:
            errors.append("名称重复: " + "、".join(duplicates))
        return errors

    def _prompt_batch_create_rule(self):
        dlg = BatchNameRuleDialog(
            self,
            title="批量创建沙盒用户",
            prompt="请输入批量创建规则：",
            default_prefix="",
            default_start=1,
            default_digits=0,
            default_count=1,
            fixed_count=None,
            digit_choice_values=[
                (0, "不加0"),
                (2, "前面加0（01）"),
            ]
        )
        try:
            if dlg.ShowModal() != wx.ID_OK:
                return None
            return dlg.result
        finally:
            dlg.Destroy()

    def _prompt_batch_rename_rule(self, target_count, default_prefix):
        dlg = BatchNameRuleDialog(
            self,
            title="批量重命名沙盒用户",
            prompt="将按当前勾选顺序依次重命名，请设置新命名规则：",
            default_prefix=default_prefix,
            default_start=1,
            default_digits=0,
            default_count=target_count,
            fixed_count=target_count
        )
        try:
            if dlg.ShowModal() != wx.ID_OK:
                return None
            return dlg.result
        finally:
            dlg.Destroy()

    def _get_sandboxie_ini_candidates(self):
        win_dir = os.environ.get("WINDIR", r"C:\Windows")
        search_dirs = [Path(win_dir)]
        sandboxie_path = str(getattr(self, "SANDBOXIE_PATH", "") or "").strip()
        if sandboxie_path:
            search_dirs.append(Path(sandboxie_path))
        candidates = []
        for filename in ("Sandboxie.ini", "Sandboxie-Plus.ini", "SandboxiePlus.ini"):
            for directory in search_dirs:
                candidate = directory / filename
                if candidate not in candidates:
                    candidates.append(candidate)
        return candidates

    def _get_sandboxie_ini_path(self):
        candidates = self._get_sandboxie_ini_candidates()
        for candidate in candidates:
            if candidate.exists():
                return candidate
        return candidates[0]

    def _read_text_with_encoding(self, path_obj):
        path = Path(path_obj)
        if not path.exists():
            return "", "utf-8"
        data = path.read_bytes()
        if data.startswith(codecs.BOM_UTF8):
            return data.decode("utf-8-sig"), "utf-8-sig"
        for encoding in ("utf-8", "utf-16", "gbk", "mbcs"):
            try:
                return data.decode(encoding), encoding
            except Exception:
                continue
        return data.decode("utf-8", errors="ignore"), "utf-8"

    def _write_text_with_encoding(self, path_obj, text, encoding):
        path = Path(path_obj)
        path.parent.mkdir(parents=True, exist_ok=True)
        with open(path, "w", encoding=encoding, newline="") as f:
            f.write(str(text or ""))

    def _backup_sandboxie_ini_before_write(self, ini_path):
        path = Path(ini_path)
        if not path.exists():
            return ""
        timestamp = datetime.datetime.now().strftime("%Y%m%d_%H%M%S")
        backup_path = path.with_name(f"{path.name}.沙河批量打开器备份_{timestamp}.bak")
        shutil.copy2(str(path), str(backup_path))
        return str(backup_path)

    def _split_ini_sections(self, text):
        content = str(text or "")
        header_pattern = re.compile(r"(?m)^[ \t]*\[(?P<name>[^\]\r\n]+)\][^\r\n]*(?:\r?\n)?")
        matches = list(header_pattern.finditer(content))
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

    def _replace_section_header_name(self, block, new_name):
        pattern = re.compile(r"(?m)^([ \t]*)\[[^\]\r\n]+\]([^\r\n]*)(\r?\n)?")
        return pattern.sub(
            lambda m: f"{m.group(1)}[{new_name}]{m.group(2)}{m.group(3) or ''}",
            block,
            count=1
        )

    def _set_ini_setting_in_block(self, block, setting_name, value, line_sep):
        setting = str(setting_name or "").strip()
        if not setting:
            return str(block or ""), False
        text = str(block or "")
        sep = "\r\n" if line_sep == "\r\n" else "\n"
        lines = text.splitlines(keepends=True)
        pattern = re.compile(rf"^([ \t]*){re.escape(setting)}([ \t]*=)", re.IGNORECASE)
        changed = False
        found = False
        if value is None or str(value) == "":
            new_lines = []
            for line in lines:
                if pattern.match(line):
                    changed = True
                    found = True
                    continue
                new_lines.append(line)
            return "".join(new_lines), changed and found
        new_value_line = f"{setting}={value}{sep}"
        new_lines = []
        for line in lines:
            if pattern.match(line):
                if line != new_value_line:
                    changed = True
                    new_lines.append(new_value_line)
                else:
                    new_lines.append(line)
                found = True
            else:
                new_lines.append(line)
        if not found:
            insert_index = 1 if new_lines else 0
            new_lines.insert(insert_index, new_value_line)
            changed = True
        return "".join(new_lines), changed

    def _build_ini_section_block(self, sandbox_name, line_sep):
        sep = "\r\n" if line_sep == "\r\n" else "\n"
        return f"[{sandbox_name}]{sep}Enabled=y{sep}ConfigLevel=9{sep}{sep}"

    def _list_ini_section_names(self, sandbox_only=False):
        ini_path = self._get_sandboxie_ini_path()
        text, _ = self._read_text_with_encoding(ini_path)
        _, sections = self._split_ini_sections(text)
        if sandbox_only:
            return [name for name, block in sections if self._is_sandbox_config_section(name, block)]
        return [name for name, _ in sections]

    def _apply_ini_section_changes(self, add_names=None, remove_names=None, rename_map=None):
        add_list = [
            str(name).strip()
            for name in (add_names or [])
            if self._is_editable_sandbox_ini_section_name(name)
        ]
        remove_set = {
            str(name).strip().lower()
            for name in (remove_names or [])
            if self._is_editable_sandbox_ini_section_name(name)
        }
        rename_lookup = {}
        for old_name, new_name in (rename_map or {}).items():
            old_value = str(old_name or "").strip()
            new_value = str(new_name or "").strip()
            if self._is_editable_sandbox_ini_section_name(old_value) and self._is_editable_sandbox_ini_section_name(new_value):
                rename_lookup[old_value.lower()] = new_value

        ini_path = self._get_sandboxie_ini_path()
        text, encoding = self._read_text_with_encoding(ini_path)
        line_sep = "\r\n" if "\r\n" in text else "\n"
        preamble, sections = self._split_ini_sections(text)

        changed = False
        used_names = set()
        rebuilt_blocks = []

        for section_name, block in sections:
            key = section_name.lower()
            if not self._is_editable_sandbox_ini_section_name(section_name):
                rebuilt_blocks.append((section_name, block))
                used_names.add(key)
                continue
            if key in remove_set:
                changed = True
                continue
            target_name = rename_lookup.get(key, section_name)
            target_key = target_name.lower()
            if target_key in used_names:
                changed = True
                continue
            if target_name != section_name:
                block = self._replace_section_header_name(block, target_name)
                changed = True
            rebuilt_blocks.append((target_name, block))
            used_names.add(target_key)

        for add_name in add_list:
            key = add_name.lower()
            if key in used_names:
                continue
            rebuilt_blocks.append((add_name, self._build_ini_section_block(add_name, line_sep)))
            used_names.add(key)
            changed = True

        new_body = ""
        for _, block in rebuilt_blocks:
            if block.endswith("\r\n") or block.endswith("\n"):
                new_body += block
            else:
                new_body += block + line_sep
        new_text = preamble + new_body

        if not changed and ini_path.exists():
            return False, ""
        if new_text != text or (changed and not ini_path.exists()):
            try:
                self._backup_sandboxie_ini_before_write(ini_path)
            except Exception:
                pass
            self._write_text_with_encoding(ini_path, new_text, encoding)
            return True, ""
        return changed, ""

    def _clean_user_settings_block(self, block, line_sep):
        sep = "\r\n" if line_sep == "\r\n" else "\n"
        lines = str(block or "").splitlines(keepends=True)
        remove_names = {
            "sbiectrl_windowcoords",
            "enabled",
            "configlevel",
            "boxalias",
            "boxaliasdisabled",
        }
        new_lines = []
        removed = []
        for line in lines:
            stripped = str(line or "").strip()
            if stripped and not stripped.startswith("#") and "=" in stripped:
                key = stripped.split("=", 1)[0].strip().lower()
                if key in remove_names:
                    removed.append(key)
                    continue
            new_lines.append(line)
        if new_lines and not (new_lines[-1].endswith("\r\n") or new_lines[-1].endswith("\n")):
            new_lines[-1] = new_lines[-1] + sep
        return "".join(new_lines), removed

    def _repair_sandboxie_control_user_settings(self):
        ini_path = self._get_sandboxie_ini_path()
        text, encoding = self._read_text_with_encoding(ini_path)
        if not text:
            return False, "未找到 Sandboxie.ini 或配置为空"

        line_sep = "\r\n" if "\r\n" in text else "\n"
        preamble, sections = self._split_ini_sections(text)
        if not sections:
            return False, "Sandboxie.ini 中未找到配置段"

        changed = False
        touched_sections = []
        rebuilt = []
        removed_count = 0

        for section_name, block in sections:
            lower_name = str(section_name or "").strip().lower()
            if lower_name.startswith("usersettings_"):
                new_block, removed = self._clean_user_settings_block(block, line_sep)
                if removed:
                    changed = True
                    removed_count += len(removed)
                    touched_sections.append(section_name)
                    block = new_block
            rebuilt.append(block if (block.endswith("\r\n") or block.endswith("\n")) else block + line_sep)

        if not changed:
            return False, "未发现需要清理的 Sandboxie Control 用户设置"

        new_text = preamble + "".join(rebuilt)
        backup_path = ""
        try:
            backup_path = self._backup_sandboxie_ini_before_write(ini_path)
        except Exception:
            backup_path = ""
        self._write_text_with_encoding(ini_path, new_text, encoding)
        try:
            self._reload_sandboxie_config()
        except Exception:
            pass

        detail = f"已清理 {len(touched_sections)} 个 UserSettings 段中的 {removed_count} 条异常/易冲突记录"
        if backup_path:
            detail += f"\n已备份原配置：{backup_path}"
        return True, detail

    def _reload_sandboxie_config(self):
        result, _ = self._run_sandboxie_start(["/reload"], timeout=6)
        return result is not None and result.returncode == 0

    def _rename_sandbox_refs_in_settings(self, rename_map):
        changed = False
        if not rename_map:
            return False

        for group_name, members in list(self.sandbox_groups.items()):
            new_members = []
            seen = set()
            for member in members:
                target = rename_map.get(member, member)
                if target and target not in seen:
                    seen.add(target)
                    new_members.append(target)
            if new_members != members:
                self.sandbox_groups[group_name] = new_members
                changed = True

        new_pinned = []
        seen_pinned = set()
        for name in self.pinned_sandboxes:
            target_name = rename_map.get(name, name)
            if target_name and target_name not in seen_pinned:
                seen_pinned.add(target_name)
                new_pinned.append(target_name)
        if new_pinned != self.pinned_sandboxes:
            self.pinned_sandboxes = new_pinned
            changed = True

        new_volume_memory = {}
        for name, level in self.sandbox_volume_memory.items():
            target_name = rename_map.get(name, name)
            if target_name:
                new_volume_memory[target_name] = level
        if new_volume_memory != self.sandbox_volume_memory:
            self.sandbox_volume_memory = new_volume_memory
            changed = True

        new_mute_cache = {}
        for name, state in self._cached_mute_state_by_sandbox.items():
            target_name = rename_map.get(name, name)
            if target_name:
                new_mute_cache[target_name] = state
        if new_mute_cache != self._cached_mute_state_by_sandbox:
            self._cached_mute_state_by_sandbox = new_mute_cache
            changed = True
        self._cached_mute_state_all = None
        return changed

    def _rename_sandbox_refs_in_settings_by_display(self, display_rename_map):
        changed = False
        if not display_rename_map:
            return False
        lookup = {
            str(old or "").strip().lower(): str(new or "").strip()
            for old, new in display_rename_map.items()
            if str(old or "").strip() and str(new or "").strip()
        }
        if not lookup:
            return False

        for group_name, members in list(self.sandbox_groups.items()):
            new_members = []
            seen = set()
            for member in members:
                target = lookup.get(str(member or "").strip().lower(), member)
                if target and target not in seen:
                    seen.add(target)
                    new_members.append(target)
            if new_members != members:
                self.sandbox_groups[group_name] = new_members
                changed = True

        new_pinned = []
        seen_pinned = set()
        for name in self.pinned_sandboxes:
            target_name = lookup.get(str(name or "").strip().lower(), name)
            if target_name and target_name not in seen_pinned:
                seen_pinned.add(target_name)
                new_pinned.append(target_name)
        if new_pinned != self.pinned_sandboxes:
            self.pinned_sandboxes = new_pinned
            changed = True

        new_volume_memory = {}
        for name, level in self.sandbox_volume_memory.items():
            target_name = lookup.get(str(name or "").strip().lower(), name)
            if target_name:
                new_volume_memory[target_name] = level
        if new_volume_memory != self.sandbox_volume_memory:
            self.sandbox_volume_memory = new_volume_memory
            changed = True
        return changed

    def _delete_sandbox_refs_in_settings(self, names_to_delete):
        changed = False
        delete_set = {str(name).strip() for name in (names_to_delete or []) if str(name).strip()}
        if not delete_set:
            return False
        delete_lookup = {name.lower() for name in delete_set}

        for group_name, members in list(self.sandbox_groups.items()):
            new_members = [member for member in members if member not in delete_set and str(member or "").strip().lower() not in delete_lookup]
            if new_members != members:
                if new_members:
                    self.sandbox_groups[group_name] = new_members
                else:
                    del self.sandbox_groups[group_name]
                changed = True

        new_pinned = [name for name in self.pinned_sandboxes if name not in delete_set and str(name or "").strip().lower() not in delete_lookup]
        if new_pinned != self.pinned_sandboxes:
            self.pinned_sandboxes = new_pinned
            changed = True

        for name in list(self.sandbox_volume_memory.keys()):
            if name in delete_set or str(name or "").strip().lower() in delete_lookup:
                del self.sandbox_volume_memory[name]
                changed = True

        for name in list(self._cached_mute_state_by_sandbox.keys()):
            if name in delete_set or str(name or "").strip().lower() in delete_lookup:
                del self._cached_mute_state_by_sandbox[name]
                changed = True
        self._cached_mute_state_all = None
        return changed

    def _delete_sandbox_refs_in_settings_by_display(self, names_to_delete):
        display_set = {str(name).strip() for name in (names_to_delete or []) if str(name).strip()}
        if not display_set:
            return False
        display_lookup = {name.lower() for name in display_set}
        changed = False

        for group_name, members in list(self.sandbox_groups.items()):
            new_members = [member for member in members if str(member or "").strip().lower() not in display_lookup]
            if new_members != members:
                if new_members:
                    self.sandbox_groups[group_name] = new_members
                else:
                    del self.sandbox_groups[group_name]
                changed = True

        new_pinned = [name for name in self.pinned_sandboxes if str(name or "").strip().lower() not in display_lookup]
        if new_pinned != self.pinned_sandboxes:
            self.pinned_sandboxes = new_pinned
            changed = True

        for name in list(self.sandbox_volume_memory.keys()):
            if str(name or "").strip().lower() in display_lookup:
                del self.sandbox_volume_memory[name]
                changed = True
        return changed

    def _rename_sandbox_refs_in_schedule_jobs_by_display(self, display_rename_map):
        if not display_rename_map:
            return False
        lookup = {
            str(old or "").strip().lower(): str(new or "").strip()
            for old, new in display_rename_map.items()
            if str(old or "").strip() and str(new or "").strip()
        }
        if not lookup:
            return False
        changed = False
        for task in getattr(self, "_schedule_jobs", {}).values():
            targets = task.get("sandbox_targets")
            if not isinstance(targets, list):
                continue
            new_targets = []
            seen = set()
            for name in targets:
                target_name = lookup.get(str(name or "").strip().lower(), name)
                if target_name and target_name not in seen:
                    seen.add(target_name)
                    new_targets.append(target_name)
            if new_targets != targets:
                targets[:] = new_targets
                changed = True
            if targets:
                task_key = str(task.get("description", "") or "")
                if "定时启动" in task_key:
                    task["description"] = f"定时启动 {len(targets)} 个沙盒"
                elif "定时关闭" in task_key:
                    task["description"] = f"定时关闭 {len(targets)} 个沙盒"
        return changed

    def _delete_sandbox_refs_in_schedule_jobs_by_display(self, names_to_delete):
        display_set = {str(name).strip() for name in (names_to_delete or []) if str(name).strip()}
        if not display_set:
            return False
        display_lookup = {name.lower() for name in display_set}
        changed = False
        empty_task_keys = []
        for task_key, task in getattr(self, "_schedule_jobs", {}).items():
            targets = task.get("sandbox_targets")
            if not isinstance(targets, list):
                continue
            new_targets = [
                name for name in targets
                if str(name or "").strip().lower() not in display_lookup
            ]
            if new_targets != targets:
                targets[:] = new_targets
                changed = True
            if not targets:
                empty_task_keys.append(task_key)
                continue
            desc = str(task.get("description", "") or "")
            if "定时启动" in desc:
                task["description"] = f"定时启动 {len(targets)} 个沙盒"
            elif "定时关闭" in desc:
                task["description"] = f"定时关闭 {len(targets)} 个沙盒"
        for task_key in empty_task_keys:
            self._cancel_schedule_task(task_key, update_status=False)
        return changed

    def _rename_sandbox_directories(self, rename_map):
        if not rename_map:
            return True, ""
        root = Path(self.sandbox_root)
        root.mkdir(parents=True, exist_ok=True)
        temp_map = {}
        finalized = []
        seed = int(time.time() * 1000)
        try:
            for idx, (old_name, _) in enumerate(rename_map.items()):
                old_path = root / old_name
                if not old_path.exists():
                    raise FileNotFoundError(f"未找到沙盒目录: {old_name}")
                temp_name = f"__sbx_tmp_{seed}_{idx}"
                temp_path = root / temp_name
                suffix = 0
                while temp_path.exists():
                    suffix += 1
                    temp_name = f"__sbx_tmp_{seed}_{idx}_{suffix}"
                    temp_path = root / temp_name
                os.rename(old_path, temp_path)
                temp_map[old_name] = temp_name

            for old_name, new_name in rename_map.items():
                temp_path = root / temp_map[old_name]
                new_path = root / new_name
                os.rename(temp_path, new_path)
                finalized.append((old_name, new_name))
            return True, ""
        except Exception as e:
            for old_name, new_name in reversed(finalized):
                new_path = root / new_name
                old_path = root / old_name
                if new_path.exists() and not old_path.exists():
                    try:
                        os.rename(new_path, old_path)
                    except Exception:
                        pass
            for old_name, temp_name in temp_map.items():
                temp_path = root / temp_name
                old_path = root / old_name
                if temp_path.exists() and not old_path.exists():
                    try:
                        os.rename(temp_path, old_path)
                    except Exception:
                        pass
            return False, str(e)

    def _apply_sandbox_aliases(self, alias_map):
        alias_changes = {
            str(name or "").strip(): str(alias or "").strip()
            for name, alias in (alias_map or {}).items()
            if str(name or "").strip()
        }
        if not alias_changes:
            return False, ""

        any_changed = False
        failed = []
        for internal_name, alias in alias_changes.items():
            if self._set_box_alias_via_official(internal_name, alias):
                any_changed = True
            else:
                failed.append(internal_name)
        if failed:
            preview = "、".join(failed[:8])
            if len(failed) > 8:
                preview += f" 等 {len(failed)} 个沙盒"
            return any_changed, f"以下沙盒别名更新失败：{preview}"
        return any_changed, ""

    def _safe_delete_directory(self, dir_path):
        path = Path(dir_path)
        if not path.exists():
            return True
        try:
            def _on_rm_error(func, target_path, exc_info):
                try:
                    os.chmod(target_path, 0o777)
                    func(target_path)
                except Exception:
                    pass
            shutil.rmtree(path, ignore_errors=False, onerror=_on_rm_error)
            return not path.exists()
        except Exception:
            return False

    def _delete_registry_tree(self, hive, subkey):
        key_path = str(subkey or "").strip()
        if not key_path:
            return True

        def _delete_children(root_hive, current_path):
            try:
                with winreg.OpenKey(root_hive, current_path, 0, winreg.KEY_READ | winreg.KEY_WRITE) as key:
                    children = []
                    index = 0
                    while True:
                        try:
                            child_name = winreg.EnumKey(key, index)
                            children.append(child_name)
                            index += 1
                        except OSError:
                            break
                for child_name in children:
                    child_path = f"{current_path}\\{child_name}"
                    _delete_children(root_hive, child_path)
                winreg.DeleteKey(root_hive, current_path)
                return True
            except FileNotFoundError:
                return True
            except Exception:
                return False

        return _delete_children(hive, key_path)

    def _cleanup_sandbox_registry_entries(self, sandbox_names):
        names = [str(name).strip() for name in (sandbox_names or []) if str(name).strip()]
        if not names:
            return 0, []

        base_paths = [
            (winreg.HKEY_CURRENT_USER, r"Software\Sandboxie\Boxes"),
            (winreg.HKEY_CURRENT_USER, r"Software\Sandboxie-Plus\Boxes"),
            (winreg.HKEY_CURRENT_USER, r"Software\SandboxiePlus\Boxes"),
            (winreg.HKEY_LOCAL_MACHINE, r"SOFTWARE\Sandboxie\Boxes"),
            (winreg.HKEY_LOCAL_MACHINE, r"SOFTWARE\Sandboxie-Plus\Boxes"),
            (winreg.HKEY_LOCAL_MACHINE, r"SOFTWARE\SandboxiePlus\Boxes"),
            (winreg.HKEY_LOCAL_MACHINE, r"SOFTWARE\WOW6432Node\Sandboxie\Boxes"),
            (winreg.HKEY_LOCAL_MACHINE, r"SOFTWARE\WOW6432Node\Sandboxie-Plus\Boxes"),
            (winreg.HKEY_LOCAL_MACHINE, r"SOFTWARE\WOW6432Node\SandboxiePlus\Boxes"),
        ]

        cleaned = 0
        failed = []
        for sandbox_name in names:
            for hive, base_path in base_paths:
                target_path = f"{base_path}\\{sandbox_name}"
                ok = self._delete_registry_tree(hive, target_path)
                if ok:
                    cleaned += 1
                else:
                    failed.append(target_path)
        return cleaned, failed

    def _ensure_sandbox_stopped_before_delete(self, sandbox_name, timeout_seconds=6.0):
        name = str(sandbox_name or "").strip()
        if not name:
            return True
        try:
            initial_pids = self._get_sandbox_pids(name)
        except Exception:
            initial_pids = []
        tracked_pids = sorted(self._normalize_pid_set(initial_pids))
        if not tracked_pids:
            return True

        # 先尝试通过 Sandboxie 官方命令结束沙盒进程，再按已捕获 PID 实际校验是否退出。
        try:
            self._terminate_sandbox_via_start(name)
        except Exception:
            pass

        remaining_pids = self._wait_for_pids_exit(
            tracked_pids,
            timeout=max(1.0, float(timeout_seconds)),
            interval=0.15,
        )
        if not remaining_pids:
            return True

        try:
            self._terminate_pids(remaining_pids)
        except Exception:
            pass
        remaining_pids = self._wait_for_pids_exit(remaining_pids, timeout=3.0, interval=0.12)
        return not bool(remaining_pids)

    def on_batch_create_sandboxes(self, event):
        rule = self._prompt_batch_create_rule()
        if not rule:
            return
        names = self._build_batch_sandbox_names(
            rule["prefix"],
            rule["start"],
            rule["count"],
            rule["digits"]
        )
        errors = self._validate_batch_sandbox_names(names)
        if errors:
            wx.MessageBox("\n".join(errors), "名称校验失败", wx.OK | wx.ICON_WARNING, self)
            return

        try:
            os.makedirs(self.sandbox_root, exist_ok=True)
        except Exception as e:
            wx.MessageBox(f"创建沙盒根目录失败: {str(e)}", "创建失败", wx.OK | wx.ICON_ERROR, self)
            return

        existing_display_lower = {
            str(name or "").strip().lower()
            for name in self._get_existing_sandbox_display_names()
            if str(name or "").strip()
        }
        existing_internal_lower = {
            str(name or "").strip().lower()
            for name in self._get_existing_sandbox_names()
            if str(name or "").strip()
        }
        create_targets = [
            name for name in names
            if str(name or "").strip().lower() not in existing_display_lower
            and str(name or "").strip().lower() not in existing_internal_lower
        ]
        skipped = [
            name for name in names
            if str(name or "").strip().lower() in existing_display_lower
            or str(name or "").strip().lower() in existing_internal_lower
        ]
        if not create_targets:
            self.lbl_status.SetLabel("目标沙盒用户已存在，无需创建")
            return

        created = []
        failed = []
        total = len(create_targets)
        for idx, name in enumerate(create_targets, start=1):
            self._set_status_label_immediately(f"正在创建沙盒用户 {idx}/{total}: {name}")
            ok, err = self._create_sandbox_via_official(name)
            if ok:
                created.append(name)
            else:
                failed.append((name, err or "创建失败"))

        if hasattr(self, "sandbox_list"):
            self.sandbox_list.on_refresh_sandbox_list(None)

        if failed:
            fail_lines = [f"{name}: {reason}" for name, reason in failed[:8]]
            message = "以下沙盒用户创建失败：\n" + "\n".join(fail_lines)
            if len(failed) > 8:
                message += f"\n... 其余 {len(failed) - 8} 项已省略"
            wx.MessageBox(message, "批量创建结果", wx.OK | wx.ICON_WARNING, self)

        status_parts = [f"已创建 {len(created)} 个沙盒用户"]
        if skipped:
            status_parts.append(f"跳过 {len(skipped)} 个已存在")
        if failed:
            status_parts.append(f"失败 {len(failed)} 个")
        self.lbl_status.SetLabel("，".join(status_parts))

    def on_batch_rename_sandboxes(self, event):
        selected = self._get_batch_target_sandboxes()
        if not selected:
            self.lbl_status.SetLabel("请先勾选或选中要重命名的沙盒用户")
            return
        default_prefix = make_default_sandbox_display_prefix(selected[0])
        rule = self._prompt_batch_rename_rule(len(selected), default_prefix)
        if not rule:
            return

        target_names = self._build_batch_sandbox_names(
            rule["prefix"],
            rule["start"],
            rule["count"],
            rule["digits"]
        )
        errors = self._validate_batch_sandbox_names(target_names)
        if errors:
            wx.MessageBox("\n".join(errors), "名称校验失败", wx.OK | wx.ICON_WARNING, self)
            return

        old_set = set(selected)
        existing_displays = self._get_existing_sandbox_display_names()
        existing_display_lower = {str(name).lower() for name in existing_displays}
        old_display_lower = {str(name).lower() for name in old_set}
        collisions = [name for name in target_names if name.lower() in existing_display_lower and name.lower() not in old_display_lower]
        if collisions:
            wx.MessageBox(
                "以下目标名称已存在，无法重命名：\n" + "\n".join(collisions[:12]),
                "重命名冲突",
                wx.OK | wx.ICON_WARNING,
                self
            )
            return

        rename_map = {}
        for old_name, new_name in zip(selected, target_names):
            if old_name != new_name:
                rename_map[old_name] = new_name
        if not rename_map:
            self.lbl_status.SetLabel("目标名称与当前名称一致，无需重命名")
            return

        internal_rename_map = {}
        alias_map = {}
        reserved_internal = set()
        existing_internal_lower = {str(name).lower() for name in self._get_existing_sandbox_names()}
        selected_internal_lower = set()
        for display_name in selected:
            internal_name = self._normalize_sandbox_name(display_name)
            if internal_name:
                selected_internal_lower.add(internal_name.lower())

        for old_display, new_display in rename_map.items():
            old_internal = self._normalize_sandbox_name(old_display)
            if not old_internal:
                continue
            target_internal = self._display_name_to_internal_candidate(new_display)
            can_rename_internal = (
                is_valid_internal_sandbox_name(target_internal)
                and (
                    target_internal.lower() in selected_internal_lower
                    or target_internal.lower() not in existing_internal_lower
                )
                and target_internal.lower() not in {name.lower() for name in reserved_internal}
            )
            if can_rename_internal and target_internal.lower() != old_internal.lower():
                internal_rename_map[old_internal] = target_internal
                reserved_internal.add(target_internal)
                effective_internal = target_internal
            else:
                effective_internal = old_internal
            default_display = self._display_name_for_sandbox(effective_internal, {})
            alias_map[effective_internal] = "" if default_display == new_display else new_display

        if internal_rename_map:
            running_targets = []
            for old_display in rename_map.keys():
                old_internal = self._normalize_sandbox_name(old_display)
                if not old_internal or old_internal not in internal_rename_map:
                    continue
                if self._get_target_pids_for_sandbox(old_internal):
                    running_targets.append(old_display)
            if running_targets:
                wx.MessageBox(
                    "以下沙盒仍在运行，请先关闭后再重命名：\n" + "\n".join(running_targets[:12]),
                    "重命名失败",
                    wx.OK | wx.ICON_WARNING,
                    self
                )
                self.lbl_status.SetLabel("批量重命名失败")
                return
            ini_sections_lower = {name.lower() for name in self._list_ini_section_names(sandbox_only=True)}
            selected_lower = {name.lower() for name in internal_rename_map.keys()}
            ini_collisions = [
                name for name in internal_rename_map.values()
                if name.lower() in ini_sections_lower and name.lower() not in selected_lower
            ]
            if ini_collisions:
                wx.MessageBox(
                    "Sandboxie.ini 中已存在以下同名分节，无法重命名：\n" + "\n".join(ini_collisions[:12]),
                    "INI 冲突",
                    wx.OK | wx.ICON_WARNING,
                    self
                )
                return

            ok, err = self._rename_sandbox_directories(internal_rename_map)
            if not ok:
                wx.MessageBox(f"批量重命名目录失败：{err}", "重命名失败", wx.OK | wx.ICON_ERROR, self)
                self.lbl_status.SetLabel("批量重命名失败")
                return

        ini_error = ""
        try:
            ini_changed, ini_error = self._apply_ini_section_changes(
                rename_map=internal_rename_map,
                add_names=list(internal_rename_map.values())
            )
            if ini_changed:
                self._reload_sandboxie_config()
        except Exception as e:
            ini_error = str(e)

        alias_changed, alias_error = self._apply_sandbox_aliases(alias_map)
        if alias_changed:
            self._reload_sandboxie_config()
        if alias_error and not ini_error:
            ini_error = alias_error

        refs_changed = self._rename_sandbox_refs_in_settings_by_display(rename_map)
        self._rename_sandbox_refs_in_schedule_jobs_by_display(rename_map)
        self._rename_sandbox_refs_in_active_launch_by_display(rename_map)
        if internal_rename_map:
            refs_changed = self._rename_sandbox_refs_in_settings(internal_rename_map) or refs_changed
        if refs_changed:
            self.save_settings()

        try:
            self._refresh_sandbox_display_maps()
        except Exception:
            pass

        if hasattr(self, "sandbox_list"):
            self._suppress_selection_status(2.0)
            self.sandbox_list.on_refresh_sandbox_list(None)

        if ini_error:
            wx.MessageBox(f"目录已重命名，但 Sandboxie.ini 更新失败：{ini_error}", "重命名提示", wx.OK | wx.ICON_WARNING, self)
        self.lbl_status.SetLabel(f"已批量重命名 {len(rename_map)} 个沙盒用户")

    def on_batch_delete_sandboxes(self, event):
        selected = self._get_batch_target_sandboxes()
        if not selected:
            self.lbl_status.SetLabel("请先勾选或选中要删除的沙盒用户")
            return

        preview = "\n".join(selected[:12])
        if len(selected) > 12:
            preview += f"\n... 其余 {len(selected) - 12} 项已省略"
        message = f"确定要删除以下沙盒用户吗？\n\n{preview}\n\n该操作不可恢复。"
        if wx.MessageBox(message, "确认批量删除", wx.YES_NO | wx.ICON_QUESTION, self) != wx.YES:
            return
        self._delete_sandbox_refs_in_active_launch_by_display(selected)

        blocked_running = []
        for idx, sandbox_name in enumerate(selected, start=1):
            self._set_status_label_immediately(f"正在检查并终止运行进程 {idx}/{len(selected)}: {sandbox_name}")
            if not self._ensure_sandbox_stopped_before_delete(sandbox_name):
                blocked_running.append(sandbox_name)

        deleted = []
        deleted_internal = []
        failed = []
        failed_details = []
        candidates = [name for name in selected if name not in blocked_running]
        for idx, sandbox_name in enumerate(candidates, start=1):
            self._set_status_label_immediately(f"正在删除沙盒用户 {idx}/{len(candidates)}: {sandbox_name}")
            internal_name = self._normalize_sandbox_name(sandbox_name)
            ok, err = self._delete_sandbox_via_official(sandbox_name)
            target_dir = os.path.join(self.sandbox_root, internal_name or sandbox_name)
            dir_deleted = self._safe_delete_directory(target_dir)
            if not dir_deleted:
                ok = False
                if not err:
                    err = "目录删除失败"
            if ok:
                deleted.append(sandbox_name)
                if internal_name:
                    deleted_internal.append(internal_name)
            else:
                failed.append(sandbox_name)
                failed_details.append((sandbox_name, err or "删除失败"))

        refs_changed = self._delete_sandbox_refs_in_settings_by_display(set(deleted))
        self._delete_sandbox_refs_in_schedule_jobs_by_display(set(deleted))
        if deleted_internal:
            refs_changed = self._delete_sandbox_refs_in_settings(set(deleted_internal)) or refs_changed
        if refs_changed:
            self.save_settings()

        reg_failed = []
        if deleted_internal or deleted:
            try:
                _, reg_failed = self._cleanup_sandbox_registry_entries(deleted_internal or deleted)
            except Exception:
                reg_failed = []

        if hasattr(self, "sandbox_list"):
            self._suppress_selection_status(2.0)
            self.sandbox_list.on_refresh_sandbox_list(None)

        if failed:
            detail_lines = [f"{name}: {reason}" for name, reason in failed_details[:12]]
            detail_text = "\n".join(detail_lines) if detail_lines else "\n".join(failed[:12])
            wx.MessageBox(
                "以下沙盒用户删除失败：\n" + detail_text,
                "批量删除结果",
                wx.OK | wx.ICON_WARNING,
                self
            )
        if blocked_running:
            wx.MessageBox(
                "以下沙盒仍在运行，已跳过删除：\n" + "\n".join(blocked_running[:12]),
                "批量删除结果",
                wx.OK | wx.ICON_WARNING,
                self
            )
        if reg_failed:
            preview = "\n".join(reg_failed[:12])
            wx.MessageBox(
                f"已删除目录，但以下注册表项清理失败：\n{preview}",
                "注册表清理提示",
                wx.OK | wx.ICON_WARNING,
                self
            )

        if deleted and not failed:
            self.lbl_status.SetLabel(f"已批量删除 {len(deleted)} 个沙盒用户")
        elif deleted and (failed or blocked_running):
            self.lbl_status.SetLabel(f"已删除 {len(deleted)} 个，失败 {len(failed)} 个，跳过运行中 {len(blocked_running)} 个")
        else:
            self.lbl_status.SetLabel("未删除任何沙盒用户")

    def on_context_menu(self, event):
        self.show_app_path_context_menu(event)

    def show_app_path_context_menu(self, event=None):
        if hasattr(self, "combo_app_path"):
            try:
                pos = event.GetPosition() if event is not None else wx.DefaultPosition
            except Exception:
                pos = wx.DefaultPosition
            if pos != wx.DefaultPosition:
                try:
                    item, flags = self.combo_app_path.HitTest(self.combo_app_path.ScreenToClient(pos))
                    if item != wx.NOT_FOUND and flags & wx.LIST_HITTEST_ONITEM:
                        current = self.combo_app_path.GetFirstSelected()
                        if current != -1 and current != item:
                            self.combo_app_path.Select(current, 0)
                        self.combo_app_path.Select(item)
                        self.combo_app_path.Focus(item)
                        self.combo_app_path.EnsureVisible(item)
                except Exception:
                    pass

        has_history = bool(getattr(self, "app_history", []))
        has_current = bool(self.combo_app_path.GetValue())
        menu = wx.Menu()
        open_app_item = menu.Append(wx.ID_ANY, self._menu_label("选择应用", "open_app"), "选择要在沙盒中运行的应用")
        self.Bind(wx.EVT_MENU, self.on_menu_open_app, open_app_item)
        paste_app_item = menu.Append(wx.ID_ANY, self._menu_label("粘贴 .exe 或文件路径", "paste_app"), "从剪贴板粘贴应用路径")
        self.Bind(wx.EVT_MENU, self.on_menu_paste_app, paste_app_item)
        menu.AppendSeparator()
        edit_note_item = menu.Append(wx.ID_ANY, self._menu_label("路径记忆备注", "memory_edit_note"), "编辑当前路径的备注")
        self.Bind(wx.EVT_MENU, self.on_edit_note, edit_note_item)
        edit_note_item.Enable(has_current)
        adjust_count_item = menu.Append(wx.ID_ANY, self._menu_label("调整记忆数", "memory_adjust_count"), "调整路径记忆数量")
        self.Bind(wx.EVT_MENU, self.on_adjust_count, adjust_count_item)
        toggle_label = "隐藏路径" if not self.hide_path else "显示路径"
        toggle_path_item = menu.Append(wx.ID_ANY, self._menu_label(toggle_label, "memory_toggle_path_display"))
        self.Bind(wx.EVT_MENU, self.on_toggle_path_display, toggle_path_item)
        menu.AppendSeparator()
        delete_current_item = menu.Append(wx.ID_ANY, self._menu_label("删除当前记忆", "memory_delete_current"), "从记忆列表中删除当前路径")
        self.Bind(wx.EVT_MENU, self.on_delete_current, delete_current_item)
        delete_current_item.Enable(has_current)
        clear_memory_item = menu.Append(wx.ID_ANY, self._menu_label("清空所有路径记忆", "memory_clear_all"), "清空所有路径记忆")
        self.Bind(wx.EVT_MENU, self.on_clear_memory, clear_memory_item)
        clear_memory_item.Enable(has_history)
        menu.AppendSeparator()
        copy_path_item = menu.Append(wx.ID_ANY, self._menu_label("复制完整路径", "memory_copy_path"), "复制完整路径到剪贴板")
        self.Bind(wx.EVT_MENU, self.on_copy_path, copy_path_item)
        copy_path_item.Enable(has_current)
        self.combo_app_path.PopupMenu(menu)
        menu.Destroy()

    def on_edit_note(self, event):
        current_text = self.combo_app_path.GetValue()
        if not current_text:
            return
        current_index = self.combo_app_path.GetFirstSelected() if hasattr(self, "combo_app_path") else -1
        current_path, current_note = self._resolve_current_app_path_and_note(current_text, preferred_index=current_index)
        if not current_path:
            return
        dialog = wx.Dialog(self, title="路径记忆备注", size=(400, 150))
        panel = wx.Panel(dialog)
        sizer = wx.BoxSizer(wx.VERTICAL)
        sizer.Add(wx.StaticText(panel, label="请输入路径备注:"), 0, wx.ALL, 10)
        text_note = wx.TextCtrl(panel, value=current_note, size=(300, -1))
        sizer.Add(text_note, 0, wx.LEFT | wx.RIGHT | wx.BOTTOM, 10)
        btn_sizer = wx.BoxSizer(wx.HORIZONTAL)
        btn_sizer.AddStretchSpacer()
        btn_ok = wx.Button(panel, label="确定")
        btn_ok.Bind(wx.EVT_BUTTON, lambda e: end_dialog_safely(dialog, wx.ID_OK))
        btn_sizer.Add(btn_ok, 0, wx.RIGHT, 5)
        btn_cancel = wx.Button(panel, label="取消")
        btn_cancel.Bind(wx.EVT_BUTTON, lambda e: end_dialog_safely(dialog, wx.ID_CANCEL))
        btn_sizer.Add(btn_cancel, 0)
        sizer.Add(btn_sizer, 0, wx.EXPAND | wx.ALL, 10)
        panel.SetSizer(sizer)
        dialog.Centre()
        if dialog.ShowModal() == wx.ID_OK:
            new_note = text_note.GetValue().strip()
            if new_note:
                target_index = self._find_app_history_index_by_path(current_path)
                if target_index == -1:
                    if not self.add_to_history(current_path, refresh=False):
                        self.lbl_status.SetLabel("路径记忆已关闭，备注未保存")
                        dialog.Destroy()
                        return
                    target_index = self._find_app_history_index_by_path(current_path)
                if target_index == -1:
                    self.lbl_status.SetLabel("备注保存失败")
                    dialog.Destroy()
                    return
                self.app_history[target_index]["note"] = new_note
                self.save_settings()
                self.update_combo_box()
                item = self._get_app_history_item_by_path(current_path)
                fallback_text = self.format_display_text(item) if item is not None else current_text
                self._set_combo_app_path_by_history_path(current_path, fallback_text)
                self.lbl_status.SetLabel("备注已更新")
        dialog.Destroy()

    def extract_path_and_note(self, text, preferred_index=None):
        preferred_path = self._get_app_list_path_by_index(preferred_index)
        if preferred_path:
            item = self._get_app_history_item_by_path(preferred_path)
            if item is not None:
                return item["path"], item["note"]
            return preferred_path, os.path.basename(preferred_path)
        if ' - "' in text and text.endswith('"'):
            parts = text.rsplit(' - "', 1)
            if len(parts) == 2:
                return parts[1][:-1], parts[0]
        else:
            for item in self.app_history:
                if self.format_display_text(item) == text:
                    return item["path"], item["note"]
            return text, os.path.basename(text)
        return None, None

    def on_adjust_count(self, event):
        dialog = wx.NumberEntryDialog(self, "请输入路径记忆数量:", "调整记忆数", "路径记忆数量", self.max_history_items, 0, 20)
        if dialog.ShowModal() == wx.ID_OK:
            new_count = dialog.GetValue()
            self.max_history_items = new_count
            self.settings["max_history_items"] = new_count
            self.save_settings()
            if len(self.app_history) > new_count:
                self.app_history = self.app_history[:new_count]
                self.save_settings()
                self.update_combo_box()
            self.lbl_status.SetLabel(f"路径记忆数量已设置为: {new_count}")
        dialog.Destroy()

    def on_clear_memory(self, event):
        if wx.MessageBox("确定要清空所有路径记忆吗？", "确认清空", wx.YES_NO | wx.ICON_QUESTION) == wx.YES:
            self.app_history.clear()
            self.save_settings()
            self.update_combo_box()
            self.combo_app_path.SetValue("")
            self.lbl_status.SetLabel("路径记忆已清空")

    def on_delete_current(self, event):
        current_text = self.combo_app_path.GetValue()
        if not current_text:
            return
        current_index = self.combo_app_path.GetFirstSelected() if hasattr(self, "combo_app_path") else -1
        current_path, _ = self._resolve_current_app_path_and_note(current_text, preferred_index=current_index)
        if not current_path:
            return
        if wx.MessageBox("确定要删除当前路径记忆吗？", "确认删除", wx.YES_NO | wx.ICON_QUESTION) == wx.YES:
            for i, item in enumerate(self.app_history):
                if item["path"] == current_path:
                    self.app_history.pop(i)
                    self.save_settings()
                    self.update_combo_box()
                    self.lbl_status.SetLabel("当前路径已从记忆列表中删除")
                    return
            self.lbl_status.SetLabel("当前路径不在记忆列表中")

    def on_toggle_path_display(self, event):
        self.hide_path = not self.hide_path
        self.settings["hide_path"] = self.hide_path
        self.save_settings()
        self.update_combo_box()
        self.lbl_status.SetLabel("已隐藏完整路径" if self.hide_path else "已显示完整路径")

    def on_copy_path(self, event):
        current_text = self.combo_app_path.GetValue()
        if not current_text:
            return
        current_index = self.combo_app_path.GetFirstSelected() if hasattr(self, "combo_app_path") else -1
        current_path, _ = self._resolve_current_app_path_and_note(current_text, preferred_index=current_index)
        if not current_path:
            current_path = current_text
        if not wx.TheClipboard.Open():
            self.lbl_status.SetLabel("无法访问剪贴板")
            return
        write_attempted = False
        try:
            write_attempted = bool(wx.TheClipboard.SetData(wx.TextDataObject(current_path)))
            if write_attempted and hasattr(wx.TheClipboard, "Flush"):
                try:
                    wx.TheClipboard.Flush()
                except Exception:
                    pass
        except Exception:
            write_attempted = False
        finally:
            try:
                wx.TheClipboard.Close()
            except Exception:
                pass
        copied = False
        if write_attempted:
            for _ in range(5):
                if not wx.TheClipboard.Open():
                    time.sleep(0.02)
                    continue
                try:
                    data = wx.TextDataObject()
                    if wx.TheClipboard.GetData(data) and data.GetText() == current_path:
                        copied = True
                        break
                except Exception:
                    pass
                finally:
                    try:
                        wx.TheClipboard.Close()
                    except Exception:
                        pass
                time.sleep(0.02)
        self.lbl_status.SetLabel("路径已复制到剪贴板" if copied else "复制路径失败")

    def on_combo_text_changed(self, event):
        if hasattr(self.combo_app_path, "is_programmatic_update") and self.combo_app_path.is_programmatic_update():
            event.Skip()
            return
        text = self.combo_app_path.GetValue()
        if text:
            current_index = self.combo_app_path.GetFirstSelected() if hasattr(self, "combo_app_path") else -1
            current_path, _ = self._resolve_current_app_path_and_note(text, preferred_index=current_index)
            if current_path and os.path.exists(current_path):
                self.APP_PATH = current_path
                self.APP_DIR = os.path.dirname(current_path)
                self.lbl_status.SetLabel(f"已选择: {os.path.basename(current_path)}")

    def on_app_item_checked_remember(self, event):
        if hasattr(self.combo_app_path, "is_programmatic_update") and self.combo_app_path.is_programmatic_update():
            event.Skip()
            return
        index = event.GetIndex()
        if index >= 0:
            text = self.combo_app_path.GetItemText(index)
            current_path, _ = self.extract_path_and_note(text, preferred_index=index)
            if current_path and os.path.exists(current_path):
                self.APP_PATH = current_path
                self.APP_DIR = os.path.dirname(current_path)
                self.add_to_history(current_path, refresh=False)
    def on_sandbox_toggled(self, selected_count):
        try:
            if getattr(self.sandbox_list, "_list_refresh_in_progress", False):
                return
        except Exception:
            pass
        self._user_interacted_with_sandbox_list = True
        if time.time() < getattr(self, "_suppress_selection_status_until", 0.0):
            return
        self.lbl_status.SetLabel(f"已选中 {selected_count} 个沙盒")
    def _suppress_selection_status(self, seconds=1.5):
        try:
            self._suppress_selection_status_until = max(
                getattr(self, "_suppress_selection_status_until", 0.0),
                time.time() + float(seconds)
            )
        except Exception:
            self._suppress_selection_status_until = time.time() + 1.5

    def on_memory_manager(self, event):
        dialog_history = [dict(item) for item in self.app_history if isinstance(item, dict)]
        dialog = PathMemoryManagerDialog(self, dialog_history, self.settings.copy(), self.on_memory_saved)
        dialog.ShowModal()
        dialog.Destroy()

    def on_memory_saved(self, app_history, settings):
        settings = settings if isinstance(settings, dict) else {}
        self.settings = settings
        self.max_history_items = parse_int_setting(settings.get("max_history_items", 10), 10, 0, 20)
        self.settings["max_history_items"] = self.max_history_items
        self.app_history = self._normalize_app_history(app_history, limit=self.max_history_items)
        self.hide_path = parse_bool_setting(settings.get("hide_path", False), False)
        self.settings["hide_path"] = self.hide_path
        self.save_settings()
        self.update_combo_box()
        self.lbl_status.SetLabel("路径记忆设置已保存")

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

    def on_browse_app(self, event):
        with wx.FileDialog(self, "选择要在沙盒中打开的应用", os.getcwd(), "",
                           "可执行文件 (*.exe)|*.exe", wx.FD_OPEN | wx.FD_FILE_MUST_EXIST) as dlg:
            if dlg.ShowModal() == wx.ID_OK:
                self.APP_PATH = dlg.GetPath()
                self.APP_DIR = os.path.dirname(self.APP_PATH)
                self.add_to_history(self.APP_PATH)
                self._set_combo_app_path_by_history_path(self.APP_PATH, self.APP_PATH)
                self._focus_app_path_list()
                self.lbl_status.SetLabel(f"已选择: {os.path.basename(self.APP_PATH)}")

    def _resolve_launch_entries(self, entries):
        try:
            self._refresh_sandbox_display_maps()
        except Exception:
            pass
        available_display = set(self.sandbox_list.scan_sandboxes())
        available_internal = {str(name or "").strip().lower() for name in available_display}
        try:
            available_internal.update(str(name or "").strip().lower() for name in self._sandbox_name_to_display.keys())
        except Exception:
            pass
        direct_entries = []
        seen_direct = set()
        for entry in entries or []:
            text = str(entry or "").strip()
            if not text:
                continue
            if text not in seen_direct:
                seen_direct.add(text)
                direct_entries.append(text)

        resolved = []

        def add_resolved_sandbox(name):
            internal_name = self._normalize_sandbox_name(name)
            if not internal_name:
                return
            if internal_name.lower() not in available_internal and str(name or "").strip() not in available_display:
                return
            display_name = self._get_sandbox_display_name(internal_name)
            if display_name and display_name not in resolved:
                resolved.append(display_name)

        for text in direct_entries:
            add_resolved_sandbox(text)
        return resolved

    def _prompt_schedule_delay_seconds(self, title, message, default_seconds=30):
        dlg = wx.NumberEntryDialog(
            self,
            message,
            "延迟秒数",
            title,
            int(max(1, default_seconds)),
            1,
            86400
        )
        try:
            if dlg.ShowModal() != wx.ID_OK:
                return 0
            return int(dlg.GetValue())
        finally:
            dlg.Destroy()

    def _schedule_task(self, task_key, delay_seconds, callback, description, sandbox_targets=None, retry_delay_seconds=1):
        delay = int(max(1, delay_seconds))
        self._schedule_seq += 1
        task_id = self._schedule_seq
        job_key = f"{str(task_key or '').strip() or 'task'}#{task_id}"
        timer = self._safe_call_later(delay * 1000, self._execute_scheduled_task, job_key, task_id, callback)
        self._schedule_jobs[job_key] = {
            "id": task_id,
            "task_type": str(task_key or "").strip(),
            "timer": timer,
            "description": str(description or "").strip(),
            "eta": time.time() + delay,
            "sandbox_targets": sandbox_targets if isinstance(sandbox_targets, list) else None,
            "retry_delay_seconds": int(max(1, retry_delay_seconds)),
        }
        desc = self._schedule_jobs[job_key]["description"] or task_key
        self.lbl_status.SetLabel(f"已设置定时任务（{delay} 秒后）: {desc}")
        return job_key

    def _execute_scheduled_task(self, task_key, task_id, callback):
        if self._is_window_closing():
            self._schedule_jobs.pop(task_key, None)
            return
        task = self._schedule_jobs.get(task_key)
        if not task or task.get("id") != task_id:
            return
        description = task.get("description", task_key)
        try:
            should_complete = callback()
        except Exception as e:
            self._schedule_jobs.pop(task_key, None)
            self._safe_set_status_label(f"定时任务执行失败: {description} ({str(e)})")
            return
        if should_complete is False:
            task = self._schedule_jobs.get(task_key)
            if not task or task.get("id") != task_id or self._is_window_closing():
                self._schedule_jobs.pop(task_key, None)
                return
            retry_delay = int(max(1, task.get("retry_delay_seconds", 1) or 1))
            task["eta"] = time.time() + retry_delay
            task["timer"] = self._safe_call_later(retry_delay * 1000, self._execute_scheduled_task, task_key, task_id, callback)
            return
        self._schedule_jobs.pop(task_key, None)

    def _cancel_schedule_task(self, task_key, update_status=True):
        task = self._schedule_jobs.pop(task_key, None)
        if not task:
            return False
        timer = task.get("timer")
        if timer is not None:
            try:
                timer.Stop()
            except Exception:
                pass
        if update_status:
            desc = task.get("description", task_key)
            self.lbl_status.SetLabel(f"已取消定时任务: {desc}")
        return True

    def on_cancel_all_schedules(self, event=None, update_status=True):
        keys = list(self._schedule_jobs.keys())
        if not keys:
            if update_status:
                self.lbl_status.SetLabel("当前没有定时任务")
            return
        for key in keys:
            self._cancel_schedule_task(key, update_status=False)
        if update_status:
            self.lbl_status.SetLabel("已取消全部定时任务")

    def _close_selected_sandboxes(self, sandboxes):
        targets = [str(name).strip() for name in (sandboxes or []) if str(name).strip()]
        if not targets:
            self.lbl_status.SetLabel("没有可关闭的沙盒用户")
            return
        self._delete_sandbox_refs_in_active_launch_by_display(targets)
        closed_count = 0
        partial_count = 0
        for sandbox_name in targets:
            close_result = self._close_sandbox_processes(sandbox_name)
            if close_result.get("status") == "all_closed":
                closed_count += 1
            elif close_result.get("status") == "partially_closed":
                partial_count += 1
        if partial_count > 0:
            self.lbl_status.SetLabel(
                f"已关闭 {closed_count}/{len(targets)} 个沙盒用户的进程，另有 {partial_count} 个仍有进程未关闭"
            )
        elif closed_count > 0:
            self.lbl_status.SetLabel(f"已关闭 {closed_count}/{len(targets)} 个沙盒用户的进程")
        else:
            self.lbl_status.SetLabel("未关闭任何沙盒用户进程")
        self._refresh_process_ui_after_action(refresh_list=True)

    def on_schedule_launch_sandboxes(self, event):
        selected = self._get_batch_target_sandboxes()
        if not selected:
            self.lbl_status.SetLabel("请先勾选或选中要定时启动的沙盒用户")
            return
        app_targets = self._get_checked_app_targets()
        if not app_targets:
            self.lbl_status.SetLabel("请先在应用列表中按空格选中要打开的应用")
            return
        delay = self._prompt_schedule_delay_seconds("定时启动沙盒", "请设置多少秒后启动所选沙盒：", default_seconds=30)
        if delay <= 0:
            return
        targets = list(selected)
        scheduled_app_targets = list(app_targets)

        def _do_launch():
            if not targets:
                self.lbl_status.SetLabel("没有可启动的沙盒用户")
                return True
            if self.is_launching:
                self.lbl_status.SetLabel("当前正在启动中，定时启动顺延 1 秒")
                return False
            self._launch_selected_sandboxes(targets, app_targets=scheduled_app_targets)
            return True

        self._schedule_task(
            "launch_selected",
            delay,
            _do_launch,
            f"定时启动 {len(targets)} 个沙盒",
            sandbox_targets=targets,
            retry_delay_seconds=1
        )

    def on_schedule_close_sandboxes(self, event):
        selected = self._get_batch_target_sandboxes()
        if not selected:
            self.lbl_status.SetLabel("请先勾选或选中要定时关闭的沙盒用户")
            return
        delay = self._prompt_schedule_delay_seconds("定时关闭沙盒", "请设置多少秒后关闭所选沙盒：", default_seconds=30)
        if delay <= 0:
            return
        targets = list(selected)
        self._schedule_task(
            "close_selected",
            delay,
            lambda: self._close_selected_sandboxes(targets),
            f"定时关闭 {len(targets)} 个沙盒",
            sandbox_targets=targets
        )

    def _get_checked_app_targets(self):
        checked_indices = []
        if hasattr(self.combo_app_path, "get_checked_values"):
            try:
                checked_indices = self.combo_app_path.get_checked_items()
            except Exception:
                checked_indices = []
        # App list is a checkbox list like the sandbox user list: require explicit Space check.
        # Do not treat the focused row as checked; this avoids screen-reader ambiguity.
        targets = []
        seen = set()
        for index in checked_indices:
            current_path = self._get_app_list_path_by_index(index)
            if not current_path:
                text = self.combo_app_path.GetItemText(index)
                current_path, _ = self.extract_path_and_note(text, preferred_index=index)
            if not current_path:
                current_path = str(self.combo_app_path.GetItemText(index) or "").strip()
            if not current_path:
                continue
            key = os.path.normcase(os.path.abspath(current_path))
            if key in seen:
                continue
            seen.add(key)
            targets.append((current_path, os.path.dirname(current_path)))
        return targets

    def _prepare_launch_target(self):
        app_targets = self._get_checked_app_targets()
        return self._prepare_launch_target_from_app_targets(app_targets)

    def _prepare_launch_target_from_app_targets(self, app_targets):
        app_targets = list(app_targets or [])
        if not app_targets:
            self.lbl_status.SetLabel("请先在应用列表中按空格选中要打开的应用")
            return False
        invalid = [path for path, _ in app_targets if not path or not os.path.exists(path)]
        if invalid:
            self.lbl_status.SetLabel("应用路径无效，请重新选择")
            return False
        if not self.SANDBOXIE_PATH:
            self.lbl_status.SetLabel("未找到沙盒安装路径")
            return False
        start_exe = self._get_sandboxie_start_exe()
        if not start_exe:
            self.lbl_status.SetLabel("未找到沙盒启动程序 Start.exe")
            return False
        self.APP_PATHS = app_targets
        self.APP_PATH = app_targets[0][0]
        self.APP_DIR = app_targets[0][1]
        return True

    def _launch_selected_sandboxes(self, sandboxes, app_targets=None):
        if self.is_launching:
            self.lbl_status.SetLabel("当前正在启动中，请稍候")
            return
        targets = [str(name).strip() for name in (sandboxes or []) if str(name).strip()]
        if not targets:
            self.lbl_status.SetLabel("请先在沙盒用户列表中按空格选中要启动的沙盒")
            return
        if app_targets is None:
            if not self._prepare_launch_target():
                return
        else:
            if not self._prepare_launch_target_from_app_targets(app_targets):
                return
        for app_path, _ in getattr(self, "APP_PATHS", []) or []:
            self.add_to_history(app_path)
        app_count = len(getattr(self, "APP_PATHS", []) or [])
        self.is_launching = True
        self.btn_launch.Disable()
        self.btn_launch.SetLabel("正在启动中...")
        self._active_launch_targets = targets
        if app_count > 1:
            self.lbl_status.SetLabel(f"准备启动 {len(targets)} 个沙盒、{app_count} 个应用")
        start_time = time.time()
        launch_session_id = self._begin_launch_session()
        self._log_operation_safe(f"启动任务已创建: 沙盒={len(targets)}, 应用={app_count}, 任务={len(targets) * app_count}")
        threading.Thread(target=self._launch_thread, args=(launch_session_id, targets, start_time), daemon=True).start()

    def _launch_selected_sandboxes_prepared(self, sandboxes, app_targets):
        if self.is_launching:
            self.lbl_status.SetLabel("当前正在启动中，请稍候")
            return
        targets = [str(name).strip() for name in (sandboxes or []) if str(name).strip()]
        if not targets:
            self.lbl_status.SetLabel("请先在沙盒用户列表中按空格选中要启动的沙盒")
            return
        if not self._prepare_launch_target_from_app_targets(app_targets):
            return
        for app_path, _ in getattr(self, "APP_PATHS", []) or []:
            self.add_to_history(app_path)
        app_count = len(getattr(self, "APP_PATHS", []) or [])
        self.is_launching = True
        self.btn_launch.Disable()
        self.btn_launch.SetLabel("正在启动中...")
        self._active_launch_targets = targets
        if app_count > 1:
            self.lbl_status.SetLabel(f"准备启动 {len(targets)} 个沙盒、{app_count} 个应用")
        else:
            self.lbl_status.SetLabel(f"准备启动 {len(targets)} 个沙盒")
        start_time = time.time()
        launch_session_id = self._begin_launch_session()
        self._log_operation_safe(f"启动任务已创建: 沙盒={len(targets)}, 应用={app_count}, 任务={len(targets) * app_count}")
        threading.Thread(target=self._launch_thread, args=(launch_session_id, targets, start_time), daemon=True).start()

    def on_launch(self, event):
        if self.is_launching:
            self.lbl_status.SetLabel("当前正在启动中，请稍候")
            return
        self._suppress_list_refresh_status(2.0)
        self._suppress_selection_status(2.0)
        checked_entries = self.sandbox_list.get_checked_entry_texts()
        app_targets = self._get_checked_app_targets()
        if not app_targets:
            self.lbl_status.SetLabel("请先在应用列表中按空格选中要打开的应用")
            return
        self.lbl_status.SetLabel("正在准备启动沙盒...")
        self._log_operation_safe(f"启动按钮触发: 勾选沙盒={len(checked_entries)}, 勾选应用={len(app_targets)}")

        def _prepare():
            selected = self._resolve_launch_entries(checked_entries)
            self._log_operation_safe(f"启动目标解析完成: 可启动沙盒={len(selected)}")
            self._safe_call_after(self._launch_selected_sandboxes_prepared, selected, app_targets)

        threading.Thread(target=_prepare, daemon=True).start()

    def _compute_dynamic_launch_delay_seconds(self, batch_size):
        if not getattr(self, "dynamic_launch_delay_by_load", False):
            return 0
        try:
            import psutil
            cpu_percent = float(psutil.cpu_percent(interval=0.08))
        except Exception:
            return 0
        if cpu_percent >= 95:
            return 2
        if cpu_percent >= 88:
            return 1
        if cpu_percent >= 80 and int(max(1, batch_size)) > 1:
            return 1
        return 0

    def _launch_thread(self, launch_session_id, sandboxes, start_time):
        app_targets = list(getattr(self, "APP_PATHS", []) or [])
        if not app_targets and getattr(self, "APP_PATH", ""):
            app_targets = [(self.APP_PATH, getattr(self, "APP_DIR", ""))]
        total_sandboxes = len(sandboxes or [])
        total_apps = len(app_targets)
        total = total_sandboxes * total_apps
        self._log_operation_safe(f"启动线程开始: 沙盒={total_sandboxes}, 应用={total_apps}, 任务={total}")
        if total <= 0:
            total_time = time.time() - start_time
            self._safe_call_after(
                self._launch_done,
                launch_session_id,
                0,
                0,
                0,
                total_time,
                0,
                self._is_launch_session_cancelled(launch_session_id)
            )
            return

        success = 0
        failed = 0
        skipped = 0
        done_count = 0
        interval = int(max(0, self.launch_interval))
        batch_size = min(total_sandboxes, max(1, int(self.launch_concurrency_limit)))
        serial_relief_mode = (interval == 0 and batch_size == 1)
        serial_relief_sleep = 0.06
        lock = threading.Lock()

        for batch_start in range(0, total_sandboxes, batch_size):
            if self._is_launch_session_cancelled(launch_session_id):
                break
            batch = list(sandboxes[batch_start:batch_start + batch_size])
            launch_targets = []

            for sandbox_name in batch:
                if not str(sandbox_name or "").strip():
                    continue
                if self._is_launch_session_cancelled(launch_session_id):
                    break
                if self.skip_running_sandbox_on_launch:
                    try:
                        if self._get_sandbox_pids(sandbox_name):
                            skipped += total_apps
                            done_count += total_apps
                            self._safe_call_after(
                                self._set_launch_status_if_active,
                                launch_session_id,
                                f"已跳过已运行沙盒 {sandbox_name} ({done_count}/{total})"
                            )
                            continue
                    except Exception:
                        pass
                for app_path, app_dir in app_targets:
                    launch_targets.append((sandbox_name, app_path, app_dir))

            if launch_targets:
                self._safe_call_after(
                    self._set_launch_status_if_active,
                    launch_session_id,
                    f"正在启动第 {batch_start // batch_size + 1} 批，共 {len(launch_targets)} 个启动任务..."
                )

            def _launch_one(target):
                nonlocal success, failed, done_count
                if self._is_launch_session_cancelled(launch_session_id):
                    return
                sandbox_name, app_path, app_dir = target
                launched = self.start_app_in_sandbox(sandbox_name, app_path, app_dir)
                volume_key = self._get_volume_memory_key(sandbox_name)
                if launched and getattr(self, "remember_volume", True) and volume_key in self.sandbox_volume_memory:
                    threading.Thread(
                        target=self._restore_saved_volume_for_sandbox,
                        args=(sandbox_name,),
                        daemon=True
                    ).start()
                with lock:
                    if launched:
                        success += 1
                    else:
                        failed += 1
                    done_count += 1
                    progress = done_count
                self._safe_call_after(
                    self._set_launch_status_if_active,
                    launch_session_id,
                    f"启动进度 {progress}/{total}: {sandbox_name}，{os.path.basename(app_path)}"
                )

            threads = []
            for target in launch_targets:
                if self._is_launch_session_cancelled(launch_session_id):
                    break
                t = threading.Thread(target=_launch_one, args=(target,), daemon=True)
                t.start()
                threads.append(t)
            for t in threads:
                t.join()

            has_more_batches = (batch_start + batch_size) < total_sandboxes
            effective_interval = interval
            adaptive_interval = self._compute_dynamic_launch_delay_seconds(batch_size)
            if adaptive_interval > effective_interval:
                effective_interval = adaptive_interval
            if self._is_launch_session_cancelled(launch_session_id):
                break
            if has_more_batches and effective_interval > 0:
                for rem in range(effective_interval, 0, -1):
                    if self._is_launch_session_cancelled(launch_session_id):
                        break
                    self._safe_call_after(
                        self._set_launch_status_if_active,
                        launch_session_id,
                        f"下一批启动等待 {rem} 秒..."
                    )
                    time.sleep(1)
            elif has_more_batches and serial_relief_mode:
                if self._is_launch_session_cancelled(launch_session_id):
                    break
                time.sleep(serial_relief_sleep)

        total_time = time.time() - start_time
        self._log_operation_safe(
            f"启动线程结束: 成功={success}, 失败={failed}, 跳过={skipped}, 总数={total}, 耗时={total_time:.2f}s"
        )
        self._safe_call_after(
            self._launch_done,
            launch_session_id,
            success,
            total,
            skipped,
            total_time,
            failed,
            self._is_launch_session_cancelled(launch_session_id)
        )

    def _launch_done(self, launch_session_id, success, total, skipped, total_time, failed=0, cancelled=False):
        self.is_launching = False
        try:
            if int(launch_session_id or 0) == int(getattr(self, "_launch_session_id", 0) or 0):
                self._launch_cancel_event.clear()
        except Exception:
            pass
        self._active_launch_targets = None
        if self._is_window_closing():
            return
        try:
            self.btn_launch.Enable()
            self.btn_launch.SetLabel("启动选中沙盒")
        except RuntimeError:
            return
        self._safe_call_later(1, self._refresh_visible_sandbox_running_states)
        self._safe_call_later(800, self._refresh_visible_sandbox_running_states)
        self._safe_call_later(2500, self._refresh_visible_sandbox_running_states)
        if cancelled:
            return
        if total <= 0:
            launch_status_text = "就绪"
        else:
            launch_status_text = f"启动完成：成功 {success}/{total}"
            if skipped > 0:
                launch_status_text += f"，跳过 {skipped}"
            if failed > 0:
                launch_status_text += f"，失败 {failed}"
        if launch_status_text:
            self._safe_set_status_label(f"{launch_status_text}。按 F5 可刷新运行状态")
            logger = getattr(self, "_log_operation_safe", None)
            if callable(logger):
                logger(launch_status_text)
        if success > 0:
            self._speak_process_assist(f"已启动{success}个沙盒", event_key="launch_done")

    def on_backup_sandboxes(self, event):
        BackupRestoreMgr(self).backup()

    def on_restore_sandboxes(self, event):
        BackupRestoreMgr(self).restore()


def main():
    cleanup_legacy_release_artifacts()
    single = SingleInstance("SandboxLauncher")
    if not single.check():
        single.notify_existing_instance()
        return
    app = wx.App()
    frame = SandboxLauncher()
    import atexit
    atexit.register(single.release)
    app.MainLoop()

if __name__ == "__main__":
    main()















