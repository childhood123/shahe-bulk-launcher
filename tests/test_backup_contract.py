import ast
import json
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
CONTRACT = json.loads(
    (ROOT / "tests" / "contracts" / "launcher_api.json").read_text(encoding="utf-8")
)


class BackupContractTests(unittest.TestCase):
    def test_backup_restore_manager_keeps_direct_methods(self):
        path = ROOT / "shahe_bulk_launcher" / "backup.py"
        self.assertTrue(path.is_file())
        if not path.is_file():
            return
        tree = ast.parse(path.read_text(encoding="utf-8"))
        backup_class = next(
            node for node in tree.body
            if isinstance(node, ast.ClassDef) and node.name == "BackupRestoreMgr"
        )
        methods = [
            node.name
            for node in backup_class.body
            if isinstance(node, (ast.FunctionDef, ast.AsyncFunctionDef))
        ]
        self.assertEqual(CONTRACT["classes"]["BackupRestoreMgr"], methods)
        source = path.read_text(encoding="utf-8")
        self.assertNotIn("[sys.executable, __file__]", source)


if __name__ == "__main__":
    unittest.main()
