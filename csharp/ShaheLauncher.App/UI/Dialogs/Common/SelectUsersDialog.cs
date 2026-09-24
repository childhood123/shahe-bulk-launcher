using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ShaheLauncher.App.UI.Controls;

namespace ShaheLauncher.App.UI.Dialogs.Common
{
    public sealed class SelectUsersDialog : Form
    {
        private readonly string[] _users;
        private string[] _selectedUsers = new string[0];

        public SelectUsersDialog(Form parent, string title, IEnumerable<string> users)
        {
            if (parent != null)
            {
                Owner = parent;
            }

            _users = (users ?? Enumerable.Empty<string>()).ToArray();
            Text = title ?? string.Empty;
            Size = new Size(400, 500);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            AutoScaleMode = AutoScaleMode.None;
            InitializeUi();
        }

        public Label PromptLabel { get; private set; }
        public CheckableListCtrl UserList { get; private set; }
        public Button OkButton { get; private set; }
        public Button CancelActionButton { get; private set; }

        public void ConfirmSelection()
        {
            _selectedUsers = UserList.GetCheckedItems()
                .Where(index => index >= 0 && index < _users.Length)
                .Select(index => _users[index])
                .ToArray();
            DialogResult = DialogResult.OK;
            Close();
        }

        public string[] GetSelectedUsers()
        {
            return _selectedUsers.ToArray();
        }

        private void InitializeUi()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(5)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            PromptLabel = new Label
            {
                Text = "请选择沙盒用户（可多选）：",
                AutoSize = true,
                TabStop = false,
                Margin = new Padding(5)
            };
            UserList = new CheckableListCtrl
            {
                Dock = DockStyle.Fill,
                TabIndex = 0,
                Margin = new Padding(5)
            };
            foreach (string user in _users)
            {
                UserList.AppendItem(user);
            }

            OkButton = new Button { Text = "确定", AutoSize = true, TabIndex = 1 };
            OkButton.Click += (sender, eventArgs) => ConfirmSelection();
            CancelActionButton = new Button
            {
                Text = "取消",
                AutoSize = true,
                TabIndex = 2,
                DialogResult = DialogResult.Cancel
            };
            CancelActionButton.Click += (sender, eventArgs) => Close();
            var buttons = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Anchor = AnchorStyles.None,
                Margin = new Padding(5)
            };
            OkButton.Margin = new Padding(5);
            CancelActionButton.Margin = new Padding(5);
            buttons.Controls.Add(OkButton);
            buttons.Controls.Add(CancelActionButton);
            layout.Controls.Add(PromptLabel, 0, 0);
            layout.Controls.Add(UserList, 0, 1);
            layout.Controls.Add(buttons, 0, 2);
            Controls.Add(layout);
            AcceptButton = OkButton;
            CancelButton = CancelActionButton;
        }
    }
}
