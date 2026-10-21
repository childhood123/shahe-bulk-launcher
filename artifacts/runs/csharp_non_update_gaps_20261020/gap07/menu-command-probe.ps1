[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)] [int]$ProcessId,
    [Parameter(Mandatory = $true)] [ValidateSet('dump', 'invoke')]
    [string]$Action,
    [string]$MenuPath = '设置(&S)|路径记忆管理'
)

$ErrorActionPreference = 'Stop'

Add-Type -TypeDefinition @'
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

public sealed class RemainingMenuEntry
{
    public int Position;
    public int CommandId;
    public int State;
    public string Text;
    public List<RemainingMenuEntry> Children = new List<RemainingMenuEntry>();
}

public static class RemainingMenuNative
{
    private const uint MiimState = 0x00000001;
    private const uint MiimId = 0x00000002;
    private const uint MiimSubmenu = 0x00000004;
    private const uint MiimString = 0x00000040;
    private const uint WmCommand = 0x0111;
    private const uint MfsDisabled = 0x00000003;

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
        public IntPtr ItemData;
        public IntPtr Text;
        public uint TextLength;
        public IntPtr AcceleratedText;
    }

    [DllImport("user32.dll")] private static extern IntPtr GetMenu(IntPtr window);
    [DllImport("user32.dll")] private static extern int GetMenuItemCount(IntPtr menu);
    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool GetMenuItemInfo(IntPtr menu, uint item, bool byPosition, ref MenuItemInfo info);
    [DllImport("user32.dll")] private static extern bool PostMessage(IntPtr window, uint message, IntPtr wParam, IntPtr lParam);

    private static MenuItemInfo ReadItem(IntPtr menu, uint position)
    {
        var info = new MenuItemInfo
        {
            Size = (uint)Marshal.SizeOf(typeof(MenuItemInfo)),
            Mask = MiimState | MiimId | MiimSubmenu | MiimString,
            TextLength = 1024,
            Text = Marshal.AllocHGlobal(2048)
        };
        if (!GetMenuItemInfo(menu, position, true, ref info))
        {
            Marshal.FreeHGlobal(info.Text);
            throw new InvalidOperationException("GetMenuItemInfo failed: " + Marshal.GetLastWin32Error());
        }
        return info;
    }

    private static string ReadText(MenuItemInfo info)
    {
        try { return info.Text == IntPtr.Zero ? String.Empty : Marshal.PtrToStringUni(info.Text) ?? String.Empty; }
        finally { if (info.Text != IntPtr.Zero) Marshal.FreeHGlobal(info.Text); }
    }

    private static RemainingMenuEntry[] ReadMenuInternal(IntPtr menu)
    {
        var entries = new List<RemainingMenuEntry>();
        int count = GetMenuItemCount(menu);
        for (uint position = 0; position < count; position++)
        {
            MenuItemInfo info = ReadItem(menu, position);
            string text = ReadText(info);
            var entry = new RemainingMenuEntry
            {
                Position = (int)position,
                CommandId = unchecked((int)info.Id),
                State = unchecked((int)info.State),
                Text = text
            };
            if (info.SubMenu != IntPtr.Zero) entry.Children.AddRange(ReadMenuInternal(info.SubMenu));
            entries.Add(entry);
        }
        return entries.ToArray();
    }

    public static RemainingMenuEntry[] ReadMenu(IntPtr window)
    {
        IntPtr menu = GetMenu(window);
        if (menu == IntPtr.Zero) throw new InvalidOperationException("Main menu was not found");
        return ReadMenuInternal(menu);
    }

    public static string MenuInfo(IntPtr window, string[] path)
    {
        RemainingMenuEntry[] entries = ReadMenu(window);
        RemainingMenuEntry current = null;
        for (int depth = 0; depth < path.Length; depth++)
        {
            RemainingMenuEntry[] candidates = current == null ? entries : current.Children.ToArray();
            current = null;
            foreach (RemainingMenuEntry entry in candidates)
            {
                if (String.Equals(entry.Text, path[depth], StringComparison.Ordinal))
                {
                    current = entry;
                    break;
                }
            }
            if (current == null) return "found=False;path=" + String.Join("|", path);
        }
        return String.Format("found=True;command={0};state=0x{1:X};enabled={2};path={3}",
            current.CommandId, current.State, (current.State & MfsDisabled) == 0, String.Join("|", path));
    }

    public static bool Invoke(IntPtr window, string[] path)
    {
        RemainingMenuEntry[] entries = ReadMenu(window);
        RemainingMenuEntry current = null;
        for (int depth = 0; depth < path.Length; depth++)
        {
            RemainingMenuEntry[] candidates = current == null ? entries : current.Children.ToArray();
            current = null;
            foreach (RemainingMenuEntry entry in candidates)
            {
                if (String.Equals(entry.Text, path[depth], StringComparison.Ordinal))
                {
                    current = entry;
                    break;
                }
            }
            if (current == null || (depth < path.Length - 1 && current.Children.Count == 0)) return false;
        }
        if ((current.State & MfsDisabled) != 0) return false;
        return PostMessage(window, WmCommand, new IntPtr(current.CommandId), IntPtr.Zero);
    }
}
'@

$targetProcess = Get-Process -Id $ProcessId
$targetProcess.Refresh()
$mainHandle = $targetProcess.MainWindowHandle
if ($mainHandle -eq [IntPtr]::Zero) { throw "Process $ProcessId has no main window handle" }

function Write-Entries {
    param([object[]]$Entries, [int]$Depth = 0)
    foreach ($entry in $Entries) {
        ('{0}position={1} command={2} state=0x{3:X} text={4}' -f ('  ' * $Depth), $entry.Position, $entry.CommandId, $entry.State, $entry.Text)
        if ($entry.Children.Count -gt 0) { Write-Entries $entry.Children ($Depth + 1) }
    }
}

if ($Action -eq 'dump') {
    Write-Output ('window=0x{0:X}' -f $mainHandle.ToInt64())
    Write-Entries ([RemainingMenuNative]::ReadMenu($mainHandle))
    exit 0
}

$pathParts = $MenuPath -split '\|'
Write-Output ([RemainingMenuNative]::MenuInfo($mainHandle, $pathParts))
$invoked = [RemainingMenuNative]::Invoke($mainHandle, $pathParts)
Write-Output ('invoked={0};path={1}' -f $invoked, $MenuPath)
