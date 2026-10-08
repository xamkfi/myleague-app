using './backend.bicep'

// ============================================================================
// Production Environment Parameters - Backend
// ============================================================================

// Environment configuration
param environmentName = 'prod'
param baseName = 'myleague'

// Location - West Europe is typically good for European users
param location = 'westeurope'

// App Service Plan - Basic B2 (2 cores, 3.5 GB). B1 (1.75 GB) ran out of
// memory headroom and kept firing the plan memory alert.
// NOTE: SignalR uses an in-memory timer store, so keep instance count at 1
// (do not scale out) until a Redis/Azure SignalR backplane is added.
param appServicePlanSku = 'B2'

// PostgreSQL - Burstable B1ms; upgrade to Standard_B2s if CPU alerts fire often
param postgresSku = 'Standard_B1ms'
param postgresAdminUser = 'myleagueadmin'

// Secret - provided at deploy time (GitHub environment secret or CLI/script prompt)
param postgresAdminPassword = ''

// PostgreSQL backups - longer retention for production
param postgresBackupRetentionDays = 21

// CORS - Release Production / infra-deploy override this with the live SWA URL.
// After the first successful prod provision, commit that hostname here.
param allowedOrigins = []

// Secret - provided at deploy time (GitHub environment secret or CLI/script prompt)
param jwtSecretKey = ''

// Seed - admin email for initial user (provided at deploy time)
param seedAdminEmail = ''

// Frontend base URL - overridden at provision time from the SWA hostname
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

// Monthly cost budget (USD) for the prod resource group.
// Expected spend ~42 USD: App Service B2 ~26 + PostgreSQL B1ms ~12-15 + extras.
param monthlyBudgetAmount = 50

// Must match the existing budget; Azure rejects changes to the start date
param budgetStartDate = '2026-09-01T00:00:00Z'

// Log Analytics daily ingestion cap (GB) - hard guard against runaway costs
param appInsightsDailyCapGb = 1
