import ast
import json
from pathlib import Path


root = Path(__file__).resolve().parents[4]
controls_source = root / "shahe_bulk_launcher" / "ui" / "controls.py"
hotkeys_source = root / "shahe_bulk_launcher" / "features" / "hotkeys.py"


def class_node(source, name):
    module = ast.parse(source.read_text(encoding="utf-8-sig"))
    return next(
        node for node in module.body
        if isinstance(node, ast.ClassDef) and node.name == name
    )


def extract_methods(source, owner, names):
    methods = ast.Module(
        body=[
            node for node in owner.body
            if isinstance(node, ast.FunctionDef) and node.name in names
        ],
        type_ignores=[],
    )
    namespace = {}
    exec(compile(methods, str(source), "exec"), namespace)
    return namespace


controls_class = class_node(controls_source, "CheckableListCtrl")
controls = extract_methods(
    controls_source,
    controls_class,
    {"toggle_select_all", "on_select_all"},
)


class FakeList:
    toggle_select_all = controls["toggle_select_all"]
    on_select_all = controls["on_select_all"]

    def __init__(self, checked, special=()):
        self.checked = list(checked)
        self.special = set(special)
        self.calls = []

    def GetItemCount(self):
        return len(self.checked)

    def _is_special_index(self, index):
        return index in self.special

    def IsItemChecked(self, index):
        return self.checked[index]

    def check_all(self):
        self.calls.append("check")
        for index in range(len(self.checked)):
            if index not in self.special:
                self.checked[index] = True

    def uncheck_all(self):
        self.calls.append("uncheck")
        for index in range(len(self.checked)):
            if index not in self.special:
                self.checked[index] = False


values = FakeList([True, False, False], special={1})
values.toggle_select_all()
assert values.checked == [True, False, True]
assert values.calls == ["check"]

values.toggle_select_all()
assert values.checked == [False, False, False]
assert values.calls == ["check", "uncheck"]

empty = FakeList([])
empty.toggle_select_all()
assert empty.calls == []

context = FakeList([True, True])
context.on_select_all(None)
context.on_select_all(None)
assert context.checked == [True, True]
assert context.calls == ["check", "check"]

hotkeys_class = class_node(hotkeys_source, "HotkeysMixin")
hotkeys = extract_methods(hotkeys_source, hotkeys_class, {"on_ctrl_a_select"})


class ToggleCounter:
    def __init__(self):
        self.calls = 0

    def toggle_select_all(self):
        self.calls += 1


counter = ToggleCounter()
host = type("HotkeyHost", (), {"sandbox_list": counter})()
hotkeys["on_ctrl_a_select"](host, None)
assert counter.calls == 1

create_menu_bar = next(
    node for node in hotkeys_class.body
    if isinstance(node, ast.FunctionDef) and node.name == "create_menu_bar"
)
assert any(
    isinstance(node, ast.Call)
    and isinstance(node.func, ast.Attribute)
    and node.func.attr == "Bind"
    and len(node.args) >= 3
    and isinstance(node.args[1], ast.Attribute)
    and node.args[1].attr == "on_ctrl_a_select"
    and isinstance(node.args[2], ast.Name)
    and node.args[2].id == "select_all_item"
    for node in ast.walk(create_menu_bar)
)

print(json.dumps({
    "passed": 6,
    "failed": 0,
    "skipped": 0,
    "controls_source": str(controls_source),
    "hotkeys_source": str(hotkeys_source),
    "uses_gui": False,
}, ensure_ascii=False))
