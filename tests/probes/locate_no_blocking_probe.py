import importlib.util
import time
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
TARGET = ROOT / "沙河批量打开器.py"
spec = importlib.util.spec_from_file_location("shahe_locate_probe_target", TARGET)
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
    frame._cancel_locate_window_reinforcement = lambda wait=False: None
    frame._get_sandbox_internal_and_display_name = lambda name: (name, name)
    frame._get_target_pids_for_sandbox = lambda name: [123]
    frame._find_windows_for_pids = lambda pids: [
        {"hwnd": 1, "pid": 123, "title": "x", "minimized": True}
    ]
    frame._prompt_window_choice = lambda display_name, windows: 1
    frame._stabilize_activated_window = lambda hwnd: True
    frame._start_locate_window_reinforcement = lambda hwnd, keep_long_tail=False: None
    frame._has_foreground_window_streak = lambda *args, **kwargs: True
    frame._activate_window = lambda hwnd: True
    frame._speak_process_assist = lambda *args, **kwargs: None

    start = time.perf_counter()
    frame.on_locate_current_sandbox("测试用户")
    elapsed = time.perf_counter() - start

    assert elapsed < 0.5, elapsed
finally:
    frame.force_exit_requested = True
    frame.Destroy()
    app.Destroy()

print("PASS locate_no_blocking_probe")
