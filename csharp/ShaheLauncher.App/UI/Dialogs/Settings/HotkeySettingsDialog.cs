using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ShaheLauncher.App.UI.Dialogs.Common;

namespace ShaheLauncher.App.UI.Dialogs.Settings
{
    public sealed class HotkeySettingsDialog : Form
    {
        private sealed class ActionRow
        {
            public string Action { get; set; }
            public string Label { get; set; }
        }

        private readonly List<ActionRow> _actions;
        private readonly Dictionary<string, string> _defaultHotkeys;
        private readonly List<IDictionary<string, object>> _reservedHotkeys;
        private readonly Func<string, Tuple<int, int>> _parseHotkey;
        private readonly Func<int, int, string> _formatHotkey;

        public HotkeySettingsDialog(
            Form parent,
            IEnumerable<IDictionary<string, object>> actionSpecs,
            IDictionary<string, string> hotkeys,
            IDictionary<string, string> defaultHotkeys,
            Func<string, Tuple<int, int>> parseHotkeyCallback,
            Func<int, int, string> formatHotkeyCallback,
            bool hotkeysEnabled,
            IEnumerable<IDictionary<string, object>> reservedHotkeys)
        {
            if (parent != null)
            {
                Owner = parent;
            }

            _actions = (actionSpecs ?? Enumerable.Empty<IDictionary<string, object>>())
                .Select(spec => new ActionRow
                {
                    Action = GetText(spec, "action"),
                    Label = GetText(spec, "label")
                })
                .ToList();
            _defaultHotkeys = new Dictionary<string, string>(
                defaultHotkeys ?? new Dictionary<string, string>());
            _reservedHotkeys = (reservedHotkeys ?? Enumerable.Empty<IDictionary<string, object>>()).ToList();
            _parseHotkey = parseHotkeyCallback;
            _formatHotkey = formatHotkeyCallback;
            WorkingHotkeys = _actions.ToDictionary(
                row => row.Action,
                row => GetHotkey(hotkeys, row.Action));
            ResultHotkeys = new Dictionary<string, string>(hotkeys ?? new Dictionary<string, string>());
            ResultHotkeysEnabled = hotkeysEnabled;

            Text = "快捷键设置";
            Size = new Size(760, 560);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            AutoScaleMode = AutoScaleMode.None;
            InitializeUi(hotkeysEnabled);
        }

        public ListBox ActionList { get; private set; }
        public TextBox HotkeyTextBox { get; private set; }
        public CheckBox HotkeysEnabledCheckBox { get; private set; }
        public Button ResetButton { get; private set; }
        public Button OkButton { get; private set; }
        public Button CancelActionButton { get; private set; }
        public IDictionary<string, string> WorkingHotkeys { get; private set; }
        public IDictionary<string, string> ResultHotkeys { get; private set; }
        public bool ResultHotkeysEnabled { get; private set; }
        public string CurrentAction { get; private set; }

        public void SelectAction(int index)
        {
            if (index < 0 || index >= _actions.Count)
            {
                return;
            }

            if (ActionList.SelectedIndex == index)
            {
                LoadSelectedHotkey();
                return;
            }

            ActionList.SelectedIndex = index;
        }

        public bool CaptureHotkey(Keys keyCode, Keys modifiers)
        {
            if (keyCode == Keys.Tab)
            {
                return false;
            }

            if (keyCode == Keys.ShiftKey || keyCode == Keys.ControlKey || keyCode == Keys.Menu)
            {
                return true;
            }

            if (keyCode == Keys.Back || keyCode == Keys.Delete)
            {
                HotkeyTextBox.Text = string.Empty;
                SaveEditorHotkey();
                return true;
            }

            Tuple<int, int> parsed = null;
            string text = string.Empty;
            try
            {
                text = _formatHotkey == null ? string.Empty : _formatHotkey((int)modifiers, (int)keyCode);
                parsed = string.IsNullOrEmpty(text) || _parseHotkey == null ? null : _parseHotkey(text);
            }
            catch
            {
                parsed = null;
            }

            if (parsed == null)
            {
                return true;
            }

            HotkeyTextBox.Text = _formatHotkey(parsed.Item1, parsed.Item2);
            HotkeyTextBox.SelectionStart = HotkeyTextBox.TextLength;
            SaveEditorHotkey();
            return true;
        }

        public bool ResetHotkeys()
        {
            if (!DialogMessageService.Confirm(
                this,
                "确定要将全部快捷键恢复为默认值吗？",
                "重置快捷键",
                MessageBoxIcon.Question))
            {
                return false;
            }

            WorkingHotkeys = _actions.ToDictionary(
                row => row.Action,
                row => GetHotkey(_defaultHotkeys, row.Action));
            HotkeysEnabledCheckBox.Checked = false;
            LoadSelectedHotkey();
            return true;
        }

        public bool ConfirmSettings()
        {
            SaveEditorHotkey();
            var newHotkeys = new Dictionary<string, string>();
            var invalid = new List<string>();
            var reservedConflicts = new List<string>();
            var used = new Dictionary<Tuple<int, int>, string>();
            var duplicates = new Dictionary<Tuple<int, int>, List<string>>();
            var reservedUsed = BuildReservedHotkeys();

            foreach (ActionRow row in _actions)
            {
                string rawText = GetHotkey(WorkingHotkeys, row.Action).Trim();
                if (rawText.Length == 0)
                {
                    newHotkeys[row.Action] = string.Empty;
                    continue;
                }

                Tuple<int, int> parsed = ParseHotkey(rawText);
                if (parsed == null)
                {
                    invalid.Add(row.Label + ": " + rawText);
                    continue;
                }

                string reservedLabel;
                if (reservedUsed.TryGetValue(parsed, out reservedLabel))
                {
                    reservedConflicts.Add(
                        row.Label + ": " + rawText + "（已用于" + reservedLabel + "）");
                    continue;
                }

                string usedLabel;
                if (used.TryGetValue(parsed, out usedLabel))
                {
                    List<string> labels;
                    if (!duplicates.TryGetValue(parsed, out labels))
                    {
                        labels = new List<string> { usedLabel };
                        duplicates[parsed] = labels;
                    }

                    labels.Add(row.Label);
                    continue;
                }

                used[parsed] = row.Label;
                try
                {
                    newHotkeys[row.Action] = _formatHotkey(parsed.Item1, parsed.Item2);
                }
                catch
                {
                    newHotkeys[row.Action] = rawText;
                }
            }

            if (invalid.Count != 0)
            {
                DialogMessageService.Show(
                    this,
                    "以下快捷键格式无效，请修改后重试：\n" + string.Join("\n", invalid),
                    "快捷键校验失败",
                    MessageBoxIcon.Warning);
                return false;
            }

            if (reservedConflicts.Count != 0)
            {
                DialogMessageService.Show(
                    this,
                    "以下快捷键已被固定基础快捷键占用，请修改后重试：\n" +
                    string.Join("\n", reservedConflicts),
                    "快捷键冲突",
                    MessageBoxIcon.Warning);
                return false;
            }

            if (duplicates.Count != 0)
            {
                DialogMessageService.Show(
                    this,
                    "以下功能使用了重复快捷键，请修改后重试：\n" +
                    string.Join("\n", duplicates.Values.Select(labels => string.Join("、", labels))),
                    "快捷键冲突",
                    MessageBoxIcon.Warning);
                return false;
            }

            foreach (ActionRow row in _actions)
            {
                if (!newHotkeys.ContainsKey(row.Action))
                {
                    newHotkeys[row.Action] = string.Empty;
                }
            }

            ResultHotkeys = newHotkeys;
            ResultHotkeysEnabled = HotkeysEnabledCheckBox.Checked;
            DialogResult = DialogResult.OK;
            Close();
            return true;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Keys keyCode = keyData & Keys.KeyCode;
            if (keyCode == Keys.Escape)
            {
                if (ActiveControl == HotkeyTextBox)
                {
                    CaptureHotkey(Keys.Escape, keyData & Keys.Modifiers);
                }
                else
                {
                    DialogResult = DialogResult.Cancel;
                    Close();
                }

                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void InitializeUi(bool hotkeysEnabled)
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
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.Controls.Add(new Label
            {
                Text = "快捷键列表",
                AutoSize = true,
                Dock = DockStyle.Fill,
                TabStop = false,
                Margin = new Padding(0, 0, 0, 10)
            }, 0, 0);

            var content = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                TabStop = false,
                Margin = new Padding(0)
            };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62F));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38F));
            ActionList = new ListBox
            {
                Dock = DockStyle.Fill,
                AccessibleName = "快捷键功能列表",
                TabIndex = 0,
                Margin = new Padding(0, 0, 10, 0)
            };
            foreach (ActionRow row in _actions)
            {
                ActionList.Items.Add(row.Label);
            }

            var editor = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                ColumnCount = 1,
                RowCount = 4,
                TabStop = false,
                Margin = new Padding(0)
            };
            editor.Controls.Add(new Label
            {
                Text = "当前功能快捷键：",
                AutoSize = true,
                TabStop = false,
                Margin = new Padding(0, 0, 0, 5)
            }, 0, 0);
            HotkeyTextBox = new TextBox
            {
                AccessibleName = "当前功能快捷键",
                Dock = DockStyle.Top,
                TabIndex = 1,
                Margin = new Padding(0, 0, 0, 10)
            };
            HotkeysEnabledCheckBox = new CheckBox
            {
                Text = "启用快捷键",
                Checked = hotkeysEnabled,
                AutoSize = true,
                TabIndex = 2,
                Margin = new Padding(0, 0, 0, 12)
            };
            editor.Controls.Add(HotkeyTextBox, 0, 1);
            editor.Controls.Add(HotkeysEnabledCheckBox, 0, 2);
            content.Controls.Add(ActionList, 0, 0);
            content.Controls.Add(editor, 1, 0);
            layout.Controls.Add(content, 0, 1);

            ResetButton = new Button { Text = "重置所有快捷键设置", AutoSize = true, TabIndex = 3 };
            OkButton = new Button { Text = "确定", AutoSize = true, TabIndex = 4 };
            CancelActionButton = new Button
            {
                Text = "取消",
                AutoSize = true,
                TabIndex = 5,
                DialogResult = DialogResult.Cancel
            };
            var buttons = CreateButtons();
            layout.Controls.Add(buttons, 0, 2);
            Controls.Add(layout);
            AcceptButton = OkButton;

            ActionList.SelectedIndexChanged += (sender, eventArgs) =>
            {
                SaveEditorHotkey();
                LoadSelectedHotkey();
            };
            HotkeyTextBox.Leave += (sender, eventArgs) => SaveEditorHotkey();
            HotkeyTextBox.KeyDown += OnHotkeyKeyDown;
            ResetButton.Click += (sender, eventArgs) => ResetHotkeys();
            OkButton.Click += (sender, eventArgs) => ConfirmSettings();
            CancelActionButton.Click += (sender, eventArgs) => Close();

            if (ActionList.Items.Count != 0)
            {
                ActionList.SelectedIndex = 0;
            }
            Shown += (sender, eventArgs) => ActionList.Focus();
        }

        private Control CreateButtons()
        {
            var row = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                ColumnCount = 2,
                RowCount = 1,
                TabStop = false,
                Margin = new Padding(0, 10, 0, 0)
            };
            row.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            var right = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                TabStop = false,
                Margin = new Padding(0)
            };
            CancelActionButton.Margin = new Padding(0);
            OkButton.Margin = new Padding(0, 0, 6, 0);
            right.Controls.Add(CancelActionButton);
            right.Controls.Add(OkButton);
            row.Controls.Add(ResetButton, 0, 0);
            row.Controls.Add(right, 1, 0);
            return row;
        }

        private void OnHotkeyKeyDown(object sender, KeyEventArgs eventArgs)
        {
            bool handled = CaptureHotkey(eventArgs.KeyCode, eventArgs.Modifiers);
            if (!handled)
            {
                return;
            }

            eventArgs.Handled = true;
            eventArgs.SuppressKeyPress = true;
        }

        private void LoadSelectedHotkey()
        {
            int index = ActionList.SelectedIndex;
            if (index < 0 || index >= _actions.Count)
            {
                CurrentAction = string.Empty;
                HotkeyTextBox.Text = string.Empty;
                return;
            }

            CurrentAction = _actions[index].Action;
            HotkeyTextBox.Text = GetHotkey(WorkingHotkeys, CurrentAction);
            HotkeyTextBox.SelectionStart = HotkeyTextBox.TextLength;
        }

        private void SaveEditorHotkey()
        {
            if (string.IsNullOrEmpty(CurrentAction))
            {
                return;
            }

            WorkingHotkeys[CurrentAction] = (HotkeyTextBox.Text ?? string.Empty).Trim();
        }

        private Dictionary<Tuple<int, int>, string> BuildReservedHotkeys()
        {
            var result = new Dictionary<Tuple<int, int>, string>();
            foreach (IDictionary<string, object> item in _reservedHotkeys)
            {
                string hotkeyText = GetText(item, "hotkey");
                if (hotkeyText.Length == 0)
                {
                    continue;
                }

                Tuple<int, int> parsed = ParseHotkey(hotkeyText);
                if (parsed != null)
                {
                    string label = GetText(item, "label");
                    result[parsed] = label.Length == 0 ? hotkeyText : label;
                }
            }

            return result;
        }

        private Tuple<int, int> ParseHotkey(string text)
        {
            try
            {
                return _parseHotkey == null ? null : _parseHotkey(text);
            }
            catch
            {
                return null;
            }
        }

        private static string GetHotkey(IDictionary<string, string> values, string action)
        {
            string value;
            return values != null && values.TryGetValue(action, out value)
                ? value ?? string.Empty
                : string.Empty;
        }

        private static string GetText(IDictionary<string, object> values, string key)
        {
            object value;
            return values != null && values.TryGetValue(key, out value)
                ? Convert.ToString(value)?.Trim() ?? string.Empty
                : string.Empty;
        }
    }
}
