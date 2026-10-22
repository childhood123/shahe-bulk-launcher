# REVIEW-05 当前用户操作未选中提示映射

Python 所有当前用户动作在 `features/hotkeys.py` 通过同一入口，未选中时文案为「请先在用户列表中选中一个用户」。C# `MainForm.ExecuteCurrentSandboxAction` 目前共用入口使用旧短文案，影响静音、音量、播放设备、关闭、定位、置顶六类动作。
只改这一处字符串；测试通过真实 `ExecuteApplicationAction` 菜单分派逐项确认回调不执行且文案完全一致。
