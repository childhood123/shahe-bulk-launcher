import ast
import json
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
PATH = ROOT / "shahe_bulk_launcher" / "features" / "process_actions.py"
ENTRY_PATH = ROOT / "沙河批量打开器.py"
METHOD_MODULES = json.loads(
    (ROOT / "tests" / "contracts" / "method_modules.json").read_text(encoding="utf-8")
)


class ProcessActionsContractTests(unittest.TestCase):
    def test_process_actions_keeps_assigned_methods_and_caches(self):
        self.assertTrue(PATH.is_file())
        if not PATH.is_file():
            return
        tree = ast.parse(PATH.read_text(encoding="utf-8"))
        target = next(
            node for node in tree.body
            if isinstance(node, ast.ClassDef) and node.name == "ProcessActionsMixin"
        )
        methods = [
            node.name
            for node in target.body
            if isinstance(node, (ast.FunctionDef, ast.AsyncFunctionDef))
        ]
        self.assertEqual(METHOD_MODULES["process_actions"], methods)
        assignments = {
            target.id
            for node in tree.body
            if isinstance(node, ast.Assign)
            for target in node.targets
            if isinstance(target, ast.Name)
        }
        self.assertTrue({"pythoncom", "AudioUtilities", "AudioRouter"} <= assignments)

    def test_entry_no_longer_owns_audio_backend_caches(self):
        tree = ast.parse(ENTRY_PATH.read_text(encoding="utf-8-sig"))
        assignments = {
            target.id
            for node in tree.body
            if isinstance(node, ast.Assign)
            for target in node.targets
            if isinstance(target, ast.Name)
        }
        self.assertTrue(
            {"pythoncom", "AudioUtilities", "AudioRouter"}.isdisjoint(assignments)
        )


if __name__ == "__main__":
    unittest.main()
