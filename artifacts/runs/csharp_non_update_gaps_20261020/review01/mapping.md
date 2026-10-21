# REVIEW-01 映射

- Python `features/scheduling.py::_prepare_launch_target_from_app_targets`：应用文件、Sandboxie 安装目录和 `Start.exe` 均在启动会话建立前校验，失败时保持原中文状态。
- Python `features/scheduling.py::_resolve_launch_entries`：普通启动入口先把显示名/内部名映射到当前有效沙盒集合；无有效目标不进入启动会话。
- C# `MainForm.StartLaunch` 原先直接调用 `LaunchScheduler.BeginLaunch`，绕过已有 `PrepareLaunchTargetFromAppTargets`，也未在直接按钮/快捷键入口统一解析目标。
- 修复复用现有 Core 校验和 `LaunchScheduler.ResolveLaunchEntries`，保留 `Ctrl+Enter`、按钮、分组/定时入口和既有状态文案。
