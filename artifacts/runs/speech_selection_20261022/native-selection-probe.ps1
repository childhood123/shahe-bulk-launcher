param([ValidateSet('automatic','nvda','zdsr','boy')][string]$Selection, [switch]$Speak, [string]$Name, [switch]$NoReaders, [string]$AppDirectory, [switch]$WaitForNvda, [switch]$RetryUnavailableNvda)
$ErrorActionPreference = 'Stop'
$runRoot = $PSScriptRoot
if (-not $Name) { $Name = 'native-' + $Selection }
$output = Join-Path $runRoot 'bin\x64\Release\net48'
if (-not $AppDirectory) { $AppDirectory = $output }
$record = [ordered]@{ commandLine = [Environment]::CommandLine; simulatedNoReaderProcesses = [bool]$NoReaders; simulatedUnavailableNvda = [bool]$RetryUnavailableNvda; appDirectory = $AppDirectory; activeRoute = ''; savedSelection = ''; requestedLibrary = ''; scenario = $Selection; pid = $PID; is64Bit = [Environment]::Is64BitProcess; outputDirectory = $output; selectedLibrary = ''; loaded = $false; detected = ''; outputInvoked = $false; runtimeCleared = $false; status = 'failed'; returnCode = 1; errors = @() }
$tracePath = Join-Path $runRoot ($Name + '-trace.log')
function Trace-Probe([string]$stage) { Add-Content -LiteralPath $tracePath -Value ((Get-Date -Format o) + ' pid=' + $PID + ' ' + $stage) -Encoding utf8 }
Trace-Probe 'start'
$service = $null
try {
    $assembly = [Reflection.Assembly]::LoadFrom((Join-Path $output 'ShaheLauncher.Core.dll'))
    $type = $assembly.GetType('ShaheLauncher.Core.Speech.SpeechService', $true)
    $callbacksType = $assembly.GetType('ShaheLauncher.Core.Speech.SpeechServiceCallbacks', $true)
    $callbacks = [Activator]::CreateInstance($callbacksType)
    if ($NoReaders -or $RetryUnavailableNvda) {
        $names = if ($RetryUnavailableNvda) { 'new string[] { "nvda", "zdsr" }' } else { 'new string[0]' }
        Add-Type -TypeDefinition ('public static class ReaderProbeInputs { public static System.Collections.Generic.IEnumerable<string> None() { return ' + $names + '; } }')
        $property = $callbacksType.GetProperty('EnumerateProcessNames')
        $getter = [Delegate]::CreateDelegate($property.PropertyType, [ReaderProbeInputs].GetMethod('None'))
        $property.SetValue($callbacks, $getter)
    }
    $configuration = Join-Path $runRoot ('native-config-' + $Selection)
    $service = $type.GetConstructors()[0].Invoke([object[]]@([string]$AppDirectory, [string]$configuration, $callbacks.PSObject.BaseObject))
    $value = if ($Selection -eq 'automatic') { '' } else { $Selection }
    $type.GetMethod('SaveSpeechInterfaceDll').Invoke($service, @($value)) | Out-Null
    $binding = [Reflection.BindingFlags]::Instance -bor [Reflection.BindingFlags]::NonPublic
    $record.selectedLibrary = [string]$type.GetMethod('GetAssistSpeechLibraryPath', $binding).Invoke($service, $null)
    $record.requestedLibrary = $record.selectedLibrary
    Trace-Probe 'before ensure'
    $record.loaded = [bool]$type.GetMethod('EnsureAssistSpeechRuntime').Invoke($service, $null)
    Trace-Probe ('after ensure: ' + $record.loaded)
    $record.activeRoute = [string]$type.GetField('_assistSpeechRoute', $binding).GetValue($service)
    $record.savedSelection = [string]$service.SpeechInterfaceDll
    $record.selectedLibrary = [string]$type.GetMethod('GetAssistSpeechLibraryPathForRoute', $binding).Invoke($service, @($record.activeRoute))
    $runtimeField = $type.GetField('_assistSpeechRuntime', $binding)
    $runtime = $runtimeField.GetValue($service)
    if ($record.loaded -and $runtime) {
        Trace-Probe 'before detect'
        $record.detected = [string]$runtime.DetectScreenReader.DynamicInvoke()
        Trace-Probe ('after detect: ' + $record.detected)
        if ($Speak) {
            $text = if ($NoReaders) { 'Windows 系统语音回退测试' } elseif ($Selection -eq 'nvda') { 'NVDA 接口朗读测试' } elseif ($Selection -eq 'zdsr') { '争渡接口朗读测试' } else { '自动接口朗读测试' }
            $runtime.Output.DynamicInvoke($text, $false) | Out-Null
            $record.outputInvoked = $true
        }
        if ($RetryUnavailableNvda) {
            for ($attempt = 0; $attempt -lt 5; $attempt++) {
                Trace-Probe ('before unavailable NVDA retry ' + $attempt)
                if (-not $service.EnsureAssistSpeechRuntime()) { throw 'Fallback was lost during unavailable NVDA retry.' }
                Trace-Probe ('after unavailable NVDA retry ' + $attempt)
            }
        }
        if ($WaitForNvda) {
            if ($Selection -ne 'nvda' -or $record.activeRoute -eq 'nvda') { throw 'Recovery requires NVDA selected but initially unavailable.' }
            $record.initial = [ordered]@{ route = $record.activeRoute; detected = $record.detected; savedSelection = $record.savedSelection; outputInvoked = $record.outputInvoked }
            if ($NoReaders) { $callbacks.EnumerateProcessNames = $null; $record.processEnumerationRestored = $true }
            Trace-Probe 'waiting for NVDA recovery'
            $deadline = [DateTime]::UtcNow.AddSeconds(45)
            do {
                Start-Sleep -Milliseconds 250
                Trace-Probe 'before recovery ensure'
                $record.loaded = [bool]$type.GetMethod('EnsureAssistSpeechRuntime').Invoke($service, $null)
                Trace-Probe ('after recovery ensure: ' + $record.loaded)
                $record.activeRoute = [string]$type.GetField('_assistSpeechRoute', $binding).GetValue($service)
            } while ($record.activeRoute -ne 'nvda' -and [DateTime]::UtcNow -lt $deadline)
            $runtime = $runtimeField.GetValue($service)
            if (-not $record.loaded -or $record.activeRoute -ne 'nvda' -or $service.SpeechInterfaceDll -ne 'nvda') { throw 'Selected NVDA did not recover before timeout.' }
            $record.detected = [string]$runtime.DetectScreenReader.DynamicInvoke()
            $record.selectedLibrary = [string]$type.GetMethod('GetAssistSpeechLibraryPathForRoute', $binding).Invoke($service, @($record.activeRoute))
            $type.GetMethod('SaveSpeechInterfaceDll').Invoke($service, @('nvda')) | Out-Null
            if (-not [object]::ReferenceEquals($runtime, $runtimeField.GetValue($service))) { throw 'Saving the same selection replaced its live backend.' }
            if ($Speak) { $runtime.Output.DynamicInvoke('NVDA 恢复后固定接口朗读测试', $false) | Out-Null }
            $record.savedSelection = [string]$service.SpeechInterfaceDll
            $record.recoveredWithoutChangingSelection = $true
            Trace-Probe 'NVDA recovered and same selection kept its backend'
        }
        $currentRuntime = $runtimeField.GetValue($service)
        $type.GetMethod('SaveSpeechInterfaceDll').Invoke($service, @($value)) | Out-Null
        $record.sameSelectionKeepsBackend = [object]::ReferenceEquals($currentRuntime, $runtimeField.GetValue($service))
        $record.savedSelection = [string]$service.SpeechInterfaceDll
        if (-not $record.sameSelectionKeepsBackend -or $record.savedSelection -ne $value) { throw 'Confirming the selected reader replaced its connection or choice.' }
        $record.status = 'passed'
        $record.returnCode = 0
    } else {
        $record.status = 'unavailable'
        $record.returnCode = 3
    }
} catch { $record.errors += $_.Exception.ToString() }
finally {
    if ($service) {
        try {
            Trace-Probe 'before shutdown'
            $service.ShutdownAssistSpeechRuntime()
            Trace-Probe 'after shutdown'
            $record.runtimeCleared = $null -eq $runtimeField.GetValue($service)
        } catch { $record.errors += $_.Exception.ToString(); $record.status = 'failed'; $record.returnCode = 1 }
    }
}
$resultPath = Join-Path $runRoot ($Name + '-result.json')
$record | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $resultPath -Encoding utf8
$entry = '- VOICE-03 原生接口 ' + $Selection + '：返回码 ' + $record.returnCode + '，状态 ' + $record.status + '，实际加载/检测/输出调用与释放、PID、构建目录见 artifacts/runs/speech_selection_20261022/' + $Name + '-result.json；输出调用证据不等同于人工听感确认。'
Add-Content -NoNewline -LiteralPath (Join-Path (Get-Location) 'PROJECT_STRUCTURE.md') -Value ("`n" + $entry + "`n") -Encoding utf8
$record | ConvertTo-Json -Depth 6
exit $record.returnCode
