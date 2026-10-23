$ErrorActionPreference = 'Stop'
$repo = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..\..'))
$utf8 = New-Object Text.UTF8Encoding($false)
$staging = Join-Path $PSScriptRoot 'staging'
[IO.Directory]::CreateDirectory($staging) | Out-Null

function Read-HeadFile([string]$path) {
    $start = New-Object Diagnostics.ProcessStartInfo
    $start.FileName = 'git'
    $start.Arguments = 'show "HEAD:' + $path + '"'
    $start.WorkingDirectory = $repo
    $start.UseShellExecute = $false
    $start.CreateNoWindow = $true
    $start.RedirectStandardOutput = $true
    $start.RedirectStandardError = $true
    $start.StandardOutputEncoding = $utf8
    $process = [Diagnostics.Process]::Start($start)
    $content = $process.StandardOutput.ReadToEnd()
    $errorText = $process.StandardError.ReadToEnd()
    $process.WaitForExit()
    if ($process.ExitCode -ne 0) { throw $errorText }
    $process.Dispose()
    return $content
}

$planPath = 'docs/superpowers/plans/2026-10-20-remaining-migration-closure.md'
$plan = [IO.File]::ReadAllText((Join-Path $repo $planPath))
$marker = '### FIX-CONFIG-01：配置读取失败时保护原文件'
$position = $plan.IndexOf($marker, [StringComparison]::Ordinal)
if ($position -lt 0) { throw 'Approved plan section is missing' }
$planSection = $plan.Substring($position).Replace("`r`n", "`n")
$planHead = Read-HeadFile $planPath
if ($planHead.Contains($marker)) { throw 'This preparation is only for the initial feature commit' }
[IO.File]::WriteAllText((Join-Path $staging 'plan.md'), ($planHead.TrimEnd() + "`n`n" + $planSection), $utf8)

$structure = [IO.File]::ReadAllText((Join-Path $repo 'PROJECT_STRUCTURE.md')).Replace("`r`n", "`n")
$summary = @($structure.Split("`n") | Where-Object { $_.StartsWith('- 用户最新范围与 FIX-CONFIG-01：') })
$records = @($structure.Split("`n") | Where-Object { $_.StartsWith('- FIX-CONFIG-01 ') })
if ($summary.Count -ne 1 -or $records.Count -lt 7) { throw 'Feature summary or verification records are incomplete' }
$structureHead = Read-HeadFile 'PROJECT_STRUCTURE.md'
$summaryPosition = $structureHead.IndexOf('- 当前唯一计划：', [StringComparison]::Ordinal)
if ($summaryPosition -lt 0) { throw 'Current summary anchor is missing' }
$structureIndex = $structureHead.Insert($summaryPosition, $summary[0] + "`n")
$structureIndex = $structureIndex.TrimEnd() + "`n`n" + ($records -join "`n`n") + "`n"
[IO.File]::WriteAllText((Join-Path $staging 'PROJECT_STRUCTURE.md'), $structureIndex, $utf8)

$prefix = 'artifacts/runs/configuration_failure_safety_20261023/'
$files = @(
    'csharp/ShaheLauncher.Core/Configuration/ConfigStore.cs',
    'csharp/ShaheLauncher.Tests/ConfigurationFailureSafetyContractTests.cs',
    'docs/superpowers/specs/2026-10-23-configuration-failure-safety-design.md',
    'PROJECT_STRUCTURE.md',
    $planPath
)
$files += @(Get-ChildItem -LiteralPath $PSScriptRoot -File | Where-Object {
    $_.Extension -in @('.ps1', '.md', '.json', '.log', '.trx')
} | ForEach-Object { $prefix + $_.Name })
$files += $prefix + 'staging-files.txt'
$files = @($files | Sort-Object -Unique)
[IO.File]::WriteAllText((Join-Path $PSScriptRoot 'staging-files.txt'), (($files -join "`n") + "`n"), $utf8)
$files

# Preparation only: the working copies and Git index are not modified here.
