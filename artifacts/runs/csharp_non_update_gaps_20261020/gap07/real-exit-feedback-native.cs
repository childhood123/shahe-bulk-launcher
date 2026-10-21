using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

public static class Gap07Native
{
    public delegate bool EnumWindowsProc(IntPtr handle, IntPtr lParam);
    private const uint InputKeyboard = 1, KeyUp = 2, WmCommand = 0x0111, WmKeyDown = 0x0100, WmKeyUp = 0x0101, WmSysKeyDown = 0x0104, WmSysKeyUp = 0x0105;
    private const ushort VkEscape = 0x1B, VkAlt = 0x12, VkF4 = 0x73;
    private const uint MiimState = 1, MiimId = 2, MiimSubmenu = 4, MiimString = 0x40, MfsDisabled = 3;

    [StructLayout(LayoutKind.Sequential)] private struct KeyInput { public ushort Vk, Scan; public uint Flags, Time; public UIntPtr Extra; }
    [StructLayout(LayoutKind.Explicit, Size = 32)] private struct InputUnion { [FieldOffset(0)] public KeyInput Key; }
    [StructLayout(LayoutKind.Sequential)] private struct Input { public uint Type; public InputUnion Value; }
    [StructLayout(LayoutKind.Sequential)] private struct GuiThreadInfo { public uint Size, Flags; public IntPtr Active, Focus, Capture, MenuOwner, MoveSize, Caret; public int Left, Top, Right, Bottom; }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)] private struct MenuInfo
    {
        public uint Size, Mask, Type, State, Id;
        public IntPtr SubMenu, Checked, Unchecked, Data, Text;
        public uint TextLength;
        public IntPtr Bitmap;
    }

    [DllImport("user32.dll")] private static extern bool EnumWindows(EnumWindowsProc callback, IntPtr parameter);
    [DllImport("user32.dll")] private static extern bool EnumChildWindows(IntPtr parent, EnumWindowsProc callback, IntPtr parameter);
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr handle, out uint processId);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetWindowText(IntPtr handle, StringBuilder text, int length);
    [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr handle);
    [DllImport("user32.dll")] public static extern bool IsIconic(IntPtr handle);
    [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr handle);
    [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll", SetLastError = true)] private static extern uint SendInput(uint count, Input[] inputs, int size);
    [DllImport("user32.dll")] private static extern IntPtr GetMenu(IntPtr window);
    [DllImport("user32.dll")] private static extern int GetMenuItemCount(IntPtr menu);
    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)] private static extern bool GetMenuItemInfo(IntPtr menu, uint item, bool byPosition, ref MenuInfo info);
    [DllImport("user32.dll")] private static extern bool PostMessage(IntPtr window, uint message, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll")] private static extern bool GetGUIThreadInfo(uint threadId, ref GuiThreadInfo info);

    public static string Text(IntPtr handle)
    {
        var text = new StringBuilder(1024);
        GetWindowText(handle, text, text.Capacity);
        return text.ToString();
    }

    public static IntPtr FindMainWindow(int processId)
    {
        IntPtr result = IntPtr.Zero;
        EnumWindows((handle, unused) =>
        {
            uint candidate;
            GetWindowThreadProcessId(handle, out candidate);
            if ((int)candidate == processId && Text(handle).Length != 0) { result = handle; return false; }
            return true;
        }, IntPtr.Zero);
        return result;
    }

    public static string[] ChildTexts(IntPtr parent)
    {
        var result = new List<string>();
        EnumChildWindows(parent, (handle, unused) => { string text = Text(handle); if (text.Length != 0) result.Add(text); return true; }, IntPtr.Zero);
        return result.ToArray();
    }

    public static bool Focus(IntPtr window)
    {
        SetForegroundWindow(window);
        Thread.Sleep(250);
        return GetForegroundWindow() == window;
    }

    private static Input Key(ushort key, bool up)
    {
        return new Input { Type = InputKeyboard, Value = new InputUnion { Key = new KeyInput { Vk = key, Flags = up ? KeyUp : 0 } } };
    }

    private static void Send(params Input[] inputs)
    {
        if (SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input))) != inputs.Length)
            throw new InvalidOperationException("SendInput failed: " + Marshal.GetLastWin32Error());
    }

    public static void Escape() { Send(Key(VkEscape, false), Key(VkEscape, true)); }
    public static void AltF4() { Send(Key(VkAlt, false), Key(VkF4, false), Key(VkF4, true), Key(VkAlt, true)); }

    private static IntPtr FocusedWindow(IntPtr main)
    {
        uint processId;
        uint threadId = GetWindowThreadProcessId(main, out processId);
        var info = new GuiThreadInfo { Size = (uint)Marshal.SizeOf(typeof(GuiThreadInfo)) };
        return GetGUIThreadInfo(threadId, ref info) && info.Focus != IntPtr.Zero ? info.Focus : main;
    }

    public static bool PostEscape(IntPtr main)
    {
        IntPtr target = FocusedWindow(main);
        return PostMessage(target, WmKeyDown, new IntPtr(VkEscape), new IntPtr(1))
            && PostMessage(target, WmKeyUp, new IntPtr(VkEscape), new IntPtr(unchecked((int)0xC0000001)));
    }

    public static bool PostAltF4(IntPtr main)
    {
        return PostMessage(main, WmSysKeyDown, new IntPtr(VkF4), new IntPtr(0x20000001))
            && PostMessage(main, WmSysKeyUp, new IntPtr(VkF4), new IntPtr(unchecked((int)0xE0000001)));
    }

    private static int FindCommand(IntPtr menu, string prefix)
    {
        int count = GetMenuItemCount(menu);
        for (uint position = 0; position < count; position++)
        {
            IntPtr memory = Marshal.AllocHGlobal(2048);
            try
            {
                var info = new MenuInfo { Size = (uint)Marshal.SizeOf(typeof(MenuInfo)), Mask = MiimState | MiimId | MiimSubmenu | MiimString, Text = memory, TextLength = 1024 };
                if (!GetMenuItemInfo(menu, position, true, ref info)) continue;
                string text = Marshal.PtrToStringUni(memory) ?? String.Empty;
                if (text.StartsWith(prefix, StringComparison.Ordinal) && (info.State & MfsDisabled) == 0) return unchecked((int)info.Id);
                if (info.SubMenu != IntPtr.Zero) { int child = FindCommand(info.SubMenu, prefix); if (child >= 0) return child; }
            }
            finally { Marshal.FreeHGlobal(memory); }
        }
        return -1;
    }

    public static int InvokeMenu(IntPtr window, string prefix)
    {
        IntPtr menu = GetMenu(window);
        int command = menu == IntPtr.Zero ? -1 : FindCommand(menu, prefix);
        return command >= 0 && PostMessage(window, WmCommand, new IntPtr(command), IntPtr.Zero) ? command : -1;
    }
}
