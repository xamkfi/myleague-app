<#
.SYNOPSIS
    Builds and deploys the MyLeague .NET backend to an Azure App Service.

.DESCRIPTION
    Interactive deployment script that:
    1. Builds and publishes the .NET API project
    2. Creates a zip package
    3. Lists available App Services and lets you pick one (or accepts a name)
    4. Deploys via az webapp deploy
    5. Optionally runs EF Core database migrations

    No infrastructure changes - just app build and deploy.
    Run infra/provision/provision-backend.ps1 first to create the App Service.

.PARAMETER Environment
    The environment to deploy to (dev, staging, prod). Default: dev

.PARAMETER ResourceGroupName
    Override the resource group name. Default: myleague-{Environment}-rg

.PARAMETER AppServiceName
    The App Service to deploy to. If not provided, available services will be listed.

.PARAMETER SkipLogin
    Skip the Azure login check (use if already logged in).

.PARAMETER SkipMigrations
    Skip the database migration prompt.

.PARAMETER RunMigrations
    Automatically run database migrations without prompting.

.EXAMPLE
    .\deploy-backend.ps1
    # Fully interactive - asks all questions

.EXAMPLE
    .\deploy-backend.ps1 -Environment dev -AppServiceName myleague-dev-api -SkipLogin
    # Non-interactive deployment to a specific App Service
#>

param(
    [Parameter()]
    [ValidateSet('dev', 'staging', 'prod')]
    [string]$Environment = 'dev',

    [Parameter()]
    [string]$ResourceGroupName,

    [Parameter()]
    [string]$AppServiceName,

    [Parameter()]
    [switch]$SkipLogin,

    [Parameter()]
    [switch]$SkipMigrations,

    [Parameter()]
    [switch]$RunMigrations
)

$ErrorActionPreference = 'Stop'

# Helper functions
function Write-Step { param($Message) Write-Host "`n>> $Message" -ForegroundColor Cyan }
function Write-Success { param($Message) Write-Host "[OK] $Message" -ForegroundColor Green }
function Write-WarningMsg { param($Message) Write-Host "[!] $Message" -ForegroundColor Yellow }
function Write-ErrorMsg { param($Message) Write-Host "[X] $Message" -ForegroundColor Red }

Write-Host @"

================================================================
        MyLeague Backend Deployment
================================================================

"@ -ForegroundColor Magenta

Write-Host "Environment: $Environment"
Write-Host ""

# Set default resource group name
if (-not $ResourceGroupName) {
    $ResourceGroupName = "myleague-$Environment-rg"
}

# ============================================================================
# Prerequisites Check
# ============================================================================

Write-Step "Checking prerequisites..."

# Check Azure CLI
if (Get-Command az -ErrorAction SilentlyContinue) {
    Write-Success "Azure CLI available"
}
else {
    Write-ErrorMsg "Azure CLI is not installed. Please install from: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
    exit 1
}

# Check .NET SDK
if (Get-Command dotnet -ErrorAction SilentlyContinue) {
    $dotnetVersion = dotnet --version
    Write-Success ".NET SDK available: $dotnetVersion"
}
else {
    Write-ErrorMsg ".NET SDK is not installed. Please install from: https://dotnet.microsoft.com/download"
    exit 1
}

# ============================================================================
# Azure Login
# ============================================================================

if (-not $SkipLogin) {
    Write-Step "Checking Azure login status..."

    $account = az account show 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-WarningMsg "Not logged in to Azure. Starting login..."
        az login
        if ($LASTEXITCODE -ne 0) {
            Write-ErrorMsg "Azure login failed"
            exit 1
        }
    }

    $accountInfo = az account show --query "{name:name, id:id}" -o json | ConvertFrom-Json
    Write-Success "Logged in to subscription: $($accountInfo.name)"
}

# ============================================================================
# Select App Service
# ============================================================================

Write-Step "Selecting deployment target..."

if (-not $AppServiceName) {
    Write-Host "  Searching for App Services in resource group '$ResourceGroupName'..." -ForegroundColor Gray

    $appListJson = az webapp list `
        --resource-group $ResourceGroupName `
        --query "[].{name:name, resourceGroup:resourceGroup, defaultHostName:defaultHostName, state:state}" `
        -o json 2>$null

    if ($LASTEXITCODE -ne 0 -or -not $appListJson) {
        Write-ErrorMsg "Failed to list App Services. Make sure resource group '$ResourceGroupName' exists."
        exit 1
    }

    $appList = $appListJson | ConvertFrom-Json

    if ($appList.Count -eq 0) {
        Write-ErrorMsg "No App Services found in resource group '$ResourceGroupName'."
        Write-ErrorMsg "Run infra\provision\provision-backend.ps1 first to create one."
        exit 1
    }

    Write-Host ""
    Write-Host "  Available App Services:" -ForegroundColor White
    Write-Host "  -----------------------" -ForegroundColor Gray

    for ($i = 0; $i -lt $appList.Count; $i++) {
        $app = $appList[$i]
        Write-Host "  [$($i + 1)] " -NoNewline -ForegroundColor Yellow
        Write-Host "$($app.name)" -NoNewline -ForegroundColor White
        Write-Host "  (https://$($app.defaultHostName))" -NoNewline -ForegroundColor Gray
        Write-Host "  [$($app.state)]" -ForegroundColor DarkGray
    }

    Write-Host ""

    if ($appList.Count -eq 1) {
        $selectedIndex = 0
        Write-Host "  Only one App Service found, auto-selecting: $($appList[0].name)" -ForegroundColor Gray
    }
    else {
        $selection = Read-Host "  Select an App Service (1-$($appList.Count))"
        $selectedIndex = [int]$selection - 1

        if ($selectedIndex -lt 0 -or $selectedIndex -ge $appList.Count) {
            Write-ErrorMsg "Invalid selection."
            exit 1
        }
    }

    $selectedApp = $appList[$selectedIndex]
    $AppServiceName = $selectedApp.name

    Write-Success "Selected: $AppServiceName (https://$($selectedApp.defaultHostName))"
}
else {
    Write-Success "Target: $AppServiceName"
}

# ============================================================================
# Build Application
# ============================================================================

Write-Step "Building .NET application..."

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$infraDir = Split-Path -Parent $scriptDir
$repoRoot = Split-Path -Parent $infraDir
$webApiPath = Join-Path $repoRoot "src\backend\WebAPI"
$publishPath = Join-Path $webApiPath "publish"
$zipPath = Join-Path $webApiPath "app.zip"

if (-not (Test-Path $webApiPath)) {
    Write-ErrorMsg "WebAPI project not found at: $webApiPath"
    exit 1
}

# Clean previous publish output
if (Test-Path $publishPath) {
    Remove-Item -Recurse -Force $publishPath
}
if (Test-Path $zipPath) {
    Remove-Item -Force $zipPath
}

# Publish
Write-Host "  Publishing Release build..." -ForegroundColor Gray
Push-Location $webApiPath

dotnet publish -c Release -o ./publish --nologo
if ($LASTEXITCODE -ne 0) {
    Pop-Location
    Write-ErrorMsg "Build failed"
    exit 1
}
Write-Success "Build complete"

# Create zip
Write-Host "  Creating deployment package..." -ForegroundColor Gray
Compress-Archive -Path ./publish/* -DestinationPath ./app.zip -Force
Write-Success "Package created: app.zip"

Pop-Location

# ============================================================================
# Confirm Deployment
# ============================================================================

Write-Host ""
Write-Host "================================================================" -ForegroundColor Yellow
Write-Host "  Deployment Summary" -ForegroundColor Yellow
Write-Host "================================================================" -ForegroundColor Yellow
Write-Host ""
Write-Host "  Target App Service: $AppServiceName" -ForegroundColor White
Write-Host "  Resource Group:     $ResourceGroupName" -ForegroundColor White
Write-Host "  Environment:        $Environment" -ForegroundColor White
Write-Host ""
Write-Host "================================================================" -ForegroundColor Yellow
Write-Host ""

$confirmDeploy = Read-Host "  Proceed with deployment? (Y/n)"
if ($confirmDeploy -eq 'n' -or $confirmDeploy -eq 'N') {
    Write-WarningMsg "Deployment cancelled."
    exit 0
}

# ============================================================================
# Deploy Application
# ============================================================================

Write-Step "Deploying to Azure App Service '$AppServiceName'..."

az webapp deploy `
    --resource-group $ResourceGroupName `
    --name $AppServiceName `
    --src-path $zipPath `
    --type zip `
    --output none

if ($LASTEXITCODE -ne 0) {
    Write-ErrorMsg "Deployment failed"
    exit 1
}

Write-Success "Application deployed"

# ============================================================================
# Database Migrations (optional)
# ============================================================================

if (-not $SkipMigrations) {
    Write-Step "Database Migrations"

    $shouldMigrate = $false

    if ($RunMigrations) {
        $shouldMigrate = $true
    }
    else {
        Write-Host ""
        Write-Host "  Would you like to run EF Core database migrations?" -ForegroundColor White
        Write-Host "  This will apply any pending migrations to the Azure database." -ForegroundColor Gray
        Write-Host ""

        $migrateChoice = Read-Host "  Run migrations? (y/N)"
        if ($migrateChoice -eq 'y' -or $migrateChoice -eq 'Y') {
            $shouldMigrate = $true
        }
    }

    if ($shouldMigrate) {
        Write-Host ""
        Write-Host "  To run migrations, you need the database connection string." -ForegroundColor Yellow
        Write-Host "  Format: Host=<fqdn>;Database=myleague;Username=<user>;Password=<pass>;SSL Mode=Require;Trust Server Certificate=true" -ForegroundColor Gray
        Write-Host ""

        $connString = Read-Host "  Enter PostgreSQL connection string"

        if ($connString) {
            Write-Host "  Running migrations..." -ForegroundColor Gray

            $env:ConnectionStrings__DefaultConnection = $connString

            Push-Location $webApiPath
            dotnet ef database update --project ../Infrastructure/Infrastructure.csproj --no-build 2>&1
            $migrationResult = $LASTEXITCODE

            # Try with build if no-build fails
            if ($migrationResult -ne 0) {
                Write-WarningMsg "Retrying with build..."
                dotnet ef database update --project ../Infrastructure/Infrastructure.csproj
                $migrationResult = $LASTEXITCODE
            }
            Pop-Location

            $env:ConnectionStrings__DefaultConnection = $null

            if ($migrationResult -eq 0) {
                Write-Success "Migrations applied successfully"
            }
            else {
                Write-ErrorMsg "Migration failed. You can run manually later."
            }
        }
        else {
            Write-WarningMsg "No connection string provided, skipping migrations."
        }
    }
    else {
        Write-Host "  Skipping migrations." -ForegroundColor Gray
    }
}

# ============================================================================
# Health Check
# ============================================================================

Write-Step "Checking deployment health..."

$appUrl = "https://$AppServiceName.azurewebsites.net"
$healthUrl = "$appUrl/health/ready"

Write-Host "  Waiting for app to start..." -ForegroundColor Gray
Start-Sleep -Seconds 10

try {
    $healthResponse = Invoke-WebRequest -Uri $healthUrl -UseBasicParsing -TimeoutSec 30
    if ($healthResponse.StatusCode -eq 200) {
        Write-Success "Health check passed: $healthUrl"
    }
    else {
        Write-WarningMsg "Health check returned status $($healthResponse.StatusCode)"
    }
}
catch {
    Write-WarningMsg "Health check failed (app may still be starting): $healthUrl"
    Write-Host "  Try again in a minute: curl $healthUrl" -ForegroundColor Gray
}

# ============================================================================
# Clean Up
# ============================================================================

# Remove publish artifacts
if (Test-Path $publishPath) {
    Remove-Item -Recurse -Force $publishPath -ErrorAction SilentlyContinue
}
if (Test-Path $zipPath) {
    Remove-Item -Force $zipPath -ErrorAction SilentlyContinue
}

# ============================================================================
# Done
# ============================================================================

Write-Host @"

================================================================
  Deployment Complete!
================================================================

  App Service:     $AppServiceName
  URL:             $appUrl
  Health Check:    $healthUrl
  API Docs:        $appUrl/scalar/v1

  View logs:
  az webapp log tail --resource-group $ResourceGroupName --name $AppServiceName

================================================================

"@ -ForegroundColor Green

Write-Host "Deployment completed at $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray
