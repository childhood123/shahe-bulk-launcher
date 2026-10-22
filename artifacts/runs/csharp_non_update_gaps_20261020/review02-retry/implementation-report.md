# REVIEW-02 结果

`MainForm.StartAppInSandbox` 现在在首次尝试前刷新官方集合；启动失败重试会刷新缓存后再判断禁用沙盒。`ReloadSandboxieConfig` 对 Start.exe 使用现有同步命令执行器并等待最多 6 秒；reload 异常仍继续重试，保持 Python 行为。

定向 3/3、受影响 122/122、C# 全量 337/337、Python 29/29、compileall 返回 0、Release x64 0 警告/0 错误、差异检查返回 0。真实 Sandboxie 资源未触碰。
