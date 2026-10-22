# AUDIT-01 结果

首次真实隔离启动发现首次初始化时序竞态：`Program` 在 `Application.Run` 前调用 `InitializeApplication(true)`，外部关闭可能让消息循环随后访问已释放窗体。已改为 `InitializeApplication(false)`，先创建托盘，再交给 `Application.Run` 首次显示。

最终隔离烟雾使用假 Sandboxie `Start.exe`、`show_tray=false`、扫描完成/就绪等待和 WM_CLOSE fallback，取得主窗口句柄与标题并以退出码 0 正常退出。工具合约 6/6、受影响 120/120、C# 全量 344/344、Python 29/29、compileall 0、Release x64 0 警告/0 错误、范围差异检查返回 0。

这不是完整定位功能审计；无窗口、多窗口、失效句柄和真实用户进程定位仍待后续审计。
