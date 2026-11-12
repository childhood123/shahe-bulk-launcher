using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaheLauncher.Core.Backup;
using ShaheLauncher.Core.SandboxAdmin;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("IniEncodingAndReadonly")]
    public sealed class IniEncodingAndReadonlyContractTests
    {
        private const string Preamble = "; 保留目标文件注释\r\n";
        private const string Global = "[GlobalSettings]\r\nKeep=全局中文\r\n";
        private const string Other = "[Other]\r\nKeep=未选中的中文\r\n";
        private const string OldBox = "[BoxA]\r\nValue=旧配置中文\r\n";
        private const string NewBox = "[BoxA]\r\nValue=新配置中文\r\n";

        [DataTestMethod]
        [DataRow("utf8", "utf8")]
        [DataRow("utf8-bom", "utf8-bom")]
        [DataRow("utf16", "utf16")]
        [DataRow("gbk", "gbk")]
        [DataRow("utf8", "gbk")]
        [DataRow("utf8-bom", "utf16")]
        [DataRow("utf16", "utf8")]
        [DataRow("gbk", "utf8-bom")]
        public void MergeUsesTargetEncodingAndBomWhileDecodingBackupIndependently(
            string targetEncoding, string backupEncoding)
        {
            using (var files = new OwnedFiles())
            {
                string target = files.WriteIni("target.ini", Preamble + Global + OldBox + Other, targetEncoding);
                string backup = files.WriteIni("backup.ini", NewBox + "[Other]\r\nKeep=不得导入\r\n", backupEncoding);
                byte[] originalBackup = File.ReadAllBytes(backup);

                new BackupRestoreService().MergeIniSections(backup, target, new[] { "BoxA" });

                AssertIniBytes(target, Preamble + Global + NewBox + Other, targetEncoding);
                CollectionAssert.AreEqual(originalBackup, File.ReadAllBytes(backup), "The backup is input only.");
            }
        }

        [DataTestMethod]
        [DataRow("utf8")]
        [DataRow("utf8-bom")]
        [DataRow("utf16")]
        [DataRow("gbk")]
        public void AdminReadWriteRoundTripPreservesChineseAndExactBom(string encodingName)
        {
            using (var files = new OwnedFiles())
            {
                const string original = Preamble + Global + OldBox + Other;
                string path = files.WriteIni("Sandboxie.ini", original, encodingName);
                byte[] before = File.ReadAllBytes(path);
                var service = new SandboxAdminService();

                Tuple<string, string> loaded = service.ReadTextWithEncoding(path);

                Assert.AreEqual(original, loaded.Item1, "Decode Chinese without replacement characters or a leading BOM character.");
                service.WriteTextWithEncoding(path, loaded.Item1, loaded.Item2);
                CollectionAssert.AreEqual(before, File.ReadAllBytes(path), "Round trip must retain the original encoding and BOM.");
            }
        }

        [DataTestMethod]
        [DataRow("utf8")]
        [DataRow("utf8-bom")]
        [DataRow("utf16")]
        [DataRow("gbk")]
        public void AdminSectionChangesPreserveEncodingChineseAndReservedSections(string encodingName)
        {
            using (var files = new OwnedFiles())
            {
                const string userSettings = "[UserSettings_Test]\r\nKeep=用户设置中文\r\n";
                const string removed = "[BoxRemove]\r\nValue=待删除中文\r\n";
                string original = Preamble + Global + userSettings + OldBox + removed + Other;
                string path = files.WriteIni("Sandboxie.ini", original, encodingName);

                Tuple<bool, string> result = new SandboxAdminService().ApplyIniSectionChanges(
                    path,
                    new[] { "BoxAdded", "GlobalSettings", "UserSettings_Test" },
                    new[] { "BoxRemove", "GlobalSettings", "UserSettings_Test" },
                    new Dictionary<string, string>
                    {
                        { "BoxA", "BoxRenamed" },
                        { "GlobalSettings", "StolenGlobal" },
                        { "UserSettings_Test", "StolenUserSettings" }
                    });

                Assert.IsTrue(result.Item1, result.Item2);
                AssertIniBytes(path, Preamble + Global + userSettings
                    + "[BoxRenamed]\r\nValue=旧配置中文\r\n" + Other
                    + "[BoxAdded]\r\nEnabled=y\r\nConfigLevel=9\r\n\r\n", encodingName);
                AssertBackupBytes(path, Encode(original, encodingName));
            }
        }

        [DataTestMethod]
        [DataRow("utf8")]
        [DataRow("utf8-bom")]
        [DataRow("utf16")]
        [DataRow("gbk")]
        public void AdminRepairPreservesEncodingAndUnrelatedChineseBeforeReload(string encodingName)
        {
            using (var files = new OwnedFiles())
            {
                const string dirty = "[UserSettings_Test]\r\nSbieCtrl_WindowCoords=1\r\nEnabled=y\r\nKeep=用户中文\r\n";
                const string clean = "[UserSettings_Test]\r\nKeep=用户中文\r\n";
                const string original = Preamble + Global + dirty + OldBox + Other;
                const string expected = Preamble + Global + clean + OldBox + Other;
                string path = files.WriteIni("Sandboxie.ini", original, encodingName);
                var reloadedBytes = new List<byte[]>();

                Tuple<bool, string> result = new SandboxAdminService().RepairSandboxieControlUserSettings(
                    path, () => { reloadedBytes.Add(File.ReadAllBytes(path)); return true; });

                Assert.IsTrue(result.Item1, result.Item2);
                AssertIniBytes(path, expected, encodingName);
                Assert.AreEqual(1, reloadedBytes.Count, "Reload exactly once after writing the repaired INI.");
                CollectionAssert.AreEqual(Encode(expected, encodingName), reloadedBytes[0]);
                AssertBackupBytes(path, Encode(original, encodingName));
            }
        }

        [DataTestMethod]
        [DataRow("utf8", true)]
        [DataRow("utf8-bom", true)]
        [DataRow("utf16", true)]
        [DataRow("gbk", true)]
        [DataRow("utf8", false)]
        [DataRow("utf8-bom", false)]
        [DataRow("utf16", false)]
        [DataRow("gbk", false)]
        public void TransactionalRestorePreservesEncodingReadonlyNonTargetsAndServiceState(
            string encodingName, bool initiallyRunning)
        {
            using (var fixture = new RestoreFixture(encodingName, initiallyRunning))
            {
                BackupRestoreResult result = fixture.Restore("BoxA");

                Assert.IsTrue(result.Success, result.Error);
                Assert.AreEqual(string.Empty, result.Error);
                AssertIniBytes(fixture.TargetIni, Preamble + Global + NewBox + Other, encodingName);
                Assert.AreEqual("还原的新数据", File.ReadAllText(fixture.NewTarget));
                Assert.IsFalse(File.Exists(fixture.OldTarget));
                Assert.AreEqual(initiallyRunning, fixture.Running);
                CollectionAssert.AreEqual(initiallyRunning ? new[] { "stop", "reload", "restart" } : new string[0],
                    fixture.ServiceCalls.ToArray());
                fixture.AssertNonTargetUnchanged();
                fixture.AssertNoRecoveryCopies();
            }
        }

        [DataTestMethod]
        [DataRow("utf8")]
        [DataRow("utf8-bom")]
        [DataRow("utf16")]
        [DataRow("gbk")]
        public void ReloadFailureRollsBackOriginalIniBytesAndReadonlyDataBeforeResumingService(string encodingName)
        {
            using (var fixture = new RestoreFixture(encodingName, true))
            {
                byte[] originalIni = File.ReadAllBytes(fixture.TargetIni);
                byte[] originalData = File.ReadAllBytes(fixture.OldTarget);
                FileAttributes originalAttributes = File.GetAttributes(fixture.OldTarget);
                int reloads = 0;
                fixture.Callbacks.ReloadSandboxieConfig = () =>
                {
                    fixture.ServiceCalls.Add("reload");
                    if (++reloads == 1)
                    {
                        fixture.Running = true;
                        throw new IOException("injected reload failure");
                    }
                };

                BackupRestoreResult result = fixture.Restore("BoxA");

                Assert.IsFalse(result.Success);
                StringAssert.Contains(result.Error, "injected reload failure");
                CollectionAssert.AreEqual(originalIni, File.ReadAllBytes(fixture.TargetIni));
                CollectionAssert.AreEqual(originalData, File.ReadAllBytes(fixture.OldTarget));
                Assert.AreEqual(originalAttributes, File.GetAttributes(fixture.OldTarget));
                Assert.IsFalse(File.Exists(fixture.NewTarget));
                Assert.IsTrue(fixture.Running);
                CollectionAssert.AreEqual(new[] { "stop", "reload", "stop", "reload", "restart" },
                    fixture.ServiceCalls.ToArray());
                CollectionAssert.DoesNotContain(fixture.Statuses, BackupRestoreService.RestoreCompletedStatus);
                CollectionAssert.DoesNotContain(fixture.Progress, 100);
                fixture.AssertNonTargetUnchanged();
                fixture.AssertNoRecoveryCopies();
            }
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void ReadonlyTargetIniRestorePreservesOriginalStateAndCleansOwnedTemporaryFiles(bool initiallyRunning)
        {
            using (var fixture = new RestoreFixture("utf8-bom", initiallyRunning))
            {
                File.SetAttributes(fixture.TargetIni, File.GetAttributes(fixture.TargetIni) | FileAttributes.ReadOnly);
                byte[] originalIni = File.ReadAllBytes(fixture.TargetIni);
                FileAttributes originalIniAttributes = File.GetAttributes(fixture.TargetIni);
                byte[] originalData = File.ReadAllBytes(fixture.OldTarget);
                FileAttributes originalDataAttributes = File.GetAttributes(fixture.OldTarget);

                BackupRestoreResult result = fixture.Restore("BoxA");

                Assert.IsFalse(result.Success, "A read-only target INI must still reject restore.");
                Assert.IsFalse(string.IsNullOrWhiteSpace(result.Error));
                Assert.AreEqual(BackupRestoreService.RestoreFailedStatus, result.Status);
                CollectionAssert.AreEqual(originalIni, File.ReadAllBytes(fixture.TargetIni));
                Assert.AreEqual(originalIniAttributes, File.GetAttributes(fixture.TargetIni),
                    "Cleanup must not clear the target INI's read-only attribute.");
                CollectionAssert.AreEqual(originalData, File.ReadAllBytes(fixture.OldTarget));
                Assert.AreEqual(originalDataAttributes, File.GetAttributes(fixture.OldTarget));
                Assert.IsFalse(File.Exists(fixture.NewTarget));
                Assert.AreEqual(initiallyRunning, fixture.Running);
                Assert.AreEqual(0, fixture.ServiceCalls.Count, "Preparation failure must not stop, reload or restart services.");
                CollectionAssert.AreEqual(new[] { BackupRestoreService.RestoreFailedStatus }, fixture.Statuses.ToArray());
                CollectionAssert.DoesNotContain(fixture.Progress, 100);
                fixture.AssertNonTargetUnchanged();
                fixture.AssertNoRecoveryCopies();
            }
        }

        [TestMethod]
        public void RestoreRejectsTraversalBeforeChangingReadonlyFilesOrServiceState()
        {
            using (var fixture = new RestoreFixture("gbk", true))
            {
                byte[] originalIni = File.ReadAllBytes(fixture.TargetIni);
                byte[] originalData = File.ReadAllBytes(fixture.OldTarget);
                FileAttributes originalAttributes = File.GetAttributes(fixture.OldTarget);

                BackupRestoreResult result = fixture.Restore("../Other");

                Assert.IsFalse(result.Success);
                StringAssert.Contains(result.Error, "无效的备份沙盒用户");
                CollectionAssert.AreEqual(originalIni, File.ReadAllBytes(fixture.TargetIni));
                CollectionAssert.AreEqual(originalData, File.ReadAllBytes(fixture.OldTarget));
                Assert.AreEqual(originalAttributes, File.GetAttributes(fixture.OldTarget));
                Assert.IsTrue(fixture.Running);
                Assert.AreEqual(0, fixture.ServiceCalls.Count);
                fixture.AssertNonTargetUnchanged();
                fixture.AssertNoRecoveryCopies();
            }
        }

        [TestMethod]
        public void AdminDeleteRemovesNestedReadonlyFilesWithoutTouchingSibling()
        {
            using (var files = new OwnedFiles())
            {
                string target = files.PathFor("BoxA");
                string file = files.WriteIni("BoxA/profile/readonly.ini", "中文数据", "gbk");
                string sibling = files.WriteIni("Other/readonly.ini", "非目标中文", "utf16");
                File.SetAttributes(file, File.GetAttributes(file) | FileAttributes.ReadOnly);
                File.SetAttributes(sibling, File.GetAttributes(sibling) | FileAttributes.ReadOnly);
                File.SetAttributes(Path.GetDirectoryName(file), File.GetAttributes(Path.GetDirectoryName(file)) | FileAttributes.ReadOnly);
                byte[] siblingBytes = File.ReadAllBytes(sibling);
                FileAttributes siblingAttributes = File.GetAttributes(sibling);

                bool deleted = new SandboxAdminService().SafeDeleteDirectory(target);

                Assert.IsTrue(deleted, "Python retries directory deletion after clearing read-only attributes.");
                Assert.IsFalse(Directory.Exists(target));
                CollectionAssert.AreEqual(siblingBytes, File.ReadAllBytes(sibling));
                Assert.AreEqual(siblingAttributes, File.GetAttributes(sibling));
            }
        }

        [TestMethod]
        public void AdminDeleteReportsLockedReadonlyFileWithoutRemovingItsData()
        {
            using (var files = new OwnedFiles())
            {
                string file = files.WriteIni("BoxA/locked.ini", "被占用的数据", "gbk");
                byte[] before = File.ReadAllBytes(file);
                File.SetAttributes(file, File.GetAttributes(file) | FileAttributes.ReadOnly);

                using (FileStream held = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    Assert.IsFalse(new SandboxAdminService().SafeDeleteDirectory(files.PathFor("BoxA")),
                        "Clearing read-only attributes must not turn a sharing violation into reported success.");
                    CollectionAssert.AreEqual(before, File.ReadAllBytes(file));
                }
            }
        }

        [DataTestMethod]
        [DataRow("merge")]
        [DataRow("admin-change")]
        [DataRow("admin-repair")]
        public void ReadonlyIniWriteIsRejectedWithoutChangingBytesOrReloading(string operation)
        {
            using (var files = new OwnedFiles())
            {
                string target = files.WriteIni("Sandboxie.ini", Preamble + Global + OldBox + Other
                    + "[UserSettings_Test]\r\nSbieCtrl_WindowCoords=1\r\nKeep=中文\r\n", "utf8-bom");
                string backup = files.WriteIni("backup.ini", NewBox, "utf8");
                byte[] before = File.ReadAllBytes(target);
                File.SetAttributes(target, File.GetAttributes(target) | FileAttributes.ReadOnly);
                FileAttributes attributes = File.GetAttributes(target);
                int reloads = 0;

                Assert.ThrowsException<UnauthorizedAccessException>(() =>
                {
                    var admin = new SandboxAdminService();
                    if (operation == "merge")
                        new BackupRestoreService().MergeIniSections(backup, target, new[] { "BoxA" });
                    else if (operation == "admin-change")
                        admin.ApplyIniSectionChanges(target, null, null,
                            new Dictionary<string, string> { { "BoxA", "BoxRenamed" } });
                    else
                        admin.RepairSandboxieControlUserSettings(target, () => { reloads++; return true; });
                });

                CollectionAssert.AreEqual(before, File.ReadAllBytes(target));
                Assert.AreEqual(attributes, File.GetAttributes(target));
                Assert.AreEqual(0, reloads);
                if (operation != "merge") AssertBackupBytes(target, before);
            }
        }

        private static byte[] Encode(string text, string encodingName)
        {
            Encoding encoding;
            switch (encodingName)
            {
                case "utf8": encoding = new UTF8Encoding(false, true); break;
                case "utf8-bom": encoding = new UTF8Encoding(true, true); break;
                case "utf16": encoding = new UnicodeEncoding(false, true, true); break;
                case "gbk": encoding = Encoding.GetEncoding(936, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback); break;
                default: throw new ArgumentException("Unknown fixture encoding: " + encodingName);
            }
            return encoding.GetPreamble().Concat(encoding.GetBytes(text)).ToArray();
        }

        private static void AssertIniBytes(string path, string expected, string encodingName)
        {
            CollectionAssert.AreEqual(Encode(expected, encodingName), File.ReadAllBytes(path),
                "Retain target " + encodingName + " bytes, BOM, line endings and all unrelated Chinese content.");
        }

        private static void AssertBackupBytes(string iniPath, byte[] expected)
        {
            string[] backups = Directory.GetFiles(Path.GetDirectoryName(iniPath),
                Path.GetFileName(iniPath) + ".沙河批量打开器备份_*.bak");
            Assert.AreEqual(1, backups.Length);
            CollectionAssert.AreEqual(expected, File.ReadAllBytes(backups[0]), "The recovery backup must preserve original bytes.");
        }

        private sealed class RestoreFixture : IDisposable
        {
            private readonly OwnedFiles files = new OwnedFiles();
            private readonly string extractedRoot;
            private readonly string usersRoot;
            private readonly string nonTarget;
            private readonly byte[] nonTargetBytes;
            private readonly FileAttributes nonTargetAttributes;

            public RestoreFixture(string encodingName, bool initiallyRunning)
            {
                extractedRoot = files.PathFor("extracted");
                usersRoot = files.PathFor("target/SandboxUsers");
                TargetIni = files.WriteIni("target/Sandboxie.ini", Preamble + Global + OldBox + Other, encodingName);
                files.WriteIni("extracted/Sandboxie.ini", NewBox + "[Other]\r\nKeep=不得导入\r\n", "utf8-bom");
                string source = files.WriteIni("extracted/SandboxUsers/BoxA/new.txt", "还原的新数据", "utf8");
                OldTarget = files.WriteIni("target/SandboxUsers/BoxA/old.txt", "必须能够回退的旧数据", "utf8");
                NewTarget = files.PathFor("target/SandboxUsers/BoxA/new.txt");
                nonTarget = files.WriteIni("target/SandboxUsers/Other/keep.txt", "未选择用户的数据", "gbk");
                foreach (string path in new[] { source, OldTarget, nonTarget })
                    File.SetAttributes(path, File.GetAttributes(path) | FileAttributes.ReadOnly);
                nonTargetBytes = File.ReadAllBytes(nonTarget);
                nonTargetAttributes = File.GetAttributes(nonTarget);
                Running = initiallyRunning;
                Callbacks = new BackupRestoreCallbacks
                {
                    IsSandboxieServiceRunning = () => Running,
                    StopSandboxieServices = () => { ServiceCalls.Add("stop"); Running = false; },
                    ReloadSandboxieConfig = () => ServiceCalls.Add("reload"),
                    RestartSandboxieServices = () => { ServiceCalls.Add("restart"); Running = true; },
                    SetStatus = Statuses.Add,
                    UpdateProgress = (value, text) => Progress.Add(value)
                };
            }

            public string TargetIni { get; }
            public string OldTarget { get; }
            public string NewTarget { get; }
            public bool Running { get; set; }
            public BackupRestoreCallbacks Callbacks { get; }
            public List<string> ServiceCalls { get; } = new List<string>();
            public List<string> Statuses { get; } = new List<string>();
            public List<int> Progress { get; } = new List<int>();

            public BackupRestoreResult Restore(string name)
            {
                return new BackupRestoreService().RestoreSelectedUsersAsync(
                    extractedRoot, usersRoot, TargetIni, new[] { name }, Callbacks).GetAwaiter().GetResult();
            }

            public void AssertNonTargetUnchanged()
            {
                CollectionAssert.AreEqual(nonTargetBytes, File.ReadAllBytes(nonTarget));
                Assert.AreEqual(nonTargetAttributes, File.GetAttributes(nonTarget));
            }

            public void AssertNoRecoveryCopies()
            {
                Assert.AreEqual(0, Directory.GetFileSystemEntries(files.PathFor("target"),
                    ".shahe-restore-*", SearchOption.AllDirectories).Length);
            }

            public void Dispose() { files.Dispose(); }
        }

        private sealed class OwnedFiles : IDisposable
        {
            private readonly string root = Path.Combine(Path.GetTempPath(), "shahe-ini-encoding-readonly", Guid.NewGuid().ToString("N"));

            public string PathFor(string relative)
            {
                string fullPath = Path.GetFullPath(Path.Combine(root, relative));
                if (!fullPath.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Fixture path escaped its owned root.");
                return fullPath;
            }

            public string WriteIni(string relative, string text, string encodingName)
            {
                string path = PathFor(relative);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllBytes(path, Encode(text, encodingName));
                return path;
            }

            public void Dispose()
            {
                if (Directory.Exists(root)) DeleteOwnedDirectory(root);
            }

            private void DeleteOwnedDirectory(string directory)
            {
                string fullPath = Path.GetFullPath(directory);
                if (!string.Equals(fullPath, root, StringComparison.OrdinalIgnoreCase)
                    && !fullPath.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Cleanup escaped its owned root.");
                if ((File.GetAttributes(fullPath) & FileAttributes.ReparsePoint) != 0)
                    throw new IOException("Cleanup refuses reparse points.");
                foreach (string file in Directory.GetFiles(fullPath))
                {
                    FileAttributes attributes = File.GetAttributes(file);
                    if ((attributes & FileAttributes.ReparsePoint) != 0)
                        throw new IOException("Cleanup refuses reparse points.");
                    // Backups and copied data can inherit read-only; clear it only during owned fixture cleanup.
                    File.SetAttributes(file, attributes & ~FileAttributes.ReadOnly);
                    File.Delete(file);
                }
                foreach (string child in Directory.GetDirectories(fullPath)) DeleteOwnedDirectory(child);
                File.SetAttributes(fullPath, File.GetAttributes(fullPath) & ~FileAttributes.ReadOnly);
                Directory.Delete(fullPath);
            }
        }
    }
}
