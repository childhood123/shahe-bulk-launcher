using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaheLauncher.Core.Configuration;
using ShaheLauncher.Core.Settings;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Common")]
    public sealed class ConfigurationFailureSafetyContractTests
    {
        private const string Malformed = "[settings]\r\nmax_history_items=17\r\nsandbox_groups={\"important\":[\"BoxA\"]}\r\n[unfinished\r\n";

        [TestMethod]
        public void StartupSettingsLoadPreservesMalformedConfigurationBytes()
        {
            WithStore((path, store) =>
            {
                File.WriteAllText(path, Malformed, new UTF8Encoding(false));
                byte[] before = File.ReadAllBytes(path);
                var service = new LauncherSettingsService(store, string.Empty, Path.GetDirectoryName(path));

                IDictionary<string, object> settings = service.LoadSettings();

                Assert.AreEqual(10, Convert.ToInt32(settings["max_history_items"]));
                CollectionAssert.AreEqual(before, File.ReadAllBytes(path), "Loading defaults must not overwrite unreadable user settings.");
                Assert.AreEqual(1, Directory.GetFiles(Path.GetDirectoryName(path)).Length);
            });
        }

        [DataTestMethod]
        [DataRow("plain")]
        [DataRow("secret")]
        [DataRow("settings")]
        public void EveryWriteRejectsMalformedConfigurationWithoutChangingBytes(string operation)
        {
            WithStore((path, store) =>
            {
                File.WriteAllText(path, Malformed, new UTF8Encoding(false));
                byte[] before = File.ReadAllBytes(path);
                bool saved = operation == "plain"
                    ? store.SetText("value", "new-value", false)
                    : operation == "secret"
                        ? store.SetSecretText("value", "new-value")
                        : store.SetJson("settings_json", new Dictionary<string, object> { { "max_history_items", 39 } });

                Assert.IsFalse(saved, "The existing INI cannot be read; writing must fail, not reset it.");
                CollectionAssert.AreEqual(before, File.ReadAllBytes(path));
                Assert.IsFalse(File.Exists(path + ".tmp"), "A failed read must not reach the save stage.");
            });
        }

        [TestMethod]
        public void TemporarilyUnreadableConfigurationIsNotReplacedAndRecoversAfterUnlock()
        {
            WithStore((path, store) =>
            {
                File.WriteAllText(path, "[config]\r\nvalue=original\r\n", new UTF8Encoding(false));
                byte[] before = File.ReadAllBytes(path);
                bool saved;
                // Deny reading but allow replacement: this catches fallback-to-empty writes.
                using (FileStream held = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Delete))
                {
                    Assert.AreEqual("fallback", store.GetText("value", "fallback"));
                    saved = store.SetText("value", "changed", false);
                }

                Assert.IsFalse(saved, "A sharing violation must not be treated as a missing file.");
                CollectionAssert.AreEqual(before, File.ReadAllBytes(path));
                Assert.IsTrue(store.SetText("value", "changed", false));
                Assert.AreEqual("changed", store.GetText("value", string.Empty));
            });
        }

        [TestMethod]
        public void RepairedConfigurationCanBeSavedByTheSameStoreInstance()
        {
            WithStore((path, store) =>
            {
                File.WriteAllText(path, Malformed, new UTF8Encoding(false));
                Assert.IsNull(store.GetJson("settings_json", null));
                Assert.IsFalse(store.SetText("value", "changed", false));

                File.WriteAllText(path, "[config]\r\nvalue=original\r\n[settings]\r\nmax_history_items=17\r\n", new UTF8Encoding(false));

                Assert.IsTrue(store.SetJson("settings_json", new Dictionary<string, object> { { "max_history_items", 39 } }));
                Assert.AreEqual("original", store.GetText("value", string.Empty));
                var settings = (IDictionary<string, object>)store.GetJson("settings_json", null);
                Assert.AreEqual(39, Convert.ToInt32(settings["max_history_items"]));
            });
        }

        private static void WithStore(Action<string, ConfigStore> action)
        {
            string root = Path.Combine(Path.GetTempPath(), "shahe-config-safety-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            try
            {
                string path = Path.Combine(root, "shahe_config.ini");
                action(path, new ConfigStore(path, "test-fingerprint", () => 0L));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }
    }
}
