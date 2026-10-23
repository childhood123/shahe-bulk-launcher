# 功能列表回车执行：只读核对

用户意图待确认区域：快捷键设置窗口的功能列表。

C# HotkeySettingsDialog 的 ActionList.SelectedIndexChanged 只保存工作副本并加载快捷键；AcceptButton 指向确定，当前没有执行所选 action 的回调。MainForm.ShowHotkeySettingsDialog 仅在 DialogResult.OK 时保存热键。原版 settings_dialogs.py 同样仅提供选择、编辑、确认逻辑。

拟复用 MainForm.ExecuteApplicationAction 分派已有动作；执行前关闭快捷键设置窗口，避免模态窗口阻塞。仅列表焦点且无修饰 Enter 触发；录入框保持原语义。尚未修改生产源码、测试或活动计划的功能范围。

完整拟追加计划：proposed-plan.diff。等待用户确认后实施。

2026-10-23 收尾复核：上文为授权前的历史方案，后续用户“要所有都这样”已确认全部功能行。现实现通过 RequestedAction 退出并释放模态，再用既有 SafeCallAfter / DispatchApplicationAction 执行；OK 保存分支和全部业务检查保留，焦点移动不执行，录入框 Enter 继续捕获。逐项核对新增契约及现有原始结果，未发现阻断问题。完整工作树门禁包含先前未提交 AUDIT-04 测试；本功能提交不包含该测试。真实场景最终已通过，前置无效探针保留，生产代码在通过后没有新增变化。
