import importlib.util
import json
import tempfile
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
TARGET = ROOT / "沙河批量打开器.py"
spec = importlib.util.spec_from_file_location("shahe_update_helper_probe_target", TARGET)
mod = importlib.util.module_from_spec(spec)
spec.loader.exec_module(mod)

import shahe_bulk_launcher.features.updates as updates_module

originals = {
    "is_packaged_app": updates_module.is_packaged_app,
    "get_app_dir": updates_module.get_app_dir,
    "get_current_executable_path": updates_module.get_current_executable_path,
    "get_update_temp_dir": updates_module.get_update_temp_dir,
    "getpid": updates_module.os.getpid,
    "time": updates_module.time.time,
}

try:
    with tempfile.TemporaryDirectory(prefix="shahe_update_helper_probe_") as temp_dir:
        app_dir = Path(temp_dir) / "app"
        app_dir.mkdir()
        main_exe = app_dir / "沙河批量打开器.exe"
        updater_exe = app_dir / "updater.exe"
        internal_dir = app_dir / "internal"
        internal_dir.mkdir()
        main_exe.write_bytes(b"fake executable")
        updater_exe.write_bytes(b"fake updater")
        (internal_dir / "runtime.dat").write_bytes(b"runtime")

        updates_module.is_packaged_app = lambda: True
        updates_module.get_app_dir = lambda: str(app_dir)
        updates_module.get_current_executable_path = lambda: str(main_exe)
        updates_module.get_update_temp_dir = lambda base_dir=None: str(
            Path(base_dir or app_dir) / "tenp"
        )
        updates_module.os.getpid = lambda: 12345
        updates_module.time.time = lambda: 1000

        frame = mod.SandboxLauncher.__new__(mod.SandboxLauncher)
        package_info = {
            "url": "https://example.invalid/update.zip",
            "kind": "patch",
            "sha256": "abc",
            "signature": "sig",
            "encrypted": False,
            "size": 1,
        }
        cmd, error = frame._prepare_updater_command(package_info, "9999.0.0")

        assert not error, error
        assert cmd, "update command should be created"
        helper_dir = app_dir / "tenp" / "updater_helper_12345_1000"
        helper = helper_dir / "updater.exe"
        assert Path(cmd[0]) == helper, cmd
        assert helper.read_bytes() == updater_exe.read_bytes()
        assert (helper_dir / "internal" / "runtime.dat").read_bytes() == b"runtime"
        assert "--exe" in cmd and cmd[cmd.index("--exe") + 1] == str(main_exe), cmd

        request_path = Path(cmd[cmd.index("--request-file") + 1])
        assert request_path.parent == app_dir / "tenp", request_path
        request = json.loads(request_path.read_text(encoding="utf-8"))
        assert request["current_version"] == mod.APP_VERSION, request
        assert request["target_version"] == "9999.0.0", request
        assert request["download_workers"] == "4", request
finally:
    updates_module.is_packaged_app = originals["is_packaged_app"]
    updates_module.get_app_dir = originals["get_app_dir"]
    updates_module.get_current_executable_path = originals["get_current_executable_path"]
    updates_module.get_update_temp_dir = originals["get_update_temp_dir"]
    updates_module.os.getpid = originals["getpid"]
    updates_module.time.time = originals["time"]

print("PASS update_helper_command_probe")
