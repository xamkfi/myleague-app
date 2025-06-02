import { VITE_API_URL } from "../constants/config";

export interface Club {
  id: string;
  name: string;
  foundingDate: string;
  city: string;
  country: string;
  websiteUrl: string;
  logoUrl: string;
  contactEmail: string;
}

interface ApiResponse {
  success: boolean;
  data: Club[];
  message: string;
  errors: null | string[];
}

export const getClubs = async (): Promise<Club[]> => {
  try {
    const response = await fetch(`${VITE_API_URL}/Clubs`);
    const data: ApiResponse = await response.json();
    
    
    if (!data.success) {
      throw new Error(data.message || 'Failed to fetch clubs');
    }
    
    return data.data;
  } catch (error) {
    console.error('Error fetching clubs:', error);
    throw error;
  }
}; 