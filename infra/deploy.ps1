<#
.SYNOPSIS
    Deploys MyLeague backend infrastructure to Azure.

.DESCRIPTION
    This script automates the deployment of Azure infrastructure for the MyLeague application.
    It creates/uses a resource group and deploys App Service and PostgreSQL resources.

.PARAMETER Environment
    The environment to deploy (dev, staging, prod). Default: dev

.PARAMETER Location
    The Azure region for resources. Default: westeurope

.PARAMETER ResourceGroupName
    Override the resource group name. Default: myleague-{Environment}-rg

.PARAMETER PostgresPassword
    The PostgreSQL admin password. If not provided, you will be prompted.

.PARAMETER SkipLogin
    Skip the Azure login check (use if already logged in).

.EXAMPLE
    .\deploy.ps1
    # Interactive deployment with prompts

.EXAMPLE
    .\deploy.ps1 -Environment staging -Location northeurope
    # Deploy to staging in North Europe

.EXAMPLE
    .\deploy.ps1 -PostgresPassword "MySecurePass123!"
    # Deploy with password provided (not recommended for production)
#>

param(
    [Parameter()]
    [ValidateSet('dev', 'staging', 'prod')]
    [string]$Environment = 'dev',

    [Parameter()]
    [string]$Location = 'westeurope',

    [Parameter()]
    [string]$ResourceGroupName,

    [Parameter()]
    [string]$PostgresPassword,

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
        MyLeague Azure Infrastructure Deployment
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

Write-Step "Deploying infrastructure (this may take 5-10 minutes)..."

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$templateFile = Join-Path $scriptDir "main.bicep"
$parametersFile = Join-Path $scriptDir "main.bicepparam"

# Validate template first
Write-Host "  Validating template..." -ForegroundColor Gray
az deployment group validate `
    --resource-group $ResourceGroupName `
    --template-file $templateFile `
    --parameters $parametersFile `
    --parameters postgresAdminPassword=$PostgresPassword `
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
    --parameters location=$Location `
    --parameters environmentName=$Environment `
    --name "myleague-$Environment-$(Get-Date -Format 'yyyyMMdd-HHmmss')" `
    --output json

if ($LASTEXITCODE -ne 0) {
    Write-ErrorMsg "Deployment failed"
    exit 1
}

Write-Success "Infrastructure deployed successfully!"

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
Write-Host "  Deployment Complete!" -ForegroundColor Green
Write-Host "================================================================" -ForegroundColor Green
Write-Host ""
Write-Host "  API URL:        $($outputs.apiUrl.value)" -ForegroundColor Green
Write-Host "  App Service:    $($outputs.appServiceName.value)" -ForegroundColor Green  
Write-Host "  PostgreSQL:     $($outputs.postgresServerName.value)" -ForegroundColor Green
Write-Host "  Database:       $($outputs.databaseName.value)" -ForegroundColor Green
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

"@ -ForegroundColor Cyan

Write-Host "Deployment completed at $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray
