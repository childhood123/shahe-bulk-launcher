$ErrorActionPreference='Stop'
$before=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'environment-before.json') -Raw|ConvertFrom-Json
$all=@(Get-CimInstance Win32_Process)
$missing=@($before.processes|Where-Object {$_.Name -match '^(nvda|zdsr|jfw)' -and $_.ProcessId -notin $all.ProcessId})
$autoRun=(Get-ItemProperty -LiteralPath 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run' -Name SandboxLauncher -ErrorAction SilentlyContinue).SandboxLauncher
$residual=@($all|Where-Object {$_.ExecutablePath -and $_.ExecutablePath.StartsWith($PSScriptRoot+'\real-install\',[StringComparison]::OrdinalIgnoreCase)}|Select-Object ProcessId,Name,ExecutablePath)
$iniHash=if(Test-Path -LiteralPath $before.iniPath){(Get-FileHash -LiteralPath $before.iniPath).Hash}else{$null}
$result=[ordered]@{returnCode=0;iniUnchanged=$iniHash -eq $before.iniHash;readerPidsPreserved=$missing.Count -eq 0;autoRunUnchanged=$autoRun -eq $before.autoRun;residualProcesses=$residual}
if(-not $result.iniUnchanged -or -not $result.readerPidsPreserved -or -not $result.autoRunUnchanged -or $residual.Count){$result.returnCode=1}
$result|ConvertTo-Json -Depth 5|Set-Content -LiteralPath (Join-Path $PSScriptRoot 'environment-after.json') -Encoding utf8
[IO.File]::AppendAllText('D:\mycodex\git\PROJECT_STRUCTURE.md',([string][char]10)+"- LOG-01 实测后环境核对：返回码 $($result.returnCode)；正式配置、自启动、读屏及隔离进程清理证据见 artifacts/runs/diagnostic_logging_release_20261023/environment-after.json。"+([string][char]10),[Text.UTF8Encoding]::new($false))
$result|ConvertTo-Json -Depth 5
exit $result.returnCode
