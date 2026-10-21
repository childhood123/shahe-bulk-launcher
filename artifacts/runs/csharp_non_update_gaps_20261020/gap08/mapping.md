# GAP-08 Sandboxie repair reload 映射

- Python 参考：`shahe_bulk_launcher/features/sandbox_admin.py::_repair_sandboxie_control_user_settings`。
- Python UI：`shahe_bulk_launcher/features/sandboxie.py::on_repair_sandboxie_control_settings`。
- C# 服务：`SandboxAdminService.RepairSandboxieControlUserSettings()` 已按 Python 行为实现：只有实际清理并写入后才调用 reload，且吞掉 reload 异常后仍返回修复成功。
- C# reload：`MainForm.ReloadSandboxieConfig()` 已复用 Sandboxie `Start.exe /reload`，缺失时回退 `Sandboxie.exe /reload`。
- 确认缺口：`MainForm` 正常构造后 `ReloadSandboxieConfigCallback` 为 `null`；repair 只执行 nullable callback，生产路径可能写入成功但不 reload。
- 最小实现：在 `MainForm` 构造函数中把现有 `ReloadSandboxieConfig()` 绑定到 `ReloadSandboxieConfigCallback`，不新增第二套命令实现。
- 验证边界：使用临时隔离 INI 验证取消、无需修复、写入失败、实际写入一次 reload 和 reload 异常反馈；不读取或修改用户 Sandboxie.ini。
