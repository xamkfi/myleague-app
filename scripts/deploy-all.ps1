param(
    [Parameter(Mandatory=$true)][string]$AcrName,
    [string]$Tag = "latest",
    [string]$BackendAppName,
    [string]$FrontendAppName,
    [string]$ResourceGroup,
    [string]$ApiUrl = "/api",
    [switch]$UseAcrBuild,
    [switch]$Deploy
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Deploying Backend and Frontend" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Deploy backend
Write-Host "Step 1: Deploying Backend" -ForegroundColor Yellow
Write-Host "---------------------------" -ForegroundColor Yellow
& "$PSScriptRoot\deploy-backend.ps1" `
    -AcrName $AcrName `
    -Tag $Tag `
    -ResourceGroup $ResourceGroup `
    -ContainerAppName $BackendAppName `
    -Deploy:$Deploy

if ($LASTEXITCODE -ne 0) {
    Write-Error "Backend deployment failed"
    exit 1
}

Write-Host ""
Write-Host "Step 2: Deploying Frontend" -ForegroundColor Yellow
Write-Host "---------------------------" -ForegroundColor Yellow

# Deploy frontend
& "$PSScriptRoot\deploy-frontend.ps1" `
    -AcrName $AcrName `
    -Tag $Tag `
    -ApiUrl $ApiUrl `
    -UseAcrBuild:$UseAcrBuild `
    -ResourceGroup $ResourceGroup `
    -ContainerAppName $FrontendAppName `
    -Deploy:$Deploy

if ($LASTEXITCODE -ne 0) {
    Write-Error "Frontend deployment failed"
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "All deployments completed successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

