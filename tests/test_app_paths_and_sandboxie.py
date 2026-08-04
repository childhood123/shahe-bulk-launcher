import ast
import json
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
FEATURE_DIR = ROOT / "shahe_bulk_launcher" / "features"
METHOD_MODULES = json.loads(
    (ROOT / "tests" / "contracts" / "method_modules.json").read_text(encoding="utf-8")
)
TARGETS = {
    "app_paths": "AppPathsMixin",
    "sandboxie": "SandboxieMixin",
}


class AppPathsAndSandboxieTests(unittest.TestCase):
    def test_target_mixins_keep_assigned_methods(self):
        for module_name, class_name in TARGETS.items():
            path = FEATURE_DIR / f"{module_name}.py"
            with self.subTest(module_name=module_name):
                self.assertTrue(path.is_file())
            if not path.is_file():
                continue
            tree = ast.parse(path.read_text(encoding="utf-8"))
            target = next(
                node for node in tree.body
                if isinstance(node, ast.ClassDef) and node.name == class_name
            )
            methods = [
                node.name
                for node in target.body
                if isinstance(node, (ast.FunctionDef, ast.AsyncFunctionDef))
            ]
            self.assertEqual(METHOD_MODULES[module_name], methods)


if __name__ == "__main__":
    unittest.main()
