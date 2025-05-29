import type { Person, PersonFormData } from '../../types/admin/personTypes';

const API_BASE_URL = '/api';

export const personApi = {
  getAll: async (): Promise<Person[]> => {
    const response = await fetch(`${API_BASE_URL}/persons`);
    if (!response.ok) {
      throw new Error('Failed to fetch persons');
    }
    return response.json();
  },

  getById: async (id: string): Promise<Person> => {
    const response = await fetch(`${API_BASE_URL}/persons/${id}`);
    if (!response.ok) {
      throw new Error('Failed to fetch person');
    }
    return response.json();
  },

  create: async (data: PersonFormData): Promise<Person> => {
    const response = await fetch(`${API_BASE_URL}/persons`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(data),
    });
    if (!response.ok) {
      throw new Error('Failed to create person');
    }
    return response.json();
  },

  update: async (id: string, data: PersonFormData): Promise<Person> => {
    const response = await fetch(`${API_BASE_URL}/persons/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(data),
    });
    if (!response.ok) {
      throw new Error('Failed to update person');
    }
    return response.json();
  },

  delete: async (id: string): Promise<void> => {
    const response = await fetch(`${API_BASE_URL}/persons/${id}`, {
      method: 'DELETE',
    });
    if (!response.ok) {
      throw new Error('Failed to delete person');
    }
  },
}; 