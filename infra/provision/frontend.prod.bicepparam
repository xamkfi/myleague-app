using './frontend.bicep'

// ============================================================================
// Production Environment Parameters - Frontend
// ============================================================================

// Environment configuration
param environmentName = 'prod'
param baseName = 'myleague'

// Location for Static Web App (limited region availability)
param location = 'westeurope'

// SKU - Free tier is sufficient at this scale (100 GB bandwidth/month,
// custom domains supported). Upgrade to Standard only if you need SLA,
// more staging slots, or private endpoints.
param sku = 'Free'

// Backend API URL (informational app setting on the SWA; the actual build-time
// value comes from VITE_API_URL during the frontend build)
param apiBackendUrl = 'https://myleague-prod-api.azurewebsites.net/api'
