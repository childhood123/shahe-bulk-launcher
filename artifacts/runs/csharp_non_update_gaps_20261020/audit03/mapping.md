# AUDIT-03 Sandboxie 生命周期

先实现计划要求的只读 `verify-clean`：重用 preflight 的哈希、配置、INI、目录、进程和服务读取，输出独立 verify-clean 报告，绝不修改资源。其余生命周期阶段仅在通过 preflight、精确 ShaheAudit_* 所有权和安全进程门禁后执行。

本轮异常路径映射：SandboxieLifecycleAudit 生命周期入口 → AuditSafety 所有权/回滚/恢复检查；LaunchScheduler 的真实定时回调 → 原生 Timer 完成等待；SandboxieClient 命令 → 执行前精确所有权复验和命令后观察。审计器直接写 SbieIni 的重命名/删除仅证明审计工具动作，不视为 MainForm 生产入口已经验收。
