import ast
import json
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
CONTRACT = json.loads(
    (ROOT / "tests" / "contracts" / "launcher_api.json").read_text(encoding="utf-8")
)
CLASS_NAMES = (
    "ManagedDelayedCall",
    "OperationStatusText",
    "SingleColumnListCtrl",
    "AppPathListCtrl",
    "CheckableListCtrl",
)


class UiControlsContractTests(unittest.TestCase):
    def test_control_classes_keep_direct_methods(self):
        controls_path = ROOT / "shahe_bulk_launcher" / "ui" / "controls.py"
        self.assertTrue(controls_path.is_file())
        tree = ast.parse(controls_path.read_text(encoding="utf-8"))
        classes = {
            node.name: [
                item.name
                for item in node.body
                if isinstance(item, (ast.FunctionDef, ast.AsyncFunctionDef))
            ]
            for node in tree.body
            if isinstance(node, ast.ClassDef)
        }

        for class_name in CLASS_NAMES:
            with self.subTest(class_name=class_name):
                self.assertEqual(CONTRACT["classes"][class_name], classes[class_name])


if __name__ == "__main__":
    unittest.main()
