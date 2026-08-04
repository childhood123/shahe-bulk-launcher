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















