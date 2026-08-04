import importlib.util
import time
from pathlib import Path

SRC = Path(__file__).resolve().parents[2] / "沙河批量打开器.py"
spec = importlib.util.spec_from_file_location('shahe_launcher_under_test', SRC)
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
    original_app_path = frame.APP_PATH
    for attempt in range(10):
        text = wx.TextDataObject("不是路径，也不是可执行文件")
        if wx.TheClipboard.Open():
            try:
                wx.TheClipboard.Clear()
                if wx.TheClipboard.SetData(text):
                    wx.TheClipboard.Flush()
                    break
            finally:
                wx.TheClipboard.Close()
        time.sleep(0.2)
    else:
        raise RuntimeError("clipboard set text failed")

    frame.on_menu_paste_app(None)
    app.Yield()
    print("STATUS", frame.lbl_status.GetLabel(), flush=True)
    print("APP_PATH", frame.APP_PATH, flush=True)
    assert frame.lbl_status.GetLabel() == "剪贴板没有包含有效的数据"
    assert frame.APP_PATH == original_app_path
finally:
    try:
        frame.force_exit_requested = True
        frame.Destroy()
    except Exception:
        pass
    app.Destroy()
print("PASS paste_invalid_clipboard_probe")
