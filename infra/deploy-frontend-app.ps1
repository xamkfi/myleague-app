<#
.SYNOPSIS
    Builds and deploys the MyLeague React frontend to Azure Static Web App.

.DESCRIPTION
    This script builds the React application and deploys it to an existing Azure Static Web App.
    No infrastructure deployment - just app build and deploy.

.PARAMETER ApiBackendUrl
    The URL of the backend API. Required.

.PARAMETER DeploymentToken
    The Static Web App deployment token. If not provided, will try to get from Azure.

.PARAMETER StaticWebAppName
    The name of the Static Web App. Default: myleague-dev-web

.PARAMETER ResourceGroupName
    The resource group name. Default: myleague-dev-rg

.EXAMPLE
    .\deploy-frontend-app.ps1 -ApiBackendUrl "https://myleague-dev-api.azurewebsites.net"

.EXAMPLE
    .\deploy-frontend-app.ps1 -ApiBackendUrl "https://api.example.com" -DeploymentToken "your-token"
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$ApiBackendUrl,

    [Parameter()]
    [string]$DeploymentToken,

    [Parameter()]
    [string]$StaticWebAppName = 'myleague-dev-web',

    [Parameter()]
    [string]$ResourceGroupName = 'myleague-dev-rg'
)

$ErrorActionPreference = 'Stop'

# Helper functions
function Write-Step { param($Message) Write-Host "`n>> $Message" -ForegroundColor Cyan }
function Write-Success { param($Message) Write-Host "[OK] $Message" -ForegroundColor Green }
function Write-ErrorMsg { param($Message) Write-Host "[X] $Message" -ForegroundColor Red }

Write-Host @"

================================================================
        MyLeague Frontend App Deployment
================================================================

"@ -ForegroundColor Magenta

Write-Host "Backend API URL: $ApiBackendUrl"
Write-Host ""

# ============================================================================
# Prerequisites Check
# ============================================================================

Write-Step "Checking prerequisites..."

# Check pnpm
if (Get-Command pnpm -ErrorAction SilentlyContinue) {
    Write-Success "pnpm available"
}
else {
    Write-ErrorMsg "pnpm is not installed. Please install from: https://pnpm.io/installation"
    exit 1
}

# Check SWA CLI
if (Get-Command swa -ErrorAction SilentlyContinue) {
    Write-Success "SWA CLI available"
}
else {
    Write-Host "  Installing SWA CLI..." -ForegroundColor Gray
    npm install -g @azure/static-web-apps-cli
    if ($LASTEXITCODE -ne 0) {
        Write-ErrorMsg "Failed to install SWA CLI"
        exit 1
    }
    Write-Success "SWA CLI installed"
}

# ============================================================================
# Get Deployment Token
# ============================================================================

if (-not $DeploymentToken) {
    Write-Step "Getting deployment token from Azure..."
    
    # Check if logged in
    $account = az account show 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  Logging in to Azure..." -ForegroundColor Gray
        az login
    }

    $DeploymentToken = az staticwebapp secrets list `
        --name $StaticWebAppName `
        --resource-group $ResourceGroupName `
        --query "properties.apiKey" -o tsv 2>$null

    if (-not $DeploymentToken) {
        Write-ErrorMsg "Failed to get deployment token. Make sure the Static Web App exists."
        Write-ErrorMsg "Run deploy-frontend.ps1 first to create the infrastructure."
        exit 1
    }
    Write-Success "Got deployment token"
}

# ============================================================================
# Build Application
# ============================================================================

Write-Step "Building React application..."

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$frontendPath = Join-Path (Split-Path -Parent $scriptDir) "src\frontend"

if (-not (Test-Path $frontendPath)) {
    Write-ErrorMsg "Frontend path not found: $frontendPath"
    exit 1
}

Push-Location $frontendPath

# Create .env.production
$envFile = ".env.production"
"VITE_API_URL=$ApiBackendUrl" | Out-File -FilePath $envFile -Encoding utf8
Write-Success "Created .env.production"

# Install dependencies
Write-Host "  Installing dependencies..." -ForegroundColor Gray
pnpm install --force
if ($LASTEXITCODE -ne 0) {
    # Try cleaning node_modules and retry
    Write-Host "  First attempt failed, cleaning node_modules..." -ForegroundColor Yellow
    if (Test-Path "node_modules") {
        Remove-Item -Recurse -Force "node_modules" -ErrorAction SilentlyContinue
    }
    pnpm install
    if ($LASTEXITCODE -ne 0) {
        Pop-Location
        Write-ErrorMsg "Failed to install dependencies"
        Write-ErrorMsg "Try closing VS Code/editors and run again, or manually run:"
        Write-ErrorMsg "  cd src/frontend"
        Write-ErrorMsg "  Remove-Item -Recurse -Force node_modules"
        Write-ErrorMsg "  pnpm install"
        exit 1
    }
}

# Build
Write-Host "  Building..." -ForegroundColor Gray
pnpm run build
if ($LASTEXITCODE -ne 0) {
    Pop-Location
    Write-ErrorMsg "Build failed"
    exit 1
}
Write-Success "Build complete"

# ============================================================================
# Deploy Application
# ============================================================================

Write-Step "Deploying to Azure Static Web App..."

swa deploy ./dist --deployment-token $DeploymentToken --env production
if ($LASTEXITCODE -ne 0) {
    Pop-Location
    Write-ErrorMsg "Deployment failed"
    exit 1
}

Pop-Location

Write-Success "Deployment complete!"

# ============================================================================
# Done
# ============================================================================

Write-Host @"

================================================================
  Deployment Complete!
================================================================

  Frontend URL: https://$StaticWebAppName.azurestaticapps.net
  Backend API:  $ApiBackendUrl

================================================================

"@ -ForegroundColor Green
