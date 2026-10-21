# GAP-10 映射
- Python on_browse_app -> MainForm.SelectApplicationPath / AddApplicationPath：标题、exe 唯一过滤、当前目录、成功状态与焦点。
- Python on_paste_file -> MainForm.PasteApplicationPath：文件优先沿用；访问失败和数据异常分别保留原文案。
- Python on_copy_path -> MainForm.CopyCurrentAppPath：写入后最多五次精确回读；20ms 重试；写入访问失败与复制失败分开。
- Ctrl+O/Ctrl+V、焦点回调、AccessibleName/TabIndex 与最大历史 999 保持现有接线。测试只用内存 reader/writer。
