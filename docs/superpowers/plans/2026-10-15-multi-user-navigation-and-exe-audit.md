# 多用户导航卡顿与真实 EXE 审计实施计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking. 本计划采用当前会话内联执行，不启动子代理。

**Goal:** 完成启动多个 Sandboxie 用户后用户列表上下浏览不流畅的剩余根因验证和修复，并继续完成真实 EXE 与 Sandboxie 集成审计；全部迁移门禁满足前，不得声明整个软件迁移完成。

**Architecture:** 保持 `MainForm`、`CheckableListCtrl`、`HotkeyController`、`ProcessActionService` 和 `SandboxieClient` 的职责边界。先用 WinForms STA 契约测试冻结可复现行为，再使用隔离 Release x64 副本进行真实导航和 Sandboxie 测量。日志和探针结果统一写入 `artifacts/runs/`。

**Tech Stack:** .NET Framework 4.8、WinForms、C# 7.3、MSTest、Release x64、PowerShell、Sandboxie、Python 现有基线测试。

## Global Constraints

- 遵守 `AGENTS.md` 的模块映射、逐模块验证、文档同步、独立提交和状态声明规则。
- 当前任务属于真实 EXE 与 Sandboxie 集成审计，不得把契约测试通过写成整个软件完成。
- 保留现有 UI、中文文案、快捷键、焦点顺序、AccessibleName、配置键和菜单行为。
- 只修复已有 RED 测试或真实证据确认的性能回归，不重新设计界面。
- 不回退、覆盖或暂存用户已有的无关改动。
- 运行中的 EXE 可能锁定默认输出；优先使用隔离 `BaseOutputPath`，未经用户允许不停止活动进程。
- 临时测试文件、探针、日志、截图和结构化结果统一放入 `artifacts/runs/<本轮名称>/`。
- 每个行为修复必须先有 RED 证据，再实现最小修复，再运行定向、全量和真实 EXE 验证。
- `PROJECT_STRUCTURE.md` 的历史记录只追加；当前状态只维护在文件顶部摘要和当前计划表。

## 当前状态

- 当前日期：2026-10-15。
- 当前分支：`沙河批量打开器`。
- 最新已提交提交：`0d454ec fix: reduce live process menu navigation scans`。
- 最近一次完整自动化门禁：C# `211/211`、Python `29/29`、Release x64 构建 `0` 警告/`0` 错误、Python 生产源码编译通过、`git diff --check` 通过。
- 当前工作树新增了 `ProcessMenuRuntimeContractTests.MovingSelectionSchedulesOneLiveRefreshRequest`；已完成有效 RED、最小修复、隔离 GREEN、受影响回归和 C# 全量回归。
- 当前运行实例 PID：`10444`，可能锁定默认 Release 输出中的 `ShaheLauncher.Core.dll`。
- 已确认的性能优化：移除选择路径重复可见状态扫描、共享 Sandboxie PID 快照、合并实时 worker、将全局 PID 扫描移出 UI 线程、移除菜单栏同步 PID 扫描和未使用的设备枚举。
- 未确认的问题：本轮修复后的真实 EXE 复测尚未执行；既有突发导航结果中出现过 `750 ms` 超时或约 `2 s` 未到达目标，但部分结果同时存在焦点校准或索引漂移，不能直接作为产品结论。
- C# 当前可以启动、显示主窗口并执行已验证功能，但真实 EXE 与 Sandboxie 集成审计仍未完成。

## Task 1: 新会话基线

**Files:** `AGENTS.md`、`PROJECT_STRUCTURE.md`、本计划、相关 C# 源码和测试（只读）。

- [x] 读取 `AGENTS.md`、`PROJECT_STRUCTURE.md`、本计划和本轮 `artifacts/runs/` 日志。
- [x] 运行 `git status --short` 和 `git log -8 --oneline --decorate`，记录用户已有改动，不执行回退或清理。
- [x] 运行 `Get-Process -Name '沙河批量打开器' -ErrorAction SilentlyContinue | Select-Object Id,ProcessName,Responding,CPU,WorkingSet,StartTime`，记录活动 PID。
- [x] 建立 `artifacts/runs/csharp_navigation_followup_20261015/`，后续所有本轮输出写入此目录。

**Completion criterion:** 当前 HEAD、工作树、活动进程、计划和日志目录均已确认，且没有回退用户改动。

## Task 2: 执行当前 RED 测试

**Files:** `csharp/ShaheLauncher.Tests/ProcessMenuRuntimeContractTests.cs`、`artifacts/runs/csharp_navigation_followup_20261015/`。

- [x] 确认 `MovingSelectionSchedulesOneLiveRefreshRequest` 包含“选中第 0 项、移动到第 1 项、刷新请求增量必须为 1”的断言，并修正测试为显式触发旧项取消选中和新项选中事件。
- [x] 使用隔离构建：`dotnet msbuild csharp\ShaheBulkLauncher.sln /t:Build /p:Configuration=Release /p:Platform=x64 /p:BaseOutputPath=<本轮 isolated-bin> /m`，输出保存为 `isolated-build.log`。
- [x] 使用隔离测试程序集运行 `MovingSelectionSchedulesOneLiveRefreshRequest`；修正后的 RED 输出保存为 `selection-refresh-count-red2.log`。
- [x] 记录实际刷新次数：修正测试后当前实现为 `2` 次，预期为 `1` 次；初版未触发事件得到 `0` 次的结果保留为无效测试证据。

**Completion criterion:** 测试已真实执行，并记录返回码、通过/失败结果、实际刷新次数和日志路径。

## Task 3: 修复一次移动产生多次刷新请求

**Files:** `csharp/ShaheLauncher.App/UI/Controls/CheckableListCtrl.cs`、`csharp/ShaheLauncher.Tests/ProcessMenuRuntimeContractTests.cs`。

- [x] 仅在 Task 2 确认 RED 后，调整 `OnSelectedIndexChanged`：先取得 `GetSelectedIndex()`，只有 `index >= 0` 时调用 `SelectionProcessStateRefreshCallback` 和 `AnnounceItemState(index)`。
- [x] 重新运行单项测试，输出保存为 `selection-refresh-count-green.log`。
- [x] 运行 `TestCategory=Application|TestCategory=Controls|TestCategory=Lifecycle`，输出保存为 `selection-refresh-affected-regression.log`。

Expected: 定向测试和受影响分类均为 `0` 失败、`0` 跳过，选择、勾选、菜单状态和无障碍契约无回归。

**Completion criterion:** RED 转 GREEN，且没有修改与选择刷新无关的模块。

## Task 4: 验证过期计时器和菜单重建竞争

**Files:** `MainForm.Lifecycle.cs`、`MainForm.cs`、`ManagedDelayedCall.cs`、`ProcessMenuRuntimeContractTests.cs`。

- [ ] 运行现有 `ProcessMenuLiveStateRefreshCoalescesWhileWorkerIsRunning`，确认已有 worker 运行时不会启动并行扫描。
- [ ] 记录 `OnSelectedIndexChanged -> ScheduleSandboxSelectionProcessStateRefresh -> FlushSandboxSelectionProcessStateRefresh -> ScheduleProcessMenuLiveStateRefresh -> StartProcessMenuLiveStateRefresh -> FinishProcessMenuLiveStateRefresh -> ScheduleMenuBarRebuild -> FlushMenuBarRebuild -> CreateMenuBar` 的调用次数。
- [ ] 只有确认过期回调或重复菜单重建时，才新增固定计数的 RED 契约并修改定时器或 token；没有稳定 RED 证据时不修改 `ManagedDelayedCall`、刷新 token 或菜单重建机制。

**Completion criterion:** 明确记录该假设被确认或排除，不以猜测代替计数证据。

## Task 5: 真实多用户导航测量

**Files:** 现有导航探针、隔离 Release x64 副本、`artifacts/runs/csharp_navigation_followup_20261015/`。

- [ ] 测量空闲、1 个运行用户、2 个运行用户、4 个运行用户，以及每个用户 1 个进程和 5 个进程。
- [ ] 每个场景记录单键中位数、最大值、20 键突发耗时、超时、丢键、起始/目标/最终索引和实际 PID。
- [ ] 起始索引漂移、焦点未确认、列表句柄失效或只有跨进程消息超时而没有应用事件证据时，丢弃该结果，不写成产品结论。
- [ ] 清理本轮 PID 和临时文件，确认 `Get-Service -Name SbieSvc | Select-Object Status,Name` 返回 `Running`；不删除用户已有沙盒，不停止服务。

**Completion criterion:** 至少取得一轮无索引漂移、无焦点校准异常的有效测量；有效 `>300 ms` UI 延迟继续进入根因定位，否则记录为探针问题。

## Task 6: 剩余真实 EXE 与 Sandboxie 审计

按顺序逐项完成，每项完成后立即更新 `PROJECT_STRUCTURE.md`：

- [ ] 关闭当前进程：对照 Python `_terminate_pids`、`on_close_current_sandbox` 和 C# `TerminatePids`、`OnCloseCurrentSandbox`。
- [ ] 关闭全部进程：只使用专用审计沙盒，验证状态栏、运行标记、菜单状态和 PID 清理。
- [ ] 定位当前进程：验证单窗口、多窗口、无窗口和窗口选择对话框。
- [ ] 路径记忆：验证新增、选择、勾选、备注、隐藏、删除、清空和管理对话框保存。
- [ ] 分组和批量管理：逐项审计 `create_group`、`launch_group`、`delete_group`、`batch_create_sandboxes`、`batch_rename_sandboxes`、`batch_delete_sandboxes`。
- [ ] 定时动作：逐项审计 `schedule_launch`、`schedule_close` 和 `cancel_scheduled_tasks`。
- [ ] 托盘、键盘和无障碍：验证托盘生命周期、方向键、左右导航开关、`Alt+Up`、`Alt+Down`、`F5`、`Enter`、`Backspace`、`Space`、焦点顺序和 AccessibleName。
- [ ] 备份还原：只在隔离环境执行真实覆盖类动作；普通环境只做非破坏性备份和安全解压验证。

**Completion criterion:** 每个动作都有 Python 映射、C# 入口、测试结果和真实 EXE 证据，或明确记录为环境风险；未完成项继续保留在待验证列表。

## Task 7: 文档、最终门禁和独立提交

**Files:** `PROJECT_STRUCTURE.md`、本轮实际修复涉及的 C# 文件、对应契约测试、本轮 `artifacts/runs/` 目录。

- [x] 只更新顶部当前摘要和当前计划表；测试数字、当前模块、真实 EXE 证据、待验证项和风险必须与日志一致；历史记录只追加。
- [x] 运行 C# 受影响分类和全量测试，记录实际通过数、失败数和跳过数。
- [x] 运行 `python -m unittest discover -s tests -p "test_*.py" -v`、`python -m compileall -q shahe_bulk_launcher tests`、标准 Release 构建并记录活动进程锁定失败、隔离 Release 构建和 `git diff --check`。
- [x] 提交前只暂存本轮实际修复、测试和文档；未暂存用户原有 `HotkeyController.cs`、`SettingsRuntimeContractTests.cs`、临时备份或历史输出。
- [x] 使用提交消息 `fix: reduce duplicate selection refresh requests`；提交哈希为 `816a797`，本地分支为 `沙河批量打开器`。
- [x] 远端历史分叉时使用 `git push origin HEAD:fix/multi-user-navigation-20261015`，代码修复提交 `816a797` 已推送到该远端分支，未使用强制推送；文档同步提交 `267494d` 已在本地提交，但后续推送因 GitHub 连接超时未确认成功。

**Completion criterion:** 代码、测试、文档、构建、全量回归和真实 EXE 证据与日志一致；仍有未完成审计时，最终报告明确列出当前模块、待验证项、风险和 C# 是否可完整运行。
