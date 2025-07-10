import type { Person, PersonFormData, PersonRole } from '../../types/admin/personTypes';

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
      throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch persons');
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
      throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch person');
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
    if (!response.ok) {
      throw new Error('Failed to create person');
    }
    const apiResponse: ApiResponse<Person> = await response.json();
    if (!apiResponse.success) {
      throw new Error(apiResponse.errors?.join(', ') || 'Failed to create person');
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
    if (!response.ok) {
      throw new Error('Failed to update person');
    }
    const apiResponse: ApiResponse<Person> = await response.json();
    if (!apiResponse.success) {
      throw new Error(apiResponse.errors?.join(', ') || 'Failed to update person');
    }
    return apiResponse.data;
  },

  delete: async (id: string): Promise<void> => {
    const response = await fetch(`${API_URL}/persons/${id}`, {
      method: 'DELETE',
    });
    if (!response.ok) {
      throw new Error('Failed to delete person');
    }
    const apiResponse: ApiResponse<void> = await response.json();
    if (!apiResponse.success) {
      throw new Error(apiResponse.errors?.join(', ') || 'Failed to delete person');
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
    if (!response.ok) {
      throw new Error('Failed to update registration status');
    }
    const apiResponse: ApiResponse<Person> = await response.json();
    if (!apiResponse.success) {
      throw new Error(apiResponse.errors?.join(', ') || 'Failed to update registration status');
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
    if (!response.ok) {
      throw new Error('Failed to update person role');
    }
    const apiResponse: ApiResponse<Person> = await response.json();
    console.log('Role update response:', apiResponse); // Debug log
    if (!apiResponse.success) {
      throw new Error(apiResponse.errors?.join(', ') || 'Failed to update person role');
    }
    return apiResponse.data;
  },
}; 