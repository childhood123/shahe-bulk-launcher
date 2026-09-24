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
                    Path.Combine(root, "Configuration"),
                    GetProperty<string>(formType, form, "ConfigurationDirectory"));
                Assert.AreEqual(
                    Path.Combine(root, "Configuration", "shahe_config.ini"),
                    GetProperty<string>(formType, form, "ConfigurationStorePath"));
                Assert.AreEqual(
                    Path.Combine(root, "app_config.dat"),
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

        [TestMethod]
        public void BuildsExactClassicMenusWithoutChangingAccessibilityOrder()
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
                    "准备要在沙盒中打开的应用，复选列表，按空格选中或取消选中",
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
        public void ReleaseBuildCopiesUpdaterBesideMainExecutable()
        {
            string root = FindRepositoryRoot();
            string projectSource = File.ReadAllText(Path.Combine(root, "csharp", "ShaheLauncher.App", "ShaheLauncher.App.csproj"));
            StringAssert.Contains(projectSource, "ShaheLauncher.Updater.csproj");
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
