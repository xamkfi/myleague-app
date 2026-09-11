import type {
  ApiResponse,
  PaginatedApiResponse,
  FootballMatchDto,
  CreateFootballMatchRequest,
  UpdateFootballMatchRequest,
  GetFootballMatchesRequest,
  AssignMatchTeamsRequest
} from '../../types/football/footballTypes';
import type { FootballPosition } from '../../types/football/footballTypes';
import { authFetch } from '../utils/authFetch';
import { parseErrorResponse } from '../utils/ParseErrorResponse';
import { API_URL } from '../../constants/config';

// Phase 2 of the Football match controller refactor split the original FootballMatchController
// into smaller controllers grouped by concern: queries+CRUD live at api/football-matches; the
// per-match lifecycle, events, officials and roster operations live at sibling routes under
// api/football-matches/{matchId}/... Keep this constant local to this service so it's easy to
// grep when the routes change again.
const MATCHES_PATH = 'football-matches';

export const footballMatchService = {
  /**
   * Get all Football matches with pagination and filtering
   */
  getAll: async (params?: GetFootballMatchesRequest): Promise<PaginatedApiResponse<FootballMatchDto>> => {
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
      if (params?.teamCategory) searchParams.append('teamCategory', params.teamCategory);

      const url = `${API_URL}/${MATCHES_PATH}?${searchParams.toString()}`;
      
      const response = await authFetch(url);
      const apiResponse: PaginatedApiResponse<FootballMatchDto> = await response.json();
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(apiResponse, 'Failed to fetch Football matches');
        throw new Error(errorMessage);
      }
      
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch Football matches');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in footballMatchService.getAll:', error);
      throw error;
    }
  },

  /**
   * Delete a goal event from a match
   */
  deleteGoal: async (matchId: string, goalEventId: string): Promise<ApiResponse<FootballMatchDto>> => {
    const url = `${API_URL}/${MATCHES_PATH}/${matchId}/events/goal/${goalEventId}`;
    const response = await authFetch(url, {
      method: 'DELETE'
    });

    const apiResponse: ApiResponse<FootballMatchDto> = await response.json();
    
    if (!response.ok) {
      const errorMessage = await parseErrorResponse(apiResponse, 'Failed to delete goal');
      throw new Error(errorMessage);
    }
    return apiResponse;
  },

  deleteCard: async (matchId: string, cardEventId: string): Promise<ApiResponse<FootballMatchDto>> => {
    const url = `${API_URL}/${MATCHES_PATH}/${matchId}/events/card/${cardEventId}`;
    const response = await authFetch(url, { method: 'DELETE' });
    const apiResponse: ApiResponse<FootballMatchDto> = await response.json();
    if (!response.ok) {
      const errorMessage = await parseErrorResponse(apiResponse, 'Failed to delete card');
      throw new Error(errorMessage);
    }
    return apiResponse;
  },

  deleteSubstitution: async (matchId: string, substitutionEventId: string): Promise<ApiResponse<FootballMatchDto>> => {
    const url = `${API_URL}/${MATCHES_PATH}/${matchId}/events/substitution/${substitutionEventId}`;
    const response = await authFetch(url, { method: 'DELETE' });
    const apiResponse: ApiResponse<FootballMatchDto> = await response.json();
    if (!response.ok) {
      const errorMessage = await parseErrorResponse(apiResponse, 'Failed to delete substitution');
      throw new Error(errorMessage);
    }
    return apiResponse;
  },

  /**
   * Get matches by season ID
   */
  getBySeason: async (competitionId: string): Promise<ApiResponse<FootballMatchDto[]>> => {
    try {
      const url = `${API_URL}/${MATCHES_PATH}/by-competitionId/${competitionId}`;
      
      const response = await authFetch(url);
      const apiResponse: ApiResponse<FootballMatchDto[]> = await response.json();
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(apiResponse, 'Failed to fetch matches by season');
        throw new Error(errorMessage);
      }
      
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch matches by season');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in footballMatchService.getBySeason:', error);
      throw error;
    }
  },

  /**
   * Get matches by team ID
   */
  getByTeam: async (teamId: string): Promise<ApiResponse<FootballMatchDto[]>> => {
    try {
      const url = `${API_URL}/${MATCHES_PATH}/by-team/${teamId}`;
      
      const response = await authFetch(url);
      const apiResponse: ApiResponse<FootballMatchDto[]> = await response.json();
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(apiResponse, 'Failed to fetch matches by team');
        throw new Error(errorMessage);
      }
      
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch matches by team');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in footballMatchService.getByTeam:', error);
      throw error;
    }
  },

  /**
   * Get today's matches by team ID
   */
  getTodaysMatchesByTeam: async (teamId: string): Promise<ApiResponse<FootballMatchDto[]>> => {
    try {
      const url = `${API_URL}/${MATCHES_PATH}/by-team/${teamId}/today`;
      
      const response = await authFetch(url);
      
      const apiResponse: ApiResponse<FootballMatchDto[]> = await response.json();
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(apiResponse, 'Failed to fetch today\'s matches by team');
        throw new Error(errorMessage);
      }
      
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch today\'s matches by team');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in footballMatchService.getTodaysMatchesByTeam:', error);
      throw error;
    }
  },

  /**
   * Get a Football match by ID
   */
  getById: async (id: string): Promise<ApiResponse<FootballMatchDto>> => {
    try {
      const url = `${API_URL}/${MATCHES_PATH}/by-id/${id}`;
      
      const response = await authFetch(url);
      const apiResponse: ApiResponse<FootballMatchDto> = await response.json();
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(apiResponse, 'Failed to fetch Football match');
        throw new Error(errorMessage);
      }
      
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch Football match');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in footballMatchService.getById:', error);
      throw error;
    }
  },

  /**
   * Create a new Football match
   */
  create: async (data: CreateFootballMatchRequest): Promise<ApiResponse<FootballMatchDto>> => {
    try {
      
      const response = await authFetch(`${API_URL}/${MATCHES_PATH}`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
      });
      const apiResponse: ApiResponse<FootballMatchDto> = await response.json();
      
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(apiResponse, 'Failed to create Football match');
        throw new Error(errorMessage);
      }
      
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to create Football match');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in footballMatchService.create:', error);
      throw error;
    }
  },

  /**
   * Update a Football match
   */
  update: async (data: UpdateFootballMatchRequest): Promise<ApiResponse<FootballMatchDto>> => {
    try {
      
      const response = await authFetch(`${API_URL}/${MATCHES_PATH}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
      });
      const apiResponse: ApiResponse<FootballMatchDto> = await response.json();
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(apiResponse, 'Failed to update Football match');
        throw new Error(errorMessage);
      }
      
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to update Football match');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in footballMatchService.update:', error);
      throw error;
    }
  },

  /**
   * Permanently delete a Football match. Only allowed for matches still in the
   * Scheduled state (server enforces this). Used by the tournament JSON import
   * revert flow to remove freshly created matches.
   */
  delete: async (id: string): Promise<ApiResponse<void>> => {
    try {
      const response = await authFetch(`${API_URL}/${MATCHES_PATH}/${id}`, {
        method: 'DELETE',
      });

      const apiResponse: ApiResponse<void> = await response.json();

      if (!response.ok) {
        const errorMessage = await parseErrorResponse(apiResponse, 'Failed to delete Football match');
        throw new Error(errorMessage);
      }

      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to delete Football match');
      }

      return apiResponse;
    } catch (error) {
      console.error('Error in footballMatchService.delete:', error);
      throw error;
    }
  },

  /**
   * Start a Football match
   */
  start: async (id: string): Promise<ApiResponse<FootballMatchDto>> => {
    try {
      
      const response = await authFetch(`${API_URL}/${MATCHES_PATH}/${id}/start`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
      });
      const apiResponse: ApiResponse<FootballMatchDto> = await response.json();
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(apiResponse, 'Failed to start Football match');
        throw new Error(errorMessage);
      }
      
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to start Football match');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in footballMatchService.start:', error);
      throw error;
    }
  },

  /**
   * Assign or clear the home/away team slots on a scheduled (or postponed) match. Pass
   * `null` for either side to reset that slot back to "to be determined". When the match
   * is a playoff bracket match the change is also automatically propagated forward to the
   * downstream bracket slot (provided the next match has not started yet).
   *
   * Throws on validation failure / 4xx / 5xx with the server's error message so the
   * caller can surface it directly in a toast or banner.
   */
  assignTeams: async (
    id: string,
    payload: AssignMatchTeamsRequest
  ): Promise<ApiResponse<FootballMatchDto>> => {
    try {
      const response = await authFetch(`${API_URL}/${MATCHES_PATH}/${id}/teams`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(payload),
      });

      const apiResponse: ApiResponse<FootballMatchDto> = await response.json();

      if (!response.ok) {
        const errorMessage = await parseErrorResponse(apiResponse, 'Failed to update match teams');
        throw new Error(errorMessage);
      }

      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || apiResponse.message || 'Failed to update match teams');
      }

      return apiResponse;
    } catch (error) {
      console.error('Error in footballMatchService.assignTeams:', error);
      throw error;
    }
  },

  /**
   * Complete a Football match
   */
  complete: async (id: string): Promise<ApiResponse<FootballMatchDto>> => {
    try {
      
      const response = await authFetch(`${API_URL}/${MATCHES_PATH}/${id}/complete`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
      });
      
      const apiResponse: ApiResponse<FootballMatchDto> = await response.json();

      if (!response.ok) {
        const errorMessage = await parseErrorResponse(apiResponse, 'Failed to complete Football match');
        throw new Error(errorMessage);
      }
      
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to complete Football match');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in footballMatchService.complete:', error);
      throw error;
    }
  },

  /**
   * Reopen a previously completed Football match. Reverts the per-match aggregates that the
   * backend applied at completion time (team / player / goalie season stats) and moves the
   * status back to InProgress so the operator can keep editing events or finish the match
   * again. The backend rejects this for playoff matches.
   */
  reopen: async (id: string): Promise<ApiResponse<FootballMatchDto>> => {
    try {

      const response = await authFetch(`${API_URL}/${MATCHES_PATH}/${id}/reopen`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
      });

      const apiResponse: ApiResponse<FootballMatchDto> = await response.json();

      if (!response.ok) {
        const errorMessage = await parseErrorResponse(apiResponse, 'Failed to reopen Football match');
        throw new Error(errorMessage);
      }

      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to reopen Football match');
      }

      return apiResponse;
    } catch (error) {
      console.error('Error in footballMatchService.reopen:', error);
      throw error;
    }
  },

  /**
   * Change match venue
   */
  changeVenue: async (id: string, venue: string): Promise<ApiResponse<FootballMatchDto>> => {
    try {
      // Fetch current match to preserve scheduledDateTime
      const current = await (await authFetch(`${API_URL}/${MATCHES_PATH}/by-id/${id}`)).json() as ApiResponse<FootballMatchDto>;
      if (!current.success || !current.data) {
        throw new Error(current.errors?.join(', ') || 'Failed to fetch current match');
      }
      const response = await authFetch(`${API_URL}/${MATCHES_PATH}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ id, scheduledDateTime: current.data.scheduledDateTime, venue }),
      });
      
      const apiResponse: ApiResponse<FootballMatchDto> = await response.json();
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(apiResponse, 'Failed to change match venue');
        throw new Error(errorMessage);
      }
      
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to change match venue');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in footballMatchService.changeVenue:', error);
      throw error;
    }
  },

  /**
   * Change match date/time
   */
  changeDateTime: async (id: string, scheduledDateTime: string): Promise<ApiResponse<FootballMatchDto>> => {
    try {
      // Fetch current match to preserve venue
      const current = await (await authFetch(`${API_URL}/${MATCHES_PATH}/by-id/${id}`)).json() as ApiResponse<FootballMatchDto>;
      if (!current.success || !current.data) {
        throw new Error(current.errors?.join(', ') || 'Failed to fetch current match');
      }
      const response = await authFetch(`${API_URL}/${MATCHES_PATH}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ id, scheduledDateTime, venue: current.data.venue }),
      });
      const apiResponse: ApiResponse<FootballMatchDto> = await response.json();
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(apiResponse, 'Failed to change match date/time');
        throw new Error(errorMessage);
      }
      
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to change match date/time');
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in footballMatchService.changeDateTime:', error);
      throw error;
    }
  },

  setLineup: async (
    matchId: string,
    teamId: string,
    players: { playerId: string; position: FootballPosition; isOnField: boolean }[]
  ): Promise<ApiResponse<FootballMatchDto>> => {
    const response = await authFetch(`${API_URL}/${MATCHES_PATH}/${matchId}/teams/${teamId}/lineup`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ players })
    });
    const apiResponse: ApiResponse<FootballMatchDto> = await response.json();
    if (!response.ok) {
      const errorMessage = await parseErrorResponse(apiResponse, 'Failed to update lineup');
      throw new Error(errorMessage);
    }
    if (!apiResponse.success) {
      throw new Error(apiResponse.errors?.join(', ') || 'Failed to update lineup');
    }
    return apiResponse;
  },

  /**
   * Change match referee â€” replaces the officials list with this single referee.
   */
  changeReferee: async (matchId: string, refereeId: string): Promise<ApiResponse<FootballMatchDto>> => {
    try {
      const response = await authFetch(`${API_URL}/${MATCHES_PATH}/${matchId}/officials/referee/${refereeId}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
      });

      const raw = await response.text();
      const apiResponse: ApiResponse<FootballMatchDto> = raw ? JSON.parse(raw) : { success: response.ok, data: undefined as unknown as FootballMatchDto, message: '', errors: [] };
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(apiResponse, 'Failed to change match referee');
        throw new Error(errorMessage);
      }

      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to change match referee');
      }
      return apiResponse;
    } catch (error) {
      console.error('Error in footballMatchService.changeReferee:', error);
      throw error;
    }
  },

  /**
   * Update match officials list
   */
  updateOfficials: async (matchId: string, officials: string[]): Promise<ApiResponse<FootballMatchDto>> => {
    try {
      const response = await authFetch(`${API_URL}/${MATCHES_PATH}/${matchId}/officials`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ officials }),
      });

      const raw = await response.text();
      const apiResponse: ApiResponse<FootballMatchDto> = raw ? JSON.parse(raw) : { success: response.ok, data: undefined as unknown as FootballMatchDto, message: '', errors: [] };
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(apiResponse, 'Failed to update match officials');
        throw new Error(errorMessage);
      }

      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to update match officials');
      }
      return apiResponse;
    } catch (error) {
      console.error('Error in footballMatchService.updateOfficials:', error);
      throw error;
    }
  },

  /**
   * Add a single official (append).
   */
  addOfficial: async (matchId: string, refereeId: string): Promise<ApiResponse<FootballMatchDto>> => {
    try {
      const response = await authFetch(`${API_URL}/${MATCHES_PATH}/${matchId}/officials`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ refereeId }),
      });

      const raw = await response.text();
      const apiResponse: ApiResponse<FootballMatchDto> = raw ? JSON.parse(raw) : { success: response.ok, data: undefined as unknown as FootballMatchDto, message: '', errors: [] };
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(apiResponse, 'Failed to add official');
        throw new Error(errorMessage);
      }

      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to add official');
      }
      return apiResponse;
    } catch (error) {
      console.error('Error in footballMatchService.addOfficial:', error);
      throw error;
    }
  },

  /**
   * Delete an official from a match.
   */
  deleteOfficial: async (matchId: string, refereeId: string): Promise<ApiResponse<FootballMatchDto>> => {
    try {
      const response = await authFetch(`${API_URL}/${MATCHES_PATH}/${matchId}/officials/${refereeId}`, {
        method: 'DELETE',
        headers: {
          'Content-Type': 'application/json',
        },
      });

      const raw = await response.text();
      const apiResponse: ApiResponse<FootballMatchDto> = raw ? JSON.parse(raw) : { success: response.ok, data: undefined as unknown as FootballMatchDto, message: '', errors: [] };
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(apiResponse, 'Failed to delete official');
        throw new Error(errorMessage);
      }

      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to delete official');
      }
      return apiResponse;
    } catch (error) {
      console.error('Error in footballMatchService.deleteOfficial:', error);
      throw error;
    }
  }
};
