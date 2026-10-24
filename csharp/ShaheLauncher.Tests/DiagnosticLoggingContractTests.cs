using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ShaheLauncher.App.UI.Dialogs.Common;
using ShaheLauncher.App.UI.Dialogs.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ShaheLauncher.App;
using ShaheLauncher.Core.Configuration;
using ShaheLauncher.Core.Constants;
using ShaheLauncher.Core.Runtime;
using ShaheLauncher.Core.Settings;
using ShaheLauncher.Core.Updates;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("DiagnosticLogging")]
    public sealed class DiagnosticLoggingContractTests
    {
        [TestMethod]
        public void LoggingDefaultsToOffAndRequiresAnExplicitDetailedChoice()
        {
            Assert.AreEqual("off", LauncherConstants.NormalizeLogOutputMode(null));
            Assert.AreEqual("off", LauncherConstants.NormalizeLogOutputMode("invalid"));
            Assert.AreEqual("detailed", LauncherConstants.NormalizeLogOutputMode("详细日志输出"));
            Assert.AreEqual("detailed", LauncherConstants.NormalizeLogOutputMode("detailed"));
            Assert.AreEqual("off", new LauncherSettingsState().LogOutputMode);
            WithDirectory(root => Assert.AreEqual("off", Settings(root).CreateDefaultSettings()["log_output_mode"]));
        }

        [TestMethod]
        public void DisabledRuntimeDoesNotCreateEitherLogDirectory()
        {
            WithDirectory(root =>
            {
                Runtime(root).AppendDailyLog("must not be written");
                Assert.IsFalse(Directory.Exists(Path.Combine(root, "log")));
                Assert.IsFalse(Directory.Exists(Path.Combine(root, "logs")));
            });
        }

        [TestMethod]
        public void EnabledRuntimeSerializesWritersRotatesByDateAndStopsImmediately()
        {
            WithDirectory(root =>
            {
                DateTime now = new DateTime(2026, 10, 23, 23, 59, 59);
                var first = Runtime(root, () => now);
                var second = Runtime(root, () => now);
                SetMode(first, "detailed");
                SetMode(second, "detailed");
                Parallel.For(0, 100, index => (index % 2 == 0 ? first : second).AppendDailyLog("step-" + index));
                string path = Path.Combine(root, "logs", "2026-10-23.log");
                Assert.AreEqual(100, File.ReadAllLines(path).Distinct().Count());
                now = now.AddSeconds(1);
                first.AppendDailyLog("next day");
                StringAssert.Contains(File.ReadAllText(Path.Combine(root, "logs", "2026-10-24.log")), "next day");
                SetMode(first, "off");
                first.AppendDailyLog("must not be written");
                Assert.IsFalse(File.ReadAllText(Path.Combine(root, "logs", "2026-10-24.log")).Contains("must not"));
                Assert.IsFalse(Directory.Exists(Path.Combine(root, "log")));
            });
        }

        [TestMethod]
        public void MainFormSwitchControlsDirectCallbacksAndActionFailures()
        {
            Exception failure = null;
            var thread = new Thread(() =>
            {
                try
                {
                    WithDirectory(root =>
                    {
                        using (var form = new MainForm(Runtime(root)))
                        {
                            form.LogOutputMode = "off";
                            form.Runtime.AppendDailyLog("hidden");
                            Assert.IsFalse(Directory.Exists(Path.Combine(root, "logs")));
                            form.LogOutputMode = "detailed";
                            Assert.IsFalse(form.OnCloseRequest(false));
                            var dispatch = typeof(MainForm).GetMethod("DispatchApplicationAction", BindingFlags.NonPublic | BindingFlags.Instance);
                            form.ApplicationActionCallback = action => { throw new InvalidOperationException("diagnostic failure"); };
                            Assert.ThrowsException<TargetInvocationException>(() => dispatch.Invoke(form, new object[] { "open_settings" }));
                            string text = File.ReadAllText(Directory.GetFiles(Path.Combine(root, "logs")).Single());
                            StringAssert.Contains(text, "窗口关闭请求:");
                            StringAssert.Contains(text, "操作开始: open_settings");
                            StringAssert.Contains(text, "操作异常: open_settings");
                            StringAssert.Contains(text, "InvalidOperationException");
                            StringAssert.Contains(text, "diagnostic failure");
                            form.LogOutputMode = "off";
                            form.Runtime.AppendDailyLog("hidden-after-disable");
                            Assert.AreEqual(text, File.ReadAllText(Directory.GetFiles(Path.Combine(root, "logs")).Single()));
                        }
                    });
                }
                catch (Exception error) { failure = error; }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            Assert.IsTrue(thread.Join(10000));
            if (failure != null) throw failure;
        }

        [DataTestMethod]
        [DataRow(false, false)]
        [DataRow(true, false)]
        [DataRow(true, true)]
        public void InstallerUsesSavedSwitchAndRecordsStagesOrFailure(bool enabled, bool failDownload)
        {
            WithDirectory(root =>
            {
                string app = Path.Combine(root, "app");
                Directory.CreateDirectory(app);
                string exe = Path.Combine(app, "main.exe");
                File.WriteAllText(exe, "old");
                var store = new ConfigStore(Path.Combine(app, "Configurations", "shahe_config.ini"), Environment.UserName, () => 1);
                Assert.IsTrue(store.SetJson("settings_json", new Dictionary<string, object> { { "log_output_mode", enabled ? "detailed" : "off" } }));
                string payload = Path.Combine(root, "payload");
                Directory.CreateDirectory(payload);
                File.WriteAllText(Path.Combine(payload, "main.exe"), "new");
                File.WriteAllText(Path.Combine(payload, "new.dll"), "dll");
                string zip = Path.Combine(root, "package.zip");
                ZipFile.CreateFromDirectory(payload, zip);
                string request = Path.Combine(root, "request.json");
                File.WriteAllText(request, JsonConvert.SerializeObject(new {
                    url = "https://example.invalid/package.zip", sha256 = UpdateInstaller.ComputeSha256(zip),
                    size = new FileInfo(zip).Length, restart = true, target_version = "test"
                }));
                bool restarted = false;
                int code = UpdateRunner.Run(new[] { "--app-dir", app, "--exe", exe, "--pid", "0", "--request-file", request },
                    (url, target) => { if (failDownload) throw new IOException("download probe"); File.Copy(zip, target); },
                    path => restarted = true);
                Assert.AreEqual(failDownload ? 1 : 0, code);
                Assert.AreEqual(!failDownload, restarted);
                if (!enabled)
                {
                    Assert.IsFalse(Directory.Exists(Path.Combine(app, "logs")));
                    Assert.IsFalse(Directory.Exists(Path.Combine(app, "log")));
                    return;
                }
                string text = File.ReadAllText(Directory.GetFiles(Path.Combine(app, "logs")).Single());
                foreach (string stage in failDownload
                    ? new[] { "更新安装开始", "等待主程序退出", "下载更新包", "IOException", "download probe", "清理更新临时文件" }
                    : new[] { "更新安装开始", "等待主程序退出", "下载更新包", "校验更新包", "解压更新包", "备份安装目录", "替换文件: new.dll", "重启主程序", "更新安装完成", "清理更新临时文件" })
                    StringAssert.Contains(text, stage);
            });
        }

        [TestMethod]
        public void EveryAltAndUserProcessMenuLeafAndFixedHotkeyRecordsItsAction()
        {
            OnSta(() => WithDirectory(root =>
            {
                using (var form = new MainForm(Runtime(root)))
                {
                    var logs = new List<string>();
                    var dispatched = new List<string>();
                    form.LogOutputMode = "detailed";
                    form.LogOperationCallback = logs.Add;
                    form.ApplicationActionCallback = dispatched.Add;
                    form.UpdateComboBoxCallback = null;
                    form.ApplyHotkeysCallback = null;
                    form.InitializeUi();
                    var leaves = MenuLeaves(form.Menu.MenuItems).ToList();
                    foreach (MenuItem item in leaves)
                    {
                        logs.Clear();
                        int before = dispatched.Count;
                        item.Enabled = true;
                        item.PerformClick();
                        Assert.AreEqual(before + 1, dispatched.Count, item.Text);
                        StringAssert.Contains(string.Join("\n", logs), "菜单执行: " + item.Text.Split('\t')[0]);
                        StringAssert.Contains(string.Join("\n", logs), "操作开始: " + dispatched.Last());
                        StringAssert.Contains(string.Join("\n", logs), "操作分派完成: " + dispatched.Last());
                    }
                    form.SandboxList.Items.Add("日志用户 【运行中】");
                    form.MuteCurrentCallback = name => { };
                    form.MuteAllCallback = () => { };
                    form.VolumeCurrentCallback = (name, delta) => { };
                    form.VolumeAllCallback = delta => { };
                    form.SwitchOutputCurrentCallback = name => { };
                    form.SwitchOutputAllCallback = () => { };
                    form.CloseCurrentCallback = name => { };
                    form.CloseAllCallback = () => { };
                    form.LocateCurrentCallback = name => { };
                    form.SandboxList.RefreshProcessStateCallback = () => { };
                    using (ContextMenu context = form.SandboxList.BuildContextMenu(0))
                    {
                        var processItems = MenuLeaves(context.MenuItems).Where(item => item.Text.Contains("进程")).ToList();
                        Assert.AreEqual(12, processItems.Count);
                        Assert.IsTrue(leaves.Count + processItems.Count > 35,
                            "Audit all original actions across the Alt and user-list menus.");
                        foreach (MenuItem item in processItems)
                        {
                            logs.Clear();
                            item.PerformClick();
                            StringAssert.Contains(string.Join("\n", logs), "沙盒菜单 / " + item.Text);
                        }
                    }
                    logs.Clear();
                    form.ShowSettingsDialogCallback = () => false;
                    form.FixedHotkeyActionCallback("settings");
                    StringAssert.Contains(string.Join("\n", logs), "操作开始: settings");
                    form.LogOutputMode = "off";
                    logs.Clear();
                    leaves[0].PerformClick();
                    form.FixedHotkeyActionCallback("settings");
                    Assert.AreEqual(0, logs.Count);
                }
            }));
        }

        [TestMethod]
        public void DialogButtonActionsAreLoggedWithoutLoggingFocusChanges()
        {
            OnSta(() => WithDirectory(root =>
            {
                using (var form = new MainForm(Runtime(root)))
                using (var dialog = new SettingsDialog(form, form.SettingsService.CreateDefaultSettings(), values => { }, form.SettingsService.CreateDefaultSettings()))
                {
                    var logs = new List<string>();
                    form.LogOutputMode = "detailed";
                    form.LogOperationCallback = logs.Add;
                    DialogMessageService.ConfirmHandler = (text, title, icon) => false;
                    try
                    {
                        typeof(Control).GetMethod("OnEnter", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(dialog.ResetButton, new object[] { EventArgs.Empty });
                        Assert.AreEqual(0, logs.Count, "Focus must not be described as execution.");
                        typeof(Button).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(dialog.ResetButton, new object[] { EventArgs.Empty });
                        StringAssert.Contains(string.Join("\n", logs), "操作开始: 设置 / " + dialog.ResetButton.Text);
                        StringAssert.Contains(string.Join("\n", logs), "确认结果: 重置设置=False");
                    }
                    finally { DialogMessageService.ConfirmHandler = null; }
                }
            }));
        }

        private static IEnumerable<MenuItem> MenuLeaves(Menu.MenuItemCollection items)
        {
            foreach (MenuItem item in items)
            {
                if (item.MenuItems.Count != 0)
                {
                    foreach (MenuItem child in MenuLeaves(item.MenuItems)) yield return child;
                }
                else if (item.Text != "-") yield return item;
            }
        }

        private static void OnSta(Action action)
        {
            Exception failure = null;
            var thread = new Thread(() => { try { action(); } catch (Exception error) { failure = error; } });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            Assert.IsTrue(thread.Join(10000));
            if (failure != null) throw failure;
        }

        private static LauncherRuntime Runtime(string root, Func<DateTime> now = null)
        {
            string exe = Path.Combine(root, "main.exe");
            return new LauncherRuntime(true, exe, exe, now ?? (() => DateTime.Now));
        }

        private static LauncherSettingsService Settings(string root)
        {
            return new LauncherSettingsService(new ConfigStore(Path.Combine(root, "Configurations", "shahe_config.ini"), "test", () => 1), "", root);
        }

        private static void SetMode(LauncherRuntime runtime, string mode)
        {
            PropertyInfo property = typeof(LauncherRuntime).GetProperty("LogOutputMode");
            Assert.IsNotNull(property, "The shared writer must enforce the logging switch.");
            property.SetValue(runtime, mode);
        }

        private static void WithDirectory(Action<string> action)
        {
            string root = Path.Combine(Path.GetTempPath(), "ShaheDiagnosticLogging_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            try { action(root); }
            finally { Directory.Delete(root, true); }
        }
    }
}
