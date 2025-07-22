import type { ApiResponse } from '../../types/floorball/floorballTypes';

const API_URL = import.meta.env.VITE_API_URL || '/api';

export interface TeamNameResult {
  id: string;
  name: string;
}

export interface DropdownOption {
  id: string;
  name: string;
  [key: string]: unknown;
}

export interface SearchResult {
  data: DropdownOption[];
  pagination: {
    hasNextPage: boolean;
    totalCount: number;
  };
}

export const floorballTeamNameSearchService = {
  /**
   * Get team names filtered by name filter (non-paginated backend call)
   */
  getTeamNames: async (nameFilter: string): Promise<ApiResponse<TeamNameResult[]>> => {
    const searchParams = new URLSearchParams();
    if (nameFilter) {
      searchParams.append('nameFilter', nameFilter);
    }

    const url = `${API_URL}/FloorballTeam/names${searchParams.toString() ? `?${searchParams.toString()}` : ''}`;

    const response = await fetch(url);
    if (!response.ok) {
      throw new Error('Failed to fetch team names');
    }

    const apiResponse: ApiResponse<TeamNameResult[]> = await response.json();
    if (!apiResponse.success) {
      throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch team names');
    }

    return apiResponse;
  },

  /**
   * Search team names for dropdown with simulated pagination
   */
  searchTeams: async (query: string, page: number): Promise<SearchResult> => {
    try {
      const response = await floorballTeamNameSearchService.getTeamNames(query);

      if (!response.success || !response.data) {
        throw new Error('Failed to fetch team names');
      }

      const pageSize = 50;
      const allTeams = response.data.map(team => ({
        id: team.id,
        name: team.name,
      }));

      const filteredTeams = query.trim()
        ? allTeams.filter(team =>
            team.name.toLowerCase().includes(query.toLowerCase())
          )
        : allTeams;

      const startIndex = (page - 1) * pageSize;
      const pagedTeams = filteredTeams.slice(startIndex, startIndex + pageSize);

      return {
        data: pagedTeams,
        pagination: {
          hasNextPage: startIndex + pageSize < filteredTeams.length,
          totalCount: filteredTeams.length,
        },
      };
    } catch (error) {
      throw new Error(error instanceof Error ? error.message : 'Failed to search team names');
    }
  },
};
