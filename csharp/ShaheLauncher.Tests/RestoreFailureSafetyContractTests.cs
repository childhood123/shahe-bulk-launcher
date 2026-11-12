using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaheLauncher.Core.Backup;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Backup")]
    public sealed class RestoreFailureSafetyContractTests
    {
        [DataTestMethod]
        [DataRow("data")]
        [DataRow("ini")]
        public async Task BlockedRollbackKeepsOriginalCopyAndReportsItsLocation(string blocked)
        {
            using (var fixture = new RestoreFixture())
            {
                FileStream held = null;
                try
                {
                    fixture.Callbacks.ReloadSandboxieConfig = () =>
                    {
                        string path = blocked == "data"
                            ? Path.Combine(fixture.UsersRoot, "BoxA", "new.txt") : fixture.TargetIni;
                        held = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                        throw new IOException("reload failed with rollback file locked");
                    };
                    BackupRestoreResult result = await fixture.RestoreAsync("BoxA");
                    AssertFailure(result, fixture);
                    Assert.IsFalse(fixture.Running, "Do not resume the service while data and INI cannot be rolled back consistently.");
                    StringAssert.Contains(result.Error, "回退失败");
                    if (blocked == "data")
                    {
                        string oldCopy = Directory.GetFiles(fixture.UsersRoot, "old.txt", SearchOption.AllDirectories)
                            .Single(path => path.Contains(".shahe-restore-") && path.Contains("old-0"));
                        StringAssert.Contains(result.Error, Path.GetDirectoryName(oldCopy));
                        Assert.IsTrue(File.ReadAllText(oldCopy).Length > 0);
                        CollectionAssert.AreEqual(fixture.OriginalIni, File.ReadAllBytes(fixture.TargetIni));
                    }
                    else
                    {
                        string oldIni = Directory.GetFiles(Path.GetDirectoryName(fixture.TargetIni), "*.previous").Single();
                        StringAssert.Contains(result.Error, oldIni);
                        CollectionAssert.AreEqual(fixture.OriginalIni, File.ReadAllBytes(oldIni));
                        Assert.IsTrue(File.Exists(Path.Combine(fixture.UsersRoot, "BoxA", "old.txt")));
                    }
                }
                finally { held?.Dispose(); }
            }
        }

        [TestMethod]
        public async Task FailedFirstRestoreRemovesNewDirectoryAndIni()
        {
            using (var fixture = new RestoreFixture())
            {
                string destination = Path.Combine(fixture.UsersRoot, "BoxA");
                Directory.Delete(destination, true);
                File.Delete(fixture.TargetIni);
                fixture.Callbacks.ReloadSandboxieConfig = () => { throw new IOException("first restore reload failed"); };
                BackupRestoreResult result = await fixture.RestoreAsync("BoxA");
                AssertFailure(result, fixture);
                Assert.IsFalse(Directory.Exists(destination));
                Assert.IsFalse(File.Exists(fixture.TargetIni));
                Assert.IsTrue(fixture.Running);
            }
        }

        [TestMethod]
        public async Task ReadOnlyOldFileCanBeReplacedWithoutLeavingRecoveryCopies()
        {
            using (var fixture = new RestoreFixture())
            {
                string oldFile = Path.Combine(fixture.UsersRoot, "BoxA", "old.txt");
                File.SetAttributes(oldFile, FileAttributes.ReadOnly);
                try
                {
                    BackupRestoreResult result = await fixture.RestoreAsync("BoxA");
                    Assert.IsTrue(result.Success, result.Error);
                    AssertRestoredUser(fixture, "BoxA");
                    Assert.AreEqual(0, Directory.GetDirectories(fixture.UsersRoot, ".shahe-restore-*").Length);
                }
                finally { if (File.Exists(oldFile)) File.SetAttributes(oldFile, FileAttributes.Normal); }
            }
        }

        [TestMethod]
        public async Task LockedBackupSourcePreservesOriginalDataIniAndRunningService()
        {
            using (var fixture = new RestoreFixture())
            {
                BackupRestoreResult result;
                using (FileStream held = File.Open(
                    Path.Combine(fixture.ExtractedRoot, "SandboxUsers", "BoxA", "new.txt"),
                    FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    result = await fixture.RestoreAsync("BoxA");
                }

                fixture.AssertOriginalTargets();
                Assert.IsTrue(fixture.Running, "Preparation failure must preserve the initially running service.");
                AssertFailure(result, fixture);
            }
        }

        [TestMethod]
        public async Task LockedTargetIniRollsBackDataAndPreservesIniBytes()
        {
            using (var fixture = new RestoreFixture())
            {
                BackupRestoreResult result;
                using (FileStream held = File.Open(
                    fixture.TargetIni, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    result = await fixture.RestoreAsync("BoxA");
                }

                fixture.AssertOriginalTargets();
                Assert.IsTrue(fixture.Running, "INI write failure must leave the service running again.");
                AssertFailure(result, fixture);
            }
        }

        [TestMethod]
        public async Task DuplicateBackupSectionsAreRejectedBeforeReplacingTargets()
        {
            using (var fixture = new RestoreFixture())
            {
                File.AppendAllText(fixture.BackupIni, "[BoxA]\r\nValue=duplicate\r\n", new UTF8Encoding(false));
                bool observedTargetChange = false;
                // Record observations instead of throwing assertions inside a callback the service can catch.
                fixture.Callbacks.UpdateProgress = (value, message) =>
                {
                    observedTargetChange |= !fixture.OriginalUsers.SequenceEqual(SnapshotDirectory(fixture.UsersRoot))
                        || !fixture.OriginalIni.SequenceEqual(File.ReadAllBytes(fixture.TargetIni));
                };

                BackupRestoreResult result = await fixture.RestoreAsync("BoxA");

                Assert.IsFalse(observedTargetChange, "Invalid duplicate sections must be rejected before target replacement.");
                fixture.AssertOriginalTargets();
                Assert.IsTrue(fixture.Running);
                AssertFailure(result, fixture);
            }
        }

        [TestMethod]
        public async Task LockedLaterUserRollsBackEarlierUserAndAllIniSections()
        {
            using (var fixture = new RestoreFixture())
            {
                BackupRestoreResult result;
                using (FileStream held = File.Open(
                    Path.Combine(fixture.UsersRoot, "BoxB", "old.txt"),
                    FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    result = await fixture.RestoreAsync("BoxA", "BoxB");
                }

                fixture.AssertOriginalTargets();
                Assert.IsTrue(fixture.Running, "A later user's failure must restore the service as well as earlier users.");
                AssertFailure(result, fixture);
            }
        }

        [TestMethod]
        public async Task ReloadExceptionRollsBackAllSelectedDataIniAndRestoresService()
        {
            using (var fixture = new RestoreFixture())
            {
                const string error = "injected reload failure";
                bool reloadCalled = false;
                bool firstReloadSawNewDataAndIni = false;
                fixture.Callbacks.ReloadSandboxieConfig = () =>
                {
                    if (!reloadCalled)
                    {
                        firstReloadSawNewDataAndIni =
                            SnapshotDirectory(Path.Combine(fixture.ExtractedRoot, "SandboxUsers", "BoxA"))
                                .SequenceEqual(SnapshotDirectory(Path.Combine(fixture.UsersRoot, "BoxA")))
                            && SnapshotDirectory(Path.Combine(fixture.ExtractedRoot, "SandboxUsers", "BoxB"))
                                .SequenceEqual(SnapshotDirectory(Path.Combine(fixture.UsersRoot, "BoxB")))
                            && File.ReadAllText(fixture.TargetIni).Contains("Value=new-BoxA")
                            && File.ReadAllText(fixture.TargetIni).Contains("Value=new-BoxB");
                    }
                    reloadCalled = true;
                    throw new IOException(error);
                };

                BackupRestoreResult result = await fixture.RestoreAsync("BoxA", "BoxB");

                Assert.IsTrue(reloadCalled, "The injected reload failure must be reached.");
                Assert.IsTrue(firstReloadSawNewDataAndIni, "Reload must exercise the failure after data and INI replacement.");
                fixture.AssertOriginalTargets();
                Assert.IsTrue(fixture.Running, "Even a repeatedly failing reload must not skip service recovery.");
                AssertFailure(result, fixture);
                StringAssert.Contains(result.Error, error);
            }
        }

        [TestMethod]
        public async Task StopExceptionStillAttemptsServiceRecoveryAndReportsFailure()
        {
            using (var fixture = new RestoreFixture())
            {
                const string error = "injected stop failure after service stopped";
                fixture.Callbacks.StopSandboxieServices = () =>
                {
                    fixture.StopCalled = true;
                    fixture.Running = false;
                    throw new IOException(error);
                };

                BackupRestoreResult result = await fixture.RestoreAsync("BoxA");

                Assert.IsTrue(fixture.StopCalled, "The stop failure must be reached.");
                fixture.AssertOriginalTargets();
                Assert.IsTrue(fixture.RestartCalled, "A stop callback may have stopped the service before throwing.");
                Assert.IsTrue(fixture.Running);
                AssertFailure(result, fixture);
                StringAssert.Contains(result.Error, error);
            }
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public async Task ServiceRecoveryExceptionNeverReportsRestoreSuccess(bool reloadFails)
        {
            using (var fixture = new RestoreFixture())
            {
                const string restartError = "injected service recovery failure";
                if (reloadFails)
                {
                    fixture.Callbacks.ReloadSandboxieConfig = () => { throw new IOException("injected reload failure"); };
                }
                fixture.Callbacks.RestartSandboxieServices = () =>
                {
                    fixture.RestartCalled = true;
                    throw new IOException(restartError);
                };

                BackupRestoreResult result = await fixture.RestoreAsync("BoxA", "BoxB");

                Assert.IsTrue(fixture.RestartCalled, "Service recovery must be attempted on both success and rollback paths.");
                Assert.IsFalse(fixture.Running, "The throwing recovery callback cannot restore the simulated service.");
                AssertFailure(result, fixture);
                StringAssert.Contains(result.Error, restartError, "A service recovery failure must remain visible to the caller.");
                if (reloadFails)
                {
                    fixture.AssertOriginalTargets();
                }
            }
        }

        [DataTestMethod]
        [DataRow("../outside")]
        [DataRow("..\\outside")]
        [DataRow("absolute")]
        public async Task InvalidUserPathsCannotChangeAnythingOutsideSandboxUsers(string user)
        {
            using (var fixture = new RestoreFixture())
            {
                if (user == "absolute")
                {
                    user = Path.Combine(fixture.Root, "absolute-user");
                }
                string source = RequireUnderRoot(fixture.Root,
                    Path.Combine(fixture.ExtractedRoot, "SandboxUsers", user));
                string destination = RequireUnderRoot(fixture.Root, Path.Combine(fixture.UsersRoot, user));
                WriteText(Path.Combine(source, "outside.txt"), "backup outside sentinel");
                WriteText(Path.Combine(destination, "outside.txt"), "precious outside sentinel");
                string[] before = SnapshotDirectory(fixture.Root);

                BackupRestoreResult result = await fixture.RestoreAsync("BoxA", user);

                CollectionAssert.AreEqual(before, SnapshotDirectory(fixture.Root),
                    "Reject the entire selection without changing selected users, backup sources, INI or outside sentinels.");
                Assert.IsTrue(fixture.Running);
                AssertFailure(result, fixture);
            }
        }

        [TestMethod]
        public async Task SuccessfulRestoreWithDefaultServiceStateLeavesUnselectedUsersAndSectionsUnchanged()
        {
            using (var fixture = new RestoreFixture())
            {
                string[] boxBBefore = SnapshotDirectory(Path.Combine(fixture.UsersRoot, "BoxB"));
                string[] unselectedBefore = SnapshotDirectory(Path.Combine(fixture.UsersRoot, "Unselected"));

                BackupRestoreResult result = await fixture.RestoreAsync("BoxA");

                Assert.IsTrue(result.Success, result.Error);
                Assert.AreEqual(string.Empty, result.Error);
                Assert.AreEqual(BackupRestoreService.RestoreCompletedStatus, result.Status);
                Assert.IsTrue(fixture.Running, "A null service-state query must retain the initially-running behavior.");
                Assert.IsTrue(fixture.StopCalled);
                Assert.IsTrue(fixture.RestartCalled);
                AssertRestoredUser(fixture, "BoxA");
                CollectionAssert.AreEqual(boxBBefore, SnapshotDirectory(Path.Combine(fixture.UsersRoot, "BoxB")));
                CollectionAssert.AreEqual(unselectedBefore, SnapshotDirectory(Path.Combine(fixture.UsersRoot, "Unselected")));
                string ini = File.ReadAllText(fixture.TargetIni);
                StringAssert.Contains(ini, "[BoxA]\r\nValue=new-BoxA\r\n");
                StringAssert.Contains(ini, "[BoxB]\r\nValue=old-BoxB\r\n");
                StringAssert.Contains(ini, RestoreFixture.UnselectedSection);
                StringAssert.Contains(ini, RestoreFixture.GlobalSection);
            }
        }

        [TestMethod]
        public async Task InitiallyStoppedServiceIsNeitherStoppedNorRestartedDuringRestore()
        {
            using (var fixture = new RestoreFixture())
            {
                fixture.Running = false;
                SetServiceStateQuery(fixture.Callbacks, () => fixture.Running);

                BackupRestoreResult result = await fixture.RestoreAsync("BoxA");

                Assert.IsTrue(result.Success, result.Error);
                AssertRestoredUser(fixture, "BoxA");
                StringAssert.Contains(File.ReadAllText(fixture.TargetIni), "[BoxA]\r\nValue=new-BoxA\r\n");
                Assert.IsFalse(fixture.StopCalled, "An initially stopped service must not be stopped again.");
                Assert.IsFalse(fixture.RestartCalled, "Restore must preserve the initial stopped state.");
                Assert.IsFalse(fixture.Running);
            }
        }

        [TestMethod]
        public async Task ServiceStateQueryExceptionPreservesDataIniAndDoesNotStopService()
        {
            using (var fixture = new RestoreFixture())
            {
                const string error = "injected service state query failure";
                SetServiceStateQuery(fixture.Callbacks, () => { throw new IOException(error); });

                BackupRestoreResult result = await fixture.RestoreAsync("BoxA");

                fixture.AssertOriginalTargets();
                Assert.IsFalse(fixture.StopCalled, "A failed query must not be interpreted as a running service.");
                Assert.IsFalse(fixture.RestartCalled);
                Assert.IsTrue(fixture.Running);
                AssertFailure(result, fixture);
                StringAssert.Contains(result.Error, error);
            }
        }

        private static void SetServiceStateQuery(BackupRestoreCallbacks callbacks, Func<bool> query)
        {
            PropertyInfo property = typeof(BackupRestoreCallbacks).GetProperty("IsSandboxieServiceRunning");
            Assert.IsNotNull(property, "BackupRestoreCallbacks.IsSandboxieServiceRunning must exist.");
            Assert.AreEqual(typeof(Func<bool>), property.PropertyType);
            Assert.IsTrue(property.CanWrite);
            property.SetValue(callbacks, query);
        }

        private static void AssertFailure(BackupRestoreResult result, RestoreFixture fixture)
        {
            Assert.IsFalse(result.Success, "A failed restore must not report success.");
            Assert.AreEqual(BackupRestoreService.RestoreFailedStatus, result.Status);
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.Error), "A failed restore must include its error.");
            CollectionAssert.DoesNotContain(fixture.Statuses, BackupRestoreService.RestoreCompletedStatus);
            Assert.IsTrue(fixture.Statuses.Count > 0);
            Assert.AreEqual(BackupRestoreService.RestoreFailedStatus, fixture.Statuses.Last());
        }

        private static void AssertRestoredUser(RestoreFixture fixture, string user)
        {
            CollectionAssert.AreEqual(
                SnapshotDirectory(Path.Combine(fixture.ExtractedRoot, "SandboxUsers", user)),
                SnapshotDirectory(Path.Combine(fixture.UsersRoot, user)));
        }

        private static string[] SnapshotDirectory(string root)
        {
            if (!Directory.Exists(root))
            {
                return new[] { "<missing directory>" };
            }
            string prefix = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            return Directory.GetDirectories(root, "*", SearchOption.AllDirectories)
                .Select(path => "D:" + path.Substring(prefix.Length))
                .Concat(Directory.GetFiles(root, "*", SearchOption.AllDirectories)
                    .Select(path => "F:" + path.Substring(prefix.Length) + ":" + Convert.ToBase64String(File.ReadAllBytes(path))))
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
        }

        private static string RequireUnderRoot(string root, string path)
        {
            string prefix = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            string fullPath = Path.GetFullPath(path);
            Assert.IsTrue(fullPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase),
                "Fixture path must remain inside its isolated root: " + fullPath);
            return fullPath;
        }

        private static void WriteText(string path, string text)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, text, new UTF8Encoding(false));
        }

        private sealed class RestoreFixture : IDisposable
        {
            public const string GlobalSection = "[GlobalSettings]\r\nKeep=全局配置\r\n\r\n";
            public const string UnselectedSection = "[Unselected]\r\nKeep=未选择用户\r\n\r\n";
            private readonly string temporaryParent;

            public RestoreFixture()
            {
                temporaryParent = Path.Combine(Path.GetTempPath(), "shahe-restore-failure-safety");
                Root = RequireUnderRoot(temporaryParent, Path.Combine(temporaryParent, Guid.NewGuid().ToString("N")));
                ExtractedRoot = Path.Combine(Root, "extracted");
                UsersRoot = Path.Combine(Root, "target", "SandboxUsers");
                TargetIni = Path.Combine(Root, "target", "Sandboxie.ini");
                BackupIni = Path.Combine(ExtractedRoot, "Sandboxie.ini");
                foreach (string user in new[] { "BoxA", "BoxB", "Unselected" })
                {
                    string source = Path.Combine(ExtractedRoot, "SandboxUsers", user);
                    string target = Path.Combine(UsersRoot, user);
                    WriteText(Path.Combine(source, "new.txt"), "new-data-" + user);
                    WriteText(Path.Combine(source, "profile", "settings.txt"), "new-profile-" + user);
                    Directory.CreateDirectory(Path.Combine(source, "new-empty"));
                    WriteText(Path.Combine(target, "old.txt"), "precious-old-data-" + user);
                    WriteText(Path.Combine(target, "profile", "settings.txt"), "旧配置-" + user);
                    Directory.CreateDirectory(Path.Combine(target, "old-empty"));
                }
                WriteText(TargetIni, "; preserve this comment\r\n" + GlobalSection
                    + "[BoxA]\r\nValue=old-BoxA\r\n\r\n[BoxB]\r\nValue=old-BoxB\r\n\r\n" + UnselectedSection);
                WriteText(BackupIni, "[BoxA]\r\nValue=new-BoxA\r\n\r\n[BoxB]\r\nValue=new-BoxB\r\n\r\n"
                    + "[Unselected]\r\nKeep=must-not-restore\r\n\r\n");
                OriginalUsers = SnapshotDirectory(UsersRoot);
                OriginalIni = File.ReadAllBytes(TargetIni);
                Running = true;
                Statuses = new List<string>();
                Callbacks = new BackupRestoreCallbacks
                {
                    StopSandboxieServices = () => { StopCalled = true; Running = false; },
                    RestartSandboxieServices = () => { RestartCalled = true; Running = true; },
                    ReloadSandboxieConfig = () => { },
                    SetStatus = Statuses.Add
                };
            }

            public string Root { get; private set; }
            public string ExtractedRoot { get; private set; }
            public string UsersRoot { get; private set; }
            public string TargetIni { get; private set; }
            public string BackupIni { get; private set; }
            public string[] OriginalUsers { get; private set; }
            public byte[] OriginalIni { get; private set; }
            public List<string> Statuses { get; private set; }
            public BackupRestoreCallbacks Callbacks { get; private set; }
            public bool Running { get; set; }
            public bool StopCalled { get; set; }
            public bool RestartCalled { get; set; }

            public Task<BackupRestoreResult> RestoreAsync(params string[] users)
            {
                return new BackupRestoreService().RestoreSelectedUsersAsync(
                    ExtractedRoot, UsersRoot, TargetIni, users, Callbacks);
            }

            public void AssertOriginalTargets()
            {
                CollectionAssert.AreEqual(OriginalUsers, SnapshotDirectory(UsersRoot),
                    "Every selected and unselected user's files and empty directories must remain unchanged on failure.");
                CollectionAssert.AreEqual(OriginalIni, File.ReadAllBytes(TargetIni),
                    "Failure must preserve the exact original INI bytes.");
            }

            public void Dispose()
            {
                string cleanupPath = RequireUnderRoot(temporaryParent, Root);
                if (Directory.Exists(cleanupPath))
                {
                    Directory.Delete(cleanupPath, true);
                }
            }
        }
    }
}
