using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaheLauncher.App.UI.Dialogs.Common;
using ShaheLauncher.App.UI.Dialogs.Settings;
using ShaheLauncher.Core.Configuration;
using ShaheLauncher.Core.Settings;

namespace ShaheLauncher.Tests
{
    [TestClass, TestCategory("Common"), TestCategory("SettingsService")]
    public sealed class ConfigSaveReliabilityContractTests
    {
        [TestMethod]
        public void ConcurrentStoreInstancesRetainEveryAcknowledgedKey()
        {
            WithStore((path, store) =>
            {
                Assert.IsTrue(store.SetText("original", "保留", false));
                int failures = 0;
                using (var ready = new Barrier(8))
                {
                    Task[] writers = Enumerable.Range(0, 8).Select(writer => Task.Factory.StartNew(() =>
                    {
                        var ownStore = new ConfigStore(path, "test", () => 0);
                        ready.SignalAndWait();
                        for (int value = 0; value < 32; value++)
                            if (!ownStore.SetText("writer" + writer, value.ToString(), false)) Interlocked.Increment(ref failures);
                    }, TaskCreationOptions.LongRunning)).ToArray();
                    Assert.IsTrue(Task.WaitAll(writers, 15000));
                }
                Assert.AreEqual(0, failures, "Concurrent independent keys must all be acknowledged.");
                for (int writer = 0; writer < 8; writer++)
                    Assert.AreEqual("31", store.GetText("writer" + writer, "missing"));
                Assert.AreEqual("保留", store.GetText("original", "missing"));
            });
        }

        [DataTestMethod]
        [DataRow("save")]
        [DataRow("apply")]
        [DataRow("reset")]
        public void UnwritableSettingsNeverReportSuccessfulSave(string operation)
        {
            WithStore((path, store) =>
            {
                File.WriteAllText(path, "[settings]\r\nvalue=original\r\n[broken\r\n");
                byte[] original = File.ReadAllBytes(path);
                var service = new LauncherSettingsService(store, "", Path.GetDirectoryName(path));
                var state = new LauncherSettingsState { Settings = service.CreateDefaultSettings() };
                var messages = new List<string>();
                int autoRunWrites = 0;
                var callbacks = new LauncherSettingsCallbacks
                {
                    SetStatus = messages.Add, ApplyAutoRun = () => autoRunWrites++
                };
                Assert.ThrowsException<SettingsSaveException>(() =>
                {
                    if (operation == "save") service.SaveSettings(state);
                    else if (operation == "apply") service.ApplySettings(state, service.CreateDefaultSettings(), callbacks);
                    else service.ResetSettings(state, callbacks);
                });
                Assert.IsFalse(messages.Contains(LauncherSettingsService.SettingsSavedStatus));
                Assert.IsFalse(messages.Contains(LauncherSettingsService.ResetCompletedStatus));
                Assert.AreEqual(0, autoRunWrites);
                CollectionAssert.AreEqual(original, File.ReadAllBytes(path));
            });
        }

        [TestMethod]
        public void SettingsDialogRemainsOpenAndReportsWriteFailure()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                var messages = new List<string>();
                Action<string, string, MessageBoxIcon> old = DialogMessageService.Handler;
                DialogMessageService.Handler = (text, title, icon) => messages.Add(text);
                try
                {
                    using (var dialog = new SettingsDialog(form, form.SettingsService.CreateDefaultSettings(),
                        values => { throw new IOException("isolated write denied"); }, form.SettingsService.CreateDefaultSettings()))
                    {
                        Assert.IsFalse(dialog.SaveSettings());
                        Assert.AreEqual(DialogResult.None, dialog.DialogResult);
                        Assert.IsFalse(dialog.IsDisposed);
                        Assert.IsTrue(messages.Any(text => text.Contains("保存失败")));
                    }
                }
                finally { DialogMessageService.Handler = old; }
            });
        }

        [TestMethod]
        public void MenuSettingFailureIsReportedWithoutSavedStatusOrUnhandledException()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                MigrationRuntimeTestHelpers.Invoke(form, "InitializeUi");
                Directory.CreateDirectory(form.ConfigurationDirectory);
                File.WriteAllText(form.ConfigurationStorePath, "[settings]\r\nvalue=old\r\n[broken\r\n");
                byte[] before = File.ReadAllBytes(form.ConfigurationStorePath);
                var messages = new List<string>();
                form.ShowMessageCallback = (text, title, icon) => messages.Add(title + ":" + text);
                MigrationRuntimeTestHelpers.Invoke(form, "DispatchApplicationAction", "toggle_remember_volume");
                Assert.AreEqual(1, messages.Count);
                StringAssert.Contains(messages[0], "保存失败");
                StringAssert.Contains(form.StatusLabel.Text, "保存失败");
                CollectionAssert.AreEqual(before, File.ReadAllBytes(form.ConfigurationStorePath));
            });
        }

        private static void WithStore(Action<string, ConfigStore> action)
        {
            string root = Path.Combine(Path.GetTempPath(), "shahe-save-reliability-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            try
            {
                string path = Path.Combine(root, "config.ini");
                action(path, new ConfigStore(path, "test", () => 0));
            }
            finally { Directory.Delete(root, true); }
        }
    }
}
