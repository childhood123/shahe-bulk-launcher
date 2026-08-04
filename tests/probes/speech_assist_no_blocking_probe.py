import importlib.util
import threading
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
TARGET = ROOT / "沙河批量打开器.py"
spec = importlib.util.spec_from_file_location("shahe_speech_assist_probe_target", TARGET)
mod = importlib.util.module_from_spec(spec)
spec.loader.exec_module(mod)


class Probe:
    _destroy_started = False
    speech_assist_hints = {"enabled": True}

    def __init__(self):
        self._assist_speech_call_lock = threading.Lock()
        self._assist_speech_runtime = None
        self.completed = threading.Event()
        self.calls = []

    def _is_window_closing(self):
        return False

    def _ensure_assist_speech_runtime(self):
        self._assist_speech_runtime = {
            "silence": lambda: self.calls.append(("silence", threading.get_ident())),
            "output": self._output,
        }
        return True

    def _output(self, content, interrupt):
        self.calls.append(("output", threading.get_ident(), content, bool(interrupt)))
        self.completed.set()


probe = Probe()
main_thread_id = threading.get_ident()
mod.SandboxLauncher._speak_assist_text(probe, "测试朗读", interrupt=True)
assert probe.completed.wait(2.0), probe.calls
assert [call[0] for call in probe.calls] == ["silence", "output"], probe.calls
assert all(call[1] != main_thread_id for call in probe.calls), probe.calls
assert probe.calls[-1][2:] == ("测试朗读", True), probe.calls
print("PASS speech_assist_no_blocking_probe")
