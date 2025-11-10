# Database Seeder Script
# Populates the backend database with initial test data
# Usage: 
#   .\scripts\seed-data.ps1 -BackendUrl "https://your-backend-url.azurecontainerapps.io/"
#   OR get URL from terraform: $url = terraform output -raw backend_url; .\scripts\seed-data.ps1 -BackendUrl $url

[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$BackendUrl
)

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "  Database Seeder" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# Ensure URL ends with /
if (-not $BackendUrl.EndsWith("/")) {
    $BackendUrl += "/"
}

# Get seeder path
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Split-Path -Parent $scriptDir
$seederPath = Join-Path $rootDir "src" | Join-Path -ChildPath "tools" | Join-Path -ChildPath "Seeder"

if (-not (Test-Path $seederPath)) {
    Write-Error "Seeder directory not found at: $seederPath"
    exit 1
}

# Navigate to seeder directory
Write-Host "Preparing seeder..." -ForegroundColor Yellow
$originalLocation = Get-Location
Set-Location $seederPath

try {
    # Check if seeder project exists
    if (-not (Test-Path "Seeder.csproj")) {
        Write-Error "Seeder.csproj not found in: $seederPath"
        Set-Location $originalLocation
        exit 1
    }
    
    # Set environment variable for seeder to use
    $env:SEEDER_BASEURL = $BackendUrl
    
    Write-Host "Running seeder against: $BackendUrl" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "--------------------------------------" -ForegroundColor DarkGray
    Write-Host ""
    
    # Run the seeder (build if needed)
    & dotnet run --configuration Release
    $exitCode = $LASTEXITCODE
    
    Write-Host ""
    Write-Host "--------------------------------------" -ForegroundColor DarkGray
    Write-Host ""
    
    # Clear environment variable
    Remove-Item Env:\SEEDER_BASEURL -ErrorAction SilentlyContinue
    
    if ($exitCode -eq 0) {
        Write-Host "Seeding completed successfully!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Your database is now populated with test data." -ForegroundColor Cyan
        Write-Host ""
    }
    else {
        Write-Host ""
        Write-Error "Seeding failed with exit code $exitCode"
        Set-Location $originalLocation
        exit $exitCode
    }
}
catch {
    Write-Host ""
    Write-Error "An error occurred: $_"
    Set-Location $originalLocation
    exit 1
}
finally {
    Set-Location $originalLocation
}

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Done!" -ForegroundColor Green
Write-Host ""
