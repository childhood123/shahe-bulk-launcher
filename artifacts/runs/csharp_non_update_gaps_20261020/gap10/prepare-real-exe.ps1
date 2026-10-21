param([Parameter(Mandatory=$true)][string]$BuildDirectory)
$ErrorActionPreference = 'Stop'
$build = (Resolve-Path -LiteralPath $BuildDirectory).Path
$run = Join-Path $PSScriptRoot ('real-exe-' + (Get-Date -Format 'yyyyMMdd_HHmmss_fff'))
$app = Join-Path $run 'app'
$null = New-Item -ItemType Directory -Path $app -Force
Copy-Item -Path (Join-Path $build '*') -Destination $app -Recurse
$configuration = Join-Path $app 'Configuration'
$sandbox = Join-Path $app 'fake-sandboxie'
$sandboxRoot = Join-Path $app 'sandbox-root'
$null = New-Item -ItemType Directory -Path $configuration,$sandbox,$sandboxRoot -Force
[IO.File]::WriteAllBytes((Join-Path $sandbox 'Start.exe'), [byte[]]@())
$fixture = Join-Path $run 'Gap10-selected.exe'
[IO.File]::WriteAllBytes($fixture, [byte[]]@(77,90))
$settings = [ordered]@{
    app_history = @(); max_history_items = 999; auto_run = $false; show_tray = $false
    hotkeys_enabled = $false; hide_path = $false; sandbox_root = $sandboxRoot
    sandboxie_path = $sandbox; speech_assist_hints = @{ enabled = $false }
}
$lines = @('[config]','','[settings]')
foreach ($entry in $settings.GetEnumerator()) { $lines += $entry.Key + '=' + (ConvertTo-Json -InputObject $entry.Value -Compress -Depth 6) }
[IO.File]::WriteAllText((Join-Path $configuration 'shahe_config.ini'), (($lines -join "`r`n") + "`r`n"), [Text.UTF8Encoding]::new($false))
$executable = Join-Path $app '沙河批量打开器.exe'
$guard = Join-Path $PSScriptRoot 'preserve-test-environment.ps1'
$arguments = '-NoProfile -STA -ExecutionPolicy Bypass -File "' + $guard + '" -RunDirectory "' + $run + '" -FixturePath "' + $fixture + '"'
$helper = Start-Process -FilePath "$env:WINDIR\System32\WindowsPowerShell\v1.0\powershell.exe" -ArgumentList $arguments -WindowStyle Hidden -PassThru
$info = [ordered]@{ runDirectory = $run; buildDirectory = $build; executable = $executable; sha256 = (Get-FileHash $executable -Algorithm SHA256).Hash; fixturePath = $fixture; guardPid = $helper.Id }
$info | ConvertTo-Json | Set-Content (Join-Path $PSScriptRoot 'real-exe-info.json') -Encoding UTF8
$info | ConvertTo-Json
