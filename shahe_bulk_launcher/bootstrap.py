import datetime
import os
import sys
from pathlib import Path


ENTRY_SCRIPT_PATH = Path(__file__).resolve().parent.parent / "沙河批量打开器.py"


def is_packaged_app():
    return bool(globals().get("__compiled__", False)) or os.environ.get("SHAHE_PACKAGED_APP") == "1"


def get_current_executable_path():
    if is_packaged_app():
        argv0 = str(getattr(sys, "argv", [""])[0] or "").strip()
        if argv0:
            executable_path = os.path.abspath(argv0)
            executable_dir = os.path.dirname(executable_path)
            if os.path.basename(executable_dir).lower() == "internal":
                public_executable = os.path.join(os.path.dirname(executable_dir), os.path.basename(executable_path))
                if os.path.exists(public_executable):
                    return public_executable
            return executable_path
    return str(ENTRY_SCRIPT_PATH)


def get_resource_path(relative_path):
    return os.path.join(get_app_dir(), relative_path)


def get_app_dir():
    if is_packaged_app():
        executable_dir = os.path.dirname(os.path.abspath(str(getattr(sys, "argv", [""])[0] or "")))
        if os.path.basename(executable_dir).lower() == "internal":
            return os.path.dirname(executable_dir)
        return os.path.dirname(get_current_executable_path())
    return str(ENTRY_SCRIPT_PATH.parent)


def _cleanup_release_internal_dir(internal_dir):
    for obsolete_name in ("沙河批量打开器.exe", "updater.exe"):
        try:
            obsolete_path = Path(internal_dir) / obsolete_name
            if obsolete_path.is_file():
                obsolete_path.unlink()
        except Exception:
            pass


def cleanup_legacy_release_artifacts():
    if not is_packaged_app():
        return
    app_dir = Path(get_app_dir())
    for candidate in (app_dir / "internal" / "沙河批量打开器.exe",):
        try:
            if candidate.is_file():
                candidate.unlink()
        except Exception:
            pass
    internal_dir = app_dir / "internal"
    if not internal_dir.is_dir():
        return
    _cleanup_release_internal_dir(internal_dir)


def append_daily_log(message):
    text = str(message or "").strip()
    if not text:
        return
    try:
        log_dir = Path(get_app_dir()) / "log"
        log_dir.mkdir(parents=True, exist_ok=True)
        log_file = log_dir / f"{datetime.datetime.now().strftime('%Y-%m-%d')}.log"
        timestamp = datetime.datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        with open(log_file, "a", encoding="utf-8") as file:
            file.write(f"[{timestamp}] {text}\n")
    except Exception:
        pass
