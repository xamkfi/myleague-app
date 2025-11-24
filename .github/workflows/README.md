# GitHub Workflows - CI/CD Setup

This directory contains GitHub Actions workflows for continuous integration and deployment.

## Workflows Overview

### CI Workflows (Continuous Integration)
- **`backend-ci.yaml`** - Builds, tests, and validates backend (.NET)
- **`frontend-ci.yaml`** - Builds, lints, and validates frontend (React)

### CD Workflows (Continuous Deployment)
- **`backend-cd.yml`** - Deploys backend to Azure App Service
- **`frontend-cd.yml`** - Deploys frontend to Azure Static Web Apps

## Required GitHub Secrets

To enable automatic deployment, you need to configure the following secrets in your GitHub repository:

### 1. Backend Deployment Secret

**`AZURE_WEBAPP_PUBLISH_PROFILE`**
- **What**: Publish profile for Azure App Service
- **How to get it**:
  ```bash
  az webapp deployment list-publishing-profiles \
    --name app-myleague-bicep-dev \
    --resource-group rg-myleague-bicep-dev \
    --xml
  ```
  Or download from Azure Portal:
  1. Go to your App Service → `app-myleague-bicep-dev`
  2. Click **Get publish profile** in the toolbar
  3. Open the downloaded `.PublishSettings` file
  4. Copy the entire XML content

### 2. Frontend Deployment Secret

**`AZURE_STATIC_WEB_APPS_API_TOKEN`**
- **What**: Deployment token for Azure Static Web Apps
- **How to get it**:
  ```bash
  az staticwebapp secrets list \
    --name swa-myleague-bicep-dev \
    --resource-group rg-myleague-bicep-dev \
    --query properties.apiKey \
    --output tsv
  ```
  Or get from Azure Portal:
  1. Go to your Static Web App → `swa-myleague-bicep-dev`
  2. Go to **Manage deployment token** under Deployment
  3. Copy the token

## Setting Up GitHub Secrets

1. Go to your GitHub repository
2. Navigate to **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret**
4. Add each secret:
   - Name: `AZURE_WEBAPP_PUBLISH_PROFILE`
   - Value: Paste the publish profile XML
   - Click **Add secret**
   
   Repeat for `AZURE_STATIC_WEB_APPS_API_TOKEN`

## Workflow Triggers

### Backend CD
- Triggers on push to `main` or `master` branches
- Only when files in `src/backend/**` are changed
- Can be manually triggered via workflow_dispatch

### Frontend CD
- Triggers on push to `main` or `master` branches
- Only when files in `src/frontend/**` are changed
- Can be manually triggered via workflow_dispatch
- Creates preview deployments for pull requests

## Manual Deployment

You can manually trigger deployments:

1. Go to **Actions** tab in GitHub
2. Select the workflow (`Backend CD` or `Frontend CD`)
3. Click **Run workflow**
4. Select branch and click **Run workflow**

## Environment Configuration

### Backend Environment Variables
Set in Azure App Service Configuration:
- `POSTGRESQLCONNSTR_DefaultConnection` - Database connection string
- `ASPNETCORE_ENVIRONMENT` - Environment (Development/Production)

### Frontend Environment Variables
Set in workflow or Azure Static Web Apps Configuration:
- `VITE_API_URL` - Backend API URL (set in workflow)

## Deployment Environments

Both workflows use GitHub Environments named `Production`. You can configure:
- Required reviewers
- Deployment protection rules
- Environment secrets

Go to **Settings** → **Environments** → **Production** to configure.

## Monitoring Deployments

### Backend
- Health check endpoint: `https://app-myleague-bicep-dev.azurewebsites.net/health`
- API documentation: `https://app-myleague-bicep-dev.azurewebsites.net/scalar/v1`

### Frontend
- Production URL: `https://green-desert-0956c3a03.3.azurestaticapps.net`
- Preview URLs created for PRs automatically

## Troubleshooting

### Backend deployment fails
1. Check publish profile is valid
2. Verify App Service is running
3. Check App Service logs in Azure Portal

### Frontend deployment fails
1. Verify Static Web Apps API token is valid
2. Check build succeeds locally
3. Ensure `VITE_API_URL` is set correctly

### CORS issues after deployment
1. Ensure Azure App Service CORS is cleared (let app handle it)
2. Verify backend CORS middleware is configured
3. Check both deployments completed successfully

## Next Steps

1. ✅ Configure GitHub secrets
2. ✅ Push to main/master branch
3. ✅ Monitor workflow execution in Actions tab
4. ✅ Verify deployments in Azure Portal
5. ✅ Test the deployed applications

