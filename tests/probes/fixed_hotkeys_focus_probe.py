import importlib.util
from pathlib import Path


TARGET = Path(__file__).resolve().parents[2] / "沙河批量打开器.py"
spec = importlib.util.spec_from_file_location("shahe_fixed_hotkeys_probe_target", TARGET)
mod = importlib.util.module_from_spec(spec)
spec.loader.exec_module(mod)
wx = mod.wx


class Event:
    def __init__(self, key, ctrl=False, alt=False, shift=False):
        self.key = key
        self.ctrl = ctrl
        self.alt = alt
        self.shift = shift
        self.skipped = None

    def GetKeyCode(self):
        return self.key

    def ControlDown(self):
        return self.ctrl

    def AltDown(self):
        return self.alt

    def ShiftDown(self):
        return self.shift

    def Skip(self, value=True):
        self.skipped = value


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

    def on_browse_app(self, event):
        self.open_called = True

    def on_paste_file(self, event):
        self.paste_called = True

    def on_f5_refresh(self, event):
        self.refresh_called = True

    def on_ctrl_a_select(self, event):
        self.select_all_called = True


app = wx.App(False)
frame = ProbeFrame()
try:
    frame.hotkeys_enabled = False
    frame._apply_hotkeys()
    frame.btn_launch.SetFocus()
    for event in (
        Event(ord("O"), ctrl=True),
        Event(ord("V"), ctrl=True),
        Event(wx.WXK_F5),
        Event(ord("A"), ctrl=True),
    ):
        frame.on_frame_char_hook(event)

    assert getattr(frame, "open_called", False)
    assert getattr(frame, "paste_called", False)
    assert getattr(frame, "refresh_called", False)
    assert getattr(frame, "select_all_called", False)
    print("PASS fixed_hotkeys_focus_probe")
finally:
    frame.force_exit_requested = True
    frame.Destroy()
    app.Destroy()
