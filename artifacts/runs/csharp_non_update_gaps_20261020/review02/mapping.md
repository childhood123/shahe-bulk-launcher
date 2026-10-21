# REVIEW-02 映射

- Python `features/sandboxie.py::start_app_in_sandbox`：首次返回“无效的沙盒名参数”时调用 `_reload_sandboxie_config()`，重试一次，再重新读取官方沙盒集合并刷新列表/反馈。
- C# `SandboxieClient.StartAppInSandbox` 已有 reload、重试和 `ListOfficialSandboxNames` 分支；缺口在 `MainForm.StartAppInSandbox` 只传了刷新映射、刷新列表和状态回调，两个恢复回调保持 null。
- 修复复用现有 `ReloadSandboxieConfigCallback`/`ReloadSandboxieConfig()` 和 `GetOfficialSandboxNames()`，不新增 Sandboxie 命令实现。
