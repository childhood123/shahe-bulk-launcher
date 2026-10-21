# REVIEW-04 关闭完成反馈映射

Python `features/process_actions.py::_finish_close_current_sandbox/_finish_close_all_sandboxes` 对应 C# `ProcessActionService.OnCloseCurrentSandbox/PostCloseStatus`。
成功关闭当前用户：原状态文案，Speak「已关闭{显示名}进程」/close_current，然后完整刷新。
成功关闭全部：原状态文案，Speak「已关闭全部进程」/close_all，然后完整刷新。
no_target：保留各自无目标文案，不朗读，RefreshProcessUi(false)。
partially_closed：保留部分关闭文案，不朗读，完整刷新；closed_some 的全部关闭分支仍按 Python 朗读。
验证使用真实服务入口与后台完成投递，假 PID 和已有 OS 回调隔离，不关闭真实程序。先定向 RED，再 GREEN、受影响、全量、Python、compileall、隔离 Release 和差异检查，每次立即记录 PROJECT_STRUCTURE。
