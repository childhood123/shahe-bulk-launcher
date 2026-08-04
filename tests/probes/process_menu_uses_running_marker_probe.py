import importlib.util
from pathlib import Path


TARGET = Path(__file__).resolve().parents[2] / "沙河批量打开器.py"
spec = importlib.util.spec_from_file_location("shahe_process_menu_running_probe_target", TARGET)
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


app = wx.App(False)
frame = ProbeFrame()
try:
    frame.sandbox_list.DeleteAllItems()
    row = frame.sandbox_list.InsertItem(
        frame.sandbox_list.GetItemCount(),
        frame.sandbox_list.format_sandbox_item_text("测试用户", True),
    )
    frame.sandbox_list.Select(row)
    frame.sandbox_list.Focus(row)
    frame.sandbox_list.context_item_index = row

    frame._process_menu_live_state_cache = {}
    state = frame._get_process_menu_states()
    assert state["selected_has_process"] is True, state
    assert state["all_has_process"] is True, state

    enabled = []
    original_menu_label_with_state = frame._menu_label_with_state

    def capture_menu_label(label, action, enabled_flag):
        enabled.append((action, bool(enabled_flag)))
        return original_menu_label_with_state(label, action, enabled_flag)

    frame._menu_label_with_state = capture_menu_label
    frame.create_menu_bar()
    expected_enabled = {
        "mute_current",
        "mute_all",
        "volume_current_up",
        "volume_current_down",
        "volume_all_up",
        "volume_all_down",
        "switch_output_current",
        "switch_output_all",
        "close_current_process",
        "close_all_processes",
        "locate_current_process",
    }
    actual = {action for action, enabled_flag in enabled if enabled_flag}
    assert not (expected_enabled - actual), (expected_enabled - actual, enabled)
    print("PASS process_menu_uses_running_marker_probe")
finally:
    frame.force_exit_requested = True
    frame.Destroy()
    app.Destroy()
