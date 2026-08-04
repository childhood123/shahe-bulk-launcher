import importlib.util
import os
import shutil
import tempfile
from pathlib import Path

TARGET = Path(os.environ.get("SHAHE_LAUNCHER_TARGET", str(Path(__file__).resolve().parents[2] / "沙河批量打开器.py")))
spec = importlib.util.spec_from_file_location("shahe_launch_detached_immediate_failure_probe_target", TARGET)
mod = importlib.util.module_from_spec(spec)
spec.loader.exec_module(mod)
from shahe_bulk_launcher.features.sandboxie import SandboxieMixin

root = Path(tempfile.mkdtemp(prefix="shahe_launch_detached_failure_probe_"))
start_exe = root / "Start.exe"
app_dir = root / "app"
app_exe = app_dir / "target.exe"
start_exe.write_bytes(b"fake start")
app_dir.mkdir()
app_exe.write_bytes(b"fake app")


class Dummy:
    pass


dummy = Dummy()
for name, attr in SandboxieMixin.__dict__.items():
    if callable(attr):
        setattr(dummy, name, attr.__get__(dummy, Dummy))

status_messages = []
refresh_calls = []

dummy._normalize_sandbox_name = lambda name: str(name or "").strip()
dummy._get_sandbox_display_name = lambda name: str(name or "").strip()
dummy._get_official_sandbox_name_set = lambda: set()
dummy._get_sandboxie_start_exe = lambda: str(start_exe)
dummy._reload_sandboxie_config = lambda: refresh_calls.append("reload")
dummy._refresh_sandbox_display_maps = lambda: refresh_calls.append("maps")
dummy._safe_call_after = lambda callback, *args, **kwargs: callback(*args, **kwargs)


class Label:
    def SetLabel(self, value):
        status_messages.append(str(value))


dummy.lbl_status = Label()

popen_calls = []


class ImmediateFailedPopen:
    pid = 4343

    def __init__(self, *args, **kwargs):
        popen_calls.append((args, kwargs))
        output = kwargs.get("stdout")
        if output:
            output.write("Sandboxie 启动\r\n无效的沙盒名参数: StaleBox\r\n")
            output.flush()

    def poll(self):
        return 1


original_popen = mod.subprocess.Popen
try:
    mod.subprocess.Popen = ImmediateFailedPopen
    launched = dummy.start_app_in_sandbox("StaleBox", str(app_exe), str(app_dir))
finally:
    mod.subprocess.Popen = original_popen
    shutil.rmtree(root, ignore_errors=True)

print("launched", launched, "popen_calls", len(popen_calls), "status_messages", status_messages, "refresh_calls", refresh_calls, flush=True)
if launched:
    raise AssertionError("immediately failed Start.exe spawn was reported as successful launch")
if not popen_calls:
    raise AssertionError("Start.exe was not dispatched")
if not any("无效" in message or "Sandboxie 未启用" in message for message in status_messages):
    raise AssertionError("invalid sandbox failure was not surfaced")
print("PASS tmp_launch_detached_immediate_failure_probe", flush=True)
