# Deployment Scripts

This directory contains PowerShell scripts for building and deploying Docker images to Azure Container Registry (ACR) and Azure Container Apps.

## Scripts

- **`deploy-backend.ps1`** - Builds, tags, and pushes the backend Docker image
- **`deploy-frontend.ps1`** - Builds, tags, and pushes the frontend Docker image
- **`deploy-all.ps1`** - Wrapper script that deploys both backend and frontend
- **`seed-data.ps1`** - Populates the database with initial test data
- **`test-scripts.ps1`** - Test script to validate all scripts work correctly

## Quick Start

### Basic Usage

```powershell
# Get ACR name and backend URL
cd terraform
$ACR_NAME = terraform output -raw container_registry_name
$BACKEND_URL = terraform output -raw backend_url
$API_URL = "$BACKEND_URL/api"
cd ..

# Deploy with version tag
.\scripts\deploy-all.ps1 `
    -AcrName $ACR_NAME `
    -Tag "1.2.3" `
    -ApiUrl $API_URL `
    -BackendAppName "myleague-dev-backend" `
    -FrontendAppName "myleague-dev-frontend" `
    -ResourceGroup "myleague-dev-rg" `
    -Deploy
```

## Documentation

- **[VERSIONING-GUIDE.md](./VERSIONING-GUIDE.md)** - Complete guide on versioning and deploying new versions
- **[SEEDING-GUIDE.md](./SEEDING-GUIDE.md)** - Complete guide on database seeding
- **[../terraform/DEPLOYMENT-SCRIPTS.md](../terraform/DEPLOYMENT-SCRIPTS.md)** - Detailed script documentation
- **[../terraform/DEPLOYMENT-GUIDE.md](../terraform/DEPLOYMENT-GUIDE.md)** - Full deployment guide

## Testing

Run the test script to validate all scripts work correctly:

```powershell
.\scripts\test-scripts.ps1
```

## Common Tasks

### Deploy New Version

See [VERSIONING-GUIDE.md](./VERSIONING-GUIDE.md) for detailed instructions.

```powershell
$VERSION = "1.2.3"
.\scripts\deploy-all.ps1 -AcrName $ACR_NAME -Tag $VERSION -ApiUrl $API_URL ... -Deploy
```

### Seed Database with Test Data

After deploying the backend, populate the database with initial test data:

```powershell
# Get backend URL from Terraform
cd terraform
$backendUrl = terraform output -raw backend_url
cd ..

# Run the seeder
.\scripts\seed-data.ps1 -BackendUrl $backendUrl
```

Or in one line:
```powershell
.\scripts\seed-data.ps1 -BackendUrl "https://your-backend-url.azurecontainerapps.io"
```

**Note**: Make sure your infrastructure is deployed and the backend is running before seeding.

### Rollback to Previous Version

```powershell
az containerapp update `
    --name myleague-dev-backend `
    --resource-group myleague-dev-rg `
    --image "$ACR_NAME.azurecr.io/webapi:1.2.2"
```

### List Available Versions

```powershell
az acr repository show-tags --name $ACR_NAME --repository webapi --output table
```

## Script Parameters

### Common Parameters

| Parameter | Required | Default | Description |
|-----------|----------|---------|-------------|
| `AcrName` | Yes | - | Azure Container Registry name |
| `Tag` | No | `latest` | Docker image tag (version) |
| `ResourceGroup` | No | - | Resource group for Container Apps |
| `ContainerAppName` | No | - | Container App name |
| `Deploy` | No | - | Switch to enable Container Apps deployment |

### Backend-Specific

| Parameter | Default | Description |
|-----------|---------|-------------|
| `ImageName` | `webapi` | Image repository name in ACR |
| `Dockerfile` | `src/backend/WebAPI/Dockerfile` | Path to Dockerfile |
| `Context` | `.` | Build context (project root) |

### Frontend-Specific

| Parameter | Default | Description |
|-----------|---------|-------------|
| `ImageName` | `frontend` | Image repository name in ACR |
| `Dockerfile` | `src/frontend/Dockerfile.prod` | Path to Dockerfile |
| `Context` | `src/frontend` | Build context |
| `ApiUrl` | `/api` | Backend API URL for build-time configuration |
| `UseAcrBuild` | - | Use ACR cloud build instead of local Docker |

## Examples

### Deploy Backend Only

```powershell
.\scripts\deploy-backend.ps1 `
    -AcrName $ACR_NAME `
    -Tag "1.2.3" `
    -ResourceGroup "myleague-dev-rg" `
    -ContainerAppName "myleague-dev-backend" `
    -Deploy
```

### Deploy Frontend Only

```powershell
.\scripts\deploy-frontend.ps1 `
    -AcrName $ACR_NAME `
    -Tag "1.2.3" `
    -ApiUrl $API_URL `
    -ResourceGroup "myleague-dev-rg" `
    -ContainerAppName "myleague-dev-frontend" `
    -Deploy
```

### Build Without Deploying

```powershell
# Build and push only (no deployment)
.\scripts\deploy-backend.ps1 -AcrName $ACR_NAME -Tag "1.2.3"
# (No -Deploy flag)
```

### Use ACR Cloud Build (Frontend)

```powershell
.\scripts\deploy-frontend.ps1 `
    -AcrName $ACR_NAME `
    -Tag "1.2.3" `
    -ApiUrl $API_URL `
    -UseAcrBuild
```

## Need Help?

- Check [VERSIONING-GUIDE.md](./VERSIONING-GUIDE.md) for versioning best practices
- See [../terraform/DEPLOYMENT-SCRIPTS.md](../terraform/DEPLOYMENT-SCRIPTS.md) for detailed parameter documentation
- Run `Get-Help .\scripts\deploy-backend.ps1` for script help

