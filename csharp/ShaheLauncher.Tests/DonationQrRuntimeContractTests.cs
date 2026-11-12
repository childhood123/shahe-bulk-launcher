using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
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
                ClipboardSnapshot previousClipboard = RetryClipboard(() => ClipboardSnapshot.Capture(Clipboard.GetDataObject()));
                var previousHandler = DialogMessageService.Handler;
                string message = string.Empty;
                bool clipboardMayHaveChanged = false;
                Exception testFailure = null;
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
                        clipboardMayHaveChanged = true;
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
                catch (Exception error)
                {
                    testFailure = error;
                    throw;
                }
                finally
                {
                    DialogMessageService.Handler = previousHandler;
                    using (previousClipboard)
                    {
                        if (clipboardMayHaveChanged)
                        {
                            try { previousClipboard.Restore(); }
                            catch (Exception restoreFailure)
                            {
                                if (testFailure != null)
                                    throw new AggregateException("Donation verification and clipboard restoration both failed.", testFailure, restoreFailure);
                                throw;
                            }
                        }
                    }
                }
            });
        }

        [TestMethod]
        public void ClipboardSnapshotSurvivesSourceMutationAndDisposal()
        {
            RunInSta(() =>
            {
                ClipboardSnapshot snapshot = null;
                try
                {
                    using (var bitmap = new Bitmap(2, 2))
                    using (var stream = new MemoryStream(new byte[] { 1, 2, 3 }))
                    {
                        bitmap.SetPixel(0, 0, Color.Red);
                        stream.Position = 2;
                        var files = new[] { "fixture.txt" };
                        var bytes = new byte[] { 4, 5 };
                        var source = new DataObject();
                        source.SetData(DataFormats.UnicodeText, false, "fixture text");
                        source.SetData(DataFormats.Bitmap, false, bitmap);
                        source.SetData(DataFormats.FileDrop, false, files);
                        source.SetData("Fixture.Stream", false, stream);
                        source.SetData("Fixture.Bytes", false, bytes);

                        snapshot = ClipboardSnapshot.Capture(source);
                        CollectionAssert.AreEquivalent(source.GetFormats(false), snapshot.Data.GetFormats(false));
                        Assert.AreEqual(2L, stream.Position, "Capture must preserve the source stream position.");
                        bitmap.SetPixel(0, 0, Color.Blue);
                        stream.Position = 0;
                        stream.WriteByte(9);
                        files[0] = "changed.txt";
                        bytes[0] = 9;
                        source.SetData(DataFormats.UnicodeText, false, "changed text");
                    }

                    Assert.AreEqual("fixture text", snapshot.Data.GetData(DataFormats.UnicodeText, false));
                    Assert.AreEqual(Color.Red.ToArgb(), ((Bitmap)snapshot.Data.GetData(DataFormats.Bitmap, false)).GetPixel(0, 0).ToArgb());
                    CollectionAssert.AreEqual(new[] { "fixture.txt" }, (string[])snapshot.Data.GetData(DataFormats.FileDrop, false));
                    CollectionAssert.AreEqual(new byte[] { 4, 5 }, (byte[])snapshot.Data.GetData("Fixture.Bytes", false));
                    var copiedStream = (MemoryStream)snapshot.Data.GetData("Fixture.Stream", false);
                    Assert.AreEqual(0L, copiedStream.Position);
                    CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, copiedStream.ToArray());
                    snapshot.Dispose();
                    Assert.IsFalse(copiedStream.CanRead, "The snapshot must release the resources it owns.");
                }
                finally { snapshot?.Dispose(); }
            });
        }

        [TestMethod]
        public void ClipboardSnapshotRejectsDataItCannotPreserve()
        {
            RunInSta(() =>
            {
                var source = new DataObject();
                source.SetData("Fixture.Unsupported", false, new object());
                Assert.ThrowsException<InvalidOperationException>(() => ClipboardSnapshot.Capture(source));
            });
        }

        private static T RetryClipboard<T>(Func<T> operation)
        {
            for (int attempt = 0; ; attempt++)
            {
                try { return operation(); }
                catch (ExternalException error) when (error.ErrorCode == unchecked((int)0x800401D0) && attempt < 2)
                {
                    Thread.Sleep(100);
                }
            }
        }

        private sealed class ClipboardSnapshot : IDisposable
        {
            private readonly List<IDisposable> _ownedData = new List<IDisposable>();
            public DataObject Data { get; } = new DataObject();

            public static ClipboardSnapshot Capture(IDataObject source)
            {
                var snapshot = new ClipboardSnapshot();
                try
                {
                    // Materialize every native format while the original OLE owner is still available.
                    if (source != null)
                    {
                        foreach (string format in source.GetFormats(false))
                        {
                            object data = CloneData(source.GetData(format, false));
                            if (data is IDisposable owned) snapshot._ownedData.Add(owned);
                            snapshot.Data.SetData(format, false, data);
                        }
                    }
                    return snapshot;
                }
                catch
                {
                    snapshot.Dispose();
                    throw;
                }
            }

            public void Restore()
            {
                if (Data.GetFormats(false).Length == 0)
                    RetryClipboard(() => { Clipboard.Clear(); return true; });
                else
                    Clipboard.SetDataObject(Data, true, 10, 100);
            }

            public void Dispose()
            {
                foreach (IDisposable owned in _ownedData) owned.Dispose();
                _ownedData.Clear();
            }

            private static object CloneData(object data)
            {
                if (data is string) return data;
                if (data is Image image) return image.Clone();
                if (data is byte[] bytes) return bytes.Clone();
                if (data is string[] strings) return strings.Clone();
                if (data is Stream stream && stream.CanRead && stream.CanSeek)
                {
                    long position = stream.Position;
                    var copy = new MemoryStream();
                    try
                    {
                        try
                        {
                            stream.Position = 0;
                            stream.CopyTo(copy);
                        }
                        finally { stream.Position = position; }
                        copy.Position = 0;
                        return copy;
                    }
                    catch
                    {
                        copy.Dispose();
                        throw;
                    }
                }
                throw new InvalidOperationException("Clipboard data could not be safely materialized; the clipboard was not changed.");
            }
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
