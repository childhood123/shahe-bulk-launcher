# Python→C# 剩余迁移收口计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. 每个模块必须独立完成映射、RED、GREEN、回归、文档、提交和推送。

**Goal:** 收口全部已确认的 Python→C# 行为差异、运行时接线、真实 EXE 审计和发布流程，最终满足 `AGENTS.md` 的整体迁移完成标准。

**Architecture:** 保持现有 Core、App、Updater 和 AuditRunner 边界，不重设计 UI。先处理非更新代码差异，再处理读屏配置集成和真实审计；更新子系统继续暂缓，直到用户明确恢复更新工作。

**Tech Stack:** .NET Framework 4.8、WinForms、C# 7.3、Release x64、MSTest、Python unittest、Sandboxie、Tolk。

## 新会话入口

新会话严格按以下顺序读取：

1. `AGENTS.md`
2. `PROJECT_STRUCTURE.md` 顶部“当前执行摘要”
3. 本计划的“当前状态”和“严格执行顺序”
4. 当前模块对应的最新 `artifacts/runs/` 日志
5. 当前模块的 Python 参考实现和 C# 当前入口

不要从历史计划继续执行，也不要根据历史测试数字判断当前状态。本计划中第一个未完成模块是唯一当前模块；实时测试数字、PID、提交哈希和日志路径只以 `PROJECT_STRUCTURE.md` 为准。

## 当前状态

| 状态 | 模块 |
| --- | --- |
| 已完成，不重做 | Task 1–20 已确认范围、GAP-03 至 GAP-06、VOICE-01 |
| 当前模块 | GAP-07 Esc、Alt+F4 与退出反馈 |
| 后续代码模块 | GAP-08、GAP-09、GAP-10、VOICE-02 |
| 真实审计 | AUDIT-01 至 AUDIT-08 |
| 暂缓更新 | UPDATE-01、GAP-01、GAP-02 |
| 最终收口 | RELEASE-01 |

当前 C# 主程序能够启动、显示主窗口、执行已验证功能并正常退出；整体迁移尚未完成。唯一确认的空生产入口是 `csharp/ShaheLauncher.Updater/Program.cs`。

## 严格执行顺序

1. GAP-07 退出反馈
2. GAP-08 Sandboxie repair reload
3. GAP-09 路径记忆数量范围
4. GAP-10 路径选择与剪贴板文案
5. VOICE-02 读屏配置与 `lib` 运行时接线
6. AUDIT-01 至 AUDIT-08 真实审计
7. UPDATE-01、GAP-01、GAP-02 更新子系统
8. RELEASE-01 最终发布与整体完成门禁

禁止跳过当前模块，也禁止把多个模块混合迁移后统一测试。更新功能继续暂缓，除非用户明确恢复更新工作。

## 已完成且不得重复

- Python 20 个模块对应的 C# 类型、服务和主窗口组装已完成已确认范围。
- `Program.cs`、`MainForm`、单实例和 WinForms 生命周期已接入真实启动流程。
- GAP-03 复杂 JSON、GAP-04 朗读配置保留、GAP-05 剪贴板文件优先、GAP-06 全选切换已完成并推送。
- VOICE-01 Tolk x64 已完成 ZDSR、NVDA、OneCore 回退和生产 `SpeechService` 原生调用验证。
- 关闭当前/全部进程真实验证已完成。
- `batch-create`、`create-group`、`launch-group`、`schedule-launch` 和 cleanup 已取得真实证据。
- 真实备份和独立安全解压一致性验证已完成；真实覆盖还原仍未完成。

## 每个模块的固定循环

1. 读取 Python 参考实现和 C# 当前入口。
2. 建立模块映射和目标行为清单。
3. 写运行时失败测试并取得有效 RED；编译错误或测试工具错误不算 RED。
4. 立即把 RED 结果追加到 `PROJECT_STRUCTURE.md`。
5. 实施最小修复。
6. 取得定向 GREEN，并立即同步文档。
7. 运行受影响回归、C# 全量、Python 全量、compileall、Release x64 和差异检查。
8. 有交互的功能补真实 EXE 证据；无效焦点、句柄或超时证据标记为 inconclusive。
9. 重新读取本计划，核对完成项和风险。
10. 展示计划复选框拟修改差异并取得用户确认。
11. 明确列出并只暂存当前模块文件。
12. 独立提交并推送，报告哈希、远端分支和 `git revert` 命令。
13. 完成后才进入下一个模块。

---

### GAP-07: Esc、Alt+F4 与退出反馈

**Python reference:** `shahe_bulk_launcher/features/hotkeys.py`、`features/lifecycle.py`

**Files:**
- Modify: `csharp/ShaheLauncher.App/MainForm.cs`
- Modify only if RED requires: `csharp/ShaheLauncher.App/MainForm.Lifecycle.cs`
- Create: `csharp/ShaheLauncher.Tests/ExitFeedbackRuntimeContractTests.cs`

- [ ] 建立 Esc、固定 Alt+F4、文件菜单退出的运行时 RED。
- [ ] 固定状态文案：“已最小化到托盘”“当前托盘不可用，已最小化窗口”“正在退出软件”。
- [ ] 固定 `SpeechCallback(text, "launch_done")` 调用次数和先后顺序。
- [ ] 复用现有 `MinimizeToTray()` 和关闭生命周期，不修改用户已有 `HotkeyController.cs`。
- [ ] 回归托盘关闭隐藏、ForceExit、重复关闭、Tolk 卸载和无托盘场景。
- [ ] 完成共用门禁、真实键盘/菜单验证、文档、独立提交和推送。

### GAP-08: Sandboxie repair reload

**Python reference:** `shahe_bulk_launcher/features/sandbox_admin.py::_repair_sandboxie_control_user_settings`

**Files:**
- Modify: `csharp/ShaheLauncher.App/MainForm.cs`
- Create: `csharp/ShaheLauncher.Tests/SandboxieRepairReloadRuntimeContractTests.cs`

- [ ] RED 证明正常构造路径的 `ReloadSandboxieConfigCallback` 当前为 null。
- [ ] 接线已有 `ReloadSandboxieConfig()`；不得新增第二套 `/reload` 命令实现。
- [ ] 验证只有实际写入配置后调用一次 reload。
- [ ] 验证取消、无需修复、写入失败和 reload 异常反馈。
- [ ] 真实验证只使用隔离 INI，不修改用户 Sandboxie 资源。
- [ ] 完成共用门禁、文档、独立提交和推送。

### GAP-09: 路径记忆数量范围

**Python reference:** `features/settings.py`、`features/app_paths.py`、`ui/settings_dialogs.py`

**Files:**
- Modify: `csharp/ShaheLauncher.App/MainForm.cs`
- Create: `csharp/ShaheLauncher.Tests/PathHistoryLimitRuntimeContractTests.cs`

- [ ] 对 `-1`、`0`、`20`、`21`、`999` 和无效值建立 Python/C# 对照 RED。
- [ ] 将 `MainForm.LoadApplicationSettings()` 的范围从 `0–999` 恢复为 `0–20`。
- [ ] 回归 `LauncherSettingsService`、管理对话框、保存重载和历史裁剪。
- [ ] 真实 EXE 验证 `0`、普通值和 `20` 的保存及重启恢复。
- [ ] 完成共用门禁、文档、独立提交和推送。

### GAP-10: 路径选择与剪贴板文案

**Python reference:** `shahe_bulk_launcher/features/app_paths.py`

**Files:**
- Modify: `csharp/ShaheLauncher.App/MainForm.cs`
- Modify only if RED requires: `csharp/ShaheLauncher.Core/AppPaths/AppPathHistoryService.cs`
- Create: `csharp/ShaheLauncher.Tests/AppPathDialogTextRuntimeContractTests.cs`

- [ ] RED 固定对话框标题“选择要在沙盒中打开的应用”。
- [ ] RED 固定过滤器“可执行文件 (*.exe)|*.exe”，不额外显示所有文件。
- [ ] 对齐选择成功、剪贴板访问失败、粘贴异常和复制后回读确认。
- [ ] 保持 Ctrl+O、Ctrl+V、焦点恢复、TabIndex 和 AccessibleName。
- [ ] 完成真实文件选择/剪贴板验证、共用门禁、文档、独立提交和推送。

### VOICE-02: 读屏参数与 lib 运行时配置

**Risk:** 设置页保存到 `Configuration/ZDSRAPI.ini` 与 `Configuration/boyctrl.ini`；新 Tolk 运行时携带 `lib/ZDSRAPI.ini` 与 `lib/byctrl.conf`，实际读取路径和新版保益配置格式尚未确认。

**Files:**
- Inspect/modify after RED: `csharp/ShaheLauncher.Core/Speech/SpeechService.cs`
- Inspect: `csharp/ShaheLauncher.App/lib/ZDSRAPI.ini`
- Inspect: `csharp/ShaheLauncher.App/lib/byctrl.conf`
- Create: `csharp/ShaheLauncher.Tests/SpeechRuntimeConfigContractTests.cs`

- [ ] 使用隔离 `lib` 和 `Configuration` 分别修改参数，探针确认厂商 DLL 实际读取位置。
- [ ] 固定当前设置页保存结果是否影响真实 ZDSR 和保益输出。
- [ ] 对照 `boyctrl.ini` 的 Param1–4 与新版 `byctrl.conf` 键，记录可映射和不可映射项。
- [ ] 未确认真实读取行为前，不向安装目录写入运行时配置。
- [ ] 若需要改变运行时目录或配置格式，先形成独立设计并展示给用户确认。
- [ ] 完成真实 ZDSR/保益验证、共用门禁、文档、独立提交和推送；无法验证的保益环境单列风险。

---

### AUDIT-01: 定位当前进程

- [ ] 验证无窗口、单窗口、多窗口选择对话框、最小化窗口和失效句柄。
- [ ] 记录目标 PID、窗口句柄、选择结果、状态文案和清理结果。

### AUDIT-02: 路径记忆完整真实交互

- [ ] 在 GAP-03 修复后的真实 JSON 数组历史上重新验证启动恢复。
- [ ] 验证备注、删除当前、清空全部和经典上下文菜单键盘入口。
- [ ] 已通过的粘贴、选择、勾选、数量保存和隐藏路径不重复测试，除非受后续修改影响。

### AUDIT-03: Sandboxie 剩余生命周期

- [ ] `delete-group` 成功路径。
- [ ] `schedule-close`；SbieCtrl 存在时保持阻断，不擅自关闭用户进程。
- [ ] `cancel-schedules` 有真实任务场景。
- [ ] `batch-rename` 和 `batch-delete`。
- [ ] 实现只读 `verify-clean`，复核精确所有权、配置哈希、section、目录和进程清零。

### AUDIT-04: 键盘、托盘与无障碍

- [ ] Alt+Up/Alt+Down 启用态。
- [ ] NumPadEnter 真实底层键码。
- [ ] Enter 进入分组和 Backspace 返回的成功路径。
- [ ] 完整屏幕阅读器朗读、菜单助记键、焦点恢复和 AccessibleName。

### AUDIT-05: 多用户导航与刷新竞争

- [ ] 完成空闲、1/2/4 用户及每用户 1/5 进程矩阵。
- [ ] 记录单键中位数、最大值、20 键突发、超时、丢键和索引。
- [ ] 计数核对 refresh worker、过期 timer 和菜单重建；没有稳定 RED 不修改定时机制。

### AUDIT-06: 备份与还原

- [ ] 保留已通过的真实备份与独立解压证据。
- [ ] 只在专用 VM/测试机执行真实覆盖还原、服务停止/重启和配置合并。
- [ ] 普通用户环境不得执行破坏性还原。

### AUDIT-07: 发布目录与真实启动

- [ ] 从干净 checkout 生成独立 Release x64 发布目录。
- [ ] 核对主 EXE、updater.exe、Tolk `lib` 七文件、配置和资源。
- [ ] 完成启动、显示主窗口、菜单操作、正常退出和无残留进程。

### AUDIT-08: 当前行为总对照

- [ ] 逐项核对 Python/C# 中文文案、菜单、快捷键、焦点顺序、AccessibleName 和配置键。
- [ ] 生成最终差异清单；新增差异必须单独 RED/GREEN，不能直接并入最终提交。

---

## 更新子系统：继续暂缓

本节只记录，未获用户明确恢复更新工作的指令前不得实施。

### UPDATE-01: GitCode 更新源

- [ ] 用户提供 GitCode 仓库、正式清单 URL、分支/tag 和发布约定。
- [ ] 替换当前 `https://lqh.viyf.net/shahe-bulk-launcher/update.json`。
- [ ] 验证缓存、签名、哈希、超时、错误响应和回退规则。

### GAP-01: 更新结果 UI 和下载链

**Confirmed gaps:** `UpdateNoticePanel` 已创建但没有结果接线；`DownloadUpdateCallback` 正常构造未赋值；`CheckUpdateWorker()` 只设置状态文本。

- [ ] 建立版本结果、面板显示/隐藏、稍后下载和强制更新 RED。
- [ ] 接线 `UpdateNoticePanel`、`UpdateNoticeLabel`、`DownloadUpdateButton` 和 `DownloadUpdateCallback`。
- [ ] 接线 updater 命令准备、进程启动、主程序关闭和错误反馈。

### GAP-02: Updater 安装入口

**Confirmed gap:** `csharp/ShaheLauncher.Updater/Program.Main` 为空；`UpdateInstaller` 目前只有参数、请求和哈希 helper。

- [ ] 获取原 updater 源码；没有源码时必须由用户明确批准按现有请求协议重建。
- [ ] RED 覆盖等待主 PID、下载、大小/哈希/签名、解密、解压、原子替换、回滚和重启。
- [ ] 只在隔离发布副本执行端到端更新，不直接覆盖仓库或用户正式安装目录。
- [ ] 完成 GitCode 清单到新版本启动的完整真实验证。

---

### RELEASE-01: 整体迁移完成门禁

- [ ] 所有代码 GAP、VOICE 和 AUDIT 项均完成或有用户接受的外部阻断。
- [ ] 更新源、更新 UI、Updater 和发布目录完整验证。
- [ ] C# 定向测试、C# 全量、Python 全量、compileall、Release x64、差异检查全部通过。
- [ ] 干净发布目录能够启动、显示主窗口、执行核心工作流并正常退出。
- [ ] Sandboxie 审计资源、配置、目录和进程全部恢复基线。
- [ ] `PROJECT_STRUCTURE.md` 写明最终状态、证据和遗留风险。
- [ ] 创建最终迁移提交；只有满足 `AGENTS.md` 全部条件后才能声明整体迁移完成。

## 共用验证命令

每个模块使用独立 `artifacts/runs/<模块名>/bin/` 作为绝对 `BaseOutputPath`：

```powershell
dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64
python -m unittest discover -s tests -p "test_*.py" -v
python -m compileall -q shahe_bulk_launcher tests
dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64
git diff --check
git diff --cached --check
```

## 新会话恢复规则

- 用户说“继续工作”时，从本计划第一个未完成模块开始；当前首先执行 GAP-07。
- 如果模块已有 RED 没有 GREEN，从修复阶段继续。
- 如果代码和验证完成但未提交，先做计划核对、暂存审计、提交和推送。
- 如果真实探针为 inconclusive，保留证据并停在该模块，不写成产品通过或失败。
- 历史计划只用于追溯，不覆盖本计划和 `PROJECT_STRUCTURE.md` 顶部摘要。
- 更新模块必须等待用户明确恢复更新工作。

## 当前阻断与风险

- 原 updater 源码尚未提供。
- GitCode 仓库和正式清单地址尚未确定。
- SbieCtrl 运行会阻断 schedule-close 的安全执行。
- 真实覆盖还原需要专用 VM/测试机。
- Python 3.14 环境缺少 `cryptography`，Python 加密真实执行只能记录环境阻断。
- VOICE-02 的厂商配置读取路径和新版保益配置映射尚无真实证据。
- 工作树含用户已有改动，所有提交必须选择性暂存。
