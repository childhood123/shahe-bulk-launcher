using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ShaheLauncher.Tests
{
    [TestClass, TestCategory("Application"), TestCategory("AppPaths")]
    public sealed class AppPathDialogTextRuntimeContractTests
    {
        [TestMethod]
        public void BrowseDialogRetainsPythonTitleAndExeOnlyFilter()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                var method = form.GetType().GetMethod("CreateApplicationPathDialog", BindingFlags.Static | BindingFlags.NonPublic);
                Assert.IsNotNull(method, "选择应用对话框必须可独立核对配置");
                using (var dialog = (OpenFileDialog)method.Invoke(null, null))
                {
                    Assert.AreEqual("选择要在沙盒中打开的应用", dialog.Title);
                    Assert.AreEqual("可执行文件 (*.exe)|*.exe", dialog.Filter);
                    Assert.IsTrue(dialog.CheckFileExists);
                    Assert.IsFalse(dialog.Multiselect);
                    Assert.AreEqual(Environment.CurrentDirectory, dialog.InitialDirectory);
                }
            });
        }

        [TestMethod]
        public void SelectingExistingApplicationRetainsSuccessText()
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                form.InitializeUi();
                string path = Path.Combine(root, "测试.exe"); File.WriteAllText(path, "test");
                MigrationRuntimeTestHelpers.Invoke(form, "AddApplicationPath", path);
                Assert.AreEqual("已选择: 测试.exe", form.StatusLabel.Text);
            });
        }

        [DataTestMethod]
        [DataRow(false, "无法访问剪贴板")]
        [DataRow(true, "粘贴文件时出错: 数据读取失败")]
        public void PasteDistinguishesAccessFailureAndDataException(bool dataFailure, string expected)
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                form.InitializeUi();
                form.ClipboardDataReader = () => { if (dataFailure) throw new InvalidOperationException("数据读取失败"); throw new ExternalException("clipboard locked"); };
                MigrationRuntimeTestHelpers.Invoke(form, "PasteApplicationPath");
                Assert.AreEqual(expected, form.StatusLabel.Text);
            });
        }

        [DataTestMethod]
        [DataRow("match", "路径已复制到剪贴板", 1)]
        [DataRow("retry", "路径已复制到剪贴板", 3)]
        [DataRow("mismatch", "复制路径失败", 5)]
        [DataRow("read-error", "复制路径失败", 5)]
        [DataRow("write-error", "复制路径失败", 0)]
        [DataRow("access-error", "无法访问剪贴板", 0)]
        [DataRow("no-writer", "复制路径失败", 0)]
        public void CopyRequiresExactReadbackAndRetries(string scenario, string expected, int expectedReads)
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                form.InitializeUi();
                string path = Path.Combine(root, "测试.exe"); File.WriteAllText(path, "test");
                MigrationRuntimeTestHelpers.Invoke(form, "AddApplicationPath", path);
                int reads = 0; string written = null;
                form.ClipboardTextWriter = value =>
                {
                    written = value;
                    if (scenario == "write-error") throw new InvalidOperationException("write failed");
                    if (scenario == "access-error") throw new ExternalException("clipboard locked");
                };
                if (scenario == "no-writer") form.ClipboardTextWriter = null;
                form.ClipboardDataReader = () =>
                {
                    reads++;
                    if (scenario == "read-error") throw new InvalidOperationException("read failed");
                    var data = new DataObject();
                    data.SetData(DataFormats.UnicodeText, scenario == "match" || scenario == "retry" && reads == 3 ? path : "different");
                    return data;
                };
                MigrationRuntimeTestHelpers.Invoke(form, "CopyCurrentAppPath");
                Assert.AreEqual(expected, form.StatusLabel.Text);
                Assert.AreEqual(expectedReads, reads);
                if (scenario != "no-writer") Assert.AreEqual(path, written);
            });
        }
    }
}
