import type { 
  ApiResponse
} from '../../types/football/footballTypes';
import type { FootballTournamentGroupStandingDto } from '../../types/football/tournamentTypes';
import { API_URL } from '../../constants/config';
import { parseErrorResponse } from '../utils/ParseErrorResponse';

// Statistics DTOs matching the backend soccer shape
export interface FootballPlayerSeasonStatisticsDto {
  id: string;
  playerId: string;
  teamId: string;
  competitionId: string;
  playerName: string;
  teamName: string;
  teamLogo?: string | null;
  seasonName: string;
  gamesPlayed: number;
  goals: number;
  assists: number;
  points: number;
  yellowCards: number;
  redCards: number;
}

export interface FootballTeamSeasonStatisticsDto {
  id: string;
  teamId: string;
  competitionId: string;
  teamName: string;
  teamLogo: string;
  seasonName: string;
  gamesPlayed: number;
  wins: number;
  draws: number;
  losses: number;
  points: number;
  goalsFor: number;
  goalsAgainst: number;
  goalDifference: number;
  homeWins: number;
  homeLosses: number;
  awayWins: number;
  awayLosses: number;
  cleanSheets: number;
  yellowCards: number;
  redCards: number;
  lastFiveForm?: FootballGameResult[];
}

export interface FootballMatchTeamStatisticsDto {
  id: string;
  matchId: string;
  teamId: string;
  teamName: string;
  goals: number;
  yellowCards: number;
  redCards: number;
  substitutions: number;
  cleanSheet: boolean;
}

export interface FootballSeasonStatisticsSummaryDto {
  competitionId: string;
  seasonName: string;
  teamStandings: FootballTeamSeasonStatisticsDto[];
  topScorers: FootballPlayerSeasonStatisticsDto[];
  topAssists: FootballPlayerSeasonStatisticsDto[];
  totalGames: number;
  totalGoals: number;
  averageGoalsPerGame: number;
}

export enum FootballGameResult {
  Win = 'Win',
  Loss = 'Loss',
  Draw = 'Draw'
}

export interface PersonPublicDto {
  id: string | null;
  firstName: string;
  lastName: string;
  birthDate: string | null;
  fullName: string;
  isRegistered: boolean | null;
}

export interface FootballPlayerPublicDto {
  id: string;
  personId: string;
  person: PersonPublicDto;
  isActive: boolean;
  position: string;
  careerGoals: number;
  careerAssists: number;
  team: { id: string; name: string } | null;
}

export interface FootballPlayerProfileDto {
  player: FootballPlayerPublicDto;
  seasonStatistics: FootballPlayerSeasonStatisticsDto[] | null;
}

export const footballStatisticsService = {
  /**
   * Get team statistics for a specific season
   */
  getTeamStatistics: async (competitionId: string, teamId: string): Promise<FootballTeamSeasonStatisticsDto> => {
    try {
      const response = await fetch(`${API_URL}/football/statistics/team/${competitionId}/${teamId}`);
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch team statistics');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FootballTeamSeasonStatisticsDto> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to fetch team statistics'));
      }
      
      return apiResponse.data;
    } catch (error) {
      console.error('Error fetching team statistics:', error);
      throw error;
    }
  },

  /**
   * Get a team's combined statistics aggregated across every competition (regular seasons +
   * tournaments) the team has played in. Backed by /team-aggregate/{teamId}, which exists
   * specifically so the team page's Statistics tab can surface tournament games and points
   * alongside the regular-season totals â€” `getTeamStatistics` is keyed on a single competition
   * and would silently drop those tournament rows.
   */
  getAggregatedTeamStatistics: async (teamId: string): Promise<FootballTeamSeasonStatisticsDto> => {
    try {
      const response = await fetch(`${API_URL}/football/statistics/team-aggregate/${teamId}`);

      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch aggregated team statistics');
        throw new Error(errorMessage);
      }

      const apiResponse: ApiResponse<FootballTeamSeasonStatisticsDto> = await response.json();

      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to fetch aggregated team statistics'));
      }

      return apiResponse.data;
    } catch (error) {
      console.error('Error fetching aggregated team statistics:', error);
      throw error;
    }
  },

  /**
   * Get per-player statistics for a team aggregated across every competition (regular seasons +
   * tournaments) the team has played in. Each player appears once with their summed totals.
   */
  getAggregatedTeamPlayerStatistics: async (teamId: string): Promise<FootballPlayerSeasonStatisticsDto[]> => {
    try {
      const response = await fetch(`${API_URL}/football/statistics/team-players-aggregate/${teamId}`);

      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch aggregated team player statistics');
        throw new Error(errorMessage);
      }

      const apiResponse: ApiResponse<FootballPlayerSeasonStatisticsDto[]> = await response.json();

      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to fetch aggregated team player statistics'));
      }

      return apiResponse.data;
    } catch (error) {
      console.error('Error fetching aggregated team player statistics:', error);
      throw error;
    }
  },

  /**
   * Get all player statistics for a specific team in a season
   */
  getTeamPlayerStatistics: async (competitionId: string, teamId: string): Promise<FootballPlayerSeasonStatisticsDto[]> => {
    try {
      const response = await fetch(`${API_URL}/football/statistics/team-players/${competitionId}/${teamId}`);

      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch team player statistics');
        throw new Error(errorMessage);
      }

      const apiResponse: ApiResponse<FootballPlayerSeasonStatisticsDto[]> = await response.json();

      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to fetch team player statistics'));
      }

      return apiResponse.data;
    } catch (error) {
      console.error('Error fetching team player statistics:', error);
      throw error;
    }
  },

  /**
   * Get player statistics for a specific season
   */
  getPlayerStatistics: async (competitionId: string, playerId: string): Promise<FootballPlayerSeasonStatisticsDto> => {
    try {
      const response = await fetch(`${API_URL}/football/statistics/player/${competitionId}/${playerId}`);
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch player statistics');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FootballPlayerSeasonStatisticsDto> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to fetch player statistics'));
      }
      
      return apiResponse.data;
    } catch (error) {
      console.error('Error fetching player statistics:', error);
      throw error;
    }
  },

  /**
   * Get match statistics for a specific match
   */
  getMatchStatistics: async (matchId: string): Promise<FootballMatchTeamStatisticsDto[]> => {
    try {
      const response = await fetch(`${API_URL}/football/statistics/match/${matchId}`);
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch match statistics');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FootballMatchTeamStatisticsDto[]> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to fetch match statistics'));
      }
      
      return apiResponse.data;
    } catch (error) {
      console.error('Error fetching match statistics:', error);
      throw error;
    }
  },

  /**
   * Get top scorers for a specific season
   */
  getTopScorers: async (competitionId: string, topN: number = 10): Promise<FootballPlayerSeasonStatisticsDto[]> => {
    try {
      const response = await fetch(`${API_URL}/football/statistics/topscorers/${competitionId}?topN=${topN}`);
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch top scorers');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FootballPlayerSeasonStatisticsDto[]> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to fetch top scorers'));
      }
      
      return apiResponse.data;
    } catch (error) {
      console.error('Error fetching top scorers:', error);
      throw error;
    }
  },

  /**
   * Get season statistics summary
   */
  getSeasonStatistics: async (competitionId: string): Promise<FootballSeasonStatisticsSummaryDto> => {
    try {
      const response = await fetch(`${API_URL}/football/statistics/season/${competitionId}`);
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch season statistics');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FootballSeasonStatisticsSummaryDto> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to fetch season statistics'));
      }
      
      return apiResponse.data;
    } catch (error) {
      console.error('Error fetching season statistics:', error);
      throw error;
    }
  },

  /**
   * Get full player profile with all season statistics
   */
  getPlayerProfile: async (playerId: string): Promise<FootballPlayerProfileDto> => {
    try {
      const response = await fetch(`${API_URL}/football/statistics/playerprofile/${playerId}`);

      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch player profile');
        throw new Error(errorMessage);
      }

      const apiResponse: ApiResponse<FootballPlayerProfileDto> = await response.json();

      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to fetch player profile'));
      }

      return apiResponse.data;
    } catch (error) {
      console.error('Error fetching player profile:', error);
      throw error;
    }
  },

  /**
   * Get team standings for a specific season
   */
  getTeamStandings: async (competitionId: string): Promise<FootballTeamSeasonStatisticsDto[]> => {
    try {
      const response = await fetch(`${API_URL}/football/statistics/standings/${competitionId}`);
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch team standings');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FootballTeamSeasonStatisticsDto[]> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to fetch team standings'));
      }
      
      return apiResponse.data;
    } catch (error) {
      console.error('Error fetching team standings:', error);
      throw error;
    }
  },

  /**
   * Get standings for a single tournament group, computed from completed group-stage matches.
   */
  getTournamentGroupStandings: async (groupId: string): Promise<FootballTournamentGroupStandingDto[]> => {
    try {
      const response = await fetch(`${API_URL}/football/statistics/standings/group/${groupId}`);

      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch tournament group standings');
        throw new Error(errorMessage);
      }

      const apiResponse: ApiResponse<FootballTournamentGroupStandingDto[]> = await response.json();

      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to fetch tournament group standings'));
      }

      return apiResponse.data;
    } catch (error) {
      console.error('Error fetching tournament group standings:', error);
      throw error;
    }
  }
};
