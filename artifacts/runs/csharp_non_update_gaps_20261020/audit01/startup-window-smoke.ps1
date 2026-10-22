param([string]$SourceDirectory, [string]$OutputDirectory)
$ErrorActionPreference = 'Stop'
if (-not ('Audit01.NativeMethods' -as [type])) {
    Add-Type @'
using System;
using System.Runtime.InteropServices;
namespace Audit01 { public static class NativeMethods {
    [DllImport("user32.dll", SetLastError=true)] public static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
} }
'@
}
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$target = Join-Path $OutputDirectory 'app'
$outputFull = [IO.Path]::GetFullPath($OutputDirectory).TrimEnd('\')
$targetFull = [IO.Path]::GetFullPath($target).TrimEnd('\')
if ($targetFull -eq $outputFull -or -not $targetFull.StartsWith($outputFull + '\', [StringComparison]::OrdinalIgnoreCase)) {
    throw "Refusing cleanup outside audit output: $targetFull"
}
if (Test-Path $targetFull) { Remove-Item -LiteralPath $targetFull -Recurse -Force }
Copy-Item -LiteralPath $SourceDirectory -Destination $target -Recurse -Force
$config = Join-Path $target 'Configuration\shahe_config.ini'
New-Item -ItemType Directory -Force -Path (Split-Path $config) | Out-Null
$configText = if (Test-Path $config) { [IO.File]::ReadAllText($config) } else { "[settings]`r`n" }
$configText = $configText.Replace('show_tray=true', 'show_tray=false')
if ($configText -notmatch '(?m)^show_tray=') { $configText += "show_tray=false`r`n" }
$fakeSandboxie = Join-Path $target 'audit-sandboxie'
New-Item -ItemType Directory -Force -Path $fakeSandboxie | Out-Null
Set-Content -LiteralPath (Join-Path $fakeSandboxie 'Start.exe') -Value '' -Encoding ascii
$fakeJsonPath = $fakeSandboxie.Replace('\', '\\')
if ($configText -match '(?m)^sandboxie_path=') {
    $configText = [regex]::Replace($configText, '(?m)^sandboxie_path=.*$', 'sandboxie_path="' + $fakeJsonPath + '"')
} else { $configText += ('sandboxie_path="' + $fakeJsonPath + '"' + "`r`n") }
[IO.File]::WriteAllText($config, $configText, [Text.UTF8Encoding]::new($false))
$exe = Join-Path $target '沙河批量打开器.exe'
$started = [DateTimeOffset]::Now
$process = [Diagnostics.Process]::Start((New-Object Diagnostics.ProcessStartInfo -Property @{ FileName=$exe; WorkingDirectory=$target; UseShellExecute=$true }))
$record = [ordered]@{ started=$started.ToString('o'); pid=$process.Id; executable=$exe; mainWindowHandle=''; title=''; startupMs=$null; closeMethod=''; closeMainWindow=$false; exited=$false; exitCode=$null; status='inconclusive'; error='' }
try {
    $deadline = [DateTime]::UtcNow.AddSeconds(20)
    do {
        Start-Sleep -Milliseconds 200
        $process.Refresh()
    } while ($process.MainWindowHandle -eq [IntPtr]::Zero -and [DateTime]::UtcNow -lt $deadline -and -not $process.HasExited)
    if ($process.MainWindowHandle -eq [IntPtr]::Zero) { throw 'Main window handle was not observed.' }
    $record.mainWindowHandle = ('0x{0:X}' -f $process.MainWindowHandle.ToInt64())
    $record.title = $process.MainWindowTitle
    $record.startupMs = ([DateTimeOffset]::Now - $started).TotalMilliseconds
    $readyDeadline = [DateTime]::UtcNow.AddSeconds(20)
    $ready = $false
    do {
        $logFiles = @(Get-ChildItem (Join-Path $target 'log') -Filter '*.log' -File -ErrorAction SilentlyContinue)
        $logText = ($logFiles | ForEach-Object { Get-Content $_.FullName -Raw -ErrorAction SilentlyContinue }) -join "`n"
        $ready = $logText -match '已扫描到 Sandboxie 安装位置|未找到 Sandboxie 安装路径|状态: 就绪|状态: 已刷新 .*沙盒用户'
        if (-not $ready) { Start-Sleep -Milliseconds 250; $process.Refresh() }
    } while (-not $ready -and [DateTime]::UtcNow -lt $readyDeadline -and -not $process.HasExited)
    if (-not $ready) { throw 'Startup scan completion was not observed.' }
    Start-Sleep -Seconds 2
    $process.Refresh()
    if ($process.HasExited) { throw 'Process exited before the stable-window close step.' }
    $record.closeMainWindow = $process.CloseMainWindow()
    $record.closeMethod = 'Process.CloseMainWindow'
    if (-not $record.closeMainWindow) {
        $record.closeMainWindow = [Audit01.NativeMethods]::PostMessage($process.MainWindowHandle, 0x0010, [IntPtr]::Zero, [IntPtr]::Zero)
        $record.closeMethod = 'WM_CLOSE'
    }
    $record.exited = $process.WaitForExit(5000)
    if (-not $record.exited) {
        $process.Refresh()
        $record.closeMainWindow = [Audit01.NativeMethods]::PostMessage($process.MainWindowHandle, 0x0010, [IntPtr]::Zero, [IntPtr]::Zero)
        $record.closeMethod = $record.closeMethod + '+WM_CLOSE'
        $record.exited = $process.WaitForExit(10000)
    }
    if ($record.exited) { $record.exitCode = $process.ExitCode; $record.status = if ($record.exitCode -eq 0) { 'passed' } else { 'failed' } }
    else { throw 'Main window did not exit after CloseMainWindow.' }
} catch { $record.error = $_.Exception.Message }
finally {
    try { if (-not $process.HasExited) { $process.Kill(); $process.WaitForExit(5000) } } catch {}
}
$record | ConvertTo-Json -Depth 6 | Set-Content (Join-Path $OutputDirectory 'startup-window-smoke.json') -Encoding utf8
$record | ConvertTo-Json -Depth 6
if ($record.status -eq 'passed') { exit 0 } else { exit 3 }
