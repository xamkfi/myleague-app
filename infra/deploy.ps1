# MyLeague Azure Infrastructure Deployment Script
# PowerShell script to deploy Azure infrastructure using Bicep

param(
    [Parameter(Mandatory=$false)]
    [string]$ResourceGroup = "rg-myleague-bicep-dev",
    
    [Parameter(Mandatory=$false)]
    [string]$Location = "westeurope",
    
    [Parameter(Mandatory=$false)]
    [string]$PostgresPassword = "",
    
    [Parameter(Mandatory=$false)]
    [string]$PostgresUsername = "myleagueadmin",
    
    [Parameter(Mandatory=$false)]
    [string]$Environment = "dev",
    
    [Parameter(Mandatory=$false)]
    [string]$ProjectName = "myleague-bicep",
    
    [Parameter(Mandatory=$false)]
    [switch]$ValidateOnly,
    
    [Parameter(Mandatory=$false)]
    [switch]$ShowOutputs
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "MyLeague Azure Infrastructure Deployment" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if Azure CLI is installed
Write-Host "Checking Azure CLI installation..." -ForegroundColor Yellow
$azCommand = Get-Command az -ErrorAction SilentlyContinue
if ($null -ne $azCommand) {
    Write-Host "Azure CLI found" -ForegroundColor Green
} else {
    Write-Host "Azure CLI not found. Please install it from: https://aka.ms/installazurecliwindows" -ForegroundColor Red
    exit 1
}

# Check if logged in
Write-Host "Checking Azure login status..." -ForegroundColor Yellow
$account = az account show 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Not logged in. Logging in..." -ForegroundColor Yellow
    az login
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to login to Azure" -ForegroundColor Red
        exit 1
    }
}
Write-Host "Logged in to Azure" -ForegroundColor Green

# Get current subscription
$subscription = az account show --query "{Name:name, Id:id}" -o json | ConvertFrom-Json
Write-Host "Current subscription: $($subscription.Name)" -ForegroundColor Cyan
Write-Host ""

# Prompt for password if not provided
if ([string]::IsNullOrEmpty($PostgresPassword)) {
    Write-Host "Enter PostgreSQL administrator password:" -ForegroundColor Yellow
    Write-Host "(Must be at least 8 characters with uppercase, lowercase, numbers, and special characters)" -ForegroundColor Gray
    $securePassword = Read-Host -AsSecureString
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
    $PostgresPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
}

# Create resource group
Write-Host "Creating resource group: $ResourceGroup..." -ForegroundColor Yellow
$rgExists = az group exists --name $ResourceGroup
if ($rgExists -eq "false") {
    az group create --name $ResourceGroup --location $Location
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to create resource group" -ForegroundColor Red
        exit 1
    }
    Write-Host "Resource group created" -ForegroundColor Green
} else {
    Write-Host "Resource group already exists" -ForegroundColor Green
}

# Navigate to infra directory
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptPath

# Validate template
Write-Host ""
Write-Host "Validating Bicep template..." -ForegroundColor Yellow
az deployment group validate `
    --resource-group $ResourceGroup `
    --template-file main.bicep `
    --parameters environment=$Environment `
                 projectName=$ProjectName `
                 location=$Location `
                 postgresAdminUsername=$PostgresUsername `
                 postgresAdminPassword=$PostgresPassword `
                 appServiceSku=B1 `
                 staticWebAppSku=Free `
    --output none

if ($LASTEXITCODE -ne 0) {
    Write-Host "Template validation failed" -ForegroundColor Red
    exit 1
}
Write-Host "Template validation passed" -ForegroundColor Green

if ($ValidateOnly) {
    Write-Host ""
    Write-Host "Validation complete. Exiting (ValidateOnly flag set)." -ForegroundColor Cyan
    exit 0
}

# Deploy infrastructure
Write-Host ""
Write-Host "Deploying infrastructure..." -ForegroundColor Yellow
Write-Host "This will take approximately 10-15 minutes..." -ForegroundColor Gray
Write-Host ""

$deploymentName = "myleague-infra-deployment-$(Get-Date -Format 'yyyyMMdd-HHmmss')"

az deployment group create `
    --resource-group $ResourceGroup `
    --template-file main.bicep `
    --name $deploymentName `
    --parameters environment=$Environment `
                 projectName=$ProjectName `
                 location=$Location `
                 postgresAdminUsername=$PostgresUsername `
                 postgresAdminPassword=$PostgresPassword `
                 appServiceSku=B1 `
                 staticWebAppSku=Free

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "Deployment failed" -ForegroundColor Red
    Write-Host "Check the error details above or run:" -ForegroundColor Yellow
    Write-Host "  az deployment group show --resource-group $ResourceGroup --name $deploymentName --query properties.error" -ForegroundColor Gray
    exit 1
}

Write-Host ""
Write-Host "Deployment completed successfully!" -ForegroundColor Green
Write-Host ""

# Show outputs
if ($ShowOutputs -or $true) {
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "Deployment Outputs" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    
    $outputs = az deployment group show `
        --resource-group $ResourceGroup `
        --name $deploymentName `
        --query properties.outputs `
        -o json | ConvertFrom-Json
    
    Write-Host "Backend (App Service):" -ForegroundColor Yellow
    Write-Host "  URL: $($outputs.appServiceUrl.value)" -ForegroundColor White
    Write-Host "  Name: $($outputs.appServiceHostname.value)" -ForegroundColor Gray
    Write-Host ""
    
    Write-Host "Frontend (Static Web App):" -ForegroundColor Yellow
    Write-Host "  URL: $($outputs.staticWebAppUrl.value)" -ForegroundColor White
    Write-Host "  Name: $($outputs.staticWebAppHostname.value)" -ForegroundColor Gray
    Write-Host ""
    
    Write-Host "Database (PostgreSQL):" -ForegroundColor Yellow
    Write-Host "  Server: $($outputs.postgresServerFqdn.value)" -ForegroundColor White
    Write-Host "  Database: $($outputs.postgresDatabaseName.value)" -ForegroundColor Gray
    Write-Host ""
    
    Write-Host "Application Insights:" -ForegroundColor Yellow
    Write-Host "  Name: appi-$ProjectName-$Environment" -ForegroundColor Gray
    Write-Host ""
    
    Write-Host "Static Web App Deployment Token:" -ForegroundColor Yellow
    Write-Host "  $($outputs.staticWebAppDeploymentToken.value)" -ForegroundColor Gray
    Write-Host ""
    
    # Save to file
    $outputFile = "deployment-outputs-$deploymentName.json"
    $outputs | ConvertTo-Json -Depth 10 | Out-File -FilePath $outputFile
    Write-Host "Full outputs saved to: $outputFile" -ForegroundColor Cyan
    Write-Host ""
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "1. Deploy your backend application to App Service" -ForegroundColor White
Write-Host "2. Deploy your frontend application to Static Web App" -ForegroundColor White
Write-Host "3. Run database migrations" -ForegroundColor White
Write-Host "4. Configure frontend environment variables" -ForegroundColor White
Write-Host ""
Write-Host "View resources in Azure Portal:" -ForegroundColor Cyan
Write-Host "  az portal --resource-group $ResourceGroup" -ForegroundColor Gray
Write-Host ""

