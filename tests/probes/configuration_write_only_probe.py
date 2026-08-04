
import importlib.util
from pathlib import Path

TARGET = Path(__file__).resolve().parents[2] / "沙河批量打开器.py"
spec = importlib.util.spec_from_file_location("shahe_launcher_target", TARGET)
mod = importlib.util.module_from_spec(spec)
spec.loader.exec_module(mod)

class Probe:
    CONFIG_DIR = r"C:\not-writable\Configuration"
    def __init__(self):
        self.calls = []
    def _ensure_writable_dir(self, folder_path):
        self.calls.append(str(folder_path))
        return False

probe = Probe()
speech_dir = mod.SandboxLauncher._get_speech_runtime_dir(probe)
zdsr_path = mod.SandboxLauncher._get_reader_ini_path(probe, "ZDSRAPI.ini", for_write=True)
boy_path = mod.SandboxLauncher._get_reader_ini_path(probe, "boyctrl.ini", for_write=True)
print("speech_dir", speech_dir)
print("zdsr_path", zdsr_path)
print("boy_path", boy_path)
for value in (speech_dir, zdsr_path, boy_path):
    text = str(value).replace("/", "\\")
    if "\\internal\\" in text.lower() or text.lower().endswith("\\internal"):
        raise AssertionError(f"configuration write path fell back to internal: {value}")
    if "app_config.dat" in text.lower():
        raise AssertionError(f"configuration write path fell back to old config file: {value}")
    if "Configuration" not in str(value):
        raise AssertionError(f"configuration write path is not under Configuration: {value}")
print("PASS tmp_configuration_write_only_probe")
