using './backend.bicep'

// ============================================================================
// Staging Environment Parameters - Backend
// ============================================================================

// Environment configuration
param environmentName = 'staging'
param baseName = 'myleague'

// Location - West Europe is typically good for European users
param location = 'westeurope'

// App Service Plan - Basic B1 (lowest tier that supports Always On)
// Cost tip: stop the staging App Service when not testing:
//   az webapp stop --name myleague-staging-api --resource-group myleague-staging-rg
param appServicePlanSku = 'B1'

// PostgreSQL - Burstable B1ms (lowest cost)
param postgresSku = 'Standard_B1ms'
param postgresAdminUser = 'myleagueadmin'

// Secret - provided at deploy time (GitHub environment secret or CLI/script prompt)
param postgresAdminPassword = ''

// PostgreSQL backups - minimum retention for staging
param postgresBackupRetentionDays = 7

// CORS - staging Static Web App + local Vite against the staging API
param allowedOrigins = [
  'http://localhost:5173'
  'https://mango-sky-00d1a9c03.7.azurestaticapps.net'
]

// Secret - provided at deploy time (GitHub environment secret or CLI/script prompt)
param jwtSecretKey = ''

// Seed - admin email for initial user (provided at deploy time)
param seedAdminEmail = ''

param frontendBaseUrl = 'https://mango-sky-00d1a9c03.7.azurestaticapps.net'

// ============================================================================
// Monitoring & alerting
// ============================================================================

// Admin email that receives alerts - provided at deploy time
// (GitHub environment variable ALERT_EMAIL or script prompt).
// Leave empty to skip deploying alerts entirely.
param alertEmail = ''

// External uptime test disabled in staging to keep costs/noise down
param enableAvailabilityTest = false

// Monthly cost budget (USD) for the staging resource group
param monthlyBudgetAmount = 35

// Log Analytics daily ingestion cap (GB) - hard guard against runaway costs
param appInsightsDailyCapGb = 1
