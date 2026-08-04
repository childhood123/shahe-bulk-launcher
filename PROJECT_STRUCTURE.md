# 沙河批量打开器项目结构

## 项目用途

本仓库用于维护沙河批量打开器 Python 主程序。当前任务是在不改变界面、文案、快捷键、焦点、无障碍、配置键和运行行为的前提下，将单文件实现拆分为独立模块。

## 当前基线

- 正式参考源码：`D:\编程\沙河批量打开器.py`
- 仓库入口：`D:\mycodex\git\沙河批量打开器.py`
- 两个文件在拆分开始前 SHA-256 相同：`E640619384AACB0B70CB58BCFAE17C0D4A4D7AB45CC6B8CF7CCD0F6E56215233`
- 基线规模：12,071 行，530,664 字节。
- 核心主窗口：`SandboxLauncher`，包含 400 个方法，约占 8,400 行。
- 原代码已有外部公共模块依赖：`shahe_bulk_launcher.common`，但拆分前仓库未包含该包。

## 保留入口

- `沙河批量打开器.py` 保留原文件名和直接启动方式。
- 入口继续导出原顶层常量、函数、控件、对话框、`BackupRestoreMgr`、`SandboxLauncher`、`SingleInstance` 和 `main`，兼容现有导入代码及回归探针。

## 目标结构

```text
沙河批量打开器.py
shahe_bulk_launcher/
    __init__.py
    app.py
    bootstrap.py
    common.py
    constants.py
    backup.py
    single_instance.py
    ui/
        __init__.py
        controls.py
        common_dialogs.py
        settings_dialogs.py
        tray.py
    features/
        __init__.py
        lifecycle.py
        settings.py
        app_paths.py
        sandboxie.py
        process_actions.py
        updates.py
        speech.py
        hotkeys.py
        sandbox_admin.py
        scheduling.py
tests/
artifacts/
```

## 模块映射

| 原代码职责 | 目标模块 |
| --- | --- |
| 打包运行时、资源路径、日志 | `shahe_bulk_launcher/bootstrap.py` |
| 常量、默认快捷键、名称校验 | `shahe_bulk_launcher/constants.py` |
| 配置、版本和更新公共工具 | `shahe_bulk_launcher/common.py` |
| 列表控件、状态控件、延迟调用 | `shahe_bulk_launcher/ui/controls.py` |
| 二维码、用户选择、命名对话框 | `shahe_bulk_launcher/ui/common_dialogs.py` |
| 设置、快捷键、语音和路径记忆对话框 | `shahe_bulk_launcher/ui/settings_dialogs.py` |
| 托盘 | `shahe_bulk_launcher/ui/tray.py` |
| 备份恢复 | `shahe_bulk_launcher/backup.py` |
| 主窗口初始化、生命周期和 UI 调度 | `shahe_bulk_launcher/features/lifecycle.py` |
| 配置读写和路径校正 | `shahe_bulk_launcher/features/settings.py` |
| 应用路径、历史、剪贴板 | `shahe_bulk_launcher/features/app_paths.py` |
| Sandboxie 配置、名称、PID、启动接口 | `shahe_bulk_launcher/features/sandboxie.py` |
| 定位、关闭、静音、音量、播放设备 | `shahe_bulk_launcher/features/process_actions.py` |
| 更新检查和更新器启动 | `shahe_bulk_launcher/features/updates.py` |
| Tolk、读屏和语音辅助 | `shahe_bulk_launcher/features/speech.py` |
| 菜单、快捷键和列表导航 | `shahe_bulk_launcher/features/hotkeys.py` |
| 分组、批量管理、INI、别名 | `shahe_bulk_launcher/features/sandbox_admin.py` |
| 定时任务和批量启动 | `shahe_bulk_launcher/features/scheduling.py` |
| 主窗口组装和程序入口 | `shahe_bulk_launcher/app.py` |
| 单实例控制 | `shahe_bulk_launcher/single_instance.py` |

## 构建与测试

- 语法验证：`python -m compileall -q .`
- 单元与结构回归：`python -m unittest discover -s tests -v`
- 入口导入烟雾测试：由 `tests` 中的兼容测试覆盖。
- UI 和关键业务回归：从 `D:\编程\MyCodex\shahe_launcher_test` 迁入与本次模块拆分有关的探针，并改为引用仓库入口。
- 临时输出、日志和截图统一放入 `artifacts/` 的用途子目录。

## 无障碍约束

- 不修改现有控件名称、焦点顺序、键盘操作和状态反馈。
- 焦点移动与选择变化不得新增高成本或破坏性操作。
- 启动、关闭、更新和批量管理继续由明确按钮、菜单或快捷键触发。

## 验证记录

- 2026-08-04：Task 4 汇总回归 `python -m unittest tests.test_foundation_modules tests.test_move_foundation_entry_tool tests.test_ui_controls_contract tests.test_ui_dialogs_contract -v` 通过 5/5，基础模块、生成器、控件、对话框和托盘结构契约同时有效。
- 2026-08-04：Task 4 对话框/托盘迁移 GREEN：`python -m py_compile "沙河批量打开器.py" shahe_bulk_launcher\ui\common_dialogs.py shahe_bulk_launcher\ui\settings_dialogs.py shahe_bulk_launcher\ui\tray.py artifacts\tools\move_ui_dialogs.py tests\test_ui_dialogs_contract.py` 通过；`python -m unittest tests.test_ui_dialogs_contract -v` 2/2 通过；入口导入烟雾确认 `SettingsDialog`、`QRCodeDialog`、`TaskBarIcon` 均来自新模块；`dialog_control_names_probe.py` 与 `dialog_control_names_more_probe.py` 均通过，关键设置和朗读控件 AccessibleName 保持不变。
- 2026-08-04：Task 4 对话框/托盘 RED 验证：`python -m unittest tests.test_ui_dialogs_contract -v` 最终稳定为 4 个预期失败，分别对应 `common_dialogs.py`、`settings_dialogs.py`、`tray.py` 及二维码辅助函数目标文件尚不存在；测试不再产生额外文件读取错误。
- 2026-08-04：Task 3 汇总回归 `python -m unittest tests.test_foundation_modules tests.test_move_foundation_entry_tool tests.test_ui_controls_contract -v` 通过 3/3，确认基础模块、迁移工具模板和五个控件类方法契约同时保持有效。
- 2026-08-04：Task 3 相邻包导入兼容修复后，`python tests\probes\sandbox_list_navigation_no_status_probe.py` 和 `python tests\probes\sandbox_list_navigation_coalesces_live_state_probe.py` 均通过；`python -m py_compile "沙河批量打开器.py" shahe_bulk_launcher\ui\controls.py` 通过。验证确认列表焦点移动不触发勾选计数状态更新，连续浏览的 live-state 与菜单刷新仍保持合并调度。
- 2026-08-04：Task 3 首次运行两个工作区列表探针均在导入入口时失败，错误为 `ModuleNotFoundError: No module named 'shahe_bulk_launcher'`。根因确认是探针从 `tests/probes` 作为脚本启动时仓库根目录不在 `sys.path`，而旧入口路径引导只识别 `SCRIPT_DIR/MyCodex` 布局，未识别现在与入口相邻的 `shahe_bulk_launcher` 包；控件逻辑尚未执行。该失败作为相邻包导入兼容回归证据记录。
- 2026-08-04：Task 3 控件模块 RED 验证：运行 `python -m unittest tests.test_ui_controls_contract -v`，测试因 `ModuleNotFoundError: No module named 'shahe_bulk_launcher.ui'` 按预期失败。测试随后改为直接比较目标模块 AST 与基线类方法清单，以覆盖 `classmethod` 等描述符且避免把 wx 导入行为混入结构契约。
- 2026-08-04：Task 2 基础模块完成。新增 `shahe_bulk_launcher/common.py`、`constants.py`、`bootstrap.py`、`single_instance.py` 和包入口，旧脚本改为显式导入同名 API。修复仅存在于迁移工具模板的补丁标记问题后，`python -m unittest tests.test_move_foundation_entry_tool -v` 1/1 通过；`python -m py_compile "沙河批量打开器.py" shahe_bulk_launcher\__init__.py shahe_bulk_launcher\common.py shahe_bulk_launcher\constants.py shahe_bulk_launcher\bootstrap.py shahe_bulk_launcher\single_instance.py tests\test_foundation_modules.py` 通过；`python -m unittest tests.test_foundation_modules -v` 1/1 通过；非 UI 入口导入烟雾检查确认 `APP_VERSION=4.0.46`、`get_app_dir()` 仍为 `D:\mycodex\git`，且入口导出的 `SingleInstance` 来自新模块。
- 2026-08-04：Task 2 迁移工具回归 RED：首次运行入口语法验证在第 114 行失败，调查确认 `artifacts/tools/move_foundation_entry.py` 的 `IMPORT_BLOCK` 模板把补丁标记 `+` 作为普通字符写入 28 行导入文本。新增 `tests/test_move_foundation_entry_tool.py` 后运行 1 项测试按预期失败，精确列出这 28 行，后续修复生成器而非只修生成结果。
- 2026-08-04：Task 2 基础模块 RED 验证：运行 `python -m unittest tests.test_foundation_modules -v`，1 项测试因 `ModuleNotFoundError: No module named 'shahe_bulk_launcher'` 按预期失败，确认测试覆盖目标包而非旧单文件定义。
- 2026-08-04：完成拆分前 API 契约捕获，记录 19 个顶层函数、17 个顶层类、11 个全大写常量和 `SandboxLauncher` 400 个方法；方法目标模块清单合计同为 400，无遗漏。运行 `python -m py_compile artifacts\tools\capture_launcher_contract.py tests\test_modular_structure.py` 通过。运行 `python -m unittest tests.test_modular_structure.ModularStructureTests.test_target_modules_exist -v` 按预期失败，唯一原因是设计中的 20 个目标模块尚未创建，作为后续迁移的 RED 基线。
- 2026-08-04：建立拆分前项目基线和原项目到模块的映射；尚未执行迁移或测试。
