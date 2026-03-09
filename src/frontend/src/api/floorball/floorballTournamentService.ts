import type {
  ApiResponse,
  FloorballTournamentDto,
  FloorballTournamentSummaryDto,
  FloorballTournamentGroupDto,
  FloorballTournamentGroupTeamDto,
  FloorballTournamentGroupStandingsDto,
  FloorballMatchDto,
  CreateFloorballTournamentRequest,
  UpdateFloorballTournamentRequest,
  AddGroupToTournamentRequest,
  CreateTournamentMatchRequest,
} from '../../types/floorball/floorballTypes';
import { authFetch } from '../utils/authFetch';
import { parseErrorResponse } from '../utils/ParseErrorResponse';
import { API_URL } from '../../constants/config';

const BASE = `${API_URL}/FloorballTournament`;

export const floorballTournamentService = {
  getAll: async (status?: string): Promise<ApiResponse<FloorballTournamentSummaryDto[]>> => {
    const params = status ? `?status=${encodeURIComponent(status)}` : '';
    const response = await authFetch(`${BASE}${params}`);
    const apiResponse: ApiResponse<FloorballTournamentSummaryDto[]> = await response.json();
    if (!response.ok) {
      const errorMessage = await parseErrorResponse(apiResponse, 'Failed to fetch tournaments');
      throw new Error(errorMessage);
    }
    if (!apiResponse.success) {
      throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch tournaments');
    }
    return apiResponse;
  },

  getById: async (id: string): Promise<ApiResponse<FloorballTournamentDto>> => {
    const response = await authFetch(`${BASE}/${id}`);
    const apiResponse: ApiResponse<FloorballTournamentDto> = await response.json();
    if (!response.ok) {
      const errorMessage = await parseErrorResponse(apiResponse, 'Failed to fetch tournament');
      throw new Error(errorMessage);
    }
    if (!apiResponse.success) {
      throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch tournament');
    }
    return apiResponse;
  },

  create: async (data: CreateFloorballTournamentRequest): Promise<ApiResponse<FloorballTournamentDto>> => {
    const response = await authFetch(BASE, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    });
    const apiResponse: ApiResponse<FloorballTournamentDto> = await response.json();
    if (!response.ok) {
      const errorMessage = await parseErrorResponse(apiResponse, 'Failed to create tournament');
      throw new Error(errorMessage);
    }
    if (!apiResponse.success) {
      throw new Error(apiResponse.errors?.join(', ') || 'Failed to create tournament');
    }
    return apiResponse;
  },

  update: async (id: string, data: UpdateFloorballTournamentRequest): Promise<ApiResponse<FloorballTournamentDto>> => {
    const response = await authFetch(`${BASE}/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    });
    const apiResponse: ApiResponse<FloorballTournamentDto> = await response.json();
    if (!response.ok) {
      const errorMessage = await parseErrorResponse(apiResponse, 'Failed to update tournament');
      throw new Error(errorMessage);
    }
    if (!apiResponse.success) {
      throw new Error(apiResponse.errors?.join(', ') || 'Failed to update tournament');
    }
    return apiResponse;
  },

  delete: async (id: string): Promise<ApiResponse<null>> => {
    const response = await authFetch(`${BASE}/${id}`, { method: 'DELETE' });
    const apiResponse: ApiResponse<null> = await response.json();
    if (!response.ok) {
      const errorMessage = await parseErrorResponse(apiResponse, 'Failed to delete tournament');
      throw new Error(errorMessage);
    }
    return apiResponse;
  },

  changeStatus: async (id: string, action: string): Promise<ApiResponse<FloorballTournamentDto>> => {
    const response = await authFetch(`${BASE}/${id}/status`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ action }),
    });
    const apiResponse: ApiResponse<FloorballTournamentDto> = await response.json();
    if (!response.ok) {
      const errorMessage = await parseErrorResponse(apiResponse, 'Failed to change tournament status');
      throw new Error(errorMessage);
    }
    if (!apiResponse.success) {
      throw new Error(apiResponse.errors?.join(', ') || 'Failed to change tournament status');
    }
    return apiResponse;
  },

  addGroup: async (tournamentId: string, data: AddGroupToTournamentRequest): Promise<ApiResponse<FloorballTournamentGroupDto>> => {
    const response = await authFetch(`${BASE}/${tournamentId}/groups`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    });
    const apiResponse: ApiResponse<FloorballTournamentGroupDto> = await response.json();
    if (!response.ok) {
      const errorMessage = await parseErrorResponse(apiResponse, 'Failed to add group');
      throw new Error(errorMessage);
    }
    if (!apiResponse.success) {
      throw new Error(apiResponse.errors?.join(', ') || 'Failed to add group');
    }
    return apiResponse;
  },

  removeGroup: async (tournamentId: string, groupId: string): Promise<ApiResponse<null>> => {
    const response = await authFetch(`${BASE}/${tournamentId}/groups/${groupId}`, {
      method: 'DELETE',
    });
    const apiResponse: ApiResponse<null> = await response.json();
    if (!response.ok) {
      const errorMessage = await parseErrorResponse(apiResponse, 'Failed to remove group');
      throw new Error(errorMessage);
    }
    return apiResponse;
  },

  addTeamToGroup: async (tournamentId: string, groupId: string, teamId: string): Promise<ApiResponse<FloorballTournamentGroupTeamDto>> => {
    const response = await authFetch(`${BASE}/${tournamentId}/groups/${groupId}/teams/${teamId}`, {
      method: 'POST',
    });
    const apiResponse: ApiResponse<FloorballTournamentGroupTeamDto> = await response.json();
    if (!response.ok) {
      const errorMessage = await parseErrorResponse(apiResponse, 'Failed to add team to group');
      throw new Error(errorMessage);
    }
    if (!apiResponse.success) {
      throw new Error(apiResponse.errors?.join(', ') || 'Failed to add team to group');
    }
    return apiResponse;
  },

  removeTeamFromGroup: async (tournamentId: string, groupId: string, teamId: string): Promise<ApiResponse<null>> => {
    const response = await authFetch(`${BASE}/${tournamentId}/groups/${groupId}/teams/${teamId}`, {
      method: 'DELETE',
    });
    const apiResponse: ApiResponse<null> = await response.json();
    if (!response.ok) {
      const errorMessage = await parseErrorResponse(apiResponse, 'Failed to remove team from group');
      throw new Error(errorMessage);
    }
    return apiResponse;
  },

  getGroupStandings: async (tournamentId: string, groupId: string): Promise<ApiResponse<FloorballTournamentGroupStandingsDto>> => {
    const response = await authFetch(`${BASE}/${tournamentId}/groups/${groupId}/standings`);
    const apiResponse: ApiResponse<FloorballTournamentGroupStandingsDto> = await response.json();
    if (!response.ok) {
      const errorMessage = await parseErrorResponse(apiResponse, 'Failed to fetch group standings');
      throw new Error(errorMessage);
    }
    if (!apiResponse.success) {
      throw new Error(apiResponse.errors?.join(', ') || 'Failed to fetch group standings');
    }
    return apiResponse;
  },

  createMatch: async (tournamentId: string, data: CreateTournamentMatchRequest): Promise<ApiResponse<FloorballMatchDto>> => {
    const response = await authFetch(`${BASE}/${tournamentId}/matches`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    });
    const apiResponse: ApiResponse<FloorballMatchDto> = await response.json();
    if (!response.ok) {
      const errorMessage = await parseErrorResponse(apiResponse, 'Failed to create tournament match');
      throw new Error(errorMessage);
    }
    if (!apiResponse.success) {
      throw new Error(apiResponse.errors?.join(', ') || 'Failed to create tournament match');
    }
    return apiResponse;
  },
};
