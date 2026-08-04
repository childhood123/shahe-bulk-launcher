import importlib.util
from pathlib import Path


TARGET = Path(__file__).resolve().parents[2] / "沙河批量打开器.py"
spec = importlib.util.spec_from_file_location("shahe_group_navigation_probe_target", TARGET)
mod = importlib.util.module_from_spec(spec)
spec.loader.exec_module(mod)
wx = mod.wx


class ProbeFrame(mod.SandboxLauncher):
    def first_time_scan_sandboxie(self):
        pass

    def _refresh_sandbox_list_on_startup(self):
        pass

    def _auto_check_update_after_startup(self):
        pass

    def _apply_auto_run(self):
        pass

    def create_task_bar_icon(self):
        pass

    def _load_speech_config(self):
        pass

    def _scan_sandbox_display_names(self):
        return ["用户A", "用户B", "用户C", "未分组D"]

    def _get_running_sandbox_states(self, sandbox_names, *args, **kwargs):
        return {str(name): False for name in sandbox_names or []}

    def save_settings(self):
        self._saved_settings_count = getattr(self, "_saved_settings_count", 0) + 1


def find_item(list_ctrl, text_part):
    for index in range(list_ctrl.GetItemCount()):
        if text_part in list_ctrl.GetItemText(index):
            return index
    return -1


class KeyProbeEvent:
    def __init__(self, key_code):
        self._key_code = key_code
        self.skipped = None

    def GetKeyCode(self):
        return self._key_code

    def ControlDown(self):
        return False

    def Skip(self, value=True):
        self.skipped = value


app = wx.App(False)
frame = ProbeFrame()
try:
    frame.sandbox_groups = {"组一": ["用户A", "用户B"]}
    frame.pinned_sandboxes = []
    frame.sandbox_list.current_group_view = ""
    frame.sandbox_list.on_refresh_sandbox_list(None)
    wx.Yield()

    group_index = find_item(frame.sandbox_list, "[分组] 组一")
    assert group_index >= 0, "group item not shown"
    assert frame.sandbox_list._is_group_index(group_index)
    frame.sandbox_list.CheckItem(group_index, True)
    assert not frame.sandbox_list.IsItemChecked(group_index)
    frame.sandbox_list.check_all()
    assert not frame.sandbox_list.IsItemChecked(group_index)
    assert "[分组] 组一" not in frame.sandbox_list.get_checked_entry_texts()

    frame.sandbox_list.Select(group_index)
    frame.sandbox_list.Focus(group_index)
    frame.sandbox_list.on_key_down(KeyProbeEvent(wx.WXK_RETURN))
    wx.Yield()
    assert frame.sandbox_list.current_group_view == "组一"
    assert frame.sandbox_list.GetItemCount() == 2

    frame.sandbox_list.on_key_down(KeyProbeEvent(wx.WXK_BACK))
    wx.Yield()
    assert frame.sandbox_list.current_group_view == ""
    assert find_item(frame.sandbox_list, "[分组] 组一") >= 0
    print("PASS group_navigation_probe")
finally:
    frame.force_exit_requested = True
    frame.Destroy()
    app.Destroy()
