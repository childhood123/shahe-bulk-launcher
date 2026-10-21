# REVIEW-03 映射

- Python `features/hotkeys.py::on_hotkey_refresh_process_state`：不刷新沙盒列表，只刷新可见运行状态；按是否有目标给出“正在刷新进程状态”或“没有可刷新的进程状态”，异步完成后给出“已刷新进程状态”。
- C# 原 `refresh_process_state` 分派调用 `RefreshProcessUi(true)`，会触发完整列表刷新，并没有三段状态反馈。
- 修复复用现有 `CheckableListCtrl.RefreshVisibleRunningStates` worker，增加完成回调；不改列表刷新 worker 的并发/取消模型。
