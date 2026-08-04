import ast
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "沙河批量打开器.py"
OUTPUT = ROOT / "shahe_bulk_launcher" / "ui" / "controls.py"
CLASS_NAMES = (
    "ManagedDelayedCall",
    "OperationStatusText",
    "SingleColumnListCtrl",
    "AppPathListCtrl",
    "CheckableListCtrl",
)
MODULE_PREAMBLE = """import os
import threading
import time

import wx
import wx.lib.mixins.listctrl as listmix

from ..constants import get_default_sandbox_root


"""
ENTRY_IMPORT = """from shahe_bulk_launcher.ui.controls import (
    AppPathListCtrl,
    CheckableListCtrl,
    ManagedDelayedCall,
    OperationStatusText,
    SingleColumnListCtrl,
)

"""


def main():
    text = SOURCE.read_text(encoding="utf-8-sig")
    lines = text.splitlines(keepends=True)
    tree = ast.parse(text)
    classes = []
    import_anchor = None

    for node in tree.body:
        if isinstance(node, ast.ClassDef) and node.name in CLASS_NAMES:
            classes.append(node)
        elif isinstance(node, ast.ImportFrom) and node.module == "shahe_bulk_launcher.single_instance":
            import_anchor = node.end_lineno

    found = [node.name for node in classes]
    if found != list(CLASS_NAMES):
        raise RuntimeError(f"Control class mismatch: {found}")
    if import_anchor is None:
        raise RuntimeError("Foundation import anchor is missing")

    class_texts = ["".join(lines[node.lineno - 1:node.end_lineno]).rstrip() for node in classes]
    for node in sorted(classes, key=lambda item: item.lineno, reverse=True):
        del lines[node.lineno - 1:node.end_lineno]

    lines.insert(import_anchor, "\n" + ENTRY_IMPORT)
    OUTPUT.write_text(MODULE_PREAMBLE + "\n\n".join(class_texts) + "\n", encoding="utf-8", newline="")
    SOURCE.write_text("".join(lines), encoding="utf-8", newline="")


if __name__ == "__main__":
    main()
