using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaheLauncher.App.UI.Dialogs.Common;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("AuditRunner")]
    public sealed class SandboxieLifecycleAuditRunnerTests
    {
        private const string AssemblyName = "ShaheLauncher.AuditRunner";

        [TestMethod]
        public void LifecycleRequiresBaselineExecuteAndUniquePrefix()
        {
            Type commandLine = RequireType("AuditCommandLine");
            AssertRejected(() => Invoke(commandLine, "Parse", (object)new[] { "lifecycle" }));
            AssertRejected(() => Invoke(commandLine, "Parse", (object)new[] { "lifecycle", "--execute", "--prefix", "ShaheAudit_One" }));
            AssertRejected(() => Invoke(commandLine, "Parse", (object)new[] { "lifecycle", "--execute", "--baseline", "baseline.json" }));
            AssertRejected(() => Invoke(commandLine, "Parse", (object)new[] { "lifecycle", "--execute", "--baseline", "baseline.json", "--prefix", "Other" }));
            AssertRejected(() => Invoke(commandLine, "Parse", (object)new[]
            {
                "lifecycle", "--execute", "--baseline", "baseline.json", "--prefix", "ShaheAudit_One"
            }));

            object options = Invoke(
                commandLine,
                "Parse", (object)new[]
                {
                    "lifecycle", "--execute", "--baseline", "baseline.json",
                    "--prefix", "ShaheAudit_One", "--stage", "batch-create", "--state", "state.json"
                });
            Assert.AreEqual("Lifecycle", Property<object>(options, "Mode").ToString());
            Assert.AreEqual(true, Property<bool>(options, "Execute"));
            Assert.AreEqual("baseline.json", Property<string>(options, "BaselinePath"));
            Assert.AreEqual("ShaheAudit_One", Property<string>(options, "Prefix"));
            Assert.AreEqual("batch-create", Property<string>(options, "Stage"));
            Assert.AreEqual("state.json", Property<string>(options, "StatePath"));
        }

        [TestMethod]
        public void VerifyCleanProducesReadOnlyOwnershipReport()
        {
            string directory = Path.Combine(Path.GetTempPath(), "ShaheAuditRunnerTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            try
            {
                string target = Path.Combine(directory, "target.exe");
                string sandboxie = Path.Combine(directory, "Sandboxie");
                string sandboxRoot = Path.Combine(directory, "Sandbox");
                string config = Path.Combine(directory, "shahe_config.ini");
                string ini = Path.Combine(directory, "Sandboxie.ini");
                Directory.CreateDirectory(sandboxie);
                Directory.CreateDirectory(sandboxRoot);
                File.WriteAllText(target, "target");
                File.WriteAllText(config,
                    "[settings]\r\nsandboxie_path=\"" + sandboxie.Replace("\\", "\\\\")
                    + "\"\r\nsandbox_root=\"" + sandboxRoot.Replace("\\", "\\\\") + "\"\r\n");
                File.WriteAllText(ini, "[GlobalSettings]\r\n");
                byte[] targetBefore = File.ReadAllBytes(target);
                byte[] configBefore = File.ReadAllBytes(config);
                byte[] iniBefore = File.ReadAllBytes(ini);

                Type optionsType = RequireType("AuditOptions");
                Type modeType = RequireType("AuditMode");
                object options = New(optionsType,
                    "Mode", Enum.Parse(modeType, "VerifyClean"),
                    "TargetPath", target,
                    "ConfigPath", config,
                    "SandboxieIniPath", ini,
                    "OutputDirectory", directory,
                    "ExpectedTargetSha256", Sha256(targetBefore),
                    "ExpectedConfigSha256", Sha256(configBefore),
                    "ExpectedIniSha256", Sha256(iniBefore));
                Type auditType = RequireType("SandboxieLifecycleAudit");
                int code = (int)auditType.GetMethod("Run").Invoke(Activator.CreateInstance(auditType), new[] { options });

                string report = Directory.GetFiles(directory, "auditrunner-verify-clean-*-result.json").Single();
                string text = File.ReadAllText(report);
                StringAssert.Contains(text, "\"Kind\":\"auditrunner-verify-clean\"");
                Assert.IsFalse(text.Contains("Mode is not implemented yet"));
                Assert.AreEqual(3, code, "Unbound verify-clean must not report a clean lifecycle.");
                StringAssert.Contains(text, "\"ownedResources\":false");
                CollectionAssert.AreEqual(targetBefore, File.ReadAllBytes(target));
                CollectionAssert.AreEqual(configBefore, File.ReadAllBytes(config));
                CollectionAssert.AreEqual(iniBefore, File.ReadAllBytes(ini));
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public void ProcessSnapshotCanExcludeItsOwnLauncherChain()
        {
            Type audit = RequireType("SandboxieLifecycleAudit");
            MethodInfo method = audit.GetMethod(
                "GetAncestorProcessIds",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(method);
            var values = ((IEnumerable<int>)method.Invoke(null, null)).ToArray();
            CollectionAssert.Contains(values, Process.GetCurrentProcess().Id);
        }

        [TestMethod]
        public void MenuCommandsAreResolvedFromCurrentUniqueTitles()
        {
            Type commandType = RequireType("MenuCommandInfo");
            Type safety = RequireType("AuditSafety");
            IList commands = List(commandType,
                New(commandType, "Id", 41, "Title", "刷新"),
                New(commandType, "Id", 907, "Title", "退出软件\tAlt+F4"));
            Assert.AreEqual(907, Invoke(safety, "FindUniqueMenuCommand", commands, "退出软件\tAlt+F4"));

            IList duplicates = List(commandType,
                New(commandType, "Id", 1, "Title", "退出软件\tAlt+F4"),
                New(commandType, "Id", 2, "Title", "退出软件\tAlt+F4"));
            AssertRejected(() => Invoke(safety, "FindUniqueMenuCommand", duplicates, "退出软件\tAlt+F4"));
            AssertRejected(() => Invoke(safety, "FindUniqueMenuCommand", commands, "不存在"));
        }

        [TestMethod]
        public void ExistingSandboxProcessCannotEnableFailureCleanup()
        {
            Type claimType = RequireType("OwnershipClaim");
            IList claims = List(claimType, New(claimType, "InternalName", "Box901", "Alias", "owned"));
            int launchNotifications = 0;
            AssertRejected(() => Invoke(RequireType("AuditSafety"), "ConfirmOwnedLaunchStart", claims,
                new Func<string, IEnumerable<int>>(name => new[] { 123 }),
                new Action(() => launchNotifications++)));
            Assert.AreEqual(0, launchNotifications, "A rejected launch must never authorize exception cleanup.");
        }

        [TestMethod]
        public void TimerStopCannotAuthorizeCleanupWhileCallbackIsStillRunning()
        {
            using (var entered = new ManualResetEventSlim())
            using (var release = new ManualResetEventSlim())
            using (var ended = new ManualResetEventSlim())
            {
                var timer = new System.Threading.Timer(_ =>
                {
                    entered.Set();
                    release.Wait();
                    ended.Set();
                }, null, 1, Timeout.Infinite);
                try
                {
                    Assert.IsTrue(entered.Wait(TimeSpan.FromSeconds(3)));
                    MethodInfo stop = RequireType("SandboxieLifecycleAudit").GetMethod("StopAuditTimer", BindingFlags.Static | BindingFlags.NonPublic);
                    Assert.AreEqual(false, stop.Invoke(null, new object[] { timer, 20 }), "Active callbacks must prevent cleanup.");
                    Assert.IsFalse(ended.IsSet);
                }
                finally
                {
                    release.Set();
                    Assert.IsTrue(ended.Wait(TimeSpan.FromSeconds(3)));
                    timer.Dispose();
                }
            }
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void CleanupRechecksOwnershipBeforeEveryMutation(bool changeCurrentAlias)
        {
            Type claimType = RequireType("OwnershipClaim");
            Type snapshotType = RequireType("OwnershipSnapshot");
            Type safety = RequireType("AuditSafety");
            object baseline = Snapshot(snapshotType, true, new Dictionary<string, string>(), new string[0]);
            var aliases = new Dictionary<string, string> { ["Box901"] = "owned-1", ["Box902"] = "owned-2" };
            var writes = new List<string>();
            object first = New(claimType, "InternalName", "Box901", "Alias", "owned-1", "DirectoryName", "Box901");
            object second = New(claimType, "InternalName", "Box902", "Alias", "owned-2", "DirectoryName", "Box902");
            foreach (object claim in new[] { first, second })
            {
                Invoke(safety, "DeleteOwnedResource", claim,
                    new Func<string, int>(name =>
                    {
                        writes.Add("delete:" + name);
                        if (name == "Box901") aliases[changeCurrentAlias ? "Box901" : "Box902"] = "user-changed";
                        return 0;
                    }), new Func<string, bool>(name => false),
                    new Action<string>(name => writes.Add("section:" + name)),
                    new Action(() => Invoke(safety, "SelectOwnedResourcesForCleanup", baseline,
                        Snapshot(snapshotType, true, aliases, new[] { "Box901", "Box902" }), List(claimType, claim))));
                if (changeCurrentAlias) break;
            }
            CollectionAssert.AreEqual(changeCurrentAlias
                ? new[] { "delete:Box901" } : new[] { "delete:Box901", "section:Box901" }, writes.ToArray());
        }

        [TestMethod]
        public void VerifyCleanRejectsOrphanDirectoryOutsideAuditPrefix()
        {
            Type snapshotType = RequireType("OwnershipSnapshot");
            Type claimType = RequireType("OwnershipClaim");
            Type safety = RequireType("AuditSafety");
            object baseline = Snapshot(snapshotType, true,
                new Dictionary<string, string> { ["DefaultBox"] = "user" }, new[] { "DefaultBox" });
            object current = Snapshot(snapshotType, true,
                new Dictionary<string, string> { ["DefaultBox"] = "user" }, new[] { "DefaultBox", "Box901" });
            IList claims = List(claimType, New(claimType, "InternalName", "Box901",
                "Alias", "ShaheAudit_RunOne", "DirectoryName", "Box901"));
            object result = Invoke(safety, "VerifyOwnedResourcesAbsent", baseline, current, claims);
            Assert.AreEqual("Inconclusive", Property<object>(result, "Status").ToString());
            Property<IList<string>>(current, "Directories").Remove("Box901");
            result = Invoke(safety, "VerifyOwnedResourcesAbsent", baseline, current, claims);
            Assert.AreEqual("Passed", Property<object>(result, "Status").ToString());
            Property<IDictionary<string, string>>(current, "AliasesBySection")["Box950"] = "ShaheAudit_RunOne";
            result = Invoke(safety, "VerifyOwnedResourcesAbsent", baseline, current, claims);
            Assert.AreEqual("Inconclusive", Property<object>(result, "Status").ToString());
        }

        [TestMethod]
        public void ProcessTerminationRejectsChangedOwnershipBeforeExecutingCommand()
        {
            string directory = Path.Combine(Path.GetTempPath(), "ShaheAuditRunnerTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            string ini = Path.Combine(directory, "Sandboxie.ini");
            try
            {
                Directory.CreateDirectory(Path.Combine(directory, "Box901"));
                File.WriteAllText(ini, "[Box901]\r\nBoxAlias=user-changed\r\n");
                object baseline = New(RequireType("PreflightReport"), "IniReadable", true,
                    "SandboxieIniPath", ini, "SandboxRoot", directory);
                object claim = New(RequireType("OwnershipClaim"), "InternalName", "Box901",
                    "Alias", "ShaheAudit_Original", "DirectoryName", "Box901");
                var client = new ShaheLauncher.Core.Sandboxie.SandboxieClient
                    { SandboxiePath = Path.Combine(directory, "missing-install") };
                MethodInfo terminate = RequireType("SandboxieLifecycleAudit").GetMethod(
                    "TerminateOwnedSandboxProcess", BindingFlags.Static | BindingFlags.NonPublic);
                AssertRejected(() => terminate.Invoke(null, new[] { baseline, client, claim, (object)false }));
                StringAssert.Contains(File.ReadAllText(ini), "user-changed");
            }
            finally { Directory.Delete(directory, true); }
        }

        [TestMethod]
        public void ProcessQueryFailureCannotBeReportedAsZeroProcesses()
        {
            var client = new ShaheLauncher.Core.Sandboxie.SandboxieClient
                { SandboxiePath = Path.Combine(Path.GetTempPath(), "MissingSandboxie-" + Guid.NewGuid().ToString("N")) };
            MethodInfo query = RequireType("SandboxieLifecycleAudit").GetMethod(
                "QuerySandboxPids", BindingFlags.Static | BindingFlags.NonPublic);
            AssertRejected(() => query.Invoke(null, new object[] { client, "Box901" }));
        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(2)]
        public void AliasRenameRestoresAttemptedWritesAfterCommandFailure(int failAt)
        {
            Type claimType = RequireType("OwnershipClaim");
            IList claims = List(claimType,
                New(claimType, "InternalName", "Box901", "Alias", "original-1", "DirectoryName", "Box901"),
                New(claimType, "InternalName", "Box902", "Alias", "original-2", "DirectoryName", "Box902"));
            var aliases = new Dictionary<string, string> { ["Box901"] = "original-1", ["Box902"] = "original-2" };
            int writes = 0;
            bool committed = false;
            AssertRejected(() => Invoke(RequireType("AuditSafety"), "RenameOwnedAliases", claims,
                new[] { "renamed-1", "renamed-2" },
                new Func<string, string, int>((name, alias) =>
                {
                    aliases[name] = alias;
                    return ++writes == failAt ? 1 : 0;
                }), new Action(() => { }), new Action(() => committed = true)));
            Assert.AreEqual("original-1", aliases["Box901"]);
            Assert.AreEqual("original-2", aliases["Box902"]);
            Assert.IsFalse(committed);
        }

        [TestMethod]
        public void AliasRollbackContinuesAfterOneRestoreFails()
        {
            Type claimType = RequireType("OwnershipClaim");
            IList claims = List(claimType,
                New(claimType, "InternalName", "Box901", "Alias", "original-1", "DirectoryName", "Box901"),
                New(claimType, "InternalName", "Box902", "Alias", "original-2", "DirectoryName", "Box902"));
            var aliases = new Dictionary<string, string> { ["Box901"] = "original-1", ["Box902"] = "original-2" };
            try
            {
                Invoke(RequireType("AuditSafety"), "RenameOwnedAliases", claims,
                    new[] { "renamed-1", "renamed-2" },
                    new Func<string, string, int>((name, alias) =>
                    {
                        if (name == "Box901" && alias == "original-1") return 9;
                        aliases[name] = alias;
                        return 0;
                    }), new Action(() => { }),
                    new Action(() => { throw new InvalidOperationException("commit failed"); }));
                Assert.Fail("Expected commit failure.");
            }
            catch (TargetInvocationException exception)
            {
                StringAssert.Contains(exception.InnerException.Message, "Box901");
                StringAssert.Contains(exception.InnerException.Message, "rollback");
            }
            Assert.AreEqual("original-2", aliases["Box902"]);
        }

        [TestMethod]
        public void CleanupSelectionAllowsOnlyAbsentOwnedResourcesOnRetry()
        {
            Type snapshotType = RequireType("OwnershipSnapshot");
            Type claimType = RequireType("OwnershipClaim");
            Type safety = RequireType("AuditSafety");
            object baseline = Snapshot(snapshotType, true,
                new Dictionary<string, string> { ["DefaultBox"] = "user" }, new[] { "DefaultBox" });
            object current = Snapshot(snapshotType, true,
                new Dictionary<string, string> { ["DefaultBox"] = "user", ["Box902"] = "audit-2" }, new[] { "DefaultBox" });
            IList claims = List(claimType,
                New(claimType, "InternalName", "Box901", "Alias", "audit-1", "DirectoryName", "Box901"),
                New(claimType, "InternalName", "Box902", "Alias", "audit-2", "DirectoryName", "Box902"));
            Assert.AreEqual(2, ((IList)Invoke(safety, "SelectOwnedResourcesForCleanup", baseline, current, claims)).Count);
            AssertRejected(() => Invoke(safety, "SelectOwnedResources", baseline, current, claims));
            Property<IDictionary<string, string>>(current, "AliasesBySection")["Box902"] = "user-changed";
            AssertRejected(() => Invoke(safety, "SelectOwnedResourcesForCleanup", baseline, current, claims));
            Property<IDictionary<string, string>>(current, "AliasesBySection").Remove("Box902");
            Property<IList<string>>(current, "Directories").Add("Box902");
            AssertRejected(() => Invoke(safety, "SelectOwnedResourcesForCleanup", baseline, current, claims));
        }

        [TestMethod]
        public void OwnedConfigurationCleanupCanBeRepeatedWithoutWritingAgain()
        {
            string directory = Path.Combine(Path.GetTempPath(), "ShaheAuditRunnerTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, "config.ini");
            byte[] baseline = Encoding.UTF8.GetBytes("baseline");
            try
            {
                File.WriteAllBytes(path, baseline);
                using (var held = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    object result = Invoke(RequireType("AuditSafety"), "RestoreOwnedFile", path,
                        baseline, Sha256(baseline), Sha256(Encoding.UTF8.GetBytes("old audit mutation")));
                    Assert.AreEqual("Passed", Property<object>(result, "Status").ToString());
                }
                CollectionAssert.AreEqual(baseline, File.ReadAllBytes(path));
            }
            finally { Directory.Delete(directory, true); }
        }

        [TestMethod]
        public void LifecycleStateWritePreservesPreviousFileWhenReplacementIsBlocked()
        {
            string directory = Path.Combine(Path.GetTempPath(), "ShaheAuditRunnerTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, "state.json");
            byte[] original = Encoding.UTF8.GetBytes("{\"BaselineId\":\"before\"}");
            MethodInfo write = RequireType("SandboxieLifecycleAudit").GetMethod("WriteLifecycleState", BindingFlags.Static | BindingFlags.NonPublic);
            object state = New(RequireType("LifecycleState"), "BaselineId", "after");
            try
            {
                File.WriteAllBytes(path, original);
                using (var held = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    bool rejected = false;
                    try { write.Invoke(null, new[] { (object)path, state }); }
                    catch (TargetInvocationException exception)
                    {
                        Assert.IsInstanceOfType(exception.InnerException, typeof(IOException));
                        rejected = true;
                    }
                    CollectionAssert.AreEqual(original, File.ReadAllBytes(path));
                    Assert.IsTrue(rejected, "A state file opened without delete sharing cannot be atomically replaced.");
                }
                write.Invoke(null, new[] { (object)path, state });
                StringAssert.Contains(File.ReadAllText(path), "after");
                Assert.AreEqual(1, Directory.GetFiles(directory).Length, "No staging file should remain.");
            }
            finally { Directory.Delete(directory, true); }
        }

        [TestMethod]
        public void CleanupSelectionRequiresExactUniqueUnchangedOwnership()
        {
            Type snapshotType = RequireType("OwnershipSnapshot");
            Type claimType = RequireType("OwnershipClaim");
            Type safety = RequireType("AuditSafety");
            object claim = New(claimType,
                "InternalName", "Box901",
                "Alias", "ShaheAudit_RunA_01",
                "DirectoryName", "Box901");
            object baseline = Snapshot(snapshotType, true,
                new Dictionary<string, string> { { "DefaultBox", string.Empty } },
                new[] { "DefaultBox" });
            object current = Snapshot(snapshotType, true,
                new Dictionary<string, string>
                {
                    { "DefaultBox", string.Empty },
                    { "Box901", "ShaheAudit_RunA_01" },
                    { "Box902", "ShaheAudit_RunA_010" }
                },
                new[] { "DefaultBox", "Box901", "Box902" });
            IList claims = List(claimType, claim);
            IList owned = (IList)Invoke(safety, "SelectOwnedResources", baseline, current, claims);
            Assert.AreEqual(1, owned.Count);
            Assert.AreEqual("Box901", Property<string>(owned[0], "InternalName"));

            object baselineConflict = Snapshot(snapshotType, true,
                new Dictionary<string, string> { { "Box901", "ShaheAudit_RunA_01" } },
                new[] { "Box901" });
            AssertRejected(() => Invoke(safety, "SelectOwnedResources", baselineConflict, current, claims));

            object duplicateAlias = Snapshot(snapshotType, true,
                new Dictionary<string, string>
                {
                    { "Box901", "ShaheAudit_RunA_01" },
                    { "Box999", "ShaheAudit_RunA_01" }
                },
                new[] { "Box901", "Box999" });
            AssertRejected(() => Invoke(safety, "SelectOwnedResources", baseline, duplicateAlias, claims));

            object changedAlias = Snapshot(snapshotType, true,
                new Dictionary<string, string> { { "Box901", "ShaheAudit_RunA_Changed" } },
                new[] { "Box901" });
            AssertRejected(() => Invoke(safety, "SelectOwnedResources", baseline, changedAlias, claims));
            AssertRejected(() => Invoke(
                safety,
                "SelectOwnedResources",
                baseline,
                Snapshot(snapshotType, false, new Dictionary<string, string>(), new string[0]),
                claims));
        }

        [TestMethod]
        public void CleanupSelectionRejectsBaselineSectionWithoutAlias()
        {
            Type snapshotType = RequireType("OwnershipSnapshot");
            Type claimType = RequireType("OwnershipClaim");
            Type safety = RequireType("AuditSafety");
            object claim = New(claimType,
                "InternalName", "Box901",
                "Alias", "ShaheAudit_RunA_01",
                "DirectoryName", "Box901");
            object baseline = Snapshot(snapshotType, true,
                new Dictionary<string, string>(),
                new string[0]);
            Property<IList<string>>(baseline, "Sections").Add("Box901");
            object current = Snapshot(snapshotType, true,
                new Dictionary<string, string> { { "Box901", "ShaheAudit_RunA_01" } },
                new[] { "Box901" });
            Property<IList<string>>(current, "Sections").Add("Box901");

            AssertRejected(() => Invoke(
                safety,
                "SelectOwnedResources",
                baseline,
                current,
                List(claimType, claim)));
        }

        [TestMethod]
        public void CleanupDeletesSectionOnlyAfterSilentDeleteAndDirectoryDisappearance()
        {
            Type claimType = RequireType("OwnershipClaim");
            Type safety = RequireType("AuditSafety");
            object claim = New(claimType,
                "InternalName", "Box901",
                "Alias", "ShaheAudit_RunA_01",
                "DirectoryName", "Box901");
            var calls = new List<string>();
            object result = Invoke(
                safety,
                "DeleteOwnedResource",
                claim,
                new Func<string, int>(name => { calls.Add("delete:" + name); return 0; }),
                new Func<string, bool>(name => { calls.Add("exists:" + name); return false; }),
                new Action<string>(name => calls.Add("section:" + name)));
            Assert.AreEqual("Passed", Property<object>(result, "Status").ToString());
            CollectionAssert.AreEqual(
                new[] { "delete:Box901", "exists:Box901", "section:Box901" },
                calls.ToArray());

            calls.Clear();
            result = Invoke(
                safety,
                "DeleteOwnedResource",
                claim,
                new Func<string, int>(name => { calls.Add("delete:" + name); return 5; }),
                new Func<string, bool>(name => { calls.Add("exists:" + name); return false; }),
                new Action<string>(name => calls.Add("section:" + name)));
            Assert.AreEqual("Inconclusive", Property<object>(result, "Status").ToString());
            CollectionAssert.AreEqual(new[] { "delete:Box901" }, calls.ToArray());

            calls.Clear();
            result = Invoke(
                safety,
                "DeleteOwnedResource",
                claim,
                new Func<string, int>(name => { calls.Add("delete:" + name); return 0; }),
                new Func<string, bool>(name => { calls.Add("exists:" + name); return true; }),
                new Action<string>(name => calls.Add("section:" + name)));
            Assert.AreEqual("Inconclusive", Property<object>(result, "Status").ToString());
            CollectionAssert.AreEqual(new[] { "delete:Box901", "exists:Box901" }, calls.ToArray());
        }

        [TestMethod]
        public void ProbeAndCleanupFailuresAreNeverProductFailuresOrPassed()
        {
            Type failureType = RequireType("AuditFailureKind");
            Type safety = RequireType("AuditSafety");
            foreach (string name in new[]
            {
                "FocusNotConfirmed", "IndexDrift", "WindowInvalid", "Timeout",
                "MenuNotUnique", "CleanupIncomplete"
            })
            {
                object status = Invoke(safety, "ClassifyFailure", Enum.Parse(failureType, name));
                Assert.AreEqual("Inconclusive", status.ToString(), name);
            }
            object productFailure = Enum.Parse(failureType, "ProductFailure");
            Assert.AreEqual("Failed", Invoke(safety, "ClassifyFailure", productFailure).ToString());
        }

        [TestMethod]
        public void LifecycleStagesRequireExactOrderAndBoundState()
        {
            Type stateType = RequireType("LifecycleState");
            Type safety = RequireType("AuditSafety");
            object state = New(stateType,
                "BaselineId", "baseline-one",
                "Prefix", "ShaheAudit_RunOne",
                "CompletedStages", new List<string>());

            Invoke(safety, "ValidateLifecycleStage", state, "baseline-one", "ShaheAudit_RunOne", "batch-create");
            AssertRejected(() => Invoke(
                safety, "ValidateLifecycleStage", state, "baseline-one", "ShaheAudit_RunOne", "create-group"));

            Property<IList<string>>(state, "CompletedStages").Add("batch-create");
            Invoke(safety, "ValidateLifecycleStage", state, "baseline-one", "ShaheAudit_RunOne", "create-group");
            AssertRejected(() => Invoke(
                safety, "ValidateLifecycleStage", state, "baseline-one", "ShaheAudit_RunOne", "batch-create"));
            AssertRejected(() => Invoke(
                safety, "ValidateLifecycleStage", state, "baseline-two", "ShaheAudit_RunOne", "create-group"));
            AssertRejected(() => Invoke(
                safety, "ValidateLifecycleStage", state, "baseline-one", "ShaheAudit_RunTwo", "create-group"));
        }

        [TestMethod]
        public void BatchCreateInitializesBoundLifecycleStateOnlyWhenMissing()
        {
            Type safety = RequireType("AuditSafety");
            string directory = Path.Combine(Path.GetTempPath(), "ShaheAuditRunnerTests", Guid.NewGuid().ToString("N"));
            string path = Path.Combine(directory, "state.json");
            try
            {
                object state = Invoke(
                    safety,
                    "LoadOrCreateLifecycleState",
                    path,
                    "baseline-one",
                    "ShaheAudit_RunOne",
                    "batch-create");
                Assert.IsTrue(File.Exists(path));
                Assert.AreEqual("baseline-one", Property<string>(state, "BaselineId"));
                Assert.AreEqual("ShaheAudit_RunOne", Property<string>(state, "Prefix"));
                Assert.AreEqual(0, Property<IList<string>>(state, "CompletedStages").Count);
                Assert.AreEqual(0, Property<IList>(state, "OwnershipClaims").Count);

                string otherPath = Path.Combine(directory, "other.json");
                AssertRejected(() => Invoke(
                    safety,
                    "LoadOrCreateLifecycleState",
                    otherPath,
                    "baseline-one",
                    "ShaheAudit_RunOne",
                    "create-group"));
                Assert.IsFalse(File.Exists(otherPath));
            }
            finally
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
        }

        [TestMethod]
        public void BatchCreateConfirmationRequiresOwnedForegroundButtonNotification()
        {
            Type automation = RequireType("Win32Automation");
            MethodInfo method = automation.GetMethod(
                "PostButtonClickNotification",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(method, "Batch-create confirmation must notify only an owned foreground button.");

            Stopwatch timer = Stopwatch.StartNew();
            object result = method.Invoke(null, new object[] { IntPtr.Zero, IntPtr.Zero });
            timer.Stop();

            Assert.AreEqual(false, result);
            Assert.IsTrue(timer.Elapsed < TimeSpan.FromSeconds(1));
        }

        [TestMethod]
        [DoNotParallelize]
        public void BatchCreateConfirmationWaitsForTheButtonHandler()
        {
            Form dialog = null;
            Button button = null;
            var ready = new ManualResetEventSlim();
            var clicked = new ManualResetEventSlim();
            var uiThread = new Thread(() =>
            {
                dialog = new Form { Text = "Audit click test" };
                button = new Button { Text = "确定" };
                button.Click += (sender, args) =>
                {
                    Thread.Sleep(200);
                    clicked.Set();
                };
                dialog.Controls.Add(button);
                dialog.Shown += (sender, args) => ready.Set();
                Application.Run(dialog);
            });
            uiThread.SetApartmentState(ApartmentState.STA);
            uiThread.Start();

            try
            {
                Assert.IsTrue(ready.Wait(TimeSpan.FromSeconds(5)), "Test dialog did not open.");
                Type automation = RequireType("Win32Automation");
                MethodInfo foreground = automation.GetMethod(
                    "EnsureForegroundWindow",
                    BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo click = automation.GetMethod(
                    "PostButtonClickNotification",
                    BindingFlags.NonPublic | BindingFlags.Static);
                Assert.IsNotNull(foreground);
                Assert.IsNotNull(click);
                bool submitted = false;
                TimeSpan elapsed = TimeSpan.Zero;
                dialog.Invoke(new Action(() =>
                {
                    for (int attempt = 0; attempt < 3 && !submitted; attempt++)
                    {
                        if (!(bool)foreground.Invoke(null, new object[] { dialog.Handle, 2000 }))
                        {
                            continue;
                        }
                        Stopwatch timer = Stopwatch.StartNew();
                        submitted = (bool)click.Invoke(null, new object[] { dialog.Handle, button.Handle });
                        timer.Stop();
                        if (submitted)
                        {
                            elapsed = timer.Elapsed;
                        }
                    }
                }));

                Assert.IsTrue(submitted, "Owned foreground button notification was not delivered.");
                Assert.IsTrue(clicked.IsSet, "Confirmation returned before the owned button handler completed.");
                Assert.IsTrue(elapsed >= TimeSpan.FromMilliseconds(150));
            }
            finally
            {
                if (dialog != null && dialog.IsHandleCreated)
                {
                    dialog.BeginInvoke(new Action(dialog.Close));
                }
                Assert.IsTrue(uiThread.Join(TimeSpan.FromSeconds(5)), "Test dialog did not close.");
                ready.Dispose();
                clicked.Dispose();
            }
        }

        [TestMethod]
        public void BatchCreateFocusRecoveryRejectsInvalidWindow()
        {
            Type automation = RequireType("Win32Automation");
            MethodInfo method = automation.GetMethod(
                "EnsureForegroundWindow",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(method, "Batch-create focus recovery must verify the target window.");
            Assert.AreEqual(false, method.Invoke(null, new object[] { IntPtr.Zero, 10 }));
        }

        [TestMethod]
        public void WindowTextSnapshotIncludesStatusLabels()
        {
            using (var form = new Form())
            using (var label = new Label { Text = "已创建 1 个沙盒用户" })
            {
                form.Controls.Add(label);
                form.Show();
                Type automation = RequireType("Win32Automation");
                MethodInfo method = automation.GetMethod(
                    "GetNonEmptyWindowTexts",
                    BindingFlags.NonPublic | BindingFlags.Static);
                Assert.IsNotNull(method);
                var texts = (IList<string>)method.Invoke(null, new object[] { form.Handle });
                CollectionAssert.Contains(texts.ToList(), label.Text);
            }
        }

        [TestMethod]
        public void BatchCreateValuesAreSetByAccessibleNamesNotWindowOrder()
        {
            BatchNameRuleDialog dialog = null;
            var selectionChanges = new List<int>();
            var ready = new ManualResetEventSlim();
            var uiThread = new Thread(() =>
            {
                dialog = new BatchNameRuleDialog(
                    null,
                    "批量创建沙盒用户",
                    string.Empty,
                    "王五",
                    8,
                    0,
                    1,
                    null,
                    new[]
                    {
                        new KeyValuePair<int, string>(0, "不加0"),
                        new KeyValuePair<int, string>(2, "前面加0（01）")
                    });
                dialog.DigitsChoice.SelectedIndexChanged += (sender, args) =>
                    selectionChanges.Add(dialog.DigitsChoice.SelectedIndex);
                dialog.Shown += (sender, args) => ready.Set();
                Application.Run(dialog);
            });
            uiThread.SetApartmentState(ApartmentState.STA);
            uiThread.Start();

            try
            {
                Assert.IsTrue(ready.Wait(TimeSpan.FromSeconds(5)), "Test dialog did not open.");
                Type automation = RequireType("Win32Automation");
                MethodInfo method = automation.GetMethod(
                    "SetBatchCreateValuesByAccessibleName",
                    BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo numericInput = automation.GetMethod(
                    "SetNumericSpinnerValueByAccessibleName",
                    BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo findButtons = automation.GetMethod(
                    "FindNumericSpinnerButtonsByAccessibleName",
                    BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo spinnerClick = automation.GetMethod(
                    "SetNumericEditTextAndCommit",
                    BindingFlags.NonPublic | BindingFlags.Static);
                FieldInfo editSelectAll = automation.GetField(
                    "EmSetSel",
                    BindingFlags.NonPublic | BindingFlags.Static);
                FieldInfo editReplaceSelection = automation.GetField(
                    "EmReplaceSel",
                    BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo getParent = automation.GetMethod(
                    "GetParent",
                    BindingFlags.NonPublic | BindingFlags.Static);
                Assert.IsNotNull(method, "Batch-create values must be set by exact accessible names.");
                Assert.IsNotNull(numericInput, "NumericUpDown values must be committed through its named spinner edit.");
                Assert.IsNotNull(findButtons);
                Assert.IsNotNull(spinnerClick, "Numeric input must commit the exact UpDownEdit text through a real focus change.");
                Assert.IsNotNull(editSelectAll, "Numeric input must select the native Edit text before replacing it.");
                Assert.IsNotNull(editReplaceSelection, "Numeric input must replace the native Edit selection as a user edit.");
                Assert.AreEqual(0x00B1, (int)editSelectAll.GetRawConstantValue());
                Assert.AreEqual(0x00C2, (int)editReplaceSelection.GetRawConstantValue());
                Assert.IsNotNull(getParent);
                IntPtr actualStartEdit = dialog.StartInput.Controls.Cast<Control>()
                    .Single(control => control.GetType().Name == "UpDownEdit").Handle;
                Assert.AreEqual(
                    dialog.StartInput.Handle,
                    (IntPtr)getParent.Invoke(null, new object[] { actualStartEdit }),
                    "The native edit parent must be the NumericUpDown window.");
                IntPtr actualStartButtons = dialog.StartInput.Controls.Cast<Control>()
                    .Single(control => control.GetType().Name == "UpDownButtons").Handle;
                Assert.AreEqual(
                    actualStartButtons,
                    (IntPtr)findButtons.Invoke(null, new object[] { dialog.Handle, "起始编号" }),
                    "Runner must resolve the exact UpDownButtons child window.");
                MethodInfo trySet = automation.GetMethod(
                    "TrySetBatchCreateValuesByAccessibleName", BindingFlags.NonPublic | BindingFlags.Static);
                Assert.IsNotNull(trySet);
                var arguments = new object[] { dialog.Handle, "ShaheAudit_Test_", 1, 1, 2, null };
                bool updated = (bool)trySet.Invoke(null, arguments);
                string failure = Convert.ToString(arguments[5]);

                object[] values = (object[])dialog.Invoke(new Func<object[]>(() => new object[]
                {
                    dialog.PrefixTextBox.Text,
                    Decimal.ToInt32(dialog.StartInput.Value),
                    dialog.DigitsChoice.SelectedIndex,
                    Decimal.ToInt32(dialog.CountInput.Value)
                }));
                int[] observedSelections = (int[])dialog.Invoke(new Func<int[]>(() => selectionChanges.ToArray()));
                CollectionAssert.Contains(observedSelections, 1,
                    "Native selection must notify the managed control. Updated=" + updated + "; " + failure);
                CollectionAssert.AreEqual(
                    new object[] { "ShaheAudit_Test_", 1, 1, 2 },
                    values,
                    "Updated=" + updated + "; Failure=" + failure + "; Actual: " + string.Join(" | ", values.Select(value => value == null ? "<null>" : value.ToString()))
                    + "; Start children: "
                    + string.Join(", ", dialog.StartInput.Controls.Cast<Control>().Select(control =>
                        control.GetType().FullName + "=0x" + control.Handle.ToInt64().ToString("X")))
                    + "; Count children: "
                    + string.Join(", ", dialog.CountInput.Controls.Cast<Control>().Select(control =>
                        control.GetType().FullName + "=0x" + control.Handle.ToInt64().ToString("X"))));
                Assert.IsTrue(updated, "Semantic values changed but the Runner did not verify them.");
            }
            finally
            {
                if (dialog != null && dialog.IsHandleCreated)
                {
                    dialog.BeginInvoke(new Action(dialog.Close));
                }
                Assert.IsTrue(uiThread.Join(TimeSpan.FromSeconds(5)), "Test dialog did not close.");
                ready.Dispose();
            }
        }

        [TestMethod]
        public void BatchCreatePlanUsesOriginalShaheOfficialPath()
        {
            Type audit = RequireType("SandboxieLifecycleAudit");
            MethodInfo method = audit.GetMethod(
                "BuildBatchCreateAliases",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(method);
            MethodInfo create = audit.GetMethod(
                "CreateOwnedSandboxViaOfficial",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(create);

            var aliases = (string[])method.Invoke(
                null,
                new object[] { "ShaheAudit_Test" });

            CollectionAssert.AreEqual(
                new[] { "ShaheAudit_Test_Alias-01", "ShaheAudit_Test_Alias-02" },
                aliases);
        }

        [TestMethod]
        public void CreateGroupPreservesExistingSettingsAndUsesExactOwnedMembers()
        {
            Type safety = RequireType("AuditSafety");
            var settings = new Dictionary<string, object>
            {
                { "unchanged", "keep" },
                {
                    "sandbox_groups",
                    new Dictionary<string, object>
                    {
                        { "existing", new List<string> { "UserOne" } }
                    }
                }
            };

            var updated = (IDictionary<string, object>)Invoke(
                safety,
                "AddOwnedGroupSettings",
                settings,
                "ShaheAudit_RunOne_Group",
                new[] { "ShaheAudit_RunOne_Alias-01", "ShaheAudit_RunOne_Alias-02" });

            Assert.AreEqual("keep", updated["unchanged"]);
            var groups = (IDictionary<string, object>)updated["sandbox_groups"];
            CollectionAssert.AreEqual(
                new[] { "UserOne" },
                ((IEnumerable<string>)groups["existing"]).ToArray());
            CollectionAssert.AreEqual(
                new[] { "ShaheAudit_RunOne_Alias-01", "ShaheAudit_RunOne_Alias-02" },
                ((IEnumerable<string>)groups["ShaheAudit_RunOne_Group"]).ToArray());
        }

        [TestMethod]
        public void OwnedConfigurationRestoreRequiresExactExpectedHash()
        {
            Type safety = RequireType("AuditSafety");
            string directory = Path.Combine(Path.GetTempPath(), "ShaheAuditRunnerTests", Guid.NewGuid().ToString("N"));
            string path = Path.Combine(directory, "shahe_config.ini");
            byte[] baseline = Encoding.UTF8.GetBytes("baseline");
            byte[] owned = Encoding.UTF8.GetBytes("owned");
            byte[] drifted = Encoding.UTF8.GetBytes("drifted");
            Directory.CreateDirectory(directory);
            try
            {
                File.WriteAllBytes(path, owned);
                object result = Invoke(
                    safety,
                    "RestoreOwnedFile",
                    path,
                    baseline,
                    Sha256(baseline),
                    Sha256(owned));
                Assert.AreEqual("Passed", Property<object>(result, "Status").ToString());
                CollectionAssert.AreEqual(baseline, File.ReadAllBytes(path));

                File.WriteAllBytes(path, drifted);
                result = Invoke(
                    safety,
                    "RestoreOwnedFile",
                    path,
                    baseline,
                    Sha256(baseline),
                    Sha256(owned));
                Assert.AreEqual("Inconclusive", Property<object>(result, "Status").ToString());
                CollectionAssert.AreEqual(drifted, File.ReadAllBytes(path));
            }
            finally
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
        }

        [TestMethod]
        public void LaunchGroupSelectsOnlyExactBoundGroupClaims()
        {
            Type stateType = RequireType("LifecycleState");
            Type claimType = RequireType("OwnershipClaim");
            Type safety = RequireType("AuditSafety");
            object first = New(claimType,
                "InternalName", "Box901",
                "Alias", "ShaheAudit_RunOne_Alias-01",
                "DirectoryName", "Box901");
            object second = New(claimType,
                "InternalName", "Box902",
                "Alias", "ShaheAudit_RunOne_Alias-02",
                "DirectoryName", "Box902");
            object state = New(stateType,
                "Prefix", "ShaheAudit_RunOne",
                "GroupName", "ShaheAudit_RunOne_Group",
                "GroupMembers", new List<string> { "ShaheAudit_RunOne_Alias-01", "ShaheAudit_RunOne_Alias-02" },
                "OwnershipClaims", List(claimType, first, second));

            IList selected = (IList)Invoke(safety, "SelectOwnedGroupClaims", state);
            Assert.AreEqual(2, selected.Count);
            Assert.AreEqual("Box901", Property<string>(selected[0], "InternalName"));
            Assert.AreEqual("Box902", Property<string>(selected[1], "InternalName"));

            Property<IList<string>>(state, "GroupMembers")[1] = "ShaheAudit_RunOne_Alias-020";
            AssertRejected(() => Invoke(safety, "SelectOwnedGroupClaims", state));
        }

        [TestMethod]
        public void LifecycleSchedulingStagesAreImplementedByCompiledRunner()
        {
            Type audit = RequireType("SandboxieLifecycleAudit");
            foreach (string name in new[]
            {
                "RunLifecycleScheduleLaunch",
                "RunLifecycleScheduleClose",
                "RunLifecycleCancelSchedules"
            })
            {
                Assert.IsNotNull(
                    audit.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static),
                    name + " must exist");
            }
        }

        [TestMethod]
        public void LifecycleRenameAndDeleteStagesAreImplementedByCompiledRunner()
        {
            Type audit = RequireType("SandboxieLifecycleAudit");
            Assert.IsNotNull(audit.GetMethod("RunLifecycleBatchRename", BindingFlags.NonPublic | BindingFlags.Static));
            Assert.IsNotNull(audit.GetMethod("RunLifecycleBatchDelete", BindingFlags.NonPublic | BindingFlags.Static));

            Type stateType = RequireType("LifecycleState");
            Type claimType = RequireType("OwnershipClaim");
            Type safety = RequireType("AuditSafety");
            object state = New(stateType,
                "Prefix", "ShaheAudit_RunOne",
                "GroupName", "ShaheAudit_RunOne_Group",
                "GroupMembers", new List<string> { "ShaheAudit_RunOne_Renamed 01", "ShaheAudit_RunOne_Renamed 02" },
                "CompletedStages", new List<string>
                {
                    "batch-create", "create-group", "launch-group", "schedule-launch",
                    "schedule-close", "cancel-schedules", "batch-rename"
                },
                "OwnershipClaims", List(claimType,
                    New(claimType, "InternalName", "Box901", "Alias", "ShaheAudit_RunOne_Renamed 01", "DirectoryName", "Box901"),
                    New(claimType, "InternalName", "Box902", "Alias", "ShaheAudit_RunOne_Renamed 02", "DirectoryName", "Box902")));
            IList selected = (IList)Invoke(safety, "SelectOwnedGroupClaims", state);
            Assert.AreEqual(2, selected.Count);
        }

        [TestMethod]
        public void CleanupUsesASeparateNonOwningProcessGate()
        {
            Type audit = RequireType("SandboxieLifecycleAudit");
            Assert.IsNotNull(
                audit.GetMethod("EnsureCleanupProcessGate", BindingFlags.NonPublic | BindingFlags.Static),
                "Cleanup must not reuse the lifecycle gate that rejects unrelated SbieCtrl processes.");
        }

        [TestMethod]
        public void RecoveryCleanupRequiresBoundStateWithExactClaims()
        {
            Type safety = RequireType("AuditSafety");
            string directory = Path.Combine(Path.GetTempPath(), "ShaheAuditRunnerTests", Guid.NewGuid().ToString("N"));
            string path = Path.Combine(directory, "recovery-state.json");
            Directory.CreateDirectory(directory);
            try
            {
                File.WriteAllText(path,
                    "{\"BaselineId\":\"baseline-one\",\"Prefix\":\"ShaheAudit_RunOne\"," +
                    "\"CompletedStages\":[],\"OwnershipClaims\":[{" +
                    "\"InternalName\":\"Box54\",\"Alias\":\"王五8\",\"DirectoryName\":\"Box54\"}]}");

                object state = Invoke(
                    safety,
                    "LoadLifecycleStateForCleanup",
                    path,
                    "baseline-one",
                    "ShaheAudit_RunOne");
                Assert.AreEqual(1, Property<IList>(state, "OwnershipClaims").Count);
                AssertRejected(() => Invoke(
                    safety,
                    "LoadLifecycleStateForCleanup",
                    path,
                    "baseline-two",
                    "ShaheAudit_RunOne"));
                AssertRejected(() => Invoke(
                    safety,
                    "LoadLifecycleStateForCleanup",
                    path,
                    "baseline-one",
                    "ShaheAudit_RunTwo"));

                File.WriteAllText(path,
                    "{\"BaselineId\":\"baseline-one\",\"Prefix\":\"ShaheAudit_RunOne\"," +
                    "\"CompletedStages\":[],\"OwnershipClaims\":[]}");
                AssertRejected(() => Invoke(
                    safety,
                    "LoadLifecycleStateForCleanup",
                    path,
                    "baseline-one",
                    "ShaheAudit_RunOne"));
            }
            finally
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
        }

        [TestMethod]
        public void OwnedResourceNamesPreserveExactCurrentCasing()
        {
            Type safety = RequireType("AuditSafety");
            Assert.AreEqual(
                "ShaheAudit_20261017_16091",
                Invoke(
                    safety,
                    "FindUniqueCurrentName",
                    new[] { "Box53", "ShaheAudit_20261017_16091" },
                    "shaheaudit_20261017_16091"));
            AssertRejected(() => Invoke(
                safety,
                "FindUniqueCurrentName",
                new[] { "ShaheAudit_Test", "shaheaudit_test" },
                "SHAHEAUDIT_TEST"));
            AssertRejected(() => Invoke(
                safety,
                "FindUniqueCurrentName",
                new[] { "Box53" },
                "ShaheAudit_Missing"));
        }

        [TestMethod]
        public void BatchCreateObservationAllowsThirtySecondsPerSandbox()
        {
            Type audit = RequireType("SandboxieLifecycleAudit");
            MethodInfo method = audit.GetMethod(
                "GetBatchCreateObservationTimeoutMilliseconds",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(method);
            Assert.AreEqual(60000, method.Invoke(null, new object[] { 2 }));
        }

        private static object Snapshot(
            Type type,
            bool readable,
            IDictionary<string, string> sections,
            IEnumerable<string> directories)
        {
            return New(type,
                "IniReadable", readable,
                "AliasesBySection", sections,
                "Directories", directories.ToList());
        }

        private static Type RequireType(string name)
        {
            Type type = Assembly.Load(AssemblyName).GetType("ShaheLauncher.AuditRunner." + name, false);
            Assert.IsNotNull(type, name + " must exist");
            return type;
        }

        private static string Sha256(byte[] value)
        {
            using (SHA256 algorithm = SHA256.Create())
            {
                return BitConverter.ToString(algorithm.ComputeHash(value)).Replace("-", string.Empty);
            }
        }

        private static void AssertRejected(Action action)
        {
            try
            {
                action();
                Assert.Fail("Expected invocation to be rejected");
            }
            catch (TargetInvocationException exception)
            {
                Assert.IsInstanceOfType(exception.InnerException, typeof(InvalidOperationException));
            }
        }

        private static object Invoke(Type type, string name, params object[] args)
        {
            MethodInfo method = type.GetMethod(name, BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(method, name + " must be public static");
            ParameterInfo[] parameters = method.GetParameters();
            if (args.Length < parameters.Length && parameters.Skip(args.Length).All(parameter => parameter.IsOptional))
                args = args.Concat(parameters.Skip(args.Length).Select(parameter => parameter.DefaultValue)).ToArray();
            return method.Invoke(null, args);
        }

        private static object New(Type type, params object[] values)
        {
            object result = Activator.CreateInstance(type);
            for (int index = 0; index < values.Length; index += 2)
            {
                string name = (string)values[index];
                PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
                Assert.IsNotNull(property, name + " must be a public property");
                property.SetValue(result, values[index + 1]);
            }
            return result;
        }

        private static IList List(Type itemType, params object[] values)
        {
            IList result = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType));
            foreach (object value in values)
            {
                result.Add(value);
            }
            return result;
        }

        private static T Property<T>(object target, string name)
        {
            PropertyInfo property = target.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(property, name + " must be a public property");
            return (T)property.GetValue(target);
        }
    }
}
