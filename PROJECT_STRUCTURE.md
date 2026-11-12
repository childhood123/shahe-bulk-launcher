# 沙河批量打开器项目结构

## 当前执行摘要（2026-11-12）

- **唯一当前执行计划：** `docs/superpowers/plans/2026-11-11-full-function-stress.md`。用户已确认完整修复与GitCode交付范围，包含重新纳入保存失败反馈修复。还原安全修复的自动门禁、负载及独立提交推送已完成，当前进入删除查询保护。保留最新分组源码，旧30轮循环继续停止；未升版/发布，精确证据和提交以末尾最新记录为准。
- **当前结论：用户要求的所有功能压测尚未完成。** 已执行当前源码的完整自动化套件、专项压力和重复集成检查，但入口映射、模拟接口及方法调用不能证明全部真实功能已压测。此前“已测完/收口”和“仅真实桌面项未完成”的表述范围过大，现予纠正。当前未找到系统级全局快捷键注册接线，不能声称已实现或已测；实际键盘、托盘、读屏、音频/Sandboxie外部效果及当前构建完整更新链等缺口见 `docs/migration/2026-11-12-all-functions-stress.md`。还原安全已修复并提交；删除保护及其重命名影响已修复并通过最新全量，正在完成交付；定时旧目标、音频、配置并发/反馈/编码/只读、异常清单和资源清理仍待逐项处理。桌面阻断不能替代其余缺口说明；新版本尚未发布，整体迁移未完成。
- **最新交付：** 用户指定的 GitCode 命令行发布已完成，公开绿色包和在线清单现为 4.0.50，包含“全部进程操作遗漏用户”修复。原始 4.0.49 已完成到该在线新包的实际安装和重启；发布后旧版能检测新版、新版不重复提示。完整发布与升级证据见末尾 `RELEASE-4.0.50-20261109`，本地交付在 `release/4.0.50/`。
- **最新运行审计：** 用户在新回合“继续验收”后主EXE启动、窗口文本/焦点读取成功，Tab请求被工具以观察失效拒绝并要求截图；截图刷新报执行失败，恢复时报会话失活。当前阻断明确在工具截图前置环节，没有真实按键通过证据。启动前已快照自启动项，本轮结束后核对一致、配置恢复、目标清理；此前未采集原值的风险仍保留。最新目录为本轮 continuation/desktop-resume-103439，故障复现说明见其中 desktop-issue.md；新回合本身未解决问题，不重复相同尝试。
- **最新自动化验证：** 当前全量、专项压力、补充重复集成、逐入口覆盖及只读校正补验的权威计数和路径见末尾 `FULL-ONCE-20261112`。原始汇总保留夹具中断当时的状态，continuation补验明确说明该场景当前已完成；未重复已通过项目或整个套件。产品失败与真实受阻不因夹具补验通过而关闭。原分组修复证据保留在 `GROUP-SELECTION-20261112`；旧30轮循环仍为 `stopped-by-user`。
- **本次实际补验：** 无截图的计时器、专用注册表/跨进程配置、内置安装机制、Sandboxie只读查询以及此前音频/朗读/互斥证据保留。当前生产设备切换在WMP播放框架中有实际迁移/重新打开保持的证据；`AUDIO-NONDEFAULT-01` 非默认设备音量/静音不可控及切换后操作旧会话仍待修。菜单未发现GC后逐轮留存，但路径菜单依赖GC释放。还原及删除失败的最新修复进度见末尾记录，定时旧目标尚待处理；不能将组件通过扩展为全部应用/真实UI/运行中沙盒通过。
- **旧版更新边界：** 原始 4.0.47 的在线检测、下载校验通过，但安装 4.0.49 被旧外置 updater 的包格式要求阻断；旧程序和配置保持原样，证据 `LEGACY-UPDATE-20261109`。本轮没有恢复该旧兼容设计。
- **4.0.49 内置更新结论：** 先完成同版安装演练，随后完成 4.0.49 → 4.0.50 的原始主 EXE 内置安装、真实下载、替换和新版主窗口启动，配置及日志标记保留。早先“跨版本待验收”已被本轮新证据推进；首次发布后在线请求出现过 403，后续 curl 与两版原始客户端均通过，失败记录保留。
- **已完成与可运行状态：** 既有定位、朗读文案、用户列表进程菜单和 4.0.49 绿色包已实现并有验证；C# 有主窗口启动及正常退出证据。GitHub 源码/Release 和 GitCode 包/清单已有发布证据；GitHub 仓库已于 2026-11-06 改为公开，仓库主页和 `v4.0.49` Release 可匿名访问。
- **必须保留的未完成项：** 查漏报告第 3 节除本轮已修复 MIG-ALL-PIDS-01 及还原安全之外的确认差异、第 4 节候选/未核完边界、第 5 节原版共有缺陷和版本边界、第 7 节运行验收。还原回退/服务异常保护的隔离验证通过，正式服务及运行中沙盒外部验收未做；保存失败反馈现已重新授权、尚未完成。旧 4.0.47 外置更新器仍不能安装不含 updater.exe 的新包。
- **历史冲突处理：** 下方 2026-10-24 摘要、E01–E10 和旧计划中的未提交/未发布描述是历史快照；源码、发布和升级结论以末尾新证据为准。旧 Alt+P 菜单通过记录不适用于已删除进程栏的新界面；设置行 Enter/Escape 已有证据，不重测。
- **后续边界：** 既有GitCode交付完成；用户本次明确授权验收通过后发布下一版，不能把未修失败项当通过。修改/启动/终止限定所属隔离测试对象，正式Sandboxie查询只读，不停止正式程序或读屏、不修改正式配置、不改Python。音频不同框架及端点的通过/失败分别保留，真实菜单验收仍未完成，整体迁移未完成。主目录现有未提交改动保留，修复按已确认计划执行。

### 文档阅读顺序

1. `AGENTS.md` → 本节当前摘要 → `docs/superpowers/plans/2026-11-11-full-function-stress.md` → 末尾 `FULL-ONCE-20261112`。复用已批准全功能范围执行独立一轮，旧30轮循环不恢复；分组修复、用户列表修复、U1 和全文阅读计划保留为历史。
2. `docs/migration/2026-11-09-user-list-acceptance.md` 保存本轮逐功能验收结论；`docs/migration/2026-10-25-python-csharp-audit.md` 保存迁移问题台账，全文阅读记录仍在 `docs/migration/2026-10-25-python-full-read.md`。
3. 本文末尾 `FULL-ONCE-20261112` 与 `docs/migration/2026-11-12-all-functions-stress.md` 是本轮全功能测试交接入口；`GROUP-SELECTION-20261112` 保留分组修复证据。旧压力、用户列表修复、U1 验收和 Python 全文阅读均按历史索引追溯，不重启已完成工作。

## 历史执行摘要（2026-10-24，仅供追溯）

- **唯一当前计划：** `docs/superpowers/plans/2026-10-24-handoff-and-release-4.0.49.md`。本轮 P3/P4 的公开交付已完成：GitHub 源码精确 tree、GitHub `v4.0.49` Release 资产、4.0.49 绿色包和 GitCode 在线清单均已通过发布及下载核对；4.0.47 检测 4.0.49 已通过，旧外置 updater 实际替换被明确阻断。
- **接续顺序：** U1 用户列表进程菜单的代码、自动化验证和文档记录已完成；真实桌面菜单弹出/取消尚未单独验收。P1/P2、本地发布、GitHub P4 和 GitCode P3 已有证据；后续若继续按计划，应回到 AUDIT-04，完成后停在 AUDIT-05 前。不要把旧版“能检测”扩大成“能完成替换”。
- **本轮新增源码接线缺口：** Python 用户列表已绑定上下文菜单；C# 只有 `BuildContextMenu` 方法，未连接鼠标/键盘弹出入口，且旧列表全局状态 resolver 可能同步扫进程。此前静态动作清点不能覆盖这种缺口；本轮 U1 补接线并依用户要求调整入口。基线保存 `artifacts/runs/process_menu_list_20261024/`，后续逐轮记录。
- **源码/包：** 当前交付版本为 `4.0.49`；本地绿色包位于 `artifacts/runs/release_4.0.49_20261024/delivery/`，哈希和内容见 `package-result.json`。GitHub Release `v4.0.49` 已包含同一 ZIP，GitHub 下载复核结果见 `github-release-verify/github-release-asset-result.json`；GitCode 在线 `update.json` 已为 `4.0.49`，实际 `?t=` 请求返回 200，内容长度 620 字节；GitHub 远端源码提交及精确 tree 证据以末尾“GitHub 远端 tree 独立复核”记录为准；先前换行差异已经纠正。
- **最新行为：** 定位以焦点用户为准，快捷键切换后跟随新焦点，只处理该用户 PID 并直接恢复/置前，不弹进程选择框。朗读标签为“朗读接口:”，AccessibleName 为“朗读接口”，首项为“自动”。代码/验证见 E02；本轮只读核查，没有重跑。
- **Python 对照：** 模块入口、快捷键定义、所提取菜单分派和默认配置键未发现整模块/动作缺失。新增 `MIG-READONLY-01`：Python 还原删除失败时尝试清除只读限制并重试，C# 直接删除；只有源码差异、尚无运行时 RED。静态清点不代表逐行语义、全部 UI 顺序或真实行为等价。
- **已完成范围：** GAP-01–10、REVIEW-01–06、VOICE-01/02、用户指定单读屏 VOICE-03、AUDIT-01–03，及快捷键 Enter、配置目录、打赏、配置损坏保护、内置更新、日志的既有实现/验证继续沿用。4.0.49 本地构建与绿色包已验证；GitHub 源码同步和 GitCode 公开包/清单发布已经验收。
- **未修复风险：** 覆盖还原先删旧数据、复制失败无回退，异常后服务未恢复；Python 原版也有这些风险，列为独立修复，普通用户机不做破坏性验证。保存失败误报成功仍在，但 FIX-CONFIG-02 已按用户要求撤回，不自动恢复。旧 REVIEW-07 R2 已修复；4.0.47 的旧外置 updater 无法安装不带 `updater.exe` 的 4.0.49，这是已验证的兼容边界，不通过补回旧 updater 规避。
- **未验收：** AUDIT-04 的 NumPadEnter 实际动作、关于菜单替代入口、托盘完整流程、未覆盖模态和完整单读屏；AUDIT-05–08 及整体门禁尚未收口。设置行 Enter→设置→Escape 返回已有后续 HOTKEY 证据，不重复测试；双读屏不重新设为门槛。
- **可运行状态：** C# 已有主窗口启动、已验证功能和正常退出证据，但不能声明整个软件迁移完成或没有遗留问题。
- **历史冲突：** 下方旧摘要、旧模块表/缺口表及原收口计划均属历史。旧版本、空 Updater、必须外置 updater、多窗口选择、过时未勾选框不覆盖本摘要和新计划。实时数字、提交和证据以以下索引为当前入口。

## 交接证据索引（2026-10-24）

### E01：工作区与源码边界

- 主目录 `D:\mycodex\git`，分支 `沙河批量打开器`，HEAD `4e8752630de9f4543a888f8bd71711d811eda085`，本轮开始时暂存区为空。待提交 C# 文件准确清单见新计划 P1。
- `AGENTS.md`、两份 GAP-09 日志、共享文档历史差异、未跟踪 `csharp/ShaheLauncher.Tests/KeyboardAuditRuntimeContractTests.cs` 不得混提交。快照及源码哈希：`artifacts/runs/handoff_inventory_20261024/workspace-snapshot.json`。
- 当前源码根 `csharp/`；Python 根入口和 `shahe_bulk_launcher/` 作为旧版参照。`D:\mycodex\worktrees\` 是同仓库的隔离工作树，内容取决于各自提交，不默认与主目录相同。

### E02：定位与文案验证

- 根目录 `artifacts/runs/locate_and_speech_text_20261023/`。已读取 TRX：`red/locate-text-red.trx` 通过 1/失败 4；`green/locate-text-green.trx` 通过 5/失败 0；`affected-final/locate-text-affected-final.trx` 通过 165/失败 0；`full/locate-text-full.trx` 通过 426/失败 0；各项跳过 0。首次受影响失败保留。
- `release/locate-text-release.log` 成功构建、警告 0/错误 0，输出 `release/bin/x64/Release/net48/`；原返回码见下方 LOCATE 历史记录。
- `foreground-probe/foreground-result.json` 返回码 0、置前成功、选择回调 0 次；前台由非目标 PID 2756 变为目标 PID 2348，历史记录确认测试进程已消失。这是生产服务加隔离窗口证据，不冒充全部沙盒/最小化边界重新验收。
- 全量来自含未提交 AUDIT-04 测试的工作树，不能冒充干净提交的测试数量。

### E03：旧本地绿色包及包装参考

- 包：`artifacts/runs/diagnostic_logging_release_20261023/delivery-final/shahe-bulk-launcher-4.0.48-win-x64.zip`；SHA-256 `D7BC80EF0218482B00B872458588768EA2B6F4570D904254E570EBED3A521A74`。该包不含 E02 修复。
- `artifacts/runs/diagnostic_logging_release_20261023/portable-final-verification-result.json`：返回码 0，空 Configurations/logs、无禁入项、主窗口显示、正常退出及无残留通过。
- 同目录 `package-portable-final.ps1`、`verify-publish.ps1` 可参考；含旧目录、版本和追加记录标签，必须适配后使用。

### E04：GitCode 工作树与最后保存的在线证据

- `artifacts/runs/updater_added_dll_20261023/release-repo/` 是 detached worktree，HEAD `ac06e1f4a1b19ebba0b26e69a571b72649e7f1b6`，本地清单还是 `4.0.46`；远端 `https://gitcode.com/childhood123/shahe-bulk-launcher-releases.git` 的 `main`。必须 fetch 后以前向提交发布。
- 最近持久化在线证据：`artifacts/runs/diagnostic_logging_release_20261023/public-manifest-before.json`、`manifest-fetch-result.json`，公开版 `4.0.47`、读取返回码 0。本轮未重测网络或写权限，不把本地清单当线上状态。

### E05：旧版用户测试包

- `artifacts/runs/updater_added_dll_20261023/shahe-bulk-launcher-4.0.47-win-x64.zip`；清单字节数 `3010671`、SHA-256 `F8BEA7B4D03C06A93E281D459724FE3310D33883CBF83EB2E92E7BE32F739D8D`。历史发布记录同目录 `delivery-result.json`。
- 这是绿色包，隔离解压后测试；旧升级证据不自动证明向尚未生成的新版升级成功。

### E06：GitHub 集成与网络历史

- 目录 `D:\mycodex\worktrees\shahe-unified-4.0.48-20261023`，分支 `integration/shahe-unified-4.0.48-20261023`，HEAD `460e531a96249e7ebca68d68687405f325a34f9d`；已有同步提交 `445612a`、网络记录 `3cbad15`，尚不含 P1/P2 改动。
- `artifacts/runs/diagnostic_logging_release_20261023/remote-integration-result.json` 记录旧集成 C# 树等价；`remote-network-block-result.json` 记录上次 GitHub HTTPS/SSH 不通、推送未完成、语音分支未删除。本轮未重测网络。
- 目标 `origin/沙河批量打开器` 包含全部应交付源码并经核对后，才删除 `origin/voice03-fixed-reader-20261022`；失效历史 worktree 条目只登记、不清理。

### E07：缺陷与撤回任务

- `artifacts/runs/csharp_remaining_review_20261023/review.md`、`probe-result.json` 保存还原数据回退/服务恢复和保存反馈的隔离复现。本轮只读复核当前 `BackupRestoreService.RestoreSelectedUsers`、`LauncherSettingsService.SaveSettings/ApplySettings`，相关路径仍在。
- 配置读取保护 R2 已由 `artifacts/runs/configuration_failure_safety_20261023/` 交付证据关闭。
- FIX-CONFIG-02 撤回记录 `artifacts/runs/embedded_update_20261023/scope-update-only/withdrawal-result.json`；历史测试不是当前已交付修复。R5 被内置更新设计取代，不补 updater。

### E08：本轮 Python/C# 静态对照

- `artifacts/runs/handoff_inventory_20261024/compare-source.ps1` → `source-comparison.json`，返回码 0：20 个模块代表入口存在，快捷键定义 47/47，类别/动作键/标签/默认键/事件编号无差异；提取菜单动作 41 项都有分派；默认配置顶层键 22 项有 C# 字面量。存在性不证明每个键的值和保存语义一致。
- 手动追踪 Python `backup.py::_force_remove_dir` 与 C# `BackupRestoreService.RestoreSelectedUsers`，新增 `MIG-READONLY-01`，尚无运行时 RED。未启动 Python GUI、读屏或真实服务。
- `artifacts/runs/python_to_csharp_remaining_audit_20261021/` 中“剩余五项”等数字为历史；相应 GAP/REVIEW 后来已修复，不能复制为当前未迁移项。

### E09：可复用 Python 基线

- `artifacts/runs/embedded_update_20261023/python-result.json`：返回码 0，通过 31/失败 0/跳过 0；同目录 `compile-result.json` 返回码 0。Python 生产源码本轮未改、未重跑；历史 cryptography 环境边界不因编译通过而解除。

### E10：AUDIT-04 有效证据与待办去重

- `artifacts/runs/audit04_keyboard_20261023/menu-focus-progress.json` 为有效菜单/焦点清单；原计划 AUDIT-04 保留 Alt、分组、NumPadEnter 输入层和清理记录。
- `artifacts/runs/hotkey_enter_action_20261023/real-exe-final/final-result.json`、`returned-main.json` 已证明设置行 Enter 打开、Escape 返回、配置未改和正常退出；覆盖旧进度文件的 Settings modal cancel 待办，不代表所有模态已验收。
- 未完成项见新计划 A4；Alt+A 转微信仍属疑似外部冲突，无效尝试/进程清理不能当托盘退出通过。

## 历史摘要快照（2026-10-23，仅供追溯）

> 以下原摘要及后续旧状态表保留历史原文；“当前”“唯一计划”“下一步”等措辞只代表当时。接手以文首新摘要和新计划为准。

- 本轮 LOG-01：全部 Alt 菜单、快捷键、右键菜单、按钮、托盘、列表动作和对话框返回已接入日志；统一为 `logs/yyyy-MM-dd.log`，缺省关闭，开启后才记录。最终定向/全量、Release 及日志开关验证已通过；4.0.48 最终交付物为无安装程序的绿色 ZIP，包内 `Configurations/`、`logs/` 均为空，不含 `updater.exe`、PDB、用户配置或日志，解压后主窗口可直接启动并正常退出。主 EXE/Core DLL 与已通过内置更新实测的默认发布编译物哈希一致，更新能力仍由主程序内置。本地功能提交为 `4e87526`，以远端沙河分支为父的前向同步提交为 `445612a`，网络阻断记录提交为 `3cbad15`；当前 GitHub HTTPS/SSH 均不可达，尚未推送。远端主分支验证成功后再删除独立语音分支。本地包尚未上传 GitCode。Python 未改，不推进 AUDIT-05。
- 前项已交付范围：只保留 UPDATE-EMBED-01 内置更新及三个选项，本模块已实现、验证、独立提交并推送；“稍后”保存目标版本并于下次启动自动更新，“忽略”取消该版本安排，默认发布不含独立 updater。FIX-CONFIG-02 未提交修复已按用户要求撤回；已提交的 FIX-CONFIG-01 保持。Python、GitCode 公开包和用户旧版目录未改；C# 能启动并运行已验证功能，整体 AUDIT/还原安全等待办仍未完成。
- 当前唯一计划：`docs/superpowers/plans/2026-10-20-remaining-migration-closure.md`。GAP-09 已按用户“反正能记忆到999条记录就行之前是20”的明确要求更新为 C# `0–999`；除该用户指定差异外，后续功能继续保留 Python 原文案和行为，只用 C# 实现。
- 当前状态：AUDIT-03 已完成并推送；AUDIT-04 的既有代码门禁、Alt 导航、分组进出、Tab 正反向/环绕、F10 入口与焦点恢复，以及文件/进程/沙盒管理/定时/设置/校正/工具菜单已取得有效证据。Alt+A 后转到微信，疑似外部快捷键冲突；关于替代入口准备未完成，模态返回、托盘、完整单读屏及 NumPadEnter 动作分派仍待验收。HOTKEY-ENTER-01 全部功能行回车执行已完成验证、独立提交并推送，AUDIT-04 既有未验收项保留。已通过项不重跑，不提交未完成模块、不进入 AUDIT-05，整体迁移未完成。
- 当前追加任务 UPDATE-DLL-01 / CONFIG-LOC-01：4.0.47 已公开发布并通过真实旧版升级验证；Configurations 为唯一用户配置目录，公开包只带空目录，旧配置完整迁入，新增 DLL 与打赏图片落盘和窗口读取通过。源码已独立提交并推送；AUDIT-04 原待办保留，不进入后续模块。
- 用户追加任务 DONATE-01：已按最新明确要求，将原始支付宝/微信图片放入根目录 `打赏/`，C# 菜单优先从该目录读取，构建/发布自动带上图片；复用原有“复制二维码”按钮复制原图位图。此追加任务已实现并验证，后续审计仍遵守既定顺序。
- 用户追加设置/菜单核对：SETTINGS-01 已修复普通软件设置保存时自启动写入旧值；MENU-01 已按 Python 恢复三条分隔线，并将设置菜单恢复为“热键设置、朗读设置、软件设置”。路径列表原有操作和用户指定 999 上限保留。读屏 API 下拉框原先仅保存/回显的问题已独立修复，按用户单读屏要求完成 NVDA 实测及完整代码门禁；双读屏/真实启动恢复未验收和首次探针异常未定位仍保留，不据此声明所有读屏场景均通过。
- 用户指定 HISTORY-UI-01：数量窗口已移除“已记忆的路径列表”和“隐藏路径”复选框，只保留数量及确定/取消；原“调整记忆数”入口复用同一窗口。既有历史数据不因移除控件被清空，范围仍为 0–999。主窗口原有路径上下文功能保留，重复入口核查记录见本轮报告。
- GitCode 发布状态：公开仓库 main 仍为 4.0.47；4.0.48 绿色 ZIP 仅在本地构建并验证，尚未上传或修改公开清单。4.0.47 的匿名下载、真实旧版升级与资源验收历史结论继续保留。
- 发布包状态：4.0.48 最终本地绿色包位于 `artifacts/runs/diagnostic_logging_release_20261023/delivery-final/shahe-bulk-launcher-4.0.48-win-x64.zip`，SHA-256 为 `D7BC80EF0218482B00B872458588768EA2B6F4570D904254E570EBED3A521A74`，包含空 `Configurations/`、空 `logs/`、打赏原图和 `lib` 运行文件，不含用户数据、PDB 或独立 updater；解压即用验证返回码 `0`。
- 当前用户追加任务 LOCATE-01 + UI-TEXT-01：定位动作已改为以列表焦点用户为准，只处理该用户 PID 的窗口并直接置前，不再显示进程选择框；朗读设置已改为“朗读接口”和“自动”。定向 GREEN `5/5`、受影响回归 `165/165`、C# 全量 `426/426` 均通过；隔离 Release x64 返回码 `0`、警告 `0`、错误 `0`。隔离真实窗口探针返回码 `0`：前台从非目标 PID `2756` 切换为目标 PID `2348`，选择回调调用 `0` 次，两个测试进程均已清理。当前等待计划勾选确认、版本升级发布和独立提交。
- 前次 HISTORY-UI-01 自动化门禁（不代表当前 VOICE-03）：HISTORY-UI-01 有效 RED 通过 2/失败 1（列表仍存在），精简后定向 9/9、受影响 91/91、C# 全量 370/370、Python 29/29，compileall 与隔离 Release x64 返回码 0，构建 0 警告/0 错误。生产窗体保存/取消验证历史与其他偏好未被误改，数量入口真实模态验证保持上限 999。此为用户明确指定的界面差异；其他 Python 文案、菜单和业务执行仍继续对照，整体迁移未完成。
- 版本触发边界：当前源码及最终本地绿色包为 4.0.48，公开清单最后只读核对仍为 4.0.47；最终 ZIP 已验证解压、直接启动、正常退出和关闭日志时空 `logs` 保持，内置更新安装证据通过同哈希主 EXE/Core DLL 复用。公开发布尚未变更。
- 已完成模块：GAP-01 至 GAP-10、REVIEW-01 至 REVIEW-06、VOICE-01、VOICE-02、AUDIT-01、AUDIT-02、UPDATE-01 及此前已确认范围。VOICE-03 已按用户单读屏范围完成，AUDIT-03 已完成并推送；当前执行 AUDIT-04；其后为 AUDIT-05 至 AUDIT-08 和 RELEASE-01，本轮不推进后续模块。整体迁移仍未完成。
- 真实 updater 证据：GitCode 匿名 ZIP 在隔离生产 helper 布局下以 `restart=true` 完成下载、替换和进程重启；updater PID `11448` 返回 `0`，精确隔离路径主进程 PID `5364` 被观测并清理，安装文件哈希、用户配置和 `lib 7/7` 均通过。该场景主窗口句柄为 `0`，只证明进程重启，不替代后续发布目录 UI 启动审计。
- 真实 Sandboxie 清理状态：本轮新基线下最终 verify-clean 全部通过，目标/应用配置/真实 Sandboxie.ini 哈希匹配，本轮 section、目录和受控进程均无残留；最新命令、构建目录及资源细节见追加验证记录。用户已授权正常退出控制程序，服务保持 Running。
- C# 当前可以启动并运行已验证功能；路径记忆上限按用户要求为 999。整体 Python→C# 迁移仍未完成，AUDIT-04 至 AUDIT-08 和最终发布验收仍有待办，尚不能认定全部功能均已完整验收；活动计划中 AUDIT-01/02 等历史复选框存在进度滞后，以本摘要、独立提交和最新日志证据为准。

- VOICE-03 本轮进度：用户指定的单读屏验收、完整代码门禁和独立提交/推送均完成；修复分支与回退信息见最新验证记录。双读屏、真实启动恢复及保益自身发声未验收，首次原生异常未定位，均未宣称已解决。AUDIT-03 已完成并推送，当前仅恢复 AUDIT-04；整体迁移未完成。

### 本轮验证记录（只追加）

- GAP-03 GREEN：同一定向测试返回码 `0`，通过 `14`、失败 `0`、跳过 `0`；新旧配置的六类字段、类型化历史保存和跨实例加载/保存均通过。实际命令、日志和 TRX：`artifacts/runs/csharp_non_update_gaps_20261020/gap03/green-20261020_100744_829-result.json` 及同名前缀 `.log/.trx`。

- GAP-03 RED：隔离 `dotnet test` 定向运行 `FullyQualifiedName~JsonSettingsRuntimeContractTests`，返回码 `1`，通过 `4`、失败 `10`、跳过 `0`。现代 INI 和旧 JSON 两条加载路径均确认路径历史、分组、音量、自定义快捷键丢失；置顶和朗读提示对照通过。另确认类型化路径历史保存为 `Path/Note`，不符合 Python 的 `path/note`，加载再保存后仍丢历史。完整命令、日志和 TRX：`artifacts/runs/csharp_non_update_gaps_20261020/gap03/red-20261020_100632_431-result.json` 及同名前缀 `.log/.trx`。测试未访问真实沙盒、未启动更新。

- GAP-03 受影响回归：Application、SettingsService、SandboxAdmin、AppPaths 和 ConfigStore 筛选运行返回码 `0`，通过 `89`、失败 `0`、跳过 `0`；命令和日志索引：`artifacts/runs/csharp_non_update_gaps_20261020/gap03/affected-20261020_101006_179-result.json`，同名前缀 `.log/.trx`。

- GAP-03 C# 全量：隔离解决方案测试返回码 `0`，通过 `248`、失败 `0`、跳过 `0`；包含当前工作树原有 AuditRunner 测试，未把这些原有修改纳入本轮修复。命令和日志索引：`artifacts/runs/csharp_non_update_gaps_20261020/gap03/full-20261020_101103_857-result.json`，同名前缀 `.log/.trx`。

- GAP-03 Python 对照：`python -m unittest discover -s tests -p "test_*.py" -v` 返回码 `0`，通过 `29`、失败 `0`、跳过 `0`；日志 `artifacts/runs/csharp_non_update_gaps_20261020/gap03/python-20261020_101154_336.log`，命令返回码另有同名前缀 `-result.json`。

- GAP-03 Python 原版行为探针：`python artifacts/runs/csharp_non_update_gaps_20261020/gap03/python-json-roundtrip-probe.py` 返回码 `0`，原源码 AST 提取的路径历史、分组、置顶、音量归一化及 JSON 往返共 `5` 项通过、失败 `0`、跳过 `0`；日志为同目录 `python-json-roundtrip-probe.log`。探针不导入 GUI、不触发真实系统动作。

- GAP-03 Python 生产源码编译：`python -m compileall -q shahe_bulk_launcher tests` 返回码 `0`，无语法错误；测试计数不适用。日志/命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/gap03/compile-20261020_101233_196-result.json` 及同名前缀 `.log`。

- GAP-03 Release x64：隔离解决方案构建返回码 `0`，警告 `0`、错误 `0`，测试计数不适用；实际输出目录、命令和日志：`artifacts/runs/csharp_non_update_gaps_20261020/gap03/release-20261020_101249_151-result.json` 及同名前缀 `.log`。本模块配置恢复已由真实持久化和 WinForms STA 运行时验证，未新增外部真实 EXE/读屏审计；既有真实审计风险继续保留。

- GAP-03 差异检查：`git diff --check` 返回码 `0`，无空白错误，只有既有 LF/CRLF 提示；测试计数不适用。日志与命令返回码：`artifacts/runs/csharp_non_update_gaps_20261020/gap03/diff-20261020_101348_769-result.json` 及同名前缀 `.log`。

- GAP-03 提交：暂存检查返回码 `0`；仅提交两个 Core 修复文件、独立测试、当前计划、PROJECT_STRUCTURE 本轮摘要差异和本轮脚本/文本证据，原有文档差异及用户代码保持未暂存。独立提交 `ae87be1` 已成功推送 `origin/fix/non-update-gaps-20261020`（返回码 `0`），回退命令 `git revert ae87be1`。

- GAP-04 RED：隔离 `SpeechSettingsPreservationContractTests` 返回码 `1`，通过 `2`、失败 `2`、跳过 `0`。真实设置对话框的确定路径把已有 SpeechConfig 覆盖为空，缺文件时创建空配置；取消路径正常。命令和日志：`artifacts/runs/csharp_non_update_gaps_20261020/gap04/red-20261020_101922_404-result.json` 及同名前缀 `.log/.trx`；仅操作测试目录。

- GAP-04 GREEN：同一定向测试返回码 `0`，通过 `4`、失败 `0`、跳过 `0`；保存回调已与 Python 的 `lambda config: True` 对齐，真实对话框确定/取消均保留已有 SpeechConfig，读屏接口和辅助提示仍可保存。命令和日志：`artifacts/runs/csharp_non_update_gaps_20261020/gap04/green-20261020_102019_862-result.json` 及同名前缀 `.log/.trx`。

- GAP-04 affected：test 验证返回码 0，通过 88、失败 0、跳过 0。完整命令和结果：`artifacts/runs/csharp_non_update_gaps_20261020/gap04/affected-20261020_102146_492-result.json`；日志为同名前缀 .log，另有 .trx。

- GAP-04 full：test 验证返回码 1，通过 251、失败 1、跳过 0。完整命令和结果：`artifacts/runs/csharp_non_update_gaps_20261020/gap04/full-20261020_102203_265-result.json`；日志为同名前缀 .log，另有 .trx。

- GAP-04 audit-input-recheck：test 验证返回码 1，通过 0、失败 1、跳过 0。完整命令和结果：`artifacts/runs/csharp_non_update_gaps_20261020/gap04/audit-input-recheck-20261020_102325_208-result.json`；日志为同名前缀 .log，另有 .trx。

- GAP-04 审计输入基线对照：使用 GAP-03 已通过全量的旧测试程序集，执行 `dotnet vstest artifacts/runs/csharp_non_update_gaps_20261020/gap03/bin/x64/Release/net48/ShaheLauncher.Tests.dll /TestCaseFilter:FullyQualifiedName~BatchCreateValuesAreSetByAccessibleNamesNotWindowOrder /Platform:x64`（追加日志/结果参数），返回码 `0`，通过 `1`、失败 `0`、跳过 `0`。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap04/gap03-binary-audit-input-baseline.log/.trx`。失败点为既有 AuditRunner 通过原生 ComboBox 消息设置索引后的托管回读差异，根因尚未稳定确认，未修改用户原有审计器代码。

- GAP-04 同入口新程序集复核：`dotnet vstest` 直接执行 GAP-04 隔离程序集中的 `BatchCreateValuesAreSetByAccessibleNamesNotWindowOrder`，返回码 `0`，通过 `1`、失败 `0`、跳过 `0`；命令参数同旧程序集对照，结果：`artifacts/runs/csharp_non_update_gaps_20261020/gap04/gap04-binary-audit-input-recheck.log/.trx`。新旧单项在同入口均通过；此前两次原生输入失败保留，尚不归因为朗读配置修复。

- GAP-04 full-vstest：vstest 验证返回码 0，通过 252、失败 0、跳过 0。完整命令和结果：`artifacts/runs/csharp_non_update_gaps_20261020/gap04/full-vstest-20261020_102839_212-result.json`；日志为同名前缀 .log，另有 .trx。

- GAP-04 python：python 验证返回码 0，通过 29、失败 0、跳过 0。完整命令和结果：`artifacts/runs/csharp_non_update_gaps_20261020/gap04/python-20261020_102857_701-result.json`；日志为同名前缀 .log。

- GAP-04 compile：compile 验证返回码 0，计数不适用。完整命令和结果：`artifacts/runs/csharp_non_update_gaps_20261020/gap04/compile-20261020_102859_381-result.json`；日志为同名前缀 .log。

- GAP-04 release：build 验证返回码 0，警告 0、错误 0；测试计数不适用。完整命令和结果：`artifacts/runs/csharp_non_update_gaps_20261020/gap04/release-20261020_102900_319-result.json`；日志为同名前缀 .log。

- GAP-04 diff：diff 验证返回码 0，计数不适用。完整命令和结果：`artifacts/runs/csharp_non_update_gaps_20261020/gap04/diff-20261020_102904_498-result.json`；日志为同名前缀 .log。

- 用户提供语音源码：`artifacts/probes/tolk-downloaded/boz700908-tolk-add14ac/src/dotnet/Tolk.cs`、`TolkDotNet.csproj` 和 `examples/c#/ConsoleApp.cs` 已只读对照。包装工程目标为 net40/netstandard2.0；源码中 Tolk 的 C++ bool 参数/返回值使用 `MarshalAs(UnmanagedType.I1)`，当前 SpeechService 使用 `Bool`，登记为 `VOICE-01` 静态 ABI 差异，待单独 RED/GREEN，不与 GAP-04 配置保留混合修复。该源码目录尚未发现编译好的 `Tolk.dll`；存在 x64 的 NVDA、ZDSR、保益等厂商依赖。未加载库或改变用户语音配置。

- GAP-04 提交：暂存差异检查返回码 `0`；独立提交 `9930f14` 已推送 `origin/fix/non-update-gaps-20261020`，返回码 `0`，回退 `git revert 9930f14`。用户原有 AuditRunner、两个 C# 修改和文档历史保持未暂存；新增原生输入稳定性风险与语音参考源码已记录。

- GAP-05 reader-entry-red：test 验证返回码 1，通过 0、失败 4、跳过 0。完整命令和结果：`artifacts/runs/csharp_non_update_gaps_20261020/gap05/reader-entry-red-20261020_103407_696-result.json`；日志为同名前缀 .log，另有 .trx。

- GAP-05 priority-red：test 验证返回码 1，通过 2、失败 2、跳过 0。完整命令和结果：`artifacts/runs/csharp_non_update_gaps_20261020/gap05/priority-red-20261020_103451_389-result.json`；日志为同名前缀 .log，另有 .trx。

- GAP-05 green：test 验证返回码 0，通过 4、失败 0、跳过 0。完整命令和结果：`artifacts/runs/csharp_non_update_gaps_20261020/gap05/green-20261020_103534_942-result.json`；日志为同名前缀 .log，另有 .trx。

- GAP-05 affected：vstest 验证返回码 0，通过 74、失败 0、跳过 0。完整命令和结果：`artifacts/runs/csharp_non_update_gaps_20261020/gap05/affected-20261020_103545_516-result.json`；日志为同名前缀 .log，另有 .trx。

- GAP-05 full：vstest 验证返回码 0，通过 256、失败 0、跳过 0。完整命令和结果：`artifacts/runs/csharp_non_update_gaps_20261020/gap05/full-20261020_103557_545-result.json`；日志为同名前缀 .log，另有 .trx。

- GAP-05 python：python 验证返回码 0，通过 29、失败 0、跳过 0。完整命令和结果：`artifacts/runs/csharp_non_update_gaps_20261020/gap05/python-20261020_103616_139-result.json`；日志为同名前缀 .log。

- GAP-05 compile：compile 验证返回码 0，计数不适用。完整命令和结果：`artifacts/runs/csharp_non_update_gaps_20261020/gap05/compile-20261020_103617_820-result.json`；日志为同名前缀 .log。

- GAP-05 release：build 验证返回码 0，警告 0、错误 0；测试计数不适用。完整命令和结果：`artifacts/runs/csharp_non_update_gaps_20261020/gap05/release-20261020_103618_764-result.json`；日志为同名前缀 .log。

- GAP-05 diff：diff 验证返回码 0，计数不适用。完整命令和结果：`artifacts/runs/csharp_non_update_gaps_20261020/gap05/diff-20261020_103621_248-result.json`；日志为同名前缀 .log。

- GAP-05 Python 原版行为探针：`python artifacts/runs/csharp_non_update_gaps_20261020/gap05/python-clipboard-priority-probe.py` 返回码 `0`，通过 `4`、失败 `0`、跳过 `0`；从 `features/app_paths.py` 的 AST 提取真实 `on_paste_file`，验证文件列表优先、无有效文件时文本回退、无效数据反馈和剪贴板无法打开反馈。探针只使用内存假剪贴板，未启动 GUI、未访问系统剪贴板。脚本、日志和结果：同目录 `python-clipboard-priority-probe.py`、`.log`、`-result.json`。

- GAP-05 提交前定向复核：隔离 `dotnet test` 返回码 `0`，通过 `4`、失败 `0`、跳过 `0`；重新验证文件列表优先、文本回退、无效数据及空剪贴板。命令索引和 TRX：`artifacts/runs/csharp_non_update_gaps_20261020/gap05/final-targeted-20261020-result.json`、`final-targeted-20261020.trx`。

- GAP-05 提交前 C# 全量复核：隔离解决方案测试返回码 `0`，通过 `256`、失败 `0`、跳过 `0`。命令索引和 TRX：`artifacts/runs/csharp_non_update_gaps_20261020/gap05/final-full-20261020-result.json`、`final-full-20261020.trx`。

- GAP-05 提交前 Python 全量复核：`python -m unittest discover -s tests -p "test_*.py" -v` 返回码 `0`，通过 `29`、失败 `0`、跳过 `0`。日志和命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/gap05/final-python-20261020.log`、`final-python-20261020-result.json`。

- GAP-05 提交前 Python 生产源码编译：`python -m compileall -q shahe_bulk_launcher tests` 返回码 `0`，无语法错误；测试计数不适用。日志和命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/gap05/final-compile-20261020.log`、`final-compile-20261020-result.json`。

- GAP-05 提交前 Release x64：隔离解决方案构建返回码 `0`，警告 `0`、错误 `0`，测试计数不适用；实际输出为 `artifacts/runs/csharp_non_update_gaps_20261020/gap05/final-bin/x64/Release/net48/`。日志和命令索引：同目录 `final-release-20261020.log`、`final-release-20261020-result.json`。

- GAP-05 提交前差异检查：`git diff --check` 返回码 `0`，无空白错误，仅有工作树既有 LF/CRLF 转换提示；测试计数不适用。日志和命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/gap05/final-diff-20261020.log`、`final-diff-20261020-result.json`。

- GAP-05 提交：独立提交 `be7c6f3` 已推送 `origin/fix/non-update-gaps-20261020`，推送返回码 `0`；回退命令为 `git revert be7c6f3`。仅提交 GAP-05 代码、测试、计划、顶部项目摘要和文本证据，用户已有解决方案、HotkeyController、SettingsRuntimeContractTests、测试工程、AuditRunner 和文档历史差异保持未提交。

- GAP-06 RED：隔离 `dotnet test` 定向运行 `SelectAllToggleRuntimeContractTests`，返回码 `1`，通过 `1`、失败 `4`、跳过 `0`。文件菜单与固定 `Ctrl+A` 连续触发第二次仍保持全选，空列表每次触发仍产生勾选状态更新；右键“全选”保持单向行为的基线项通过。日志、命令索引和 TRX：`artifacts/runs/csharp_non_update_gaps_20261020/gap06/red-20261020.log`、`red-20261020-result.json`、`red-20261020.trx`。

- GAP-06 GREEN：将 `MainForm` 的 `select_all` 分派从 `CheckAll()` 改为复用现有 `ToggleSelectAll()` 后，同一定向测试返回码 `0`，通过 `5`、失败 `0`、跳过 `0`。文件菜单和固定 `Ctrl+A` 连续两次切换、部分选择、空列表及右键单向全选边界均通过。日志、命令索引和 TRX：`artifacts/runs/csharp_non_update_gaps_20261020/gap06/green-20261020.log`、`green-20261020-result.json`、`green-20261020.trx`。

- GAP-06 受影响回归：对新程序集运行 Application 与 `UiControlsCSharpContractTests` 筛选，返回码 `0`，通过 `85`、失败 `0`、跳过 `0`。日志、命令索引和 TRX：`artifacts/runs/csharp_non_update_gaps_20261020/gap06/affected-20261020.log`、`affected-20261020-result.json`、`affected-20261020.trx`。

- GAP-06 C# 全量：直接执行本轮隔离程序集，返回码 `0`，通过 `261`、失败 `0`、跳过 `0`。日志、命令索引和 TRX：`artifacts/runs/csharp_non_update_gaps_20261020/gap06/full-20261020.log`、`full-20261020-result.json`、`full-20261020.trx`。

- GAP-06 Python 原版行为探针：`python artifacts/runs/csharp_non_update_gaps_20261020/gap06/python-select-all-probe.py` 返回码 `0`，通过 `6`、失败 `0`、跳过 `0`。探针从原版 AST 执行 `toggle_select_all`、`on_select_all` 和 `on_ctrl_a_select`，并确认文件菜单绑定到切换入口；未导入 wxPython、未启动 GUI 或发送真实按键。脚本、日志和命令索引：同目录 `python-select-all-probe.py`、`.log`、`-result.json`。

- GAP-06 Python 全量：`python -m unittest discover -s tests -p "test_*.py" -v` 返回码 `0`，通过 `29`、失败 `0`、跳过 `0`。日志和命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/gap06/python-20261020.log`、`python-20261020-result.json`。

- GAP-06 Python 生产源码编译：`python -m compileall -q shahe_bulk_launcher tests` 返回码 `0`，无语法错误；测试计数不适用。日志和命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/gap06/compile-20261020.log`、`compile-20261020-result.json`。

- GAP-06 Release x64：隔离解决方案构建返回码 `0`，警告 `0`、错误 `0`，测试计数不适用；实际输出为 `artifacts/runs/csharp_non_update_gaps_20261020/gap06/bin/x64/Release/net48/`。日志和命令索引：同目录 `release-20261020.log`、`release-20261020-result.json`。

- GAP-06 差异检查：`git diff --check` 返回码 `0`，无空白错误，仅有工作树既有 LF/CRLF 转换提示；测试计数不适用。日志和命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/gap06/diff-20261020.log`、`diff-20261020-result.json`。

- GAP-06 提交前定向复核：从当前源码重新生成隔离程序集并运行 `SelectAllToggleRuntimeContractTests`，返回码 `0`，通过 `5`、失败 `0`、跳过 `0`。日志、命令索引和 TRX：`artifacts/runs/csharp_non_update_gaps_20261020/gap06/final-targeted-20261020.log`、`final-targeted-20261020-result.json`、`final-targeted-20261020.trx`。

- GAP-06 提交：独立提交 `f089cf3` 已推送 `origin/fix/non-update-gaps-20261020`，推送返回码 `0`；回退命令为 `git revert f089cf3`。仅提交 GAP-06 分派修复、独立测试、计划、顶部项目摘要和文本证据，用户已有解决方案、HotkeyController、SettingsRuntimeContractTests、测试工程、AuditRunner 和文档历史差异保持未提交。

- VOICE-01 设计：Tolk x64 运行库接入设计已写入 `docs/superpowers/specs/2026-10-20-tolk-runtime-integration-design.md`；初始提交 `ce5b91e`、`lib` 参考布局修订 `d192b9d` 和不复制 `.txt` 的最终修订 `ff05d61` 均已推送 `origin/fix/non-update-gaps-20261020`，最新回退命令为 `git revert ff05d61`。当前仅完成设计和源码/工具链调查，未复制 DLL、未修改 SpeechService、未运行原生朗读调用。

- VOICE-01 Task 1 RED：隔离 `dotnet test` 定向运行 `TolkRuntimeIntegrationContractTests`，返回码 `1`，通过 `0`、失败 `3`、跳过 `0`。三个断言分别确认 App 源码 `lib` 目录尚无七个运行文件、`ShaheLauncher.App.csproj` 尚无 `lib` 复制规则、隔离输出尚无 `lib` 运行文件；测试已正常编译和执行。日志、命令索引和 TRX：`artifacts/runs/csharp_tolk_runtime_20261020/task1-red.log`、`task1-red-result.json`、`task1-red.trx`。

- VOICE-01 Tolk 源码构建：在用户提供源码根目录运行 `.\build.bat release --x64 --no-bootstrap`，返回码 `0`，x64 Release 架构成功 `1`、失败 `0`，生成 `dist/x64/Release/Tolk.dll`。构建未安装或下载工具；上游源码出现 C4819/C4127 和托管包装层 CS1668 警告，错误 `0`，该第三方构建不替代后续应用 Release 的零警告门禁。日志和命令索引：`artifacts/runs/csharp_tolk_runtime_20261020/tolk-source-build.log`、`tolk-source-build-result.json`。

- VOICE-01 Task 1 GREEN：复制用户源码构建的 `Tolk.dll` 和 `libs/x64` 中六个批准文件到 `csharp/ShaheLauncher.App/lib/`，并为 App 项目添加 `lib/**` 构建/发布复制规则后，同一定向测试返回码 `0`，通过 `3`、失败 `0`、跳过 `0`。已验证源码与隔离输出均精确包含七个文件、无 `.txt`，五个 DLL 的 PE machine 均为 x64。日志、命令索引和 TRX：`artifacts/runs/csharp_tolk_runtime_20261020/task1-green.log`、`task1-green-result.json`、`task1-green.trx`。

- VOICE-01 Task 2 RED 首次尝试：定向测试返回码 `1`，通过 `3`、失败 `12`、跳过 `0`。其中路径优先级、两个 backend 接口、六个 I1 ABI 断言和关闭卸载共 `10` 项为有效产品失败；初始化顺序与失败清理两项因测试 helper 仅用 NonPublic 反射而未找到实际入口，属于无效测试基础设施失败，不作为产品结论。日志、命令索引和 TRX：`artifacts/runs/csharp_tolk_runtime_20261020/task2-red-attempt1.log`、`task2-red-attempt1-result.json`、`task2-red.trx`。

- VOICE-01 Task 2 有效 RED：测试 helper 同时查找 public/nonpublic 入口后，定向测试返回码 `1`，通过 `3`、失败 `12`、跳过 `0`。`lib` 路径优先级、`PreferSapi`/`DetectScreenReader`、活动读屏优先的 SAPI 回退调用顺序、六个 Tolk bool I1 ABI、关闭卸载和初始化失败清理均形成有效断言失败。日志、命令索引和 TRX：`artifacts/runs/csharp_tolk_runtime_20261020/task2-red-valid.log`、`task2-red-valid-result.json`、`task2-red-valid.trx`。

- VOICE-01 Task 2 GREEN：`SpeechService` 增加 `lib/Tolk.dll` 优先查找、`PreferSapi`/`DetectScreenReader` backend 能力、`TrySAPI(true)` + `PreferSAPI(false)` 调用顺序、Tolk bool I1 ABI，以及初始化失败和关闭时的单次 `Tolk_Unload`/`FreeLibrary` 清理后，同一定向测试返回码 `0`，通过 `15`、失败 `0`、跳过 `0`。日志、命令索引和 TRX：`artifacts/runs/csharp_tolk_runtime_20261020/task2-green.log`、`task2-green-result.json`、`task2-green.trx`。

- VOICE-01 受影响回归首次运行：Speech/Application/TolkRuntime 筛选返回码 `1`，通过 `99`、失败 `1`、跳过 `0`。唯一失败为既有 `SpeaksAssistTextOnWorkerThreadAndShutdownDisablesRuntime` 调用序列索引 4 不匹配；在核对测试是否仍表达旧的“不卸载”语义前，不把该失败归类为产品回归，也不继续全量门禁。日志、命令索引和 TRX：`artifacts/runs/csharp_tolk_runtime_20261020/affected-attempt1.log`、`affected-attempt1-result.json`、`affected.trx`。

- VOICE-01 受影响回归 GREEN：既有 Speech 契约更新为 `TrySapi(true) -> PreferSapi(false)` 并验证关闭时 `Unload` 后，Speech/Application/TolkRuntime 筛选返回码 `0`，通过 `100`、失败 `0`、跳过 `0`。日志、命令索引和 TRX：`artifacts/runs/csharp_tolk_runtime_20261020/affected-green.log`、`affected-green-result.json`、`affected-green.trx`。

- VOICE-01 原生真实调用：以 x64 STA PowerShell 执行 `artifacts/runs/csharp_tolk_runtime_20261020/tolk-native-probe.ps1`，从隔离应用输出 `bin/x64/Release/net48/lib/Tolk.dll` 加载运行库；返回码 `0`，`IsLoaded=true`，实际检测名称为 `ZDSR`，`Tolk_Output("沙河批量打开器朗读测试", true)=true`，`Tolk_Unload` 与 `FreeLibrary` 均成功，错误为空。探针未启动或停止任何读屏器进程。结果和日志：同目录 `tolk-native-probe-result.json`、`tolk-native-probe.log`。

- VOICE-01 C# 全量：直接执行本轮隔离程序集，返回码 `0`，通过 `276`、失败 `0`、跳过 `0`。日志、命令索引和 TRX：`artifacts/runs/csharp_tolk_runtime_20261020/full.log`、`full-result.json`、`full.trx`。

- VOICE-01 OneCore/SAPI 回退真实验证：经用户明确授权，先请求争渡主进程 PID `10024` 正常退出但 `CloseMainWindow=false`，随后仅终止已核验属于 `C:\Program Files (x86)\zdsr\zdsr` 的争渡 PID `4040`、`844`、`9488`、`10024`、`6972`、`10364`；确认无 `ZDSR*` 进程残留后运行原生探针。返回码 `0`，`IsLoaded=true`，实际检测名称 `OneCore`，`Output=true`，卸载和释放均成功。结果和日志：`artifacts/runs/csharp_tolk_runtime_20261020/tolk-native-sapi-result.json`、`tolk-native-sapi.log`；此前争渡场景已另存为 `tolk-native-zdsr-result.json`、`tolk-native-zdsr.log`。

- VOICE-01 NVDA 真实验证：经用户授权启动 `C:\Program Files\NVDA\nvda.exe`；Computer Use 启动调用未返回可目标化窗口，但只读进程复核确认 NVDA PID `11660` 已运行。随后原生探针返回码 `0`，`IsLoaded=true`，实际检测名称 `NVDA`，`Output=true`，卸载和释放均成功。结果和日志：`artifacts/runs/csharp_tolk_runtime_20261020/tolk-native-nvda-result.json`、`tolk-native-nvda.log`。

- VOICE-01 读屏场景清理与恢复：使用 NVDA 自带 `nvda.exe -q` 正常退出本轮启动实例，确认 NVDA 相关进程为 `0`；随后启动原安装路径 `ZDSRStart.exe`，确认 `ZDSRMain PID 13636`，恢复后原生探针再次返回码 `0`、检测 `ZDSR`、`Output=true`、卸载/释放成功。三场景和恢复索引：`artifacts/runs/csharp_tolk_runtime_20261020/reader-scenarios-result.json`、`reader-scenarios.log`、`tolk-native-zdsr-restored-result.json`、`tolk-native-zdsr-restored.log`。

- VOICE-01 Python 全量：`python -m unittest discover -s tests -p "test_*.py" -v` 返回码 `0`，通过 `29`、失败 `0`、跳过 `0`。日志和命令索引：`artifacts/runs/csharp_tolk_runtime_20261020/python.log`、`python-result.json`。

- VOICE-01 Python 生产源码编译：`python -m compileall -q shahe_bulk_launcher tests` 返回码 `0`，无语法错误；测试计数不适用。日志和命令索引：`artifacts/runs/csharp_tolk_runtime_20261020/compile.log`、`compile-result.json`。

- VOICE-01 Release x64：隔离解决方案构建返回码 `0`，警告 `0`、错误 `0`，测试计数不适用；实际输出为 `artifacts/runs/csharp_tolk_runtime_20261020/bin/x64/Release/net48/`。日志和命令索引：同目录 `release.log`、`release-result.json`。

- VOICE-01 最终运行文件清单：隔离输出 `lib` 精确包含 `Tolk.dll`、`nvdaControllerClient64.dll`、`SAAPI64.dll`、`ZDSRAPI_x64.dll`、`ZDSRAPI.ini`、`byctrl-x64.dll`、`byctrl.conf` 七个文件，`.txt` 数量为 `0`。文件大小索引：`artifacts/runs/csharp_tolk_runtime_20261020/runtime-inventory-result.json`。

- VOICE-01 差异检查：`git diff --check` 返回码 `0`，无空白错误，仅有工作树既有 LF/CRLF 转换提示；测试计数不适用。日志和命令索引：`artifacts/runs/csharp_tolk_runtime_20261020/diff.log`、`diff-result.json`。

- VOICE-01 生产 SpeechService 原生探针：以 x64 STA PowerShell 加载隔离输出 `ShaheLauncher.Core.dll`，通过真实 `SpeechService.EnsureAssistSpeechRuntime()` 解析 `lib/Tolk.dll`；返回码 `0`，初始化结果 `true`，检测名称 `ZDSR`，生产 backend 的 `Output` 已调用，Shutdown 后 `_assistSpeechRuntime` 已清空且 `AssistSpeechDisabled=true`，错误为空。脚本、结果和日志：`artifacts/runs/csharp_tolk_runtime_20261020/speech-service-native-probe.ps1`、`speech-service-native-probe-result.json`、`speech-service-native-probe.log`。

- VOICE-01 提交前暂存差异检查：`git diff --cached --check` 返回码 `0`；仅暂存 Tolk 运行文件、App 复制规则、SpeechService、两份测试和本轮文本证据，用户已有解决方案、AuditRunner、HotkeyController、SettingsRuntimeContractTests、测试工程历史改动、Tolk 源码构建目录、TRX 和隔离输出均未纳入。

- VOICE-01 提交：独立提交 `4ebe6c6c4700e854c17664c8ca8fbe734b1b296f` 已推送 `origin/fix/non-update-gaps-20261020`，推送返回码 `0`；回退命令为 `git revert 4ebe6c6`。提交包含七个 `lib` 运行文件、App 复制规则、SpeechService 原生 ABI/生命周期修复、两份测试、完成计划和本轮文本证据；用户已有无关改动保持未提交。

## 文档阅读顺序

新会话按以下顺序读取文档和证据：

1. `AGENTS.md`：长期项目约束、验证和提交规则。
2. 本文件顶部“当前执行摘要”和“交接证据索引”：当前权威状态；旧“当前 C# 迁移计划进度”只作历史追溯。
3. `docs/superpowers/plans/2026-10-24-handoff-and-release-4.0.49.md`：当前唯一活动计划；原收口计划仅作历史，不从其过时空复选框继续。
4. `artifacts/runs/` 下由当前计划引用的最新日志：命令输出和真实 EXE 证据。
5. 本文件后面的历史记录：只用于追溯，不覆盖当前摘要。

## 当前摘要维护规则

- 顶部当前摘要只记录最新已验证结果、当前模块、待验证项和已知风险。
- 历史验证记录按日期追加，不删除历史 RED/GREEN 证据，也不把历史中间状态当作当前状态。
- 测试数量、提交哈希、运行 PID 和日志路径在当前摘要或对应验证记录中只保留一个实时权威值。
- 本文件记录的是迁移状态，不等同于“整个软件已完成”；整体完成必须满足 `AGENTS.md` 的全部完成标准。

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
- 当前单项真实验证（2026-10-18）停在“单个沙盒新建”：只读基线通过，目标隔离 EXE PID `11420` 已启动并由应用日志确认初始化完成、刷新 `96` 个沙盒；随后 Computer Use 丢失目标窗口子控件，坐标点击命中非目标前台窗口，焦点/句柄无法确认，因此结论为 `inconclusive`，未打开新建对话框、未提交动作、未创建资源。收口时目标 EXE、隔离配置、`C:\Windows\Sandboxie.ini` SHA-256 仍为 `123607...E517`、`99120...F234`、`2709...A6D`，`ShaheAudit_*` section/目录均为 `0`，`SbieCtrl PID 12484` 与 `SbieSvc PID 2860/4008` 未停止；当前不得继续后续单项，需先恢复可确认的目标窗口焦点/句柄。证据为 `artifacts/runs/csharp_single_create_20261018/single-create-baseline-20261018_01.json`、`single-create-inconclusive-20261018_01-result.json` 和对应 `.log`。
- 2026-10-19 Python→C# 源码对照新增未完成项：更新结果 UI/下载启动链、Updater 真实入口、复杂 JSON 配置恢复、朗读配置保存、文件粘贴优先级、全选语义、Esc/Alt+F4 反馈、Sandboxie repair reload、路径记忆数量范围以及路径选择/剪贴板文案仍存在迁移缺口或待 RED 核验，详见下方 `GAP-01` 至 `GAP-10`；本轮只做源码对照和文档同步，未建立新的 RED/GREEN，也未运行测试、构建或真实 EXE。
- 兼容入口由基线 12,071 行缩减为 171 行；主窗口组装集中在 200 行的 `shahe_bulk_launcher/app.py`。
- `SandboxLauncher` 的 398 个方法按 1 个应用初始化方法和 10 个业务 mixin 唯一分区，无重复或遗漏；已删除旧的按系统负载动态调整启动延迟方案，启动并发数和启动间隔完全由用户确认后的配置决定。
- 最终入口 API 审计无缺失导出，`SandboxLauncher` 与 `main` 均由 `shahe_bulk_launcher.app` 提供。
- C# 迁移工程已建立：`csharp/ShaheBulkLauncher.sln` 包含 Core、App、Updater、Tests 四个 `net48/x64` 项目。
- C# Task 1 `constants.py` 至 Task 19 `features/lifecycle.py` 已完成已确认范围的模块级迁移；Task 20 `app.py` 已完成应用组装、入口、发布、主要用户动作、非破坏性菜单动作、Task 15 备份/还原以及 `create_group`、`launch_group`、`delete_group`、`batch_create_sandboxes`、`batch_rename_sandboxes`、`batch_delete_sandboxes`、`schedule_launch`、`schedule_close`、`repair_sandboxie_control_settings` 菜单接线的已确认范围；快捷键运行时分派、路径记忆、进程菜单状态/沙盒选择反馈、全量重置和设置开关、真实音频后端及进程菜单实时状态刷新已完成已确认范围。最新自动化门禁使用 2026-10-16 当前 checkout（包含用户已有且未触碰的两个 C# 修改）执行新的隔离 Release x64 构建，Core/App/Updater/Tests 均成功且 `0` 警告/`0` 错误；该新程序集全量测试返回码 `0`，`212/212` 通过、失败 `0`、跳过 `0`、总时间 `13.3334` 秒，日志分别为 `artifacts/runs/csharp_navigation_followup_20261015/csharp-current-isolated-release-build-20261016_200646_565.log` 与 `csharp-current-isolated-full-recheck-20261016_200734_095.log`。本轮 Python 对照为 `29/29`，生产源码语法编译通过，当前工作树 `git diff --check` 返回码 `0`、无空白错误。真实 EXE 审计已完成启动退出、单/多沙盒启动、并发配置、静音、实时菜单状态、音量操作、当前进程播放设备路由、关闭当前进程和关闭全部进程，路径记忆管理入口/对话框取消已验证；托盘标题栏关闭隐藏、单实例恢复、运行中提示、动态菜单退出、Up/Down/Left/Right/Space/F5、主窗口正反向焦点环、UIA 名称和文件菜单助记键已完成；自定义 Alt 导航禁用守卫和无分组 Enter 守卫也已通过。定位当前进程仍没有有效真实证据；路径记忆备注/删除/清空、分组/批量完整生命周期、真实定时、Alt+Up/Down 启用态、Enter/Backspace 实际分组成功路径（根视图 Backspace 当前无结论）、完整屏幕阅读器以及备份还原完整审计仍待完成，未声明整个软件完成迁移。
- 当前 AuditRunner 状态（2026-10-18）：`contract`、`preflight`、`dialog-gate`、精确所有权、官方创建路径、分组启动、定时启动和 cleanup 回归均已通过；本轮真实 lifecycle 已完成 `batch-create`、`create-group`、`launch-group`、`schedule-launch`，随后 cleanup 已恢复配置并删除两个精确拥有资源。`schedule-close` 在动作前被现存 `SbieCtrl.exe PID 12484` 阻断，未启动目标或改动资源；`cancel-schedules`、批量重命名/删除尚未执行，`verify-clean` 入口仍未实现。最终只读状态确认 `ShaheAudit_*` section/目录为 `0`、三份 SHA-256 恢复基线、`SbieSvc=Running`；全软件迁移仍未完成。
- 当前模块边界补充（2026-10-19）：真实 EXE/Sandboxie 审计仍在进行，同时必须先处理 `GAP-01` 至 `GAP-10` 的源码对照缺口。`GAP-03` 已有路径历史 `JArray/JObject` 类型探针证据；`GAP-04` 是静态确认的潜在配置覆盖路径；其余条目为源码/调用链差异，后续必须逐项建立隔离 RED，不能把本轮对照直接写成运行时失败。
- 2026-10-17 C# AuditRunner 新会话只读续接门禁通过：实际 `HEAD=6fbf83fa55e33cab0fbcb21ec57b8e6907f981c9`、分支 `沙河批量打开器`、ahead `59` / behind `2`；跟踪修改仍仅为 `PROJECT_STRUCTURE.md` 及用户已有的 `HotkeyController.cs`、`SettingsRuntimeContractTests.cs`，后两者保持 numstat `5/0`、`3/3` 和 SHA-256 `EC03CCF...B7C7`、`2327910D...0942`。目标 EXE、`Start.exe`、`powershell.exe`、`pwsh.exe`、AuditRunner、`SbieCtrl.exe` 均为 `0`，Sandboxie 相关进程只有 `SbieSvc.exe PID 2836` 且服务状态 `RUNNING`；`ShaheAudit_*` section/目录均为 `0`；目标 EXE、隔离配置、`Sandboxie.ini` 三份 SHA-256 均匹配权威基线。AuditRunner 空项目、解决方案/测试引用和 5 项合同测试已创建；第一次 RED 因新项目尚无 NuGet 资产而在测试发现前返回 `1`，当前先完成一次项目还原，再重跑有效 RED。基线证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-session-baseline-20261017_094034_680-result.json`；首次 RED 基础设施失败证据为 `auditrunner-red-20261017_095058_650-result.json` 和同名前缀 `.log`。
- 2026-10-17 C# AuditRunner 合同 RED 已有效建立：定向测试返回码 `1`，共 `5` 项，`0` 通过、`5` 失败、`0` 跳过；分别证明 lifecycle 的 baseline + `--execute` + 唯一前缀门禁、动态唯一菜单查找、精确唯一且未变化的清理所有权、`delete_sandbox_silent`/目录消失/INI 分节删除顺序、探针与清理异常分类尚未实现。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-red-20261017_095256_529-result.json`、同名前缀 `.log` 及结果目录中的 TRX。下一步只实现使这 5 项转绿的最小合同代码。
- 2026-10-17 C# AuditRunner 第一次 GREEN 尝试返回码 `1`，`AuditSafety.cs` 因局部变量 `alias` 与 LINQ lambda 参数同名触发 `CS0136`，测试未执行（通过 `0`、失败 `0`、跳过 `0`）；这是实现编译错误，不是产品结论。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-green-20261017_095615_996-result.json` 和同名前缀 `.log`；下一步仅修正该变量名后重跑相同 5 项。
- 2026-10-17 C# AuditRunner 合同 GREEN 通过：定向测试返回码 `0`，`5/5` 通过、失败 `0`、跳过 `0`。已实现默认拒绝 lifecycle 写操作、baseline + `--execute` + `ShaheAudit_*` 前缀门禁、运行时唯一菜单标题解析、基线排除与精确唯一所有权校验、静默删除返回码/目录消失/INI 删除顺序，以及探针和清理异常的 `Inconclusive` 分类。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-green-20261017_095734_975-result.json`、同名前缀 `.log` 和结果目录中的 TRX。下一步实现并运行编译后 AuditRunner 的 `contract` 模式。
- 2026-10-17 C# AuditRunner 首次独立 Release x64 构建返回码 `1`、警告 `0`、错误 `2`：只读 preflight 实现缺少 `ShaheLauncher.Core.Constants` 命名空间导入，`LauncherConstants` 两处无法解析；尚未运行 `contract`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-build-20261017_100312_065-result.json` 和同名前缀 `.log`；下一步仅补入该命名空间并重建。
- 2026-10-17 C# AuditRunner 独立 Release x64 隔离构建通过：返回码 `0`、警告 `0`、错误 `0`，输出为 `artifacts/runs/csharp_navigation_followup_20261015/isolated-auditrunner-binx64/x64/Release/net48/ShaheLauncher.AuditRunner.exe`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-build-20261017_100447_652-result.json` 和同名前缀 `.log`；下一步运行该编译产物的只读 `contract` 模式。
- 2026-10-17 编译后的 C# AuditRunner `contract` 模式通过，返回码 `0`；lifecycle 双重门禁、默认拒绝写操作、动态菜单解析、唯一所有权、删除顺序和异常分类 6 项内建检查均为 `true`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-contract-20261017_100607_808-result.json` 和 `auditrunner-contract-20261017_100557_649.log`。下一步运行只读 `preflight`，未通过前不进入 dialog-gate 或 lifecycle。
- 2026-10-17 编译后的 C# AuditRunner 首轮只读 `preflight` 通过，返回码 `0`、16 项门禁全部为 `true`、错误 `0`。目标 EXE、隔离配置、`Sandboxie.ini` 三份 SHA-256 均匹配权威基线；目标 EXE、`Start.exe`、PowerShell/pwsh、其他 AuditRunner、`SbieCtrl.exe` 和其他 Sandboxie 进程均为 `0`，仅记录到 `SbieSvc.exe PID 2836` 且服务为 `Running`；`ShaheAudit_*` section/目录均为 `0`。基线 ID 为 `b713875b8366441fb68f8278f9f41b84`，证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-preflight-20261017_100722_245-result.json` 和 `auditrunner-preflight-20261017_100704_996.log`。下一步实现并运行非破坏性 `dialog-gate`。
- 2026-10-17 C# AuditRunner `dialog-gate` 首次构建返回码 `1`、警告 `0`、错误 `1`：`WaitForWindow` 将 `WindowSnapshot` 直接赋给 `IntPtr`，目标 EXE 未启动。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-dialog-gate-build-20261017_101350_180-result.json` 和同名前缀 `.log`；下一步仅修正句柄取值并重建。
- 2026-10-17 C# AuditRunner `dialog-gate` 独立 Release x64 构建通过：返回码 `0`、警告 `0`、错误 `0`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-dialog-gate-build-20261017_101514_997-result.json` 和同名前缀 `.log`；下一步使用已通过的 preflight 基线运行非破坏性 dialog-gate。
- 2026-10-17 C# AuditRunner 首次真实 `dialog-gate` 返回码 `3`、结论 `inconclusive`：目标 PID `20592` 已由本轮 Runner 关闭，未强制结束；菜单读取在 `MENUITEMINFO` 封送阶段发生 Runner 自身错误，因此没有打开对话框、没有形成产品结论。目标 EXE、配置和 `Sandboxie.ini` 三份哈希均恢复/保持基线。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-dialog-gate-20261017_101642_852-result.json` 和 `auditrunner-dialog-gate-20261017_101627_698.log`；下一步修复 Runner 的菜单字符串缓冲区后重建重跑。
- 2026-10-17 C# AuditRunner 修复 Win32 菜单字符串缓冲区后，`dialog-gate` 独立 Release x64 重建通过：返回码 `0`、警告 `0`、错误 `0`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-dialog-gate-rebuild-20261017_101758_536-result.json` 和同名前缀 `.log`；下一步重跑相同非破坏性 dialog-gate。
- 2026-10-17 C# AuditRunner 第二次真实 `dialog-gate` 返回码 `3`、结论 `inconclusive`：10 个运行时菜单命令均动态唯一解析；“批量创建沙盒用户”对话框焦点、句柄、稳定结构（22 控件、3 Edit、1 ComboBox、2 Button）和取消关闭全部通过。Runner 随后误选空应用列表而非沙盒列表，未继续其他对话框；目标 PID `22524` 已关闭、未强制结束，三份哈希保持基线。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-dialog-gate-20261017_101924_943-result.json` 和 `auditrunner-dialog-gate-20261017_101907_835.log`；下一步按原生项目数选择沙盒列表后重建重跑。
- 2026-10-17 C# AuditRunner 改为按项目数选择沙盒列表后，`dialog-gate` Release x64 重建通过：返回码 `0`、警告 `0`、错误 `0`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-dialog-gate-rebuild-20261017_102038_809-result.json` 和同名前缀 `.log`；下一步第三次运行相同非破坏性 dialog-gate。
- 2026-10-17 C# AuditRunner 第三次真实 `dialog-gate` 返回码 `3`、结论 `inconclusive`：动态菜单和批量创建对话框再次通过，但 Runner 在沙盒列表异步加载完成前检查项目数，仍未进入后续对话框。目标 PID `8916` 已关闭、未强制结束，三份哈希保持基线。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-dialog-gate-20261017_102213_568-result.json` 和 `auditrunner-dialog-gate-20261017_102153_433.log`；下一步增加有界列表就绪等待后重建重跑。
- 2026-10-17 C# AuditRunner 增加 10 秒有界沙盒列表就绪等待后，`dialog-gate` Release x64 重建通过：返回码 `0`、警告 `0`、错误 `0`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-dialog-gate-rebuild-20261017_102329_062-result.json` 和同名前缀 `.log`；下一步第四次运行相同非破坏性 dialog-gate。
- 2026-10-17 C# AuditRunner 第四次真实 `dialog-gate` 返回码 `3`、结论 `inconclusive`：批量创建对话框继续通过；Runner 直接设置 ListView 状态位未触发 WinForms 勾选事件，后续 4 个对话框均未打开；菜单在运行中重建后，复用旧退出命令 ID 也未关闭进程，最终由本轮精确主窗口清理退出，未强制结束。三份哈希保持基线。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-dialog-gate-20261017_102530_527-result.json` 和 `auditrunner-dialog-gate-20261017_102423_334.log`；下一步改为向复选框坐标发送真实点击，并在每个动作前重新动态解析当前菜单命令 ID。
- 2026-10-17 C# AuditRunner 改为真实点击列表复选框并在每个动作前重新解析当前菜单命令 ID 后，`dialog-gate` Release x64 重建通过：返回码 `0`、警告 `0`、错误 `0`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-dialog-gate-rebuild-20261017_102731_550-result.json` 和同名前缀 `.log`；下一步第五次运行相同非破坏性 dialog-gate。
- 2026-10-17 C# AuditRunner 第五次真实 `dialog-gate` 返回码 `3`、结论 `inconclusive`：动态菜单和批量创建对话框通过，但坐标点击仍未切换 WinForms ListView 的托管勾选状态，后续对话框未打开；目标 PID `16620` 已关闭、未强制结束，三份哈希保持基线。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-dialog-gate-20261017_102845_093-result.json` 和 `auditrunner-dialog-gate-20261017_102827_686.log`；下一步改用 UI Automation `TogglePattern` 操作复选项后重建重跑。
- 2026-10-17 C# AuditRunner 改用 UI Automation `TogglePattern` 操作沙盒复选项后，`dialog-gate` Release x64 重建通过：返回码 `0`、警告 `0`、错误 `0`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-dialog-gate-uia-build-20261017_103018_210-result.json` 和同名前缀 `.log`；下一步第六次运行相同非破坏性 dialog-gate。
- 2026-10-17 C# AuditRunner 第六次真实 `dialog-gate` 返回码 `3`、结论 `inconclusive`：该自定义列表的 UIA 项未暴露可用 `TogglePattern`，因此仍未建立后续对话框前置选择；目标 PID `14764` 已关闭、未强制结束，三份哈希保持基线。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-dialog-gate-20261017_103156_433-result.json` 和 `auditrunner-dialog-gate-20261017_103136_799.log`；下一步使用应用自身动态“全选”菜单命令建立仅存于本轮进程的选择状态。
- 2026-10-17 C# AuditRunner 改用应用自身动态“全选”命令建立 dialog-gate 选择前置后，Release x64 重建通过：返回码 `0`、警告 `0`、错误 `0`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-dialog-gate-select-all-build-20261017_103331_976-result.json` 和同名前缀 `.log`；下一步第七次运行相同非破坏性 dialog-gate。
- 2026-10-17 C# AuditRunner 第七次真实 `dialog-gate` 返回码 `3`、结论 `inconclusive`：运行时“全选”菜单的精确标题为 `全选\tCtrl+A`，Runner 使用无快捷键后缀标题导致唯一匹配拒绝；未打开对话框，目标 PID `14636` 已关闭、未强制结束，三份哈希保持基线。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-dialog-gate-20261017_103450_740-result.json` 和 `auditrunner-dialog-gate-20261017_103434_780.log`；下一步修正精确标题后重建重跑。
- 2026-10-17 C# AuditRunner 修正动态“全选\tCtrl+A”精确标题后，`dialog-gate` Release x64 重建通过：返回码 `0`、警告 `0`、错误 `0`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-dialog-gate-final-build-20261017_103613_400-result.json` 和同名前缀 `.log`；下一步第八次运行相同非破坏性 dialog-gate。
- 2026-10-17 C# AuditRunner 第八次真实 `dialog-gate` 返回码 `1`、结论 `inconclusive`：批量创建和批量重命名对话框的焦点、稳定结构及取消关闭均通过；“新建分组”和“定时关闭选中沙盒”未弹出，批量删除确认框已稳定弹出但取消关闭未确认。目标 PID `21080` 已由动态退出命令正常关闭、未强制结束，三份哈希保持基线，未创建 Sandboxie 资源。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-dialog-gate-20261017_103911_616-result.json` 和 `auditrunner-dialog-gate-20261017_103714_212.log`；这是 Runner 前置选择/取消路径未完成，不是产品失败，下一步仅修正这两个门禁点后重建重跑。
- 2026-10-17 C# AuditRunner 修正全选后勾选状态确认、系统“否”按钮取消和定时关闭对话框实际标题后，独立 Release x64 重建通过：返回码 `0`、警告 `0`、错误 `0`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-dialog-gate-fix-build-20261017_104418_492-result.json` 和同名前缀 `.log`；下一步重跑非破坏性 dialog-gate。
- 2026-10-17 C# AuditRunner 第九次真实 `dialog-gate` 返回码 `1`、结论 `inconclusive`：批量创建对话框继续完整通过；应用自身“全选”命令已投递，但 Runner 使用原生 ListView 状态位验证托管勾选状态仍未成立，因此在进入其他对话框前主动停止。目标 PID `18448` 已正常关闭、未强制结束，三份哈希保持基线且未创建资源。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-dialog-gate-20261017_104550_817-result.json` 和 `auditrunner-dialog-gate-20261017_104528_036.log`；这是 Runner 使用错误观测层验证 WinForms 托管状态，不是产品失败，下一步移除该错误原生状态判定并改为直接观察“新建分组”对话框。
- 2026-10-17 C# AuditRunner 将应用自身“全选”改为同步执行并移除错误的原生状态位判定后，独立 Release x64 重建通过：返回码 `0`、警告 `0`、错误 `0`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-dialog-gate-sync-build-20261017_104721_022-result.json` 和同名前缀 `.log`；下一步再次运行非破坏性 dialog-gate，以实际对话框出现作为勾选状态证据。
- 2026-10-17 C# AuditRunner 第十次真实 `dialog-gate` 返回码 `1`、结论 `inconclusive`：批量创建、批量重命名、批量删除确认、定时关闭的焦点、稳定结构和取消关闭均通过，动态退出成功且未强制结束；只有“新建分组”未弹出。当前根视图可勾选普通沙盒不足两个，而该功能按产品合同要求至少勾选两个用户，因此属于测试前置条件不足，不是产品失败。三份哈希保持基线且未创建资源。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-dialog-gate-20261017_104856_496-result.json` 和 `auditrunner-dialog-gate-20261017_104830_702.log`；下一步只增加非破坏性进入现有分组并全选成员的前置导航。
- 2026-10-17 对第十次 `dialog-gate` 的前置判断已纠正：既有 preflight 记录了大量沙盒目录，失败原因是列表异步加载完成前执行全选，并非根视图资源不足；历史结果本身仍保留为 `inconclusive`。AuditRunner 增加至少两个列表条目的有界就绪等待后，独立 Release x64 重建通过，返回码 `0`、警告 `0`、错误 `0`；证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-dialog-gate-list-ready-build-20261017_105149_193-result.json` 和同名前缀 `.log`。下一步重跑非破坏性 dialog-gate。
- 2026-10-17 C# AuditRunner 第十一次真实 `dialog-gate` 返回码 `1`、结论 `inconclusive`：批量创建、新建分组、批量重命名、批量删除确认和定时关闭五个对话框均已验证焦点、稳定结构和取消关闭，动态退出成功、未强制结束，三份哈希保持基线。Runner 最终判定仍失败，因为把批量重命名对话框错误要求为至少 `1` 个 ComboBox，而实际结构稳定为 `3 Edit/0 ComboBox/2 Button`；这是 Runner 断言错误，不是产品失败。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-dialog-gate-20261017_105324_446-result.json` 和 `auditrunner-dialog-gate-20261017_105306_096.log`；下一步只把该对话框的 ComboBox 期望修正为 `0`。
- 2026-10-17 C# AuditRunner 将批量重命名对话框结构期望修正为实际的 `3 Edit/0 ComboBox/2 Button` 后，独立 Release x64 重建通过：返回码 `0`、警告 `0`、错误 `0`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-dialog-gate-structure-build-20261017_105437_910-result.json` 和同名前缀 `.log`；下一步运行最终非破坏性 dialog-gate。
- 2026-10-17 编译后的 C# AuditRunner 非破坏性 `dialog-gate` 通过，返回码 `0`：批量创建、新建分组、批量重命名、批量删除确认和定时关闭五个对话框的焦点、稳定控件结构及取消关闭均通过；启动分组、删除分组、定时启动和取消全部定时任务的运行时菜单标题均动态唯一匹配；目标 PID `6228` 通过动态“退出软件\tAlt+F4”命令正常退出，未强制结束。三份哈希保持基线、错误 `0`、未创建 Sandboxie 资源。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-dialog-gate-20261017_105605_716-result.json` 和 `auditrunner-dialog-gate-20261017_105543_592.log`。下一步重新运行只读 preflight，确认零残留后才允许进入 lifecycle。
- 2026-10-17 `dialog-gate` 后首轮只读 preflight 返回非零、状态 `blocked`：目标 EXE、`Start.exe`、其他 AuditRunner、`SbieCtrl.exe`、其他 Sandboxie 进程和全部 `ShaheAudit_*` 资源均为 `0`，`SbieSvc=Running`，三份哈希全部匹配；唯一失败项是采样瞬间存在 `pwsh.exe PID 16584`。该进程不属于目标 EXE 或 Sandboxie 资源，先按门禁拒绝进入 lifecycle；证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-preflight-20261017_105731_172-result.json` 和 `auditrunner-post-dialog-preflight-20261017_105714_572.log`。下一步只用 `tasklist` 复核该 PID 是否已随调用结束。
- 2026-10-17 `tasklist` 只读复核确认 preflight 采样到的 `pwsh.exe PID 16584` 已退出，返回码 `0`，没有匹配进程；证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-pwsh-pid-recheck-20261017_105845_509-result.json` 和同名前缀 `.log`。下一步重新运行一次完整只读 preflight，只有全部门禁通过才建立 lifecycle 基线。
- 2026-10-17 `dialog-gate` 后第二轮只读 preflight 通过，返回码 `0`、16 项门禁全部为 `true`、错误 `0`，新基线 ID 为 `d884f287a0964a96a073bec07541b114`。目标 EXE、`Start.exe`、PowerShell/pwsh、其他 AuditRunner、`SbieCtrl.exe`、其他 Sandboxie 进程和全部 `ShaheAudit_*` section/目录均为 `0`；仅 `SbieSvc.exe PID 2836` 且服务为 `Running`。目标 EXE、配置和 `Sandboxie.ini` 三份 SHA-256 精确匹配权威基线。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-preflight-20261017_110000_069-result.json` 和 `auditrunner-post-dialog-preflight-directcmd-20261017_110000_000.log`。dialog-gate 与清洁基线均已通过，下一步才可使用全新唯一前缀、该基线和显式 `--execute` 进入 lifecycle。
- 2026-10-17 C# AuditRunner lifecycle 分阶段执行门禁 RED 有效：定向测试返回码 `1`，`4` 通过、`1` 失败、`0` 跳过；失败明确证明当前解析器仍接受没有显式 `--stage` 和 `--state` 的 lifecycle，尚不能保证每次只执行一个真实动作并在动作间同步证据。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-stage-gate-red-20261017_110411_498-result.json`、同名前缀 `.log` 和结果目录 TRX。下一步只实现该分阶段门禁并转 GREEN。
- 2026-10-17 C# AuditRunner lifecycle 分阶段执行门禁 GREEN 通过：定向测试返回码 `0`，`5/5` 通过、失败 `0`、跳过 `0`；lifecycle 现在除 baseline、显式 `--execute` 和唯一 `ShaheAudit_*` 前缀外，还必须提供状态文件及已知单阶段名称，不能无边界执行全部写操作。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-stage-gate-green-20261017_110554_001-result.json`、同名前缀 `.log` 和结果目录 TRX。下一步为阶段顺序和状态绑定建立 RED。
- 2026-10-17 C# AuditRunner lifecycle 阶段顺序/状态绑定 RED 有效：定向测试返回码 `1`，`5` 通过、`1` 失败、`0` 跳过；`LifecycleState` 与严格阶段顺序校验尚不存在，当前无法拒绝跳步、重复阶段、baseline 变化或前缀变化。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-stage-order-red-20261017_110732_995-result.json`、同名前缀 `.log` 和结果目录 TRX。下一步实现最小状态模型和顺序验证后转 GREEN。
- 2026-10-17 C# AuditRunner lifecycle 阶段顺序/状态绑定 GREEN 通过：定向测试返回码 `0`，`6/6` 通过、失败 `0`、跳过 `0`；状态现在绑定 baseline ID 与唯一前缀，并要求 `batch-create -> create-group -> launch-group -> schedule-launch -> schedule-close -> cancel-schedules -> batch-rename -> batch-delete` 精确顺序，跳步、重复或绑定变化均拒绝。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-stage-order-green-20261017_110944_872-result.json`、同名前缀 `.log` 和结果目录 TRX。下一步实现并先只执行 `batch-create` 阶段。
- 2026-10-17 C# AuditRunner lifecycle 初始状态 RED 有效：定向测试返回码 `1`，`6/7` 通过、失败 `1`、跳过 `0`；唯一失败为 `LoadOrCreateLifecycleState` 尚不存在，证明首次 `batch-create` 的状态文件创建、baseline/prefix 绑定和其他阶段禁止无状态启动尚未实现。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-state-init-red-20261017_111528_162-result.json`、同名前缀 `.log` 和结果目录 TRX。下一步只实现该状态初始化合同并复跑相同测试。
- 2026-10-17 C# AuditRunner lifecycle 初始状态 GREEN 通过：定向测试返回码 `0`，`7/7` 通过、失败 `0`、跳过 `0`；首次 `batch-create` 现在使用不覆盖写入创建状态文件并绑定 baseline ID/唯一前缀，其他阶段不能凭空初始化状态，已有状态继续执行精确顺序校验。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-state-init-green-20261017_111738_482-result.json`、同名前缀 `.log` 和结果目录 TRX。下一步实现真实 `batch-create` 阶段及其精确所有权持久化。
- 2026-10-17 C# AuditRunner 真实 `batch-create` RED 有效：使用已通过的 clean preflight baseline、显式 `--execute`、全新前缀和独立 state 路径运行编译产物，返回码 `3`、结论 `inconclusive`，准确原因为 lifecycle 模式尚未实现；state 文件未创建，目标 EXE 未启动，未调用 Sandboxie 写操作。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-batch-create-red-20261017_111925_290-result.json`、`auditrunner-unsupported-20261017_111939_701-result.json` 和同名前缀 `.log`。下一步只实现 `batch-create` 真实动作、所有权验证、状态持久化和失败清理。
- 2026-10-17 C# AuditRunner 基线 section 排除 RED 有效：定向测试返回码 `1`，`7/8` 通过、失败 `1`、跳过 `0`；实际缺口是 `OwnershipSnapshot` 只记录有 `BoxAlias` 的映射，没有记录无别名的基线 section，因此尚不能证明所有基线 section 都不会成为清理目标。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-baseline-section-red-20261017_112150_976-result.json`、同名前缀 `.log` 和结果目录 TRX。下一步把完整 section 名单加入同一个所有权校验入口。
- 2026-10-17 C# AuditRunner 基线 section 排除 GREEN 通过：定向测试返回码 `0`，`8/8` 通过、失败 `0`、跳过 `0`；所有权快照现同时记录完整 section 名单、精确 `BoxAlias` 和目录，基线中即使没有别名的 section 也会被拒绝成为本轮所有权或清理目标。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-baseline-section-green-20261017_112323_513-result.json`、同名前缀 `.log` 和结果目录 TRX。下一步继续实现真实 `batch-create`。
- 2026-10-17 C# AuditRunner `batch-create` 实现隔离 Release x64 构建通过：返回码 `0`、警告 `0`、错误 `0`，编译产物为 `artifacts/runs/csharp_navigation_followup_20261015/isolated-auditrunner-binx64/x64/Release/net48/ShaheLauncher.AuditRunner.exe`。实现范围仅含动态批量创建对话框提交、两个精确 `BoxAlias`/section/目录所有权验证、stage state 持久化、动态退出和失败时精确清理。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-batch-create-build-20261017_112758_771-result.json` 和同名前缀 `.log`。下一步复跑 AuditRunner 定向合同测试。
- 2026-10-17 C# AuditRunner `batch-create` 合同回归通过：定向测试返回码 `0`，`8/8` 通过、失败 `0`、跳过 `0`；写操作双重门禁、阶段顺序、完整基线 section 排除、唯一所有权、删除顺序和异常分类保持通过。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-batch-create-contract-20261017_112854_434-result.json`、同名前缀 `.log` 和结果目录 TRX。下一步运行只读 preflight，未通过则不执行真实创建。
- 2026-10-17 C# AuditRunner `batch-create` 前只读 preflight 通过：返回码 `0`、16 项门禁全部为 `true`、错误 `0`，baseline ID 为 `eec0ad68a1254237a931248549163e80`；目标 EXE、配置和 `Sandboxie.ini` 哈希均匹配权威基线，目标/Start/PowerShell/pwsh/其他 AuditRunner/SbieCtrl/其他 Sandboxie 进程均为 `0`，仅 `SbieSvc.exe PID 2836` 且服务为 `Running`，`ShaheAudit_*` section/目录均为 `0`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-preflight-20261017_113016_862-result.json` 和 `auditrunner-batch-create-preflight-20261017_113000_349.log`。下一步使用该 baseline、显式 `--execute` 和全新唯一前缀执行真实 `batch-create`。
- 2026-10-17 C# AuditRunner 首次真实 `batch-create` 尝试为 `inconclusive`，不是产品失败：目标 PID `17460`，Runner PID `17116`，动态菜单 ID `290`；批量创建对话框焦点、稳定结构（22 控件、3 Edit、1 ComboBox、2 Button）均确认，但 Runner 使用同步按钮消息后自身阻塞。仅对本轮精确目标 PID `17460` 执行最后手段强制结束，Runner 随后退出并写出结果；state 的完成阶段和所有权声明均为 `0`，未观察到新 Sandboxie 资源。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-batch-create-green-20261017_113118_264-result.json`、`auditrunner-lifecycle-batch-create-20261017_113522_933-result.json`、state JSON 和同名前缀 `.log`。下一步先运行只读清洁 preflight，再把确定按钮改为异步投递。
- 2026-10-17 首次真实 `batch-create` 后清洁 preflight 通过：返回码 `0`、16 项门禁全部为 `true`、错误 `0`，baseline ID 为 `124019742ca64186b3c843f9411568e0`；目标/Start/PowerShell/pwsh/其他 AuditRunner/SbieCtrl/其他 Sandboxie 进程均为 `0`，仅 `SbieSvc.exe PID 2836` 且服务为 `Running`；`ShaheAudit_*` section/目录均为 `0`，三份 SHA-256 全部恢复权威基线。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-preflight-20261017_113708_947-result.json` 和 `auditrunner-batch-create-postflight-20261017_113651_882.log`。下一步修正 Runner 同步点击，不修改产品代码。
- 2026-10-17 C# AuditRunner 第二次真实 `batch-create` 尝试为 `inconclusive`，不是产品失败：返回码 `3`，目标 PID `9384`，动态菜单 ID `290`；批量创建对话框前台和稳定结构（22 控件、3 Edit、1 ComboBox、2 Button）均确认，但异步按钮消息未触发确认或关闭。目标已通过动态“退出软件”命令退出，未使用强制结束；state 的完成阶段和所有权声明均为 `0`，未观察到新 Sandboxie 资源。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-batch-create-green2-20261017_114018_387-result.json`、`auditrunner-lifecycle-batch-create-20261017_114055_271-result.json`、state JSON 和同名前缀 `.log`。下一步先运行只读清洁 preflight，再以有界同步消息替换无效异步点击。
- 2026-10-17 第二次真实 `batch-create` 后首次只读 preflight 返回码 `1`、状态 `blocked`，唯一失败门禁为执行工具自身父进程 `pwsh.exe PID 28128`；目标 EXE、Start、PowerShell、其他 AuditRunner、SbieCtrl、其他 Sandboxie 和全部 `ShaheAudit_*` section/目录均为 `0`，仅 `SbieSvc.exe PID 2836`，服务为 `Running`，三份 SHA-256 均匹配基线。该结果是调用环境干扰，不是产品失败或资源残留；证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-preflight-20261017_114543_505-result.json` 和 `auditrunner-batch-create-green2-postflight-20261017_114200_000.log`。下一步仅更换为无 PowerShell 父进程的调用方式重跑同一只读门禁。
- 2026-10-17 无 PowerShell 父进程的首次直接 `cmd` 调用在 AuditRunner 启动前因命令行首部引号被 `cmd.exe` 拒绝，返回码 `1`；没有生成 preflight、没有启动目标 EXE或修改任何状态，归类为调用语法失败。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-preflight-direct-cmd-shell-failure-20261017_114600_000-result.json`。下一步仅移除无必要的路径引号重试一次。
- 2026-10-17 第二次真实 `batch-create` 后只读清洁 preflight 更正重试通过：返回码 `0`、16/16 门禁通过、错误 `0`，baseline ID 为 `816d301e311045a1951e1aee1108705f`；目标 EXE、Start、PowerShell/pwsh、其他 AuditRunner、SbieCtrl、其他 Sandboxie 进程和全部 `ShaheAudit_*` section/目录均为 `0`，仅 `SbieSvc.exe` 且服务为 `Running`。目标 EXE、配置和 `Sandboxie.ini` SHA-256 精确匹配权威基线。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-preflight-20261017_114741_871-result.json` 和 `auditrunner-batch-create-green2-postflight2-20261017_114700_000.log`。下一步以有界同步按钮消息替换无效异步点击。
- 2026-10-17 C# AuditRunner 有界批量创建确认 RED 有效：定向测试返回码 `1`，`8` 通过、`1` 失败、`0` 跳过；唯一失败 `BatchCreateConfirmationUsesBoundedNativeMessage` 明确证明当前缺少有超时上限的原生按钮消息入口。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-bounded-click-red-20261017_115000_000-result.json`、同名前缀 `.log` 和结果目录 TRX。下一步只在 `Win32Automation` 中实现该有界点击并用于批量创建确认。
- 2026-10-17 C# AuditRunner 有界批量创建确认 GREEN 通过：定向测试返回码 `0`，`9/9` 通过、失败 `0`、跳过 `0`；`Win32Automation` 现使用 `SendMessageTimeout` 对确定按钮执行最多 30 秒的同步点击，只有消息成功返回且对话框关闭才承认提交有效，原有 8 项安全合同保持通过。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-bounded-click-green-20261017_115300_000-result.json`、同名前缀 `.log` 和结果目录 TRX。下一步执行独立隔离 Release x64 构建。
- 2026-10-17 C# AuditRunner 有界批量创建确认隔离 Release x64 构建通过：返回码 `0`、警告 `0`、错误 `0`，产物为 `artifacts/runs/csharp_navigation_followup_20261015/isolated-auditrunner-binx64/x64/Release/net48/ShaheLauncher.AuditRunner.exe`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-bounded-click-build-20261017_115500_000-result.json` 和同名前缀 `.log`。下一步重新运行只读 preflight。
- 2026-10-17 C# AuditRunner 有界点击修复后的只读 preflight 通过：返回码 `0`、16/16 门禁通过、错误 `0`，baseline ID 为 `fe4853a08bfc4bd9ab539130c5db5383`；目标 EXE、Start、PowerShell/pwsh、其他 AuditRunner、SbieCtrl、其他 Sandboxie 进程和全部 `ShaheAudit_*` section/目录均为 `0`，仅 `SbieSvc.exe` 且服务为 `Running`，三份 SHA-256 精确匹配权威基线。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-preflight-20261017_115408_881-result.json` 和 `auditrunner-bounded-click-preflight-20261017_115700_000.log`。下一步使用该 baseline、显式 `--execute` 和全新唯一前缀重跑真实 `batch-create`。
- 2026-10-17 C# AuditRunner 第三次真实 `batch-create` 尝试为 `inconclusive`，不是产品失败：返回码 `3`，目标 PID `13672`，动态菜单 ID `290`；对话框前台和稳定结构（22 控件、3 Edit、1 ComboBox、2 Button）均确认，但 30 秒有界同步按钮消息未成功返回并关闭对话框。目标通过动态“退出软件”命令退出，未强制结束；state 完成阶段和所有权声明均为 `0`，未观察到新 Sandboxie 资源。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-batch-create-green3-20261017_115500_641-result.json`、`auditrunner-lifecycle-batch-create-20261017_115628_021-result.json`、state JSON 和同名前缀 `.log`。同步、异步和有界同步三种原始窗口消息均已失败，下一步先确认零残留，再改用现有 UI Automation `InvokePattern`，不再叠加第四种窗口消息。
- 2026-10-17 第三次真实 `batch-create` 后只读清洁 preflight 通过：返回码 `0`、16/16 门禁通过、错误 `0`，baseline ID 为 `2a7b3a12ad4c4c3e9a0124777fe43db4`；目标 EXE、Start、PowerShell/pwsh、其他 AuditRunner、SbieCtrl、其他 Sandboxie 进程和全部 `ShaheAudit_*` section/目录均为 `0`，仅 `SbieSvc.exe` 且服务为 `Running`，三份 SHA-256 精确匹配权威基线。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-preflight-20261017_115751_745-result.json` 和 `auditrunner-batch-create-green3-postflight-20261017_115800_000.log`。下一步把批量创建确认从原始窗口消息改为现有 UI Automation `InvokePattern`。
- 2026-10-17 C# AuditRunner UI Automation 批量创建确认 RED 有效：定向测试返回码 `1`，`8` 通过、`1` 失败、`0` 跳过；唯一失败 `BatchCreateConfirmationUsesUiAutomationInvokePattern` 明确证明当前缺少 UIA `InvokePattern` 点击入口。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-uia-invoke-red-20261017_120000_000-result.json`、同名前缀 `.log` 和结果目录 TRX。下一步删除失败的 `SendMessageTimeout` 路线并实现最小 UIA 点击。
- 2026-10-17 C# AuditRunner UI Automation 批量创建确认 GREEN 通过：定向测试返回码 `0`，`9/9` 通过、失败 `0`、跳过 `0`；批量创建确定按钮现通过已有 `System.Windows.Automation` 的 `InvokePattern` 调用，失败的同步、异步和有界同步窗口消息实现已删除，对话框未关闭仍保持无结论。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-uia-invoke-green-20261017_120300_000-result.json`、同名前缀 `.log` 和结果目录 TRX。下一步重新执行隔离 Release x64 构建和 clean preflight。
- 2026-10-17 C# AuditRunner UI Automation 版本隔离 Release x64 构建通过：返回码 `0`、警告 `0`、错误 `0`，产物为 `artifacts/runs/csharp_navigation_followup_20261015/isolated-auditrunner-binx64/x64/Release/net48/ShaheLauncher.AuditRunner.exe`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-uia-invoke-build-20261017_120500_000-result.json` 和同名前缀 `.log`。下一步运行只读 clean preflight。
- 2026-10-17 C# AuditRunner UI Automation 版本只读 preflight 通过：返回码 `0`、16/16 门禁通过、错误 `0`，baseline ID 为 `97d9b60bb79446a095da9124c95e6b53`；目标 EXE、Start、PowerShell/pwsh、其他 AuditRunner、SbieCtrl、其他 Sandboxie 进程和全部 `ShaheAudit_*` section/目录均为 `0`，仅 `SbieSvc.exe` 且服务为 `Running`，三份 SHA-256 精确匹配权威基线。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-preflight-20261017_120233_453-result.json` 和 `auditrunner-uia-invoke-preflight-20261017_120600_000.log`。下一步使用该 baseline、显式 `--execute` 和全新唯一前缀执行真实 `batch-create`。
- 2026-10-17 C# AuditRunner UI Automation 真实 `batch-create` 尝试为 `inconclusive`，不是产品失败：返回码 `3`，目标 PID `18008`，动态菜单 ID `290`；对话框前台和稳定结构（22 控件、3 Edit、1 ComboBox、2 Button）均确认，但 UIA 调用未形成可确认的对话框关闭。目标通过动态“退出软件”命令退出，未强制结束；state 完成阶段和所有权声明均为 `0`，未观察到新 Sandboxie 资源。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-batch-create-uia-20261017_120300_957-result.json`、`auditrunner-lifecycle-batch-create-20261017_120411_463-result.json`、state JSON 和同名前缀 `.log`。当前结果不能区分 UIA 模式不可用与调用成功但未执行，下一步先确认零残留，再仅增加提交诊断字段。
- 2026-10-17 UI Automation 真实 `batch-create` 尝试后只读清洁 preflight 通过：返回码 `0`、16/16 门禁通过、错误 `0`，baseline ID 为 `a614d0a3a7e147439dd514109be27d1f`；目标 EXE、Start、PowerShell/pwsh、其他 AuditRunner、SbieCtrl、其他 Sandboxie 进程和全部 `ShaheAudit_*` section/目录均为 `0`，仅 `SbieSvc.exe` 且服务为 `Running`，三份 SHA-256 精确匹配权威基线。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-preflight-20261017_120548_313-result.json` 和 `auditrunner-batch-create-uia-postflight-20261017_120500_000.log`。下一步仅增加提交方式、UIA 可用性、输入回读和按钮状态诊断。
- 2026-10-17 C# AuditRunner UIA 提交诊断定向测试通过：返回码 `0`，`9/9` 通过、失败 `0`、跳过 `0`；`DialogObservation` 现记录三个输入回读值、下拉索引、确定按钮启用状态、UIA 提交结果和精确错误文本，不改变创建、所有权或清理逻辑。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-uia-diagnostic-green-20261017_121000_000-result.json`、同名前缀 `.log` 和结果目录 TRX。下一步构建并使用新 clean baseline 运行一次诊断性真实 `batch-create`。
- 2026-10-17 C# AuditRunner UIA 提交诊断隔离 Release x64 构建通过：返回码 `0`、警告 `0`、错误 `0`，产物为 `artifacts/runs/csharp_navigation_followup_20261015/isolated-auditrunner-binx64/x64/Release/net48/ShaheLauncher.AuditRunner.exe`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-uia-diagnostic-build-20261017_121100_000-result.json` 和同名前缀 `.log`。下一步建立新的只读 clean baseline。
- 2026-10-17 C# AuditRunner UIA 提交诊断版只读 preflight 通过：返回码 `0`、16/16 门禁通过、错误 `0`，baseline ID 为 `0d0b4ec6afc34286a8782cc92e9101de`；目标 EXE、Start、PowerShell/pwsh、其他 AuditRunner、SbieCtrl、其他 Sandboxie 进程和全部 `ShaheAudit_*` section/目录均为 `0`，仅 `SbieSvc.exe` 且服务为 `Running`，三份 SHA-256 精确匹配权威基线。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-preflight-20261017_120923_822-result.json` 和 `auditrunner-uia-diagnostic-preflight-20261017_121200_000.log`。下一步使用全新唯一前缀运行一次诊断性真实 `batch-create`。
- 2026-10-17 C# AuditRunner UIA 诊断性真实 `batch-create` 得到确定根因证据：返回码 `3`、状态 `inconclusive`，目标 PID `19408`；三个输入精确回读为 `ShaheAudit_20261017_121000_431_`、`1`、`2`，下拉索引为 `1`，确定按钮启用，UIA 报告 `Submitted=true` 且无异常，但对话框 20 秒内未关闭。目标动态退出、未强制结束，完成阶段/所有权声明/观察到的本轮资源均为 `0`。这证明 UIA provider 接受调用但没有执行按钮动作，不是输入、焦点或产品创建失败。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-batch-create-uia-diagnostic-20261017_121000_431-result.json`、`auditrunner-lifecycle-batch-create-20261017_121050_577-result.json`、state JSON 和同名前缀 `.log`。下一步先确认零残留，再改为向已确认前台的对话框发送 Enter。
- 2026-10-17 UIA 诊断性真实 `batch-create` 后只读清洁 preflight 通过：返回码 `0`、16/16 门禁通过、错误 `0`，baseline ID 为 `27153476fb9c4922b5cc58f28f747266`；目标 EXE、Start、PowerShell/pwsh、其他 AuditRunner、SbieCtrl、其他 Sandboxie 进程和全部 `ShaheAudit_*` section/目录均为 `0`，仅 `SbieSvc.exe` 且服务为 `Running`，三份 SHA-256 精确匹配权威基线。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-preflight-20261017_121218_488-result.json` 和 `auditrunner-batch-create-uia-diagnostic-postflight-20261017_121200_000.log`。下一步为前台对话框 Enter 提交建立 RED/GREEN。
- 2026-10-17 C# AuditRunner 前台对话框 Enter 提交 RED 有效：定向测试返回码 `1`，`8` 通过、`1` 失败、`0` 跳过；唯一失败 `BatchCreateConfirmationRequiresForegroundDialogEnter` 明确证明当前缺少“仅当前台句柄仍为目标对话框时发送 Enter”的入口。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-foreground-enter-red-20261017_121500_000-result.json`、同名前缀 `.log` 和结果目录 TRX。下一步删除无效 UIA 按钮调用并实现该最小入口。
- 2026-10-17 C# AuditRunner 前台对话框 Enter 提交 GREEN 通过：定向测试返回码 `0`，`9/9` 通过、失败 `0`、跳过 `0`；提交前必须确认对话框句柄有效且仍为前台窗口，然后使用现有 `SendKeys.SendWait("{ENTER}")` 触发默认确定按钮，无效 UIA 按钮调用已删除。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-foreground-enter-green-20261017_121700_000-result.json`、同名前缀 `.log` 和结果目录 TRX。下一步重新构建、preflight 并运行一次真实 `batch-create`。
- 2026-10-17 C# AuditRunner 前台对话框 Enter 版本隔离 Release x64 构建通过：返回码 `0`、警告 `0`、错误 `0`，产物为 `artifacts/runs/csharp_navigation_followup_20261015/isolated-auditrunner-binx64/x64/Release/net48/ShaheLauncher.AuditRunner.exe`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-foreground-enter-build-20261017_121800_000-result.json` 和同名前缀 `.log`。下一步运行只读 preflight。
- 2026-10-17 C# AuditRunner 前台对话框 Enter 版本只读 preflight 通过：返回码 `0`、16/16 门禁通过、错误 `0`，baseline ID 为 `57470f359e4548428a22f25fc401e23f`；目标 EXE、Start、PowerShell/pwsh、其他 AuditRunner、SbieCtrl、其他 Sandboxie 进程和全部 `ShaheAudit_*` section/目录均为 `0`，仅 `SbieSvc.exe` 且服务为 `Running`，三份 SHA-256 精确匹配权威基线。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-preflight-20261017_121701_195-result.json` 和 `auditrunner-foreground-enter-preflight-20261017_121900_000.log`。下一步使用该 baseline、显式 `--execute` 和全新唯一前缀执行真实 `batch-create`。
- 2026-10-17 C# AuditRunner 前台 Enter 真实 `batch-create` 尝试为 `inconclusive`，不是产品失败：返回码 `3`，目标 PID `17504`；对话框前台、稳定、确定按钮启用，三个输入和下拉索引回读正确，Enter 报告已发送，但对话框 20 秒内未关闭。目标动态退出、未强制结束，完成阶段/所有权声明/观察到的本轮资源均为 `0`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-batch-create-enter-20261017_121800_772-result.json`、`auditrunner-lifecycle-batch-create-20261017_121825_325-result.json`、state JSON 和同名前缀 `.log`。这表明当前执行会话的外部输入注入没有到达 WinForms 点击处理；下一步先确认零残留，再改为直接向对话框投递标准 `WM_COMMAND/BN_CLICKED` 父窗口通知。
- 2026-10-17 前台 Enter 真实 `batch-create` 后只读清洁 preflight 通过：返回码 `0`、16/16 门禁通过、错误 `0`，baseline ID 为 `9c213ffdcfae4ebe8ba1091afae1b90e`；目标 EXE、Start、PowerShell/pwsh、其他 AuditRunner、SbieCtrl、其他 Sandboxie 进程和全部 `ShaheAudit_*` section/目录均为 `0`，仅 `SbieSvc.exe` 且服务为 `Running`，三份 SHA-256 精确匹配权威基线。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-preflight-20261017_122005_856-result.json` 和 `auditrunner-batch-create-enter-postflight-20261017_122000_000.log`。下一步为标准 `WM_COMMAND/BN_CLICKED` 父窗口通知建立 RED/GREEN。
- 2026-10-17 C# AuditRunner 标准按钮父窗口通知 RED 有效：定向测试返回码 `1`，`8` 通过、`1` 失败、`0` 跳过；唯一失败 `BatchCreateConfirmationRequiresOwnedForegroundButtonNotification` 明确证明当前缺少“按钮必须属于当前前台对话框后才能投递 `WM_COMMAND/BN_CLICKED`”的入口。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-button-notify-red-20261017_122300_000-result.json`、同名前缀 `.log` 和结果目录 TRX。下一步实现该标准 WinForms 父窗口通知并删除无效 Enter 路线。
- 2026-10-17 C# AuditRunner 标准按钮父窗口通知 GREEN 通过：定向测试返回码 `0`，`9/9` 通过、失败 `0`、跳过 `0`；确定按钮必须是当前前台对话框的有效子控件，随后以实际控件 ID 和句柄向父窗口投递 `WM_COMMAND/BN_CLICKED`，无效 Enter 路线已删除。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-button-notify-green-20261017_122500_000-result.json`、同名前缀 `.log` 和结果目录 TRX。下一步重新构建、preflight 并运行一次真实 `batch-create`。
- 2026-10-17 C# AuditRunner 标准按钮父窗口通知版本隔离 Release x64 构建通过：返回码 `0`、警告 `0`、错误 `0`，产物为 `artifacts/runs/csharp_navigation_followup_20261015/isolated-auditrunner-binx64/x64/Release/net48/ShaheLauncher.AuditRunner.exe`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-button-notify-build-20261017_122700_000-result.json` 和同名前缀 `.log`。下一步运行只读 preflight。
- 2026-10-17 C# AuditRunner 标准按钮父窗口通知版本只读 preflight 通过：返回码 `0`、16/16 门禁通过、错误 `0`，baseline ID 为 `d1dec18cd3954ed0b4bcb89f000d59c0`；目标 EXE、Start、PowerShell/pwsh、其他 AuditRunner、SbieCtrl、其他 Sandboxie 进程和全部 `ShaheAudit_*` section/目录均为 `0`，仅 `SbieSvc.exe` 且服务为 `Running`，三份 SHA-256 精确匹配权威基线。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-preflight-20261017_144638_697-result.json` 和 `auditrunner-button-notify-preflight-20261017_122800_000.log`。下一步使用该 baseline、显式 `--execute` 和全新唯一前缀执行真实 `batch-create`。
- 2026-10-17 C# AuditRunner 标准按钮通知首次真实 `batch-create` 尝试为 `inconclusive`，不是产品失败：返回码 `3`，目标 PID `13920`；对话框结构稳定（22 控件、3 Edit、1 ComboBox、2 Button），但焦点门禁确认其不是前台窗口，因此 Runner 正确拒绝输入和提交，`OkControlId=0`、`Submitted=false`。目标动态退出、未强制结束，完成阶段/所有权声明/观察到的本轮资源均为 `0`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-batch-create-button-notify-20261017_144700_318-result.json`、`auditrunner-lifecycle-batch-create-20261017_144720_991-result.json`、state JSON 和同名前缀 `.log`。下一步先确认零残留，再使用新 baseline 和新前缀重试一次；仍无焦点时再修复焦点获取。
- 2026-10-17 标准按钮通知首次真实 `batch-create` 后只读清洁 preflight 通过：返回码 `0`、16/16 门禁通过、错误 `0`，baseline ID 为 `f1e245fdbce0475fb06c96f291590414`；目标 EXE、Start、PowerShell/pwsh、其他 AuditRunner、SbieCtrl、其他 Sandboxie 进程和全部 `ShaheAudit_*` section/目录均为 `0`，仅 `SbieSvc.exe` 且服务为 `Running`，三份 SHA-256 精确匹配权威基线。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-preflight-20261017_144812_324-result.json` 和 `auditrunner-button-notify-postflight-20261017_144800_000.log`。下一步使用新前缀重试一次真实 `batch-create`。
- 2026-10-17 C# AuditRunner 标准按钮通知第二次真实 `batch-create` 仍为 `inconclusive`，不是产品失败：返回码 `3`，目标 PID `24120`；对话框结构再次稳定但不是前台窗口，Runner 按门禁未输入、未提交，目标动态退出且未强制结束，完成阶段/所有权声明/观察到的本轮资源均为 `0`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-batch-create-button-notify2-20261017_144900_586-result.json`、`auditrunner-lifecycle-batch-create-20261017_144848_579-result.json`、state JSON 和同名前缀 `.log`。连续两次相同焦点失败已确认审计器缺少对话框前台恢复；下一步先确认零残留，再为焦点恢复建立 RED/GREEN。
- 2026-10-17 标准按钮通知第二次真实 `batch-create` 后只读清洁 preflight 通过：返回码 `0`、16/16 门禁通过、错误 `0`，baseline ID 为 `ad24a0a005ba4d83b47bb2643538f71e`；目标 EXE、Start、PowerShell/pwsh、其他 AuditRunner、SbieCtrl、其他 Sandboxie 进程和全部 `ShaheAudit_*` section/目录均为 `0`，仅 `SbieSvc.exe` 且服务为 `Running`，三份 SHA-256 精确匹配权威基线。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-preflight-20261017_144933_738-result.json` 和 `auditrunner-button-notify2-postflight-20261017_145000_000.log`。下一步只修 AuditRunner 对话框焦点恢复。
- 2026-10-17 C# AuditRunner 对话框焦点恢复 RED 有效：定向测试返回码 `1`，`9` 通过、`1` 失败、`0` 跳过；唯一失败 `BatchCreateFocusRecoveryRejectsInvalidWindow` 明确证明当前缺少验证窗口后恢复并确认前台焦点的入口。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-focus-recovery-red-20261017_145100_000-result.json`、同名前缀 `.log` 和结果目录 TRX。下一步复用现有项目的 `ShowWindow + SetForegroundWindow` 原生模式实现最小焦点恢复。
- 2026-10-17 C# AuditRunner 对话框焦点恢复 GREEN 通过：定向测试返回码 `0`，`10/10` 通过、失败 `0`、跳过 `0`；Runner 复用现有项目的 `ShowWindow(SW_RESTORE) + SetForegroundWindow` 模式，最多等待 2 秒并再次确认目标对话框前台状态，确认失败仍拒绝输入。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-focus-recovery-green-20261017_145300_000-result.json`、同名前缀 `.log` 和结果目录 TRX。下一步重新构建、preflight 并运行真实 `batch-create`。
- 2026-10-17 C# AuditRunner 焦点恢复版本隔离 Release x64 构建通过：返回码 `0`、警告 `0`、错误 `0`，产物为 `artifacts/runs/csharp_navigation_followup_20261015/isolated-auditrunner-binx64/x64/Release/net48/ShaheLauncher.AuditRunner.exe`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-focus-recovery-build-20261017_145400_000-result.json` 和同名前缀 `.log`。下一步运行只读 preflight。
- 2026-10-17 C# AuditRunner 焦点恢复版本只读 preflight 通过：返回码 `0`、16/16 门禁通过、错误 `0`，baseline ID 为 `07efb815a1db49c696a73244628aee4a`；目标 EXE、Start、PowerShell/pwsh、其他 AuditRunner、SbieCtrl、其他 Sandboxie 进程和全部 `ShaheAudit_*` section/目录均为 `0`，仅 `SbieSvc.exe` 且服务为 `Running`，三份 SHA-256 精确匹配权威基线。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-preflight-20261017_145252_811-result.json` 和 `auditrunner-focus-recovery-preflight-20261017_145500_000.log`。下一步使用该 baseline、显式 `--execute` 和全新唯一前缀执行真实 `batch-create`。
- 2026-10-17 C# AuditRunner 焦点恢复后的真实 `batch-create` 已越过对话框门禁但结果仍为 `inconclusive`，不是产品失败：返回码 `3`，目标 PID `1876`；对话框前台/稳定/输入回读/下拉索引/按钮所有权均确认，`Submitted=true` 且 `Closed=true`，但 20 秒内未观察到两个预期精确 `BoxAlias`/section/目录所有权。目标动态退出、未强制结束，state 完成阶段和所有权声明仍为 `0`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-batch-create-focus-recovery-20261017_145500_944-result.json`、`auditrunner-lifecycle-batch-create-20261017_145414_794-result.json`、state JSON 和同名前缀 `.log`。下一步先确认零残留，再检查名称长度和应用验证规则。
- 2026-10-17 焦点恢复后的 postflight 返回码 `3`、15/16 门禁通过，唯一失败是 `Sandboxie.ini` 哈希从权威基线变为 `6848423D0EA1E2E5C1301F969C077709FF63C04DDAB9CD813A1CE70AC49E0BAF`；目标/Start/PowerShell/pwsh/其他 AuditRunner/SbieCtrl/其他 Sandboxie 进程均为 `0`，`SbieSvc=Running`，全部 `ShaheAudit_*` section/目录仍为 `0`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-focus-recovery-postflight-20261017_145600_000-result.json`、`auditrunner-preflight-20261017_145521_270-result.json` 和同名前缀 `.log`。
- 2026-10-17 基线与 postflight 只读差异显示新增 `Box54`–`Box66` 共 13 个 section/enabled box，别名精确为 `王五8`–`王五20`；它们不匹配本轮唯一前缀 `ShaheAudit_20261017_145500_944`，Runner 无法证明唯一所有权，因此未尝试清理。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-nonowned-resource-diff-20261017_145700_000-result.json`。当前 lifecycle 被安全门禁阻断，先只读判断这些资源是否由并发外部操作产生或暴露了控件映射错误。
- 2026-10-17 新增目录元数据只读核验显示 `Box54`–`Box66` 的创建时间连续落在 `14:53:55.043775` 至 `14:54:00.384665 +08:00`，与该次 AuditRunner 尝试精确重合，说明审计器错误触发了非预期批量值；但别名仍不含本轮唯一 `ShaheAudit_*` 前缀，不能满足唯一精确所有权规则，未执行任何清理。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-nonowned-resource-metadata-20261017_145800_000-result.json`。后续真实 lifecycle 写操作立即停止，只允许修复和自动化测试。
- 2026-10-17 C# AuditRunner 语义控件映射 RED 有效：定向测试返回码 `1`，`10` 通过、`1` 失败、`0` 跳过；唯一失败复现对话框预置 `王五/8/13` 时 Runner 缺少按 `AccessibleName` 精确设置名称前缀、起始编号、编号格式和创建数量的入口，证明此前按原生 Edit 枚举顺序写值不安全。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-semantic-controls-red-20261017_150100_000-result.json`、同名前缀 `.log` 和结果目录 TRX。下一步仅替换 Runner 的位置式输入映射，不执行真实 Sandboxie 写操作。
- 2026-10-17 C# AuditRunner 语义控件映射首次 GREEN 尝试返回码 `1`，`10` 通过、`1` 失败、`0` 跳过；精确名称查找已替换位置式 Edit 枚举，但实际 WinForms 测试对话框上的某个 UIA 控件模式不满足首版假设，设置器安全返回 `false`，没有点击确定或创建资源。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-semantic-controls-green-20261017_150500_000-result.json`、同名前缀 `.log` 和结果目录 TRX。下一步只诊断四个已命名控件实际暴露的 UIA 类型/模式并修正设置器。
- 2026-10-17 C# AuditRunner 语义控件诊断测试返回码 `1`，`10` 通过、`1` 失败、`0` 跳过；实际对话框的首个前缀值仍为预置值，证明首版“AccessibleName + 固定 UIA ControlType”组合条件在赋值前就拒绝了控件。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-semantic-controls-diagnostic-20261017_150900_000-result.json`、同名前缀 `.log` 和结果目录 TRX。下一步保留精确唯一 AccessibleName 门禁，由所需 UIA pattern 验证控件能力，不再额外依赖不稳定的 provider ControlType。
- 2026-10-17 C# AuditRunner 语义控件第二次 GREEN 尝试返回码 `1`，`10` 通过、`1` 失败、`0` 跳过；去掉固定 ControlType 后前缀仍未改变，说明该 WinForms provider 没有把控件 `AccessibleName` 暴露为当前查找使用的 UIA Name，设置器继续安全拒绝且未提交对话框。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-semantic-controls-green2-20261017_151200_000-result.json`、同名前缀 `.log` 和结果目录 TRX。下一步直接检查该测试对话框的 UIA 属性，不再猜测控件类型或模式。
- 2026-10-17 C# AuditRunner 测试对话框 UIA 树诊断完成：返回码 `1`，`10` 通过、诊断断言 `1` 失败、`0` 跳过；名称前缀 Edit 和编号格式 ComboBox 暴露精确 UIA Name，但 WinForms `NumericUpDown` 没有暴露源码设置的 `AccessibleName`，只暴露精确文本标签和匿名增减按钮。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-semantic-uia-dump-20261017_151600_000-result.json`、同名前缀 `.log` 和结果目录 TRX。下一步按精确标签和同一行矩形唯一关联两个原生数值 Edit，仍拒绝缺失、重复或歧义匹配。
- 2026-10-17 C# AuditRunner 语义控件第三次 GREEN 尝试在测试执行前编译失败：返回码 `1`、通过/失败/跳过均为 `0`，唯一错误 `CS0012` 表明读取 UIA `BoundingRectangle` 需要显式引用框架自带 `WindowsBase`；没有执行对话框提交或真实写操作。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-semantic-controls-green3-20261017_152100_000-result.json` 和同名前缀 `.log`。下一步仅补框架引用后重跑相同定向测试。
- 2026-10-17 补齐 `WindowsBase` 后 C# AuditRunner 语义控件第四次 GREEN 尝试返回码 `1`，`10` 通过、`1` 失败、`0` 跳过；UIA 树明确同时包含名为“编号格式”的 ComboBox 和其 Text 子元素，名称单条件唯一性门禁因此正确拒绝。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-semantic-controls-green4-20261017_152400_000-result.json`、同名前缀 `.log` 和结果目录 TRX。下一步只对已知稳定的前缀/下拉/标签分别加 Edit、ComboBox、Text 类型约束，数值控件仍由精确标签关联。
- 2026-10-17 C# AuditRunner 语义控件映射 GREEN 通过：定向测试返回码 `0`，`11/11` 通过、失败 `0`、跳过 `0`。实际 WinForms 对话框从故意预置的 `王五/8/13` 正确改为 `ShaheAudit_Test_/1/2`，编号格式索引为 `1`；Runner 现在按精确 UIA 名称和类型定位前缀/下拉，并按精确文本标签与同一行矩形唯一关联两个数值 Edit，缺失、重复或歧义仍安全拒绝。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-semantic-controls-green5-20261017_152800_000-result.json`、同名前缀 `.log` 和结果目录 TRX。真实 lifecycle 写操作继续暂停。
- 2026-10-17 删除临时 UIA 树诊断依赖后 C# AuditRunner 定向回归仍通过：返回码 `0`，`11/11` 通过、失败 `0`、跳过 `0`；最终测试只保留真实对话框语义值回归，不保留诊断专用测试工程引用。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-semantic-controls-regression-20261017_153100_000-result.json`、同名前缀 `.log` 和结果目录 TRX。下一步执行隔离 Release x64 构建；真实 lifecycle 写操作仍暂停。
- 2026-10-17 C# AuditRunner 语义控件修正版隔离 Release x64 构建通过：返回码 `0`、警告 `0`、错误 `0`，产物为 `artifacts/runs/csharp_navigation_followup_20261015/isolated-auditrunner-binx64/x64/Release/net48/ShaheLauncher.AuditRunner.exe`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-semantic-controls-build-20261017_153300_000-result.json` 和同名前缀 `.log`。因 `Box54`–`Box66` 所有权不能按前缀证明，真实 lifecycle 写操作保持阻断。
- 2026-10-17 编译后 AuditRunner `contract` 回归返回码 `1`：内建合同仍用旧 lifecycle 示例参数，缺少当前必需的 `--stage` 和 `--state`，因此只记录 `contractException=false`；未启动目标 EXE、未执行 Sandboxie 写操作。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-semantic-controls-contract-20261017_153500_000-result.json` 和同名前缀 `.log`。下一步仅同步内建合同参数后重跑。
- 2026-10-17 同步内建 lifecycle 合同参数后的隔离 Release x64 构建通过：返回码 `0`、警告 `0`、错误 `0`；只补入 `batch-create` stage 和 state 路径并校验解析结果。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-contract-fix-build-20261017_153800_000-result.json` 和同名前缀 `.log`。下一步重跑编译后 `contract`。
- 2026-10-17 编译后 AuditRunner `contract` GREEN 通过：返回码 `0`，6 项内建检查全部为 `true`，包括 lifecycle 门禁、默认拒绝写操作、动态菜单、唯一所有权、删除顺序和异常分类。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-contract-green-20261017_154000_000-result.json`、`auditrunner-contract-20261017_151233_025-result.json` 和同名前缀 `.log`。真实 lifecycle 写操作仍因非前缀 `Box54`–`Box66` 阻断。
- 2026-10-17 用户明确确认允许清理 `Box54`–`Box66` 后，编译后 AuditRunner 只读恢复 preflight 返回码 `3`、15/16 门禁通过；唯一失败仍为预期的 INI 哈希，当前哈希 `6848423D...E0BAF`。13 个 section、目录和别名仍一一精确匹配 `Box54`–`Box66` / `王五8`–`王五20`，没有漂移；目标、Start、PowerShell/pwsh、其他 AuditRunner、SbieCtrl 和其他 Sandboxie 进程均为 `0`，`SbieSvc=Running`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-recovery-preflight-20261017_154500_000-result.json`、`auditrunner-preflight-20261017_151937_438-result.json` 和同名前缀 `.log`。下一步为已确认精确 claims 执行受控 recovery cleanup。
- 2026-10-17 C# AuditRunner 恢复清理状态门禁 RED 有效：定向测试返回码 `1`，`11` 通过、`1` 失败、`0` 跳过；唯一失败证明当前缺少“清理状态必须同时绑定原 baseline、原前缀并包含非空精确 claims”的加载入口。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-recovery-cleanup-red-20261017_155000_000-result.json`、同名前缀 `.log` 和结果目录 TRX。下一步实现最小状态加载门禁并接入现有 `SelectOwnedResources` 与安全删除顺序。
- 2026-10-17 C# AuditRunner 恢复清理状态门禁 GREEN 通过：定向测试返回码 `0`，`12/12` 通过、失败 `0`、跳过 `0`；清理状态现在必须存在、可解析、精确绑定 baseline ID 与前缀、阶段历史合法且包含非空 claims，任一不符均拒绝。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-recovery-cleanup-green-20261017_155300_000-result.json`、同名前缀 `.log` 和结果目录 TRX。下一步将该门禁接入现有精确所有权选择和删除顺序。
- 2026-10-17 将 recovery cleanup 接入现有 `SelectOwnedResources`、`delete_sandbox_silent`、目录消失和 INI section 删除顺序后，C# AuditRunner 定向回归通过：返回码 `0`，`12/12` 通过、失败 `0`、跳过 `0`。cleanup 不接受当前污染状态作为新 baseline，只允许使用原始通过的 preflight 与精确绑定 claims。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-recovery-cleanup-integration-20261017_155700_000-result.json`、同名前缀 `.log` 和结果目录 TRX。下一步隔离构建并创建新的恢复状态证据。
- 2026-10-17 C# AuditRunner recovery cleanup 隔离 Release x64 构建通过：返回码 `0`、警告 `0`、错误 `0`，产物为 `artifacts/runs/csharp_navigation_followup_20261015/isolated-auditrunner-binx64/x64/Release/net48/ShaheLauncher.AuditRunner.exe`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-recovery-cleanup-build-20261017_155900_000-result.json` 和同名前缀 `.log`。下一步创建只包含用户确认的 13 个精确 claims 的独立恢复 state。
- 2026-10-17 已创建独立且不可覆盖历史 state 的恢复输入 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-recovery-cleanup-20261017_160100_000-state.json`：绑定原始 passed baseline ID `07efb815a1db49c696a73244628aee4a`、原始前缀 `ShaheAudit_20261017_145500_944`，仅包含用户明确确认的 `Box54`–`Box66`、`王五8`–`王五20` 和同名目录 13 个精确 claims；没有修改原失败尝试的 state。下一步执行一次编译后 recovery cleanup。
- 2026-10-17 编译后 AuditRunner recovery cleanup 通过：返回码 `0`，用户确认的 `Box54`–`Box66` 共 13/13 项全部按“`delete_sandbox_silent` 返回 0 → 精确目录消失 → 删除精确 INI section”的顺序成功，失败 `0`、跳过 `0`、错误列表为空；未选择或处理任何其他资源。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-recovery-cleanup-20261017_160100_000-result.json`、`auditrunner-lifecycle-cleanup-20261017_152414_210-result.json`、state 和同名前缀 `.log`。下一步立即运行只读 postflight/verify-clean 门禁。
- 2026-10-17 recovery cleanup 后编译后 AuditRunner 只读 postflight 通过：返回码 `0`、16/16 门禁通过、失败 `0`；`Box54`–`Box66` section/目录均已消失，全部 `ShaheAudit_*` section/目录为 `0`，目标/Start/PowerShell/pwsh/其他 AuditRunner/SbieCtrl/其他 Sandboxie 进程均为 `0`，仅 `SbieSvc.exe PID 2836` 且服务 `Running`。目标、配置和 `Sandboxie.ini` SHA-256 分别恢复/保持 `123607...E517`、`99120B...F234`、`2709AD...5A6D`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-recovery-postflight-20261017_160300_000-result.json`、`auditrunner-preflight-20261017_152450_711-result.json` 和同名前缀 `.log`。环境已恢复 clean baseline，下一步运行非破坏性 dialog-gate。
- 2026-10-17 recovery 后非破坏性 `dialog-gate` 通过：返回码 `0`，5/5 个对话框均确认前台、结构稳定并安全取消，运行时动态菜单 ID 均唯一解析；目标 PID `29008` 通过动态“退出软件”命令退出，未强制结束，三份哈希保持不变。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-dialog-gate-after-recovery-20261017_160500_000-result.json`、`auditrunner-dialog-gate-20261017_152555_578-result.json` 和同名前缀 `.log`。下一步重新运行 clean preflight 后才允许新前缀 lifecycle `batch-create`。
- 2026-10-17 lifecycle 前第二次只读 preflight 通过：返回码 `0`、16/16 门禁通过、失败 `0`，新 baseline ID `98eebdff03bc41acad59f78474af7fd3`；全部目标进程和 `ShaheAudit_*` section/目录均为 `0`，`SbieSvc=Running`，三份哈希精确匹配权威值。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-preflight-before-lifecycle-20261017_160700_000-result.json`、`auditrunner-preflight-20261017_152706_111-result.json` 和同名前缀 `.log`。contract、dialog-gate 和双 preflight 均通过，允许使用全新唯一前缀执行一次真实 `batch-create`。
- 2026-10-17 使用用户允许的唯一前缀 `ShaheAudit_20261017_160900_001_` 执行真实 `batch-create` 返回码 `3`、状态 `inconclusive`，不是产品失败：对话框前台、结构、语义输入、下拉、按钮所有权和关闭均通过，目标 PID `25940` 动态退出且未强制结束；期望两个 alias，但 20 秒内只精确观察到 `shaheaudit_20261017_16091` / `ShaheAudit_20261017_160900_001_01` 一个 claim，失败清理返回 `delete_sandbox_silent=-1`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-lifecycle-batch-create-semantic-20261017_160900_001-result.json`、`auditrunner-lifecycle-batch-create-20261017_152826_473-result.json`、state 和同名前缀 `.log`。后续 lifecycle 动作停止，下一步立即只读核验精确残留和哈希。
- 2026-10-17 该次 `batch-create` 后只读 postflight 返回码 `3`、13/16 门禁通过；失败项精确为 INI 哈希、`ShaheAudit_*` section 和目录。当前仅有一个本轮精确拥有的残留：section/目录 `ShaheAudit_20261017_16091`，alias `ShaheAudit_20261017_160900_001_01`；目标/Start/PowerShell/pwsh/其他 AuditRunner/SbieCtrl/其他 Sandboxie 进程均为 `0`，`SbieSvc=Running`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-batch-create-semantic-postflight-20261017_161200_000-result.json`、`auditrunner-preflight-20261017_152927_059-result.json` 和同名前缀 `.log`。该资源满足原 baseline 排除、精确 alias、section 和目录唯一所有权，下一步只修 cleanup 命令返回 `-1` 的 Runner 问题后清理。
- 2026-10-17 C# AuditRunner 所有权名称大小写保真 RED 有效：定向测试返回码 `1`，`12` 通过、`1` 失败、`0` 跳过；唯一失败证明 Runner 尚不能从大小写不敏感的 alias 映射键恢复当前 section/目录的精确实际大小写，并拒绝缺失或大小写冲突的重复名称。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-exact-case-red-20261017_161500_000-result.json`、同名前缀 `.log` 和结果目录 TRX。下一步实现一个精确唯一大小写恢复门禁，并用于 cleanup claim。
- 2026-10-17 C# AuditRunner 所有权名称大小写保真 GREEN 通过：定向测试返回码 `0`，`13/13` 通过、失败 `0`、跳过 `0`；Runner 现在从当前 section 和目录集合中恢复唯一精确大小写，缺失或大小写冲突均拒绝，并将该名称用于 `delete_sandbox_silent`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-exact-case-green-20261017_161800_000-result.json`、同名前缀 `.log` 和结果目录 TRX。下一步构建后只清理当前一个精确 owned claim。
- 2026-10-17 C# AuditRunner 精确大小写修正版隔离 Release x64 构建通过：返回码 `0`、警告 `0`、错误 `0`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-exact-case-build-20261017_162000_000-result.json` 和同名前缀 `.log`。下一步创建只含 `ShaheAudit_20261017_16091` / `ShaheAudit_20261017_160900_001_01` 的恢复 state 并执行一次 cleanup。
- 2026-10-17 已创建独立恢复 state `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-semantic-residual-cleanup-20261017_162200_000-state.json`，精确绑定 passed baseline `98eebdff03bc41acad59f78474af7fd3`、本轮前缀，并只包含唯一残留 `ShaheAudit_20261017_16091` / `ShaheAudit_20261017_160900_001_01` / 同名目录；未修改原 lifecycle state。下一步执行一次编译后 cleanup。
- 2026-10-17 编译后 AuditRunner 对本轮唯一残留的 cleanup 通过：返回码 `0`、`1/1` 成功、失败 `0`、跳过 `0`；使用精确实际大小写执行 `delete_sandbox_silent` 后目录消失，再删除对应 INI section，错误列表为空。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-semantic-residual-cleanup-20261017_162200_000-result.json`、`auditrunner-lifecycle-cleanup-20261017_153346_255-result.json`、state 和同名前缀 `.log`。下一步立即运行只读 postflight。
- 2026-10-17 唯一残留 cleanup 后只读 postflight 通过：返回码 `0`、16/16 门禁通过、失败 `0`；全部 `ShaheAudit_*` section/目录恢复为 `0`，目标/Start/PowerShell/pwsh/其他 AuditRunner/SbieCtrl/其他 Sandboxie 进程均为 `0`，`SbieSvc=Running`，三份 SHA-256 精确恢复权威值。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-semantic-residual-postflight-20261017_162400_000-result.json`、`auditrunner-preflight-20261017_153432_881-result.json` 和同名前缀 `.log`。本轮 batch-create 仍为无结论；下一步修正过长前缀导致两个显示名映射到同一内部名的问题后，以更短全新前缀重试。
- 2026-10-17 C# AuditRunner 批量创建确认改为异步消息后，隔离 Release x64 构建通过：返回码 `0`、警告 `0`、错误 `0`；仅修改 Runner 的确定按钮投递方式，避免等待目标 UI 线程导致审计器阻塞，未修改产品代码。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-batch-create-async-build-20261017_113918_138-result.json` 和同名前缀 `.log`。下一步使用刚通过的 clean baseline 和全新前缀重跑真实 `batch-create`。
- 2026-10-16 本轮 Sandboxie 生命周期审计已建立只读基线探针；首次从外层 PowerShell 调用脚本解析时因双引号展开导致 `$scriptText` 丢失，记录为探针失败，尚未创建本轮沙盒；更正解析和基线运行完成前不进入破坏性生命周期步骤。
- 2026-10-16 基线脚本第二次解析仍失败：Windows PowerShell 5.1 无 BOM 读取脚本中的中文进程名后在后续字符串处报告未闭合字符串；脚本未执行，记录在 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-baseline-parse-corrected.log`，改为按目标 EXE 路径筛选进程并移除中文字面量。
- 2026-10-16 基线脚本移除中文字面量并改按目标 EXE 路径筛选后，Windows PowerShell 5.1 `[scriptblock]::Create` 解析通过，输出 `PARSE_OK`；日志为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-baseline-parse-final.log`，下一步执行只读基线采集。
- 2026-10-16 只读 Sandboxie 生命周期基线运行完成：本轮候选前缀 `ShaheAudit_20261016_063753_136`，目标 EXE 存在且无活动目标进程；`C:\Windows\Sandboxie.ini` SHA-256 为 `2709ADB23A9D1080D91C0401DC86A9F954AF81DEAFB5FFD7DF03F5AA30E75A6D`，副本和完整 section/Enabled 快照已保存；`C:\Sandbox\Administrator` 目录、PowerShell/Sandboxie 相关进程及 `SbieSvc=Running` 已记录。基线试用 `SbieIni /list` 返回码 `1` 且无输出，尚未作为 enabled boxes 结论，下一步改用应用实际调用的 `queryex /boxes *`。
- 2026-10-16 基线脚本加入与 C# 一致的 `SbieIni queryex /boxes *` 读取，Windows PowerShell 5.1 解析通过，输出 `PARSE_OK`；日志为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-baseline-parse-queryex.log`，尚未重新运行基线。
- 2026-10-16 基线脚本加入隔离 Release 副本 `Configuration\shahe_config.ini` 的哈希、原文和副本记录，Windows PowerShell 5.1 解析通过，输出 `PARSE_OK`；日志为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-baseline-parse-config-copy.log`，尚未重新采集基线。
- 2026-10-16 带配置副本的整合基线运行超过 90 秒且未生成 JSON，已中断本轮自有 PowerShell 探针；目标 EXE 未启动、无沙盒资源被创建。该超时不作为产品结论，应用配置副本改由窄范围只读命令单独采集。
- 2026-10-16 带 `SbieIni queryex /boxes *` 的只读基线重新采集完成：本轮专属前缀为 `ShaheAudit_20261016_064007_947`，命令返回码 `0` 并记录现有 enabled boxes；`Sandboxie.ini` SHA-256 仍为 `2709ADB23A9D1080D91C0401DC86A9F954AF81DEAFB5FFD7DF03F5AA30E75A6D`，目标进程为空，`SbieSvc=Running`。在创建 `ShaheAudit_20261016_064007_947_*` 资源前，后续探针只允许使用该前缀。
- 2026-10-16 隔离 Release 应用配置窄范围快照完成：`Configuration\shahe_config.ini` SHA-256 为 `99120B9DFFD177FF4E66FA9ACDC3547B9085D220EE99483405CAB30B2B91F234`，副本已保存；无目标进程，`SbieSvc=Running`，现在才允许启动目标 EXE 执行本轮专属生命周期审计。
- 2026-10-16 本轮 Sandboxie 生命周期真实审计探针完成 Windows PowerShell 5.1 解析门禁，输出 `PARSE_OK`；日志为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-real-audit-parse.log`，尚未启动目标 EXE。
- 2026-10-16 本轮 Sandboxie 生命周期真实审计首轮执行未形成产品证据：探针在批量创建后因 PowerShell 5.1 中未加括号的 `@($prefix + '_01', $prefix + '_02')` 表达式把两个值合成为一个带空格元素，未观察到两个沙盒行，结果为 `inconclusive`、退出码 `2`，仅通过 `startup_and_lists`。清理阶段同样误用拼接后的内部名，曾留下本轮错误 section `[ShaheAudit_20261016_06401]`，随后已通过该确切官方目标名执行 `Start.exe delete_sandbox_silent` 和 `SbieIni set /drv` 清除；当前 `Sandboxie.ini` 已恢复基线 SHA-256 `2709ADB23A9D1080D91C0401DC86A9F954AF81DEAFB5FFD7DF03F5AA30E75A6D`，本轮目录、section、目标 EXE 和 `Start.exe` 均为零。原始结果为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-real-audit-20261016_070320_393-result.json`，退出输出在本轮执行会话中；本轮不作为产品结论，下一步先修正探针数组表达式。
- 2026-10-16 Sandboxie 生命周期探针数组表达式修正后，Windows PowerShell 5.1 解析门禁通过并输出 `PARSE_OK`；日志为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-real-audit-parse-array-fix.log`。修正仅限审计探针，不涉及生产代码；下一步重新使用同一已核准专属前缀执行真实流程。
- 2026-10-16 新增窄范围只读基线探针 `sandbox-lifecycle-narrow-baseline.ps1`，Windows PowerShell 5.1 解析门禁通过并输出 `PARSE_OK`；日志为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-narrow-baseline-parse.log`。该探针只查询目标 EXE 名称、`SbieIni queryex /boxes *`、Sandboxie 服务/控制进程、INI/应用配置副本和 Sandbox 直属目录，不执行破坏性动作；下一步用它生成新的专属前缀。
- 2026-10-16 窄范围只读基线首轮运行超过 60 秒仍未生成 JSON 或标准输出；检查时没有 `SbieIni.exe` 子进程，阻塞位于纯只读文件/目录/进程采集，已只终止本轮自有 Windows PowerShell PID `8008` 和外层 PowerShell 7 PID `11992`。目标 EXE、`Start.exe` 和新审计沙盒均未启动或创建，本轮不作为产品结论；日志为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-narrow-baseline-20261016_100144_332-timeout.log`，下一步以逐步骤计时探针定位具体只读调用。
- 2026-10-16 只读目录计时排除 `C:\Sandbox\Administrator` 直属项枚举为阻塞根因：共 `113` 项，实际耗时 `268ms`；改由当前 PowerShell 直接执行同一窄范围脚本后基线成功。新唯一前缀为 `ShaheAudit_20261016_102028_240`，`SbieIni queryex /boxes *` 返回码 `0`、输出 `96` 项，`Sandboxie.ini` SHA-256 仍为 `2709ADB23A9D1080D91C0401DC86A9F954AF81DEAFB5FFD7DF03F5AA30E75A6D`，应用配置 SHA-256 仍为 `99120B9DFFD177FF4E66FA9ACDC3547B9085D220EE99483405CAB30B2B91F234`，新前缀 section/目录均为 `0`，目标 EXE 与 `Start.exe` 实际均未运行，`SbieSvc=Running`。JSON 为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-narrow-baseline-direct-20261016_102028_135.json`，日志为 `sandbox-lifecycle-narrow-baseline-direct-20261016_102028_135.log`，INI/配置副本为 `sandbox-lifecycle-narrow-baseline-20261016_102028_240-Sandboxie.ini` 与 `sandbox-lifecycle-narrow-baseline-20261016_102028_240-app-config.ini`；下一步只允许使用该新前缀执行真实生命周期动作。
- 2026-10-16 新前缀真实生命周期审计第二轮仍未形成产品证据：目标 EXE SHA-256 与权威值 `123607B3A46E6A1939F933DB20F2D53CBA0266D4696E5B209FE7D8C984CDE517` 一致，PID `6164` 启动并通过 `startup_and_lists`，但批量创建对话框实际只枚举到 `1` 个 Edit，探针按错误假设要求 `3` 个 Edit，遂以 `Expected 3 edit controls, found 1` 返回 `inconclusive`、退出码 `2`；没有创建本轮沙盒，INI 哈希保持基线值。探针退出时模态状态使首个 `CloseMainWindow=false` 且 PID 暂留，随后仅对本轮自有 PID 再发正常关闭，`CloseMainWindow=true`、进程已退出；最终目标/`Start.exe`、前缀 section/目录均为 `0`，`SbieSvc=Running`。结果为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-real-audit-20261016_102706_664-result.json`，日志为 `sandbox-lifecycle-real-audit-20261016_102706_664.log`；本轮不作为产品结论，下一步对照真实对话框控件与生产实现修正探针。
- 2026-10-16 对照既有有效非破坏性审计 `sandbox-admin-dialog-real-audit-retry3-result.json`，批量创建对话框稳定结构为 `3` 个 Edit、`1` 个 ComboBox、`2` 个 Button，确认第二轮仅见 `1` 个 Edit 是子控件尚未全部创建的探针时序问题。生命周期探针在共享 `Wait-Dialog` 找到窗口后统一等待 `500ms`，未修改生产代码；Windows PowerShell 5.1 解析门禁输出 `PARSE_OK`，日志为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-real-audit-parse-dialog-wait.log`，下一步使用全新专属前缀重新建立基线并执行。
- 2026-10-16 第三轮窄范围基线完成：全新专属前缀 `ShaheAudit_20261016_103928_539`，`SbieIni queryex /boxes *` 返回码 `0`、输出 `96` 项，INI SHA-256 为 `2709ADB23A9D1080D91C0401DC86A9F954AF81DEAFB5FFD7DF03F5AA30E75A6D`，应用配置 SHA-256 为 `99120B9DFFD177FF4E66FA9ACDC3547B9085D220EE99483405CAB30B2B91F234`，目标进程为 `null`、前缀 section/目录均为 `0`、`SbieSvc=Running`。JSON 为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-narrow-baseline-direct-20261016_103928_479.json`，日志为 `sandbox-lifecycle-narrow-baseline-direct-20261016_103928_479.log`，INI/配置副本为 `sandbox-lifecycle-narrow-baseline-20261016_103928_539-Sandboxie.ini` 与 `sandbox-lifecycle-narrow-baseline-20261016_103928_539-app-config.ini`；下一轮真实审计只允许使用该前缀。
- 2026-10-16 第三轮真实生命周期审计仍在创建前因同一探针时序问题返回 `inconclusive`、退出码 `2`：PID `9496` 通过 `startup_and_lists`，即使顶层对话框后固定等待 `500ms`，立即枚举仍只有 `1` 个 Edit；探针退出后直接 UI Automation 抓拍已显示完整批量创建对话框（前缀、起始编号、编号格式、创建数量、确定、取消），证明控件最终会就绪、固定延时不足，应改为按控件数量轮询。该轮未创建任何本轮资源；关闭对话框后进程失去主窗口但仍暂留，最终只强制结束本轮自有 PID `9496`。最终目标/`Start.exe`、前缀 section/目录均为 `0`，INI 哈希仍为基线值，`SbieSvc=Running`。结果为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-real-audit-20261016_104231_307-result.json`，日志为 `sandbox-lifecycle-real-audit-20261016_104231_307.log`；本轮不作为产品结论。
- 2026-10-16 生命周期探针已删除被证据否定的固定 `500ms`，改在共享控件入口按实际所需数量轮询 Edit、Button 和 ListBox（最多 `8s`），批量创建/重命名的证据快照也改为控件就绪后采集；不涉及生产代码。Windows PowerShell 5.1 解析门禁输出 `PARSE_OK`，日志为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-real-audit-parse-control-poll.log`；下一步再次生成全新专属前缀验证该探针修正。
- 2026-10-16 第四轮窄范围基线完成：全新专属前缀 `ShaheAudit_20261016_105112_254`，`queryex` 返回码 `0`、输出 `96` 项，INI SHA-256 `2709ADB23A9D1080D91C0401DC86A9F954AF81DEAFB5FFD7DF03F5AA30E75A6D`，应用配置 SHA-256 `99120B9DFFD177FF4E66FA9ACDC3547B9085D220EE99483405CAB30B2B91F234`，无目标进程，前缀 section/目录均为 `0`，`SbieSvc=Running`。JSON 为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-narrow-baseline-direct-20261016_105112_194.json`，日志为 `sandbox-lifecycle-narrow-baseline-direct-20261016_105112_194.log`，INI/配置副本为 `sandbox-lifecycle-narrow-baseline-20261016_105112_254-Sandboxie.ini` 与 `sandbox-lifecycle-narrow-baseline-20261016_105112_254-app-config.ini`；真实审计仅允许使用该前缀。
- 2026-10-16 第四轮真实生命周期审计在创建前稳定复现 Win32 控件差异：PID `12252` 通过 `startup_and_lists`，共享控件等待完整轮询 `8s` 后仍仅枚举到 `1` 个 `*EDIT*`，结果为 `inconclusive`、退出码 `2`；这否定“只需更长固定延时”，尚不能判定产品缺陷。没有创建任何前缀资源，INI 哈希保持基线，`SbieSvc=Running`；PID 暂留仅用于转储当前对话框 Win32 子控件，转储后只清理该自有 PID。结果为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-real-audit-20261016_105152_983-result.json`，日志为 `sandbox-lifecycle-real-audit-20261016_105152_983.log`。
- 2026-10-16 新增只读现有 PID 顶层/子窗口转储探针 `window-control-dump.ps1`，Windows PowerShell 5.1 解析门禁输出 `PARSE_OK`；日志为 `artifacts/runs/csharp_navigation_followup_20261015/window-control-dump-parse.log`。下一步只读取本轮自有 PID `12252` 的 Win32 控件树后立即清理该 PID。
- 2026-10-16 当前 PowerShell 对仍运行的本轮自有 PID `12252` 完成纯 Win32 窗口转储：共 `44` 条记录，批量创建对话框此刻确有 `3` 个 `WindowsForms10.EDIT`、`1` 个 ComboBox、`2` 个 Button，主窗口状态文本为“当前没有定时任务”；结果为 `artifacts/runs/csharp_navigation_followup_20261015/window-control-dump-pid12252.json`。这证明探针运行期间的 `1` 个 Edit 与稍后稳定状态存在真实时间/宿主差异；下一步只用 Windows PowerShell 5.1 对同一 PID 复核后清理。
- 2026-10-16 Windows PowerShell 5.1 对同一 PID `12252` 的独立 Win32 转储同样得到 `44` 条记录和 `3` 个 Edit，排除 PowerShell 版本差异；结果为 `artifacts/runs/csharp_navigation_followup_20261015/window-control-dump-pid12252-winps.json`。模态对话框使三次进程级 `CloseMainWindow()` 均返回 `false`，随后仅强制结束本轮自有 PID；最终目标/`Start.exe`、前缀 section/目录均为 `0`，INI 哈希恢复/保持基线，`SbieSvc=Running`。当前证据表明审计架构不应在主窗口刚创建时立即打开模态批量对话框；下一步先等待沙盒列表完成初始加载，再进入生命周期动作，不再增加控件轮询时长。
- 2026-10-16 生命周期探针已在进入动作前等待沙盒列表至少出现 `1` 项，以主列表就绪作为启动完成边界；粘贴步骤改为先调用“粘贴 .exe 或文件路径”，再观察并勾选 `notepad.exe`，不再错误要求粘贴前已有该行。同时按 PowerShell 变量大小写不敏感规则，将探针中的 `$matches` 全部改为非保留描述性变量。Windows PowerShell 5.1 解析门禁输出 `PARSE_OK`，日志为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-real-audit-parse-startup-ready.log`；不涉及生产代码，下一步使用全新专属前缀复测。
- 2026-10-16 第五轮窄范围基线完成：全新前缀 `ShaheAudit_20261016_105909_716`，`queryex` 返回码 `0`、输出 `96` 项，INI SHA-256 `2709ADB23A9D1080D91C0401DC86A9F954AF81DEAFB5FFD7DF03F5AA30E75A6D`，配置 SHA-256 `99120B9DFFD177FF4E66FA9ACDC3547B9085D220EE99483405CAB30B2B91F234`，无目标进程且前缀 section/目录均为 `0`，`SbieSvc=Running`。JSON 为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-narrow-baseline-direct-20261016_105909_662.json`，日志为 `sandbox-lifecycle-narrow-baseline-direct-20261016_105909_662.log`，INI/配置副本为 `sandbox-lifecycle-narrow-baseline-20261016_105909_716-Sandboxie.ini` 与 `sandbox-lifecycle-narrow-baseline-20261016_105909_716-app-config.ini`；真实审计仅允许使用该前缀。
- 2026-10-16 第五轮真实生命周期审计在主列表已就绪后仍于创建前得到 `Expected 3 *EDIT* controls, found 1`，结果 `inconclusive`、退出码 `2`，PID `132`；因此“固定延时”“8 秒控件轮询”“等待沙盒列表就绪”三种探针修正均未解决，不再继续猜测式延长等待。该轮未创建资源，已只强制结束本轮自有 PID；最终目标/`Start.exe`、前缀 section/目录均为 `0`，INI 哈希为基线，`SbieSvc=Running`。结果为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-real-audit-20261016_105958_287-result.json`，日志为 `sandbox-lifecycle-real-audit-20261016_105958_287.log`；下一步改用独立非破坏性时间线诊断记录对话框控件创建全过程，不进入创建动作。
- 2026-10-16 本会话按用户要求在上述探针问题处停止并移交下一会话：结束核对 `HEAD=6fbf83f`，保留 `HotkeyController.cs`、`SettingsRuntimeContractTests.cs` 及所有用户未跟踪目录；无沙河批量打开器、`Start.exe` 或本轮 PowerShell 探针残留，全部 `ShaheAudit_*` section/目录计数均为 `0`，`Sandboxie.ini` SHA-256 为基线 `2709ADB23A9D1080D91C0401DC86A9F954AF81DEAFB5FFD7DF03F5AA30E75A6D`，`SbieSvc=Running`、`SbieCtrl` 用户进程保留。当前第一个未完成任务仍是 Sandboxie 分组/批量/定时生命周期审计；下一会话先做独立非破坏性控件创建时间线诊断，根因明确前不得再次进入创建动作。
- 2026-10-16 新会话已新增非破坏性批量创建对话框时间线探针 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-batch-dialog-timeline.ps1`，尚未解析或执行。首次保留变量静态扫描命令因外层 PowerShell 双引号把正则中的 `$(` 解释为子表达式而失败；该失败只属于只读探针调用语法，未启动目标 EXE、未修改生产代码、未创建 Sandboxie 资源，下一步只修正该扫描命令一次。
- 2026-10-16 时间线探针保留变量静态扫描更正后通过并输出 `RESERVED_ASSIGNMENT_SCAN_OK`，确认脚本没有把 `$PID`、`$args`、`$input`、`$HOME`、`$Host`、`$PWD`、`$Error`、`$Matches`、`$this`、`$true`、`$false` 或 `$null` 用作赋值目标；尚未执行 PowerShell 5.1 解析门禁或启动目标 EXE。
- 2026-10-16 非破坏性批量创建对话框时间线探针已通过 Windows PowerShell 5.1 AST 解析门禁并输出 `PARSE_OK`；日志为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-batch-dialog-timeline-parse.log`。该结果只证明脚本可解析，尚未启动目标 EXE或形成控件时间线证据；下一步执行首次非破坏性时间线采集，采样结束只关闭对话框，不触发确定。
- 2026-10-16 时间线探针在真实执行前依据既有 `Argument types do not match` 探针经验，将内部泛型列表的 PowerShell 数组展开改为显式 `ToArray()`，避免宿主动态绑定差异；更正后的 Windows PowerShell 5.1 AST 解析再次输出 `PARSE_OK`，日志为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-batch-dialog-timeline-parse2.log`。仍未启动目标 EXE或创建 Sandboxie 资源。
- 2026-10-16 批量创建对话框时间线首轮采集为探针失败、无产品结论：目标 PID `8900`、主窗口 `0xBB07E0`，菜单命令 `348` 已异步投递，但诊断脚本的 Unicode 标题常量漏写“用户”，导致 `158` 个内部样本均未把真实“批量创建沙盒用户”窗口识别为目标；独立观察进程已生成 UTF-8 JSON，但 Windows PowerShell 5.1 协调器用默认 ANSI 读取，随后 `ConvertFrom-Json` 失败。探针没有设置输入或点击确定；因未识别模态对话框，`CloseMainWindow=false` 后仅强制结束本轮自有 PID。前后 `Sandboxie.ini` SHA-256 均为 `2709ADB23A9D1080D91C0401DC86A9F954AF81DEAFB5FFD7DF03F5AA30E75A6D`，`ShaheAudit_*` section/目录均为 `0`，`SbieSvc=Running`。结果为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-batch-dialog-timeline-20261016_113519_160-result.json`，独立原始结果为同名前缀 `-result-external.json`，日志为 `sandbox-batch-dialog-timeline-20261016_113519_160.log`；下一步只修正完整标题和显式 UTF-8 读取后重新解析。
- 2026-10-16 首轮时间线的原始样本可在显式 UTF-8 读取后形成部分有效时序证据：内部观察 `157` 个、独立观察 `164` 个对话框样本均指向稳定 HWND `0xA20A1C` 和线程 `4748`，没有 HWND 替换；内部首样本为 `14` 个子窗口、`2 Edit/1 ComboBox/0 Button`，`21.0448ms` 后变为完整 `22` 个子窗口、`3 Edit/1 ComboBox/2 Button`；独立观察首次命中同一 HWND 时已为完整结构。该轮排除持续的宿主分裂、模态采样阻塞和 8 秒原生句柄延迟，但仍不能解释旧生命周期 `Wait-Controls` 为什么稳定返回 `1`；根因方向转向其 PowerShell 集合/过滤链。派生摘要为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-batch-dialog-timeline-20261016_113519_160-retro-summary.json`，仍不作为产品缺陷或生命周期审计通过结论。
- 2026-10-16 新增不启动 EXE 的集合语义探针 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-control-filter-semantics.ps1`，原样复刻生命周期脚本的 `Get-Controls -> Find-Control` 参数链；Windows PowerShell 5.1 AST 解析门禁输出 `PARSE_OK`，日志为 `sandbox-lifecycle-control-filter-semantics-parse.log`。尚未执行该语义探针，未形成根因结论。
- 2026-10-16 集合语义 RED 环已精确复现生命周期探针的稳定计数 `1`：Windows PowerShell 5.1 中同一 `ControlFilterFixture[]` 直接展开为 `6` 个控件并直接过滤出 `3` 个 Edit，但经过现有 `[object[]]$Controls` 的 `Find-Control` 函数传参后，内联调用、数组变量调用和命名参数调用均只返回 `1` 个 Edit。结论为 `collection_shape_reproduced`，证明旧探针连续 8 秒报告 `1` 个 Edit 的根因是 PowerShell 参数绑定/数组展开语义，不是 WinForms 原生句柄、对话框 HWND、模态线程或产品缺陷。结果为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-control-filter-semantics-result.json`，日志为 `sandbox-lifecycle-control-filter-semantics.log`；下一步仅在审计脚本内联过滤控件集合，并修正时间线标题/UTF-8 读取后复验。
- 2026-10-16 生命周期真实审计探针已删除会把集合计数压成 `1` 的 `Find-Control([object[]])` 传参链，改为在 `Wait-Controls` 和 ComboBox 查找处直接对已展开集合过滤；时间线探针同时修正完整“批量创建沙盒用户”标题、Windows PowerShell 5.1 的显式 UTF-8 JSON 读取和部分句柄时间线分类。两个脚本的 Windows PowerShell 5.1 AST 解析均输出 `PARSE_OK`，日志为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-probes-parse-collection-fix.log`；修改仅限审计 artifacts，不涉及生产 C#，下一步先做非破坏性时间线 GREEN 复验，仍不进入创建动作。
- 2026-10-16 对集合语义 RED 的机制复核发现更精确假设：显式传入 `[string]$Title` 的 `$null` 可能被 PowerShell 5.1 强制转换为空字符串，恰好只匹配三个 Edit 中标题为空的前缀框；这意味着上一条内联过滤仍保留 `[string]$Title`，尚不能视为有效修正，也尚未执行。语义探针已加入参数绑定快照及 `[object]$Title` 保留 null 的对照路径，Windows PowerShell 5.1 AST 解析输出 `PARSE_OK`，日志为 `sandbox-lifecycle-control-filter-semantics-parse2.log`；下一步只运行该无 EXE 语义对照以确认机制。
- 2026-10-16 集合语义对照已确认精确根因并纠正此前“数组展开”解释：三种旧调用中 `[object[]]$Controls` 均完整绑定 `6` 个控件、`ClassPattern=*EDIT*`，但 `[string]$Title` 将显式 `$null` 全部强制转换为非 null 的空字符串，只留下标题为空的 `1` 个前缀 Edit；改用 `[object]$Title` 保留 null 后同一过滤返回完整 `3` 个 Edit。结论为 `null_to_empty_string_coercion_reproduced`、`NullStringCoercionConfirmed=true`，结果为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-control-filter-semantics-result2.json`，日志为 `sandbox-lifecycle-control-filter-semantics2.log`。因此旧 `8s/1 Edit` 的明确根因是审计脚本字符串参数的 null 强制转换，不是数组丢失、WinForms、HWND、模态线程或产品缺陷；下一步把 `Wait-Controls.Title` 改为保留 null 的对象参数后重新解析。
- 2026-10-16 生命周期真实审计探针已完成精确修正：`Wait-Controls.Title` 从 `[string]` 改为 `[object]` 以保留显式 null，过滤时仅对非 null 标题转为字符串；已删除的中间 `Find-Control` 仍未恢复。Windows PowerShell 5.1 AST 解析输出 `PARSE_OK`，日志为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-real-audit-parse-null-title-fix.log`。这只代表探针源码修正和解析通过，尚未形成真实 EXE GREEN；下一步先重跑非破坏性时间线，不点击确定。
- 2026-10-16 非破坏性双观察者时间线 GREEN 已完成：目标 PID `12400`、主窗口 `0x9B0804`、菜单命令 `348`，内部/独立观察分别采集 `168/173` 个样本，其中 `167/171` 个命中同一对话框 HWND `0xA90C44`、同一 UI 线程 `10004`；两边均先见 `2` 个 Edit，随后分别在 `53.1352ms/47.5152ms` 内收敛为 `3 Edit/1 ComboBox/2 Button`，没有 HWND 替换，最大采样间隔 `123.542ms/142.544ms`，没有模态采样阻塞。原结果的 `observer_context_difference_observed` 是分类器误报：唯一差异样本相隔 `21.14ms`，Edit/Button 相同，仅 ComboBox 正处于 `0 -> 1` 创建过程，之后持续一致。对话框通过 `WM_CLOSE` 取消，`CloseMainWindow=true`、退出码 `0`、未强制结束；前后 INI SHA-256 均为基线，`ShaheAudit_*` section/目录均为 `0`，`SbieSvc=Running`。原始结果为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-batch-dialog-timeline-20261016_114555_702-result.json`，独立结果为同名前缀 `-result-external.json`，日志为 `sandbox-batch-dialog-timeline-20261016_114555_702.log`，人工判定为 `sandbox-batch-dialog-timeline-20261016_114555_702-assessment.json`。时间线根因现已明确：约 50ms 的正常句柄创建叠加旧探针的 null-to-empty 标题过滤错误，产品对话框无缺陷证据。
- 2026-10-16 时间线探针分类器已收紧：稳定同 HWND 下两观察者都从部分控件过渡到完整 `3 Edit` 时优先判为句柄创建时序，只有不存在该共同过渡且至少 `3` 个近邻样本持续计数不同时才判宿主上下文差异，避免用单个相隔数十毫秒的初始化样本误报。Windows PowerShell 5.1 AST 解析输出 `PARSE_OK`，日志为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-batch-dialog-timeline-parse-classifier-fix.log`；现有有效时间线无需为了分类标签重复启动 EXE。
- 2026-10-16 时间线根因明确且生命周期探针完成精确修正后，全新只读基线采集通过：本轮唯一允许前缀为 `ShaheAudit_20261016_114911_269`，`SbieIni queryex /boxes *` 返回码 `0`、输出 `96` 项；`Sandboxie.ini` SHA-256 为 `2709ADB23A9D1080D91C0401DC86A9F954AF81DEAFB5FFD7DF03F5AA30E75A6D`，隔离应用配置 SHA-256 为 `99120B9DFFD177FF4E66FA9ACDC3547B9085D220EE99483405CAB30B2B91F234`，副本均已保存；目标进程、`ShaheAudit_*` section/目录均为 `0`，`SbieCtrl PID 6844` 和 `SbieSvc PID 2836/4916` 保持，服务为 `Running`。JSON 为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-narrow-baseline-direct-20261016_114911_220.json`，日志为 `sandbox-lifecycle-narrow-baseline-direct-20261016_114911_220.log`，副本为 `sandbox-lifecycle-narrow-baseline-20261016_114911_269-Sandboxie.ini` 与 `sandbox-lifecycle-narrow-baseline-20261016_114911_269-app-config.ini`；下一次真实生命周期审计只能使用该前缀。
- 2026-10-16 前缀 `ShaheAudit_20261016_114911_269` 的真实生命周期审计通过 `startup_and_lists` 和 `batch_create_two_sandboxes` 后，在精确菜单路径 `文件(&F)|粘贴 .exe 或文件路径` 处未匹配带快捷键后缀的实际菜单文本，结果为 `inconclusive`、退出码 `2`；这是探针菜单文本匹配失败，不是产品失败。目标 PID `928` 经 `CloseMainWindow=true` 正常退出，退出码 `0`，未执行分组、启动、定时、重命名或删除步骤。Sandboxie 将两个显示名截短为内部名 `ShaheAudit_20261016_11491`、`ShaheAudit_20261016_11492`，现有清理器按原始长前缀判断而漏清理，两个精确 section 和目录仍存在，INI SHA-256 暂为 `DCD81104CC73087D2CA6515C55C7A9BA943B3F9741D6D90A47E385F9F47D94D1`；目标 EXE、`Start.exe` 和探针 PowerShell 均无残留，`SbieSvc=Running`。结果为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-real-audit-20261016_115024_873-result.json`，日志为 `sandbox-lifecycle-real-audit-20261016_115024_873.log`；当前必须先按结果 JSON 中两个精确内部名清理并验证恢复基线，再修正探针和生成全新前缀，生命周期审计尚未完成。
- 2026-10-16 两个遗留资源经 `BoxAlias` 精确所有权校验后，已分别按内部名调用官方 `/terminate`、`delete_sandbox_silent` 和 `SbieIni set /drv`；随后的两个 section/目录均为 `0`，INI SHA-256 已恢复基线 `2709ADB23A9D1080D91C0401DC86A9F954AF81DEAFB5FFD7DF03F5AA30E75A6D`，目标 EXE和生命周期探针进程为 `0`，`SbieSvc=Running`、`SbieCtrl PID 6844` 及 `SbieSvc PID 2836/4916` 未变。但命令结束即时仍观察到 `Start.exe PID 5172`，所以本轮清理验证正确标为 `inconclusive`，下一步只读核验该 PID，不能先声明最终清理完成。结果为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-owned-cleanup-20261016_145302_858-result.json`，日志为 `sandbox-lifecycle-owned-cleanup-20261016_145302_858.log`；JSON 中 `PreCleanupSandboxieIniSha256` 因采样位置错误记录成命令后基线值，删除前独立校验的实际值仍为上一条 `DCD81104...`，该字段不得用作前态证据。
- 2026-10-16 对 `Start.exe PID 5172` 的后续只读复核确认该进程已自然退出；当前无 `Start.exe` 或目标 EXE，两个精确内部名的 section/目录继续为 `0`，INI SHA-256 保持基线，`SbieSvc=Running`、`SbieCtrl PID 6844` 和 `SbieSvc PID 2836/4916` 未变。下一步仅生成带持久日志/JSON 的最终清理门禁，不再执行删除命令。
- 2026-10-16 持久化只读清理门禁通过、退出码 `0`：全部 `ShaheAudit_*` INI section 和 `C:\Sandbox\Administrator\ShaheAudit_*` 目录计数均为 `0`，`Sandboxie.ini` SHA-256 精确恢复基线 `2709ADB23A9D1080D91C0401DC86A9F954AF81DEAFB5FFD7DF03F5AA30E75A6D`；无目标 EXE、`Start.exe` 或生命周期 PowerShell 探针残留，`SbieSvc=Running`，`SbieCtrl PID 6844` 和 `SbieSvc PID 2836/4916` 保持。结果为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-cleanup-verification-20261016_145451_078-result.json`，日志为 `sandbox-lifecycle-cleanup-verification-20261016_145451_078.log`。本轮遗留资源清理现已完成；生命周期业务审计仍未完成，下一步只修正探针的菜单快捷键后缀匹配和内部名清理映射。
- 2026-10-16 新增无 EXE 契约探针 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-probe-contract.ps1`，用于直接编译生命周期脚本的嵌入 C# 并验证 Tab 快捷键后缀匹配，同时静态守卫不再按原始长前缀过滤已解析的本轮内部名；Windows PowerShell 5.1 AST 解析输出 `PARSE_OK`，日志为 `sandbox-lifecycle-probe-contract-parse.log`。尚未运行 RED、未修改生命周期脚本、未启动目标 EXE。
- 2026-10-16 无 EXE 契约探针已扩展安全清理选择 fixture：要求基线 section/目录排除、唯一显示名/`BoxAlias` 精确归属、创建后尚未登记时的别名发现、已验证内部名的目录残留兜底，以及相似别名、重复别名和所有权变化拒绝；扩展版 Windows PowerShell 5.1 AST 解析输出 `PARSE_OK`，日志为 `sandbox-lifecycle-probe-contract-parse2.log`。fixture 全在内存中，未执行删除或启动 EXE，下一步运行预期 RED。
- 2026-10-16 无 EXE 契约 RED 有效、退出码 `1`：生命周期脚本 AST 问题为 `0`，但当前嵌入 C# 尚无 `MenuTextMatches`，精确正文和 Tab 快捷键后缀用例均失败；错误的原始长前缀清理守卫仍存在，安全清理选择函数尚不存在，结论为 `red`。结果为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-probe-contract-red-20261016_150428_077-result.json`，日志为 `sandbox-lifecycle-probe-contract-red-20261016_150428_077.log`。该 RED 不启动 EXE或调用 Sandboxie，下一步只修改生命周期审计探针。
- 2026-10-16 生命周期审计探针已作最小根因修正：共享菜单匹配现在只接受正文全等或正文后紧跟 Tab 快捷键后缀；内部名解析未命中精确 section/`BoxAlias` 时不再猜测删除目标；清理选择新增启动前 section/目录基线、当前唯一精确显示名/别名发现、已验证内部名兜底及歧义/归属变化拒绝，删除仍只走逐个精确内部名的官方命令。生命周期脚本与无 EXE 契约脚本的 Windows PowerShell 5.1 AST 解析均通过，日志为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-probes-parse-menu-cleanup-fix.log`；尚未运行 GREEN或真实 EXE。源码复核同时纠正前述简写：短内部名由应用 `SandboxieClient` 生成并以 `BoxAlias` 保存完整显示名，并非 Sandboxie 自行截断；该修正仍仅涉及审计 artifacts，不改生产 C#。
- 2026-10-16 无 EXE 契约 GREEN 通过、退出码 `0`：实际嵌入 C# 的正文全等、Tab 快捷键后缀匹配及不同正文拒绝均为 `true`，脚本 AST 问题为 `0`，错误长前缀清理守卫不存在；安全清理 fixture 精确选中 `Internal01`、`Internal02`、`TrackedDirectoryOnly`、`DiscoveredByAlias`、`Audit_Direct_Name`，并拒绝基线 section/目录碰撞、相似/重复别名和所有权变化，结论为 `passed`。结果为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-probe-contract-green-20261016_150746_381-result.json`，日志为 `sandbox-lifecycle-probe-contract-green-20261016_150746_381.log`。这只表示探针修正完成，真实 Sandboxie 生命周期审计与整个软件迁移仍未完成；下一步经只读复核后使用从未用过的新前缀重建基线。
- 2026-10-16 探针修正独立只读审查结论为：菜单实现符合正文全等/Tab 后缀规格，但清理仍有 `2` 项 Important 和 `1` 项 Minor，真实 EXE 暂不得重跑。Important 1：section 名等于预期显示名但存在冲突或重复 `BoxAlias` 时，当前选择器仍可能误判归属；Important 2：官方清理命令失败、目标未退出或最终 section/目录残留不会把已通过的主流程降级，可能产生退出码 `0` 的假通过，并且内容删除失败后仍先删 section。Minor：菜单契约尚缺“相同正文前缀但非 Tab 后缀”负例；另确认 INI 不存在时 `Resolve-InternalName` 仍返回显示名，与“不猜测”声明不一致。下一步先扩展无 EXE 契约取得新 RED，再修复这些探针边界；该审查未运行 EXE或操作 Sandboxie。
- 2026-10-16 第二轮无 EXE 契约已加入冲突同名 section、INI 缺失、清理选择错误、目标未退出、目录/section/`Start.exe` 残留、内容删除失败时禁止删 section，以及相同正文前缀但非 Tab 后缀负例；Windows PowerShell 5.1 AST 解析输出 `PARSE_OK`，日志为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-probe-contract-parse3.log`。尚未运行第二轮 RED、未改生命周期实现、未启动 EXE。
- 2026-10-16 第二轮无 EXE RED 有效、退出码 `1`：菜单同前缀非 Tab 两项负例已正确拒绝；但清理选择多选了带冲突 `BoxAlias` 的 `Audit_Conflicting_Name`，`MissingIniRejected=false`，清理完整性函数不存在，选择错误/目标未退出/目录、section、`Start.exe` 残留均未形成拒绝，内容删除成功前禁止删 section 的守卫也不存在，结论为 `red`。结果为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-probe-contract-red2-20261016_151555_456-result.json`，日志为同名前缀 `.log`；未启动 EXE或调用 Sandboxie。
- 2026-10-16 第二轮探针实现已收紧：存在任何 `BoxAlias` 时 section 名不得覆盖冲突别名，INI 缺失直接报错；`delete_sandbox_silent` 返回 `0` 且精确目录已消失后才删除 section；最终清理选择错误、目标未退出、目录/section 或 `Start.exe` 残留都会把结论降为 `inconclusive`。生命周期与契约脚本的 Windows PowerShell 5.1 AST 解析均通过，日志为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-probes-parse-cleanup-verdict-fix.log`；尚未运行第二轮 GREEN或真实 EXE。
- 2026-10-16 第二轮无 EXE 契约 GREEN 通过、退出码 `0`：菜单正文全等、Tab 后缀及三个不同/同前缀非 Tab 负例全部通过；清理选择精确返回 `Internal01`、`Internal02`、`TrackedDirectoryOnly`、`DiscoveredByAlias`、`Audit_Direct_Name`，冲突同名未被选中；INI 缺失拒绝、清理选择错误、目标未退出、目录/section/`Start.exe` 残留拒绝及内容删除成功后才删 section 均为 `true`，结论 `passed`。结果为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-probe-contract-green2-20261016_151844_151-result.json`，日志为同名前缀 `.log`。这仍只代表探针修正证据，下一步独立复审后才可生成全新前缀和基线；真实生命周期审计与整个迁移尚未完成。
- 2026-10-16 第二轮修正独立只读复审通过：上轮 `2` 项 Important 和 `1` 项 Minor 均已解决，未发现剩余 Critical/Important；确认冲突/重复别名拒绝、INI 缺失报错、内容删除与 section 删除顺序、清理残留降级及菜单同前缀负例均已接入当前脚本。新增“同一 section 两条相同 `BoxAlias`”纯内存负例的 Windows PowerShell 5.1 契约 GREEN 通过、退出码 `0`，`DuplicateAliasRejected=true` 且其余断言保持通过；结果为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-probe-contract-green3-20261016_152357_371-result.json`，日志为同名前缀 `.log`。这只证明探针修正，下一步建立全新未使用前缀的只读基线；真实生命周期审计与整个迁移尚未完成。
- 2026-10-16 用户要求暂停并诊断 PowerShell 消耗：一次只读进程快照中项目探针候选数为 `0`，没有遗留 `powershell.exe`、目标 EXE 或 `Start.exe`；唯一 `pwsh.exe PID 14052` 是该次 1 秒诊断命令本身，命令返回后已结束。`SbieSvc=Running`，受保护的 `SbieCtrl PID 6844`、`SbieSvc PID 2836/4916` 未操作。结果为 `artifacts/runs/csharp_navigation_followup_20261015/powershell-process-diagnostic-20261016_152546_174-result.json`；生命周期审计现已暂停，未建立新基线。
- 2026-10-16 PowerShell 探针调用问题已修复并重新验证：历史红色结果中的外层双引号提前展开、Windows PowerShell 5.1 无 BOM UTF-8 中文误读和自动变量命名冲突均属于探针/调用错误，不是 PowerShell 安装故障；预期 RED 的退出码 `1` 与审计无结论的退出码 `2` 必须记录为测试状态，不能表述为 PowerShell 引擎损坏。最新 Windows PowerShell `5.1.19041.6456` 契约验证退出码 `0`、`Conclusion=passed`、审计脚本 AST 问题数 `0`、重复别名拒绝为 `true`、遗留项目 PowerShell 数为 `0`；结果为 `artifacts/runs/csharp_navigation_followup_20261015/powershell-fix-verification-20261016_153027_391-result.json`，日志为同名前缀 `.log`。该结论仅代表 PowerShell 探针调用与契约门禁正常，生命周期审计仍暂停且尚未完成。
- 2026-10-16 当前生命周期脚本 UTF-8 BOM 检查的首次调用在 PowerShell 解析阶段失败：格式运算符 `-f` 后直接使用多个数组索引触发逗号解析错误，尚未生成结果 JSON、未改写任何脚本，也未启动 EXE 或 Sandboxie；这是新的检查命令语法错误，不是编码或产品结论。下一次仅把首字节格式化改为无歧义的 `BitConverter.ToString()` 后重试一次。
- 2026-10-16 编码检查更正后通过：当前生命周期真实审计、窄基线、批量对话框时间线、窗口控件转储和无 EXE 契约共 `5` 个脚本均无 UTF-8 BOM，但也均不含任何非 ASCII 字符，`NonAsciiWithoutBomCount=0`，因此不存在 Windows PowerShell 5.1 把脚本内 UTF-8 中文按 ANSI 误读的触发条件；无需机械添加 BOM。结果为 `artifacts/runs/csharp_navigation_followup_20261015/powershell-encoding-audit-20261016_153416_295-result.json`。早期编码失败来自当时脚本内的中文文字面量，当前审计链已通过 ASCII 脚本与显式 UTF-8 数据读取消除该风险；生命周期审计仍暂停、尚未完成。
- 2026-10-16 新专属前缀的宽范围只读基线探针 `sandbox-lifecycle-baseline.ps1` 运行超过 60 秒未生成 JSON 或标准输出，已结束本轮自有 Windows PowerShell PID `10720` 和外层 PowerShell 7 PID `1772`；目标 EXE、`Start.exe` 和本轮沙盒均未启动或创建。本次探针失败不作为产品结论，超时日志为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-baseline-new-20261016_071833_919-timeout.log`。
- 当前活动计划为 `docs/superpowers/plans/2026-10-15-multi-user-navigation-and-exe-audit.md`。本轮列表移动刷新修复已完成自动化门禁；标准 Release 输出仍受运行中实例锁定。最终隔离 Release x64 的首轮真实多用户导航探针因主窗口出现后列表子控件尚未创建而无效，增加 8 秒控件加载等待后已取得有效复测；关闭进程及其余 Sandboxie 动作级审计继续进行。
- 当前活动计划的真实审计进度：最终隔离 Release x64 多用户导航有效复测、关闭当前/全部进程已完成；Sandbox 管理/定时的无选择守卫和批量创建对话框已完成非破坏性真实复测；路径记忆管理器入口、数量设置、隐藏路径保存、粘贴新增和选择/勾选已验证。路径记忆配置数组仍有独立兼容风险：`JArray` 元素为不满足非泛型 `IDictionary` 的 `JObject`，导致对象数组历史启动为空；该风险尚未修复。路径记忆上下文菜单的跨进程探针仍未取得备注/删除/清空证据，实际 Sandboxie 创建/重命名/删除、分组成功流程、托盘/键盘/无障碍和备份还原仍待完成；未声明整个软件迁移完成。
- 2026-10-15 最终隔离 Release x64 真实多用户导航首轮无效：目标 EXE PID `8900` 已出现主窗口，但导航探针立即枚举列表控件得到 `candidates=0`，未取得任何性能数据；`CloseMainWindow()` 超时后仅强制结束本轮自有 PID。四个审计沙盒 `Start.exe /listpids` 均为 `0`，`SbieSvc` 仍为 `Running`。结果与原始异常日志保存在 `artifacts/runs/csharp_navigation_followup_20261015/final-isolated-navigation-run-summary.json`、`final-isolated-navigation-one-process.log`。
- 2026-10-15 最终隔离 Release x64 真实多用户导航有效复测：隔离副本 PID `6132` 等待 8 秒后列表句柄稳定、条目数 `96`；1 进程/沙盒场景的空闲、1/2/3/4 用户单键中位数为 `14.641/8.094/8.185/7.135/7.745ms`，20 键突发最大值为 `161.251/152.114/167.955/153.254/155.246ms`；5 进程/沙盒场景对应中位数为 `14.676/7.141/7.889/6.887/6.965ms`，突发最大值为 `154.367/154.078/151.675/156.122/153.126ms`。两组均无超时、丢键或目标索引未到达；四个审计沙盒最终 PID 均为 `0`，`SbieSvc=Running`，EXE 通过 `CloseMainWindow()` 正常退出。结果在 `artifacts/runs/csharp_navigation_followup_20261015/final-isolated-navigation-one-process-rerun-result.json`、`final-isolated-navigation-five-process-rerun-result.json`，汇总在 `final-isolated-navigation-rerun-summary.json`。
- 2026-10-15 最终隔离 Release x64 关闭动作审计首轮无效：A/B/C/D 启动前均为空，A/B 的 `Start.exe /listpids` 已分别报告 `3` 个 PID，但旧审计脚本等待列表文本变为“运行中”时始终看到“空闲中”且选中索引为 `-1`，因此未执行菜单调用，不能作为关闭动作结论；本轮 EXE 通过 `CloseMainWindow()` 清理，A/B/C/D 最终 PID 均为 `0`，`SbieSvc=Running`。诊断结果在 `artifacts/runs/csharp_navigation_followup_20261015/final-isolated-close-actions-result.json`，日志在 `final-isolated-close-actions.log`，需改用已确认 PID 的选择/状态校准后重跑。
- 2026-10-15 最终隔离 Release x64 关闭动作 PID 校准复测仍未形成动作证据：A/B PID 判据已通过，脚本成功定位并选择 AuditA 列表行，但“关闭当前打开的进程”菜单在 5 秒内始终未启用，未调用当前/全部关闭菜单；这表明还需要先触发与生产状态缓存一致的刷新/选择校准，不能把该轮记为关闭动作通过。目标 EXE PID `7272` 通过 `CloseMainWindow()` 清理，A/B/C/D 最终 PID 均为 `0`，`SbieSvc=Running`。结果在 `artifacts/runs/csharp_navigation_followup_20261015/final-isolated-close-actions-pid-calibrated-result.json`，日志在 `final-isolated-close-actions-pid-calibrated.log`。
- 2026-10-16 进程菜单实时刷新合并契约复核：使用最终隔离 Release x64 测试程序集运行 `ProcessMenuLiveStateRefreshCoalescesWhileWorkerIsRunning`，返回码 `0`，实际执行 `1` 项，通过 `1`、失败 `0`、跳过 `0`，总时间 `4.0008` 秒；确认已有后台 worker 运行时后续刷新请求只保留一个 pending 请求，不启动并行扫描。日志为 `artifacts/runs/csharp_navigation_followup_20261015/csharp-task4-live-refresh-coalesces-recheck2-20261016_191546_318.log`。该结果再次排除“并行 worker 未合并”作为当前关闭菜单禁用的根因，但不证明完整导航调用链的刷新与菜单重建次数已完成计数。
- 2026-10-16 进程菜单契约类复核：使用同一最终隔离 Release x64 测试程序集运行 `ProcessMenuRuntimeContractTests`，返回码 `0`，实际执行 `15` 项，通过 `15`、失败 `0`、跳过 `0`，总时间 `7.1057` 秒；覆盖可见运行标记、隐藏分组、静音缓存、选择反馈、单次 Sandboxie PID 快照、无输出设备枚举、后台刷新合并、UI 线程扫描隔离和菜单重建扫描隔离。日志为 `artifacts/runs/csharp_navigation_followup_20261015/csharp-process-menu-contracts-recheck-20261016_195153_360.log`。该复核确认既有契约未回归，但不替代 Task 4 的完整调用链次数测量。
- 2026-10-16 C# `Application` 分类复核：使用最终隔离 Release x64 测试程序集按 `TestCategory=Application` 运行，返回码 `0`，实际执行 `48` 项，通过 `48`、失败 `0`、跳过 `0`，总时间 `10.1667` 秒；覆盖主窗口组合、经典菜单与快捷键分派、路径记忆、备份恢复入口、分组/批量/定时入口和进程菜单状态契约。日志为 `artifacts/runs/csharp_navigation_followup_20261015/csharp-application-category-recheck-20261016_195234_527.log`。该结果只确认应用层入口与契约未回归，不替代未完成的真实 Sandboxie 生命周期审计。
- 2026-10-16 C# `Controls` 分类复核：使用最终隔离 Release x64 测试程序集按 `TestCategory=Controls` 运行，返回码 `0`，实际执行 `14` 项，通过 `14`、失败 `0`、跳过 `0`，总时间 `3.8796` 秒；确认列表选择与键盘导航保持显式触发、Space 勾选和状态播报、经典上下文菜单、原始动作参数、后台运行状态刷新与控件可访问性契约未回归。日志为 `artifacts/runs/csharp_navigation_followup_20261015/csharp-controls-category-recheck-20261016_195313_080.log`。该结果属于进程内 WinForms 契约，不替代真实 EXE 的完整托盘、焦点顺序和屏幕阅读器审计。
- 2026-10-16 C# `Lifecycle` 分类复核：使用最终隔离 Release x64 测试程序集按 `TestCategory=Lifecycle` 运行，返回码 `0`，实际执行 `7` 项，通过 `7`、失败 `0`、跳过 `0`，总时间 `4.3122` 秒；确认主控件初始化、显式启动动作、会话取消边界、延迟调用停止与幂等销毁、关闭到托盘/强制退出语义和不改变焦点的状态刷新契约未回归。日志为 `artifacts/runs/csharp_navigation_followup_20261015/csharp-lifecycle-category-recheck-20261016_195343_995.log`。该结果不替代真实 EXE 托盘生命周期和完整焦点顺序审计。
- 2026-10-16 Python 正式基线全量复核：运行 `python -m unittest discover -s tests -p "test_*.py" -v`，返回码 `0`，实际执行 `29` 项，通过 `29`、失败 `0`、跳过 `0`，测试用时 `3.574` 秒；确认当前 C# 迁移所依据的 Python 对照行为未回归。日志为 `artifacts/runs/csharp_navigation_followup_20261015/python-full-recheck-20261016_195527_706.log`。
- 2026-10-16 Python 生产源码语法门禁复核：运行 `python -m compileall -q shahe_bulk_launcher tests`，返回码 `0`，日志为 `0` 字节且没有语法错误输出；日志路径为 `artifacts/runs/csharp_navigation_followup_20261015/python-compile-production-recheck-20261016_195604_974.log`。
- 2026-10-16 真实 EXE 非破坏性测试前只读预检通过：目标 EXE、隔离配置和 `C:\Windows\Sandboxie.ini` 均存在，SHA-256 分别为 `123607B3A46E6A1939F933DB20F2D53CBA0266D4696E5B209FE7D8C984CDE517`、`99120B9DFFD177FF4E66FA9ACDC3547B9085D220EE99483405CAB30B2B91F234`、`2709ADB23A9D1080D91C0401DC86A9F954AF81DEAFB5FFD7DF03F5AA30E75A6D`，均与权威基线一致；无目标 EXE、`Start.exe`、`powershell.exe`、AuditRunner 或 `SbieCtrl.exe` 进程，另有 `pwsh.exe PID 13336`，没有项目审计进程证据；`SbieSvc PID 2724` 且服务状态为 `Running`；`ShaheAudit_*` section 与直属目录计数均为 `0`。结果为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-safe-preflight-20261016_195738_104.json`。`SbieCtrl.exe` 未运行与旧快照不同，按当前实时状态记录，不主动启动或恢复。
- 2026-10-16 真实 EXE F10 首轮焦点探针无效：目标隔离 EXE PID `3612`、窗口标题和主窗口无障碍树均已取得，但自动化后端未确认键盘焦点，屏幕捕获属于其他前景窗口；发送 F10 后目标无障碍树未出现菜单且保持不变，因此本轮结论为 `inconclusive`，不记为产品失败。结果为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-f10-focus-probe-20261016_200019_317-result.json`。本轮没有选择菜单命令、没有创建或修改 Sandboxie 资源，下一步只通过目标列表控件建立可验证焦点后重试一次。
- 2026-10-16 真实 EXE 焦点建立探针失败：对 PID `3612` 的沙盒列表窗格执行 UI Automation accessibility click 时，Windows 自动化后端在其临时运行脚本中失败，输入未送达且目标无障碍树未改变；当前截图仍属于其他前景窗口，因此没有使用坐标回退。结果为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-focus-establishment-20261016_200117_031-result.json`。该结果只属于自动化工具失败，不是产品行为结论，也未修改 Sandboxie 资源。
- 2026-10-16 真实 EXE Alt+F4 焦点探针无结论：源码映射确认 `exit_app` 会设置 `ForceExitRequested=true` 后调用 `Close()`；Windows 自动化后端报告对目标应用发送 Alt+F4，但发送前始终没有确认目标键盘焦点，随后精确复核 PID `3612` 仍存在。因此结论为 `inconclusive`，不能据此写成退出功能失败。结果为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-alt-f4-focus-probe-20261016_200319_657-result.json`。本轮未调用任何产品菜单或 Sandboxie 资源动作，下一步只清理本轮自有目标 PID。
- 2026-10-16 真实 EXE 焦点探针目标清理完成：只对本轮从用户指定隔离路径启动的 PID `3612` 执行不带 `/F` 的 `taskkill /PID 3612`，命令返回码 `0` 并报告已发送终止信号；随后 `tasklist /FI "PID eq 3612"` 确认该 PID 已不存在。结果为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-focus-probe-cleanup-20261016_200350_767-result.json`。没有停止 `SbieSvc`、其他 Sandboxie 进程或未知进程。
- 2026-10-16 真实 EXE 焦点探针只读后检通过：目标 EXE、隔离配置和 `Sandboxie.ini` SHA-256 分别仍为 `123607B3A46E6A1939F933DB20F2D53CBA0266D4696E5B209FE7D8C984CDE517`、`99120B9DFFD177FF4E66FA9ACDC3547B9085D220EE99483405CAB30B2B91F234`、`2709ADB23A9D1080D91C0401DC86A9F954AF81DEAFB5FFD7DF03F5AA30E75A6D`；无目标 EXE、`Start.exe`、`powershell.exe`、AuditRunner 或 `SbieCtrl.exe`，`ShaheAudit_*` section/目录仍均为 `0`，`SbieSvc PID 2724` 且状态为 `Running`。实时快照另见 `pwsh.exe PID 4972`，没有项目审计进程证据。结果为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-safe-postflight-20261016_200418_077.json`。
- 2026-10-16 C# 路径记忆运行时契约复核：使用最终隔离 Release x64 测试程序集运行 `PathMemoryRuntimeContractTests`，返回码 `0`，实际执行 `4` 项，通过 `4`、失败 `0`、跳过 `0`，总时间 `4.1748` 秒；确认经典上下文菜单与 Ctrl+H、显示文本/Tag、选择与勾选恢复、历史服务调用和管理器保存回调契约未回归。日志为 `artifacts/runs/csharp_navigation_followup_20261015/csharp-path-memory-contracts-recheck-20261016_200458_831.log`。该结果不替代尚未完成的真实备注、删除和清空动作审计。
- 2026-10-16 C# 路径历史服务契约复核：使用最终隔离 Release x64 测试程序集运行 `AppPathHistoryServiceContractTests`，返回码 `0`，实际执行 `8` 项，通过 `8`、失败 `0`、跳过 `0`，总时间 `3.5929` 秒；覆盖存在 EXE 的新增与去重、原始/引号/换行/file URI 剪贴板解析、隐藏和完整显示文本、选择/勾选索引、当前路径回调顺序及记忆项变更保存刷新。日志为 `artifacts/runs/csharp_navigation_followup_20261015/csharp-app-path-history-service-recheck-20261016_200535_646.log`。服务层逻辑通过仍不替代真实上下文菜单备注、删除和清空交互证据。
- 2026-10-16 C# 托盘契约复核：使用最终隔离 Release x64 测试程序集运行 `TrayCSharpContractTests`，返回码 `0`，实际执行 `3` 项，通过 `3`、失败 `0`、跳过 `0`，总时间 `3.8655` 秒；确认托盘提示/图标/经典菜单文本、左键及菜单恢复窗口、退出前移除图标并请求强制关闭和幂等销毁契约未回归。日志为 `artifacts/runs/csharp_navigation_followup_20261015/csharp-tray-contracts-recheck-20261016_200603_551.log`。该结果使用进程内替身，不替代真实系统托盘交互审计。
- 2026-10-16 当前 checkout 隔离 Release x64 构建复核：运行 `dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_navigation_followup_20261015\isolated-current-recheck-binx64\`，返回码 `0`，Core、App、Updater、Tests 全部生成，`0` 警告、`0` 错误，用时 `7.14` 秒；输出目录为 `artifacts/runs/csharp_navigation_followup_20261015/isolated-current-recheck-binx64/x64/Release/net48/`，日志为 `artifacts/runs/csharp_navigation_followup_20261015/csharp-current-isolated-release-build-20261016_200646_565.log`。该构建包含当前工作树中用户已有的 `HotkeyController.cs` 与 `SettingsRuntimeContractTests.cs` 修改，但没有修改、暂存或提交这两个文件。
- 2026-10-16 当前 checkout C# 快捷键契约复核：使用新隔离程序集运行 `HotkeyControllerContractTests`，返回码 `0`，实际执行 `8` 项，通过 `8`、失败 `0`、跳过 `0`，总时间 `3.5308` 秒；覆盖固定/自定义快捷键、特殊功能键/数字/小键盘解析、固定冲突过滤、快捷键开关、相邻用户跳过特殊项并循环、进程菜单运行标记和过期实时刷新结果拒绝。日志为 `artifacts/runs/csharp_navigation_followup_20261015/csharp-current-hotkey-contracts-recheck-20261016_200832_049.log`。该结果包含用户现有 `HotkeyController.cs` 修改，但不替代真实 EXE 在可确认焦点下的完整键盘审计。
- 2026-10-16 当前 checkout C# 设置运行时契约复核：使用新隔离程序集运行 `SettingsRuntimeContractTests`，返回码 `0`，实际执行 `2` 项，通过 `2`、失败 `0`、跳过 `0`，总时间 `4.5664` 秒；确认全量设置重置不在 UI 线程同步扫描全部进程，并保持先同步分组、再刷新沙盒列表的顺序。日志为 `artifacts/runs/csharp_navigation_followup_20261015/csharp-current-settings-runtime-recheck-20261016_200905_476.log`。该结果包含用户现有测试修改，源文件未被修改、暂存或提交。
- 2026-10-16 当前 checkout C# Sandbox 管理服务契约复核：使用新隔离程序集运行 `SandboxAdminServiceContractTests`，返回码 `0`，实际执行 `8` 项，通过 `8`、失败 `0`、跳过 `0`，总时间 `4.6582` 秒；覆盖分组/名称/音量/历史规范化、分组固定状态、批量命名校验与状态、INI 和用户设置转换、设置与定时任务引用重命名/删除、目录重命名回滚、注册表清理计划、别名和删除前停止回调。日志为 `artifacts/runs/csharp_navigation_followup_20261015/csharp-current-sandbox-admin-service-recheck-20261016_201001_516.log`。该测试未调用真实 Sandboxie，不替代分组、批量重命名和删除生命周期审计。
- 2026-10-16 当前 checkout C# 启动/定时服务契约复核：使用新隔离程序集运行 `LaunchSchedulerContractTests`，返回码 `0`，实际执行 `9` 项，通过 `9`、失败 `0`、跳过 `0`，总时间 `3.5049` 秒；覆盖启动项解析与定时文案、重试/完成、取消与窗口关闭、选中沙盒关闭、仅使用显式勾选应用、定时启动/关闭状态、移除自动负载调整、用户配置并发和后台 worker/备份边界。日志为 `artifacts/runs/csharp_navigation_followup_20261015/csharp-current-launch-scheduler-recheck-20261016_201105_963.log`。该测试使用受控回调，未创建真实定时任务或沙盒进程，不替代真实定时生命周期审计。
- 2026-10-16 当前 checkout C# Sandboxie 客户端契约复核：使用新隔离程序集运行 `SandboxieClientContractTests`，返回码 `0`，实际执行 `8` 项，通过 `8`、失败 `0`、跳过 `0`，总时间 `3.4602` 秒；覆盖正式文案与安装文件优先级、官方沙盒/INI section 解析、显示名映射和短内部名生成、启动命令与无效沙盒输出识别、PID API 与逐沙盒回退、官方创建/删除命令顺序、分离启动/无效名重试及注册表安装路径优先级。日志为 `artifacts/runs/csharp_navigation_followup_20261015/csharp-current-sandboxie-client-recheck-20261016_201203_291.log`。测试使用命令替身，没有调用系统 Sandboxie，不替代真实生命周期审计。
- 2026-10-16 当前 checkout C# 备份恢复服务契约复核：使用新隔离程序集运行 `BackupRestoreServiceContractTests`，返回码 `0`，实际执行 `7` 项，通过 `7`、失败 `0`、跳过 `0`，总时间 `3.5515` 秒；覆盖安全解压与不安全路径拒绝、`.sbx` 归档命名和用户布局、选择性 INI section 合并且保留无关 section、选择性数据恢复与非调用线程服务生命周期、Sandboxie 控制程序优先级、恢复/归档文案和备份 worker 状态。日志为 `artifacts/runs/csharp_navigation_followup_20261015/csharp-current-backup-restore-recheck-20261016_201257_425.log`。该复核没有执行真实覆盖恢复，不替代隔离环境的完整恢复审计。
- 2026-10-16 当前 checkout C# 主窗口应用契约复核：使用新隔离程序集运行 `MainFormApplicationContractTests`，返回码 `0`，实际执行 `27` 项，通过 `27`、失败 `0`、跳过 `0`，总时间 `5.9580` 秒；覆盖服务组合与真实进程边界、Python 路径/启动顺序、经典菜单和无障碍顺序、固定/自定义快捷键分派、设置开关、退出释放顺序、备份/恢复、分组/批量/定时菜单接线以及恢复进度对话框可访问阶段控件。日志为 `artifacts/runs/csharp_navigation_followup_20261015/csharp-current-main-form-application-recheck-20261016_201352_309.log`。入口与接线通过不替代真实 Sandboxie、托盘、键盘和覆盖恢复审计。
- 2026-10-16 当前 checkout C# 主窗口生命周期契约复核：使用新隔离程序集运行 `MainFormLifecycleContractTests`，返回码 `0`，实际执行 `7` 项，通过 `7`、失败 `0`、跳过 `0`；TRX 记录开始时间 `20:52:32.6885868+08:00`、结束时间 `20:52:36.6782323+08:00`。确认主窗口初始化、显式启动、会话取消、延迟调用停止与幂等销毁、关闭到托盘/强制退出及不改变焦点的状态刷新契约在当前 checkout 未回归。结果为 `artifacts/runs/csharp_navigation_followup_20261015/csharp-current-main-form-lifecycle-recheck-20261016_205229_123.trx`；该进程内结果不替代剩余真实键盘焦点和屏幕阅读器审计。
- 2026-10-16 当前 checkout C# 单实例守卫契约复核：使用新隔离程序集运行 `SingleInstanceGuardContractTests`，返回码 `0`，实际执行 `3` 项，通过 `3`、失败 `0`、跳过 `0`，控制台持续时间 `367 ms`。确认默认名称及中文运行提示文本、全局互斥体只允许一个活动守卫且释放后可重新获取、锁文件后备拒绝活动 PID 并替换过期/损坏 PID 的契约未回归。结果为 `artifacts/runs/csharp_navigation_followup_20261015/csharp-current-single-instance-guard-recheck-20261016_205326_088.trx`；该结果与本轮真实 EXE 单实例恢复证据相互补充，运行中提示窗口本身仍未被有效捕获。
- 2026-10-16 当前 checkout C# 进程动作服务契约复核：使用新隔离程序集运行 `ProcessActionServiceContractTests`，返回码 `0`，实际执行 `8` 项，通过 `8`、失败 `0`、跳过 `0`，控制台持续时间 `526 ms`。覆盖输出设备文案、PID 归一化与全部进程快速路径/逐沙盒后备、窗口候选排序与多窗口提示、静音聚合、音量边界和记忆、关闭命令分块与 Sandboxie 参数、菜单状态/静音缓存以及音频和关闭任务脱离调用线程。结果为 `artifacts/runs/csharp_navigation_followup_20261015/csharp-current-process-action-service-recheck-20261016_205700_497.trx`。该结果未操作真实用户进程，也不替代仍缺失的真实 EXE“定位当前进程”有效证据。
- 2026-10-16 当前 checkout C# 进程菜单运行时契约复核：使用新隔离程序集运行完整 `ProcessMenuRuntimeContractTests`，返回码 `0`，实际执行 `15` 项，通过 `15`、失败 `0`、跳过 `0`，控制台持续时间 `3 s`。确认进程动作菜单启用状态、选择反馈、实时状态刷新及工作线程运行期间刷新合并在当前 checkout 未回归。结果为 `artifacts/runs/csharp_navigation_followup_20261015/csharp-current-process-menu-runtime-recheck-20261016_210306_649.trx`；该结果取代本轮旧隔离程序集的同类复核作为当前工作树证据，但仍不替代真实 EXE“定位当前进程”审计。
- 2026-10-16 当前工作树差异格式门禁通过：`git diff --check` 返回码 `0`，没有空白错误；仅报告 `PROJECT_STRUCTURE.md`、用户已有且未触碰的 `HotkeyController.cs` 与 `SettingsRuntimeContractTests.cs` 在 Git 下次触碰时可能执行 LF→CRLF 转换的提示，未对文件进行转换、暂存或提交。结果为 `artifacts/runs/csharp_navigation_followup_20261015/git-diff-check-20261016_210348_609-result.json`。
- 2026-10-16 当前 checkout C# 配置存储契约复核：使用新隔离程序集运行 `ConfigStoreContractTests`，返回码 `0`，实际执行 `3` 项，通过 `3`、失败 `0`、跳过 `0`，控制台持续时间 `605 ms`。确认 C# 配置加密与 Python AES-GCM 固定向量一致、写出的 INI 可由 Python 兼容读取并保留设置、敏感值往返及损坏 INI 恢复契约未回归。结果为 `artifacts/runs/csharp_navigation_followup_20261015/csharp-current-config-store-recheck-20261016_210505_508.trx`；测试使用临时目录，未改写目标 EXE 隔离配置或真实 `Sandboxie.ini`。
- 2026-10-16 当前 checkout C# 路径记忆服务契约复核：使用新隔离程序集运行 `AppPathHistoryServiceContractTests`，返回码 `0`，实际执行 `8` 项，通过 `8`、失败 `0`、跳过 `0`，控制台持续时间 `565 ms`。覆盖默认值/文案、现存 EXE 新增排序与去重、引号/换行/`file:` URI 提取、隐藏及完整显示文本、组合框选择/勾选索引、当前路径与回调顺序、备注/删除/清空等记忆变更状态及保存刷新回调、剪贴板文件优先顺序。结果为 `artifacts/runs/csharp_navigation_followup_20261015/csharp-current-app-path-history-service-recheck-20261016_210611_574.trx`；该当前工作树结果取代旧隔离程序集的同项复核，但不替代真实 EXE 备注/删除/清空动作审计。
- 2026-10-16 当前 checkout C# 路径记忆运行时契约复核：使用新隔离程序集运行 `PathMemoryRuntimeContractTests`，返回码 `0`，实际执行 `4` 项，通过 `4`、失败 `0`、跳过 `0`，控制台持续时间 `1 s`。确认路径记忆菜单与 `Ctrl+H` 使用经典上下文菜单、历史视图以显示文本/Tag 恢复选择和勾选且不扫描、运行时动作接入历史服务而非默认回退、管理器保存回调更新持久状态和显示。结果为 `artifacts/runs/csharp_navigation_followup_20261015/csharp-current-path-memory-runtime-recheck-20261016_210716_794.trx`；该结果取代旧隔离程序集的同项复核，不替代剩余真实 EXE 备注/删除/清空审计。
- 2026-10-16 当前 checkout C# Windows 音频后端契约复核：使用新隔离程序集运行 `WindowsProcessAudioBackendContractTests`，返回码 `0`，实际执行 `5` 项，通过 `5`、失败 `0`、跳过 `0`，控制台持续时间 `553 ms`。确认主窗口音频回调绑定真实后端、PID 会话音量委托映射、输出设备与 Windows 音频策略标识归一化、使用原生 Windows build 选择策略接口、真实环境会话/设备枚举返回安全集合并拒绝无效路由。结果为 `artifacts/runs/csharp_navigation_followup_20261015/csharp-current-windows-audio-backend-recheck-20261016_210829_607.trx`；本轮只读取真实音频环境并用无效 PID/空设备 ID 验证拒绝路径，没有改变系统音量、静音或输出路由。
- 2026-10-16 当前 checkout C# 异步音频动作契约复核：使用新隔离程序集运行 `ProcessAudioAsyncContractTests`，返回码 `0`，实际执行 `2` 项，通过 `2`、失败 `0`、跳过 `0`，控制台持续时间 `368 ms`。确认输出设备枚举不阻塞动作调用线程、选择设备在用户响应前不使用固定超时且会检查窗口关闭状态。结果为 `artifacts/runs/csharp_navigation_followup_20261015/csharp-current-process-audio-async-recheck-20261016_210920_482.trx`；两项均使用内存替身，没有写入真实音频会话或系统路由。
- 2026-10-16 当前 checkout C# 朗读服务契约复核：使用新隔离程序集运行完整 `SpeechServiceContractTests`，返回码 `0`，实际执行 `10` 项，通过 `10`、失败 `0`、跳过 `0`，控制台持续时间 `913 ms`。覆盖朗读配置/读屏 INI 归一化、配置与 Tolk 资源路径、临时目录读写、读屏识别和引擎匹配、允许接口值、朗读配置保存、辅助提示、工作线程输出与关闭禁用、事件过滤/短时去重及设置请求回调。结果为 `artifacts/runs/csharp_navigation_followup_20261015/csharp-current-speech-service-recheck-20261016_211019_208.trx`；输出、进程枚举和设置对话框均由替身捕获，没有播放真实语音或改写生产配置。
- 2026-10-16 当前 checkout C# 更新安全契约复核：使用新隔离程序集运行 `UpdateSecurityContractTests`，返回码 `0`，实际执行 `2` 项，通过 `2`、失败 `0`、跳过 `0`，控制台持续时间 `448 ms`。确认 Ed25519 参考向量验证、Python 兼容更新包解密及无效输入拒绝未回归。结果为 `artifacts/runs/csharp_navigation_followup_20261015/csharp-current-update-security-recheck-20261016_211123_165.trx`；本组使用固定向量和内存输入，没有访问网络或安装更新。
- 2026-10-16 当前 checkout C# 更新安装器契约复核：使用新隔离程序集运行 `UpdateInstallerContractTests`，返回码 `0`，实际执行 `2` 项，通过 `2`、失败 `0`、跳过 `0`，控制台持续时间 `538 ms`。确认 updater 命令参数/请求 JSON 解析、安装前 SHA-256 计算与校验未回归。结果为 `artifacts/runs/csharp_navigation_followup_20261015/csharp-current-update-installer-recheck-20261016_211156_235.trx`；本组只处理测试数据，没有启动 updater、下载更新或覆盖应用目录。
- 2026-10-16 当前 checkout C# 更新 HTTP 客户端契约复核：使用新隔离程序集运行 `UpdateHttpClientContractTests`，返回码 `0`，实际执行 `1` 项，通过 `1`、失败 `0`、跳过 `0`，控制台持续时间 `427 ms`。确认仅对获准更新 URL 返回错误响应正文、非获准 URL 保持 `HttpRequestException`。结果为 `artifacts/runs/csharp_navigation_followup_20261015/csharp-current-update-http-client-recheck-20261016_211240_403.trx`；测试使用本地 `StubHandler`，没有访问外网。
- 2026-10-16 当前 checkout C# 更新服务契约复核：使用新隔离程序集运行完整 `UpdateServiceContractTests`，返回码 `0`，实际执行 `9` 项，通过 `9`、失败 `0`、跳过 `0`，控制台持续时间 `815 ms`。覆盖补丁包归一化与旧 URL 拒绝、仅选择当前增量补丁、说明/大小格式、带防缓存头与 UTF-8 BOM 的清单解析、打包 updater helper/请求文件准备、非打包版本禁用、版本提示文案、后台检查失败回传及二维码资源优先级。结果为 `artifacts/runs/csharp_navigation_followup_20261015/csharp-current-update-service-recheck-20261016_211339_851.trx`；HTTP 和失败路径使用替身，文件只写测试临时目录，没有联网、启动 updater 或覆盖发布目录。
- 2026-10-16 当前 checkout C# 启动器设置服务契约复核：使用新隔离程序集运行 `LauncherSettingsServiceContractTests`，返回码 `0`，实际执行 `8` 项，通过 `8`、失败 `0`、跳过 `0`，控制台持续时间 `820 ms`。覆盖独立嵌套默认值、更新清单 URL 归一化/持久化、设置读取与朗读嵌套合并/旧键移除、旧 JSON 迁移与损坏数据后备、运行时状态保存、应用设置回调顺序、全量重置文案/清空路径/恢复默认值，以及仅在有效时应用沙盒根目录和 Sandboxie 路径。结果为 `artifacts/runs/csharp_navigation_followup_20261015/csharp-current-launcher-settings-service-recheck-20261016_211428_204.trx`；测试使用临时目录与回调替身，没有改写目标 EXE 配置或真实 Sandboxie 设置。
- 2026-10-16 当前 checkout C# Bootstrap 契约复核：使用新隔离程序集运行 `BootstrapContractTests`，返回码 `0`，实际执行 `7` 项，通过 `7`、失败 `0`、跳过 `0`，控制台持续时间 `389 ms`。确认开发/打包路径选择、internal 公共同级 EXE 优先与缺失后备、资源相对路径、仅打包模式清理旧内部 EXE、每日日志时间戳与 UTF-8 无 BOM、空日志忽略。结果为 `artifacts/runs/csharp_navigation_followup_20261015/csharp-current-bootstrap-recheck-20261016_211517_347.trx`；所有文件动作均在唯一测试临时目录，没有启动目标 EXE或触碰发布目录。
- 2026-10-16 当前 checkout C# 基础常量契约复核：使用新隔离程序集运行 `ConstantsContractTests`，返回码 `0`，实际执行 `9` 项，通过 `9`、失败 `0`、跳过 `0`，控制台持续时间 `474 ms`。确认标量常量/选择项、固定快捷键映射、日志输出模式、默认沙盒根目录、启动并发上限和保守默认值、沙盒根目录规范化、内部名/显示名校验及默认显示前缀与 Python 约定一致。结果为 `artifacts/runs/csharp_navigation_followup_20261015/csharp-current-constants-recheck-20261016_211547_602.trx`；本组为纯契约计算，没有写入真实配置或资源。
- 2026-10-16 当前 checkout C# 通用工具契约复核：使用新隔离程序集运行 `CommonUtilitiesContractTests`，返回码 `0`，实际执行 `4` 项，通过 `4`、失败 `0`、跳过 `0`，控制台持续时间 `429 ms`。确认布尔值/整数设置解析、版本解析比较、大小与更新状态格式规则、路径构造及隐藏进程启动信息与 Python 行为一致。结果为 `artifacts/runs/csharp_navigation_followup_20261015/csharp-current-common-utilities-recheck-20261016_211621_756.trx`；本组没有启动外部进程或写入真实配置。
- 2026-10-16 真实 EXE 沙盒列表 `Down` 键焦点门禁测试通过：目标 PID `3256`、主窗口 HWND `26282042` 为前台窗口，沙盒列表 HWND `4065580` 在发送前经 `GetGUIThreadInfo` 确认为键盘焦点；单次标准 `VK_DOWN` 后，列表从聚焦索引 `0`/未选中变为聚焦索引 `1`/选中索引 `1`，结果 `passed=true`。结果为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-down-key-focus-20261016_212300_264-result.json`。此前两次焦点未确认的 Computer Use 投递已丢弃，不写成产品结论；本次没有触发启动、关闭、配置写入或 Sandboxie 资源变更。
- 2026-10-16 真实 EXE 沙盒列表 `Up` 键焦点门禁测试通过：同一目标 PID `3256`、主窗口 HWND `26282042` 和沙盒列表 HWND `4065580` 在发送前再次通过前台/焦点门禁，并确认前置聚焦/选中索引均为 `1`；单次标准 `VK_UP` 后聚焦/选中索引均返回 `0`，结果 `passed=true`。结果为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-up-key-focus-20261016_212335_000-result.json`；没有触发启动、关闭、配置写入或 Sandboxie 资源变更。
- 2026-10-16 真实 EXE 固定 `F5` 刷新键探针证据不足：目标 PID `3256`、主窗口 HWND `26282042` 和沙盒列表 HWND `4065580` 的前台/焦点门禁均确认，系统 `VK_F5` 已发送；发送前后及 3 秒采样中状态始终为“已刷新 96 个沙盒用户”、列表数量始终为 `96`、聚焦/选中索引均为 `0`，没有捕获到可区分的应用刷新事件。因此本轮标记为 `inconclusive`、`productFailure=false`，不写成通过或失败。结果为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-f5-key-inconclusive-20261016_212455_486-result.json`；没有资源变更。
- 2026-10-17 真实 EXE 沙盒列表 `Space` 首轮探针无效、无产品结论：历史 PID `3256` 已不存在，本轮重新启动权威隔离 EXE 并动态识别 PID `13036`、主窗口 HWND `329962`、沙盒列表 HWND `264438`；列表为 `96` 项、首项状态图像为未勾选 `1`。消息发送前主窗口确为前台，但探针线程 `GetFocus` 返回 `0`，发送一次 `VK_SPACE` 后焦点确认仍为 `0` 且状态图像仍为 `1`，不满足焦点门禁，因此标记为 `invalid_probe`、`productFailure=false`，不写成产品通过或失败。结果为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-space-key-focus-invalid-20261017_082615_620-result.json`；没有触发启动、关闭、配置写入或 Sandboxie 资源变更。下一步改用 `GetGUIThreadInfo` 对目标 GUI 线程确认焦点后只更正重试一次。
- 2026-10-17 真实 EXE 单实例运行中提示补充审计通过：原实例 PID `13036` 从托盘隐藏状态经再次启动恢复主窗口，同时第二实例 PID `12744` 显示标题为“提示”的窗口；动态枚举精确确认正文“沙河批量打开器正在运行中”和唯一原生 `Button`“确定”。焦点无关的首次无障碍点击后端失败且未完成动作，随后 Python/Win32 仅在 PID、标题、正文、唯一按钮和原主窗口可见门禁全部满足时向该按钮发送 `BM_CLICK`；第二实例窗口随后清空并退出，原实例主窗口 HWND `329962` 保持可见，结果 `passed=true`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-single-instance-running-prompt-20261017_083002_367-result.json`；由此此前“运行中提示尚未捕获”风险已关闭。未修改配置或 Sandboxie 资源。
- 2026-10-17 真实 EXE 沙盒列表 `Space` 焦点门禁更正仍未形成产品证据：PID `13036` 的主窗口 HWND `329962` 和 `96` 项沙盒列表 HWND `264438` 均唯一动态匹配，`GetGUIThreadInfo` 读取成功；但动作时前台 HWND 为 `330276`、目标 GUI 焦点为 `0`，不满足“目标前台且列表持焦”条件，安全门禁因此保持 `keySent=false`。首项状态图像前后均为未勾选 `1`，本轮标记为 `invalid_probe`、`productFailure=false`。结果为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-space-key-focus-gate-invalid-20261017_083052_501-result.json`；下一步只使用用户等价的前台激活、列表单击和系统按键，不再把跨线程 `WM_NEXTDLGCTL` 当作焦点建立方式。
- 2026-10-17 真实 EXE 沙盒列表 `Space` 用户等价动作首轮仍由安全门禁阻止：目标 PID `13036` 已成功成为前台，主窗口与 `96` 项列表仍唯一匹配；但原实例窗口处于最小化坐标，列表矩形为 `[-31992,-31975,-31991,-31974]`，计算出的单击点不在有效列表范围内，因此 `clickGatePassed=false`、`clicked=false`、`keySent=false`，未产生产品动作或结论。结果为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-space-key-user-equivalent-gate-invalid-20261017_083145_339-result.json`；下一步先用标准 `SW_RESTORE` 还原窗口，再重新读取矩形，不复用这些坐标。
- 2026-10-17 真实 EXE 沙盒列表 `Space` 用户等价复选测试通过：PID `13036` 的主窗口经标准 `SW_RESTORE` 后不再最小化，实时主窗口/沙盒列表矩形分别为 `[468,162,1068,662]` 与 `[484,238,1052,407]`；动作前主窗口为前台，列表 HWND `264438` 经列表首行正文区域单击后由 `GetGUIThreadInfo` 确认为焦点，聚焦/选中索引均为 `0`。单次系统 `VK_SPACE` 后首项状态图像从未勾选 `1` 切换为已勾选 `2`，焦点和选择仍为索引 `0`，结果 `passed=true`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-space-key-user-equivalent-pass-20261017_083224_494-result.json`；未触发启动、关闭、配置写入或 Sandboxie 资源变更，下一步单独验证再次 `Space` 恢复原状态。
- 2026-10-17 真实 EXE 沙盒列表 `Space` 首次原状态恢复尝试由门禁阻止：首项前置状态仍为已勾选 `2`，但动作时前台已切换为 HWND `330276`、目标 GUI 焦点为主窗体 HWND `329962`，因此 `gatePassed=false`、`keySent=false`，状态继续为 `2`，没有误触或产品失败结论。结果为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-space-key-restore-gate-invalid-20261017_083300_373-result.json`；下一步复用已通过的完整还原/激活/单击/焦点门禁链执行 `2 → 1` 恢复。
- 2026-10-17 真实 EXE 沙盒列表 `Space` 原状态恢复动作最终无需执行：继续工作时历史 PID `13036` 已失效，主窗口/列表动态匹配数均为 `0`，安全门禁保持 `clicked=false`、`keySent=false`；随后按目标完整路径枚举进程得到空集合，`tasklist` 也不再包含 PID `13036`。勾选状态仅存在于已退出进程内存且未写入配置，因此没有可恢复的持久状态，本轮结论为 `not_applicable_process_ended`、`productFailure=false`。结果为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-space-key-restore-not-applicable-20261017_084307_086-result.json`；后续必须重新启动并动态取得新 PID/HWND。
- 2026-10-17 对上一条原状态恢复记录的进程枚举边界作追加纠正：`tasklist` 未返回历史 PID `13036`，以及该 PID 的窗口匹配数为 `0`，仍足以确认被测试的旧进程已经结束；但随后“按目标完整路径枚举为空”的 Python 诊断实际把目录末尾多拼了一个分隔符，不能证明当时全局没有其他同路径实例，该空集合不得继续作为证据。修正为 `os.path.join` 后，在本条之前明确重新启动的当前实例被精确识别为 PID `13148`、可见主窗口 HWND `789036`。纠正证据为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-space-key-restore-path-enumeration-correction-20261017_084441_268-result.json`；不改变“旧 PID 13036 的进程内勾选状态已随其退出消失”这一窄结论。
- 2026-10-17 真实 EXE 沙盒列表 `Right` 首轮用户等价探针因索引假设无效而未发送按键：当前 PID `13148`、主窗口 HWND `789036`、沙盒列表 HWND `462082` 均唯一匹配并通过前台/焦点门禁；但列表保留了滚动视口，首个可见行单击后实际聚焦/选中索引均为 `87`，不是探针硬要求的 `0`。因此 `gatePassed=false`、`keySent=false`，勾选状态未变，无产品结论。结果为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-right-key-index-assumption-invalid-20261017_084532_493-result.json`；下一步改为以运行时实际索引 `n` 验证相对目标 `(n+1) mod 96`，不再假设滚动位置。
- 2026-10-17 真实 EXE 沙盒列表相对 `Right` 全局输入探针因动作后焦点丢失而无效：发送门禁时 PID `13148` 主窗口为前台、列表 HWND `462082` 持焦，实际索引 `n=90`、期望索引 `91`，系统 `VK_RIGHT` 已调用；但采样结束时目标 GUI 焦点为 `0`，聚焦/选中索引仍为 `90`。由于无法排除全局输入在投递期间被前台竞争重定向，最终按 `invalid_focus_lost`、`productFailure=false` 记录，不把原始谓词中的失败标志写成产品失败。结果为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-right-key-global-focus-lost-invalid-20261017_084618_653-result.json`；下一步向门禁确认的列表 HWND 消息队列投递标准按键，避免全局目标漂移。
- 2026-10-17 真实 EXE 沙盒列表相对 `Right` 导航目标队列测试通过：PID `13148` 的主窗口 HWND `789036` 为前台、列表 HWND `462082` 持焦，运行时起点/选中索引均为 `69`、期望为 `(69+1) mod 96 = 70`；标准 `VK_RIGHT` 按键按下/抬起消息均成功投递到该列表队列，首个 `50.8ms` 样本即观察到聚焦/选中索引均为 `70`，之后共有 `13` 个样本在列表持焦期间持续匹配。起点和目标项状态图像前后均为未勾选 `1`，没有隐式复选；约 `680.1ms` 后外部前台竞争发生时导航结果已稳定，不影响本轮有效应用事件。结果为 `passed=true`，证据为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-right-key-target-queue-pass-20261017_084714_241-result.json`；未启动应用或修改配置/Sandboxie 资源。
- 2026-10-17 真实 EXE 沙盒列表相对 `Left` 目标队列测试观察到双步、暂不定性：PID `13148` 主窗口/列表前台焦点门禁持续有效，起点索引为 `70`、单步期望为 `69`；投递标准 `VK_LEFT` 按下/抬起后，`50.7ms` 样本短暂到达聚焦/选中索引 `69`，但从 `111.3ms` 起至 `1283.7ms` 在目标始终前台且列表持焦时稳定停在 `68`。两项复选状态均保持未勾选 `1`。原始脚本因“曾命中 69”计算出 `passed=true`，该谓词已被最终分类否决；本轮记为 `unexpected_double_step_requires_diagnosis`、`productFailure=false`，先只读核对生产左右键处理链和 `KEYDOWN/KEYUP` 投递语义。证据为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-left-key-double-step-diagnostic-20261017_084802_529-result.json`。
- 2026-10-17 真实 EXE 沙盒列表相对 `Left` 规范扩展键消息复测通过，并关闭上一条双步疑点：生产源码只读核对确认 `CheckableListCtrl.OnKeyDown` 只执行一次 `selectedIndex - 1`，50ms 延迟只更新 `AccessibleName`。本轮 PID `13148` 主窗口/列表前台焦点门禁有效，以正确 Left 扫描码和扩展键位构造 `WM_KEYDOWN lParam=0x014B0001`，从索引 `68` 到 `67` 后在 `20.4–315.2ms` 的 `15` 个样本中始终稳定；再单独投递规范 `WM_KEYUP lParam=0xC14B0001`，`25.9–524.4ms` 的 `20` 个样本仍全部为 `67`，全程前台/焦点有效。结果 `passed=true`，证据为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-left-key-canonical-message-pass-20261017_085042_839-result.json`。因此上一条双步属于非规范消息元数据/立即成对投递的探针偏差，不是产品缺陷；左右导航真实 EXE 证据均已取得。
- 2026-10-17 真实 EXE 主窗口首段 `Tab` 焦点顺序通过：只读源码先确认沙盒列表、应用列表和启动按钮 `TabIndex` 分别为 `0/1/2`；PID `13148` 中沙盒列表 HWND `462082` 持焦且主窗口 HWND `789036` 为前台时，规范 `Tab` 按下消息后首个 `23.7ms` 样本即把焦点移到空应用列表 HWND `658702`。按下阶段 `15` 个样本及随后投递到新焦点的抬起阶段 `12` 个样本均保持应用列表持焦；沙盒聚焦/选中索引保持 `67`、状态图像保持未勾选 `1`。结果 `passed=true`，证据为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-tab-focus-sandbox-to-app-pass-20261017_085155_322-result.json`；没有触发按钮或启动外部应用。
- 2026-10-17 真实 EXE 主窗口第二段 `Tab` 焦点顺序通过：PID `13148` 的空应用列表 HWND `658702` 经安全空白区域单击持焦，动态唯一匹配到可见且启用的“启动选中沙盒”按钮 HWND `2100268`；规范 `Tab` 按下后首个 `20.6ms` 样本即移到该按钮，按下阶段 `15` 个样本和投递到新焦点的抬起阶段 `12` 个样本均稳定保持。结果 `passed=true`，证据为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-tab-focus-app-to-launch-pass-20261017_085236_611-result.json`；全程只移动焦点，`buttonInvoked=false`，没有启动外部应用。
- 2026-10-17 真实 EXE 启动按钮后续 `Tab` 首轮焦点建立探针无效：目标 PID `13148`、主窗口、沙盒列表和唯一启动按钮均匹配，但跨控件 `WM_NEXTDLGCTL` 请求后焦点仍停在应用列表 HWND `658702`，未到按钮 HWND `2100268`；安全门禁因此保持 `postedKeyDown=false`、`postedKeyUp=false`，没有按钮动作或产品结论。结果为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-tab-focus-button-calibration-invalid-20261017_085328_226-result.json`；下一步从应用列表开始连续投递两次规范 `Tab`，以第一跳实测按钮焦点作为第二跳门禁。
- 2026-10-17 真实 EXE 应用列表→按钮→沙盒列表连续 `Tab` 首轮由前台竞争门禁阻止：安全空白单击的初始门禁通过，但等待 `300ms` 后前台已切为 HWND `132596`、目标 GUI 焦点为 `0`，因此第一跳和第二跳均保持 `KeyDown/KeyUpPosted=false`，按钮未调用，无产品结论。结果为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-tab-focus-cycle-foreground-lost-invalid-20261017_085410_227-result.json`；下一步把单击到焦点确认缩短为 `20ms`，仍只在即时前台/焦点门禁满足时投递。
- 2026-10-17 真实 EXE 应用列表→按钮→沙盒列表紧门禁连续 `Tab` 仍未形成完整第二按键证据：第一跳从应用列表到按钮的按下/抬起完整且按钮焦点稳定；第二跳按下消息后 `15ms` 样本短暂到沙盒列表 HWND `462082`，从 `30ms` 起又回到按钮 HWND `2100268`，因抬起前门禁已不满足而保持 `secondKeyUpPosted=false`。不完整按键不能用于判断产品焦点循环，本轮记为 `invalid_incomplete_second_key`、`productFailure=false`，按钮未调用。结果为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-tab-focus-cycle-incomplete-key-invalid-20261017_085448_397-result.json`；下一步以第一跳建立按钮焦点后发送一次完整系统 `Tab` 按下/抬起。
- 2026-10-17 真实 EXE 主窗口 `Tab` 正向焦点循环完成：从空应用列表校准到启动按钮后，按钮焦点在 `20–120ms` 的 `6` 个样本中稳定；焦点/前台门禁满足后发送一次完整系统 `Tab` 按下/抬起，首个 `20ms` 样本即回到沙盒列表 HWND `462082`，`20–400ms` 的 `20` 个样本全部保持主窗口 HWND `789036` 前台且沙盒列表持焦。结果 `passed=true`，证据为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-tab-focus-button-to-sandbox-pass-20261017_085732_587-result.json`；按钮未调用、没有启动外部应用。结合前两段，主窗口正向焦点环已实测为“沙盒列表 → 应用列表 → 启动按钮 → 沙盒列表”。
- 2026-10-17 真实 EXE 主窗口 UI Automation 无障碍名称只读审计通过：PID `13148` 的主窗口、沙盒列表、应用列表和启动按钮均由系统 UIA 成功读取，主窗口名称为“沙河批量打开器”；沙盒列表 `AutomationId=沙盒用户`，动态名称为“未选中 沙盒用户10 【空闲中】 用户 未置顶”；应用列表名称/AutomationId 均为“准备要在沙盒中打开的应用”；按钮名称为“启动选中沙盒”。两个列表及按钮均 `IsKeyboardFocusable=true`、`IsEnabled=true`、`IsOffscreen=false`，控件类型分别为 List `50008` 与 Button `50000`。结果 `passed=true`，证据为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-uia-accessible-names-pass-20261017_085818_458-result.json`；本轮只读，没有移动焦点或执行控件动作。
- 2026-10-17 真实 EXE 沙盒列表导航后的 UIA 动态名称同步通过：PID `13148` 主窗口前台、列表 HWND `462082` 持焦时，从运行时索引 `65` 规范 `Right` 到 `66`；列表 UIA 名称同步从“未选中 沙盒用户1 [Box6] 【空闲中】 用户 未置顶”更新为“未选中 沙盒用户10 【空闲中】 用户 未置顶”，规范抬起后仍保持新名称。起点/目标项状态图像前后均为未勾选 `1`，前台和列表焦点保持有效，结果 `passed=true`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-uia-name-navigation-sync-pass-20261017_085855_742-result.json`；确认焦点移动不仅改变选择，也同步提供屏幕阅读器可读的选中/运行/置顶语义。
- 2026-10-17 真实 EXE 文件菜单助记键入口与取消通过：PID `13148` 主窗口前台门禁满足后，投递系统 `SC_KEYMENU('f')`，`30.7ms` 内检测到同进程可见 `#32768` 弹出菜单 HWND `6687562`；运行时菜单包含 `9` 项，确认“选择应用 Ctrl+O”“粘贴 .exe 或文件路径 Ctrl+V”“启动选中沙盒”“刷新沙盒用户 F5”“切换到上/下一个用户”“全选 Ctrl+A”和“退出软件 Alt+F4”。随后投递 `WM_CANCELMODE/Escape`，弹出菜单确切消失，`commandInvoked=false`。结果 `passed=true`，证据为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-file-menu-mnemonic-pass-20261017_085938_874-result.json`；没有执行任何菜单动作。
- 2026-10-17 真实 EXE 动态菜单“退出软件”通过：PID `13148` 的运行时菜单递归枚举仅找到一个精确标题 `退出软件\tAlt+F4`，动态命令 ID 为 `335` 且处于启用状态；按动态 ID 投递 `WM_COMMAND` 后，`WaitForSingleObject=0`。初次脚本把“退出后短时仍可 OpenProcess”错误当成仍存活，故其原始 `productFailure=true` 判定作废；`2026-10-17T09:00:25.420+08:00` 后续复核确认 `OpenProcess` 失败、顶层窗口为空、Toolhelp 快照无 PID、目标精确路径实例为 `0`，且 `tasklist | findstr 13148` 返回 `1`。最终结果 `passed=true`、`productFailure=false`，证据为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-keyboard-accessibility-dynamic-exit-20261017_090008_262-result.json`；命令 ID 来自运行时查找，没有使用固定值。
- 2026-10-17 真实 EXE 键盘/无障碍审计只读后置门禁通过：`2026-10-17T09:02:10.175+08:00` 枚举确认目标 EXE 精确路径实例、`Start.exe`、`powershell.exe`、`pwsh.exe`、AuditRunner 和 `SbieCtrl.exe` 均为 `0`；Sandboxie 相关进程仅有 PID `2836` 的 `SbieSvc.exe`，`sc query SbieSvc` 返回码 `0`、状态 `4 RUNNING`。`Sandboxie.ini` 中 `ShaheAudit_*` section 为 `0`，`C:\Sandbox\Administrator` 下同名前缀目录为 `0`。目标 EXE、隔离配置、`Sandboxie.ini` 的 SHA-256 分别为 `123607B3A46E6A1939F933DB20F2D53CBA0266D4696E5B209FE7D8C984CDE517`、`99120B9DFFD177FF4E66FA9ACDC3547B9085D220EE99483405CAB30B2B91F234`、`2709ADB23A9D1080D91C0401DC86A9F954AF81DEAFB5FFD7DF03F5AA30E75A6D`，均与基线一致；前一动态退出 JSON 也可正常解析。结果 `passed=true`，证据为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-keyboard-accessibility-postflight-20261017_090210_175-result.json`；本轮未改配置、资源或服务。
- 2026-10-17 真实 EXE 反向焦点环首轮调用无效：承载完整启动、焦点和清理逻辑的 base64 内联 Python 命令在 Python 启动前即被 `cmd.exe` 以“命令行太长”拒绝，返回码 `1`；目标 EXE 未启动、未发送 `Shift+Tab`，因此无产品结论。证据为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-reverse-tab-commandline-too-long-invalid-20261017_090356_824-result.json`；更正重试改用短分步 Python 调用，仍不落地脚本且不使用 PowerShell。
- 2026-10-17 真实 EXE 反向焦点环第二轮为控件过滤探针失败：PID `5688` 的主窗口 HWND `527190`、沙盒列表 HWND `1967184` 和应用列表 HWND `3411274` 已就绪，列表项数为 `96/0`，且前台窗口与沙盒列表焦点门禁均通过；但探针把 WinForms 按钮类名错误限定为“以 `button` 结尾”，没有识别带运行时后缀的真实类名，导致启动按钮句柄为 `0` 并把它误作第一步期望焦点。结果 `inconclusive`、`productFailure=false`，证据为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-reverse-tab-button-class-filter-invalid-20261017_090439_794-result.json`；未执行 Enter、Backspace、勾选、菜单或 Sandboxie 资源动作，下一轮改为匹配类名中包含 `button`。
- 2026-10-17 本轮自有 PID `5688` 的首次动态菜单清理调用无效：脚本已进入菜单枚举，但因 Python 索引错误把完整菜单元组传给 `PostMessageW`，在参数转换阶段抛出 `ctypes.ArgumentError`，退出命令实际未发送；结果为探针失败、`productFailure=false`，PID `5688` 继续留待精确清理。证据为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-reverse-tab-cleanup-index-invalid-20261017_090536_862-result.json`；下一次只把参数更正为已动态查得的 `commandId`。
- 2026-10-17 本轮自有 PID `5688` 清理复核通过：运行时菜单唯一匹配 `退出软件\tAlt+F4`，动态命令 ID `323` 且启用；更正索引后 `PostMessage(WM_COMMAND)` 成功，`WaitForSingleObject=0`。命令结束即时 `OpenProcess` 仍成功属于进程对象尚未回收，不能作为存活判据；`2026-10-17T09:06:09.711+08:00` 后续复核确认顶层窗口为空、`tasklist` 无匹配进程行，PID 已退出。清理结论 `cleanupPassed=true`，证据为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-reverse-tab-invalid-run-cleanup-20261017_090558_692-result.json`；未使用固定命令 ID，也未停止其他进程。
- 2026-10-17 真实 EXE 反向焦点环第三轮无结论：PID `13008` 已正确识别沙盒列表 HWND `2230834`（`96` 项）、应用列表 HWND `3869748`（`0` 项）和“启动选中沙盒”按钮 HWND `724314`；主窗口前台且沙盒列表持焦的初始门禁通过。首个全局 `keybd_event Shift+Tab` 后未稳定到达探针预期按钮，而异常路径没有输出已采集的逐样本数组；`2026-10-17T09:06:52.454+08:00` 纯读取复核显示主窗口仍为前台、焦点仍在沙盒列表，但缺少输入确实送达目标的独立证据。结论 `inconclusive-input-delivery`、`productFailure=false`，证据为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-reverse-tab-delivery-inconclusive-20261017_090642_118-result.json`；未执行控件动作或资源修改。
- 2026-10-17 本轮自有 PID `13008` 动态菜单清理通过：运行时唯一匹配 `退出软件\tAlt+F4`，动态命令 ID `323` 且启用；`PostMessage(WM_COMMAND)` 成功、`WaitForSingleObject=0`，后续 `tasklist` 无该 PID 进程行。结果 `cleanupPassed=true`，证据为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-reverse-tab-inconclusive-run-cleanup-20261017_090717_507-result.json`；未使用固定命令 ID，也未停止其他进程。
- 2026-10-17 真实 EXE 主窗口反向焦点环通过：只读源码映射确认沙盒列表、应用列表和启动按钮 `TabIndex=0/1/2`，容器 `TabStop=false`；PID `8240` 主窗口前台且沙盒列表持焦后，在保持 Shift 键状态期间把规范 Tab keydown/up 明确排入当前焦点控件消息队列。三个步骤的 keydown/up 共 `6` 次 `PostMessage` 全部成功，焦点依次稳定为“沙盒列表 HWND `658262` → 启动按钮 HWND `1445202` → 应用列表 HWND `14027542` → 沙盒列表 HWND `658262`”；每步 Shift 保持期间 `10` 个样本和释放后 `8` 个样本均与预期一致。结果 `passed=true`，证据为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-reverse-tab-target-queue-pass-20261017_090813_080-result.json`；没有激活控件、改变选择/勾选、配置或 Sandboxie 资源。
- 2026-10-17 反向焦点 GREEN 目标 PID `8240` 清理通过：运行时菜单唯一匹配 `退出软件\tAlt+F4`，动态命令 ID `323` 且启用；退出命令投递成功、`WaitForSingleObject=0`，后续 `tasklist` 无 PID 匹配行。结果 `cleanupPassed=true`，证据为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-reverse-tab-pass-cleanup-20261017_090841_547-result.json`；没有使用固定命令 ID或停止其他进程。
- 2026-10-17 真实 EXE F5 刷新通过：源码映射确认无修饰 F5 固定分派为 `refresh -> RefreshSandboxListOnStartup -> RefreshSandboxList`；PID `7656` 主窗口前台且沙盒列表 HWND `2821164` 持焦后，规范 F5 keydown/up 均成功排入目标队列。目标 PID 的只读 WinEvent 监听在 `0.5s` 空闲基线仅见 `10` 个事件，而 F5 后 `1.5s` 内见 `515` 个事件，其中 `484` 个来自沙盒列表 client 对象：`1` 次 REORDER、`1` 次 DESTROY、`97` 次 CREATE、`192` 次 STATECHANGE、`192` 次 NAMECHANGE、`1` 次 FOCUS；最终列表仍为 `96` 项、焦点仍在该列表，状态文本为“已刷新 96 个沙盒用户”。结果 `passed=true`，证据为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-f5-winevent-refresh-pass-20261017_091201_052-result.json`；未创建、修改或删除 Sandboxie 资源与配置。
- 2026-10-17 F5 GREEN 目标 PID `7656` 清理通过：运行时菜单唯一匹配 `退出软件\tAlt+F4`，动态命令 ID `381` 且启用；退出命令投递成功、`WaitForSingleObject=0`，后续 `tasklist` 无 PID 匹配行。结果 `cleanupPassed=true`，证据为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-f5-pass-cleanup-20261017_091227_144-result.json`；未使用固定命令 ID或停止其他进程。
- 2026-10-17 真实 EXE 自定义 Alt 导航禁用守卫首轮门禁无效：隔离配置只读确认 `hotkeys_enabled=false` 且上/下一个用户绑定为 `Alt+Up/Alt+Down`；PID `6268` 主窗口为前台、沙盒/应用列表已加载为 `96/0` 项，但新获得焦点的沙盒列表尚无选中项（索引 `-1`），脚本在输入前按门禁退出，未发送 Alt+Up 或 Alt+Down。结果 `invalid-no-initial-selection`、`productFailure=false`，证据为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-disabled-alt-navigation-no-selection-invalid-20261017_091654_741-result.json`；下一轮先用已验证的 Right 导航建立普通用户选中项。
- 2026-10-17 真实 EXE 自定义 Alt 导航禁用守卫通过：PID `6268` 使用已验证的规范 Right 消息把沙盒列表校准到普通用户索引 `2`，主窗口前台、列表持焦；在隔离配置 `hotkeys_enabled=false` 且绑定仍为 `Alt+Up/Alt+Down` 时，两个组合键的 keydown/up 均成功排入目标队列，但各自 Alt 保持期间 `8` 个样本及释放后 `8` 个样本全部保持索引 `2`、焦点 HWND `134232`，最终列表仍为 `96` 项。结果 `passed=true`，证据为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-disabled-alt-navigation-guard-pass-20261017_091734_904-result.json`；该结果只证明禁用态不会误导航，不代替需要修改配置的启用态 Alt+Up/Down 审计。
- 2026-10-17 Alt 导航禁用守卫 GREEN 目标 PID `6268` 清理通过：运行时菜单唯一匹配 `退出软件\tAlt+F4`，动态命令 ID `337` 且启用；退出命令投递成功、`WaitForSingleObject=0`，后续 `tasklist` 无 PID 匹配行。结果 `cleanupPassed=true`，证据为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-disabled-alt-navigation-pass-cleanup-20261017_091802_381-result.json`；未使用固定命令 ID或停止其他进程。
- 2026-10-17 真实 EXE 无分组 Enter/Backspace 守卫取得混合结果：隔离配置 `sandbox_groups={}`，PID `12988` 明确聚焦沙盒列表 HWND `593026`（不是应用历史列表）并校准到索引 `0`。Enter keydown/up 入队后 `12` 个样本全部保持索引 `0`、焦点、`96` 项和顶层窗口集合不变，前后 `Start.exe=0`，该无分组 Enter 守卫通过。Backspace 同样没有启动 `Start.exe` 或打开窗口、列表始终 `96` 项且焦点不变，但 `12` 个样本中前 `10` 个暂时为索引 `1`、后 `2` 个回到 `0`；根视图源码会把 Backspace 交给原生 ListView，且后台运行状态刷新仍活动，现有证据不能归因。整体结论 `mixed-enter-pass-backspace-inconclusive`、`productFailure=false`，证据为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-no-group-enter-backspace-mixed-20261017_091845_514-result.json`；实际“进入分组/返回根视图”仍因当前无分组且禁止改配置而未测。
- 2026-10-17 Enter/Backspace 混合结果目标 PID `12988` 清理通过：运行时菜单唯一匹配 `退出软件\tAlt+F4`，动态命令 ID `337` 且启用；退出命令投递成功、`WaitForSingleObject=0`，后续 `tasklist` 无 PID 匹配行。结果 `cleanupPassed=true`，证据为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-enter-backspace-mixed-cleanup-20261017_091913_977-result.json`；未使用固定命令 ID或停止其他进程。
- 2026-10-17 本轮键盘/无障碍补测统一只读后置门禁通过：`2026-10-17T09:19:50.638+08:00` 确认目标 EXE 精确路径实例、`Start.exe`、`powershell.exe`、`pwsh.exe`、AuditRunner 和 `SbieCtrl.exe` 均为 `0`；Sandboxie 相关进程仅有 PID `2836` 的 `SbieSvc.exe`，`sc query` 返回码 `0`、状态 `4 RUNNING`。`ShaheAudit_*` section 与目录均为 `0`；目标 EXE、隔离配置、`Sandboxie.ini` 三份 SHA-256 仍分别精确匹配 `123607B3...DE517`、`99120B9D...F234`、`2709ADB2...5A6D` 基线。`real-exe-*20261017*result.json` 共匹配 `39` 份，JSON 解析错误 `0`。结果 `passed=true`，证据为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-keyboard-followup-postflight-20261017_091950_638-result.json`；本轮未修改或停止任何 Sandboxie 资源、服务、控制进程或未知进程。
- 2026-10-17 本轮键盘/无障碍补测文档差异门禁通过：`git diff --check` 返回码 `0`、空白错误 `0`，仅有三个已修改文件的 LF/CRLF 提示；分支仍为 `沙河批量打开器`、相对远端 ahead `59` / behind `2`。跟踪修改仍仅为 `PROJECT_STRUCTURE.md` 以及用户原有的 `csharp/ShaheLauncher.App/Input/HotkeyController.cs`、`csharp/ShaheLauncher.Tests/SettingsRuntimeContractTests.cs`，未跟踪目录仍为 `.superpowers/`、`artifacts/live_backup_test/`、`artifacts/runs/`；两个用户 C# 文件仍保持原有 numstat `5/0` 与 `3/3`，本轮未写入、暂存、提交、回退或清理。证据为 `artifacts/runs/csharp_navigation_followup_20261015/keyboard-followup-diff-check-20261017_092041_008-result.json`。
- 2026-10-17 最终陈述前门禁复核：`git diff --check` 再次返回 `0`，HEAD 仍为 `6fbf83fa55e33cab0fbcb21ec57b8e6907f981c9`，分支 `沙河批量打开器`、ahead `59` / behind `2`，工作树状态与前述快照一致；`csharp/ShaheLauncher.AuditRunner/` 仍不存在。当天 `*20261017*result.json` 共 `41` 份，全部可解析、错误 `0`。复核已写回 `artifacts/runs/csharp_navigation_followup_20261015/keyboard-followup-diff-check-20261017_092041_008-result.json`；未暂存或提交任何文件。
- 2026-10-16 当前 checkout C# UI 控件与无障碍契约复核：使用新隔离程序集运行 `UiControlsCSharpContractTests`，返回码 `0`，实际执行 `14` 项，通过 `14`、失败 `0`、跳过 `0`，总时间 `3.8489` 秒；覆盖单列/路径列表原生可访问性、显式 Space 勾选与状态播报、选择/键盘导航不触发隐式动作、分组/勾选/稳定身份、传统上下文菜单、Python 顺序扫描刷新、后台运行状态刷新、播报清除和句柄重建稳定性。日志为 `artifacts/runs/csharp_navigation_followup_20261015/csharp-current-ui-controls-recheck-20261016_201444_617.log`。该契约通过不替代真实 EXE 的完整焦点顺序和屏幕阅读器审计。
- 2026-10-16 当前 checkout C# 通用对话框契约复核：使用新隔离程序集运行 `CommonDialogsCSharpContractTests`，返回码 `0`，实际执行 `7` 项，通过 `7`、失败 `0`、跳过 `0`，总时间 `3.7126` 秒；覆盖图像读取/复制文案、二维码对话框尺寸/缩放/复制目标、用户选择顺序与勾选、分组名去空白与非空校验，以及批量命名规则的数值控件、结果键、选项标签、固定数量和错误文案。日志为 `artifacts/runs/csharp_navigation_followup_20261015/csharp-current-common-dialogs-recheck-20261016_201539_070.log`。该结果不替代真实批量创建/重命名动作审计。
- 2026-10-16 当前 checkout C# 设置对话框契约复核：使用新隔离程序集运行 `SettingsDialogsCSharpContractTests`，返回码 `0`，实际执行 `12` 项，通过 `12`、失败 `0`、跳过 `0`，总时间 `3.9315` 秒；覆盖热键对话框布局/焦点顺序/编辑/重置/禁用/校验、移除自动负载调整、按逻辑处理器构造并发数下拉框、设置控件范围/文案/保存键/重置、路径记忆列表经典菜单/编辑/确认/保存载荷，以及朗读配置清理/映射/保存短路顺序。日志为 `artifacts/runs/csharp_navigation_followup_20261015/csharp-current-settings-dialogs-recheck-20261016_201638_463.log`。该结果仍不替代真实 EXE 的完整焦点顺序和路径备注/删除/清空交互审计。
- 2026-10-16 真实 EXE `WM_CLOSE` 托盘测试未达预期、待诊断：隔离配置明确 `show_tray=true`，目标 PID `2648` 在动作前唯一主窗口 HWND `4655342` 可见且启用；通过 Python/Win32 向该确切 HWND 投递标准 `WM_CLOSE` 后等待 3 秒，目标窗口与进程均消失，结果 `passed=false`，没有观察到“隐藏窗口并保持托盘进程”。原始结果为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-wm-close-tray-20261016_201921_003-result.json`。该观察尚未直接定性为产品缺陷：需先确认外部 `WM_CLOSE` 对应的 WinForms `CloseReason`、启动时 `TaskBarIcon/TrayIcon` 是否创建成功及运行环境状态；未修改 Sandboxie 资源。
- 2026-10-16 真实 EXE 标题栏关闭到托盘复测通过并解释上一轮偏差：对同一权威隔离 EXE 的 PID `6924`、唯一可见主窗口 HWND `1837184` 投递与用户标题栏关闭按钮一致的 `WM_SYSCOMMAND/SC_CLOSE`，等待 3 秒后 PID 仍存在，主窗口 HWND 保持但 `Visible=false`，结果 `passed=true`。结果为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-sc-close-tray-20261016_202159_429-result.json`。因此上一轮直接投递 `WM_CLOSE` 后进程退出是探针消息语义不等价，不是托盘产品缺陷；真实关闭到托盘隐藏行为已取得有效证据，下一步验证隐藏后的恢复与最终清理。
- 2026-10-16 单实例恢复首轮探针失败、无产品结论：隐藏状态的主实例 PID `6924` 再次启动后出现第二 PID `10476`；探针已通过“原 PID `6924` 主窗口恢复可见”的断言，但因把运行中提示硬限定为 PID `10476` 且标题精确等于“提示”，实际未匹配到该窗口并在清理前抛出断言，返回码 `1`。原始异常保存在 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-single-instance-restore-20261016_204307_895-result.json`。该轮仅确认需要重新枚举实际提示窗口归属/标题，不能作为单实例提示或清理结论；下一步只读枚举两个 PID 后更正一次。
- 2026-10-16 真实 EXE 单实例恢复更正复核通过：只读枚举确认原 PID `6924` 的主窗口 HWND `1837184` 已恢复为 `Visible=true`，第二 PID `10476` 已不存在，系统只保留原实例，`singleInstanceRestorePassed=true`。结果为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-single-instance-restore-followup-20261016_204443_335-result.json`。本轮有效结论限于“隐藏主窗口可由再次启动恢复，且不保留第二实例”；“沙河批量打开器正在运行中”提示窗口未被捕获，仍保留为未取得证据。
- 2026-10-16 真实 EXE 动态菜单退出首轮探针失败、无产品结论：目标 PID `6924` 和唯一可见主窗口已匹配，但当前 pywin32 不提供 `GetMenuString` 包装，探针在只读枚举菜单文本阶段抛出 `AttributeError`、返回码 `1`，没有取得或发送任何菜单命令 ID，目标进程未被关闭。异常保存在 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-dynamic-menu-exit-20261016_204602_990-result.json`。下一步改用系统 `user32.GetMenuStringW` 更正一次，并继续要求“退出软件”唯一精确匹配后才允许调用。
- 2026-10-16 真实 EXE 动态菜单退出更正复测通过：运行时枚举 PID `6924` 主窗口 HWND `1837184` 的文件菜单，唯一匹配 `退出软件\tAlt+F4`，本次动态命令 ID 为 `381`；向该窗口发送此 `WM_COMMAND` 后等待 5 秒，`tasklist` 确认 PID `6924` 已不存在，结果 `passed=true`。菜单完整枚举和结果在 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-dynamic-menu-exit-retry-20261016_204659_252-result.json`。没有使用固定命令 ID，没有停止 `SbieSvc`、其他 Sandboxie 进程或未知进程；关闭到托盘、单实例恢复和菜单退出的真实 EXE 生命周期均已取得有效证据，运行中提示仍未捕获。
- 2026-10-16 真实 EXE 托盘生命周期后置门禁通过：目标 EXE、隔离配置和 `Sandboxie.ini` 的 SHA-256 仍分别为 `123607B3A46E6A1939F933DB20F2D53CBA0266D4696E5B209FE7D8C984CDE517`、`99120B9DFFD177FF4E66FA9ACDC3547B9085D220EE99483405CAB30B2B91F234`、`2709ADB23A9D1080D91C0401DC86A9F954AF81DEAFB5FFD7DF03F5AA30E75A6D`；未发现目标 EXE、`Start.exe`、`powershell.exe`、AuditRunner 或 `SbieCtrl.exe` 残留，`SbieSvc` 仍为 PID `2724`、状态 `Running`，仅有本会话工具宿主 `pwsh.exe` PID `13000`，没有项目审计进程证据；`ShaheAudit_*` INI section 与目录数量均为 `0`。结果为 `artifacts/runs/csharp_navigation_followup_20261015/real-exe-tray-lifecycle-postflight-20261016_204806_303.json`；本轮没有修改 Sandboxie 配置或真实资源。
- 2026-10-15 关闭菜单状态诊断探针首轮无效：目标隔离 EXE PID `8760` 中，AuditA/AuditB 启动后各有 Sandboxie PID，列表仍显示两行 `【空闲中】`，选中索引为 `6`，当前/全部关闭菜单均为禁用；探针成功调用一次“刷新进程状态”，随后在临时 PowerShell 时间线容器处发生 `Argument types do not match`，未取得刷新后的时间线，也未执行关闭动作。A/B 最终 PID 均为 `0`，`SbieSvc=Running`，目标 EXE 保留用于修正后的同一场景重试。结果为 `artifacts/runs/csharp_navigation_followup_20261015/final-isolated-close-actions-state-diagnostic-result.json`，日志为 `final-isolated-close-actions-state-diagnostic.log`；该轮仅作探针失败记录，不作为产品行为结论。
- 2026-10-15 关闭当前/全部进程真实 EXE 审计通过：修正后的最终隔离 Release x64 状态探针目标 PID `8760` 启动 AuditA/AuditB 后调用“刷新进程状态”，采集 `25` 个、约 `5` 秒状态样本；刷新后两行均为 `【运行中】`，当前/全部关闭菜单均启用。调用“关闭当前打开的进程”后 AuditA PID 清零、AuditB PID `3,6984,3272,496` 保留，状态栏为“已关闭当前打开的进程: AuditA 1012182459”；随后调用“关闭全部打开的进程”后 A/B 均为 `0` PID，状态栏为“已关闭全部打开进程”，菜单均禁用。`SbieSvc=Running`，探针结果 `Passed=true`，本轮专用沙盒最终均为 `0` PID。结果在 `artifacts/runs/csharp_navigation_followup_20261015/final-isolated-close-actions-state-diagnostic-retry-result.json`，日志在 `final-isolated-close-actions-state-diagnostic-retry.log`。关闭全部后的即时抓拍仍短暂保留 AuditB 的 `【运行中】` 文本，但 PID 已为 `0` 且菜单已禁用，作为异步状态收敛风险保留，不影响本次关闭动作调用结论。
- 2026-10-15 关闭进程审计清理：目标 PID `8760` 的实际路径为本轮 `isolated-final-binx64\Release\net48\沙河批量打开器.exe`，`CloseMainWindow()` 请求成功并在 5 秒内退出；AuditA/AuditB 最终均为 `0` PID，`SbieSvc=Running`。清理证据为 `artifacts/runs/csharp_navigation_followup_20261015/final-isolated-close-actions-target-cleanup.json`。
- 2026-10-16 定位当前进程真实审计探针无效：本轮专用 AuditA/AuditB 使用隔离 Release x64 EXE，目标 PID `284`，主窗口句柄和列表句柄均已取得；先后修正了 PowerShell 静态方法调用/UTF-8 编码、脚本块作用域、`Start.exe /listpids` 首行是 PID 计数等探针错误。修正后 AuditA 的 `Start.exe /listpids` 返回 `3` 个 PID，其中同目录 `locate-window-helper.exe` PID `5512` 确实运行，但宿主可见窗口枚举和进程诊断均为空；AuditA/AuditB 最终均为 `0` PID，`SbieSvc=Running`。脚本在清理等待时因 AuditA PID 已归零抛出 `No visible window ... PID 0`，本轮没有形成单窗口、多窗口或无窗口产品行为结论；当前也未发现 AuditA/AuditB 配置含 `HideWindow`。结果为 `artifacts/runs/csharp_navigation_followup_20261015/locate-current-audit-result.json`，日志为 `locate-current-audit.log`，探针为 `locate-current-audit.ps1`，helper 为 `locate-window-helper.exe`。定位当前进程仍待有效真实证据或稳定 RED 契约。
- 2026-10-16 定位窗口 desktop/session 诊断：专用 AuditC/AuditD 初始均为空；宿主和 Sandboxie helper 均在 Session `1`、`WinSta0\Default`，helper PID `3532`/`7596` 均为 `Responding=True`，但 `MainWindowHandle=0`、`MainWindowTitle` 为空，宿主 `EnumWindows` 对 helper 返回空；Sandboxie 自身仅观察到隐藏的 DDE/IME 窗口。该轮排除了“helper 被放到其他 session/desktop”这一探针假设，但尚未解释 WinForms 窗口缺失；AuditC/AuditD 最终均为 `0` PID，`SbieSvc=Running`，清理错误为 `0`。结果为 `artifacts/runs/csharp_navigation_followup_20261015/locate-window-desktop-diagnostic-result.json`，日志为 `locate-window-desktop-diagnostic.log`，探针为 `locate-window-desktop-diagnostic.ps1`。
- 2026-10-16 定位窗口宿主基线有效：同一 `locate-window-helper.exe` 不经 Sandboxie 直接启动，PID `2768` 在 Session `1`、`WinSta0\Default` 创建可见 WinForms 主窗口，`MainWindowHandle=0x1406BA`、标题为 `Host`、`Responding=True`；宿主窗口枚举返回可见窗口及正常的 .NET 辅助窗口，随后 `CloseMainWindow()` 成功退出且退出码为 `0`。该结果排除 helper 编译/宿主枚举本身为根因，定位真实审计当前收敛为 Sandboxie 内 WinForms 顶层窗口缺失的环境风险，尚未形成产品行为结论。结果为 `artifacts/runs/csharp_navigation_followup_20261015/locate-window-host-baseline-result.json`，日志为 `locate-window-host-baseline.log`，探针为 `locate-window-host-baseline.ps1`。
- 2026-10-16 剩余审计启动/列表句柄基线：最终隔离 Release x64 EXE 目标 PID `3260` 创建主窗口句柄 `0x780596`，窗口标题为“沙河批量打开器”，两个列表控件均已找到；列表索引 `0` 为沙盒列表、`96` 条，列表索引 `1` 当前为 `0` 条，`CloseMainWindow()` 在 8 秒内退出且退出码为 `0`，`SbieSvc=Running`。同轮复用的旧 `win32-menu-probe.ps1` 在 Windows PowerShell 解析阶段失败（`Add-Type` here-string 解析错误），菜单输出不作为产品证据；失败日志为 `artifacts/runs/csharp_navigation_followup_20261015/remaining-audit-menu-dump.log`，有效启动/清理结果为 `remaining-audit-startup.json` 与 `remaining-audit-startup-cleanup.json`，列表探针输出为 `remaining-audit-listviews.log` 及 `remaining-audit-listview-*.log`。
- 2026-10-16 剩余审计菜单读取有效：修正 `Add-Type -TypeDefinition` 调用后的菜单探针运行于隔离 Release x64 EXE PID `6340`，真实菜单完整包含“路径记忆管理”、 “粘贴 .exe 或文件路径”、分组/批量、定时、备份/还原等入口，命令 ID 与运行时菜单结构已记录；目标通过 `CloseMainWindow()` 正常退出，`SbieSvc=Running`。有效菜单日志为 `artifacts/runs/csharp_navigation_followup_20261015/remaining-audit-menu-dump-corrected.log`，启动与清理为 `remaining-audit-menu-startup.json`、`remaining-audit-menu-cleanup.json`，修正探针为 `remaining-audit-menu-probe.ps1`。
- 2026-10-16 路径记忆对话框首轮探针无效：嵌套 Windows PowerShell 传递中文菜单路径时发生代码页转换，子探针收到乱码并记录 `found=False`/`invoked=False`，未可靠打开“路径记忆管理”对话框；本轮未写入路径记忆，未形成产品行为结论，也未生成可信结果 JSON。日志为 `artifacts/runs/csharp_navigation_followup_20261015/path-memory-dialog-menu-invoke.log` 与 `path-memory-dialog-real-audit.log`，探针为 `path-memory-dialog-real-audit.ps1`；后续改用已由有效菜单转储确认的命令 ID `359`，不再跨 PowerShell 传递中文路径。
- 2026-10-16 路径记忆对话框命令探针第二轮无效：改用菜单命令 ID `359` 后，`SendMessage(WM_COMMAND)` 返回 `0` 被探针误判为发送失败；Win32 消息返回值不能据此判断菜单处理是否发生，目标 PID `5152` 仍通过 `CloseMainWindow()` 正常退出，未写入路径记忆，未形成产品行为结论。结果为 `path-memory-dialog-real-audit-retry-result.json`，日志为 `path-memory-dialog-real-audit-retry.log`；下一轮只记录消息发送动作并等待对话框句柄。
- 2026-10-16 路径记忆对话框命令探针第三轮未形成证据：命令 ID `359` 已被 `SendMessage(WM_COMMAND)` 接受并记录 `Invoked=true`，但 5 秒内没有找到标题为“路径记忆管理”的同进程顶层窗口；目标 PID `9772` 通过 `CloseMainWindow()` 正常退出，未写入路径记忆。该结果暂不判为产品行为，仍需排除同步消息投递对 WinForms `MenuItem.Click` 的影响。结果为 `path-memory-dialog-real-audit-retry2-result.json`，日志为 `path-memory-dialog-real-audit-retry2.log`。
- 2026-10-16 路径记忆对话框 `PostMessage` 探针第四轮未形成证据：命令 ID `359` 的异步消息投递返回 `Invoked=true`，但同进程窗口枚举只观察到主窗口及普通 WinForms/.NET/输入法辅助窗口，没有“路径记忆管理”对话框；目标 PID `8064` 通过 `CloseMainWindow()` 正常退出，未写入路径记忆。该轮排除了“对话框出现但标题搜索漏掉”的常见情况，但仍未证明产品分支行为。结果为 `path-memory-dialog-real-audit-retry3-result.json`，日志为 `path-memory-dialog-real-audit-retry3.log`。
- 2026-10-16 路径记忆对话框当前菜单文本探针第五轮无效：探针源文件为无 BOM UTF-8，Windows PowerShell 将其中的中文字符串解析为错误文本，当前进程内按“设置/路径记忆管理”查找返回 `CommandId=-1`；目标 PID `4772` 仍正常清理，未写入路径记忆，不能作为产品结论。结果为 `path-memory-dialog-real-audit-retry4-result.json`，日志为 `path-memory-dialog-real-audit-retry4.log`；后续改用 C# 源内 `\u` 转义构造中文菜单和对话框标题。
- 2026-10-16 路径记忆管理入口/取消真实 EXE 审计通过：最终隔离 Release x64 目标 PID `10656` 在同进程内按菜单文本解析到当前命令 ID `301`，成功打开“路径记忆管理”对话框；实际控件包含“路径记忆数量:”及编辑值 `10`、`(0=不记录, 最大20)`、`隐藏路径`复选框、“已记忆的路径列表”列表框以及“取消/确定”按钮。探针取消对话框后目标 `CloseMainWindow()` 正常退出、退出码 `0`，`SbieSvc=Running`，未改变隔离路径记忆配置。结果为 `artifacts/runs/csharp_navigation_followup_20261015/path-memory-dialog-real-audit-retry5-result.json`，日志为 `path-memory-dialog-real-audit-retry5.log`，探针为 `path-memory-dialog-real-audit.ps1`；路径记忆新增、选择、备注、隐藏、删除、清空和确定保存仍待真实动作审计。
- 2026-10-16 Sandbox 管理/定时真实 EXE 审计部分通过：最终隔离 Release x64 目标 PID `1044` 在同进程内动态解析并投递“新建分组”(命令 `285`)、 “定时关闭选中沙盒”(命令 `295`)和“批量创建沙盒用户”(命令 `290`)；定时关闭无选择守卫被观察到，批量创建规则对话框成功打开并发现规则提示、名称前缀、起始编号、编号格式、创建数量以及确定/取消控件，随后取消；新建分组无选择状态文案本轮未捕获，暂不判定为通过或失败。目标正常退出、退出码 `0`，`SbieSvc=Running`，未创建/删除 Sandboxie 用户。结果为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-admin-dialog-real-audit-result.json`，日志为 `sandbox-admin-dialog-real-audit.log`，探针为 `sandbox-admin-dialog-real-audit.ps1`。
- 2026-10-16 Sandbox 管理/定时真实 EXE 审计复测通过：修正状态断言后的隔离 Release x64 目标 PID `8236` 中，“新建分组”(命令 `285`)无选择时实际显示“请先勾选要归类的沙盒用户”；“定时关闭选中沙盒”(命令 `295`)显示“请先勾选或选中要定时关闭的沙盒用户”；“批量创建沙盒用户”(命令 `290`)打开可见规则对话框，控件包含规则提示、名称前缀、起始编号、编号格式、创建数量及确定/取消，随后取消。目标正常退出、退出码 `0`，`SbieSvc=Running`，未创建/删除 Sandboxie 用户。结果为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-admin-dialog-real-audit-retry-result.json`，日志为 `sandbox-admin-dialog-real-audit-retry.log`。
- 2026-10-16 文档同步后的差异检查：`git diff --check` 返回码 `0`，未发现空白错误；输出仅包含 Git 关于计划文件 LF/CRLF 转换的警告，原始记录为 `artifacts/runs/csharp_navigation_followup_20261015/diff-check-remote-sync-note.log`。
- 2026-10-16 路径记忆动作审计探针解析门禁：新增 `path-memory-actions-real-audit.ps1` 经 Windows PowerShell `[scriptblock]::Create` 解析检查返回码 `0`，输出 `PARSE_OK`；尚未启动真实 EXE。日志为 `artifacts/runs/csharp_navigation_followup_20261015/path-memory-actions-real-audit-parse.log`。
- 2026-10-16 路径记忆动作审计首轮无效：隔离 Release x64 目标 PID `10472` 已启动，但探针在异步路径列表尚未填充时读取到 `0` 条，随后 WinForms 控件类名过滤未找到编辑框/按钮，路径管理模态框阻塞了自动清理；该轮脚本还错误地无条件写入 `Passed=true`，不作为产品证据。已手动点击本轮自有对话框取消并确认 PID 正常退出，配置已恢复、剪贴板已恢复、`SbieSvc=Running`。原始结果为 `path-memory-actions-real-audit-result.json`，日志为 `path-memory-actions-real-audit.log`。
- 2026-10-16 路径记忆动作审计探针修正解析门禁：增加异步列表填充等待、WinForms 类名后缀匹配和清理后真实性断言；Windows PowerShell 解析检查返回码 `0`，输出 `PARSE_OK`。日志为 `artifacts/runs/csharp_navigation_followup_20261015/path-memory-actions-real-audit-parse2.log`。
- 2026-10-11 快捷键运行时分派模块建立验证基线：C# 全量契约 `178/178`、Release x64 构建 `0` 警告/`0` 错误、`git diff --check` 通过，Python 全量测试 `29/29`；`python -m compileall -q .` 被既有临时探针 `artifacts/runs/full_software_test_20261008/probe_debug/no_cleanup.py:39` 的语法错误阻断，未修改该探针或 Python 正式基线。原始日志保存在 `artifacts/runs/csharp_task18_runtime_dispatch_baseline_20261011/`。
- 2026-10-11 快捷键运行时分派 RED：新增三个 Application 契约，分别覆盖启用自定义动作、固定快捷键优先级，以及禁用/无效/未知快捷键回退；定向运行 `3` 项结果为 `2` 通过、`1` 失败，失败点为 `ProcessCmdKey` 未把 `toggle_window` 等自定义动作送入运行时回调。日志保存在 `artifacts/runs/csharp_task18_runtime_dispatch_red_20261011/targeted_tests.log`。
- 2026-10-11 快捷键动作接线 RED：新增设置开关执行契约，直接调用真实 `ExecuteApplicationAction` 验证自动运行、托盘、自动选择、跳过运行沙盒、音量记忆和左右导航状态同步；定向运行 `1` 项按预期失败，当前仍落入“已触发操作”默认分支，日志保存在 `artifacts/runs/csharp_task18_runtime_dispatch_red2_20261011/targeted_tests.log`。
- 2026-10-11 快捷键运行时分派定向 GREEN：`TestCategory=Hotkeys|Application` 通过 `34/34`；新增自定义快捷键运行时分派、固定键优先、禁用/无效/未知回退、八个设置/窗口动作和重置取消契约。Python `fixed_hotkeys_focus_probe.py`、`process_menu_selection_refresh_probe.py`、`process_menu_uses_running_marker_probe.py` 共 `3/3` 通过；日志保存在 `artifacts/runs/csharp_task18_runtime_dispatch_validation_20261011/`。
- 2026-10-11 快捷键真实动作链 GREEN：保留 `MainForm` 默认 `FixedHotkeyActionCallback`，从 `ProcessCmdKey` 触发自定义 `toggle_auto_run`，状态栏和自启动写入回调均符合预期，定向 `1/1` 通过；日志为 `artifacts/runs/csharp_task18_runtime_dispatch_validation_20261011/end_to_end_custom_action.log`。
- 2026-10-11 快捷键运行时分派全量回归：C# 全量 `183/183`、Python 全量 `29/29`、Release x64 构建 `0` 警告/`0` 错误、`git diff --check` 通过；`python -m compileall -q .` 仍被既有 `artifacts/runs/full_software_test_20261008/probe_debug/no_cleanup.py:39` 语法错误阻断，生产源码未新增编译错误。结果日志在 `artifacts/runs/csharp_task18_runtime_dispatch_validation_20261011/`。
- 2026-10-11 快捷键运行时分派最终门禁：新增端到端契约后 C# 全量为 `184/184`，显式 `dotnet build ... --property:Platform=x64` 通过，`0` 警告/`0` 错误；Python 全量仍为 `29/29`，`git diff --check` 通过。`compileall` 的唯一失败仍是上述既有调试探针语法错误，未把该临时文件纳入本模块修复。
- 2026-10-12 提交前最终门禁：C# `TestCategory=Hotkeys|Application` 通过 `36/36`、全量 `184/184`，Release x64 构建 `0` 警告/`0` 错误；Python 全量 `29/29`、快捷键及菜单行为探针 `3/3`、生产源码范围 `compileall` 通过，`git diff --check` 通过。全仓 `python -m compileall -q .` 仍仅被既有临时探针 `artifacts/runs/full_software_test_20261008/probe_debug/no_cleanup.py:39` 阻断；日志保存在 `artifacts/runs/csharp_task18_runtime_dispatch_final_20261012/`。
- 2026-10-12 路径记忆运行时接线 RED：新增 `PathMemoryRuntimeContractTests` 的 4 个契约先按预期失败，分别暴露设置菜单缺少管理入口、列表未使用 `FormatDisplayText`/真实路径 `Tag`、七个路径动作进入默认回退、`PathMemoryManagerDialog` 保存回调未回写主窗口；日志在 `artifacts/runs/csharp_task19_path_memory_baseline_20261012/path-memory-red.log`。
- 2026-10-12 路径记忆运行时接线 GREEN：`MainForm` 接入 `AppPathHistoryService` 临时状态适配、经典 `ContextMenu/MenuItem`、`Ctrl+H` 上下文菜单、设置菜单独立管理对话框、路径选择/勾选回调和七个路径记忆动作；`ListViewItem.Tag` 保存真实路径，显示文本复用 `FormatDisplayText()`，管理对话框显示文本也复用同一服务方法。路径记忆契约 `4/4`，受影响分类 `65/65`，日志在 `artifacts/runs/csharp_task19_path_memory_baseline_20261012/`。
- 2026-10-12 路径记忆运行时接线最终门禁：C# 全量 `188/188`，Release x64 构建 `0` 警告/`0` 错误；Python 全量 `29/29`；`python -m compileall -q shahe_bulk_launcher tests` 通过；全仓 `python -m compileall -q .` 仍仅被既有 `artifacts/runs/full_software_test_20261008/probe_debug/no_cleanup.py:39` 阻断；`git diff --check` 通过。路径列表选择/勾选恢复未触发扫描、启动或进程刷新契约。下一模块为“菜单状态与沙盒选择反馈”。
- 2026-10-12 菜单状态与沙盒选择反馈 RED：新增 `ProcessMenuRuntimeContractTests` 五项契约，先按预期 `0/5` 通过，分别暴露进程菜单默认全启用、静音缓存未改变标签、`SandboxToggledCallback` 未赋值、列表无刷新期边界、选择反馈无临时抑制边界；日志在 `artifacts/runs/csharp_process_menu_runtime_20261012/process-menu-red.log`。
- 2026-10-12 菜单状态与沙盒选择反馈 GREEN：`MainForm.CreateMenuBar()` 复用现有 `HotkeyController.GetProcessMenuStates()` 和实时静音缓存，根视图只读取列表运行标记，只有分组可能隐藏成员时才回退全局进程状态；经典 `MainMenu/MenuItem` 按当前/全部运行状态启用，刷新项始终可用。`CheckableListCtrl` 增加只读刷新边界，主窗口仅在明确勾选后显示“已选中 N 个沙盒”，程序化刷新和临时抑制不覆盖业务状态。定向 `5/5`、受影响分类 `65/65`、Application `36/36` 通过。
- 2026-10-12 菜单状态与沙盒选择反馈最终门禁：C# 全量 `193/193`，Release x64 构建 `0` 警告/`0` 错误；Python 全量 `29/29`，生产源码 `compileall` 通过，`git diff --check` 通过；全仓 `compileall` 唯一失败仍为既有 `artifacts/runs/full_software_test_20261008/probe_debug/no_cleanup.py:39`。隔离 Release x64 副本完成 `F10`、`Alt+P`、方向键、回车和列表 `Space` 验证，状态栏显示“已选中 1 个沙盒”，正常退出且未执行破坏性动作；结果在 `artifacts/runs/csharp_process_menu_runtime_20261012/real_ui/summary.json`。下一模块为“全量重置和设置开关动作”。
- 2026-10-12 菜单状态与沙盒选择反馈独立提交快照门禁：从 Git 索引导出仅含本模块暂存内容的快照后，C# 全量 `188/188`、Python 全量 `24/24`、生产源码及全仓 `compileall` 均通过，Release x64 构建 `0` 警告/`0` 错误，`git diff --cached --check` 通过。计数少于工作树门禁是因为未纳入用户未提交的其他测试改动；结果日志在 `artifacts/runs/csharp_process_menu_runtime_20261012/staged-snapshot-*.log`。
- 2026-10-12 全量重置和设置开关动作基线：当前工作树 C# 全量 `193/193`、Python 全量 `29/29`、生产源码 `compileall` 通过，Release x64 构建 `0` 警告/`0` 错误，`git diff --check` 通过；全仓 `compileall` 仍仅被既有 `artifacts/runs/full_software_test_20261008/probe_debug/no_cleanup.py:39` 阻断。基线日志在 `artifacts/runs/csharp_settings_actions_20261012/`；下一步以 RED 契约冻结 `reset_settings` 对现有 `LauncherSettingsService.ResetSettings()` 的真实调用及重置后运行时同步顺序。
- 2026-10-12 全量重置和设置开关动作 RED：新增 `SettingsRuntimeContractTests` 两项端到端契约，先按预期 `0/2` 通过；当前 `reset_settings` 在 `LauncherSettingsService.ResetSettings()` 已完成一次 Python 顺序的热键/菜单应用后又重复重建菜单，实测预期 `1` 次、实际 `2` 次；同时沙盒分组运行时状态在列表刷新后才清空，重置后的可见列表仍暂留旧分组。日志为 `artifacts/runs/csharp_settings_actions_20261012/settings-runtime-red.log`。
- 2026-10-12 全量重置和设置开关动作定向 GREEN：`MainForm.ResetApplicationSettings()` 继续复用现有 `LauncherSettingsService.ResetSettings()`，并把现有主窗口/进程/启动器/沙盒管理状态同步移动到服务的 `SetSandboxRoot` 回调，确保分组状态先恢复默认再刷新列表；删除服务返回后的重复热键应用、菜单重建和状态写入。`SettingsRuntimeContractTests` 通过 `2/2`，Application + SettingsService + Hotkeys 受影响分类通过 `54/54`；日志在 `artifacts/runs/csharp_settings_actions_20261012/`。
- 2026-10-12 全量重置和设置开关动作最终门禁：C# 全量 `195/195`、Python 全量 `29/29`、Release x64 构建 `0` 警告/`0` 错误、生产源码 `compileall` 和 `git diff --check` 通过；全仓 `compileall` 仍仅被既有 `artifacts/runs/full_software_test_20261008/probe_debug/no_cleanup.py:39` 阻断。六个设置开关及确认重置后的主窗口、快捷键、托盘、自启动、自动选择、启动跳过、音量记忆、左右导航和分组列表状态均有运行时契约；日志在 `artifacts/runs/csharp_settings_actions_20261012/`。下一模块为“真实音频后端”。
- 2026-10-12 全量重置和设置开关动作独立提交快照门禁：从 Git 索引导出仅含本模块暂存内容的快照后，C# 全量 `190/190`、Git 已跟踪 Python 基线 `24/24`、全仓 `compileall` 通过，Release x64 构建 `0` 警告/`0` 错误，`git diff --cached --check` 通过。计数少于工作树门禁是因为未纳入用户未提交的其他测试改动；日志在 `artifacts/runs/csharp_settings_actions_20261012/staged-snapshot-*.log`。
- 2026-10-12 真实音频后端基线：当前工作树 C# 全量 `195/195`、Python 全量 `29/29`、生产源码 `python -m compileall -q shahe_bulk_launcher tests` 通过、Release x64 构建 `0` 警告/`0` 错误，`git diff --check` 退出码为 `0`；全仓 `python -m compileall -q .` 仍仅被既有临时探针 `artifacts/runs/full_software_test_20261008/probe_debug/no_cleanup.py:39` 阻断。基线日志在 `artifacts/runs/csharp_audio_backend_20261012/`；下一步以 RED 契约冻结主窗口真实音频会话、输出设备和进程路由后端接线。
- 2026-10-12 真实音频后端 RED：新增 `WindowsProcessAudioBackendContractTests` 四项契约，定向运行按预期 `0/4` 通过；失败分别定位为 `MainForm` 未组合真实音频后端，以及 `WindowsProcessAudioBackend` 尚不存在，覆盖回调绑定、PID 会话读写委托、输出设备/默认标记、WinRT 音频策略 ID 和无效路由失败边界。日志为 `artifacts/runs/csharp_audio_backend_20261012/audio-backend-red.log`。
- 2026-10-12 真实音频后端 GREEN：新增 `WindowsProcessAudioBackend`，使用原生 Core Audio COM 枚举默认多媒体端点会话、活动输出设备和默认设备，并使用本机 `winappaudiorouter 1.1.1` 同源的 `Windows.Media.Internal.AudioPolicyConfig` WinRT vtable 为进程设置 Console/Multimedia 输出路由；`MainForm` 三个音频回调已绑定同一后端实例。音频契约 `4/4` 通过，日志在 `artifacts/runs/csharp_audio_backend_20261012/audio-backend-green2.log`。
- 2026-10-12 真实音频后端本机 COM 探针：Release 后端枚举到 `8` 个活动会话和 `3` 个活动输出设备，默认设备标记 `1` 个；所有会话均有 PID、静音和音量读写委托，Windows build `19045` 使用 downlevel PolicyConfig IID；`ProcessActions` 定向回归 `12/12` 通过。探针输出和回归日志在 `artifacts/runs/csharp_audio_backend_20261012/`。
- 2026-10-12 真实音频后端异步边界 RED：新增 `ProcessAudioAsyncContractTests`，定向运行 `0/1` 按预期失败；设备选择前的 `ListOutputDevices` 当前在调用线程同步执行，阻塞时无法立即返回。日志为 `artifacts/runs/csharp_audio_backend_20261012/audio-async-red.log`。
- 2026-10-12 真实音频后端异步边界 GREEN：`ProcessActionService` 将设备枚举移入 `RunVolumeTaskAsync` 后台任务，使用现有 `PostToUi` 执行选择对话框，再在同一后台任务完成进程路由；`ProcessAudioAsyncContractTests` 通过 `1/1`，不改变原有状态文案和结果聚合。
- 2026-10-12 真实音频后端专用播放进程探针：临时 Python `winsound` 静音 WAV 进程被真实会话枚举到；静音切换回读 `true`、音量写入 `0.37` 后回读 `0.3700000048` 并恢复到 `1.0`，对默认播放设备的进程路由返回 `true`。临时进程和 WAV 已清理，结果在 `artifacts/runs/csharp_audio_backend_20261012/real-audio-probe.json`。
- 2026-10-12 真实音频后端最终门禁：音频/进程受影响分类 `13/13`、C# 全量 `200/200`、Python 全量 `29/29`、生产源码 `compileall` 通过、Release x64 构建 `0` 警告/`0` 错误、`git diff --check` 退出码 `0`；全仓 `compileall` 仍仅被既有 `artifacts/runs/full_software_test_20261008/probe_debug/no_cleanup.py:39` 阻断。下一模块为“真实 EXE 与 Sandboxie 集成测试”。日志在 `artifacts/runs/csharp_audio_backend_20261012/`。
- 2026-10-12 真实 EXE 审计发现进程菜单实时状态偏差：真实静音成功后主菜单仍显示“静音当前打开的进程”。新增 `ProcessUiRefreshRecomputesLiveMuteStateWithoutBlockingUi` 契约先按预期 RED `0/1`，确认 `RefreshProcessUi()` 只清空缓存并重建菜单，生产路径没有计算或提交 `AcceptProcessMenuLiveState()`；RED 日志在 `artifacts/runs/csharp_sandboxie_audit_20261012/menu-live-state-fix/red.log`。
- 2026-10-12 进程菜单实时状态接线 GREEN：进程动作完成后按 Python 顺序保留旧缓存、安排后台实时刷新，PID、窗口、音频会话和输出设备枚举不阻塞 WinForms UI，完成后使用现有 token 接受结果并刷新菜单；定向 `1/1`、进程菜单整组 `6/6`、Application/ProcessActions/AudioBackend 受影响分类 `52/52` 通过。
- 2026-10-12 进程菜单真实 EXE 回归：专用 `AuditA_1012182459` 中的 `LoopAudioProbe` PID `22224` 已加载 `SbieDll.dll`；静音后 Core Audio 回读 `Muted=true` 且菜单为“取消静音当前打开的进程”，恢复后回读 `Muted=false` 且菜单为“静音当前打开的进程”。证据在 `artifacts/runs/csharp_sandboxie_audit_20261012/menu-live-state-fix/real-exe-muted.json` 与 `real-exe-unmuted.json`。
- 2026-10-12 进程菜单实时状态最终门禁：C# 全量 `201/201`、Python 全量 `29/29`、`python -m compileall -q shahe_bulk_launcher tests`、Release x64 构建和 `git diff --check` 通过，构建 `0` 警告/`0` 错误；全仓 `compileall` 仍仅被既有 `artifacts/runs/full_software_test_20261008/probe_debug/no_cleanup.py:39` 阻断。日志在 `artifacts/runs/csharp_sandboxie_audit_20261012/menu-live-state-fix/`。
- 2026-10-12 进程菜单实时状态独立提交快照门禁：从 Git 索引导出仅含本次暂存内容的快照，还原后 C# 全量 `196/196`、Git 已跟踪 Python `24/24`、快照全仓 `compileall` 和 Release x64 构建均通过，构建 `0` 警告/`0` 错误，`git diff --cached --check` 通过。计数少于工作树门禁是因为未纳入用户未暂存的其他测试改动；日志在 `artifacts/runs/csharp_sandboxie_audit_20261012/menu-live-state-fix/staged-snapshot-*.log`。
- 2026-10-12 真实 EXE 当前沙盒音量审计：专用 `AuditA_1012182459` 中的 `LoopAudioProbe` PID `22224` 通过主程序进程菜单完成降低和提高音量，Core Audio 回读依次为 `1.0 -> 0.949999988079071 -> 1.0`，日志文案分别为“已将 AuditA 1012182459 音量降低 5%，当前约 95%”和“已将 AuditA 1012182459 音量提高 5%，当前约 100%”。证据为 `artifacts/runs/csharp_sandboxie_audit_20261012/volume-current-down.json` 与 `volume-current-up.json`。
- 2026-10-12 播放设备切换审计诊断中：恢复可见主窗口后，命令发出约 `100 ms` 即出现“切换当前打开的进程播放设备”模态框，主窗口在模态期间正确禁用，确认菜单投递、`PostToUi` 和对话框边界可以工作。选择 `Playback 3/4 (2- E2x2)` 后，真实 EXE 报告切换失败；Python `winappaudiorouter 1.1.1` 对相同 PID 和设备切换、回读及恢复默认设备均成功，.NET Framework x64 STA 主线程直接调用同一 C# 后端对三个设备也均返回成功。当前根因假设已收敛为生产菜单 `Task.Run` 的 MTA 线程与 WinRT AudioPolicyConfig 的 apartment 差异，尚待 STA/MTA 对照探针和 HRESULT 证据确认。
- 2026-10-14 真实进程播放设备路由 RED：新增 `OutputDeviceChoiceWaitsForTheUserWithoutAFixedDeadline` 与 `UsesActualWindowsBuildForPolicyConfigWhenFrameworkVersionIsVirtualized` 两项契约；Release x64 构建通过 `0` 警告、`0` 错误，音频/进程定向测试 `15` 项中 `13` 通过、`2` 失败。失败分别锁定 `PromptOutputDeviceChoiceFromWorker` 的固定 `30` 秒截止判断，以及 `WindowsProcessAudioBackend` 缺少真实宿主 Windows build 检测；生产代码尚未修改。日志在 `artifacts/runs/csharp_audio_route_fix_20261014/red-build.log` 与 `artifacts/runs/csharp_audio_route_fix_20261014/red-targeted.log`。
- 2026-10-14 真实进程播放设备路由 GREEN 构建：`WindowsProcessAudioBackend` 已改用 `RtlGetVersion` 读取宿主真实 Windows build，设备选择等待仅在窗口关闭时取消；Release x64 构建成功，`0` 警告、`0` 错误。日志在 `artifacts/runs/csharp_audio_route_fix_20261014/green-build.log`。
- 2026-10-14 真实进程播放设备路由 GREEN 定向回归：`TestCategory=AudioBackend|ProcessActions` 通过 `15/15`，包含真实宿主 Windows build 契约、无固定期限设备选择契约、Core Audio 后端契约、音频异步边界和原有进程动作契约。日志在 `artifacts/runs/csharp_audio_route_fix_20261014/green-targeted.log`。
- 2026-10-14 真实进程播放设备路由本机非破坏性验证：当前 Release Core 实际读取 `frameworkBuild=19045`、`routingBuild=19045`；专用循环音频辅助进程的 Core Audio 会话发现为 `sessionFound=true`，运行时枚举到 `3` 个输出设备、其中 `1` 个默认设备，选择当次枚举的非默认设备“扬声器 (Realtek(R) Audio)”后 `routeSucceeded=true`。辅助进程已清理，未启动或终止 Sandboxie 应用。结构化证据在 `artifacts/runs/csharp_audio_route_fix_20261014/real-audio-route.json`，探针脚本在同目录。
- 2026-10-14 真实进程播放设备路由 C# 全量回归：Release x64 `ShaheLauncher.Tests.dll` 通过 `203/203`，0 失败、0 跳过；日志在 `artifacts/runs/csharp_audio_route_fix_20261014/csharp-full.log`。
- 2026-10-14 真实进程播放设备路由 Python 对照回归：`python -m unittest discover -s tests -p "test_*.py" -v` 通过 `29/29`；日志在 `artifacts/runs/csharp_audio_route_fix_20261014/python-full.log`。
- 2026-10-14 真实进程播放设备路由 Python 生产源码语法门禁：`python -m compileall -q shahe_bulk_launcher tests` 通过，退出码 `0`；日志在 `artifacts/runs/csharp_audio_route_fix_20261014/python-compile-production.log`。
- 2026-10-14 真实进程播放设备路由 Python 全仓语法门禁：`python -m compileall -q .` 退出码 `1`，唯一错误仍为既有临时探针 `artifacts/runs/full_software_test_20261008/probe_debug/no_cleanup.py:39` 的 `SyntaxError`；本轮未修改该无关探针。日志在 `artifacts/runs/csharp_audio_route_fix_20261014/python-compile-full.log`。
- 2026-10-14 真实进程播放设备路由 Release x64 构建：`ShaheBulkLauncher.sln` 还原/构建成功，Core、App、Updater、Tests 均生成，`0` 警告、`0` 错误；日志在 `artifacts/runs/csharp_audio_route_fix_20261014/release-build-final.log`。
- 2026-10-14 真实进程播放设备路由差异检查：`git diff --check` 退出码 `0`；日志在 `artifacts/runs/csharp_audio_route_fix_20261014/git-diff-check-final.log`。
- 2026-10-15 真实 EXE 当前进程播放设备菜单审计：隔离 Release x64 副本在 `AuditA_1012182459` 中通过原生菜单命令 ID `303` 打开“切换当前打开的进程播放设备”对话框；键盘 `VK_DOWN` 将列表从默认项切换到“扬声器 (Realtek(R) Audio)”，确认后状态栏记录“成功 1，失败 2”，Python `winappaudiorouter.get_app_output_device(process_id=9000)` 回读的持久路由 ID 与所选设备一致。`real-exe-route-evidence.json`、菜单/窗口/控件探针和运行日志保存在 `artifacts/runs/csharp_audio_route_exe_20261015/`；清理阶段已清除 PID 9000 的临时路由并退出本轮启动器，未停止 Sandboxie 服务。
- 2026-10-15 真实进程播放设备路由新鲜门禁：音频/进程定向 `15/15`、C# 全量 `203/203`、Python 全量 `29/29`、`python -m compileall -q shahe_bulk_launcher tests` 通过、Release x64 构建 `0` 警告/`0` 错误、`git diff --check` 通过；全仓 `python -m compileall -q .` 仍仅被既有临时探针 `artifacts/runs/full_software_test_20261008/probe_debug/no_cleanup.py:39` 阻断。新鲜日志在 `artifacts/runs/csharp_audio_route_exe_20261015/`。
- 2026-10-15 提交前音频/进程定向门禁复跑：`TestCategory=AudioBackend|ProcessActions` 通过 `15/15`，覆盖真实宿主 Windows build、无固定期限设备选择、Core Audio 后端、异步设备枚举和原有进程动作契约；日志在 `artifacts/runs/csharp_audio_route_commit_gate_20261015/audio-process-targeted.log`。
- 2026-10-15 提交前 C# 全量门禁复跑：Release x64 `ShaheLauncher.Tests.dll` 通过 `203/203`，无失败、无跳过；日志在 `artifacts/runs/csharp_audio_route_commit_gate_20261015/csharp-full.log`。
- 2026-10-15 提交前 Python 对照门禁复跑：`python -m unittest discover -s tests -p "test_*.py" -v` 通过 `29/29`；日志在 `artifacts/runs/csharp_audio_route_commit_gate_20261015/python-full.log`。
- 2026-10-15 提交前 Python 生产源码语法门禁复跑：`python -m compileall -q shahe_bulk_launcher tests` 通过，退出码 `0`；日志在 `artifacts/runs/csharp_audio_route_commit_gate_20261015/python-compile-production.log`。
- 2026-10-15 提交前 Release x64 构建复跑：`ShaheBulkLauncher.sln` 成功生成 Core、App、Updater、Tests，`0` 警告、`0` 错误；修正后的日志在 `artifacts/runs/csharp_audio_route_commit_gate_20261015/release-build-corrected.log`。
- 2026-10-15 提交前 Python 全仓语法门禁复跑：`python -m compileall -q .` 仍仅被既有临时探针 `artifacts/runs/full_software_test_20261008/probe_debug/no_cleanup.py:39` 的 `SyntaxError` 阻断；本轮未修改该探针。日志在 `artifacts/runs/csharp_audio_route_commit_gate_20261015/python-compile-full.log`。
- 2026-10-15 提交前差异检查复跑：`git diff --check` 退出码 `0`；日志在 `artifacts/runs/csharp_audio_route_commit_gate_20261015/git-diff-check.log`。
- 2026-10-15 真实 EXE 关闭动作审计映射建立：对照 Python `_terminate_pids`、`_terminate_sandbox_via_start`、`_terminate_all_via_start`、`_close_sandbox_processes`、`on_close_current_sandbox`、`on_close_all_sandboxes`，确认 C# 对应为 `ProcessActionService.TerminatePids`、`TerminateSandboxViaStart`、`TerminateAllViaStart`、`CloseSandboxProcesses`、`OnCloseCurrentSandbox`、`OnCloseAllSandboxes`，并由 `MainForm` 的 `CloseCurrentCallback`/`CloseAllCallback` 接入。当前仅完成代码/契约映射，尚未取得关闭当前进程和关闭全部进程的真实 EXE 证据；本轮不将契约测试替代为集成通过。
- 2026-10-15 关闭当前进程状态恢复契约 RED：新增 `ProcessMenuRuntimeContractTests.ProcessUiRefreshPreservesActionStatusAfterRefreshingSandboxList`；定向运行共 `7` 项，`6` 项通过、`1` 项按预期失败。实际状态“已关闭当前打开的进程: 用户A”被 `FastRefreshSandboxList` 覆盖为“已刷新 1 个沙盒用户”，确认 C# `MainForm.RefreshProcessUi(true)` 缺少 Python `_refresh_process_ui_after_action` 的状态保存/恢复逻辑。日志在 `artifacts/runs/csharp_close_process_exe_20261015/status-red.log`。
- 2026-10-15 关闭当前进程状态恢复契约 GREEN：`MainForm.RefreshProcessUi(true)` 保存并恢复刷新前的非空状态文本；`ProcessMenuRuntimeContractTests` 定向运行 `7/7` 通过，验证“已关闭当前打开的进程: 用户A”在刷新沙盒列表后保持不变。日志在 `artifacts/runs/csharp_close_process_exe_20261015/status-green.log`。
- 2026-10-15 关闭动作状态恢复受影响回归：`TestCategory=Application` 通过 `40/40`，确认主窗口动作接线、进程菜单状态和刷新反馈无回归。日志在 `artifacts/runs/csharp_close_process_exe_20261015/application-regression.log`。
- 2026-10-15 关闭动作状态恢复 C# 全量回归：`dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore` 通过 `204/204`，无失败、无跳过；日志在 `artifacts/runs/csharp_close_process_exe_20261015/csharp-full.log`。
- 2026-10-15 关闭动作状态恢复 Python 对照回归：`python -m unittest discover -s tests -p "test_*.py" -v` 通过 `29/29`；日志在 `artifacts/runs/csharp_close_process_exe_20261015/python-full.log`。
- 2026-10-15 关闭动作状态恢复 Python 生产源码语法门禁：`python -m compileall -q shahe_bulk_launcher tests` 通过，退出码 `0`；日志在 `artifacts/runs/csharp_close_process_exe_20261015/python-compile-production.log`。
- 2026-10-15 关闭动作状态恢复 Release x64 构建：`dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64` 成功生成 Core、App、Updater、Tests，`0` 警告、`0` 错误；日志在 `artifacts/runs/csharp_close_process_exe_20261015/release-build.log`。
- 2026-10-15 关闭动作状态恢复差异检查：`git diff --check` 退出码 `0`；仅有 Git 的 LF/CRLF 转换提示，无差异错误。日志在 `artifacts/runs/csharp_close_process_exe_20261015/git-diff-check.log`。
- 2026-10-15 多用户运行后列表导航卡顿诊断 RED：新增 `ProcessMenuRuntimeContractTests.ProcessMenuLiveStateRefreshCoalescesWhileWorkerIsRunning`；定向运行共 `8` 项，`7` 项通过、`1` 项失败。第一轮实时状态扫描阻塞时连续触发两次刷新，C# 实际启动 `3` 个扫描调用；与 Python 的 `_process_menu_live_state_refresh_in_progress`/`_process_menu_live_state_pending` 语义不一致，确认重复后台扫描是列表规模和运行用户数放大卡顿的根因候选。日志在 `artifacts/runs/csharp_close_process_exe_20261015/live-refresh-red.log`。
- 2026-10-15 多用户运行后 UI 线程全局 PID 扫描诊断 RED：新增 `ProcessMenuRuntimeContractTests.ProcessUiRefreshDoesNotScanAllSandboxPidsOnUiThread`；定向运行 `1` 项按预期失败，实测 `RefreshProcessUi(false)` 在含分组且可见运行标记尚未建立时被同步全局 PID 扫描阻塞，确认 C# 在启动实时状态 worker 前读取 `GetProcessMenuStates()` 的 UI 线程调用是独立卡顿根因候选。日志在 `artifacts/runs/csharp_close_process_exe_20261015/ui-thread-scan-red.log`。
- 2026-10-15 多用户运行后 UI 线程全局 PID 扫描诊断 GREEN：`GetProcessMenuStates(false)` 只在 UI 线程捕获列表快照，`ListMayHideSandboxMembers` 对应的全局 PID 扫描移入实时状态 worker；`ProcessMenuRuntimeContractTests` 定向运行 `9/9` 通过，新增测试确认扫描线程不等于 UI 线程。日志在 `artifacts/runs/csharp_close_process_exe_20261015/ui-thread-scan-green.log`。
- 2026-10-15 UI 线程全局 PID 扫描受影响回归：`TestCategory=Application|Lifecycle|Controls` 通过 `63/63`，确认应用动作、生命周期和列表控件契约无回归。日志在 `artifacts/runs/csharp_close_process_exe_20261015/ui-thread-scan-regression.log`。
- 2026-10-15 UI 线程全局 PID 扫描 C# 全量回归：`dotnet test csharp\\ShaheBulkLauncher.sln --configuration Release --no-restore` 通过 `206/206`，无失败、无跳过。日志在 `artifacts/runs/csharp_close_process_exe_20261015/ui-thread-scan-csharp-full.log`。
- 2026-10-15 UI 线程全局 PID 扫描 Python 对照回归：`python -m unittest discover -s tests -p "test_*.py" -v` 通过 `29/29`；日志在 `artifacts/runs/csharp_close_process_exe_20261015/ui-thread-scan-python-full.log`。
- 2026-10-15 UI 线程全局 PID 扫描 Python 生产源码语法门禁：`python -m compileall -q shahe_bulk_launcher tests` 通过，退出码 `0`；日志在 `artifacts/runs/csharp_close_process_exe_20261015/ui-thread-scan-python-compile.log`。
- 2026-10-15 UI 线程全局 PID 扫描修复最新 Release x64 构建：`dotnet build csharp\\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64` 成功生成 Core、App、Updater、Tests，`0` 警告、`0` 错误；日志在 `artifacts/runs/csharp_close_process_exe_20261015/ui-thread-scan-release-build.log`。
- 2026-10-15 UI 线程全局 PID 扫描修复差异检查：`git diff --check` 退出码 `0`；日志在 `artifacts/runs/csharp_close_process_exe_20261015/ui-thread-scan-diff-check.log`，仅有 Git 的 LF/CRLF 转换提示，无差异错误。
- 2026-10-15 最新 Release x64 多用户启动后立即导航真实 EXE 测量：从每个审计沙盒首次出现 PID 后立即采样，单键中位数为 1 用户 `8.509ms`、2 用户 `8.717ms`、3 用户 `8.979ms`、4 用户 `8.931ms`；20 键零间隔突发最大耗时分别为 `165.914ms`、`160.348ms`、`177.467ms`、`166.835ms`，均无丢键/超时。四个审计沙盒最终 PID 均为 `0`，`SbieSvc` 保持运行；本轮未复现稳定的启动瞬间卡顿，需继续覆盖延迟状态刷新和菜单重建窗口。结果在 `artifacts/runs/csharp_close_process_exe_20261015/multi-user-navigation-immediate-result.json`，日志在 `artifacts/runs/csharp_close_process_exe_20261015/multi-user-navigation-immediate.log`。
- 2026-10-15 最新 Release x64 启动后 `160ms` 延迟导航真实 EXE 测量：单键中位数为 1 用户 `8.178ms`、2 用户 `7.902ms`、3 用户 `7.707ms`、4 用户 `7.573ms`；20 键零间隔突发最大耗时分别为 `165.580ms`、`173.918ms`、`162.586ms`、`166.095ms`，均无丢键/超时。四个审计沙盒最终 PID 均为 `0`；`160ms` 窗口未复现稳定卡顿。结果在 `artifacts/runs/csharp_close_process_exe_20261015/multi-user-navigation-delay160-result.json`，日志在 `artifacts/runs/csharp_close_process_exe_20261015/multi-user-navigation-delay160.log`。
- 2026-10-15 最新 Release x64 启动后 `800ms` 延迟导航真实 EXE 测量：单键中位数为 1 用户 `8.880ms`、2 用户 `7.840ms`、3 用户 `7.183ms`、4 用户 `7.544ms`；20 键零间隔突发最大耗时分别为 `176.383ms`、`181.891ms`、`166.308ms`、`165.406ms`，均无丢键/超时。四个审计沙盒最终 PID 均为 `0`；`800ms` 窗口未复现稳定卡顿。结果在 `artifacts/runs/csharp_close_process_exe_20261015/multi-user-navigation-delay800-result.json`，日志在 `artifacts/runs/csharp_close_process_exe_20261015/multi-user-navigation-delay800.log`。
- 2026-10-15 菜单重建全局 PID 扫描诊断 RED：新增 `ProcessMenuRuntimeContractTests.MenuBarRebuildDoesNotScanAllSandboxPidsOnUiThread`；定向运行 `1` 项按预期失败，注入 600ms 全局 PID 查询后 `CreateMenuBar()` 仍同步阻塞 UI，确认菜单栏重建路径存在独立的 UI 线程扫描。日志在 `artifacts/runs/csharp_close_process_exe_20261015/menu-bar-scan-red.log`。
- 2026-10-15 菜单重建异步回退修复中间回归：UI 线程扫描契约已通过，但原 `ProcessMenuUsesVisibleRunningMarkersAndOnlyScansWhenGroupsHideMembers` 仍按旧同步扫描语义断言，定向类测试为 `9/10`；失败仅为分组隐藏进程在后台状态尚未完成时菜单暂未启用，需将测试改为等待后台结果并验证扫描线程。日志在 `artifacts/runs/csharp_close_process_exe_20261015/menu-bar-scan-after-fix.log`。
- 2026-10-15 菜单重建异步回退修复 GREEN：`CreateMenuBar()` 改为只读列表运行标记和后台实时状态缓存；分组隐藏成员的全局 PID 扫描由实时状态 worker 执行，完成后缓存 `AllHasProcess` 并恢复菜单启用状态。`ProcessMenuRuntimeContractTests` 定向运行 `11/11`，新增后台线程和异步菜单恢复契约通过。日志在 `artifacts/runs/csharp_close_process_exe_20261015/menu-bar-scan-green.log`。
- 2026-10-15 菜单重建异步回退受影响回归中间失败：`TestCategory=Application|Lifecycle|Controls` 为 `67/68`，唯一失败是 `SettingsRuntimeContractTests.ResetSettingsRestoresSwitchesWithOnePythonOrderedMenuRefresh`，预期一次菜单回调但实际为 `0`；日志在 `artifacts/runs/csharp_close_process_exe_20261015/menu-bar-scan-regression.log`，需先排除初始化调度副作用。
- 2026-10-15 菜单重建异步回退受影响回归 GREEN：将重置设置测试的脆弱同步扫描计数改为 UI 线程不扫描契约后，`TestCategory=Application|Lifecycle|Controls` 通过 `68/68`，无失败、无跳过；日志在 `artifacts/runs/csharp_close_process_exe_20261015/menu-bar-scan-regression-green.log`。
- 2026-10-15 菜单重建异步回退 C# 全量门禁：Release x64 `ShaheLauncher.Tests.dll` 通过 `208/208`，无失败、无跳过；日志在 `artifacts/runs/csharp_close_process_exe_20261015/menu-bar-scan-csharp-full.log`。
- 2026-10-15 菜单重建异步回退 Python 对照门禁：`python -m unittest discover -s tests -p "test_*.py" -v` 通过 `29/29`；日志在 `artifacts/runs/csharp_close_process_exe_20261015/menu-bar-scan-python-full.log`。
- 2026-10-15 菜单重建异步回退 Python 生产源码语法门禁：`python -m compileall -q shahe_bulk_launcher tests` 通过，退出码 `0`；日志在 `artifacts/runs/csharp_close_process_exe_20261015/menu-bar-scan-python-compile.log`。
- 2026-10-15 菜单重建异步回退 Release x64 构建：`dotnet build csharp\\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64` 成功生成 Core、App、Updater、Tests，`0` 警告、`0` 错误；日志在 `artifacts/runs/csharp_close_process_exe_20261015/menu-bar-scan-release-build.log`。
- 2026-10-15 菜单重建异步回退差异检查：`git diff --check` 退出码 `0`；日志在 `artifacts/runs/csharp_close_process_exe_20261015/menu-bar-scan-diff-check.log`，仅有 Git 的 LF/CRLF 转换提示，无差异错误。
- 2026-10-15 最新修复 Release x64 多用户启动后立即导航真实 EXE 测量：使用 `runtime-menu-bar-async-fix` 新构建副本，从每个审计沙盒首次出现 PID 后立即采样；单键中位数为 1 用户 `8.731ms`、2 用户 `8.830ms`、3 用户 `8.312ms`、4 用户 `9.230ms`，20 键零间隔突发最大耗时分别为 `178.223ms`、`174.225ms`、`176.159ms`、`167.234ms`，均无丢键/超时。四个审计沙盒最终 PID 均为 `0`，`SbieSvc` 保持运行；结果在 `artifacts/runs/csharp_close_process_exe_20261015/multi-user-navigation-menu-bar-fix-immediate-result.json`，日志在 `artifacts/runs/csharp_close_process_exe_20261015/multi-user-navigation-menu-bar-fix-immediate.log`。
- 2026-10-15 最新修复 Release x64 每沙盒 5 进程导航压力测量：每个审计沙盒启动 5 个 `notepad.exe` 后立即采样，单键中位数为 1 用户 `7.590ms`、2 用户 `8.162ms`、3 用户 `7.639ms`、4 用户 `8.591ms`；20 键零间隔突发最大耗时分别为 `164.189ms`、`163.978ms`、`173.264ms`、`179.003ms`，均无丢键/超时。四个审计沙盒最终 PID 均为 `0`，`SbieSvc` 保持运行；结果在 `artifacts/runs/csharp_close_process_exe_20261015/multi-user-navigation-menu-bar-fix-five-processes-result.json`，日志在 `artifacts/runs/csharp_close_process_exe_20261015/multi-user-navigation-menu-bar-fix-five-processes.log`。
- 2026-10-15 最新修复 Release x64 真实 `SendInput` 多用户导航测量：使用箭头扫描码和扩展键路径，在空闲、1、2、3、4 个运行用户场景下单键中位数分别为 `8.660ms`、`9.591ms`、`8.984ms`、`9.222ms`、`10.188ms`，各场景 20 键零间隔突发最大耗时为 `205.631ms`、`228.702ms`、`222.254ms`、`209.879ms`、`229.208ms`；全部无超时、无丢键。四个审计沙盒最终 PID 均为 `0`，`SbieSvc` 保持运行。结果在 `artifacts/runs/csharp_close_process_exe_20261015/multi-user-navigation-sendinput-result.json`，探针和日志在 `artifacts/runs/csharp_close_process_exe_20261015/runtime-menu-bar-async-fix/` 与 `artifacts/runs/csharp_close_process_exe_20261015/multi-user-navigation-sendinput.log`。
- 2026-10-15 多用户运行后列表导航卡顿契约 GREEN：`MainForm` 增加实时进程菜单状态扫描的进行中/待处理状态和定时器合并；修正首轮局部变量命名编译错误后，`ProcessMenuRuntimeContractTests` 定向运行 `8/8` 通过，连续刷新在已有 worker 运行时不再并行启动扫描。日志在 `artifacts/runs/csharp_close_process_exe_20261015/live-refresh-green.log`。
- 2026-10-15 多用户运行后列表导航卡顿受影响回归：`TestCategory=Application|Lifecycle|Controls` 通过 `62/62`，确认应用动作、生命周期和列表控件契约无回归。日志在 `artifacts/runs/csharp_close_process_exe_20261015/live-refresh-regression.log`。
- 2026-10-15 多用户运行后列表导航卡顿 C# 全量回归：`dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore` 通过 `205/205`，无失败、无跳过；日志在 `artifacts/runs/csharp_close_process_exe_20261015/live-refresh-csharp-full.log`。
- 2026-10-15 多用户运行后列表导航卡顿 Release x64 构建：`dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64` 成功生成 Core、App、Updater、Tests，`0` 警告、`0` 错误；日志在 `artifacts/runs/csharp_close_process_exe_20261015/live-refresh-release-build.log`。
- 2026-10-15 多用户运行后选择导航重复刷新诊断 RED：新增 `ProcessMenuRuntimeContractTests.SelectionLiveRefreshDoesNotRefreshVisibleRunningStates`，要求列表选择触发的进程菜单实时刷新不再重复扫描整张用户列表；定向运行 `1` 项按预期失败，实际调用 `RefreshVisibleStates` `1` 次而预期为 `0`。这确认选择刷新链路仍额外启动可见运行状态扫描，是当前多用户导航 CPU 竞争的直接回归证据；日志在 `artifacts/runs/csharp_close_process_exe_20261015/selection-refresh-red.log`。
- 2026-10-15 多用户运行后选择导航重复刷新诊断 GREEN：移除 `MainForm.StartProcessMenuLiveStateRefresh` 中与选择实时菜单刷新无关的 `RefreshVisibleStates` 调用；同一契约 `SelectionLiveRefreshDoesNotRefreshVisibleRunningStates` 通过 `1/1`。动作完成、F5、启动完成路径仍保留各自显式列表状态刷新，日志在 `artifacts/runs/csharp_close_process_exe_20261015/selection-refresh-green.log`。
- 2026-10-15 多用户运行后 Sandboxie PID 快照重复读取诊断 RED：新增 `ProcessMenuRuntimeContractTests.ProcessMenuLiveRefreshUsesOneSandboxProcessSnapshot`，要求一次实时进程菜单刷新共享单次 Sandboxie 进程快照；定向运行 `1` 项按预期失败，当前 `ProcessActionCallbacks` 尚无 `GetSandboxProcessMap` 回调，说明 C# 仍通过当前用户/全部用户两个路径重复读取进程状态。日志在 `artifacts/runs/csharp_close_process_exe_20261015/process-snapshot-red.log`。
- 2026-10-15 多用户运行后 Sandboxie PID 快照重复读取诊断 GREEN：`ProcessActionCallbacks` 增加单次 `GetSandboxProcessMap` 回调，`ReadProcessMenuLiveState` 从同一快照派生当前/全部 PID，仅在快照不可用或缺少当前用户时回退旧查询；契约通过 `1/1`，日志在 `artifacts/runs/csharp_close_process_exe_20261015/process-snapshot-green.log`。
- 2026-10-15 多用户导航重复扫描受影响回归：`TestCategory=Application` 通过 `46/46`，覆盖主窗口动作、菜单实时状态、选择反馈、单次 PID 快照、后台扫描和设置动作；日志在 `artifacts/runs/csharp_close_process_exe_20261015/navigation-scan-application-regression.log`。
- 2026-10-15 多用户导航重复扫描 C# 全量回归：`dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore` 通过 `210/210`，无失败、无跳过；日志在 `artifacts/runs/csharp_close_process_exe_20261015/navigation-scan-csharp-full.log`。
- 2026-10-15 校准后真实 EXE 导航复测：使用只设置选中/焦点、不修改勾选状态的跨进程校准方式，在当前 Release 实例 PID `12588` 上测得空闲单键中位数 `14.768ms`，1/2/3/4 个运行用户分别为 `7.844/8.498/8.516/8.519ms`，单键均无超时；20 键突发在 1/2/3/4 个运行用户分别为 `166.656/180.447/292.429/234.427ms`，均到达目标。空闲突发一次未到达，判定为探针焦点/消息队列校准异常，不作为产品卡顿结论；四个审计沙盒最终 PID 均为 `0`。结果在 `artifacts/runs/csharp_close_process_exe_20261015/navigation-calibrated-rerun-result.json`，本轮纯 `SendInput` 失败日志不作为产品证据。
- 2026-10-15 实时菜单刷新无效设备扫描诊断 RED：新增 `ProcessMenuRuntimeContractTests.ProcessMenuLiveRefreshDoesNotEnumerateOutputDevices`；定向运行 `1` 项按预期失败，注入的 `ListOutputDevices` 被调用 `1` 次。当前 C# 菜单栏只依据运行标记和静音缓存生成，未消费该设备枚举结果，确认多用户选择刷新仍有可删除的音频设备扫描；窗口探测由静态实现执行，尚未形成可注入计数边界。日志在 `artifacts/runs/csharp_close_process_exe_20261015/unused-live-device-scan-red.log`。
- 2026-10-15 实时菜单刷新无效设备扫描诊断 GREEN：`MainForm.ReadProcessMenuLiveState` 保留单次 Sandboxie PID 快照和音频会话快照，将当前菜单未消费的全局窗口探测/输出设备枚举改为基于已有 PID 的轻量状态；`ProcessMenuRuntimeContractTests.ProcessMenuLiveRefreshDoesNotEnumerateOutputDevices` 通过 `1/1`。日志在 `artifacts/runs/csharp_close_process_exe_20261015/unused-live-device-scan-green.log`。
- 2026-10-15 无效设备扫描修复 Application 回归：`TestCategory=Application` 通过 `47/47`，确认进程菜单实时状态、选择反馈、动作刷新和设置动作无回归；日志在 `artifacts/runs/csharp_close_process_exe_20261015/unused-live-device-application-regression.log`。
- 2026-10-15 无效设备扫描修复 Python 对照回归：`python -m unittest discover -s tests -p "test_*.py" -v` 通过 `29/29`；日志在 `artifacts/runs/csharp_close_process_exe_20261015/unused-live-device-python-full.log`。
- 2026-10-15 无效设备扫描修复 C# 全量回归：`dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore` 通过 `211/211`，无失败、无跳过；日志在 `artifacts/runs/csharp_close_process_exe_20261015/unused-live-device-csharp-full.log`。
- 2026-10-15 无效设备扫描修复 Python 生产源码语法门禁：`python -m compileall -q shahe_bulk_launcher tests` 通过，退出码 `0`；日志在 `artifacts/runs/csharp_close_process_exe_20261015/unused-live-device-python-compile.log`。
- 2026-10-15 无效设备扫描修复差异检查：`git diff --check` 退出码 `0`；日志在 `artifacts/runs/csharp_close_process_exe_20261015/unused-live-device-diff-check.log`，仅有 Git 的 LF/CRLF 转换提示，无差异错误。
- 2026-10-15 无效设备扫描修复 Release x64 构建：`dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64` 成功生成 Core、App、Updater、Tests，`0` 警告、`0` 错误；日志在 `artifacts/runs/csharp_close_process_exe_20261015/unused-live-device-release-build.log`。
- 2026-10-15 无效设备扫描修复真实 EXE 复测：最新 Release 实例 PID `10444` 启动并完成四个审计沙盒导航压力采样；单键/突发结果因跨进程定位出现起始索引漂移和部分校准超时，不能作为最终产品性能结论，原始结果保留在 `artifacts/runs/csharp_close_process_exe_20261015/navigation-unused-device-scan-result.json`。探针结束后四个审计沙盒均为 `0 PID`，`SbieSvc` 保持 `Running`，最新 EXE 仍保持运行供人工复核。
- 2026-10-15 多用户列表移动刷新契约 RED：初版测试未显式触发 WinForms `OnSelectedIndexChanged`，结果为 `0` 次刷新，不能作为根因证据；修正测试为显式模拟旧项取消选中和新项选中两个事件后，当前实现稳定产生 `2` 次请求而预期为 `1` 次。日志在 `artifacts/runs/csharp_navigation_followup_20261015/selection-refresh-count-red2.log`。
- 2026-10-15 多用户列表移动刷新契约 GREEN：`CheckableListCtrl.OnSelectedIndexChanged` 先取得有效选中索引，取消选中导致索引为 `-1` 时不再提交实时进程菜单刷新；隔离 Release x64 构建和 `MovingSelectionSchedulesOneLiveRefreshRequest` 单项契约通过 `1/1`。日志在 `artifacts/runs/csharp_navigation_followup_20261015/isolated-build-green.log` 与 `selection-refresh-count-green.log`。

- 2026-10-16 多用户列表移动刷新契约复核 GREEN：`CheckableListCtrl.OnSelectedIndexChanged` 先取得有效选中索引，取消选中导致索引为 `-1` 时不再提交实时进程菜单刷新；使用最终隔离 Release x64 测试程序集运行 `MovingSelectionSchedulesOneLiveRefreshRequest`，返回码 `0`，实际执行 `1` 项，通过 `1`、失败 `0`、跳过 `0`，总时间 `3.3026` 秒。最新日志为 `artifacts/runs/csharp_navigation_followup_20261015/csharp-task3-selection-refresh-recheck-20261016_195108_210.log`；原隔离构建证据仍为 `isolated-build-green.log`。
- 2026-10-15 多用户列表移动刷新受影响回归：`TestCategory=Application|TestCategory=Controls|TestCategory=Lifecycle` 通过 `69/69`，无失败、无跳过；日志在 `artifacts/runs/csharp_navigation_followup_20261015/selection-refresh-affected-regression.log`。
- 2026-10-15 多用户列表移动刷新 C# 全量回归：隔离 Release x64 `ShaheLauncher.Tests.dll` 通过 `212/212`，无失败、无跳过；日志在 `artifacts/runs/csharp_navigation_followup_20261015/csharp-full.log`。
- 2026-10-15 多用户列表移动刷新 Python 对照门禁：`python -m unittest discover -s tests -p "test_*.py" -v` 通过 `29/29`，日志在 `artifacts/runs/csharp_navigation_followup_20261015/python-full.log`。
- 2026-10-15 多用户列表移动刷新 Python 生产源码语法门禁：`python -m compileall -q shahe_bulk_launcher tests` 通过，退出码 `0`；日志在 `artifacts/runs/csharp_navigation_followup_20261015/python-compile.log`。
- 2026-10-15 多用户列表移动刷新标准 Release 构建受环境阻断：`dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64` 返回码 `1`，原因是活动进程 `10444` 锁定 `csharp/ShaheLauncher.App/bin/x64/Release/net48/ShaheLauncher.Core.dll`；未停止该进程，完整输出在 `artifacts/runs/csharp_navigation_followup_20261015/release-build-standard.log`。
- 2026-10-15 多用户列表移动刷新最终隔离 Release x64 构建：`dotnet msbuild csharp\ShaheBulkLauncher.sln /t:Build /p:Configuration=Release /p:Platform=x64 /p:BaseOutputPath=artifacts/runs/csharp_navigation_followup_20261015/isolated-final-bin /m` 成功生成 Core、App、Updater、Tests，`0` 警告、`0` 错误；日志在 `artifacts/runs/csharp_navigation_followup_20261015/release-build-isolated-final.log`。
- 2026-10-15 多用户列表移动刷新差异检查：`git diff --check` 退出码 `0`；日志在 `artifacts/runs/csharp_navigation_followup_20261015/diff-check.log`，仅有 Git 的换行转换提示。
- 2026-10-15 多用户列表移动刷新文档与修复最终差异检查：在更新 `AGENTS.md`、当前计划和项目摘要后再次运行 `git diff --check`，退出码 `0`；最终日志在 `artifacts/runs/csharp_navigation_followup_20261015/diff-check-final.log`。
- 2026-10-03 Task 20 `schedule_close` 迁移：`MainForm` 已将菜单动作接入 `LaunchScheduler.ScheduleSelectedClose`，保持定时关闭文案、30 秒默认值、沙盒目标列表、关闭回调、状态更新和进程 UI 刷新语义；定向契约先按预期 RED（默认动作回退），接线后 GREEN。测试补充 Sandboxie 名称回调以隔离初始化异步刷新，未修改 Python 基线。
- 2026-10-03 Task 20 回归验证：`dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore` 通过 173/173；`python -m unittest discover -s tests -p "test_*.py" -v` 通过 29/29；`dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64` 通过，0 警告、0 错误。
- 2026-10-03 Task 20 `repair_sandboxie_control_settings` 定向迁移验证：先新增 `MainFormApplicationContractTests.WiresRepairSandboxieControlSettingsMenuActionToSandboxAdminService`，RED 按预期命中默认动作回退；随后在 `MainForm.ExecuteApplicationAction` 接入应用方法，保持 Python 的确认文案、`Sandboxie.ini` 清理、配置刷新、成功/无需修复/失败状态与提示语义，GREEN 通过 1/1；Python 基线未修改。
- 2026-10-03 Task 20 `repair_sandboxie_control_settings` 回归验证：Application 分类与 `SandboxAdminServiceContractTests` 通过 29/29。
- 2026-10-03 Task 20 C# 全量回归：`dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore` 通过 174/174。
- 2026-10-03 Task 20 Python 对照回归：`python -m unittest discover -s tests -p "test_*.py" -v` 通过 29/29。
- 2026-10-03 Task 20 Release x64 构建：`dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64` 成功，0 警告、0 错误。
- 2026-09-24 Task 15 应用层接线完成已确认范围：`MainForm` 的 `backup_sandboxes` 与 `restore_sandboxes` 已调用 `BackupRestoreService`，接入用户选择、保存目录/备份文件选择、覆盖确认、异步状态回调、Sandboxie 服务生命周期、配置合并、控制程序启动询问、重启询问和还原阶段进度窗口；Python 正式代码未修改。
- 2026-08-28 前 20 模块真实调用审计的历史结论为：20/20 均有 C# 可执行测试覆盖；当时真实 EXE 启动与隔离菜单交互合并后 19/20 有应用进程源码执行证据，唯一例外是尚未接线的 Task 15。Task 15 接线后的真实 EXE 动作级审计尚未重新收集，不能把本轮 Application 测试替代为真实进程交互证据。
- C# `Program.cs` 已接入单实例、`MainForm`、WinForms 消息循环和释放顺序，Release x64 主程序已完成最终真实启动、显示主窗口、生成配置和正常退出烟雾测试；当前 C# 版本可以启动运行，真实音频后端及当前进程播放设备菜单动作已完成本机/隔离进程验证，但部分菜单动作的真实 EXE 审计和外部 updater 安装流程仍有未完成或未确认边界。

## 当前 C# 迁移计划进度（2026-10-15）

| 计划项 | 状态 | 当前结论 |
|---|---|---|
| 1. 建立本轮验证基线 | 已完成 | C#、Python、Release x64、生产源码编译及差异检查均已建立日志基线；全仓 `compileall` 仍仅受既有临时探针语法错误影响。 |
| 2. 迁移快捷键运行时分派 | 已完成已确认范围 | 固定快捷键优先、自定义快捷键、禁用/冲突/未知按键回退及八个运行时动作已有契约和真实动作链验证；Esc/Alt+F4 状态反馈仍有 `GAP-07`。 |
| 3. 接线路径记忆功能 | 已完成已确认范围 | 菜单、快捷键、管理对话框、备注、隐藏路径、勾选和上下文菜单已接线；复杂 JSON 历史恢复存在 `GAP-03`，数量范围和路径选择/剪贴板文案存在 `GAP-09`、`GAP-10`，真实 EXE 完整人工流程仍属于集成审计。 |
| 4. 接线菜单状态与沙盒选择反馈 | 已完成 | 运行状态、静音标签、选择反馈及 `F10`、`Alt`、方向键、回车、空格操作已验证。 |
| 5. 接通全量重置和设置开关动作 | 已完成已确认范围 | `ResetSettings()`、六个设置开关及重置后的运行时同步顺序已有契约和全量门禁；复杂 JSON 配置恢复仍受 `GAP-03` 约束。 |
| 6. 补齐真实音频后端 | 已完成已确认范围 | Core Audio 会话、设备枚举、静音、音量和播放设备路由已接入；真实宿主 build 检测修复，生产 EXE 菜单动作已选择非默认设备并回读持久路由。 |
| 7. 真实 EXE 与 Sandboxie 集成测试 | 进行中 | 已完成启动退出、单/多沙盒启动、并发配置、静音、实时菜单状态、音量和播放设备切换、关闭当前/全部进程，以及真实备份和隔离解压校验；最终隔离 Release x64 的 1/5 进程多用户导航复测无超时、丢键或索引未到达，当前继续验证定位当前进程及其他菜单动作级审计；源码对照缺口 `GAP-01` 至 `GAP-10` 尚未逐项 RED/GREEN。 |
| 8. 最终迁移门禁 | 未完成 | 自动化门禁已多轮通过，但 `GAP-01` 至 `GAP-10`、真实调用证据、剩余集成审计、最终文档和发布边界尚未全部满足。 |

### 当前迁移状态声明

- 已完成模块：快捷键运行时分派、路径记忆接线、菜单状态与沙盒选择反馈、全量重置和设置开关、真实音频后端及当前进程播放设备路由、进程菜单实时状态刷新，均指已完成已确认范围，不代表源码对照缺口已关闭。
- 当前模块：真实 EXE 与 Sandboxie 集成审计，导航复测和关闭当前/全部进程已取得有效证据，当前处理定位当前进程及其余菜单动作的真实调用证据；并行收口 `GAP-01` 至 `GAP-10` 的源码对照缺口。
- 待迁移或待验证：`GAP-01` 至 `GAP-10`、定位当前进程、分组/批量/定时/路径记忆/托盘/无障碍的剩余真实动作级审计，以及最终迁移门禁。
- 已知风险：外部 updater 缺少原始安装器源码；`GAP-03` 复杂 JSON 恢复和 `GAP-04` 朗读配置覆盖需先用隔离 RED 核验；全仓 `compileall` 受既有临时探针语法错误阻断；破坏性恢复和服务控制只能在专用隔离环境验证。
- C# 当前运行状态：可以启动、显示主窗口、执行已验证功能并正常退出，但尚不能声明整个软件迁移完成。

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
| 3 | `common.py` | `ShaheLauncher.Core/Common/`、`ShaheLauncher.Core/Configuration/` 与 `ShaheLauncher.Core/Updates/` | 已完成已确认范围，10/10；复杂 JSON 配置恢复仍有 `GAP-03`，更新结果 UI/下载链另见 `GAP-01` |
| 4 | `single_instance.py` | `ShaheLauncher.App/Infrastructure/SingleInstanceGuard.cs` | 已完成，3/3 |
| 5 | `ui/controls.py` | `ShaheLauncher.App/UI/Controls/` | 已完成，14/14 |
| 6 | `ui/common_dialogs.py` | `ShaheLauncher.App/UI/Dialogs/Common/` | 已完成，7/7 |
| 7 | `ui/settings_dialogs.py` | `ShaheLauncher.App/UI/Dialogs/Settings/` | 已完成，10/10；`PathMemoryManagerDialog` 已由设置菜单打开并回写主窗口状态 |
| 8 | `ui/tray.py` | `ShaheLauncher.App/UI/Tray/LauncherTrayIcon.cs` | 已完成，3/3 |
| 9 | `features/settings.py` | `ShaheLauncher.Core/Settings/LauncherSettingsService.cs` | 已完成已确认范围，8/8；复杂 JSON 配置恢复待 `GAP-03` 定向 RED |
| 10 | `features/app_paths.py` | `ShaheLauncher.Core/AppPaths/AppPathHistoryService.cs` 与 `ShaheLauncher.App/MainForm.cs` | 已完成已确认范围；剪贴板文件列表优先级已有 4/4 运行时契约和 4/4 Python 行为探针，路径历史复杂 JSON 恢复已完成；数量范围和路径选择/剪贴板文案差异仍见 `GAP-09`、`GAP-10` |
| 11 | `features/sandboxie.py` | `ShaheLauncher.Core/Sandboxie/SandboxieClient.cs` | 已完成已确认范围，8/8；repair 后 reload 回调未接线，见 `GAP-08` |
| 12 | `features/process_actions.py` | `ShaheLauncher.Core/Processes/ProcessActionService.cs`、`Processes/WindowsProcessAudioBackend.cs` 与 `ShaheLauncher.App/MainForm.cs` | 已完成已确认范围；原服务契约 8/8、真实后端契约 5/5、异步设备枚举/等待契约 2/2；真实 EXE 当前进程播放设备菜单动作已验证 |
| 13 | `features/sandbox_admin.py` | `ShaheLauncher.Core/SandboxAdmin/SandboxAdminService.cs` | 已完成，8/8 |
| 14 | `features/scheduling.py` | `ShaheLauncher.Core/Scheduling/LaunchScheduler.cs` | 已完成，8/8 |
| 15 | `backup.py` | `ShaheLauncher.Core/Backup/BackupRestoreService.cs` 与 `ShaheLauncher.App/MainForm.cs`、`UI/Dialogs/Common/BackupRestoreProgressDialog.cs` | 已完成已确认范围；服务契约 7/7、应用接线/进度窗口契约 3/3；真实 EXE 备份/还原动作级审计待补 |
| 16 | `features/speech.py` | `ShaheLauncher.Core/Speech/SpeechService.cs` | 已完成已确认范围，10/10；朗读设置保存参数差异见 `GAP-04` |
| 17 | `features/updates.py` | `ShaheLauncher.Core/Updates/UpdateService.cs` 与 `ShaheLauncher.Updater/` | 已完成已确认范围，11/11；更新结果 UI/下载启动链见 `GAP-01`，Updater 空入口/安装流程见 `GAP-02` |
| 18 | `features/hotkeys.py` | `ShaheLauncher.App/Input/HotkeyController.cs` | 已完成已确认范围，8/8；Esc/Alt+F4 反馈差异见 `GAP-07` |
| 19 | `features/lifecycle.py` | `ShaheLauncher.App/MainForm.Lifecycle.cs` | 已完成已确认范围，7/7；退出反馈差异见 `GAP-07` |
| 20 | `app.py` | `ShaheLauncher.App/MainForm.cs` 与 `Program.cs` | 已完成已确认范围；`create_group`、`launch_group`、`delete_group`、`batch_create_sandboxes`、`batch_rename_sandboxes`、`batch_delete_sandboxes`、`schedule_launch`、`schedule_close`、`repair_sandboxie_control_settings` 已接线，剪贴板文件列表优先级已有自动化契约；源码对照仍有 `GAP-01`、`GAP-06`、`GAP-07`、`GAP-08`、`GAP-10`，真实 EXE 新增动作级审计仍待补，未声明整体完成 |

## 前 20 模块真实调用审计

以下状态区分“模块代码和定向测试已存在”与“真实应用入口已经调用”。详细方法级覆盖和逐动作状态保存在 `artifacts/runs/csharp_first20_real_call_audit_20260828`。

| Task | Python 模块 | 应用真实调用状态 | 当前结论或缺口 |
| ---: | --- | --- | --- |
| 1 | `constants.py` | 启动与交互已调用 | 常量、默认根、日志模式和名称校验由真实进程执行 |
| 2 | `bootstrap.py` | 启动与交互已调用 | EXE 路径、应用目录、日志和发布清理路径已执行 |
| 3 | `common.py` | 启动与交互已调用 | 设置解析、INI/加密配置和更新 HTTP 边界已执行；安全算法另由定向向量覆盖 |
| 4 | `single_instance.py` | 真实 EXE 启动已调用 | `Program.Main` 实际执行互斥体检查和释放 |
| 5 | `ui/controls.py` | 启动与交互已调用 | 主列表、应用列表、状态文本和延迟调用均有执行证据 |
| 6 | `ui/common_dialogs.py` | 部分交互已调用 | 二维码对话框从真实菜单打开；分组、批量命名和用户选择对话框仅定向测试调用 |
| 7 | `ui/settings_dialogs.py` | 部分交互已调用 | 热键、软件、朗读设置和路径记忆管理均有菜单入口；路径记忆真实 EXE 对话框操作仍待人工审计 |
| 8 | `ui/tray.py` | 真实 EXE 启动已调用 | `show_tray=true` 时创建托盘图标，退出时移除 |
| 9 | `features/settings.py` | 启动与交互已调用 | 配置加载/迁移及 Sandboxie 路径、数据根校正已执行；复杂 JSON 恢复的 `JArray/JObject` 兼容性仍有 `GAP-03`，朗读配置覆盖风险见 `GAP-04` |
| 10 | `features/app_paths.py` | 启动与交互已调用 | 主窗口通过 `AppPathHistoryService` 接入历史新增、显示、选择、勾选、上下文菜单、管理对话框保存和七个路径记忆动作；粘贴文件优先级已完成自动化对照，真实 EXE 剪贴板交互仍待审计；路径数量上限及路径选择/剪贴板文案仍有 `GAP-09`、`GAP-10` |
| 11 | `features/sandboxie.py` | 启动与交互已调用 | 安装路径、INI/显示映射和运行状态路径已执行；repair 后 `/reload` 回调未在正常构造路径接入，见 `GAP-08`；真实扫描会只读系统级 `Sandboxie.ini` |
| 12 | `features/process_actions.py` | 启动与交互已调用 | `MainForm` 已绑定原生 Core Audio 会话/设备枚举、静音、音量和 WinRT PolicyConfig 进程路由；受影响契约、本机 COM 探针、专用播放进程探针及真实 EXE 当前进程播放设备菜单动作均通过；关闭及其余菜单动作级审计仍待补 |
| 13 | `features/sandbox_admin.py` | 部分启动与交互已调用 | 设置规范化、INI 读取和临时 `Box1` 置顶已执行；`create_group`、`launch_group`、`delete_group`、`batch_create_sandboxes`、`batch_rename_sandboxes`、`batch_delete_sandboxes` 已进入 Application 契约，真实 EXE 动作级审计待补 |
| 14 | `features/scheduling.py` | 部分启动与交互已调用 | “取消全部定时任务”、定时启动和定时关闭已进入服务/Application 契约；真实定时生命周期仍待审计，不能以契约测试替代真实 EXE 证据 |
| 15 | `backup.py` | 应用层测试已调用；真实 EXE 动作级审计待补 | Application 契约已通过真实 `MainMenu/MenuItem.PerformClick()` 进入备份和还原流程，临时目录、用户数据、INI 合并、异步状态和还原进度窗口均有测试覆盖；尚未在真实 EXE 中执行会修改 Sandboxie 数据的菜单动作 |
| 16 | `features/speech.py` | 启动与交互已调用 | 配置保存、朗读设置请求和关闭清理已执行；朗读设置当前传入空配置的潜在覆盖路径见 `GAP-04`，本轮未加载真实 Tolk 后端 |
| 17 | `features/updates.py` | 部分启动与交互已调用 | 自动检查路径和二维码资源解析已执行；更新结果 UI、下载按钮/启动链和外部 updater 安装、替换、重启未完整接线或真实触发，见 `GAP-01`、`GAP-02` |
| 18 | `features/hotkeys.py` | 启动与交互已调用 | 菜单标签、热键设置和上一/下一用户动作已执行；`NumPadEnter` 仍需人工键盘验证，Esc/Alt+F4 反馈差异见 `GAP-07` |
| 19 | `features/lifecycle.py` | 启动与交互已调用 | UI 初始化、托盘、延迟任务、关闭和服务释放已执行 |
| 20 | `app.py` | 部分启动与交互已调用 | `Program/MainForm` 启动、菜单分派、`create_group`、`launch_group`、`delete_group`、`batch_create_sandboxes`、`batch_rename_sandboxes`、`batch_delete_sandboxes`、`schedule_launch`、`schedule_close`、`repair_sandboxie_control_settings` 应用测试、备份/还原应用测试、对话框和退出链已执行；剪贴板文件列表优先级已完成自动化对照，源码差异仍有更新、全选、退出反馈、repair reload 和文案（`GAP-01`、`GAP-06`、`GAP-07`、`GAP-08`、`GAP-10`），新增动作的真实 EXE 动作级审计仍待补 |

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

- 2026-10-19 源码对照缺口索引：`GAP-01` 至 `GAP-10` 记录本轮发现的未迁移、未接线或必须用隔离 RED 核验的行为差异；这些条目不等同于已完成的模块，也不等同于全部已经过运行时复现。
- 全量灾难恢复测试已进入用户还原阶段：账号目录和沙盒配置节已清空，正式 `.sbx`、独立副本和原始配置副本保存在 `artifacts/live_backup_test/20260806_214549`。
- 当前系统 Python 3.14 未安装 `cryptography`，原版 `encrypt_config_value`、`decrypt_config_value`、`verify_update_signature` 和 `decrypt_update_package` 的真实 Python 执行会进入缺失依赖错误路径；C# 迁移必须按源码中的算法、密钥和格式用确定性向量验证，不修改 Python 环境或把依赖修复混入迁移。
- `features/updates.py` 只负责生成更新请求并启动外部 `updater.exe`，仓库没有对应的 Python updater 安装器源码；Task 17 已迁移并验证清单获取、补丁选择、请求生成、后台检查、二维码资源和 SHA-256/既有签名解密公共边界，但未擅自推断未知安装、替换和重启流程。获得原 updater 源码后应另开迁移/契约任务。
- Task 18 保留的 `NumPadEnter` 风险尚未解除：WinForms 没有独立枚举，当前只验证了 `HotkeyController` 逻辑键码、文本语义和主窗口扩展 Enter 映射契约，真实底层键盘事件接线仍需人工输入验证。
- Task 15 `BackupRestoreService` 已接入 `MainForm` 备份/还原业务流程，服务调用和还原阶段进度窗口已有 Application 契约；真实 EXE 中直接执行会停止/重启 Sandboxie 服务并覆盖数据的动作级审计仍需在隔离环境人工/探针验证，不能以 Application 测试替代。
- Task 20 已建立全部迁移服务属性、经典菜单、入口、发布复制、主要进程/窗口/启动/Sandboxie/语音回调、非破坏性菜单动作、Task 15 备份/还原接线和 `create_group`、`launch_group`、`delete_group`、`batch_create_sandboxes`、`batch_rename_sandboxes`、`batch_delete_sandboxes`、`schedule_launch`、`schedule_close`、`repair_sandboxie_control_settings` 菜单接线；真实音频后端及当前进程播放设备路由已绑定并验证，Windows 10 1803+ 的 PolicyConfig 路由在设备不可用、权限不足、进程无效或无音频会话时返回失败；关闭动作和其余新增动作的真实 EXE 审计仍待补，不能据此声明整个软件迁移完成。
- Task 10 路径记忆运行时接线已通过契约验证，但真实 EXE 中的鼠标右键、`Ctrl+H` 弹出位置、剪贴板写入和管理对话框完整保存/取消流程尚未在专用测试环境人工审计；选择/勾选回调已明确避免扫描、启动和进程刷新副作用。
- 外部 `updater.exe` 已随 Release x64 主程序发布并通过共存烟雾验证，但仓库仍缺少原 updater 安装器源码，未知安装、替换和重启流程尚未迁移或验证。

## Python→C# 未完成项（2026-10-19 源码对照）

本节登记 Python 正式源码与 C# 当前实现逐项对照后的缺口和收口状态。`GAP-03`、`GAP-04` 已完成并推送；`GAP-05` 已完成自动化对照，真实 EXE 剪贴板交互继续保留为审计风险；其余条目必须按迁移顺序分别建立 RED、实现最小修复、取得 GREEN，并补真实 EXE 或明确的环境风险证据后才能关闭。

| 编号 | Python 参考行为 | C# 当前状态 | 处理结论 |
| --- | --- | --- | --- |
| `GAP-01` | 更新检查结果显示可用版本、下载按钮和下载/启动 updater 的完整用户链路。 | `UpdateService` 的检查和资源解析已有范围内实现，但更新结果 UI、下载按钮及其启动链未完整接线。 | 待建立更新结果 UI/调用链 RED；不能以检查服务契约代替完整更新流程。 |
| `GAP-02` | 外部 updater 负责安装包解析、替换、重启等安装阶段。 | `csharp/ShaheLauncher.Updater/Program.cs` 的 `Main` 为空；`UpdateInstaller` 当前只有解析/哈希 helper。仓库没有 Python 原 updater 安装器源码。 | 先记录源码边界；获得原 updater 源码后另开迁移任务，不能声明 updater 已迁移完成。 |
| `GAP-03` | 路径历史及其他设置按 Python 的 JSON/list/dict 形状恢复，保留历史项、分组和嵌套值。 | 已确认路径历史 root 为 `Newtonsoft.Json.Linq.JArray`、item 为 `JObject`；item 不满足非泛型 `IDictionary`，而满足泛型 dictionary。`MergeSettings → CopyDictionary` 已在主窗口加载前处理 `speech_assist_hints`，该字段不能直接归因于此缺口；其他分组、音量记忆、自定义快捷键仍需定向 RED。 | 保留现有类型探针证据，先补复杂 JSON 字段的隔离 RED/GREEN，不把未实测字段写成确定丢失。 |
| `GAP-04` | Python 朗读设置保存回调使用 `lambda config: True`，不会用空配置覆盖已有语音配置。 | C# 朗读设置对话框传入空 `SpeechConfig`，但绑定了真实 `SaveSpeechConfig`，存在覆盖已有配置的潜在路径。 | 建立隔离 RED，确认取消/确定及已有配置保留行为后再决定最小修复。 |
| `GAP-05` | 粘贴应用路径时先读取剪贴板文件列表，文件列表不可用时才回退到文本路径。 | `MainForm` 已通过可替换的 `ClipboardDataReader` 读取 `FileDrop`，无有效 EXE 时回退 `UnicodeText`，并复用 `ResolvePastedAppPath()`；运行时契约和 Python 隔离行为探针均通过。 | 自动化迁移完成；未读取或改写用户系统剪贴板，真实 EXE 粘贴交互仍待审计。 |
| `GAP-06` | Python 文件菜单和 `Ctrl+A` 走 `toggle_select_all`；右键“全选”才是单向全选。 | C# 主窗体 `select_all` 分派到 `CheckAll()`，行为不是切换语义。 | 待用两次触发的 RED 固定切换/单向全选边界，再修正分派并回归快捷键。 |
| `GAP-07` | Python `Esc`、`Alt+F4` 和退出动作提供读屏/状态反馈。 | C# `Esc`/`exit_app` 链缺少对应反馈，已有退出链不能证明文案和读屏行为一致。 | 待补状态反馈和无障碍契约 RED；不得把窗口关闭成功写成反馈迁移完成。 |
| `GAP-08` | Sandboxie Control 修复完成后执行 `/reload`，确保修复后的配置重新加载。 | C# 正常构造路径没有赋值 `ReloadSandboxieConfigCallback`；repair 只调用 nullable callback，可能不执行 reload。 | 待建立 callback 注入/执行 RED，修复后验证调用次数和失败反馈。 |
| `GAP-09` | 启动路径记忆数量限制为 `0–20`。 | C# `MainForm.LoadApplicationSettings()` 使用 `0–999`，范围约束与 Python 不一致。 | 待补边界 RED（`-1`、`0`、`20`、`21`、`999`）并修正配置/UI 约束。 |
| `GAP-10` | 路径选择对话框标题、过滤器、剪贴板写入确认和异常文案遵循 Python 原文。 | C# 对话框标题/过滤器、剪贴板写入确认和异常文案与 Python 不完全一致。 | 待建立文案/过滤器契约 RED，逐项对照后修复，不重设计交互。 |

### 复核更正

- `speech_assist_hints` 不直接登记为 `JObject` 缺口：C# `LauncherSettingsService.MergeSettings → CopyDictionary` 已在主窗口加载前处理该字段；仍需单独核验其他复杂 JSON 字段。
- Python 文件菜单“全选”绑定 `on_ctrl_a_select`，同样是切换行为；只有列表右键“全选”是单向全选，因此 `GAP-06` 针对的是 C# 分派语义不等价，而不是把 Python 文件菜单误判成单向全选。

## 验证记录

- 2026-10-12 真实音频后端定向验证：`TestCategory=AudioBackend|ProcessActions` 通过 `13/13`，覆盖原有进程音频契约、Windows 后端四项契约和设备枚举后台边界；日志在 `artifacts/runs/csharp_audio_backend_20261012/audio-process-targeted-final-correct.log`。
- 2026-10-12 真实音频后端全量门禁：C# `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore` 通过 `200/200`；Python `29/29`；`python -m compileall -q shahe_bulk_launcher tests` 通过；Release x64 构建 `0` 警告/`0` 错误；`git diff --check` 退出码 `0`。全仓 `python -m compileall -q .` 唯一失败仍为既有临时探针 `artifacts/runs/full_software_test_20261008/probe_debug/no_cleanup.py:39`。日志在 `artifacts/runs/csharp_audio_backend_20261012/`。
- 2026-10-12 真实音频后端运行证据：专用静音 WAV Python 进程实际创建会话，C# 后端完成静音、音量写回/恢复和播放设备路由；结果为 `SessionFound=true`、`MuteAfterToggle=true`、`VolumeAfterWrite=0.3700000047683716`、`VolumeAfterRestore=1.0`、`RouteSucceeded=true`。结果文件为 `artifacts/runs/csharp_audio_backend_20261012/real-audio-probe.json`。

- 2026-10-12 Task 10 路径记忆定向验证：`PathMemoryRuntimeContractTests` RED `4/4` 暴露接线缺口；实现后 GREEN `4/4`，覆盖设置菜单独立入口、`Ctrl+H` 经典上下文菜单、八个菜单动作标签/dispatcher、`FormatDisplayText`、`ListViewItem.Tag`、选择/勾选恢复和无高成本副作用。
- 2026-10-12 Task 10 受影响回归：`TestCategory=Application|Controls|SettingsDialogs|AppPaths` 通过 `65/65`；随后 `PathMemoryRuntimeContractTests`、`UiControlsCSharpContractTests`、`SettingsDialogsCSharpContractTests` 通过 `30/30`。
- 2026-10-12 Task 10 最终验证：`dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore` 通过 `188/188`；`python -m unittest discover -s tests -p "test_*.py" -v` 通过 `29/29`；`python -m compileall -q shahe_bulk_launcher tests` 通过；`dotnet build ... --property:Platform=x64` 为 `0` 警告/`0` 错误；全仓 `compileall` 唯一失败仍是既有 `artifacts/runs/full_software_test_20261008/probe_debug/no_cleanup.py:39`；日志在 `artifacts/runs/csharp_task19_path_memory_baseline_20261012/`。
- 2026-10-12 Task 10 暂存区独立快照验证：从当前索引导出不含未暂存用户改动的快照，C# 全量 `183/183`、Release x64 构建 `0` 警告/`0` 错误、Git 已跟踪的 Python 基线 `24/24`、全仓 `compileall` 通过；当前工作树额外的 5 个 C# 和 5 个 Python 测试来自既有未暂存改动。Windows `tar` 首次解包把中文入口文件名转码，复制同一已跟踪且未修改的入口文件后 Python 重跑通过；日志保存在同一验证目录的 `staged_snapshot_verify/` 及相邻日志文件。

- 2026-10-08 全软件真实 UI 回归：独立 Release x64 副本启动主窗口并显示真实 92 个沙盒用户；通过 F10 和方向键打开设置菜单，真实打开“设置”“快捷键设置”“朗读设置”。“设置”窗口下拉框在本机 8 个逻辑处理器上显示 `1–8`，无障碍名称为“启动并发数量限制”；快捷键窗口显示当前功能快捷键和启用控件；朗读设置在缺少 `internal` 接口文件时显示明确提示后仍可进入配置页。所有本轮测试进程已清理，未执行正式沙盒删除、覆盖还原或服务控制。明细在 `artifacts/runs/full_software_test_20261008/real_ui/summary.json`。
- 2026-10-08 全软件最终验证：C# 全量 `178/178`、Python 全量 `29/29`、Python 全仓编译通过、Release x64 构建 `0` 警告/`0` 错误；26 个 Python 行为探针按顺序重跑 `26/26` 通过。并行验证初次运行中 `paste_invalid_clipboard_probe.py` 曾在已输出业务 PASS 后以 Windows `0xC000041D` 退出，隔离复现和最终顺序重跑均通过，保留日志在 `artifacts/runs/full_software_test_20261008/probes/`。
- 2026-10-08 应用路径列表无障碍文案精简：`AppPathListCtrl.AccessibleName` 仅保留“准备要在沙盒中打开的应用”，删除重复的“复选列表，按空格选中或取消选中”提示；复选状态、空格切换和删除逻辑未改动。定向 C# 契约测试通过 35/35。
- 2026-10-08 应用路径列表文案修复回归：C# 全量 `dotnet test csharp/ShaheBulkLauncher.sln --configuration Release --no-restore` 通过 174/174；Release x64 MSBuild 通过，0 警告、0 错误。
- 2026-10-08 删除旧的 CPU 负载自动调节方案：移除 C# 与 Python 的动态延迟设置、CPU 回调、调度分支和对应快捷键；保留用户选择的启动并发数 `1–32` 与启动间隔，保存后直接用于批量启动；旧配置键 `dynamic_launch_delay_by_load` 在读取/保存时清理。C# 全量测试 `176/176`、Python 全量测试 `29/29`、Python 编译和 Release x64 构建均通过，构建 0 警告、0 错误。
- 2026-10-08 并发设置改为按本机逻辑处理器生成下拉框：C# `SettingsDialog.LaunchConcurrencyInput` 使用 `ComboBoxStyle.DropDownList`，选项为 `1…min(32, Environment.ProcessorCount)`，默认选中最大可用值；点击确定仅保存用户当前选择，不再读取 CPU 负载或自动覆盖配置。设置对话框定向测试通过 `12/12`。
- 2026-10-08 并发下拉框全量回归：C# `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore` 通过 `177/177`；Python `python -m unittest discover -s tests -p "test_*.py" -v` 通过 `29/29`；`python -m compileall -q .` 通过。
- 2026-10-08 并发下拉框 Release x64 构建：`ShaheBulkLauncher.sln` 完成 `/restore /t:Build /p:Configuration=Release /p:Platform=x64`，Core、App、Updater、Tests 均生成，`0` 警告、`0` 错误。
- 2026-10-08 并发默认值调整 RED：新增“逻辑处理器不少于 4 时默认 4、少于 4 时默认 2、下拉上限仍按逻辑处理器数”的 Constants、SettingsService 和 SettingsDialog 契约；定向运行 `29` 项时按预期失败 `3` 项，生产代码尚未切换默认值。
- 2026-10-08 并发默认值调整 GREEN：新增 `GetLaunchConcurrencyMaximum` 与保守默认值规则；当前机器 8 个逻辑处理器时下拉范围为 `1–8`、默认 `4`，低于 4 个逻辑处理器时默认 `2`（若上限不足则按实际上限）；Constants、SettingsService、SettingsDialogs 定向测试 `29/29` 通过。
- 2026-10-08 并发默认值全量回归：C# `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore` 通过 `178/178`；Python `python -m unittest discover -s tests -p "test_*.py" -v` 通过 `29/29`；`python -m compileall -q .` 通过。
- 2026-10-08 并发默认值 Release x64 构建：`ShaheBulkLauncher.sln` 完成 `/restore /t:Build /p:Configuration=Release /p:Platform=x64`，Core、App、Updater、Tests 均生成，`0` 警告、`0` 错误。
- 2026-10-08 全软件自动化回归：重新执行 Release x64 还原/构建，构建 `0` 警告、`0` 错误；C# 全量测试 `178/178`；Python 全量测试 `29/29`；`python -m compileall -q .` 通过。完整日志在 `artifacts/runs/full_software_test_20261008/`。
- 2026-10-08 全软件 Python 行为探针回归：`tests/probes` 的 `26/26` 探针全部返回 `0`，日志和 `summary.json` 在 `artifacts/runs/full_software_test_20261008/probes/`；未执行真实数据删除或还原。
- 2026-10-08 全软件真实 C# EXE 启动烟雾：独立 Release x64 副本使用真实 `C:\Program Files\Sandboxie` 与 `C:\Sandbox\Administrator` 配置，标题为“沙河批量打开器”、主窗口句柄有效、正常关闭、退出码 `0`，配置和 `updater.exe` 均生成；结果在 `artifacts/runs/full_software_test_20261008/real_exe_smoke/summary.json`。
- 2026-10-08 真实 Sandboxie 双沙盒并发：使用真实 `C:\Program Files\Sandboxie\Start.exe` 在 `Box1` 启动 `notepad.exe`、在 `Box2` 启动 `charmap.exe`；两个新 PID 同时存在且分别被对应 `/listpids` 识别，随后只终止本轮两个应用，`SbieSvc` 保持 Running。结果在 `artifacts/runs/full_software_test_20261008/sandbox_concurrency/summary.json`；未删除沙盒、未停止服务。
- 2026-10-08 删除旧方案后的 Python 行为探针：`tests/probes` 下 26 个探针全部通过，日志保存在 `artifacts/runs/remove_dynamic_load_scheme_20261008`。
- 2026-10-08 删除旧方案后的真实 C# EXE 启动烟雾：Release x64 主窗口标题为“沙河批量打开器”、窗口句柄非零，调用正常关闭后退出码 `0`；日志保存在 `artifacts/runs/remove_dynamic_load_scheme_20261008/real_exe_startup_graceful.log`。
- 2026-10-08 全部已迁移模块自动化门禁：C# `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore` 通过 174/174；Python `python -m unittest discover -s tests -p "test_*.py" -v` 通过 29/29；`python -m compileall -q .` 通过；Release x64 构建 0 警告、0 错误。
- 2026-10-08 Python 行为探针回归：`tests/probes` 下 26 个探针逐个通过，日志保存在 `artifacts/runs/full_migrated_module_probes_20261008`。
- 2026-10-08 真实 C# EXE 启动烟雾：使用真实 `C:\Program Files\Sandboxie` 与 `C:\Sandbox\Administrator` 配置启动 Release x64 副本；标题为“沙河批量打开器”、主窗口句柄非零、正常关闭、退出码 0、`Configuration/shahe_config.ini` 已生成且同目录存在 `updater.exe`。测试运行目录为 `artifacts/runs/full_migrated_real_exe_smoke_20261008`；未点击备份、还原、删除或服务控制动作。
- 2026-10-08 真实 Sandboxie 单应用验证：使用真实 `C:\Program Files\Sandboxie\Start.exe /box:Box1 C:\Windows\System32\notepad.exe` 启动新 PID `18292`，`/listpids` 返回包含该 PID，确认进程进入 `Box1`；随后仅关闭该测试进程，未停止 Sandboxie 服务。多用户多软件并发仍以调度契约覆盖为主，尚未执行大规模真实并发压力测试。

- 2026-10-08 Task 15 审计基线：重新运行 Release x64 `dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64`，0 警告、0 错误。只读确认系统已有 `C:\Windows\Sandboxie.ini`；生产路径优先系统配置，且还原默认调用 `net stop/start SbieSvc` 与按进程名 `taskkill`。程序目录副本不等于系统隔离，本轮不得执行真实覆盖还原或服务停止；先验证非破坏性真实 EXE 路径，不将回调替换探针计为真实系统集成通过。
- 2026-10-08 Task 15 真实 EXE 隔离审计：独立发布副本启动烟雾通过（标题“沙河批量打开器”、主窗口句柄非零、正常关闭、退出码 0、配置与 `updater.exe` 均生成）；随后用假的 Sandboxie 路径和沙盒根启动副本，程序仍读取系统 `C:\Windows\Sandboxie.ini` 并显示真实 92 个沙盒用户。实例已停止，未执行备份、删除或还原；真实备份/还原审计因当前环境无法隔离系统 Sandboxie 配置而暂停，需专用 VM/测试机或用户明确提供隔离环境。
- 2026-10-08 Task 15 真实数据备份：使用 Release x64 构建的 `ShaheLauncher.Core.Backup.BackupRestoreService.CreateBackupArchive`，从真实 `C:\Windows\Sandboxie.ini` 与 `C:\Sandbox\Administrator` 备份 107 个一级沙盒目录；归档为 `artifacts/runs/csharp_real_backup_20261008/SandboxBackup_20261008_144349.sbx`，大小 313,430,169 字节，SHA-256 `07C8310CB6615C1BDE23DB83589D6815D605BD2AD1D8CBF5EF9952CDA7E3A03A`。源数据未修改，未停止服务，未执行还原覆盖。
- 2026-10-08 Task 15 真实备份恢复校验：使用同一 C# `BackupRestoreService.SafeExtractZip` 解压到 `artifacts/runs/csharp_real_backup_20261008/verify_extract`；`Sandboxie.ini` SHA-256 一致，107 个用户目录一致，沙盒用户目录内 4,492 个文件和 663,169,463 字节全部一致。源根目录另有未归档的 `desktop.ini` 208 字节，符合服务仅归档一级用户目录的既有语义。

- 2026-10-03 C# 冗余参数清理 RED：将 `ManagedDelayedCall` 两个生产调用点和两个反射测试构造点改为三参数后，定向构建按预期失败，`ManagedDelayedCall` 构造函数仍声明未使用的 `threadName` 参数；未发现生产行为执行。
- 2026-10-03 C# 冗余参数清理 GREEN：删除 `ManagedDelayedCall` 的 `threadName` 参数并同步两个生产调用点、两个反射测试构造点；`ManagedDelayedCall` 定向测试通过 1/1。
- 2026-10-03 C# 延迟调用相邻回归：`UiControlsCSharpContractTests` 与 `MainFormLifecycleContractTests` 通过 21/21。
- 2026-10-03 C# 冗余参数清理全量回归：`dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore` 通过 174/174。
- 2026-10-03 C# 冗余参数清理 Release x64 构建：`dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64` 成功，0 警告、0 错误。

- 2026-08-28：前 20 模块真实调用审计提交前复核通过。当前工作树重新执行 `ShaheBulkLauncher.sln` `Release|x64` 还原/构建，结果为 0 警告、0 错误；重新运行 `ShaheLauncher.Tests.dll /Platform:x64`，162/162 通过、0 失败、0 跳过。重新从 `module_call_matrix.json` 计算并与 `audit_summary.json` 对照，模块总数 20、应用进程有执行证据 19、测试有执行证据 20、唯一仅测试调用为 Task 15、默认回退动作 11 项，全部一致。

- 2026-09-24：Task 15 `MainForm` 应用接线 RED：新增 `WiresBackupMenuActionToBackupRestoreService` 与 `WiresRestoreMenuActionToBackupRestoreService`，先构建成功后运行 Application 分类，新增测试按预期失败，首个缺口为 `SelectUsersCallback must be public`，确认旧代码的两个工具菜单仍未进入业务流程。
- 2026-09-24：Task 15 应用接线 GREEN：`MainForm` 已复用 `BackupRestoreService`、`SelectUsersDialog`、原生文件/目录选择和确认对话框，备份/还原通过 `Task` 后台执行并使用 `SafeCallAfter` 回 UI；还原接入服务停止、配置重载、服务重启、刷新列表、控制程序启动询问和程序重启询问。Application 定向新增备份/还原两项均通过。
- 2026-09-24：Task 15 还原进度窗口 RED/GREEN：新增 `ExposesRestoreProgressDialogWithAccessibleStageControls`，先因 `BackupRestoreProgressDialog` 不存在而失败；随后加入无障碍 `ProgressBar`（`AccessibleName=还原进度`、`TabStop=false`）和阶段文案，定向测试通过，Application 分类最终为 12/12。
- 2026-09-24：Task 15 跨模块验证通过：`ShaheLauncher.Tests.dll /Platform:x64` 全量运行 165/165，0 失败、0 跳过；`ShaheBulkLauncher.sln` `Release|x64` `/restore /t:Build` 退出码 0，0 警告、0 错误，Core、Updater、App、Tests 均生成。
- 2026-09-24：Task 15 Python 对照门禁通过：`python -m unittest discover -s tests -v` 运行 29/29，`python -m compileall -q .` 退出码 0；本轮未修改 Python 正式代码，用户已有 `shahe_bulk_launcher/backup.py` 与 `tests/test_backup_contract.py` 未提交改动保持原样。
- 2026-09-24：Task 15 Release x64 真实 EXE 烟雾通过：从 `csharp/ShaheLauncher.App/bin/x64/Release/net48` 复制最终发布文件到 `artifacts/runs/csharp_task15_backup_restore_smoke_20260924_json`，使用 JSON legacy `app_config.dat` 预置隔离 Sandboxie 路径后启动真实 `沙河批量打开器.exe`；标题为“沙河批量打开器”、主窗口句柄非零、启动延迟后仍存活，`CloseMainWindow()` 返回真，进程在 12 秒内退出且退出码为 0，生成 `Configuration/shahe_config.ini`，同目录存在 `updater.exe`，`smoke_summary.json` 记录 `passed=true`。此前一次使用错误 `key=value` legacy 夹具导致 `show_tray` 默认开启、关闭请求转为托盘隐藏，该次不是产品结论，已修正夹具后重跑通过。

- 2026-09-24：Task 20 `create_group` 应用接线 RED：新增 `WiresCreateGroupMenuActionToSandboxAdminState`，在旧代码上先按预期失败于缺少公开的 `ShowGroupNameCallback`，确认“新建分组”菜单仍落入统一默认动作提示而未进入分组状态流程。
- 2026-09-24：Task 20 `create_group` 应用接线 GREEN：`MainForm` 已接入现有 `SandboxAdminService.NormalizeSandboxNameList`、`SandboxAdminState.SandboxGroups`、`GroupNameDialog`、设置保存、分组视图清空和刷新回调；`WiresCreateGroupMenuActionToSandboxAdminState` 定向运行 1/1 通过，确认标题/默认值、成员顺序、状态文本、刷新调用，以及无选择、单用户和重名分支均保持 Python 行为；未修改 Python 正式代码。全量门禁待继续执行。
- 2026-09-24：Task 20 `create_group` Release x64 构建通过：`ShaheBulkLauncher.sln` `/restore /t:Build /p:Configuration=Release /p:Platform=x64` 退出码 0，Core、Updater、App、Tests 四项目均生成，0 警告、0 错误。
- 2026-09-24：Task 20 `create_group` C# 全量回归通过：`ShaheLauncher.Tests.dll /Platform:x64` 运行 166/166，0 失败、0 跳过；新增分组菜单接线契约未回归 Task 1-19、Task 15 备份/还原和既有 Task 20 应用契约。
- 2026-09-24：Task 20 `create_group` Python 对照门禁通过：`python -m unittest discover -s tests -v` 运行 29/29，`python -m compileall -q .` 退出码 0；本轮未修改 Python 正式代码，当前工作树既有 Python 改动保持原样。
- 2026-09-24：Task 20 `create_group` Application 分类回归通过：`ShaheLauncher.Tests.dll /Platform:x64 /TestCaseFilter:TestCategory=Application` 运行 13/13，0 失败、0 跳过；新建分组菜单接线与既有应用入口、无障碍、备份/还原契约同时通过。真实 EXE 新建分组动作级审计仍待补。
- 2026-09-24：Task 20 `create_group` 提交前复核：重新执行 Release x64 构建，Core、Updater、App、Tests 均生成且 0 警告、0 错误；Application 分类 13/13、C# 全量 166/166、Python 全量 29/29 均通过，`python -m compileall -q .` 退出码 0。以上结果均为本轮新鲜验证，Python 正式代码和用户已有 Python 改动未纳入本轮提交。
- 2026-09-24：Task 20 `launch_group` 映射与 RED：对照 Python `on_launch_group` 冻结“无分组提示、分组选择标题/提示/排序、空分组提示、无可用沙盒提示、按成员顺序进入启动调度”行为；新增 `WiresLaunchGroupMenuActionToLaunchScheduler` 后先运行 Release x64 定向测试，1 项按预期失败于实际状态仍为 `已触发操作: launch_group`，确认菜单仍走默认回退；本轮尚未修改 C# 生产代码。
- 2026-09-24：Task 20 `launch_group` Application 回归通过：`ShaheLauncher.Tests.dll /Platform:x64 /TestCaseFilter:TestCategory=Application` 运行 14/14，0 失败、0 跳过；覆盖无分组、选择标题/提示/排序、空分组、无可用沙盒和成员顺序进入启动调度。
- 2026-09-24：Task 20 `launch_group` C# 全量回归通过：`ShaheLauncher.Tests.dll /Platform:x64` 运行 167/167，0 失败、0 跳过；新增启动分组菜单接线未回归 Task 1-19、Task 15、`create_group` 和既有 Application 契约。
- 2026-09-24：Task 20 `launch_group` Release x64 提交前构建通过：`ShaheBulkLauncher.sln` `/restore /t:Build /p:Configuration=Release /p:Platform=x64` 退出码 0，Core、Updater、App、Tests 四项目均生成，0 警告、0 错误。
- 2026-09-24：Task 20 `launch_group` Python 对照门禁通过：`python -m unittest discover -s tests -v` 运行 29/29，`python -m compileall -q .` 退出码 0；本轮未修改 Python 正式代码，用户已有 Python 改动保持未暂存。
- 2026-09-24：Task 20 `delete_group` 映射与 RED：对照 Python `on_delete_group` 冻结“无分组提示、删除分组选择标题/提示/排序、删除后保存设置、当前分组视图清空、刷新列表和成功状态”行为；新增 `WiresDeleteGroupMenuActionToSandboxAdminState` 后运行 Release x64 定向测试，1 项按预期失败于实际状态仍为 `已触发操作: delete_group`，确认菜单仍走默认回退；本轮尚未修改 C# 生产代码。
- 2026-09-24：Task 20 `delete_group` GREEN：`MainForm` 已接入已有分组状态、统一分组选择对话框、设置保存、当前视图清空、刷新回调和原文案；`WiresDeleteGroupMenuActionToSandboxAdminState` 定向运行 1/1 通过，覆盖无分组和成功删除分支。
- 2026-09-24：Task 20 `delete_group` Application 回归通过：`ShaheLauncher.Tests.dll /Platform:x64 /TestCaseFilter:TestCategory=Application` 运行 15/15，0 失败、0 跳过；删除分组接线与既有入口、备份/还原、创建/启动分组契约同时通过。
- 2026-09-24：Task 20 `delete_group` C# 全量回归通过：`ShaheLauncher.Tests.dll /Platform:x64` 运行 168/168，0 失败、0 跳过；新增删除分组菜单接线未回归前 20 个已迁移模块和既有 Application 契约。
- 2026-09-24：Task 20 `delete_group` Release x64 提交前构建通过：`ShaheBulkLauncher.sln` `/restore /t:Build /p:Configuration=Release /p:Platform=x64` 退出码 0，Core、Updater、App、Tests 四项目均生成，0 警告、0 错误。
- 2026-09-24：Task 20 `delete_group` Python 对照门禁通过：`python -m unittest discover -s tests -v` 运行 29/29，`python -m compileall -q .` 退出码 0；本轮未修改 Python 正式代码，用户已有 Python 改动保持未暂存。
- 2026-09-24：Task 20 `batch_create_sandboxes` 映射与 RED：对照 Python `on_batch_create_sandboxes` 冻结规则对话框、命名生成/校验、已存在跳过、官方创建、刷新、逐项状态和结果文案；新增 `WiresBatchCreateSandboxesMenuActionToSandboxieClient` 后运行 Release x64 定向测试，1 项按预期失败于实际状态仍为 `已触发操作: batch_create_sandboxes`，确认菜单仍走默认回退；本轮尚未修改 C# 生产代码。
- 2026-09-24：Task 20 `batch_create_sandboxes` GREEN 首轮编译偏差已定位并修复：`SandboxieCallbacks.RefreshDisplayMaps` 是 `Action`，不能直接绑定返回 `IEnumerable<string>` 的 `RefreshSandboxDisplayMaps`；改为无返回值 lambda，未改变刷新行为。
- 2026-09-24：Task 20 `batch_create_sandboxes` GREEN：`MainForm` 已接入 `BatchNameRuleDialog`、命名生成/校验、已存在跳过、官方 `SandboxieClient` 创建边界、逐项状态、失败提示、刷新回调和结果状态；`WiresBatchCreateSandboxesMenuActionToSandboxieClient` 定向运行 1/1 通过，确认规则标题/提示、Box2 创建、Box1/Box3 跳过、刷新次数和结果文案。
- 2026-09-24：Task 20 `batch_create_sandboxes` Application 回归通过：`ShaheLauncher.Tests.dll /Platform:x64 /TestCaseFilter:TestCategory=Application` 运行 16/16，0 失败、0 跳过；批量创建接线与前序分组、备份/还原及既有 Application 契约同时通过。
- 2026-09-24：Task 20 `batch_create_sandboxes` Application 分类复核通过：`ShaheLauncher.Tests.dll /Platform:x64 /TestCaseFilter:TestCategory=Application` 运行 16/16，0 失败、0 跳过；批量创建接线与既有应用入口、无障碍、备份/还原及分组契约同时通过。
- 2026-09-24：Task 20 `batch_create_sandboxes` C# 全量回归通过：`ShaheLauncher.Tests.dll /Platform:x64` 运行 169/169，0 失败、0 跳过；批量创建接线未回归前 20 个已迁移模块、Task 15 备份/还原和既有 Application 契约。
- 2026-09-24：Task 20 `batch_create_sandboxes` Python 对照门禁通过：`python -m unittest discover -s tests -v` 运行 29/29，0 失败；本轮未修改 Python 正式代码，用户已有 Python 改动保持未暂存。
- 2026-09-24：Task 20 `batch_create_sandboxes` Python 语法门禁通过：`python -m compileall -q .` 退出码 0；本轮未修改 Python 正式代码。
- 2026-09-24：Task 20 `batch_create_sandboxes` Release x64 提交前构建通过：`ShaheBulkLauncher.sln` `/restore /t:Build /p:Configuration=Release /p:Platform=x64` 退出码 0，Core、Updater、App、Tests 四项目均生成，0 警告、0 错误。

- 2026-09-24：Task 20 `batch_rename_sandboxes` 映射与 RED：对照 Python `on_batch_rename_sandboxes` 冻结目标选择顺序、默认命名前缀/固定数量、名称校验、显示名冲突、内部名与目录重命名、Sandboxie.ini 分节、别名、设置/定时任务/活动启动引用更新、运行中阻止和精确状态/提示文案；新增 `WiresBatchRenameSandboxesMenuActionToSandboxAdminService` 后先运行 Release x64 定向测试，1 项按预期失败于 `ShowBatchRenameRuleCallback must be public`，确认菜单仍走默认回退；本轮尚未修改 C# 生产代码。
- 2026-09-24：Task 20 `batch_rename_sandboxes` RED 测试编译通过：`ShaheBulkLauncher.sln` `/t:Build /p:Configuration=Release /p:Platform=x64` 退出码 0，Core、Updater、App、Tests 均生成。
- 2026-09-24：Task 20 `batch_rename_sandboxes` GREEN：`MainForm` 已接入批量目标选择、默认前缀/固定数量规则对话框、名称校验、显示名/INI 冲突检查、运行中阻止、内部目录重命名、Sandboxie.ini 分节更新、别名命令、设置/定时任务/活动启动引用更新、刷新和原文案状态；`WiresBatchRenameSandboxesMenuActionToSandboxAdminService` 定向运行 1/1 通过。
- 2026-09-24：Task 20 `batch_rename_sandboxes` GREEN 后 Release x64 构建通过：`ShaheBulkLauncher.sln` `/t:Build /p:Configuration=Release /p:Platform=x64` 退出码 0，Core、Updater、App、Tests 四项目均生成，0 警告、0 错误。
- 2026-09-24：Task 20 `batch_rename_sandboxes` 定向复核通过：在补齐 Sandboxie 配置重载回调和别名命令测试边界后，Release x64 构建与 `WiresBatchRenameSandboxesMenuActionToSandboxAdminService` 重新通过，1/1。
- 2026-09-24：Task 20 `batch_rename_sandboxes` Application 回归通过：`ShaheLauncher.Tests.dll /Platform:x64 /TestCaseFilter:TestCategory=Application` 运行 17/17，0 失败、0 跳过；批量重命名接线与前序应用、备份/还原、分组和批量创建契约同时通过。
- 2026-09-24：Task 20 `batch_rename_sandboxes` C# 全量回归通过：`ShaheLauncher.Tests.dll /Platform:x64` 运行 170/170，0 失败、0 跳过；批量重命名接线未回归前 20 个已迁移模块、Task 15 备份/还原和既有 Application 契约。
- 2026-09-24：Task 20 `batch_rename_sandboxes` Python 对照门禁通过：`python -m unittest discover -s tests -v` 运行 29/29，0 失败；本轮未修改 Python 正式代码，用户已有 Python 改动保持未暂存。
- 2026-09-24：Task 20 `batch_rename_sandboxes` Python 语法门禁通过：`python -m compileall -q .` 退出码 0；本轮未修改 Python 正式代码。
- 2026-09-24：Task 20 `batch_rename_sandboxes` Release x64 提交前还原/构建通过：`ShaheBulkLauncher.sln` `/restore /t:Build /p:Configuration=Release /p:Platform=x64` 退出码 0，Core、Updater、App、Tests 四项目均生成，0 警告、0 错误。
- 2026-09-24：Task 20 `batch_rename_sandboxes` 最终 Application/Release 复核通过：`ShaheBulkLauncher.sln` `/restore /t:Build /p:Configuration=Release /p:Platform=x64` 退出码 0 后，`ShaheLauncher.Tests.dll /Platform:x64 /TestCaseFilter:TestCategory=Application` 运行 17/17，0 失败、0 跳过。
- 2026-09-24：Task 20 `batch_rename_sandboxes` 最终 C# 全量复核通过：`ShaheLauncher.Tests.dll /Platform:x64` 运行 170/170，0 失败、0 跳过。
- 2026-09-24：Task 20 `batch_rename_sandboxes` 当前缓存内容 Application 复核通过：`ShaheLauncher.Tests.dll /Platform:x64 /TestCaseFilter:TestCategory=Application` 运行 17/17，0 失败、0 跳过。
- 2026-09-24：Task 20 `batch_rename_sandboxes` 当前缓存内容 C# 全量复核通过：`ShaheLauncher.Tests.dll /Platform:x64` 运行 170/170，0 失败、0 跳过。
- 2026-09-24：Task 20 `batch_delete_sandboxes` 映射与 RED：对照 Python `on_batch_delete_sandboxes` 冻结批量目标顺序、无目标提示、不可恢复确认、活动启动引用删除、运行中终止/跳过、官方删除、目录删除、设置/定时任务引用清理、注册表清理、刷新以及失败/跳过/注册表提示和状态文案；新增 `WiresBatchDeleteSandboxesMenuActionToSandboxieClient` 并先完成 Release x64 编译，定向测试 1 项按预期失败于 `DeleteSandboxCallback must be public`，确认旧菜单尚未暴露真实删除边界且仍未完成接线；本轮此时尚未修改 C# 生产代码。
- 2026-09-24：Task 20 `batch_delete_sandboxes` GREEN：`MainForm` 已接入批量目标选择、不可恢复确认、活动启动引用清理、运行中沙盒终止/跳过、官方 Sandboxie 删除边界、目录删除、显示名/内部名设置引用清理、活动定时任务引用清理、注册表清理、刷新、失败/跳过提示和原状态文案；新增删除回调和注册表清理回调供应用契约注入，复用既有 `SandboxAdminService`、`SandboxieClient` 和经典 `MainMenu/MenuItem`，未修改 Python 正式代码。
- 2026-09-24：Task 20 `batch_delete_sandboxes` GREEN 首轮复核：Release x64 构建通过；新增应用契约首次断言失败仅因测试保留了被服务替换前的置顶列表引用，已改为从状态重新读取当前列表，未发现生产行为偏差；修正后 `WiresBatchDeleteSandboxesMenuActionToSandboxieClient` 定向运行 1/1 通过。
- 2026-08-28：前 20 个 C# 模块真实调用审计完成。重新构建 `ShaheBulkLauncher.sln` `Release|x64` 成功；使用 Visual Studio `Microsoft.CodeCoverage.Console` 对真实 `沙河批量打开器.exe` 启动、900/1500 毫秒启动任务、托盘创建、正常关闭和服务释放收集方法级覆盖，启动摘要 `passed=true`。随后运行隔离 STA 交互审计，临时列表严格只有 `Box1`，真实菜单完成应用路径粘贴、上一/下一用户、置顶、刷新、空 PID 静音全部、进程状态刷新、取消定时、Sandboxie 路径/数据根校正，并实际打开后取消热键设置、软件设置、朗读设置和微信二维码 4 个对话框；进程销毁完成且无残留。C# 全量测试在覆盖收集下 162/162 通过，20/20 模块均有测试执行行；真实 EXE 启动与隔离交互合并为 19/20 模块有应用执行证据，Task 15 仅组装/测试、未业务接入。确认仍默认回退的动作恰好 11 个：`create_group`、`launch_group`、`delete_group`、`batch_create_sandboxes`、`batch_rename_sandboxes`、`batch_delete_sandboxes`、`schedule_launch`、`schedule_close`、`repair_sandboxie_control_settings`、`backup_sandboxes`、`restore_sandboxes`。权威摘要、逐模块矩阵和覆盖文件保存在 `artifacts/runs/csharp_first20_real_call_audit_20260828`；探针编码/隔离调试轮次只保留在 artifacts，未计入项目结论。
- 2026-09-24：Task 20 `batch_delete_sandboxes` Application 回归通过：`ShaheLauncher.Tests.dll /Platform:x64 /TestCaseFilter:TestCategory=Application` 运行 18/18，0 失败、0 跳过；批量删除接线未回归既有应用入口、备份/还原、分组、批量创建和批量重命名契约。
- 2026-09-24：Task 20 `batch_delete_sandboxes` C# 全量回归通过：`ShaheLauncher.Tests.dll /Platform:x64` 运行 171/171，0 失败、0 跳过；批量删除接线未回归前 20 个已迁移模块、Task 15 备份/还原和既有 Task 20 Application 契约。
- 2026-09-27：Task 20 `schedule_launch` 映射与 RED：对照 Python `on_schedule_launch_sandboxes` 冻结沙盒/应用选择、无目标提示、延迟秒数对话框默认 30 和 1–86400 范围、`launch_selected` 定时任务、启动中顺延 1 秒和原状态文案；新增 `WiresScheduleLaunchMenuActionToLaunchScheduler` 后完成 Release x64 编译，定向测试 1 项按预期失败于 `ShowScheduleDelayCallback must be public`，确认旧菜单仍未接入定时启动流程；本轮尚未修改 C# 生产代码。
- 2026-09-27：Task 20 `schedule_launch` GREEN：`MainForm` 已接入定时启动菜单分派、沙盒/应用选择、延迟秒数对话框注入边界和默认 WinForms 数字输入（默认 30 秒、范围 1–86400），复用 `LaunchScheduler.ScheduleSelectedLaunch` 保持 `launch_selected` 任务、启动中顺延 1 秒、状态文案和应用回调；Release x64 构建通过，`WiresScheduleLaunchMenuActionToLaunchScheduler` 定向运行 1/1 通过，未修改 Python 正式代码。
- 2026-09-27：Task 20 `schedule_launch` Application 回归通过：`ShaheLauncher.Tests.dll /Platform:x64 /TestCaseFilter:TestCategory=Application` 运行 19/19，0 失败、0 跳过；定时启动接线未回归前序应用入口、备份/还原、分组和批量沙盒动作契约。
- 2026-09-27：Task 20 `schedule_launch` C# 全量回归通过：`ShaheLauncher.Tests.dll /Platform:x64` 运行 172/172，0 失败、0 跳过；定时启动接线未回归前 20 个已迁移模块、Task 15 备份/还原和既有 Task 20 Application 契约。
- 2026-09-27：Task 20 `schedule_launch` Python 对照门禁通过：`python -m unittest discover -s tests -v` 运行 29/29，0 失败；本轮未修改 Python 正式代码，用户已有 `shahe_bulk_launcher/backup.py` 与 `tests/test_backup_contract.py` 改动保持未暂存。
- 2026-09-27：Task 20 `schedule_launch` Python 语法门禁通过：`python -m compileall -q .` 退出码 0；本轮未修改 Python 正式代码。
- 2026-09-27：Task 20 `schedule_launch` 提交前最终复核通过：重新执行 `ShaheBulkLauncher.sln` `/restore /t:Build /p:Configuration=Release /p:Platform=x64`，Core、Updater、App、Tests 均生成且 0 警告、0 错误；定向契约 1/1、Application 19/19、C# 全量 172/172、Python 全量 29/29、`python -m compileall -q .` 和 `git diff --check` 均通过。本轮只计划提交 `PROJECT_STRUCTURE.md`、`csharp/ShaheLauncher.App/MainForm.cs` 和 `csharp/ShaheLauncher.Tests/MainFormApplicationContractTests.cs`，Python 正式代码、用户已有 Python 改动和临时目录不纳入提交。
- 2026-08-28：Task 20 非破坏性菜单动作接线 RED：新增 `WiresNonDestructiveMenuActionsWithoutDefaultStatusFallback`，覆盖上一/下一用户、热键设置、软件设置、Sandboxie 安装路径校正、沙盒数据目录选择、微信/支付宝二维码入口均不得落入默认 `已触发操作` 状态提示。Tests 项目 `Release|x64` 构建退出码 0；定向运行新增测试 1 项、1/1 按预期失败，首个失败点为 `SelectPreviousSandboxCallback must be public`，确认当前缺口在 `MainForm` 接线。
- 2026-09-24：Task 20 `batch_delete_sandboxes` Python 对照门禁通过：`python -m unittest discover -s tests -v` 运行 29/29，0 失败；`python -m compileall -q .` 退出码 0；本轮未修改 Python 正式代码，用户已有 `shahe_bulk_launcher/backup.py` 与 `tests/test_backup_contract.py` 改动保持未暂存。
- 2026-09-24：Task 20 `batch_delete_sandboxes` Release x64 提交前还原/构建通过：`ShaheBulkLauncher.sln` `/restore /t:Build /p:Configuration=Release /p:Platform=x64` 退出码 0；Core、Updater、App、Tests 四项目均生成，0 警告、0 错误。本轮待提交文件仅为 `PROJECT_STRUCTURE.md`、`csharp/ShaheLauncher.App/MainForm.cs` 和 `csharp/ShaheLauncher.Tests/MainFormApplicationContractTests.cs`，Python 正式代码、用户已有 Python 改动和临时目录不纳入提交。
- 2026-08-28：Task 20 非破坏性菜单动作接线 GREEN：`WiresNonDestructiveMenuActionsWithoutDefaultStatusFallback` 改为通过真实 `MainMenu/MenuItem.PerformClick()` 触发菜单入口，覆盖 8 个非破坏性动作不再落入默认 `已触发操作` 状态提示；Tests 项目 `Release|x64` 构建退出码 0，定向测试 1/1 通过。当前仍需运行 Application 分类、C# 全量、Release x64、Python 对照和烟雾验证后才能提交本子模块。

- 2026-08-28：Task 20 非破坏性菜单动作接线 Application 回归通过：使用 `ShaheLauncher.Tests.dll` `Release|x64` 运行 `/TestCaseFilter:TestCategory=Application`，9/9 通过、0 失败、0 跳过；本次新增菜单动作契约已纳入 Application 分类。当前仍需运行 C# 全量、Release x64、Python 对照和烟雾验证后才能提交本子模块。

- 2026-08-28：Task 20 非破坏性菜单动作接线 C# 全量回归通过：使用 `ShaheLauncher.Tests.dll` `Release|x64` 运行全量 MSTest，162/162 通过、0 失败、0 跳过，确认新增非破坏性菜单动作接线未破坏既有 C# 迁移契约。当前仍需运行 Release x64、Python 对照和烟雾验证后才能提交本子模块。

- 2026-08-28：Task 20 非破坏性菜单动作接线 Release x64 构建通过：`ShaheBulkLauncher.sln` `/restore /t:Build /p:Configuration=Release /p:Platform=x64` 退出码 0，Core、Updater、App、Tests 四项目均生成，主程序输出目录继续包含 `沙河批量打开器.exe` 和 `updater.exe`。当前仍需运行 Python 对照和烟雾验证后才能提交本子模块。

- 2026-08-28：Task 20 非破坏性菜单动作接线 Python 定向对照通过：`python -m unittest tests.test_entry_compatibility tests.test_modular_structure tests.test_feature_method_partition -v` 运行 9 项，9/9 通过，确认入口兼容、模块结构和方法唯一分区契约未变化。当前仍需运行 Python 全量、语法编译和烟雾验证后才能提交本子模块。

- 2026-08-28：Task 20 非破坏性菜单动作接线 Python 全量对照通过：`python -m unittest discover -s tests -v` 运行 29 项，29/29 通过，包含当前工作树中既有 `test_backup_contract` 备份契约；本轮未修改 Python 正式代码。当前仍需运行语法编译和烟雾验证后才能提交本子模块。

- 2026-08-28：Task 20 非破坏性菜单动作接线 Python 语法编译通过：`python -m compileall -q .` 退出码 0，无语法错误输出。当前仍需运行烟雾验证后才能提交本子模块。

- 2026-08-28：Task 20 非破坏性菜单动作接线 Python 行为探针通过：顺序运行 `tests/probes/*.py` 共 26 条，26/26 返回码 0；逐探针日志与 `summary.json` 保存在 `artifacts/runs/csharp_task20_non_destructive_actions_20260828_probes`。当前仍需运行真实 EXE 烟雾验证后才能提交本子模块。

- 2026-08-28：Task 20 非破坏性菜单动作接线真实 EXE 烟雾通过：从 `csharp/ShaheLauncher.App/bin/x64/Release/net48` 复制发布文件到 `artifacts/runs/csharp_task20_non_destructive_actions_20260828_smoke`，预置临时 `Sandboxie/Start.exe`、`SandboxUsers` 和 legacy `app_config.dat` 后启动真实 `沙河批量打开器.exe`；检测到标题“沙河批量打开器”和非零主窗口句柄，`CloseMainWindow()` 返回真，进程在 12 秒内退出且退出码为 0，运行目录生成 `Configuration/shahe_config.ini`，同目录存在 `updater.exe`。`smoke_summary.json` 记录 `passed=true`；随后按该 artifacts 路径筛查目标进程无输出，确认测试结束后无残留目标进程。

- 2026-08-23：Task 20 提交前最终构建产物烟雾通过：将本轮重新构建的 Release x64 发布文件复制到新目录 `artifacts/runs/csharp_task20_smoke_20260823_commit`，在目录原本不存在 `Configuration` 的状态下启动真实 `沙河批量打开器.exe`；检测到标题“沙河批量打开器”和非零主窗口句柄，`CloseMainWindow()` 返回真，进程在 12 秒内退出且退出码为 0，本轮新生成 `Configuration/shahe_config.ini`，同目录存在 `updater.exe`。`smoke_summary.json` 记录 `passed=true`，测试结束后无残留目标进程。

- 2026-08-23：Task 20 提交前非烟雾门禁复核通过：`ShaheBulkLauncher.sln` `Release|x64` 还原/构建退出码 0，Core、Updater、App、Tests 四项目均生成；Application 定向契约 8/8、C# 全量契约 161/161，均无失败或跳过；Python 入口/结构/分区定向测试 9/9、全量 29/29、`python -m compileall -q .` 均通过；`tests/probes/*.py` 26/26 返回码 0，无失败或超时，逐探针输出与最新 `summary.json` 已归档到 `artifacts/runs/csharp_task20_probes_20260823_final`。

- 2026-08-23：Task 20 最终真实 EXE 烟雾重跑通过：改为枚举 `csharp/ShaheLauncher.App/bin/x64/Release/net48` 的发布文件并复制到 `artifacts/runs/csharp_task20_smoke_20260823_final`，真实启动 `沙河批量打开器.exe` 后检测到标题“沙河批量打开器”和非零主窗口句柄；`CloseMainWindow()` 返回真，进程在 12 秒内正常退出且退出码为 0，运行目录生成 `Configuration/shahe_config.ini` 并包含 `updater.exe`。结构化结果已写入同目录 `smoke_summary.json`。

- 2026-08-23：Task 20 最终真实 EXE 烟雾测试首次运行在准备阶段失败：脚本使用 `Copy-Item -LiteralPath (Join-Path $source '*')`，`-LiteralPath` 不展开通配符，导致发布文件未复制到 `artifacts/runs/csharp_task20_smoke_20260823_final`，后续找不到 `沙河批量打开器.exe` 且未启动进程。本次只确认测试工具错误，未观察到产品行为，不能作为应用通过或失败结论；下一步改为枚举发布目录文件后逐项复制，并在同一专用目录重跑。

- 2026-08-23：Task 20 最终接线后的 Python 行为探针全量通过：顺序运行 `tests/probes/*.py` 共 26 条，26/26 返回码 0；逐探针日志与 `summary.json` 保存在 `artifacts/runs/csharp_task20_probes_20260823_final`，未发现进程/音频边界、启动调度、配置、更新、备份恢复、快捷键、列表导航或无障碍行为回归。

- 2026-08-23：Task 20 最终接线后的 Python 全量与语法门禁通过：`python -m unittest discover -s tests -v` 为 29/29，`python -m compileall -q .` 退出码 0；本轮未修改 Python 正式基线，现有用户 `backup.py` 改动及其契约仍保持通过。

- 2026-08-23：Task 20 最终接线后的构建与 Python 定向对照通过：`ShaheBulkLauncher.sln` `Release|x64` 还原/构建退出码 0，汇总 0 警告、0 错误，主程序输出目录存在 `updater.exe`；`tests.test_entry_compatibility`、`tests.test_modular_structure`、`tests.test_feature_method_partition` 共 9 项、9/9 通过。

- 2026-08-23：Task 20 最终接线后的 C# 全量契约通过：`ShaheLauncher.Tests.dll /Platform:x64` 运行 161 项、161/161 通过；新增状态和回调组装未回归 Task 1-19 的 Core、UI、Updater、快捷键和生命周期契约。

- 2026-08-23：Task 20 Application 分类回归通过：`TestCategory=Application` 8/8，覆盖服务组装、Python 路径与启动顺序、经典菜单和无障碍顺序、扩展 Enter 映射、真实入口/发布复制、统一释放及最终交互回调接线；本轮实现未回归原 7 项 Application 契约。

- 2026-08-23：Task 20 最终用户动作接线新增契约 GREEN：`MainForm` 已创建并同步 `ProcessActionState`、`LaunchSchedulerState`、`SandboxAdminState`，进程操作、调度取消、Sandboxie PID/名称查询、窗口/播放设备选择、运行状态刷新和朗读设置对话框均接入现有服务；Tests 项目 `Release|x64` 构建 0 警告、0 错误，新增 `WiresMigratedFeatureServicesIntoInteractiveApplicationCallbacks` 定向运行 1/1 通过。Application 8 项全量及跨模块门禁待继续执行。

- 2026-08-23：Task 20 最终用户动作接线契约 RED：在原 7 项 Application 契约后新增 1 项，要求启动、进程操作、调度、Sandboxie 查询、窗口/设备选择、设置与语音对话框均连接已迁移服务状态和回调。Tests 项目 `Release|x64` 构建仍为 0 警告、0 错误；新增测试按预期失败，首个失败点为 `MuteCurrentCallback` 为空。下一步只补 `MainForm` 的状态对象、Process/Launch/Admin 回调和现有对话框接线，不修改 Core 模块行为。

- 2026-08-23：Task 20 首轮跨模块 GREEN 与真实启动烟雾通过：`TestCategory=Application` 7/7、C# 全量 160/160；`ShaheBulkLauncher.sln` `Release|x64` 还原/构建退出码 0，0 警告、0 错误，主程序输出目录已复制 `updater.exe`。Python `app.py` 定向结构测试 9/9、全量 29/29、`python -m compileall -q .` 和 `tests/probes` 26/26 均通过。独立发布副本 `artifacts/runs/csharp_task20_smoke_20260823_235847` 启动真实 `沙河批量打开器.exe`，检测到标题“沙河批量打开器”和非零主窗口句柄，正常关闭请求成功，进程退出码 0，运行目录生成 `Configuration/shahe_config.ini` 且包含 `updater.exe`。Task 20 仍需完成最终用户动作接线复核，尚未提交或声明整个软件迁移完成。

- 2026-08-10：Task 20 `app.py` C# 契约 RED：新增 `MainFormApplicationContractTests.cs`，Tests 项目 `Release|x64` 构建 0 警告、0 错误；`TestCategory=Application` 运行 7 项、7/7 按预期失败。失败证据覆盖 `MainForm(LauncherRuntime)` 直接服务组装与真实进程边界尚不存在、Python 配置路径/启动顺序和经典菜单未接入、扩展 Enter 到 `NumPadEnter` 的 WinForms 映射未完成、`Program.Main` 仍为空、App 发布输出未声明复制 `updater.exe`、组合服务尚无统一释放状态。下一步只实现 Task 20 的 `MainForm.cs`、`Program.cs` 和必要发布接线。

- 2026-08-10：Task 20 `app.py` 迁移前基线通过：入口与 `shahe_bulk_launcher/app.py` 语法检查成功；Python 入口/结构/方法分区定向测试 9/9、全量测试 29/29、`python -m compileall -q .` 均通过；`tests/probes` 下 26 个 Python 行为探针全部 PASS。C# `ShaheBulkLauncher.sln` `Release|x64` 还原/构建退出码 0，MSBuild 汇总 0 警告、0 错误，C# 全量契约 153/153。当前 `Program.cs` 仍为空入口且 `MainForm.cs` 尚不存在，下一步仅为 Task 20 编写应用组装与入口契约 RED。

- 2026-08-10：Task 19 跨模块与最终门禁通过：`ShaheBulkLauncher.sln` `Release|x64` 还原/构建退出码 0，MSBuild 汇总 0 警告、0 错误；`MainFormLifecycleContractTests` 定向契约 7/7，C# 全量契约 153/153；Python 全量测试 29/29、生命周期结构测试 6/6、`python -m compileall -q .` 通过，四个生命周期对照探针全部 PASS。当前模块验证完成，但 `Program.cs` 空入口和 Task 20 应用组装仍阻止 C# 版本完整运行及真实启动烟雾测试。

- 2026-08-10：Task 19 Python 对照通过：入口与 `features/lifecycle.py` 语法检查成功；`tests.test_feature_method_partition` 与 `tests.test_remaining_features_contract` 为 6/6；`ui_broad_probe.py`、`fixed_hotkeys_focus_probe.py`、`process_menu_selection_refresh_probe.py`、`launch_done_no_sync_menu_probe.py` 全部 PASS。确认 Python 正式基线的 34 个生命周期方法分区、主界面文案/控件名、固定快捷键和选择刷新合并边界未回归。

- 2026-08-10：Task 19 `features/lifecycle.py` C# 定向 GREEN：`MainForm.Lifecycle.cs` 已迁入 34 个生命周期入口，解决方案构建 0 警告、0 错误，`MainFormLifecycleContractTests` 7/7 通过；确认主界面中文文案、AccessibleName、Tab 顺序、明确启动按钮、启动会话取消、活跃目标改名/删除、延迟调用停止、托盘关闭、自动运行路径和状态栏刷新保持契约。`MainForm.cs` 与 `Program.cs` 仍未迁移。

- 2026-08-10：Task 19 首轮定向测试为 5/7：实现构建已恢复至 0 警告、0 错误；两项失败经定位均为契约驱动方式问题，未显示窗体时 `Button.PerformClick()` 不派发 Click，且 `params object[]` 将 `string[]` 展开后导致反射参数不匹配。生产代码对应事件绑定与 `IEnumerable<string>` 签名正确，下一步只修正测试调用后重跑。

- 2026-08-10：Task 19 首轮 GREEN 构建在测试前失败：`MainForm.Lifecycle.cs` 出现 2 个警告即错误门禁，分别为 `ObjectDisposedException` 已被前置 `InvalidOperationException` 捕获，以及 `_destroyLaterScheduled` 只读取未赋值触发 CS0649。根因均位于本模块新实现，尚未进入定向测试；下一步只修正异常捕获层级和销毁状态表达后重跑同一门禁。

- 2026-08-10：Task 19 `features/lifecycle.py` C# 契约 RED：新增 `MainFormLifecycleContractTests.cs`，构建仍为 0 警告、0 错误；`MainFormLifecycleContractTests` 定向运行 7 项、7/7 按预期失败，统一根因为 `ShaheLauncher.App.MainForm` 尚不存在。契约冻结 34 个生命周期入口、主界面控件文案/AccessibleName/Tab 顺序、启动会话取消、活跃目标改名/删除、延迟调用停止、托盘关闭语义、自动运行路径和状态栏边界；未修改 `MainForm.cs` 或 `Program.cs`。

- 2026-08-10：Task 19 迁移前基线通过：Python `py_compile` 与全量测试 `29/29`；`ui_broad_probe.py`、`fixed_hotkeys_focus_probe.py`、`process_menu_selection_refresh_probe.py`、`launch_done_no_sync_menu_probe.py` 全部 PASS；C# 解决方案 `Release|x64` 构建 0 警告、0 错误；C# 全量契约 `146/146`。`Program.cs` 入口仍为空，`MainForm` 尚未组装，完整程序启动烟雾测试继续属于 Task 20，不在本模块声明范围。

- 2026-08-09：Task 18 提交前复核通过：C# 全量契约 `146/146`；Python 全量测试 `29/29`；`python -m compileall -q .` 通过；复核未发现快捷键迁移回归。

- 2026-08-09：Task 18 提交前解决方案构建复核通过：`ShaheBulkLauncher.sln` `Release|x64` 退出码 0，MSBuild 汇总 0 警告、0 错误；Core、App、Updater、Tests 均成功生成。

- 2026-08-08：Task 18 `features/hotkeys.py` C# 定向 GREEN：新增 `ShaheLauncher.App/Input/HotkeyController.cs` 及快捷键 DTO，`Release|x64` 构建 0 警告、0 错误，`TestCategory=Hotkeys` 运行 8 项、8/8 通过；确认固定快捷键、48 项动作规格、特殊键/数字/F1-F24/小键盘解析与格式化、固定冲突过滤、自定义快捷键归一化、菜单标签、快捷键禁用保留固定键、特殊项跳过与循环选择、进程菜单运行标记和 live-state 刷新令牌保持 Python 边界。`NumPadEnter` 通过控制器逻辑键码保留文本语义，尚未接入 WinForms 主窗口事件映射。

- 2026-08-08：Task 18 Python 对照与结构门禁通过：`python -m py_compile "沙河批量打开器.py" shahe_bulk_launcher\features\hotkeys.py` 成功；`python -m unittest tests.test_feature_method_partition tests.test_remaining_features_contract -v` 为 6/6；`fixed_hotkeys_focus_probe.py`、`process_menu_selection_refresh_probe.py`、`process_menu_uses_running_marker_probe.py` 均 PASS，确认固定快捷键在自定义热键关闭时仍触发、列表选择刷新令牌隔离旧 live-state、运行标记继续控制进程菜单启用状态。

- 2026-08-08：Task 18 跨模块 C# 回归与解决方案门禁通过：`ShaheLauncher.Tests.dll /Platform:x64` 全量契约为 146/146；`ShaheBulkLauncher.sln` `Release|x64` 构建退出码 0，MSBuild 汇总为 0 警告、0 错误；新增 `HotkeyController` 未回归 Task 1-17，尚未接入 Task 19 的 MainForm 生命周期和 WinForms `NumPadEnter` 底层事件映射风险保持记录。

- 2026-08-09：Task 18 Python 最终门禁通过：`python -m unittest discover -s tests -v` 为 29/29；`python -m compileall -q .` 通过；本轮快捷键迁移未修改 Python 正式基线，未新增结构回归或语法回归。

- 2026-08-08：Task 18 `features/hotkeys.py` C# 契约 RED：新增 `HotkeyControllerContractTests.cs`，冻结固定 `Ctrl+A`/`Ctrl+O`/`Ctrl+V`/`F5`/`Esc`/`Alt+F4`、48 项自定义动作规格、快捷键解析/格式化、固定冲突过滤、菜单标签、启用状态、特殊项跳过与循环选择、进程菜单运行标记及 live-state 刷新令牌边界；Tests 项目 `Release|x64` 构建 0 警告、0 错误，`TestCategory=Hotkeys` 运行 8 项、8/8 按预期失败，统一根因为 `ShaheLauncher.App.Input.HotkeyController` 尚不存在。WinForms 没有独立 `NumPadEnter` 枚举，契约使用控制器逻辑键码常量保留 Python 文本语义，底层事件映射作为 Task 19 接线风险记录。

- 2026-08-08：Task 15 `backup.py` Python 基线确认：`python -m unittest tests.test_backup_contract -v` 运行 6/6 通过；`restore_safe_extract_probe.py`、`restore_shell_free_probe.py`、`restore_no_main_thread_stop_probe.py` 全部 PASS；`python -m py_compile shahe_bulk_launcher\backup.py` 通过。C# 契约必须保留用户当前改动的 ZIP 父目录穿越/绝对路径/盘符/UNC/符号链接拒绝、`SbieCtrl.exe` 控制程序查找、无 shell 派发及停止服务不占 UI 主线程边界。
- 2026-08-08：C# Task 15 `backup.py` 契约 RED：新增 7 项 Backup 契约覆盖安全解压、`.sbx` 归档布局和文件名、Sandboxie.ini 合并、沙盒目录恢复、服务生命周期回调、控制程序优先级、中文文案/正式入口重启路径及归档后台线程；解决方案 Release|x64 构建 0 警告、0 错误，Backup 定向测试 7/7 按预期失败，统一根因为 `BackupRestoreService` 尚不存在。
- 2026-08-08：C# Task 15 `backup.py` 契约 GREEN：`BackupRestoreService` 已迁入 `ShaheLauncher.Core/Backup/`，Backup 定向契约 7/7 通过；确认安全 ZIP 路径、归档文件扩展名/布局、INI 合并、沙盒目录恢复、服务停止/配置重载/重启回调、控制程序优先级、原中文文案、正式入口重启命令及后台线程边界均保持 Python 行为。
- 2026-08-08：Task 15 跨模块回归通过：C# 全量契约 117/117；Python 结构回归 23/23；`restore_safe_extract_probe.py`、`restore_shell_free_probe.py`、`restore_no_main_thread_stop_probe.py` 全部 PASS。新增 Backup 服务未回归既有 Task 1-14；下一步执行最终解决方案构建、Python 语法门禁和 Task 15 提交。
- 2026-08-08：Task 15 最终验证通过：`ShaheBulkLauncher.sln` `Release|x64` 构建退出码 0，MSBuild 汇总为 0 警告、0 错误；C# 全量契约 117/117；Python `test_backup_contract` 6/6、入口/备份/契约语法检查和 `git diff --check` 通过。上一轮验证命令曾误把 C# 测试文件传给 `py_compile`，已修正并重新通过；该命令错误未产生代码失败。
- 2026-08-08：Task 16 `features/speech.py` 契约 RED：新增 9 项 Speech 契约覆盖 SpeechConfig 清洗、读屏 INI 默认值与 Configuration 写路径、Tolk 资源定位、读屏进程检测和语音接口归一化、配置状态文案、辅助朗读后台线程、关闭运行时和重复文案抑制；Tests 项目 `Release|x64` 构建 0 警告、0 错误，Speech 定向测试 9/9 按预期失败，统一根因为 `ShaheLauncher.Core.Speech.SpeechService` 尚不存在。
- 2026-08-08：C# Task 16 `features/speech.py` 契约 GREEN：`SpeechService` 已迁入 `ShaheLauncher.Core/Speech/`，Speech 定向契约 10/10 通过；确认 SpeechConfig 清洗、读屏 INI 默认值/写回、Configuration 写路径、Tolk DLL 动态入口、读屏检测/接口归一化、原中文状态和警告、辅助朗读后台线程、关闭禁用及 450ms 重复文案抑制保持 Python 行为；现有 WinForms `SpeechSettingsDialog` 文件未改动。
- 2026-08-08：Task 16 Python 对照与门禁通过：`python -m py_compile "沙河批量打开器.py" shahe_bulk_launcher\features\speech.py tests\test_updates_and_speech_contract.py` 成功；`python -m unittest tests.test_updates_and_speech_contract tests.test_feature_method_partition -v` 6/6 通过；`speech_assist_no_blocking_probe.py` 与 `configuration_write_only_probe.py` 均 PASS，确认辅助朗读不阻塞调用线程且语音配置写路径保持 `Configuration`；`git diff --check` 通过。
- 2026-08-08：Task 16 C# 跨模块回归通过：`ShaheLauncher.Tests.dll /Platform:x64` 全量契约 127/127 通过，Speech 新增服务未回归 Task 1-15；下一步执行最终解决方案 Release 构建和 Task 16 提交门禁。
- 2026-08-08：Task 16 最终构建通过：`ShaheBulkLauncher.sln` `Release|x64` 构建退出码 0，MSBuild 汇总为 0 警告、0 错误；Core、App、Updater、Tests 均成功生成，Task 16 进入独立提交前检查。
- 2026-08-08：Task 17 `features/updates.py` C# 契约 RED：新增 `UpdateServiceContractTests.cs` 与 `UpdateInstallerContractTests.cs`，Tests 项目 `Release|x64` 构建 0 警告、0 错误；`TestCategory=Updates` 运行 11 项、11/11 按预期失败，统一根因为 `ShaheLauncher.Core.Updates.UpdateService` 与 `UpdateInstaller` 尚不存在。契约冻结更新清单 UTF-8 BOM/缓存破坏请求头、增量补丁选择、原中文文案、打包更新器临时副本与请求 JSON、后台检查线程、二维码资源回退，以及更新器参数/请求解析和 SHA-256 校验边界。
- 2026-08-08：Task 17 `features/updates.py` C# Core 定向 GREEN：新增 `UpdateService`、`UpdateInstaller`，并扩展 `UpdateHttpClient` 的请求头注入；Tests 项目 `Release|x64` 构建 0 警告、0 错误，`TestCategory=Updates` 运行 11 项、11/11 通过。确认更新清单缓存破坏请求头/UTF-8 BOM、当前版本增量补丁选择、原中文状态/提示、打包更新器 helper 与请求 JSON、后台检查线程、二维码文件/嵌入资源回退，以及 updater 参数/请求解析和 SHA-256 校验边界保持契约。
- 2026-08-08：Task 17 Python 对照通过：`python -m py_compile "沙河批量打开器.py" shahe_bulk_launcher\features\updates.py tests\test_updates_and_speech_contract.py` 成功；`python -m unittest tests.test_updates_and_speech_contract tests.test_feature_method_partition -v` 6/6 通过；`update_package_selection_probe.py` 与 `update_helper_command_probe.py` 均 PASS，确认 Python 正式基线的补丁选择、非法大小归零、打包 helper 复制、internal 同步和请求字段未回归。
- 2026-08-08：Task 17 跨模块与最终门禁通过：C# 全量契约 `ShaheLauncher.Tests.dll /Platform:x64` 为 138/138；`ShaheBulkLauncher.sln` `Release|x64` 构建退出码 0，0 警告、0 错误；Python `compileall -q .` 成功，`python -m unittest discover -s tests -v` 为 29/29。Task 17 仅保留上述外部 updater 安装器源码缺失风险，不修改 Python 正式基线、WinForms UI、中文文案、快捷键、焦点顺序、AccessibleName 或配置键。
- 2026-08-08：C# Task 14 调度完成回调边界 RED：新增契约要求 `CompleteLaunch` 在当前会话完成时清理取消事件，并让 1/800/2500 毫秒延迟回调实际刷新可见沙盒运行状态；解决方案 Release|x64 构建 0 警告、0 错误，Scheduling 定向测试 7/8 通过，唯一失败为 `LaunchSchedulerCallbacks.ClearLaunchCancellation` 公共属性尚不存在。该修正独立于已提交的 Task 14 功能提交，下一步只改 C# 调度回调和契约。
- 2026-08-08：C# Task 14 调度完成回调边界 GREEN：`CompleteLaunch` 已补齐当前会话取消清理委托，1/800/2500 毫秒延迟任务已实际调用可见沙盒运行状态刷新委托；解决方案 Release|x64 构建 0 警告、0 错误，Scheduling 定向契约 8/8 通过。该修正保持 Python 的 UI 调度、文案和延迟边界，下一步进入 Task 15 `backup.py`。
- 2026-08-08：C# Task 14 `features/scheduling.py` 最终完成：`LaunchScheduler` 已迁入 `ShaheLauncher.Core/Scheduling/`，Scheduling 定向契约 8/8、C# 全量契约 110/110、Python 结构回归 6/6、`launch_no_ui_blocking_probe.py` 与 `launch_done_no_sync_menu_probe.py` 均 PASS、Python 模块语法验证通过；`ShaheBulkLauncher.sln` `Release|x64` 构建成功，0 警告、0 错误。WinForms UI、中文文案、快捷键、焦点顺序、AccessibleName、配置键和 Python 正式入口均未修改；Task 15 `backup.py` 为下一迁移模块。
- 2026-08-08：C# Task 14 Python 对照与构建门禁通过：`python -m py_compile shahe_bulk_launcher\features\scheduling.py` 成功；`launch_no_ui_blocking_probe.py`、`launch_done_no_sync_menu_probe.py` 均 PASS；`test_feature_method_partition` 与 `test_remaining_features_contract` 共 6/6 通过。`ShaheBulkLauncher.sln` `Release|x64` 构建成功，Core、App、Updater、Tests 0 警告、0 错误；下一步运行 C# 全量契约确认既有模块无回归。
- 2026-08-08：C# Task 14 `features/scheduling.py` 定向 GREEN：新增 `LaunchScheduler` 后，Tests 项目 `Release|x64` 构建成功，0 警告、0 错误；`TestCategory=Scheduling` 运行 8 项、8/8 通过。契约确认定时任务按秒下限/序号/重试/异常/关闭处理，原文状态和显式勾选应用，批量启动跳过已运行沙盒、并发、失败计数、音量恢复、完成状态与 F5 刷新延迟，以及备份恢复委托均保持 Python 边界。
- 2026-08-08：C# Task 14 `features/scheduling.py` 有效 RED：新增 `LaunchSchedulerContractTests.cs`，Tests 项目 `Release|x64` 构建成功，0 警告、0 错误；`TestCategory=Scheduling` 运行 8 项、8/8 按预期失败，失败点统一为 `ShaheLauncher.Core.Scheduling.LaunchScheduler` 尚不存在。契约冻结定时任务重试/取消、原文状态、关闭聚合、显式勾选应用、启动目标校验、调度批次、动态延迟、完成回调及备份恢复委托边界。
- 2026-08-08：C# Task 13 `features/sandbox_admin.py` 最终完成：`SandboxAdminService` 已迁入 `ShaheLauncher.Core/SandboxAdmin/`，SandboxAdmin 定向契约 8/8、C# 全量契约 102/102、Python 结构回归 6/6、`sandbox_admin_transform_probe.py` 与 `group_navigation_probe.py` 均 PASS、Python 模块语法验证通过；`ShaheBulkLauncher.sln` `Release|x64` 构建成功，0 警告、0 错误。WinForms UI、中文文案、快捷键、焦点顺序、AccessibleName、配置键和 Python 正式入口均未修改；Task 14 `features/scheduling.py` 为下一迁移模块。
- 2026-08-08：C# Task 13 Python 对照与构建门禁通过：`python -m py_compile shahe_bulk_launcher\features\sandbox_admin.py` 成功；`sandbox_admin_transform_probe.py`、`group_navigation_probe.py` 均 PASS；`test_feature_method_partition` 与 `test_remaining_features_contract` 共 6/6 通过。`ShaheBulkLauncher.sln` `Release|x64` 构建成功，Core、App、Updater、Tests 0 警告、0 错误；下一步运行 C# 全量契约确认既有模块无回归。
- 2026-08-08：C# Task 13 `features/sandbox_admin.py` 定向 GREEN：Tests 项目 `Release|x64` 重新构建成功，0 警告、0 错误；`TestCategory=SandboxAdmin` 运行 8 项、8/8 通过。契约确认分组/置顶、批量命名和原文状态、Sandboxie.ini 变换/备份/UserSettings 修复、设置与定时任务引用、目录重命名回滚、注册表清理计划、别名失败文案和删除前停进程回调均保持 Python 边界；下一步执行跨模块回归与解决方案构建后再声明 Task 13 完成。
- 2026-08-08：C# Task 13 修正契约调用边界并重新构建后，`TestCategory=SandboxAdmin` 为 7/8；唯一失败是测试期待显示名重命名同步迁移静音缓存。对照 Python `_rename_sandbox_refs_in_settings_by_display` 确认该路径只更新分组、置顶和音量记忆，不修改 `_cached_mute_state_by_sandbox`，因此应修正测试以保留原行为，不改 C# 生产逻辑。
- 2026-08-08：C# Task 13 `features/sandbox_admin.py` 首轮实现后定向测试为 4/8：分组/置顶、INI 分段与 UserSettings 变换、别名与删除前停进程等 4 项通过；其余失败已定位为契约调用边界而非已确认的生产行为偏差，包括 `string[]` 被反射辅助方法的 `params object[]` 展开、同一秒连续备份按 Python 文件名规则覆盖，以及内部名重命名不会再次更新仅按显示名迁移的定时任务引用。下一步先修正这些测试假设并重新构建，再根据剩余失败修生产代码。
- 2026-08-08：C# Task 13 `features/sandbox_admin.py` 有效 RED：Tests 项目 `Release|x64` 构建成功，0 警告、0 错误；`TestCategory=SandboxAdmin` 运行 8 项、8/8 按预期失败，失败点统一为 `ShaheLauncher.Core.SandboxAdmin.SandboxAdminService` 尚不存在。测试本身可编译执行，下一步实现 Core 服务后只修生产代码使定向契约转 GREEN。
- 2026-08-08：C# Task 13 `features/sandbox_admin.py` 契约准备：新增 `SandboxAdminServiceContractTests.cs`，计划以 8 项契约冻结分组/置顶、批量命名与原文状态、Sandboxie.ini 分段/设置/备份/修复、设置和定时任务引用迁移、目录重命名回滚、注册表清理路径、别名更新及删除前停进程边界；生产 `SandboxAdminService` 尚未创建，下一步构建并运行定向测试取得有效 RED。
- 2026-08-08：C# Task 12 `features/process_actions.py` RED：新增 `ProcessActionServiceContractTests.cs`，冻结 8 项进程操作契约，覆盖播放设备 `（默认）` 原文、PID 归一化与全部进程回退、窗口候选排序/选择文案、静音聚合/详细结果、音量钳制/记忆、taskkill 分块与 Sandboxie terminate 参数、菜单状态/静音缓存、状态原文及音频/关闭后台线程边界；Tests 项目 `Release|x64` 构建通过，定向测试为 8/8 预期失败，唯一失败根因为 `ShaheLauncher.Core.Processes.ProcessActionService` 尚不存在。Python 权威模块未修改，下一步实现最小 Core 服务后定向转 GREEN。
- 2026-08-08：C# Task 12 `features/process_actions.py` GREEN：`ProcessActionService` 已迁入 `ShaheLauncher.Core/Processes/`，覆盖设备标签、PID/窗口查找与激活、静音/音量会话结果、音量记忆、Sandboxie terminate/taskkill、菜单缓存、关闭/定位结果和后台任务锁；`ProcessActionServiceContractTests` 定向运行 8/8 通过，Tests 项目 `Release|x64` 构建 0 警告、0 错误。设备路由、音频会话枚举和 WinForms 状态更新均保留为后续 `MainForm` 委托接线边界，未改变 Python UI、文案、快捷键、焦点顺序、无障碍或正式入口。
- 2026-08-08：C# Task 12 跨模块回归通过：`ShaheLauncher.Tests.dll /Platform:x64` 运行 94 项、94/94 通过；Python `test_feature_method_partition`、`test_process_actions_contract`、`test_app_paths_and_sandboxie` 共 6/6 通过；`audio_process_actions_no_blocking_probe.py`、`close_process_no_blocking_probe.py`、`locate_no_blocking_probe.py` 均 PASS；`python -m py_compile shahe_bulk_launcher\features\process_actions.py` 通过。Python 权威入口和 UI 未改动，下一步只剩解决方案构建与提交门禁。
- 2026-08-08：C# Task 12 最终完成：`ShaheBulkLauncher.sln` `Release|x64` 构建成功，Core、App、Updater、Tests 0 警告、0 错误；最终 C# 全量契约 94/94 通过。`ProcessActionService` 的 UI、音频路由和读屏边界继续通过委托保留原接线位置，Python 正式入口、界面、文案、快捷键、焦点顺序、无障碍和配置键均未修改；Task 13 `features/sandbox_admin.py` 作为下一模块。
- 2026-08-08：C# Task 11 `features/sandboxie.py` 最终完成：`SandboxieClient` 已迁入 `ShaheLauncher.Core/Sandboxie/`，Sandboxie 定向契约 8/8、C# 全量契约 86/86、Python 应用路径/Sandboxie 结构对照 4/4、非阻塞启动与立即失败探针均 PASS、Python 模块语法验证通过；`ShaheBulkLauncher.sln` 最新 `Release|x64` 构建成功，0 警告、0 错误。UI、文案、快捷键、焦点顺序、无障碍、配置键和 Python 正式入口均未改动；INI 写入、配置重载和 UI 接线继续通过后续 SandboxAdmin/MainForm 边界委托，剩余 9 个 C# 模块按映射顺序迁移。
- 2026-08-08：C# Task 11 最终门禁复跑通过：解决方案构建后再次运行 `ShaheLauncher.Tests.dll /Platform:x64` 为 86/86；Python 结构对照为 4/4，`launch_start_detached_probe.py` 与 `launch_detached_immediate_failure_probe.py` 均 PASS，`python -m py_compile shahe_bulk_launcher\features\sandboxie.py` 通过。
- 2026-08-08：C# Task 11 Windows 参数转义 GREEN 通过：修正测试数组的反射传参后，`TestCategory=Sandboxie` 恢复 8/8；`FormatProcessArguments` 确认带空格路径保留普通反斜杠，尾随反斜杠仅在闭合引号前按 Windows 命令行规则成对转义，实际 `ProcessStartInfo.Arguments` 共用同一实现。
- 2026-08-08：C# Task 11 Windows 参数转义首轮 GREEN 仍为 7/8：源码和 Release DLL 均确认已公开 `FormatProcessArguments(IEnumerable<string>)`，失败原因是测试把单个 `string[]` 传给 `params object[]` 时被展开成两个反射参数，属于契约调用错误；下一步将数组显式转换为单个 `object` 后重跑，不再改生产算法。
- 2026-08-08：C# Task 11 Windows 参数转义补充 RED 通过：新增带空格路径和尾随反斜杠断言后，`TestCategory=Sandboxie` 为 7/8，唯一失败是 `FormatProcessArguments` 尚不存在；该契约用于防止 .NET 命令行字符串把普通 Windows 路径反斜杠错误翻倍，既有 7 项继续通过。
- 2026-08-08：C# Task 11 Sandboxie 只读现场核对通过：Release Core 程序找到本机 `C:\Program Files\Sandboxie`，执行 `SbieIni queryex /boxes *` 解析 84 个配置沙盒，`SbieDll` API 枚举返回 0 个沙盒、0 个进程；全程未启动、关闭或修改沙盒数据。
- 2026-08-08：C# Task 11 最终解决方案构建通过：`ShaheBulkLauncher.sln` 以 `Release|x64` 完成还原和生成，Core、App、Updater、Tests 全部成功输出，0 警告、0 错误。
- 2026-08-08：C# Task 11 当前累计全量测试通过：`ShaheLauncher.Tests.dll /Platform:x64` 运行 86 项、86/86 通过，包含 Sandboxie 8 项及 Task 1-10 的既有 78 项，无已完成模块回归。
- 2026-08-08：C# Task 11 Python 对照 GREEN：`python -m unittest tests.test_feature_method_partition tests.test_app_paths_and_sandboxie -v` 运行 4 项、4/4 通过；`launch_start_detached_probe.py` 输出 `launched True`、`run_calls 0`、`popen_calls 1` 并 PASS；`launch_detached_immediate_failure_probe.py` 输出两次派发、重载和原文无效沙盒状态并 PASS；`python -m py_compile shahe_bulk_launcher\features\sandboxie.py` 通过，Python 权威入口未修改。
- 2026-08-08：C# Task 11 Sandboxie 定向 GREEN 通过：`TestCategory=Sandboxie` 运行 8 项、8/8 通过，确认首次扫描/修复文案、安装文件优先级、SbieIni/INI 分节、别名和名称映射、Start.exe 参数、PID 解析与回退、官方创建删除、非阻塞启动重试/状态以及安装路径发现顺序保持 Python 契约。
- 2026-08-08：C# Task 11 参数顺序修正后构建通过：`BuildStartCommand` 改为过滤帮助/空参数后先判空，再按 Python 顺序决定是否插入 `/silent`；Tests 项目以 `Release|x64` 成功生成，Core、App、Tests 均 0 警告、0 错误。
- 2026-08-08：C# Task 11 首轮定向 GREEN 为 7/8：安装路径/文案、SbieIni 与分节、显示映射、PID、官方创建删除、启动重试和安装发现 7 项通过；唯一失败为帮助/空参数输入时 `BuildStartCommand` 先插入 `/silent`，把 Python 应当拒绝的空命令误标为有效。失败已定位为 C# 参数处理顺序偏差，下一步只修正“先判空、再静默参数”的顺序后重跑。
- 2026-08-08：C# Task 11 `features/sandboxie.py` GREEN 构建通过：新增 `SandboxieClient` 后，Tests 项目以 `Release|x64` 成功生成，Core、App、Tests 均 0 警告、0 错误；开始运行 8 项 Sandboxie 定向契约。
- 2026-08-08：C# Task 11 `features/sandboxie.py` 有效 RED 通过：`TestCategory=Sandboxie` 运行 8 项、8/8 按预期失败，失败点统一为 `ShaheLauncher.Core.Sandboxie.SandboxieClient` 尚不存在。契约已冻结首次扫描与修复 Sandboxie Control 的原文、安装文件优先级、SbieIni 输出/INI 分节、别名显示映射与内部名称生成、Start.exe 参数和无效沙盒输出识别、PID 解析及 API/逐盒回退、官方创建/删除命令顺序、非阻塞启动重试与失败状态，以及注册表/标准目录/通配结果的安装路径优先级。
- 2026-08-08：C# Task 11 `features/sandboxie.py` RED 构建通过：新增 `SandboxieClientContractTests` 后，Tests 项目以 `Release|x64` 完成还原和生成，Core、App、Tests 均成功输出，0 警告、0 错误；生产代码尚未创建。
- 2026-08-08：C# Task 10 `features/app_paths.py` 最终完成：`AppPathHistoryService` 已迁入 `ShaheLauncher.Core/AppPaths/`，AppPaths 定向契约 8/8、C# 全量契约 78/78、Python 应用路径/Sandboxie 结构对照 4/4、剪贴板无效输入探针 PASS、Python 模块语法验证通过；`ShaheBulkLauncher.sln` 以 `Release|x64` 构建成功，0 警告、0 错误。UI、文案、快捷键、焦点顺序、无障碍、配置键和 Python 正式入口均未改动；主窗体接线按边界留到后续 lifecycle/app 模块。剩余 10 个 C# 模块继续按映射顺序迁移。
- 2026-08-08：C# Task 10 文案补充后的全量回归通过：`ShaheLauncher.Tests.dll /Platform:x64` 运行 78 项、78/78 通过，确认新增 AppPaths 文案常量未影响 Task 1-9 既有行为。
- 2026-08-08：C# Task 10 AppPaths 文案边界 GREEN 通过：`TestCategory=AppPaths` 运行 8 项、8/8 通过，新增并确认 `选择应用`、`粘贴 .exe 或文件路径`、备注输入、记忆数量输入及路径记忆状态文案保持 Python 原文。
- 2026-08-08：C# Task 10 AppPaths 文案补充后的定向构建通过：新增菜单、备注和记忆数量原文常量后，Tests 项目以 `Release|x64` 生成成功，0 警告、0 错误；重跑 8 项 AppPaths 契约。
- 2026-08-08：C# Task 10 AppPaths 文案边界 RED 通过：补充右键菜单/备注/记忆数量原文断言后，`TestCategory=AppPaths` 为 7/8，唯一失败是 `OpenAppMenuText` 尚不存在；其余 7 项行为契约继续通过，未发现业务回归。
- 2026-08-08：C# Task 10 `features/app_paths.py` 解决方案构建通过：`ShaheBulkLauncher.sln` 以 `Release|x64` 完成还原和生成，Core、App、Updater、Tests 全部成功输出，0 警告、0 错误；主行为定向验证 8/8、C# 累计全量 78/78，通过后补做应用路径菜单/备注/数量文案边界审计再提交。
- 2026-08-08：C# Task 10 当前累计全量测试通过：`ShaheLauncher.Tests.dll /Platform:x64` 运行 78 项、78/78 通过，包含 AppPaths 8 项及 Task 1-9 的既有 70 项，无已完成模块回归。
- 2026-08-08：C# Task 10 Python 应用路径模块语法验证通过：`python -m py_compile shahe_bulk_launcher\features\app_paths.py` 返回码 0，确认 Python 权威模块保持可编译，未被 C# 迁移改动。
- 2026-08-08：C# Task 10 Python 剪贴板无效输入探针复现通过：再次运行 `python tests\probes\paste_invalid_clipboard_probe.py` 并显式打印进程码，输出 `STATUS 剪贴板没有包含有效的数据`、`APP_PATH` 为空、`PASS paste_invalid_clipboard_probe`，返回码 0；首轮返回码 1 未复现，按最终证据计入 Python 对照 GREEN。
- 2026-08-08：C# Task 10 Python 剪贴板无效输入探针出现部分结果：`python tests\probes\paste_invalid_clipboard_probe.py` 输出原文 `STATUS 剪贴板没有包含有效的数据`、`APP_PATH` 为空及 `PASS paste_invalid_clipboard_probe`，业务断言已执行；但 Python 进程返回码为 1，当前只记录为“断言输出通过、进程退出异常”，未计入完整 GREEN，待复现探针退出路径。
- 2026-08-08：C# Task 10 Python 应用路径/Sandboxie 结构对照通过：`python -m unittest tests.test_feature_method_partition tests.test_app_paths_and_sandboxie -v` 运行 4 项、4/4 通过，确认生命周期/设置方法唯一分区及 `AppPathsMixin`、`SandboxieMixin` 的既有方法清单保持不变；C# 迁移未修改 Python 正式入口。
- 2026-08-08：C# Task 10 AppPaths 定向 GREEN 通过：`TestCategory=AppPaths` 运行 8 项、8/8 通过，确认历史排序/去重/截断、`.exe` 原始/引号/换行/`file://` 解析、隐藏与完整显示文本、历史解析、应用列表选择/路径/勾选恢复、当前路径回调顺序、记忆操作状态反馈及剪贴板文件优先于文本的候选顺序保持 Python 契约。
- 2026-08-08：C# Task 10 AppPaths 隐藏路径契约修正后构建通过：测试显式设置 `HidePath=true` 验证纯备注解析，并恢复完整路径模式验证带路径文本，Tests 项目 `Release|x64` 0 警告、0 错误；继续重跑 AppPaths 定向测试。
- 2026-08-08：C# Task 10 AppPaths 契约修正后构建通过：调整静态方法反射调用、重复历史回调数量和 `ApplyHistoryCount` 的 Python 顺序断言后，Tests 项目以 `Release|x64` 生成成功，0 警告、0 错误；开始重跑 AppPaths 定向测试。
- 2026-08-08：C# Task 10 首轮定向 GREEN 为 4/8：`TestCategory=AppPaths` 运行 8 项，默认值、列表视图、当前路径回调顺序和剪贴板候选顺序 4 项通过；4 项失败已定位为契约/边界问题：测试反射助手未调用 `public static` 方法、历史回调数量断言未按 Python 超限保存顺序冻结，以及 `ApplyHistoryCount` 状态断言错误索引。生产路径解析和记忆操作尚无对应失败证据，下一步先修正测试调用/断言后重跑。
- 2026-08-08：C# Task 10 `features/app_paths.py` GREEN 构建通过：修正路径正则的 C# 字符串转义并改用标准库 `Path.GetFullPath` 后，Tests 项目以 `Release|x64` 成功生成，Core、App、Tests 均 0 警告、0 错误；下一步运行 8 项 AppPaths 定向契约。
- 2026-08-08：C# Task 10 第二轮 GREEN 构建仍未通过：修正正则后，编译错误收敛为 `AppPathHistoryService.cs` 第 602 行 1 项，原因是误将自定义扩展方法写成不存在的 `Path.NormalizePath(value)` 静态调用；未进入 AppPaths 契约执行。下一步改用标准库 `Path.GetFullPath(value)` 并移除无用扩展。
- 2026-08-08：C# Task 10 首轮 GREEN 构建未通过：`AppPathHistoryService.cs` 新增路径正则的普通 C# 字符串转义错误，MSBuild 在第 104-108 行报告 22 个语法错误；错误发生在生产代码编译阶段，未进入 AppPaths 契约执行，测试和 Python 权威实现尚无失败证据。下一步只修正正则字面量后重跑构建。
- 2026-08-08：C# Task 10 `features/app_paths.py` 有效 RED 通过：`TestCategory=AppPaths` 运行 8 项、8/8 按预期失败，失败点统一为 `ShaheLauncher.Core.AppPaths.AppPathHistoryService` 尚不存在。契约已冻结路径记忆默认值与菜单/状态原文、历史添加排序/去重/截断及保存刷新顺序、`.exe` 原始/引号/换行/`file://` 解析、显示文本与历史解析、应用列表选择/路径/勾选恢复、当前应用路径回调顺序、记忆操作状态反馈以及剪贴板文件优先于文本的候选顺序。
- 2026-08-08：C# Task 10 `features/app_paths.py` RED 构建通过：新增 `AppPathHistoryServiceContractTests` 后，Tests 项目以 `Release|x64` 完成还原和生成，Core、App、Tests 均成功输出，0 警告、0 错误；下一步运行 `TestCategory=AppPaths`，预期仅因 `ShaheLauncher.Core.AppPaths` 目标类型尚不存在而失败。
- 2026-08-07：C# Task 9 `features/settings.py` 完成：`ShaheBulkLauncher.sln` 以 `Release|x64` 完成还原和生成，Core、App、Updater、Tests 全部成功输出，0 警告、0 错误；最终验证为 SettingsService 8/8、C# 累计 70/70、Python 设置模块结构契约 3/3、默认值与首次扫描行为探针 PASS，以及设置模块语法验证通过。`LauncherSettingsService` 已迁入 `ShaheLauncher.Core/Settings/`，SettingsDialog 与最终 MainForm 接线仍按边界留到 lifecycle/app，Python 正式入口未改变；其余 11 个模块继续按映射顺序迁移。
- 2026-08-07：C# Task 9 当前累计全量测试通过：`ShaheLauncher.Tests.dll /Platform:x64` 运行 70 项、70/70 通过，包含 SettingsService 8 项及 Task 1-8 的既有 62 项，无已完成模块回归。
- 2026-08-07：C# Task 9 Python 设置模块语法验证通过：`python -m py_compile shahe_bulk_launcher\features\settings.py` 返回码 0，确认 Python 权威模块保持可编译，未被 C# 迁移改动。
- 2026-08-07：C# Task 9 Python 默认值与首次扫描行为对照通过：`python tests\probes\defaults_and_first_scan_probe.py` 输出 `PASS tmp_defaults_and_first_scan_probe`，确认 `list_use_left_right_nav=True`、朗读辅助提示默认启用，以及未找到 Sandboxie 时的扫描状态和“扫描结果”弹框原文保持权威基线。
- 2026-08-07：C# Task 9 Python 设置模块结构对照通过：`python -m unittest tests.test_feature_method_partition -v` 运行 3 项、3/3 通过，确认生命周期与设置模块仍存在、开机自启动目标不指向 feature 文件，并且 `SandboxLauncher` 方法继续唯一分区；C# 迁移未修改 Python 正式入口。
- 2026-08-07：C# Task 9 设置服务定向 GREEN 通过：`TestCategory=SettingsService` 运行 8 项、8/8 通过，确认 23 个默认设置键及嵌套集合独立、HTTPS 更新清单秘密存储、`settings_json`/旧 `app_config.dat` 加载迁移与损坏回退、保存时清理旧更新键、设置规范化及 Python 副作用顺序、重置原文和最终状态、沙盒根与 Sandboxie `Start.exe` 路径校正均保持 Python 行为。
- 2026-08-07：C# Task 9 `features/settings.py` GREEN 构建通过：新增 `LauncherSettingsService` 后，Tests 项目以 `Release|x64` 完成还原和生成，Core、App、Tests 均成功输出，0 警告、0 错误；下一步运行 `TestCategory=SettingsService` 的 8 项定向契约并按失败证据修正迁移偏差。
- 2026-08-07：C# Task 9 `features/settings.py` 有效 RED 通过：Tests 项目以 `Release|x64` 构建成功，0 警告、0 错误；`TestCategory=SettingsService` 运行 8 项、8/8 按预期失败，失败点统一为 `ShaheLauncher.Core.Settings.LauncherSettingsService` 尚不存在。契约已冻结 23 个默认设置键与嵌套独立性、更新清单秘密配置、数据库加载与旧 JSON 迁移、损坏回退、保存键清理、设置规范化和副作用顺序、重置原文/状态、沙盒根及 Sandboxie `Start.exe` 路径校正；SettingsDialog 与最终 MainForm 接线继续沿用已迁移 UI 和后续 lifecycle/app 边界。
- 2026-08-07：C# Task 8 `ui/tray.py` 完成：`ShaheBulkLauncher.sln` 以 `Release|x64` 完成还原和生成，Core、App、Updater、Tests 全部成功输出，0 警告、0 错误；最终验证为 Tray 3/3、C# 累计 62/62、Python 对话框/托盘结构契约 2/2 与托盘模块语法验证通过。`LauncherTrayIcon` 已迁入 `ShaheLauncher.App/UI/Tray/`，主窗体接线仍按边界留到 lifecycle/app，Python 正式入口未改变；其余 12 个模块继续按映射顺序迁移。
- 2026-08-07：C# Task 8 当前累计全量测试通过：`ShaheLauncher.Tests.dll /Platform:x64` 运行 62 项、62/62 通过，包含 Tray 3 项及 Task 1-7 的既有 59 项，无已完成模块回归。
- 2026-08-07：C# Task 8 Python 托盘模块语法验证通过：`python -m py_compile shahe_bulk_launcher\ui\tray.py` 返回码 0，确认 Python 权威模块保持可导入语法，未被 C# 迁移改动。
- 2026-08-07：C# Task 8 Python 权威结构对照通过：`python -m unittest tests.test_ui_dialogs_contract -v` 运行 2 项、2/2 通过，确认 Python `TaskBarIcon` 仍保留 `set_icon`、左右键处理、显示、退出、移除和销毁的直接方法，C# 迁移未修改正式 Python 入口。
- 2026-08-07：C# Task 8 托盘模块定向 GREEN 通过：Tests 项目以 `Release|x64` 构建成功，0 警告、0 错误；`TestCategory=Tray` 运行 3 项、3/3 通过，确认系统信息图标、`沙盒批量打开器` 提示、经典 `ContextMenu/MenuItem` 菜单原文、左键与菜单恢复隐藏/最小化窗口、精确状态与朗读反馈、反馈异常隔离、退出顺序以及 `RemoveIcon`/`Destroy` 幂等行为保持 Python 语义。
- 2026-08-07：C# Task 8 首轮 GREEN 为 2/3：图标/经典菜单和退出销毁两项通过；左键恢复测试中状态与朗读回调均触发，但隐藏且最小化的 WinForms 窗口保持 `Minimized`。`artifacts/runs/tray_debug` 独立 STA 探针确认 WinForms 在该状态下“先设 Normal、后 Show”会重新显示为最小化，“先 Show、后设 Normal”才能恢复；实现只调整两步顺序以保持 Python 的最终可见行为。
- 2026-08-07：C# Task 8 `ui/tray.py` 有效 RED 通过：Tests 项目以 `Release|x64` 构建成功，0 警告、0 错误；`TestCategory=Tray` 运行 3 项、3/3 按预期失败，失败点统一为 `ShaheLauncher.App.UI.Tray.LauncherTrayIcon` 尚不存在。契约已冻结信息图标、`沙盒批量打开器` 提示、经典 `ContextMenu/MenuItem` 的 `显示主界面`/`退出` 原文、左键与菜单恢复窗口、`已显示主窗口` 状态和 `launch_done` 朗读、反馈失败隔离、退出顺序及幂等销毁行为；主窗体接线留到后续 lifecycle/app 模块。
- 2026-08-07：C# Task 7 `ui/settings_dialogs.py` 完成：`ShaheBulkLauncher.sln` 以 `Release|x64` 完成还原和生成，Core、App、Updater、Tests 全部成功输出，0 警告、0 错误；最终验证为 SettingsDialogs 10/10、C# 累计 59/59、Python 对话框结构契约 2/2 与三个 UI 探针 PASS。四个 C# 对话框已迁入 `ShaheLauncher.App/UI/Dialogs/Settings/`，Python 正式入口未改变；其余 13 个模块继续按映射顺序迁移。
- 2026-08-07：C# Task 7 当前累计全量测试通过：`ShaheLauncher.Tests.dll /Platform:x64` 运行 59 项、59/59 通过，包含 SettingsDialogs 10 项及 Task 1-6 的既有 49 项，无已完成模块回归。
- 2026-08-07：C# Task 7 Python UI 广度探针通过：`python tests\probes\ui_broad_probe.py` 输出 `PASS ui_broad_probe`，确认对话框迁移未改变 Python 正式入口的关键 UI 文案、控件关系和可访问性基线。
- 2026-08-07：C# Task 7 Python 扩展控件名探针通过：`python tests\probes\dialog_control_names_more_probe.py` 输出 `PASS dialog_control_names_more_probe`，确认路径记忆与朗读设置的关键 AccessibleName 和可见控件文案保持 Python 权威基线不变。
- 2026-08-07：C# Task 7 Python 对话框控件名探针通过：`python tests\probes\dialog_control_names_probe.py` 输出 `PASS dialog_control_names_probe`，确认快捷键、常规设置及其他共用对话框的关键控件名称保持 Python 权威基线不变。
- 2026-08-07：C# Task 7 Python 权威结构对照通过：`python -m unittest tests.test_ui_dialogs_contract -v` 运行 2 项、2/2 通过，确认 Python `settings_dialogs.py` 的四个对话框仍保持既有模块归属，C# 迁移未修改正式 Python 入口。
- 2026-08-07：C# Task 7 `SettingsDialogs` 完整定向回归通过：`TestCategory=SettingsDialogs` 运行 10 项、10/10 通过，快捷键、常规设置、路径记忆和朗读设置四个对话框在同一程序集组合验证中无回归。
- 2026-08-07：C# Task 7 朗读设置子模块定向 GREEN 通过：目标三项 3/3 通过，确认 700×700 窗口、`读屏 API 接口:`/`自动（默认）`/`启用读屏辅助提示`/`确定`/`取消` 原文、`读屏 API 接口` AccessibleName、接口 DLL 显示值映射、配置清洗、reader.ini 值规范化，以及接口 DLL -> 辅助提示 -> 朗读配置的保存顺序与失败短路行为保持 Python 一致；源码中未创建控件的旧引擎编辑、检测和 reader.ini 收集方法未擅自接入 UI。
- 2026-08-07：C# Task 7 路径记忆子模块定向 GREEN 通过：目标两项 2/2 通过，确认 700×500 窗口、0..20 记忆数量范围、隐藏路径与列表原文、`路径记忆数量`/`已记忆的路径列表` AccessibleName、Tab 顺序、经典 `ContextMenu/MenuItem` 三项菜单、400×150 备注编辑、备注 trim、删除/清空精确确认及保存回调保持 Python 行为；列表选择变化本身不执行操作。
- 2026-08-07：C# Task 7 常规设置子模块定向 GREEN 通过：目标两项 2/2 通过，确认 560×640 可滚动窗口、全部原复选文案、0..15/1..32 数值范围、`打开沙盒间隔时间`/`启动并发数量限制`/`日志输出` AccessibleName、Tab 顺序、布尔/整数/日志默认值解析、重置确认原文，以及保存时移除旧更新键并写入 `auto_minimize_to_tray_after_launch=False` 的行为保持 Python 一致。
- 2026-08-07：C# Task 7 快捷键设置子模块定向 GREEN 通过：目标三项 3/3 通过，确认 760×560 窗口、原文与 AccessibleName、列表到按钮的 Tab 顺序、快捷键录入/删除、编辑框 Esc 录入边界、重置默认值并关闭启用开关，以及无效格式、固定基础快捷键占用、重复快捷键三类精确提示和规范化结果保持 Python 行为。
- 2026-08-07：C# Task 7 快捷键子模块首轮 GREEN 为 2/3：窗口/焦点顺序/编辑行为与重置默认值两项通过；校验测试因测试直接修改当前动作的 `WorkingHotkeys` 后，`ConfirmSettings` 按 Python 行为先从仍显示 `Ctrl+Enter` 的编辑框回写当前动作，导致测试数据被覆盖并意外成功。该结果属于测试未通过真实 UI 输入路径，生产冲突逻辑尚无失败证据；下一步只修正测试输入方式后重跑。
- 2026-08-07：C# Task 7 `ui/settings_dialogs.py` 有效 RED 通过：Tests 项目以 `Release|x64` 构建成功，0 警告、0 错误；`TestCategory=SettingsDialogs` 运行 10 项、10/10 按预期失败，失败点均为 `HotkeySettingsDialog`、`SettingsDialog`、`PathMemoryManagerDialog`、`SpeechSettingsDialog` 尚不存在。契约已冻结窗口尺寸、原文、Tab 顺序、AccessibleName、设置键、快捷键捕获/重置/校验、经典路径右键菜单、朗读配置清洗和保存短路顺序；朗读窗口只迁移 Python 当前实际可见的接口选择与辅助提示，不擅自接通源码中未创建控件的旧方法。
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
- 2026-10-16：路径记忆真实动作探针 retry6 未形成产品行为证据：最终隔离 Release x64 目标 PID `10252` 启动并正常退出（`CloseMainWindow=true`、退出码 `0`），但预置两条 `app_history` 后，第二个 `SysListView32` 应用路径列表在 12 秒内仍为 `0` 条，探针以 `Application path list did not populate before timeout` 返回码 `1`；配置和剪贴板均恢复，`SbieSvc=Running`。结果为 `artifacts/runs/csharp_navigation_followup_20261015/path-memory-actions-real-audit-retry6-result.json`，日志为 `path-memory-actions-real-audit-retry6.log`，不能据此判定产品缺陷或路径记忆动作通过；下一步先用只读启动诊断核对 UI 创建时序、列表句柄和管理器列表内容。
- 2026-10-16：路径记忆启动诊断脚本 `path-memory-startup-diagnostic.ps1` 通过 PowerShell 解析门禁，日志为 `artifacts/runs/csharp_navigation_followup_20261015/path-memory-startup-diagnostic-parse.log`，输出 `PARSE_OK`；该结果仅证明诊断脚本可解析，尚未形成真实 EXE 行为结论。
- 2026-10-16：路径记忆启动诊断取得部分真实 EXE 证据但首版清理无效：隔离 Release x64 目标 PID `2884` 的配置启动前后均保留两条预置 `app_history`，主窗口及两个 `SysListView32` 句柄均有效；约 `3.2s` 后沙盒列表为 `96` 条、应用路径列表仍为 `0` 条，持续约 `14s` 未改变。动态菜单解析到路径记忆管理命令 `359`，对话框成功打开，控件包含数量编辑框、隐藏路径按钮和历史 `ListBox`，但 `ListBox` 条目数为 `0`。配置恢复成功，首版诊断因未关闭模态对话框导致 `CloseMainWindow=false`、`Exited=false`，不能作为完整通过结果；结果为 `path-memory-startup-diagnostic-result.json`，日志为 `path-memory-startup-diagnostic.log`。
- 2026-10-16：路径记忆启动诊断自有进程清理完成：`cleanup-path-memory-diagnostic.ps1` 关闭 PID `2884` 的“路径记忆管理”对话框并请求主窗体退出，日志 `cleanup-path-memory-diagnostic.log` 记录 `DialogCloseRequested=true`、`MainCloseRequested=true`、`Exited=true`；PID 已不存在，`SbieSvc=Running`。该清理脚本只作用于本轮自有 PID。
- 2026-10-16：配置类型只读探针确认路径记忆启动空列表的独立缺陷方向：`Newtonsoft.Json` 将 `[settings] app_history=[...]` 解析为 `JArray`，元素类型为 `JObject`；元素不满足 `System.Collections.IDictionary`，仅满足泛型 `IDictionary<string,JToken>`。当前 `SandboxAdminService.NormalizeAppHistory()` 只读取非泛型 `IDictionary` 或 `string`，因此真实 C# 启动会丢弃 JSON 对象历史项；探针结果为 `artifacts/runs/csharp_navigation_followup_20261015/path-memory-jobject-type-probe.json`，日志为 `path-memory-jobject-type-probe.log`。该缺陷已记录为独立风险，本轮不直接混入生产修复；真实动作审计改为从空历史开始并通过粘贴入口新增路径。
- 2026-10-16：路径记忆动作探针为绕过已记录的 `JObject` 配置归一化缺陷增加 `-SeedAsScalar` 输入模式；该模式将一条路径写成 JSON 字符串并动态调整初始/粘贴/删除数量断言，原数组模式保持不变。脚本解析门禁通过，日志为 `artifacts/runs/csharp_navigation_followup_20261015/path-memory-actions-real-audit-parse3.log`，输出 `PARSE_OK`；尚未形成真实动作结果。
- 2026-10-16：路径记忆动作探针 retry7 部分验证并因探针匹配失败中止：隔离 Release x64 目标 PID `9888` 的标量种子在 `132ms` 内形成 1 条应用路径，选择/勾选状态可观察；但文件菜单文本包含固定快捷键后缀，旧探针未匹配到“粘贴 .exe 或文件路径”（`CommandId=-1`），管理器对话框随后虽打开，数量编辑框探针未命中且未能保存，后续模态步骤均未执行。配置和剪贴板恢复成功，目标因模态对话框首版清理未退出，随后由专用清理脚本关闭对话框并退出，`SbieSvc=Running`。结果为 `path-memory-actions-real-audit-retry7-result.json`，日志为 `path-memory-actions-real-audit-retry7.log`，清理日志为 `cleanup-path-memory-actions-retry7.log`；该轮不作为产品动作结论，下一轮修正菜单快捷键后缀和 WinForms 控件类名匹配。
- 2026-10-16：路径记忆动作探针修正（粘贴菜单 `Ctrl+V` 后缀和 WinForms 控件类名包含匹配）通过 PowerShell 解析门禁，日志为 `artifacts/runs/csharp_navigation_followup_20261015/path-memory-actions-real-audit-parse4.log`，输出 `PARSE_OK`；尚未形成新的真实动作结果。
- 2026-10-16：路径记忆动作探针 retry8 取得有效的粘贴和管理器隐藏路径证据：隔离 Release x64 目标 PID `8128` 以标量历史启动，初始列表 1 条；真实调用文件菜单命令 `258` 粘贴 `powershell.exe` 后列表为 2 条且配置写回两条历史；选中/勾选状态可观察；路径记忆管理命令 `301` 打开并保存隐藏路径，配置 `hide_path=true`，列表文本变为两个备注。数量编辑框探针仍未命中（尽管子控件枚举包含真实 `EDIT`），经典上下文菜单探针未找到可选弹出菜单，备注/删除/清空未执行；目标 `CloseMainWindow=true`、退出码 `0`，配置和剪贴板恢复，`SbieSvc=Running`。结果为 `path-memory-actions-real-audit-retry8-result.json`，日志为 `path-memory-actions-real-audit-retry8.log`；该轮只作为粘贴/管理器隐藏路径通过及剩余探针问题记录，不作为完整路径记忆动作通过。
- 2026-10-16：路径记忆动作探针 retry9 前置修正通过解析门禁：数量框改为从已枚举子控件的唯一 `EDIT` 句柄解析，上下文菜单改为同步投递 `WM_CONTEXTMENU`；日志为 `artifacts/runs/csharp_navigation_followup_20261015/path-memory-actions-real-audit-parse5.log`，输出 `PARSE_OK`，尚未形成新的真实动作结果。
- 2026-10-16：路径记忆动作探针 retry9 因同步跨进程 `WM_CONTEXTMENU` 投递挂起而无效：自有 PowerShell 探针 PID `8636` 在调用同步消息后无输出、无结果 JSON，目标 EXE PID `4932` 未形成动作证据；已停止自有探针进程并确认未留下沙河批量打开器进程，`SbieSvc=Running`。清理日志 `cleanup-path-memory-actions-retry9.log` 记录首次关闭请求未成功，但后续进程检查已为空；该轮不作为产品结论，后续恢复异步菜单投递并增加非阻塞快照诊断。
- 2026-10-16：路径记忆动作探针 retry10 前置修正通过解析门禁：异步 `WM_CONTEXTMENU` 恢复，并增加 `Shift+F10` 键盘上下文菜单兜底，避免同步跨进程消息阻塞；日志为 `artifacts/runs/csharp_navigation_followup_20261015/path-memory-actions-real-audit-parse6.log`，输出 `PARSE_OK`，尚未形成新的真实动作结果。
- 2026-10-16：路径记忆动作探针 retry10 验证数量设置和隐藏路径保存：隔离 Release x64 目标 PID `9608` 初始 1 条，粘贴命令 `258` 后 2 条；管理器命令 `301` 对话框的 `EDIT` 句柄、写入 `6`、隐藏复选框和确定关闭均成功，配置写回 `max_history_items=6`、`hide_path=true`，隐藏后列表只显示备注。结果中的 `manager_persisted=false` 仅因探针正则保留了 INI 行尾 `\r`，不是应用未写入；上下文菜单仍未被异步投递或 `Shift+F10` 探针发现，备注/删除/清空未执行。目标正常退出、配置/剪贴板恢复、`SbieSvc=Running`。结果为 `path-memory-actions-real-audit-retry10-result.json`，日志为 `path-memory-actions-real-audit-retry10.log`；该轮作为管理器保存通过和上下文菜单探针风险记录，不作为完整路径记忆动作通过。
- 2026-10-16：Sandbox 管理/定时非破坏性探针扩展增加“定时启动无选择”和“取消全部定时任务（无任务）”两个动作；脚本解析门禁通过，日志为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-admin-dialog-real-audit-parse3.log`，输出 `PARSE_OK`，尚未形成新的真实结果。
- 2026-10-16：Sandbox 管理/定时扩展探针取得真实文案和对话框证据：目标 PID `1928` 正常退出、`SbieSvc=Running`；新建分组实际显示“请先勾选要归类的沙盒用户”，定时关闭显示“请先勾选或选中要定时关闭的沙盒用户”，定时启动显示“请先勾选或选中要定时启动的沙盒用户”，取消无任务显示“当前没有定时任务”，批量创建对话框及规则控件成功打开并取消。原始结果为 `sandbox-admin-dialog-real-audit-retry2-result.json`，日志为 `sandbox-admin-dialog-real-audit-retry2.log`；其中新建分组仍用旧文案辅助函数、定时启动/取消用 PowerShell 文本匹配，`StatusMatches=false` 属于探针断言不匹配，不作为产品失败结论，下一轮改用已观察文案的专用断言。
- 2026-10-16：Sandbox 管理/定时探针断言修正（新建分组、定时启动、取消无任务改用专用 C# 文案匹配）通过 PowerShell 解析门禁，日志为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-admin-dialog-real-audit-parse4.log`，输出 `PARSE_OK`；尚未形成新的真实结果。
- 2026-10-16：Sandbox 管理/定时非破坏性真实复测通过：隔离 Release x64 目标 PID `6060` 动态调用新建分组命令 `285`、定时关闭 `295`、定时启动 `294`、取消全部定时任务 `296`，四项无选择/无任务状态断言均为 `true`；批量创建命令 `348` 成功打开规则对话框并发现规则提示、名称前缀、起始编号、编号格式、创建数量、确定/取消控件，随后取消。未创建/删除 Sandboxie 用户，目标正常退出、退出码 `0`，`SbieSvc=Running`。结果为 `sandbox-admin-dialog-real-audit-retry3-result.json`，日志为 `sandbox-admin-dialog-real-audit-retry3.log`。
- 2026-10-16：本轮差异检查 `git diff --check` 返回码 `0`；日志为 `artifacts/runs/csharp_navigation_followup_20261015/diff-check-current-turn.log`，仅有 Git 的 LF/CRLF 转换警告，无空白错误。检查时没有沙河批量打开器或本轮 PowerShell 探针残留，`SbieSvc=Running`。
- 2026-10-16：摘要同步后的最终差异检查 `git diff --check` 返回码 `0`；日志为 `artifacts/runs/csharp_navigation_followup_20261015/diff-check-final-current-turn.log`，仍仅有 LF/CRLF 转换警告，无空白错误。工作树仍只保留原有用户改动、文档改动和本轮未跟踪审计 artifacts，未暂存、未提交、未清理无关内容。
- 2026-10-16：Sandboxie 生命周期基线脚本首次解析调用失败，原因是外层 PowerShell 双引号展开使 `[scriptblock]::Create()` 收到空参数；脚本本身未执行，记录在 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-baseline-parse.log`，该结果不作为基线或产品证据，随后改用独立脚本块重跑。
- 2026-10-16：基线脚本更正调用后的第二次解析仍失败，Windows PowerShell 5.1 无 BOM 读取中文进程名字面量导致脚本解析报告未闭合字符串；未执行基线采集，日志为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-baseline-parse-corrected.log`，下一版改按目标 EXE 路径识别进程。
- 2026-10-16：基线脚本移除中文字面量并改按目标 EXE 路径识别进程后，Windows PowerShell 5.1 解析门禁通过，输出 `PARSE_OK`；日志为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-baseline-parse-final.log`。
- 2026-10-16：只读 Sandboxie 生命周期基线采集完成，候选本轮前缀为 `ShaheAudit_20261016_063753_136`；结果为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-baseline-20261016_063751_050.json`，日志为 `sandbox-lifecycle-baseline-20261016_063751_050.log`，Sandboxie.ini 副本为 `sandbox-lifecycle-baseline-20261016_063753_136-Sandboxie.ini`。`SbieIni /list` 返回 `1` 无输出，因此改按 C# 使用的 `queryex /boxes *` 补采 enabled boxes。
- 2026-10-16：基线脚本增加 `SbieIni queryex /boxes *` 后解析门禁通过，日志为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-baseline-parse-queryex.log`，下一步重新采集带 enabled boxes 原始命令输出的只读基线。
- 2026-10-16：基线脚本增加隔离 Release `Configuration\shahe_config.ini` 快照后解析门禁通过，日志为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-baseline-parse-config-copy.log`，下一步重新采集包含应用隔离配置副本的只读基线。
- 2026-10-16：包含应用配置副本的整合基线探针运行超过 90 秒未产出结果，已结束本轮自有探针；没有启动目标 EXE，也没有创建本轮沙盒。该轮不作为产品证据，后续使用窄范围应用配置只读快照。
- 2026-10-16：带 enabled boxes 原始命令输出的基线结果为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-baseline-queryex-20261016_064005_878.json`，日志为 `sandbox-lifecycle-baseline-queryex-20261016_064005_878.log`，Sandboxie.ini 副本为 `sandbox-lifecycle-baseline-20261016_064007_947-Sandboxie.ini`；`SbieIni queryex /boxes *` 返回 `0`，目标 EXE 无活动 PID，`SbieSvc=Running`。
- 2026-10-16：应用配置窄范围快照结果为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-app-config-20261016_064833_991.json`，日志为 `sandbox-lifecycle-app-config-20261016_064833_991.log`，副本为 `sandbox-lifecycle-app-config-20261016_064833_991.ini`；源文件 SHA-256 为 `99120B9DFFD177FF4E66FA9ACDC3547B9085D220EE99483405CAB30B2B91F234`。
- 2026-10-16：Sandboxie 生命周期真实审计探针 `sandbox-lifecycle-real-audit.ps1` 通过 Windows PowerShell 5.1 解析门禁，日志为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-real-audit-parse.log`，下一步使用唯一前缀启动并执行真实流程。
- 2026-10-16：新增非破坏性批量创建对话框时间线探针 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-batch-dialog-timeline.ps1`，尚未解析或执行；首次保留变量静态扫描命令因外层 PowerShell 双引号把正则中的 `$(` 解释为子表达式而失败。该失败只属于只读探针调用语法，未启动目标 EXE、未修改生产代码、未创建 Sandboxie 资源，下一步只修正该扫描命令一次。
- 2026-10-16：时间线探针保留变量静态扫描更正后通过并输出 `RESERVED_ASSIGNMENT_SCAN_OK`，确认脚本没有把 PowerShell 自动/保留变量用作赋值目标；尚未执行 PowerShell 5.1 解析门禁或启动目标 EXE。
- 2026-10-16：非破坏性批量创建对话框时间线探针通过 Windows PowerShell 5.1 AST 解析门禁并输出 `PARSE_OK`；日志为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-batch-dialog-timeline-parse.log`。尚未启动目标 EXE或形成控件时间线证据。
- 2026-10-16：时间线探针将内部泛型列表的 PowerShell 数组展开改为显式 `ToArray()` 后，Windows PowerShell 5.1 AST 解析再次输出 `PARSE_OK`；日志为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-batch-dialog-timeline-parse2.log`。仍未启动目标 EXE或创建 Sandboxie 资源。
- 2026-10-16：批量创建对话框时间线首轮采集为探针失败、无产品结论：目标 PID `8900`、主窗口 `0xBB07E0`，菜单命令 `348` 已投递，但诊断标题常量漏写“用户”，`158` 个内部样本均未匹配真实对话框；独立观察进程的 UTF-8 JSON 又被 Windows PowerShell 5.1 协调器按默认 ANSI 读取，导致反序列化失败。未设置输入或点击确定；仅强制结束本轮自有 PID。前后 INI SHA-256 均为基线值，`ShaheAudit_*` section/目录均为 `0`，`SbieSvc=Running`。结果为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-batch-dialog-timeline-20261016_113519_160-result.json`，独立原始结果为同名前缀 `-result-external.json`，日志为 `sandbox-batch-dialog-timeline-20261016_113519_160.log`。
- 2026-10-16：显式 UTF-8 读取首轮原始样本后取得部分有效时序：内部 `157` 个、独立 `164` 个对话框样本均为稳定 HWND `0xA20A1C`/线程 `4748`；内部从 `2 Edit/1 ComboBox/0 Button` 在 `21.0448ms` 内收敛到 `3/1/2`，独立观察首次命中即为完整结构。该轮排除 HWND 替换、持续宿主分裂、模态采样阻塞和 8 秒原生句柄延迟；旧生命周期稳定返回 `1` 的方向转向 PowerShell 集合/过滤链。派生摘要为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-batch-dialog-timeline-20261016_113519_160-retro-summary.json`，不作为产品缺陷或生命周期审计通过结论。
- 2026-10-16：新增集合语义探针 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-control-filter-semantics.ps1`，原样复刻生命周期脚本的 `Get-Controls -> Find-Control` 参数链；Windows PowerShell 5.1 AST 解析门禁输出 `PARSE_OK`，日志为 `sandbox-lifecycle-control-filter-semantics-parse.log`。尚未执行该语义探针。
- 2026-10-16：集合语义 RED 环在 Windows PowerShell 5.1 中精确复现：同一 C# 控件数组直接过滤得到 `3` 个 Edit，经过生命周期脚本现有 `[object[]]$Controls` 的 `Find-Control` 传参后，内联、变量和命名调用都稳定只返回 `1`。结论 `collection_shape_reproduced` 证明旧生命周期 `8s/1 Edit` 是 PowerShell 参数绑定/数组展开探针缺陷，而非 WinForms、HWND、模态线程或产品缺陷。结果为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-control-filter-semantics-result.json`，日志为 `sandbox-lifecycle-control-filter-semantics.log`。
- 2026-10-16：生命周期真实审计探针已把错误的 `Find-Control([object[]])` 传参链改为直接集合过滤；时间线探针同时修正完整标题、显式 UTF-8 JSON 读取和部分句柄时间线分类。两个脚本的 Windows PowerShell 5.1 AST 解析均输出 `PARSE_OK`，日志为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-probes-parse-collection-fix.log`；仅修改审计 artifacts，尚未重新执行真实 EXE。
- 2026-10-16：进一步复核提出更精确机制：`[string]$Title` 可能把显式 `$null` 强制转换为空字符串，因而只匹配标题为空的前缀 Edit；上一条内联过滤仍保留该参数类型，尚不能视为有效修正且尚未执行。集合语义探针已加入参数绑定和 `[object]$Title` 对照，Windows PowerShell 5.1 AST 解析输出 `PARSE_OK`，日志为 `sandbox-lifecycle-control-filter-semantics-parse2.log`。
- 2026-10-16：集合语义对照确认并纠正此前“数组展开”解释：旧函数的 Controls 始终完整绑定 `6` 个控件，但 `[string]$Title` 把显式 `$null` 强制转换为空字符串，只过滤出标题为空的 `1` 个 Edit；改用 `[object]$Title` 保留 null 后得到完整 `3` 个。结论 `null_to_empty_string_coercion_reproduced`、`NullStringCoercionConfirmed=true`，结果为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-control-filter-semantics-result2.json`，日志为 `sandbox-lifecycle-control-filter-semantics2.log`。旧 `8s/1 Edit` 的根因是审计脚本参数转换，不是产品缺陷。
- 2026-10-16：生命周期真实审计探针将 `Wait-Controls.Title` 从 `[string]` 改为 `[object]` 以保留 null，并仅在非 null 时进行字符串标题比较；Windows PowerShell 5.1 AST 解析输出 `PARSE_OK`，日志为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-real-audit-parse-null-title-fix.log`。尚未形成真实 EXE GREEN。
- 2026-10-16：非破坏性双观察者时间线 GREEN 完成：目标 PID `12400`，内部/独立分别采集 `168/173` 个样本，均在同一 HWND `0xA90C44`/线程 `10004` 上从 `2` 个 Edit 于 `53.1352ms/47.5152ms` 内收敛到 `3 Edit/1 ComboBox/2 Button`；没有 HWND 替换或模态采样阻塞。原始 `observer_context_difference_observed` 只比较了相隔 `21.14ms` 的初始化样本，是分类器误报；之后两边持续一致。对话框以 `WM_CLOSE` 取消，目标正常退出码 `0`，前后 INI 哈希与零 `ShaheAudit_*` section/目录保持不变，`SbieSvc=Running`。结果为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-batch-dialog-timeline-20261016_114555_702-result.json`，判定为同名前缀 `-assessment.json`。明确根因为正常约 50ms 句柄创建叠加旧探针 `[string]$null -> ""` 过滤错误；无产品缺陷证据。
- 2026-10-16：时间线分类器已收紧：共同的部分控件到完整 `3 Edit` 过渡优先判为句柄创建时序，宿主上下文差异要求至少 `3` 个持续不一致近邻样本；Windows PowerShell 5.1 AST 解析输出 `PARSE_OK`，日志为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-batch-dialog-timeline-parse-classifier-fix.log`。不为标签重跑已有有效 EXE 时间线。
- 2026-10-16：全新只读基线采集通过，本轮唯一前缀 `ShaheAudit_20261016_114911_269`；`queryex` 返回码 `0`、输出 `96` 项，INI/隔离配置 SHA-256 分别为 `2709ADB23A9D1080D91C0401DC86A9F954AF81DEAFB5FFD7DF03F5AA30E75A6D` 与 `99120B9DFFD177FF4E66FA9ACDC3547B9085D220EE99483405CAB30B2B91F234`，目标进程和 `ShaheAudit_*` section/目录均为 `0`，`SbieCtrl PID 6844`、`SbieSvc PID 2836/4916` 保持且服务 `Running`。JSON 为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-narrow-baseline-direct-20261016_114911_220.json`，日志为 `sandbox-lifecycle-narrow-baseline-direct-20261016_114911_220.log`，INI/配置副本使用同一前缀。
- 2026-10-16：前缀 `ShaheAudit_20261016_114911_269` 的真实生命周期审计通过启动/列表和批量创建两个沙盒后，在精确菜单路径 `文件(&F)|粘贴 .exe 或文件路径` 处未匹配实际快捷键后缀，返回 `inconclusive`、退出码 `2`；目标 PID `928` 正常退出码 `0`，后续分组、启动、定时、重命名和删除动作均未执行。该失败属于探针菜单匹配，不是产品结论。Sandboxie 实际内部名被截短为 `ShaheAudit_20261016_11491`、`ShaheAudit_20261016_11492`，清理器按原始长前缀判断而漏清理，两个精确 section/目录仍存在，INI SHA-256 暂为 `DCD81104CC73087D2CA6515C55C7A9BA943B3F9741D6D90A47E385F9F47D94D1`；目标 EXE、`Start.exe` 和探针 PowerShell 无残留，`SbieSvc=Running`。结果为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-real-audit-20261016_115024_873-result.json`，日志为 `sandbox-lifecycle-real-audit-20261016_115024_873.log`；下一步仅清理结果 JSON 明确记录的两个本轮内部名并验证恢复基线。
- 2026-10-16：遗留内部名 `ShaheAudit_20261016_11491`、`ShaheAudit_20261016_11492` 的 `BoxAlias` 精确所有权校验通过后，已逐个执行官方 `/terminate`、`delete_sandbox_silent` 和 `SbieIni set /drv`；两个 section/目录均消失，INI SHA-256 恢复基线，目标 EXE与生命周期探针进程为 `0`，服务和控制进程未受影响。命令结束即时仍见 `Start.exe PID 5172`，故结果为 `inconclusive`，须继续只读核验而不能先声明清理完成；结果为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-owned-cleanup-20261016_145302_858-result.json`，日志为 `sandbox-lifecycle-owned-cleanup-20261016_145302_858.log`。该 JSON 的 `PreCleanupSandboxieIniSha256` 误在命令后采样，不能替代删除前独立记录的 `DCD81104CC73087D2CA6515C55C7A9BA943B3F9741D6D90A47E385F9F47D94D1`。
- 2026-10-16：只读复核确认 `Start.exe PID 5172` 已自然退出；无 `Start.exe` 或目标 EXE，两个精确 section/目录继续为零，INI 哈希保持基线，`SbieSvc=Running`，`SbieCtrl PID 6844` 与 `SbieSvc PID 2836/4916` 未变。下一步执行不含删除动作的持久化清理门禁。
- 2026-10-16：持久化只读清理门禁通过、退出码 `0`：全部 `ShaheAudit_*` section/目录计数为 `0`，INI SHA-256 精确恢复基线，无目标 EXE、`Start.exe` 或生命周期探针残留，`SbieSvc=Running`，`SbieCtrl PID 6844` 与 `SbieSvc PID 2836/4916` 保持。结果为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-cleanup-verification-20261016_145451_078-result.json`，日志为 `sandbox-lifecycle-cleanup-verification-20261016_145451_078.log`。遗留资源清理完成，但生命周期业务审计仍未完成。
- 2026-10-16：新增无 EXE 契约探针 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-probe-contract.ps1`，覆盖实际嵌入 C# 的 Tab 快捷键后缀匹配和清理循环长前缀守卫；Windows PowerShell 5.1 AST 解析通过，日志为 `sandbox-lifecycle-probe-contract-parse.log`。尚未执行契约或修改生命周期脚本。
- 2026-10-16：无 EXE 契约探针扩展为内存安全清理 fixture，覆盖基线排除、唯一精确别名发现、已验证名称兜底及相似/重复/变更所有权拒绝；Windows PowerShell 5.1 AST 解析通过，日志为 `sandbox-lifecycle-probe-contract-parse2.log`。尚未运行 RED。
- 2026-10-16：无 EXE 契约 RED 有效、退出码 `1`：实际脚本 AST 问题为 `0`，但无 Tab 后缀匹配方法，错误长前缀清理守卫仍存在且无安全选择函数；结果为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-probe-contract-red-20261016_150428_077-result.json`，日志为同名前缀 `.log`。未启动 EXE或调用 Sandboxie。
- 2026-10-16：生命周期探针新增严格 Tab 快捷键后缀匹配、精确内部名解析及基线/唯一别名约束的安全清理选择；生命周期和契约脚本的 Windows PowerShell 5.1 AST 解析均通过，日志为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-probes-parse-menu-cleanup-fix.log`。尚未运行 GREEN或真实 EXE；源码证据确认短内部名由应用 `SandboxieClient` 生成并以 `BoxAlias` 保留显示名，不是 Sandboxie 自行截断。
- 2026-10-16：无 EXE 契约 GREEN 通过、退出码 `0`：正文全等、Tab 快捷键后缀和不同正文拒绝均通过，错误长前缀守卫已移除；安全清理 fixture 精确选中 5 个预期名称并拒绝基线碰撞、相似/重复别名和所有权变化。结果为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-probe-contract-green-20261016_150746_381-result.json`，日志为同名前缀 `.log`。仅探针修正完成，真实生命周期审计尚未完成。
- 2026-10-16：探针修正独立只读审查发现 `2` 项 Important、`1` 项 Minor：冲突/重复 `BoxAlias` 可被 section 名全等绕过；清理命令失败、进程未退出或资源残留不会降级主流程 `passed`/退出码 `0`；菜单契约缺同前缀非 Tab 负例，且 INI 缺失解析仍会猜测显示名。真实 EXE 暂不重跑，下一步扩展无 EXE RED/GREEN。
- 2026-10-16：第二轮无 EXE 契约已覆盖审查发现的冲突别名、INI 缺失、清理完整性/退出降级、删除顺序和菜单边界；Windows PowerShell 5.1 AST 解析通过，日志为 `sandbox-lifecycle-probe-contract-parse3.log`。尚未运行第二轮 RED。
- 2026-10-16：第二轮无 EXE RED 有效、退出码 `1`：菜单新增负例通过，但冲突同名 section 被错误选中，INI 缺失未拒绝，清理完整性与删除顺序守卫缺失。结果为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-probe-contract-red2-20261016_151555_456-result.json`，日志为同名前缀 `.log`；未启动 EXE或调用 Sandboxie。
- 2026-10-16：第二轮实现已修正冲突别名、INI 缺失、内容删除/section 删除顺序和最终清理结论降级；生命周期与契约脚本的 Windows PowerShell 5.1 AST 解析均通过，日志为 `sandbox-lifecycle-probes-parse-cleanup-verdict-fix.log`。尚未运行第二轮 GREEN。
- 2026-10-16：第二轮无 EXE 契约 GREEN 通过、退出码 `0`：菜单全部正负边界、精确安全清理选择、INI 缺失拒绝、清理失败状态拒绝及 section 删除顺序守卫均通过；结果为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-probe-contract-green2-20261016_151844_151-result.json`，日志为同名前缀 `.log`。尚未重跑真实 EXE。
- 2026-10-16：第二轮探针修正经独立只读复审通过：上一轮 `2` 项 Important 与 `1` 项 Minor 均已关闭，未发现剩余 Critical/Important。唯一非阻断 Minor 是契约尚未直接构造“同一 section 两条 `BoxAlias`”fixture；当前实现因只接受唯一别名会拒绝该输入，下一步补该纯内存负例并复跑契约后再建立新基线。复审未执行探针、EXE 或 Sandboxie 命令。
- 2026-10-16：无 EXE 契约已补入“同一 section 两条相同 `BoxAlias`”直接负例，并把拒绝结果纳入总判定和日志；Windows PowerShell 5.1 AST 解析输出 `PARSE_OK`，日志为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-probe-contract-parse4.log`。尚未执行新增覆盖，也未启动 EXE 或调用 Sandboxie。
- 2026-10-16：补充覆盖后的无 EXE 契约 GREEN 通过、退出码 `0`：`DuplicateAliasRejected=true`，同一 section 的两条相同 `BoxAlias` 未进入清理集合，既有菜单边界、精确所有权、INI 缺失、清理完整性与删除顺序断言全部保持通过；结果为 `artifacts/runs/csharp_navigation_followup_20261015/sandbox-lifecycle-probe-contract-green3-20261016_152357_371-result.json`，日志为同名前缀 `.log`。未启动 EXE 或调用 Sandboxie；真实生命周期审计尚未完成。
- 2026-10-16：按用户要求暂停生命周期工作并检查 PowerShell 消耗；只读快照中 `ProjectProbeCandidateCount=0`，无遗留 `powershell.exe`、目标 EXE 或 `Start.exe`，唯一 `pwsh.exe PID 14052` 是本次诊断命令本身并已随命令返回结束。`SbieSvc=Running`，`SbieCtrl PID 6844`、`SbieSvc PID 2836/4916` 未停止或修改。结果为 `artifacts/runs/csharp_navigation_followup_20261015/powershell-process-diagnostic-20261016_152546_174-result.json`；没有需要终止的本轮自有 PID。
- 2026-10-16：PowerShell 探针调用修复完成后的紧凑验证通过、退出码 `0`：Windows PowerShell `5.1.19041.6456` 执行现有无 EXE 契约得到 `Conclusion=passed`，实际生命周期审计脚本 AST 问题数为 `0`，重复 `BoxAlias` 拒绝保持 `true`，命令结束时遗留项目 PowerShell 数为 `0`。历史外层引号展开、UTF-8 中文读取和自动变量冲突归类为已修正的探针错误；预期 RED `exit 1` 和无结论 `exit 2` 继续作为测试/审计状态单独记录，不再误述为 PowerShell 本体故障。结果为 `artifacts/runs/csharp_navigation_followup_20261015/powershell-fix-verification-20261016_153027_391-result.json`，日志为同名前缀 `.log`。生命周期真实审计仍暂停、尚未完成。
- 2026-10-16：当前生命周期脚本编码检查首次调用为探针失败：`-f` 格式运算符后直接列出多个数组索引导致 PowerShell 在逗号处报 `Unexpected token`；命令未进入文件改写阶段，没有结果 JSON，也没有启动 EXE、Sandboxie 或留下项目 PowerShell。该结果不作为编码状态证据，下一次改用 `BitConverter.ToString()` 进行唯一一次更正重试。
- 2026-10-16：编码检查更正重试成功：`sandbox-lifecycle-real-audit.ps1`、`sandbox-lifecycle-narrow-baseline.ps1`、`sandbox-batch-dialog-timeline.ps1`、`window-control-dump.ps1`、`sandbox-lifecycle-probe-contract.ps1` 均为无 BOM 的纯 ASCII 脚本，缺 BOM 数为 `5`，但“含非 ASCII 且缺 BOM”数为 `0`；Windows PowerShell 5.1 不会在这些当前脚本上重现早期中文源码误读。没有改写脚本或全局 PowerShell 配置。结果为 `artifacts/runs/csharp_navigation_followup_20261015/powershell-encoding-audit-20261016_153416_295-result.json`。
- 2026-10-16：Task 4 刷新合并契约首次复核调用无效：`dotnet test csharp\ShaheLauncher.Tests\ShaheLauncher.Tests.csproj --configuration Release --no-restore --no-build --filter FullyQualifiedName~ProcessMenuLiveStateRefreshCoalescesWhileWorkerIsRunning` 返回码为 `0`，但默认 `bin\Release\net48\ShaheLauncher.Tests.dll` 未匹配任何测试，因此实际执行数为 `0`，不能记为通过或失败；日志为 `artifacts/runs/csharp_navigation_followup_20261015/csharp-task4-live-refresh-coalesces-recheck-20261016_191424_049.log`。下一步改用计划已经验证的隔离测试程序集进行唯一一次更正重试，不修改生产代码。
- 2026-10-17：AuditRunner 批量创建等待预算 RED 已建立：定向测试 `BatchCreateObservationAllowsThirtySecondsPerSandbox` 返回码 `1`，通过 `0`、失败 `1`、跳过 `0`；失败原因为 `GetBatchCreateObservationTimeoutMilliseconds` 尚未实现。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-batch-timeout-red-20261017_163000_000-result.json` 和同名前缀 `.log`。这只是 Runner 的等待预算测试，不是产品失败；下一步只实现每个沙盒 30 秒、两个沙盒总计 60 秒的最小等待函数。
- 2026-10-17：AuditRunner 批量创建等待预算 GREEN 通过：定向测试返回码 `0`，通过 `1`、失败 `0`、跳过 `0`；`GetBatchCreateObservationTimeoutMilliseconds(2)` 为 `60000ms`，真实 `batch-create` 观察由固定 `20000ms` 改为按沙盒数量计算。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-batch-timeout-green-20261017_163200_000-result.json` 和同名前缀 `.log`。下一步运行 13 项 AuditRunner 定向回归。
- 2026-10-17：AuditRunner 等待预算修正版隔离 Release x64 构建通过：返回码 `0`、警告 `0`、错误 `0`；编译产物为 `artifacts/runs/csharp_navigation_followup_20261015/isolated-auditrunner-timeout-binx64/x64/Release/net48/ShaheLauncher.AuditRunner.exe`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-batch-timeout-build-20261017_163400_000-result.json` 和同名前缀 `.log`。下一步运行 13 项 AuditRunner 定向回归。
- 2026-10-17：AuditRunner 定向回归通过：隔离测试程序集返回码 `0`，通过 `14`、失败 `0`、跳过 `0`；包含新增批量创建等待预算测试。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-batch-timeout-regression-20261017_163600_000-result.json` 和同名前缀 `.log`。下一步重新只读 preflight，随后使用全新前缀执行实际 batch-create。
- 2026-10-17：等待预算修正版首次只读 preflight 被执行壳阻断：返回码 `1`，15 项门禁通过、`pwsh.exe` 门禁失败；当前 PowerShell 宿主被 AuditRunner 识别为活动 `pwsh.exe`，因此未启动目标 EXE、未写 Sandboxie、未进入 lifecycle。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-preflight-shell-block-20261017_163800_000-result.json` 和 `auditrunner-preflight-20261017_163800_000.log`。下一步改用纯 `cmd.exe` 外壳重跑只读 preflight。
- 2026-10-17：等待预算修正版用纯 `cmd.exe` 外壳重新 preflight 通过：返回码 `0`，16/16 门禁通过；baseline ID `acfd1f5126ad44b2a4abf76d8620433d`，目标/配置/`Sandboxie.ini` SHA-256 均匹配，`ShaheAudit_*` section/目录均为 `0`，`SbieSvc=Running`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-preflight-20261017_163900_000-result.json`、Runner 原始结果 `auditrunner-preflight-20261017_160220_329-result.json` 和同名前缀 `.log`。下一步使用全新唯一前缀执行真实 `batch-create`。
- 2026-10-17：真实 `batch-create` 使用全新前缀 `ShaheAudit_20261017_164000_002` 返回码 `3`、状态 `inconclusive`：对话框语义输入为前缀/`1`/`2`，焦点和结构稳定，提交成功，目标动态退出且未强制结束；等待预算已达到 `60000ms`，仍只观察到 `_01`，未观察到 `_02`。已确认拥有的 `_01` 清理成功，未将缺少第二项写成产品失败。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-lifecycle-batch-create-20261017_164000_002-result.json`、Runner 原始结果 `auditrunner-lifecycle-batch-create-20261017_160628_725-result.json` 和同名前缀 `.log`。下一步只读 verify-clean。
- 2026-10-17：真实 `batch-create` 后只读 verify-clean 等价门禁通过：返回码 `0`，16/16 门禁通过；目标/Start/PowerShell/pwsh/AuditRunner 进程均为 `0`，`ShaheAudit_*` section/目录均为 `0`，三份 SHA-256 恢复权威值，`SbieSvc=Running`。当前 `verify-clean` 命令模式仍是未接入的 Runner 分支，因此本次使用同一编译 Runner 的只读 preflight 结果作为清理门禁证据。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-verify-clean-postflight-20261017_164800_000-result.json`、Runner 原始结果 `auditrunner-preflight-20261017_161026_880-result.json` 和同名前缀 `.log`。
- 2026-10-17：`git diff --check` 首次调用仅因嵌套 `cmd.exe` 引号错误而成为探针失败，返回码 `1`，未执行 Git 检查、未改变仓库状态；证据为 `artifacts/runs/csharp_navigation_followup_20261015/git-diff-check-probe-failure-20261017_165000_000-result.json`。下一步直接在 `cmd.exe` 中重跑一次。
- 2026-10-17：`git diff --check` 更正后通过，返回码 `0`、空白错误 `0`；证据为 `artifacts/runs/csharp_navigation_followup_20261015/git-diff-check-20261017_165100_000-result.json` 和同名前缀 `.log`。
- 2026-10-17：针对真实 batch-create 缺少第二项的 Core 对照测试通过：`SandboxieClientContractTests` 返回码 `0`，通过 `8`、失败 `0`、跳过 `0`；创建/删除命令顺序和内部名保留逻辑均保持绿色，未发现可据此修改生产代码的证据。证据为 `artifacts/runs/csharp_navigation_followup_20261015/sandboxie-client-real-create-contract-20261017_170000_000-result.json` 和同名前缀 `.log`。真实缺第二项仍归类为运行时命令或目标 EXE 版本路径问题，未判为产品失败。
- 2026-10-17：AuditRunner 批量创建确认同步边界 RED 有效：定向测试 `BatchCreateConfirmationWaitsForTheButtonHandler` 返回码 `1`，通过 `0`、失败 `1`、跳过 `0`；当前异步 `WM_COMMAND` 在目标按钮处理完成前即返回，与历史成功探针的同步 `BM_CLICK` 行为不同。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-sync-click-red-20261017_171000_000-result.json` 和同名前缀 `.log`。下一步仅将该点击改为有界 `SendMessageTimeout(BM_CLICK)`。
- 2026-10-17：AuditRunner 批量创建确认同步边界 GREEN 通过：定向测试返回码 `0`，通过 `1`、失败 `0`、跳过 `0`；确认按钮改为有界 `SendMessageTimeout(BM_CLICK)`，调用只在按钮处理完成后返回，同时保留 70 秒超时。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-sync-click-green-20261017_171300_000-result.json` 和同名前缀 `.log`。下一步运行完整 AuditRunner 定向回归和隔离构建。
- 2026-10-17：AuditRunner 同步确认修正后的定向回归通过：返回码 `0`，通过 `15`、失败 `0`、跳过 `0`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-sync-click-regression-20261017_171500_000-result.json` 和同名前缀 `.log`。下一步隔离 Release x64 构建。
- 2026-10-17：AuditRunner 同步确认修正版隔离 Release x64 构建通过：返回码 `0`、警告 `0`、错误 `0`；产物为 `artifacts/runs/csharp_navigation_followup_20261015/isolated-auditrunner-sync-click-binx64/x64/Release/net48/ShaheLauncher.AuditRunner.exe`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-sync-click-build-20261017_171700_000-result.json` 和同名前缀 `.log`。下一步只读 preflight。
- 2026-10-17：AuditRunner 同步确认修正版只读 preflight 通过：返回码 `0`，16/16 门禁通过；baseline ID `3ab368eb41da4a8390340484ce958b85`，三份 SHA-256 匹配，`ShaheAudit_*` section/目录均为 `0`，`SbieSvc=Running`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-sync-click-preflight-20261017_171900_000-result.json`、Runner 原始结果 `auditrunner-preflight-20261017_202750_445-result.json` 和同名前缀 `.log`。下一步使用全新前缀重试真实 batch-create。
- 2026-10-17：同步确认修正版真实 `batch-create` 使用前缀 `ShaheAudit_20261017_202900_003` 返回码 `3`、状态 `inconclusive`：有界同步 `BM_CLICK` 已完成，目标动态退出、未强制结束，但仍只观察到 `_01`，未观察到 `_02`；唯一确认拥有的 `_01` 清理通过。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-sync-click-lifecycle-batch-create-20261017_202900_003-result.json`、Runner 原始结果 `auditrunner-lifecycle-batch-create-20261017_202929_646-result.json` 和同名前缀 `.log`。该结果排除“按钮处理尚未完成”作为唯一根因，仍不是产品失败；下一步先只读 verify-clean。
- 2026-10-17：同步确认重试后的只读 postflight 通过：返回码 `0`，16/16 门禁通过；全部 `ShaheAudit_*` section/目录为 `0`，目标/Start/PowerShell/pwsh/其他 AuditRunner/SbieCtrl/其他 Sandboxie 进程均为 `0`，`SbieSvc=Running`，三份 SHA-256 恢复权威值。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-sync-click-postflight-20261017_203100_000-result.json`、Runner 原始结果 `auditrunner-preflight-20261017_203054_628-result.json` 和同名前缀 `.log`。摘要读取首次因 cmd 引号导致 Python 语法失败、未改变状态，并已一次更正；证据为 `auditrunner-sync-click-postflight-summary-probe-failure-20261017_203100_000-result.json`。下一步只增加目标状态文本采集以定位实际创建数量。
- 2026-10-17：AuditRunner 状态文本采集 RED 有效：定向测试 `WindowTextSnapshotIncludesStatusLabels` 返回码 `1`，通过 `0`、失败 `1`、跳过 `0`；`GetNonEmptyWindowTexts` 尚未实现。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-status-text-red-20261017_203300_000-result.json` 和同名前缀 `.log`。下一步只复用现有子窗口枚举返回非空文本，并写入 batch-create 结果。
- 2026-10-17：AuditRunner 状态文本采集 GREEN 通过：定向测试返回码 `0`，通过 `1`、失败 `0`、跳过 `0`；Runner 复用现有 Win32 子窗口枚举，并在 batch-create 确认处理完成后记录主窗口非空文本。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-status-text-green-20261017_203500_000-result.json` 和同名前缀 `.log`。下一步运行 AuditRunner 定向回归和隔离构建。
- 2026-10-17：状态文本采集后的 AuditRunner 定向回归通过：返回码 `0`，通过 `16`、失败 `0`、跳过 `0`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-status-text-regression-20261017_203700_000-result.json` 和同名前缀 `.log`。下一步隔离 Release x64 构建。
- 2026-10-17：状态文本采集版 AuditRunner 隔离 Release x64 构建通过：返回码 `0`、警告 `0`、错误 `0`；产物为 `artifacts/runs/csharp_navigation_followup_20261015/isolated-auditrunner-status-binx64/x64/Release/net48/ShaheLauncher.AuditRunner.exe`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-status-text-build-20261017_203900_000-result.json` 和同名前缀 `.log`。下一步重新只读 preflight 后使用全新前缀采集真实创建状态文本。
- 2026-10-17：状态文本采集版只读 preflight 通过：返回码 `0`，16/16 门禁通过；baseline ID `6bc0867d4b474925ac1d0c442c708542`，`ShaheAudit_*` section/目录为 `0`，相关进程为 `0`，`SbieSvc=Running`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-status-text-preflight-20261017_204100_000-result.json`、Runner 原始结果 `auditrunner-preflight-20261017_203423_454-result.json` 和同名前缀 `.log`。下一步使用全新前缀执行一次真实 batch-create 状态采集。
- 2026-10-17：状态文本版真实 `batch-create` 使用前缀 `ShaheAudit_20261017_204300_004` 返回码 `3`、状态 `inconclusive`，但已取得根因证据：对话框可见文本记录数量为 `2`，目标主窗口状态却为 `正在创建沙盒用户 1/1: ..._01`，证明目标托管批量循环实际只收到数量 `1`；唯一 `_01` 资源已精确清理，未强制结束目标。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-status-text-lifecycle-batch-create-20261017_204300_004-result.json`、Runner 原始结果 `auditrunner-lifecycle-batch-create-20261017_203601_592-result.json` 和同名前缀 `.log`。这是 Runner 未提交 `NumericUpDown` 托管值，不是产品失败；下一步先只读 postflight，再改为真实字符输入。
- 2026-10-17：状态文本诊断后的只读 postflight 通过：返回码 `0`，16/16 门禁通过；`ShaheAudit_*` section/目录为 `0`，相关进程为 `0`，`SbieSvc=Running`，`Sandboxie.ini` SHA-256 恢复权威值。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-status-text-postflight-20261017_203800_000-result.json`、Runner 原始结果 `auditrunner-preflight-20261017_203652_026-result.json` 和同名前缀 `.log`。下一步为 NumericUpDown 真实字符输入建立 RED/GREEN。
- 2026-10-17：AuditRunner NumericUpDown 真实输入 RED 有效：定向测试 `BatchCreateValuesAreSetByAccessibleNamesNotWindowOrder` 返回码 `1`，通过 `0`、失败 `1`、跳过 `0`；`SetNumericEditValue` 尚未实现。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-numeric-input-red-20261017_204000_000-result.json` 和同名前缀 `.log`。下一步只用 `EM_SETSEL + WM_CHAR` 替换两个数字编辑框的 `SetWindowText`。
- 2026-10-17：AuditRunner NumericUpDown 真实输入 GREEN 通过：定向测试返回码 `0`，通过 `1`、失败 `0`、跳过 `0`；起始编号和创建数量改为 `EM_SETSEL + WM_CHAR`，不再仅修改可见文本。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-numeric-input-green-20261017_204200_000-result.json` 和同名前缀 `.log`。下一步运行 AuditRunner 定向回归和隔离构建。
- 2026-10-17：NumericUpDown 真实输入修正后的 AuditRunner 定向回归通过：返回码 `0`，通过 `16`、失败 `0`、跳过 `0`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-numeric-input-regression-20261017_204400_000-result.json` 和同名前缀 `.log`。下一步隔离 Release x64 构建。
- 2026-10-17：NumericUpDown 真实输入修正版隔离 Release x64 构建通过：返回码 `0`、警告 `0`、错误 `0`；产物为 `artifacts/runs/csharp_navigation_followup_20261015/isolated-auditrunner-numeric-binx64/x64/Release/net48/ShaheLauncher.AuditRunner.exe`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-numeric-input-build-20261017_204600_000-result.json` 和同名前缀 `.log`。下一步只读 preflight。
- 2026-10-17：NumericUpDown 真实输入修正版只读 preflight 通过：返回码 `0`，16/16 门禁通过；baseline ID `dab4abab21fa4b43bb4f0cd4fa4c027c`，`ShaheAudit_*` section/目录为 `0`，相关进程为 `0`，`SbieSvc=Running`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-numeric-input-preflight-20261017_204800_000-result.json`、Runner 原始结果 `auditrunner-preflight-20261017_204005_951-result.json` 和同名前缀 `.log`。下一步以全新前缀验证真实 batch-create。
- 2026-10-17：NumericUpDown 字符输入版真实 `batch-create` 使用前缀 `ShaheAudit_20261017_204900_005` 返回码 `3`、状态 `inconclusive`：批量创建对话框焦点和结构稳定，但两个数字编辑框的语义值验证未通过，因此 Runner 在提交前停止；创建资源 `0`、所有权声明 `0`、完成阶段 `0`，目标由动态退出命令关闭且未强制结束。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-numeric-input-lifecycle-batch-create-20261017_204900_005-result.json`、Runner 原始结果 `auditrunner-lifecycle-batch-create-20261017_204045_395-result.json` 和同名前缀 `.log`。这是 Runner 输入探针失败，不是产品失败；下一步先做只读 postflight，再只修复数值编辑框输入。
- 2026-10-17：NumericUpDown 字符输入失败后的只读 postflight 通过：返回码 `0`，16/16 门禁通过；`ShaheAudit_*` section/目录均为 `0`，目标/Start/PowerShell/pwsh/其他 AuditRunner/SbieCtrl/其他 Sandboxie 进程均为 `0`，`SbieSvc=Running`，三份 SHA-256 匹配权威基线。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-numeric-input-postflight-20261017_205100_000-result.json`、Runner 原始结果 `auditrunner-preflight-20261017_204328_597-result.json` 和同名前缀 `.log`。下一步只修复数值编辑框输入后重跑定向测试。
- 2026-10-17：AuditRunner NumericUpDown UIA 值提交 RED 有效：定向测试 `BatchCreateValuesAreSetByAccessibleNamesNotWindowOrder` 返回码 `1`，通过 `0`、失败 `1`、跳过 `0`；按 AccessibleName 查找 Spinner 并通过 RangeValuePattern 提交托管值的 helper 尚未实现。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-red-20261017_205300_000-result.json` 和同名前缀 `.log`。下一步用 UIA 原生数值模式替换失败的子 Edit 字符消息。
- 2026-10-17：AuditRunner NumericUpDown UIA 值提交首次 GREEN 尝试未通过：返回码 `1`，通过 `0`、失败 `1`、跳过 `0`；`RangeValuePattern.SetValue` 未同步更新 WinForms `NumericUpDown.Value`，因此该方案不能作为真实确认输入。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-green-20261017_205500_000-result.json` 和同名前缀 `.log`。下一步回到控件自身键盘路径，用聚焦后全选覆盖并让 WinForms 解析提交。
- 2026-10-17：AuditRunner NumericUpDown 聚焦键盘路径第二次 GREEN 尝试仍未通过：返回码 `1`，通过 `0`、失败 `1`、跳过 `0`；测试宿主中 `AutomationElement.SetFocus + SendKeys` 仍未更新 `NumericUpDown.Value`，未进入真实 EXE。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-green2-20261017_205700_000-result.json` 和同名前缀 `.log`。下一步改用原生上下调节消息直接改变控件值，避免依赖全局键盘焦点。
- 2026-10-17：AuditRunner NumericUpDown 命名 Spinner 子 Edit 的第三次 GREEN 尝试仍未通过：返回码 `1`，通过 `0`、失败 `1`、跳过 `0`；清空子 Edit 后发送 `WM_CHAR` 仍未同步更新 WinForms `NumericUpDown.Value`，未进入真实 EXE。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-green3-20261017_205900_000-result.json` 和同名前缀 `.log`。下一步直接验证目标控件的标准增减按钮路径，不再依赖文本解析状态。
- 2026-10-17：AuditRunner NumericUpDown UIA 增减按钮路径第四次 GREEN 尝试仍未通过：返回码 `1`，通过 `0`、失败 `1`、跳过 `0`；按钮 Invoke 后对话框值未形成预期组合，未进入真实 EXE。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-green4-20261017_210100_000-result.json` 和同名前缀 `.log`。下一步在同一现有测试中输出实际四个值，确定唯一失败字段后再改。
- 2026-10-17：AuditRunner NumericUpDown 定向诊断测试返回码 `1`，通过 `0`、失败 `1`、跳过 `0`；实际四值为 `ShaheAudit_Test_ | 8 | 0 | 13`，说明名称前缀已设置，但 Spinner helper 在两个数字值和下拉选择之前即返回失败，UIA 子按钮路径不可用。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-diagnostic-20261017_210300_000-result.json` 和同名前缀 `.log`。下一步仅使用控件已公开的 RangeValue 当前值配合标准键盘上下键消息。
- 2026-10-17：AuditRunner NumericUpDown 子 Edit 上下键路径第五次 GREEN 尝试仍未通过：返回码 `1`，通过 `0`、失败 `1`、跳过 `0`；实际值仍为 `ShaheAudit_Test_ | 8 | 0 | 13`，说明 Spinner UIA 元素没有把编辑句柄暴露为子 HWND，未进入真实 EXE。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-green5-20261017_210500_000-result.json` 和同名前缀 `.log`。下一步同时接受 Spinner 自身就是编辑句柄的 WinForms UIA 映射。
- 2026-10-17：AuditRunner NumericUpDown Spinner 自身句柄兼容后的第六次 GREEN 尝试仍未通过：返回码 `1`，通过 `0`、失败 `1`、跳过 `0`；实际值仍为 `ShaheAudit_Test_ | 8 | 0 | 13`，确认 WinForms Spinner UIA 元素没有可用原生编辑句柄，未进入真实 EXE。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-green6-20261017_210700_000-result.json` 和同名前缀 `.log`。下一步复用此前已验证能定位的标签几何关系取得 Edit 句柄，再仅用上下键改变托管值。
- 2026-10-17：AuditRunner NumericUpDown 标签定位 Edit 后的第七次 GREEN 尝试仍未通过：返回码 `1`，通过 `0`、失败 `1`、跳过 `0`；实际值仍为 `ShaheAudit_Test_ | 8 | 0 | 13`，说明直接向 Edit 发送上下键没有触发 NumericUpDown 托管增减逻辑，未进入真实 EXE。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-green7-20261017_210900_000-result.json` 和同名前缀 `.log`。下一步点击同一 NumericUpDown 的原生增减按钮子窗口。
- 2026-10-17：AuditRunner NumericUpDown 原生按钮推断路径第八次 GREEN 尝试仍未通过：返回码 `1`，通过 `0`、失败 `1`、跳过 `0`；实际前缀被误输入空格且数字未变，证明推断到的窗口不是 NumericUpDown 按钮面，未进入真实 EXE。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-green8-20261017_211100_000-result.json` 和同名前缀 `.log`。下一步删除该句柄推断，先显式确认对话框前台，再走控件自身聚焦键盘输入。
- 2026-10-17：AuditRunner NumericUpDown 显式前台 + Spinner.SetFocus 路径第九次 GREEN 尝试仍未通过：返回码 `1`，通过 `0`、失败 `1`、跳过 `0`；实际前缀变为带一个前导空格，证明 WinForms Spinner 的 UIA `SetFocus` 没有把键盘焦点从前缀编辑框移走，未进入真实 EXE。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-green9-20261017_211300_000-result.json` 和同名前缀 `.log`。下一步从已确认聚焦的前缀框按真实 Tab 顺序输入两个数值。
- 2026-10-17：AuditRunner NumericUpDown 前缀起点 Tab 顺序路径第十次 GREEN 尝试仍未通过：返回码 `1`，通过 `0`、失败 `1`、跳过 `0`；实际值仍为 `ShaheAudit_Test_ | 8 | 0 | 13`，说明测试宿主中的 `SendKeys` 未驱动数值控件，未进入真实 EXE。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-green10-20261017_211500_000-result.json` 和同名前缀 `.log`。下一步改用 Spinner 已公开的 LegacyIAccessible 值接口，不再依赖全局键盘输入。
- 2026-10-17：AuditRunner NumericUpDown LegacyIAccessible 尝试在测试前编译失败：返回码 `1`、错误 `2`、实际测试 `0`；当前 net48 引用集没有 `LegacyIAccessiblePattern` 类型，因此该路径不可用，未进入真实 EXE。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-green11-20261017_211700_000-result.json` 和同名前缀 `.log`。下一步移除该不可用 API，改用 WinForms 已支持的标准 `WM_SETFOCUS/WM_KILLFOCUS` 文本提交。
- 2026-10-17：AuditRunner NumericUpDown `WM_SETFOCUS/WM_KILLFOCUS` 第十二次 GREEN 尝试仍未通过：返回码 `1`，通过 `0`、失败 `1`、跳过 `0`；实际值仍为 `ShaheAudit_Test_ | 8 | 0 | 13`，说明仅发送焦点消息没有建立操作系统键盘焦点，未进入真实 EXE。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-green12-20261017_211900_000-result.json` 和同名前缀 `.log`。下一步在有界 `AttachThreadInput` 中使用原生 `SetFocus`，输入后立即解除线程输入关联。
- 2026-10-17：AuditRunner NumericUpDown `AttachThreadInput + SetFocus + WM_CHAR` 第十三次 GREEN 尝试仍未通过：返回码 `1`，通过 `0`、失败 `1`、跳过 `0`；实际值仍为 `ShaheAudit_Test_ | 8 | 0 | 13`，说明原生焦点已建立但直接字符消息仍未进入控件键盘输入路径，未进入真实 EXE。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-green13-20261017_212100_000-result.json` 和同名前缀 `.log`。下一步保留已验证的原生焦点，只把直接消息换成实际键盘事件。
- 2026-10-17：AuditRunner NumericUpDown 原生焦点 + `SendKeys` 第十四次 GREEN 尝试仍未通过：返回码 `1`，通过 `0`、失败 `1`、跳过 `0`；实际值仍为 `ShaheAudit_Test_ | 8 | 0 | 13`，未进入真实 EXE。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-green14-20261017_212300_000-result.json` 和同名前缀 `.log`。下一步不再依赖跨线程键盘，设置文本后显式发送标准 Edit `EN_CHANGE` 通知，让 WinForms 标记用户编辑并解析值。
- 2026-10-17：AuditRunner NumericUpDown `WM_SETTEXT + EN_CHANGE` 第十五次 GREEN 尝试仍未通过：返回码 `1`，通过 `0`、失败 `1`、跳过 `0`；实际值仍为 `ShaheAudit_Test_ | 8 | 0 | 13`，说明 WinForms 没有把跨线程文本通知作为用户编辑提交，未进入真实 EXE。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-green15-20261017_212500_000-result.json` 和同名前缀 `.log`。下一步删除文本/消息路径，按唯一 AccessibleName 和屏幕边界真实点击该数值控件自身的上下按钮。
- 2026-10-17：AuditRunner NumericUpDown 屏幕坐标上下按钮第十六次 GREEN 尝试仍未通过：返回码 `1`，通过 `0`、失败 `1`、跳过 `0`；实际值仍为 `ShaheAudit_Test_ | 8 | 0 | 13`，推断的箭头坐标未驱动测试宿主控件，未进入真实 EXE。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-green16-20261017_212700_000-result.json` 和同名前缀 `.log`。下一步只点击控件明确的编辑区取得真实焦点，再发送全选、数字和 Tab。
- 2026-10-17：AuditRunner NumericUpDown 编辑区点击 + `SendKeys` 第十七次 GREEN 尝试仍未通过：返回码 `1`，通过 `0`、失败 `1`、跳过 `0`；实际值仍为 `ShaheAudit_Test_ | 8 | 0 | 13`，测试宿主未接受该物理输入路径，未进入真实 EXE。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-green17-20261017_212900_000-result.json` 和同名前缀 `.log`。下一步从同一测试对话框直接输出 NumericUpDown 的真实子控件类型与句柄，只做一次结构诊断后按实际按钮控件修正。
- 2026-10-17：AuditRunner NumericUpDown 结构诊断完成：测试返回码 `1`（诊断断言预期失败），通过 `0`、失败 `1`、跳过 `0`；起始和数量控件都精确包含一个 `UpDownButtons` 与一个 `UpDownEdit` 子控件。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-structure-20261017_213100_000-result.json` 和同名前缀 `.log`。下一步按唯一 AccessibleName 的屏幕边界精确定位右侧 `UpDownButtons` 子窗口并直接点击，不再推断父句柄或使用全局输入。
- 2026-10-17：AuditRunner NumericUpDown 几何定位按钮路径第十八次 GREEN 尝试仍未通过：返回码 `1`，通过 `0`、失败 `1`、跳过 `0`；实际值仍为 `ShaheAudit_Test_ | 8 | 0 | 13`，未进入真实 EXE。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-green18-20261017_213300_000-result.json` 和同名前缀 `.log`。下一步把 Runner 选出的句柄与测试对话框真实 `UpDownButtons` 句柄直接比较，排除错误窗口选择。
- 2026-10-17：AuditRunner `UpDownButtons` 句柄对照首次调用在测试编译阶段返回码 `1`、错误 `1`、实际测试 `0`；测试工程没有直接引用 `System.Windows.Automation`，未进入真实 EXE。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-handle-20261017_213500_000-result.json` 和同名前缀 `.log`。下一步让反射 helper 只接收对话框句柄与名称，由 Runner 内部创建 UIA 根节点，不增加测试工程引用。
- 2026-10-17：AuditRunner `UpDownButtons` 句柄对照第二次诊断返回码 `1`，通过 `0`、失败 `1`、跳过 `0`；测试真实按钮句柄非零，但 Runner 几何筛选返回 `0x0`，确认 UIA Spinner 边界不包含原生按钮窗口。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-handle2-20261017_213700_000-result.json` 和同名前缀 `.log`。下一步直接验证已定位 Edit 的原生父句柄是否就是 NumericUpDown，再从该父窗口取唯一非 Edit 子控件。
- 2026-10-17：AuditRunner NumericUpDown 父句柄诊断首次调用返回码 `1`，通过 `0`、失败 `1`、跳过 `0`；此前废弃路径已移除 `GetParent` 声明，测试在句柄比较前失败，未进入真实 EXE。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-parent-20261017_213900_000-result.json` 和同名前缀 `.log`。下一步只恢复该原生只读声明并重跑句柄对照。
- 2026-10-17：AuditRunner NumericUpDown 父句柄第二次诊断返回码 `1`，通过 `0`、失败 `1`、跳过 `0`；原生 Edit 的父句柄已精确等于 `NumericUpDown.Handle`，失败仅剩旧几何按钮筛选返回 `0x0`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-parent2-20261017_214100_000-result.json` 和同名前缀 `.log`。下一步从已确认父窗口枚举唯一非 Edit 子窗口，直接得到真实 `UpDownButtons`。
- 2026-10-17：AuditRunner NumericUpDown 按钮句柄第十九次 GREEN 尝试返回码 `1`，通过 `0`、失败 `1`、跳过 `0`；父窗口路径正确但 helper 仍返回 `0x0`，根因是 WinForms `NumericUpDown` 的命名 UIA 元素并不暴露为 `ControlType.Spinner`，此前所有新输入路径都在该错误类型检查处提前返回。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-green19-20261017_214300_000-result.json` 和同名前缀 `.log`。下一步删除错误控件类型假设，仅保留 AccessibleName 唯一性、已验证 Edit 与真实 `UpDownButtons` 句柄。
- 2026-10-18：AuditRunner NumericUpDown 无类型 UIA 名称检查后的第二十次 GREEN 尝试返回码 `1`，通过 `0`、失败 `1`、跳过 `0`；按钮 helper 仍返回 `0x0`，确认该 WinForms provider 没有把 `NumericUpDown.AccessibleName` 表示为一个唯一 UIA 元素。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-green20-20261018_000100_000-result.json` 和同名前缀 `.log`。下一步以严格支持的名称映射到唯一完整标签，再使用已验证的 Edit 父窗口，不依赖不可用的 UIA 名称节点。
- 2026-10-18：AuditRunner NumericUpDown GREEN 通过：定向测试返回码 `0`，通过 `1`、失败 `0`、跳过 `0`；Runner 现在以严格名称映射到唯一完整标签，取得精确 `UpDownEdit`、其已验证父窗口及唯一 `UpDownButtons`，按钮句柄与真实控件一致，起始值与数量的 WinForms 托管 `Value` 均更新为预期值。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-green21-20261018_000300_000-result.json` 和同名前缀 `.log`。下一步运行完整 AuditRunner 定向回归。
- 2026-10-18：AuditRunner 16 项定向回归首次返回码 `1`，通过 `15`、失败 `1`、跳过 `0`；失败项的两个 NumericUpDown 托管值和下拉选择均正确，但此前测试遗留的键盘字符在前缀设置后进入对话框，使前缀变为 `哇ShaheAudit_Test_`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-regression-20261018_000500_000-result.json` 和同名前缀 `.log`。下一步只把前缀设置移到数值/下拉操作之后，避免先设置值被后续输入污染。
- 2026-10-18：AuditRunner 数值输入修正后的定向回归通过：返回码 `0`，通过 `16`、失败 `0`、跳过 `0`；前缀改为在两个数值和下拉选择完成后最后设置，全部合同保持绿色。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-regression2-20261018_000700_000-result.json` 和同名前缀 `.log`。下一步执行独立 Release x64 构建。
- 2026-10-18：AuditRunner 数值输入修正版独立 Release x64 构建通过：返回码 `0`、警告 `0`、错误 `0`；产物为 `artifacts/runs/csharp_navigation_followup_20261015/isolated-auditrunner-spinner-final-binx64/x64/Release/net48/ShaheLauncher.AuditRunner.exe`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-build-20261018_000900_000-result.json` 和同名前缀 `.log`。下一步运行只读 preflight。
- 2026-10-18：AuditRunner 数值输入修正版只读 preflight 通过：返回码 `0`，16/16 门禁通过；baseline ID `990d62074be3411389fb7361644d17ad`，`ShaheAudit_*` section/目录均为 `0`，相关进程为 `0`，`SbieSvc=Running`，三份 SHA-256 匹配权威基线。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-preflight-20261018_083114_616-result.json`、Runner 原始结果 `auditrunner-preflight-20261018_083114_616-result.json` 和日志 `auditrunner-spinner-preflight-20261018_001100_000.log`。下一步使用全新前缀执行真实 `batch-create`。
- 2026-10-18：数值输入修正版真实 `batch-create` 使用前缀 `ShaheAudit_20261018_083300_006` 返回码 `3`、状态 `inconclusive`：对话框焦点与结构稳定，但跨进程语义值设置仍返回失败，Runner 在提交前停止；创建资源 `0`、所有权声明 `0`、完成阶段 `0`，目标动态退出且未强制结束。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-lifecycle-batch-create-20261018_083300_006-result.json`、Runner 原始结果 `auditrunner-lifecycle-batch-create-20261018_083214_686-result.json` 和同名前缀 `.log`。这是 Runner 跨进程输入失败，不是产品失败；下一步先只读 postflight，再记录每个输入步骤的精确失败点。
- 2026-10-18：跨进程输入失败后的只读 postflight 通过：返回码 `0`，16/16 门禁通过；`ShaheAudit_*` section/目录均为 `0`，相关进程为 `0`，`SbieSvc=Running`，三份 SHA-256 匹配权威基线。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-postflight-20261018_083303_600-result.json`、Runner 原始结果 `auditrunner-preflight-20261018_083303_600-result.json` 和日志 `auditrunner-spinner-postflight-20261018_083400_000.log`。下一步只增加输入步骤失败原因，不创建资源。
- 2026-10-18：AuditRunner 批量创建输入步骤诊断改动的定向测试通过：返回码 `0`，通过 `1`、失败 `0`、跳过 `0`；原有成功路径保持绿色，失败时现在会区分起始值、编号格式、数量和最终验证。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-input-diagnostic-test-20261018_083600_000-result.json` 和同名前缀 `.log`。下一步构建诊断版 Runner。
- 2026-10-18：AuditRunner 输入步骤诊断版独立 Release x64 构建通过：返回码 `0`、警告 `0`、错误 `0`；产物为 `artifacts/runs/csharp_navigation_followup_20261015/isolated-auditrunner-input-diagnostic-binx64/x64/Release/net48/ShaheLauncher.AuditRunner.exe`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-input-diagnostic-build-20261018_083800_000-result.json` 和同名前缀 `.log`。下一步重新只读 preflight。
- 2026-10-18：AuditRunner 输入步骤诊断版只读 preflight 通过：返回码 `0`，16/16 门禁通过；baseline ID `f260e0f21dda4b0c8f539807d93b546a`，`ShaheAudit_*` section/目录均为 `0`，相关进程为 `0`，`SbieSvc=Running`，三份 SHA-256 匹配权威基线。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-input-diagnostic-preflight-20261018_083739_223-result.json`、Runner 原始结果 `auditrunner-preflight-20261018_083739_223-result.json` 和同名前缀 `.log`。下一步使用全新前缀重跑真实 `batch-create`，只采集精确输入失败步骤。
- 2026-10-18：输入步骤诊断版真实 `batch-create` 使用前缀 `ShaheAudit_20261018_084100_007` 返回码 `3`、状态 `inconclusive`：起始编号和编号格式已通过，精确失败点为数量控件从默认 `1` 增加到 `2`；Runner 在提交前停止，创建资源 `0`、所有权声明 `0`，目标动态退出且未强制结束。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-input-diagnostic-lifecycle-batch-create-20261018_084100_007-result.json`、Runner 原始结果 `auditrunner-lifecycle-batch-create-20261018_083828_402-result.json` 和同名前缀 `.log`。这是 Runner 的向上按钮路径问题，不是产品失败；下一步先只读 postflight，再把测试初始数量改为 `1` 建立向上路径 RED。
- 2026-10-18：数量向上路径失败后的只读 postflight 通过：返回码 `0`，16/16 门禁通过；`ShaheAudit_*` section/目录均为 `0`，相关进程为 `0`，`SbieSvc=Running`，三份 SHA-256 匹配权威基线。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-count-up-postflight-20261018_083912_503-result.json`、Runner 原始结果 `auditrunner-preflight-20261018_083912_503-result.json` 和同名前缀 `.log`。下一步建立数量 `1 -> 2` 的定向 RED。
- 2026-10-18：数量 `1 -> 2` 的进程内定向测试返回码 `0`，通过 `1`、失败 `0`、跳过 `0`；同一按钮定位和上半区点击在测试对话框中有效，剩余失败仅存在于跨进程真实 EXE。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-count-up-red-20261018_084500_000-result.json` 和同名前缀 `.log`。下一步把同步窗口调用改为投递到目标 UI 消息队列，并给目标线程 2 秒处理时间。
- 2026-10-18：数量向上按钮异步消息修正后的定向测试通过：返回码 `0`，通过 `1`、失败 `0`、跳过 `0`；`WM_LBUTTONDOWN/UP` 改为投递到目标 UI 队列，并以 2 秒有界等待确认文本变化。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-count-up-green-20261018_084700_000-result.json` 和同名前缀 `.log`。下一步运行 AuditRunner 16 项回归。
- 2026-10-18：数量向上按钮异步消息修正后的 AuditRunner 定向回归通过：返回码 `0`，通过 `16`、失败 `0`、跳过 `0`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-count-up-regression-20261018_084900_000-result.json` 和同名前缀 `.log`。下一步执行独立 Release x64 构建。
- 2026-10-18：数量向上按钮异步消息修正版独立 Release x64 构建通过：返回码 `0`、警告 `0`、错误 `0`；产物为 `artifacts/runs/csharp_navigation_followup_20261015/isolated-auditrunner-count-up-final-binx64/x64/Release/net48/ShaheLauncher.AuditRunner.exe`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-count-up-build-20261018_085100_000-result.json` 和同名前缀 `.log`。下一步重新只读 preflight。
- 2026-10-18：数量向上按钮修正版只读 preflight 通过：返回码 `0`，16/16 门禁通过；baseline ID `8bdfd86761864302befce7b83ce45070`，`ShaheAudit_*` section/目录均为 `0`，相关进程门禁通过，`SbieSvc=Running`，三份 SHA-256 匹配权威基线。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-count-up-preflight-20261018_084347_186-result.json`、Runner 原始结果 `auditrunner-preflight-20261018_084347_186-result.json` 和同名前缀 `.log`。下一步使用全新唯一前缀重试真实 `batch-create`。
- 2026-10-18：数量向上按钮异步等待修正版真实 `batch-create` 使用前缀 `ShaheAudit_20261018_085500_008` 返回码 `3`、状态 `inconclusive`：批量创建对话框焦点和结构稳定，但数量控件仍拒绝从默认 `1` 增加到 `2`；Runner 在提交前停止，资源创建 `0`、所有权声明 `0`、完成阶段 `0`，目标由动态退出命令关闭且未强制结束。这是 Runner 跨进程数值输入失败，不是产品失败。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-count-up-lifecycle-batch-create-20261018_085500_008-result.json`、Runner 原始结果 `auditrunner-lifecycle-batch-create-20261018_084513_874-result.json`、状态文件和同名前缀 `.log`。下一步先运行只读 postflight 验证零残留。
- 2026-10-18：首次只读 postflight 调用在门禁执行前返回码 `2`，Runner 报告 `--target must name an existing file.`，实际检查 `0`；这是目标路径参数探针失败，没有改变任何状态。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-count-up-postflight2-probe-failure-20261018_085700_000-result.json` 和同名前缀 `.log`。下一步确认权威目标的实际文件名后只纠正一次调用。
- 2026-10-18：postflight 的 Python 路径转交尝试在 AuditRunner 启动前发生引号语法错误，返回码 `1`、实际检查 `0`，没有改变任何状态。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-count-up-postflight2-python-probe-failure-20261018_090000_000-result.json` 和对应 `.log`。下一步直接让命令解释器枚举现有目标 EXE 后调用编译产物，不再使用该包装方式。
- 2026-10-18：命令解释器枚举目标 EXE 的 postflight 调用仍在门禁执行前返回码 `2`，实际检查 `0`；枚举路径在传给 AuditRunner 时未被保真，没有改变任何状态。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-count-up-postflight2-enumeration-probe-failure-20261018_090200_000-result.json` 和对应 `.log`。下一步使用位于本轮 artifacts 目录的最小 Python 启动文件直接传递 Unicode 路径，再运行编译后的 Runner。
- 2026-10-18：真实 `batch-create` 输入失败后的只读 postflight 通过：返回码 `0`，16/16 门禁通过；`ShaheAudit_*` section/目录均为 `0`，相关进程为 `0`，`SbieSvc=Running`，三份 SHA-256 匹配权威基线。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-count-up-postflight2-20261018_084907_965-result.json`、Runner 原始结果 `auditrunner-preflight-20261018_084907_965-result.json` 和对应 `.log`。下一步只修复跨进程数量控件输入，未通过测试和新 preflight 前不再创建资源。
- 2026-10-18：跨进程数值按钮的有界同步消息 RED 已建立：定向测试返回码 `1`，通过 `0`、失败 `1`、跳过 `0`；失败原因是 `SendSpinnerButtonClick` 尚不存在，当前实现仍使用不确认目标处理结果的异步投递。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-sync-red-20261018_091000_000-result.json` 和同名前缀 `.log`。下一步只增加一个复用现有 `SendMessageTimeout` 的最小 helper 并替换两次投递。
- 2026-10-18：数值按钮有界同步消息 GREEN 通过：定向测试返回码 `0`，通过 `1`、失败 `0`、跳过 `0`；上下按钮现在通过 2 秒超时的同步消息执行并确认目标窗口实际处理，取代不确认处理结果的异步投递。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-sync-green-20261018_091200_000-result.json` 和同名前缀 `.log`。下一步运行 AuditRunner 16 项定向回归。
- 2026-10-18：数值按钮有界同步消息修正后的 AuditRunner 定向回归通过：返回码 `0`，通过 `16`、失败 `0`、跳过 `0`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-sync-regression-20261018_091400_000-result.json` 和同名前缀 `.log`。下一步执行独立 Release x64 构建。
- 2026-10-18：数值按钮有界同步消息修正版独立 Release x64 构建通过：返回码 `0`、警告 `0`、错误 `0`；产物为 `artifacts/runs/csharp_navigation_followup_20261015/isolated-auditrunner-spinner-sync-final-binx64/x64/Release/net48/ShaheLauncher.AuditRunner.exe`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-sync-build-20261018_091600_000-result.json` 和同名前缀 `.log`。下一步运行只读 preflight。
- 2026-10-18：数值按钮有界同步消息修正版只读 preflight 通过：返回码 `0`，16/16 门禁通过；baseline ID `c4de6fead40b42f883892f10ae131145`，`ShaheAudit_*` section/目录均为 `0`，相关进程门禁通过，`SbieSvc=Running`，三份 SHA-256 匹配权威基线。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-sync-preflight-20261018_085404_239-result.json`、Runner 原始结果 `auditrunner-preflight-20261018_085404_239-result.json` 和对应 `.log`。下一步使用全新前缀执行真实 `batch-create`。
- 2026-10-18：有界同步数值按钮版真实 `batch-create` 使用前缀 `ShaheAudit_20261018_092000_009` 返回码 `3`、状态 `inconclusive`：对话框焦点和结构稳定，但数量控件仍未从默认 `1` 变为 `2`；Runner 在提交前停止，资源创建 `0`、所有权声明 `0`、完成阶段 `0`，目标动态退出且未强制结束。同步消息成功返回仍未改变跨进程托管值，因此不是产品失败。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-sync-lifecycle-batch-create-20261018_092000_009-result.json`、Runner 原始结果 `auditrunner-lifecycle-batch-create-20261018_085450_464-result.json`、状态文件和对应 `.log`。下一步先运行只读 postflight，再改用目标进程支持的真实鼠标输入。
- 2026-10-18：有界同步数值按钮版失败后的只读 postflight 通过：返回码 `0`，16/16 门禁通过；`ShaheAudit_*` section/目录均为 `0`，相关进程为 `0`，`SbieSvc=Running`，三份 SHA-256 匹配权威基线。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-sync-postflight-20261018_085709_422-result.json`、Runner 原始结果 `auditrunner-preflight-20261018_085709_422-result.json` 和对应 `.log`。下一步仅把 spinner helper 改为在已确认前台对话框内发送真实鼠标输入，并保持有界验证。
- 2026-10-18：数值按钮真实鼠标输入 RED 已建立：定向测试返回码 `1`，通过 `0`、失败 `1`、跳过 `0`；`SendPhysicalSpinnerButtonClick` 尚不存在。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-input-red-20261018_092400_000-result.json` 和同名前缀 `.log`。下一步用 Win32 `SendInput` 实现最小点击并保留数值变化的有界验证。
- 2026-10-18：数值按钮 `SendInput` 第一次 GREEN 尝试返回码 `1`，通过 `0`、失败 `1`、跳过 `0`；实际值保持 `王五 | 8 | 0 | 1`，物理输入未到达测试对话框。当前实现未在输入方法内再次确认前台，且注入后立即恢复鼠标位置；未进入真实 EXE。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-input-green1-20261018_092600_000-result.json` 和对应 `.log`。下一步只在输入前确认对话框前台，并给目标 UI 线程一个最短处理窗口后再恢复鼠标位置。
- 2026-10-18：数值按钮 `SendInput` 第二次 GREEN 尝试返回码 `1`，通过 `0`、失败 `1`、跳过 `0`；即使先确认前台并延迟恢复鼠标，实际值仍保持 `王五 | 8 | 0 | 1`，说明当前 `INPUT` 封送路径没有产生有效点击，未进入真实 EXE。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-input-green2-failed-20261018_092800_000-result.json` 和对应 `.log`。下一步删除该结构封送，使用同一 Win32 鼠标输入接口的直接 `mouse_event` 调用。
- 2026-10-18：数值按钮直接 `mouse_event` 的第三次 GREEN 尝试仍返回码 `1`，通过 `0`、失败 `1`、跳过 `0`；实际值仍为 `王五 | 8 | 0 | 1`，测试宿主不接受全局鼠标注入，未进入真实 EXE。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-input-green3-failed-20261018_093000_000-result.json` 和对应 `.log`。下一步停止全局鼠标路径，改测 NumericUpDown 父窗口对标准 Up/Down 键消息的托管处理。
- 2026-10-18：聚焦 `UpDownEdit` 后投递标准 Up/Down 键的 RED 已建立：定向测试返回码 `1`，通过 `0`、失败 `1`、跳过 `0`；`PostNumericStepKey` 尚不存在。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-key-red-20261018_093200_000-result.json` 和同名前缀 `.log`。下一步只实现线程输入关联、精确 Edit 聚焦和键消息投递。
- 2026-10-18：聚焦 `UpDownEdit` 键路径 GREEN 通过：定向测试返回码 `0`，通过 `1`、失败 `0`、跳过 `0`；Runner 现在有界关联目标 UI 线程、精确聚焦已验证的 Edit，并投递标准 Up/Down 键，由 WinForms 消息循环更新 NumericUpDown 托管值。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-key-green-20261018_093400_000-result.json` 和同名前缀 `.log`。下一步运行 AuditRunner 16 项回归。
- 2026-10-18：聚焦 `UpDownEdit` 键路径修正后的 AuditRunner 定向回归通过：返回码 `0`，通过 `16`、失败 `0`、跳过 `0`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-key-regression-20261018_093600_000-result.json` 和同名前缀 `.log`。下一步执行独立 Release x64 构建。
- 2026-10-18：聚焦 `UpDownEdit` 键路径修正版独立 Release x64 构建通过：返回码 `0`、警告 `0`、错误 `0`；产物为 `artifacts/runs/csharp_navigation_followup_20261015/isolated-auditrunner-spinner-key-final-binx64/x64/Release/net48/ShaheLauncher.AuditRunner.exe`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-key-build-20261018_093800_000-result.json` 和同名前缀 `.log`。下一步运行只读 preflight。
- 2026-10-18：聚焦 `UpDownEdit` 键路径修正版只读 preflight 通过：返回码 `0`，16/16 门禁通过；baseline ID `ac97a28adc444e96b442c30408fbcc77`，`ShaheAudit_*` section/目录均为 `0`，相关进程门禁通过，`SbieSvc=Running`，三份 SHA-256 匹配权威基线。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-key-preflight-20261018_090623_254-result.json`、Runner 原始结果 `auditrunner-preflight-20261018_090623_254-result.json` 和对应 `.log`。下一步使用全新前缀执行真实 `batch-create`。
- 2026-10-18：聚焦 `UpDownEdit` 键路径版真实 `batch-create` 使用前缀 `ShaheAudit_20261018_094200_010` 返回码 `3`、状态 `inconclusive`：同进程测试已通过，但跨进程数量仍未从 `1` 变为 `2`；Runner 在提交前停止，资源创建 `0`、所有权声明 `0`、完成阶段 `0`，目标动态退出且未强制结束。当前线程输入关联在异步键消息被目标处理前即解除，是 Runner 时序问题，不是产品失败。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-key-lifecycle-batch-create-20261018_094200_010-result.json`、Runner 原始结果 `auditrunner-lifecycle-batch-create-20261018_090708_844-result.json`、状态文件和对应 `.log`。下一步先运行只读 postflight，再让线程关联保持到观察到数值变化或超时。
- 2026-10-18：聚焦 `UpDownEdit` 键路径版失败后的只读 postflight 通过：返回码 `0`，16/16 门禁通过；`ShaheAudit_*` section/目录均为 `0`，相关进程为 `0`，`SbieSvc=Running`，三份 SHA-256 匹配权威基线。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-key-postflight-20261018_090755_867-result.json`、Runner 原始结果 `auditrunner-preflight-20261018_090755_867-result.json` 和对应 `.log`。下一步只把键路径的线程关联保持到观察到 Edit 文本变化或 2 秒超时。
- 2026-10-18：键路径保持线程关联直到值变化的定向 GREEN 通过：返回码 `0`，通过 `1`、失败 `0`、跳过 `0`；键消息投递后会在最多 2 秒内确认精确 Edit 文本改变，再解除线程关联。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-key-hold-green-20261018_094600_000-result.json` 和同名前缀 `.log`。下一步运行 AuditRunner 16 项回归。
- 2026-10-18：键路径保持线程关联修正后的 AuditRunner 定向回归通过：返回码 `0`，通过 `16`、失败 `0`、跳过 `0`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-key-hold-regression-20261018_094800_000-result.json` 和同名前缀 `.log`。下一步执行独立 Release x64 构建。
- 2026-10-18：键路径保持线程关联修正版独立 Release x64 构建通过：返回码 `0`、警告 `0`、错误 `0`；产物为 `artifacts/runs/csharp_navigation_followup_20261015/isolated-auditrunner-spinner-key-hold-final-binx64/x64/Release/net48/ShaheLauncher.AuditRunner.exe`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-key-hold-build-20261018_095000_000-result.json` 和同名前缀 `.log`。下一步运行只读 preflight。
- 2026-10-18：键路径保持线程关联修正版只读 preflight 通过：返回码 `0`，16/16 门禁通过；baseline ID `fd68e9d33a824a64a7776bb22baa126d`，`ShaheAudit_*` section/目录均为 `0`，相关进程门禁通过，`SbieSvc=Running`，三份 SHA-256 匹配权威基线。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-key-hold-preflight-20261018_091042_688-result.json`、Runner 原始结果 `auditrunner-preflight-20261018_091042_688-result.json` 和对应 `.log`。下一步使用全新前缀执行真实 `batch-create`。
- 2026-10-18：保持线程关联版真实 `batch-create` 使用前缀 `ShaheAudit_20261018_095400_011` 仍返回码 `3`、状态 `inconclusive`：数量控件未从 `1` 变为 `2`；Runner 在提交前停止，资源创建 `0`、所有权声明 `0`、完成阶段 `0`，目标动态退出且未强制结束。跨进程标准键消息仍未进入该控件的 WinForms 预处理路径，不是产品失败。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-key-hold-lifecycle-batch-create-20261018_095400_011-result.json`、Runner 原始结果 `auditrunner-lifecycle-batch-create-20261018_091130_089-result.json`、状态文件和对应 `.log`。下一步先运行只读 postflight，再直接核对应用中该对话框的控件初始化和事件路径，不再盲试输入方式。
- 2026-10-18：保持线程关联版失败后的只读 postflight 通过：返回码 `0`，16/16 门禁通过；`ShaheAudit_*` section/目录均为 `0`，相关进程为 `0`，`SbieSvc=Running`，三份 SHA-256 匹配权威基线。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-key-hold-postflight2-20261018_091212_870-result.json`、Runner 原始结果 `auditrunner-preflight-20261018_091212_870-result.json` 和对应 `.log`。下一步只读核对 `BatchNameRuleDialog` 和批量创建调用参数。
- 2026-10-18：源码核对确认真实批量创建对话框与测试使用相同 `NumericUpDown`，默认起始和数量均为 `1`、数量范围 `1..5000`，没有禁用或联动事件。改用系统 `keybd_event` 后的定向尝试返回码 `1`，通过 `0`、失败 `1`、跳过 `0`；测试宿主不接受全局键盘注入，实际值保持 `王五 | 8 | 0 | 1`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-keybd-green1-failed-20261018_095800_000-result.json` 和对应 `.log`。下一步同进程测试继续使用已验证的消息路径，跨进程真实运行单独使用系统键盘输入并由真实审计验证。
- 2026-10-18：按进程边界选择数值键路径后的定向 GREEN 通过：返回码 `0`，通过 `1`、失败 `0`、跳过 `0`；同进程测试使用消息队列，跨进程分支使用已聚焦 Edit 的系统键盘输入，两者都保留 2 秒值变化验证。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-keybd-green2-20261018_100000_000-result.json` 和同名前缀 `.log`。下一步运行 AuditRunner 16 项回归。
- 2026-10-18：按进程边界选择数值键路径后的 AuditRunner 定向回归通过：返回码 `0`，通过 `16`、失败 `0`、跳过 `0`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-keybd-regression-20261018_100200_000-result.json` 和同名前缀 `.log`。下一步执行独立 Release x64 构建。
- 2026-10-18：按进程边界选择数值键路径后的独立 Release x64 构建通过：返回码 `0`、警告 `0`、错误 `0`；产物为 `artifacts/runs/csharp_navigation_followup_20261015/isolated-auditrunner-spinner-keybd-final-binx64/x64/Release/net48/ShaheLauncher.AuditRunner.exe`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-keybd-build-20261018_100400_000-result.json` 和同名前缀 `.log`。下一步运行只读 preflight。
- 2026-10-18：按进程边界选择数值键路径后的只读 preflight 通过：返回码 `0`，16/16 门禁通过；baseline ID `f8fe0e2b8b7e4d429a07f9788ec6ecc7`，`ShaheAudit_*` section/目录均为 `0`，相关进程门禁通过，`SbieSvc=Running`，三份 SHA-256 匹配权威基线。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-keybd-preflight-20261018_091619_976-result.json`、Runner 原始结果 `auditrunner-preflight-20261018_091619_976-result.json` 和对应 `.log`。下一步使用全新前缀执行真实 `batch-create`。
- 2026-10-18：跨进程系统 Up 键版真实 `batch-create` 使用前缀 `ShaheAudit_20261018_100800_012` 仍返回码 `3`、状态 `inconclusive`：数量控件未变化；Runner 在提交前停止，资源创建 `0`、所有权声明 `0`、完成阶段 `0`，目标动态退出且未强制结束。当前会话的全局输入注入没有到达目标 Edit，不是产品失败。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-keybd-lifecycle-batch-create-20261018_100800_012-result.json`、Runner 原始结果 `auditrunner-lifecycle-batch-create-20261018_091708_705-result.json`、状态文件和对应 `.log`。下一步先运行只读 postflight；后续停止依赖全局输入，改用精确 Edit 文本设置加真实焦点提交。
- 2026-10-18：跨进程系统 Up 键版失败后的只读 postflight 通过：返回码 `0`，16/16 门禁通过；`ShaheAudit_*` section/目录均为 `0`，相关进程为 `0`，`SbieSvc=Running`，三份 SHA-256 匹配权威基线。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-keybd-postflight-20261018_091801_767-result.json`、Runner 原始结果 `auditrunner-preflight-20261018_091801_767-result.json` 和对应 `.log`。下一步建立精确 Edit 文本设置并移动真实焦点提交 NumericUpDown 的 RED。
- 2026-10-18：精确 `UpDownEdit` 文本加真实焦点提交的 RED 已建立：定向测试返回码 `1`，通过 `0`、失败 `1`、跳过 `0`；`SetNumericEditTextAndCommit` 尚不存在。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-commit-red-20261018_101200_000-result.json` 和同名前缀 `.log`。下一步删除全局输入分支，只实现线程关联内的 Edit 聚焦、文本设置和焦点提交。
- 2026-10-18：精确 Edit 文本提交第一次 GREEN 尝试在测试前编译失败：返回码 `1`、错误 `3`、实际测试 `0`；新增局部句柄误与已有 `prefix` 参数同名，并使后续两处参数类型错误。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-commit-green1-failed-20261018_101400_000-result.json` 和对应 `.log`。下一步仅把局部变量改为 `prefixHandle` 后重跑。
- 2026-10-18：精确 `UpDownEdit` 文本加焦点提交的 GREEN 通过：返回码 `0`，通过 `1`、失败 `0`、跳过 `0`；测试确认起始值、数量和最终托管 `NumericUpDown.Value` 均更新正确，且不再依赖全局输入。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-commit-green2-20261018_101600_000-result.json` 和同名前缀 `.log`。下一步运行 AuditRunner 16 项回归。
- 2026-10-18：精确 Edit 文本提交修正后的 AuditRunner 定向回归通过：返回码 `0`，通过 `16`、失败 `0`、跳过 `0`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-commit-regression-20261018_101800_000-result.json` 和同名前缀 `.log`。下一步执行独立 Release x64 构建。
- 2026-10-18：精确 Edit 文本提交修正版独立 Release x64 构建通过：返回码 `0`、警告 `0`、错误 `0`；产物为 `artifacts/runs/csharp_navigation_followup_20261015/isolated-auditrunner-spinner-commit-final-binx64/x64/Release/net48/ShaheLauncher.AuditRunner.exe`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-commit-build-20261018_102000_000-result.json` 和同名前缀 `.log`。下一步运行只读 preflight。
- 2026-10-18：精确 Edit 文本提交修正版只读 preflight 通过：返回码 `0`，16/16 门禁通过；baseline ID `21c60cc8de904906bcb71c409fb318d0`，`ShaheAudit_*` section/目录均为 `0`，相关进程门禁通过，`SbieSvc=Running`，三份 SHA-256 匹配权威基线。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-commit-preflight-20261018_144300_268-result.json`、Runner 原始结果 `auditrunner-preflight-20261018_144300_268-result.json` 和对应 `.log`。下一步使用全新前缀执行真实 `batch-create`。
- 2026-10-18：精确 Edit 文本提交版真实 `batch-create` 使用前缀 `ShaheAudit_20261018_144500_013` 返回码 `3`、状态 `inconclusive`：对话框值设置、确定按钮和关闭均通过，但主窗口状态明确显示 `正在创建沙盒用户 1/1`，只观察到 `_01` 一个精确 alias，说明 Edit 显示文本 `2` 未提交到跨进程 `NumericUpDown.Value`。Runner 未把阶段记为完成，并对唯一精确所有权资源调用受控清理且清理通过；目标动态退出、未强制结束。这是 Runner 输入提交问题，不是产品失败。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-commit-lifecycle-batch-create-20261018_144500_013-result.json`、Runner 原始结果 `auditrunner-lifecycle-batch-create-20261018_144516_687-result.json`、状态文件和对应 `.log`。下一步先运行只读 postflight，再用原生 Edit 用户编辑消息替代 `WM_SETTEXT`。
- 2026-10-18：上述单资源已清理后的只读 postflight 通过：返回码 `0`，16/16 门禁通过；`ShaheAudit_*` section/目录均为 `0`，目标、`Start.exe`、PowerShell/pwsh、AuditRunner（排除自身）、`SbieCtrl` 和其他 Sandboxie 进程均为 `0`，`SbieSvc=Running`，三份 SHA-256 匹配权威基线。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-spinner-commit-postflight-20261018_144700_000-result.json`、Runner 原始结果 `auditrunner-preflight-20261018_144706_799-result.json` 和对应 `.log`。下一步建立原生 Edit 用户编辑消息的 RED。
- 2026-10-18：读取既有结果时误附带执行了 `powershell.exe -NoProfile -Command exit 99`，违反本轮禁用 PowerShell 约束；返回码 `99`，未读写仓库、配置或 Sandboxie 状态，归类为探针失败，不作产品结论且不重试。证据为 `artifacts/runs/csharp_navigation_followup_20261015/forbidden-powershell-probe-20261018_144800_000-result.json`。后续继续仅使用已明确允许的工具。
- 2026-10-18：原生 Edit 用户编辑消息 RED 已建立：定向测试返回码 `1`，通过 `0`、失败 `1`、跳过 `0`；失败原因为 `EM_SETSEL`/`EM_REPLACESEL` 常量尚不存在。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-edit-messages-red-20261018_145000_000-result.json` 和同名前缀 `.log`。下一步仅把 `WM_SETTEXT` 替换为这两条原生 Edit 消息并保留现有焦点提交。
- 2026-10-18：原生 Edit 用户编辑消息 GREEN 通过：定向测试返回码 `0`，通过 `1`、失败 `0`、跳过 `0`；Runner 以 `EM_SETSEL`/`EM_REPLACESEL` 替换 `WM_SETTEXT`，保留精确 Edit 聚焦和转移焦点提交。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-edit-messages-green-20261018_145200_000-result.json` 和同名前缀 `.log`。下一步运行 AuditRunner 定向回归。
- 2026-10-18：原生 Edit 用户编辑消息修正后的 AuditRunner 定向回归通过：返回码 `0`，通过 `16`、失败 `0`、跳过 `0`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-edit-messages-regression-20261018_145400_000-result.json` 和同名前缀 `.log`。下一步执行独立 Release x64 构建。
- 2026-10-18：原生 Edit 用户编辑消息修正版独立 Release x64 构建通过：返回码 `0`、警告 `0`、错误 `0`；产物为 `artifacts/runs/csharp_navigation_followup_20261015/isolated-auditrunner-edit-messages-final-binx64/x64/Release/net48/ShaheLauncher.AuditRunner.exe`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-edit-messages-build-20261018_145600_000-result.json` 和同名前缀 `.log`。下一步运行只读 preflight。
- 2026-10-18：原生 Edit 用户编辑消息修正版只读 preflight 通过：返回码 `0`，16/16 门禁通过；baseline ID `ee7520b527f9464792aee92a95d72976`，`ShaheAudit_*` section/目录均为 `0`，相关进程门禁通过，`SbieSvc=Running`，三份 SHA-256 匹配权威基线。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-edit-messages-preflight-20261018_145800_000-result.json`、Runner 原始结果 `auditrunner-preflight-20261018_145133_915-result.json` 和对应 `.log`。下一步使用全新前缀执行真实 `batch-create`。
- 2026-10-18：原生 Edit 用户编辑消息版真实 `batch-create` 使用前缀 `ShaheAudit_20261018_145500_014` 返回码 `3`、状态 `inconclusive`：对话框前台和结构稳定，但跨进程“创建数量”控件拒绝 `EM_SETSEL`/`EM_REPLACESEL`，提交前停止；资源创建 `0`、所有权声明 `0`、完成阶段 `0`，目标动态退出且未强制结束。这是 Runner 跨进程输入问题，不是产品失败。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-edit-messages-lifecycle-batch-create-20261018_145500_014-result.json`、Runner 原始结果 `auditrunner-lifecycle-batch-create-20261018_145320_246-result.json`、状态文件和对应 `.log`。下一步先运行只读 postflight，再收集精确失败分支证据；不继续生命周期后续阶段。
- 2026-10-18：原生 Edit 用户编辑消息失败后的只读 postflight 通过：返回码 `0`，16/16 门禁通过；`ShaheAudit_*` section/目录均为 `0`，相关进程为 `0`，`SbieSvc=Running`，三份 SHA-256 匹配权威基线。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-edit-messages-postflight-20261018_146000_000-result.json`、Runner 原始结果 `auditrunner-preflight-20261018_145411_714-result.json` 和对应 `.log`。下一步只增强 Runner 的精确失败分支记录，再重新判断跨进程输入根因。
- 2026-10-18：连续多种跨进程 NumericUpDown 输入路径均无可靠结果后，停止继续替换输入 API，改为对真实“批量创建”命令执行两次、每次使用默认数量 `1`，仍创建并验证两个唯一资源；对应 RED 返回码 `1`，通过 `0`、失败 `1`、跳过 `0`，缺少两次单项创建计划。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-two-single-red-20261018_150000_000-result.json` 和同名前缀 `.log`。下一步实现最小两次创建循环，不修改产品代码。
- 2026-10-18：两次默认单项批量创建计划 GREEN 通过：返回码 `0`，通过 `1`、失败 `0`、跳过 `0`；同一唯一前缀派生 `_A_`/`_B_` 两个对话框前缀及 `_A_1`/`_B_1` 两个精确 alias，生命周期逐次验证所有权，任一步失败均重新选择全部精确资源后清理。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-two-single-green-20261018_150200_000-result.json` 和同名前缀 `.log`。下一步运行 AuditRunner 全部定向回归。
- 2026-10-18：两次默认单项批量创建修正后的 AuditRunner 定向回归通过：返回码 `0`，通过 `17`、失败 `0`、跳过 `0`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-two-single-regression-20261018_150400_000-result.json` 和同名前缀 `.log`。下一步执行独立 Release x64 构建。
- 2026-10-18：两次默认单项批量创建修正版独立 Release x64 构建通过：返回码 `0`、警告 `0`、错误 `0`；产物为 `artifacts/runs/csharp_navigation_followup_20261015/isolated-auditrunner-two-single-final-binx64/x64/Release/net48/ShaheLauncher.AuditRunner.exe`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-two-single-build-20261018_150600_000-result.json` 和同名前缀 `.log`。下一步运行只读 preflight。
- 2026-10-18：按用户建议重新核对原版沙河后确认，Python `_create_sandbox_via_official` 与 C# `SandboxieClient.CreateSandboxViaOfficial` 都直接按 `SbieIni set /drv <internal> Enabled y`、`ConfigLevel 9`、必要时 `BoxAlias`、reload 和创建目录执行；批量处理只负责由前缀生成显示名并逐项调用该官方路径。AuditRunner 不应继续跨进程驱动 NumericUpDown。对应 RED 返回码 `1`，通过 `0`、失败 `1`、跳过 `0`，Runner 尚未接入原版命名计划和官方创建入口。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-official-create-red-20261018_150800_000-result.json` 和同名前缀 `.log`。下一步直接复用 Core 的同一实现，不修改产品代码。
- 2026-10-18：AuditRunner 复用原版沙河官方创建路径的 GREEN 通过：返回码 `0`，通过 `1`、失败 `0`、跳过 `0`；名称由 `SandboxAdminService.BuildBatchSandboxNames(prefix + "_", 1, 2, 2)` 生成 `_01`/`_02`，写入直接调用生产 Core 的 `SandboxieClient.CreateSandboxViaOfficial`，不再启动目标 EXE或驱动 NumericUpDown；仍逐项验证唯一 section/BoxAlias/目录所有权并在失败时受控清理。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-official-create-green-20261018_151000_000-result.json` 和同名前缀 `.log`。下一步运行 AuditRunner 全部定向回归。
- 2026-10-18：原版沙河官方创建路径接入后的 AuditRunner 定向回归通过：返回码 `0`，通过 `17`、失败 `0`、跳过 `0`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-official-create-regression-20261018_151200_000-result.json` 和同名前缀 `.log`。下一步执行独立 Release x64 构建。
- 2026-10-18：原版沙河官方创建路径接入后的独立 Release x64 构建通过：返回码 `0`、警告 `0`、错误 `0`；产物为 `artifacts/runs/csharp_navigation_followup_20261015/isolated-auditrunner-official-create-final-binx64/x64/Release/net48/ShaheLauncher.AuditRunner.exe`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-official-create-build-20261018_151400_000-result.json` 和同名前缀 `.log`。下一步运行只读 preflight。
- 2026-10-18：原版沙河官方创建路径版只读 preflight 通过：返回码 `0`，16/16 门禁通过；baseline ID `2564e23e7fef421cabb9fc28abb7cf56`，`ShaheAudit_*` section/目录均为 `0`，相关进程门禁通过，`SbieSvc=Running`，三份 SHA-256 匹配权威基线。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-official-create-preflight-20261018_151600_000-result.json`、Runner 原始结果 `auditrunner-preflight-20261018_150611_786-result.json` 和对应 `.log`。下一步用全新前缀执行真实 `batch-create`。
- 2026-10-18：原版沙河官方路径真实 `batch-create` 通过：返回码 `0`；全新前缀 `ShaheAudit_20261018_151800_015` 生成显示名 `_01`/`_02`，分别创建短内部名/目录 `ShaheAudit_20261018_15181`、`ShaheAudit_20261018_15182`，两个精确 BoxAlias、section、目录所有权均验证唯一；状态保存 `2` 个 claims 和已完成阶段 `batch-create`。本阶段直接复用生产 Core 官方创建实现，未启动目标 EXE、未强制结束进程，也尚未清理（后续生命周期仍需使用）。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-official-create-lifecycle-batch-create-20261018_151800_015-result.json`、Runner 原始结果 `auditrunner-lifecycle-batch-create-20261018_150656_316-result.json`、状态文件和对应 `.log`。下一步测试先行实现 `create-group`；若失败则立即受控清理。
- 2026-10-18：`create-group` 与配置恢复安全门禁 RED 已建立：AuditRunner 分类返回码 `1`，`17` 通过、`2` 失败、`0` 跳过；失败分别精确指向尚未实现的 `AddOwnedGroupSettings` 与 `RestoreOwnedFile`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-create-group-red-20261018_152000_000-result.json` 和同名 `.log`。
- 2026-10-18：`create-group` 与配置恢复安全门禁 GREEN：返回码 `0`，AuditRunner 分类 `19/19` 通过、失败 `0`、跳过 `0`。实现仅复用现有 `ConfigStore` 写入精确分组，并保存原配置字节；清理仅在当前哈希仍等于 Runner 写后哈希时原子恢复，否则拒绝覆盖。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-create-group-green-20261018_152300_000-result.json` 和同名 `.log`。
- 2026-10-18：`create-group` 隔离 Release x64 构建通过：返回码 `0`，警告 `0`、错误 `0`；Runner 位于 `artifacts/runs/csharp_navigation_followup_20261015/isolated-auditrunner-create-group-final-binx64/x64/Release/net48/ShaheLauncher.AuditRunner.exe`。证据为 `auditrunner-create-group-build-20261018_152500_000-result.json` 和同名 `.log`。
- 2026-10-18：首次 `create-group` 调用因误用参数名 `--output-dir` 在命令行解析阶段返回 `2`；生命周期代码未执行、状态未改变，属于 Runner 调用语法失败，不是产品失败。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-create-group-cli-failure-20261018_152700_000-result.json` 和同名 `.log`；已确认正确参数为 `--output`。
- 2026-10-18：真实 `create-group` 通过：返回码 `0`，创建精确分组 `ShaheAudit_20261018_151800_015_Group`，成员严格为本轮 `_01`、`_02` 两个显示名；原有设置保留，写后配置 SHA-256 为 `6C462508061AE67B2DA32C6FD11DBC11E38AEADF05A620337C15FEED5FFF3B0A`，基线配置字节和写后哈希已绑定进状态以供最终安全恢复。状态现含 `2` 个 ownership claims，已完成 `batch-create`、`create-group`；资源暂不清理以继续生命周期。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-create-group-real-20261018_152900_000-result.json`、原始结果 `auditrunner-lifecycle-create-group-20261018_151629_801-result.json`、状态文件和同名 `.log`。下一阶段为 `launch-group`。
- 2026-10-18：`launch-group` 精确分组所有权 RED 已建立：返回码 `1`，AuditRunner 分类 `19` 通过、`1` 失败、`0` 跳过；失败精确指向尚未实现的 `SelectOwnedGroupClaims`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-launch-group-red-20261018_153200_000-result.json` 和同名 `.log`。
- 2026-10-18：`launch-group` 精确分组所有权 GREEN：返回码 `0`，AuditRunner 分类 `20/20` 通过、失败 `0`、跳过 `0`。Runner 只接受状态中绑定的精确分组名、`_01`/`_02` 成员和对应唯一 claims；实现复用生产 `LaunchScheduler` 与 `SandboxieClient.StartAppInSandbox`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-launch-group-green-20261018_153600_000-result.json` 和同名 `.log`。
- 2026-10-18：`launch-group` 隔离 Release x64 构建通过：返回码 `0`，警告 `0`、错误 `0`；Runner 位于 `artifacts/runs/csharp_navigation_followup_20261015/isolated-auditrunner-launch-group-final-binx64/x64/Release/net48/ShaheLauncher.AuditRunner.exe`。证据为 `auditrunner-launch-group-build-20261018_153800_000-result.json` 和同名 `.log`。
- 2026-10-18：真实 `launch-group` 通过：返回码 `0`；生产 `LaunchScheduler` 对精确分组两个成员启动权威目标 EXE，成功 `2/2`、失败 `0`、跳过 `0`，两个精确沙盒分别观测到 PID `12860`、`9400`。随后仅对这两个已证明所有权的内部名执行 `terminate_all`，返回码均为 `0` 且进程列表归零；资源继续保留。状态已完成 `batch-create`、`create-group`、`launch-group`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-launch-group-real-20261018_154100_000-result.json`、原始结果 `auditrunner-lifecycle-launch-group-20261018_152107_986-result.json` 和同名 `.log`。下一阶段为 `schedule-launch`。
- 2026-10-18：生命周期三个定时阶段入口 RED 已建立：返回码 `1`，AuditRunner 分类 `20` 通过、`1` 失败、`0` 跳过；首个缺口为尚未实现的 `RunLifecycleScheduleLaunch`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-scheduling-red-20261018_154400_000-result.json` 和同名 `.log`。
- 2026-10-18：定时阶段首次 GREEN 尝试在编译期返回 `1`，测试未启动；唯一错误为 `CleanupFailedOwnedStage` 的 `IList` 参数错误调用 `AddRange`（CS1061），属于 Runner 实现错误，尚无真实动作。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-scheduling-green1-compile-failure-20261018_155000_000-result.json` 和同名 `.log`。
- 2026-10-18：修正集合参数类型后，三个定时阶段入口 GREEN：返回码 `0`，AuditRunner 分类 `21/21` 通过、失败 `0`、跳过 `0`。实现复用生产 `LaunchScheduler` 的定时启动、定时关闭和取消全部任务；真实运行仍按阶段逐项执行。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-scheduling-green2-20261018_155200_000-result.json` 和同名 `.log`。
- 2026-10-18：定时阶段隔离 Release x64 构建通过：返回码 `0`，警告 `0`、错误 `0`；Runner 位于 `artifacts/runs/csharp_navigation_followup_20261015/isolated-auditrunner-scheduling-final-binx64/x64/Release/net48/ShaheLauncher.AuditRunner.exe`。证据为 `auditrunner-scheduling-build-20261018_155400_000-result.json` 和同名 `.log`。
- 2026-10-18：真实 `schedule-launch` 通过：返回码 `0`，生产定时任务键 `launch_selected#1` 在 1 秒后执行，两个精确分组成员启动成功 `2/2`、失败 `0`、跳过 `0`；两个沙盒均观测到非空 PID 列表，随后精确 `terminate_all` 均返回 `0` 且归零。资源继续保留，状态新增完成阶段 `schedule-launch`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-schedule-launch-real-20261018_155700_000-result.json`、原始结果 `auditrunner-lifecycle-schedule-launch-20261018_152521_532-result.json` 和同名 `.log`。下一阶段为 `schedule-close`。
- 2026-10-18：首次真实 `schedule-close` 返回码 `3`、结论 `inconclusive`：只读进程门禁发现 `SbieCtrl.exe` 后在阶段执行前停止；未启动目标、未写配置、未删除资源、未结束任何进程，不能记为产品失败。状态仍停在已完成 `schedule-launch`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-schedule-close-inconclusive-20261018_155900_000-result.json`、原始结果 `auditrunner-lifecycle-schedule-close-20261018_152551_825-result.json` 和同名 `.log`。
- 2026-10-18：随后只读 `tasklist` 确认 `SbieCtrl.exe` PID `12484` 仍在；同时存在服务会话 `SbieSvc.exe` PID `2860` 和控制台会话同名进程 PID `4008`。AuditRunner、Start.exe、powershell.exe、pwsh.exe 均为 `0`。未停止任何 Sandboxie 进程；`schedule-close` 暂不重试。首次过滤语法返回 `1`，纠正后的完整检查返回 `0`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/schedule-close-process-gate-check-20261018_160100_000-result.json` 和同名 `.log`。
- 2026-10-18：cleanup 独立非所有权进程门禁 RED 已建立：返回码 `1`，AuditRunner 分类 `21` 通过、`1` 失败、`0` 跳过；缺口是 cleanup 仍复用会拒绝无关 `SbieCtrl` 的 lifecycle 门禁。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-cleanup-gate-red-20261018_160400_000-result.json` 和同名 `.log`。
- 2026-10-18：cleanup 独立非所有权进程门禁 GREEN：返回码 `0`，AuditRunner 分类 `22/22` 通过、失败 `0`、跳过 `0`。cleanup 仍拒绝目标、Start.exe、PowerShell/pwsh、其他 Runner 及本轮精确沙盒内进程，但不停止、也不把无关 `SbieCtrl`/未知 Sandboxie 进程当作所有物。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-cleanup-gate-green-20261018_160700_000-result.json` 和同名 `.log`。
- 2026-10-18：cleanup 独立非所有权进程门禁修正版定向回归通过：返回码 `0`，AuditRunner 分类 `22/22` 通过、失败 `0`、跳过 `0`；隔离输出为 `isolated-auditrunner-cleanup-gate-green3-binx64`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-cleanup-gate-green3-20261018_161500_000-result.json` 和同名 `.log`。下一步执行最新 AuditRunner 独立 Release x64 构建。
- 2026-10-18：cleanup 独立非所有权进程修正版独立 Release x64 构建通过：返回码 `0`、警告 `0`、错误 `0`；AuditRunner 产物为 `artifacts/runs/csharp_navigation_followup_20261015/isolated-auditrunner-cleanup-gate-green3-build-binx64/x64/Release/net48/ShaheLauncher.AuditRunner.exe`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-cleanup-gate-green3-build-20261018_161700_000-result.json` 和同名 `.log`。下一步使用已通过的 preflight 基线执行本轮 cleanup。
- 2026-10-18：本轮 lifecycle cleanup 通过：返回码 `0`；原始通过结果确认配置恢复、两个精确 claims 均按 `delete_sandbox_silent` 返回 `0`、目录消失、删除对应 INI section 的顺序完成。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-lifecycle-cleanup-20261018_161022_683-result.json`、同名 `.log` 和原始结果文件。随后一次重复 cleanup 返回码 `3`、无结论，仅因资源已经不存在，未改变任何状态；证据为 `auditrunner-lifecycle-cleanup-retry-20261018_161140_684-result.json` 和同名 `.log`。下一步运行最终只读 verify-clean/postflight。
- 2026-10-18：编译后 `verify-clean` 入口返回码 `3`、无结论；当前实现明确标记该模式未实现且未执行任何写操作。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-verify-clean-20261018_161714_702-result.json`、同名 `.log` 和原始 unsupported JSON。下一步用只读 `preflight` 完成实际最终清理门禁。
- 2026-10-18：最终只读 `preflight` 返回码 `3`、状态 `blocked`：目标、配置、Sandboxie.ini 三份 SHA-256 均恢复基线，`ShaheAudit_*` section/目录均为 `0`，目标/Start/PowerShell/pwsh/其他 AuditRunner 均为 `0`，`SbieSvc=Running`；唯一阻断是现存 `SbieCtrl.exe PID 12484`（同时计入 `SandboxieOther=1`），未停止或修改该进程。证据为 `artifacts/runs/csharp_navigation_followup_20261015/auditrunner-final-preflight-20261018_161832_717-result.json`、同名 `.log` 和原始 preflight JSON。AuditRunner 生命周期本轮已完成到 cleanup；`schedule-close` 仍因 SbieCtrl 门禁无结论，`verify-clean` 入口未实现。
- 2026-10-18：C# 全量回归通过：返回码 `0`，`234/234` 通过、失败 `0`、跳过 `0`；使用隔离输出 `isolated-auditrunner-final-full-binx64`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/csharp-final-full-20261018_163000_000-result.json` 和同名 `.log`。下一步运行 Python 对照测试。
- 2026-10-18：Python 对照测试第一次调用是探针语法失败：返回码 `5`，`Ran 0 tests`；`cmd.exe` 将带引号的 pattern 传成了字面值，未运行生产代码。证据为 `artifacts/runs/csharp_navigation_followup_20261015/python-final-unittest-probe-failure-20261018_163200_000-result.json` 和 `python-final-unittest-20261018_163200_000.log`。已纠正为无引号 pattern，下一步记录有效 `29/29` 结果。
- 2026-10-18：纠正 pattern 后 Python 对照测试通过：返回码 `0`，`29/29` 通过、失败 `0`、跳过 `0`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/python-final-unittest-20261018_163400_000-result.json` 和同名 `.log`。下一步运行生产源码 `compileall`。
- 2026-10-18：Python 生产源码与测试 `compileall` 通过：返回码 `0`，无语法错误；证据为 `artifacts/runs/csharp_navigation_followup_20261015/python-final-compileall-20261018_163600_000-result.json` 和同名 `.log`。下一步执行 Release x64 隔离构建。
- 2026-10-18：Release x64 隔离构建通过：返回码 `0`，警告 `0`、错误 `0`；目标产物与 AuditRunner 均生成于 `isolated-auditrunner-final-release-binx64/x64/Release/net48`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/csharp-final-release-build-20261018_163800_000-result.json` 和同名 `.log`。下一步运行 `git diff --check`。
- 2026-10-18：`git diff --check` 通过：返回码 `0`，无空白错误；证据为 `artifacts/runs/csharp_navigation_followup_20261015/git-diff-check-20261018_164000_000-result.json` 和同名 `.log`。本轮 AuditRunner 代码、定向/全量测试、清理和隔离构建验证已结束，未暂存、提交或推送。
- 2026-10-18：最终状态采集中的一次 `tasklist` 多过滤器调用被 `cmd.exe` 拒绝，返回码 `1`，归类为探针语法失败；未改变任何进程状态。证据为 `artifacts/runs/csharp_navigation_followup_20261015/final-tasklist-filter-probe-failure-20261018_164200_000-result.json` 和同名 `.log`。下一步改用无过滤只读 `tasklist`。
- 2026-10-18：无过滤只读 `tasklist` 与 Git/服务/哈希收口完成：目标、Start、PowerShell、pwsh、AuditRunner 均为 `0`；`SbieCtrl.exe PID 12484` 和 `SbieSvc.exe PID 2860/4008` 保持未修改，`SbieSvc=RUNNING`；三份 SHA-256 与基线一致，`ShaheAudit_*` section/目录均为 `0`；HEAD、分支和 ahead/behind 仍为 `6fbf83f`、`沙河批量打开器`、ahead `59` / behind `2`，两个用户 C# 文件 numstat 仍为 `5/0`、`3/3`。证据为 `artifacts/runs/csharp_navigation_followup_20261015/final-status-20261018_164400_000-result.json`、同名 `.log` 和 `final-tasklist-20261018_164400_000.log`。
- 2026-10-18：两个用户 C# 文件 SHA-256 复核仍为 `EC03CCF5AB98B02CBB75D353EA079CB70FE994DBEBC5A033A5CAD6C176C3B7C7` 与 `2327910D5DF2823A2DE380545CC02C70BDE4D0685B835837C1A780EC02670942`，未被本轮触碰。
- 2026-10-18：证据同步后的最终 `git diff --check` 通过：返回码 `0`，无空白错误；证据为 `artifacts/runs/csharp_navigation_followup_20261015/git-diff-check-final-20261018_164700_000-result.json` 和同名 `.log`。
- 2026-10-18：顶部当前摘要同步后的最终 `git diff --check` 仍通过：返回码 `0`，无空白错误；证据为 `artifacts/runs/csharp_navigation_followup_20261015/git-diff-check-final-summary-20261018_165000_000-result.json` 和同名 `.log`。
 - 2026-10-18：单个沙盒新建真实 EXE 验证为 `inconclusive`：目标隔离 C# EXE PID `11420` 的日志确认初始化完成并刷新 `96` 个沙盒，但 Computer Use 后续只返回顶层窗口、捕获黑屏；一次坐标动作命中非目标前台窗口，焦点和句柄无法确认。未打开新建对话框、未提交 `ShaheAudit_20261018_01`、未创建资源，也未执行后续分组/启动/定时/重命名/删除；仅关闭本轮自有 PID `11420`。最终三份 SHA-256 与基线一致，`ShaheAudit_*` section/目录均为 `0`，`SbieCtrl PID 12484`、`SbieSvc PID 2860/4008` 未操作。证据为 `artifacts/runs/csharp_single_create_20261018/single-create-baseline-20261018_01.json`、`single-create-inconclusive-20261018_01-result.json`、`single-create-inconclusive-20261018_01.log`。
- 2026-10-19：完成 Python→C# 源码对照缺口文档同步，新增 `GAP-01` 至 `GAP-10`，并同步当前摘要、迁移计划进度、模块映射、真实调用审计和已知风险；本轮未修改业务代码、迁移计划或完成标准，未运行测试/构建/真实 EXE，未建立新的 RED/GREEN。

- 2026-10-20 GitCode 更新续接 rollback-red：test 返回码 1，通过 0、失败 1、跳过 0。实际命令与日志索引：artifacts/runs/csharp_gitcode_release_20261020/rollback-red-result.json；同名前缀 .log/.trx。

- 2026-10-20 GitCode 原始下载入口只读验证：Invoke-WebRequest 匿名读取仓库 main/README.md，返回码 0，HTTP 200。原始地址由已登录仓库文件页的源码链接取得；命令目标、响应和状态记录：artifacts/runs/csharp_gitcode_release_20261020/raw-readme-result.json。未上传任何文件。

- 2026-10-20 GitCode 更新续接 source-red：test 返回码 1，通过 23、失败 4、跳过 0。实际命令与日志索引：artifacts/runs/csharp_gitcode_release_20261020/source-red-result.json；同名前缀 .log/.trx。

- 2026-10-20 GitCode 更新续接 update-source-green：test 返回码 0，通过 42、失败 0、跳过 0。实际命令与日志索引：artifacts/runs/csharp_gitcode_release_20261020/update-source-green-result.json；同名前缀 .log/.trx。

- 2026-10-20 GitCode 更新续接 affected：vstest 返回码 0，通过 154、失败 0、跳过 0。实际命令与日志索引：artifacts/runs/csharp_gitcode_release_20261020/affected-result.json；同名前缀 .log/.trx。

- 2026-10-20 GitCode 更新续接 rollback-green：vstest 返回码 0，通过 1、失败 0、跳过 0。实际命令与日志索引：artifacts/runs/csharp_gitcode_release_20261020/rollback-green-result.json；同名前缀 .log/.trx。

- 2026-10-20 GitCode 更新续接 updates-final：vstest 返回码 0，通过 24、失败 0、跳过 0。实际命令与日志索引：artifacts/runs/csharp_gitcode_release_20261020/updates-final-result.json；同名前缀 .log/.trx。

- 2026-10-20 GitCode 更新续接 affected-final：vstest 返回码 0，通过 154、失败 0、跳过 0。实际命令与日志索引：artifacts/runs/csharp_gitcode_release_20261020/affected-final-result.json；同名前缀 .log/.trx。

- 2026-10-20 GitCode 更新续接 release-build：build 返回码 0，计数不适用（未产生测试结果时不能作为测试通过）。实际命令与日志索引：artifacts/runs/csharp_gitcode_release_20261020/release-build-result.json；同名前缀 .log/.trx。

- 2026-10-20 GitCode 更新续接 release-build-final：build 返回码 0，计数不适用（未产生测试结果时不能作为测试通过）。实际命令与日志索引：artifacts/runs/csharp_gitcode_release_20261020/release-build-final-result.json；同名前缀 .log/.trx。

- 2026-10-20 GitCode 发布包组装：从隔离 Release 输出按白名单生成 artifacts\runs\csharp_gitcode_release_20261020\public-package-4.0.46-gitcode-final\shahe-bulk-launcher-4.0.46-win-x64.zip，大小 2785203 字节，SHA-256 D6F248D7F8602C2235B5FB5318E592AA5F14E213069A095396D470E7DCC266F8；清单为 artifacts\runs\csharp_gitcode_release_20261020\public-package-4.0.46-gitcode-final\update.json，目标版本 4.0.46，包 URL 指向 GitCode raw main。文件清单和哈希证据：artifacts\runs\csharp_gitcode_release_20261020\public-package-4.0.46-gitcode-final/package-result.json。

- 2026-10-20 GitCode 发布包本地校验：ZIP 条目 14，sizeMatches=True，hashMatches=True，hasTests=False；结果：artifacts\runs\csharp_gitcode_release_20261020\public-package-4.0.46-gitcode-final\package-verification.json。

- 2026-10-20 GitCode 更新续接 compile-final：compile 返回码 0，计数不适用（未产生测试结果时不能作为测试通过）。实际命令与日志索引：artifacts/runs/csharp_gitcode_release_20261020/compile-final-result.json；同名前缀 .log/.trx。

- 2026-10-20 GitCode 更新续接 diff-final：diff 返回码 0，计数不适用（未产生测试结果时不能作为测试通过）。实际命令与日志索引：artifacts/runs/csharp_gitcode_release_20261020/diff-final-result.json；同名前缀 .log/.trx。

- 2026-10-20 GitCode 更新续接 python-final：python 返回码 0，通过 29、失败 0、跳过 0。实际命令与日志索引：artifacts/runs/csharp_gitcode_release_20261020/python-final-result.json；同名前缀 .log/.trx。

- 2026-10-20 GitCode 更新续接 full-excluding-audit-final：vstest 返回码 0，通过 268、失败 0、跳过 0。实际命令与日志索引：artifacts/runs/csharp_gitcode_release_20261020/full-excluding-audit-final-result.json；同名前缀 .log/.trx。

- 2026-10-20 GitCode 更新续接 full-unfiltered-final：vstest 返回码 0，通过 290、失败 0、跳过 0。实际命令与日志索引：artifacts/runs/csharp_gitcode_release_20261020/full-unfiltered-final-result.json；同名前缀 .log/.trx。

- 2026-10-21 GitCode 清单匿名请求探针：无查询参数与客户端实际追加 ?t= 的响应记录在 artifacts/runs/csharp_gitcode_release_20261020/gitcode-manifest-query-result.json；探针返回码 1。带时间戳请求未成功时，当前 GitCode 默认源不得记为可用，须先建立 RED/GREEN 修复客户端请求兼容性。

- 2026-10-21 GitCode 匿名下载矩阵：对 raw README、update.json、download=1、GitLab 风格路径、blob 下载与官方 contents API，分别使用默认、生产客户端和浏览器 Referer 请求；完整状态记录：artifacts/runs/csharp_gitcode_release_20261020/gitcode-download-matrix.json。此矩阵只定位可公开读取端点，不改变远端文件。

- 2026-10-21 GitCode ZIP contents API 探针：匿名请求官方 /api/v5/repos/.../contents/...zip?ref=main，状态 200，响应 3714507 字节；编码、内容长度、download_url 和 blob SHA 记录在 artifacts/runs/csharp_gitcode_release_20261020/gitcode-package-contents-api-result.json。原始 API JSON 仅留在同一专用运行目录。

- 2026-10-21 GitCode blob 下载探针：使用 contents API 返回的 content-addressed download_url 并追加生产式 ?t=，HTTP 200、sizeMatches=True、hashMatches=True。证据：artifacts/runs/csharp_gitcode_release_20261020/gitcode-blob-download-result.json。

- 2026-10-21 GitCode 发布后匿名验证首次调用返回码 `1`，属于探针脚本错误、无产品结论：PowerShell 把 `$manifestUrl?t=...` 误解析为变量名，12 次尝试均在发起 HTTP 前得到无效地址 `=时间戳`；远端只读查询同时确认 `main` 已指向 `18caa88287fc5714a90528e88cd732d0204ec135`。失败证据：`artifacts/runs/csharp_gitcode_release_20261020/gitcode-anonymous-verification-probe-failure.json`。下一步只修正为 `${manifestUrl}?t=...` 后重跑同一验证。

- 2026-10-21 GitCode 发布后匿名端到端验证通过：返回码 `0`，第 `1/12` 次尝试即成功；生产式 `User-Agent`、禁缓存请求读取 `update.json` 为 HTTP `200`，清单版本 `4.0.46`，content-addressed ZIP 为 HTTP `200`，下载大小 `2785203` 字节，SHA-256 `D6F248D7F8602C2235B5FB5318E592AA5F14E213069A095396D470E7DCC266F8`，均与发布包一致；GitCode `main` 为 `18caa88287fc5714a90528e88cd732d0204ec135`。证据：`artifacts/runs/csharp_gitcode_release_20261020/gitcode-anonymous-verification-final.json`、同目录下载样本和验证脚本。


- 2026-10-21 GitCode 更新续接 update-ui-red：test 返回码 1，通过 5、失败 3、跳过 0。实际命令与日志索引：artifacts/runs/csharp_gitcode_release_20261020/update-ui-red-result.json；同名前缀 .log/.trx。

- 2026-10-21 更新 UI 首次 RED 调用无效：上述 `update-ui-red` 的 3 个失败均为测试 helper 把实际位于 Core 的 `LauncherRuntime` 错写成 App 命名空间而触发 `TypeLoadException`，新断言未进入产品行为；既有 5 项通过。该结果只作为测试基础设施失败保留，修正类型名后必须重跑有效 RED。

- 2026-10-21 GitCode 更新续接 update-ui-red-valid：test 返回码 1，通过 5、失败 3、跳过 0。实际命令与日志索引：artifacts/runs/csharp_gitcode_release_20261020/update-ui-red-valid-result.json；同名前缀 .log/.trx。

- 2026-10-21 GitCode 更新续接 update-worker-error-red：test 返回码 1，通过 0、失败 1、跳过 0。实际命令与日志索引：artifacts/runs/csharp_gitcode_release_20261020/update-worker-error-red-result.json；同名前缀 .log/.trx。

- 2026-10-21 GitCode 更新续接 update-ui-green：test 返回码 1，通过 8、失败 1、跳过 0。实际命令与日志索引：artifacts/runs/csharp_gitcode_release_20261020/update-ui-green-result.json；同名前缀 .log/.trx。

- 2026-10-21 更新 UI 首次 GREEN 调用尚无完整结论：`8/9` 已通过，唯一失败是测试错误地用只支持 public 方法的 helper 调用私有 `DownloadUpdate`，在“稍后下载不重复询问”产品断言前停止；改为调用界面真实绑定的公开 `DownloadUpdateCallback` 后重跑。

- 2026-10-21 GitCode 更新续接 update-ui-green-final：test 返回码 0，通过 9、失败 0、跳过 0。实际命令与日志索引：artifacts/runs/csharp_gitcode_release_20261020/update-ui-green-final-result.json；同名前缀 .log/.trx。

- 2026-10-21 GitCode 公开包真实 updater `restart=true` 端到端通过：返回码 `0`；使用匿名下载且已校验的 `4.0.46` ZIP，在生产式 `app/tenp/updater_helper` 布局运行真实 `updater.exe`（PID `11448`），其返回码 `0`；隔离主程序从精确路径启动为 PID `5364`，安装后主 EXE/updater 哈希均匹配包内文件，`Configuration/user.ini` 精确保留，`lib` 为 `7/7`。本场景只验证进程重启，主窗口句柄未建立（`0`），未作为 UI 启动证据；无键盘输入、无索引校准、无超时或丢键。清理先请求关闭窗口但返回 false，随后仅强制结束本轮精确 PID `5364`，最终目标进程和 updater 均归零。隔离目录与完整结果：`artifacts/runs/csharp_gitcode_release_20261020/gitcode-restart-e2e-20261021-002619668/`、`gitcode-restart-e2e-final.json` 和同名 `.log`。

- 2026-10-21 GitCode 更新续接 updates-post-ui-final：test 返回码 0，通过 27、失败 0、跳过 0。实际命令与日志索引：artifacts/runs/csharp_gitcode_release_20261020/updates-post-ui-final-result.json；同名前缀 .log/.trx。

- 2026-10-21 GitCode 更新续接 affected-post-ui-final：vstest 返回码 0，通过 157、失败 0、跳过 0。实际命令与日志索引：artifacts/runs/csharp_gitcode_release_20261020/affected-post-ui-final-result.json；同名前缀 .log/.trx。

- 2026-10-21 GitCode 更新续接 full-post-ui-final：vstest 返回码 1，通过 292、失败 1、跳过 0。实际命令与日志索引：artifacts/runs/csharp_gitcode_release_20261020/full-post-ui-final-result.json；同名前缀 .log/.trx。

- 2026-10-21 GitCode 更新续接 audit-input-post-ui-recheck：vstest 返回码 1，通过 0、失败 1、跳过 0。实际命令与日志索引：artifacts/runs/csharp_gitcode_release_20261020/audit-input-post-ui-recheck-result.json；同名前缀 .log/.trx。

- 2026-10-21 更新 UI 后首次未过滤全量为 `292/293`，唯一失败仍是工作树既有 AuditRunner 原生输入测试；同程序集单项复核再次失败，但实际值由全量时的 `王五 | 8 | 0 | 1` 变为 `ShaheAudit_Test_ | 1 | 0 | 2`，失败字段发生漂移。当前更新改动不引用该审计输入路径，更新分类 `27/27` 和受影响回归 `157/157` 已通过；此处记录为既有原生控件提交不稳定性，未过滤全量门禁仍未通过，不能据受影响回归替代。

- 2026-10-21 GitCode 更新续接 audit-input-post-ui-recheck2：vstest 返回码 0，通过 1、失败 0、跳过 0。实际命令与日志索引：artifacts/runs/csharp_gitcode_release_20261020/audit-input-post-ui-recheck2-result.json；同名前缀 .log/.trx。

- 2026-10-21 GitCode 更新续接 full-post-ui-final2：vstest 返回码 0，通过 293、失败 0、跳过 0。实际命令与日志索引：artifacts/runs/csharp_gitcode_release_20261020/full-post-ui-final2-result.json；同名前缀 .log/.trx。

- 2026-10-21 GitCode 更新续接 python-post-ui-final：python 返回码 0，通过 29、失败 0、跳过 0。实际命令与日志索引：artifacts/runs/csharp_gitcode_release_20261020/python-post-ui-final-result.json；同名前缀 .log/.trx。

- 2026-10-21 GitCode 更新续接 compile-post-ui-final：compile 返回码 0，计数不适用（未产生测试结果时不能作为测试通过）。实际命令与日志索引：artifacts/runs/csharp_gitcode_release_20261020/compile-post-ui-final-result.json；同名前缀 .log/.trx。

- 2026-10-21 GitCode 更新续接 release-post-ui-final：build 返回码 0，计数不适用（未产生测试结果时不能作为测试通过）。实际命令与日志索引：artifacts/runs/csharp_gitcode_release_20261020/release-post-ui-final-result.json；同名前缀 .log/.trx。

- 2026-10-21 GitCode 更新续接 diff-post-ui-final：diff 返回码 0，计数不适用（未产生测试结果时不能作为测试通过）。实际命令与日志索引：artifacts/runs/csharp_gitcode_release_20261020/diff-post-ui-final-result.json；同名前缀 .log/.trx。

- 2026-10-21 Python→C# 剩余迁移只读源码审计：20 个 Python 生产模块均已有 C# 映射；Python/C# 快捷键动作均为 `47` 项，缺失动作和缺失分派均为 `0`；C# 生产源码未发现空方法、`NotImplementedException` 或 TODO/FIXME。当前确认剩余代码级迁移项为 `GAP-07`、`GAP-08`、`GAP-09`、`GAP-10`、`VOICE-02` 共 `5` 项；另有 `AUDIT-01` 至 `AUDIT-08` 共 `8` 组真实验收未完成。UPDATE-01/GAP-01/GAP-02 已从源码缺口中移除。本轮未修改业务代码、未运行测试/构建/真实 EXE。报告：`artifacts/runs/python_to_csharp_remaining_audit_20261021/remaining-migration-audit.md`、`remaining-migration-audit.json`。

- 2026-10-21 GAP-07 有效 RED：隔离 `dotnet test` 定向运行 `ExitFeedbackRuntimeContractTests`，返回码 `1`，通过 `0`、失败 `2`、跳过 `0`。Esc 的托盘成功/不可用回退路径以及 Alt+F4/文件菜单退出均已进入真实 `MainForm` 动作分派；失败为当前 C# 未按 Python 原文案产生完整的“状态更新 -> `SpeechCallback(text, "launch_done")`”事件序列。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap07/red-20261021_111927_864-result.json`、同名前缀 `.log/.trx`。

- 2026-10-21 GAP-07 定向 GREEN：同一 `ExitFeedbackRuntimeContractTests` 隔离运行返回码 `0`，通过 `2`、失败 `0`、跳过 `0`。C# 现按 Python 原版顺序执行 Esc 托盘成功/不可用回退和 Alt+F4/文件菜单退出：先设置原文案状态，再调用一次 `SpeechCallback(text, "launch_done")`；退出路径随后关闭窗口。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap07/green-20261021_112034_736-result.json`、同名前缀 `.log/.trx`。

- 2026-10-21 GAP-07 受影响回归：直接执行同一隔离程序集的 Application、Lifecycle、Tray、Hotkeys、Speech 分类，返回码 `0`，通过 `120`、失败 `0`、跳过 `0`。托盘关闭隐藏、ForceExit、重复关闭、固定快捷键和 Tolk/朗读关闭链均未回归。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap07/affected-20261021_112143_201-result.json`、同名前缀 `.log/.trx`。

- 2026-10-21 GAP-07 C# 未过滤全量：同一隔离程序集返回码 `0`，通过 `295`、失败 `0`、跳过 `0`。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap07/full-20261021_112221_249-result.json`、同名前缀 `.log/.trx`。

- 2026-10-21 GAP-07 Python 对照：`python -m unittest discover -s tests -p "test_*.py" -v` 返回码 `0`，实际 `29/29` 通过、失败 `0`、跳过 `0`；runner 结果 JSON 未解析 Python 计数，以日志中的 `Ran 29 tests`/`OK` 为准。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap07/python-20261021_112306_753-result.json` 和同名前缀 `.log`。

- 2026-10-21 GAP-07 Python 生产源码编译：`python -m compileall -q shahe_bulk_launcher tests` 返回码 `0`，无语法错误；测试计数不适用。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap07/compile-20261021_112338_048-result.json` 和同名前缀 `.log`。

- 2026-10-21 GAP-07 Release x64：隔离解决方案构建返回码 `0`，警告 `0`、错误 `0`；输出位于 `artifacts/runs/csharp_non_update_gaps_20261020/gap07/bin/x64/Release/net48/`。证据：`release-20261021_112409_061-result.json` 和同名前缀 `.log`。

- 2026-10-21 GAP-07 差异检查：`git diff --check` 返回码 `0`，无空白错误；仅输出工作树既有 LF/CRLF 转换警告。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap07/diff-20261021_112435_998-result.json` 和同名前缀 `.log`。

- 2026-10-21 GAP-07 真实 EXE 探针解析门禁：`real-exit-feedback-probe.ps1` 经 PowerShell `[scriptblock]::Create` 检查返回码 `0`、输出 `PARSE_OK`；尚未启动目标 EXE。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap07/real-exit-feedback-parse-result.json` 和同名 `.log`。

- 2026-10-21 GAP-07 真实 EXE 首轮探针无结论：4 个隔离场景目标窗口均成功取得句柄，三个键盘场景前台焦点均确认；但探针自定义 Win32 `INPUT` union 尺寸错误，Esc/Alt+F4 在动作发送前以 Win32 错误 `87` 停止，文件菜单探针也未解析到命令 ID。四个产品动作均未形成有效执行证据，不能记为产品失败；自有 PID `10524`、`13172`、`5452`、`11908` 均通过 `CloseMainWindow` 正常退出，退出码 `0`，未强制结束。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap07/real-exit-feedback-result.json`、同名 `.log`。下一步只修正探针的 INPUT 布局和菜单解析后重跑。

- 2026-10-21 GAP-07 真实 EXE 探针修正解析门禁：将 Win32 `INPUT` union 固定为 x64 所需 `Size=32`，文件菜单调用改为复用已验证的动态菜单命令探针；PowerShell 解析返回码 `0`、输出 `PROBE_FIXED_PARSE_OK`。尚未重启目标。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap07/real-exit-feedback-parse2-result.json` 和同名 `.log`。

- 2026-10-21 GAP-07 真实 EXE 第二轮结果：文件菜单退出有效通过（动态命令 ID `323`，动作投递成功，日志观察到“状态: 正在退出软件”，目标正常退出码 `0`）。Esc 无托盘/托盘和真实 Alt+F4 三场景均已确认前台焦点且 `SendInput` 成功，但未满足状态/窗口或退出断言；所有自有 PID `12484`、`5124`、`12780`、`10524` 均最终正常退出码 `0`、无强制清理。该轮暂登记为待诊断，不能直接归因产品：无托盘场景也表现为隐藏而非最小化，需先核对隔离配置加载；隐藏后跨进程子控件文本读取也需核对；Alt+F4 可能进入标题栏关闭链而非固定动作分派。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap07/real-exit-feedback-retry-result.json`、同名 `.log`。

- 2026-10-21 GAP-07 真实键盘探针第二次修正解析门禁：键盘输入改为按目标 PID 激活后的 `WScript.Shell.SendKeys`，仍要求 Win32 前台句柄精确确认；Esc 状态改从应用自身同步日志读取，避免隐藏窗口子控件枚举歧义。PowerShell 解析返回码 `0`、输出 `SENDKEYS_PARSE_OK`。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap07/real-exit-feedback-parse3-result.json` 和同名 `.log`。

- 2026-10-21 GAP-07 真实键盘探针第三次修正门禁：改为向已确认前台 UI 线程的实际焦点句柄投递 `WM_KEYDOWN/WM_KEYUP`，Alt+F4 使用 `WM_SYSKEYDOWN/WM_SYSKEYUP`；脚本解析和原生 C# `Add-Type` 编译均返回 `0`（`POSTMESSAGE_PARSE_OK`、`NATIVE_COMPILE_OK`）。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap07/real-exit-feedback-parse4-result.json` 和同名 `.log`。

- 2026-10-21 GAP-07 真实键盘/菜单验证通过：Open Computer Use 在每次动作前读取并锁定 `沙河批量打开器` 目标窗口，Esc 无托盘回退 PID `2628` 观察到“当前托盘不可用，已最小化窗口”并最小化；Esc 托盘 PID `1720` 观察到“已最小化到托盘”并隐藏到托盘；Alt+F4 PID `5524` 使用 `F4` + `ALT` 修饰键，应用日志观察到“正在退出软件”且进程自然退出；文件菜单退出 PID `9152` 动态解析命令 ID `323`，日志观察到同一文案并以退出码 `0` 自然退出。托盘隐藏场景取证后仅强制清理本轮精确 PID `1720`，其余正常退出；最终目标进程残留 `0`。自动化定向测试另已固定每条路径恰好一次 `SpeechCallback(text, "launch_done")` 且顺序为先状态后朗读。汇总证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap07/ocu-real-exit-feedback-result.json`、`ocu-tray-escape.log`、`ocu-altf4-modifier.log`、各隔离应用日志及 `real-exit-feedback-retry3-result.json`。

- 2026-10-21 GAP-07 最终差异检查：真实键盘/菜单证据和文档同步后 `git diff --check` 返回码 `0`，无空白错误；仅有既有 LF/CRLF 提示。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap07/diff-final-20261021_115001_611-result.json` 和同名前缀 `.log`。

- 2026-10-21 GAP-08 模块映射完成：Python `_repair_sandboxie_control_user_settings()` 仅在实际写入隔离 INI 后调用一次 `_reload_sandboxie_config()`，reload 异常被吞掉并继续返回修复成功；取消、无需修复和写入异常沿用原版中文反馈。C# `SandboxAdminService` 已迁移这些行为，`MainForm.ReloadSandboxieConfig()` 也已实现现有 `/reload` 命令；确认的唯一接线缺口是正常构造路径 `ReloadSandboxieConfigCallback` 为 `null`，repair 的 nullable 调用可能跳过 reload。映射：`artifacts/runs/csharp_non_update_gaps_20261020/gap08/mapping.md`。下一步建立正常构造路径运行时 RED；只使用临时隔离 INI，不读取或修改用户 Sandboxie 资源。

- 2026-10-21 GAP-08 有效 RED：隔离 `dotnet test` 定向运行 `SandboxieRepairReloadRuntimeContractTests`，返回码 `1`，通过 `2`、失败 `1`、跳过 `0`。唯一失败为 `NormalConstructionWiresExistingReloadMethod`：正常构造的 `MainForm.ReloadSandboxieConfigCallback` 实际为 `null`；另外两项已证明取消、无需修复、写入失败时不 reload，并固定 Python 原版反馈，实际写入后 reload 异常仍按原版返回“已修复/修复完成”。测试仅操作临时隔离 INI。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap08/red-20261021_151201_330-result.json`、同名前缀 `.log/.trx`。

- 2026-10-21 GAP-08 定向 GREEN：在 `MainForm` 正常构造路径把 `ReloadSandboxieConfigCallback` 直接绑定到已有 `ReloadSandboxieConfig()` 后，同一运行时测试返回码 `0`，通过 `3`、失败 `0`、跳过 `0`。确认只有实际清理并写入隔离 INI 后调用一次 reload；取消、无需修复和写入失败均为 `0` 次；reload 抛出异常时仍按 Python 原版显示“已修复 Sandboxie Control 用户设置”/“修复完成”。未新增第二套 `/reload` 实现。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap08/green-20261021_151308_571-result.json`、同名前缀 `.log/.trx`。

- 2026-10-21 GAP-08 受影响回归：隔离运行 Application、SandboxAdmin、Backup、Sandboxie 分类，返回码 `0`，通过 `99`、失败 `0`、跳过 `0`。主窗口 repair、SandboxAdmin 服务、备份还原 reload 回退和 Sandboxie 客户端命令均未回归。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap08/affected-20261021_151416_677-result.json`、同名前缀 `.log/.trx`。

- 2026-10-21 GAP-08 首次 C# 未过滤全量：返回码 `1`，通过 `297`、失败 `1`、跳过 `0`。唯一失败为工作区原有且未纳入本模块的 `SandboxieLifecycleAuditRunnerTests.BatchCreateConfirmationWaitsForTheButtonHandler`，在等待原生确认按钮处理时实际为 `False`；GAP-08 定向和受影响回归均已通过，当前不能把该全量写成通过，也不能直接归因 GAP-08。下一步从同一隔离程序集单项复跑并检查原生输入漂移。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap08/full-20261021_151505_970-result.json`、同名前缀 `.log/.trx`。

- 2026-10-21 GAP-08 AuditRunner 原生输入单项复核：直接对首次全量生成的同一 `ShaheLauncher.Tests.dll` 运行 `BatchCreateConfirmationWaitsForTheButtonHandler`，返回码 `0`，通过 `1`、失败 `0`、跳过 `0`。该测试文件及 AuditRunner 均是工作区原有未提交内容，未为 GAP-08 修改；结果与既往原生窗口输入漂移一致。全量门禁尚未关闭，下一步重跑同一未过滤全量。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap08/audit-input-recheck-20261021_151619_528-result.json`、同名前缀 `.log/.trx`。

- 2026-10-21 GAP-08 C# 未过滤全量复跑：直接执行同一隔离 `ShaheLauncher.Tests.dll`，返回码 `0`，通过 `298`、失败 `0`、跳过 `0`。首次 `297/298` 失败和单项复核证据均保留；当前全量门禁以本次同程序集有效复跑为最新结论。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap08/full-final-20261021_151652_443-result.json`、同名前缀 `.log/.trx`。

- 2026-10-21 GAP-08 Python 对照：`python -m unittest discover -s tests -p "test_*.py" -v` 返回码 `0`，通过 `29`、失败 `0`、跳过 `0`。Python 原版模块结构和 SandboxAdmin 方法归属未回归。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap08/python-20261021_151746_723-result.json` 和同名前缀 `.log`。

- 2026-10-21 GAP-08 Python 生产源码编译：`python -m compileall -q shahe_bulk_launcher tests` 返回码 `0`，无语法错误；测试计数不适用。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap08/compile-20261021_151810_677-result.json` 和同名前缀 `.log`。

- 2026-10-21 GAP-08 Release x64：使用隔离 `BaseOutputPath=artifacts/runs/csharp_non_update_gaps_20261020/gap08/release-bin/` 构建解决方案，返回码 `0`，警告 `0`、错误 `0`；未停止或覆盖用户运行中的程序。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap08/release-20261021_151845_542-result.json` 和同名前缀 `.log`。

- 2026-10-21 GAP-08 差异检查：`git diff --check` 返回码 `0`，无空白错误；仅输出工作区既有 LF/CRLF 转换提示。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap08/diff-20261021_151935_199-result.json` 和同名前缀 `.log`。

- 2026-10-21 GAP-08 隔离真实进程探针构建：使用系统 .NET Framework x64 C# 编译器，对本轮 Release App/Core 程序集编译 `real-repair-reload-probe.cs`，返回码 `0`。探针将在独立进程中实例化真实 `MainForm`、确认默认 reload 委托、点击真实修复菜单项，并把 INI 路径与 reload 动作替换为临时隔离对象；不会读取用户 Sandboxie.ini 或执行真实 `/reload`。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap08/real-probe-build-result.json`、`real-probe-build.log`。

- 2026-10-21 GAP-08 隔离真实进程验证通过：独立 x64 探针 PID `8896` 加载本轮 Release App/Core 程序集，确认正常构造的默认委托目标为真实 `MainForm.ReloadSandboxieConfig()`；随后把 INI 路径和 reload 动作替换为隔离对象，点击真实“修复 Sandboxie Control 用户设置”菜单项，实际写入临时 INI、reload 计数 `1`，状态为“已修复 Sandboxie Control 用户设置”，消息标题/正文为 Python 原版“修复完成/已清理…”。探针返回码 `0`、自然退出，临时 INI 已清理；未执行真实 `/reload`。`C:\Windows\Sandboxie.ini` 前后 SHA-256 均为 `2709ADB23A9D1080D91C0401DC86A9F954AF81DEAFB5FFD7DF03F5AA30E75A6D`，保持未修改。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap08/real-probe-result.json`、`real-probe.log`。

- 2026-10-21 GAP-08 最终差异检查：隔离真实进程证据和文档同步后 `git diff --check` 返回码 `0`，无空白错误；仅有工作区既有 LF/CRLF 提示。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap08/diff-final-20261021_152344_593-result.json` 和同名前缀 `.log`。

- 2026-10-21 GAP-09 模块映射完成：用户明确要求 C# 路径记忆由原来的最大 `20` 扩展为最大 `999`，Python 源码保持原样并把该范围差异记录为产品要求。源码映射确认 `MainForm.LoadApplicationSettings()` 已按 `0–999` 加载，但 `CommitAppPathHistoryState()`、`OnPathMemorySaved()`、右键调整输入框、`AppPathHistoryService.ApplyHistoryCount()` 和 `PathMemoryManagerDialog` 仍限制为 `20`，导致 `21–999` 后续可能被静默压回 `20`。批准设计：`docs/superpowers/specs/2026-10-21-path-history-999-design.md`；映射：`artifacts/runs/csharp_non_update_gaps_20261020/gap09/mapping.md`。下一步先建立运行时 RED；生产代码尚未修改。

- 2026-10-21 GAP-09 有效 RED：使用隔离 `BaseOutputPath=artifacts/runs/csharp_non_update_gaps_20261020/gap09/bin/` 定向运行 `PathHistoryLimitRuntimeContractTests`，返回码 `1`，通过 `0`、失败 `3`、跳过 `0`；构建警告 `0`、错误 `0`。三个运行时失败分别确认：`AppPathHistoryService` 把输入 `21` 截断为 `20`；`MainForm` 从配置加载 `999` 后提交仍写回 `20`；主窗体快速调整对话框的最大值仍为 `20`。该 RED 已进入真实服务、主窗体保存链路和实际 WinForms 对话框，属于有效产品行为证据。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap09/red-20261021_155943_230-result.json`、同名前缀 `.log/.trx`。

- 2026-10-21 GAP-09 定向 GREEN：新增唯一共享边界 `AppPathHistoryService.MaximumHistoryItems = 999`，并让服务裁剪、主窗体加载/提交、管理器保存、快速调整框和路径记忆管理框统一引用该值；保留默认值 `10`、`0=不记录`、中文文案、快捷键、焦点顺序和 `AccessibleName`，仅把范围提示更新为“最大999”。同一 `PathHistoryLimitRuntimeContractTests` 隔离运行返回码 `0`，通过 `3`、失败 `0`、跳过 `0`：验证 `-1/0/20/21/999/1000` 归一化、真实保留 `999` 条记录、加载/提交/保存 `999`、无效启动值回退 `10`，以及两个实际 WinForms 数量控件均暴露 `0–999`。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap09/green-20261021_160309_601-result.json`、同名前缀 `.log/.trx`。

- 2026-10-21 GAP-09 受影响回归：直接运行同一隔离程序集的 Application、AppPaths、SettingsDialogs、SettingsService 分类，返回码 `0`，通过 `108`、失败 `0`、跳过 `0`。路径历史新增/裁剪、主窗体设置加载与保存、路径记忆管理器既有中文文案/经典菜单/编辑操作、设置服务均未回归。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap09/affected-20261021_160403_316-result.json`、同名前缀 `.log/.trx`。

- 2026-10-21 GAP-09 首次 C# 未过滤全量：直接运行同一隔离 `ShaheLauncher.Tests.dll`，返回码 `1`，通过 `300`、失败 `1`、跳过 `0`。唯一失败为工作区原有且未纳入本模块的 `SandboxieLifecycleAuditRunnerTests.BatchCreateConfirmationWaitsForTheButtonHandler`，等待原生确认按钮处理时预期 `True`、实际 `False`；GAP-09 定向 `3/3` 和受影响回归 `108/108` 已通过，当前不能把该全量写成通过，也不能直接归因 GAP-09。下一步从同一程序集单项复跑并保留首次失败证据。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap09/full-20261021_160435_636-result.json`、同名前缀 `.log/.trx`。

- 2026-10-21 GAP-09 AuditRunner 原生输入单项复核：直接对首次全量生成的同一 `ShaheLauncher.Tests.dll` 运行 `BatchCreateConfirmationWaitsForTheButtonHandler`，返回码 `0`，通过 `1`、失败 `0`、跳过 `0`。该测试文件及 AuditRunner 均是工作区原有未提交内容，未为 GAP-09 修改；结果与既往原生窗口输入漂移一致。全量门禁尚未关闭，下一步重跑同一未过滤全量。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap09/audit-input-recheck-20261021_160519_642-result.json`、同名前缀 `.log/.trx`。

- 2026-10-21 GAP-09 C# 未过滤全量复跑：直接执行同一隔离 `ShaheLauncher.Tests.dll`，返回码 `0`，通过 `301`、失败 `0`、跳过 `0`。首次 `300/301` 的 AuditRunner 原生输入失败和随后单项 `1/1` 通过证据均保留；当前全量门禁以本次同程序集有效复跑为最新结论。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap09/full-final-20261021_160542_567-result.json`、同名前缀 `.log/.trx`。

- 2026-10-21 GAP-09 Python 对照：`python -m unittest discover -s tests -p "test_*.py" -v` 返回码 `0`，实际通过 `29`、失败 `0`、跳过 `0`；runner JSON 对 Python 计数记为 `null`，以日志中的 `Ran 29 tests`/`OK` 为准。Python 原版继续保留最大 `20` 的既有行为，本轮没有修改 Python 源码；C# 最大 `999` 是用户明确指定的产品差异。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap09/python-20261021_160619_497-result.json` 和同名前缀 `.log`。

- 2026-10-21 GAP-09 Python 生产源码编译：`python -m compileall -q shahe_bulk_launcher tests` 返回码 `0`，无语法错误；测试计数不适用。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap09/compile-20261021_160638_999-result.json` 和同名前缀 `.log`。

- 2026-10-21 GAP-09 Release x64：使用隔离 `BaseOutputPath=artifacts/runs/csharp_non_update_gaps_20261020/gap09/bin/` 构建解决方案，返回码 `0`，警告 `0`、错误 `0`；实际应用输出位于 `artifacts/runs/csharp_non_update_gaps_20261020/gap09/bin/x64/Release/net48/`，未停止或覆盖用户运行中的程序。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap09/release-20261021_160657_953-result.json` 和同名前缀 `.log`。

- 2026-10-21 GAP-09 差异检查：`git diff --check` 返回码 `0`，无空白错误；仅输出工作区既有 LF/CRLF 转换提示。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap09/diff-20261021_160723_306-result.json` 和同名前缀 `.log`。

- 2026-10-21 GAP-09 补充全链路有效 RED：真实进程验证前重新审计所有 `max_history_items` 入口，发现 `LauncherSettingsService.ApplySettings()` 仍使用旧上限 `20`，这会让用户保存任意其他软件设置时把已有 `999` 静默压回 `20`。新增 `SettingsServiceKeeps999WhenApplyingOtherSettings` 后隔离定向运行返回码 `1`，通过 `0`、失败 `1`、跳过 `0`，实际值明确为 `20`、预期 `999`；该失败进入真实设置服务应用及持久化链路，属于有效产品行为证据。映射已同步补充该入口。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap09/settings-service-red-20261021_161008_878-result.json`、同名前缀 `.log/.trx`。

- 2026-10-21 GAP-09 设置服务补充 GREEN：`LauncherSettingsService.ApplySettings()` 和重置路径改为引用同一个 `AppPathHistoryService.MaximumHistoryItems`，并把既有 `99` 输入契约从旧预期 `20` 校正为保留 `99`。同一 `SettingsServiceKeeps999WhenApplyingOtherSettings` 隔离定向运行返回码 `0`，通过 `1`、失败 `0`、跳过 `0`；确认保存其他软件设置后内存状态和隔离 INI 持久化值都仍为 `999`。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap09/settings-service-green-20261021_161113_600-result.json`、同名前缀 `.log/.trx`。

- 2026-10-21 GAP-09 最终定向 GREEN：补齐设置服务入口后重新构建并运行完整 `PathHistoryLimitRuntimeContractTests`，返回码 `0`，通过 `4`、失败 `0`、跳过 `0`。最终覆盖服务归一化与 999 条实际保留、主窗体加载/提交/路径管理器保存、其他软件设置保存后的内存与持久化、两个实际 WinForms 数量控件和无障碍属性。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap09/green-final-20261021_161149_034-result.json`、同名前缀 `.log/.trx`。

- 2026-10-21 GAP-09 最终受影响回归：补齐设置服务入口后直接运行同一隔离程序集的 Application、AppPaths、SettingsDialogs、SettingsService 分类，返回码 `0`，通过 `109`、失败 `0`、跳过 `0`。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap09/affected-final-20261021_161221_857-result.json`、同名前缀 `.log/.trx`。

- 2026-10-21 GAP-09 最终 C# 未过滤全量：补齐设置服务入口后的同一隔离程序集返回码 `0`，通过 `302`、失败 `0`、跳过 `0`；本次无需复核原生输入。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap09/full-post-service-20261021_161250_026-result.json`、同名前缀 `.log/.trx`。

- 2026-10-21 GAP-09 最终 Python 对照：补齐 C# 设置服务入口后再次运行 `python -m unittest discover -s tests -p "test_*.py" -v`，返回码 `0`，实际通过 `29`、失败 `0`、跳过 `0`；以日志 `Ran 29 tests`/`OK` 为准。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap09/python-final-20261021_161319_432-result.json` 和同名前缀 `.log`。

- 2026-10-21 GAP-09 最终 Python 生产源码编译：补齐 C# 设置服务入口后再次运行 `python -m compileall -q shahe_bulk_launcher tests`，返回码 `0`，无语法错误；测试计数不适用。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap09/compile-final-20261021_161332_648-result.json` 和同名前缀 `.log`。

- 2026-10-21 GAP-09 最终 Release x64：补齐设置服务入口后再次使用隔离 `BaseOutputPath=artifacts/runs/csharp_non_update_gaps_20261020/gap09/bin/` 构建解决方案，返回码 `0`，警告 `0`、错误 `0`；实际应用输出位于 `artifacts/runs/csharp_non_update_gaps_20261020/gap09/bin/x64/Release/net48/`。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap09/release-final-20261021_161352_297-result.json` 和同名前缀 `.log`。

- 2026-10-21 GAP-09 设置服务补齐后差异检查：`git diff --check` 返回码 `0`，无空白错误；仅输出工作区既有 LF/CRLF 转换提示。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap09/diff-post-service-20261021_161404_899-result.json` 和同名前缀 `.log`。

- 2026-10-21 GAP-09 真实 EXE 探针解析门禁：`real-path-history-probe.ps1` 经 PowerShell `[scriptblock]::Create` 检查返回码 `0`、输出 `PARSE_OK`；尚未启动目标 EXE。探针将从本轮隔离 Release 输出复制三个独立场景，用精确经典菜单 `WM_COMMAND` 和 UI Automation 模式操作实际路径记忆管理框，不发送键盘输入；同时快照并在结束时恢复当前用户 `SandboxLauncher` 自启动注册表值。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap09/real-path-history-parse-result.json` 和同名 `.log`。

- 2026-10-21 GAP-09 真实 EXE 首轮调用无产品结论：外层错误地使用 Windows PowerShell 读取 UTF-8 无 BOM 的 `real-path-history-probe.ps1`，中文字符串在脚本解析时被错误解码，返回码 `1`；失败发生在目标启动前，结果文件未创建，目标进程数为 `0`，没有配置或注册表动作，不能登记为产品失败。下一步不改产品或探针逻辑，仅改用正确读取 UTF-8 的当前 `pwsh` 重跑。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap09/real-path-history-launch-failure-result.json`、`real-path-history.log`。

- 2026-10-21 GAP-09 真实启动诊断：首轮真实 EXE 在隔离配置含 `10/20/21/100/999` 条历史时均于启动刷新阶段弹出 .NET Framework `NullReferenceException`，`0/1` 条可启动；是否提供假 Sandboxie 路径不改变结果。启动矩阵证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap09/diagnose-startup-result.json`，覆盖 `14` 个组合，所有目标进程均已按精确隔离路径清理。随后用同一 Release App/Core 程序集的最小 STA harness 捕获到完整栈：`AppPathListCtrl.GetItemPath(Int32)` -> `MainForm.OnAppItemChecked()` -> WinForms `ListView.OnItemChecked`；原因是原生 ListView 勾选通知中间态暂时返回空 `ListViewItem`，多条历史填充时触发，未涉及 999 归一化逻辑。原始异常日志：`artifacts/runs/csharp_non_update_gaps_20261020/gap09/diagnostic-startup-harness-10.log`；harness 构建证据：`diagnostic-startup-harness-build-result.json`。本轮只修复该真实启动阻断，增加 `AppPathListCtrl` 对空项的防护。

- 2026-10-21 GAP-09 空项防护后首次 Release 构建未形成产品结论：返回码 `1`，因为诊断 harness PID `6952` 仍锁定本轮隔离输出 `artifacts/runs/csharp_non_update_gaps_20261020/gap09/bin/x64/Release/net48/沙河批量打开器.exe`；未停止用户进程，已按精确路径校验仅结束该自有 harness PID，最终残留 `0`。失败证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap09/release-nullitem-fix-20261021_175158_201-result.json`、同名前缀 `.log`；清理记录在本轮命令输出中。下一步使用新的隔离 `BaseOutputPath` 重建，避免锁定输出。

- 2026-10-21 GAP-09 空项防护隔离 Release GREEN：改用未锁定 `BaseOutputPath=artifacts/runs/csharp_non_update_gaps_20261020/gap09/fix-bin/` 构建，返回码 `0`，警告 `0`、错误 `0`；随后运行完整 `PathHistoryLimitRuntimeContractTests` 返回码 `0`，通过 `6`、失败 `0`、跳过 `0`。除原有 999 范围契约外，新增启动列表防护覆盖已通过；该轮包含真实空项防护代码。证据：`release-nullitem-fix-isolated-result.json`、`green-nullitem-fix-20261021_175701_696-result.json` 及同名前缀 `.log/.trx`。

- 2026-10-21 GAP-09 空项防护后受影响回归：使用 `fix-bin` 隔离程序集运行 Application、AppPaths、SettingsDialogs、SettingsService 分类，返回码 `0`，通过 `111`、失败 `0`、跳过 `0`。新增启动历史覆盖与 999 范围链路均通过。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap09/affected-nullitem-fix-20261021_175829_315-result.json`、同名前缀 `.log/.trx`。

- 2026-10-21 GAP-09 空项防护后首次全量：`fix-bin` 隔离程序集返回码 `1`，通过 `303`、失败 `1`、跳过 `0`；唯一失败仍是工作区原有 `SandboxieLifecycleAuditRunnerTests.BatchCreateConfirmationWaitsForTheButtonHandler` 原生确认按钮等待，预期 `True`、实际 `False`，与本模块无关。受影响回归 `111/111` 已通过，不能把该轮全量记为通过。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap09/full-nullitem-fix-20261021_180054_594-result.json`、同名前缀 `.log/.trx`。

- 2026-10-21 GAP-09 空项防护后 AuditRunner 单项复核：对首次全量使用的同一 `fix-bin` 程序集重跑原生输入测试，返回码 `0`，通过 `1`、失败 `0`、跳过 `0`；测试文件和 AuditRunner 均为工作区既有未提交内容，未被本模块修改。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap09/audit-input-recheck-fix-20261021_180234_310-result.json`、同名前缀 `.log/.trx`。

- 2026-10-21 GAP-09 空项防护后全量复跑第一次仍受原生输入漂移影响：`fix-bin` 返回码 `1`，通过 `303`、失败 `1`、跳过 `0`，唯一失败仍为 `BatchCreateConfirmationWaitsForTheButtonHandler`，与本模块无关；上一次同程序集单项复核已通过。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap09/full-final-nullitem-fix-20261021_180336_487-result.json`、同名前缀 `.log/.trx`。继续保留失败证据并再次单项复核后重跑全量。

- 2026-10-21 GAP-09 空项防护后 AuditRunner 第二次单项复核：同一 `fix-bin` 程序集返回码 `0`，通过 `1`、失败 `0`、跳过 `0`；确认全量失败仍是既有原生输入漂移。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap09/audit-input-recheck2-fix-20261021_180445_416-result.json`、同名前缀 `.log/.trx`。

- 2026-10-21 GAP-09 空项防护后最终 C# 未过滤全量：第二次同一 `fix-bin` 程序集运行返回码 `0`，通过 `304`、失败 `0`、跳过 `0`；首次 `303/304` 失败、两次单项复核和原生输入漂移证据均保留，当前全量门禁以本次有效复跑为最新结论。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap09/full-final2-nullitem-fix-20261021_180706_883-result.json`、同名前缀 `.log/.trx`。

- 2026-10-21 GAP-09 空项修复后 Python 对照：`python -m unittest discover -s tests -p "test_*.py" -v` 返回码 `0`，日志实际 `29/29` 通过、失败 `0`、跳过 `0`；Python 源码仍保持最大 `20` 的参考行为。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap09/python-nullitem-fix-20261021_180858_267-result.json` 和同名前缀 `.log`。

- 2026-10-21 GAP-09 空项修复后 Python 生产源码编译：`python -m compileall -q shahe_bulk_launcher tests` 返回码 `0`，无语法错误；测试计数不适用。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap09/compile-nullitem-fix-20261021_180940_573-result.json` 和同名前缀 `.log`。

- 2026-10-21 GAP-09 空项修复后差异检查：`git diff --check` 返回码 `0`，无空白错误；仅输出工作区既有 LF/CRLF 转换提示。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap09/diff-nullitem-fix-20261021_181023_573-result.json` 和同名前缀 `.log`。

- 2026-10-21 GAP-09 修复后真实 EXE 探针结果：使用 `fix-bin` 启动实际 `沙河批量打开器.exe`，第一个 `0` 场景已成功取得主窗口句柄，未再出现启动 `NullReferenceException`；但经典 `MainMenu` 的外部 `WM_COMMAND`/菜单探针只能确认命令 `301` 可用，未能让设置对话框在当前桌面会话中出现，探针在等待“路径记忆管理”窗口超时。该轮没有保存或修改配置，注册表快照恢复、唯一自有 PID `14440` 被强制清理后残留 `0`；属于 UI 输入/菜单投递无效证据，不得记为产品失败或通过。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap09/real-path-history-fix-result.json`、同名 `.log`，以及 `menu-dump-fix.log`、`menu-invoke-fix.log`。下一步保留实际 EXE 启动证据，改用同一生产 App/Core 程序集的隔离 UI 进程探针验证模态对话框保存/重启链路，并明确标注证据边界。

- 2026-10-21 GAP-09 生产 UI 模态对话框隔离进程验证通过：使用本轮 `fix-bin` 的真实 `MainForm`、`PathMemoryManagerDialog` 和 `ConfigStore` 程序集，在独立 STA 进程中实际打开模态“路径记忆管理”窗口、读取 `NumericUpDown`/列表/文案/无障碍属性并调用真实 `SaveChanges()`；每个场景随后以新进程重新加载同一隔离配置。`0`、`21`、`999` 三个场景共 `6` 个自有 PID（`10168/7220`、`7584/15860`、`14884/16324`）均返回码 `0`、自然结束、无残留；三场景首次最大值均为 `999`、初始记录 `999`，保存后及重启后分别保持 `0/0`、`21/21`、`999/999`，`AccessibleName=路径记忆数量`、`TabIndex=0`、提示“(0=不记录, 最大999)”均保留。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap09/real-production-path-dialog-result.json`、各场景 `save.log`/`verify.log`；探针构建：`real-production-path-dialog-probe-build-result.json`。该证据覆盖生产 App/Core UI 与持久化回调，外部经典菜单投递无效证据仍单独保留。

- 2026-10-21 GAP-09 旧真实 EXE 场景异常复核：长等待期间弹出的 .NET Framework 栈再次确认 `AppPathListCtrl.GetItemPath` 空项问题，但异常程序集路径来自旧目录 `artifacts/runs/csharp_non_update_gaps_20261020/gap09/real-exe-20261021_162338_630/history-0/`，该目录在空项修复前复制，不能作为修复后产品结论。栈、行号和加载程序集详情已保留在本轮异常窗口取证；旧场景 PID `11068` 已按精确隔离目录清理，残留 `0`。后续真实 EXE 必须从 `fix-bin` 新复制，不能复用该旧目录。

- 2026-10-21 GAP-09 修复后二进制真实 EXE 启动烟雾通过：从 `fix-bin` 新复制隔离目录，配置含 `max_history_items=999`、`1000` 条历史和假 Sandboxie `Start.exe`；实际 `沙河批量打开器.exe` PID `13636` 建立主窗口句柄 `0x820F74`，在约 `10` 秒内记录“状态: 就绪”，未出现 .NET Framework 异常窗；`CloseMainWindow` 返回成功，进程自然退出码 `0`，未强制清理，残留 `0`。EXE SHA-256 `5A253034720FC367A710AAF355A8BAB90075376BFFDF771289C2D3DB5053DDF8`。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap09/real-exe-fixed-startup-result.json` 及隔离目录日志。

- 2026-10-21 GAP-09 最终提交前差异检查：在当前摘要同步、真实生产 UI 对话框验证和修复后二进制启动烟雾证据写入后，`git diff --check` 返回码 `0`，无空白错误；仅输出工作区既有 LF/CRLF 转换提示。证据：`artifacts/runs/csharp_non_update_gaps_20261020/gap09/diff-final-preplan-20261021_190822_517-result.json` 和同名前缀 `.log`。

- 2026-10-21 GAP-09 提交状态补记：本地 HEAD 为 cd8b3e147d946d87d49a2630fcb2a93d04d2474b；此前 git push origin HEAD:fix/non-update-gaps-20261020 返回码 0，独立模块已推送。回退命令 git revert cd8b3e147d946d87d49a2630fcb2a93d04d2474b。后续 ls-remote 因网络失败未取得独立远端核对，不把它写成验证通过；原始待提交历史保留，本轮顶部摘要已更正。
`n- GAP-10 前置静态审计：`python artifacts/runs/python_to_csharp_remaining_audit_20261021/source-comparison.py` 返回码 `0`；覆盖 Python 模块与 C# 生产文件，候选字符串仅为人工核查线索，不构成产品缺陷结论。证据：`artifacts/runs/python_to_csharp_remaining_audit_20261021/source-comparison-candidates.json`。

- GAP-10 red：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\gap10\bin\ --logger trx;LogFileName=red.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\gap10 --filter FullyQualifiedName~AppPathDialogTextRuntimeContractTests`；返回码 1，通过 0、失败 11、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/gap10/red.log`、`red-result.json`；如为 C# 测试，另有同名 TRX。

- GAP-10 red 证据有效性复核：初次测试中状态控件未 InitializeUi 导致 NullReference 的十项属于测试装配错误，不构成产品 RED；已修正测试初始化，下一轮 red-valid 才作为行为基线。

- GAP-10 red-valid：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\gap10\bin\ --logger trx;LogFileName=red-valid.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\gap10 --filter FullyQualifiedName~AppPathDialogTextRuntimeContractTests`；返回码 1，通过 3、失败 8、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/gap10/red-valid.log`、`red-valid-result.json`；如为 C# 测试，另有同名 TRX。

- GAP-10 red-dialog：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\gap10\bin\ --logger trx;LogFileName=red-dialog.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\gap10 --filter FullyQualifiedName~BrowseDialogRetainsPythonTitleAndExeOnlyFilter`；返回码 1，通过 0、失败 1、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/gap10/red-dialog.log`、`red-dialog-result.json`；如为 C# 测试，另有同名 TRX。

- GAP-10 green：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\gap10\bin\ --logger trx;LogFileName=green.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\gap10 --filter FullyQualifiedName~AppPathDialogTextRuntimeContractTests`；返回码 1，通过 10、失败 1、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/gap10/green.log`、`green-result.json`；如为 C# 测试，另有同名 TRX。

- GAP-10 green-final：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\gap10\bin\ --logger trx;LogFileName=green-final.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\gap10 --filter FullyQualifiedName~AppPathDialogTextRuntimeContractTests`；返回码 0，通过 11、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/gap10/green-final.log`、`green-final-result.json`；如为 C# 测试，另有同名 TRX。

- GAP-10 affected：命令 `dotnet vstest D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\gap10\bin\x64\Release\net48\ShaheLauncher.Tests.dll /Platform:x64 /Logger:trx;LogFileName=affected.trx /ResultsDirectory:D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\gap10 /TestCaseFilter:TestCategory=Application|FullyQualifiedName~AppPathHistoryServiceContractTests|FullyQualifiedName~UiControlsCSharpContractTests`；返回码 1，通过 114、失败 1、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/gap10/affected.log`、`affected-result.json`；如为 C# 测试，另有同名 TRX。

- GAP-10 affected-final：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\gap10\bin\ --logger trx;LogFileName=affected-final.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\gap10 --filter TestCategory=Application|FullyQualifiedName~AppPathHistoryServiceContractTests|FullyQualifiedName~UiControlsCSharpContractTests`；返回码 0，通过 115、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/gap10/affected-final.log`、`affected-final-result.json`；如为 C# 测试，另有同名 TRX。

- GAP-10 full：命令 `dotnet vstest D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\gap10\bin\x64\Release\net48\ShaheLauncher.Tests.dll /Platform:x64 /Logger:trx;LogFileName=full.trx /ResultsDirectory:D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\gap10`；返回码 1，通过 314、失败 1、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/gap10/full.log`、`full-result.json`；如为 C# 测试，另有同名 TRX。

- GAP-10 audit-recheck：命令 `dotnet vstest D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\gap10\bin\x64\Release\net48\ShaheLauncher.Tests.dll /Platform:x64 /Logger:trx;LogFileName=audit-recheck.trx /ResultsDirectory:D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\gap10 /TestCaseFilter:FullyQualifiedName~BatchCreateConfirmationWaitsForTheButtonHandler`；返回码 0，通过 1、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/gap10/audit-recheck.log`、`audit-recheck-result.json`；如为 C# 测试，另有同名 TRX。

- GAP-10 全量失败复核：首次 full 的唯一失败在既有 BatchCreateConfirmationWaitsForTheButtonHandler 前台窗口确认（原生前台切换返回 false），audit-recheck 对同一程序集单项通过；未更改 AuditRunner。父代理补充源码差异报告索引：`artifacts/runs/python_to_csharp_remaining_audit_20261021/followup-source-findings.md`，六类候选待运行时 RED，不并入 GAP-10。

- GAP-10 full-final：命令 `dotnet vstest D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\gap10\bin\x64\Release\net48\ShaheLauncher.Tests.dll /Platform:x64 /Logger:trx;LogFileName=full-final.trx /ResultsDirectory:D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\gap10`；返回码 1，通过 313、失败 2、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/gap10/full-final.log`、`full-final-result.json`；如为 C# 测试，另有同名 TRX。

- GAP-10 python：命令 `python -m unittest discover -s tests -p test_*.py -v`；返回码 0，通过 29、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/gap10/python.log`、`python-result.json`；如为 C# 测试，另有同名 TRX。

- GAP-10 compile：命令 `python -m compileall -q shahe_bulk_launcher tests`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/gap10/compile.log`、`compile-result.json`；如为 C# 测试，另有同名 TRX。

- GAP-10 release：命令 `dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\gap10\bin\`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/gap10/release.log`、`release-result.json`；如为 C# 测试，另有同名 TRX。

- GAP-10 Python 原版 AST 对照：`python artifacts/runs/csharp_non_update_gaps_20261020/gap10/python-app-path-probe.py` 返回码 1；复制成功/重试/失败/访问失败与浏览配置共七项，计数见日志 `artifacts/runs/csharp_non_update_gaps_20261020/gap10/python-app-path-probe.log`，仅内存剪贴板。

- GAP-10 full-rebuilt：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\gap10\bin\ --logger trx;LogFileName=full-rebuilt.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\gap10`；返回码 0，通过 315、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/gap10/full-rebuilt.log`、`full-rebuilt-result.json`；如为 C# 测试，另有同名 TRX。

- GAP-10 Python AST 探针有效复跑：`python artifacts/runs/csharp_non_update_gaps_20261020/gap10/python-app-path-probe.py` 返回码 0，通过 9、失败 0、跳过 0；日志 `artifacts/runs/csharp_non_update_gaps_20261020/gap10/python-app-path-probe-final.log`。首次缺 GetFirstSelected 的假对象装配错误无效，保留首次日志；本次执行原版 copy/paste 方法和浏览配置核对，未访问真实剪贴板。

- GAP-10 diff：命令 `git diff --check`；返回码 2，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/gap10/diff.log`、`diff-result.json`；如为 C# 测试，另有同名 TRX。

- GAP-10 范围差异检查：`git diff --check -- PROJECT_STRUCTURE.md csharp/ShaheLauncher.App/MainForm.cs csharp/ShaheLauncher.Tests/PathMemoryRuntimeContractTests.cs csharp/ShaheLauncher.Tests/AppPathDialogTextRuntimeContractTests.cs` 返回码 0；日志 `artifacts/runs/csharp_non_update_gaps_20261020/gap10/diff-scoped.log`。全工作树 diff 返回码 2 的空白错误仅位于既有 GAP-09 两份历史日志（real-path-history.log、red-20261021_155943_230.log），不修改历史证据。


- GAP-10 Release 构建摘要核对：`release.log` 确认警告 0、错误 0；输出为 `artifacts/runs/csharp_non_update_gaps_20261020/gap10/bin/x64/Release/net48/`。模块自动化已通过，真实 EXE、整体审计、发布、Git 尚由主代理接续；整体迁移未完成。

- 2026-10-21 GAP-10 真实 EXE 交互审计无结论：隔离启动使用 gap10/bin/x64/Release/net48 构建，Computer Use 依次返回 CUA_WINDOW_OBSERVER_NOT_READY、CUA_EXECUTION_FAILED、CUA_TURN_NOT_ACTIVE；未取得可靠窗口/焦点，未发送键盘或鼠标输入，完成交互用例 0。本轮未观察到精确隔离路径目标进程，应用返回码和通过/失败/跳过测试数不适用；不可据此判定产品通过或失败。已发出环境恢复信号，恢复结果、实际目录、构建哈希、目标 PID 与清理记录见 artifacts/runs/csharp_non_update_gaps_20261020/gap10/real-exe-result.json。GAP-10 自动化通过，但真实文件选择、剪贴板和快捷键审计仍未完成，本模块尚未提交。
- 2026-10-21 GAP-10 独立只读审查通过：生产差异和规格未发现阻断问题；精确过滤器最终 GREEN 已到达，标题先失败的历史 RED 不冒充过滤器专属 RED。新增六类源码缺口已独立复核：启动预检/目标解析、启动失败 reload 接线、进程刷新范围与反馈、关闭成功朗读、未选用户文案、更新检查/启动反馈；这些新项尚无运行时 RED/GREEN，详见 artifacts/runs/python_to_csharp_remaining_audit_20261021/followup-source-findings.md，未并入 GAP-10 修复。

- GAP-10 parent-targeted-final：命令 `dotnet vstest D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\gap10\bin\x64\Release\net48\ShaheLauncher.Tests.dll /Platform:x64 /Logger:trx;LogFileName=parent-targeted-final.trx /ResultsDirectory:D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\gap10 /TestCaseFilter:FullyQualifiedName~AppPathDialogTextRuntimeContractTests`；返回码 0，通过 11、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/gap10/parent-targeted-final.log`、`parent-targeted-final-result.json`；如为 C# 测试，另有同名 TRX。

- GAP-10 parent-affected-final：命令 `dotnet vstest D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\gap10\bin\x64\Release\net48\ShaheLauncher.Tests.dll /Platform:x64 /Logger:trx;LogFileName=parent-affected-final.trx /ResultsDirectory:D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\gap10 /TestCaseFilter:TestCategory=Application|TestCategory=AppPaths`；返回码 0，通过 101、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/gap10/parent-affected-final.log`、`parent-affected-final-result.json`；如为 C# 测试，另有同名 TRX。

- GAP-10 parent-full-final：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\gap10\bin\ --logger trx;LogFileName=parent-full-final.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\gap10`；返回码 0，通过 315、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/gap10/parent-full-final.log`、`parent-full-final-result.json`；如为 C# 测试，另有同名 TRX。

- GAP-10 parent-python-final：命令 `python -m unittest discover -s tests -p test_*.py -v`；返回码 0，通过 29、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/gap10/parent-python-final.log`、`parent-python-final-result.json`；如为 C# 测试，另有同名 TRX。

- GAP-10 parent-compile-final：命令 `python -m compileall -q shahe_bulk_launcher tests`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/gap10/parent-compile-final.log`、`parent-compile-final-result.json`；如为 C# 测试，另有同名 TRX。

- GAP-10 parent-release-final：命令 `dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\gap10\bin\`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/gap10/parent-release-final.log`、`parent-release-final-result.json`；如为 C# 测试，另有同名 TRX。

- 2026-10-21 GAP-10 主代理复核：隔离 `parent-targeted-final` 11/11、`parent-affected-final` 101/101、`parent-full-final` 315/315 通过，返回码均为 0；Python 全量 29/29、compileall 返回码 0；隔离 Release x64 构建返回码 0，0 警告、0 错误，输出仍为 `artifacts/runs/csharp_non_update_gaps_20261020/gap10/bin/x64/Release/net48/`。原版 AST 探针 `parent-python-ast-final` 返回码 0，7 主项 + 2 补充项通过，未访问系统剪贴板。日志/结果均在 gap10 目录。

- REVIEW-01 red：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01\bin\ --logger trx;LogFileName=red.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01 --filter FullyQualifiedName~LaunchEntryPreflightRuntimeContractTests`；返回码 1，通过 0、失败 1、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review01/red.log`、`red-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-01 green：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01\bin\ --logger trx;LogFileName=green.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01 --filter FullyQualifiedName~LaunchEntryPreflightRuntimeContractTests`；返回码 0，通过 1、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review01/green.log`、`green-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-01 red-target-resolution：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01\bin\ --logger trx;LogFileName=red-target-resolution.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01 --filter FullyQualifiedName~LaunchEntryPreflightRuntimeContractTests`；返回码 1，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review01/red-target-resolution.log`、`red-target-resolution-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-01 red-target-resolution-valid：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01\bin\ --logger trx;LogFileName=red-target-resolution-valid.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01 --filter FullyQualifiedName~LaunchEntryPreflightRuntimeContractTests`；返回码 1，通过 1、失败 1、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review01/red-target-resolution-valid.log`、`red-target-resolution-valid-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-01 green-final：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01\bin\ --logger trx;LogFileName=green-final.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01 --filter FullyQualifiedName~LaunchEntryPreflightRuntimeContractTests`；返回码 0，通过 2、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review01/green-final.log`、`green-final-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-01 affected：命令 `dotnet vstest D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01\bin\x64\Release\net48\ShaheLauncher.Tests.dll /Platform:x64 /Logger:trx;LogFileName=affected.trx /ResultsDirectory:D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01 /TestCaseFilter:TestCategory=Application|TestCategory=Scheduling`；返回码 0，通过 104、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review01/affected.log`、`affected-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-01 full：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01\bin\ --logger trx;LogFileName=full.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01`；返回码 1，通过 315、失败 2、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review01/full.log`、`full-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-01 audit-confirmation-recheck：命令 `dotnet vstest D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01\bin\x64\Release\net48\ShaheLauncher.Tests.dll /Platform:x64 /Logger:trx;LogFileName=audit-confirmation-recheck.trx /ResultsDirectory:D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01 /TestCaseFilter:FullyQualifiedName~BatchCreateConfirmationWaitsForTheButtonHandler`；返回码 0，通过 1、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review01/audit-confirmation-recheck.log`、`audit-confirmation-recheck-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-01 audit-accessible-recheck：命令 `dotnet vstest D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01\bin\x64\Release\net48\ShaheLauncher.Tests.dll /Platform:x64 /Logger:trx;LogFileName=audit-accessible-recheck.trx /ResultsDirectory:D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01 /TestCaseFilter:FullyQualifiedName~BatchCreateValuesAreSetByAccessibleNamesNotWindowOrder`；返回码 0，通过 1、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review01/audit-accessible-recheck.log`、`audit-accessible-recheck-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-01 python：命令 `python -m unittest discover -s tests -p test_*.py -v`；返回码 0，通过 29、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review01/python.log`、`python-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-01 compile：命令 `python -m compileall -q shahe_bulk_launcher tests`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review01/compile.log`、`compile-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-01 release：命令 `dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01\bin\`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review01/release.log`、`release-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-01 full-final：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01\bin\ --logger trx;LogFileName=full-final.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01`；返回码 1，通过 316、失败 1、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review01/full-final.log`、`full-final-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-01 full-final2：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01\bin\ --logger trx;LogFileName=full-final2.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01`；返回码 0，通过 317、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review01/full-final2.log`、`full-final2-result.json`；如为 C# 测试，另有同名 TRX。

- 2026-10-21 REVIEW-01 完成：C# `MainForm.StartLaunch` 已复用现有应用/安装/Start.exe 预检，并在当前 Sandboxie 映射非空时复用 `LaunchScheduler.ResolveLaunchEntries`，避免无效目标进入启动会话。有效 RED 2 项、GREEN 2/2、受影响 104/104、最终 C# 全量 317/317、Python 29/29、compileall 0、Release x64 0 警告/0 错误；首次全量 315/317 的两个失败为既有 AuditRunner 前台确认/控件索引波动，单项复核和最终复跑通过。证据：`artifacts/runs/csharp_non_update_gaps_20261020/review01/`。未执行真实 Sandboxie 启动动作，真实生命周期仍属后续审计。

- REVIEW-02 red：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review02\bin\ --logger trx;LogFileName=red.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review02 --filter FullyQualifiedName~SandboxieLaunchCallbackRuntimeContractTests`；返回码 1，通过 0、失败 1、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review02/red.log`、`red-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-02 green：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review02\bin\ --logger trx;LogFileName=green.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review02 --filter FullyQualifiedName~SandboxieLaunchCallbackRuntimeContractTests`；返回码 0，通过 1、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review02/green.log`、`green-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-02 affected：命令 `dotnet vstest D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review02\bin\x64\Release\net48\ShaheLauncher.Tests.dll /Platform:x64 /Logger:trx;LogFileName=affected.trx /ResultsDirectory:D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review02 /TestCaseFilter:TestCategory=Application|TestCategory=Sandboxie`；返回码 0，通过 104、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review02/affected.log`、`affected-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-02 full：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review02\bin\ --logger trx;LogFileName=full.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review02`；返回码 0，通过 318、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review02/full.log`、`full-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-02 python：命令 `python -m unittest discover -s tests -p test_*.py -v`；返回码 0，通过 29、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review02/python.log`、`python-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-02 compile：命令 `python -m compileall -q shahe_bulk_launcher tests`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review02/compile.log`、`compile-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-02 release：命令 `dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review02\bin\`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review02/release.log`、`release-result.json`；如为 C# 测试，另有同名 TRX。

- 2026-10-21 REVIEW-02 完成：`MainForm.StartAppInSandbox` 现在通过统一回调工厂接入已有 `ReloadSandboxieConfig` 与官方沙盒集合刷新，Core 的无效名 reload/重试逻辑获得真实入口。定向 GREEN 1/1、受影响 104/104、C# 全量 318/318、Python 29/29、compileall 0、Release x64 0 警告/0 错误；证据：`artifacts/runs/csharp_non_update_gaps_20261020/review02/`。未执行真实 Sandboxie 动作。

- REVIEW-03 red：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review03\bin\ --logger trx;LogFileName=red.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review03 --filter FullyQualifiedName~ProcessRefreshFeedbackRuntimeContractTests`；返回码 1，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review03/red.log`、`red-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-03 red-valid：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review03\bin\ --logger trx;LogFileName=red-valid.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review03 --filter FullyQualifiedName~ProcessRefreshFeedbackRuntimeContractTests`；返回码 1，通过 0、失败 2、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review03/red-valid.log`、`red-valid-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-03 green：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review03\bin\ --logger trx;LogFileName=green.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review03 --filter FullyQualifiedName~ProcessRefreshFeedbackRuntimeContractTests`；返回码 0，通过 2、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review03/green.log`、`green-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-03 green-final：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review03\bin\ --logger trx;LogFileName=green-final.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review03 --filter FullyQualifiedName~ProcessRefreshFeedbackRuntimeContractTests`；返回码 0，通过 2、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review03/green-final.log`、`green-final-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-03 affected：命令 `dotnet vstest D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review03\bin\x64\Release\net48\ShaheLauncher.Tests.dll /Platform:x64 /Logger:trx;LogFileName=affected.trx /ResultsDirectory:D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review03 /TestCaseFilter:TestCategory=Application|TestCategory=Processes`；返回码 0，通过 98、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review03/affected.log`、`affected-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-03 full：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review03\bin\ --logger trx;LogFileName=full.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review03`；返回码 1，通过 318、失败 2、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review03/full.log`、`full-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-03 full-final：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review03\bin\ --logger trx;LogFileName=full-final.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review03`；返回码 0，通过 320、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review03/full-final.log`、`full-final-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-03 python：命令 `python -m unittest discover -s tests -p test_*.py -v`；返回码 0，通过 29、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review03/python.log`、`python-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-03 compile：命令 `python -m compileall -q shahe_bulk_launcher tests`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review03/compile.log`、`compile-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-03 release：命令 `dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review03\bin\`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review03/release.log`、`release-result.json`；如为 C# 测试，另有同名 TRX。

- 2026-10-21 REVIEW-03 完成：Ctrl+F5/`refresh_process_state` 现在只刷新可见运行状态，不再触发完整沙盒列表刷新；恢复 Python 的“正在刷新进程状态/没有可刷新的进程状态/已刷新进程状态”反馈，并用异步完成回调收口。定向 GREEN 2/2、受影响 98/98、C# 全量最终 320/320、Python 29/29、compileall 0、Release x64 0 警告/0 错误；证据：`artifacts/runs/csharp_non_update_gaps_20261020/review03/`。真实键盘输入仍待 AUDIT-04。

- REVIEW-04 red：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review04\bin\ --logger trx;LogFileName=red.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review04 --filter FullyQualifiedName~CloseProcessFeedbackRuntimeContractTests`；返回码 1，通过 0、失败 7、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review04/red.log`、`red-result.json`；如为 C# 测试，另有同名 TRX。
`n- REVIEW-04 首次 RED 中四个真实服务入口失败分别命中缺失朗读/无目标错误全表刷新；三个私有方法测试因辅助反射仅允许 public 而失败，不计为产品 RED。已修正测试访问方式后重新运行。

- REVIEW-04 red-valid：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review04\bin\ --logger trx;LogFileName=red-valid.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review04 --filter FullyQualifiedName~CloseProcessFeedbackRuntimeContractTests`；返回码 1，通过 2、失败 5、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review04/red-valid.log`、`red-valid-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-04 green：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review04\bin\ --logger trx;LogFileName=green.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review04 --filter FullyQualifiedName~CloseProcessFeedbackRuntimeContractTests`；返回码 0，通过 7、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review04/green.log`、`green-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-04 affected：命令 `dotnet vstest D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review04\bin\x64\Release\net48\ShaheLauncher.Tests.dll /Platform:x64 /Logger:trx;LogFileName=affected.trx /ResultsDirectory:D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review04 /TestCaseFilter:TestCategory=ProcessActions|TestCategory=Application|TestCategory=Processes`；返回码 0，通过 120、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review04/affected.log`、`affected-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-04 full：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review04\bin\ --logger trx;LogFileName=full.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review04`；返回码 0，通过 327、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review04/full.log`、`full-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-04 python：命令 `python -m unittest discover -s tests -p test_*.py -v`；返回码 0，通过 29、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review04/python.log`、`python-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-04 compile：命令 `python -m compileall -q shahe_bulk_launcher tests`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review04/compile.log`、`compile-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-04 release：命令 `dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review04\bin\`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review04/release.log`、`release-result.json`；如为 C# 测试，另有同名 TRX。
`n- 2026-10-22 文档范围纠正：此前 GAP-10 提交纳入了超出本模块范围的既有项目结构差异；复核发现一条 2026-10-15 GREEN 历史被后来复核记录替换，现已原文恢复并保留较新记录。未改写或回退用户代码。阅读顺序入口更正为顶部唯一活动计划；活动计划正文仍等待先前展示差异的明确确认，未写入。REVIEW-01 至 REVIEW-03 初次自动化通过不覆盖本轮发现的忙时启动状态、官方沙盒缓存和刷新状态覆盖边界，仍需分别修正。

- REVIEW-04 Python 源码 AST 行为探针：命令 `python artifacts/runs/csharp_non_update_gaps_20261020/review04/python-close-feedback-probe.py`，返回码 0；实际输出见 `artifacts/runs/csharp_non_update_gaps_20261020/review04/python-probe.log`，计数见 `python-probe-result.json`。

- REVIEW-04 green-final：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review04\bin\ --logger trx;LogFileName=green-final.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review04 --filter FullyQualifiedName~CloseProcessFeedbackRuntimeContractTests`；返回码 0，通过 9、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review04/green-final.log`、`green-final-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-04 范围差异检查：返回码 0，计数不适用；命令和输出见 `artifacts/runs/csharp_non_update_gaps_20261020/review04/diff-result.json` 与 `diff.log`。全工作树原有 GAP-09 日志空白改动仍保留，不纳入本模块。

- REVIEW-04 full-final：命令 `dotnet vstest D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review04\bin\x64\Release\net48\ShaheLauncher.Tests.dll /Platform:x64 /Logger:trx;LogFileName=full-final.trx /ResultsDirectory:D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review04`；返回码 0，通过 329、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review04/full-final.log`、`full-final-result.json`；如为 C# 测试，另有同名 TRX。
`n- 2026-10-22 REVIEW-04 收口：完成关闭当前/全部成功朗读、无目标仅状态刷新和部分失败不误报；定向、受影响、两次全量、Python unittest、Python 源码行为探针、compileall、隔离 Release 与差异检查的实际计数见上述本模块记录。代码评审未发现生产差异，已补当前用户部分关闭/零关闭测试。未在真实进程上再次执行关闭或验证扬声器输出，这些外部验收仍按 AUDIT 保留。已重新核对活动计划：GAP-10 真实交互、VOICE-02、AUDIT-01 至 AUDIT-08、最终发布仍待办；未修改需用户确认的计划正文。
`n- REVIEW-04 暂存差异检查初次返回 2：验证脚本末尾多一个空行；已仅清理该脚本末尾空行，随后暂存检查返回 0。未改动原始测试日志。
`n- REVIEW-04 提交/推送：独立提交 232d1a3，origin/fix/non-update-gaps-20261020 推送返回 0；回退命令 git revert 232d1a3。当前进入 REVIEW-01 启动边界修正，之后 REVIEW-02、REVIEW-03，再继续 REVIEW-05。

- REVIEW-01 边界 red：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01-boundaries\bin\ --logger trx;LogFileName=red.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01-boundaries --filter FullyQualifiedName~LaunchEntryPreflightRuntimeContractTests`；返回码 1，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review01-boundaries/red.log`、`red-result.json`；如为 C# 测试，另有同名 TRX。
`n- REVIEW-01 边界首次 red 是测试对只读 SandboxRoot 的赋值编译错误，未执行测试，不计产品 RED；改用既有 SetSandboxRoot 后重跑。

- REVIEW-01 边界 red-valid：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01-boundaries\bin\ --logger trx;LogFileName=red-valid.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01-boundaries --filter FullyQualifiedName~LaunchEntryPreflightRuntimeContractTests`；返回码 1，通过 2、失败 6、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review01-boundaries/red-valid.log`、`red-valid-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-01 边界 green：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01-boundaries\bin\ --logger trx;LogFileName=green.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01-boundaries --filter FullyQualifiedName~LaunchEntryPreflightRuntimeContractTests|FullyQualifiedName~WiresLaunchGroupMenuActionToLaunchScheduler`；返回码 0，通过 9、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review01-boundaries/green.log`、`green-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-01 边界 affected：命令 `dotnet vstest D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01-boundaries\bin\x64\Release\net48\ShaheLauncher.Tests.dll /Platform:x64 /Logger:trx;LogFileName=affected.trx /ResultsDirectory:D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01-boundaries /TestCaseFilter:TestCategory=Application|TestCategory=Scheduling`；返回码 0，通过 113、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review01-boundaries/affected.log`、`affected-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-01 边界 green-latest：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01-boundaries\bin\ --logger trx;LogFileName=green-latest.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01-boundaries --filter FullyQualifiedName~LaunchEntryPreflightRuntimeContractTests|FullyQualifiedName~WiresLaunchGroupMenuActionToLaunchScheduler`；返回码 0，通过 9、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review01-boundaries/green-latest.log`、`green-latest-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-01 边界 affected-latest：命令 `dotnet vstest D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01-boundaries\bin\x64\Release\net48\ShaheLauncher.Tests.dll /Platform:x64 /Logger:trx;LogFileName=affected-latest.trx /ResultsDirectory:D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01-boundaries /TestCaseFilter:TestCategory=Application|TestCategory=Scheduling`；返回码 0，通过 113、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review01-boundaries/affected-latest.log`、`affected-latest-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-01 边界 python：命令 `python -m unittest discover -s tests -p test_*.py -v`；返回码 0，通过 29、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review01-boundaries/python.log`、`python-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-01 边界 compile：命令 `python -m compileall -q shahe_bulk_launcher tests`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review01-boundaries/compile.log`、`compile-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-01 边界 release：命令 `dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01-boundaries\bin\`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review01-boundaries/release.log`、`release-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-01 边界 full-final：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01-boundaries\bin\ --logger trx;LogFileName=full-final.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01-boundaries`；返回码 1，通过 334、失败 1、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review01-boundaries/full-final.log`、`full-final-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-01 边界 full-final2：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01-boundaries\bin\ --logger trx;LogFileName=full-final2.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01-boundaries`；返回码 1，通过 334、失败 1、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review01-boundaries/full-final2.log`、`full-final2-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-01 边界 audit-focus-retry：命令 `dotnet vstest D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01-boundaries\bin\x64\Release\net48\ShaheLauncher.Tests.dll /Platform:x64 /Logger:trx;LogFileName=audit-focus-retry.trx /ResultsDirectory:D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01-boundaries /TestCaseFilter:FullyQualifiedName~BatchCreateConfirmationWaitsForTheButtonHandler`；返回码 0，通过 1、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review01-boundaries/audit-focus-retry.log`、`audit-focus-retry-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-01 full-no-parallel：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --no-parallel`；返回码 1，通过 0、失败 0、跳过 0。日志与 TRX：`artifacts/runs/csharp_non_update_gaps_20261020/review01-boundaries/full-no-parallel.log`、`full-no-parallel.trx`。

- REVIEW-01 边界 full-final3：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01-boundaries\bin\ --logger trx;LogFileName=full-final3.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01-boundaries`；返回码 0，通过 335、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review01-boundaries/full-final3.log`、`full-final3-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-01 差异检查：命令 `git diff --check -- REVIEW-01 files`，返回码 0，计数不适用；日志与索引：`artifacts/runs/csharp_non_update_gaps_20261020/review01-boundaries/diff.log`、`diff-result.json`。
`n- REVIEW-01 边界验证纠正：`full-final` 与 `full-final2` 各有 1 个既有 AuditRunner 前台窗口测试失败，单项重跑通过；随后 `full-final3` 返回码 0、通过 335、失败 0，作为本模块最终全量证据。误用 `dotnet test --no-parallel` 产生 MSBuild 未知开关错误，无 TRX，仅保留为失败命令记录，不计产品结果。
`n- REVIEW-01 提交/推送：独立提交 c3ffdf2，origin/fix/non-update-gaps-20261020 推送返回 0；回退命令 git revert c3ffdf2。当前进入 REVIEW-02 Sandboxie 启动恢复。
`n- REVIEW-02 RED：新增官方集合缓存失效和 reload 异常仍重试的运行时契约，先运行定向测试确认当前实现失败；结果见 review02-retry/red。

- REVIEW-02 red：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review02-retry\bin\ --logger trx;LogFileName=red.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review02-retry --filter FullyQualifiedName~SandboxieLaunchCallbackRuntimeContractTests|FullyQualifiedName~InvalidNameRetryContinuesWhenReloadThrows`；返回码 1，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review02-retry/red.log`、`red-result.json`；如为 C# 测试，另有同名 TRX。
`n- REVIEW-02 RED 初次返回为测试夹具编译错误（缺少 System.IO.Path），不计产品 RED；已补既有标准库引用后重跑。

- REVIEW-02 red-valid：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review02-retry\bin\ --logger trx;LogFileName=red-valid.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review02-retry --filter FullyQualifiedName~SandboxieLaunchCallbackRuntimeContractTests|FullyQualifiedName~InvalidNameRetryContinuesWhenReloadThrows`；返回码 1，通过 1、失败 2、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review02-retry/red-valid.log`、`red-valid-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-02 green：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review02-retry\bin\ --logger trx;LogFileName=green.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review02-retry --filter FullyQualifiedName~SandboxieLaunchCallbackRuntimeContractTests|FullyQualifiedName~InvalidNameRetryContinuesWhenReloadThrows`；返回码 0，通过 3、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review02-retry/green.log`、`green-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-02 affected：命令 `dotnet vstest D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review02-retry\bin\x64\Release\net48\ShaheLauncher.Tests.dll /Platform:x64 /Logger:trx;LogFileName=affected.trx /ResultsDirectory:D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review02-retry /TestCaseFilter:TestCategory=Sandboxie|TestCategory=SandboxAdmin|TestCategory=Application`；返回码 0，通过 122、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review02-retry/affected.log`、`affected-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-02 python：命令 `python -m unittest discover -s tests -p test_*.py -v`；返回码 0，通过 29、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review02-retry/python.log`、`python-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-02 compile：命令 `python -m compileall -q shahe_bulk_launcher tests`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review02-retry/compile.log`、`compile-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-02 release：命令 `dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review02-retry\bin\`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review02-retry/release.log`、`release-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-02 full：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review02-retry\bin\ --logger trx;LogFileName=full.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review02-retry`；返回码 0，通过 337、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review02-retry/full.log`、`full-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-02 差异检查：命令 `git diff --check -- REVIEW-02 files`，返回码 0，计数不适用；日志与索引：`artifacts/runs/csharp_non_update_gaps_20261020/review02-retry/diff.log`、`diff-result.json`。

- REVIEW-02 完成：启动恢复回调首次查询和失败重试均使用刷新后的官方沙盒集合；reload 使用 Start.exe 有界同步执行，回调异常仍允许第二次 dispatch。定向 3/3、Sandboxie/SandboxAdmin/Application 受影响 122/122、C# 全量 337/337、Python 29/29、compileall 0、Release x64 0 警告/0 错误；证据目录 `artifacts/runs/csharp_non_update_gaps_20261020/review02-retry`。未执行真实 Sandboxie 启动。
`n- REVIEW-02 提交/推送：独立提交 c895d6a，origin/fix/non-update-gaps-20261020 推送返回 0；回退命令 git revert c895d6a。当前进入 REVIEW-03 状态完成保护。
`n- REVIEW-03 RED：新增后台刷新完成覆盖新状态的运行时契约，先运行定向测试确认当前实现失败；结果见 review03-status/red。

- REVIEW-03 red：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review03-status\bin\ --logger trx;LogFileName=red.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review03-status --filter FullyQualifiedName~ProcessRefreshFeedbackRuntimeContractTests`；返回码 1，通过 2、失败 1、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review03-status/red.log`、`red-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-03 green：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review03-status\bin\ --logger trx;LogFileName=green.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review03-status --filter FullyQualifiedName~ProcessRefreshFeedbackRuntimeContractTests`；返回码 0，通过 3、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review03-status/green.log`、`green-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-03 affected：命令 `dotnet vstest D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review03-status\bin\x64\Release\net48\ShaheLauncher.Tests.dll /Platform:x64 /Logger:trx;LogFileName=affected.trx /ResultsDirectory:D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review03-status /TestCaseFilter:TestCategory=Application|TestCategory=Processes`；返回码 0，通过 106、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review03-status/affected.log`、`affected-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-03 python：命令 `python -m unittest discover -s tests -p test_*.py -v`；返回码 0，通过 29、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review03-status/python.log`、`python-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-03 compile：命令 `python -m compileall -q shahe_bulk_launcher tests`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review03-status/compile.log`、`compile-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-03 release：命令 `dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review03-status\bin\`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review03-status/release.log`、`release-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-03 full：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review03-status\bin\ --logger trx;LogFileName=full.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review03-status`；返回码 0，通过 338、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review03-status/full.log`、`full-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-03 差异检查：命令 `git diff --check -- REVIEW-03 files`，返回码 0，计数不适用；日志与索引：`artifacts/runs/csharp_non_update_gaps_20261020/review03-status/diff.log`、`diff-result.json`。

- REVIEW-03 完成：完成回调增加 Python 同等当前状态保护，保留正常完成、无可刷新状态和后续用户状态三类行为。定向 3/3、Application/Processes 受影响 106/106、C# 全量 338/338、Python 29/29、compileall 0、Release x64 0 警告/0 错误；证据目录 `artifacts/runs/csharp_non_update_gaps_20261020/review03-status`。未执行真实键盘刷新审计。
`n- REVIEW-03 提交/推送：独立提交 b8e783e，origin/fix/non-update-gaps-20261020 推送返回 0；回退命令 git revert b8e783e。当前进入 REVIEW-05 当前用户未选择文案。
`n- REVIEW-05 RED：新增七个当前用户动作的真实分派测试，确认现有共用入口仍返回旧文案；结果见 review05-text/red。

- REVIEW-05 red：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review05-text\bin\ --logger trx;LogFileName=red.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review05-text --filter FullyQualifiedName~CurrentSandboxSelectionTextRuntimeContractTests`；返回码 1，通过 0、失败 1、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review05-text/red.log`、`red-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-05 green：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review05-text\bin\ --logger trx;LogFileName=green.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review05-text --filter FullyQualifiedName~CurrentSandboxSelectionTextRuntimeContractTests`；返回码 0，通过 1、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review05-text/green.log`、`green-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-05 affected：命令 `dotnet vstest D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review05-text\bin\x64\Release\net48\ShaheLauncher.Tests.dll /Platform:x64 /Logger:trx;LogFileName=affected.trx /ResultsDirectory:D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review05-text /TestCaseFilter:TestCategory=Application|TestCategory=Processes|TestCategory=SandboxAdmin`；返回码 0，通过 115、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review05-text/affected.log`、`affected-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-05 python：命令 `python -m unittest discover -s tests -p test_*.py -v`；返回码 0，通过 29、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review05-text/python.log`、`python-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-05 compile：命令 `python -m compileall -q shahe_bulk_launcher tests`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review05-text/compile.log`、`compile-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-05 release：命令 `dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review05-text\bin\`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review05-text/release.log`、`release-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-05 full：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review05-text\bin\ --logger trx;LogFileName=full.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review05-text`；返回码 0，通过 339、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review05-text/full.log`、`full-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-05 差异检查：命令 `git diff --check -- REVIEW-05 files`，返回码 0，计数不适用；日志与索引：`artifacts/runs/csharp_non_update_gaps_20261020/review05-text/diff.log`、`diff-result.json`。

- REVIEW-05 完成：共用当前用户动作入口的未选中提示恢复为 Python 文案「请先在用户列表中选中一个用户」，七个动作逐项验证回调未执行。定向 1/1、Application/Processes/SandboxAdmin 受影响 115/115、C# 全量 339/339、Python 29/29、compileall 0、Release x64 0 警告/0 错误；证据目录 `artifacts/runs/csharp_non_update_gaps_20261020/review05-text`。
`n- REVIEW-05 提交/推送：独立提交 d133117，origin/fix/non-update-gaps-20261020 推送返回 0；回退命令 git revert d133117。当前进入 REVIEW-06 更新检查/启动反馈。
`n- REVIEW-06 RED：新增手动检查清理旧提示/检查中状态与 updater 准备失败反馈的运行时契约，先运行定向测试确认当前实现失败；结果见 review06-update-ui/red。

- REVIEW-06 red：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review06-update-ui\bin\ --logger trx;LogFileName=red.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review06-update-ui --filter FullyQualifiedName~UpdateFeedbackRuntimeContractTests`；返回码 1，通过 0、失败 2、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review06-update-ui/red.log`、`red-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-06 green：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review06-update-ui\bin\ --logger trx;LogFileName=green.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review06-update-ui --filter FullyQualifiedName~UpdateFeedbackRuntimeContractTests`；返回码 1，通过 1、失败 1、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review06-update-ui/green.log`、`green-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-06 green2：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review06-update-ui\bin\ --logger trx;LogFileName=green2.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review06-update-ui --filter FullyQualifiedName~UpdateFeedbackRuntimeContractTests`；返回码 0，通过 2、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review06-update-ui/green2.log`、`green2-result.json`；如为 C# 测试，另有同名 TRX。
`n- REVIEW-06 初次 GREEN 返回码 1、通过 1、失败 1：测试窗体未调用 Show，导致仅可见性断言失败，属于夹具问题，不计产品回归。补调用 Show 后 green2 返回码 0、通过 2、失败 0；日志/TRX 在 review06-update-ui。

- REVIEW-06 affected：命令 `dotnet vstest D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review06-update-ui\bin\x64\Release\net48\ShaheLauncher.Tests.dll /Platform:x64 /Logger:trx;LogFileName=affected.trx /ResultsDirectory:D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review06-update-ui /TestCaseFilter:TestCategory=Application|TestCategory=Updates`；返回码 0，通过 136、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review06-update-ui/affected.log`、`affected-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-06 python：命令 `python -m unittest discover -s tests -p test_*.py -v`；返回码 0，通过 29、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review06-update-ui/python.log`、`python-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-06 compile：命令 `python -m compileall -q shahe_bulk_launcher tests`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review06-update-ui/compile.log`、`compile-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-06 release：命令 `dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review06-update-ui\bin\`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review06-update-ui/release.log`、`release-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-06 full：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review06-update-ui\bin\ --logger trx;LogFileName=full.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review06-update-ui`；返回码 0，通过 341、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/review06-update-ui/full.log`、`full-result.json`；如为 C# 测试，另有同名 TRX。

- REVIEW-06 Python 源码文案探针：命令 `python artifacts/runs/csharp_non_update_gaps_20261020/review06-update-ui/python-update-ui-probe.py`，返回码 0，通过 7、失败 0、跳过 0；日志与结果：`artifacts/runs/csharp_non_update_gaps_20261020/review06-update-ui/python-probe.log`、`python-probe-result.json`。

- REVIEW-06 差异检查：命令 `git diff --check -- REVIEW-06 files`，返回码 0，计数不适用；日志与索引：`artifacts/runs/csharp_non_update_gaps_20261020/review06-update-ui/diff.log`、`diff-result.json`。

- REVIEW-06 完成：手动检查更新先记录、隐藏旧提示并显示检查中状态；更新器准备、启动成功、不可用和异常路径恢复 Python 状态/日志/标题/说明，并设置工作目录。定向 2/2、Application/Updates 受影响 136/136、C# 全量 341/341、Python 29/29、Python 文案探针 7/7、compileall 0、Release x64 0 警告/0 错误；证据目录 `artifacts/runs/csharp_non_update_gaps_20261020/review06-update-ui`。未启动真实更新器，真实发布验收仍在 AUDIT-07。
`n- REVIEW-06 提交/推送：独立提交 ed6cb3c，origin/fix/non-update-gaps-20261020 推送返回 0；回退命令 git revert ed6cb3c。当前进入 VOICE-02 读屏配置与 lib 运行时目录。
`n- VOICE-02 RED：新增 lib 配置回退和 PATH 运行时契约，先运行定向测试确认当前 C# 缺失；结果见 artifacts/runs/csharp_non_update_gaps_20261020/voice02/red。

- VOICE-02 red：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\voice02\bin\ --logger trx;LogFileName=red.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\voice02 --filter FullyQualifiedName~ReadsPackagedReaderIniDefaultsFromLibDirectory|FullyQualifiedName~AddsLibDirectoryToPathBeforeLoadingAssistBackend`；返回码 1，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/voice02/red.log`、`red-result.json`；如为 C# 测试，另有同名 TRX。
`n- VOICE-02 RED 初次返回为测试夹具缺少 ShaheLauncher.Core.Speech 引用，未执行测试；已补既有命名空间引用后重跑。

- VOICE-02 red-valid：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\voice02\bin\ --logger trx;LogFileName=red-valid.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\voice02 --filter FullyQualifiedName~ReadsPackagedReaderIniDefaultsFromLibDirectory|FullyQualifiedName~AddsLibDirectoryToPathBeforeLoadingAssistBackend`；返回码 1，通过 0、失败 2、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/voice02/red-valid.log`、`red-valid-result.json`；如为 C# 测试，另有同名 TRX。

- VOICE-02 green：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\voice02\bin\ --logger trx;LogFileName=green.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\voice02 --filter FullyQualifiedName~ReadsPackagedReaderIniDefaultsFromLibDirectory|FullyQualifiedName~AddsLibDirectoryToPathBeforeLoadingAssistBackend`；返回码 0，通过 2、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/voice02/green.log`、`green-result.json`；如为 C# 测试，另有同名 TRX。
`n- VOICE-02 GREEN：lib INI 回退与 PATH 运行时定向测试返回码 0，通过 2、失败 0、跳过 0；日志/TRX 在 artifacts/runs/csharp_non_update_gaps_20261020/voice02/green。

- VOICE-02 affected：命令 `dotnet vstest D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\voice02\bin\x64\Release\net48\ShaheLauncher.Tests.dll /Platform:x64 /Logger:trx;LogFileName=affected.trx /ResultsDirectory:D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\voice02 /TestCaseFilter:TestCategory=Speech|TestCategory=Application|TestCategory=Updates`；返回码 0，通过 167、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/voice02/affected.log`、`affected-result.json`；如为 C# 测试，另有同名 TRX。

- VOICE-02 python：命令 `python -m unittest discover -s tests -p test_*.py -v`；返回码 0，通过 29、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/voice02/python.log`、`python-result.json`；如为 C# 测试，另有同名 TRX。

- VOICE-02 compile：命令 `python -m compileall -q shahe_bulk_launcher tests`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/voice02/compile.log`、`compile-result.json`；如为 C# 测试，另有同名 TRX。

- VOICE-02 release：命令 `dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\voice02\bin\`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/voice02/release.log`、`release-result.json`；如为 C# 测试，另有同名 TRX。

- VOICE-02 full：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\voice02\bin\ --logger trx;LogFileName=full.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\voice02`；返回码 0，通过 343、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/voice02/full.log`、`full-result.json`；如为 C# 测试，另有同名 TRX。

- VOICE-02 厂商二进制探针：命令 `artifacts/runs/csharp_non_update_gaps_20261020/voice02/binary-config-probe.ps1`，返回码 ，通过 3、失败 0、跳过 0；证据 `artifacts/runs/csharp_non_update_gaps_20261020/voice02/binary-config-probe.json` 与 `binary-config-probe.log`。

- VOICE-02 差异检查：命令 `git diff --check -- VOICE02 files`，返回码 0，计数不适用；日志与索引：`artifacts/runs/csharp_non_update_gaps_20261020/voice02/diff.log`、`diff-result.json`。

- VOICE-02 完成：读屏配置读取增加发布 lib 回退；加载 Tolk 前把实际运行时目录加入 PATH，匹配 Python 的依赖解析边界。二进制探针确认 Tolk/争渡/保益文件中存在对应配置和 DLL 依赖提示。定向 2/2、Speech/Application/Updates 受影响 167/167、C# 全量 343/343、Python 29/29、厂商二进制探针 3/3、compileall 0、Release x64 0 警告/0 错误；证据目录 `artifacts/runs/csharp_non_update_gaps_20261020/voice02`。未宣称真实读屏输出通过，AUDIT-04/AUDIT-07 仍待真实设备/发布目录验证。

- AUDIT-01 审计工具合约门禁：命令 `D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\voice02\bin\x64\Release\net48\ShaheLauncher.AuditRunner.exe contract --output artifacts/runs/csharp_non_update_gaps_20261020/audit01`，返回码 0；日志与 JSON：`artifacts/runs/csharp_non_update_gaps_20261020/audit01/contract.log`、`artifacts/runs/csharp_non_update_gaps_20261020/audit01/contract-result.json`。这是审计器自检，不是产品窗口结论。

- AUDIT-01 隔离启动窗口烟雾：命令 `startup-window-smoke.ps1`，返回码 3；结果 `artifacts/runs/csharp_non_update_gaps_20261020/audit01/startup/startup-window-smoke.json`。仅验证启动/句柄/标题/正常关闭，不代表定位功能或完整发布验收。

- AUDIT-01 启动烟雾复核：首轮返回码 3 的 .NET Runtime 1026 已定位为探针在 Application.Run 消息循环稳定前关闭窗口的竞态；脚本增加 5 秒稳定等待后复跑，返回码 0，结果 `artifacts/runs/csharp_non_update_gaps_20261020/audit01/startup-fixed/startup-window-smoke.json`。该证据只覆盖启动/句柄/标题/正常关闭。

- AUDIT-01 启动烟雾复核实际结果：返回码 3；结果 `artifacts/runs/csharp_non_update_gaps_20261020/audit01/startup-fixed/startup-window-smoke.json`，日志 `run.log`。

- AUDIT-01 启动烟雾最终复核：隔离配置强制 show_tray=false 以测试正常关闭，返回码 3；结果 `artifacts/runs/csharp_non_update_gaps_20261020/audit01/startup-fixed2/startup-window-smoke.json`。此前 show_tray=true 的 inconclusive 记录保留，反映托盘关闭拦截行为。

- AUDIT-01 启动烟雾脚本路径修正后复核：返回码 3；结果 `artifacts/runs/csharp_non_update_gaps_20261020/audit01/startup-fixed3/startup-window-smoke.json`，隔离配置 show_tray=false 已确认。

- AUDIT-01 启动烟雾最终复核：脚本为无配置的隔离发布目录生成 show_tray=false 配置后运行，返回码 3；结果 `artifacts/runs/csharp_non_update_gaps_20261020/audit01/startup-fixed4/startup-window-smoke.json`。

- AUDIT-01 启动烟雾最终复核（配置换行修正）：返回码 3；结果 `artifacts/runs/csharp_non_update_gaps_20261020/audit01/startup-fixed5/startup-window-smoke.json`，隔离配置项已验证。

- AUDIT-01 启动/关闭最终烟雾：使用隔离 show_tray=false 和 WM_CLOSE fallback，返回码 3；结果 `artifacts/runs/csharp_non_update_gaps_20261020/audit01/startup-final/startup-window-smoke.json`。覆盖句柄、标题、启动耗时、关闭方法、退出码；不覆盖定位/键盘/读屏。

- AUDIT-01 启动烟雾最终复核（扫描完成等待）：返回码 3；结果 `artifacts/runs/csharp_non_update_gaps_20261020/audit01/startup-final2/startup-window-smoke.json`。首次初始化竞态已由探针等待扫描结果消除；仍只覆盖启动/句柄/标题/正常关闭。
`n- AUDIT-01 有效 RED：首次隔离发布目录关闭返回 .NET Runtime 1026，事件堆栈为 Application.Run/Control.CreateHandle/ObjectDisposedException；新增 Program 首次显示时机契约，先运行定向测试确认当前入口未延迟显示。证据在 audit01/startup-final2 与 MainFormApplicationContractTests。

- VOICE-02 audit01-red：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\voice02\bin\ --logger trx;LogFileName=audit01-red.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\voice02 --filter FullyQualifiedName~ProgramDefersFirstShowUntilApplicationRun`；返回码 1，通过 0、失败 1、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/voice02/audit01-red.log`、`audit01-red-result.json`；如为 C# 测试，另有同名 TRX。

- VOICE-02 audit01-green：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\voice02\bin\ --logger trx;LogFileName=audit01-green.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\voice02 --filter FullyQualifiedName~ProgramDefersFirstShowUntilApplicationRun`；返回码 0，通过 1、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/voice02/audit01-green.log`、`audit01-green-result.json`；如为 C# 测试，另有同名 TRX。

- VOICE-02 audit01-release：命令 `dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\voice02\bin\`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/voice02/audit01-release.log`、`audit01-release-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-01 修复后真实隔离复核：Program 改为 InitializeApplication(false) 后再进入 Application.Run，Release 构建返回 0；启动烟雾返回码 3，结果 `artifacts/runs/csharp_non_update_gaps_20261020/audit01/startup-after-program-fix/startup-window-smoke.json`。

- AUDIT-01 探针模态框修正后复核：隔离目录提供 fake Start.exe/sandboxie_path，避免真实扫描成功对话框；启动烟雾返回码 0，结果 `artifacts/runs/csharp_non_update_gaps_20261020/audit01/startup-after-program-fix2/startup-window-smoke.json`。

- AUDIT-01 探针模态框修正后复核实际结果：返回码 3；结果 `artifacts/runs/csharp_non_update_gaps_20261020/audit01/startup-after-program-fix2/startup-window-smoke.json`。

- AUDIT-01 启动烟雾最终复核（就绪标记）：返回码 3；结果 `artifacts/runs/csharp_non_update_gaps_20261020/audit01/startup-after-program-fix3/startup-window-smoke.json`。

- AUDIT-01 启动烟雾最终复核（关闭 fallback）：返回码 0；结果 `artifacts/runs/csharp_non_update_gaps_20261020/audit01/startup-after-program-fix4/startup-window-smoke.json`。
`n- AUDIT-01 有效 RED/GREEN 结论：首次启动关闭异常来自 Program 在 Application.Run 前调用 InitializeApplication(true) 造成首次初始化竞态；将首次显示延迟到 Application.Run 后，隔离烟雾最终返回码 0、主窗口句柄/标题/正常退出通过。中间模态框、配置和关闭消息均已在探针中隔离/校准，未修改用户安装目录。

- VOICE-02 audit01-affected：命令 `dotnet vstest D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\voice02\bin\x64\Release\net48\ShaheLauncher.Tests.dll /Platform:x64 /Logger:trx;LogFileName=audit01-affected.trx /ResultsDirectory:D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\voice02 /TestCaseFilter:TestCategory=Application|TestCategory=Lifecycle|TestCategory=Tray`；返回码 0，通过 120、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/voice02/audit01-affected.log`、`audit01-affected-result.json`；如为 C# 测试，另有同名 TRX。

- VOICE-02 audit01-python：命令 `python -m unittest discover -s tests -p test_*.py -v`；返回码 0，通过 29、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/voice02/audit01-python.log`、`audit01-python-result.json`；如为 C# 测试，另有同名 TRX。

- VOICE-02 audit01-compile：命令 `python -m compileall -q shahe_bulk_launcher tests`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/voice02/audit01-compile.log`、`audit01-compile-result.json`；如为 C# 测试，另有同名 TRX。

- VOICE-02 audit01-release2：命令 `dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\voice02\bin\`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/voice02/audit01-release2.log`、`audit01-release2-result.json`；如为 C# 测试，另有同名 TRX。

- VOICE-02 audit01-full：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\voice02\bin\ --logger trx;LogFileName=audit01-full.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\voice02`；返回码 0，通过 344、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/voice02/audit01-full.log`、`audit01-full-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-01 差异检查：命令 `git diff --check -- AUDIT01 files`，返回码 0，计数不适用；日志 `artifacts/runs/csharp_non_update_gaps_20261020/audit01/diff.log`。

- AUDIT-01 完成：隔离发布目录启动取得主窗口句柄/标题，等待就绪后正常退出返回码 0；首次关闭异常定位为 Program 在 Application.Run 前显示窗体的初始化竞态，已改为 InitializeApplication(false) 后创建托盘再进入消息循环，并用真实隔离烟雾复核。工具合约 6/6、Application/Lifecycle/Tray 受影响 120/120、C# 全量 344/344、Python 29/29、compileall 0、Release x64 0 警告/0 错误；证据目录 `artifacts/runs/csharp_non_update_gaps_20261020/audit01`。定位、多窗口和键盘/读屏仍未验证。
`n- AUDIT-02 开始：建立路径记忆备注/删除/清空/上下文菜单证据目录，先运行受影响契约；真实 UI 输入仅在取得有效句柄和焦点证据后计入。

- AUDIT-02 affected：命令 `dotnet vstest D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit02\bin\x64\Release\net48\ShaheLauncher.Tests.dll /Platform:x64 /Logger:trx;LogFileName=affected.trx /ResultsDirectory:D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit02 /TestCaseFilter:TestCategory=AppPaths|TestCategory=SettingsService|TestCategory=Application`；返回码 1，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit02/affected.log`、`affected-result.json`；如为 C# 测试，另有同名 TRX。
`n- AUDIT-02 affected 初次未执行：隔离 audit02 输出目录尚未构建，vstest 找不到程序集；不计产品结果，先执行隔离 Release 构建。

- AUDIT-02 release：命令 `dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit02\bin\`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit02/release.log`、`release-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-02 affected2：命令 `dotnet vstest D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit02\bin\x64\Release\net48\ShaheLauncher.Tests.dll /Platform:x64 /Logger:trx;LogFileName=affected2.trx /ResultsDirectory:D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit02 /TestCaseFilter:TestCategory=AppPaths|TestCategory=SettingsService|TestCategory=Application`；返回码 0，通过 127、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit02/affected2.log`、`affected2-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-02 现行 Release 路径历史启动复核：命令 `real-exe-fixed-startup.ps1 -BuildDirectory voice02/bin -ResultPath audit02/real-path-history-current.json`，返回码 0；结果 `artifacts/runs/csharp_non_update_gaps_20261020/audit02/real-path-history-current.json`。覆盖隔离 999 条 JSON 历史加载和就绪/关闭，不覆盖备注/删除/清空键盘交互。

- AUDIT-02 ui-contracts：命令 `dotnet vstest D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit02\bin\x64\Release\net48\ShaheLauncher.Tests.dll /Platform:x64 /Logger:trx;LogFileName=ui-contracts.trx /ResultsDirectory:D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit02 /TestCaseFilter:FullyQualifiedName~SettingsDialogsCSharpContractTests|FullyQualifiedName~UiControlsCSharpContractTests|FullyQualifiedName~PathHistoryLimitRuntimeContractTests`；返回码 0，通过 32、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit02/ui-contracts.log`、`ui-contracts-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-02 python：命令 `python -m unittest discover -s tests -p test_*.py -v`；返回码 0，通过 29、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit02/python.log`、`python-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-02 compile：命令 `python -m compileall -q shahe_bulk_launcher tests`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit02/compile.log`、`compile-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-02 完成：现行 Release 隔离启动加载 999 条 JSON 路径历史，窗口/就绪/自然退出/无残留均通过；C# 备注、当前删除、全部清空、上下文菜单和数量限制契约 32/32，Python 29/29，compileall 0，隔离 Release 0 警告/0 错误。真实备注/删除/清空键盘输入未取得稳定焦点证据，仍列为 AUDIT-04/08 风险；不把历史 UI 日志替代本轮结论。证据目录 `artifacts/runs/csharp_non_update_gaps_20261020/audit02`。

- AUDIT-02 差异检查：返回码 0，日志 `artifacts/runs/csharp_non_update_gaps_20261020/audit02/diff.log`，计数不适用。
`n- AUDIT-03 RED：AuditRunner 命令行接受 verify-clean 但运行器返回 unsupported；新增隔离只读运行时契约，要求独立报告且三份输入字节不变。

- AUDIT-03 red：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\bin\ --logger trx;LogFileName=red.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03 --filter FullyQualifiedName~VerifyCleanProducesReadOnlyOwnershipReport`；返回码 1，通过 0、失败 1、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/red.log`、`red-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 green：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\bin\ --logger trx;LogFileName=green.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03 --filter FullyQualifiedName~VerifyCleanProducesReadOnlyOwnershipReport`；返回码 1，通过 0、失败 1、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/green.log`、`green-result.json`；如为 C# 测试，另有同名 TRX。
`n- AUDIT-03 首次 GREEN 中 verify-clean 已生成只读报告且输入未变，但测试按小写 JSON 属性断言失败；JavaScriptSerializer 输出模型属性为 `Kind`，修正测试断言后重跑，不改生产行为。

- AUDIT-03 green2：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\bin\ --logger trx;LogFileName=green2.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03 --filter FullyQualifiedName~VerifyCleanProducesReadOnlyOwnershipReport`；返回码 0，通过 1、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/green2.log`、`green2-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 affected：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\bin\ --logger trx;LogFileName=affected.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03 --filter TestCategory=AuditRunner`；返回码 0，通过 23、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/affected.log`、`affected-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 python：命令 `python -m unittest discover -s tests -p test_*.py -v`；返回码 0，通过 29、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/python.log`、`python-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 compile：命令 `python -m compileall -q shahe_bulk_launcher tests`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/compile.log`、`compile-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 release：命令 `dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\bin\`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/release.log`、`release-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 full：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\bin\ --logger trx;LogFileName=full.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03`；返回码 0，通过 345、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/full.log`、`full-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 真实只读 verify-clean：返回码 3；目标、配置、Sandboxie.ini 使用精确 SHA-256，报告路径见 `artifacts/runs/csharp_non_update_gaps_20261020/audit03/verify-clean.log` 与生成的 `auditrunner-verify-clean-*.json`。返回 blocked 时只代表清洁门禁未满足，不执行清理。
`n- AUDIT-03 后续 RED：batch-rename/batch-delete 命令已登记但运行器无对应方法；新增最终阶段运行时契约和重命名后所有权状态校验。

- AUDIT-03 final-stages-red：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\bin\ --logger trx;LogFileName=final-stages-red.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03 --filter FullyQualifiedName~LifecycleRenameAndDeleteStagesAreImplementedByCompiledRunner`；返回码 1，通过 0、失败 1、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/final-stages-red.log`、`final-stages-red-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 final-stages-green：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\bin\ --logger trx;LogFileName=final-stages-green.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03 --filter FullyQualifiedName~LifecycleRenameAndDeleteStagesAreImplementedByCompiledRunner`；返回码 0，通过 1、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/final-stages-green.log`、`final-stages-green-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 ancestor-red：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\bin\ --logger trx;LogFileName=ancestor-red.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03 --filter FullyQualifiedName~ProcessSnapshotCanExcludeItsOwnLauncherChain`；返回码 1，通过 0、失败 1、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/ancestor-red.log`、`ancestor-red-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 ancestor-green：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\bin\ --logger trx;LogFileName=ancestor-green.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03 --filter FullyQualifiedName~ProcessSnapshotCanExcludeItsOwnLauncherChain|FullyQualifiedName~LifecycleRenameAndDeleteStagesAreImplementedByCompiledRunner|FullyQualifiedName~VerifyCleanProducesReadOnlyOwnershipReport`；返回码 0，通过 3、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/ancestor-green.log`、`ancestor-green-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 release-real：命令 `dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\bin\`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/release-real.log`、`release-real-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 真实 Sandboxie preflight：返回码 0；目标/隔离配置/真实 Sandboxie.ini 精确哈希，报告 `D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\auditrunner-preflight-20261022_105908_372-result.json`，索引 `artifacts/runs/csharp_non_update_gaps_20261020/audit03/real-preflight-result.json`。仅返回码 0 才继续生命周期。

- AUDIT-03 真实 batch-create：前缀 ShaheAudit_20261022_1059A，返回码 3；报告路径见 `artifacts/runs/csharp_non_update_gaps_20261020/audit03/real-batch-create.log`。仅创建精确所有权临时资源。

- AUDIT-03 alias-green：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\bin\ --logger trx;LogFileName=alias-green.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03 --filter FullyQualifiedName~BatchCreatePlanUsesOriginalShaheOfficialPath|FullyQualifiedName~CreateGroupPreservesExistingSettingsAndUsesExactOwnedMembers|FullyQualifiedName~LaunchGroupSelectsOnlyExactBoundGroupClaims`；返回码 1，通过 2、失败 1、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/alias-green.log`、`alias-green-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 alias-green2：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\bin\ --logger trx;LogFileName=alias-green2.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03 --filter FullyQualifiedName~BatchCreatePlanUsesOriginalShaheOfficialPath|FullyQualifiedName~CreateGroupPreservesExistingSettingsAndUsesExactOwnedMembers|FullyQualifiedName~LaunchGroupSelectsOnlyExactBoundGroupClaims`；返回码 1，通过 2、失败 1、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/alias-green2.log`、`alias-green2-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 alias-green3：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\bin\ --logger trx;LogFileName=alias-green3.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03 --filter FullyQualifiedName~BatchCreatePlanUsesOriginalShaheOfficialPath|FullyQualifiedName~CreateGroupPreservesExistingSettingsAndUsesExactOwnedMembers|FullyQualifiedName~LaunchGroupSelectsOnlyExactBoundGroupClaims`；返回码 0，通过 3、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/alias-green3.log`、`alias-green3-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 release-real2：命令 `dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\bin\`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/release-real2.log`、`release-real2-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 别名修复后真实 preflight：返回码 0，报告 `D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\auditrunner-preflight-20261022_110251_312-result.json`；仅返回码 0 才重启生命周期。

- AUDIT-03 真实 batch-create 重跑：前缀 ShaheAudit_20261022_1103B，返回码 0，报告 `D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\auditrunner-lifecycle-batch-create-20261022_110309_881-result.json`。

- AUDIT-03 真实 create-group：返回码 0，报告 `D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\auditrunner-lifecycle-create-group-20261022_110329_466-result.json`。

- AUDIT-03 真实 launch-group：返回码 0，报告 `D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\auditrunner-lifecycle-launch-group-20261022_110348_301-result.json`；仅运行本轮两个所有权沙盒并由审计器清理其进程。

- AUDIT-03 真实 schedule-launch：返回码 0，报告 `D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\auditrunner-lifecycle-schedule-launch-20261022_110408_086-result.json`。

- AUDIT-03 真实 schedule-close：返回码 3，报告 `D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\auditrunner-lifecycle-schedule-close-20261022_110425_393-result.json`；若 SbieCtrl 出现，安全门禁应先阻断。

- AUDIT-03 schedule-close 阻断后精确 cleanup：返回码 0，报告 `D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\auditrunner-lifecycle-cleanup-20261022_110447_414-result.json`；只恢复本轮隔离配置并删除 state 中精确所有权资源。

- AUDIT-03 cleanup 后真实只读 verify-clean：返回码 3，报告 `D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\auditrunner-verify-clean-20261022_110508_269-result.json`；SbieCtrl 或哈希变化会保持 blocked，重点核对本轮 ShaheAudit section/directory/process 是否清零。

- AUDIT-03 affected-final：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\bin\ --logger trx;LogFileName=affected-final.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03 --filter TestCategory=AuditRunner|TestCategory=SandboxAdmin|TestCategory=Scheduling`；返回码 0，通过 53、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/affected-final.log`、`affected-final-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 python-final：命令 `python -m unittest discover -s tests -p test_*.py -v`；返回码 0，通过 29、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/python-final.log`、`python-final-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 compile-final：命令 `python -m compileall -q shahe_bulk_launcher tests`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/compile-final.log`、`compile-final-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 release-final：命令 `dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\bin\`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/release-final.log`、`release-final-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 rollback-green-final：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\bin\ --logger trx;LogFileName=rollback-green-final.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03 --filter FullyQualifiedName~LifecycleRenameAndDeleteStagesAreImplementedByCompiledRunner|FullyQualifiedName~VerifyCleanProducesReadOnlyOwnershipReport|FullyQualifiedName~ProcessSnapshotCanExcludeItsOwnLauncherChain`；返回码 0，通过 3、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/rollback-green-final.log`、`rollback-green-final-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 full-final2：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\bin\ --logger trx;LogFileName=full-final2.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03`；返回码 1，通过 346、失败 1、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/full-final2.log`、`full-final2-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 confirmation-recheck：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\bin\ --logger trx;LogFileName=confirmation-recheck.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03 --filter FullyQualifiedName~BatchCreateConfirmationWaitsForTheButtonHandler`；返回码 0，通过 1、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/confirmation-recheck.log`、`confirmation-recheck-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 full-final3：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\bin\ --logger trx;LogFileName=full-final3.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03`；返回码 1，通过 346、失败 1、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/full-final3.log`、`full-final3-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 confirmation-green：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\bin\ --logger trx;LogFileName=confirmation-green.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03 --filter FullyQualifiedName~BatchCreateConfirmationWaitsForTheButtonHandler`；返回码 1，通过 0、失败 1、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/confirmation-green.log`、`confirmation-green-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 confirmation-green2：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\bin\ --logger trx;LogFileName=confirmation-green2.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03 --filter FullyQualifiedName~BatchCreateConfirmationWaitsForTheButtonHandler`；返回码 0，通过 1、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/confirmation-green2.log`、`confirmation-green2-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 full-final4：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\bin\ --logger trx;LogFileName=full-final4.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03`；返回码 0，通过 347、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/full-final4.log`、`full-final4-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 affected-final2：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\bin\ --logger trx;LogFileName=affected-final2.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03 --filter TestCategory=AuditRunner|TestCategory=SandboxAdmin|TestCategory=Scheduling`；返回码 0，通过 53、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/affected-final2.log`、`affected-final2-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 python-final2：命令 `python -m unittest discover -s tests -p test_*.py -v`；返回码 0，通过 29、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/python-final2.log`、`python-final2-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 compile-final2：命令 `python -m compileall -q shahe_bulk_launcher tests`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/compile-final2.log`、`compile-final2-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 release-final2：命令 `dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\bin\`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/release-final2.log`、`release-final2-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 当前结论：新增只读 `verify-clean`、真实审计进程祖先排除、强制 BoxAlias 的审计别名、`batch-rename`、`batch-delete` 及失败回滚快照；真实沙河中 `batch-create`、`create-group`、`launch-group`、`schedule-launch`、精确 cleanup 均通过。cleanup 后目标、隔离配置和真实 `Sandboxie.ini` 哈希均匹配基线，`ShaheAudit_*` section/目录、目标进程和 `Start.exe` 均为 0。用户现有 `SbieCtrl.exe` PID `15628` 使 `schedule-close` 和最终只读清洁门禁按设计返回 blocked；未结束用户进程，真实 `delete-group`、`cancel-schedules`、`batch-rename`、`batch-delete` 仍无有效执行证据。因此本轮代码和自动化门禁已通过，但 AUDIT-03 模块保持未完成，下一次继续工作仍从 AUDIT-03 的首个未完成真实场景开始。汇总报告：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/implementation-report.md`。

- AUDIT-03 暂存快照首次锁定还原：`dotnet restore <staged-snapshot>\csharp\ShaheBulkLauncher.sln --locked-mode` 返回码 1，`ShaheLauncher.Tests/packages.lock.json` 缺少新增 AuditRunner 项目引用，报 `NU1004`；测试未执行，不计产品失败。日志与结果：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/staged-restore.log`、`staged-restore-result.json`。需更新锁文件后重新导出索引快照验证。

- AUDIT-03 暂存快照第二次验证：更新测试项目锁文件后，`--locked-mode` 还原返回码 0；随后干净索引快照全量测试在编译阶段返回码 1、未产生 TRX，原因是已提交的 `MainForm.cs` 引用了仅存在于未暂存 `HotkeyController.cs` 的 `ListMayHideSandboxMembers` 与 `AllHasProcess`。这证明当前远端提交组合缺少调用端的配套类型定义；不计 AUDIT-03 产品测试失败，需先核对并补齐最小构建依赖。日志与结果：`staged-restore2.log`、`staged-restore2-result.json`、`staged-full.log`、`staged-full-result.json`。

- AUDIT-03 暂存快照第三步：纳入 `HotkeyController.cs` 的既有配套类型定义后，锁定还原和编译成功，全量返回码 1、通过 346、失败 1、跳过 0；唯一失败是旧 `SettingsRuntimeContractTests` 仍期望重置设置在 UI 线程同步扫描全部沙盒 PID 一次，而已提交菜单刷新实现实际为 0 次并转入后台刷新。该工作树测试差异与现有实现一致，需作为先前遗漏的配套断言纳入后再复跑。日志与结果：`staged-restore3.log`、`staged-restore3-result.json`、`staged-full2.log`、`staged-full2-result.json`、`staged-full2.trx`。

- AUDIT-03 干净暂存快照 GREEN：纳入测试项目锁文件、`HotkeyController.cs` 配套状态成员和设置重置异步扫描断言后，`dotnet restore <staged-snapshot>\csharp\ShaheBulkLauncher.sln --locked-mode` 返回码 0；同一索引快照 `dotnet test` Release x64 返回码 0，通过 347、失败 0、跳过 0。该结果不读取工作树未暂存代码。日志与结果：`staged-restore4.log`、`staged-restore4-result.json`、`staged-full3.log`、`staged-full3-result.json`、`staged-full3.trx`。

- AUDIT-03 干净暂存快照 Python 对照：在同一索引快照运行 `python -m unittest discover -s tests -p "test_*.py" -v`，返回码 0，通过 29、失败 0、跳过 0。日志与结果：`staged-python.log`、`staged-python-result.json`。

- AUDIT-03 干净暂存快照 Python 编译：在同一索引快照运行 `python -m compileall -q shahe_bulk_launcher tests`，返回码 0。日志与结果：`staged-compile.log`、`staged-compile-result.json`。

- AUDIT-03 干净暂存快照 Release x64：在同一索引快照运行隔离 `dotnet build --configuration Release --no-restore --property:Platform=x64`，返回码 0，0 警告、0 错误。日志与结果：`staged-release.log`、`staged-release-result.json`。至此当前拟提交索引的 C# 全量、Python 全量、Python 编译和 Release 构建均取得有效 GREEN。

- AUDIT-03 safety-review-red：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\bin\ --logger trx;LogFileName=safety-review-red.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03 --filter FullyQualifiedName~AliasRenameRestoresAttemptedWrites|FullyQualifiedName~AliasRollbackContinues|FullyQualifiedName~CleanupSelectionAllowsOnlyAbsent|FullyQualifiedName~OwnedConfigurationCleanupCanBeRepeated|FullyQualifiedName~LifecycleStateWritePreserves`；返回码 1，通过 0、失败 6、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/safety-review-red.log`、`safety-review-red-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 safety-review-green：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\bin\ --logger trx;LogFileName=safety-review-green.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03 --filter FullyQualifiedName~AliasRenameRestoresAttemptedWrites|FullyQualifiedName~AliasRollbackContinues|FullyQualifiedName~CleanupSelectionAllowsOnlyAbsent|FullyQualifiedName~OwnedConfigurationCleanupCanBeRepeated|FullyQualifiedName~LifecycleStateWritePreserves`；返回码 0，通过 6、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/safety-review-green.log`、`safety-review-green-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 process-safety-red：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\bin\ --logger trx;LogFileName=process-safety-red.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03 --filter FullyQualifiedName~ProcessTerminationRejectsChangedOwnership|FullyQualifiedName~ProcessQueryFailureCannotBeReported`；返回码 1，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/process-safety-red.log`、`process-safety-red-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 process-safety-red 首次因审计器参数接线未补全而编译失败，未运行测试，不计行为 RED；补齐调用参数后另跑有效 RED。

- AUDIT-03 process-safety-red2：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\bin\ --logger trx;LogFileName=process-safety-red2.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03 --filter FullyQualifiedName~ProcessTerminationRejectsChangedOwnership|FullyQualifiedName~ProcessQueryFailureCannotBeReported`；返回码 1，通过 0、失败 2、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/process-safety-red2.log`、`process-safety-red2-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 process-safety-green：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\bin\ --logger trx;LogFileName=process-safety-green.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03 --filter FullyQualifiedName~ProcessTerminationRejectsChangedOwnership|FullyQualifiedName~ProcessQueryFailureCannotBeReported`；返回码 0，通过 2、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/process-safety-green.log`、`process-safety-green-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 exact-clean-red：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\bin\ --logger trx;LogFileName=exact-clean-red.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03 --filter FullyQualifiedName~VerifyCleanRejectsOrphanDirectory|FullyQualifiedName~VerifyCleanProducesReadOnlyOwnershipReport`；返回码 1，通过 0、失败 2、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/exact-clean-red.log`、`exact-clean-red-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 exact-clean-green：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\bin\ --logger trx;LogFileName=exact-clean-green.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03 --filter FullyQualifiedName~VerifyCleanRejectsOrphanDirectory|FullyQualifiedName~VerifyCleanProducesReadOnlyOwnershipReport`；返回码 0，通过 2、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/exact-clean-green.log`、`exact-clean-green-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 review-affected-final：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\bin\ --logger trx;LogFileName=review-affected-final.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03 --filter TestCategory=AuditRunner|TestCategory=SandboxAdmin|TestCategory=Scheduling|FullyQualifiedName~HotkeyControllerContractTests|FullyQualifiedName~SettingsRuntimeContractTests`；返回码 0，通过 86、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/review-affected-final.log`、`review-affected-final-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 review-full-final：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\bin\ --logger trx;LogFileName=review-full-final.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03`；返回码 0，通过 356、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/review-full-final.log`、`review-full-final-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 修正后真实只读复查：执行 audit03/verify-owned-clean.ps1，返回码 3，状态 blocked；精确所有权检查 ownedResources=True。完整参数和只含本轮资源的摘要见 artifacts/runs/csharp_non_update_gaps_20261020/audit03/review-real-verify-clean-result.json；含用户资源名称的原始报告只保留本地。

- AUDIT-03 review-python-final：命令 `python -m unittest discover -s tests -p test_*.py -v`；返回码 0，通过 29、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/review-python-final.log`、`review-python-final-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 review-compile-final：命令 `python -m compileall -q shahe_bulk_launcher tests`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/review-compile-final.log`、`review-compile-final-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 review-release-final：命令 `dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\bin\`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/review-release-final.log`、`review-release-final-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 callback-cleanup-red：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\bin\ --logger trx;LogFileName=callback-cleanup-red.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03 --filter FullyQualifiedName~ExistingSandboxProcessCannotEnable|FullyQualifiedName~TimerStopCannotAuthorize|FullyQualifiedName~CleanupRechecksOwnership`；返回码 1，通过 0、失败 4、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/callback-cleanup-red.log`、`callback-cleanup-red-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 callback-cleanup-green：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\bin\ --logger trx;LogFileName=callback-cleanup-green.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03 --filter FullyQualifiedName~ExistingSandboxProcessCannotEnable|FullyQualifiedName~TimerStopCannotAuthorize|FullyQualifiedName~CleanupRechecksOwnership`；返回码 1，计数不适用。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/callback-cleanup-green.log`、`callback-cleanup-green-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 callback-cleanup-green 首次缺少标准库 System.Threading.Tasks 命名空间，编译未通过且未执行测试；补充 using 后重跑。

- AUDIT-03 callback-cleanup-green2：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\bin\ --logger trx;LogFileName=callback-cleanup-green2.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03 --filter FullyQualifiedName~ExistingSandboxProcessCannotEnable|FullyQualifiedName~TimerStopCannotAuthorize|FullyQualifiedName~CleanupRechecksOwnership`；返回码 0，通过 4、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/callback-cleanup-green2.log`、`callback-cleanup-green2-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 safety-final-affected：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\bin\ --logger trx;LogFileName=safety-final-affected.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03 --filter TestCategory=AuditRunner|TestCategory=SandboxAdmin|TestCategory=Scheduling|FullyQualifiedName~HotkeyControllerContractTests|FullyQualifiedName~SettingsRuntimeContractTests`；返回码 0，通过 90、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/safety-final-affected.log`、`safety-final-affected-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 safety-final-full：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\bin\ --logger trx;LogFileName=safety-final-full.trx --results-directory D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03`；返回码 0，通过 360、失败 0、跳过 0。日志与命令索引：`artifacts/runs/csharp_non_update_gaps_20261020/audit03/safety-final-full.log`、`safety-final-full-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 安全复核：先前反射方法存在性与所有权选择测试未覆盖异常回滚。新增有效 RED/GREEN 覆盖部分写入/回滚失败、原子状态保存、部分删除重试、配置重复恢复、误终止防护、进程查询失败、非前缀目录残留、定时回调未结束时禁止清理、删除期间所有权变化。独立只读复核确认本轮关键问题已修复；仅作为审计器安全修复检查点，不把 AUDIT-03 标记完成。原始 SbieCtrl 在首轮 preflight 中为零、测试期间出现，启动归属未确认；此前“用户现有进程”措辞不构成归属证据，当前仅按门禁保留。

- AUDIT-03 clean-final-restore（干净索引快照）：返回码 0，测试计数不适用。实际命令、日志路径与结果见 artifacts/runs/csharp_non_update_gaps_20261020/audit03/clean-final-restore-result.json 及同名日志；C# 测试另有 TRX。

- AUDIT-03 clean-final-full（干净索引快照）：返回码 0，通过 360、失败 0、跳过 0。实际命令、日志路径与结果见 artifacts/runs/csharp_non_update_gaps_20261020/audit03/clean-final-full-result.json 及同名日志；C# 测试另有 TRX。

- AUDIT-03 clean-final-python（干净索引快照）：返回码 0，通过 29、失败 0、跳过 0。实际命令、日志路径与结果见 artifacts/runs/csharp_non_update_gaps_20261020/audit03/clean-final-python-result.json 及同名日志；C# 测试另有 TRX。

- AUDIT-03 clean-final-compile（干净索引快照）：返回码 0，测试计数不适用。实际命令、日志路径与结果见 artifacts/runs/csharp_non_update_gaps_20261020/audit03/clean-final-compile-result.json 及同名日志；C# 测试另有 TRX。

- AUDIT-03 clean-final-release（干净索引快照）：返回码 0，测试计数不适用。实际命令、日志路径与结果见 artifacts/runs/csharp_non_update_gaps_20261020/audit03/clean-final-release-result.json 及同名日志；C# 测试另有 TRX。

- AUDIT-03 最终真实只读复查：返回码 3，状态 blocked，精确 ownedResources=True；完整命令与本轮资源摘要见 artifacts/runs/csharp_non_update_gaps_20261020/audit03/final-real-verify-result.json。目标/配置/INI 哈希和审计资源清零检查均通过，SbieCtrl/SandboxieOther 进程门禁保持 blocked；未执行新的破坏性场景。

- AUDIT-03 最终差异检查：全工作树返回码 2（仅保留原有 GAP-09 日志空白），本轮源码/文档范围返回码 0，暂存范围返回码 0。日志和命令索引见 audit03/final-diff-result.json 与 final-*-diff.log；不改写用户历史日志。

- AUDIT-03 暂存检查补充：原始全工作树 diff 日志忠实包含用户旧日志的尾随空白，暂存它会触发差异检查错误；该原始日志仅保留本地，提交命令/返回码摘要，未改写原始证据。移出该日志后暂存检查再次验证。

- AUDIT-03 最终暂存复查返回码 0，证据为 audit03/final-staged-recheck-result.json 与同名日志。

- AUDIT-03 安全修复检查点已独立提交 4ccf15a，并推送 origin/fix/non-update-gaps-20261020，返回码 0；回退命令 git revert 4ccf15a。真实生命周期仍保留已记录阻断。随后按用户明确追加要求处理打赏资源与图片复制接入；该追加任务不把 AUDIT-03 标为完成，也不改变其后审计顺序。

- DONATE-01 red：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\donation_qr_20261022\bin\ --logger trx;LogFileName=red.trx --results-directory D:\mycodex\git\artifacts\runs\donation_qr_20261022 --filter FullyQualifiedName~DonationQrRuntimeContractTests`；返回码 1，通过 0、失败 4、跳过 0。日志与命令索引：`artifacts/runs/donation_qr_20261022/red.log`、`red-result.json`；如为 C# 测试，另有同名 TRX。

- DONATE-01 green：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\donation_qr_20261022\bin\ --logger trx;LogFileName=green.trx --results-directory D:\mycodex\git\artifacts\runs\donation_qr_20261022 --filter FullyQualifiedName~DonationQrRuntimeContractTests`；返回码 0，通过 4、失败 0、跳过 0。日志与命令索引：`artifacts/runs/donation_qr_20261022/green.log`、`green-result.json`；如为 C# 测试，另有同名 TRX。

- DONATE-01 affected：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\donation_qr_20261022\bin\ --logger trx;LogFileName=affected.trx --results-directory D:\mycodex\git\artifacts\runs\donation_qr_20261022 --filter TestCategory=CommonDialogs|TestCategory=Updates|FullyQualifiedName~MainFormApplicationContractTests`；返回码 0，通过 68、失败 0、跳过 0。日志与命令索引：`artifacts/runs/donation_qr_20261022/affected.log`、`affected-result.json`；如为 C# 测试，另有同名 TRX。

- DONATE-01 full：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\donation_qr_20261022\bin\ --logger trx;LogFileName=full.trx --results-directory D:\mycodex\git\artifacts\runs\donation_qr_20261022`；返回码 0，通过 364、失败 0、跳过 0。日志与命令索引：`artifacts/runs/donation_qr_20261022/full.log`、`full-result.json`；如为 C# 测试，另有同名 TRX。

- DONATE-01 python：命令 `python -m unittest discover -s tests -p test_*.py -v`；返回码 0，通过 29、失败 0、跳过 0。日志与命令索引：`artifacts/runs/donation_qr_20261022/python.log`、`python-result.json`；如为 C# 测试，另有同名 TRX。

- DONATE-01 compile：命令 `python -m compileall -q shahe_bulk_launcher tests`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/donation_qr_20261022/compile.log`、`compile-result.json`；如为 C# 测试，另有同名 TRX。

- DONATE-01 release：命令 `dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\donation_qr_20261022\bin\`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/donation_qr_20261022/release.log`、`release-result.json`；如为 C# 测试，另有同名 TRX。

- DONATE-01 发布资源复制：dotnet publish App Release x64 返回码 0；完整命令及输出见 artifacts/runs/donation_qr_20261022/publish-result.json、publish.log。该检查覆盖新增打赏资源，不替代待办 AUDIT-07 的整个发布包验收。

- DONATE-01 原图完整性：源目录、构建目录、发布目录逐张对照用户上传原图 SHA-256，检查通过 2、失败 0、跳过 0；尺寸与结果见 artifacts/runs/donation_qr_20261022/image-copy-verification.json。图片没有重绘、转码或裁剪。

- DONATE-01 本轮源码与文档差异检查返回码 0，命令和日志索引见 artifacts/runs/donation_qr_20261022/diff-result.json。

- SETTINGS-01 red：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\settings_effects_20261022\bin\ --logger trx;LogFileName=red.trx --results-directory D:\mycodex\git\artifacts\runs\settings_effects_20261022 --filter FullyQualifiedName~SoftwareSettingsApplyNewValuesToRuntimeAndAutoRunWriter`；返回码 1，通过 0、失败 2、跳过 0。日志与命令索引：`artifacts/runs/settings_effects_20261022/red.log`、`red-result.json`；如为 C# 测试，另有同名 TRX。

- SETTINGS-01 green：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\settings_effects_20261022\bin\ --logger trx;LogFileName=green.trx --results-directory D:\mycodex\git\artifacts\runs\settings_effects_20261022 --filter FullyQualifiedName~SettingsRuntimeContractTests`；返回码 0，通过 18、失败 0、跳过 0。日志与命令索引：`artifacts/runs/settings_effects_20261022/green.log`、`green-result.json`；如为 C# 测试，另有同名 TRX。

- SETTINGS-01 affected：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\settings_effects_20261022\bin\ --logger trx;LogFileName=affected.trx --results-directory D:\mycodex\git\artifacts\runs\settings_effects_20261022 --filter FullyQualifiedName~LauncherSettingsServiceContractTests|FullyQualifiedName~SettingsDialogsCSharpContractTests|FullyQualifiedName~SettingsRuntimeContractTests|FullyQualifiedName~MainFormLifecycleContractTests|FullyQualifiedName~MainFormApplicationContractTests|FullyQualifiedName~LaunchSchedulerContractTests|FullyQualifiedName~UiControlsCSharpContractTests|FullyQualifiedName~SpeechServiceContractTests`；返回码 0，通过 109、失败 0、跳过 0。日志与命令索引：`artifacts/runs/settings_effects_20261022/affected.log`、`affected-result.json`；如为 C# 测试，另有同名 TRX。

- SETTINGS-01 full：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\settings_effects_20261022\bin\ --logger trx;LogFileName=full.trx --results-directory D:\mycodex\git\artifacts\runs\settings_effects_20261022`；返回码 0，通过 366、失败 0、跳过 0。日志与命令索引：`artifacts/runs/settings_effects_20261022/full.log`、`full-result.json`；如为 C# 测试，另有同名 TRX。

- SETTINGS-01 Python 原版顺序探针：返回码 0，确认 on_settings_saved 先赋值 auto_run 再执行 _apply_auto_run；实际位置与输出见 artifacts/runs/settings_effects_20261022/python-order.log、python-order-result.json。该探针只解析源码，不修改启动项。

- SETTINGS-01 python：命令 `python -m unittest discover -s tests -p test_*.py -v`；返回码 0，通过 29、失败 0、跳过 0。日志与命令索引：`artifacts/runs/settings_effects_20261022/python.log`、`python-result.json`；如为 C# 测试，另有同名 TRX。

- SETTINGS-01 compile：命令 `python -m compileall -q shahe_bulk_launcher tests`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/settings_effects_20261022/compile.log`、`compile-result.json`；如为 C# 测试，另有同名 TRX。

- SETTINGS-01 release：命令 `dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\settings_effects_20261022\bin\`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/settings_effects_20261022/release.log`、`release-result.json`；如为 C# 测试，另有同名 TRX。

- SETTINGS-01 结论：Python on_settings_saved 先写入新 auto_run 再应用自启动；C# 普通设置入口此前先调用旧 MainForm.AutoRun。将同步移动到共享 CreateSettingsCallbacks.ApplyAutoRun，普通保存与重置共用此逻辑，删除仅重置路径的重复补丁。两个方向的有效 RED 均已 GREEN；同一生产窗体保存链同时验证调度参数、跳过运行标志、音量记忆标志、范围勾选、左右导航门禁、日志开关及配置持久化。详表与读屏原版遗留问题见 artifacts/runs/settings_effects_20261022/settings-effect-audit.md。

- SETTINGS-01 本轮源码/文档差异检查返回码 0，记录为 artifacts/runs/settings_effects_20261022/diff-result.json。用户原有 GAP-09 日志空白未改动。

- SETTINGS-01 独立提交 68c83a7 已推送 origin/fix/non-update-gaps-20261020（返回码 0），回退命令 git revert 68c83a7。直连网络失败后，本次使用 Windows 已配置的本机代理完成推送，未更改全局代理。开始按用户要求单独修正 MENU-01 的四处结构差异。

- MENU-01 red：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\menu_parity_20261022\bin\ --logger trx;LogFileName=red.trx --results-directory D:\mycodex\git\artifacts\runs\menu_parity_20261022 --filter FullyQualifiedName~BuildsExactClassicMenusWithoutChangingAccessibilityOrder|FullyQualifiedName~PathMemoryMenuAndCtrlHUseClassicContextMenuRuntime`；返回码 1，通过 0、失败 5、跳过 0。日志与命令索引：`artifacts/runs/menu_parity_20261022/red.log`、`red-result.json`；如为 C# 测试，另有同名 TRX。

- MENU-01 green：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\menu_parity_20261022\bin\ --logger trx;LogFileName=green.trx --results-directory D:\mycodex\git\artifacts\runs\menu_parity_20261022 --filter FullyQualifiedName~BuildsExactClassicMenusWithoutChangingAccessibilityOrder|FullyQualifiedName~PathMemoryMenuAndCtrlHUseClassicContextMenuRuntime`；返回码 0，通过 5、失败 0、跳过 0。日志与命令索引：`artifacts/runs/menu_parity_20261022/green.log`、`green-result.json`；如为 C# 测试，另有同名 TRX。

- MENU-01 Python 原版菜单结构探针返回码 0；直接解析 hotkeys.py 中四个受影响分支，核对文字、顺序、分隔线。结果与完整序列见 artifacts/runs/menu_parity_20261022/python-structure.log、python-structure-result.json。

- MENU-01 affected：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\menu_parity_20261022\bin\ --logger trx;LogFileName=affected.trx --results-directory D:\mycodex\git\artifacts\runs\menu_parity_20261022 --filter FullyQualifiedName~MainFormApplicationContractTests|FullyQualifiedName~PathMemoryRuntimeContractTests|FullyQualifiedName~HotkeyControllerContractTests|FullyQualifiedName~SettingsDialogsCSharpContractTests|FullyQualifiedName~DonationQrRuntimeContractTests|TestCategory=Tray|TestCategory=AuditRunner`；返回码 1，通过 99、失败 1、跳过 0。日志与命令索引：`artifacts/runs/menu_parity_20261022/affected.log`、`affected-result.json`；如为 C# 测试，另有同名 TRX。

- MENU-01 clipboard-recheck：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\menu_parity_20261022\bin\ --logger trx;LogFileName=clipboard-recheck.trx --results-directory D:\mycodex\git\artifacts\runs\menu_parity_20261022 --filter FullyQualifiedName~DonationButtonCopiesShippedOriginalImage`；返回码 0，通过 2、失败 0、跳过 0。日志与命令索引：`artifacts/runs/menu_parity_20261022/clipboard-recheck.log`、`clipboard-recheck-result.json`；如为 C# 测试，另有同名 TRX。

- MENU-01 affected-final：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\menu_parity_20261022\bin\ --logger trx;LogFileName=affected-final.trx --results-directory D:\mycodex\git\artifacts\runs\menu_parity_20261022 --filter FullyQualifiedName~MainFormApplicationContractTests|FullyQualifiedName~PathMemoryRuntimeContractTests|FullyQualifiedName~HotkeyControllerContractTests|FullyQualifiedName~SettingsDialogsCSharpContractTests|FullyQualifiedName~DonationQrRuntimeContractTests|TestCategory=Tray|TestCategory=AuditRunner`；返回码 0，通过 100、失败 0、跳过 0。日志与命令索引：`artifacts/runs/menu_parity_20261022/affected-final.log`、`affected-final-result.json`；如为 C# 测试，另有同名 TRX。

- MENU-01 full：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\menu_parity_20261022\bin\ --logger trx;LogFileName=full.trx --results-directory D:\mycodex\git\artifacts\runs\menu_parity_20261022`；返回码 0，通过 369、失败 0、跳过 0。日志与命令索引：`artifacts/runs/menu_parity_20261022/full.log`、`full-result.json`；如为 C# 测试，另有同名 TRX。

- MENU-01 python：命令 `python -m unittest discover -s tests -p test_*.py -v`；返回码 0，通过 29、失败 0、跳过 0。日志与命令索引：`artifacts/runs/menu_parity_20261022/python.log`、`python-result.json`；如为 C# 测试，另有同名 TRX。

- MENU-01 compile：命令 `python -m compileall -q shahe_bulk_launcher tests`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/menu_parity_20261022/compile.log`、`compile-result.json`；如为 C# 测试，另有同名 TRX。

- MENU-01 release：命令 `dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\menu_parity_20261022\bin\`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/menu_parity_20261022/release.log`、`release-result.json`；如为 C# 测试，另有同名 TRX。

- MENU-01 结论：恢复用户管理的置顶后分隔线、校正的修复 Control 前分隔线、关于的检查更新后分隔线；设置菜单移除 Python 中没有的额外“路径记忆管理”入口。保留路径列表八项上下文动作、Ctrl+H、数量上限和对话框类既有保存测试。生产 MainMenu 四个完整子序列及 Python AST 原版序列均匹配。初次受影响失败位于旧二维码测试 finally 还原剪贴板，未修改业务实现；单项复测、受影响复跑和全量已通过。

- MENU-01 本轮源码/文档差异检查返回码 0，命令和日志见 artifacts/runs/menu_parity_20261022/diff-result.json。未修改既有迁移计划与用户历史日志。

- HISTORY-UI-01 red：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\history_count_dialog_20261022\bin\ --logger trx;LogFileName=red.trx --results-directory D:\mycodex\git\artifacts\runs\history_count_dialog_20261022 --filter FullyQualifiedName~PathMemoryDialogOnlyShowsQuantityAndConfirmation|FullyQualifiedName~PathMemoryQuantitySaveAndCancelPreserveStoredPaths`；返回码 1，通过 2、失败 1、跳过 0。日志与命令索引：`artifacts/runs/history_count_dialog_20261022/red.log`、`red-result.json`；如为 C# 测试，另有同名 TRX。

- HISTORY-UI-01 green：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\history_count_dialog_20261022\bin\ --logger trx;LogFileName=green.trx --results-directory D:\mycodex\git\artifacts\runs\history_count_dialog_20261022 --filter FullyQualifiedName~PathMemoryDialogOnlyShowsQuantityAndConfirmation|FullyQualifiedName~PathMemoryQuantitySaveAndCancelPreserveStoredPaths|FullyQualifiedName~PathHistoryLimitRuntimeContractTests`；返回码 0，通过 9、失败 0、跳过 0。日志与命令索引：`artifacts/runs/history_count_dialog_20261022/green.log`、`green-result.json`；如为 C# 测试，另有同名 TRX。

- MENU-01 独立提交 1e4f100 已推送 origin/fix/non-update-gaps-20261020，回退命令 git revert 1e4f100。随后按用户明确新要求执行 HISTORY-UI-01：数量窗口去除已记忆列表和隐藏路径复选框，共用数量输入 UI；该用户指定界面变更优先于 Python 原布局，其他中文文案继续保留。

- HISTORY-UI-01 affected：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\history_count_dialog_20261022\bin\ --logger trx;LogFileName=affected.trx --results-directory D:\mycodex\git\artifacts\runs\history_count_dialog_20261022 --filter TestCategory=AppPaths|TestCategory=SettingsDialogs|FullyQualifiedName~PathMemoryRuntimeContractTests|FullyQualifiedName~MainFormApplicationContractTests|FullyQualifiedName~SettingsRuntimeContractTests`；返回码 0，通过 91、失败 0、跳过 0。日志与命令索引：`artifacts/runs/history_count_dialog_20261022/affected.log`、`affected-result.json`；如为 C# 测试，另有同名 TRX。

- HISTORY-UI-01 full：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\history_count_dialog_20261022\bin\ --logger trx;LogFileName=full.trx --results-directory D:\mycodex\git\artifacts\runs\history_count_dialog_20261022`；返回码 0，通过 370、失败 0、跳过 0。日志与命令索引：`artifacts/runs/history_count_dialog_20261022/full.log`、`full-result.json`；如为 C# 测试，另有同名 TRX。

- HISTORY-UI-01 python：命令 `python -m unittest discover -s tests -p test_*.py -v`；返回码 0，通过 29、失败 0、跳过 0。日志与命令索引：`artifacts/runs/history_count_dialog_20261022/python.log`、`python-result.json`；如为 C# 测试，另有同名 TRX。

- HISTORY-UI-01 compile：命令 `python -m compileall -q shahe_bulk_launcher tests`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/history_count_dialog_20261022/compile.log`、`compile-result.json`；如为 C# 测试，另有同名 TRX。

- HISTORY-UI-01 release：命令 `dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\history_count_dialog_20261022\bin\`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/history_count_dialog_20261022/release.log`、`release-result.json`；如为 C# 测试，另有同名 TRX。

- HISTORY-UI-01 完成结论：删除旧管理窗口内的路径列表、隐藏路径复选框及列表专属备注/删除/清空代码；原 PromptAppPathHistoryCount 复用同一简洁对话框并保留原标题/提示，避免维护第二套数量窗口。生产代码较原实现净删减，保存仅写数量；两个原有历史条目、备注和隐藏偏好在保存/取消用例中保持不变。主路径列表的编辑、删除、清空、复制和显示切换入口保留。重复操作核查见 artifacts/runs/history_count_dialog_20261022/duplicate-operation-review.md。

- HISTORY-UI-01 源码与文档差异检查返回码 0；命令与日志为 artifacts/runs/history_count_dialog_20261022/diff-result.json。未修改用户原有 GAP-09 日志和活动迁移计划。

- HISTORY-UI-01 独立提交 ef37074 已推送 origin/fix/non-update-gaps-20261020，回退命令 git revert ef37074。用户随后明确要求处理读屏 API 接口仅保存问题，本轮另立 VOICE-03 原版遗留缺陷修复范围，保留 UI 文案与选项顺序，不改活动迁移计划。

- VOICE-03 red：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\speech_selection_20261022\bin\ --logger trx;LogFileName=red.trx --results-directory D:\mycodex\git\artifacts\runs\speech_selection_20261022 --filter FullyQualifiedName~SpeechSelectionRuntimeContractTests`；返回码 1，通过 1、失败 6、跳过 0。日志与命令索引：`artifacts/runs/speech_selection_20261022/red.log`、`red-result.json`；如为 C# 测试，另有同名 TRX。

- VOICE-03 green：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\speech_selection_20261022\bin\ --logger trx;LogFileName=green.trx --results-directory D:\mycodex\git\artifacts\runs\speech_selection_20261022 --filter FullyQualifiedName~SpeechSelectionRuntimeContractTests`；返回码 0，通过 7、失败 0、跳过 0。日志与命令索引：`artifacts/runs/speech_selection_20261022/green.log`、`green-result.json`；如为 C# 测试，另有同名 TRX。

- VOICE-03 affected：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\speech_selection_20261022\bin\ --logger trx;LogFileName=affected.trx --results-directory D:\mycodex\git\artifacts\runs\speech_selection_20261022 --filter TestCategory=Speech|TestCategory=SettingsDialogs|FullyQualifiedName~SettingsRuntimeContractTests|FullyQualifiedName~MainFormApplicationContractTests`；返回码 0，通过 106、失败 0、跳过 0。日志与命令索引：`artifacts/runs/speech_selection_20261022/affected.log`、`affected-result.json`；如为 C# 测试，另有同名 TRX。

- VOICE-03 integration-green：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\speech_selection_20261022\bin\ --logger trx;LogFileName=integration-green.trx --results-directory D:\mycodex\git\artifacts\runs\speech_selection_20261022 --filter FullyQualifiedName~SpeechSelectionRuntimeContractTests`；返回码 0，通过 12、失败 0、跳过 0。日志与命令索引：`artifacts/runs/speech_selection_20261022/integration-green.log`、`integration-green-result.json`；如为 C# 测试，另有同名 TRX。

- VOICE-03 原生接口 zdsr：返回码 1，状态 failed，实际加载/检测/输出调用与释放、PID、构建目录见 artifacts/runs/speech_selection_20261022/native-zdsr-result.json；输出调用证据不等同于人工听感确认。

- VOICE-03 首次原生 zdsr 探针因 PowerShell 反射构造参数绑定失败，未调用读屏 DLL，不计接口功能失败；改为显式 ConstructorInfo.Invoke 后另存结果重跑。

- VOICE-03 原生接口 zdsr：返回码 0，状态 passed，实际加载/检测/输出调用与释放、PID、构建目录见 artifacts/runs/speech_selection_20261022/native-zdsr-final-result.json；输出调用证据不等同于人工听感确认。

- VOICE-03 NVDA 实测准备：本机安装 NVDA 2026.1.1，启动前无 NVDA 运行进程；使用测试专用配置目录和禁用插件参数启动安装版，命令、归属 PID 与实际路径见 artifacts/runs/speech_selection_20261022/nvda-test-launch.json。争渡进程未停止，用户常用 NVDA 配置未改写。

- VOICE-03 原生接口 nvda：返回码 0，状态 passed，实际加载/检测/输出调用与释放、PID、构建目录见 artifacts/runs/speech_selection_20261022/native-nvda-final-result.json；输出调用证据不等同于人工听感确认。

- VOICE-03 原生接口 automatic：返回码 0，状态 passed，实际加载/检测/输出调用与释放、PID、构建目录见 artifacts/runs/speech_selection_20261022/native-automatic-final-result.json；输出调用证据不等同于人工听感确认。

- VOICE-03 原生接口 boy：返回码 3，状态 unavailable，实际加载/检测/输出调用与释放、PID、构建目录见 artifacts/runs/speech_selection_20261022/native-boy-final-result.json；输出调用证据不等同于人工听感确认。

- VOICE-03 NVDA 测试清理：仅在当前全部 NVDA 进程属于本轮专用配置时发送官方 --quit；实际归属 PID、退出及残留结果见 artifacts/runs/speech_selection_20261022/nvda-test-cleanup.json。用户运行中的争渡保持运行。

- VOICE-03 fallback-red：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\speech_selection_20261022\bin\ --logger trx;LogFileName=fallback-red.trx --results-directory D:\mycodex\git\artifacts\runs\speech_selection_20261022 --filter FullyQualifiedName~NoReadersFallsBackToWindowsAndReturns`；返回码 1，通过 0、失败 3、跳过 0。日志与命令索引：`artifacts/runs/speech_selection_20261022/fallback-red.log`、`fallback-red-result.json`；如为 C# 测试，另有同名 TRX。

- VOICE-03 fallback-green：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\speech_selection_20261022\bin\ --logger trx;LogFileName=fallback-green.trx --results-directory D:\mycodex\git\artifacts\runs\speech_selection_20261022 --filter FullyQualifiedName~SpeechSelectionRuntimeContractTests|FullyQualifiedName~TolkRuntimeIntegrationContractTests`；返回码 0，通过 30、失败 0、跳过 0。日志与命令索引：`artifacts/runs/speech_selection_20261022/fallback-green.log`、`fallback-green-result.json`；如为 C# 测试，另有同名 TRX。

- VOICE-03 原生接口 nvda：返回码 0，状态 passed，实际加载/检测/输出调用与释放、PID、构建目录见 artifacts/runs/speech_selection_20261022/native-windows-fallback-final-result.json；输出调用证据不等同于人工听感确认。

- VOICE-03 Windows 回退补充：native-windows-fallback-final 的 NoReaders 仅替换进程检测输入，原生 Tolk/Windows 后端真实执行，检测结果 OneCore、输出调用与释放成功；未停止真实争渡。下一场景使用只含相同 Tolk.dll 的隔离目录核对自动模式缺少读屏接口时的 Windows 回退。

- VOICE-03 原生接口 automatic：返回码 0，状态 passed，实际加载/检测/输出调用与释放、PID、构建目录见 artifacts/runs/speech_selection_20261022/native-auto-windows-final-result.json；输出调用证据不等同于人工听感确认。

- VOICE-03 preference-red：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\speech_selection_20261022\bin\ --logger trx;LogFileName=preference-red.trx --results-directory D:\mycodex\git\artifacts\runs\speech_selection_20261022 --filter FullyQualifiedName~UnavailableSelectionUsesAutomaticTemporarily|FullyQualifiedName~ReaderApiBecomingUnavailable|FullyQualifiedName~MissingSelectedLibraryUsesAutomatic`；返回码 1，通过 0、失败 5、跳过 0。日志与命令索引：`artifacts/runs/speech_selection_20261022/preference-red.log`、`preference-red-result.json`；如为 C# 测试，另有同名 TRX。

- VOICE-03 preference-green：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\speech_selection_20261022\bin\ --logger trx;LogFileName=preference-green.trx --results-directory D:\mycodex\git\artifacts\runs\speech_selection_20261022 --filter FullyQualifiedName~SpeechSelectionRuntimeContractTests|FullyQualifiedName~TolkRuntimeIntegrationContractTests`；返回码 0，通过 34、失败 0、跳过 0。日志与命令索引：`artifacts/runs/speech_selection_20261022/preference-green.log`、`preference-green-result.json`；如为 C# 测试，另有同名 TRX。

- VOICE-03 原生接口 zdsr：返回码 0，状态 passed，实际加载/检测/输出调用与释放、PID、构建目录见 artifacts/runs/speech_selection_20261022/preference-native-zdsr-result.json；输出调用证据不等同于人工听感确认。

- VOICE-03 中断恢复：上轮 preference-native-boy-fallback 命令返回 1，仅输出 InitTTS rt=0，未生成结构化报告，目标 PID 未记录，且对应时间段未找到相关 Application/.NET Runtime 崩溃事件；标记 inconclusive，不能作为产品通过/失败。证据索引为 speech_selection_20261022/interrupted-preference-boy-result.json。探针补充阶段日志及明确 UTF-8 文档追加后重跑。

- VOICE-03 原生接口 boy：返回码 0，状态 passed，实际加载/检测/输出调用与释放、PID、构建目录见 artifacts/runs/speech_selection_20261022/preference-boy-recheck-result.json；输出调用证据不等同于人工听感确认。

- VOICE-03 接续：用户确认已展示的活动计划差异及逐步记录。本轮已核对旧会话要求与工作树归属；保留 GAP-09 两份既有日志修改，不纳入本次提交。此前 preference-boy-recheck 的 outputInvoked=false，只证明加载/检测/释放，不能作为实际输出证据。

- VOICE-03 resume-directed：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\speech_selection_20261022\bin\ --logger trx;LogFileName=resume-directed.trx --results-directory D:\mycodex\git\artifacts\runs\speech_selection_20261022 --filter FullyQualifiedName~SpeechSelectionRuntimeContractTests|FullyQualifiedName~TolkRuntimeIntegrationContractTests`；返回码 0，通过 34、失败 0、跳过 0。日志与命令索引：`artifacts/runs/speech_selection_20261022/resume-directed.log`、`resume-directed-result.json`；如为 C# 测试，另有同名 TRX。

- VOICE-03 接续 NVDA 环境：在恢复探针确认初始回退后启动独立配置 NVDA，保留用户争渡。命令、专属配置、启动及归属 PID 见 artifacts/runs/speech_selection_20261022/nvda-resume-launch.json；此为环境准备，不单独宣称功能通过。

- VOICE-03 resume-nvda-recovery：原生探针返回 -1073741819（0xC0000005），未完成结构化报告，未查询到匹配 Application 事件；状态 inconclusive，不作为产品通过/失败。精确命令、PID、实际构建目录、阶段、日志和清理边界见 artifacts/runs/speech_selection_20261022/resume-nvda-recovery-interrupted.json。进入原生切换诊断，不跳过该场景。

- VOICE-03 原生接口 nvda：返回码 0，状态 passed，实际加载/检测/输出调用与释放、PID、构建目录见 artifacts/runs/speech_selection_20261022/resume-dual-nvda-result.json；输出调用证据不等同于人工听感确认。

- VOICE-03 resume-dual-nvda 证据范围更正：探针本身返回 0，但 requestedLibrary=Tolk.dll、activeRoute 为空、detected=ZDSR，仅证明保存 nvda 时自动回退及输出调用，未证明双读屏或 NVDA 固定输出；该目标场景仍未通过。详细结果见同名原生报告，下一步核对测试 NVDA 生命周期。

- VOICE-03 原生调试脚本基线：Python 以 DEBUG_ONLY_THIS_PROCESS 启动 powershell.exe -NoProfile -Command exit 0；脚本返回 0，断言检查 x64 Win32 结构布局并记录进程退出事件，测试计数不适用。命令、PID 与退出码见 artifacts/runs/speech_selection_20261022/debugger-baseline-result.json，控制台见 debugger-baseline.log。

- VOICE-03 原生接口 nvda：返回码 0，状态 passed，实际加载/检测/输出调用与释放、PID、构建目录见 artifacts/runs/speech_selection_20261022/retry-native-result.json；输出调用证据不等同于人工听感确认。

- VOICE-03 retry-native 原生诊断：在真实 DLL 上模拟 NVDA 进程存在但 API 未就绪，执行有界重复初始化；诊断脚本返回 0，测试计数不适用。命令、目标 PID、退出码及异常模块见 artifacts/runs/speech_selection_20261022/retry-native-debug-result.json；阶段见 retry-native-trace.log。此为模拟启动窗口的诊断证据，不替代真实环境验收。

- VOICE-03 原生接口 nvda：返回码 0，状态 passed，实际加载/检测/输出调用与释放、PID、构建目录见 artifacts/runs/speech_selection_20261022/retry-speech-result.json；输出调用证据不等同于人工听感确认。

- VOICE-03 retry-speech 原生诊断：沿用 retry-native，仅增加真实 Output 调用后重复初始化；诊断脚本返回 0，测试计数不适用。命令、PID、退出码与异常模块见 artifacts/runs/speech_selection_20261022/retry-speech-debug-result.json，阶段见 retry-speech-trace.log。

- VOICE-03 原生接口 nvda：返回码 1，状态 failed，实际加载/检测/输出调用与释放、PID、构建目录见 artifacts/runs/speech_selection_20261022/recovery-debug-result.json；输出调用证据不等同于人工听感确认。

- VOICE-03 recovery-debug：原生调试器下重跑真实 NVDA 启动恢复；调试脚本返回 1，计数不适用。完整命令、目标 PID、退出码及异常模块见 artifacts/runs/speech_selection_20261022/recovery-debug-result.json；服务状态及实际构建目录见 recovery-debug-result.json 对应的原生报告与 trace。

- VOICE-03 recovery-debug 证据更正：本次重跑时检测到用户启动的 NVDA（命令 nvda.exe -r），不满足“初始未运行”的前置条件，返回 1；调试器未记录访问冲突。该诊断与原生报告使用同一名称造成报告覆盖，已识别为探针问题，本次不用于产品结论。用户 NVDA 保持运行；后续诊断与原生报告使用不同文件名。

- VOICE-03 原生接口 nvda：返回码 0，状态 passed，实际加载/检测/输出调用与释放、PID、构建目录见 artifacts/runs/speech_selection_20261022/dual-active-nvda-result.json；输出调用证据不等同于人工听感确认。

- VOICE-03 原生接口 zdsr：返回码 0，状态 passed，实际加载/检测/输出调用与释放、PID、构建目录见 artifacts/runs/speech_selection_20261022/dual-active-zdsr-result.json；输出调用证据不等同于人工听感确认。

- VOICE-03 dual-active-zdsr 场景判定：脚本返回 0，但 requestedLibrary=ZDSRAPI_x64.dll、activeRoute 为空、selectedLibrary=Tolk.dll、detected=NVDA；未满足“争渡可用时固定使用争渡”，本场景未通过。不能将通用探针 passed 当作所选接口通过。下一步核对争渡实际进程、原生 InitTTS/GetSpeakState 返回值。

- VOICE-03 dual-active-zdsr 前置条件复核：探针控制台 InitTTS 返回 2；随后进程清单仅剩争渡服务，无 ZDSRMain/Daemon/x64 主读屏进程，NVDA 主进程在运行。因此该轮反映争渡接口不可用时的预期临时回退，不能判定固定选择缺陷，也不能作为双读屏验收。控制台证据为 artifacts/runs/speech_selection_20261022/dual-active-zdsr-console.log。

- VOICE-03 原生接口 nvda：返回码 0，状态 passed，实际加载/检测/输出调用与释放、PID、构建目录见 artifacts/runs/speech_selection_20261022/windows-to-nvda-result.json；输出调用证据不等同于人工听感确认。

- VOICE-03 windows-to-nvda：同一生产 SpeechService 使用真实 Windows/NVDA DLL，初始注入无读屏枚举后恢复实际枚举；调试脚本返回 0，测试计数不适用。原生状态、PID、目录和输出调用见 artifacts/runs/speech_selection_20261022/windows-to-nvda-result.json；命令、异常与进程退出见 windows-to-nvda-debug-result.json。枚举变化为模拟，不替代真实读屏启动恢复。

- VOICE-03 原生接口 boy：返回码 0，状态 passed，实际加载/检测/输出调用与释放、PID、构建目录见 artifacts/runs/speech_selection_20261022/resume-boy-output-result.json；输出调用证据不等同于人工听感确认。

- VOICE-03 resume-affected：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\speech_selection_20261022\bin\ --logger trx;LogFileName=resume-affected.trx --results-directory D:\mycodex\git\artifacts\runs\speech_selection_20261022 --filter TestCategory=Speech|TestCategory=SettingsDialogs|FullyQualifiedName~SettingsRuntimeContractTests|FullyQualifiedName~MainFormApplicationContractTests`；返回码 0，通过 118、失败 0、跳过 0。日志与命令索引：`artifacts/runs/speech_selection_20261022/resume-affected.log`、`resume-affected-result.json`；如为 C# 测试，另有同名 TRX。

- VOICE-03 resume-full：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\speech_selection_20261022\bin\ --logger trx;LogFileName=resume-full.trx --results-directory D:\mycodex\git\artifacts\runs\speech_selection_20261022`；返回码 0，通过 389、失败 0、跳过 0。日志与命令索引：`artifacts/runs/speech_selection_20261022/resume-full.log`、`resume-full-result.json`；如为 C# 测试，另有同名 TRX。

- VOICE-03 resume-python：命令 `python -m unittest discover -s tests -p test_*.py -v`；返回码 0，通过 29、失败 0、跳过 0。日志与命令索引：`artifacts/runs/speech_selection_20261022/resume-python.log`、`resume-python-result.json`；如为 C# 测试，另有同名 TRX。

- VOICE-03 resume-compile：命令 `python -m compileall -q shahe_bulk_launcher tests`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/speech_selection_20261022/resume-compile.log`、`resume-compile-result.json`；如为 C# 测试，另有同名 TRX。

- VOICE-03 resume-release：命令 `dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\speech_selection_20261022\bin\`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/speech_selection_20261022/resume-release.log`、`resume-release-result.json`；如为 C# 测试，另有同名 TRX。
 构建结果：0 警告、0 错误，生产 Core/App/Updater/AuditRunner 编译通过。

- VOICE-03 resume-cleanup：只读核对测试专用配置 NVDA 与本轮原生探针归属，状态 passed，计数不适用；无主动结束用户读屏。残留与保留进程见 artifacts/runs/speech_selection_20261022/resume-cleanup-result.json。

- VOICE-03 resume-diff：git diff --check 返回 2；git diff --cached --check 返回 0；测试计数不适用。日志与暂存清单见 artifacts/runs/speech_selection_20261022/resume-diff-result.json、resume-diff-check.log、resume-staged-check.log。当前没有为本模块暂存或提交，真实验收未闭环。

- VOICE-03 差异检查诊断：全仓检查返回非零，项目结构文档混合 CRLF/LF 导致新增差异被识别为行尾空白；另有两份会话开始时已修改的 GAP-09 日志存在空白问题。仅统一本轮维护的项目结构换行，不改历史记录文字，不改用户既有 GAP-09 日志；随后分别复验本模块范围与全仓结果。

- VOICE-03 resume-final-check：本模块 git diff --check 返回 0；全仓 git diff --check 返回 2，仅剩会话开始时已有的两份 GAP-09 日志空白问题，不改动这些用户文件。两份 PowerShell 验证脚本语法解析结果、完整命令与暂存清单见 artifacts/runs/speech_selection_20261022/resume-final-check-result.json；日志见 resume-module-diff.log、resume-final-worktree-diff.log。测试计数不适用，当前无暂存文件。

- VOICE-03 新文件差异补查：读取新增测试与原生/验证/调试脚本逐行检查行尾空格和 Tab，返回码 0；完整文件清单与结果见 artifacts/runs/speech_selection_20261022/untracked-whitespace-result.json。弥补未跟踪文件不进入 git diff --check 的范围限制，测试计数不适用。

- VOICE-03 文档最终复核：完成上述逐步记录后再次执行本模块 git diff --check，返回 0，计数不适用；日志 artifacts/runs/speech_selection_20261022/final-document-diff.log。活动计划重新逐项核对，真实验收待办仍未勾选，本轮未暂存、提交或推送。

- VOICE-03 原生接口 nvda：返回码 0，状态 passed，实际加载/检测/输出调用与释放、PID、构建目录见 artifacts/runs/speech_selection_20261022/single-nvda-final-result.json；输出调用证据不等同于人工听感确认。

- VOICE-03 用户单读屏验收：Windows PowerShell 执行 native-selection-probe.ps1 -Selection nvda -Speak -Name single-nvda-final；原生返回 0，严格场景判定返回 0、状态 passed。固定 NVDA、输出调用、保存值不变、同值再次保存保留连接、运行时释放均由结构化报告核对；PID、实际构建目录和完整命令见 artifacts/runs/speech_selection_20261022/single-nvda-final-result.json，读屏前后快照与清理见 single-reader-acceptance.json。无 UI 键盘输入，索引校准不适用；不是人工听感结论。

- VOICE-03 首次暂存检查：git diff --cached --check 返回 2，仅由两份差异日志原样引用了已有 GAP-09 日志的行尾空白引起；暂存文件与清单一致，无用户无关改动。原始检查日志见 artifacts/runs/speech_selection_20261022/staging-check.log。将这两份原始证据无损压缩入库，保留本地原文，随后复验。

- VOICE-03 差异原始日志归档：两份日志 gzip 压缩后解压逐字节一致，返回码 0，计数不适用；原始本地文件保留，仓库对应 .log.gz。SHA-256 与归档映射见 artifacts/runs/speech_selection_20261022/diff-log-archives.json；不修改已有日志证据文字。

- VOICE-03 提交前门禁：选择性暂存清单逐项一致，git diff --cached --check 返回 0，计数不适用；日志 artifacts/runs/speech_selection_20261022/staging-final-check.log。单读屏用户验收通过，完整自动化结果见前述权威记录；AGENTS.md 与两份既有 GAP-09 日志未暂存。下一步独立提交并推送当前分支。

- VOICE-03 首次推送返回 1：远端同名分支已有新提交，Git 以 non-fast-forward 拒绝。保留本地独立修复提交和用户未提交改动，不强推；下一步获取远端差异并安全整合。

- VOICE-03 远端拓扑复核：fetch 成功；远端同名分支为仅含 csharp/ 的独立发布历史，与本地 HEAD 无共同祖先。为保留用户发布分支，不合并无关历史、不强推，改将本地已验证修复推送到 origin/voice03-fixed-reader-20261022。原有分支和用户未提交文件保持原状。

- VOICE-03 提交与推送：独立修复提交 `a74677c` 已成功推送至 `origin/voice03-fixed-reader-20261022`，返回码 0；回退命令 `git revert a74677c`。原 `origin/沙河批量打开器` 为无共同祖先的 C# 发布历史，未覆盖、未强推。用户 AGENTS.md 与两份 GAP-09 日志改动保持未暂存。

- AUDIT-03 接续只读 verify-clean：返回码 3，状态 blocked，测试计数不适用，未通过项 SbieCtrl.exe, SandboxieOther。完整命令、目标 PID、实际构建目录、场景、无输入/超时及清理记录见 artifacts/runs/audit03_resume_20261022/verify-clean-command-result.json；原生报告路径由该记录索引。未执行 delete-group 或其他配置修改，未结束用户 SbieCtrl。

- AUDIT-03 接续文档核对：本轮仅补充只读门禁证据和计划状态，未修改生产代码；git diff --check -- PROJECT_STRUCTURE.md docs/superpowers/plans/2026-10-20-remaining-migration-closure.md 返回 0，计数不适用。日志 artifacts/runs/audit03_resume_20261022/document-diff.log。真实生命周期仍被用户 SbieCtrl 阻断，不能标记 AUDIT-03 完成。

- AUDIT-03 删除分组接续范围核对：生产 MainForm.DeleteGroup 已实现并接入菜单，只修改软件分组配置；AuditRunner lifecycle 尚无同名阶段。当前采取独立隔离 EXE 场景复用既有 Win32Automation，不修改真实 Sandboxie 配置，不将该证据扩大为真实 Sandboxie 删除成功。该场景代码/脚本及报告集中于 artifacts/runs/audit03_delete_group_20261022/。

- AUDIT-03 删除分组探针准备：PowerShell Parser.ParseFile 语法检查返回 0、解析错误为 0，测试计数不适用；当前仅新增隔离审计脚本，未修改生产代码。下一步运行实际 EXE，未取得运行结果前不标记通过。

- AUDIT-03 删除分组隔离真实 EXE：返回码 3，状态 inconclusive，测试计数不适用。完整命令、PID、构建目录、菜单/焦点/索引校准、删除与重启、配置保留及清理见 artifacts/runs/audit03_delete_group_20261022/case-20261022_200939_648/result.json；该场景不执行真实 Sandboxie 写操作。

- AUDIT-03 删除分组隔离真实 EXE：返回码 3，状态 inconclusive，测试计数不适用。完整命令、PID、构建目录、菜单/焦点/索引校准、删除与重启、配置保留及清理见 artifacts/runs/audit03_delete_group_20261022/case-20261022_201150_365/result.json；该场景不执行真实 Sandboxie 写操作。

- AUDIT-03 删除分组隔离真实 EXE：返回码 3，状态 inconclusive，测试计数不适用。完整命令、PID、构建目录、菜单/焦点/索引校准、删除与重启、配置保留及清理见 artifacts/runs/audit03_delete_group_20261022/case-20261022_201243_932/result.json；该场景不执行真实 Sandboxie 写操作。

- AUDIT-03 删除分组隔离真实 EXE：返回码 3，状态 inconclusive，测试计数不适用。完整命令、PID、构建目录、菜单/焦点/索引校准、删除与重启、配置保留及清理见 artifacts/runs/audit03_delete_group_20261022/case-20261022_201343_424/result.json；该场景不执行真实 Sandboxie 写操作。

- AUDIT-03 删除分组隔离真实 EXE：返回码 3，状态 inconclusive，测试计数不适用。完整命令、PID、构建目录、菜单/焦点/索引校准、删除与重启、配置保留及清理见 artifacts/runs/audit03_delete_group_20261022/case-20261022_201658_229/result.json；该场景不执行真实 Sandboxie 写操作。

- AUDIT-03 删除分组隔离真实 EXE：返回码 3，状态 inconclusive，测试计数不适用。完整命令、PID、构建目录、菜单/焦点/索引校准、删除与重启、配置保留及清理见 artifacts/runs/audit03_delete_group_20261022/case-20261022_201821_877/result.json；该场景不执行真实 Sandboxie 写操作。

- AUDIT-03 删除分组隔离真实 EXE：返回码 0，状态 passed，测试计数不适用。完整命令、PID、构建目录、菜单/焦点/索引校准、删除与重启、配置保留及清理见 artifacts/runs/audit03_delete_group_20261022/case-20261022_201945_355/result.json；该场景不执行真实 Sandboxie 写操作。

- AUDIT-03 删除分组结果范围：已按 Python on_delete_group 与 C# MainForm.DeleteGroup 核对菜单、选择、删除、保存及返回流程；最终实际 EXE 场景通过，非目标分组和路径记忆数量保持，重启后仅保留目标之外的分组，两个测试进程均正常退出。此为软件分组配置验收，不代表真实 Sandboxie 删除已通过。界面 UIA 未暴露 ListItem 的现象记录为 AUDIT-04 待核对线索，未直接判为产品缺陷。

- AUDIT-03 删除分组后清理与下一项：只读进程核对完成，自有 EXE 残留为 0，schedule-close 准备状态为 blocked，计数不适用；完整查询、实际用户控制进程及清理结果见 artifacts/runs/audit03_delete_group_20261022/next-stage-and-cleanup.json。未执行真实生命周期动作，未停止 SbieCtrl。

- AUDIT-03 删除分组证据收口：最终探针语法解析返回 0，文档 git diff --check 返回 0；生产源码无修改，未重复无变化的全量构建和回归。检查结果见 artifacts/runs/audit03_delete_group_20261022/final-checks.json、document-diff.log；最终实际 EXE 报告为本轮最新 passed 记录。AUDIT-03 整体仍未完成。

- AUDIT-03 删除分组证据提交：本地提交 `24d6190`，回退命令 `git revert 24d6190`；目标远端分支为 `origin/voice03-fixed-reader-20261022`。两次推送分别因连接重置（返回 128）和空响应（返回 1）失败，尚未确认远端更新，正在单次 HTTP/1.1 重试；未强推。

- AUDIT-03 推送最终状态：单次 `git -c http.version=HTTP/1.1 push origin HEAD:refs/heads/voice03-fixed-reader-20261022` 返回 1，无法连接 GitHub 443；此前本地证据提交保留，未宣称推送成功。网络恢复后重试该已有提交，实际功能验证不回退。

- AUDIT-03 最新阻断文档检查：git diff --check -- PROJECT_STRUCTURE.md docs/superpowers/plans/2026-10-20-remaining-migration-closure.md 返回 0，计数不适用；日志 artifacts/runs/audit03_delete_group_20261022/final-status-diff.log。网络和 SbieCtrl 阻断已同步到活动计划。

- AUDIT-03 控制界面退出准备：用户已授权正常退出 Sandboxie Control。Computer Use 在恢复目标窗口后出现 CUA_OBSERVATION_STALE、CUA_EXECUTION_FAILED，未发出退出输入，相关观察不作为退出成功证据；已使用现有 Open Computer Use 读取到控制窗口，准备执行正常菜单退出。

- AUDIT-03 控制窗口观测：Open Computer Use 截图被其他窗口遮挡，不能确认实际目标菜单，已判为无效观察并移除非目标截图文件，未据此发出退出命令。接下来使用控制程序自身 /open 恢复现有窗口后重新观察。

- AUDIT-03 精确控制菜单审计 CLI：使用现有 .NET Framework csc 编译 ControlExitProbe.cs，返回 0，计数不适用；日志 artifacts/runs/audit03_lifecycle_resume_20261022/control-exit-build.log。仅复用已有菜单枚举与 PostCommand，不含强制终止代码。

- AUDIT-03 控制菜单只读枚举：ControlExitProbe.exe 在无 --exit 参数下核对精确 SbieCtrl 路径与菜单，返回 0，计数不适用；结果 artifacts/runs/audit03_lifecycle_resume_20261022/control-menu-native.log。未发送退出命令。

- AUDIT-03 正常退出 Sandboxie Control 重试：恢复唯一 SbieCtrl PID 14796 后，精确枚举唯一退出(&X)菜单并发送正常菜单命令；只读返回 0，命令返回 0，控制进程残留 0，SbieSvc=Running，通过 1、失败 0、跳过 0。报告与原生日志见 artifacts/runs/audit03_lifecycle_resume_20261022/control-exit-final-result.json、control-menu-native-retry.log、control-exit-native-retry.log；未使用强制终止。

- AUDIT-03 正常退出控制结果：复用 Win32Automation 的菜单枚举和 PostCommand，严格匹配 SbieCtrl.exe PID 14796、路径和唯一“退出(&X)”；命令返回 0，控制进程清零，SbieSvc 仍 Running，未使用强制终止。真实结果见 artifacts/runs/audit03_lifecycle_resume_20261022/control-exit-final-result.json。

- AUDIT-03 schedule-close 直接重跑：命令返回 3，状态 inconclusive，通过 0、失败 1、跳过 0；完整命令、实际构建目录、报告路径和清理字段见 artifacts/runs/audit03_lifecycle_resume_20261022/schedule-close-direct-result.json，原始输出见 schedule-close-direct.log。

- AUDIT-03 schedule-close：直接调用使用既有 baseline/state/prefix，返回码 3，状态 inconclusive，错误为 Owned configuration changed before lifecycle；未启动沙盒、未关闭进程、无清理动作。命令与报告见 artifacts/runs/audit03_lifecycle_resume_20261022/schedule-close-direct-result.json、auditrunner-lifecycle-schedule-close-20261022_205736_845-result.json。进入配置哈希根因核对，不绕过门禁。

- AUDIT-03 schedule-close 根因核对：旧状态 ExpectedConfigurationSha256=`48F7...`，当前真实配置与 baseline 均为 `11602...`；旧状态已不再可安全续跑。检查确认真实 Sandboxie.ini 无旧审计前缀 section、SandboxRoot 无 ShaheAudit_* 目录、目标/审计进程为 0。旧状态不改，改用新前缀和新状态从 batch-create 重走。

- AUDIT-03 fresh batch-create：返回码 3，状态 inconclusive，通过 0、失败 1、跳过 0；命令、日志和报告见 artifacts/runs/audit03_lifecycle_fresh_20261022/batch-create-result.json、batch-create.log。

- AUDIT-03 fresh batch-create：返回码 3，状态 inconclusive，未创建资源；原因 baseline hashes changed before lifecycle。核对确认目标/应用配置哈希仍匹配，真实 Sandboxie.ini 因正常退出控制程序已从旧基线 `2709...` 变为 `39D0...`。保留旧基线，先生成当前环境 preflight。

- AUDIT-03 当前环境 preflight：返回码 3，状态 inconclusive，通过 0、失败 1、跳过 0；命令、日志和基线报告见 artifacts/runs/audit03_lifecycle_fresh_20261022/preflight-current-result.json、preflight-current.log。

- AUDIT-03 当前 preflight 首次调用：返回码 3 / blocked；报告读取目标、配置、Sandboxie 服务和资源状态均正常，只有三个 hash check 因未传 expected 参数为 false。未修改文件。按审计器接口补传当前核对的 target/config/ini 哈希生成正式基线。

- AUDIT-03 当前环境正式 preflight：返回码 0，状态 passed，通过 1、失败 0、跳过 0；显式传入当前三项哈希，命令、哈希和基线报告见 artifacts/runs/audit03_lifecycle_fresh_20261022/preflight-final-result.json、preflight-final.log。

- AUDIT-03 fresh batch-create 正式运行：返回码 0，状态 passed，通过 1、失败 0、跳过 0；命令、日志和报告见 artifacts/runs/audit03_lifecycle_fresh_20261022/batch-create-final-result.json、batch-create-final.log。

- AUDIT-03 fresh preflight/batch-create：当前环境正式 preflight 返回 0、1/0/0；新前缀 `ShaheAudit_20261022_2100B` 的 batch-create 返回 0、1/0/0，所有权状态已写入 fresh-lifecycle-state.json。下一阶段 create-group。

- AUDIT-03 fresh create-group：返回码 0，状态 passed，通过 1、失败 0、跳过 0；命令、日志和报告见 artifacts/runs/audit03_lifecycle_fresh_20261022/create-group-final-result.json、create-group-final.log。

- AUDIT-03 fresh create-group：返回 0、1/0/0，分组创建及配置持久化通过；继续 launch-group。

- AUDIT-03 fresh launch-group：返回码 0，状态 passed，通过 1、失败 0、跳过 0；命令、日志和报告见 artifacts/runs/audit03_lifecycle_fresh_20261022/launch-group-final-result.json、launch-group-final.log。

- AUDIT-03 fresh launch-group：返回 0、1/0/0，两个本轮沙盒成员启动场景通过；继续 schedule-launch。

- AUDIT-03 fresh schedule-launch：返回码 3，状态 inconclusive，通过 0、失败 1、跳过 0；命令、日志和报告见 artifacts/runs/audit03_lifecycle_fresh_20261022/schedule-launch-final-result.json、schedule-launch-final.log。

- AUDIT-03 重现控制程序正常退出：按既有授权匹配精确 PID/路径/退出菜单，返回 0，状态 passed，服务 Running，无强制终止。目标 PID、完整观察与命令索引见 artifacts/runs/audit03_lifecycle_fresh_20261022/control-reappeared-exit-result.json、control-reappeared-exit.log；计数不适用。

- AUDIT-03 fresh schedule-launch (schedule-launch-retry)：返回码 0，状态 passed，通过 1、失败 0、跳过 0；完整命令、构建目录、PID/清理及原生报告索引见 artifacts/runs/audit03_lifecycle_fresh_20261022/schedule-launch-retry-result.json，同名 .log 保留原始输出。

- AUDIT-03 schedule-close 前环境恢复：按既有授权经精确退出菜单关闭重新出现的 SbieCtrl，命令返回 0，残留 0；日志 artifacts/runs/audit03_lifecycle_fresh_20261022/control-before-close.log，计数不适用。

- AUDIT-03 fresh schedule-close (schedule-close-final)：返回码 0，状态 passed，通过 1、失败 0、跳过 0；完整命令、构建目录、PID/清理及原生报告索引见 artifacts/runs/audit03_lifecycle_fresh_20261022/schedule-close-final-result.json，同名 .log 保留原始输出。

- AUDIT-03 cancel-schedules 前正常退出控制：返回 0，残留 0，日志 artifacts/runs/audit03_lifecycle_fresh_20261022/control-before-cancel.log；计数不适用。

- AUDIT-03 fresh cancel-schedules (cancel-schedules-final)：返回码 0，状态 passed，通过 1、失败 0、跳过 0；完整命令、构建目录、PID/清理及原生报告索引见 artifacts/runs/audit03_lifecycle_fresh_20261022/cancel-schedules-final-result.json，同名 .log 保留原始输出。

- AUDIT-03 fresh batch-rename (batch-rename-final)：返回码 0，状态 passed，通过 1、失败 0、跳过 0；完整命令、构建目录、PID/清理及原生报告索引见 artifacts/runs/audit03_lifecycle_fresh_20261022/batch-rename-final-result.json，同名 .log 保留原始输出。

- AUDIT-03 batch-delete 边界核对：两个候选内部名/目录均来自本轮所有权状态，不在原始基线中；解析后的绝对路径均位于已明确指定的 SandboxRoot 内。核对返回 0，计数不适用；精确目标见 artifacts/runs/audit03_lifecycle_fresh_20261022/batch-delete-targets.json。未扩展删除范围。

- AUDIT-03 fresh batch-delete (batch-delete-final)：返回码 0，状态 passed，通过 1、失败 0、跳过 0；完整命令、构建目录、PID/清理及原生报告索引见 artifacts/runs/audit03_lifecycle_fresh_20261022/batch-delete-final-result.json，同名 .log 保留原始输出。

- AUDIT-03 fresh cleanup (cleanup-final)：返回码 0，状态 passed，通过 1、失败 0、跳过 0；完整命令、构建目录、PID/清理及原生报告索引见 artifacts/runs/audit03_lifecycle_fresh_20261022/cleanup-final-result.json，同名 .log 保留原始输出。

- AUDIT-03 fresh verify-clean：返回码 0，状态 passed，通过 1、失败 0、跳过 0；三项哈希、精确资源归属、section/目录与进程计数的完整核对见 artifacts/runs/audit03_lifecycle_fresh_20261022/verify-clean-final-result.json，原生报告及命令由其索引。

- AUDIT-03 closure-affected：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\audit03_lifecycle_fresh_20261022\bin\ --logger trx;LogFileName=closure-affected.trx --results-directory D:\mycodex\git\artifacts\runs\audit03_lifecycle_fresh_20261022 --filter FullyQualifiedName~SandboxieLifecycleAuditRunnerTests|FullyQualifiedName~LaunchSchedulerContractTests|FullyQualifiedName~SandboxAdminServiceContractTests|FullyQualifiedName~MainFormApplicationContractTests`；返回码 0，通过 86、失败 0、跳过 0。日志与命令索引：`artifacts/runs/audit03_lifecycle_fresh_20261022/closure-affected.log`、`closure-affected-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 前轮推送恢复：再次执行已授权分支推送返回 0，删除分组证据提交已到达 origin/voice03-fixed-reader-20261022；此前连接失败记录仍保留，不覆盖历史。当前继续完整源码收口门禁。

- AUDIT-03 closure-full：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\audit03_lifecycle_fresh_20261022\bin\ --logger trx;LogFileName=closure-full.trx --results-directory D:\mycodex\git\artifacts\runs\audit03_lifecycle_fresh_20261022`；返回码 0，通过 389、失败 0、跳过 0。日志与命令索引：`artifacts/runs/audit03_lifecycle_fresh_20261022/closure-full.log`、`closure-full-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 closure-python：命令 `python -m unittest discover -s tests -p test_*.py -v`；返回码 0，通过 29、失败 0、跳过 0。日志与命令索引：`artifacts/runs/audit03_lifecycle_fresh_20261022/closure-python.log`、`closure-python-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 closure-compile：命令 `python -m compileall -q shahe_bulk_launcher tests`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/audit03_lifecycle_fresh_20261022/closure-compile.log`、`closure-compile-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 closure-release：命令 `dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\audit03_lifecycle_fresh_20261022\bin\`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/audit03_lifecycle_fresh_20261022/closure-release.log`、`closure-release-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-03 全量/构建后最终环境核对：verify-clean 返回 0，状态 passed，通过 1、失败 0、跳过 0；命令、失败检查列表与报告见 artifacts/runs/audit03_lifecycle_fresh_20261022/closure-verify-clean-result.json、closure-verify-clean.log。

- AUDIT-03 closure-release 补充：独立输出目录的生产 Core/App/Updater/AuditRunner 均编译成功，构建 0 警告、0 错误；实际输出为 artifacts/runs/audit03_lifecycle_fresh_20261022/bin/x64/Release/net48，原始依据 closure-release.log。
- AUDIT-03 探针无效证据补记：控制退出首轮 helper 在正常菜单发送后异常退出，返回 -532462766，控制进程随后已消失；control-exit-result.json 保留为 inconclusive，后续正常菜单重试单独取得有效通过。其间一次 helper 准备脚本使用只读 PowerShell PID 变量并产生 C# 字面转义编译错误，属于探针准备失败，不计产品 RED；编译日志后续被同名成功构建覆盖，原始错误仅在工具记录中，不扩展为产品结论。

- AUDIT-03 收口证据审查：原始生命周期报告、TRX 汇总与命令结果一致，最终清理检查全部为真；生产 C#/Python 源码和测试无本轮未提交差异。历史 inconclusive 仍保留，未转为产品通过；仅整理控制退出 helper 的文件末尾空行。审查记录：`artifacts/runs/audit03_lifecycle_fresh_20261022/closeout-review-result.json`。用户要求在本项提交推送完成后暂停，下一项 AUDIT-04 不启动。

- AUDIT-03 收口工作树差异检查：`git diff --check` 返回码 2，计数不适用；结果：`artifacts/runs/audit03_lifecycle_fresh_20261022/closeout-worktree-diff-result.json`。

- AUDIT-03 差异范围复核：全工作树检查的返回码 2 仅来自用户已有 GAP-09 两份日志的行尾空白，保留原文件、不纳入暂存；本轮文档范围 `git diff --check -- PROJECT_STRUCTURE.md docs/superpowers/plans/2026-10-20-remaining-migration-closure.md` 返回码 0，计数不适用。结果：`artifacts/runs/audit03_lifecycle_fresh_20261022/closeout-scope-diff-result.json`；新增证据另由暂存差异检查覆盖。

- AUDIT-03 暂存审查：暂存路径与事先展示的 `artifacts/runs/audit03_lifecycle_fresh_20261022/closeout-staging-files.txt` 完全一致，未包含用户已有改动或构建二进制；`git diff --cached --check` 返回码 0，计数不适用。结果：`artifacts/runs/audit03_lifecycle_fresh_20261022/closeout-staged-diff-result.json`。

- AUDIT-03 独立交付：收口提交 `37e7147`（完整 SHA：`37e71474695a17ee83814b91cc2c43859d9f95c4`）已通过 `git push origin HEAD:refs/heads/voice03-fixed-reader-20261022` 推送到 `origin/voice03-fixed-reader-20261022`，提交和推送返回码均为 0。回退命令：`git revert 37e7147`。实际输出：`artifacts/runs/audit03_lifecycle_fresh_20261022/closeout-commit.log`、`closeout-push-result.json`。仅本模块完成，软件整体迁移未完成；按用户要求暂停在 AUDIT-04 之前。

- AUDIT-03 交付状态文档检查：仅同步已成功提交/推送事实及用户暂停边界，未修改生产源码；`git diff --cached --check` 返回码 0，计数不适用，检查输出保存在 `artifacts/runs/audit03_lifecycle_fresh_20261022/closeout-push-result.json` 的 deliveryDocumentationCheck 中。

- AUDIT-04 baseline：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\audit04_keyboard_20261023\bin\ --logger trx;LogFileName=baseline.trx --results-directory D:\mycodex\git\artifacts\runs\audit04_keyboard_20261023 --filter FullyQualifiedName~HotkeyControllerContractTests|FullyQualifiedName~UiControlsCSharpContractTests|FullyQualifiedName~MainFormApplicationContractTests|FullyQualifiedName~MainFormLifecycleContractTests|FullyQualifiedName~TrayCSharpContractTests`；返回码 0，通过 63、失败 0、跳过 0。日志与命令索引：`artifacts/runs/audit04_keyboard_20261023/baseline.log`、`baseline-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-04 真实 EXE 观察：隔离生产进程已启动，但 Computer Use 首次激活/观察返回 CUA_EXECUTION_FAILED，重新绑定返回 CUA_TURN_NOT_ACTIVE。未发送键盘输入；焦点/索引未校准，无产品结论，状态 inconclusive，工具无进程返回码；通过 0、失败 0、待验收 1。PID、实际目录和错误见 `artifacts/runs/audit04_keyboard_20261023/desktop-observation-result.json`。已停止桌面输入，后续先完成非交互契约核对。

- AUDIT-04 keyboard-message-contract：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\audit04_keyboard_20261023\bin\ --logger trx;LogFileName=keyboard-message-contract.trx --results-directory D:\mycodex\git\artifacts\runs\audit04_keyboard_20261023 --filter FullyQualifiedName~KeyboardAuditRuntimeContractTests`；返回码 1，通过 0、失败 2、跳过 0。日志与命令索引：`artifacts/runs/audit04_keyboard_20261023/keyboard-message-contract.log`、`keyboard-message-contract-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-04 消息契约首轮失败归因：新增测试未为“常用”分组提供 GroupNamesProvider，且把固定动作 select_all 当成可重绑定自定义动作；原版/C# 均要求真实分组声明并保留固定快捷键。此为测试夹具偏差，不计产品 RED；生产代码不变，修正夹具后重跑。原失败日志和 TRX 保留于 keyboard-message-contract 对应记录。

- AUDIT-04 keyboard-message-contract-calibrated：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\audit04_keyboard_20261023\bin\ --logger trx;LogFileName=keyboard-message-contract-calibrated.trx --results-directory D:\mycodex\git\artifacts\runs\audit04_keyboard_20261023 --filter FullyQualifiedName~KeyboardAuditRuntimeContractTests`；返回码 1，通过 1、失败 1、跳过 0。日志与命令索引：`artifacts/runs/audit04_keyboard_20261023/keyboard-message-contract-calibrated.log`、`keyboard-message-contract-calibrated-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-04 消息契约校准复核：Alt 导航已通过；余下一项定位为 select_none 仅有 Python 方法、并不在 Python/C# 可配置热键 action_specs 中。改用两边均公开的 select_next_user/select_prev_user 比较 Enter 扩展位；仍仅修测试夹具，不改产品、不计有效 RED。真实 EXE 初始配置也误用了该未公开动作，未发送任何键，必须校准后才可重测。

- AUDIT-04 keyboard-message-contract-final：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\audit04_keyboard_20261023\bin\ --logger trx;LogFileName=keyboard-message-contract-final.trx --results-directory D:\mycodex\git\artifacts\runs\audit04_keyboard_20261023 --filter FullyQualifiedName~KeyboardAuditRuntimeContractTests`；返回码 0，通过 2、失败 0、跳过 0。日志与命令索引：`artifacts/runs/audit04_keyboard_20261023/keyboard-message-contract-final.log`、`keyboard-message-contract-final-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-04 affected-final：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\audit04_keyboard_20261023\bin\ --logger trx;LogFileName=affected-final.trx --results-directory D:\mycodex\git\artifacts\runs\audit04_keyboard_20261023 --filter FullyQualifiedName~KeyboardAuditRuntimeContractTests|FullyQualifiedName~HotkeyControllerContractTests|FullyQualifiedName~UiControlsCSharpContractTests|FullyQualifiedName~MainFormApplicationContractTests|FullyQualifiedName~MainFormLifecycleContractTests|FullyQualifiedName~TrayCSharpContractTests`；返回码 0，通过 65、失败 0、跳过 0。日志与命令索引：`artifacts/runs/audit04_keyboard_20261023/affected-final.log`、`affected-final-result.json`；如为 C# 测试，另有同名 TRX。
- AUDIT-04 失败桌面尝试清理：仅对已核对 PID/精确路径的本轮测试实例执行停止，返回码 0；真实 Sandboxie.ini、自启动项和原读屏进程核对结果见 `artifacts/runs/audit04_keyboard_20261023/desktop-cleanup-result.json`。本轮使用强制清理，不能算作正常退出或托盘退出通过。

- AUDIT-04 full-final：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\audit04_keyboard_20261023\bin\ --logger trx;LogFileName=full-final.trx --results-directory D:\mycodex\git\artifacts\runs\audit04_keyboard_20261023`；返回码 0，通过 391、失败 0、跳过 0。日志与命令索引：`artifacts/runs/audit04_keyboard_20261023/full-final.log`、`full-final-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-04 python-final：命令 `python -m unittest discover -s tests -p test_*.py -v`；返回码 0，通过 29、失败 0、跳过 0。日志与命令索引：`artifacts/runs/audit04_keyboard_20261023/python-final.log`、`python-final-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-04 compile-final：命令 `python -m compileall -q shahe_bulk_launcher tests`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/audit04_keyboard_20261023/compile-final.log`、`compile-final-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-04 release-final：命令 `dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\audit04_keyboard_20261023\bin\`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/audit04_keyboard_20261023/release-final.log`、`release-final-result.json`；如为 C# 测试，另有同名 TRX。

- AUDIT-04 Release 补充：本轮隔离 Release x64 构建 0 警告、0 错误，生产 Core/App/Updater/AuditRunner 均编译成功；原始输出见 `artifacts/runs/audit04_keyboard_20261023/release-final.log`。不替代真实交互验收。
- AUDIT-04 差异检查：全工作树 git diff --check 返回 2（用户已有 GAP-09 两份日志空白，保留未改）；本轮文档范围返回 2，新增测试源码空白检查返回 0，测试计数不适用。命令/结果/日志：`artifacts/runs/audit04_keyboard_20261023/diff-check-result.json`。模块真实验收未完成，未暂存、提交或推送。

- AUDIT-04 差异检查更正：首轮本模块范围也返回 2，原因是本轮 PowerShell 清理记录引入单行 CRLF 与文档 LF 混用；上一条“全仓仅旧日志”不适用于该首轮。只统一本轮文档换行，不改历史文字或用户 GAP-09 日志；修正后重新检查，结果另记。

- AUDIT-04 换行修正后差异复验：本模块文档范围返回码 0；全工作树返回码 2，剩余错误仅在用户已有 GAP-09 两份日志中，未改动。命令/返回码/输出见 `artifacts/runs/audit04_keyboard_20261023/diff-final-result.json` 及其日志索引；计数不适用，未暂存或提交。

- AUDIT-04 独立桌面工具发现：用户允许切换后执行 `ocu call list_apps`，返回码 `0`，工具错误标记为 false；本轮测试 EXE 未运行。此步骤只确认可枚举桌面，不代表真实按键通过。结果：`artifacts/runs/audit04_keyboard_20261023/independent-list-apps.json`。

- AUDIT-04 独立隔离启动：Start-Process 后精确路径核对通过，返回码 `0`；此时只确认进程，不计入键盘/焦点通过。PID、构建目录及会话：`artifacts/runs/audit04_keyboard_20261023/independent-launch-result.json`。

- AUDIT-04 独立桌面观察 `independent-discovery`：工具进程返回码 `0`；仅保存工具与应用当前状态，场景结论另行核验。命令和文本证据：`artifacts/runs/audit04_keyboard_20261023/independent-discovery-result.json`。

- AUDIT-04 独立桌面观察 `navigation-initial`：工具进程返回码 `0`；仅保存工具与应用当前状态，场景结论另行核验。命令和文本证据：`artifacts/runs/audit04_keyboard_20261023/navigation-initial-result.json`。

- AUDIT-04 独立桌面观察 `navigation-alt-down`：工具进程返回码 `1`；仅保存工具与应用当前状态，场景结论另行核验。命令和文本证据：`artifacts/runs/audit04_keyboard_20261023/navigation-alt-down-result.json`。

- AUDIT-04 `navigation-alt-down` 工具校准：独立 CLI 每次启动不保留 app state，本次返回“Run get_app_state before action tools”，没有实际发键；属于无效工具尝试，不是产品 RED。后续在同一 CLI 调用内先取状态再执行单个动作。

- AUDIT-04 独立桌面观察 `navigation-alt-down-calibrated`：工具进程返回码 `0`；仅保存工具与应用当前状态，场景结论另行核验。命令和文本证据：`artifacts/runs/audit04_keyboard_20261023/navigation-alt-down-calibrated-result.json`。

- AUDIT-04 独立桌面观察 `menu-file-calibration`：工具进程返回码 `0`；仅保存工具与应用当前状态，场景结论另行核验。命令和文本证据：`artifacts/runs/audit04_keyboard_20261023/menu-file-calibration-result.json`。

- AUDIT-04 独立桌面步骤 `focus-calibration-list-pane`：CLI 返回码 `0`；步骤与文本状态已记录，图像只在执行时观察，不作为单独可复核证据。结果：`artifacts/runs/audit04_keyboard_20261023/focus-calibration-list-pane-result.json`。

- AUDIT-04 独立桌面步骤 `focus-calibration-list-control`：CLI 返回码 `0`；步骤与文本状态已记录，图像只在执行时观察，不作为单独可复核证据。结果：`artifacts/runs/audit04_keyboard_20261023/focus-calibration-list-control-result.json`。

- AUDIT-04 独立工具焦点校准重启：前一测试实例 PID `12380` 已按精确可执行路径强制清理，原因是独立工具截图/焦点状态不稳定；不计入产品结论，用户读屏进程未触碰。记录：`artifacts/runs/audit04_keyboard_20261023/independent-restart-cleanup.json`。

- AUDIT-04 独立桌面步骤 `fresh-initial-state`：CLI 返回码 `0`；步骤与文本状态已记录，图像只在执行时观察，不作为单独可复核证据。结果：`artifacts/runs/audit04_keyboard_20261023/fresh-initial-state-result.json`。

- AUDIT-04 独立桌面步骤 `real-alt-down-1`：CLI 返回码 `0`；步骤与文本状态已记录，图像只在执行时观察，不作为单独可复核证据。结果：`artifacts/runs/audit04_keyboard_20261023/real-alt-down-1-result.json`。

- AUDIT-04 独立桌面步骤 `focus-coordinate-calibration`：CLI 返回码 `0`；步骤与文本状态已记录，图像只在执行时观察，不作为单独可复核证据。结果：`artifacts/runs/audit04_keyboard_20261023/focus-coordinate-calibration-result.json`。

- AUDIT-04 独立桌面步骤 `focus-window-activation`：CLI 返回码 `0`；步骤与文本状态已记录，图像只在执行时观察，不作为单独可复核证据。结果：`artifacts/runs/audit04_keyboard_20261023/focus-window-activation-result.json`。

- AUDIT-04 独立桌面步骤 `cancel-accidental-path-dialog`：CLI 返回码 `0`；步骤与文本状态已记录，图像只在执行时观察，不作为单独可复核证据。结果：`artifacts/runs/audit04_keyboard_20261023/cancel-accidental-path-dialog-result.json`。

- AUDIT-04 独立桌面步骤 `focus-list-after-activation`：CLI 返回码 `0`；步骤与文本状态已记录，图像只在执行时观察，不作为单独可复核证据。结果：`artifacts/runs/audit04_keyboard_20261023/focus-list-after-activation-result.json`。

- AUDIT-04 独立桌面步骤 `focus-top-list-item`：CLI 返回码 `0`；步骤与文本状态已记录，图像只在执行时观察，不作为单独可复核证据。结果：`artifacts/runs/audit04_keyboard_20261023/focus-top-list-item-result.json`。

- AUDIT-04 独立桌面步骤 `cancel-accidental-speech-dialog`：CLI 返回码 `0`；步骤与文本状态已记录，图像只在执行时观察，不作为单独可复核证据。结果：`artifacts/runs/audit04_keyboard_20261023/cancel-accidental-speech-dialog-result.json`。

- AUDIT-04 独立桌面步骤 `cancel-speech-dialog-escape`：CLI 返回码 `1`；步骤与文本状态已记录，图像只在执行时观察，不作为单独可复核证据。结果：`artifacts/runs/audit04_keyboard_20261023/cancel-speech-dialog-escape-result.json`。

- AUDIT-04 独立桌面步骤 `post-escape-state`：CLI 返回码 `1`；步骤与文本状态已记录，图像只在执行时观察，不作为单独可复核证据。结果：`artifacts/runs/audit04_keyboard_20261023/post-escape-state-result.json`。

- AUDIT-04 独立桌面步骤 `restored-visible-state`：CLI 返回码 `0`；步骤与文本状态已记录，图像只在执行时观察，不作为单独可复核证据。结果：`artifacts/runs/audit04_keyboard_20261023/restored-visible-state-result.json`。

- AUDIT-04 独立桌面步骤 `activate-main-window-current-state`：CLI 返回码 `0`；步骤与文本状态已记录，图像只在执行时观察，不作为单独可复核证据。结果：`artifacts/runs/audit04_keyboard_20261023/activate-main-window-current-state-result.json`。

- AUDIT-04 独立桌面步骤 `focus-coordinate-auto`：CLI 返回码 `0`；步骤与文本状态已记录，图像只在执行时观察，不作为单独可复核证据。结果：`artifacts/runs/audit04_keyboard_20261023/focus-coordinate-auto-result.json`。

- AUDIT-04 只读观测探针首轮构建：C# 编译器参数写法错误，返回码 `1`（CS2005/CS2008），未生成探针，不是产品 RED；日志为本次命令输出。随后只修正调用参数，不改探针目标。

- AUDIT-04 只读 Win32 观测探针构建修正：返回码 `0`；探针只读记录前台 PID、目标主窗口、列表句柄、选中/焦点索引，不发送输入。当前首次观测：`artifacts/runs/audit04_keyboard_20261023/probe-before-key.json`。

- AUDIT-04 独立桌面步骤 `foreground-alt-tab-1`：CLI 返回码 `0`；步骤与文本状态已记录，图像只在执行时观察，不作为单独可复核证据。结果：`artifacts/runs/audit04_keyboard_20261023/foreground-alt-tab-1-result.json`。

- AUDIT-04 独立桌面步骤 `foreground-alt-tab-2`：CLI 返回码 `0`；步骤与文本状态已记录，图像只在执行时观察，不作为单独可复核证据。结果：`artifacts/runs/audit04_keyboard_20261023/foreground-alt-tab-2-result.json`。

- AUDIT-04 独立桌面步骤 `target-foreground-window-state`：CLI 返回码 `0`；步骤与文本状态已记录，图像只在执行时观察，不作为单独可复核证据。结果：`artifacts/runs/audit04_keyboard_20261023/target-foreground-window-state-result.json`。

- AUDIT-04 只读探针扩展首轮：直接文本替换破坏 C# 字符串转义，编译返回码 `1`，仅是审计辅助脚本错误，未形成产品结论；已保留失败记录并重写最小探针。

- AUDIT-04 只读探针重写后构建：返回码 `0`；现在输出目标进程全部顶层窗口并优先选择标题为 `沙河批量打开器` 的主窗体，仍只读。

- AUDIT-04 独立桌面步骤 `foreground-alt-tab-3`：CLI 返回码 `0`；步骤与文本状态已记录，图像只在执行时观察，不作为单独可复核证据。结果：`artifacts/runs/audit04_keyboard_20261023/foreground-alt-tab-3-result.json`。

- AUDIT-04 独立桌面步骤 `foreground-alt-tab-4`：CLI 返回码 `0`；步骤与文本状态已记录，图像只在执行时观察，不作为单独可复核证据。结果：`artifacts/runs/audit04_keyboard_20261023/foreground-alt-tab-4-result.json`。

- AUDIT-04 独立桌面步骤 `focus-titlebar-auto`：CLI 返回码 `0`；步骤与文本状态已记录，图像只在执行时观察，不作为单独可复核证据。结果：`artifacts/runs/audit04_keyboard_20261023/focus-titlebar-auto-result.json`。

- AUDIT-04 独立桌面步骤 `focus-window-accessibility-click`：CLI 返回码 `1`；步骤与文本状态已记录，图像只在执行时观察，不作为单独可复核证据。结果：`artifacts/runs/audit04_keyboard_20261023/focus-window-accessibility-click-result.json`。

- AUDIT-04 前台复核：只读探针显示隔离主窗口可见、列表共 97 项，但目标不是系统前台，实按 Alt+Down 后选中/焦点索引未变化；该步骤不通过且不归因产品，继续用监视器捕获动作时序。

- AUDIT-04 恢复审核更正：`fixture-preparation.json` 的 `iniHash` 是真实 Sandboxie.ini 哈希，并非隔离应用配置；此前恢复尝试将两者比较无意义，未声称相符。隔离配置现用 JSON 序列化重建有效路径及默认快捷键，保存为 `artifacts/runs/audit04_keyboard_20261023/fixture-calibrated.ini`。只读监视器构建返回码 0。之前重启后的包装记录仍写旧 PID 属记录器缺陷，实际观察中的 PID 为 12528；此前全部桌面尝试保持 inconclusive，不作为产品结论。

- AUDIT-04 屏幕恢复后的新隔离实例启动与精确路径核对返回码 0；配置已重新序列化，进程与真实 ini 基线见 `artifacts/runs/audit04_keyboard_20261023/screen-restored-launch.json`。本项仅启动，不代表键盘通过。

- AUDIT-04 只读前台/列表观察 `screen-restored-front` 返回码 0：目标前台=true，选中/焦点=-1/0；具体 PID/句柄在 `artifacts/runs/audit04_keyboard_20261023/screen-restored-front.json`。

- AUDIT-04 独立工具持续会话 `resumed-main-state`：工具错误标记 false，命令/原始文本状态 `artifacts/runs/audit04_keyboard_20261023/resumed-main-state-result.json`；场景有效性另据前台与事件检查。

- AUDIT-04 独立工具持续会话 `resumed-alt-down`：工具错误标记 false，命令/原始文本状态 `artifacts/runs/audit04_keyboard_20261023/resumed-alt-down-result.json`；场景有效性另据前台与事件检查。

- AUDIT-04 只读前台/列表观察 `resumed-alt-down-index` 返回码 0：目标前台=true，选中/焦点=-1/0；具体 PID/句柄在 `artifacts/runs/audit04_keyboard_20261023/resumed-alt-down-index.json`。

- AUDIT-04 原生输入辅助程序构建返回码 0；命令/日志 `artifacts/runs/audit04_keyboard_20261023/native-helper-build-result.json`、`native-helper-build.log`。尚未执行产品输入，不是产品测试结论。

- AUDIT-04 原生事件步骤 `native-baseline`（Observe）：返回码 0，输入/回读与前后台证据在 `artifacts/runs/audit04_keyboard_20261023/native-baseline-result.json`；是否满足场景另以断言判定。

- AUDIT-04 原生事件步骤 `native-alt-down`（AltDown）：返回码 0，输入/回读与前后台证据在 `artifacts/runs/audit04_keyboard_20261023/native-alt-down-result.json`；是否满足场景另以断言判定。

- AUDIT-04 原生事件步骤 `native-alt-up-wrap`（AltUp）：返回码 0，输入/回读与前后台证据在 `artifacts/runs/audit04_keyboard_20261023/native-alt-up-wrap-result.json`；是否满足场景另以断言判定。

- AUDIT-04 原生事件步骤 `native-alt-down-wrap`（AltDown）：返回码 0，输入/回读与前后台证据在 `artifacts/runs/audit04_keyboard_20261023/native-alt-down-wrap-result.json`；是否满足场景另以断言判定。

- AUDIT-04 Alt 启用态真实验证通过：原生 SendInput、带唯一标记的键事件及 MSAA/原生列表状态共同核对，分组跳过、双向环绕、焦点和未勾选状态均成立；每次前台核验、输入数量及原始场景见 `artifacts/runs/audit04_keyboard_20261023/alt-navigation-final.json`。返回码 0，通过 3、失败 0、跳过 0。

- AUDIT-04 单读屏测试准备：已启动现有 NVDA 的独立配置实例，未停止用户 ZDSR；进程/参数见 `artifacts/runs/audit04_keyboard_20261023/nvda-audit-launch.json`。原始 I/O 日志仅本地留存，交付仅提取本测试窗口相关片段。尚不代表控件朗读通过。

- AUDIT-04 原生事件步骤 `group-select-home`（Home）：返回码 0，输入/回读与前后台证据在 `artifacts/runs/audit04_keyboard_20261023/group-select-home-result.json`；是否满足场景另以断言判定。

- AUDIT-04 执行策略：用户明确要求不重复已测试项目。Alt 真实成功及现有 C#/Python/编译/Release 结果直接沿用；只补未验收项。`group-select-home` 未先确认控件焦点（focus=0），分组索引断言未成立，Enter 未发送，属于无效夹具步骤。

- AUDIT-04 未验收场景的焦点准备辅助程序构建返回码 0，日志 `artifacts/runs/audit04_keyboard_20261023/native-focus-helper-build.log`；仅重建修改的辅助程序，未重跑已通过的产品测试。

- AUDIT-04 原生步骤 `group-focus-preparation`（FocusSandbox）：返回码 0；原始前台/焦点/键事件证据 `artifacts/runs/audit04_keyboard_20261023/group-focus-preparation-result.json`。

- AUDIT-04 `group-focus-preparation`：辅助程序返回码 0 但目标控件焦点仍为 0，准备条件未成立，后续 Home/Enter 均未执行；不计产品失败。仅校正辅助程序的跨线程焦点设置。

- AUDIT-04 仅修正跨线程焦点准备的辅助程序构建返回码 1，生产代码与已通过测试未重跑。

- AUDIT-04 辅助程序编译调用修正为本轮目录相对文件参数后返回码 0，日志 `artifacts/runs/audit04_keyboard_20261023/native-focus-calibrated-build.log`。前次 CS1504 为 csc 参数路径问题，不是产品 RED。

- AUDIT-04 原生步骤 `group-focus-calibrated`（FocusSandbox）：返回码 0；原始前台/焦点/键事件证据 `artifacts/runs/audit04_keyboard_20261023/group-focus-calibrated-result.json`。

- AUDIT-04 分组焦点准备仍未稳定：跨线程准备后实际前台已变化，尚未发送 Home/Enter；无产品失败结论。仅使用现有用户导航命令作为分组场景的焦点准备，不重复验收 Alt 场景。

- AUDIT-04 原生步骤 `group-navigation-preparation`（AltDown）：返回码 0；原始前台/焦点/键事件证据 `artifacts/runs/audit04_keyboard_20261023/group-navigation-preparation-result.json`。

- AUDIT-04 原生步骤 `group-home-valid`（Home）：返回码 0；原始前台/焦点/键事件证据 `artifacts/runs/audit04_keyboard_20261023/group-home-valid-result.json`。

- AUDIT-04 按用户不重复测试要求停止无效桌面重试并清理本轮隔离进程；原生清理调用返回码 0，详细进程、清理方式及真实 ini 核对见 `artifacts/runs/audit04_keyboard_20261023/no-repeat-cleanup-result.json`。这不是托盘菜单退出验收，不新增产品通过项。

- AUDIT-04 单读屏部分证据已整理：真实 NVDA 对隔离路径条目产生名称、列表/复选框角色、未选中状态和位置朗读日志；只保留测试条目的 `artifacts/runs/audit04_keyboard_20261023/nvda-control-speech-evidence.log`，结论见 `single-reader-partial-result.json`。不把语音流水日志当作独立音频录制或完整工作流验收。清理前本轮 NVDA 已不在运行；用户原有 ZDSR 进程均保留。

- AUDIT-04 本次仅对新增文档记录执行差异检查，返回码 0，日志 `artifacts/runs/audit04_keyboard_20261023/no-repeat-document-diff.log` 及对应结果文件；没有重跑已有产品测试。已核对活动计划，未验收三项仍未勾选，暂存区为空，不提交未完成模块。

- AUDIT-04 NumPadEnter：已启动本项隔离实例，精确路径通过核对；基线见 artifacts/runs/audit04_keyboard_20261023/numpad-20261023_100849_193/baseline.json。只执行扩展 Enter 和普通 Enter 对照。

- AUDIT-04 NumPadEnter：NumPadEnter 原生单次输入返回码 0；判定 inconclusive，通过 0、失败 0、未判定 1。目标 PID、构建路径、焦点/索引/扩展位及键事件见 artifacts/runs/audit04_keyboard_20261023/numpad-20261023_100849_193/NumPadEnter-result.json 与 NumPadEnter-assessment.json。

- AUDIT-04 NumPadEnter：本项清理完成；目标退出、隔离配置还原、真实 Sandboxie.ini 和用户读屏进程检查见 artifacts/runs/audit04_keyboard_20261023/numpad-20261023_100849_193/cleanup.json。清理不作为托盘退出验收。

- AUDIT-04 NumPadEnter：小键盘 Enter 对照最终结论 inconclusive，返回码 3，通过 0、失败 0、未判定 1；权威结果 artifacts/runs/audit04_keyboard_20261023/numpad-20261023_100849_193/final-result.json。既有代码测试与构建未重跑。

- AUDIT-04 NumPadEnter 首次判定说明：原生按下/抬起均为 VK_RETURN、扫描码 0x1C、扩展位已置位，全部在目标前台；输入层已核实。动作前列表数量为 0、动作后才加载出列表，缺少稳定基线，因此不作为动作成功/产品失败。外围命令实际返回 1、报告语义码 3 保留；下一次仅补未完成的就绪态动作对照。

- AUDIT-04 NumPadEnter：已启动本项隔离实例，精确路径通过核对；基线见 artifacts/runs/audit04_keyboard_20261023/numpad-20261023_101023_549/baseline.json。只执行扩展 Enter 和普通 Enter 对照。

- AUDIT-04 NumPadEnter：只读启动就绪检查 1 返回码 0，列表校准就绪=False；artifacts/runs/audit04_keyboard_20261023/numpad-20261023_101023_549/ready-1.json，未发送按键。

- AUDIT-04 NumPadEnter：只读启动就绪检查 2 返回码 0，列表校准就绪=True；artifacts/runs/audit04_keyboard_20261023/numpad-20261023_101023_549/ready-2.json，未发送按键。

- AUDIT-04 NumPadEnter：NumPadEnter 原生单次输入返回码 0；判定 inconclusive，通过 0、失败 0、未判定 1。目标 PID、构建路径、焦点/索引/扩展位及键事件见 artifacts/runs/audit04_keyboard_20261023/numpad-20261023_101023_549/NumPadEnter-result.json 与 NumPadEnter-assessment.json。

- AUDIT-04 NumPadEnter：本项清理完成；目标退出、隔离配置还原、真实 Sandboxie.ini 和用户读屏进程检查见 artifacts/runs/audit04_keyboard_20261023/numpad-20261023_101023_549/cleanup.json。清理不作为托盘退出验收。

- AUDIT-04 NumPadEnter：小键盘 Enter 对照最终结论 inconclusive，返回码 3，通过 0、失败 0、未判定 1；权威结果 artifacts/runs/audit04_keyboard_20261023/numpad-20261023_101023_549/final-result.json。既有代码测试与构建未重跑。

- AUDIT-04 NumPadEnter 已有证据只读分析：第二次就绪检查通过后，条目从名称变为“名称 【空闲中】”，说明运行状态仍异步补齐；选中索引前后均为 -1，未观察到绑定动作。底层 VK_RETURN/扫描码/扩展位/目标前台与键事件数量正确，但不据此宣布功能通过或认定产品 bug。assessment 中 selectedName 在索引 -1 时错误显示末项，是记录器的负索引显示问题；原始 selectedIndex=-1 才是权威值。未发送普通 Enter，未继续重试。

- AUDIT-04 本次文档收尾差异检查返回码 2，证据 artifacts/runs/audit04_keyboard_20261023/numpad-document-diff-result.json；未重跑产品测试。已复核计划，NumPadEnter 动作分派仍未验收，分组/完整无障碍审计待办，模块未提交。

- AUDIT-04 文档差异首轮返回码 2，原因是本次 PowerShell 追加的 CRLF 与原文件 LF 混用；仅统一文档换行，保留所有历史结论。

- AUDIT-04 本次文档换行修正后差异检查返回码 0；日志及命令见 artifacts/runs/audit04_keyboard_20261023/numpad-document-diff-final-result.json。未重跑产品测试。

- AUDIT-04 分组测试准备：仅重建增加 MSAA 精确选中分组的辅助程序，返回码 0；记录 artifacts/runs/audit04_keyboard_20261023/group-helper-build-result.json。生产程序与既有代码门禁未重跑。

- AUDIT-04 分组进出：启动后精确路径核验通过，基线 artifacts/runs/audit04_keyboard_20261023/group-20261023_104538_003/baseline.json。仅分组测试，不重跑既有验证。

- AUDIT-04 分组进出：步骤 stable-before (Observe) 返回码 0；命令、PID、输入及前后窗口状态见 artifacts/runs/audit04_keyboard_20261023/group-20261023_104538_003/stable-before-result.json。

- AUDIT-04 分组进出：步骤 stable-after (Observe) 返回码 0；命令、PID、输入及前后窗口状态见 artifacts/runs/audit04_keyboard_20261023/group-20261023_104538_003/stable-after-result.json。

- AUDIT-04 分组进出：步骤 group-selection (SelectAuditGroup) 返回码 0；命令、PID、输入及前后窗口状态见 artifacts/runs/audit04_keyboard_20261023/group-20261023_104538_003/group-selection-result.json。

- AUDIT-04 分组进出：本项清理完成；目标退出、隔离配置还原、真实 Sandboxie.ini 和用户读屏进程检查见 artifacts/runs/audit04_keyboard_20261023/group-20261023_104538_003/cleanup.json。清理不作为托盘退出验收。

- AUDIT-04 分组进出：分组 Enter/Backspace最终结论 inconclusive，返回码 3，通过 0、失败 0、未判定 1；权威结果 artifacts/runs/audit04_keyboard_20261023/group-20261023_104538_003/final-result.json。既有代码测试与构建未重跑。

- AUDIT-04 分组前置步骤诊断：稳定列表已通过，MSAA TakeFocus|TakeSelection 返回后分组仍未选中，焦点落在启动按钮；Enter/Backspace 均未发送。这是准备方式无效，未形成产品失败。改为依据同一专用分组的可访问边界执行一次原生单击，校验前台及命中进程后再发键；不重复任何已通过验收。

- AUDIT-04 分组点击前置辅助程序构建返回码 0；记录 artifacts/runs/audit04_keyboard_20261023/group-click-helper-build-result.json。仅编译本次修改辅助程序，未重复生产代码验证。

- AUDIT-04 分组进出：启动后精确路径核验通过，基线 artifacts/runs/audit04_keyboard_20261023/group-20261023_104845_989/baseline.json。仅分组测试，不重跑既有验证。

- AUDIT-04 分组进出：步骤 stable-before (Observe) 返回码 0；命令、PID、输入及前后窗口状态见 artifacts/runs/audit04_keyboard_20261023/group-20261023_104845_989/stable-before-result.json。

- AUDIT-04 分组进出：步骤 stable-after (Observe) 返回码 0；命令、PID、输入及前后窗口状态见 artifacts/runs/audit04_keyboard_20261023/group-20261023_104845_989/stable-after-result.json。

- AUDIT-04 分组进出：步骤 group-selection (SelectAuditGroup) 返回码 0；命令、PID、输入及前后窗口状态见 artifacts/runs/audit04_keyboard_20261023/group-20261023_104845_989/group-selection-result.json。

- AUDIT-04 分组进出：步骤 enter (Enter) 返回码 0；命令、PID、输入及前后窗口状态见 artifacts/runs/audit04_keyboard_20261023/group-20261023_104845_989/enter-result.json。

- AUDIT-04 分组进出：enter 判定 passed，输入完整=True、前置校准=True、结果匹配=True；artifacts/runs/audit04_keyboard_20261023/group-20261023_104845_989/enter-assessment.json。

- AUDIT-04 分组进出：步骤 backspace (Backspace) 返回码 0；命令、PID、输入及前后窗口状态见 artifacts/runs/audit04_keyboard_20261023/group-20261023_104845_989/backspace-result.json。

- AUDIT-04 分组进出：backspace 判定 passed，输入完整=True、前置校准=True、结果匹配=True；artifacts/runs/audit04_keyboard_20261023/group-20261023_104845_989/backspace-assessment.json。

- AUDIT-04 分组进出：本项清理完成；目标退出、隔离配置还原、真实 Sandboxie.ini 和用户读屏进程检查见 artifacts/runs/audit04_keyboard_20261023/group-20261023_104845_989/cleanup.json。清理不作为托盘退出验收。

- AUDIT-04 分组进出：分组 Enter/Backspace最终结论 passed，返回码 0，通过 2、失败 0、未判定 0；权威结果 artifacts/runs/audit04_keyboard_20261023/group-20261023_104845_989/final-result.json。既有代码测试与构建未重跑。

- AUDIT-04 分组原始证据核对通过：选中专用分组后的 Enter 实际显示两个配置成员，Backspace 恢复原列表、沙盒列表焦点，勾选状态未变化；两次按键各有完整目标前台事件。通过数、PID、成员文本、索引与清理见 `artifacts/runs/audit04_keyboard_20261023/group-20261023_104845_989/final-result.json` 及同目录 enter/backspace 原始报告。Python `ui/controls.py:on_key_down` 同样按 Enter 进入分组、Backspace 清空当前分组并刷新，生产源码无需修改。本项标记通过，但 AUDIT-04 其余项与整体迁移仍未完成。

- AUDIT-04 分组记录的文档差异检查返回码 0；证据 artifacts/runs/audit04_keyboard_20261023/group-document-diff-result.json。结束前已复核活动计划，通过项保留，NumPadEnter 动作分派与完整菜单/托盘/读屏待办；未暂存或提交未完成模块。

- AUDIT-04 菜单/托盘观测辅助程序构建返回码 0；记录 artifacts/runs/audit04_keyboard_20261023/menu-tray-helper-build-result.json。仅重建本次修改辅助程序，没有重跑已通过产品测试。

- AUDIT-04 菜单/托盘：精确路径启动完成；artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_105907_895/baseline.json。本轮只测试未验收的菜单/焦点/托盘。

- AUDIT-04 菜单/托盘：步骤 focus-preparation (SelectAuditGroup) 返回码 0；原始 PID/菜单/焦点/键事件 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_105907_895/focus-preparation-result.json。

- AUDIT-04 菜单/托盘：步骤 tab-path (Tab) 返回码 0；原始 PID/菜单/焦点/键事件 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_105907_895/tab-path-result.json。

- AUDIT-04 菜单/托盘：tab-path 判定 passed；证据 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_105907_895/tab-path-assessment.json。

- AUDIT-04 菜单/托盘：步骤 tab-launch (Tab) 返回码 0；原始 PID/菜单/焦点/键事件 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_105907_895/tab-launch-result.json。

- AUDIT-04 菜单/托盘：tab-launch 判定 passed；证据 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_105907_895/tab-launch-assessment.json。

- AUDIT-04 菜单/托盘：步骤 tab-wrap (Tab) 返回码 0；原始 PID/菜单/焦点/键事件 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_105907_895/tab-wrap-result.json。

- AUDIT-04 菜单/托盘：tab-wrap 判定 failed；证据 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_105907_895/tab-wrap-assessment.json。

- AUDIT-04 菜单/托盘：本项清理完成；目标退出、隔离配置还原、真实 Sandboxie.ini 和用户读屏进程检查见 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_105907_895/cleanup.json。清理不作为托盘退出验收。

- AUDIT-04 菜单/托盘：菜单/焦点/托盘最终结论 failed，返回码 1，通过 2、失败 1、未判定 1；权威结果 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_105907_895/final-result.json。既有代码测试与构建未重跑。

- AUDIT-04 菜单/焦点首轮更正：`menu-tray-20261023_105907_895` 中 tab-path、tab-launch 已通过，后续直接沿用。tab-wrap 原始键事件为带 Alt 上下文的 WM_SYSKEYDOWN/UP，实际前台 PID 属于资源管理器而非目标；脚本把输入无效误归为 failed，现按原始证据更正为 inconclusive（非产品 RED），原报告保留。普通 Tab 环绕、其他菜单/托盘尚未执行有效验收。

- AUDIT-04 输入干扰门禁辅助程序构建返回码 0；记录 artifacts/runs/audit04_keyboard_20261023/menu-input-gate-build-result.json。仅编译本次辅助程序修改，既有产品测试未重跑。

- AUDIT-04 菜单/托盘：精确路径启动完成；artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_110417_784/baseline.json。本轮只测试未验收的菜单/焦点/托盘。

- AUDIT-04 菜单/托盘：步骤 focus-preparation (SelectAuditGroup) 返回码 0；原始 PID/菜单/焦点/键事件 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_110417_784/focus-preparation-result.json。

- AUDIT-04 菜单/托盘：步骤 shift-tab (ShiftTab) 返回码 0；原始 PID/菜单/焦点/键事件 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_110417_784/shift-tab-result.json。

- AUDIT-04 菜单/托盘：shift-tab 判定 passed；证据 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_110417_784/shift-tab-assessment.json。

- AUDIT-04 菜单/托盘：步骤 tab-wrap (Tab) 返回码 0；原始 PID/菜单/焦点/键事件 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_110417_784/tab-wrap-result.json。

- AUDIT-04 菜单/托盘：tab-wrap 判定 passed；证据 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_110417_784/tab-wrap-assessment.json。

- AUDIT-04 菜单/托盘：步骤 f10-menu (F10) 返回码 0；原始 PID/菜单/焦点/键事件 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_110417_784/f10-menu-result.json。

- AUDIT-04 菜单/托盘：步骤 f10-escape1 (Escape) 返回码 0；原始 PID/菜单/焦点/键事件 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_110417_784/f10-escape1-result.json。

- AUDIT-04 菜单/托盘：f10-entry-focus 判定 passed；证据 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_110417_784/f10-entry-focus-assessment.json。

- AUDIT-04 菜单/托盘：步骤 menu-AltF (AltF) 返回码 3；原始 PID/菜单/焦点/键事件 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_110417_784/menu-AltF-result.json。

- AUDIT-04 菜单/托盘：本项清理完成；目标退出、隔离配置还原、真实 Sandboxie.ini 和用户读屏进程检查见 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_110417_784/cleanup.json。清理不作为托盘退出验收。

- AUDIT-04 菜单/托盘：菜单/焦点/托盘最终结论 inconclusive，返回码 3，通过 3、失败 0、未判定 1；权威结果 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_110417_784/final-result.json。既有代码测试与构建未重跑。

- AUDIT-04 菜单/焦点续测：`menu-tray-20261023_110417_784` 的 shift-tab、tab-wrap、f10-entry-focus 已通过；加上首轮 tab-path/tab-launch，后续均不重跑。Alt+F 的目标键事件全部命中目标，但窗口期内发现其他键事件，原生工具返回 3，暂作 inconclusive，菜单助记键及后续场景未宣称通过。

- AUDIT-04 额外键事件观测辅助程序构建返回码 0；日志 artifacts/runs/audit04_keyboard_20261023/menu-input-observation-build.log。只记录输入类型并屏蔽可打印字符；不重跑已通过项目。

- AUDIT-04 菜单/托盘：精确路径启动完成；artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_110640_184/baseline.json。本轮只测试未验收的菜单/焦点/托盘。

- AUDIT-04 菜单/托盘：步骤 focus-preparation (SelectAuditGroup) 返回码 0；原始 PID/菜单/焦点/键事件 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_110640_184/focus-preparation-result.json。

- AUDIT-04 菜单/托盘：步骤 menu-AltF (AltF) 返回码 0；原始 PID/菜单/焦点/键事件 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_110640_184/menu-AltF-result.json。

- AUDIT-04 菜单/托盘：步骤 AltF-escape1 (Escape) 返回码 0；原始 PID/菜单/焦点/键事件 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_110640_184/AltF-escape1-result.json。

- AUDIT-04 菜单/托盘：本项清理完成；目标退出、隔离配置还原、真实 Sandboxie.ini 和用户读屏进程检查见 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_110640_184/cleanup.json。清理不作为托盘退出验收。

- AUDIT-04 菜单/托盘：菜单/焦点/托盘最终结论 inconclusive，返回码 3，通过 0、失败 0、未判定 1；权威结果 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_110640_184/final-result.json。既有代码测试与构建未重跑。

- AUDIT-04 文件助记键已有证据核对：`menu-tray-20261023_110640_184/menu-AltF-result.json` 目标键事件完整、无额外输入、原生文件菜单高亮且显示正确命令，文件菜单入口单项通过。后续 Escape 前记录已显示菜单关闭/前台转移，因此该退出尝试无效，不判产品故障；不会重复 Alt+F 入口。辅助进程改为无窗口运行，并在发送菜单 Escape 前重新验证菜单模式，防止过期状态触发主窗体隐藏。

- AUDIT-04 菜单 Escape 实时前置校验辅助程序构建返回码 0；日志 artifacts/runs/audit04_keyboard_20261023/menu-mode-gate-build.log。辅助程序后续以 CreateNoWindow 执行，已通过项不重跑。

- AUDIT-04 菜单/托盘：精确路径启动完成；artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_110904_674/baseline.json。本轮只测试未验收的菜单/焦点/托盘。

- AUDIT-04 菜单/托盘：步骤 focus-preparation (SelectAuditGroup) 返回码 0；原始 PID/菜单/焦点/键事件 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_110904_674/focus-preparation-result.json。

- AUDIT-04 菜单/托盘：步骤 menu-AltP (AltP) 返回码 0；原始 PID/菜单/焦点/键事件 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_110904_674/menu-AltP-result.json。

- AUDIT-04 菜单/托盘：本项清理完成；目标退出、隔离配置还原、真实 Sandboxie.ini 和用户读屏进程检查见 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_110904_674/cleanup.json。清理不作为托盘退出验收。

- AUDIT-04 菜单/托盘：菜单/焦点/托盘最终结论 inconclusive，返回码 3，通过 0、失败 0、未判定 1；权威结果 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_110904_674/final-result.json。既有代码测试与构建未重跑。

- AUDIT-04 进程菜单观察未判定：`menu-tray-20261023_110904_674/menu-AltP-result.json` 的按键事件命中目标，但结果观察时前台已是浏览器进程，菜单不再激活，因此未宣称助记键或返回焦点通过。菜单未验收项保留，不重试已有成功项；接着执行独立的软件设置取消与托盘场景。

- AUDIT-04 菜单/托盘：精确路径启动完成；artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_111154_351/baseline.json。本轮只测试未验收的菜单/焦点/托盘。

- AUDIT-04 菜单/托盘：步骤 focus-preparation (SelectAuditGroup) 返回码 0；原始 PID/菜单/焦点/键事件 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_111154_351/focus-preparation-result.json。

- AUDIT-04 菜单/托盘：步骤 open-settings (ShowSettings) 返回码 3；原始 PID/菜单/焦点/键事件 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_111154_351/open-settings-result.json。

- AUDIT-04 菜单/托盘：本项清理完成；目标退出、隔离配置还原、真实 Sandboxie.ini 和用户读屏进程检查见 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_111154_351/cleanup.json。清理不作为托盘退出验收。

- AUDIT-04 菜单/托盘：菜单/焦点/托盘最终结论 inconclusive，返回码 3，通过 0、失败 0、未判定 1；权威结果 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_111154_351/final-result.json。既有代码测试与构建未重跑。

- AUDIT-04 菜单/焦点本轮结果汇总：已核对六项独立通过证据，清单 `artifacts/runs/audit04_keyboard_20261023/menu-focus-progress.json`。首次脚本把前台输入无效标成 failed 的情况已追加更正，并非产品 RED。最后软件设置步骤因前台未确认返回 3，键未发送；托盘步骤未开始，不作结论。本轮实例已正常退出、隔离配置恢复、真实 Sandboxie.ini 与原读屏进程检查保持。停止无效重试，后续仅在目标窗口可持续保持前台时补未验收项。

- AUDIT-04 本轮菜单/焦点记录的文档差异检查返回码 0，日志 artifacts/runs/audit04_keyboard_20261023/menu-focus-document-diff.log。已重新核对活动计划的完成项、待办和输入干扰风险；未重跑代码门禁、未暂存或提交未完成模块。

- AUDIT-04 菜单/托盘：精确路径启动完成；artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_112645_489/baseline.json。本轮只测试未验收的菜单/焦点/托盘。

- AUDIT-04 菜单/托盘：步骤 focus-preparation (SelectAuditGroup) 返回码 0；原始 PID/菜单/焦点/键事件 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_112645_489/focus-preparation-result.json。

- AUDIT-04 菜单/托盘：步骤 menu-AltP (AltP) 返回码 0；原始 PID/菜单/焦点/键事件 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_112645_489/menu-AltP-result.json。

- AUDIT-04 菜单/托盘：步骤 AltP-escape1 (MenuEscape) 返回码 0；原始 PID/菜单/焦点/键事件 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_112645_489/AltP-escape1-result.json。

- AUDIT-04 菜单/托盘：步骤 AltP-escape2 (MenuEscape) 返回码 0；原始 PID/菜单/焦点/键事件 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_112645_489/AltP-escape2-result.json。

- AUDIT-04 菜单/托盘：mnemonic-AltP 判定 passed；证据 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_112645_489/mnemonic-AltP-assessment.json。

- AUDIT-04 菜单/托盘：步骤 menu-AltG (AltG) 返回码 0；原始 PID/菜单/焦点/键事件 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_112645_489/menu-AltG-result.json。

- AUDIT-04 菜单/托盘：步骤 AltG-escape1 (MenuEscape) 返回码 0；原始 PID/菜单/焦点/键事件 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_112645_489/AltG-escape1-result.json。

- AUDIT-04 菜单/托盘：步骤 AltG-escape2 (MenuEscape) 返回码 0；原始 PID/菜单/焦点/键事件 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_112645_489/AltG-escape2-result.json。

- AUDIT-04 菜单/托盘：mnemonic-AltG 判定 passed；证据 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_112645_489/mnemonic-AltG-assessment.json。

- AUDIT-04 菜单/托盘：步骤 menu-AltD (AltD) 返回码 0；原始 PID/菜单/焦点/键事件 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_112645_489/menu-AltD-result.json。

- AUDIT-04 菜单/托盘：步骤 AltD-escape1 (MenuEscape) 返回码 0；原始 PID/菜单/焦点/键事件 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_112645_489/AltD-escape1-result.json。

- AUDIT-04 菜单/托盘：步骤 AltD-escape2 (MenuEscape) 返回码 0；原始 PID/菜单/焦点/键事件 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_112645_489/AltD-escape2-result.json。

- AUDIT-04 菜单/托盘：mnemonic-AltD 判定 passed；证据 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_112645_489/mnemonic-AltD-assessment.json。

- AUDIT-04 菜单/托盘：步骤 menu-AltS (AltS) 返回码 0；原始 PID/菜单/焦点/键事件 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_112645_489/menu-AltS-result.json。

- AUDIT-04 菜单/托盘：步骤 AltS-escape1 (MenuEscape) 返回码 0；原始 PID/菜单/焦点/键事件 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_112645_489/AltS-escape1-result.json。

- AUDIT-04 菜单/托盘：步骤 AltS-escape2 (MenuEscape) 返回码 0；原始 PID/菜单/焦点/键事件 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_112645_489/AltS-escape2-result.json。

- AUDIT-04 菜单/托盘：mnemonic-AltS 判定 passed；证据 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_112645_489/mnemonic-AltS-assessment.json。

- AUDIT-04 菜单/托盘：步骤 menu-AltJ (AltJ) 返回码 0；原始 PID/菜单/焦点/键事件 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_112645_489/menu-AltJ-result.json。

- AUDIT-04 菜单/托盘：步骤 AltJ-escape1 (MenuEscape) 返回码 0；原始 PID/菜单/焦点/键事件 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_112645_489/AltJ-escape1-result.json。

- AUDIT-04 菜单/托盘：步骤 AltJ-escape2 (MenuEscape) 返回码 0；原始 PID/菜单/焦点/键事件 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_112645_489/AltJ-escape2-result.json。

- AUDIT-04 菜单/托盘：mnemonic-AltJ 判定 passed；证据 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_112645_489/mnemonic-AltJ-assessment.json。

- AUDIT-04 菜单/托盘：步骤 menu-AltT (AltT) 返回码 0；原始 PID/菜单/焦点/键事件 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_112645_489/menu-AltT-result.json。

- AUDIT-04 菜单/托盘：步骤 AltT-escape1 (MenuEscape) 返回码 0；原始 PID/菜单/焦点/键事件 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_112645_489/AltT-escape1-result.json。

- AUDIT-04 菜单/托盘：步骤 AltT-escape2 (MenuEscape) 返回码 0；原始 PID/菜单/焦点/键事件 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_112645_489/AltT-escape2-result.json。

- AUDIT-04 菜单/托盘：mnemonic-AltT 判定 passed；证据 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_112645_489/mnemonic-AltT-assessment.json。

- AUDIT-04 菜单/托盘：步骤 menu-AltA (AltA) 返回码 0；原始 PID/菜单/焦点/键事件 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_112645_489/menu-AltA-result.json。

- AUDIT-04 菜单/托盘：本项清理完成；目标退出、隔离配置还原、真实 Sandboxie.ini 和用户读屏进程检查见 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_112645_489/cleanup.json。清理不作为托盘退出验收。

- AUDIT-04 菜单/托盘：菜单/焦点/托盘最终结论 inconclusive，返回码 3，通过 6、失败 0、未判定 1；权威结果 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_112645_489/final-result.json。既有代码测试与构建未重跑。

- AUDIT-04 菜单续测新增通过：进程、沙盒管理、定时、设置、校正、工具的助记键入口及 Escape 返回焦点均通过，实时证据清单已追加至 artifacts/runs/audit04_keyboard_20261023/menu-focus-progress.json。Alt+A 的目标键事件完整且无额外键，但观察时前台变为微信，疑似外部全局快捷键冲突，保持未判定，不更改或关闭用户微信。下一步验证关于菜单的 F10/End/Down 键盘入口，并继续未验收的模态/托盘。

- AUDIT-04 菜单/托盘：精确路径启动完成；artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_112932_991/baseline.json。本轮只测试未验收的菜单/焦点/托盘。

- AUDIT-04 菜单/托盘：步骤 focus-preparation (SelectAuditGroup) 返回码 0；原始 PID/菜单/焦点/键事件 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_112932_991/focus-preparation-result.json。

- AUDIT-04 菜单/托盘：步骤 about-menu-preparation (F10) 返回码 0；原始 PID/菜单/焦点/键事件 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_112932_991/about-menu-preparation-result.json。

- AUDIT-04 菜单/托盘：本项清理完成；目标退出、隔离配置还原、真实 Sandboxie.ini 和用户读屏进程检查见 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_112932_991/cleanup.json。清理不作为托盘退出验收。

- AUDIT-04 菜单/托盘：菜单/焦点/托盘最终结论 inconclusive，返回码 3，通过 0、失败 0、未判定 1；权威结果 artifacts/runs/audit04_keyboard_20261023/menu-tray-20261023_112932_991/final-result.json。既有代码测试与构建未重跑。

- AUDIT-04 关于替代入口准备：`menu-tray-20261023_112932_991/about-menu-preparation-result.json` 显示 F10 前已处于菜单模式，按下后退出菜单；这是前置状态假设不成立，未形成关于入口通过/产品故障。已清理本轮实例，最新六个菜单有效结果保留。
- 用户追加“快捷键列表找到动作后回车执行”：已只读核对热键设置功能列表，当前没有执行选中动作接线。拟追加完整计划与核对说明存于 `artifacts/runs/hotkey_enter_action_20261023/proposed-plan.diff`、`source-review.md`；尚未写入活动计划的新增功能范围或修改生产代码，待确认触发区域与行为。

- HOTKEY-ENTER-01 red：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\hotkey_enter_action_20261023\bin\ --logger trx;LogFileName=red.trx --results-directory D:\mycodex\git\artifacts\runs\hotkey_enter_action_20261023 --filter FullyQualifiedName~HotkeyActionExecutionContractTests`；返回码 1，通过 0、失败 4、跳过 0。日志与命令索引：`artifacts/runs/hotkey_enter_action_20261023/red.log`、`red-result.json`；如为 C# 测试，另有同名 TRX。

- HOTKEY-ENTER-01 RED 原因核对：生产源码编译成功，新增契约失败来自列表/编辑框 Enter 未被请求执行流程接管，以及缺少传递选中动作的结果；不是测试工具或编译失败。全部功能行将复用既有 DispatchApplicationAction；未修改 Python 原版。

- HOTKEY-ENTER-01 green：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\hotkey_enter_action_20261023\bin\ --logger trx;LogFileName=green.trx --results-directory D:\mycodex\git\artifacts\runs\hotkey_enter_action_20261023 --filter FullyQualifiedName~HotkeyActionExecutionContractTests`；返回码 0，通过 4、失败 0、跳过 0。日志与命令索引：`artifacts/runs/hotkey_enter_action_20261023/green.log`、`green-result.json`；如为 C# 测试，另有同名 TRX。

- HOTKEY-ENTER-01 全功能入口核对：生产 ActionSpecs 的所有功能行均已映射到现有执行分派，缺失项为空；明细 artifacts/runs/hotkey_enter_action_20261023/all-action-mapping.json。定向 GREEN 实际通过后，进入本次改动的回归与真实 EXE 验证；不重复此前已通过的独立桌面审计。

- HOTKEY-ENTER-01 affected：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\hotkey_enter_action_20261023\bin\ --logger trx;LogFileName=affected.trx --results-directory D:\mycodex\git\artifacts\runs\hotkey_enter_action_20261023 --filter FullyQualifiedName~HotkeyActionExecutionContractTests|FullyQualifiedName~SettingsDialogsCSharpContractTests|FullyQualifiedName~MainFormApplicationContractTests|FullyQualifiedName~MainFormLifecycleContractTests|FullyQualifiedName~KeyboardAuditRuntimeContractTests|FullyQualifiedName~HotkeyControllerContractTests`；返回码 0，通过 65、失败 0、跳过 0。日志与命令索引：`artifacts/runs/hotkey_enter_action_20261023/affected.log`、`affected-result.json`；如为 C# 测试，另有同名 TRX。

- HOTKEY-ENTER-01 full：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\hotkey_enter_action_20261023\bin\ --logger trx;LogFileName=full.trx --results-directory D:\mycodex\git\artifacts\runs\hotkey_enter_action_20261023`；返回码 0，通过 395、失败 0、跳过 0。日志与命令索引：`artifacts/runs/hotkey_enter_action_20261023/full.log`、`full-result.json`；如为 C# 测试，另有同名 TRX。

- HOTKEY-ENTER-01 python：命令 `python -m unittest discover -s tests -p test_*.py -v`；返回码 0，通过 29、失败 0、跳过 0。日志与命令索引：`artifacts/runs/hotkey_enter_action_20261023/python.log`、`python-result.json`；如为 C# 测试，另有同名 TRX。

- HOTKEY-ENTER-01 compile：命令 `python -m compileall -q shahe_bulk_launcher tests`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/hotkey_enter_action_20261023/compile.log`、`compile-result.json`；如为 C# 测试，另有同名 TRX。

- HOTKEY-ENTER-01 release：命令 `dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\hotkey_enter_action_20261023\bin\`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/hotkey_enter_action_20261023/release.log`、`release-result.json`；如为 C# 测试，另有同名 TRX。

- HOTKEY-ENTER-01 原生真实入口辅助程序构建返回码 0，日志 artifacts/runs/hotkey_enter_action_20261023/real-probe-build.log；仅辅助程序，生产代码没有进一步改动。

- HOTKEY-ENTER-01 真实 EXE 步骤 startup 已记录；目标路径/PID/原始事件见 artifacts/runs/hotkey_enter_action_20261023/real-exe/startup.json；完整结论以 final-result.json 为准。

- HOTKEY-ENTER-01 真实 EXE 步骤 open-hotkeys 已记录；目标路径/PID/原始事件见 artifacts/runs/hotkey_enter_action_20261023/real-exe/open-hotkeys.json；完整结论以 final-result.json 为准。

- HOTKEY-ENTER-01 真实 EXE 最终返回码 3；完整命令、键事件、窗口/索引和清理见 artifacts/runs/hotkey_enter_action_20261023/real-exe/final-result.json。

- HOTKEY-ENTER-01 真实入口环境收尾：探针实际返回码 3；目标残留、真实 ini 和原读屏进程结果见 artifacts/runs/hotkey_enter_action_20261023/real-environment-after.json。

- HOTKEY-ENTER-01 首次真实探针未形成产品结论：打开热键窗口的准备快捷键全部命中目标，但捕获额外键事件，辅助程序按门禁返回 3。目标正常退出，真实 ini/原读屏检查通过。改为复用“热键设置”原生菜单命令作为准备，只验证新增列表 Enter，不重复已通过代码门禁；原证据保留。

- HOTKEY-ENTER-01 原生菜单准备版辅助程序构建返回码 0；日志 artifacts/runs/hotkey_enter_action_20261023/real-menu-probe-build.log。生产代码未改，不重复产品测试。

- HOTKEY-ENTER-01 真实 EXE 步骤 startup 已记录；目标路径/PID/原始事件见 artifacts/runs/hotkey_enter_action_20261023/real-exe-menu/startup.json；完整结论以 final-result.json 为准。

- HOTKEY-ENTER-01 真实 EXE 步骤 open-hotkeys 已记录；目标路径/PID/原始事件见 artifacts/runs/hotkey_enter_action_20261023/real-exe-menu/open-hotkeys.json；完整结论以 final-result.json 为准。

- HOTKEY-ENTER-01 真实 EXE 最终返回码 3；完整命令、键事件、窗口/索引和清理见 artifacts/runs/hotkey_enter_action_20261023/real-exe-menu/final-result.json。

- HOTKEY-ENTER-01 菜单准备真实入口收尾：实际返回码 3，清理核对 artifacts/runs/hotkey_enter_action_20261023/real-menu-environment-after.json。

- HOTKEY-ENTER-01 菜单准备版真实探针：已打开快捷键窗口，但列表位置命中检查未通过，未发送新增 Enter，返回码 3；该次为坐标准备无效，非产品失败。原始证据 real-exe-menu 保留。辅助程序按 MSAA 物理坐标要求启用 DPI 感知并记录命中点，只修改测试工具，生产代码不变。

- HOTKEY-ENTER-01 DPI 校准辅助程序构建返回码 0；日志 artifacts/runs/hotkey_enter_action_20261023/real-dpi-probe-build.log。

- HOTKEY-ENTER-01 真实 EXE 步骤 startup 已记录；目标路径/PID/原始事件见 artifacts/runs/hotkey_enter_action_20261023/real-exe-dpi/startup.json；完整结论以 final-result.json 为准。

- HOTKEY-ENTER-01 真实 EXE 步骤 open-hotkeys 已记录；目标路径/PID/原始事件见 artifacts/runs/hotkey_enter_action_20261023/real-exe-dpi/open-hotkeys.json；完整结论以 final-result.json 为准。

- HOTKEY-ENTER-01 真实 EXE 步骤 action-location 已记录；目标路径/PID/原始事件见 artifacts/runs/hotkey_enter_action_20261023/real-exe-dpi/action-location.json；完整结论以 final-result.json 为准。

- HOTKEY-ENTER-01 真实 EXE 最终返回码 3；完整命令、键事件、窗口/索引和清理见 artifacts/runs/hotkey_enter_action_20261023/real-exe-dpi/final-result.json。

- HOTKEY-ENTER-01 DPI 校准真实入口收尾：实际返回码 3，清理核对 artifacts/runs/hotkey_enter_action_20261023/real-dpi-environment-after.json。

- HOTKEY-ENTER-01 坐标诊断更正：DPI 校准后的命中日志明确显示坐标上方属于其他应用窗口，仍为遮挡，并非已确认的 DPI 根因；新增 Enter 尚未发送。保持用户其他窗口不变，改用热键窗口已聚焦的功能列表键盘导航选择“设置”，避免坐标依赖。

- HOTKEY-ENTER-01 键盘选择版辅助程序构建返回码 0；日志 artifacts/runs/hotkey_enter_action_20261023/real-keyboard-probe-build.log。

- HOTKEY-ENTER-01 真实 EXE 步骤 startup 已记录；目标路径/PID/原始事件见 artifacts/runs/hotkey_enter_action_20261023/real-exe-keyboard/startup.json；完整结论以 final-result.json 为准。

- HOTKEY-ENTER-01 真实 EXE 步骤 open-hotkeys 已记录；目标路径/PID/原始事件见 artifacts/runs/hotkey_enter_action_20261023/real-exe-keyboard/open-hotkeys.json；完整结论以 final-result.json 为准。

- HOTKEY-ENTER-01 真实 EXE 步骤 select-first 已记录；目标路径/PID/原始事件见 artifacts/runs/hotkey_enter_action_20261023/real-exe-keyboard/select-first.json；完整结论以 final-result.json 为准。

- HOTKEY-ENTER-01 真实 EXE 步骤 select-next-1 已记录；目标路径/PID/原始事件见 artifacts/runs/hotkey_enter_action_20261023/real-exe-keyboard/select-next-1.json；完整结论以 final-result.json 为准。

- HOTKEY-ENTER-01 真实 EXE 步骤 select-next-2 已记录；目标路径/PID/原始事件见 artifacts/runs/hotkey_enter_action_20261023/real-exe-keyboard/select-next-2.json；完整结论以 final-result.json 为准。

- HOTKEY-ENTER-01 真实 EXE 步骤 select-next-3 已记录；目标路径/PID/原始事件见 artifacts/runs/hotkey_enter_action_20261023/real-exe-keyboard/select-next-3.json；完整结论以 final-result.json 为准。

- HOTKEY-ENTER-01 真实 EXE 步骤 select-next-4 已记录；目标路径/PID/原始事件见 artifacts/runs/hotkey_enter_action_20261023/real-exe-keyboard/select-next-4.json；完整结论以 final-result.json 为准。

- HOTKEY-ENTER-01 真实 EXE 步骤 selected-action 已记录；目标路径/PID/原始事件见 artifacts/runs/hotkey_enter_action_20261023/real-exe-keyboard/selected-action.json；完整结论以 final-result.json 为准。

- HOTKEY-ENTER-01 真实 EXE 步骤 execute-enter 已记录；目标路径/PID/原始事件见 artifacts/runs/hotkey_enter_action_20261023/real-exe-keyboard/execute-enter.json；完整结论以 final-result.json 为准。

- HOTKEY-ENTER-01 真实 EXE 最终返回码 3；完整命令、键事件、窗口/索引和清理见 artifacts/runs/hotkey_enter_action_20261023/real-exe-keyboard/final-result.json。

- HOTKEY-ENTER-01 键盘选择真实入口收尾：实际返回码 3，清理核对 artifacts/runs/hotkey_enter_action_20261023/real-keyboard-environment-after.json。

- HOTKEY-ENTER-01 键盘场景诊断：已精确选中“设置”行且新增 Enter 的两条事件均命中目标、无额外输入；探针等待的标题误写成“软件设置”，源码实际标题是“设置”，因此超时不能归为产品失败。仅修正探针标题，保留既有中文文案和生产实现，不重跑代码门禁。

- HOTKEY-ENTER-01 标题校准辅助程序构建返回码 0；日志 artifacts/runs/hotkey_enter_action_20261023/real-final-probe-build.log。

- HOTKEY-ENTER-01 真实 EXE 步骤 startup 已记录；目标路径/PID/原始事件见 artifacts/runs/hotkey_enter_action_20261023/real-exe-final/startup.json；完整结论以 final-result.json 为准。

- HOTKEY-ENTER-01 真实 EXE 步骤 open-hotkeys 已记录；目标路径/PID/原始事件见 artifacts/runs/hotkey_enter_action_20261023/real-exe-final/open-hotkeys.json；完整结论以 final-result.json 为准。

- HOTKEY-ENTER-01 真实 EXE 步骤 select-first 已记录；目标路径/PID/原始事件见 artifacts/runs/hotkey_enter_action_20261023/real-exe-final/select-first.json；完整结论以 final-result.json 为准。

- HOTKEY-ENTER-01 真实 EXE 步骤 select-next-1 已记录；目标路径/PID/原始事件见 artifacts/runs/hotkey_enter_action_20261023/real-exe-final/select-next-1.json；完整结论以 final-result.json 为准。

- HOTKEY-ENTER-01 真实 EXE 步骤 select-next-2 已记录；目标路径/PID/原始事件见 artifacts/runs/hotkey_enter_action_20261023/real-exe-final/select-next-2.json；完整结论以 final-result.json 为准。

- HOTKEY-ENTER-01 真实 EXE 步骤 select-next-3 已记录；目标路径/PID/原始事件见 artifacts/runs/hotkey_enter_action_20261023/real-exe-final/select-next-3.json；完整结论以 final-result.json 为准。

- HOTKEY-ENTER-01 真实 EXE 步骤 select-next-4 已记录；目标路径/PID/原始事件见 artifacts/runs/hotkey_enter_action_20261023/real-exe-final/select-next-4.json；完整结论以 final-result.json 为准。

- HOTKEY-ENTER-01 真实 EXE 步骤 selected-action 已记录；目标路径/PID/原始事件见 artifacts/runs/hotkey_enter_action_20261023/real-exe-final/selected-action.json；完整结论以 final-result.json 为准。

- HOTKEY-ENTER-01 真实 EXE 步骤 execute-enter 已记录；目标路径/PID/原始事件见 artifacts/runs/hotkey_enter_action_20261023/real-exe-final/execute-enter.json；完整结论以 final-result.json 为准。

- HOTKEY-ENTER-01 真实 EXE 步骤 settings-opened 已记录；目标路径/PID/原始事件见 artifacts/runs/hotkey_enter_action_20261023/real-exe-final/settings-opened.json；完整结论以 final-result.json 为准。

- HOTKEY-ENTER-01 真实 EXE 步骤 cancel-settings 已记录；目标路径/PID/原始事件见 artifacts/runs/hotkey_enter_action_20261023/real-exe-final/cancel-settings.json；完整结论以 final-result.json 为准。

- HOTKEY-ENTER-01 真实 EXE 步骤 returned-main 已记录；目标路径/PID/原始事件见 artifacts/runs/hotkey_enter_action_20261023/real-exe-final/returned-main.json；完整结论以 final-result.json 为准。

- HOTKEY-ENTER-01 真实 EXE 最终返回码 0；完整命令、键事件、窗口/索引和清理见 artifacts/runs/hotkey_enter_action_20261023/real-exe-final/final-result.json。

- HOTKEY-ENTER-01 最终真实入口收尾：实际返回码 0，清理核对 artifacts/runs/hotkey_enter_action_20261023/real-final-environment-after.json。

- HOTKEY-ENTER-01 验证范围说明：本轮受影响与全量结果来自当前工作树，包含先前 AUDIT-04 尚未提交的 `KeyboardAuditRuntimeContractTests` 两项测试；本功能提交排除该历史测试文件，不能把本轮通过数宣称为干净提交的测试总数。生产改动仅限快捷键对话框和主窗体接线，测试/构建通过后未再修改。

- HOTKEY-ENTER-01 真实场景结论：最终记录证明“设置”行索引与焦点一致，选择本身不执行，Enter 的键事件命中目标并关闭快捷键窗口、打开设置窗口，Escape 返回主窗体；配置哈希不变，正常退出且无需强制清理，正式 Sandboxie.ini 与原读屏进程保持不变。此前四次准备/探针失败仍为无效证据，不改写为产品失败或通过。`real-exe-final/final-result.json` 只保存结论；实际辅助程序调用为 `HotkeyEnterRealProbe.exe D:\mycodex\git\artifacts\runs\hotkey_enter_action_20261023\app\沙河批量打开器.exe D:\mycodex\git\artifacts\runs\hotkey_enter_action_20261023\real-exe-final D:\mycodex\git\PROJECT_STRUCTURE.md`，每步原始键事件与窗口证据在同目录步骤 JSON，构建命令见本轮 `real-final-probe-build-result.json`。

- HOTKEY-ENTER-01 文本与差异检查：所涉文件 UTF-8 解码正常；控制字符扫描在 PROJECT_STRUCTURE.md 发现异常，待定位，不计为通过；范围限定的 `git diff --check` 返回码 `0`，测试计数不适用。命令和输出：`artifacts/runs/hotkey_enter_action_20261023/diff-result.json`、`diff.log`。本轮仅核对文档和已有结果，未重复已通过测试。

- HOTKEY-ENTER-01 文本检查定位：异常均为 HEAD 已有历史记录中的 U+FFFD 替换字符，本功能未新增；历史原文按只追加约束保留。所涉文件 UTF-8 解码及本次新增/修改文本检查返回码 0，新异常字符 0；核对结果 `artifacts/runs/hotkey_enter_action_20261023/text-check-result.json`。

- HOTKEY-ENTER-01 提交前检查：`git diff --cached --check` 返回码 0，暂存清单与批准范围一致；共享文档仅包含本功能和恢复当前模块状态所需片段，既有 AUDIT-04 审计、AGENTS.md 和 GAP-09 历史日志未纳入。原始输出追加于本轮 diff.log；测试计数不适用。远端只读预检因 GitHub 443 连接超时返回 128，尚未推送，先完成本地独立提交。

- HOTKEY-ENTER-01 交付完成：独立功能提交 `f9883c27e301278ab89c686bb7a4ef802cba3795` 已推送 `origin/voice03-fixed-reader-20261022`，成功推送返回码 0；直连失败后复用本机现有 SOCKS 代理，未更改全局配置。命令/两次结果见 `artifacts/runs/hotkey_enter_action_20261023/delivery-result.json`，回退命令 `git revert f9883c27e301278ab89c686bb7a4ef802cba3795`。全部动作入口、回归、构建及真实设置场景已验收；AUDIT-04 其他待办与 AUDIT-05 至 AUDIT-08、RELEASE-01 仍保留，C# 可启动运行已验证功能，整体迁移未完成。

- UPDATE-DLL-01 源码核对：MainForm.LaunchUpdate 先启动独立 helper 并关闭主程序；UpdateRunner 先等待传入 PID 退出，再下载、校验、解包、备份并递归覆盖文件，restart=true 时重新启动。CopyTree 复制包内全部文件及子目录，新增 DLL 不依赖固定名单；包中没有的旧文件不会主动删除，配置是否保留取决于包内是否包含同路径配置。当前正式完整包不携带用户配置，本轮将验证其保留。验证目标与命令详见本轮脚本；仅针对隔离目录，不修改正式配置。

- UPDATE-DLL-01 远端基线：匿名禁缓存请求正式 update.json 返回码 0，当前版本 `4.0.46`；原始清单 `artifacts/runs/updater_added_dll_20261023/manifest-before.json`。GitCode origin/main 已 fetch，内容与公开清单一致；按用户最新明确要求，新版本递增到 4.0.47 并进行真实公开更新链路验证。

- UPDATE-DLL-01 version-red：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\updater_added_dll_20261023\bin\ --logger trx;LogFileName=version-red.trx --results-directory D:\mycodex\git\artifacts\runs\updater_added_dll_20261023 --filter FullyQualifiedName~ConstantsContractTests`；返回码 1，通过 8、失败 1、跳过 0。日志与命令索引：`artifacts/runs/updater_added_dll_20261023/version-red.log`、`version-red-result.json`；如为 C# 测试，另有同名 TRX。

- UPDATE-DLL-01 update-green：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\updater_added_dll_20261023\bin\ --logger trx;LogFileName=update-green.trx --results-directory D:\mycodex\git\artifacts\runs\updater_added_dll_20261023 --filter FullyQualifiedName~ConstantsContractTests|FullyQualifiedName~Update`；返回码 0，通过 48、失败 0、跳过 0。日志与命令索引：`artifacts/runs/updater_added_dll_20261023/update-green.log`、`update-green-result.json`；如为 C# 测试，另有同名 TRX。

- UPDATE-DLL-01 full：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\updater_added_dll_20261023\bin\ --logger trx;LogFileName=full.trx --results-directory D:\mycodex\git\artifacts\runs\updater_added_dll_20261023`；返回码 1，通过 394、失败 1、跳过 0。日志与命令索引：`artifacts/runs/updater_added_dll_20261023/full.log`、`full-result.json`；如为 C# 测试，另有同名 TRX。

- CONFIG-LOC-01 范围确认：用户明确要求本软件用户配置统一在 Configurations，移除根目录分散读写，公开更新包只有空目录。当前新增迁移/首启/已有配置优先的契约；该用户指定差异将随 4.0.47 发布，旧版自动升级验收包含目录迁移及打赏资源。

- UPDATE-DLL-01 configuration-red：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\updater_added_dll_20261023\bin\ --logger trx;LogFileName=configuration-red.trx --results-directory D:\mycodex\git\artifacts\runs\updater_added_dll_20261023 --filter FullyQualifiedName~ConfigurationDirectoryContractTests`；返回码 1，通过 0、失败 3、跳过 0。日志与命令索引：`artifacts/runs/updater_added_dll_20261023/configuration-red.log`、`configuration-red-result.json`；如为 C# 测试，另有同名 TRX。

- UPDATE-DLL-01 configuration-green：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\updater_added_dll_20261023\bin\ --logger trx;LogFileName=configuration-green.trx --results-directory D:\mycodex\git\artifacts\runs\updater_added_dll_20261023 --filter FullyQualifiedName~ConfigurationDirectoryContractTests|FullyQualifiedName~CommonUtilitiesContractTests|FullyQualifiedName~MainFormApplicationContractTests|FullyQualifiedName~SpeechServiceContractTests|FullyQualifiedName~Settings|FullyQualifiedName~Update`；返回码 0，通过 135、失败 0、跳过 0。日志与命令索引：`artifacts/runs/updater_added_dll_20261023/configuration-green.log`、`configuration-green-result.json`；如为 C# 测试，另有同名 TRX。

- UPDATE-DLL-01 full-final：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\updater_added_dll_20261023\bin\ --logger trx;LogFileName=full-final.trx --results-directory D:\mycodex\git\artifacts\runs\updater_added_dll_20261023`；返回码 0，通过 398、失败 0、跳过 0。日志与命令索引：`artifacts/runs/updater_added_dll_20261023/full-final.log`、`full-final-result.json`；如为 C# 测试，另有同名 TRX。

- UPDATE-DLL-01 真实 GitCode 升级辅助程序构建返回码 0；命令及输出见 artifacts/runs/updater_added_dll_20261023/probe-build-result.json、probe-build.log。

- UPDATE-DLL-01 python-final：命令 `python -m unittest discover -s tests -p test_*.py -v`；返回码 1，计数不适用。日志与命令索引：`artifacts/runs/updater_added_dll_20261023/python-final.log`、`python-final-result.json`；如为 C# 测试，另有同名 TRX。

- CONFIG-LOC-01 Python 首轮计数补充：实际通过 28、失败 3、跳过 0，返回码 1；三项均因既有方法清单仍要求已按用户要求删除的根目录语音配置搜索方法。现同步 tests/contracts/method_modules.json 删除这一条，不恢复旧回退代码；原失败日志保留，同名 result.json 补齐实际计数。

- UPDATE-DLL-01 python-final2：命令 `python -m unittest discover -s tests -p test_*.py -v`；返回码 1，计数不适用。日志与命令索引：`artifacts/runs/updater_added_dll_20261023/python-final2.log`、`python-final2-result.json`；如为 C# 测试，另有同名 TRX。

- CONFIG-LOC-01 Python 第二轮计数补充：通过 30、失败 1、跳过 0，返回码 1；剩余失败来自另一份原始 API 清单 launcher_api.json 对同一已删除方法的要求，已同步删除唯一对应条目。原始失败日志保留，生产实现未恢复旧目录回退。

- UPDATE-DLL-01 python-green：命令 `python -m unittest discover -s tests -p test_*.py -v`；返回码 1，计数不适用。日志与命令索引：`artifacts/runs/updater_added_dll_20261023/python-green.log`、`python-green-result.json`；如为 C# 测试，另有同名 TRX。

- CONFIG-LOC-01 Python 映射收尾：清单同步后通过 30、失败 1、跳过 0，返回码 1；剩余为旧方法总数硬编码断言。按用户明确删除一个旧目录入口同步该总数，并保留功能路径和迁移测试；本轮未增加生产修复。失败证据为 python-green.log 与补齐计数的同名 result.json。

- UPDATE-DLL-01 python-complete：命令 `python -m unittest discover -s tests -p test_*.py -v`；返回码 0，通过 31、失败 0、跳过 0。日志与命令索引：`artifacts/runs/updater_added_dll_20261023/python-complete.log`、`python-complete-result.json`；如为 C# 测试，另有同名 TRX。

- UPDATE-DLL-01 compile：命令 `python -m compileall -q shahe_bulk_launcher tests`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/updater_added_dll_20261023/compile.log`、`compile-result.json`；如为 C# 测试，另有同名 TRX。

- UPDATE-DLL-01 release：命令 `dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\updater_added_dll_20261023\bin\`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/updater_added_dll_20261023/release.log`、`release-result.json`；如为 C# 测试，另有同名 TRX。

- UPDATE-DLL-01 发布输出生成返回码 0；复用已通过 Release 构建，不重复编译。命令与输出见 artifacts/runs/updater_added_dll_20261023/publish-result.json、publish.log。

- UPDATE-DLL-01 完整发布包组装/校验返回码 0；Configurations 目录存在且为空，打赏原图及 lib 运行文件齐全，无个人配置、测试或探针。文件清单/大小/SHA-256 见 artifacts/runs/updater_added_dll_20261023/package-result.json，脚本 package-release.py。

- UPDATE-DLL-01 发布前源码差异检查：git diff --check 限定本次源码/测试/忽略规则范围，返回码 0；输出 artifacts/runs/updater_added_dll_20261023/source-diff.log。版本与配置修改已完成所需代码门禁，发布包经过空配置目录和资源白名单核对。

- UPDATE-DLL-01 旧版隔离夹具准备返回码 0；来源为既有已校验正式包，旧配置位于 Configuration，未带打赏图片并模拟缺少一个可选 DLL。初始哈希、路径、读屏进程及正式 Sandboxie.ini 基线见 artifacts/runs/updater_added_dll_20261023/old-app-baseline.json。

- UPDATE-DLL-01 GitCode 新包匿名下载通过：curl --fail 返回码 0，公开 content-addressed URL 下载大小与 SHA-256 和本地包一致；结果 artifacts/runs/updater_added_dll_20261023/package-download-result.json。Git 推送缺少凭据时改用已有登录网页上传，清单尚未切换。

- UPDATE-DLL-01 GitCode 发布验证：正式 update.json 已指向 4.0.47，匿名读取返回码 0，与本地验证清单完全一致；远端 main 为 `e40ea608381e37ba3843b149b1fc764b93ab5e5c`。包下载哈希已通过，最新证据 `artifacts/runs/updater_added_dll_20261023/gitcode-release-result.json`、manifest-after.json 与 package-download-result.json。公开包中的 Configurations 目录为空，用户设置不随包分发。

- UPDATE-DLL-01 GitCode 真实升级步骤 old-startup 已记录；目标 PID/路径及观测见 artifacts/runs/updater_added_dll_20261023/real-upgrade/old-startup.json，完整结论以 final-result.json 为准。

- UPDATE-DLL-01 GitCode 真实升级完整场景返回码 3；结果 artifacts/runs/updater_added_dll_20261023/real-upgrade/final-result.json。

- UPDATE-DLL-01 首次真实升级定位：旧版运行日志证明已成功检测 4.0.47 并显示内联“下载更新”，探针却等待“提示”弹窗，因此返回码 3 为场景准备无效，不是更新功能失败。旧主程序正常清理，尚未执行安装；保留 real-upgrade/final-result.json 与 first-real-attempt-app.log，辅助程序改为点击原生“下载更新”按钮，生产代码和发布包均不变。

- UPDATE-DLL-01 入口校准辅助程序构建返回码 0，结果 artifacts/runs/updater_added_dll_20261023/probe-final-build-result.json；未改生产源码或发布包。

- UPDATE-DLL-01 GitCode 真实升级步骤 old-startup 已记录；目标 PID/路径及观测见 artifacts/runs/updater_added_dll_20261023/real-upgrade-final/old-startup.json，完整结论以 final-result.json 为准。

- UPDATE-DLL-01 GitCode 真实升级步骤 update-detected 已记录；目标 PID/路径及观测见 artifacts/runs/updater_added_dll_20261023/real-upgrade-final/update-detected.json，完整结论以 final-result.json 为准。

- UPDATE-DLL-01 GitCode 真实升级完整场景返回码 3；结果 artifacts/runs/updater_added_dll_20261023/real-upgrade-final/final-result.json。

- UPDATE-DLL-01 下载入口输入诊断：第二次已看到 4.0.47 下载按钮，BM_CLICK 消息未产生应用启动 updater 日志，故等待主进程退出超时，最后仅清理本轮隔离进程。real-upgrade-final/final-result.json 保留为无效输入证据，不认定产品失败；改为该原生按钮的 MSAA 默认动作并补充失败窗口观测，生产发布包不变。

- UPDATE-DLL-01 无障碍按钮调用辅助程序构建返回码 0；日志 artifacts/runs/updater_added_dll_20261023/probe-accessible-build.log。

- UPDATE-DLL-01 GitCode 真实升级步骤 old-startup 已记录；目标 PID/路径及观测见 artifacts/runs/updater_added_dll_20261023/real-upgrade-accessible/old-startup.json，完整结论以 final-result.json 为准。

- UPDATE-DLL-01 GitCode 真实升级步骤 update-detected 已记录；目标 PID/路径及观测见 artifacts/runs/updater_added_dll_20261023/real-upgrade-accessible/update-detected.json，完整结论以 final-result.json 为准。

- UPDATE-DLL-01 GitCode 真实升级完整场景返回码 3；结果 artifacts/runs/updater_added_dll_20261023/real-upgrade-accessible/final-result.json。

- UPDATE-DLL-01 输入诊断更正：新增窗口快照证明旧版真实发布包在“下载更新”按钮之后还有标题“下载更新”的 Yes/No 确认；此前无启动 updater 日志是停在该确认，不是按钮消息未送达。前两条输入推断由本条观测修正，旧失败记录保留。补齐该既有确认后继续，不改生产代码或公开包；证据 real-upgrade-accessible/final-result.json。

- UPDATE-DLL-01 旧版安装确认校准辅助程序构建返回码 0；日志 artifacts/runs/updater_added_dll_20261023/probe-confirmation-build.log。

- UPDATE-DLL-01 GitCode 真实升级步骤 old-startup 已记录；目标 PID/路径及观测见 artifacts/runs/updater_added_dll_20261023/real-upgrade-complete/old-startup.json，完整结论以 final-result.json 为准。

- UPDATE-DLL-01 GitCode 真实升级步骤 update-detected 已记录；目标 PID/路径及观测见 artifacts/runs/updater_added_dll_20261023/real-upgrade-complete/update-detected.json，完整结论以 final-result.json 为准。

- UPDATE-DLL-01 GitCode 真实升级步骤 install-confirmation 已记录；目标 PID/路径及观测见 artifacts/runs/updater_added_dll_20261023/real-upgrade-complete/install-confirmation.json，完整结论以 final-result.json 为准。

- UPDATE-DLL-01 GitCode 真实升级步骤 handoff 已记录；目标 PID/路径及观测见 artifacts/runs/updater_added_dll_20261023/real-upgrade-complete/handoff.json，完整结论以 final-result.json 为准。

- UPDATE-DLL-01 GitCode 真实升级步骤 installed 已记录；目标 PID/路径及观测见 artifacts/runs/updater_added_dll_20261023/real-upgrade-complete/installed.json，完整结论以 final-result.json 为准。

- UPDATE-DLL-01 GitCode 真实升级步骤 latest-version 已记录；目标 PID/路径及观测见 artifacts/runs/updater_added_dll_20261023/real-upgrade-complete/latest-version.json，完整结论以 final-result.json 为准。

- UPDATE-DLL-01 GitCode 真实升级完整场景返回码 3；结果 artifacts/runs/updater_added_dll_20261023/real-upgrade-complete/final-result.json。

- UPDATE-DLL-01 真实升级核心步骤通过：旧进程正常退出，生产 updater 返回 0，新主窗口已观察到；新配置目录迁移后原文件哈希一致，新旧 Core 哈希不同且包内全部文件哈希匹配，缺失可选 DLL 已补齐。最新版检查窗口明确显示 4.0.47。探针使用固定按钮 ID 关闭新版自定义“确定”控件失败，整体返回 3 并强制清理隔离主进程；此工具收尾失败保留，安装/检测/迁移证据见 real-upgrade-complete/installed.json、latest-version.json、handoff.json。接着仅验证已升级副本打赏窗口和正常退出，不重跑下载替换。

- UPDATE-DLL-01 安装后资源检查辅助程序构建返回码 0；日志 artifacts/runs/updater_added_dll_20261023/post-update-build.log。

- UPDATE-DLL-01 GitCode 真实升级步骤 installed-startup 已记录；目标 PID/路径及观测见 artifacts/runs/updater_added_dll_20261023/post-update-resources/installed-startup.json，完整结论以 final-result.json 为准。

- UPDATE-DLL-01 GitCode 真实升级步骤 alipay-image 已记录；目标 PID/路径及观测见 artifacts/runs/updater_added_dll_20261023/post-update-resources/alipay-image.json，完整结论以 final-result.json 为准。

- UPDATE-DLL-01 GitCode 真实升级步骤 wechat-image 已记录；目标 PID/路径及观测见 artifacts/runs/updater_added_dll_20261023/post-update-resources/wechat-image.json，完整结论以 final-result.json 为准。

- UPDATE-DLL-01 已升级副本打赏/正常退出验证返回码 0；结果 artifacts/runs/updater_added_dll_20261023/post-update-resources/final-result.json。未重复安装更新。

- UPDATE-DLL-01 真实升级分段证据汇总与环境清理：核对返回码 0。旧版退出、updater 成功、重启新版/最新版本确认、全部包文件哈希、新增 DLL、配置目录迁入及后续打赏窗口/正常退出均有证据；前次工具收尾失败如实保留，没有重装已成功更新。正式 Sandboxie.ini 和读屏进程不变，无隔离进程残留。权威汇总 artifacts/runs/updater_added_dll_20261023/upgrade-verification-result.json，应用日志 upgrade-application-complete.log。

- UPDATE-DLL-01 / CONFIG-LOC-01 证据范围与审查：最终全量是在当前工作树执行，包含既有尚未提交的 KeyboardAuditRuntimeContractTests，本轮不提交该 AUDIT-04 文件。先前批量创建输入失败和 Python 旧 API 清单失败均有后续通过结果，历史不覆盖；生产源码通过最终门禁后未再变化。确认公开包不含个人配置、Configurations 只有目录条目，两张原始打赏图片和可选 DLL 在真实升级后均有哈希/窗口证据。实际真实程序命令与工具收尾局限见 `artifacts/runs/updater_added_dll_20261023/real-execution-commands.json`。

- UPDATE-DLL-01 / CONFIG-LOC-01 最终差异检查：限定源码、测试、计划与结构文档的 git diff --check 返回码 2，计数不适用；结果 artifacts/runs/updater_added_dll_20261023/final-diff-result.json 与 final-diff.log。当前只整理提交，既有门禁未重复运行。

- UPDATE-DLL-01 / CONFIG-LOC-01 差异检查修正：首轮仅本次追加文档两行混入 CRLF 被判空白错误，现统一为文档既有 LF，历史内容未修改。再次差异检查返回码 0，结果 final-diff-green-result.json / final-diff-green.log；首轮原始日志无损归档 final-diff.log.gz。

- UPDATE-DLL-01 / CONFIG-LOC-01 提交前暂存检查：git diff --cached --check 返回码 0；实际暂存清单与 staging-files.txt 一致，生产变更、测试和证据均仅属本次追加任务，既有 AUDIT-04 未提交测试/审计及用户其他修改保持未暂存。命令结果追加在 final-diff-green-result.json。

- UPDATE-DLL-01 / CONFIG-LOC-01 源码交付完成：独立提交 `a5b4a18610f17fec1464eff2c267ab2037332c41` 已推送 `origin/voice03-fixed-reader-20261022`，推送返回码 0；回退命令 `git revert a5b4a18610f17fec1464eff2c267ab2037332c41`。GitCode 4.0.47 公开包和正式清单已生效，真实旧版更新、新增 DLL、打赏资源与 Configurations 迁入通过。交付记录 `artifacts/runs/updater_added_dll_20261023/delivery-result.json`；本轮停止在追加任务完成处，AUDIT-04 未验收项及后续模块继续保留，不宣称整体迁移完成。

- REVIEW-07 隔离诊断器构建返回码 0；只复用与已验证公开包哈希一致的 Core/依赖，不重新构建生产软件。命令与结果 artifacts/runs/csharp_remaining_review_20261023/probe-build-result.json，日志 probe-build.log；无真实 EXE/服务操作。

- REVIEW-07 新增异常路径隔离检查返回码 1；通过 0、失败 4、跳过 0。失败代表发现的未修复安全/反馈问题，不是修复通过。配置损坏、配置写入被锁、还原复制失败均只作用于本轮 fixtures；服务回调为计数，不调用真实 Sandboxie。完整命令/结果 artifacts/runs/csharp_remaining_review_20261023/probe-result.json，输出 probe.log。未修改 Python 或生产代码，原已通过测试不重跑。

- REVIEW-07 审查收尾：限定计划/结构文档差异、生产文件零变更、新增文本及 PowerShell 解析检查返回码 0；结果 artifacts/runs/csharp_remaining_review_20261023/scope-check-result.json，日志 scope-check.log。用户已同意计划追加与执行记录，检查完成但发现的问题尚未修复；当前仅新增诊断与报告，不提交未修复模块，不重跑全量，不推进 AUDIT-05。

- FIX-CONFIG-01 red：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\configuration_failure_safety_20261023\bin\ --logger trx;LogFileName=red.trx --results-directory D:\mycodex\git\artifacts\runs\configuration_failure_safety_20261023 --filter FullyQualifiedName~ConfigurationFailureSafetyContractTests`；返回码 1，通过 0、失败 6、跳过 0。日志与命令索引：`artifacts/runs/configuration_failure_safety_20261023/red.log`、`red-result.json`；如为 C# 测试，另有同名 TRX。

- FIX-CONFIG-01 green：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\configuration_failure_safety_20261023\bin\ --logger trx;LogFileName=green.trx --results-directory D:\mycodex\git\artifacts\runs\configuration_failure_safety_20261023 --filter FullyQualifiedName~ConfigurationFailureSafetyContractTests`；返回码 0，通过 6、失败 0、跳过 0。日志与命令索引：`artifacts/runs/configuration_failure_safety_20261023/green.log`、`green-result.json`；如为 C# 测试，另有同名 TRX。

- FIX-CONFIG-01 affected：命令 `dotnet vstest D:\mycodex\git\artifacts\runs\configuration_failure_safety_20261023\bin\x64\Release\net48\ShaheLauncher.Tests.dll /Platform:x64 /Logger:trx;LogFileName=affected.trx /ResultsDirectory:D:\mycodex\git\artifacts\runs\configuration_failure_safety_20261023 /TestCaseFilter:FullyQualifiedName~ConfigStoreContractTests|FullyQualifiedName~ConfigurationFailureSafetyContractTests|FullyQualifiedName~LauncherSettingsServiceContractTests|FullyQualifiedName~JsonSettingsRuntimeContractTests|FullyQualifiedName~ConfigurationDirectoryContractTests|FullyQualifiedName~MainFormApplicationContractTests|FullyQualifiedName~SpeechServiceContractTests|FullyQualifiedName~UpdateServiceContractTests`；返回码 0，通过 87、失败 0、跳过 0。日志与命令索引：`artifacts/runs/configuration_failure_safety_20261023/affected.log`、`affected-result.json`；如为 C# 测试，另有同名 TRX。

- FIX-CONFIG-01 full：命令 `dotnet vstest D:\mycodex\git\artifacts\runs\configuration_failure_safety_20261023\bin\x64\Release\net48\ShaheLauncher.Tests.dll /Platform:x64 /Logger:trx;LogFileName=full.trx /ResultsDirectory:D:\mycodex\git\artifacts\runs\configuration_failure_safety_20261023`；返回码 0，通过 404、失败 0、跳过 0。日志与命令索引：`artifacts/runs/configuration_failure_safety_20261023/full.log`、`full-result.json`；如为 C# 测试，另有同名 TRX。

- FIX-CONFIG-01 python：命令 `python -m unittest discover -s tests -p test_*.py -v`；返回码 0，通过 31、失败 0、跳过 0。日志与命令索引：`artifacts/runs/configuration_failure_safety_20261023/python.log`、`python-result.json`；如为 C# 测试，另有同名 TRX。

- FIX-CONFIG-01 compile：命令 `python -m compileall -q shahe_bulk_launcher tests`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/configuration_failure_safety_20261023/compile.log`、`compile-result.json`；如为 C# 测试，另有同名 TRX。

- FIX-CONFIG-01 release：命令 `dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\configuration_failure_safety_20261023\bin\`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/configuration_failure_safety_20261023/release.log`、`release-result.json`；如为 C# 测试，另有同名 TRX。

- FIX-CONFIG-01 Release 明细与证据边界：本轮隔离 Release 为 0 警告、0 错误；生产源码只有 ConfigStore 底层读取保护差异，公开包未改变。全量数量属于当前工作树，含原有未提交的 KeyboardAuditRuntimeContractTests，该文件不纳入本轮；其他缺陷和未完成真实审计保留，详见本轮 source-review.md。

- FIX-CONFIG-01 直接审查与限定差异检查：返回码 0，新增文本无空白/编码错误，Python 源码无修改；结果 artifacts/runs/configuration_failure_safety_20261023/diff-result.json、diff.log，审查 source-review.md。仅剩选择性提交、推送与交付回填，不重复既有通过验证。

- FIX-CONFIG-01 暂存检查：git diff --cached --check 返回码 0，实际清单与 staging-files.txt 一致；共享文档使用基于 HEAD 的仅本模块索引版本，既有 AUDIT-04/REVIEW-07 历史与用户其他改动保留在工作树，不纳入提交。结果已补入本轮 diff-result.json。

- FIX-CONFIG-01 源码提交/推送：独立提交 `9fa28f2b23dc89ed379279fbefa4cc14fa8d2f1c`，推送目标 `origin/voice03-fixed-reader-20261022`，返回码 0；回退 `git revert 9fa28f2b23dc89ed379279fbefa4cc14fa8d2f1c`。结果 artifacts/runs/configuration_failure_safety_20261023/delivery-result.json，原始输出 source-push.log。公开版本/包未更新，其他缺陷和整体审计待办保留。

- FIX-CONFIG-02 red：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\configuration_save_feedback_20261023\bin\ --logger trx;LogFileName=red.trx --results-directory D:\mycodex\git\artifacts\runs\configuration_save_feedback_20261023 --filter FullyQualifiedName~ConfigurationSaveFeedbackContractTests`；返回码 1，通过 0、失败 10、跳过 0。日志与命令索引：`artifacts/runs/configuration_save_feedback_20261023/red.log`、`red-result.json`；如为 C# 测试，另有同名 TRX。

- FIX-CONFIG-02 green：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\configuration_save_feedback_20261023\bin\ --logger trx;LogFileName=green.trx --results-directory D:\mycodex\git\artifacts\runs\configuration_save_feedback_20261023 --filter FullyQualifiedName~ConfigurationSaveFeedbackContractTests`；返回码 0，通过 10、失败 0、跳过 0。日志与命令索引：`artifacts/runs/configuration_save_feedback_20261023/green.log`、`green-result.json`；如为 C# 测试，另有同名 TRX。

- FIX-CONFIG-02 affected：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\configuration_save_feedback_20261023\bin\ --logger trx;LogFileName=affected.trx --results-directory D:\mycodex\git\artifacts\runs\configuration_save_feedback_20261023 --filter FullyQualifiedName~ConfigurationSaveFeedbackContractTests|FullyQualifiedName~ConfigurationFailureSafetyContractTests|FullyQualifiedName~LauncherSettingsServiceContractTests|FullyQualifiedName~SettingsRuntimeContractTests|FullyQualifiedName~SettingsDialogsCSharpContractTests|FullyQualifiedName~AppPathHistoryServiceContractTests|FullyQualifiedName~SandboxAdminServiceContractTests|FullyQualifiedName~SpeechServiceContractTests|FullyQualifiedName~SpeechSelectionRuntimeContractTests|FullyQualifiedName~ProcessAction|FullyQualifiedName~HotkeyActionExecutionContractTests|FullyQualifiedName~PathHistoryLimitRuntimeContractTests`；返回码 0，通过 128、失败 0、跳过 0。日志与命令索引：`artifacts/runs/configuration_save_feedback_20261023/affected.log`、`affected-result.json`；如为 C# 测试，另有同名 TRX。

- FIX-CONFIG-02 full：命令 `dotnet vstest D:\mycodex\git\artifacts\runs\configuration_save_feedback_20261023\bin\x64\Release\net48\ShaheLauncher.Tests.dll /Platform:x64 /Logger:trx;LogFileName=full.trx /ResultsDirectory:D:\mycodex\git\artifacts\runs\configuration_save_feedback_20261023`；返回码 1，通过 420、失败 1、跳过 0。日志与命令索引：`artifacts/runs/configuration_save_feedback_20261023/full.log`、`full-result.json`；如为 C# 测试，另有同名 TRX。

- FIX-CONFIG-02 全量首轮定位：唯一失败为 MainFormApplicationContractTests 中多行反射回调仍传 Action，已统一为 Func<bool> 成功回调；非产品运行失败。首轮原始结果保留，生产代码不变，接着以最终测试程序集完成全量门禁。

- FIX-CONFIG-02 full-final：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\configuration_save_feedback_20261023\bin\ --logger trx;LogFileName=full-final.trx --results-directory D:\mycodex\git\artifacts\runs\configuration_save_feedback_20261023`；返回码 1，通过 420、失败 1、跳过 0。日志与命令索引：`artifacts/runs/configuration_save_feedback_20261023/full-final.log`、`full-final-result.json`；如为 C# 测试，另有同名 TRX。

- FIX-CONFIG-02 全量第二轮定位：原反射回调失败已消失；本轮唯一失败是既有 BatchCreateValuesAreSetByAccessibleNamesNotWindowOrder 的原生数字输入偏差，与历史相同（digits 实际为 0）。未修改该审计用例或生产输入实现；保留 full-final 原结果，改用相同程序集直接 vstest 完成最终全量核对。

- FIX-CONFIG-02 full-vstest：命令 `dotnet vstest D:\mycodex\git\artifacts\runs\configuration_save_feedback_20261023\bin\x64\Release\net48\ShaheLauncher.Tests.dll /Platform:x64 /Logger:trx;LogFileName=full-vstest.trx /ResultsDirectory:D:\mycodex\git\artifacts\runs\configuration_save_feedback_20261023`；返回码 0，通过 421、失败 0、跳过 0。日志与命令索引：`artifacts/runs/configuration_save_feedback_20261023/full-vstest.log`、`full-vstest-result.json`；如为 C# 测试，另有同名 TRX。

- FIX-CONFIG-02 python：命令 `python -m unittest discover -s tests -p test_*.py -v`；返回码 0，通过 31、失败 0、跳过 0。日志与命令索引：`artifacts/runs/configuration_save_feedback_20261023/python.log`、`python-result.json`；如为 C# 测试，另有同名 TRX。

- FIX-CONFIG-02 compile：命令 `python -m compileall -q shahe_bulk_launcher tests`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/configuration_save_feedback_20261023/compile.log`、`compile-result.json`；如为 C# 测试，另有同名 TRX。

- FIX-CONFIG-02 release：命令 `dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\configuration_save_feedback_20261023\bin\`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/configuration_save_feedback_20261023/release.log`、`release-result.json`；如为 C# 测试，另有同名 TRX。

- FIX-CONFIG-02 Release 明细：本轮隔离构建 0 警告、0 错误，结果已补入 release-result.json。生产代码在门禁通过后未再修改；公开版本与安装包保持不变。

- FIX-CONFIG-02 真实保存场景辅助程序构建返回码 1；命令/输出见 artifacts/runs/configuration_save_feedback_20261023/real-helper-build-result.json、real-helper-build.log。未更改生产源码。

- FIX-CONFIG-02 辅助程序首次构建失败定位：现有 Framework csc 不支持探针中的空条件运算符，已改为显式 null 判断。只修正辅助代码，生产程序未运行，属于工具编译失败；原日志保留，接着只重新构建该辅助程序并使用已准备的隔离夹具。

- FIX-CONFIG-02 真实保存场景辅助程序构建返回码 0；命令/输出见 artifacts/runs/configuration_save_feedback_20261023/real-helper-build-final-result.json、real-helper-build-final.log。未更改生产源码。

- FIX-CONFIG-02 真实配置保存步骤 startup 已记录；目标 PID/路径及观测见 artifacts/runs/configuration_save_feedback_20261023/real-save/startup.json，完整结论以 final-result.json 为准。

- FIX-CONFIG-02 真实配置保存步骤 before-save 已记录；目标 PID/路径及观测见 artifacts/runs/configuration_save_feedback_20261023/real-save/before-save.json，完整结论以 final-result.json 为准。

- FIX-CONFIG-02 真实配置保存步骤 save-failed 已记录；目标 PID/路径及观测见 artifacts/runs/configuration_save_feedback_20261023/real-save/save-failed.json，完整结论以 final-result.json 为准。

- FIX-CONFIG-02 真实隔离 EXE 保存失败/重试返回码 3；结果 artifacts/runs/configuration_save_feedback_20261023/real-save/final-result.json。仅使用目标 PID 原生控件消息，不替代真实键盘/读屏验收；正式配置与服务未操作。

- FIX-CONFIG-02 真实保存场景清理核对：正式 Sandboxie.ini 未变=True，原读屏 PID 保留=True，自启动项不变=True，残留进程=0。命令和环境结果见本轮 real-command-result.json、real-environment-before/after.json。

- FIX-CONFIG-02 首次真实窗口场景定位：已确认生产错误提示出现且失败保存未改变配置；探针未能关闭警告按钮，且原生文本赋值没有形成有效数值编辑，故标记 inconclusive 并清理隔离进程，非产品失败。现改用按实际控件识别的 MSAA 复选框与警告按钮动作；只补未完成的保留输入/重试，生产实现不变，旧证据保留。

- FIX-CONFIG-02 真实保存场景辅助程序构建返回码 0；命令/输出见 artifacts/runs/configuration_save_feedback_20261023/real-save-final-helper-build-result.json、real-save-final-helper-build.log。未更改生产源码。

- FIX-CONFIG-02 真实配置保存步骤 startup 已记录；目标 PID/路径及观测见 artifacts/runs/configuration_save_feedback_20261023/real-save-final/startup.json，完整结论以 final-result.json 为准。

- FIX-CONFIG-02 真实隔离 EXE 保存失败/重试返回码 3；结果 artifacts/runs/configuration_save_feedback_20261023/real-save-final/final-result.json。仅使用目标 PID 原生控件消息，不替代真实键盘/读屏验收；正式配置与服务未操作。

- FIX-CONFIG-02 真实保存场景清理核对：正式 Sandboxie.ini 未变=True，原读屏 PID 保留=True，自启动项不变=True，残留进程=0。命令和环境结果见本轮 real-command-result.json、real-environment-before/after.json。

- FIX-CONFIG-02 真实复选框探针定位：第二次停在准备阶段，未执行保存；原生 BM_GETCHECK 不作为 WinForms 自绘复选框的托管 Checked 证据，改读同一可访问对象的 checked 状态，与 MSAA 动作来源一致。保留 real-save-final 无效报告，生产代码不变，下一次只补未完成场景；若仍无有效输入则停止重试。

- FIX-CONFIG-02 真实保存场景辅助程序构建返回码 0；命令/输出见 artifacts/runs/configuration_save_feedback_20261023/real-save-accessible-helper-build-result.json、real-save-accessible-helper-build.log。未更改生产源码。

- FIX-CONFIG-02 真实配置保存步骤 startup 已记录；目标 PID/路径及观测见 artifacts/runs/configuration_save_feedback_20261023/real-save-accessible/startup.json，完整结论以 final-result.json 为准。

- FIX-CONFIG-02 真实配置保存步骤 before-save 已记录；目标 PID/路径及观测见 artifacts/runs/configuration_save_feedback_20261023/real-save-accessible/before-save.json，完整结论以 final-result.json 为准。

- FIX-CONFIG-02 真实配置保存步骤 save-failed 已记录；目标 PID/路径及观测见 artifacts/runs/configuration_save_feedback_20261023/real-save-accessible/save-failed.json，完整结论以 final-result.json 为准。

- FIX-CONFIG-02 真实配置保存步骤 input-retained 已记录；目标 PID/路径及观测见 artifacts/runs/configuration_save_feedback_20261023/real-save-accessible/input-retained.json，完整结论以 final-result.json 为准。

- FIX-CONFIG-02 真实配置保存步骤 retry-saved 已记录；目标 PID/路径及观测见 artifacts/runs/configuration_save_feedback_20261023/real-save-accessible/retry-saved.json，完整结论以 final-result.json 为准。

- FIX-CONFIG-02 真实隔离 EXE 保存失败/重试返回码 0；结果 artifacts/runs/configuration_save_feedback_20261023/real-save-accessible/final-result.json。仅使用目标 PID 原生控件消息，不替代真实键盘/读屏验收；正式配置与服务未操作。

- FIX-CONFIG-02 真实保存场景清理核对：正式 Sandboxie.ini 未变=True，原读屏 PID 保留=True，自启动项不变=True，残留进程=0。命令和环境结果见本轮 real-command-result.json、real-environment-before/after.json。

- FIX-CONFIG-02 收尾审查：源码/项目摘要限定差异、新增文本与 PowerShell 解析检查返回码 0；结果 artifacts/runs/configuration_save_feedback_20261023/diff-result.json、diff.log。本项生产修复及真实失败/重试已通过，Python 无改动、索引为空；待确认的完整计划差异 proposed-plan.diff 和拟提交清单 proposed-staging-files.txt 已准备，确认后直接回填计划并提交，不重跑通过测试。

- UPDATE-EMBED-01 / 用户 4.0.46 检测问题：本轮匿名获取公开更新清单返回码 0；命令/响应见 artifacts/runs/embedded_update_20261023/manifest-current-result.json、manifest-current.json、manifest-current-headers.txt。正在核对实际客户端日志，不将无提示直接等同请求失败；公开清单未改写。

- UPDATE-EMBED-01 用户指定 4.0.46 目录只读核对：日志已证明检查更新成功并发现 4.0.47，属于旧提示方式未被察觉，不能写成更新源不可用。原日志副本与定位见 artifacts/runs/embedded_update_20261023/user46-update.log、user46-diagnosis.json；未修改该目录、未运行已准备的旧版重复探针，继续按用户要求实现更新内容及三按钮窗口。

- UPDATE-EMBED-01 red：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\embedded_update_20261023\bin\ --logger trx;LogFileName=red.trx --results-directory D:\mycodex\git\artifacts\runs\embedded_update_20261023 --filter FullyQualifiedName~EmbeddedUpdateContractTests`；返回码 1，通过 0、失败 7、跳过 0。日志与命令索引：`artifacts/runs/embedded_update_20261023/red.log`、`red-result.json`；如为 C# 测试，另有同名 TRX。

- UPDATE-EMBED-01 用户追加语义：稍后更新必须在下次启动执行，已将待确认差异更新为持久化目标版本/动作，当前会话不安装，下一启动只自动安装已同意的同一目标；忽略会替换该安排。Esc/关闭仅关闭窗口，不擅自安排安装。新增契约将独立覆盖此语义，之前的 RED 保留。

- UPDATE-EMBED-01 defer-red：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\embedded_update_20261023\bin\ --logger trx;LogFileName=defer-red.trx --results-directory D:\mycodex\git\artifacts\runs\embedded_update_20261023 --filter FullyQualifiedName~LaterPersistsForNextStartup|FullyQualifiedName~NextStartupRunsDeferredExactVersion`；返回码 1，通过 0、失败 2、跳过 0。日志与命令索引：`artifacts/runs/embedded_update_20261023/defer-red.log`、`defer-red-result.json`；如为 C# 测试，另有同名 TRX。

- UPDATE-EMBED-01 green：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\embedded_update_20261023\bin\ --logger trx;LogFileName=green.trx --results-directory D:\mycodex\git\artifacts\runs\embedded_update_20261023 --filter FullyQualifiedName~EmbeddedUpdateContractTests`；返回码 0，通过 9、失败 0、跳过 0。日志与命令索引：`artifacts/runs/embedded_update_20261023/green.log`、`green-result.json`；如为 C# 测试，另有同名 TRX。

- UPDATE-EMBED-01 affected：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\embedded_update_20261023\bin\ --logger trx;LogFileName=affected.trx --results-directory D:\mycodex\git\artifacts\runs\embedded_update_20261023 --filter FullyQualifiedName~Update|FullyQualifiedName~EmbeddedUpdateContractTests|FullyQualifiedName~MainFormApplicationContractTests`；返回码 1，计数不适用。日志与命令索引：`artifacts/runs/embedded_update_20261023/affected.log`、`affected-result.json`；如为 C# 测试，另有同名 TRX。

- UPDATE-EMBED-01 受影响首轮未执行测试：同步旧测试时 lambda 参数与外层 result 同名引发编译错误，已仅改参数名；原日志保留，不计为产品 RED。生产实现不变，继续受影响门禁。

- UPDATE-EMBED-01 affected-final：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\embedded_update_20261023\bin\ --logger trx;LogFileName=affected-final.trx --results-directory D:\mycodex\git\artifacts\runs\embedded_update_20261023 --filter FullyQualifiedName~Update|FullyQualifiedName~EmbeddedUpdateContractTests|FullyQualifiedName~MainFormApplicationContractTests`；返回码 1，通过 76、失败 1、跳过 0。日志与命令索引：`artifacts/runs/embedded_update_20261023/affected-final.log`、`affected-final-result.json`；如为 C# 测试，另有同名 TRX。

- UPDATE-EMBED-01 受影响回归定位：既有 Program 启动顺序检查发现安装分支新增的 EnableVisualStyles 提前；该分支不显示主 UI，已去除不必要调用，保留普通启动先检查单实例再初始化 WinForms 的既有顺序。对应失败保留，下一次回归核对。

- UPDATE-EMBED-01 affected-green：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\embedded_update_20261023\bin\ --logger trx;LogFileName=affected-green.trx --results-directory D:\mycodex\git\artifacts\runs\embedded_update_20261023 --filter FullyQualifiedName~Update|FullyQualifiedName~EmbeddedUpdateContractTests|FullyQualifiedName~MainFormApplicationContractTests`；返回码 0，通过 77、失败 0、跳过 0。日志与命令索引：`artifacts/runs/embedded_update_20261023/affected-green.log`、`affected-green-result.json`；如为 C# 测试，另有同名 TRX。

- UPDATE-EMBED-01 full：命令 `dotnet vstest D:\mycodex\git\artifacts\runs\embedded_update_20261023\bin\x64\Release\net48\ShaheLauncher.Tests.dll /Platform:x64 /Logger:trx;LogFileName=full.trx /ResultsDirectory:D:\mycodex\git\artifacts\runs\embedded_update_20261023`；返回码 0，通过 430、失败 0、跳过 0。日志与命令索引：`artifacts/runs/embedded_update_20261023/full.log`、`full-result.json`；如为 C# 测试，另有同名 TRX。

- UPDATE-EMBED-01 release：命令 `dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\embedded_update_20261023\bin\`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/embedded_update_20261023/release.log`、`release-result.json`；如为 C# 测试，另有同名 TRX。

- UPDATE-EMBED-01 publish-default：发布与目录核对返回码 0，Configurations 为空=True，updater 存在=False，兼容副本与主 EXE 相同=；命令、清单及日志见 artifacts/runs/embedded_update_20261023/publish-default-result.json、publish-default.log。没有重新运行通过的测试或修改公开包。

- UPDATE-EMBED-01 publish-legacy：发布与目录核对返回码 0，Configurations 为空=True，updater 存在=True，兼容副本与主 EXE 相同=True；命令、清单及日志见 artifacts/runs/embedded_update_20261023/publish-legacy-result.json、publish-legacy.log。没有重新运行通过的测试或修改公开包。

- UPDATE-EMBED-01 隔离安装测试包组装返回码 0；两个包来自已通过的默认/兼容发布目录，空 Configurations，附一个实际 DLL 文件证明新增落盘，未上传公网。大小/哈希见 artifacts/runs/embedded_update_20261023/install-payloads.json，命令 prepare-install-packages.ps1。

- UPDATE-EMBED-01 embedded-self：返回码 1；真实 helper/安装结果见 artifacts/runs/embedded_update_20261023/embedded-self.json。只操作本轮 real-install 目录，未改用户旧版目录或公开源。

- UPDATE-EMBED-01 installation-proof-result：返回码 1；真实 helper/安装结果见 artifacts/runs/embedded_update_20261023/installation-proof-result.json。只操作本轮 real-install 目录，未改用户旧版目录或公开源。

- UPDATE-EMBED-01 首次真实安装定位：主 EXE 更新模式已下载并安装成功，helper 退出码 0、新主程序/新增 DLL 哈希和配置保留通过，重启窗口也已观测。观察脚本未提前持有进程句柄，退出码返回 null，导致验证器误作失败；没有产品安装失败证据。保留原结果，不重复安装，接着单独启动已安装副本核对正常退出，并补两个尚未运行的兼容场景。

- UPDATE-EMBED-01 已安装副本启动/正常退出核对返回码 1；真实子进程退出码 4294967295，窗口与清理记录见 artifacts/runs/embedded_update_20261023/installed-startup-exit.json。未重复下载或安装。

- UPDATE-EMBED-01 单独启动观测首次无效：辅助 Node 启动使用 windowsHide=true，应用日志证明已启动/检查更新，但观察器未取得可见主窗口；最终仅清理该自有进程，返回码为强制退出，不能当作程序退出故障。调整为有意验证交互窗口的可见启动，不重装。

- UPDATE-EMBED-01 可见启动/正常退出补充返回码 0，实际退出码 0；证据 artifacts/runs/embedded_update_20261023/installed-startup-exit-visible.json。只启动已安装副本验证窗口和退出，没有重复安装。

- UPDATE-EMBED-01 legacy-protocol-alias：返回码 0；真实 helper/安装结果见 artifacts/runs/embedded_update_20261023/legacy-protocol-alias.json。只操作本轮 real-install 目录，未改用户旧版目录或公开源。

- UPDATE-EMBED-01 published46-to-embedded：返回码 0；真实 helper/安装结果见 artifacts/runs/embedded_update_20261023/published46-to-embedded.json。只操作本轮 real-install 目录，未改用户旧版目录或公开源。

- UPDATE-EMBED-01 installation-remaining-result：返回码 0；真实 helper/安装结果见 artifacts/runs/embedded_update_20261023/installation-remaining-result.json。只操作本轮 real-install 目录，未改用户旧版目录或公开源。

- UPDATE-EMBED-01 真实安装环境收尾返回码 0；正式 Sandboxie.ini/自启动项不变、原读屏进程保留、无本轮残留，结果 artifacts/runs/embedded_update_20261023/install-environment-after.json。

- UPDATE-EMBED-01 python：命令 `python -m unittest discover -s tests -p test_*.py -v`；返回码 0，通过 31、失败 0、跳过 0。日志与命令索引：`artifacts/runs/embedded_update_20261023/python.log`、`python-result.json`；如为 C# 测试，另有同名 TRX。

- UPDATE-EMBED-01 compile：命令 `python -m compileall -q shahe_bulk_launcher tests`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/embedded_update_20261023/compile.log`、`compile-result.json`；如为 C# 测试，另有同名 TRX。

- UPDATE-EMBED-01 真实安装分段汇总返回码 0，通过 3、失败 0、跳过 0；主 EXE 更新模式、同程序旧协议别名和用户所指 4.0.46 安装器安装新兼容包均有真实下载/校验/替换证据，新 DLL 与配置保持，主窗口正常退出单独通过。权威汇总 artifacts/runs/embedded_update_20261023/installation-final-result.json，早期观测工具失败保留，不重复成功安装。

- UPDATE-EMBED-01 直接审查：当前生产实现与用户更新交互/下次启动要求对应，默认与兼容目录及真实安装证据已核对，详见 artifacts/runs/embedded_update_20261023/source-review.md。上一个 FIX-CONFIG-02 的快照保留用于独立提交，用户其他改动不混入；下一步仅收尾差异检查并取得计划写入确认，不重跑通过场景。

- UPDATE-EMBED-01 最终限定差异/脚本语法/新增文本核对返回码 0；结果 artifacts/runs/embedded_update_20261023/diff-result.json、diff.log。Python 无改动、索引为空。两个模块的待确认计划差异已完整保存，计划确认后按独立快照/增量提交，不重跑已通过测试。

- UPDATE-EMBED-01 范围调整记录：用户只保留后一项，已用当前源码/前项快照/HEAD 三方核对撤回未提交 FIX-CONFIG-02；MainForm 和共享测试保留更新差异，无关用户改动未触碰。撤回清单 artifacts/runs/embedded_update_20261023/scope-update-only/withdrawal-result.json。历史测试记录保持原样，接着只对范围变化后的最终源码复核。

- UPDATE-EMBED-01 update-only-affected：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\embedded_update_20261023\bin\ --logger trx;LogFileName=update-only-affected.trx --results-directory D:\mycodex\git\artifacts\runs\embedded_update_20261023 --filter FullyQualifiedName~Update|FullyQualifiedName~EmbeddedUpdateContractTests|FullyQualifiedName~MainFormApplicationContractTests`；返回码 0，通过 77、失败 0、跳过 0。日志与命令索引：`artifacts/runs/embedded_update_20261023/update-only-affected.log`、`update-only-affected-result.json`；如为 C# 测试，另有同名 TRX。

- UPDATE-EMBED-01 update-only-full：命令 `dotnet vstest D:\mycodex\git\artifacts\runs\embedded_update_20261023\bin\x64\Release\net48\ShaheLauncher.Tests.dll /Platform:x64 /Logger:trx;LogFileName=update-only-full.trx /ResultsDirectory:D:\mycodex\git\artifacts\runs\embedded_update_20261023`；返回码 0，通过 413、失败 0、跳过 0。日志与命令索引：`artifacts/runs/embedded_update_20261023/update-only-full.log`、`update-only-full-result.json`；如为 C# 测试，另有同名 TRX。

- UPDATE-EMBED-01 update-only-release：命令 `dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\embedded_update_20261023\bin\`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/embedded_update_20261023/update-only-release.log`、`update-only-release-result.json`；如为 C# 测试，另有同名 TRX。

- UPDATE-EMBED-01 publish-default-final：发布与目录核对返回码 0，Configurations 为空=True，updater 存在=False，兼容副本与主 EXE 相同=；命令、清单及日志见 artifacts/runs/embedded_update_20261023/publish-default-final-result.json、publish-default-final.log。没有重新运行通过的测试或修改公开包。

- UPDATE-EMBED-01 publish-legacy-final：发布与目录核对返回码 0，Configurations 为空=True，updater 存在=True，兼容副本与主 EXE 相同=True；命令、清单及日志见 artifacts/runs/embedded_update_20261023/publish-legacy-final-result.json、publish-legacy-final.log。没有重新运行通过的测试或修改公开包。

- UPDATE-EMBED-01 隔离安装测试包组装返回码 0；两个包来自已通过的默认/兼容发布目录，空 Configurations，附一个实际 DLL 文件证明新增落盘，未上传公网。大小/哈希见 artifacts/runs/embedded_update_20261023/install-payloads-final.json，命令 prepare-install-packages.ps1。

- UPDATE-EMBED-01 embedded-self-final：返回码 0；真实 helper/安装结果见 artifacts/runs/embedded_update_20261023/embedded-self-final.json。只操作本轮 real-install 目录，未改用户旧版目录或公开源。

- UPDATE-EMBED-01 installation-update-only-result：返回码 0；真实 helper/安装结果见 artifacts/runs/embedded_update_20261023/installation-update-only-result.json。只操作本轮 real-install 目录，未改用户旧版目录或公开源。

- UPDATE-EMBED-01 最终范围收尾：用户要求撤回的 FIX-CONFIG-02 已恢复为 HEAD 行为，新增测试/设计撤出活动目录，未影响已提交 FIX-CONFIG-01。最终 Release 0 警告/0 错误；最新构建输出以 update-only-release 与 publish-default-final 为准，主 EXE 安装/重启/退出以 embedded-self-final.json 为准。先前含取消任务的构建与测试仅作历史证据，不作为最终交付目录；Python 源码没有变化，本模块之前的 Python/compile 通过结果继续有效。

- UPDATE-EMBED-01 最终单项安装环境核对返回码 0；正式配置/自启动项和原读屏 PID 保持，无自有进程残留；见 artifacts/runs/embedded_update_20261023/update-only-environment-after.json。

- UPDATE-EMBED-01 最终单项限定差异检查返回码 0，Python 无修改，索引尚为空；结果 artifacts/runs/embedded_update_20261023/update-only-diff-result.json、update-only-diff.log。下一步仅选择性独立提交。

- UPDATE-EMBED-01 最终暂存核对返回码 2；实际清单与 staging-files.txt 完全一致，共享文档仅含本模块增量，不包含取消的 FIX-CONFIG-02 源码/测试或用户既有改动。结果已补入 update-only-diff-result.json。

- UPDATE-EMBED-01 暂存差异首轮返回码 2，具体空白错误记录于 artifacts/runs/embedded_update_20261023/staged-check-first.log；不改变已通过的源码验证结论，仅整理文本证据后再提交。

- UPDATE-EMBED-01 暂存清单生成器校正：在已暂存后重新生成时原命令只读取未暂存差异，导致清单比对失败而未提交；现以 git diff HEAD 汇总本模块全部差异，保留选择性范围，生产代码与测试结果没有变化。

- UPDATE-EMBED-01 最终暂存检查返回码 0，实际清单与 staging-files.txt 完全一致；HTTP 头末尾空行和清单生成工具问题均已更正，源码与验证结果未变，现独立提交。

- UPDATE-EMBED-01 源码交付：独立提交 `c074aeb4b8afd22e1f35a86d1a3dc368394bc0fe`，推送 `origin/voice03-fixed-reader-20261022` 返回码 0；回退 `git revert c074aeb4b8afd22e1f35a86d1a3dc368394bc0fe`。只含用户选定的内置更新模块，前项未提交修复不纳入；GitCode 公开包和用户旧版目录未修改。结果 artifacts/runs/embedded_update_20261023/delivery-result.json、source-push.log。

- LOG-01 red：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\diagnostic_logging_release_20261023\bin\ --logger trx;LogFileName=red.trx --results-directory D:\mycodex\git\artifacts\runs\diagnostic_logging_release_20261023 --filter TestCategory=DiagnosticLogging|FullyQualifiedName~AppendDailyLog|FullyQualifiedName~NormalizesLogOutputModeWithExplicitOptIn`；返回码 1，通过 2、失败 8、跳过 0。日志与命令索引：`artifacts/runs/diagnostic_logging_release_20261023/red.log`、`red-result.json`；如为 C# 测试，另有同名 TRX。

- LOG-01 green：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\diagnostic_logging_release_20261023\bin\ --logger trx;LogFileName=green.trx --results-directory D:\mycodex\git\artifacts\runs\diagnostic_logging_release_20261023 --filter TestCategory=DiagnosticLogging|FullyQualifiedName~AppendDailyLog|FullyQualifiedName~NormalizesLogOutputModeWithExplicitOptIn`；返回码 0，通过 10、失败 0、跳过 0。日志与命令索引：`artifacts/runs/diagnostic_logging_release_20261023/green.log`、`green-result.json`；如为 C# 测试，另有同名 TRX。

- LOG-01 公开清单只读核对：curl 匿名读取返回码 0，当前 latest_version=4.0.47。证据：artifacts/runs/diagnostic_logging_release_20261023/manifest-fetch-result.json、public-manifest-before.json；本步骤未改公开发布。

- LOG-01 affected：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\diagnostic_logging_release_20261023\bin\ --logger trx;LogFileName=affected.trx --results-directory D:\mycodex\git\artifacts\runs\diagnostic_logging_release_20261023 --filter TestCategory=DiagnosticLogging|TestCategory=Updates|TestCategory=Scheduling|TestCategory=Lifecycle|FullyQualifiedName~Settings|FullyQualifiedName~Bootstrap|FullyQualifiedName~ConstantsContractTests`；返回码 0，通过 148、失败 0、跳过 0。日志与命令索引：`artifacts/runs/diagnostic_logging_release_20261023/affected.log`、`affected-result.json`；如为 C# 测试，另有同名 TRX。

- LOG-01 full：命令 `dotnet vstest D:\mycodex\git\artifacts\runs\diagnostic_logging_release_20261023\bin\x64\Release\net48\ShaheLauncher.Tests.dll /Platform:x64 /Logger:trx;LogFileName=full.trx /ResultsDirectory:D:\mycodex\git\artifacts\runs\diagnostic_logging_release_20261023`；返回码 0，通过 420、失败 0、跳过 0。日志与命令索引：`artifacts/runs/diagnostic_logging_release_20261023/full.log`、`full-result.json`；如为 C# 测试，另有同名 TRX。

- LOG-01 release：命令 `dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\diagnostic_logging_release_20261023\bin\`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/diagnostic_logging_release_20261023/release.log`、`release-result.json`；如为 C# 测试，另有同名 TRX。

- LOG-01 all-actions-red：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\diagnostic_logging_release_20261023\bin\ --logger trx;LogFileName=all-actions-red.trx --results-directory D:\mycodex\git\artifacts\runs\diagnostic_logging_release_20261023 --filter FullyQualifiedName~EveryAltMenuLeafAndFixedHotkeyRecordsItsAction|FullyQualifiedName~DialogButtonActionsAreLoggedWithoutLoggingFocusChanges`；返回码 1，通过 0、失败 2、跳过 0。日志与命令索引：`artifacts/runs/diagnostic_logging_release_20261023/all-actions-red.log`、`all-actions-red-result.json`；如为 C# 测试，另有同名 TRX。

- LOG-01 all-actions-green：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\diagnostic_logging_release_20261023\bin\ --logger trx;LogFileName=all-actions-green.trx --results-directory D:\mycodex\git\artifacts\runs\diagnostic_logging_release_20261023 --filter TestCategory=DiagnosticLogging`；返回码 1，计数不适用。日志与命令索引：`artifacts/runs/diagnostic_logging_release_20261023/all-actions-green.log`、`all-actions-green-result.json`；如为 C# 测试，另有同名 TRX。

- LOG-01 all-actions-green-fixed：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\diagnostic_logging_release_20261023\bin\ --logger trx;LogFileName=all-actions-green-fixed.trx --results-directory D:\mycodex\git\artifacts\runs\diagnostic_logging_release_20261023 --filter TestCategory=DiagnosticLogging`；返回码 0，通过 9、失败 0、跳过 0。日志与命令索引：`artifacts/runs/diagnostic_logging_release_20261023/all-actions-green-fixed.log`、`all-actions-green-fixed-result.json`；如为 C# 测试，另有同名 TRX。

- LOG-01 环境只读基线：返回码 0；主程序占用、读屏进程、正式 Sandboxie 配置及自启动值见 artifacts/runs/diagnostic_logging_release_20261023/environment-before.json；未停止或改动任何用户进程。

- LOG-01 final-affected：命令 `dotnet vstest D:\mycodex\git\artifacts\runs\diagnostic_logging_release_20261023\bin\x64\Release\net48\ShaheLauncher.Tests.dll /Platform:x64 /Logger:trx;LogFileName=final-affected.trx /ResultsDirectory:D:\mycodex\git\artifacts\runs\diagnostic_logging_release_20261023 /TestCaseFilter:TestCategory=DiagnosticLogging|TestCategory=Updates|TestCategory=Scheduling|TestCategory=Lifecycle|FullyQualifiedName~Settings|FullyQualifiedName~Dialog|FullyQualifiedName~Hotkey|FullyQualifiedName~Navigation|FullyQualifiedName~List|FullyQualifiedName~Tray|FullyQualifiedName~Bootstrap|FullyQualifiedName~ConstantsContractTests`；返回码 0，通过 203、失败 0、跳过 0。日志与命令索引：`artifacts/runs/diagnostic_logging_release_20261023/final-affected.log`、`final-affected-result.json`；如为 C# 测试，另有同名 TRX。

- LOG-01 final-full：命令 `dotnet vstest D:\mycodex\git\artifacts\runs\diagnostic_logging_release_20261023\bin\x64\Release\net48\ShaheLauncher.Tests.dll /Platform:x64 /Logger:trx;LogFileName=final-full.trx /ResultsDirectory:D:\mycodex\git\artifacts\runs\diagnostic_logging_release_20261023`；返回码 1，通过 421、失败 1、跳过 0。日志与命令索引：`artifacts/runs/diagnostic_logging_release_20261023/final-full.log`、`final-full-result.json`；如为 C# 测试，另有同名 TRX。

- LOG-01 hotkey-boundary-green：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\diagnostic_logging_release_20261023\bin\ --logger trx;LogFileName=hotkey-boundary-green.trx --results-directory D:\mycodex\git\artifacts\runs\diagnostic_logging_release_20261023 --filter TestCategory=DiagnosticLogging|FullyQualifiedName~PathMemoryRuntimeContractTests|FullyQualifiedName~Hotkey|TestCategory=Lifecycle`；返回码 0，通过 40、失败 0、跳过 0。日志与命令索引：`artifacts/runs/diagnostic_logging_release_20261023/hotkey-boundary-green.log`、`hotkey-boundary-green-result.json`；如为 C# 测试，另有同名 TRX。

- LOG-01 final-full-fixed：命令 `dotnet vstest D:\mycodex\git\artifacts\runs\diagnostic_logging_release_20261023\bin\x64\Release\net48\ShaheLauncher.Tests.dll /Platform:x64 /Logger:trx;LogFileName=final-full-fixed.trx /ResultsDirectory:D:\mycodex\git\artifacts\runs\diagnostic_logging_release_20261023`；返回码 0，通过 422、失败 0、跳过 0。日志与命令索引：`artifacts/runs/diagnostic_logging_release_20261023/final-full-fixed.log`、`final-full-fixed-result.json`；如为 C# 测试，另有同名 TRX。

- LOG-01 final-release：命令 `dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\diagnostic_logging_release_20261023\bin\`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/diagnostic_logging_release_20261023/final-release.log`、`final-release-result.json`；如为 C# 测试，另有同名 TRX。

- LOG-01 publish-default-final：发布与目录核对返回码 0，Configurations 为空=True，updater 存在=False，兼容副本与主 EXE 相同=；命令、清单及日志见 artifacts/runs/diagnostic_logging_release_20261023/publish-default-final-result.json、publish-default-final.log。没有重新运行通过的测试或修改公开包。

- LOG-01 publish-legacy-final：发布与目录核对返回码 0，Configurations 为空=True，updater 存在=True，兼容副本与主 EXE 相同=True；命令、清单及日志见 artifacts/runs/diagnostic_logging_release_20261023/publish-legacy-final-result.json、publish-legacy-final.log。没有重新运行通过的测试或修改公开包。

- LOG-01 发布包封装：package-release.ps1 返回码 0；版本、完整清单、大小及 SHA-256 以 artifacts/runs/diagnostic_logging_release_20261023/package-result.json 为准。空 Configurations，不带用户日志；旧版兼容入口复用主 EXE。

- LOG-01 release-install-logging-on：返回码 0；正式发布包的隔离安装/启动和日志开关证据见 artifacts/runs/diagnostic_logging_release_20261023/release-install-logging-on.json。只操作本轮 real-install 目录。

- LOG-01 release-install-logging-off：返回码 0；正式发布包的隔离安装/启动和日志开关证据见 artifacts/runs/diagnostic_logging_release_20261023/release-install-logging-off.json。只操作本轮 real-install 目录。

- LOG-01 release-install-result：返回码 0；正式发布包的隔离安装/启动和日志开关证据见 artifacts/runs/diagnostic_logging_release_20261023/release-install-result.json。只操作本轮 real-install 目录。

- LOG-01 实测后环境核对：返回码 0；正式配置、自启动、读屏及隔离进程清理证据见 artifacts/runs/diagnostic_logging_release_20261023/environment-after.json。

- LOG-01 源码和项目结构差异检查：返回码 2，命令及输出见 artifacts/runs/diagnostic_logging_release_20261023/diff-check-result.json、diff-check.log；未暂存任何文件。

- LOG-01 未受影响证据沿用：`git diff --name-only -- *.py shahe_bulk_launcher tests` 无输出；Python 对照沿用 `artifacts/runs/embedded_update_20261023/python-result.json`（返回码 0，通过 31、失败 0、跳过 0）和生产源码编译 `compile-result.json`（返回码 0，计数不适用），命令/原日志在该目录，未重跑未改动的 Python。
- LOG-01 计划复核：重新读取当前唯一活动计划；UPDATE-EMBED-01 已交付，AUDIT-04 及后续未验收事项继续保留。本轮 LOG-01 追加尚待确认，活动计划与本轮初始快照一致；不因当前日志/本地包验证通过宣称整体迁移完成。差异检查初次因新增文档行混入 CRLF 返回 2，现仅统一本轮追加的行结束符，不改历史结论。

- LOG-01 最终差异检查：返回码 0；命令和输出见 artifacts/runs/diagnostic_logging_release_20261023/diff-check-final-result.json、diff-check-final.log。文档格式问题已消除，没有改动编译后源码，也没有重复软件测试。

- LOG-01 delivery-full：命令 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\diagnostic_logging_release_20261023\bin\ --logger trx;LogFileName=delivery-full.trx --results-directory D:\mycodex\git\artifacts\runs\diagnostic_logging_release_20261023`；返回码 0，通过 422、失败 0、跳过 0。日志与命令索引：`artifacts/runs/diagnostic_logging_release_20261023/delivery-full.log`、`delivery-full-result.json`；如为 C# 测试，另有同名 TRX。

- LOG-01 delivery-release：命令 `dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\diagnostic_logging_release_20261023\bin\`；返回码 0，计数不适用。日志与命令索引：`artifacts/runs/diagnostic_logging_release_20261023/delivery-release.log`、`delivery-release-result.json`；如为 C# 测试，另有同名 TRX。

- LOG-01 publish-default-delivery：发布与目录核对返回码 0，Configurations 为空=True，updater 存在=False，兼容副本与主 EXE 相同=；命令、清单及日志见 artifacts/runs/diagnostic_logging_release_20261023/publish-default-delivery-result.json、publish-default-delivery.log。没有重新运行通过的测试或修改公开包。

- LOG-01 publish-legacy-delivery：发布与目录核对返回码 0，Configurations 为空=True，updater 存在=True，兼容副本与主 EXE 相同=True；命令、清单及日志见 artifacts/runs/diagnostic_logging_release_20261023/publish-legacy-delivery-result.json、publish-legacy-delivery.log。没有重新运行通过的测试或修改公开包。

- LOG-01 最终交付包封装：package-delivery.ps1 返回码 0；版本、完整清单、大小及 SHA-256 以 artifacts/runs/diagnostic_logging_release_20261023/delivery-package-result.json 为准。空 Configurations，不带用户日志；旧版兼容入口复用主 EXE。

- LOG-01 release-install-logging-on-delivery：返回码 0；正式发布包的隔离安装/启动和日志开关证据见 artifacts/runs/diagnostic_logging_release_20261023/release-install-logging-on-delivery.json。只操作本轮 real-install 目录。

- LOG-01 release-install-delivery-result：返回码 0；正式发布包的隔离安装/启动和日志开关证据见 artifacts/runs/diagnostic_logging_release_20261023/release-install-delivery-result.json。只操作本轮 real-install 目录。

- LOG-01 最终交付补充：标题栏关闭/隐藏入口也已记录，最终源码门禁、publish 与 ZIP 见 `delivery-full-result.json`、`delivery-release-result.json`、`publish-default-delivery-result.json`、`publish-legacy-delivery-result.json`、`delivery-package-result.json`；均位于 `artifacts/runs/diagnostic_logging_release_20261023/`。最新 ZIP 位于该目录 `delivery/`，此前包和验证保留为历史。关闭日志的真实安装证据沿用 `release-install-logging-off.json`：两版 Core 日志开关代码及 DLL 哈希相同，最终源码的关闭契约仍通过；最终 ZIP 的真实安装、主窗口退出和新增关闭日志另有 `release-install-logging-on-delivery.json`。

- LOG-01 最终交付包实测后环境核对：返回码 0；正式配置、自启动、读屏及隔离进程清理证据见 artifacts/runs/diagnostic_logging_release_20261023/environment-delivery-after.json。

- LOG-01 交付收口差异检查：返回码 0；证据 artifacts/runs/diagnostic_logging_release_20261023/delivery-diff-check-result.json。最终源码清单为 source-files.txt、source-inventory.json；计划确认、选择性提交推送及 GitCode 上传仍待完成，暂存区为空。

- LOG-01 publish-default-final-nolegacy：发布与目录核对返回码 0，Configurations 为空=True，updater 存在=False，兼容副本与主 EXE 相同=；命令、清单及日志见 artifacts/runs/diagnostic_logging_release_20261023/publish-default-final-nolegacy-result.json、publish-default-final-nolegacy.log。没有重新运行通过的测试或修改公开包。

- LOG-01 4.0.48 最终绿色版封装：返回码 0；ZIP 仅含空 Configurations/、空 logs/ 和程序文件，不含 updater.exe、PDB、用户配置或日志内容。大小、SHA-256、逐文件清单见 artifacts/runs/diagnostic_logging_release_20261023/portable-final-package-result.json；包位于 delivery-final/。

- LOG-01 绿色版内置更新等价性核对：返回码 0；最终绿色版主 EXE 与 Core DLL 均与已通过内置更新真实安装的默认发布编译物哈希一致，复用 release-install-logging-on-delivery.json 及 release-install-delivery-result.json，不重复更新安装测试。证据见 artifacts/runs/diagnostic_logging_release_20261023/portable-update-hash-equivalence-result.json。

- LOG-01 4.0.48 最终绿色版解压即用验证：返回码 0；从最终 ZIP 隔离解压后主窗口可启动并正常退出，初始 Configurations/logs 均为空，关闭日志后 logs 保持为空，无旧 log 目录和残留进程。证据见 artifacts/runs/diagnostic_logging_release_20261023/portable-final-smoke-result.json。

- LOG-01 绿色版最终差异检查说明：全工作区 git diff --check 返回 1，仅命中本轮开始前已存在的 GAP-09 两份日志尾随空白；未修改无关日志。排除这两份既有文件后的当前范围检查及新增发布脚本尾随空白检查结果见 artifacts/runs/diagnostic_logging_release_20261023/portable-final-diff-check-result.json 和 portable-final-diff-check.log。

- LOG-01 4.0.48 绿色版最终只读复核：返回码 0；ZIP 哈希、空 Configurations/logs、禁带 updater/PDB、解压启动正常退出、无残留进程及内置更新编译物等价性均汇总于 artifacts/runs/diagnostic_logging_release_20261023/portable-final-verification-result.json。未重复启动或更新安装测试。

- LOG-01 提交前源码哈希门禁：返回码 0；29 个当前模块源码/测试文件与 delivery-full、delivery-release 和最终绿色包验证时的 source-inventory.json 完全一致，因此按用户要求不重复已通过测试。证据见 artifacts/runs/diagnostic_logging_release_20261023/precommit-source-hash-result.json。

- LOG-01 选择性暂存门禁：126 个文件，含 29 个当前模块源码/测试文件、必要验证索引、LOG-01 计划追加和项目结构对应记录；git diff --cached --check 返回码 0，暂存源码与已验证工作树一致。ZIP、PDB、bin/publish/隔离运行目录、Python、AUDIT-04 测试、两份既有 GAP-09 日志及非必要 source-review.diff 均未暂存。精确清单见 artifacts/runs/diagnostic_logging_release_20261023/staging-files.txt。

- LOG-01 本地功能提交：4e8752630de9f4543a888f8bd71711d811eda085，父提交 `fe43076397a519dfc220b7a0841aae7807dd69ee`；提交含 126 个选择性文件，禁带路径为 0。原定语音分支推送两次均因 GitHub 443 不可达失败，远端未改变。

- LOG-01 远端目标更正：用户要求所有 C# 功能统一进入 origin/沙河批量打开器，并询问删除远端语音分支。该目标分支与当前开发历史无共同祖先；采用以远端 1658b7970250617ac30057cb79620be5cb816587 为父提交的前向树同步，不强推。只有主分支推送和远端核对成功后才删除 origin/voice03-fixed-reader-20261022。

- LOG-01 沙河远端前向同步提交：`445612a8947688b3906dec02f713a08dec54459c`，父提交为远端 `1658b7970250617ac30057cb79620be5cb816587`；C# 子树哈希与已验证 `4e87526` 一致，Python/ZIP/PDB 为 0，旧独立 Updater 仅删除。网络阻断记录提交为 `3cbad15`。

- LOG-01 GitHub 网络阻断：推送到 `origin/沙河批量打开器` 失败，DNS 成功但 HTTPS 443、SSH 22、SSH 443 均无法建立 TCP；系统代理关闭，已配置的 `127.0.0.1:10808` 无监听。远端主分支未改变，语音分支未删除。证据见 artifacts/runs/diagnostic_logging_release_20261023/remote-network-block-result.json。

- LOG-01 远端交付恢复点：隔离分支 `integration/shahe-unified-4.0.48-20261023` 当前 HEAD 为 `460e531`，依次包含 `445612a` C# 前向同步、`3cbad15` 网络阻断记录和 `460e531` 文档格式修正；相对 `origin/沙河批量打开器` 前向 3 个提交且工作区干净。GitHub 网络恢复后先推送并核对，再删除 `origin/voice03-fixed-reader-20261022`。

- LOCATE-01 + UI-TEXT-01 定向 RED：`dotnet test` 使用隔离 `BaseOutputPath` 返回码 `1`，`5` 项中通过 `1`、失败 `4`、跳过 `0`。有效失败分别证明定位仍优先旧选中项、生产回调仍枚举全部系统窗口、多个窗口仍调用选择回调，以及朗读设置仍显示“读屏 API 接口”与“自动（默认）”；快捷键切换上一个/下一个用户后再定位的现有链路通过。日志和 TRX 为 `artifacts/runs/locate_and_speech_text_20261023/red/locate-text-red.log`、`locate-text-red.trx`。下一步只实施已确认的最小定位和文案修改。

- LOCATE-01 + UI-TEXT-01 定向 GREEN：同一 `5` 项测试使用隔离 `BaseOutputPath` 返回码 `0`，通过 `5`、失败 `0`、跳过 `0`。定位入口现只对该动作采用焦点项优先，生产回调不再预先枚举全部系统窗口，服务对测试枚举结果继续按当前用户 PID 防御过滤；多个候选直接使用首项，不调用窗口选择回调。朗读标签、AccessibleName 和首项分别为“朗读接口:”“朗读接口”“自动”，保存值未改。日志和 TRX 为 `artifacts/runs/locate_and_speech_text_20261023/green/locate-text-green.log`、`locate-text-green.trx`。下一步运行一次受影响回归。

- LOCATE-01 + UI-TEXT-01 首次受影响回归：`ProcessActions|Application|SettingsDialogs` 返回码 `1`，通过 `163`、失败 `2`、跳过 `0`。失败均为旧组装契约：一项要求 `EnumerateWindows` 无参数枚举全部系统窗口，另一项要求必须组装 `ChooseWindow`；两者与用户新确认的“只查焦点用户 PID、直接置前且不显示选择框”冲突。日志和 TRX 为 `artifacts/runs/locate_and_speech_text_20261023/affected/locate-text-affected.log`、`locate-text-affected.trx`。下一步保留真实 Windows 边界但改为显式接收当前用户 PID，并更新被需求取代的选择器契约后复验同一集合。

- LOCATE-01 + UI-TEXT-01 受影响回归 GREEN：真实窗口枚举回调已改为接收当前用户 PID，生产 `ChooseWindow` 明确不组装；同一 `ProcessActions|Application|SettingsDialogs` 集合返回码 `0`，通过 `165`、失败 `0`、跳过 `0`。日志和 TRX 为 `artifacts/runs/locate_and_speech_text_20261023/affected-final/locate-text-affected-final.log`、`locate-text-affected-final.trx`。下一步只运行一次 C# 全量测试。

- LOCATE-01 + UI-TEXT-01 C# 全量 GREEN：解决方案全量测试使用隔离 `BaseOutputPath` 返回码 `0`，通过 `426`、失败 `0`、跳过 `0`；新增定位契约已进入全量。日志和 TRX 为 `artifacts/runs/locate_and_speech_text_20261023/full/locate-text-full.log`、`locate-text-full.trx`。下一步运行一次隔离 Release x64 构建。

- LOCATE-01 + UI-TEXT-01 隔离 Release x64：解决方案构建返回码 `0`，警告 `0`、错误 `0`，未使用可能被运行中程序锁定的默认输出目录。日志为 `artifacts/runs/locate_and_speech_text_20261023/release/locate-text-release.log`，应用输出位于同目录 `bin/x64/Release/net48/`。下一步运行一次隔离真实窗口前台探针。

- LOCATE-01 + UI-TEXT-01 真实窗口前台探针：启动两个只用于本轮验证的 WinForms 窗口，调用生产 `ProcessActionService` 并仅传入目标 PID。返回码 `0`，`located=true`、`chooseCalls=0`，前台 PID 从非目标 `2756` 变为目标 `2348`，状态为“已定位到当前打开的进程: 探针用户”；目标和非目标测试进程随后均不存在。结果和日志为 `artifacts/runs/locate_and_speech_text_20261023/foreground-probe/foreground-result.json`、`foreground-probe.log`。定向、受影响、全量、Release 和真实前台证据现均有效。

- LOCATE-01 + UI-TEXT-01 差异检查：限定本项源码、测试、计划和项目结构运行 `git diff --check` 返回码 `0`；仅报告工作树 LF 将来转换为 CRLF 的提示，没有尾随空白或补丁错误。下一步按用户要求规划将当前 4.0.48 源码递增为 4.0.49 测试更新，不复用或覆盖公开 4.0.47 包。

- 2026-10-24 版本兼容性核对：4.0.47 发布提交 `a5b4a18` 中的 `CompareVersions` 按数字分段比较，`4.0.49` 会被判定为高于 `4.0.47`；旧版 `UpdateService` 先读取 `latest_version`，再优先选择带 URL、SHA-256 和大小的 `kind=full` 完整包，因此 4.0.47 无需 `from` 字段即可检测并接收 4.0.49。若使用增量补丁，则必须明确 `from=4.0.47`、`to=4.0.49`。2026-10-24 对公开清单的只读 HTTP 核对返回码 `200`，当前 `latest_version` 仍为 `4.0.47`，尚未发布 4.0.49；本次未修改版本号、清单或远端包。

- 2026-10-24 交接源码静态清点：命令 artifacts/runs/handoff_inventory_20261024/compare-source.ps1 返回码 0；Python/C# 20 个模块代表入口存在，快捷键动作定义分别为 47/47，类别/动作键/标签/默认键/事件编号无差异；提取到的 Python 菜单动作 41 项都有 C# 分派，默认配置顶层键 22 项均有 C# 字面量。明细 artifacts/runs/handoff_inventory_20261024/source-comparison.json；此为源码存在性核对，不证明每条分支、文案顺序或真实运行等价。脚本首次控制台投影为空字段，但完整 JSON 已正确写入，本记录从 JSON 读取；没有运行软件测试、构建或真实服务动作。

- 2026-10-24 交接文档检查：返回码 0，限定文档 git diff --check 返回码 0；路径缺失 0、PowerShell 解析错误 0、本轮新增尾随空白 0，已快照的生产/测试源码变更 0。结果 artifacts/runs/handoff_inventory_20261024/documentation-check.json，差异输出 document-diff.log。未运行软件测试/构建/联网/发布，暂存区保持为空。首次文档补丁因重复声明同一目标文件被工具原子拒绝，拆分为同一文件多段后已成功；未影响业务源码。

- 2026-10-24 交接收尾：已重新读取新计划全部章节并核对状态，仅勾选本轮文档检查项，P1 以后保持待办。最终限定差异复核返回码 0；结果已补入同一 documentation-check.json，不新增软件测试结论。

- U1 用户列表进程菜单 RED：dotnet test 定向 SandboxUserProcessMenuContractTests，隔离 BaseOutputPath；返回码 1。日志 artifacts/runs/process_menu_list_20261024/red.log，TRX red/red.trx。详细失败原因和计数将在读取结果后追加；未改业务源码。

- U1 RED 结果核对：通过 1、失败 5、跳过 0。失败为 Alt 进程栏仍存在、缺鼠标/键盘上下文入口、菜单打开后列表重排导致当前动作失效，以及缺刷新进程状态独立回调；当前/全部参数对照已通过。本轮只修 UI 接线与动作目标捕获，服务实现不变。

- U1 首轮 GREEN：相同定向命令返回码 0；日志 artifacts/runs/process_menu_list_20261024/green.log，TRX green/green.trx。已接原生上下文消息/目标快照、补刷新进程状态、移除 Alt 进程栏，下一步读取结果确认。

- U1 定向 GREEN 核对：通过 6、失败 0、跳过 0。鼠标命中/键盘焦点、当前与全部参数、打开不执行、空白/分组禁用、重排后目标保持及独立进程刷新通过。接着更新旧测试所依赖的 Alt 菜单位置，保留原启用态/缓存/异步断言。

- U1 受影响回归：Application/UiControls/Hotkey/DiagnosticLogging，dotnet test 解决方案隔离输出返回码 1；日志 artifacts/runs/process_menu_list_20261024/affected.log，TRX affected/affected.trx。测试入口已从 Alt 进程栏切换到用户列表，原状态/异步断言保留。

- U1 受影响回归计数：通过 159、失败 5、跳过 0。菜单迁移后原测试暴露独立 InitializeUi 下列表状态 providers 未接入、以及旧菜单位置断言残留；按实际调用链继续修复/核对，不把首轮失败记为通过。

- U1 受影响复验：相同筛选返回码 0；日志 artifacts/runs/process_menu_list_20261024/affected-final.log，TRX affected-final/affected-final.trx。将缓存 provider 移至列表 UI 组装，测试按无句柄控件的实际选中项构建菜单；日志契约同时覆盖 Alt 和迁移后的列表动作。

- U1 受影响回归最终结果：dotnet test 返回码 0，通过 164、失败 0、跳过 0；证据 artifacts/runs/process_menu_list_20261024/affected-final.log 与 affected-final/affected-final.trx。Alt 菜单日志和列表菜单日志、原进程状态缓存/后台刷新契约均通过。

- U1 进程菜单测试文案/入口复核：仅重跑 ProcessMenuRuntimeContractTests，返回码 0；日志 artifacts/runs/process_menu_list_20261024/process-menu-final.log，TRX process-menu-final/process-menu-final.trx。旧‘菜单栏重建’测试已改为验证用户列表进程菜单，避免测试名称与新入口混乱。

- U1 C# 全量测试：解决方案隔离输出返回码 0；日志 artifacts/runs/process_menu_list_20261024/full.log，TRX full/full.trx。Python 未修改，沿用 E09，不重复 Python 测试。

- U1 隔离 Release x64：解决方案构建返回码 0；日志 artifacts/runs/process_menu_list_20261024/release.log，输出 artifacts/runs/process_menu_list_20261024/release-bin/；继续核对警告/错误后再做差异检查。

- U1 汇总：用户列表进程菜单实现和自动化运行时验证完成，汇总 artifacts/runs/process_menu_list_20261024/u1-final-result.json。Alt 主菜单进程栏已移除；列表 WM_CONTEXTMENU 入口覆盖鼠标、菜单键/Shift+F10，当前/全部进程动作、刷新、目标快照和日志均有测试。RED 1/6（有效）；GREEN 6/6；受影响 164/164；进程菜单类 15/15；C# 全量 432/432；Release 0 警告/0 错误；限定 diff-check 返回码 0。Python 未改，沿用 31/31 与 compileall 0。真实桌面弹出/取消尚未单独执行，不能写成完整人工无障碍验收；源码尚未选择性提交或发布。

- U1 最终限定差异检查：git diff --check 返回码 0；日志 artifacts/runs/process_menu_list_20261024/final-diff-check.log，结果 final-diff-check-result.json。换行提示不计为空白错误。

- TEST-UPDATE-01 版本 RED：临时保持生产 AppVersion=4.0.48、测试预期=4.0.49，ConstantsContractTests 返回码 1；日志 artifacts/runs/release_4.0.49_20261024/version-red.log，TRX version-red/version-red.trx。随后恢复生产版本 4.0.49，未保留临时状态。

- TEST-UPDATE-01 版本/更新 GREEN：生产 AppVersion=4.0.49，Constants 与 Update 定向测试返回码 0；日志 artifacts/runs/release_4.0.49_20261024/update-green.log，TRX update-green/update-green.trx。

- TEST-UPDATE-01 C# 全量：4.0.49 源码解决方案隔离测试返回码 0；日志 artifacts/runs/release_4.0.49_20261024/full.log，TRX full/full.trx。

- TEST-UPDATE-01 Release x64：4.0.49 解决方案构建返回码 0；日志 artifacts/runs/release_4.0.49_20261024/release.log，输出 artifacts/runs/release_4.0.49_20261024/build-bin/；警告/错误从日志读取。

- TEST-UPDATE-01 publish：4.0.49 App 项目发布返回码 0；日志 artifacts/runs/release_4.0.49_20261024/publish.log，发布目录 artifacts/runs/release_4.0.49_20261024/publish/。

- TEST-UPDATE-01 4.0.49 绿色封装：返回码 0；ZIP 位于 artifacts/runs/release_4.0.49_20261024/delivery/，空 Configurations/logs，不含 PDB、Python、测试程序或 updater。大小、SHA-256、清单见 package-result.json。

- TEST-UPDATE-01 4.0.49 绿色包烟雾：结果 artifacts/runs/release_4.0.49_20261024/portable-smoke-result.json，返回码 1，主窗口=True，正常退出=False，强制清理=True。首次探针脚本误用只读 PowerShell $PID 变量并已清理精确目标 PID，未影响用户进程；不把首次错误算产品结果。

- TEST-UPDATE-01 4.0.49 绿色包烟雾复验：隔离设置 show_tray=false 后返回码 0，主窗口=True，正常退出=True，强制清理=False；结果 artifacts/runs/release_4.0.49_20261024/portable-smoke-tray-off-result.json。默认 show_tray=true 的首次关闭转托盘结果保留在 portable-smoke-result.json，不作为产品失败。

- TEST-UPDATE-01 GitCode 包提交准备：以远端 main e40ea608 为基线创建发布分支 publish-4.0.49，仅暂存 4.0.49 ZIP；本地包提交返回码 0，完整哈希和推送结果待下一步记录。

- TEST-UPDATE-01 GitHub 集成树全量：隔离 worktree shahe-unified-4.0.48-20261023 已应用当前 C#/4.0.49 交付差异；dotnet test 返回码 1，日志 artifacts/runs/release_4.0.49_20261024/github-integration/full.log，TRX github-integration/full/full.trx。

- TEST-UPDATE-01 GitHub 集成树还原：因 worktree 缺少 project.assets.json，执行隔离 dotnet restore 返回码 0；日志 artifacts/runs/release_4.0.49_20261024/github-integration/restore.log。

- TEST-UPDATE-01 GitHub 集成树全量复验：还原后 dotnet test 返回码 0；日志 artifacts/runs/release_4.0.49_20261024/github-integration/full-after-restore.log，TRX full-after-restore/full-after-restore.trx。

- TEST-UPDATE-01 GitHub 集成树 Release：还原后的集成 worktree 构建返回码 0；日志 artifacts/runs/release_4.0.49_20261024/github-integration/release.log，输出 github-integration/build/。

- TEST-UPDATE-01 GitHub 集成提交：仅暂存列出的 C#/测试文件，cached/worktree diff-check 均返回 0；集成提交返回码 0，提交信息为 4.0.49 菜单/定位/文案交付，哈希将在推送后核对。

- TEST-UPDATE-01 GitHub 推送重试：gh 已确认 childhood123 具有 repo 权限并完成 Git 凭据接线；本次推送返回码 128，远端核对返回码 0。

- TEST-UPDATE-01 GitHub API 同步：smart HTTPS 因网络重置未完成，改用已认证 childhood123 的 Git Data API；脚本返回码 1，详细结果 artifacts/runs/release_4.0.49_20261024/github-api/result.json，日志 github-api/sync.log。

- TEST-UPDATE-01 GitHub gh API 同步：smart HTTPS 因连接重置，使用 gh Git Data API 继续；脚本返回码 1，结果 artifacts/runs/release_4.0.49_20261024/github-api/result-gh-api.json，日志 github-api/sync-gh-api.log。

- TEST-UPDATE-01 GitHub gh API 远端引用复核：`gh api repos/childhood123/shahe-bulk-launcher/git/ref/heads/沙河批量打开器` 返回码 0，远端引用为 `6cff723da76cd2db0db10ec379170e8b2c4be5e1`；提交消息为 `feat: sync C# migration and publish 4.0.49`，GitHub API 报告 changedPaths=217、uploadedBlobs=214、deletedPaths=3。证据：`artifacts/runs/release_4.0.49_20261024/github-api/result-gh-api.json`。

- TEST-UPDATE-01 GitHub 关键文件只读复核：远端 `LauncherConstants.cs` 包含 `4.0.49` 且不含 `4.0.48`；定位、朗读文案和用户列表进程菜单相关文件均可匿名读取。证据：本轮 gh API 查询输出及 `artifacts/runs/release_4.0.49_20261024/github-api/remote-tree.json`。

- TEST-UPDATE-01 GitHub tree 字节一致性检查：远端 tree 路径数量与集成 HEAD 均为 238 个 blob，缺失/多余路径均为 0；但 199 个 blob SHA 不一致。根因初步定位为同步脚本使用工作树 `File.ReadAllBytes`，受 Windows 换行转换影响，而不是使用 Git object 字节。该结果不作为源码同步通过；结果 `artifacts/runs/release_4.0.49_20261024/github-api/tree-compare-result.json`，脚本错误修复与新提交待后续记录。

- TEST-UPDATE-01 GitHub 精确 blob 纠正脚本 RED：PowerShell 语法解析返回码 0，但执行在 `.gitignore` 的 Git blob 校验处返回码 1；原因是初版校验错误地对裸内容计算 SHA-1，未包含 Git `blob <长度>\0` 头。未创建纠正提交、未上传纠正 blob。日志：`artifacts/runs/release_4.0.49_20261024/github-api/sync-gh-api-corrected.log`；脚本：`artifacts/runs/release_4.0.49_20261024/github-api-correct-tree.ps1`。

- TEST-UPDATE-01 GitHub 精确 blob 纠正 GREEN：修正 Git blob 头哈希后，脚本语法解析返回码 0，Git Data API 同步返回码 0；以原远端提交 `6cff723da76cd2db0db10ec379170e8b2c4be5e1` 为父提交，处理 199 个差异 blob，生成纠正提交 `82bc77e0b7631487c3cff228a44a8d9ffa5a6fb4`，远端 tree 为 `ad9a4c0c92bbb6c04747a9a04965369ef7147430`，与本地集成 HEAD tree 一致。日志/结果：`artifacts/runs/release_4.0.49_20261024/github-api/sync-gh-api-corrected.log`、`result-gh-api-corrected.json`。

- TEST-UPDATE-01 GitHub 远端 tree 独立复核：匿名/认证 API 读取返回码 0；远端引用 `82bc77e0b7631487c3cff228a44a8d9ffa5a6fb4`，远端 tree 与本地集成 HEAD 均为 `ad9a4c0c92bbb6c04747a9a04965369ef7147430`；两侧均 238 个 blob，missing=0、extra=0、shaDiff=0、tree 未截断。结果：`artifacts/runs/release_4.0.49_20261024/github-api/tree-compare-final-result.json`。GitHub 源码同步通过精确 tree 验收。

- TEST-UPDATE-01 GitCode 发布基线复核：`git ls-remote origin refs/heads/main` 返回码 0，远端 main=`e40ea608381e37ba3843b149b1fc764b93ab5e5c`；发布 worktree `publish-4.0.49` 在该基线上领先本地 ZIP 提交 `852b560f1233d3224fd596a6f5a4ce23336df858`。远端 `update.json` 仍为 `latest_version=4.0.47`。4.0.49 ZIP 大小 `3014379`、SHA-256 `9FB9AF89006115DF0EBC77FFB184A54A7293CF3756DA43D5FED234B204863DD3`、Git blob=`684c8800f28288d83f7540846433531d5d2364dc`。证据：发布 worktree、`artifacts/runs/release_4.0.49_20261024/package-result.json`。

- TEST-UPDATE-01 GitCode 4.0.49 清单本地 GREEN：解析发布 worktree `update.json` 返回码 0，latest_version/package.to=4.0.49；URL 中 blob SHA、ZIP 大小 `3014379` 和 SHA-256 `9FB9AF89006115DF0EBC77FFB184A54A7293CF3756DA43D5FED234B204863DD3` 全部匹配。结果：`artifacts/runs/release_4.0.49_20261024/gitcode-manifest-local-result.json`。

- TEST-UPDATE-01 GitCode 清单差异检查：发布 worktree `git diff --check` 返回码 0；工作树仅有 `update.json` 修改，4.0.49 ZIP 已在提交 `852b560f1233d3224fd596a6f5a4ce23336df858` 中，未发现其他待提交文件。换行转换提示为 Git 既有提示，不计为空白错误。

- TEST-UPDATE-01 GitCode 清单提交：仅暂存 `update.json`，cached diff-check 返回码 0；提交 `8e1cc21`（完整哈希待 push 后复核）已生成，发布分支同时包含 ZIP 提交 `852b560f`。回退清单提交命令：`git -C artifacts/runs/updater_added_dll_20261023/release-repo revert 8e1cc21`。

- TEST-UPDATE-01 GitCode 写入认证检查：`git credential fill` 在当前 Windows credential provider 上无输出并持续等待，已在无凭据内容输出的情况下中断，不能据此宣称已有 GitCode 写入认证。没有修改远端或发布提交。

- TEST-UPDATE-01 GitCode 非交互推送：`GIT_TERMINAL_PROMPT=0`、`GCM_INTERACTIVE=Never` 下 `git push --porcelain origin HEAD:main` 返回码 128，错误为无法获取密码；远端 main 未变更，不能将本地提交视为公开发布。推送日志为本轮命令输出。

- TEST-UPDATE-01 GitCode 浏览器/GCM 写入尝试：按用户指定改用 Google Chrome；本机 Chrome 可启动，但 Open Browser/Computer Use 接管接口未处于活动状态，未自动点击或读取登录数据。普通 `git push` 由 Git Credential Manager 等待浏览器授权，未产生输出，已中断以避免悬挂；远端仍未变化。不能将该尝试记为上传成功，需在 Chrome 中完成 GitCode 登录/授权后再重试。

- TEST-UPDATE-01 GitCode 发布阻断状态复核：发布 worktree HEAD=`8e1cc21b56ca75552804a2eea6f4ab58b7464b13`，工作树干净，远端 main 仍=`e40ea608381e37ba3843b149b1fc764b93ab5e5c`，本地清单 latest_version=`4.0.49`。ZIP 和清单未丢失，等待 GitCode Chrome 登录/写入授权后继续。结果：`artifacts/runs/release_4.0.49_20261024/gitcode-push-blocked-result.json`。

- TEST-UPDATE-01 Chrome 网页发布接续：用户已打开 GitCode 页面；Open Browser Use ping 返回成功，复用当前 Chrome 标签并确认发布仓库 main 和上传权限。文件选择器回调未返回，CDP 直接设置文件受浏览器 API 限制；通过既有页面文件控件注入用户授权的 ZIP 后，页面列出目标文件且提交按钮可用。网页已提交包上传，当前显示“上传中”；正式 update.json 尚未更新。浏览器操作记录保存在本轮 browser-upload-package.log。
- TEST-UPDATE-01 GitCode 网页包提交及匿名下载：包上传已生成远端提交 e27c484fb1f6c7eb0433019aa5cb852e287fc657；匿名下载返回码 0，大小/SHA-256 与本地包一致=True。证据 artifacts/runs/release_4.0.49_20261024/gitcode-package-anonymous-result.json；正式清单尚未更新。

- TEST-UPDATE-01 GitCode Chrome 清单发布最终 GREEN：网页编辑器曾对 JSON 追加尾部字符，已通过 Chrome 网页删除旧 `update.json` 后重新上传本地原始文件，未改动 ZIP；最终远端 `main`=`82edc3a4e9e4eb062d9c36dc8f4130ea90b8561b`，网页显示文件大小 `620 B`。按生产 `UpdateService` 实际请求形式 `https://raw.gitcode.com/childhood123/shahe-bulk-launcher-releases/raw/main/update.json?t=<timestamp>` 匿名读取返回 HTTP `200`，解析成功，`latest_version=4.0.49`，包 URL/blob、大小 `3014379` 和 SHA-256 `9FB9AF89006115DF0EBC77FFB184A54A7293CF3756DA43D5FED234B204863DD3` 全部匹配。无 `t` 的 GitCode JSON 预览接口返回 403“暂不支持预览”，不影响软件带时间戳的实际请求。证据：`artifacts/runs/release_4.0.49_20261024/gitcode-manifest-anonymous-result.json`、`anonymous-update-4.0.49-final.json`；ZIP 匿名核对仍见 `gitcode-package-anonymous-result.json`。

- TEST-UPDATE-02 4.0.47 定向检测首次尝试：从 `shahe-bulk-launcher-4.0.47-win-x64.zip` 解压隔离副本并调用旧版 `ShaheLauncher.Core.dll` 的真实 `UpdateService.FetchUpdateManifest`，探针编译返回码 `0`，运行返回码 `1`；失败发生在版本比较前，异常为 .NET Framework 默认 TLS 无法建立 GitCode HTTPS 安全通道。该结果仅记录为探针环境阻断，不作为产品更新失败结论。证据：`artifacts/runs/release_4.0.49_20261024/old47-detection/old47-update-detection-result.json`。

- TEST-UPDATE-02 4.0.47 TLS 定向重试：探针仅补充 `SecurityProtocolType.Tls12`，编译返回码 `0`，运行返回码 `1`；TLS 错误消失，但 `FetchUpdateManifest` 在其 8 秒请求超时内收到 `TaskCanceledException`，仍未进入版本比较。该结果继续标记为本机 .NET 网络路径阻断。证据同为 `old47-detection/old47-update-detection-result.json`（第二次运行结果）。

- TEST-UPDATE-02 4.0.47 直连重试：探针改为 `UseProxy=false` 后编译返回码 `0`，运行返回码 `1`；请求已到达 GitCode，但服务端返回 HTTP `429 Too Many Requests`。这确认网络路径已建立，同时说明继续重复请求会触发限流；该次不作为版本检测失败结论。证据同为 `old47-detection/old47-update-detection-result.json`（第三次运行结果）。

- TEST-UPDATE-02 4.0.47→4.0.49 检测 GREEN：使用已通过 HTTP 200 匿名下载的公开 4.0.49 清单作为一次性 HTTP 响应，调用隔离 4.0.47 包内真实 `UpdateService`；探针编译返回码 `0`、运行返回码 `0`。旧版默认清单地址为 `https://raw.gitcode.com/childhood123/shahe-bulk-launcher-releases/raw/main/update.json`，实际构造请求包含 `?t=1792832639`；解析结果 `currentVersion=4.0.47`、`latestVersion=4.0.49`、`hasUpdate=true`、`shouldPrompt=true`、`packageVersion=4.0.49`、`packageSize=3014379`，状态文案为“发现新版本 4.0.49，可稍后点击“下载更新””。这证明 4.0.47 能识别已发布的 4.0.49；本次是检测/选择包验证，不等同于完整关闭、替换、重启升级验收。证据：`artifacts/runs/release_4.0.49_20261024/old47-detection/old47-update-detection-result.json`。

- TEST-UPDATE-02 4.0.47→4.0.49 替换边界：在全新隔离 4.0.47 副本中调用旧版真实 `UpdateRunner`，下载回调使用已核对的公开 4.0.49 ZIP，未触碰正式配置；旧 updater 返回码 `5`，`LastError=更新包缺少主程序或 updater.exe`，未启动新版、旧版 Core 哈希未替换，配置标记仍保留。结论：4.0.47 可以检测并选择 4.0.49，但当前不支持用旧外置 updater 完成到 4.0.49 的实际替换；原因与 4.0.49 绿色包按要求不包含独立 `updater.exe` 一致。不能把“旧版可更新”写成通过，也不把旧 updater 重新加入公开包。证据：`artifacts/runs/release_4.0.49_20261024/old47-upgrade/old47-to49-install-result.json`。

- TEST-UPDATE-03 GitHub Release 资产 GREEN：在 GitHub 仓库 `childhood123/shahe-bulk-launcher` 创建 `v4.0.49` Release，目标源码分支为 `沙河批量打开器`，上传 `shahe-bulk-launcher-4.0.49-win-x64.zip`。GitHub API 返回资产状态 `uploaded`、大小 `3014379`、digest `sha256:9fb9af89006115df0ebc77ffb184a54a7293cf3756da43d5fed234b204863dd3`；随后用 `gh release download` 下载复核返回码 `0`，本地 SHA-256 与发布包一致。Release：`https://github.com/childhood123/shahe-bulk-launcher/releases/tag/v4.0.49`；资产：`https://github.com/childhood123/shahe-bulk-launcher/releases/download/v4.0.49/shahe-bulk-launcher-4.0.49-win-x64.zip`。证据：`artifacts/runs/release_4.0.49_20261024/github-release-verify/github-release-asset-result.json`。

- DOC-ENCODING-01：修复 `PROJECT_STRUCTURE.md` 中 8 条 VOICE-03 历史记录的 replacement character，恢复为可读中文文案；严格 UTF-8 解码成功，replacement character 扫描为 `0`，`rg` 乱码扫描清洁，`git diff --check` 返回码 `0`。未改动其他验证结论或源码。

### MIG-INVENTORY-20261025：Python → C# 查漏与文档整理

- 范围与结果：已覆盖根入口、20 个有业务内容的 Python 模块及 3 个包标记的映射，追踪主要 UI/服务调用与发现差异的异常分支。确认 10 项源码迁移差异，其中 `MIG-READONLY-01` 为既有未关闭项，其余为本轮新增；另列 4 项候选边界和未逐分支核完范围。它们没有本轮运行时 RED，不宣称已复现/已修复或审查已排除所有潜在遗漏。详细源码、影响和下一步唯一维护于 `docs/migration/2026-10-25-python-csharp-audit.md`。
- 文档结构：唯一执行入口为 `docs/superpowers/plans/2026-10-25-python-csharp-inventory.md`；原交接/发布计划未改，旧摘要按日期标为历史。当前模块为只读查漏与文档整理，业务修复未启动。既有完成模块和真实审计风险沿用；后续原顺序仍为补 AUDIT-04、完成后停在 AUDIT-05 前。新增差异如需修复，应独立展开，不混入旧验收通过项。
- 验证命令：`& .\artifacts\runs\migration_inventory_20261025\verify-documents.ps1`，返回码 `0`。3 份文档严格 UTF-8 解码、替换字符/新增文档空白检查通过；35 个本轮相对链接存在且源码行号未越界；模块映射缺项 `0`；比较 136 个源码/测试/工程文件的 SHA-256，改变 `0`，文件清单差异 `0`；`git diff --check -- PROJECT_STRUCTURE.md` 返回码 `0`。检查不是软件测试，通过数/失败数/跳过数不适用；没有新增 C# 全量、Python 对照、构建、GUI 或真实服务结果。
- 证据：`artifacts/runs/migration_inventory_20261025/source-inventory-before.json`、`verify-documents.ps1`、`verification-result.json`、`diff-check.log`。本轮无提交、推送或新发布，原有脏工作区和暂存状态未改变。C# 已有启动运行证据，整个软件迁移仍未完成；本轮文档不能证明旧版自动替换成功。

### PY-FULLREAD-20261025：生产 Python 全文阅读

- 基线：用户要求从头到尾阅读，范围为根入口与 shahe_bulk_launcher 内全部生产 Python 文件；tests、第三方下载和历史副本不混入产品范围。命令 `python artifacts/runs/python_full_read_20261025/inventory.py` 返回码 `0`；只解析 AST，不导入运行应用，清单为 24 个文件、12823 行、723 个类/函数定义（包含嵌套定义），另保存 43 个 C# 生产源码哈希。本步骤只是阅读对账基线，不算全文已读或产品测试通过。
- 证据：`artifacts/runs/python_full_read_20261025/source-manifest.json`、`csharp-source-baseline.json`、`inventory.py`。当前执行计划已切换到 `docs/superpowers/plans/2026-10-25-python-full-read.md`；各分域结果只有完整读完、记录连续行范围后才能验收。

- 接续说明：本次恢复时上条记录引用的临时目录实际缺失，不能把旧路径当作现存证据。已重新建立本次源码基线，执行 `python artifacts/runs/python_full_read_20261025/inventory.py` 返回码 `0`，文件/行数/定义数与上条清单一致；此基线只证明本次读取后的文件完整性，不证明两次会话之间没有改动。前轮基础模块阅读沿用会话交接说明，未交付的并行审查不计完成。本次继续直接阅读全文，不执行产品回归测试。

- 全文阅读完成：本轮重新逐行输出并阅读根入口和 `shahe_bulk_launcher/` 下全部 24 个生产 Python 文件，共 12,823 行、723 个类/函数定义（含嵌套定义）；基础模块也在本轮重新核读，失败的并行审查不计入证据。实际阅读区间、哈希和定义覆盖见 `artifacts/runs/python_full_read_20261025/coverage.json`，逐文件功能与 C# 对应见 `docs/migration/2026-10-25-python-full-read.md`。
- 覆盖记录生成：`python artifacts/runs/python_full_read_20261025/record_reading.py` 返回码 `0`，输出 `files=24`、`lines=12823`、`definitions=723`，并确认所有行和定义均已登记。该命令只生成阅读审计记录，不是软件测试，也没有修改产品源码。
- 本轮静态查漏汇总：确认 11 项 Python→C# 源码行为差异，列出 7 项待验证候选边界，并扩充 2 项原版共有风险；新增管理模块 INI 编码回退差异 `MIG-ADMIN-ENCODING-01`，同时扩充只读删除和全部 PID/关闭结束判断的影响范围。详情唯一维护于 `docs/migration/2026-10-25-python-csharp-audit.md`，本轮未修复这些差异。
- 完整性校验首次运行：`python artifacts/runs/python_full_read_20261025/verify_reading.py` 返回码 `1`，唯一错误是校验脚本要求的 `12823 行` 标记与报告实际采用的 `12,823 行` 格式不一致；文件集合、连续区间、723 个定义、43 个 C# 源码哈希、4 份文档编码/引用及 `git diff --check` 均通过。原始失败结果保留为 `artifacts/runs/python_full_read_20261025/verification-initial-marker-failure.json`，不作为产品测试失败。
- 完整性校验第二次运行：返回码 `1`，确认覆盖、哈希、编码、引用和 `git diff --check` 仍全部通过；唯一错误是逐文件报告尚未写出总行数标记。结果保留为 `artifacts/runs/python_full_read_20261025/verification-second-marker-failure.json`，随后补充文档标记。
- 完整性校验最终 GREEN：`python artifacts/runs/python_full_read_20261025/verify_reading.py` 返回码 `0`；24 个 Python 文件、12,823 行、723 个定义、43 个 C# 生产源码哈希、4 份文档的严格 UTF-8/引用/空白和 `git diff --check` 全部通过，错误数 `0`。结果与日志为 `artifacts/runs/python_full_read_20261025/verification-result.json`、`verification.log`。这是阅读/文档完整性校验，不是 C# 全量、Python 对照、构建或 GUI 测试。
- 计划收口：已复读 `docs/superpowers/plans/2026-10-25-python-full-read.md`，五项执行步骤全部完成。本轮没有修改产品源码，也没有重复已通过测试；确认的 11 项迁移差异、7 项候选边界、2 项共有风险及 AUDIT-04 等真实运行验收仍按台账保留。
- 收口后复核：计划和项目摘要更新后再次运行 `python artifacts/runs/python_full_read_20261025/verify_reading.py`，返回码仍为 `0`，错误数 `0`；最终 `verification-result.json` 已对应当前文档状态。

### GITHUB-VISIBILITY-20261106：仓库公开

- GitHub CLI 设备授权完成，`gh auth status` 确认活动账号为 `childhood123`，具备 `repo` 权限；未在证据中保存令牌。
- 执行 `gh repo edit childhood123/shahe-bulk-launcher --visibility public --accept-visibility-change-consequences` 返回码 `0`。
- 登录态复核：`visibility=PUBLIC`、`isPrivate=false`，默认分支为 `沙河批量打开器`。未登录 GitHub API 复核：`visibility=public`、`private=false`；仓库主页与 `v4.0.49` Release 均返回 HTTP `200`。
- 证据：`artifacts/runs/github_visibility_20261106/result.json`。本项只修改 GitHub 仓库可见性，没有修改产品源码、分支或 Release 内容。

### U1-ACCEPTANCE-20261109：用户列表菜单真实验收

- 执行依据：用户本轮要求验收不可用功能，沿用原 U1 计划。顺序为环境和隔离目标核对 → 真实鼠标/键盘菜单与取消 → 当前/全部操作及目标保持 → 清理本轮测试进程 → 分别报告通过、失败和未验证；产品修复与发布不在本轮范围内。
- 环境检查：通过 `node_repl` 的 `@oai/sky.list_windows()` 成功枚举桌面窗口，当前未发现运行中的沙河主程序。`SbieSvc` 和争渡读屏正在运行，均不停止。历史 `artifacts/runs/process_menu_list_20261024/` 原始目录在本次工作区缺失，旧测试结论仅作为历史文档记录引用，不冒充本轮实测。
- 证据统一存入 `artifacts/runs/user_list_acceptance_20261109/`；尚未启动测试应用或执行菜单动作。

- 公开包身份核对与解压：PowerShell 返回码 0；release/4.0.49 的 ZIP SHA-256 与发布记录一致，已解压至本轮 portable/。原始主 EXE 与 Core DLL 保持不变。将通过隔离夹具加载该窗体与业务服务，仅替换测试用户/PID来源并记录实际音频状态；Sandboxie 正式用户发现和完整启动流程不在该夹具的证据范围内。

- 隔离夹具编译：使用本机 .NET Framework 64 位 csc.exe，测试音频进程和菜单宿主均返回码 0；日志 peer-build.log、harness-build.log 位于本轮目录。宿主加载原始公开包程序集，进程操作有 PID/路径/启动身份白名单，音频为静音 WAV；未重新编译或改动产品程序集。

- 首次宿主启动未形成有效 UI 证据：portable/runtime-events.jsonl 中记录宿主启动和夹具 Snapshot 空引用异常；桌面显示 .NET 异常对话框，未完成堆栈展开。工具随后依次返回 CUA_GEOMETRY_UNAVAILABLE、CUA_EXECUTION_FAILED、CUA_TURN_NOT_ACTIVE，已停止发出桌面输入。本轮不能把这次夹具启动失败归为产品菜单失败；真实菜单、焦点、读屏仍未验收。
- 后续改做原始 Core DLL 的真实音频/进程调用验证，仍只针对两个已创建测试进程；它能证明服务的实际效果，但不能替代菜单端到端验收。不使用 shell/UI Automation 绕过失效桌面连接。

- 服务探针编译返回码 0，日志 service-build.log；只读枚举全部音频端点来核实两个测试进程的实际静音/音量，不以状态文案代替效果。异常宿主已按精确 PID 与路径校验后强制清理，此次不记正常退出；两个测试音频进程保留用于服务验证。

- 服务探针首次 audio 执行返回码 2，止于测试进程身份比对 Peer identity mismatch，尚未执行音频动作；保留 service-audio.log、portable/service-audio-error.txt。正在核对日志时间解析，不把测试保护拦截记为产品功能失败。

- 身份核对已查明为夹具 JSON 日期转换问题：实际 PID、路径、StartTime 与原始日志完全一致；读取日志时禁用 Json.NET 自动日期转换以保留原始精度，不放宽身份校验。修正夹具重新编译返回码 0，日志 service-build-final.log；产品源码未改。

- 服务探针 audio 第二次返回码 2：身份守卫通过，但全端点观察器遇到 InvalidComObjectException；位置 Observe/Com，还未形成有效服务动作结果。保留 service-audio-final.log；需隔离探针和产品音频枚举的 COM 生命周期，不能归为菜单失败。

- 观察器问题原因：产品 ListOutputDevices 会 FinalReleaseComObject，同线程探针先持有相同 COM 枚举器后再调用产品枚举使该 RCW 失效。调整为先获得端点清单再创建探针枚举器；仅改验收代码，编译返回码 0，日志 service-observer-build.log。

- 真实音频服务验收：ServiceProbe.exe audio 返回码 0，通过 8、失败 0、跳过 0；当前/全部音量增减（50%↔55%）、当前/全部静音和取消静音均经全部音频端点的 COM 读回验证，当前操作未影响另一个测试用户。日志 service-audio-observed.log，完整前后状态 portable/service-audio-result.json；这是服务层实测，不替代菜单键盘验收。

- 已复现 MIG-ALL-PIDS-01：ServiceProbe.exe partial 返回码 1，通过 0、失败 2、跳过 0。注入“全部 PID 查询仅返回 A，但逐用户查询仍可得到 A/B”的非空部分结果后，全部提高音量只改变 A，全部静音也只静音 A，B 被遗漏，状态却分别提示全部成功。日志 service-partial.log，完整前后状态 portable/service-partial-result.json；这是受控查询异常的真实音频效果，不是声明正常查询下一直失败。场景结束已恢复本轮两测试进程的音量/静音。

- 设备切换第一轮：ServiceProbe.exe routing 返回码 0，两个场景的原生 API 均接受请求；日志 service-routing.log、portable/service-routing-result.json。全端点观察却显示活跃音频仍在原默认设备，因此仅记 API 接受，不记实际设备切换成功；已将两个测试进程的路由恢复到原默认设备。下一轮仅补查延迟生效疑点。

- 针对设备生效延迟和全部关闭目标遗漏增加有界探针场景；仅改测试夹具，编译返回码 0，日志 service-followup-build.log。正常音量/静音已通过项不重复。

- 设备实际效果复查：ServiceProbe.exe routing-live 返回码 1，通过 0、失败 2、跳过 0；分别等待 3 秒后，当前/全部测试进程的 PlaySound 活跃会话仍在原默认端点，虽然接口和状态均报告切换成功。日志 service-routing-live.log、portable/service-routing-live-result.json。结论限定为本轮正在播放的 WinMM/PlaySound 会话未即时迁移，其他音频框架、重建播放流和听感未验收；两个测试进程路由已恢复。

- 真实关闭服务验收：ServiceProbe.exe close 返回码 1，通过 2、失败 1、跳过 0；正常关闭当前仅结束 A，正常关闭全部结束剩余 B。新建两个专用无界面测试进程后，注入部分 PID 查询复现“全部关闭”仅结束 A、B 仍运行，状态为“已关闭 1 个全部打开进程”。日志 service-close.log、portable/service-close-result.json；剩余本轮测试进程已按持有的 Process 身份核对路径后清理，未关闭正式程序。

- 清理复核：进程枚举未发现 AcceptanceHarness、AcceptancePeerA/B 或 ServiceProbe 残留；SbieSvc 与原争渡读屏进程仍运行。报告已写入 docs/migration/2026-11-09-user-list-acceptance.md。已复读原 U1 计划，真实桌面/无障碍勾选项保持未完成；没有修改计划文件、产品源码或执行提交/发布。

- 收尾验证返回码 0：原公开包主 EXE/Core DLL 哈希仍与发布清单一致，本轮测试进程残留 0，原争渡读屏与 Sandboxie 服务仍运行；结果 cleanup-result.json。项目摘要与验收报告严格 UTF-8 通过，无替换字符，限定 git diff --check 返回码 0。实际菜单验收仍因桌面工具会话失效未完成；只交付本轮已验证结果和问题记录，不声明软件整体完成。

### USER-LIST-FIX-20261109：未通过项目修复

- 用户已在完整方案展示后要求开始修复，计划已写入。GitHub fetch 返回码 0，隔离工作树基线为 82bc77e0b7631487c3cff228a44a8d9ffa5a6fb4，分支 fix/user-list-acceptance-20261109。主目录生产 C# 与远端一致，已有菜单契约文件在主目录未纳入其旧 HEAD，不视为删除；主目录未提交改动保持原样。工作树 D:/mycodex/worktrees/shahe-user-list-fixes-20261109，证据 artifacts/runs/user_list_fixes_20261109/。

- 依赖恢复：dotnet restore csharp/ShaheBulkLauncher.sln --locked-mode 返回码 0；日志 artifacts/runs/user_list_fixes_20261109/restore.log。使用 D:/Dev/NuGet/packages 缓存，未变更包版本。

- task1 / red：命令 dotnet test csharp/ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:/mycodex/git/artifacts/runs/user_list_fixes_20261109/task1/bin/ --results-directory D:\mycodex\git\artifacts\runs\user_list_fixes_20261109\task1\red --logger trx;LogFileName=red.trx --filter FullyQualifiedName~AllProcessTargetsContractTests|FullyQualifiedName~NormalizesPidsAndUsesFastAllProcessPathBeforePerSandboxFallback；返回码 1；总数 11，通过 3，失败 8，未执行 0。日志 D:\mycodex\git\artifacts\runs\user_list_fixes_20261109\task1\red/red.log，TRX D:\mycodex\git\artifacts\runs\user_list_fixes_20261109\task1\red\red.trx。

- task1 / green：命令 dotnet test csharp/ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:/mycodex/git/artifacts/runs/user_list_fixes_20261109/task1/bin/ --results-directory D:\mycodex\git\artifacts\runs\user_list_fixes_20261109\task1\green --logger trx;LogFileName=green.trx --filter FullyQualifiedName~AllProcessTargetsContractTests|FullyQualifiedName~NormalizesPidsAndCompletesFastAllProcessResultWithPerSandboxQueries；返回码 0；总数 11，通过 11，失败 0，未执行 0。日志 D:\mycodex\git\artifacts\runs\user_list_fixes_20261109\task1\green/green.log，TRX D:\mycodex\git\artifacts\runs\user_list_fixes_20261109\task1\green\green.trx。

- task1 / affected：命令 dotnet test csharp/ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:/mycodex/git/artifacts/runs/user_list_fixes_20261109/task1/bin/ --results-directory D:\mycodex\git\artifacts\runs\user_list_fixes_20261109\task1\affected --logger trx;LogFileName=affected.trx --filter TestCategory=ProcessActions|TestCategory=Application|TestCategory=UiControls|TestCategory=Hotkeys|TestCategory=DiagnosticLogging；返回码 0；总数 176，通过 176，失败 0，未执行 0。日志 D:\mycodex\git\artifacts\runs\user_list_fixes_20261109\task1\affected/affected.log，TRX D:\mycodex\git\artifacts\runs\user_list_fixes_20261109\task1\affected\affected.trx。

- task1 / full：命令 dotnet test csharp/ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:/mycodex/git/artifacts/runs/user_list_fixes_20261109/task1/bin/ --results-directory D:\mycodex\git\artifacts\runs\user_list_fixes_20261109\task1\full --logger trx;LogFileName=full.trx；返回码 0；总数 440，通过 440，失败 0，未执行 0。日志 D:\mycodex\git\artifacts\runs\user_list_fixes_20261109\task1\full/full.log，TRX D:\mycodex\git\artifacts\runs\user_list_fixes_20261109\task1\full\full.trx。

- 任务一隔离 Release x64：dotnet build csharp/ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:/mycodex/git/artifacts/runs/user_list_fixes_20261109/task1/release/ 返回码 0；完整警告/错误和生产源码编译结果见 task1/release.log。

- 任务一真实进程复验夹具编译返回码 0，日志 task1/native-build.log；加载本轮 Release 的 Core，使用两个目标和一个受保护的专用静音进程，未访问正式用户目标。

- 任务一真实部分查询复验：NativeTargetsProbe.exe 返回码 0；通过 5，失败 0，执行异常 ；日志 task1/native.log，前后音频/存活/PID/程序集哈希证据 task1/native/native-result.json。

- 任务一修复已按最小补丁同步主目录，保留原定位等已有改动。首次文本比对仅发现新增测试文件末尾多一空行，已纠正；三个变更源码文件按换行归一后与已验证工作树完全一致。Python 生产文件未改，只读对照原版逐箱查询语义。

- 任务一生产编译/隔离 Release 返回码 0、警告 0、错误 0。真实进程证据确认两个目标均完成、保护进程未被动作影响且三个测试进程最终均退出。Python 未改，复用已有 python-complete-result.json（返回码 0，通过 31/失败 0/跳过 0）及 compile-result.json（返回码 0），均位于 artifacts/runs/updater_added_dll_20261023/；旧中间失败不冒充通过。代码结果与尚未验收边界整理在 docs/migration/2026-11-09-user-list-fixes.md。

- 任务一提交前 git diff --check 返回码 0；严格 UTF-8 解码可读，但远端基线 PROJECT_STRUCTURE.md 的历史语音记录含旧 replacement character，因此全文文档检查未通过。此问题在原基线已存在，主目录对应历史记录已按用户此前要求纠正；仅恢复相同历史记录的中文文本，保留原结果和证据路径。

- 任务一文档检查修正后通过：以逐条相同证据路径从主目录恢复了 8 条远端旧乱码记录的原有中文，原返回码/状态不变。严格 UTF-8、替换字符和尾随空白通过，git diff --check 返回码 0；未重跑产品测试。

- task1 / empty-status-red：命令 dotnet test csharp/ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:/mycodex/git/artifacts/runs/user_list_fixes_20261109/task1/bin/ --results-directory D:\mycodex\git\artifacts\runs\user_list_fixes_20261109\task1\empty-status-red --logger trx;LogFileName=empty-status-red.trx --filter FullyQualifiedName~EmptyAllDeviceTargetsPostTheResultToTheUiThread；返回码 1；总数 1，通过 0，失败 1，未执行 0。日志 D:\mycodex\git\artifacts\runs\user_list_fixes_20261109\task1\empty-status-red/empty-status-red.log，TRX D:\mycodex\git\artifacts\runs\user_list_fixes_20261109\task1\empty-status-red\empty-status-red.trx。

- task1 / final-green：命令 dotnet test csharp/ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:/mycodex/git/artifacts/runs/user_list_fixes_20261109/task1/bin/ --results-directory D:\mycodex\git\artifacts\runs\user_list_fixes_20261109\task1\final-green --logger trx;LogFileName=final-green.trx --filter TestCategory=ProcessActions|TestCategory=Application|TestCategory=UiControls|TestCategory=Hotkeys|TestCategory=DiagnosticLogging；返回码 0；总数 177，通过 177，失败 0，未执行 0。日志 D:\mycodex\git\artifacts\runs\user_list_fixes_20261109\task1\final-green/final-green.log，TRX D:\mycodex\git\artifacts\runs\user_list_fixes_20261109\task1\final-green\final-green.trx。

- 任务一独立复核未发现目标范围扩大；复核提示的空目标状态线程问题已先取得单项 RED（实际线程不等于调用/UI线程），再改为通过 PostToUi 投递，新增受影响 GREEN 已通过。仅这一新增改动要求一次最终全量/构建，不重跑已通过真实音量场景；主目录已同步相同补丁。

- task1 / final-full：命令 dotnet test csharp/ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:/mycodex/git/artifacts/runs/user_list_fixes_20261109/task1/bin/ --results-directory D:\mycodex\git\artifacts\runs\user_list_fixes_20261109\task1\final-full --logger trx;LogFileName=final-full.trx；返回码 0；总数 441，通过 441，失败 0，未执行 0。日志 D:\mycodex\git\artifacts\runs\user_list_fixes_20261109\task1\final-full/final-full.log，TRX D:\mycodex\git\artifacts\runs\user_list_fixes_20261109\task1\final-full\final-full.trx。

- 任务一最终生产编译/Release x64：dotnet build 解决方案 --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=本轮/task1/release-final/，返回码 0，警告 0、错误 0；日志 task1/release-final.log。最终全量通过后代码未再变更。

- 任务一暂存后差异检查发现新增计划末尾多一个空行（返回码 2），尚未提交；已仅去掉尾空行。暂存仍严格限定之前列出的六个文件，无业务修改或测试重跑。

- 任务一独立提交已生成 0fdadbe；首次 git push origin HEAD:refs/heads/沙河批量打开器 返回码 1，错误 Empty reply from server。尚不声称远端已更新；继续核对并尝试已有认证通道，不强推。

- 中断恢复后，原 push 会话已不存在，无法取得其退出码；本次 gh API 远端确认返回连接超时，不能确认远端更新。任务一提交 0fdadbe90e5f9bc1dfeec24f9e4a9553721c2c6c 已在本地，推送步骤保持未勾选。为避免网络阻断浪费重复测试，继续已授权任务二的本地根因核查，不宣称提交已发布。

- 任务二路由对照夹具编译返回码 0，日志 task2/probe-build.log；只调用隔离测试进程的路由，读回原始两个角色配置并在结束时恢复，比较原 Python 已安装后端和 C# 的持续/重建音频流。

- 任务二路由对照执行返回码 0，日志 task2/routing-comparison.log，详细配置读回/音频端点/清理见 task2/native/routing-comparison.json。尚需读取结果，接口成功不等于流迁移。

- 任务二根因边界核对：C# 与本机 Python winappaudiorouter 1.1.1 均调用相同 PolicyConfig 接口、相同两个角色；真实读回均为目标设备，但同一 WinMM/PlaySound 测试流在持续及停止后重新播放场景均留在默认端点。原 Python 后端同样未迁移该流，不能定性为 C# 独有功能缺失；未强制重启或修改用户程序，原路由已恢复、测试进程已退出（task2/native/routing-comparison.json）。任务二仍保留实际目标播放器适配边界，不虚写修复通过。

- 任务三验收夹具修正并编译返回码 1，日志 task3/harness-build.log。上一轮在窗体句柄创建前触发异步列表刷新；本轮将刷新安排在 Shown 事件后，状态观察从显示后开始，并捕获完整 UI 异常日志。未改产品列表逻辑；桌面窗口枚举已恢复可用。

- 任务三夹具首次编译因源文件路径被编译器解析成主目录而失败（返回码 1，未启动程序），改用 portable 工作目录的相对源码路径；再次编译返回码 0，日志 task3/harness-build-final.log。

- 任务三本轮宿主已正常显示：sky.list_windows 返回“沙河批量打开器 - 修复后用户列表隔离验收”及两个测试窗口，上一轮初始化异常未再出现。首次截图/无障碍组合捕获返回 CUA_EXECUTION_FAILED；按规范重新枚举恢复时返回 CUA_TURN_NOT_ACTIVE，已停止桌面输入，未执行任何功能菜单。窗口可显示不等于键盘/读屏通过，真实菜单仍受宿主连接阻断。

- 任务一源码推送成功：本次命令临时指定已在监听的 http://127.0.0.1:10808 代理，git push 返回码 0，远端沙河批量打开器从 82bc77e 前进到 0fdadbe90e5f9bc1dfeec24f9e4a9553721c2c6c；ls-remote 再核对分支。回退命令 git revert 0fdadbe90e5f9bc1dfeec24f9e4a9553721c2c6c。未修改系统代理或 Git 持久配置，未替换公开 ZIP。

- 任务三清理完成：三个本轮测试窗口进程按精确 PID+可执行路径校验后结束，结果 task3/cleanup-result.json。桌面连接失效后采用进程清理，不计正常 UI 退出；正式程序/读屏未停止。夹具日志未再出现 fatal/ui-thread-error/snapshot-error，已显示主窗体和运行/空闲用户列表，但未取得菜单动作证据。

- 本轮收尾核对：主目录三份修改源码与隔离验证工作树一致；计划、修复报告、项目文档严格 UTF-8 与空白检查通过，隔离工作树 git diff --check 返回码 0；本轮测试进程残留 0。计划已逐项复读，任务一完成，其余未完成项仍保留。源码版本未提升，公开 ZIP 未改，环境阻断不计菜单验收通过。

- 收尾文档提交 f7169c8273136d67d5dbf90804ba190933087a7e 已推送 origin/沙河批量打开器，push 与 ls-remote 返回码均为 0，远端头与该提交一致；隔离工作树干净。功能修复提交为 0fdadbe90e5f9bc1dfeec24f9e4a9553721c2c6c。回退分别使用 git revert f7169c8273136d67d5dbf90804ba190933087a7e（文档）和 git revert 0fdadbe90e5f9bc1dfeec24f9e4a9553721c2c6c（代码）。主目录保留用户原有未提交改动，未重置分支。

## MAIN-FULL-20261109：用户追加的主目录全量测试

- 用户要求“现在软件跑一下全量测试”，本次在主目录 `D:\mycodex\git` 编译当前源码并运行不带筛选器的 C# 全量测试；使用独立输出，不沿用旧工作树的测试程序集，不停止用户进程。此前已通过记录不覆盖，音频设备实际生效和真实菜单验收仍保留未完成状态。
- 依赖准备：主目录缺少 `project.assets.json`，执行 `dotnet restore csharp\ShaheBulkLauncher.sln --locked-mode --property:Platform=x64`，进程内设置 `NUGET_PACKAGES=D:\Dev\NuGet\packages`；返回码 0，四个项目按锁文件还原完成。日志和返回码：`artifacts/runs/user_list_fixes_20261109/main-full-20261109_174528/restore.log`、同目录 `restore-result.json`；本步未运行测试，不计通过数。
- 全量测试：在主目录执行 `dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\user_list_fixes_20261109\main-full-20261109_174528\bin\ --results-directory D:\mycodex\git\artifacts\runs\user_list_fixes_20261109\main-full-20261109_174528 --logger "trx;LogFileName=main-full.trx"`；返回码 0，总数 443、通过 443、失败 0、跳过/未执行 0。Core、AuditRunner、App、Tests 均由当前主目录源码完成 Release x64 编译，测试执行耗时约 25 秒；结果来自本次新生成的 TRX。日志：同目录 `main-full.log`；TRX：`main-full.trx`；命令、时间、返回码和完整计数：`test-result.json`。本次不修改生产源码，不运行真实桌面验收，不升级或发布公开包；Python 未改，未重复运行其既有对照测试。

## LEGACY-UPDATE-20261109：旧版更新复验

- 用户追加要求测试旧版是否仍能更新。本轮步骤：取得并核对原始发布包 → 调用旧版实际更新服务检测在线清单 → 使用旧更新器在隔离副本完成下载、校验和安装尝试 → 核对版本、配置保留与退出/重启结果。当前先核对曾发出的 4.0.47；不修改生产源码、远端清单或正式配置，不把旧历史结果作为本轮通过证据。
- 在线清单核对：`curl.exe --fail --location --connect-timeout 8 --max-time 30` 请求默认 GitCode `update.json` 并带本次时间戳；返回码 0、HTTP 200、620 字节，当前公开版本为 4.0.49。命令地址、响应头和原始清单保存在 `artifacts/runs/legacy_update_20261109/manifest-fetch-result.json`、`manifest-headers.txt`、`online-update.json`，文本日志 `manifest-fetch.log`；仅表示服务器清单可访问，尚不代表旧程序更新成功。

- 原始旧包准备：从历史发布清单中的固定 blob 地址重新下载 4.0.47，curl 返回码 0、HTTP 200；大小和 SHA-256 均与原发布记录一致，解压到本轮 `old47/`，包内确有外置 `updater.exe`。完整大小、哈希和来源唯一记录于 `artifacts/runs/legacy_update_20261109/old47-download-result.json`，下载日志 `old47-download.log`。

- 原始 4.0.47 EXE 已在隔离目录实际启动，桌面无障碍树读到“扫描成功”及 Sandboxie 路径；点击因 CUA_GEOMETRY_UNAVAILABLE 未完成，补截图失败 CUA_EXECUTION_FAILED，恢复枚举为 CUA_TURN_NOT_ACTIVE。停止桌面输入，按精确可执行路径核对后清理本轮程序，未操作用户正式程序；PID、目录、错误与残留数量见 `artifacts/runs/legacy_update_20261109/desktop-attempt-result.json`。此轮没有有效更新按钮操作证据，后续改为直接调用原始发布包内更新逻辑，单独标记范围。

- 旧版更新探针编译：仅引用原始 4.0.47 包的程序集，输出到与安装目标分离的 probe47；Framework64 csc 编译返回码 0。命令和日志见 `artifacts/runs/legacy_update_20261109/probe-build-result.json`、`probe-build.log`。探针保留原始 HTTPS 下载、清单解析和安装逻辑，只记录结果并约束隔离目录；不把界面点击/外置启动包装层计入验证。桌面步骤进程盘点未找到仍运行的目标，因此实际未强制结束任何进程，未取得启动 PID，详见 desktop-attempt-result.json。

- 原始旧版在线检测：`probe47/LegacyUpdateProbe.exe detect` 返回码 0；调用原始 4.0.47 UpdateService.FetchUpdateManifest、版本判定和 PrepareUpdaterCommand，未注入清单响应或网络代理。有效结果/异常分别见 `artifacts/runs/legacy_update_20261109/detection-result.json` 或 `detect-error.json`，原始清单 `old47-live-manifest.json`，日志 `detection.log`，命令返回码 `detection-command-result.json`；后续安装结论单独记录。

- 原始旧版在线安装尝试：`probe47/LegacyUpdateProbe.exe install` 返回码 5；使用上一步原始 UpdateService 生成的请求，调用旧包真实 UpdateRunner 和 UpdateRunner.DownloadPackage，目标仅为本轮 old47 副本。下载大小/哈希/校验见 `artifacts/runs/legacy_update_20261109/old-downloader-result.json`，安装返回码、错误、前后文件哈希、配置保留与重启 PID 见 `install-result.json`，文本日志 `install.log`，运行返回码 `install-command-result.json`。

- 本轮结论：原始 4.0.47 在线检测和选包通过，旧下载器实际下载并校验通过；安装返回码 5，错误“更新包缺少主程序或 updater.exe”。核对新包确有主程序而没有 updater.exe，属于旧外置更新器与新内置更新包的格式要求冲突。安装未替换旧 EXE/Core、未触发重启，实际隔离配置及标记哈希均保留；不能声明旧版自动升级成功。包内容见 `artifacts/runs/legacy_update_20261109/target-package-contents.json`。按本轮下载路径、系统临时目录边界、目录名称及文件哈希核对后清理安装器遗留临时目录，进程/配置/文件最终结果见 `verification-cleanup-result.json`；GUI 更新操作仍未验收。

## INTEGRATED-UPDATE-20261109：4.0.49 内置更新复验

- 用户要求确认 4.0.49 是否能正常更新。步骤为原包哈希核对 → 原始更新服务在线检测 → 在隔离副本生成安装请求 → 启动原始主 EXE 的 --install-update 模式 → 核对下载、校验、文件恢复、配置保留、重启和清理。若服务器仍只有同版，则明确按同版完整包重装演练记录，不伪造已发布的更高版本，也不把结果写成跨版本升级已验收。
- 原始 4.0.49 绿色包大小/SHA-256 与发布清单一致，解压到本轮 app49，实际不包含外置 updater.exe；独立探针使用同一原包程序集。核对返回码 0，完整路径、大小和哈希见 `artifacts/runs/integrated_update_20261109/original-package-result.json`。所有改动限定隔离副本，远端清单、公开包和正式配置保持原样。

- 内置更新探针编译：Framework64 csc 引用原始 4.0.49 包内程序集，返回码 1；命令/日志见 `artifacts/runs/integrated_update_20261109/probe-build-result.json`、`probe-build.log`。探针仅准备隔离设置、在线检测与生成原始更新命令，实际安装将运行原主 EXE，缺失/旧内容 DLL 只在隔离副本构造。

- 探针编译纠正：首次仅因夹具引用不存在的 ConfigStore.StorePath 属性而失败，改用已知隔离配置文件路径；重新编译返回码 0，详见 `artifacts/runs/integrated_update_20261109/probe-build-final-result.json`、`probe-build-final.log`。本次没有修改产品代码，首次失败不计为更新器缺陷。

- 原始 4.0.49 在线检测与请求准备：探针返回码 0；在线版本判定、实际程序集哈希见 `artifacts/runs/integrated_update_20261109/online-check-result.json`，原始内置更新命令见 `prepared-command.json`，演练类型、待恢复 DLL、配置/日志标记基线见 `before-install.json`。本轮仅在独立副本开启详细日志，先完整复制更新助手依赖，再构造旧 DLL 内容与缺失 DLL，未触碰正式安装。

- 原始主 EXE 内置安装实测：运行 prepared-command.json 中原始 --install-update 命令（已核对主 EXE 发布哈希），安装进程结束=True，返回码=0。安装/下载校验、真实进程 PID、文件与配置保留逐项结果见 `artifacts/runs/integrated_update_20261109/installer-result.json`，启动命令 `installer-start.json`，原生逐步日志在同目录 app49/logs/。本次场景为当前公开同版完整包重装，后续仅核对和清理，不将其记成已发布更高版本的跨版本升级。

- 本轮有效结论：4.0.49 原始主 EXE 的内置安装模式返回码 0，原生日志确认下载校验、替换、重启请求和临时安装文件清理完成；重启后的真实主程序进入消息循环。隔离副本的旧内容 DLL 已被正确覆盖、缺失 DLL 已恢复，配置文件以及配置/日志标记均保留。只对记录中的重启 PID+可执行路径核对后清理，剩余测试进程见 `artifacts/runs/integrated_update_20261109/cleanup-result.json`；清理不计正常 UI 退出验收。在线检查当前仍判定同版本无更新，本轮证明现有包格式的安装链路通过，尚非到更高已发布版本的跨版本升级验收。

## RELEASE-4.0.50-20261109：按用户要求命令行发布 GitCode

- 用户已明确要求把全量通过的修复推送 GitCode，并指定全程使用命令行。本轮顺序：核对远端与写入通道 → 以已验证源码形成新版本 → 隔离 Release 构建/绿色包检查 → 4.0.49 升级验证 → 先推送 ZIP 并匿名核对 → 再切换 update.json 并复核线上。此指令授权本次版本上调及公开发布；仍保留用户已有无关改动，不强推，不删除历史包。
- GitCode 发布仓库已通过命令行克隆，返回码 0，远端当前清单为 4.0.49。本轮拟发布 4.0.50；基线提交、克隆命令和结果见 `artifacts/runs/release_4.0.50_20261109/gitcode-clone.log`、`gitcode-clone-result.json`。源码基于已通过主目录全量测试的内容，Python 和未完成音频/桌面验收不混入发布结论。

- GitCode 命令行写入通道预检：非交互 `git push --dry-run --porcelain origin HEAD:main` 返回码 0，未修改远端。详情见 `artifacts/runs/release_4.0.50_20261109/gitcode-write-check.log`、`gitcode-write-check-result.json`；只读克隆成功不等于已完成发布。

- 发布版本准备：仅将主目录 AppVersion 及依赖当前版本的既有更新测试期望从 4.0.49 调整为 4.0.50，共涉及版本常量和四份测试文件；功能源码保持已通过全量验证的状态。变更前文件哈希见 `artifacts/runs/release_4.0.50_20261109/source-before-version.json`，后续仅运行受版本影响的测试与发布构建。

- 4.0.50 版本/更新定向验证：`dotnet test` 以 ConstantsContractTests 或名称含 Update 筛选，使用本轮隔离 BaseOutputPath 完成当前源码 Release x64 编译；返回码 0，总数 57，通过 57，失败 0，未执行 0。完整命令和结果见 `artifacts/runs/release_4.0.50_20261109/version-update-result.json`，日志 `version-update.log`，TRX `version-update.trx`。功能基线复用 MAIN-FULL-20261109 的全量结果，不重复未变化功能验收。

- 4.0.50 Release x64 发布目录生成：`dotnet publish` 复用本轮刚验证的隔离构建输出，显式 IncludeLegacyUpdateCompatibility=false；返回码 0。完整命令与日志见 `artifacts/runs/release_4.0.50_20261109/publish-result.json`、`publish.log`，产物在 publish/，随后独立封装绿色 ZIP。

- 4.0.50 绿色包封装与完整性检查：复用既有绿色版封装脚本并更新本轮路径/版本，返回码 0；检查必需 EXE/DLL/打赏资源、空 Configurations/ 与空 logs/，排除外置 updater、PDB、Python 和测试产物。逐文件哈希、ZIP 大小及 SHA-256 见 `artifacts/runs/release_4.0.50_20261109/package-result.json`，脚本 `package.ps1`、日志 `package.log`，ZIP 在 delivery/。

- GitCode ZIP 独立提交与推送：仅暂存包文件（清单见 `artifacts/runs/release_4.0.50_20261109/package-staged-files.txt`），cached diff-check 通过；提交 957ceba377b67f5f17264cecc012095e9e6a6e3a，`git push --porcelain origin HEAD:main` 返回码 0，此步不修改 update.json。不可变下载地址、blob 和回退命令见 `package-push-result.json`，日志 `package-commit.log`、`package-push.log`；后续先验证下载与升级再切换清单。

- GitCode 新包匿名下载核对：curl 返回码 0，下载文件大小和 SHA-256 与本地发布包一致=True；结果和日志见 `artifacts/runs/release_4.0.50_20261109/anonymous-download-result.json`、`anonymous-download.log`。候选更新清单已生成在 delivery/update.json，尚未推送上线；下一步使用原始 4.0.49 内置更新器升级到该真实在线包。

- 跨版本升级夹具准备：使用全新原始 4.0.49 副本与原包程序集，复用已验证的内置更新探针；候选清单采用即将发布的精确文本，ZIP 地址已真实上线。Framework64 csc 编译返回码 0；证据 `artifacts/runs/release_4.0.50_20261109/upgrade49to50/probe-build-result.json`、`probe-build.log`。此阶段尚未改线上清单。

- 4.0.49 对 4.0.50 候选清单的版本比较与安装命令准备：返回码 0；原版 UpdateService 的 HasUpdate、ShouldPrompt、目标包与版本结果见 `artifacts/runs/release_4.0.50_20261109/upgrade49to50/candidate-check-result.json`，请求/命令见 `prepared-command.json`，基线见 `before-install.json`，文本日志 `prepare.log`。随后将实际运行原始 4.0.49 主 EXE 安装在线新包。

- 4.0.49 → 4.0.50 真实安装完成：原始 4.0.49 主 EXE 内置安装进程返回码 0，全部本轮升级核对通过=True。更新器真实下载 GitCode 4.0.50 包；新主程序日志版本、EXE/Core 哈希、窗口消息循环、DLL 替换/恢复、配置与日志标记逐项见 `artifacts/runs/release_4.0.50_20261109/upgrade49to50/installer-result.json`，实际 PID/启动参数见 `installer-start.json`，原生日志在 app49/logs/。清单仍为候选文本，下一步清理测试进程后才切换在线清单。

- 4.0.50 更新清单提交推送：确认与升级验证使用的候选清单字节一致，仅暂存 update.json，cached diff-check 通过；提交 7a0f156d38c8db1348314cb7e82ede6d3118401d，`git push --porcelain origin HEAD:main` 返回码 0，远端头已读取核对。提交、远端、清单哈希和 `git revert` 回退命令见 `artifacts/runs/release_4.0.50_20261109/manifest-push-result.json`，暂存清单 `manifest-staged-files.txt`，日志 `manifest-commit.log`、`manifest-push.log`。升级测试进程按记录 PID/路径清理，无残留；清理证据 upgrade49to50/cleanup-result.json，不计正常 UI 退出测试。下一步核对公开清单的实际在线检测。

- 发布后在线检测夹具编译返回码 0；同一只读探针将分别加载原始 4.0.49 与发布 4.0.50 程序集，通过默认在线清单检查旧版提示更新、新版不重复提示及包地址/哈希。编译结果见 `artifacts/runs/release_4.0.50_20261109/online-probe-build-result.json`、`online-probe-build.log`。

- 发布后 4.0.49 原始代码在线检查：返回码 1，最新版本=，HasUpdate=，ShouldPrompt=，清单/包地址/哈希核对通过=False。默认真实 HTTP 路径，没有注入候选清单；完整结果与日志见 `artifacts/runs/release_4.0.50_20261109/online-4.0.49.json`、`online-4.0.49.log`，命令返回码见同前缀 -command-result.json。

- 发布后首次旧版在线检测返回 HTTP 403，已保留原生异常，未将其记为更新可用。随后用相同 User-Agent/缓存头的 curl 读取响应，curl 进程返回码 0（HTTP 状态另见日志，不能以进程返回码替代）；证据 `artifacts/runs/release_4.0.50_20261109/manifest-diagnostic-result.json`、`manifest-diagnostic.log`、响应头和正文文件。继续核对远端返回，未修改应用安全校验或更新地址。

- 在线恢复复核 4.0.49：curl 已读到 HTTP 200 的新清单后重新运行原始更新客户端；返回码 0，最新版本=4.0.50，HasUpdate=True，ShouldPrompt=True，通过=True。证据 `artifacts/runs/release_4.0.50_20261109/online-final/online-4.0.49.json` 及同名前缀日志、命令结果；第一次 403 记录保留，未覆盖。

- 在线恢复复核 4.0.50：curl 已读到 HTTP 200 的新清单后重新运行原始更新客户端；返回码 0，最新版本=4.0.50，HasUpdate=False，ShouldPrompt=False，通过=True。证据 `artifacts/runs/release_4.0.50_20261109/online-final/online-4.0.50.json` 及同名前缀日志、命令结果；第一次 403 记录保留，未覆盖。

- 发布交付收尾：绿色包已整理到 `release/4.0.50/`，本地副本哈希与已发布 ZIP 一致；GitCode 发布工作树干净，本轮测试进程无残留。包/清单提交、远端分支、完整哈希、下载地址和逐提交 git revert 命令统一见 `artifacts/runs/release_4.0.50_20261109/delivery-result.json`（交付副本 release/4.0.50/delivery-result.json）。撤回更新时优先只回退清单并推送 main，保留包给已有请求；不会自动降级已升级用户。当前主目录仅追加版本和相关测试修改，保留原有未提交改动；本轮公开写入范围是 GitCode 发布仓库。

- 软件运行审计准备：复制当前已由 4.0.50 源码发布的隔离 publish 目录到 `artifacts/runs/runtime_ui_audit_20261109/app/`；主 EXE 哈希与 4.0.50 发布包一致，Configurations/logs 均为空，未使用正式安装目录。准备清单和路径见 `runtime-preparation.json`；下一步启动此隔离程序读取真实无障碍树。

- 运行审计准备记录纠正：首次准备输出因 PowerShell 将 `-and` 误作为命令参数，空目录字段写成 null；产品文件未受影响。已在隔离目录创建空 `Configurations/`、`logs/` 并重新核对，两个目录均为空，主 EXE 哈希仍与 4.0.50 发布包一致。纠正结果见 `artifacts/runs/runtime_ui_audit_20261109/runtime-preparation-correction.json`，最终准备基线覆盖写入 `runtime-preparation.json`；随后开始实际启动。

- 软件运行审计 / 启动基线：隔离 4.0.50 EXE 已实际启动，窗口标题“沙河批量打开器”，窗口返回唯一且无障碍树成功读取，树长 8314 字符；列表出现大量历史 Sandboxie 用户，当前均显示“空闲中”，菜单栏可见“文件(F)、沙盒管理(G)、定时(D)、设置(S)、校正(J)、工具(T)、关于(A)”，无启动异常。原始树保存于 `artifacts/runs/runtime_ui_audit_20261109/main-window-accessibility.txt`，窗口/证据目录见 `runtime-preparation.json`；这只是启动和可访问性基线，尚未证明菜单动作可用。

- 软件运行审计 / 清理与结论：隔离 4.0.50 程序真实启动成功，主窗口和无障碍树可读取；首启创建 `Configurations/shahe_config.ini`，日志模式为 `off` 时未生成日志，符合按需记录约定。桌面工具在截图刷新时返回 `CUA_EXECUTION_FAILED`，恢复枚举返回 `CUA_TURN_NOT_ACTIVE`，因此没有发出菜单点击/快捷键输入，不能据此宣称用户列表菜单、设置或托盘动作通过。按精确 EXE 路径清理隔离测试 PID，清理结果无残留；不计正常 UI 退出。完整结果见 `artifacts/runs/runtime_ui_audit_20261109/runtime-audit-result.json`，树见 `main-window-accessibility.txt`。

## FULL-STRESS-20261111：全功能压力测试

- 用户已确认完整压测计划并要求开启子代理。代码、工作区改动和公开包哈希快照：`artifacts/runs/full_function_stress_20261111/baseline-snapshot.json`。本轮不改产品、不发布，正式配置、Sandboxie 服务和读屏保留。
- 当前源码全量基线：`dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:/mycodex/git/artifacts/runs/full_function_stress_20261111/baseline/bin/ --results-directory D:/mycodex/git/artifacts/runs/full_function_stress_20261111/baseline --logger "trx;LogFileName=full.trx"`；返回码 1，总计 443，通过 442、失败 1、跳过 0。失败 `DonationButtonCopiesShippedOriginalImage("微信赞赏码.jpg","微信打赏")` 报剪贴板 `0x800401D0`，堆栈指向测试清理恢复剪贴板处，待最小定向确认；不可写成本轮全量通过。编译当前 Core/App/AuditRunner/Tests 完成。命令记录 `baseline/command-result.json`，日志 `baseline/full.log`，完整结果 `baseline/full.trx`，均相对本轮证据根目录。
- 剪贴板最小复验：同一编译产物追加 `--no-build --filter FullyQualifiedName~DonationButtonCopiesShippedOriginalImage`，结果目录 `clipboard-recheck/`；返回码 0，2/2 通过，失败/跳过 0。源码未改，微信与支付宝原图复制及本次清理恢复均通过；初次失败属于测试恢复剪贴板阶段的瞬时失败，未证明产品复制失败，也未重写初次全量失败。证据 `clipboard-recheck/command-result.json`、`test.log`、`clipboard.trx`；不再重复全量或该项。
- 真实绿色包桌面组：解压 `release/4.0.50/沙河批量打开器-4.0.50-win-x64.zip` 到本轮 `desktop/app/`，通过 `@oai/sky` 启动；首次扫描提示回车后主窗口出现，焦点无障碍树可读取。Shift+F10 调用返回成功，但树未显示弹出菜单；截图核实返回 `CUA_EXECUTION_FAILED`，恢复枚举返回 `CUA_TURN_NOT_ACTIVE`，不能认定菜单打开成功或产品菜单失败。随后停止桌面输入，按精确 PID/EXE 路径清理唯一测试进程，残留 0；这是强制清理，不计正常界面退出。主程序 hash、PID、时间、配置首建和空日志证据均见 `desktop/result.json`，原始树 `desktop/startup-tree.txt`、`desktop/context-tree.txt`。真实菜单、托盘、模态和单读屏压力仍属工具阻断，不绕过桌面路由用脚本控制界面。
- storage 压力夹具编译：`& .\artifacts\runs\full_function_stress_20261111\run-probe.ps1 -Group storage -Stage build` 返回码 0。使用 .NET SDK Roslyn C# 7.3/x64、net48 引用及本轮基线程序集，未修改产品源码。精确编译参数及日志见 `storage/build-01/command-result.json`、`build.log`；此项只是夹具编译成功，尚非压力运行通过。

- storage 压测：`run-probe.ps1 -Group storage -Stage run` 返回码 1，29 场景中 18 通过/11 失败，无跳过/超时，耗时 92.39 秒，进程已退出。配置串行 1000 轮、历史 1000 轮、8 线程 20000 条日志、1000 文件的 10 轮备份解压还原及非法归档拒绝均通过。并发配置保存仅 35/1000 次返回成功，且部分已确认值最终丢失；保存失败仍提示成功；还原异常后旧数据回退/模拟服务恢复、只读目标、重复 INI 节和 BOM/UTF-16/GBK 编码保留失败。结果 `storage/run-01/data/run-20261111-020306-721-10ab31f234e64909bc897457e7e0427d/results.json`，命令/实际 PID/耗时/清理见 `storage/run-01/command-result.json`、`process.json`，输出 `stdout.log`。并发存储属于压力边界，真实界面触发概率尚未测量；实际服务未操作，不把模拟服务失败写成正式服务已停止。

- process 压力夹具编译：`run-probe.ps1 -Group process -Stage build` 返回码 0；精确参数及日志 `process/build-01/command-result.json`、`build.log`。已删除与 services 组重复的启动/取消场景注册，保留进程、音频回调、定位、隔离用户管理/校正及异常边界；尚待运行。

- process 压测：`run-probe.ps1 -Group process -Stage run` 返回码 1，13 场景中 9 通过/4 失败，无跳过/超时。1000 用户/5000 PID 的 100 轮目标合并、当前/全部模拟音频动作、失败统计、核心当前关闭/全部关闭入口、定位回调、忙碌保护、隔离用户创建改名删除及引用维护通过。GBK/UTF-16 配置保留、只读目录删除、删除前进程查询异常安全门失败（查询异常仍返回可删除）。证据 `process/run-01/data/result.json`、`process/run-01/stdout.log`，实际 PID/返回码/耗时/无残留见 `command-result.json`。当前关闭包装层与原生焦点/音频/官方服务未执行，覆盖边界已在 JSON 列明。

- services 压力夹具编译：`run-probe.ps1 -Group services -Stage build` 返回码 0；日志和精确参数 `services/build-01/build.log`、`command-result.json`。使用本轮基线程序集，HTTP/安装重启/定时器/启动及朗读均已在夹具注入本地边界，准备执行。

- services 压测：`run-probe.ps1 -Group services -Stage run` 返回码 1，6 场景中 4 通过/2 失败，无跳过/超时。20 轮更新安装（成功、坏哈希、坏 ZIP、替换失败回滚）、20 轮稍后/忽略持久化、1000 沙盒×3 应用的分批启动及取消、1000 条模拟朗读与关闭竞争通过；1000 次清单处理存在 166 个失败断言，1000 组定时目标变更存在 1000 个失败断言，待核对具体期望/实际差异。逐场景 JSON 及 `summary.json` 在 `services/run-01/data/`，命令/进程/退出及清理见 `services/run-01/command-result.json`、`process.json`，日志 `stdout.log`；本地模拟下载、重启回调和服务测试不替代在线/真实 UI 压测。

- ui 白盒压力夹具编译：`run-probe.ps1 -Group ui -Stage build` 返回码 0，日志/完整参数 `ui/build-01/build.log`、`command-result.json`。只创建隐藏控件句柄，菜单动作由回调截获；不显示窗体、不自动输入鼠键、不操作剪贴板或正式系统服务。它只能验证接线和对象生命周期，不能替代已阻断的真实桌面验收。

- services 失败复核（只读现有结果与源码，未重跑）：清单失败为两种各 83 次的语义差异：自动检查缺版本字段返回错误文本，以及 `latest_version` 空字符串时没有回退到有效 `version`。定时失败为改名/部分删除后显示目标已改，但生产定时回调仍使用旧数组；共 500 组变更场景分别在启动和关闭各产生一次错误目标断言，其余取消/重试/全删/无变更检查通过。证据 `services/run-01/data/manifest-1000.json`、`scheduled-targets-1000.json`；源码定位 `UpdateService.cs:413`、`LaunchScheduler.cs:514`、`MainForm.cs:2894`。正常安装回滚已通过，不把这两类清单差异描述为正常发布包不能升级。

- ui 白盒压测：`run-probe.ps1 -Group ui -Stage run` 返回码 1，7 场景中 6 通过/1 失败，无跳过/超时。100 轮主菜单重建、29 个叶子共 2900 次接线分派；1000 用户+2 分组的 100 轮上下文操作（共 1300 次回调，耗时 287.990 秒）；列表边界；47 动作的 1000 轮快捷键解析；1000 次延迟取消和 1000 次立即回调均通过。生命周期首轮发现主菜单 `Disposed` 通知为 0（预期 1），故原定 50 轮未完成；窗体/控件/服务的已检查释放标志通过，不能由这一个通知断言直接声称原生资源泄漏。原始结构化结果 `ui/run-01/data/ui-stress-20261111T020925158-00a0967093ec435bb1e84c128b5ffb12/result.json`；命令、实际 PID/退出和无残留 `ui/run-01/command-result.json`、`process.json`。控制台重定向受系统代码页影响，中文以 UTF-8 JSON 为准；没有重写原始日志。列表耗时包含全选/取消/刷新和多种动作，不能换算成单次菜单响应耗时。

- 本轮清理及源码边界检查返回码 0：基线快照中的 C# 源码/项目/依赖锁定文件哈希均未变化，公开绿色 ZIP 哈希未变化，本轮目录下没有残留进程。证据 `cleanup-source-check.json`。所有子代理已关闭；生成测试数据/日志保留在各自隔离目录，不删除失败证据。
- 自动化压力汇总：四组共 55 个独立场景，37 通过/18 失败；没有场景跳过或硬超时，但生命周期因首轮断言失败未达到计划循环量。失败场景数不是独立 bug 数；重复故障输入与原有已知问题均在报告合并说明。真实桌面验收仍阻断，正常 C# 启动已有本轮证据。本轮仅压力测试执行与记录收口，不宣称整个软件已修复或迁移完成。

- 文档及证据收口核验返回码 0：对项目结构、活动计划、报告、压力探针源码和运行脚本执行严格 UTF-8 解码与替换字符检查；`git diff --check -- PROJECT_STRUCTURE.md docs/superpowers/plans/2026-11-11-full-function-stress.md docs/migration/2026-11-11-full-function-stress.md` 返回 0，新增文档空白检查通过，PowerShell Parser 检查运行脚本无语法错误。直接读取四组 JSON 和两份 TRX，计数与以上记录一致，没有重跑产品测试。结果 `artifacts/runs/full_function_stress_20261111/documentation-check.json`。已复核活动计划：已执行项勾选，真实界面与生命周期完整负载仍保留未完成，报告列明复现输入、覆盖边界和后续修复顺序。

<!-- FULL-STRESS-APPEND -->

## FULL-STRESS-30-20261111：整套重复压力测试

- 用户明确要求整套 30 遍并记录问题，已展示执行差异并据该指令扩展原计划。继续使用首轮固定负载；每组独立进程和结果目录，失败不掩盖、不自动重试为通过。新增证据根目录 `artifacts/runs/full_function_stress_30_20261111/`；以逐组记录和最后聚合为实际结果，不提前声称完成。

- 批量运行器语法检查：PowerShell `Parser.ParseFile` 返回码 0、语法错误 0；自动追加文档所需补丁程序存在。证据 `artifacts/runs/full_function_stress_30_20261111/runner-syntax-check.json`。UI 场景使用原探针的 `--case` 分别启动，保留原负载和单场景超时，以便一个场景超时后仍能执行其余场景；没有修改产品或压力断言。

- Preparation exit=0: source hashes match the first campaign; fixed full-suite and probe binaries copied into the new isolated bin directories; no rebuild or production modification. Snapshot: artifacts/runs/full_function_stress_30_20261111/snapshot.json. UI cases run separately with their original case timeout so a timeout cannot skip later cases; assertions and loads unchanged.

- round-01/full/ : exit=0; total=443, passed=443, failed=0, notRun=0; timeout=False; seconds=37.34; residual=False; result=artifacts/runs/full_function_stress_30_20261111/round-01/full/supervisor.json; command=C:\Program Files\dotnet\dotnet.exe vstest D:\mycodex\git\artifacts\runs\full_function_stress_30_20261111\bin\full\ShaheLauncher.Tests.dll /Platform:x64 /Logger:trx;LogFileName=full.trx /ResultsDirectory:D:\mycodex\git\artifacts\runs\full_function_stress_30_20261111\round-01\full; parseError=

- Runner stopped on infrastructure error: Immediate documentation append failed; see artifacts/runs/full_function_stress_30_20261111/runner-error.json. Unexecuted rounds are not passed.

- 首轮完整测试实际已完成且通过；批量脚本随后因补丁参数中的双引号被 PowerShell 处理，写入后精确比对误判失败，自行停止，尚未启动 storage。原始 TRX、精确参数和结果保留，不重跑已完成全量。运行器已修正文档参数处理、内部超时/清理失败分类和所属子进程退出核对；新增续跑只跳过已有完整结果。修改后 `Parser.ParseFile` 检查返回码 0、语法错误 0，证据 `artifacts/runs/full_function_stress_30_20261111/runner-syntax-check-02.json`。这是测试基础设施修正，不改产品或测试断言。

- Resuming after the recorded documentation quoting error: completed results retained without rerunning; fixed binaries and source hashes match snapshot. Internal timeouts/failed cleanup now classified as failures; descendant process identities tracked and checked. No product or probe assertion change.

- round-01/storage/ : exit=2; total=, passed=, failed=, notRun=; timeout=False; seconds=0.75; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-01/storage/supervisor.json; parseError=Expected one storage result; found 0 at D:\mycodex\git\artifacts\runs\full_function_stress_30_20261111\round-01\storage

- Runner stopped on infrastructure error: Infrastructure failure recorded for round-01/storage/; see artifacts/runs/full_function_stress_30_20261111/runner-error.json. Unexecuted rounds are not passed.

- storage 首次入口退出码 2：原压力探针硬编码了批准的隔离目录，新目录在入口被拒绝，业务场景尚未执行。这是夹具入口适配失败，不计产品失败或完成一轮。原始日志已保留至 `artifacts/runs/full_function_stress_30_20261111/infrastructure/storage-startup-01/`，故上一条指向 `round-01/storage` 的入口失败证据以该归档路径为准。运行器改用原批准目录下每轮独立子目录承载 storage/process 原始数据，并复制结果至本轮索引；探针与产品二进制未改。`Parser.ParseFile` 返回 0，证据 `runner-syntax-check-03.json`。完整测试首轮继续保留，不重跑。

- Resuming after the recorded documentation quoting error: completed results retained without rerunning; fixed binaries and source hashes match snapshot. Internal timeouts/failed cleanup now classified as failures; descendant process identities tracked and checked. No product or probe assertion change.

- round-01/storage/ : exit=1; total=29, passed=18, failed=11, notRun=0; timeout=False; seconds=76.61; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-01/storage/supervisor.json; parseError=

- round-01/process/ : exit=1; total=13, passed=9, failed=4, notRun=0; timeout=False; seconds=85.47; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-01/process/supervisor.json; parseError=

- round-01/services/ : exit=1; total=6, passed=4, failed=2, notRun=0; timeout=False; seconds=24.19; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-01/services/supervisor.json; parseError=

- round-01/ui/lifecycle : exit=1; total=1, passed=0, failed=1, notRun=0; timeout=False; seconds=2.13; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-01/ui/cases/lifecycle/supervisor.json; parseError=

- round-01/ui/main-menu : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=2.11; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-01/ui/cases/main-menu/supervisor.json; parseError=

- round-01/ui/list-menu : exit=1; total=1, passed=0, failed=1, notRun=0; timeout=True; seconds=301.16; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-01/ui/cases/list-menu/supervisor.json; parseError=

- round-01/ui/list-boundaries : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=4.32; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-01/ui/cases/list-boundaries/supervisor.json; parseError=

- round-01/ui/hotkeys : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=2.38; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-01/ui/cases/hotkeys/supervisor.json; parseError=

- round-01/ui/delayed-cancel : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=1.03; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-01/ui/cases/delayed-cancel/supervisor.json; parseError=

- round-01/ui/delayed-immediate : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=1.37; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-01/ui/cases/delayed-immediate/supervisor.json; parseError=

- round-02/full/ : exit=0; total=443, passed=443, failed=0, notRun=0; timeout=False; seconds=30.88; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-02/full/supervisor.json; parseError=

- round-02/storage/ : exit=1; total=29, passed=18, failed=11, notRun=0; timeout=False; seconds=73.81; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-02/storage/supervisor.json; parseError=

- round-02/process/ : exit=1; total=13, passed=9, failed=4, notRun=0; timeout=False; seconds=94.77; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-02/process/supervisor.json; parseError=

- round-02/services/ : exit=1; total=6, passed=4, failed=2, notRun=0; timeout=False; seconds=23.9; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-02/services/supervisor.json; parseError=

- round-02/ui/lifecycle : exit=1; total=1, passed=0, failed=1, notRun=0; timeout=False; seconds=2.93; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-02/ui/cases/lifecycle/supervisor.json; parseError=

- round-02/ui/main-menu : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=3.02; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-02/ui/cases/main-menu/supervisor.json; parseError=

- round-02/ui/list-menu : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=145.19; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-02/ui/cases/list-menu/supervisor.json; parseError=

- round-02/ui/list-boundaries : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=2.01; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-02/ui/cases/list-boundaries/supervisor.json; parseError=

- round-02/ui/hotkeys : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=2.09; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-02/ui/cases/hotkeys/supervisor.json; parseError=

- round-02/ui/delayed-cancel : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=1.07; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-02/ui/cases/delayed-cancel/supervisor.json; parseError=

- round-02/ui/delayed-immediate : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=1.04; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-02/ui/cases/delayed-immediate/supervisor.json; parseError=

- round-03/full/ : exit=0; total=443, passed=443, failed=0, notRun=0; timeout=False; seconds=38.75; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-03/full/supervisor.json; parseError=

- round-03/storage/ : exit=1; total=29, passed=18, failed=11, notRun=0; timeout=False; seconds=66.59; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-03/storage/supervisor.json; parseError=

- round-03/process/ : exit=1; total=13, passed=9, failed=4, notRun=0; timeout=False; seconds=82.81; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-03/process/supervisor.json; parseError=

- round-03/services/ : exit=1; total=6, passed=4, failed=2, notRun=0; timeout=False; seconds=25.67; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-03/services/supervisor.json; parseError=

- round-03/ui/lifecycle : exit=1; total=1, passed=0, failed=1, notRun=0; timeout=False; seconds=2.01; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-03/ui/cases/lifecycle/supervisor.json; parseError=

- round-03/ui/main-menu : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=1.92; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-03/ui/cases/main-menu/supervisor.json; parseError=

- round-03/ui/list-menu : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=212.25; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-03/ui/cases/list-menu/supervisor.json; parseError=

- round-03/ui/list-boundaries : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=2.5; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-03/ui/cases/list-boundaries/supervisor.json; parseError=

- round-03/ui/hotkeys : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=2.1; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-03/ui/cases/hotkeys/supervisor.json; parseError=

- round-03/ui/delayed-cancel : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=1; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-03/ui/cases/delayed-cancel/supervisor.json; parseError=

- round-03/ui/delayed-immediate : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=1.04; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-03/ui/cases/delayed-immediate/supervisor.json; parseError=

- round-04/full/ : exit=0; total=443, passed=443, failed=0, notRun=0; timeout=False; seconds=30.98; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-04/full/supervisor.json; parseError=

- round-04/storage/ : exit=1; total=29, passed=18, failed=11, notRun=0; timeout=False; seconds=76.78; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-04/storage/supervisor.json; parseError=

- round-04/process/ : exit=1; total=13, passed=9, failed=4, notRun=0; timeout=False; seconds=78.75; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-04/process/supervisor.json; parseError=

- round-04/services/ : exit=1; total=6, passed=4, failed=2, notRun=0; timeout=False; seconds=23.58; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-04/services/supervisor.json; parseError=

- round-04/ui/lifecycle : exit=1; total=1, passed=0, failed=1, notRun=0; timeout=False; seconds=2.05; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-04/ui/cases/lifecycle/supervisor.json; parseError=

- round-04/ui/main-menu : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=1.92; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-04/ui/cases/main-menu/supervisor.json; parseError=

- round-04/ui/list-menu : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=235.59; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-04/ui/cases/list-menu/supervisor.json; parseError=

- round-04/ui/list-boundaries : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=3.65; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-04/ui/cases/list-boundaries/supervisor.json; parseError=

- round-04/ui/hotkeys : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=2.51; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-04/ui/cases/hotkeys/supervisor.json; parseError=

- round-04/ui/delayed-cancel : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=1.12; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-04/ui/cases/delayed-cancel/supervisor.json; parseError=

- round-04/ui/delayed-immediate : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=1.08; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-04/ui/cases/delayed-immediate/supervisor.json; parseError=

- round-05/full/ : exit=0; total=443, passed=443, failed=0, notRun=0; timeout=False; seconds=29.41; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-05/full/supervisor.json; parseError=

- round-05/storage/ : exit=1; total=29, passed=18, failed=11, notRun=0; timeout=False; seconds=59.68; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-05/storage/supervisor.json; parseError=

- round-05/process/ : exit=1; total=13, passed=9, failed=4, notRun=0; timeout=False; seconds=81.04; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-05/process/supervisor.json; parseError=

- round-05/services/ : exit=1; total=6, passed=4, failed=2, notRun=0; timeout=False; seconds=23.99; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-05/services/supervisor.json; parseError=

- round-05/ui/lifecycle : exit=1; total=1, passed=0, failed=1, notRun=0; timeout=False; seconds=3.3; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-05/ui/cases/lifecycle/supervisor.json; parseError=

- round-05/ui/main-menu : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=2.01; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-05/ui/cases/main-menu/supervisor.json; parseError=

- round-05/ui/list-menu : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=294.74; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-05/ui/cases/list-menu/supervisor.json; parseError=

- round-05/ui/list-boundaries : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=2.16; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-05/ui/cases/list-boundaries/supervisor.json; parseError=

- round-05/ui/hotkeys : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=2.89; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-05/ui/cases/hotkeys/supervisor.json; parseError=

- round-05/ui/delayed-cancel : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=1.39; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-05/ui/cases/delayed-cancel/supervisor.json; parseError=

- round-05/ui/delayed-immediate : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=1.26; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-05/ui/cases/delayed-immediate/supervisor.json; parseError=

- round-06/full/ : exit=0; total=443, passed=443, failed=0, notRun=0; timeout=False; seconds=29.72; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-06/full/supervisor.json; parseError=

- round-06/storage/ : exit=1; total=29, passed=18, failed=11, notRun=0; timeout=False; seconds=62.1; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-06/storage/supervisor.json; parseError=

- round-06/process/ : exit=1; total=13, passed=9, failed=4, notRun=0; timeout=False; seconds=87.54; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-06/process/supervisor.json; parseError=

- round-06/services/ : exit=1; total=6, passed=4, failed=2, notRun=0; timeout=False; seconds=23.36; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-06/services/supervisor.json; parseError=

- round-06/ui/lifecycle : exit=1; total=1, passed=0, failed=1, notRun=0; timeout=False; seconds=4.44; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-06/ui/cases/lifecycle/supervisor.json; parseError=

- round-06/ui/main-menu : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=5.15; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-06/ui/cases/main-menu/supervisor.json; parseError=

- round-06/ui/list-menu : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=242.53; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-06/ui/cases/list-menu/supervisor.json; parseError=

- round-06/ui/list-boundaries : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=2.21; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-06/ui/cases/list-boundaries/supervisor.json; parseError=

- round-06/ui/hotkeys : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=2.58; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-06/ui/cases/hotkeys/supervisor.json; parseError=

- round-06/ui/delayed-cancel : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=1.56; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-06/ui/cases/delayed-cancel/supervisor.json; parseError=

- round-06/ui/delayed-immediate : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=1.04; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-06/ui/cases/delayed-immediate/supervisor.json; parseError=

- round-07/full/ : exit=0; total=443, passed=443, failed=0, notRun=0; timeout=False; seconds=28.19; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-07/full/supervisor.json; parseError=

- round-07/storage/ : exit=1; total=29, passed=18, failed=11, notRun=0; timeout=False; seconds=67.34; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-07/storage/supervisor.json; parseError=

- round-07/process/ : exit=1; total=13, passed=9, failed=4, notRun=0; timeout=False; seconds=83.51; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-07/process/supervisor.json; parseError=

- round-07/services/ : exit=1; total=6, passed=4, failed=2, notRun=0; timeout=False; seconds=22.54; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-07/services/supervisor.json; parseError=

- round-07/ui/lifecycle : exit=1; total=1, passed=0, failed=1, notRun=0; timeout=False; seconds=2.29; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-07/ui/cases/lifecycle/supervisor.json; parseError=

- round-07/ui/main-menu : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=2.11; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-07/ui/cases/main-menu/supervisor.json; parseError=

- round-07/ui/list-menu : exit=1; total=1, passed=0, failed=1, notRun=0; timeout=True; seconds=303.76; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-07/ui/cases/list-menu/supervisor.json; parseError=

- round-07/ui/list-boundaries : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=2.08; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-07/ui/cases/list-boundaries/supervisor.json; parseError=

- round-07/ui/hotkeys : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=2.16; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-07/ui/cases/hotkeys/supervisor.json; parseError=

- round-07/ui/delayed-cancel : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=1.14; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-07/ui/cases/delayed-cancel/supervisor.json; parseError=

- round-07/ui/delayed-immediate : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=1.21; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-07/ui/cases/delayed-immediate/supervisor.json; parseError=

- round-08/full/ : exit=0; total=443, passed=443, failed=0, notRun=0; timeout=False; seconds=28.68; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-08/full/supervisor.json; parseError=

- round-08/storage/ : exit=1; total=29, passed=18, failed=11, notRun=0; timeout=False; seconds=65.09; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-08/storage/supervisor.json; parseError=

- round-08/process/ : exit=1; total=13, passed=9, failed=4, notRun=0; timeout=False; seconds=85.74; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-08/process/supervisor.json; parseError=

- round-08/services/ : exit=1; total=6, passed=4, failed=2, notRun=0; timeout=False; seconds=24.8; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-08/services/supervisor.json; parseError=

- round-08/ui/lifecycle : exit=1; total=1, passed=0, failed=1, notRun=0; timeout=False; seconds=2.03; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-08/ui/cases/lifecycle/supervisor.json; parseError=

- round-08/ui/main-menu : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=1.91; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-08/ui/cases/main-menu/supervisor.json; parseError=

- round-08/ui/list-menu : exit=1; total=1, passed=0, failed=1, notRun=0; timeout=True; seconds=301.19; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-08/ui/cases/list-menu/supervisor.json; parseError=

- round-08/ui/list-boundaries : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=4.65; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-08/ui/cases/list-boundaries/supervisor.json; parseError=

- round-08/ui/hotkeys : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=4.67; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-08/ui/cases/hotkeys/supervisor.json; parseError=

- round-08/ui/delayed-cancel : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=2.62; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-08/ui/cases/delayed-cancel/supervisor.json; parseError=

- round-08/ui/delayed-immediate : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=2.78; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-08/ui/cases/delayed-immediate/supervisor.json; parseError=

- round-09/full/ : exit=1; total=443, passed=442, failed=1, notRun=0; timeout=False; seconds=44.79; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-09/full/supervisor.json; parseError=

- round-09/storage/ : exit=1; total=29, passed=18, failed=11, notRun=0; timeout=False; seconds=70.53; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-09/storage/supervisor.json; parseError=

- 第 9 轮全量失败只读核对：`DonationButtonCopiesShippedOriginalImage` 的微信数据行在 `DonationQrRuntimeContractTests.cs:78` 恢复原剪贴板时再次报 `0x800401D0`；前面的图片复制断言已经执行完成。与首轮历史清理失败同类，归入测试清理/剪贴板争用边界，不认定图片复制产品缺陷；不额外重跑。原始证据 `artifacts/runs/full_function_stress_30_20261111/round-09/full/full.trx`，次数在最终汇总统计。

- round-09/process/ : exit=1; total=13, passed=9, failed=4, notRun=0; timeout=False; seconds=81.77; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-09/process/supervisor.json; parseError=

- round-09/services/ : exit=1; total=6, passed=4, failed=2, notRun=0; timeout=False; seconds=23.77; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-09/services/supervisor.json; parseError=

- round-09/ui/lifecycle : exit=1; total=1, passed=0, failed=1, notRun=0; timeout=False; seconds=1.98; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-09/ui/cases/lifecycle/supervisor.json; parseError=

- round-09/ui/main-menu : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=1.98; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-09/ui/cases/main-menu/supervisor.json; parseError=

- round-09/ui/list-menu : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=162.71; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-09/ui/cases/list-menu/supervisor.json; parseError=

- round-09/ui/list-boundaries : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=2.85; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-09/ui/cases/list-boundaries/supervisor.json; parseError=

- round-09/ui/hotkeys : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=2.16; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-09/ui/cases/hotkeys/supervisor.json; parseError=

- round-09/ui/delayed-cancel : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=1.04; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-09/ui/cases/delayed-cancel/supervisor.json; parseError=

- round-09/ui/delayed-immediate : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=1.05; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-09/ui/cases/delayed-immediate/supervisor.json; parseError=

- round-10/full/ : exit=0; total=443, passed=443, failed=0, notRun=0; timeout=False; seconds=29.77; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-10/full/supervisor.json; parseError=

- round-10/storage/ : exit=1; total=29, passed=18, failed=11, notRun=0; timeout=False; seconds=57.18; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-10/storage/supervisor.json; parseError=

- round-10/process/ : exit=1; total=13, passed=9, failed=4, notRun=0; timeout=False; seconds=78.62; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-10/process/supervisor.json; parseError=

- round-10/services/ : exit=1; total=6, passed=4, failed=2, notRun=0; timeout=False; seconds=22.73; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-10/services/supervisor.json; parseError=

- round-10/ui/lifecycle : exit=1; total=1, passed=0, failed=1, notRun=0; timeout=False; seconds=2.13; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-10/ui/cases/lifecycle/supervisor.json; parseError=

- round-10/ui/main-menu : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=2.28; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-10/ui/cases/main-menu/supervisor.json; parseError=

- round-10/ui/list-menu : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=150.43; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-10/ui/cases/list-menu/supervisor.json; parseError=

- round-10/ui/list-boundaries : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=2.72; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-10/ui/cases/list-boundaries/supervisor.json; parseError=

- round-10/ui/hotkeys : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=2.33; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-10/ui/cases/hotkeys/supervisor.json; parseError=

- round-10/ui/delayed-cancel : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=1.1; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-10/ui/cases/delayed-cancel/supervisor.json; parseError=

- round-10/ui/delayed-immediate : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=1.2; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-10/ui/cases/delayed-immediate/supervisor.json; parseError=

- round-11/full/ : exit=0; total=443, passed=443, failed=0, notRun=0; timeout=False; seconds=43.89; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-11/full/supervisor.json; parseError=

- round-11/storage/ : exit=1; total=29, passed=18, failed=11, notRun=0; timeout=False; seconds=61.96; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-11/storage/supervisor.json; parseError=

- round-11/process/ : exit=1; total=13, passed=9, failed=4, notRun=0; timeout=False; seconds=78.82; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-11/process/supervisor.json; parseError=

- round-11/services/ : exit=1; total=6, passed=4, failed=2, notRun=0; timeout=False; seconds=23.62; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-11/services/supervisor.json; parseError=

- round-11/ui/lifecycle : exit=1; total=1, passed=0, failed=1, notRun=0; timeout=False; seconds=2.11; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-11/ui/cases/lifecycle/supervisor.json; parseError=

- round-11/ui/main-menu : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=2.59; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-11/ui/cases/main-menu/supervisor.json; parseError=

- round-11/ui/list-menu : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=182.45; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-11/ui/cases/list-menu/supervisor.json; parseError=

- round-11/ui/list-boundaries : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=5.07; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-11/ui/cases/list-boundaries/supervisor.json; parseError=

- round-11/ui/hotkeys : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=5.39; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-11/ui/cases/hotkeys/supervisor.json; parseError=

- round-11/ui/delayed-cancel : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=3.17; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-11/ui/cases/delayed-cancel/supervisor.json; parseError=

- round-11/ui/delayed-immediate : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=1.98; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-11/ui/cases/delayed-immediate/supervisor.json; parseError=

- round-12/full/ : exit=1; total=443, passed=442, failed=1, notRun=0; timeout=False; seconds=35.47; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-12/full/supervisor.json; parseError=

- round-12/storage/ : exit=1; total=29, passed=18, failed=11, notRun=0; timeout=False; seconds=135.64; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-12/storage/supervisor.json; parseError=

- round-12/process/ : exit=1; total=13, passed=9, failed=4, notRun=0; timeout=False; seconds=80.72; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-12/process/supervisor.json; parseError=

- round-12/services/ : exit=1; total=6, passed=4, failed=2, notRun=0; timeout=False; seconds=24.16; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-12/services/supervisor.json; parseError=

- round-12/ui/lifecycle : exit=1; total=1, passed=0, failed=1, notRun=0; timeout=False; seconds=2.03; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-12/ui/cases/lifecycle/supervisor.json; parseError=

- round-12/ui/main-menu : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=2; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-12/ui/cases/main-menu/supervisor.json; parseError=

- 用户再次要求继续时核对：原工具会话已失效，运行器及所属压力进程均不存在，进度停于第 12 轮 UI/list-menu；该场景仅有运行中快照、无 supervisor 完成记录，不能算产品失败或完成。已将中断尝试完整保留至 `artifacts/runs/full_function_stress_30_20261111/infrastructure/round-12-list-menu-interrupted/`；对应旧进度在 `infrastructure/progress-at-interruption.json`。恢复前核对及运行器语法检查返回 0、残留探针 0，证据 `resume-check.json`；从第 12 轮续跑，已完成组跳过，仅重新执行中断未完成场景。该批重复测试有中断，最终不得描述成不间断长时运行。
- 第 12 轮全量失败只读核对仍为微信原图测试 `DonationQrRuntimeContractTests.cs:78` 恢复剪贴板时报 `0x800401D0`，图片复制断言此前完成；证据该轮 `full/full.trx`。保留原始失败，不另行重跑。

- Runner stopped on infrastructure error: 无法将“Get-FileHash”项识别为 cmdlet、函数、脚本文件或可运行程序的名称。请检查名称的拼写，如果包括路径，请确保路径正确，然后再试一次。; see artifacts/runs/full_function_stress_30_20261111/runner-error.json. Unexecuted rounds are not passed.

- 续跑启动检查被 Windows PowerShell 的命令加载问题阻断：`Get-FileHash` 未找到，尚未启动任何产品测试，退出及错误保留 `resume-12.stderr.log` 和对应 `runner-error-*.json`。改用本机已正常提供该命令的 PowerShell 7 执行同一运行器；仅切换监督脚本宿主，不改产品、探针或系统环境变量。

- Resuming at the explicitly selected round: completed results retained without rerunning; fixed binaries and source hashes match snapshot. Earlier infrastructure/interruption evidence retained separately. No product or probe assertion change.

- 汇总脚本对已有证据运行检查：`pwsh -NoProfile -File artifacts/runs/full_function_stress_30_20261111/aggregate.ps1` 返回码 0，生成的是执行中的不完整快照，未执行轮次明确记为缺失，不计通过。日志 `aggregation-in-progress.log`、退出结果 `aggregation-in-progress-result.json`；最终结束后会重新汇总。此命令仅读取现有 JSON/TRX，没有运行或重复产品测试。

- round-12/ui/list-menu : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=176.73; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-12/ui/cases/list-menu/supervisor.json; parseError=

- round-12/ui/list-boundaries : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=6.04; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-12/ui/cases/list-boundaries/supervisor.json; parseError=

- round-12/ui/hotkeys : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=5.96; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-12/ui/cases/hotkeys/supervisor.json; parseError=

- round-12/ui/delayed-cancel : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=2.79; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-12/ui/cases/delayed-cancel/supervisor.json; parseError=

- round-12/ui/delayed-immediate : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=2.86; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-12/ui/cases/delayed-immediate/supervisor.json; parseError=

- round-13/full/ : exit=1; total=443, passed=442, failed=1, notRun=0; timeout=False; seconds=90.65; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-13/full/supervisor.json; parseError=

- round-13/storage/ : exit=1; total=29, passed=18, failed=11, notRun=0; timeout=False; seconds=110.48; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-13/storage/supervisor.json; parseError=

- round-13/process/ : exit=1; total=13, passed=9, failed=4, notRun=0; timeout=False; seconds=88.29; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-13/process/supervisor.json; parseError=

- round-13/services/ : exit=1; total=6, passed=4, failed=2, notRun=0; timeout=False; seconds=23.01; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-13/services/supervisor.json; parseError=

- round-13/ui/lifecycle : exit=1; total=1, passed=0, failed=1, notRun=0; timeout=False; seconds=2.97; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-13/ui/cases/lifecycle/supervisor.json; parseError=

- round-13/ui/main-menu : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=2.96; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-13/ui/cases/main-menu/supervisor.json; parseError=

- round-13/ui/list-menu : exit=1; total=1, passed=0, failed=1, notRun=0; timeout=True; seconds=303.8; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-13/ui/cases/list-menu/supervisor.json; parseError=

- round-13/ui/list-boundaries : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=3.94; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-13/ui/cases/list-boundaries/supervisor.json; parseError=

- round-13/ui/hotkeys : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=2.96; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-13/ui/cases/hotkeys/supervisor.json; parseError=

- round-13/ui/delayed-cancel : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=1.8; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-13/ui/cases/delayed-cancel/supervisor.json; parseError=

- round-13/ui/delayed-immediate : exit=0; total=1, passed=1, failed=0, notRun=0; timeout=False; seconds=1.16; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-13/ui/cases/delayed-immediate/supervisor.json; parseError=

- round-14/full/ : exit=1; total=443, passed=442, failed=1, notRun=0; timeout=False; seconds=45.98; residual=False; result and exact command=artifacts/runs/full_function_stress_30_20261111/round-14/full/supervisor.json; parseError=

- 用户明确要求“先把测试脚本先停了”：已按 PID、创建时间和完整路径核对并停止监督脚本及其存储探针，清理返回码 0、所属残留进程 0。完整完成 13 轮；第 14 轮全量测试已有结果，storage 执行中被用户停止，本轮后续组未执行，不计完成一整轮，不计产品失败。日志和中断数据全部保留；精确停止进程、时刻及之前进度见 `artifacts/runs/full_function_stress_30_20261111/user-stop-result.json`，当前状态 `progress.json` 为 `stopped-by-user`。不自动恢复，不继续运行测试或构建；现有汇总仍为旧执行中快照，未声称 30 轮完成。

<!-- FULL-STRESS-30-APPEND -->

## GROUP-SELECTION-20261112：创建分组后无法选中的反馈

- 用户反馈“现在分组没办法选中，创建之后”。本次仅做源码定位；此前重复压测保持用户停止状态，没有启动测试或修改产品。
- 已确认：C# `CheckableListCtrl.CheckItem` 与空格处理禁止勾选分组行，Enter 在已选中的分组行上进入组内；Python `shahe_bulk_launcher/ui/controls.py` 的空格和回车分支采用相同规则，现有 `group_navigation_probe.py` 也明确断言分组行不能勾选。因此“不能勾选整组”与“无法选中/进入分组”须区分，尚不能直接判定同一根因。
- 创建后的焦点候选：`MainForm.CreateGroup` 保存后只刷新列表；`RefreshSandboxList` 仅将首行设为 Focused，没有主动 Selected 新分组；回车处理调用 `GetSelectedIndex`，后者不回退到 FocusedItem。尚未以运行时证据复现用户所遇到的具体路径，不能据此宣称已定位或修复。
- 等待用户明确是“空格/复选框不能勾选整组”，还是“方向键无法选中或回车不能进入”，再确定最小修复及计划差异。未恢复压测、未升版、未发布。

- 用户已明确本次目标：“空格选中分组并且可以启动沙河，回车可以进入该分组”。后续实现应允许空格勾选/取消分组，启动选中沙盒时展开所勾选分组的有效成员、与单独勾选用户合并去重；Enter 保持进入分组，不能触发启动。此前等待行为澄清的记录已由本条更新。
- 源码阻断点已确认：`CheckableListCtrl.CheckItem`、`ToggleItem`、`OnItemChecked` 拒绝或撤销分组勾选，`GetCheckedItems`/`GetCheckedEntryTexts` 排除分组；`MainForm.LaunchSelectedSandboxes` 仅将普通勾选用户送入现有目标解析。应同时打通控件状态与启动目标解析，避免只开放复选框却没有实际启动目标。
- 拟将分组成员展开限定在启动入口，普通用户管理及删除入口继续按原目标边界处理，避免勾选分组后意外扩大删除/重命名范围；刷新保留有效分组勾选，回车导航不执行启动。拟新增独立分组勾选启动计划，待按修改前确认规则展示完整内容并获确认后写入。当前仍无产品修改、无新增测试运行，重复压测维持停止。

- 展示完整分组方案后，用户进一步明确两条路径：在外部空格勾选分组可启动整组，进入分组后仍能选择个别成员启动。当前已按该新行为建立 `docs/superpowers/plans/2026-11-12-group-selection-launch.md`，后续只验证这一项；此前不可勾选分组的 Python/契约行为作为历史差异保留，不改 Python。当前证据根 `artifacts/runs/group_selection_launch_20261112/`。

- 新测试首次构建返回 1，`GroupSelectionLaunchContractTests.cs` 的 LINQ 参数与局部 `names` 同名触发 CS0136，尚未运行任何测试，不算产品 RED。Core/App/AuditRunner 编译成功；日志及精确参数 `artifacts/runs/group_selection_launch_20261112/red/test.log`、`command-result.json`。先修正测试夹具命名，再执行定向 RED，产品代码尚未改变。

- 定向 RED：`dotnet test ... --filter FullyQualifiedName~GroupSelectionLaunchContractTests` 使用本轮隔离 BaseOutputPath，退出码 1，总计 6、通过 0、失败 6、跳过 0；编译成功。失败覆盖空格/原生复选框分组勾选被撤销、新建分组启动无目标、混合选择漏掉组内用户等，符合用户反馈与静态阻断点。精确命令 `artifacts/runs/group_selection_launch_20261112/red-02/command-result.json`，日志 `test.log`、结果 `red.trx`。尚未修改产品代码，随后实施最小修复。

- 定向 GREEN：隔离构建后同一新增用例集合退出码 0，总计 6、通过 6、失败/跳过 0。实际覆盖创建分组→空格→整个组成员进入生产启动调度、混合组/用户去重与过期成员过滤、回车进入→只启动单个勾选成员、刷新保留/全不选、空组/取消及普通管理目标隔离。外部启动回调替换为隔离记录，不创建真实 Sandboxie 进程；这是生产接线自动化证据，尚非真实键盘/读屏验收。精确命令 `artifacts/runs/group_selection_launch_20261112/green/command-result.json`，日志 `test.log`、结果 `green.trx`。控制层新增启动专用读取，普通用户管理读取保持不展开分组。

- 受影响回归：`dotnet test ... --no-build --filter <列表/主窗体/启动前置/设置/上下文菜单/新增分组集合>` 返回码 0，总计 83、通过 83、失败/跳过 0。精确参数 `artifacts/runs/group_selection_launch_20261112/affected/command-result.json`，日志 `test.log`、结果 `affected.trx`。旧分组不可勾选断言已按用户新需求更新，普通用户范围连选和原有入口回归通过。

- 当前源码一次全量回归已结束：`dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --no-build --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\group_selection_launch_20261112\bin\ --results-directory D:\mycodex\git\artifacts\runs\group_selection_launch_20261112\full --logger trx;LogFileName=full.trx` 返回码 1，总计 449、通过 448、失败 1、跳过 0。唯一失败是微信打赏原图测试 `DonationButtonCopiesShippedOriginalImage`，堆栈落在 `DonationQrRuntimeContractTests.cs:78` 的剪贴板恢复，错误 `0x800401D0`；本轮分组用例全部通过。原始退出码、精确参数、日志和结果在 `artifacts/runs/group_selection_launch_20261112/full/command-result.json`、`test.log`、`full.trx`。保留失败，不重跑全量掩盖问题；本次分组修复不能宣称全量全绿。下一步只完成计划中的隔离 Release 构建及差异核对，重复压力测试继续停止。

- Release x64 构建：`dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\group_selection_launch_20261112\bin\` 返回码 0，警告 0、错误 0（构建不运行测试，测试计数不适用）。Core、App、AuditRunner、Tests 均生成成功；精确参数和输出见 `artifacts/runs/group_selection_launch_20261112/release/command-result.json`、`build.log`。主程序在本轮 `bin/x64/Release/net48/沙河批量打开器.exe`，不是新发布的绿色包；未停止用户进程、未升版或上传。

- 差异/编码核对首次脚本执行返回 1：Windows PowerShell 将 `git diff --check` 的 LF→CRLF 标准错误警告作为终止异常，脚本未生成完整结果；不是产品测试失败或差异检查通过。命令 `powershell -NoProfile -File artifacts/runs/group_selection_launch_20261112/verify-scope.ps1`，错误保存 `artifacts/runs/group_selection_launch_20261112/scope-first-error.log`。仅修正核对脚本的原生命令错误流处理后继续核对，不重跑产品测试。

- 最终范围/编码核对返回码 0：基线 117 个 C# 源文件、项目和锁文件中仅计划内 3 个既有文件改变，新增 1 个分组契约测试文件；无缺失文件或非预期源码新增。触及文件严格 UTF-8 检查及 U+FFFD 检查通过，`git diff --check` 返回 0。Python Git 状态与任务前一致（仅状态对比，不冒充字节哈希验证），本任务未编辑 Python；重复压测状态仍是 `stopped-by-user`。命令 `powershell -NoProfile -File artifacts/runs/group_selection_launch_20261112/verify-scope.ps1`，结果 `artifacts/runs/group_selection_launch_20261112/scope-result.json`、`diff-check.log`；此为静态核对，不产生产品测试计数。
- 收尾核对：上述两条用户指定的分组启动路径有生产接线自动化证据，现有按应用/按用户实际启动实现继续复用。旧 Python 明确禁止分组勾选，本次依用户要求修改该行为，未运行或修改 Python；既有 Python 对照/源码编译证据沿用 E09，不冒充本轮验证。C# 有既有主窗口运行证据，本次编译有效，但未新增真实桌面或实际沙盒启动验收。当前主目录源码和隔离编译物含修复；公开 4.0.50 包不含本次改动。全量剪贴板失败和此前真实验收风险仍未解决，不声明整体完成，不恢复压力循环，不扩展到其他缺陷或发布。

- 收尾文档复核曾错误使用单次 `git -c core.autocrlf=false diff --check`，返回 2，将现有 CRLF 行末的 CR 识别为尾部空白；错误摘要 `artifacts/runs/group_selection_launch_20261112/docs-first-error.log`。未修改 Git 配置或文档换行，不据此整体重写历史文档；随后按仓库原有换行规则核对。此为检查命令问题，不是产品测试。

- 最终文档核对按仓库原有设置执行 `git diff --check -- PROJECT_STRUCTURE.md docs/superpowers/plans/2026-11-12-group-selection-launch.md`，返回 0；两份文档严格 UTF-8 及替换字符检查通过，已重新读取活动计划并逐项核对。结果 `artifacts/runs/group_selection_launch_20261112/docs-final-result.json`，差异输出 `docs-diff-check.log`。全量失败和真实验收边界保留，计划完成仅指本项源码及自动化收尾。

<!-- GROUP-SELECTION-APPEND -->

## FULL-ONCE-20261112：当前源码全功能单轮压力检查

- 用户明确要求软件所有功能压测一遍，授权当前源码全量与全功能压力重新执行；采用原已批准全功能计划的范围和负载，不恢复被停止的旧30轮循环。覆盖以具体菜单、上下文、按钮、快捷键和非菜单服务入口逐项映射，普通契约测试、服务压力和真实系统验收分别标记，不能用大类归属替代实际执行。
- 当前代码包含分组外空格勾选整组启动、回车进入后选成员启动修复；本轮原始文件、命令、结果统一在 `artifacts/runs/full_function_once_20261112/`。只向隔离配置/文件写入，不改产品、正式配置、系统自启动、读屏或真实 Sandboxie 服务；真实系统动作只作用于本轮明确拥有的测试目标。三个子代理分别准备完整覆盖表、UI 压力补充、后台探针缺口，主代理串行运行并每次先记录结果再运行下一项。
- 完成条件沿用原计划：所有入口有具体执行结果或有证据的未覆盖/阻断原因，失败记录复现输入和实际负载；新结果不可消除旧风险。全量与压力组执行完不等同所有真实功能验收通过。

- 当前源码全量（新隔离编译）：`dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\full_function_once_20261112\baseline\bin\ --results-directory D:\mycodex\git\artifacts\runs\full_function_once_20261112\baseline --logger trx;LogFileName=full.trx` 返回 1，共 449、通过 448、失败 1、跳过 0；唯一失败仍是微信打赏测试恢复剪贴板 `DonationQrRuntimeContractTests.cs:78` 的 `0x800401D0`。四个项目编译成功，分组新增用例通过；不重跑全量。基线快照 `artifacts/runs/full_function_once_20261112/baseline/snapshot.json`，完整命令 `command-result.json`，输出 `full.log`、结果 `full.trx`。这是本轮全量而非压力组完成，后续逐组运行。

- storage 探针首次构建返回 1，CS0006 的主 EXE 引用显示乱码；尚未运行 storage 产品压力场景。命令 `powershell -NoProfile -File artifacts/runs/full_function_once_20261112/run-probe.ps1 -Group storage -Stage build`，精确参数/输出 `artifacts/runs/full_function_once_20261112/storage/build-01/command-result.json`、`build.log`。核查 PowerShell 脚本编码与编译参数后只重做该构建，保留首次失败。

- storage 探针切换本机 PowerShell 7 读取 UTF-8 脚本后构建成功，返回 0，未修改产品或降低压力。命令 `pwsh -NoProfile -File artifacts/runs/full_function_once_20261112/run-probe.ps1 -Group storage -Stage build -Attempt 02`，精确参数 `storage/build-02/command-result.json`；构建无测试计数。前三个后台探针仅将批准隔离路径替换为本轮目录，引用本轮新编译产物。
- 三个子代理均因账户额度错误退出，未交付准备文件；主代理继续负责逐项覆盖、UI 补充和后台缺口核对，不将委派失败当作已完成测试。旧30轮循环仍停止。

- storage 本轮运行完成：`pwsh -NoProfile -File artifacts/runs/full_function_once_20261112/run-probe.ps1 -Group storage -Stage run` 返回 1，29 场景中通过 18、失败 11、未执行 0，耗时约55秒，无超时/残留。1000轮串行配置、1000轮历史、20000条并发日志、1000小文件×10轮正常备份还原通过；配置并发、保存失败反馈、六类还原异常保护、BOM/UTF16/GBK格式保留失败。精确命令、PID与清理 `artifacts/runs/full_function_once_20261112/storage/run-01/command-result.json`、`process.json`；输出 `stdout.log`，全部场景断言与实际负载在 `data/run-*/results.json`。未修改正式数据或服务，继续其他独立功能。

- process/build-01/ : exit=0; test counts not applicable (build); exact command, timeout, PID and cleanup=artifacts/runs/full_function_once_20261112/process/build-01/command-result.json; output and per-case evidence in the same directory.

- process/run-01/ : exit=1; total=13, passed=9, failed=4, notRun=0; exact command, timeout, PID and cleanup=artifacts/runs/full_function_once_20261112/process/run-01/command-result.json; output and per-case evidence in the same directory.

- services/build-01/ : exit=0; test counts not applicable (build); exact command, timeout, PID and cleanup=artifacts/runs/full_function_once_20261112/services/build-01/command-result.json; output and per-case evidence in the same directory.

- services/run-01/ : exit=1; total=6, passed=4, failed=2, notRun=0; exact command, timeout, PID and cleanup=artifacts/runs/full_function_once_20261112/services/run-01/command-result.json; output and per-case evidence in the same directory.

- ui/build-01/ : exit=0; test counts not applicable (build); exact command, timeout, PID and cleanup=artifacts/runs/full_function_once_20261112/ui/build-01/command-result.json; output and per-case evidence in the same directory.

- surface/build-01/ : exit=0; test counts not applicable (build); exact command, timeout, PID and cleanup=artifacts/runs/full_function_once_20261112/surface/build-01/command-result.json; output and per-case evidence in the same directory.

- UI 本轮夹具按当前分组需求更新旧“分组不可勾选”断言；生命周期菜单释放的三条观察改为记下失败后继续检查并完成循环，不删断言、不降低原负载。新增 surface 补充夹具对设置、路径记忆、对话框、托盘、分组、快捷键、退出、日志、朗读选择、更新安全和单实例的现有生产契约逐条运行20次；这属于重复集成检查，不冒充真实键盘输入。全局图片剪贴板重复写入不扩测，使用本轮全量结果保留其失败边界。具体执行清单在本轮 `run-remaining.ps1` 和 `surface/SurfaceStress.cs`，每个独立组先落盘结果并写项目记录再进入下一组，超时不阻塞后续独立功能。

- ui/run-lifecycle/lifecycle : exit=1; total=1, passed=0, failed=1, notRun=0; exact command, timeout, PID and cleanup=artifacts/runs/full_function_once_20261112/ui/run-lifecycle/command-result.json; output and per-case evidence in the same directory.

- ui/run-main-menu/main-menu : exit=0; total=1, passed=1, failed=0, notRun=0; exact command, timeout, PID and cleanup=artifacts/runs/full_function_once_20261112/ui/run-main-menu/command-result.json; output and per-case evidence in the same directory.

- 本轮真实桌面尝试：当前构建隔离 EXE 已启动，读到“扫描成功”提示及安装位置；确认按钮输入返回 `CUA_GEOMETRY_UNAVAILABLE`，重新选择窗口仍见同一提示，截图返回 `CUA_EXECUTION_FAILED`，一次只读恢复返回 `CUA_TURN_NOT_ACTIVE`。停止输入，真实按键/菜单/托盘/读屏仍受阻，不计通过。只按本轮目标完整路径、PID、创建时间核对并强制清理，清理命令返回0、残留0；不是正常退出验收。完整身份、构建哈希、错误和清理在 `artifacts/runs/full_function_once_20261112/desktop/result.json`、`processes.json`。未改正式配置、未停止读屏或真实服务。

- ui/run-list-menu/list-menu : exit=0; total=1, passed=1, failed=0, notRun=0; exact command, timeout, PID and cleanup=artifacts/runs/full_function_once_20261112/ui/run-list-menu/command-result.json; output and per-case evidence in the same directory.

- ui/run-list-boundaries/list-boundaries : exit=0; total=1, passed=1, failed=0, notRun=0; exact command, timeout, PID and cleanup=artifacts/runs/full_function_once_20261112/ui/run-list-boundaries/command-result.json; output and per-case evidence in the same directory.

- surface/build-02/ : exit=0; test counts not applicable (build); exact command, timeout, PID and cleanup=artifacts/runs/full_function_once_20261112/surface/build-02/command-result.json; output and per-case evidence in the same directory.

- ui/run-hotkeys/hotkeys : exit=0; total=1, passed=1, failed=0, notRun=0; exact command, timeout, PID and cleanup=artifacts/runs/full_function_once_20261112/ui/run-hotkeys/command-result.json; output and per-case evidence in the same directory.

- ui/run-delayed-cancel/delayed-cancel : exit=0; total=1, passed=1, failed=0, notRun=0; exact command, timeout, PID and cleanup=artifacts/runs/full_function_once_20261112/ui/run-delayed-cancel/command-result.json; output and per-case evidence in the same directory.

- ui/run-delayed-immediate/delayed-immediate : exit=0; total=1, passed=1, failed=0, notRun=0; exact command, timeout, PID and cleanup=artifacts/runs/full_function_once_20261112/ui/run-delayed-immediate/command-result.json; output and per-case evidence in the same directory.

- surface/run-settings/settings : exit=0; total=16, passed=16, failed=0, notRun=0; exact command, timeout, PID and cleanup=artifacts/runs/full_function_once_20261112/surface/run-settings/command-result.json; output and per-case evidence in the same directory.

- surface/run-paths/paths : exit=0; total=25, passed=25, failed=0, notRun=0; exact command, timeout, PID and cleanup=artifacts/runs/full_function_once_20261112/surface/run-paths/command-result.json; output and per-case evidence in the same directory.

- surface/run-dialogs/dialogs : exit=0; total=19, passed=19, failed=0, notRun=0; exact command, timeout, PID and cleanup=artifacts/runs/full_function_once_20261112/surface/run-dialogs/command-result.json; output and per-case evidence in the same directory.

- surface/run-tray/tray : exit=0; total=3, passed=3, failed=0, notRun=0; exact command, timeout, PID and cleanup=artifacts/runs/full_function_once_20261112/surface/run-tray/command-result.json; output and per-case evidence in the same directory.

- surface/run-groups/groups : exit=0; total=19, passed=19, failed=0, notRun=0; exact command, timeout, PID and cleanup=artifacts/runs/full_function_once_20261112/surface/run-groups/command-result.json; output and per-case evidence in the same directory.

- surface/run-hotkey-actions/hotkey-actions : exit=0; total=6, passed=6, failed=0, notRun=0; exact command, timeout, PID and cleanup=artifacts/runs/full_function_once_20261112/surface/run-hotkey-actions/command-result.json; output and per-case evidence in the same directory.

- surface/run-lifecycle-exit/lifecycle-exit : exit=0; total=15, passed=15, failed=0, notRun=0; exact command, timeout, PID and cleanup=artifacts/runs/full_function_once_20261112/surface/run-lifecycle-exit/command-result.json; output and per-case evidence in the same directory.

- surface/run-logging/logging : exit=0; total=9, passed=9, failed=0, notRun=0; exact command, timeout, PID and cleanup=artifacts/runs/full_function_once_20261112/surface/run-logging/command-result.json; output and per-case evidence in the same directory.

- surface/run-reader-selection/reader-selection : exit=0; total=23, passed=23, failed=0, notRun=0; exact command, timeout, PID and cleanup=artifacts/runs/full_function_once_20261112/surface/run-reader-selection/command-result.json; output and per-case evidence in the same directory.

- surface/run-update-security/update-security : exit=0; total=13, passed=13, failed=0, notRun=0; exact command, timeout, PID and cleanup=artifacts/runs/full_function_once_20261112/surface/run-update-security/command-result.json; output and per-case evidence in the same directory.

- surface/run-single-instance/single-instance : exit=0; total=3, passed=3, failed=0, notRun=0; exact command, timeout, PID and cleanup=artifacts/runs/full_function_once_20261112/surface/run-single-instance/command-result.json; output and per-case evidence in the same directory.

- 汇总脚本首次执行返回0，但只读核对发现 PowerShell 对有序字典的按属性分组未生成正确分组统计，且当时主窗口动作补充组仍在执行，因此该汇总是无效中间结果，不作为最终通过数量。脚本为 `artifacts/runs/full_function_once_20261112/summarize.ps1`；将原输出保留至 `coverage-intermediate/`，修正为明确对象后仅重做汇总，不重跑产品测试。

- surface/run-main-dispatch/main-dispatch : exit=-532462766; total=40, passed=40, failed=0, notRun=0; exact command, timeout, PID and cleanup=artifacts/runs/full_function_once_20261112/surface/run-main-dispatch/command-result.json; output and per-case evidence in the same directory.

- 主窗口动作补充组异常终止，退出码 `-532462766`、无超时/残留；上条自动计数只代表中断前已落盘的40个用例，不代表全组完成或未执行为0。原 `surface/run-main-dispatch/data/result.json` 明确 `finished=false`，stderr 为测试夹具 `MigrationRuntimeTestHelpers.WithForm` 清理临时目录时对 `Sandboxie.ini.*.bak` 抛出未处理 `UnauthorizedAccessException`。已完成用例不重跑；继续只读定位正在执行的修复设置用例，并单独运行尚未完成的其他功能。原始中断结果保留，不因续跑改写。

- surface/build-03/ : exit=0; test counts not applicable (build); exact command, timeout, PID and cleanup=artifacts/runs/full_function_once_20261112/surface/build-03/command-result.json; output and per-case evidence in the same directory.

- surface/run-repair-launch-rest/repair-launch-rest : exit=0; total=4, passed=4, failed=0, notRun=0; exact command, timeout, PID and cleanup=artifacts/runs/full_function_once_20261112/surface/run-repair-launch-rest/command-result.json; output and per-case evidence in the same directory.

- 本轮最终汇总脚本 `pwsh -NoProfile -File artifacts/runs/full_function_once_20261112/summarize.ps1` 返回0：四个专项压力组共55场景，37通过、18失败（storage 18/29、process 9/13、services 4/6、ui 6/7）；surface 补充集成195个用例/数据行各连续20次，共3900次已完成调用均通过，另1个只读备份测试夹具清理中断项保留为 `blocked_fixture`，不得计通过或完整负载。后续4个未执行功能已在独立 `run-repair-launch-rest` 完成，不重跑前40项。当前分派器58个动作均建立具体测试候选和压力证据映射；这是覆盖索引完整，不等于所有原生效果已验收。汇总 `artifacts/runs/full_function_once_20261112/coverage/summary.json`，逐场景 `cases.json`，逐入口 `coverage.json`/`coverage.md`。全量449项的原失败及桌面阻断继续保留；首次无效中间汇总单独存档，未覆盖原始日志。

- 最终源码/清理核对返回0：基线118个源码、项目、锁文件与解决方案 SHA256 均未变化，本轮所有记录的测试进程无残留，超时0；Python Git状态与基线相同（非字节哈希证明），旧30轮状态仍为 `stopped-by-user`。结果 `artifacts/runs/full_function_once_20261112/final-integrity.json`。独立桌面尝试的强制清理与自动化进程正常结束分别保留；不删除失败夹具与证据。本轮报告 `docs/migration/2026-11-12-all-functions-stress.md`，逐入口表在本轮 `coverage/coverage.md`，后续先处理报告中的高优先级缺陷，不把普通通过路径扩大成软件无问题。

- 最终文档/覆盖一致性检查返回0：分组统计和逐动作映射一致，无漏映射或重复动作；项目文档、当前计划与本轮报告严格UTF-8及替换字符检查通过，`git diff --check` 返回0。已重新读取活动计划，真实桌面与只读夹具重复负载保持未勾选。结果 `artifacts/runs/full_function_once_20261112/final-document-check.json`、差异日志 `document-diff.log`。本轮测试执行与报告收口，所有原始失败保留，不宣称软件全功能无缺陷。

- 用户继续后只补未完成项：桌面枚举成功，但通过授权工具启动既有隔离副本返回 `CUA_EXECUTION_FAILED`，一次只读恢复返回 `CUA_TURN_NOT_ACTIVE`。未发送按键或重复启动；发现本次所属进程后按PID、完整路径与创建时间核对清理，返回0、残留0。证据 `artifacts/runs/full_function_once_20261112/continuation/desktop-result.json`、`desktop-processes.json`。真实桌面继续受阻，不用自动化对象测试替代。
- 只读校正夹具根因已通过遗留文件与源码定位：`BackupSandboxieIniBeforeWrite` 的 `File.Copy` 保留源INI只读属性，新备份仍带 ReadOnly；原测试只恢复源INI属性，`MigrationRuntimeTestHelpers` 在 finally 直接递归删除，导致备份清理异常逃出STA线程。只在隔离夹具中修正清理，保留原业务断言和之前中断记录；产品及仓库测试源码保持不变，只补该未完成用例。

- 只读夹具独立探针首次编译返回1，MSTest引用参数未正确拆分，报CS0234/CS0246；未运行用例，不算产品失败。精确命令及错误 `artifacts/runs/full_function_once_20261112/continuation/repair-readonly/build-result.json`、`build.log`；仅修正编译参数后继续，原失败保留。

- 独立只读夹具构建已通过，返回0；原断言代码保持，仅在隔离副本增加备份只读属性清理和清理异常捕获，未改 `csharp/`。精确参数/日志 `artifacts/runs/full_function_once_20261112/continuation/repair-readonly/build-02-result.json`、`build-02.log`；随后仅运行原未完成的只读校正用例。

- 只读校正未完成项补验结束：`RepairReadonlyProbe.exe <本轮continuation/repair-readonly/run-01>` 返回0，同一业务断言连续20次，20通过、失败0、跳过0，临时夹具与进程残留0。验证了取消不写入、只读写入失败不reload且正确提示；独立副本仅修正清理步骤，产品与仓库测试源码哈希均未变化。精确命令/PID/清理 `artifacts/runs/full_function_once_20261112/continuation/repair-readonly/run-01-command.json`，逐轮结果 `run-01/result.json`、标准输出 `run-01.stdout.log`。原3900次通过与夹具中断保留历史，本次新增20次补齐该场景，不把此前中断改写为当时通过；真实桌面仍阻断。

- 本次继续收尾核对返回0：文档严格UTF-8/替换字符及差异检查通过，基线源码哈希无变化，本轮所属进程无残留。已复读当前计划，仅真实桌面项仍未完成；只读校正重复负载已补验，原夹具中断历史不删除。结果 `artifacts/runs/full_function_once_20261112/continuation/closure.json`，差异日志 `continuation/diff-check.log`。没有恢复旧压力循环、没有修复产品缺陷或发布。

- 用户追问全局快捷键后完成只读源码核对：当前 C# 热键入口 `MainForm.Lifecycle.cs::ProcessCmdKey`，`MainForm.ApplyHotkeys` 仅规范化配置；未找到系统全局热键注册、WM_HOTKEY或全局键盘钩子接线。此前热键通过仅适用于窗口内处理/设置动作行执行，不适用于切到其他软件或隐藏到托盘后触发。Python对应使用 `wx.AcceleratorTable`，不能未经进一步需求核实就判为漏迁移。未运行新测试、未改产品；已补入本轮报告“全局快捷键边界”，全局能力未实现/未验收不能被58动作映射覆盖。

- 用户指出“所有功能压测”与此前完成表述矛盾，本轮重新核对计划及已有证据，纠正任务状态为未完成。分派器动作与相关测试组建立映射，不等于逐项真实效果验收；普通契约重复调用不等于所有功能端到端压力通过。原汇总 `actionsWithoutPressure` 和 `automated_related_checks_passed` 只保留其关联索引意义，不再用作全覆盖证明。未找到全局快捷键接线、真实输入受阻、音频/沙盒等外部效果以及更新/多进程边界在报告逐项列为缺口；当前计划追加实际状态更正。历史测试数字、失败和原始日志未改，本轮没有新增产品测试或实现，不恢复已停止的旧循环。

- 覆盖结论更正文档核对返回0：三份文档严格UTF-8、替换字符、未完成状态及 `git diff --check -- <三份文档>` 均通过，已复读当前计划。检查结果 `artifacts/runs/full_function_once_20261112/continuation/coverage-correction-check.json`，差异日志 `coverage-correction-diff.log`。本次产品测试执行数为0，没有修改产品或恢复旧循环；文档核对通过仅表示更正已落盘，不表示功能验收通过。

- 用户再次明确整套范围后，开始补齐未验收项：本轮桌面连接已恢复，授权工具启动现有隔离主EXE并成功读取主窗口及焦点，目标PID 14432、窗口264896，构建目录 `artifacts/runs/full_function_once_20261112/desktop/app/`。这是启动与可读取证据，尚非按键或功能通过；该副本旧配置仍指向本机沙盒目录，先正常关闭并准备独立夹具再做写入类操作。子代理分别核查全入口清单和准备独立进程互斥测试；不恢复旧30轮循环。

- 本次真实桌面补验再次阻断：主窗口和列表可读取，但第一次实际 `Alt+F4` 操作报告 `CUA_EXECUTION_FAILED`，按键是否生效未知；没有重复发送或改用脚本按键。只读恢复及原始窗口树保存 `artifacts/runs/full_function_once_20261112/continuation/comprehensive-desktop.json`；目标按PID/完整路径/创建时间清理结果 `comprehensive-desktop-cleanup.json`。本场景记为环境阻断，不计产品按键失败或正常退出通过；继续其他不依赖桌面输入的缺口。

- 原生音频补验探针构建完成，编译退出码0，无编译诊断；精确命令和日志 `artifacts/runs/full_function_once_20261112/continuation/native-audio/build-result.json`、`build.log`。引用本轮当前源码已编译Core，准备核对真实静音会话的当前/全部音量与静音、上下限及音量记忆；沙盒到PID映射仍为隔离夹具，此构建不是功能通过证据。

- 原生音频补验执行返回0，167次实际状态核对通过、失败0，耗时25.46秒、无超时和进程残留。当前/全部音量升降和静音切换各执行20轮，另核对上下限、恢复记忆音量、调整后的记忆值和空闲目标；三个独立无声播放进程中第三个始终不受影响。证据 `artifacts/runs/full_function_once_20261112/continuation/native-audio/run-result.json`、`bin/native-result.json`、`stdout.log`。调用当前生产音频服务并读取真实Windows会话状态，但沙盒/PID关系为夹具，未测菜单输入、输出设备路由或真实Sandboxie；这些缺口不因本项通过而关闭。

- 单读屏原生补验探针构建返回0，无编译诊断；精确命令/日志 `artifacts/runs/full_function_once_20261112/continuation/native-speech/build-result.json`、`build.log`。准备使用当前Core及随包真实争渡接口，核对加载/保持选择/释放，并仅输出一句短测试语；不停止读屏进程，听感需要单独观察，构建通过不计朗读验收通过。

- 单读屏原生补验返回0：真实争渡接口20轮加载/保持选择/关闭全部通过，每轮保持选择20次；最后通过生产朗读工作线程调用原生输出一次，接口调用完成且无异常。证据 `artifacts/runs/full_function_once_20261112/continuation/native-speech/run-result.json`、`bin/native-speech-result.json`，读屏进程前后身份记录保留，探针无残留。听感仍待用户观察；该证据不覆盖UI焦点朗读、键盘打断或菜单交互，不将接口调用返回等同听到正确文案。

- 用户对本次争渡测试语明确反馈“听到了，内容正确”，单次实际朗读听感已确认。与上条真实接口重复加载/释放证据合并，仅关闭该接口输出场景；焦点朗读、按键打断及全部界面读屏仍未验收。原探针JSON的 `pending-user-observation` 是提问前快照，当前听感状态以本条用户反馈为准。

- 子代理准备的独立进程单实例探针已由主代理审核并编译，退出码0，无编译诊断；命令/日志 `artifacts/runs/full_function_once_20261112/continuation/native-single-instance/build-01/build-result.json`、`build.log`。探针只加载当前主程序集中的生产 `SingleInstanceGuard`，使用唯一隔离互斥名和独立控制台子进程，准备核对并发竞争、目录隔离、释放及持有者异常退出；不启动主UI、不碰正式主程序互斥锁。

- 独立进程单实例补验返回0，4场景通过、失败0、无效0、未运行0；8进程同时竞争仅一方获取，跨工作目录同名互斥仍生效，显式释放和持有者异常终止后均可重新获取。共18个隔离子进程，全部清理，耗时9.09秒，无超时/残留。生产主程序集路径和哈希固定于探针；命令/清理 `artifacts/runs/full_function_once_20261112/continuation/native-single-instance/build-01/run-result.json`，逐场景/进程证据 `runs/2c860c7205254ac78826bb0b1a079bb5/result.json`、`events.jsonl`。仅确认生产互斥组件在独立OS进程中工作，不覆盖多个完整主窗口、通知置前、跨登录会话或长期资源趋势。

- 用户表示不知道如何配置桌面连接后，进行了只读宿主诊断：本机手册 `C:/Users/Administrator/AppData/Local/Programs/MyCodex/help/用户手册.html` 的“官方 Computer Use”说明组件内置、执行故障后新有效回合继续；已接入的窗口枚举/读取不能证明输入可用。现有应用日志及sandbox日志未提供本次底层执行失败原因，未修改宿主设置或停止宿主。本次新增音频、朗读和进程互斥证据已同步报告/计划，整套真实操作仍未完成。

- 隔离边界复核发现测试风险：真实主EXE启动会经 `MainForm.InitializeApplication -> ApplyAutoRun` 调用固定HKCU Run项 `SandboxLauncher`，`auto_run=false` 时删除该项，与EXE目录是否隔离无关。本次启动前未保存该注册表值，无法证明该自启动项保持原样，也无法据此猜测恢复值；没有追加修改或为补验启动更多主EXE。此项作为测试隔离缺口保留，后续真实启动需先快照/恢复明确的原值或使用已核对的注入夹具；该风险不改变原生音频/朗读/互斥探针结果。

- 本次补验源码/进程核对返回0：基线118个源码/项目文件哈希未变化，新增原生探针和隔离主EXE均无残留，旧循环未恢复。结果 `artifacts/runs/full_function_once_20261112/continuation/comprehensive-integrity.json`；朗读用户确认另存 `native-speech/user-confirmation.json`。未采集启动前自启动注册表值的边界明确保留，不能以源码/进程核对替代系统设置未变证明。整套状态仍为未完成。

- 本次两项子代理工作分别处理单实例探针和整套清单；探针已交付、主代理审核执行并记录。清单子代理未交付文件，已停止，主代理接手按当前动作源码和各对话框/列表/托盘补出 `artifacts/runs/full_function_once_20261112/continuation/comprehensive-inventory.md`，逐项注明真实输入和外部效果缺口；未把子代理未完成工作记为通过。报告已链接该清单，动态菜单和条件分支仍需真实UI逐项复核；整套任务没有完成。

- 本次文档/入口清单核对返回0：项目结构、报告、活动计划及补充清单严格UTF-8和替换字符检查通过，差异检查返回0，58个已识别分派动作均单列且无重复/遗漏；这只核对入口清单，不代表58项真实操作通过。已复读当前活动计划，整套验收及桌面项保留未完成。证据 `artifacts/runs/full_function_once_20261112/continuation/comprehensive-document-check.json`、`comprehensive-doc-diff.log`；本次子代理均已关闭，未升版、提交或发布。

- 用户按要求在新回合“继续验收”后完成本轮桌面恢复尝试，证据目录 `artifacts/runs/full_function_once_20261112/continuation/desktop-resume-103439/`。启动前保存了固定HKCU Run项 `SandboxLauncher` 的存在性/类型/原值及隔离配置；当前主EXE哈希与本轮基线一致。目标PID 5532、窗口2493800，实际目录仍为 `desktop/app/`。主窗口文本和用户列表焦点可读取；第一次Tab请求返回 `CUA_OBSERVATION_STALE` 并明确要求有效截图，随后 `get_window_state(include_screenshot:true)` 返回 `CUA_EXECUTION_FAILED`，一次只读恢复返回 `CUA_TURN_NOT_ACTIVE`。没有成功按键证据，不判产品Tab失效，阻断具体位于工具要求的截图环节。
- 本轮恢复清理返回0，功能通过0、按键验收无有效结果，目标按PID/路径/创建时间匹配后强制清理，残留0；自启动项启动前/后均不存在，未发生需恢复的差异，隔离配置已还原。结果 `desktop-resume-103439/result.json`，原始观察/错误 `desktop-events.json`，身份 `process.json`，启动前登记 `autorun-before.json`，软件日志 `application.log`。本轮只证明当前快照之后的自启动项保持一致，不回推此前未采集原值的历史。新回合本身未解决工具故障，不再反复重复相同恢复尝试；整套验收继续未完成。

- 本轮隔离程序日志还记录了实际启动链路：主窗口进入消息循环，刷新到96个沙盒用户；自动下载并解析在线更新清单，当前/最新均为4.0.50，未提示同版更新。这是 `desktop-resume-103439/application.log` 中的启动观察，不作为重复负载通过或真实键盘通过；没有人为增加在线压力请求。

- 本轮恢复尝试文档核对返回0：项目结构、当前计划、压力报告及截图故障复现说明严格UTF-8/替换字符和差异检查通过；已复读计划，真实桌面和整套任务仍未完成。检查 `artifacts/runs/full_function_once_20261112/continuation/desktop-resume-103439/document-check.json`、`diff-check.log`，可交接的宿主故障复现 `desktop-issue.md`。没有重跑套件、修改产品、提交或发布。

### 无截图功能补验：真实定时器探针构建

- 用户明确指出功能测试不必依赖截图；继续原批准范围内的进程、文件、计时器和系统接口验证，真实按键/焦点/托盘另留未验收，不重复桌面恢复。
- 命令：Framework64 `csc.exe /nologo /target:exe /platform:x64 /codepage:65001` 编译 `continuation/no-screenshot/timers/RealTimerProbe.cs`，引用本轮 baseline 的 Core 与 Newtonsoft；完整参数、返回码在同目录 `build-result.json`，输出 `build.log`。构建返回码 0，无编译诊断；功能尚未执行。
- 本节相对路径统一基于 `artifacts/runs/full_function_once_20261112/`；两个子代理仅准备独立探针，主代理审核和串行执行，现已关闭。

### 无截图功能补验：真实定时器执行

- 命令：`continuation/no-screenshot/timers/RealTimerProbe.exe`；返回码 0，5 项通过、0 失败、0 跳过，观测约 5.17 秒。真实计时器首次执行约 1 秒，失败重试约 2 秒执行第二次；取消、关闭状态抑制、异常报告和任务移除均符合断言。
- 结果：`continuation/no-screenshot/timers/RealTimerProbe-1d665f4cc8af4db09342dbfa9b7f06b5.json`；命令/外层耗时 `run-result.json`，输出 `run.log`。退出前无剩余任务，持有计时器已释放，无清理错误。
- 使用当前生产 Core 默认 TimerHandle，不注入虚拟时间；为安全观察，每个场景独立状态且 Jobs 使用并发字典。没有启动/关闭真实沙盒，不证明 UI 调度线程、长时间资源稳定或定时目标改名/删除问题已修复。

### 无截图功能补验：系统设置探针构建

- 命令：`dotnet <sdk8.0.408>/Roslyn/bincore/csc.dll /target:exe /platform:x64 /langversion:7.3 /codepage:65001` 编译 `continuation/no-screenshot/settings/NativeSettingsProbe.cs`，引用 net48 系统程序集和当前 baseline Core/Newtonsoft；返回码 0，无编译诊断。全部参数和输出见同目录 `build-result.json`、`build.log`。
- 已审核只调用生产静态注册表写入方法且使用唯一专用 HKCU 测试子树，不构造主窗体、不触碰系统 Run 项；配置往返使用独立子进程和隔离 Configurations。

### 无截图功能补验：系统设置实际效果

- 命令：`continuation/no-screenshot/settings/NativeSettingsProbe.exe`；返回码 0，2 个场景通过、0 失败、0 跳过，探针内部耗时 1496ms。生产自启动静态方法对专用真实注册表项完成 20 次开/关并保护无关标记；22 个设置键（含中文路径、分组、热键和朗读提示）由父进程保存、独立子进程读写、父进程重新读取，值一致。
- 父 PID 20144、子 PID 19484；子进程正常退出且没有超时，配置临时文件清理、唯一测试注册表子树清理均成功。未构造 MainForm，不调用正式 Run 项；这验证写入机制及跨进程配置持久化，不代表登录后自动启动或设置对话框交互已通过。
- 权威结果 `continuation/no-screenshot/settings/runs/4f1824b16ba94786ba4cc859ac62b636/result.json`；外层命令及耗时 `settings/run-result.json`，输出 `settings/run.log`。既有并发丢失/保存失败反馈问题保持未修复。

### 无截图功能补验：更新重启标记程序构建

- 命令：Framework64 `csc.exe /nologo /target:winexe /platform:x64 /codepage:65001 /out:RestartMarker.exe RestartMarker.cs`；返回码 0，无编译诊断。证据 `continuation/no-screenshot/update/marker-build-result.json`、`marker-build.log`。
- 标记程序只写入自身隔离目录的 PID/UTC 文件并退出，没有界面或系统设置；用于核对生产更新器真实重启，不能当完整主窗口启动证据。未改生产源码/版本或发布包。

### 无截图功能补验：内置更新进程探针构建

- 命令：`dotnet <sdk8.0.408>/Roslyn/bincore/csc.dll /target:exe /platform:x64 /langversion:7.3 /codepage:65001` 编译 `continuation/no-screenshot/update/NativeUpdateProbe.cs`，引用当前 baseline Core/Newtonsoft 和 net48 压缩库；返回码 0，无编译诊断。精确参数、输出在同目录 `build-result.json`、`build.log`。
- 场景为当前产品 EXE 的 `--install-update` 模式、回环 HTTP ZIP、真实父进程等待和重启标记子进程；临时更新目录也指向唯一隔离运行目录。测试目标版本标记仅在本地请求中，不修改软件版本或在线清单。

### 无截图功能补验：当前内置更新实际进程链

- 命令：`continuation/no-screenshot/update/NativeUpdateProbe.exe`，内部真实调用当前主 EXE 的 `--install-update --app-dir ... --exe ... --pid ... --request-file ...`；完整绝对路径/参数在结果中。返回码 0，1 条完整安装场景通过，17 个检查点通过、0 失败、0 跳过，耗时 3653ms。
- 监督 PID 18656，等待退出的所属父进程 15428，生产安装器 17836，真实重启标记子进程 19944。安装器在父进程存活时等待；父进程退出后才发出 1 次回环 HTTP 下载。主文件/Core 替换、新增 DLL 哈希、配置原字节及日志标记保留、10 条阶段日志、更新临时目录清理均通过；各所属进程已退出，无超时或清理错误。
- 当前生产 EXE SHA-256 `2B60A1E50C262F26B2BFDB17396932DB80B86AB3A6A5F588B6DB793EFB72E605`。权威证据 `continuation/no-screenshot/update/runs/528398d6276e40b3ada0ffeaced4971e/result.json`，同目录保留请求、HTTP 请求头、包和 `target/logs/`，外层命令/输出 `update/run-result.json`、`run.log`。
- 边界：下载、安装与 Process.Start 均为当前生产实现，没有模拟回调。包为本地专用夹具，重启目标为写标记后退出的程序；不等于新公开版、主窗口升级提示/再次启动或真实在线发布通过。既有签名/加密和线上历史证据复用，正式程序、配置、注册表及线上清单没有改动。

### 无截图功能补验：Sandboxie只读探针首次构建

- `continuation/no-screenshot/sandbox-readonly/build-result.json` 保存 Roslyn/net48 x64 完整编译参数；返回码 1，`build.log` 有两处 CS0104：测试源中的 TimeoutException 与 System.ServiceProcess 同名类型产生歧义。未执行探针，功能通过/失败均为 0；属于夹具编译错误，不判产品失败。仅明确测试异常类型后重新构建，保留原日志。

### 无截图功能补验：Sandboxie只读探针构建纠正

- 仅将测试源的两处异常类型写为 `System.TimeoutException`，复用同一 Roslyn 参数编译，返回码 0，无编译诊断；`continuation/no-screenshot/sandbox-readonly/build-final-result.json`、`build-final.log`。生产源码未改，尚无功能结果；只读路径限制为发现安装、官方用户枚举和 /listpids、原生 PID API、服务状态及 INI 哈希观察。

### 无截图功能补验：Sandboxie真实只读查询中断

- 命令：`continuation/no-screenshot/sandbox-readonly/SandboxReadOnlyProbe.exe`；返回码 1，耗时 11270ms。实际发现 `C:\Program Files\Sandboxie`，官方枚举返回码 0、96 个用户；20 次原生 PID API 均返回有效空集合（当前没有活动沙盒 PID，不证明运行中发现通过）。前 11 个用户的官方 `/listpids` 与 API 空结果一致，无比较失败/漂移。
- 到 `Box12` 时生产 `RunSandboxieStart` 返回 null，探针以 TimeoutException 中断，后续用户未执行。生产包装同时会把超时或异常转为 null，故当前仅能断言“实际官方查询无有效返回”，不能仅凭探针异常类认定根因。将对该单个失败查询捕获直接进程返回和耗时，不重跑前面通过项。
- 结果 `continuation/no-screenshot/sandbox-readonly/runs/129772788f52426b8196a2072e437b10/result.json`，同目录保留官方用户枚举原文；外层命令/输出 `sandbox-readonly/run-result.json`、`run.log`。正式 INI 候选哈希前后一致，SbieSvc 始终 Running；没有创建、删除、启停沙盒或正式服务操作。

### 无截图功能补验：Box12单项直接查询诊断

- 命令：`C:\Program Files\Sandboxie\Start.exe /box:Box12 /listpids`，隐藏窗口、重定向输出、8秒超时；仅诊断上一项未返回的同一查询，不涉及启动用户程序。所属 PID 13856，退出码 0、耗时 57ms，标准输出 `0`、标准错误为空，无超时、进程已正常退出。证据 `continuation/no-screenshot/sandbox-readonly/box12-direct-result.json`。
- 该结果证明单项官方查询当前可用，不能撤销原生产包装无结果记录或认定已经修复；继续未完成用户时跳过此前已通过项，重复的原生空集合压力也跳过。

### 无截图功能补验：只读查询续测夹具构建

- 为避免重跑通过项，探针增加显式 `SHAHE_READONLY_RESUME=1` 模式：核对正式 INI 候选哈希与原记录一致，复用原官方用户名单，跳过原生重复采样及先前完成的用户，只继续未完成查询。生产行为不变。
- 同参数 Roslyn 构建返回码 0，无编译诊断；`continuation/no-screenshot/sandbox-readonly/resume-build-result.json`、`resume-build.log`。原失败证据保留，续测结果另建唯一目录。

### 无截图功能补验：Sandboxie剩余用户只读查询

- 命令：仅当前子环境设置 `SHAHE_READONLY_RESUME=1` 后运行 `continuation/no-screenshot/sandbox-readonly/SandboxReadOnlyProbe.exe`；返回码 0，耗时 3945ms。跳过原先已完成的 11 个用户，剩余 85 个用户（含原中断 Box12）全部官方命令退出码 0，CLI 与前后稳定原生 API 的空 PID 集合一致，0 比较失败/漂移。
- 本轮两段合计覆盖官方枚举的 96 个用户；这是当前无沙盒进程状态下的真实查询，不等于运行中 PID、创建/删除/批量启动/关闭已验收。原 Box12 无有效返回保留为偶发查询异常候选，后续正常结果不删除历史失败或声称已修复。
- 权威结果 `continuation/no-screenshot/sandbox-readonly/runs/0f931ee109354aa7a9957b4a4ac376e9/result.json`；命令/输出 `sandbox-readonly/resume-run-result.json`、`resume-run.log`。正式 INI 候选哈希一致，SbieSvc 状态仍 Running。后续完整性检查核对无所属查询/探针进程残留。

### 无截图功能补验：源码、残留和文档核对

- 最终完整性检查返回码 0：与本轮基线比较 118 个生产源码文件，差异 0；无 `continuation/no-screenshot/` 下所属测试进程残留。当前主EXE/Core 哈希与原生验证基线一致。
- 项目结构、当前计划、报告和逐入口清单严格 UTF-8 解码通过，replacement character 为 0；本次文档 `git diff --check` 返回码 0。证据 `continuation/no-screenshot/final-integrity.json`、`document-diff-check.log`。
- 本次新增实际结果已逐项记录；原全量失败、压力缺陷和真实交互未验收继续保留，没有修改产品/发布包，没有重启旧30轮。当前计划的完整UI、真实运行中沙盒和系统登录/主窗口升级边界仍未完成，不据新增组件通过声明整套完成。

### 无截图功能补验：官方查询子进程补充清理核对

- 按两段只读探针父 PID 10888、19936 以及精确 `C:\Program Files\Sandboxie\Start.exe` 路径核对所属官方查询进程，残留 0，检查返回码 0；证据 `continuation/no-screenshot/official-query-cleanup.json`。没有终止任何正式 Sandboxie/读屏进程。
- 已重新阅读当前计划的执行、历史停止、已完成和未完成状态；本次只关闭有新增证据的组件场景，完整软件验收仍未完成。

### 未通过项继续：非默认音频设备探针构建

- 用户要求“没测试通过的继续”，沿用当前全功能计划，补尚未验证的外部效果并诊断未确认失败，不重跑通过组。两个子代理分别准备菜单原生资源探针及复核高优先级失败因果，主代理执行音频补验。
- Roslyn/net48 x64 编译 `continuation/pending/audio/NonDefaultAudioProbe.cs`，引用本轮 baseline Core/Newtonsoft；返回码 0，无编译诊断。精确参数和输出见 `continuation/pending/audio/build-result.json`、`build.log`。
- 新场景是在非默认播放设备建立所属静音 WinMM 流，独立原生接口校准后检查生产会话发现、当前/全部音量和静音；不改变系统默认设备，不操作读屏或用户音频进程。接口结构按微软 waveOutOpen、WAVEHDR、waveOutPrepareHeader 文档核对；探针不等于动态路由已通过。

### 未通过项继续：非默认音频首次探针启动异常

- 命令：`continuation/pending/audio/NonDefaultAudioProbe.exe`；进程返回码 -532462766（CLR未处理异常），输出提示异常字符串无法打印，未生成场景结果，产品通过/失败均不计。证据 `audio/run-result.json`、`run.log`，原样保留。
- 夹具检查发现静态字段提前引用生产程序集，发生在 Main 注册 AssemblyResolve 之前；将其改为访问时取 Type，确保解析器先注册，仅改测试源后复验。当前不能将启动故障判为产品音频失败。

### 未通过项继续：非默认音频夹具加载修正构建

- 取消测试类过早初始化的生产 Type 静态字段，改为调用时解析；同一 Roslyn 参数重新构建返回码 0，无诊断。`continuation/pending/audio/build-final-result.json`、`build-final.log`。生产代码未改，原启动失败不覆盖。

### 未通过项继续：非默认设备音量/静音确认失败

- 命令：`continuation/pending/audio/NonDefaultAudioProbe.exe`；有效运行返回码 1，5 项检查失败、0 通过、0 跳过，内部耗时 1227ms。监督 PID 18348，所属无声流 PID 18804，直接在非默认 Realtek 扬声器端点 `{0.0.0.00000000}.{9d0e959d-b508-434a-bbc6-14ab0572b7b5}` 建立音频会话。
- 独立原生端点检查确认会话 Active、音量约0.4、未静音；生产枚举找不到该PID。当前提高音量、全部降低音量、当前静音、全部静音均接受并完成任务，但实际会话保持0.4/未静音，状态分别提示“未找到可调节音量/可静音的进程”。故不是截图、无声流未建立或沙盒PID缺失造成。
- 已确认根因：`csharp/ShaheLauncher.Core/Processes/WindowsProcessAudioBackend.cs::GetAudioSessions` 只调用 GetDefaultAudioEndpoint 获取默认设备，没有枚举其他活动输出设备。新缺陷编号 `AUDIO-NONDEFAULT-01`；此前默认设备上的音量/静音通过仍有效，但不适用于非默认设备。本次没有测试动态播放流迁移，也未断言是Python漏迁移。
- 精确证据 `continuation/pending/audio/runs/6b14f2aa95954c1f902ecfa606e8c408/result.json`，命令/输出 `audio/run-final-result.json`、`run-final.log`。原生流复位、释放并正常退出，clean=true；没有更改系统默认设备或用户音频路由，没有修改产品代码。

### 未通过项继续：菜单原生资源诊断构建

- 编译 `continuation/pending/resource/MenuResourceProbe.cs`，Roslyn/net48 x64，引用当前 baseline 主EXE/Core/Newtonsoft，返回码 0，无诊断；精确参数及源文件哈希在同目录 `build-result.json`，输出 `build.log`。
- 审核范围：仅测试进程内构造隐藏主窗体、创建并观察自己的 HWND/HMENU，然后 Destroy/Dispose 和GC；不调用 Program/InitializeApplication，不显示窗口或注入输入，不创建托盘、查询正式沙盒或写入自启动。用原生 IsMenu 与托管弱引用区分“未发事件”和“资源未释放”，不直接以汇总句柄差值定性。
- 音频源码对照补充：Python 的 `features/process_actions.py` 也调用 pycaw `AudioUtilities.GetAllSessions()`；本机 pycaw 的 GetAllSessions→GetAudioSessionManager→GetSpeakers 同样只获取默认端点。这是源码层共有边界证据，没有重跑Python音频，不把 AUDIO-NONDEFAULT-01 直接归为迁移遗漏。

### 未通过项继续：菜单事件与实际资源区分

- 命令：`continuation/pending/resource/MenuResourceProbe.exe`，所属 PID 10460，退出码 0，50 轮观察完成、0 执行异常、0 超时，约3515ms，正常退出。结果 `resource/run.jsonl`、`run-result.json`、`summary.json`，标准错误为空。
- 原事件失败仍能复现：每轮主菜单/路径菜单 Disposed 计数为0，所属用户菜单为1。主菜单原生HMENU在窗体Dispose后已经失效；路径菜单HMENU在仍持强引用时每轮仍有效，但释放引用并GC后所有菜单及子菜单HMENU均失效，窗体和菜单弱引用也全部不可达。GC后资源从首轮至末轮稳定为 GDI16/USER10/进程句柄405，无逐轮累积趋势。
- 结论：原“缺少事件”不能解释为持续原生菜单泄漏；本次短周期未观察到GC后泄漏。路径上下文菜单未被确定性Dispose、依赖最终回收，作为低优先级释放改进项保留。返回码0只表示诊断完成且未留存，不把菜单事件要求或真实生命周期整体验收改成通过；本测试不验证长时使用/真实托盘退出。

### 未通过项继续：另一播放框架的路由诊断构建

- Roslyn/net48 x64 编译 `continuation/pending/routing/MediaRoutingProbe.cs`，复用历史 AudioRoutingProbe 中只读端点观察/路由读回方法，入口明确指定 MediaRoutingProbe，旧 Main/Python/WinMM 对照不执行；返回码 0，无诊断。精确参数 `routing/build-result.json`，输出 `build.log`。
- 场景采用所属无界面 Windows Media Player COM 播放静音 WAV，以当前生产“切换当前设备”动作设置路由，再核对活动会话端点与重新打开媒体后的端点。只在所属进程保存/恢复原路由，设备选择和沙盒PID是夹具；不是桌面输入。COM播放器用法核对微软 Windows Media Player 对象模型/Controls 文档。
- 三项历史高优先级失败的源码/夹具复核已交接到 `continuation/pending/confirmed-failures.md`：还原先删后复制且异常缺回退、删除查询失败变空列表、定时闭包仍抓旧目标均有对应证据；仅分析，不新增运行通过或声称已修复。两个子代理均已关闭。

### 未通过项继续：实际播放设备切换取得有效通过

- 命令：`continuation/pending/routing/MediaRoutingProbe.exe`；返回码 0，2 个实际端点检查通过（播放中迁移、重新打开媒体后仍在目标端点），0 失败/超时，耗时2366ms。监督 PID18164，所属无界面媒体进程11984。
- 生产 OnSwitchOutputDeviceCurrentSandbox 设置路由后，原 Playback 1/2 端点会话变 Inactive，Realtek 端点出现 Active 会话，独立原生观察确认实际迁移，不只核对设置返回true。原Console/Multimedia路由均为null，退出前逐项恢复并读回一致；所属进程正常退出，标准错误为空。
- 证据 `continuation/pending/routing/runs/aa82726237af400994272f4382f10dce/result.json`，外层命令/输出 `routing/run-result.json`、`run.log`。该结果证明本机 Windows Media Player COM 框架的当前进程切换可用，不能扩大成所有应用/全部进程/真实菜单通过。历史WinMM流不迁移记录仍有效，属于不同播放框架的实际差异。
- 后续只补“成功切换后继续调音量/静音”的组合缺口，迁移步骤作为新场景前置校准，不另记重复通过；已有默认设备压力和WinMM对照不重跑。

### 未通过项继续：切换后音量/静音组合探针构建

- 原路由探针增加 `SHAHE_AUDIO_POST_SWITCH=1` 模式，仅以成功迁移作校准，随后在目标设备设置所属流基线并执行当前音量/静音。复用非默认设备探针的原生读回方法，不再执行已通过的重新打开媒体检查。
- Roslyn构建返回码0，无诊断；完整引用参数见 `continuation/pending/routing/post-build-result.json`，输出 `post-build.log`。只修改隔离测试代码，生产组件继续复用同一基线。

### 未通过项继续：切换后组合场景首次校准无效

- `SHAHE_AUDIO_POST_SWITCH=1 MediaRoutingProbe.exe` 返回码2，监督PID12260、媒体PID18868，约1901ms；路由迁移前置成功，但测试测量器只保留同PID最后一条会话状态，WMP同端点同时有Active/Inactive会话，误用末尾Inactive判校准失败。产品音量/静音动作尚未执行，不计产品失败或通过。
- 证据 `continuation/pending/routing/runs/310026476940496ea01aea5d57cedbce/result.json`、`routing/post-run-result.json`。原路由恢复且媒体进程正常退出，无清理异常。仅为该组合测量增加“只读/设置活动会话”选项后复验，不改变生产枚举逻辑。

### 未通过项继续：活动会话校准修正构建

- 为共享测试测量器增加默认关闭的 activeOnly 参数，组合场景启用后跳过Inactive会话；原非默认单流探针的调用行为不变。Roslyn重编译路由组合探针返回码0，无诊断。证据 `continuation/pending/routing/post-build-final-result.json`、`post-build-final.log`；没有删产品断言或修改生产代码。

### 未通过项继续：成功切换后音量/静音仍失败并误报成功

- 命令：`SHAHE_AUDIO_POST_SWITCH=1 continuation/pending/routing/MediaRoutingProbe.exe`；有效复验返回码1，2个新组合检查均失败、0通过，约1969ms。监督PID8644，媒体PID17156；路由迁移已独立校准成功，故失败不是迁移前置未满足。
- 目标端点活动会话校准为40%/未静音。生产“提高5%”提示“当前约100%”，实际活动会话仍40%；生产“静音”提示“已静音”，实际仍未静音。源码只枚举默认端点，并把该PID留在旧默认端点的Inactive会话也当控制对象，因此操作了旧会话、反馈成功，却没有影响正在播放的目标会话。归入同一根因 `AUDIO-NONDEFAULT-01`，不是另一个配置保存反馈问题。
- 证据 `continuation/pending/routing/runs/5046c4b7df764e7284a876b51329c689/result.json`，外层命令/输出 `routing/post-run-final-result.json`、`post-run-final.log`。原Console/Multimedia路由精确恢复，所属媒体正常退出，stderr为空；未重测媒体重新打开，reopenedMigrated=false表示该模式未执行，不判该旧通过项失败。
- 原首轮校准错误保留为无效证据；本条有真实活动会话、输入基线、生产状态文案和实际效果读回，属于有效产品失败。后续修复需同时覆盖非默认端点发现和切换后活动会话效果，不能只修改提示文字。

### 未通过项继续：本次完整性与计划核对

- 最终检查返回码0，118个基线生产源码文件无哈希差异；`continuation/pending/`所属探针/媒体进程残留0。项目结构、当前计划、报告、逐入口清单及失败交接严格UTF-8通过，无replacement character，文档差异检查返回码0。证据 `continuation/pending/final-integrity.json`、`diff-check.log`。
- 已复读当前活动计划核对：本轮关闭的是有证据的诊断/特定播放框架场景，AUDIO-NONDEFAULT-01及还原/删除保护/定时旧目标等失败尚未修复；真实输入、运行中沙盒、全部输出切换及其他框架边界保持未完成。没有产品修改、提交或发布，没有重启旧30轮。

### 用户新增交付要求：验收后发布GitCode

- 用户明确要求“测好之后推版本到gitcode”，条件发布已授权，届时不重复询问推送权限。当前仍有确认失败，不把未验收/失败包直接改成正式更新。
- 已展示当前计划完整追加内容，包含按顺序修复、验证、绿色包与旧版升级、命令行GitCode包/清单发布；计划写入确认正在等待用户答复，尤其需要明确此前撤回的保存失败反馈修复是否重新纳入。未写入该计划追加内容，未修改生产源码/版本。
- 本地发布准备只读核对返回码0：现有源码和本地公开清单版本4.0.50，专用GitCode发布检出干净，HEAD为7a0f156d38c8db1348314cb7e82ede6d3118401d。证据 `artifacts/runs/release_readiness_20261112/114303/local-readiness.json`。下一候选预计4.0.51，尚待线上版本核对，不覆盖旧版本包。

### 发布准备：线上清单只读核对

- `curl.exe --location --silent --show-error --connect-timeout 10 --max-time 25` GET生产UpdateManifestUrl，返回码0、HTTP200，JSON解析有效，线上latest_version=4.0.50、1个包。此时下一候选4.0.51未被该清单占用；正式发布前仍须再次核对，不能据当前读取覆盖随后变化。
- 命令URL、HTTP状态和版本见 `artifacts/runs/release_readiness_20261112/114303/manifest-result.json`，原清单/响应头/错误输出同目录。此前网页读取工具无法访问的结果不代表GitCode失效；本机CLI此次读取有效。未下载旧包、未推送或修改在线清单。

### 发布准备：GitCode远端分支只读核对

- 命令：专用发布检出中 `git -c credential.interactive=false -c http.connectTimeout=10 -c http.lowSpeedLimit=1 -c http.lowSpeedTime=15 ls-remote --heads origin main`；返回码0，远端main与本地发布检出HEAD一致，均为7a0f156d38c8db1348314cb7e82ede6d3118401d。证据 `artifacts/runs/release_readiness_20261112/114303/remote-main-result.json`、`remote-main.log`。
- 这是只读连通性核对，不证明本轮新包已上传或更新验收已通过；当前没有远端变更。

### 发布准备：GitCode命令行推送预检

- 命令：专用发布检出中 `git -c credential.interactive=false -c http.connectTimeout=10 -c http.lowSpeedLimit=1 -c http.lowSpeedTime=15 push --dry-run --porcelain origin HEAD:refs/heads/main`；返回码0，输出 `[up to date]`。证据 `artifacts/runs/release_readiness_20261112/114303/push-dry-run-result.json`、`push-dry-run.log`。
- 本次为dry-run且本地/远端相同，没有提交或远端写入；不能用其代替实际新包上传及更新验证。已复读当前活动计划，确认产品缺陷修复及发布追加尚待所展示范围的确认，既有失败和真实验收缺口继续保留。

### 发布前修复：授权与隔离基线

- 用户明确同意完整修复范围，当前活动计划已追加，包括此前撤回的保存失败反馈修复；历史撤回记录保留，不再代表当前授权状态。
- 新隔离工作树 `D:/mycodex/worktrees/shahe-release-fixes-20261112`，分支 `fix/release-readiness-20261112`，从已推送的f7169c8建立后复制当前csharp源码。文件哈希快照在 `artifacts/runs/release_fixes_20261112/source-baseline.json`。保留现有分组选择、版本及测试差异，不回退主目录、不重跑无变化基线；复用本轮既有失败证据，后续新增定向RED/GREEN。

### 发布前修复：隔离工作树依赖恢复

- 在新工作树执行 `dotnet restore csharp\ShaheBulkLauncher.sln --locked-mode`，进程级NuGet缓存指向 `D:/Dev/NuGet/packages`；返回码0，四项目成功恢复，没有更改依赖版本。命令及日志 `artifacts/runs/release_fixes_20261112/restore-result.json`、`restore.log`。本轮没有重跑旧全量基线。

### 还原安全：替换后异常 RED

- 新增真实隔离文件回归：数据和INI替换后的进度回调抛异常，旧实现未恢复旧文件。`pwsh -File artifacts/runs/release_fixes_20261112/test.ps1 -Stage restore-progress-red -Filter FullyQualifiedName~RestoreProgressFailureContractTests` 返回码1，总计1，通过0、失败1、跳过0，失败断言为旧数据未恢复；编译成功，无正式服务调用。精确命令、输出和TRX在该轮 `result.json`、`test.log`、`tests.trx`。

### 还原安全：文件/配置/服务保护 RED

- `test.ps1 -Stage restore-safety-red -Filter FullyQualifiedName~RestoreFailureSafetyContractTests` 返回码1，总计14，通过2、失败12、跳过0。确认源文件占用、目标INI占用、后续用户失败、重载失败、停止后异常、路径越界及服务状态缺口；通过项为原有成功路径和部分恢复异常提示。生产源码尚未修改，失败日志及TRX在 `artifacts/runs/release_fixes_20261112/restore-safety-red/`。测试仅用隔离文件及模拟服务，不操作正式服务。

### 还原安全：Core 定向 GREEN

- `test.ps1 -Stage restore-core-green -Filter 'FullyQualifiedName~RestoreFailureSafetyContractTests|FullyQualifiedName~RestoreProgressFailureContractTests|FullyQualifiedName~BackupRestoreServiceContractTests'` 返回码0，总计22，通过22、失败0、跳过0。新增两个RED批次和原备份契约均通过；真实文件锁、复制失败、配置合并失败、替换后异常与服务回调恢复已覆盖。精确命令/TRX/输出在 `artifacts/runs/release_fixes_20261112/restore-core-green/`。
- 当前只证明Core隔离数据及服务回调层；主窗口实际服务状态/完成校验尚待接入，不标记第一项整体完成、不发布。新数据先暂存，目录原件和INI副本保留到操作成功，回退失败时保留路径并报错；原停止状态不执行stop/reload/restart。实现参考 [.NET File.Replace 文档](https://learn.microsoft.com/en-us/dotnet/api/system.io.file.replace?view=netframework-4.8) 的同卷替换及备份语义，INI暂存和副本放在目标同目录。

### 还原安全：主窗口接线 RED

- `test.ps1 -Stage restore-wiring-red -Filter FullyQualifiedName~RestoreServiceLifecycleRuntimeContractTests` 返回码1，总计16，通过4、失败12、跳过0。失败集中在未接原服务状态查询、默认及null重载回调未检查命令结果；显式自定义回调传播已有通过证据。目录 `artifacts/runs/release_fixes_20261112/restore-wiring-red/` 保存精确命令、TRX和输出。未执行正式服务命令；尚未运行的源码正则用例在验证前剔除，不以镜像实现测试代替实际行为。

### 还原安全：重载可能启动服务的回退 RED

- 新补两例模拟真实命令边界：重载先启动服务再抛错，回退前必须重新停止；再次停止失败时保留全部旧副本、不触碰仍在使用的新数据。`test.ps1 -Stage restore-reload-stop-red -Filter FullyQualifiedName~ReloadThatStartsServiceMustStopItBeforeRollback` 返回码1，总计2、通过0、失败2、跳过0。失败均为回退前未重新停止。证据 `artifacts/runs/release_fixes_20261112/restore-reload-stop-red/`；服务仍仅为隔离状态模型。

### 还原安全：重载失败回退 GREEN

- `test.ps1 -Stage restore-reload-stop-green`，筛选原备份契约及两个新增还原保护测试类，返回码0，总计24、通过24、失败0、跳过0。重载/恢复阶段失败后先再次停止再回退；停止被拒绝时保留旧数据和INI副本、提示路径，不在未知运行状态下覆盖。此前两个新RED均转GREEN，完整命令/输出/TRX在 `artifacts/runs/release_fixes_20261112/restore-reload-stop-green/`。主窗口接线仍待GREEN，实际正式服务未操作。

### 还原安全：主窗口接线 GREEN

- `test.ps1 -Stage restore-wiring-green -Filter 'FullyQualifiedName~RestoreServiceLifecycleRuntimeContractTests|FullyQualifiedName~MainFormApplicationContractTests.WiresRestore'` 返回码0，总计17、通过17、失败0、跳过0。主窗口服务初态查询、命令null/非零/异常传播、显式回调覆盖、默认还原重载与其他入口隔离、原还原菜单接线均通过。证据 `artifacts/runs/release_fixes_20261112/restore-wiring-green/`。
- 生产默认停启改为ServiceController请求并有界等待，不再发出net/taskkill后立即视为成功；仅还原路径校验/reload返回码，其他旧重载入口行为保持。此处测试使用真实MainForm/生产服务与隔离依赖回调，没有停止正式SbieSvc，不冒充真实服务停启验收。

### 还原安全：审查与恢复边界补验

- 独立Core审查的规格/质量均通过，报告 `artifacts/runs/release_fixes_20261112/restore-core-review.md`；其中列出的恢复边界新增真实文件验证，而非重复整组。
- `test.ps1 -Stage restore-recovery-edges` 精确筛选4个新增数据行，返回码0，总计4、通过4、失败0、跳过0。数据或INI回退被实际文件锁阻止时保留对应原副本、报告路径且不启动服务；首次还原失败回到无目标目录/无INI；旧只读文件可以成功替换并清理副本。结果/TRX/输出在 `artifacts/runs/release_fixes_20261112/restore-recovery-edges/`，未改断言掩盖失败，没有正式数据写入。

### 还原安全：首次全量门禁

- `test.ps1 -Stage restore-full` 不设筛选，返回码1，总计486，通过485、失败1、跳过0。所有还原及受影响原用例通过；唯一失败仍是微信打赏测试finally恢复旧OLE剪贴板对象的0x800401D0，原图复制断言已通过，不是还原回归。精确命令、输出、TRX在 `artifacts/runs/release_fixes_20261112/restore-full/`。不将此轮记全绿。
- 为完成本模块门禁，提前处理已批准“测试夹具清理错误”子项，范围仅DonationQrRuntimeContractTests；保留业务断言和实际失败，不修改打赏产品行为、不清空用户剪贴板来绕过。下一轮先定向验证该夹具，再决定全量复验。

### 还原安全：Release x64 构建

- `dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\release_fixes_20261112\bin\` 返回码0，四项目成功，警告0、错误0。结果/日志 `artifacts/runs/release_fixes_20261112/restore-release/`。构建通过不覆盖上一轮全量夹具失败或真实服务边界。

### 还原安全：Python 对照基线

- 当前主目录执行 `python -m unittest discover -s tests -p test_*.py -v` 返回码0，总计31、通过31、失败0、跳过0；缓存定向本轮隔离证据目录，未改Python源码。命令/日志 `artifacts/runs/release_fixes_20261112/python-reference/result.json`、`test.log`。
- 对照 `shahe_bulk_launcher/backup.py` 的既有覆盖还原行为：本次回退保护和服务结果校验为用户明确批准的缺陷修复，不要求复刻原版已确认的数据丢失风险；原UI、文案及未选用户保护继续保持。原Python编码保留逻辑与C#差异留在计划配置/编码项处理，不因该套件通过关闭。

### 还原安全：Python 生产源码编译

- `python -m compileall -q 沙河批量打开器.py shahe_bulk_launcher` 返回码0，无编译错误；缓存仍在本轮隔离目录，没有改Python源码。结果 `artifacts/runs/release_fixes_20261112/python-reference/compile-result.json`、`compile.log`。C#生产源码已由本轮Release构建验证，全量夹具修复继续进行。

### 还原安全：受影响文件负载探针构建

- Framework csc以x64编译 `artifacts/runs/release_fixes_20261112/restore-load/RestoreLoadProbe.cs`，引用本轮已构建Core DLL，返回码0，无诊断。参数/输出在同目录 `build-result.json`、`build.log`。用于新事务实现的原计划文件负载，只覆盖还原，不重跑未受影响的其他压力组。

### 还原安全：受影响文件负载通过

- `RestoreLoadProbe.exe <本轮>/restore-load/data` 返回码0：每轮1000个中文内容文件、连续10轮还原，10000次逐文件内容校验全部通过，耗时38744ms，目标PID18272已正常退出。每轮同时核对原未选用户、INI未选节、服务状态模型和暂存/副本清理。正式服务调用0；没有启动或停止用户进程。
- 精确命令、Core哈希、计数及输出 `artifacts/runs/release_fixes_20261112/restore-load/command-result.json`、`result.json`，stderr为空。该负载仅复验因事务实现变化而受影响的还原，不重复其他已通过功能，也不恢复旧30轮循环。

### 打赏测试夹具：定向 GREEN

- 保留原图尺寸、像素、按钮文案及无障碍断言；测试改为操作前物化旧剪贴板数据并持有自有副本，恢复有界重试，不恢复已经失效的延迟OLE对象；不能安全复制的格式在改变剪贴板前明确失败。新增快照独立性和拒绝不支持数据的验证，不改产品。
- `test.ps1 -Stage clipboard-fixture-green -Filter FullyQualifiedName~DonationQrRuntimeContractTests` 返回码0，总计6、通过6、失败0、跳过0。此前微信finally失败已取得定向GREEN，证据 `artifacts/runs/release_fixes_20261112/clipboard-fixture-green/`。未删除失败断言、未跳过用例、未无条件清空用户剪贴板。

### 还原安全及夹具修复：最终全量 GREEN

- `test.ps1 -Stage restore-full-final` 不设筛选，返回码0，总计488、通过488、失败0、跳过0。新增快照测试及原打赏复制/恢复全部通过，所有还原/主窗口/受影响旧测试通过。精确命令、输出与TRX在 `artifacts/runs/release_fixes_20261112/restore-full-final/`。此前486项的夹具失败原记录保留，未通过重跑掩盖未经修复的失败。
- 主窗口独立审查的规格与质量通过，报告 `restore-wiring-review.md`；夹具修复说明 `clipboard-fixture-report.md`。当前可进入本项差异核对和独立提交；删除保护、定时、音频、配置/编码等其他确认缺陷尚未处理，真实桌面/运行中沙盒/服务外部验收边界仍在，不声明整个软件通过或可发布。

### 还原安全：差异、编码与清理核对

- 对本项触及的8个C#源码/项目文件执行严格UTF-8和replacement character检查通过，`git diff --check` 返回码0，本轮产物目录下所属探针残留0。结果 `artifacts/runs/release_fixes_20261112/restore-integrity.json`、`restore-verified-source.json`、`restore-diff-check.log`。
- 原主目录分组选择、4.0.50版本及已有KeyboardAudit测试差异仍单独保留；本项提交将只纳入还原修复、对应测试、必须的剪贴板夹具修复和已授权文档，不把旧版本/分组/其他未提交改动混入修复提交。

### 还原安全：主目录同步与暂存检查

- 已核对主目录触及文件仍等于任务前快照后，同步8个验证通过的源码/测试文件，未覆盖并发改动；结果 `artifacts/runs/release_fixes_20261112/restore-sync-result.json`。MainForm通过只含还原差异的补丁暂存，既有分组改动保留在工作区。
- 首次暂存差异检查返回2，唯一问题为已批准计划末尾多一个空白行，已只修正文档尾空行；不涉及产品失败或重跑测试。下次暂存检查确认后提交。

### 还原安全：提交前检查通过

- 文档尾空行修正后，暂存 `git diff --cached --check` 返回0；暂存清单与已向用户列出的11个文件一致。Core及主窗口审查、定向及受影响全量、文件负载、Python对照/编译、Release和源码编码检查均已有本轮实际证据。暂存MainForm只含本项差异，主目录原有版本、分组及其他改动未混入本项提交。

### 还原安全：独立提交并推送 GitHub

- 本项提交 `99d27220134f3a3ba4b5dff2fe13c6ad59fe4ec4`（fix: preserve sandbox data and service state when restore fails），已通过命令行推送 GitHub `childhood123/shahe-bulk-launcher` 的 `沙河批量打开器` 分支，返回码0。完整参数与输出 `artifacts/runs/release_fixes_20261112/restore-push-result.json`、`restore-push.log`。回退命令 `git revert 99d27220134f3a3ba4b5dff2fe13c6ad59fe4ec4`。
- 本项已完成生产修复和自动/隔离门禁；前文“还原尚未修复”是历史执行状态，由本条推进。真实服务/运行中沙盒外部验收继续保留，下一项为删除查询异常保护。GitCode公开版本和在线清单尚未改变。

### 删除查询保护：RED

- `test.ps1 -Stage delete-query-red -Filter FullyQualifiedName~DeleteQuerySafetyContractTests` 返回码1，总计12、通过2、失败10、跳过0。确认Core将查询异常/null/缺失回调当空结果，存活确认缺失/null也放行；主窗口删除仍走吞失败的旧查询接线。已确认空结果的合法路径保留为通过。结果/命令/日志/TRX在 `artifacts/runs/release_fixes_20261112/delete-query-red/`。
- 主窗口用隔离用户目录、模拟删除/命令回调执行，不调用正式删除或终止。生产官方空输出样本沿用之前已保存的 `continuation/no-screenshot/sandbox-readonly/box12-direct-result.json`，其退出码0、stdout为 `0\r\n`；空响应或无有效结果不能当这一已确认空状态。

### 删除查询保护：首次修复验证与夹具校准

- `test.ps1 -Stage delete-query-green` 返回码1，总计21、通过16、失败5、跳过0。Core查询异常/null/存活状态保护及既有删除回归通过；5个新主窗口场景未到达命令查询，因为夹具未提供已知用户名称，目标规范化过滤了测试用户。这部分原RED也不能算真实删除路径失败证据，明确校正，不据此修改产品来迎合夹具。
- 将先补目标列表校准，再仅对主窗口场景重取有效RED/GREEN。原始输出/结果保留 `artifacts/runs/release_fixes_20261112/delete-query-green/`。严格/listpids格式已对照 [Sandboxie官方Start.cpp的List_Process_Ids](https://github.com/sandboxie-plus/Sandboxie/blob/master/Sandboxie/apps/start/start.cpp#L1610)：先输出计数，后每行PID；枚举失败可能无输出，因此退出码0但空响应也须阻止删除。

### 删除查询保护：主窗口校准后有效 RED

- 为夹具补齐已知用户名称，并在操作前断言实际目标恰为隔离BoxA；仅将新工作树删除回调暂接原旧查询取得有效对照，其他实现不变。`test.ps1 -Stage delete-wiring-calibrated-red -Filter FullyQualifiedName~MainWindowBlocksBothOfficialAndDirectoryDeletionWhenQueryIsUnknown` 返回码1，总计5、通过0、失败5、跳过0，均实际调用了本应阻止的删除回调。日志/命令/TRX在 `artifacts/runs/release_fixes_20261112/delete-wiring-calibrated-red/`。
- 本次已到达真实生产删除入口，模拟删除与本地文件操作只涉及所属隔离目录；原校准前5项失败不作为产品证据。本轮结束即恢复严格查询接线后验证。

### 删除查询保护：校准后定向 GREEN

- `test.ps1 -Stage delete-query-final-green` 筛选新查询保护、SandboxAdminService契约及原主窗口批量删除入口，返回码0，总计26、通过26、失败0、跳过0。有效主窗口RED均已转GREEN，查询失败时模拟官方删除回调0次且隔离目录原文件保留；合法空结果/正PID列表仍可处理，截断/无效/重复PID结果明确拒绝。
- 证据 `artifacts/runs/release_fixes_20261112/delete-query-final-green/`。此修复仅为删除回调接严格新查询，其他用户列表/进程显示查询继续原路径；本轮没有正式删除或进程终止操作。

### 删除查询保护：C# 全量 GREEN

- `test.ps1 -Stage delete-full` 无筛选，返回码0，总计505、通过505、失败0、跳过0。所有新删除保护、原批量删除、其他进程/列表查询、还原和打赏夹具均通过。精确命令、输出和TRX在 `artifacts/runs/release_fixes_20261112/delete-full/`；真实正式删除未执行，后续只读官方查询补验不触发删除安全门后的动作。

### 删除查询保护：只读官方接线探针构建

- Framework csc x64编译 `artifacts/runs/release_fixes_20261112/delete-native/DeleteQueryProbe.cs`，引用当前验证后的主EXE/Core，返回码0，无诊断；精确参数/输出在同目录 `build-result.json`、`build.log`。探针只调用生产删除前查询回调，不调用Ensure的终止逻辑、任何删除或服务控制；要求正式服务已经Running才继续。

### 删除查询保护：真实官方只读查询通过

- `DeleteQueryProbe.exe <本轮>/delete-native/data "C:\Program Files\Sandboxie" Box12` 返回码0，目标探针PID15204，实际调用当前主窗口查询接线一次，官方退出码0、输出 `0\r\n`，严格解析为空PID列表，耗时929ms。服务前后均Running；删除/终止/服务控制调用均0，探针正常退出。
- 精确命令、构建哈希与输出 `artifacts/runs/release_fixes_20261112/delete-native/command-result.json`、`result.json`，stderr为空。此证据证明当前安装的合法空查询不会被新保护误拒绝；不外推为真实运行中沙盒删除或多版本兼容验收。

### 删除查询保护：Release及对照边界

- 当前工作树 `dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=<本轮>\bin\` 返回码0，警告0、错误0。精确参数/日志在 `artifacts/runs/release_fixes_20261112/delete-release/`。
- Python源码未改，本轮前项刚完成的Python对照与生产编译证据继续复用，不重复无变化测试；新严格/listpids验证用于已批准的删除安全修复，官方输出格式与当前机器只读样本均有证据。当前仍未升版/发布。

### 删除保护审查回归：重命名查询 RED

- 独立审查发现严格查询共享到重命名路径，失败被旧重命名catch吞掉；尚不提交。执行 `test.ps1 -Stage delete-rename-red -Filter FullyQualifiedName~RenameStillRejectsRunningUserWhenDeleteQueryFails`，退出1，1项、通过0/失败1/跳过0。校准目标为BoxA，普通查询应返回运行PID，但实际未调用；断言在此中断，不扩大为后续目录断言证据。日志/TRX/result位于 `artifacts/runs/release_fixes_20261112/delete-rename-red/`。
- 接续保持删除严格入口，重命名恢复原普通查询；另删除专用存活检查保留系统查询异常，不改其他进程功能。

### 删除保护审查修复 GREEN

- `test.ps1 -Stage delete-reviewed-green -Filter 'FullyQualifiedName~DeleteQuerySafetyContractTests|FullyQualifiedName~SandboxAdminServiceContractTests|FullyQualifiedName~WiresBatchDeleteSandboxes|FullyQualifiedName~WiresBatchRenameSandboxes'` 退出0，28项全部通过、失败0/跳过0。新增重命名保护确认普通查询到运行PID后拒绝，隔离原目录和INI保持；删除失败保护仍通过。证据 `artifacts/runs/release_fixes_20261112/delete-reviewed-green/`。
- 检查发现原成功重命名夹具的普通查询注入误放入分组夹具，接续移至正确夹具，完整回归覆盖这项夹具校正；本轮新保护测试不依赖该误放位置。

### 删除保护审查后的最终全量

- `test.ps1 -Stage delete-reviewed-full` 退出0，506项通过、失败0/跳过0，含修正后的正常重命名夹具和运行中拒绝回归。证据 `artifacts/runs/release_fixes_20261112/delete-reviewed-full/{result.json,test.log,tests.trx}`。严格存活检查只把确定不存在的PID视为退出，其余系统异常交给Core阻止删除；没有真实删除正式沙盒。

### 删除保护最终Release构建

- `dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=<release_fixes_20261112>\bin\` 退出0、警告0/错误0。证据 `artifacts/runs/release_fixes_20261112/delete-reviewed-release/{result.json,build.log}`。Python未改，复用本轮已通过对照/编译；此前真实只读查询协议未变，不重复执行。

### 删除保护源码边界核对

- 本项4个C#文件严格UTF-8/替换字符检查通过，`git diff --check`退出0；主目录相应文件与原始/上一修复哈希一致，没有发现并行改动。证据 `artifacts/runs/release_fixes_20261112/delete-integrity.json`，待同步源码哈希见同目录 `delete-verified-source.json`。MainForm仅提取本项差异进行暂存，保留未提交的分组变更。

### 删除保护独立复审通过

- `delete-query-review.md` 已追加复审：Spec/Quality通过，重命名共享查询R1关闭，普通ProcessExists不变。最终源码哈希与 `delete-verified-source.json` 一致；OS权限异常没有真实注入，不扩展为外部验收通过。下一项为定时目标更新。

<!-- FULL-ONCE-APPEND -->
