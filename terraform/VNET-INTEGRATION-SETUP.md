# VNet Integration Setup for Container Apps Environment

## Problem
The Container Apps Environment (`myleague-env`) needs VNet integration to allow Container Apps to access the private PostgreSQL database.

## Important Note
**VNet integration cannot be changed via Terraform on an existing environment** without recreating it, which would require deleting all Container Apps first.

## Solution: Enable VNet Integration Manually

### Option 1: Azure Portal (Easiest)

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to: **Resource Groups** → `myleague-rg`
3. Click on **`myleague-env`** (Container Apps Environment)
4. In the left menu, go to **Settings** → **Networking**
5. Under **VNet Integration**, click **Add subnet**
6. Select:
   - **Virtual network**: `myleague-vnet`
   - **Subnet**: `container-apps-subnet`
7. Click **Add**
8. Wait for the integration to complete (1-2 minutes)

### Option 2: Azure CLI (PowerShell)

Run this command to enable VNet integration:

```powershell
$subnetId = "/subscriptions/c8d51cd3-afd4-4f93-8587-bf42566ad130/resourceGroups/myleague-rg/providers/Microsoft.Network/virtualNetworks/myleague-vnet/subnets/container-apps-subnet"

az rest --method PUT `
  --url "https://management.azure.com/subscriptions/c8d51cd3-afd4-4f93-8587-bf42566ad130/resourceGroups/myleague-rg/providers/Microsoft.App/managedEnvironments/myleague-env?api-version=2023-05-01" `
  --headers "Content-Type=application/json" `
  --body "{`"properties`":{`"vnetConfiguration`":{`"infrastructureSubnetId`":`"$subnetId`"}}}"
```

### After Enabling VNet Integration

1. **Wait 2-3 minutes** for the environment to update
2. **Restart the backend Container App** to pick up the new network configuration:

```powershell
# Restart backend
az containerapp revision restart `
  --name myleague-backend `
  --resource-group myleague-rg `
  --revision myleague-backend--l8a0ys6

# Or create a new revision by updating the image
az containerapp update `
  --name myleague-backend `
  --resource-group myleague-rg `
  --image myleagueacrom63tv.azurecr.io/webapi:latest
```

3. **Verify backend can connect** by checking logs:

```powershell
az containerapp logs show `
  --name myleague-backend `
  --resource-group myleague-rg `
  --tail 50
```

### Update Terraform Configuration

After manually enabling VNet integration, update `terraform/main.tf`:

1. Uncomment the `infrastructure_subnet_id` line
2. Uncomment the `lifecycle` block to prevent Terraform from trying to change it:

```terraform
resource "azurerm_container_app_environment" "main" {
  # ... other config ...
  infrastructure_subnet_id   = azurerm_subnet.container_apps.id
  
  lifecycle {
    ignore_changes = [infrastructure_subnet_id]
  }
}
```

This tells Terraform to ignore changes to this field, so it won't try to recreate the environment.

### Verify It's Working

After enabling VNet integration and restarting the backend:

1. Check backend health:
   ```powershell
   az containerapp revision list --name myleague-backend --resource-group myleague-rg --query "[0].{Status:properties.runningState, Health:properties.healthState}"
   ```

2. Test the backend URL:
   ```
   https://myleague-backend.redsky-7ba1d635.westeurope.azurecontainerapps.io/health
   ```

3. Check logs for database connection success (no more "Name or service not known" errors)

