import ast
import json
from pathlib import Path

source = Path('shahe_bulk_launcher/features/settings.py').read_text(encoding='utf-8-sig')
tree = ast.parse(source)
method = next(node for node in ast.walk(tree) if isinstance(node, ast.FunctionDef) and node.name == 'on_settings_saved')
assignments = [node.lineno for node in ast.walk(method) if isinstance(node, ast.Assign) and any(isinstance(target, ast.Attribute) and isinstance(target.value, ast.Name) and target.value.id == 'self' and target.attr == 'auto_run' for target in node.targets)]
calls = [node.lineno for node in ast.walk(method) if isinstance(node, ast.Call) and isinstance(node.func, ast.Attribute) and node.func.attr == '_apply_auto_run']
assert assignments and calls and max(assignments) < min(calls), 'Python must assign the new auto_run before applying it'
print(json.dumps({'passed': 1, 'failed': 0, 'skipped': 0, 'assignment_lines': assignments, 'apply_lines': calls, 'behavior': 'new auto_run is assigned before native auto-run application'}, ensure_ascii=False, indent=2))
