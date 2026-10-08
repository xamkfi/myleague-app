using './backend.bicep'

// ============================================================================
// MAHL Production Environment Parameters - Backend
// ============================================================================
// Same production behavior as backend.prod.bicepparam. baseName separates this
// instance (mahl-prod-*) from the XAMK production resource group.

// Environment configuration. Keep 'prod' so the API runs as ASP.NET Production
// and login-code auto-fill stays disabled.
param environmentName = 'prod'
param baseName = 'mahl'

// The MAHL subscription rejects new resources in West Europe. Sweden Central
// keeps the API, database, and storage in the EU. The Static Web App stays in
// East US 2 because that resource type has no other European region.
param location = 'swedencentral'

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

// CORS - Release MAHL Production overrides this with the live SWA URL plus the
// custom domains in CUSTOM_DOMAIN_ORIGINS; keep both lists in sync.
param allowedOrigins = [
  'https://orange-mushroom-0ad28900f.1.azurestaticapps.net'
  'https://www.mahl.fi'
  'https://mahl.fi'
]

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

// Monthly cost budget (USD) for the MAHL prod resource group.
// Expected spend ~42 USD: App Service B2 ~26 + PostgreSQL B1ms ~12-15 + extras.
param monthlyBudgetAmount = 50

// Must match the existing budget; Azure rejects changes to the start date
param budgetStartDate = '2026-09-01T00:00:00Z'

// Log Analytics daily ingestion cap (GB) - hard guard against runaway costs
param appInsightsDailyCapGb = 1
