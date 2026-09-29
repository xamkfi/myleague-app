using './frontend.bicep'

// ============================================================================
// MAHL Production Environment Parameters - Frontend
// ============================================================================
// Same production behavior as frontend.prod.bicepparam. baseName separates this
// instance (mahl-prod-web) from the XAMK production Static Web App.

// Environment configuration. Keep 'prod' so this stays a production deployment.
param environmentName = 'prod'
param baseName = 'mahl'

// Location for Static Web App (limited region availability)
param location = 'westeurope'

// SKU - Free tier is sufficient at this scale (100 GB bandwidth/month,
// custom domains supported). Upgrade to Standard only if you need SLA,
// more staging slots, or private endpoints.
param sku = 'Free'

// Backend API URL (informational app setting on the SWA; the actual build-time
// value comes from VITE_API_URL during the frontend build)
param apiBackendUrl = 'https://mahl-prod-api.azurewebsites.net/api'
