import type { 
  FloorballTeamPlayer,
  ApiResponse,
  PaginatedApiResponse 
} from '../../types/floorball/floorballTypes';

const API_URL = import.meta.env.VITE_API_URL || '/api';

export interface FloorballPlayerDto {
  id: string;
  personId: string;
  firstName: string;
  lastName: string;
  fullName: string;
  dateOfBirth: string;
  isActive: boolean;
  position?: string;
  jerseyNumber?: number;
  gamesPlayed: number;
  goals: number;
  assists: number;
  penaltyMinutes: number;
  teamId?: string;
}

export interface GetFloorballPlayersRequest {
  page?: number;
  pageSize?: number;
  isActive?: boolean;
  position?: string;
  teamId?: string;
  searchTerm?: string;
}

export const floorballPlayerService = {
  /**
   * Get all floorball players with pagination and filtering
   */
  getAll: async (params?: GetFloorballPlayersRequest): Promise<PaginatedApiResponse<FloorballPlayerDto>> => {
    try {
      const searchParams = new URLSearchParams();
      
      if (params?.page) searchParams.append('page', params.page.toString());
      if (params?.pageSize) searchParams.append('pageSize', params.pageSize.toString());
      if (params?.isActive !== undefined) searchParams.append('isActive', params.isActive.toString());
      if (params?.position) searchParams.append('position', params.position);
      if (params?.teamId) searchParams.append('teamId', params.teamId);
      if (params?.searchTerm) searchParams.append('searchTerm', params.searchTerm);

      const url = `${API_URL}/FloorballPlayer${searchParams.toString() ? `?${searchParams.toString()}` : ''}`;
      
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
        pageSize: 100 // Get all players for the team
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
  }
}; 