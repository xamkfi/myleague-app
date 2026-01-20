# MyLeague Azure Infrastructure

This directory contains Bicep templates for deploying the MyLeague infrastructure to Azure.

## Architecture

The infrastructure consists of:

### Backend
- **App Service Plan** (Basic B1) - Hosts the .NET 9 API
- **App Service** - The MyLeague API application
- **PostgreSQL Flexible Server** (Burstable B1ms) - Database server

### Frontend
- **Azure Static Web App** (Free tier) - Hosts the React SPA

## Prerequisites

1. **Azure CLI** - Install from https://docs.microsoft.com/en-us/cli/azure/install-azure-cli
2. **Azure Subscription** - You need an active Azure subscription
3. **Bicep CLI** - Usually included with Azure CLI, or install separately

### Verify Installation

```bash
# Check Azure CLI version
az --version

# Check Bicep version
az bicep version
```

## Quick Start (Recommended)

Use the deployment scripts for the easiest experience:

### Windows (PowerShell)

```powershell
# Navigate to infra folder
cd infra

# Run deployment script (interactive)
.\deploy.ps1

# Or with parameters
.\deploy.ps1 -Environment dev -Location westeurope
```

### Linux / macOS (Bash)

```bash
# Navigate to infra folder
cd infra

# Make script executable (first time only)
chmod +x deploy.sh

# Run deployment script (interactive)
./deploy.sh

# Or with parameters
./deploy.sh -e dev -l westeurope
```

### Script Options

| Option | PowerShell | Bash | Description |
|--------|------------|------|-------------|
| Environment | `-Environment dev` | `-e dev` | Target environment (dev, staging, prod) |
| Location | `-Location westeurope` | `-l westeurope` | Azure region |
| Resource Group | `-ResourceGroupName mygroup` | `-g mygroup` | Override resource group name |
| Password | `-PostgresPassword "pass"` | `-p "pass"` | PostgreSQL password (prompted if not provided) |
| Skip Login | `-SkipLogin` | `-s` | Skip Azure login check |

---

## Frontend Deployment

After deploying the backend, deploy the frontend:

### Windows (PowerShell)

```powershell
# Deploy infrastructure only
.\deploy-frontend.ps1

# Deploy infrastructure AND application
.\deploy-frontend.ps1 -DeployApp

# With specific backend URL
.\deploy-frontend.ps1 -ApiBackendUrl "https://myleague-dev-api.azurewebsites.net" -DeployApp
```

### Frontend Script Options

| Option | Description |
|--------|-------------|
| `-Environment dev` | Target environment (dev, staging, prod) |
| `-Location westeurope` | Azure region |
| `-ApiBackendUrl "url"` | Backend API URL (auto-detected if not provided) |
| `-DeployApp` | Also build and deploy the React application |
| `-SkipLogin` | Skip Azure login check |

### Manual Frontend Deployment

```bash
# 1. Deploy infrastructure
az deployment group create \
  --resource-group myleague-dev-rg \
  --template-file main-frontend.bicep \
  --parameters main-frontend.bicepparam

# 2. Get deployment token
az staticwebapp secrets list \
  --name myleague-dev-web \
  --query "properties.apiKey" -o tsv

# 3. Build and deploy React app
cd src/frontend
pnpm install
pnpm run build
swa deploy ./dist --deployment-token <your-token>
```

---

## Manual Backend Deployment

If you prefer manual deployment:

### 1. Login to Azure

```bash
az login
```

### 2. Set Your Subscription (if you have multiple)

```bash
# List subscriptions
az account list --output table

# Set the subscription you want to use
az account set --subscription "Your Subscription Name"
```

### 3. Create Resource Group

```bash
# Create resource group in West Europe (or your preferred region)
az group create --name myleague-dev-rg --location westeurope
```

### 4. Deploy Infrastructure

```bash
# Deploy with password prompt
az deployment group create \
  --resource-group myleague-dev-rg \
  --template-file main.bicep \
  --parameters main.bicepparam

# Or provide password directly (not recommended for production)
az deployment group create \
  --resource-group myleague-dev-rg \
  --template-file main.bicep \
  --parameters main.bicepparam \
  --parameters postgresAdminPassword='YourSecurePassword123!'
```

### 5. Get Deployment Outputs

```bash
# View all outputs
az deployment group show \
  --resource-group myleague-dev-rg \
  --name main \
  --query properties.outputs

# Get just the API URL
az deployment group show \
  --resource-group myleague-dev-rg \
  --name main \
  --query properties.outputs.apiUrl.value -o tsv
```

## Deploying Your Application

After the infrastructure is deployed, deploy your .NET API:

### Option 1: Using Azure CLI

```bash
# Navigate to the WebAPI project
cd src/backend/WebAPI

# Publish the application
dotnet publish -c Release -o ./publish

# Create a zip file
Compress-Archive -Path ./publish/* -DestinationPath ./app.zip -Force

# Deploy to Azure
az webapp deploy \
  --resource-group myleague-dev-rg \
  --name myleague-dev-api \
  --src-path ./app.zip \
  --type zip
```

### Option 2: Using Visual Studio / Rider

1. Right-click on the WebAPI project
2. Select "Publish"
3. Choose "Azure" as target
4. Select your App Service (myleague-dev-api)
5. Click Publish

### Option 3: GitHub Actions (CI/CD)

Set up GitHub Actions with the publish profile:

```bash
# Get the publish profile
az webapp deployment list-publishing-profiles \
  --resource-group myleague-dev-rg \
  --name myleague-dev-api \
  --xml
```

## Database Migrations

After deployment, run Entity Framework migrations:

```bash
# Set connection string environment variable
$env:ConnectionStrings__DefaultConnection = "Host=myleague-dev-postgres.postgres.database.azure.com;Database=myleague;Username=myleagueadmin;Password=YourPassword;SSL Mode=Require;Trust Server Certificate=true"

# Navigate to WebAPI project
cd src/backend/WebAPI

# Run migrations
dotnet ef database update --project ../Infrastructure/Infrastructure.csproj
```

Or connect to the database using a tool like Azure Data Studio or pgAdmin.

## Estimated Costs

| Resource | SKU | Monthly Cost (approx) |
|----------|-----|----------------------|
| App Service Plan | Basic B1 | ~$13 |
| PostgreSQL Flexible Server | Burstable B1ms | ~$12 |
| Static Web App | Free | $0 |
| **Total** | | **~$25/month** |

## Customization

### Change Region

Edit `main.bicepparam` and update the `location` parameter:

```bicep
param location = 'northeurope'  // or any other Azure region
```

### Change SKUs

For more performance, update the SKU parameters in `main.bicepparam`:

```bicep
param appServicePlanSku = 'B2'           // More CPU/RAM
param postgresSku = 'Standard_B2s'       // More database resources
```

### Add CORS Origins

Update the `allowedOrigins` array for your frontend URLs:

```bicep
param allowedOrigins = [
  'http://localhost:5173'
  'https://your-frontend-domain.com'
]
```

## Troubleshooting

### View App Service Logs

```bash
# Stream live logs
az webapp log tail --resource-group myleague-dev-rg --name myleague-dev-api

# Download logs
az webapp log download --resource-group myleague-dev-rg --name myleague-dev-api
```

### Check App Service Health

```bash
# Check if the API is healthy
curl https://myleague-dev-api.azurewebsites.net/health/ready
```

### Connect to PostgreSQL

```bash
# Allow your IP through firewall
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

## File Structure

```
infra/
├── deploy.ps1                 # Backend deployment script (Windows)
├── deploy.sh                  # Backend deployment script (Linux/macOS)
├── deploy-frontend.ps1        # Frontend deployment script (Windows)
├── main.bicep                 # Backend infrastructure template
├── main.bicepparam            # Backend parameters (dev)
├── main-frontend.bicep        # Frontend infrastructure template
├── main-frontend.bicepparam   # Frontend parameters (dev)
├── modules/
│   ├── app-service-plan.bicep # App Service Plan module
│   ├── app-service.bicep      # App Service module
│   ├── postgresql.bicep       # PostgreSQL module
│   └── static-web-app.bicep   # Static Web App module
└── README.md                  # This file
```
