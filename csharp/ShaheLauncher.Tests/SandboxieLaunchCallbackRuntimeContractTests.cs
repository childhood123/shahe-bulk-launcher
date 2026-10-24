using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaheLauncher.App;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Application")]
    [TestCategory("Sandboxie")]
    public sealed class SandboxieLaunchCallbackRuntimeContractTests
    {
        [TestMethod]
        public void MainFormLaunchCallbacksExposeReloadAndOfficialNameRefresh()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                form.InitializeUi();
                int reloads = 0;
                form.ReloadSandboxieConfigCallback = () => reloads++;

                MethodInfo factory = typeof(MainForm).GetMethod(
                    "CreateSandboxieLaunchCallbacks",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.IsNotNull(factory, "启动 Sandboxie 回调工厂必须存在");
                object callbacks = factory.Invoke(form, null);
                Assert.IsNotNull(callbacks);

                PropertyInfo reload = callbacks.GetType().GetProperty("ReloadConfiguration");
                PropertyInfo official = callbacks.GetType().GetProperty("ListOfficialSandboxNames");
                Assert.IsNotNull(reload);
                Assert.IsNotNull(official);
                Assert.IsNotNull(reload.GetValue(callbacks));
                Assert.IsNotNull(official.GetValue(callbacks));

                ((Action)reload.GetValue(callbacks)).Invoke();
                Assert.AreEqual(1, reloads);
                var names = (IList<string>)((Func<IList<string>>)official.GetValue(callbacks)).Invoke();
                Assert.IsNotNull(names);
            });
        }

        [TestMethod]
        public void OfficialNameRetryCallbackDiscardsInitializedCache()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                form.InitializeUi();
                FieldInfo namesField = typeof(MainForm).GetField(
                    "_officialSandboxNames",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                FieldInfo initializedField = typeof(MainForm).GetField(
                    "_sandboxDisplayMapsInitialized",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.IsNotNull(namesField);
                Assert.IsNotNull(initializedField);
                namesField.SetValue(form, new List<string> { "OldBox" });
                initializedField.SetValue(form, true);
                form.SandboxieClient.SandboxiePath = Path.Combine(root, "missing-sandboxie");
                form.SandboxieClient.SandboxRoot = Path.Combine(root, "missing-sandboxes");

                MethodInfo factory = typeof(MainForm).GetMethod(
                    "CreateSandboxieLaunchCallbacks",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                object callbacks = factory.Invoke(form, null);
                var official = (Func<IList<string>>)callbacks.GetType()
                    .GetProperty("ListOfficialSandboxNames")
                    .GetValue(callbacks);

                CollectionAssert.AreEqual(new string[0], official().ToArray());
            });
        }
    }
}
