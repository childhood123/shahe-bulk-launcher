import json
import os

import wx

from ..common import parse_bool_setting, parse_int_setting
from ..constants import (
    LOG_OUTPUT_DETAILED,
    UPDATE_MANIFEST_URL,
    get_default_sandbox_root,
    normalize_log_output_mode,
    normalize_sandbox_root_path,
)
from ..ui.settings_dialogs import SettingsDialog


class SettingsMixin:
    def _get_default_settings(self):
        return {
            "max_history_items": 10,
            "hide_path": False,
            "app_history": [],
            "sandboxie_path": "",
            "sandbox_root": get_default_sandbox_root(),
            "launch_interval": 5,
            "launch_concurrency_limit": 1,
            "show_tray": True,
            "auto_run": False,
            "auto_select_range_on_check": False,
            "list_use_left_right_nav": True,
            "auto_minimize_to_tray_after_launch": False,
            "skip_running_sandbox_on_launch": False,
            "remember_volume": True,
            "log_output_mode": LOG_OUTPUT_DETAILED,
            "hotkeys_enabled": False,
            "hotkeys": {},
            "sandbox_groups": {},
            "pinned_sandboxes": [],
            "sandbox_volume_memory": {},
            "speech_interface_dll": "",
            "speech_assist_hints": {
                "enabled": True,
                "mute_current": "静音当前打开的进程",
                "mute_all": "静音全部打开的进程"
            }
        }

    def _get_update_manifest_url(self):
        url = str(getattr(self, "update_manifest_url", "") or "").strip()
        if not url.lower().startswith("https://"):
            url = UPDATE_MANIFEST_URL
        return url if url else UPDATE_MANIFEST_URL

    def _set_update_manifest_url(self, url):
        value = str(url or "").strip()
        if not value or not value.lower().startswith("https://"):
            value = UPDATE_MANIFEST_URL
        self.update_manifest_url = value
        try:
            self.config_store.set_secret_text("update_manifest_url", value)
        except Exception:
            pass
        return value

    def load_settings(self):
        defaults = self._get_default_settings()
        try:
            stored_settings = self.config_store.get_json("settings_json", None)
            if isinstance(stored_settings, dict):
                merged = dict(defaults)
                merged.update(stored_settings)
                speech_hints = dict(defaults.get("speech_assist_hints", {}))
                loaded_hints = stored_settings.get("speech_assist_hints", {})
                if isinstance(loaded_hints, dict):
                    speech_hints.update(loaded_hints)
                merged["speech_assist_hints"] = speech_hints
                merged.pop("auto_update_on_startup", None)
                merged.pop("auto_check_update_on_startup", None)
                merged.pop("update_manifest_url", None)
                merged.pop("dynamic_launch_delay_by_load", None)
                merged["log_output_mode"] = normalize_log_output_mode(merged.get("log_output_mode", LOG_OUTPUT_DETAILED))
                self.update_manifest_url = self.config_store.get_secret_text("update_manifest_url", UPDATE_MANIFEST_URL) or UPDATE_MANIFEST_URL
                return merged
            if os.path.exists(self.CONFIG_FILE):
                with open(self.CONFIG_FILE, 'r', encoding='utf-8') as f:
                    loaded = json.load(f)
                if isinstance(loaded, dict):
                    merged = dict(defaults)
                    merged.update(loaded)
                    speech_hints = dict(defaults.get("speech_assist_hints", {}))
                    loaded_hints = loaded.get("speech_assist_hints", {})
                    if isinstance(loaded_hints, dict):
                        speech_hints.update(loaded_hints)
                    merged["speech_assist_hints"] = speech_hints
                    merged.pop("auto_update_on_startup", None)
                    merged.pop("auto_check_update_on_startup", None)
                    merged.pop("update_manifest_url", None)
                    merged.pop("dynamic_launch_delay_by_load", None)
                    merged["log_output_mode"] = normalize_log_output_mode(merged.get("log_output_mode", LOG_OUTPUT_DETAILED))
                    try:
                        self.config_store.set_json("settings_json", merged)
                    except Exception:
                        pass
                    try:
                        os.makedirs(self.CONFIG_DIR, exist_ok=True)
                    except Exception:
                        pass
                    self.update_manifest_url = self.config_store.get_secret_text("update_manifest_url", UPDATE_MANIFEST_URL) or UPDATE_MANIFEST_URL
                    self._set_update_manifest_url(self.update_manifest_url)
                    return merged
        except Exception:
            pass
        self.update_manifest_url = self.config_store.get_secret_text("update_manifest_url", UPDATE_MANIFEST_URL) or UPDATE_MANIFEST_URL
        self._set_update_manifest_url(self.update_manifest_url)
        defaults["log_output_mode"] = normalize_log_output_mode(defaults.get("log_output_mode", LOG_OUTPUT_DETAILED))
        return defaults

    def save_settings(self):
        try:
            self.settings["app_history"] = self._normalize_app_history(
                self.app_history,
                limit=self.max_history_items
            )
            self.settings["max_history_items"] = self.max_history_items
            self.settings["hide_path"] = self.hide_path
            self.settings["sandboxie_path"] = getattr(self, 'SANDBOXIE_PATH', '')
            self.settings["sandbox_root"] = self.sandbox_root
            self.settings["launch_interval"] = self.launch_interval
            self.settings["launch_concurrency_limit"] = self.launch_concurrency_limit
            self.settings.pop("dynamic_launch_delay_by_load", None)
            self.settings["show_tray"] = self.show_tray
            self.settings["auto_run"] = self.auto_run
            self.settings.pop("auto_update_on_startup", None)
            self.settings.pop("auto_check_update_on_startup", None)
            self.settings["auto_select_range_on_check"] = self.auto_select_range_on_check
            self.settings["list_use_left_right_nav"] = self.list_use_left_right_nav
            self.settings["auto_minimize_to_tray_after_launch"] = False
            self.settings["skip_running_sandbox_on_launch"] = self.skip_running_sandbox_on_launch
            self.settings["remember_volume"] = self.remember_volume
            self.settings["log_output_mode"] = normalize_log_output_mode(getattr(self, "log_output_mode", LOG_OUTPUT_DETAILED))
            self.settings["hotkeys_enabled"] = self.hotkeys_enabled
            self.settings["hotkeys"] = self.hotkeys
            self.settings["sandbox_groups"] = self.sandbox_groups
            self.settings["pinned_sandboxes"] = self.pinned_sandboxes
            self.settings["sandbox_volume_memory"] = self.sandbox_volume_memory
            self.settings["speech_interface_dll"] = self.speech_interface_dll
            self.settings["speech_assist_hints"] = self.speech_assist_hints
            self.settings.pop("auto_update_on_startup", None)
            self.settings.pop("auto_check_update_on_startup", None)
            self.settings.pop("update_manifest_url", None)
            self.config_store.set_json("settings_json", self.settings)
            self._set_update_manifest_url(self._get_update_manifest_url())
        except Exception:
            pass

    def on_settings(self, event):
        dialog = SettingsDialog(
            self,
            self.settings.copy(),
            self.on_settings_saved,
            default_settings=self._get_default_settings()
        )
        dialog.ShowModal()
        dialog.Destroy()

    def on_reset_settings(self, event):
        if wx.MessageBox(
            "确定要将全部设置重置为默认值吗？\n该操作会覆盖当前设置。",
            "重置设置",
            wx.YES_NO | wx.ICON_WARNING,
            self
        ) != wx.YES:
            return
        defaults = self._get_default_settings()
        self.app_history = list(defaults.get("app_history", []))
        self.max_history_items = parse_int_setting(defaults.get("max_history_items", 10), 10, 0, 20)
        self.hide_path = parse_bool_setting(defaults.get("hide_path", False), False)
        self.APP_PATH = ""
        self.APP_DIR = ""
        self.SANDBOXIE_PATH = str(defaults.get("sandboxie_path", "") or "").strip()
        self.sandbox_root = normalize_sandbox_root_path(defaults.get("sandbox_root", get_default_sandbox_root()))
        if not self.sandbox_root:
            self.sandbox_root = get_default_sandbox_root()
        self.SANDBOX_ROOT = self.sandbox_root
        if hasattr(self, "combo_app_path"):
            try:
                self.combo_app_path.SetValue("")
                self.update_combo_box()
            except Exception:
                pass
        self.on_settings_saved(defaults)
        self.lbl_status.SetLabel("已重置所有设置为默认值")

    def _apply_sandbox_root(self, new_root, refresh=True):
        value = normalize_sandbox_root_path(new_root)
        if not value:
            return False
        self.sandbox_root = value
        self.SANDBOX_ROOT = value
        if hasattr(self, "sandbox_list"):
            self.sandbox_list.set_sandbox_root(value)
            if refresh:
                self.sandbox_list.on_refresh_sandbox_list(None)
        self.save_settings()
        return True

    def _apply_sandboxie_path(self, new_path):
        value = str(new_path or "").strip()
        if not value:
            return False
        try:
            value = os.path.abspath(os.path.normpath(value))
        except Exception:
            pass
        start_exe = os.path.join(value, "Start.exe")
        if not os.path.exists(start_exe):
            return False
        self.SANDBOXIE_PATH = value
        self.save_settings()
        return True

    def on_manual_correct_sandboxie_path(self, event):
        default_path = self.SANDBOXIE_PATH if getattr(self, "SANDBOXIE_PATH", "") else r"C:\Program Files\Sandboxie"
        dlg = wx.DirDialog(
            self,
            "手动校正 Sandboxie 安装路径",
            defaultPath=default_path,
            style=wx.DD_DEFAULT_STYLE | wx.DD_DIR_MUST_EXIST
        )
        try:
            if dlg.ShowModal() != wx.ID_OK:
                return
            selected_path = dlg.GetPath()
        finally:
            dlg.Destroy()

        if self._apply_sandboxie_path(selected_path):
            self.lbl_status.SetLabel(f"已校正 Sandboxie 安装路径: {self.SANDBOXIE_PATH}")
            wx.MessageBox("安装路径已保存。", "校正完成", wx.OK | wx.ICON_INFORMATION)
        else:
            self.lbl_status.SetLabel("该目录下未找到 Start.exe，未修改安装路径")
            wx.MessageBox("该目录不是有效的 Sandboxie 安装目录。", "校正失败", wx.OK | wx.ICON_WARNING)

    def on_select_sandbox_root(self, event):
        default_path = self.sandbox_root if getattr(self, "sandbox_root", "") else get_default_sandbox_root()
        dlg = wx.DirDialog(
            self,
            "选择沙盒用户数据目录",
            defaultPath=default_path,
            style=wx.DD_DEFAULT_STYLE | wx.DD_DIR_MUST_EXIST
        )
        try:
            if dlg.ShowModal() != wx.ID_OK:
                return
            selected_root = dlg.GetPath()
        finally:
            dlg.Destroy()

        if self._apply_sandbox_root(selected_root, refresh=True):
            self.lbl_status.SetLabel(f"已切换沙盒数据目录: {self.sandbox_root}")
        else:
            self.lbl_status.SetLabel("沙盒数据目录无效，未修改")

    def on_settings_saved(self, settings):
        settings = settings if isinstance(settings, dict) else {}
        self.settings = settings
        self.max_history_items = parse_int_setting(
            settings.get("max_history_items", self.max_history_items),
            self.max_history_items,
            0,
            20
        )
        self.hide_path = parse_bool_setting(settings.get("hide_path", self.hide_path), self.hide_path)
        self.log_output_mode = normalize_log_output_mode(settings.get("log_output_mode", self.log_output_mode))
        self.settings["log_output_mode"] = self.log_output_mode
        incoming_history = settings.get("app_history", self.app_history)
        if isinstance(incoming_history, list):
            self.app_history = incoming_history
        self.launch_interval = parse_int_setting(settings.get("launch_interval", 5), 5, 0, 15)
        self.launch_concurrency_limit = parse_int_setting(
            settings.get("launch_concurrency_limit", self.launch_concurrency_limit),
            self.launch_concurrency_limit,
            1,
            32
        )
        settings.pop("dynamic_launch_delay_by_load", None)
        self.auto_run = parse_bool_setting(settings.get("auto_run", False), False)
        settings.pop("auto_update_on_startup", None)
        settings.pop("auto_check_update_on_startup", None)
        self.auto_select_range_on_check = parse_bool_setting(
            settings.get("auto_select_range_on_check", False),
            False
        )
        self.list_use_left_right_nav = parse_bool_setting(
            settings.get("list_use_left_right_nav", self.list_use_left_right_nav),
            self.list_use_left_right_nav
        )
        self.minimize_to_tray_after_launch = False
        self.skip_running_sandbox_on_launch = parse_bool_setting(
            settings.get("skip_running_sandbox_on_launch", False),
            False
        )
        self.remember_volume = parse_bool_setting(settings.get("remember_volume", self.remember_volume), self.remember_volume)
        self.hotkeys_enabled = parse_bool_setting(settings.get("hotkeys_enabled", self.hotkeys_enabled), self.hotkeys_enabled)
        self.hotkeys = self._normalize_hotkey_map(settings.get("hotkeys", self.hotkeys))
        self.sandbox_root = normalize_sandbox_root_path(settings.get("sandbox_root", self.sandbox_root))
        if not self.sandbox_root:
            self.sandbox_root = get_default_sandbox_root()
        self.settings["sandbox_root"] = self.sandbox_root
        self.SANDBOX_ROOT = self.sandbox_root
        self.sandbox_groups = self._normalize_sandbox_groups(settings.get("sandbox_groups", {}))
        self.pinned_sandboxes = self._normalize_sandbox_name_list(settings.get("pinned_sandboxes", self.pinned_sandboxes))
        self.sandbox_volume_memory = self._normalize_volume_memory(
            settings.get("sandbox_volume_memory", self.sandbox_volume_memory)
        )
        self.speech_interface_dll = self._normalize_speech_interface_value(
            settings.get("speech_interface_dll", self.speech_interface_dll)
        )
        if str(settings.get("speech_interface_dll", "") or "").strip() and not self.speech_interface_dll:
            self.speech_interface_dll = ""
            self.settings["speech_interface_dll"] = ""
        raw_hints = settings.get("speech_assist_hints", self.speech_assist_hints)
        self.speech_assist_hints = raw_hints if isinstance(raw_hints, dict) else {}
        self.speech_assist_hints["enabled"] = parse_bool_setting(self.speech_assist_hints.get("enabled", True), True)
        if not str(self.speech_assist_hints.get("mute_current", "") or "").strip():
            self.speech_assist_hints["mute_current"] = "静音当前打开的进程"
        if not str(self.speech_assist_hints.get("mute_all", "") or "").strip():
            self.speech_assist_hints["mute_all"] = "静音全部打开的进程"
        new_show_tray = parse_bool_setting(settings.get("show_tray", self.show_tray), self.show_tray)
        if new_show_tray != self.show_tray:
            self.show_tray = new_show_tray
            if self.show_tray:
                self.create_task_bar_icon()
            else:
                self._disable_tray_display()
        if hasattr(self, "sandbox_list"):
            self.sandbox_list.set_sandbox_root(self.sandbox_root)
            self.sandbox_list.set_range_auto_check_enabled(self.auto_select_range_on_check)
            self.sandbox_list.set_left_right_navigation_enabled(self.list_use_left_right_nav)
            self.sandbox_list.on_refresh_sandbox_list(None)
        if hasattr(self, "combo_app_path"):
            try:
                self.update_combo_box()
            except Exception:
                pass
        self._apply_hotkeys()
        self.create_menu_bar()
        self.save_settings()
        self._apply_auto_run()
        self._schedule_process_menu_live_state_refresh(delay_ms=1)
        self.lbl_status.SetLabel("设置已保存")
