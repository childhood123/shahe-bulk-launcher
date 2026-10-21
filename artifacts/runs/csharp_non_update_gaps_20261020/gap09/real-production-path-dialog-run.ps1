param(
    [Parameter(Mandatory = $true)] [string]$BuildDirectory,
    [Parameter(Mandatory = $true)] [string]$ProbeExecutable,
    [Parameter(Mandatory = $true)] [string]$ResultPath
)
$ErrorActionPreference = 'Stop'

$build = (Resolve-Path -LiteralPath $BuildDirectory).Path
$probe = (Resolve-Path -LiteralPath $ProbeExecutable).Path
$resultFile = [IO.Path]::GetFullPath($ResultPath)
$runRoot = Join-Path (Split-Path -Parent $resultFile) ('production-path-dialog-' + (Get-Date -Format 'yyyyMMdd_HHmmss_fff'))
$null = New-Item -ItemType Directory -Path $runRoot -Force
$rows = [System.Collections.ArrayList]::new()
$caught = $null

try {
    foreach ($target in @(0, 21, 999)) {
        $scenario = Join-Path $runRoot ('history-' + $target)
        $null = New-Item -ItemType Directory -Path $scenario -Force
        Copy-Item -Path (Join-Path $build '*') -Destination $scenario -Recurse -Force
        Copy-Item -LiteralPath $probe -Destination $scenario -Force
        $exe = Join-Path $scenario '沙河批量打开器.exe'
        $harness = Join-Path $scenario ([IO.Path]::GetFileName($probe))
        $saveLog = Join-Path $scenario 'save.log'
        $verifyLog = Join-Path $scenario 'verify.log'
        $save = Start-Process -FilePath $harness -ArgumentList @($exe, 'save', $target) -WorkingDirectory $scenario -Wait -PassThru -RedirectStandardOutput $saveLog -RedirectStandardError ($saveLog + '.err')
        $saveText = (Get-Content -LiteralPath $saveLog -Raw).Trim()
        $saveJson = $saveText | ConvertFrom-Json
        $verify = Start-Process -FilePath $harness -ArgumentList @($exe, 'verify', $target) -WorkingDirectory $scenario -Wait -PassThru -RedirectStandardOutput $verifyLog -RedirectStandardError ($verifyLog + '.err')
        $verifyText = (Get-Content -LiteralPath $verifyLog -Raw).Trim()
        $verifyJson = $verifyText | ConvertFrom-Json
        [void]$rows.Add([ordered]@{
            target = $target
            directory = $scenario
            savePid = $saveJson.pid
            verifyPid = $verifyJson.pid
            saveExitCode = $save.ExitCode
            verifyExitCode = $verify.ExitCode
            save = $saveJson
            verify = $verifyJson
            passed = $save.ExitCode -eq 0 -and $verify.ExitCode -eq 0 -and $saveJson.passed -and $verifyJson.passed
            saveLog = $saveLog
            verifyLog = $verifyLog
        })
        if ($save.ExitCode -ne 0 -or $verify.ExitCode -ne 0 -or -not $saveJson.passed -or -not $verifyJson.passed) {
            throw "target $target production dialog probe failed"
        }
    }
}
catch {
    $caught = $_.Exception.ToString()
}

$result = [ordered]@{
    timestamp = (Get-Date).ToString('o')
    success = $null -eq $caught -and @($rows | Where-Object { -not $_.passed }).Count -eq 0 -and $rows.Count -eq 3
    buildDirectory = $build
    probeExecutable = $probe
    runRoot = $runRoot
    inputMethod = 'same production MainForm/PathMemoryManagerDialog assemblies in isolated STA processes; no mocked dialog'
    scenarios = $rows
    error = $caught
}
$json = ConvertTo-Json -InputObject $result -Depth 16
[IO.File]::WriteAllText($resultFile, $json, [Text.UTF8Encoding]::new($false))
Write-Output $json
if (-not $result.success) { exit 1 }
exit 0
