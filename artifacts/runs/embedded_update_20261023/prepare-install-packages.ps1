param([string]$Suffix = "")
$ErrorActionPreference='Stop'
$utf8=New-Object Text.UTF8Encoding($false)
$repo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..\..'))
Add-Type -AssemblyName System.IO.Compression.FileSystem
$inventory=@{}
foreach($kind in @('default','legacy')) {
$source=Join-Path $PSScriptRoot ('publish-'+$kind+$Suffix)
$payload=Join-Path $PSScriptRoot ('payload-'+$kind+$Suffix)
if(Test-Path -LiteralPath $payload){throw 'Fresh payload directory required'}
[IO.Directory]::CreateDirectory($payload)|Out-Null
foreach($file in Get-ChildItem -LiteralPath $source -Recurse -File) {
if($file.Extension -eq '.pdb'){continue}
$relative=$file.FullName.Substring($source.Length+1)
$destination=Join-Path $payload $relative
[IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($destination))|Out-Null
Copy-Item -LiteralPath $file.FullName -Destination $destination
}
[IO.Directory]::CreateDirectory((Join-Path $payload 'Configurations'))|Out-Null
Copy-Item -LiteralPath (Join-Path $source 'ShaheLauncher.Core.dll') -Destination (Join-Path $payload 'lib\new-runtime.dll')
$zip=Join-Path $PSScriptRoot ('payload-'+$kind+$Suffix+'.zip')
[IO.Compression.ZipFile]::CreateFromDirectory($payload,$zip)
$inventory[$kind]=@{zip=$zip;sha256=(Get-FileHash -LiteralPath $zip).Hash;size=(Get-Item -LiteralPath $zip).Length;mainSha256=(Get-FileHash -LiteralPath (Join-Path $payload '沙河批量打开器.exe')).Hash;coreSha256=(Get-FileHash -LiteralPath (Join-Path $payload 'ShaheLauncher.Core.dll')).Hash;newDllSha256=(Get-FileHash -LiteralPath (Join-Path $payload 'lib\new-runtime.dll')).Hash}
}
[IO.File]::WriteAllText((Join-Path $PSScriptRoot ('install-payloads'+$Suffix+'.json')),(($inventory|ConvertTo-Json -Depth 5)+"`n"),$utf8)
[IO.File]::AppendAllText((Join-Path $repo 'PROJECT_STRUCTURE.md'),"`n- UPDATE-EMBED-01 隔离安装测试包组装返回码 0；两个包来自已通过的默认/兼容发布目录，空 Configurations，附一个实际 DLL 文件证明新增落盘，未上传公网。大小/哈希见 artifacts/runs/embedded_update_20261023/install-payloads$Suffix.json，命令 prepare-install-packages.ps1。`n",$utf8)
$inventory|ConvertTo-Json -Depth 5
