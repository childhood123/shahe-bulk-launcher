using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaheLauncher.Core.Backup;

namespace ShaheLauncher.Tests
{
    [TestClass, TestCategory("Backup")]
    public sealed class RestoreProgressFailureContractTests
    {
        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void ReloadThatStartsServiceMustStopItBeforeRollback(bool secondStopFails)
        {
            string root = Path.Combine(Path.GetTempPath(), "ShaheRestoreReload-" + Guid.NewGuid().ToString("N"));
            string extracted = Path.Combine(root, "backup");
            string users = Path.Combine(root, "users");
            string box = Path.Combine(users, "BoxA");
            string ini = Path.Combine(root, "target.ini");
            try
            {
                Directory.CreateDirectory(Path.Combine(extracted, "SandboxUsers", "BoxA"));
                Directory.CreateDirectory(box);
                File.WriteAllText(Path.Combine(extracted, "SandboxUsers", "BoxA", "new.txt"), "new");
                File.WriteAllText(Path.Combine(extracted, "Sandboxie.ini"), "[BoxA]\r\nValue=new\r\n");
                File.WriteAllText(Path.Combine(box, "old.txt"), "old");
                File.WriteAllText(ini, "[BoxA]\r\nValue=old\r\n");
                bool running = true;
                int stops = 0;
                int starts = 0;
                bool stoppedBeforeRollback = false;
                var result = new BackupRestoreService().RestoreSelectedUsersAsync(extracted, users, ini,
                    new[] { "BoxA" }, new BackupRestoreCallbacks
                    {
                        StopSandboxieServices = () =>
                        {
                            stops++;
                            if (stops == 2)
                            {
                                stoppedBeforeRollback = running && File.Exists(Path.Combine(box, "new.txt"))
                                    && File.ReadAllText(ini).Contains("Value=new");
                                if (secondStopFails) throw new IOException("second stop refused");
                            }
                            running = false;
                        },
                        ReloadSandboxieConfig = () =>
                        {
                            running = true;
                            throw new IOException("reload failed after service started");
                        },
                        RestartSandboxieServices = () => { starts++; running = true; }
                    }).GetAwaiter().GetResult();
                Assert.IsFalse(result.Success);
                Assert.IsTrue(stoppedBeforeRollback, "A reload failure may leave the service running; stop before touching data.");
                if (secondStopFails)
                {
                    StringAssert.Contains(result.Error, "second stop refused");
                    Assert.IsTrue(File.Exists(Path.Combine(box, "new.txt")), "Do not roll back while stop was refused.");
                    StringAssert.Contains(File.ReadAllText(ini), "Value=new");
                    Assert.IsTrue(Directory.GetFiles(users, "old.txt", SearchOption.AllDirectories).Length > 0,
                        "Keep the original data copy for recovery.");
                    Assert.IsTrue(Directory.GetFiles(root, "*.previous").Length > 0, "Keep the original INI copy.");
                    Assert.AreEqual(0, starts);
                }
                else
                {
                    Assert.AreEqual("old", File.ReadAllText(Path.Combine(box, "old.txt")));
                    StringAssert.Contains(File.ReadAllText(ini), "Value=old");
                    Assert.IsTrue(running);
                }
            }
            finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
        }

        [TestMethod]
        public void FailureAfterDataAndIniReplacementRestoresBothAndResumesService()
        {
            string root = Path.Combine(Path.GetTempPath(), "ShaheRestoreProgress-" + Guid.NewGuid().ToString("N"));
            string extracted = Path.Combine(root, "backup");
            string users = Path.Combine(root, "users");
            string ini = Path.Combine(root, "target.ini");
            try
            {
                Directory.CreateDirectory(Path.Combine(extracted, "SandboxUsers", "BoxA"));
                Directory.CreateDirectory(Path.Combine(users, "BoxA"));
                File.WriteAllText(Path.Combine(extracted, "SandboxUsers", "BoxA", "new.txt"), "new");
                File.WriteAllText(Path.Combine(extracted, "Sandboxie.ini"), "[BoxA]\r\nValue=new\r\n");
                File.WriteAllText(Path.Combine(users, "BoxA", "old.txt"), "old");
                File.WriteAllText(ini, "[BoxA]\r\nValue=old\r\n");
                byte[] oldIni = File.ReadAllBytes(ini);
                bool running = true;
                BackupRestoreResult result = new BackupRestoreService().RestoreSelectedUsersAsync(
                    extracted, users, ini, new[] { "BoxA" }, new BackupRestoreCallbacks
                    {
                        StopSandboxieServices = () => running = false,
                        RestartSandboxieServices = () => running = true,
                        UpdateProgress = (value, text) =>
                        {
                            if (value == 90) throw new IOException("progress callback failed after replacement");
                        }
                    }).GetAwaiter().GetResult();
                Assert.IsFalse(result.Success);
                StringAssert.Contains(result.Error, "progress callback failed");
                Assert.IsTrue(File.Exists(Path.Combine(users, "BoxA", "old.txt")), "Old data must be restored.");
                Assert.AreEqual("old", File.ReadAllText(Path.Combine(users, "BoxA", "old.txt")));
                Assert.IsFalse(File.Exists(Path.Combine(users, "BoxA", "new.txt")));
                CollectionAssert.AreEqual(oldIni, File.ReadAllBytes(ini));
                Assert.IsTrue(running, "A stopped service must be resumed on failure.");
            }
            finally
            {
                if (Directory.Exists(root)) Directory.Delete(root, true);
            }
        }
    }
}
