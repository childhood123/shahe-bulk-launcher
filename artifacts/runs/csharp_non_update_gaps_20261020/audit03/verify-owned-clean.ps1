$ErrorActionPreference = 'Stop'
$runRoot = $PSScriptRoot
$baselinePath = Join-Path $runRoot 'auditrunner-preflight-20261022_110251_312-result.json'
$statePath = Join-Path $runRoot 'real-lifecycle-state2.json'
$baseline = Get-Content -LiteralPath $baselinePath -Raw | ConvertFrom-Json
$state = Get-Content -LiteralPath $statePath -Raw | ConvertFrom-Json
$runner = Join-Path $runRoot 'bin\x64\Release\net48\ShaheLauncher.AuditRunner.exe'
$arguments = @('verify-clean','--baseline',$baselinePath,'--state',$statePath,'--prefix',$state.Prefix,
    '--target',$baseline.TargetPath,'--config',$baseline.ConfigPath,'--sandboxie-ini',$baseline.SandboxieIniPath,
    '--expected-target-sha256',$baseline.TargetSha256,'--expected-config-sha256',$baseline.ConfigSha256,
    '--expected-ini-sha256',$baseline.SandboxieIniSha256,'--output',$runRoot)
$log = Join-Path $runRoot 'review-real-verify-clean.log'
& $runner @arguments *> $log
$code = $LASTEXITCODE
$reportPath = (Get-Content -LiteralPath $log | Select-Object -Last 1).Trim()
$report = Get-Content -LiteralPath $reportPath -Raw | ConvertFrom-Json
$result = [ordered]@{
    command = @($runner) + $arguments
    returnCode = $code
    status = $report.Status
    report = $reportPath
    checks = $report.Checks
    processCounts = $report.ProcessCounts
    auditSectionCount = @($report.ShaheAuditSections).Count
    auditDirectoryCount = @($report.ShaheAuditDirectories).Count
    ownedClaims = $state.OwnershipClaims
    errors = $report.Errors
}
$resultPath = Join-Path $runRoot 'review-real-verify-clean-result.json'
$result | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $resultPath -Encoding utf8
$entry = '- AUDIT-03 修正后真实只读复查：执行 audit03/verify-owned-clean.ps1，返回码 ' + $code + '，状态 ' + $report.Status + '；精确所有权检查 ownedResources=' + $report.Checks.ownedResources + '。完整参数和只含本轮资源的摘要见 artifacts/runs/csharp_non_update_gaps_20261020/audit03/review-real-verify-clean-result.json；含用户资源名称的原始报告只保留本地。'
Add-Content -LiteralPath (Join-Path (Get-Location) 'PROJECT_STRUCTURE.md') -Value ([Environment]::NewLine + $entry)
$result | ConvertTo-Json -Depth 8
