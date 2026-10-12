import ast
import json
import stat
import tempfile
import unittest
import zipfile
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
CONTRACT = json.loads(
    (ROOT / "tests" / "contracts" / "launcher_api.json").read_text(encoding="utf-8")
)


class BackupContractTests(unittest.TestCase):
    def _safe_extract_zip(self):
        from shahe_bulk_launcher import backup as backup_mod

        safe_extract = getattr(backup_mod, "safe_extract_zip", None)
        self.assertTrue(callable(safe_extract), "backup.safe_extract_zip must exist")
        return safe_extract

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

    def test_safe_extract_zip_extracts_regular_backup(self):
        safe_extract = self._safe_extract_zip()
        if not safe_extract:
            return
        with tempfile.TemporaryDirectory() as temp:
            temp_path = Path(temp)
            archive = temp_path / "backup.sbx"
            output = temp_path / "output"
            with zipfile.ZipFile(archive, "w") as zf:
                zf.writestr("Sandboxie.ini", "[Box1]\nEnabled=y\n")
                zf.writestr("SandboxUsers/Box1/RegHive", b"registry-data")

            safe_extract(archive, output)

            self.assertEqual(
                (output / "SandboxUsers" / "Box1" / "RegHive").read_bytes(),
                b"registry-data",
            )

    def test_safe_extract_zip_rejects_parent_traversal(self):
        self._assert_unsafe_member_rejected("../outside.txt")
        self._assert_unsafe_member_rejected(r"..\outside.txt")

    def test_safe_extract_zip_rejects_absolute_and_drive_paths(self):
        self._assert_unsafe_member_rejected("/absolute.txt")
        self._assert_unsafe_member_rejected(r"C:\absolute.txt")
        self._assert_unsafe_member_rejected(r"\\server\share\outside.txt")

    def test_safe_extract_zip_rejects_symbolic_links(self):
        safe_extract = self._safe_extract_zip()
        if not safe_extract:
            return
        with tempfile.TemporaryDirectory() as temp:
            temp_path = Path(temp)
            archive = temp_path / "backup.sbx"
            output = temp_path / "output"
            link = zipfile.ZipInfo("SandboxUsers/Box1/link")
            link.create_system = 3
            link.external_attr = (stat.S_IFLNK | 0o777) << 16
            with zipfile.ZipFile(archive, "w") as zf:
                zf.writestr(link, "../../outside.txt")

            with self.assertRaises(ValueError):
                safe_extract(archive, output)

    def test_find_control_supports_classic_sbiectrl(self):
        from shahe_bulk_launcher.backup import BackupRestoreMgr

        with tempfile.TemporaryDirectory() as temp:
            install_dir = Path(temp)
            control = install_dir / "SbieCtrl.exe"
            control.write_bytes(b"")
            frame = type(
                "Frame",
                (),
                {
                    "sandbox_root": str(install_dir / "Sandbox"),
                    "SANDBOXIE_PATH": str(install_dir),
                },
            )()

            found = BackupRestoreMgr(frame)._find_sandboxie_control_exe()

            self.assertEqual(str(control), found)

    def _assert_unsafe_member_rejected(self, member_name):
        safe_extract = self._safe_extract_zip()
        if not safe_extract:
            return
        with tempfile.TemporaryDirectory() as temp:
            temp_path = Path(temp)
            archive = temp_path / "backup.sbx"
            output = temp_path / "output"
            with zipfile.ZipFile(archive, "w") as zf:
                zf.writestr(member_name, "unsafe")

            with self.assertRaises(ValueError):
                safe_extract(archive, output)


if __name__ == "__main__":
    unittest.main()
