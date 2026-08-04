import os
import re
import time
from urllib.parse import unquote

import wx

from ..common import end_dialog_safely, parse_bool_setting, parse_int_setting
from ..ui.settings_dialogs import PathMemoryManagerDialog


class AppPathsMixin:
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
