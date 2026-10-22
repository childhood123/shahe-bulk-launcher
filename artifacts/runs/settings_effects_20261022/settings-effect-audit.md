# 设置接线与 Python 对照

## 软件设置

| 项目 | C# 实际执行位置 | 本轮结论 |
| --- | --- | --- |
| 启动间隔、并发数量 | MainForm.SyncFeatureStatesFromSettings → LaunchScheduler.RunLaunchBatch | 保存后进入批量调度参数 |
| 开机自动启动 | CreateSettingsCallbacks → ApplyAutoRun → 注册表写入边界 | 已用启用/关闭两种 RED 复现旧值写入；修复为先同步当前 AutoRun 再调用写入 |
| 在托盘上显示 | LauncherSettingsService.ApplySettings → CreateTaskBarIcon / DisableTrayDisplay | 已有真实托盘创建/移除接线 |
| 首尾两点自动勾选中间用户 | SetRangeAutoCheckEnabled → CheckableListCtrl.CheckItem | 生产保存链测试实际勾选首尾并检查中间项 |
| 启动跳过已运行的沙盒 | LaunchState.SkipRunningSandboxOnLaunch → RunLaunchBatch PID 分支 | 已接调度执行，后续真实 Sandboxie 审计仍独立保留 |
| 记忆进程音量 | ProcessState / LaunchState → 记忆与 RestoreSavedVolume 回调 | 已接存储和恢复链，真实音频设备验收不能只靠配置测试 |
| 用户列表左右光标选择 | SetLeftRightNavigationEnabled → CheckableListCtrl.OnKeyDown | 已接运行开关和键盘处理 |
| 日志输出 | MainForm.LogOutputMode → LogOperation | 保存链测试实际核对开关后的记录调用 |

快捷键菜单接到 HotkeySettingsDialog 和 ProcessCmdKey；固定基础快捷键与可关闭的功能快捷键按 Python 原有规则分开。路径记忆功能和最大 999 保留用户已明确要求。

## 原版遗留：读屏接口选择

Python features/speech.py 的 _save_speech_interface_dll 和 C# SpeechService.SaveSpeechInterfaceDll 均只归一化、保存和回显；实际 Tolk 初始化/输出未读取所选接口。不得宣称选择 NVDA、争渡或保益能强制切换实际输出。该问题属于 Python 原有行为，需要另立功能修复范围，不能静默混入本轮设置时序修复。

“启用读屏辅助提示”两端均有实际发声入口门禁，但不会立即终止已经播报或已排队的语音。语速、音调和厂商 INI 参数不属于当前 Python 可见设置页，不能把遗留模型当成已提供的 UI 功能。

## 菜单

独立静态对照发现 C# 多出“路径记忆管理”设置菜单入口，并缺少用户管理、校正、关于的三条分隔线；在 SETTINGS-01 完成验证提交后单独修正。本轮不改变中文文案或菜单结构。
