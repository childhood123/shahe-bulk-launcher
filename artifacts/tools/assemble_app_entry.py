import ast
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
ENTRY = ROOT / "沙河批量打开器.py"
APP = ROOT / "shahe_bulk_launcher" / "app.py"
PACKAGE_INIT = ROOT / "shahe_bulk_launcher" / "__init__.py"

APP_PREAMBLE = """import os
import threading

import wx

from .bootstrap import cleanup_legacy_release_artifacts, get_app_dir
from .common import (
    ConfigStore,
    get_configuration_db_path,
    get_configuration_dir,
    parse_bool_setting,
    parse_int_setting,
)
from .constants import (
    APP_VERSION,
    LOG_OUTPUT_DETAILED,
    OLD_DEFAULT_SANDBOX_ROOT,
    get_default_sandbox_root,
    normalize_log_output_mode,
    normalize_sandbox_root_path,
)
from .features.app_paths import AppPathsMixin
from .features.hotkeys import HotkeysMixin
from .features.lifecycle import LifecycleMixin
from .features.process_actions import ProcessActionsMixin
from .features.sandbox_admin import SandboxAdminMixin
from .features.sandboxie import SandboxieMixin
from .features.scheduling import SchedulingMixin
from .features.settings import SettingsMixin
from .features.speech import SpeechMixin
from .features.updates import UpdatesMixin
from .single_instance import SingleInstance


"""

CLASS_HEADER = """class SandboxLauncher(
    LifecycleMixin,
    SettingsMixin,
    AppPathsMixin,
    SandboxieMixin,
    ProcessActionsMixin,
    UpdatesMixin,
    SpeechMixin,
    HotkeysMixin,
    SandboxAdminMixin,
    SchedulingMixin,
    wx.Frame,
):
"""

ENTRY_IMPORTS = """import wx
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
"""

PACKAGE_TEXT = '''"""沙河批量打开器实现包。"""

__all__ = ("SandboxLauncher", "main")


def __getattr__(name):
    if name in __all__:
        from .app import SandboxLauncher, main

        return {"SandboxLauncher": SandboxLauncher, "main": main}[name]
    raise AttributeError(name)


def __dir__():
    return sorted(set(globals()) | set(__all__))
'''


def main():
    text = ENTRY.read_text(encoding="utf-8-sig")
    lines = text.splitlines(keepends=True)
    tree = ast.parse(text)
    launcher = next(
        node for node in tree.body
        if isinstance(node, ast.ClassDef) and node.name == "SandboxLauncher"
    )
    direct_methods = [
        node
        for node in launcher.body
        if isinstance(node, (ast.FunctionDef, ast.AsyncFunctionDef))
    ]
    if [node.name for node in direct_methods] != ["__init__"]:
        raise RuntimeError("SandboxLauncher must contain only __init__ before final assembly")
    init_node = direct_methods[0]
    init_text = "".join(lines[init_node.lineno - 1:init_node.end_lineno]).rstrip()

    main_node = next(
        node for node in tree.body
        if isinstance(node, ast.FunctionDef) and node.name == "main"
    )
    main_text = "".join(lines[main_node.lineno - 1:main_node.end_lineno]).rstrip()

    bootstrap_call = next(
        node for node in tree.body
        if isinstance(node, ast.Expr)
        and isinstance(node.value, ast.Call)
        and isinstance(node.value.func, ast.Name)
        and node.value.func.id == "_bootstrap_internal_runtime"
    )
    bootstrap_text = "".join(lines[:bootstrap_call.end_lineno]).rstrip()

    APP.write_text(
        APP_PREAMBLE + CLASS_HEADER + init_text + "\n\n\n" + main_text + "\n",
        encoding="utf-8",
        newline="",
    )
    ENTRY.write_text(
        bootstrap_text + "\n\n" + ENTRY_IMPORTS,
        encoding="utf-8",
        newline="",
    )
    PACKAGE_INIT.write_text(PACKAGE_TEXT, encoding="utf-8", newline="")


if __name__ == "__main__":
    main()
