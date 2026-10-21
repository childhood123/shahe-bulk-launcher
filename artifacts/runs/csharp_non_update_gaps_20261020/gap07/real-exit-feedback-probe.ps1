param([string]$OutputPath = '')
$ErrorActionPreference = 'Stop'
$runDirectory = $PSScriptRoot
if (-not $OutputPath) { $OutputPath = Join-Path $runDirectory 'real-exit-feedback-result.json' }
$source = Join-Path $runDirectory 'bin\x64\Release\net48'
$root = Join-Path $runDirectory 'real-exe'
New-Item -ItemType Directory -Path $root -Force | Out-Null
Add-Type -Path (Join-Path $runDirectory 'real-exit-feedback-native.cs')

function Wait-Condition([scriptblock]$Condition, [int]$Milliseconds = 10000) {
    $deadline = [DateTime]::UtcNow.AddMilliseconds($Milliseconds)
    do { if (& $Condition) { return $true }; Start-Sleep -Milliseconds 100 } while ([DateTime]::UtcNow -lt $deadline)
    return $false
}

function New-App([string]$Name, [bool]$ShowTray) {
    $directory = Join-Path $root $Name
    New-Item -ItemType Directory -Path $directory -Force | Out-Null
    foreach ($file in @('沙河批量打开器.exe','沙河批量打开器.exe.config','ShaheLauncher.Core.dll','updater.exe','updater.exe.config','Newtonsoft.Json.dll','BouncyCastle.Crypto.dll')) {
        Copy-Item -LiteralPath (Join-Path $source $file) -Destination $directory -Force
    }
    Copy-Item -LiteralPath (Join-Path $source 'lib') -Destination $directory -Recurse -Force
    $configDirectory = Join-Path $directory 'Configuration'
    New-Item -ItemType Directory -Path $configDirectory -Force | Out-Null
    $tray = if ($ShowTray) { 'true' } else { 'false' }
    $config = "[settings]`r`nauto_run=false`r`nshow_tray=$tray`r`nhotkeys_enabled=false`r`nsandboxie_path=`"C:\\Program Files\\Sandboxie`"`r`nsandbox_root=`"C:\\Sandbox\\Administrator`"`r`nlog_output_mode=`"detailed`"`r`n"
    [IO.File]::WriteAllText((Join-Path $configDirectory 'shahe_config.ini'), $config, [Text.UTF8Encoding]::new($false))
    return $directory
}

function Read-Log([string]$Directory) {
    $logDirectory = Join-Path $Directory 'log'
    if (-not (Test-Path -LiteralPath $logDirectory)) { return '' }
    $file = Get-ChildItem -LiteralPath $logDirectory -Filter '*.log' | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if ($null -eq $file) { return '' }
    return Get-Content -LiteralPath $file.FullName -Raw
}

function Cleanup([Diagnostics.Process]$Process) {
    $value = [ordered]@{close_main_window=$false; forced=$false; exited=$false; exit_code=$null}
    try {
        $Process.Refresh()
        if (-not $Process.HasExited) {
            $value.close_main_window = $Process.CloseMainWindow()
            if (-not $Process.WaitForExit(3000)) { Stop-Process -Id $Process.Id -Force; $value.forced = $true; $Process.WaitForExit(5000) | Out-Null }
        }
        $Process.Refresh(); $value.exited = $Process.HasExited
        if ($Process.HasExited) { $value.exit_code = $Process.ExitCode }
    } catch {}
    return [pscustomobject]$value
}

function Run-Scenario([string]$Name, [bool]$ShowTray, [string]$Action, [string]$Expected) {
    $directory = New-App $Name $ShowTray
    $exe = Join-Path $directory '沙河批量打开器.exe'
    $info = [Diagnostics.ProcessStartInfo]::new($exe); $info.WorkingDirectory = $directory; $info.UseShellExecute = $true
    $process = [Diagnostics.Process]::Start($info)
    $record = [ordered]@{name=$Name;action=$Action;show_tray=$ShowTray;pid=$process.Id;executable=$exe;main_window='';input_method='';focus_confirmed=$null;command_id=$null;menu_probe='';action_sent=$false;status_observed=$false;visible_after=$null;iconic_after=$null;exited_after_action=$false;exit_code=$null;cleanup=$null;result='inconclusive';error=''}
    try {
        $handle = [IntPtr]::Zero
        $ready = Wait-Condition { $script:gap07Handle = [Gap07Native]::FindMainWindow($process.Id); $script:gap07Handle -ne [IntPtr]::Zero } 20000
        $handle = $script:gap07Handle
        if (-not $ready) { throw 'Main window handle was not found.' }
        $record.main_window = '0x{0:X}' -f $handle.ToInt64()
        Start-Sleep -Seconds 5
        if ($Action -eq 'menu') {
            $menuOutput = & (Join-Path $runDirectory 'menu-command-probe.ps1') -ProcessId $process.Id -Action invoke -MenuPath ("文件(&F)|退出软件`tAlt+F4") 2>&1
            $record.menu_probe = $menuOutput -join "`n"
            if ($record.menu_probe -match 'command=(\d+)') { $record.command_id = [int]$Matches[1] }
            $record.action_sent = $record.menu_probe -match 'invoked=True'
            if (-not $record.action_sent) { throw 'Exit menu command was not found or invoked.' }
        } else {
            $shell = New-Object -ComObject WScript.Shell
            $activated = $shell.AppActivate($process.Id)
            Start-Sleep -Milliseconds 250
            $record.focus_confirmed = $activated -and [Gap07Native]::Focus($handle)
            if (-not $record.focus_confirmed) { throw 'Foreground focus was not confirmed.' }
            $record.input_method = 'PostMessage to focused WinForms handle'
            $record.action_sent = if ($Action -eq 'escape') { [Gap07Native]::PostEscape($handle) } else { [Gap07Native]::PostAltF4($handle) }
            if (-not $record.action_sent) { throw 'Keyboard messages were not posted.' }
        }
        if ($Action -eq 'escape') {
            $record.status_observed = Wait-Condition { (Read-Log $directory).Contains('状态: ' + $Expected) } 5000
            $record.visible_after = [Gap07Native]::IsWindowVisible($handle)
            $record.iconic_after = [Gap07Native]::IsIconic($handle)
            $stateOk = if ($ShowTray) { -not $record.visible_after } else { $record.iconic_after }
            $record.result = if ($record.status_observed -and $stateOk) { 'passed' } else { 'failed' }
        } else {
            $record.exited_after_action = $process.WaitForExit(10000)
            if ($record.exited_after_action) { $record.exit_code = $process.ExitCode }
            Start-Sleep -Milliseconds 300
            $record.status_observed = (Read-Log $directory).Contains('状态: ' + $Expected)
            $record.result = if ($record.exited_after_action -and $record.exit_code -eq 0 -and $record.status_observed) { 'passed' } else { 'failed' }
        }
    } catch { $record.error = $_.Exception.Message; $record.result = 'inconclusive' }
    finally { $record.cleanup = Cleanup $process }
    return [pscustomobject]$record
}

$scenarios = @()
$scenarios += Run-Scenario 'escape-window-fallback' $false 'escape' '当前托盘不可用，已最小化窗口'; Start-Sleep -Seconds 1
$scenarios += Run-Scenario 'escape-tray' $true 'escape' '已最小化到托盘'; Start-Sleep -Seconds 1
$scenarios += Run-Scenario 'alt-f4' $false 'alt-f4' '正在退出软件'; Start-Sleep -Seconds 1
$scenarios += Run-Scenario 'file-menu-exit' $false 'menu' '正在退出软件'
$result = [ordered]@{tested_at=[DateTimeOffset]::Now.ToString('o');source_directory=$source;scenario_root=$root;passed=@($scenarios|Where-Object result -eq 'passed').Count;failed=@($scenarios|Where-Object result -eq 'failed').Count;inconclusive=@($scenarios|Where-Object result -eq 'inconclusive').Count;scenarios=$scenarios}
$result | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $OutputPath -Encoding utf8
$result | ConvertTo-Json -Depth 8
if ($result.failed -gt 0) { exit 1 }
if ($result.inconclusive -gt 0) { exit 3 }
exit 0
