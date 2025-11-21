#!/bin/bash
# MyLeague Azure Infrastructure Deployment Script
# Bash script to deploy Azure infrastructure using Bicep

set -e  # Exit on error

# Default values
RESOURCE_GROUP="${RESOURCE_GROUP:-rg-myleague-bicep-dev}"
LOCATION="${LOCATION:-westeurope}"
POSTGRES_USERNAME="${POSTGRES_USERNAME:-myleagueadmin}"
ENVIRONMENT="${ENVIRONMENT:-dev}"
PROJECT_NAME="${PROJECT_NAME:-myleague-bicep}"
VALIDATE_ONLY="${VALIDATE_ONLY:-false}"
SHOW_OUTPUTS="${SHOW_OUTPUTS:-true}"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

echo -e "${CYAN}========================================${NC}"
echo -e "${CYAN}MyLeague Azure Infrastructure Deployment${NC}"
echo -e "${CYAN}========================================${NC}"
echo ""

# Check if Azure CLI is installed
echo -e "${YELLOW}Checking Azure CLI installation...${NC}"
if ! command -v az &> /dev/null; then
    echo -e "${RED}✗ Azure CLI not found. Please install it from: https://aka.ms/installazurecli${NC}"
    exit 1
fi
echo -e "${GREEN}✓ Azure CLI found${NC}"

# Check if logged in
echo -e "${YELLOW}Checking Azure login status...${NC}"
if ! az account show &> /dev/null; then
    echo -e "${YELLOW}Not logged in. Logging in...${NC}"
    az login
    if [ $? -ne 0 ]; then
        echo -e "${RED}✗ Failed to login to Azure${NC}"
        exit 1
    fi
fi
echo -e "${GREEN}✓ Logged in to Azure${NC}"

# Get current subscription
SUBSCRIPTION_NAME=$(az account show --query name -o tsv)
echo -e "${CYAN}Current subscription: $SUBSCRIPTION_NAME${NC}"
echo ""

# Prompt for password if not provided
if [ -z "$POSTGRES_PASSWORD" ]; then
    echo -e "${YELLOW}Enter PostgreSQL administrator password:${NC}"
    echo -e "Must be at least 8 characters with uppercase, lowercase, numbers, and special characters"
    read -s POSTGRES_PASSWORD
    echo ""
fi

# Create resource group
echo -e "${YELLOW}Creating resource group: $RESOURCE_GROUP...${NC}"
if ! az group show --name "$RESOURCE_GROUP" &> /dev/null; then
    az group create --name "$RESOURCE_GROUP" --location "$LOCATION"
    if [ $? -ne 0 ]; then
        echo -e "${RED}✗ Failed to create resource group${NC}"
        exit 1
    fi
    echo -e "${GREEN}✓ Resource group created${NC}"
else
    echo -e "${GREEN}✓ Resource group already exists${NC}"
fi

# Navigate to script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
cd "$SCRIPT_DIR"

# Validate template
echo ""
echo -e "${YELLOW}Validating Bicep template...${NC}"
az deployment group validate \
    --resource-group "$RESOURCE_GROUP" \
    --template-file main.bicep \
    --parameters environment="$ENVIRONMENT" \
                 projectName="$PROJECT_NAME" \
                 location="$LOCATION" \
                 postgresAdminUsername="$POSTGRES_USERNAME" \
                 postgresAdminPassword="$POSTGRES_PASSWORD" \
                 appServiceSku=B1 \
                 staticWebAppSku=Free \
    --output none

if [ $? -ne 0 ]; then
    echo -e "${RED}✗ Template validation failed${NC}"
    exit 1
fi
echo -e "${GREEN}✓ Template validation passed${NC}"

if [ "$VALIDATE_ONLY" = "true" ]; then
    echo ""
    echo -e "${CYAN}Validation complete. Exiting (VALIDATE_ONLY=true).${NC}"
    exit 0
fi

# Deploy infrastructure
echo ""
echo -e "${YELLOW}Deploying infrastructure...${NC}"
echo -e "This will take approximately 10-15 minutes..."
echo ""

DEPLOYMENT_NAME="myleague-infra-deployment-$(date +%Y%m%d-%H%M%S)"

az deployment group create \
    --resource-group "$RESOURCE_GROUP" \
    --template-file main.bicep \
    --name "$DEPLOYMENT_NAME" \
    --parameters environment="$ENVIRONMENT" \
                 projectName="$PROJECT_NAME" \
                 location="$LOCATION" \
                 postgresAdminUsername="$POSTGRES_USERNAME" \
                 postgresAdminPassword="$POSTGRES_PASSWORD" \
                 appServiceSku=B1 \
                 staticWebAppSku=Free

if [ $? -ne 0 ]; then
    echo ""
    echo -e "${RED}✗ Deployment failed${NC}"
    echo -e "${YELLOW}Check the error details above or run:${NC}"
    echo -e "  az deployment group show --resource-group $RESOURCE_GROUP --name $DEPLOYMENT_NAME --query properties.error"
    exit 1
fi

echo ""
echo -e "${GREEN}✓ Deployment completed successfully!${NC}"
echo ""

# Show outputs
if [ "$SHOW_OUTPUTS" = "true" ]; then
    echo -e "${CYAN}========================================${NC}"
    echo -e "${CYAN}Deployment Outputs${NC}"
    echo -e "${CYAN}========================================${NC}"
    echo ""
    
    BACKEND_URL=$(az deployment group show --resource-group "$RESOURCE_GROUP" --name "$DEPLOYMENT_NAME" --query properties.outputs.appServiceUrl.value -o tsv)
    FRONTEND_URL=$(az deployment group show --resource-group "$RESOURCE_GROUP" --name "$DEPLOYMENT_NAME" --query properties.outputs.staticWebAppUrl.value -o tsv)
    POSTGRES_SERVER=$(az deployment group show --resource-group "$RESOURCE_GROUP" --name "$DEPLOYMENT_NAME" --query properties.outputs.postgresServerFqdn.value -o tsv)
    DATABASE_NAME=$(az deployment group show --resource-group "$RESOURCE_GROUP" --name "$DEPLOYMENT_NAME" --query properties.outputs.postgresDatabaseName.value -o tsv)
    SWA_TOKEN=$(az deployment group show --resource-group "$RESOURCE_GROUP" --name "$DEPLOYMENT_NAME" --query properties.outputs.staticWebAppDeploymentToken.value -o tsv)
    
    echo -e "${YELLOW}Backend (App Service):${NC}"
    echo -e "  URL: ${BACKEND_URL}"
    echo ""
    
    echo -e "${YELLOW}Frontend (Static Web App):${NC}"
    echo -e "  URL: ${FRONTEND_URL}"
    echo ""
    
    echo -e "${YELLOW}Database (PostgreSQL):${NC}"
    echo -e "  Server: ${POSTGRES_SERVER}"
    echo -e "  Database: ${DATABASE_NAME}"
    echo ""
    
    echo -e "${YELLOW}Application Insights:${NC}"
    echo -e "  Name: appi-${PROJECT_NAME}-${ENVIRONMENT}"
    echo ""
    
    echo -e "${YELLOW}Static Web App Deployment Token:${NC}"
    echo -e "  ${SWA_TOKEN}"
    echo ""
    
    # Save to file
    OUTPUT_FILE="deployment-outputs-${DEPLOYMENT_NAME}.json"
    az deployment group show --resource-group "$RESOURCE_GROUP" --name "$DEPLOYMENT_NAME" --query properties.outputs -o json > "$OUTPUT_FILE"
    echo -e "${CYAN}Full outputs saved to: $OUTPUT_FILE${NC}"
    echo ""
fi

echo -e "${CYAN}========================================${NC}"
echo -e "${CYAN}Next Steps:${NC}"
echo -e "${CYAN}========================================${NC}"
echo -e "1. Deploy your backend application to App Service"
echo -e "2. Deploy your frontend application to Static Web App"
echo -e "3. Run database migrations"
echo -e "4. Configure frontend environment variables"
echo ""
echo -e "${CYAN}View resources in Azure Portal:${NC}"
echo -e "  az portal --resource-group $RESOURCE_GROUP"
echo ""

