using './frontend.bicep'

// ============================================================================
// Development Environment Parameters - Frontend
// ============================================================================

// Environment configuration
param environmentName = 'dev'
param baseName = 'myleague'

// Location for Static Web App
// Note: Static Web Apps have limited region availability
param location = 'westeurope'

// SKU - Free tier for development
param sku = 'Free'

// Backend API URL - Update this after deploying the backend!
// Example: 'https://myleague-dev-api.azurewebsites.net'
param apiBackendUrl = ''
