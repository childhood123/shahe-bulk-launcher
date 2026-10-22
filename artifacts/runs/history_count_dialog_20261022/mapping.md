# 路径数量窗口精简

用户明确要求移除数量设置窗口的“已记忆的路径列表”及“隐藏路径”选项。该要求优先于此前逐项保留 Python 布局的通用约束。

目标：PathMemoryManagerDialog 仅显示路径记忆数量、范围提示、确定与取消；保留 0–999 范围。移除列表附属的备注/删除/清空处理，但主窗口路径列表及其对应服务与上下文操作不变。保存只修改数量，不清空传入历史或改写其余设置；取消不保存。

调用：MainForm.ShowPathMemoryManagerDialog → PathMemoryManagerDialog → OnPathMemorySaved。原有 AdjustAppPathHistoryCount → PromptAppPathHistoryCount 已是独立数值框，本轮同时回归其数量范围。

另做只读重复入口核查，区分冗余窗口与菜单/快捷键的不同访问方式，不把检查范围当成删除所有类似功能的授权。

重复实现核查后，将可达的 PromptAppPathHistoryCount 改为复用精简后的 PathMemoryManagerDialog，并保持其原有“路径记忆数量”标题与“请输入路径记忆数量:”提示。数量设置实际入口因此也只使用这一个窗口实现。
