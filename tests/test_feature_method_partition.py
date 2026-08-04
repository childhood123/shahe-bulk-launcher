import ast
import json
import unittest
from collections import Counter
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
APP = ROOT / "shahe_bulk_launcher" / "app.py"
FEATURE_DIR = ROOT / "shahe_bulk_launcher" / "features"
METHOD_MODULES = json.loads(
    (ROOT / "tests" / "contracts" / "method_modules.json").read_text(encoding="utf-8")
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


def _class_methods(path, class_name):
    tree = ast.parse(path.read_text(encoding="utf-8-sig"))
    target = next(
        node for node in tree.body
        if isinstance(node, ast.ClassDef) and node.name == class_name
    )
    methods = [
        node.name
        for node in target.body
        if isinstance(node, (ast.FunctionDef, ast.AsyncFunctionDef))
    ]
    bases = [ast.unparse(base) for base in target.bases]
    return methods, bases


class FeatureMethodPartitionTests(unittest.TestCase):
    def test_lifecycle_and_settings_modules_exist(self):
        self.assertTrue((FEATURE_DIR / "lifecycle.py").is_file())
        self.assertTrue((FEATURE_DIR / "settings.py").is_file())

    def test_methods_are_defined_exactly_once(self):
        entry_methods, entry_bases = _class_methods(APP, "SandboxLauncher")
        implemented = list(entry_methods)
        expected_entry = set(METHOD_MODULES["app"])

        for module_name, mixin_name in CLASS_NAMES.items():
            path = FEATURE_DIR / f"{module_name}.py"
            expected = METHOD_MODULES[module_name]
            if path.is_file():
                methods, _ = _class_methods(path, mixin_name)
                self.assertEqual(expected, methods, module_name)
                self.assertNotIn("__init__", methods, module_name)
                self.assertIn(mixin_name, entry_bases, module_name)
                implemented.extend(methods)
            else:
                expected_entry.update(expected)

        self.assertEqual(expected_entry, set(entry_methods))
        duplicates = sorted(name for name, count in Counter(implemented).items() if count > 1)
        self.assertEqual([], duplicates)
        expected_all = {name for methods in METHOD_MODULES.values() for name in methods}
        self.assertEqual(expected_all, set(implemented))

    def test_lifecycle_does_not_register_feature_file_as_startup_target(self):
        path = FEATURE_DIR / "lifecycle.py"
        self.assertTrue(path.is_file())
        if path.is_file():
            self.assertNotIn("os.path.abspath(__file__)", path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
