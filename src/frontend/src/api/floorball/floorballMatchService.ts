import type { 
  ApiResponse,
  PaginatedApiResponse,
  FloorballMatchDto,
  CreateFloorballMatchRequest,
  UpdateFloorballMatchRequest,
  GetFloorballMatchesRequest
} from '../../types/floorball/floorballTypes';
import { authFetch } from '../utils/authFetch';
import { parseErrorResponse } from '../utils/ParseErrorResponse';
import { API_URL } from '../../constants/config';

export const floorballMatchService = {
  /**
   * Get all floorball matches with pagination and filtering
   */
  getAll: async (params?: GetFloorballMatchesRequest): Promise<PaginatedApiResponse<FloorballMatchDto>> => {
    try {
      const searchParams = new URLSearchParams();
      
      if (params?.page) searchParams.append('page', params.page.toString());
      if (params?.pageSize) searchParams.append('pageSize', params.pageSize.toString());
      if (params?.competitionId) searchParams.append('competitionId', params.competitionId);
      if (params?.teamId) searchParams.append('teamId', params.teamId);
      if (params?.startDate) searchParams.append('startDate', params.startDate);
      if (params?.endDate) searchParams.append('endDate', params.endDate);
      if (params?.sortOrder) searchParams.append('sortOrder', params.sortOrder);
      if (params?.status) searchParams.append('status', params.status);
      if (params?.searchQuery) searchParams.append('searchQuery', params.searchQuery);
      if (params?.tournamentGroupId) searchParams.append('tournamentGroupId', params.tournamentGroupId);
      if (params?.competitionType) searchParams.append('competitionType', params.competitionType);

      const url = `${API_URL}/FloorballMatch?${searchParams.toString()}`;
      console.log('Fetching matches from URL:', url);
      
      const response = await authFetch(url);
      const apiResponse: PaginatedApiResponse<FloorballMatchDto> = await response.json();
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(apiResponse, 'Failed to fetch floorball matches');
        throw new Error(errorMessage);
      }
      
      console.log('API Response:', apiResponse);
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch floorball matches');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballMatchService.getAll:', error);
      throw error;
    }
  },

  /**
   * Delete a goal event from a match
   */
  deleteGoal: async (matchId: string, goalEventId: string): Promise<ApiResponse<FloorballMatchDto>> => {
    const url = `${API_URL}/FloorballMatch/${matchId}/goal/${goalEventId}`;
    console.log('DELETE goal URL:', url);
    const response = await authFetch(url, {
      method: 'DELETE'
    });

    const apiResponse: ApiResponse<FloorballMatchDto> = await response.json();
    
    if (!response.ok) {
      const errorMessage = await parseErrorResponse(apiResponse, 'Failed to delete goal');
      throw new Error(errorMessage);
    }
    return apiResponse;
  },

  /**
   * Delete a penalty event from a match
   */
  deletePenalty: async (matchId: string, penaltyEventId: string): Promise<ApiResponse<FloorballMatchDto>> => {
    const url = `${API_URL}/FloorballMatch/${matchId}/penalty/${penaltyEventId}`;
    console.log('DELETE penalty URL:', url);
    const response = await authFetch(url, {
      method: 'DELETE'
    });
    const apiResponse: ApiResponse<FloorballMatchDto> = await response.json();
    if (!response.ok) {
      const errorMessage = await parseErrorResponse(apiResponse, 'Failed to delete penalty');
      throw new Error(errorMessage);
    }
    return apiResponse;
  },

  /**
   * Delete a save event from a match
   */
  deleteSave: async (matchId: string, saveEventId: string): Promise<ApiResponse<FloorballMatchDto>> => {
    const url = `${API_URL}/FloorballMatch/${matchId}/save/${saveEventId}`;
    console.log('DELETE save URL:', url);
    const response = await authFetch(url, {
      method: 'DELETE'
    });

    const apiResponse: ApiResponse<FloorballMatchDto> = await response.json();
    
    if (!response.ok) {
      const errorMessage = await parseErrorResponse(apiResponse, 'Failed to delete save');
      throw new Error(errorMessage);
    }
    return apiResponse;
  },

  /**
   * Get matches by season ID
   */
  getBySeason: async (competitionId: string): Promise<ApiResponse<FloorballMatchDto[]>> => {
    try {
      const url = `${API_URL}/FloorballMatch/by-competitionId/${competitionId}`;
      console.log('Fetching matches by season from URL:', url);
      
      const response = await authFetch(url);
      const apiResponse: ApiResponse<FloorballMatchDto[]> = await response.json();
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(apiResponse, 'Failed to fetch matches by season');
        throw new Error(errorMessage);
      }
      
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch matches by season');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballMatchService.getBySeason:', error);
      throw error;
    }
  },

  /**
   * Get matches by team ID
   */
  getByTeam: async (teamId: string): Promise<ApiResponse<FloorballMatchDto[]>> => {
    try {
      const url = `${API_URL}/FloorballMatch/by-team/${teamId}`;
      console.log('Fetching matches by team from URL:', url);
      
      const response = await authFetch(url);
      const apiResponse: ApiResponse<FloorballMatchDto[]> = await response.json();
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(apiResponse, 'Failed to fetch matches by team');
        throw new Error(errorMessage);
      }
      
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch matches by team');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballMatchService.getByTeam:', error);
      throw error;
    }
  },

  /**
   * Get today's matches by team ID
   */
  getTodaysMatchesByTeam: async (teamId: string): Promise<ApiResponse<FloorballMatchDto[]>> => {
    try {
      const url = `${API_URL}/FloorballMatch/by-team/${teamId}/today`;
      console.log('Fetching today\'s matches by team from URL:', url);
      
      const response = await authFetch(url);
      
      const apiResponse: ApiResponse<FloorballMatchDto[]> = await response.json();
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(apiResponse, 'Failed to fetch today\'s matches by team');
        throw new Error(errorMessage);
      }
      
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch today\'s matches by team');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballMatchService.getTodaysMatchesByTeam:', error);
      throw error;
    }
  },

  /**
   * Get a floorball match by ID
   */
  getById: async (id: string): Promise<ApiResponse<FloorballMatchDto>> => {
    try {
      const url = `${API_URL}/FloorballMatch/by-id/${id}`;
      console.log('Fetching match from URL:', url);
      
      const response = await authFetch(url);
      const apiResponse: ApiResponse<FloorballMatchDto> = await response.json();
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(apiResponse, 'Failed to fetch floorball match');
        throw new Error(errorMessage);
      }
      
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch floorball match');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballMatchService.getById:', error);
      throw error;
    }
  },

  /**
   * Create a new floorball match
   */
  create: async (data: CreateFloorballMatchRequest): Promise<ApiResponse<FloorballMatchDto>> => {
    try {
      console.log('Creating match:', data);
      
      const response = await authFetch(`${API_URL}/FloorballMatch`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
      });
      const apiResponse: ApiResponse<FloorballMatchDto> = await response.json();
      console.log('Create API Response:', apiResponse);
      
      console.log('Create response status:', response.status);
      console.log('Create response ok:', response.ok);
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(apiResponse, 'Failed to create floorball match');
        throw new Error(errorMessage);
      }
      
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to create floorball match');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballMatchService.create:', error);
      throw error;
    }
  },

  /**
   * Update a floorball match
   */
  update: async (data: UpdateFloorballMatchRequest): Promise<ApiResponse<FloorballMatchDto>> => {
    try {
      console.log('Updating match:', data);
      
      const response = await authFetch(`${API_URL}/FloorballMatch`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
      });
      const apiResponse: ApiResponse<FloorballMatchDto> = await response.json();
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(apiResponse, 'Failed to update floorball match');
        throw new Error(errorMessage);
      }
      
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to update floorball match');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballMatchService.update:', error);
      throw error;
    }
  },

  /**
   * Permanently delete a floorball match. Only allowed for matches still in the
   * Scheduled state (server enforces this). Used by the tournament JSON import
   * revert flow to remove freshly created matches.
   */
  delete: async (id: string): Promise<ApiResponse<void>> => {
    try {
      const response = await authFetch(`${API_URL}/FloorballMatch/${id}`, {
        method: 'DELETE',
      });

      const apiResponse: ApiResponse<void> = await response.json();

      if (!response.ok) {
        const errorMessage = await parseErrorResponse(apiResponse, 'Failed to delete floorball match');
        throw new Error(errorMessage);
      }

      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to delete floorball match');
      }

      return apiResponse;
    } catch (error) {
      console.error('Error in floorballMatchService.delete:', error);
      throw error;
    }
  },

  /**
   * Start a floorball match
   */
  start: async (id: string): Promise<ApiResponse<FloorballMatchDto>> => {
    try {
      console.log('Starting match with ID:', id);
      
      const response = await authFetch(`${API_URL}/FloorballMatch/start-match/${id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
      });
      const apiResponse: ApiResponse<FloorballMatchDto> = await response.json();
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(apiResponse, 'Failed to start floorball match');
        throw new Error(errorMessage);
      }
      
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to start floorball match');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballMatchService.start:', error);
      throw error;
    }
  },

  /**
   * Complete a floorball match
   */
  complete: async (id: string): Promise<ApiResponse<FloorballMatchDto>> => {
    try {
      console.log('Completing match with ID:', id);
      
      const response = await authFetch(`${API_URL}/FloorballMatch/complete-match/${id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
      });
      
      const apiResponse: ApiResponse<FloorballMatchDto> = await response.json();

      if (!response.ok) {
        const errorMessage = await parseErrorResponse(apiResponse, 'Failed to complete floorball match');
        throw new Error(errorMessage);
      }
      
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to complete floorball match');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballMatchService.complete:', error);
      throw error;
    }
  },

  /**
   * Change match season
   */
  changeCompetition: async (id: string, competitionId: string): Promise<ApiResponse<FloorballMatchDto>> => {
    try {
      console.log('Changing season for match with ID:', id, 'to season:', competitionId);
      
      const response = await authFetch(`${API_URL}/FloorballMatchEvent/match/${id}/season`, {
        method: 'PATCH',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ newCompetitionId: competitionId }),
      });
      
      const apiResponse: ApiResponse<FloorballMatchDto> = await response.json();
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(apiResponse, 'Failed to change match season');
        throw new Error(errorMessage);
      }
      
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to change match season');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballMatchService.changeCompetition:', error);
      throw error;
    }
  },

  /**
   * Change match teams
   */
  changeTeams: async (id: string, homeTeamId: string, awayTeamId: string): Promise<ApiResponse<FloorballMatchDto>> => {
    try {
      console.log('Changing teams for match with ID:', id, 'home:', homeTeamId, 'away:', awayTeamId);
      
      const response = await authFetch(`${API_URL}/FloorballMatchEvent/match/${id}/teams`, {
        method: 'PATCH',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ newHomeTeamId: homeTeamId, newAwayTeamId: awayTeamId }),
      });
      
      const apiResponse: ApiResponse<FloorballMatchDto> = await response.json();
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(apiResponse, 'Failed to change match teams');
        throw new Error(errorMessage);
      }
      
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to change match teams');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballMatchService.changeTeams:', error);
      throw error;
    }
  },

  /**
   * Change match venue
   */
  changeVenue: async (id: string, venue: string): Promise<ApiResponse<FloorballMatchDto>> => {
    try {
      console.log('Changing venue for match with ID:', id, 'to venue:', venue);
      // Fetch current match to preserve scheduledDateTime
      const current = await (await authFetch(`${API_URL}/FloorballMatch/by-id/${id}`)).json() as ApiResponse<FloorballMatchDto>;
      if (!current.success || !current.data) {
        throw new Error(current.errors?.join(', ') || 'Failed to fetch current match');
      }
      const response = await authFetch(`${API_URL}/FloorballMatch`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ id, scheduledDateTime: current.data.scheduledDateTime, venue }),
      });
      
      const apiResponse: ApiResponse<FloorballMatchDto> = await response.json();
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(apiResponse, 'Failed to change match venue');
        throw new Error(errorMessage);
      }
      
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to change match venue');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballMatchService.changeVenue:', error);
      throw error;
    }
  },

  /**
   * Change match date/time
   */
  changeDateTime: async (id: string, scheduledDateTime: string): Promise<ApiResponse<FloorballMatchDto>> => {
    try {
      console.log('Changing date/time for match with ID:', id, 'to:', scheduledDateTime);
      // Fetch current match to preserve venue
      const current = await (await authFetch(`${API_URL}/FloorballMatch/by-id/${id}`)).json() as ApiResponse<FloorballMatchDto>;
      if (!current.success || !current.data) {
        throw new Error(current.errors?.join(', ') || 'Failed to fetch current match');
      }
      const response = await authFetch(`${API_URL}/FloorballMatch`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ id, scheduledDateTime, venue: current.data.venue }),
      });
      const apiResponse: ApiResponse<FloorballMatchDto> = await response.json();
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(apiResponse, 'Failed to change match date/time');
        throw new Error(errorMessage);
      }
      
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to change match date/time');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballMatchService.changeDateTime:', error);
      throw error;
    }
  },

  /**
   * Changes the active goalie for a team in a match
   */
  changeGoalie: async (matchId: string, teamId: string, goalieId: string): Promise<ApiResponse<FloorballMatchDto>> => {
    try {
      const response = await authFetch(`${API_URL}/FloorballMatch/${matchId}/team/${teamId}/goalie/${goalieId}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
      });

      const apiResponse: ApiResponse<FloorballMatchDto> = await response.json();
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(apiResponse, 'Failed to change goalie');
        throw new Error(errorMessage);
      }

      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to change goalie');
      }
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballMatchService.changeGoalie:', error);
      throw error;
    }
  },

  /**
   * Change match referee
   */
  changeReferee: async (matchId: string, refereeId: string): Promise<ApiResponse<FloorballMatchDto>> => {
    try {
      const response = await authFetch(`${API_URL}/FloorballMatch/${matchId}/referee/${refereeId}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
      });

      const raw = await response.text();
      const apiResponse: ApiResponse<FloorballMatchDto> = raw ? JSON.parse(raw) : { success: response.ok, data: undefined as unknown as FloorballMatchDto, message: '', errors: [] };
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(apiResponse, 'Failed to change match referee');
        throw new Error(errorMessage);
      }

      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to change match referee');
      }
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballMatchService.changeReferee:', error);
      throw error;
    }
  },

  /**
   * Update match officials list
   */
  updateOfficials: async (matchId: string, officials: string[]): Promise<ApiResponse<FloorballMatchDto>> => {
    try {
      const response = await authFetch(`${API_URL}/FloorballMatch/${matchId}/officials`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ officials }),
      });

      const raw = await response.text();
      const apiResponse: ApiResponse<FloorballMatchDto> = raw ? JSON.parse(raw) : { success: response.ok, data: undefined as unknown as FloorballMatchDto, message: '', errors: [] };
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(apiResponse, 'Failed to update match officials');
        throw new Error(errorMessage);
      }

      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to update match officials');
      }
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballMatchService.updateOfficials:', error);
      throw error;
    }
  },

  /**
   * Add a single official (append).
   */
  addOfficial: async (matchId: string, refereeId: string): Promise<ApiResponse<FloorballMatchDto>> => {
    try {
      const response = await authFetch(`${API_URL}/FloorballMatch/${matchId}/officials`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ refereeId }),
      });

      const raw = await response.text();
      const apiResponse: ApiResponse<FloorballMatchDto> = raw ? JSON.parse(raw) : { success: response.ok, data: undefined as unknown as FloorballMatchDto, message: '', errors: [] };
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(apiResponse, 'Failed to add official');
        throw new Error(errorMessage);
      }

      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to add official');
      }
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballMatchService.addOfficial:', error);
      throw error;
    }
  },

  /**
   * Delete an official from a match.
   */
  deleteOfficial: async (matchId: string, refereeId: string): Promise<ApiResponse<FloorballMatchDto>> => {
    try {
      const response = await authFetch(`${API_URL}/FloorballMatch/${matchId}/officials/${refereeId}`, {
        method: 'DELETE',
        headers: {
          'Content-Type': 'application/json',
        },
      });

      const raw = await response.text();
      const apiResponse: ApiResponse<FloorballMatchDto> = raw ? JSON.parse(raw) : { success: response.ok, data: undefined as unknown as FloorballMatchDto, message: '', errors: [] };
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(apiResponse, 'Failed to delete official');
        throw new Error(errorMessage);
      }

      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to delete official');
      }
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballMatchService.deleteOfficial:', error);
      throw error;
    }
  }
}; 