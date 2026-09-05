# ========================================================================================
# ERPInfinity - Automated Service Database Initialization Script (PowerShell)
# Usage: .\scripts\Create_ERP_Databases.ps1 -SqlServerInstance "localhost" -MongoUri "mongodb://localhost:27017"
# ========================================================================================

param (
    [string]$SqlServerInstance = "localhost",
    [string]$MongoUri = "mongodb://localhost:27017"
)

$ErrorActionPreference = "Stop"

Write-Host "==========================================================================" -ForegroundColor Cyan
Write-Host " ERPInfinity Microservices Database Creator Script " -ForegroundColor Yellow
Write-Host "==========================================================================" -ForegroundColor Cyan

# 1. SQL Server Database Creation
Write-Host "`n[1/2] Connecting to SQL Server ($SqlServerInstance) to create service databases..." -ForegroundColor Green

$sqlScriptPath = Join-Path $PSScriptRoot "Create_All_Services_Databases.sql"

if (Test-Path $sqlScriptPath) {
    try {
        if (Get-Command sqlcmd -ErrorAction SilentlyContinue) {
            sqlcmd -S $SqlServerInstance -E -i "$sqlScriptPath"
            Write-Host "✓ SQL Server Databases created successfully using sqlcmd!" -ForegroundColor Green
        }
        else {
            Write-Host "! sqlcmd tool not found in PATH. Executing via .NET System.Data.SqlClient..." -ForegroundColor Yellow
            $sqlContent = Get-Content $sqlScriptPath -Raw
            $sqlStatements = $sqlContent -split "(?i)\r?\nGO\r?\n"
            
            $connectionString = "Server=$SqlServerInstance;Database=master;Integrated Security=True;TrustServerCertificate=True;"
            $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
            $connection.Open()

            foreach ($stmt in $sqlStatements) {
                if (-not [string]::IsNullOrWhiteSpace($stmt)) {
                    $cmd = $connection.CreateCommand()
                    $cmd.CommandText = $stmt
                    $cmd.ExecuteNonQuery() | Out-Null
                }
            }
            $connection.Close()
            Write-Host "✓ SQL Server Databases created successfully via .NET SqlClient!" -ForegroundColor Green
        }
    }
    catch {
        Write-Host "X Failed to execute SQL Server script: $_" -ForegroundColor Red
    }
}
else {
    Write-Host "X SQL script file not found at: $sqlScriptPath" -ForegroundColor Red
}

# 2. MongoDB Collection Setup
Write-Host "`n[2/2] Connecting to MongoDB ($MongoUri) to initialize CQRS read projections..." -ForegroundColor Green

$mongoScriptPath = Join-Path $PSScriptRoot "Create_All_MongoDB_Collections.js"

if (Test-Path $mongoScriptPath) {
    if (Get-Command mongosh -ErrorAction SilentlyContinue) {
        mongosh "$MongoUri" "$mongoScriptPath"
        Write-Host "✓ MongoDB CQRS collections initialized successfully using mongosh!" -ForegroundColor Green
    }
    elseif (Get-Command mongo -ErrorAction SilentlyContinue) {
        mongo "$MongoUri" "$mongoScriptPath"
        Write-Host "✓ MongoDB CQRS collections initialized successfully using legacy mongo CLI!" -ForegroundColor Green
    }
    else {
        Write-Host "! Neither 'mongosh' nor 'mongo' CLI was found in PATH." -ForegroundColor Yellow
        Write-Host "  Please run MongoDB script manually using mongosh or MongoDB Compass:" -ForegroundColor Yellow
        Write-Host "  File location: $mongoScriptPath" -ForegroundColor LightGray
    }
}

Write-Host "`n==========================================================================" -ForegroundColor Cyan
Write-Host " ERPInfinity Database Provisioning Complete! " -ForegroundColor Green
Write-Host "==========================================================================" -ForegroundColor Cyan
