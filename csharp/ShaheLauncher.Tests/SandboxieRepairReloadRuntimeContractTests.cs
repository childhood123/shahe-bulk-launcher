using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaheLauncher.App;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Application")]
    [TestCategory("SandboxAdmin")]
    public sealed class SandboxieRepairReloadRuntimeContractTests
    {
        [TestMethod]
        public void NormalConstructionWiresExistingReloadMethod()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                Action reload = form.ReloadSandboxieConfigCallback;

                Assert.IsNotNull(reload, "正常构造的 MainForm 必须接通修复后的 Sandboxie reload。");
                Assert.AreSame(form, reload.Target);
                Assert.AreEqual("ReloadSandboxieConfig", reload.Method.Name);
            });
        }

        [TestMethod]
        public void RepairReloadsOnceOnlyAfterWritingAndKeepsPythonFeedbackOnReloadFailure()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                string iniPath = Path.Combine(root, "Sandboxie.ini");
                File.WriteAllText(
                    iniPath,
                    "[UserSettings_Test]\r\nSbieCtrl_WindowCoords=1\r\nEnabled=y\r\nKeep=ok\r\n",
                    new UTF8Encoding(false));
                var messages = new List<string>();
                Configure(form, iniPath, DialogResult.Yes, messages);
                int reloads = 0;
                form.ReloadSandboxieConfigCallback = () =>
                {
                    reloads++;
                    throw new InvalidOperationException("reload failed");
                };

                MigrationRuntimeTestHelpers.Invoke(form, "RepairSandboxieControlSettings");

                Assert.AreEqual(1, reloads);
                Assert.AreEqual("已修复 Sandboxie Control 用户设置", form.StatusLabel.Text);
                Assert.AreEqual(1, messages.Count);
                StringAssert.StartsWith(messages[0], "修复完成|已清理 1 个 UserSettings 段中的 2 条异常/易冲突记录");
                string repaired = File.ReadAllText(iniPath);
                StringAssert.Contains(repaired, "Keep=ok");
                Assert.IsFalse(repaired.Contains("SbieCtrl_WindowCoords="));
                Assert.IsFalse(repaired.Contains("Enabled="));

                messages.Clear();
                MigrationRuntimeTestHelpers.Invoke(form, "RepairSandboxieControlSettings");

                Assert.AreEqual(1, reloads, "无需写入时不得再次 reload。");
                Assert.AreEqual("未发现需要修复的 Sandboxie Control 用户设置", form.StatusLabel.Text);
                CollectionAssert.AreEqual(
                    new[] { "无需修复|未发现需要清理的 Sandboxie Control 用户设置" },
                    messages.ToArray());
            });
        }

        [TestMethod]
        public void CancellationAndWriteFailureKeepPythonFeedbackWithoutReload()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                string iniPath = Path.Combine(root, "Sandboxie.ini");
                const string original =
                    "[UserSettings_Test]\r\nSbieCtrl_WindowCoords=1\r\nEnabled=y\r\nKeep=ok\r\n";
                File.WriteAllText(iniPath, original, new UTF8Encoding(false));
                var messages = new List<string>();
                int reloads = 0;
                form.ReloadSandboxieConfigCallback = () => reloads++;
                Configure(form, iniPath, DialogResult.No, messages);
                string statusBeforeCancel = form.StatusLabel.Text;

                MigrationRuntimeTestHelpers.Invoke(form, "RepairSandboxieControlSettings");

                Assert.AreEqual(0, reloads);
                Assert.AreEqual(original, File.ReadAllText(iniPath));
                Assert.AreEqual(statusBeforeCancel, form.StatusLabel.Text);
                Assert.AreEqual(0, messages.Count);

                form.ConfirmMessageCallback = (text, title, buttons, icon) => DialogResult.Yes;
                File.SetAttributes(iniPath, File.GetAttributes(iniPath) | FileAttributes.ReadOnly);
                try
                {
                    MigrationRuntimeTestHelpers.Invoke(form, "RepairSandboxieControlSettings");
                }
                finally
                {
                    File.SetAttributes(iniPath, FileAttributes.Normal);
                    // File.Copy preserves the read-only source attribute on the owned recovery backup.
                    string ownedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
                    foreach (string backup in Directory.EnumerateFiles(root,
                        "Sandboxie.ini.沙河批量打开器备份_*.bak", SearchOption.TopDirectoryOnly))
                    {
                        string fullPath = Path.GetFullPath(backup);
                        if (!fullPath.StartsWith(ownedRoot, StringComparison.OrdinalIgnoreCase))
                            throw new IOException("Cleanup escaped fixture");
                        FileAttributes attributes = File.GetAttributes(fullPath);
                        if ((attributes & FileAttributes.ReparsePoint) != 0)
                            throw new IOException("Reparse point in fixture");
                        File.SetAttributes(fullPath, attributes & ~FileAttributes.ReadOnly);
                    }
                }

                Assert.AreEqual(0, reloads, "写入失败时不得 reload。");
                Assert.AreEqual("修复 Sandboxie Control 用户设置失败", form.StatusLabel.Text);
                Assert.AreEqual(1, messages.Count);
                StringAssert.StartsWith(messages[0], "修复失败|修复失败：\n");
            });
        }

        private static void Configure(
            MainForm form,
            string iniPath,
            DialogResult answer,
            IList<string> messages)
        {
            form.InitializeUi();
            form.SandboxieIniPathCallback = () => iniPath;
            form.ConfirmMessageCallback = (text, title, buttons, icon) => answer;
            form.ShowMessageCallback = (text, title, icon) => messages.Add(title + "|" + text);
        }
    }
}
