param(
    [Parameter(Mandatory=$true)][string]$Gap,
    [Parameter(Mandatory=$true)][string]$Phase,
    [Parameter(Mandatory=$true)][ValidateSet('test','vstest','python','compile','build','diff')][string]$Kind,
    [string]$Filter = ''
)
$ErrorActionPreference = 'Stop'
$workspacePath = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
$resultDirectory = Join-Path $PSScriptRoot $Gap
$null = New-Item -ItemType Directory -Path $resultDirectory -Force
$stamp = Get-Date -Format 'yyyyMMdd_HHmmss_fff'
$prefix = Join-Path $resultDirectory ($Phase + '-' + $stamp)
$binPath = (Join-Path $resultDirectory 'bin') + '\'
$toolName = 'dotnet'
switch ($Kind) {
    'test' {
        $toolArguments = @('test', 'csharp\ShaheBulkLauncher.sln', '--configuration', 'Release', '--no-restore', '--property:Platform=x64', ('--property:BaseOutputPath=' + $binPath), '--logger', ('trx;LogFileName=' + $Phase + '-' + $stamp + '.trx'), '--results-directory', $resultDirectory)
        if ($Filter) { $toolArguments += @('--filter', $Filter) }
    }
    'vstest' {
        $assemblyPath = Join-Path $binPath 'x64\Release\net48\ShaheLauncher.Tests.dll'
        $toolArguments = @('vstest', $assemblyPath, '/Platform:x64', ('/Logger:trx;LogFileName=' + $Phase + '-' + $stamp + '.trx'), ('/ResultsDirectory:' + $resultDirectory))
        if ($Filter) { $toolArguments += ('/TestCaseFilter:' + $Filter) }
    }
    'build' { $toolArguments = @('build', 'csharp\ShaheBulkLauncher.sln', '--configuration', 'Release', '--no-restore', '--property:Platform=x64', ('--property:BaseOutputPath=' + $binPath)) }
    'python' { $toolName = 'python'; $toolArguments = @('-m', 'unittest', 'discover', '-s', 'tests', '-p', 'test_*.py', '-v') }
    'compile' { $toolName = 'python'; $toolArguments = @('-m', 'compileall', '-q', 'shahe_bulk_launcher', 'tests') }
    'diff' { $toolName = 'git'; $toolArguments = @('diff', '--check') }
}
Push-Location $workspacePath
try {
    & $toolName @toolArguments *> ($prefix + '.log')
    $returnCode = $LASTEXITCODE
} finally { Pop-Location }
$result = [ordered]@{ kind=$Kind; command=(@($toolName) + $toolArguments); returnCode=$returnCode; log=($prefix + '.log'); passed=$null; failed=$null; skipped=$null }
if ($Kind -in @('test', 'vstest')) {
    $trxPath = Join-Path $resultDirectory ($Phase + '-' + $stamp + '.trx')
    if (Test-Path -LiteralPath $trxPath) {
        [xml]$trx = Get-Content -LiteralPath $trxPath -Raw
        $counters = $trx.TestRun.ResultSummary.Counters
        $result.passed = [int]$counters.passed
        $result.failed = [int]$counters.failed
        $result.skipped = [int]$counters.notExecuted
        $result.trx = $trxPath
    }
}
$result | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath ($prefix + '-result.json') -Encoding utf8
$result | ConvertTo-Json -Depth 6
Get-Content -LiteralPath ($prefix + '.log') -Tail 45
exit $returnCode
