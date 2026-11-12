using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaheLauncher.App;
using ShaheLauncher.Core.SandboxAdmin;
using ShaheLauncher.Core.Sandboxie;

namespace ShaheLauncher.Tests
{
    [TestClass, TestCategory("SettingsService")]
    public sealed class SettingsSaveEntryPointsRuntimeTests
    {
        [TestMethod]
        public void StartupKeepsDiscoveredPathAndContinuesAfterSettingsWriteFailure()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                byte[] before = BreakConfiguration(form);
                string install = Path.Combine(root, "Sandboxie");
                Directory.CreateDirectory(install);
                File.WriteAllText(Path.Combine(install, "Start.exe"), "fixture, never execute");
                form.FindSandboxiePathCallback = () => install;
                var messages = new List<string>();
                form.ShowMessageCallback = (text, title, icon) => messages.Add(text);
                form.InitializeApplication(false);
                Assert.IsTrue(form.ApplicationInitialized);
                Assert.AreEqual(install, form.SandboxieClient.SandboxiePath);
                Assert.IsTrue(messages.Any(text => text.Contains("保存失败")));
                CollectionAssert.AreEqual(before, File.ReadAllBytes(form.ConfigurationStorePath));
            });
        }

        [TestMethod]
        public void PathCheckboxReportsSaveFailureAndAlwaysRestoresEventIndex()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                form.InitializeUi();
                string app = Path.Combine(root, "fixture.exe");
                File.WriteAllText(app, "not executed");
                form.AppPathList.BeginProgrammaticUpdate();
                form.AppPathList.ReplaceItems(new[] { "fixture" }, new[] { app }, new int[0], 0);
                form.AppPathList.EndProgrammaticUpdate();
                byte[] before = BreakConfiguration(form);
                var messages = new List<string>();
                form.ShowMessageCallback = (text, title, icon) => messages.Add(text);
                int calls = 0;
                Action callback = form.AppItemCheckedCallback;
                form.AppItemCheckedCallback = () => { calls++; callback(); };
                typeof(ListView).GetMethod("OnItemChecked", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(form.AppPathList, new object[] { new ItemCheckedEventArgs(form.AppPathList.Items[0]) });
                Assert.AreEqual(1, calls);
                Assert.IsTrue(messages.Any(text => text.Contains("保存失败")));
                Assert.AreEqual(-1, typeof(MainForm).GetField("_appItemCheckedIndex", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form));
                CollectionAssert.AreEqual(before, File.ReadAllBytes(form.ConfigurationStorePath));
            });
        }

        [TestMethod]
        public void SuccessfulDeletionCompletesCleanupAndReportsUnsavedReferences()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                form.InitializeUi();
                string users = Path.Combine(root, "users");
                string box = Path.Combine(users, "BoxA");
                Directory.CreateDirectory(box);
                File.WriteAllText(Path.Combine(box, "data.txt"), "owned");
                form.SandboxieClient.SandboxRoot = users;
                form.SandboxList.ReplaceItemTexts(new[] { "BoxA" }, new int[0]);
                form.SandboxList.CheckItem(0, true);
                form.SandboxAdminState.PinnedSandboxes.Add("BoxA");
                form.SandboxAdminCallbacks.GetExistingSandboxDisplayNames = () => new[] { "BoxA" };
                form.SandboxAdminCallbacks.GetExistingSandboxNames = () => new[] { "BoxA" };
                form.SandboxAdminCallbacks.NormalizeSandboxName = name => name;
                form.SandboxAdminCallbacks.GetSandboxPids = name => new int[0];
                form.ConfirmMessageCallback = (text, title, buttons, icon) => DialogResult.Yes;
                form.DeleteSandboxCallback = name => new SandboxieOperationResult { Success = true, InternalName = name };
                int cleanups = 0, refreshes = 0;
                form.CleanupSandboxRegistryCallback = names => { cleanups++; return new SandboxRegistryCleanupResult(); };
                form.RefreshSandboxListCallback = () => refreshes++;
                var messages = new List<string>();
                form.ShowMessageCallback = (text, title, icon) => messages.Add(text);
                byte[] before = BreakConfiguration(form);
                MigrationRuntimeTestHelpers.Invoke(form, "BatchDeleteSandboxes");
                Assert.IsFalse(Directory.Exists(box));
                Assert.AreEqual(1, cleanups);
                Assert.AreEqual(1, refreshes);
                Assert.IsFalse(form.SandboxAdminState.PinnedSandboxes.Contains("BoxA"));
                Assert.IsTrue(messages.Any(text => text.Contains("保存失败")));
                StringAssert.Contains(form.StatusLabel.Text, "已批量删除 1 个沙盒用户");
                StringAssert.Contains(form.StatusLabel.Text, "保存失败");
                CollectionAssert.AreEqual(before, File.ReadAllBytes(form.ConfigurationStorePath));
            });
        }

        [TestMethod]
        public void SuccessfulRenameRefreshesResultAndReportsUnsavedReferences()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                form.InitializeUi();
                string users = Path.Combine(root, "users");
                string oldBox = Path.Combine(users, "BoxA");
                Directory.CreateDirectory(oldBox);
                File.WriteAllText(Path.Combine(oldBox, "keep.txt"), "owned");
                string ini = Path.Combine(root, "Sandboxie.ini");
                File.WriteAllText(ini, "[BoxA]\r\nEnabled=y\r\n");
                form.SandboxieClient.SandboxRoot = users;
                form.SandboxieIniPathCallback = () => ini;
                form.ReloadSandboxieConfigCallback = () => { };
                form.SetSandboxAliasCallback = (name, alias) => true;
                form.SandboxList.ReplaceItemTexts(new[] { "BoxA" }, new int[0]);
                form.SandboxList.CheckItem(0, true);
                form.SandboxAdminState.PinnedSandboxes.Add("BoxA");
                form.SandboxAdminCallbacks.NormalizeSandboxName = name => name;
                form.SandboxAdminCallbacks.GetExistingSandboxNames = () => new[] { "BoxA" };
                form.SandboxAdminCallbacks.GetExistingSandboxDisplayNames = () => new[] { "BoxA" };
                form.ProcessCallbacks.GetSandboxPids = name => new int[0];
                form.ShowBatchRenameRuleCallback = (title, prompt, prefix, count) => new Dictionary<string, object>
                {
                    { "prefix", "Renamed" }, { "start", 1 }, { "digits", 0 }, { "count", 1 }
                };
                int refreshes = 0;
                form.RefreshSandboxListCallback = () => refreshes++;
                var messages = new List<string>();
                form.ShowMessageCallback = (text, title, icon) => messages.Add(text);
                byte[] before = BreakConfiguration(form);
                MigrationRuntimeTestHelpers.Invoke(form, "BatchRenameSandboxes");
                Assert.IsFalse(Directory.Exists(oldBox));
                Assert.AreEqual("owned", File.ReadAllText(Path.Combine(users, "Renamed1", "keep.txt")));
                StringAssert.Contains(File.ReadAllText(ini), "[Renamed1]");
                Assert.AreEqual(1, refreshes);
                Assert.IsTrue(form.SandboxAdminState.PinnedSandboxes.Contains("Renamed1"));
                Assert.IsTrue(messages.Any(text => text.Contains("保存失败")));
                StringAssert.Contains(form.StatusLabel.Text, "保存失败");
                CollectionAssert.AreEqual(before, File.ReadAllBytes(form.ConfigurationStorePath));
            });
        }

        private static byte[] BreakConfiguration(MainForm form)
        {
            Directory.CreateDirectory(form.ConfigurationDirectory);
            File.WriteAllText(form.ConfigurationStorePath, "[config]\r\nvalue=original\r\n[broken\r\n");
            return File.ReadAllBytes(form.ConfigurationStorePath);
        }
    }
}
