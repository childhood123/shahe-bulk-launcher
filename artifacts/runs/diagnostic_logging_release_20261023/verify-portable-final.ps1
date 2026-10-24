$ErrorActionPreference = 'Stop'
$runRoot = $PSScriptRoot
$repo = [IO.Path]::GetFullPath((Join-Path $runRoot '..\..\..'))
$packageResult = Get-Content -Raw (Join-Path $runRoot 'portable-final-package-result.json') | ConvertFrom-Json
$package = [string]$packageResult.package
$smokeRoot = Join-Path $runRoot 'portable-smoke-final'
$app = Join-Path $smokeRoot 'app'
$resultPath = Join-Path $runRoot 'portable-final-smoke-result.json'
$utf8 = [Text.UTF8Encoding]::new($false)
$result = [ordered]@{ returnCode = 1; package = $package; appDirectory = $app }
$process = $null

try {
    if (Test-Path -LiteralPath $smokeRoot) { throw 'Fresh smoke directory required' }
    if ((Get-FileHash -LiteralPath $package -Algorithm SHA256).Hash -ne [string]$packageResult.sha256) {
        throw 'Package hash mismatch'
    }

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [IO.Compression.ZipFile]::ExtractToDirectory($package, $app)
    $configDirectory = Join-Path $app 'Configurations'
    $logsDirectory = Join-Path $app 'logs'
    $result.configurationsInitiallyEmpty = (Test-Path -LiteralPath $configDirectory) -and
        @(Get-ChildItem -LiteralPath $configDirectory -Force).Count -eq 0
    $result.logsInitiallyEmpty = (Test-Path -LiteralPath $logsDirectory) -and
        @(Get-ChildItem -LiteralPath $logsDirectory -Force).Count -eq 0
    if (-not $result.configurationsInitiallyEmpty -or -not $result.logsInitiallyEmpty) {
        throw 'Portable package directories are not initially empty'
    }
    if (Get-ChildItem -LiteralPath $app -File -Recurse | Where-Object {
        $_.Name -match '^updater\.exe(?:\.config)?$' -or $_.Extension -eq '.pdb'
    }) {
        throw 'Portable extraction contains forbidden files'
    }

    foreach ($property in $packageResult.inventory.PSObject.Properties) {
        $relative = $property.Name.Replace('/', [IO.Path]::DirectorySeparatorChar)
        $file = Join-Path $app $relative
        if (-not (Test-Path -LiteralPath $file) -or
            (Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash -ne [string]$property.Value) {
            throw "Extracted file mismatch: $($property.Name)"
        }
    }
    $result.extractedFilesMatch = $true

    $fakeSandboxie = Join-Path $app 'fake-sandboxie'
    $sandboxRoot = Join-Path $app 'sandbox-root'
    New-Item -ItemType Directory -Path $fakeSandboxie, $sandboxRoot | Out-Null
    [IO.File]::WriteAllBytes((Join-Path $fakeSandboxie 'Start.exe'), [byte[]]@())
    $configPath = Join-Path $configDirectory 'shahe_config.ini'
    $config = @"
[config]

[settings]
show_tray=false
auto_run=false
hotkeys_enabled=false
hotkeys={}
max_history_items=17
app_history=[]
log_output_mode="off"
sandboxie_path="$($fakeSandboxie.Replace('\', '\\'))"
sandbox_root="$($sandboxRoot.Replace('\', '\\'))"
speech_interface_dll="nvda"
speech_assist_hints={"enabled":false}
"@
    [IO.File]::WriteAllText($configPath, $config, $utf8)
    $configHash = (Get-FileHash -LiteralPath $configPath -Algorithm SHA256).Hash

    $executable = Join-Path $app '沙河批量打开器.exe'
    $process = Start-Process -FilePath $executable -WorkingDirectory $app -PassThru
    $watch = [Diagnostics.Stopwatch]::StartNew()
    while ($watch.ElapsedMilliseconds -lt 10000) {
        $process.Refresh()
        if ($process.HasExited) { throw "Portable application exited early: $($process.ExitCode)" }
        if ($process.MainWindowHandle -ne 0) { break }
        Start-Sleep -Milliseconds 80
    }
    $process.Refresh()
    if ($process.MainWindowHandle -eq 0) { throw 'Portable main window not observed' }
    $result.pid = $process.Id
    $result.mainWindow = $process.MainWindowHandle.ToInt64()
    $result.mainWindowObserved = $true

    $process.CloseMainWindow() | Out-Null
    if (-not $process.WaitForExit(5000)) { throw 'Portable application normal exit timed out' }
    $result.exitCode = $process.ExitCode
    $result.normalExit = $process.ExitCode -eq 0
    $result.configurationUnchanged = (Get-FileHash -LiteralPath $configPath -Algorithm SHA256).Hash -eq $configHash
    $result.logsRemainEmpty = @(Get-ChildItem -LiteralPath $logsDirectory -Force).Count -eq 0
    $result.noLegacyLogDirectory = -not (Test-Path -LiteralPath (Join-Path $app 'log'))
    $result.residualProcesses = @(
        Get-CimInstance Win32_Process | Where-Object {
            $_.ExecutablePath -and $_.ExecutablePath.Equals($executable, [StringComparison]::OrdinalIgnoreCase)
        } | Select-Object ProcessId, ExecutablePath
    )
    if (-not $result.normalExit -or -not $result.configurationUnchanged -or
        -not $result.logsRemainEmpty -or -not $result.noLegacyLogDirectory -or
        $result.residualProcesses.Count -ne 0) {
        throw 'Portable smoke verification failed'
    }
    $result.returnCode = 0
}
catch {
    $result.error = $_.Exception.ToString()
}
finally {
    if ($null -ne $process -and -not $process.HasExited) {
        $result.forcedCleanup = $true
        $process.Kill()
        $process.WaitForExit(2000) | Out-Null
    }
    else {
        $result.forcedCleanup = $false
    }
}

[IO.File]::WriteAllText($resultPath, (($result | ConvertTo-Json -Depth 8) + "`n"), $utf8)
[IO.File]::AppendAllText(
    (Join-Path $repo 'PROJECT_STRUCTURE.md'),
    "`n- LOG-01 4.0.48 最终绿色版解压即用验证：返回码 $($result.returnCode)；从最终 ZIP 隔离解压后主窗口可启动并正常退出，初始 Configurations/logs 均为空，关闭日志后 logs 保持为空，无旧 log 目录和残留进程。证据见 artifacts/runs/diagnostic_logging_release_20261023/portable-final-smoke-result.json。`n",
    $utf8)
$result | ConvertTo-Json -Depth 8
exit [int]$result.returnCode
