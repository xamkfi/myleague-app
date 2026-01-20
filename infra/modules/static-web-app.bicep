@description('The name of the Static Web App')
param name string

@description('The Azure region for the Static Web App')
param location string = resourceGroup().location

@description('The SKU for the Static Web App')
@allowed([
  'Free'
  'Standard'
])
param sku string = 'Free'

@description('The URL of the backend API')
param apiBackendUrl string = ''

@description('Tags to apply to the Static Web App')
param tags object = {}

resource staticWebApp 'Microsoft.Web/staticSites@2023-12-01' = {
  name: name
  location: location
  tags: tags
  sku: {
    name: sku
    tier: sku
  }
  properties: {
    stagingEnvironmentPolicy: 'Enabled'
    allowConfigFileUpdates: true
    buildProperties: {
      appLocation: '/'
      apiLocation: ''
      outputLocation: 'dist'
      appBuildCommand: 'pnpm run build'
    }
  }
}

// Configure app settings for the Static Web App
resource staticWebAppSettings 'Microsoft.Web/staticSites/config@2023-12-01' = {
  parent: staticWebApp
  name: 'appsettings'
  properties: {
    VITE_API_URL: apiBackendUrl
  }
}

@description('The default hostname of the Static Web App')
output hostname string = staticWebApp.properties.defaultHostname

@description('The URL of the Static Web App')
output url string = 'https://${staticWebApp.properties.defaultHostname}'

@description('The resource ID of the Static Web App')
output id string = staticWebApp.id

@description('The name of the Static Web App')
output name string = staticWebApp.name

@description('The deployment token for CI/CD')
output deploymentToken string = staticWebApp.listSecrets().properties.apiKey
