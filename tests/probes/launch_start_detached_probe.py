import importlib.util
import os
import shutil
import tempfile
import time
from pathlib import Path

TARGET = Path(os.environ.get("SHAHE_LAUNCHER_TARGET", str(Path(__file__).resolve().parents[2] / "沙河批量打开器.py")))
spec = importlib.util.spec_from_file_location("shahe_launch_detached_probe_target", TARGET)
mod = importlib.util.module_from_spec(spec)
spec.loader.exec_module(mod)
from shahe_bulk_launcher.features.sandboxie import SandboxieMixin

root = Path(tempfile.mkdtemp(prefix="shahe_launch_detached_probe_"))
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

dummy._normalize_sandbox_name = lambda name: str(name or "").strip()
dummy._get_sandbox_display_name = lambda name: str(name or "").strip()
dummy._get_official_sandbox_name_set = lambda: {"boxa"}
dummy._get_sandboxie_start_exe = lambda: str(start_exe)
dummy._is_invalid_sandbox_name_output = lambda output: False
dummy._reload_sandboxie_config = lambda: None
dummy._refresh_sandbox_display_maps = lambda: None
dummy._safe_call_after = lambda callback, *args, **kwargs: callback(*args, **kwargs)

run_calls = []
popen_calls = []


class SlowCompletedProcess:
    returncode = 0
    stdout = ""
    stderr = ""


class FakePopen:
    pid = 4242


def slow_run(*args, **kwargs):
    run_calls.append((args, kwargs))
    time.sleep(0.35)
    return SlowCompletedProcess()


def fast_popen(*args, **kwargs):
    popen_calls.append((args, kwargs))
    return FakePopen()


original_run = mod.subprocess.run
original_popen = mod.subprocess.Popen
try:
    mod.subprocess.run = slow_run
    mod.subprocess.Popen = fast_popen
    started_at = time.perf_counter()
    launched = dummy.start_app_in_sandbox("BoxA", str(app_exe), str(app_dir))
    elapsed = time.perf_counter() - started_at
finally:
    mod.subprocess.run = original_run
    mod.subprocess.Popen = original_popen
    shutil.rmtree(root, ignore_errors=True)

print("launched", launched, "elapsed", round(elapsed, 3), "run_calls", len(run_calls), "popen_calls", len(popen_calls), flush=True)
if not launched:
    raise AssertionError("sandbox launch should report success once Start.exe is dispatched")
if run_calls:
    raise AssertionError("sandbox application launch used blocking subprocess.run")
if not popen_calls:
    raise AssertionError("sandbox application launch did not dispatch with subprocess.Popen")
if elapsed >= 0.20:
    raise AssertionError(f"sandbox application launch waited for Start.exe completion: {elapsed:.3f}s")
print("PASS tmp_launch_start_detached_probe", flush=True)
