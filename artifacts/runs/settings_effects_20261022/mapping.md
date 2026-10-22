# 设置真实生效核查

用户要求沿用 Python 全部中文文案、菜单排列，C# 负责真实功能。本轮只验证和修复普通软件设置保存后的执行接线，不修改 UI 文案或排列。

Python SettingsDialog.on_ok → SettingsMixin.on_settings_saved 先更新 auto_run，再调用 _apply_auto_run。C# SettingsDialog.SaveSettings → MainForm.ApplySettingsFromDialog → LauncherSettingsService.ApplySettings → CreateSettingsCallbacks.ApplyAutoRun；主窗口 AutoRun 目前在服务回调之后才更新，存在旧值写入风险。

失败测试从生产设置窗体保存进入真实 MainForm 回调，同时检查启动间隔/并发、运行跳过、音量记忆、列表开关、日志输出、配置持久化与注册表边界接收的新值。注册表写入以记录回调替代，列表使用隔离数据，不操作真实沙河或启动项。
