import type { ApiResponse } from '../../types/floorball/floorballTypes';
import type {
  FloorballTournamentDto,
  CreateFloorballTournamentRequest,
  FloorballPlayoffBracketDto,
  PlayoffScheduleSlotRequest,
} from '../../types/floorball/tournamentTypes';
import { authFetch } from '../utils/authFetch';
import { parseErrorResponse } from '../utils/ParseErrorResponse';
import { API_URL } from '../../constants/config';

export const floorballTournamentService = {
  getAll: async (): Promise<ApiResponse<FloorballTournamentDto[]>> => {
    try {
      const response = await authFetch(`${API_URL}/FloorballTournament`);

      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch tournaments');
        throw new Error(errorMessage);
      }

      const apiResponse: ApiResponse<FloorballTournamentDto[]> = await response.json();

      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to fetch tournaments'));
      }

      return apiResponse;
    } catch (error) {
      console.error('Error in floorballTournamentService.getAll:', error);
      throw error;
    }
  },

  getActive: async (): Promise<ApiResponse<FloorballTournamentDto[]>> => {
    try {
      const response = await authFetch(`${API_URL}/FloorballTournament/active`);

      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch active tournaments');
        throw new Error(errorMessage);
      }

      const apiResponse: ApiResponse<FloorballTournamentDto[]> = await response.json();

      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to fetch active tournaments'));
      }

      return apiResponse;
    } catch (error) {
      console.error('Error in floorballTournamentService.getActive:', error);
      throw error;
    }
  },

  getById: async (id: string): Promise<ApiResponse<FloorballTournamentDto>> => {
    try {
      const response = await authFetch(`${API_URL}/FloorballTournament/${id}`);

      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch tournament');
        throw new Error(errorMessage);
      }

      const apiResponse: ApiResponse<FloorballTournamentDto> = await response.json();

      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to fetch tournament'));
      }

      return apiResponse;
    } catch (error) {
      console.error('Error in floorballTournamentService.getById:', error);
      throw error;
    }
  },

  create: async (data: CreateFloorballTournamentRequest): Promise<ApiResponse<FloorballTournamentDto>> => {
    try {
      const response = await authFetch(`${API_URL}/FloorballTournament`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data),
      });

      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to create tournament');
        throw new Error(errorMessage);
      }

      const apiResponse: ApiResponse<FloorballTournamentDto> = await response.json();

      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to create tournament'));
      }

      return apiResponse;
    } catch (error) {
      console.error('Error in floorballTournamentService.create:', error);
      throw error;
    }
  },

  update: async (id: string, data: CreateFloorballTournamentRequest): Promise<ApiResponse<FloorballTournamentDto>> => {
    try {
      const response = await authFetch(`${API_URL}/FloorballTournament/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data),
      });

      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to update tournament');
        throw new Error(errorMessage);
      }

      const apiResponse: ApiResponse<FloorballTournamentDto> = await response.json();

      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to update tournament'));
      }

      return apiResponse;
    } catch (error) {
      console.error('Error in floorballTournamentService.update:', error);
      throw error;
    }
  },

  delete: async (id: string): Promise<void> => {
    try {
      const response = await authFetch(`${API_URL}/FloorballTournament/${id}`, {
        method: 'DELETE',
      });

      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to delete tournament');
        throw new Error(errorMessage);
      }

      const apiResponse: ApiResponse<void> = await response.json();

      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to delete tournament'));
      }
    } catch (error) {
      console.error('Error in floorballTournamentService.delete:', error);
      throw error;
    }
  },

  cancel: async (id: string): Promise<ApiResponse<FloorballTournamentDto>> => {
    try {
      const response = await authFetch(`${API_URL}/FloorballTournament/${id}/cancel`, {
        method: 'POST',
      });

      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to cancel tournament');
        throw new Error(errorMessage);
      }

      const apiResponse: ApiResponse<FloorballTournamentDto> = await response.json();

      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to cancel tournament'));
      }

      return apiResponse;
    } catch (error) {
      console.error('Error in floorballTournamentService.cancel:', error);
      throw error;
    }
  },

  startGroupStage: async (id: string): Promise<ApiResponse<FloorballTournamentDto>> => {
    try {
      const response = await authFetch(`${API_URL}/FloorballTournament/${id}/start-group-stage`, {
        method: 'POST',
      });

      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to start group stage');
        throw new Error(errorMessage);
      }

      const apiResponse: ApiResponse<FloorballTournamentDto> = await response.json();

      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to start group stage'));
      }

      return apiResponse;
    } catch (error) {
      console.error('Error in floorballTournamentService.startGroupStage:', error);
      throw error;
    }
  },

  getPlayoffBracket: async (id: string): Promise<ApiResponse<FloorballPlayoffBracketDto>> => {
    try {
      const response = await authFetch(`${API_URL}/FloorballTournament/${id}/playoff-bracket`);

      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to fetch playoff bracket');
        throw new Error(errorMessage);
      }

      const apiResponse: ApiResponse<FloorballPlayoffBracketDto> = await response.json();

      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to fetch playoff bracket'));
      }

      return apiResponse;
    } catch (error) {
      console.error('Error in floorballTournamentService.getPlayoffBracket:', error);
      throw error;
    }
  },

  /**
   * Replaces the tournament's pre-defined playoff schedule slots in one request. Pass an empty
   * array to clear the schedule. The backend rejects edits once the playoff stage has started.
   */
  updatePlayoffSchedule: async (
    id: string,
    slots: PlayoffScheduleSlotRequest[]
  ): Promise<ApiResponse<FloorballTournamentDto>> => {
    try {
      const response = await authFetch(`${API_URL}/FloorballTournament/${id}/playoff-schedule`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ slots }),
      });

      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to update playoff schedule');
        throw new Error(errorMessage);
      }

      const apiResponse: ApiResponse<FloorballTournamentDto> = await response.json();

      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to update playoff schedule'));
      }

      return apiResponse;
    } catch (error) {
      console.error('Error in floorballTournamentService.updatePlayoffSchedule:', error);
      throw error;
    }
  },

  startPlayoffStage: async (id: string): Promise<ApiResponse<FloorballTournamentDto>> => {
    try {
      const response = await authFetch(`${API_URL}/FloorballTournament/${id}/start-playoff-stage`, {
        method: 'POST',
      });

      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to start playoff stage');
        throw new Error(errorMessage);
      }

      const apiResponse: ApiResponse<FloorballTournamentDto> = await response.json();

      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to start playoff stage'));
      }

      return apiResponse;
    } catch (error) {
      console.error('Error in floorballTournamentService.startPlayoffStage:', error);
      throw error;
    }
  },

  complete: async (id: string): Promise<ApiResponse<FloorballTournamentDto>> => {
    try {
      const response = await authFetch(`${API_URL}/FloorballTournament/${id}/complete`, {
        method: 'POST',
      });

      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to complete tournament');
        throw new Error(errorMessage);
      }

      const apiResponse: ApiResponse<FloorballTournamentDto> = await response.json();

      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to complete tournament'));
      }

      return apiResponse;
    } catch (error) {
      console.error('Error in floorballTournamentService.complete:', error);
      throw error;
    }
  },

  addGroup: async (id: string, groupName: string): Promise<ApiResponse<FloorballTournamentDto>> => {
    try {
      const response = await authFetch(`${API_URL}/FloorballTournament/${id}/groups`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ groupName }),
      });

      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to add group');
        throw new Error(errorMessage);
      }

      const apiResponse: ApiResponse<FloorballTournamentDto> = await response.json();

      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to add group'));
      }

      return apiResponse;
    } catch (error) {
      console.error('Error in floorballTournamentService.addGroup:', error);
      throw error;
    }
  },

  removeGroup: async (id: string, groupId: string): Promise<void> => {
    try {
      const response = await authFetch(`${API_URL}/FloorballTournament/${id}/groups/${groupId}`, {
        method: 'DELETE',
      });

      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to remove group');
        throw new Error(errorMessage);
      }

      const apiResponse: ApiResponse<void> = await response.json();

      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to remove group'));
      }
    } catch (error) {
      console.error('Error in floorballTournamentService.removeGroup:', error);
      throw error;
    }
  },

  addTeamToGroup: async (id: string, groupId: string, teamId: string): Promise<ApiResponse<FloorballTournamentDto>> => {
    try {
      const response = await authFetch(`${API_URL}/FloorballTournament/${id}/groups/${groupId}/teams`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ teamId }),
      });

      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to add team to group');
        throw new Error(errorMessage);
      }

      const apiResponse: ApiResponse<FloorballTournamentDto> = await response.json();

      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to add team to group'));
      }

      return apiResponse;
    } catch (error) {
      console.error('Error in floorballTournamentService.addTeamToGroup:', error);
      throw error;
    }
  },

  removeTeamFromGroup: async (id: string, groupId: string, teamId: string): Promise<void> => {
    try {
      const response = await authFetch(`${API_URL}/FloorballTournament/${id}/groups/${groupId}/teams/${teamId}`, {
        method: 'DELETE',
      });

      if (!response.ok) {
        const errorMessage = await parseErrorResponse(response, 'Failed to remove team from group');
        throw new Error(errorMessage);
      }

      const apiResponse: ApiResponse<void> = await response.json();

      if (!apiResponse.success) {
        throw new Error(await parseErrorResponse(apiResponse, 'Failed to remove team from group'));
      }
    } catch (error) {
      console.error('Error in floorballTournamentService.removeTeamFromGroup:', error);
      throw error;
    }
  },
};
