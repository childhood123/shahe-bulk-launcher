using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ShaheLauncher.Core.Common;
using ShaheLauncher.Core.Constants;

namespace ShaheLauncher.App.Input
{
    public sealed class HotkeyActionSpec
    {
        public string Category { get; set; }
        public string Action { get; set; }
        public string Label { get; set; }
        public string DefaultHotkey { get; set; }
        public int EventId { get; set; }
    }

    public sealed class HotkeyBinding
    {
        public string Action { get; set; }
        public int Modifiers { get; set; }
        public int KeyCode { get; set; }
        public int EventId { get; set; }
    }

    public sealed class HotkeyReservedSpec
    {
        public string Action { get; set; }
        public string Label { get; set; }
        public string Hotkey { get; set; }
    }

    public sealed class HotkeySandboxItem
    {
        public string Identity { get; set; }
        public bool IsSpecial { get; set; }
        public bool IsGroup { get; set; }
        public bool IsRunning { get; set; }
    }

    public sealed class HotkeyProcessMenuState
    {
        public string SelectedName { get; set; }
        public bool SelectedHasProcess { get; set; }
        public bool AllHasProcess { get; set; }
        public bool SelectedHasWindow { get; set; }
        public bool AllHasWindow { get; set; }
    }

    public sealed class HotkeyProcessLiveState
    {
        public bool? SelectedMuteState { get; set; }
        public bool? AllMuteState { get; set; }
        public bool SelectedHasWindow { get; set; }
        public bool OutputDevicesAvailable { get; set; }
        public bool SelectedAudioSession { get; set; }
        public bool AllAudioSession { get; set; }
    }

    public sealed class HotkeyController
    {
        // WinForms has no separate NumPadEnter enum; keep Python's logical token distinct.
        public const int NumPadEnterKeyCode = 0x10001;

        private static readonly Regex FunctionKeyPattern =
            new Regex(@"^F([1-9]|1[0-9]|2[0-4])$", RegexOptions.CultureInvariant);

        private static readonly IReadOnlyDictionary<string, int> SpecialKeyCodes =
            new ReadOnlyDictionary<string, int>(new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { "ESC", (int)Keys.Escape },
                { "ESCAPE", (int)Keys.Escape },
                { "ENTER", (int)Keys.Enter },
                { "RETURN", (int)Keys.Enter },
                { "SPACE", (int)Keys.Space },
                { "TAB", (int)Keys.Tab },
                { "BACKSPACE", (int)Keys.Back },
                { "DELETE", (int)Keys.Delete },
                { "DEL", (int)Keys.Delete },
                { "INSERT", (int)Keys.Insert },
                { "INS", (int)Keys.Insert },
                { "HOME", (int)Keys.Home },
                { "END", (int)Keys.End },
                { "PAGEUP", (int)Keys.PageUp },
                { "PGUP", (int)Keys.PageUp },
                { "PAGEDOWN", (int)Keys.PageDown },
                { "PGDN", (int)Keys.PageDown },
                { "LEFT", (int)Keys.Left },
                { "RIGHT", (int)Keys.Right },
                { "UP", (int)Keys.Up },
                { "DOWN", (int)Keys.Down }
            });

        private static readonly IReadOnlyDictionary<string, int> NumpadKeyCodes =
            new ReadOnlyDictionary<string, int>(new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { "NUMPAD0", (int)Keys.NumPad0 },
                { "NUMPAD1", (int)Keys.NumPad1 },
                { "NUMPAD2", (int)Keys.NumPad2 },
                { "NUMPAD3", (int)Keys.NumPad3 },
                { "NUMPAD4", (int)Keys.NumPad4 },
                { "NUMPAD5", (int)Keys.NumPad5 },
                { "NUMPAD6", (int)Keys.NumPad6 },
                { "NUMPAD7", (int)Keys.NumPad7 },
                { "NUMPAD8", (int)Keys.NumPad8 },
                { "NUMPAD9", (int)Keys.NumPad9 },
                { "NUMPADENTER", NumPadEnterKeyCode }
            });

        private readonly ReadOnlyCollection<HotkeyActionSpec> _actionSpecs;
        private HotkeyProcessMenuState _processMenuStateCache;
        private HotkeyProcessLiveState _processMenuLiveStateCache;
        private int _processMenuLiveStateRefreshToken;

        public HotkeyController()
        {
            _actionSpecs = BuildActionSpecs();
            _processMenuLiveStateCache = CreateDefaultProcessMenuLiveState();
        }

        public static IReadOnlyDictionary<string, string> FixedHotkeyTextByAction
        {
            get { return LauncherConstants.FixedHotkeyTextByAction; }
        }

        public static IReadOnlyDictionary<string, string> FixedHotkeyLabelByAction
        {
            get { return LauncherConstants.FixedHotkeyLabelByAction; }
        }

        public IReadOnlyList<HotkeyActionSpec> ActionSpecs
        {
            get { return _actionSpecs; }
        }

        public Tuple<int, int> ParseHotkeyText(string hotkeyText)
        {
            string text = (hotkeyText ?? string.Empty).Trim();
            if (text.Length == 0)
            {
                return null;
            }

            string[] parts = text.Replace('＋', '+')
                .Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Trim())
                .Where(part => part.Length != 0)
                .ToArray();
            if (parts.Length == 0)
            {
                return null;
            }

            int modifiers = 0;
            string keyToken = parts[parts.Length - 1];
            for (int index = 0; index < parts.Length - 1; index++)
            {
                string token = parts[index].ToUpperInvariant();
                if (token == "CTRL" || token == "CONTROL")
                {
                    modifiers |= (int)Keys.Control;
                }
                else if (token == "ALT")
                {
                    modifiers |= (int)Keys.Alt;
                }
                else if (token == "SHIFT")
                {
                    modifiers |= (int)Keys.Shift;
                }
                else
                {
                    return null;
                }
            }

            int? keyCode = ParseHotkeyKeyToken(keyToken);
            return keyCode.HasValue ? Tuple.Create(modifiers, keyCode.Value) : null;
        }

        public string FormatHotkeyText(int modifiers, int keyCode)
        {
            if (keyCode == 0)
            {
                return string.Empty;
            }

            var parts = new List<string>();
            if ((modifiers & (int)Keys.Control) != 0)
            {
                parts.Add("Ctrl");
            }

            if ((modifiers & (int)Keys.Alt) != 0)
            {
                parts.Add("Alt");
            }

            if ((modifiers & (int)Keys.Shift) != 0)
            {
                parts.Add("Shift");
            }

            string keyName = FormatHotkeyKeyCode(keyCode);
            if (keyName.Length == 0)
            {
                return string.Empty;
            }

            parts.Add(keyName);
            return string.Join("+", parts);
        }

        public IDictionary<string, string> NormalizeHotkeyMap(IDictionary<string, string> hotkeys)
        {
            IDictionary<string, string> raw = hotkeys ?? new Dictionary<string, string>();
            var reserved = new HashSet<Tuple<int, int>>();
            foreach (string text in FixedHotkeyTextByAction.Values)
            {
                Tuple<int, int> parsed = ParseHotkeyText(text);
                if (parsed != null)
                {
                    reserved.Add(parsed);
                }
            }

            var normalized = new Dictionary<string, string>();
            foreach (HotkeyActionSpec spec in _actionSpecs)
            {
                string value;
                if (!raw.TryGetValue(spec.Action, out value))
                {
                    value = spec.DefaultHotkey;
                }

                value = (value ?? string.Empty).Trim();
                if (value.Length == 0)
                {
                    normalized[spec.Action] = string.Empty;
                    continue;
                }

                Tuple<int, int> parsed = ParseHotkeyText(value);
                normalized[spec.Action] = parsed == null || reserved.Contains(parsed)
                    ? string.Empty
                    : FormatHotkeyText(parsed.Item1, parsed.Item2);
            }

            return normalized;
        }

        public string GetHotkeyText(string action, IDictionary<string, string> hotkeys, bool hotkeysEnabled)
        {
            string fixedText;
            if (FixedHotkeyTextByAction.TryGetValue(action ?? string.Empty, out fixedText)
                && (fixedText ?? string.Empty).Trim().Length != 0)
            {
                return fixedText;
            }

            if (!hotkeysEnabled)
            {
                return string.Empty;
            }

            string value;
            if (hotkeys == null || !hotkeys.TryGetValue(action ?? string.Empty, out value))
            {
                return string.Empty;
            }

            value = (value ?? string.Empty).Trim();
            if (value.Length == 0)
            {
                return string.Empty;
            }

            Tuple<int, int> parsed = ParseHotkeyText(value);
            return parsed == null ? value : FormatHotkeyText(parsed.Item1, parsed.Item2);
        }

        public string BuildMenuLabel(string label, string action, IDictionary<string, string> hotkeys, bool hotkeysEnabled)
        {
            string text = label ?? string.Empty;
            string hotkey = GetHotkeyText(action, hotkeys, hotkeysEnabled);
            return hotkey.Length == 0 ? text : text + "\t" + hotkey;
        }

        public string BuildMenuLabelWithState(string label, string action, bool enabled, IDictionary<string, string> hotkeys, bool hotkeysEnabled)
        {
            return BuildMenuLabel(label, action, hotkeys, hotkeysEnabled);
        }

        public IReadOnlyList<HotkeyReservedSpec> GetReservedHotkeySpecs()
        {
            var result = new List<HotkeyReservedSpec>();
            foreach (KeyValuePair<string, string> item in FixedHotkeyTextByAction)
            {
                string label;
                FixedHotkeyLabelByAction.TryGetValue(item.Key, out label);
                result.Add(new HotkeyReservedSpec
                {
                    Action = item.Key,
                    Label = string.IsNullOrEmpty(label) ? item.Key : label,
                    Hotkey = item.Value
                });
            }

            return new ReadOnlyCollection<HotkeyReservedSpec>(result);
        }

        public IReadOnlyList<HotkeyBinding> BuildAcceleratorEntries(IDictionary<string, string> hotkeys, bool hotkeysEnabled)
        {
            var result = new List<HotkeyBinding>();
            foreach (KeyValuePair<string, string> item in FixedHotkeyTextByAction)
            {
                Tuple<int, int> parsed = ParseHotkeyText(item.Value);
                if (parsed != null)
                {
                    result.Add(new HotkeyBinding
                    {
                        Action = item.Key,
                        Modifiers = parsed.Item1,
                        KeyCode = parsed.Item2,
                        EventId = 0
                    });
                }
            }

            if (!hotkeysEnabled)
            {
                return new ReadOnlyCollection<HotkeyBinding>(result);
            }

            IDictionary<string, string> normalized = NormalizeHotkeyMap(hotkeys);
            foreach (HotkeyActionSpec spec in _actionSpecs)
            {
                string value;
                if (!normalized.TryGetValue(spec.Action, out value) || value.Length == 0)
                {
                    continue;
                }

                Tuple<int, int> parsed = ParseHotkeyText(value);
                if (parsed != null)
                {
                    result.Add(new HotkeyBinding
                    {
                        Action = spec.Action,
                        Modifiers = parsed.Item1,
                        KeyCode = parsed.Item2,
                        EventId = spec.EventId
                    });
                }
            }

            return new ReadOnlyCollection<HotkeyBinding>(result);
        }

        public string DispatchFixedHotkey(Keys keyData)
        {
            Keys keyCode = keyData & Keys.KeyCode;
            Keys modifiers = keyData & Keys.Modifiers;
            if (keyCode == Keys.F4 && (modifiers & Keys.Alt) != 0 && (modifiers & Keys.Control) == 0)
            {
                return "exit_app";
            }

            if (keyCode == Keys.Escape && modifiers == Keys.None)
            {
                return "minimize_to_tray";
            }

            if ((keyCode == Keys.O || keyCode == Keys.A) && (modifiers & Keys.Control) != 0
                && (modifiers & (Keys.Alt | Keys.Shift)) == Keys.None)
            {
                return keyCode == Keys.O ? "open_app" : "select_all";
            }

            if (keyCode == Keys.V && (modifiers & Keys.Control) != 0
                && (modifiers & (Keys.Alt | Keys.Shift)) == Keys.None)
            {
                return "paste_app";
            }

            if (keyCode == Keys.F5 && modifiers == Keys.None)
            {
                return "refresh";
            }

            return string.Empty;
        }

        public bool SelectAdjacentSandbox(
            IEnumerable items,
            int currentIndex,
            int direction,
            Action<int> selectIndex,
            Action<string> setStatus)
        {
            List<HotkeySandboxItem> values = ToSandboxItems(items);
            var indexes = values.Select((item, index) => new { item, index })
                .Where(value => value.item != null
                    && !value.item.IsSpecial
                    && (value.item.Identity ?? string.Empty).Trim().Length != 0)
                .Select(value => value.index)
                .ToList();
            if (indexes.Count == 0)
            {
                setStatus?.Invoke("当前列表没有可选择的用户");
                return false;
            }

            int target;
            int position = indexes.IndexOf(currentIndex);
            if (position < 0)
            {
                target = direction < 0 ? indexes[indexes.Count - 1] : indexes[0];
            }
            else
            {
                int offset = direction < 0 ? -1 : 1;
                int targetPosition = (position + offset + indexes.Count) % indexes.Count;
                target = indexes[targetPosition];
            }

            selectIndex?.Invoke(target);
            return true;
        }

        public HotkeyProcessMenuState GetProcessMenuStates(
            IEnumerable items,
            int selectedIndex,
            bool listMayHideSandboxMembers,
            Func<bool> hasAnyRunningSandboxProcess)
        {
            List<HotkeySandboxItem> values = ToSandboxItems(items);
            HotkeySandboxItem selected = selectedIndex >= 0 && selectedIndex < values.Count
                ? values[selectedIndex]
                : null;
            string selectedName = selected == null || selected.IsSpecial
                ? string.Empty
                : (selected.Identity ?? string.Empty).Trim();
            bool selectedHasProcess = selected != null && !selected.IsSpecial && selected.IsRunning;
            bool allHasProcess = values.Any(item => item != null && !item.IsSpecial && item.IsRunning);
            if (!allHasProcess && listMayHideSandboxMembers)
            {
                allHasProcess = hasAnyRunningSandboxProcess != null && hasAnyRunningSandboxProcess();
            }

            var result = new HotkeyProcessMenuState
            {
                SelectedName = selectedName,
                SelectedHasProcess = selectedHasProcess,
                AllHasProcess = allHasProcess,
                SelectedHasWindow = false,
                AllHasWindow = allHasProcess
            };
            _processMenuStateCache = result;
            return result;
        }

        public void InvalidateProcessMenuStateCache()
        {
            _processMenuStateCache = null;
        }

        public HotkeyProcessMenuState GetCachedProcessMenuState()
        {
            return _processMenuStateCache;
        }

        public HotkeyProcessLiveState GetProcessMenuLiveState()
        {
            return CopyLiveState(_processMenuLiveStateCache);
        }

        public int InvalidateProcessMenuLiveStateCache(bool clearValues)
        {
            _processMenuLiveStateRefreshToken++;
            if (clearValues)
            {
                _processMenuLiveStateCache = CreateDefaultProcessMenuLiveState();
            }

            return _processMenuLiveStateRefreshToken;
        }

        public bool AcceptProcessMenuLiveState(
            int token,
            HotkeyProcessLiveState state,
            Action refreshMenu)
        {
            if (token != _processMenuLiveStateRefreshToken || state == null)
            {
                return false;
            }

            _processMenuLiveStateCache = CopyLiveState(state);
            try
            {
                refreshMenu?.Invoke();
            }
            catch
            {
            }

            return true;
        }

        public bool ToggleBooleanSetting(
            IDictionary<string, object> settings,
            string settingKey,
            bool currentValue,
            string enabledText,
            string disabledText,
            Action<bool> afterApply,
            Action saveSettings,
            Action createMenuBar,
            Action<string> setStatus)
        {
            bool finalValue = !CommonUtilities.ParseBoolSetting(currentValue, false);
            if (settings != null)
            {
                settings[settingKey ?? string.Empty] = finalValue;
            }

            try
            {
                afterApply?.Invoke(finalValue);
            }
            catch
            {
            }

            if (settings != null)
            {
                settings[settingKey ?? string.Empty] = finalValue;
            }

            saveSettings?.Invoke();
            createMenuBar?.Invoke();
            setStatus?.Invoke(finalValue ? enabledText : disabledText);
            return finalValue;
        }

        private static ReadOnlyCollection<HotkeyActionSpec> BuildActionSpecs()
        {
            var specs = new List<HotkeyActionSpec>
            {
                Spec("软件层面", "toggle_window", "显示/隐藏窗口", "Ctrl+Shift+W", 2999),
                Spec("软件层面", "hotkey_settings", "打开快捷键设置", "Ctrl+Shift+K", 3025),
                Spec("软件层面", "speech_settings", "朗读设置", "Ctrl+Shift+R", 3033),
                Spec("软件层面", "reset_settings", "重置所有设置为默认值", "", 3038),
                Spec("常用操作", "settings", "设置", "Ctrl+Shift+S", 3000),
                Spec("路径记忆", "memory_manager", "打开应用路径菜单", "Ctrl+H", 3003),
                Spec("路径记忆", "memory_edit_note", "路径记忆备注", "", 3050),
                Spec("路径记忆", "memory_adjust_count", "调整记忆数", "", 3051),
                Spec("路径记忆", "memory_toggle_path_display", "隐藏/显示路径", "", 3052),
                Spec("路径记忆", "memory_delete_current", "删除当前记忆", "", 3053),
                Spec("路径记忆", "memory_clear_all", "清空所有路径记忆", "", 3054),
                Spec("路径记忆", "memory_copy_path", "复制完整路径", "", 3055),
                Spec("常用操作", "launch_selected", "启动选中沙盒", "Ctrl+Enter", 3016),
                Spec("常用操作", "select_prev_user", "切换到上一个用户", "Alt+Up", 3036),
                Spec("常用操作", "select_next_user", "切换到下一个用户", "Alt+Down", 3037),
                Spec("常用操作", "create_group", "新建分组", "", 3026),
                Spec("常用操作", "launch_group", "启动分组", "", 3027),
                Spec("常用操作", "delete_group", "删除分组", "", 3028),
                Spec("进程操作", "mute_current", "静音当前打开的进程", "Ctrl+M", 3017),
                Spec("进程操作", "mute_all", "静音全部打开的进程", "Ctrl+Shift+M", 3006),
                Spec("进程操作", "volume_all_up", "提高全部打开的进程音量（+5%）", "Ctrl+Shift+Up", 3007),
                Spec("进程操作", "volume_all_down", "降低全部打开的进程音量（-5%）", "Ctrl+Shift+Down", 3008),
                Spec("进程操作", "volume_current_up", "提高当前打开的进程音量（+5%）", "Ctrl+Up", 3018),
                Spec("进程操作", "volume_current_down", "降低当前打开的进程音量（-5%）", "Ctrl+Down", 3019),
                Spec("进程操作", "switch_output_current", "切换当前打开的进程播放设备", "", 3034),
                Spec("进程操作", "switch_output_all", "切换全部打开的进程播放设备", "", 3035),
                Spec("进程操作", "close_current_process", "关闭当前打开的进程", "", 3020),
                Spec("进程操作", "close_all_processes", "关闭全部打开的进程", "", 3021),
                Spec("进程操作", "locate_current_process", "定位当前打开的进程", "Ctrl+L", 3022),
                Spec("进程操作", "refresh_process_state", "刷新进程状态", "Ctrl+F5", 3057),
                Spec("沙盒管理", "batch_create_sandboxes", "批量创建沙盒用户", "", 3009),
                Spec("沙盒管理", "batch_rename_sandboxes", "批量重命名沙盒用户", "", 3010),
                Spec("沙盒管理", "batch_delete_sandboxes", "批量删除沙盒用户", "", 3011),
                Spec("沙盒管理", "pin_current_user", "置顶/取消置顶当前用户", "", 3012),
                Spec("定时任务", "schedule_launch", "定时启动选中沙盒", "", 3013),
                Spec("定时任务", "schedule_close", "定时关闭选中沙盒", "", 3014),
                Spec("定时任务", "cancel_scheduled_tasks", "取消全部定时任务", "", 3015),
                Spec("工具操作", "backup_sandboxes", "备份沙盒用户", "", 3029),
                Spec("工具操作", "restore_sandboxes", "还原沙盒用户", "", 3030),
                Spec("目录校正", "manual_correct_sandboxie_path", "手动校正 Sandboxie 安装路径", "", 3031),
                Spec("目录校正", "select_sandbox_root", "选择沙盒用户数据目录", "", 3032),
                Spec("设置开关", "toggle_auto_run", "切换：开机自动启动本程序", "", 3040),
                Spec("设置开关", "toggle_show_tray", "切换：在托盘上显示", "", 3041),
                Spec("设置开关", "toggle_auto_select_range", "切换：首尾两点自动勾选中间用户", "", 3042),
                Spec("设置开关", "toggle_skip_running_sandbox_on_launch", "切换：启动跳过已运行的沙盒", "", 3045),
                Spec("设置开关", "toggle_remember_volume", "切换：记忆进程音量", "", 3046),
                Spec("设置开关", "toggle_list_left_right_nav", "切换：用户列表左右光标选择", "", 3047)
            };
            return new ReadOnlyCollection<HotkeyActionSpec>(specs);
        }

        private static HotkeyActionSpec Spec(string category, string action, string label, string defaultHotkey, int eventId)
        {
            return new HotkeyActionSpec
            {
                Category = category,
                Action = action,
                Label = label,
                DefaultHotkey = defaultHotkey,
                EventId = eventId
            };
        }

        private static int? ParseHotkeyKeyToken(string token)
        {
            string key = (token ?? string.Empty).Trim();
            if (key.Length == 0)
            {
                return null;
            }

            int special;
            if (SpecialKeyCodes.TryGetValue(key, out special))
            {
                return special;
            }

            int numpad;
            if (NumpadKeyCodes.TryGetValue(key, out numpad))
            {
                return numpad;
            }

            Match functionMatch = FunctionKeyPattern.Match(key.ToUpperInvariant());
            if (functionMatch.Success)
            {
                int functionIndex;
                if (int.TryParse(functionMatch.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out functionIndex))
                {
                    return (int)Keys.F1 + functionIndex - 1;
                }
            }

            if (key.Length == 1)
            {
                char value = key[0];
                if (value > 127 || char.IsControl(value) || !char.IsWhiteSpace(value) && !char.IsLetterOrDigit(value) && !char.IsPunctuation(value) && !char.IsSymbol(value))
                {
                    return null;
                }

                return char.ToUpperInvariant(value);
            }

            return null;
        }

        private static string FormatHotkeyKeyCode(int keyCode)
        {
            if (keyCode == NumPadEnterKeyCode)
            {
                return "NumPadEnter";
            }

            var special = new Dictionary<int, string>
            {
                { (int)Keys.Escape, "Esc" },
                { (int)Keys.Enter, "Enter" },
                { (int)Keys.Space, "Space" },
                { (int)Keys.Tab, "Tab" },
                { (int)Keys.Back, "Backspace" },
                { (int)Keys.Delete, "Delete" },
                { (int)Keys.Insert, "Insert" },
                { (int)Keys.Home, "Home" },
                { (int)Keys.End, "End" },
                { (int)Keys.PageUp, "PageUp" },
                { (int)Keys.PageDown, "PageDown" },
                { (int)Keys.Left, "Left" },
                { (int)Keys.Right, "Right" },
                { (int)Keys.Up, "Up" },
                { (int)Keys.Down, "Down" },
                { (int)Keys.NumPad0, "NumPad0" },
                { (int)Keys.NumPad1, "NumPad1" },
                { (int)Keys.NumPad2, "NumPad2" },
                { (int)Keys.NumPad3, "NumPad3" },
                { (int)Keys.NumPad4, "NumPad4" },
                { (int)Keys.NumPad5, "NumPad5" },
                { (int)Keys.NumPad6, "NumPad6" },
                { (int)Keys.NumPad7, "NumPad7" },
                { (int)Keys.NumPad8, "NumPad8" },
                { (int)Keys.NumPad9, "NumPad9" }
            };
            string specialName;
            if (special.TryGetValue(keyCode, out specialName))
            {
                return specialName;
            }

            int firstFunction = (int)Keys.F1;
            int lastFunction = (int)Keys.F24;
            if (keyCode >= firstFunction && keyCode <= lastFunction)
            {
                return "F" + (keyCode - firstFunction + 1).ToString(CultureInfo.InvariantCulture);
            }

            if ((keyCode >= 48 && keyCode <= 57) || (keyCode >= 65 && keyCode <= 90))
            {
                return ((char)keyCode).ToString();
            }

            return keyCode == 0 ? string.Empty : keyCode.ToString(CultureInfo.InvariantCulture);
        }

        private static List<HotkeySandboxItem> ToSandboxItems(IEnumerable items)
        {
            var result = new List<HotkeySandboxItem>();
            if (items == null)
            {
                return result;
            }

            foreach (object item in items)
            {
                result.Add(item as HotkeySandboxItem);
            }

            return result;
        }

        private static HotkeyProcessLiveState CreateDefaultProcessMenuLiveState()
        {
            return new HotkeyProcessLiveState
            {
                SelectedMuteState = null,
                AllMuteState = null,
                SelectedHasWindow = false,
                OutputDevicesAvailable = false,
                SelectedAudioSession = false,
                AllAudioSession = false
            };
        }

        private static HotkeyProcessLiveState CopyLiveState(HotkeyProcessLiveState state)
        {
            if (state == null)
            {
                return CreateDefaultProcessMenuLiveState();
            }

            return new HotkeyProcessLiveState
            {
                SelectedMuteState = state.SelectedMuteState,
                AllMuteState = state.AllMuteState,
                SelectedHasWindow = state.SelectedHasWindow,
                OutputDevicesAvailable = state.OutputDevicesAvailable,
                SelectedAudioSession = state.SelectedAudioSession,
                AllAudioSession = state.AllAudioSession
            };
        }
    }
}
