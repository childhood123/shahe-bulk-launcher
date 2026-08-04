import importlib.util
from pathlib import Path


TARGET = Path(__file__).resolve().parents[2] / "沙河批量打开器.py"
spec = importlib.util.spec_from_file_location("shahe_ui_broad_probe_target", TARGET)
mod = importlib.util.module_from_spec(spec)
spec.loader.exec_module(mod)
wx = mod.wx


class ProbeFrame(mod.SandboxLauncher):
    def first_time_scan_sandboxie(self):
        pass

    def _refresh_sandbox_list_on_startup(self):
        pass

    def _auto_check_update_after_startup(self):
        pass

    def _apply_auto_run(self):
        pass

    def create_task_bar_icon(self):
        pass

    def _load_speech_config(self):
        pass


def collect_menu_labels(menu):
    labels = []
    for item in menu.GetMenuItems():
        if item.IsSeparator():
            continue
        labels.append(item.GetItemLabel())
        submenu = item.GetSubMenu()
        if submenu:
            labels.extend(collect_menu_labels(submenu))
    return labels


app = wx.App(False)
frame = ProbeFrame()
messages = []
old_message_box = wx.MessageBox
wx.MessageBox = lambda message, caption="", style=0, parent=None: messages.append(
    (caption, message)
) or wx.OK
try:
    frame.hotkeys_enabled = False
    frame._apply_hotkeys()
    frame.create_menu_bar()
    menu_labels = []
    menubar = frame.GetMenuBar()
    for index in range(menubar.GetMenuCount()):
        menu_labels.extend(collect_menu_labels(menubar.GetMenu(index)))
    joined = "\n".join(menu_labels)
    for expected in (
        "选择应用\tCtrl+O",
        "粘贴 .exe 或文件路径\tCtrl+V",
        "刷新沙盒用户\tF5",
        "全选\tCtrl+A",
        "退出软件\tAlt+F4",
    ):
        assert expected in joined, expected
    assert "检查更新\t" not in joined
    assert "微信打赏\t" not in joined
    assert "支付宝打赏\t" not in joined

    assert not hasattr(frame, "btn_paste_app")
    assert frame.combo_app_path.GetName() == "准备要在沙盒中打开的应用"
    assert frame.sandbox_list.GetName() == "沙盒用户"
    assert frame.btn_launch.GetLabelText() == "启动选中沙盒"

    dialog = mod.HotkeySettingsDialog(
        frame,
        frame.hotkey_action_specs,
        frame.hotkeys,
        frame.default_hotkeys,
        frame._parse_hotkey_text,
        frame._format_hotkey_text,
        hotkeys_enabled=True,
        reserved_hotkeys=frame._get_reserved_hotkey_specs(),
    )
    actions = [spec["action"] for spec in frame.hotkey_action_specs]
    assert "select_all" not in actions
    assert "select_none" not in actions
    assert "show_wechat_qr" not in actions
    assert "show_alipay_qr" not in actions
    assert "check_update" not in actions
    assert actions.index("mute_current") < actions.index("mute_all")
    defaults = {spec["action"]: spec["default"] for spec in frame.hotkey_action_specs}
    assert defaults["locate_current_process"] == "Ctrl+L"
    assert defaults["launch_selected"] == "Ctrl+Enter"
    dialog.working_hotkeys = {spec["action"]: "" for spec in frame.hotkey_action_specs}
    dialog.working_hotkeys["settings"] = "Ctrl+V"
    dialog.chk_hotkeys_enabled.SetValue(True)
    dialog.on_ok(None)
    assert messages and "固定基础快捷键" in messages[-1][1]
    dialog.Destroy()

    batch_dialog = mod.BatchNameRuleDialog(
        frame,
        title="批量创建沙盒用户",
        prompt="请输入批量创建规则：",
        default_prefix="测试用户",
        default_start=1,
        default_digits=0,
        default_count=1,
        fixed_count=None,
        digit_choice_values=[
            (0, "不加0"),
            (2, "前面加0（01）"),
        ],
    )
    assert hasattr(batch_dialog, "choice_digits")
    choices = [
        batch_dialog.choice_digits.GetString(i)
        for i in range(batch_dialog.choice_digits.GetCount())
    ]
    assert choices and "不加0" in choices[0]
    assert any("加0" in choice for choice in choices[1:])
    batch_dialog.Destroy()
    print("PASS ui_broad_probe")
finally:
    wx.MessageBox = old_message_box
    frame.force_exit_requested = True
    frame.Destroy()
    app.Destroy()
