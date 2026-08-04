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

- 2026-08-04：Task 9 更新模块汇总结构回归 `python -m unittest tests.test_foundation_modules tests.test_move_foundation_entry_tool tests.test_ui_controls_contract tests.test_ui_dialogs_contract tests.test_backup_contract tests.test_feature_method_partition tests.test_app_paths_and_sandboxie tests.test_process_actions_contract tests.test_updates_and_speech_contract -v` 通过 14/14。
- 2026-08-04：Task 9 更新行为探针 GREEN：修正工作区探针导入顺序后，`update_package_selection_probe.py` 与 `update_helper_command_probe.py` 均通过。验证确认仅选择匹配当前版本且包含 `url` 的增量补丁，非法大小继续归零；更新助手复制到应用目录 `tenp/updater_helper_*`，同步复制 `internal`，请求文件仍写入 `tenp`，主程序参数继续指向兼容入口。
- 2026-08-04：Task 9 更新行为探针首轮：`update_package_selection_probe.py` 通过；`update_helper_command_probe.py` 在业务逻辑执行前因 `ModuleNotFoundError: No module named 'shahe_bulk_launcher'` 失败。根因是工作区探针先于仓库入口直接导入 `features.updates`，脚本从 `tests/probes` 启动时仓库根尚未加入 `sys.path`；该问题属于探针适配，需先加载兼容入口再引用模块全局。
- 2026-08-04：Task 9 更新模块结构 GREEN：`python -m py_compile "沙河批量打开器.py" shahe_bulk_launcher\features\updates.py artifacts\tools\split_launcher_mixins.py tests\test_updates_and_speech_contract.py` 通过；`python -m unittest tests.test_feature_method_partition tests.test_updates_and_speech_contract -v` 5/5 通过。20 个更新、更新器和打赏二维码方法唯一迁入 `UpdatesMixin`，`qr_embedded` 可选导入和 `QR_EMBEDDED_IMAGES` 回退缓存整体迁移，旧入口不再持有副本。
- 2026-08-04：Task 9 更新模块 RED 验证：`python -m unittest tests.test_updates_and_speech_contract -v` 产生 2 个预期失败，分别对应 `features/updates.py` 尚不存在，以及旧入口仍加载并持有 `qr_embedded.QR_EMBEDDED_IMAGES`。冻结清单确认本阶段先迁移 20 个更新、更新器和打赏二维码相关方法，二维码可选资源缓存必须随模块迁移。
- 2026-08-04：Task 8 汇总结构回归 `python -m unittest tests.test_foundation_modules tests.test_move_foundation_entry_tool tests.test_ui_controls_contract tests.test_ui_dialogs_contract tests.test_backup_contract tests.test_feature_method_partition tests.test_app_paths_and_sandboxie tests.test_process_actions_contract -v` 通过 12/12。
- 2026-08-04：Task 8 进程操作行为探针 GREEN：迁入工作区的 `audio_process_actions_no_blocking_probe.py`、`close_process_no_blocking_probe.py`、`locate_no_blocking_probe.py` 均通过。验证确认静音与播放设备切换、关闭当前进程仍在后台线程执行，相关任务锁能释放；定位当前进程调用在 0.5 秒阈值内返回，未因模块拆分引入 UI 主线程阻塞。
- 2026-08-04：Task 8 进程操作模块结构 GREEN：`python -m py_compile "沙河批量打开器.py" shahe_bulk_launcher\features\process_actions.py artifacts\tools\split_launcher_mixins.py tests\test_process_actions_contract.py` 通过；`python -m unittest tests.test_feature_method_partition tests.test_process_actions_contract -v` 5/5 通过。63 个进程操作方法唯一迁入 `ProcessActionsMixin`，`pythoncom`、`AudioUtilities`、`AudioRouter` 三项可选后端缓存随模块迁移，旧入口不再持有重复状态。
- 2026-08-04：Task 8 进程操作模块 RED 验证：扩展入口缓存所有权契约后，`python -m unittest tests.test_process_actions_contract -v` 产生 2 个预期失败：`shahe_bulk_launcher/features/process_actions.py` 尚不存在，且旧入口仍定义 `pythoncom`、`AudioUtilities`、`AudioRouter`。冻结清单确认本阶段迁移 63 个定位、关闭、静音、音量和播放设备相关方法；三项可选依赖缓存必须随方法迁入新模块，入口不再保留副本。
- 2026-08-04：Task 7 汇总结构回归 `python -m unittest tests.test_foundation_modules tests.test_move_foundation_entry_tool tests.test_ui_controls_contract tests.test_ui_dialogs_contract tests.test_backup_contract tests.test_feature_method_partition tests.test_app_paths_and_sandboxie -v` 通过 10/10。
- 2026-08-04：Task 7 应用路径/Sandboxie 迁移 GREEN：`python -m py_compile "沙河批量打开器.py" shahe_bulk_launcher\features\app_paths.py shahe_bulk_launcher\features\sandboxie.py artifacts\tools\split_launcher_mixins.py tests\test_app_paths_and_sandboxie.py` 通过；`python -m unittest tests.test_feature_method_partition tests.test_app_paths_and_sandboxie -v` 4/4 通过，31 个应用路径方法和 43 个 Sandboxie 方法唯一分区；入口导入烟雾确认方法来自新模块。`paste_invalid_clipboard_probe.py` 通过；测试桩适配 mixin 后，`launch_start_detached_probe.py` 与 `launch_detached_immediate_failure_probe.py` 均通过，正常派发仅调用非阻塞 `Popen`，立即非零退出仍报告失败并刷新无效沙盒路径。
- 2026-08-04：Task 7 首轮行为探针：`paste_invalid_clipboard_probe.py` 通过；两条启动探针在进入 Start.exe 派发前因测试 Dummy 只绑定 `start_app_in_sandbox` 而失败，错误为缺少同一 `SandboxieMixin` 中的 `_dispatch_sandboxie_start_detached`。该失败定位为旧单文件测试桩的 `SandboxLauncher.__dict__` 假设不再成立，业务实现尚未出现失败证据；探针需绑定完整 `SandboxieMixin` 方法集后重跑。
- 2026-08-04：Task 7 应用路径/Sandboxie RED 验证：`python -m unittest tests.test_app_paths_and_sandboxie -v` 产生 2 个预期失败，分别对应 `features/app_paths.py` 与 `features/sandboxie.py` 尚不存在；冻结清单确认目标方法数为 31 和 43。
- 2026-08-04：Task 6 汇总结构回归 `python -m unittest tests.test_foundation_modules tests.test_move_foundation_entry_tool tests.test_ui_controls_contract tests.test_ui_dialogs_contract tests.test_backup_contract tests.test_feature_method_partition -v` 通过 9/9。
- 2026-08-04：Task 6 生命周期/设置迁移 GREEN：`python -m py_compile "沙河批量打开器.py" shahe_bulk_launcher\features\__init__.py shahe_bulk_launcher\features\lifecycle.py shahe_bulk_launcher\features\settings.py artifacts\tools\split_launcher_mixins.py tests\test_feature_method_partition.py` 通过；`python -m unittest tests.test_feature_method_partition -v` 3/3 通过，34 个生命周期方法和 12 个设置方法唯一分区且原类已接入两个 mixin；入口导入烟雾确认方法来自新模块。`defaults_and_first_scan_probe.py` 与 `configuration_write_only_probe.py` 均通过，默认值、首次扫描状态/弹框和 Configuration 写路径保持不变。开机自启动路径已改用 `get_current_executable_path()`，避免登记移动后的 feature 文件。
- 2026-08-04：Task 6 主窗口分区 RED 验证：`python -m unittest tests.test_feature_method_partition.FeatureMethodPartitionTests.test_lifecycle_and_settings_modules_exist -v` 1 项测试因 `features/lifecycle.py` 与 `features/settings.py` 尚不存在按预期失败。冻结清单确认本阶段分别迁移 34 个生命周期方法和 12 个设置方法，`SandboxLauncher.__init__` 继续暂留原类。
- 2026-08-04：Task 5 汇总结构回归 `python -m unittest tests.test_foundation_modules tests.test_move_foundation_entry_tool tests.test_ui_controls_contract tests.test_ui_dialogs_contract tests.test_backup_contract -v` 通过 6/6。
- 2026-08-04：Task 5 备份恢复迁移 GREEN：`python -m py_compile "沙河批量打开器.py" shahe_bulk_launcher\backup.py artifacts\tools\move_backup.py tests\test_backup_contract.py` 通过；`python -m unittest tests.test_backup_contract -v` 1/1 通过；`restore_safe_extract_probe.py`、`restore_shell_free_probe.py`、`restore_no_main_thread_stop_probe.py` 均通过。验证确认恢复仍调用安全解压边界、子进程调用不启用 shell、停止 Sandboxie 服务不在 UI 主线程执行。源码重启路径已从移动后的 `__file__` 改为 `get_current_executable_path()`，保持启动原入口的行为。原项目未定义 `safe_extract_zip` 的风险仍未修复，探针按原设计显式注入该依赖。
- 2026-08-04：Task 5 备份恢复 RED 验证：`python -m unittest tests.test_backup_contract -v` 1 项测试因 `shahe_bulk_launcher/backup.py` 尚不存在按预期失败。静态审计同时确认原正式源码和仓库基线的 `BackupRestoreMgr.restore` 调用 `safe_extract_zip`，但入口及公共模块均未定义或导入该名称；该原项目风险不混入本次结构迁移，另行记录。另确认 `_ask_restart_app` 使用 `__file__`，移动类后必须改用已保持入口语义的 `get_current_executable_path()`，否则会重启 `backup.py`。
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
