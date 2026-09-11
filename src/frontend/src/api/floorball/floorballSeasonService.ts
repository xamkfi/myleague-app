import type { 
  ApiResponse,
  PaginatedApiResponse,
  FloorballTeam,
  FloorballMatchRules
} from '../../types/floorball/floorballTypes';
import type { SeasonContentBlockItem, SeasonContentBlocksDto } from '../../types/common/seasonContent';
import { authFetch } from '../utils/authFetch';
import { parseErrorResponse } from '../utils/ParseErrorResponse';
import { API_URL } from '../../constants/config';

export interface FloorballSeasonDivisionDto {
  divisionId: string;
  teamCount: number;
  teamIds: string[];
}

export interface FloorballSeasonDto {
  id: string;
  name: string;
  startDate: string;
  endDate: string;
  isActive: boolean;
  isCompleted: boolean;
  seasonDivisions: FloorballSeasonDivisionDto[];
  teams: FloorballTeam[];
  matches: unknown[];
  matchRules: FloorballMatchRules;
  teamCategory?: string;
}

export interface FloorballSeasonSummaryDto {
  id: string;
  name: string;
  startDate: string;
  endDate: string;
  isActive: boolean;
  isCompleted: boolean;
  seasonYear: string;
  teamCategory?: string;
}

export interface FloorballSeasonYearDto {
  year: string;
  seasonCount: number;
  hasActiveSeason: boolean;
}

export interface GetFloorballSeasonsPagedParams {
  page?: number;
  pageSize?: number;
  seasonYear?: string;
  teamCategory?: string;
}

export interface CreateFloorballSeasonRequest {
  name: string;
  startDate: string;
  endDate: string;
  divisionIds: string[];
  numberOfPeriods: number;
  periodDurationMinutes: number;
  allowOvertime: boolean;
  overtimeDurationMinutes: number;
  allowShootout: boolean;
  teamCategory?: string;
}

export interface UpdateFloorballSeasonRequest {
  name: string;
  startDate: string;
  endDate: string;
  numberOfPeriods: number;
  periodDurationMinutes: number;
  allowOvertime: boolean;
  overtimeDurationMinutes: number;
  allowShootout: boolean;
  teamCategory?: string;
}

export const floorballSeasonService = {
  /**
   * Get all floorball seasons
   */
  getAll: async (): Promise<ApiResponse<FloorballSeasonDto[]>> => {
    try {
      const url = `${API_URL}/FloorballSeason`;
      
      const response = await authFetch(url);
      
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch floorball seasons');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FloorballSeasonDto[]> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to fetch floorball seasons'));
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballSeasonService.getAll:', error);
      throw error;
    }
  },

  /**
   * Get distinct season years for public navigation
   */
  getYears: async (): Promise<FloorballSeasonYearDto[]> => {
    const response = await authFetch(`${API_URL}/FloorballSeason/years`);
    if (!response.ok) {
      const errorMessage = await parseErrorResponse(response, 'Failed to fetch floorball season years');
      throw new Error(errorMessage);
    }

    const apiResponse: ApiResponse<FloorballSeasonYearDto[]> = await response.json();
    if (!apiResponse.success) {
      throw new Error(await parseErrorResponse(apiResponse, 'Failed to fetch floorball season years'));
    }

    return apiResponse.data ?? [];
  },

  /**
   * Get paginated slim season list (optional season-year filter)
   */
  getPaged: async (
    params: GetFloorballSeasonsPagedParams = {}
  ): Promise<PaginatedApiResponse<FloorballSeasonSummaryDto>> => {
    const searchParams = new URLSearchParams();
    searchParams.set('page', String(params.page ?? 1));
    searchParams.set('pageSize', String(params.pageSize ?? 6));
    if (params.seasonYear) {
      searchParams.set('seasonYear', params.seasonYear);
    }
    if (params.teamCategory) {
      searchParams.set('teamCategory', params.teamCategory);
    }

    const response = await authFetch(`${API_URL}/FloorballSeason/paged?${searchParams.toString()}`);
    if (!response.ok) {
      const errorMessage = await parseErrorResponse(response, 'Failed to fetch floorball seasons');
      throw new Error(errorMessage);
    }

    const apiResponse: PaginatedApiResponse<FloorballSeasonSummaryDto> = await response.json();
    if (!apiResponse.success) {
      throw new Error(await parseErrorResponse(apiResponse, 'Failed to fetch floorball seasons'));
    }

    return apiResponse;
  },

  /**
   * Get active floorball seasons
   */
  getActive: async (): Promise<ApiResponse<FloorballSeasonDto[]>> => {
    try {
      const url = `${API_URL}/FloorballSeason/active`;
      
      const response = await authFetch(url);
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch active floorball seasons');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FloorballSeasonDto[]> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to fetch active floorball seasons'));
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballSeasonService.getActive:', error);
      throw error;
    }
  },

  /**
   * Get a floorball season by ID
   */
  getById: async (id: string): Promise<ApiResponse<FloorballSeasonDto>> => {
    try {
      const url = `${API_URL}/FloorballSeason/${id}`;
      
      const response = await authFetch(url);
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch floorball season');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FloorballSeasonDto> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to fetch floorball season'));
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballSeasonService.getById:', error);
      throw error;
    }
  },

  /**
   * Create a new floorball season
   */
  create: async (data: CreateFloorballSeasonRequest): Promise<ApiResponse<FloorballSeasonDto>> => {
    try {
      
      const response = await authFetch(`${API_URL}/FloorballSeason`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
      });
      
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to create floorball season');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FloorballSeasonDto> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to create floorball season'));
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballSeasonService.create:', error);
      throw error;
    }
  },

  /**
   * Update a floorball season
   */
  update: async (id: string, data: UpdateFloorballSeasonRequest): Promise<ApiResponse<FloorballSeasonDto>> => {
    const response = await authFetch(`${API_URL}/FloorballSeason/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(data),
    });

    if (!response.ok) {
      const errorMessage = await parseErrorResponse(response, 'Failed to update floorball season');
      throw new Error(errorMessage);
    }

    const apiResponse: ApiResponse<FloorballSeasonDto> = await response.json();

    if (!apiResponse.success) {
      throw new Error(await parseErrorResponse(apiResponse, 'Failed to update floorball season'));
    }

    return apiResponse;
  },

  /**
   * Delete a floorball season
   */
  delete: async (id: string): Promise<void> => {
    try {
      
      const response = await authFetch(`${API_URL}/FloorballSeason/${id}`, {
        method: 'DELETE',
      });
      
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to delete floorball season');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<void> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to delete floorball season'));
      }
    } catch (error) {
      console.error('Error in floorballSeasonService.delete:', error);
      throw error;
    }
  },

  /**
   * Activate a floorball season
   */
  activate: async (id: string): Promise<ApiResponse<FloorballSeasonDto>> => {
    try {
      
      const response = await authFetch(`${API_URL}/FloorballSeason/${id}/activate`, {
        method: 'PUT',
      });
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to activate floorball season');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FloorballSeasonDto> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to activate floorball season'));
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballSeasonService.activate:', error);
      throw error;
    }
  },

  /**
   * Deactivate a floorball season
   */
  deactivate: async (id: string): Promise<ApiResponse<FloorballSeasonDto>> => {
    try {
      
      const response = await authFetch(`${API_URL}/FloorballSeason/${id}/deactivate`, {
        method: 'PUT',
      });
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to deactivate floorball season');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FloorballSeasonDto> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to deactivate floorball season'));
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballSeasonService.deactivate:', error);
      throw error;
    }
  },

  /**
   * Complete a floorball season
   */
  complete: async (id: string): Promise<ApiResponse<FloorballSeasonDto>> => {
    try {
      
      const response = await authFetch(`${API_URL}/FloorballSeason/${id}/complete`, {
        method: 'PUT',
      });
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to complete floorball season');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FloorballSeasonDto> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to complete floorball season'));
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballSeasonService.complete:', error);
      throw error;
    }
  },

  /**
   * Add a team to a floorball season
   */
  addTeamToSeason: async (competitionId: string, teamId: string): Promise<ApiResponse<FloorballSeasonDto>> => {
    try {
      
      const response = await authFetch(`${API_URL}/FloorballSeason/${competitionId}/teams/${teamId}`, {
        method: 'POST',
      });
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to add team to season');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FloorballSeasonDto> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to add team to season'));
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballSeasonService.addTeamToSeason:', error);
      throw error;
    }
  },

  /**
   * Remove a team from a floorball season
   */
  removeTeamFromSeason: async (competitionId: string, teamId: string): Promise<ApiResponse<FloorballSeasonDto>> => {
    try {
      
      const response = await authFetch(`${API_URL}/FloorballSeason/${competitionId}/teams/${teamId}`, {
        method: 'DELETE',
      });
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to remove team from season');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FloorballSeasonDto> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to remove team from season'));
      }
      
      return apiResponse;
    } catch (error) {
      console.error('Error in floorballSeasonService.removeTeamFromSeason:', error);
      throw error;
    }
  },

  /**
   * Add a division to a floorball season
   */
  addDivisionToSeason: async (competitionId: string, divisionId: string): Promise<ApiResponse<void>> => {
    try {
      
      const response = await authFetch(`${API_URL}/FloorballSeason/${competitionId}/divisions/${divisionId}`, {
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
      console.error('Error in floorballSeasonService.addDivisionToSeason:', error);
      throw error;
    }
  },

  /**
   * Remove a division from a floorball season
   */
  removeDivisionFromSeason: async (competitionId: string, divisionId: string): Promise<ApiResponse<void>> => {
    try {
      
      const response = await authFetch(`${API_URL}/FloorballSeason/${competitionId}/divisions/${divisionId}`, {
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
      console.error('Error in floorballSeasonService.removeDivisionFromSeason:', error);
      throw error;
    }
  },

  /**
   * Add a team to a specific division of a floorball season
   */
  addTeamToSeasonDivision: async (competitionId: string, divisionId: string, teamId: string): Promise<ApiResponse<void>> => {
    try {
      
      const response = await authFetch(`${API_URL}/FloorballSeason/${competitionId}/divisions/${divisionId}/teams/${teamId}`, {
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
      console.error('Error in floorballSeasonService.addTeamToSeasonDivision:', error);
      throw error;
    }
  },

  /**
   * Remove a team from a specific division of a floorball season
   */
  removeTeamFromSeasonDivision: async (competitionId: string, divisionId: string, teamId: string): Promise<ApiResponse<void>> => {
    try {
      
      const response = await authFetch(`${API_URL}/FloorballSeason/${competitionId}/divisions/${divisionId}/teams/${teamId}`, {
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
      console.error('Error in floorballSeasonService.removeTeamFromSeasonDivision:', error);
      throw error;
    }
  },

  getContentBlocks: async (seasonId: string): Promise<SeasonContentBlocksDto> => {
    const response = await authFetch(`${API_URL}/FloorballSeason/${seasonId}/content-blocks`);
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
      `${API_URL}/FloorballSeason/content-blocks${query ? `?${query}` : ''}`,
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
    const response = await authFetch(`${API_URL}/FloorballSeason/${seasonId}/content-blocks`, {
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