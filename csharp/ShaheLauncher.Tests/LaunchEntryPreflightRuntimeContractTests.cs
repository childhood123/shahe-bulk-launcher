using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
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

                MigrationRuntimeTestHelpers.Invoke(
                    form,
                    "StartLaunch",
                    new[] { "MissingBox" },
                    new[] { new LaunchAppTarget { Path = app, Directory = root } });

                Assert.AreEqual(0, launchButtonChanges);
                Assert.IsFalse(form.LaunchState.IsLaunching);
                StringAssert.Contains("请先在沙盒用户列表中按空格选中要启动的沙盒", form.StatusLabel.Text);
            });
        }
    }
}
