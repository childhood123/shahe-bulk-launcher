
import importlib.util
from pathlib import Path

TARGET = Path(__file__).resolve().parents[2] / "沙河批量打开器.py"
spec = importlib.util.spec_from_file_location("shahe_launcher_target", TARGET)
mod = importlib.util.module_from_spec(spec)
spec.loader.exec_module(mod)

class DefaultProbe:
    def __init__(self):
        self.settings = mod.SandboxLauncher._get_default_settings(self)

settings = DefaultProbe().settings
print("default_list_use_left_right_nav", settings.get("list_use_left_right_nav"))
print("default_speech_assist_enabled", settings.get("speech_assist_hints", {}).get("enabled"))
if settings.get("list_use_left_right_nav") is not True:
    raise AssertionError("list_use_left_right_nav default should be True")
if settings.get("speech_assist_hints", {}).get("enabled") is not True:
    raise AssertionError("speech assist hints default should be True")

class ScanProbe:
    def __init__(self):
        self.messages = []
        self.statuses = []
    def _safe_set_status_label(self, text):
        self.statuses.append(str(text or ""))
        return True
    def _is_window_closing(self):
        return False
    def find_sandboxie_path(self):
        return ""

old_message_box = mod.wx.MessageBox
try:
    probe = ScanProbe()
    mod.wx.MessageBox = lambda message, caption="", style=0, parent=None: probe.messages.append((caption, message)) or mod.wx.OK
    mod.SandboxLauncher.first_time_scan_sandboxie(probe)
finally:
    mod.wx.MessageBox = old_message_box
print("scan_statuses", probe.statuses)
print("scan_messages", probe.messages)
if not probe.messages:
    raise AssertionError("first scan failure should show MessageBox")
if "未找到 Sandboxie 安装路径" not in probe.messages[-1][1]:
    raise AssertionError("first scan failure MessageBox text is wrong")

print("PASS tmp_defaults_and_first_scan_probe")
