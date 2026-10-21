$ErrorActionPreference = 'Stop'

$runDirectory = $PSScriptRoot
$manifestUrl = 'https://raw.gitcode.com/childhood123/shahe-bulk-launcher-releases/raw/main/update.json'
$expectedPackageUrl = 'https://raw.gitcode.com/childhood123/shahe-bulk-launcher-releases/blobs/ab39c61fecdcd38d737ef615bb84fb2cbf055051/shahe-bulk-launcher-4.0.46-win-x64.zip'
$expectedSize = 2785203
$expectedSha256 = 'D6F248D7F8602C2235B5FB5318E592AA5F14E213069A095396D470E7DCC266F8'
$downloadPath = Join-Path $runDirectory 'gitcode-anonymous-package-final.zip'
$resultPath = Join-Path $runDirectory 'gitcode-anonymous-verification-final.json'
$attempts = @()
$verified = $false
$verifiedManifest = $null

for ($attempt = 1; $attempt -le 12 -and -not $verified; $attempt++) {
    $cacheBuster = [DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds()
    $manifestRequestUrl = "${manifestUrl}?t=$cacheBuster"
    $attemptResult = [ordered]@{
        attempt = $attempt
        started_at = [DateTimeOffset]::UtcNow.ToString('o')
        manifest_url = $manifestRequestUrl
    }

    try {
        $manifestResponse = Invoke-WebRequest `
            -Uri $manifestRequestUrl `
            -UserAgent 'SandboxBatchLauncher/4.0.46' `
            -Headers @{
                Accept = 'application/json'
                'Cache-Control' = 'no-cache, no-store, max-age=0'
                Pragma = 'no-cache'
            } `
            -SkipHttpErrorCheck `
            -TimeoutSec 20

        $attemptResult.manifest_status = [int]$manifestResponse.StatusCode
        if ($manifestResponse.StatusCode -ne 200) {
            $attemptResult.error = "Manifest returned HTTP $($manifestResponse.StatusCode)."
        }
        else {
            $manifest = $manifestResponse.Content | ConvertFrom-Json
            $package = @($manifest.packages)[0]
            $attemptResult.latest_version = $manifest.latest_version
            $attemptResult.package_url = $package.url
            $attemptResult.manifest_size = [int64]$package.size
            $attemptResult.manifest_sha256 = [string]$package.sha256

            if ($manifest.latest_version -ne '4.0.46' -or
                $package.url -ne $expectedPackageUrl -or
                [int64]$package.size -ne $expectedSize -or
                [string]$package.sha256 -ne $expectedSha256) {
                $attemptResult.error = 'Manifest values do not match the prepared release.'
            }
            else {
                $packageRequestUrl = "$($package.url)?t=$cacheBuster"
                $attemptResult.package_request_url = $packageRequestUrl
                $packageResponse = Invoke-WebRequest `
                    -Uri $packageRequestUrl `
                    -UserAgent 'SandboxBatchLauncher/4.0.46' `
                    -Headers @{
                        Accept = 'application/octet-stream'
                        'Cache-Control' = 'no-cache, no-store, max-age=0'
                        Pragma = 'no-cache'
                    } `
                    -OutFile $downloadPath `
                    -PassThru `
                    -SkipHttpErrorCheck `
                    -TimeoutSec 30

                $attemptResult.package_status = [int]$packageResponse.StatusCode
                if ($packageResponse.StatusCode -ne 200) {
                    $attemptResult.error = "Package returned HTTP $($packageResponse.StatusCode)."
                }
                else {
                    $actualSize = (Get-Item -LiteralPath $downloadPath).Length
                    $actualSha256 = (Get-FileHash -LiteralPath $downloadPath -Algorithm SHA256).Hash
                    $attemptResult.downloaded_size = $actualSize
                    $attemptResult.downloaded_sha256 = $actualSha256
                    $verified = $actualSize -eq $expectedSize -and $actualSha256 -eq $expectedSha256
                    if (-not $verified) {
                        $attemptResult.error = 'Downloaded package size or SHA-256 does not match.'
                    }
                    else {
                        $verifiedManifest = $manifest
                    }
                }
            }
        }
    }
    catch {
        $attemptResult.error = $_.Exception.Message
    }

    $attemptResult.completed_at = [DateTimeOffset]::UtcNow.ToString('o')
    $attempts += [pscustomobject]$attemptResult
    if (-not $verified -and $attempt -lt 12) {
        Start-Sleep -Seconds 5
    }
}

$remoteHeadLine = git ls-remote 'https://gitcode.com/childhood123/shahe-bulk-launcher-releases.git' 'refs/heads/main'
$remoteHead = (($remoteHeadLine -split '\s+')[0]).Trim()
$finalAttempt = $attempts[-1]
$result = [ordered]@{
    verified = $verified
    verified_at = [DateTimeOffset]::UtcNow.ToString('o')
    gitcode_remote = 'https://gitcode.com/childhood123/shahe-bulk-launcher-releases.git'
    remote_branch = 'main'
    remote_head = $remoteHead
    manifest_url = $manifestUrl
    manifest_status = $finalAttempt.manifest_status
    latest_version = $verifiedManifest.latest_version
    package_url = $finalAttempt.package_url
    package_status = $finalAttempt.package_status
    expected_size = $expectedSize
    downloaded_size = $finalAttempt.downloaded_size
    expected_sha256 = $expectedSha256
    downloaded_sha256 = $finalAttempt.downloaded_sha256
    attempts = $attempts
}

$result | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $resultPath -Encoding utf8
$result | ConvertTo-Json -Depth 8

if (-not $verified) {
    exit 1
}
