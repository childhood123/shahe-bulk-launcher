param(
    [Parameter(Mandatory=$true)][string]$Phase,
    [ValidateSet('test','vstest','build','python','compile','diff')][string]$Kind = 'test',
    [string]$Filter = '',
    [string]$Project = 'csharp\ShaheBulkLauncher.sln'
)
$ErrorActionPreference = 'Stop'
$workspace = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
$prefix = Join-Path $PSScriptRoot $Phase
$bin = (Join-Path $PSScriptRoot 'bin') + '\'
$tool = 'dotnet'
switch ($Kind) {
    'test' {
        $arguments = @('test', $Project, '--configuration', 'Release', '--no-restore', '--property:Platform=x64', "--property:BaseOutputPath=$bin", '--logger', "trx;LogFileName=$Phase.trx", '--results-directory', $PSScriptRoot)
        if ($Filter) { $arguments += @('--filter', $Filter) }
    }
    'vstest' {
        $arguments = @('vstest', (Join-Path $bin 'x64\Release\net48\ShaheLauncher.Tests.dll'), '/Platform:x64', "/Logger:trx;LogFileName=$Phase.trx", "/ResultsDirectory:$PSScriptRoot")
        if ($Filter) { $arguments += "/TestCaseFilter:$Filter" }
    }
    'build' { $arguments = @('build', $Project, '--configuration', 'Release', '--no-restore', '--property:Platform=x64', "--property:BaseOutputPath=$bin") }
    'python' { $tool = 'python'; $arguments = @('-m','unittest','discover','-s','tests','-p','test_*.py','-v') }
    'compile' { $tool = 'python'; $arguments = @('-m','compileall','-q','shahe_bulk_launcher','tests') }
    'diff' { $tool = 'git'; $arguments = @('diff','--check') }
}
Push-Location $workspace
try {
    & $tool @arguments *> ($prefix + '.log')
    $code = $LASTEXITCODE
} finally { Pop-Location }
$result = [ordered]@{ command = @($tool) + $arguments; returnCode = $code; passed = $null; failed = $null; skipped = $null; log = $prefix + '.log' }
if (Test-Path -LiteralPath ($prefix + '.trx')) {
    [xml]$trx = Get-Content -LiteralPath ($prefix + '.trx') -Raw
    $counts = $trx.TestRun.ResultSummary.Counters
    $result.passed = [int]$counts.passed
    $result.failed = [int]$counts.failed
    $result.skipped = [int]$counts.notExecuted
}
if ($Kind -eq 'python') {
    $output = Get-Content -LiteralPath ($prefix + '.log') -Raw
    if ($code -eq 0 -and $output -match 'Ran (\d+) tests') {
        $result.passed = [int]$Matches[1]; $result.failed = 0; $result.skipped = 0
    }
}
$result | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath ($prefix + '-result.json') -Encoding utf8
$countsText = if ($null -eq $result.passed) { '计数不适用（未产生测试结果时不能作为测试通过）' } else { "通过 $($result.passed)、失败 $($result.failed)、跳过 $($result.skipped)" }
$entry = "`n- 2026-10-21 GitCode 更新续接 $Phase：$Kind 返回码 $code，$countsText。实际命令与日志索引：artifacts/runs/csharp_gitcode_release_20261020/$Phase-result.json；同名前缀 .log/.trx。"
Add-Content -LiteralPath (Join-Path $workspace 'PROJECT_STRUCTURE.md') -Value $entry -Encoding utf8
$result | ConvertTo-Json -Depth 6
Get-Content -LiteralPath ($prefix + '.log') -Tail 12
exit $code
