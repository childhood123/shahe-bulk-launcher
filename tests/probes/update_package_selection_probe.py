import importlib.util
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
TARGET = ROOT / "沙河批量打开器.py"
spec = importlib.util.spec_from_file_location("shahe_update_package_probe_target", TARGET)
mod = importlib.util.module_from_spec(spec)
spec.loader.exec_module(mod)

frame = mod.SandboxLauncher.__new__(mod.SandboxLauncher)
manifest = {
    "latest_version": "9999.0.0",
    "patches": [
        {
            "from": mod.APP_VERSION,
            "to_version": "9999.0.0",
            "url": "https://example.invalid/update.zip",
            "size": "bad",
            "encrypted": "false",
        }
    ],
}
package = frame._select_update_package(manifest, manifest["latest_version"])
assert package is not None, "matching incremental package was not selected"
assert package["kind"] == "patch", package
assert package["size"] == 0, package
assert package["encrypted"] is False, package

legacy_manifest = {
    "latest_version": "9999.0.0",
    "patches": [
        {
            "from": mod.APP_VERSION,
            "to_version": "9999.0.0",
            "download_url": "https://example.invalid/legacy.zip",
        }
    ],
}
assert frame._select_update_package(legacy_manifest, "9999.0.0") is None
print("PASS update_package_selection_probe")
