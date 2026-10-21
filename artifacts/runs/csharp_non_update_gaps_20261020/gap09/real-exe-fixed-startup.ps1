param(
    [Parameter(Mandatory = $true)] [string]$BuildDirectory,
    [Parameter(Mandatory = $true)] [string]$ResultPath
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes

function New-SettingsText {
    param([string]$SandboxiePath, [string]$SandboxRoot)
    $history = @(
        for ($index = 0; $index -lt 1000; $index++) {
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

function Get-WindowByProcess {
    param([int]$ProcessId, [string]$Name)
    $windows = [System.Windows.Automation.AutomationElement]::RootElement.FindAll(
        [System.Windows.Automation.TreeScope]::Children,
        [System.Windows.Automation.Condition]::TrueCondition)
    foreach ($window in $windows) {
        try {
            if ($window.Current.ProcessId -eq $ProcessId -and ($Name.Length -eq 0 -or $window.Current.Name -eq $Name)) {
                return $window
            }
        }
        catch { }
    }
    return $null
}

$build = (Resolve-Path -LiteralPath $BuildDirectory).Path
$resultFile = [IO.Path]::GetFullPath($ResultPath)
$runRoot = Join-Path (Split-Path -Parent $resultFile) ('real-exe-fixed-' + (Get-Date -Format 'yyyyMMdd_HHmmss_fff'))
$scenario = Join-Path $runRoot 'history-999'
$null = New-Item -ItemType Directory -Path $scenario -Force
Copy-Item -Path (Join-Path $build '*') -Destination $scenario -Recurse -Force
$fakeSandboxie = Join-Path $scenario 'fake-sandboxie'
$sandboxRoot = Join-Path $scenario 'sandbox-root'
$configuration = Join-Path $scenario 'Configuration'
$null = New-Item -ItemType Directory -Path $fakeSandboxie -Force
$null = New-Item -ItemType Directory -Path $sandboxRoot -Force
$null = New-Item -ItemType Directory -Path $configuration -Force
[IO.File]::WriteAllBytes((Join-Path $fakeSandboxie 'Start.exe'), [byte[]]@())
$configPath = Join-Path $configuration 'shahe_config.ini'
[IO.File]::WriteAllText($configPath, (New-SettingsText $fakeSandboxie $sandboxRoot), [Text.UTF8Encoding]::new($false))
$executable = Join-Path $scenario '沙河批量打开器.exe'
$hash = (Get-FileHash -LiteralPath $executable -Algorithm SHA256).Hash
$process = Start-Process -FilePath $executable -WorkingDirectory $scenario -PassThru
$start = Get-Date
$deadline = $start.AddSeconds(90)
$ready = $false
$window = $false
$exception = $null
$lastLog = ''
while ((Get-Date) -lt $deadline) {
    $process.Refresh()
    $window = $process.MainWindowHandle -ne [IntPtr]::Zero
    $logFile = Get-ChildItem -LiteralPath (Join-Path $scenario 'log') -File -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime | Select-Object -Last 1
    if ($null -ne $logFile) {
        $lastLog = (Get-Content -LiteralPath $logFile.FullName -Tail 20) -join "`n"
        $ready = $lastLog -match '状态: 就绪'
    }
    $exception = Get-WindowByProcess -ProcessId $process.Id -Name 'Microsoft .NET Framework'
    if ($null -ne $exception) { break }
    if ($window -and $ready) { break }
    Start-Sleep -Milliseconds 250
}

$exceptionText = ''
if ($null -ne $exception) {
    $text = $exception.FindAll(
        [System.Windows.Automation.TreeScope]::Descendants,
        [System.Windows.Automation.Condition]::TrueCondition)
    $exceptionText = (($text | ForEach-Object { try { $_.Current.Name } catch { } }) -join "`n")
}
$close = $false
$natural = $false
$forced = $false
if ($window -and $null -eq $exception) {
    $close = $process.CloseMainWindow()
    $natural = $process.WaitForExit(10000)
}
if (-not $natural) {
    $info = Get-CimInstance Win32_Process -Filter "ProcessId = $($process.Id)"
    if ($null -ne $info) {
        $full = [IO.Path]::GetFullPath($info.ExecutablePath)
        if (-not $full.StartsWith($scenario + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
            throw "拒绝清理非本轮进程：$full"
        }
        Stop-Process -Id $process.Id -Force
        $forced = $true
        $null = $process.WaitForExit(5000)
    }
}
$result = [ordered]@{
    timestamp = (Get-Date).ToString('o')
    buildDirectory = $build
    scenario = $scenario
    executable = $executable
    executableSha256 = $hash
    configPath = $configPath
    pid = $process.Id
    mainWindowHandle = ('0x{0:X}' -f $process.MainWindowHandle.ToInt64())
    windowObserved = $window
    readyObserved = $ready
    exceptionObserved = $null -ne $exception
    exceptionText = $exceptionText
    closeMainWindow = $close
    naturalExit = $natural
    forcedCleanup = $forced
    exitCode = if ($process.HasExited) { $process.ExitCode } else { $null }
    remaining = $null -ne (Get-Process -Id $process.Id -ErrorAction SilentlyContinue)
    lastLog = $lastLog
    passed = $window -and $ready -and $null -eq $exception -and $close -and $natural -and -not $forced
}
$result | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $resultFile -Encoding UTF8
Get-Content -LiteralPath $resultFile
if (-not $result.passed) { exit 1 }
exit 0
