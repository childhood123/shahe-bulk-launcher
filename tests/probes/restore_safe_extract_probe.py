import importlib.util
from pathlib import Path


TARGET = Path(__file__).resolve().parents[2] / "沙河批量打开器.py"
spec = importlib.util.spec_from_file_location("shahe_restore_safe_extract_probe_target", TARGET)
mod = importlib.util.module_from_spec(spec)
spec.loader.exec_module(mod)
from shahe_bulk_launcher import backup as backup_mod
wx = mod.wx


captured = {
    "extractall_called": 0,
    "safe_extract_called": 0,
}


class FakeZipFile:
    def __init__(self, *args, **kwargs):
        pass
    def __enter__(self):
        return self
    def __exit__(self, exc_type, exc, tb):
        return False
    def extractall(self, *args, **kwargs):
        captured["extractall_called"] += 1


class FakeFileDialog:
    def __init__(self, *args, **kwargs):
        self._path = r"D:\fake\backup.sbx"
    def __enter__(self):
        return self
    def __exit__(self, exc_type, exc, tb):
        return False
    def ShowModal(self):
        return wx.ID_OK
    def GetPath(self):
        return self._path


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


app = wx.App(False)
frame = ProbeFrame()
mgr = mod.BackupRestoreMgr(frame)

original_zipfile = backup_mod.zipfile.ZipFile
original_safe_extract = getattr(backup_mod, "safe_extract_zip", None)
original_filedialog = backup_mod.wx.FileDialog
original_exists = backup_mod.Path.exists
original_mkdir = backup_mod.Path.mkdir
original_iter_roots = mgr._iter_backup_user_roots
original_messagebox = backup_mod.wx.MessageBox
original_rmtree = backup_mod.shutil.rmtree
original_force_remove = mgr._force_remove_dir


def fake_safe_extract(zip_path, extract_dir):
    captured["safe_extract_called"] += 1


def fake_exists(self):
    text = str(self)
    if text.endswith("backup.sbx"):
        return True
    if text.endswith("restore_temp"):
        return False
    return True


try:
    backup_mod.zipfile.ZipFile = FakeZipFile
    backup_mod.safe_extract_zip = fake_safe_extract
    backup_mod.wx.FileDialog = FakeFileDialog
    backup_mod.Path.exists = fake_exists
    backup_mod.Path.mkdir = lambda self, parents=False, exist_ok=False: None
    mgr._iter_backup_user_roots = lambda base_dir: iter([])
    backup_mod.wx.MessageBox = lambda *args, **kwargs: wx.OK
    backup_mod.shutil.rmtree = lambda *args, **kwargs: None
    mgr._force_remove_dir = lambda path: None

    mgr.restore()
finally:
    backup_mod.zipfile.ZipFile = original_zipfile
    if original_safe_extract is not None:
        backup_mod.safe_extract_zip = original_safe_extract
    backup_mod.wx.FileDialog = original_filedialog
    backup_mod.Path.exists = original_exists
    backup_mod.Path.mkdir = original_mkdir
    mgr._iter_backup_user_roots = original_iter_roots
    backup_mod.wx.MessageBox = original_messagebox
    backup_mod.shutil.rmtree = original_rmtree
    mgr._force_remove_dir = original_force_remove
    frame.force_exit_requested = True
    frame.Destroy()
    app.Destroy()


assert captured["extractall_called"] == 0, captured
assert captured["safe_extract_called"] == 1, captured
print("PASS restore_safe_extract_probe")
