param([Parameter(Mandatory=$true)][string]$BuildDirectory)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
$build = (Resolve-Path -LiteralPath $BuildDirectory).Path
$run = Join-Path $PSScriptRoot ('case-' + (Get-Date -Format 'yyyyMMdd_HHmmss_fff'))
$app = Join-Path $run 'app'
$null = New-Item -ItemType Directory -Path $app -Force
Copy-Item -Path (Join-Path $build '*') -Destination $app -Recurse
$configDirectory = Join-Path $app 'Configuration'
$fakeSandboxie = Join-Path $app 'fake-sandboxie'
$fakeRoot = Join-Path $app 'sandbox-root'
$null = New-Item -ItemType Directory -Path $configDirectory,$fakeSandboxie,$fakeRoot -Force
[IO.File]::WriteAllBytes((Join-Path $fakeSandboxie 'Start.exe'), [byte[]]@())
$configPath = Join-Path $configDirectory 'shahe_config.ini'
$removeGroup = 'ShaheAudit_DeleteGroup_A_Remove'
$keepGroup = 'ShaheAudit_DeleteGroup_Z_Keep'
$settings = [ordered]@{
    app_history=@(); max_history_items=37; auto_run=$false; show_tray=$false
    hotkeys_enabled=$false; hide_path=$false; sandbox_root=$fakeRoot; sandboxie_path=$fakeSandboxie
    speech_assist_hints=@{enabled=$false}; sandbox_groups=@{}
}
$settings.sandbox_groups[$removeGroup]=@('FixtureOne')
$settings.sandbox_groups[$keepGroup]=@('FixtureTwo')
$text=@('[config]','','[settings]')
foreach($entry in $settings.GetEnumerator()) { $text += $entry.Key+'='+(ConvertTo-Json -InputObject $entry.Value -Compress -Depth 8) }
[IO.File]::WriteAllText($configPath, ($text -join "`r`n")+"`r`n", [Text.UTF8Encoding]::new($false))
$exe=Join-Path $app '沙河批量打开器.exe'
$null=[Reflection.Assembly]::LoadFrom((Join-Path $build 'ShaheLauncher.Core.dll'))
$assembly=[Reflection.Assembly]::LoadFrom((Join-Path $build 'ShaheLauncher.AuditRunner.exe'))
$native=$assembly.GetType('ShaheLauncher.AuditRunner.Win32Automation',$true)
$flags=[Reflection.BindingFlags]::Static -bor [Reflection.BindingFlags]::Public -bor [Reflection.BindingFlags]::NonPublic
$readGroups=$assembly.GetType('ShaheLauncher.AuditRunner.AuditSafety',$true).GetMethod('ReadSandboxGroups',$flags)
if(-not $readGroups){throw 'Required existing group-reader method unavailable.'}
function Native([string]$method,[object[]]$values) { $native.GetMethod($method,$flags).Invoke($null,$values) }
$sendListMessage=$native.GetMethod('SendMessage',$flags,$null,[Type[]]@([IntPtr],[int],[IntPtr],[IntPtr]),$null)
function ListMessage([IntPtr]$handle,[int]$message,[int]$index,[IntPtr]$buffer) { [IntPtr]$sendListMessage.Invoke($null,@($handle,$message,[IntPtr]$index,$buffer)) }
function Read-GroupItems([IntPtr]$dialog) {
    $controls=Native 'GetDescendants' @($dialog)
    $lists=@($controls | Where-Object { $_.ClassName -like '*LISTBOX*' })
    Check ($lists.Count -eq 1) 'Native group list was missing or ambiguous.'
    $list=[IntPtr]$lists[0].Handle
    $count=(ListMessage $list 0x18B 0 ([IntPtr]::Zero)).ToInt32()
    Check ($count -ge 1 -and $count -le 2) 'Unexpected fixture group count.'
    $rows=@()
    for($index=0;$index -lt $count;$index++) {
        $length=(ListMessage $list 0x18A $index ([IntPtr]::Zero)).ToInt32()
        Check ($length -ge 0 -and $length -le 1024) 'Invalid group label length.'
        $buffer=[Runtime.InteropServices.Marshal]::AllocHGlobal($length+1)
        try {
            $read=(ListMessage $list 0x189 $index $buffer).ToInt32()
            Check ($read -ge 0 -and $read -le $length) 'Group text read failed.'
            $rows+=@{index=$index;name=[Runtime.InteropServices.Marshal]::PtrToStringAnsi($buffer,$read)}
        } finally { [Runtime.InteropServices.Marshal]::FreeHGlobal($buffer) }
    }
    return @{handle=$list;items=$rows}
}
function Check([bool]$condition,[string]$message) { if(-not $condition) { throw $message } }
function Read-Settings {
    $store=[ShaheLauncher.Core.Configuration.ConfigStore]::new($configPath,[Environment]::UserName,[Func[long]]{[DateTimeOffset]::UtcNow.ToUnixTimeSeconds()})
    return $store.GetJson('settings_json',$null)
}
function Open-GroupDialog([Diagnostics.Process]$target) {
    $deadline=[DateTime]::UtcNow.AddSeconds(10)
    $ownedWindow=$null
    do {
        $top=Native 'GetTopWindows' @()
        $ownedWindow=@($top | Where-Object { $_.Title -like '沙河批量打开器*' -and (Native 'WindowProcessId' @([IntPtr]$_.Handle)) -eq $target.Id }) | Select-Object -First 1
        if(-not $ownedWindow){Start-Sleep -Milliseconds 100}
    } while(-not $ownedWindow -and [DateTime]::UtcNow -lt $deadline)
    Check ($null -ne $ownedWindow) 'No main window.'
    $window=[IntPtr]$ownedWindow.Handle
    Check ([bool](Native 'EnsureForegroundWindow' @($window,2000))) 'Main window foreground unconfirmed.'
    $commands=Native 'GetMenuCommands' @($window)
    $id=[ShaheLauncher.AuditRunner.AuditSafety]::FindUniqueMenuCommand([ShaheLauncher.AuditRunner.MenuCommandInfo[]]$commands,'删除分组')
    Check ([bool](Native 'PostCommand' @($window,$id))) 'Menu dispatch failed.'
    $dialog=[IntPtr](Native 'WaitForWindow' @($target.Id,$window,'删除分组',10000))
    Check ($dialog -ne [IntPtr]::Zero) 'No owned delete-group dialog.'
    Check ([bool](Native 'EnsureForegroundWindow' @($dialog,2000))) 'Dialog foreground unconfirmed.'
    $element=[Windows.Automation.AutomationElement]::FromHandle($dialog)
    $groups=Read-GroupItems $dialog
    return @{window=$window;dialog=$dialog;element=$element;items=$groups.items;list=$groups.handle;commandId=$id;wasHidden=(-not $ownedWindow.Visible)}
}
$registryPath='Software\Microsoft\Windows\CurrentVersion\Run'
$key=[Microsoft.Win32.Registry]::CurrentUser.OpenSubKey($registryPath,$false)
$existed=$null -ne $key -and $key.GetValueNames() -contains 'SandboxLauncher'
$oldValue=if($existed){$key.GetValue('SandboxLauncher',$null,[Microsoft.Win32.RegistryValueOptions]::DoNotExpandEnvironmentNames)}else{$null}
$oldKind=if($existed){$key.GetValueKind('SandboxLauncher')}else{$null}
if($key){$key.Dispose()}
$realIni=Join-Path $env:WINDIR 'Sandboxie.ini'
$iniBefore=(Get-FileHash -LiteralPath $realIni -Algorithm SHA256).Hash
$controlBefore=@(Get-Process -Name SbieCtrl -ErrorAction SilentlyContinue | ForEach-Object Id)
$report=[ordered]@{commandLine=[Environment]::CommandLine;status='inconclusive';returnCode=3;sourceBuildDirectory=$build;actualBuildDirectory=$app;executable=$exe;executableSha256=(Get-FileHash -LiteralPath $exe -Algorithm SHA256).Hash;configPath=$configPath;pid=$null;restartPid=$null;menuCommandId=$null;selectedIndex=$null;selectedName='';foregroundConfirmed=$false;removed=$false;otherGroupPreserved=$false;otherSettingPreserved=$false;restartPersisted=$false;normalExit=$false;forcedCleanup=$false;userControlPreserved=$false;realIniUnchanged=$false;autoRunRestored=$false;keyLoss='not applicable: semantic selection and owned button command';timeout=$false;errors=@()}
$process=$null
$window=[IntPtr]::Zero
try {
    Check (@(Get-Process -Name '沙河批量打开器' -ErrorAction SilentlyContinue).Count -eq 0) 'A user launcher is already running; preserve its single instance.'
    $process=Start-Process -FilePath $exe -WorkingDirectory $app -WindowStyle Hidden -PassThru
    $report.pid=$process.Id
    $view=Open-GroupDialog $process
    $window=$view.window
    $report.mainWindowWasHidden=$view.wasHidden
    $report.mainWindowHandle=$window.ToInt64()
    $report.menuCommandId=$view.commandId
    $report.foregroundConfirmed=$true
    $report.observedItems=@($view.items | ForEach-Object { $_.name })
    $report.observedControls=@($view.element.FindAll([Windows.Automation.TreeScope]::Descendants,[Windows.Automation.Condition]::TrueCondition) | ForEach-Object { @{name=$_.Current.Name;type=$_.Current.ControlType.ProgrammaticName} })
    $matches=@($view.items | Where-Object { $_.name -eq $removeGroup })
    Check ($matches.Count -eq 1) 'Target group name was missing or ambiguous.'
    $report.selectedIndex=[int]$matches[0].index
    $selected=(ListMessage $view.list 0x186 $report.selectedIndex ([IntPtr]::Zero)).ToInt32()
    Check ($selected -eq $report.selectedIndex) 'Native selection failed.'
    $confirmed=(ListMessage $view.list 0x188 0 ([IntPtr]::Zero)).ToInt32()
    Check ($confirmed -eq $report.selectedIndex) 'Selected index was not confirmed.'
    $report.selectedName=[string]$view.items[$confirmed].name
    Check ($report.selectedName -eq $removeGroup) 'Selected name mismatch.'
    $report.selectionBackend='Native LISTBOX exact text and selected-index readback; UIA ListItem unavailable.'
    $controls=Native 'GetDescendants' @($view.dialog)
    $ok=@($controls | Where-Object { $_.Title -eq '确定' -and $_.ClassName -like '*BUTTON*' })
    Check ($ok.Count -eq 1) 'Owned OK button was missing or ambiguous.'
    Check ([bool](Native 'PostButtonClickNotification' @($view.dialog,[IntPtr]$ok[0].Handle))) 'Explicit confirmation failed.'
    $deadline=[DateTime]::UtcNow.AddSeconds(5)
    do {
        Start-Sleep -Milliseconds 100
        $saved=Read-Settings
        $groups=$readGroups.Invoke($null,[object[]]@($saved.PSObject.BaseObject))
    } while($groups.ContainsKey($removeGroup) -and [DateTime]::UtcNow -lt $deadline)
    $report.removed=-not $groups.ContainsKey($removeGroup)
    $report.otherGroupPreserved=$groups.ContainsKey($keepGroup) -and ($groups[$keepGroup] -join ',') -eq 'FixtureTwo'
    $report.otherSettingPreserved=[int]$saved['max_history_items'] -eq 37
    Check ($report.removed -and $report.otherGroupPreserved -and $report.otherSettingPreserved) 'Deletion changed the wrong persisted state.'
    Check ([bool](Native 'CloseWindow' @($window))) 'Could not close owned main window.'
    Check ($process.WaitForExit(10000)) 'Owned main process did not exit normally.'
    $process=Start-Process -FilePath $exe -WorkingDirectory $app -WindowStyle Hidden -PassThru
    $report.restartPid=$process.Id
    $view=Open-GroupDialog $process
    $window=$view.window
    $names=@($view.items | ForEach-Object { $_.name })
    $report.restartPersisted=$names.Count -eq 1 -and $names[0] -eq $keepGroup
    Check $report.restartPersisted 'Restart did not preserve the group deletion.'
    Check ([bool](Native 'CloseWindow' @($view.dialog))) 'Could not cancel reopened group dialog.'
    Check ([bool](Native 'CloseWindow' @($window))) 'Could not close restarted main window.'
    $report.normalExit=$process.WaitForExit(10000)
    Check $report.normalExit 'Restarted process did not exit normally.'
    $report.status='passed';$report.returnCode=0
} catch { $report.errors += $_.Exception.ToString(); $report.timeout = $_.Exception.ToString() -match 'did not exit|No main window|No owned delete-group dialog' }
finally {
    if($process -and -not $process.HasExited) {
        $owned=Get-CimInstance Win32_Process -Filter "ProcessId = $($process.Id)"
        if($owned -and [string]::Equals([IO.Path]::GetFullPath($owned.ExecutablePath),$exe,[StringComparison]::OrdinalIgnoreCase)) {
            Stop-Process -Id $process.Id -Force
            $null=$process.WaitForExit(5000)
            $report.forcedCleanup=$true
        }
        $report.status='inconclusive';$report.returnCode=3
    }
    $key=[Microsoft.Win32.Registry]::CurrentUser.OpenSubKey($registryPath,$true)
    $currentExists=$null -ne $key -and $key.GetValueNames() -contains 'SandboxLauncher'
    if($existed -and -not $currentExists) {
        if(-not $key){$key=[Microsoft.Win32.Registry]::CurrentUser.CreateSubKey($registryPath)}
        $key.SetValue('SandboxLauncher',$oldValue,$oldKind)
    }
    $currentExists=$null -ne $key -and $key.GetValueNames() -contains 'SandboxLauncher'
    $report.autoRunRestored=if($existed){$currentExists -and $key.GetValue('SandboxLauncher',$null,[Microsoft.Win32.RegistryValueOptions]::DoNotExpandEnvironmentNames) -eq $oldValue -and $key.GetValueKind('SandboxLauncher') -eq $oldKind}else{-not $currentExists}
    if($key){$key.Dispose()}
    $report.realIniUnchanged=(Get-FileHash -LiteralPath $realIni -Algorithm SHA256).Hash -eq $iniBefore
    $controlAfter=@(Get-Process -Name SbieCtrl -ErrorAction SilentlyContinue | ForEach-Object Id)
    $report.userControlPreserved=@(Compare-Object $controlBefore $controlAfter).Count -eq 0
    if(-not ($report.autoRunRestored -and $report.realIniUnchanged -and $report.userControlPreserved)){$report.status='inconclusive';$report.returnCode=3}
    $report | ConvertTo-Json -Depth 7 | Set-Content -LiteralPath (Join-Path $run 'result.json') -Encoding utf8
    $entry='- AUDIT-03 删除分组隔离真实 EXE：返回码 '+$report.returnCode+'，状态 '+$report.status+'，测试计数不适用。完整命令、PID、构建目录、菜单/焦点/索引校准、删除与重启、配置保留及清理见 '+$run.Replace((Get-Location).Path+'\','').Replace('\','/')+'/result.json；该场景不执行真实 Sandboxie 写操作。'
    Add-Content -NoNewline -LiteralPath (Join-Path (Get-Location) 'PROJECT_STRUCTURE.md') -Encoding utf8 -Value ("`n"+$entry+"`n")
}
$report | ConvertTo-Json -Depth 7
exit $report.returnCode
