import importlib.util
import threading
from pathlib import Path


TARGET = Path(__file__).resolve().parents[2] / "沙河批量打开器.py"
spec = importlib.util.spec_from_file_location("shahe_launch_done_probe_target", TARGET)
mod = importlib.util.module_from_spec(spec)
spec.loader.exec_module(mod)


class FakeButton:
    def __init__(self):
        self.enabled = False
        self.label = ""

    def Enable(self):
        self.enabled = True

    def SetLabel(self, value):
        self.label = str(value or "")


class Dummy:
    pass


dummy = Dummy()
dummy.btn_launch = FakeButton()
dummy._launch_session_id = 7
dummy._launch_cancel_event = threading.Event()
dummy._window_closing = False
dummy.status_labels = []
dummy.speech = []
dummy.later_calls = []
dummy.visible_refresh_count = 0
dummy.sync_process_refresh_count = 0
dummy.is_launching = True
dummy._active_launch_targets = ["BoxA"]
dummy._is_window_closing = lambda: False
dummy._safe_set_status_label = lambda text: dummy.status_labels.append(str(text or "")) or True
dummy._speak_process_assist = lambda text, event_key=None: dummy.speech.append(
    (text, event_key)
)
dummy._safe_call_later = lambda delay_ms, callback, *args, **kwargs: dummy.later_calls.append(
    (delay_ms, callback, args, kwargs)
)
dummy._refresh_visible_sandbox_running_states = lambda: setattr(
    dummy,
    "visible_refresh_count",
    dummy.visible_refresh_count + 1,
) or True


def sync_process_refresh(refresh_list=False):
    dummy.sync_process_refresh_count += 1
    raise AssertionError("_launch_done must not synchronously rebuild process UI/menu")


dummy._refresh_process_ui_after_action = sync_process_refresh
dummy._launch_done = mod.SandboxLauncher._launch_done.__get__(dummy, Dummy)
dummy._launch_done(7, True, 3, 0, 0.1, failed=0, cancelled=False)

assert dummy.btn_launch.enabled
assert dummy.btn_launch.label == "启动选中沙盒", dummy.btn_launch.label
assert dummy.sync_process_refresh_count == 0
assert dummy.status_labels and "启动完成" in dummy.status_labels[-1], dummy.status_labels
print("PASS launch_done_no_sync_menu_probe")
