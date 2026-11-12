using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaheLauncher.Core.Updates;

namespace ShaheLauncher.Tests
{
    [TestClass, TestCategory("Updates")]
    public sealed class UpdateManifestAndMenuCleanupContractTests
    {
        [DataTestMethod]
        [DataRow("missing", "9999.0.0")]
        [DataRow("null", "9999.0.0")]
        [DataRow("empty", "9999.0.0")]
        [DataRow("present", "9998.0.0")]
        public void ResolvesEmptyPrimaryVersionWithoutOverridingNonemptyPrimary(string mode, string expected)
        {
            string root = Path.Combine(Path.GetTempPath(), "shahe-manifest-" + Guid.NewGuid().ToString("N"));
            using (var service = new UpdateService(root, false, string.Empty, null, null))
            {
                var manifest = new Dictionary<string, object>
                {
                    { "version", "9999.0.0" },
                    { "package", new Dictionary<string, object>
                        { { "url", "https://example.invalid/package.zip" }, { "kind", "full" },
                          { "sha256", new string('a', 64) }, { "size", 128 } } }
                };
                if (mode != "missing") manifest["latest_version"] = mode == "null" ? null : mode == "empty" ? "" : "9998.0.0";
                UpdateDisplayResult result = service.BuildUpdateDisplayResult(manifest, false, false);
                Assert.AreEqual(expected, result.LatestVersion);
                Assert.AreEqual(string.Empty, result.ErrorText);
                Assert.IsTrue(result.HasUpdate);
                Assert.IsTrue(result.ShouldPrompt);
                Assert.IsNotNull(result.Package);
            }
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void MissingVersionReportsOnlyManualFailureThroughMainForm(bool silent)
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                form.InitializeUi();
                form.StatusLabel.Text = "原状态";
                var messages = new List<string>();
                form.ShowMessageCallback = (text, title, icon) => messages.Add(text);
                using (var service = new UpdateService(root, false, string.Empty, null, null))
                {
                    UpdateDisplayResult result = service.BuildUpdateDisplayResult(new Dictionary<string, object>(), silent, false);
                    Assert.AreEqual(silent ? "" : "更新清单缺少 latest_version", result.ErrorText);
                    Assert.IsFalse(result.HasUpdate);
                    Assert.IsFalse(result.ShouldShowMessage);
                    Assert.IsFalse(result.ShouldShowNotice);
                    Assert.IsFalse(result.ShouldPrompt);
                    Assert.IsNull(result.Package);
                    form.ShowUpdateNotice(result);
                    Assert.AreEqual(silent ? 0 : 1, messages.Count);
                    if (silent) Assert.AreEqual("原状态", form.StatusLabel.Text);
                    else StringAssert.Contains(messages[0], "更新清单缺少 latest_version");
                }
            });
        }

        [TestMethod]
        public void DisposingFormImmediatelyReleasesCurrentPathMenuAndChildrenOnce()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                form.InitializeUi();
                ContextMenu menu = form.AppPathList.ContextMenu;
                Assert.IsNotNull(menu);
                Assert.IsTrue(menu.MenuItems.Count > 0);
                MenuItem child = menu.MenuItems[0];
                int menuDisposed = 0, childDisposed = 0;
                menu.Disposed += (sender, args) => menuDisposed++;
                child.Disposed += (sender, args) => childDisposed++;
                IntPtr handle = menu.Handle;
                Assert.IsTrue(IsMenu(handle));
                try
                {
                    form.Dispose();
                    Assert.IsFalse(IsMenu(handle), "A strong menu reference must not retain its native HMENU after form disposal.");
                    Assert.AreEqual(1, menuDisposed);
                    Assert.AreEqual(1, childDisposed);
                    form.Dispose();
                    Assert.AreEqual(1, menuDisposed);
                    Assert.AreEqual(1, childDisposed);
                }
                finally { if (menuDisposed == 0) menu.Dispose(); }
                GC.KeepAlive(menu);
            });
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsMenu(IntPtr handle);
    }
}
