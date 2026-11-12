using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Automation;
using System.Windows.Forms;

namespace ShaheLauncher.AuditRunner
{
    internal sealed class WindowSnapshot
    {
        public IntPtr Handle { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public bool Visible { get; set; }
        public bool Enabled { get; set; }
    }

    internal sealed class DialogObservation
    {
        public string Name { get; set; } = string.Empty;
        public int CommandId { get; set; }
        public string Handle { get; set; } = string.Empty;
        public bool Foreground { get; set; }
        public bool Stable { get; set; }
        public bool Closed { get; set; }
        public bool OkEnabled { get; set; }
        public int OkControlId { get; set; }
        public bool Submitted { get; set; }
        public string SubmitError { get; set; } = string.Empty;
        public IList<string> InputTexts { get; set; } = new List<string>();
        public int ComboIndex { get; set; } = -1;
        public int ControlCount { get; set; }
        public int EditCount { get; set; }
        public int ComboBoxCount { get; set; }
        public int ButtonCount { get; set; }
    }

    internal static class Win32Automation
    {
        private const uint MiimId = 0x2;
        private const uint MiimSubmenu = 0x4;
        private const uint MiimString = 0x40;
        private const int WmCommand = 0x111;
        private const int WmClose = 0x10;
        private const int BmClick = 0xF5;
        private const int EmSetSel = 0xB1;
        private const int EmReplaceSel = 0xC2;
        private const uint SendMessageAbortIfHung = 0x2;
        private const int CbGetCurSel = 0x147;
        private const int CbSetCurSel = 0x14E;
        private const int CbnSelChange = 1;
        private const int LvmFirst = 0x1000;
        private const int LvmGetItemCount = LvmFirst + 4;
        private const int LvmGetItemRect = LvmFirst + 14;
        private const int LvmSetItemState = LvmFirst + 43;
        private const int LvmGetItemState = LvmFirst + 44;
        private const int WmLeftButtonDown = 0x201;
        private const int WmLeftButtonUp = 0x202;
        private const int MkLeftButton = 0x1;
        private const int ShowWindowRestore = 9;
        private const uint LvisStateImageMask = 0xF000;

        internal static IntPtr WaitForMainWindow(Process process, int timeoutMilliseconds)
        {
            Stopwatch timer = Stopwatch.StartNew();
            while (timer.ElapsedMilliseconds < timeoutMilliseconds)
            {
                if (process.HasExited)
                {
                    return IntPtr.Zero;
                }
                process.Refresh();
                if (process.MainWindowHandle != IntPtr.Zero && IsWindow(process.MainWindowHandle))
                {
                    return process.MainWindowHandle;
                }
                Thread.Sleep(50);
            }
            return IntPtr.Zero;
        }

        internal static IList<MenuCommandInfo> GetMenuCommands(IntPtr window)
        {
            var commands = new List<MenuCommandInfo>();
            IntPtr menu = GetMenu(window);
            if (menu != IntPtr.Zero)
            {
                ReadMenu(menu, commands);
            }
            return commands;
        }

        internal static IList<string> GetNonEmptyWindowTexts(IntPtr parent)
        {
            return GetDescendants(parent)
                .Select(item => (item.Title ?? string.Empty).Trim())
                .Where(value => value.Length != 0)
                .Distinct(StringComparer.Ordinal)
                .ToList();
        }

        internal static bool PostCommand(IntPtr window, int commandId)
        {
            return PostMessage(window, WmCommand, new IntPtr(commandId), IntPtr.Zero);
        }

        internal static bool SendCommand(IntPtr window, int commandId)
        {
            if (!IsWindow(window))
            {
                return false;
            }
            SendMessage(window, WmCommand, new IntPtr(commandId), IntPtr.Zero);
            return true;
        }

        internal static bool WaitForSandboxItemCount(
            IntPtr mainWindow,
            int minimum,
            int timeoutMilliseconds)
        {
            return WaitUntil(() =>
            {
                WindowSnapshot list = FindLargestList(mainWindow);
                return list != null
                    && SendMessage(list.Handle, LvmGetItemCount, IntPtr.Zero, IntPtr.Zero).ToInt32() >= minimum;
            }, timeoutMilliseconds);
        }

        internal static DialogObservation OpenAndCancelDialog(
            int processId,
            IntPtr mainWindow,
            int commandId,
            string expectedTitle,
            int timeoutMilliseconds)
        {
            var result = new DialogObservation { Name = expectedTitle, CommandId = commandId };
            if (!PostCommand(mainWindow, commandId))
            {
                return result;
            }

            IntPtr dialog = WaitForWindow(processId, mainWindow, expectedTitle, timeoutMilliseconds);
            if (dialog == IntPtr.Zero)
            {
                return result;
            }
            result.Handle = Hex(dialog);
            result.Foreground = GetForegroundWindow() == dialog;

            int stableSamples = 0;
            string previous = string.Empty;
            Stopwatch timer = Stopwatch.StartNew();
            while (timer.ElapsedMilliseconds < timeoutMilliseconds)
            {
                IList<WindowSnapshot> controls = GetDescendants(dialog);
                int edits = CountClass(controls, "EDIT");
                int combos = CountClass(controls, "COMBOBOX");
                int buttons = CountClass(controls, "BUTTON");
                string signature = controls.Count + ":" + edits + ":" + combos + ":" + buttons;
                stableSamples = signature == previous ? stableSamples + 1 : 0;
                previous = signature;
                result.ControlCount = controls.Count;
                result.EditCount = edits;
                result.ComboBoxCount = combos;
                result.ButtonCount = buttons;
                if (stableSamples >= 2 && buttons > 0)
                {
                    result.Stable = true;
                    break;
                }
                Thread.Sleep(50);
            }

            IList<WindowSnapshot> finalControls = GetDescendants(dialog);
            IntPtr noButton = GetDlgItem(dialog, 7);
            WindowSnapshot cancel = finalControls.FirstOrDefault(control =>
                control.ClassName.IndexOf("BUTTON", StringComparison.OrdinalIgnoreCase) >= 0
                && (control.Title.IndexOf("取消", StringComparison.OrdinalIgnoreCase) >= 0
                    || control.Title.IndexOf("否", StringComparison.OrdinalIgnoreCase) >= 0
                    || control.Title.IndexOf("Cancel", StringComparison.OrdinalIgnoreCase) >= 0
                    || string.Equals(control.Title, "No", StringComparison.OrdinalIgnoreCase)));
            if (noButton != IntPtr.Zero)
            {
                SendMessage(noButton, BmClick, IntPtr.Zero, IntPtr.Zero);
            }
            else if (cancel != null)
            {
                SendMessage(cancel.Handle, BmClick, IntPtr.Zero, IntPtr.Zero);
            }
            else
            {
                PostMessage(dialog, WmClose, IntPtr.Zero, IntPtr.Zero);
            }
            result.Closed = WaitUntil(() => !IsWindow(dialog), timeoutMilliseconds);
            return result;
        }

        internal static DialogObservation OpenAndSubmitBatchCreateDialog(
            int processId,
            IntPtr mainWindow,
            int commandId,
            string prefix)
        {
            var result = new DialogObservation { Name = "批量创建沙盒用户", CommandId = commandId };
            if (!PostCommand(mainWindow, commandId))
            {
                return result;
            }

            IntPtr dialog = WaitForWindow(processId, mainWindow, result.Name, 10000);
            if (dialog == IntPtr.Zero)
            {
                return result;
            }
            result.Handle = Hex(dialog);
            result.Foreground = EnsureForegroundWindow(dialog, 2000);
            IList<WindowSnapshot> controls = WaitForStableControls(dialog, result, 10000);
            WindowSnapshot ok = controls.FirstOrDefault(control =>
                control.ClassName.IndexOf("BUTTON", StringComparison.OrdinalIgnoreCase) >= 0
                && string.Equals(control.Title, "确定", StringComparison.Ordinal));
            if (!result.Foreground || !result.Stable || ok == null)
            {
                return result;
            }
            string inputFailure;
            if (!TrySetBatchCreateValuesByAccessibleName(dialog, prefix, 1, 0, 1, out inputFailure))
            {
                result.SubmitError = inputFailure;
                return result;
            }

            result.InputTexts = new[] { prefix, "1", "1" };
            result.ComboIndex = 0;
            result.OkEnabled = ok.Enabled;
            result.OkControlId = GetDlgCtrlID(ok.Handle);
            result.Submitted = PostButtonClickNotification(dialog, ok.Handle);
            result.SubmitError = result.Submitted
                ? string.Empty
                : "Button was not a valid owned control of the foreground dialog.";
            bool closed = WaitUntil(() => !IsWindow(dialog), 20000);
            result.Closed = result.Submitted && closed;
            return result;
        }

        private static bool SetBatchCreateValuesByAccessibleName(
            IntPtr dialog,
            string prefix,
            int start,
            int digitsIndex,
            int count)
        {
            string ignored;
            return TrySetBatchCreateValuesByAccessibleName(
                dialog,
                prefix,
                start,
                digitsIndex,
                count,
                out ignored);
        }

        private static bool TrySetBatchCreateValuesByAccessibleName(
            IntPtr dialog,
            string prefix,
            int start,
            int digitsIndex,
            int count,
            out string failure)
        {
            failure = string.Empty;
            try
            {
                if (!EnsureForegroundWindow(dialog, 2000))
                {
                    failure = "Batch-create dialog was not foreground.";
                    return false;
                }
                AutomationElement root = AutomationElement.FromHandle(dialog);
                AutomationElement prefixInput = FindUniqueNamedControl(root, "名称前缀", ControlType.Edit);
                AutomationElement digitsInput = FindUniqueNamedControl(root, "编号格式", ControlType.ComboBox);
                if (prefixInput == null || digitsInput == null)
                {
                    failure = "Prefix or digit-format control was missing or duplicated.";
                    return false;
                }

                object valuePattern;
                if (!prefixInput.TryGetCurrentPattern(ValuePattern.Pattern, out valuePattern))
                {
                    failure = "Prefix control did not expose ValuePattern.";
                    return false;
                }

                IntPtr combo = new IntPtr(digitsInput.Current.NativeWindowHandle);
                IntPtr prefixHandle = new IntPtr(prefixInput.Current.NativeWindowHandle);
                if (!SetNumericSpinnerValueByAccessibleName(
                        root,
                        dialog,
                        "起始编号",
                        start,
                        combo))
                {
                    failure = "Start-number control rejected the requested value.";
                    return false;
                }
                if (combo == IntPtr.Zero
                    || SendMessage(combo, CbSetCurSel, new IntPtr(digitsIndex), IntPtr.Zero).ToInt32() != digitsIndex)
                {
                    failure = "Digit-format control rejected the requested selection.";
                    return false;
                }
                // CB_SETCURSEL changes the native value without notifying the managed parent.
                IntPtr comboParent = GetParent(combo);
                IntPtr notificationResult;
                if (comboParent == IntPtr.Zero || SendMessageTimeout(
                    comboParent, WmCommand,
                    new IntPtr((GetDlgCtrlID(combo) & 0xffff) | (CbnSelChange << 16)), combo,
                    SendMessageAbortIfHung, 2000, out notificationResult) == IntPtr.Zero)
                {
                    failure = "Digit-format selection notification failed.";
                    return false;
                }
                if (!SetNumericSpinnerValueByAccessibleName(
                        root,
                        dialog,
                        "创建数量",
                        count,
                        prefixHandle))
                {
                    failure = "Count control rejected the requested value.";
                    return false;
                }
                ((ValuePattern)valuePattern).SetValue(prefix);

                AutomationElement currentDigits = FindUniqueNamedControl(
                    AutomationElement.FromHandle(dialog), "编号格式", ControlType.ComboBox);
                IntPtr currentCombo = currentDigits == null
                    ? IntPtr.Zero : new IntPtr(currentDigits.Current.NativeWindowHandle);
                if (currentCombo != combo || !IsWindow(combo))
                {
                    failure = "Digit-format handle changed: old=" + combo + ", current=" + currentCombo;
                    return false;
                }

                bool verified = WaitUntil(() =>
                {
                    object currentPrefix;
                    return prefixInput.TryGetCurrentPattern(ValuePattern.Pattern, out currentPrefix)
                        && string.Equals(((ValuePattern)currentPrefix).Current.Value, prefix, StringComparison.Ordinal)
                        && SendMessage(combo, CbGetCurSel, IntPtr.Zero, IntPtr.Zero).ToInt32() == digitsIndex;
                }, 2000);
                failure = verified ? string.Empty : "Prefix or digit-format verification failed.";
                return verified;
            }
            catch (Exception exception)
            {
                failure = "Batch-create input probe failed: " + exception.GetType().Name + ": " + exception.Message;
                return false;
            }
        }

        private static bool SetNumericSpinnerValueByAccessibleName(
            AutomationElement root,
            IntPtr dialog,
            string name,
            int value,
            IntPtr commitFocus)
        {
            WindowSnapshot edit = FindNumericSpinnerEditByAccessibleName(root, dialog, name);
            int current;
            int target = Math.Max(0, value);
            if (edit == null || !int.TryParse(WindowText(edit.Handle), out current))
            {
                return false;
            }

            if (current != target
                && !SetNumericEditTextAndCommit(edit.Handle, commitFocus, target))
            {
                return false;
            }
            return int.TryParse(WindowText(edit.Handle), out current) && current == target;
        }

        private static bool SetNumericEditTextAndCommit(IntPtr edit, IntPtr commitFocus, int value)
        {
            if (edit == IntPtr.Zero
                || commitFocus == IntPtr.Zero
                || !IsWindow(edit)
                || !IsWindow(commitFocus))
            {
                return false;
            }

            uint processId;
            uint targetThread = GetWindowThreadProcessId(edit, out processId);
            uint currentThread = GetCurrentThreadId();
            bool attached = currentThread == targetThread
                || AttachThreadInput(currentThread, targetThread, true);
            if (!attached)
            {
                return false;
            }

            try
            {
                SetFocus(edit);
                if (GetFocus() != edit)
                {
                    return false;
                }
                string expected = Math.Max(0, value).ToString();
                SendMessage(edit, EmSetSel, IntPtr.Zero, new IntPtr(-1));
                SendMessage(edit, EmReplaceSel, new IntPtr(1), expected);
                SetFocus(commitFocus);
                if (GetFocus() != commitFocus)
                {
                    return false;
                }
                return WaitUntil(
                    () => string.Equals(WindowText(edit), expected, StringComparison.Ordinal),
                    2000);
            }
            finally
            {
                if (currentThread != targetThread)
                {
                    AttachThreadInput(currentThread, targetThread, false);
                }
            }
        }

        private static IntPtr FindNumericSpinnerButtonsByAccessibleName(
            IntPtr dialog,
            string name)
        {
            AutomationElement root = AutomationElement.FromHandle(dialog);
            WindowSnapshot edit = FindNumericSpinnerEditByAccessibleName(root, dialog, name);
            if (edit == null)
            {
                return IntPtr.Zero;
            }
            IntPtr parent = GetParent(edit.Handle);
            WindowSnapshot buttons = GetDescendants(parent).SingleOrDefault(control =>
                control.Handle != edit.Handle
                && control.ClassName.IndexOf("EDIT", StringComparison.OrdinalIgnoreCase) < 0);
            return buttons == null ? IntPtr.Zero : buttons.Handle;
        }

        private static WindowSnapshot FindNumericSpinnerEditByAccessibleName(
            AutomationElement root,
            IntPtr dialog,
            string name)
        {
            string labelName = string.Equals(name, "起始编号", StringComparison.Ordinal)
                ? "起始编号（第一个编号）:"
                : string.Equals(name, "创建数量", StringComparison.Ordinal)
                    ? "创建数量（要创建几个用户）:"
                    : string.Empty;
            return labelName.Length == 0
                ? null
                : FindUniqueEditForLabel(root, GetDescendants(dialog), labelName);
        }

        private static AutomationElement FindUniqueNamedControl(
            AutomationElement root,
            string name,
            ControlType controlType)
        {
            AutomationElementCollection matches = root.FindAll(
                TreeScope.Descendants,
                new AndCondition(
                    new PropertyCondition(AutomationElement.NameProperty, name),
                    new PropertyCondition(AutomationElement.ControlTypeProperty, controlType)));
            return matches.Count == 1 ? matches[0] : null;
        }

        private static WindowSnapshot FindUniqueEditForLabel(
            AutomationElement root,
            IEnumerable<WindowSnapshot> controls,
            string labelName)
        {
            AutomationElement label = FindUniqueNamedControl(root, labelName, ControlType.Text);
            if (label == null)
            {
                return null;
            }

            var labelBounds = label.Current.BoundingRectangle;
            var matches = new List<WindowSnapshot>();
            foreach (WindowSnapshot control in controls.Where(item =>
                item.ClassName.IndexOf("EDIT", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                Rectangle bounds;
                if (GetWindowRect(control.Handle, out bounds)
                    && bounds.Left >= labelBounds.Right
                    && bounds.Top < labelBounds.Bottom
                    && bounds.Bottom > labelBounds.Top)
                {
                    matches.Add(control);
                }
            }
            return matches.Count == 1 ? matches[0] : null;
        }

        private static bool PostButtonClickNotification(IntPtr dialog, IntPtr button)
        {
            if (dialog == IntPtr.Zero
                || button == IntPtr.Zero
                || !IsWindow(dialog)
                || !IsChild(dialog, button)
                || GetForegroundWindow() != dialog)
            {
                return false;
            }

            IntPtr ignored;
            return SendMessageTimeout(
                button,
                BmClick,
                IntPtr.Zero,
                IntPtr.Zero,
                SendMessageAbortIfHung,
                70000,
                out ignored) != IntPtr.Zero;
        }

        private static bool EnsureForegroundWindow(IntPtr window, int timeoutMilliseconds)
        {
            if (window == IntPtr.Zero || !IsWindow(window) || timeoutMilliseconds <= 0)
            {
                return false;
            }
            if (GetForegroundWindow() == window)
            {
                return true;
            }

            ShowWindow(window, ShowWindowRestore);
            SetForegroundWindow(window);
            return WaitUntil(() => IsWindow(window) && GetForegroundWindow() == window, timeoutMilliseconds);
        }

        internal static bool CheckFirstSandboxItems(IntPtr mainWindow, int count)
        {
            WindowSnapshot list = null;
            Stopwatch timer = Stopwatch.StartNew();
            while (timer.ElapsedMilliseconds < 10000)
            {
                list = FindLargestList(mainWindow);
                if (list != null
                    && SendMessage(list.Handle, LvmGetItemCount, IntPtr.Zero, IntPtr.Zero).ToInt32() >= count)
                {
                    break;
                }
                Thread.Sleep(100);
            }
            if (list == null || SendMessage(list.Handle, LvmGetItemCount, IntPtr.Zero, IntPtr.Zero).ToInt32() < count)
            {
                return false;
            }
            for (int index = 0; index < count; index++)
            {
                if (!SetAutomationItemChecked(list.Handle, index, true))
                {
                    return false;
                }
            }
            return true;
        }

        internal static void ClearFirstSandboxItems(IntPtr mainWindow, int count)
        {
            WindowSnapshot list = FindLargestList(mainWindow);
            if (list == null)
            {
                return;
            }
            for (int index = 0; index < count; index++)
            {
                SetAutomationItemChecked(list.Handle, index, false);
            }
        }

        internal static bool WaitForExit(Process process, int timeoutMilliseconds)
        {
            try { return process.WaitForExit(timeoutMilliseconds); }
            catch { return process.HasExited; }
        }

        internal static bool CloseWindow(IntPtr window)
        {
            return window != IntPtr.Zero && PostMessage(window, WmClose, IntPtr.Zero, IntPtr.Zero);
        }

        private static void ReadMenu(IntPtr menu, IList<MenuCommandInfo> commands)
        {
            int count = GetMenuItemCount(menu);
            for (uint index = 0; index < count; index++)
            {
                IntPtr textBuffer = Marshal.AllocHGlobal(1024);
                try
                {
                    for (int offset = 0; offset < 1024; offset += 2)
                    {
                        Marshal.WriteInt16(textBuffer, offset, 0);
                    }
                    var info = new MenuItemInfo
                    {
                        Size = (uint)Marshal.SizeOf(typeof(MenuItemInfo)),
                        Mask = MiimId | MiimSubmenu | MiimString,
                        TypeData = textBuffer,
                        CharacterCount = 511
                    };
                    if (!GetMenuItemInfo(menu, index, true, ref info))
                    {
                        continue;
                    }
                    string text = Marshal.PtrToStringUni(textBuffer) ?? string.Empty;
                    if (info.SubMenu != IntPtr.Zero)
                    {
                        ReadMenu(info.SubMenu, commands);
                    }
                    else if (info.Id != 0 && text.Length != 0 && text != "-")
                    {
                        commands.Add(new MenuCommandInfo { Id = unchecked((int)info.Id), Title = text });
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(textBuffer);
                }
            }
        }

        private static IntPtr WaitForWindow(int processId, IntPtr mainWindow, string title, int timeoutMilliseconds)
        {
            Stopwatch timer = Stopwatch.StartNew();
            while (timer.ElapsedMilliseconds < timeoutMilliseconds)
            {
                WindowSnapshot match = GetTopWindows().FirstOrDefault(window =>
                    window.Handle != mainWindow
                    && window.Visible
                    && string.Equals(window.Title, title, StringComparison.Ordinal)
                    && WindowProcessId(window.Handle) == processId);
                if (match != null)
                {
                    return match.Handle;
                }
                Thread.Sleep(50);
            }
            return IntPtr.Zero;
        }

        private static IList<WindowSnapshot> GetTopWindows()
        {
            var result = new List<WindowSnapshot>();
            EnumWindows((window, parameter) => { result.Add(Snapshot(window)); return true; }, IntPtr.Zero);
            return result;
        }

        private static IList<WindowSnapshot> GetDescendants(IntPtr parent)
        {
            var result = new List<WindowSnapshot>();
            EnumChildWindows(parent, (window, parameter) => { result.Add(Snapshot(window)); return true; }, IntPtr.Zero);
            return result;
        }

        private static IList<WindowSnapshot> WaitForStableControls(
            IntPtr dialog,
            DialogObservation result,
            int timeoutMilliseconds)
        {
            IList<WindowSnapshot> controls = new List<WindowSnapshot>();
            int stableSamples = 0;
            string previous = string.Empty;
            Stopwatch timer = Stopwatch.StartNew();
            while (timer.ElapsedMilliseconds < timeoutMilliseconds)
            {
                controls = GetDescendants(dialog);
                int edits = CountClass(controls, "EDIT");
                int combos = CountClass(controls, "COMBOBOX");
                int buttons = CountClass(controls, "BUTTON");
                string signature = controls.Count + ":" + edits + ":" + combos + ":" + buttons;
                stableSamples = signature == previous ? stableSamples + 1 : 0;
                previous = signature;
                result.ControlCount = controls.Count;
                result.EditCount = edits;
                result.ComboBoxCount = combos;
                result.ButtonCount = buttons;
                if (stableSamples >= 2 && buttons > 0)
                {
                    result.Stable = true;
                    return controls;
                }
                Thread.Sleep(50);
            }
            return controls;
        }

        private static WindowSnapshot Snapshot(IntPtr window)
        {
            return new WindowSnapshot
            {
                Handle = window,
                Title = WindowText(window),
                ClassName = WindowClass(window),
                Visible = IsWindowVisible(window),
                Enabled = IsWindowEnabled(window)
            };
        }

        private static int CountClass(IEnumerable<WindowSnapshot> controls, string value)
        {
            return controls.Count(item => item.ClassName.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static void SetListItemChecked(IntPtr list, int index, bool value)
        {
            var item = new ListViewItem
            {
                StateMask = LvisStateImageMask,
                State = (uint)((value ? 2 : 1) << 12)
            };
            SendMessage(list, LvmSetItemState, new IntPtr(index), ref item);
        }

        private static bool ClickListItemCheckBox(IntPtr list, int index)
        {
            var rectangle = new Rectangle { Left = 0 };
            if (SendMessage(list, LvmGetItemRect, new IntPtr(index), ref rectangle) == IntPtr.Zero)
            {
                return false;
            }
            int x = rectangle.Left + 8;
            int y = rectangle.Top + Math.Max(1, (rectangle.Bottom - rectangle.Top) / 2);
            IntPtr coordinates = new IntPtr((y << 16) | (x & 0xFFFF));
            SendMessage(list, WmLeftButtonDown, new IntPtr(MkLeftButton), coordinates);
            SendMessage(list, WmLeftButtonUp, IntPtr.Zero, coordinates);
            return true;
        }

        private static bool IsListItemChecked(IntPtr list, int index)
        {
            int state = SendMessage(
                list,
                LvmGetItemState,
                new IntPtr(index),
                new IntPtr(unchecked((int)LvisStateImageMask))).ToInt32();
            return (state & LvisStateImageMask) == (2 << 12);
        }

        private static bool SetAutomationItemChecked(IntPtr list, int index, bool value)
        {
            try
            {
                AutomationElement root = AutomationElement.FromHandle(list);
                AutomationElementCollection items = root.FindAll(
                    TreeScope.Children,
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ListItem));
                if (index < 0 || index >= items.Count)
                {
                    return false;
                }
                AutomationElement item = items[index];
                object pattern;
                if (item.TryGetCurrentPattern(TogglePattern.Pattern, out pattern))
                {
                    var toggle = (TogglePattern)pattern;
                    bool checkedNow = toggle.Current.ToggleState == ToggleState.On;
                    if (checkedNow != value)
                    {
                        toggle.Toggle();
                    }
                    return WaitUntil(() =>
                    {
                        object currentPattern;
                        return item.TryGetCurrentPattern(TogglePattern.Pattern, out currentPattern)
                            && (((TogglePattern)currentPattern).Current.ToggleState == ToggleState.On) == value;
                    }, 2000);
                }

                item.SetFocus();
                SendKeys.SendWait(" ");
                return WaitUntil(() => IsListItemChecked(list, index) == value, 2000);
            }
            catch
            {
                return false;
            }
        }

        private static WindowSnapshot FindLargestList(IntPtr mainWindow)
        {
            return GetDescendants(mainWindow)
                .Where(item => item.ClassName.IndexOf("SysListView32", StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderByDescending(item => SendMessage(item.Handle, LvmGetItemCount, IntPtr.Zero, IntPtr.Zero).ToInt32())
                .FirstOrDefault();
        }

        private static bool WaitUntil(Func<bool> predicate, int timeoutMilliseconds)
        {
            Stopwatch timer = Stopwatch.StartNew();
            while (timer.ElapsedMilliseconds < timeoutMilliseconds)
            {
                if (predicate())
                {
                    return true;
                }
                Thread.Sleep(50);
            }
            return predicate();
        }

        private static int WindowProcessId(IntPtr window)
        {
            uint processId;
            GetWindowThreadProcessId(window, out processId);
            return unchecked((int)processId);
        }

        private static string WindowText(IntPtr window)
        {
            int length = GetWindowTextLength(window);
            var text = new StringBuilder(Math.Max(1, length + 1));
            GetWindowText(window, text, text.Capacity);
            return text.ToString();
        }

        private static string WindowClass(IntPtr window)
        {
            var text = new StringBuilder(512);
            GetClassName(window, text, text.Capacity);
            return text.ToString();
        }

        private static string Hex(IntPtr value)
        {
            return "0x" + value.ToInt64().ToString("X");
        }

        private delegate bool EnumWindowsCallback(IntPtr window, IntPtr parameter);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct MenuItemInfo
        {
            public uint Size;
            public uint Mask;
            public uint Type;
            public uint State;
            public uint Id;
            public IntPtr SubMenu;
            public IntPtr CheckedBitmap;
            public IntPtr UncheckedBitmap;
            public UIntPtr ItemData;
            public IntPtr TypeData;
            public uint CharacterCount;
            public IntPtr ItemBitmap;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct ListViewItem
        {
            public uint Mask;
            public int Item;
            public int SubItem;
            public uint State;
            public uint StateMask;
            public IntPtr Text;
            public int TextMax;
            public int Image;
            public IntPtr Parameter;
            public int Indent;
            public int GroupId;
            public uint Columns;
            public IntPtr ColumnPointer;
            public IntPtr ColumnFormatPointer;
            public int Group;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Rectangle
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")] private static extern IntPtr GetMenu(IntPtr window);
        [DllImport("user32.dll")] private static extern int GetMenuItemCount(IntPtr menu);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern bool GetMenuItemInfo(IntPtr menu, uint item, bool byPosition, ref MenuItemInfo info);
        [DllImport("user32.dll")] private static extern bool PostMessage(IntPtr window, int message, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr window, int message, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", SetLastError = true)] private static extern IntPtr SendMessageTimeout(IntPtr window, int message, IntPtr wParam, IntPtr lParam, uint flags, uint timeoutMilliseconds, out IntPtr result);
        [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr window, int message, IntPtr wParam, ref ListViewItem item);
        [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr window, int message, IntPtr wParam, ref Rectangle rectangle);
        [DllImport("user32.dll")] private static extern bool EnumWindows(EnumWindowsCallback callback, IntPtr parameter);
        [DllImport("user32.dll")] private static extern bool EnumChildWindows(IntPtr parent, EnumWindowsCallback callback, IntPtr parameter);
        [DllImport("user32.dll")] private static extern bool IsWindow(IntPtr window);
        [DllImport("user32.dll")] private static extern bool IsWindowVisible(IntPtr window);
        [DllImport("user32.dll")] private static extern bool IsWindowEnabled(IntPtr window);
        [DllImport("user32.dll")] private static extern bool IsChild(IntPtr parent, IntPtr child);
        [DllImport("user32.dll")] private static extern IntPtr GetParent(IntPtr window);
        [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr window, int command);
        [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr window);
        [DllImport("user32.dll")] private static extern IntPtr SetFocus(IntPtr window);
        [DllImport("user32.dll")] private static extern IntPtr GetFocus();
        [DllImport("user32.dll")] private static extern bool AttachThreadInput(uint currentThread, uint targetThread, bool attach);
        [DllImport("user32.dll")] private static extern IntPtr GetDlgItem(IntPtr dialog, int itemId);
        [DllImport("user32.dll")] private static extern int GetDlgCtrlID(IntPtr window);
        [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr window, out Rectangle rectangle);
        [DllImport("user32.dll")] private static extern int GetWindowTextLength(IntPtr window);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetWindowText(IntPtr window, StringBuilder text, int capacity);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern IntPtr SendMessage(IntPtr window, int message, IntPtr wParam, string lParam);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetClassName(IntPtr window, StringBuilder text, int capacity);
        [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);
        [DllImport("kernel32.dll")] private static extern uint GetCurrentThreadId();
    }
}
