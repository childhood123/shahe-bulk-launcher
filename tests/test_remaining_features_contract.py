import ast
import json
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
FEATURE_DIR = ROOT / "shahe_bulk_launcher" / "features"
METHOD_MODULES = json.loads(
    (ROOT / "tests" / "contracts" / "method_modules.json").read_text(encoding="utf-8")
)


def direct_methods(path, class_name):
    tree = ast.parse(path.read_text(encoding="utf-8"))
    target = next(
        node for node in tree.body
        if isinstance(node, ast.ClassDef) and node.name == class_name
    )
    return [
        node.name
        for node in target.body
        if isinstance(node, (ast.FunctionDef, ast.AsyncFunctionDef))
    ]


class RemainingFeaturesContractTests(unittest.TestCase):
    def test_hotkeys_mixin_keeps_assigned_methods(self):
        path = FEATURE_DIR / "hotkeys.py"
        self.assertTrue(path.is_file())
        if path.is_file():
            self.assertEqual(
                METHOD_MODULES["hotkeys"],
                direct_methods(path, "HotkeysMixin"),
            )

    def test_sandbox_admin_mixin_keeps_assigned_methods(self):
        path = FEATURE_DIR / "sandbox_admin.py"
        self.assertTrue(path.is_file())
        if path.is_file():
            self.assertEqual(
                METHOD_MODULES["sandbox_admin"],
                direct_methods(path, "SandboxAdminMixin"),
            )


if __name__ == "__main__":
    unittest.main()
