import importlib.util
import threading
from pathlib import Path


TARGET = Path(__file__).resolve().parents[2] / "沙河批量打开器.py"
spec = importlib.util.spec_from_file_location("shahe_restore_no_main_thread_probe_target", TARGET)
mod = importlib.util.module_from_spec(spec)
spec.loader.exec_module(mod)
from shahe_bulk_launcher import backup as backup_mod
wx = mod.wx


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


class FakeSelectUsersDialog:
    def __init__(self, *args, **kwargs):
        pass

    def ShowModal(self):
        return wx.ID_OK

    def get_selected_users(self):
        return ["用户A"]

    def Destroy(self):
        pass


class FakeProgressDialog:
    def __init__(self, *args, **kwargs):
        self.updates = []

    def Update(self, *args, **kwargs):
        self.updates.append((args, kwargs))
        return True

    def Destroy(self):
        pass


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


class FakeUserRoot:
    def __truediv__(self, name):
        return self

    def exists(self):
        return False

    def mkdir(self, *args, **kwargs):
        pass


class FakeBackupRoot:
    def __truediv__(self, name):
        return self

    def iterdir(self):
        class Entry:
            name = "用户A"

            def is_dir(self):
                return True

        return [Entry()]


app = wx.App(False)
frame = ProbeFrame()
mgr = mod.BackupRestoreMgr(frame)

real_thread = threading.Thread
main_thread_id = threading.get_ident()
stop_thread_ids = []
started_threads = []
message_titles = []
original_file_dialog = backup_mod.wx.FileDialog
original_select_dialog = backup_mod.SelectUsersDialog
original_progress_dialog = backup_mod.wx.ProgressDialog
original_message_box = backup_mod.wx.MessageBox
original_safe_extract = getattr(backup_mod, "safe_extract_zip", None)
original_exists = backup_mod.Path.exists
original_mkdir = backup_mod.Path.mkdir
original_rmtree = backup_mod.shutil.rmtree
original_copytree = backup_mod.shutil.copytree
original_thread = backup_mod.threading.Thread
original_iter_roots = mgr._iter_backup_user_roots
original_find_control = mgr._find_sandboxie_control_exe
original_reload = mgr._reload_sandboxie_config
original_sandbox_users_root = mgr.sandbox_users_root
original_safe_call_after = mgr._safe_frame_call_after


class ImmediateThread:
    def __init__(self, target, daemon=False):
        self.target = target
        self.daemon = daemon

    def start(self):
        started_threads.append(self)
        worker = real_thread(target=self.target)
        worker.start()
        worker.join()


def fake_stop_services():
    stop_thread_ids.append(threading.get_ident())


def fake_exists(self):
    text = str(self)
    if text.endswith("backup.sbx"):
        return True
    if text.endswith("restore_temp"):
        return False
    if text.endswith("Sandboxie.ini"):
        return False
    return True


try:
    backup_mod.wx.FileDialog = FakeFileDialog
    backup_mod.SelectUsersDialog = FakeSelectUsersDialog
    backup_mod.wx.ProgressDialog = FakeProgressDialog
    def fake_message_box(*args, **kwargs):
        title = str(args[1] if len(args) > 1 else "")
        message_titles.append(title)
        return wx.YES if title == "确认还原" else wx.NO

    backup_mod.wx.MessageBox = fake_message_box
    backup_mod.safe_extract_zip = lambda *args, **kwargs: None
    backup_mod.Path.exists = fake_exists
    backup_mod.Path.mkdir = lambda self, parents=False, exist_ok=False: None
    backup_mod.shutil.rmtree = lambda *args, **kwargs: None
    backup_mod.shutil.copytree = lambda *args, **kwargs: None
    backup_mod.threading.Thread = ImmediateThread
    mgr._iter_backup_user_roots = lambda base_dir: iter([FakeBackupRoot()])
    mgr._stop_sandboxie_services = fake_stop_services
    mgr._find_sandboxie_control_exe = lambda: None
    mgr._reload_sandboxie_config = lambda: None
    mgr.sandbox_users_root = FakeUserRoot()
    mgr._safe_frame_call_after = lambda callback, *args, **kwargs: None

    mgr.restore()
finally:
    backup_mod.wx.FileDialog = original_file_dialog
    backup_mod.SelectUsersDialog = original_select_dialog
    backup_mod.wx.ProgressDialog = original_progress_dialog
    backup_mod.wx.MessageBox = original_message_box
    if original_safe_extract is None:
        try:
            delattr(backup_mod, "safe_extract_zip")
        except AttributeError:
            pass
    else:
        backup_mod.safe_extract_zip = original_safe_extract
    backup_mod.Path.exists = original_exists
    backup_mod.Path.mkdir = original_mkdir
    backup_mod.shutil.rmtree = original_rmtree
    backup_mod.shutil.copytree = original_copytree
    backup_mod.threading.Thread = original_thread
    mgr._iter_backup_user_roots = original_iter_roots
    mgr._find_sandboxie_control_exe = original_find_control
    mgr._reload_sandboxie_config = original_reload
    mgr.sandbox_users_root = original_sandbox_users_root
    mgr._safe_frame_call_after = original_safe_call_after
    frame.force_exit_requested = True
    frame.Destroy()
    app.Destroy()


assert started_threads, "restore work was not dispatched to a worker thread"
assert stop_thread_ids, "stop service path was not reached"
assert all(thread_id != main_thread_id for thread_id in stop_thread_ids), stop_thread_ids
print("PASS restore_no_main_thread_stop_probe")
