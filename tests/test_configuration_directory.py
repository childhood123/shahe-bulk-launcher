import os
import tempfile
import unittest
from pathlib import Path
from shahe_bulk_launcher.common import get_configuration_dir, prepare_configuration_dir


class ConfigurationDirectoryTests(unittest.TestCase):
    def test_empty_published_directory_migrates_old_settings_without_changing_bytes(self):
        with tempfile.TemporaryDirectory() as root:
            old = Path(root, "Configuration")
            current = Path(root, "Configurations")
            old.mkdir()
            current.mkdir()
            data = b"[settings]\nmax_history_items=39\n"
            (old / "shahe_config.ini").write_bytes(data)
            self.assertEqual(str(current), prepare_configuration_dir(root))
            self.assertEqual(data, (current / "shahe_config.ini").read_bytes())
            self.assertFalse(old.exists())

    def test_new_configuration_wins_and_fresh_directory_is_created_empty(self):
        with tempfile.TemporaryDirectory() as root:
            current = Path(prepare_configuration_dir(root))
            self.assertEqual(os.path.join(root, "Configurations"), get_configuration_dir(root))
            self.assertEqual([], list(current.iterdir()))
            (current / "shahe_config.ini").write_text("new", encoding="utf-8")
            old = Path(root, "Configuration")
            old.mkdir()
            (old / "shahe_config.ini").write_text("old", encoding="utf-8")
            prepare_configuration_dir(root)
            self.assertEqual("new", (current / "shahe_config.ini").read_text(encoding="utf-8"))
            self.assertEqual("old", (old / "shahe_config.ini").read_text(encoding="utf-8"))
