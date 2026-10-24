$ErrorActionPreference = 'Stop'
$runRoot = $PSScriptRoot
$repo = [IO.Path]::GetFullPath((Join-Path $runRoot '..\..\..'))
$source = Join-Path $runRoot 'publish-default-final-nolegacy'
$delivery = Join-Path $runRoot 'delivery-final'
$package = Join-Path $delivery 'shahe-bulk-launcher-4.0.48-win-x64.zip'
$resultPath = Join-Path $runRoot 'portable-final-package-result.json'
$utf8 = [Text.UTF8Encoding]::new($false)
$code = 0
$result = [ordered]@{ returnCode = 1; version = '4.0.48'; package = $package }

try {
    if (Test-Path -LiteralPath $package) { throw 'Fresh package path required' }
    New-Item -ItemType Directory -Path $delivery -Force | Out-Null
    Add-Type -AssemblyName System.IO.Compression.FileSystem

    $archive = [IO.Compression.ZipFile]::Open($package, [IO.Compression.ZipArchiveMode]::Create)
    $inventory = [ordered]@{}
    try {
        $archive.CreateEntry('Configurations/') | Out-Null
        $archive.CreateEntry('logs/') | Out-Null
        foreach ($file in Get-ChildItem -LiteralPath $source -File -Recurse | Sort-Object FullName) {
            if ($file.Extension -eq '.pdb') { continue }
            $relative = $file.FullName.Substring($source.Length + 1).Replace('\', '/')
            if ($relative -match '^(Configurations|logs|log)/' -or $relative -match '^updater\.exe(?:\.config)?$') {
                throw "Forbidden package file: $relative"
            }
            [IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
                $archive,
                $file.FullName,
                $relative,
                [IO.Compression.CompressionLevel]::Optimal) | Out-Null
            $inventory[$relative] = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
        }
    }
    finally {
        $archive.Dispose()
    }

    $required = @(
        '沙河批量打开器.exe',
        '沙河批量打开器.exe.config',
        'BouncyCastle.Crypto.dll',
        'Newtonsoft.Json.dll',
        'ShaheLauncher.Core.dll',
        'lib/byctrl-x64.dll',
        'lib/byctrl.conf',
        'lib/nvdaControllerClient64.dll',
        'lib/SAAPI64.dll',
        'lib/Tolk.dll',
        'lib/ZDSRAPI_x64.dll',
        'lib/ZDSRAPI.ini',
        '打赏/微信赞赏码.jpg',
        '打赏/支付宝赞赏码.jpg'
    )
    foreach ($name in $required) {
        if (-not $inventory.Contains($name)) { throw "Missing package file: $name" }
    }

    $check = [IO.Compression.ZipFile]::OpenRead($package)
    try {
        $entries = @($check.Entries)
        foreach ($directory in @('Configurations/', 'logs/')) {
            $directoryEntries = @($entries | Where-Object {
                $_.FullName.StartsWith($directory, [StringComparison]::OrdinalIgnoreCase)
            })
            if ($directoryEntries.Count -ne 1 -or $directoryEntries[0].FullName -ne $directory -or $directoryEntries[0].Length -ne 0) {
                throw "Package directory is not empty: $directory"
            }
        }
        if ($entries | Where-Object { $_.FullName -match '(^|/)updater\.exe(?:\.config)?$' -or $_.FullName.EndsWith('.pdb') }) {
            throw 'Updater compatibility files or PDB files were packaged'
        }

        $mainEntry = $check.GetEntry('沙河批量打开器.exe')
        if ($null -eq $mainEntry) { throw 'Main executable entry missing' }
        $stream = $mainEntry.Open()
        $sha = [Security.Cryptography.SHA256]::Create()
        try {
            $archiveMainHash = [Convert]::ToHexString($sha.ComputeHash($stream))
        }
        finally {
            $sha.Dispose()
            $stream.Dispose()
        }
        if ($archiveMainHash -ne $inventory['沙河批量打开器.exe']) { throw 'Main executable hash changed in ZIP' }
    }
    finally {
        $check.Dispose()
    }

    $result.returnCode = 0
    $result.size = (Get-Item -LiteralPath $package).Length
    $result.sha256 = (Get-FileHash -LiteralPath $package -Algorithm SHA256).Hash
    $result.configurationsEmpty = $true
    $result.logsEmpty = $true
    $result.updaterPresent = $false
    $result.pdbPackaged = $false
    $result.mainExeSha256 = $inventory['沙河批量打开器.exe']
    $result.inventory = $inventory
}
catch {
    $code = 1
    $result.error = $_.Exception.ToString()
}

[IO.File]::WriteAllText($resultPath, (($result | ConvertTo-Json -Depth 8) + "`n"), $utf8)
[IO.File]::AppendAllText(
    (Join-Path $repo 'PROJECT_STRUCTURE.md'),
    "`n- LOG-01 4.0.48 最终绿色版封装：返回码 $code；ZIP 仅含空 Configurations/、空 logs/ 和程序文件，不含 updater.exe、PDB、用户配置或日志内容。大小、SHA-256、逐文件清单见 artifacts/runs/diagnostic_logging_release_20261023/portable-final-package-result.json；包位于 delivery-final/。`n",
    $utf8)
$result | ConvertTo-Json -Depth 8
exit $code
