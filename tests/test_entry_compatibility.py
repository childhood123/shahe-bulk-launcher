import ast
import importlib
import importlib.util
import json
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
ENTRY_PATH = ROOT / "沙河批量打开器.py"
APP_PATH = ROOT / "shahe_bulk_launcher" / "app.py"
CONTRACT = json.loads(
    (ROOT / "tests" / "contracts" / "launcher_api.json").read_text(encoding="utf-8")
)
EXPECTED_BASES = [
    "LifecycleMixin",
    "SettingsMixin",
    "AppPathsMixin",
    "SandboxieMixin",
    "ProcessActionsMixin",
    "UpdatesMixin",
    "SpeechMixin",
    "HotkeysMixin",
    "SandboxAdminMixin",
    "SchedulingMixin",
    "wx.Frame",
]


class EntryCompatibilityTests(unittest.TestCase):
    def test_app_module_assembles_launcher_with_only_init(self):
        self.assertTrue(APP_PATH.is_file())
        if not APP_PATH.is_file():
            return
        tree = ast.parse(APP_PATH.read_text(encoding="utf-8"))
        target = next(
            node for node in tree.body
            if isinstance(node, ast.ClassDef) and node.name == "SandboxLauncher"
        )
        methods = [
            node.name
            for node in target.body
            if isinstance(node, (ast.FunctionDef, ast.AsyncFunctionDef))
        ]
        self.assertEqual(["__init__"], methods)
        self.assertEqual(EXPECTED_BASES, [ast.unparse(base) for base in target.bases])

    def test_entry_exports_original_api_from_app(self):
        self.assertTrue(APP_PATH.is_file())
        if not APP_PATH.is_file():
            return
        spec = importlib.util.spec_from_file_location("shahe_entry_compat_target", ENTRY_PATH)
        entry = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(entry)
        app_module = importlib.import_module("shahe_bulk_launcher.app")

        for name in CONTRACT["functions"]:
            self.assertTrue(hasattr(entry, name), name)
        for name in CONTRACT["classes"]:
            self.assertTrue(hasattr(entry, name), name)
        for name in CONTRACT["constants"]:
            self.assertTrue(hasattr(entry, name), name)
        for name in CONTRACT["classes"]["SandboxLauncher"]:
            self.assertTrue(callable(getattr(entry.SandboxLauncher, name, None)), name)

        self.assertIs(entry.SandboxLauncher, app_module.SandboxLauncher)
        self.assertIs(entry.main, app_module.main)
        entry_tree = ast.parse(ENTRY_PATH.read_text(encoding="utf-8-sig"))
        self.assertFalse(
            any(
                isinstance(node, ast.ClassDef) and node.name == "SandboxLauncher"
                for node in entry_tree.body
            )
        )

    def test_package_lazily_exposes_app_api(self):
        self.assertTrue(APP_PATH.is_file())
        if not APP_PATH.is_file():
            return
        package = importlib.import_module("shahe_bulk_launcher")
        app_module = importlib.import_module("shahe_bulk_launcher.app")
        self.assertIs(package.SandboxLauncher, app_module.SandboxLauncher)
        self.assertIs(package.main, app_module.main)


if __name__ == "__main__":
    unittest.main()
