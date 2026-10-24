using System;
using System.Drawing;
using System.Windows.Forms;
using ShaheLauncher.Core.Constants;
using ShaheLauncher.Core.Updates;

namespace ShaheLauncher.App.UI.Dialogs.Common
{
    public sealed class UpdateAvailableDialog : Form
    {
        public UpdateAvailableDialog(UpdateDisplayResult update)
        {
            if (update == null) throw new ArgumentNullException(nameof(update));
            Text = "软件更新";
            Size = new Size(600, 400);
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2,
                Padding = new Padding(12), TabStop = false
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            NotesTextBox = new TextBox
            {
                Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill, AccessibleName = "更新内容", TabIndex = 0,
                Text = "当前版本：" + LauncherConstants.AppVersion + Environment.NewLine
                    + "新版本：" + update.LatestVersion + Environment.NewLine + Environment.NewLine
                    + "更新内容：" + Environment.NewLine
                    + (string.IsNullOrWhiteSpace(update.ReleaseNotes) ? "此版本未提供更新说明。" : update.ReleaseNotes)
            };
            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, AutoSize = true, TabStop = false, TabIndex = 1,
                FlowDirection = FlowDirection.LeftToRight, WrapContents = false
            };
            UpdateNowButton = CreateButton("立即更新", 1, DialogResult.Yes);
            LaterButton = CreateButton("稍后更新", 2, DialogResult.No);
            IgnoreButton = CreateButton("忽略此版本", 3, DialogResult.Ignore);
            buttons.Controls.AddRange(new Control[] { UpdateNowButton, LaterButton, IgnoreButton });
            layout.Controls.Add(NotesTextBox, 0, 0);
            layout.Controls.Add(buttons, 0, 1);
            Controls.Add(layout);
            Shown += (sender, args) => { NotesTextBox.Focus(); NotesTextBox.SelectionStart = 0; NotesTextBox.SelectionLength = 0; };
        }

        public TextBox NotesTextBox { get; private set; }
        public Button UpdateNowButton { get; private set; }
        public Button LaterButton { get; private set; }
        public Button IgnoreButton { get; private set; }

        protected override bool ProcessCmdKey(ref Message message, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                return true;
            }
            return base.ProcessCmdKey(ref message, keyData);
        }

        private static Button CreateButton(string text, int tabIndex, DialogResult result)
        {
            return new Button { Text = text, AccessibleName = text, AutoSize = true, TabIndex = tabIndex, DialogResult = result };
        }
    }
}
