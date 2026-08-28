using './backend.bicep'

// ============================================================================
// Production Environment Parameters - Backend
// ============================================================================

// Environment configuration
param environmentName = 'prod'
param baseName = 'myleague'

// Location - West Europe is typically good for European users
param location = 'westeurope'

// App Service Plan - Basic B1 handles ~5k users with a single instance.
// NOTE: SignalR uses an in-memory timer store, so keep instance count at 1
// (do not scale out) until a Redis/Azure SignalR backplane is added.
param appServicePlanSku = 'B1'

// PostgreSQL - Burstable B1ms; upgrade to Standard_B2s if CPU alerts fire often
param postgresSku = 'Standard_B1ms'
param postgresAdminUser = 'myleagueadmin'

// Secret - provided at deploy time (GitHub environment secret or CLI/script prompt)
param postgresAdminPassword = ''

// PostgreSQL backups - longer retention for production
param postgresBackupRetentionDays = 21

// CORS - update with the actual Static Web App hostname after the frontend
// is provisioned for the first time (SWA hostnames are auto-generated)
param allowedOrigins = []

// Secret - provided at deploy time (GitHub environment secret or CLI/script prompt)
param jwtSecretKey = ''

// Seed - admin email for initial user (provided at deploy time)
param seedAdminEmail = ''

// Frontend base URL - update after the frontend SWA is provisioned
param frontendBaseUrl = ''

// ============================================================================
// Monitoring & alerting
// ============================================================================

// Admin email that receives alerts - provided at deploy time
// (GitHub environment variable ALERT_EMAIL or script prompt).
// Leave empty to skip deploying alerts entirely.
param alertEmail = ''

// External uptime test enabled in prod: pings /health/ready every 5 minutes
// from 3 European regions and alerts if 2+ locations fail
param enableAvailabilityTest = true

// Monthly cost budget (USD) for the prod resource group
param monthlyBudgetAmount = 35

// Log Analytics daily ingestion cap (GB) - hard guard against runaway costs
param appInsightsDailyCapGb = 1
