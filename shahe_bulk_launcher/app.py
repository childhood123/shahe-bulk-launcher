import os
import threading

import wx

from .bootstrap import cleanup_legacy_release_artifacts, get_app_dir
from .common import (
    ConfigStore,
    get_configuration_db_path,
    get_configuration_dir,
    parse_bool_setting,
    parse_int_setting,
)
from .constants import (
    APP_VERSION,
    LOG_OUTPUT_DETAILED,
    OLD_DEFAULT_SANDBOX_ROOT,
    get_default_sandbox_root,
    normalize_log_output_mode,
    normalize_sandbox_root_path,
)
from .features.app_paths import AppPathsMixin
from .features.hotkeys import HotkeysMixin
from .features.lifecycle import LifecycleMixin
from .features.process_actions import ProcessActionsMixin
from .features.sandbox_admin import SandboxAdminMixin
from .features.sandboxie import SandboxieMixin
from .features.scheduling import SchedulingMixin
from .features.settings import SettingsMixin
from .features.speech import SpeechMixin
from .features.updates import UpdatesMixin
from .single_instance import SingleInstance


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
