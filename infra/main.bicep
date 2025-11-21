// Main Bicep Template for MyLeague Application
// Deploys complete infrastructure: PostgreSQL, App Service, Static Web App, and Application Insights

targetScope = 'resourceGroup'

// ==================== Parameters ====================

@description('Environment name (dev, staging, prod)')
@allowed([
  'dev'
  'staging'
  'prod'
])
param environment string = 'dev'

@description('Project name used for resource naming')
param projectName string = 'myleague-bicep'

@description('Azure region for resources')
param location string = resourceGroup().location

@description('Azure region for Static Web App (limited availability)')
param staticWebAppLocation string = 'westeurope'

// PostgreSQL Parameters
@description('PostgreSQL administrator username')
@secure()
param postgresAdminUsername string

@description('PostgreSQL administrator password')
@secure()
param postgresAdminPassword string

@description('PostgreSQL database name')
param databaseName string = 'myleague'

// App Service Parameters
@description('App Service SKU')
@allowed([
  'B1'
  'B2'
  'S1'
  'S2'
  'P1v2'
])
param appServiceSku string = 'B1'

// Static Web App Parameters
@description('Static Web App SKU')
@allowed([
  'Free'
  'Standard'
])
param staticWebAppSku string = 'Free'

// ==================== Variables ====================

var resourceSuffix = '${projectName}-${environment}'
var postgresServerName = 'psql-${resourceSuffix}'
var appServicePlanName = 'asp-${resourceSuffix}'
var appServiceName = 'app-${resourceSuffix}'
var staticWebAppName = 'swa-${resourceSuffix}'
var appInsightsName = 'appi-${resourceSuffix}'
var logAnalyticsName = 'log-${resourceSuffix}'

// ==================== Resources ====================

// Log Analytics Workspace (required for Application Insights)
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: logAnalyticsName
  location: location
  tags: {
    environment: environment
    project: projectName
  }
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
    }
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

// Application Insights
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  tags: {
    environment: environment
    project: projectName
  }
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
    Flow_Type: 'Bluefield'
    Request_Source: 'rest'
    RetentionInDays: 30
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

// PostgreSQL Flexible Server
module postgresql 'modules/postgresql.bicep' = {
  name: 'postgresql-deployment'
  params: {
    serverName: postgresServerName
    location: location
    administratorLogin: postgresAdminUsername
    administratorPassword: postgresAdminPassword
    postgresVersion: '16'
    skuTier: 'Burstable'
    skuName: 'Standard_B1ms'
    storageSizeGB: 32
    backupRetentionDays: 7
    environment: environment
    databaseName: databaseName
  }
}

// App Service (Backend API)
module appService 'modules/appservice.bicep' = {
  name: 'appservice-deployment'
  params: {
    appServiceName: appServiceName
    appServicePlanName: appServicePlanName
    location: location
    skuName: appServiceSku
    postgresConnectionString: postgresql.outputs.connectionString
    appInsightsInstrumentationKey: appInsights.properties.InstrumentationKey
    appInsightsConnectionString: appInsights.properties.ConnectionString
    frontendUrl: 'https://${staticWebAppName}.azurestaticapps.net'
    environment: environment
  }
  dependsOn: [
    postgresql
    appInsights
  ]
}

// Static Web App (Frontend)
module staticWebApp 'modules/staticwebapp.bicep' = {
  name: 'staticwebapp-deployment'
  params: {
    staticWebAppName: staticWebAppName
    location: staticWebAppLocation
    skuName: staticWebAppSku
    backendApiUrl: appService.outputs.appServiceUrl
    environment: environment
  }
  dependsOn: [
    appService
  ]
}

// ==================== Outputs ====================

@description('PostgreSQL server FQDN')
output postgresServerFqdn string = postgresql.outputs.serverFqdn

@description('PostgreSQL database name')
output postgresDatabaseName string = postgresql.outputs.databaseName

@description('App Service URL')
output appServiceUrl string = appService.outputs.appServiceUrl

@description('App Service hostname')
output appServiceHostname string = appService.outputs.appServiceHostname

@description('Static Web App URL')
output staticWebAppUrl string = staticWebApp.outputs.staticWebAppUrl

@description('Static Web App hostname')
output staticWebAppHostname string = staticWebApp.outputs.staticWebAppHostname

@description('Application Insights Instrumentation Key')
output appInsightsInstrumentationKey string = appInsights.properties.InstrumentationKey

@description('Application Insights Connection String')
output appInsightsConnectionString string = appInsights.properties.ConnectionString

@description('Static Web App Deployment Token')
@secure()
output staticWebAppDeploymentToken string = staticWebApp.outputs.deploymentToken

// Summary output for easy reference
output deploymentSummary object = {
  frontend: {
    url: staticWebApp.outputs.staticWebAppUrl
    name: staticWebApp.outputs.staticWebAppName
  }
  backend: {
    url: appService.outputs.appServiceUrl
    name: appService.outputs.appServiceName
  }
  database: {
    server: postgresql.outputs.serverFqdn
    database: postgresql.outputs.databaseName
  }
  monitoring: {
    appInsightsName: appInsights.name
    logAnalyticsName: logAnalytics.name
  }
}

