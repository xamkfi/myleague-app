<#
.SYNOPSIS
    Provisions MyLeague backend infrastructure in Azure.

.DESCRIPTION
    This script provisions Azure infrastructure for the MyLeague backend:
    App Service Plan, App Service, PostgreSQL Flexible Server, and Storage Account.
    This does NOT deploy the application code - use the deploy scripts for that.

.PARAMETER Environment
    The environment to provision (staging, prod). Default: staging

.PARAMETER Location
    The Azure region for resources. Default: westeurope

.PARAMETER ResourceGroupName
    Override the resource group name. Default: myleague-{Environment}-rg

.PARAMETER PostgresPassword
    The PostgreSQL admin password. If not provided, you will be prompted.

.PARAMETER JwtSecretKey
    The JWT secret key for signing tokens. Must be at least 32 characters.
    If not provided, you will be prompted.

.PARAMETER SeedAdminEmail
    The admin email for database seeding (optional).
    If set, an admin user with this email is created on first startup.

.PARAMETER AlertEmail
    The admin email that receives monitoring alerts (health, errors, DB, cost).
    If not provided, you will be prompted. Leave empty to skip deploying alerts.

.PARAMETER SkipLogin
    Skip the Azure login check (use if already logged in).

.EXAMPLE
    .\provision-backend.ps1
    # Interactive provisioning with prompts (staging)

.EXAMPLE
    .\provision-backend.ps1 -Environment prod -AlertEmail admin@example.com
    # Provision production with monitoring alerts to admin@example.com
#>

param(
    [Parameter()]
    [ValidateSet('staging', 'prod')]
    [string]$Environment = 'staging',

    [Parameter()]
    [string]$Location = 'westeurope',

    [Parameter()]
    [string]$ResourceGroupName,

    [Parameter()]
    [string]$PostgresPassword,

    [Parameter()]
    [string]$JwtSecretKey,

    [Parameter()]
    [string]$SeedAdminEmail,

    [Parameter()]
    [string]$AlertEmail,

    [Parameter()]
    [switch]$SkipLogin
)

# Script configuration
$ErrorActionPreference = 'Stop'
$InformationPreference = 'Continue'

# Colors for output
function Write-Step { param($Message) Write-Host "`n>> $Message" -ForegroundColor Cyan }
function Write-Success { param($Message) Write-Host "[OK] $Message" -ForegroundColor Green }
function Write-WarningMsg { param($Message) Write-Host "[!] $Message" -ForegroundColor Yellow }
function Write-ErrorMsg { param($Message) Write-Host "[X] $Message" -ForegroundColor Red }

# Banner
Write-Host @"

================================================================
     MyLeague Backend Infrastructure Provisioning
================================================================

"@ -ForegroundColor Magenta

Write-Host "Environment: $Environment"
Write-Host "Location:    $Location"
Write-Host ""

# Set default resource group name if not provided
if (-not $ResourceGroupName) {
    $ResourceGroupName = "myleague-$Environment-rg"
}

# ============================================================================
# Prerequisites Check
# ============================================================================

Write-Step "Checking prerequisites..."

# Check Azure CLI
if (Get-Command az -ErrorAction SilentlyContinue) {
    $azVersion = az --version 2>&1 | Select-Object -First 1
    Write-Success "Azure CLI installed: $azVersion"
}
else {
    Write-ErrorMsg "Azure CLI is not installed. Please install from: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
    exit 1
}

# Check Bicep
$bicepCheck = az bicep version 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Success "Bicep CLI available"
}
else {
    Write-WarningMsg "Bicep not found, installing..."
    az bicep install
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Bicep installed"
    }
    else {
        Write-ErrorMsg "Failed to install Bicep"
        exit 1
    }
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
    
    # Confirm subscription
    $confirm = Read-Host "Use this subscription? (Y/n)"
    if ($confirm -eq 'n' -or $confirm -eq 'N') {
        Write-Host "`nAvailable subscriptions:" -ForegroundColor Yellow
        az account list --query "[].{Name:name, ID:id, IsDefault:isDefault}" -o table
        
        $subId = Read-Host "`nEnter subscription ID to use"
        az account set --subscription $subId
        if ($LASTEXITCODE -ne 0) {
            Write-ErrorMsg "Failed to set subscription"
            exit 1
        }
        Write-Success "Switched to subscription: $subId"
    }
}

# ============================================================================
# Get PostgreSQL Password
# ============================================================================

Write-Step "Configuring deployment parameters..."

if (-not $PostgresPassword) {
    Write-Host ""
    Write-Host "PostgreSQL Admin Password Requirements:" -ForegroundColor Yellow
    Write-Host "  - Minimum 8 characters"
    Write-Host "  - Must contain uppercase, lowercase, numbers"
    Write-Host "  - Avoid special characters that need escaping"
    Write-Host ""
    
    $securePassword = Read-Host "Enter PostgreSQL admin password" -AsSecureString
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
    $PostgresPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
    [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($BSTR)
    
    if ($PostgresPassword.Length -lt 8) {
        Write-ErrorMsg "Password must be at least 8 characters"
        exit 1
    }
}

# Get JWT Secret Key
if (-not $JwtSecretKey) {
    Write-Host ""
    Write-Host "JWT Secret Key Requirements:" -ForegroundColor Yellow
    Write-Host "  - Minimum 32 characters"
    Write-Host "  - Used for HMAC-SHA256 token signing"
    Write-Host "  - Must be kept secret and unique per environment"
    Write-Host ""
    
    $secureJwt = Read-Host "Enter JWT secret key" -AsSecureString
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($secureJwt)
    $JwtSecretKey = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
    [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($BSTR)
    
    if ($JwtSecretKey.Length -lt 32) {
        Write-ErrorMsg "JWT secret key must be at least 32 characters"
        exit 1
    }
}

# Get Seed Admin Email (optional)
if (-not $SeedAdminEmail) {
    Write-Host ""
    Write-Host "Admin Seed Email (optional):" -ForegroundColor Yellow
    Write-Host "  - If set, an admin user with this email is created on first startup"
    Write-Host "  - Leave empty to skip"
    Write-Host ""
    
    $SeedAdminEmail = Read-Host "Enter admin email (or press Enter to skip)"
}

# Get Alert Email (optional but recommended)
if (-not $AlertEmail) {
    Write-Host ""
    Write-Host "Monitoring Alert Email (recommended):" -ForegroundColor Yellow
    Write-Host "  - Receives automatic alerts: app down, HTTP 5xx spikes, slow responses,"
    Write-Host "    CPU/memory pressure, database issues, and cost budget warnings"
    Write-Host "  - Leave empty to skip deploying monitoring alerts"
    Write-Host ""

    $AlertEmail = Read-Host "Enter alert email (or press Enter to skip)"
}

Write-Success "Parameters configured"

# ============================================================================
# Create Resource Group
# ============================================================================

Write-Step "Creating resource group '$ResourceGroupName' in '$Location'..."

$rgExists = az group exists --name $ResourceGroupName
if ($rgExists -eq 'true') {
    Write-WarningMsg "Resource group already exists"
}
else {
    az group create --name $ResourceGroupName --location $Location --output none
    if ($LASTEXITCODE -ne 0) {
        Write-ErrorMsg "Failed to create resource group"
        exit 1
    }
    Write-Success "Resource group created"
}

# ============================================================================
# Deploy Infrastructure
# ============================================================================

Write-Step "Provisioning infrastructure (this may take 5-10 minutes)..."

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$templateFile = Join-Path $scriptDir "backend.bicep"
$parametersFile = Join-Path $scriptDir "backend.$Environment.bicepparam"

if (-not (Test-Path $parametersFile)) {
    Write-ErrorMsg "Parameter file not found: $parametersFile"
    exit 1
}

# Validate template first
Write-Host "  Validating template..." -ForegroundColor Gray
az deployment group validate `
    --resource-group $ResourceGroupName `
    --template-file $templateFile `
    --parameters $parametersFile `
    --parameters postgresAdminPassword=$PostgresPassword `
    --parameters jwtSecretKey=$JwtSecretKey `
    --parameters seedAdminEmail=$SeedAdminEmail `
    --parameters alertEmail=$AlertEmail `
    --parameters location=$Location `
    --parameters environmentName=$Environment `
    --output none

if ($LASTEXITCODE -ne 0) {
    Write-ErrorMsg "Template validation failed"
    exit 1
}
Write-Host "  Template validated" -ForegroundColor Gray

# Deploy
Write-Host "  Deploying resources..." -ForegroundColor Gray
$deploymentOutput = az deployment group create `
    --resource-group $ResourceGroupName `
    --template-file $templateFile `
    --parameters $parametersFile `
    --parameters postgresAdminPassword=$PostgresPassword `
    --parameters jwtSecretKey=$JwtSecretKey `
    --parameters seedAdminEmail=$SeedAdminEmail `
    --parameters alertEmail=$AlertEmail `
    --parameters location=$Location `
    --parameters environmentName=$Environment `
    --name "myleague-$Environment-$(Get-Date -Format 'yyyyMMdd-HHmmss')" `
    --output json

if ($LASTEXITCODE -ne 0) {
    Write-ErrorMsg "Provisioning failed"
    exit 1
}

Write-Success "Infrastructure provisioned successfully!"

# ============================================================================
# Display Outputs
# ============================================================================

Write-Step "Deployment Outputs"

$outputs = az deployment group show `
    --resource-group $ResourceGroupName `
    --name (az deployment group list --resource-group $ResourceGroupName --query "[0].name" -o tsv) `
    --query "properties.outputs" `
    -o json | ConvertFrom-Json

Write-Host ""
Write-Host "================================================================" -ForegroundColor Green
Write-Host "  Infrastructure Provisioned!" -ForegroundColor Green
Write-Host "================================================================" -ForegroundColor Green
Write-Host ""
Write-Host "  API URL:        $($outputs.apiUrl.value)" -ForegroundColor Green
Write-Host "  App Service:    $($outputs.appServiceName.value)" -ForegroundColor Green  
Write-Host "  PostgreSQL:     $($outputs.postgresServerName.value)" -ForegroundColor Green
Write-Host "  Database:       $($outputs.databaseName.value)" -ForegroundColor Green
Write-Host "  Comm Service:   $($outputs.communicationServiceName.value)" -ForegroundColor Green
Write-Host "  Email Sender:   $($outputs.acsSenderAddress.value)" -ForegroundColor Green
Write-Host ""
Write-Host "================================================================" -ForegroundColor Green

# ============================================================================
# Next Steps
# ============================================================================

Write-Host @"

================================================================
                       Next Steps
================================================================

1. Deploy your application:
   cd infra/deploy
   .\deploy-backend.ps1

   Or manually:
   cd src/backend/WebAPI
   dotnet publish -c Release -o ./publish
   Compress-Archive -Path ./publish/* -DestinationPath ./app.zip -Force
   az webapp deploy --resource-group $ResourceGroupName --name $($outputs.appServiceName.value) --src-path ./app.zip --type zip

2. Run database migrations:
   `$env:ConnectionStrings__DefaultConnection = "Host=$($outputs.postgresServerFqdn.value);Database=myleague;Username=myleagueadmin;Password=<your-password>;SSL Mode=Require;Trust Server Certificate=true"
   dotnet ef database update --project ../Infrastructure/Infrastructure.csproj

3. View logs:
   az webapp log tail --resource-group $ResourceGroupName --name $($outputs.appServiceName.value)

4. Check health:
   curl $($outputs.apiUrl.value)/health/ready

Note: Azure Communication Services Email and JWT authentication have
been automatically configured by this provisioning script.
The ACS sender address is: $($outputs.acsSenderAddress.value)

"@ -ForegroundColor Cyan

Write-Host "Provisioning completed at $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray
