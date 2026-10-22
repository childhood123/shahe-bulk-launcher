using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShaheLauncher.App.UI.Dialogs.Common;
using ShaheLauncher.Core.Updates;

namespace ShaheLauncher.Tests
{
    [TestClass, TestCategory("CommonDialogs"), TestCategory("Updates"), DoNotParallelize]
    public sealed class DonationQrRuntimeContractTests
    {
        [DataTestMethod]
        [DataRow("微信赞赏码.jpg")]
        [DataRow("支付宝赞赏码.jpg")]
        public void DonationDirectoryTakesPriorityOverLegacyCopies(string filename)
        {
            string root = Path.Combine(Path.GetTempPath(), "ShaheDonationTests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(root, "打赏"));
            Directory.CreateDirectory(Path.Combine(root, "internal"));
            try
            {
                string requested = Path.Combine(root, "打赏", filename);
                File.WriteAllBytes(requested, new byte[] { 1 });
                File.WriteAllBytes(Path.Combine(root, filename), new byte[] { 2 });
                File.WriteAllBytes(Path.Combine(root, "internal", filename), new byte[] { 3 });
                using (var service = new UpdateService(root, false, string.Empty, null, null))
                    Assert.AreEqual(requested, service.ResolveQrImagePath(filename));
            }
            finally { Directory.Delete(root, true); }
        }

        [DataTestMethod]
        [DataRow("微信赞赏码.jpg", "微信打赏")]
        [DataRow("支付宝赞赏码.jpg", "支付宝打赏")]
        public void DonationButtonCopiesShippedOriginalImage(string filename, string title)
        {
            RunInSta(() =>
            {
                string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string imagePath = Path.Combine(appDirectory, "打赏", filename);
                Assert.IsTrue(File.Exists(imagePath), "Build output must contain the original donation image: " + filename);
                using (var service = new UpdateService(appDirectory, false, string.Empty, null, null))
                    Assert.AreEqual(imagePath, service.ResolveQrImagePath(filename));
                IDataObject previousClipboard = Clipboard.GetDataObject();
                var previousHandler = DialogMessageService.Handler;
                string message = string.Empty;
                DialogMessageService.Handler = (text, caption, icon) => message = caption + "|" + text;
                try
                {
                    using (var original = new Bitmap(imagePath))
                    using (var dialog = new QRCodeDialog(null, title, imagePath, null, imagePath))
                    {
                        dialog.Show();
                        Assert.IsTrue(dialog.QrPictureBox.Image.Width <= 450 && dialog.QrPictureBox.Image.Height <= 450);
                        Assert.AreEqual("复制二维码", dialog.CopyButton.Text);
                        Assert.AreEqual("复制二维码", dialog.CopyButton.AccessibilityObject.Name);
                        Assert.AreEqual(0, dialog.CopyButton.TabIndex);
                        Assert.AreEqual(1, dialog.CloseButton.TabIndex);
                        dialog.CopyButton.PerformClick();
                        Assert.AreEqual("成功|二维码已复制到剪贴板！", message);
                        Assert.IsTrue(Clipboard.ContainsImage());
                        using (Image copied = Clipboard.GetImage())
                        using (var pixels = new Bitmap(copied))
                        {
                            Assert.AreEqual(original.Size, copied.Size, "Copy must use the full image, not the scaled preview.");
                            foreach (Point point in new[] { new Point(0, 0), new Point(original.Width / 2, original.Height / 2), new Point(original.Width - 1, original.Height - 1) })
                                Assert.AreEqual(original.GetPixel(point.X, point.Y), pixels.GetPixel(point.X, point.Y));
                        }
                        dialog.Close();
                    }
                }
                finally
                {
                    DialogMessageService.Handler = previousHandler;
                    if (previousClipboard != null) Clipboard.SetDataObject(previousClipboard, true);
                    else Clipboard.Clear();
                }
            });
        }

        private static void RunInSta(Action action)
        {
            Exception failure = null;
            var thread = new Thread(() => { try { action(); } catch (Exception error) { failure = error; } });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            Assert.IsTrue(thread.Join(TimeSpan.FromSeconds(20)), "Donation dialog verification timed out.");
            if (failure != null) Assert.Fail(failure.ToString());
        }
    }
}
