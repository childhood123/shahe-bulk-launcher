# Tolk x64 运行库接入设计

## 状态

已由用户确认方案 A，待编写实施计划并按 TDD 执行。本文只定义 `VOICE-01`，不包含 `GAP-07` 至 `GAP-10`、更新功能或界面重设计。

## 目标

- 使用用户提供的 Tolk 源码构建 x64 Release 原生运行库。
- 在 C# 应用项目中新建自包含的 `lib/` 目录，并随 Release x64 输出复制到软件根目录旁的 `lib/`。
- 当 NVDA、争渡或保益读屏器运行时，通过 Tolk 向活动读屏器输出文本。
- 当没有活动读屏器时，启用 Tolk 的 OneCore/SAPI 回退语音。
- 修正当前 C# 原生委托与 Tolk C++ `bool` ABI 不一致的问题。
- 验证真实 `Load -> Detect/Output -> Unload` 调用链，而不只验证文件存在或编译通过。

## 非目标

- 不接入 x86、ARM64 或 ARM64EC 应用构建。
- 不引用或维护第二套 `TolkDotNet.dll` 托管包装层。
- 不修改 Tolk 上游的驱动检测顺序，也不主动禁用其其他内置、COM 或 UIA 驱动。
- 不改变朗读设置界面、辅助提示文案或三个读屏器的现有 INI 配置格式。
- 不处理 `GAP-07` 的 Esc、Alt+F4 和退出反馈。
- 不下载第三方预编译二进制；构建输入只使用用户提供的源码树。

## 方案选择

### 采用：现有 `SpeechService` 动态加载原生 Tolk

保留 `SpeechService` 通过 `LoadLibrary`/`GetProcAddress` 创建 `SpeechAssistBackend` 的现有边界。该方案能使用绝对路径加载 `lib/Tolk.dll`，不需要修改进程级 DLL 搜索路径，也不会引入重复的托管包装层。

### 未采用：引用 `TolkDotNet.dll`

官方 C# 包装层使用 `[DllImport("Tolk.dll")]`。当主库位于 `lib/` 子目录时，还需要额外修改全局 DLL 搜索路径；同时会与当前 `SpeechService` 的生命周期、锁和回调结构重复。

### 未采用：每次应用构建时编译 Tolk

这会让普通 .NET 构建依赖 C++、CMake 和 Visual Studio Build Tools，增加构建时间和失败面。Tolk 原生库只在更新其源码时重新构建，应用构建仅复制已经验证的运行时文件。

## 源码与产物

Tolk 源码根目录：

`artifacts/probes/tolk-downloaded/boz700908-tolk-add14ac`

使用源码自带构建入口生成 x64 Release：

```text
build.bat release --x64 --no-bootstrap
```

本机已有 Visual Studio 2022 Build Tools、CMake 3.31.6 和 Ninja 1.12.1，不允许构建脚本自动安装工具。

应用源码目录：

```text
csharp/ShaheLauncher.App/lib/
├── Tolk.dll
├── nvdaControllerClient64.dll
├── SAAPI64.dll
├── ZDSRAPI_x64.dll
├── ZDSRAPI.ini
├── byctrl-x64.dll
├── byctrl.conf
├── LICENSE.txt
└── LICENSE-NVDA.txt
```

该布局参考 `D:\Program Files\bilibili-player`：EXE 位于软件根目录，旁边只有一层 `lib/`。参考目录只用于确定布局和文件集合；实际复制内容全部来自用户提供的 Tolk 源码构建及其 `libs/x64/`，不复用参考软件中的旧版 `Tolk.dll` 或客户端 DLL。

这些文件分别提供 Tolk 主库、NVDA、System Access、争渡、保益 x64 客户端模块、争渡/保益默认配置及随源码提供的许可证。其他 x86-only 或非目标读屏模块不复制；缺少它们时对应驱动不可用，但不得影响三个目标读屏器和 OneCore/SAPI 回退。

`TolkDotNet.dll`、`Tolk.lib`、`Tolk.exp`、`TolkGml.dll`、PDB、包装示例和构建中间文件不进入应用运行目录。

## 构建与发布布局

`ShaheLauncher.App.csproj` 将 `lib/**` 声明为内容文件，并在 Debug/Release 构建后复制到：

```text
<OutputDirectory>/lib/
```

不得把 DLL 平铺到 EXE 根目录。隔离构建、标准 Release 构建和后续发布目录必须得到同一相对布局。

发布验证必须检查九个文件均存在，且 `Tolk.dll` 和四个客户端模块为 x64 PE 文件。`byctrl.conf` 与 `ZDSRAPI.ini` 必须使用用户提供源码树中的版本，不复制参考软件内的旧配置。

## 运行时查找

`SpeechService.GetTolkDllPath()` 的顺序调整为：

1. `<AppDirectory>/lib/Tolk.dll`
2. `<AppDirectory>/internal/Tolk.dll`，保留旧发布兼容
3. `<AppDirectory>/Tolk.dll`，保留旧发布兼容

始终向原生加载器传递完整路径。Tolk 源码中的 `TolkLoadLibrary` 会优先从 `Tolk.dll` 所在目录加载同级客户端模块，因此不调用 `SetDllDirectory`，也不污染进程全局 DLL 搜索路径。

## ABI 与原生接口

Tolk 导出的 C++ `bool` 为一字节。以下委托参数或返回值使用 `[MarshalAs(UnmanagedType.I1)]`：

- `Tolk_TrySAPI(bool)` 参数
- `Tolk_PreferSAPI(bool)` 参数
- `Tolk_IsLoaded()` 返回值
- `Tolk_Silence()` 返回值
- `Tolk_Output(string, bool)` 的 `interrupt` 参数和返回值

Win32 `FreeLibrary` 返回的是四字节 `BOOL`，继续使用 `UnmanagedType.Bool`，不得随 Tolk ABI 一并修改。

新增并加载以下现有 Tolk 导出：

- `Tolk_PreferSAPI`
- `Tolk_DetectScreenReader`

检测名称用于验证和诊断，不用于重新实现 Tolk 的驱动选择算法。

## 调用顺序

首次需要辅助朗读时，在现有 `_assistSpeechInitLock` 下执行：

1. 从软件根目录旁的 `lib` 解析 `Tolk.dll`。
2. 加载导出并构造 `SpeechAssistBackend`。
3. 调用 `Tolk_TrySAPI(true)`，启用 OneCore/SAPI。
4. 调用 `Tolk_PreferSAPI(false)`，保持活动读屏器优先、系统语音仅作为回退。
5. 调用 `Tolk_Load()`。
6. 要求 `Tolk_IsLoaded()` 返回 `true`。
7. 后续在现有 `_assistSpeechCallLock` 下调用 `Tolk_Output()`；中断输出前先调用 `Tolk_Silence()`。

Tolk 的自动检测负责选择活动读屏器。三个目标读屏器中任一运行时，其客户端模块位于 `Tolk.dll` 同目录；都未运行时，检测链最终进入 OneCore/SAPI。Tolk 仍可能识别无需额外 DLL 的其他上游支持读屏器，这一行为不在本任务中移除。

## 生命周期与错误处理

- `SpeechAssistBackend.Unload` 必须同时匹配调用 `Tolk_Unload()` 并只释放一次原生模块句柄。
- `ShutdownAssistSpeechRuntime()` 必须在持有现有调用锁时取得并清空当前 backend，然后执行 `Unload`。
- `Load` 后接口不完整、`IsLoaded=false` 或初始化抛错时，必须清理已经加载的模块，不留下悬挂句柄。
- 单次进程内初始化失败后维持现有禁用策略，避免每条提示重复加载损坏的 DLL。
- 文件缺失、位数不匹配、缺少导出或原生加载失败不得使主程序崩溃；设置页继续给出可感知的缺失/失败反馈。
- Tolk 非线程安全；所有原生调用继续由 `_assistSpeechCallLock` 串行化。

## 测试策略

### RED

先建立失败测试证明：

- `GetTolkDllPath()` 尚未查找 `lib/Tolk.dll`。
- App 项目尚未把 `lib` 复制到输出目录。
- Tolk 委托仍使用错误的 `UnmanagedType.Bool`。
- 当前初始化调用 `TrySapi(false)`，不符合回退要求。
- 当前关闭路径没有调用 backend `Unload`。

### GREEN

- 单元测试验证路径优先级、调用顺序、I1 标注、失败清理和只卸载一次。
- 构建契约验证九个运行时文件复制到隔离输出的 `lib/`。
- 原生加载探针从隔离输出加载 `Tolk.dll`，核验必需导出，并依次执行 `TrySAPI(true)`、`PreferSAPI(false)`、`Load`、`IsLoaded`、`DetectScreenReader`、`Output`、`Unload`。
- 真实输出短句固定为“沙河批量打开器朗读测试”。如果没有活动读屏器，应由 OneCore/SAPI 回退输出。
- 探针记录实际检测名称、每个 API 的结果、进程位数、DLL 路径、返回码和清理结果。

### 回归门禁

- Speech 定向测试。
- 受影响的 Application/Speech 回归。
- C# 全量测试。
- Python 全量与生产源码编译。
- Release x64 隔离构建和发布目录文件检查。
- `git diff --check`。

## 完成标准

- x64 `Tolk.dll` 从用户提供源码成功构建，不依赖下载的预编译主库。
- 应用源码和 Release 输出均具有规定的根目录 `lib/` 布局及许可证。
- C# 原生接口的 Tolk `bool` 全部使用 I1，Win32 `BOOL` 保持 Bool。
- 三个目标读屏器运行时可以由 Tolk 自动检测并输出；无活动读屏器时 OneCore/SAPI 可以输出测试短句。
- 初始化失败和退出路径均能释放原生运行时，主程序不崩溃。
- 所有自动化门禁通过，原生真实调用证据写入 `artifacts/runs/` 和 `PROJECT_STRUCTURE.md`。
- `VOICE-01` 使用独立 Git 提交，不与 `GAP-06`、`GAP-07` 或更新功能混合。

## 已知风险

- 实际读屏输出依赖用户机器上对应读屏进程和客户端 API 的兼容性；自动化加载通过不能代替三个真实读屏器逐一审计。
- OneCore/SAPI 真实输出会产生可听见的测试短句，执行探针时必须明确记录场景。
- 上游 Tolk 支持的其他 COM/UIA 读屏器仍可能参与自动检测；若后续要求严格只允许三种读屏器，需要单独修改并维护 Tolk 原生源码分支。
- 客户端模块由各厂商授权；发布时必须保留源码包提供的许可证，并在正式发布前复核各厂商再分发条款。
