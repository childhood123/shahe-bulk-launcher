import argparse
import ast
import json
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "沙河批量打开器.py"
FEATURE_DIR = ROOT / "shahe_bulk_launcher" / "features"
METHOD_MODULES = json.loads(
    (ROOT / "tests" / "contracts" / "method_modules.json").read_text(encoding="utf-8")
)
ORDER = (
    "lifecycle",
    "settings",
    "app_paths",
    "sandboxie",
    "process_actions",
    "updates",
    "speech",
    "hotkeys",
    "sandbox_admin",
    "scheduling",
)
CLASS_NAMES = {
    "lifecycle": "LifecycleMixin",
    "settings": "SettingsMixin",
    "app_paths": "AppPathsMixin",
    "sandboxie": "SandboxieMixin",
    "process_actions": "ProcessActionsMixin",
    "updates": "UpdatesMixin",
    "speech": "SpeechMixin",
    "hotkeys": "HotkeysMixin",
    "sandbox_admin": "SandboxAdminMixin",
    "scheduling": "SchedulingMixin",
}
PREAMBLES = {
    "lifecycle": """import os
import threading
import time
import winreg

import wx

from ..bootstrap import append_daily_log, get_current_executable_path, is_packaged_app
from ..constants import LOG_OUTPUT_DETAILED, normalize_log_output_mode
from ..ui.controls import AppPathListCtrl, CheckableListCtrl, ManagedDelayedCall, OperationStatusText
from ..ui.tray import TaskBarIcon


""",
    "settings": """import json
import os

import wx

from ..common import parse_bool_setting, parse_int_setting
from ..constants import (
    LOG_OUTPUT_DETAILED,
    UPDATE_MANIFEST_URL,
    get_default_sandbox_root,
    normalize_log_output_mode,
    normalize_sandbox_root_path,
)
from ..ui.settings_dialogs import SettingsDialog


""",
    "app_paths": """import os
import re
import time
from urllib.parse import unquote

import wx

from ..common import end_dialog_safely, parse_bool_setting, parse_int_setting
from ..ui.settings_dialogs import PathMemoryManagerDialog


""",
    "sandboxie": """import ctypes
import glob
import os
import re
import subprocess
import tempfile
import time
import winreg
from ctypes import wintypes

import wx

from ..common import get_hidden_subprocess_kwargs
from ..constants import (
    get_internal_sandbox_name_validation_error,
    get_sandbox_display_name_validation_error,
    is_valid_internal_sandbox_name,
)


""",
}


def _feature_import(module_name):
    return f"from shahe_bulk_launcher.features.{module_name} import {CLASS_NAMES[module_name]}\n"


def main(selected_modules):
    selected = tuple(module for module in ORDER if module in selected_modules)
    if set(selected) != set(selected_modules):
        raise RuntimeError(f"Unknown feature modules: {sorted(set(selected_modules) - set(ORDER))}")
    for module_name in selected:
        if module_name not in PREAMBLES:
            raise RuntimeError(f"Missing preamble for {module_name}")
        if (FEATURE_DIR / f"{module_name}.py").exists():
            raise RuntimeError(f"Feature module already exists: {module_name}")

    text = SOURCE.read_text(encoding="utf-8-sig")
    lines = text.splitlines(keepends=True)
    tree = ast.parse(text)
    launcher = next(
        node for node in tree.body
        if isinstance(node, ast.ClassDef) and node.name == "SandboxLauncher"
    )
    methods = {
        node.name: node
        for node in launcher.body
        if isinstance(node, (ast.FunctionDef, ast.AsyncFunctionDef))
    }

    selected_nodes = []
    for module_name in selected:
        expected_names = METHOD_MODULES[module_name]
        missing = [name for name in expected_names if name not in methods]
        if missing:
            raise RuntimeError(f"Missing {module_name} methods: {missing}")
        module_nodes = [methods[name] for name in expected_names]
        method_texts = [
            "".join(lines[node.lineno - 1:node.end_lineno]).rstrip()
            for node in module_nodes
        ]
        body = "\n\n".join(method_texts)
        if module_name == "lifecycle":
            old_path = "get_current_executable_path() if is_packaged_app() else os.path.abspath(__file__)"
            if old_path not in body:
                raise RuntimeError("Lifecycle startup path no longer matches the baseline")
            body = body.replace(old_path, "get_current_executable_path()", 1)
        output = FEATURE_DIR / f"{module_name}.py"
        output.write_text(
            PREAMBLES[module_name] + f"class {CLASS_NAMES[module_name]}:\n" + body + "\n",
            encoding="utf-8",
            newline="",
        )
        selected_nodes.extend(module_nodes)

    for node in sorted(selected_nodes, key=lambda item: item.lineno, reverse=True):
        del lines[node.lineno - 1:node.end_lineno]

    first_body_line = min(node.lineno for node in launcher.body)
    migrated = {
        module_name
        for module_name in ORDER
        if (FEATURE_DIR / f"{module_name}.py").exists() or module_name in selected
    }
    header = ["class SandboxLauncher(\n"]
    header.extend(f"    {CLASS_NAMES[module_name]},\n" for module_name in ORDER if module_name in migrated)
    header.extend(["    wx.Frame,\n", "):\n"])
    lines[launcher.lineno - 1:first_body_line - 1] = header

    import_anchor = None
    for node in tree.body:
        if isinstance(node, ast.ImportFrom):
            if node.module == "shahe_bulk_launcher.backup" or str(node.module or "").startswith("shahe_bulk_launcher.features."):
                import_anchor = node.end_lineno
    if import_anchor is None:
        raise RuntimeError("Feature import anchor is missing")
    import_text = "".join(_feature_import(module_name) for module_name in selected) + "\n"
    lines.insert(import_anchor, import_text)
    SOURCE.write_text("".join(lines), encoding="utf-8", newline="")


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("modules", nargs="+")
    args = parser.parse_args()
    main(args.modules)
