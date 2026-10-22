# REVIEW-03 状态完成保护映射

Python `CheckableListCtrl._finish_running_states_refresh` 在清理 pending 后只在状态文本仍为「正在刷新进程状态」时设置「已刷新进程状态」。C# `MainForm.OnRunningStatesRefreshCompleted` 目前无当前状态判断，会覆盖启动/关闭/选择等后续反馈。
最小修复是在现有 pending 清理后比较 `StatusLabel.Text.Trim()`，不增加 token 或新状态机。测试保留正常完成、空状态和“用户后续状态”三条实际 WinForms 入口。
