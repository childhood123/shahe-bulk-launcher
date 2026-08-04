# 沙河批量打开器 Python 模块化设计

## 状态

用户已确认采用“按业务域拆分 mixin，保留原入口和全部行为”的方案。

## 目标

将 `沙河批量打开器.py` 的 12,071 行单文件实现拆入职责清晰的 Python 包，同时满足以下条件：

- 保留 `沙河批量打开器.py` 的文件名和直接启动方式。
- 不改变 UI、文案、快捷键、菜单 ID、焦点顺序、读屏名称和状态反馈。
- 不改变配置路径、配置键、默认值、更新流程和发布运行时判断。
- 不借拆分重写业务逻辑或修复原程序行为问题。
- 迁移一个模块后立即验证并更新 `PROJECT_STRUCTURE.md`，再迁移下一个模块。

## 非目标

- 不重写为 MVC、MVVM 或依赖注入架构。
- 不把现有方法改造成新的服务 API。
- 不新增第三方依赖。
- 不同步修改 `D:\编程` 下的正式源码。
- 不进行无关格式化、命名调整或界面重新设计。

## 方案选择

### 采用：业务域 mixin

`SandboxLauncher` 继续是唯一的 `wx.Frame` 实例。原有方法按业务职责移动到 mixin 类，所有运行状态仍保存在同一个 `self` 上。跨模块调用继续使用原有方法名，因此无需重写 400 个方法之间的调用关系。

### 未采用：只拆顶层类

该方案风险低，但会留下约 8,400 行的 `SandboxLauncher`，不能解决主要臃肿点。

### 未采用：服务对象组合

该方案长期边界更强，但必须重新连接状态、线程、wx 回调和错误反馈，超出本次纯结构迁移范围。

## 目标架构

```text
沙河批量打开器.py                 # 兼容入口和打包前置引导
shahe_bulk_launcher/
    __init__.py
    app.py                       # SandboxLauncher 组装和 main()
    bootstrap.py                 # 运行目录、资源路径、日志、发布清理
    common.py                    # 配置、版本、更新公共工具
    constants.py                 # 常量和纯名称校验
    backup.py                    # BackupRestoreMgr
    single_instance.py           # SingleInstance
    ui/
        controls.py
        common_dialogs.py
        settings_dialogs.py
        tray.py
    features/
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
```

## 依赖方向

1. 兼容入口先完成发布运行时路径引导，再导入包。
2. `constants` 和 `common` 不导入 UI 或 `app`。
3. `ui` 可以依赖 `wx`、`constants` 和 `common`，但不能导入 `app`。
4. feature mixin 可以依赖低层模块和 UI 类型，但不能导入 `SandboxLauncher`。
5. `app.py` 位于依赖顶部，导入所有 mixin 并组装 `SandboxLauncher`。
6. mixin 不定义独立应用状态，也不使用协作式 `super()` 链；`SandboxLauncher.__init__` 仍只有一个明确实现。

## 兼容入口

入口脚本不只调用 `main()`，还会显式重新导出原脚本的顶层 API，包括：

- 版本、更新地址、日志模式和固定快捷键常量。
- 路径、校验、日志和运行时辅助函数。
- 自定义列表控件、状态控件和全部对话框。
- `TaskBarIcon`、`BackupRestoreMgr`、`SandboxLauncher`、`SingleInstance` 和 `main`。

这样原有 `importlib` 探针和外部代码仍可通过入口模块访问原名称。不会使用动态 `globals()` 注入或运行时猴子补丁。

## 状态与数据流

- 配置仍由 `SandboxLauncher` 持有，配置读取和保存顺序不变。
- 列表控件仍通过父窗口方法查询状态和触发明确动作。
- 后台线程和 `wx.CallAfter`/延迟调用边界保持原样。
- 定时任务、启动会话、进程状态缓存、语音运行时和托盘对象仍挂在原主窗口实例。
- feature mixin 之间只通过已有的实例属性和实例方法协作。

## 错误处理

- 原来写状态栏的错误继续写相同状态文本。
- 原来弹出消息框的错误继续使用相同标题、正文和图标。
- 原来容忍清理失败的 `try/except` 不在本次迁移中收紧。
- 导入错误只在结构测试中暴露，不增加会掩盖缺失模块的新兜底逻辑。
- 如果迁移中发现真实业务 bug，只记录复现证据和风险，不混入本次结构改动。

## 迁移顺序

每个阶段都遵循：先添加会失败的结构/兼容测试，确认失败原因是目标模块尚不存在；移动最少代码；运行定向测试和语法验证；更新 `PROJECT_STRUCTURE.md`。

1. 建立测试目录、兼容 API 基线和模块成员清单。
2. 迁移 `common`、`constants`、`bootstrap` 和 `single_instance`。
3. 迁移 `ui.controls`。
4. 迁移普通对话框、设置类对话框和托盘。
5. 迁移 `BackupRestoreMgr`。
6. 迁移生命周期和设置 mixin。
7. 迁移应用路径和 Sandboxie 基础能力 mixin。
8. 迁移进程操作 mixin。
9. 迁移更新和语音 mixin。
10. 迁移快捷键、沙盒管理和调度启动 mixin。
11. 在 `app.py` 组装最终类，将入口缩减为兼容外观。
12. 运行全量结构、语法、回归探针和启动烟雾验证。

## 测试策略

### 结构测试

使用标准库 `unittest` 和 `ast`，不新增测试框架。测试覆盖：

- 目标模块存在且可编译。
- 每个原顶层类和函数只在一个实现模块中定义。
- `SandboxLauncher` 的原 400 个方法在最终组装类上仍全部可见。
- 入口继续导出基线 API。
- feature 模块不导入 `app.py`，防止循环依赖。

### 行为回归

从现有 `D:\编程\MyCodex\shahe_launcher_test` 选择与模块边界有关的探针，复制到仓库 `tests/probes/` 并将目标路径改为仓库入口。优先覆盖：

- 控件名称、焦点和列表浏览不触发高成本操作。
- 设置默认值、配置读写和固定快捷键。
- 批量启动非阻塞与启动完成 UI 边界。
- 进程关闭、定位和音频操作的后台线程边界。
- 更新包选择和更新器启动命令。
- 备份还原安全解压和非阻塞恢复。

### 每阶段验证

```powershell
python -m py_compile "沙河批量打开器.py" <本阶段模块>
python -m unittest <本阶段定向测试> -v
```

### 最终验证

```powershell
python -m compileall -q .
python -m unittest discover -s tests -v
```

如本机 wxPython 和 Sandboxie 环境可用，再执行入口导入、主窗口创建和关键真实操作烟雾验证。无法自动验证的发布打包行为必须在 `PROJECT_STRUCTURE.md` 中明确记录剩余风险，不能用语法通过代替发布验证。

## 完成标准

- 入口名称和启动命令不变。
- 单文件只保留引导、显式兼容导出和 `main()` 调用。
- 目标包没有循环导入。
- 原顶层 API 和 `SandboxLauncher` 方法集合保持兼容。
- 全部新增结构测试、迁入的关联探针和语法验证通过。
- 每个模块的迁移与验证结果已即时写入 `PROJECT_STRUCTURE.md`。
- 未确认行为差异均记录为风险，没有混入业务修复。
