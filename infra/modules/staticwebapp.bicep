// Static Web App Module
// Creates Azure Static Web App for React frontend

@description('The name of the Static Web App')
param staticWebAppName string

@description('The location where the Static Web App will be deployed')
param location string = 'westeurope' // Static Web Apps have limited region availability

@description('The SKU of the Static Web App')
@allowed([
  'Free'
  'Standard'
])
param skuName string = 'Free'

@description('Backend API URL (App Service URL)')
param backendApiUrl string

@description('Environment tag')
param environment string = 'dev'

// Static Web App
resource staticWebApp 'Microsoft.Web/staticSites@2023-01-01' = {
  name: staticWebAppName
  location: location
  tags: {
    environment: environment
    project: 'myleague'
  }
  sku: {
    name: skuName
    tier: skuName
  }
  properties: {
    repositoryUrl: '' // Leave empty for manual deployment, configure in Azure Portal or GitHub Actions later
    branch: '' // Leave empty for manual deployment
    buildProperties: {
      appLocation: '/src/frontend' // Path to frontend code
      apiLocation: '' // Not using SWA managed functions
      outputLocation: 'dist' // Vite build output directory
    }
    stagingEnvironmentPolicy: 'Enabled'
    allowConfigFileUpdates: true
    enterpriseGradeCdnStatus: 'Disabled'
  }
}

// Configure backend API integration via staticwebapp.config.json
// Note: This file should be created in the frontend src/public folder
// Example content:
// {
//   "routes": [
//     {
//       "route": "/api/*",
//       "rewrite": "{backendApiUrl}/api/{*}"
//     }
//   ],
//   "navigationFallback": {
//     "rewrite": "/index.html"
//   }
// }

// Outputs
@description('Static Web App default hostname')
output staticWebAppHostname string = staticWebApp.properties.defaultHostname

@description('Static Web App URL')
output staticWebAppUrl string = 'https://${staticWebApp.properties.defaultHostname}'

@description('Static Web App name')
output staticWebAppName string = staticWebApp.name

@description('Static Web App deployment token (use for CI/CD)')
output deploymentToken string = staticWebApp.listSecrets().properties.apiKey

