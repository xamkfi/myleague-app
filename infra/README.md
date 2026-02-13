# MyLeague Azure Infrastructure

This directory contains infrastructure provisioning (Bicep/IaC) and application deployment scripts, organized into separate folders.

## Folder Structure

```
infra/
├── provision/                        # Infrastructure provisioning (Bicep + scripts)
│   ├── backend.bicep                 # Backend infrastructure template
│   ├── backend.bicepparam            # Backend parameters (dev)
│   ├── frontend.bicep                # Frontend infrastructure template (SWA)
│   ├── frontend.bicepparam           # Frontend parameters (dev)
│   ├── provision-backend.ps1         # Provision backend infra (Windows)
│   ├── provision-backend.sh          # Provision backend infra (Linux/macOS)
│   ├── provision-frontend.ps1        # Provision frontend infra (Windows)
│   └── modules/
│       ├── app-service-plan.bicep    # App Service Plan module
│       ├── app-service.bicep         # App Service module
│       ├── postgresql.bicep          # PostgreSQL module
│       ├── static-web-app.bicep      # Static Web App module
│       └── storage-account.bicep     # Storage Account module
├── deploy/                           # Application deployment scripts
│   └── deploy-frontend.ps1          # Build & deploy React app to SWA (interactive)
└── README.md
```

**`provision/`** = Create or update Azure resources (Bicep infrastructure-as-code)
**`deploy/`** = Build and deploy application code to existing Azure resources

## Architecture

### Backend
- **App Service Plan** (Basic B1) - Hosts the .NET 9 API
- **App Service** - The MyLeague API application
- **PostgreSQL Flexible Server** (Burstable B1ms) - Database server
- **Storage Account** - Image uploads

### Frontend
- **Azure Static Web App** (Free tier) - Hosts the React SPA

## Prerequisites

1. **Azure CLI** - Install from https://docs.microsoft.com/en-us/cli/azure/install-azure-cli
2. **Azure Subscription** - You need an active Azure subscription
3. **Bicep CLI** - Usually included with Azure CLI
4. **pnpm** - For frontend builds (https://pnpm.io/installation)
5. **SWA CLI** - For frontend deployment (`npm install -g @azure/static-web-apps-cli`)

## Quick Start

### 1. Provision Backend Infrastructure

```powershell
cd infra/provision
.\provision-backend.ps1
```

Or on Linux/macOS:

```bash
cd infra/provision
chmod +x provision-backend.sh
./provision-backend.sh
```

### 2. Provision Frontend Infrastructure

```powershell
cd infra/provision
.\provision-frontend.ps1
```

### 3. Deploy Frontend Application

```powershell
cd infra/deploy
.\deploy-frontend.ps1
```

The deploy script is interactive and will:
1. Ask for the backend API base URL (e.g. `https://myleague-dev-api.azurewebsites.net`)
2. Ask whether to append `/api` to the URL
3. List available Azure Static Web Apps and let you pick one
4. Build the React app and deploy it

## Script Reference

### Provision Scripts (infra/provision/)

| Script | Description |
|--------|-------------|
| `provision-backend.ps1` | Provisions backend infra (App Service, PostgreSQL, Storage) |
| `provision-backend.sh` | Same as above, for Linux/macOS |
| `provision-frontend.ps1` | Provisions frontend infra (Azure Static Web App) |

#### Backend Provision Options

| Option | Description |
|--------|-------------|
| `-Environment dev` | Target environment (dev, staging, prod) |
| `-Location westeurope` | Azure region |
| `-ResourceGroupName mygroup` | Override resource group name |
| `-PostgresPassword "pass"` | PostgreSQL password (prompted if not provided) |
| `-SkipLogin` | Skip Azure login check |

#### Frontend Provision Options

| Option | Description |
|--------|-------------|
| `-Environment dev` | Target environment (dev, staging, prod) |
| `-Location westeurope` | Azure region |
| `-ApiBackendUrl "url"` | Backend API URL (auto-detected if not provided) |
| `-SkipLogin` | Skip Azure login check |

### Deploy Scripts (infra/deploy/)

| Script | Description |
|--------|-------------|
| `deploy-frontend.ps1` | Builds and deploys React app to an existing SWA |

#### Frontend Deploy Options

| Option | Description |
|--------|-------------|
| `-ApiBaseUrl "url"` | Backend API base URL (prompted if not provided) |
| `-AppendApi` | Append /api to the URL (prompted if not provided) |
| `-NoAppendApi` | Do not append /api |
| `-StaticWebAppName "name"` | Target SWA (lists available if not provided) |
| `-ResourceGroupName "rg"` | Resource group for the SWA |
| `-DeploymentToken "token"` | SWA deployment token (auto-fetched if not provided) |
| `-SkipLogin` | Skip Azure login check |

## Manual Deployment

### Deploy Backend Application

After provisioning backend infrastructure:

```powershell
# Navigate to the WebAPI project
cd src/backend/WebAPI

# Publish the application
dotnet publish -c Release -o ./publish

# Create a zip file
Compress-Archive -Path ./publish/* -DestinationPath ./app.zip -Force

# Deploy to Azure
az webapp deploy --resource-group myleague-dev-rg --name myleague-dev-api --src-path ./app.zip --type zip
```

### Run Database Migrations

```powershell
$env:ConnectionStrings__DefaultConnection = "Host=myleague-dev-postgres.postgres.database.azure.com;Database=myleague;Username=myleagueadmin;Password=YourPassword;SSL Mode=Require;Trust Server Certificate=true"

cd src/backend/WebAPI
dotnet ef database update --project ../Infrastructure/Infrastructure.csproj
```

## Estimated Costs

| Resource | SKU | Monthly Cost (approx) |
|----------|-----|----------------------|
| App Service Plan | Basic B1 | ~$13 |
| PostgreSQL Flexible Server | Burstable B1ms | ~$12 |
| Static Web App | Free | $0 |
| Storage Account | Standard_LRS | ~$0.02/GB/month |
| **Total** | | **~$25/month** |

## Troubleshooting

### View App Service Logs

```bash
az webapp log tail --resource-group myleague-dev-rg --name myleague-dev-api
```

### Check API Health

```bash
curl https://myleague-dev-api.azurewebsites.net/health/ready
```

### Connect to PostgreSQL

```bash
az postgres flexible-server firewall-rule create \
  --resource-group myleague-dev-rg \
  --name myleague-dev-postgres \
  --rule-name AllowMyIP \
  --start-ip-address <your-ip> \
  --end-ip-address <your-ip>
```

## Clean Up

To delete all resources:

```bash
az group delete --name myleague-dev-rg --yes --no-wait
```
