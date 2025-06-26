import { floorballTeamService } from './floorballTeamService';
import type { FloorballTeam } from '../../types/floorball/floorballTypes';

export interface DropdownOption {
  id: string;
  name: string;
}

export interface SearchResult {
  data: DropdownOption[];
  pagination: {
    hasNextPage: boolean;
    totalCount: number;
  };
}

export const floorballTeamSearchService = {
  /**
   * Search teams for dropdown with pagination support
   */
  searchTeams: async (query: string = '', page: number = 1): Promise<SearchResult> => {
    try {
      const response = await floorballTeamService.getAll({
        page,
        pageSize: 50, // Use max allowed for teams
        // Note: The backend doesn't support text search yet, so we'll do client-side filtering
      });

      if (!response.success || !response.data) {
        throw new Error('Failed to fetch teams');
      }

      // Convert teams to dropdown options
      let teams: DropdownOption[] = response.data.map((team: FloorballTeam) => ({
        id: team.id,
        name: team.name,
      }));

      // Client-side filtering if query is provided
      if (query.trim()) {
        teams = teams.filter(team => 
          team.name.toLowerCase().includes(query.toLowerCase())
        );
      }

      return {
        data: teams,
        pagination: {
          hasNextPage: response.pagination?.hasNextPage || false,
          totalCount: response.pagination?.totalCount || teams.length,
        },
      };
    } catch (error) {
      throw new Error(error instanceof Error ? error.message : 'Failed to search teams');
    }
  },
};

export const floorballSeasonSearchService = {
  /**
   * Search seasons for dropdown (seasons are typically fewer, so we can load all)
   */
  searchSeasons: async (query: string = '', page: number = 1): Promise<SearchResult> => {
    try {
      // Import the season service here to avoid circular dependencies
      const { floorballSeasonService } = await import('./floorballSeasonService');
      
      const response = await floorballSeasonService.getAll();

      if (!response.success || !response.data) {
        throw new Error('Failed to fetch seasons');
      }

      // Convert seasons to dropdown options
      let seasons: DropdownOption[] = response.data.map((season: any) => ({
        id: season.id,
        name: `${season.year} - ${season.name}`,
      }));

      // Client-side filtering if query is provided
      if (query.trim()) {
        seasons = seasons.filter(season => 
          season.name.toLowerCase().includes(query.toLowerCase())
        );
      }

      // For seasons, we typically show all in one page since there aren't many
      return {
        data: seasons,
        pagination: {
          hasNextPage: false, // All seasons loaded
          totalCount: seasons.length,
        },
      };
    } catch (error) {
      throw new Error(error instanceof Error ? error.message : 'Failed to search seasons');
    }
  },
}; 