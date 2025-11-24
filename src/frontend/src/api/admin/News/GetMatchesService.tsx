interface FloorballMatch {
  id: string;
  seasonId: string;
  homeTeamId: string;
  homeTeamName: string;
  awayTeamId: string;
  awayTeamName: string;
  scheduledDateTime: string;
  venue?: string;
  status: 'scheduled' | 'in_progress' | 'completed' | 'cancelled';
  homeScore: number;
  awayScore: number;
  wentToOvertime: boolean;
  wentToShootout: boolean;
  periodScores: Record<string, { homeScore: number; awayScore: number }>;
  officials: string[];
  goalEvents: unknown[];
  penaltyEvents: unknown[];
}

interface ApiResponse<T> {
  success: boolean;
  data: T;
  message: string;
  errors: string[];
}

import { VITE_API_URL } from '../../../constants/config';

const API_URL = VITE_API_URL;

export const getMatchesService = {
  getAll: async (): Promise<FloorballMatch[]> => {
    const response = await fetch(`${API_URL}/FloorballMatch`);
    if (!response.ok) {
      throw new Error('Failed to fetch matches');
    }
    const apiResponse: ApiResponse<FloorballMatch[]> = await response.json();
    if (!apiResponse.success) {
      throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch matches');
    }
    return apiResponse.data;
  },

  getById: async (id: string): Promise<FloorballMatch> => {
    const response = await fetch(`${API_URL}/FloorballMatch/${id}`);
    if (!response.ok) {
      throw new Error('Failed to fetch match');
    }
    const apiResponse: ApiResponse<FloorballMatch> = await response.json();
    if (!apiResponse.success) {
      throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch match');
    }
    return apiResponse.data;
  },

  create: async (data: Omit<FloorballMatch, 'id'>): Promise<FloorballMatch> => {
    const response = await fetch(`${API_URL}/FloorballMatch`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(data),
    });
    if (!response.ok) {
      throw new Error('Failed to create match');
    }
    const apiResponse: ApiResponse<FloorballMatch> = await response.json();
    if (!apiResponse.success) {
      throw new Error(apiResponse.errors?.join(', ') || 'Failed to create match');
    }
    return apiResponse.data;
  },

  update: async (id: string, data: Partial<FloorballMatch>): Promise<FloorballMatch> => {
    const response = await fetch(`${API_URL}/FloorballMatch/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(data),
    });
    if (!response.ok) {
      throw new Error('Failed to update match');
    }
    const apiResponse: ApiResponse<FloorballMatch> = await response.json();
    if (!apiResponse.success) {
      throw new Error(apiResponse.errors?.join(', ') || 'Failed to update match');
    }
    return apiResponse.data;
  },

  delete: async (id: string): Promise<void> => {
    const response = await fetch(`${API_URL}/FloorballMatch/${id}`, {
      method: 'DELETE',
    });
    if (!response.ok) {
      throw new Error('Failed to delete match');
    }
    const apiResponse: ApiResponse<void> = await response.json();
    if (!apiResponse.success) {
      throw new Error(apiResponse.errors?.join(', ') || 'Failed to delete match');
    }
  },
};

export type { FloorballMatch };
