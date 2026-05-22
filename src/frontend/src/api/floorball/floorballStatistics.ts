import type { 
  ApiResponse
} from '../../types/floorball/floorballTypes';
import type { FloorballTournamentGroupStandingDto } from '../../types/floorball/tournamentTypes';
import { API_URL } from '../../constants/config';
import { parseErrorResponse } from '../utils/ParseErrorResponse';

// Statistics DTOs matching the backend
export interface FloorballPlayerSeasonStatisticsDto {
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
  penaltyMinutes: number;
  plusMinusRating: number;
  shotsOnGoal: number;
  shotPercentage: number;
  powerPlayGoals: number;
  powerPlayAssists: number;
  shortHandedGoals: number;
  shortHandedAssists: number;
  gameWinningGoals: number;
  overtimeGoals: number;
  faceoffWins: number;
  faceoffAttempts: number;
  faceoffPercentage: number;
}

export interface FloorballTeamSeasonStatisticsDto {
  id: string;
  teamId: string;
  competitionId: string;
  teamName: string;
  teamLogo: string;
  seasonName: string;
  gamesPlayed: number;
  wins: number;
  losses: number;
  ties: number;
  points: number;
  goalsFor: number;
  goalsAgainst: number;
  goalDifference: number;
  shotsFor: number;
  shotsAgainst: number;
  shotPercentage: number;
  powerPlayGoals: number;
  powerPlayOpportunities: number;
  powerPlayPercentage: number;
  shortHandedGoals: number;
  penaltyKillOpportunities: number;
  penaltyKillPercentage: number;
  penaltyMinutes: number;
  faceoffWins: number;
  faceoffAttempts: number;
  faceoffPercentage: number;
  homeWins: number;
  homeLosses: number;
  awayWins: number;
  awayLosses: number;
  lastFiveForm?: FloorballGameResult[];
}

export interface FloorballMatchTeamStatisticsDto {
  id: string;
  matchId: string;
  teamId: string;
  teamName: string;
  shotsOnGoal: number;
  shotsTotal: number;
  shotPercentage: number;
  faceoffWins: number;
  faceoffAttempts: number;
  faceoffPercentage: number;
  powerPlayOpportunities: number;
  powerPlayGoals: number;
  powerPlayMinutes: number;
  penaltyKillOpportunities: number;
  penaltyKillSuccess: number;
  shortHandedGoals: number;
  penaltyMinutes: number;
  hits: number;
  blockedShots: number;
  takeaways: number;
  giveaways: number;
}

export interface FloorballGoalieSeasonStatisticsDto {
  id: string;
  playerId: string;
  teamId: string;
  competitionId: string;
  playerName: string;
  teamName: string;
  seasonName: string;
  gamesPlayed: number;
  gamesStarted: number;
  wins: number;
  losses: number;
  ties: number;
  saves: number;
  shotsAgainst: number;
  savePercentage: number;
  goalsAgainst: number;
  goalsAgainstAverage: number;
  shutouts: number;
  minutesPlayed: number;
  powerPlaySaves: number;
  powerPlayShotsAgainst: number;
  powerPlaySavePercentage: number;
  shortHandedSaves: number;
  shortHandedShotsAgainst: number;
  shortHandedSavePercentage: number;
}

export interface FloorballSeasonStatisticsSummaryDto {
  competitionId: string;
  seasonName: string;
  startDate: string;
  endDate: string;
  teamStandings: FloorballTeamSeasonStatisticsDto[];
  topScorers: FloorballPlayerSeasonStatisticsDto[];
  topAssists: FloorballPlayerSeasonStatisticsDto[];
  topGoalies: FloorballGoalieSeasonStatisticsDto[];
  totalGames: number;
  totalGoals: number;
  averageGoalsPerGame: number;
}

export enum FloorballGameResult {
  Win = 'Win',
  Loss = 'Loss', 
  Tie = 'Tie'
}

export interface PersonPublicDto {
  id: string | null;
  firstName: string;
  lastName: string;
  birthDate: string | null;
  fullName: string;
  isRegistered: boolean | null;
}

export interface FloorballPlayerPublicDto {
  id: string;
  personId: string;
  person: PersonPublicDto;
  isActive: boolean;
  position: string;
  careerGoals: number;
  careerAssists: number;
  team: { id: string; name: string } | null;
}

export interface FloorballPlayerProfileDto {
  player: FloorballPlayerPublicDto;
  seasonStatistics: FloorballPlayerSeasonStatisticsDto[] | null;
  seasonStatisticsForGoalie: FloorballGoalieSeasonStatisticsDto[] | null;
}

export const floorballStatisticsService = {
  /**
   * Get team statistics for a specific season
   */
  getTeamStatistics: async (competitionId: string, teamId: string): Promise<FloorballTeamSeasonStatisticsDto> => {
    try {
      const response = await fetch(`${API_URL}/floorball/statistics/team/${competitionId}/${teamId}`);
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch team statistics');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FloorballTeamSeasonStatisticsDto> = await response.json();
      
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
   * alongside the regular-season totals — `getTeamStatistics` is keyed on a single competition
   * and would silently drop those tournament rows.
   */
  getAggregatedTeamStatistics: async (teamId: string): Promise<FloorballTeamSeasonStatisticsDto> => {
    try {
      const response = await fetch(`${API_URL}/floorball/statistics/team-aggregate/${teamId}`);

      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch aggregated team statistics');
        throw new Error(errorMessage);
      }

      const apiResponse: ApiResponse<FloorballTeamSeasonStatisticsDto> = await response.json();

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
  getAggregatedTeamPlayerStatistics: async (teamId: string): Promise<FloorballPlayerSeasonStatisticsDto[]> => {
    try {
      const response = await fetch(`${API_URL}/floorball/statistics/team-players-aggregate/${teamId}`);

      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch aggregated team player statistics');
        throw new Error(errorMessage);
      }

      const apiResponse: ApiResponse<FloorballPlayerSeasonStatisticsDto[]> = await response.json();

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
  getTeamPlayerStatistics: async (competitionId: string, teamId: string): Promise<FloorballPlayerSeasonStatisticsDto[]> => {
    try {
      const response = await fetch(`${API_URL}/floorball/statistics/team-players/${competitionId}/${teamId}`);

      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch team player statistics');
        throw new Error(errorMessage);
      }

      const apiResponse: ApiResponse<FloorballPlayerSeasonStatisticsDto[]> = await response.json();

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
  getPlayerStatistics: async (competitionId: string, playerId: string): Promise<FloorballPlayerSeasonStatisticsDto> => {
    try {
      const response = await fetch(`${API_URL}/floorball/statistics/player/${competitionId}/${playerId}`);
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch player statistics');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FloorballPlayerSeasonStatisticsDto> = await response.json();
      
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
  getMatchStatistics: async (matchId: string): Promise<FloorballMatchTeamStatisticsDto[]> => {
    try {
      const response = await fetch(`${API_URL}/floorball/statistics/match/${matchId}`);
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch match statistics');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FloorballMatchTeamStatisticsDto[]> = await response.json();
      
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
  getTopScorers: async (competitionId: string, topN: number = 10): Promise<FloorballPlayerSeasonStatisticsDto[]> => {
    try {
      const response = await fetch(`${API_URL}/floorball/statistics/topscorers/${competitionId}?topN=${topN}`);
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch top scorers');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FloorballPlayerSeasonStatisticsDto[]> = await response.json();
      
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
  getSeasonStatistics: async (competitionId: string): Promise<FloorballSeasonStatisticsSummaryDto> => {
    try {
      const response = await fetch(`${API_URL}/floorball/statistics/season/${competitionId}`);
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch season statistics');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FloorballSeasonStatisticsSummaryDto> = await response.json();
      
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
  getPlayerProfile: async (playerId: string): Promise<FloorballPlayerProfileDto> => {
    try {
      const response = await fetch(`${API_URL}/floorball/statistics/playerprofile/${playerId}`);

      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch player profile');
        throw new Error(errorMessage);
      }

      const apiResponse: ApiResponse<FloorballPlayerProfileDto> = await response.json();

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
  getTeamStandings: async (competitionId: string): Promise<FloorballTeamSeasonStatisticsDto[]> => {
    try {
      const response = await fetch(`${API_URL}/floorball/statistics/standings/${competitionId}`);
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch team standings');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FloorballTeamSeasonStatisticsDto[]> = await response.json();
      
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
  getTournamentGroupStandings: async (groupId: string): Promise<FloorballTournamentGroupStandingDto[]> => {
    try {
      const response = await fetch(`${API_URL}/floorball/statistics/standings/group/${groupId}`);

      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch tournament group standings');
        throw new Error(errorMessage);
      }

      const apiResponse: ApiResponse<FloorballTournamentGroupStandingDto[]> = await response.json();

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
