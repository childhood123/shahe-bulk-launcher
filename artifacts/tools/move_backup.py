import ast
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "沙河批量打开器.py"
OUTPUT = ROOT / "shahe_bulk_launcher" / "backup.py"
PREAMBLE = """import codecs
import datetime
import os
import re
import shutil
import subprocess
import sys
import threading
import time
import zipfile
from pathlib import Path

import wx

from .bootstrap import get_app_dir, get_current_executable_path, is_packaged_app
from .common import get_update_temp_dir
from .constants import get_default_sandbox_root
from .ui.common_dialogs import SelectUsersDialog


"""
ENTRY_IMPORT = "from shahe_bulk_launcher.backup import BackupRestoreMgr\n\n"


def main():
    text = SOURCE.read_text(encoding="utf-8-sig")
    lines = text.splitlines(keepends=True)
    tree = ast.parse(text)
    backup_class = None
    import_anchor = None

    for node in tree.body:
        if isinstance(node, ast.ClassDef) and node.name == "BackupRestoreMgr":
            backup_class = node
        elif isinstance(node, ast.ImportFrom) and node.module == "shahe_bulk_launcher.ui.tray":
            import_anchor = node.end_lineno

    if backup_class is None or import_anchor is None:
        raise RuntimeError("Backup migration anchors are missing")

    class_text = "".join(lines[backup_class.lineno - 1:backup_class.end_lineno]).rstrip()
    restart_source = "[sys.executable, __file__] if not is_packaged_app() else [get_current_executable_path()]"
    restart_target = "[sys.executable, get_current_executable_path()] if not is_packaged_app() else [get_current_executable_path()]"
    if restart_source not in class_text:
        raise RuntimeError("Backup restart command no longer matches the baseline")
    class_text = class_text.replace(restart_source, restart_target, 1)

    del lines[backup_class.lineno - 1:backup_class.end_lineno]
    lines.insert(import_anchor, "\n" + ENTRY_IMPORT)
    OUTPUT.write_text(PREAMBLE + class_text + "\n", encoding="utf-8", newline="")
    SOURCE.write_text("".join(lines), encoding="utf-8", newline="")


if __name__ == "__main__":
    main()
