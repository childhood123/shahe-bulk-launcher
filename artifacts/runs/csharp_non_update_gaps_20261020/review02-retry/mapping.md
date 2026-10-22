# REVIEW-02 Sandboxie 启动恢复映射

Python `sandboxie.py::start_app_in_sandbox` 每次启动先查询官方启用沙盒；第一次遇到 invalid sandbox name 时同步 reload，捕获 reload 异常后仍进行第二次 dispatch，再重新查询官方集合，失效时刷新映射和列表。
C# 入口是 `MainForm.StartAppInSandbox` -> `SandboxieClient.StartAppInSandbox` -> `CreateSandboxieLaunchCallbacks`。修复只复用现有 `SandboxieClient.RunSandboxieStart` 的有界同步执行和已有刷新方法，不新增启动协议。
验证覆盖：生产回调工厂不读取旧官方缓存、初始查询和重试查询使用新数据、reload 异常仍继续第二次 dispatch；随后定向、Sandboxie 受影响、C# 全量、Python、compileall、Release、差异检查。
