import { VITE_API_URL } from "../../constants/config";
import { parseErrorResponse } from "../utils/ParseErrorResponse";
import { authFetch } from '../utils/authFetch';

export interface Club {
  id: string;
  name: string;
  foundingDate: string | null;
  city: string | null;
  country: string | null;
  websiteUrl: string | null;
  logoUrl: string | null;
  contactEmail: string | null;
}

export interface ClubRequest {
  name: string;
  city?: string | null;
  country?: string | null;
  foundingDate?: string | null;
  websiteUrl?: string | null;
  logoUrl?: string | null;
  contactEmail?: string | null;
}

// Normalize raw API club object to Club type (handles both camelCase and PascalCase from API).
function normalizeClub(raw: Record<string, unknown>): Club {
  const str = (key: string) => {
    const v = raw[key] ?? raw[key.charAt(0).toUpperCase() + key.slice(1)];
    return v != null && typeof v === 'string' ? v : null;
  };
  return {
    id: String(raw.id ?? raw.Id ?? ''),
    name: String(raw.name ?? raw.Name ?? ''),
    foundingDate: str('foundingDate'),
    city: str('city'),
    country: str('country'),
    websiteUrl: str('websiteUrl'),
    logoUrl: str('logoUrl'),
    contactEmail: str('contactEmail'),
  };
}

// Errors are parsed via parseErrorResponse; ErrorPopup can show { title, errors } when message is JSON.

export const clubService = {
  getAll: async (): Promise<Club[]> => {
    const allClubs: Club[] = [];
    let currentPage = 1;
    const pageSize = 50; // Maximum allowed page size
    let hasMorePages = true;

    while (hasMorePages) {
      const response = await authFetch(`${VITE_API_URL}/Clubs?page=${currentPage}&pageSize=${pageSize}`);
      const data = await response.json();
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(data, 'Failed to fetch clubs');
        throw new Error(errorMessage || 'Failed to fetch clubs');
      }
      
      if (!data?.success) {
        const errorMessage = await parseErrorResponse(data, 'Failed to fetch clubs');
        throw new Error(errorMessage || 'Failed to fetch clubs');
      }
      
      // Handle paginated response structure
      if (data.data && Array.isArray(data.data)) {
        allClubs.push(...data.data);
        
        // Check if there are more pages
        if (data.pagination) {
          hasMorePages = data.pagination.hasNextPage || false;
          currentPage++;
        } else {
          // If no pagination info, assume no more pages if we got less than pageSize
          hasMorePages = data.data.length === pageSize;
          currentPage++;
        }
      } else if (Array.isArray(data)) {
        // Fallback: if data is directly an array
        allClubs.push(...data);
        hasMorePages = false;
      } else {
        console.error('Unexpected response structure from clubs API:', data);
        throw new Error('Unexpected response structure from clubs API');
      }
    }

    return allClubs;
  },

  getPaged: async (page: number, pageSize: number = 50): Promise<{
    data: Club[],
    pagination: {
      currentPage: number;
      pageSize: number;
      totalCount: number;
      totalPages: number;
      hasNextPage: boolean;
      hasPreviousPage: boolean;
      startItem: number;
      endItem: number;
    }
  }> => {
    const params = new URLSearchParams();
    params.append('page', String(page));
    params.append('pageSize', String(pageSize));
    const response = await authFetch(`${VITE_API_URL}/Clubs?${params.toString()}`);
    const data = await response.json();
    if (!response.ok || !data?.success) {
      const errorMessage = await parseErrorResponse(data, 'Failed to fetch clubs');
      throw new Error(errorMessage || 'Failed to fetch clubs');
    }
    return { data: data.data, pagination: data.pagination };
  },

  getById: async (id: string): Promise<Club> => {
    const response = await authFetch(`${VITE_API_URL}/Clubs/${id}`);
    const data = await response.json();
    if (!response.ok || !data?.success) {
      const errorMessage = await parseErrorResponse(data, 'Failed to fetch club');
      throw new Error(errorMessage || 'Failed to fetch club');
    }
    const raw = data.data as Record<string, unknown>;
    return raw && typeof raw === 'object' ? normalizeClub(raw) : (data.data as Club);
  },

  create: async (payload: ClubRequest): Promise<Club> => {
    const response = await authFetch(`${VITE_API_URL}/Clubs`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(payload)
    });
    const data = await response.json();
    if (!response.ok || !data?.success) {
      const errorMessage = await parseErrorResponse(data, 'Failed to create club');
      throw new Error(errorMessage || 'Failed to create club');
    }
    return data.data;
  },

  update: async (id: string, payload: ClubRequest): Promise<Club> => {
    const response = await authFetch(`${VITE_API_URL}/Clubs/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(payload)
    });
    const data = await response.json();
    if (!response.ok || !data?.success) {
      const errorMessage = await parseErrorResponse(data, 'Failed to update club');
      throw new Error(errorMessage || 'Failed to update club');
    }
    return data.data;
  },

  remove: async (id: string): Promise<void> => {
    const response = await authFetch(`${VITE_API_URL}/Clubs/${id}`, {
      method: 'DELETE'
    });
    const data = await response.json();
    if (!response.ok || !data?.success) {
      const errorMessage = await parseErrorResponse(data, 'Failed to delete club');
      throw new Error(errorMessage || 'Failed to delete club');
    }
  },

  searchByName: async (name: string): Promise<Club[]> => {
    const params = new URLSearchParams();
    params.append('name', name);
    const response = await authFetch(`${VITE_API_URL}/Clubs/search?${params.toString()}`);
    const data = await response.json();
    if (!response.ok || !data?.success) {
      // Handle 404 as empty results, not an error
      if (response.status === 404) {
        return [];
      }
      const errorMessage = await parseErrorResponse(data, 'Failed to search clubs');
      throw new Error(errorMessage || 'Failed to search clubs');
    }
    return data.data || [];
  }
};

export const getClubs = async (): Promise<Club[]> => {
  return clubService.getAll();
};