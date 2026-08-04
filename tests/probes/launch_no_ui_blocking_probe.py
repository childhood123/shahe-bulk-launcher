import importlib.util
import time
from pathlib import Path


TARGET = Path(__file__).resolve().parents[2] / "沙河批量打开器.py"
spec = importlib.util.spec_from_file_location("shahe_launch_no_blocking_probe_target", TARGET)
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


app = wx.App(False)
frame = ProbeFrame()
try:
    frame.sandbox_list.get_checked_entry_texts = lambda: ["测试用户"]
    frame._prepare_launch_target = lambda: True
    frame.APP_PATHS = [(r"C:\Windows\System32\notepad.exe", r"C:\Windows\System32")]
    frame.add_to_history = lambda *args, **kwargs: False
    frame._launch_thread = lambda *args, **kwargs: None

    def slow_resolve(entries):
        time.sleep(0.65)
        return ["测试用户"]

    frame._resolve_launch_entries = slow_resolve
    start = time.perf_counter()
    frame.on_launch(None)
    elapsed = time.perf_counter() - start
    assert elapsed < 0.25, f"on_launch blocked UI for {elapsed:.3f}s"

    refresh_calls = []

    def slow_refresh_running_states():
        refresh_calls.append(time.perf_counter())
        time.sleep(0.65)
        return True

    frame._refresh_visible_sandbox_running_states = slow_refresh_running_states
    start = time.perf_counter()
    frame._launch_done(1, 1, 1, 0, 0.1, 0, False)
    elapsed = time.perf_counter() - start
    assert elapsed < 0.25, f"_launch_done blocked UI for {elapsed:.3f}s"
    assert not refresh_calls, "launch_done should not refresh running states synchronously"
    print("PASS launch_no_ui_blocking_probe")
finally:
    frame.force_exit_requested = True
    frame.Destroy()
    app.Destroy()
