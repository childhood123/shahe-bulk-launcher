using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaheLauncher.App;
using ShaheLauncher.Core.Updates;
using Newtonsoft.Json.Linq;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Updates")]
    public sealed class EmbeddedUpdateContractTests
    {
        [TestMethod]
        public void PreparesMainExecutableCopyWithoutStandaloneUpdater()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                string exe = Path.Combine(root, "沙河批量打开器.exe");
                File.WriteAllText(exe, "main-with-update-mode");
                File.WriteAllText(exe + ".config", "runtime-config");
                File.WriteAllText(Path.Combine(root, "ShaheLauncher.Core.dll"), "core");
                using (var service = new UpdateService(root, true, exe, null, new UpdateServiceCallbacks()))
                {
                    var prepared = service.PrepareUpdaterCommand(new UpdatePackageInfo { Url = "https://example.invalid/update.zip", Size = 1, Sha256 = "abc" }, "9.0.0");
                    Assert.AreEqual(string.Empty, prepared.Error);
                    Assert.AreEqual(Path.GetFileName(exe), Path.GetFileName(prepared.Command[0]));
                    Assert.AreEqual("--install-update", prepared.Command[1]);
                    Assert.AreEqual(File.ReadAllText(exe), File.ReadAllText(prepared.Command[0]));
                    Assert.AreEqual("runtime-config", File.ReadAllText(prepared.Command[0] + ".config"));
                    Assert.IsFalse(File.Exists(Path.Combine(Path.GetDirectoryName(prepared.Command[0]), "updater.exe")));
                }
            });
        }

        [TestMethod]
        public void InstallsPackageWithoutUpdaterAndKeepsConfigurations()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                string source = Path.Combine(root, "payload");
                string app = Path.Combine(root, "installed");
                Directory.CreateDirectory(Path.Combine(source, "lib"));
                Directory.CreateDirectory(Path.Combine(app, "Configurations"));
                File.WriteAllText(Path.Combine(source, "main.exe"), "new-main");
                File.WriteAllText(Path.Combine(source, "lib", "new.dll"), "new-runtime");
                File.WriteAllText(Path.Combine(app, "main.exe"), "old-main");
                File.WriteAllText(Path.Combine(app, "Configurations", "user.ini"), "preserve");
                Assert.IsTrue(UpdateRunner.InstallPackage(source, app, Path.Combine(root, "backup"), "main.exe"), UpdateRunner.LastError);
                Assert.AreEqual("new-runtime", File.ReadAllText(Path.Combine(app, "lib", "new.dll")));
                Assert.AreEqual("preserve", File.ReadAllText(Path.Combine(app, "Configurations", "user.ini")));
            });
        }

        [DataTestMethod]
        [DataRow("立即更新", DialogResult.Yes)]
        [DataRow("稍后更新", DialogResult.No)]
        [DataRow("忽略此版本", DialogResult.Ignore)]
        public void UpdateDialogStartsOnReadableNotesAndOffersThreeExplicitActions(string button, DialogResult expected)
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                Type type = typeof(MainForm).Assembly.GetType("ShaheLauncher.App.UI.Dialogs.Common.UpdateAvailableDialog");
                Assert.IsNotNull(type, "The accessible update-choice dialog must exist.");
                var result = Available();
                Set(result, "ReleaseNotes", "修复保存反馈\r\n新增运行库");
                using (var dialog = (Form)Activator.CreateInstance(type, result))
                {
                    dialog.Show(form);
                    var notes = Descendants(dialog).OfType<TextBox>().Single();
                    Assert.IsTrue(notes.ReadOnly && notes.Multiline);
                    Assert.AreEqual("更新内容", notes.AccessibleName);
                    StringAssert.Contains(notes.Text, "修复保存反馈");
                    Assert.AreSame(notes, dialog.ActiveControl);
                    Assert.IsNull(dialog.AcceptButton, "Enter while reading notes must not install updates.");
                    var actions = Descendants(dialog).OfType<Button>().OrderBy(item => item.TabIndex).ToArray();
                    CollectionAssert.AreEqual(new[] { "立即更新", "稍后更新", "忽略此版本" }, actions.Select(item => item.Text).ToArray());
                    Assert.AreEqual(0, notes.TabIndex);
                    CollectionAssert.AreEqual(new[] { 1, 2, 3 }, actions.Select(item => item.TabIndex).ToArray());
                    Assert.AreEqual(DialogResult.None, dialog.DialogResult);
                    actions.Single(item => item.Text == button).PerformClick();
                    Assert.AreEqual(expected, dialog.DialogResult);
                }
            });
        }

        [TestMethod]
        public void IgnorePersistsAndSkipsOnlyAutomaticChecksForThatVersion()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                form.InitializeUi();
                form.Show();
                int prompts = 0;
                SetPrompt(form, update => { prompts++; return prompts == 1 ? DialogResult.Ignore : DialogResult.No; });
                var result = Available();
                form.ShowUpdateNotice(result);
                var preference = (JObject)form.ConfigStore.GetJson("update_preference", null);
                Assert.AreEqual("9.0.0", (string)preference["version"]);
                Assert.AreEqual("ignore", (string)preference["action"]);
                Assert.IsFalse(form.UpdateNoticePanel.Visible);
                form.ShowUpdateNotice(result);
                Assert.AreEqual(1, prompts);
                Set(result, "IsManualCheck", true);
                form.ShowUpdateNotice(result);
                Assert.AreEqual(2, prompts, "A manual check must still allow the ignored update.");
                Assert.IsTrue(form.UpdateNoticePanel.Visible);
                Assert.IsFalse(form.ForceExitRequested);
            });
        }

        [TestMethod]
        public void IgnoreSaveFailureIsReportedAndDoesNotHideAvailableUpdate()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                form.InitializeUi(); form.Show();
                Assert.IsTrue(form.ConfigStore.SetText("sentinel", "keep", false));
                var messages = new List<string>();
                form.ShowMessageCallback = (text, title, icon) => messages.Add(text);
                SetPrompt(form, result => DialogResult.Ignore);
                using (File.Open(form.ConfigurationStorePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    form.ShowUpdateNotice(Available());
                Assert.IsNull(form.ConfigStore.GetJson("update_preference", null));
                Assert.AreEqual(1, messages.Count);
                StringAssert.Contains(messages[0], "保存失败");
                Assert.IsTrue(form.UpdateNoticePanel.Visible);
            });
        }

        [TestMethod]
        public void LaterPersistsForNextStartupWithoutStartingDuringThisSession()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                form.InitializeUi(); form.Show();
                SetPrompt(form, result => DialogResult.No);
                form.ShowUpdateNotice(Available());
                var preference = (JObject)form.ConfigStore.GetJson("update_preference", null);
                Assert.AreEqual("later", (string)preference["action"]);
                Assert.AreEqual("9.0.0", (string)preference["version"]);
                Assert.IsFalse(form.ForceExitRequested);
                Assert.IsTrue(form.UpdateNoticePanel.Visible);
            });
        }

        [TestMethod]
        public void NextStartupRunsDeferredExactVersionButPromptsForDifferentVersion()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                Assert.IsTrue(form.ConfigStore.SetJson("update_preference", new Dictionary<string, string>
                { { "version", "9.0.0" }, { "action", "later" } }));
                form.InitializeUi();
                int prompts = 0;
                var messages = new List<string>();
                SetPrompt(form, result => { prompts++; return DialogResult.Cancel; });
                form.ShowMessageCallback = (text, title, icon) => messages.Add(text);
                form.ShowUpdateNotice(Available());
                Assert.AreEqual(0, prompts, "The previously approved version must proceed at startup.");
                Assert.AreEqual(1, messages.Count, "The non-packaged test app must reach the installer preparation guard.");
                StringAssert.Contains(messages[0], "自动更新只能在打包后的发布版中使用");
                var newer = Available(); newer.LatestVersion = "10.0.0";
                form.ShowUpdateNotice(newer);
                Assert.AreEqual(1, prompts, "A changed target requires reading the new release notes.");
            });
        }

        private static UpdateDisplayResult Available()
        {
            return new UpdateDisplayResult { LatestVersion = "9.0.0", HasUpdate = true, ShouldPrompt = true,
                ShouldShowNotice = true, Package = new UpdatePackageInfo { Kind = "full", Url = "https://example.invalid/update.zip", Size = 1, Sha256 = "abc" } };
        }
        private static void SetPrompt(MainForm form, Func<UpdateDisplayResult, DialogResult> callback)
        {
            Set(form, "ShowUpdateDialogCallback", callback);
        }
        private static void Set(object obj, string name, object value)
        {
            var property = obj.GetType().GetProperty(name);
            Assert.IsNotNull(property, "Missing update behavior: " + name);
            property.SetValue(obj, value, null);
        }
        private static IEnumerable<Control> Descendants(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                yield return child;
                foreach (var nested in Descendants(child)) yield return nested;
            }
        }
    }
}
