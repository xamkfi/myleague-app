// App Service Module
// Creates App Service Plan and App Service for .NET 9 backend API

@description('The name of the App Service')
param appServiceName string

@description('The name of the App Service Plan')
param appServicePlanName string

@description('The location where resources will be deployed')
param location string = resourceGroup().location

@description('The SKU of the App Service Plan')
@allowed([
  'B1'
  'B2'
  'B3'
  'S1'
  'S2'
  'S3'
  'P1v2'
  'P2v2'
  'P3v2'
])
param skuName string = 'B1'

@description('PostgreSQL connection string')
@secure()
param postgresConnectionString string

@description('Application Insights Instrumentation Key')
@secure()
param appInsightsInstrumentationKey string

@description('Application Insights Connection String')
@secure()
param appInsightsConnectionString string

@description('Frontend URL for CORS')
param frontendUrl string

@description('Environment tag')
param environment string = 'dev'

// App Service Plan (Linux)
resource appServicePlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: appServicePlanName
  location: location
  tags: {
    environment: environment
    project: 'myleague'
  }
  kind: 'linux'
  sku: {
    name: skuName
  }
  properties: {
    reserved: true // Required for Linux
  }
}

// App Service
resource appService 'Microsoft.Web/sites@2023-01-01' = {
  name: appServiceName
  location: location
  tags: {
    environment: environment
    project: 'myleague'
  }
  kind: 'app,linux'
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|9.0'
      alwaysOn: false // Set to false for B1 tier (Basic doesn't support AlwaysOn)
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      http20Enabled: true
      healthCheckPath: '/health'
      cors: {
        allowedOrigins: [
          frontendUrl
          'https://*.azurestaticapps.net' // Allow all Static Web App URLs
        ]
        supportCredentials: true
      }
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: environment == 'dev' ? 'Development' : 'Production'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsightsConnectionString
        }
        {
          name: 'ApplicationInsightsAgent_EXTENSION_VERSION'
          value: '~3'
        }
        {
          name: 'XDT_MicrosoftApplicationInsights_Mode'
          value: 'recommended'
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }
      ]
      connectionStrings: [
        {
          name: 'DefaultConnection'
          connectionString: postgresConnectionString
          type: 'PostgreSQL'
        }
      ]
    }
  }
}

// Outputs
@description('App Service hostname')
output appServiceHostname string = appService.properties.defaultHostName

@description('App Service URL')
output appServiceUrl string = 'https://${appService.properties.defaultHostName}'

@description('App Service name')
output appServiceName string = appService.name

@description('App Service Plan ID')
output appServicePlanId string = appServicePlan.id

