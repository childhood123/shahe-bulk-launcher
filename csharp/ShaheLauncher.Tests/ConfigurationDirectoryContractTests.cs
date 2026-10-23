using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaheLauncher.App;
using ShaheLauncher.Core.Runtime;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("SettingsDialogs")]
    public sealed class ConfigurationDirectoryContractTests
    {
        [TestMethod]
        public void EmptyPackagedDirectoryReceivesPreviousConfigurationWithoutChangingBytes()
        {
            MigrationRuntimeTestHelpers.WithForm((root, unused) =>
            {
                string app = Path.Combine(root, "upgrade");
                string old = Path.Combine(app, "Configuration");
                string current = Path.Combine(app, "Configurations");
                Directory.CreateDirectory(old);
                Directory.CreateDirectory(current);
                string settings = "[settings]\r\nmax_history_items=39\r\n";
                File.WriteAllText(Path.Combine(old, "shahe_config.ini"), settings);
                File.WriteAllText(Path.Combine(old, "SpeechConfig.json"), "{}");
                using (var form = CreateForm(app))
                {
                    Assert.AreEqual(current, form.ConfigurationDirectory);
                    Assert.AreEqual(settings, File.ReadAllText(form.ConfigurationStorePath));
                    Assert.IsTrue(File.Exists(Path.Combine(current, "SpeechConfig.json")));
                    Assert.IsFalse(Directory.Exists(old), "The old directory must be migrated only once.");
                    Assert.AreEqual(39, Convert.ToInt32(form.SettingsService.LoadSettings()["max_history_items"]));
                }
            });
        }

        [TestMethod]
        public void FreshStartupCreatesConfigurationAndIgnoresScatteredRootSettings()
        {
            MigrationRuntimeTestHelpers.WithForm((root, unused) =>
            {
                string app = Path.Combine(root, "fresh");
                Directory.CreateDirectory(app);
                File.WriteAllText(Path.Combine(app, "app_config.dat"), "{\"max_history_items\":777}");
                File.WriteAllText(Path.Combine(app, "SpeechConfig.json"), "{\"root_only\":true}");
                File.WriteAllText(Path.Combine(app, "ZDSRAPI.ini"), "[settings]\r\nchannelName=root-only\r\n");
                using (var form = CreateForm(app))
                {
                    Assert.AreEqual(Path.Combine(app, "Configurations"), form.ConfigurationDirectory);
                    Assert.IsTrue(Directory.Exists(form.ConfigurationDirectory));
                    Assert.AreEqual(10, Convert.ToInt32(form.SettingsService.LoadSettings()["max_history_items"]));
                    Assert.IsTrue(File.Exists(form.ConfigurationStorePath), "First load must create the user's configuration.");
                    Assert.AreEqual(Path.Combine(form.ConfigurationDirectory, "ZDSRAPI.ini"), form.SpeechService.GetReaderIniPath("ZDSRAPI.ini", false));
                    Assert.IsFalse(form.SpeechService.LoadSpeechConfig().ContainsKey("root_only"));
                    Assert.IsTrue(File.Exists(Path.Combine(form.ConfigurationDirectory, "SpeechConfig.json")));
                    Assert.AreEqual("{\"root_only\":true}", File.ReadAllText(Path.Combine(app, "SpeechConfig.json")));
                }
            });
        }

        [TestMethod]
        public void ExistingNewConfigurationTakesPriorityOverPreviousDirectory()
        {
            MigrationRuntimeTestHelpers.WithForm((root, unused) =>
            {
                string app = Path.Combine(root, "existing");
                Directory.CreateDirectory(Path.Combine(app, "Configuration"));
                Directory.CreateDirectory(Path.Combine(app, "Configurations"));
                File.WriteAllText(Path.Combine(app, "Configuration", "shahe_config.ini"), "[settings]\nmax_history_items=20\n");
                File.WriteAllText(Path.Combine(app, "Configurations", "shahe_config.ini"), "[settings]\nmax_history_items=39\n");
                using (var form = CreateForm(app))
                {
                    Assert.AreEqual(39, Convert.ToInt32(form.SettingsService.LoadSettings()["max_history_items"]));
                    Assert.IsTrue(File.Exists(Path.Combine(app, "Configuration", "shahe_config.ini")), "Existing old data must not be deleted when the new directory already has settings.");
                }
            });
        }

        private static MainForm CreateForm(string directory)
        {
            string executable = Path.Combine(directory, "沙河批量打开器.exe");
            return new MainForm(new LauncherRuntime(false, executable, executable, () => DateTime.Now));
        }
    }
}
