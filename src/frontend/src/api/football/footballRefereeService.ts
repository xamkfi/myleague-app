import type { ApiResponse, PaginatedApiResponse } from '../../types/football/footballTypes';
import { authFetch } from '../utils/authFetch';
import { API_URL } from '../../constants/config';

// Referee types based on the backend DTOs
export interface FootballRefereeDto {
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

export interface GetFootballRefereesRequest {
  page?: number;
  pageSize?: number;
  isActive?: boolean;
  searchTerm?: string;
  licenseExpiringWithinDays?: number;
}

export interface CreateFootballRefereeRequest {
  PersonId: string;
  LicenseIssueDate: string;
  LicenseExpiryDate: string;
}

export interface UpdateFootballRefereeRequest {
  licenseIssueDate?: string;
  licenseExpiryDate?: string;
  matchesOfficiated: number;
  isActive: boolean;
}

export const footballRefereeService = {
  /**
   * Get all Football referees with pagination and filtering
   */
  getAll: async (params?: GetFootballRefereesRequest): Promise<PaginatedApiResponse<FootballRefereeDto>> => {
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

      const url = `${API_URL}/FootballReferee?${searchParams.toString()}`;
      
      const response = await authFetch(url);
      
      
      if (!response.ok) {
        const errorText = await response.text();
        console.error('API Error Response:', errorText);
        throw new Error(`HTTP ${response.status}: ${errorText || 'Failed to fetch Football referees'}`);
      }
      
      const apiResponse: PaginatedApiResponse<FootballRefereeDto> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch Football referees');
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
      console.error('Error in footballRefereeService.getAll:', error);
      throw error;
    }
  },

  /**
   * Get a Football referee by ID
   */
  getById: async (id: string): Promise<FootballRefereeDto> => {
    try {
      const url = `${API_URL}/FootballReferee/${id}`;
      
      const response = await authFetch(url);
      
      if (!response.ok) {
        const errorText = await response.text();
        console.error('API Error Response:', errorText);
        throw new Error(`HTTP ${response.status}: ${errorText || 'Failed to fetch Football referee'}`);
      }
      
      const apiResponse: ApiResponse<FootballRefereeDto> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch Football referee');
      }
      
      return apiResponse.data;
    } catch (error) {
      console.error('Error in footballRefereeService.getById:', error);
      throw error;
    }
  },

  /**
   * Create a Football referee from a person
   */
  create: async (data: CreateFootballRefereeRequest): Promise<FootballRefereeDto> => {
    try {
      
      const response = await authFetch(`${API_URL}/FootballReferee`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
      });
      
      
      if (!response.ok) {
        const errorText = await response.text();
        console.error('Create API Error Response:', errorText);
        throw new Error(`HTTP ${response.status}: ${errorText || 'Failed to create Football referee'}`);
      }
      
      const apiResponse: ApiResponse<FootballRefereeDto> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to create Football referee');
      }
      
      return apiResponse.data;
    } catch (error) {
      console.error('Error in footballRefereeService.create:', error);
      throw error;
    }
  },

  /**
   * Update a Football referee
   */
  update: async (id: string, data: UpdateFootballRefereeRequest): Promise<FootballRefereeDto> => {
    try {
      
      const response = await authFetch(`${API_URL}/FootballReferee/${id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
      });
      
      
      if (!response.ok) {
        const errorText = await response.text();
        console.error('Update API Error Response:', errorText);
        throw new Error(`HTTP ${response.status}: ${errorText || 'Failed to update Football referee'}`);
      }
      
      const apiResponse: ApiResponse<FootballRefereeDto> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to update Football referee');
      }
      
      return apiResponse.data;
    } catch (error) {
      console.error('Error in footballRefereeService.update:', error);
      throw error;
    }
  },

  /**
   * Delete a Football referee
   */
  delete: async (id: string): Promise<void> => {
    try {
      
      const response = await authFetch(`${API_URL}/FootballReferee/${id}`, {
        method: 'DELETE',
      });
      
      
      if (!response.ok) {
        const errorText = await response.text();
        console.error('Delete API Error Response:', errorText);
        throw new Error(`HTTP ${response.status}: ${errorText || 'Failed to delete Football referee'}`);
      }
      
      const apiResponse: ApiResponse<void> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to delete Football referee');
      }
    } catch (error) {
      console.error('Error in footballRefereeService.delete:', error);
      throw error;
    }
  }
}; 