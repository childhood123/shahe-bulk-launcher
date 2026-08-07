using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("CommonDialogs")]
    public sealed class CommonDialogsCSharpContractTests
    {
        private const string Namespace = "ShaheLauncher.App.UI.Dialogs.Common.";

        [TestMethod]
        public void ImageUtilitiesLoadBytesAndCopyWithExactMessages()
        {
            RunInSta(() =>
            {
                Type messagesType = RequireType("DialogMessageService");
                Type imagesType = RequireType("DialogImageUtilities");
                var messages = new List<string>();
                SetStaticProperty(
                    messagesType,
                    "Handler",
                    new Action<string, string, MessageBoxIcon>((text, title, icon) => messages.Add(title + "|" + text + "|" + (int)icon)));
                string directory = CreateTempDirectory();
                string imagePath = Path.Combine(directory, "二维码.png");
                try
                {
                    byte[] bytes = CreatePngBytes(8, 6);
                    using (var image = (Image)ReflectionTestHelpers.InvokeStatic(imagesType, "LoadImageFromBytes", (object)bytes))
                    {
                        Assert.AreEqual(new Size(8, 6), image.Size);
                    }
                    File.WriteAllBytes(imagePath, bytes);
                    Assert.AreEqual(true, ReflectionTestHelpers.InvokeStatic(imagesType, "CopyImageToClipboard", imagePath, null));
                    Assert.IsTrue(Clipboard.ContainsImage());
                    Assert.AreEqual("成功|二维码已复制到剪贴板！|64", messages.Last());

                    string missing = Path.Combine(directory, "missing.png");
                    Assert.AreEqual(false, ReflectionTestHelpers.InvokeStatic(imagesType, "CopyImageToClipboard", missing, null));
                    Assert.AreEqual("错误|未找到图片文件：" + missing + "|16", messages.Last());
                }
                finally
                {
                    SetStaticProperty(messagesType, "Handler", null);
                    Directory.Delete(directory, true);
                }
            });
        }

        [TestMethod]
        public void QrCodeDialogPreservesSizeTextScalingAndCopyTarget()
        {
            RunInSta(() =>
            {
                Type type = RequireType("QRCodeDialog");
                byte[] bytes = CreatePngBytes(900, 300);
                var dialog = (Form)Activator.CreateInstance(type, null, "微信打赏", string.Empty, bytes, "D:\\copy.png");
                Assert.AreEqual("微信打赏", dialog.Text);
                Assert.AreEqual(new Size(500, 600), dialog.Size);
                var picture = GetProperty<PictureBox>(type, dialog, "QrPictureBox");
                Assert.AreEqual(new Size(450, 150), picture.Image.Size);
                Assert.AreEqual("用手机微信扫码请作者吃辣条", GetProperty<Label>(type, dialog, "TipLabel").Text);
                Assert.AreEqual("复制二维码", GetProperty<Button>(type, dialog, "CopyButton").Text);
                Assert.AreEqual(new Size(120, 35), GetProperty<Button>(type, dialog, "CopyButton").Size);
                Assert.AreEqual("关闭", GetProperty<Button>(type, dialog, "CloseButton").Text);
                Assert.AreEqual(new Size(80, 35), GetProperty<Button>(type, dialog, "CloseButton").Size);
                Assert.AreEqual("D:\\copy.png", ReflectionTestHelpers.InvokeInstance(dialog, "GetCopyTarget"));
                dialog.Dispose();
            });
        }

        [TestMethod]
        public void SelectUsersDialogPreservesTextOrderAndCheckedSelection()
        {
            RunInSta(() =>
            {
                Type type = RequireType("SelectUsersDialog");
                var users = new[] { "用户1", "用户2", "用户3" };
                var dialog = (Form)Activator.CreateInstance(type, null, "选择用户", users);
                Assert.AreEqual("选择用户", dialog.Text);
                Assert.AreEqual(new Size(400, 500), dialog.Size);
                Assert.AreEqual("请选择沙盒用户（可多选）：", GetProperty<Label>(type, dialog, "PromptLabel").Text);
                var list = GetProperty<ListView>(type, dialog, "UserList");
                CollectionAssert.AreEqual(users, list.Items.Cast<ListViewItem>().Select(item => item.Text).ToArray());
                Assert.AreEqual("沙盒用户", list.AccessibleName);
                ReflectionTestHelpers.InvokeInstance(list, "CheckItem", 0, true);
                ReflectionTestHelpers.InvokeInstance(list, "CheckItem", 2, true);
                ReflectionTestHelpers.InvokeInstance(dialog, "ConfirmSelection");
                CollectionAssert.AreEqual(
                    new[] { "用户1", "用户3" },
                    ((IEnumerable)ReflectionTestHelpers.InvokeInstance(dialog, "GetSelectedUsers")).Cast<string>().ToArray());
                Assert.AreEqual(DialogResult.OK, dialog.DialogResult);
                Assert.AreEqual("确定", GetProperty<Button>(type, dialog, "OkButton").Text);
                Assert.AreEqual("取消", GetProperty<Button>(type, dialog, "CancelActionButton").Text);
                dialog.Dispose();
            });
        }

        [TestMethod]
        public void GroupNameDialogTrimsValueAndRequiresNonBlankInput()
        {
            RunInSta(() =>
            {
                Type type = RequireType("GroupNameDialog");
                var dialog = (Form)Activator.CreateInstance(type, null, "分组名称", " 默认组 ");
                Assert.AreEqual(new Size(380, 150), dialog.Size);
                Assert.AreEqual("请输入分组名称:", GetProperty<Label>(type, dialog, "PromptLabel").Text);
                var input = GetProperty<TextBox>(type, dialog, "NameTextBox");
                Assert.AreEqual("分组名称", input.AccessibleName);
                input.Text = "   ";
                Assert.AreEqual(false, ReflectionTestHelpers.InvokeInstance(dialog, "ConfirmName"));
                Assert.AreEqual(DialogResult.None, dialog.DialogResult);
                input.Text = "  常用  ";
                Assert.AreEqual(true, ReflectionTestHelpers.InvokeInstance(dialog, "ConfirmName"));
                Assert.AreEqual("常用", GetProperty(type, dialog, "Value"));
                Assert.AreEqual(DialogResult.OK, dialog.DialogResult);
                Assert.AreEqual("确定", GetProperty<Button>(type, dialog, "OkButton").Text);
                Assert.AreEqual("取消", GetProperty<Button>(type, dialog, "CancelActionButton").Text);
                dialog.Dispose();
            });
        }

        [TestMethod]
        public void BatchNameRuleDialogPreservesNumericControlsAndResultKeys()
        {
            RunInSta(() =>
            {
                Type type = RequireType("BatchNameRuleDialog");
                var dialog = (Form)Activator.CreateInstance(
                    type,
                    null,
                    "批量创建沙盒用户",
                    "请输入批量创建规则：",
                    "用户",
                    1,
                    2,
                    3,
                    null,
                    null);
                Assert.AreEqual(new Size(430, 320), dialog.Size);
                Assert.AreEqual("请输入批量创建规则：", GetProperty<Label>(type, dialog, "PromptLabel").Text);
                Assert.AreEqual(
                    "示例：前缀=沙盒用户，起始=1，位数=2，数量=3 -> 沙盒用户01、沙盒用户02、沙盒用户03",
                    GetProperty<Label>(type, dialog, "ExampleLabel").Text);
                AssertControlName(type, dialog, "PrefixTextBox", "名称前缀");
                AssertControlName(type, dialog, "StartInput", "起始编号");
                AssertControlName(type, dialog, "DigitsInput", "编号位数");
                AssertControlName(type, dialog, "CountInput", "创建数量");
                Assert.AreEqual(0m, GetProperty<NumericUpDown>(type, dialog, "StartInput").Minimum);
                Assert.AreEqual(999999m, GetProperty<NumericUpDown>(type, dialog, "StartInput").Maximum);
                Assert.AreEqual(8m, GetProperty<NumericUpDown>(type, dialog, "DigitsInput").Maximum);
                Assert.AreEqual(5000m, GetProperty<NumericUpDown>(type, dialog, "CountInput").Maximum);
                Assert.AreEqual(true, ReflectionTestHelpers.InvokeInstance(dialog, "ConfirmRule"));
                IDictionary result = (IDictionary)GetProperty(type, dialog, "Result");
                Assert.AreEqual("用户", result["prefix"]);
                Assert.AreEqual(1, result["start"]);
                Assert.AreEqual(2, result["digits"]);
                Assert.AreEqual(3, result["count"]);
                Assert.AreEqual(DialogResult.OK, dialog.DialogResult);
                dialog.Dispose();
            });
        }

        [TestMethod]
        public void BatchNameRuleDialogPreservesChoiceLabelsAndFixedCount()
        {
            RunInSta(() =>
            {
                Type type = RequireType("BatchNameRuleDialog");
                var choices = new[]
                {
                    new KeyValuePair<int, string>(0, "不加0"),
                    new KeyValuePair<int, string>(2, "前面加0（01）"),
                    new KeyValuePair<int, string>(4, "")
                };
                var dialog = (Form)Activator.CreateInstance(
                    type, null, "批量重命名沙盒用户", "将按当前勾选顺序依次重命名，请设置新命名规则：",
                    "沙盒用户", 1, 2, 9, (int?)4, choices);
                Assert.AreEqual(new Size(430, 300), dialog.Size);
                var choice = GetProperty<ComboBox>(type, dialog, "DigitsChoice");
                Assert.AreEqual("编号格式", choice.AccessibleName);
                CollectionAssert.AreEqual(new[] { "不加0", "前面加0（01）" }, choice.Items.Cast<string>().ToArray());
                Assert.AreEqual(1, choice.SelectedIndex);
                Assert.AreEqual("目标数量: 4", GetProperty<Label>(type, dialog, "TargetCountLabel").Text);
                Assert.IsNull(GetProperty(type, dialog, "CountInput"));
                Assert.AreEqual(true, ReflectionTestHelpers.InvokeInstance(dialog, "ConfirmRule"));
                IDictionary result = (IDictionary)GetProperty(type, dialog, "Result");
                Assert.AreEqual(2, result["digits"]);
                Assert.AreEqual(4, result["count"]);
                dialog.Dispose();
            });
        }

        [TestMethod]
        public void BatchNameRuleDialogUsesExactValidationMessages()
        {
            RunInSta(() =>
            {
                Type messagesType = RequireType("DialogMessageService");
                Type type = RequireType("BatchNameRuleDialog");
                var messages = new List<string>();
                SetStaticProperty(
                    messagesType,
                    "Handler",
                    new Action<string, string, MessageBoxIcon>((text, title, icon) => messages.Add(title + "|" + text + "|" + (int)icon)));
                try
                {
                    var emptyPrefix = (Form)Activator.CreateInstance(
                        type, null, "批量创建沙盒用户", "提示", "", 1, 0, 1, null, null);
                    Assert.AreEqual(false, ReflectionTestHelpers.InvokeInstance(emptyPrefix, "ConfirmRule"));
                    Assert.AreEqual("输入错误|名称前缀不能为空。|48", messages.Last());
                    emptyPrefix.Dispose();

                    var invalidCount = (Form)Activator.CreateInstance(
                        type, null, "批量重命名沙盒用户", "提示", "用户", 1, 0, 1, (int?)0, null);
                    Assert.AreEqual(false, ReflectionTestHelpers.InvokeInstance(invalidCount, "ConfirmRule"));
                    Assert.AreEqual("输入错误|数量必须大于 0。|48", messages.Last());
                    invalidCount.Dispose();
                }
                finally
                {
                    SetStaticProperty(messagesType, "Handler", null);
                }
            });
        }

        private static Type RequireType(string name)
        {
            Type type = Assembly.Load("沙河批量打开器").GetType(Namespace + name, false);
            Assert.IsNotNull(type, Namespace + name + " must exist");
            return type;
        }

        private static T GetProperty<T>(Type type, object target, string name) where T : class
        {
            return (T)GetProperty(type, target, name);
        }

        private static object GetProperty(Type type, object target, string name)
        {
            PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(property, name + " must be a public property");
            return property.GetValue(target);
        }

        private static void SetStaticProperty(Type type, string name, object value)
        {
            PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(property, name + " must be a public static property");
            property.SetValue(null, value);
        }

        private static void AssertControlName(Type type, object target, string propertyName, string expected)
        {
            var control = (Control)GetProperty(type, target, propertyName);
            Assert.AreEqual(expected, control.AccessibleName);
        }

        private static string CreateTempDirectory()
        {
            string path = Path.Combine(Path.GetTempPath(), "shahe-dialogs-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        private static byte[] CreatePngBytes(int width, int height)
        {
            using (var bitmap = new Bitmap(width, height))
            using (Graphics graphics = Graphics.FromImage(bitmap))
            using (var stream = new MemoryStream())
            {
                graphics.Clear(Color.White);
                bitmap.Save(stream, ImageFormat.Png);
                return stream.ToArray();
            }
        }

        private static void RunInSta(Action action)
        {
            Exception error = null;
            var thread = new Thread(() =>
            {
                try { action(); }
                catch (Exception caught) { error = caught; }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (error != null) throw new AssertFailedException("STA action failed: " + error, error);
        }
    }
}
