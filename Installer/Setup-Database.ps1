[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$InstallDir)

$ErrorActionPreference = 'Stop'
$candidates = @('localhost\KAIO', 'localhost\SQLEXPRESS', 'localhost', '(localdb)\MSSQLLocalDB')
$connection = $null

foreach ($server in $candidates) {
    $candidate = "Server=$server;Database=master;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=3;"
    try {
        $test = New-Object System.Data.SqlClient.SqlConnection $candidate
        $test.Open()
        $test.Close()
        $connection = $candidate
        break
    } catch { }
}

if ($null -eq $connection) {
    throw 'SQL Server nao encontrado. Instale o SQL Server Express e execute o instalador novamente.'
}

$master = New-Object System.Data.SqlClient.SqlConnection $connection
$master.Open()
$create = $master.CreateCommand()
$create.CommandText = "IF DB_ID(N'infiniteCoffee') IS NULL CREATE DATABASE infiniteCoffee;"
$create.ExecuteNonQuery() | Out-Null
$master.Close()

$databaseConnection = $connection.Replace('Database=master', 'Database=infiniteCoffee')
$sql = Get-Content -LiteralPath (Join-Path $InstallDir 'database\InstallDatabase.sql') -Raw
$db = New-Object System.Data.SqlClient.SqlConnection $databaseConnection
$db.Open()
$command = $db.CreateCommand()
$command.CommandTimeout = 120
foreach ($batch in [regex]::Split($sql, '(?=CREATE OR ALTER PROCEDURE)')) {
    if (-not [string]::IsNullOrWhiteSpace($batch)) {
        $command.CommandText = $batch
        $command.ExecuteNonQuery() | Out-Null
    }
}
$db.Close()

$config = @{ ConnectionStrings = @{ DefaultConnection = $databaseConnection } } | ConvertTo-Json -Depth 4
$configPath = Join-Path $InstallDir 'server\appsettings.Production.json'
Set-Content -LiteralPath $configPath -Value $config -Encoding UTF8
Write-Host "Banco infiniteCoffee configurado usando $($databaseConnection.Split(';')[0])."
