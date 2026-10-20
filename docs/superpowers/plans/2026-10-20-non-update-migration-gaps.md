# 非更新功能迁移缺口实施计划

> 执行方式：使用 executing-plans 和 test-driven-development，按模块顺序内联执行；每个模块验证、记录和独立提交完成后进入下一项。

## 目标与本轮授权

用户本轮明确要求“先不做更新相关的功能，先做别的，先把这个写入计划文档，把别的先做了”。据此调整执行顺序：暂缓 GitCode 更新源配置、GAP-01 更新结果 UI/下载启动链、GAP-02 Updater 安装入口，先处理 GAP-03 至 GAP-10。该调整只改变任务顺序，不修改项目长期约束或整体完成标准。

本文件是当前唯一执行计划。`2026-10-15-multi-user-navigation-and-exe-audit.md` 保留为历史计划，其剩余真实 EXE/Sandboxie 审计继续保留为未完成项；之前展示的 GitCode 计划未落盘，不作为当前执行入口。

## 技术与边界

- 目标为 .NET Framework 4.8、WinForms、C# 7.3、Release x64；测试复用 MSTest 和 Python unittest。
- 对照仓库内 Python 原版迁移行为、文案、快捷键、焦点、无障碍和配置键。
- 更新地址、清单协议、更新 UI、Updater 安装流程均不进入本轮实现。
- 工作树中已有的 AuditRunner、解决方案/测试工程、HotkeyController、SettingsRuntimeContractTests 和文档改动属于原有工作，保留且不混入本轮提交；涉及同文件时只提交本轮差异。
- 每项先建立运行时 RED，再实施最小修改、取得 GREEN；已有正确行为保留为回归项，不伪造 RED。
- 真实 EXE、剪贴板和 Sandboxie 验证使用隔离副本与精确归属资源；不能确认焦点、句柄或清理时记为无结论。
- 测试计数、提交哈希、PID 和实际日志索引只记录在 PROJECT_STRUCTURE.md；本计划仅维护步骤及边界。

## 顺序、文件与验收场景

### GAP-03：复杂 JSON 配置恢复

参考：`shahe_bulk_launcher/features/settings.py`、`features/app_paths.py`、`features/sandbox_admin.py`、`features/hotkeys.py`。

实现范围：`csharp/ShaheLauncher.Core/Configuration/ConfigStore.cs`、`Settings/LauncherSettingsService.cs`、`SandboxAdmin/SandboxAdminService.cs`、`csharp/ShaheLauncher.App/MainForm.cs` 中经 RED 确认的最小读取边界。

测试：新增 `csharp/ShaheLauncher.Tests/JsonSettingsRuntimeContractTests.cs`；通过真实 ConfigStore 序列化、重新构造、主窗口加载和再次保存验证路径历史的 path/note、分组成员、置顶、音量、自定义快捷键及朗读提示。同时覆盖旧 JSON 配置加载与非 JSON 字典输入，避免修复破坏现有服务契约。

- [x] 建立模块映射和隔离配置 RED。
- [x] 修复已确认的 JSON 类型兼容偏差，定向 GREEN。
- [ ] 完成共用验证门禁、文档同步和独立提交。

### GAP-04：朗读配置保留

参考：`shahe_bulk_launcher/features/speech.py`。范围：`MainForm.cs`、`ShaheLauncher.Core/Speech/SpeechService.cs` 及现有朗读设置对话框；测试新增独立运行时契约文件。

- [ ] 用已有非空朗读配置验证打开、取消、确定各路径，固定 Python 保存回调语义。
- [ ] RED 确认后修复空配置覆盖路径，验证原有语音配置保留。
- [ ] 完成共用验证门禁、文档同步和独立提交。

### GAP-05：剪贴板文件列表优先

参考：`shahe_bulk_launcher/features/app_paths.py`。范围：`MainForm.cs` 及现有 `ResolvePastedAppPath`；测试覆盖文件列表与文本同时存在、只有文本、空剪贴板和无效路径。

- [ ] 建立文件列表优先级与状态反馈 RED。
- [ ] 接入已有解析能力，取得 GREEN，并验证显式粘贴动作。
- [ ] 完成共用验证门禁、文档同步和独立提交。

### GAP-06：全选切换

参考：Python `on_ctrl_a_select` / `toggle_select_all` 与列表右键全选。范围：`MainForm.cs` 的动作分派；复用 CheckableListCtrl 切换能力。

- [ ] 对文件菜单和 Ctrl+A 连续触发两次建立 RED；右键全选保持单向全选。
- [ ] 修复分派，回归部分选择、空列表、文件菜单和快捷键。
- [ ] 完成共用验证门禁、文档同步和独立提交。

### GAP-07：Esc 与退出反馈

参考：`shahe_bulk_launcher/features/hotkeys.py`、`features/lifecycle.py`。范围：`MainForm.cs`、`MainForm.Lifecycle.cs`，优先复用已有状态和朗读回调，不混入用户原有 HotkeyController 改动。

- [ ] 建立 Esc、Alt+F4、退出菜单对应的状态/读屏反馈 RED。
- [ ] 对齐 Python 文案、反馈顺序和窗口生命周期，回归托盘与重复关闭。
- [ ] 完成共用验证门禁、文档同步和独立提交。

### GAP-08：Sandboxie repair reload

参考：`shahe_bulk_launcher/features/sandboxie.py`。范围：`MainForm.cs`、现有 SandboxieClient 和修复回调。

- [ ] 建立正常构造路径 reload 接线 RED，验证修复完成后的调用次数、取消和失败反馈。
- [ ] 复用现有 `/reload` 执行入口，取得 GREEN；真实验证不修改用户沙盒资源。
- [ ] 完成共用验证门禁、文档同步和独立提交。

### GAP-09：路径记忆数量范围

参考：Python 配置加载和设置对话框。范围：`MainForm.cs`、现有设置服务/对话框中由 RED 确认的范围差异。

- [ ] 固定 -1、0、20、21、999 和无效值在加载/保存/UI 路径的 Python 结果。
- [ ] 恢复 0–20 的范围与原版无效值处理，取得 GREEN。
- [ ] 完成共用验证门禁、文档同步和独立提交。

### GAP-10：路径选择与剪贴板文案

参考：`shahe_bulk_launcher/features/app_paths.py`。范围：`MainForm.cs` 的原生路径对话框与剪贴板提示。

- [ ] 建立标题、过滤器、成功/失败提示的原文契约 RED。
- [ ] 逐项对齐文案与选择行为，保持键盘和 AccessibleName 契约。
- [ ] 完成共用验证门禁、文档同步和独立提交。

## 每项共用验证门禁

日志和临时文件归入 `artifacts/runs/csharp_non_update_gaps_20261020/<gap>/`，隔离构建使用该 gap 目录下的 `bin/` 作为绝对 BaseOutputPath。每条命令结束后先更新 PROJECT_STRUCTURE.md，再运行下一条；RED 的失败必须是目标行为断言失败，编译/环境失败不算有效 RED。

1. 定向 RED/GREEN：`dotnet test csharp\ShaheLauncher.Tests\ShaheLauncher.Tests.csproj --configuration Release --no-restore --property:Platform=x64 --filter FullyQualifiedName~<本模块测试类>`，追加本模块的绝对 BaseOutputPath 与 TRX 结果目录。
2. 受影响回归：同一测试项目按本模块涉及的 Application、SettingsService、SandboxAdmin、AppPaths、Speech、Controls 或 Lifecycle 分类筛选。
3. C# 全量：`dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64`，追加本模块隔离 BaseOutputPath。
4. Python 对照：`python -m unittest discover -s tests -p "test_*.py" -v`；新增模块行为探针只在隔离数据上运行。
5. 生产源码编译：`python -m compileall -q shahe_bulk_launcher tests`。
6. Release：`dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64`；输出锁定时记录失败文件/进程，再使用隔离 BaseOutputPath。
7. 差异：`git diff --check`；检查本轮暂存 diff，确认用户已有改动未被带入。
8. 对有交互的模块补真实 EXE 验证；环境阻断单列，不以单元测试代替真实审计。
9. 更新当前状态、模块映射和追加验证记录，核对本计划步骤，再独立提交并推送；分支分叉时使用独立远端任务分支，不强推。

## 完成标准与剩余风险

- 每个已完成非更新缺口具备 Python 对照、有效 RED/GREEN、受影响/全量验证、构建、文档和独立提交证据。
- 未取得真实 EXE 证据的部分继续标记待审计；更新功能与旧计划的未完成真实审计保留。
- 本轮范围完成不等于整个软件迁移完成；C# 是否可以完整运行按最新证据单独报告。
- GitCode 账号/仓库/清单地址、原 Updater 源码、真实屏幕阅读器审计、发布与破坏性恢复验证仍是后续边界。
