param(
    [Parameter(Mandatory=$true)][string]$AcrName,            # e.g. myleagueacrom63tv
    [string]$ImageName = "frontend",
    [string]$Tag = "latest",
    [string]$Dockerfile = "src/frontend/Dockerfile.prod",   # falls back to Dockerfile if .prod doesn't exist
    [string]$Context = "src/frontend",
    [string]$ApiUrl = "/api",                                # build-time API base, if your Dockerfile uses ARG
    [switch]$UseAcrBuild,                                    # build in cloud (ACR Tasks) instead of local docker
    [string]$ResourceGroup,                                  # optional for ACA deploy
    [string]$ContainerAppName,                               # optional for ACA deploy
    [switch]$Deploy
)

$ErrorActionPreference = "Stop"

# Change to script directory and then go to parent (project root)
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir
Set-Location ..

Write-Host "== Frontend build/tag/push ==" -ForegroundColor Cyan
Write-Host "Working directory: $(Get-Location)" -ForegroundColor Gray

$fullName = "$AcrName.azurecr.io/$ImageName`:$Tag"

# Check if Dockerfile.prod exists, otherwise use Dockerfile
if (-not (Test-Path $Dockerfile) -and $Dockerfile -like "*Dockerfile.prod") {
    $fallbackDockerfile = $Dockerfile -replace "\.prod$", ""
    if (Test-Path $fallbackDockerfile) {
        Write-Host "Warning: Dockerfile.prod not found, using Dockerfile instead" -ForegroundColor Yellow
        $Dockerfile = $fallbackDockerfile
    }
}

# Verify Dockerfile exists
if (-not (Test-Path $Dockerfile)) {
    Write-Error "Dockerfile not found at: $Dockerfile"
    exit 1
}

if ($UseAcrBuild) {
    Write-Host "== Frontend ACR cloud build ==" -ForegroundColor Cyan
    Write-Host "Logging into Azure Container Registry: $AcrName..." -ForegroundColor Yellow
    az acr login --name $AcrName
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to login to Azure Container Registry"
        exit 1
    }

    Write-Host "Building in ACR with API URL: $ApiUrl" -ForegroundColor Green
    az acr build `
        --registry $AcrName `
        --image "$ImageName`:$Tag" `
        --file $Dockerfile `
        --build-arg "VITE_API_URL=$ApiUrl" `
        $Context
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to build frontend image in ACR"
        exit 1
    }
} else {
    Write-Host "== Frontend local docker build ==" -ForegroundColor Cyan
    
    # Verify Docker is available and running
    try {
        docker --version | Out-Null
        docker ps | Out-Null
    } catch {
        Write-Error "Docker is not installed or not running. Please start Docker Desktop."
        exit 1
    }

    Write-Host "Logging into Azure Container Registry: $AcrName..." -ForegroundColor Yellow
    az acr login --name $AcrName
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to login to Azure Container Registry"
        exit 1
    }

    Write-Host "Building with API URL: $ApiUrl" -ForegroundColor Green
    if ($ApiUrl -and $ApiUrl -ne "/api") {
        docker build `
            --build-arg "VITE_API_URL=$ApiUrl" `
            -t "$ImageName`:$Tag" `
            -f $Dockerfile `
            $Context
    } else {
        docker build `
            -t "$ImageName`:$Tag" `
            -f $Dockerfile `
            $Context
    }
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to build frontend image"
        exit 1
    }

    Write-Host "Tagging as $fullName ..." -ForegroundColor Green
    docker tag "$ImageName`:$Tag" $fullName
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to tag frontend image"
        exit 1
    }

    Write-Host "Pushing $fullName ..." -ForegroundColor Green
    docker push $fullName
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to push frontend image"
        exit 1
    }
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

