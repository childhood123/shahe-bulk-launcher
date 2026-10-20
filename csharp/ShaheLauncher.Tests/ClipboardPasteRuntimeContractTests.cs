using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaheLauncher.App;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("Application")]
    public sealed class ClipboardPasteRuntimeContractTests
    {
        [DataTestMethod]
        [DataRow("files-first")]
        [DataRow("text-fallback")]
        [DataRow("invalid")]
        [DataRow("empty")]
        public void PasteActionResolvesClipboardDataLikePython(string scenario)
        {
            MigrationRuntimeTestHelpers.WithForm((root, form) =>
            {
                var data = new DataObject();
                string filePath = Path.Combine(root, "文件应用.exe");
                string textPath = Path.Combine(root, "文本应用.exe");
                File.WriteAllText(filePath, "test");
                File.WriteAllText(textPath, "test");
                if (scenario == "files-first") data.SetData(DataFormats.FileDrop, new[] { filePath });
                if (scenario == "files-first" || scenario == "text-fallback") data.SetText(textPath);
                if (scenario == "invalid") data.SetText("not an application path");
                PropertyInfo reader = typeof(MainForm).GetProperty("ClipboardDataReader");
                Assert.IsNotNull(reader, "粘贴动作必须接入可读取文件列表和文本的剪贴板入口");
                Assert.IsNotNull(reader.GetValue(form), "正常构造路径必须连接真实剪贴板读取");
                reader.SetValue(form, new Func<IDataObject>(() => data));
                form.CreateMenuBarCallback = null;
                form.InitializeUi();
                form.AppPathList.SetValue("原有路径");

                MigrationRuntimeTestHelpers.Invoke(form, "ExecuteApplicationAction", "paste_app");

                if (scenario == "files-first" || scenario == "text-fallback")
                {
                    string expected = scenario == "files-first" ? filePath : textPath;
                    Assert.AreEqual(expected, form.LaunchState.AppPath);
                    StringAssert.Contains(form.AppPathList.GetValue(), expected);
                    Assert.AreEqual("已从剪贴板粘贴应用: " + Path.GetFileName(expected), form.StatusLabel.Text);
                }
                else
                {
                    Assert.AreEqual("原有路径", form.AppPathList.GetValue());
                    Assert.AreEqual("剪贴板没有包含有效的数据", form.StatusLabel.Text);
                }
            });
        }
    }
}
