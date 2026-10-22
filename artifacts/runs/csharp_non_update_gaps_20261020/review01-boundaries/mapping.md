# REVIEW-01 边界映射
Python scheduling.py：共享启动先忙时拒绝，再检查目标，再应用路径预检；普通 on_launch 先检查应用勾选，后台刷新可用集合，回 UI 后启动；分组先解析；定时执行保留捕获目标。
C# MainForm 对应 StartLaunch、LaunchSelectedSandboxes、LaunchGroup、LaunchCallbacks.LaunchSelectedSandboxes。移出共享入口中的缓存过滤，复用列表扫描和 Core.ResolveLaunchEntries；忙时禁止任何会话/路径改写。
有效 RED 检查忙时/空目标优先级、普通启动先调度再扫描、空可用集合拒绝、内部名别名和定时捕获目标；随后定向、受影响、全量、Python、compileall、Release 和差异验证。无需新增接口或依赖。
