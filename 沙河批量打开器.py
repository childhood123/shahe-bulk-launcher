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




















































class SandboxLauncher(
    LifecycleMixin,
    SettingsMixin,
    AppPathsMixin,
    SandboxieMixin,
    ProcessActionsMixin,
    UpdatesMixin,
    SpeechMixin,
    HotkeysMixin,
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















