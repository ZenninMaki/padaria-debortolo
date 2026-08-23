[CmdletBinding()]
param(
    [switch]$SkipFlutter
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$artifacts = Join-Path $root 'artifacts'
$serverOutput = Join-Path $artifacts 'server'
$desktopOutput = Join-Path $artifacts 'desktop'

Remove-Item -LiteralPath $artifacts -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $artifacts | Out-Null
New-Item -ItemType Directory -Path $serverOutput, $desktopOutput | Out-Null

Write-Host 'Publicando backend ASP.NET Core self-contained...'
dotnet publish (Join-Path $root 'InfiniteCoffee2\InfiniteCoffee2.csproj') `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --output $serverOutput

if (-not $SkipFlutter) {
    Write-Host 'Gerando aplicativo Flutter Desktop...'
    Push-Location (Join-Path $root 'InfiniteCoffeeMobile')
    try {
        flutter build windows --release
    }
    finally {
        Pop-Location
    }

    $flutterOutput = Join-Path $root 'InfiniteCoffeeMobile\build\windows\x64\runner\Release'
    Copy-Item -Path (Join-Path $flutterOutput '*') -Destination $desktopOutput -Recurse -Force
}

Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Start-PadariaDebortolo.cmd') -Destination $serverOutput -Force
Write-Host "Artefatos gerados em $artifacts"
