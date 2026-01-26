@description('The name of the PostgreSQL Flexible Server')
param name string

@description('The Azure region for the PostgreSQL server')
param location string = resourceGroup().location

@description('The administrator username for PostgreSQL')
param administratorLogin string

@description('The administrator password for PostgreSQL')
@secure()
param administratorPassword string

@description('The name of the database to create')
param databaseName string = 'myleague'

@description('The SKU name for the PostgreSQL server')
@allowed([
  'Standard_B1ms'
  'Standard_B2s'
  'Standard_D2s_v3'
])
param skuName string = 'Standard_B1ms'

@description('The tier of the PostgreSQL server')
@allowed([
  'Burstable'
  'GeneralPurpose'
  'MemoryOptimized'
])
param skuTier string = 'Burstable'

@description('The storage size in GB')
@minValue(32)
@maxValue(16384)
param storageSizeGB int = 32

@description('The PostgreSQL version')
@allowed([
  '14'
  '15'
  '16'
])
param postgresVersion string = '16'

@description('Tags to apply to the PostgreSQL server')
param tags object = {}

resource postgresServer 'Microsoft.DBforPostgreSQL/flexibleServers@2023-12-01-preview' = {
  name: name
  location: location
  tags: tags
  sku: {
    name: skuName
    tier: skuTier
  }
  properties: {
    version: postgresVersion
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorPassword
    storage: {
      storageSizeGB: storageSizeGB
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
  }
}

// Create the application database
resource database 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2023-12-01-preview' = {
  parent: postgresServer
  name: databaseName
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
}

// Firewall rule to allow Azure services
resource allowAzureServices 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2023-12-01-preview' = {
  parent: postgresServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

@description('The fully qualified domain name of the PostgreSQL server')
output fqdn string = postgresServer.properties.fullyQualifiedDomainName

@description('The connection string for the PostgreSQL database')
output connectionString string = 'Host=${postgresServer.properties.fullyQualifiedDomainName};Database=${databaseName};Username=${administratorLogin};Password=${administratorPassword};SSL Mode=Require;Trust Server Certificate=true'

@description('The resource ID of the PostgreSQL server')
output id string = postgresServer.id

@description('The name of the PostgreSQL server')
output name string = postgresServer.name

@description('The name of the database')
output databaseName string = database.name
