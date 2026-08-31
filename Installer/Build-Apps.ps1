[CmdletBinding()]
param(
    [string]$ApiBaseUrl = 'http://192.168.1.101:5049',
    [switch]$SkipWindows,
    [switch]$SkipAndroid
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$mobile = Join-Path $root 'InfiniteCoffeeMobile'
$mobileDefines = @("--dart-define=API_BASE_URL=$ApiBaseUrl")
$desktopDefines = @('--dart-define=API_BASE_URL=http://localhost:5049')

$readOnlyToken = $env:PADARIA_READONLY_TOKEN
$writeToken = $env:PADARIA_MOBILE_WRITE_TOKEN
if (-not [string]::IsNullOrWhiteSpace($readOnlyToken)) {
    $mobileDefines += "--dart-define=API_ACCESS_TOKEN=$readOnlyToken"
    $desktopDefines += "--dart-define=API_ACCESS_TOKEN=$readOnlyToken"
}
if (-not [string]::IsNullOrWhiteSpace($writeToken)) {
    $mobileDefines += "--dart-define=API_WRITE_TOKEN=$writeToken"
    $desktopDefines += "--dart-define=API_WRITE_TOKEN=$writeToken"
}

Push-Location $mobile
try {
    if (-not $SkipAndroid) {
        Write-Host 'Gerando APK...'
        & flutter build apk --release @mobileDefines
    }
    if (-not $SkipWindows) {
        Write-Host 'Gerando aplicativo Windows...'
        & flutter build windows --release @desktopDefines
    }
}
finally {
    Pop-Location
}

Write-Host 'Build concluido.'
