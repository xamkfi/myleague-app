# Versioning and Deployment Guide

This guide explains how to manage versions and deploy new releases of your MyLeague application using the deployment scripts.

## Table of Contents

1. [Versioning Strategies](#versioning-strategies)
2. [Deploying a New Version](#deploying-a-new-version)
3. [Version Tag Examples](#version-tag-examples)
4. [Deployment Workflows](#deployment-workflows)
5. [Rolling Back to a Previous Version](#rolling-back-to-a-previous-version)
6. [Best Practices](#best-practices)
7. [Troubleshooting](#troubleshooting)

---

## Versioning Strategies

### Recommended: Semantic Versioning

Use semantic versioning format: `MAJOR.MINOR.PATCH`

- **MAJOR**: Breaking changes (e.g., `2.0.0`)
- **MINOR**: New features, backward compatible (e.g., `1.2.0`)
- **PATCH**: Bug fixes, backward compatible (e.g., `1.1.1`)

**Examples:**
- `1.0.0` - Initial release
- `1.0.1` - Bug fix
- `1.1.0` - New feature
- `2.0.0` - Major update with breaking changes

### Alternative: Date-Based Versioning

Use date format: `YYYY-MM-DD` or `YYYYMMDD-HHMM`

**Examples:**
- `2025-01-15` - Release on January 15, 2025
- `20250115-1430` - Release on January 15, 2025 at 14:30

### Alternative: Git Commit-Based

Use short commit hash: `git-<hash>`

**Examples:**
- `git-a1b2c3d` - Based on commit hash

---

## Deploying a New Version

### Step 1: Get Your ACR Name and Backend URL

First, get the required information from Terraform outputs:

```powershell
cd terraform
$ACR_NAME = terraform output -raw container_registry_name
$BACKEND_URL = terraform output -raw backend_url
$API_URL = "$BACKEND_URL/api"
cd ..
```

### Step 2: Choose Your Version Tag

Decide on a version tag. For example:
- `1.2.3` (semantic versioning)
- `2025-01-15` (date-based)
- `v1.2.3` (with 'v' prefix)

### Step 3: Deploy Backend

```powershell
.\scripts\deploy-backend.ps1 `
    -AcrName $ACR_NAME `
    -Tag "1.2.3" `
    -ResourceGroup "myleague-dev-rg" `
    -ContainerAppName "myleague-dev-backend" `
    -Deploy
```

**What this does:**
1. Builds Docker image with tag `1.2.3`
2. Tags it as `$ACR_NAME.azurecr.io/webapi:1.2.3`
3. Pushes to Azure Container Registry
4. Deploys to Azure Container Apps (if `-Deploy` is used)

### Step 4: Deploy Frontend

```powershell
.\scripts\deploy-frontend.ps1 `
    -AcrName $ACR_NAME `
    -Tag "1.2.3" `
    -ApiUrl $API_URL `
    -ResourceGroup "myleague-dev-rg" `
    -ContainerAppName "myleague-dev-frontend" `
    -Deploy
```

**What this does:**
1. Builds Docker image with tag `1.2.3` and API URL
2. Tags it as `$ACR_NAME.azurecr.io/frontend:1.2.3`
3. Pushes to Azure Container Registry
4. Deploys to Azure Container Apps (if `-Deploy` is used)

### Step 5: Deploy Both (Alternative)

Instead of deploying separately, you can deploy both at once:

```powershell
.\scripts\deploy-all.ps1 `
    -AcrName $ACR_NAME `
    -Tag "1.2.3" `
    -ApiUrl $API_URL `
    -BackendAppName "myleague-dev-backend" `
    -FrontendAppName "myleague-dev-frontend" `
    -ResourceGroup "myleague-dev-rg" `
    -Deploy
```

---

## Version Tag Examples

### Example 1: Semantic Versioning

```powershell
# Deploy version 1.2.3
$VERSION = "1.2.3"
.\scripts\deploy-all.ps1 `
    -AcrName $ACR_NAME `
    -Tag $VERSION `
    -ApiUrl $API_URL `
    -BackendAppName "myleague-dev-backend" `
    -FrontendAppName "myleague-dev-frontend" `
    -ResourceGroup "myleague-dev-rg" `
    -Deploy
```

### Example 2: Date-Based Versioning

```powershell
# Deploy with today's date
$VERSION = "2025-01-15"
.\scripts\deploy-all.ps1 `
    -AcrName $ACR_NAME `
    -Tag $VERSION `
    -ApiUrl $API_URL `
    -BackendAppName "myleague-dev-backend" `
    -FrontendAppName "myleague-dev-frontend" `
    -ResourceGroup "myleague-dev-rg" `
    -Deploy
```

### Example 3: Build Without Deploying

Sometimes you want to build and push the image but not deploy it yet:

```powershell
# Build and push only (no deployment)
.\scripts\deploy-backend.ps1 `
    -AcrName $ACR_NAME `
    -Tag "1.2.3"
    # Note: No -Deploy flag, so it won't update Container Apps
```

### Example 4: Deploy Specific Version Later

If you built an image earlier, you can deploy it later:

```powershell
# Deploy an existing image version
az containerapp update `
    --name myleague-dev-backend `
    --resource-group myleague-dev-rg `
    --image "$ACR_NAME.azurecr.io/webapi:1.2.3"
```

---

## Deployment Workflows

### Workflow 1: Development Deployment

For development/testing environment:

```powershell
# Set variables
cd terraform
$ACR_NAME = terraform output -raw container_registry_name
$BACKEND_URL = terraform output -raw backend_url
$API_URL = "$BACKEND_URL/api"
cd ..

# Deploy with version tag
$VERSION = "dev-$(Get-Date -Format 'yyyyMMdd-HHmm')"
.\scripts\deploy-all.ps1 `
    -AcrName $ACR_NAME `
    -Tag $VERSION `
    -ApiUrl $API_URL `
    -BackendAppName "myleague-dev-backend" `
    -FrontendAppName "myleague-dev-frontend" `
    -ResourceGroup "myleague-dev-rg" `
    -Deploy
```

### Workflow 2: Production Deployment

For production environment:

```powershell
# Set variables
cd terraform
$ACR_NAME = terraform output -raw container_registry_name
$BACKEND_URL = terraform output -raw backend_url
$API_URL = "$BACKEND_URL/api"
cd ..

# Deploy with semantic version
$VERSION = "1.2.3"
.\scripts\deploy-all.ps1 `
    -AcrName $ACR_NAME `
    -Tag $VERSION `
    -ApiUrl $API_URL `
    -BackendAppName "myleague-prod-backend" `
    -FrontendAppName "myleague-prod-frontend" `
    -ResourceGroup "myleague-prod-rg" `
    -Deploy
```

### Workflow 3: Hotfix Deployment

For urgent bug fixes:

```powershell
# Quick hotfix deployment
$VERSION = "1.2.4"  # Patch version bump
.\scripts\deploy-backend.ps1 `
    -AcrName $ACR_NAME `
    -Tag $VERSION `
    -ResourceGroup "myleague-prod-rg" `
    -ContainerAppName "myleague-prod-backend" `
    -Deploy
```

### Workflow 4: Staged Rollout

Deploy to staging first, then production:

```powershell
# Step 1: Deploy to staging
$VERSION = "1.2.3"
.\scripts\deploy-all.ps1 `
    -AcrName $ACR_NAME `
    -Tag $VERSION `
    -ApiUrl $STAGING_API_URL `
    -BackendAppName "myleague-staging-backend" `
    -FrontendAppName "myleague-staging-frontend" `
    -ResourceGroup "myleague-staging-rg" `
    -Deploy

# Step 2: Test in staging
# ... perform testing ...

# Step 3: Deploy to production (same version)
.\scripts\deploy-all.ps1 `
    -AcrName $ACR_NAME `
    -Tag $VERSION `
    -ApiUrl $PROD_API_URL `
    -BackendAppName "myleague-prod-backend" `
    -FrontendAppName "myleague-prod-frontend" `
    -ResourceGroup "myleague-prod-rg" `
    -Deploy
```

---

## Rolling Back to a Previous Version

### Method 1: Deploy Previous Version

If you know the previous version tag:

```powershell
# Rollback backend to version 1.2.2
az containerapp update `
    --name myleague-dev-backend `
    --resource-group myleague-dev-rg `
    --image "$ACR_NAME.azurecr.io/webapi:1.2.2"

# Rollback frontend to version 1.2.2
az containerapp update `
    --name myleague-dev-frontend `
    --resource-group myleague-dev-rg `
    --image "$ACR_NAME.azurecr.io/frontend:1.2.2"
```

### Method 2: List Available Versions

First, see what versions are available in ACR:

```powershell
# List backend image tags
az acr repository show-tags `
    --name $ACR_NAME `
    --repository webapi `
    --orderby time_desc `
    --output table

# List frontend image tags
az acr repository show-tags `
    --name $ACR_NAME `
    --repository frontend `
    --orderby time_desc `
    --output table
```

### Method 3: Check Current Version

See what version is currently deployed:

```powershell
# Check backend version
az containerapp show `
    --name myleague-dev-backend `
    --resource-group myleague-dev-rg `
    --query "properties.template.containers[0].image" `
    --output tsv

# Check frontend version
az containerapp show `
    --name myleague-dev-frontend `
    --resource-group myleague-dev-rg `
    --query "properties.template.containers[0].image" `
    --output tsv
```

### Method 4: Rollback Script

Create a quick rollback script:

```powershell
# Rollback to previous version
$PREVIOUS_VERSION = "1.2.2"
$ACR_NAME = "myleagueacrt2dns6"
$RG = "myleague-dev-rg"

# Rollback backend
az containerapp update `
    --name myleague-dev-backend `
    --resource-group $RG `
    --image "$ACR_NAME.azurecr.io/webapi:$PREVIOUS_VERSION"

# Rollback frontend
az containerapp update `
    --name myleague-dev-frontend `
    --resource-group $RG `
    --image "$ACR_NAME.azurecr.io/frontend:$PREVIOUS_VERSION"
```

---

## Best Practices

### 1. Always Tag Your Versions

**❌ Bad:**
```powershell
# Always using 'latest' makes rollbacks impossible
.\scripts\deploy-backend.ps1 -AcrName $ACR_NAME -Tag "latest" -Deploy
```

**✅ Good:**
```powershell
# Use specific version tags
.\scripts\deploy-backend.ps1 -AcrName $ACR_NAME -Tag "1.2.3" -Deploy
```

### 2. Keep 'latest' for Development Only

Use `latest` tag only for development/testing:

```powershell
# Development: OK to use 'latest'
.\scripts\deploy-backend.ps1 -AcrName $ACR_NAME -Tag "latest" -Deploy

# Production: Always use specific version
.\scripts\deploy-backend.ps1 -AcrName $ACR_NAME -Tag "1.2.3" -Deploy
```

### 3. Document Your Versions

Keep a changelog or version history:

```
Version 1.2.3 (2025-01-15)
- Added new feature X
- Fixed bug Y
- Updated dependencies

Version 1.2.2 (2025-01-10)
- Fixed critical security issue
- Performance improvements
```

### 4. Test Before Production

Always test in development/staging before production:

```powershell
# 1. Deploy to dev
.\scripts\deploy-all.ps1 -AcrName $ACR_NAME -Tag "1.2.3" ... -Deploy

# 2. Test thoroughly

# 3. Deploy to production (same version)
.\scripts\deploy-all.ps1 -AcrName $ACR_NAME -Tag "1.2.3" ... -Deploy
```

### 5. Build Once, Deploy Many

Build the image once, deploy to multiple environments:

```powershell
# Build once
.\scripts\deploy-backend.ps1 -AcrName $ACR_NAME -Tag "1.2.3"
# (No -Deploy flag, just builds and pushes)

# Deploy to dev
az containerapp update --name myleague-dev-backend ... --image "$ACR_NAME.azurecr.io/webapi:1.2.3"

# Deploy to staging
az containerapp update --name myleague-staging-backend ... --image "$ACR_NAME.azurecr.io/webapi:1.2.3"

# Deploy to production
az containerapp update --name myleague-prod-backend ... --image "$ACR_NAME.azurecr.io/webapi:1.2.3"
```

### 6. Use Version Variables

Make version management easier:

```powershell
# Set version once
$VERSION = "1.2.3"
$ACR_NAME = "myleagueacrt2dns6"
$RG = "myleague-dev-rg"

# Use in all commands
.\scripts\deploy-all.ps1 `
    -AcrName $ACR_NAME `
    -Tag $VERSION `
    -ApiUrl $API_URL `
    -BackendAppName "myleague-dev-backend" `
    -FrontendAppName "myleague-dev-frontend" `
    -ResourceGroup $RG `
    -Deploy
```

---

## Troubleshooting

### Problem: "Image not found" when deploying

**Solution:** Make sure the image was built and pushed first:

```powershell
# Check if image exists
az acr repository show-tags `
    --name $ACR_NAME `
    --repository webapi `
    --output table

# If not found, build it first (without -Deploy)
.\scripts\deploy-backend.ps1 -AcrName $ACR_NAME -Tag "1.2.3"
```

### Problem: "Tag already exists" warning

**Solution:** This is normal if you're rebuilding the same version. The new image will overwrite the old one. If you want to keep old versions, use unique tags.

### Problem: Deployment succeeds but app doesn't update

**Solution:** Check if the Container App is using the correct image:

```powershell
# Check current image
az containerapp show `
    --name myleague-dev-backend `
    --resource-group myleague-dev-rg `
    --query "properties.template.containers[0].image" `
    --output tsv

# Force a new revision
az containerapp update `
    --name myleague-dev-backend `
    --resource-group myleague-dev-rg `
    --image "$ACR_NAME.azurecr.io/webapi:1.2.3" `
    --revision-suffix "v123"
```

### Problem: Frontend can't connect to backend

**Solution:** Make sure you're using the correct API URL:

```powershell
# Get the correct backend URL
cd terraform
$BACKEND_URL = terraform output -raw backend_url
$API_URL = "$BACKEND_URL/api"
cd ..

# Deploy frontend with correct API URL
.\scripts\deploy-frontend.ps1 `
    -AcrName $ACR_NAME `
    -Tag "1.2.3" `
    -ApiUrl $API_URL `
    -Deploy
```

---

## Quick Reference

### Common Commands

```powershell
# Get ACR name
cd terraform
$ACR_NAME = terraform output -raw container_registry_name
cd ..

# Deploy new version
$VERSION = "1.2.3"
.\scripts\deploy-all.ps1 `
    -AcrName $ACR_NAME `
    -Tag $VERSION `
    -ApiUrl $API_URL `
    -BackendAppName "myleague-dev-backend" `
    -FrontendAppName "myleague-dev-frontend" `
    -ResourceGroup "myleague-dev-rg" `
    -Deploy

# List available versions
az acr repository show-tags --name $ACR_NAME --repository webapi --output table

# Check current version
az containerapp show --name myleague-dev-backend --resource-group myleague-dev-rg --query "properties.template.containers[0].image" --output tsv

# Rollback to previous version
az containerapp update --name myleague-dev-backend --resource-group myleague-dev-rg --image "$ACR_NAME.azurecr.io/webapi:1.2.2"
```

---

## Additional Resources

- [Deployment Scripts Documentation](../terraform/DEPLOYMENT-SCRIPTS.md)
- [Main Deployment Guide](../terraform/DEPLOYMENT-GUIDE.md)
- [Azure Container Apps Documentation](https://learn.microsoft.com/en-us/azure/container-apps/)

---

**Last Updated:** 2025-01-15

