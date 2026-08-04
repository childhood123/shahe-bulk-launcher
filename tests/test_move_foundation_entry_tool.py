import importlib.util
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
TOOL = ROOT / "artifacts" / "tools" / "move_foundation_entry.py"


class MoveFoundationEntryToolTests(unittest.TestCase):
    def test_generated_import_block_has_no_patch_markers(self):
        spec = importlib.util.spec_from_file_location("move_foundation_entry", TOOL)
        module = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(module)

        bad_lines = [line for line in module.IMPORT_BLOCK.splitlines() if line.startswith("+")]
        self.assertEqual([], bad_lines)


if __name__ == "__main__":
    unittest.main()
