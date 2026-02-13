// ============================================================================
// MyLeague Frontend Infrastructure - Static Web App Deployment
// ============================================================================
// This template deploys:
// - Azure Static Web App (Free tier) for React frontend
// ============================================================================

targetScope = 'resourceGroup'

// ============================================================================
// Parameters
// ============================================================================

@description('The environment name (dev, staging, prod)')
@allowed([
  'dev'
  'staging'
  'prod'
])
param environmentName string = 'dev'

@description('The Azure region for the Static Web App')
// Note: Static Web Apps have limited region availability
@allowed([
  'westeurope'
  'centralus'
  'eastus2'
  'westus2'
  'eastasia'
  'westus'
])
param location string = 'westeurope'

@description('The base name for all resources')
param baseName string = 'myleague'

@description('The URL of the backend API')
param apiBackendUrl string = ''

@description('The SKU for the Static Web App')
@allowed([
  'Free'
  'Standard'
])
param sku string = 'Free'

// ============================================================================
// Variables
// ============================================================================

var resourcePrefix = '${baseName}-${environmentName}'
var staticWebAppName = '${resourcePrefix}-web'

var tags = {
  Environment: environmentName
  Application: baseName
  Component: 'Frontend'
  ManagedBy: 'Bicep'
}

// ============================================================================
// Modules
// ============================================================================

// Static Web App for React Frontend
module staticWebApp 'modules/static-web-app.bicep' = {
  name: 'staticWebApp'
  params: {
    name: staticWebAppName
    location: location
    sku: sku
    apiBackendUrl: apiBackendUrl
    tags: tags
  }
}

// ============================================================================
// Outputs
// ============================================================================

@description('The URL of the deployed frontend')
output frontendUrl string = staticWebApp.outputs.url

@description('The hostname of the deployed frontend')
output frontendHostname string = staticWebApp.outputs.hostname

@description('The name of the Static Web App')
output staticWebAppName string = staticWebApp.outputs.name

@description('The deployment token for CI/CD (use with swa deploy)')
#disable-next-line outputs-should-not-contain-secrets
output deploymentToken string = staticWebApp.outputs.deploymentToken
