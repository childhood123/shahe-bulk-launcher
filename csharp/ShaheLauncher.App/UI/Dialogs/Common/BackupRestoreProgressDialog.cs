using System;
using System.Drawing;
using System.Windows.Forms;

namespace ShaheLauncher.App.UI.Dialogs.Common
{
    public sealed class BackupRestoreProgressDialog : Form
    {
        public BackupRestoreProgressDialog(Form parent)
        {
            if (parent != null)
            {
                Owner = parent;
            }

            Text = "还原沙盒数据";
            Size = new Size(420, 130);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ControlBox = false;
            ShowInTaskbar = false;
            AutoScaleMode = AutoScaleMode.None;
            InitializeUi();
        }

        public ProgressBar ProgressBar { get; private set; }
        public Label StageLabel { get; private set; }

        public void UpdateProgress(int value, string text)
        {
            ProgressBar.Value = Math.Max(ProgressBar.Minimum, Math.Min(ProgressBar.Maximum, value));
            StageLabel.Text = text ?? string.Empty;
        }

        private void InitializeUi()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10),
                TabStop = false
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            StageLabel = new Label
            {
                Text = "准备...",
                AutoSize = true,
                TabStop = false,
                Margin = new Padding(0, 0, 0, 8)
            };
            ProgressBar = new ProgressBar
            {
                Dock = DockStyle.Fill,
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                AccessibleName = "还原进度",
                TabStop = false
            };
            layout.Controls.Add(StageLabel, 0, 0);
            layout.Controls.Add(ProgressBar, 0, 1);
            Controls.Add(layout);
        }
    }
}
