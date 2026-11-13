# 附加朗读操作结果

用户已确认按本范围实现、记录验证、勾选完成项及按原规则提交；随后明确排除全选/全都不选的附加朗读，并要求快捷键执行同样调用辅助朗读接口。该最新指令作为本计划的直接修订授权。

## 目标与边界

复用朗读设置中的“启用读屏辅助提示”、所选朗读接口及现有配置键，不新增开关或调整控件顺序。启用时，在操作完成后简短播报对象与真实结果；关闭时不附加朗读。正常读屏的焦点和控件状态反馈保持。菜单、快捷键及已有按钮入口复用同一完成回调，不在按键按下时提前播报成功。

范围：当前/全部静音和取消静音、音量、播放设备、关闭进程、定位、置顶/取消置顶、手动刷新、启动完成。全选、全都不选，无论菜单还是快捷键均不增加辅助朗读。自动刷新、启动准备/扫描/逐项进度不增加附加朗读。

文案例：用户A音量调到55%；存在多个不同音量时明确“平均音量”；已定位到用户A进程；用户A已静音；已关闭用户A进程；已启动2个用户。部分成功、无目标和失败必须真实简短，取消不能报成功。快速连续调音量避免排队读过期结果。启动只读一次最终结果，部分失败同时说明失败数量。

只改C#附加朗读接线和结果反馈，不改底层业务语义、不改Python、不恢复旧压力循环。既有分组焦点/选中数量细节、真实输入和发布门禁继续保留。本轮不以测试回调证明真实接口发声或真实沙盒外部效果。

## 文件

- `csharp/ShaheLauncher.Core/Speech/SpeechService.cs`：开关、允许的事件、去重及过期音量抑制。
- `csharp/ShaheLauncher.Core/Processes/ProcessActionService.cs`：按实际执行结果生成短反馈。
- `csharp/ShaheLauncher.Core/Scheduling/LaunchScheduler.cs`：最终启动反馈。
- `csharp/ShaheLauncher.App/MainForm.cs`、`MainForm.Lifecycle.cs`、`UI/Controls/CheckableListCtrl.cs`：置顶和手动刷新接线；菜单与快捷键共用结果入口。
- 必要时复用 `SandboxAdminService.cs` 的置顶结果回调；现有朗读设置保存和立即生效由测试核对，不重设界面。
- 对应专项/运行契约测试、本计划与 `PROJECT_STRUCTURE.md`；隔离脚本及结果置于本轮 `artifacts/runs/`。

## 步骤

- [x] 建立隔离工作区和结果目录，补失败测试，记录RED；复用未变的既有基线。语音队列、启动结果、进程结果和App接线均保留了实际RED证据并逐项转为GREEN。
- [x] 接入进程操作和启动的真实结果短朗读。结果反馈覆盖成功、部分成功、无目标、失败和取消，启动只保留最终一次反馈。
- [x] 接入置顶和手动刷新，核对菜单/快捷键共用、全选/全都不选无附加朗读。列表F5、刷新快捷键和菜单走同一完成回调；全选与全都不选没有新增辅助朗读。
- [x] 核对开关关闭、保存后立即生效、快速音量过期抑制和关闭清理，完成定向GREEN与受影响回归。复审后受影响回归为206/206通过。
- [x] 完成一次当前C#全量、隔离Release、差异/编码检查，复用未修改的Python对照与编译证据；每轮结束立即记录到项目结构。复审后全量为659/659通过，Release为0警告/0错误，`git diff --check`退出0。
- [ ] 复读计划、审查实现和风险、同步主目录本次文件并核对内容，独立提交并按既有规则推送源码，报告提交及回退方式。真实输入/发声和公开包若未做，明确保留未验收。

## 验证命令

```powershell
dotnet restore csharp\ShaheBulkLauncher.sln --locked-mode
dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=<本轮目录>\bin\ --results-directory <本轮阶段目录> --logger "trx;LogFileName=tests.trx" --filter <定向或受影响条件>
dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=<本轮目录>\bin\ --results-directory <本轮全量目录> --logger "trx;LogFileName=tests.trx"
dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore --property:Platform=x64 --property:BaseOutputPath=<本轮目录>\bin\
git diff --check
```

依赖使用既有NuGet缓存；生产用户进程不因输出锁定被停止。结果的实际命令、退出码、计数、哈希和路径只在PROJECT验证记录维护。

## 完成标准与风险

每个RED对应GREEN；最终受影响回归、完整测试和构建通过，操作失败/部分成功/取消不误报；辅助朗读关闭后无新增输出，菜单和快捷键一致，全选/全都不选保持无附加输出。仅改变辅助朗读文案，已有状态/日志保留排障细节。独立记录自动化与真实读屏/键盘边界；既有候选ZIP不自动含有本轮源码变化，不声称整个软件迁移或发布已完成。

当前剩余风险：SpeechService 对原生读屏后端的 `Load`、`Output`、`Unload` 仍是同步调用；如果第三方后端自身永久阻塞，关闭流程可能等待，当前没有引入不安全的强制超时或卸载。真实读屏听感、真实键盘/菜单、运行中沙盒和公开包尚未因本轮自动化测试而重新验收；这些边界不能由契约测试替代。
