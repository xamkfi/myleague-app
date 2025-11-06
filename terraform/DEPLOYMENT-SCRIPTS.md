# Deployment Scripts Guide

This directory contains PowerShell scripts for building and deploying Docker images to Azure Container Registry (ACR) and optionally to Azure Container Apps.

## Scripts Overview

### `deploy-backend.ps1`
Builds, tags, and pushes the backend Docker image to ACR. Optionally deploys to Azure Container Apps.

### `deploy-frontend.ps1`
Builds, tags, and pushes the frontend Docker image to ACR. Supports local Docker builds or ACR cloud builds. Optionally deploys to Azure Container Apps.

### `deploy-all.ps1`
Wrapper script that runs both backend and frontend deployments sequentially.

## Usage Examples

### Basic Usage (Build and Push Only)

```powershell
# Deploy backend only
.\terraform\deploy-backend.ps1 -AcrName myleagueacrom63tv -Tag latest

# Deploy frontend only
.\terraform\deploy-frontend.ps1 -AcrName myleagueacrom63tv -Tag latest -ApiUrl https://myleague-backend.xyz.azurecontainerapps.io

# Deploy both
.\terraform\deploy-all.ps1 -AcrName myleagueacrom63tv -Tag latest -ApiUrl https://myleague-backend.xyz.azurecontainerapps.io
```

### With Azure Container Apps Deployment

```powershell
# Deploy backend and update Container App
.\terraform\deploy-backend.ps1 `
    -AcrName myleagueacrom63tv `
    -Tag latest `
    -ResourceGroup myleague-rg `
    -ContainerAppName myleague-backend `
    -Deploy

# Deploy frontend and update Container App
.\terraform\deploy-frontend.ps1 `
    -AcrName myleagueacrom63tv `
    -Tag latest `
    -ResourceGroup myleague-rg `
    -ContainerAppName myleague-frontend `
    -ApiUrl https://myleague-backend.xyz.azurecontainerapps.io `
    -Deploy

# Deploy both with Container Apps update
.\terraform\deploy-all.ps1 `
    -AcrName myleagueacrom63tv `
    -Tag latest `
    -ResourceGroup myleague-rg `
    -BackendAppName myleague-backend `
    -FrontendAppName myleague-frontend `
    -ApiUrl https://myleague-backend.xyz.azurecontainerapps.io `
    -Deploy
```

### Using ACR Cloud Builds (Frontend Only)

For frontend, you can build directly in Azure Container Registry instead of locally:

```powershell
.\terraform\deploy-frontend.ps1 `
    -AcrName myleagueacrom63tv `
    -Tag latest `
    -ApiUrl https://myleague-backend.xyz.azurecontainerapps.io `
    -UseAcrBuild
```

## Parameters

### Common Parameters

| Parameter | Required | Default | Description |
|-----------|----------|---------|-------------|
| `AcrName` | Yes | - | Azure Container Registry name |
| `Tag` | No | `latest` | Docker image tag |
| `ResourceGroup` | No | - | Resource group for Container Apps (required with `-Deploy`) |
| `ContainerAppName` | No | - | Container App name (required with `-Deploy`) |
| `Deploy` | No | - | Switch to enable Container Apps deployment |

### Backend-Specific Parameters

| Parameter | Required | Default | Description |
|-----------|----------|---------|-------------|
| `ImageName` | No | `webapi` | Image repository name in ACR |
| `Dockerfile` | No | `src/backend/WebAPI/Dockerfile` | Path to Dockerfile |
| `Context` | No | `.` | Build context (project root) |

### Frontend-Specific Parameters

| Parameter | Required | Default | Description |
|-----------|----------|---------|-------------|
| `ImageName` | No | `frontend` | Image repository name in ACR |
| `Dockerfile` | No | `src/frontend/Dockerfile.prod` | Path to Dockerfile (falls back to `Dockerfile` if `.prod` doesn't exist) |
| `Context` | No | `src/frontend` | Build context |
| `ApiUrl` | No | `/api` | Backend API URL for build-time configuration |
| `UseAcrBuild` | No | - | Use ACR cloud build instead of local Docker |

### Deploy-All Parameters

| Parameter | Required | Default | Description |
|-----------|----------|---------|-------------|
| `BackendAppName` | No | - | Backend Container App name |
| `FrontendAppName` | No | - | Frontend Container App name |

## Benefits

1. **Separation of Concerns**: Deploy backend and frontend independently
2. **Easier Rollbacks**: Rollback one service without affecting the other
3. **Flexible Tagging**: Use different tags for backend and frontend
4. **Optional Deployment**: Build/push without deploying to Container Apps
5. **Better Error Handling**: Clear error messages and exit codes
6. **Cloud Build Support**: Option to build frontend in ACR instead of locally

## Error Handling

All scripts use `$ErrorActionPreference = "Stop"` and check `$LASTEXITCODE` after each critical command. Scripts will exit with code 1 on failure.

## Requirements

- PowerShell 5.1 or later
- Azure CLI installed and logged in (`az login`)
- Docker installed and running (for local builds)
- Appropriate Azure permissions for ACR and Container Apps

## Notes

- Scripts automatically change to the project root directory
- Dockerfile paths are relative to the project root
- If `Dockerfile.prod` doesn't exist for frontend, it automatically falls back to `Dockerfile`
- The `-ApiUrl` parameter is only used if your frontend Dockerfile accepts a `VITE_API_URL` build argument

