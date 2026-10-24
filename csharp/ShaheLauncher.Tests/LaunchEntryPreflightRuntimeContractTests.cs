using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaheLauncher.App;
using ShaheLauncher.Core.Sandboxie;
using ShaheLauncher.Core.Scheduling;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Application")]
    [TestCategory("Scheduling")]
    public sealed class LaunchEntryPreflightRuntimeContractTests
    {
        [TestMethod]
        public void MainLaunchRejectsMissingApplicationBeforeStartingSession()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                form.InitializeUi();
                int launchButtonChanges = 0;
                form.LaunchCallbacks.SetLaunchButton = (enabled, text) => launchButtonChanges++;

                var missing = new LaunchAppTarget
                {
                    Path = Path.Combine(root, "missing.exe"),
                    Directory = root
                };

                MigrationRuntimeTestHelpers.Invoke(
                    form,
                    "StartLaunch",
                    new[] { "Box1" },
                    new[] { missing });

                Assert.AreEqual(0, launchButtonChanges);
                Assert.IsFalse(form.LaunchState.IsLaunching);
                Assert.AreEqual("应用路径无效，请重新选择", form.StatusLabel.Text);
            });
        }

        [TestMethod]
        public void MainLaunchRejectsSandboxOutsideCurrentDisplayMap()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                form.InitializeUi();
                string sandboxie = Path.Combine(root, "sandboxie");
                Directory.CreateDirectory(sandboxie);
                File.WriteAllText(Path.Combine(sandboxie, "Start.exe"), string.Empty);
                form.SandboxieClient.SandboxiePath = sandboxie;

                FieldInfo mapsField = typeof(MainForm).GetField(
                    "_sandboxDisplayMaps", BindingFlags.Instance | BindingFlags.NonPublic);
                FieldInfo initializedField = typeof(MainForm).GetField(
                    "_sandboxDisplayMapsInitialized", BindingFlags.Instance | BindingFlags.NonPublic);
                FieldInfo officialField = typeof(MainForm).GetField(
                    "_officialSandboxNames", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.IsNotNull(mapsField);
                Assert.IsNotNull(initializedField);
                Assert.IsNotNull(officialField);
                mapsField.SetValue(form, new SandboxieDisplayMaps
                {
                    NameToDisplay = new Dictionary<string, string> { { "Box1", "Box1" } },
                    DisplayToName = new Dictionary<string, string> { { "Box1", "Box1" } }
                });
                initializedField.SetValue(form, true);
                officialField.SetValue(form, new List<string> { "Box1" });

                string app = Path.Combine(root, "valid.exe");
                File.WriteAllText(app, string.Empty);
                int launchButtonChanges = 0;
                form.LaunchCallbacks.SetLaunchButton = (enabled, text) => launchButtonChanges++;
                form.SandboxList.SandboxDisplayNameScanner = () => new[] { "Box1" };
                form.SandboxList.Items.Add(new System.Windows.Forms.ListViewItem("MissingBox") { Checked = true });
                form.AppPathList.ReplaceItems(new[] { app });
                form.AppPathList.Items[0].Checked = true;
                form.LaunchCallbacks.RunBackground = action => action();
                form.LaunchCallbacks.PostToUi = action => action();
                MigrationRuntimeTestHelpers.Invoke(form, "LaunchSelectedSandboxes");

                Assert.AreEqual(0, launchButtonChanges);
                Assert.IsFalse(form.LaunchState.IsLaunching);
                StringAssert.Contains("请先在沙盒用户列表中按空格选中要启动的沙盒", form.StatusLabel.Text);
            });
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void BusyLaunchDoesNotChangeActiveState(bool validApp)
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                form.InitializeUi();
                string app = PrepareFiles(root, form);
                var original = new List<LaunchAppTarget> { new LaunchAppTarget { Path = "original.exe" } };
                form.LaunchState.AppTargets = original;
                form.LaunchState.AppPath = "original.exe";
                form.LaunchState.IsLaunching = true;
                form.LaunchState.LaunchSessionId = 17;
                form.LaunchCallbacks.AddHistory = path => Assert.Fail("Busy request changed history");
                form.LaunchCallbacks.SetLaunchButton = (enabled, text) => Assert.Fail("Busy request changed launch button");
                MigrationRuntimeTestHelpers.Invoke(form, "StartLaunch", new[] { "Box1" },
                    new[] { new LaunchAppTarget { Path = validApp ? app : app + ".missing" } });
                Assert.AreSame(original, form.LaunchState.AppTargets);
                Assert.AreEqual("original.exe", form.LaunchState.AppPath);
                Assert.AreEqual(17, form.LaunchState.LaunchSessionId);
                Assert.AreEqual("当前正在启动中，请稍候", form.StatusLabel.Text);
            });
        }

        [TestMethod]
        public void EmptyTargetsAreRejectedBeforeInvalidApplication()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                form.InitializeUi();
                MigrationRuntimeTestHelpers.Invoke(form, "StartLaunch", new[] { "  " },
                    new[] { new LaunchAppTarget { Path = "missing.exe" } });
                Assert.AreEqual("请先在沙盒用户列表中按空格选中要启动的沙盒", form.StatusLabel.Text);
            });
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void OrdinaryLaunchResolvesFreshNamesOnBackground(bool available)
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                form.InitializeUi();
                string app = PrepareFiles(root, form);
                SetMaps(form, "Box1", "旧用户");
                form.SandboxList.SetSandboxRoot(Path.Combine(root, "empty-sandboxes"));
                form.SandboxList.Items.Add(new System.Windows.Forms.ListViewItem("Box1") { Checked = true });
                form.AppPathList.ReplaceItems(new[] { app });
                form.AppPathList.Items[0].Checked = true;
                int scans = 0;
                form.SandboxList.SandboxDisplayNameScanner = () =>
                {
                    scans++;
                    SetMaps(form, available ? "Box1" : null, "新用户");
                    return available ? new[] { "新用户" } : new string[0];
                };
                Action prepare = null;
                form.LaunchCallbacks.RunBackground = action => prepare = action;
                using (var posted = new BlockingCollection<Action>())
                {
                    StubLaunch(form, posted);
                    MigrationRuntimeTestHelpers.Invoke(form, "LaunchSelectedSandboxes");
                    Assert.AreEqual(0, scans);
                    Assert.IsNotNull(prepare, "Normal launch must queue its name scan");
                    Assert.AreEqual("正在准备启动沙盒...", form.StatusLabel.Text);
                    prepare();
                    Action start;
                    Assert.IsTrue(posted.TryTake(out start, 5000));
                    start();
                    Assert.AreEqual(1, scans);
                    if (available)
                    {
                        CollectionAssert.AreEqual(new[] { "新用户" }, form.LaunchState.ActiveLaunchTargets.ToArray());
                        Assert.AreEqual("准备启动 1 个沙盒", form.StatusLabel.Text);
                        Action complete;
                        Assert.IsTrue(posted.TryTake(out complete, 5000));
                        complete();
                    }
                    else
                    {
                        Assert.IsFalse(form.LaunchState.IsLaunching);
                        Assert.AreEqual("请先在沙盒用户列表中按空格选中要启动的沙盒", form.StatusLabel.Text);
                    }
                }
            });
        }

        [TestMethod]
        public void ScheduledLaunchKeepsCapturedTargetsWithoutAvailabilityFiltering()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                form.InitializeUi();
                string app = PrepareFiles(root, form);
                SetMaps(form, "OtherBox", "其他用户");
                using (var posted = new BlockingCollection<Action>())
                {
                    StubLaunch(form, posted);
                    form.LaunchCallbacks.LaunchSelectedSandboxes(new[] { "CapturedBox" },
                        new object[] { new LaunchAppTarget { Path = app, Directory = root } });
                    Assert.IsTrue(form.LaunchState.IsLaunching);
                    CollectionAssert.AreEqual(new[] { "CapturedBox" }, form.LaunchState.ActiveLaunchTargets.ToArray());
                    Action complete;
                    Assert.IsTrue(posted.TryTake(out complete, 5000));
                    complete();
                }
            });
        }

        private static string PrepareFiles(string root, MainForm form)
        {
            string sandboxie = Path.Combine(root, "sandboxie");
            Directory.CreateDirectory(sandboxie);
            File.WriteAllText(Path.Combine(sandboxie, "Start.exe"), "");
            form.SandboxieClient.SandboxiePath = sandboxie;
            string app = Path.Combine(root, "valid.exe");
            File.WriteAllText(app, "");
            return app;
        }

        private static void SetMaps(MainForm form, string name, string display)
        {
            var maps = new SandboxieDisplayMaps();
            if (name != null)
            {
                maps.NameToDisplay[name] = display;
                maps.DisplayToName[display] = name;
            }
            typeof(MainForm).GetField("_sandboxDisplayMaps", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(form, maps);
            typeof(MainForm).GetField("_sandboxDisplayMapsInitialized", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(form, true);
        }

        private static void StubLaunch(MainForm form, BlockingCollection<Action> posted)
        {
            form.LaunchCallbacks.PostToUi = action => posted.Add(action);
            form.LaunchCallbacks.StartAppInSandbox = (name, path, directory) => true;
            form.LaunchCallbacks.GetSandboxPids = name => new int[0];
            form.LaunchCallbacks.AddHistory = path => { };
            form.LaunchCallbacks.Speak = (text, key) => { };
            form.LaunchCallbacks.SetLaunchStatusIfActive = (id, text) => { };
            form.LaunchCallbacks.RefreshVisibleSandboxStates = () => { };
        }
    }
}
