terraform {
  required_version = ">= 1.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.0"
    }
  }

  backend "azurerm" {
    # Configure backend storage in terraform/backend.tfvars
    # resource_group_name  = "terraform-state-rg"
    # storage_account_name = "tfstateq8zi4k"
    # container_name       = "tfstate"
    # key                  = "myleague-app.terraform.tfstate"
  }
}

provider "azurerm" {
  features {
    resource_group {
      prevent_deletion_if_contains_resources = false
    }
  }
}

# Resource Group
resource "azurerm_resource_group" "main" {
  name     = var.resource_group_name
  location = var.location

  tags = var.tags
}

# Random suffix for unique resource names
resource "random_string" "suffix" {
  length  = 6
  special = false
  upper   = false
}

# Azure Container Registry
resource "azurerm_container_registry" "main" {
  name                = "${var.acr_name}${random_string.suffix.result}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  sku                 = "Basic"
  # Enable admin user - required since role assignment is skipped
  # This allows Container Apps to pull images without role assignments
  admin_enabled       = true

  tags = var.tags
}

# Azure Database for PostgreSQL Flexible Server
resource "azurerm_postgresql_flexible_server" "main" {
  name                   = "${var.postgres_server_name}-${random_string.suffix.result}"
  resource_group_name    = azurerm_resource_group.main.name
  location               = azurerm_resource_group.main.location
  version                = "16"
  
  # VNet integration only when public access is disabled (production mode)
  delegated_subnet_id    = var.enable_postgres_public_access ? null : azurerm_subnet.postgres.id
  private_dns_zone_id    = var.enable_postgres_public_access ? null : azurerm_private_dns_zone.postgres.id
  
  administrator_login    = var.postgres_admin_user
  administrator_password = random_password.postgres_password.result
  
  # Zone must be set if it was set before - keep existing zone or remove via lifecycle
  # Using lifecycle ignore_changes to prevent zone modification issues
  lifecycle {
    ignore_changes = [zone]
  }

  storage_mb = var.postgres_storage_mb
  sku_name   = var.postgres_sku_name

  # High availability is disabled by default - omit the block to disable it

  # Public access control
  # Production: private VNet access only (more secure)
  # Development: can enable public access with firewall rules for easier local development
  public_network_access_enabled = var.enable_postgres_public_access

  maintenance_window {
    day_of_week  = 0
    start_hour   = 0
    start_minute = 0
  }

  depends_on = [azurerm_private_dns_zone_virtual_network_link.postgres]

  tags = var.tags
}

# PostgreSQL Database
resource "azurerm_postgresql_flexible_server_database" "main" {
  name      = var.postgres_database_name
  server_id = azurerm_postgresql_flexible_server.main.id
  collation = "en_US.utf8"
  charset   = "utf8"
}

# PostgreSQL Firewall Rules (only for development with public access)
# When public_network_access_enabled = true, these rules control who can connect
# For production with private VNet access, firewall rules are not needed
resource "azurerm_postgresql_flexible_server_firewall_rule" "allowed_ips" {
  count            = var.enable_postgres_public_access ? length(var.allowed_ip_addresses) : 0
  name             = "AllowedIP-${count.index}"
  server_id        = azurerm_postgresql_flexible_server.main.id
  start_ip_address = var.allowed_ip_addresses[count.index]
  end_ip_address   = var.allowed_ip_addresses[count.index]
}

# Allow Azure services to access the database (when public access is enabled)
# This allows Container Apps to connect when not using VNet integration
resource "azurerm_postgresql_flexible_server_firewall_rule" "allow_azure_services" {
  count            = var.enable_postgres_public_access ? 1 : 0
  name             = "AllowAzureServices"
  server_id        = azurerm_postgresql_flexible_server.main.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

# Application Insights
resource "azurerm_application_insights" "main" {
  name                = "${var.app_name}-insights-${random_string.suffix.result}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  application_type    = "web"

  # Link to Log Analytics Workspace (required if it was previously set)
  workspace_id = azurerm_log_analytics_workspace.main.id

  # Cost optimization: Cap daily data ingestion
  daily_data_cap_in_gb                  = 0.1
  daily_data_cap_notifications_disabled  = true

  # Ensure workspace is created first
  depends_on = [azurerm_log_analytics_workspace.main]

  tags = var.tags
}

# Log Analytics Workspace
resource "azurerm_log_analytics_workspace" "main" {
  name                = "${var.app_name}-logs-${random_string.suffix.result}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  sku                 = "PerGB2018"
  # Minimum retention for PerGB2018 SKU is 30 days
  retention_in_days   = 30

  tags = var.tags
}

# Virtual Network for Container Apps
resource "azurerm_virtual_network" "main" {
  name                = "${var.app_name}-vnet"
  address_space       = ["10.0.0.0/16"]
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name

  tags = var.tags
}

# Subnet for Container Apps
# Container Apps environment requires /23 subnet (not /24) for proper scaling
# Note: Delegation will be automatically added by Azure when the environment is created
# with infrastructure_subnet_id - we don't set it in Terraform to avoid conflicts
resource "azurerm_subnet" "container_apps" {
  name                 = "container-apps-subnet-v2"
  resource_group_name  = azurerm_resource_group.main.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = ["10.0.4.0/23"]  # Using different address range to avoid overlap

  # Remove delegation from Terraform - Azure will handle it automatically
  # when the Container Apps Environment is created with this subnet
  # delegation {
  #   name = "Microsoft.App/environments"
  #   service_delegation {
  #     name    = "Microsoft.App/environments"
  #     actions = ["Microsoft.Network/virtualNetworks/subnets/join/action"]
  #   }
  # }
}

# Subnet for PostgreSQL
# Using 10.0.2.0/24 to avoid overlap with Container Apps subnet (10.0.0.0/23)
resource "azurerm_subnet" "postgres" {
  name                 = "postgres-subnet"
  resource_group_name  = azurerm_resource_group.main.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = ["10.0.2.0/24"]

  delegation {
    name = "Microsoft.DBforPostgreSQL/flexibleServers"
    service_delegation {
      name    = "Microsoft.DBforPostgreSQL/flexibleServers"
      actions = ["Microsoft.Network/virtualNetworks/subnets/join/action"]
    }
  }
}

# Private DNS Zone for PostgreSQL
# Must be exactly "privatelink.postgres.database.azure.com" for Flexible Server with private access
resource "azurerm_private_dns_zone" "postgres" {
  name                = "privatelink.postgres.database.azure.com"
  resource_group_name = azurerm_resource_group.main.name

  tags = var.tags
}

# Private DNS Zone Virtual Network Link
resource "azurerm_private_dns_zone_virtual_network_link" "postgres" {
  name                  = "${var.app_name}-postgres-dns-link"
  resource_group_name   = azurerm_resource_group.main.name
  private_dns_zone_name = azurerm_private_dns_zone.postgres.name
  virtual_network_id    = azurerm_virtual_network.main.id
  registration_enabled  = false

  tags = var.tags
}

# Container Apps Environment
# IMPORTANT: VNet integration MUST be set at creation time - cannot be added later
# This creates the environment with VNet integration from the start
resource "azurerm_container_app_environment" "main" {
  name                       = "${var.app_name}-env"
  location                   = azurerm_resource_group.main.location
  resource_group_name        = azurerm_resource_group.main.name
  log_analytics_workspace_id = azurerm_log_analytics_workspace.main.id
  
  # VNet integration - required for accessing private PostgreSQL database
  # This MUST be set at creation - cannot be changed later
  infrastructure_subnet_id = azurerm_subnet.container_apps.id

  # Internal load balancer not needed for external access
  internal_load_balancer_enabled = false

  tags = var.tags

  # Ensure subnet exists before creating environment
  depends_on = [
    azurerm_subnet.container_apps,
    azurerm_private_dns_zone_virtual_network_link.postgres
  ]
}

# Container App for Backend API
resource "azurerm_container_app" "backend" {
  name                         = "${var.app_name}-backend"
  container_app_environment_id = azurerm_container_app_environment.main.id
  resource_group_name          = azurerm_resource_group.main.name
  revision_mode                = "Single"

  # Use ACR admin credentials since role assignment is skipped
  registry {
    server   = azurerm_container_registry.main.login_server
    username = azurerm_container_registry.main.admin_username
    password_secret_name = "acr-password"
  }

  # Store ACR password as a secret
  secret {
    name  = "acr-password"
    value = azurerm_container_registry.main.admin_password
  }

  # Note: Database and Application Insights secrets are stored in Key Vault
  # The backend application fetches them directly from Key Vault using managed identity
  # No need to store them here in Container Apps secrets

  # Keep identity for other uses (not needed for ACR with admin enabled)
  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.main.id]
  }

  ingress {
    allow_insecure_connections = false
    external_enabled           = true
    target_port                = 8080
    transport                  = "http"
    traffic_weight {
      latest_revision = true
      percentage      = 100
    }
  }

  template {
    min_replicas = var.backend_min_replicas
    max_replicas = var.backend_max_replicas

    container {
      name   = "webapi"
      image  = "${azurerm_container_registry.main.login_server}/${var.backend_image_name}:${var.backend_image_tag}"
      cpu    = var.backend_cpu
      memory = var.backend_memory

      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = "Production"
      }

      env {
        name  = "ASPNETCORE_URLS"
        value = "http://+:8080"
      }

      # Key Vault URI for direct integration (backend will fetch secrets from Key Vault)
      env {
        name  = "KeyVault__VaultUri"
        value = azurerm_key_vault.main.vault_uri
      }

      # Managed Identity Client ID - tells DefaultAzureCredential which identity to use
      env {
        name  = "AZURE_CLIENT_ID"
        value = azurerm_user_assigned_identity.main.client_id
      }

      # CORS allowed origins - includes frontend URL
      # Format: semicolon-separated list of origins
      env {
        name  = "Cors__AllowedOrigins"
        value = "https://${azurerm_container_app.frontend.ingress[0].fqdn}"
      }

      # Connection strings will be loaded from Key Vault by the backend application
      # The backend uses Azure Key Vault configuration provider to fetch:
      # - PostgresConnectionString -> ConnectionStrings:DefaultConnection
      # - AppInsightsConnectionString -> ApplicationInsights:ConnectionString
    }
  }

  tags = var.tags
}

# Container App for Frontend
resource "azurerm_container_app" "frontend" {
  name                         = "${var.app_name}-frontend"
  container_app_environment_id = azurerm_container_app_environment.main.id
  resource_group_name          = azurerm_resource_group.main.name
  revision_mode                = "Single"

  # Use ACR admin credentials since role assignment is skipped
  registry {
    server   = azurerm_container_registry.main.login_server
    username = azurerm_container_registry.main.admin_username
    password_secret_name = "acr-password"
  }

  # Store ACR password as a secret
  secret {
    name  = "acr-password"
    value = azurerm_container_registry.main.admin_password
  }

  # Keep identity for other uses (not needed for ACR with admin enabled)
  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.main.id]
  }

  ingress {
    allow_insecure_connections = false
    external_enabled           = true
    target_port                = 80  # nginx serves on port 80 (production build)
    transport                  = "http"
    traffic_weight {
      latest_revision = true
      percentage      = 100
    }
  }

  template {
    min_replicas = var.frontend_min_replicas
    max_replicas = var.frontend_max_replicas

    container {
      name   = "frontend"
      image  = "${azurerm_container_registry.main.login_server}/${var.frontend_image_name}:${var.frontend_image_tag}"
      cpu    = var.frontend_cpu
      memory = var.frontend_memory

      # Note: Vite environment variables (VITE_*) are embedded at build time
      # For runtime configuration, use a config file or window object
      # This env var is here for reference but won't work for Vite apps at runtime
      # Use stable ingress FQDN instead of latest_revision_fqdn to avoid Terraform inconsistencies
      env {
        name  = "API_URL"
        value = "https://${azurerm_container_app.backend.ingress[0].fqdn}"
      }
    }
  }

  tags = var.tags
}

# User Assigned Identity for Container Apps
resource "azurerm_user_assigned_identity" "main" {
  name                = "${var.app_name}-identity-${random_string.suffix.result}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name

  tags = var.tags
}

# Role Assignment for Container Apps to pull from ACR
# COMMENTED OUT: Requires Owner or User Access Administrator role
# Using ACR admin credentials instead (enabled in ACR resource)
# To use managed identity: uncomment this and enable Owner/User Access Admin role
#
# resource "azurerm_role_assignment" "acr_pull" {
#   scope                = azurerm_container_registry.main.id
#   role_definition_name = "AcrPull"
#   principal_id         = azurerm_user_assigned_identity.main.principal_id
#   skip_service_principal_aad_check = true
# }

# Azure Key Vault for storing sensitive configuration
resource "azurerm_key_vault" "main" {
  name                       = "${var.app_name}-kv-${random_string.suffix.result}"
  location                   = azurerm_resource_group.main.location
  resource_group_name        = azurerm_resource_group.main.name
  tenant_id                  = data.azurerm_client_config.current.tenant_id
  sku_name                   = "standard"
  soft_delete_retention_days = 7
  purge_protection_enabled   = false

  # Enable access policy for the managed identity
  access_policy {
    tenant_id = data.azurerm_client_config.current.tenant_id
    object_id = azurerm_user_assigned_identity.main.principal_id

    secret_permissions = [
      "Get",
      "List"
    ]
  }

  # Enable access for current user (to write secrets via Terraform)
  access_policy {
    tenant_id = data.azurerm_client_config.current.tenant_id
    object_id = data.azurerm_client_config.current.object_id

    secret_permissions = [
      "Get",
      "Set",
      "Delete",
      "List",
      "Purge"
    ]
  }

  tags = var.tags
}

# Key Vault Secret for PostgreSQL Connection String
# Using double-dash notation: "--" maps to ":" in .NET configuration
# "ConnectionStrings--DefaultConnection" -> "ConnectionStrings:DefaultConnection"
resource "azurerm_key_vault_secret" "postgres_connection_string" {
  name         = "ConnectionStrings--DefaultConnection"
  value        = "Host=${azurerm_postgresql_flexible_server.main.fqdn};Database=${azurerm_postgresql_flexible_server_database.main.name};Username=${var.postgres_admin_user};Password=${random_password.postgres_password.result};SSL Mode=Require;"
  key_vault_id = azurerm_key_vault.main.id

  tags = var.tags

  depends_on = [
    azurerm_key_vault.main,
    azurerm_postgresql_flexible_server.main,
    azurerm_postgresql_flexible_server_database.main
  ]
}

# Key Vault Secret for Application Insights Connection String
# Using double-dash notation: "--" maps to ":" in .NET configuration
# "ApplicationInsights--ConnectionString" -> "ApplicationInsights:ConnectionString"
resource "azurerm_key_vault_secret" "app_insights_connection_string" {
  name         = "ApplicationInsights--ConnectionString"
  value        = azurerm_application_insights.main.connection_string
  key_vault_id = azurerm_key_vault.main.id

  tags = var.tags

  depends_on = [
    azurerm_key_vault.main,
    azurerm_application_insights.main
  ]
}

# Grant Container App managed identity access to Key Vault
# Using role-based access (more secure than access policies)
resource "azurerm_role_assignment" "key_vault_secrets_user" {
  scope                = azurerm_key_vault.main.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_user_assigned_identity.main.principal_id
}

# Random password for PostgreSQL
resource "random_password" "postgres_password" {
  length  = 32
  special = true
}

# Random password for Jump Box VM (if SSH key not provided)
resource "random_password" "jumpbox_password" {
  count   = var.enable_jumpbox && var.jumpbox_ssh_public_key == "" ? 1 : 0
  length  = 20
  special = true
}

# Subnet for Jump Box VM
resource "azurerm_subnet" "jumpbox" {
  count                = var.enable_jumpbox ? 1 : 0
  name                 = "jumpbox-subnet"
  resource_group_name  = azurerm_resource_group.main.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = ["10.0.3.0/24"]
}

# Network Security Group for Jump Box
resource "azurerm_network_security_group" "jumpbox" {
  count               = var.enable_jumpbox ? 1 : 0
  name                = "${var.app_name}-jumpbox-nsg"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name

  # Allow SSH from anywhere (you can restrict this to your IP for better security)
  security_rule {
    name                       = "AllowSSH"
    priority                   = 1001
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "22"
    source_address_prefix      = "*"  # Change to your IP for better security
    destination_address_prefix = "*"
  }

  tags = var.tags
}

# Public IP for Jump Box
resource "azurerm_public_ip" "jumpbox" {
  count               = var.enable_jumpbox ? 1 : 0
  name                = "${var.app_name}-jumpbox-ip"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  allocation_method   = "Static"
  sku                 = "Standard"

  tags = var.tags
}

# Network Interface for Jump Box
resource "azurerm_network_interface" "jumpbox" {
  count               = var.enable_jumpbox ? 1 : 0
  name                = "${var.app_name}-jumpbox-nic"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name

  ip_configuration {
    name                          = "internal"
    subnet_id                     = azurerm_subnet.jumpbox[0].id
    private_ip_address_allocation = "Dynamic"
    public_ip_address_id          = azurerm_public_ip.jumpbox[0].id
  }

  tags = var.tags
}

# Associate NSG with Jump Box NIC
resource "azurerm_network_interface_security_group_association" "jumpbox" {
  count                     = var.enable_jumpbox ? 1 : 0
  network_interface_id      = azurerm_network_interface.jumpbox[0].id
  network_security_group_id = azurerm_network_security_group.jumpbox[0].id
}

# Jump Box Linux VM
resource "azurerm_linux_virtual_machine" "jumpbox" {
  count               = var.enable_jumpbox ? 1 : 0
  name                = "${var.app_name}-jumpbox"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  size                = var.jumpbox_vm_size
  admin_username      = var.jumpbox_admin_username

  # Disable password authentication if SSH key is provided
  disable_password_authentication = var.jumpbox_ssh_public_key != ""

  network_interface_ids = [
    azurerm_network_interface.jumpbox[0].id,
  ]

  # Use SSH key if provided, otherwise use password
  dynamic "admin_ssh_key" {
    for_each = var.jumpbox_ssh_public_key != "" ? [1] : []
    content {
      username   = var.jumpbox_admin_username
      public_key = var.jumpbox_ssh_public_key
    }
  }

  admin_password = var.jumpbox_ssh_public_key == "" ? random_password.jumpbox_password[0].result : null

  os_disk {
    name                 = "${var.app_name}-jumpbox-osdisk"
    caching              = "ReadWrite"
    storage_account_type = "Standard_LRS"
    disk_size_gb         = 30
  }

  source_image_reference {
    publisher = "Canonical"
    offer     = "0001-com-ubuntu-server-jammy"
    sku       = "22_04-lts-gen2"
    version   = "latest"
  }

  # Install PostgreSQL client and other tools
  custom_data = base64encode(<<-EOF
    #!/bin/bash
    set -e
    
    # Update package list
    apt-get update
    
    # Install PostgreSQL client
    apt-get install -y postgresql-client
    
    # Install useful tools
    apt-get install -y curl wget vim nano net-tools
    
    # Create welcome message
    cat > /etc/motd << 'MOTD'
    ╔═══════════════════════════════════════════════════════════╗
    ║          MyLeague Jump Box - Database Access VM           ║
    ╚═══════════════════════════════════════════════════════════╝
    
    PostgreSQL Connection:
    ----------------------
    psql "host=${azurerm_postgresql_flexible_server.main.fqdn} port=5432 dbname=${var.postgres_database_name} user=${var.postgres_admin_user} sslmode=require"
    
    Password stored in: /home/${var.jumpbox_admin_username}/.pgpass
    
    MOTD
    
    # Create .pgpass file for automatic authentication
    cat > /home/${var.jumpbox_admin_username}/.pgpass << 'PGPASS'
    ${azurerm_postgresql_flexible_server.main.fqdn}:5432:${var.postgres_database_name}:${var.postgres_admin_user}:${random_password.postgres_password.result}
    PGPASS
    
    chown ${var.jumpbox_admin_username}:${var.jumpbox_admin_username} /home/${var.jumpbox_admin_username}/.pgpass
    chmod 600 /home/${var.jumpbox_admin_username}/.pgpass
    
    echo "Jump box setup completed!" | tee -a /var/log/cloud-init-output.log
  EOF
  )

  tags = merge(var.tags, {
    Purpose = "Database Access Jump Box"
  })

  depends_on = [
    azurerm_network_interface_security_group_association.jumpbox,
    azurerm_postgresql_flexible_server.main
  ]
}

# Data source for current Azure client configuration
data "azurerm_client_config" "current" {}

