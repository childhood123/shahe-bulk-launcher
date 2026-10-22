# REVIEW-06 更新检查/启动反馈映射

Python `updates.py::on_check_update` 手动检查先记录操作、隐藏旧更新提示、写入「正在检查更新...」，再启动后台检查。Python `_launch_updater` 在准备、不可用、启动成功和异常四条路径分别写状态、日志和保留文案。
C# `ExecuteApplicationAction("check_update")` 目前直接启动 `CheckUpdateWorker(false, true)`；`LaunchUpdate` 只处理准备错误/Process.Start 异常，缺少准备、启动成功和 Python 对应失败状态/提示。
本轮复用现有 UpdateService、MainForm 的 `ShowUpdateNotice`/`ShowMessageCallback` 和 `Runtime.AppendDailyLog`，不改下载、安装、回滚协议。测试隔离为非生产 updater 命令准备错误、手动检查状态和成功启动前后状态回调；不启动真实 updater。
