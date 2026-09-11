import { floorballTeamService } from './floorballTeamService';
import type { FloorballTeam } from '../../types/floorball/floorballTypes';
import type { FloorballSeasonDto } from './floorballSeasonService';

export interface DropdownOption {
  id: string;
  name: string;
  [key: string]: unknown; // Allow additional properties
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
  searchTeams: async (query: string, page: number): Promise<SearchResult> => {
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
  searchSeasons: async (query: string, page: number): Promise<SearchResult> => {
    try {
      // Note: Since seasons are typically few, we load all and ignore pagination
      // but keep the page parameter for interface compatibility
      console.debug(`Searching seasons - page: ${page}, query: "${query}"`);
      
      // Import the season service here to avoid circular dependencies
      const { floorballSeasonService } = await import('./floorballSeasonService');
      
      const response = await floorballSeasonService.getAll();

      if (!response.success || !response.data) {
        throw new Error('Failed to fetch seasons');
      }

      // Convert seasons to dropdown options
      let seasons: DropdownOption[] = response.data.map((season: FloorballSeasonDto) => ({
        id: season.id,
        name: season.name,
      }));

      // Client-side filtering if query is provided
      if (query.trim()) {
        seasons = seasons.filter(season => 
          season.name.toLowerCase().includes(query.toLowerCase())
        );
      }

      // For seasons, we typically show all in one page since there aren't many
      // Note: page parameter is required by interface but not used since we load all seasons
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

export const floorballTournamentSearchService = {
  /**
   * Search tournaments for dropdown (typically few, so we can load all and filter client-side
   * — same pattern as the season search service).
   */
  searchTournaments: async (query: string, page: number): Promise<SearchResult> => {
    try {
      console.debug(`Searching tournaments - page: ${page}, query: "${query}"`);

      const { floorballTournamentService } = await import('./floorballTournamentService');
      const response = await floorballTournamentService.getAll();

      if (!response.success || !response.data) {
        throw new Error('Failed to fetch tournaments');
      }

      let tournaments: DropdownOption[] = response.data.map((tournament) => ({
        id: tournament.id,
        name: tournament.name,
      }));

      if (query.trim()) {
        tournaments = tournaments.filter((tournament) =>
          tournament.name.toLowerCase().includes(query.toLowerCase())
        );
      }

      return {
        data: tournaments,
        pagination: {
          hasNextPage: false,
          totalCount: tournaments.length,
        },
      };
    } catch (error) {
      throw new Error(error instanceof Error ? error.message : 'Failed to search tournaments');
    }
  },
};
