#!/bin/bash

# ============================================================================
# MyLeague Backend Infrastructure Provisioning Script
# ============================================================================
# Usage: ./provision-backend.sh [options]
#
# Options:
#   -e, --environment   Environment (staging, prod). Default: staging
#   -l, --location      Azure region. Default: westeurope
#   -g, --resource-group Override resource group name
#   -p, --password      PostgreSQL admin password (will prompt if not provided)
#   -j, --jwt-secret    JWT secret key for token signing (min 32 chars, prompted if not provided)
#   -a, --admin-email   Admin email for database seeding (optional)
#   -m, --alert-email   Admin email for monitoring alerts (optional, prompted if not provided)
#   -s, --skip-login    Skip Azure login check
#   -h, --help          Show this help message
# ============================================================================

set -e

# Default values
ENVIRONMENT="staging"
LOCATION="westeurope"
RESOURCE_GROUP=""
POSTGRES_PASSWORD=""
JWT_SECRET_KEY=""
SEED_ADMIN_EMAIL=""
ALERT_EMAIL=""
SKIP_LOGIN=false

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
MAGENTA='\033[0;35m'
NC='\033[0m' # No Color

# Helper functions
print_step() { echo -e "\n${CYAN}>> $1${NC}"; }
print_success() { echo -e "${GREEN}[OK] $1${NC}"; }
print_warning() { echo -e "${YELLOW}[!] $1${NC}"; }
print_error() { echo -e "${RED}[X] $1${NC}"; }

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -e|--environment)
            ENVIRONMENT="$2"
            shift 2
            ;;
        -l|--location)
            LOCATION="$2"
            shift 2
            ;;
        -g|--resource-group)
            RESOURCE_GROUP="$2"
            shift 2
            ;;
        -p|--password)
            POSTGRES_PASSWORD="$2"
            shift 2
            ;;
        -j|--jwt-secret)
            JWT_SECRET_KEY="$2"
            shift 2
            ;;
        -a|--admin-email)
            SEED_ADMIN_EMAIL="$2"
            shift 2
            ;;
        -m|--alert-email)
            ALERT_EMAIL="$2"
            shift 2
            ;;
        -s|--skip-login)
            SKIP_LOGIN=true
            shift
            ;;
        -h|--help)
            echo "Usage: ./provision-backend.sh [options]"
            echo ""
            echo "Options:"
            echo "  -e, --environment   Environment (staging, prod). Default: staging"
            echo "  -l, --location      Azure region. Default: westeurope"
            echo "  -g, --resource-group Override resource group name"
            echo "  -p, --password      PostgreSQL admin password"
            echo "  -j, --jwt-secret    JWT secret key (min 32 chars)"
            echo "  -a, --admin-email   Admin email for database seeding (optional)"
            echo "  -m, --alert-email   Admin email for monitoring alerts (optional)"
            echo "  -s, --skip-login    Skip Azure login check"
            echo "  -h, --help          Show this help message"
            exit 0
            ;;
        *)
            echo "Unknown option: $1"
            exit 1
            ;;
    esac
done

# Validate environment
if [[ ! "$ENVIRONMENT" =~ ^(staging|prod)$ ]]; then
    print_error "Invalid environment. Must be: staging or prod"
    exit 1
fi

# Set default resource group if not provided
if [ -z "$RESOURCE_GROUP" ]; then
    RESOURCE_GROUP="myleague-${ENVIRONMENT}-rg"
fi

# Banner
echo -e "${MAGENTA}"
echo "================================================================"
echo "     MyLeague Backend Infrastructure Provisioning"
echo "================================================================"
echo -e "${NC}"

echo "Environment: $ENVIRONMENT"
echo "Location:    $LOCATION"
echo ""

# ============================================================================
# Prerequisites Check
# ============================================================================

print_step "Checking prerequisites..."

# Check Azure CLI
if ! command -v az &> /dev/null; then
    print_error "Azure CLI is not installed. Please install from: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
    exit 1
fi
print_success "Azure CLI installed"

# Check/Install Bicep
if ! az bicep version &> /dev/null; then
    print_warning "Bicep not found, installing..."
    az bicep install
fi
print_success "Bicep CLI available"

# ============================================================================
# Azure Login
# ============================================================================

if [ "$SKIP_LOGIN" = false ]; then
    print_step "Checking Azure login status..."
    
    if ! az account show &> /dev/null; then
        print_warning "Not logged in to Azure. Starting login..."
        az login
    fi
    
    ACCOUNT_NAME=$(az account show --query "name" -o tsv)
    print_success "Logged in to subscription: $ACCOUNT_NAME"
    
    # Confirm subscription
    read -p "Use this subscription? (Y/n): " confirm
    if [[ "$confirm" =~ ^[Nn]$ ]]; then
        echo ""
        echo -e "${YELLOW}Available subscriptions:${NC}"
        az account list --query "[].{Name:name, ID:id, IsDefault:isDefault}" -o table
        
        read -p "Enter subscription ID to use: " SUB_ID
        az account set --subscription "$SUB_ID"
        print_success "Switched to subscription: $SUB_ID"
    fi
fi

# ============================================================================
# Get PostgreSQL Password
# ============================================================================

print_step "Configuring deployment parameters..."

if [ -z "$POSTGRES_PASSWORD" ]; then
    echo ""
    echo -e "${YELLOW}PostgreSQL Admin Password Requirements:${NC}"
    echo "  - Minimum 8 characters"
    echo "  - Must contain uppercase, lowercase, numbers"
    echo "  - Avoid special characters that need escaping"
    echo ""
    
    read -sp "Enter PostgreSQL admin password: " POSTGRES_PASSWORD
    echo ""
    
    if [ ${#POSTGRES_PASSWORD} -lt 8 ]; then
        print_error "Password must be at least 8 characters"
        exit 1
    fi
fi

# Get JWT Secret Key
if [ -z "$JWT_SECRET_KEY" ]; then
    echo ""
    echo -e "${YELLOW}JWT Secret Key Requirements:${NC}"
    echo "  - Minimum 32 characters"
    echo "  - Used for HMAC-SHA256 token signing"
    echo "  - Must be kept secret and unique per environment"
    echo ""
    
    read -sp "Enter JWT secret key: " JWT_SECRET_KEY
    echo ""
    
    if [ ${#JWT_SECRET_KEY} -lt 32 ]; then
        print_error "JWT secret key must be at least 32 characters"
        exit 1
    fi
fi

# Get Seed Admin Email (optional)
if [ -z "$SEED_ADMIN_EMAIL" ]; then
    echo ""
    echo -e "${YELLOW}Admin Seed Email (optional):${NC}"
    echo "  - If set, an admin user with this email is created on first startup"
    echo "  - Leave empty to skip"
    echo ""
    
    read -p "Enter admin email (or press Enter to skip): " SEED_ADMIN_EMAIL
fi

# Get Alert Email (optional but recommended)
if [ -z "$ALERT_EMAIL" ]; then
    echo ""
    echo -e "${YELLOW}Monitoring Alert Email (recommended):${NC}"
    echo "  - Receives automatic alerts: app down, HTTP 5xx spikes, slow responses,"
    echo "    CPU/memory pressure, database issues, and cost budget warnings"
    echo "  - Leave empty to skip deploying monitoring alerts"
    echo ""

    read -p "Enter alert email (or press Enter to skip): " ALERT_EMAIL
fi

print_success "Parameters configured"

# ============================================================================
# Create Resource Group
# ============================================================================

print_step "Creating resource group '$RESOURCE_GROUP' in '$LOCATION'..."

if az group exists --name "$RESOURCE_GROUP" | grep -q "true"; then
    print_warning "Resource group already exists"
else
    az group create --name "$RESOURCE_GROUP" --location "$LOCATION" --output none
    print_success "Resource group created"
fi

# ============================================================================
# Provision Infrastructure
# ============================================================================

print_step "Provisioning infrastructure (this may take 5-10 minutes)..."

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TEMPLATE_FILE="$SCRIPT_DIR/backend.bicep"
PARAMETERS_FILE="$SCRIPT_DIR/backend.${ENVIRONMENT}.bicepparam"
DEPLOYMENT_NAME="myleague-${ENVIRONMENT}-$(date +%Y%m%d-%H%M%S)"

if [ ! -f "$PARAMETERS_FILE" ]; then
    print_error "Parameter file not found: $PARAMETERS_FILE"
    exit 1
fi

# Validate template
echo "  Validating template..."
az deployment group validate \
    --resource-group "$RESOURCE_GROUP" \
    --template-file "$TEMPLATE_FILE" \
    --parameters "$PARAMETERS_FILE" \
    --parameters postgresAdminPassword="$POSTGRES_PASSWORD" \
    --parameters jwtSecretKey="$JWT_SECRET_KEY" \
    --parameters seedAdminEmail="$SEED_ADMIN_EMAIL" \
    --parameters alertEmail="$ALERT_EMAIL" \
    --parameters location="$LOCATION" \
    --parameters environmentName="$ENVIRONMENT" \
    --output none

echo "  Template validated"

# Deploy
echo "  Deploying resources..."
az deployment group create \
    --resource-group "$RESOURCE_GROUP" \
    --template-file "$TEMPLATE_FILE" \
    --parameters "$PARAMETERS_FILE" \
    --parameters postgresAdminPassword="$POSTGRES_PASSWORD" \
    --parameters jwtSecretKey="$JWT_SECRET_KEY" \
    --parameters seedAdminEmail="$SEED_ADMIN_EMAIL" \
    --parameters alertEmail="$ALERT_EMAIL" \
    --parameters location="$LOCATION" \
    --parameters environmentName="$ENVIRONMENT" \
    --name "$DEPLOYMENT_NAME" \
    --output none

print_success "Infrastructure provisioned successfully!"

# ============================================================================
# Display Outputs
# ============================================================================

print_step "Deployment Outputs"

LATEST_DEPLOYMENT=$(az deployment group list --resource-group "$RESOURCE_GROUP" --query "[0].name" -o tsv)

API_URL=$(az deployment group show --resource-group "$RESOURCE_GROUP" --name "$LATEST_DEPLOYMENT" --query "properties.outputs.apiUrl.value" -o tsv)
APP_SERVICE_NAME=$(az deployment group show --resource-group "$RESOURCE_GROUP" --name "$LATEST_DEPLOYMENT" --query "properties.outputs.appServiceName.value" -o tsv)
POSTGRES_SERVER=$(az deployment group show --resource-group "$RESOURCE_GROUP" --name "$LATEST_DEPLOYMENT" --query "properties.outputs.postgresServerName.value" -o tsv)
POSTGRES_FQDN=$(az deployment group show --resource-group "$RESOURCE_GROUP" --name "$LATEST_DEPLOYMENT" --query "properties.outputs.postgresServerFqdn.value" -o tsv)
DATABASE_NAME=$(az deployment group show --resource-group "$RESOURCE_GROUP" --name "$LATEST_DEPLOYMENT" --query "properties.outputs.databaseName.value" -o tsv)
COMM_SERVICE_NAME=$(az deployment group show --resource-group "$RESOURCE_GROUP" --name "$LATEST_DEPLOYMENT" --query "properties.outputs.communicationServiceName.value" -o tsv)
ACS_SENDER=$(az deployment group show --resource-group "$RESOURCE_GROUP" --name "$LATEST_DEPLOYMENT" --query "properties.outputs.acsSenderAddress.value" -o tsv)

echo ""
echo -e "${GREEN}================================================================${NC}"
echo -e "${GREEN}  Infrastructure Provisioned!${NC}"
echo -e "${GREEN}================================================================${NC}"
echo -e "${GREEN}  API URL:        $API_URL${NC}"
echo -e "${GREEN}  App Service:    $APP_SERVICE_NAME${NC}"
echo -e "${GREEN}  PostgreSQL:     $POSTGRES_SERVER${NC}"
echo -e "${GREEN}  Database:       $DATABASE_NAME${NC}"
echo -e "${GREEN}  Comm Service:   $COMM_SERVICE_NAME${NC}"
echo -e "${GREEN}  Email Sender:   $ACS_SENDER${NC}"
echo -e "${GREEN}================================================================${NC}"

echo -e "${CYAN}"
cat << EOF

================================================================
                       Next Steps
================================================================

1. Deploy your application:
   cd src/backend/WebAPI
   dotnet publish -c Release -o ./publish
   zip -r app.zip ./publish/*
   az webapp deploy --resource-group $RESOURCE_GROUP --name $APP_SERVICE_NAME --src-path ./app.zip --type zip

2. Run database migrations:
   export ConnectionStrings__DefaultConnection="Host=$POSTGRES_FQDN;Database=myleague;Username=myleagueadmin;Password=<your-password>;SSL Mode=Require;Trust Server Certificate=true"
   dotnet ef database update --project ../Infrastructure/Infrastructure.csproj

3. View logs:
   az webapp log tail --resource-group $RESOURCE_GROUP --name $APP_SERVICE_NAME

Note: Azure Communication Services Email and JWT authentication have
been automatically configured by this provisioning script.
The ACS sender address is: $ACS_SENDER

EOF
echo -e "${NC}"

echo "Provisioning completed at $(date '+%Y-%m-%d %H:%M:%S')"
