import sys
import os
import subprocess
import time
import winreg
import glob
import json
import shutil
import tempfile
from pathlib import Path

_INTERNAL_DLL_HANDLES = []


def _is_packaged_runtime():
    return bool(globals().get("__compiled__", False)) or os.environ.get("SHAHE_PACKAGED_APP") == "1"


def _cleanup_release_internal_dir(internal_dir):
    for obsolete_name in ("沙河批量打开器.exe", "updater.exe"):
        try:
            obsolete_path = Path(internal_dir) / obsolete_name
            if obsolete_path.is_file():
                obsolete_path.unlink()
        except Exception:
            pass


def _bootstrap_internal_runtime():
    if not _is_packaged_runtime():
        return
    try:
        executable_path = Path(str(getattr(sys, "argv", [""])[0] or "")).resolve()
    except Exception:
        executable_path = Path(__file__).resolve()
    executable_dir = executable_path.parent
    if executable_dir.name.lower() == "internal":
        app_root = executable_dir.parent
    else:
        app_root = executable_dir
    internal_dir = app_root / "internal"
    if not internal_dir.is_dir():
        return
    _cleanup_release_internal_dir(internal_dir)
    internal_text = str(internal_dir)
    if internal_text not in sys.path:
        sys.path.insert(0, internal_text)
    path_parts = [part for part in os.environ.get("PATH", "").split(os.pathsep) if part]
    if os.path.normcase(internal_text) not in {os.path.normcase(part) for part in path_parts}:
        os.environ["PATH"] = internal_text + (os.pathsep + os.environ.get("PATH", "") if os.environ.get("PATH") else "")
    if hasattr(os, "add_dll_directory"):
        try:
            _INTERNAL_DLL_HANDLES.append(os.add_dll_directory(internal_text))
        except Exception:
            pass


_bootstrap_internal_runtime()

import wx
import wx.lib.mixins.listctrl as listmix
import threading
import datetime
import zipfile
import configparser
import re
import ctypes
import codecs
import urllib.request
import urllib.error
from ctypes import wintypes
from urllib.parse import unquote

SCRIPT_DIR = Path(__file__).resolve().parent
for shared_root in (
    SCRIPT_DIR,
    SCRIPT_DIR / "MyCodex",
    SCRIPT_DIR.parent if SCRIPT_DIR.name == "shahe_bulk_launcher" else None,
):
    if shared_root and (shared_root / "shahe_bulk_launcher" / "common.py").exists():
        shared_root_path = str(shared_root)
        if shared_root_path not in sys.path:
            sys.path.insert(0, shared_root_path)
        break

if _is_packaged_runtime():
    from common import (
        ConfigStore,
        compare_versions,
        end_dialog_safely,
        get_configuration_db_path,
        get_configuration_dir,
        get_hidden_subprocess_kwargs,
        get_update_temp_dir,
        is_allowed_update_body_status,
        open_update_url,
        parse_bool_setting,
        parse_int_setting,
        parse_version_parts,
    )
else:
    from shahe_bulk_launcher.common import (
        ConfigStore,
        compare_versions,
        end_dialog_safely,
        get_configuration_db_path,
        get_configuration_dir,
        get_hidden_subprocess_kwargs,
        get_update_temp_dir,
        is_allowed_update_body_status,
        open_update_url,
        parse_bool_setting,
        parse_int_setting,
        parse_version_parts,
    )

from shahe_bulk_launcher.bootstrap import (
    append_daily_log,
    cleanup_legacy_release_artifacts,
    get_app_dir,
    get_current_executable_path,
    get_resource_path,
    is_packaged_app,
)
from shahe_bulk_launcher.constants import (
    APP_VERSION,
    FIXED_HOTKEY_LABEL_BY_ACTION,
    FIXED_HOTKEY_TEXT_BY_ACTION,
    LOG_OUTPUT_CHOICES,
    LOG_OUTPUT_DETAILED,
    LOG_OUTPUT_OFF,
    OLD_DEFAULT_SANDBOX_ROOT,
    SANDBOX_INTERNAL_NAME_PATTERN,
    UPDATE_MANIFEST_URL,
    get_default_sandbox_root,
    get_internal_sandbox_name_validation_error,
    get_sandbox_display_name_validation_error,
    is_valid_internal_sandbox_name,
    make_default_sandbox_display_prefix,
    normalize_log_output_mode,
    normalize_sandbox_root_path,
)
from shahe_bulk_launcher.single_instance import SingleInstance
from shahe_bulk_launcher.ui.controls import (
    AppPathListCtrl,
    CheckableListCtrl,
    ManagedDelayedCall,
    OperationStatusText,
    SingleColumnListCtrl,
)
from shahe_bulk_launcher.ui.common_dialogs import (
    BatchNameRuleDialog,
    GroupNameDialog,
    QRCodeDialog,
    SelectUsersDialog,
    _load_wx_image_from_bytes,
    copy_image_to_clipboard,
)
from shahe_bulk_launcher.ui.settings_dialogs import (
    HotkeySettingsDialog,
    PathMemoryManagerDialog,
    SettingsDialog,
    SpeechSettingsDialog,
)
from shahe_bulk_launcher.ui.tray import TaskBarIcon
from shahe_bulk_launcher.backup import BackupRestoreMgr
from shahe_bulk_launcher.app import SandboxLauncher, main


if __name__ == "__main__":
    main()
