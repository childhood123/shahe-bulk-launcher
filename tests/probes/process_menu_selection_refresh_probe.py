import importlib.util
from pathlib import Path


TARGET = Path(__file__).resolve().parents[2] / "沙河批量打开器.py"
spec = importlib.util.spec_from_file_location("shahe_process_menu_selection_probe_target", TARGET)
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


def select_row(frame, index):
    current = frame.sandbox_list.GetFirstSelected()
    if current != -1 and current != index:
        frame.sandbox_list.Select(current, 0)
        wx.Yield()
    frame.sandbox_list.Select(index)
    frame.sandbox_list.Focus(index)
    frame.sandbox_list.EnsureVisible(index)
    wx.Yield()


app = wx.App(False)
frame = ProbeFrame()
try:
    frame.sandbox_list.DeleteAllItems()
    row_a = frame.sandbox_list.InsertItem(
        frame.sandbox_list.GetItemCount(),
        frame.sandbox_list.format_sandbox_item_text("用户A", True),
    )
    row_b = frame.sandbox_list.InsertItem(
        frame.sandbox_list.GetItemCount(),
        frame.sandbox_list.format_sandbox_item_text("用户B", True),
    )

    sandbox_pids = {"用户A": [101], "用户B": [202]}
    mute_states = {(101,): True, (202,): False}
    window_states = {(101,): True, (202,): False}
    frame._get_sandbox_process_map_via_api = lambda: {
        name.lower(): list(pids) for name, pids in sandbox_pids.items()
    }
    frame._get_target_pids_for_sandbox = lambda name: list(
        sandbox_pids.get(str(name or "").strip(), [])
    )
    frame._get_target_pids_for_all = lambda: [101, 202]
    frame._get_mute_state_for_pids = lambda pids: mute_states.get(tuple(pids))
    frame._find_window_for_pids = lambda pids: 1 if window_states.get(tuple(pids), False) else 0
    frame._list_output_audio_devices = lambda: [{"id": "1", "name": "Speaker"}]

    select_row(frame, row_a)
    token_a = int(frame._process_menu_live_state_refresh_token or 0)
    assert token_a > 0, token_a
    frame._process_menu_live_state_refresh_worker(token_a)
    wx.Yield()
    live_state_a = frame._get_process_menu_live_state()
    assert live_state_a["selected_mute_state"] is True, live_state_a
    assert live_state_a["selected_has_window"] is True, live_state_a

    select_row(frame, row_b)
    token_b = int(frame._process_menu_live_state_refresh_token or 0)
    assert token_b > token_a, (token_a, token_b)
    stale_state = frame._get_process_menu_live_state()
    assert stale_state["selected_mute_state"] is None, stale_state
    assert stale_state["selected_has_window"] is False, stale_state

    frame._process_menu_live_state_refresh_worker(token_b)
    wx.Yield()
    live_state_b = frame._get_process_menu_live_state()
    assert live_state_b["selected_mute_state"] is False, live_state_b
    assert live_state_b["selected_audio_session"] is True, live_state_b
    assert live_state_b["selected_has_window"] is False, live_state_b
    print("PASS process_menu_selection_refresh_probe")
finally:
    frame.force_exit_requested = True
    frame.Destroy()
    app.Destroy()
