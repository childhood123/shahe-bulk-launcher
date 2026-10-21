import ast
import json
from pathlib import Path
from types import SimpleNamespace

source = Path('shahe_bulk_launcher/features/app_paths.py')
tree = ast.parse(source.read_text(encoding='utf-8-sig'))
cls = next(n for n in tree.body if isinstance(n, ast.ClassDef) and n.name == 'AppPathsMixin')
methods = {n.name: n for n in cls.body if isinstance(n, ast.FunctionDef)}

class Text:
    def __init__(self, text=''): self.text = text
    def GetText(self): return self.text
class Label:
    def SetLabel(self, text): self.text = text
class Clipboard:
    def __init__(self, mode): self.mode = mode; self.reads = 0
    def Open(self): return self.mode != 'access-error'
    def SetData(self, data):
        if self.mode == 'write-error': raise ValueError('write failed')
        self.value = data.text
        return True
    def Flush(self): pass
    def Close(self): pass
    def GetData(self, data):
        self.reads += 1
        if self.mode == 'read-error': raise ValueError('read failed')
        data.text = self.value if self.mode == 'match' or self.mode == 'retry' and self.reads == 3 else 'different'
        return True

for mode, expected, reads in [
    ('match', '路径已复制到剪贴板', 1), ('retry', '路径已复制到剪贴板', 3),
    ('mismatch', '复制路径失败', 5), ('read-error', '复制路径失败', 5),
    ('write-error', '复制路径失败', 0), ('access-error', '无法访问剪贴板', 0)]:
    clipboard = Clipboard(mode)
    namespace = {'wx': SimpleNamespace(TheClipboard=clipboard, TextDataObject=Text), 'time': SimpleNamespace(sleep=lambda _: None)}
    exec(compile(ast.Module(body=[methods['on_copy_path']], type_ignores=[]), str(source), 'exec'), namespace)
    owner = SimpleNamespace(combo_app_path=SimpleNamespace(GetValue=lambda: 'C:/app.exe', GetFirstSelected=lambda: -1),
        _resolve_current_app_path_and_note=lambda *a, **k: ('C:/app.exe', 'app'), lbl_status=Label())
    namespace['on_copy_path'](owner, None)
    assert owner.lbl_status.text == expected, (mode, owner.lbl_status.text)
    assert clipboard.reads == reads

browse = ast.get_source_segment(source.read_text(encoding='utf-8-sig'), methods['on_browse_app'])
assert '选择要在沙盒中打开的应用' in browse
assert '可执行文件 (*.exe)|*.exe' in browse
assert '所有文件' not in browse
print(json.dumps({'passed': 7, 'failed': 0, 'skipped': 0, 'uses_system_clipboard': False}, ensure_ascii=False))

for access_failure in (False, True):
    clipboard = Clipboard('access-error' if access_failure else 'read-error')
    namespace = {'wx': SimpleNamespace(TheClipboard=clipboard, FileDataObject=Text, TextDataObject=Text)}
    exec(compile(ast.Module(body=[methods['on_paste_file']], type_ignores=[]), str(source), 'exec'), namespace)
    owner = SimpleNamespace(lbl_status=Label())
    namespace['on_paste_file'](owner, None)
    assert owner.lbl_status.text == ('无法访问剪贴板' if access_failure else '粘贴文件时出错: read failed')
print(json.dumps({'additional_paste_passed': 2, 'failed': 0, 'skipped': 0}, ensure_ascii=False))
