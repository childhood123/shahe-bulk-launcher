import ast
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "沙河批量打开器.py"
UI_DIR = ROOT / "shahe_bulk_launcher" / "ui"
GROUPS = {
    "common_dialogs.py": (
        "_load_wx_image_from_bytes",
        "copy_image_to_clipboard",
        "QRCodeDialog",
        "SelectUsersDialog",
        "GroupNameDialog",
        "BatchNameRuleDialog",
    ),
    "settings_dialogs.py": (
        "HotkeySettingsDialog",
        "SettingsDialog",
        "PathMemoryManagerDialog",
        "SpeechSettingsDialog",
    ),
    "tray.py": ("TaskBarIcon",),
}
PREAMBLES = {
    "common_dialogs.py": """import os

import wx

from ..common import end_dialog_safely
from .controls import CheckableListCtrl


""",
    "settings_dialogs.py": """import wx

from ..common import end_dialog_safely, parse_bool_setting, parse_int_setting
from ..constants import LOG_OUTPUT_CHOICES, LOG_OUTPUT_DETAILED, normalize_log_output_mode


""",
    "tray.py": """import wx


""",
}
ENTRY_IMPORT = """from shahe_bulk_launcher.ui.common_dialogs import (
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

"""


def main():
    text = SOURCE.read_text(encoding="utf-8-sig")
    lines = text.splitlines(keepends=True)
    tree = ast.parse(text)
    nodes_by_name = {}
    import_anchor = None

    wanted = {name for names in GROUPS.values() for name in names}
    for node in tree.body:
        if isinstance(node, (ast.FunctionDef, ast.AsyncFunctionDef, ast.ClassDef)) and node.name in wanted:
            nodes_by_name[node.name] = node
        elif isinstance(node, ast.ImportFrom) and node.module == "shahe_bulk_launcher.ui.controls":
            import_anchor = node.end_lineno

    if set(nodes_by_name) != wanted:
        raise RuntimeError(f"UI node mismatch: {sorted(wanted - set(nodes_by_name))}")
    if import_anchor is None:
        raise RuntimeError("UI controls import anchor is missing")

    for filename, names in GROUPS.items():
        parts = [
            "".join(lines[nodes_by_name[name].lineno - 1:nodes_by_name[name].end_lineno]).rstrip()
            for name in names
        ]
        (UI_DIR / filename).write_text(
            PREAMBLES[filename] + "\n\n".join(parts) + "\n",
            encoding="utf-8",
            newline="",
        )

    for node in sorted(nodes_by_name.values(), key=lambda item: item.lineno, reverse=True):
        del lines[node.lineno - 1:node.end_lineno]

    lines.insert(import_anchor, "\n" + ENTRY_IMPORT)
    SOURCE.write_text("".join(lines), encoding="utf-8", newline="")


if __name__ == "__main__":
    main()
