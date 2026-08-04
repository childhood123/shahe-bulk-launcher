import ast
import json
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
ENTRY_PATH = ROOT / "沙河批量打开器.py"
FEATURE_DIR = ROOT / "shahe_bulk_launcher" / "features"
METHOD_MODULES = json.loads(
    (ROOT / "tests" / "contracts" / "method_modules.json").read_text(encoding="utf-8")
)


def assigned_names(tree):
    return {
        target.id
        for node in ast.walk(tree)
        if isinstance(node, ast.Assign)
        for target in node.targets
        if isinstance(target, ast.Name)
    }


class UpdatesAndSpeechContractTests(unittest.TestCase):
    def test_updates_mixin_keeps_assigned_methods_and_qr_cache(self):
        path = FEATURE_DIR / "updates.py"
        self.assertTrue(path.is_file())
        if not path.is_file():
            return
        tree = ast.parse(path.read_text(encoding="utf-8"))
        target = next(
            node for node in tree.body
            if isinstance(node, ast.ClassDef) and node.name == "UpdatesMixin"
        )
        methods = [
            node.name
            for node in target.body
            if isinstance(node, (ast.FunctionDef, ast.AsyncFunctionDef))
        ]
        self.assertEqual(METHOD_MODULES["updates"], methods)
        self.assertIn("QR_EMBEDDED_IMAGES", assigned_names(tree))
        self.assertTrue(
            any(
                isinstance(node, ast.ImportFrom) and node.module == "qr_embedded"
                for node in ast.walk(tree)
            )
        )

    def test_entry_no_longer_owns_qr_embedded_cache(self):
        tree = ast.parse(ENTRY_PATH.read_text(encoding="utf-8-sig"))
        self.assertNotIn("QR_EMBEDDED_IMAGES", assigned_names(tree))
        self.assertFalse(
            any(
                isinstance(node, ast.ImportFrom) and node.module == "qr_embedded"
                for node in ast.walk(tree)
            )
        )

    def test_speech_mixin_keeps_assigned_methods_and_entry_root_semantics(self):
        path = FEATURE_DIR / "speech.py"
        self.assertTrue(path.is_file())
        if not path.is_file():
            return
        tree = ast.parse(path.read_text(encoding="utf-8"))
        target = next(
            node for node in tree.body
            if isinstance(node, ast.ClassDef) and node.name == "SpeechMixin"
        )
        methods = [
            node.name
            for node in target.body
            if isinstance(node, (ast.FunctionDef, ast.AsyncFunctionDef))
        ]
        self.assertEqual(METHOD_MODULES["speech"], methods)
        self.assertFalse(
            any(
                isinstance(node, ast.Name) and node.id == "__file__"
                for node in ast.walk(tree)
            ),
            "speech resource lookup must not use the moved feature file as its root",
        )


if __name__ == "__main__":
    unittest.main()
