import importlib.util
from pathlib import Path


TARGET = Path(__file__).resolve().parents[2] / "沙河批量打开器.py"
spec = importlib.util.spec_from_file_location("shahe_sandbox_admin_probe_target", TARGET)
mod = importlib.util.module_from_spec(spec)
spec.loader.exec_module(mod)

frame = mod.SandboxLauncher.__new__(mod.SandboxLauncher)
names = frame._build_batch_sandbox_names("用户", 1, 3, 2)
assert names == ["用户01", "用户02", "用户03"], names
assert frame._validate_batch_sandbox_names(names) == []
errors = frame._validate_batch_sandbox_names(["User", "user", ""])
assert any("名称重复" in item for item in errors), errors
assert any("不能为空" in item for item in errors), errors

source = "GlobalSettings=y\r\n\r\n[BoxA]\r\nEnabled=y\r\nConfigLevel=8\r\n\r\n[BoxB]\r\nEnabled=n\r\n"
preamble, sections = frame._split_ini_sections(source)
assert preamble == "GlobalSettings=y\r\n\r\n", repr(preamble)
assert [name for name, _block in sections] == ["BoxA", "BoxB"], sections

renamed = frame._replace_section_header_name(sections[0][1], "BoxRenamed")
assert renamed.startswith("[BoxRenamed]\r\n"), repr(renamed)
updated, changed = frame._set_ini_setting_in_block(
    renamed,
    "ConfigLevel",
    "9",
    "\r\n",
)
assert changed is True
assert "ConfigLevel=9\r\n" in updated, repr(updated)
removed, changed = frame._set_ini_setting_in_block(updated, "Enabled", None, "\r\n")
assert changed is True
assert "Enabled=" not in removed, repr(removed)
assert frame._build_ini_section_block("BoxC", "\r\n") == (
    "[BoxC]\r\nEnabled=y\r\nConfigLevel=9\r\n\r\n"
)
print("PASS sandbox_admin_transform_probe")
