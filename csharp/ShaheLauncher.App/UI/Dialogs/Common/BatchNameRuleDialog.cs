using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ShaheLauncher.App.UI.Dialogs.Common
{
    public sealed class BatchNameRuleDialog : Form
    {
        private readonly int? _fixedCount;
        private readonly List<KeyValuePair<int, string>> _digitChoices;

        public BatchNameRuleDialog(
            Form parent,
            string title,
            string prompt,
            string defaultPrefix,
            int defaultStart,
            int defaultDigits,
            int defaultCount,
            int? fixedCount,
            IEnumerable<KeyValuePair<int, string>> digitChoiceValues)
        {
            if (parent != null)
            {
                Owner = parent;
            }

            _fixedCount = fixedCount;
            _digitChoices = (digitChoiceValues ?? Enumerable.Empty<KeyValuePair<int, string>>())
                .Select(item => new KeyValuePair<int, string>(item.Key, (item.Value ?? string.Empty).Trim()))
                .Where(item => item.Value.Length != 0)
                .ToList();
            Text = title ?? string.Empty;
            Size = new Size(430, fixedCount.HasValue ? 300 : 320);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            AutoScaleMode = AutoScaleMode.None;
            InitializeUi(prompt ?? string.Empty, defaultPrefix ?? string.Empty, defaultStart, defaultDigits, defaultCount);
        }

        public IDictionary Result { get; private set; }
        public Label PromptLabel { get; private set; }
        public Label ExampleLabel { get; private set; }
        public TextBox PrefixTextBox { get; private set; }
        public NumericUpDown StartInput { get; private set; }
        public NumericUpDown DigitsInput { get; private set; }
        public ComboBox DigitsChoice { get; private set; }
        public NumericUpDown CountInput { get; private set; }
        public Label TargetCountLabel { get; private set; }
        public Button OkButton { get; private set; }
        public Button CancelActionButton { get; private set; }

        public bool ConfirmRule()
        {
            string prefix = (PrefixTextBox.Text ?? string.Empty).Trim();
            if (prefix.Length == 0)
            {
                DialogMessageService.Show(this, "名称前缀不能为空。", "输入错误", MessageBoxIcon.Warning);
                return false;
            }

            int digits;
            if (_digitChoices.Count != 0)
            {
                int selectedIndex = DigitsChoice.SelectedIndex;
                if (selectedIndex < 0 || selectedIndex >= _digitChoices.Count)
                {
                    DialogMessageService.Show(this, "请选择编号格式。", "输入错误", MessageBoxIcon.Warning);
                    return false;
                }

                digits = _digitChoices[selectedIndex].Key;
            }
            else
            {
                digits = Decimal.ToInt32(DigitsInput.Value);
            }

            int count = _fixedCount ?? Decimal.ToInt32(CountInput.Value);
            if (count <= 0)
            {
                DialogMessageService.Show(this, "数量必须大于 0。", "输入错误", MessageBoxIcon.Warning);
                return false;
            }

            Result = new Dictionary<string, object>
            {
                { "prefix", prefix },
                { "start", Math.Max(0, Decimal.ToInt32(StartInput.Value)) },
                { "digits", Math.Max(0, digits) },
                { "count", count }
            };
            DialogResult = DialogResult.OK;
            Close();
            return true;
        }

        private void InitializeUi(string prompt, string defaultPrefix, int defaultStart, int defaultDigits, int defaultCount)
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                ColumnCount = 1,
                Padding = new Padding(10)
            };
            PromptLabel = CreateTextLabel(prompt);
            ExampleLabel = CreateTextLabel(
                "示例：前缀=沙盒用户，起始=1，位数=2，数量=3 -> 沙盒用户01、沙盒用户02、沙盒用户03");
            PrefixTextBox = new TextBox
            {
                Text = defaultPrefix,
                AccessibleName = "名称前缀",
                Dock = DockStyle.Fill,
                TabIndex = 0
            };
            StartInput = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 999999,
                Value = Clamp(defaultStart, 0, 999999),
                AccessibleName = "起始编号",
                Width = 100,
                TabIndex = 1
            };
            layout.Controls.Add(PromptLabel);
            layout.Controls.Add(ExampleLabel);
            layout.Controls.Add(CreateLabeledRow("名称前缀:", PrefixTextBox));
            layout.Controls.Add(CreateLabeledRow("起始编号（第一个编号）:", StartInput));
            AddDigitsRow(layout, defaultDigits);
            AddCountRow(layout, defaultCount);
            AddButtons(layout);
            Controls.Add(layout);
            AcceptButton = OkButton;
            CancelButton = CancelActionButton;
            Shown += (sender, eventArgs) => PrefixTextBox.Focus();
        }

        private void AddDigitsRow(TableLayoutPanel layout, int defaultDigits)
        {
            if (_digitChoices.Count != 0)
            {
                DigitsChoice = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    AccessibleName = "编号格式",
                    Width = 180,
                    TabIndex = 2
                };
                DigitsChoice.Items.AddRange(_digitChoices.Select(item => (object)item.Value).ToArray());
                int expectedDigits = Math.Max(0, defaultDigits);
                int selectedIndex = _digitChoices.FindIndex(item => item.Key == expectedDigits);
                DigitsChoice.SelectedIndex = selectedIndex >= 0 ? selectedIndex : 0;
                layout.Controls.Add(CreateLabeledRow("编号格式:", DigitsChoice));
                return;
            }

            DigitsInput = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 8,
                Value = Clamp(defaultDigits, 0, 8),
                AccessibleName = "编号位数",
                Width = 100,
                TabIndex = 2
            };
            layout.Controls.Add(CreateLabeledRow("编号位数（0=不补零，2=01）:", DigitsInput));
        }

        private void AddCountRow(TableLayoutPanel layout, int defaultCount)
        {
            if (_fixedCount.HasValue)
            {
                TargetCountLabel = CreateTextLabel("目标数量: " + Math.Max(0, _fixedCount.Value));
                layout.Controls.Add(TargetCountLabel);
                return;
            }

            CountInput = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 5000,
                Value = Clamp(defaultCount, 1, 5000),
                AccessibleName = "创建数量",
                Width = 100,
                TabIndex = 3
            };
            layout.Controls.Add(CreateLabeledRow("创建数量（要创建几个用户）:", CountInput));
        }

        private void AddButtons(TableLayoutPanel layout)
        {
            int firstButtonTabIndex = _fixedCount.HasValue ? 3 : 4;
            OkButton = new Button
            {
                Text = "确定",
                AutoSize = true,
                TabIndex = firstButtonTabIndex
            };
            OkButton.Click += (sender, eventArgs) => ConfirmRule();
            CancelActionButton = new Button
            {
                Text = "取消",
                AutoSize = true,
                DialogResult = DialogResult.Cancel,
                TabIndex = firstButtonTabIndex + 1
            };

            var buttons = new FlowLayoutPanel
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                TabStop = false
            };
            buttons.Controls.Add(OkButton);
            buttons.Controls.Add(CancelActionButton);
            layout.Controls.Add(buttons);
        }

        private static Label CreateTextLabel(string text)
        {
            return new Label
            {
                Text = text ?? string.Empty,
                AutoSize = true,
                Dock = DockStyle.Fill,
                TabStop = false
            };
        }

        private static Control CreateLabeledRow(string labelText, Control input)
        {
            var row = new TableLayoutPanel
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                TabStop = false
            };
            row.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            var label = CreateTextLabel(labelText);
            label.Anchor = AnchorStyles.Left;
            input.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            row.Controls.Add(label, 0, 0);
            row.Controls.Add(input, 1, 0);
            return row;
        }

        private static decimal Clamp(int value, int minimum, int maximum)
        {
            return Math.Min(maximum, Math.Max(minimum, value));
        }
    }
}
