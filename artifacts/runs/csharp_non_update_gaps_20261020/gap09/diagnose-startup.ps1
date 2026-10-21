param(
    [Parameter(Mandatory = $true)] [string]$BuildDirectory,
    [Parameter(Mandatory = $true)] [string]$ResultPath
)
$ErrorActionPreference = 'Stop'

function New-SettingsText {
    param([int]$HistoryCount, [string]$SandboxiePath, [string]$SandboxRoot)
    $history = @(
        for ($index = 0; $index -lt $HistoryCount; $index++) {
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
        sandboxie_path = $SandboxiePath
        show_tray = $false
    }
    $lines = [System.Collections.Generic.List[string]]::new()
    [void]$lines.Add('[config]')
    [void]$lines.Add('')
    [void]$lines.Add('[settings]')
    foreach ($entry in $settings.GetEnumerator()) {
        [void]$lines.Add($entry.Key + '=' + (ConvertTo-Json -InputObject $entry.Value -Compress -Depth 8))
    }
    return [string]::Join("`r`n", $lines) + "`r`n"
}

function Get-WindowName {
    param([int]$ProcessId)
    Add-Type -AssemblyName UIAutomationClient -ErrorAction SilentlyContinue
    Add-Type -AssemblyName UIAutomationTypes -ErrorAction SilentlyContinue
    $root = [System.Windows.Automation.AutomationElement]::RootElement
    $windows = $root.FindAll(
        [System.Windows.Automation.TreeScope]::Children,
        [System.Windows.Automation.Condition]::TrueCondition)
    foreach ($window in $windows) {
        try {
            if ($window.Current.ProcessId -eq $ProcessId) { return $window.Current.Name }
        }
        catch { }
    }
    return ''
}

$resolvedBuild = (Resolve-Path -LiteralPath $BuildDirectory).Path
$source = Join-Path $resolvedBuild '沙河批量打开器.exe'
$outputRoot = Join-Path (Split-Path -Parent (Join-Path (Split-Path -Parent $ResultPath) 'x')) ('startup-matrix-' + (Get-Date -Format 'yyyyMMdd_HHmmss_fff'))
$null = New-Item -ItemType Directory -Path $outputRoot -Force
$rows = [System.Collections.ArrayList]::new()
$caught = $null
try {
    foreach ($historyCount in @(0, 1, 10, 20, 21, 100, 999)) {
        foreach ($useFakeSandboxie in @($false, $true)) {
            $name = ('history-{0}-fake-{1}' -f $historyCount, [int]$useFakeSandboxie)
            $dir = Join-Path $outputRoot $name
            $null = New-Item -ItemType Directory -Path $dir -Force
            Copy-Item -LiteralPath $resolvedBuild -Destination (Join-Path $dir 'app') -Recurse -Force
            $appDir = Join-Path $dir 'app'
            $configDir = Join-Path $appDir 'Configuration'
            $null = New-Item -ItemType Directory -Path $configDir -Force
            $sandboxRoot = Join-Path $dir 'sandbox-root'
            $null = New-Item -ItemType Directory -Path $sandboxRoot -Force
            $sandboxiePath = ''
            if ($useFakeSandboxie) {
                $sandboxiePath = Join-Path $dir 'fake-sandboxie'
                $null = New-Item -ItemType Directory -Path $sandboxiePath -Force
                [System.IO.File]::WriteAllBytes((Join-Path $sandboxiePath 'Start.exe'), [byte[]]@())
            }
            $configPath = Join-Path $configDir 'shahe_config.ini'
            [System.IO.File]::WriteAllText(
                $configPath,
                (New-SettingsText $historyCount $sandboxiePath $sandboxRoot),
                [System.Text.UTF8Encoding]::new($false))
            $logBefore = Join-Path $appDir 'log'
            $proc = Start-Process -FilePath (Join-Path $appDir '沙河批量打开器.exe') -WorkingDirectory $appDir -PassThru
            $deadline = [DateTime]::UtcNow.AddSeconds(6)
            $windowName = ''
            while ([DateTime]::UtcNow -lt $deadline) {
                if ($proc.HasExited) { break }
                $windowName = Get-WindowName $proc.Id
                if ($windowName.Length -ne 0) { break }
                Start-Sleep -Milliseconds 100
            }
            $proc.Refresh()
            $info = Get-CimInstance Win32_Process -Filter "ProcessId = $($proc.Id)"
            if ($null -ne $info) {
                $full = [IO.Path]::GetFullPath($info.ExecutablePath)
                if (-not $full.StartsWith($dir + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
                    throw "拒绝清理非本轮进程：$full"
                }
                Stop-Process -Id $proc.Id -Force
                $null = $proc.WaitForExit(3000)
            }
            $logTail = @()
            if (Test-Path -LiteralPath $logBefore) {
                $logFile = Get-ChildItem -LiteralPath $logBefore -File | Sort-Object LastWriteTime | Select-Object -Last 1
                if ($null -ne $logFile) { $logTail = @(Get-Content -LiteralPath $logFile.FullName -Tail 12) }
            }
            [void]$rows.Add([ordered]@{
                historyCount = $historyCount
                fakeSandboxie = $useFakeSandboxie
                directory = $dir
                pid = $proc.Id
                windowName = $windowName
                mainWindowTitle = $proc.MainWindowTitle
                exited = $proc.HasExited
                exitCode = if ($proc.HasExited) { $proc.ExitCode } else { $null }
                logTail = $logTail
            })
        }
    }
}
catch {
    $caught = $_.Exception.ToString()
}
$result = [ordered]@{
    timestamp = (Get-Date).ToString('o')
    success = $null -eq $caught
    buildDirectory = $resolvedBuild
    outputRoot = $outputRoot
    rows = $rows
    error = $caught
}
$json = ConvertTo-Json -InputObject $result -Depth 12
[System.IO.File]::WriteAllText($ResultPath, $json, [System.Text.UTF8Encoding]::new($false))
Write-Output $json
if ($null -ne $caught) { exit 1 }
exit 0
