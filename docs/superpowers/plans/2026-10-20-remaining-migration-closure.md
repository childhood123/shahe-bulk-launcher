# Python→C# 剩余迁移收口计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. 每个模块必须独立完成映射、RED、GREEN、回归、文档、提交和推送。

**Goal:** 收口全部已确认的 Python→C# 行为差异、运行时接线、真实 EXE 审计和发布流程，最终满足 `AGENTS.md` 的整体迁移完成标准。

**Architecture:** 保持现有 Core、App、Updater 和 AuditRunner 边界，不重设计 UI。VOICE-03 已按用户单读屏要求收口，AUDIT-03 当前仅完成证据审查和独立提交；提交推送后按用户要求暂停，后续审计不启动。

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
| 已完成，不重做 | Task 1–20 已确认范围、GAP-01 至 GAP-10、VOICE-01、VOICE-02、UPDATE-01、AUDIT-01、AUDIT-02；以项目结构最新有效证据为准 |
| 已验收追加修复 | VOICE-03 读屏固定选择与临时回退，按用户单读屏范围完成 |
| 当前模块 | AUDIT-03 验证已完成，独立提交/推送后暂停 |
| 后续真实审计 | AUDIT-04 至 AUDIT-08；按用户要求暂停，尚未启动下一项 |
| 最终收口 | RELEASE-01 |

当前 C# 主程序能够启动、显示主窗口、执行已验证功能并正常退出；Updater 生产入口已接通并通过隔离真实进程验证。整体迁移尚未完成。

## 严格执行顺序

VOICE-03 已按用户单读屏范围验收并推送独立修复分支。以下既定迁移顺序保持不变，AUDIT-03 验证已补齐，完成独立提交/推送后暂停；只有用户明确恢复工作时才进入 AUDIT-04。

1. GAP-09 路径记忆数量范围
2. GAP-10 路径选择与剪贴板文案
3. VOICE-02 读屏配置与 `lib` 运行时接线
4. AUDIT-01 至 AUDIT-08 真实审计
5. RELEASE-01 最终发布与整体完成门禁

禁止跳过当前模块，也禁止把多个模块混合迁移后统一测试。已完成的更新链路不得重复迁移；后续版本发布只更新版本、包、清单和校验证据。

## 已完成且不得重复

- Python 20 个模块对应的 C# 类型、服务和主窗口组装已完成已确认范围。
- `Program.cs`、`MainForm`、单实例和 WinForms 生命周期已接入真实启动流程。
- GAP-03 复杂 JSON、GAP-04 朗读配置保留、GAP-05 剪贴板文件优先、GAP-06 全选切换已完成并推送。
- VOICE-01 Tolk x64 已完成 ZDSR、NVDA、OneCore 回退和生产 `SpeechService` 原生调用验证。
- UPDATE-01、GAP-01、GAP-02 已完成 GitCode 默认源、更新 UI、完整包、Updater 安装/回滚/重启和匿名发布验证；当前 `4.0.46` 清单是同版本发布基线。
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

- [x] 建立 Esc、固定 Alt+F4、文件菜单退出的运行时 RED。
- [x] 固定状态文案：“已最小化到托盘”“当前托盘不可用，已最小化窗口”“正在退出软件”。
- [x] 固定 `SpeechCallback(text, "launch_done")` 调用次数和先后顺序。
- [x] 复用现有 `MinimizeToTray()` 和关闭生命周期，不修改用户已有 `HotkeyController.cs`。
- [x] 回归托盘关闭隐藏、ForceExit、重复关闭、Tolk 卸载和无托盘场景。
- [x] 完成共用门禁、真实键盘/菜单验证、文档、独立提交和推送。

### GAP-08: Sandboxie repair reload

**Python reference:** `shahe_bulk_launcher/features/sandbox_admin.py::_repair_sandboxie_control_user_settings`

**Files:**
- Modify: `csharp/ShaheLauncher.App/MainForm.cs`
- Create: `csharp/ShaheLauncher.Tests/SandboxieRepairReloadRuntimeContractTests.cs`

- [x] RED 证明正常构造路径的 `ReloadSandboxieConfigCallback` 当前为 null。
- [x] 接线已有 `ReloadSandboxieConfig()`；不得新增第二套 `/reload` 命令实现。
- [x] 验证只有实际写入配置后调用一次 reload。
- [x] 验证取消、无需修复、写入失败和 reload 异常反馈。
- [x] 真实验证只使用隔离 INI，不修改用户 Sandboxie 资源。
- [x] 完成共用门禁、文档、独立提交和推送。

### GAP-09: 路径记忆数量范围（用户指定最大 999）

**Python reference:** `features/settings.py`、`features/app_paths.py`、`ui/settings_dialogs.py`。Python 原版上限为 20；用户已明确要求 C# 上限采用 999，本模块将该差异记录为产品要求，不修改 Python 源码。

**Design:** `docs/superpowers/specs/2026-10-21-path-history-999-design.md`

**Files:**
- Modify: `csharp/ShaheLauncher.App/MainForm.cs`
- Modify: `csharp/ShaheLauncher.Core/AppPaths/AppPathHistoryService.cs`
- Modify: `csharp/ShaheLauncher.App/UI/Controls/AppPathListCtrl.cs`
- Modify: `csharp/ShaheLauncher.App/UI/Dialogs/Settings/PathMemoryManagerDialog.cs`
- Modify: `csharp/ShaheLauncher.Core/Settings/LauncherSettingsService.cs`
- Modify targeted assertions only: `csharp/ShaheLauncher.Tests/SettingsDialogsCSharpContractTests.cs`
- Modify targeted assertions only: `csharp/ShaheLauncher.Tests/LauncherSettingsServiceContractTests.cs`
- Create: `csharp/ShaheLauncher.Tests/PathHistoryLimitRuntimeContractTests.cs`

- [x] 对 `-1`、`0`、`20`、`21`、`999`、`1000` 和无效值建立 C# 运行时 RED，同时记录 Python 原版 `0–20` 与用户指定 `0–999` 的有意差异。
- [x] 在 `AppPathHistoryService` 建立唯一最大值 `999`，统一启动加载、状态提交、管理器保存、其他设置保存、右键调整入口和历史裁剪。
- [x] 将提示文案更新为“(0=不记录, 最大999)”；保留其余 Python 文案、快捷键、焦点顺序、`TabIndex` 和 `AccessibleName`。
- [x] 回归 `LauncherSettingsService`、两个数量设置入口、保存重载、历史裁剪和多条历史启动空项防护；验证负数归零、超过上限归 `999`、无效启动值归默认 `10`。
- [x] 完成真实生产 UI 模态对话框的 `0/21/999` 保存与重启恢复验证，并完成修复后二进制真实 EXE 启动烟雾验证。
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

### VOICE-03: 读屏固定选择与临时回退

**目标：** 手动选择始终作为保存偏好；所选读屏可用时使用它，
不可用时临时自动检测其他读屏，均不可用时回退 Windows 语音；
所选读屏恢复后重新使用它，点击确定不得覆盖保存偏好。

**边界：** 作为用户明确要求的独立功能修复，保留现有文案、
选项顺序和配置键；不使用子代理，不关闭用户运行中的读屏。

**涉及文件：**
- csharp/ShaheLauncher.Core/Speech/SpeechService.cs
- csharp/ShaheLauncher.Tests/SpeechSelectionRuntimeContractTests.cs
- artifacts/runs/speech_selection_20261022/native-selection-probe.ps1
- artifacts/runs/speech_selection_20261022/run-verification.ps1
- PROJECT_STRUCTURE.md
- 本活动计划

**执行步骤与记录：**
- [x] 接续核对：读取旧会话要求、活动计划、现有差异和最后有效报告。
- [x] 核对既有 RED/GREEN 对应关系及所有受影响调用入口。
- [x] 重跑读屏定向测试，确认当前源码与既有验证结果一致。
- [x] 按用户最新要求，以一个实际运行的读屏验证固定选择、输出调用及保存偏好；取消双读屏同时运行要求。
- [x] 验证保益不可用时的临时回退，区分检测成功与实际输出调用。
- [x] 重新连接已有单测和模拟枚举下的真实 DLL 证据；按用户本轮单读屏验收要求，真实启动恢复保留为未验收风险。本轮自有进程清理已核对。
- [x] 核对 Windows 回退证据，明确模拟进程枚举与真实环境的区别。
- [x] 已确认的功能偏差均有既有 RED/GREEN 对应；本轮未确认新的可复现功能偏差，首次原生异常单列为未定位风险，不宣称修复。
- [x] 运行受影响回归及 C# 全量测试，每轮结束立即同步项目结构。
- [x] 运行 Python 对照、compileall、隔离 Release x64 构建和差异检查，
      每项结束立即同步项目结构。
- [x] 核对全部步骤和风险，列出暂存文件，独立提交并推送。
- [x] 更新当前入口，恢复 AUDIT-03 的第一个未完成场景。

**验证命令：**
- 定向：使用现有 run-verification.ps1，Kind=test，
  Filter=FullyQualifiedName~SpeechSelectionRuntimeContractTests|FullyQualifiedName~TolkRuntimeIntegrationContractTests。
- 原生：使用现有 native-selection-probe.ps1，
  Selection 分别为 nvda、zdsr、boy、automatic；输出验证须带 Speak。
- 全量及构建：使用本计划“共用验证命令”，BaseOutputPath 指向
  artifacts/runs/speech_selection_20261022/bin/ 的绝对路径。

**每步记录：** 执行前标明当前步骤，结束后更新完成状态、
结果概述、下一步和风险；实时数字与日志索引只在项目结构中维护。

**完成标准：** 按用户“一个能成功就可以”的最新要求，单个真实读屏固定选择与输出成功即可完成本轮真实验收；
固定偏好与回退行为的既有回归证据保留，规定回归通过；
真实探针记录 PID、实际构建目录、场景及清理结果；
环境限制和未验证行为明确列出，不用模拟验证替代真实审计。

- 执行记录：已核对手动路由、无读屏 Windows 回退、偏好保持三组 RED/GREEN；调用链覆盖设置确认、启动加载、后台朗读和退出释放。旧 Python 仅保存选择的问题属于用户明确要求的独立修复。下一步重跑当前源码定向测试。

- 执行记录：当前源码读屏定向复验通过，既有三个需求阶段的行为仍保持；下一步进行真实读屏回退、双读屏选择及重新连接验证。

- 执行记录：进入真实场景验证；复用现有原生探针，增加同一 SpeechService 等待 NVDA 启动的有界检查，并验证再次保存同一选择保持原连接。验证脚本明确使用 UTF-8 记录并返回实际退出码。

- 执行记录：真实 NVDA 恢复探针在原生切换阶段异常退出，缺少完整报告及应用事件，按无效证据记录；保留专用 NVDA 测试进程用于定位，未动用户争渡。当前先缩小复现场景，不能据定向测试宣称恢复功能完成。

- 执行记录：准备双读屏验证时，原生报告显示 NVDA 选择实际落到争渡自动回退；探针通用 passed 不等于双读屏场景通过，当前核对测试 NVDA 是否仍运行。

- 执行记录：核对到测试 NVDA 曾正常退出，上一份双读屏报告只能算回退证据。为区分探针异常和真实原生故障，添加仅调试自己创建子进程的 Win32 诊断脚本，记录致命异常所在模块及已卸载模块；不附加用户进程。

- 执行记录：原生调试脚本的自有子进程基线通过。下一步在真实 DLL 上模拟“NVDA 进程已出现但 API 尚不可用”的启动窗口，缩小为反复初始化回退后端，捕获致命异常模块；该模拟不替代真实双读屏验证。

- 执行记录：模拟接口尚未就绪、但不发声时，重复初始化原生回退后端未出现异常；下一步仅增加实际输出调用，检查是否与发声后的卸载有关。

- 执行记录：增加输出调用后的重复初始化仍未复现异常，不能将先前退出归因于卸载。下一步在原生调试器下重跑真实 NVDA 启动恢复场景，记录进程级异常或完整恢复报告。

- 执行记录：真实恢复重跑时 NVDA 已由用户启动，初始条件不成立；不关闭用户 NVDA。诊断工具报告命名冲突已识别，后续使用独立名称。现在利用已存在的双读屏环境核对固定输出，再通过明确标注的枚举模拟检查同服务原生切换。

- 执行记录：双读屏实际运行时，手动 NVDA 已取得固定路由、输出调用、保存值保持和释放证据；下一步核对手动争渡。

- 执行记录：双读屏的手动 NVDA 通过，但手动争渡实际回退到 NVDA；保存值仍为争渡。该场景未通过，当前核对争渡 API 可用性及初始化返回值，未将自动回退当作手动固定成功。

- 执行记录：争渡回退的原因已核对为主读屏退出、服务仍在，原生初始化返回不可用；当前无法取得双读屏同时可用的验收证据。保留用户进程，继续完成同服务真实 DLL 切换（显式标记模拟枚举）与独立回归门禁。

- 执行记录：同一生产服务已完成 Windows OneCore → NVDA 的真实 DLL 输出与切换，保存偏好保持且再次保存不替换连接；初始无读屏状态由枚举注入模拟，未当作真实关闭/启动验收。原生调试器未捕获访问冲突。

- 执行记录：本机未安装保益的实际环境中，保存 boy 后临时经 Tolk 使用 NVDA，输出调用与释放均成功，保存值仍为 boy；未宣称保益自身发声已验证。下一步完成受影响及全量回归。

- 执行记录：受影响的朗读、设置对话框、设置运行时及主窗口回归通过，计数与命令已写入项目结构；当前进入不排除类别的 C# 全量测试。

- 执行记录：受影响回归与未排除任何类别的 C# 全量均通过，实际计数和返回码由项目结构对应记录维护。下一步逐项运行 Python 对照、compileall、隔离 Release 和差异检查。

- 执行记录：Python 原版对照测试通过，未修改 Python 需求或文案；下一步 compileall。

- 执行记录：Python 生产源码及测试 compileall 返回成功；下一步使用计划指定隔离输出目录构建 Release x64。

- 执行记录：隔离 Release x64 构建通过，生产程序集已编译且无警告/错误；下一步差异检查。

- 执行记录：清理归属核对完成：本轮专用 NVDA 和原生探针均无残留，用户 NVDA 与争渡服务保持运行。真实双读屏和无模拟启动恢复仍未完成，先保留未勾选状态。

- 执行记录：差异检查发现项目结构混合换行及两份既有 GAP-09 日志空白。只修正本轮文档换行，保留历史文字及用户日志；分别记录本模块检查与全仓检查，不把全仓失败写成通过。

- 执行记录：Python 对照、compileall、隔离 Release、本模块差异及新增文件空白检查均完成；全仓差异检查仍受既有 GAP-09 日志空白阻断，已如实记录。未修复或暂存用户已有无关改动。

- 执行记录：结束前逐项复核：代码门禁和测试进程清理已完成；双读屏实际验收、真实启动恢复和首次原生异常仍待补证，条件修复项尚不能关闭。保留 VOICE-03 为当前入口，不暂存、提交或推送；下一步需要稳定的 NVDA 与争渡主程序同时运行环境。

- 执行记录：用户明确调整本轮验收：不用双读屏，一个真实读屏成功即可。功能目标不变；以当前 NVDA 核对固定选择、实际输出调用和再次保存选择，双读屏/真实启动恢复与首次异常保留风险，不伪造通过。

- 执行记录：按用户缩减后的验收范围，真实 NVDA 固定选择、输出调用、同值再次确认保留连接和退出释放全部通过，用户 NVDA 进程保持不变。现在核对提交范围和完整验证索引。

- 执行记录：提交前自行复核完成：原生声明、固定偏好/临时路由分离、同值确认、锁顺序、关闭后不重建及设置持久化均与已通过测试一致。生产源码与对应测试在完整回归之后未修改；不重复无变化的全量测试。准备仅暂存本模块源码、测试、文档和顶层证据文件。

- 执行记录：首次暂存检查只发现两份原始差异日志引用的既有空白；已无损 gzip 归档并逐字节复核，原始本地日志保留。重新检查暂存差异后提交，未触碰用户 GAP-09 文件。

- 执行记录：选择性暂存清单和暂存差异检查通过；单读屏真实验收及既有完整回归满足用户当前范围。提交/推送尚未执行，执行后再标记该步骤完成并恢复 AUDIT-03。

- 执行记录：独立修复已本地提交，但首次推送因远端已有新提交被拒绝；不强推、不回退用户改动。先读取远端差异，需要时在隔离工作树整合后再次验证和推送。

- 执行记录：远端同名分支与本地无共同祖先，且只维护 C# 发布目录；采用独立远端修复分支推送当前已验证提交，保留现有发布分支，不做跨历史合并。

- 执行记录：独立修复已成功推送到独立远端分支；用户发布分支保持原状。代码与证据提交完成，下一步更新当前入口并恢复 AUDIT-03。

- 执行记录：当前入口已切回 AUDIT-03，先核对 delete-group 的既有审计入口及 SbieCtrl 安全门禁；本轮不重新打开已取消的双读屏验收。

**未完成风险：** 本机没有保益，无法验证其真实发声；
争渡主读屏当前未运行，双读屏同时可用与真实启动恢复尚未验收；
首次恢复探针异常退出尚未复现或定位，不能据自动化通过关闭风险；
已取得保益缺席回退的输出调用证据，但原生调用不等同于人工听感确认。
双读屏和真实启动恢复不作为用户本轮单读屏验收的阻断，保留为未验证风险；
首次异常不宣称已修复。整体迁移仍未完成，本模块单读屏验收通过后按既定规则提交。

### AUDIT-01: 定位当前进程

- [ ] 验证无窗口、单窗口、多窗口选择对话框、最小化窗口和失效句柄。
- [ ] 记录目标 PID、窗口句柄、选择结果、状态文案和清理结果。

### AUDIT-02: 路径记忆完整真实交互

- [ ] 在 GAP-03 修复后的真实 JSON 数组历史上重新验证启动恢复。
- [ ] 验证备注、删除当前、清空全部和经典上下文菜单键盘入口。
- [ ] 已通过的粘贴、选择、勾选、数量保存和隐藏路径不重复测试，除非受后续修改影响。

### AUDIT-03: Sandboxie 剩余生命周期

- [x] `delete-group` 成功路径：生产 EXE 的隔离配置中，删除指定分组、保留其他分组与设置、重启恢复和正常退出均通过。
- [x] `schedule-close`：新基线下真实启动两个本轮沙盒成员，计时回调按精确内部名关闭并验证进程清零；SbieCtrl 仍按既定门禁检查，仅在用户明确授权后正常退出。
- [x] `cancel-schedules`：生产调度器创建待执行启动/关闭任务后全部取消，任务集合清零且回调未执行。
- [x] `batch-rename` 和 `batch-delete`：新前缀两个真实沙盒的别名/分组成员更新及精确归属删除均通过。
- [x] 只读 `verify-clean` 已执行：三项哈希、精确所有权、section/目录及所有受控进程检查全部通过。

- 接续记录：当前先核对 delete-group 的生命周期安全门禁；中断后未发现本轮残留探针，用户 SbieCtrl 仍运行。复用已有只读 verify-clean，记录当前阻断和资源状态，不启动任何删除/关闭操作。

- 接续结果：只读 verify-clean 已执行；目标及配置哈希、精确资源归属检查通过，用户 SbieCtrl 仍运行，生命周期安全门禁阻断。delete-group 尚未执行，其他待办不勾选、不跳至 AUDIT-04；待用户关闭 Sandboxie Control 后再继续。

- 当前执行：软件 DeleteGroup 已接生产菜单，但既有 AuditRunner lifecycle 没有 delete-group 阶段。复用 Win32Automation 辅助方法，在生产 EXE 的专用副本和隔离配置中核对删除指定分组、保留其他分组/设置及重启恢复；使用隔离 Sandboxie 占位目录，不调用真实 Sandboxie 写操作，不修改或绕过既有 lifecycle 安全门禁。真实 Sandboxie 项仍受 SbieCtrl 门禁约束。

- 执行记录：首轮隔离 EXE 未取得主窗口句柄，探针超时并清理了精确归属的测试进程，标记 inconclusive；真实 Sandboxie 配置和用户控制程序均保持不变。先检查隐藏启动与窗口发现方式，不据此判定产品功能失败。

- 执行记录：第二轮已定位窗口，但探针向泛型菜单辅助函数传入 PowerShell Object[]，类型绑定失败；属于探针问题，未进入删除操作，仍记 inconclusive。补充显式 MenuCommandInfo[] 转换后复验，生产代码不变。

- 执行记录：第三轮真实菜单已打开删除分组窗口且前台确认成功，但尚未唯一匹配目标分组项，未确认删除；按控件/索引未校准记录 inconclusive。接下来读取实际控件名称并等待列表稳定，不盲选索引。

- 执行记录：观测确认该窗口在 UI Automation 中未暴露 ListItem，按钮也呈现 Pane；不再重复等待同一条件。探针改用现有 Win32 辅助层读取原生 LISTBOX 的完整项目文字，按精确名称定位并回读选中索引后显式确认。该发现保留给后续无障碍审计，不据此直接判为产品缺陷。

- 执行记录：原生列表已精确匹配目标名称并回读确认索引，确定按钮显式调用成功；配置回读阶段发现探针未显式加载 Core 程序集，仍记 inconclusive。补齐依赖加载后完成同一路径的保存与重启复验，生产代码不变。

- 执行记录：配置读取已进入，但 ReadSandboxGroups 是 internal 方法，探针的直接调用不可见；本轮仍属探针失败而非产品失败。与既有窗口辅助方法一致，改用已核对签名的反射调用，并在启动前解析该方法。

- 最新执行结果：删除分组隔离真实 EXE 验证通过，前台/精确名称/选中索引已校准，保存后重启仍正确，正常退出且无强制清理；真实 Sandboxie.ini、自启动项和用户 SbieCtrl 均保持原状。此前各轮探针问题继续保留为 inconclusive。当前下一项为 schedule-close，仍遵守 SbieCtrl 安全门禁，不跳过顺序。

- 收口记录：删除分组子场景的脚本、历次无效报告和最终通过报告集中保留；未修改生产源码，因此不重复无变化的全量构建/回归。本轮以真实 EXE 验证和脚本/差异检查收口，AUDIT-03 整体仍未完成，下一项 schedule-close 等待用户控制程序退出。

- 提交记录：删除分组子场景已形成独立本地提交；首次推送遇到连接重置，再次推送收到空响应，正在以单次 HTTP/1.1 设置重试。收到成功结果前不声明远端已更新。下一项 schedule-close 仍等待用户关闭或明确授权退出 Sandboxie Control。

- 最新阻断：HTTP/1.1 推送重试也因无法连接 GitHub 失败，证据仅确认在本地提交，远端未确认更新；网络恢复后重试既有提交，不重新实施或重复无变化测试。当前实际功能待办仍从 schedule-close 开始，SbieCtrl 未获退出授权前不执行。

- 当前授权与步骤：用户针对“正常退出 Sandboxie Control 后继续”的明确询问回复“开始”，已授权正常退出控制界面；保留服务和非本轮进程。先恢复并观察控制窗口，通过正常退出菜单关闭，再重新建立生命周期基线。Computer Use 状态捕获失败，已切换到现有 Open Computer Use；未执行强制结束。

- 控制界面操作恢复：两套可视化工具未可靠绑定目标；转为独立审计 CLI，复用既有 Win32Automation，严格匹配用户授权的 SbieCtrl.exe 路径、PID 和唯一“退出”菜单。先只读枚举，确认后仅发正常菜单命令，不调用 Kill/TerminateProcess。

- 控制退出前确认：只读原生枚举已获得精确目标的唯一“退出(&X)”菜单。按用户已给授权执行该正常菜单动作，并核对控制进程退出、服务保持运行；不调用强制终止。

- 控制退出结果：精确 PID、路径和唯一退出菜单均已核对；正常菜单命令返回 0，SbieCtrl 消失，SbieSvc 保持 Running，未使用强制终止。开始 schedule-close 真实生命周期阶段，复用既有通过基线和本轮所有权状态。

- schedule-close 首次有效执行结果：审计器返回 3 / inconclusive，错误为“Owned configuration changed before lifecycle.”；未启动目标沙盒、未执行定时关闭、无清理动作。当前核对真实配置文件、基线哈希和状态文件，不绕过安全门禁。

- 配置根因：旧 `real-lifecycle-state2.json` 的 ExpectedConfigurationSha256 为中间阶段值 中间阶段哈希，实际配置哈希已回到基线 原始应用配置基线哈希，因此 schedule-close 被安全门禁拒绝；真实 Sandboxie 当前无该旧前缀 section/目录/目标进程残留。保留旧状态不改，创建新唯一前缀和状态文件，从 batch-create 重新执行到 schedule-close。

- fresh batch-create 首次门禁：返回 3 / inconclusive，错误为 baseline hashes changed before lifecycle；核对发现真实 Sandboxie.ini 哈希从旧基线 控制程序退出前的配置哈希 变为当前 当前环境配置哈希，目标与应用配置哈希未变。原因是正常退出控制程序后配置文件被规范化；旧基线不覆盖，先运行当前环境只读 preflight 生成新基线。

- 当前 preflight 首次调用：返回 3 / blocked，报告已读取当前环境但 targetHash/configHash/iniHash 因未提供 expected 参数为 false；未修改文件。已确认 preflight 设计要求显式期望哈希，下一步用当前只读核对出的三个哈希生成正式新基线。

- fresh preflight 与 batch-create：当前环境正式基线通过，batch-create 通过并建立新前缀所有权；继续 create-group。每个阶段完成后立即记录，不复用旧状态。

- fresh create-group：新前缀分组及配置持久化通过；继续 launch-group。

- fresh launch-group：两个本轮沙盒成员启动及清理通过；继续 schedule-launch，再进入 schedule-close。

- fresh schedule-launch 首次运行被重新出现的 SbieCtrl 门禁阻断；此前 batch-create/create-group/launch-group 已通过且所有权状态仍有效。按已有用户授权再次精确正常退出控制程序，然后重试 schedule-launch，不改基线/状态、不重建资源。

- fresh schedule-launch 重试通过：真实计时回调启动两个已确认归属的沙盒成员，原生进程已观测并按精确内部名清理。当前进入待补验收项 schedule-close，先复核控制程序是否再次出现。

- fresh schedule-close 通过：实际成员 PID 已记录，定时关闭和逐成员进程清理均通过，没有用户进程混入。当前继续 cancel-schedules 的真实待执行任务场景。

- fresh cancel-schedules 通过：生产调度器的两个待执行任务已取消，未执行启动/关闭回调。当前继续 batch-rename，仅操作新前缀所有权中的两个沙盒。

- fresh batch-rename 通过：本轮两个沙盒的别名重命名、分组成员与配置更新均经审计核对。当前先核对精确内部名与绝对目录，再执行 batch-delete；不处理其他历史或用户沙盒。

- fresh batch-delete 通过：先核对绝对目录及基线不存在性，再由既有审计安全逻辑删除两个精确归属资源。当前执行 cleanup 恢复应用配置，然后只读 verify-clean 核对哈希、section、目录和进程。

- fresh cleanup 通过：应用配置恢复原始基线，两项本轮沙盒资源已不存在；进入最终只读 verify-clean，不将 cleanup 返回 0 直接替代全项清理核对。

- fresh verify-clean 通过：目标/应用配置/Sandboxie.ini 哈希均匹配当前通过基线，本轮资源与进程清零。AUDIT-03 的真实场景验证已补齐；正在用独立输出目录完成当前源码的收口门禁及差异检查，独立提交/推送后再进入 AUDIT-04。

- 收口门禁进度：当前源码的受影响回归通过，进入完整 C# 测试。前轮删除分组证据的网络推送已恢复成功；旧失败记录保留，当前不再将网络作为阻断。

- 收口完整 C# 测试通过，当前进入 Python 对照、生产源码编译与独立 Release x64；实际计数及命令以项目结构追加记录为准。

- Python 原版对照通过；继续 compileall，未改变 Python 原版需求或实现。

- 生产 Python 源码及测试 compileall 通过；当前执行独立输出目录的 Release x64 构建，不覆盖真实审计所绑定的原二进制和基线。

- 收口 Release x64 与全量后环境复核通过：独立目录构建无警告/错误，最终 verify-clean 再次全项通过。当前审查证据清单、差异和当前计划，随后独立提交 AUDIT-03；整体软件仍未完成。

- 收口审查：已逐项核对本节验收项和原始报告；本轮未修改生产代码，既有完整门禁继续有效。当前只完成文档、证据及独立提交/推送；遵照用户停止边界，不启动 AUDIT-04。

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

## 更新子系统：已完成

本节于 2026-10-21 完成实现和验证；实时测试数字、GitCode 提交、包哈希和日志路径以 `PROJECT_STRUCTURE.md` 顶部摘要及最新验证记录为准。

### UPDATE-01: GitCode 更新源

- [x] 确定公开仓库 `childhood123/shahe-bulk-launcher-releases`、`main` 分支、正式清单 URL 和完整包发布约定。
- [x] 将旧默认源 `https://lqh.viyf.net/shahe-bulk-launcher/update.json` 迁移到 GitCode，并保留自定义 HTTPS 源。
- [x] 验证禁缓存请求、HTTPS、size/SHA-256、超时、错误响应和旧默认源回退；签名/解密继续由已有更新契约保护。

### GAP-01: 更新结果 UI 和下载链

- [x] RED/GREEN 覆盖最新版、清单错误、无适用包、普通更新稍后下载和强制更新拒绝退出。
- [x] 接线 `UpdateNoticePanel`、`UpdateNoticeLabel`、`DownloadUpdateButton` 和 `DownloadUpdateCallback`，保持 AccessibleName 与键盘焦点。
- [x] 接线 updater 命令准备、进程启动、主程序关闭和错误反馈。

### GAP-02: Updater 安装入口

- [x] 用户确认恢复更新工作，并批准在原 updater 源码缺失时按现有请求协议重建生产入口。
- [x] RED/GREEN 覆盖等待主 PID、下载、大小/哈希/签名、解密、安全解压、备份、替换、回滚和重启。
- [x] 仅在隔离发布副本执行端到端更新，未覆盖仓库工作目录或用户正式安装目录。
- [x] 完成 GitCode 匿名清单/包验证及生产 helper 布局 `restart=true` 真实进程验证。

边界：公开清单与应用 `AppVersion` 当前同为 `4.0.46`，因此这是可用发布基线，同版本客户端不会触发升级提示；下一次真实升级发布必须提高版本号并重新生成包、清单和校验证据。

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

- 当前按用户要求在 AUDIT-03 交付后暂停；用户明确说“继续工作”时，先核对本项提交/推送记录，完成后从 AUDIT-04 继续；
  VOICE-03 已按用户单读屏范围完成，历史复选框滞后时以项目结构最新有效证据为准。
- 如果模块已有 RED 没有 GREEN，从修复阶段继续。
- 如果代码和验证完成但未提交，先做计划核对、暂存审计、提交和推送。
- 如果真实探针为 inconclusive，保留证据并停在该模块，不写成产品通过或失败。
- 历史计划只用于追溯，不覆盖本计划和 `PROJECT_STRUCTURE.md` 顶部摘要。
- UPDATE-01、GAP-01、GAP-02 已完成；后续新版本发布不改变当前模块顺序。

## 当前阻断与风险

- GAP-09 的 C# 最大值 `999` 是用户明确指定的产品要求，与 Python 原版最大值 `20` 有意不同；后续对照不得再把 C# 回退到 `20`。
- 原 updater 源码未提供；当前实现按用户批准的现有请求协议重建，后续取得原源码时需另做差异审计，不回写本轮结论。
- GitCode 清单与应用当前同为 `4.0.46`，只构成发布基线；未来升级发布必须提高版本号。
- `restart=true` 已证明精确隔离路径进程被启动，但该场景未取得主窗口句柄，发布目录 UI 启动仍由 AUDIT-07 验证。
- SbieCtrl 运行会阻断 schedule-close 的安全执行。
- 真实覆盖还原需要专用 VM/测试机。
- Python 3.14 环境缺少 `cryptography`，Python 加密真实执行只能记录环境阻断。
- VOICE-02 的厂商配置读取路径和新版保益配置映射尚无真实证据。
- 工作树含用户已有改动，所有提交必须选择性暂存。
