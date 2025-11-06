# Terraform Configuration Improvements

This document summarizes the improvements made to the Terraform configuration based on best practices and security recommendations.

## Changes made

### 1. Key Vault Access Model (Critical Fix)

**Problem**: Mixed access model (access policies + RBAC) caused conflicts and could break managed identity reads.

**Fix**: Switched to RBAC-only model.

```terraform
resource "azurerm_key_vault" "main" {
  # Added:
  enable_rbac_authorization = true
  
  # Removed: Both access_policy blocks
}

# Added role assignment for Terraform operator:
resource "azurerm_role_assignment" "key_vault_administrator" {
  scope                = azurerm_key_vault.main.id
  role_definition_name = "Key Vault Administrator"
  principal_id         = data.azurerm_client_config.current.object_id
}
```

**Impact**: More secure, scalable, and prevents managed identity access issues.

---

### 2. FQDN Cycle Dependency (Critical Fix)

**Problem**: Backend referenced frontend FQDN, frontend referenced backend FQDN → circular dependency.

**Fix**: Backend CORS now uses a variable instead of cross-referencing frontend.

```terraform
# Backend env var now uses variable:
env {
  name  = "Cors__AllowedOrigins"
  value = var.cors_allowed_origins  # No longer references frontend FQDN
}
```

**New variable**:
```terraform
variable "cors_allowed_origins" {
  description = "CORS allowed origins for backend (use * for dev, specific URLs for prod)"
  type        = string
  default     = "*"
}
```

**Impact**: Eliminates circular dependency, allows flexible CORS configuration.

---

### 3. Frontend API_URL Environment Variable (Removed)

**Problem**: Vite environment variables are build-time only; runtime env vars don't work.

**Fix**: Removed the unused `API_URL` env var from frontend container.

**Impact**: Cleaner configuration, no misleading unused variables.

---

### 4. Subnet Delegation (Explicit)

**Problem**: Delegation was commented out, relying on Azure's automatic behavior.

**Fix**: Made delegation explicit in Terraform.

```terraform
resource "azurerm_subnet" "container_apps" {
  # Added explicit delegation:
  delegation {
    name = "Microsoft.App/environments"
    service_delegation {
      name    = "Microsoft.App/environments"
      actions = ["Microsoft.Network/virtualNetworks/subnets/join/action"]
    }
  }
}
```

**Impact**: Prevents surprises from Azure behavior changes, more predictable.

---

### 5. Jump Box Security (Improved)

**Problem**: SSH allowed from any IP address (`*`).

**Fix**: Made SSH source IPs configurable via variable.

```terraform
security_rule {
  # Changed from: source_address_prefix = "*"
  # To:
  source_address_prefixes = length(var.jumpbox_allowed_source_ips) > 0 ? var.jumpbox_allowed_source_ips : ["*"]
}
```

**New variable**:
```terraform
variable "jumpbox_allowed_source_ips" {
  description = "List of IP addresses allowed to SSH to jump box (empty list allows all)"
  type        = list(string)
  default     = []
}
```

**Impact**: Better security when configured with specific IPs.

---

### 6. PostgreSQL Backup Configuration (Added)

**Problem**: Backup settings were not explicitly configured.

**Fix**: Added explicit backup configuration.

```terraform
resource "azurerm_postgresql_flexible_server" "main" {
  # Added:
  backup_retention_days        = var.postgres_backup_retention_days
  geo_redundant_backup_enabled = var.postgres_geo_redundant_backup
  version                      = var.postgres_version  # Now configurable
}
```

**New variables**:
```terraform
variable "postgres_version" {
  default = "16"
}

variable "postgres_backup_retention_days" {
  default = 7
}

variable "postgres_geo_redundant_backup" {
  default = false
}
```

**Impact**: Explicit backup control, better disaster recovery planning.

---

### 7. Output FQDN Consistency (Fixed)

**Problem**: Outputs used `latest_revision_fqdn` while main.tf used `ingress[0].fqdn`.

**Fix**: Made outputs consistent with main.tf.

```terraform
output "backend_url" {
  value = "https://${azurerm_container_app.backend.ingress[0].fqdn}"
}
```

**Impact**: Consistent FQDN references across configuration.

---

## New variables summary

| Variable | Default | Description |
|----------|---------|-------------|
| `cors_allowed_origins` | `"*"` | CORS origins for backend (use specific URLs for prod) |
| `jumpbox_allowed_source_ips` | `[]` | List of IPs allowed to SSH (empty = all) |
| `postgres_version` | `"16"` | PostgreSQL server version |
| `postgres_backup_retention_days` | `7` | Backup retention period |
| `postgres_geo_redundant_backup` | `false` | Enable geo-redundant backups |

---

## Migration guide

### For new deployments

Just use the updated configuration. All defaults are set.

### For existing deployments

1. **Key Vault RBAC migration**
   
   After applying the changes, Key Vault will switch from access policies to RBAC. This requires:
   - Ensuring your Azure account has permissions to assign RBAC roles
   - The role assignment will happen automatically on `terraform apply`

2. **CORS configuration**
   
   After first apply:
   ```powershell
   # Get the frontend URL
   cd terraform
   $FRONTEND_URL = terraform output -raw frontend_url
   
   # Update terraform.tfvars with actual frontend URL
   # cors_allowed_origins = "https://myleague-dev-frontend.XXXX.azurecontainerapps.io"
   
   # Re-apply
   terraform apply -var-file="terraform.tfvars.dev"
   ```

3. **Optional: Restrict jump box SSH**
   
   In your `.tfvars` file:
   ```hcl
   jumpbox_allowed_source_ips = ["YOUR_IP_ADDRESS"]
   ```

---

## Configuration examples

### Development (`terraform.tfvars.dev`)

```hcl
# Permissive CORS for development
cors_allowed_origins = "*"

# PostgreSQL settings
postgres_version                 = "16"
postgres_backup_retention_days   = 7
postgres_geo_redundant_backup    = false

# Jump box disabled (using public DB access)
enable_jumpbox = false
```

### Production (`terraform.tfvars.prod`)

```hcl
# Strict CORS for production (update after first deployment)
cors_allowed_origins = "https://myleague-frontend.XXXX.azurecontainerapps.io"

# PostgreSQL settings
postgres_version                 = "16"
postgres_backup_retention_days   = 30
postgres_geo_redundant_backup    = true

# Jump box enabled with restricted SSH
enable_jumpbox = true
jumpbox_allowed_source_ips = ["YOUR_OFFICE_IP", "YOUR_HOME_IP"]
```

---

## Testing

After applying changes:

1. **Verify Key Vault access**:
   ```powershell
   # Check role assignments
   az role assignment list --scope $(terraform output -raw key_vault_uri) --output table
   ```

2. **Verify backend CORS**:
   ```powershell
   # Check backend logs for CORS errors
   az containerapp logs show --name myleague-dev-backend --resource-group myleague-dev-rg --follow
   ```

3. **Verify PostgreSQL backups**:
   ```powershell
   # Check backup configuration
   az postgres flexible-server show --name <server-name> --resource-group <rg> --query "{retention:backup.backupRetentionDays,geoRedundant:backup.geoRedundantBackup}"
   ```

---

## Rollback

If issues occur:

1. **Key Vault**: The old access policies were removed. To rollback:
   - Remove `enable_rbac_authorization = true`
   - Add back the access_policy blocks
   - Remove the `azurerm_role_assignment.key_vault_administrator`

2. **CORS**: Set `cors_allowed_origins = "*"` temporarily

---

## Additional recommendations (not implemented)

These are optional improvements you can consider:

1. **ACR Managed Identity**: Switch from admin credentials to managed identity for better security (requires Owner role)

2. **Health Checks**: Add liveness and readiness probes to Container Apps

3. **Custom Domains**: Configure custom domains and SSL certificates

4. **Azure Bastion**: Replace jump box with Azure Bastion for better security

---

**Last Updated**: 2025-01-15

