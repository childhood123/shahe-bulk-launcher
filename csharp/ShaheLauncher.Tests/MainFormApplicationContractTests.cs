using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaheLauncher.Core.SandboxAdmin;
using ShaheLauncher.Core.Sandboxie;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Application")]
    public sealed class MainFormApplicationContractTests
    {
        private const string MainFormTypeName = "ShaheLauncher.App.MainForm";

        [TestMethod]
        public void ComposesEveryMigratedServiceAndRealProcessBoundary()
        {
            string root = CreateTemporaryDirectory();
            object form = null;
            try
            {
                Type formType = RequireMainFormType();
                form = CreateForm(formType, root);
                var expectedServices = new Dictionary<string, string>
                {
                    { "Runtime", "ShaheLauncher.Core.Runtime.LauncherRuntime" },
                    { "ConfigStore", "ShaheLauncher.Core.Configuration.ConfigStore" },
                    { "SettingsService", "ShaheLauncher.Core.Settings.LauncherSettingsService" },
                    { "AppPathHistoryService", "ShaheLauncher.Core.AppPaths.AppPathHistoryService" },
                    { "SandboxieClient", "ShaheLauncher.Core.Sandboxie.SandboxieClient" },
                    { "ProcessActionService", "ShaheLauncher.Core.Processes.ProcessActionService" },
                    { "SandboxAdminService", "ShaheLauncher.Core.SandboxAdmin.SandboxAdminService" },
                    { "LaunchScheduler", "ShaheLauncher.Core.Scheduling.LaunchScheduler" },
                    { "BackupRestoreService", "ShaheLauncher.Core.Backup.BackupRestoreService" },
                    { "SpeechService", "ShaheLauncher.Core.Speech.SpeechService" },
                    { "UpdateService", "ShaheLauncher.Core.Updates.UpdateService" }
                };
                foreach (KeyValuePair<string, string> item in expectedServices)
                {
                    object service = GetProperty<object>(formType, form, item.Key);
                    Assert.IsNotNull(service, item.Key + " must be composed");
                    Assert.AreEqual(item.Value, service.GetType().FullName, item.Key);
                }

                object callbacks = GetProperty<object>(formType, form, "ProcessCallbacks");
                foreach (string callbackName in new[]
                {
                    "GetAudioSessions", "ListOutputDevices", "SetOutputDevice",
                    "EnumerateWindows", "ActivateWindow", "RunProcessCommand"
                })
                {
                    Delegate callback = GetProperty<Delegate>(callbacks.GetType(), callbacks, callbackName);
                    Assert.IsNotNull(callback, callbackName + " must use a real Windows boundary");
                }

                IEnumerable sessions = (IEnumerable)GetProperty<Delegate>(
                    callbacks.GetType(), callbacks, "GetAudioSessions").DynamicInvoke();
                IEnumerable devices = (IEnumerable)GetProperty<Delegate>(
                    callbacks.GetType(), callbacks, "ListOutputDevices").DynamicInvoke();
                IEnumerable windows = (IEnumerable)GetProperty<Delegate>(
                    callbacks.GetType(), callbacks, "EnumerateWindows").DynamicInvoke();
                Assert.IsNotNull(sessions);
                Assert.IsNotNull(devices);
                Assert.IsNotNull(windows);
            }
            finally
            {
                Destroy(form);
                DeleteDirectory(root);
            }
        }

        [TestMethod]
        public void InitializesPythonPathsStateAndStartupOrder()
        {
            string root = CreateTemporaryDirectory();
            object form = null;
            try
            {
                Type formType = RequireMainFormType();
                form = CreateForm(formType, root);
                var calls = new List<string>();
                SetProperty(formType, form, "FindSandboxiePathCallback", new Func<string>(() =>
                {
                    calls.Add("scan");
                    return string.Empty;
                }));
                SetProperty(
                    formType,
                    form,
                    "ShowMessageCallback",
                    new Action<string, string, MessageBoxIcon>((text, title, icon) => calls.Add("message:" + title)));
                SetProperty(
                    formType,
                    form,
                    "StartupDelayCallback",
                    new Func<int, Action, IDisposable>((delay, callback) =>
                    {
                        calls.Add("delay:" + delay);
                        return new StubDisposable();
                    }));
                SetProperty(
                    formType,
                    form,
                    "AutoRunWriter",
                    new Action<string, string, bool>((key, path, enabled) => calls.Add("autorun")));

                ReflectionTestHelpers.InvokeInstance(form, "InitializeApplication", false);

                Assert.AreEqual(root, GetProperty<string>(formType, form, "AppDirectory"));
                Assert.AreEqual(
                    Path.Combine(root, "Configurations"),
                    GetProperty<string>(formType, form, "ConfigurationDirectory"));
                Assert.AreEqual(
                    Path.Combine(root, "Configurations", "shahe_config.ini"),
                    GetProperty<string>(formType, form, "ConfigurationStorePath"));
                Assert.AreEqual(
                    Path.Combine(root, "Configurations", "app_config.dat"),
                    GetProperty<string>(formType, form, "LegacyConfigPath"));
                Assert.IsTrue(GetProperty<bool>(formType, form, "ApplicationInitialized"));
                CollectionAssert.AreEqual(
                    new[] { "scan", "message:扫描结果", "delay:900", "autorun", "delay:1500" },
                    calls.ToArray());
                Assert.AreEqual(
                    "未找到 Sandboxie 安装路径，请在“校正”菜单中手动校正 Sandboxie 安装路径。",
                    GetProperty<Label>(formType, form, "StatusLabel").Text);
                Assert.IsFalse(GetProperty<Form>(formType, form, "SelfForm").Visible);
            }
            finally
            {
                Destroy(form);
                DeleteDirectory(root);
            }
        }

        [DataTestMethod]
        [DataRow("用户管理", "置顶/取消置顶当前用户|-|批量创建沙盒用户|批量重命名沙盒用户|批量删除沙盒用户")]
        [DataRow("设置(&S)", "热键设置|朗读设置|软件设置")]
        [DataRow("校正(&J)", "手动校正 Sandboxie 安装路径|选择沙盒用户数据目录|-|修复 Sandboxie Control 用户设置")]
        [DataRow("关于(&A)", "检查更新|-|请作者吃辣条")]
        public void BuildsExactClassicMenusWithoutChangingAccessibilityOrder(string branchTitle, string expectedChildren)
        {
            string root = CreateTemporaryDirectory();
            object form = null;
            try
            {
                string sandboxie = Path.Combine(root, "Sandboxie");
                Directory.CreateDirectory(sandboxie);
                File.WriteAllText(Path.Combine(sandboxie, "Start.exe"), string.Empty);
                Type formType = RequireMainFormType();
                form = CreateForm(formType, root);
                SetProperty(formType, form, "FindSandboxiePathCallback", new Func<string>(() => sandboxie));
                SetProperty(
                    formType,
                    form,
                    "StartupDelayCallback",
                    new Func<int, Action, IDisposable>((delay, callback) => new StubDisposable()));
                SetProperty(
                    formType,
                    form,
                    "ShowMessageCallback",
                    new Action<string, string, MessageBoxIcon>((text, title, icon) => { }));
                SetProperty(
                    formType,
                    form,
                    "AutoRunWriter",
                    new Action<string, string, bool>((key, path, enabled) => { }));

                ReflectionTestHelpers.InvokeInstance(form, "InitializeApplication", false);

                Form window = GetProperty<Form>(formType, form, "SelfForm");
                Assert.IsNotNull(window.Menu, "MainMenu must be used for classic keyboard navigation");
                CollectionAssert.AreEqual(
                    new[] { "文件(&F)", "进程(&P)", "沙盒管理(&G)", "定时(&D)", "设置(&S)", "校正(&J)", "工具(&T)", "关于(&A)" },
                    window.Menu.MenuItems.Cast<MenuItem>().Select(item => item.Text).ToArray());

                MenuItem branch = FlattenMenu(window.Menu.MenuItems)
                    .Single(item => item.Text.Split('\t')[0] == branchTitle);
                CollectionAssert.AreEqual(expectedChildren.Split('|'),
                    branch.MenuItems.Cast<MenuItem>().Select(item => item.Text.Split('\t')[0]).ToArray(),
                    "Menu wording, order and separators must match the Python branch.");

                string[] allItems = FlattenMenu(window.Menu.MenuItems).Select(item => item.Text).ToArray();
                foreach (string text in new[]
                {
                    "选择应用\tCtrl+O", "粘贴 .exe 或文件路径\tCtrl+V",
                    "启动选中沙盒", "刷新沙盒用户\tF5", "全选\tCtrl+A",
                    "静音当前打开的进程", "关闭全部打开的进程", "定位当前打开的进程",
                    "批量创建沙盒用户", "定时启动选中沙盒", "热键设置",
                    "手动校正 Sandboxie 安装路径", "备份沙盒用户", "检查更新"
                })
                {
                    CollectionAssert.Contains(allItems, text);
                }

                Assert.AreEqual(0, GetProperty<Control>(formType, form, "SandboxList").TabIndex);
                Assert.AreEqual(1, GetProperty<Control>(formType, form, "AppPathList").TabIndex);
                Assert.AreEqual(2, GetProperty<Control>(formType, form, "LaunchButton").TabIndex);
                Assert.AreEqual("沙盒用户", GetProperty<Control>(formType, form, "SandboxList").AccessibleName);
                Assert.AreEqual(
                    "准备要在沙盒中打开的应用",
                    GetProperty<Control>(formType, form, "AppPathList").AccessibleName);
            }
            finally
            {
                Destroy(form);
                DeleteDirectory(root);
            }
        }

        [TestMethod]
        public void MapsExtendedEnterToPythonNumPadEnterAndKeepsFixedKeys()
        {
            string root = CreateTemporaryDirectory();
            object form = null;
            try
            {
                Type formType = RequireMainFormType();
                form = CreateForm(formType, root);
                int numpadEnter = (int)ReflectionTestHelpers.InvokeInstance(
                    form, "NormalizeHotkeyKeyCode", Keys.Enter, true);
                int regularEnter = (int)ReflectionTestHelpers.InvokeInstance(
                    form, "NormalizeHotkeyKeyCode", Keys.Enter, false);
                Assert.AreEqual(0x10001, numpadEnter);
                Assert.AreEqual((int)Keys.Enter, regularEnter);
                Assert.AreEqual(
                    "open_app",
                    ReflectionTestHelpers.InvokeInstance(form, "ResolveHotkeyAction", Keys.Control | Keys.O, false));
            }
            finally
            {
                Destroy(form);
                DeleteDirectory(root);
            }
        }

        [TestMethod]
        public void ProcessCmdKeyDispatchesEnabledCustomActionsThroughRuntimePath()
        {
            string root = CreateTemporaryDirectory();
            object form = null;
            try
            {
                Type formType = RequireMainFormType();
                form = CreateForm(formType, root);
                var hotkeys = new Dictionary<string, string>
                {
                    { "toggle_window", "Ctrl+Shift+W" },
                    { "reset_settings", "Ctrl+Shift+X" },
                    { "toggle_auto_run", "Ctrl+Shift+1" },
                    { "toggle_show_tray", "Ctrl+Shift+2" },
                    { "toggle_auto_select_range", "Ctrl+Shift+3" },
                    { "toggle_skip_running_sandbox_on_launch", "Ctrl+Shift+4" },
                    { "toggle_remember_volume", "Ctrl+Shift+5" },
                    { "toggle_list_left_right_nav", "Ctrl+Shift+6" }
                };
                SetPrivateField(formType, form, "_hotkeys", hotkeys);
                SetPrivateField(formType, form, "_hotkeysEnabled", true);
                var actions = new List<string>();
                SetProperty(formType, form, "FixedHotkeyActionCallback", new Action<string>(actions.Add));

                var expected = new[]
                {
                    new { Key = Keys.Control | Keys.Shift | Keys.W, Action = "toggle_window" },
                    new { Key = Keys.Control | Keys.Shift | Keys.X, Action = "reset_settings" },
                    new { Key = Keys.Control | Keys.Shift | Keys.D1, Action = "toggle_auto_run" },
                    new { Key = Keys.Control | Keys.Shift | Keys.D2, Action = "toggle_show_tray" },
                    new { Key = Keys.Control | Keys.Shift | Keys.D3, Action = "toggle_auto_select_range" },
                    new { Key = Keys.Control | Keys.Shift | Keys.D4, Action = "toggle_skip_running_sandbox_on_launch" },
                    new { Key = Keys.Control | Keys.Shift | Keys.D5, Action = "toggle_remember_volume" },
                    new { Key = Keys.Control | Keys.Shift | Keys.D6, Action = "toggle_list_left_right_nav" }
                };

                foreach (var item in expected)
                {
                    Assert.IsTrue(InvokeProcessCmdKey(form, item.Key), item.Action + " must be handled");
                    Assert.AreEqual(item.Action, actions.Last());
                }

                CollectionAssert.AreEqual(
                    expected.Select(item => item.Action).ToArray(),
                    actions.ToArray());
            }
            finally
            {
                Destroy(form);
                DeleteDirectory(root);
            }
        }

        [TestMethod]
        public void ProcessCmdKeyKeepsFixedHotkeysAheadOfCustomBindings()
        {
            string root = CreateTemporaryDirectory();
            object form = null;
            try
            {
                Type formType = RequireMainFormType();
                form = CreateForm(formType, root);
                SetPrivateField(formType, form, "_hotkeys", new Dictionary<string, string>
                {
                    { "toggle_window", "Ctrl+O" }
                });
                SetPrivateField(formType, form, "_hotkeysEnabled", true);
                var actions = new List<string>();
                SetProperty(formType, form, "FixedHotkeyActionCallback", new Action<string>(actions.Add));

                Assert.IsTrue(InvokeProcessCmdKey(form, Keys.Control | Keys.O));
                CollectionAssert.AreEqual(new[] { "open_app" }, actions.ToArray());
            }
            finally
            {
                Destroy(form);
                DeleteDirectory(root);
            }
        }

        [TestMethod]
        public void ProcessCmdKeyRunsCustomActionThroughMainFormDispatcher()
        {
            string root = CreateTemporaryDirectory();
            object form = null;
            try
            {
                Type formType = RequireMainFormType();
                form = CreateForm(formType, root);
                ReflectionTestHelpers.InvokeInstance(form, "InitializeUi");
                SetPrivateField(formType, form, "_hotkeys", new Dictionary<string, string>
                {
                    { "toggle_auto_run", "Ctrl+Shift+1" }
                });
                SetPrivateField(formType, form, "_hotkeysEnabled", true);
                var writes = new List<bool>();
                SetProperty(
                    formType,
                    form,
                    "AutoRunWriter",
                    new Action<string, string, bool>((key, path, enabled) => writes.Add(enabled)));

                Assert.IsTrue(InvokeProcessCmdKey(form, Keys.Control | Keys.Shift | Keys.D1));
                Assert.IsTrue(GetProperty<bool>(formType, form, "AutoRun"));
                CollectionAssert.AreEqual(new[] { true }, writes.ToArray());
                Assert.AreEqual(
                    "已开启：开机自动启动本程序",
                    GetProperty<Label>(formType, form, "StatusLabel").Text);
            }
            finally
            {
                Destroy(form);
                DeleteDirectory(root);
            }
        }

        [TestMethod]
        public void ProcessCmdKeyLeavesDisabledInvalidAndUnknownHotkeysToWinForms()
        {
            string root = CreateTemporaryDirectory();
            object form = null;
            try
            {
                Type formType = RequireMainFormType();
                form = CreateForm(formType, root);
                var actions = new List<string>();
                SetProperty(formType, form, "FixedHotkeyActionCallback", new Action<string>(actions.Add));

                SetPrivateField(formType, form, "_hotkeys", new Dictionary<string, string>
                {
                    { "toggle_window", "Ctrl+Shift+W" }
                });
                SetPrivateField(formType, form, "_hotkeysEnabled", false);
                Assert.IsFalse(InvokeProcessCmdKey(form, Keys.Control | Keys.Shift | Keys.W));

                SetPrivateField(formType, form, "_hotkeysEnabled", true);
                SetPrivateField(formType, form, "_hotkeys", new Dictionary<string, string>
                {
                    { "toggle_window", "not-a-hotkey" }
                });
                Assert.IsFalse(InvokeProcessCmdKey(form, Keys.Control | Keys.Shift | Keys.W));
                Assert.IsFalse(InvokeProcessCmdKey(form, Keys.F24));
                Assert.AreEqual(0, actions.Count);
            }
            finally
            {
                Destroy(form);
                DeleteDirectory(root);
            }
        }

        [TestMethod]
        public void ExecutesSettingToggleActionsAndUpdatesRuntimeState()
        {
            string root = CreateTemporaryDirectory();
            object form = null;
            try
            {
                Type formType = RequireMainFormType();
                form = CreateForm(formType, root);
                ReflectionTestHelpers.InvokeInstance(form, "InitializeUi");
                var autoRunWrites = new List<bool>();
                SetProperty(
                    formType,
                    form,
                    "AutoRunWriter",
                    new Action<string, string, bool>((key, path, enabled) => autoRunWrites.Add(enabled)));

                InvokePrivate(form, "ExecuteApplicationAction", "toggle_auto_run");
                Assert.IsTrue(GetProperty<bool>(formType, form, "AutoRun"));
                CollectionAssert.AreEqual(new[] { true }, autoRunWrites.ToArray());

                InvokePrivate(form, "ExecuteApplicationAction", "toggle_show_tray");
                Assert.IsFalse(GetProperty<bool>(formType, form, "ShowTray"));

                InvokePrivate(form, "ExecuteApplicationAction", "toggle_auto_select_range");
                InvokePrivate(form, "ExecuteApplicationAction", "toggle_skip_running_sandbox_on_launch");
                InvokePrivate(form, "ExecuteApplicationAction", "toggle_remember_volume");
                InvokePrivate(form, "ExecuteApplicationAction", "toggle_list_left_right_nav");

                object settingsState = GetPrivateField(formType, form, "_settingsState");
                Assert.IsTrue(GetProperty<bool>(settingsState.GetType(), settingsState, "AutoSelectRangeOnCheck"));
                Assert.IsFalse(GetProperty<bool>(settingsState.GetType(), settingsState, "ListUseLeftRightNavigation"));
                Assert.IsTrue(GetProperty<bool>(settingsState.GetType(), settingsState, "SkipRunningSandboxOnLaunch"));
                Assert.IsFalse(GetProperty<bool>(settingsState.GetType(), settingsState, "RememberVolume"));
                object launchState = GetProperty<object>(formType, form, "LaunchState");
                object processState = GetProperty<object>(formType, form, "ProcessState");
                Assert.IsTrue(GetProperty<bool>(launchState.GetType(), launchState, "SkipRunningSandboxOnLaunch"));
                Assert.IsFalse(GetProperty<bool>(processState.GetType(), processState, "RememberVolume"));
                Assert.AreEqual(
                    "已关闭：用户列表左右光标选择",
                    GetProperty<Label>(formType, form, "StatusLabel").Text);
            }
            finally
            {
                Destroy(form);
                DeleteDirectory(root);
            }
        }

        [TestMethod]
        public void ExecutesWindowToggleAndHonorsResetCancellation()
        {
            string root = CreateTemporaryDirectory();
            object form = null;
            try
            {
                Type formType = RequireMainFormType();
                form = CreateForm(formType, root);
                ReflectionTestHelpers.InvokeInstance(form, "InitializeUi");

                InvokePrivate(form, "ExecuteApplicationAction", "toggle_window");
                Assert.IsTrue(GetProperty<Form>(formType, form, "SelfForm").Visible);
                Assert.AreEqual("已显示主窗口", GetProperty<Label>(formType, form, "StatusLabel").Text);

                string statusBeforeReset = GetProperty<Label>(formType, form, "StatusLabel").Text;
                SetProperty(
                    formType,
                    form,
                    "ConfirmMessageCallback",
                    new Func<string, string, MessageBoxButtons, MessageBoxIcon, DialogResult>(
                        (text, title, buttons, icon) => DialogResult.No));
                InvokePrivate(form, "ExecuteApplicationAction", "reset_settings");
                Assert.AreEqual(
                    statusBeforeReset,
                    GetProperty<Label>(formType, form, "StatusLabel").Text);
            }
            finally
            {
                Destroy(form);
                DeleteDirectory(root);
            }
        }

        [TestMethod]
        public void ProgramPreservesCleanupSingleInstanceRunAndReleaseOrder()
        {
            string root = FindRepositoryRoot();
            string programSource = File.ReadAllText(Path.Combine(root, "csharp", "ShaheLauncher.App", "Program.cs"));
            int cleanup = programSource.IndexOf("CleanupLegacyReleaseArtifacts", StringComparison.Ordinal);
            int guard = programSource.IndexOf("new SingleInstanceGuard(\"SandboxLauncher\")", StringComparison.Ordinal);
            int check = programSource.IndexOf(".Check()", StringComparison.Ordinal);
            int visualStyles = programSource.IndexOf("Application.EnableVisualStyles", StringComparison.Ordinal);
            int initialize = programSource.IndexOf("InitializeApplication", StringComparison.Ordinal);
            int run = programSource.IndexOf("Application.Run", StringComparison.Ordinal);
            int release = programSource.LastIndexOf(".Release()", StringComparison.Ordinal);

            Assert.IsTrue(cleanup >= 0 && cleanup < guard, "legacy cleanup must run before the guard");
            Assert.IsTrue(guard < check && check < visualStyles, "single-instance rejection must happen before WinForms initialization");
            Assert.IsTrue(visualStyles < initialize && initialize < run, "the form must initialize before Application.Run");
            Assert.IsTrue(run < release, "the guard must be released after the message loop");
            StringAssert.Contains(programSource, "NotifyExistingInstance");
            StringAssert.Contains(programSource, "SetCompatibleTextRenderingDefault(false)");
            StringAssert.Contains(programSource, "finally");
        }

        [TestMethod]
        public void ProgramDefersFirstShowUntilApplicationRun()
        {
            string root = FindRepositoryRoot();
            string programSource = File.ReadAllText(Path.Combine(root, "csharp", "ShaheLauncher.App", "Program.cs"));
            int initialize = programSource.IndexOf("InitializeApplication(false)", StringComparison.Ordinal);
            int tray = programSource.IndexOf("CreateTaskBarIcon", StringComparison.Ordinal);
            int run = programSource.IndexOf("Application.Run", StringComparison.Ordinal);
            Assert.IsTrue(initialize >= 0, "startup must initialize without showing before Application.Run");
            Assert.IsTrue(tray >= 0 && tray < run, "tray setup must remain before the message loop");
            Assert.IsTrue(initialize < run, "initialization must precede Application.Run");
        }

        [TestMethod]
        public void ReleaseBuildUsesEmbeddedUpdaterAndOptionalLegacyCompatibility()
        {
            string root = FindRepositoryRoot();
            string projectSource = File.ReadAllText(Path.Combine(root, "csharp", "ShaheLauncher.App", "ShaheLauncher.App.csproj"));
            Assert.IsFalse(projectSource.Contains("ShaheLauncher.Updater.csproj"));
            StringAssert.Contains(projectSource, "IncludeLegacyUpdateCompatibility");
            StringAssert.Contains(projectSource, "updater.exe");
            StringAssert.Contains(projectSource, "Copy");
        }

        [TestMethod]
        public void DestroyDisposesComposedApplicationServices()
        {
            string root = CreateTemporaryDirectory();
            object form = null;
            try
            {
                Type formType = RequireMainFormType();
                form = CreateForm(formType, root);
                Assert.IsTrue((bool)ReflectionTestHelpers.InvokeInstance(form, "Destroy"));
                Assert.IsTrue(GetProperty<bool>(formType, form, "ApplicationServicesDisposed"));
                form = null;
            }
            finally
            {
                Destroy(form);
                DeleteDirectory(root);
            }
        }

        [TestMethod]
        public void WiresMigratedFeatureServicesIntoInteractiveApplicationCallbacks()
        {
            string root = CreateTemporaryDirectory();
            object form = null;
            try
            {
                Type formType = RequireMainFormType();
                form = CreateForm(formType, root);
                foreach (string callbackName in new[]
                {
                    "LaunchSelectedCallback", "MuteCurrentCallback", "MuteAllCallback",
                    "VolumeCurrentCallback", "VolumeAllCallback", "SwitchOutputCurrentCallback",
                    "SwitchOutputAllCallback", "CloseCurrentCallback", "CloseAllCallback",
                    "LocateCurrentCallback", "RefreshSandboxListCallback", "CancelAllSchedulesCallback"
                })
                {
                    Assert.IsNotNull(
                        GetProperty<Delegate>(formType, form, callbackName),
                        callbackName + " must route an explicit user action");
                }

                object processCallbacks = GetProperty<object>(formType, form, "ProcessCallbacks");
                foreach (string callbackName in new[]
                {
                    "GetSandboxPids", "GetAllSandboxPids", "ScanSandboxNames",
                    "GetSandboxieStartExecutable", "GetOfficialSandboxNames", "RunSandboxieStart",
                    "ChooseWindow", "ChooseOutputDevice", "ResolveSandboxNames",
                    "ResolveVolumeMemoryKey", "RefreshProcessUi", "RefreshVisibleStates"
                })
                {
                    Assert.IsNotNull(
                        GetProperty<Delegate>(processCallbacks.GetType(), processCallbacks, callbackName),
                        callbackName + " must use the composed runtime");
                }

                object speech = GetProperty<object>(formType, form, "SpeechService");
                object speechCallbacks = GetProperty<object>(speech.GetType(), speech, "Callbacks");
                Assert.IsNotNull(GetProperty<Delegate>(speechCallbacks.GetType(), speechCallbacks, "EnumerateProcessNames"));
                Assert.IsNotNull(GetProperty<Delegate>(speechCallbacks.GetType(), speechCallbacks, "ShowSettings"));
                Assert.IsNotNull(GetProperty<object>(formType, form, "ProcessState"));
                Assert.IsNotNull(GetProperty<object>(formType, form, "LaunchState"));
                Assert.IsNotNull(GetProperty<object>(formType, form, "SandboxAdminState"));
            }
            finally
            {
                Destroy(form);
                DeleteDirectory(root);
            }
        }

        [TestMethod]
        public void WiresNonDestructiveMenuActionsWithoutDefaultStatusFallback()
        {
            string root = CreateTemporaryDirectory();
            object form = null;
            try
            {
                string sandboxie = Path.Combine(root, "Sandboxie");
                string sandboxRoot = Path.Combine(root, "SandboxUsers");
                Directory.CreateDirectory(sandboxie);
                Directory.CreateDirectory(sandboxRoot);
                File.WriteAllText(Path.Combine(sandboxie, "Start.exe"), string.Empty);
                Type formType = RequireMainFormType();
                form = CreateForm(formType, root);
                SetProperty(formType, form, "FindSandboxiePathCallback", new Func<string>(() => sandboxie));
                SetProperty(
                    formType,
                    form,
                    "StartupDelayCallback",
                    new Func<int, Action, IDisposable>((delay, callback) => new StubDisposable()));
                SetProperty(
                    formType,
                    form,
                    "ShowMessageCallback",
                    new Action<string, string, MessageBoxIcon>((text, title, icon) => { }));
                SetProperty(
                    formType,
                    form,
                    "AutoRunWriter",
                    new Action<string, string, bool>((key, path, enabled) => { }));
                ReflectionTestHelpers.InvokeInstance(form, "InitializeApplication", false);

                var calls = new List<string>();
                SetProperty(formType, form, "SelectPreviousSandboxCallback", new Action(() => calls.Add("previous")));
                SetProperty(formType, form, "SelectNextSandboxCallback", new Action(() => calls.Add("next")));
                SetProperty(formType, form, "ShowHotkeySettingsCallback", new Func<bool>(() =>
                {
                    calls.Add("hotkeys");
                    return true;
                }));
                SetProperty(formType, form, "ShowSettingsDialogCallback", new Func<bool>(() =>
                {
                    calls.Add("settings");
                    return true;
                }));
                SetProperty(formType, form, "SelectDirectoryCallback", new Func<string, string, string>((title, defaultPath) =>
                {
                    calls.Add("directory:" + title);
                    return title == "手动校正 Sandboxie 安装路径" ? sandboxie : sandboxRoot;
                }));
                SetProperty(formType, form, "ShowQrCodeCallback", new Action<string>(name => calls.Add("qr:" + name)));

                ClickMenuActionAndAssertNoFallback(formType, form, "切换到上一个用户", "select_prev_user");
                ClickMenuActionAndAssertNoFallback(formType, form, "切换到下一个用户", "select_next_user");
                ClickMenuActionAndAssertNoFallback(formType, form, "热键设置", "hotkey_settings");
                ClickMenuActionAndAssertNoFallback(formType, form, "软件设置", "settings");
                ClickMenuActionAndAssertNoFallback(formType, form, "手动校正 Sandboxie 安装路径", "manual_correct_sandboxie_path");
                ClickMenuActionAndAssertNoFallback(formType, form, "选择沙盒用户数据目录", "select_sandbox_root");
                ClickMenuActionAndAssertNoFallback(formType, form, "微信打赏", "show_wechat_qr");
                ClickMenuActionAndAssertNoFallback(formType, form, "支付宝打赏", "show_alipay_qr");

                CollectionAssert.AreEqual(
                    new[]
                    {
                        "previous", "next", "hotkeys", "settings",
                        "directory:手动校正 Sandboxie 安装路径",
                        "directory:选择沙盒用户数据目录",
                        "qr:微信赞赏码.jpg", "qr:支付宝赞赏码.jpg"
                    },
                    calls.ToArray());
                object sandboxieClient = GetProperty<object>(formType, form, "SandboxieClient");
                Assert.AreEqual(Path.GetFullPath(sandboxie), GetProperty<string>(
                    sandboxieClient.GetType(), sandboxieClient, "SandboxiePath"));
                Assert.AreEqual(Path.GetFullPath(sandboxRoot), GetProperty<string>(
                    sandboxieClient.GetType(), sandboxieClient, "SandboxRoot"));
            }
            finally
            {
                Destroy(form);
                DeleteDirectory(root);
            }
        }

        [TestMethod]
        public void WiresBackupMenuActionToBackupRestoreService()
        {
            string root = CreateTemporaryDirectory();
            object form = null;
            try
            {
                string sandboxie = Path.Combine(root, "Sandboxie");
                string sandboxRoot = Path.Combine(root, "SandboxUsers");
                string saveRoot = Path.Combine(root, "backups");
                Directory.CreateDirectory(sandboxie);
                Directory.CreateDirectory(Path.Combine(sandboxRoot, "Box1"));
                Directory.CreateDirectory(saveRoot);
                File.WriteAllText(Path.Combine(sandboxie, "Start.exe"), string.Empty);
                File.WriteAllText(Path.Combine(sandboxie, "Sandboxie.ini"), "[Box1]\r\nEnabled=y\r\n");
                File.WriteAllText(Path.Combine(sandboxRoot, "Box1", "data.txt"), "backup-data");

                Type formType = RequireMainFormType();
                form = CreateForm(formType, root);
                SetProperty(formType, form, "FindSandboxiePathCallback", new Func<string>(() => sandboxie));
                SetProperty(
                    formType,
                    form,
                    "StartupDelayCallback",
                    new Func<int, Action, IDisposable>((delay, callback) => new StubDisposable()));
                SetProperty(
                    formType,
                    form,
                    "ShowMessageCallback",
                    new Action<string, string, MessageBoxIcon>((text, title, icon) => { }));
                SetProperty(
                    formType,
                    form,
                    "AutoRunWriter",
                    new Action<string, string, bool>((key, path, enabled) => { }));
                ReflectionTestHelpers.InvokeInstance(form, "InitializeApplication", false);

                object sandboxieClient = GetProperty<object>(formType, form, "SandboxieClient");
                SetProperty(sandboxieClient.GetType(), sandboxieClient, "SandboxiePath", sandboxie);
                SetProperty(sandboxieClient.GetType(), sandboxieClient, "SandboxRoot", sandboxRoot);
                Assert.AreEqual(Path.GetFullPath(sandboxRoot), GetProperty<string>(
                    sandboxieClient.GetType(), sandboxieClient, "SandboxRoot"));
                Assert.AreEqual(1, Directory.GetDirectories(sandboxRoot).Length);
                SetProperty(formType, form, "SandboxieIniPathCallback", new Func<string>(() =>
                    Path.Combine(sandboxie, "Sandboxie.ini")));
                SetProperty(
                    formType,
                    form,
                    "SelectUsersCallback",
                    new Func<string, IEnumerable<string>, string[]>(
                        (title, users) => new[] { "Box1" }));
                SetProperty(
                    formType,
                    form,
                    "SelectDirectoryCallback",
                    new Func<string, string, string>((title, defaultPath) => saveRoot));

                ClickMenuActionAndAssertNoFallback(formType, form, "备份沙盒用户", "backup_sandboxes");
                WaitForStatus(formType, form, "备份完成");

                Assert.AreEqual(
                    1,
                    Directory.GetFiles(saveRoot, "SandboxBackup_*.sbx", SearchOption.TopDirectoryOnly).Length);
            }
            finally
            {
                Destroy(form);
                DeleteDirectory(root);
            }
        }

        [TestMethod]
        public void WiresRestoreMenuActionToBackupRestoreService()
        {
            string root = CreateTemporaryDirectory();
            object form = null;
            try
            {
                string sandboxie = Path.Combine(root, "Sandboxie");
                string sandboxRoot = Path.Combine(root, "SandboxUsers");
                string archive = Path.Combine(root, "restore.sbx");
                Directory.CreateDirectory(sandboxie);
                Directory.CreateDirectory(Path.Combine(sandboxRoot, "Box1"));
                File.WriteAllText(Path.Combine(sandboxie, "Start.exe"), string.Empty);
                File.WriteAllText(Path.Combine(sandboxie, "Sandboxie.ini"), "[Box1]\r\nEnabled=old\r\n");
                File.WriteAllText(Path.Combine(sandboxRoot, "Box1", "data.txt"), "old-data");
                CreateArchive(archive, new Dictionary<string, byte[]>
                {
                    { "SandboxUsers/Box1/data.txt", Bytes("restored-data") },
                    { "Sandboxie.ini", Bytes("[Box1]\r\nEnabled=new\r\n") }
                });

                Type formType = RequireMainFormType();
                form = CreateForm(formType, root);
                SetProperty(formType, form, "FindSandboxiePathCallback", new Func<string>(() => sandboxie));
                SetProperty(
                    formType,
                    form,
                    "StartupDelayCallback",
                    new Func<int, Action, IDisposable>((delay, callback) => new StubDisposable()));
                SetProperty(
                    formType,
                    form,
                    "ShowMessageCallback",
                    new Action<string, string, MessageBoxIcon>((text, title, icon) => { }));
                SetProperty(
                    formType,
                    form,
                    "AutoRunWriter",
                    new Action<string, string, bool>((key, path, enabled) => { }));
                ReflectionTestHelpers.InvokeInstance(form, "InitializeApplication", false);

                object sandboxieClient = GetProperty<object>(formType, form, "SandboxieClient");
                SetProperty(sandboxieClient.GetType(), sandboxieClient, "SandboxiePath", sandboxie);
                SetProperty(sandboxieClient.GetType(), sandboxieClient, "SandboxRoot", sandboxRoot);
                Assert.AreEqual(Path.GetFullPath(sandboxRoot), GetProperty<string>(
                    sandboxieClient.GetType(), sandboxieClient, "SandboxRoot"));
                Assert.AreEqual(1, Directory.GetDirectories(sandboxRoot).Length);
                SetProperty(formType, form, "SandboxieIniPathCallback", new Func<string>(() =>
                    Path.Combine(sandboxie, "Sandboxie.ini")));
                SetProperty(
                    formType,
                    form,
                    "SelectUsersCallback",
                    new Func<string, IEnumerable<string>, string[]>(
                        (title, users) => new[] { "Box1" }));
                SetProperty(
                    formType,
                    form,
                    "SelectBackupFileCallback",
                    new Func<string, string>(title => archive));
                SetProperty(
                    formType,
                    form,
                    "ConfirmMessageCallback",
                    new Func<string, string, MessageBoxButtons, MessageBoxIcon, DialogResult>(
                        (text, title, buttons, icon) =>
                            title == "确认还原" ? DialogResult.Yes : DialogResult.No));
                SetProperty(formType, form, "StopSandboxieServicesCallback", new Action(() => { }));
                SetProperty(formType, form, "ReloadSandboxieConfigCallback", new Action(() => { }));
                SetProperty(formType, form, "RestartSandboxieServicesCallback", new Action(() => { }));

                ClickMenuActionAndAssertNoFallback(formType, form, "还原沙盒用户", "restore_sandboxes");
                WaitForStatus(formType, form, "还原完成");

                Assert.AreEqual("restored-data", File.ReadAllText(Path.Combine(sandboxRoot, "Box1", "data.txt")));
                StringAssert.Contains(File.ReadAllText(Path.Combine(sandboxie, "Sandboxie.ini")), "Enabled=new");
            }
            finally
            {
                Destroy(form);
                DeleteDirectory(root);
            }
        }

        [TestMethod]
        public void WiresCreateGroupMenuActionToSandboxAdminState()
        {
            string root = CreateTemporaryDirectory();
            object form = null;
            try
            {
                Type formType = RequireMainFormType();
                form = CreateForm(formType, root);
                SetProperty(
                    formType,
                    form,
                    "StartupDelayCallback",
                    new Func<int, Action, IDisposable>((delay, callback) => new StubDisposable()));
                SetProperty(
                    formType,
                    form,
                    "ShowMessageCallback",
                    new Action<string, string, MessageBoxIcon>((text, title, icon) => { }));
                SetProperty(
                    formType,
                    form,
                    "AutoRunWriter",
                    new Action<string, string, bool>((key, path, enabled) => { }));
                ReflectionTestHelpers.InvokeInstance(form, "InitializeApplication", false);

                object sandboxList = GetProperty<object>(formType, form, "SandboxList");
                ReflectionTestHelpers.InvokeInstance(
                    sandboxList,
                    "ReplaceItemTexts",
                    new[] { "Box1", "Box2" },
                    new int[0]);
                ReflectionTestHelpers.InvokeInstance(sandboxList, "CheckItem", 0, true);
                ReflectionTestHelpers.InvokeInstance(sandboxList, "CheckItem", 1, true);
                object adminCallbacks = GetProperty<object>(formType, form, "SandboxAdminCallbacks");
                SetProperty(
                    adminCallbacks.GetType(),
                    adminCallbacks,
                    "GetExistingSandboxDisplayNames",
                    new Func<IEnumerable<string>>(() => new[] { "Box1", "Box2" }));
                SetProperty(
                    adminCallbacks.GetType(),
                    adminCallbacks,
                    "GetExistingSandboxNames",
                    new Func<IEnumerable<string>>(() => new[] { "Box1", "Box2" }));
                SetProperty(
                    adminCallbacks.GetType(),
                    adminCallbacks,
                    "NormalizeSandboxName",
                    new Func<string, string>(name => name));
                SetProperty(
                    adminCallbacks.GetType(),
                    adminCallbacks,
                    "GetSandboxDisplayName",
                    new Func<string, string>(name => name));
                var prompts = new List<string>();
                SetProperty(
                    formType,
                    form,
                    "ShowGroupNameCallback",
                    new Func<string, string, string>((title, defaultValue) =>
                    {
                        prompts.Add(title + ":" + defaultValue);
                        return "常用用户";
                    }));
                int refreshes = 0;
                SetProperty(
                    formType,
                    form,
                    "RefreshSandboxListCallback",
                    new Action(() => refreshes++));

                ClickMenuActionAndAssertNoFallback(formType, form, "新建分组", "create_group");

                object state = GetProperty<object>(formType, form, "SandboxAdminState");
                IDictionary groups = GetProperty<IDictionary>(state.GetType(), state, "SandboxGroups");
                Assert.IsTrue(groups.Contains("常用用户"));
                CollectionAssert.AreEqual(
                    new[] { "Box1", "Box2" },
                    ((IEnumerable)groups["常用用户"]).Cast<object>().ToArray());
                Assert.AreEqual("已保存分组: 常用用户", GetProperty<Label>(formType, form, "StatusLabel").Text);
                Assert.AreEqual(string.Empty, GetProperty<string>(sandboxList.GetType(), sandboxList, "CurrentGroupView"));
                CollectionAssert.AreEqual(new[] { "新建分组:" }, prompts.ToArray());
                Assert.AreEqual(1, refreshes);

                ReflectionTestHelpers.InvokeInstance(sandboxList, "UncheckAll");
                ClickMenuActionAndAssertNoFallback(formType, form, "新建分组", "create_group");
                Assert.AreEqual("请先勾选要归类的沙盒用户", GetProperty<Label>(formType, form, "StatusLabel").Text);

                ReflectionTestHelpers.InvokeInstance(sandboxList, "CheckItem", 0, true);
                ClickMenuActionAndAssertNoFallback(formType, form, "新建分组", "create_group");
                Assert.AreEqual("请至少勾选 2 个用户再新建分组", GetProperty<Label>(formType, form, "StatusLabel").Text);

                ReflectionTestHelpers.InvokeInstance(sandboxList, "CheckItem", 1, true);
                ClickMenuActionAndAssertNoFallback(formType, form, "新建分组", "create_group");
                Assert.AreEqual("分组已存在: 常用用户", GetProperty<Label>(formType, form, "StatusLabel").Text);
                CollectionAssert.AreEqual(new[] { "新建分组:", "新建分组:" }, prompts.ToArray());
                Assert.AreEqual(1, refreshes);
            }
            finally
            {
                Destroy(form);
                DeleteDirectory(root);
            }
        }


        [TestMethod]
        public void WiresLaunchGroupMenuActionToLaunchScheduler()
        {
            string root = CreateTemporaryDirectory();
            object form = null;
            try
            {
                Type formType = RequireMainFormType();
                form = CreateForm(formType, root);
                SetProperty(
                    formType,
                    form,
                    "StartupDelayCallback",
                    new Func<int, Action, IDisposable>((delay, callback) => new StubDisposable()));
                SetProperty(
                    formType,
                    form,
                    "ShowMessageCallback",
                    new Action<string, string, MessageBoxIcon>((text, title, icon) => { }));
                SetProperty(
                    formType,
                    form,
                    "AutoRunWriter",
                    new Action<string, string, bool>((key, path, enabled) => { }));
                ReflectionTestHelpers.InvokeInstance(form, "InitializeApplication", false);

                object sandboxList = GetProperty<object>(formType, form, "SandboxList");
                SetProperty(sandboxList.GetType(), sandboxList, "SandboxDisplayNameScanner",
                    new Func<IEnumerable<string>>(() => new[] { "Box1", "Box2" }));

                object adminCallbacks = GetProperty<object>(formType, form, "SandboxAdminCallbacks");
                SetProperty(
                    adminCallbacks.GetType(),
                    adminCallbacks,
                    "GetExistingSandboxDisplayNames",
                    new Func<IEnumerable<string>>(() => new[] { "Box1", "Box2" }));
                SetProperty(
                    adminCallbacks.GetType(),
                    adminCallbacks,
                    "GetExistingSandboxNames",
                    new Func<IEnumerable<string>>(() => new[] { "Box1", "Box2" }));
                SetProperty(
                    adminCallbacks.GetType(),
                    adminCallbacks,
                    "NormalizeSandboxName",
                    new Func<string, string>(name => name));
                SetProperty(
                    adminCallbacks.GetType(),
                    adminCallbacks,
                    "GetSandboxDisplayName",
                    new Func<string, string>(name => name));

                ClickMenuActionAndAssertNoFallback(formType, form, "启动分组", "launch_group");
                Assert.AreEqual(
                    "当前没有已保存的分组",
                    GetProperty<Label>(formType, form, "StatusLabel").Text);

                object state = GetProperty<object>(formType, form, "SandboxAdminState");
                IDictionary groups = GetProperty<IDictionary>(state.GetType(), state, "SandboxGroups");
                groups["A组"] = new List<string> { "Box2", "Box1" };
                groups["B组"] = new List<string>();
                groups["C组"] = new List<string> { "Missing" };

                var prompts = new List<string>();
                var choices = new Queue<string>(new[] { "A组", "B组", "C组" });
                SetProperty(
                    formType,
                    form,
                    "ShowGroupChoiceCallback",
                    new Func<string, string, IEnumerable<string>, string>((title, prompt, values) =>
                    {
                        prompts.Add(title + "|" + prompt + "|" + string.Join(",", values));
                        return choices.Dequeue();
                    }));
                var launches = new List<string[]>();
                SetProperty(
                    formType,
                    form,
                    "LaunchGroupCallback",
                    new Action<IEnumerable<string>>(names => launches.Add(names.ToArray())));

                ClickMenuActionAndAssertNoFallback(formType, form, "启动分组", "launch_group");
                CollectionAssert.AreEqual(new[] { "Box2", "Box1" }, launches.Single());
                CollectionAssert.AreEqual(
                    new[] { "启动分组|请选择要启动的分组|A组,B组,C组" },
                    prompts.ToArray());

                ClickMenuActionAndAssertNoFallback(formType, form, "启动分组", "launch_group");
                Assert.AreEqual("分组为空: B组", GetProperty<Label>(formType, form, "StatusLabel").Text);
                Assert.AreEqual(1, launches.Count);

                ClickMenuActionAndAssertNoFallback(formType, form, "启动分组", "launch_group");
                Assert.AreEqual("分组内没有可用沙盒: C组", GetProperty<Label>(formType, form, "StatusLabel").Text);
                Assert.AreEqual(1, launches.Count);
            }
            finally
            {
                Destroy(form);
                DeleteDirectory(root);
            }
        }

        [TestMethod]
        public void WiresScheduleLaunchMenuActionToLaunchScheduler()
        {
            string root = CreateTemporaryDirectory();
            object form = null;
            try
            {
                string appPath = Path.Combine(root, "demo.exe");
                File.WriteAllText(appPath, string.Empty);

                Type formType = RequireMainFormType();
                form = CreateForm(formType, root);
                var scheduledDelays = new List<int>();
                SetProperty(
                    formType,
                    form,
                    "StartupDelayCallback",
                    new Func<int, Action, IDisposable>((delay, callback) =>
                    {
                        scheduledDelays.Add(delay);
                        return new StubDisposable();
                    }));
                SetProperty(
                    formType,
                    form,
                    "ShowMessageCallback",
                    new Action<string, string, MessageBoxIcon>((text, title, icon) => { }));
                SetProperty(
                    formType,
                    form,
                    "AutoRunWriter",
                    new Action<string, string, bool>((key, path, enabled) => { }));
                SetProperty(
                    formType,
                    form,
                    "ShowScheduleDelayCallback",
                    new Func<string, string, int, int>((title, prompt, defaultValue) =>
                    {
                        Assert.AreEqual("定时启动沙盒", title);
                        Assert.AreEqual("请设置多少秒后启动所选沙盒：", prompt);
                        Assert.AreEqual(30, defaultValue);
                        return 30;
                    }));
                ReflectionTestHelpers.InvokeInstance(form, "InitializeApplication", false);

                object sandboxList = GetProperty<object>(formType, form, "SandboxList");
                ReflectionTestHelpers.InvokeInstance(sandboxList, "ReplaceItemTexts", new[] { "Box1" }, new int[0]);
                ReflectionTestHelpers.InvokeInstance(sandboxList, "CheckItem", 0, true);

                object adminCallbacks = GetProperty<object>(formType, form, "SandboxAdminCallbacks");
                SetProperty(
                    adminCallbacks.GetType(),
                    adminCallbacks,
                    "GetExistingSandboxDisplayNames",
                    new Func<IEnumerable<string>>(() => new[] { "Box1" }));
                SetProperty(
                    adminCallbacks.GetType(),
                    adminCallbacks,
                    "GetExistingSandboxNames",
                    new Func<IEnumerable<string>>(() => new[] { "Box1" }));
                SetProperty(
                    adminCallbacks.GetType(),
                    adminCallbacks,
                    "NormalizeSandboxName",
                    new Func<string, string>(name => name));
                SetProperty(
                    adminCallbacks.GetType(),
                    adminCallbacks,
                    "GetSandboxDisplayName",
                    new Func<string, string>(name => name));

                object appList = GetProperty<object>(formType, form, "AppPathList");
                ReflectionTestHelpers.InvokeInstance(appList, "ReplaceItems", (object)new[] { appPath });
                GetProperty<ListView>(formType, form, "AppPathList").Items[0].Checked = true;

                ClickMenuActionAndAssertNoFallback(formType, form, "定时启动选中沙盒", "schedule_launch");

                object state = GetProperty<object>(formType, form, "LaunchState");
                IDictionary jobs = GetProperty<IDictionary>(state.GetType(), state, "Jobs");
                Assert.AreEqual(1, jobs.Count);
                object job = jobs.Values.Cast<object>().Single();
                Assert.AreEqual("launch_selected", GetProperty<string>(job.GetType(), job, "TaskType"));
                Assert.AreEqual("定时启动 1 个沙盒", GetProperty<string>(job.GetType(), job, "Description"));
                CollectionAssert.AreEqual(
                    new[] { "Box1" },
                    ((IEnumerable)GetProperty<object>(job.GetType(), job, "SandboxTargets")).Cast<object>().ToArray());
                Assert.AreEqual(1, GetProperty<int>(job.GetType(), job, "RetryDelaySeconds"));
                CollectionAssert.Contains(scheduledDelays, 30000);
                Assert.AreEqual("已设置定时任务（30 秒后）: 定时启动 1 个沙盒", GetProperty<Label>(
                    formType, form, "StatusLabel").Text);
            }
            finally
            {
                Destroy(form);
                DeleteDirectory(root);
            }
        }

        [TestMethod]
        public void WiresScheduleCloseMenuActionToLaunchScheduler()
        {
            string root = CreateTemporaryDirectory();
            object form = null;
            try
            {
                Type formType = RequireMainFormType();
                form = CreateForm(formType, root);
                var scheduledDelays = new List<int>();
                SetProperty(
                    formType,
                    form,
                    "StartupDelayCallback",
                    new Func<int, Action, IDisposable>((delay, callback) =>
                    {
                        scheduledDelays.Add(delay);
                        return new StubDisposable();
                    }));
                SetProperty(
                    formType,
                    form,
                    "ShowMessageCallback",
                    new Action<string, string, MessageBoxIcon>((text, title, icon) => { }));
                SetProperty(
                    formType,
                    form,
                    "AutoRunWriter",
                    new Action<string, string, bool>((key, path, enabled) => { }));
                SetProperty(
                    formType,
                    form,
                    "ShowScheduleDelayCallback",
                    new Func<string, string, int, int>((title, prompt, defaultValue) =>
                    {
                        Assert.AreEqual("定时关闭沙盒", title);
                        Assert.AreEqual("请设置多少秒后关闭所选沙盒：", prompt);
                        Assert.AreEqual(30, defaultValue);
                        return 30;
                    }));
                ReflectionTestHelpers.InvokeInstance(form, "InitializeApplication", false);

                object sandboxList = GetProperty<object>(formType, form, "SandboxList");
                ReflectionTestHelpers.InvokeInstance(sandboxList, "ReplaceItemTexts", new[] { "Box1" }, new int[0]);
                ReflectionTestHelpers.InvokeInstance(sandboxList, "CheckItem", 0, true);

                object adminCallbacks = GetProperty<object>(formType, form, "SandboxAdminCallbacks");
                SetProperty(
                    adminCallbacks.GetType(),
                    adminCallbacks,
                    "GetExistingSandboxDisplayNames",
                    new Func<IEnumerable<string>>(() => new[] { "Box1" }));
                SetProperty(
                    adminCallbacks.GetType(),
                    adminCallbacks,
                    "GetExistingSandboxNames",
                    new Func<IEnumerable<string>>(() => new[] { "Box1" }));
                SetProperty(
                    adminCallbacks.GetType(),
                    adminCallbacks,
                    "NormalizeSandboxName",
                    new Func<string, string>(name => name));
                SetProperty(
                    adminCallbacks.GetType(),
                    adminCallbacks,
                    "GetSandboxDisplayName",
                    new Func<string, string>(name => name));

                object launchCallbacks = GetProperty<object>(formType, form, "LaunchCallbacks");
                SetProperty(
                    launchCallbacks.GetType(),
                    launchCallbacks,
                    "DeleteActiveLaunchReferencesByDisplay",
                    new Action<IEnumerable<string>>(names => { }));
                SetProperty(
                    launchCallbacks.GetType(),
                    launchCallbacks,
                    "CloseSandboxProcesses",
                    new Func<string, string>(name => "all_closed"));
                SetProperty(
                    launchCallbacks.GetType(),
                    launchCallbacks,
                    "RefreshProcessUi",
                    new Action<bool>(refresh => { }));

                ClickMenuActionAndAssertNoFallback(formType, form, "定时关闭选中沙盒", "schedule_close");

                object state = GetProperty<object>(formType, form, "LaunchState");
                IDictionary jobs = GetProperty<IDictionary>(state.GetType(), state, "Jobs");
                Assert.AreEqual(1, jobs.Count);
                object job = jobs.Values.Cast<object>().Single();
                Assert.AreEqual("close_selected", GetProperty<string>(job.GetType(), job, "TaskType"));
                Assert.AreEqual("定时关闭 1 个沙盒", GetProperty<string>(job.GetType(), job, "Description"));
                CollectionAssert.AreEqual(
                    new[] { "Box1" },
                    ((IEnumerable)GetProperty<object>(job.GetType(), job, "SandboxTargets")).Cast<object>().ToArray());
                Assert.AreEqual(1, GetProperty<int>(job.GetType(), job, "RetryDelaySeconds"));
                CollectionAssert.Contains(scheduledDelays, 30000);
                Assert.AreEqual("已设置定时任务（30 秒后）: 定时关闭 1 个沙盒", GetProperty<Label>(
                    formType, form, "StatusLabel").Text);
            }
            finally
            {
                Destroy(form);
                DeleteDirectory(root);
            }
        }

        [TestMethod]
        public void WiresDeleteGroupMenuActionToSandboxAdminState()
        {
            string root = CreateTemporaryDirectory();
            object form = null;
            try
            {
                Type formType = RequireMainFormType();
                form = CreateForm(formType, root);
                SetProperty(
                    formType,
                    form,
                    "StartupDelayCallback",
                    new Func<int, Action, IDisposable>((delay, callback) => new StubDisposable()));
                SetProperty(
                    formType,
                    form,
                    "ShowMessageCallback",
                    new Action<string, string, MessageBoxIcon>((text, title, icon) => { }));
                SetProperty(
                    formType,
                    form,
                    "AutoRunWriter",
                    new Action<string, string, bool>((key, path, enabled) => { }));
                ReflectionTestHelpers.InvokeInstance(form, "InitializeApplication", false);

                ClickMenuActionAndAssertNoFallback(formType, form, "删除分组", "delete_group");
                Assert.AreEqual(
                    "当前没有已保存的分组",
                    GetProperty<Label>(formType, form, "StatusLabel").Text);

                object state = GetProperty<object>(formType, form, "SandboxAdminState");
                IDictionary groups = GetProperty<IDictionary>(state.GetType(), state, "SandboxGroups");
                groups["常用用户"] = new List<string> { "Box1", "Box2" };
                object sandboxList = GetProperty<object>(formType, form, "SandboxList");
                SetProperty(sandboxList.GetType(), sandboxList, "CurrentGroupView", "常用用户");
                int refreshes = 0;
                SetProperty(
                    formType,
                    form,
                    "RefreshSandboxListCallback",
                    new Action(() => refreshes++));
                var prompts = new List<string>();
                SetProperty(
                    formType,
                    form,
                    "ShowGroupChoiceCallback",
                    new Func<string, string, IEnumerable<string>, string>((title, prompt, choices) =>
                    {
                        prompts.Add(title + "|" + prompt + "|" + string.Join(",", choices));
                        return "常用用户";
                    }));

                ClickMenuActionAndAssertNoFallback(formType, form, "删除分组", "delete_group");
                Assert.IsFalse(groups.Contains("常用用户"));
                Assert.AreEqual(string.Empty, GetProperty<string>(sandboxList.GetType(), sandboxList, "CurrentGroupView"));
                Assert.AreEqual("已删除分组: 常用用户", GetProperty<Label>(formType, form, "StatusLabel").Text);
                CollectionAssert.AreEqual(
                    new[] { "删除分组|请选择要删除的分组|常用用户" },
                    prompts.ToArray());
                Assert.AreEqual(1, refreshes);
            }
            finally
            {
                Destroy(form);
                DeleteDirectory(root);
            }
        }
        [TestMethod]
        public void WiresBatchCreateSandboxesMenuActionToSandboxieClient()
        {
            string root = CreateTemporaryDirectory();
            object form = null;
            try
            {
                Type formType = RequireMainFormType();
                form = CreateForm(formType, root);
                SetProperty(
                    formType,
                    form,
                    "StartupDelayCallback",
                    new Func<int, Action, IDisposable>((delay, callback) => new StubDisposable()));
                SetProperty(
                    formType,
                    form,
                    "ShowMessageCallback",
                    new Action<string, string, MessageBoxIcon>((text, title, icon) => { }));
                SetProperty(
                    formType,
                    form,
                    "AutoRunWriter",
                    new Action<string, string, bool>((key, path, enabled) => { }));
                ReflectionTestHelpers.InvokeInstance(form, "InitializeApplication", false);

                string sandboxRoot = Path.Combine(root, "SandboxUsers");
                object sandboxieClient = GetProperty<object>(formType, form, "SandboxieClient");
                SetProperty(sandboxieClient.GetType(), sandboxieClient, "SandboxRoot", sandboxRoot);
                object adminCallbacks = GetProperty<object>(formType, form, "SandboxAdminCallbacks");
                SetProperty(
                    adminCallbacks.GetType(),
                    adminCallbacks,
                    "GetExistingSandboxDisplayNames",
                    new Func<IEnumerable<string>>(() => new[] { "Box1", "Box3" }));
                SetProperty(
                    adminCallbacks.GetType(),
                    adminCallbacks,
                    "GetExistingSandboxNames",
                    new Func<IEnumerable<string>>(() => new[] { "Box1", "Box3" }));
                SetProperty(
                    adminCallbacks.GetType(),
                    adminCallbacks,
                    "NormalizeSandboxName",
                    new Func<string, string>(name => name));
                SetProperty(
                    adminCallbacks.GetType(),
                    adminCallbacks,
                    "GetSandboxDisplayName",
                    new Func<string, string>(name => name));

                var prompts = new List<string>();
                SetProperty(
                    formType,
                    form,
                    "ShowBatchCreateRuleCallback",
                    new Func<string, string, IDictionary>((title, prompt) =>
                    {
                        prompts.Add(title + "|" + prompt);
                        return new Dictionary<string, object>
                        {
                            { "prefix", "Box" },
                            { "start", 1 },
                            { "digits", 0 },
                            { "count", 3 }
                        };
                    }));
                var created = new List<string>();
                SetProperty(
                    formType,
                    form,
                    "CreateSandboxCallback",
                    new Func<string, SandboxieOperationResult>(name =>
                    {
                        created.Add(name);
                        return new SandboxieOperationResult { Success = true, InternalName = name };
                    }));
                int refreshes = 0;
                SetProperty(
                    formType,
                    form,
                    "RefreshSandboxListCallback",
                    new Action(() => refreshes++));

                ClickMenuActionAndAssertNoFallback(formType, form, "批量创建沙盒用户", "batch_create_sandboxes");
                CollectionAssert.AreEqual(new[] { "Box2" }, created.ToArray());
                CollectionAssert.AreEqual(
                    new[] { "批量创建沙盒用户|请输入批量创建规则：" },
                    prompts.ToArray());
                Assert.AreEqual("已创建 1 个沙盒用户，跳过 2 个已存在", GetProperty<Label>(formType, form, "StatusLabel").Text);
                Assert.AreEqual(1, refreshes);
            }
            finally
            {
                Destroy(form);
                DeleteDirectory(root);
            }
        }

        [TestMethod]
        public void WiresBatchRenameSandboxesMenuActionToSandboxAdminService()
        {
            string root = CreateTemporaryDirectory();
            object form = null;
            try
            {
                string sandboxRoot = Path.Combine(root, "SandboxUsers");
                Directory.CreateDirectory(Path.Combine(sandboxRoot, "Box1"));
                Directory.CreateDirectory(Path.Combine(sandboxRoot, "Box2"));
                string iniPath = Path.Combine(root, "Sandboxie.ini");
                File.WriteAllText(
                    iniPath,
                    "[Box1]\r\nEnabled=y\r\nConfigLevel=9\r\n\r\n[Box2]\r\nEnabled=y\r\nConfigLevel=9\r\n\r\n",
                    Encoding.UTF8);

                Type formType = RequireMainFormType();
                form = CreateForm(formType, root);
                SetProperty(
                    formType,
                    form,
                    "StartupDelayCallback",
                    new Func<int, Action, IDisposable>((delay, callback) => new StubDisposable()));
                SetProperty(
                    formType,
                    form,
                    "ShowMessageCallback",
                    new Action<string, string, MessageBoxIcon>((text, title, icon) => { }));
                SetProperty(
                    formType,
                    form,
                    "AutoRunWriter",
                    new Action<string, string, bool>((key, path, enabled) => { }));
                ReflectionTestHelpers.InvokeInstance(form, "InitializeApplication", false);

                object sandboxieClient = GetProperty<object>(formType, form, "SandboxieClient");
                SetProperty(sandboxieClient.GetType(), sandboxieClient, "SandboxRoot", sandboxRoot);
                SetProperty(formType, form, "SandboxieIniPathCallback", new Func<string>(() => iniPath));
                SetProperty(formType, form, "ReloadSandboxieConfigCallback", new Action(() => { }));
                SetProperty(formType, form, "SetSandboxAliasCallback", new Func<string, string, bool>((name, alias) => true));

                object sandboxList = GetProperty<object>(formType, form, "SandboxList");
                ReflectionTestHelpers.InvokeInstance(
                    sandboxList,
                    "ReplaceItemTexts",
                    new[] { "Box1", "Box2" },
                    new int[0]);
                ReflectionTestHelpers.InvokeInstance(sandboxList, "CheckItem", 0, true);
                ReflectionTestHelpers.InvokeInstance(sandboxList, "CheckItem", 1, true);

                object adminCallbacks = GetProperty<object>(formType, form, "SandboxAdminCallbacks");
                SetProperty(
                    adminCallbacks.GetType(),
                    adminCallbacks,
                    "GetExistingSandboxDisplayNames",
                    new Func<IEnumerable<string>>(() => new[] { "Box1", "Box2" }));
                SetProperty(
                    adminCallbacks.GetType(),
                    adminCallbacks,
                    "GetExistingSandboxNames",
                    new Func<IEnumerable<string>>(() => new[] { "Box1", "Box2" }));
                SetProperty(
                    adminCallbacks.GetType(),
                    adminCallbacks,
                    "NormalizeSandboxName",
                    new Func<string, string>(name => name));
                SetProperty(
                    adminCallbacks.GetType(),
                    adminCallbacks,
                    "GetSandboxDisplayName",
                    new Func<string, string>(name => name));
                SetProperty(
                    adminCallbacks.GetType(),
                    adminCallbacks,
                    "GetSandboxPids",
                    new Func<string, IEnumerable<int>>(name => new int[0]));
                SetProperty(
                    adminCallbacks.GetType(),
                    adminCallbacks,
                    "SaveSettings",
                    new Action(() => { }));

                var prompts = new List<string>();
                SetProperty(
                    formType,
                    form,
                    "ShowBatchRenameRuleCallback",
                    new Func<string, string, string, int, IDictionary>((title, prompt, prefix, count) =>
                    {
                        prompts.Add(title + "|" + prompt + "|" + prefix + "|" + count);
                        return new Dictionary<string, object>
                        {
                            { "prefix", "Renamed" },
                            { "start", 1 },
                            { "digits", 0 },
                            { "count", 2 }
                        };
                    }));
                int refreshes = 0;
                SetProperty(
                    formType,
                    form,
                    "RefreshSandboxListCallback",
                    new Action(() => refreshes++));

                ClickMenuActionAndAssertNoFallback(formType, form, "批量重命名沙盒用户", "batch_rename_sandboxes");

                CollectionAssert.AreEqual(
                    new[] { "批量重命名沙盒用户|将按当前勾选顺序依次重命名，请设置新命名规则：|Box|2" },
                    prompts.ToArray());
                Assert.IsFalse(Directory.Exists(Path.Combine(sandboxRoot, "Box1")));
                Assert.IsFalse(Directory.Exists(Path.Combine(sandboxRoot, "Box2")));
                Assert.IsTrue(Directory.Exists(Path.Combine(sandboxRoot, "Renamed1")));
                Assert.IsTrue(Directory.Exists(Path.Combine(sandboxRoot, "Renamed2")));
                string ini = File.ReadAllText(iniPath);
                StringAssert.Contains(ini, "[Renamed1]");
                StringAssert.Contains(ini, "[Renamed2]");
                Assert.AreEqual("已批量重命名 2 个沙盒用户", GetProperty<Label>(formType, form, "StatusLabel").Text);
                Assert.AreEqual(1, refreshes);
            }
            finally
            {
                Destroy(form);
                DeleteDirectory(root);
            }
        }
        [TestMethod]
        public void WiresRepairSandboxieControlSettingsMenuActionToSandboxAdminService()
        {
            string root = CreateTemporaryDirectory();
            object form = null;
            try
            {
                string iniPath = Path.Combine(root, "Sandboxie.ini");
                File.WriteAllText(
                    iniPath,
                    "[UserSettings_Test]\r\nSbieCtrl_WindowCoords=1\r\nEnabled=y\r\nKeep=ok\r\n",
                    new UTF8Encoding(false));
                Type formType = RequireMainFormType();
                form = CreateForm(formType, root);
                int reloads = 0;
                var messages = new List<string>();
                SetProperty(
                    formType,
                    form,
                    "StartupDelayCallback",
                    new Func<int, Action, IDisposable>((delay, callback) => new StubDisposable()));
                SetProperty(
                    formType,
                    form,
                    "ShowMessageCallback",
                    new Action<string, string, MessageBoxIcon>((text, title, icon) => messages.Add(title + ":" + text)));
                SetProperty(
                    formType,
                    form,
                    "ConfirmMessageCallback",
                    new Func<string, string, MessageBoxButtons, MessageBoxIcon, DialogResult>(
                        (text, title, buttons, icon) => DialogResult.Yes));
                SetProperty(formType, form, "SandboxieIniPathCallback", new Func<string>(() => iniPath));
                SetProperty(formType, form, "ReloadSandboxieConfigCallback", new Action(() => reloads++));
                SetProperty(
                    formType,
                    form,
                    "AutoRunWriter",
                    new Action<string, string, bool>((key, path, enabled) => { }));

                ReflectionTestHelpers.InvokeInstance(form, "InitializeApplication", false);
                ClickMenuActionAndAssertNoFallback(
                    formType,
                    form,
                    "修复 Sandboxie Control 用户设置",
                    "repair_sandboxie_control_settings");

                Assert.AreEqual(1, reloads);
                string repaired = File.ReadAllText(iniPath);
                StringAssert.Contains(repaired, "Keep=ok");
                Assert.IsFalse(repaired.Contains("SbieCtrl_WindowCoords="));
                StringAssert.Contains(string.Join("\n", messages), "修复完成");
            }
            finally
            {
                Destroy(form);
                DeleteDirectory(root);
            }
        }

        [TestMethod]
        public void WiresBatchDeleteSandboxesMenuActionToSandboxieClient()
        {
            string root = CreateTemporaryDirectory();
            object form = null;
            try
            {
                string sandboxRoot = Path.Combine(root, "SandboxUsers");
                Directory.CreateDirectory(Path.Combine(sandboxRoot, "Box1"));

                Type formType = RequireMainFormType();
                form = CreateForm(formType, root);
                SetProperty(
                    formType,
                    form,
                    "StartupDelayCallback",
                    new Func<int, Action, IDisposable>((delay, callback) => new StubDisposable()));
                SetProperty(
                    formType,
                    form,
                    "ShowMessageCallback",
                    new Action<string, string, MessageBoxIcon>((text, title, icon) => { }));
                SetProperty(
                    formType,
                    form,
                    "ConfirmMessageCallback",
                    new Func<string, string, MessageBoxButtons, MessageBoxIcon, DialogResult>(
                        (text, title, buttons, icon) => DialogResult.Yes));
                SetProperty(
                    formType,
                    form,
                    "AutoRunWriter",
                    new Action<string, string, bool>((key, path, enabled) => { }));
                ReflectionTestHelpers.InvokeInstance(form, "InitializeApplication", false);

                object sandboxieClient = GetProperty<object>(formType, form, "SandboxieClient");
                SetProperty(sandboxieClient.GetType(), sandboxieClient, "SandboxRoot", sandboxRoot);

                object sandboxList = GetProperty<object>(formType, form, "SandboxList");
                ReflectionTestHelpers.InvokeInstance(sandboxList, "ReplaceItemTexts", new[] { "Box1" }, new int[0]);
                ReflectionTestHelpers.InvokeInstance(sandboxList, "CheckItem", 0, true);

                object state = GetProperty<object>(formType, form, "SandboxAdminState");
                IDictionary groups = GetProperty<IDictionary>(state.GetType(), state, "SandboxGroups");
                groups["常用用户"] = new List<string> { "Box1" };
                IList pinned = GetProperty<IList>(state.GetType(), state, "PinnedSandboxes");
                pinned.Add("Box1");

                object adminCallbacks = GetProperty<object>(formType, form, "SandboxAdminCallbacks");
                SetProperty(
                    adminCallbacks.GetType(),
                    adminCallbacks,
                    "GetExistingSandboxDisplayNames",
                    new Func<IEnumerable<string>>(() => new[] { "Box1" }));
                SetProperty(
                    adminCallbacks.GetType(),
                    adminCallbacks,
                    "GetExistingSandboxNames",
                    new Func<IEnumerable<string>>(() => new[] { "Box1" }));
                SetProperty(
                    adminCallbacks.GetType(),
                    adminCallbacks,
                    "NormalizeSandboxName",
                    new Func<string, string>(name => name));
                SetProperty(
                    adminCallbacks.GetType(),
                    adminCallbacks,
                    "GetSandboxDisplayName",
                    new Func<string, string>(name => name));
                SetProperty(
                    adminCallbacks.GetType(),
                    adminCallbacks,
                    "GetSandboxPids",
                    new Func<string, IEnumerable<int>>(name => new int[0]));

                var deleted = new List<string>();
                SetProperty(
                    formType,
                    form,
                    "DeleteSandboxCallback",
                    new Func<string, SandboxieOperationResult>(name =>
                    {
                        deleted.Add(name);
                        return new SandboxieOperationResult
                        {
                            Success = true,
                            InternalName = name
                        };
                    }));
                SetProperty(
                    formType,
                    form,
                    "CleanupSandboxRegistryCallback",
                    new Func<IEnumerable<string>, SandboxRegistryCleanupResult>(
                        names => new SandboxRegistryCleanupResult()));

                int refreshes = 0;
                SetProperty(
                    formType,
                    form,
                    "RefreshSandboxListCallback",
                    new Action(() => refreshes++));

                ClickMenuActionAndAssertNoFallback(formType, form, "批量删除沙盒用户", "batch_delete_sandboxes");

                CollectionAssert.AreEqual(new[] { "Box1" }, deleted.ToArray());
                Assert.IsFalse(Directory.Exists(Path.Combine(sandboxRoot, "Box1")));
                Assert.IsFalse(groups.Contains("常用用户"));
                Assert.AreEqual(0, GetProperty<IList>(state.GetType(), state, "PinnedSandboxes").Count);
                Assert.AreEqual("已批量删除 1 个沙盒用户", GetProperty<Label>(formType, form, "StatusLabel").Text);
                Assert.AreEqual(1, refreshes);
            }
            finally
            {
                Destroy(form);
                DeleteDirectory(root);
            }
        }
        [TestMethod]
        public void ExposesRestoreProgressDialogWithAccessibleStageControls()
        {
            Type dialogType = Assembly.Load("沙河批量打开器").GetType(
                "ShaheLauncher.App.UI.Dialogs.Common.BackupRestoreProgressDialog", false);
            Assert.IsNotNull(dialogType, "restore progress dialog must exist");
            Form dialog = (Form)Activator.CreateInstance(dialogType, new object[] { null });
            try
            {
                Assert.AreEqual("还原沙盒数据", dialog.Text);
                ProgressBar progress = GetProperty<ProgressBar>(dialogType, dialog, "ProgressBar");
                Label stage = GetProperty<Label>(dialogType, dialog, "StageLabel");
                Assert.AreEqual("还原进度", progress.AccessibleName);
                Assert.IsFalse(progress.TabStop);
                ReflectionTestHelpers.InvokeInstance(dialog, "UpdateProgress", 70, "合并配置文件...");
                Assert.AreEqual(70, progress.Value);
                Assert.AreEqual("合并配置文件...", stage.Text);
            }
            finally
            {
                dialog.Dispose();
            }
        }

        private static object CreateForm(Type formType, string root)
        {
            Type runtimeType = Assembly.Load("ShaheLauncher.Core")
                .GetType("ShaheLauncher.Core.Runtime.LauncherRuntime", true);
            string executable = Path.Combine(root, "沙河批量打开器.exe");
            object runtime = Activator.CreateInstance(
                runtimeType,
                false,
                executable,
                executable,
                new Func<DateTime>(() => new DateTime(2026, 8, 10, 12, 0, 0)));
            ConstructorInfo constructor = formType.GetConstructor(new[] { runtimeType });
            Assert.IsNotNull(constructor, "MainForm must support direct runtime composition without a container");
            return constructor.Invoke(new[] { runtime });
        }

        private static Type RequireMainFormType()
        {
            Type type = Assembly.Load("沙河批量打开器").GetType(MainFormTypeName, false);
            Assert.IsNotNull(type, MainFormTypeName + " must exist");
            return type;
        }

        private static T GetProperty<T>(Type type, object target, string name)
        {
            PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(property, name + " must be public");
            return (T)property.GetValue(target, null);
        }

        private static void SetProperty(Type type, object target, string name, object value)
        {
            PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(property, name + " must be public");
            property.SetValue(target, value, null);
        }

        private static void SetPrivateField(Type type, object target, string name, object value)
        {
            FieldInfo field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, name + " must exist");
            field.SetValue(target, value);
        }

        private static object GetPrivateField(Type type, object target, string name)
        {
            FieldInfo field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, name + " must exist");
            return field.GetValue(target);
        }

        private static object InvokePrivate(object target, string methodName, params object[] args)
        {
            MethodInfo method = target.GetType().GetMethod(
                methodName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, methodName + " must remain private and callable");
            return method.Invoke(target, args);
        }

        private static bool InvokeProcessCmdKey(object form, Keys keyData)
        {
            MethodInfo method = form.GetType().GetMethod(
                "ProcessCmdKey",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, "ProcessCmdKey must remain available to WinForms");
            Message message = new Message();
            object[] arguments = { message, keyData };
            return (bool)method.Invoke(form, arguments);
        }

        private static void ClickMenuActionAndAssertNoFallback(Type formType, object form, string visibleLabel, string action)
        {
            Form window = GetProperty<Form>(formType, form, "SelfForm");
            Assert.IsNotNull(window.Menu, "Main menu must exist");
            MenuItem item = FlattenMenu(window.Menu.MenuItems)
                .SingleOrDefault(candidate => GetVisibleMenuText(candidate) == visibleLabel);
            Assert.IsNotNull(item, visibleLabel + " menu item must exist");
            item.PerformClick();
            string status = GetProperty<Label>(formType, form, "StatusLabel").Text;
            Assert.AreNotEqual("已触发操作: " + action, status, action + " must not use the default action fallback");
        }

        private static void WaitForStatus(Type formType, object form, string expected)
        {
            DateTime deadline = DateTime.UtcNow.AddSeconds(5);
            while (DateTime.UtcNow < deadline)
            {
                Application.DoEvents();
                string status = GetProperty<Label>(formType, form, "StatusLabel").Text;
                if (status == expected)
                {
                    return;
                }
                Thread.Sleep(25);
            }

            Assert.AreEqual(expected, GetProperty<Label>(formType, form, "StatusLabel").Text);
        }

        private static string GetVisibleMenuText(MenuItem item)
        {
            return ((item.Text ?? string.Empty).Split('\t')[0] ?? string.Empty).Replace("&", string.Empty);
        }

        private static IEnumerable<MenuItem> FlattenMenu(Menu.MenuItemCollection items)
        {
            foreach (MenuItem item in items)
            {
                if (item.Text != "-")
                {
                    yield return item;
                }
                foreach (MenuItem child in FlattenMenu(item.MenuItems))
                {
                    yield return child;
                }
            }
        }

        private static string FindRepositoryRoot()
        {
            foreach (string start in new[] { Environment.CurrentDirectory, AppDomain.CurrentDomain.BaseDirectory })
            {
                DirectoryInfo directory = new DirectoryInfo(start);
                while (directory != null)
                {
                    if (File.Exists(Path.Combine(directory.FullName, "PROJECT_STRUCTURE.md")))
                    {
                        return directory.FullName;
                    }
                    directory = directory.Parent;
                }
            }
            Assert.Fail("Repository root not found");
            return string.Empty;
        }

        private static string CreateTemporaryDirectory()
        {
            string path = Path.Combine(Path.GetTempPath(), "shahe_app_contract_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        private static void Destroy(object form)
        {
            if (form == null)
            {
                return;
            }
            try
            {
                ReflectionTestHelpers.InvokeInstance(form, "Destroy");
            }
            catch
            {
                (form as IDisposable)?.Dispose();
            }
        }

        private static void DeleteDirectory(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
            catch
            {
            }
        }

        private static void CreateArchive(string path, IDictionary<string, byte[]> entries)
        {
            using (ZipArchive archive = ZipFile.Open(path, ZipArchiveMode.Create))
            {
                foreach (KeyValuePair<string, byte[]> item in entries)
                {
                    ZipArchiveEntry entry = archive.CreateEntry(item.Key);
                    using (Stream stream = entry.Open())
                    {
                        stream.Write(item.Value, 0, item.Value.Length);
                    }
                }
            }
        }

        private static byte[] Bytes(string value)
        {
            return Encoding.UTF8.GetBytes(value ?? string.Empty);
        }

        private sealed class StubDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
