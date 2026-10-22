$targets = @(
    'csharp/ShaheLauncher.App/lib/Tolk.dll',
    'csharp/ShaheLauncher.App/lib/ZDSRAPI_x64.dll',
    'csharp/ShaheLauncher.App/lib/byctrl-x64.dll'
)
$found = [ordered]@{}
foreach ($target in $targets) {
    $bytes = [IO.File]::ReadAllBytes($target)
    $ascii = [Text.Encoding]::ASCII.GetString($bytes)
    $wide = [Text.Encoding]::Unicode.GetString($bytes)
    $found[$target] = [ordered]@{
        zdsrIni = $ascii.Contains('%s\ZDSRAPI.ini') -or $wide.Contains('ZDSRAPI.ini')
        boyConfig = $wide.Contains('byctrl.conf')
        byctrlDll = $ascii.Contains('byctrl-x64.dll') -or $wide.Contains('byctrl-x64.dll')
    }
}
$hasMissing = ($found.Values | ForEach-Object { -not ($_.zdsrIni -or $_.boyConfig -or $_.byctrlDll) }) -contains $true
if ($hasMissing) { throw ($found | ConvertTo-Json -Depth 5) }
$found | ConvertTo-Json -Depth 5 | Set-Content 'artifacts/runs/csharp_non_update_gaps_20261020/voice02/binary-config-probe.json' -Encoding utf8
Write-Output 'PASS: vendor binaries expose packaged ZDSRAPI.ini/byctrl.conf dependency hints; failed=0 skipped=0'
