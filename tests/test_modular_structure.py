import ast
import json
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
CONTRACT = json.loads(
    (ROOT / "tests" / "contracts" / "launcher_api.json").read_text(encoding="utf-8")
)
EXPECTED_MODULES = (
    "shahe_bulk_launcher/app.py",
    "shahe_bulk_launcher/bootstrap.py",
    "shahe_bulk_launcher/common.py",
    "shahe_bulk_launcher/constants.py",
    "shahe_bulk_launcher/backup.py",
    "shahe_bulk_launcher/single_instance.py",
    "shahe_bulk_launcher/ui/controls.py",
    "shahe_bulk_launcher/ui/common_dialogs.py",
    "shahe_bulk_launcher/ui/settings_dialogs.py",
    "shahe_bulk_launcher/ui/tray.py",
    "shahe_bulk_launcher/features/lifecycle.py",
    "shahe_bulk_launcher/features/settings.py",
    "shahe_bulk_launcher/features/app_paths.py",
    "shahe_bulk_launcher/features/sandboxie.py",
    "shahe_bulk_launcher/features/process_actions.py",
    "shahe_bulk_launcher/features/updates.py",
    "shahe_bulk_launcher/features/speech.py",
    "shahe_bulk_launcher/features/hotkeys.py",
    "shahe_bulk_launcher/features/sandbox_admin.py",
    "shahe_bulk_launcher/features/scheduling.py",
)


class ModularStructureTests(unittest.TestCase):
    def test_target_modules_exist(self):
        missing = [path for path in EXPECTED_MODULES if not (ROOT / path).is_file()]
        self.assertEqual([], missing)

    def test_contract_keeps_original_launcher_methods(self):
        self.assertEqual(398, len(CONTRACT["classes"]["SandboxLauncher"]))

    def test_feature_modules_do_not_import_app(self):
        feature_dir = ROOT / "shahe_bulk_launcher" / "features"
        if not feature_dir.is_dir():
            self.fail("feature package is missing")
        for path in feature_dir.glob("*.py"):
            tree = ast.parse(path.read_text(encoding="utf-8"))
            imports = {
                alias.name
                for node in ast.walk(tree)
                if isinstance(node, ast.Import)
                for alias in node.names
            }
            from_imports = {
                node.module or ""
                for node in ast.walk(tree)
                if isinstance(node, ast.ImportFrom)
            }
            self.assertNotIn("shahe_bulk_launcher.app", imports | from_imports)


if __name__ == "__main__":
    unittest.main()
