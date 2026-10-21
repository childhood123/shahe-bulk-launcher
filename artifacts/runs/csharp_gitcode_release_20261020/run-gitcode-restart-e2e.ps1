$ErrorActionPreference = 'Stop'

$runDirectory = $PSScriptRoot
$packagePath = Join-Path $runDirectory 'gitcode-anonymous-package-final.zip'
$packageUrl = 'https://raw.gitcode.com/childhood123/shahe-bulk-launcher-releases/blobs/ab39c61fecdcd38d737ef615bb84fb2cbf055051/shahe-bulk-launcher-4.0.46-win-x64.zip'
$expectedSize = 2785203
$expectedSha256 = 'D6F248D7F8602C2235B5FB5318E592AA5F14E213069A095396D470E7DCC266F8'
$scenarioName = 'gitcode-restart-e2e-' + [DateTimeOffset]::UtcNow.ToString('yyyyMMdd-HHmmssfff')
$scenarioRoot = Join-Path $runDirectory $scenarioName
$packageDirectory = Join-Path $scenarioRoot 'package'
$appDirectory = Join-Path $scenarioRoot 'app'
$helperDirectory = Join-Path $appDirectory 'tenp\updater_helper'
$requestPath = Join-Path $appDirectory 'tenp\update_request.json'
$resultPath = Join-Path $runDirectory 'gitcode-restart-e2e-final.json'
$targetProcessId = 0
$updaterProcessId = 0
$closeMainWindowResult = $false
$forcedCleanup = $false
$result = [ordered]@{
    verified = $false
    scenario = 'GitCode anonymous package, production helper layout, restart=true'
    scenario_root = $scenarioRoot
    package_url = $packageUrl
    package_path = $packagePath
    expected_size = $expectedSize
    expected_sha256 = $expectedSha256
    helper_directory = $helperDirectory
    request_path = $requestPath
    updater_pid = $null
    updater_exit_code = $null
    target_pid = $null
    target_executable = $null
    target_main_window_handle = $null
    target_main_window_title = $null
    restart_observed = $false
    main_hash_matches = $false
    updater_hash_matches = $false
    configuration_preserved = $false
    lib_file_count = 0
    close_main_window_result = $false
    forced_cleanup = $false
    cleanup_complete = $false
    error = ''
    completed_at = $null
}

try {
    $actualSize = (Get-Item -LiteralPath $packagePath).Length
    $actualSha256 = (Get-FileHash -LiteralPath $packagePath -Algorithm SHA256).Hash
    if ($actualSize -ne $expectedSize -or $actualSha256 -ne $expectedSha256) {
        throw 'The anonymous GitCode package no longer matches the verified size or SHA-256.'
    }

    New-Item -ItemType Directory -Path $packageDirectory, $appDirectory, $helperDirectory -Force | Out-Null
    Expand-Archive -LiteralPath $packagePath -DestinationPath $packageDirectory
    $publishedMains = @(Get-ChildItem -LiteralPath $packageDirectory -Filter '*.exe' |
        Where-Object Name -ne 'updater.exe')
    if ($publishedMains.Count -ne 1) {
        throw 'The package does not contain exactly one main executable.'
    }
    $publishedMain = $publishedMains[0]

    $mainExecutable = Join-Path $appDirectory $publishedMain.Name
    Set-Content -LiteralPath $mainExecutable -Value 'old-main' -NoNewline -Encoding ascii
    Set-Content -LiteralPath (Join-Path $appDirectory 'updater.exe') -Value 'old-updater' -NoNewline -Encoding ascii
    $configurationDirectory = Join-Path $appDirectory 'Configuration'
    New-Item -ItemType Directory -Path $configurationDirectory -Force | Out-Null
    $configurationMarker = Join-Path $configurationDirectory 'user.ini'
    Set-Content -LiteralPath $configurationMarker -Value 'keep-user-data' -NoNewline -Encoding utf8

    Copy-Item -LiteralPath (Join-Path $packageDirectory 'updater.exe') -Destination $helperDirectory
    Get-ChildItem -LiteralPath $packageDirectory -Filter '*.dll' | Copy-Item -Destination $helperDirectory
    $helperUpdater = Join-Path $helperDirectory 'updater.exe'

    $request = [ordered]@{
        url = $packageUrl
        package_kind = 'full'
        current_version = '4.0.45'
        target_version = '4.0.46'
        sha256 = $expectedSha256
        size = $expectedSize
        restart = $true
        encrypted = $false
        download_workers = '4'
    }
    New-Item -ItemType Directory -Path (Split-Path -Parent $requestPath) -Force | Out-Null
    $request | ConvertTo-Json | Set-Content -LiteralPath $requestPath -Encoding utf8

    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $helperUpdater
    $startInfo.WorkingDirectory = $helperDirectory
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.WindowStyle = [System.Diagnostics.ProcessWindowStyle]::Hidden
    $startInfo.ArgumentList.Add('--app-dir')
    $startInfo.ArgumentList.Add($appDirectory)
    $startInfo.ArgumentList.Add('--exe')
    $startInfo.ArgumentList.Add($mainExecutable)
    $startInfo.ArgumentList.Add('--pid')
    $startInfo.ArgumentList.Add('0')
    $startInfo.ArgumentList.Add('--request-file')
    $startInfo.ArgumentList.Add($requestPath)
    $updaterProcess = [System.Diagnostics.Process]::Start($startInfo)
    $updaterProcessId = $updaterProcess.Id
    $result.updater_pid = $updaterProcessId
    if (-not $updaterProcess.WaitForExit(90000)) {
        $updaterProcess.Kill()
        throw 'The updater did not exit within 90 seconds.'
    }

    $result.updater_exit_code = $updaterProcess.ExitCode
    if ($updaterProcess.ExitCode -ne 0) {
        throw "The updater returned $($updaterProcess.ExitCode)."
    }

    $restartDeadline = [DateTime]::UtcNow.AddSeconds(20)
    do {
        $startedProcess = Get-CimInstance Win32_Process |
            Where-Object { $_.ExecutablePath -eq $mainExecutable } |
            Select-Object -First 1
        if ($null -eq $startedProcess) {
            Start-Sleep -Milliseconds 200
        }
    } while ($null -eq $startedProcess -and [DateTime]::UtcNow -lt $restartDeadline)

    if ($null -eq $startedProcess) {
        throw 'The updater returned success but the restarted main executable was not observed.'
    }

    $targetProcessId = [int]$startedProcess.ProcessId
    $result.target_pid = $targetProcessId
    $result.target_executable = $startedProcess.ExecutablePath
    $result.restart_observed = $true
    $targetProcess = Get-Process -Id $targetProcessId -ErrorAction Stop
    try { $null = $targetProcess.WaitForInputIdle(10000) } catch { }
    $targetProcess.Refresh()
    $result.target_main_window_handle = $targetProcess.MainWindowHandle.ToInt64()
    $result.target_main_window_title = $targetProcess.MainWindowTitle

    $publishedMainHash = (Get-FileHash -LiteralPath $publishedMain.FullName -Algorithm SHA256).Hash
    $installedMainHash = (Get-FileHash -LiteralPath $mainExecutable -Algorithm SHA256).Hash
    $publishedUpdaterHash = (Get-FileHash -LiteralPath (Join-Path $packageDirectory 'updater.exe') -Algorithm SHA256).Hash
    $installedUpdaterHash = (Get-FileHash -LiteralPath (Join-Path $appDirectory 'updater.exe') -Algorithm SHA256).Hash
    $result.main_hash_matches = $publishedMainHash -eq $installedMainHash
    $result.updater_hash_matches = $publishedUpdaterHash -eq $installedUpdaterHash
    $result.configuration_preserved = (Get-Content -LiteralPath $configurationMarker -Raw) -eq 'keep-user-data'
    $result.lib_file_count = @(Get-ChildItem -LiteralPath (Join-Path $appDirectory 'lib') -File).Count
    $result.verified = $result.restart_observed -and
        $result.main_hash_matches -and
        $result.updater_hash_matches -and
        $result.configuration_preserved -and
        $result.lib_file_count -eq 7
}
catch {
    $result.error = $_.Exception.Message
}
finally {
    if ($targetProcessId -gt 0) {
        try {
            $targetProcess = Get-Process -Id $targetProcessId -ErrorAction Stop
            $closeMainWindowResult = $targetProcess.CloseMainWindow()
            if (-not $targetProcess.WaitForExit(3000)) {
                Stop-Process -Id $targetProcessId -Force
                $forcedCleanup = $true
            }
        }
        catch {
            if ($result.error.Length -eq 0) {
                $result.error = 'Target cleanup failed: ' + $_.Exception.Message
            }
        }
    }

    if ($updaterProcessId -gt 0) {
        try {
            $remainingUpdater = Get-Process -Id $updaterProcessId -ErrorAction Stop
            Stop-Process -Id $updaterProcessId -Force
        }
        catch { }
    }

    $result.close_main_window_result = $closeMainWindowResult
    $result.forced_cleanup = $forcedCleanup
    $result.cleanup_complete = $targetProcessId -eq 0 -or $null -eq (Get-Process -Id $targetProcessId -ErrorAction SilentlyContinue)
    $result.verified = $result.verified -and $result.cleanup_complete -and $result.error.Length -eq 0
    $result.completed_at = [DateTimeOffset]::UtcNow.ToString('o')
    $result | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $resultPath -Encoding utf8
    $result | ConvertTo-Json -Depth 6
}

if (-not $result.verified) {
    exit 1
}
