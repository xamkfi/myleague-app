import { VITE_API_URL } from "../../constants/config";
import { parseErrorResponse } from "../utils/ParseErrorResponse";

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

export interface ClubRequest {
  name: string;
  city: string;
  country: string;
  foundingDate: string | null;
  websiteUrl?: string | null;
  logoUrl?: string | null;
  contactEmail?: string | null;
}

// Note: API response shapes may vary (ApiResponse or ProblemDetails). We parse dynamically.
// TODO: Standardize all API services (news, matches, seasons, floorball, persons, etc.)
//       to route errors through parseErrorResponse and surface them via ErrorPopup.
//       Contract: thrown Error.message should be a JSON string of the form
//       {"title": string, "errors": string[]} so the UI can render consistent messages.

export const clubService = {
  getAll: async (): Promise<Club[]> => {
    const response = await fetch(`${VITE_API_URL}/Clubs?page=1&pageSize=1000`);
    const data = await response.json();
    if (!response.ok || !data?.success) {
      const errorMessage = await parseErrorResponse(data, 'Failed to fetch clubs');
      throw new Error(errorMessage || 'Failed to fetch clubs');
    }
    return data.data;
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
    const response = await fetch(`${VITE_API_URL}/Clubs?${params.toString()}`);
    const data = await response.json();
    if (!response.ok || !data?.success) {
      const errorMessage = await parseErrorResponse(data, 'Failed to fetch clubs');
      throw new Error(errorMessage || 'Failed to fetch clubs');
    }
    return { data: data.data, pagination: data.pagination };
  },

  getById: async (id: string): Promise<Club> => {
    const response = await fetch(`${VITE_API_URL}/Clubs/${id}`);
    const data = await response.json();
    if (!response.ok || !data?.success) {
      const errorMessage = await parseErrorResponse(data, 'Failed to fetch club');
      throw new Error(errorMessage || 'Failed to fetch club');
    }
    return data.data;
  },

  create: async (payload: ClubRequest): Promise<Club> => {
    const response = await fetch(`${VITE_API_URL}/Clubs`, {
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
    const response = await fetch(`${VITE_API_URL}/Clubs/${id}`, {
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
    const response = await fetch(`${VITE_API_URL}/Clubs/${id}`, {
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
    const response = await fetch(`${VITE_API_URL}/Clubs/search?${params.toString()}`);
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