using System;
using ShaheLauncher.App.Infrastructure;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ShaheLauncher.Core.Common;

namespace ShaheLauncher.App.UI.Dialogs.Settings
{
    public sealed class SpeechSettingsDialog : Form
    {
        private static readonly HashSet<string> BlockedEngines = new HashSet<string>
        {
            "VTTS", "None", "_None", "无语音输出_None", "无语音输出"
        };

        private readonly Dictionary<int, string> _interfaceValues = new Dictionary<int, string>();
        private readonly Func<IDictionary<string, object>, bool> _onSaveConfig;
        private readonly Func<string, bool> _onSaveInterfaceDll;
        private readonly Func<IDictionary<string, object>, bool> _onSaveAssistHints;

        public SpeechSettingsDialog(
            Form parent,
            IDictionary<string, object> speechConfig,
            Func<IDictionary<string, object>, bool> onSaveCallback,
            IDictionary<string, object> readerIniSettings,
            Func<IDictionary<string, object>, bool> onSaveReaderIniCallback,
            Func<IList<string>, IDictionary<string, object>> detectReaderCallback,
            IEnumerable<object> interfaceDllChoices,
            string selectedInterfaceDll,
            Func<string, bool> onSaveInterfaceDllCallback,
            IDictionary<string, object> assistHints,
            Func<IDictionary<string, object>, bool> onSaveAssistHintsCallback)
        {
            if (parent != null)
            {
                Owner = parent;
            }

            _onSaveConfig = onSaveCallback;
            _onSaveInterfaceDll = onSaveInterfaceDllCallback;
            _onSaveAssistHints = onSaveAssistHintsCallback;
            Config = SanitizeConfig(speechConfig);
            ReaderIniSettings = NormalizeReaderIniSettings(readerIniSettings);
            EngineNames = ReadDictionary(Config, "TTSInfos").Keys.ToList();
            Text = "朗读设置";
            Size = new Size(700, 700);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            AutoScaleMode = AutoScaleMode.None;
            InitializeUi(interfaceDllChoices, selectedInterfaceDll, assistHints);
        }

        public IDictionary<string, object> Config { get; private set; }
        public IDictionary<string, object> ReaderIniSettings { get; private set; }
        public IList<string> EngineNames { get; private set; }
        public ComboBox InterfaceChoice { get; private set; }
        public CheckBox AssistEnabledCheckBox { get; private set; }
        public Button OkButton { get; private set; }
        public Button CancelActionButton { get; private set; }

        public static IDictionary<string, object> SanitizeConfig(IDictionary<string, object> config)
        {
            var result = new Dictionary<string, object>(config ?? new Dictionary<string, object>());
            var cleanedInfos = new Dictionary<string, object>();
            string firstEngine = string.Empty;
            object rawInfos;
            var infos = config != null && config.TryGetValue("TTSInfos", out rawInfos)
                ? rawInfos as IDictionary<string, object>
                : null;
            if (infos != null)
            {
                foreach (KeyValuePair<string, object> pair in infos)
                {
                    string engineName = (pair.Key ?? string.Empty).Trim();
                    if (engineName.Length == 0 || BlockedEngines.Contains(engineName))
                    {
                        continue;
                    }

                    var item = pair.Value as IDictionary<string, object> ?? new Dictionary<string, object>();
                    var cleaned = new Dictionary<string, object>
                    {
                        { "Speed", ClampInt(GetValue(item, "Speed", 100), 100, 0, 200) },
                        { "Volume", ClampInt(GetValue(item, "Volume", 100), 100, 0, 100) },
                        { "Pitch", ClampInt(GetValue(item, "Pitch", 50), 50, 0, 100) },
                        { "Name", Convert.ToString(GetValue(item, "Name", string.Empty))?.Trim() ?? string.Empty }
                    };
                    cleanedInfos[engineName] = cleaned;
                    if (firstEngine.Length == 0)
                    {
                        firstEngine = engineName;
                    }
                }
            }

            result["TTSInfos"] = cleanedInfos;
            string defaultEngine = Convert.ToString(GetValue(result, "DefaultEngine", string.Empty))?.Trim()
                ?? string.Empty;
            if (BlockedEngines.Contains(defaultEngine) || !cleanedInfos.ContainsKey(defaultEngine))
            {
                defaultEngine = string.Empty;
            }
            if (defaultEngine.Length == 0)
            {
                defaultEngine = firstEngine;
            }
            result["DefaultEngine"] = defaultEngine;
            return result;
        }

        public static IDictionary<string, object> NormalizeReaderIniSettings(
            IDictionary<string, object> settings)
        {
            var source = settings ?? new Dictionary<string, object>();
            IDictionary<string, object> zdsrSource = ReadDictionary(source, "zdsr");
            IDictionary<string, object> boySource = ReadDictionary(source, "boy");
            return new Dictionary<string, object>
            {
                {
                    "zdsr",
                    new Dictionary<string, object>
                    {
                        { "type", ReadTrimmed(zdsrSource, "type", string.Empty) },
                        { "channelName", ReadTrimmed(zdsrSource, "channelName", string.Empty) },
                        { "keyDownInterrupt", ReadTrimmed(zdsrSource, "keyDownInterrupt", string.Empty) },
                        { "alwaysInterrupt", ReadTrimmed(zdsrSource, "alwaysInterrupt", string.Empty) }
                    }
                },
                {
                    "boy",
                    new Dictionary<string, object>
                    {
                        { "Param1", ReadPythonTrue(boySource, "Param1", "false") },
                        { "Param2", ReadPythonTrue(boySource, "Param2", "false") },
                        { "Param3", ReadPythonTrue(boySource, "Param3", "true") },
                        { "Param4", ReadPythonTrue(boySource, "Param4", "true") }
                    }
                }
            };
        }

        public string GetSelectedInterfaceValue()
        {
            string value;
            return _interfaceValues.TryGetValue(InterfaceChoice.SelectedIndex, out value)
                ? value
                : string.Empty;
        }

        public bool ConfirmSettings()
        {
            string interfaceDll = GetSelectedInterfaceValue();
            if (_onSaveInterfaceDll != null && !_onSaveInterfaceDll(interfaceDll))
            {
                return false;
            }

            var assistHints = new Dictionary<string, object>
            {
                { "enabled", AssistEnabledCheckBox.Checked }
            };
            if (_onSaveAssistHints != null && !_onSaveAssistHints(assistHints))
            {
                return false;
            }

            if (_onSaveConfig == null || !_onSaveConfig(Config))
            {
                return false;
            }

            DialogResult = DialogResult.OK;
            Close();
            return true;
        }

        private void InitializeUi(
            IEnumerable<object> interfaceDllChoices,
            string selectedInterfaceDll,
            IDictionary<string, object> assistHints)
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
            InterfaceChoice = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                AccessibleName = "朗读接口",
                Dock = DockStyle.Fill,
                TabIndex = 0
            };
            AddInterfaceChoice("自动", string.Empty);
            foreach (object item in interfaceDllChoices ?? Enumerable.Empty<object>())
            {
                var values = item as IDictionary<string, object>;
                string path = values == null
                    ? Convert.ToString(item)?.Trim() ?? string.Empty
                    : ReadTrimmed(values, "path", string.Empty);
                if (path.Length == 0)
                {
                    continue;
                }

                string label = values == null ? path : ReadTrimmed(values, "label", string.Empty);
                AddInterfaceChoice(label.Length == 0 ? path : label, path);
            }

            SelectInterfaceValue(selectedInterfaceDll);
            AssistEnabledCheckBox = new CheckBox
            {
                Text = "启用读屏辅助提示",
                Checked = CommonUtilities.ParseBoolSetting(
                    GetValue(assistHints, "enabled", false), false),
                AutoSize = true,
                TabIndex = 1,
                Margin = new Padding(0, 0, 0, 10)
            };
            OkButton = new Button { Text = "确定", AutoSize = true, TabIndex = 2 };
            CancelActionButton = new Button
            {
                Text = "取消",
                AutoSize = true,
                TabIndex = 3,
                DialogResult = DialogResult.Cancel
            };
            layout.Controls.Add(CreateInterfaceRow(), 0, 0);
            layout.Controls.Add(AssistEnabledCheckBox, 0, 1);
            layout.Controls.Add(CreateButtonRow(), 0, 2);
            Controls.Add(layout);
            AcceptButton = OkButton;
            CancelButton = CancelActionButton;
            OkButton.Click += (sender, eventArgs) => OperationLog.Run(this, Text + " / " + OkButton.Text, () => ConfirmSettings());
            CancelActionButton.Click += (sender, eventArgs) => OperationLog.Run(this, Text + " / " + CancelActionButton.Text, () => Close());
            Shown += (sender, eventArgs) => InterfaceChoice.Focus();
        }

        private Control CreateInterfaceRow()
        {
            var row = new TableLayoutPanel
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                TabStop = false,
                Margin = new Padding(0, 0, 0, 10)
            };
            row.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            row.Controls.Add(new Label
            {
                Text = "朗读接口:",
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                TabStop = false,
                Margin = new Padding(0, 5, 8, 0)
            }, 0, 0);
            row.Controls.Add(InterfaceChoice, 1, 0);
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
                TabStop = false
            };
            CancelActionButton.Margin = new Padding(0);
            OkButton.Margin = new Padding(0, 0, 6, 0);
            row.Controls.Add(CancelActionButton);
            row.Controls.Add(OkButton);
            return row;
        }

        private void AddInterfaceChoice(string label, string value)
        {
            int index = InterfaceChoice.Items.Add(label);
            _interfaceValues[index] = value;
        }

        private void SelectInterfaceValue(string selectedValue)
        {
            string target = selectedValue ?? string.Empty;
            foreach (KeyValuePair<int, string> item in _interfaceValues)
            {
                if (item.Value == target)
                {
                    InterfaceChoice.SelectedIndex = item.Key;
                    return;
                }
            }

            InterfaceChoice.SelectedIndex = InterfaceChoice.Items.Count == 0 ? -1 : 0;
        }

        private static int ClampInt(object value, int defaultValue, int minimum, int maximum)
        {
            return CommonUtilities.ParseIntSetting(value, defaultValue, minimum, maximum);
        }

        private static IDictionary<string, object> ReadDictionary(
            IDictionary<string, object> values,
            string key)
        {
            return GetValue(values, key, null) as IDictionary<string, object>
                ?? new Dictionary<string, object>();
        }

        private static string ReadTrimmed(
            IDictionary<string, object> values,
            string key,
            string defaultValue)
        {
            return Convert.ToString(GetValue(values, key, defaultValue))?.Trim() ?? string.Empty;
        }

        private static string ReadPythonTrue(
            IDictionary<string, object> values,
            string key,
            string defaultValue)
        {
            return string.Equals(
                ReadTrimmed(values, key, defaultValue),
                "true",
                StringComparison.OrdinalIgnoreCase)
                ? "true"
                : "false";
        }

        private static object GetValue(
            IDictionary<string, object> values,
            string key,
            object defaultValue)
        {
            object value;
            return values != null && values.TryGetValue(key, out value) ? value : defaultValue;
        }
    }
}
