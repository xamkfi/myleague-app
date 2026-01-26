#!/bin/bash

# ============================================================================
# MyLeague Azure Infrastructure Deployment Script
# ============================================================================
# Usage: ./deploy.sh [options]
#
# Options:
#   -e, --environment   Environment (dev, staging, prod). Default: dev
#   -l, --location      Azure region. Default: westeurope
#   -g, --resource-group Override resource group name
#   -p, --password      PostgreSQL admin password (will prompt if not provided)
#   -s, --skip-login    Skip Azure login check
#   -h, --help          Show this help message
# ============================================================================

set -e

# Default values
ENVIRONMENT="dev"
LOCATION="westeurope"
RESOURCE_GROUP=""
POSTGRES_PASSWORD=""
SKIP_LOGIN=false

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
MAGENTA='\033[0;35m'
NC='\033[0m' # No Color

# Helper functions
print_step() { echo -e "\n${CYAN}▶ $1${NC}"; }
print_success() { echo -e "${GREEN}✓ $1${NC}"; }
print_warning() { echo -e "${YELLOW}⚠ $1${NC}"; }
print_error() { echo -e "${RED}✗ $1${NC}"; }

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
        -s|--skip-login)
            SKIP_LOGIN=true
            shift
            ;;
        -h|--help)
            echo "Usage: ./deploy.sh [options]"
            echo ""
            echo "Options:"
            echo "  -e, --environment   Environment (dev, staging, prod). Default: dev"
            echo "  -l, --location      Azure region. Default: westeurope"
            echo "  -g, --resource-group Override resource group name"
            echo "  -p, --password      PostgreSQL admin password"
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
if [[ ! "$ENVIRONMENT" =~ ^(dev|staging|prod)$ ]]; then
    print_error "Invalid environment. Must be: dev, staging, or prod"
    exit 1
fi

# Set default resource group if not provided
if [ -z "$RESOURCE_GROUP" ]; then
    RESOURCE_GROUP="myleague-${ENVIRONMENT}-rg"
fi

# Banner
echo -e "${MAGENTA}"
echo "╔══════════════════════════════════════════════════════════════╗"
echo "║           MyLeague Azure Infrastructure Deployment           ║"
echo "╚══════════════════════════════════════════════════════════════╝"
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
    ACCOUNT_ID=$(az account show --query "id" -o tsv)
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
# Deploy Infrastructure
# ============================================================================

print_step "Deploying infrastructure (this may take 5-10 minutes)..."

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TEMPLATE_FILE="$SCRIPT_DIR/main.bicep"
PARAMETERS_FILE="$SCRIPT_DIR/main.bicepparam"
DEPLOYMENT_NAME="myleague-${ENVIRONMENT}-$(date +%Y%m%d-%H%M%S)"

# Validate template
echo "  Validating template..."
az deployment group validate \
    --resource-group "$RESOURCE_GROUP" \
    --template-file "$TEMPLATE_FILE" \
    --parameters "$PARAMETERS_FILE" \
    --parameters postgresAdminPassword="$POSTGRES_PASSWORD" \
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
    --parameters location="$LOCATION" \
    --parameters environmentName="$ENVIRONMENT" \
    --name "$DEPLOYMENT_NAME" \
    --output none

print_success "Infrastructure deployed successfully!"

# ============================================================================
# Display Outputs
# ============================================================================

print_step "Deployment Outputs"

# Get the latest deployment name
LATEST_DEPLOYMENT=$(az deployment group list --resource-group "$RESOURCE_GROUP" --query "[0].name" -o tsv)

# Get outputs
API_URL=$(az deployment group show --resource-group "$RESOURCE_GROUP" --name "$LATEST_DEPLOYMENT" --query "properties.outputs.apiUrl.value" -o tsv)
APP_SERVICE_NAME=$(az deployment group show --resource-group "$RESOURCE_GROUP" --name "$LATEST_DEPLOYMENT" --query "properties.outputs.appServiceName.value" -o tsv)
POSTGRES_SERVER=$(az deployment group show --resource-group "$RESOURCE_GROUP" --name "$LATEST_DEPLOYMENT" --query "properties.outputs.postgresServerName.value" -o tsv)
POSTGRES_FQDN=$(az deployment group show --resource-group "$RESOURCE_GROUP" --name "$LATEST_DEPLOYMENT" --query "properties.outputs.postgresServerFqdn.value" -o tsv)
DATABASE_NAME=$(az deployment group show --resource-group "$RESOURCE_GROUP" --name "$LATEST_DEPLOYMENT" --query "properties.outputs.databaseName.value" -o tsv)

echo ""
echo -e "${GREEN}┌─────────────────────────────────────────────────────────────────┐${NC}"
echo -e "${GREEN}│  Deployment Complete!                                           │${NC}"
echo -e "${GREEN}├─────────────────────────────────────────────────────────────────┤${NC}"
echo -e "${GREEN}│                                                                 │${NC}"
echo -e "${GREEN}│  API URL:        $API_URL${NC}"
echo -e "${GREEN}│  App Service:    $APP_SERVICE_NAME${NC}"
echo -e "${GREEN}│  PostgreSQL:     $POSTGRES_SERVER${NC}"
echo -e "${GREEN}│  Database:       $DATABASE_NAME${NC}"
echo -e "${GREEN}│                                                                 │${NC}"
echo -e "${GREEN}└─────────────────────────────────────────────────────────────────┘${NC}"

# ============================================================================
# Next Steps
# ============================================================================

echo -e "${CYAN}"
cat << EOF

╔══════════════════════════════════════════════════════════════╗
║                         Next Steps                           ║
╚══════════════════════════════════════════════════════════════╝

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

4. Check health:
   curl $API_URL/health/ready

EOF
echo -e "${NC}"

echo "Deployment completed at $(date '+%Y-%m-%d %H:%M:%S')"
