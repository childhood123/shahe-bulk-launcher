using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ShaheLauncher.App.UI.Dialogs.Common;
using ShaheLauncher.Core.Common;
using ShaheLauncher.Core.Constants;

namespace ShaheLauncher.App.UI.Dialogs.Settings
{
    public sealed class SettingsDialog : Form
    {
        private readonly IDictionary<string, object> _settings;
        private readonly IDictionary<string, object> _defaultSettings;
        private readonly Action<IDictionary<string, object>> _onSave;

        public SettingsDialog(
            Form parent,
            IDictionary<string, object> settings,
            Action<IDictionary<string, object>> onSaveCallback,
            IDictionary<string, object> defaultSettings)
        {
            if (parent != null)
            {
                Owner = parent;
            }

            _settings = settings ?? new Dictionary<string, object>();
            _defaultSettings = defaultSettings ?? new Dictionary<string, object>();
            _onSave = onSaveCallback;
            Text = "设置";
            Size = new Size(560, 640);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            AutoScaleMode = AutoScaleMode.None;
            InitializeUi();
        }

        public NumericUpDown LaunchIntervalInput { get; private set; }
        public ComboBox LaunchConcurrencyInput { get; private set; }
        public CheckBox AutoRunCheckBox { get; private set; }
        public CheckBox ShowTrayCheckBox { get; private set; }
        public CheckBox AutoSelectRangeCheckBox { get; private set; }
        public CheckBox SkipRunningCheckBox { get; private set; }
        public CheckBox RememberVolumeCheckBox { get; private set; }
        public CheckBox ListNavigationCheckBox { get; private set; }
        public ComboBox LogOutputChoice { get; private set; }
        public Button ResetButton { get; private set; }
        public Button OkButton { get; private set; }
        public Button CancelActionButton { get; private set; }

        public bool ResetDefaults()
        {
            if (!DialogMessageService.Confirm(
                this,
                "确定要将设置窗口中的选项恢复为默认值吗？",
                "重置设置",
                MessageBoxIcon.Question))
            {
                return false;
            }

            ApplyValues(_defaultSettings);
            return true;
        }

        public bool SaveSettings()
        {
            _settings["launch_interval"] = Decimal.ToInt32(LaunchIntervalInput.Value);
            _settings["launch_concurrency_limit"] = Convert.ToInt32(LaunchConcurrencyInput.SelectedItem);
            _settings.Remove("dynamic_launch_delay_by_load");
            _settings["auto_run"] = AutoRunCheckBox.Checked;
            _settings.Remove("auto_update_on_startup");
            _settings.Remove("auto_check_update_on_startup");
            _settings["show_tray"] = ShowTrayCheckBox.Checked;
            _settings["auto_select_range_on_check"] = AutoSelectRangeCheckBox.Checked;
            _settings["auto_minimize_to_tray_after_launch"] = false;
            _settings["skip_running_sandbox_on_launch"] = SkipRunningCheckBox.Checked;
            _settings["remember_volume"] = RememberVolumeCheckBox.Checked;
            _settings["list_use_left_right_nav"] = ListNavigationCheckBox.Checked;
            _settings["log_output_mode"] = GetSelectedLogOutputMode();
            _onSave?.Invoke(_settings);
            DialogResult = DialogResult.OK;
            Close();
            return true;
        }

        public void ApplyValues(IDictionary<string, object> values)
        {
            IDictionary<string, object> data = values ?? new Dictionary<string, object>();
            LaunchIntervalInput.Value = CommonUtilities.ParseIntSetting(
                GetValue(data, "launch_interval", 5), 5, 0, 15);
            int maximumConcurrency = LauncherConstants.GetLaunchConcurrencyMaximum();
            int selectedConcurrency = CommonUtilities.ParseIntSetting(
                GetValue(data, "launch_concurrency_limit", LauncherConstants.GetDefaultLaunchConcurrencyLimit()),
                LauncherConstants.GetDefaultLaunchConcurrencyLimit(),
                1,
                maximumConcurrency);
            LaunchConcurrencyInput.SelectedItem = selectedConcurrency;
            AutoRunCheckBox.Checked = GetBool(data, "auto_run", false);
            ShowTrayCheckBox.Checked = GetBool(data, "show_tray", true);
            AutoSelectRangeCheckBox.Checked = GetBool(data, "auto_select_range_on_check", false);
            SkipRunningCheckBox.Checked = GetBool(data, "skip_running_sandbox_on_launch", false);
            RememberVolumeCheckBox.Checked = GetBool(data, "remember_volume", true);
            ListNavigationCheckBox.Checked = GetBool(data, "list_use_left_right_nav", true);
            SetLogOutputChoice(Convert.ToString(GetValue(data, "log_output_mode", LauncherConstants.LogOutputDetailed)));
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
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                TabStop = false
            };
            var layout = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Top,
                ColumnCount = 1,
                Padding = new Padding(10),
                TabStop = false
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            LaunchIntervalInput = CreateNumberInput(0, 15, "打开沙盒间隔时间", 0);
            LaunchConcurrencyInput = CreateConcurrencyInput("启动并发数量限制", 1);
            AutoRunCheckBox = CreateCheckBox("开机自动启动本程序", 2);
            ShowTrayCheckBox = CreateCheckBox("在托盘上显示", 3);
            AutoSelectRangeCheckBox = CreateCheckBox("首尾两点自动勾选中间用户", 4);
            SkipRunningCheckBox = CreateCheckBox("启动跳过已运行的沙盒", 5);
            RememberVolumeCheckBox = CreateCheckBox("记忆进程音量（下次启动自动恢复）", 6);
            ListNavigationCheckBox = CreateCheckBox("用户列表启用左右光标选择", 7);
            LogOutputChoice = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                AccessibleName = "日志输出",
                Width = 150,
                TabIndex = 8
            };
            foreach (KeyValuePair<string, string> choice in LauncherConstants.LogOutputChoices)
            {
                LogOutputChoice.Items.Add(choice.Value);
            }

            layout.Controls.Add(CreateNumberRow(
                "打开沙盒间隔时间（秒）:", LaunchIntervalInput, "(0=无等待，快速启动)"));
            layout.Controls.Add(CreateNumberRow(
                "启动并发数量限制:", LaunchConcurrencyInput, "(1=排队顺序启动)"));
            layout.Controls.Add(CreateLabel(
                "说明：并发上限=同一时刻最多启动几个；启动间隔=每次分配启动任务前的等待秒数。"));
            layout.Controls.Add(AutoRunCheckBox);
            layout.Controls.Add(ShowTrayCheckBox);
            layout.Controls.Add(AutoSelectRangeCheckBox);
            layout.Controls.Add(SkipRunningCheckBox);
            layout.Controls.Add(RememberVolumeCheckBox);
            layout.Controls.Add(ListNavigationCheckBox);
            layout.Controls.Add(CreateNumberRow("日志输出:", LogOutputChoice, string.Empty));

            ResetButton = new Button { Text = "重置所有设置", AutoSize = true, TabIndex = 9 };
            OkButton = new Button { Text = "确定", AutoSize = true, TabIndex = 10 };
            CancelActionButton = new Button
            {
                Text = "取消",
                AutoSize = true,
                TabIndex = 11,
                DialogResult = DialogResult.Cancel
            };
            layout.Controls.Add(CreateButtonRow());
            panel.Controls.Add(layout);
            Controls.Add(panel);
            AcceptButton = OkButton;
            CancelButton = CancelActionButton;
            ResetButton.Click += (sender, eventArgs) => ResetDefaults();
            OkButton.Click += (sender, eventArgs) => SaveSettings();
            CancelActionButton.Click += (sender, eventArgs) => Close();
            ApplyValues(_settings);
        }

        private Control CreateButtonRow()
        {
            var row = new TableLayoutPanel
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
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
            OkButton.Margin = new Padding(0, 0, 5, 0);
            right.Controls.Add(CancelActionButton);
            right.Controls.Add(OkButton);
            row.Controls.Add(ResetButton, 0, 0);
            row.Controls.Add(right, 1, 0);
            return row;
        }

        private void SetLogOutputChoice(string mode)
        {
            string selectedMode = LauncherConstants.NormalizeLogOutputMode(mode);
            for (int index = 0; index < LauncherConstants.LogOutputChoices.Count; index++)
            {
                if (LauncherConstants.LogOutputChoices[index].Key == selectedMode)
                {
                    LogOutputChoice.SelectedIndex = index;
                    return;
                }
            }

            LogOutputChoice.SelectedIndex = 1;
        }

        private string GetSelectedLogOutputMode()
        {
            int index = LogOutputChoice.SelectedIndex;
            return index >= 0 && index < LauncherConstants.LogOutputChoices.Count
                ? LauncherConstants.LogOutputChoices[index].Key
                : LauncherConstants.LogOutputDetailed;
        }

        private static NumericUpDown CreateNumberInput(
            int minimum,
            int maximum,
            string accessibleName,
            int tabIndex)
        {
            return new NumericUpDown
            {
                Minimum = minimum,
                Maximum = maximum,
                AccessibleName = accessibleName,
                Width = 80,
                TabIndex = tabIndex
            };
        }

        private static ComboBox CreateConcurrencyInput(string accessibleName, int tabIndex)
        {
            int maximum = LauncherConstants.GetLaunchConcurrencyMaximum();
            int defaultValue = LauncherConstants.GetDefaultLaunchConcurrencyLimit();
            var input = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                AccessibleName = accessibleName,
                Width = 80,
                TabIndex = tabIndex
            };
            for (int value = 1; value <= maximum; value++)
            {
                input.Items.Add(value);
            }

            input.SelectedIndex = defaultValue - 1;
            return input;
        }

        private static CheckBox CreateCheckBox(string text, int tabIndex)
        {
            return new CheckBox
            {
                Text = text,
                AutoSize = true,
                TabIndex = tabIndex,
                Margin = new Padding(0, 0, 0, 10)
            };
        }

        private static Control CreateNumberRow(string labelText, Control input, string suffixText)
        {
            var row = new FlowLayoutPanel
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                TabStop = false,
                Margin = new Padding(0, 0, 0, 10)
            };
            row.Controls.Add(CreateLabel(labelText));
            row.Controls.Add(input);
            if (!string.IsNullOrEmpty(suffixText))
            {
                row.Controls.Add(CreateLabel(suffixText));
            }

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

        private static bool GetBool(IDictionary<string, object> values, string key, bool defaultValue)
        {
            return CommonUtilities.ParseBoolSetting(GetValue(values, key, defaultValue), defaultValue);
        }

        private static object GetValue(IDictionary<string, object> values, string key, object defaultValue)
        {
            object value;
            return values != null && values.TryGetValue(key, out value) ? value : defaultValue;
        }
    }
}
