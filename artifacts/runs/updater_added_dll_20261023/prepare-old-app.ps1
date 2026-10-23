$ErrorActionPreference = 'Stop'
$runPath = $PSScriptRoot
$repoRoot = Split-Path (Split-Path (Split-Path $runPath -Parent) -Parent) -Parent
$oldArchive = Join-Path $repoRoot 'artifacts\runs\csharp_gitcode_release_20261020\gitcode-anonymous-package-final.zip'
$oldApp = Join-Path $runPath 'old-app'
if (Test-Path -LiteralPath $oldApp) { throw 'Old fixture exists; preserve evidence and use a new scenario.' }
if ((Get-FileHash -LiteralPath $oldArchive -Algorithm SHA256).Hash -ne 'D6F248D7F8602C2235B5FB5318E592AA5F14E213069A095396D470E7DCC266F8') { throw 'Old official package hash mismatch.' }
Expand-Archive -LiteralPath $oldArchive -DestinationPath $oldApp
New-Item -ItemType Directory -Path (Join-Path $oldApp 'Configuration'), (Join-Path $oldApp 'fake-sandboxie'), (Join-Path $oldApp 'sandbox-root'), (Join-Path $runPath 'fixture-omitted') | Out-Null
Copy-Item -LiteralPath (Join-Path $repoRoot 'artifacts\runs\hotkey_enter_action_20261023\app\fake-sandboxie\Start.exe') -Destination (Join-Path $oldApp 'fake-sandboxie\Start.exe')
$omittedSource = [IO.Path]::GetFullPath((Join-Path $oldApp 'lib\byctrl-x64.dll'))
$omittedTarget = [IO.Path]::GetFullPath((Join-Path $runPath 'fixture-omitted\byctrl-x64.dll'))
$allowedRoot = [IO.Path]::GetFullPath($runPath) + [IO.Path]::DirectorySeparatorChar
if (-not $omittedSource.StartsWith($allowedRoot, [StringComparison]::OrdinalIgnoreCase) -or -not $omittedTarget.StartsWith($allowedRoot, [StringComparison]::OrdinalIgnoreCase)) { throw 'Omitted DLL path outside scenario.' }
Move-Item -LiteralPath $omittedSource -Destination $omittedTarget
$settings = [ordered]@{
 max_history_items = 17
 hide_path = $true
 show_tray = $false
 auto_run = $false
 hotkeys_enabled = $false
 hotkeys = @{}
 app_history = @(@{ path = 'D:\Audit04\程序一.exe'; note = 'GitCode 升级保留记录' })
 app_path = 'D:\Audit04\程序一.exe'
 sandbox_root = (Join-Path $oldApp 'sandbox-root')
 sandboxie_path = (Join-Path $oldApp 'fake-sandboxie')
 speech_interface_dll = 'nvda'
 speech_assist_hints = @{ enabled = $false }
}
$lines = @('[config]', '', '[settings]')
foreach ($key in $settings.Keys) { $lines += $key + '=' + (ConvertTo-Json -InputObject $settings[$key] -Compress -Depth 6) }
$configPath = Join-Path $oldApp 'Configuration\shahe_config.ini'
[IO.File]::WriteAllText($configPath, ($lines -join [Environment]::NewLine) + [Environment]::NewLine, [Text.UTF8Encoding]::new($false))
$readers = @(Get-Process | Where-Object { $_.ProcessName -match '^(nvda|ZDSR|BoyCtrl|baoyi)' } | Select-Object -ExpandProperty Id)
$result = [ordered]@{ returnCode = 0; app = (Join-Path $oldApp '沙河批量打开器.exe'); sourcePackage = $oldArchive; oldConfigurationHash = (Get-FileHash -LiteralPath $configPath -Algorithm SHA256).Hash; missingDonation = (-not (Test-Path -LiteralPath (Join-Path $oldApp '打赏'))); missingOptionalDll = (-not (Test-Path -LiteralPath $omittedSource)); readerPids = $readers; realSandboxieIniPath = 'C:\Windows\Sandboxie.ini'; realSandboxieIniHash = (Get-FileHash -LiteralPath 'C:\Windows\Sandboxie.ini' -Algorithm SHA256).Hash }
$result | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $runPath 'old-app-baseline.json') -Encoding utf8
[IO.File]::AppendAllText((Join-Path $repoRoot 'PROJECT_STRUCTURE.md'), ([string][char]10 + '- UPDATE-DLL-01 旧版隔离夹具准备返回码 0；来源为既有已校验正式包，旧配置位于 Configuration，未带打赏图片并模拟缺少一个可选 DLL。初始哈希、路径、读屏进程及正式 Sandboxie.ini 基线见 artifacts/runs/updater_added_dll_20261023/old-app-baseline.json。' + [char]10), [Text.UTF8Encoding]::new($false))
$result | ConvertTo-Json -Depth 5
