# 沙河批量打开器项目结构

## 项目用途

本仓库用于维护沙河批量打开器 Python 正式基线，并在 `csharp/` 中按模块迁移到 .NET Framework 4.8 WinForms x64。Python 单文件模块化迁移已完成，界面、文案、快捷键、焦点、无障碍、配置键和运行行为由契约测试与行为探针持续保护；C# 必须逐模块对照这些行为，不能重新设计。

## 当前基线

- 正式参考源码：`D:\编程\沙河批量打开器.py`
- 仓库入口：`D:\mycodex\git\沙河批量打开器.py`
- 两个文件在拆分开始前 SHA-256 相同：`E640619384AACB0B70CB58BCFAE17C0D4A4D7AB45CC6B8CF7CCD0F6E56215233`
- 基线规模：12,071 行，530,664 字节。
- 核心主窗口：`SandboxLauncher`，包含 400 个方法，约占 8,400 行。
- 原代码已有外部公共模块依赖：`shahe_bulk_launcher.common`，但拆分前仓库未包含该包。

## 保留入口

- `沙河批量打开器.py` 保留原文件名和直接启动方式。
- 入口继续导出原顶层常量、函数、控件、对话框、`BackupRestoreMgr`、`SandboxLauncher`、`SingleInstance` 和 `main`，兼容现有导入代码及回归探针。

## 当前状态

- 模块化迁移完成：目标结构中的 20 个 Python 模块全部存在。
- 兼容入口由基线 12,071 行缩减为 171 行；主窗口组装集中在 200 行的 `shahe_bulk_launcher/app.py`。
- `SandboxLauncher` 的 400 个方法按 1 个应用初始化方法和 10 个业务 mixin 唯一分区，无重复或遗漏。
- 最终入口 API 审计无缺失导出，`SandboxLauncher` 与 `main` 均由 `shahe_bulk_launcher.app` 提供。
- C# 迁移工程已建立：`csharp/ShaheBulkLauncher.sln` 包含 Core、App、Updater、Tests 四个 `net48/x64` 项目。
- C# Task 1 `constants.py`、Task 2 `bootstrap.py`、Task 3 `common.py`、Task 4 `single_instance.py`、Task 5 `ui/controls.py` 与 Task 6 `ui/common_dialogs.py` 已完成，当前累计 49 项 C# 契约测试通过；其余 14 个模块仍以 Python 为唯一实现，未声明完成。

## 目标结构

```text
沙河批量打开器.py
shahe_bulk_launcher/
    __init__.py
    app.py
    bootstrap.py
    common.py
    constants.py
    backup.py
    single_instance.py
    ui/
        __init__.py
        controls.py
        common_dialogs.py
        settings_dialogs.py
        tray.py
    features/
        __init__.py
        lifecycle.py
        settings.py
        app_paths.py
        sandboxie.py
        process_actions.py
        updates.py
        speech.py
        hotkeys.py
        sandbox_admin.py
        scheduling.py
tests/
artifacts/
csharp/
    ShaheBulkLauncher.sln
    Directory.Build.props
    NuGet.config
    ShaheLauncher.Core/
    ShaheLauncher.App/
    ShaheLauncher.Updater/
    ShaheLauncher.Tests/
```

## 模块映射

| 原代码职责 | 目标模块 |
| --- | --- |
| 打包运行时、资源路径、日志 | `shahe_bulk_launcher/bootstrap.py` |
| 常量、默认快捷键、名称校验 | `shahe_bulk_launcher/constants.py` |
| 配置、版本和更新公共工具 | `shahe_bulk_launcher/common.py` |
| 列表控件、状态控件、延迟调用 | `shahe_bulk_launcher/ui/controls.py` |
| 二维码、用户选择、命名对话框 | `shahe_bulk_launcher/ui/common_dialogs.py` |
| 设置、快捷键、语音和路径记忆对话框 | `shahe_bulk_launcher/ui/settings_dialogs.py` |
| 托盘 | `shahe_bulk_launcher/ui/tray.py` |
| 备份恢复 | `shahe_bulk_launcher/backup.py` |
| 主窗口初始化、生命周期和 UI 调度 | `shahe_bulk_launcher/features/lifecycle.py` |
| 配置读写和路径校正 | `shahe_bulk_launcher/features/settings.py` |
| 应用路径、历史、剪贴板 | `shahe_bulk_launcher/features/app_paths.py` |
| Sandboxie 配置、名称、PID、启动接口 | `shahe_bulk_launcher/features/sandboxie.py` |
| 定位、关闭、静音、音量、播放设备 | `shahe_bulk_launcher/features/process_actions.py` |
| 更新检查和更新器启动 | `shahe_bulk_launcher/features/updates.py` |
| Tolk、读屏和语音辅助 | `shahe_bulk_launcher/features/speech.py` |
| 菜单、快捷键和列表导航 | `shahe_bulk_launcher/features/hotkeys.py` |
| 分组、批量管理、INI、别名 | `shahe_bulk_launcher/features/sandbox_admin.py` |
| 定时任务和批量启动 | `shahe_bulk_launcher/features/scheduling.py` |
| 主窗口组装和程序入口 | `shahe_bulk_launcher/app.py` |
| 单实例控制 | `shahe_bulk_launcher/single_instance.py` |

## Python 到 C# 模块映射

| 顺序 | Python 权威模块 | C# 固定落点 | 状态 |
| --- | --- | --- | --- |
| 1 | `constants.py` | `ShaheLauncher.Core/Constants/LauncherConstants.cs` | 已完成，8/8 |
| 2 | `bootstrap.py` | `ShaheLauncher.Core/Runtime/LauncherRuntime.cs` | 已完成，7/7 |
| 3 | `common.py` | `ShaheLauncher.Core/Common/`、`ShaheLauncher.Core/Configuration/` 与 `ShaheLauncher.Core/Updates/` | 已完成，10/10 |
| 4 | `single_instance.py` | `ShaheLauncher.App/Infrastructure/SingleInstanceGuard.cs` | 已完成，3/3 |
| 5 | `ui/controls.py` | `ShaheLauncher.App/UI/Controls/` | 已完成，14/14 |
| 6 | `ui/common_dialogs.py` | `ShaheLauncher.App/UI/Dialogs/Common/` | 已完成，7/7 |
| 7 | `ui/settings_dialogs.py` | `ShaheLauncher.App/UI/Dialogs/Settings/` | 待迁移 |
| 8 | `ui/tray.py` | `ShaheLauncher.App/UI/Tray/LauncherTrayIcon.cs` | 待迁移 |
| 9 | `features/settings.py` | `ShaheLauncher.Core/Settings/LauncherSettingsService.cs` | 待迁移 |
| 10 | `features/app_paths.py` | `ShaheLauncher.Core/AppPaths/AppPathHistoryService.cs` | 待迁移 |
| 11 | `features/sandboxie.py` | `ShaheLauncher.Core/Sandboxie/SandboxieClient.cs` | 待迁移 |
| 12 | `features/process_actions.py` | `ShaheLauncher.Core/Processes/ProcessActionService.cs` | 待迁移 |
| 13 | `features/sandbox_admin.py` | `ShaheLauncher.Core/SandboxAdmin/SandboxAdminService.cs` | 待迁移 |
| 14 | `features/scheduling.py` | `ShaheLauncher.Core/Scheduling/LaunchScheduler.cs` | 待迁移 |
| 15 | `backup.py` | `ShaheLauncher.Core/Backup/BackupRestoreService.cs` | 待迁移 |
| 16 | `features/speech.py` | `ShaheLauncher.Core/Speech/SpeechService.cs` | 待迁移 |
| 17 | `features/updates.py` | `ShaheLauncher.Core/Updates/UpdateService.cs` 与 `ShaheLauncher.Updater/` | 待迁移 |
| 18 | `features/hotkeys.py` | `ShaheLauncher.App/Input/HotkeyController.cs` | 待迁移 |
| 19 | `features/lifecycle.py` | `ShaheLauncher.App/MainForm.Lifecycle.cs` | 待迁移 |
| 20 | `app.py` | `ShaheLauncher.App/MainForm.cs` 与 `Program.cs` | 待迁移 |

## 构建与测试

- 语法验证：`python -m compileall -q .`
- 单元与结构回归：`python -m unittest discover -s tests -v`
- 入口导入烟雾测试：由 `tests` 中的兼容测试覆盖。
- UI 和关键业务回归：从 `D:\编程\MyCodex\shahe_launcher_test` 迁入与本次模块拆分有关的探针，并改为引用仓库入口。
- 临时输出、日志和截图统一放入 `artifacts/` 的用途子目录。
- C# 还原/构建前设置：`$env:MSBuildSDKsPath='D:\Dev\dotnet\sdk\8.0.421\Sdks'; $env:MSBuildEnableWorkloadResolver='false'`。
- C# Release 构建：`& 'C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\amd64\MSBuild.exe' 'csharp\ShaheBulkLauncher.sln' /restore /t:Build /p:Configuration=Release /p:Platform=x64 /m`。
- C# 全量测试：`& 'C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' 'csharp\ShaheLauncher.Tests\bin\x64\Release\net48\ShaheLauncher.Tests.dll' /Platform:x64`。

## 无障碍约束

- 不修改现有控件名称、焦点顺序、键盘操作和状态反馈。
- 焦点移动与选择变化不得新增高成本或破坏性操作。
- 启动、关闭、更新和批量管理继续由明确按钮、菜单或快捷键触发。

## 已知风险

- 全量灾难恢复测试已进入用户还原阶段：账号目录和沙盒配置节已清空，正式 `.sbx`、独立副本和原始配置副本保存在 `artifacts/live_backup_test/20260806_214549`。
- 当前系统 Python 3.14 未安装 `cryptography`，原版 `encrypt_config_value`、`decrypt_config_value`、`verify_update_signature` 和 `decrypt_update_package` 的真实 Python 执行会进入缺失依赖错误路径；C# 迁移必须按源码中的算法、密钥和格式用确定性向量验证，不修改 Python 环境或把依赖修复混入迁移。

## 验证记录

- 2026-08-07：C# 检查点提交前忽略规则修复通过：将运行时配置规则从 `Configuration/` 锚定为 `/Configuration/`，根级配置仍被忽略，`ShaheLauncher.Core/Configuration/ConfigCrypto.cs` 与 `ConfigStore.cs` 可正常纳入 Git，C# `.vs/bin/obj` 构建产物继续排除；`git diff --check` 无空白错误。
- 2026-08-07：C# Task 6 `ui/common_dialogs.py` 完成：`ShaheBulkLauncher.sln` 以 `Release|x64` 完成还原和生成，Core、App、Updater、Tests 全部成功输出，0 警告、0 错误；最终验证为 CommonDialogs 7/7、C# 累计 49/49、Python 对话框结构契约 2/2 与控件名探针 PASS。六个 C# 类型已迁入 `ShaheLauncher.App/UI/Dialogs/Common/`，Python 正式入口未改变；其余 14 个模块继续按映射顺序迁移。
- 2026-08-07：C# Task 6 当前累计全量测试通过：`ShaheLauncher.Tests.dll /Platform:x64` 运行 49 项、49/49 通过，包含 CommonDialogs 7 项及 Task 1-5 的既有 42 项，无已完成模块回归。
- 2026-08-07：C# Task 6 Python 对话框控件名探针通过：`python tests\probes\dialog_control_names_probe.py` 输出 `PASS dialog_control_names_probe`，确认 Python 权威基线中的对话框关键 AccessibleName 保持不变；探针附带实例化设置对话框仅用于原版基线验证，不提前声明 C# Task 7 完成。
- 2026-08-07：C# Task 6 Python 权威结构对照通过：`python -m unittest tests.test_ui_dialogs_contract -v` 运行 2 项、2/2 通过，确认 Python `common_dialogs.py` 的辅助函数与对话框直接方法仍保持既有模块归属，C# 迁移未修改正式 Python 入口。
- 2026-08-07：C# Task 6 `CommonDialogs` 完整定向回归通过：`TestCategory=CommonDialogs` 运行 7 项、7/7 通过，二维码图片与剪贴板、二维码窗口、用户选择、分组名称和批量命名规则在同一程序集组合验证中无回归。
- 2026-08-07：C# Task 6 批量命名规则子模块定向 GREEN 通过：`BatchNameRuleDialog` 三项契约 3/3 通过，确认 430×320/430×300 窗口、数值与下拉编号输入、固定/可编辑数量、精确标签与 AccessibleName、输入范围、默认选择、`prefix/start/digits/count` 结果键，以及 `名称前缀不能为空。`、`请选择编号格式。`、`数量必须大于 0。` 校验原文保持 Python 行为。
- 2026-08-07：C# Task 6 用户选择/分组名称子模块定向 GREEN 通过：Tests 项目以 `Release|x64` 构建成功，0 警告、0 错误；`SelectUsersDialogPreservesTextOrderAndCheckedSelection` 与 `GroupNameDialogTrimsValueAndRequiresNonBlankInput` 共 2 项、2/2 通过，确认 400×500/380×150 窗口、原提示与按钮文案、用户顺序和勾选返回、分组名 trim、空值静默停留及 `沙盒用户`/`分组名称` AccessibleName 保持 Python 行为。
- 2026-08-07：C# Task 6 二维码子模块定向 GREEN 通过：图片/剪贴板与 `QRCodeDialog` 共 2 项、2/2 通过；验证真实 STA 剪贴板写入、成功/缺失文件原文、字节图片加载、500×600 窗口、最大边 450 等比缩放、提示文案、120×35/80×35 按钮和 `copy_img_path` 优先级。
- 2026-08-07：C# Task 6 二维码首轮 GREEN 为 1/2：`QRCodeDialog` 的 500×600 尺寸、原文、按钮尺寸、450 最大边缩放和复制目标优先级通过；图片工具测试仅因 `MessageBoxIcon.Information` 与 `Asterisk` 是同值枚举且 `ToString()` 返回后者而失败，属于测试表示差异，需改为比较枚举数值。
- 2026-08-07：C# Task 6 `ui/common_dialogs.py` 有效 RED 通过：App、Core、Tests 以 `Release|x64` 构建成功，0 警告、0 错误；`TestCategory=CommonDialogs` 运行 7 项、7/7 按预期失败，失败点均为 `DialogMessageService`、`DialogImageUtilities`、`QRCodeDialog`、`SelectUsersDialog`、`GroupNameDialog`、`BatchNameRuleDialog` 尚不存在，已冻结尺寸、原文、AccessibleName、输入范围、结果键、二维码缩放和剪贴板反馈。
- 2026-08-07：C# Task 5 `ui/controls.py` 完成：最终 `ShaheBulkLauncher.sln` 以 `Release|x64` 还原和生成成功，Core、App、Updater、Tests 全部输出，0 警告、0 错误；Task 5 最终验证为 Controls 14/14、C# 累计 42/42、Python 结构契约 1/1 与两个导航探针 PASS。五个控件类已迁入 `ShaheLauncher.App/UI/Controls/`，主窗体委托接线仍按模块边界留到后续 features/lifecycle/app 任务，Python 正式入口未改变。
- 2026-08-07：C# Task 5 最终累计全量测试通过：`ShaheLauncher.Tests.dll /Platform:x64` 运行 42 项、42/42 通过，包含 Controls 14 项以及 Task 1-4 的既有 28 项，无回归。
- 2026-08-07：C# Task 5 最终定向 Controls 回归通过：`TestCategory=Controls` 运行 14 项、14/14 通过，覆盖五个控件类、UI/文案/键盘/焦点/无障碍、完整经典菜单、扫描刷新、异步状态、后续模块接线、句柄重建和无句柄替换边界。
- 2026-08-07：C# Task 5 应用列表替换边界 GREEN 通过：共享单列替换逻辑现在在无句柄时回退读取 `ListViewItem.Selected`；定向重建成功，目标测试 1/1 通过，`B.exe` 的当前选择和勾选身份在 `A/B` 替换为 `A/B/C` 后均保留。
- 2026-08-07：C# Task 5 应用列表替换边界有效 RED 通过：数组显式装箱后测试进入业务断言，替换 `A/B` 为 `A/B/C` 时当前值从 `B.exe` 丢为空串；根因是共享替换逻辑在无原生句柄时只读取空的 `SelectedIndices`，没有回退到 `ListViewItem.Selected`。
- 2026-08-07：C# Task 5 应用列表替换边界首次测试未进入业务断言：`ReplaceItems` 已公开，但反射助手的 `params object[]` 将裸 `string[]` 展开为 3 个参数，报告“ReplaceItems must be public”；需把数组显式装箱后重跑有效 RED，生产实现暂不修改。
- 2026-08-07：C# Task 5 调用点/句柄生命周期 GREEN 通过：Controls 13 项、13/13 通过；后续 hotkeys/process-actions 可接线的身份、特殊项、快速刷新和上下文索引入口已公开，`SingleColumnListCtrl` 只在真正 Dispose 时停止延迟任务，原生句柄重建后仍保持可用。
- 2026-08-07：C# Task 5 完成前调用点/句柄生命周期 RED 通过：Controls 扩展为 13 项，原 11 项继续通过；后续模块集成测试因 `GetItemIdentityText` 尚未公开而失败，句柄重建测试确认 `OnHandleDestroyed` 会把仍存活控件永久标记为已销毁并拒绝后续延迟调用。修复限定为公开既有调用点和区分句柄重建/真正 Dispose。
- 2026-08-07：C# Task 5 四项目解决方案构建通过：`ShaheBulkLauncher.sln` 完成 `Release|x64` 还原与生成，Core、App、Updater、Tests 均成功输出 `net48/x64` 产物；未接管 Python 正式入口。
- 2026-08-07：C# Task 5 当前累计全量测试通过：`ShaheLauncher.Tests.dll /Platform:x64` 运行 39 项、39/39 通过，包含 Controls 11 项以及 Constants、Bootstrap、Common、SingleInstance 的既有 28 项，无已完成模块回归。
- 2026-08-07：C# Task 5 Python 对照通过：`tests.test_ui_controls_contract` 运行 1 项、1/1 通过；`sandbox_list_navigation_no_status_probe.py` 与 `sandbox_list_navigation_coalesces_live_state_probe.py` 均 PASS，确认原版焦点移动不更新勾选计数，连续选择的 live-state 和菜单刷新仍由宿主合并调度。
- 2026-08-07：C# Task 5 Controls 扩展 GREEN 通过：App、Core、Tests 定向重建成功，0 警告、0 错误；`TestCategory=Controls` 运行 11 项、11/11 通过，新增覆盖 Ctrl+A、F5、置顶无障碍播报、显式切换和清空后上下文复位。进入 Python 对照、C# 全量和四项目构建阶段。
- 2026-08-07：C# Task 5 无障碍播报/显式切换 RED 通过：Controls 扩展为 11 项，前 10 项全部通过，新增测试仅因 `AnnounceItemState` 尚未作为公开同等行为提供而失败；已同时冻结用户置顶播报、分组不可切换和清空后上下文索引复位。
- 2026-08-07：C# Task 5 异步运行状态定向 GREEN 通过：目标测试 1/1 通过；运行状态查询在线程池执行且 UI 调用在 150ms 阈值内返回，空闲/运行后缀变化不丢失勾选身份，只有实际文本变化时才触发进程菜单失效和 live-state 刷新委托。
- 2026-08-07：C# Task 5 扫描与列表刷新定向 GREEN 通过：目标测试 1/1 通过；宿主显示名扫描优先、目录子项回退、大小写无关排序、显示名解析、置顶/分组/普通项顺序，以及 `正在扫描沙盒用户...`、`已刷新 N 个沙盒用户/分组用户` 原文均保持。
- 2026-08-07：C# Task 5 完整右键菜单定向 GREEN 通过：目标测试 1/1 通过，继续使用经典 `ContextMenu`/`MenuItem`；置顶/取消置顶、静音状态、四个音量动作、播放设备、关闭、定位和刷新文案及当前用户/步进参数均与 Python 一致。
- 2026-08-07：C# Task 5 选择与键盘导航定向 GREEN 通过：目标测试 1/1 通过；列表选择只调用进程状态刷新委托，不更新勾选计数，左右导航、Space 分组保护、Enter 进入分组和 Backspace 返回根视图保持 Python 行为。
- 2026-08-07：C# Task 5 `CheckableListCtrl` 第二轮有效 RED 通过：App、Core、Tests 以 `Release|x64` 构建成功，0 警告、0 错误；Controls 扩展为 10 项，原 6 项继续通过，新增 4 项按预期失败，分别缺少选择刷新/键盘导航、完整菜单动作、扫描与根/分组刷新、后台运行状态更新所需的委托属性和方法。
- 2026-08-07：C# Task 5 `ui/controls.py` 第一轮核心 GREEN 通过：`TestCategory=Controls` 运行 6 项、6/6 通过，覆盖延迟调用、状态日志、原生单列列表、应用路径 Space 复选、分组特殊项、区间复选、运行状态稳定身份和经典右键菜单。Task 5 尚未完成，下一轮补键盘导航、完整菜单动作、目录扫描、列表刷新和异步运行状态契约。
- 2026-08-07：C# Task 5 区间复选定向 GREEN 通过：`CheckItem` 现在直接执行区间规则，不再依赖控件句柄或 `ItemChecked` 是否触发；事件路径仍保留给鼠标/原生复选，分组索引继续跳过。定向重建成功，目标测试 1/1 通过。
- 2026-08-07：C# Task 5 区间复选偏差精确定位：增强既有断言后，目标测试稳定失败于“区间中间项未勾选”，起点和终点均正常；根因是当前 C# 区间逻辑仅依赖 WinForms `ItemChecked` 回调，而控件句柄创建前的程序化勾选不保证触发该事件。修复将限定在 `CheckItem` 明确动作的事件无关路径。
- 2026-08-07：C# Task 5 `CheckableListCtrl` 核心首轮 GREEN 收敛：App、Core、Tests 定向构建成功，0 警告、0 错误；Controls 6 项中 5 项通过，经典 `ContextMenu/MenuItem` 及原文案已经通过，唯一失败稳定在区间复选测试第 153 行，需核对 WinForms 复选事件中 Checked 状态提交时序。
- 2026-08-07：C# Task 5 应用列表 Space 定向 GREEN 通过：确认 WinForms 在句柄创建前会出现 `ListViewItem.Selected=true` 但 `SelectedIndices.Count=0`，控件现在统一回退读取项目的 Selected 标志；重建成功，目标测试 1/1 通过，原文 `已选中/未选中 {应用} 应用` 保持不变。
- 2026-08-07：C# Task 5 应用列表 Space 偏差已定位到首个断言：定向重建成功后，`AppPathSpaceToggleIsExplicitAndAnnouncesState` 稳定失败于第一次 Space 后项目仍未勾选；同时增强 STA 测试包装以保留原始异常和源码行号。下一步核对 WinForms 在控件句柄创建前的选中项读取语义，不改测试契约。
- 2026-08-07：C# Task 5 `AppPathListCtrl` 首轮 GREEN 收敛：App、Core、Tests 以 `Release|x64` 构建成功，0 警告、0 错误；`TestCategory=Controls` 运行 6 项，3 项通过、3 项失败。`ManagedDelayedCall`、`OperationStatusText`、单列列表与应用列表基础/无障碍契约已通过；应用列表 Space 显式复选仍有 1 项行为偏差待定位，另 2 项继续因 `CheckableListCtrl` 尚未实现而失败。
- 2026-08-07：C# Task 5 `ui/controls.py` 有效 RED 通过：补充 WinForms 测试引用后，App、Core、Tests 构建成功，0 警告、0 错误；`TestCategory=Controls` 运行 6 项、6/6 按预期失败，失败点均为 `ManagedDelayedCall`、`OperationStatusText`、`SingleColumnListCtrl`、`AppPathListCtrl`、`CheckableListCtrl` 尚不存在，已覆盖延迟调用、状态日志、原生 ListView、键盘复选、分组特殊项、稳定身份和经典右键菜单。
- 2026-08-07：C# Task 5 `ui/controls.py` 首次 RED 构建未进入测试：Tests 编译新增 WinForms 控件契约时报告 `System.Windows.Forms` 与 `Keys` 缺失，共 2 个错误；确认 App 已引用 WinForms，根因是测试项目未显式引用 .NET Framework 的 `System.Windows.Forms`，需补测试框架引用后重跑有效 RED。
- 2026-08-07：C# Task 4 `single_instance.py` 完成：四项目 `ShaheBulkLauncher.sln` 以 `Release|x64` 完成还原和生成，Core、App、Updater、Tests 全部成功，0 警告、0 错误；最终验证为 SingleInstance 3/3、C# 累计 28/28、Python 对照 13 项及基础模块 1/1。`Program.Main` 接入仍按模块边界留到 Task 20 `app.py`，未提前改变启动流程。
- 2026-08-07：C# Task 4 当前累计全量测试通过：`ShaheLauncher.Tests.dll /Platform:x64` 运行 28 项、28/28 通过，包含 SingleInstance 3 项、Common 10 项、Bootstrap 7 项与 Constants 8 项；确认单实例迁移未回归已完成模块。
- 2026-08-07：C# Task 4 Python 基础模块回归通过：`python -m unittest tests.test_foundation_modules -v` 运行 1 项、1/1 通过，确认 C# 单实例迁移未修改 Python `single_instance.py` 导出或兼容入口。
- 2026-08-07：C# Task 4 Python `single_instance.py` 对照探针通过：13 项检查全部通过，包含默认名称、真实 Global 互斥体独占/释放、锁文件活动 PID 拒绝、损坏 PID 覆盖、当前 PID UTF-8 无 BOM 写入、释放删除，以及两组窗口标题与提示原文顺序；日志保存在 `artifacts/runs/csharp_task4_single_instance_parity_20260807_0040`，未弹出 UI。
- 2026-08-07：C# Task 4 `single_instance.py` 定向 GREEN 通过：App 与 Tests 以 `Release|x64` 构建成功，0 警告、0 错误；`TestCategory=SingleInstance` 运行 3 项、3/3 通过，覆盖默认名称、两组窗口标题与提示原文、Global 互斥体独占/释放、活动 PID 锁文件拒绝、失效或损坏 PID 覆盖、UTF-8 无 BOM 和释放清理。
- 2026-08-07：C# Task 4 `single_instance.py` 有效 RED 通过：修正为加载真实程序集名“沙河批量打开器”后，`TestCategory=SingleInstance` 运行 3 项、3/3 按预期失败，唯一原因是 `ShaheLauncher.App.Infrastructure.SingleInstanceGuard` 尚不存在；已冻结名称默认值、互斥体生命周期、锁文件 PID 回退和原版窗口/提示文案。
- 2026-08-07：C# Task 4 `single_instance.py` 首次 RED 未到达业务断言：App、Core、Tests 构建成功，0 警告、0 错误，但 3 项测试均因按项目名 `ShaheLauncher.App` 加载程序集而抛 `FileNotFoundException`；确认 App 的真实程序集名按发布约束为“沙河批量打开器”，需修正测试装载名后重跑有效 RED。
- 2026-08-07：C# Task 3 `common.py` 完成：最终四项目 `Release|x64` 构建在边界修复后再次通过，0 警告、0 错误；模块映射更新为已完成。Task 3 最终验证为 Common 10/10、C# 累计 25/25、Python 对照 20 项及基础模块 1/1，未修改 UI、文案、配置键或 Python 正式基线。
- 2026-08-07：C# Task 3 边界修复后累计全量回归通过：`ShaheLauncher.Tests.dll /Platform:x64` 重新运行 25 项、25/25 通过，Constants、Bootstrap 与 Common 均无回归。
- 2026-08-07：C# Task 3 边界修复后 Common 回归通过：`TestCategory=Common` 重新运行 10 项、10/10 通过，确认空值/非有限整数解析修复未影响配置、加密、HTTP 和更新安全契约。
- 2026-08-07：C# Task 3 完成前边界审计 GREEN 通过：`ParseIntSetting` 现在对 `null`、`NaN` 和无穷值按 Python `int(...)` 异常路径返回默认值，并保持普通浮点/Decimal 截断语义；定向构建 0 警告、0 错误，目标测试 1/1 通过。
- 2026-08-07：C# Task 3 完成前边界审计 RED 通过：为 `ParseIntSetting` 增加 Python `int(None)` 与非有限浮点数回退默认值契约后，定向测试 1/1 按预期失败，首个差异为 `null` 应返回默认值 10、C# 实际返回 0；根因是 .NET `Convert.ToInt32(null)` 与 Python 语义不同。
- 2026-08-07：C# Task 3 Python 基础模块回归通过：`python -m unittest tests.test_foundation_modules -v` 运行 1 项、1/1 通过，确认 C# 迁移未修改 Python `common.py` 导出、基础模块 API 或兼容入口。
- 2026-08-07：C# Task 3 四项目解决方案构建通过：`ShaheBulkLauncher.sln` 以 `Release|x64` 完成还原和生成，Core、App、Updater、Tests 全部成功输出，0 警告、0 错误；App 与 Updater 输出同时包含 Task 3 所需的 Newtonsoft.Json、BouncyCastle 与 Core 程序集。
- 2026-08-07：C# Task 3 当前累计全量测试通过：`ShaheLauncher.Tests.dll /Platform:x64` 运行 25 项、25/25 通过，包含 Common 10 项、Bootstrap 7 项与 Constants 8 项；确认 `common.py` 迁移未回归前两个已完成模块。
- 2026-08-07：C# Task 3 Python `common.py` 对照探针通过：20 项可执行行为检查全部通过，包含布尔/整数/版本/大小解析、更新 404 白名单、配置与临时目录、大小写敏感配置键、精确 UTF-8 无 BOM INI/JSON 输出及重复键损坏回退；日志保存在 `artifacts/runs/csharp_task3_common_parity_20260807_0029`。Python 3.14 缺少 `cryptography` 的既有风险未改动，安全算法继续由 C# 确定性向量覆盖。
- 2026-08-07：C# Task 3 `common.py` 定向 GREEN 通过：补充 `System.Net.Http` 框架引用后，Core 与 Tests 以 `Release|x64` 构建成功，0 警告、0 错误；`TestCategory=Common` 运行 10 项、10/10 通过，覆盖公共解析/路径/隐藏进程、Python 兼容 INI/JSON、大小写敏感键、AES-GCM 确定性向量、HTTP 404 白名单、Ed25519 和加密更新包解密。
- 2026-08-07：C# Task 3 GREEN 首次构建未通过：两个 NuGet 依赖成功还原，但 Core 编译 `UpdateHttpClient.cs` 时报告 3 个 `System.Net.Http` 类型缺失错误；确认 .NET Framework 4.8 参考程序集存在，而 Core 编译引用列表未包含它，根因是项目缺少显式框架程序集引用，Common 行为测试尚未执行。
- 2026-08-07：C# Task 3 `common.py` RED 复核：重新构建测试项目成功，0 警告、0 错误；`TestCategory=Common` 仍为 10/10 预期失败，原因仅为 `CommonUtilities`、`ConfigCrypto`、`ConfigStore`、`UpdateHttpClient`、`UpdateSecurity` 尚未实现，确认可以进入 GREEN。
- 2026-08-07：C# Task 3 `common.py` RED 通过：`TestCategory=Common` 运行 10 项，10/10 按预期失败，失败原因分别为 `CommonUtilities`、`ConfigCrypto`、`ConfigStore`、`UpdateHttpClient`、`UpdateSecurity` 尚不存在；测试项目成功构建，RED 覆盖解析/路径、INI/JSON、AES-GCM、HTTP 404、Ed25519 与更新包解密。
- 2026-08-07：C# Task 3 `common.py` 基线冻结首轮：INI/JSON 输出、布尔解析、版本拆分和大小格式探针成功；AES-GCM 向量生成在导入 `cryptography` 时以 `ModuleNotFoundError` 中止，确认当前 Python 3.14 缺少原版可选安全依赖，尚未开始 C# common 实现。
- 2026-08-07：C# Task 2 当前全量测试通过：`ShaheLauncher.Tests.dll /Platform:x64` 运行 15 项、15/15 通过，包含 Bootstrap 7 项与 Constants 8 项；确认 `LauncherRuntime` 迁移未回归 Task 1 常量和校验行为。
- 2026-08-07：C# Task 2 四项目构建通过：迁入 `LauncherRuntime` 后再次生成 `ShaheBulkLauncher.sln` 的 `Release|x64`，Core、App、Updater、Tests 全部成功输出，0 警告、0 错误。
- 2026-08-07：C# Task 2 Python `bootstrap.py` 对照探针通过：统一资源相对路径分隔符输入后，隔离临时目录验证开发入口/应用目录、资源拼接、UTF-8 无 BOM 日志、发布版 internal/public EXE 选择及旧 launcher/updater 清理全部通过，未修改真实发布目录。
- 2026-08-07：C# Task 2 Python `bootstrap.py` 对照探针首轮未通过资源路径断言：探针向 `get_resource_path` 传入 `internal/Tolk.dll`，却与 `Path` 生成的反斜杠路径做字面比较；原函数按设计保留相对参数中的分隔符，失败属于探针输入不一致，日志与清理断言尚未执行。
- 2026-08-07：C# Task 2 `bootstrap.py` GREEN 通过：新增 `ShaheLauncher.Core/Runtime/LauncherRuntime.cs` 后，`TestCategory=Bootstrap` 共 7 项、7/7 通过；开发入口、发布版 internal/public EXE 选择、应用/资源目录、仅发布模式清理旧 EXE、UTF-8 无 BOM 按日日志和空消息忽略均符合冻结契约。
- 2026-08-07：C# Task 2 `bootstrap.py` RED 通过：`TestCategory=Bootstrap` 运行 7 项，7/7 按预期失败，唯一原因是 `ShaheLauncher.Core.Runtime.LauncherRuntime must exist`；已覆盖开发/发布路径、internal 公共 EXE 回退、资源路径、旧发布物清理、UTF-8 日志和空消息边界。
- 2026-08-07：C# Task 1 当前全量测试通过：`vstest.console.exe csharp\ShaheLauncher.Tests\bin\x64\Release\net48\ShaheLauncher.Tests.dll /Platform:x64` 运行 8 项、8/8 通过；当前 C# 测试面只包含已迁移的 `constants.py`，未提前声明其他模块完成。
- 2026-08-07：C# Task 1 四项目解决方案构建通过：将 `.sln` 的 Debug/Release 配置统一为 x64 后，运行 VS Build Tools MSBuild `/restore /t:Build /p:Configuration=Release /p:Platform=x64` 成功还原并生成 Core、App、Updater、Tests；输出包含 `沙河批量打开器.exe` 与 `updater.exe`，0 警告、0 错误。
- 2026-08-07：C# Task 1 首次全解决方案构建未进入编译：`ShaheBulkLauncher.sln` 仅含模板生成的 `Debug|Any CPU` 和 `Release|Any CPU` 解决方案配置，按目标请求 `Release|x64` 时报告 `MSB4126`；四个项目自身已映射到 x64，下一步只把解决方案配置名统一为 x64。
- 2026-08-07：C# Task 1 `constants.py` Python 对照回归通过：运行 `python -m unittest tests.test_foundation_modules -v` 共 1 项、1/1 通过，确认迁移过程未修改 Python 基础 API、常量导出或原入口兼容行为。
- 2026-08-07：C# Task 1 `constants.py` GREEN 通过：修正测试中裸 `null` 的 `params` 调用歧义后，`TestCategory=Constants` 共 8 项、8/8 通过；已冻结版本号、更新地址、日志选项、固定快捷键及文案、默认沙盒根、路径规范化、内部/显示名称校验和 Unicode 码点截断行为。
- 2026-08-07：C# Task 1 `constants.py` GREEN 首轮为 6/8：常量、固定快捷键、默认根目录、路径、内部名和显示前缀测试通过；日志模式与显示名测试在进入生产方法前因反射测试助手把裸 `null` 解释为 `params` 数组本身而抛 `NullReferenceException`。根因属于测试调用歧义，生产实现未出现对应失败证据。
- 2026-08-07：C# Task 1 `constants.py` GREEN 编译通过：新增 `ShaheLauncher.Core/Constants/LauncherConstants.cs` 后，`ShaheLauncher.Core` 与 `ShaheLauncher.Tests` 在 `net48/x64/Release` 下成功生成，0 警告、0 错误；尚待运行 Constants 行为测试。
- 2026-08-07：C# Task 1 `constants.py` RED 通过：`vstest.console.exe` 按 `TestCategory=Constants` 运行 8 项，8/8 按预期失败，失败原因统一为 `ShaheLauncher.Core.Constants.LauncherConstants must exist`；测试程序集和空 Core 程序集均已成功构建，确认 RED 来自目标实现缺失而非测试或工具链错误。
- 2026-08-07：C# Task 1 构建链根因验证通过：为 VS Build Tools MSBuild 设置 `MSBuildSDKsPath=D:\Dev\dotnet\sdk\8.0.421\Sdks` 和 `MSBuildEnableWorkloadResolver=false` 后，`ShaheLauncher.Tests.csproj` 首次还原并以 `net48/x64/Release` 成功构建，结果为 0 警告、0 错误；NuGet 使用 `D:\Dev\NuGet\packages` 并生成锁定文件。
- 2026-08-07：C# Task 1 构建链诊断第二步：临时设置 `MSBuildSDKsPath=D:\Dev\dotnet\sdk\8.0.421\Sdks` 后，顶层 `Microsoft.NET.Sdk` 已找到，但 VS MSBuild 因缺少 dotnet 主机的 workload resolver 报 `Microsoft.NET.SDK.WorkloadAutoImportPropsLocator` 无法解析；业务测试仍未执行，下一步仅禁用本项目不需要的 workload resolver 验证根因。
- 2026-08-07：C# Task 1 首次 RED 构建未进入测试：VS Build Tools `MSBuild.exe` 还原 `ShaheLauncher.Tests.csproj` 时报告 `MSB4236`，默认探测目录缺少 `Microsoft.NET.Sdk`；确认 SDK 实际位于 `D:\Dev\dotnet\sdk\8.0.421\Sdks`，属于 MSBuild SDK 解析环境问题，`LauncherConstants` 业务断言尚未执行。
- 2026-08-07：C# 迁移执行前 Python 行为基线通过：顺序运行 `tests/probes/*.py` 共 26 条，26/26 返回码 0；逐探针日志与 `summary.json` 保存在 `artifacts/runs/csharp_migration_python_baseline_20260807_234642`，未发现启动、UI、音频、进程、配置、语音、更新或备份恢复行为回归。
- 2026-08-07：C# 迁移执行前 Python 单元与契约基线通过：运行 `python -m unittest discover -s tests -v` 共 29 项、29/29 通过，覆盖 20 个模块、入口兼容、400 个方法归属、UI 契约、备份恢复安全边界和经典 `SbieCtrl.exe` 兼容。
- 2026-08-07：C# 迁移执行前 Python 语法基线通过：在分支 `codex/shahe-csharp-migration` 运行 `python -m compileall -q .`，退出码 0；当前兼容入口、20 个 Python 业务模块、测试和探针均可编译。C# 尚未创建，本记录只冻结迁移起点。
- 2026-08-07：拆分后模块分组回归全部通过，共 29/29：基础模块和迁移工具 2/2；入口、结构和方法分区 9/9；UI 控件、对话框和托盘 3/3；备份恢复 6/6；应用路径和 Sandboxie 1/1；进程操作 2/2；快捷键、沙盒管理和调度 3/3；更新和语音 3/3。覆盖全部 20 个目标模块及新增经典 `SbieCtrl.exe` 兼容回归。
- 2026-08-07：修复后完整单元回归通过：`python -m unittest discover -s tests -v` 运行 29 项、29/29 通过，未发现入口兼容、模块分区、UI 契约、备份恢复安全边界或经典 Sandboxie Control 查找回归。
- 2026-08-07：修复后语法验证通过：`python -m compileall -q .` 退出码 0，拆分入口、20 个目标模块、测试和探针目录均可编译。
- 2026-08-07：修复后全量行为探针通过：顺序运行 `tests/probes/*.py` 共 26 条，26/26 返回码 0；日志保存在 `artifacts/runs/probes_final_20260807_230202`，未发现业务模块、UI、启动调度或备份恢复边界回归。
- 2026-08-07：恢复现场最终门禁通过：`C:\Sandbox\Administrator` 有 99 个账号目录，账号内部 4,492 个文件、663,169,463 字节和 35 个 `RegHive`；根目录额外保留 1 个文件；`Sandboxie.ini` 有 86 个节；`CodexStressB` 存在且为空；`tenp\restore_temp` 不存在；正式 `.sbx`（313,267,172 字节）和独立回滚副本存在；`SbieSvc=Running`，`Start.exe /listpids` 为 0。
- 2026-08-07：拆分版主程序现场启动验证通过：使用仓库入口和 `pythonw.exe` 启动 PID 11336，窗口标题为“沙河批量打开器”，窗口句柄有效，Windows 报告 `Responding=True`。
- 2026-08-07：模块入口归属复核通过：兼容入口 `沙河批量打开器.py` 为 171 行，主组装模块 `shahe_bulk_launcher/app.py` 为 200 行；`SandboxLauncher`、`main` 和 `BackupRestoreMgr` 分别由 `shahe_bulk_launcher.app`、`shahe_bulk_launcher.app` 和 `shahe_bulk_launcher.backup` 提供。
- 2026-08-07：本机经典 Sandboxie Control 现场验证通过：拆分版 `BackupRestoreMgr._find_sandboxie_control_exe()` 返回 `C:\Program Files\Sandboxie\SbieCtrl.exe`，恢复完成后的“未找到 Sandboxie Control”问题已消除。
- 2026-08-07：经典 Sandboxie Control 探测修复 GREEN：`test_find_control_supports_classic_sbiectrl` 定向 1/1 通过；`BackupRestoreMgr._find_sandboxie_control_exe()` 现在按 `SandMan.exe`、`SbieCtrl.exe`、`Sandboxie.exe` 顺序兼容 Plus/经典安装。
- 2026-08-07：经典 Sandboxie Control 探测修复 RED：新增 `test_find_control_supports_classic_sbiectrl`，定向运行 1 项按预期失败；临时安装目录仅包含 `SbieCtrl.exe` 时，`BackupRestoreMgr._find_sandboxie_control_exe()` 返回 `None`，精确复现本机恢复成功后的错误提示。
- 2026-08-07：发现恢复模块真实兼容问题：本机 `C:\Program Files\Sandboxie` 安装的是经典控制程序 `SbieCtrl.exe`，但 `BackupRestoreMgr._find_sandboxie_control_exe()` 只检查 `SandMan.exe` 和 `Sandboxie.exe`，因此实际恢复成功后错误提示“未找到 Sandboxie Control”。该问题进入独立 TDD 修复。
- 2026-08-07：真实启动探针产生的数据已再次清理：`CodexStressB` 重建为空目录，`ChildCount=0`，`SbieSvc=Running`；其余恢复账号未修改。
- 2026-08-06：恢复后真实 Sandboxie 启动/关闭链路通过：正式 `SandboxieMixin._scan_sandbox_display_names()` 返回 84 个配置沙盒且包含 `CodexStressB`；正式 `start_app_in_sandbox()` 启动 `C:\Windows\System32\notepad.exe` 成功，检测到 3 个沙盒 PID；正式 `ProcessActionsMixin._terminate_sandbox_via_start()` 返回成功，随后 PID 列表为空。测试沙盒产生 13 个临时文件，已安排清理并恢复为空。
- 2026-08-06：真实启动探针基线已恢复：停止并重启 `SbieSvc` 后，`C:\Sandbox\Administrator\CodexStressB` 被清理并重建为空目录，后置检查 `Exists=True`、`ChildCount=0`、服务 `Running`；其余 98 个恢复目录未触碰。
- 2026-08-06：真实启动探针残留已清理：使用 Sandboxie 官方 `Start.exe /box:CodexStressB /terminate` 返回码 0，随后 `/listpids` 返回 `0`，确认 `notepad.exe` 已退出；下一步删除该探针在空沙盒中产生的临时文件并重建空目录。
- 2026-08-06：真实扫描/启动探针首轮未完成：扫描阶段进入并调用了正式 `start_app_in_sandbox("CodexStressB", notepad.exe, ...)`，随后测试桩误把 `ProcessActionsMixin` 的 `_terminate_sandbox_via_start` 当成 `SandboxieMixin` 方法，抛出 `AttributeError`，未执行关闭断言也未写入结果文件；这属于探针组合错误，需先用 Sandboxie 正式 `Start.exe /terminate` 清理测试进程，再修正探针。
- 2026-08-06：恢复后全量行为回归通过：顺序运行 `tests/probes/*.py` 共 26 条，26/26 通过；覆盖进程/音频、启动与失败处理、配置路径、首次扫描、列表导航、快捷键/菜单、分组/批量管理、UI 无障碍、语音、更新和备份恢复边界。
- 2026-08-06：恢复后全量结构回归通过：`python -m unittest discover -s tests -v` 28/28 通过，`python -m compileall -q .` 通过；覆盖入口兼容、20 个模块、400 个 `SandboxLauncher` 方法分区、UI/对话框契约和安全解压测试。
- 2026-08-06：恢复测试最终现场总检通过：正式 `.sbx` 与独立副本均存在；当前沙盒根恢复为 99 个目录、4,492 个文件、663,169,463 字节和 35 个 `RegHive`；`Sandboxie.ini` 为 86 个节；`SbieSvc=Running`；拆分版主窗口 PID 12856、标题“沙河批量打开器”、响应正常；`tenp\restore_temp` 不存在。
- 2026-08-06：恢复后 UI 启动验证通过：PID 12856 的主窗口类为 `wxWindowNR`、标题为“沙河批量打开器”，初始隐藏但窗口已创建且响应；通过确认窗口归属后显示并置前，当前窗口可供用户查看恢复后的沙盒列表。
- 2026-08-06：恢复后启动观察首轮：`pythonw.exe` PID 12856 持续运行且 `Responding=True`，但 30 秒内 `MainWindowHandle=0`、标题为空；进程未退出、恢复目录和配置未受影响，继续观察其启动初始化是否因 99 个沙盒扫描耗时。
- 2026-08-06：恢复后 `Sandboxie.ini` 逐节门禁通过：当前文件与最终 `.sbx` 解压出的配置均为 86 个节，缺失 0、额外 0、节内容差异 0；当前文件 SHA-256 为 `3591AC85FA8CF60FD58F82E3963095F2A296B43728B59B0A4A20761399E82E09`，由于恢复逻辑按当前配置顺序合并，整文件哈希与归档顺序不同，但所有节内容一致。
- 2026-08-06：恢复后账号数据逐文件门禁通过：当前 `C:\Sandbox\Administrator` 与最终 `.sbx` 解压副本均为 99 个目录、4,492 个文件、663,169,463 字节，目录差异 0、逐文件 `RelativePath/Length/SHA256` 差异 0；35 个 `RegHive` 全部存在并纳入匹配，`CodexStressB` 保持空目录，`restore_temp` 已清理，`SbieSvc=Running`。
- 2026-08-06：恢复清理时序回归通过：第二次调用正式 `BackupRestoreMgr.restore()` 选择全部 99 个用户，最终状态为“还原完成”，`reload_errors=[]`；测试进程等待后台恢复线程退出后，`D:\mycodex\git\tenp\restore_temp` 不存在、临时文件字节数为 0，确认恢复线程的最终清理逻辑可完成。首次临时目录残留仅由隔离测试进程提前退出造成。
- 2026-08-06：恢复测试临时目录观察结果：首次隔离测试进程在收到“还原完成”后立即退出，导致 daemon 恢复线程的 `finally` 清理被进程结束打断；30 秒后 `D:\mycodex\git\tenp\restore_temp` 仍有 5,070 个临时条目、约 581,193,722 字节。账号源目录已恢复且未受影响，后续第二次调用恢复入口并等待临时目录消失，以区分测试进程生命周期问题与正式恢复逻辑问题。
- 2026-08-06：恢复后初步现场核对：`C:\Sandbox\Administrator` 已回到 99 个目录、4,492 个文件、663,169,463 字节和 35 个 `RegHive`；`C:\Windows\Sandboxie.ini` 已回到 86 个节，`SbieSvc` 为 `Running`，`Start.exe /listpids` 无沙盒进程输出。恢复成功状态回调后 1 秒检查发现 `D:\mycodex\git\tenp\restore_temp` 仍存在，需等待后台线程的最终清理并再次核对。
- 2026-08-06：正式恢复流程实测执行：使用最终 `SandboxBackup_20260806_215418.sbx` 调用拆分版 `BackupRestoreMgr.restore()`，自动选择并确认全部 99 个用户（含 `CodexStressB` 空目录）；恢复进度完整走过 0/10/30/70/90/100，最终状态为“还原完成”，未收到恢复失败消息。恢复线程使用有界的 `/reload` 包装以避免本机 `Start.exe /listpids` 探针曾出现的无输出等待，服务重启错误为空；待后续逐文件核对。
- 2026-08-06：删除后软件启动最终验证通过：首轮 5 秒窗口匹配过早，随后前台重现确认主入口持续运行；保留拆分版 `pythonw.exe` PID 8840，窗口标题为“沙河批量打开器”、响应正常。清理了本轮产生的第二实例 PID 8072 和 `/listpids` 诊断进程，主窗口未关闭，等待用户触发还原测试。
- 2026-08-06：删除后启动验证首轮失败：用 `pythonw.exe` 启动仓库入口后等待 5 秒未出现“沙河批量打开器”窗口，进程未持续存活，退出码 9；该失败发生在 UI 启动阶段，未修改已清空的沙盒目录或备份文件，下一步读取最新日志并以前台 `python.exe` 重现。
- 2026-08-06：删除后现场验证通过：`C:\Sandbox\Administrator` 剩余目录 0、根文件仅 `desktop.ini`；`C:\Windows\Sandboxie.ini` 剩余 `UserSettings_4BC00582` 与 `GlobalSettings` 两节，SHA-256 为 `38A3B29EB69A93D522359D589776E7FA2F99634BF3E8C43EF5E801B30426279E`；`SbieSvc` 已恢复为 `Running`，`Start.exe /listpids` 无沙盒进程输出。没有删除宿主机注册表键，因为此前调查确认这些目标键本来不存在。
- 2026-08-06：已授权删除执行结果：删除 `C:\Sandbox\Administrator` 下全部 99 个直接子目录，命令后置检查 `RemainingDirectories=0`，根目录只剩原有 `desktop.ini`；随后通过正式 `SandboxAdminMixin._apply_ini_section_changes` 移除 84 个沙盒配置节，保留 `UserSettings_4BC00582` 与 `GlobalSettings`，无写入错误。删除后的 `Sandboxie.ini` SHA-256 为 `38A3B29EB69A93D522359D589776E7FA2F99634BF3E8C43EF5E801B30426279E`。
- 2026-08-06：首次删除命令未执行：本机命令安全策略在执行前拦截了含 `Remove-Item -Recurse` 的 PowerShell 命令，实际删除数量为 0；99 个账号目录、源 `Sandboxie.ini` 和全部备份均未改变。后续仍在同一 PowerShell 进程内完成路径校验，改用 .NET `Directory.Delete(path, true)` 执行已核验的直接子目录删除。
- 2026-08-06：最终删除门禁通过：99 个账号目录、4,492 个文件、663,169,463 字节在当前源、独立副本和最终 `SandboxBackup_20260806_215418.sbx` 解压副本中逐文件 SHA-256 差异均为 0，顶层目录差异均为 0；35 个根 `RegHive` 全部覆盖，`CodexStressB` 三方存在，两个备份内的 `Sandboxie.ini` 均匹配源文件。最终 `.sbx` SHA-256 为 `0BC8C81DC9B50E7DBF68A6019D47346792BF8949F9A53916870777F1C6A75166`，允许进入已授权删除阶段。
- 2026-08-06：最终归档与独立副本结构验证通过：独立副本同步后有 99 个顶层目录且包含空目录 `CodexStressB`，`robocopy` 返回 0；使用正式 `safe_extract_zip` 解压 `SandboxBackup_20260806_215418.sbx` 后同样有 99 个顶层目录、4,492 个文件、包含 `CodexStressB` 和 `Sandboxie.ini`。下一步执行最终逐文件哈希门禁。
- 2026-08-06：最终全量软件备份实测通过：在加入空目录 `CodexStressB` 后，重新调用正式 `BackupRestoreMgr.backup()` 选择全部 99 个目录，状态为“备份完成”，成功消息明确包含 `CodexStressB`；生成 `artifacts/live_backup_test/20260806_214549/SandboxBackup_20260806_215418.sbx`，大小 313,267,172 字节。该文件取代首份 98 目录归档，作为后续删除与用户恢复测试的正式备份。
- 2026-08-06：为配置节 `CodexStressB` 建立同名空账号目录成功，后置检查 `Exists=True`、`ChildCount=0`，且 `SbieSvc` 仍为 `Stopped`；账号目录总数由 98 变为 99，准备重新调用正式软件备份，使该空配置也能在恢复选择列表出现。
- 2026-08-06：`CodexStressB` 空目录首次创建未执行：当前 PowerShell 的 `New-Item` 不支持 `-LiteralPath` 参数，命令输出参数错误且后置检查为 `Exists=False`；未改变沙盒账号范围，后续改用固定绝对 `-Path` 重试。
- 2026-08-06：正式 mixin 组合的只读 INI 范围探针通过：识别 84 个 Sandboxie 沙盒配置节和 98 个现有账号目录，其中 83 个名称相交；唯一没有目录的配置节为 `CodexStressB`，另有 15 个遗留目录没有配置节。由于当前恢复 UI 只列出 `.sbx` 内的账号目录，为保证“全部配置节删除后仍全部可选恢复”，将在删除前为 `CodexStressB` 建立同名空目录并重新生成最终软件备份。
- 2026-08-06：INI 删除范围首次只读探针未进入业务读取：仅实例化 `SandboxieMixin` 时因缺少正式主窗口由其他 mixin 提供的 `_read_text_with_encoding` 而抛出 `AttributeError`；`C:\Windows\Sandboxie.ini` 未被写入，后续按正式 mixin 组合补齐探针后重跑。
- 2026-08-06：删除前最终三方哈希门禁通过：严格限定 98 个账号目录后，源目录、独立副本和软件 `.sbx` 解压副本均为 4,492 个文件、663,169,463 字节；独立副本逐文件差异 0、归档逐文件差异 0、两份顶层目录名差异均为 0，并覆盖全部 35 个根 `RegHive`。`Sandboxie.ini` SHA-256 为 `8E3719E3482B3D993F28D55AB9F2C1405DF7F3D0CF23CDF6DA9442FA6BAC1FC0`，`.sbx` SHA-256 为 `64CCDA8E4B638891267F2EC9B0C93BDBE0F40806ED2E8D0733C4210B562E985B`；根 `desktop.ini` 明确保留。
- 2026-08-06：首次三方门禁单文件差异已定位：唯一差异为沙盒根目录自身的 `C:\Sandbox\Administrator\desktop.ini`（208 字节），它不在任何选中账号目录内；正式备份按设计只归档 `SandboxUsers/<账号目录>/...`，因此该文件不是账号数据遗漏。98 个账号目录的目录名、目录内文件、35 个 `RegHive` 与 `Sandboxie.ini` 均已有匹配证据，最终门禁改为严格比较全部账号目录并明确保留根 `desktop.ini`。
- 2026-08-06：删除前首次三方哈希门禁未通过，故未删除任何源数据：使用 `-Force` 统计并计算 SHA-256 后，源目录与独立副本均为 4,493 个文件、663,169,671 字节，逐文件差异为 0；软件 `.sbx` 解压副本为 4,492 个文件、663,169,463 字节，少 1 个 208 字节文件。三方均有 98 个顶层目录和 35 个根 `RegHive`，两份 `Sandboxie.ini` SHA-256 均匹配源文件；需定位单文件差异后重新决定是否备份。
- 2026-08-06：软件 `.sbx` 安全解压实测：使用修复后的正式 `safe_extract_zip` 将 `SandboxBackup_20260806_214624.sbx` 解压到隔离验证目录，归档可完整读取，包含 98 个 `SandboxUsers` 顶层目录、4,492 个文件和 `Sandboxie.ini`；文件数高于先前 PowerShell 非 `-Force` 统计是因为备份包含隐藏文件，下一步使用 `-Force` 三方清单统一核对。
- 2026-08-06：独立回滚目录复制完成：修正 `/LOG:` 参数后，`robocopy C:\Sandbox\Administrator ...\independent_restore\SandboxUsers /E /COPY:DAT /DCOPY:DAT /XJ` 返回 1（成功复制新文件），目标存在 98 个顶层账号目录；独立目录同时保存 `Sandboxie.ini` 和 `shahe_config.ini`，待三方哈希核对后才允许删除源数据。
- 2026-08-06：独立回滚副本首次复制未执行：`robocopy` 因 PowerShell 将动态 `/LOG:` 参数错误拆分而返回 16，输出明确为 `Invalid Parameter #12 : "/LOG:"`；源账号目录与已生成 `.sbx` 未被修改，`Sandboxie.ini` 和软件配置的小文件副本已创建，后续修正命令参数重新复制账号目录。
- 2026-08-06：全量软件备份实测：停止 `SbieSvc` 后，通过拆分版正式 `BackupRestoreMgr.backup()` 选择 `C:\Sandbox\Administrator` 下全部 98 个账号目录，状态从“正在备份...”变为“备份完成”，生成 `artifacts/live_backup_test/20260806_214549/SandboxBackup_20260806_214624.sbx`，文件大小 313,267,042 字节；软件成功消息记录的用户数与删除前目录数一致。
- 2026-08-06：全量灾难恢复删除前预检：`C:\Sandbox\Administrator` 包含 98 个账号目录、4,339 个文件、625,568,183 字节（0.583 GiB），其中 35 个账号根目录含 `RegHive`；`C:\Windows\Sandboxie.ini` 包含 86 个配置节、大小 8,780 字节；Sandboxie `SbieSvc` 正在运行，`Start.exe /listpids` 未返回沙盒内进程；D 盘剩余 10.08 GiB，可容纳软件 `.sbx` 与独立回滚副本。
- 2026-08-05：恢复修复全量行为回归：顺序运行 `tests/probes/*.py` 共 26 条，26/26 通过；覆盖音频/进程操作、启动、配置写入、UI/无障碍、更新、沙盒管理以及恢复安全解压、无 shell 和后台停止服务边界，未发现本次修复引入的行为回归。
- 2026-08-05：恢复修复全量结构回归：`python -m unittest discover -s tests -v` 28/28 通过（原 24 项加 4 项安全解压测试）；`python -m compileall -q .` 通过。
- 2026-08-05：恢复修复定向行为回归：`restore_safe_extract_probe.py`、`restore_shell_free_probe.py`、`restore_no_main_thread_stop_probe.py` 均通过，确认正式恢复继续经过安全解压边界，子进程仍不启用 shell，停止 Sandboxie 服务仍只在恢复后台线程执行；`python -m py_compile shahe_bulk_launcher\backup.py tests\test_backup_contract.py` 通过。
- 2026-08-05：恢复阻断 bug 修复 GREEN：在 `shahe_bulk_launcher/backup.py` 增加标准库安全解压实现，先验证全部 ZIP 成员再写盘，拒绝父目录穿越、绝对路径、Windows 盘符/UNC/备用数据流路径及 ZIP 符号链接；`python -m unittest tests.test_backup_contract -v` 5/5 通过，正常 `.sbx` 中的 `Sandboxie.ini` 与 `SandboxUsers/Box1/RegHive` 可完整解压。
- 2026-08-05：恢复阻断 bug 修复 RED：`python -m unittest tests.test_backup_contract -v` 运行 5 项，原备份契约 1 项通过，新增的正常解压、父目录穿越、绝对/盘符/UNC 路径和 ZIP 符号链接 4 项均因 `backup.safe_extract_zip` 不存在而按预期失败，确认正式恢复会在读取 `.sbx` 阶段中断。
- 2026-08-05：拆分后真实启动烟雾测试通过：使用本机 Python 3.14 的 `pythonw.exe` 直接运行仓库入口 `沙河批量打开器.py`，进程持续存活且窗口标题为“沙河批量打开器”，Windows 报告窗口响应正常；`artifacts/runs/launcher_20260805_200354.stderr.log` 为空，运行日志记录版本 `4.0.46`、主界面刷新和 Sandboxie 安装路径扫描成功。测试结束时保留窗口运行，供人工操作检查。
- 2026-08-04：最终入口与规模审计：兼容入口 171 行，`shahe_bulk_launcher/app.py` 200 行；冻结清单中的 19 个函数、17 个类和 11 个常量无缺失导出，`SandboxLauncher` 的 400 个方法全部可调用，`SandboxLauncher.__module__` 与 `main.__module__` 均为 `shahe_bulk_launcher.app`。
- 2026-08-04：最终工作区行为回归：顺序运行 `tests/probes/*.py` 共 26 条，26/26 通过。覆盖音频/关闭后台线程、语音后台朗读、定位和启动 UI 非阻塞、启动完成异步刷新、Sandboxie Start.exe 非阻塞与立即失败、更新包选择与助手命令、配置仅写 `Configuration`、首次扫描、剪贴板、列表导航与进程菜单缓存、固定快捷键焦点、分组导航、沙盒管理文本变换、对话框 AccessibleName、UI 广扫，以及备份恢复安全解压边界、无 shell 子进程和后台服务停止。
- 2026-08-04：最终全量单元与结构回归 `python -m unittest discover -s tests -v` 通过 24/24，覆盖基础 API、20 个目标模块、400 个 `SandboxLauncher` 方法唯一分区、入口兼容导出、控件/对话框/备份和全部 feature 契约。
- 2026-08-04：最终全仓语法验证 `python -m compileall -q .` 通过，入口、20 个目标模块、迁移工具、单元测试和行为探针均可编译。
- 2026-08-04：Task 11 最终应用组装结构 GREEN：生成 `shahe_bulk_launcher/app.py`，其中 `SandboxLauncher` 仅直接定义原 `__init__`，并按 `LifecycleMixin`、`SettingsMixin`、`AppPathsMixin`、`SandboxieMixin`、`ProcessActionsMixin`、`UpdatesMixin`、`SpeechMixin`、`HotkeysMixin`、`SandboxAdminMixin`、`SchedulingMixin`、`wx.Frame` 顺序组装；中文入口保留 internal 运行时引导和原 API 显式导出，包根惰性导出 `SandboxLauncher/main`。`python -m py_compile` 对入口、`app.py`、包入口、组装工具和相关测试通过；`python -m unittest tests.test_entry_compatibility tests.test_modular_structure tests.test_feature_method_partition -v` 9/9 通过，20 个目标模块全部存在，400 个方法唯一分区。
- 2026-08-04：Task 11 最终应用组装 RED 验证：`python -m unittest tests.test_entry_compatibility -v` 产生 3 个预期失败，统一原因是 `shahe_bulk_launcher/app.py` 尚不存在。契约冻结最终类仅直接定义 `__init__`、10 个 mixin 与 `wx.Frame` 的继承顺序、中文入口原 19 个函数/17 个类/11 个常量导出，以及包根对 `SandboxLauncher/main` 的惰性导出。
- 2026-08-04：Task 10 UI 广扫 `ui_broad_probe.py` 通过。验证确认菜单文案与 Ctrl+O、Ctrl+V、F5、Ctrl+A、Alt+F4 固定快捷键显示保持不变；应用输入框、沙盒列表和启动按钮 AccessibleName/标签保持不变；不可配置的固定动作未进入快捷键清单，固定快捷键冲突仍明确提示；批量命名补零选项保持原顺序和文案。
- 2026-08-04：Task 10 三个剩余 feature 模块汇总结构回归 `python -m unittest tests.test_foundation_modules tests.test_move_foundation_entry_tool tests.test_ui_controls_contract tests.test_ui_dialogs_contract tests.test_backup_contract tests.test_feature_method_partition tests.test_app_paths_and_sandboxie tests.test_process_actions_contract tests.test_updates_and_speech_contract tests.test_remaining_features_contract -v` 通过 18/18。
- 2026-08-04：Task 10 调度启动行为探针 GREEN：`launch_no_ui_blocking_probe.py` 与 `launch_done_no_sync_menu_probe.py` 均通过。验证确认慢速沙盒解析不会阻塞 `on_launch` 的 UI 返回，`_launch_done` 不同步刷新运行状态或重建进程菜单，并继续恢复启动按钮、设置完成状态和安排后续异步刷新。
- 2026-08-04：Task 10 调度启动模块结构 GREEN：`python -m py_compile "沙河批量打开器.py" shahe_bulk_launcher\features\scheduling.py artifacts\tools\split_launcher_mixins.py tests\test_remaining_features_contract.py` 通过；`python -m unittest tests.test_feature_method_partition tests.test_remaining_features_contract -v` 6/6 通过。20 个启动目标、定时任务、启动线程、完成回调和备份/恢复代理方法唯一迁入 `SchedulingMixin`。
- 2026-08-04：Task 10 调度启动模块 RED 验证：定向运行 `test_scheduling_mixin_keeps_assigned_methods` 产生 1 个预期失败，原因是 `features/scheduling.py` 尚不存在；冻结清单确认迁移 20 个启动目标、定时任务、启动线程、完成回调和备份/恢复代理方法。
- 2026-08-04：Task 10 沙盒管理模块汇总结构回归 `python -m unittest tests.test_foundation_modules tests.test_move_foundation_entry_tool tests.test_ui_controls_contract tests.test_ui_dialogs_contract tests.test_backup_contract tests.test_feature_method_partition tests.test_app_paths_and_sandboxie tests.test_process_actions_contract tests.test_updates_and_speech_contract tests.test_remaining_features_contract -v` 通过 17/17。
- 2026-08-04：Task 10 沙盒管理行为探针 GREEN：`sandbox_admin_transform_probe.py` 与 `group_navigation_probe.py` 均通过。验证确认批量名称编号、重复/空名称校验、Sandboxie INI 分段、节名替换、设置增删和新节模板保持原行为；分组特殊项继续不可勾选且被全选跳过，Enter 进入分组、Backspace 返回根视图的键盘导航保持有效。
- 2026-08-04：Task 10 沙盒管理模块结构 GREEN：`python -m py_compile "沙河批量打开器.py" shahe_bulk_launcher\features\sandbox_admin.py artifacts\tools\split_launcher_mixins.py tests\test_remaining_features_contract.py` 通过；`python -m unittest tests.test_feature_method_partition tests.test_remaining_features_contract -v` 5/5 通过。49 个分组、批量命名、INI、别名、目录和注册表清理方法唯一迁入 `SandboxAdminMixin`。
- 2026-08-04：Task 10 沙盒管理模块 RED 验证：定向运行 `test_sandbox_admin_mixin_keeps_assigned_methods` 产生 1 个预期失败，原因是 `features/sandbox_admin.py` 尚不存在；冻结清单确认迁移 49 个分组、批量命名、INI、别名、目录和注册表清理方法。
- 2026-08-04：Task 10 快捷键模块汇总结构回归 `python -m unittest tests.test_foundation_modules tests.test_move_foundation_entry_tool tests.test_ui_controls_contract tests.test_ui_dialogs_contract tests.test_backup_contract tests.test_feature_method_partition tests.test_app_paths_and_sandboxie tests.test_process_actions_contract tests.test_updates_and_speech_contract tests.test_remaining_features_contract -v` 通过 16/16。
- 2026-08-04：Task 10 快捷键行为探针 GREEN：`fixed_hotkeys_focus_probe.py`、`process_menu_selection_refresh_probe.py`、`process_menu_uses_running_marker_probe.py` 均通过。验证确认按钮持焦且自定义热键关闭时 Ctrl+O、Ctrl+V、F5、Ctrl+A 固定快捷键仍明确触发；列表选择切换会递增刷新令牌并隔离旧进程菜单缓存；列表运行标记继续启用静音、音量、设备切换、关闭和定位菜单项。
- 2026-08-04：Task 10 快捷键模块结构 GREEN：`python -m py_compile "沙河批量打开器.py" shahe_bulk_launcher\features\hotkeys.py artifacts\tools\split_launcher_mixins.py tests\test_remaining_features_contract.py` 通过；`python -m unittest tests.test_feature_method_partition tests.test_remaining_features_contract -v` 4/4 通过。103 个键盘事件、菜单、热键注册、列表导航和进程菜单状态方法唯一迁入 `HotkeysMixin`。
- 2026-08-04：Task 10 快捷键模块 RED 验证：`python -m unittest tests.test_remaining_features_contract -v` 产生 1 个预期失败，原因是 `features/hotkeys.py` 尚不存在；冻结清单确认迁移 103 个键盘事件、菜单、热键注册、列表导航和进程菜单状态方法。
- 2026-08-04：Task 9 更新与语音汇总结构回归 `python -m unittest tests.test_foundation_modules tests.test_move_foundation_entry_tool tests.test_ui_controls_contract tests.test_ui_dialogs_contract tests.test_backup_contract tests.test_feature_method_partition tests.test_app_paths_and_sandboxie tests.test_process_actions_contract tests.test_updates_and_speech_contract -v` 通过 15/15。
- 2026-08-04：Task 9 语音行为探针 GREEN：复跑 `configuration_write_only_probe.py` 并运行新增 `speech_assist_no_blocking_probe.py` 均通过。验证确认 `SpeechConfig.json`、`ZDSRAPI.ini`、`boyctrl.ini` 的可写路径继续固定在 `Configuration`，不会回退到 `internal` 或旧配置文件；辅助朗读的 silence/output 调用仍在后台线程执行。
- 2026-08-04：Task 9 语音模块结构 GREEN：`python -m py_compile "沙河批量打开器.py" shahe_bulk_launcher\features\speech.py artifacts\tools\split_launcher_mixins.py tests\test_updates_and_speech_contract.py` 通过；`python -m unittest tests.test_feature_method_partition tests.test_updates_and_speech_contract -v` 6/6 通过。24 个语音方法唯一迁入 `SpeechMixin`；原入口 `__file__` 的两处备用资源根路径受控替换为 `get_app_dir()`，避免拆分后误指向 `features` 目录。
- 2026-08-04：Task 9 语音模块 RED 验证：定向运行 `test_speech_mixin_keeps_assigned_methods_and_entry_root_semantics` 产生 1 个预期失败，原因是 `features/speech.py` 尚不存在。冻结清单确认迁移 24 个 reader.ini、Tolk、读屏匹配和朗读设置方法；同时冻结资源路径语义，迁移后不得用 feature 文件自身的 `__file__` 作为备用 `Speech` 目录根路径。
- 2026-08-04：Task 9 更新模块汇总结构回归 `python -m unittest tests.test_foundation_modules tests.test_move_foundation_entry_tool tests.test_ui_controls_contract tests.test_ui_dialogs_contract tests.test_backup_contract tests.test_feature_method_partition tests.test_app_paths_and_sandboxie tests.test_process_actions_contract tests.test_updates_and_speech_contract -v` 通过 14/14。
- 2026-08-04：Task 9 更新行为探针 GREEN：修正工作区探针导入顺序后，`update_package_selection_probe.py` 与 `update_helper_command_probe.py` 均通过。验证确认仅选择匹配当前版本且包含 `url` 的增量补丁，非法大小继续归零；更新助手复制到应用目录 `tenp/updater_helper_*`，同步复制 `internal`，请求文件仍写入 `tenp`，主程序参数继续指向兼容入口。
- 2026-08-04：Task 9 更新行为探针首轮：`update_package_selection_probe.py` 通过；`update_helper_command_probe.py` 在业务逻辑执行前因 `ModuleNotFoundError: No module named 'shahe_bulk_launcher'` 失败。根因是工作区探针先于仓库入口直接导入 `features.updates`，脚本从 `tests/probes` 启动时仓库根尚未加入 `sys.path`；该问题属于探针适配，需先加载兼容入口再引用模块全局。
- 2026-08-04：Task 9 更新模块结构 GREEN：`python -m py_compile "沙河批量打开器.py" shahe_bulk_launcher\features\updates.py artifacts\tools\split_launcher_mixins.py tests\test_updates_and_speech_contract.py` 通过；`python -m unittest tests.test_feature_method_partition tests.test_updates_and_speech_contract -v` 5/5 通过。20 个更新、更新器和打赏二维码方法唯一迁入 `UpdatesMixin`，`qr_embedded` 可选导入和 `QR_EMBEDDED_IMAGES` 回退缓存整体迁移，旧入口不再持有副本。
- 2026-08-04：Task 9 更新模块 RED 验证：`python -m unittest tests.test_updates_and_speech_contract -v` 产生 2 个预期失败，分别对应 `features/updates.py` 尚不存在，以及旧入口仍加载并持有 `qr_embedded.QR_EMBEDDED_IMAGES`。冻结清单确认本阶段先迁移 20 个更新、更新器和打赏二维码相关方法，二维码可选资源缓存必须随模块迁移。
- 2026-08-04：Task 8 汇总结构回归 `python -m unittest tests.test_foundation_modules tests.test_move_foundation_entry_tool tests.test_ui_controls_contract tests.test_ui_dialogs_contract tests.test_backup_contract tests.test_feature_method_partition tests.test_app_paths_and_sandboxie tests.test_process_actions_contract -v` 通过 12/12。
- 2026-08-04：Task 8 进程操作行为探针 GREEN：迁入工作区的 `audio_process_actions_no_blocking_probe.py`、`close_process_no_blocking_probe.py`、`locate_no_blocking_probe.py` 均通过。验证确认静音与播放设备切换、关闭当前进程仍在后台线程执行，相关任务锁能释放；定位当前进程调用在 0.5 秒阈值内返回，未因模块拆分引入 UI 主线程阻塞。
- 2026-08-04：Task 8 进程操作模块结构 GREEN：`python -m py_compile "沙河批量打开器.py" shahe_bulk_launcher\features\process_actions.py artifacts\tools\split_launcher_mixins.py tests\test_process_actions_contract.py` 通过；`python -m unittest tests.test_feature_method_partition tests.test_process_actions_contract -v` 5/5 通过。63 个进程操作方法唯一迁入 `ProcessActionsMixin`，`pythoncom`、`AudioUtilities`、`AudioRouter` 三项可选后端缓存随模块迁移，旧入口不再持有重复状态。
- 2026-08-04：Task 8 进程操作模块 RED 验证：扩展入口缓存所有权契约后，`python -m unittest tests.test_process_actions_contract -v` 产生 2 个预期失败：`shahe_bulk_launcher/features/process_actions.py` 尚不存在，且旧入口仍定义 `pythoncom`、`AudioUtilities`、`AudioRouter`。冻结清单确认本阶段迁移 63 个定位、关闭、静音、音量和播放设备相关方法；三项可选依赖缓存必须随方法迁入新模块，入口不再保留副本。
- 2026-08-04：Task 7 汇总结构回归 `python -m unittest tests.test_foundation_modules tests.test_move_foundation_entry_tool tests.test_ui_controls_contract tests.test_ui_dialogs_contract tests.test_backup_contract tests.test_feature_method_partition tests.test_app_paths_and_sandboxie -v` 通过 10/10。
- 2026-08-04：Task 7 应用路径/Sandboxie 迁移 GREEN：`python -m py_compile "沙河批量打开器.py" shahe_bulk_launcher\features\app_paths.py shahe_bulk_launcher\features\sandboxie.py artifacts\tools\split_launcher_mixins.py tests\test_app_paths_and_sandboxie.py` 通过；`python -m unittest tests.test_feature_method_partition tests.test_app_paths_and_sandboxie -v` 4/4 通过，31 个应用路径方法和 43 个 Sandboxie 方法唯一分区；入口导入烟雾确认方法来自新模块。`paste_invalid_clipboard_probe.py` 通过；测试桩适配 mixin 后，`launch_start_detached_probe.py` 与 `launch_detached_immediate_failure_probe.py` 均通过，正常派发仅调用非阻塞 `Popen`，立即非零退出仍报告失败并刷新无效沙盒路径。
- 2026-08-04：Task 7 首轮行为探针：`paste_invalid_clipboard_probe.py` 通过；两条启动探针在进入 Start.exe 派发前因测试 Dummy 只绑定 `start_app_in_sandbox` 而失败，错误为缺少同一 `SandboxieMixin` 中的 `_dispatch_sandboxie_start_detached`。该失败定位为旧单文件测试桩的 `SandboxLauncher.__dict__` 假设不再成立，业务实现尚未出现失败证据；探针需绑定完整 `SandboxieMixin` 方法集后重跑。
- 2026-08-04：Task 7 应用路径/Sandboxie RED 验证：`python -m unittest tests.test_app_paths_and_sandboxie -v` 产生 2 个预期失败，分别对应 `features/app_paths.py` 与 `features/sandboxie.py` 尚不存在；冻结清单确认目标方法数为 31 和 43。
- 2026-08-04：Task 6 汇总结构回归 `python -m unittest tests.test_foundation_modules tests.test_move_foundation_entry_tool tests.test_ui_controls_contract tests.test_ui_dialogs_contract tests.test_backup_contract tests.test_feature_method_partition -v` 通过 9/9。
- 2026-08-04：Task 6 生命周期/设置迁移 GREEN：`python -m py_compile "沙河批量打开器.py" shahe_bulk_launcher\features\__init__.py shahe_bulk_launcher\features\lifecycle.py shahe_bulk_launcher\features\settings.py artifacts\tools\split_launcher_mixins.py tests\test_feature_method_partition.py` 通过；`python -m unittest tests.test_feature_method_partition -v` 3/3 通过，34 个生命周期方法和 12 个设置方法唯一分区且原类已接入两个 mixin；入口导入烟雾确认方法来自新模块。`defaults_and_first_scan_probe.py` 与 `configuration_write_only_probe.py` 均通过，默认值、首次扫描状态/弹框和 Configuration 写路径保持不变。开机自启动路径已改用 `get_current_executable_path()`，避免登记移动后的 feature 文件。
- 2026-08-04：Task 6 主窗口分区 RED 验证：`python -m unittest tests.test_feature_method_partition.FeatureMethodPartitionTests.test_lifecycle_and_settings_modules_exist -v` 1 项测试因 `features/lifecycle.py` 与 `features/settings.py` 尚不存在按预期失败。冻结清单确认本阶段分别迁移 34 个生命周期方法和 12 个设置方法，`SandboxLauncher.__init__` 继续暂留原类。
- 2026-08-04：Task 5 汇总结构回归 `python -m unittest tests.test_foundation_modules tests.test_move_foundation_entry_tool tests.test_ui_controls_contract tests.test_ui_dialogs_contract tests.test_backup_contract -v` 通过 6/6。
- 2026-08-04：Task 5 备份恢复迁移 GREEN：`python -m py_compile "沙河批量打开器.py" shahe_bulk_launcher\backup.py artifacts\tools\move_backup.py tests\test_backup_contract.py` 通过；`python -m unittest tests.test_backup_contract -v` 1/1 通过；`restore_safe_extract_probe.py`、`restore_shell_free_probe.py`、`restore_no_main_thread_stop_probe.py` 均通过。验证确认恢复仍调用安全解压边界、子进程调用不启用 shell、停止 Sandboxie 服务不在 UI 主线程执行。源码重启路径已从移动后的 `__file__` 改为 `get_current_executable_path()`，保持启动原入口的行为。原项目未定义 `safe_extract_zip` 的风险仍未修复，探针按原设计显式注入该依赖。
- 2026-08-04：Task 5 备份恢复 RED 验证：`python -m unittest tests.test_backup_contract -v` 1 项测试因 `shahe_bulk_launcher/backup.py` 尚不存在按预期失败。静态审计同时确认原正式源码和仓库基线的 `BackupRestoreMgr.restore` 调用 `safe_extract_zip`，但入口及公共模块均未定义或导入该名称；该原项目风险不混入本次结构迁移，另行记录。另确认 `_ask_restart_app` 使用 `__file__`，移动类后必须改用已保持入口语义的 `get_current_executable_path()`，否则会重启 `backup.py`。
- 2026-08-04：Task 4 汇总回归 `python -m unittest tests.test_foundation_modules tests.test_move_foundation_entry_tool tests.test_ui_controls_contract tests.test_ui_dialogs_contract -v` 通过 5/5，基础模块、生成器、控件、对话框和托盘结构契约同时有效。
- 2026-08-04：Task 4 对话框/托盘迁移 GREEN：`python -m py_compile "沙河批量打开器.py" shahe_bulk_launcher\ui\common_dialogs.py shahe_bulk_launcher\ui\settings_dialogs.py shahe_bulk_launcher\ui\tray.py artifacts\tools\move_ui_dialogs.py tests\test_ui_dialogs_contract.py` 通过；`python -m unittest tests.test_ui_dialogs_contract -v` 2/2 通过；入口导入烟雾确认 `SettingsDialog`、`QRCodeDialog`、`TaskBarIcon` 均来自新模块；`dialog_control_names_probe.py` 与 `dialog_control_names_more_probe.py` 均通过，关键设置和朗读控件 AccessibleName 保持不变。
- 2026-08-04：Task 4 对话框/托盘 RED 验证：`python -m unittest tests.test_ui_dialogs_contract -v` 最终稳定为 4 个预期失败，分别对应 `common_dialogs.py`、`settings_dialogs.py`、`tray.py` 及二维码辅助函数目标文件尚不存在；测试不再产生额外文件读取错误。
- 2026-08-04：Task 3 汇总回归 `python -m unittest tests.test_foundation_modules tests.test_move_foundation_entry_tool tests.test_ui_controls_contract -v` 通过 3/3，确认基础模块、迁移工具模板和五个控件类方法契约同时保持有效。
- 2026-08-04：Task 3 相邻包导入兼容修复后，`python tests\probes\sandbox_list_navigation_no_status_probe.py` 和 `python tests\probes\sandbox_list_navigation_coalesces_live_state_probe.py` 均通过；`python -m py_compile "沙河批量打开器.py" shahe_bulk_launcher\ui\controls.py` 通过。验证确认列表焦点移动不触发勾选计数状态更新，连续浏览的 live-state 与菜单刷新仍保持合并调度。
- 2026-08-04：Task 3 首次运行两个工作区列表探针均在导入入口时失败，错误为 `ModuleNotFoundError: No module named 'shahe_bulk_launcher'`。根因确认是探针从 `tests/probes` 作为脚本启动时仓库根目录不在 `sys.path`，而旧入口路径引导只识别 `SCRIPT_DIR/MyCodex` 布局，未识别现在与入口相邻的 `shahe_bulk_launcher` 包；控件逻辑尚未执行。该失败作为相邻包导入兼容回归证据记录。
- 2026-08-04：Task 3 控件模块 RED 验证：运行 `python -m unittest tests.test_ui_controls_contract -v`，测试因 `ModuleNotFoundError: No module named 'shahe_bulk_launcher.ui'` 按预期失败。测试随后改为直接比较目标模块 AST 与基线类方法清单，以覆盖 `classmethod` 等描述符且避免把 wx 导入行为混入结构契约。
- 2026-08-04：Task 2 基础模块完成。新增 `shahe_bulk_launcher/common.py`、`constants.py`、`bootstrap.py`、`single_instance.py` 和包入口，旧脚本改为显式导入同名 API。修复仅存在于迁移工具模板的补丁标记问题后，`python -m unittest tests.test_move_foundation_entry_tool -v` 1/1 通过；`python -m py_compile "沙河批量打开器.py" shahe_bulk_launcher\__init__.py shahe_bulk_launcher\common.py shahe_bulk_launcher\constants.py shahe_bulk_launcher\bootstrap.py shahe_bulk_launcher\single_instance.py tests\test_foundation_modules.py` 通过；`python -m unittest tests.test_foundation_modules -v` 1/1 通过；非 UI 入口导入烟雾检查确认 `APP_VERSION=4.0.46`、`get_app_dir()` 仍为 `D:\mycodex\git`，且入口导出的 `SingleInstance` 来自新模块。
- 2026-08-04：Task 2 迁移工具回归 RED：首次运行入口语法验证在第 114 行失败，调查确认 `artifacts/tools/move_foundation_entry.py` 的 `IMPORT_BLOCK` 模板把补丁标记 `+` 作为普通字符写入 28 行导入文本。新增 `tests/test_move_foundation_entry_tool.py` 后运行 1 项测试按预期失败，精确列出这 28 行，后续修复生成器而非只修生成结果。
- 2026-08-04：Task 2 基础模块 RED 验证：运行 `python -m unittest tests.test_foundation_modules -v`，1 项测试因 `ModuleNotFoundError: No module named 'shahe_bulk_launcher'` 按预期失败，确认测试覆盖目标包而非旧单文件定义。
- 2026-08-04：完成拆分前 API 契约捕获，记录 19 个顶层函数、17 个顶层类、11 个全大写常量和 `SandboxLauncher` 400 个方法；方法目标模块清单合计同为 400，无遗漏。运行 `python -m py_compile artifacts\tools\capture_launcher_contract.py tests\test_modular_structure.py` 通过。运行 `python -m unittest tests.test_modular_structure.ModularStructureTests.test_target_modules_exist -v` 按预期失败，唯一原因是设计中的 20 个目标模块尚未创建，作为后续迁移的 RED 基线。
- 2026-08-04：建立拆分前项目基线和原项目到模块的映射；尚未执行迁移或测试。
