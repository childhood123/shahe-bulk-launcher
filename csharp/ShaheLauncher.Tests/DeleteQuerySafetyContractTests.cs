using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaheLauncher.Core.SandboxAdmin;
using ShaheLauncher.Core.Sandboxie;

namespace ShaheLauncher.Tests
{
    [TestClass, TestCategory("SandboxAdmin")]
    public sealed class DeleteQuerySafetyContractTests
    {
        [TestMethod]
        public void RenameStillRejectsRunningUserWhenDeleteQueryFails()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                string users = Path.Combine(root, "users");
                string sentinel = Path.Combine(users, "BoxA", "keep.txt");
                Directory.CreateDirectory(Path.GetDirectoryName(sentinel));
                File.WriteAllText(sentinel, "original");
                string ini = Path.Combine(root, "Sandboxie.ini");
                string originalIni = "[BoxA]\r\nEnabled=y\r\n";
                File.WriteAllText(ini, originalIni);
                form.SandboxieClient.SandboxRoot = users;
                form.SandboxieIniPathCallback = () => ini;
                form.ReloadSandboxieConfigCallback = () => { };
                form.SetSandboxAliasCallback = (name, alias) => true;
                MigrationRuntimeTestHelpers.Invoke(form, "InitializeUi");
                form.SandboxList.ReplaceItemTexts(new[] { "BoxA" }, new int[0]);
                form.SandboxList.CheckItem(0, true);
                form.SandboxAdminCallbacks.NormalizeSandboxName = name => name;
                form.SandboxAdminCallbacks.GetExistingSandboxDisplayNames = () => new[] { "BoxA" };
                form.SandboxAdminCallbacks.GetExistingSandboxNames = () => new[] { "BoxA" };
                CollectionAssert.AreEqual(new[] { "BoxA" },
                    ((IList<string>)MigrationRuntimeTestHelpers.Invoke(form, "GetBatchTargetSandboxes")).ToArray());
                int normalQueries = 0;
                form.ProcessCallbacks.GetSandboxPids = name => { normalQueries++; return new[] { 123 }; };
                form.ProcessCallbacks.RunSandboxieStart = args => { throw new IOException("strict query unavailable"); };
                form.ShowBatchRenameRuleCallback = (title, prompt, prefix, count) => new Dictionary<string, object>
                {
                    { "prefix", "Renamed" }, { "start", 1 }, { "digits", 0 }, { "count", 1 }
                };
                var messages = new List<string>();
                form.ShowMessageCallback = (text, title, icon) => messages.Add(text);
                MigrationRuntimeTestHelpers.Invoke(form, "BatchRenameSandboxes");
                Assert.AreEqual(1, normalQueries, "Rename must retain the ordinary process query.");
                Assert.IsTrue(messages.Any(text => text.Contains("仍在运行")));
                Assert.IsTrue(File.Exists(sentinel));
                Assert.AreEqual("original", File.ReadAllText(sentinel));
                Assert.AreEqual(originalIni, File.ReadAllText(ini));
                Assert.IsFalse(Directory.Exists(Path.Combine(users, "Renamed1")));
            });
        }

        [DataTestMethod]
        [DataRow("throws")]
        [DataRow("null")]
        [DataRow("missing")]
        public void UnknownProcessQueryCannotAuthorizeDeletionOrTermination(string mode)
        {
            int terminations = 0;
            var callbacks = new SandboxAdminCallbacks
            {
                GetSandboxPids = mode == "missing" ? null : new Func<string, IEnumerable<int>>(_ =>
                {
                    if (mode == "throws") throw new IOException("process query failed");
                    return null;
                }),
                TerminateSandbox = _ => { terminations++; return true; },
                TerminatePids = _ => terminations++
            };
            Assert.IsFalse(new SandboxAdminService().EnsureSandboxStoppedBeforeDelete("BoxA", 0, callbacks));
            Assert.AreEqual(0, terminations);
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void MissingOrNullLivenessResultCannotProveProcessesExited(bool nullResult)
        {
            var callbacks = new SandboxAdminCallbacks
            {
                GetSandboxPids = _ => new[] { 123 },
                GetAlivePids = nullResult ? new Func<IEnumerable<int>, IList<int>>(_ => null) : null,
                TerminateSandbox = _ => true,
                TerminatePids = _ => { }
            };
            Assert.IsFalse(new SandboxAdminService().EnsureSandboxStoppedBeforeDelete("BoxA", 0, callbacks));
        }

        [TestMethod]
        public void ConfirmedEmptyQueryAllowsDeletionWithoutTermination()
        {
            var callbacks = new SandboxAdminCallbacks
            {
                GetSandboxPids = _ => new int[0],
                TerminateSandbox = _ => { Assert.Fail("No running processes need termination."); return false; }
            };
            Assert.IsTrue(new SandboxAdminService().EnsureSandboxStoppedBeforeDelete("BoxA", 0, callbacks));
        }

        [DataTestMethod]
        [DataRow("throws")]
        [DataRow("null")]
        [DataRow("nonzero")]
        [DataRow("empty-output")]
        [DataRow("invalid-output")]
        public void MainWindowBlocksBothOfficialAndDirectoryDeletionWhenQueryIsUnknown(string mode)
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                string users = Path.Combine(root, "users");
                string sentinel = Path.Combine(users, "BoxA", "keep.txt");
                Directory.CreateDirectory(Path.GetDirectoryName(sentinel));
                File.WriteAllText(sentinel, "original");
                form.SandboxieClient.SandboxRoot = users;
                form.SandboxieClient.SandboxiePath = Path.Combine(root, "missing-sandboxie");
                MigrationRuntimeTestHelpers.Invoke(form, "InitializeUi");
                form.SandboxList.ReplaceItemTexts(new[] { "BoxA" }, new int[0]);
                form.SandboxList.CheckItem(0, true);
                form.SandboxAdminCallbacks.NormalizeSandboxName = name => name;
                form.SandboxAdminCallbacks.GetExistingSandboxDisplayNames = () => new[] { "BoxA" };
                form.SandboxAdminCallbacks.GetExistingSandboxNames = () => new[] { "BoxA" };
                CollectionAssert.AreEqual(new[] { "BoxA" },
                    ((IList<string>)MigrationRuntimeTestHelpers.Invoke(form, "GetBatchTargetSandboxes")).ToArray());
                form.ConfirmMessageCallback = (text, title, buttons, icon) => DialogResult.Yes;
                form.ShowMessageCallback = (text, title, icon) => { };
                int queries = 0;
                form.ProcessCallbacks.RunSandboxieStart = args =>
                {
                    queries++;
                    CollectionAssert.AreEqual(new[] { "/box:BoxA", "/listpids" }, args.ToArray());
                    if (mode == "throws") throw new IOException("isolated query failure");
                    if (mode == "null") return null;
                    if (mode == "nonzero") return Tuple.Create(7, "query refused");
                    return Tuple.Create(0, mode == "empty-output" ? "" : "unexpected query output");
                };
                int deletions = 0;
                form.DeleteSandboxCallback = name => { deletions++; return new SandboxieOperationResult(); };
                MigrationRuntimeTestHelpers.Invoke(form, "BatchDeleteSandboxes");
                Assert.AreEqual(0, deletions);
                Assert.IsTrue(File.Exists(sentinel));
                Assert.AreEqual("original", File.ReadAllText(sentinel));
                Assert.AreEqual(1, queries, "Deletion must use a query that preserves failure information.");
            });
        }

        [TestMethod]
        public void MainWindowDeleteQueryPreservesConfirmedEmptyResult()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                form.SandboxAdminCallbacks.NormalizeSandboxName = name => name;
                form.ProcessCallbacks.RunSandboxieStart = args => Tuple.Create(0, "0\r\n");
                Assert.AreEqual(0, form.SandboxAdminCallbacks.GetSandboxPids("BoxA").Count());
            });
        }

        [TestMethod]
        public void MainWindowDeleteQueryPreservesPidListWithoutTreatingCountAsPid()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                form.SandboxAdminCallbacks.NormalizeSandboxName = name => name;
                form.ProcessCallbacks.RunSandboxieStart = args => Tuple.Create(0, "2\r\n123\r\n456\r\n");
                CollectionAssert.AreEqual(new[] { 123, 456 }, form.SandboxAdminCallbacks.GetSandboxPids("BoxA").ToArray());
            });
        }

        [DataTestMethod]
        [DataRow("2\r\n123\r\n")]
        [DataRow("1\r\n0\r\n")]
        [DataRow("2\r\n123\r\n123\r\n")]
        [DataRow("error 123")]
        public void MainWindowDeleteQueryRejectsTruncatedOrMalformedPidLists(string output)
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                form.SandboxAdminCallbacks.NormalizeSandboxName = name => name;
                form.ProcessCallbacks.RunSandboxieStart = args => Tuple.Create(0, output);
                Assert.ThrowsException<InvalidOperationException>(() => form.SandboxAdminCallbacks.GetSandboxPids("BoxA").ToArray());
            });
        }
    }
}
