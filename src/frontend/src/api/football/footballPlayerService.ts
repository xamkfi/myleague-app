import type { 
  ApiResponse,
  PaginatedApiResponse,
  FootballPosition,
} from '../../types/football/footballTypes';
import type { Address, ContactInfo } from '../../types/admin/personTypes';
import { authFetch } from '../utils/authFetch';
import { API_URL } from '../../constants/config';

export interface PersonDto {
  id: string;
  firstName: string;
  lastName: string;
  birthDate: string;
  fullName: string;
  isRegistered: boolean;
  address?: Address;
  contactInfo?: ContactInfo;
}

export interface FootballPlayerDto {
  id: string;
  personId: string;
  person: PersonDto;  // Nested person object to match backend
  isActive: boolean;
  position: FootballPosition;
  careerGoals: number;
  careerAssists: number;
  jerseyNumber?: number;
  team?: {
    id: string;
    name: string;
  } | null;
}

export interface GetFootballPlayersRequest {
  page?: number;
  pageSize?: number;
  isActive?: boolean;
  position?: FootballPosition;
  teamId?: string;
  searchTerm?: string;
  signal?: AbortSignal;
}

export interface UpdateFootballPlayerRequest {
  isActive: boolean;
}

export interface CreateFootballPlayerRequest {
  personId: string;
}

export interface FootballPlayerMatchStatsDto {
  goals: number;
  assists: number;
  yellowCards: number;
  redCards: number;
  playedMinutes: number;
}

export interface FootballPlayerMatchDto {
  id: string;
  competitionId: string;
  /**
   * Display name of the competition (season or tournament).
   * Backend renamed from "seasonName" to "competitionName" when seasons were
   * generalized to FootballCompetition (TPH base for seasons + tournaments).
   */
  competitionName: string;
  homeTeamId: string;
  homeTeamName: string;
  awayTeamId: string;
  awayTeamName: string;
  scheduledDateTime: string;
  venue: string | null;
  status: string;
  homeScore: number;
  awayScore: number;
  wentToExtraTime: boolean;
  wentToPenaltyShootout: boolean;
  periodScores: Record<string, { homeScore: number; awayScore: number }>;
  playerStats: FootballPlayerMatchStatsDto | null;
}

export interface FootballPlayerStatsDto {
  gamesPlayed: number;
  goals: number;
  assists: number;
  points: number;
  yellowCards: number;
  redCards: number;
}

export interface FootballPlayerTeamCareerStatsDto {
  teamId: string;
  teamName: string;
  stats: FootballPlayerStatsDto;
}

export interface FootballPlayerWithMatchesDto {
  id: string;
  playerName: string;
  position: FootballPosition;
  jerseyNumber: number | null;
  teamName: string;
  teamId: string;
  isActive: boolean;
  careerStats: FootballPlayerTeamCareerStatsDto[];
  recentMatches: FootballPlayerMatchDto[];
}

export const footballPlayerService = {
  /**
   * Get all Football players with pagination and filtering
   */
  getAll: async (params?: GetFootballPlayersRequest): Promise<PaginatedApiResponse<FootballPlayerDto>> => {
    try {
      const searchParams = new URLSearchParams();
      
      // Always provide page (default to 1 if not specified)
      const page = params?.page ?? 1;
      searchParams.append('page', page.toString());
      
      // Always provide pageSize (default to 0 for backend default)
      const pageSize = params?.pageSize ?? 0;
      searchParams.append('pageSize', pageSize.toString());
      
      if (params?.isActive !== undefined) searchParams.append('isActive', params.isActive.toString());
      if (params?.position) searchParams.append('position', params.position);
      if (params?.teamId) searchParams.append('teamId', params.teamId);
      if (params?.searchTerm) searchParams.append('searchTerm', params.searchTerm);

      const url = `${API_URL}/FootballPlayer?${searchParams.toString()}`;
      const response = await authFetch(url, { signal: params?.signal });
      
      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`HTTP ${response.status}: ${errorText || 'Failed to fetch Football players'}`);
      }
      
      const apiResponse: PaginatedApiResponse<FootballPlayerDto> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch Football players');
      }
      
      return apiResponse;
    } catch (error) {
      if (error instanceof DOMException && error.name === 'AbortError') {
        throw error;
      }
      throw error;
    }
  },

  /**
   * Get players by team ID
   */
  getByTeamId: async (teamId: string): Promise<FootballPlayerDto[]> => {
    try {
      
      const response = await footballPlayerService.getAll({
        teamId,
        pageSize: 50 // Use max allowed page size to get as many players as possible
      });
      
      return response.data || [];
    } catch (error) {
      console.error('Error in footballPlayerService.getByTeamId:', error);
      throw error;
    }
  },

  /**
   * Get a Football player by ID
   */
  getById: async (id: string): Promise<FootballPlayerDto> => {
    try {
      const url = `${API_URL}/FootballPlayer/${id}`;
      
      const response = await authFetch(url);
      
      if (!response.ok) {
        const errorText = await response.text();
        console.error('API Error Response:', errorText);
        throw new Error(`HTTP ${response.status}: ${errorText || 'Failed to fetch Football player'}`);
      }
      
      const apiResponse: ApiResponse<FootballPlayerDto> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch Football player');
      }
      
      return apiResponse.data;
    } catch (error) {
      console.error('Error in footballPlayerService.getById:', error);
      throw error;
    }
  },

  /**
   * Update a Football player
   */
  update: async (id: string, data: UpdateFootballPlayerRequest): Promise<FootballPlayerDto> => {
    try {
      
      const response = await authFetch(`${API_URL}/FootballPlayer/${id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
      });
      
      
      if (!response.ok) {
        const errorText = await response.text();
        console.error('Update API Error Response:', errorText);
        throw new Error(`HTTP ${response.status}: ${errorText || 'Failed to update Football player'}`);
      }
      
      const apiResponse: ApiResponse<FootballPlayerDto> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to update Football player');
      }
      
      return apiResponse.data;
    } catch (error) {
      console.error('Error in footballPlayerService.update:', error);
      throw error;
    }
  },

  /**
   * Create a Football player from a person
   */
  create: async (data: CreateFootballPlayerRequest): Promise<FootballPlayerDto> => {
    try {
      
      const response = await authFetch(`${API_URL}/FootballPlayer`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
      });
      
      
      if (!response.ok) {
        const errorText = await response.text();
        console.error('Create API Error Response:', errorText);
        throw new Error(`HTTP ${response.status}: ${errorText || 'Failed to create Football player'}`);
      }
      
      const apiResponse: ApiResponse<FootballPlayerDto> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to create Football player');
      }
      
      return apiResponse.data;
    } catch (error) {
      console.error('Error in footballPlayerService.create:', error);
      throw error;
    }
  },

  /**
   * Get a Football player's match history with performance statistics
   */
  getPlayerMatches: async (id: string, limit: number = 50): Promise<FootballPlayerWithMatchesDto> => {
    try {
      const url = `${API_URL}/FootballPlayer/${id}/matches?limit=${limit}`;
      const response = await fetch(url);

      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`HTTP ${response.status}: ${errorText || 'Failed to fetch player matches'}`);
      }

      const apiResponse: ApiResponse<FootballPlayerWithMatchesDto> = await response.json();

      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch player matches');
      }

      return apiResponse.data;
    } catch (error) {
      console.error('Error in footballPlayerService.getPlayerMatches:', error);
      throw error;
    }
  },

  /**
   * Delete a Football player
   */
  delete: async (id: string): Promise<void> => {
    try {
      
      const response = await authFetch(`${API_URL}/FootballPlayer/${id}`, {
        method: 'DELETE',
      });
      
      
      if (!response.ok) {
        const errorText = await response.text();
        console.error('Delete API Error Response:', errorText);
        throw new Error(`HTTP ${response.status}: ${errorText || 'Failed to delete Football player'}`);
      }
      
      const apiResponse: ApiResponse<void> = await response.json();
      
      if (!apiResponse.success) {
        throw new Error(apiResponse.errors?.join(', ') || 'Failed to delete Football player');
      }
    } catch (error) {
      console.error('Error in footballPlayerService.delete:', error);
      throw error;
    }
  }
}; 