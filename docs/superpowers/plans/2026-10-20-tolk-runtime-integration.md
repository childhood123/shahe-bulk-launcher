# Tolk x64 运行库接入实施计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 从用户提供的源码构建并接入 Tolk x64，使三个目标读屏器优先输出，无读屏器时回退 OneCore/SAPI。

**Architecture:** 保留 `SpeechService` 的动态原生加载边界。Tolk 及依赖位于软件根目录旁的 `lib/`，应用构建只负责复制；Tolk 原生库单独从提供的源码构建。

**Tech Stack:** .NET Framework 4.8、WinForms、C# 7.3、x64、MSTest、CMake、Visual Studio 2022 Build Tools。

## Global Constraints

- 只构建和发布 x64 Release。
- 不下载预编译的 Tolk 主库。
- 不引用 `TolkDotNet.dll`。
- `lib/` 只包含 5 个 DLL 和 2 个配置文件，不包含 `.txt`。
- 活动读屏器优先；没有活动读屏器时回退 OneCore/SAPI。
- Tolk C++ `bool` 使用 `UnmanagedType.I1`；Win32 `BOOL` 保持 `UnmanagedType.Bool`。
- 不进入 `GAP-07`，不修改更新功能。
- `VOICE-01` 使用一个独立提交并推送现有任务分支。
- 工作树中的解决方案、AuditRunner、`HotkeyController.cs`、`SettingsRuntimeContractTests.cs` 和测试工程历史改动属于用户已有内容，不得混入本任务提交。

---

### Task 1: 构建并封装 Tolk x64 运行文件

**Files:**
- Create: `csharp/ShaheLauncher.App/lib/Tolk.dll`
- Create: `csharp/ShaheLauncher.App/lib/nvdaControllerClient64.dll`
- Create: `csharp/ShaheLauncher.App/lib/SAAPI64.dll`
- Create: `csharp/ShaheLauncher.App/lib/ZDSRAPI_x64.dll`
- Create: `csharp/ShaheLauncher.App/lib/ZDSRAPI.ini`
- Create: `csharp/ShaheLauncher.App/lib/byctrl-x64.dll`
- Create: `csharp/ShaheLauncher.App/lib/byctrl.conf`
- Modify: `csharp/ShaheLauncher.App/ShaheLauncher.App.csproj`
- Create: `csharp/ShaheLauncher.Tests/TolkRuntimeIntegrationContractTests.cs`

**Interfaces:**
- Consumes: Tolk 源码 `artifacts/probes/tolk-downloaded/boz700908-tolk-add14ac`。
- Produces: 软件根目录相对路径 `lib/Tolk.dll` 及六个同级运行文件。

- [ ] **Step 1: 建立文件清单和复制规则 RED**

测试断言源码和构建输出的 `lib` 文件名精确为：

```csharp
string[] expected =
{
    "Tolk.dll",
    "nvdaControllerClient64.dll",
    "SAAPI64.dll",
    "ZDSRAPI_x64.dll",
    "ZDSRAPI.ini",
    "byctrl-x64.dll",
    "byctrl.conf"
};
string repositoryRoot = FindRepositoryRoot();
string libDirectory = Path.Combine(
    repositoryRoot,
    "csharp",
    "ShaheLauncher.App",
    "lib");
string[] actualFiles = Directory.Exists(libDirectory)
    ? Directory.GetFiles(libDirectory).Select(Path.GetFileName).ToArray()
    : new string[0];
CollectionAssert.AreEquivalent(expected, actualFiles);
Assert.AreEqual(0, Directory.GetFiles(libDirectory, "*.txt").Length);
```

测试文件实现 `FindRepositoryRoot()`：从 `AppDomain.CurrentDomain.BaseDirectory` 逐级向上查找 `csharp/ShaheBulkLauncher.sln`，找不到时直接 `Assert.Fail`。

运行：

```powershell
dotnet test csharp\ShaheBulkLauncher.sln --configuration Release --no-restore `
  --property:Platform=x64 `
  --filter FullyQualifiedName~TolkRuntimeIntegrationContractTests
```

预期：因 `lib` 和项目复制规则不存在而失败。

- [ ] **Step 2: 从用户提供源码构建 Tolk**

在 `artifacts/probes/tolk-downloaded/boz700908-tolk-add14ac` 运行：

```powershell
.\build.bat release --x64 --no-bootstrap
```

预期：生成 `dist\x64\Release\Tolk.dll`，返回码 `0`；不得安装或下载构建工具。

- [ ] **Step 3: 复制七个运行文件**

主库来自 `dist\x64\Release\Tolk.dll`；其余文件来自源码树的 `libs\x64\`。不得从 `D:\Program Files\bilibili-player` 复制旧文件。

- [ ] **Step 4: 添加 App 项目复制规则**

```xml
<ItemGroup>
  <None Update="lib\**\*">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    <CopyToPublishDirectory>PreserveNewest</CopyToPublishDirectory>
  </None>
</ItemGroup>
```

- [ ] **Step 5: 运行文件清单测试取得 GREEN**

预期：七个文件存在、没有 `.txt`、五个 DLL 均为 x64 PE，并复制到隔离输出的 `lib/`。

---

### Task 2: 修复 SpeechService 原生 ABI 和生命周期

**Files:**
- Modify: `csharp/ShaheLauncher.Core/Speech/SpeechService.cs`
- Test: `csharp/ShaheLauncher.Tests/TolkRuntimeIntegrationContractTests.cs`

**Interfaces:**
- Consumes: `lib/Tolk.dll` 和现有 `SpeechAssistBackend` 初始化锁、调用锁。
- Produces: `PreferSapi`、`DetectScreenReader` backend 能力，以及可重复安全关闭的原生模块生命周期。

- [ ] **Step 1: 建立路径、ABI、回退和卸载 RED**

```csharp
string appRoot = Path.Combine(Path.GetTempPath(), "shahe-tolk-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(Path.Combine(appRoot, "lib"));
File.WriteAllBytes(Path.Combine(appRoot, "lib", "Tolk.dll"), new byte[] { 0 });
var calls = new List<string>();
int unloadCalls = 0;
var backend = new SpeechAssistBackend
{
    TrySapi = value => calls.Add("try-sapi:" + value.ToString().ToLowerInvariant()),
    Load = () => calls.Add("load"),
    IsLoaded = () => { calls.Add("is-loaded"); return true; },
    Unload = () => unloadCalls++
};
PropertyInfo preferSapi = typeof(SpeechAssistBackend).GetProperty("PreferSapi");
Assert.IsNotNull(preferSapi, "backend 必须公开 PreferSapi");
preferSapi.SetValue(
    backend,
    new Action<bool>(value => calls.Add(
        "prefer-sapi:" + value.ToString().ToLowerInvariant())));
Assert.IsNotNull(
    typeof(SpeechAssistBackend).GetProperty("DetectScreenReader"),
    "backend 必须公开 DetectScreenReader");
var service = new SpeechService(
    appRoot,
    Path.Combine(appRoot, "Configuration"),
    new SpeechServiceCallbacks { LoadAssistBackend = path => backend });
Assert.AreEqual(Path.Combine(appRoot, "lib", "Tolk.dll"), service.GetTolkDllPath());
MethodInfo ensure = typeof(SpeechService).GetMethod(
    "EnsureAssistSpeechRuntime",
    BindingFlags.Instance | BindingFlags.NonPublic);
Assert.IsTrue((bool)ensure.Invoke(service, null));
CollectionAssert.AreEqual(
    new[] { "try-sapi:true", "prefer-sapi:false", "load", "is-loaded" },
    calls);
Type outputDelegate = typeof(SpeechService).GetNestedType(
    "TolkOutputDelegate",
    BindingFlags.NonPublic);
MarshalAsAttribute marshalAttribute = outputDelegate.GetMethod("Invoke")
    .ReturnParameter.GetCustomAttribute<MarshalAsAttribute>();
Assert.AreEqual(UnmanagedType.I1, marshalAttribute.Value);
service.ShutdownAssistSpeechRuntime();
service.ShutdownAssistSpeechRuntime();
Assert.AreEqual(1, unloadCalls);
```

以上断言拆分为 `PrefersRootLibTolkPath`、`BackendExposesPreferenceAndDetection`、`InitializationEnablesFallbackAfterReaders`、`TolkDelegatesUseI1` 和 `ShutdownUnloadsBackendOnce` 五个测试。新增属性通过反射检查，确保 RED 是断言失败而不是测试工程编译失败。

当前预期失败：未查找 `lib/Tolk.dll`、调用 `TrySapi(false)`、没有 `PreferSapi`、Tolk bool 使用 `UnmanagedType.Bool`、Shutdown 没有调用 backend `Unload`。

- [ ] **Step 2: 扩展 backend 接口**

```csharp
public Action<bool> TrySapi { get; set; }
public Action<bool> PreferSapi { get; set; }
public Func<string> DetectScreenReader { get; set; }
public Action Load { get; set; }
public Func<bool> IsLoaded { get; set; }
public Action Silence { get; set; }
public Action<string, bool> Output { get; set; }
public Action Unload { get; set; }
```

- [ ] **Step 3: 修改查找和初始化顺序**

查找顺序：

```csharp
Path.Combine(AppDirectory, "lib", "Tolk.dll")
Path.Combine(AppDirectory, "internal", "Tolk.dll")
Path.Combine(AppDirectory, "Tolk.dll")
```

初始化顺序：

```csharp
backend.TrySapi?.Invoke(true);
backend.PreferSapi?.Invoke(false);
backend.Load();
if (!backend.IsLoaded()) throw new InvalidOperationException("Tolk 初始化失败");
```

- [ ] **Step 4: 修正 Tolk bool ABI**

`Tolk_TrySAPI`、`Tolk_PreferSAPI` 参数，以及 `IsLoaded`、`Silence`、`Output` 返回值和 `Output` 的 interrupt 参数均使用 `[MarshalAs(UnmanagedType.I1)]`。`FreeLibrary` 保持 `[return: MarshalAs(UnmanagedType.Bool)]`。

新增导出委托：

```csharp
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
private delegate void TolkPreferSapiDelegate(
    [MarshalAs(UnmanagedType.I1)] bool preferSapi);

[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
private delegate IntPtr TolkDetectScreenReaderDelegate();
```

`DetectScreenReader` 使用 `Marshal.PtrToStringUni(pointer) ?? string.Empty` 返回托管字符串。

- [ ] **Step 5: 补齐清理**

初始化失败、`IsLoaded=false` 和程序关闭时均执行一次 `Tolk_Unload()` 与 `FreeLibrary(module)`；重复关闭不得重复释放。

```csharp
int released = 0;
Action release = () =>
{
    if (Interlocked.Exchange(ref released, 1) != 0)
    {
        return;
    }
    try
    {
        unload();
    }
    finally
    {
        NativeMethods.FreeLibrary(module);
    }
};
```

`EnsureAssistSpeechRuntime()` 的失败分支调用当前局部 backend 的 `Unload`；成功后由 `ShutdownAssistSpeechRuntime()` 取得并清空 `_assistSpeechRuntime`，再调用一次 `Unload`。

- [ ] **Step 6: 运行 Speech 定向测试取得 GREEN**

预期：路径、调用顺序、I1 ABI、失败清理和重复关闭全部通过。

---

### Task 3: 原生调用、回归和独立提交

**Files:**
- Create: `artifacts/runs/csharp_tolk_runtime_20261020/tolk-native-probe.ps1`
- Modify: `PROJECT_STRUCTURE.md`
- Modify: `docs/superpowers/plans/2026-10-20-tolk-runtime-integration.md`

**Interfaces:**
- Consumes: 隔离 Release 输出的 `lib/Tolk.dll`。
- Produces: 原生加载、检测、输出、卸载的 JSON/日志证据，以及 VOICE-01 独立提交。

- [ ] **Step 1: 构建隔离 Release x64**

```powershell
dotnet build csharp\ShaheBulkLauncher.sln --configuration Release --no-restore `
  --property:Platform=x64 `
  --property:BaseOutputPath=D:\mycodex\git\artifacts\runs\csharp_tolk_runtime_20261020\bin\
```

预期：`0` 警告、`0` 错误，输出含 `lib` 七个文件且无 `.txt`。

- [ ] **Step 2: 执行原生真实调用探针**

探针依次调用：

```text
LoadLibrary
Tolk_TrySAPI(true)
Tolk_PreferSAPI(false)
Tolk_Load
Tolk_IsLoaded
Tolk_DetectScreenReader
Tolk_Output("沙河批量打开器朗读测试", true)
Tolk_Unload
FreeLibrary
```

预期：返回码 `0`、`IsLoaded=true`、`Output=true`、模块成功释放；记录实际检测到的读屏器或 OneCore/SAPI。

- [ ] **Step 3: 执行回归门禁**

```powershell
dotnet vstest D:\mycodex\git\artifacts\runs\csharp_tolk_runtime_20261020\bin\x64\Release\net48\ShaheLauncher.Tests.dll /Platform:x64 `
  /TestCaseFilter:"TestCategory=Speech|TestCategory=Application|FullyQualifiedName~TolkRuntime"
dotnet vstest D:\mycodex\git\artifacts\runs\csharp_tolk_runtime_20261020\bin\x64\Release\net48\ShaheLauncher.Tests.dll /Platform:x64
python -m unittest discover -s tests -p "test_*.py" -v
python -m compileall -q shahe_bulk_launcher tests
git diff --check
```

- [ ] **Step 4: 每条验证后同步 PROJECT_STRUCTURE**

记录 RED/GREEN、通过/失败/跳过数量、构建警告/错误、DLL 路径、检测名称、输出结果和日志路径。

- [ ] **Step 5: 提交前列出精确暂存文件**

排除用户已有的解决方案、AuditRunner、`HotkeyController.cs`、`SettingsRuntimeContractTests.cs`、测试工程历史改动、TRX 和构建目录。

- [ ] **Step 6: 创建并推送独立提交**

```powershell
git commit -m "feat: bundle and load Tolk x64 runtime"
git push origin HEAD:refs/heads/fix/non-update-gaps-20261020
```

提交后运行 `git rev-parse HEAD` 取得实际提交哈希，并报告对应的 `git revert 实际提交哈希` 命令。

## Completion Criteria

- `lib` 精确包含七个运行文件且无 `.txt`。
- `Tolk.dll` 从用户源码成功构建。
- 三个目标读屏器优先，OneCore/SAPI 可回退。
- Tolk bool ABI、初始化和卸载生命周期正确。
- 原生真实输出、定向测试、全量测试、Python 对照和 Release x64 构建全部通过。
- `PROJECT_STRUCTURE.md` 记录结果和剩余真实读屏器逐项审计风险。
- `VOICE-01` 形成独立提交并推送。

## Risks

- 原生真实调用会播放“沙河批量打开器朗读测试”。
- 自动化通过不能替代 NVDA、争渡和保益三个真实读屏器逐项审计。
- 上游 Tolk 的其他 COM/UIA 驱动仍可能参与自动检测。
- 运行目录不复制许可证 `.txt`；正式发布前仍需单独复核再分发条款和许可证呈现方式。
