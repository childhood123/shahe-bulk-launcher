import ctypes
import os
import subprocess
import threading
import time
from ctypes import wintypes

import wx

from ..constants import is_valid_internal_sandbox_name

pythoncom = None
AudioUtilities = None
AudioRouter = None


class ProcessActionsMixin:
    def _ensure_audio_backend(self):
        global pythoncom, AudioUtilities
        if pythoncom is None:
            try:
                import pythoncom as _pythoncom
                pythoncom = _pythoncom
            except Exception:
                pythoncom = None
        if AudioUtilities is None:
            try:
                from pycaw.pycaw import AudioUtilities as _AudioUtilities
                AudioUtilities = _AudioUtilities
            except Exception:
                AudioUtilities = None
        return AudioUtilities is not None

    def _ensure_audio_router_backend(self):
        global AudioRouter
        if AudioRouter is None:
            try:
                import winappaudiorouter as _audio_router
                AudioRouter = _audio_router
            except Exception:
                AudioRouter = False
        return bool(AudioRouter)

    def _list_output_audio_devices(self):
        if not self._ensure_audio_router_backend():
            return []
        try:
            devices = AudioRouter.list_output_devices()
        except Exception:
            return []
        result = []
        for dev in devices or []:
            try:
                dev_id = str(getattr(dev, "id", "") or "").strip()
                dev_name = str(getattr(dev, "name", "") or "").strip()
                is_default = bool(getattr(dev, "is_default", False))
            except Exception:
                continue
            if not dev_id:
                continue
            label = dev_name or dev_id
            if is_default:
                label = f"{label}（默认）"
            result.append({
                "id": dev_id,
                "name": dev_name or dev_id,
                "label": label,
                "is_default": is_default
            })
        return result

    def _prompt_output_device_choice(self, title):
        devices = self._list_output_audio_devices()
        if not devices:
            self.lbl_status.SetLabel("未找到可用的播放设备")
            return None
        return self._choose_output_device_from_devices(title, devices)

    def _choose_output_device_from_devices(self, title, devices):
        choices = [item["label"] for item in devices]
        dlg = wx.SingleChoiceDialog(self, "请选择播放设备", title, choices)
        try:
            if dlg.ShowModal() != wx.ID_OK:
                return None
            idx = int(dlg.GetSelection())
        finally:
            dlg.Destroy()
        if idx < 0 or idx >= len(devices):
            return None
        return devices[idx]

    def _set_output_device_for_pids(self, pids, device_id):
        if not self._ensure_audio_router_backend():
            return 0, 0
        pid_list = sorted(self._normalize_pid_set(pids))
        if not pid_list:
            return 0, 0
        changed = 0
        failed = 0
        for pid in pid_list:
            try:
                result = AudioRouter.set_app_output_device(process_id=int(pid), device=str(device_id))
                if isinstance(result, dict) and int(pid) in result:
                    changed += 1
                elif result:
                    changed += 1
                else:
                    failed += 1
            except Exception:
                failed += 1
        return changed, failed

    def _get_target_pids_for_sandbox(self, sandbox_name):
        if not sandbox_name:
            return []
        return sorted(set(self._get_sandbox_pids(sandbox_name)))

    def _find_window_for_pids(self, pids):
        pid_set = self._normalize_pid_set(pids)
        if not pid_set:
            return 0
        try:
            user32 = ctypes.windll.user32
        except Exception:
            return 0

        GW_OWNER = 4
        ownerless_candidates = []
        owned_candidates = []
        enum_windows_proc = ctypes.WINFUNCTYPE(ctypes.c_bool, wintypes.HWND, wintypes.LPARAM)

        def _enum_proc(hwnd, lparam):
            try:
                if not user32.IsWindowVisible(hwnd):
                    return True
                owner = user32.GetWindow(hwnd, GW_OWNER)
                pid = wintypes.DWORD()
                user32.GetWindowThreadProcessId(hwnd, ctypes.byref(pid))
                if pid.value not in pid_set:
                    return True
                try:
                    title_len = int(user32.GetWindowTextLengthW(hwnd))
                except Exception:
                    title_len = 0
                item = (hwnd, title_len)
                if owner:
                    owned_candidates.append(item)
                else:
                    ownerless_candidates.append(item)
            except Exception:
                pass
            return True

        cb = enum_windows_proc(_enum_proc)
        try:
            user32.EnumWindows(cb, 0)
        except Exception:
            return 0

        candidates_with_title = [item for item in ownerless_candidates if item[1] > 0]
        if not candidates_with_title:
            candidates_with_title = [item for item in owned_candidates if item[1] > 0]
        if candidates_with_title:
            candidates = [item[0] for item in candidates_with_title]
        elif ownerless_candidates:
            candidates = [item[0] for item in ownerless_candidates]
        elif owned_candidates:
            candidates = [item[0] for item in owned_candidates]
        else:
            return 0

        non_minimized = [hwnd for hwnd in candidates if not user32.IsIconic(hwnd)]
        if non_minimized:
            candidates = non_minimized
        foreground = user32.GetForegroundWindow()
        if foreground in candidates:
            return int(foreground)
        return int(candidates[0])

    def _get_window_title(self, hwnd):
        if not hwnd:
            return ""
        try:
            user32 = ctypes.windll.user32
            title_len = int(user32.GetWindowTextLengthW(hwnd))
            if title_len <= 0:
                return ""
            buffer = ctypes.create_unicode_buffer(title_len + 1)
            user32.GetWindowTextW(hwnd, buffer, len(buffer))
            return str(buffer.value or "").strip()
        except Exception:
            return ""

    def _find_windows_for_pids(self, pids):
        pid_set = self._normalize_pid_set(pids)
        if not pid_set:
            return []
        try:
            user32 = ctypes.windll.user32
        except Exception:
            return []

        GW_OWNER = 4
        ownerless_candidates = []
        owned_candidates = []
        enum_windows_proc = ctypes.WINFUNCTYPE(ctypes.c_bool, wintypes.HWND, wintypes.LPARAM)

        def _enum_proc(hwnd, lparam):
            try:
                if not user32.IsWindowVisible(hwnd):
                    return True
                owner = user32.GetWindow(hwnd, GW_OWNER)
                pid = wintypes.DWORD()
                user32.GetWindowThreadProcessId(hwnd, ctypes.byref(pid))
                if pid.value not in pid_set:
                    return True
                title = self._get_window_title(hwnd)
                item = {
                    "hwnd": int(hwnd),
                    "pid": int(pid.value),
                    "title": title,
                    "owner": bool(owner),
                    "minimized": bool(user32.IsIconic(hwnd)),
                }
                if owner:
                    owned_candidates.append(item)
                else:
                    ownerless_candidates.append(item)
            except Exception:
                pass
            return True

        cb = enum_windows_proc(_enum_proc)
        try:
            user32.EnumWindows(cb, 0)
        except Exception:
            return []

        candidates = [item for item in ownerless_candidates if item.get("title")]
        if not candidates:
            candidates = [item for item in owned_candidates if item.get("title")]
        if not candidates:
            candidates = ownerless_candidates or owned_candidates
        if not candidates:
            return []

        non_minimized = [item for item in candidates if not item.get("minimized")]
        if non_minimized:
            candidates = non_minimized
        foreground = int(user32.GetForegroundWindow() or 0)
        candidates.sort(key=lambda item: (0 if item.get("hwnd") == foreground else 1, item.get("pid", 0), item.get("hwnd", 0)))
        return candidates

    def _prompt_window_choice(self, sandbox_name, windows):
        options = list(windows or [])
        if not options:
            return 0
        if len(options) == 1:
            return int(options[0].get("hwnd") or 0)
        labels = []
        for index, item in enumerate(options, 1):
            title = str(item.get("title") or "").strip() or "<无标题窗口>"
            pid = int(item.get("pid") or 0)
            labels.append(f"{index}. PID {pid} - {title}")
        dlg = wx.SingleChoiceDialog(self, f"当前用户 {sandbox_name} 检测到多个打开进程，请选择要定位的进程", "定位当前打开的进程", labels)
        try:
            if dlg.ShowModal() != wx.ID_OK:
                return 0
            idx = dlg.GetSelection()
            if idx == wx.NOT_FOUND or idx < 0 or idx >= len(options):
                return 0
            return int(options[idx].get("hwnd") or 0)
        finally:
            dlg.Destroy()

    def _activate_window(self, hwnd):
        if not hwnd:
            return False
        try:
            user32 = ctypes.windll.user32
            try:
                kernel32 = ctypes.windll.kernel32
            except Exception:
                kernel32 = None
            SW_RESTORE = 9
            SW_SHOW = 5
            SWP_NOSIZE = 0x0001
            SWP_NOMOVE = 0x0002
            SWP_SHOWWINDOW = 0x0040
            HWND_TOPMOST = -1
            HWND_NOTOPMOST = -2
            VK_MENU = 0x12
            KEYEVENTF_KEYUP = 0x0002
            hwnd = int(hwnd or 0)
            if not hwnd:
                return False
            if hasattr(user32, "IsWindow") and not user32.IsWindow(hwnd):
                return False
            try:
                requires_long_stability = bool(user32.IsIconic(hwnd))
            except Exception:
                requires_long_stability = False

            def _get_foreground_hwnd():
                try:
                    return int(user32.GetForegroundWindow() or 0)
                except Exception:
                    return 0

            def _is_foreground_target(stable_checks, interval_seconds=0.1):
                checks = max(1, int(stable_checks or 1))
                for index in range(checks):
                    if _get_foreground_hwnd() != hwnd:
                        return False
                    if index + 1 < checks:
                        time.sleep(max(0.0, float(interval_seconds or 0.0)))
                return True

            def _prime_foreground_permission():
                try:
                    user32.keybd_event(VK_MENU, 0, 0, 0)
                    user32.keybd_event(VK_MENU, 0, KEYEVENTF_KEYUP, 0)
                except Exception:
                    pass

            def _bring_to_front(use_alt_prime=False):
                try:
                    was_iconic = bool(user32.IsIconic(hwnd))
                except Exception:
                    was_iconic = False
                try:
                    if was_iconic:
                        user32.ShowWindow(hwnd, SW_RESTORE)
                        time.sleep(0.25)
                    else:
                        user32.ShowWindow(hwnd, SW_SHOW)
                except Exception:
                    pass
                if use_alt_prime:
                    _prime_foreground_permission()
                attached_pairs = []
                foreground_hwnd = _get_foreground_hwnd()
                try:
                    target_thread = int(user32.GetWindowThreadProcessId(hwnd, None) or 0)
                except Exception:
                    target_thread = 0
                try:
                    foreground_thread = int(user32.GetWindowThreadProcessId(foreground_hwnd, None) or 0) if foreground_hwnd else 0
                except Exception:
                    foreground_thread = 0
                try:
                    current_thread = int(kernel32.GetCurrentThreadId() or 0) if kernel32 else 0
                except Exception:
                    current_thread = 0
                try:
                    attach_thread_input = getattr(user32, "AttachThreadInput", None)
                    if callable(attach_thread_input):
                        for pair in (
                            (foreground_thread, target_thread),
                            (foreground_thread, current_thread),
                            (target_thread, current_thread),
                        ):
                            src, dst = pair
                            if src and dst and src != dst and pair not in attached_pairs:
                                try:
                                    attach_thread_input(src, dst, True)
                                    attached_pairs.append(pair)
                                except Exception:
                                    pass
                except Exception:
                    attached_pairs = []
                try:
                    user32.BringWindowToTop(hwnd)
                    user32.SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW)
                    user32.SetWindowPos(hwnd, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW)
                    if hasattr(user32, "SetActiveWindow"):
                        user32.SetActiveWindow(hwnd)
                    if hasattr(user32, "SetFocus"):
                        user32.SetFocus(hwnd)
                    user32.SetForegroundWindow(hwnd)
                except Exception:
                    pass
                finally:
                    if attached_pairs:
                        detach_thread_input = getattr(user32, "AttachThreadInput", None)
                        if callable(detach_thread_input):
                            for src, dst in reversed(attached_pairs):
                                try:
                                    detach_thread_input(src, dst, False)
                                except Exception:
                                    pass

            if not requires_long_stability and _is_foreground_target(1, interval_seconds=0.0):
                return True

            stable_checks = 2
            deadline = time.time() + 5.0
            attempt_index = 0
            while time.time() < deadline:
                _bring_to_front(use_alt_prime=attempt_index > 0)
                time.sleep(0.1)
                if _is_foreground_target(stable_checks):
                    return True
                attempt_index += 1
            return False
        except Exception:
            return False

    def _stabilize_activated_window(self, hwnd):
        target_hwnd = int(hwnd or 0)
        if not target_hwnd or self._is_window_closing():
            return False
        for _ in range(6):
            if self._is_window_closing():
                return False
            try:
                if self._activate_window(target_hwnd):
                    if self._has_foreground_window_streak(target_hwnd, required_streak=2, total_checks=4, interval_seconds=0.08):
                        time.sleep(0.1)
                        if self._has_foreground_window_streak(target_hwnd, required_streak=2, total_checks=3, interval_seconds=0.08):
                            return True
                elif self._has_foreground_window_streak(target_hwnd, required_streak=2, total_checks=3, interval_seconds=0.08):
                    time.sleep(0.12)
                    if self._has_foreground_window_streak(target_hwnd, required_streak=2, total_checks=3, interval_seconds=0.08):
                        return True
            except Exception:
                return False
            time.sleep(0.14)
        return False

    def _start_locate_window_reinforcement(self, hwnd, keep_long_tail=False):
        target_hwnd = int(hwnd or 0)
        if not target_hwnd or self._is_window_closing():
            return
        self._cancel_locate_window_reinforcement(wait=False)
        stop_event = threading.Event()

        def _worker():
            start_time = time.monotonic()
            try:
                offsets = [0.08, 0.22, 0.42, 0.72]
                if keep_long_tail:
                    offsets.extend((1.12, 1.52, 1.92, 2.32))
                for target_offset in offsets:
                    remaining = max(0.0, float(target_offset) - (time.monotonic() - start_time))
                    if stop_event.wait(remaining):
                        return
                    if self._is_window_closing() or stop_event.is_set():
                        return
                    try:
                        user32 = ctypes.windll.user32
                        if hasattr(user32, "IsWindow") and not user32.IsWindow(target_hwnd):
                            return
                    except Exception:
                        return
                    try:
                        self._activate_window(target_hwnd)
                    except Exception:
                        return
            finally:
                try:
                    with self._locate_reinforcement_lock:
                        if getattr(self, "_locate_reinforcement_thread", None) is threading.current_thread():
                            self._locate_reinforcement_thread = None
                            self._locate_reinforcement_event = None
                except Exception:
                    pass

        thread = threading.Thread(target=_worker, name="LocateWindowReinforcement", daemon=True)
        try:
            with self._locate_reinforcement_lock:
                self._locate_reinforcement_event = stop_event
                self._locate_reinforcement_thread = thread
        except Exception:
            return
        thread.start()

    def _has_foreground_window_streak(self, hwnd, required_streak=3, total_checks=8, interval_seconds=0.1):
        target_hwnd = int(hwnd or 0)
        if not target_hwnd:
            return False
        try:
            user32 = ctypes.windll.user32
        except Exception:
            return False
        needed = max(1, int(required_streak or 1))
        checks = max(needed, int(total_checks or needed))
        streak = 0
        for index in range(checks):
            try:
                current_foreground = int(user32.GetForegroundWindow() or 0)
            except Exception:
                current_foreground = 0
            if current_foreground == target_hwnd:
                streak += 1
                if streak >= needed:
                    return True
            else:
                streak = 0
            if index + 1 < checks:
                time.sleep(max(0.0, float(interval_seconds or 0.0)))
        return False

    def _move_focus_off_target_pids(self, pids):
        pid_set = self._normalize_pid_set(pids)
        if not pid_set:
            return False
        try:
            user32 = ctypes.windll.user32
        except Exception:
            return False

        def _get_foreground_info():
            try:
                foreground_hwnd = int(user32.GetForegroundWindow() or 0)
            except Exception:
                foreground_hwnd = 0
            foreground_pid = 0
            if foreground_hwnd:
                try:
                    pid = wintypes.DWORD()
                    user32.GetWindowThreadProcessId(foreground_hwnd, ctypes.byref(pid))
                    foreground_pid = int(pid.value or 0)
                except Exception:
                    foreground_pid = 0
            return foreground_hwnd, foreground_pid

        foreground_hwnd, foreground_pid = _get_foreground_info()
        if foreground_pid not in pid_set:
            return False

        minimize_codes = (6, 11)
        minimized_hwnds = set()
        deadline = time.time() + 1.5
        while time.time() < deadline:
            current_hwnd, current_pid = _get_foreground_info()
            if current_pid not in pid_set:
                return True

            candidate_hwnds = []
            if current_hwnd:
                candidate_hwnds.append(current_hwnd)
            try:
                for item in self._find_windows_for_pids(pid_set):
                    hwnd = int(item.get("hwnd") or 0)
                    if hwnd and hwnd not in candidate_hwnds:
                        candidate_hwnds.append(hwnd)
            except Exception:
                pass

            moved_any = False
            for hwnd in candidate_hwnds:
                if not hwnd or hwnd in minimized_hwnds:
                    continue
                minimized_hwnds.add(hwnd)
                for show_code in minimize_codes:
                    try:
                        user32.ShowWindow(hwnd, show_code)
                    except Exception:
                        pass
                moved_any = True
                time.sleep(0.12)
                current_hwnd, current_pid = _get_foreground_info()
                if current_pid not in pid_set:
                    return True
            if not moved_any:
                break
        return False

    def _get_target_pids_for_all(self):
        all_pids_fast = self._get_all_sandbox_pids()
        if all_pids_fast:
            return sorted(set(all_pids_fast))

        sandbox_names = []
        try:
            sandbox_names = self.sandbox_list.scan_sandboxes()
        except Exception:
            sandbox_names = []

        all_pids = set()
        for sandbox_name in sandbox_names:
            all_pids.update(self._get_sandbox_pids(sandbox_name))
        return sorted(all_pids)

    def _normalize_pid_set(self, pids):
        pid_set = set()
        for pid in pids or []:
            try:
                value = int(pid)
            except Exception:
                continue
            if value > 0:
                pid_set.add(value)
        return pid_set

    def _get_mute_state_for_pids(self, pids):
        pid_set = self._normalize_pid_set(pids)
        if not pid_set or not self._ensure_audio_backend():
            return None
        com_initialized = False
        try:
            if pythoncom is not None:
                pythoncom.CoInitialize()
                com_initialized = True
            states = []
            for session in AudioUtilities.GetAllSessions():
                try:
                    proc = getattr(session, "Process", None)
                    pid = getattr(proc, "pid", None)
                    if pid not in pid_set:
                        continue
                    volume = getattr(session, "SimpleAudioVolume", None)
                    if volume is None:
                        continue
                    states.append(bool(volume.GetMute()))
                except Exception:
                    continue
            if not states:
                return None
            return all(states)
        except Exception:
            return None
        finally:
            if com_initialized:
                try:
                    pythoncom.CoUninitialize()
                except Exception:
                    pass

    def _set_mute_for_pids(self, pids, mute):
        result = self._set_mute_for_pids_detailed(pids, mute)
        return int(result.get("changed_sessions", 0) or 0)

    def _set_mute_for_pids_detailed(self, pids, mute):
        pid_set = self._normalize_pid_set(pids)
        if not pid_set or not self._ensure_audio_backend():
            return {
                "changed_sessions": 0,
                "changed_pids": set(),
                "matched_pids": set(),
                "failed_pids": set(),
            }
        changed_sessions = 0
        changed_pids = set()
        matched_pids = set()
        failed_pids = set()
        com_initialized = False
        try:
            if pythoncom is not None:
                pythoncom.CoInitialize()
                com_initialized = True
            for session in AudioUtilities.GetAllSessions():
                try:
                    proc = getattr(session, "Process", None)
                    pid = getattr(proc, "pid", None)
                    if pid not in pid_set:
                        continue
                    volume = getattr(session, "SimpleAudioVolume", None)
                    if volume is None:
                        continue
                    matched_pids.add(int(pid))
                    volume.SetMute(1 if mute else 0, None)
                    changed_sessions += 1
                    changed_pids.add(int(pid))
                except Exception:
                    try:
                        if pid in pid_set:
                            failed_pids.add(int(pid))
                    except Exception:
                        pass
                    continue
        except Exception:
            return {
                "changed_sessions": int(changed_sessions),
                "changed_pids": set(changed_pids),
                "matched_pids": set(matched_pids),
                "failed_pids": set(failed_pids),
            }
        finally:
            if com_initialized:
                try:
                    pythoncom.CoUninitialize()
                except Exception:
                    pass
        return {
            "changed_sessions": int(changed_sessions),
            "changed_pids": set(changed_pids),
            "matched_pids": set(matched_pids),
            "failed_pids": set(failed_pids),
        }

    def _adjust_volume_for_pids(self, pids, delta):
        result = self._adjust_volume_for_pids_detailed(pids, delta)
        return int(result.get("changed_sessions", 0) or 0), result.get("avg_level")

    def _adjust_volume_for_pids_detailed(self, pids, delta):
        pid_set = self._normalize_pid_set(pids)
        if not pid_set or not self._ensure_audio_backend():
            return {
                "changed_sessions": 0,
                "avg_level": None,
                "changed_pids": set(),
                "matched_pids": set(),
                "failed_pids": set(),
            }

        try:
            delta = float(delta)
        except Exception:
            return {
                "changed_sessions": 0,
                "avg_level": None,
                "changed_pids": set(),
                "matched_pids": set(),
                "failed_pids": set(),
            }
        if delta == 0:
            return {
                "changed_sessions": 0,
                "avg_level": None,
                "changed_pids": set(),
                "matched_pids": set(),
                "failed_pids": set(),
            }

        changed = 0
        level_values = []
        changed_pids = set()
        matched_pids = set()
        failed_pids = set()
        com_initialized = False
        try:
            if pythoncom is not None:
                pythoncom.CoInitialize()
                com_initialized = True
            for session in AudioUtilities.GetAllSessions():
                try:
                    proc = getattr(session, "Process", None)
                    pid = getattr(proc, "pid", None)
                    if pid not in pid_set:
                        continue
                    volume = getattr(session, "SimpleAudioVolume", None)
                    if volume is None:
                        continue
                    matched_pids.add(int(pid))
                    current = float(volume.GetMasterVolume())
                    target = max(0.0, min(1.0, current + delta))
                    volume.SetMasterVolume(target, None)
                    level_values.append(target)
                    changed += 1
                    changed_pids.add(int(pid))
                except Exception:
                    try:
                        if pid in pid_set:
                            failed_pids.add(int(pid))
                    except Exception:
                        pass
                    continue
        except Exception:
            return {
                "changed_sessions": int(changed),
                "avg_level": (int(round((sum(level_values) / len(level_values)) * 100)) if level_values else None),
                "changed_pids": set(changed_pids),
                "matched_pids": set(matched_pids),
                "failed_pids": set(failed_pids),
            }
        finally:
            if com_initialized:
                try:
                    pythoncom.CoUninitialize()
                except Exception:
                    pass

        if not level_values:
            return {
                "changed_sessions": 0,
                "avg_level": None,
                "changed_pids": set(changed_pids),
                "matched_pids": set(matched_pids),
                "failed_pids": set(failed_pids),
            }
        avg_level = int(round((sum(level_values) / len(level_values)) * 100))
        return {
            "changed_sessions": int(changed),
            "avg_level": avg_level,
            "changed_pids": set(changed_pids),
            "matched_pids": set(matched_pids),
            "failed_pids": set(failed_pids),
        }

    def _set_volume_for_pids(self, pids, level_percent):
        pid_set = self._normalize_pid_set(pids)
        if not pid_set or not self._ensure_audio_backend():
            return 0, None
        try:
            target = max(0.0, min(1.0, float(level_percent) / 100.0))
        except Exception:
            return 0, None

        changed = 0
        level_values = []
        com_initialized = False
        try:
            if pythoncom is not None:
                pythoncom.CoInitialize()
                com_initialized = True
            for session in AudioUtilities.GetAllSessions():
                try:
                    proc = getattr(session, "Process", None)
                    pid = getattr(proc, "pid", None)
                    if pid not in pid_set:
                        continue
                    volume = getattr(session, "SimpleAudioVolume", None)
                    if volume is None:
                        continue
                    volume.SetMasterVolume(target, None)
                    level_values.append(target)
                    changed += 1
                except Exception:
                    continue
        except Exception:
            return 0, None
        finally:
            if com_initialized:
                try:
                    pythoncom.CoUninitialize()
                except Exception:
                    pass

        if not level_values:
            return 0, None
        avg_level = int(round((sum(level_values) / len(level_values)) * 100))
        return changed, avg_level

    def _remember_volume_for_sandbox(self, sandbox_name, level_percent):
        if not getattr(self, "remember_volume", True):
            return False
        name = self._get_volume_memory_key(sandbox_name)
        if not name:
            return False
        try:
            level = int(round(float(level_percent)))
        except Exception:
            return False
        level = max(0, min(100, level))
        if self.sandbox_volume_memory.get(name) == level:
            return False
        self.sandbox_volume_memory[name] = level
        return True

    def _remember_volume_for_all_running_sandboxes(self, level_percent):
        if not getattr(self, "remember_volume", True):
            return
        changed = False
        sandbox_names = []
        try:
            sandbox_names = self.sandbox_list.scan_sandboxes()
        except Exception:
            sandbox_names = []
        for sandbox_name in sandbox_names:
            pids = self._get_target_pids_for_sandbox(sandbox_name)
            if not pids:
                continue
            if self._remember_volume_for_sandbox(sandbox_name, level_percent):
                changed = True
        if changed:
            self.save_settings()

    def _restore_saved_volume_for_sandbox(self, sandbox_name):
        if not getattr(self, "remember_volume", True):
            return
        memory_key = self._get_volume_memory_key(sandbox_name)
        target_level = self.sandbox_volume_memory.get(memory_key)
        if target_level is None:
            return
        deadline = time.time() + 8.0
        while time.time() < deadline:
            pids = self._get_target_pids_for_sandbox(sandbox_name)
            if pids:
                changed, _ = self._set_volume_for_pids(pids, target_level)
                if changed > 0:
                    return
            time.sleep(0.45)

    def _has_volume_session_for_pids(self, pids):
        pid_set = self._normalize_pid_set(pids)
        if not pid_set or not self._ensure_audio_backend():
            return False
        com_initialized = False
        try:
            if pythoncom is not None:
                pythoncom.CoInitialize()
                com_initialized = True
            for session in AudioUtilities.GetAllSessions():
                try:
                    proc = getattr(session, "Process", None)
                    pid = getattr(proc, "pid", None)
                    if pid not in pid_set:
                        continue
                    if getattr(session, "SimpleAudioVolume", None) is not None:
                        return True
                except Exception:
                    continue
            return False
        except Exception:
            return False
        finally:
            if com_initialized:
                try:
                    pythoncom.CoUninitialize()
                except Exception:
                    pass

    def _terminate_pids(self, pids):
        pid_list = sorted(self._normalize_pid_set(pids))
        if not pid_list:
            return 0

        def _pid_exists(pid):
            try:
                os.kill(pid, 0)
                return True
            except PermissionError:
                return True
            except OSError:
                return False

        alive_before = {pid for pid in pid_list if _pid_exists(pid)}
        if not alive_before:
            return 0

        chunk_size = 60
        for i in range(0, len(pid_list), chunk_size):
            chunk = pid_list[i:i + chunk_size]
            cmd = ["taskkill", "/T", "/F"]
            for pid in chunk:
                cmd.extend(["/PID", str(pid)])
            try:
                subprocess.run(
                    cmd,
                    stdout=subprocess.DEVNULL,
                    stderr=subprocess.DEVNULL,
                    timeout=8
                )
            except Exception:
                continue

        time.sleep(0.12)
        alive_after = {pid for pid in alive_before if _pid_exists(pid)}
        return len(alive_before - alive_after)

    def _get_alive_pids(self, pids):
        pid_list = sorted(self._normalize_pid_set(pids))
        if not pid_list:
            return []

        alive = []
        for pid in pid_list:
            try:
                os.kill(pid, 0)
                alive.append(pid)
            except PermissionError:
                alive.append(pid)
            except OSError:
                continue
        return alive

    def _wait_for_pids_exit(self, pids, timeout=1.2, interval=0.12):
        alive = self._get_alive_pids(pids)
        if not alive:
            return []
        deadline = time.time() + max(0.0, float(timeout))
        sleep_interval = max(0.05, float(interval))
        while alive and time.time() < deadline:
            time.sleep(sleep_interval)
            alive = self._get_alive_pids(alive)
        return alive

    def _terminate_sandbox_via_start(self, sandbox_name):
        sandbox_name = self._normalize_sandbox_name(sandbox_name)
        if not sandbox_name:
            return False
        if not is_valid_internal_sandbox_name(sandbox_name):
            return False
        try:
            official_names = self._get_official_sandbox_name_set()
        except Exception:
            official_names = set()
        if official_names and sandbox_name.lower() not in official_names:
            return False
        result, _ = self._run_sandboxie_start([f"/box:{sandbox_name}", "/terminate"], timeout=8)
        if result is None:
            return False
        return result.returncode == 0

    def _terminate_all_via_start(self):
        result, _ = self._run_sandboxie_start(["/terminate_all"], timeout=8)
        if result is None:
            return False
        return result.returncode == 0

    def get_close_menu_state_for_all(self):
        return True if self._get_sandboxie_start_exe() else None

    def get_close_menu_state_for_sandbox(self, sandbox_name):
        return True if sandbox_name and self._get_sandboxie_start_exe() else None

    def get_volume_menu_state_for_sandbox(self, sandbox_name):
        pids = self._get_target_pids_for_sandbox(sandbox_name)
        return True if self._has_volume_session_for_pids(pids) else None

    def get_volume_menu_state_for_all(self):
        pids = self._get_target_pids_for_all()
        return True if self._has_volume_session_for_pids(pids) else None

    def get_cached_mute_state_for_sandbox(self, sandbox_name):
        """Return the real current mute state for the selected sandbox.

        The menu used to trust _cached_mute_state_by_sandbox only.  That could
        make user B show "unmute current process" after user A had been muted,
        even when B's current process was not muted.  For the current item we
        verify the audio session now, then refresh the small cache.
        """
        name = self._normalize_sandbox_name(sandbox_name)
        if not name:
            return None
        state = self.get_mute_menu_state_for_sandbox(name)
        if state is None:
            self._cached_mute_state_by_sandbox.pop(name, None)
        else:
            self._cached_mute_state_by_sandbox[name] = bool(state)
        return state

    def get_cached_mute_state_for_all(self):
        return self._cached_mute_state_all

    def get_menu_cached_mute_state_for_sandbox(self, sandbox_name):
        name = self._normalize_sandbox_name(sandbox_name)
        if not name:
            return None
        return self._cached_mute_state_by_sandbox.get(name)

    def get_mute_menu_state_for_sandbox(self, sandbox_name):
        return self._get_mute_state_for_pids(self._get_target_pids_for_sandbox(sandbox_name))

    def get_locate_menu_state_for_sandbox(self, sandbox_name):
        pids = self._get_target_pids_for_sandbox(sandbox_name)
        if not pids:
            return None
        return True if self._find_window_for_pids(pids) else None

    def get_mute_menu_state_for_all(self):
        return self._get_mute_state_for_pids(self._get_target_pids_for_all())

    def _refresh_process_ui_after_action(self, refresh_list=False):
        try:
            self._invalidate_process_menu_state_cache()
            self._invalidate_process_menu_live_state_cache(clear_values=False)
        except Exception:
            pass
        if refresh_list and hasattr(self, "sandbox_list"):
            current_status = ""
            try:
                current_status = self.lbl_status.GetLabel()
            except Exception:
                current_status = ""
            self._suppress_selection_status(8.0)
            try:
                self.sandbox_list.on_refresh_sandbox_list(None)
            except Exception:
                pass
            if current_status:
                try:
                    self.lbl_status.SetLabel(current_status)
                except Exception:
                    pass
        self._schedule_process_menu_live_state_refresh(delay_ms=1)
        self._schedule_menu_bar_rebuild(delay_ms=24)

    def _refresh_visible_sandbox_running_states(self):
        try:
            if hasattr(self, "sandbox_list"):
                return bool(self.sandbox_list.refresh_visible_running_states())
        except Exception:
            pass
        return False

    def on_mute_current_sandbox(self, sandbox_name):
        sandbox_name, display_name = self._get_sandbox_internal_and_display_name(sandbox_name)

        def _worker():
            pids = self._get_target_pids_for_sandbox(sandbox_name)
            if not pids:
                self._safe_call_after(self._finish_mute_current_sandbox, sandbox_name, display_name, None, None, "no_target")
                return
            state = self._get_mute_state_for_pids(pids)
            target_mute = False if state is True else True
            try:
                mute_result = self._set_mute_for_pids_detailed(pids, target_mute)
            except Exception:
                self._safe_call_after(self._finish_mute_current_sandbox, sandbox_name, display_name, target_mute, None, "failed")
                return
            self._safe_call_after(self._finish_mute_current_sandbox, sandbox_name, display_name, target_mute, mute_result, "")

        self._run_volume_task_async(f"正在切换静音状态: {display_name}", _worker)

    def _finish_mute_current_sandbox(self, sandbox_name, display_name, target_mute, mute_result, failure_reason=""):
        if failure_reason == "no_target":
            self._cached_mute_state_by_sandbox.pop(sandbox_name, None)
            self.lbl_status.SetLabel(
                self._with_assist_hint("mute_current", f"当前用户 {display_name} 没有可静音的已开进程")
            )
            self._refresh_process_ui_after_action(refresh_list=False)
            return
        if failure_reason == "failed" or not isinstance(mute_result, dict):
            self._cached_mute_state_by_sandbox.pop(sandbox_name, None)
            self._cached_mute_state_all = None
            action = "取消静音" if not target_mute else "静音"
            self.lbl_status.SetLabel(
                self._with_assist_hint("mute_current", f"{action}失败: {display_name}")
            )
            self._refresh_process_ui_after_action(refresh_list=False)
            return
        changed = int(mute_result.get("changed_sessions", 0) or 0)
        changed_pids = set(mute_result.get("changed_pids", set()) or set())
        matched_pids = set(mute_result.get("matched_pids", set()) or set())
        failed_pids = set(mute_result.get("failed_pids", set()) or set())
        fully_changed = bool(matched_pids) and changed_pids == matched_pids and not failed_pids
        if changed > 0 and fully_changed:
            self._cached_mute_state_by_sandbox[sandbox_name] = target_mute
            self._cached_mute_state_all = None
            self.lbl_status.SetLabel(
                self._with_assist_hint("mute_current", f"{'已取消静音' if not target_mute else '已静音'}: {display_name}")
            )
            self._speak_process_assist(
                f"{'取消静音' if not target_mute else '静音'}{display_name}进程",
                event_key="mute_current"
            )
        elif changed > 0:
            self._cached_mute_state_by_sandbox.pop(sandbox_name, None)
            self._cached_mute_state_all = None
            self.lbl_status.SetLabel(
                self._with_assist_hint(
                    "mute_current",
                    f"{'已部分取消静音' if not target_mute else '已部分静音'}: {display_name}"
                )
            )
            self._speak_process_assist(
                f"{'部分取消静音' if not target_mute else '部分静音'}{display_name}进程",
                event_key="mute_current"
            )
        elif failed_pids:
            self._cached_mute_state_by_sandbox.pop(sandbox_name, None)
            self._cached_mute_state_all = None
            self.lbl_status.SetLabel(
                self._with_assist_hint(
                    "mute_current",
                    f"{'取消静音' if not target_mute else '静音'}失败: {display_name}"
                )
            )
        else:
            self._cached_mute_state_by_sandbox.pop(sandbox_name, None)
            self.lbl_status.SetLabel(
                self._with_assist_hint("mute_current", f"未找到可静音的进程: {display_name}")
            )
        self._refresh_process_ui_after_action(refresh_list=False)

    def on_mute_all_sandboxes(self):
        def _worker():
            pids = self._get_target_pids_for_all()
            if not pids:
                self._safe_call_after(self._finish_mute_all_sandboxes, None, None, "no_target")
                return
            state = self._get_mute_state_for_pids(pids)
            target_mute = False if state is True else True
            try:
                mute_result = self._set_mute_for_pids_detailed(pids, target_mute)
            except Exception:
                self._safe_call_after(self._finish_mute_all_sandboxes, target_mute, None, "failed")
                return
            self._safe_call_after(self._finish_mute_all_sandboxes, target_mute, mute_result, "")

        self._run_volume_task_async("正在切换全部打开进程静音状态...", _worker)

    def _finish_mute_all_sandboxes(self, target_mute, mute_result, failure_reason=""):
        if failure_reason == "no_target":
            self._cached_mute_state_all = None
            self.lbl_status.SetLabel(
                self._with_assist_hint("mute_all", "当前没有可静音的全部打开进程")
            )
            self._refresh_process_ui_after_action(refresh_list=False)
            return
        if failure_reason == "failed" or not isinstance(mute_result, dict):
            self._cached_mute_state_all = None
            self._cached_mute_state_by_sandbox.clear()
            action = "取消静音" if not target_mute else "静音"
            self.lbl_status.SetLabel(
                self._with_assist_hint("mute_all", f"{action}全部打开的进程失败")
            )
            self._refresh_process_ui_after_action(refresh_list=False)
            return
        changed = int(mute_result.get("changed_sessions", 0) or 0)
        changed_pids = set(mute_result.get("changed_pids", set()) or set())
        matched_pids = set(mute_result.get("matched_pids", set()) or set())
        failed_pids = set(mute_result.get("failed_pids", set()) or set())
        fully_changed = bool(matched_pids) and changed_pids == matched_pids and not failed_pids
        if changed > 0 and fully_changed:
            self._cached_mute_state_all = target_mute
            if hasattr(self, "sandbox_list") and self.sandbox_list is not None:
                try:
                    for i in range(self.sandbox_list.GetItemCount()):
                        if self.sandbox_list._is_special_index(i):
                            continue
                        sandbox_name = self.sandbox_list.get_item_identity_text(i)
                        if not sandbox_name:
                            continue
                        self._cached_mute_state_by_sandbox[sandbox_name] = target_mute
                except Exception:
                    pass
            self.lbl_status.SetLabel(
                self._with_assist_hint("mute_all", f"{'已取消静音' if not target_mute else '已静音'}全部打开的进程")
            )
            self._speak_process_assist(
                "全部取消静音" if not target_mute else "全部静音",
                event_key="mute_all"
            )
        elif changed > 0:
            self._cached_mute_state_all = None
            self._cached_mute_state_by_sandbox.clear()
            self.lbl_status.SetLabel(
                self._with_assist_hint(
                    "mute_all",
                    f"{'已部分取消静音' if not target_mute else '已部分静音'}全部打开的进程"
                )
            )
            self._speak_process_assist(
                "全部进程部分取消静音" if not target_mute else "全部进程部分静音",
                event_key="mute_all"
            )
        elif failed_pids:
            self._cached_mute_state_all = None
            self._cached_mute_state_by_sandbox.clear()
            self.lbl_status.SetLabel(
                self._with_assist_hint(
                    "mute_all",
                    f"{'取消静音' if not target_mute else '静音'}全部打开的进程失败"
                )
            )
        else:
            self._cached_mute_state_all = None
            self.lbl_status.SetLabel(
                self._with_assist_hint("mute_all", "未找到可静音的全部打开进程")
            )
        self._refresh_process_ui_after_action(refresh_list=False)

    def on_switch_output_device_current_sandbox(self, sandbox_name):
        sandbox_name, display_name = self._get_sandbox_internal_and_display_name(sandbox_name)
        pids = self._get_target_pids_for_sandbox(sandbox_name)
        if not pids:
            self.lbl_status.SetLabel(f"当前用户 {display_name} 没有可切换设备的已开进程")
            return
        devices = self._list_output_audio_devices()
        if not devices:
            self.lbl_status.SetLabel("未找到可用的播放设备")
            return
        device = self._choose_output_device_from_devices("切换当前打开的进程播放设备", devices)
        if not device:
            return

        def _worker():
            try:
                changed, failed = self._set_output_device_for_pids(pids, device["id"])
            except Exception:
                self._safe_call_after(self._finish_switch_output_device_current_sandbox, display_name, device, 0, 1, True)
                return
            self._safe_call_after(self._finish_switch_output_device_current_sandbox, display_name, device, changed, failed, False)

        self._run_volume_task_async(f"正在切换当前打开进程播放设备: {display_name}", _worker)

    def _finish_switch_output_device_current_sandbox(self, display_name, device, changed, failed, exception_failed=False):
        if exception_failed:
            self.lbl_status.SetLabel(f"切换播放设备失败: {display_name}")
            self._refresh_process_ui_after_action(refresh_list=False)
            return
        if changed > 0 and failed <= 0:
            self.lbl_status.SetLabel(f"已切换当前打开进程播放设备: {display_name} -> {device['name']}")
            self._speak_process_assist(
                f"把{display_name}进程切换到播放设备{device['name']}",
                event_key="switch_output_current"
            )
        elif changed > 0:
            self.lbl_status.SetLabel(
                f"已部分切换当前打开进程播放设备: {display_name} -> {device['name']}（成功 {changed}，失败 {failed}）"
            )
            self._speak_process_assist(
                f"部分切换{display_name}进程到播放设备{device['name']}",
                event_key="switch_output_current"
            )
        elif failed > 0:
            self.lbl_status.SetLabel(f"切换播放设备失败: {display_name}")
        else:
            self.lbl_status.SetLabel(f"未找到可切换设备的进程: {display_name}")
        self._refresh_process_ui_after_action(refresh_list=False)

    def on_switch_output_device_all_sandboxes(self):
        pids = self._get_target_pids_for_all()
        if not pids:
            self.lbl_status.SetLabel("当前没有可切换设备的全部打开进程")
            return
        devices = self._list_output_audio_devices()
        if not devices:
            self.lbl_status.SetLabel("未找到可用的播放设备")
            return
        device = self._choose_output_device_from_devices("切换全部打开的进程播放设备", devices)
        if not device:
            return

        def _worker():
            try:
                changed, failed = self._set_output_device_for_pids(pids, device["id"])
            except Exception:
                self._safe_call_after(self._finish_switch_output_device_all_sandboxes, device, 0, 1, True)
                return
            self._safe_call_after(self._finish_switch_output_device_all_sandboxes, device, changed, failed, False)

        self._run_volume_task_async("正在切换全部打开进程播放设备...", _worker)

    def _finish_switch_output_device_all_sandboxes(self, device, changed, failed, exception_failed=False):
        if exception_failed:
            self.lbl_status.SetLabel("切换全部打开进程播放设备失败")
            self._refresh_process_ui_after_action(refresh_list=False)
            return
        if changed > 0 and failed <= 0:
            self.lbl_status.SetLabel(f"已切换全部打开进程播放设备 -> {device['name']}")
            self._speak_process_assist(
                f"把全部进程切换到播放设备{device['name']}",
                event_key="switch_output_all"
            )
        elif changed > 0:
            self.lbl_status.SetLabel(
                f"已部分切换全部打开进程播放设备 -> {device['name']}（成功 {changed}，失败 {failed}）"
            )
            self._speak_process_assist(
                f"部分进程已切换到播放设备{device['name']}",
                event_key="switch_output_all"
            )
        elif failed > 0:
            self.lbl_status.SetLabel("切换全部打开进程播放设备失败")
        else:
            self.lbl_status.SetLabel("未找到可切换设备的全部打开进程")
        self._refresh_process_ui_after_action(refresh_list=False)

    def _run_volume_task_async(self, start_status, worker):
        if not self._volume_task_lock.acquire(blocking=False):
            self.lbl_status.SetLabel("正在执行音频操作，请稍候")
            return
        self.lbl_status.SetLabel(str(start_status or "正在执行音频操作..."))

        def _task():
            status_text = ""
            try:
                result = worker()
                if isinstance(result, str) and result.strip():
                    status_text = result.strip()
            except Exception as e:
                status_text = f"音频操作失败: {str(e)}"
            finally:
                self._volume_task_lock.release()
            if status_text:
                self._safe_call_after(self.lbl_status.SetLabel, status_text)
            self._safe_call_after(self.create_menu_bar)

        threading.Thread(target=_task, daemon=True).start()

    def on_adjust_volume_all_sandboxes(self, step_percent):
        step = int(step_percent)

        def _worker():
            pids = self._get_target_pids_for_all()
            if not pids:
                return "当前没有可调节音量的全部打开进程"
            volume_result = self._adjust_volume_for_pids_detailed(pids, step / 100.0)
            changed = int(volume_result.get("changed_sessions", 0) or 0)
            avg_level = volume_result.get("avg_level")
            changed_pids = set(volume_result.get("changed_pids", set()) or set())
            matched_pids = set(volume_result.get("matched_pids", set()) or set())
            failed_pids = set(volume_result.get("failed_pids", set()) or set())
            fully_changed = bool(matched_pids) and changed_pids == matched_pids and not failed_pids
            if changed <= 0:
                if failed_pids:
                    return f"{'提高' if step > 0 else '降低'}全部打开进程音量失败"
                return "未找到可调节音量的全部打开进程"
            if fully_changed and avg_level is not None:
                self._remember_volume_for_all_running_sandboxes(avg_level)
            action = "提高" if step > 0 else "降低"
            if fully_changed:
                self._speak_process_assist(
                    f"全部进程音量{action}{abs(step)}%",
                    event_key="volume_all"
                )
                if avg_level is None:
                    return f"已将全部打开进程音量{action} {abs(step)}%"
                return f"已将全部打开进程音量{action} {abs(step)}%，当前约 {avg_level}%"
            self._speak_process_assist(
                f"全部进程音量部分{action}{abs(step)}%",
                event_key="volume_all"
            )
            return f"已部分将全部打开进程音量{action} {abs(step)}%"

        action = "提高" if step > 0 else "降低"
        self._run_volume_task_async(f"正在{action}全部打开进程音量...", _worker)

    def on_adjust_volume_current_sandbox(self, sandbox_name, step_percent=5):
        sandbox_name, display_name = self._get_sandbox_internal_and_display_name(sandbox_name)
        step = int(step_percent)

        def _worker():
            pids = self._get_target_pids_for_sandbox(sandbox_name)
            if not pids:
                return f"当前用户 {display_name} 没有可调节音量的已开进程"
            volume_result = self._adjust_volume_for_pids_detailed(pids, step / 100.0)
            changed = int(volume_result.get("changed_sessions", 0) or 0)
            avg_level = volume_result.get("avg_level")
            changed_pids = set(volume_result.get("changed_pids", set()) or set())
            matched_pids = set(volume_result.get("matched_pids", set()) or set())
            failed_pids = set(volume_result.get("failed_pids", set()) or set())
            fully_changed = bool(matched_pids) and changed_pids == matched_pids and not failed_pids
            if changed <= 0:
                if failed_pids:
                    return f"{'提高' if step > 0 else '降低'}音量失败: {display_name}"
                return f"未找到可调节音量的进程: {display_name}"
            if fully_changed and avg_level is not None and self._remember_volume_for_sandbox(sandbox_name, avg_level):
                self.save_settings()
            action = "提高" if step > 0 else "降低"
            if fully_changed:
                self._speak_process_assist(
                    f"{display_name}进程音量{action}{abs(step)}%",
                    event_key="volume_current"
                )
                if avg_level is None:
                    return f"已将 {display_name} 音量{action} {abs(step)}%"
                return f"已将 {display_name} 音量{action} {abs(step)}%，当前约 {avg_level}%"
            self._speak_process_assist(
                f"{display_name}进程音量部分{action}{abs(step)}%",
                event_key="volume_current"
            )
            return f"已部分将 {display_name} 音量{action} {abs(step)}%"

        action = "提高" if step > 0 else "降低"
        self._run_volume_task_async(f"正在{action}{display_name}进程音量...", _worker)

    def _close_sandbox_processes(self, sandbox_name, allow_pid_fallback=True):
        sandbox_name = self._normalize_sandbox_name(sandbox_name)
        result = {
            "sandbox_name": sandbox_name,
            "status": "none_closed",
            "closed": 0,
            "any_closed": False,
        }
        self._cancel_locate_window_reinforcement(wait=True)
        pids_before = self._get_target_pids_for_sandbox(sandbox_name)
        try:
            self._move_focus_off_target_pids(pids_before)
        except Exception:
            pass
        total_closed = 0
        if self._terminate_sandbox_via_start(sandbox_name):
            if pids_before:
                total_before = len(self._normalize_pid_set(pids_before))
                remaining_after_start = self._wait_for_pids_exit(pids_before)
                if not remaining_after_start:
                    result["status"] = "all_closed"
                    result["closed"] = total_before
                    result["any_closed"] = True
                    return result
                total_closed += max(0, total_before - len(self._normalize_pid_set(remaining_after_start)))
                pids = remaining_after_start
            else:
                pids = self._get_target_pids_for_sandbox(sandbox_name)
                if not pids:
                    result["status"] = "no_target"
                    return result
        else:
            pids = list(pids_before)
            if not pids:
                pids = self._get_target_pids_for_sandbox(sandbox_name)
        if not pids:
            result["status"] = "no_target"
            return result
        if not allow_pid_fallback:
            result["closed"] = total_closed
            if total_closed > 0:
                result["status"] = "partially_closed"
                result["any_closed"] = True
            return result
        tracked_pids = sorted(self._normalize_pid_set(pids))
        self._terminate_pids(tracked_pids)
        remaining_after_pid = self._wait_for_pids_exit(tracked_pids, timeout=1.0, interval=0.1)
        total_closed += max(0, len(self._normalize_pid_set(tracked_pids)) - len(self._normalize_pid_set(remaining_after_pid)))
        result["closed"] = total_closed
        if not remaining_after_pid:
            result["status"] = "all_closed"
            result["any_closed"] = total_closed > 0
        elif total_closed > 0:
            result["status"] = "partially_closed"
            result["any_closed"] = True
        return result

    def _run_close_process_task_async(self, start_status, worker):
        if not self._close_process_task_lock.acquire(blocking=False):
            self.lbl_status.SetLabel("正在关闭进程，请稍候")
            return
        self.lbl_status.SetLabel(str(start_status or "正在关闭进程..."))

        def _task():
            try:
                worker()
            except Exception as e:
                self._safe_call_after(self.lbl_status.SetLabel, f"关闭进程失败: {str(e)}")
            finally:
                self._close_process_task_lock.release()

        threading.Thread(target=_task, daemon=True).start()

    def _finish_close_current_sandbox(self, display_name, close_result):
        status = close_result.get("status")
        closed = int(close_result.get("closed", 0) or 0)
        if status == "all_closed":
            self.lbl_status.SetLabel(f"已关闭当前打开的进程: {display_name}")
            self._speak_process_assist(f"已关闭{display_name}进程", event_key="close_current")
            self._refresh_process_ui_after_action(refresh_list=True)
            return
        if status == "no_target":
            self.lbl_status.SetLabel(f"当前用户 {display_name} 没有可关闭的已开进程")
            self._refresh_process_ui_after_action(refresh_list=False)
            return
        if status == "partially_closed":
            self.lbl_status.SetLabel(f"已关闭部分进程，仍有进程未关闭: {display_name}")
        elif closed > 0:
            self.lbl_status.SetLabel(f"已关闭 {closed} 个进程: {display_name}")
            self._speak_process_assist(f"已关闭{display_name}进程", event_key="close_current")
        else:
            self.lbl_status.SetLabel(f"未关闭任何进程: {display_name}")
        self._refresh_process_ui_after_action(refresh_list=True)

    def on_close_current_sandbox(self, sandbox_name):
        sandbox_name, display_name = self._get_sandbox_internal_and_display_name(sandbox_name)

        def _worker():
            close_result = self._close_sandbox_processes(sandbox_name)
            self._safe_call_after(self._finish_close_current_sandbox, display_name, close_result)

        self._run_close_process_task_async(f"正在关闭当前打开的进程: {display_name}", _worker)

    def on_close_all_sandboxes(self):
        def _worker():
            self._cancel_active_launch()
            pids_before_all = self._get_target_pids_for_all()
            if self._terminate_all_via_start():
                if pids_before_all:
                    remaining_after_start_all = self._wait_for_pids_exit(pids_before_all)
                    if not remaining_after_start_all:
                        self._safe_call_after(self._finish_close_all_sandboxes, "all_closed", 0, False)
                        return
                    any_terminated = len(remaining_after_start_all) < len(self._normalize_pid_set(pids_before_all))
                else:
                    remaining_after_start_all = self._get_target_pids_for_all()
                    if not remaining_after_start_all:
                        self._safe_call_after(self._finish_close_all_sandboxes, "no_target", 0, False)
                        return
                    any_terminated = False
            else:
                remaining_after_start_all = list(pids_before_all)
                any_terminated = False
            sandbox_names = self._scan_sandbox_names_for_worker()
            for sandbox_name in sandbox_names:
                close_result = self._close_sandbox_processes(sandbox_name, allow_pid_fallback=False)
                if close_result.get("any_closed"):
                    any_terminated = True
            pids = self._get_target_pids_for_all()
            if not pids and remaining_after_start_all:
                pids = list(remaining_after_start_all)
            if not pids:
                self._safe_call_after(
                    self._finish_close_all_sandboxes,
                    "all_closed" if any_terminated else "no_target",
                    0,
                    any_terminated
                )
                return
            closed = self._terminate_pids(pids)
            remaining_pids = []
            if closed > 0 or any_terminated:
                try:
                    remaining_pids = self._get_target_pids_for_all()
                except Exception:
                    remaining_pids = list(pids)
            if not remaining_pids and (closed > 0 or any_terminated):
                self._safe_call_after(self._finish_close_all_sandboxes, "all_closed", closed, any_terminated)
            elif closed > 0:
                self._safe_call_after(self._finish_close_all_sandboxes, "closed_some", closed, any_terminated)
            elif any_terminated:
                self._safe_call_after(self._finish_close_all_sandboxes, "partially_closed", 0, any_terminated)
            else:
                self._safe_call_after(self._finish_close_all_sandboxes, "none_closed", 0, False)

        self._run_close_process_task_async("正在关闭全部打开进程...", _worker)

    def _finish_close_all_sandboxes(self, status, closed=0, any_terminated=False):
        closed = int(closed or 0)
        if status == "all_closed":
            self.lbl_status.SetLabel("已关闭全部打开进程")
            self._speak_process_assist("已关闭全部进程", event_key="close_all")
            self._refresh_process_ui_after_action(refresh_list=True)
            return
        if status == "no_target":
            self.lbl_status.SetLabel("当前没有可关闭的全部打开进程")
            self._refresh_process_ui_after_action(refresh_list=False)
            return
        if status == "closed_some":
            self.lbl_status.SetLabel(f"已关闭 {closed} 个全部打开进程")
            self._speak_process_assist("已关闭全部进程", event_key="close_all")
        elif status == "partially_closed" or any_terminated:
            self.lbl_status.SetLabel("已关闭部分打开进程，仍有进程未关闭")
        else:
            self.lbl_status.SetLabel("未关闭任何全部打开进程")
        self._refresh_process_ui_after_action(refresh_list=True)

    def on_locate_current_sandbox(self, sandbox_name):
        self._cancel_locate_window_reinforcement(wait=True)
        sandbox_name, display_name = self._get_sandbox_internal_and_display_name(sandbox_name)
        pids = self._get_target_pids_for_sandbox(sandbox_name)
        if not pids:
            self.lbl_status.SetLabel(f"当前用户 {display_name} 没有可定位的打开进程")
            return
        windows = self._find_windows_for_pids(pids)
        if not windows:
            self.lbl_status.SetLabel(f"当前用户 {display_name} 没有可定位的打开进程")
            return
        hwnd = self._prompt_window_choice(display_name, windows)
        if not hwnd:
            self.lbl_status.SetLabel(f"已取消定位当前打开的进程: {display_name}")
            return
        selected_window = None
        target_hwnd = int(hwnd or 0)
        for item in windows:
            try:
                if int(item.get("hwnd") or 0) == target_hwnd:
                    selected_window = item
                    break
            except Exception:
                continue
        selected_was_minimized = bool((selected_window or {}).get("minimized"))
        if not self._stabilize_activated_window(hwnd):
            self.lbl_status.SetLabel(f"定位进程失败: {display_name}")
            return
        if selected_was_minimized:
            self._start_locate_window_reinforcement(hwnd, keep_long_tail=True)
            def _finish_minimized_locate():
                if self._is_window_closing():
                    return
                self._cancel_locate_window_reinforcement(wait=True)
                if not self._stabilize_activated_window(hwnd):
                    self.lbl_status.SetLabel(f"定位进程失败: {display_name}")
                    return
                self._start_locate_window_reinforcement(hwnd, keep_long_tail=True)
                if len(windows) > 1:
                    if not self._has_foreground_window_streak(hwnd, required_streak=2, total_checks=4, interval_seconds=0.1):
                        try:
                            self._activate_window(hwnd)
                        except Exception:
                            pass
                        if not self._has_foreground_window_streak(hwnd, required_streak=2, total_checks=4, interval_seconds=0.1):
                            self._cancel_locate_window_reinforcement(wait=True)
                            self.lbl_status.SetLabel(f"定位进程失败: {display_name}")
                            return
                self.lbl_status.SetLabel(f"已定位到当前打开的进程: {display_name}")
                self._speak_process_assist(f"已定位{display_name}进程", event_key="locate_current")
            self._safe_call_later(2450, _finish_minimized_locate)
            return
        if len(windows) > 1:
            if not self._has_foreground_window_streak(hwnd, required_streak=2, total_checks=4, interval_seconds=0.1):
                try:
                    self._activate_window(hwnd)
                except Exception:
                    pass
                if not self._has_foreground_window_streak(hwnd, required_streak=2, total_checks=4, interval_seconds=0.1):
                    self._cancel_locate_window_reinforcement(wait=True)
                    self.lbl_status.SetLabel(f"定位进程失败: {display_name}")
                    return
        self.lbl_status.SetLabel(f"已定位到当前打开的进程: {display_name}")
        self._speak_process_assist(f"已定位{display_name}进程", event_key="locate_current")
