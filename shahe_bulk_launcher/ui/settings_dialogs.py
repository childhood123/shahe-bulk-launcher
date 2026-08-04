import wx

from ..common import end_dialog_safely, parse_bool_setting, parse_int_setting
from ..constants import LOG_OUTPUT_CHOICES, LOG_OUTPUT_DETAILED, normalize_log_output_mode


class HotkeySettingsDialog(wx.Dialog):
    def __init__(
        self,
        parent,
        action_specs,
        hotkeys,
        default_hotkeys,
        parse_hotkey_callback,
        format_hotkey_callback,
        hotkeys_enabled=True,
        reserved_hotkeys=None
    ):
        super().__init__(parent, title="快捷键设置", size=(760, 560))
        self.action_specs = list(action_specs or [])
        self.hotkeys = dict(hotkeys or {})
        self.default_hotkeys = dict(default_hotkeys or {})
        self.parse_hotkey_callback = parse_hotkey_callback
        self.format_hotkey_callback = format_hotkey_callback
        self.reserved_hotkeys = list(reserved_hotkeys or [])
        self.result_hotkeys = dict(self.hotkeys)
        self.result_hotkeys_enabled = bool(hotkeys_enabled)
        self.working_hotkeys = {
            spec["action"]: str(self.hotkeys.get(spec["action"], "") or "")
            for spec in self.action_specs
        }
        self.current_action = ""
        self.row_actions = []
        self.init_ui()
        self.Bind(wx.EVT_CHAR_HOOK, self.on_char_hook)

    def on_char_hook(self, event):
        key_code = event.GetKeyCode()
        focused = wx.Window.FindFocus()
        if key_code == wx.WXK_ESCAPE and focused is not self.txt_hotkey:
            end_dialog_safely(self, wx.ID_CANCEL)
        else:
            event.Skip()

    def init_ui(self):
        panel = wx.Panel(self)
        main_sizer = wx.BoxSizer(wx.VERTICAL)
        tip = wx.StaticText(
            panel,
            label="快捷键列表"
        )
        main_sizer.Add(tip, 0, wx.EXPAND | wx.ALL, 10)

        content_sizer = wx.BoxSizer(wx.HORIZONTAL)
        self.list_actions = wx.ListBox(panel, style=wx.LB_SINGLE)
        self.list_actions.SetName("快捷键功能列表")
        for spec in self.action_specs:
            label = str(spec.get("label", "") or "").strip()
            self.row_actions.append(spec["action"])
            self.list_actions.Append(label)
        self.list_actions.Bind(wx.EVT_LISTBOX, self.on_action_selected)
        content_sizer.Add(self.list_actions, 1, wx.EXPAND | wx.RIGHT, 10)

        editor_sizer = wx.BoxSizer(wx.VERTICAL)
        editor_sizer.Add(wx.StaticText(panel, label="当前功能快捷键："), 0, wx.BOTTOM, 5)
        self.txt_hotkey = wx.TextCtrl(panel, value="", style=wx.TE_PROCESS_ENTER)
        self.txt_hotkey.SetName("当前功能快捷键")
        self.txt_hotkey.Bind(wx.EVT_KILL_FOCUS, self.on_hotkey_focus_lost)
        self.txt_hotkey.Bind(wx.EVT_KEY_DOWN, self.on_hotkey_key_down)
        editor_sizer.Add(self.txt_hotkey, 0, wx.EXPAND | wx.BOTTOM, 10)

        self.chk_hotkeys_enabled = wx.CheckBox(panel, label="启用快捷键")
        self.chk_hotkeys_enabled.SetValue(self.result_hotkeys_enabled)
        editor_sizer.Add(self.chk_hotkeys_enabled, 0, wx.BOTTOM, 12)

        content_sizer.Add(editor_sizer, 0, wx.EXPAND)
        main_sizer.Add(content_sizer, 1, wx.EXPAND | wx.LEFT | wx.RIGHT, 10)

        button_sizer = wx.BoxSizer(wx.HORIZONTAL)
        self.btn_reset_hotkeys = wx.Button(panel, label="重置所有快捷键设置")
        self.btn_reset_hotkeys.Bind(wx.EVT_BUTTON, self.on_reset_hotkeys)
        button_sizer.Add(self.btn_reset_hotkeys, 0, wx.RIGHT, 8)
        button_sizer.AddStretchSpacer()
        self.btn_ok = wx.Button(panel, label="确定")
        self.btn_ok.Bind(wx.EVT_BUTTON, self.on_ok)
        button_sizer.Add(self.btn_ok, 0, wx.RIGHT, 6)
        self.btn_cancel = wx.Button(panel, label="取消")
        self.btn_cancel.Bind(wx.EVT_BUTTON, lambda e: end_dialog_safely(self, wx.ID_CANCEL))
        button_sizer.Add(self.btn_cancel, 0)
        main_sizer.Add(button_sizer, 0, wx.EXPAND | wx.ALL, 10)

        panel.SetSizer(main_sizer)
        self.Centre()

        if self.list_actions.GetCount() > 0:
            self.list_actions.SetSelection(0)
            self._load_selected_hotkey_to_editor()
        self.list_actions.SetFocus()
        self.txt_hotkey.MoveAfterInTabOrder(self.list_actions)
        self.chk_hotkeys_enabled.MoveAfterInTabOrder(self.txt_hotkey)
        self.btn_reset_hotkeys.MoveAfterInTabOrder(self.chk_hotkeys_enabled)
        self.btn_ok.MoveAfterInTabOrder(self.btn_reset_hotkeys)
        self.btn_cancel.MoveAfterInTabOrder(self.btn_ok)

    def on_hotkey_key_down(self, event):
        key_code = event.GetKeyCode()
        if key_code == wx.WXK_TAB:
            event.Skip()
            return
        if key_code in (wx.WXK_SHIFT, wx.WXK_CONTROL, wx.WXK_ALT):
            event.Skip(False)
            return
        if key_code in (wx.WXK_BACK, wx.WXK_DELETE):
            self.txt_hotkey.SetValue("")
            self._save_editor_to_current_action()
            event.Skip(False)
            return

        modifiers = 0
        if event.ControlDown():
            modifiers |= wx.ACCEL_CTRL
        if event.AltDown():
            modifiers |= wx.ACCEL_ALT
        if event.ShiftDown():
            modifiers |= wx.ACCEL_SHIFT

        try:
            text = self.format_hotkey_callback(modifiers, key_code)
            parsed = self.parse_hotkey_callback(text) if text else None
        except Exception:
            text = ""
            parsed = None

        if parsed:
            normalized = self.format_hotkey_callback(parsed[0], parsed[1])
            self.txt_hotkey.SetValue(normalized)
            self.txt_hotkey.SetInsertionPointEnd()
            self._save_editor_to_current_action()
        event.Skip(False)

    def on_action_selected(self, event):
        self._save_editor_to_current_action()
        self._load_selected_hotkey_to_editor()
        event.Skip()

    def on_hotkey_focus_lost(self, event):
        self._save_editor_to_current_action()
        event.Skip()

    def _load_selected_hotkey_to_editor(self):
        idx = self.list_actions.GetSelection()
        if idx == wx.NOT_FOUND or idx >= len(self.row_actions):
            self.current_action = ""
            self.txt_hotkey.SetValue("")
            return
        action = self.row_actions[idx]
        self.current_action = action
        self.txt_hotkey.SetValue(str(self.working_hotkeys.get(action, "") or ""))
        self.txt_hotkey.SetInsertionPointEnd()

    def _save_editor_to_current_action(self):
        if not self.current_action:
            return
        self.working_hotkeys[self.current_action] = str(self.txt_hotkey.GetValue() or "").strip()

    def on_reset_hotkeys(self, event):
        if wx.MessageBox(
            "确定要将全部快捷键恢复为默认值吗？",
            "重置快捷键",
            wx.YES_NO | wx.ICON_QUESTION,
            self
        ) != wx.YES:
            return
        self.working_hotkeys = {
            spec["action"]: str(self.default_hotkeys.get(spec["action"], "") or "")
            for spec in self.action_specs
        }
        self.chk_hotkeys_enabled.SetValue(False)
        self._load_selected_hotkey_to_editor()

    def on_ok(self, event):
        self._save_editor_to_current_action()
        new_hotkeys = {}
        duplicates = {}
        used = {}
        invalid = []
        reserved_conflicts = []
        reserved_used = {}
        for item in self.reserved_hotkeys:
            if not isinstance(item, dict):
                continue
            hotkey_text = str(item.get("hotkey", "") or "").strip()
            if not hotkey_text:
                continue
            parsed = None
            try:
                parsed = self.parse_hotkey_callback(hotkey_text)
            except Exception:
                parsed = None
            if parsed:
                reserved_used[(parsed[0], parsed[1])] = str(item.get("label", "") or hotkey_text)
        for spec in self.action_specs:
            action = spec["action"]
            label = spec["label"]
            raw_text = str(self.working_hotkeys.get(action, "") or "").strip()
            if not raw_text:
                new_hotkeys[action] = ""
                continue
            parsed = None
            try:
                parsed = self.parse_hotkey_callback(raw_text)
            except Exception:
                parsed = None
            if not parsed:
                invalid.append(f"{label}: {raw_text}")
                continue
            key = (parsed[0], parsed[1])
            if key in reserved_used:
                reserved_conflicts.append(f"{label}: {raw_text}（已用于{reserved_used[key]}）")
                continue
            if key in used:
                duplicates.setdefault(key, [used[key]])
                duplicates[key].append(label)
                continue
            used[key] = label
            try:
                normalized_text = self.format_hotkey_callback(parsed[0], parsed[1])
            except Exception:
                normalized_text = raw_text
            new_hotkeys[action] = normalized_text

        if invalid:
            wx.MessageBox(
                "以下快捷键格式无效，请修改后重试：\n" + "\n".join(invalid),
                "快捷键校验失败",
                wx.OK | wx.ICON_WARNING,
                self
            )
            return
        if reserved_conflicts:
            wx.MessageBox(
                "以下快捷键已被固定基础快捷键占用，请修改后重试：\n" + "\n".join(reserved_conflicts),
                "快捷键冲突",
                wx.OK | wx.ICON_WARNING,
                self
            )
            return
        if duplicates:
            lines = []
            for labels in duplicates.values():
                lines.append("、".join(labels))
            wx.MessageBox(
                "以下功能使用了重复快捷键，请修改后重试：\n" + "\n".join(lines),
                "快捷键冲突",
                wx.OK | wx.ICON_WARNING,
                self
            )
            return

        for spec in self.action_specs:
            action = spec["action"]
            if action not in new_hotkeys:
                new_hotkeys[action] = ""
        self.result_hotkeys = new_hotkeys
        self.result_hotkeys_enabled = bool(self.chk_hotkeys_enabled.GetValue())
        end_dialog_safely(self, wx.ID_OK)

class SettingsDialog(wx.Dialog):
    def __init__(self, parent, settings, on_save_callback, default_settings=None):
        super().__init__(parent, title="设置", size=(560, 640))
        self.settings = settings
        self.on_save_callback = on_save_callback
        self.default_settings = dict(default_settings or {})
        self.init_ui()
        self.Bind(wx.EVT_CHAR_HOOK, self.on_char_hook)

    def _get_bool_setting(self, data, key, default=False):
        source = data if isinstance(data, dict) else {}
        return parse_bool_setting(source.get(key, default), default)

    def _set_log_output_choice(self, mode):
        selected_mode = normalize_log_output_mode(mode)
        for index, (choice_mode, _) in enumerate(LOG_OUTPUT_CHOICES):
            if choice_mode == selected_mode:
                self.choice_log_output.SetSelection(index)
                return
        self.choice_log_output.SetSelection(1)

    def _get_selected_log_output_mode(self):
        selection = self.choice_log_output.GetSelection()
        if 0 <= selection < len(LOG_OUTPUT_CHOICES):
            return LOG_OUTPUT_CHOICES[selection][0]
        return LOG_OUTPUT_DETAILED

    def on_char_hook(self, event):
        key_code = event.GetKeyCode()
        if key_code == wx.WXK_ESCAPE:
            end_dialog_safely(self, wx.ID_CANCEL)
        else:
            event.Skip()

    def init_ui(self):
        panel = wx.ScrolledWindow(self)
        panel.SetScrollRate(0, 12)
        sizer = wx.BoxSizer(wx.VERTICAL)
        interval_sizer = wx.BoxSizer(wx.HORIZONTAL)
        interval_sizer.Add(wx.StaticText(panel, label="打开沙盒间隔时间（秒）:"), 0, wx.ALIGN_CENTER_VERTICAL | wx.RIGHT, 5)
        interval_value = parse_int_setting(self.settings.get("launch_interval", 5), 5, 0, 15)
        self.spin_interval = wx.SpinCtrl(panel, min=0, max=15, initial=interval_value)
        self.spin_interval.SetName("打开沙盒间隔时间")
        interval_sizer.Add(self.spin_interval, 0, wx.RIGHT, 5)
        interval_sizer.Add(wx.StaticText(panel, label="(0=无等待，快速启动)"), 0, wx.ALIGN_CENTER_VERTICAL)
        interval_sizer.AddStretchSpacer()
        sizer.Add(interval_sizer, 0, wx.EXPAND | wx.ALL, 10)
        concurrency_sizer = wx.BoxSizer(wx.HORIZONTAL)
        concurrency_sizer.Add(wx.StaticText(panel, label="启动并发数量限制:"), 0, wx.ALIGN_CENTER_VERTICAL | wx.RIGHT, 5)
        concurrency_value = parse_int_setting(self.settings.get("launch_concurrency_limit", 1), 1, 1, 32)
        self.spin_launch_concurrency = wx.SpinCtrl(
            panel,
            min=1,
            max=32,
            initial=concurrency_value
        )
        self.spin_launch_concurrency.SetName("启动并发数量限制")
        concurrency_sizer.Add(self.spin_launch_concurrency, 0, wx.RIGHT, 5)
        concurrency_sizer.Add(wx.StaticText(panel, label="(1=排队顺序启动)"), 0, wx.ALIGN_CENTER_VERTICAL)
        concurrency_sizer.AddStretchSpacer()
        sizer.Add(concurrency_sizer, 0, wx.EXPAND | wx.LEFT | wx.RIGHT | wx.BOTTOM, 10)
        launch_tip_sizer = wx.BoxSizer(wx.HORIZONTAL)
        launch_tip_sizer.Add(
            wx.StaticText(
                panel,
                label="说明：并发上限=同一时刻最多启动几个；启动间隔=每次分配启动任务前的等待秒数。"
            ),
            0,
            wx.ALIGN_CENTER_VERTICAL | wx.RIGHT,
            5
        )
        launch_tip_sizer.AddStretchSpacer()
        sizer.Add(launch_tip_sizer, 0, wx.EXPAND | wx.LEFT | wx.RIGHT | wx.BOTTOM, 10)
        dynamic_delay_sizer = wx.BoxSizer(wx.HORIZONTAL)
        self.cb_dynamic_launch_delay = wx.CheckBox(panel, label="批量启动时按系统负载动态调整延迟")
        self.cb_dynamic_launch_delay.SetValue(self._get_bool_setting(self.settings, "dynamic_launch_delay_by_load", False))
        dynamic_delay_sizer.Add(self.cb_dynamic_launch_delay, 0, wx.RIGHT, 5)
        dynamic_delay_sizer.AddStretchSpacer()
        sizer.Add(dynamic_delay_sizer, 0, wx.EXPAND | wx.LEFT | wx.RIGHT | wx.BOTTOM, 10)
        auto_run_sizer = wx.BoxSizer(wx.HORIZONTAL)
        self.cb_auto_run = wx.CheckBox(panel, label="开机自动启动本程序")
        self.cb_auto_run.SetValue(self._get_bool_setting(self.settings, "auto_run", False))
        auto_run_sizer.Add(self.cb_auto_run, 0, wx.RIGHT, 5)
        auto_run_sizer.AddStretchSpacer()
        sizer.Add(auto_run_sizer, 0, wx.EXPAND | wx.LEFT | wx.RIGHT | wx.BOTTOM, 10)
        tray_sizer = wx.BoxSizer(wx.HORIZONTAL)
        self.cb_show_tray = wx.CheckBox(panel, label="在托盘上显示")
        self.cb_show_tray.SetValue(self._get_bool_setting(self.settings, "show_tray", True))
        tray_sizer.Add(self.cb_show_tray, 0, wx.RIGHT, 5)
        tray_sizer.AddStretchSpacer()
        sizer.Add(tray_sizer, 0, wx.EXPAND | wx.LEFT | wx.RIGHT | wx.BOTTOM, 10)
        range_select_sizer = wx.BoxSizer(wx.HORIZONTAL)
        self.cb_auto_select_range = wx.CheckBox(panel, label="首尾两点自动勾选中间用户")
        self.cb_auto_select_range.SetValue(self._get_bool_setting(self.settings, "auto_select_range_on_check", False))
        range_select_sizer.Add(self.cb_auto_select_range, 0, wx.RIGHT, 5)
        range_select_sizer.AddStretchSpacer()
        sizer.Add(range_select_sizer, 0, wx.EXPAND | wx.LEFT | wx.RIGHT | wx.BOTTOM, 10)
        skip_running_sizer = wx.BoxSizer(wx.HORIZONTAL)
        self.cb_skip_running_on_launch = wx.CheckBox(panel, label="启动跳过已运行的沙盒")
        self.cb_skip_running_on_launch.SetValue(self._get_bool_setting(self.settings, "skip_running_sandbox_on_launch", False))
        skip_running_sizer.Add(self.cb_skip_running_on_launch, 0, wx.RIGHT, 5)
        skip_running_sizer.AddStretchSpacer()
        sizer.Add(skip_running_sizer, 0, wx.EXPAND | wx.LEFT | wx.RIGHT | wx.BOTTOM, 10)
        remember_volume_sizer = wx.BoxSizer(wx.HORIZONTAL)
        self.cb_remember_volume = wx.CheckBox(panel, label="记忆进程音量（下次启动自动恢复）")
        self.cb_remember_volume.SetValue(self._get_bool_setting(self.settings, "remember_volume", True))
        remember_volume_sizer.Add(self.cb_remember_volume, 0, wx.RIGHT, 5)
        remember_volume_sizer.AddStretchSpacer()
        sizer.Add(remember_volume_sizer, 0, wx.EXPAND | wx.LEFT | wx.RIGHT | wx.BOTTOM, 10)
        list_nav_sizer = wx.BoxSizer(wx.HORIZONTAL)
        self.cb_list_left_right_nav = wx.CheckBox(panel, label="用户列表启用左右光标选择")
        self.cb_list_left_right_nav.SetValue(self._get_bool_setting(self.settings, "list_use_left_right_nav", True))
        list_nav_sizer.Add(self.cb_list_left_right_nav, 0, wx.RIGHT, 5)
        list_nav_sizer.AddStretchSpacer()
        sizer.Add(list_nav_sizer, 0, wx.EXPAND | wx.LEFT | wx.RIGHT | wx.BOTTOM, 10)
        log_sizer = wx.BoxSizer(wx.HORIZONTAL)
        log_sizer.Add(wx.StaticText(panel, label="日志输出:"), 0, wx.ALIGN_CENTER_VERTICAL | wx.RIGHT, 5)
        self.choice_log_output = wx.Choice(panel, choices=[label for _, label in LOG_OUTPUT_CHOICES])
        self.choice_log_output.SetName("日志输出")
        self._set_log_output_choice(self.settings.get("log_output_mode", LOG_OUTPUT_DETAILED))
        log_sizer.Add(self.choice_log_output, 0, wx.RIGHT, 5)
        log_sizer.AddStretchSpacer()
        sizer.Add(log_sizer, 0, wx.EXPAND | wx.LEFT | wx.RIGHT | wx.BOTTOM, 10)
        button_sizer = wx.BoxSizer(wx.HORIZONTAL)
        self.btn_reset_defaults = wx.Button(panel, label="重置所有设置")
        self.btn_reset_defaults.Bind(wx.EVT_BUTTON, self.on_reset_defaults)
        button_sizer.Add(self.btn_reset_defaults, 0, wx.RIGHT, 8)
        button_sizer.AddStretchSpacer()
        self.btn_ok = wx.Button(panel, label="确定")
        self.btn_ok.Bind(wx.EVT_BUTTON, self.on_ok)
        button_sizer.Add(self.btn_ok, 0, wx.RIGHT, 5)
        self.btn_cancel = wx.Button(panel, label="取消")
        self.btn_cancel.Bind(wx.EVT_BUTTON, self.on_cancel)
        button_sizer.Add(self.btn_cancel, 0)
        sizer.Add(button_sizer, 0, wx.EXPAND | wx.ALL, 10)
        panel.SetSizer(sizer)
        panel.FitInside()
        dialog_sizer = wx.BoxSizer(wx.VERTICAL)
        dialog_sizer.Add(panel, 1, wx.EXPAND)
        self.SetSizer(dialog_sizer)

    def _apply_values(self, values):
        data = values if isinstance(values, dict) else {}
        self.spin_interval.SetValue(parse_int_setting(data.get("launch_interval", 5), 5, 0, 15))
        concurrency_value = parse_int_setting(data.get("launch_concurrency_limit", 1), 1, 1, 32)
        self.spin_launch_concurrency.SetValue(concurrency_value)
        self.cb_dynamic_launch_delay.SetValue(self._get_bool_setting(data, "dynamic_launch_delay_by_load", False))
        self.cb_auto_run.SetValue(self._get_bool_setting(data, "auto_run", False))
        self.cb_show_tray.SetValue(self._get_bool_setting(data, "show_tray", True))
        self.cb_auto_select_range.SetValue(self._get_bool_setting(data, "auto_select_range_on_check", False))
        self.cb_skip_running_on_launch.SetValue(self._get_bool_setting(data, "skip_running_sandbox_on_launch", False))
        self.cb_remember_volume.SetValue(self._get_bool_setting(data, "remember_volume", True))
        self.cb_list_left_right_nav.SetValue(self._get_bool_setting(data, "list_use_left_right_nav", True))
        self._set_log_output_choice(data.get("log_output_mode", LOG_OUTPUT_DETAILED))

    def on_reset_defaults(self, event):
        if wx.MessageBox(
            "确定要将设置窗口中的选项恢复为默认值吗？",
            "重置设置",
            wx.YES_NO | wx.ICON_QUESTION,
            self
        ) != wx.YES:
            return
        defaults = self.default_settings if isinstance(self.default_settings, dict) else {}
        self._apply_values(defaults)

    def on_ok(self, event):
        self.settings["launch_interval"] = self.spin_interval.GetValue()
        self.settings["launch_concurrency_limit"] = self.spin_launch_concurrency.GetValue()
        self.settings["dynamic_launch_delay_by_load"] = self.cb_dynamic_launch_delay.GetValue()
        self.settings["auto_run"] = self.cb_auto_run.GetValue()
        self.settings.pop("auto_update_on_startup", None)
        self.settings.pop("auto_check_update_on_startup", None)
        self.settings["show_tray"] = self.cb_show_tray.GetValue()
        self.settings["auto_select_range_on_check"] = self.cb_auto_select_range.GetValue()
        self.settings["auto_minimize_to_tray_after_launch"] = False
        self.settings["skip_running_sandbox_on_launch"] = self.cb_skip_running_on_launch.GetValue()
        self.settings["remember_volume"] = self.cb_remember_volume.GetValue()
        self.settings["list_use_left_right_nav"] = self.cb_list_left_right_nav.GetValue()
        self.settings["log_output_mode"] = self._get_selected_log_output_mode()
        self.on_save_callback(self.settings)
        end_dialog_safely(self, wx.ID_OK)

    def on_cancel(self, event):
        end_dialog_safely(self, wx.ID_CANCEL)

class PathMemoryManagerDialog(wx.Dialog):
    def __init__(self, parent, app_history, settings, on_save_callback):
        super().__init__(parent, title="路径记忆管理", size=(700, 500))
        self.app_history = app_history
        self.settings = settings
        self.on_save_callback = on_save_callback
        self.init_ui()
        self.update_list()
        self.Bind(wx.EVT_CHAR_HOOK, self.on_char_hook)

    def _get_bool_setting(self, data, key, default=False):
        source = data if isinstance(data, dict) else {}
        return parse_bool_setting(source.get(key, default), default)

    def on_char_hook(self, event):
        key_code = event.GetKeyCode()
        if key_code == wx.WXK_ESCAPE:
            end_dialog_safely(self, wx.ID_CANCEL)
        else:
            event.Skip()

    def init_ui(self):
        panel = wx.Panel(self)
        sizer = wx.BoxSizer(wx.VERTICAL)
        settings_sizer = wx.BoxSizer(wx.VERTICAL)
        history_count_sizer = wx.BoxSizer(wx.HORIZONTAL)
        history_count_sizer.Add(wx.StaticText(panel, label="路径记忆数量:"), 0, wx.ALIGN_CENTER_VERTICAL | wx.RIGHT, 5)
        history_value = parse_int_setting(self.settings.get("max_history_items", 10), 10, 0, 20)
        self.spin_history = wx.SpinCtrl(panel, min=0, max=20, initial=history_value)
        self.spin_history.SetName("路径记忆数量")
        history_count_sizer.Add(self.spin_history, 0, wx.RIGHT, 5)
        history_count_sizer.Add(wx.StaticText(panel, label="(0=不记录, 最大20)"), 0, wx.ALIGN_CENTER_VERTICAL)
        history_count_sizer.AddStretchSpacer()
        settings_sizer.Add(history_count_sizer, 0, wx.EXPAND | wx.ALL, 5)
        hide_path_sizer = wx.BoxSizer(wx.HORIZONTAL)
        self.cb_hide_path = wx.CheckBox(panel, label="隐藏路径")
        self.cb_hide_path.SetValue(self._get_bool_setting(self.settings, "hide_path", False))
        hide_path_sizer.Add(self.cb_hide_path, 0, wx.RIGHT, 5)
        hide_path_sizer.AddStretchSpacer()
        settings_sizer.Add(hide_path_sizer, 0, wx.EXPAND | wx.ALL, 5)
        sizer.Add(settings_sizer, 0, wx.EXPAND | wx.ALL, 5)
        sizer.Add(wx.StaticText(panel, label="已记忆的路径列表:"), 0, wx.ALL, 5)
        self.history_list = wx.ListBox(panel, style=wx.LB_SINGLE)
        self.history_list.SetName("已记忆的路径列表")
        sizer.Add(self.history_list, 1, wx.EXPAND | wx.ALL, 5)
        button_sizer = wx.BoxSizer(wx.HORIZONTAL)
        button_sizer.AddStretchSpacer()
        self.btn_ok = wx.Button(panel, label="确定")
        self.btn_ok.Bind(wx.EVT_BUTTON, self.on_ok)
        button_sizer.Add(self.btn_ok, 0, wx.RIGHT, 5)
        self.btn_cancel = wx.Button(panel, label="取消")
        self.btn_cancel.Bind(wx.EVT_BUTTON, self.on_cancel)
        button_sizer.Add(self.btn_cancel, 0)
        sizer.Add(button_sizer, 0, wx.EXPAND | wx.ALL, 5)
        panel.SetSizer(sizer)
        self.history_list.Bind(wx.EVT_LISTBOX, self.on_selection_changed)
        self.history_list.Bind(wx.EVT_CONTEXT_MENU, self.on_context_menu)

    def on_context_menu(self, event):
        menu = wx.Menu()
        edit_note_item = menu.Append(wx.ID_ANY, "路径记忆备注", "编辑选中路径的备注")
        self.Bind(wx.EVT_MENU, self.on_edit_note, edit_note_item)
        menu.AppendSeparator()
        delete_current_item = menu.Append(wx.ID_ANY, "删除当前记忆", "从记忆列表中删除当前路径")
        self.Bind(wx.EVT_MENU, self.on_remove_selected, delete_current_item)
        clear_memory_item = menu.Append(wx.ID_ANY, "清空所有路径记忆", "清空所有路径记忆")
        self.Bind(wx.EVT_MENU, self.on_clear_all, clear_memory_item)
        self.PopupMenu(menu)
        menu.Destroy()

    def update_list(self):
        self.history_list.Clear()
        for item in self.app_history:
            display_text = item["note"] if self._get_bool_setting(self.settings, "hide_path", False) else f'{item["note"]} - "{item["path"]}"'
            self.history_list.Append(display_text)

    def on_selection_changed(self, event):
        event.Skip()

    def on_edit_note(self, event):
        selection = self.history_list.GetSelection()
        if selection == wx.NOT_FOUND:
            return
        item = self.app_history[selection]
        current_note = item["note"]
        dialog = wx.Dialog(self, title="路径记忆备注", size=(400, 150))
        panel = wx.Panel(dialog)
        sizer = wx.BoxSizer(wx.VERTICAL)
        sizer.Add(wx.StaticText(panel, label="请输入路径备注:"), 0, wx.ALL, 10)
        text_note = wx.TextCtrl(panel, value=current_note, size=(300, -1))
        sizer.Add(text_note, 0, wx.LEFT | wx.RIGHT | wx.BOTTOM, 10)
        button_sizer = wx.BoxSizer(wx.HORIZONTAL)
        button_sizer.AddStretchSpacer()
        btn_ok = wx.Button(panel, label="确定")
        btn_ok.Bind(wx.EVT_BUTTON, lambda e: end_dialog_safely(dialog, wx.ID_OK))
        button_sizer.Add(btn_ok, 0, wx.RIGHT, 5)
        btn_cancel = wx.Button(panel, label="取消")
        btn_cancel.Bind(wx.EVT_BUTTON, lambda e: end_dialog_safely(dialog, wx.ID_CANCEL))
        button_sizer.Add(btn_cancel, 0)
        sizer.Add(button_sizer, 0, wx.EXPAND | wx.ALL, 10)
        panel.SetSizer(sizer)
        dialog.Centre()
        if dialog.ShowModal() == wx.ID_OK:
            new_note = text_note.GetValue().strip()
            if new_note:
                item["note"] = new_note
                self.update_list()
                self.history_list.SetSelection(selection)
        dialog.Destroy()

    def on_remove_selected(self, event):
        selection = self.history_list.GetSelection()
        if selection == wx.NOT_FOUND:
            return
        if wx.MessageBox(
            "确定要删除当前路径记忆吗？",
            "确认删除",
            wx.YES_NO | wx.NO_DEFAULT | wx.ICON_QUESTION,
            self
        ) == wx.YES:
            self.app_history.pop(selection)
            self.update_list()

    def on_clear_all(self, event):
        if wx.MessageBox(
            "确定要清空所有路径记忆吗？",
            "确认清空",
            wx.YES_NO | wx.NO_DEFAULT | wx.ICON_QUESTION,
            self
        ) == wx.YES:
            self.app_history.clear()
            self.update_list()

    def on_ok(self, event):
        self.settings["max_history_items"] = self.spin_history.GetValue()
        self.settings["hide_path"] = self.cb_hide_path.GetValue()
        self.on_save_callback(self.app_history, self.settings)
        end_dialog_safely(self, wx.ID_OK)

    def on_cancel(self, event):
        end_dialog_safely(self, wx.ID_CANCEL)

class SpeechSettingsDialog(wx.Dialog):
    BLOCKED_ENGINES = {"VTTS", "None", "_None", "无语音输出_None", "无语音输出"}

    def __init__(
        self,
        parent,
        speech_config,
        on_save_callback,
        reader_ini_settings=None,
        on_save_reader_ini_callback=None,
        detect_reader_callback=None,
        interface_dll_choices=None,
        selected_interface_dll="",
        on_save_interface_dll_callback=None,
        assist_hints=None,
        on_save_assist_hints_callback=None
    ):
        super().__init__(parent, title="朗读设置", size=(700, 700))
        self.on_save_callback = on_save_callback
        self.on_save_reader_ini_callback = on_save_reader_ini_callback
        self.detect_reader_callback = detect_reader_callback
        self.interface_dll_choices = list(interface_dll_choices or [])
        self.selected_interface_dll = str(selected_interface_dll or "")
        self.on_save_interface_dll_callback = on_save_interface_dll_callback
        self.assist_hints = dict(assist_hints or {})
        self.on_save_assist_hints_callback = on_save_assist_hints_callback
        self.config = self.sanitize_config(speech_config or {})
        self.reader_ini_settings = self._normalize_reader_ini_settings(reader_ini_settings or {})
        self.engine_names = list(self.config.get("TTSInfos", {}).keys())
        self.current_engine = ""
        self._choice_value_map = {}
        self.init_ui()
        self.Centre()

    @classmethod
    def sanitize_config(cls, config):
        result = dict(config) if isinstance(config, dict) else {}
        tts_infos = result.get("TTSInfos")
        if not isinstance(tts_infos, dict):
            tts_infos = {}
        cleaned_infos = {}
        for name, item in tts_infos.items():
            engine_name = str(name or "").strip()
            if (not engine_name) or (engine_name in cls.BLOCKED_ENGINES):
                continue
            if not isinstance(item, dict):
                item = {}
            try:
                speed = int(item.get("Speed", 100))
            except Exception:
                speed = 100
            try:
                volume = int(item.get("Volume", 100))
            except Exception:
                volume = 100
            try:
                pitch = int(item.get("Pitch", 50))
            except Exception:
                pitch = 50
            voice_name = str(item.get("Name", "") or "").strip()
            cleaned_infos[engine_name] = {
                "Speed": max(0, min(200, speed)),
                "Volume": max(0, min(100, volume)),
                "Pitch": max(0, min(100, pitch)),
                "Name": voice_name
            }
        result["TTSInfos"] = cleaned_infos

        default_engine = result.get("DefaultEngine")
        if default_engine is None:
            default_engine = ""
        default_engine = str(default_engine).strip()
        if default_engine in cls.BLOCKED_ENGINES:
            default_engine = ""
        if default_engine and default_engine not in cleaned_infos:
            default_engine = ""
        if not default_engine and cleaned_infos:
            default_engine = next(iter(cleaned_infos.keys()))
        result["DefaultEngine"] = default_engine
        return result

    @classmethod
    def _normalize_reader_ini_settings(cls, settings):
        default = {
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
        normalized = dict(default)
        source = settings if isinstance(settings, dict) else {}
        zdsr_source = source.get("zdsr", {}) if isinstance(source.get("zdsr", {}), dict) else {}
        boy_source = source.get("boy", {}) if isinstance(source.get("boy", {}), dict) else {}
        normalized["zdsr"] = {
            "type": str(zdsr_source.get("type", "") or "").strip(),
            "channelName": str(zdsr_source.get("channelName", "") or "").strip(),
            "keyDownInterrupt": str(zdsr_source.get("keyDownInterrupt", "") or "").strip(),
            "alwaysInterrupt": str(zdsr_source.get("alwaysInterrupt", "") or "").strip()
        }
        normalized["boy"] = {
            "Param1": "true" if str(boy_source.get("Param1", "false")).strip().lower() == "true" else "false",
            "Param2": "true" if str(boy_source.get("Param2", "false")).strip().lower() == "true" else "false",
            "Param3": "true" if str(boy_source.get("Param3", "true")).strip().lower() == "true" else "false",
            "Param4": "true" if str(boy_source.get("Param4", "true")).strip().lower() == "true" else "false",
        }
        return normalized

    def _append_choice_item(self, choice_ctrl, label, value):
        index = choice_ctrl.Append(label)
        self._choice_value_map[(id(choice_ctrl), index)] = str(value)
        return index

    def _set_choice_by_value(self, choice_ctrl, target_value):
        target = str(target_value or "")
        ctrl_id = id(choice_ctrl)
        for i in range(choice_ctrl.GetCount()):
            if self._choice_value_map.get((ctrl_id, i), "") == target:
                choice_ctrl.SetSelection(i)
                return
        if choice_ctrl.GetCount() > 0:
            choice_ctrl.SetSelection(0)

    def _get_choice_value(self, choice_ctrl):
        idx = choice_ctrl.GetSelection()
        if idx == wx.NOT_FOUND:
            return ""
        return str(self._choice_value_map.get((id(choice_ctrl), idx), ""))

    def init_ui(self):
        panel = wx.Panel(self)
        main_sizer = wx.BoxSizer(wx.VERTICAL)

        interface_sizer = wx.BoxSizer(wx.HORIZONTAL)
        interface_sizer.Add(wx.StaticText(panel, label="读屏 API 接口:"), 0, wx.ALIGN_CENTER_VERTICAL | wx.RIGHT, 8)
        self.choice_interface_dll = wx.Choice(panel)
        self.choice_interface_dll.SetName("读屏 API 接口")
        self._append_choice_item(self.choice_interface_dll, "自动（默认）", "")
        first_dll_value = ""
        for item in self.interface_dll_choices:
            if isinstance(item, dict):
                path = str(item.get("path", "") or "").strip()
                label = str(item.get("label", "") or "").strip() or path
            else:
                path = str(item or "").strip()
                label = path
            if not path:
                continue
            self._append_choice_item(self.choice_interface_dll, label, path)
            if not first_dll_value:
                first_dll_value = path
        selected_value = self.selected_interface_dll
        self._set_choice_by_value(self.choice_interface_dll, selected_value)
        interface_sizer.Add(self.choice_interface_dll, 1)
        main_sizer.Add(interface_sizer, 0, wx.EXPAND | wx.ALL, 10)

        self.chk_assist_enabled = wx.CheckBox(panel, label="启用读屏辅助提示")
        self.chk_assist_enabled.SetValue(parse_bool_setting(self.assist_hints.get("enabled", False), False))
        main_sizer.Add(self.chk_assist_enabled, 0, wx.LEFT | wx.RIGHT | wx.BOTTOM, 10)

        btn_sizer = wx.BoxSizer(wx.HORIZONTAL)
        btn_sizer.AddStretchSpacer()
        btn_ok = wx.Button(panel, label="确定")
        btn_ok.Bind(wx.EVT_BUTTON, self.on_ok)
        btn_sizer.Add(btn_ok, 0, wx.RIGHT, 6)
        btn_cancel = wx.Button(panel, label="取消")
        btn_cancel.Bind(wx.EVT_BUTTON, lambda e: end_dialog_safely(self, wx.ID_CANCEL))
        btn_sizer.Add(btn_cancel, 0)
        main_sizer.Add(btn_sizer, 0, wx.EXPAND | wx.ALL, 10)

        panel.SetSizer(main_sizer)
        self.choice_interface_dll.SetFocus()
        return

    def _save_current_engine_from_editor(self):
        if not self.current_engine:
            return
        tts_infos = self.config.setdefault("TTSInfos", {})
        current = tts_infos.setdefault(self.current_engine, {})
        current["Speed"] = int(self.spin_speed.GetValue())
        current["Volume"] = int(self.spin_volume.GetValue())
        current["Pitch"] = int(self.spin_pitch.GetValue())
        current["Name"] = str(self.text_voice_name.GetValue() or "").strip()

    def _load_engine_to_editor(self, engine_name):
        self.current_engine = str(engine_name or "")
        item = self.config.get("TTSInfos", {}).get(self.current_engine, {})
        self.spin_speed.SetValue(int(item.get("Speed", 100)))
        self.spin_volume.SetValue(int(item.get("Volume", 100)))
        self.spin_pitch.SetValue(int(item.get("Pitch", 50)))
        self.text_voice_name.SetValue(str(item.get("Name", "") or ""))

    def on_engine_changed(self, event):
        self._save_current_engine_from_editor()
        idx = self.choice_edit_engine.GetSelection()
        if idx != wx.NOT_FOUND and 0 <= idx < len(self.engine_names):
            self._load_engine_to_editor(self.engine_names[idx])
        event.Skip()

    def _collect_reader_ini_settings(self):
        return {
            "zdsr": {
                "type": self._get_choice_value(self.choice_zdsr_type),
                "channelName": str(self.text_zdsr_channel_name.GetValue() or "").strip(),
                "keyDownInterrupt": self._get_choice_value(self.choice_zdsr_key_interrupt),
                "alwaysInterrupt": self._get_choice_value(self.choice_zdsr_always_interrupt)
            },
            "boy": {
                "Param1": "true" if self.chk_boy_param1.GetValue() else "false",
                "Param2": "true" if self.chk_boy_param2.GetValue() else "false",
                "Param3": "true" if self.chk_boy_param3.GetValue() else "false",
                "Param4": "true" if self.chk_boy_param4.GetValue() else "false"
            }
        }

    def on_detect_reader_and_match_engine(self, event):
        if not callable(self.detect_reader_callback):
            self.text_detect_result.SetLabel("当前检测: 当前版本未接入检测回调")
            return
        detected = self.detect_reader_callback(self.engine_names)
        if not isinstance(detected, dict):
            self.text_detect_result.SetLabel("当前检测: 未识别到读屏")
            return
        reader_name = str(detected.get("reader_name", "") or "").strip()
        process_name = str(detected.get("process_name", "") or "").strip()
        reader_key = str(detected.get("reader_key", "") or "").strip().lower()
        matched_engine = str(detected.get("engine_name", "") or "").strip()
        if reader_name:
            self.text_detect_result.SetLabel(f"当前检测: {reader_name}（{process_name or '未知进程'}）")
        else:
            self.text_detect_result.SetLabel("当前检测: 未识别到读屏")

        # 检测到读屏时，先按读屏类型自动填充对应 ini 参数，避免用户手动选择。
        if reader_key == "zdsr":
            self._set_choice_by_value(self.choice_zdsr_type, "0")
        elif reader_key == "boy":
            self.chk_boy_param1.SetValue(False)
            self.chk_boy_param2.SetValue(False)
            self.chk_boy_param3.SetValue(True)
            self.chk_boy_param4.SetValue(True)

        if matched_engine and matched_engine in self.engine_names:
            idx = self.choice_default_engine.FindString(matched_engine)
            if idx != wx.NOT_FOUND:
                self.choice_default_engine.SetSelection(idx)
                self.choice_edit_engine.SetSelection(idx)
                self._load_engine_to_editor(matched_engine)

    def on_ok(self, event):
        interface_dll = self._get_choice_value(self.choice_interface_dll)
        if callable(self.on_save_interface_dll_callback):
            if not self.on_save_interface_dll_callback(interface_dll):
                return
        assist_hints = {"enabled": bool(self.chk_assist_enabled.GetValue())}
        if callable(self.on_save_assist_hints_callback):
            if not self.on_save_assist_hints_callback(assist_hints):
                return
        if not self.on_save_callback(self.config):
            return
        end_dialog_safely(self, wx.ID_OK)
