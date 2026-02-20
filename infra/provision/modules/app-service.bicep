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

// Authentication parameters
@description('The JWT secret key for signing tokens (must be at least 32 characters)')
@secure()
param jwtSecretKey string

@description('The JWT issuer')
param jwtIssuer string = 'MyLeague'

@description('The JWT audience')
param jwtAudience string = 'MyLeague'

@description('The Azure Communication Services connection string')
@secure()
param acsConnectionString string = ''

@description('The Azure Communication Services sender email address')
param acsSenderAddress string = ''

@description('The admin email for database seeding')
param seedAdminEmail string = ''

@description('The base URL of the frontend application (e.g. https://myleague.fi)')
param frontendBaseUrl string = ''

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
        // Authentication settings
        {
          name: 'Jwt__SecretKey'
          value: jwtSecretKey
        }
        {
          name: 'Jwt__Issuer'
          value: jwtIssuer
        }
        {
          name: 'Jwt__Audience'
          value: jwtAudience
        }
        {
          name: 'AzureCommunicationServices__ConnectionString'
          value: acsConnectionString
        }
        {
          name: 'AzureCommunicationServices__SenderAddress'
          value: acsSenderAddress
        }
        {
          name: 'Seed__AdminEmail'
          value: seedAdminEmail
        }
        {
          name: 'Frontend__BaseUrl'
          value: frontendBaseUrl
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
