import type { 
  ApiResponse,
  PaginatedApiResponse,
  FootballTeam,
  FootballMatchRules
} from '../../types/football/footballTypes';
import type { SeasonContentBlockItem, SeasonContentBlocksDto } from '../../types/common/seasonContent';
import { authFetch } from '../utils/authFetch';
import { parseErrorResponse } from '../utils/ParseErrorResponse';
import { API_URL } from '../../constants/config';

export interface FootballSeasonDivisionDto {
  divisionId: string;
  teamCount: number;
  teamIds: string[];
}

export interface FootballStandingRulesDto {
  winPoints: number;
  drawPoints: number;
  lossPoints: number;
}

export interface FootballSeasonDto {
  id: string;
  name: string;
  startDate: string;
  endDate: string;
  isActive: boolean;
  isCompleted: boolean;
  seasonDivisions: FootballSeasonDivisionDto[];
  teams: FootballTeam[];
  matches: unknown[];
  matchRules: FootballMatchRules;
  standingRules?: FootballStandingRulesDto;
  teamCategory?: string;
}

export interface FootballSeasonSummaryDto {
  id: string;
  name: string;
  startDate: string;
  endDate: string;
  isActive: boolean;
  isCompleted: boolean;
  seasonYear: string;
  teamCategory?: string;
}

export interface FootballSeasonYearDto {
  year: string;
  seasonCount: number;
  hasActiveSeason: boolean;
}

export interface GetFootballSeasonsPagedParams {
  page?: number;
  pageSize?: number;
  seasonYear?: string;
  teamCategory?: string;
}

export interface CreateFootballSeasonRequest {
  name: string;
  startDate: string;
  endDate: string;
  divisionIds: string[];
  numberOfHalves: number;
  halfDurationMinutes: number;
  playersOnField: number;
  requireGoalkeeper: boolean;
  maxSubstitutions: number;
  requireOfficialsToStart: boolean;
  allowExtraTime: boolean;
  extraTimeHalfCount: number;
  extraTimeHalfDurationMinutes: number;
  allowPenaltyShootout: boolean;
  winPoints: number;
  drawPoints: number;
  lossPoints: number;
  teamCategory?: string;
}

export const FOOTBALL_HOBBY_MATCH_RULE_DEFAULTS = {
  numberOfHalves: 2,
  halfDurationMinutes: 20,
  playersOnField: 5,
  requireGoalkeeper: true,
  maxSubstitutions: 99,
  requireOfficialsToStart: false,
  allowExtraTime: false,
  extraTimeHalfCount: 2,
  extraTimeHalfDurationMinutes: 5,
  allowPenaltyShootout: false,
};

export const FOOTBALL_HOBBY_STANDING_RULE_DEFAULTS = {
  winPoints: 3,
  drawPoints: 1,
  lossPoints: 0,
};

export interface UpdateFootballSeasonRequest {
  name: string;
  startDate: string;
  endDate: string;
  numberOfHalves: number;
  halfDurationMinutes: number;
  playersOnField: number;
  requireGoalkeeper: boolean;
  maxSubstitutions: number;
  requireOfficialsToStart: boolean;
  allowExtraTime: boolean;
  extraTimeHalfCount: number;
  extraTimeHalfDurationMinutes: number;
  allowPenaltyShootout: boolean;
  winPoints: number;
  drawPoints: number;
  lossPoints: number;
  teamCategory?: string;
}

export const footballSeasonService = {
  /**
   * Get all Football seasons
   */
  getAll: async (): Promise<ApiResponse<FootballSeasonDto[]>> => {
    try {
      const url = `${API_URL}/FootballSeason`;
      
      const response = await authFetch(url);
      
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch Football seasons');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FootballSeasonDto[]> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to fetch Football seasons'));
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in footballSeasonService.getAll:', error);
      throw error;
    }
  },

  /**
   * Get distinct season years for public navigation
   */
  getYears: async (): Promise<FootballSeasonYearDto[]> => {
    const response = await authFetch(`${API_URL}/FootballSeason/years`);
    if (!response.ok) {
      const errorMessage = await parseErrorResponse(response, 'Failed to fetch Football season years');
      throw new Error(errorMessage);
    }

    const apiResponse: ApiResponse<FootballSeasonYearDto[]> = await response.json();
    if (!apiResponse.success) {
      throw new Error(await parseErrorResponse(apiResponse, 'Failed to fetch Football season years'));
    }

    return apiResponse.data ?? [];
  },

  /**
   * Get paginated slim season list (optional season-year filter)
   */
  getPaged: async (
    params: GetFootballSeasonsPagedParams = {}
  ): Promise<PaginatedApiResponse<FootballSeasonSummaryDto>> => {
    const searchParams = new URLSearchParams();
    searchParams.set('page', String(params.page ?? 1));
    searchParams.set('pageSize', String(params.pageSize ?? 6));
    if (params.seasonYear) {
      searchParams.set('seasonYear', params.seasonYear);
    }
    if (params.teamCategory) {
      searchParams.set('teamCategory', params.teamCategory);
    }

    const response = await authFetch(`${API_URL}/FootballSeason/paged?${searchParams.toString()}`);
    if (!response.ok) {
      const errorMessage = await parseErrorResponse(response, 'Failed to fetch Football seasons');
      throw new Error(errorMessage);
    }

    const apiResponse: PaginatedApiResponse<FootballSeasonSummaryDto> = await response.json();
    if (!apiResponse.success) {
      throw new Error(await parseErrorResponse(apiResponse, 'Failed to fetch Football seasons'));
    }

    return apiResponse;
  },

  /**
   * Get active Football seasons
   */
  getActive: async (): Promise<ApiResponse<FootballSeasonDto[]>> => {
    try {
      const url = `${API_URL}/FootballSeason/active`;
      
      const response = await authFetch(url);
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch active Football seasons');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FootballSeasonDto[]> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to fetch active Football seasons'));
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in footballSeasonService.getActive:', error);
      throw error;
    }
  },

  /**
   * Get a Football season by ID
   */
  getById: async (id: string): Promise<ApiResponse<FootballSeasonDto>> => {
    try {
      const url = `${API_URL}/FootballSeason/${id}`;
      
      const response = await authFetch(url);
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch Football season');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FootballSeasonDto> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to fetch Football season'));
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in footballSeasonService.getById:', error);
      throw error;
    }
  },

  /**
   * Create a new Football season
   */
  create: async (data: CreateFootballSeasonRequest): Promise<ApiResponse<FootballSeasonDto>> => {
    try {
      
      const response = await authFetch(`${API_URL}/FootballSeason`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
      });
      
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to create Football season');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FootballSeasonDto> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to create Football season'));
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in footballSeasonService.create:', error);
      throw error;
    }
  },

  /**
   * Update a Football season
   */
  update: async (id: string, data: UpdateFootballSeasonRequest): Promise<ApiResponse<FootballSeasonDto>> => {
    try {
      
      const response = await authFetch(`${API_URL}/FootballSeason/${id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
      });
      
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to update Football season');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FootballSeasonDto> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to update Football season'));
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in footballSeasonService.update:', error);
      throw error;
    }
  },

  /**
   * Delete a Football season
   */
  delete: async (id: string): Promise<void> => {
    try {
      
      const response = await authFetch(`${API_URL}/FootballSeason/${id}`, {
        method: 'DELETE',
      });
      
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to delete Football season');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<void> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to delete Football season'));
      }
    } catch (error) {
      console.error('Error in footballSeasonService.delete:', error);
      throw error;
    }
  },

  /**
   * Activate a Football season
   */
  activate: async (id: string): Promise<ApiResponse<FootballSeasonDto>> => {
    try {
      
      const response = await authFetch(`${API_URL}/FootballSeason/${id}/activate`, {
        method: 'PUT',
      });
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to activate Football season');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FootballSeasonDto> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to activate Football season'));
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in footballSeasonService.activate:', error);
      throw error;
    }
  },

  /**
   * Deactivate a Football season
   */
  deactivate: async (id: string): Promise<ApiResponse<FootballSeasonDto>> => {
    try {
      
      const response = await authFetch(`${API_URL}/FootballSeason/${id}/deactivate`, {
        method: 'PUT',
      });
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to deactivate Football season');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FootballSeasonDto> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to deactivate Football season'));
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in footballSeasonService.deactivate:', error);
      throw error;
    }
  },

  /**
   * Complete a Football season
   */
  complete: async (id: string): Promise<ApiResponse<FootballSeasonDto>> => {
    try {
      
      const response = await authFetch(`${API_URL}/FootballSeason/${id}/complete`, {
        method: 'PUT',
      });
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to complete Football season');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FootballSeasonDto> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to complete Football season'));
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in footballSeasonService.complete:', error);
      throw error;
    }
  },

  /**
   * Add a team to a Football season
   */
  addTeamToSeason: async (competitionId: string, teamId: string): Promise<ApiResponse<FootballSeasonDto>> => {
    try {
      
      const response = await authFetch(`${API_URL}/FootballSeason/${competitionId}/teams/${teamId}`, {
        method: 'POST',
      });
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to add team to season');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FootballSeasonDto> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to add team to season'));
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in footballSeasonService.addTeamToSeason:', error);
      throw error;
    }
  },

  /**
   * Remove a team from a Football season
   */
  removeTeamFromSeason: async (competitionId: string, teamId: string): Promise<ApiResponse<FootballSeasonDto>> => {
    try {
      
      const response = await authFetch(`${API_URL}/FootballSeason/${competitionId}/teams/${teamId}`, {
        method: 'DELETE',
      });
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to remove team from season');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FootballSeasonDto> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to remove team from season'));
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in footballSeasonService.removeTeamFromSeason:', error);
      throw error;
    }
  },

  /**
   * Add a division to a Football season
   */
  addDivisionToSeason: async (competitionId: string, divisionId: string): Promise<ApiResponse<void>> => {
    try {
      
      const response = await authFetch(`${API_URL}/FootballSeason/${competitionId}/divisions/${divisionId}`, {
        method: 'POST',
      });
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to add division to season');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<void> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to add division to season'));
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in footballSeasonService.addDivisionToSeason:', error);
      throw error;
    }
  },

  /**
   * Remove a division from a Football season
   */
  removeDivisionFromSeason: async (competitionId: string, divisionId: string): Promise<ApiResponse<void>> => {
    try {
      
      const response = await authFetch(`${API_URL}/FootballSeason/${competitionId}/divisions/${divisionId}`, {
        method: 'DELETE',
      });
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to remove division from season');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<void> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to remove division from season'));
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in footballSeasonService.removeDivisionFromSeason:', error);
      throw error;
    }
  },

  /**
   * Add a team to a specific division of a Football season
   */
  addTeamToSeasonDivision: async (competitionId: string, divisionId: string, teamId: string): Promise<ApiResponse<void>> => {
    try {
      
      const response = await authFetch(`${API_URL}/FootballSeason/${competitionId}/divisions/${divisionId}/teams/${teamId}`, {
        method: 'POST',
      });
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to add team to season division');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<void> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to add team to season division'));
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in footballSeasonService.addTeamToSeasonDivision:', error);
      throw error;
    }
  },

  /**
   * Remove a team from a specific division of a Football season
   */
  removeTeamFromSeasonDivision: async (competitionId: string, divisionId: string, teamId: string): Promise<ApiResponse<void>> => {
    try {
      
      const response = await authFetch(`${API_URL}/FootballSeason/${competitionId}/divisions/${divisionId}/teams/${teamId}`, {
        method: 'DELETE',
      });
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to remove team from season division');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<void> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to remove team from season division'));
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in footballSeasonService.removeTeamFromSeasonDivision:', error);
      throw error;
    }
  },

  getContentBlocks: async (seasonId: string): Promise<SeasonContentBlocksDto> => {
    const response = await authFetch(`${API_URL}/FootballSeason/${seasonId}/content-blocks`);
    if (!response.ok) {
      throw new Error(await parseErrorResponse(response, 'Failed to fetch season content blocks'));
    }
    const apiResponse: ApiResponse<SeasonContentBlocksDto> = await response.json();
    if (!apiResponse.success || !apiResponse.data) {
      throw new Error(await parseErrorResponse(apiResponse, 'Failed to fetch season content blocks'));
    }
    return apiResponse.data;
  },

  getFeaturedContentBlocks: async (seasonYear?: string): Promise<SeasonContentBlocksDto> => {
    const searchParams = new URLSearchParams();
    if (seasonYear) {
      searchParams.set('seasonYear', seasonYear);
    }
    const query = searchParams.toString();
    const response = await authFetch(
      `${API_URL}/FootballSeason/content-blocks${query ? `?${query}` : ''}`,
    );
    if (!response.ok) {
      throw new Error(await parseErrorResponse(response, 'Failed to fetch season content blocks'));
    }
    const apiResponse: ApiResponse<SeasonContentBlocksDto> = await response.json();
    if (!apiResponse.success || !apiResponse.data) {
      throw new Error(await parseErrorResponse(apiResponse, 'Failed to fetch season content blocks'));
    }
    return apiResponse.data;
  },

  replaceContentBlocks: async (
    seasonId: string,
    items: SeasonContentBlockItem[],
  ): Promise<SeasonContentBlocksDto> => {
    const response = await authFetch(`${API_URL}/FootballSeason/${seasonId}/content-blocks`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ items }),
    });
    if (!response.ok) {
      throw new Error(await parseErrorResponse(response, 'Failed to update season content blocks'));
    }
    const apiResponse: ApiResponse<SeasonContentBlocksDto> = await response.json();
    if (!apiResponse.success || !apiResponse.data) {
      throw new Error(await parseErrorResponse(apiResponse, 'Failed to update season content blocks'));
    }
    return apiResponse.data;
  },
}; 