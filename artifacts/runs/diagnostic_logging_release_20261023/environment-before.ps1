$ErrorActionPreference='Stop'
$runRoot=$PSScriptRoot
$processes=@(Get-CimInstance Win32_Process | Where-Object { $_.Name -eq '沙河批量打开器.exe' -or $_.Name -eq 'updater.exe' -or $_.Name -match '^(nvda|zdsr|jfw)' } | Select-Object ProcessId,Name,ExecutablePath)
$key='HKCU:\Software\Microsoft\Windows\CurrentVersion\Run'
$autoRun=Get-ItemProperty -LiteralPath $key -Name SandboxLauncher -ErrorAction SilentlyContinue
$ini='C:\Windows\Sandboxie.ini'
$result=[ordered]@{returnCode=0;processes=$processes;autoRun=$autoRun.SandboxLauncher;iniPath=$ini;iniHash=if(Test-Path -LiteralPath $ini){(Get-FileHash -LiteralPath $ini).Hash}else{$null}}
$result|ConvertTo-Json -Depth 5|Set-Content -LiteralPath (Join-Path $runRoot 'environment-before.json') -Encoding utf8
[IO.File]::AppendAllText('D:\mycodex\git\PROJECT_STRUCTURE.md', ([string][char]10)+'- LOG-01 环境只读基线：返回码 0；主程序占用、读屏进程、正式 Sandboxie 配置及自启动值见 artifacts/runs/diagnostic_logging_release_20261023/environment-before.json；未停止或改动任何用户进程。'+([string][char]10), [Text.UTF8Encoding]::new($false))
$result|ConvertTo-Json -Depth 5
