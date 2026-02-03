// ============================================================================
// Azure Storage Account Module
// ============================================================================
// Creates a Storage Account with a blob container for image uploads.
// Uses connection string authentication (storage account key).
// ============================================================================

@description('The name of the Storage Account (must be globally unique, 3-24 lowercase letters/numbers)')
param name string

@description('The Azure region for the Storage Account')
param location string = resourceGroup().location

@description('The name of the blob container for images')
param containerName string = 'images'

@description('The SKU for the Storage Account')
@allowed([
  'Standard_LRS'
  'Standard_GRS'
  'Standard_ZRS'
])
param skuName string = 'Standard_LRS'

@description('Tags to apply to the Storage Account')
param tags object = {}

// ============================================================================
// Resources
// ============================================================================

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: name
  location: location
  tags: tags
  sku: {
    name: skuName
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: true // Required for public image URLs
    publicNetworkAccess: 'Enabled'
  }
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  parent: storageAccount
  name: 'default'
  properties: {
    deleteRetentionPolicy: {
      enabled: true
      days: 7
    }
  }
}

resource container 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  parent: blobService
  name: containerName
  properties: {
    publicAccess: 'Blob' // Blobs are publicly readable, container listing is private
  }
}

// ============================================================================
// Outputs
// ============================================================================

@description('The name of the Storage Account')
output name string = storageAccount.name

@description('The resource ID of the Storage Account')
output id string = storageAccount.id

@description('The primary blob endpoint')
output blobEndpoint string = storageAccount.properties.primaryEndpoints.blob

@description('The name of the blob container')
output containerName string = container.name

@description('The connection string for the Storage Account')
output connectionString string = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage}'
