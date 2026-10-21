param([string]$Name, [string]$Kind = 'test', [string]$Filter = '')
$ErrorActionPreference = 'Continue'
$root = 'D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\review01'
$log = Join-Path $root "$Name.log"
$trx = Join-Path $root "$Name.trx"
switch ($Kind) {
 'test' { $exe = 'dotnet'; $argv = @('test','csharp\ShaheBulkLauncher.sln','--configuration','Release','--no-restore','--property:Platform=x64',"--property:BaseOutputPath=$root\bin\",'--logger',"trx;LogFileName=$Name.trx",'--results-directory',$root); if ($Filter) { $argv += @('--filter',$Filter) } }
 'vstest' { $exe = 'dotnet'; $argv = @('vstest',"$root\bin\x64\Release\net48\ShaheLauncher.Tests.dll",'/Platform:x64',"/Logger:trx;LogFileName=$Name.trx","/ResultsDirectory:$root"); if ($Filter) { $argv += "/TestCaseFilter:$Filter" } }
 'python' { $exe = 'python'; $argv = @('-m','unittest','discover','-s','tests','-p','test_*.py','-v') }
 'compile' { $exe = 'python'; $argv = @('-m','compileall','-q','shahe_bulk_launcher','tests') }
 'release' { $exe = 'dotnet'; $argv = @('build','csharp\ShaheBulkLauncher.sln','--configuration','Release','--no-restore','--property:Platform=x64',"--property:BaseOutputPath=$root\bin\") }
}
& $exe @argv *> $log
$code = $LASTEXITCODE
$result = [ordered]@{ command = @($exe) + $argv; returnCode = $code; log = $log; passed = $null; failed = $null; skipped = $null }
if (Test-Path $trx) { [xml]$xml = Get-Content $trx; $c = $xml.TestRun.ResultSummary.Counters; $result.passed = [int]$c.passed; $result.failed = [int]$c.failed; $result.skipped = [int]$c.notExecuted; $result.trx = $trx }
if ($Kind -eq 'python') { $raw = Get-Content $log -Raw; if ($raw -match 'Ran (\d+) tests' -and $code -eq 0) { $result.passed = [int]$Matches[1]; $result.failed = 0; $result.skipped = 0 } }
$result | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $root "$Name-result.json") -Encoding utf8
$counts = if ($null -eq $result.passed) { '计数不适用' } else { "通过 $($result.passed)、失败 $($result.failed)、跳过 $($result.skipped)" }
Add-Content PROJECT_STRUCTURE.md "`n- REVIEW-01 $Name：命令 ``$exe $($argv -join ' ')``；返回码 $code，$counts。日志与命令索引：``artifacts/runs/csharp_non_update_gaps_20261020/review01/$Name.log``、``$Name-result.json``；如为 C# 测试，另有同名 TRX。"
$result | ConvertTo-Json -Depth 5
