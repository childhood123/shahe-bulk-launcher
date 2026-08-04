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

- 2026-08-04：建立拆分前项目基线和原项目到模块的映射；尚未执行迁移或测试。
