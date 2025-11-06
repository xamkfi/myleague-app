# ============================================================================
# Terraform Remote State Backend Configuration (OPTIONAL FOR DEVELOPMENT)
# ============================================================================
# 
# FOR DEVELOPMENT (SOLO WORK):
#   You DON'T need this! Just use local state (terraform.tfstate).
#   Simply run: terraform init
#   
# FOR PRODUCTION/TEAM WORK:
#   Enable remote state to prevent conflicts and enable collaboration.
#   Follow the setup instructions below.
#
# ============================================================================

# Azure Storage Backend Configuration
resource_group_name  = "terraform-state-rg"
storage_account_name = "tfstateq8zi4k"  # Must be globally unique
container_name       = "tfstate"
key                  = "myleague-app.terraform.tfstate"

# Security: Use Azure AD authentication (more secure than storage keys)
use_azuread_auth = true

# ============================================================================
# SETUP INSTRUCTIONS (for production/team use)
# ============================================================================
#
# 1. Create storage account for Terraform state:
#    
#    az group create --name terraform-state-rg --location "West Europe"
#    
#    az storage account create \
#      --name tfstateq8zi4k \
#      --resource-group terraform-state-rg \
#      --location "West Europe" \
#      --sku Standard_LRS \
#      --allow-blob-public-access false
#    
#    az storage container create \
#      --name tfstate \
#      --account-name tfstateq8zi4k
#
# 2. Grant yourself access (for Azure AD auth):
#    
#    $userId = az ad signed-in-user show --query id -o tsv
#    $storageId = az storage account show --name tfstateq8zi4k --resource-group terraform-state-rg --query id -o tsv
#    az role assignment create --role "Storage Blob Data Contributor" --assignee $userId --scope $storageId
#
# 3. Initialize Terraform with remote state:
#    
#    terraform init -backend-config=backend.tfvars
#
# 4. Migrate existing local state (if you have one):
#    
#    terraform init -migrate-state -backend-config=backend.tfvars
#
# ============================================================================
# BENEFITS OF REMOTE STATE
# ============================================================================
# - State locking (prevents concurrent modifications)
# - Team collaboration (shared state)
# - State backup and versioning
# - More secure than local state files
# ============================================================================

