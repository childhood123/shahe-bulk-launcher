import importlib.util
from pathlib import Path

TARGET = Path(__file__).resolve().parents[2] / "沙河批量打开器.py"
spec = importlib.util.spec_from_file_location("shahe_dialog_names_more_probe_target", TARGET)
mod = importlib.util.module_from_spec(spec)
spec.loader.exec_module(mod)
wx = mod.wx

class ProbeFrame(mod.SandboxLauncher):
    def first_time_scan_sandboxie(self): pass
    def _refresh_sandbox_list_on_startup(self): pass
    def _auto_check_update_after_startup(self): pass
    def _apply_auto_run(self): pass
    def create_task_bar_icon(self): pass
    def _load_speech_config(self): pass

app = wx.App(False)
frame = ProbeFrame()
try:
    hotkey = mod.HotkeySettingsDialog(
        frame,
        frame.hotkey_action_specs,
        frame.hotkeys,
        frame.default_hotkeys,
        frame._parse_hotkey_text,
        frame._format_hotkey_text,
        hotkeys_enabled=True,
        reserved_hotkeys=frame._get_reserved_hotkey_specs(),
    )
    assert hotkey.list_actions.GetName() == '快捷键功能列表'
    assert hotkey.txt_hotkey.GetName() == '当前功能快捷键'
    hotkey.Destroy()

    speech = mod.SpeechSettingsDialog(frame, {}, lambda data: None)
    assert speech.choice_interface_dll.GetName() == '读屏 API 接口'
    speech.Destroy()
finally:
    frame.force_exit_requested = True
    frame.Destroy()
    app.Destroy()
print('PASS dialog_control_names_more_probe')
