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
from shahe_bulk_launcher.features.app_paths import AppPathsMixin
from shahe_bulk_launcher.features.sandboxie import SandboxieMixin
from shahe_bulk_launcher.features.process_actions import ProcessActionsMixin
from shahe_bulk_launcher.features.updates import UpdatesMixin
from shahe_bulk_launcher.features.speech import SpeechMixin
from shahe_bulk_launcher.features.hotkeys import HotkeysMixin
from shahe_bulk_launcher.features.sandbox_admin import SandboxAdminMixin
from shahe_bulk_launcher.features.scheduling import SchedulingMixin






















































class SandboxLauncher(
    LifecycleMixin,
    SettingsMixin,
    AppPathsMixin,
    SandboxieMixin,
    ProcessActionsMixin,
    UpdatesMixin,
    SpeechMixin,
    HotkeysMixin,
    SandboxAdminMixin,
    SchedulingMixin,
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















