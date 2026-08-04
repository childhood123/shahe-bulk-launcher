import importlib.util
import threading
import time
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
TARGET = ROOT / "沙河批量打开器.py"
spec = importlib.util.spec_from_file_location("shahe_audio_process_probe_target", TARGET)
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


def wait_until(predicate, timeout=2.0):
    deadline = time.time() + timeout
    while time.time() < deadline:
        try:
            if predicate():
                return True
        except Exception:
            pass
        try:
            wx.Yield()
        except Exception:
            pass
        time.sleep(0.01)
    return False


app = wx.App(False)
frame = ProbeFrame()
try:
    main_thread_id = threading.get_ident()
    calls = {
        "mute_state": [],
        "mute_set": [],
        "output_set": [],
    }

    frame._get_sandbox_internal_and_display_name = lambda name: (name, name)
    frame._get_target_pids_for_sandbox = lambda name: [1234]
    frame._get_target_pids_for_all = lambda: [1234]
    frame._refresh_process_ui_after_action = lambda *args, **kwargs: None
    frame._speak_process_assist = lambda *args, **kwargs: None
    frame.create_menu_bar = lambda *args, **kwargs: None

    def fake_mute_state(pids):
        calls["mute_state"].append(threading.get_ident())
        return False

    def fake_set_mute(pids, mute):
        calls["mute_set"].append(threading.get_ident())
        return {
            "changed_sessions": 1,
            "changed_pids": {1234},
            "matched_pids": {1234},
            "failed_pids": set(),
        }

    device = {"id": "dev1", "name": "扬声器", "label": "扬声器", "is_default": True}
    frame._get_mute_state_for_pids = fake_mute_state
    frame._set_mute_for_pids_detailed = fake_set_mute
    frame._list_output_audio_devices = lambda: [device]
    frame._prompt_output_device_choice = lambda title: device
    frame._choose_output_device_from_devices = lambda title, devices: device

    def fake_set_output_device(pids, device_id):
        calls["output_set"].append(threading.get_ident())
        return 1, 0

    frame._set_output_device_for_pids = fake_set_output_device

    frame.on_mute_current_sandbox("测试用户")
    assert wait_until(lambda: len(calls["mute_set"]) >= 1), calls
    assert calls["mute_state"][0] != main_thread_id, calls
    assert calls["mute_set"][0] != main_thread_id, calls
    assert wait_until(lambda: not frame._volume_task_lock.locked()), "mute task lock still held"

    frame.on_mute_all_sandboxes()
    assert wait_until(lambda: len(calls["mute_set"]) >= 2), calls
    assert calls["mute_state"][1] != main_thread_id, calls
    assert calls["mute_set"][1] != main_thread_id, calls
    assert wait_until(lambda: not frame._volume_task_lock.locked()), "mute all task lock still held"

    frame.on_switch_output_device_current_sandbox("测试用户")
    assert wait_until(lambda: len(calls["output_set"]) >= 1), calls
    assert calls["output_set"][0] != main_thread_id, calls
    assert wait_until(lambda: not frame._volume_task_lock.locked()), "switch current task lock still held"

    frame.on_switch_output_device_all_sandboxes()
    assert wait_until(lambda: len(calls["output_set"]) >= 2), calls
    assert calls["output_set"][1] != main_thread_id, calls
    print("PASS audio_process_actions_no_blocking_probe")
finally:
    frame.force_exit_requested = True
    frame.Destroy()
    app.Destroy()
