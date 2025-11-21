# MyLeague Azure Infrastructure

This directory contains Bicep templates to provision Azure infrastructure for the MyLeague application.

## Architecture Overview

The infrastructure consists of:

- **Azure App Service (Linux B1)**: Hosts the .NET 9 backend API
- **Azure Static Web App (Free)**: Hosts the React frontend (Vite)
- **Azure Database for PostgreSQL Flexible Server (Burstable B1ms)**: Database server
- **Application Insights**: Application monitoring and telemetry
- **Log Analytics Workspace**: Centralized logging

## Prerequisites

Before deploying, ensure you have:

1. **Azure CLI** installed ([Install Guide](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli))
2. **Azure Subscription** with appropriate permissions
3. **Bicep CLI** (automatically installed with Azure CLI 2.20.0+)

### Verify Installation

```bash
# Check Azure CLI version
az --version

# Check Bicep version
az bicep version

# Login to Azure
az login

# Set your subscription (if you have multiple)
az account set --subscription "Your-Subscription-Name-or-ID"
```

## Deployment Steps

### 1. Create Resource Group

```bash
# Set variables
RESOURCE_GROUP="rg-myleague-bicep-dev"
LOCATION="westeurope"

# Create resource group
az group create --name $RESOURCE_GROUP --location $LOCATION
```

### 2. Set PostgreSQL Admin Password

You have two options for the PostgreSQL password:

#### Option A: Use Command Line Parameter (Quick for Dev)

```bash
# Set password as variable
POSTGRES_PASSWORD="YourSecurePassword123!"
```

#### Option B: Use Azure Key Vault (Recommended for Production)

```bash
# Create Key Vault
KEY_VAULT_NAME="kv-myleague-bicep-dev"
az keyvault create \
  --name $KEY_VAULT_NAME \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION

# Store password in Key Vault
az keyvault secret set \
  --vault-name $KEY_VAULT_NAME \
  --name "postgres-admin-password" \
  --value "YourSecurePassword123!"

# Update parameters.dev.json with your Key Vault details
```

### 3. Deploy Infrastructure

#### Option A: With Command Line Parameters (Quick)

```bash
az deployment group create \
  --resource-group $RESOURCE_GROUP \
  --template-file main.bicep \
  --parameters environment=dev \
               projectName=myleague-bicep \
               location=$LOCATION \
               postgresAdminUsername=myleagueadmin \
               postgresAdminPassword=$POSTGRES_PASSWORD \
               appServiceSku=B1 \
               staticWebAppSku=Free
```

#### Option B: With Parameters File

First, edit `parameters.dev.json` to update the Key Vault reference, then:

```bash
az deployment group create \
  --resource-group $RESOURCE_GROUP \
  --template-file main.bicep \
  --parameters parameters.dev.json \
               postgresAdminPassword=$POSTGRES_PASSWORD
```

### 4. View Deployment Outputs

```bash
# Get deployment outputs
az deployment group show \
  --resource-group $RESOURCE_GROUP \
  --name main \
  --query properties.outputs
```

The outputs will include:
- Frontend URL (Static Web App)
- Backend API URL (App Service)
- PostgreSQL server FQDN
- Application Insights keys
- Static Web App deployment token

### 5. Save Important Values

```bash
# Get Static Web App deployment token for CI/CD
az deployment group show \
  --resource-group $RESOURCE_GROUP \
  --name main \
  --query properties.outputs.staticWebAppDeploymentToken.value \
  --output tsv

# Get backend URL
az deployment group show \
  --resource-group $RESOURCE_GROUP \
  --name main \
  --query properties.outputs.appServiceUrl.value \
  --output tsv
```

## Post-Deployment Configuration

### 1. Deploy Backend Application

```bash
# Build and publish the backend
cd ../src/backend/WebAPI
dotnet publish -c Release -o ./publish

# Create deployment package
cd publish
zip -r ../deploy.zip .

# Deploy to App Service
az webapp deployment source config-zip \
  --resource-group $RESOURCE_GROUP \
  --name app-myleague-bicep-dev \
  --src ../deploy.zip
```

### 2. Deploy Frontend Application

```bash
# Get the deployment token (from step 5 above or deployment outputs)
SWA_TOKEN="<your-deployment-token>"

# Install SWA CLI
npm install -g @azure/static-web-apps-cli

# Build frontend
cd ../../../src/frontend
npm install
npm run build

# Deploy to Static Web App
swa deploy ./dist \
  --deployment-token $SWA_TOKEN \
  --app-location ./dist \
  --env production
```

### 3. Configure Frontend API Connection

Create or update `src/frontend/public/staticwebapp.config.json`:

```json
{
  "routes": [
    {
      "route": "/api/*",
      "allowedRoles": ["anonymous"]
    }
  ],
  "navigationFallback": {
    "rewrite": "/index.html",
    "exclude": ["/images/*.{png,jpg,gif,ico}", "/*.{css,scss,js}"]
  },
  "responseOverrides": {
    "404": {
      "rewrite": "/index.html"
    }
  }
}
```

Update your frontend environment configuration to use the backend URL:
- Create `.env.production` with `VITE_API_URL=https://app-myleague-bicep-dev.azurewebsites.net/api`

### 4. Run Database Migrations

```bash
# Connect to your PostgreSQL and run migrations
# Option 1: From local machine with EF Core
cd src/backend/WebAPI
dotnet ef database update --connection "Host=psql-myleague-bicep-dev.postgres.database.azure.com;Database=myleague;Username=myleagueadmin;Password=YourPassword;SSL Mode=Require"

# Option 2: Use Azure Cloud Shell or deploy a migration job
```

## Validate Deployment

```bash
# Check App Service health
BACKEND_URL=$(az deployment group show --resource-group $RESOURCE_GROUP --name main --query properties.outputs.appServiceUrl.value -o tsv)
curl "$BACKEND_URL/health"

# Check frontend
FRONTEND_URL=$(az deployment group show --resource-group $RESOURCE_GROUP --name main --query properties.outputs.staticWebAppUrl.value -o tsv)
echo "Frontend URL: $FRONTEND_URL"
```

## Managing Resources

### View Resources

```bash
# List all resources in the resource group
az resource list --resource-group $RESOURCE_GROUP --output table
```

### Update Infrastructure

```bash
# Make changes to bicep files, then redeploy
az deployment group create \
  --resource-group $RESOURCE_GROUP \
  --template-file main.bicep \
  --parameters parameters.dev.json \
               postgresAdminPassword=$POSTGRES_PASSWORD
```

### Monitor Application

```bash
# Open Application Insights in portal
az portal show --resource-group $RESOURCE_GROUP --resource-name appi-myleague-bicep-dev

# View logs
az monitor app-insights query \
  --app appi-myleague-bicep-dev \
  --resource-group $RESOURCE_GROUP \
  --analytics-query "requests | take 10"
```

### Scale Resources

```bash
# Scale App Service Plan
az appservice plan update \
  --resource-group $RESOURCE_GROUP \
  --name asp-myleague-bicep-dev \
  --sku B2

# Scale PostgreSQL
az postgres flexible-server update \
  --resource-group $RESOURCE_GROUP \
  --name psql-myleague-bicep-dev \
  --sku-name Standard_B2s
```

## Clean Up

To remove all resources:

```bash
# Delete the entire resource group (WARNING: This deletes everything!)
az group delete --name $RESOURCE_GROUP --yes --no-wait
```

## Cost Estimation (Development Environment)

Approximate monthly costs for dev environment:

- App Service B1 (Linux): ~$13/month
- PostgreSQL Flexible Server B1ms: ~$12/month
- Static Web App (Free): $0/month
- Application Insights: ~$2-5/month (based on usage)
- Log Analytics: ~$0-2/month (based on ingestion)

**Total: ~$27-32/month**

## Troubleshooting

### Deployment Fails

```bash
# View deployment logs
az deployment group show \
  --resource-group $RESOURCE_GROUP \
  --name main

# Check activity log
az monitor activity-log list \
  --resource-group $RESOURCE_GROUP \
  --max-events 50
```

### App Service Not Starting

```bash
# View App Service logs
az webapp log tail \
  --resource-group $RESOURCE_GROUP \
  --name app-myleague-bicep-dev

# Enable application logging
az webapp log config \
  --resource-group $RESOURCE_GROUP \
  --name app-myleague-bicep-dev \
  --application-logging filesystem
```

### Database Connection Issues

```bash
# Test PostgreSQL connection
az postgres flexible-server connect \
  --name psql-myleague-bicep-dev \
  --admin-user myleagueadmin \
  --admin-password $POSTGRES_PASSWORD

# Check firewall rules
az postgres flexible-server firewall-rule list \
  --resource-group $RESOURCE_GROUP \
  --name psql-myleague-bicep-dev
```

## Next Steps

1. **Set up CI/CD**: Configure GitHub Actions or Azure DevOps pipelines
2. **Add Custom Domain**: Configure custom domains for Static Web App and App Service
3. **Enable SSL**: Already enabled by default on Azure services
4. **Configure Monitoring Alerts**: Set up alerts in Application Insights
5. **Implement Backup Strategy**: Configure PostgreSQL backups for production
6. **Add Azure Key Vault**: Store secrets securely (when ready)

## Resources

- [Azure Bicep Documentation](https://docs.microsoft.com/en-us/azure/azure-resource-manager/bicep/)
- [Azure App Service Documentation](https://docs.microsoft.com/en-us/azure/app-service/)
- [Azure Static Web Apps Documentation](https://docs.microsoft.com/en-us/azure/static-web-apps/)
- [Azure PostgreSQL Documentation](https://docs.microsoft.com/en-us/azure/postgresql/)
- [Application Insights Documentation](https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)

