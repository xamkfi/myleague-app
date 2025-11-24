import type { 
  ApiResponse
} from '../../types/floorball/floorballTypes';
import { VITE_API_URL } from '../../constants/config';

const API_URL = VITE_API_URL;

// Statistics DTOs matching the backend
export interface FloorballPlayerSeasonStatisticsDto {
  id: string;
  playerId: string;
  teamId: string;
  seasonId: string;
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
  seasonId: string;
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
  seasonId: string;
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
  seasonId: string;
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

/**
 * Helper function to parse error responses properly
 */
const parseErrorResponse = async (response: Response, defaultMessage: string): Promise<string> => {
  try {
    const responseText = await response.text();
    console.error('API Error Response (raw):', responseText);
    
    if (responseText) {
      try {
        const errorResponse = JSON.parse(responseText);
        console.error('API Error Response (parsed):', errorResponse);
        
        if (errorResponse.errors && Array.isArray(errorResponse.errors)) {
          return errorResponse.errors.join(', ');
        } else if (errorResponse.message) {
          return errorResponse.message;
        } else {
          return responseText;
        }
      } catch {
        // If JSON parsing fails, use the raw text
        return responseText;
      }
    }
  } catch (readError) {
    console.error('Error reading response:', readError);
  }
  
  return `HTTP ${response.status}: ${defaultMessage}`;
};

export const floorballStatisticsService = {
  /**
   * Get team statistics for a specific season
   */
  getTeamStatistics: async (seasonId: string, teamId: string): Promise<FloorballTeamSeasonStatisticsDto> => {
    try {
      const response = await fetch(`${API_URL}/floorball/statistics/team/${seasonId}/${teamId}`);
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch team statistics');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FloorballTeamSeasonStatisticsDto> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch team statistics');
      }
      
      return apiResponse.data;
    } catch (error) {
      console.error('Error fetching team statistics:', error);
      throw error;
    }
  },

  /**
   * Get player statistics for a specific season
   */
  getPlayerStatistics: async (seasonId: string, playerId: string): Promise<FloorballPlayerSeasonStatisticsDto> => {
    try {
      const response = await fetch(`${API_URL}/floorball/statistics/player/${seasonId}/${playerId}`);
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch player statistics');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FloorballPlayerSeasonStatisticsDto> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch player statistics');
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
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch match statistics');
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
  getTopScorers: async (seasonId: string, topN: number = 10): Promise<FloorballPlayerSeasonStatisticsDto[]> => {
    try {
      const response = await fetch(`${API_URL}/floorball/statistics/topscorers/${seasonId}?topN=${topN}`);
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch top scorers');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FloorballPlayerSeasonStatisticsDto[]> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch top scorers');
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
  getSeasonStatistics: async (seasonId: string): Promise<FloorballSeasonStatisticsSummaryDto> => {
    try {
      const response = await fetch(`${API_URL}/floorball/statistics/season/${seasonId}`);
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch season statistics');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FloorballSeasonStatisticsSummaryDto> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch season statistics');
      }
      
      return apiResponse.data;
    } catch (error) {
      console.error('Error fetching season statistics:', error);
      throw error;
    }
  },

  /**
   * Get team standings for a specific season
   */
  getTeamStandings: async (seasonId: string): Promise<FloorballTeamSeasonStatisticsDto[]> => {
    try {
      const response = await fetch(`${API_URL}/floorball/statistics/standings/${seasonId}`);
      
      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch team standings');
        throw new Error(errorMessage);
      }
      
      const apiResponse: ApiResponse<FloorballTeamSeasonStatisticsDto[]> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch team standings');
      }
      
      return apiResponse.data;
    } catch (error) {
      console.error('Error fetching team standings:', error);
      throw error;
    }
  }
};
