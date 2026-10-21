[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)] [string]$BuildDirectory,
    [Parameter(Mandatory = $true)] [string]$ResultPath
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes

Add-Type -TypeDefinition @'
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public sealed class Gap09MenuEntry
{
    public int CommandId;
    public int State;
    public string Text;
    public List<Gap09MenuEntry> Children = new List<Gap09MenuEntry>();
}

public static class Gap09Native
{
    private const uint MiimState = 0x00000001;
    private const uint MiimId = 0x00000002;
    private const uint MiimSubmenu = 0x00000004;
    private const uint MiimString = 0x00000040;
    private const uint WmCommand = 0x0111;
    private const uint MfsDisabled = 0x00000003;
    private const uint LbGetCount = 0x018B;

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
    [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr window, uint message, IntPtr wParam, IntPtr lParam);

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

    private static Gap09MenuEntry[] ReadMenu(IntPtr menu)
    {
        var entries = new List<Gap09MenuEntry>();
        int count = GetMenuItemCount(menu);
        for (uint position = 0; position < count; position++)
        {
            MenuItemInfo info = ReadItem(menu, position);
            string text;
            try { text = info.Text == IntPtr.Zero ? String.Empty : Marshal.PtrToStringUni(info.Text) ?? String.Empty; }
            finally { if (info.Text != IntPtr.Zero) Marshal.FreeHGlobal(info.Text); }
            var entry = new Gap09MenuEntry
            {
                CommandId = unchecked((int)info.Id),
                State = unchecked((int)info.State),
                Text = text
            };
            if (info.SubMenu != IntPtr.Zero) entry.Children.AddRange(ReadMenu(info.SubMenu));
            entries.Add(entry);
        }
        return entries.ToArray();
    }

    private static Gap09MenuEntry Find(IntPtr window, string[] path)
    {
        Gap09MenuEntry[] entries = ReadMenu(GetMenu(window));
        Gap09MenuEntry current = null;
        for (int depth = 0; depth < path.Length; depth++)
        {
            Gap09MenuEntry[] candidates = current == null ? entries : current.Children.ToArray();
            current = null;
            foreach (Gap09MenuEntry entry in candidates)
            {
                if (String.Equals(entry.Text, path[depth], StringComparison.Ordinal))
                {
                    current = entry;
                    break;
                }
            }
            if (current == null) throw new InvalidOperationException("Menu path not found: " + String.Join("|", path));
        }
        return current;
    }

    public static int GetCommandId(IntPtr window, string[] path)
    {
        return Find(window, path).CommandId;
    }

    public static bool InvokeMenu(IntPtr window, string[] path)
    {
        Gap09MenuEntry entry = Find(window, path);
        return (entry.State & MfsDisabled) == 0
            && PostMessage(window, WmCommand, new IntPtr(entry.CommandId), IntPtr.Zero);
    }

    public static int GetListBoxCount(IntPtr window)
    {
        return unchecked((int)SendMessage(window, LbGetCount, IntPtr.Zero, IntPtr.Zero).ToInt64());
    }
}
'@

function Require {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) { throw $Message }
}

function Get-RunValueSnapshot {
    $path = 'Software\Microsoft\Windows\CurrentVersion\Run'
    $key = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey($path, $false)
    try {
        if ($null -eq $key -or -not ($key.GetValueNames() -contains 'SandboxLauncher')) {
            return [pscustomobject]@{ Exists = $false; Value = $null; Kind = $null }
        }
        return [pscustomobject]@{
            Exists = $true
            Value = $key.GetValue('SandboxLauncher', $null, [Microsoft.Win32.RegistryValueOptions]::DoNotExpandEnvironmentNames)
            Kind = [string]$key.GetValueKind('SandboxLauncher')
        }
    }
    finally {
        if ($null -ne $key) { $key.Dispose() }
    }
}

function Restore-RunValue {
    param([object]$Snapshot)
    $path = 'Software\Microsoft\Windows\CurrentVersion\Run'
    $key = [Microsoft.Win32.Registry]::CurrentUser.CreateSubKey($path)
    try {
        if ($Snapshot.Exists) {
            $kind = [Microsoft.Win32.RegistryValueKind][Enum]::Parse(
                [Microsoft.Win32.RegistryValueKind], $Snapshot.Kind)
            $key.SetValue('SandboxLauncher', $Snapshot.Value, $kind)
        }
        else {
            $key.DeleteValue('SandboxLauncher', $false)
        }
    }
    finally {
        $key.Dispose()
    }
}

function Write-IsolatedConfiguration {
    param([string]$ScenarioDirectory, [string]$SandboxieDirectory, [string]$SandboxRoot)
    $history = @(
        for ($index = 0; $index -lt 1000; $index++) {
            [ordered]@{ path = "C:\\Gap09\\App$index.exe"; note = "应用$index" }
        }
    )
    $settings = [ordered]@{
        app_history = $history
        auto_run = $false
        hide_path = $false
        hotkeys_enabled = $false
        max_history_items = 999
        sandbox_root = $SandboxRoot
        sandboxie_path = $SandboxieDirectory
        show_tray = $false
    }
    $lines = [System.Collections.Generic.List[string]]::new()
    [void]$lines.Add('[config]')
    [void]$lines.Add('')
    [void]$lines.Add('[settings]')
    foreach ($entry in $settings.GetEnumerator()) {
        $json = ConvertTo-Json -InputObject $entry.Value -Compress -Depth 8
        [void]$lines.Add($entry.Key + '=' + $json)
    }
    $configurationDirectory = Join-Path $ScenarioDirectory 'Configuration'
    $null = New-Item -ItemType Directory -Path $configurationDirectory -Force
    $path = Join-Path $configurationDirectory 'shahe_config.ini'
    [System.IO.File]::WriteAllText(
        $path,
        ([string]::Join("`r`n", $lines) + "`r`n"),
        [System.Text.UTF8Encoding]::new($false))
    return $path
}

function Read-IsolatedConfiguration {
    param([string]$Path)
    $inSettings = $false
    $values = @{}
    foreach ($line in [System.IO.File]::ReadAllLines($Path, [System.Text.Encoding]::UTF8)) {
        if ($line -match '^\s*\[(.+)\]\s*$') {
            $inSettings = $Matches[1] -eq 'settings'
            continue
        }
        if (-not $inSettings -or $line.Trim().Length -eq 0) { continue }
        $separator = $line.IndexOf('=')
        if ($separator -le 0) { continue }
        $name = $line.Substring(0, $separator).Trim()
        if ($name.EndsWith('_updated_at', [StringComparison]::Ordinal)) { continue }
        $raw = $line.Substring($separator + 1)
        try { $values[$name] = ConvertFrom-Json -InputObject $raw }
        catch { $values[$name] = $raw }
    }
    $historyCount = @($values['app_history']).Count
    return [pscustomobject]@{
        MaxHistoryItems = [Convert]::ToInt32($values['max_history_items'])
        HistoryCount = $historyCount
    }
}

function Start-Target {
    param([string]$Executable, [string]$WorkingDirectory)
    $process = Start-Process -FilePath $Executable -WorkingDirectory $WorkingDirectory -PassThru
    $deadline = [DateTime]::UtcNow.AddSeconds(20)
    while ([DateTime]::UtcNow -lt $deadline) {
        if ($process.HasExited) { throw "目标进程提前退出，PID=$($process.Id)，ExitCode=$($process.ExitCode)" }
        $process.Refresh()
        if ($process.MainWindowHandle -ne [IntPtr]::Zero -and $process.MainWindowTitle -eq '沙河批量打开器') {
            return [pscustomobject]@{ Process = $process; Handle = $process.MainWindowHandle }
        }
        Start-Sleep -Milliseconds 100
    }
    throw "等待主窗口超时，PID=$($process.Id)，Title=$($process.MainWindowTitle)"
}

function Find-TopLevelWindow {
    param([int]$TargetPid, [string]$Name)
    $windows = [System.Windows.Automation.AutomationElement]::RootElement.FindAll(
        [System.Windows.Automation.TreeScope]::Children,
        [System.Windows.Automation.Condition]::TrueCondition)
    foreach ($window in $windows) {
        try {
            if ($window.Current.ProcessId -eq $TargetPid -and $window.Current.Name -eq $Name) { return $window }
        }
        catch { }
    }
    return $null
}

function Wait-TopLevelWindow {
    param([int]$TargetPid, [string]$Name, [bool]$Present)
    $deadline = [DateTime]::UtcNow.AddSeconds(15)
    while ([DateTime]::UtcNow -lt $deadline) {
        $window = Find-TopLevelWindow -TargetPid $TargetPid -Name $Name
        if ($Present -and $null -ne $window) { return $window }
        if (-not $Present -and $null -eq $window) { return $null }
        Start-Sleep -Milliseconds 100
    }
    throw "等待窗口状态超时：PID=$TargetPid，Name=$Name，Present=$Present"
}

function Find-Descendant {
    param([object]$Root, [string]$Name, [string]$ControlTypeName)
    $elements = $Root.FindAll(
        [System.Windows.Automation.TreeScope]::Descendants,
        [System.Windows.Automation.Condition]::TrueCondition)
    foreach ($element in $elements) {
        try {
            $typeName = $element.Current.ControlType.ProgrammaticName
            if ($element.Current.Name -eq $Name -and ($ControlTypeName.Length -eq 0 -or $typeName -eq $ControlTypeName)) {
                return $element
            }
        }
        catch { }
    }
    return $null
}

function Get-SpinnerSnapshot {
    param([object]$Dialog, [Nullable[int]]$SetValue)
    $spinner = Find-Descendant -Root $Dialog -Name '路径记忆数量' -ControlTypeName 'ControlType.Spinner'
    if ($null -eq $spinner) {
        $spinner = Find-Descendant -Root $Dialog -Name '路径记忆数量' -ControlTypeName ''
    }
    Require ($null -ne $spinner) '未找到 AccessibleName=路径记忆数量 的输入控件'

    $rangeObject = $null
    $valueObject = $null
    $patternName = ''
    if ($spinner.TryGetCurrentPattern([System.Windows.Automation.RangeValuePattern]::Pattern, [ref]$rangeObject)) {
        $patternName = 'RangeValuePattern'
        if ($null -ne $SetValue) {
            $spinner.SetFocus()
            ([System.Windows.Automation.RangeValuePattern]$rangeObject).SetValue([double]$SetValue.Value)
            Start-Sleep -Milliseconds 100
        }
        $current = ([System.Windows.Automation.RangeValuePattern]$rangeObject).Current
        $value = $current.Value
        $minimum = $current.Minimum
        $maximum = $current.Maximum
    }
    elseif ($spinner.TryGetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern, [ref]$valueObject)) {
        $patternName = 'ValuePattern'
        if ($null -ne $SetValue) {
            $spinner.SetFocus()
            ([System.Windows.Automation.ValuePattern]$valueObject).SetValue([string]$SetValue.Value)
            Start-Sleep -Milliseconds 100
        }
        $value = [Convert]::ToDouble(([System.Windows.Automation.ValuePattern]$valueObject).Current.Value)
        $minimum = 0
        $maximum = 999
    }
    else {
        throw '路径记忆数量控件不支持 RangeValuePattern 或 ValuePattern'
    }

    return [pscustomobject]@{
        AccessibleName = $spinner.Current.Name
        AutomationId = $spinner.Current.AutomationId
        ControlType = $spinner.Current.ControlType.ProgrammaticName
        NativeWindowHandle = $spinner.Current.NativeWindowHandle
        Pattern = $patternName
        Minimum = [int]$minimum
        Maximum = [int]$maximum
        Value = [int]$value
    }
}

function Get-HistoryListCount {
    param([object]$Dialog)
    $list = Find-Descendant -Root $Dialog -Name '已记忆的路径列表' -ControlTypeName 'ControlType.List'
    if ($null -eq $list) { $list = Find-Descendant -Root $Dialog -Name '已记忆的路径列表' -ControlTypeName '' }
    Require ($null -ne $list) '未找到 AccessibleName=已记忆的路径列表 的列表控件'
    Require ($list.Current.NativeWindowHandle -ne 0) '路径列表没有原生窗口句柄'
    return [Gap09Native]::GetListBoxCount([IntPtr]$list.Current.NativeWindowHandle)
}

function Invoke-DialogButton {
    param([object]$Dialog, [string]$Name)
    $button = Find-Descendant -Root $Dialog -Name $Name -ControlTypeName 'ControlType.Button'
    Require ($null -ne $button) "未找到按钮：$Name"
    $invokeObject = $null
    Require ($button.TryGetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern, [ref]$invokeObject)) "按钮不支持 InvokePattern：$Name"
    $button.SetFocus()
    ([System.Windows.Automation.InvokePattern]$invokeObject).Invoke()
}

function Open-MemoryDialog {
    param([int]$TargetPid, [IntPtr]$MainHandle)
    $path = [string[]]@('设置(&S)', '路径记忆管理')
    $command = [Gap09Native]::GetCommandId($MainHandle, $path)
    Require ([Gap09Native]::InvokeMenu($MainHandle, $path)) '路径记忆管理菜单投递失败'
    $dialog = Wait-TopLevelWindow -TargetPid $TargetPid -Name '路径记忆管理' -Present $true
    return [pscustomobject]@{ CommandId = $command; Dialog = $dialog }
}

function Close-Target {
    param([object]$Process, [IntPtr]$MainHandle)
    $path = [string[]]@('文件(&F)', '退出软件')
    $command = [Gap09Native]::GetCommandId($MainHandle, $path)
    $invoked = [Gap09Native]::InvokeMenu($MainHandle, $path)
    $natural = $false
    if ($invoked) { $natural = $Process.WaitForExit(10000) }
    $forced = $false
    if (-not $natural -and -not $Process.HasExited) {
        Stop-Process -Id $Process.Id -Force
        $null = $Process.WaitForExit(5000)
        $forced = $true
    }
    return [pscustomobject]@{
        CommandId = $command
        Invoked = $invoked
        NaturalExit = $natural
        Forced = $forced
        ExitCode = if ($Process.HasExited) { $Process.ExitCode } else { $null }
    }
}

$resolvedBuild = (Resolve-Path -LiteralPath $BuildDirectory).Path
$sourceExecutable = Join-Path $resolvedBuild '沙河批量打开器.exe'
Require (Test-Path -LiteralPath $sourceExecutable -PathType Leaf) "未找到本轮 Release EXE：$sourceExecutable"
$resultFile = [System.IO.Path]::GetFullPath($ResultPath)
$runRoot = Join-Path (Split-Path -Parent $resultFile) ('real-exe-' + (Get-Date -Format 'yyyyMMdd_HHmmss_fff'))
$null = New-Item -ItemType Directory -Path $runRoot -Force
$registryBefore = Get-RunValueSnapshot
$ownedPids = [System.Collections.Generic.List[int]]::new()
$forcedCleanup = [System.Collections.Generic.List[int]]::new()
$scenarios = [System.Collections.ArrayList]::new()
$caught = $null
$result = [ordered]@{
    timestamp = (Get-Date).ToString('o')
    success = $false
    buildDirectory = $resolvedBuild
    sourceExecutable = $sourceExecutable
    sourceSha256 = (Get-FileHash -LiteralPath $sourceExecutable -Algorithm SHA256).Hash
    runRoot = $runRoot
    inputMethod = 'exact classic-menu WM_COMMAND plus UI Automation patterns; no keyboard input'
    registryBefore = $registryBefore
    registryAfter = $null
    registryRestored = $false
    scenarios = $scenarios
    cleanup = $null
    error = $null
}

try {
    foreach ($target in @(0, 21, 999)) {
        $scenarioDirectory = Join-Path $runRoot ('history-' + $target)
        $null = New-Item -ItemType Directory -Path $scenarioDirectory -Force
        Copy-Item -Path (Join-Path $resolvedBuild '*') -Destination $scenarioDirectory -Recurse -Force
        $sandboxieDirectory = Join-Path $scenarioDirectory 'fake-sandboxie'
        $sandboxRoot = Join-Path $scenarioDirectory 'sandbox-root'
        $null = New-Item -ItemType Directory -Path $sandboxieDirectory -Force
        $null = New-Item -ItemType Directory -Path $sandboxRoot -Force
        [System.IO.File]::WriteAllBytes((Join-Path $sandboxieDirectory 'Start.exe'), [byte[]]@())
        $configPath = Write-IsolatedConfiguration -ScenarioDirectory $scenarioDirectory -SandboxieDirectory $sandboxieDirectory -SandboxRoot $sandboxRoot
        $executable = Join-Path $scenarioDirectory '沙河批量打开器.exe'
        $scenario = [pscustomobject][ordered]@{
            target = $target
            directory = $scenarioDirectory
            executableSha256 = (Get-FileHash -LiteralPath $executable -Algorithm SHA256).Hash
            configPath = $configPath
            firstPid = $null
            firstMainWindowHandle = $null
            firstDialogWindowHandle = $null
            memoryMenuCommandId = $null
            initialSpinner = $null
            initialHistoryCount = $null
            savedSpinner = $null
            savedConfig = $null
            firstExit = $null
            restartPid = $null
            restartMainWindowHandle = $null
            restartDialogWindowHandle = $null
            restartSpinner = $null
            restartHistoryCount = $null
            rangeHintFound = $false
            restartConfig = $null
            restartExit = $null
            remainingOwnedPids = $null
        }
        [void]$scenarios.Add($scenario)

        $first = Start-Target -Executable $executable -WorkingDirectory $scenarioDirectory
        [void]$ownedPids.Add($first.Process.Id)
        $scenario.firstPid = $first.Process.Id
        $scenario.firstMainWindowHandle = ('0x{0:X}' -f $first.Handle.ToInt64())
        $opened = Open-MemoryDialog -TargetPid $first.Process.Id -MainHandle $first.Handle
        $scenario.memoryMenuCommandId = $opened.CommandId
        $scenario.firstDialogWindowHandle = ('0x{0:X}' -f $opened.Dialog.Current.NativeWindowHandle)
        $scenario.initialSpinner = Get-SpinnerSnapshot -Dialog $opened.Dialog
        $scenario.initialHistoryCount = Get-HistoryListCount -Dialog $opened.Dialog
        Require ($scenario.initialSpinner.Maximum -eq 999) "首次对话框最大值不是 999：$($scenario.initialSpinner.Maximum)"
        Require ($scenario.initialSpinner.Value -eq 999) "首次对话框初始值不是 999：$($scenario.initialSpinner.Value)"
        Require ($scenario.initialHistoryCount -eq 999) "首次加载未保留 999 条：$($scenario.initialHistoryCount)"
        $scenario.savedSpinner = Get-SpinnerSnapshot -Dialog $opened.Dialog -SetValue $target
        Require ($scenario.savedSpinner.Value -eq $target) "输入值未设置为 $target：$($scenario.savedSpinner.Value)"
        Invoke-DialogButton -Dialog $opened.Dialog -Name '确定'
        $null = Wait-TopLevelWindow -TargetPid $first.Process.Id -Name '路径记忆管理' -Present $false
        $scenario.savedConfig = Read-IsolatedConfiguration -Path $configPath
        Require ($scenario.savedConfig.MaxHistoryItems -eq $target) "保存后的配置值错误：$($scenario.savedConfig.MaxHistoryItems)"
        Require ($scenario.savedConfig.HistoryCount -eq $target) "保存后的记录数错误：$($scenario.savedConfig.HistoryCount)"
        $scenario.firstExit = Close-Target -Process $first.Process -MainHandle $first.Handle
        Require ($scenario.firstExit.Invoked -and $scenario.firstExit.NaturalExit -and -not $scenario.firstExit.Forced) "首次进程未自然退出，PID=$($first.Process.Id)"
        Require ($scenario.firstExit.ExitCode -eq 0) "首次进程退出码不是 0：$($scenario.firstExit.ExitCode)"

        $restart = Start-Target -Executable $executable -WorkingDirectory $scenarioDirectory
        [void]$ownedPids.Add($restart.Process.Id)
        $scenario.restartPid = $restart.Process.Id
        $scenario.restartMainWindowHandle = ('0x{0:X}' -f $restart.Handle.ToInt64())
        $reopened = Open-MemoryDialog -TargetPid $restart.Process.Id -MainHandle $restart.Handle
        $scenario.restartDialogWindowHandle = ('0x{0:X}' -f $reopened.Dialog.Current.NativeWindowHandle)
        $scenario.restartSpinner = Get-SpinnerSnapshot -Dialog $reopened.Dialog
        $scenario.restartHistoryCount = Get-HistoryListCount -Dialog $reopened.Dialog
        $scenario.rangeHintFound = $null -ne (Find-Descendant -Root $reopened.Dialog -Name '(0=不记录, 最大999)' -ControlTypeName '')
        Require ($scenario.restartSpinner.AccessibleName -eq '路径记忆数量') '重启后的 AccessibleName 不匹配'
        Require ($scenario.restartSpinner.Minimum -eq 0 -and $scenario.restartSpinner.Maximum -eq 999) '重启后的输入范围不匹配'
        Require ($scenario.restartSpinner.Value -eq $target) "重启后的输入值错误：$($scenario.restartSpinner.Value)"
        Require ($scenario.restartHistoryCount -eq $target) "重启后的列表记录数错误：$($scenario.restartHistoryCount)"
        Require $scenario.rangeHintFound '未找到“(0=不记录, 最大999)”提示'
        $scenario.restartConfig = Read-IsolatedConfiguration -Path $configPath
        Require ($scenario.restartConfig.MaxHistoryItems -eq $target) '重启后的配置值发生变化'
        Require ($scenario.restartConfig.HistoryCount -eq $target) '重启后的持久化记录数发生变化'
        Invoke-DialogButton -Dialog $reopened.Dialog -Name '取消'
        $null = Wait-TopLevelWindow -TargetPid $restart.Process.Id -Name '路径记忆管理' -Present $false
        $scenario.restartExit = Close-Target -Process $restart.Process -MainHandle $restart.Handle
        Require ($scenario.restartExit.Invoked -and $scenario.restartExit.NaturalExit -and -not $scenario.restartExit.Forced) "重启进程未自然退出，PID=$($restart.Process.Id)"
        Require ($scenario.restartExit.ExitCode -eq 0) "重启进程退出码不是 0：$($scenario.restartExit.ExitCode)"
        $scenario.remainingOwnedPids = @(
            $ownedPids | Where-Object { $null -ne (Get-Process -Id $_ -ErrorAction SilentlyContinue) }
        )
        Require ($scenario.remainingOwnedPids.Count -eq 0) "场景结束后仍有目标进程：$($scenario.remainingOwnedPids -join ',')"
    }
}
catch {
    $caught = $_
    $result.error = $_.Exception.ToString()
}
finally {
    foreach ($ownedPid in $ownedPids) {
        $remaining = Get-Process -Id $ownedPid -ErrorAction SilentlyContinue
        if ($null -ne $remaining) {
            Stop-Process -Id $ownedPid -Force
            [void]$forcedCleanup.Add($ownedPid)
        }
    }
    Restore-RunValue -Snapshot $registryBefore
    $registryAfter = Get-RunValueSnapshot
    $result.registryAfter = $registryAfter
    $result.registryRestored = (ConvertTo-Json -InputObject $registryBefore -Compress -Depth 4) -eq
        (ConvertTo-Json -InputObject $registryAfter -Compress -Depth 4)
    $remainingPids = @(
        $ownedPids | Where-Object { $null -ne (Get-Process -Id $_ -ErrorAction SilentlyContinue) }
    )
    $result.cleanup = [ordered]@{
        ownedPids = @($ownedPids)
        forcedPids = @($forcedCleanup)
        remainingPids = $remainingPids
    }
    $result.success = $null -eq $caught -and $result.registryRestored -and $remainingPids.Count -eq 0
    $json = ConvertTo-Json -InputObject $result -Depth 12
    [System.IO.File]::WriteAllText($resultFile, $json, [System.Text.UTF8Encoding]::new($false))
    Write-Output $json
}

if ($null -ne $caught) {
    $ErrorActionPreference = 'Continue'
    Write-Error $caught.Exception.Message
    exit 1
}
if (-not $result.success) { exit 1 }
exit 0
