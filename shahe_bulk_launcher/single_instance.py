import ctypes
import os

import wx

from .bootstrap import get_app_dir


class SingleInstance:
    def __init__(self, name):
        base_name = str(name or "SandboxLauncher").strip() or "SandboxLauncher"
        self.name = base_name
        self.mutex = None
        self._kernel32 = None
        self._lock_path = ""

    def _check_by_mutex(self):
        try:
            kernel32 = ctypes.windll.kernel32
            mutex_name = f"Global\\{self.name}"
            handle = kernel32.CreateMutexW(None, False, mutex_name)
            if not handle:
                return None
            last_error = int(kernel32.GetLastError())
            error_already_exists = 183
            if last_error == error_already_exists:
                try:
                    kernel32.CloseHandle(handle)
                except Exception:
                    pass
                return False
            self.mutex = handle
            self._kernel32 = kernel32
            return True
        except Exception:
            return None

    def _check_by_lock_file(self):
        temp_dir = os.getenv("TEMP") or get_app_dir()
        lock_path = os.path.join(temp_dir, f"{self.name}.lock")
        self._lock_path = lock_path
        try:
            if os.path.exists(lock_path):
                with open(lock_path, "r", encoding="utf-8") as file:
                    content = str(file.read() or "").strip()
                try:
                    old_pid = int(content)
                except Exception:
                    old_pid = 0
                if old_pid > 0 and old_pid != os.getpid():
                    try:
                        if self._pid_exists_windows(old_pid):
                            return False
                    except Exception:
                        return False
            with open(lock_path, "w", encoding="utf-8") as file:
                file.write(str(os.getpid()))
            return True
        except Exception:
            return True

    def _pid_exists_windows(self, pid_value):
        try:
            pid = int(pid_value)
        except Exception:
            return False
        if pid <= 0:
            return False
        try:
            kernel32 = ctypes.windll.kernel32
            process_query_limited_information = 0x1000
            handle = kernel32.OpenProcess(process_query_limited_information, False, pid)
            if handle:
                kernel32.CloseHandle(handle)
                return True
        except Exception:
            return False
        return False

    def notify_existing_instance(self):
        try:
            user32 = ctypes.windll.user32
            hwnd = user32.FindWindowW(None, "沙盒批量打开器")
            if not hwnd:
                hwnd = user32.FindWindowW(None, "沙河批量打开器")
            if hwnd:
                sw_restore = 9
                user32.ShowWindow(hwnd, sw_restore)
                user32.SetForegroundWindow(hwnd)
        except Exception:
            pass
        app = wx.App(False)
        wx.MessageBox("沙河批量打开器正在运行中", "提示", wx.OK | wx.ICON_INFORMATION)
        app.Destroy()

    def check(self):
        mutex_result = self._check_by_mutex()
        if mutex_result is not None:
            return bool(mutex_result)
        return self._check_by_lock_file()

    def release(self):
        try:
            if self.mutex and self._kernel32 is not None:
                self._kernel32.CloseHandle(self.mutex)
        except Exception:
            pass
        try:
            if self._lock_path and os.path.exists(self._lock_path):
                os.unlink(self._lock_path)
        except Exception:
            pass
