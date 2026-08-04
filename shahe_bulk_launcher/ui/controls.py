import os
import threading
import time

import wx
import wx.lib.mixins.listctrl as listmix

from ..constants import get_default_sandbox_root


class ManagedDelayedCall:
    def __init__(self, delay_ms, callback, on_finish=None, thread_name="ManagedDelayedCall"):
        self._delay_ms = int(max(0, delay_ms or 0))
        self._callback = callback if callable(callback) else None
        self._on_finish = on_finish if callable(on_finish) else None
        self._running = True
        self._timer = wx.CallLater(self._delay_ms, self._worker)

    def _finish(self):
        on_finish = self._on_finish
        self._on_finish = None
        if callable(on_finish):
            try:
                on_finish(self)
            except Exception:
                pass

    def _worker(self):
        try:
            callback = self._callback
            if callable(callback):
                try:
                    callback()
                except Exception:
                    pass
        finally:
            self._running = False
            self._finish()

    def Stop(self):
        self._callback = None
        timer = getattr(self, "_timer", None)
        if timer is not None:
            try:
                timer.Stop()
            except Exception:
                pass
        self._running = False
        self._finish()

    def CancelWithoutNativeStop(self):
        self._callback = None
        self._running = False
        self._finish()

    def IsRunning(self):
        return bool(self._running)

    def GetInterval(self):
        return int(self._delay_ms)

    Interval = property(GetInterval)

class OperationStatusText(wx.StaticText):
    def __init__(self, *args, logger=None, **kwargs):
        self._operation_logger = logger if callable(logger) else None
        self._last_logged_status = ""
        super().__init__(*args, **kwargs)

    def SetLabel(self, label):
        super().SetLabel(label)
        text = str(label or "").strip()
        if not text or text == self._last_logged_status:
            return
        self._last_logged_status = text
        logger = self._operation_logger
        if callable(logger):
            try:
                logger(f"状态: {text}")
            except Exception:
                pass

class SingleColumnListCtrl(wx.ListCtrl, listmix.ListCtrlAutoWidthMixin):
    def __init__(self, parent, column_title, width=520, enable_checkboxes=False):
        wx.ListCtrl.__init__(self, parent, style=wx.LC_REPORT | wx.LC_SINGLE_SEL)
        listmix.ListCtrlAutoWidthMixin.__init__(self)
        self.SetName(str(column_title or "列表"))
        self.InsertColumn(0, column_title, width=width)
        self._window_destroyed = False
        self._pending_call_laters = []
        if enable_checkboxes:
            self.EnableCheckBoxes(enable=True)
        self.Bind(wx.EVT_WINDOW_DESTROY, self._on_window_destroy)

    def _on_window_destroy(self, event):
        self._window_destroyed = True
        self._stop_pending_call_laters()
        event.Skip()

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
        try:
            self._state_refresh_pending_args = None
            self._state_refresh_in_progress = False
        except Exception:
            pass

    def _is_ui_alive(self):
        if bool(getattr(self, "_window_destroyed", False)):
            return False
        try:
            host = self.GetTopLevelParent()
        except RuntimeError:
            return False
        except Exception:
            host = None
        if host is not None:
            try:
                checker = getattr(host, "_is_window_closing", None)
                if callable(checker) and checker():
                    return False
            except Exception:
                pass
        return True

    def _safe_call_after(self, callback, *args, **kwargs):
        try:
            if not self._is_ui_alive():
                return
        except Exception:
            return

        def _runner():
            if not self._is_ui_alive():
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
        try:
            if not self._is_ui_alive():
                return None
        except Exception:
            return None

        def _runner():
            if not self._is_ui_alive():
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
                thread_name="ListSafeCallLater"
            )
            return self._register_call_later(timer)
        except Exception:
            return None

    def _get_item_identity_from_text(self, item_text):
        return str(item_text or "").strip()

    def append_item(self, text):
        return self.InsertItem(self.GetItemCount(), str(text or ""))
    def _replace_item_texts_fast(self, item_texts):
        new_texts = [str(text or "").strip() for text in (item_texts or []) if str(text or "").strip()]
        old_count = self.GetItemCount()
        old_texts = [self.GetItemText(index) for index in range(old_count)]
        if old_texts == new_texts:
            return

        old_identities = [self._get_item_identity_from_text(text) for text in old_texts]
        new_identities = [self._get_item_identity_from_text(text) for text in new_texts]
        if old_count == len(new_texts) and old_identities == new_identities:
            changed = 0
            for index, text in enumerate(new_texts):
                if old_texts[index] != text:
                    self.SetItem(index, 0, text)
                    changed += 1
            if changed:
                try:
                    self.RefreshItems(0, self.GetItemCount() - 1)
                except Exception:
                    self.Refresh()
            return

        checked_identities = set()
        checkable = hasattr(self, "IsItemChecked") and hasattr(self, "CheckItem")
        if checkable:
            for index in range(old_count):
                try:
                    if self.IsItemChecked(index):
                        checked_identities.add(self._get_item_identity_from_text(old_texts[index]))
                except Exception:
                    pass

        selected_identity = ""
        selected_index = self.GetFirstSelected()
        restore_selected_index = -1
        if selected_index != -1:
            selected_identity = self._get_item_identity_from_text(old_texts[selected_index])

        self.DeleteAllItems()
        if hasattr(self, "_current_value"):
            self._current_value = ""
        if hasattr(self, "context_item_index"):
            self.context_item_index = -1
        if hasattr(self, "_last_checked_index"):
            self._last_checked_index = None
        for text in new_texts:
            index = self.append_item(text)
            identity = self._get_item_identity_from_text(text)
            if checkable and identity in checked_identities:
                try:
                    wx.ListCtrl.CheckItem(self, index, True)
                except Exception:
                    pass
            if selected_identity and identity == selected_identity:
                restore_selected_index = index

        if restore_selected_index != -1:
            try:
                self.Select(restore_selected_index)
                self.Focus(restore_selected_index)
                self.EnsureVisible(restore_selected_index)
                if hasattr(self, "_current_value"):
                    self._current_value = self.GetItemText(restore_selected_index)
                if hasattr(self, "context_item_index"):
                    self.context_item_index = restore_selected_index
            except Exception:
                if hasattr(self, "_current_value"):
                    self._current_value = ""
                if hasattr(self, "context_item_index"):
                    self.context_item_index = -1
        elif self.GetItemCount() > 0 and selected_index == -1:
            try:
                self.Focus(0)
            except Exception:
                pass
    def Clear(self):
        self.DeleteAllItems()
        if hasattr(self, "_current_value"):
            try:
                self._current_value = ""
            except Exception:
                pass
        if hasattr(self, "context_item_index"):
            try:
                self.context_item_index = -1
            except Exception:
                pass

class AppPathListCtrl(SingleColumnListCtrl):
    """Checkable application list with the same wx.ListCtrl checkbox style as the sandbox user list."""
    def __init__(self, parent):
        super().__init__(parent, '准备要在沙盒中打开的应用', width=520, enable_checkboxes=True)
        self._current_value = ""
        self._programmatic_update_depth = 0
        self._programmatic_until_idle = False
        self._last_checked_index = None
        self._batch_checking = False
        self.Bind(wx.EVT_LIST_ITEM_SELECTED, self.on_item_selected)
        self.Bind(wx.EVT_LIST_ITEM_DESELECTED, self.on_item_deselected)
        self.Bind(wx.EVT_KEY_DOWN, self.on_key_down)
        self.Bind(wx.EVT_LEFT_DOWN, self.on_left_down)
        self.Bind(wx.EVT_LIST_ITEM_CHECKED, self.on_item_checked)
        self.Bind(wx.EVT_LIST_ITEM_UNCHECKED, self.on_item_unchecked)
        try:
            accessible = self.GetAccessible()
            if accessible:
                accessible.SetName('准备要在沙盒中打开的应用，复选列表，按空格选中或取消选中')
        except Exception:
            pass

    def begin_programmatic_update(self):
        self._programmatic_update_depth += 1
        self._programmatic_until_idle = True

    def end_programmatic_update(self):
        if self._programmatic_update_depth > 0:
            self._programmatic_update_depth -= 1
        wx.CallAfter(self._clear_programmatic_until_idle)

    def _clear_programmatic_until_idle(self):
        if self._programmatic_update_depth <= 0:
            self._programmatic_until_idle = False

    def is_programmatic_update(self):
        return self._programmatic_update_depth > 0 or self._programmatic_until_idle

    def get_checked_items(self):
        checked_items = []
        for i in range(self.GetItemCount()):
            try:
                if self.IsItemChecked(i):
                    checked_items.append(i)
            except Exception:
                pass
        return checked_items

    def get_checked_values(self):
        values = []
        for index in self.get_checked_items():
            text = self.GetItemText(index)
            if text:
                values.append(text)
        return values

    def update_status(self):
        # Keep the same lightweight status calculation style as the user list.
        return len(self.get_checked_items())

    def announce_item_state(self, item_index):
        if item_index < 0 or item_index >= self.GetItemCount():
            return
        item_text = self.GetItemText(item_index)
        checked = False
        try:
            checked = bool(self.IsItemChecked(item_index))
        except Exception:
            checked = False
        status_text = f"{'已选中' if checked else '未选中'} {item_text} 应用"
        try:
            accessible = self.GetAccessible()
            if accessible:
                accessible.SetName(status_text)
        except Exception:
            pass

    def on_item_selected(self, event):
        event.Skip()
        index = event.GetIndex()
        if index >= 0:
            self._current_value = self.GetItemText(index)
            try:
                self.announce_item_state(index)
            except Exception:
                pass

    def on_item_deselected(self, event):
        event.Skip()

    def on_item_checked(self, event):
        if not self._batch_checking and not self.is_programmatic_update():
            self.announce_item_state(event.GetIndex())
        event.Skip()

    def on_item_unchecked(self, event):
        if not self._batch_checking and not self.is_programmatic_update():
            self.announce_item_state(event.GetIndex())
        event.Skip()

    def on_key_down(self, event):
        key_code = event.GetKeyCode()
        if key_code == wx.WXK_SPACE:
            current_item = self.GetFirstSelected()
            if current_item != -1:
                is_checked = self.IsItemChecked(current_item)
                self.CheckItem(current_item, not is_checked)
                self.announce_item_state(current_item)
            event.Skip(False)
        else:
            event.Skip()

    def on_left_down(self, event):
        event.Skip()
        pos = event.GetPosition()
        item, flags = self.HitTest(pos)
        if item != wx.NOT_FOUND and flags & wx.LIST_HITTEST_ONITEM:
            self._safe_call_later(50, self.announce_item_state, item)

    def replace_items(self, item_texts):
        self._replace_item_texts_fast(item_texts)
        self._current_value = ""

    def SetValue(self, text):
        value = str(text or "")
        self._current_value = value
        self.begin_programmatic_update()
        try:
            found_index = -1
            for i in range(self.GetItemCount()):
                if self.GetItemText(i) == value:
                    found_index = i
                    break
            current = self.GetFirstSelected()
            if current != -1 and current != found_index:
                self.Select(current, 0)
            if found_index != -1:
                self.Select(found_index)
                self.Focus(found_index)
                self.EnsureVisible(found_index)
        finally:
            self.end_programmatic_update()

    def GetValue(self):
        index = self.GetFirstSelected()
        if index != -1:
            return self.GetItemText(index)
        return self._current_value

    def SetValueByIndex(self, index, text=None):
        value = str(text or "")
        self.begin_programmatic_update()
        try:
            try:
                target_index = int(index)
            except Exception:
                target_index = -1
            current = self.GetFirstSelected()
            if current != -1 and current != target_index:
                self.Select(current, 0)
            if 0 <= target_index < self.GetItemCount():
                item_text = self.GetItemText(target_index)
                self._current_value = item_text
                self.Select(target_index)
                self.Focus(target_index)
                self.EnsureVisible(target_index)
            else:
                self._current_value = value
        finally:
            self.end_programmatic_update()

class CheckableListCtrl(SingleColumnListCtrl):
    GROUP_PREFIX = "[分组] "
    RUNNING_SUFFIX = "【运行中】"
    IDLE_SUFFIX = "【空闲中】"

    def __init__(self, parent, on_toggle_callback=None, on_mute_current_callback=None, on_mute_all_callback=None, on_volume_current_callback=None, on_volume_all_callback=None, on_switch_output_current_callback=None, on_switch_output_all_callback=None, on_close_current_callback=None, on_close_all_callback=None, on_locate_current_callback=None, on_pin_toggle_callback=None):
        super().__init__(parent, "沙盒用户", width=200, enable_checkboxes=True)
        self.on_toggle_callback = on_toggle_callback
        self.on_mute_current_callback = on_mute_current_callback
        self.on_mute_all_callback = on_mute_all_callback
        self.on_volume_current_callback = on_volume_current_callback
        self.on_volume_all_callback = on_volume_all_callback
        self.on_switch_output_current_callback = on_switch_output_current_callback
        self.on_switch_output_all_callback = on_switch_output_all_callback
        self.on_close_current_callback = on_close_current_callback
        self.on_close_all_callback = on_close_all_callback
        self.on_locate_current_callback = on_locate_current_callback
        self.on_pin_toggle_callback = on_pin_toggle_callback
        self.parent = parent
        self.SANDBOX_ROOT = get_default_sandbox_root()
        self.context_item_index = -1
        self.current_group_view = ""
        self.range_auto_check_enabled = False
        self.left_right_navigation_enabled = False
        self._last_checked_index = None
        self._batch_checking = False
        self._refresh_token = 0
        self._list_refresh_in_progress = False
        self._list_refresh_pending = False
        self._list_refresh_pending_scheduled = False
        self._list_refresh_last_started_at = 0.0
        self._state_refresh_in_progress = False
        self._state_refresh_pending_args = None
        self._group_item_indexes = set()
        self.Bind(wx.EVT_LIST_ITEM_SELECTED, self.on_item_selected)
        self.Bind(wx.EVT_LIST_ITEM_DESELECTED, self.on_item_deselected)
        self.Bind(wx.EVT_KEY_DOWN, self.on_key_down)
        self.Bind(wx.EVT_LEFT_DOWN, self.on_left_down)
        self.Bind(wx.EVT_LIST_ITEM_CHECKED, self.on_item_checked)
        self.Bind(wx.EVT_LIST_ITEM_UNCHECKED, self.on_item_unchecked)
        self.Bind(wx.EVT_CONTEXT_MENU, self.on_context_menu)

    def _set_special_item_indexes(self, group_indexes=None):
        self._group_item_indexes = {int(index) for index in (group_indexes or []) if int(index) >= 0}

    def _clear_special_item_indexes(self):
        self._group_item_indexes = set()

    def _is_group_index(self, index):
        try:
            return int(index) in self._group_item_indexes
        except Exception:
            return False

    def _is_special_index(self, index):
        return self._is_group_index(index)

    def set_range_auto_check_enabled(self, enabled):
        self.range_auto_check_enabled = bool(enabled)
        self._last_checked_index = None

    def set_left_right_navigation_enabled(self, enabled):
        self.left_right_navigation_enabled = bool(enabled)

    def set_sandbox_root(self, sandbox_root):
        value = str(sandbox_root or "").strip()
        if value:
            self.SANDBOX_ROOT = value

    def get_sandbox_root(self):
        return self.SANDBOX_ROOT

    @classmethod
    def format_group_item_text(cls, group_name):
        return f"{cls.GROUP_PREFIX}{group_name}"

    def is_group_item_text(self, item_text):
        text = str(item_text or "").strip()
        if not text:
            return False
        if not text.startswith(self.GROUP_PREFIX):
            return False
        group_name = text[len(self.GROUP_PREFIX):].strip()
        if not group_name:
            return False
        host = None
        try:
            host = self.GetTopLevelParent()
        except Exception:
            host = None
        if host is None:
            return False
        try:
            groups = getattr(host, "sandbox_groups", {}) or {}
        except Exception:
            groups = {}
        return group_name in groups

    def get_group_name_from_item_text(self, item_text):
        text = str(item_text or "").strip()
        if self.is_group_item_text(text):
            return text[len(self.GROUP_PREFIX):].strip()
        return ""

    @classmethod
    def format_sandbox_item_text(cls, sandbox_name, is_running):
        suffix = cls.RUNNING_SUFFIX if is_running else cls.IDLE_SUFFIX
        return f"{sandbox_name} {suffix}"

    @classmethod
    def get_sandbox_name_from_item_text(cls, item_text):
        text = str(item_text or "").strip()
        running_marker = f" {cls.RUNNING_SUFFIX}"
        idle_marker = f" {cls.IDLE_SUFFIX}"
        if text.endswith(running_marker):
            return text[:-len(running_marker)].rstrip()
        if text.endswith(idle_marker):
            return text[:-len(idle_marker)].rstrip()
        return text

    def get_item_identity_text(self, item_index):
        if item_index < 0 or item_index >= self.GetItemCount():
            return ""
        item_text = self.GetItemText(item_index)
        if self._is_special_index(item_index):
            return item_text
        return self.get_sandbox_name_from_item_text(item_text)

    def _get_item_identity_from_text(self, item_text):
        return self._get_item_identity_from_text_with_kinds(
            item_text,
            self.is_group_item_text(item_text)
        )

    def _get_item_identity_from_text_with_kinds(self, item_text, is_group=False):
        text = str(item_text or "").strip()
        if is_group:
            return text
        return self.get_sandbox_name_from_item_text(text)

    def _replace_item_texts_fast(self, item_texts, group_indexes=None):
        new_texts = [str(text or "").strip() for text in (item_texts or []) if str(text or "").strip()]
        new_group_indexes = {int(index) for index in (group_indexes or []) if int(index) >= 0}
        old_count = self.GetItemCount()
        old_texts = [self.GetItemText(index) for index in range(old_count)]
        if old_texts == new_texts:
            self._set_special_item_indexes(new_group_indexes)
            return

        old_identities = [self.get_item_identity_text(index) for index in range(old_count)]
        new_identities = [
            self._get_item_identity_from_text_with_kinds(
                text,
                index in new_group_indexes
            )
            for index, text in enumerate(new_texts)
        ]
        if old_count == len(new_texts) and old_identities == new_identities:
            changed = 0
            for index, text in enumerate(new_texts):
                if old_texts[index] != text:
                    self.SetItem(index, 0, text)
                    changed += 1
            self._set_special_item_indexes(new_group_indexes)
            if changed:
                try:
                    self.RefreshItems(0, self.GetItemCount() - 1)
                except Exception:
                    self.Refresh()
            return

        checked_identities = set()
        for index in range(old_count):
            try:
                if self.IsItemChecked(index):
                    checked_identities.add(self.get_item_identity_text(index))
            except Exception:
                pass
        selected_identity = ""
        selected_index = self.GetFirstSelected()
        if selected_index != -1:
            selected_identity = self.get_item_identity_text(selected_index)

        self.DeleteAllItems()
        self._last_checked_index = None
        restore_selected_index = -1
        for text in new_texts:
            index = self.append_item(text)
            identity = self._get_item_identity_from_text_with_kinds(
                text,
                index in new_group_indexes
            )
            if identity in checked_identities:
                try:
                    wx.ListCtrl.CheckItem(self, index, True)
                except Exception:
                    pass
            if selected_identity and identity == selected_identity:
                restore_selected_index = index
        self._set_special_item_indexes(new_group_indexes)

        if restore_selected_index != -1:
            try:
                self.Select(restore_selected_index)
                self.Focus(restore_selected_index)
                self.EnsureVisible(restore_selected_index)
                if hasattr(self, "_current_value"):
                    self._current_value = self.GetItemText(restore_selected_index)
                if hasattr(self, "context_item_index"):
                    self.context_item_index = restore_selected_index
            except Exception:
                if hasattr(self, "_current_value"):
                    self._current_value = ""
                if hasattr(self, "context_item_index"):
                    self.context_item_index = -1
        elif self.GetItemCount() > 0 and selected_index == -1:
            try:
                self.Focus(0)
            except Exception:
                pass
        else:
            if hasattr(self, "_current_value"):
                self._current_value = ""
            if hasattr(self, "context_item_index"):
                self.context_item_index = -1

    def get_checked_entry_texts(self):
        return [self.get_item_identity_text(i) for i in self.get_checked_items()]

    def on_item_selected(self, event):
        event.Skip()
        host = self.GetTopLevelParent()
        if hasattr(host, "_schedule_sandbox_selection_process_state_refresh"):
            host._schedule_sandbox_selection_process_state_refresh()
        try:
            self.announce_item_state(event.GetIndex())
        except Exception:
            pass

    def on_item_deselected(self, event):
        event.Skip()
        host = self.GetTopLevelParent()
        if hasattr(host, "_schedule_sandbox_selection_process_state_refresh"):
            host._schedule_sandbox_selection_process_state_refresh()

    def on_key_down(self, event):
        key_code = event.GetKeyCode()
        if self.left_right_navigation_enabled and key_code in (wx.WXK_LEFT, wx.WXK_RIGHT):
            if self.GetItemCount() <= 0:
                event.Skip(False)
                return
            current_item = self.GetFirstSelected()
            if current_item == -1:
                target = 0 if key_code == wx.WXK_RIGHT else self.GetItemCount() - 1
            else:
                step = -1 if key_code == wx.WXK_LEFT else 1
                target = max(0, min(self.GetItemCount() - 1, current_item + step))
            if current_item != -1 and current_item != target:
                self.Select(current_item, 0)
            self.Select(target)
            self.Focus(target)
            self.EnsureVisible(target)
            self._safe_call_later(50, self.announce_item_state, target)
            event.Skip(False)
            return
        if key_code == wx.WXK_SPACE:
            current_item = self.GetFirstSelected()
            if current_item != -1:
                if not self._is_special_index(current_item):
                    is_checked = self.IsItemChecked(current_item)
                    self.CheckItem(current_item, not is_checked)
            event.Skip(False)
        elif key_code == wx.WXK_BACK:
            if self.current_group_view:
                self.current_group_view = ""
                self.on_refresh_sandbox_list(None)
                event.Skip(False)
                return
            event.Skip()
        elif key_code in (wx.WXK_RETURN, wx.WXK_NUMPAD_ENTER):
            current_item = self.GetFirstSelected()
            if current_item != -1:
                if self._is_group_index(current_item):
                    item_text = self.GetItemText(current_item)
                    self.current_group_view = self.get_group_name_from_item_text(item_text)
                    self.on_refresh_sandbox_list(None)
                    event.Skip(False)
                    return
            event.Skip(False)
        elif key_code == wx.WXK_F5:
            self.on_fast_refresh_sandbox_list(event)
            event.Skip(False)
        elif event.ControlDown() and key_code == ord('A'):
            self.toggle_select_all()
            event.Skip(False)
        else:
            event.Skip()

    def on_left_down(self, event):
        event.Skip()
        pos = event.GetPosition()
        item, flags = self.HitTest(pos)
        if item != wx.NOT_FOUND and flags & wx.LIST_HITTEST_ONITEM:
            self._safe_call_later(50, self.announce_item_state, item)

    def on_context_menu(self, event):
        host = self.GetTopLevelParent()
        self.context_item_index = -1
        pos = event.GetPosition()
        if pos != wx.DefaultPosition:
            item, flags = self.HitTest(self.ScreenToClient(pos))
            if item != wx.NOT_FOUND and flags & wx.LIST_HITTEST_ONITEM:
                current = self.GetFirstSelected()
                if current != -1 and current != item:
                    self.Select(current, 0)
                self.context_item_index = item
                self.Select(item)
                self.Focus(item)
            self.context_item_index = self.GetFirstSelected()
        else:
            self.context_item_index = self.GetFirstSelected()

        sandbox_name = ""
        if self.context_item_index != -1 and self.context_item_index < self.GetItemCount():
            selected_text = self.get_item_identity_text(self.context_item_index)
            if not self._is_special_index(self.context_item_index):
                sandbox_name = selected_text

        # Keep the context menu fast: do not query Sandboxie / enumerate
        # windows while the menu is opening.  Use the visible list's cached
        # running marker instead; actions themselves still re-check live PIDs.
        selected_has_process = False
        all_has_process = False
        selected_has_window = False
        all_has_window = False
        if sandbox_name and self.context_item_index != -1 and self.context_item_index < self.GetItemCount():
            try:
                raw_item_text = self.GetItemText(self.context_item_index)
                selected_has_process = raw_item_text.strip().endswith(self.RUNNING_SUFFIX)
            except Exception:
                selected_has_process = False
        try:
            for _idx in range(self.GetItemCount()):
                if self._is_special_index(_idx):
                    continue
                _text = self.GetItemText(_idx)
                if str(_text or "").strip().endswith(self.RUNNING_SUFFIX):
                    all_has_process = True
                    break
        except Exception:
            all_has_process = False
        if hasattr(host, "_resolve_all_has_process_for_menu_state"):
            try:
                all_has_process = bool(host._resolve_all_has_process_for_menu_state(all_has_process))
            except Exception:
                pass
        live_process_state = {}
        if hasattr(host, "_get_process_menu_live_state"):
            try:
                live_process_state = host._get_process_menu_live_state()
            except Exception:
                live_process_state = {}
        selected_has_window = bool(selected_has_process)
        all_has_window = all_has_process
        selected_audio_session = bool(selected_has_process)
        all_audio_session = bool(all_has_process)
        selected_mute_state = live_process_state.get("selected_mute_state") if selected_has_process else None
        all_mute_state = live_process_state.get("all_mute_state") if all_has_process else None
        output_devices_available = bool(selected_has_process or all_has_process)

        menu = wx.Menu()
        select_all_item = menu.Append(wx.ID_ANY, "全选", "全选所有沙盒用户")
        self.Bind(wx.EVT_MENU, self.on_select_all, select_all_item)
        select_none_item = menu.Append(wx.ID_ANY, "全都不选", "取消选择所有沙盒用户")
        self.Bind(wx.EVT_MENU, self.on_select_none, select_none_item)
        if sandbox_name and callable(self.on_pin_toggle_callback):
            is_pinned = False
            if hasattr(host, "is_sandbox_pinned"):
                try:
                    is_pinned = bool(host.is_sandbox_pinned(sandbox_name))
                except Exception:
                    is_pinned = False
            menu.AppendSeparator()
            pin_label = "取消置顶当前用户" if is_pinned else "置顶当前用户"
            pin_help = "将该沙盒用户固定在列表前面" if not is_pinned else "取消该沙盒用户置顶状态"
            pin_item = menu.Append(wx.ID_ANY, pin_label, pin_help)
            self.Bind(wx.EVT_MENU, self.on_toggle_pin_current_user, pin_item)
        supports_mute_actions = callable(self.on_mute_current_callback) and callable(self.on_mute_all_callback)
        if supports_mute_actions:
            menu.AppendSeparator()

            mute_current_label = "静音当前打开的进程"
            mute_current_help = "切换当前打开的进程静音状态"
            mute_current_enabled = selected_audio_session
            if mute_current_enabled:
                if selected_mute_state is True:
                    mute_current_label = "取消静音当前打开的进程"
            else:
                mute_current_label = "静音当前打开的进程"
                mute_current_help = "当前用户没有可静音的已开进程"
            mute_current_item = menu.Append(wx.ID_ANY, mute_current_label, mute_current_help)
            if mute_current_enabled:
                self.Bind(wx.EVT_MENU, self.on_mute_current_user, mute_current_item)
            else:
                mute_current_item.Enable(False)

            mute_all_label = "静音全部打开的进程"
            mute_all_help = "切换全部打开的进程静音状态"
            mute_all_enabled = all_audio_session
            if mute_all_enabled:
                if all_mute_state is True:
                    mute_all_label = "取消静音全部打开的进程"
            elif not mute_all_enabled:
                mute_all_label = "静音全部打开的进程"
                mute_all_help = "当前没有可静音的全部打开进程"
            mute_all_item = menu.Append(wx.ID_ANY, mute_all_label, mute_all_help)
            if mute_all_enabled:
                self.Bind(wx.EVT_MENU, self.on_mute_all_users, mute_all_item)
            else:
                mute_all_item.Enable(False)

        supports_volume_actions = callable(self.on_volume_current_callback) and callable(self.on_volume_all_callback)
        if supports_volume_actions:
            menu.AppendSeparator()

            volume_current_enabled = selected_audio_session

            volume_current_up_label = "提高当前打开的进程音量（+5%）"
            volume_current_up_help = "提高当前用户打开进程音量"
            if not volume_current_enabled:
                volume_current_up_label = "提高当前打开的进程音量（+5%）"
                volume_current_up_help = "当前用户没有可调节音量的已开进程"
            volume_current_up_item = menu.Append(wx.ID_ANY, volume_current_up_label, volume_current_up_help)
            if volume_current_enabled:
                self.Bind(wx.EVT_MENU, lambda evt: self.on_volume_current_user(evt, 5), volume_current_up_item)
            else:
                volume_current_up_item.Enable(False)

            volume_all_enabled = all_audio_session

            volume_current_down_label = "降低当前打开的进程音量（-5%）"
            volume_current_down_help = "降低当前用户打开进程音量"
            if not volume_current_enabled:
                volume_current_down_label = "降低当前打开的进程音量（-5%）"
                volume_current_down_help = "当前用户没有可调节音量的已开进程"
            volume_current_down_item = menu.Append(wx.ID_ANY, volume_current_down_label, volume_current_down_help)
            if volume_current_enabled:
                self.Bind(wx.EVT_MENU, lambda evt: self.on_volume_current_user(evt, -5), volume_current_down_item)
            else:
                volume_current_down_item.Enable(False)

            volume_all_up_label = "提高全部打开的进程音量（+5%）"
            volume_all_up_help = "提高全部打开进程音量"
            if not volume_all_enabled:
                volume_all_up_label = "提高全部打开的进程音量（+5%）"
                volume_all_up_help = "当前没有可调节音量的全部打开进程"
            volume_all_up_item = menu.Append(wx.ID_ANY, volume_all_up_label, volume_all_up_help)
            if volume_all_enabled:
                self.Bind(wx.EVT_MENU, lambda evt: self.on_volume_all_users(evt, 5), volume_all_up_item)
            else:
                volume_all_up_item.Enable(False)

            volume_all_down_label = "降低全部打开的进程音量（-5%）"
            volume_all_down_help = "降低全部打开进程音量"
            if not volume_all_enabled:
                volume_all_down_label = "降低全部打开的进程音量（-5%）"
                volume_all_down_help = "当前没有可调节音量的全部打开进程"
            volume_all_down_item = menu.Append(wx.ID_ANY, volume_all_down_label, volume_all_down_help)
            if volume_all_enabled:
                self.Bind(wx.EVT_MENU, lambda evt: self.on_volume_all_users(evt, -5), volume_all_down_item)
            else:
                volume_all_down_item.Enable(False)

        supports_switch_output_actions = callable(self.on_switch_output_current_callback) or callable(self.on_switch_output_all_callback)
        if supports_switch_output_actions:
            menu.AppendSeparator()
            if callable(self.on_switch_output_current_callback):
                switch_current_label = "切换当前打开的进程播放设备"
                switch_current_help = "切换当前用户打开进程的播放设备"
                switch_current_enabled = selected_has_process and output_devices_available
                if not selected_has_process:
                    switch_current_help = "当前用户没有可切换播放设备的已开进程"
                elif not output_devices_available:
                    switch_current_help = "当前没有可用的播放设备"
                switch_current_item = menu.Append(wx.ID_ANY, switch_current_label, switch_current_help)
                if switch_current_enabled:
                    self.Bind(wx.EVT_MENU, self.on_switch_output_current_user, switch_current_item)
                else:
                    switch_current_item.Enable(False)

            if callable(self.on_switch_output_all_callback):
                switch_all_label = "切换全部打开的进程播放设备"
                switch_all_help = "切换全部打开进程的播放设备"
                switch_all_enabled = all_has_process and output_devices_available
                if not all_has_process:
                    switch_all_help = "当前没有可切换播放设备的全部打开进程"
                elif not output_devices_available:
                    switch_all_help = "当前没有可用的播放设备"
                switch_all_item = menu.Append(wx.ID_ANY, switch_all_label, switch_all_help)
                if switch_all_enabled:
                    self.Bind(wx.EVT_MENU, self.on_switch_output_all_users, switch_all_item)
                else:
                    switch_all_item.Enable(False)

        supports_close_actions = callable(self.on_close_current_callback) or callable(self.on_close_all_callback)
        if supports_close_actions:
            menu.AppendSeparator()
            if callable(self.on_close_current_callback):
                close_current_label = "关闭当前打开的进程"
                close_current_help = "结束当前用户打开的进程"
                close_current_enabled = selected_has_process
                if not close_current_enabled:
                    close_current_label = "关闭当前打开的进程"
                    close_current_help = "当前用户没有可关闭的已开进程"
                close_current_item = menu.Append(wx.ID_ANY, close_current_label, close_current_help)
                if close_current_enabled:
                    self.Bind(wx.EVT_MENU, self.on_close_current_window, close_current_item)
                else:
                    close_current_item.Enable(False)

            if callable(self.on_close_all_callback):
                close_all_label = "关闭全部打开的进程"
                close_all_help = "结束全部打开的进程"
                close_all_enabled = all_has_process
                if not close_all_enabled:
                    close_all_label = "关闭全部打开的进程"
                    close_all_help = "当前没有可关闭的全部打开进程"
                close_all_item = menu.Append(wx.ID_ANY, close_all_label, close_all_help)
                if close_all_enabled:
                    self.Bind(wx.EVT_MENU, self.on_close_all_windows, close_all_item)
                else:
                    close_all_item.Enable(False)

        supports_locate_actions = callable(self.on_locate_current_callback)
        if supports_locate_actions:
            menu.AppendSeparator()
            locate_label = "定位当前打开的进程"
            locate_help = "切换并激活当前用户的打开进程"
            locate_enabled = selected_has_window
            if not locate_enabled:
                locate_label = "定位当前打开的进程"
                locate_help = "当前用户没有可定位的打开进程"
            locate_item = menu.Append(wx.ID_ANY, locate_label, locate_help)
            if locate_enabled:
                self.Bind(wx.EVT_MENU, self.on_locate_current_window, locate_item)
            else:
                locate_item.Enable(False)

        menu.AppendSeparator()
        refresh_item = menu.Append(wx.ID_ANY, "刷新沙盒用户", "刷新沙盒用户列表")
        self.Bind(wx.EVT_MENU, self.on_refresh_sandbox_list, refresh_item)
        self.PopupMenu(menu)
        menu.Destroy()

    def on_mute_current_user(self, event):
        if self.context_item_index == -1 or self.context_item_index >= self.GetItemCount():
            return
        if self.on_mute_current_callback:
            self.on_mute_current_callback(self.get_item_identity_text(self.context_item_index))

    def on_mute_all_users(self, event):
        if self.on_mute_all_callback:
            self.on_mute_all_callback()

    def on_volume_current_user(self, event, step_percent=5):
        if self.context_item_index == -1 or self.context_item_index >= self.GetItemCount():
            return
        if self.on_volume_current_callback:
            self.on_volume_current_callback(self.get_item_identity_text(self.context_item_index), step_percent)

    def on_volume_all_users(self, event, step_percent=5):
        if self.on_volume_all_callback:
            self.on_volume_all_callback(step_percent)

    def on_switch_output_current_user(self, event):
        if self.context_item_index == -1 or self.context_item_index >= self.GetItemCount():
            return
        if self.on_switch_output_current_callback:
            self.on_switch_output_current_callback(self.get_item_identity_text(self.context_item_index))

    def on_switch_output_all_users(self, event):
        if self.on_switch_output_all_callback:
            self.on_switch_output_all_callback()

    def on_close_all_windows(self, event):
        if self.on_close_all_callback:
            self.on_close_all_callback()

    def on_close_current_window(self, event):
        if self.context_item_index == -1 or self.context_item_index >= self.GetItemCount():
            return
        if self.on_close_current_callback:
            self.on_close_current_callback(self.get_item_identity_text(self.context_item_index))

    def on_locate_current_window(self, event):
        if self.context_item_index == -1 or self.context_item_index >= self.GetItemCount():
            return
        if self.on_locate_current_callback:
            self.on_locate_current_callback(self.get_item_identity_text(self.context_item_index))

    def on_toggle_pin_current_user(self, event):
        if self.context_item_index == -1 or self.context_item_index >= self.GetItemCount():
            return
        if self.on_pin_toggle_callback:
            self.on_pin_toggle_callback(self.get_item_identity_text(self.context_item_index))

    def on_select_all(self, event):
        self.check_all()

    def on_select_none(self, event):
        self.uncheck_all()

    def toggle_select_all(self):
        if self.GetItemCount() > 0:
            checkable_indexes = []
            for i in range(self.GetItemCount()):
                if self._is_special_index(i):
                    continue
                checkable_indexes.append(i)
            if not checkable_indexes:
                return
            all_checked = all(self.IsItemChecked(i) for i in checkable_indexes)
            if all_checked:
                self.uncheck_all()
            else:
                self.check_all()

    def on_refresh_sandbox_list(self, event):
        self._list_refresh_pending = False
        self._list_refresh_pending_scheduled = False
        self._list_refresh_in_progress = True
        self._list_refresh_last_started_at = time.monotonic()
        self._refresh_token += 1
        token = self._refresh_token
        try:
            if self.current_group_view:
                self._refresh_group_view()
                return

            host = self.GetTopLevelParent()
            sandboxes = self.scan_sandboxes()
            groups = []
            if hasattr(host, "get_group_names"):
                try:
                    groups = host.get_group_names()
                except Exception:
                    groups = []
            grouped_members = set()
            if hasattr(host, "get_grouped_members_set"):
                try:
                    grouped_members = {str(name or "").strip() for name in host.get_grouped_members_set()}
                except Exception:
                    grouped_members = set()
            pinned_names = set()
            if hasattr(host, "get_pinned_sandboxes"):
                try:
                    pinned_names = {str(name or "").strip() for name in host.get_pinned_sandboxes()}
                except Exception:
                    pinned_names = set()

            pinned = [name for name in sandboxes if name in pinned_names and name not in grouped_members]
            normal = [name for name in sandboxes if name not in pinned_names and name not in grouped_members]
            item_texts = []
            group_indexes = []
            for sandbox in pinned:
                item_texts.append(sandbox)
            for group_name in groups:
                group_indexes.append(len(item_texts))
                item_texts.append(self.format_group_item_text(group_name))
            for sandbox in normal:
                item_texts.append(sandbox)
            self._replace_item_texts_fast(item_texts, group_indexes=group_indexes)

            if self.GetItemCount() > 0:
                self.Focus(0)
            self.update_status()
            try:
                suppress_refresh_status = False
                try:
                    suppress_checker = getattr(host, "_should_suppress_list_refresh_status", None)
                    suppress_refresh_status = bool(suppress_checker()) if callable(suppress_checker) else False
                except Exception:
                    suppress_refresh_status = False
                if (not getattr(host, "_user_interacted_with_sandbox_list", False)) and (not suppress_refresh_status):
                    host.lbl_status.SetLabel(f"已刷新 {len(sandboxes)} 个沙盒用户")
            except Exception:
                pass
            self._refresh_running_states_async(token, "", sandboxes, getattr(host, "_get_running_sandbox_states", None))
        finally:
            self._list_refresh_in_progress = False

    def on_fast_refresh_sandbox_list(self, event=None):
        self.on_refresh_sandbox_list(event)

    def _finish_list_refresh(self, token, group_name):
        self._list_refresh_in_progress = False
        if self._list_refresh_pending:
            self._schedule_pending_list_refresh()

    def _schedule_pending_list_refresh(self):
        if self._list_refresh_pending_scheduled:
            return
        elapsed = time.monotonic() - float(getattr(self, "_list_refresh_last_started_at", 0.0) or 0.0)
        delay_seconds = max(0.12, 0.8 - elapsed)
        self._list_refresh_pending_scheduled = True

        def _run():
            self._list_refresh_pending_scheduled = False
            if not self._list_refresh_pending:
                return
            if self._list_refresh_in_progress:
                self._schedule_pending_list_refresh()
                return
            self._list_refresh_pending = False
            self.on_refresh_sandbox_list(None)

        self._safe_call_later(int(delay_seconds * 1000), _run)

    def _refresh_group_view(self):
        host = self.GetTopLevelParent()
        self._refresh_token += 1
        token = self._refresh_token
        group_name = self.current_group_view
        if self.current_group_view:
            valid_groups = []
            if hasattr(host, "get_group_names"):
                try:
                    valid_groups = host.get_group_names()
                except Exception:
                    valid_groups = []
            if self.current_group_view not in valid_groups:
                self.current_group_view = ""
                self.on_refresh_sandbox_list(None)
                return

        if not self.current_group_view:
            self.DeleteAllItems()
            self._clear_special_item_indexes()
            self._current_value = ""
            self.context_item_index = -1
            self._last_checked_index = None
            self.update_status()
            self._finish_list_refresh(token, "")
            return
        members = []
        if hasattr(host, "get_group_members"):
            try:
                members = host.get_group_members(self.current_group_view)
            except Exception:
                members = []
        pinned_names = set()
        if hasattr(host, "get_pinned_sandboxes"):
            try:
                pinned_names = {str(name) for name in host.get_pinned_sandboxes()}
            except Exception:
                pinned_names = set()

        try:
            host.lbl_status.SetLabel("正在扫描沙盒用户...")
        except Exception:
            pass
        self._refresh_group_view_worker(
            token,
            group_name,
            members,
            pinned_names,
            getattr(host, "_get_sandbox_display_name", None),
            getattr(host, "_get_running_sandbox_states", None)
        )

    def _refresh_group_view_worker(self, token, group_name, members, pinned_names, display_resolver, running_state_getter):
        try:
            members = self._normalize_display_name_list(members, display_resolver)
            pinned_names = set(self._normalize_display_name_list(pinned_names, display_resolver))
            if pinned_names:
                pinned_members = [name for name in members if name in pinned_names]
                others = [name for name in members if name not in pinned_names]
                members = pinned_members + others
            self._safe_call_after(self._apply_group_view_refresh, token, group_name, members, {})
            self._refresh_running_states_async(token, group_name, members, running_state_getter)
        except Exception:
            self._safe_call_after(self._finish_list_refresh, token, group_name)

    def _normalize_display_name_list(self, names, display_resolver):
        normalized = []
        seen = set()
        for name in names or []:
            try:
                value = display_resolver(name) if callable(display_resolver) else str(name or "").strip()
            except Exception:
                value = str(name or "").strip()
            if value and value not in seen:
                seen.add(value)
                normalized.append(value)
        return normalized

    def _apply_group_view_refresh(self, token, group_name, members, running_states):
        if token != self._refresh_token:
            self._finish_list_refresh(token, group_name)
            return
        if self.current_group_view != group_name:
            self._finish_list_refresh(token, group_name)
            return
        item_texts = []
        for sandbox in members:
            is_running = bool(running_states.get(sandbox, False))
            item_texts.append(self.format_sandbox_item_text(sandbox, is_running))
        self._replace_item_texts_fast(item_texts)
        self.update_status()
        try:
            host = self.GetTopLevelParent()
            suppress_refresh_status = False
            try:
                suppress_checker = getattr(host, "_should_suppress_list_refresh_status", None)
                suppress_refresh_status = bool(suppress_checker()) if callable(suppress_checker) else False
            except Exception:
                suppress_refresh_status = False
            if (not getattr(host, "_user_interacted_with_sandbox_list", False)) and (not suppress_refresh_status):
                host.lbl_status.SetLabel(f"已刷新 {len(members)} 个分组用户")
        except Exception:
            pass
        self._finish_list_refresh(token, group_name)

    def _refresh_root_view_async(self):
        host = self.GetTopLevelParent()
        groups = []
        if hasattr(host, "get_group_names"):
            try:
                groups = host.get_group_names()
            except Exception:
                groups = []
        grouped_members = set()
        if hasattr(host, "get_grouped_members_set"):
            try:
                grouped_members = host.get_grouped_members_set()
            except Exception:
                grouped_members = set()
        pinned_names = set()
        if hasattr(host, "get_pinned_sandboxes"):
            try:
                pinned_names = {str(name) for name in host.get_pinned_sandboxes()}
            except Exception:
                pinned_names = set()
        self._refresh_token += 1
        token = self._refresh_token
        try:
            host.lbl_status.SetLabel("正在扫描沙盒用户...")
        except Exception:
            pass
        self._refresh_root_view_worker(
            token,
            groups,
            grouped_members,
            pinned_names,
            getattr(host, "_get_sandbox_display_name", None),
            getattr(host, "_get_running_sandbox_states", None)
        )

    def _refresh_root_view_worker(self, token, groups, grouped_members, pinned_names, display_resolver, running_state_getter):
        try:
            try:
                sandboxes = self.scan_sandboxes()
            except Exception:
                sandboxes = []
            grouped_members = set(self._normalize_display_name_list(grouped_members, display_resolver))
            pinned_names = set(self._normalize_display_name_list(pinned_names, display_resolver))
            state_targets = [sandbox for sandbox in sandboxes if sandbox not in grouped_members]
            self._safe_call_after(self._apply_root_view_refresh, token, groups, grouped_members, pinned_names, sandboxes, {})
            self._refresh_running_states_async(token, "", state_targets, running_state_getter)
        except Exception:
            self._safe_call_after(self._finish_list_refresh, token, "")

    def _apply_root_view_refresh(self, token, groups, grouped_members, pinned_names, sandboxes, running_states):
        if token != self._refresh_token:
            self._finish_list_refresh(token, "")
            return
        if self.current_group_view:
            self._finish_list_refresh(token, "")
            return
        ungrouped = [sandbox for sandbox in sandboxes if sandbox not in grouped_members]
        pinned_ungrouped = []
        normal_ungrouped = []
        for sandbox in ungrouped:
            if sandbox in pinned_names:
                pinned_ungrouped.append(sandbox)
            else:
                normal_ungrouped.append(sandbox)
        item_texts = []
        group_indexes = []
        for sandbox in pinned_ungrouped:
            is_running = bool(running_states.get(sandbox, False))
            item_texts.append(self.format_sandbox_item_text(sandbox, is_running))
        for group_name in groups:
            group_indexes.append(len(item_texts))
            item_texts.append(self.format_group_item_text(group_name))
        for sandbox in normal_ungrouped:
            is_running = bool(running_states.get(sandbox, False))
            item_texts.append(self.format_sandbox_item_text(sandbox, is_running))
        self._replace_item_texts_fast(item_texts, group_indexes=group_indexes)
        self.update_status()
        try:
            host = self.GetTopLevelParent()
            suppress_refresh_status = False
            try:
                suppress_checker = getattr(host, "_should_suppress_list_refresh_status", None)
                suppress_refresh_status = bool(suppress_checker()) if callable(suppress_checker) else False
            except Exception:
                suppress_refresh_status = False
            if (not getattr(host, "_user_interacted_with_sandbox_list", False)) and (not suppress_refresh_status):
                host.lbl_status.SetLabel(f"已刷新 {len(sandboxes)} 个沙盒用户")
        except Exception:
            pass
        self._finish_list_refresh(token, "")

    def _refresh_running_states_async(self, token, group_name, sandbox_names, running_state_getter):
        if not self._is_ui_alive():
            return False
        if not callable(running_state_getter):
            return False
        targets = [str(name or "").strip() for name in (sandbox_names or []) if str(name or "").strip()]
        if not targets:
            return False
        if self._state_refresh_in_progress:
            self._state_refresh_pending_args = (token, group_name, targets, running_state_getter)
            return True
        self._state_refresh_in_progress = True
        threading.Thread(
            target=self._refresh_running_states_worker,
            args=(token, group_name, targets, running_state_getter),
            daemon=True
        ).start()
        return True

    def _refresh_running_states_worker(self, token, group_name, sandbox_names, running_state_getter):
        if not self._is_ui_alive():
            return
        try:
            try:
                running_states = running_state_getter(sandbox_names, timeout=3.0, max_workers=16)
            except TypeError:
                running_states = running_state_getter(sandbox_names)
        except Exception:
            running_states = {}
        if not self._is_ui_alive():
            return
        self._safe_call_after(self._finish_running_states_refresh, token, group_name, running_states)

    def _finish_running_states_refresh(self, token, group_name, running_states):
        try:
            if not self._is_ui_alive():
                return
            self._apply_running_states_refresh(token, group_name, running_states)
        except RuntimeError:
            return
        finally:
            self._state_refresh_in_progress = False
            pending_args = self._state_refresh_pending_args
            self._state_refresh_pending_args = None
            self._refresh_status_visible = False
            if not pending_args:
                try:
                    host = self.GetTopLevelParent()
                except Exception:
                    host = None
                if host is not None and getattr(host, "_manual_process_refresh_status_pending", False):
                    host._manual_process_refresh_status_pending = False
                    try:
                        current_status = str(host.lbl_status.GetLabel() or "").strip()
                    except Exception:
                        current_status = ""
                    if current_status == "正在刷新进程状态":
                        try:
                            host.lbl_status.SetLabel("已刷新进程状态")
                        except Exception:
                            pass
            if pending_args:
                self._safe_call_later(120, self._refresh_running_states_async, *pending_args)

    def _apply_running_states_refresh(self, token, group_name, running_states):
        if token != self._refresh_token:
            return
        if (group_name or "") != (self.current_group_view or ""):
            return
        if not isinstance(running_states, dict) or not running_states:
            return
        changed = 0
        for index in range(self.GetItemCount()):
            if self._is_special_index(index):
                continue
            item_text = self.GetItemText(index)
            sandbox_name = self.get_sandbox_name_from_item_text(item_text)
            if sandbox_name not in running_states:
                continue
            new_text = self.format_sandbox_item_text(sandbox_name, bool(running_states.get(sandbox_name)))
            if new_text != item_text:
                self.SetItem(index, 0, new_text)
                changed += 1
        if changed:
            try:
                host = self.GetTopLevelParent()
                if hasattr(host, "_invalidate_process_menu_state_cache"):
                    host._invalidate_process_menu_state_cache()
                if hasattr(host, "_schedule_process_menu_live_state_refresh"):
                    host._schedule_process_menu_live_state_refresh(delay_ms=1)
            except Exception:
                pass
            try:
                self.RefreshItems(0, self.GetItemCount() - 1)
            except Exception:
                self.Refresh()

    def refresh_visible_running_states(self):
        host = self.GetTopLevelParent()
        running_state_getter = getattr(host, "_get_running_sandbox_states", None)
        if not callable(running_state_getter):
            return False
        targets = []
        seen = set()
        for index in range(self.GetItemCount()):
            if self._is_special_index(index):
                continue
            item_text = self.GetItemText(index)
            sandbox_name = self.get_sandbox_name_from_item_text(item_text)
            if not sandbox_name:
                continue
            key = sandbox_name.lower()
            if key in seen:
                continue
            seen.add(key)
            targets.append(sandbox_name)
        if not targets:
            return False
        return self._refresh_running_states_async(
            self._refresh_token,
            self.current_group_view or "",
            targets,
            running_state_getter
        )

    def scan_sandboxes(self):
        host = None
        try:
            host = self.GetTopLevelParent()
        except Exception:
            host = None
        if host is not None:
            try:
                display_scanner = getattr(host, "_scan_sandbox_display_names", None)
            except Exception:
                display_scanner = None
            if callable(display_scanner):
                try:
                    names = [str(name or "").strip() for name in display_scanner() if str(name or "").strip()]
                    if names:
                        return sorted(names, key=lambda value: value.lower())
                except Exception:
                    pass
        sandboxes = []
        seen = set()
        display_resolver = None
        if host is not None:
            try:
                display_resolver = getattr(host, "_get_sandbox_display_name", None)
            except Exception:
                display_resolver = None
        if os.path.exists(self.SANDBOX_ROOT):
            try:
                for item in os.listdir(self.SANDBOX_ROOT):
                    full_path = os.path.join(self.SANDBOX_ROOT, item)
                    if os.path.isdir(full_path):
                        display_name = ""
                        try:
                            if callable(display_resolver):
                                display_name = str(display_resolver(item) or "").strip()
                        except Exception:
                            display_name = ""
                        if not display_name:
                            display_name = str(item or "").strip()
                        key = display_name.lower()
                        if not display_name or key in seen:
                            continue
                        seen.add(key)
                        sandboxes.append(display_name)
            except Exception:
                pass
        return sorted(sandboxes, key=lambda value: value.lower())

    def on_item_checked(self, event):
        index = event.GetIndex()
        if index >= 0:
            if self._is_special_index(index):
                try:
                    wx.ListCtrl.CheckItem(self, index, False)
                except Exception:
                    pass
                event.Skip(False)
                return
        if (
            self.range_auto_check_enabled
            and not self._batch_checking
            and self._last_checked_index is not None
            and self._last_checked_index != index
        ):
            start_idx, end_idx = sorted((self._last_checked_index, index))
            if end_idx - start_idx > 1:
                self._batch_checking = True
                try:
                    for i in range(start_idx + 1, end_idx):
                        if self._is_special_index(i):
                            continue
                        if not self.IsItemChecked(i):
                            wx.ListCtrl.CheckItem(self, i, True)
                finally:
                    self._batch_checking = False
        self._last_checked_index = index
        self.update_status()
        event.Skip()

    def on_item_unchecked(self, event):
        index = event.GetIndex()
        if self._last_checked_index == index:
            self._last_checked_index = None
        self.update_status()
        event.Skip()

    def announce_item_state(self, item_index):
        item_text = self.GetItemText(item_index)
        kind_text = "分组" if self._is_group_index(item_index) else "用户"
        pin_text = ""
        if kind_text == "用户":
            sandbox_name = self.get_sandbox_name_from_item_text(item_text)
            host = self.GetTopLevelParent()
            if hasattr(host, "is_sandbox_pinned"):
                try:
                    pin_text = "已置顶" if host.is_sandbox_pinned(sandbox_name) else "未置顶"
                except Exception:
                    pin_text = ""
        status_text = f"{'已选中' if self.IsItemChecked(item_index) else '未选中'} {item_text} {kind_text}"
        if pin_text:
            status_text = f"{status_text} {pin_text}"
        accessible = self.GetAccessible()
        if accessible:
            accessible.SetName(status_text)

    def toggle_item(self, index):
        if index < 0 or index >= self.GetItemCount():
            return
        if self._is_special_index(index):
            return
        self.CheckItem(index, not self.IsItemChecked(index))

    def CheckItem(self, index, check=True):
        if index < 0 or index >= self.GetItemCount():
            return
        try:
            item_text = self.GetItemText(index)
        except Exception:
            return
        if self._is_special_index(index):
            try:
                wx.ListCtrl.CheckItem(self, index, False)
            except Exception:
                pass
            return
        super().CheckItem(index, check)
        self.update_status()

    def update_status(self):
        if self.on_toggle_callback:
            self.on_toggle_callback(self.get_checked_count())

    def get_checked_items(self):
        checked = []
        for i in range(self.GetItemCount()):
            if self._is_special_index(i):
                continue
            if self.IsItemChecked(i):
                checked.append(i)
        return checked
    def get_checked_count(self):
        return len(self.get_checked_items())

    def check_all(self):
        self._batch_checking = True
        try:
            for i in range(self.GetItemCount()):
                if self._is_special_index(i):
                    continue
                if not self.IsItemChecked(i):
                    wx.ListCtrl.CheckItem(self, i, True)
        finally:
            self._batch_checking = False
        self._last_checked_index = None
        self.update_status()

    def uncheck_all(self):
        self._batch_checking = True
        try:
            for i in range(self.GetItemCount()):
                if self._is_special_index(i):
                    continue
                if self.IsItemChecked(i):
                    wx.ListCtrl.CheckItem(self, i, False)
        finally:
            self._batch_checking = False
        self._last_checked_index = None
        self.update_status()

    def append_item(self, text):
        index = self.GetItemCount()
        self.InsertItem(index, text)
        return index
