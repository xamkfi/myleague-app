// ============================================================================
// MyLeague App Insights ONLY Deployment
// ============================================================================
// Narrow Bicep entrypoint that provisions just the Log Analytics workspace +
// workspace-based Application Insights component. Use this when you want to
// add observability to an environment without touching PostgreSQL, App Service
// SKU, JWT, ACS or any other existing resource.
//
// After this deployment, set APPLICATIONINSIGHTS_CONNECTION_STRING on the
// App Service app settings (see az command below or the connectionString
// output of this deployment).
// ============================================================================

targetScope = 'resourceGroup'

@description('The environment name (dev, staging, prod) - used for resource naming')
@allowed([
  'dev'
  'staging'
  'prod'
])
param environmentName string = 'dev'

@description('The Azure region for the resources')
param location string = resourceGroup().location

@description('The base name for resource naming (matches backend.bicep convention)')
param baseName string = 'myleague'

@description('Retention in days for the Log Analytics workspace (30..730)')
@minValue(30)
@maxValue(730)
param retentionInDays int = 30

@description('Daily ingestion cap in GB for the Log Analytics workspace. Use -1 to disable the cap.')
param dailyQuotaGb int = 1

var resourcePrefix = '${baseName}-${environmentName}'
var appInsightsName = '${resourcePrefix}-ai'
var logAnalyticsWorkspaceName = '${resourcePrefix}-logs'

var tags = {
  Environment: environmentName
  Application: baseName
  ManagedBy: 'Bicep'
  Component: 'observability'
}

module applicationInsights 'modules/application-insights.bicep' = {
  name: 'applicationInsights'
  params: {
    name: appInsightsName
    workspaceName: logAnalyticsWorkspaceName
    location: location
    retentionInDays: retentionInDays
    dailyQuotaGb: dailyQuotaGb
    tags: tags
  }
}

@description('The Application Insights connection string')
output connectionString string = applicationInsights.outputs.connectionString

@description('The name of the Application Insights component')
output appInsightsName string = applicationInsights.outputs.name

@description('The name of the Log Analytics workspace')
output logAnalyticsWorkspaceName string = applicationInsights.outputs.workspaceName
