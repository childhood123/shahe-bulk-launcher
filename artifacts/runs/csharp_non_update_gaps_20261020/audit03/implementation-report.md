# AUDIT-03 审计器安全修复记录

本次提交是审计器修复检查点，AUDIT-03 真实场景尚未全部完成。当前业务中文文案和 Python 对照范围没有扩大。

已修复：部分重命名和回滚命令失败的恢复处理、原子状态文件替换、已恢复配置和部分删除的可重复清理、精确所有权残留检查、进程查询失败不能当成零进程、所有权变化后的误终止保护、定时回调与异常清理互斥、每次删除前重新验证所有权和空闲状态。新增回归测试覆盖这些真实行为边界，RED/GREEN 与最终门禁的权威索引位于 PROJECT_STRUCTURE.md 最新记录。

干净索引快照检查还补齐了已提交 MainForm 所依赖的 HotkeyController 状态成员、设置重置异步扫描断言，以及新增 AuditRunner 项目引用所需的测试锁文件。其余用户日志和未跟踪目录未纳入。

真实 Sandboxie 已执行批量创建、建组、分组启动、定时启动及精确清理。修正后的 verify-clean 绑定原始基线和状态文件复查，精确资源均不存在，目标、配置与 Sandboxie.ini 哈希匹配；当前 SbieCtrl.exe 仍使完整门禁返回 blocked。SbieCtrl 在早期 preflight 中不存在，测试期间出现，启动归属未确认；没有主动结束该进程。

尚缺 delete-group、schedule-close、cancel-schedules、batch-rename、batch-delete 的完整真实成功证据。审计器直接调用 SbieIni/配置的重命名和删除代码也不能替代 MainForm 生产批量入口的交互验证。后续仍先完成 AUDIT-03，再进入 AUDIT-04；整体迁移和最终发布验收未完成。

独立只读评审确认本轮发现的安全问题已在当前审计路径修复；该评审不替代真实场景验收。
