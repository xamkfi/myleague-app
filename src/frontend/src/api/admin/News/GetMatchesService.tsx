import { authFetch } from '../../utils/authFetch';
import { API_URL } from '../../../constants/config';

interface FloorballMatch {
  id: string;
  competitionId: string;
  homeTeamId: string;
  homeTeamName: string;
  homeTeamLogo: string | null;
  awayTeamId: string;
  awayTeamName: string;
  awayTeamLogo: string | null;
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

interface PaginatedApiResponse<T> {
  success: boolean;
  data: T[];
  pagination: {
    currentPage: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
    hasNextPage: boolean;
    hasPreviousPage: boolean;
    startItem: number;
    endItem: number;
  };
  message: string;
  errors: string[];
}

interface GetMatchesRequest {
  page?: number;
  pageSize?: number;
  competitionId?: string;
  teamId?: string;
  startDate?: string;
  endDate?: string;
  status?: number; // FloorballMatchStatus enum: 1=Scheduled, 3=InProgress, 4=Completed, 5=Cancelled
  sortOrder?: string;
}

export const getMatchesService = {
  getAll: async (params?: GetMatchesRequest): Promise<PaginatedApiResponse<FloorballMatch>> => {
    const searchParams = new URLSearchParams();
    
    if (params?.page) searchParams.append('page', params.page.toString());
    if (params?.pageSize) searchParams.append('pageSize', params.pageSize.toString());
    if (params?.competitionId) searchParams.append('competitionId', params.competitionId);
    if (params?.teamId) searchParams.append('teamId', params.teamId);
    if (params?.startDate) searchParams.append('startDate', params.startDate);
    if (params?.endDate) searchParams.append('endDate', params.endDate);
    if (params?.status !== undefined) searchParams.append('status', params.status.toString());
    if (params?.sortOrder) searchParams.append('sortOrder', params.sortOrder);

    const url = `${API_URL}/floorball-matches${searchParams.toString() ? `?${searchParams.toString()}` : ''}`;
    const response = await authFetch(url);
    
    if (!response.ok) {
      throw new Error('Failed to fetch matches');
    }
    
    const apiResponse: PaginatedApiResponse<FloorballMatch> = await response.json();
    if (!apiResponse.success) {
      throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch matches');
    }
    
    return apiResponse;
  },

  getById: async (id: string): Promise<FloorballMatch> => {
    const response = await authFetch(`${API_URL}/floorball-matches/by-id/${id}`);
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
    const response = await authFetch(`${API_URL}/floorball-matches`, {
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
    // Backend's UpdateFloorballMatchCommand expects the match id in the body, not in the URL.
    const response = await authFetch(`${API_URL}/floorball-matches`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ ...data, id }),
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
    const response = await authFetch(`${API_URL}/floorball-matches/${id}`, {
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
