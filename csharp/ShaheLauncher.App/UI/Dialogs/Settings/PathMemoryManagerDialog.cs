using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ShaheLauncher.App.UI.Dialogs.Common;
using ShaheLauncher.Core.Common;

namespace ShaheLauncher.App.UI.Dialogs.Settings
{
    public sealed class PathMemoryManagerDialog : Form
    {
        private readonly IList<IDictionary<string, object>> _appHistory;
        private readonly IDictionary<string, object> _settings;
        private readonly Action<IList<IDictionary<string, object>>, IDictionary<string, object>> _onSave;

        public PathMemoryManagerDialog(
            Form parent,
            IList<IDictionary<string, object>> appHistory,
            IDictionary<string, object> settings,
            Action<IList<IDictionary<string, object>>, IDictionary<string, object>> onSaveCallback)
        {
            if (parent != null)
            {
                Owner = parent;
            }

            _appHistory = appHistory ?? new List<IDictionary<string, object>>();
            _settings = settings ?? new Dictionary<string, object>();
            _onSave = onSaveCallback;
            Text = "路径记忆管理";
            Size = new Size(700, 500);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            AutoScaleMode = AutoScaleMode.None;
            InitializeUi();
            UpdateList();
        }

        public NumericUpDown HistoryCountInput { get; private set; }
        public CheckBox HidePathCheckBox { get; private set; }
        public ListBox HistoryList { get; private set; }
        public Button OkButton { get; private set; }
        public Button CancelActionButton { get; private set; }

        public void UpdateList()
        {
            HistoryList.Items.Clear();
            bool hidePath = CommonUtilities.ParseBoolSetting(
                GetValue(_settings, "hide_path", false), false);
            foreach (IDictionary<string, object> item in _appHistory)
            {
                string note = GetText(item, "note");
                string path = GetText(item, "path");
                HistoryList.Items.Add(hidePath ? note : note + " - \"" + path + "\"");
            }
        }

        public bool UpdateSelectedNote(string newNote)
        {
            int selection = HistoryList.SelectedIndex;
            if (selection < 0 || selection >= _appHistory.Count)
            {
                return false;
            }

            string note = (newNote ?? string.Empty).Trim();
            if (note.Length == 0)
            {
                return false;
            }

            _appHistory[selection]["note"] = note;
            UpdateList();
            HistoryList.SelectedIndex = selection;
            return true;
        }

        public bool RemoveSelected()
        {
            int selection = HistoryList.SelectedIndex;
            if (selection < 0 || selection >= _appHistory.Count)
            {
                return false;
            }

            if (!DialogMessageService.Confirm(
                this,
                "确定要删除当前路径记忆吗？",
                "确认删除",
                MessageBoxIcon.Question,
                true))
            {
                return false;
            }

            _appHistory.RemoveAt(selection);
            UpdateList();
            return true;
        }

        public bool ClearAll()
        {
            if (!DialogMessageService.Confirm(
                this,
                "确定要清空所有路径记忆吗？",
                "确认清空",
                MessageBoxIcon.Question,
                true))
            {
                return false;
            }

            _appHistory.Clear();
            UpdateList();
            return true;
        }

        public bool SaveChanges()
        {
            _settings["max_history_items"] = Decimal.ToInt32(HistoryCountInput.Value);
            _settings["hide_path"] = HidePathCheckBox.Checked;
            _onSave?.Invoke(_appHistory, _settings);
            DialogResult = DialogResult.OK;
            Close();
            return true;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if ((keyData & Keys.KeyCode) == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void InitializeUi()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(10)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            HistoryCountInput = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 20,
                Value = CommonUtilities.ParseIntSetting(
                    GetValue(_settings, "max_history_items", 10), 10, 0, 20),
                AccessibleName = "路径记忆数量",
                Width = 80,
                TabIndex = 0
            };
            HidePathCheckBox = new CheckBox
            {
                Text = "隐藏路径",
                Checked = CommonUtilities.ParseBoolSetting(
                    GetValue(_settings, "hide_path", false), false),
                AutoSize = true,
                TabIndex = 1
            };
            HistoryList = new ListBox
            {
                Dock = DockStyle.Fill,
                AccessibleName = "已记忆的路径列表",
                TabIndex = 2
            };
            HistoryList.ContextMenu = CreateHistoryMenu();
            OkButton = new Button { Text = "确定", AutoSize = true, TabIndex = 3 };
            CancelActionButton = new Button
            {
                Text = "取消",
                AutoSize = true,
                TabIndex = 4,
                DialogResult = DialogResult.Cancel
            };

            layout.Controls.Add(CreateCountRow(), 0, 0);
            layout.Controls.Add(HidePathCheckBox, 0, 1);
            layout.Controls.Add(new Label
            {
                Text = "已记忆的路径列表:",
                AutoSize = true,
                TabStop = false,
                Margin = new Padding(0, 10, 0, 5)
            }, 0, 2);
            layout.Controls.Add(HistoryList, 0, 3);
            layout.Controls.Add(CreateButtonRow(), 0, 4);
            Controls.Add(layout);
            AcceptButton = OkButton;
            CancelButton = CancelActionButton;
            OkButton.Click += (sender, eventArgs) => SaveChanges();
            CancelActionButton.Click += (sender, eventArgs) => Close();
        }

        private Control CreateCountRow()
        {
            var row = new FlowLayoutPanel
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                TabStop = false
            };
            row.Controls.Add(CreateLabel("路径记忆数量:"));
            row.Controls.Add(HistoryCountInput);
            row.Controls.Add(CreateLabel("(0=不记录, 最大20)"));
            return row;
        }

        private Control CreateButtonRow()
        {
            var row = new FlowLayoutPanel
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                TabStop = false,
                Margin = new Padding(0, 5, 0, 0)
            };
            CancelActionButton.Margin = new Padding(0);
            OkButton.Margin = new Padding(0, 0, 5, 0);
            row.Controls.Add(CancelActionButton);
            row.Controls.Add(OkButton);
            return row;
        }

        private ContextMenu CreateHistoryMenu()
        {
            var menu = new ContextMenu();
            menu.MenuItems.Add(new MenuItem("路径记忆备注", (sender, eventArgs) => EditSelectedNote()));
            menu.MenuItems.Add("-");
            menu.MenuItems.Add(new MenuItem("删除当前记忆", (sender, eventArgs) => RemoveSelected()));
            menu.MenuItems.Add(new MenuItem("清空所有路径记忆", (sender, eventArgs) => ClearAll()));
            return menu;
        }

        private void EditSelectedNote()
        {
            int selection = HistoryList.SelectedIndex;
            if (selection < 0 || selection >= _appHistory.Count)
            {
                return;
            }

            using (var dialog = new Form
            {
                Text = "路径记忆备注",
                Size = new Size(400, 150),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                ShowInTaskbar = false,
                AutoScaleMode = AutoScaleMode.None
            })
            {
                var layout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 3,
                    Padding = new Padding(10)
                };
                var input = new TextBox
                {
                    Text = GetText(_appHistory[selection], "note"),
                    Width = 300,
                    TabIndex = 0
                };
                var ok = new Button
                {
                    Text = "确定",
                    AutoSize = true,
                    TabIndex = 1,
                    DialogResult = DialogResult.OK
                };
                var cancel = new Button
                {
                    Text = "取消",
                    AutoSize = true,
                    TabIndex = 2,
                    DialogResult = DialogResult.Cancel
                };
                var buttons = new FlowLayoutPanel
                {
                    AutoSize = true,
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.RightToLeft,
                    WrapContents = false,
                    TabStop = false
                };
                buttons.Controls.Add(cancel);
                buttons.Controls.Add(ok);
                layout.Controls.Add(new Label
                {
                    Text = "请输入路径备注:",
                    AutoSize = true,
                    TabStop = false
                });
                layout.Controls.Add(input);
                layout.Controls.Add(buttons);
                dialog.Controls.Add(layout);
                dialog.AcceptButton = ok;
                dialog.CancelButton = cancel;
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    UpdateSelectedNote(input.Text);
                }
            }
        }

        private static Label CreateLabel(string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                TabStop = false,
                Margin = new Padding(0, 5, 5, 0)
            };
        }

        private static object GetValue(IDictionary<string, object> values, string key, object defaultValue)
        {
            object value;
            return values != null && values.TryGetValue(key, out value) ? value : defaultValue;
        }

        private static string GetText(IDictionary<string, object> values, string key)
        {
            return Convert.ToString(GetValue(values, key, string.Empty)) ?? string.Empty;
        }
    }
}
