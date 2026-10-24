$ErrorActionPreference='Stop'
$runRoot=$PSScriptRoot
$source=Join-Path $runRoot 'publish-legacy-final'
$package=Join-Path $runRoot 'shahe-bulk-launcher-4.0.48-win-x64.zip'
if(Test-Path -LiteralPath $package){throw 'Fresh package path required'}
Add-Type -AssemblyName System.IO.Compression.FileSystem
$code=0
$result=[ordered]@{returnCode=1;version='4.0.48';package=$package}
try {
 $archive=[IO.Compression.ZipFile]::Open($package,[IO.Compression.ZipArchiveMode]::Create)
 $inventory=[ordered]@{}
 try {
  $archive.CreateEntry('Configurations/')|Out-Null
  foreach($file in Get-ChildItem -LiteralPath $source -File -Recurse){
   if($file.Extension -eq '.pdb'){continue}
   $relative=$file.FullName.Substring($source.Length+1).Replace('\','/')
   if($relative -match '^(Configurations|logs|log)/'){throw 'User data must not be packaged'}
   [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive,$file.FullName,$relative,[IO.Compression.CompressionLevel]::Optimal)|Out-Null
   $inventory[$relative]=(Get-FileHash -LiteralPath $file.FullName).Hash
  }
 } finally {$archive.Dispose()}
 if($inventory['updater.exe'] -ne $inventory['沙河批量打开器.exe']){throw 'Compatibility alias differs from main'}
 $check=[IO.Compression.ZipFile]::OpenRead($package)
 try {
  $configEntries=@($check.Entries|Where-Object {$_.FullName.StartsWith('Configurations/')})
  if($configEntries.Count -ne 1 -or $configEntries[0].FullName -ne 'Configurations/' -or $configEntries[0].Length -ne 0){throw 'Configuration directory is not empty'}
 } finally {$check.Dispose()}
 $result.returnCode=0
 $result.size=(Get-Item -LiteralPath $package).Length
 $result.sha256=(Get-FileHash -LiteralPath $package).Hash
 $result.configurationsEmpty=$true
 $result.logsPackaged=$false
 $result.compatibilityAliasMatches=$true
 $result.inventory=$inventory
} catch {$code=1;$result.error=$_.Exception.ToString()}
$result|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $runRoot 'package-result.json') -Encoding utf8
[IO.File]::AppendAllText('D:\mycodex\git\PROJECT_STRUCTURE.md',([string][char]10)+"- LOG-01 发布包封装：package-release.ps1 返回码 $code；版本、完整清单、大小及 SHA-256 以 artifacts/runs/diagnostic_logging_release_20261023/package-result.json 为准。空 Configurations，不带用户日志；旧版兼容入口复用主 EXE。"+([string][char]10),[Text.UTF8Encoding]::new($false))
$result|ConvertTo-Json -Depth 6
exit $code
