# Database Seeding Guide

This guide explains how to populate your deployed backend with initial test data.

## Overview

The seeder tool automatically populates your database with realistic test data including:
- Persons (players, referees)
- Clubs and divisions
- Floorball teams with player rosters
- Competition seasons

All seeding operations are **idempotent** - you can run the seeder multiple times without creating duplicates.

---

## Quick Start

### After Initial Deployment

Once you've deployed your infrastructure and backend, seed the database:

```powershell
# Get backend URL from Terraform
cd terraform
$backendUrl = terraform output -raw backend_url
cd ..

# Run the seeder
.\scripts\seed-data.ps1 -BackendUrl $backendUrl
```

Or provide the URL directly:
```powershell
.\scripts\seed-data.ps1 -BackendUrl "https://your-backend-url.azurecontainerapps.io"
```

The script will:
1. Build the seeder project (if needed)
2. Run the seeder against your backend
3. Populate your database with test data

---

## Usage

### Basic Usage

```powershell
# Get backend URL and seed
cd terraform
$backendUrl = terraform output -raw backend_url
cd ..
.\scripts\seed-data.ps1 -BackendUrl $backendUrl
```

### Direct URL

```powershell
# Provide backend URL directly
.\scripts\seed-data.ps1 -BackendUrl "https://myleague-dev-backend.azurecontainerapps.io"
```

---

## Complete Deployment + Seeding Workflow

### For Development Environment

```powershell
# 1. Deploy infrastructure
cd terraform
terraform apply -var-file="terraform.tfvars.dev"
cd ..

# 2. Deploy backend
$ACR_NAME = terraform output -raw container_registry_name
.\scripts\deploy-backend.ps1 `
    -AcrName $ACR_NAME `
    -Tag "v1.0.0" `
    -ResourceGroup "myleague-dev-rg" `
    -ContainerAppName "myleague-dev-backend" `
    -Deploy

# 3. Seed database (NEW!)
cd terraform
$backendUrl = terraform output -raw backend_url
cd ..
.\scripts\seed-data.ps1 -BackendUrl $backendUrl

# 4. Deploy frontend (optional)
$BACKEND_URL = terraform output -raw backend_url
.\scripts\deploy-frontend.ps1 `
    -AcrName $ACR_NAME `
    -Tag "v1.0.0" `
    -ApiUrl "$BACKEND_URL/api" `
    -ResourceGroup "myleague-dev-rg" `
    -ContainerAppName "myleague-dev-frontend" `
    -Deploy
```

---

## What Data Gets Seeded?

The seeder creates the following test data:

### 1. Base Persons
- Sample person: Ada Lovelace

### 2. Clubs
- **Algorithm Athletic Club** (London, UK)
- **Binary United Club** (Manchester, UK)

### 3. Divisions
- **Premier Division** (Level 1)

### 4. Players & Goalies
- 10 field players (Alex Forward, Casey Winger, Blake Forward, etc.)
- 2 goalies (Jordan Goalie, Pat Goalie)

### 5. Referees
- 2 referees (Robin Ref, Chris Ref)

### 6. Teams
- **Falcons** (Algorithm Athletic Club)
  - 3 forwards, 2 defenders, 1 goalie
  - Home: Main Arena
  - Colors: Red/White
- **Wolves** (Binary United Club)
  - 3 forwards, 2 defenders, 1 goalie
  - Home: North Arena
  - Colors: Blue/Black

### 7. Seasons
- **2025 Regular Season** (January 1 - December 31, 2025)

---

## Customizing Seed Data

To customize the data that gets seeded, edit:

```
src/tools/Seeder/appsettings.json
```

### Example: Add a New Club

```json
{
  "Seeder": {
    "Clubs": [
      {
        "Name": "My Custom Club",
        "City": "Helsinki",
        "Country": "Finland",
        "FoundingDate": "2020-01-01T00:00:00Z",
        "WebsiteUrl": "https://myclub.com",
        "LogoUrl": "https://example.com/logo.png",
        "ContactEmail": "contact@myclub.com"
      }
    ]
  }
}
```

After editing, run the seeder again:

```powershell
.\scripts\seed-data.ps1 -BackendUrl "https://your-backend-url.azurecontainerapps.io"
```

---

## Manual Seeding (Advanced)

### Seed Against Local Development Backend

```powershell
# Start your backend locally first
cd src/backend/WebAPI
dotnet run

# In another terminal, run seeder
cd src/tools/Seeder
dotnet run --configuration Release
```

The seeder will use `http://localhost:8080/` from `appsettings.json`.

### Seed Against Custom URL

```powershell
# Set environment variable
$env:SEEDER_BASEURL = "https://my-custom-backend.com/"

# Run seeder
cd src/tools/Seeder
dotnet run --configuration Release

# Clean up
Remove-Item Env:\SEEDER_BASEURL
```

---

## Troubleshooting

### Error: "Could not get backend URL from Terraform"

**Solution**: Make sure your infrastructure is deployed first:

```powershell
cd terraform
terraform apply -var-file="terraform.tfvars.dev"
```

### Error: "Seeder.csproj not found"

**Solution**: Make sure you're running the script from the project root:

```powershell
# Should be in: C:\...\myleague-app-XAMKFI\
.\scripts\seed-data.ps1
```

### Error: "Connection refused" or "503 Service Unavailable"

**Solution**: Make sure your backend is running and healthy:

```powershell
# Check backend status
az containerapp show `
    --name myleague-dev-backend `
    --resource-group myleague-dev-rg `
    --query "properties.runningStatus"

# Check backend logs
az containerapp logs show `
    --name myleague-dev-backend `
    --resource-group myleague-dev-rg `
    --follow
```

### Error: "Database connection failed"

**Solution**: Make sure your database is running and the backend can connect:

1. Check PostgreSQL server status in Azure Portal
2. Verify firewall rules allow Azure services
3. Check backend logs for connection errors

---

## Re-seeding (Idempotent Operations)

You can safely run the seeder multiple times. It will:
- **Skip** existing records (checked by name/email)
- **Create** only new records
- **Not duplicate** data

```powershell
# Safe to run multiple times
.\scripts\seed-data.ps1
.\scripts\seed-data.ps1  # Won't create duplicates
```

---

## CI/CD Integration

To integrate seeding into your CI/CD pipeline:

### GitHub Actions Example

```yaml
- name: Seed Database
  run: |
    cd scripts
    ./seed-data.ps1 -Environment dev
  shell: pwsh
```

### Azure DevOps Example

```yaml
- task: PowerShell@2
  displayName: 'Seed Database'
  inputs:
    filePath: 'scripts/seed-data.ps1'
    arguments: '-Environment dev'
```

---

## Best Practices

1. **Seed after backend deployment** - Always seed after deploying a new backend
2. **Use version-specific data** - Keep seed data relevant to your app version
3. **Don't seed production with test data** - Use minimal/real data for prod
4. **Document custom seeds** - If you modify seed data, document the changes
5. **Check logs** - Review seeder output to ensure all data was created

---

## Related Documentation

- [Deployment Guide](../terraform/DEPLOYMENT-GUIDE.md)
- [Versioning Guide](./VERSIONING-GUIDE.md)
- [Seeder README](../src/tools/Seeder/README.md)

---

## Summary

The database seeder is a powerful tool for quickly populating your backend with realistic test data. Use it after every deployment to ensure your application has the data it needs for testing and development.

**Quick Command**:
```powershell
$url = terraform output -raw backend_url; .\scripts\seed-data.ps1 -BackendUrl $url
```

That's all you need! 🎉

