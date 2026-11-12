using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaheLauncher.App;
using ShaheLauncher.Core.Backup;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Application")]
    [TestCategory("BackupRestore")]
    public sealed class RestoreServiceLifecycleRuntimeContractTests
    {
        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void RestoreQueriesExplicitServiceStateOnlyWhenCoreRequestsIt(bool running)
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                int queries = 0;
                SetServiceQuery(form, () =>
                {
                    queries++;
                    return running;
                });

                BackupRestoreCallbacks callbacks = CreateCallbacks(form);

                Assert.AreEqual(0, queries, "构造回调不得查询正式服务。");
                Func<bool> query = GetServiceQuery(callbacks);
                Assert.AreEqual(running, query());
                Assert.AreEqual(1, queries, "Core 应获得真实注入的初态，不能固定返回 true。");
            });
        }

        [TestMethod]
        public void RestoreServiceQueryFailureReachesCoreWithoutBecomingStoppedOrRunning()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                var failure = new InvalidOperationException("service query denied");
                SetServiceQuery(form, () => { throw failure; });
                Func<bool> query = GetServiceQuery(CreateCallbacks(form));

                Assert.AreSame(failure, Assert.ThrowsException<InvalidOperationException>(() => query()));
            });
        }

        [DataTestMethod]
        [DataRow("stop")]
        [DataRow("reload")]
        [DataRow("restart")]
        public void RestoreLifecycleCallbackFailureReachesCoreUnchanged(string operation)
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                var failure = new InvalidOperationException(operation + " denied");
                int calls = 0;
                Action fail = () =>
                {
                    calls++;
                    throw failure;
                };
                form.StopSandboxieServicesCallback = fail;
                form.ReloadSandboxieConfigCallback = fail;
                form.RestartSandboxieServicesCallback = fail;
                BackupRestoreCallbacks callbacks = CreateCallbacks(form);
                Action action = operation == "stop" ? callbacks.StopSandboxieServices
                    : operation == "reload" ? callbacks.ReloadSandboxieConfig
                    : callbacks.RestartSandboxieServices;

                Assert.AreSame(failure, Assert.ThrowsException<InvalidOperationException>(action));
                Assert.AreEqual(1, calls);
            });
        }

        [DataTestMethod]
        [DataRow(false, "null")]
        [DataRow(false, "nonzero")]
        [DataRow(false, "exception")]
        [DataRow(true, "null")]
        [DataRow(true, "nonzero")]
        [DataRow(true, "exception")]
        public void RestoreDefaultReloadReportsCommandFailureWithDefaultOrNullCallback(
            bool clearReloadCallback, string outcome)
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                form.SandboxieClient.SandboxiePath = Path.Combine(root, "missing-sandboxie");
                if (clearReloadCallback) form.ReloadSandboxieConfigCallback = null;
                int calls = 0;
                var failure = new IOException("isolated reload runner failed");
                form.ProcessCallbacks.RunSandboxieStart = arguments =>
                {
                    CollectionAssert.AreEqual(new[] { "/reload" }, arguments.ToArray());
                    calls++;
                    if (outcome == "exception") throw failure;
                    return outcome == "null" ? null : Tuple.Create(37, "isolated reload refused");
                };

                Exception error = CaptureFailure(CreateCallbacks(form).ReloadSandboxieConfig);

                Assert.AreEqual(1, calls, "还原必须使用可检查结果的运行回调，包括构造器已接入旧 reload 的情形。");
                if (outcome == "exception")
                {
                    Assert.AreSame(failure, error);
                }
                else
                {
                    Assert.IsInstanceOfType(error, typeof(InvalidOperationException));
                    if (outcome == "nonzero")
                    {
                        StringAssert.Contains(error.Message, "37");
                        StringAssert.Contains(error.Message, "isolated reload refused");
                    }
                }
            });
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void RestoreDefaultReloadAcceptsOnlyConfirmedSuccessfulCommand(bool clearReloadCallback)
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                form.SandboxieClient.SandboxiePath = Path.Combine(root, "missing-sandboxie");
                if (clearReloadCallback) form.ReloadSandboxieConfigCallback = null;
                var calls = new List<string>();
                form.ProcessCallbacks.RunSandboxieStart = arguments =>
                {
                    calls.AddRange(arguments);
                    return Tuple.Create(0, string.Empty);
                };

                CreateCallbacks(form).ReloadSandboxieConfig();

                CollectionAssert.AreEqual(new[] { "/reload" }, calls);
            });
        }

        [TestMethod]
        public void RestoreKeepsExplicitReloadOverrideAndDoesNotInvokeDefaultRunner()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                int reloads = 0;
                form.ReloadSandboxieConfigCallback = () => reloads++;
                form.ProcessCallbacks.RunSandboxieStart = arguments =>
                {
                    Assert.Fail("明确注入 reload 后不得启动默认命令。");
                    return null;
                };

                CreateCallbacks(form).ReloadSandboxieConfig();

                Assert.AreEqual(1, reloads);
            });
        }

        [TestMethod]
        public void RestoreMissingExecutableFailsWhileUnrelatedReloadKeepsExistingBehavior()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                form.SandboxieClient.SandboxiePath = Path.Combine(root, "missing-sandboxie");
                Action normalReload = form.ReloadSandboxieConfigCallback;
                Assert.IsNotNull(normalReload);
                Assert.AreSame(form, normalReload.Target);
                Assert.AreEqual("ReloadSandboxieConfig", normalReload.Method.Name);

                BackupRestoreCallbacks callbacks = CreateCallbacks(form);

                Assert.AreSame(normalReload, form.ReloadSandboxieConfigCallback,
                    "创建还原回调不得替换其他入口共享的 reload。");
                normalReload();
                Assert.ThrowsException<InvalidOperationException>(callbacks.ReloadSandboxieConfig);
            });
        }

        private static BackupRestoreCallbacks CreateCallbacks(MainForm form)
        {
            return (BackupRestoreCallbacks)MigrationRuntimeTestHelpers.Invoke(
                form, "CreateBackupRestoreCallbacks", (object)null);
        }

        private static void SetServiceQuery(MainForm form, Func<bool> query)
        {
            PropertyInfo property = typeof(MainForm).GetProperty("IsSandboxieServiceRunningCallback");
            Assert.IsNotNull(property, "主窗口必须提供显式状态查询注入，测试不访问正式服务。");
            Assert.AreEqual(typeof(Func<bool>), property.PropertyType);
            property.SetValue(form, query);
        }

        private static Func<bool> GetServiceQuery(BackupRestoreCallbacks callbacks)
        {
            PropertyInfo property = typeof(BackupRestoreCallbacks).GetProperty("IsSandboxieServiceRunning");
            Assert.IsNotNull(property, "Core 必须接收原服务状态查询。");
            var query = property.GetValue(callbacks) as Func<bool>;
            Assert.IsNotNull(query, "主窗口的真实接线不能使用 Core 的 null 兼容默认值。");
            return query;
        }

        private static Exception CaptureFailure(Action action)
        {
            try { action(); }
            catch (Exception error) { return error; }
            Assert.Fail("还原必须报告命令执行失败，不能继续返回成功。");
            return null;
        }

    }
}
