using System.IO;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaheLauncher.App.UI.Dialogs.Settings;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Speech")]
    public sealed class SpeechSettingsPreservationContractTests
    {
        [DataTestMethod]
        [DataRow(true, true)]
        [DataRow(false, true)]
        [DataRow(true, false)]
        [DataRow(false, false)]
        public void ReaderSettingsConfirmationPreservesSpeechConfig(bool accept, bool existingConfig)
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                Directory.CreateDirectory(form.ConfigurationDirectory);
                string path = Path.Combine(form.ConfigurationDirectory, "SpeechConfig.json");
                const string original = "{\"DefaultEngine\":\"已有引擎\",\"TTSInfos\":{\"已有引擎\":{\"Speed\":81,\"Volume\":37,\"Pitch\":42,\"Name\":\"已有声音\"}},\"custom\":\"保留\"}";
                if (existingConfig) File.WriteAllText(path, original);
                int menus = 0;
                form.SpeechService.Callbacks.CreateMenuBar = () => menus++;
                form.SpeechService.Callbacks.ShowSettings = request =>
                {
                    Assert.AreEqual(existingConfig, File.Exists(path), "打开朗读设置不能创建语音配置");
                    if (existingConfig) Assert.AreEqual(original, File.ReadAllText(path));
                    using (var dialog = new SpeechSettingsDialog(form, request.SpeechConfig,
                        request.SaveSpeechConfig, request.ReaderIniSettings, request.SaveReaderIniSettings,
                        form.SpeechService.DetectScreenReaderAndMatchEngine, request.InterfaceDllChoices,
                        request.SelectedInterfaceDll, request.SaveInterfaceDll, request.AssistHints,
                        request.SaveAssistHints))
                    {
                        dialog.InterfaceChoice.SelectedIndex = 1;
                        dialog.AssistEnabledCheckBox.Checked = false;
                        if (accept)
                        {
                            Assert.IsTrue(dialog.ConfirmSettings());
                            Assert.AreEqual(DialogResult.OK, dialog.DialogResult);
                        }
                        else
                        {
                            dialog.DialogResult = DialogResult.Cancel;
                            dialog.Close();
                        }
                        return accept;
                    }
                };
                Assert.AreEqual(accept, form.SpeechService.OnSpeechSettings());
                Assert.AreEqual(existingConfig, File.Exists(path), "读屏设置确定不能创建空语音配置");
                if (existingConfig) Assert.AreEqual(original, File.ReadAllText(path), "已有语音配置必须逐字保留");
                Assert.AreEqual(accept ? 1 : 0, menus);
                Assert.AreEqual(accept ? "nvda" : "", form.SpeechService.SpeechInterfaceDll);
                if (accept) Assert.AreEqual(false, form.SpeechService.SpeechAssistHints["enabled"]);
            });
        }
    }
}
