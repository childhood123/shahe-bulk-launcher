param([ValidateSet('batch-create','create-group','launch-group','schedule-launch','schedule-close','cancel-schedules','batch-rename','batch-delete','cleanup')][string]$Stage,[string]$Name)
$ErrorActionPreference='Stop'
$run=$PSScriptRoot
if(-not $Name){$Name=$Stage}
$baselineRecord=Get-Content -LiteralPath (Join-Path $run 'preflight-final-result.json') -Raw | ConvertFrom-Json
if($baselineRecord.returnCode -ne 0 -or $baselineRecord.status -ne 'passed'){throw 'A passed current preflight is required.'}
$auditExe='D:\mycodex\git\artifacts\runs\csharp_non_update_gaps_20261020\audit03\bin\x64\Release\net48\ShaheLauncher.AuditRunner.exe'
$auditArguments=@('lifecycle','--execute','--baseline',[string]$baselineRecord.report,'--prefix','ShaheAudit_20261022_2100B','--stage',$Stage,'--state',(Join-Path $run 'fresh-lifecycle-state.json'),'--output',$run)
$log=Join-Path $run ($Name+'.log')
& $auditExe @auditArguments *> $log
$code=$LASTEXITCODE
$reportPath=Get-Content -LiteralPath $log | Where-Object {$_ -like '*-result.json'} | Select-Object -Last 1
$nativeReport=if($reportPath -and (Test-Path -LiteralPath $reportPath)){Get-Content -LiteralPath $reportPath -Raw | ConvertFrom-Json}else{$null}
$passed=$code -eq 0 -and $nativeReport.status -eq 'passed'
$record=[ordered]@{command=@($auditExe)+$auditArguments;returnCode=$code;status=if($nativeReport){$nativeReport.status}else{'inconclusive'};report=$reportPath;actualBuildDirectory=(Split-Path -Parent $auditExe);passed=if($passed){1}else{0};failed=if($passed){0}else{1};skipped=0;observedPids=$nativeReport.observedPids;targetProcessId=$nativeReport.targetProcessId;cleanup=$nativeReport.cleanup;processCleanup=$nativeReport.processCleanup;errors=$nativeReport.errors;focusAndIndex='not applicable for API stages; UI observations in native report when present'}
$record | ConvertTo-Json -Depth 9 | Set-Content -LiteralPath (Join-Path $run ($Name+'-result.json')) -Encoding utf8
$entry='- AUDIT-03 fresh '+$Stage+' ('+$Name+')：返回码 '+$code+'，状态 '+$record.status+'，通过 '+$record.passed+'、失败 '+$record.failed+'、跳过 0；完整命令、构建目录、PID/清理及原生报告索引见 artifacts/runs/audit03_lifecycle_fresh_20261022/'+$Name+'-result.json，同名 .log 保留原始输出。'
Add-Content -NoNewline -LiteralPath (Join-Path (Get-Location) 'PROJECT_STRUCTURE.md') -Encoding utf8 -Value ("`n"+$entry+"`n")
$record | ConvertTo-Json -Depth 9
if(-not $passed){exit 3}
