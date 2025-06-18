import type { 
  FloorballTeamPlayer,
  ApiResponse,
  PaginatedApiResponse 
} from '../../types/floorball/floorballTypes';

const API_URL = import.meta.env.VITE_API_URL || '/api';

export interface PersonDto {
  id: string;
  firstName: string;
  lastName: string;
  birthDate: string;
  fullName: string;
  isRegistered: boolean;
  address?: any;
  contactInfo?: any;
}

export interface FloorballPlayerDto {
  id: string;
  personId: string;
  person: PersonDto;  // Nested person object to match backend
  isActive: boolean;
  position: string;
  careerGoals: number;
  careerAssists: number;
}

export interface GetFloorballPlayersRequest {
  page?: number;
  pageSize?: number;
  isActive?: boolean;
  position?: string;
  teamId?: string;
  searchTerm?: string;
}

export interface UpdateFloorballPlayerRequest {
  isActive: boolean;
}

export interface CreateFloorballPlayerRequest {
  personId: string;
}

export const floorballPlayerService = {
  /**
   * Get all floorball players with pagination and filtering
   */
  getAll: async (params?: GetFloorballPlayersRequest): Promise<PaginatedApiResponse<FloorballPlayerDto>> => {
    try {
      const searchParams = new URLSearchParams();
      
      // Always provide page (default to 1 if not specified)
      const page = params?.page ?? 1;
      searchParams.append('page', page.toString());
      
      // Always provide pageSize (default to 0 for backend default)
      const pageSize = params?.pageSize ?? 0;
      searchParams.append('pageSize', pageSize.toString());
      
      if (params?.isActive !== undefined) searchParams.append('isActive', params.isActive.toString());
      if (params?.position) searchParams.append('position', params.position);
      if (params?.teamId) searchParams.append('teamId', params.teamId);
      if (params?.searchTerm) searchParams.append('searchTerm', params.searchTerm);

      const url = `${API_URL}/FloorballPlayer?${searchParams.toString()}`;
      console.log('Fetching players from URL:', url);
      console.log('Request params:', params);
      
      const response = await fetch(url);
      
      console.log('Response status:', response.status);
      console.log('Response ok:', response.ok);
      
      if (!response.ok) {
        const errorText = await response.text();
        console.error('API Error Response:', errorText);
        throw new Error(`HTTP ${response.status}: ${errorText || 'Failed to fetch floorball players'}`);
      }
      
      const apiResponse: PaginatedApiResponse<FloorballPlayerDto> = await response.json();
      console.log('API Response:', apiResponse);
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch floorball players');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballPlayerService.getAll:', error);
      throw error;
    }
  },

  /**
   * Get players by team ID
   */
  getByTeamId: async (teamId: string): Promise<FloorballPlayerDto[]> => {
    try {
      console.log('Fetching players for team ID:', teamId);
      
      const response = await floorballPlayerService.getAll({
        teamId,
        pageSize: 50 // Use max allowed page size to get as many players as possible
      });
      
      console.log('Players fetched for team:', response.data?.length || 0);
      return response.data || [];
    } catch (error) {
      console.error('Error in floorballPlayerService.getByTeamId:', error);
      throw error;
    }
  },

  /**
   * Get a floorball player by ID
   */
  getById: async (id: string): Promise<FloorballPlayerDto> => {
    try {
      const url = `${API_URL}/FloorballPlayer/${id}`;
      console.log('Fetching player from URL:', url);
      
      const response = await fetch(url);
      
      if (!response.ok) {
        const errorText = await response.text();
        console.error('API Error Response:', errorText);
        throw new Error(`HTTP ${response.status}: ${errorText || 'Failed to fetch floorball player'}`);
      }
      
      const apiResponse: ApiResponse<FloorballPlayerDto> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch floorball player');
      }
      
      return apiResponse.data;
    } catch (error) {
      console.error('Error in floorballPlayerService.getById:', error);
      throw error;
    }
  },

  /**
   * Update a floorball player
   */
  update: async (id: string, data: UpdateFloorballPlayerRequest): Promise<FloorballPlayerDto> => {
    try {
      console.log('Updating player with ID:', id);
      console.log('Update data:', data);
      
      const response = await fetch(`${API_URL}/FloorballPlayer/${id}`, {
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
        throw new Error(`HTTP ${response.status}: ${errorText || 'Failed to update floorball player'}`);
      }
      
      const apiResponse: ApiResponse<FloorballPlayerDto> = await response.json();
      console.log('Update API Response:', apiResponse);
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to update floorball player');
      }
      
      return apiResponse.data;
    } catch (error) {
      console.error('Error in floorballPlayerService.update:', error);
      throw error;
    }
  },

  /**
   * Create a floorball player from a person
   */
  create: async (data: CreateFloorballPlayerRequest): Promise<FloorballPlayerDto> => {
    try {
      console.log('Creating player for person ID:', data.personId);
      
      const response = await fetch(`${API_URL}/FloorballPlayer`, {
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
        throw new Error(`HTTP ${response.status}: ${errorText || 'Failed to create floorball player'}`);
      }
      
      const apiResponse: ApiResponse<FloorballPlayerDto> = await response.json();
      console.log('Create API Response:', apiResponse);
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to create floorball player');
      }
      
      return apiResponse.data;
    } catch (error) {
      console.error('Error in floorballPlayerService.create:', error);
      throw error;
    }
  },

  /**
   * Delete a floorball player
   */
  delete: async (id: string): Promise<void> => {
    try {
      console.log('Deleting player with ID:', id);
      
      const response = await fetch(`${API_URL}/FloorballPlayer/${id}`, {
        method: 'DELETE',
      });
      
      console.log('Delete response status:', response.status);
      console.log('Delete response ok:', response.ok);
      
      if (!response.ok) {
        const errorText = await response.text();
        console.error('Delete API Error Response:', errorText);
        throw new Error(`HTTP ${response.status}: ${errorText || 'Failed to delete floorball player'}`);
      }
      
      const apiResponse: ApiResponse<void> = await response.json();
      console.log('Delete API Response:', apiResponse);
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to delete floorball player');
      }
    } catch (error) {
      console.error('Error in floorballPlayerService.delete:', error);
      throw error;
    }
  }
}; 