import type { Person, PersonFormData, PersonRole, PaginatedApiResponse } from '../../types/admin/personTypes';
import { parseErrorResponse } from '../utils/ParseErrorResponse';
import { API_URL } from '../../constants/config';
import { authFetch } from '../utils/authFetch';

interface ApiResponse<T> {
  success: boolean;
  data: T;
  message: string;
  errors: string[];
}

export const personApi = {
  getAll: async (page: number = 1, pageSize: number = 25): Promise<PaginatedApiResponse<Person>> => {
    const searchParams = new URLSearchParams();
    searchParams.append('page', page.toString());
    searchParams.append('pageSize', pageSize.toString());

    const response = await authFetch(`${API_URL}/persons?${searchParams.toString()}`);
    
    const apiResponse: PaginatedApiResponse<Person> = await response.json();
    
    if (!response.ok || !apiResponse.success) {
      const errorMessage = await parseErrorResponse(apiResponse, "Failed to fetch persons");
      throw new Error(errorMessage || 'Failed to fetch persons');
    }

    return apiResponse;
  },

  getById: async (id: string): Promise<Person> => {
    const response = await authFetch(`${API_URL}/persons/${id}`);
    if (!response.ok) {
      throw new Error('Failed to fetch person');
    }

    const apiResponse: ApiResponse<Person> = await response.json();
    if (!apiResponse.success) {
      const errorMessage = await parseErrorResponse(apiResponse, "Failed to fetch person")

      throw new Error(errorMessage || 'Failed to fetch person');
    }
    
    return apiResponse.data;
  },

  create: async (data: PersonFormData): Promise<Person> => {
    const response = await authFetch(`${API_URL}/persons`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(data),
    });

    const apiResponse: ApiResponse<Person> = await response.json();
    if (!response.ok || !apiResponse.success) {
      const errorMessage = await parseErrorResponse(apiResponse, "Failed to create person")
      throw new Error(errorMessage || "Failed to create person");
    }

    return apiResponse.data;
  },

  update: async (id: string, data: PersonFormData): Promise<Person> => {
    const response = await authFetch(`${API_URL}/persons/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(data),
    });
    
    const apiResponse: ApiResponse<Person> = await response.json();
    if (!response.ok || !apiResponse.success) {
      const errorMessage = await parseErrorResponse(apiResponse, "Failed to update person")

      throw new Error(errorMessage || "Failed to update person");
    }

    return apiResponse.data;
  },

  delete: async (id: string): Promise<void> => {
    const response = await authFetch(`${API_URL}/persons/${id}`, {
      method: 'DELETE',
    });

    const apiResponse: ApiResponse<Person> = await response.json();
    if (!response.ok || !apiResponse.success) {
      const errorMessage = await parseErrorResponse(apiResponse, "Failed to update person")

      throw new Error(errorMessage || "Failed to delete person");
    }
  },

  updateRegistration: async (id: string, isRegistered: boolean): Promise<Person> => {
    const response = await authFetch(`${API_URL}/persons/${id}/registration`, {
      method: 'PATCH',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(isRegistered),
    });

    const apiResponse: ApiResponse<Person> = await response.json();
    if (!response.ok || !apiResponse.success) {
      const errorMessage = await parseErrorResponse(apiResponse, "Failed to update registration")

      throw new Error(errorMessage || "Failed to update registration");
    }
    
    return apiResponse.data;
  },

  updateRole: async (id: string, role: PersonRole): Promise<Person> => {
    console.log('Sending role update request:', { id, role, type: typeof role }); // Debug log
    const response = await authFetch(`${API_URL}/persons/${id}/role`, {
      method: 'PATCH',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(role),
    });

    const apiResponse: ApiResponse<Person> = await response.json();
    if (!response.ok || !apiResponse.success) {
      const errorMessage = await parseErrorResponse(apiResponse, "Failed to update person")

      throw new Error(errorMessage || "Failed to update role");
    }

    return apiResponse.data;
  },

  /**
   * Looks up a Person by their primary email address. Returns null on 404 so callers
   * can use this as a find-or-create probe without try/catching for "not found".
   */
  getByEmail: async (email: string): Promise<Person | null> => {
    if (!email || !email.trim()) return null;
    const response = await authFetch(`${API_URL}/persons/by-email?email=${encodeURIComponent(email.trim().toLowerCase())}`);
    if (response.status === 404) return null;
    const apiResponse: ApiResponse<Person> = await response.json();
    if (!response.ok || !apiResponse.success) {
      // 404 already handled. Other failures should bubble up.
      const errorMessage = await parseErrorResponse(apiResponse, 'Failed to fetch person by email');
      throw new Error(errorMessage || 'Failed to fetch person by email');
    }
    return apiResponse.data ?? null;
  },

  search: async (name: string, page: number = 1, pageSize: number = 25): Promise<PaginatedApiResponse<Person>> => {
    if (!name || !name.trim()) {
      throw new Error('Name parameter is required for search');
    }

    const searchParams = new URLSearchParams();
    searchParams.append('name', name.trim());
    searchParams.append('page', page.toString());
    searchParams.append('pageSize', pageSize.toString());

    const response = await authFetch(`${API_URL}/persons/search?${searchParams.toString()}`);
    
    const apiResponse: PaginatedApiResponse<Person> = await response.json();
    
    if (!response.ok || !apiResponse.success) {
      const errorMessage = await parseErrorResponse(apiResponse, "Failed to search persons");
      throw new Error(errorMessage || 'Failed to search persons');
    }

    return apiResponse;
  },
}; 