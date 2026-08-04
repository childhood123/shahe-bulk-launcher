import importlib.util
from pathlib import Path

TARGET = Path(__file__).resolve().parents[2] / "沙河批量打开器.py"
spec = importlib.util.spec_from_file_location("shahe_dialog_names_probe_target", TARGET)
mod = importlib.util.module_from_spec(spec)
spec.loader.exec_module(mod)
wx = mod.wx


class ProbeFrame(mod.SandboxLauncher):
    def first_time_scan_sandboxie(self): pass
    def _refresh_sandbox_list_on_startup(self): pass
    def _auto_check_update_after_startup(self): pass
    def _apply_auto_run(self): pass
    def create_task_bar_icon(self): pass
    def _load_speech_config(self): pass

app = wx.App(False)
frame = ProbeFrame()
try:
    defaults = frame._get_default_settings()
    settings = mod.SettingsDialog(frame, {}, lambda data: None, default_settings=defaults)
    assert settings.spin_interval.GetName() == '打开沙盒间隔时间'
    assert settings.spin_launch_concurrency.GetName() == '启动并发数量限制'
    settings.Destroy()

    memory = mod.PathMemoryManagerDialog(frame, [], {'max_history_items':10,'hide_path':False}, lambda a,b: None)
    assert memory.spin_history.GetName() == '路径记忆数量'
    assert memory.history_list.GetName() == '已记忆的路径列表'
    memory.Destroy()

    group = mod.GroupNameDialog(frame)
    assert group.text_name.GetName() == '分组名称'
    group.Destroy()

    batch = mod.BatchNameRuleDialog(frame, '批量创建沙盒用户', '请输入批量创建规则：', digit_choice_values=[(0,'不加0'),(2,'前面加0（01）')])
    assert batch.text_prefix.GetName() == '名称前缀'
    assert batch.spin_start.GetName() == '起始编号'
    assert batch.choice_digits.GetName() == '编号格式'
    assert batch.spin_count.GetName() == '创建数量'
    batch.Destroy()
finally:
    frame.force_exit_requested = True
    frame.Destroy()
    app.Destroy()
print('PASS dialog_control_names_probe')
