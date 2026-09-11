@description('The name of the Application Insights component')
param name string

@description('The name of the Log Analytics workspace that backs Application Insights')
param workspaceName string

@description('The Azure region for both resources')
param location string = resourceGroup().location

@description('Retention in days for the Log Analytics workspace (30..730)')
@minValue(30)
@maxValue(730)
param retentionInDays int = 30

@description('Daily ingestion cap in GB for the Log Analytics workspace. Use -1 to disable the cap.')
param dailyQuotaGb int = 1

@description('Tags to apply to both resources')
param tags object = {}

resource workspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: workspaceName
  location: location
  tags: tags
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: retentionInDays
    workspaceCapping: {
      dailyQuotaGb: dailyQuotaGb
    }
    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
    }
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: name
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: workspace.id
    IngestionMode: 'LogAnalytics'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

@description('The Application Insights connection string (use APPLICATIONINSIGHTS_CONNECTION_STRING app setting)')
output connectionString string = appInsights.properties.ConnectionString

@description('The Application Insights instrumentation key (legacy; prefer the connection string)')
output instrumentationKey string = appInsights.properties.InstrumentationKey

@description('The resource ID of the Application Insights component')
output id string = appInsights.id

@description('The name of the Application Insights component')
output name string = appInsights.name

@description('The resource ID of the Log Analytics workspace')
output workspaceId string = workspace.id

@description('The name of the Log Analytics workspace')
output workspaceName string = workspace.name
