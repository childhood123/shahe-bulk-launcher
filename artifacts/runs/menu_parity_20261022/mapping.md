# Python 菜单结构对照修正

用户明确要求菜单文字、顺序、层级遵循 Python。基线 shahe_bulk_launcher/features/hotkeys.py:create_menu_bar，对应 C# MainForm.CreateMenuBar。

本轮恢复用户管理（置顶之后）、校正（修复 Control 之前）、关于（检查更新之后）三条分隔线；设置菜单仅保留“热键设置、朗读设置、软件设置”。保留既有动作处理器、路径列表上下文菜单和快捷键，不改任何配置值或 999 上限。

每个受影响分支使用生产 MainMenu 子项含分隔线的完整序列断言；另外保留路径上下文菜单八项动作、Ctrl+H、管理器类保存功能的既有回归。
