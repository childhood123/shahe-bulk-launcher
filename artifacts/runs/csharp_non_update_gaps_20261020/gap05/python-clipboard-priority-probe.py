import ast
import json
from pathlib import Path
from types import SimpleNamespace


root = Path(__file__).resolve().parents[4]
source = root / "shahe_bulk_launcher" / "features" / "app_paths.py"
module = ast.parse(source.read_text(encoding="utf-8-sig"))
app_paths = next(
    node for node in module.body
    if isinstance(node, ast.ClassDef) and node.name == "AppPathsMixin"
)
method_node = next(
    node for node in app_paths.body
    if isinstance(node, ast.FunctionDef) and node.name == "on_paste_file"
)


class FakeFileDataObject:
    def __init__(self):
        self.filenames = []

    def GetFilenames(self):
        return list(self.filenames)


class FakeTextDataObject:
    def __init__(self):
        self.text = ""

    def GetText(self):
        return self.text


class FakeClipboard:
    def __init__(self, filenames=None, text=None, can_open=True):
        self.filenames = list(filenames or [])
        self.text = text
        self.can_open = can_open
        self.calls = []

    def Open(self):
        self.calls.append("open")
        return self.can_open

    def GetData(self, data):
        if isinstance(data, FakeFileDataObject):
            self.calls.append("files")
            data.filenames = list(self.filenames)
            return bool(self.filenames)
        self.calls.append("text")
        data.text = self.text or ""
        return self.text is not None

    def Close(self):
        self.calls.append("close")


class FakeLabel:
    def __init__(self):
        self.value = ""

    def SetLabel(self, value):
        self.value = value


class FakeOwner:
    def __init__(self, valid_values):
        self.valid_values = set(valid_values)
        self.lbl_status = FakeLabel()
        self.selected = None

    def _extract_exe_path_from_text(self, value):
        return value if value in self.valid_values else ""

    def _set_current_app_path(self, value, status_prefix):
        self.selected = (value, status_prefix)
        return True


def run_scenario(filenames=None, text=None, valid_values=(), can_open=True):
    clipboard = FakeClipboard(filenames, text, can_open)
    fake_wx = SimpleNamespace(
        TheClipboard=clipboard,
        FileDataObject=FakeFileDataObject,
        TextDataObject=FakeTextDataObject,
    )
    namespace = {"wx": fake_wx}
    extracted = ast.Module(body=[method_node], type_ignores=[])
    exec(compile(extracted, str(source), "exec"), namespace)
    owner = FakeOwner(valid_values)
    namespace["on_paste_file"](owner, None)
    return owner, clipboard.calls


owner, calls = run_scenario(["file.exe"], "text.exe", {"file.exe", "text.exe"})
assert owner.selected == ("file.exe", "已从剪贴板粘贴应用")
assert calls == ["open", "files", "close"]

owner, calls = run_scenario(["not-an-app.txt"], "text.exe", {"text.exe"})
assert owner.selected == ("text.exe", "已从剪贴板粘贴应用")
assert calls == ["open", "files", "text", "close"]

owner, calls = run_scenario(["not-an-app.txt"], "invalid", set())
assert owner.selected is None
assert owner.lbl_status.value == "剪贴板没有包含有效的数据"
assert calls == ["open", "files", "text", "close"]

owner, calls = run_scenario(can_open=False)
assert owner.selected is None
assert owner.lbl_status.value == "无法访问剪贴板"
assert calls == ["open"]

print(json.dumps({
    "passed": 4,
    "failed": 0,
    "skipped": 0,
    "source": str(source),
    "uses_system_clipboard": False,
}, ensure_ascii=False))
