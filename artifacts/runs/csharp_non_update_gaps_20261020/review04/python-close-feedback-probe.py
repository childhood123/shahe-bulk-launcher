import ast
from pathlib import Path
from types import SimpleNamespace

source = Path('shahe_bulk_launcher/features/process_actions.py')
tree = ast.parse(source.read_text(encoding='utf-8-sig'))
names = ('_finish_close_current_sandbox', '_finish_close_all_sandboxes')
methods = [n for n in ast.walk(tree) if isinstance(n, ast.FunctionDef) and n.name in names]
scope = {}
exec(compile(ast.Module(body=methods, type_ignores=[]), str(source), 'exec'), scope)
cases = 0
for all_users in (False, True):
    for status, closed, speaks, refresh in (
        ('all_closed', 2, True, True), ('no_target', 0, False, False),
        ('partially_closed', 1, False, True), ('closed_some', 1, True, True),
        ('none_closed', 0, False, True),
    ):
        spoken, refreshed, labels = [], [], []
        owner = SimpleNamespace(
            lbl_status=SimpleNamespace(SetLabel=labels.append),
            _speak_process_assist=lambda text, event_key: spoken.append((text, event_key)),
            _refresh_process_ui_after_action=lambda refresh_list: refreshed.append(refresh_list),
        )
        if all_users:
            scope[names[1]](owner, status, closed, status == 'partially_closed')
        else:
            scope[names[0]](owner, '用户一', dict(status=status, closed=closed))
        assert refreshed == [refresh], (all_users, status, refreshed)
        expected = [('已关闭全部进程', 'close_all') if all_users else ('已关闭用户一进程', 'close_current')] if speaks else []
        assert spoken == expected, (all_users, status, spoken)
        assert len(labels) == 1
        cases += 1
print(f'PASS: {cases} Python production feedback cases; failed=0 skipped=0')
