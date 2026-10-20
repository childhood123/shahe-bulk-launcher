import ast
import json
import os
from pathlib import Path


root = Path(__file__).resolve().parents[4]
source = root / "shahe_bulk_launcher" / "features" / "sandbox_admin.py"
module = ast.parse(source.read_text(encoding="utf-8-sig"))
original = next(node for node in module.body if isinstance(node, ast.ClassDef) and node.name == "SandboxAdminMixin")
names = {"_normalize_app_history", "_normalize_sandbox_groups", "_normalize_sandbox_name_list", "_normalize_volume_memory"}
functions = ast.Module(body=[node for node in original.body if isinstance(node, ast.FunctionDef) and node.name in names], type_ignores=[])
namespace = {"os": os}
exec(compile(functions, str(source), "exec"), namespace)
normalizers = type("PythonOriginalNormalizers", (), {name: namespace[name] for name in names})()
normalizers._get_volume_memory_key = lambda name: str(name).strip()
settings = json.loads(json.dumps({
    "app_history": [{"path": r"C:\隔离测试\应用.exe", "note": "工作应用"}],
    "sandbox_groups": {"常用": ["用户A", "用户B"]},
    "pinned_sandboxes": ["用户B"],
    "sandbox_volume_memory": {"用户A": 35},
    "hotkeys": {"launch_selected": "Ctrl+Shift+F8"},
    "speech_assist_hints": {"enabled": False, "mute_current": "自定义静音提示"},
}, ensure_ascii=False))
assert normalizers._normalize_app_history(settings["app_history"], 10) == settings["app_history"]
assert normalizers._normalize_sandbox_groups(settings["sandbox_groups"]) == settings["sandbox_groups"]
assert normalizers._normalize_sandbox_name_list(settings["pinned_sandboxes"]) == settings["pinned_sandboxes"]
assert normalizers._normalize_volume_memory(settings["sandbox_volume_memory"]) == settings["sandbox_volume_memory"]
assert json.loads(json.dumps(settings)) == settings
print(json.dumps({"passed": 5, "failed": 0, "skipped": 0, "source": str(source)}, ensure_ascii=False))
