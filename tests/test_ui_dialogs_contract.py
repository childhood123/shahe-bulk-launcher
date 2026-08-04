import ast
import json
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
CONTRACT = json.loads(
    (ROOT / "tests" / "contracts" / "launcher_api.json").read_text(encoding="utf-8")
)
MODULE_CLASSES = {
    "common_dialogs.py": (
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


class UiDialogsContractTests(unittest.TestCase):
    def test_dialog_and_tray_classes_keep_direct_methods(self):
        for filename, class_names in MODULE_CLASSES.items():
            path = ROOT / "shahe_bulk_launcher" / "ui" / filename
            with self.subTest(filename=filename):
                self.assertTrue(path.is_file())
            if not path.is_file():
                continue
            tree = ast.parse(path.read_text(encoding="utf-8"))
            classes = {
                node.name: [
                    item.name
                    for item in node.body
                    if isinstance(item, (ast.FunctionDef, ast.AsyncFunctionDef))
                ]
                for node in tree.body
                if isinstance(node, ast.ClassDef)
            }
            for class_name in class_names:
                with self.subTest(class_name=class_name):
                    self.assertEqual(CONTRACT["classes"][class_name], classes[class_name])

    def test_common_dialog_helpers_are_moved(self):
        path = ROOT / "shahe_bulk_launcher" / "ui" / "common_dialogs.py"
        self.assertTrue(path.is_file())
        if not path.is_file():
            return
        tree = ast.parse(path.read_text(encoding="utf-8"))
        functions = {
            node.name
            for node in tree.body
            if isinstance(node, (ast.FunctionDef, ast.AsyncFunctionDef))
        }
        self.assertTrue({"_load_wx_image_from_bytes", "copy_image_to_clipboard"} <= functions)


if __name__ == "__main__":
    unittest.main()
