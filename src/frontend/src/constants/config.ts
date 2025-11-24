// Get API URL from environment variable, or use backend URL in production
const getApiUrl = () => {
  // Priority 1: If VITE_API_URL is explicitly set (from build-time env var), use it
  if (import.meta.env.VITE_API_URL && import.meta.env.VITE_API_URL !== '/api') {
    return import.meta.env.VITE_API_URL;
  }
  
  // Priority 2: Runtime check - if running on Azure Static Web Apps, use backend URL
  if (typeof window !== 'undefined') {
    const hostname = window.location.hostname;
    // Check if we're running on Azure Static Web Apps domain
    if (hostname.includes('azurestaticapps.net') || hostname.includes('azurewebsites.net')) {
      return 'https://app-myleague-bicep-dev.azurewebsites.net/api';
    }
  }
  
  // Priority 3: In development mode (local), use localhost
  if (import.meta.env.DEV || import.meta.env.MODE === 'development') {
    return 'http://localhost:8080/api';
  }
  
  // Priority 4: Default fallback for production - use the backend App Service URL
  return 'https://app-myleague-bicep-dev.azurewebsites.net/api';
};

export const VITE_API_URL = getApiUrl();
