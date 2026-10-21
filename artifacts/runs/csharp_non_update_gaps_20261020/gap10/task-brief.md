# GAP-10 实施任务

继续已批准的迁移计划中的单个模块 GAP-10。工作目录 D:\mycodex\git，基准 HEAD cd8b3e147d946d87d49a2630fcb2a93d04d2474b。用户要求对照 Python 查缺并继续迁移，所有原文案保留，C# 实现。

## 规格

- Python reference: shahe_bulk_launcher/features/app_paths.py。
- 生产修改范围：csharp/ShaheLauncher.App/MainForm.cs；仅当 RED 必需时修改 csharp/ShaheLauncher.Core/AppPaths/AppPathHistoryService.cs。
- 新测试：csharp/ShaheLauncher.Tests/AppPathDialogTextRuntimeContractTests.cs。复用 MigrationRuntimeTestHelpers 和现有 ClipboardPriorityRuntimeContractTests 等。
- RED 固定标题“选择要在沙盒中打开的应用”，过滤器“可执行文件 (*.exe)|*.exe”，无所有文件。
- 对齐选择成功、剪贴板访问失败、粘贴异常、复制后回读确认及失败反馈；逐行以 Python 原版为准。
- 保持 Ctrl+O、Ctrl+V、焦点恢复、TabIndex 和 AccessibleName。
- 不更改 Python、不触碰用户剪贴板（测试可替换 reader/writer）、不修改用户已有无关文件。用户已有 HotkeyController、SettingsRuntimeContractTests、sln、csproj、AuditRunner 和大量 artifacts 改动。
- 路径历史最大 999 是用户有意指定，与原 Python 20 不同，不回退。

## 执行与证据

读取 AGENTS.md；不要修改 AGENTS、迁移计划或约束。你负责本模块映射、RED、最小实现、GREEN、受影响回归、全量、Python 对照/compileall 和 Release x64。将产物限定在 artifacts/runs/csharp_non_update_gaps_20261020/gap10/；尽量复用此前模块 run-verification.ps1 等工具，避免新框架。隔离 BaseOutputPath 使用本目录 bin/ 的绝对路径。

每次测试/探针/构建结束后，立即把实际命令、返回码、通过/失败/跳过数和日志路径追加到 PROJECT_STRUCTURE.md，再进行下一次验证。控制者同时仅做只读源码审计，当前由你单独负责 PROJECT_STRUCTURE.md 的验证追加。

全量可能碰到既有 AuditRunner 原生输入不稳定；必须保留失败证据、核查后有效重跑，不修改无关审计器。不得把工具错误计为有效 RED。

不提交/推送/暂存，不改计划复选框；控制者将完成真实 EXE 验证、审查、提交前文件清单及计划变更确认。先完成代码与自动化门禁并向控制者交接。报告最终文件清单、最小差异、实际验证结果及尚未验证的边界；用简短结果消息和本目录 implementation-report.md。
