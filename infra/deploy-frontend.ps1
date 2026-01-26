<#
.SYNOPSIS
    Deploys MyLeague frontend infrastructure and application to Azure.

.DESCRIPTION
    This script automates the deployment of Azure Static Web App for the MyLeague frontend.
    It can deploy just the infrastructure, or also build and deploy the React application.

.PARAMETER Environment
    The environment to deploy (dev, staging, prod). Default: dev

.PARAMETER Location
    The Azure region for resources. Default: westeurope

.PARAMETER ResourceGroupName
    Override the resource group name. Default: myleague-{Environment}-rg

.PARAMETER ApiBackendUrl
    The URL of the backend API. If not provided, will try to get from existing backend deployment.

.PARAMETER DeployApp
    If specified, also builds and deploys the React application.

.PARAMETER SkipLogin
    Skip the Azure login check (use if already logged in).

.EXAMPLE
    .\deploy-frontend.ps1
    # Deploy infrastructure only

.EXAMPLE
    .\deploy-frontend.ps1 -DeployApp
    # Deploy infrastructure and application

.EXAMPLE
    .\deploy-frontend.ps1 -ApiBackendUrl "https://myleague-dev-api.azurewebsites.net" -DeployApp
    # Deploy with specific backend URL
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
    [string]$ApiBackendUrl,

    [Parameter()]
    [switch]$DeployApp,

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
        MyLeague Frontend Infrastructure Deployment
================================================================

"@ -ForegroundColor Magenta

Write-Host "Environment: $Environment"
Write-Host "Location:    $Location"
Write-Host "Deploy App:  $DeployApp"
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

# Check for SWA CLI if deploying app
if ($DeployApp) {
    if (Get-Command swa -ErrorAction SilentlyContinue) {
        Write-Success "SWA CLI available"
    }
    else {
        Write-WarningMsg "SWA CLI not found, installing..."
        npm install -g @azure/static-web-apps-cli
        if ($LASTEXITCODE -eq 0) {
            Write-Success "SWA CLI installed"
        }
        else {
            Write-ErrorMsg "Failed to install SWA CLI. Please install Node.js and run: npm install -g @azure/static-web-apps-cli"
            exit 1
        }
    }

    # Check for pnpm
    if (Get-Command pnpm -ErrorAction SilentlyContinue) {
        Write-Success "pnpm available"
    }
    else {
        Write-ErrorMsg "pnpm is not installed. Please install from: https://pnpm.io/installation"
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
}

# ============================================================================
# Get Backend API URL
# ============================================================================

Write-Step "Configuring deployment parameters..."

if (-not $ApiBackendUrl) {
    # Try to get from existing backend deployment
    Write-Host "  Checking for existing backend deployment..." -ForegroundColor Gray
    $backendUrl = az deployment group show `
        --resource-group $ResourceGroupName `
        --name (az deployment group list --resource-group $ResourceGroupName --query "[?contains(name, 'myleague')].name | [0]" -o tsv 2>$null) `
        --query "properties.outputs.apiUrl.value" -o tsv 2>$null

    if ($backendUrl) {
        $ApiBackendUrl = $backendUrl
        Write-Success "Found backend API URL: $ApiBackendUrl"
    }
    else {
        Write-WarningMsg "Backend API URL not found. Frontend will be deployed without API configuration."
        Write-WarningMsg "You can update this later in the Static Web App settings."
        $ApiBackendUrl = ""
    }
}
else {
    Write-Success "Using provided backend API URL: $ApiBackendUrl"
}

# ============================================================================
# Check Resource Group
# ============================================================================

Write-Step "Checking resource group '$ResourceGroupName'..."

$rgExists = az group exists --name $ResourceGroupName
if ($rgExists -eq 'true') {
    Write-Success "Resource group exists"
}
else {
    Write-WarningMsg "Resource group does not exist. Creating..."
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

Write-Step "Deploying frontend infrastructure..."

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$templateFile = Join-Path $scriptDir "main-frontend.bicep"
$parametersFile = Join-Path $scriptDir "main-frontend.bicepparam"

# Validate template first
Write-Host "  Validating template..." -ForegroundColor Gray
az deployment group validate `
    --resource-group $ResourceGroupName `
    --template-file $templateFile `
    --parameters $parametersFile `
    --parameters apiBackendUrl=$ApiBackendUrl `
    --parameters location=$Location `
    --parameters environmentName=$Environment `
    --output none

if ($LASTEXITCODE -ne 0) {
    Write-ErrorMsg "Template validation failed"
    exit 1
}
Write-Host "  Template validated" -ForegroundColor Gray

# Deploy
Write-Host "  Deploying Static Web App..." -ForegroundColor Gray
$deploymentOutput = az deployment group create `
    --resource-group $ResourceGroupName `
    --template-file $templateFile `
    --parameters $parametersFile `
    --parameters apiBackendUrl=$ApiBackendUrl `
    --parameters location=$Location `
    --parameters environmentName=$Environment `
    --name "myleague-frontend-$Environment-$(Get-Date -Format 'yyyyMMdd-HHmmss')" `
    --output json

if ($LASTEXITCODE -ne 0) {
    Write-ErrorMsg "Deployment failed"
    exit 1
}

Write-Success "Frontend infrastructure deployed!"

# ============================================================================
# Get Deployment Outputs
# ============================================================================

Write-Step "Getting deployment outputs..."

$outputs = az deployment group show `
    --resource-group $ResourceGroupName `
    --name (az deployment group list --resource-group $ResourceGroupName --query "[?contains(name, 'frontend')].name | [0]" -o tsv) `
    --query "properties.outputs" `
    -o json | ConvertFrom-Json

$frontendUrl = $outputs.frontendUrl.value
$staticWebAppName = $outputs.staticWebAppName.value
$deploymentToken = $outputs.deploymentToken.value

Write-Host ""
Write-Host "================================================================" -ForegroundColor Green
Write-Host "  Infrastructure Deployed!" -ForegroundColor Green
Write-Host "================================================================" -ForegroundColor Green
Write-Host ""
Write-Host "  Frontend URL:      $frontendUrl" -ForegroundColor Green
Write-Host "  Static Web App:    $staticWebAppName" -ForegroundColor Green
Write-Host "  Backend API URL:   $ApiBackendUrl" -ForegroundColor Green
Write-Host ""
Write-Host "================================================================" -ForegroundColor Green

# ============================================================================
# Deploy Application (if requested)
# ============================================================================

if ($DeployApp) {
    Write-Step "Building and deploying React application..."

    $frontendPath = Join-Path (Split-Path -Parent $scriptDir) "src\frontend"
    
    if (-not (Test-Path $frontendPath)) {
        Write-ErrorMsg "Frontend path not found: $frontendPath"
        exit 1
    }

    # Create .env.production with API URL
    $envFile = Join-Path $frontendPath ".env.production"
    "VITE_API_URL=$ApiBackendUrl" | Out-File -FilePath $envFile -Encoding utf8
    Write-Success "Created .env.production with API URL"

    # Install dependencies
    Write-Host "  Installing dependencies..." -ForegroundColor Gray
    Push-Location $frontendPath
    pnpm install
    if ($LASTEXITCODE -ne 0) {
        Pop-Location
        Write-ErrorMsg "Failed to install dependencies"
        exit 1
    }

    # Build the application
    Write-Host "  Building application..." -ForegroundColor Gray
    pnpm run build
    if ($LASTEXITCODE -ne 0) {
        Pop-Location
        Write-ErrorMsg "Failed to build application"
        exit 1
    }
    Write-Success "Application built"

    # Deploy using SWA CLI
    Write-Host "  Deploying to Azure Static Web App..." -ForegroundColor Gray
    swa deploy ./dist `
        --deployment-token $deploymentToken `
        --env production

    if ($LASTEXITCODE -ne 0) {
        Pop-Location
        Write-ErrorMsg "Failed to deploy application"
        exit 1
    }

    Pop-Location
    Write-Success "Application deployed!"
}

# ============================================================================
# Next Steps
# ============================================================================

Write-Host @"

================================================================
                       Next Steps
================================================================

"@ -ForegroundColor Cyan

if (-not $DeployApp) {
    Write-Host @"
1. Deploy the React application:
   cd src/frontend
   pnpm install
   pnpm run build
   swa deploy ./dist --deployment-token "$deploymentToken"

   Or run this script with -DeployApp flag:
   .\deploy-frontend.ps1 -DeployApp

"@ -ForegroundColor Cyan
}

Write-Host @"
2. Update backend CORS settings:
   Add '$frontendUrl' to the allowedOrigins in main.bicepparam
   Then redeploy the backend.

3. Configure custom domain (optional):
   az staticwebapp hostname set --name $staticWebAppName --hostname your-domain.com

4. View your application:
   $frontendUrl

"@ -ForegroundColor Cyan

Write-Host "Deployment completed at $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray

# Output deployment token for CI/CD
Write-Host ""
Write-Host "================================================================" -ForegroundColor Yellow
Write-Host "  DEPLOYMENT TOKEN (save for CI/CD)" -ForegroundColor Yellow
Write-Host "================================================================" -ForegroundColor Yellow
Write-Host ""
Write-Host "  $deploymentToken" -ForegroundColor White
Write-Host ""
Write-Host "  Store this as a secret in your CI/CD pipeline." -ForegroundColor Yellow
Write-Host "================================================================" -ForegroundColor Yellow
