import type { 
  FloorballTeam, 
  FloorballTeamRequest, 
  GetFloorballTeamsRequest,
  ApiResponse,
  PaginatedApiResponse,
  FloorballTeamPlayerDto,
  UpdateFloorballTeamPlayerRequest,
  FloorballPosition,
  TeamCategory
} from '../../types/floorball/floorballTypes';
import { parseErrorResponse } from '../utils/ParseErrorResponse';

const API_URL = import.meta.env.VITE_API_URL || '/api';

export const floorballTeamService = {
  /**
   * Get all floorball teams with pagination and filtering
   */
  getAll: async (params?: GetFloorballTeamsRequest): Promise<PaginatedApiResponse<FloorballTeam>> => {
    const searchParams = new URLSearchParams();
    
    if (params?.page) searchParams.append('page', params.page.toString());
    if (params?.pageSize) searchParams.append('pageSize', params.pageSize.toString());
    if (params?.clubId) searchParams.append('clubId', params.clubId);
    if (params?.division) searchParams.append('division', params.division);

    const url = `${API_URL}/FloorballTeam${searchParams.toString() ? `?${searchParams.toString()}` : ''}`;
    
    const response = await fetch(url);
    if (!response.ok) {
      throw new Error('Failed to fetch floorball teams');
    }
    
    const apiResponse: PaginatedApiResponse<FloorballTeam> = await response.json();
    if (!apiResponse.success) {
      throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch floorball teams');
    }
    
    return apiResponse;
  },

  /**
   * Get a floorball team by ID
   */
  getById: async (id: string): Promise<FloorballTeam> => {
    const response = await fetch(`${API_URL}/FloorballTeam/${id}`);
    if (!response.ok) {
      throw new Error('Failed to fetch floorball team');
    }
    
    const apiResponse: ApiResponse<FloorballTeam> = await response.json();
    if (!apiResponse.success) {
      throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch floorball team');
    }
    
    return apiResponse.data;
  },

  /**
   * Create a new floorball team
   */
  create: async (data: FloorballTeamRequest): Promise<FloorballTeam> => {
    const response = await fetch(`${API_URL}/FloorballTeam`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(data),
    });
    
    if (!response.ok) {
      throw new Error('Failed to create floorball team');
    }
    
    const apiResponse: ApiResponse<FloorballTeam> = await response.json();
    if (!apiResponse.success) {
      const errorMessage = await parseErrorResponse(apiResponse, 'Failed to create floorball team');

      throw new Error(errorMessage || 'Failed to create floorball team');
    }
    
    return apiResponse.data;
  },

  /**
   * Update an existing floorball team
   */
  update: async (id: string, data: FloorballTeamRequest): Promise<FloorballTeam> => {
    try {
      console.log('Updating team with ID:', id);
      console.log('Update data:', data);
      
      const response = await fetch(`${API_URL}/FloorballTeam/${id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
      });
      
      console.log('Update response status:', response.status);
      console.log('Update response ok:', response.ok);
      
      const apiResponse: ApiResponse<FloorballTeam> = await response.json();
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(apiResponse, 'Failed to update floorball team');
        throw new Error(errorMessage || 'Failed to update floorball team');
      }
      
      console.log('Update API Response:', apiResponse);
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to update floorball team');
      }
      
      return apiResponse.data;
    } catch (error) {
      console.error('Error in floorballTeamService.update:', error);
      throw error;
    }
  },

  /**
   * Delete a floorball team
   */
  delete: async (id: string): Promise<void> => {
    const response = await fetch(`${API_URL}/FloorballTeam/${id}`, {
      method: 'DELETE',
    });
    const apiResponse: ApiResponse<void> = await response.json();
    
    if (!response.ok) {
      const errorMessage = await parseErrorResponse(apiResponse, 'Failed to delete floorball team');

      throw new Error(errorMessage || 'Failed to delete floorball team');
    }
    
    if (!apiResponse.success) {
      throw new Error(apiResponse.errors?.join(', ') || 'Failed to delete floorball team');
    }
  },

  /**
   * Add a player to a team with position and jersey number
   */
  addPlayerToTeam: async (
    teamId: string, 
    playerId: string, 
    position: FloorballPosition, 
    jerseyNumber?: number
  ): Promise<FloorballTeam> => {
    const searchParams = new URLSearchParams();
    searchParams.append('position', position);
    if (jerseyNumber !== undefined) {
      searchParams.append('jerseyNumber', jerseyNumber.toString());
    }

    const response = await fetch(`${API_URL}/FloorballTeam/${teamId}/players/${playerId}?${searchParams.toString()}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
    });
    const apiResponse: ApiResponse<FloorballTeam> = await response.json();

    if (!response.ok) {
      const errorMessage = await parseErrorResponse(apiResponse, 'Failed to add player to team');

      throw new Error(errorMessage || 'Failed to add player to team');
    }

    if (!apiResponse.success) {
      throw new Error(apiResponse.errors?.join(', ') || 'Failed to add player to team');
    }

    return apiResponse.data;
  },

  /**
   * Update a team player's position, jersey number, and active status
   */
  updateTeamPlayer: async (
    teamId: string, 
    playerId: string, 
    updateData: UpdateFloorballTeamPlayerRequest
  ): Promise<FloorballTeamPlayerDto> => {
    console.log('Updating team player:', { teamId, playerId, updateData });

    const response = await fetch(`${API_URL}/FloorballTeam/${teamId}/players/${playerId}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(updateData),
    });

    const apiResponse: ApiResponse<FloorballTeamPlayerDto> = await response.json();
    if (!response.ok) {
      const errorMessage = await parseErrorResponse(apiResponse, 'Failed to update team player');
      throw new Error(errorMessage || 'Failed to update team player');
    }

    console.log('Update team player API Response:', apiResponse);

    return apiResponse.data;
  },

  /**
   * Remove a player from a team
   */
  removePlayerFromTeam: async (teamId: string, playerId: string): Promise<FloorballTeam> => {
    const response = await fetch(`${API_URL}/FloorballTeam/${teamId}/players/${playerId}`, {
      method: 'DELETE',
    });
    
    const apiResponse: ApiResponse<FloorballTeam> = await response.json();
    if (!response.ok) {
      const errorMessage = await parseErrorResponse(apiResponse, 'Failed to remove player from team');
      throw new Error(errorMessage || 'Failed to remove player from team');
    }

    if (!apiResponse.success) {
      throw new Error(apiResponse.errors?.join(', ') || 'Failed to remove player from team');
    }

    return apiResponse.data;
  },

  /**
   * Get all floorball teams without roster with pagination, search, and filtering
   */
  getAllWithoutRoster: async (params?: {
    page?: number;
    pageSize?: number;
    searchTerm?: string;
    teamCategory?: TeamCategory;
  }): Promise<PaginatedApiResponse<FloorballTeam>> => {
    const searchParams = new URLSearchParams();
    
    if (params?.page) searchParams.append('page', params.page.toString());
    if (params?.pageSize) searchParams.append('pageSize', params.pageSize.toString());
    if (params?.searchTerm) searchParams.append('searchTerm', params.searchTerm);
    if (params?.teamCategory) searchParams.append('teamCategory', params.teamCategory);

    const url = `${API_URL}/FloorballTeam/without-roster${searchParams.toString() ? `?${searchParams.toString()}` : ''}`;
    
    const response = await fetch(url);
    if (!response.ok) {
      throw new Error('Failed to fetch floorball teams');
    }
    
    const apiResponse: PaginatedApiResponse<FloorballTeam> = await response.json();
    if (!apiResponse.success) {
      throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch floorball teams');
    }
    
    return apiResponse;
  },
}; 