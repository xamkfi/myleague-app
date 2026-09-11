import type { ApiResponse, PaginatedApiResponse } from '../../types/floorball/floorballTypes';
import { authFetch } from '../utils/authFetch';
import { API_URL } from '../../constants/config';

// Referee types based on the backend DTOs
export interface FloorballRefereeDto {
  id: string;
  personId: string;
  person: {
    id: string;
    firstName: string;
    lastName: string;
    fullName: string;
    birthDate: string;
    email?: string;
    phoneNumber?: string;
    address?: string;
    isRegistered: boolean;
  };
  isActive: boolean;
  licenseIssueDate?: string;
  licenseExpiryDate?: string;
  matchesOfficiated: number;
}

export interface GetFloorballRefereesRequest {
  page?: number;
  pageSize?: number;
  isActive?: boolean;
  searchTerm?: string;
  licenseExpiringWithinDays?: number;
}

export interface CreateFloorballRefereeRequest {
  PersonId: string;
  LicenseIssueDate: string;
  LicenseExpiryDate: string;
}

export interface UpdateFloorballRefereeRequest {
  licenseIssueDate?: string;
  licenseExpiryDate?: string;
  matchesOfficiated: number;
  isActive: boolean;
}

export const floorballRefereeService = {
  /**
   * Get all floorball referees with pagination and filtering
   */
  getAll: async (params?: GetFloorballRefereesRequest): Promise<PaginatedApiResponse<FloorballRefereeDto>> => {
    try {
      const searchParams = new URLSearchParams();
      
      // Always provide page (default to 1 if not specified)
      const page = params?.page ?? 1;
      searchParams.append('page', page.toString());
      
      // Always provide pageSize (default to 0 for backend default)
      const pageSize = params?.pageSize ?? 0;
      searchParams.append('pageSize', pageSize.toString());
      
      if (params?.isActive !== undefined) searchParams.append('isActive', params.isActive.toString());
      if (params?.searchTerm) searchParams.append('searchTerm', params.searchTerm);
      if (params?.licenseExpiringWithinDays) searchParams.append('licenseExpiringWithinDays', params.licenseExpiringWithinDays.toString());

      const url = `${API_URL}/FloorballReferee?${searchParams.toString()}`;
      console.log('Fetching referees from URL:', url);
      console.log('Request params:', params);
      
      const response = await authFetch(url);
      
      console.log('Response status:', response.status);
      console.log('Response ok:', response.ok);
      
      if (!response.ok) {
        const errorText = await response.text();
        console.error('API Error Response:', errorText);
        throw new Error(`HTTP ${response.status}: ${errorText || 'Failed to fetch floorball referees'}`);
      }
      
      const apiResponse: PaginatedApiResponse<FloorballRefereeDto> = await response.json();
      console.log('API Response:', apiResponse);
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch floorball referees');
      }
      
      // Ensure we return a valid PaginatedApiResponse structure
      if (!apiResponse.data) {
        console.warn('API response missing data field, setting empty array');
        apiResponse.data = [];
      }
      
      if (!Array.isArray(apiResponse.data)) {
        console.warn('API response data is not an array:', apiResponse.data);
        apiResponse.data = [];
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballRefereeService.getAll:', error);
      throw error;
    }
  },

  /**
   * Get a floorball referee by ID
   */
  getById: async (id: string): Promise<FloorballRefereeDto> => {
    try {
      const url = `${API_URL}/FloorballReferee/${id}`;
      console.log('Fetching referee from URL:', url);
      
      const response = await authFetch(url);
      
      if (!response.ok) {
        const errorText = await response.text();
        console.error('API Error Response:', errorText);
        throw new Error(`HTTP ${response.status}: ${errorText || 'Failed to fetch floorball referee'}`);
      }
      
      const apiResponse: ApiResponse<FloorballRefereeDto> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch floorball referee');
      }
      
      return apiResponse.data;
    } catch (error) {
      console.error('Error in floorballRefereeService.getById:', error);
      throw error;
    }
  },

  /**
   * Create a floorball referee from a person
   */
  create: async (data: CreateFloorballRefereeRequest): Promise<FloorballRefereeDto> => {
    try {
      console.log('Creating referee for person ID:', data.PersonId);
      
      const response = await authFetch(`${API_URL}/FloorballReferee`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
      });
      
      console.log('Create response status:', response.status);
      console.log('Create response ok:', response.ok);
      
      if (!response.ok) {
        const errorText = await response.text();
        console.error('Create API Error Response:', errorText);
        throw new Error(`HTTP ${response.status}: ${errorText || 'Failed to create floorball referee'}`);
      }
      
      const apiResponse: ApiResponse<FloorballRefereeDto> = await response.json();
      console.log('Create API Response:', apiResponse);
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to create floorball referee');
      }
      
      return apiResponse.data;
    } catch (error) {
      console.error('Error in floorballRefereeService.create:', error);
      throw error;
    }
  },

  /**
   * Update a floorball referee
   */
  update: async (id: string, data: UpdateFloorballRefereeRequest): Promise<FloorballRefereeDto> => {
    try {
      console.log('Updating referee with ID:', id);
      console.log('Update data:', data);
      
      const response = await authFetch(`${API_URL}/FloorballReferee/${id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
      });
      
      console.log('Update response status:', response.status);
      console.log('Update response ok:', response.ok);
      
      if (!response.ok) {
        const errorText = await response.text();
        console.error('Update API Error Response:', errorText);
        throw new Error(`HTTP ${response.status}: ${errorText || 'Failed to update floorball referee'}`);
      }
      
      const apiResponse: ApiResponse<FloorballRefereeDto> = await response.json();
      console.log('Update API Response:', apiResponse);
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to update floorball referee');
      }
      
      return apiResponse.data;
    } catch (error) {
      console.error('Error in floorballRefereeService.update:', error);
      throw error;
    }
  },

  /**
   * Delete a floorball referee
   */
  delete: async (id: string): Promise<void> => {
    try {
      console.log('Deleting referee with ID:', id);
      
      const response = await authFetch(`${API_URL}/FloorballReferee/${id}`, {
        method: 'DELETE',
      });
      
      console.log('Delete response status:', response.status);
      console.log('Delete response ok:', response.ok);
      
      if (!response.ok) {
        const errorText = await response.text();
        console.error('Delete API Error Response:', errorText);
        throw new Error(`HTTP ${response.status}: ${errorText || 'Failed to delete floorball referee'}`);
      }
      
      const apiResponse: ApiResponse<void> = await response.json();
      console.log('Delete API Response:', apiResponse);
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to delete floorball referee');
      }
    } catch (error) {
      console.error('Error in floorballRefereeService.delete:', error);
      throw error;
    }
  }
}; 