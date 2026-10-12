import re
import threading
import time

import wx

from ..common import parse_bool_setting
from ..constants import FIXED_HOTKEY_LABEL_BY_ACTION, FIXED_HOTKEY_TEXT_BY_ACTION
from ..ui.settings_dialogs import HotkeySettingsDialog


class HotkeysMixin:
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
