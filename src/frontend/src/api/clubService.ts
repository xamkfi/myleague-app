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
    const response = await fetch('http://localhost:8080/api/Clubs');
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