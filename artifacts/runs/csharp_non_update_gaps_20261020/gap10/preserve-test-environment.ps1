param([string]$RunDirectory, [string]$FixturePath)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
$ready = Join-Path $RunDirectory 'environment-ready.json'
$release = Join-Path $RunDirectory 'release-environment'
$result = Join-Path $RunDirectory 'environment-restored.json'
$registryPath = 'Software\Microsoft\Windows\CurrentVersion\Run'
$key = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey($registryPath, $false)
$existed = $null -ne $key -and $key.GetValueNames() -contains 'SandboxLauncher'
$oldValue = if ($existed) { $key.GetValue('SandboxLauncher', $null, [Microsoft.Win32.RegistryValueOptions]::DoNotExpandEnvironmentNames) } else { $null }
$oldKind = if ($existed) { $key.GetValueKind('SandboxLauncher') } else { $null }
if ($null -ne $key) { $key.Dispose() }
$saved = New-Object System.Windows.Forms.DataObject
try {
    $original = [System.Windows.Forms.Clipboard]::GetDataObject()
    if ($null -ne $original) {
        foreach ($format in $original.GetFormats($false)) {
            $data = $original.GetData($format, $false)
            if ($null -ne $data) { $saved.SetData($format, $false, $data) }
        }
    }
    @{ ready = $true; formatCount = $saved.GetFormats($false).Length; pid = $PID } | ConvertTo-Json | Set-Content $ready -Encoding UTF8
    $deadline = [DateTime]::UtcNow.AddMinutes(15)
    while (-not (Test-Path -LiteralPath $release) -and [DateTime]::UtcNow -lt $deadline) { Start-Sleep -Milliseconds 200 }
    $owned = [System.Windows.Forms.Clipboard]::ContainsText() -and [System.Windows.Forms.Clipboard]::GetText().Trim('"') -eq $FixturePath
    if ($owned) {
        if ($saved.GetFormats($false).Length -eq 0) { [System.Windows.Forms.Clipboard]::Clear() }
        else { [System.Windows.Forms.Clipboard]::SetDataObject($saved, $true, 5, 50) }
    }
    $key = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey($registryPath, $true)
    if ($existed) {
        if ($null -eq $key) { $key = [Microsoft.Win32.Registry]::CurrentUser.CreateSubKey($registryPath) }
        $key.SetValue('SandboxLauncher', $oldValue, $oldKind)
    } elseif ($null -ne $key) { $key.DeleteValue('SandboxLauncher', $false) }
    if ($null -ne $key) { $key.Dispose() }
    @{ success = $true; clipboardRestored = $owned; otherClipboardContentPreserved = -not $owned; autoRunRestored = $true; pid = $PID } | ConvertTo-Json | Set-Content $result -Encoding UTF8
} catch {
    @{ success = $false; error = $_.Exception.Message; pid = $PID } | ConvertTo-Json | Set-Content $result -Encoding UTF8
    exit 1
}
