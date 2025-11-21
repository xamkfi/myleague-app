# Step-by-Step Deployment Guide

This guide walks you through deploying the MyLeague Azure infrastructure using Bicep templates.

## Prerequisites Checklist

Before starting, ensure you have:

- [ ] Azure account with an active subscription
- [ ] Azure CLI installed ([Download here](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli))
- [ ] PowerShell or Bash terminal
- [ ] Permissions to create resources in your Azure subscription

---

## Step 1: Verify Azure CLI Installation

Open your terminal (PowerShell on Windows, Bash on Mac/Linux) and run:

```bash
# Check Azure CLI version (should be 2.20.0 or higher)
az --version

# Check Bicep is installed (comes with Azure CLI 2.20.0+)
az bicep version
```

**Expected Output:**
```
azure-cli                         2.x.x
bicep                              0.x.x
```

If Azure CLI is not installed, download it from: https://aka.ms/installazurecliwindows

---

## Step 2: Login to Azure

```bash
# Login to your Azure account
az login
```

This will open a browser window. Sign in with your Azure account credentials.

**After login, verify your account:**
```bash
# Show your current account
az account show

# If you have multiple subscriptions, list them
az account list --output table

# Set the correct subscription (if needed)
az account set --subscription "Your-Subscription-Name-or-ID"
```

---

## Step 3: Set Up Variables

Set these variables for easy reuse throughout the deployment:

**PowerShell:**
```powershell
$RESOURCE_GROUP = "rg-myleague-bicep-dev"
$LOCATION = "westeurope"
$POSTGRES_PASSWORD = "YourSecurePassword123!"  # Change this to a strong password!
$POSTGRES_USERNAME = "myleagueadmin"
```

**Bash (Linux/Mac/Git Bash):**
```bash
RESOURCE_GROUP="rg-myleague-bicep-dev"
LOCATION="westeurope"
POSTGRES_PASSWORD="YourSecurePassword123!"  # Change this to a strong password!
POSTGRES_USERNAME="myleagueadmin"
```

> ⚠️ **Important**: Replace `YourSecurePassword123!` with a strong password. It must contain:
> - At least 8 characters
> - Mix of uppercase, lowercase, numbers, and special characters

---

## Step 4: Create Resource Group

```bash
# Create the resource group
az group create --name $RESOURCE_GROUP --location $LOCATION
```

**Expected Output:**
```json
{
  "id": "/subscriptions/.../resourceGroups/rg-myleague-bicep-dev",
  "location": "westeurope",
  "name": "rg-myleague-bicep-dev",
  ...
}
```

**Verify it was created:**
```bash
az group show --name $RESOURCE_GROUP
```

---

## Step 5: Navigate to Infrastructure Directory

```bash
# Navigate to the infra folder
cd infra

# Verify you're in the right place (should see main.bicep)
ls  # or 'dir' on Windows PowerShell
```

---

## Step 6: Validate the Bicep Template (Optional but Recommended)

Before deploying, validate the template:

```bash
az deployment group validate \
  --resource-group $RESOURCE_GROUP \
  --template-file main.bicep \
  --parameters environment=dev \
               projectName=myleague-bicep \
               location=$LOCATION \
               postgresAdminUsername=$POSTGRES_USERNAME \
               postgresAdminPassword=$POSTGRES_PASSWORD \
               appServiceSku=B1 \
               staticWebAppSku=Free
```

**Expected Output:**
```json
{
  "error": null,
  "properties": {
    ...
  }
}
```

If you see `"error": null`, the template is valid and ready to deploy.

---

## Step 7: Deploy the Infrastructure

This is the main deployment step. It will take **10-15 minutes** to complete.

```bash
az deployment group create \
  --resource-group $RESOURCE_GROUP \
  --template-file main.bicep \
  --name myleague-infra-deployment \
  --parameters environment=dev \
               projectName=myleague-bicep \
               location=$LOCATION \
               postgresAdminUsername=$POSTGRES_USERNAME \
               postgresAdminPassword=$POSTGRES_PASSWORD \
               appServiceSku=B1 \
               staticWebAppSku=Free
```

**What's happening:**
- Creating Log Analytics Workspace
- Creating Application Insights
- Creating PostgreSQL Flexible Server (takes ~5-7 minutes)
- Creating App Service Plan
- Creating App Service
- Creating Static Web App
- Configuring connections between resources

**Expected Output:**
```json
{
  "id": "/subscriptions/.../resourceGroups/rg-myleague-bicep-dev/providers/Microsoft.Resources/deployments/myleague-infra-deployment",
  "name": "myleague-infra-deployment",
  "properties": {
    "provisioningState": "Succeeded",
    ...
  }
}
```

> ⏱️ **Wait Time**: This deployment typically takes 10-15 minutes. The PostgreSQL server creation is the longest step.

---

## Step 8: Get Deployment Outputs

After deployment succeeds, retrieve the important URLs and connection strings:

```bash
# Get all outputs
az deployment group show \
  --resource-group $RESOURCE_GROUP \
  --name myleague-infra-deployment \
  --query properties.outputs
```

**Save these values** - you'll need them later:

```bash
# Get specific outputs (PowerShell)
$OUTPUTS = az deployment group show --resource-group $RESOURCE_GROUP --name myleague-infra-deployment --query properties.outputs -o json | ConvertFrom-Json

# Backend URL
Write-Host "Backend URL: $($OUTPUTS.appServiceUrl.value)"

# Frontend URL
Write-Host "Frontend URL: $($OUTPUTS.staticWebAppUrl.value)"

# PostgreSQL Server
Write-Host "PostgreSQL Server: $($OUTPUTS.postgresServerFqdn.value)"

# Static Web App Deployment Token (for CI/CD)
Write-Host "SWA Deployment Token: $($OUTPUTS.staticWebAppDeploymentToken.value)"
```

**Bash:**
```bash
# Get specific outputs
BACKEND_URL=$(az deployment group show --resource-group $RESOURCE_GROUP --name myleague-infra-deployment --query properties.outputs.appServiceUrl.value -o tsv)
FRONTEND_URL=$(az deployment group show --resource-group $RESOURCE_GROUP --name myleague-infra-deployment --query properties.outputs.staticWebAppUrl.value -o tsv)
POSTGRES_SERVER=$(az deployment group show --resource-group $RESOURCE_GROUP --name myleague-infra-deployment --query properties.outputs.postgresServerFqdn.value -o tsv)
SWA_TOKEN=$(az deployment group show --resource-group $RESOURCE_GROUP --name myleague-infra-deployment --query properties.outputs.staticWebAppDeploymentToken.value -o tsv)

echo "Backend URL: $BACKEND_URL"
echo "Frontend URL: $FRONTEND_URL"
echo "PostgreSQL Server: $POSTGRES_SERVER"
echo "SWA Deployment Token: $SWA_TOKEN"
```

---

## Step 9: Verify Deployment

### Check All Resources Were Created

```bash
# List all resources in the resource group
az resource list --resource-group $RESOURCE_GROUP --output table
```

**Expected Resources:**
- `app-myleague-bicep-dev` (App Service)
- `asp-myleague-bicep-dev` (App Service Plan)
- `psql-myleague-bicep-dev` (PostgreSQL Server)
- `swa-myleague-bicep-dev` (Static Web App)
- `appi-myleague-bicep-dev` (Application Insights)
- `log-myleague-bicep-dev` (Log Analytics)

### Test Backend Health Endpoint

```bash
# Get backend URL
$BACKEND_URL = (az deployment group show --resource-group $RESOURCE_GROUP --name myleague-infra-deployment --query properties.outputs.appServiceUrl.value -o tsv)

# Test health endpoint (may fail until app is deployed)
curl "$BACKEND_URL/health"
```

> Note: The health endpoint will return 404 until you deploy your application code.

### Check Resource Status in Azure Portal

```bash
# Open Azure Portal to view resources
az portal --resource-group $RESOURCE_GROUP
```

---

## Step 10: Save Important Information

Create a file to store your deployment information:

**PowerShell:**
```powershell
$OUTPUTS = az deployment group show --resource-group $RESOURCE_GROUP --name myleague-infra-deployment --query properties.outputs -o json | ConvertFrom-Json

@"
MyLeague Azure Infrastructure - Deployment Information
=======================================================
Deployment Date: $(Get-Date)
Resource Group: $RESOURCE_GROUP

Backend (App Service):
  URL: $($OUTPUTS.appServiceUrl.value)
  Name: app-myleague-bicep-dev

Frontend (Static Web App):
  URL: $($OUTPUTS.staticWebAppUrl.value)
  Name: swa-myleague-bicep-dev
  Deployment Token: $($OUTPUTS.staticWebAppDeploymentToken.value)

Database (PostgreSQL):
  Server: $($OUTPUTS.postgresServerFqdn.value)
  Database: $($OUTPUTS.postgresDatabaseName.value)
  Username: $POSTGRES_USERNAME
  Password: [Stored securely - use Key Vault in production]

Application Insights:
  Name: appi-myleague-bicep-dev
  Connection String: $($OUTPUTS.appInsightsConnectionString.value)
"@ | Out-File -FilePath "deployment-info.txt"

Write-Host "Deployment information saved to deployment-info.txt"
```

---

## Troubleshooting

### Deployment Fails

**Check deployment status:**
```bash
az deployment group show \
  --resource-group $RESOURCE_GROUP \
  --name myleague-infra-deployment \
  --query properties.provisioningState
```

**View detailed error:**
```bash
az deployment group show \
  --resource-group $RESOURCE_GROUP \
  --name myleague-infra-deployment \
  --query properties.error
```

**Common Issues:**

1. **"Resource name already exists"**
   - Solution: Delete the existing resource or use a different project name

2. **"Insufficient permissions"**
   - Solution: Ensure your account has Contributor or Owner role on the subscription

3. **"PostgreSQL server creation failed"**
   - Solution: Wait a few minutes and retry. PostgreSQL creation can take 5-7 minutes.

4. **"Location not available"**
   - Solution: Try a different location like `eastus` or `northeurope`

### View Deployment Logs

```bash
# View activity log for the resource group
az monitor activity-log list \
  --resource-group $RESOURCE_GROUP \
  --max-events 20 \
  --output table
```

---

## Next Steps After Deployment

1. **Deploy Backend Application**
   - Build and publish your .NET 9 API
   - Deploy to App Service using ZIP deployment or GitHub Actions

2. **Deploy Frontend Application**
   - Build your React app with `npm run build`
   - Deploy to Static Web App using SWA CLI or GitHub Actions

3. **Run Database Migrations**
   - Connect to PostgreSQL and run Entity Framework migrations

4. **Configure Environment Variables**
   - Update frontend `.env.production` with backend URL
   - Configure App Service connection strings if needed

5. **Set Up CI/CD**
   - Configure GitHub Actions or Azure DevOps pipelines
   - Use the Static Web App deployment token for automated deployments

---

## Clean Up (When Done Testing)

To delete all resources and stop incurring costs:

```bash
# Delete the entire resource group (WARNING: This deletes everything!)
az group delete --name $RESOURCE_GROUP --yes --no-wait
```

> ⚠️ **Warning**: This permanently deletes all resources including the database and all data!

---

## Quick Reference Commands

```bash
# View all resources
az resource list --resource-group $RESOURCE_GROUP --output table

# View deployment outputs
az deployment group show --resource-group $RESOURCE_GROUP --name myleague-infra-deployment --query properties.outputs

# View App Service logs
az webapp log tail --resource-group $RESOURCE_GROUP --name app-myleague-bicep-dev

# Open Azure Portal
az portal --resource-group $RESOURCE_GROUP
```

---

## Support

If you encounter issues:
1. Check the [main README.md](README.md) for detailed documentation
2. Review Azure deployment logs in the portal
3. Verify all prerequisites are met
4. Check Azure service health status

