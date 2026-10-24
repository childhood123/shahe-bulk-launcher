param([Parameter(Mandatory=$true)][string]$Executable)
$ErrorActionPreference='Stop'
[Console]::OutputEncoding=New-Object Text.UTF8Encoding($false)
$expected=[IO.Path]::GetFullPath($Executable)
if(-not $expected.StartsWith([IO.Path]::GetFullPath($PSScriptRoot+'\real-install\'),[StringComparison]::OrdinalIgnoreCase)){throw 'Unexpected target'}
$watch=[Diagnostics.Stopwatch]::StartNew()
$target=$null
while($watch.ElapsedMilliseconds -lt 10000){
$candidate=Get-CimInstance Win32_Process | Where-Object {$_.ExecutablePath -and $_.ExecutablePath -eq $expected} | Select-Object -First 1
if($candidate){$target=Get-Process -Id $candidate.ProcessId;$target.Refresh();if($target.MainWindowHandle -ne 0){break}}
Start-Sleep -Milliseconds 80
}
$forced=$false
try {
if($null -eq $target -or $target.MainWindowHandle -eq 0){throw 'Restarted main window not observed'}
$processHandle=$target.Handle
$handle=$target.MainWindowHandle.ToInt64()
$target.CloseMainWindow()|Out-Null
if(-not $target.WaitForExit(5000)){throw 'Normal main exit timed out'}
[pscustomobject]@{pid=$target.Id;window=$handle;exitCode=$target.ExitCode;forcedCleanup=$false;path=$expected}|ConvertTo-Json -Compress
} finally {
if($null -ne $target -and -not $target.HasExited){$target.Kill();$target.WaitForExit(2000)|Out-Null}
}
