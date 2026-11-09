# 用户列表未通过项目修复

用户已在完整计划展示后回复“开始修复”，允许执行本计划并按实际结果追加记录、勾选通过项。沿用现有项目标准，不改 AGENTS.md。使用 executing-plans、TDD 与最小修复流程，按任务顺序实施。

## 目标、边界和文件

目标：修复全部进程操作遗漏用户，查明并修正播放设备切换问题，补齐用户列表菜单的实际验收。

边界：保留现有菜单、顺序、快捷键和正常文案；只操作隔离测试进程，不停止正式程序或读屏。保留已有工作区改动，不修改 Python，不升级版本或发布包。代码改动按模块验证、记录并独立提交推送；桌面阻断与产品缺陷分开记录。

涉及文件：

- `csharp/ShaheLauncher.Core/Processes/ProcessActionService.cs`
- `csharp/ShaheLauncher.App/MainForm.cs`
- `csharp/ShaheLauncher.Core/Processes/WindowsProcessAudioBackend.cs`（确认根因后按需修改）
- `csharp/ShaheLauncher.Tests/ProcessActionServiceContractTests.cs`、`AllProcessTargetsContractTests.cs`，及对应音频和菜单契约测试。
- `PROJECT_STRUCTURE.md`、`docs/migration/2026-11-09-user-list-acceptance.md`。

隔离工作树使用 `D:/mycodex/worktrees/shahe-user-list-fixes-20261109`；证据统一存放主工作区 `artifacts/runs/user_list_fixes_20261109/`。原始失败证据保留在上一轮验收目录，不覆盖旧日志。实时计数、提交和运行证据只在 PROJECT_STRUCTURE.md 验证记录维护。

## 任务一：全部进程操作

- [x] 核对已发布源码与主目录现有改动，建立隔离修复基线。
- [x] 补 RED：两个用户均运行而快速查询只返回一个时，全部目标仍包含两者；覆盖合并去重、无查询回调、逐用户查询异常及所有共享动作。
- [x] 最小修复部分结果补查，不在菜单打开或焦点移动时新增同步扫描；关闭结果以完整目标及实际存活情况判断。
- [x] GREEN：新契约通过；更新与正确行为冲突的旧断言，保留其他状态和异步断言。
- [x] 完成受影响回归、一次 C# 全量、隔离 Release x64、真实隔离进程复验及差异检查；每轮立即记录。
- [x] 复核暂存文件，只提交本项改动，推送 GitHub 并记录哈希和回退命令，然后进入任务二。

## 任务二：播放设备切换

- [x] 对照 Python 后端，区分路由设置、新建音频流和现有音频流，确认上一轮现象的根因与支持边界。
- [ ] 对确认缺陷建立失败测试，再实施最小修复；不得仅凭原生接口返回成功判定音频已切换，不强制重启用户进程。
- [ ] 分别核对当前／全部、目标隔离、失败反馈和实际端点；完成对应回归、全量、隔离构建、记录与独立提交推送。

## 任务三：真实菜单

- [ ] 排除验收夹具异常，取得有效桌面连接和窗口／焦点证据。
- [ ] 补测右键、菜单键、Shift+F10、取消、定位、置顶、选择、刷新、目标保持和读屏反馈；发现问题逐项修复和验证。
- [ ] 清理本轮测试进程，复读计划；按已修复、未验收和环境阻断分别汇报，不把服务测试代替菜单端到端证据。

## 验证命令与完成标准

在隔离工作树运行；先恢复已锁定依赖，仅当缺少 assets 时执行 `dotnet restore csharp/ShaheBulkLauncher.sln --locked-mode`。

```powershell
dotnet test csharp/ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:/mycodex/git/artifacts/runs/user_list_fixes_20261109/task1/bin/ --filter FullyQualifiedName~AllProcessTargetsContractTests
dotnet test csharp/ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:/mycodex/git/artifacts/runs/user_list_fixes_20261109/task1/bin/
dotnet build csharp/ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=D:/mycodex/git/artifacts/runs/user_list_fixes_20261109/task1/release/
git diff --check
```

每次运行另加相应 TRX 和文本日志参数，并记录实际命令、返回码与结果。音频／菜单任务使用自己的隔离输出目录，避免覆盖证据。Python 未改，复用有效对照基线，仅补本次差异必要核对。已通过的无关测试不反复执行。

完成标准：每个代码修复有对应失败与通过证据；实际音量、静音、退出和端点均核对真实测试对象；真实菜单与服务验证分别记录。桌面不可用或播放器不支持现有音频流迁移时明确保留未完成项，不以单项通过宣称整个软件迁移完成。

## 本轮执行状态

- 任务一已完成并推送，主目录同步了相同修复；已通过项不再重复运行。
- 任务二完成同机后端对照，路由写入和实际音频流生效已分开记录；实际播放器适配边界尚未解决，后两项不勾选。
- 任务三已排除旧夹具初始化异常，但桌面截图捕获后会话失效，未取得菜单操作证据；已清理测试进程，未将环境阻断记成产品通过。
- 下一步需恢复桌面工具连接，并对实际目标播放器做针对性设备切换核验。整体迁移未完成，未升级版本或发布包。
