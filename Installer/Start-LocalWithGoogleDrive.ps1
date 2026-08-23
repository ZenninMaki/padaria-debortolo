[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$CredentialsPath,
    [Parameter(Mandatory = $true)]
    [string]$FolderId,
    [string]$ApiToken = '',
    [string]$ReadOnlyToken = ''
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
if (-not (Test-Path -LiteralPath $CredentialsPath)) {
    throw "Arquivo de credenciais não encontrado: $CredentialsPath"
}

$env:GOOGLE_DRIVE_OAUTH_CLIENT_PATH = (Resolve-Path -LiteralPath $CredentialsPath).Path
$env:GOOGLE_DRIVE_FOLDER_ID = $FolderId
$env:GOOGLE_DRIVE_SNAPSHOT_NAME = 'estoque.json'
$env:PADARIA_API_TOKEN = $ApiToken
$env:PADARIA_READONLY_TOKEN = $ReadOnlyToken

dotnet run --project (Join-Path $root 'InfiniteCoffee2\InfiniteCoffee2.csproj') --launch-profile http
