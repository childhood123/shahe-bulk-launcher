param([switch]$Legacy, [string]$Suffix = "")
$ErrorActionPreference='Stop'
$repo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..\..'))
$name=if($Legacy){'publish-legacy'+$Suffix}else{'publish-default'+$Suffix}
$target=Join-Path $PSScriptRoot $name
if(Test-Path -LiteralPath $target){throw 'Fresh publish directory required'}
$utf8=New-Object Text.UTF8Encoding($false)
$arguments=@('publish','csharp\ShaheLauncher.App\ShaheLauncher.App.csproj','--configuration','Release','--no-build','--no-restore','--property:Platform=x64',('--property:BaseOutputPath='+$PSScriptRoot+'\bin\'),'--output',$target)
if($Legacy){$arguments+='--property:IncludeLegacyUpdateCompatibility=true'}
& dotnet @arguments *> (Join-Path $PSScriptRoot ($name+'.log'))
$code=$LASTEXITCODE
$files=@(Get-ChildItem -LiteralPath $target -File -Recurse | ForEach-Object {$_.FullName.Substring($target.Length+1)})
$config=Join-Path $target 'Configurations'
$empty=(Test-Path -LiteralPath $config) -and @(Get-ChildItem -LiteralPath $config -Force).Count -eq 0
$hasUpdater=Test-Path -LiteralPath (Join-Path $target 'updater.exe')
$identical=if($Legacy -and $hasUpdater){(Get-FileHash -LiteralPath (Join-Path $target 'updater.exe')).Hash -eq (Get-FileHash -LiteralPath (Join-Path $target '沙河批量打开器.exe')).Hash}else{$null}
if(-not $empty -or ($hasUpdater -ne [bool]$Legacy) -or ($Legacy -and -not $identical)){$code=1}
$result=[ordered]@{command=@('dotnet')+$arguments;returnCode=$code;configurationsEmpty=$empty;hasUpdater=$hasUpdater;legacyCopyMatchesMain=$identical;files=$files}
[IO.File]::WriteAllText((Join-Path $PSScriptRoot ($name+'-result.json')),(($result|ConvertTo-Json -Depth 6)+"`n"),$utf8)
[IO.File]::AppendAllText((Join-Path $repo 'PROJECT_STRUCTURE.md'),("`n- LOG-01 $name：发布与目录核对返回码 $code，Configurations 为空=$empty，updater 存在=$hasUpdater，兼容副本与主 EXE 相同=$identical；命令、清单及日志见 artifacts/runs/diagnostic_logging_release_20261023/$name-result.json、$name.log。没有重新运行通过的测试或修改公开包。`n"),$utf8)
$result|ConvertTo-Json -Depth 5
exit $code
