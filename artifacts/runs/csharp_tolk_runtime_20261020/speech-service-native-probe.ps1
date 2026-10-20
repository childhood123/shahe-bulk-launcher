param(
    [string]$OutputDirectory = "D:\mycodex\git\artifacts\runs\csharp_tolk_runtime_20261020\bin\x64\Release\net48",
    [string]$Text = "沙河批量打开器朗读测试",
    [string]$ResultPath = "D:\mycodex\git\artifacts\runs\csharp_tolk_runtime_20261020\speech-service-native-probe-result.json",
    [string]$LogPath = "D:\mycodex\git\artifacts\runs\csharp_tolk_runtime_20261020\speech-service-native-probe.log"
)

$returnCode = 1
$errorText = ""
$ensureResult = $false
$detected = ""
$runtimeCleared = $false
$disabledAfterShutdown = $false
try {
    $corePath = Join-Path $OutputDirectory "ShaheLauncher.Core.dll"
    $assembly = [Reflection.Assembly]::LoadFrom($corePath)
    $serviceType = $assembly.GetType("ShaheLauncher.Core.Speech.SpeechService", $true)
    $callbacksType = $assembly.GetType("ShaheLauncher.Core.Speech.SpeechServiceCallbacks", $true)
    $callbacks = [Activator]::CreateInstance($callbacksType)
    $configDirectory = "D:\mycodex\git\artifacts\runs\csharp_tolk_runtime_20261020\speech-service-config"
    [IO.Directory]::CreateDirectory($configDirectory) | Out-Null
    $service = [Activator]::CreateInstance(
        $serviceType,
        @($OutputDirectory, $configDirectory, $callbacks))

    $ensureResult = [bool]$serviceType.GetMethod("EnsureAssistSpeechRuntime").Invoke($service, $null)
    $runtimeField = $serviceType.GetField(
        "_assistSpeechRuntime",
        [Reflection.BindingFlags]::Instance -bor [Reflection.BindingFlags]::NonPublic)
    $runtime = $runtimeField.GetValue($service)
    if (-not $runtime) { throw "SpeechService did not retain a native backend" }
    $runtimeType = $runtime.GetType()
    $detectDelegate = $runtimeType.GetProperty("DetectScreenReader").GetValue($runtime)
    $detected = [string]$detectDelegate.DynamicInvoke()
    $outputDelegate = $runtimeType.GetProperty("Output").GetValue($runtime)
    $outputDelegate.DynamicInvoke($Text, $true) | Out-Null
    $serviceType.GetMethod("ShutdownAssistSpeechRuntime").Invoke($service, $null) | Out-Null
    $runtimeCleared = $null -eq $runtimeField.GetValue($service)
    $disabledAfterShutdown = [bool]$serviceType.GetProperty("AssistSpeechDisabled").GetValue($service)
    if ($ensureResult -and $detected -and $runtimeCleared -and $disabledAfterShutdown) {
        $returnCode = 0
    }
}
catch {
    $errorText = $_.Exception.ToString()
}

$record = [ordered]@{
    command = "speech-service-native-probe.ps1"
    returnCode = $returnCode
    outputDirectory = $OutputDirectory
    ensureAssistSpeechRuntime = $ensureResult
    detectedScreenReader = $detected
    outputInvoked = $ensureResult -and [bool]$detected
    runtimeCleared = $runtimeCleared
    disabledAfterShutdown = $disabledAfterShutdown
    error = $errorText
}
$json = $record | ConvertTo-Json -Depth 4
$json | Set-Content -LiteralPath $ResultPath -Encoding utf8
@(
    "returnCode: $returnCode",
    "outputDirectory: $OutputDirectory",
    "ensureAssistSpeechRuntime: $ensureResult",
    "detectedScreenReader: $detected",
    "outputInvoked: $($record.outputInvoked)",
    "runtimeCleared: $runtimeCleared",
    "disabledAfterShutdown: $disabledAfterShutdown",
    "error: $errorText"
) | Set-Content -LiteralPath $LogPath -Encoding utf8
$json
exit $returnCode
