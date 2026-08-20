using './frontend.bicep'

// ============================================================================
// Staging Environment Parameters - Frontend
// ============================================================================

// Environment configuration
param environmentName = 'staging'
param baseName = 'myleague'

// Location for Static Web App (limited region availability)
param location = 'westeurope'

// SKU - Free tier is sufficient (100 GB bandwidth/month, custom domains supported)
param sku = 'Free'

// Backend API URL (informational app setting on the SWA; the actual build-time
// value comes from VITE_API_URL during the frontend build)
param apiBackendUrl = 'https://myleague-staging-api.azurewebsites.net/api'
