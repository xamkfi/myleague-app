import type { Person, PersonFormData, PersonRole } from '../../types/admin/personTypes';
import { parseErrorResponse } from '../utils/ParseErrorResponse';

interface ApiResponse<T> {
  success: boolean;
  data: T;
  message: string;
  errors: string[];
}

const API_URL = import.meta.env.VITE_API_URL || '/api';

export const personApi = {
  getAll: async (): Promise<Person[]> => {
    const response = await fetch(`${API_URL}/persons`);
    if (!response.ok) {
      throw new Error('Failed to fetch persons');
    }
    const apiResponse: ApiResponse<Person[]> = await response.json();
    if (!apiResponse.success) {
      const errorMessage = await parseErrorResponse(apiResponse, "Failed to fetch persons")
      throw new Error(errorMessage || 'Failed to fetch persons');
    }
    return apiResponse.data;
  },

  getById: async (id: string): Promise<Person> => {
    const response = await fetch(`${API_URL}/persons/${id}`);
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
    const response = await fetch(`${API_URL}/persons`, {
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
    const response = await fetch(`${API_URL}/persons/${id}`, {
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
    const response = await fetch(`${API_URL}/persons/${id}`, {
      method: 'DELETE',
    });

    const apiResponse: ApiResponse<Person> = await response.json();
    if (!response.ok || !apiResponse.success) {
      const errorMessage = await parseErrorResponse(apiResponse, "Failed to update person")

      throw new Error(errorMessage || "Failed to delete person");
    }
  },

  updateRegistration: async (id: string, isRegistered: boolean): Promise<Person> => {
    const response = await fetch(`${API_URL}/persons/${id}/registration`, {
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
    const response = await fetch(`${API_URL}/persons/${id}/role`, {
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
}; 