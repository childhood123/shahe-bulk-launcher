# 沙河批量打开器 Python 模块化实施计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 将 12,071 行的 `沙河批量打开器.py` 拆成按业务域组织的 Python 包，同时保持原入口、原顶层 API 和全部用户可见行为。

**Architecture:** 保留一个 `SandboxLauncher` 实例和一份 `self` 状态，使用职责互斥的 mixin 承载原 400 个方法；控件、对话框、备份和单实例类直接移动到独立模块。入口脚本负责发布运行时前置引导、显式兼容导出和调用包内 `main()`。

**Tech Stack:** Python 3.14、wxPython、标准库 `ast`/`unittest`、现有 Sandboxie/音频/Tolk/更新依赖；不新增第三方包。

## Global Constraints

- 保留入口命令：`python "D:\mycodex\git\沙河批量打开器.py"`。
- 不改变 UI、文案、菜单 ID、快捷键、焦点顺序、AccessibleName、配置键、默认值和发布运行时判断。
- 不修复或重写原有业务行为；发现真实 bug 只记录风险。
- 每迁移一个模块，立即运行定向验证并更新 `PROJECT_STRUCTURE.md`。
- 临时脚本和生成数据放入 `artifacts/tools/`；回归代码放入 `tests/`。
- 保持用户已有的 `.superpowers/`、`AGENTS.md` 和无关改动不变。

---

### Task 1: 建立 API 基线和失败的结构测试

**Files:**
- Create: `artifacts/tools/capture_launcher_contract.py`
- Create: `tests/contracts/launcher_api.json`
- Create: `tests/contracts/method_modules.json`
- Create: `tests/test_modular_structure.py`
- Modify: `PROJECT_STRUCTURE.md`

**Interfaces:**
- Consumes: 拆分前入口文件 AST。
- Produces: 原顶层函数、类、常量以及 `SandboxLauncher` 400 个方法的不可变 JSON 契约；`method_modules.json` 固化每个方法的目标 mixin，后续不再依赖会变化的行号。

- [ ] **Step 1: 编写契约捕获工具**

```python
import ast
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
source = ROOT / "沙河批量打开器.py"
output = ROOT / "tests" / "contracts" / "launcher_api.json"
tree = ast.parse(source.read_text(encoding="utf-8-sig"))
functions = []
classes = {}
constants = []
for node in tree.body:
    if isinstance(node, (ast.FunctionDef, ast.AsyncFunctionDef)):
        functions.append(node.name)
    elif isinstance(node, ast.ClassDef):
        classes[node.name] = [
            item.name for item in node.body
            if isinstance(item, (ast.FunctionDef, ast.AsyncFunctionDef))
        ]
    elif isinstance(node, (ast.Assign, ast.AnnAssign)):
        targets = node.targets if isinstance(node, ast.Assign) else [node.target]
        constants.extend(
            target.id for target in targets
            if isinstance(target, ast.Name) and target.id.isupper()
        )
output.parent.mkdir(parents=True, exist_ok=True)
output.write_text(
    json.dumps(
        {"functions": functions, "classes": classes, "constants": constants},
        ensure_ascii=False,
        indent=2,
    ) + "\n",
    encoding="utf-8",
)
```

- [ ] **Step 2: 生成并检查契约**

Run: `python artifacts/tools/capture_launcher_contract.py`

Expected: JSON 中 `classes.SandboxLauncher` 长度为 400。

- [ ] **Step 3: 编写目标结构测试**

测试包含三项明确断言：设计文档中的 20 个目标 `.py` 文件全部存在；feature 文件不导入 `shahe_bulk_launcher.app`；最终入口导出契约中的全部名称。目标模块使用代码中的固定元组，不使用模糊 glob 代替清单。

- [ ] **Step 4: 运行并确认 RED**

Run: `python -m unittest tests.test_modular_structure.ModularStructureTests.test_target_modules_exist -v`

Expected: FAIL，明确列出尚未创建的 `shahe_bulk_launcher` 模块。

- [ ] **Step 5: 记录并提交基线**

```powershell
git add artifacts/tools/capture_launcher_contract.py tests/contracts/launcher_api.json tests/contracts/method_modules.json tests/test_modular_structure.py PROJECT_STRUCTURE.md
git commit -m "test: capture launcher compatibility contract"
```

### Task 2: 迁移基础模块

**Files:**
- Create: `shahe_bulk_launcher/__init__.py`
- Create: `shahe_bulk_launcher/common.py`
- Create: `shahe_bulk_launcher/constants.py`
- Create: `shahe_bulk_launcher/bootstrap.py`
- Create: `shahe_bulk_launcher/single_instance.py`
- Create: `tests/test_foundation_modules.py`
- Modify: `沙河批量打开器.py`
- Modify: `PROJECT_STRUCTURE.md`

**Interfaces:**
- Produces: 原常量、名称校验、运行目录/日志函数、公共配置工具和 `SingleInstance`。
- Consumes: `D:\编程\MyCodex\shahe_bulk_launcher\common.py` 的现有公共实现。

- [ ] **Step 1: 先写失败测试**

```python
import unittest


class FoundationModuleTests(unittest.TestCase):
    def test_foundation_api(self):
        from shahe_bulk_launcher import bootstrap, constants, single_instance
        self.assertEqual("4.0.46", constants.APP_VERSION)
        self.assertEqual("off", constants.LOG_OUTPUT_OFF)
        self.assertTrue(callable(bootstrap.get_app_dir))
        self.assertTrue(hasattr(single_instance, "SingleInstance"))
```

- [ ] **Step 2: 确认 RED**

Run: `python -m unittest tests.test_foundation_modules -v`

Expected: FAIL with missing modules or attributes.

- [ ] **Step 3: 移动最少实现**

- `constants.py`: 原 113-203 行常量和纯名称校验。
- `bootstrap.py`: 内部目录引导后的运行目录、资源、发布残留清理和每日日志。
- `single_instance.py`: 原 `SingleInstance` 类。
- `common.py`: 复制现有公共模块并仅删除重复 `import base64`。
- 原入口从新模块导入同名符号并删除已迁移定义。

- [ ] **Step 4: 验证 GREEN**

```powershell
python -m py_compile "沙河批量打开器.py" shahe_bulk_launcher\common.py shahe_bulk_launcher\constants.py shahe_bulk_launcher\bootstrap.py shahe_bulk_launcher\single_instance.py
python -m unittest tests.test_foundation_modules -v
```

- [ ] **Step 5: 更新文档并提交**

```powershell
git add "沙河批量打开器.py" shahe_bulk_launcher tests/test_foundation_modules.py PROJECT_STRUCTURE.md
git commit -m "refactor: extract launcher foundation modules"
```

### Task 3: 迁移列表和状态控件

**Files:**
- Create: `shahe_bulk_launcher/ui/__init__.py`
- Create: `shahe_bulk_launcher/ui/controls.py`
- Create: `tests/test_ui_controls_contract.py`
- Modify: `沙河批量打开器.py`
- Modify: `PROJECT_STRUCTURE.md`

**Interfaces:**
- Produces: `ManagedDelayedCall`、`OperationStatusText`、`SingleColumnListCtrl`、`AppPathListCtrl`、`CheckableListCtrl`。

- [ ] **Step 1: 写失败测试**

从契约 JSON 读取五个类的方法名，断言目标模块导出的类方法集合一致。

- [ ] **Step 2: 确认 RED**

Run: `python -m unittest tests.test_ui_controls_contract -v`

- [ ] **Step 3: 原样移动原 206-257、330-347、432-2031 行对应类**

只调整显式导入；事件绑定、AccessibleName、键盘处理和延迟调度逻辑保持不变。

- [ ] **Step 4: 验证 GREEN**

```powershell
python -m py_compile "沙河批量打开器.py" shahe_bulk_launcher\ui\controls.py
python -m unittest tests.test_ui_controls_contract -v
```

- [ ] **Step 5: 运行列表无障碍关联探针、更新文档并提交**

迁入并运行列表浏览不写状态、live-state 合并调度探针后提交：

```powershell
git add "沙河批量打开器.py" shahe_bulk_launcher/ui tests PROJECT_STRUCTURE.md
git commit -m "refactor: extract launcher list controls"
```

### Task 4: 迁移对话框和托盘

**Files:**
- Create: `shahe_bulk_launcher/ui/common_dialogs.py`
- Create: `shahe_bulk_launcher/ui/settings_dialogs.py`
- Create: `shahe_bulk_launcher/ui/tray.py`
- Create: `tests/test_ui_dialogs_contract.py`
- Modify: `沙河批量打开器.py`
- Modify: `PROJECT_STRUCTURE.md`

**Interfaces:**
- `common_dialogs`: 二维码、选择用户、分组名称、批量命名。
- `settings_dialogs`: 快捷键、普通设置、路径记忆、语音设置。
- `tray`: `TaskBarIcon`。

- [ ] **Step 1: 写失败类契约测试并确认 RED**

Run: `python -m unittest tests.test_ui_dialogs_contract -v`

- [ ] **Step 2: 移动原 350-430、2032-3165 行对应函数和类**

对话框继续通过 `parent` 调用主窗口，不导入 `app.py`；控件名称、按钮文本和绑定顺序不变。

- [ ] **Step 3: 验证 GREEN 和控件名称探针**

```powershell
python -m py_compile "沙河批量打开器.py" shahe_bulk_launcher\ui\common_dialogs.py shahe_bulk_launcher\ui\settings_dialogs.py shahe_bulk_launcher\ui\tray.py
python -m unittest tests.test_ui_dialogs_contract -v
```

- [ ] **Step 4: 更新文档并提交**

```powershell
git add "沙河批量打开器.py" shahe_bulk_launcher/ui tests PROJECT_STRUCTURE.md
git commit -m "refactor: extract launcher dialogs and tray"
```

### Task 5: 迁移备份恢复

**Files:**
- Create: `shahe_bulk_launcher/backup.py`
- Create: `tests/test_backup_contract.py`
- Modify: `沙河批量打开器.py`
- Modify: `PROJECT_STRUCTURE.md`

**Interfaces:**
- Produces: 原签名和原 13 个方法的 `BackupRestoreMgr`。

- [ ] **Step 1: 写失败测试并确认 RED**

Run: `python -m unittest tests.test_backup_contract -v`

- [ ] **Step 2: 移动原 3167-3551 行类定义**

保持停服务、恢复线程、确认和清理顺序不变。

- [ ] **Step 3: 验证 GREEN 和恢复安全探针**

```powershell
python -m py_compile "沙河批量打开器.py" shahe_bulk_launcher\backup.py
python -m unittest tests.test_backup_contract -v
```

迁入并运行安全解压、无 `shell=True`、停止服务不阻塞 UI 三个探针。

- [ ] **Step 4: 更新文档并提交**

```powershell
git add "沙河批量打开器.py" shahe_bulk_launcher/backup.py tests PROJECT_STRUCTURE.md
git commit -m "refactor: extract backup restore manager"
```

### Task 6: 建立机械提取工具并迁移生命周期与设置

**Files:**
- Create: `artifacts/tools/split_launcher_mixins.py`
- Create: `shahe_bulk_launcher/features/__init__.py`
- Create: `shahe_bulk_launcher/features/lifecycle.py`
- Create: `shahe_bulk_launcher/features/settings.py`
- Create: `tests/test_feature_method_partition.py`
- Modify: `沙河批量打开器.py`
- Modify: `PROJECT_STRUCTURE.md`

**Interfaces:**
- Produces: `LifecycleMixin`、`SettingsMixin`。
- Contract: 每个原 `SandboxLauncher` 方法在入口类或恰好一个 mixin 中定义。

- [ ] **Step 1: 写失败的分区测试**

测试解析入口和所有 feature 文件，比较契约方法集合并报告重复/遗漏：

```python
self.assertEqual(set(contract_methods), set(implemented_methods))
self.assertEqual([], duplicate_methods)
```

- [ ] **Step 2: 确认 RED**

Run: `python -m unittest tests.test_feature_method_partition -v`

- [ ] **Step 3: 编写机械提取工具**

使用 `ast` 的 `lineno/end_lineno` 和原始文本切片移动完整方法块，不用 `ast.unparse`。目标归属从 `tests/contracts/method_modules.json` 读取；该文件在拆分前根据原始行区间生成方法名并人工复核。写入前验证方法存在、只分配一次、目标路径位于仓库，未分配方法继续留在原类。

- [ ] **Step 4: 提取方法**

- `LifecycleMixin`: 原 3695-4124 的日志、启动、托盘生命周期、安全 UI 调度、状态和启动会话，以及 `init_ui`、`on_close`、`_prepare_window_destroy`、`Destroy`。
- `SettingsMixin`: 默认设置、清单地址、配置读写、设置窗口入口、重置、目录应用/校正和设置保存回调。
- `SandboxLauncher.__init__` 暂留入口类。

- [ ] **Step 5: 验证 GREEN、更新文档并提交**

```powershell
python -m py_compile "沙河批量打开器.py" shahe_bulk_launcher\features\lifecycle.py shahe_bulk_launcher\features\settings.py
python -m unittest tests.test_feature_method_partition -v
git add "沙河批量打开器.py" shahe_bulk_launcher/features artifacts/tools/split_launcher_mixins.py tests PROJECT_STRUCTURE.md
git commit -m "refactor: extract lifecycle and settings mixins"
```

### Task 7: 迁移应用路径和 Sandboxie 基础能力

**Files:**
- Create: `shahe_bulk_launcher/features/app_paths.py`
- Create: `shahe_bulk_launcher/features/sandboxie.py`
- Create: `tests/test_app_paths_and_sandboxie.py`
- Modify: `沙河批量打开器.py`
- Modify: `PROJECT_STRUCTURE.md`

**Interfaces:**
- `AppPathsMixin`: 路径历史、显示、选择/粘贴/浏览、备注和记忆管理。
- `SandboxieMixin`: 安装路径、配置、别名、名称、Start.exe/SbieIni/SbieDll、PID 和应用派发。

- [ ] **Step 1: 写失败归属测试并确认 RED**

Run: `python -m unittest tests.test_app_paths_and_sandboxie -v`

- [ ] **Step 2: 按固定归属清单移动方法**

`method_modules.json` 中 `app_paths` 接收原 4262-4658 以及 10965-11238 的路径记忆方法；`sandboxie` 接收原 5108-5942 和 11239-11412。`find_sandboxie_path` 与 `start_app_in_sandbox` 明确属于 `SandboxieMixin`。

- [ ] **Step 3: 验证 GREEN 和启动探针**

```powershell
python -m py_compile "沙河批量打开器.py" shahe_bulk_launcher\features\app_paths.py shahe_bulk_launcher\features\sandboxie.py
python -m unittest tests.test_feature_method_partition tests.test_app_paths_and_sandboxie -v
```

迁入并运行剪贴板无效路径、非阻塞启动和 Start.exe 立即失败探针。

- [ ] **Step 4: 更新文档并提交**

```powershell
git add "沙河批量打开器.py" shahe_bulk_launcher/features tests PROJECT_STRUCTURE.md
git commit -m "refactor: extract app path and Sandboxie mixins"
```

### Task 8: 迁移进程操作

**Files:**
- Create: `shahe_bulk_launcher/features/process_actions.py`
- Create: `tests/test_process_actions_contract.py`
- Modify: `沙河批量打开器.py`
- Modify: `PROJECT_STRUCTURE.md`

**Interfaces:**
- Produces: `ProcessActionsMixin`，包含音频后端、播放设备、窗口定位、静音/音量、进程终止、菜单状态和异步回调。

- [ ] **Step 1: 写失败测试并确认 RED**

Run: `python -m unittest tests.test_process_actions_contract -v`

- [ ] **Step 2: 移动原 5943-7620 行方法**

`pythoncom`、`AudioUtilities`、`AudioRouter` 缓存随模块移动；线程、锁、UI 回调和焦点移开顺序不变。

- [ ] **Step 3: 验证 GREEN 和非阻塞探针**

```powershell
python -m py_compile "沙河批量打开器.py" shahe_bulk_launcher\features\process_actions.py
python -m unittest tests.test_feature_method_partition tests.test_process_actions_contract -v
```

迁入并运行音频、关闭进程和定位三个非阻塞探针。

- [ ] **Step 4: 更新文档并提交**

```powershell
git add "沙河批量打开器.py" shahe_bulk_launcher/features/process_actions.py tests PROJECT_STRUCTURE.md
git commit -m "refactor: extract process actions mixin"
```

### Task 9: 迁移更新和语音

**Files:**
- Create: `shahe_bulk_launcher/features/updates.py`
- Create: `shahe_bulk_launcher/features/speech.py`
- Create: `tests/test_updates_and_speech_contract.py`
- Modify: `沙河批量打开器.py`
- Modify: `PROJECT_STRUCTURE.md`

**Interfaces:**
- `UpdatesMixin`: 二维码、更新清单、包选择、更新器命令、下载结果和捐赠入口。
- `SpeechMixin`: reader.ini、Tolk DLL、读屏匹配、运行时复制和朗读设置。

- [ ] **Step 1: 写失败测试并确认 RED**

Run: `python -m unittest tests.test_updates_and_speech_contract -v`

- [ ] **Step 2: 移动原 7875-8259 和 8321-8872 行方法**

可选 `qr_embedded` 留在更新模块；语音 DLL 缓存留在语音模块。URL、版本比较、错误文案和确认顺序不变。

- [ ] **Step 3: 验证 GREEN 和更新/语音探针**

```powershell
python -m py_compile "沙河批量打开器.py" shahe_bulk_launcher\features\updates.py shahe_bulk_launcher\features\speech.py
python -m unittest tests.test_feature_method_partition tests.test_updates_and_speech_contract -v
```

- [ ] **Step 4: 更新文档并提交**

```powershell
git add "沙河批量打开器.py" shahe_bulk_launcher/features tests PROJECT_STRUCTURE.md
git commit -m "refactor: extract updates and speech mixins"
```

### Task 10: 迁移快捷键、沙盒管理和调度启动

**Files:**
- Create: `shahe_bulk_launcher/features/hotkeys.py`
- Create: `shahe_bulk_launcher/features/sandbox_admin.py`
- Create: `shahe_bulk_launcher/features/scheduling.py`
- Create: `tests/test_remaining_features_contract.py`
- Modify: `沙河批量打开器.py`
- Modify: `PROJECT_STRUCTURE.md`

**Interfaces:**
- `HotkeysMixin`: 键盘事件、菜单、热键解析/注册、列表导航和进程菜单状态。
- `SandboxAdminMixin`: 分组、固定项、批量命名、INI、别名、目录和注册表清理。
- `SchedulingMixin`: 启动目标、定时任务、启动线程、完成回调和备份/恢复代理。

- [ ] **Step 1: 写失败测试并确认 RED**

Run: `python -m unittest tests.test_remaining_features_contract -v`

- [ ] **Step 2: 移动剩余方法**

- `HotkeysMixin`: `method_modules.json` 中由原 4697-5107、7643-7874、9035-9605 固化的方法名。
- `SandboxAdminMixin`: 清单中由原 9606-10964 固化的方法名。
- `SchedulingMixin`: 清单中由原 11413-11933 固化的方法名。

- [ ] **Step 3: 验证 400 个方法唯一分区并运行关键交互探针**

```powershell
python -m unittest tests.test_feature_method_partition tests.test_remaining_features_contract -v
```

迁入并运行固定快捷键焦点、UI 广扫、启动不阻塞和完成回调不重建菜单探针。

- [ ] **Step 4: 更新文档并提交**

```powershell
git add "沙河批量打开器.py" shahe_bulk_launcher/features tests PROJECT_STRUCTURE.md
git commit -m "refactor: extract remaining launcher feature mixins"
```

### Task 11: 组装最终应用并缩减兼容入口

**Files:**
- Create: `shahe_bulk_launcher/app.py`
- Create: `tests/test_entry_compatibility.py`
- Modify: `shahe_bulk_launcher/__init__.py`
- Modify: `沙河批量打开器.py`
- Modify: `PROJECT_STRUCTURE.md`

**Interfaces:**
- Produces: 最终 `SandboxLauncher`、`main()` 和显式兼容导出。

- [ ] **Step 1: 写失败的入口兼容测试**

通过 `importlib.util.spec_from_file_location` 加载中文入口，逐项断言契约中的函数、类、常量和 400 个主窗口方法可见。

- [ ] **Step 2: 确认 RED**

Run: `python -m unittest tests.test_entry_compatibility -v`

- [ ] **Step 3: 创建最终类**

提取工具将入口中剩余的 `SandboxLauncher.__init__` 完整文本移动到 `app.py`，并把类基类替换为以下确定顺序：`LifecycleMixin, SettingsMixin, AppPathsMixin, SandboxieMixin, ProcessActionsMixin, UpdatesMixin, SpeechMixin, HotkeysMixin, SandboxAdminMixin, SchedulingMixin, wx.Frame`。所有 mixin 的 AST 必须不存在名为 `__init__` 的方法，分区测试对此做显式断言。

- [ ] **Step 4: 将入口改为薄兼容外观**

保留最早执行的内部目录引导，显式导入原顶层 API，并保留：

```python
if __name__ == "__main__":
    main()
```

- [ ] **Step 5: 验证 GREEN、更新文档并提交**

```powershell
python -m py_compile "沙河批量打开器.py" shahe_bulk_launcher\app.py
python -m unittest tests.test_entry_compatibility tests.test_modular_structure tests.test_feature_method_partition -v
git add "沙河批量打开器.py" shahe_bulk_launcher tests/test_entry_compatibility.py PROJECT_STRUCTURE.md
git commit -m "refactor: assemble modular launcher entry"
```

### Task 12: 迁入回归探针并执行最终验证

**Files:**
- Create: `tests/probes/` 下的工作区适配探针
- Modify: `PROJECT_STRUCTURE.md`

**Interfaces:**
- Consumes: `D:\编程\MyCodex\shahe_launcher_test` 中与本次边界有关的行为断言。
- Produces: 不依赖外部正式源码路径的仓库内回归门禁。

- [ ] **Step 1: 参数化探针路径**

所有迁入探针统一使用：

```python
TARGET = Path(__file__).resolve().parents[2] / "沙河批量打开器.py"
```

探针生成物统一写入 `artifacts/probes/`。

- [ ] **Step 2: 运行完整语法和单元测试**

```powershell
python -m compileall -q .
python -m unittest discover -s tests -v
```

Expected: 0 failures, 0 errors。

- [ ] **Step 3: 运行全部迁入探针**

```powershell
Get-ChildItem tests\probes\*.py | ForEach-Object { python $_.FullName; if ($LASTEXITCODE -ne 0) { throw "Probe failed: $($_.Name)" } }
```

- [ ] **Step 4: 运行非破坏性启动烟雾测试**

确认 wxPython 可导入，加载入口并创建 `wx.App(False)` 和主窗口，核对标题、关键控件名称和关闭流程。不得自动启动沙盒、关闭进程、安装更新或恢复数据。

- [ ] **Step 5: 检查结构和差异**

```powershell
git diff --check
git status --short
python -c "from pathlib import Path; p=Path('沙河批量打开器.py'); print('entry_lines', len(p.read_text(encoding='utf-8-sig').splitlines()))"
```

Expected: 入口只剩引导和兼容导出；`git diff --check` 无错误。

- [ ] **Step 6: 立即写入最终验证记录并提交**

在 `PROJECT_STRUCTURE.md` 记录命令、通过数、入口行数、剩余发布验证风险和无障碍边界。

```powershell
git add tests/probes PROJECT_STRUCTURE.md
git commit -m "test: add modular launcher regression probes"
```
