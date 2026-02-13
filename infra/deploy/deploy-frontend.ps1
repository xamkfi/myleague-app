<#
.SYNOPSIS
    Builds and deploys the MyLeague React frontend to an Azure Static Web App.

.DESCRIPTION
    Interactive deployment script that:
    1. Asks for the backend API base URL
    2. Asks whether /api should be appended to the URL
    3. Lists available Azure Static Web Apps and lets you pick one
    4. Builds the React app and deploys it

    No infrastructure changes - just app build and deploy.
    Run infra/provision/provision-frontend.ps1 first to create the Static Web App.

.PARAMETER ApiBaseUrl
    The base URL of the backend API (e.g. https://myleague-dev-api.azurewebsites.net).
    If not provided, you will be prompted.

.PARAMETER AppendApi
    Whether to append /api to the base URL. If not provided, you will be prompted.

.PARAMETER StaticWebAppName
    The name of the Static Web App to deploy to. If not provided, available SWAs will be listed.

.PARAMETER ResourceGroupName
    The resource group to search for Static Web Apps. If not provided, you will be prompted.

.PARAMETER DeploymentToken
    The Static Web App deployment token. If not provided, will be fetched from Azure.

.PARAMETER SkipLogin
    Skip the Azure login check (use if already logged in).

.EXAMPLE
    .\deploy-frontend.ps1
    # Fully interactive - asks all questions

.EXAMPLE
    .\deploy-frontend.ps1 -ApiBaseUrl "https://myleague-dev-api.azurewebsites.net" -AppendApi
    # Provide API URL, append /api, still asks for SWA target
#>

param(
    [Parameter()]
    [string]$ApiBaseUrl,

    [Parameter()]
    [switch]$AppendApi,

    [Parameter()]
    [switch]$NoAppendApi,

    [Parameter()]
    [string]$StaticWebAppName,

    [Parameter()]
    [string]$ResourceGroupName,

    [Parameter()]
    [string]$DeploymentToken,

    [Parameter()]
    [switch]$SkipLogin
)

$ErrorActionPreference = 'Stop'

# Helper functions
function Write-Step { param($Message) Write-Host "`n>> $Message" -ForegroundColor Cyan }
function Write-Success { param($Message) Write-Host "[OK] $Message" -ForegroundColor Green }
function Write-WarningMsg { param($Message) Write-Host "[!] $Message" -ForegroundColor Yellow }
function Write-ErrorMsg { param($Message) Write-Host "[X] $Message" -ForegroundColor Red }

Write-Host @"

================================================================
        MyLeague Frontend Deployment
================================================================

"@ -ForegroundColor Magenta

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
# Step 1: Ask for Backend API URL
# ============================================================================

Write-Step "Configuring backend API URL..."

if (-not $ApiBaseUrl) {
    Write-Host ""
    Write-Host "  Enter the backend API base URL." -ForegroundColor White
    Write-Host "  Example: https://myleague-dev-api.azurewebsites.net" -ForegroundColor Gray
    Write-Host ""

    $ApiBaseUrl = Read-Host "  Backend API base URL"

    if (-not $ApiBaseUrl) {
        Write-ErrorMsg "Backend API URL is required."
        exit 1
    }
}

# Remove trailing slash from base URL
$ApiBaseUrl = $ApiBaseUrl.TrimEnd('/')

Write-Success "Base URL: $ApiBaseUrl"

# ============================================================================
# Step 2: Ask whether to append /api
# ============================================================================

if (-not $AppendApi -and -not $NoAppendApi) {
    Write-Host ""
    Write-Host "  Does the API use an /api prefix?" -ForegroundColor White
    Write-Host "  - If your endpoints look like: $ApiBaseUrl/api/News  -> answer Y" -ForegroundColor Gray
    Write-Host "  - If your endpoints look like: $ApiBaseUrl/News      -> answer N" -ForegroundColor Gray
    Write-Host ""

    $appendChoice = Read-Host "  Append /api to the URL? (Y/n)"

    if ($appendChoice -eq 'n' -or $appendChoice -eq 'N') {
        $AppendApi = $false
    }
    else {
        $AppendApi = $true
    }
}

if ($NoAppendApi) {
    $AppendApi = $false
}

# Build final API URL
if ($AppendApi) {
    $FinalApiUrl = "$ApiBaseUrl/api"
}
else {
    $FinalApiUrl = $ApiBaseUrl
}

Write-Host ""
Write-Host "  Final VITE_API_URL: " -NoNewline -ForegroundColor White
Write-Host "$FinalApiUrl" -ForegroundColor Green
Write-Host ""

# ============================================================================
# Step 3: Select Azure Static Web App to deploy to
# ============================================================================

Write-Step "Selecting deployment target..."

if (-not $StaticWebAppName) {
    # If no resource group specified, list all SWAs in the subscription
    Write-Host "  Searching for Azure Static Web Apps..." -ForegroundColor Gray

    if ($ResourceGroupName) {
        $swaListJson = az staticwebapp list --resource-group $ResourceGroupName --query "[].{name:name, resourceGroup:resourceGroup, defaultHostname:defaultHostname, sku:sku.name}" -o json 2>$null
    }
    else {
        $swaListJson = az staticwebapp list --query "[].{name:name, resourceGroup:resourceGroup, defaultHostname:defaultHostname, sku:sku.name}" -o json 2>$null
    }

    if ($LASTEXITCODE -ne 0 -or -not $swaListJson) {
        Write-ErrorMsg "Failed to list Static Web Apps. Make sure you have the right subscription selected."
        exit 1
    }

    $swaList = $swaListJson | ConvertFrom-Json

    if ($swaList.Count -eq 0) {
        Write-ErrorMsg "No Static Web Apps found in your subscription."
        Write-ErrorMsg "Run infra\provision\provision-frontend.ps1 first to create one."
        exit 1
    }

    Write-Host ""
    Write-Host "  Available Static Web Apps:" -ForegroundColor White
    Write-Host "  -------------------------" -ForegroundColor Gray

    for ($i = 0; $i -lt $swaList.Count; $i++) {
        $swa = $swaList[$i]
        Write-Host "  [$($i + 1)] " -NoNewline -ForegroundColor Yellow
        Write-Host "$($swa.name)" -NoNewline -ForegroundColor White
        Write-Host "  (https://$($swa.defaultHostname))" -NoNewline -ForegroundColor Gray
        Write-Host "  [$($swa.sku)]" -ForegroundColor DarkGray
        Write-Host "      Resource group: $($swa.resourceGroup)" -ForegroundColor DarkGray
    }

    Write-Host ""

    if ($swaList.Count -eq 1) {
        $selectedIndex = 0
        Write-Host "  Only one SWA found, auto-selecting: $($swaList[0].name)" -ForegroundColor Gray
    }
    else {
        $selection = Read-Host "  Select a Static Web App (1-$($swaList.Count))"
        $selectedIndex = [int]$selection - 1

        if ($selectedIndex -lt 0 -or $selectedIndex -ge $swaList.Count) {
            Write-ErrorMsg "Invalid selection."
            exit 1
        }
    }

    $selectedSwa = $swaList[$selectedIndex]
    $StaticWebAppName = $selectedSwa.name
    $ResourceGroupName = $selectedSwa.resourceGroup

    Write-Success "Selected: $StaticWebAppName (https://$($selectedSwa.defaultHostname))"
}
else {
    if (-not $ResourceGroupName) {
        # Try to find the resource group for the given SWA name
        $swaInfo = az staticwebapp show --name $StaticWebAppName --query "{resourceGroup:resourceGroup, defaultHostname:defaultHostname}" -o json 2>$null | ConvertFrom-Json
        if ($swaInfo) {
            $ResourceGroupName = $swaInfo.resourceGroup
        }
        else {
            Write-ErrorMsg "Could not find Static Web App '$StaticWebAppName'. Provide -ResourceGroupName."
            exit 1
        }
    }
    Write-Success "Target: $StaticWebAppName"
}

# ============================================================================
# Get Deployment Token
# ============================================================================

if (-not $DeploymentToken) {
    Write-Step "Getting deployment token..."

    $DeploymentToken = az staticwebapp secrets list `
        --name $StaticWebAppName `
        --resource-group $ResourceGroupName `
        --query "properties.apiKey" -o tsv 2>$null

    if (-not $DeploymentToken) {
        Write-ErrorMsg "Failed to get deployment token for '$StaticWebAppName'."
        Write-ErrorMsg "Make sure the Static Web App exists and you have access."
        exit 1
    }
    Write-Success "Got deployment token"
}

# ============================================================================
# Confirm Deployment
# ============================================================================

Write-Host ""
Write-Host "================================================================" -ForegroundColor Yellow
Write-Host "  Deployment Summary" -ForegroundColor Yellow
Write-Host "================================================================" -ForegroundColor Yellow
Write-Host ""
Write-Host "  Target SWA:      $StaticWebAppName" -ForegroundColor White
Write-Host "  Resource Group:   $ResourceGroupName" -ForegroundColor White
Write-Host "  VITE_API_URL:     $FinalApiUrl" -ForegroundColor White
Write-Host ""
Write-Host "================================================================" -ForegroundColor Yellow
Write-Host ""

$confirmDeploy = Read-Host "  Proceed with deployment? (Y/n)"
if ($confirmDeploy -eq 'n' -or $confirmDeploy -eq 'N') {
    Write-WarningMsg "Deployment cancelled."
    exit 0
}

# ============================================================================
# Build Application
# ============================================================================

Write-Step "Building React application..."

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$infraDir = Split-Path -Parent $scriptDir
$frontendPath = Join-Path (Split-Path -Parent $infraDir) "src\frontend"

if (-not (Test-Path $frontendPath)) {
    Write-ErrorMsg "Frontend path not found: $frontendPath"
    exit 1
}

Push-Location $frontendPath

# Create .env.production
$envFile = ".env.production"
"VITE_API_URL=$FinalApiUrl" | Out-File -FilePath $envFile -Encoding utf8
Write-Success "Created .env.production with VITE_API_URL=$FinalApiUrl"

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

Write-Step "Deploying to Azure Static Web App '$StaticWebAppName'..."

swa deploy ./dist --deployment-token $DeploymentToken --env production
if ($LASTEXITCODE -ne 0) {
    Pop-Location
    Write-ErrorMsg "Deployment failed"
    exit 1
}

Pop-Location

# ============================================================================
# Done
# ============================================================================

Write-Host @"

================================================================
  Deployment Complete!
================================================================

  Static Web App:  $StaticWebAppName
  VITE_API_URL:    $FinalApiUrl

  Your app should be live shortly.

================================================================

"@ -ForegroundColor Green

Write-Host "Deployment completed at $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray
