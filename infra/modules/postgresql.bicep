// PostgreSQL Flexible Server Module
// Creates Azure Database for PostgreSQL Flexible Server for development environment

@description('The name of the PostgreSQL server')
param serverName string

@description('The location where resources will be deployed')
param location string = resourceGroup().location

@description('PostgreSQL administrator username')
@secure()
param administratorLogin string

@description('PostgreSQL administrator password')
@secure()
param administratorPassword string

@description('PostgreSQL version')
@allowed([
  '16'
  '15'
  '14'
  '13'
])
param postgresVersion string = '16'

@description('The tier of the PostgreSQL server')
@allowed([
  'Burstable'
  'GeneralPurpose'
  'MemoryOptimized'
])
param skuTier string = 'Burstable'

@description('The SKU name of the PostgreSQL server')
param skuName string = 'Standard_B1ms'

@description('Storage size in GB')
param storageSizeGB int = 32

@description('Backup retention days (7-35, minimum is 7)')
@minValue(7)
@maxValue(35)
param backupRetentionDays int = 7

@description('Environment tag')
param environment string = 'dev'

@description('Database name to create')
param databaseName string = 'myleague'

// PostgreSQL Flexible Server
resource postgresServer 'Microsoft.DBforPostgreSQL/flexibleServers@2023-03-01-preview' = {
  name: serverName
  location: location
  tags: {
    environment: environment
    project: 'myleague'
  }
  sku: {
    name: skuName
    tier: skuTier
  }
  properties: {
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorPassword
    version: postgresVersion
    storage: {
      storageSizeGB: storageSizeGB
      autoGrow: 'Enabled'
    }
    backup: {
      backupRetentionDays: backupRetentionDays
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
  }
}

// Firewall rule to allow Azure services
resource allowAzureServices 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2023-03-01-preview' = {
  parent: postgresServer
  name: 'AllowAllAzureServicesAndResourcesWithinAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// Firewall rule to allow all IPs for development (adjust for production!)
resource allowAllIPs 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2023-03-01-preview' = if (environment == 'dev') {
  parent: postgresServer
  name: 'AllowAllIPs'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '255.255.255.255'
  }
}

// Create database
resource database 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2023-03-01-preview' = {
  parent: postgresServer
  name: databaseName
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
}

// Outputs
@description('PostgreSQL server FQDN')
output serverFqdn string = postgresServer.properties.fullyQualifiedDomainName

@description('PostgreSQL server name')
output serverName string = postgresServer.name

@description('Database name')
output databaseName string = database.name

@description('PostgreSQL connection string for App Service')
output connectionString string = 'Host=${postgresServer.properties.fullyQualifiedDomainName};Database=${databaseName};Username=${administratorLogin};Password=${administratorPassword};SSL Mode=Require;Trust Server Certificate=true'

