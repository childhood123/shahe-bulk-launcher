using System;
using ShaheLauncher.App.Infrastructure;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ShaheLauncher.App.UI.Dialogs.Common
{
    public sealed class QRCodeDialog : Form
    {
        private readonly string _imagePath;
        private readonly byte[] _embeddedBytes;
        private readonly string _copyImagePath;

        public QRCodeDialog(Form parent, string title, string imagePath, byte[] embeddedBytes, string copyImagePath)
        {
            if (parent != null)
            {
                Owner = parent;
            }

            Text = title ?? string.Empty;
            Size = new Size(500, 600);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            AutoScaleMode = AutoScaleMode.None;
            _imagePath = imagePath ?? string.Empty;
            _embeddedBytes = embeddedBytes;
            _copyImagePath = (copyImagePath ?? string.Empty).Trim();
            InitializeUi();
        }

        public PictureBox QrPictureBox { get; private set; }
        public Label TipLabel { get; private set; }
        public Button CopyButton { get; private set; }
        public Button CloseButton { get; private set; }

        public string GetCopyTarget()
        {
            return _copyImagePath.Length == 0 ? _imagePath : _copyImagePath;
        }

        public void CopyQrCode()
        {
            string target = GetCopyTarget();
            if (target.Length == 0)
            {
                DialogMessageService.Show(this, "未找到 internal 二维码文件，无法复制", "错误", MessageBoxIcon.Error);
                return;
            }

            DialogImageUtilities.CopyImageToClipboard(target, this);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && QrPictureBox != null)
            {
                Image image = QrPictureBox.Image;
                QrPictureBox.Image = null;
                image?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeUi()
        {
            try
            {
                Image image = _embeddedBytes != null && _embeddedBytes.Length != 0
                    ? DialogImageUtilities.LoadImageFromBytes(_embeddedBytes)
                    : LoadImageFromFile(_imagePath);
                if (image == null)
                {
                    throw new InvalidOperationException("二维码资源加载失败");
                }

                image = DialogImageUtilities.ScaleToFit(image, 450);
                BuildControls(image);
            }
            catch (Exception error)
            {
                DialogMessageService.Show(this, "无法加载图片：" + error.Message, "错误", MessageBoxIcon.Error);
                Close();
            }
        }

        private void BuildControls(Image image)
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(10)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            QrPictureBox = new PictureBox
            {
                Image = image,
                Size = image.Size,
                SizeMode = PictureBoxSizeMode.Normal,
                Anchor = AnchorStyles.None,
                TabStop = false,
                Margin = new Padding(0, 0, 0, 5)
            };
            TipLabel = new Label
            {
                Text = "用手机微信扫码请作者吃辣条",
                AutoSize = true,
                Font = new Font(Font.FontFamily, 10F, FontStyle.Regular),
                Anchor = AnchorStyles.None,
                TabStop = false,
                Margin = new Padding(0, 0, 0, 5)
            };
            CopyButton = new Button { Text = "复制二维码", Size = new Size(120, 35), TabIndex = 0 };
            CopyButton.Click += (sender, eventArgs) => OperationLog.Run(this, Text + " / " + CopyButton.Text, () => CopyQrCode());
            CloseButton = new Button { Text = "关闭", Size = new Size(80, 35), TabIndex = 1, DialogResult = DialogResult.Cancel };
            CloseButton.Click += (sender, eventArgs) => OperationLog.Run(this, Text + " / " + CloseButton.Text, () => Close());

            var buttons = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Anchor = AnchorStyles.None,
                Margin = new Padding(0)
            };
            CopyButton.Margin = new Padding(10);
            CloseButton.Margin = new Padding(10);
            buttons.Controls.Add(CopyButton);
            buttons.Controls.Add(CloseButton);
            layout.Controls.Add(QrPictureBox, 0, 0);
            layout.Controls.Add(TipLabel, 0, 1);
            layout.Controls.Add(buttons, 0, 2);
            Controls.Add(layout);
            CancelButton = CloseButton;
        }

        private static Image LoadImageFromFile(string imagePath)
        {
            if (!File.Exists(imagePath))
            {
                return null;
            }

            using (Image image = Image.FromFile(imagePath))
            {
                return new Bitmap(image);
            }
        }
    }
}
