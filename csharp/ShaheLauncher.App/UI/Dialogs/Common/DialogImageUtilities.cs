using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ShaheLauncher.App.UI.Dialogs.Common
{
    public static class DialogImageUtilities
    {
        public static Image LoadImageFromBytes(byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length == 0)
            {
                return null;
            }

            try
            {
                using (var stream = new MemoryStream(imageBytes, false))
                using (Image image = Image.FromStream(stream, true, true))
                {
                    return new Bitmap(image);
                }
            }
            catch
            {
                return null;
            }
        }

        public static bool CopyImageToClipboard(string imagePath, IWin32Window parent)
        {
            if (!File.Exists(imagePath))
            {
                DialogMessageService.Show(parent, "未找到图片文件：" + imagePath, "错误", MessageBoxIcon.Error);
                return false;
            }

            try
            {
                using (Image source = Image.FromFile(imagePath))
                using (var bitmap = new Bitmap(source))
                {
                    Clipboard.SetImage(bitmap);
                }

                DialogMessageService.Show(parent, "二维码已复制到剪贴板！", "成功", MessageBoxIcon.Information);
                return true;
            }
            catch (ExternalException)
            {
                DialogMessageService.Show(parent, "无法打开剪贴板", "错误", MessageBoxIcon.Error);
                return false;
            }
            catch (Exception error)
            {
                DialogMessageService.Show(parent, "复制失败：" + error.Message, "错误", MessageBoxIcon.Error);
                return false;
            }
        }

        public static Image ScaleToFit(Image image, int maximumSize)
        {
            if (image == null || maximumSize <= 0
                || (image.Width <= maximumSize && image.Height <= maximumSize))
            {
                return image;
            }

            double scale = (double)maximumSize / Math.Max(image.Width, image.Height);
            int width = Math.Max(1, (int)(image.Width * scale));
            int height = Math.Max(1, (int)(image.Height * scale));
            var resized = new Bitmap(width, height);
            using (Graphics graphics = Graphics.FromImage(resized))
            {
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(image, 0, 0, width, height);
            }

            image.Dispose();
            return resized;
        }
    }
}
