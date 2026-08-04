import importlib.util
import time
from pathlib import Path


TARGET = Path(__file__).resolve().parents[2] / "沙河批量打开器.py"
spec = importlib.util.spec_from_file_location("shahe_list_navigation_probe_target", TARGET)
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
    for index in range(80):
        frame.sandbox_list.append_item(f"用户{index}")

    status_updates = []

    def slow_toggle_callback(selected_count):
        status_updates.append(selected_count)
        time.sleep(0.65)

    frame.sandbox_list.on_toggle_callback = slow_toggle_callback
    frame.sandbox_list.announce_item_state = lambda index: None
    frame._invalidate_process_menu_state_cache = lambda: None
    frame._invalidate_process_menu_live_state_cache = lambda clear_values=False: None
    frame._schedule_process_menu_live_state_refresh = lambda delay_ms=1: True
    frame._schedule_menu_bar_rebuild = lambda delay_ms=40: True

    start = time.perf_counter()
    frame.sandbox_list.on_item_selected(FakeListEvent(5))
    elapsed = time.perf_counter() - start

    assert elapsed < 0.25, f"list selection blocked UI for {elapsed:.3f}s"
    assert status_updates == [], f"selection should not update checked-count status: {status_updates}"

    frame.sandbox_list.on_item_deselected(FakeListEvent(5))
    assert status_updates == [], f"deselection should not update checked-count status: {status_updates}"
    print("PASS sandbox_list_navigation_no_status_probe")
finally:
    frame.force_exit_requested = True
    frame.Destroy()
    app.Destroy()
