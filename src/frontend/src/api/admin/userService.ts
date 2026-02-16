import type { SystemUser, CreateUserPayload, UpdateUserPayload } from '../../types/admin/userTypes';
import { parseErrorResponse } from '../utils/ParseErrorResponse';
import { API_URL } from '../../constants/config';
import { authFetch } from '../utils/authFetch';

interface ApiResponse<T> {
  success: boolean;
  data: T;
  message: string;
  errors: string[];
}

export const userService = {
  getAll: async (): Promise<SystemUser[]> => {
    const response = await authFetch(`${API_URL}/Users`);

    const apiResponse: ApiResponse<SystemUser[]> = await response.json();

    if (!response.ok || !apiResponse.success) {
      const errorMessage = await parseErrorResponse(apiResponse, 'Failed to fetch users');
      throw new Error(errorMessage || 'Failed to fetch users');
    }

    return apiResponse.data;
  },

  getById: async (id: string): Promise<SystemUser> => {
    const response = await authFetch(`${API_URL}/Users/${id}`);

    const apiResponse: ApiResponse<SystemUser> = await response.json();

    if (!response.ok || !apiResponse.success) {
      const errorMessage = await parseErrorResponse(apiResponse, 'Failed to fetch user');
      throw new Error(errorMessage || 'Failed to fetch user');
    }

    return apiResponse.data;
  },

  create: async (data: CreateUserPayload): Promise<SystemUser> => {
    const response = await authFetch(`${API_URL}/Users`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(data),
    });

    const apiResponse: ApiResponse<SystemUser> = await response.json();

    if (!response.ok || !apiResponse.success) {
      const errorMessage = await parseErrorResponse(apiResponse, 'Failed to create user');
      throw new Error(errorMessage || 'Failed to create user');
    }

    return apiResponse.data;
  },

  update: async (id: string, data: UpdateUserPayload): Promise<SystemUser> => {
    const response = await authFetch(`${API_URL}/Users/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(data),
    });

    const apiResponse: ApiResponse<SystemUser> = await response.json();

    if (!response.ok || !apiResponse.success) {
      const errorMessage = await parseErrorResponse(apiResponse, 'Failed to update user');
      throw new Error(errorMessage || 'Failed to update user');
    }

    return apiResponse.data;
  },

  delete: async (id: string): Promise<void> => {
    const response = await authFetch(`${API_URL}/Users/${id}`, {
      method: 'DELETE',
    });

    const apiResponse: ApiResponse<null> = await response.json();

    if (!response.ok || !apiResponse.success) {
      const errorMessage = await parseErrorResponse(apiResponse, 'Failed to delete user');
      throw new Error(errorMessage || 'Failed to delete user');
    }
  },
};
