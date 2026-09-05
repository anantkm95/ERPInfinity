# PowerShell script to map each microservice appsettings.json to its corresponding database
$services = @(
    @{ Name="Customer"; Db="Db_Customer" },
    @{ Name="Finance"; Db="Db_Finance" },
    @{ Name="Identity"; Db="Db_Identity" },
    @{ Name="Inventory"; Db="Db_Inventory" },
    @{ Name="Notification"; Db="Db_Notification" },
    @{ Name="Order"; Db="Db_Order" },
    @{ Name="Payment"; Db="Db_Payment" },
    @{ Name="Pricing"; Db="Db_Pricing" },
    @{ Name="Product"; Db="Db_Product" },
    @{ Name="Purchase"; Db="Db_Purchase" },
    @{ Name="Reporting"; Db="Db_Reporting" },
    @{ Name="Sales"; Db="Db_Sales" },
    @{ Name="Store"; Db="Db_Store" },
    @{ Name="Warehouse"; Db="Db_Warehouse" }
)

foreach ($s in $services) {
    $serviceName = $s.Name
    $dbName = $s.Db
    $filePath = "d:\Live Project's\ERP\src\Services\" + $serviceName + "\ERPInfinity." + $serviceName + ".API\appsettings.json"
    
    if (Test-Path $filePath) {
        $json = Get-Content $filePath -Raw | ConvertFrom-Json
        
        # Ensure ConnectionStrings object exists
        if (-not $json.ConnectionStrings) {
            $json | Add-Member -MemberType NoteProperty -Name "ConnectionStrings" -Value ([pscustomobject]@{})
        }
        
        $connString = "Server=.\MSSQLSERVER01;Database=" + $dbName + ";Trusted_Connection=True;TrustServerCertificate=True;"
        $json.ConnectionStrings | Add-Member -MemberType NoteProperty -Name "DefaultConnection" -Value $connString -Force
        
        # Add MongoDbSettings for CQRS Read Projections
        if (-not $json.MongoDbSettings) {
            $mongoObj = [pscustomobject]@{
                ConnectionString = "mongodb://localhost:27017"
                DatabaseName = "ERPInfinity_CQRSRead"
            }
            $json | Add-Member -MemberType NoteProperty -Name "MongoDbSettings" -Value $mongoObj -Force
        }
        
        $json | ConvertTo-Json -Depth 10 | Set-Content $filePath
        Write-Host ("✓ Mapped ERPInfinity." + $serviceName + ".API -> Database=" + $dbName) -ForegroundColor Green
    } else {
        Write-Host ("X Could not find " + $filePath) -ForegroundColor Red
    }
}
