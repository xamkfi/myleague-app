# Complete Azure Deployment Guide

This guide walks you through deploying the MyLeague application to Azure from scratch, covering both **Development** and **Production** environments.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Initial Setup](#initial-setup)
3. [Development Deployment](#development-deployment)
4. [Production Deployment](#production-deployment)
5. [Post-Deployment Steps](#post-deployment-steps)
6. [Troubleshooting](#troubleshooting)

---

## Prerequisites

### 1. Required Software

Install the following on your local machine:

- **Azure CLI** (v2.50+)
  - Download: https://aka.ms/installazurecliwindows
  - Verify: `az --version`

- **Terraform** (v1.0+)
  - Download: https://www.terraform.io/downloads
  - Verify: `terraform version`

- **Docker Desktop**
  - Download: https://www.docker.com/products/docker-desktop
  - Verify: `docker --version` and `docker ps`

- **PowerShell** (Windows) or **Bash** (Linux/Mac)
  - Windows: Already installed
  - Linux/Mac: Use your system's package manager

### 2. Azure Account Setup

1. **Login to Azure:**
   ```powershell
   az login
   ```

2. **Set your subscription:**
   ```powershell
   # List available subscriptions
   az account list --output table
   
   # Set active subscription
   az account set --subscription "Your Subscription Name or ID"
   
   # Verify
   az account show
   ```

3. **Verify permissions:**
   - You need **Contributor** or **Owner** role on the subscription
   - Check: `az role assignment list --assignee $(az account show --query user.name -o tsv) --scope /subscriptions/$(az account show --query id -o tsv)`

### 3. Get Your Public IP Address

For development deployment, you'll need your public IP:

```powershell
# Windows PowerShell
(Invoke-WebRequest -Uri "https://ifconfig.me" -UseBasicParsing).Content

# Or use curl
curl https://ifconfig.me
```

**Save this IP** - you'll need it for the development configuration.

---

## Initial Setup

### 1. Clone/Navigate to Project

```powershell
cd C:\Users\hture01\Downloads\myleague-app-development\myleague-app-development
cd terraform
```

### 2. Understand the Structure

```
terraform/
├── main.tf                    # Main infrastructure definition
├── variables.tf                # Variable definitions
├── outputs.tf                 # Output values
├── terraform.tfvars.dev       # Development configuration
├── terraform.tfvars.prod      # Production configuration
├── deploy-dev.ps1             # Automated dev deployment script
└── DEPLOYMENT-GUIDE.md        # This file

scripts/
├── deploy-backend.ps1         # Backend image build/deploy script
├── deploy-frontend.ps1       # Frontend image build/deploy script
└── deploy-all.ps1            # Deploy both backend and frontend
```

---

## Development Deployment

Development deployment is **simpler, cheaper (~€30-40/month)**, and allows direct database access from your local machine.

### Step 1: Configure Development Environment

1. **Get your public IP address:**
   ```powershell
   $myIp = (Invoke-WebRequest -Uri "https://ifconfig.me" -UseBasicParsing).Content
   Write-Host "Your IP: $myIp"
   ```

2. **Edit `terraform.tfvars.dev`:**
   ```powershell
   # Open in your editor
   notepad terraform.tfvars.dev
   ```

3. **Update the IP address:**
   ```hcl
   allowed_ip_addresses = ["YOUR_IP_HERE"]  # Replace with your IP
   ```

### Step 2: Initialize Terraform (First Time Only)

```powershell
# Initialize Terraform
terraform init

# Create development workspace
terraform workspace new development
terraform workspace select development
```

### Step 3: Deploy Infrastructure

**Option A: Automated Script (Recommended)**
```powershell
# This script automatically detects your IP and deploys
.\deploy-dev.ps1
```

**Option B: Manual Deployment**
```powershell
# Review what will be created
terraform plan -var-file="terraform.tfvars.dev"

# Deploy infrastructure
terraform apply -var-file="terraform.tfvars.dev"
# Type 'yes' when prompted
```

**Expected time:** 10-15 minutes

### Step 4: Build and Push Docker Images

After infrastructure is created, build and push your application images:

#### 4a. Get Container Registry Name

```powershell
$acrName = terraform output -raw container_registry_name
Write-Host "ACR Name: $acrName"
```

#### 4b. Build and Push Backend

```powershell
cd ..
.\scripts\deploy-backend.ps1 -AcrName $acrName
```

#### 4c. Build and Push Frontend

```powershell
# Get backend URL for frontend build
$backendUrl = terraform output -raw backend_url
$apiUrl = "$backendUrl/api"

# Build frontend with API URL embedded
.\scripts\deploy-frontend.ps1 -AcrName $acrName -ApiUrl $apiUrl
```

### Step 5: Deploy Container Apps

If Container Apps weren't created during Step 3 (because images didn't exist), deploy them now:

```powershell
cd terraform
terraform apply -var-file="terraform.tfvars.dev"
```

### Step 6: Verify Deployment

```powershell
# Get URLs
terraform output backend_url
terraform output frontend_url

# Check backend health
$backendUrl = terraform output -raw backend_url
Invoke-WebRequest -Uri "$backendUrl/health" -UseBasicParsing

# Check Container App status
az containerapp show --name myleague-dev-backend --resource-group myleague-dev-rg --query "properties.runningStatus" -o tsv
az containerapp show --name myleague-dev-frontend --resource-group myleague-dev-rg --query "properties.runningStatus" -o tsv
```

### Step 7: Access Your Application

- **Frontend:** `https://myleague-dev-frontend.delightfulfield-067db2fa.westeurope.azurecontainerapps.io`
- **Backend API:** `https://myleague-dev-backend.delightfulfield-067db2fa.westeurope.azurecontainerapps.io`
- **API Docs:** `https://myleague-dev-backend.delightfulfield-067db2fa.westeurope.azurecontainerapps.io/scalar/v1`

---

## Production Deployment

Production deployment is **secure, scalable (~€200-260/month)**, designed for 5000+ concurrent users.

### Step 1: Configure Production Environment

1. **Edit `terraform.tfvars.prod`:**
   ```powershell
   notepad terraform.tfvars.prod
   ```

2. **Review and adjust settings:**
   - PostgreSQL SKU (currently `GP_Standard_D2s_v3`)
   - Container App scaling (min/max replicas)
   - Resource sizes (CPU/memory)
   - Jump box configuration (if needed)

### Step 2: Initialize Terraform Workspace

```powershell
# Use default workspace for production
terraform workspace select default

# Or create a production workspace
terraform workspace new production
terraform workspace select production
```

### Step 3: Deploy Infrastructure

```powershell
# Review plan
terraform plan -var-file="terraform.tfvars.prod"

# Deploy
terraform apply -var-file="terraform.tfvars.prod"
# Type 'yes' when prompted
```

**Expected time:** 15-20 minutes

### Step 4: Build and Push Docker Images

Same as development (Step 4 above), but use production ACR:

```powershell
# Get production ACR name
cd terraform
$acrName = terraform output -raw container_registry_name

# Build backend
cd ..
.\scripts\deploy-backend.ps1 -AcrName $acrName

# Build frontend with production backend URL
$backendUrl = terraform output -raw backend_url
$apiUrl = "$backendUrl/api"
.\scripts\deploy-frontend.ps1 -AcrName $acrName -ApiUrl $apiUrl
```

### Step 5: Deploy Container Apps

```powershell
cd terraform
terraform apply -var-file="terraform.tfvars.prod"
```

### Step 6: Configure Jump Box (If Enabled)

If you enabled the jump box for database access:

```powershell
# Get jump box details
terraform output jumpbox_public_ip
terraform output jumpbox_ssh_command
terraform output jumpbox_admin_password

# Connect via SSH
ssh azureuser@<jumpbox-ip>
```

See [ACCESS-DB-DBEAVER.md](./ACCESS-DB-DBEAVER.md) for database access instructions.

---

## Post-Deployment Steps

### 1. Update Application Code

When you make code changes:

#### Backend Changes:
```powershell
# Rebuild and redeploy backend
.\scripts\deploy-backend.ps1 -AcrName <acr-name> -ResourceGroup <rg-name> -ContainerAppName <app-name> -Deploy
```

#### Frontend Changes:
```powershell
# Rebuild and redeploy frontend
$backendUrl = terraform output -raw backend_url
$apiUrl = "$backendUrl/api"
.\scripts\deploy-frontend.ps1 -AcrName <acr-name> -ApiUrl $apiUrl -ResourceGroup <rg-name> -ContainerAppName <app-name> -Deploy
```

### 2. View Logs

#### Container Apps Console Logs:
```powershell
# Backend logs
az containerapp logs show --name myleague-dev-backend --resource-group myleague-dev-rg --tail 100 --type console

# Frontend logs
az containerapp logs show --name myleague-dev-frontend --resource-group myleague-dev-rg --tail 100 --type console
```

#### Application Insights:
1. Go to Azure Portal
2. Navigate to Application Insights resource
3. Click "Logs"
4. Run KQL queries (see [APPLICATION-INSIGHTS-QUERIES.md](./APPLICATION-INSIGHTS-QUERIES.md))

### 3. Monitor Resources

```powershell
# Check Container App status
az containerapp list --resource-group myleague-dev-rg --query "[].{Name:name,Status:properties.runningStatus,Replicas:properties.template.scale.minReplicas}" -o table

# Check costs (requires Cost Management permissions)
az consumption usage list --start-date $(Get-Date -Format "yyyy-MM-01") --end-date $(Get-Date -Format "yyyy-MM-dd")
```

### 4. Scale Resources

```powershell
# Scale backend
az containerapp update --name myleague-dev-backend --resource-group myleague-dev-rg --min-replicas 2 --max-replicas 5

# Scale frontend
az containerapp update --name myleague-dev-frontend --resource-group myleague-dev-rg --min-replicas 1 --max-replicas 3
```

---

## Troubleshooting

### Common Issues

#### 1. Terraform State Locked

**Error:** `Error acquiring the state lock`

**Solution:**
```powershell
# Force unlock (use with caution!)
terraform force-unlock <LOCK_ID>
```

#### 2. Container App Failed to Start

**Check logs:**
```powershell
az containerapp logs show --name <app-name> --resource-group <rg-name> --tail 100 --type console
```

**Common causes:**
- Missing Docker image in ACR
- Incorrect environment variables
- Database connection issues

**Solution:**
- Verify image exists: `az acr repository show-tags --name <acr-name> --repository <image-name>`
- Check environment variables: `az containerapp show --name <app-name> --resource-group <rg-name> --query "properties.template.containers[0].env" -o table`
- Verify database connectivity from Container App

#### 3. CORS Errors

**Symptoms:** Frontend can't call backend API

**Solution:**
1. Check backend CORS configuration:
   ```powershell
   az containerapp show --name myleague-dev-backend --resource-group myleague-dev-rg --query "properties.template.containers[0].env[?name=='Cors__AllowedOrigins']" -o table
   ```

2. Rebuild frontend with correct backend URL:
   ```powershell
   $backendUrl = terraform output -raw backend_url
   $apiUrl = "$backendUrl/api"
   .\scripts\deploy-frontend.ps1 -AcrName <acr-name> -ApiUrl $apiUrl
   ```

3. Restart backend:
   ```powershell
   az containerapp update --name myleague-dev-backend --resource-group myleague-dev-rg --revision-suffix "cors-fix"
   ```

#### 4. Database Connection Failed

**Development (Public Access):**
- Verify your IP is in firewall rules
- Check connection string in Key Vault
- Test connection from local machine

**Production (Private Access):**
- Verify VNet integration is enabled
- Check Container App can reach database subnet
- Use jump box to test connection

#### 5. Image Not Found

**Error:** `MANIFEST_UNKNOWN: manifest tagged by "latest" is not found`

**Solution:**
```powershell
# Build and push the image first
.\scripts\deploy-backend.ps1 -AcrName <acr-name>
# Then deploy Container App
terraform apply -var-file="terraform.tfvars.dev"
```

### Getting Help

1. **Check logs:** Always start with Container App logs
2. **Check Application Insights:** For structured, queryable logs
3. **Verify Terraform state:** `terraform show`
4. **Check Azure Portal:** Visual inspection of resources

---

## Quick Reference

### Development Commands

```powershell
# Deploy everything
.\deploy-dev.ps1

# Rebuild backend
.\scripts\deploy-backend.ps1 -AcrName <acr-name> -ResourceGroup myleague-dev-rg -ContainerAppName myleague-dev-backend -Deploy

# Rebuild frontend
$backendUrl = terraform output -raw backend_url
.\scripts\deploy-frontend.ps1 -AcrName <acr-name> -ApiUrl "$backendUrl/api" -ResourceGroup myleague-dev-rg -ContainerAppName myleague-dev-frontend -Deploy

# View logs
az containerapp logs show --name myleague-dev-backend --resource-group myleague-dev-rg --follow --type console
```

### Production Commands

```powershell
# Deploy infrastructure
terraform apply -var-file="terraform.tfvars.prod"

# Rebuild and deploy
.\scripts\deploy-backend.ps1 -AcrName <acr-name> -ResourceGroup myleague-rg -ContainerAppName myleague-backend -Deploy
.\scripts\deploy-frontend.ps1 -AcrName <acr-name> -ApiUrl "<backend-url>/api" -ResourceGroup myleague-rg -ContainerAppName myleague-frontend -Deploy
```

### Useful Terraform Commands

```powershell
# Initialize
terraform init

# Plan changes
terraform plan -var-file="terraform.tfvars.dev"

# Apply changes
terraform apply -var-file="terraform.tfvars.dev"

# Destroy everything (use with caution!)
terraform destroy -var-file="terraform.tfvars.dev"

# Show current state
terraform show

# List workspaces
terraform workspace list

# Switch workspace
terraform workspace select <workspace-name>
```

---

## Cost Estimation

### Development Environment
- **PostgreSQL (B_Standard_B1ms):** ~€15-20/month
- **Container Apps (scale-to-zero):** ~€5-10/month
- **ACR:** ~€5/month
- **Key Vault:** ~€0.03/month
- **Application Insights:** ~€5-10/month
- **VNet/Networking:** ~€0-5/month
- **Total:** ~€30-40/month

### Production Environment
- **PostgreSQL (GP_Standard_D2s_v3):** ~€150-180/month
- **Container Apps (always-on):** ~€30-50/month
- **ACR:** ~€5/month
- **Key Vault:** ~€0.03/month
- **Application Insights:** ~€10-20/month
- **Jump Box VM:** ~€4-18/month
- **VNet/Networking:** ~€0-5/month
- **Total:** ~€200-260/month

**Note:** Actual costs vary based on usage, region, and Azure pricing changes.

---

## Next Steps

- [Access Database with DBeaver](./ACCESS-DB-DBEAVER.md)
- [View Application Insights Logs](./APPLICATION-INSIGHTS-QUERIES.md)
- [Development vs Production Comparison](./DEV-VS-PROD-DEPLOYMENT.md)

---

**Last Updated:** November 2025

