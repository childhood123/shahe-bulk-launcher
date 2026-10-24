using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ShaheLauncher.Tests
{
    [TestClass]
    [TestCategory("SettingsDialogs")]
    public sealed class SettingsDialogsCSharpContractTests
    {
        private const string Namespace = "ShaheLauncher.App.UI.Dialogs.Settings.";

        [TestMethod]
        public void HotkeyDialogPreservesLayoutFocusOrderAndEditorBehavior()
        {
            RunInSta(() =>
            {
                Type type = RequireType("HotkeySettingsDialog");
                var dialog = CreateHotkeyDialog(type, true, null);
                Assert.AreEqual("快捷键设置", dialog.Text);
                Assert.AreEqual(new Size(760, 560), dialog.Size);
                var list = GetProperty<ListBox>(type, dialog, "ActionList");
                var editor = GetProperty<TextBox>(type, dialog, "HotkeyTextBox");
                var enabled = GetProperty<CheckBox>(type, dialog, "HotkeysEnabledCheckBox");
                Assert.AreEqual("快捷键功能列表", list.AccessibleName);
                Assert.AreEqual("当前功能快捷键", editor.AccessibleName);
                CollectionAssert.AreEqual(new[] { "启动", "设置" }, list.Items.Cast<string>().ToArray());
                Assert.AreEqual(0, list.SelectedIndex);
                Assert.AreEqual("launch", GetProperty(type, dialog, "CurrentAction"));
                Assert.AreEqual("Ctrl+Enter", editor.Text);
                AssertTabOrder(list, editor, enabled,
                    GetProperty<Button>(type, dialog, "ResetButton"),
                    GetProperty<Button>(type, dialog, "OkButton"),
                    GetProperty<Button>(type, dialog, "CancelActionButton"));

                ReflectionTestHelpers.InvokeInstance(dialog, "CaptureHotkey", Keys.L, Keys.Control);
                Assert.AreEqual("Ctrl+L", editor.Text);
                var working = GetProperty<IDictionary<string, string>>(type, dialog, "WorkingHotkeys");
                Assert.AreEqual("Ctrl+L", working["launch"]);
                ReflectionTestHelpers.InvokeInstance(dialog, "SelectAction", 1);
                Assert.AreEqual("settings", GetProperty(type, dialog, "CurrentAction"));
                ReflectionTestHelpers.InvokeInstance(dialog, "CaptureHotkey", Keys.Delete, Keys.None);
                Assert.AreEqual(string.Empty, working["settings"]);
                Assert.AreEqual("重置所有快捷键设置", GetProperty<Button>(type, dialog, "ResetButton").Text);
                Assert.AreEqual("确定", GetProperty<Button>(type, dialog, "OkButton").Text);
                Assert.AreEqual("取消", GetProperty<Button>(type, dialog, "CancelActionButton").Text);
                dialog.Dispose();
            });
        }

        [TestMethod]
        public void HotkeyDialogResetsDefaultsAndDisablesHotkeysWithExactPrompt()
        {
            RunInSta(() =>
            {
                Type messagesType = RequireCommonType("DialogMessageService");
                Type type = RequireType("HotkeySettingsDialog");
                var prompts = new List<string>();
                SetStaticProperty(messagesType, "ConfirmHandler",
                    new Func<string, string, MessageBoxIcon, bool>((text, title, icon) =>
                    {
                        prompts.Add(title + "|" + text + "|" + (int)icon);
                        return true;
                    }));
                try
                {
                    var dialog = CreateHotkeyDialog(type, true, null);
                    ReflectionTestHelpers.InvokeInstance(dialog, "ResetHotkeys");
                    Assert.AreEqual("重置快捷键|确定要将全部快捷键恢复为默认值吗？|32", prompts.Single());
                    var working = GetProperty<IDictionary<string, string>>(type, dialog, "WorkingHotkeys");
                    Assert.AreEqual("Ctrl+Enter", working["launch"]);
                    Assert.AreEqual("Ctrl+S", working["settings"]);
                    Assert.IsFalse(GetProperty<CheckBox>(type, dialog, "HotkeysEnabledCheckBox").Checked);
                    dialog.Dispose();
                }
                finally
                {
                    SetStaticProperty(messagesType, "ConfirmHandler", null);
                }
            });
        }

        [TestMethod]
        public void HotkeyDialogUsesExactValidationMessagesAndNormalizedResults()
        {
            RunInSta(() =>
            {
                Type messagesType = RequireCommonType("DialogMessageService");
                Type type = RequireType("HotkeySettingsDialog");
                var messages = new List<string>();
                SetStaticProperty(messagesType, "Handler",
                    new Action<string, string, MessageBoxIcon>((text, title, icon) =>
                        messages.Add(title + "|" + text + "|" + (int)icon)));
                var reserved = new List<IDictionary<string, object>>
                {
                    Map("hotkey", "Ctrl+V", "label", "粘贴 .exe 或文件路径")
                };
                try
                {
                    var dialog = CreateHotkeyDialog(type, true, reserved);
                    var working = GetProperty<IDictionary<string, string>>(type, dialog, "WorkingHotkeys");
                    var editor = GetProperty<TextBox>(type, dialog, "HotkeyTextBox");
                    editor.Text = "bad";
                    Assert.AreEqual(false, ReflectionTestHelpers.InvokeInstance(dialog, "ConfirmSettings"));
                    Assert.AreEqual(
                        "快捷键校验失败|以下快捷键格式无效，请修改后重试：\n启动: bad|48",
                        messages.Last());

                    editor.Text = "Ctrl+Enter";
                    ReflectionTestHelpers.InvokeInstance(dialog, "SelectAction", 1);
                    editor.Text = "Ctrl+V";
                    Assert.AreEqual(false, ReflectionTestHelpers.InvokeInstance(dialog, "ConfirmSettings"));
                    Assert.AreEqual(
                        "快捷键冲突|以下快捷键已被固定基础快捷键占用，请修改后重试：\n设置: Ctrl+V（已用于粘贴 .exe 或文件路径）|48",
                        messages.Last());

                    editor.Text = "Ctrl+S";
                    ReflectionTestHelpers.InvokeInstance(dialog, "SelectAction", 0);
                    editor.Text = "Ctrl+S";
                    Assert.AreEqual(false, ReflectionTestHelpers.InvokeInstance(dialog, "ConfirmSettings"));
                    Assert.AreEqual(
                        "快捷键冲突|以下功能使用了重复快捷键，请修改后重试：\n启动、设置|48",
                        messages.Last());

                    editor.Text = " ctrl+l ";
                    ReflectionTestHelpers.InvokeInstance(dialog, "SelectAction", 1);
                    editor.Text = string.Empty;
                    GetProperty<CheckBox>(type, dialog, "HotkeysEnabledCheckBox").Checked = false;
                    Assert.AreEqual(true, ReflectionTestHelpers.InvokeInstance(dialog, "ConfirmSettings"));
                    var result = GetProperty<IDictionary<string, string>>(type, dialog, "ResultHotkeys");
                    Assert.AreEqual("Ctrl+L", result["launch"]);
                    Assert.AreEqual(string.Empty, result["settings"]);
                    Assert.IsFalse((bool)GetProperty(type, dialog, "ResultHotkeysEnabled"));
                    Assert.AreEqual(DialogResult.OK, dialog.DialogResult);
                    dialog.Dispose();
                }
                finally
                {
                    SetStaticProperty(messagesType, "Handler", null);
                }
            });
        }

        [TestMethod]
        public void RemovesAutomaticLoadAdjustmentFromSettings()
        {
            Type type = RequireType("SettingsDialog");
            Assert.IsNull(type.GetProperty("DynamicDelayCheckBox"));
        }

        [TestMethod]
        public void BuildsConcurrencyDropdownFromLogicalProcessorCount()
        {
            RunInSta(() =>
            {
                Type type = RequireType("SettingsDialog");
                var dialog = (Form)Activator.CreateInstance(
                    type,
                    null,
                    Map(),
                    new Action<IDictionary<string, object>>(value => { }),
                    Map());
                var concurrency = GetProperty<ComboBox>(type, dialog, "LaunchConcurrencyInput");
                int expectedMaximum = Math.Max(1, Math.Min(32, Environment.ProcessorCount));
                int expectedDefault = Math.Min(expectedMaximum, Environment.ProcessorCount < 4 ? 2 : 4);
                Assert.AreEqual(ComboBoxStyle.DropDownList, concurrency.DropDownStyle);
                CollectionAssert.AreEqual(
                    Enumerable.Range(1, expectedMaximum).Cast<object>().ToArray(),
                    concurrency.Items.Cast<object>().ToArray());
                Assert.AreEqual(expectedDefault, Convert.ToInt32(concurrency.SelectedItem));
                dialog.Dispose();
            });
        }

        [TestMethod]
        public void SettingsDialogPreservesControlsRangesTextAndSaveKeys()
        {
            RunInSta(() =>
            {
                Type type = RequireType("SettingsDialog");
                IDictionary<string, object> saved = null;
                var settings = Map(
                    "launch_interval", "99",
                    "launch_concurrency_limit", "0",
                    "auto_run", false,
                    "show_tray", "关闭",
                    "auto_select_range_on_check", true,
                    "skip_running_sandbox_on_launch", false,
                    "remember_volume", true,
                    "list_use_left_right_nav", true,
                    "log_output_mode", "关闭日志输出",
                    "auto_update_on_startup", true,
                    "auto_check_update_on_startup", true);
                var defaults = Map("launch_interval", 5, "launch_concurrency_limit", 1);
                var callback = new Action<IDictionary<string, object>>(value => saved = value);
                var dialog = (Form)Activator.CreateInstance(type, null, settings, callback, defaults);
                Assert.AreEqual("设置", dialog.Text);
                Assert.AreEqual(new Size(560, 640), dialog.Size);
                var interval = GetProperty<NumericUpDown>(type, dialog, "LaunchIntervalInput");
                var concurrency = GetProperty<ComboBox>(type, dialog, "LaunchConcurrencyInput");
                var logOutput = GetProperty<ComboBox>(type, dialog, "LogOutputChoice");
                int expectedMaximum = Math.Max(1, Math.Min(32, Environment.ProcessorCount));
                Assert.AreEqual("打开沙盒间隔时间", interval.AccessibleName);
                Assert.AreEqual(0m, interval.Minimum);
                Assert.AreEqual(15m, interval.Maximum);
                Assert.AreEqual(15m, interval.Value);
                Assert.AreEqual("启动并发数量限制", concurrency.AccessibleName);
                Assert.AreEqual(ComboBoxStyle.DropDownList, concurrency.DropDownStyle);
                Assert.AreEqual(expectedMaximum, concurrency.Items.Count);
                Assert.AreEqual(1, Convert.ToInt32(concurrency.SelectedItem));
                Assert.AreEqual("日志输出", logOutput.AccessibleName);
                CollectionAssert.AreEqual(new[] { "关闭日志输出", "详细日志输出" }, logOutput.Items.Cast<string>().ToArray());
                Assert.AreEqual(0, logOutput.SelectedIndex);
                AssertControlText(type, dialog, "AutoRunCheckBox", "开机自动启动本程序");
                AssertControlText(type, dialog, "ShowTrayCheckBox", "在托盘上显示");
                AssertControlText(type, dialog, "AutoSelectRangeCheckBox", "首尾两点自动勾选中间用户");
                AssertControlText(type, dialog, "SkipRunningCheckBox", "启动跳过已运行的沙盒");
                AssertControlText(type, dialog, "RememberVolumeCheckBox", "记忆进程音量（下次启动自动恢复）");
                AssertControlText(type, dialog, "ListNavigationCheckBox", "用户列表启用左右光标选择");
                AssertTabOrder(
                    interval,
                    concurrency,
                    GetProperty<CheckBox>(type, dialog, "AutoRunCheckBox"),
                    GetProperty<CheckBox>(type, dialog, "ShowTrayCheckBox"),
                    GetProperty<CheckBox>(type, dialog, "AutoSelectRangeCheckBox"),
                    GetProperty<CheckBox>(type, dialog, "SkipRunningCheckBox"),
                    GetProperty<CheckBox>(type, dialog, "RememberVolumeCheckBox"),
                    GetProperty<CheckBox>(type, dialog, "ListNavigationCheckBox"),
                    logOutput,
                    GetProperty<Button>(type, dialog, "ResetButton"),
                    GetProperty<Button>(type, dialog, "OkButton"),
                    GetProperty<Button>(type, dialog, "CancelActionButton"));

                interval.Value = 3;
                int selectedConcurrency = Math.Min(4, expectedMaximum);
                concurrency.SelectedItem = selectedConcurrency;
                GetProperty<CheckBox>(type, dialog, "AutoRunCheckBox").Checked = true;
                logOutput.SelectedIndex = 1;
                Assert.AreEqual(true, ReflectionTestHelpers.InvokeInstance(dialog, "SaveSettings"));
                Assert.AreSame(settings, saved);
                Assert.AreEqual(3, saved["launch_interval"]);
                Assert.AreEqual(selectedConcurrency, saved["launch_concurrency_limit"]);
                Assert.AreEqual(true, saved["auto_run"]);
                Assert.AreEqual(false, saved["auto_minimize_to_tray_after_launch"]);
                Assert.AreEqual("detailed", saved["log_output_mode"]);
                Assert.IsFalse(saved.ContainsKey("auto_update_on_startup"));
                Assert.IsFalse(saved.ContainsKey("auto_check_update_on_startup"));
                Assert.IsFalse(saved.ContainsKey("dynamic_launch_delay_by_load"));
                Assert.AreEqual(DialogResult.OK, dialog.DialogResult);
                dialog.Dispose();
            });
        }

        [TestMethod]
        public void SettingsDialogResetUsesExactPromptAndPythonDefaults()
        {
            RunInSta(() =>
            {
                Type messagesType = RequireCommonType("DialogMessageService");
                Type type = RequireType("SettingsDialog");
                var prompts = new List<string>();
                SetStaticProperty(messagesType, "ConfirmHandler",
                    new Func<string, string, MessageBoxIcon, bool>((text, title, icon) =>
                    {
                        prompts.Add(title + "|" + text + "|" + (int)icon);
                        return true;
                    }));
                try
                {
                    var settings = Map("launch_interval", 1, "launch_concurrency_limit", 2);
                    var defaults = Map(
                        "launch_interval", 7,
                        "launch_concurrency_limit", 8,
                        "show_tray", false,
                        "log_output_mode", "off");
                    var callback = new Action<IDictionary<string, object>>(value => { });
                    var dialog = (Form)Activator.CreateInstance(type, null, settings, callback, defaults);
                    Assert.AreEqual(true, ReflectionTestHelpers.InvokeInstance(dialog, "ResetDefaults"));
                    Assert.AreEqual("重置设置|确定要将设置窗口中的选项恢复为默认值吗？|32", prompts.Single());
                    Assert.AreEqual(7m, GetProperty<NumericUpDown>(type, dialog, "LaunchIntervalInput").Value);
                    int expectedConcurrency = Math.Min(8, Math.Max(1, Math.Min(32, Environment.ProcessorCount)));
                    Assert.AreEqual(
                        expectedConcurrency,
                        Convert.ToInt32(GetProperty<ComboBox>(type, dialog, "LaunchConcurrencyInput").SelectedItem));
                    Assert.IsFalse(GetProperty<CheckBox>(type, dialog, "ShowTrayCheckBox").Checked);
                    Assert.AreEqual(0, GetProperty<ComboBox>(type, dialog, "LogOutputChoice").SelectedIndex);
                    dialog.Dispose();
                }
                finally
                {
                    SetStaticProperty(messagesType, "ConfirmHandler", null);
                }
            });
        }

        [TestMethod]
        public void PathMemoryDialogOnlyShowsQuantityAndConfirmation()
        {
            RunInSta(() =>
            {
                Type type = RequireType("PathMemoryManagerDialog");
                var history = CreateHistory();
                var settings = Map("max_history_items", "999", "hide_path", true);
                var callback = new Action<IList<IDictionary<string, object>>, IDictionary<string, object>>((items, values) => { });
                using (var dialog = (Form)Activator.CreateInstance(type, null, history, settings, callback))
                {
                    dialog.Show();
                    Control[] controls = DescendantControls(dialog).ToArray();
                    Assert.IsFalse(controls.Any(control => control is ListBox || control is ListView),
                        "Quantity settings must not display remembered-path records.");
                    Assert.IsFalse(controls.Any(control => control is CheckBox || control.Text.Contains("已记忆的路径列表") || control.Text == "隐藏路径"));
                    Assert.AreEqual("路径记忆管理", dialog.Text);
                    var count = GetProperty<NumericUpDown>(type, dialog, "HistoryCountInput");
                    var ok = GetProperty<Button>(type, dialog, "OkButton");
                    var cancel = GetProperty<Button>(type, dialog, "CancelActionButton");
                    Assert.AreEqual("路径记忆数量", count.AccessibleName);
                    Assert.AreEqual(0m, count.Minimum);
                    Assert.AreEqual(999m, count.Maximum);
                    Assert.AreEqual(999m, count.Value);
                    Assert.AreEqual("确定", ok.Text);
                    Assert.AreEqual("取消", cancel.Text);
                    AssertTabOrder(count, ok, cancel);
                    Assert.AreSame(ok, dialog.AcceptButton);
                    Assert.AreSame(cancel, dialog.CancelButton);
                    Assert.IsTrue(dialog.Height < 250, "Removed records must not leave a large empty panel.");
                    Assert.AreEqual(2, history.Count);
                    dialog.Close();
                }
            });
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void PathMemoryQuantitySaveAndCancelPreserveStoredPaths(bool save)
        {
            RunInSta(() =>
            {
                Type type = RequireType("PathMemoryManagerDialog");
                var history = CreateHistory();
                var settings = Map("max_history_items", 10, "hide_path", true, "unrelated", "keep");
                IList<IDictionary<string, object>> savedHistory = null;
                IDictionary<string, object> savedSettings = null;
                int saves = 0;
                var callback = new Action<IList<IDictionary<string, object>>, IDictionary<string, object>>(
                    (items, values) => { savedHistory = items; savedSettings = values; saves++; });
                using (var dialog = (Form)Activator.CreateInstance(type, null, history, settings, callback))
                {
                    dialog.Show();
                    GetProperty<NumericUpDown>(type, dialog, "HistoryCountInput").Value = 999;
                    GetProperty<Button>(type, dialog, save ? "OkButton" : "CancelActionButton").PerformClick();
                    Assert.AreEqual(save ? 1 : 0, saves);
                    Assert.AreEqual(save ? 999 : 10, settings["max_history_items"]);
                    Assert.AreEqual(true, settings["hide_path"], "Removing a control must not rewrite unrelated saved preferences.");
                    Assert.AreEqual("keep", settings["unrelated"]);
                    Assert.AreEqual(2, history.Count);
                    Assert.AreEqual("C:\\A.exe", history[0]["path"]);
                    Assert.AreEqual("应用A", history[0]["note"]);
                    Assert.AreEqual("D:\\B.exe", history[1]["path"]);
                    Assert.AreEqual("应用B", history[1]["note"]);
                    if (save)
                    {
                        Assert.AreSame(history, savedHistory);
                        Assert.AreSame(settings, savedSettings);
                    }
                    Assert.AreEqual(save ? DialogResult.OK : DialogResult.Cancel, dialog.DialogResult);
                }
            });
        }

        [TestMethod]
        public void SpeechDialogSanitizesConfigExactlyLikePython()
        {
            Type type = RequireType("SpeechSettingsDialog");
            var infos = Map(
                "VTTS", Map("Speed", 10),
                "  VoiceA  ", Map("Speed", 999, "Volume", -2, "Pitch", "bad", "Name", "  小明  "),
                "", Map("Speed", 20));
            var config = Map("TTSInfos", infos, "DefaultEngine", "missing", "Other", 7);
            var sanitized = (IDictionary<string, object>)ReflectionTestHelpers.InvokeStatic(type, "SanitizeConfig", config);
            Assert.AreEqual(7, sanitized["Other"]);
            Assert.AreEqual("VoiceA", sanitized["DefaultEngine"]);
            var cleaned = (IDictionary<string, object>)sanitized["TTSInfos"];
            CollectionAssert.AreEqual(new[] { "VoiceA" }, cleaned.Keys.ToArray());
            var voice = (IDictionary<string, object>)cleaned["VoiceA"];
            Assert.AreEqual(200, voice["Speed"]);
            Assert.AreEqual(0, voice["Volume"]);
            Assert.AreEqual(50, voice["Pitch"]);
            Assert.AreEqual("小明", voice["Name"]);
        }

        [TestMethod]
        public void SpeechDialogNormalizesReaderIniValuesExactlyLikePython()
        {
            Type type = RequireType("SpeechSettingsDialog");
            var settings = Map(
                "zdsr", Map(
                    "type", " 0 ",
                    "channelName", " 主通道 ",
                    "keyDownInterrupt", null,
                    "alwaysInterrupt", 1),
                "boy", Map(
                    "Param1", "TRUE",
                    "Param2", "1",
                    "Param3", "false"));
            var normalized = (IDictionary<string, object>)ReflectionTestHelpers.InvokeStatic(
                type, "NormalizeReaderIniSettings", settings);
            var zdsr = (IDictionary<string, object>)normalized["zdsr"];
            Assert.AreEqual("0", zdsr["type"]);
            Assert.AreEqual("主通道", zdsr["channelName"]);
            Assert.AreEqual(string.Empty, zdsr["keyDownInterrupt"]);
            Assert.AreEqual("1", zdsr["alwaysInterrupt"]);
            var boy = (IDictionary<string, object>)normalized["boy"];
            Assert.AreEqual("true", boy["Param1"]);
            Assert.AreEqual("false", boy["Param2"]);
            Assert.AreEqual("false", boy["Param3"]);
            Assert.AreEqual("true", boy["Param4"]);
        }

        [TestMethod]
        public void SpeechDialogPreservesVisibleUiChoiceMappingAndSaveShortCircuitOrder()
        {
            RunInSta(() =>
            {
                Type type = RequireType("SpeechSettingsDialog");
                var choices = new object[]
                {
                    Map("path", "D:\\reader1.dll", "label", "接口一"),
                    "D:\\reader2.dll",
                    Map("path", "", "label", "忽略")
                };
                var calls = new List<string>();
                bool allowInterface = false;
                bool allowAssist = true;
                bool allowConfig = true;
                var saveConfig = new Func<IDictionary<string, object>, bool>(value =>
                {
                    calls.Add("config");
                    return allowConfig;
                });
                var saveInterface = new Func<string, bool>(value =>
                {
                    calls.Add("interface:" + value);
                    return allowInterface;
                });
                var saveAssist = new Func<IDictionary<string, object>, bool>(value =>
                {
                    calls.Add("assist:" + value["enabled"]);
                    return allowAssist;
                });
                var dialog = (Form)Activator.CreateInstance(
                    type,
                    null,
                    Map(),
                    saveConfig,
                    null,
                    null,
                    null,
                    choices,
                    "D:\\reader2.dll",
                    saveInterface,
                    Map("enabled", "开启"),
                    saveAssist);
                Assert.AreEqual("朗读设置", dialog.Text);
                Assert.AreEqual(new Size(700, 700), dialog.Size);
                var choice = GetProperty<ComboBox>(type, dialog, "InterfaceChoice");
                var assist = GetProperty<CheckBox>(type, dialog, "AssistEnabledCheckBox");
                Assert.AreEqual("朗读接口", choice.AccessibleName);
                CollectionAssert.AreEqual(
                    new[] { "自动", "接口一", "D:\\reader2.dll" },
                    choice.Items.Cast<string>().ToArray());
                CollectionAssert.Contains(
                    DescendantControls(dialog).OfType<Label>().Select(item => item.Text).ToArray(),
                    "朗读接口:");
                Assert.AreEqual(2, choice.SelectedIndex);
                Assert.AreEqual("D:\\reader2.dll", ReflectionTestHelpers.InvokeInstance(dialog, "GetSelectedInterfaceValue"));
                Assert.AreEqual("启用读屏辅助提示", assist.Text);
                Assert.IsTrue(assist.Checked);
                AssertTabOrder(choice, assist,
                    GetProperty<Button>(type, dialog, "OkButton"),
                    GetProperty<Button>(type, dialog, "CancelActionButton"));

                Assert.AreEqual(false, ReflectionTestHelpers.InvokeInstance(dialog, "ConfirmSettings"));
                CollectionAssert.AreEqual(new[] { "interface:D:\\reader2.dll" }, calls.ToArray());
                allowInterface = true;
                allowAssist = false;
                calls.Clear();
                Assert.AreEqual(false, ReflectionTestHelpers.InvokeInstance(dialog, "ConfirmSettings"));
                CollectionAssert.AreEqual(
                    new[] { "interface:D:\\reader2.dll", "assist:True" }, calls.ToArray());
                allowAssist = true;
                allowConfig = false;
                calls.Clear();
                Assert.AreEqual(false, ReflectionTestHelpers.InvokeInstance(dialog, "ConfirmSettings"));
                CollectionAssert.AreEqual(
                    new[] { "interface:D:\\reader2.dll", "assist:True", "config" }, calls.ToArray());
                allowConfig = true;
                calls.Clear();
                Assert.AreEqual(true, ReflectionTestHelpers.InvokeInstance(dialog, "ConfirmSettings"));
                Assert.AreEqual(DialogResult.OK, dialog.DialogResult);
                dialog.Dispose();
            });
        }

        private static Form CreateHotkeyDialog(
            Type type,
            bool enabled,
            IEnumerable<IDictionary<string, object>> reserved)
        {
            var actionSpecs = new List<IDictionary<string, object>>
            {
                Map("action", "launch", "label", "启动"),
                Map("action", "settings", "label", "设置")
            };
            var hotkeys = new Dictionary<string, string>
            {
                { "launch", "Ctrl+Enter" },
                { "settings", "Ctrl+S" }
            };
            var defaults = new Dictionary<string, string>(hotkeys);
            var parse = new Func<string, Tuple<int, int>>(ParseHotkey);
            var format = new Func<int, int, string>(FormatHotkey);
            return (Form)Activator.CreateInstance(
                type, null, actionSpecs, hotkeys, defaults, parse, format, enabled, reserved);
        }

        private static Tuple<int, int> ParseHotkey(string value)
        {
            string text = (value ?? string.Empty).Trim();
            if (!text.StartsWith("Ctrl+", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            Keys key;
            return Enum.TryParse(text.Substring(5), true, out key)
                ? Tuple.Create((int)Keys.Control, (int)key)
                : null;
        }

        private static string FormatHotkey(int modifiers, int keyCode)
        {
            if (modifiers != (int)Keys.Control)
            {
                return string.Empty;
            }

            return "Ctrl+" + ((Keys)keyCode);
        }

        private static List<IDictionary<string, object>> CreateHistory()
        {
            return new List<IDictionary<string, object>>
            {
                Map("note", "应用A", "path", "C:\\A.exe"),
                Map("note", "应用B", "path", "D:\\B.exe")
            };
        }

        private static Dictionary<string, object> Map(params object[] values)
        {
            var result = new Dictionary<string, object>();
            for (int index = 0; index + 1 < values.Length; index += 2)
            {
                result[(string)values[index]] = values[index + 1];
            }

            return result;
        }

        private static Type RequireType(string name)
        {
            Type type = Assembly.Load("沙河批量打开器").GetType(Namespace + name, false);
            Assert.IsNotNull(type, Namespace + name + " must exist");
            return type;
        }

        private static Type RequireCommonType(string name)
        {
            Type type = Assembly.Load("沙河批量打开器").GetType(
                "ShaheLauncher.App.UI.Dialogs.Common." + name, false);
            Assert.IsNotNull(type, name + " must exist");
            return type;
        }

        private static T GetProperty<T>(Type type, object target, string name) where T : class
        {
            return (T)GetProperty(type, target, name);
        }

        private static object GetProperty(Type type, object target, string name)
        {
            PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(property, name + " must be a public property");
            return property.GetValue(target);
        }

        private static void SetStaticProperty(Type type, string name, object value)
        {
            PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(property, name + " must be a public static property");
            property.SetValue(null, value);
        }

        private static void AssertControlText(Type type, object target, string propertyName, string expected)
        {
            Assert.AreEqual(expected, ((Control)GetProperty(type, target, propertyName)).Text);
        }

        private static IEnumerable<Control> DescendantControls(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                yield return child;
                foreach (Control nested in DescendantControls(child)) yield return nested;
            }
        }

        private static void AssertTabOrder(params Control[] controls)
        {
            for (int index = 0; index < controls.Length; index++)
            {
                Assert.AreEqual(index, controls[index].TabIndex, controls[index].Name + " TabIndex");
            }
        }

        private static void RunInSta(Action action)
        {
            Exception error = null;
            var thread = new Thread(() =>
            {
                try { action(); }
                catch (Exception caught) { error = caught; }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (error != null)
            {
                throw new AssertFailedException("STA action failed: " + error, error);
            }
        }
    }
}
