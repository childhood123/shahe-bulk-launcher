$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false
$workspace = (Get-Location).Path
$runRoot = $PSScriptRoot
$snapshot = Join-Path $runRoot '_final_index_snapshot'
$buildOutput = Join-Path $runRoot '_final_index_bin'
if ((Test-Path -LiteralPath $snapshot) -or (Test-Path -LiteralPath $buildOutput)) { throw 'Final verification directories already exist.' }
New-Item -ItemType Directory -Path $snapshot | Out-Null
& git checkout-index --all --force ('--prefix=' + $snapshot + '\')
if ($LASTEXITCODE -ne 0) { throw 'Failed to export Git index.' }
function Invoke-Checked {
    param([string]$Name, [string]$Exe, [string[]]$Arguments)
    $log = Join-Path $runRoot ($Name + '.log')
    & $Exe @Arguments *> $log
    $code = $LASTEXITCODE
    $result = [ordered]@{ command = @($Exe) + $Arguments; returnCode = $code; log = $log; passed = $null; failed = $null; skipped = $null }
    $trx = Join-Path $runRoot ($Name + '.trx')
    if (Test-Path -LiteralPath $trx) {
        [xml]$xml = Get-Content -LiteralPath $trx
        $counter = $xml.TestRun.ResultSummary.Counters
        $result.passed = [int]$counter.passed
        $result.failed = [int]$counter.failed
        $result.skipped = [int]$counter.notExecuted
        $result.trx = $trx
    }
    $text = Get-Content -LiteralPath $log -Raw
    if ($Name -eq 'clean-final-python' -and $code -eq 0 -and $text -match 'Ran (\d+) tests') {
        $result.passed = [int]$Matches[1]; $result.failed = 0; $result.skipped = 0
    }
    if ($Name -eq 'clean-final-release') {
        if ($text -match '(\d+) 个警告') { $result.warnings = [int]$Matches[1] }
        if ($text -match '(\d+) 个错误') { $result.errors = [int]$Matches[1] }
    }
    $result | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $runRoot ($Name + '-result.json')) -Encoding utf8
    $counts = if ($null -eq $result.passed) { '测试计数不适用' } else { '通过 ' + $result.passed + '、失败 ' + $result.failed + '、跳过 ' + $result.skipped }
    $entry = '- AUDIT-03 ' + $Name + '（干净索引快照）：返回码 ' + $code + '，' + $counts + '。实际命令、日志路径与结果见 artifacts/runs/csharp_non_update_gaps_20261020/audit03/' + $Name + '-result.json 及同名日志；C# 测试另有 TRX。'
    Add-Content -LiteralPath (Join-Path $workspace 'PROJECT_STRUCTURE.md') -Value ([Environment]::NewLine + $entry)
    $result | ConvertTo-Json -Depth 6
    if ($code -ne 0) { throw ('Verification failed: ' + $Name) }
}
$solution = Join-Path $snapshot 'csharp\ShaheBulkLauncher.sln'
Invoke-Checked 'clean-final-restore' 'dotnet' @('restore',$solution,'--locked-mode')
Invoke-Checked 'clean-final-full' 'dotnet' @('test',$solution,'--configuration','Release','--no-restore','--property:Platform=x64',('--property:BaseOutputPath=' + $buildOutput + '\'),'--logger','trx;LogFileName=clean-final-full.trx','--results-directory',$runRoot)
Push-Location $snapshot
try {
    Invoke-Checked 'clean-final-python' 'python' @('-m','unittest','discover','-s','tests','-p','test_*.py','-v')
    Invoke-Checked 'clean-final-compile' 'python' @('-m','compileall','-q','shahe_bulk_launcher','tests')
} finally { Pop-Location }
Invoke-Checked 'clean-final-release' 'dotnet' @('build',$solution,'--configuration','Release','--no-restore','--property:Platform=x64',('--property:BaseOutputPath=' + $buildOutput + '\'))
