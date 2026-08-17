import type { 
  FootballTeam, 
  FootballTeamRequest, 
  GetFootballTeamsRequest,
  ApiResponse,
  PaginatedApiResponse,
  FootballTeamPlayerDto,
  UpdateFootballTeamPlayerRequest,
  FootballPosition,
  TeamCategory
} from '../../types/football/footballTypes';
import { authFetch } from '../utils/authFetch';
import { parseErrorResponse } from '../utils/ParseErrorResponse';
import { API_URL } from '../../constants/config';

export const footballTeamService = {
  /**
   * Get all Football teams with pagination and filtering
   */
  getAll: async (params?: GetFootballTeamsRequest): Promise<PaginatedApiResponse<FootballTeam>> => {
    const searchParams = new URLSearchParams();
    
    if (params?.page) searchParams.append('page', params.page.toString());
    if (params?.pageSize) searchParams.append('pageSize', params.pageSize.toString());
    if (params?.clubId) searchParams.append('clubId', params.clubId);
    if (params?.division) searchParams.append('division', params.division);
    if (params?.searchTerm) searchParams.append('searchTerm', params.searchTerm);
    params?.teamCategories?.forEach(category => searchParams.append('teamCategory', category));

    const url = `${API_URL}/FootballTeam${searchParams.toString() ? `?${searchParams.toString()}` : ''}`;
    
    const response = await authFetch(url);
    if (!response.ok) {
      throw new Error('Failed to fetch Football teams');
    }
    
    const apiResponse: PaginatedApiResponse<FootballTeam> = await response.json();
    if (!apiResponse.success) {
      throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch Football teams');
    }
    
    return apiResponse;
  },

  /**
   * Get a Football team by ID
   */
  getById: async (id: string): Promise<FootballTeam> => {
    const response = await authFetch(`${API_URL}/FootballTeam/${id}`);
    if (!response.ok) {
      throw new Error('Failed to fetch Football team');
    }
    
    const apiResponse: ApiResponse<FootballTeam> = await response.json();
    if (!apiResponse.success) {
      throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch Football team');
    }
    
    return apiResponse.data;
  },

  /**
   * Create a new Football team
   */
  create: async (data: FootballTeamRequest): Promise<FootballTeam> => {
    const response = await authFetch(`${API_URL}/FootballTeam`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(data),
    });

    // Read the body regardless of status so server-side validation messages reach the caller.
    let apiResponse: ApiResponse<FootballTeam> | null = null;
    try {
      apiResponse = await response.json();
    } catch {
      apiResponse = null;
    }

    if (!response.ok) {
      const errorMessage = await parseErrorResponse(
        apiResponse ?? response,
        `Failed to create Football team (HTTP ${response.status})`,
      );
      throw new Error(errorMessage);
    }

    if (!apiResponse) {
      throw new Error('Failed to create Football team â€” empty response body.');
    }

    if (!apiResponse.success) {
      const errorMessage = await parseErrorResponse(apiResponse, 'Failed to create Football team');
      throw new Error(errorMessage || 'Failed to create Football team');
    }

    return apiResponse.data;
  },

  /**
   * Update an existing Football team
   */
  update: async (id: string, data: FootballTeamRequest): Promise<FootballTeam> => {
    try {
      
      const response = await authFetch(`${API_URL}/FootballTeam/${id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
      });
      
      
      const apiResponse: ApiResponse<FootballTeam> = await response.json();
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(apiResponse, 'Failed to update Football team');
        throw new Error(errorMessage || 'Failed to update Football team');
      }
      
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to update Football team');
      }
      
      return apiResponse.data;
    } catch (error) {
      console.error('Error in footballTeamService.update:', error);
      throw error;
    }
  },

  /**
   * Delete a Football team
   */
  delete: async (id: string): Promise<void> => {
    const response = await authFetch(`${API_URL}/FootballTeam/${id}`, {
      method: 'DELETE',
    });
    const apiResponse: ApiResponse<void> = await response.json();
    
    if (!response.ok) {
      const errorMessage = await parseErrorResponse(apiResponse, 'Failed to delete Football team');

      throw new Error(errorMessage || 'Failed to delete Football team');
    }
    
    if (!apiResponse.success) {
      throw new Error(apiResponse.errors?.join(', ') || 'Failed to delete Football team');
    }
  },

  /**
   * Add a player to a team with position and jersey number
   */
  addPlayerToTeam: async (
    teamId: string,
    playerId: string,
    position: FootballPosition,
    jerseyNumber?: number,
    /**
     * Optional originally-requested jersey number. Set this when the caller wanted a
     * specific number but had to substitute another (e.g. the tournament import flow
     * picks the next free number on a conflict). The backend stores it so the roster
     * UI can highlight the row for admin review.
     */
    requestedJerseyNumber?: number
  ): Promise<FootballTeam> => {
    const searchParams = new URLSearchParams();
    searchParams.append('position', position);
    if (jerseyNumber !== undefined) {
      searchParams.append('jerseyNumber', jerseyNumber.toString());
    }
    if (requestedJerseyNumber !== undefined && requestedJerseyNumber !== jerseyNumber) {
      searchParams.append('requestedJerseyNumber', requestedJerseyNumber.toString());
    }

    const response = await authFetch(`${API_URL}/FootballTeam/${teamId}/players/${playerId}?${searchParams.toString()}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
    });
    const apiResponse: ApiResponse<FootballTeam> = await response.json();

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
    updateData: UpdateFootballTeamPlayerRequest
  ): Promise<FootballTeamPlayerDto> => {

    const response = await authFetch(`${API_URL}/FootballTeam/${teamId}/players/${playerId}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(updateData),
    });

    const apiResponse: ApiResponse<FootballTeamPlayerDto> = await response.json();
    if (!response.ok) {
      const errorMessage = await parseErrorResponse(apiResponse, 'Failed to update team player');
      throw new Error(errorMessage || 'Failed to update team player');
    }


    return apiResponse.data;
  },

  /**
   * Remove a player from a team
   */
  removePlayerFromTeam: async (teamId: string, playerId: string): Promise<FootballTeam> => {
    const response = await authFetch(`${API_URL}/FootballTeam/${teamId}/players/${playerId}`, {
      method: 'DELETE',
    });
    
    const apiResponse: ApiResponse<FootballTeam> = await response.json();
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
   * Get all Football teams without roster with pagination, search, and filtering
   */
  getAllWithoutRoster: async (params?: {
    page?: number;
    pageSize?: number;
    searchTerm?: string;
    teamCategory?: TeamCategory;
  }): Promise<PaginatedApiResponse<FootballTeam>> => {
    const searchParams = new URLSearchParams();
    
    if (params?.page) searchParams.append('page', params.page.toString());
    if (params?.pageSize) searchParams.append('pageSize', params.pageSize.toString());
    if (params?.searchTerm) searchParams.append('searchTerm', params.searchTerm);
    if (params?.teamCategory) searchParams.append('teamCategory', params.teamCategory);

    const url = `${API_URL}/FootballTeam/without-roster${searchParams.toString() ? `?${searchParams.toString()}` : ''}`;
    
    const response = await authFetch(url);
    if (!response.ok) {
      throw new Error('Failed to fetch Football teams');
    }
    
    const apiResponse: PaginatedApiResponse<FootballTeam> = await response.json();
    if (!apiResponse.success) {
      throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch Football teams');
    }
    
    return apiResponse;
  },
}; 