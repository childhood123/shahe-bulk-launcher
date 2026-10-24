using ShaheLauncher.App.Infrastructure;
using System.Drawing;
using System.Windows.Forms;

namespace ShaheLauncher.App.UI.Dialogs.Common
{
    public sealed class GroupNameDialog : Form
    {
        public GroupNameDialog(Form parent, string title, string defaultValue)
        {
            if (parent != null)
            {
                Owner = parent;
            }

            Text = string.IsNullOrEmpty(title) ? "分组名称" : title;
            Size = new Size(380, 150);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            AutoScaleMode = AutoScaleMode.None;
            Value = string.Empty;
            InitializeUi(defaultValue ?? string.Empty);
        }

        public string Value { get; private set; }
        public Label PromptLabel { get; private set; }
        public TextBox NameTextBox { get; private set; }
        public Button OkButton { get; private set; }
        public Button CancelActionButton { get; private set; }

        public bool ConfirmName()
        {
            string value = (NameTextBox.Text ?? string.Empty).Trim();
            if (value.Length == 0)
            {
                return false;
            }

            Value = value;
            DialogResult = DialogResult.OK;
            Close();
            return true;
        }

        private void InitializeUi(string defaultValue)
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
            PromptLabel = new Label
            {
                Text = "请输入分组名称:",
                AutoSize = true,
                TabStop = false,
                Margin = new Padding(0, 0, 0, 8)
            };
            NameTextBox = new TextBox
            {
                Text = defaultValue,
                AccessibleName = "分组名称",
                Dock = DockStyle.Top,
                TabIndex = 0,
                Margin = new Padding(0, 0, 0, 8)
            };
            OkButton = new Button { Text = "确定", AutoSize = true, TabIndex = 1 };
            OkButton.Click += (sender, eventArgs) => OperationLog.Run(this, Text + " / " + OkButton.Text, () => ConfirmName());
            CancelActionButton = new Button
            {
                Text = "取消",
                AutoSize = true,
                TabIndex = 2,
                DialogResult = DialogResult.Cancel
            };
            CancelActionButton.Click += (sender, eventArgs) => OperationLog.Run(this, Text + " / " + CancelActionButton.Text, () => Close());
            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Margin = new Padding(0)
            };
            CancelActionButton.Margin = new Padding(0);
            OkButton.Margin = new Padding(0, 0, 5, 0);
            buttons.Controls.Add(CancelActionButton);
            buttons.Controls.Add(OkButton);
            layout.Controls.Add(PromptLabel, 0, 0);
            layout.Controls.Add(NameTextBox, 0, 1);
            layout.Controls.Add(buttons, 0, 2);
            Controls.Add(layout);
            AcceptButton = OkButton;
            CancelButton = CancelActionButton;
        }
    }
}
