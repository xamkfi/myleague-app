param(
    [Parameter(Mandatory=$true)][string]$AcrName,            # e.g. myleagueacrom63tv
    [string]$ImageName = "webapi",                           # repo name in ACR
    [string]$Tag = "latest",
    [string]$Dockerfile = "src/backend/WebAPI/Dockerfile",
    [string]$Context = ".",                                  # build context from project root
    [string]$ResourceGroup,                                  # optional for ACA deploy
    [string]$ContainerAppName,                               # optional for ACA deploy
    [switch]$Deploy                                          # add -Deploy to push a new revision to ACA
)

$ErrorActionPreference = "Stop"

# Change to script directory and then go to parent (project root)
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir
Set-Location ..

Write-Host "== Backend build/tag/push ==" -ForegroundColor Cyan
Write-Host "Working directory: $(Get-Location)" -ForegroundColor Gray

# Verify Docker is available and running
try {
    docker --version | Out-Null
    docker ps | Out-Null
} catch {
    Write-Error "Docker is not installed or not running. Please start Docker Desktop."
    exit 1
}

# Verify Dockerfile exists
if (-not (Test-Path $Dockerfile)) {
    Write-Error "Dockerfile not found at: $Dockerfile"
    exit 1
}

Write-Host "Logging into Azure Container Registry: $AcrName..." -ForegroundColor Yellow
az acr login --name $AcrName
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to login to Azure Container Registry"
    exit 1
}

$fullName = "$AcrName.azurecr.io/$ImageName`:$Tag"

Write-Host "Building $fullName ..." -ForegroundColor Green
docker build -t "$ImageName`:$Tag" -f $Dockerfile $Context
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to build backend image"
    exit 1
}

Write-Host "Tagging as $fullName ..." -ForegroundColor Green
docker tag "$ImageName`:$Tag" $fullName
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to tag backend image"
    exit 1
}

Write-Host "Pushing $fullName ..." -ForegroundColor Green
docker push $fullName
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to push backend image"
    exit 1
}

if ($Deploy -and $ResourceGroup -and $ContainerAppName) {
    Write-Host "Deploying to Azure Container Apps: $ContainerAppName" -ForegroundColor Cyan
    az containerapp update `
        --name $ContainerAppName `
        --resource-group $ResourceGroup `
        --image $fullName | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to deploy to Azure Container Apps"
        exit 1
    }
    Write-Host "Deployed image: $fullName" -ForegroundColor Green
} elseif ($Deploy) {
    Write-Warning "Skipped ACA deploy: -ResourceGroup and -ContainerAppName are required when using -Deploy"
} else {
    Write-Host "Skipped ACA deploy (use -Deploy and provide -ResourceGroup/-ContainerAppName)." -ForegroundColor Gray
}

Write-Host "Done: $fullName" -ForegroundColor Green

