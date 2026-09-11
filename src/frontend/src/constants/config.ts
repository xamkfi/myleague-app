// Normalize API URL by removing trailing slashes to prevent double-slash issues (e.g., api//News)
const rawApiUrl = import.meta.env.VITE_API_URL || '/api';
export const VITE_API_URL = rawApiUrl.replace(/\/+$/, '');

// Export API_URL for convenience (same as VITE_API_URL but more semantic)
export const API_URL = VITE_API_URL;
