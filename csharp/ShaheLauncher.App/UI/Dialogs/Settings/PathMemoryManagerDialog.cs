using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ShaheLauncher.Core.AppPaths;
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
            Size = new Size(500, 160);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            AutoScaleMode = AutoScaleMode.None;
            InitializeUi();
        }

        public Label PromptLabel { get; private set; }
        public NumericUpDown HistoryCountInput { get; private set; }
        public Button OkButton { get; private set; }
        public Button CancelActionButton { get; private set; }

        public bool SaveChanges()
        {
            _settings["max_history_items"] = Decimal.ToInt32(HistoryCountInput.Value);
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
                RowCount = 2,
                Padding = new Padding(10),
                TabStop = false
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            HistoryCountInput = new NumericUpDown
            {
                Minimum = 0,
                Maximum = AppPathHistoryService.MaximumHistoryItems,
                Value = CommonUtilities.ParseIntSetting(
                    GetValue(_settings, "max_history_items", 10),
                    10,
                    0,
                    AppPathHistoryService.MaximumHistoryItems),
                AccessibleName = "路径记忆数量",
                Width = 80,
                TabIndex = 0
            };
            OkButton = new Button
            {
                Text = "确定",
                AccessibleName = "确定",
                AutoSize = true,
                TabIndex = 1
            };
            CancelActionButton = new Button
            {
                Text = "取消",
                AccessibleName = "取消",
                AutoSize = true,
                TabIndex = 2,
                DialogResult = DialogResult.Cancel
            };

            layout.Controls.Add(CreateCountRow(), 0, 0);
            layout.Controls.Add(CreateButtonRow(), 0, 1);
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
            PromptLabel = CreateLabel("路径记忆数量:");
            row.Controls.Add(PromptLabel);
            row.Controls.Add(HistoryCountInput);
            row.Controls.Add(CreateLabel(
                "(0=不记录, 最大" + AppPathHistoryService.MaximumHistoryItems + ")"));
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
    }
}
