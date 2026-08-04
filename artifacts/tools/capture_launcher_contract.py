import ast
import json
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "沙河批量打开器.py"
CONTRACT_OUTPUT = ROOT / "tests" / "contracts" / "launcher_api.json"
MODULES_OUTPUT = ROOT / "tests" / "contracts" / "method_modules.json"


def _target_module(method):
    name = method.name
    line = method.lineno
    if name == "__init__":
        return "app"
    if 3695 <= line <= 4085 or name in {
        "init_ui",
        "on_close",
        "_prepare_window_destroy",
        "Destroy",
    }:
        return "lifecycle"
    if 4086 <= line <= 4124 or 5108 <= line <= 5942 or 11239 <= line <= 11401:
        return "sandboxie"
    if 4125 <= line <= 4261 or 8260 <= line <= 8297 or 8873 <= line <= 9034:
        return "settings"
    if 4262 <= line <= 4492 or 4558 <= line <= 4658 or 10965 <= line <= 11238 or 11402 <= line <= 11412:
        return "app_paths"
    if 4697 <= line <= 5107 or 7643 <= line <= 7874 or 8298 <= line <= 8320 or 9035 <= line <= 9605:
        return "hotkeys"
    if 5943 <= line <= 7619:
        return "process_actions"
    if name == "on_repair_sandboxie_control_settings":
        return "sandboxie"
    if 7875 <= line <= 8259:
        return "updates"
    if 8321 <= line <= 8872:
        return "speech"
    if 9606 <= line <= 10964:
        return "sandbox_admin"
    if 11413 <= line <= 11933:
        return "scheduling"
    raise RuntimeError(f"Unassigned SandboxLauncher method: {name} at line {line}")


def main():
    tree = ast.parse(SOURCE.read_text(encoding="utf-8-sig"))
    functions = []
    classes = {}
    constants = []
    module_methods = {}

    for node in tree.body:
        if isinstance(node, (ast.FunctionDef, ast.AsyncFunctionDef)):
            functions.append(node.name)
        elif isinstance(node, ast.ClassDef):
            methods = [
                item
                for item in node.body
                if isinstance(item, (ast.FunctionDef, ast.AsyncFunctionDef))
            ]
            classes[node.name] = [item.name for item in methods]
            if node.name == "SandboxLauncher":
                for method in methods:
                    module_methods.setdefault(_target_module(method), []).append(method.name)
        elif isinstance(node, (ast.Assign, ast.AnnAssign)):
            targets = node.targets if isinstance(node, ast.Assign) else [node.target]
            constants.extend(
                target.id
                for target in targets
                if isinstance(target, ast.Name) and target.id.isupper()
            )

    if len(classes.get("SandboxLauncher", [])) != 400:
        raise RuntimeError("SandboxLauncher baseline method count changed")

    CONTRACT_OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    CONTRACT_OUTPUT.write_text(
        json.dumps(
            {"functions": functions, "classes": classes, "constants": constants},
            ensure_ascii=False,
            indent=2,
        ) + "\n",
        encoding="utf-8",
    )
    MODULES_OUTPUT.write_text(
        json.dumps(module_methods, ensure_ascii=False, indent=2) + "\n",
        encoding="utf-8",
    )


if __name__ == "__main__":
    main()
