import ast
import json
from pathlib import Path

source = Path('shahe_bulk_launcher/features/hotkeys.py').read_text(encoding='utf-8-sig')
tree = ast.parse(source)
method = next(node for node in ast.walk(tree) if isinstance(node, ast.FunctionDef) and node.name == 'create_menu_bar')
expected = {
    'user_submenu': ['置顶/取消置顶当前用户', '-', '批量创建沙盒用户', '批量重命名沙盒用户', '批量删除沙盒用户'],
    'settings_menu': ['热键设置', '朗读设置', '软件设置'],
    'correct_menu': ['手动校正 Sandboxie 安装路径', '选择沙盒用户数据目录', '-', '修复 Sandboxie Control 用户设置'],
    'about_menu': ['检查更新', '-', '请作者吃辣条'],
}
actual = {name: [] for name in expected}
for call in sorted((node for node in ast.walk(method) if isinstance(node, ast.Call)), key=lambda node: (node.lineno, node.col_offset)):
    member = call.func
    if not isinstance(member, ast.Attribute) or not isinstance(member.value, ast.Name) or member.value.id not in expected:
        continue
    if member.attr == 'AppendSeparator':
        actual[member.value.id].append('-')
    elif member.attr in ('Append', 'AppendSubMenu'):
        label = call.args[1]
        if isinstance(label, ast.Call):
            label = label.args[0]
        actual[member.value.id].append(ast.literal_eval(label))
assert actual == expected, (actual, expected)
print(json.dumps({'passed': len(expected), 'failed': 0, 'skipped': 0, 'menus': actual}, ensure_ascii=False, indent=2))
