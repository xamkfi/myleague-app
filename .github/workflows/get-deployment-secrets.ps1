# Script to get GitHub secrets for CI/CD workflows
# Run this script to retrieve the required deployment secrets

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "GitHub Secrets Retrieval Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Configuration
$RESOURCE_GROUP = "rg-myleague-bicep-dev"
$APP_SERVICE_NAME = "app-myleague-bicep-dev"
$STATIC_WEB_APP_NAME = "swa-myleague-bicep-dev"

# Check if Azure CLI is installed
Write-Host "Checking Azure CLI..." -ForegroundColor Yellow
try {
    az version | Out-Null
    Write-Host "✅ Azure CLI is installed" -ForegroundColor Green
} catch {
    Write-Host "❌ Azure CLI is not installed. Please install it first." -ForegroundColor Red
    Write-Host "Visit: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "Checking Azure login status..." -ForegroundColor Yellow
$account = az account show 2>$null | ConvertFrom-Json
if (!$account) {
    Write-Host "❌ Not logged in to Azure. Please run 'az login' first." -ForegroundColor Red
    exit 1
}
Write-Host "✅ Logged in as: $($account.user.name)" -ForegroundColor Green
Write-Host "   Subscription: $($account.name)" -ForegroundColor Gray
Write-Host ""

# Get Backend Publish Profile
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "1. Backend Publish Profile" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Retrieving publish profile for App Service: $APP_SERVICE_NAME" -ForegroundColor Yellow

try {
    $publishProfile = az webapp deployment list-publishing-profiles `
        --name $APP_SERVICE_NAME `
        --resource-group $RESOURCE_GROUP `
        --xml
    
    if ($publishProfile) {
        Write-Host "✅ Successfully retrieved publish profile" -ForegroundColor Green
        Write-Host ""
        Write-Host "Secret Name: AZURE_WEBAPP_PUBLISH_PROFILE" -ForegroundColor Yellow
        Write-Host "Secret Value (copy everything below):" -ForegroundColor Yellow
        Write-Host "----------------------------------------" -ForegroundColor Gray
        Write-Host $publishProfile -ForegroundColor White
        Write-Host "----------------------------------------" -ForegroundColor Gray
        Write-Host ""
        
        # Save to file
        $publishProfile | Out-File -FilePath "backend-publish-profile.xml" -Encoding UTF8
        Write-Host "💾 Saved to: backend-publish-profile.xml" -ForegroundColor Cyan
    }
} catch {
    Write-Host "❌ Failed to retrieve publish profile" -ForegroundColor Red
    Write-Host "Error: $_" -ForegroundColor Red
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "2. Frontend Deployment Token" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Retrieving deployment token for Static Web App: $STATIC_WEB_APP_NAME" -ForegroundColor Yellow

try {
    $swaToken = az staticwebapp secrets list `
        --name $STATIC_WEB_APP_NAME `
        --resource-group $RESOURCE_GROUP `
        --query properties.apiKey `
        --output tsv
    
    if ($swaToken) {
        Write-Host "✅ Successfully retrieved deployment token" -ForegroundColor Green
        Write-Host ""
        Write-Host "Secret Name: AZURE_STATIC_WEB_APPS_API_TOKEN" -ForegroundColor Yellow
        Write-Host "Secret Value:" -ForegroundColor Yellow
        Write-Host "----------------------------------------" -ForegroundColor Gray
        Write-Host $swaToken -ForegroundColor White
        Write-Host "----------------------------------------" -ForegroundColor Gray
        Write-Host ""
        
        # Save to file
        $swaToken | Out-File -FilePath "frontend-deployment-token.txt" -Encoding UTF8
        Write-Host "💾 Saved to: frontend-deployment-token.txt" -ForegroundColor Cyan
    }
} catch {
    Write-Host "❌ Failed to retrieve deployment token" -ForegroundColor Red
    Write-Host "Error: $_" -ForegroundColor Red
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Next Steps" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Go to your GitHub repository" -ForegroundColor Yellow
Write-Host "2. Navigate to Settings > Secrets and variables > Actions" -ForegroundColor Yellow
Write-Host "3. Click 'New repository secret'" -ForegroundColor Yellow
Write-Host "4. Add the following secrets:" -ForegroundColor Yellow
Write-Host ""
Write-Host "   Secret 1:" -ForegroundColor White
Write-Host "   Name:  AZURE_WEBAPP_PUBLISH_PROFILE" -ForegroundColor Cyan
Write-Host "   Value: Contents of backend-publish-profile.xml" -ForegroundColor Cyan
Write-Host ""
Write-Host "   Secret 2:" -ForegroundColor White
Write-Host "   Name:  AZURE_STATIC_WEB_APPS_API_TOKEN" -ForegroundColor Cyan
Write-Host "   Value: Contents of frontend-deployment-token.txt" -ForegroundColor Cyan
Write-Host ""
Write-Host "5. Commit and push your code to main/master branch" -ForegroundColor Yellow
Write-Host "6. Check the Actions tab to see the workflows running" -ForegroundColor Yellow
Write-Host ""
Write-Host "WARNING: Keep these secrets safe and do not commit them to git!" -ForegroundColor Red
Write-Host ""
Write-Host "For more details, see: .github/workflows/README.md" -ForegroundColor Cyan
Write-Host ""

