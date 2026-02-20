// ============================================================================
// MyLeague Backend Infrastructure - Main Deployment
// ============================================================================
// This template deploys:
// - App Service Plan (Basic B1 Linux)
// - App Service for .NET 9 API
// - PostgreSQL Flexible Server (Burstable B1ms)
// - Storage Account for image uploads
// - Azure Communication Services (Email with Azure-managed domain)
// ============================================================================

targetScope = 'resourceGroup'

// ============================================================================
// Parameters
// ============================================================================

@description('The environment name (dev, staging, prod)')
@allowed([
  'dev'
  'staging'
  'prod'
])
param environmentName string = 'dev'

@description('The Azure region for all resources')
param location string = resourceGroup().location

@description('The base name for all resources')
param baseName string = 'myleague'

@description('The PostgreSQL administrator username')
param postgresAdminUser string = 'myleagueadmin'

@description('The PostgreSQL administrator password')
@secure()
param postgresAdminPassword string

@description('The SKU for the App Service Plan')
@allowed([
  'F1'
  'B1'
])
param appServicePlanSku string = 'B1'

@description('The SKU for the PostgreSQL server')
@allowed([
  'Standard_B1ms'
  'Standard_B2s'
])
param postgresSku string = 'Standard_B1ms'

@description('Allowed CORS origins for the API')
param allowedOrigins array = []

@description('The SKU for the Storage Account')
@allowed([
  'Standard_LRS'
  'Standard_GRS'
  'Standard_ZRS'
])
param storageSku string = 'Standard_LRS'

@description('The JWT secret key for signing tokens (must be at least 32 characters)')
@secure()
param jwtSecretKey string

@description('The admin email for database seeding (optional)')
param seedAdminEmail string = ''

@description('The base URL of the frontend application (e.g. https://calm-tree-06b4ac003.2.azurestaticapps.net)')
param frontendBaseUrl string = ''

// ============================================================================
// Variables
// ============================================================================

var resourcePrefix = '${baseName}-${environmentName}'
var appServicePlanName = '${resourcePrefix}-plan'
var appServiceName = '${resourcePrefix}-api'
var postgresServerName = '${resourcePrefix}-postgres'
// Storage account names must be 3-24 lowercase alphanumeric only
var storageAccountName = toLower(replace('${baseName}${environmentName}storage', '-', ''))

var communicationServiceName = '${resourcePrefix}-comm'
var emailServiceName = '${resourcePrefix}-email'

var tags = {
  Environment: environmentName
  Application: baseName
  ManagedBy: 'Bicep'
}

var aspnetEnvironment = environmentName == 'prod' ? 'Production' : (environmentName == 'staging' ? 'Staging' : 'Development')

// ============================================================================
// Modules
// ============================================================================

// App Service Plan
module appServicePlan 'modules/app-service-plan.bicep' = {
  name: 'appServicePlan'
  params: {
    name: appServicePlanName
    location: location
    skuName: appServicePlanSku
    tags: tags
  }
}

// PostgreSQL Flexible Server
module postgres 'modules/postgresql.bicep' = {
  name: 'postgresql'
  params: {
    name: postgresServerName
    location: location
    administratorLogin: postgresAdminUser
    administratorPassword: postgresAdminPassword
    databaseName: 'myleague'
    skuName: postgresSku
    skuTier: 'Burstable'
    storageSizeGB: 32
    postgresVersion: '16'
    tags: tags
  }
}

// Storage Account for image uploads
module storageAccount 'modules/storage-account.bicep' = {
  name: 'storageAccount'
  params: {
    name: storageAccountName
    location: location
    containerName: 'images'
    skuName: storageSku
    tags: tags
  }
}

// Azure Communication Services (Email)
module communicationServices 'modules/communication-services.bicep' = {
  name: 'communicationServices'
  params: {
    name: communicationServiceName
    emailServiceName: emailServiceName
    tags: tags
  }
}

// App Service (API)
module appService 'modules/app-service.bicep' = {
  name: 'appService'
  params: {
    name: appServiceName
    location: location
    appServicePlanId: appServicePlan.outputs.id
    postgresConnectionString: postgres.outputs.connectionString
    environmentName: aspnetEnvironment
    allowedOrigins: allowedOrigins
    storageConnectionString: storageAccount.outputs.connectionString
    storageContainerName: storageAccount.outputs.containerName
    jwtSecretKey: jwtSecretKey
    acsConnectionString: communicationServices.outputs.connectionString
    acsSenderAddress: communicationServices.outputs.senderAddress
    seedAdminEmail: seedAdminEmail
    frontendBaseUrl: frontendBaseUrl
    tags: tags
  }
}

// ============================================================================
// Outputs
// ============================================================================

@description('The URL of the deployed API')
output apiUrl string = appService.outputs.url

@description('The hostname of the deployed API')
output apiHostname string = appService.outputs.hostname

@description('The name of the App Service')
output appServiceName string = appService.outputs.name

@description('The name of the App Service Plan')
output appServicePlanName string = appServicePlan.outputs.name

@description('The FQDN of the PostgreSQL server')
output postgresServerFqdn string = postgres.outputs.fqdn

@description('The name of the PostgreSQL server')
output postgresServerName string = postgres.outputs.name

@description('The name of the database')
output databaseName string = postgres.outputs.databaseName

@description('The name of the Storage Account')
output storageAccountName string = storageAccount.outputs.name

@description('The blob endpoint of the Storage Account')
output storageBlobEndpoint string = storageAccount.outputs.blobEndpoint

@description('The name of the blob container for images')
output storageContainerName string = storageAccount.outputs.containerName

@description('The name of the Communication Service')
output communicationServiceName string = communicationServices.outputs.name

@description('The name of the Email Service')
output emailServiceName string = communicationServices.outputs.emailServiceName

@description('The sender email address for the Communication Service')
output acsSenderAddress string = communicationServices.outputs.senderAddress
