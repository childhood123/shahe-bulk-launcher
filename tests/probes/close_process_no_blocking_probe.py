import importlib.util
import threading
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
TARGET = ROOT / "沙河批量打开器.py"
spec = importlib.util.spec_from_file_location("shahe_close_process_probe_target", TARGET)
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
    main_thread_id = threading.get_ident()
    close_thread_ids = []

    def fake_close_sandbox_processes(sandbox_name, allow_pid_fallback=True):
        close_thread_ids.append(threading.get_ident())
        return {
            "sandbox_name": str(sandbox_name or ""),
            "status": "all_closed",
            "closed": 1,
            "any_closed": True,
        }

    frame._close_sandbox_processes = fake_close_sandbox_processes
    frame._speak_process_assist = lambda *args, **kwargs: None
    frame._refresh_process_ui_after_action = lambda *args, **kwargs: None

    frame.on_close_current_sandbox("测试用户")
    wx.Yield()

    assert close_thread_ids, "close path was not reached"
    assert all(thread_id != main_thread_id for thread_id in close_thread_ids), close_thread_ids
    print("PASS close_process_no_blocking_probe")
finally:
    frame.force_exit_requested = True
    frame.Destroy()
    app.Destroy()
