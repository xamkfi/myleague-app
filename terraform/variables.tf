variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
  default     = "myleague-rg"
}

variable "location" {
  description = "Azure region where resources will be deployed"
  type        = string
  default     = "West Europe"
}

variable "app_name" {
  description = "Name of the application (used as prefix for resources)"
  type        = string
  default     = "myleague"
}

variable "acr_name" {
  description = "Name prefix for Azure Container Registry (will have random suffix)"
  type        = string
  default     = "myleagueacr"
}

variable "postgres_server_name" {
  description = "Name prefix for PostgreSQL server (will have random suffix)"
  type        = string
  default     = "myleague-postgres"
}

variable "postgres_admin_user" {
  description = "PostgreSQL administrator username"
  type        = string
  default     = "myleague_admin"
}

variable "postgres_database_name" {
  description = "Name of the PostgreSQL database"
  type        = string
  default     = "myleague"
}

variable "postgres_sku_name" {
  description = "PostgreSQL SKU name (e.g., B_Standard_B1ms, GP_Standard_D2s_v3)"
  type        = string
  default     = "B_Standard_B1ms"
}

variable "postgres_storage_mb" {
  description = "PostgreSQL storage size in MB"
  type        = number
  default     = 32768
}

variable "enable_postgres_public_access" {
  description = "Enable public network access to PostgreSQL (set to true for development, false for production)"
  type        = bool
  default     = false
}

variable "allowed_ip_addresses" {
  description = "List of IP addresses allowed to access PostgreSQL when public access is enabled (get your IP from https://ifconfig.me)"
  type        = list(string)
  default     = []
}

variable "backend_image_name" {
  description = "Name of the backend Docker image in ACR"
  type        = string
  default     = "webapi"
}

variable "backend_image_tag" {
  description = "Tag of the backend Docker image"
  type        = string
  default     = "latest"
}

variable "frontend_image_name" {
  description = "Name of the frontend Docker image in ACR"
  type        = string
  default     = "frontend"
}

variable "frontend_image_tag" {
  description = "Tag of the frontend Docker image"
  type        = string
  default     = "latest"
}

variable "backend_min_replicas" {
  description = "Minimum number of replicas for backend container app"
  type        = number
  default     = 1
}

variable "backend_max_replicas" {
  description = "Maximum number of replicas for backend container app"
  type        = number
  default     = 10
}

variable "frontend_min_replicas" {
  description = "Minimum number of replicas for frontend container app"
  type        = number
  default     = 1
}

variable "frontend_max_replicas" {
  description = "Maximum number of replicas for frontend container app"
  type        = number
  default     = 10
}

variable "backend_cpu" {
  description = "CPU allocation for backend container (e.g., 0.25, 0.5, 1.0)"
  type        = number
  default     = 0.5
}

variable "backend_memory" {
  description = "Memory allocation for backend container (e.g., 0.5Gi, 1.0Gi)"
  type        = string
  default     = "1.0Gi"
}

variable "frontend_cpu" {
  description = "CPU allocation for frontend container (e.g., 0.25, 0.5, 1.0)"
  type        = number
  default     = 0.25
}

variable "frontend_memory" {
  description = "Memory allocation for frontend container (e.g., 0.5Gi, 1.0Gi)"
  type        = string
  default     = "0.5Gi"
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default = {
    Environment = "Production"
    Project     = "MyLeague"
    ManagedBy   = "Terraform"
  }
}

# Jump Box VM Configuration
variable "enable_jumpbox" {
  description = "Enable jump box VM for database access"
  type        = bool
  default     = true
}

variable "jumpbox_vm_size" {
  description = "Size of the jump box VM (e.g., Standard_B1s for cheapest)"
  type        = string
  default     = "Standard_B1s"
}

variable "jumpbox_admin_username" {
  description = "Admin username for jump box VM"
  type        = string
  default     = "azureuser"
}

variable "jumpbox_ssh_public_key" {
  description = "SSH public key for jump box VM access (if not provided, password auth will be used)"
  type        = string
  default     = ""
}

variable "jumpbox_allowed_source_ips" {
  description = "List of IP addresses allowed to SSH to jump box (empty list allows all)"
  type        = list(string)
  default     = []
}

variable "cors_allowed_origins" {
  description = "CORS allowed origins for backend (use * for dev, specific URLs for prod)"
  type        = string
  default     = "*"
}

variable "postgres_version" {
  description = "PostgreSQL server version"
  type        = string
  default     = "16"
}

variable "postgres_backup_retention_days" {
  description = "Number of days to retain PostgreSQL backups"
  type        = number
  default     = 7
}

variable "postgres_geo_redundant_backup" {
  description = "Enable geo-redundant backups for PostgreSQL"
  type        = bool
  default     = false
}

