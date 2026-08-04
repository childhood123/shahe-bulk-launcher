import importlib.util
from pathlib import Path


TARGET = Path(__file__).resolve().parents[2] / "沙河批量打开器.py"
spec = importlib.util.spec_from_file_location("shahe_restore_shell_probe_target", TARGET)
mod = importlib.util.module_from_spec(spec)
spec.loader.exec_module(mod)
from shahe_bulk_launcher import backup as backup_mod
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
mgr = mod.BackupRestoreMgr(frame)
captured = []
original_run = backup_mod.subprocess.run
original_popen = backup_mod.subprocess.Popen
original_message_box = wx.MessageBox


def fake_run(cmd, *args, **kwargs):
    captured.append((cmd, dict(kwargs)))
    class Result:
        returncode = 0
        stdout = ""
        stderr = ""
    return Result()


def fake_popen(cmd, *args, **kwargs):
    captured.append((cmd, dict(kwargs)))
    class Process:
        pid = 1234
    return Process()


try:
    backup_mod.subprocess.run = fake_run
    backup_mod.subprocess.Popen = fake_popen
    wx.MessageBox = lambda *args, **kwargs: wx.YES
    mgr._stop_sandboxie_services()
    mgr._restart_sandboxie_services()
    mgr._reload_sandboxie_config()
    mgr._ask_start_control(r"C:\Program Files\Sandboxie-Plus\SandMan.exe")
finally:
    backup_mod.subprocess.run = original_run
    backup_mod.subprocess.Popen = original_popen
    wx.MessageBox = original_message_box
    frame.force_exit_requested = True
    frame.Destroy()
    app.Destroy()


assert captured, "expected subprocess.run calls"
assert all(not item[1].get("shell", False) for item in captured), captured
assert any(isinstance(item[0], list) and item[0][:2] == ["net", "stop"] for item in captured), captured
assert any(isinstance(item[0], list) and item[0][:2] == ["net", "start"] for item in captured), captured
assert any(isinstance(item[0], list) and item[0][0] == "taskkill" for item in captured), captured
assert any(isinstance(item[0], list) and item[0][0].endswith("SandMan.exe") for item in captured), captured
print("PASS restore_shell_free_probe")
