# Azure Deployment Guide for MyLeague App

This guide will help you deploy the MyLeague application to Azure using Terraform.

## Prerequisites

1. **Azure CLI** installed and configured
   ```bash
   az login
   az account set --subscription "Your Subscription Name"
   ```

2. **Terraform** installed (version >= 1.0)
   - Download from: https://www.terraform.io/downloads

3. **Docker** installed and running

4. **Azure Subscription** with appropriate permissions

## Architecture

The Terraform configuration deploys:

- **Azure Container Registry (ACR)** - For storing Docker images
- **Azure Database for PostgreSQL Flexible Server** - Database backend (private, VNet-integrated)
- **Azure Container Apps Environment** - Container hosting platform (with VNet integration)
- **Backend Container App** - .NET 9 WebAPI application
- **Frontend Container App** - React/Vite frontend application
- **Azure Key Vault** - Secure storage for connection strings and secrets
- **Application Insights** - Application monitoring and logging
- **Log Analytics Workspace** - Centralized logging
- **Virtual Network (VNet)** - Private networking for secure database access
- **Jump Box VM** (optional) - Ubuntu VM for accessing private PostgreSQL database (~€4-18/month)

## Quick Start

> **🎯 IMPORTANT: Development vs Production**
> 
> This guide supports two deployment modes:
> - **Development Mode** (Recommended for you!) - Simpler, cheaper (~€30-40/month), direct database access from DBeaver, no jump box needed
> - **Production Mode** (For 5000 users) - Secure, expensive (~€200-260/month), private database with jump box
>
> **👉 See [DEPLOYMENT-GUIDE.md](./DEPLOYMENT-GUIDE.md) for complete step-by-step deployment instructions.**
>
> **👉 See [DEV-VS-PROD-DEPLOYMENT.md](./DEV-VS-PROD-DEPLOYMENT.md) for a detailed comparison.**
>
> **TL;DR for Development:**
> ```bash
> # Get your IP: curl https://ifconfig.me
> # Add to terraform.tfvars.dev: allowed_ip_addresses = ["YOUR_IP"]
> terraform apply -var-file="terraform.tfvars.dev"
> # Connect DBeaver directly - no SSH, no jump box! 🎉
> ```

### 1. Initialize Terraform Backend (Optional but Recommended)

First, create a storage account for Terraform state:

**PowerShell (Windows):**
```powershell
# Create resource group for Terraform state
az group create --name terraform-state-rg --location "West Europe"

# Generate a unique storage account name (must be globally unique, lowercase, 3-24 chars)
$randomSuffix = -join ((48..57) + (97..122) | Get-Random -Count 6 | ForEach-Object {[char]$_})
$storageAccountName = "tfstate$randomSuffix"

# Or pick your own unique name
# $storageAccountName = "tfstatemyleague12345"

# Create storage account
az storage account create `
  --resource-group terraform-state-rg `
  --name $storageAccountName `
  --sku Standard_LRS `
  --location "West Europe"

# Create container
az storage container create `
  --name tfstate `
  --account-name $storageAccountName

# Save the storage account name
Write-Host "Storage Account Name: $storageAccountName"
```

**Bash (Linux/Mac):**
```bash
# Create resource group for Terraform state
az group create --name terraform-state-rg --location "West Europe"

# Create storage account (name must be globally unique)
az storage account create \
  --resource-group terraform-state-rg \
  --name tfstate$(openssl rand -hex 4) \
  --sku Standard_LRS \
  --location "West Europe"

# Create container
az storage container create \
  --name tfstate \
  --account-name <your-storage-account-name>
```

Create or edit `backend.tfvars` with your storage account details:

**PowerShell:**
```powershell
cd terraform
# Edit backend.tfvars with your storage account details
notepad backend.tfvars
```

**Bash:**
```bash
cd terraform
# Edit backend.tfvars with your storage account details
nano backend.tfvars
```

The `backend.tfvars` file should contain:
```hcl
resource_group_name  = "terraform-state-rg"
storage_account_name = "your-storage-account-name"
container_name       = "tfstate"
key                  = "myleague-app.terraform.tfstate"
```

Initialize Terraform with backend:

**PowerShell:**
```powershell
terraform init -backend-config=backend.tfvars
```

**Bash:**
```bash
terraform init -backend-config=backend.tfvars
```

### 2. Configure Variables

Copy the example variables file and customize:

```bash
# Copy terraform.tfvars.dev or terraform.tfvars.prod as a starting point
cp terraform.tfvars.dev terraform.tfvars
# Edit terraform.tfvars with your preferences
```

### 3. Build and Push Docker Images

#### Build Backend Image

```bash
# Navigate to project root
cd ..

# Build backend image
docker build -t webapi:latest -f src/backend/WebAPI/Dockerfile .

# Tag for ACR (replace with your ACR name after deployment)
docker tag webapi:latest <acr-name>.azurecr.io/webapi:latest
```

#### Build Frontend Image

**Important:** The frontend uses Vite, which means `VITE_API_URL` must be set at build time. You have two options:

**Option 1: Build with API URL (Recommended)**

First, deploy the infrastructure to get the backend URL, then build the frontend:

```bash
# After deployment, get the backend URL
cd terraform
terraform output backend_url

# Build frontend with the backend URL
cd ..
docker build \
  --build-arg VITE_API_URL=https://<backend-url-from-output> \
  -t frontend:latest \
  -f src/frontend/Dockerfile.prod \
  src/frontend
docker tag frontend:latest <acr-name>.azurecr.io/frontend:latest
```

**Option 2: Use PowerShell deployment scripts (Recommended)**

For better flexibility and separation of concerns, use the PowerShell scripts:

```powershell
# Deploy just the backend
.\terraform\deploy-backend.ps1 -AcrName <acr-name> -Tag latest

# Deploy just the frontend
.\terraform\deploy-frontend.ps1 -AcrName <acr-name> -Tag latest -ApiUrl https://<backend-url>

# Deploy both backend and frontend
.\terraform\deploy-all.ps1 -AcrName <acr-name> -Tag latest -ApiUrl https://<backend-url>

# With Azure Container Apps deployment
.\terraform\deploy-all.ps1 `
    -AcrName <acr-name> `
    -Tag latest `
    -ResourceGroup myleague-rg `
    -BackendAppName myleague-backend `
    -FrontendAppName myleague-frontend `
    -ApiUrl https://<backend-url> `
    -Deploy
```

**Benefits of PowerShell scripts:**
- Separate backend/frontend deployment for easier rollbacks
- Optional Azure Container Apps deployment
- Better error handling and colored output
- Support for ACR cloud builds (frontend)

**Note:** If you don't provide the backend URL, the frontend will default to `/api` which assumes the backend is on the same domain with a reverse proxy.

### 4. Deploy Infrastructure

```bash
cd terraform

# Review the deployment plan
terraform plan

# Apply the configuration
terraform apply
```

**Note:** The first deployment will create the ACR. After deployment, you'll need to:
1. Get the ACR login credentials from Terraform outputs
2. Login to ACR: `az acr login --name <acr-name>`
3. Push your images (see step 5)

### 5. Build and Push Docker Images to ACR

After the infrastructure is deployed:

```powershell
# Get the backend URL from Terraform outputs
cd terraform
$ACR_NAME = terraform output -raw container_registry_name
$BACKEND_URL = terraform output -raw backend_url

# Use PowerShell scripts (recommended)
cd ..
.\terraform\deploy-all.ps1 -AcrName $ACR_NAME -Tag latest -ApiUrl $BACKEND_URL

# OR use individual scripts
.\terraform\deploy-backend.ps1 -AcrName $ACR_NAME -Tag latest
.\terraform\deploy-frontend.ps1 -AcrName $ACR_NAME -Tag latest -ApiUrl $BACKEND_URL
```

**Alternative: Manual Docker commands**

```bash
# Get the backend URL from Terraform outputs
cd terraform
ACR_NAME=$(terraform output -raw container_registry_name | tr -d '"')
BACKEND_URL=$(terraform output -raw backend_url | tr -d '"')

# Manual build and push:
# Login to ACR
az acr login --name $ACR_NAME

# Build and push backend
docker build -t webapi:latest -f src/backend/WebAPI/Dockerfile .
docker tag webapi:latest $ACR_NAME.azurecr.io/webapi:latest
docker push $ACR_NAME.azurecr.io/webapi:latest

# Build and push frontend (with API URL)
docker build \
  --build-arg VITE_API_URL=$BACKEND_URL \
  -t frontend:latest \
  -f src/frontend/Dockerfile.prod \
  src/frontend
docker tag frontend:latest $ACR_NAME.azurecr.io/frontend:latest
docker push $ACR_NAME.azurecr.io/frontend:latest
```

### 6. Update Container Apps

After pushing images, the Container Apps will automatically pull the new images. If they don't, you can restart them:

```bash
# Restart backend
az containerapp revision restart \
  --name myleague-backend \
  --resource-group myleague-rg

# Restart frontend
az containerapp revision restart \
  --name myleague-frontend \
  --resource-group myleague-rg
```

## Important Configuration Steps

### 1. Update Frontend API URL (If Needed)

**Important:** Since the frontend uses Vite, the `VITE_API_URL` is embedded at build time. If you need to change it:

1. **Rebuild the frontend image** with the new API URL:
   ```bash
   BACKEND_URL=$(cd terraform && terraform output -raw backend_url)
   ACR_NAME=$(cd terraform && terraform output -raw container_registry_name)
   docker build --build-arg VITE_API_URL=$BACKEND_URL -t frontend:latest -f src/frontend/Dockerfile.prod src/frontend
   docker tag frontend:latest $ACR_NAME.azurecr.io/frontend:latest
   docker push $ACR_NAME.azurecr.io/frontend:latest
   ```

2. **Restart the frontend container app** to pull the new image:
   ```bash
   az containerapp revision restart \
     --name myleague-frontend \
     --resource-group myleague-rg
   ```

### 2. Configure CORS in Backend

Make sure your backend CORS configuration allows requests from the frontend URL. Update your backend configuration or add it via Azure CLI:

```bash
az containerapp update \
  --name myleague-backend \
  --resource-group myleague-rg \
  --set-env-vars "CORS__AllowedOrigins=https://<frontend-url>"
```

### 3. Database Migrations

After deployment, run Entity Framework migrations:

```bash
# Option 1: Use Azure Container Instances
az container create \
  --resource-group myleague-rg \
  --name migration-runner \
  --image <acr-name>.azurecr.io/webapi:latest \
  --registry-login-server <acr-name>.azurecr.io \
  --registry-username <acr-username> \
  --registry-password <acr-password> \
  --command-line "dotnet ef database update" \
  --environment-variables \
    ConnectionStrings__DefaultConnection="<connection-string>" \
    ASPNETCORE_ENVIRONMENT=Production

# Option 2: Connect to the container app and run migrations
az containerapp exec \
  --name myleague-backend \
  --resource-group myleague-rg \
  --command "dotnet ef database update"
```

## Post-Deployment

### Get Application URLs

```bash
terraform output backend_url
terraform output frontend_url
```

### Access Application Insights

```bash
# Get the connection string
terraform output application_insights_connection_string

# Or access via Azure Portal
az monitor app-insights component show \
  --app myleague-insights-XXXXXX \
  --resource-group myleague-rg
```

### Database Connection Details

```bash
# Get connection string (sensitive)
terraform output connection_string

# Get individual components
terraform output postgres_server_fqdn
terraform output postgres_admin_user
terraform output postgres_password
```

### Accessing the Private PostgreSQL Database

The PostgreSQL database is deployed with private access (VNet-integrated) for security. This means it's **not accessible from the public internet**.

#### Option 1: Enable Jump Box VM (Recommended for Development)

Add to `terraform.tfvars`:
```hcl
enable_jumpbox = true
```

Then apply:
```bash
terraform apply
```

Get connection details:
```bash
# Get SSH command
terraform output jumpbox_ssh_command

# Get PostgreSQL connection command
terraform output jumpbox_postgres_command
```

**📘 Full Guide:** See [JUMPBOX-GUIDE.md](./JUMPBOX-GUIDE.md) for detailed instructions on:
- Setting up and connecting to the jump box
- Using password or SSH key authentication
- Port forwarding for DBeaver/GUI tools
- Cost optimization (stop VM when not in use)
- Security best practices

#### Option 2: Azure Cloud Shell

```bash
# Open Azure Cloud Shell (https://shell.azure.com)
psql "host=<your-postgres-fqdn> port=5432 dbname=myleague user=myleague_admin sslmode=require"
```

#### Option 3: Other Methods

See [ACCESS-DB-DBEAVER.md](./ACCESS-DB-DBEAVER.md) for database access methods:
- Port forwarding via Azure Container Instance
- Temporary public access (not recommended)
- Azure Bastion with VM

## Updating the Application

### Update Docker Images

1. Build new images locally
2. Tag with new version or `latest`
3. Push to ACR
4. Container Apps will automatically pull new images if using `latest`, or update the tag in Terraform

### Update Infrastructure

```bash
cd terraform
terraform plan
terraform apply
```

## Troubleshooting

### Container App Not Starting

1. Check logs:
```bash
az containerapp logs show \
  --name myleague-backend \
  --resource-group myleague-rg \
  --follow
```

2. Check Application Insights in Azure Portal

### Database Connection Issues

1. Verify firewall rules allow Container Apps subnet
2. Check connection string format
3. Ensure SSL is enabled (already configured)

### Frontend Can't Connect to Backend

1. Verify `VITE_API_URL` environment variable
2. Check CORS configuration
3. Verify backend is accessible from frontend URL

## Cost Optimization

- **PostgreSQL**: Start with `B_Standard_B1ms` (Burstable Basic) for development
- **Container Apps**: Set min replicas to 0 for non-production environments
- **ACR**: Use Basic SKU for development (already configured)
- **Application Insights**: Set retention to 30 days (already configured)

## Documentation Index

Complete guides for deploying and managing your Azure infrastructure:

### 📚 Getting Started
- **[DEPLOYMENT-GUIDE.md](./DEPLOYMENT-GUIDE.md)** - Complete step-by-step deployment guide for both dev and prod environments (start here!)
- **[DEV-VS-PROD-DEPLOYMENT.md](./DEV-VS-PROD-DEPLOYMENT.md)** - Comparison between development and production configurations
- **[README-DEV-QUICK-START.md](./README-DEV-QUICK-START.md)** - Quick start guide for development deployment

### 🔧 Database Access
- **[ACCESS-DB-DBEAVER.md](./ACCESS-DB-DBEAVER.md)** - Guide for accessing PostgreSQL database with DBeaver (dev and prod)
- **[JUMPBOX-GUIDE.md](./JUMPBOX-GUIDE.md)** - Detailed guide for using the jump box VM
- **[QUICK-START-JUMPBOX.md](./QUICK-START-JUMPBOX.md)** - Quick reference for jump box usage

### 📊 Monitoring & Logging
- **[APPLICATION-INSIGHTS-QUERIES.md](./APPLICATION-INSIGHTS-QUERIES.md)** - KQL queries for Application Insights logs

### 🚀 Deployment Scripts
- **[DEPLOYMENT-SCRIPTS.md](./DEPLOYMENT-SCRIPTS.md)** - Documentation for PowerShell deployment scripts
- **[DEPLOY-DEV-CHECKLIST.md](./DEPLOY-DEV-CHECKLIST.md)** - Step-by-step checklist for development deployment

### 🔒 Security & Networking
- **[VNET-INTEGRATION-SETUP.md](./VNET-INTEGRATION-SETUP.md)** - Manual VNet integration setup (if needed)
- **[RECREATE-ENV-WITH-VNET.md](./RECREATE-ENV-WITH-VNET.md)** - Guide for recreating Container Apps Environment with VNet

## Clean Up

To destroy all resources:

```bash
cd terraform
terraform destroy
```

**Warning:** This will delete all resources including the database!

## Additional Resources

- [Azure Container Apps Documentation](https://docs.microsoft.com/en-us/azure/container-apps/)
- [Azure Database for PostgreSQL](https://docs.microsoft.com/en-us/azure/postgresql/)
- [Terraform Azure Provider](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs)

