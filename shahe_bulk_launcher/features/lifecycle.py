import os
import threading
import time
import winreg

import wx

from ..bootstrap import append_daily_log, get_current_executable_path, is_packaged_app
from ..constants import LOG_OUTPUT_DETAILED, normalize_log_output_mode
from ..ui.controls import AppPathListCtrl, CheckableListCtrl, ManagedDelayedCall, OperationStatusText
from ..ui.tray import TaskBarIcon


class LifecycleMixin:
    def _log_operation(self, message):
        if normalize_log_output_mode(getattr(self, "log_output_mode", LOG_OUTPUT_DETAILED)) != LOG_OUTPUT_DETAILED:
            return
        append_daily_log(message)

    def _log_operation_safe(self, message):
        logger = getattr(self, "_log_operation", None)
        if callable(logger):
            try:
                logger(message)
            except Exception:
                pass

    def _refresh_sandbox_list_on_startup(self):
        try:
            if hasattr(self, "sandbox_list"):
                current_status = ""
                try:
                    current_status = str(self.lbl_status.GetLabel() or "").strip()
                except Exception:
                    current_status = ""
                if current_status and current_status not in {"正在刷新沙盒用户...", "就绪"}:
                    self._suppress_list_refresh_status(2.0)
                self.sandbox_list.on_refresh_sandbox_list(None)
        except Exception:
            try:
                self.lbl_status.SetLabel("刷新沙盒用户失败，请按 F5 重试")
            except Exception:
                pass

    def _apply_auto_run(self):
        key_path = r"Software\Microsoft\Windows\CurrentVersion\Run"
        app_path = get_current_executable_path()
        try:
            key = winreg.OpenKey(winreg.HKEY_CURRENT_USER, key_path, 0, winreg.KEY_SET_VALUE | winreg.KEY_QUERY_VALUE)
            if self.auto_run:
                winreg.SetValueEx(key, "SandboxLauncher", 0, winreg.REG_SZ, app_path)
            else:
                try:
                    winreg.DeleteValue(key, "SandboxLauncher")
                except:
                    pass
            winreg.CloseKey(key)
        except Exception as e:
            print(f"设置开机自启动失败: {e}")

    def create_task_bar_icon(self):
        if self.task_bar_icon is None:
            try:
                self.task_bar_icon = TaskBarIcon(self)
                if self.task_bar_icon.tb_icon is None:
                    self.task_bar_icon = None
                    self.show_tray = False
                    self.settings["show_tray"] = False
                    self.save_settings()
                    print("托盘图标创建失败，已禁用托盘功能")
            except Exception as e:
                print(f"创建托盘图标失败: {e}")
                self.task_bar_icon = None
                self.show_tray = False
                self.settings["show_tray"] = False
                self.save_settings()

    def remove_task_bar_icon(self):
        if self.task_bar_icon is not None:
            try:
                self.task_bar_icon.RemoveIcon()
                self.task_bar_icon = None
            except Exception as e:
                print(f"移除托盘图标失败: {e}")
                self.task_bar_icon = None

    def _disable_tray_display(self):
        was_hidden = False
        try:
            if not self._is_window_closing():
                was_hidden = not self.IsShown()
        except Exception:
            was_hidden = False
        self.remove_task_bar_icon()
        if was_hidden:
            try:
                if self.IsIconized():
                    self.Iconize(False)
            except Exception:
                pass
            try:
                self.Show(True)
            except Exception:
                pass
            try:
                self.Raise()
            except Exception:
                pass

    def _mark_window_closing(self):
        self._window_closing = True

    def _is_window_closing(self):
        return bool(getattr(self, "_window_closing", False))

    def _register_call_later(self, timer):
        if timer is None:
            return None
        try:
            pending = getattr(self, "_pending_call_laters", None)
            if pending is None:
                pending = []
                self._pending_call_laters = pending
            pending.append(timer)
        except Exception:
            pass
        return timer

    def _unregister_call_later(self, timer):
        if timer is None:
            return
        try:
            pending = getattr(self, "_pending_call_laters", None)
            if not pending:
                return
            self._pending_call_laters = [item for item in pending if item is not timer]
        except Exception:
            pass

    def _stop_pending_call_laters(self):
        try:
            pending = list(getattr(self, "_pending_call_laters", []) or [])
            self._pending_call_laters = []
        except Exception:
            pending = []
        for timer in pending:
            try:
                if timer is not None and hasattr(timer, "Stop"):
                    timer.Stop()
                elif timer is not None and hasattr(timer, "CancelWithoutNativeStop"):
                    timer.CancelWithoutNativeStop()
            except Exception:
                pass

    def _schedule_menu_bar_rebuild(self, delay_ms=40):
        if self._is_window_closing():
            return False
        self._menu_bar_rebuild_pending = True
        timer = getattr(self, "_menu_bar_rebuild_timer", None)
        if timer is not None:
            try:
                timer.Stop()
            except Exception:
                pass
        self._menu_bar_rebuild_timer = self._safe_call_later(delay_ms, self._flush_menu_bar_rebuild)
        return self._menu_bar_rebuild_timer is not None

    def _flush_menu_bar_rebuild(self):
        self._menu_bar_rebuild_timer = None
        if self._is_window_closing():
            return
        if not getattr(self, "_menu_bar_rebuild_pending", False):
            return
        self._menu_bar_rebuild_pending = False
        try:
            self.create_menu_bar()
        except Exception:
            pass

    def _schedule_sandbox_selection_process_state_refresh(self, delay_ms=160):
        if self._is_window_closing():
            return False
        self._invalidate_process_menu_state_cache()
        self._invalidate_process_menu_live_state_cache(clear_values=True)
        timer = getattr(self, "_sandbox_selection_process_state_timer", None)
        if timer is not None:
            try:
                timer.Stop()
            except Exception:
                pass
        self._sandbox_selection_process_state_timer = self._safe_call_later(
            delay_ms,
            self._flush_sandbox_selection_process_state_refresh
        )
        return self._sandbox_selection_process_state_timer is not None

    def _flush_sandbox_selection_process_state_refresh(self):
        self._sandbox_selection_process_state_timer = None
        if self._is_window_closing():
            return
        self._schedule_process_menu_live_state_refresh(delay_ms=1)
        self._schedule_menu_bar_rebuild(delay_ms=24)

    def _cancel_locate_window_reinforcement(self, wait=False):
        lock = getattr(self, "_locate_reinforcement_lock", None)
        if lock is None:
            return
        thread = None
        stop_event = None
        try:
            with lock:
                thread = getattr(self, "_locate_reinforcement_thread", None)
                stop_event = getattr(self, "_locate_reinforcement_event", None)
                self._locate_reinforcement_thread = None
                self._locate_reinforcement_event = None
        except Exception:
            thread = None
            stop_event = None
        try:
            if stop_event is not None:
                stop_event.set()
        except Exception:
            pass
        if wait and thread and thread is not threading.current_thread():
            try:
                thread.join(4.0)
            except Exception:
                pass

    def _safe_call_after(self, callback, *args, **kwargs):
        if self._is_window_closing():
            return

        def _runner():
            if self._is_window_closing():
                return
            try:
                callback(*args, **kwargs)
            except Exception:
                pass

        try:
            wx.CallAfter(_runner)
        except Exception:
            pass

    def _safe_call_later(self, delay_ms, callback, *args, **kwargs):
        if self._is_window_closing():
            return None

        def _runner():
            if self._is_window_closing():
                return
            try:
                callback(*args, **kwargs)
            except Exception:
                pass

        try:
            timer = ManagedDelayedCall(
                delay_ms,
                _runner,
                on_finish=self._unregister_call_later,
                thread_name="FrameSafeCallLater"
            )
            return self._register_call_later(timer)
        except Exception:
            return None

    def _safe_set_status_label(self, text):
        if self._is_window_closing():
            return False
        label = getattr(self, "lbl_status", None)
        if label is None:
            return False
        try:
            label.SetLabel(text)
            return True
        except RuntimeError:
            return False

    def _begin_launch_session(self):
        try:
            with self._launch_control_lock:
                self._launch_session_id = int(getattr(self, "_launch_session_id", 0) or 0) + 1
                self._launch_cancel_event.clear()
                return self._launch_session_id
        except Exception:
            try:
                self._launch_cancel_event.clear()
            except Exception:
                pass
            self._launch_session_id = int(getattr(self, "_launch_session_id", 0) or 0) + 1
            return self._launch_session_id

    def _cancel_active_launch(self):
        if not getattr(self, "is_launching", False):
            return False
        try:
            self._launch_cancel_event.set()
            return True
        except Exception:
            return False

    def _is_launch_session_cancelled(self, session_id):
        try:
            if int(session_id or 0) != int(getattr(self, "_launch_session_id", 0) or 0):
                return True
        except Exception:
            return True
        try:
            return bool(self._launch_cancel_event.is_set())
        except Exception:
            return False

    def _set_launch_status_if_active(self, session_id, text):
        if self._is_launch_session_cancelled(session_id):
            return False
        return self._safe_set_status_label(text)

    def _rename_sandbox_refs_in_active_launch_by_display(self, display_rename_map):
        targets = getattr(self, "_active_launch_targets", None)
        if not isinstance(targets, list) or not targets or not display_rename_map:
            return False
        lookup = {
            str(old or "").strip().lower(): str(new or "").strip()
            for old, new in display_rename_map.items()
            if str(old or "").strip() and str(new or "").strip()
        }
        if not lookup:
            return False
        changed = False
        for index, name in enumerate(list(targets)):
            if not str(name or "").strip():
                continue
            key = str(name or "").strip().lower()
            target_name = lookup.get(key)
            if target_name and target_name != name:
                targets[index] = target_name
                changed = True
        return changed

    def _delete_sandbox_refs_in_active_launch_by_display(self, names_to_delete):
        targets = getattr(self, "_active_launch_targets", None)
        if not isinstance(targets, list) or not targets:
            return False
        display_set = {str(name).strip() for name in (names_to_delete or []) if str(name).strip()}
        if not display_set:
            return False
        display_lookup = {name.lower() for name in display_set}
        changed = False
        has_remaining_targets = False
        for index, name in enumerate(list(targets)):
            key = str(name or "").strip().lower()
            if not key:
                continue
            if key in display_lookup:
                targets[index] = ""
                changed = True
            else:
                has_remaining_targets = True
        if not changed:
            return False
        if not has_remaining_targets:
            self._cancel_active_launch()
        return changed

    def _flush_ui_after_status_update(self):
        if self._is_window_closing():
            return False
        try:
            label = getattr(self, "lbl_status", None)
            if label is not None:
                try:
                    label.Refresh()
                    label.Update()
                except Exception:
                    pass
            self.Refresh()
            self.Update()
            return True
        except Exception:
            return False

    def _set_status_label_immediately(self, text):
        if not self._safe_set_status_label(text):
            return False
        self._flush_ui_after_status_update()
        return True

    def _suppress_list_refresh_status(self, seconds=1.5):
        try:
            self._suppress_list_refresh_status_until = max(
                getattr(self, "_suppress_list_refresh_status_until", 0.0),
                time.time() + float(seconds)
            )
        except Exception:
            self._suppress_list_refresh_status_until = time.time() + 1.5

    def _should_suppress_list_refresh_status(self):
        try:
            return time.time() < float(getattr(self, "_suppress_list_refresh_status_until", 0.0) or 0.0)
        except Exception:
            return False

    def init_ui(self):
        panel = wx.Panel(self)
        main_sizer = wx.BoxSizer(wx.VERTICAL)
        sandbox_sizer = wx.BoxSizer(wx.VERTICAL)
        lbl_sandboxes = wx.StaticText(panel, label="可用沙盒用户:")
        self.sandbox_list = CheckableListCtrl(
            panel,
            self.on_sandbox_toggled,
            on_mute_current_callback=self.on_mute_current_sandbox,
            on_mute_all_callback=self.on_mute_all_sandboxes,
            on_volume_current_callback=self.on_adjust_volume_current_sandbox,
            on_volume_all_callback=self.on_adjust_volume_all_sandboxes,
            on_switch_output_current_callback=self.on_switch_output_device_current_sandbox,
            on_switch_output_all_callback=self.on_switch_output_device_all_sandboxes,
            on_close_current_callback=self.on_close_current_sandbox,
            on_close_all_callback=self.on_close_all_sandboxes,
            on_locate_current_callback=self.on_locate_current_sandbox,
            on_pin_toggle_callback=self.on_toggle_pin_sandbox
        )
        sandbox_sizer.Add(lbl_sandboxes, 0, wx.ALL, 5)
        sandbox_sizer.Add(self.sandbox_list, 1, wx.EXPAND | wx.ALL, 5)
        main_sizer.Add(sandbox_sizer, 1, wx.EXPAND)

        app_list_sizer = wx.BoxSizer(wx.VERTICAL)
        lbl_app_path = wx.StaticText(panel, label="准备要在沙盒中打开的应用:")
        self.combo_app_path = AppPathListCtrl(panel)
        app_list_sizer.Add(lbl_app_path, 0, wx.ALL, 5)
        app_list_sizer.Add(self.combo_app_path, 1, wx.EXPAND | wx.LEFT | wx.RIGHT | wx.BOTTOM, 5)
        main_sizer.Add(app_list_sizer, 1, wx.EXPAND)

        button_sizer = wx.BoxSizer(wx.HORIZONTAL)
        self.btn_launch = wx.Button(panel, label="启动选中沙盒")
        self.btn_launch.Bind(wx.EVT_BUTTON, self.on_launch)
        self.btn_launch.SetBackgroundColour(wx.Colour(144, 238, 144))
        button_sizer.Add(self.btn_launch, 0)
        button_sizer.AddStretchSpacer()
        main_sizer.Add(button_sizer, 0, wx.EXPAND | wx.ALL, 5)
        self.update_notice_panel = wx.Panel(panel)
        update_notice_sizer = wx.BoxSizer(wx.HORIZONTAL)
        self.lbl_update_notice = wx.StaticText(self.update_notice_panel, label="")
        self.btn_download_update = wx.Button(self.update_notice_panel, label="下载更新")
        self.btn_download_update.Bind(wx.EVT_BUTTON, self.on_download_update)
        self.btn_download_update.SetToolTip("下载并安装检测到的新版本")
        update_notice_sizer.Add(self.lbl_update_notice, 1, wx.ALIGN_CENTER_VERTICAL | wx.RIGHT, 8)
        update_notice_sizer.Add(self.btn_download_update, 0)
        self.update_notice_panel.SetSizer(update_notice_sizer)
        self.update_notice_panel.Hide()
        main_sizer.Add(self.update_notice_panel, 0, wx.EXPAND | wx.LEFT | wx.RIGHT | wx.BOTTOM, 5)
        self.lbl_status = OperationStatusText(panel, label="正在初始化...", logger=self._log_operation)
        main_sizer.Add(self.lbl_status, 0, wx.EXPAND | wx.ALL, 5)
        self.combo_app_path.Bind(wx.EVT_LIST_ITEM_SELECTED, self.on_combo_text_changed)
        self.combo_app_path.Bind(wx.EVT_LIST_ITEM_CHECKED, self.on_app_item_checked_remember)
        self.combo_app_path.Bind(wx.EVT_CONTEXT_MENU, self.on_context_menu)
        self.Bind(wx.EVT_CLOSE, self.on_close)
        self.Bind(wx.EVT_CHAR_HOOK, self.on_frame_char_hook)
        self.update_combo_box()
        # 锁定主界面 Tab 顺序：用户列表 -> 应用列表 -> 启动按钮
        self.combo_app_path.MoveAfterInTabOrder(self.sandbox_list)
        self.btn_launch.MoveAfterInTabOrder(self.combo_app_path)
        self.update_notice_panel.MoveAfterInTabOrder(self.btn_launch)
        self._apply_hotkeys()
        self.create_menu_bar()
        panel.SetSizer(main_sizer)
        panel.Layout()

    def on_close(self, event):
        has_schedule_jobs = bool(getattr(self, "_schedule_jobs", {}))
        if self.force_exit_requested:
            self.Destroy()
            return
        if self.show_tray and self.task_bar_icon and self.task_bar_icon.tb_icon and event.CanVeto():
            self.Hide()
            event.Veto()
        else:
            self.Destroy()

    def _prepare_window_destroy(self, has_schedule_jobs=False):
        self._cancel_active_launch()
        self._mark_window_closing()
        self._cancel_locate_window_reinforcement(wait=True)
        self.on_cancel_all_schedules(None, update_status=has_schedule_jobs)
        self.remove_task_bar_icon()
        self._shutdown_assist_speech_runtime()
        self._stop_pending_call_laters()

    def Destroy(self):
        if getattr(self, "_destroy_later_scheduled", False):
            return False
        if getattr(self, "_destroy_started", False):
            return False
        self._destroy_started = True
        self._prepare_window_destroy(bool(getattr(self, "_schedule_jobs", {})))
        try:
            keys = list(getattr(self, "_schedule_jobs", {}).keys())
            for key in keys:
                self._cancel_schedule_task(key, update_status=False)
        except Exception:
            pass
        try:
            return super().Destroy()
        except RuntimeError:
            return False
