using './backend.bicep'

// ============================================================================
// Development Environment Parameters
// ============================================================================

// Environment configuration
param environmentName = 'dev'
param baseName = 'myleague'

// Location - West Europe is typically good for European users
// Change this to your preferred Azure region
param location = 'westeurope'

// App Service Plan - Basic B1 for development (low cost)
param appServicePlanSku = 'B1'

// PostgreSQL - Burstable B1ms for development (low cost)
param postgresSku = 'Standard_B1ms'

// PostgreSQL admin credentials
// IMPORTANT: Change the password before deploying!
param postgresAdminUser = 'myleagueadmin'

// This password will be prompted during deployment if not provided
// You can also use: --parameters postgresAdminPassword='YourSecurePassword123!'
// Or use Azure Key Vault reference for production
param postgresAdminPassword = '' // Will be prompted during deployment

// CORS - Add your frontend URLs here when you deploy the frontend
// Example: ['https://myleague-dev.azurestaticapps.net', 'http://localhost:5173']
param allowedOrigins = [
  'http://localhost:5173'  // Vite dev server
  'http://localhost:3000'  // Alternative dev port
]

// JWT Secret Key - will be prompted during deployment
// Must be at least 32 characters for HMAC-SHA256
param jwtSecretKey = '' // Will be prompted during deployment

// Seed - admin email for initial user (optional)
// Set this to create an admin user on first startup
param seedAdminEmail = ''
