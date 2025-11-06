output "resource_group_name" {
  description = "Name of the resource group"
  value       = azurerm_resource_group.main.name
}

output "container_registry_name" {
  description = "Name of the Azure Container Registry"
  value       = azurerm_container_registry.main.name
}

output "container_registry_login_server" {
  description = "Login server URL for Azure Container Registry"
  value       = azurerm_container_registry.main.login_server
}

# ACR admin credentials (enabled since role assignment is skipped)
output "container_registry_admin_username" {
  description = "Admin username for Azure Container Registry"
  value       = azurerm_container_registry.main.admin_username
  sensitive   = true
}

output "container_registry_admin_password" {
  description = "Admin password for Azure Container Registry"
  value       = azurerm_container_registry.main.admin_password
  sensitive   = true
}

output "postgres_server_name" {
  description = "Name of the PostgreSQL server"
  value       = azurerm_postgresql_flexible_server.main.name
}

output "postgres_server_fqdn" {
  description = "Fully qualified domain name of the PostgreSQL server"
  value       = azurerm_postgresql_flexible_server.main.fqdn
}

output "postgres_database_name" {
  description = "Name of the PostgreSQL database"
  value       = azurerm_postgresql_flexible_server_database.main.name
}

output "postgres_admin_user" {
  description = "PostgreSQL administrator username"
  value       = var.postgres_admin_user
}

output "postgres_admin_password" {
  description = "PostgreSQL administrator password (randomly generated)"
  value       = random_password.postgres_password.result
  sensitive   = true
}

# Alias for backward compatibility
output "postgres_password" {
  description = "PostgreSQL administrator password (alias for postgres_admin_password)"
  value       = random_password.postgres_password.result
  sensitive   = true
}

output "connection_string" {
  description = "PostgreSQL connection string"
  value       = "Host=${azurerm_postgresql_flexible_server.main.fqdn};Database=${azurerm_postgresql_flexible_server_database.main.name};Username=${var.postgres_admin_user};Password=${random_password.postgres_password.result};SSL Mode=Require;"
  sensitive   = true
}

output "backend_url" {
  description = "URL of the backend Container App"
  value       = "https://${azurerm_container_app.backend.latest_revision_fqdn}"
}

output "frontend_url" {
  description = "URL of the frontend Container App"
  value       = "https://${azurerm_container_app.frontend.latest_revision_fqdn}"
}

output "application_insights_instrumentation_key" {
  description = "Instrumentation key for Application Insights"
  value       = azurerm_application_insights.main.instrumentation_key
  sensitive   = true
}

output "application_insights_connection_string" {
  description = "Connection string for Application Insights"
  value       = azurerm_application_insights.main.connection_string
  sensitive   = true
}

output "log_analytics_workspace_id" {
  description = "ID of the Log Analytics Workspace"
  value       = azurerm_log_analytics_workspace.main.id
}

output "key_vault_name" {
  description = "Name of the Key Vault"
  value       = azurerm_key_vault.main.name
}

output "key_vault_uri" {
  description = "URI of the Key Vault"
  value       = azurerm_key_vault.main.vault_uri
}

# Jump Box Outputs
output "jumpbox_enabled" {
  description = "Whether jump box VM is enabled"
  value       = var.enable_jumpbox
}

output "jumpbox_public_ip" {
  description = "Public IP address of the jump box VM"
  value       = var.enable_jumpbox ? azurerm_public_ip.jumpbox[0].ip_address : "N/A - Jump box not enabled"
}

output "jumpbox_ssh_command" {
  description = "SSH command to connect to jump box"
  value       = var.enable_jumpbox ? "ssh ${var.jumpbox_admin_username}@${azurerm_public_ip.jumpbox[0].ip_address}" : "N/A - Jump box not enabled"
}

output "jumpbox_admin_username" {
  description = "Admin username for jump box VM"
  value       = var.enable_jumpbox ? var.jumpbox_admin_username : "N/A - Jump box not enabled"
}

output "jumpbox_admin_password" {
  description = "Admin password for jump box VM (only if SSH key not provided)"
  value       = var.enable_jumpbox && var.jumpbox_ssh_public_key == "" ? random_password.jumpbox_password[0].result : "N/A - Using SSH key or jump box not enabled"
  sensitive   = true
}

output "jumpbox_postgres_command" {
  description = "PostgreSQL connection command from jump box"
  value       = var.enable_jumpbox ? "psql \"host=${azurerm_postgresql_flexible_server.main.fqdn} port=5432 dbname=${var.postgres_database_name} user=${var.postgres_admin_user} sslmode=require\"" : "N/A - Jump box not enabled"
}

