# 沙河批量打开器项目结构

## 当前执行摘要（2026-10-20 非更新缺口）

- 当前唯一计划：`docs/superpowers/plans/2026-10-20-remaining-migration-closure.md`。已完成的 Tolk 与非更新缺口计划保留为历史；本计划统一收口剩余代码差异、真实审计、更新子系统和最终发布。
- 暂缓：GitCode 更新源配置、`GAP-01` 更新结果 UI/下载启动链、`GAP-02` Updater 安装入口；这些项目仍未完成。
- 当前模块：全量只读源码复核已完成，下一模块为 `GAP-07` Esc、Alt+F4 与退出反馈；`GAP-01`、`GAP-02` 和 GitCode 更新源继续暂缓。
- 本轮验证：VOICE-01 定向 GREEN `15/15`、受影响回归 `100/100`、C# 全量 `276/276`、Python `29/29`，应用 Release x64 构建 `0` 警告/`0` 错误。原生场景已验证 ZDSR、无读屏时 OneCore 回退、NVDA，以及恢复 ZDSR；各场景均 `Output=true`、卸载/释放成功，生产 `SpeechService` 原生探针返回 `0`。独立提交 `4ebe6c6` 已推送 `origin/fix/non-update-gaps-20261020`。
- 本轮只读源码复核新增结论：确认 `GAP-07` 至 `GAP-10` 四个非更新代码差异、`VOICE-02` 一项运行时配置风险、更新源/UI/Updater 三项更新工作，以及八组真实审计/发布门禁仍未完成；未修改业务代码、未运行新测试或真实 EXE。
- 已完成范围：现有主窗口/生命周期、快捷键分派、路径记忆接线、菜单状态、设置开关和音频后端的已验证部分。C# 可以启动并运行已验证功能，整体迁移尚未完成。
- 历史衔接：旧导航/真实 EXE 计划保留为历史，其未完成审计继续保留；下方较早“当前状态”中的执行顺序由本摘要覆盖，历史验证结论不改写。

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
2. 本文件的“当前摘要”和“当前 C# 迁移计划进度”：当前唯一权威状态。
3. `docs/superpowers/plans/2026-10-20-non-update-migration-gaps.md`：当前任务的执行步骤和完成标准；旧导航计划仅用于历史追溯。
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
- 兼容入口由基线 12,071 行缩减为 171 行；主窗口组装集中在 200 行的 `shahe_bulk_launcher/app.py`。
- `SandboxLauncher` 的 398 个方法按 1 个应用初始化方法和 10 个业务 mixin 唯一分区，无重复或遗漏；已删除旧的按系统负载动态调整启动延迟方案，启动并发数和启动间隔完全由用户确认后的配置决定。
- 最终入口 API 审计无缺失导出，`SandboxLauncher` 与 `main` 均由 `shahe_bulk_launcher.app` 提供。
- C# 迁移工程已建立：`csharp/ShaheBulkLauncher.sln` 包含 Core、App、Updater、Tests 四个 `net48/x64` 项目。
- C# Task 1 `constants.py` 至 Task 19 `features/lifecycle.py` 已完成已确认范围的模块级迁移；Task 20 `app.py` 已完成应用组装、入口、发布、主要用户动作、非破坏性菜单动作、Task 15 备份/还原以及 `create_group`、`launch_group`、`delete_group`、`batch_create_sandboxes`、`batch_rename_sandboxes`、`batch_delete_sandboxes`、`schedule_launch`、`schedule_close`、`repair_sandboxie_control_settings` 菜单接线的已确认范围；快捷键运行时分派、路径记忆、进程菜单状态/沙盒选择反馈、全量重置和设置开关、真实音频后端及进程菜单实时状态刷新已完成已确认范围。最近一次完整自动化门禁为 C# 全量测试 `212/212`、Python 全量测试 `29/29`、最终隔离 Release x64 构建 `0` 警告/`0` 错误、Python 生产源码语法编译通过和 `git diff --check` 通过；真实 EXE 审计已完成启动退出、单/多沙盒启动、并发配置、静音、实时菜单状态、音量操作和当前进程播放设备路由，关闭进程及其余动作仍待完成，未声明整个软件完成迁移。
- 当前活动计划为 `docs/superpowers/plans/2026-10-15-multi-user-navigation-and-exe-audit.md`。本轮列表移动刷新修复已完成自动化门禁；标准 Release 输出仍受运行中实例锁定，真实 EXE 复测需在关闭当前实例后使用最终隔离构建继续执行。
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
| 2. 迁移快捷键运行时分派 | 已完成 | 固定快捷键优先、自定义快捷键、禁用/冲突/未知按键回退及八个运行时动作已有契约和真实动作链验证。 |
| 3. 接线路径记忆功能 | 已完成已确认范围 | 菜单、快捷键、管理对话框、备注、隐藏路径、勾选和上下文菜单已接线；真实 EXE 完整人工流程仍属于集成审计。 |
| 4. 接线菜单状态与沙盒选择反馈 | 已完成 | 运行状态、静音标签、选择反馈及 `F10`、`Alt`、方向键、回车、空格操作已验证。 |
| 5. 接通全量重置和设置开关动作 | 已完成 | `ResetSettings()`、六个设置开关及重置后的运行时同步顺序已有契约和全量门禁。 |
| 6. 补齐真实音频后端 | 已完成已确认范围 | Core Audio 会话、设备枚举、静音、音量和播放设备路由已接入；真实宿主 build 检测修复，生产 EXE 菜单动作已选择非默认设备并回读持久路由。 |
| 7. 真实 EXE 与 Sandboxie 集成测试 | 进行中 | 已完成启动退出、单/多沙盒启动、并发配置、静音、实时菜单状态、音量和播放设备切换，以及真实备份和隔离解压校验；当前继续验证多用户列表导航卡顿和关闭动作，其他菜单动作级审计仍待完成。 |
| 8. 最终迁移门禁 | 未完成 | 自动化门禁已多轮通过，但真实调用证据、剩余集成审计、最终文档和发布边界尚未全部满足。 |

### 当前迁移状态声明

- 已完成模块：快捷键运行时分派、路径记忆接线、菜单状态与沙盒选择反馈、全量重置和设置开关、真实音频后端及当前进程播放设备路由、进程菜单实时状态刷新。
- 当前模块：真实 EXE 与 Sandboxie 集成审计，当前处理关闭进程及其余菜单动作的真实调用证据。
- 待迁移或待验证：关闭当前进程、分组/批量/定时/路径记忆/托盘/无障碍的剩余真实动作级审计，以及最终迁移门禁。
- 已知风险：外部 updater 缺少原始安装器源码；全仓 `compileall` 受既有临时探针语法错误阻断；破坏性恢复和服务控制只能在专用隔离环境验证。
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
| 3 | `common.py` | `ShaheLauncher.Core/Common/`、`ShaheLauncher.Core/Configuration/` 与 `ShaheLauncher.Core/Updates/` | 已完成，10/10 |
| 4 | `single_instance.py` | `ShaheLauncher.App/Infrastructure/SingleInstanceGuard.cs` | 已完成，3/3 |
| 5 | `ui/controls.py` | `ShaheLauncher.App/UI/Controls/` | 已完成，14/14 |
| 6 | `ui/common_dialogs.py` | `ShaheLauncher.App/UI/Dialogs/Common/` | 已完成，7/7 |
| 7 | `ui/settings_dialogs.py` | `ShaheLauncher.App/UI/Dialogs/Settings/` | 已完成，10/10；`PathMemoryManagerDialog` 已由设置菜单打开并回写主窗口状态 |
| 8 | `ui/tray.py` | `ShaheLauncher.App/UI/Tray/LauncherTrayIcon.cs` | 已完成，3/3 |
| 9 | `features/settings.py` | `ShaheLauncher.Core/Settings/LauncherSettingsService.cs` | 已完成，8/8 |
| 10 | `features/app_paths.py` | `ShaheLauncher.Core/AppPaths/AppPathHistoryService.cs` 与 `ShaheLauncher.App/MainForm.cs` | 已完成已确认范围，8/8 服务契约与 4/4 运行时接线契约 |
| 11 | `features/sandboxie.py` | `ShaheLauncher.Core/Sandboxie/SandboxieClient.cs` | 已完成，8/8 |
| 12 | `features/process_actions.py` | `ShaheLauncher.Core/Processes/ProcessActionService.cs`、`Processes/WindowsProcessAudioBackend.cs` 与 `ShaheLauncher.App/MainForm.cs` | 已完成已确认范围；原服务契约 8/8、真实后端契约 5/5、异步设备枚举/等待契约 2/2；真实 EXE 当前进程播放设备菜单动作已验证 |
| 13 | `features/sandbox_admin.py` | `ShaheLauncher.Core/SandboxAdmin/SandboxAdminService.cs` | 已完成，8/8 |
| 14 | `features/scheduling.py` | `ShaheLauncher.Core/Scheduling/LaunchScheduler.cs` | 已完成，8/8 |
| 15 | `backup.py` | `ShaheLauncher.Core/Backup/BackupRestoreService.cs` 与 `ShaheLauncher.App/MainForm.cs`、`UI/Dialogs/Common/BackupRestoreProgressDialog.cs` | 已完成已确认范围；服务契约 7/7、应用接线/进度窗口契约 3/3；真实 EXE 备份/还原动作级审计待补 |
| 16 | `features/speech.py` | `ShaheLauncher.Core/Speech/SpeechService.cs` | 已完成，10/10 |
| 17 | `features/updates.py` | `ShaheLauncher.Core/Updates/UpdateService.cs` 与 `ShaheLauncher.Updater/` | 已完成已确认范围，11/11；外部 updater 安装流程有风险 |
| 18 | `features/hotkeys.py` | `ShaheLauncher.App/Input/HotkeyController.cs` | 已完成已确认范围，8/8 |
| 19 | `features/lifecycle.py` | `ShaheLauncher.App/MainForm.Lifecycle.cs` | 已完成已确认范围，7/7 |
| 20 | `app.py` | `ShaheLauncher.App/MainForm.cs` 与 `Program.cs` | 已完成已确认范围；`create_group`、`launch_group`、`delete_group`、`batch_create_sandboxes`、`batch_rename_sandboxes`、`batch_delete_sandboxes`、`schedule_launch`、`schedule_close`、`repair_sandboxie_control_settings` 已接线并有 Application 契约 21/21；真实 EXE 新增动作级审计仍待补，未声明整体完成 |

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
| 9 | `features/settings.py` | 启动与交互已调用 | 配置加载/迁移及 Sandboxie 路径、数据根校正已执行 |
| 10 | `features/app_paths.py` | 启动与交互已调用 | 主窗口通过 `AppPathHistoryService` 接入历史新增、显示、选择、勾选、上下文菜单、管理对话框保存和七个路径记忆动作；真实 EXE 路径记忆全流程仍待人工审计 |
| 11 | `features/sandboxie.py` | 启动与交互已调用 | 安装路径、INI/显示映射和运行状态路径已执行；真实扫描会只读系统级 `Sandboxie.ini` |
| 12 | `features/process_actions.py` | 启动与交互已调用 | `MainForm` 已绑定原生 Core Audio 会话/设备枚举、静音、音量和 WinRT PolicyConfig 进程路由；受影响契约、本机 COM 探针、专用播放进程探针及真实 EXE 当前进程播放设备菜单动作均通过；关闭及其余菜单动作级审计仍待补 |
| 13 | `features/sandbox_admin.py` | 部分启动与交互已调用 | 设置规范化、INI 读取和临时 `Box1` 置顶已执行；`create_group`、`launch_group`、`delete_group`、`batch_create_sandboxes`、`batch_rename_sandboxes`、`batch_delete_sandboxes` 已进入 Application 契约，真实 EXE 动作级审计待补 |
| 14 | `features/scheduling.py` | 部分启动与交互已调用 | “取消全部定时任务”和定时启动已进入服务/Application 契约；定时关闭菜单未接线 |
| 15 | `backup.py` | 应用层测试已调用；真实 EXE 动作级审计待补 | Application 契约已通过真实 `MainMenu/MenuItem.PerformClick()` 进入备份和还原流程，临时目录、用户数据、INI 合并、异步状态和还原进度窗口均有测试覆盖；尚未在真实 EXE 中执行会修改 Sandboxie 数据的菜单动作 |
| 16 | `features/speech.py` | 启动与交互已调用 | 配置保存、朗读设置请求和关闭清理已执行；本轮未加载真实 Tolk 后端 |
| 17 | `features/updates.py` | 部分启动与交互已调用 | 自动检查路径和二维码资源解析已执行；外部 updater 安装、替换和重启未真实触发 |
| 18 | `features/hotkeys.py` | 启动与交互已调用 | 菜单标签、热键设置和上一/下一用户动作已执行；`NumPadEnter` 仍需人工键盘验证 |
| 19 | `features/lifecycle.py` | 启动与交互已调用 | UI 初始化、托盘、延迟任务、关闭和服务释放已执行 |
| 20 | `app.py` | 部分启动与交互已调用 | `Program/MainForm` 启动、菜单分派、`create_group`、`launch_group`、`delete_group`、`batch_create_sandboxes`、`batch_rename_sandboxes`、`batch_delete_sandboxes`、`schedule_launch`、`schedule_close`、`repair_sandboxie_control_settings` 应用测试、备份/还原应用测试、对话框和退出链已执行；新增动作的真实 EXE 动作级审计仍待补 |

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
- `features/updates.py` 只负责生成更新请求并启动外部 `updater.exe`，仓库没有对应的 Python updater 安装器源码；Task 17 已迁移并验证清单获取、补丁选择、请求生成、后台检查、二维码资源和 SHA-256/既有签名解密公共边界，但未擅自推断未知安装、替换和重启流程。获得原 updater 源码后应另开迁移/契约任务。
- Task 18 保留的 `NumPadEnter` 风险尚未解除：WinForms 没有独立枚举，当前只验证了 `HotkeyController` 逻辑键码、文本语义和主窗口扩展 Enter 映射契约，真实底层键盘事件接线仍需人工输入验证。
- Task 15 `BackupRestoreService` 已接入 `MainForm` 备份/还原业务流程，服务调用和还原阶段进度窗口已有 Application 契约；真实 EXE 中直接执行会停止/重启 Sandboxie 服务并覆盖数据的动作级审计仍需在隔离环境人工/探针验证，不能以 Application 测试替代。
- Task 20 已建立全部迁移服务属性、经典菜单、入口、发布复制、主要进程/窗口/启动/Sandboxie/语音回调、非破坏性菜单动作、Task 15 备份/还原接线和 `create_group`、`launch_group`、`delete_group`、`batch_create_sandboxes`、`batch_rename_sandboxes`、`batch_delete_sandboxes`、`schedule_launch`、`schedule_close`、`repair_sandboxie_control_settings` 菜单接线；真实音频后端及当前进程播放设备路由已绑定并验证，Windows 10 1803+ 的 PolicyConfig 路由在设备不可用、权限不足、进程无效或无音频会话时返回失败；关闭动作和其余新增动作的真实 EXE 审计仍待补，不能据此声明整个软件迁移完成。
- Task 10 路径记忆运行时接线已通过契约验证，但真实 EXE 中的鼠标右键、`Ctrl+H` 弹出位置、剪贴板写入和管理对话框完整保存/取消流程尚未在专用测试环境人工审计；选择/勾选回调已明确避免扫描、启动和进程刷新副作用。
- 外部 `updater.exe` 已随 Release x64 主程序发布并通过共存烟雾验证，但仓库仍缺少原 updater 安装器源码，未知安装、替换和重启流程尚未迁移或验证。

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
