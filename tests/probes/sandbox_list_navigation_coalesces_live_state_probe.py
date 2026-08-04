import importlib.util
from pathlib import Path


TARGET = Path(__file__).resolve().parents[2] / "沙河批量打开器.py"
spec = importlib.util.spec_from_file_location("shahe_list_navigation_live_probe_target", TARGET)
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


class FakeListEvent:
    def __init__(self, index):
        self._index = index

    def GetIndex(self):
        return self._index

    def Skip(self, value=True):
        pass


app = wx.App(False)
frame = ProbeFrame()
try:
    frame.sandbox_list.DeleteAllItems()
    for index in range(30):
        frame.sandbox_list.InsertItem(
            frame.sandbox_list.GetItemCount(),
            frame.sandbox_list.format_sandbox_item_text(f"用户{index}", True),
        )

    frame.sandbox_list.announce_item_state = lambda index: None
    live_refresh_schedules = []
    menu_rebuild_schedules = []
    frame._schedule_process_menu_live_state_refresh = lambda delay_ms=1: live_refresh_schedules.append(delay_ms) or True
    frame._schedule_menu_bar_rebuild = lambda delay_ms=40: menu_rebuild_schedules.append(delay_ms) or True

    for index in range(20):
        frame.sandbox_list.on_item_selected(FakeListEvent(index))
        frame.sandbox_list.on_item_deselected(FakeListEvent(index))

    assert len(live_refresh_schedules) <= 1, live_refresh_schedules
    assert len(menu_rebuild_schedules) <= 1, menu_rebuild_schedules
    print("PASS sandbox_list_navigation_coalesces_live_state_probe")
finally:
    frame.force_exit_requested = True
    frame.Destroy()
    app.Destroy()
