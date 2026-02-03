@description('The name of the App Service')
param name string

@description('The Azure region for the App Service')
param location string = resourceGroup().location

@description('The resource ID of the App Service Plan')
param appServicePlanId string

@description('The PostgreSQL connection string')
@secure()
param postgresConnectionString string

@description('The environment name (Development, Staging, Production)')
@allowed([
  'Development'
  'Staging'
  'Production'
])
param environmentName string = 'Development'

@description('Tags to apply to the App Service')
param tags object = {}

@description('Allowed origins for CORS (comma-separated)')
param allowedOrigins array = []

@description('The connection string for the Storage Account')
@secure()
param storageConnectionString string = ''

@description('The name of the blob container for images')
param storageContainerName string = 'images'

resource appService 'Microsoft.Web/sites@2023-12-01' = {
  name: name
  location: location
  tags: tags
  kind: 'app,linux'
  properties: {
    serverFarmId: appServicePlanId
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|9.0'
      alwaysOn: true
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      http20Enabled: true
      healthCheckPath: '/health/ready'
      cors: {
        allowedOrigins: allowedOrigins
        supportCredentials: true
      }
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: environmentName
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }
        {
          name: 'AzureStorage__ContainerName'
          value: storageContainerName
        }
      ]
      connectionStrings: [
        {
          name: 'DefaultConnection'
          connectionString: postgresConnectionString
          type: 'Custom'
        }
        {
          name: 'AzureBlobStorage'
          connectionString: storageConnectionString
          type: 'Custom'
        }
      ]
    }
  }
}

@description('The default hostname of the App Service')
output hostname string = appService.properties.defaultHostName

@description('The URL of the App Service')
output url string = 'https://${appService.properties.defaultHostName}'

@description('The resource ID of the App Service')
output id string = appService.id

@description('The name of the App Service')
output name string = appService.name
