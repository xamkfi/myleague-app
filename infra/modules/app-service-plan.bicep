@description('The name of the App Service Plan')
param name string

@description('The Azure region for the App Service Plan')
param location string = resourceGroup().location

@description('The SKU name for the App Service Plan')
@allowed([
  'F1'
  'B1'
  'B2'
  'B3'
  'S1'
  'S2'
  'S3'
])
param skuName string = 'B1'

@description('Tags to apply to the App Service Plan')
param tags object = {}

resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: name
  location: location
  tags: tags
  kind: 'linux'
  sku: {
    name: skuName
    tier: skuName == 'F1' ? 'Free' : (skuName == 'B1' || skuName == 'B2' || skuName == 'B3' ? 'Basic' : 'Standard')
  }
  properties: {
    reserved: true // Required for Linux
  }
}

@description('The resource ID of the App Service Plan')
output id string = appServicePlan.id

@description('The name of the App Service Plan')
output name string = appServicePlan.name
