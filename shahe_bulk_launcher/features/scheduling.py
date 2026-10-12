import os
import threading
import time

import wx

from ..backup import BackupRestoreMgr


class SchedulingMixin:
    def _resolve_launch_entries(self, entries):
        try:
            self._refresh_sandbox_display_maps()
        except Exception:
            pass
        available_display = set(self.sandbox_list.scan_sandboxes())
        available_internal = {str(name or "").strip().lower() for name in available_display}
        try:
            available_internal.update(str(name or "").strip().lower() for name in self._sandbox_name_to_display.keys())
        except Exception:
            pass
        direct_entries = []
        seen_direct = set()
        for entry in entries or []:
            text = str(entry or "").strip()
            if not text:
                continue
            if text not in seen_direct:
                seen_direct.add(text)
                direct_entries.append(text)

        resolved = []

        def add_resolved_sandbox(name):
            internal_name = self._normalize_sandbox_name(name)
            if not internal_name:
                return
            if internal_name.lower() not in available_internal and str(name or "").strip() not in available_display:
                return
            display_name = self._get_sandbox_display_name(internal_name)
            if display_name and display_name not in resolved:
                resolved.append(display_name)

        for text in direct_entries:
            add_resolved_sandbox(text)
        return resolved

    def _prompt_schedule_delay_seconds(self, title, message, default_seconds=30):
        dlg = wx.NumberEntryDialog(
            self,
            message,
            "延迟秒数",
            title,
            int(max(1, default_seconds)),
            1,
            86400
        )
        try:
            if dlg.ShowModal() != wx.ID_OK:
                return 0
            return int(dlg.GetValue())
        finally:
            dlg.Destroy()

    def _schedule_task(self, task_key, delay_seconds, callback, description, sandbox_targets=None, retry_delay_seconds=1):
        delay = int(max(1, delay_seconds))
        self._schedule_seq += 1
        task_id = self._schedule_seq
        job_key = f"{str(task_key or '').strip() or 'task'}#{task_id}"
        timer = self._safe_call_later(delay * 1000, self._execute_scheduled_task, job_key, task_id, callback)
        self._schedule_jobs[job_key] = {
            "id": task_id,
            "task_type": str(task_key or "").strip(),
            "timer": timer,
            "description": str(description or "").strip(),
            "eta": time.time() + delay,
            "sandbox_targets": sandbox_targets if isinstance(sandbox_targets, list) else None,
            "retry_delay_seconds": int(max(1, retry_delay_seconds)),
        }
        desc = self._schedule_jobs[job_key]["description"] or task_key
        self.lbl_status.SetLabel(f"已设置定时任务（{delay} 秒后）: {desc}")
        return job_key

    def _execute_scheduled_task(self, task_key, task_id, callback):
        if self._is_window_closing():
            self._schedule_jobs.pop(task_key, None)
            return
        task = self._schedule_jobs.get(task_key)
        if not task or task.get("id") != task_id:
            return
        description = task.get("description", task_key)
        try:
            should_complete = callback()
        except Exception as e:
            self._schedule_jobs.pop(task_key, None)
            self._safe_set_status_label(f"定时任务执行失败: {description} ({str(e)})")
            return
        if should_complete is False:
            task = self._schedule_jobs.get(task_key)
            if not task or task.get("id") != task_id or self._is_window_closing():
                self._schedule_jobs.pop(task_key, None)
                return
            retry_delay = int(max(1, task.get("retry_delay_seconds", 1) or 1))
            task["eta"] = time.time() + retry_delay
            task["timer"] = self._safe_call_later(retry_delay * 1000, self._execute_scheduled_task, task_key, task_id, callback)
            return
        self._schedule_jobs.pop(task_key, None)

    def _cancel_schedule_task(self, task_key, update_status=True):
        task = self._schedule_jobs.pop(task_key, None)
        if not task:
            return False
        timer = task.get("timer")
        if timer is not None:
            try:
                timer.Stop()
            except Exception:
                pass
        if update_status:
            desc = task.get("description", task_key)
            self.lbl_status.SetLabel(f"已取消定时任务: {desc}")
        return True

    def on_cancel_all_schedules(self, event=None, update_status=True):
        keys = list(self._schedule_jobs.keys())
        if not keys:
            if update_status:
                self.lbl_status.SetLabel("当前没有定时任务")
            return
        for key in keys:
            self._cancel_schedule_task(key, update_status=False)
        if update_status:
            self.lbl_status.SetLabel("已取消全部定时任务")

    def _close_selected_sandboxes(self, sandboxes):
        targets = [str(name).strip() for name in (sandboxes or []) if str(name).strip()]
        if not targets:
            self.lbl_status.SetLabel("没有可关闭的沙盒用户")
            return
        self._delete_sandbox_refs_in_active_launch_by_display(targets)
        closed_count = 0
        partial_count = 0
        for sandbox_name in targets:
            close_result = self._close_sandbox_processes(sandbox_name)
            if close_result.get("status") == "all_closed":
                closed_count += 1
            elif close_result.get("status") == "partially_closed":
                partial_count += 1
        if partial_count > 0:
            self.lbl_status.SetLabel(
                f"已关闭 {closed_count}/{len(targets)} 个沙盒用户的进程，另有 {partial_count} 个仍有进程未关闭"
            )
        elif closed_count > 0:
            self.lbl_status.SetLabel(f"已关闭 {closed_count}/{len(targets)} 个沙盒用户的进程")
        else:
            self.lbl_status.SetLabel("未关闭任何沙盒用户进程")
        self._refresh_process_ui_after_action(refresh_list=True)

    def on_schedule_launch_sandboxes(self, event):
        selected = self._get_batch_target_sandboxes()
        if not selected:
            self.lbl_status.SetLabel("请先勾选或选中要定时启动的沙盒用户")
            return
        app_targets = self._get_checked_app_targets()
        if not app_targets:
            self.lbl_status.SetLabel("请先在应用列表中按空格选中要打开的应用")
            return
        delay = self._prompt_schedule_delay_seconds("定时启动沙盒", "请设置多少秒后启动所选沙盒：", default_seconds=30)
        if delay <= 0:
            return
        targets = list(selected)
        scheduled_app_targets = list(app_targets)

        def _do_launch():
            if not targets:
                self.lbl_status.SetLabel("没有可启动的沙盒用户")
                return True
            if self.is_launching:
                self.lbl_status.SetLabel("当前正在启动中，定时启动顺延 1 秒")
                return False
            self._launch_selected_sandboxes(targets, app_targets=scheduled_app_targets)
            return True

        self._schedule_task(
            "launch_selected",
            delay,
            _do_launch,
            f"定时启动 {len(targets)} 个沙盒",
            sandbox_targets=targets,
            retry_delay_seconds=1
        )

    def on_schedule_close_sandboxes(self, event):
        selected = self._get_batch_target_sandboxes()
        if not selected:
            self.lbl_status.SetLabel("请先勾选或选中要定时关闭的沙盒用户")
            return
        delay = self._prompt_schedule_delay_seconds("定时关闭沙盒", "请设置多少秒后关闭所选沙盒：", default_seconds=30)
        if delay <= 0:
            return
        targets = list(selected)
        self._schedule_task(
            "close_selected",
            delay,
            lambda: self._close_selected_sandboxes(targets),
            f"定时关闭 {len(targets)} 个沙盒",
            sandbox_targets=targets
        )

    def _get_checked_app_targets(self):
        checked_indices = []
        if hasattr(self.combo_app_path, "get_checked_values"):
            try:
                checked_indices = self.combo_app_path.get_checked_items()
            except Exception:
                checked_indices = []
        # App list is a checkbox list like the sandbox user list: require explicit Space check.
        # Do not treat the focused row as checked; this avoids screen-reader ambiguity.
        targets = []
        seen = set()
        for index in checked_indices:
            current_path = self._get_app_list_path_by_index(index)
            if not current_path:
                text = self.combo_app_path.GetItemText(index)
                current_path, _ = self.extract_path_and_note(text, preferred_index=index)
            if not current_path:
                current_path = str(self.combo_app_path.GetItemText(index) or "").strip()
            if not current_path:
                continue
            key = os.path.normcase(os.path.abspath(current_path))
            if key in seen:
                continue
            seen.add(key)
            targets.append((current_path, os.path.dirname(current_path)))
        return targets

    def _prepare_launch_target(self):
        app_targets = self._get_checked_app_targets()
        return self._prepare_launch_target_from_app_targets(app_targets)

    def _prepare_launch_target_from_app_targets(self, app_targets):
        app_targets = list(app_targets or [])
        if not app_targets:
            self.lbl_status.SetLabel("请先在应用列表中按空格选中要打开的应用")
            return False
        invalid = [path for path, _ in app_targets if not path or not os.path.exists(path)]
        if invalid:
            self.lbl_status.SetLabel("应用路径无效，请重新选择")
            return False
        if not self.SANDBOXIE_PATH:
            self.lbl_status.SetLabel("未找到沙盒安装路径")
            return False
        start_exe = self._get_sandboxie_start_exe()
        if not start_exe:
            self.lbl_status.SetLabel("未找到沙盒启动程序 Start.exe")
            return False
        self.APP_PATHS = app_targets
        self.APP_PATH = app_targets[0][0]
        self.APP_DIR = app_targets[0][1]
        return True

    def _launch_selected_sandboxes(self, sandboxes, app_targets=None):
        if self.is_launching:
            self.lbl_status.SetLabel("当前正在启动中，请稍候")
            return
        targets = [str(name).strip() for name in (sandboxes or []) if str(name).strip()]
        if not targets:
            self.lbl_status.SetLabel("请先在沙盒用户列表中按空格选中要启动的沙盒")
            return
        if app_targets is None:
            if not self._prepare_launch_target():
                return
        else:
            if not self._prepare_launch_target_from_app_targets(app_targets):
                return
        for app_path, _ in getattr(self, "APP_PATHS", []) or []:
            self.add_to_history(app_path)
        app_count = len(getattr(self, "APP_PATHS", []) or [])
        self.is_launching = True
        self.btn_launch.Disable()
        self.btn_launch.SetLabel("正在启动中...")
        self._active_launch_targets = targets
        if app_count > 1:
            self.lbl_status.SetLabel(f"准备启动 {len(targets)} 个沙盒、{app_count} 个应用")
        start_time = time.time()
        launch_session_id = self._begin_launch_session()
        self._log_operation_safe(f"启动任务已创建: 沙盒={len(targets)}, 应用={app_count}, 任务={len(targets) * app_count}")
        threading.Thread(target=self._launch_thread, args=(launch_session_id, targets, start_time), daemon=True).start()

    def _launch_selected_sandboxes_prepared(self, sandboxes, app_targets):
        if self.is_launching:
            self.lbl_status.SetLabel("当前正在启动中，请稍候")
            return
        targets = [str(name).strip() for name in (sandboxes or []) if str(name).strip()]
        if not targets:
            self.lbl_status.SetLabel("请先在沙盒用户列表中按空格选中要启动的沙盒")
            return
        if not self._prepare_launch_target_from_app_targets(app_targets):
            return
        for app_path, _ in getattr(self, "APP_PATHS", []) or []:
            self.add_to_history(app_path)
        app_count = len(getattr(self, "APP_PATHS", []) or [])
        self.is_launching = True
        self.btn_launch.Disable()
        self.btn_launch.SetLabel("正在启动中...")
        self._active_launch_targets = targets
        if app_count > 1:
            self.lbl_status.SetLabel(f"准备启动 {len(targets)} 个沙盒、{app_count} 个应用")
        else:
            self.lbl_status.SetLabel(f"准备启动 {len(targets)} 个沙盒")
        start_time = time.time()
        launch_session_id = self._begin_launch_session()
        self._log_operation_safe(f"启动任务已创建: 沙盒={len(targets)}, 应用={app_count}, 任务={len(targets) * app_count}")
        threading.Thread(target=self._launch_thread, args=(launch_session_id, targets, start_time), daemon=True).start()

    def on_launch(self, event):
        if self.is_launching:
            self.lbl_status.SetLabel("当前正在启动中，请稍候")
            return
        self._suppress_list_refresh_status(2.0)
        self._suppress_selection_status(2.0)
        checked_entries = self.sandbox_list.get_checked_entry_texts()
        app_targets = self._get_checked_app_targets()
        if not app_targets:
            self.lbl_status.SetLabel("请先在应用列表中按空格选中要打开的应用")
            return
        self.lbl_status.SetLabel("正在准备启动沙盒...")
        self._log_operation_safe(f"启动按钮触发: 勾选沙盒={len(checked_entries)}, 勾选应用={len(app_targets)}")

        def _prepare():
            selected = self._resolve_launch_entries(checked_entries)
            self._log_operation_safe(f"启动目标解析完成: 可启动沙盒={len(selected)}")
            self._safe_call_after(self._launch_selected_sandboxes_prepared, selected, app_targets)

        threading.Thread(target=_prepare, daemon=True).start()

    def _launch_thread(self, launch_session_id, sandboxes, start_time):
        app_targets = list(getattr(self, "APP_PATHS", []) or [])
        if not app_targets and getattr(self, "APP_PATH", ""):
            app_targets = [(self.APP_PATH, getattr(self, "APP_DIR", ""))]
        total_sandboxes = len(sandboxes or [])
        total_apps = len(app_targets)
        total = total_sandboxes * total_apps
        self._log_operation_safe(f"启动线程开始: 沙盒={total_sandboxes}, 应用={total_apps}, 任务={total}")
        if total <= 0:
            total_time = time.time() - start_time
            self._safe_call_after(
                self._launch_done,
                launch_session_id,
                0,
                0,
                0,
                total_time,
                0,
                self._is_launch_session_cancelled(launch_session_id)
            )
            return

        success = 0
        failed = 0
        skipped = 0
        done_count = 0
        interval = int(max(0, self.launch_interval))
        batch_size = min(total_sandboxes, max(1, int(self.launch_concurrency_limit)))
        serial_relief_mode = (interval == 0 and batch_size == 1)
        serial_relief_sleep = 0.06
        lock = threading.Lock()

        for batch_start in range(0, total_sandboxes, batch_size):
            if self._is_launch_session_cancelled(launch_session_id):
                break
            batch = list(sandboxes[batch_start:batch_start + batch_size])
            launch_targets = []

            for sandbox_name in batch:
                if not str(sandbox_name or "").strip():
                    continue
                if self._is_launch_session_cancelled(launch_session_id):
                    break
                if self.skip_running_sandbox_on_launch:
                    try:
                        if self._get_sandbox_pids(sandbox_name):
                            skipped += total_apps
                            done_count += total_apps
                            self._safe_call_after(
                                self._set_launch_status_if_active,
                                launch_session_id,
                                f"已跳过已运行沙盒 {sandbox_name} ({done_count}/{total})"
                            )
                            continue
                    except Exception:
                        pass
                for app_path, app_dir in app_targets:
                    launch_targets.append((sandbox_name, app_path, app_dir))

            if launch_targets:
                self._safe_call_after(
                    self._set_launch_status_if_active,
                    launch_session_id,
                    f"正在启动第 {batch_start // batch_size + 1} 批，共 {len(launch_targets)} 个启动任务..."
                )

            def _launch_one(target):
                nonlocal success, failed, done_count
                if self._is_launch_session_cancelled(launch_session_id):
                    return
                sandbox_name, app_path, app_dir = target
                launched = self.start_app_in_sandbox(sandbox_name, app_path, app_dir)
                volume_key = self._get_volume_memory_key(sandbox_name)
                if launched and getattr(self, "remember_volume", True) and volume_key in self.sandbox_volume_memory:
                    threading.Thread(
                        target=self._restore_saved_volume_for_sandbox,
                        args=(sandbox_name,),
                        daemon=True
                    ).start()
                with lock:
                    if launched:
                        success += 1
                    else:
                        failed += 1
                    done_count += 1
                    progress = done_count
                self._safe_call_after(
                    self._set_launch_status_if_active,
                    launch_session_id,
                    f"启动进度 {progress}/{total}: {sandbox_name}，{os.path.basename(app_path)}"
                )

            threads = []
            for target in launch_targets:
                if self._is_launch_session_cancelled(launch_session_id):
                    break
                t = threading.Thread(target=_launch_one, args=(target,), daemon=True)
                t.start()
                threads.append(t)
            for t in threads:
                t.join()

            has_more_batches = (batch_start + batch_size) < total_sandboxes
            effective_interval = interval
            if self._is_launch_session_cancelled(launch_session_id):
                break
            if has_more_batches and effective_interval > 0:
                for rem in range(effective_interval, 0, -1):
                    if self._is_launch_session_cancelled(launch_session_id):
                        break
                    self._safe_call_after(
                        self._set_launch_status_if_active,
                        launch_session_id,
                        f"下一批启动等待 {rem} 秒..."
                    )
                    time.sleep(1)
            elif has_more_batches and serial_relief_mode:
                if self._is_launch_session_cancelled(launch_session_id):
                    break
                time.sleep(serial_relief_sleep)

        total_time = time.time() - start_time
        self._log_operation_safe(
            f"启动线程结束: 成功={success}, 失败={failed}, 跳过={skipped}, 总数={total}, 耗时={total_time:.2f}s"
        )
        self._safe_call_after(
            self._launch_done,
            launch_session_id,
            success,
            total,
            skipped,
            total_time,
            failed,
            self._is_launch_session_cancelled(launch_session_id)
        )

    def _launch_done(self, launch_session_id, success, total, skipped, total_time, failed=0, cancelled=False):
        self.is_launching = False
        try:
            if int(launch_session_id or 0) == int(getattr(self, "_launch_session_id", 0) or 0):
                self._launch_cancel_event.clear()
        except Exception:
            pass
        self._active_launch_targets = None
        if self._is_window_closing():
            return
        try:
            self.btn_launch.Enable()
            self.btn_launch.SetLabel("启动选中沙盒")
        except RuntimeError:
            return
        self._safe_call_later(1, self._refresh_visible_sandbox_running_states)
        self._safe_call_later(800, self._refresh_visible_sandbox_running_states)
        self._safe_call_later(2500, self._refresh_visible_sandbox_running_states)
        if cancelled:
            return
        if total <= 0:
            launch_status_text = "就绪"
        else:
            launch_status_text = f"启动完成：成功 {success}/{total}"
            if skipped > 0:
                launch_status_text += f"，跳过 {skipped}"
            if failed > 0:
                launch_status_text += f"，失败 {failed}"
        if launch_status_text:
            self._safe_set_status_label(f"{launch_status_text}。按 F5 可刷新运行状态")
            logger = getattr(self, "_log_operation_safe", None)
            if callable(logger):
                logger(launch_status_text)
        if success > 0:
            self._speak_process_assist(f"已启动{success}个沙盒", event_key="launch_done")

    def on_backup_sandboxes(self, event):
        BackupRestoreMgr(self).backup()

    def on_restore_sandboxes(self, event):
        BackupRestoreMgr(self).restore()
