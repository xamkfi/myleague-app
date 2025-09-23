import type { 
  ApiResponse,
  FloorballMatchDto
} from '../../types/floorball/floorballTypes';

// Event DTOs
export interface FloorballGoalEventDto {
  teamId: string;
  playerId: string;
  assisterId?: string;
  secondaryAssisterId?: string;
  periodNumber: number;
  timeInSeconds: number;
  wasInOvertime: boolean;
  wasInShootout: boolean;
}

export interface FloorballPenaltyEventDto {
  teamId: string;
  playerId?: string;
  penaltyType: string;
  minutes: number;
  periodNumber: number;
  timeInSeconds: number;
  description: string;
}

export interface FloorballDomainEventDto {
  eventType: string;
  occurredOn: string;
  data: Record<string, unknown>;
}

const API_URL = import.meta.env.VITE_API_URL || '/api';

// Request interfaces matching backend models
export interface RecordGoalEventRequest {
  matchId: string;
  teamId: string;
  playerId: string;
  assisterId?: string;
  periodNumber: number;
  timeInSeconds: number;
  wasInOvertime: boolean;
  wasInShootout: boolean;
}

export interface RecordPenaltyEventRequest {
  matchId: string;
  teamId: string;
  playerId?: string;
  penaltyType: string;
  durationMinutes: number;
  periodNumber: number;
  timeInSeconds: number;
  description?: string;
}

// Add RecordSaveEventRequest interface
export interface RecordSaveEventRequest {
  goalieId: string;
  matchId: string;
  teamId: string;
  playerId: string;
  periodNumber: number;
  timeInSeconds: number;
  wasInOvertime: boolean;
  wasInShootout: boolean;
}

export interface UpdateGoalEventRequest extends RecordGoalEventRequest {
  eventId: string;
}

export interface UpdatePenaltyEventRequest extends RecordPenaltyEventRequest {
  eventId: string;
}

// Response DTOs matching backend
export interface FloorballGoalEventDto {
  teamId: string;
  playerId: string;
  assisterId?: string;
  secondaryAssisterId?: string;
  periodNumber: number;
  timeInSeconds: number;
  wasInOvertime: boolean;
  wasInShootout: boolean;
}

export interface FloorballPenaltyEventDto {
  teamId: string;
  playerId?: string;
  penaltyType: string;
  minutes: number;
  periodNumber: number;
  timeInSeconds: number;
  description: string;
}
// Add FloorballSaveEventDto interface
export interface FloorballSaveEventDto {
  teamId: string;
  goalieId: string;
  periodNumber: number;
  timeInSeconds: number;
  wasInOvertime: boolean;
  wasInShootout: boolean;
}

/**
 * Helper function to handle API responses consistently
 */
const handleApiResponse = async <T>(response: Response): Promise<ApiResponse<T>> => {
  if (!response.ok) {
    const errorText = await response.text();
    console.error('API Error Response:', errorText);
    throw new Error(`HTTP ${response.status}: ${errorText || 'API request failed'}`);
  }
  
  const apiResponse: ApiResponse<T> = await response.json();
  
  if (!apiResponse.success) {
    throw new Error(apiResponse.errors?.join(', ') || 'API request failed');
  }
  
  return apiResponse;
};

export const floorballMatchEventService = {
  /**
   * Get match events (goals and penalties) for a match
   */
  getMatchEvents: async (matchId: string): Promise<ApiResponse<FloorballDomainEventDto[]>> => {
    try {
      console.log('Fetching match events for match:', matchId);
      // Use FloorballMatchController to fetch the match and synthesize events
      const response = await fetch(`${API_URL}/FloorballMatch/by-id/${matchId}`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
      });

      const apiResponse = await handleApiResponse<FloorballMatchDto>(response);

      const match = apiResponse.data;
      const occurredOn = match?.scheduledDateTime ?? new Date().toISOString();

      const goalEvents: FloorballDomainEventDto[] = (match?.goalEvents ?? []).map((g) => ({
        eventType: 'FloorballGoalScoredEvent',
        occurredOn,
        data: {
          matchId,
          eventId: g.id,
          teamId: g.teamId,
          playerId: g.playerId,
          periodNumber: g.periodNumber,
          timeInSeconds: g.timeInSeconds,
          isOvertime: g.wasInOvertime,
          isShootout: g.wasInShootout,
          assisterId: g.assisterId,
          secondaryAssisterId: g.secondaryAssisterId
        }
      }));

      const penaltyEvents: FloorballDomainEventDto[] = (match?.penaltyEvents ?? []).map((p) => ({
        eventType: 'FloorballPenaltyAssignedEvent',
        occurredOn,
        data: {
          matchId,
          eventId: p.id,
          teamId: p.teamId,
          playerId: p.playerId,
          periodNumber: p.periodNumber,
          timeInSeconds: p.timeInSeconds,
          penaltyType: p.penaltyType,
          minutes: p.minutes,
          description: p.description
        }
      }));

      // Synthesize saves from DTO
      type SaveEventFromDto = {
        teamId: string;
        goalieId: string;
        periodNumber: number;
        timeInSeconds: number;
        wasInOvertime: boolean;
        wasInShootout: boolean;
      };
      const saveEvents: FloorballDomainEventDto[] = (match?.saveEvents ?? []).map((s: SaveEventFromDto) => ({
        eventType: 'FloorballSaveEvent',
        occurredOn,
        data: {
          matchId,
          teamId: s.teamId,
          goalieId: s.goalieId,
          periodNumber: s.periodNumber,
          timeInSeconds: s.timeInSeconds,
          wasInOvertime: s.wasInOvertime,
          wasInShootout: s.wasInShootout
        }
      }));

      const synthesizedEvents = [...goalEvents, ...penaltyEvents, ...saveEvents];

      return {
        success: true,
        data: synthesizedEvents,
        message: 'Match events synthesized from match DTO',
        errors: []
      };
    } catch (error) {
      console.error('Error fetching match events:', error);
      throw error;
    }
  },

  /**
   * Record a goal event in a floorball match
   */
  recordGoal: async (data: RecordGoalEventRequest): Promise<ApiResponse<FloorballMatchDto>> => {
    try {
      console.log('Recording goal:', data);
      // Switch to FloorballMatchController endpoint
      const payload = {
        matchId: data.matchId,
        scoringTeamId: data.teamId,
        scoringPlayerId: data.playerId,
        assistingPlayerId: data.assisterId,
        secondaryAssistingPlayerIs: undefined,
        periodNumber: data.periodNumber,
        timeInSeconds: data.timeInSeconds,
        description: '',
        goalType: null
      };

      const response = await fetch(`${API_URL}/FloorballMatch/record-goal`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(payload),
      });
      
      return await handleApiResponse<FloorballMatchDto>(response);
    } catch (error) {
      console.error('Error recording goal:', error);
      throw error;
    }
  },

  /**
   * Record a penalty event in a floorball match
   */
  recordPenalty: async (data: RecordPenaltyEventRequest): Promise<ApiResponse<FloorballMatchDto>> => {
    try {
      console.log('Recording penalty:', data);
      // Switch to FloorballMatchController endpoint
      const payload = {
        matchId: data.matchId,
        teamId: data.teamId,
        playerId: data.playerId,
        penaltyType: data.penaltyType,
        durationMinutes: data.durationMinutes,
        periodNumber: data.periodNumber,
        timeInSeconds: data.timeInSeconds,
        description: data.description ?? ''
      };

      const response = await fetch(`${API_URL}/FloorballMatch/record-penalty`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(payload),
      });
      
      return await handleApiResponse<FloorballMatchDto>(response);
    } catch (error) {
      console.error('Error recording penalty:', error);
      throw error;
    }
  },

  /**
   * Add recordSave method
   */
  recordSave: async (data: RecordSaveEventRequest): Promise<ApiResponse<FloorballMatchDto>> => {
    try {
      console.log('Recording save:', data);
      // Switch to FloorballMatchController endpoint
      const payload = {
        matchId: data.matchId,
        teamId: data.teamId,
        playerId: data.goalieId,
        periodNumber: data.periodNumber,
        timeInSeconds: data.timeInSeconds,
        wasInOvertime: data.wasInOvertime,
        wasInShootout: data.wasInShootout
      };

      const response = await fetch(`${API_URL}/FloorballMatch/record-save`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });
      return await handleApiResponse<FloorballMatchDto>(response);
    } catch (error) {
      console.error('Error recording save:', error);
      throw error;
    }
  },

  /**
   * Update a goal event
   */
  updateGoal: async (data: UpdateGoalEventRequest): Promise<ApiResponse<FloorballGoalEventDto>> => {
    try {
      console.log('Updating goal:', data);
      
      const response = await fetch(`${API_URL}/FloorballMatchEvent/goal`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
      });
      
      return await handleApiResponse<FloorballGoalEventDto>(response);
    } catch (error) {
      console.error('Error updating goal:', error);
      throw error;
    }
  },

  /**
   * Update a penalty event
   */
  updatePenalty: async (data: UpdatePenaltyEventRequest): Promise<ApiResponse<FloorballPenaltyEventDto>> => {
    try {
      console.log('Updating penalty:', data);
      
      const response = await fetch(`${API_URL}/FloorballMatchEvent/penalty`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
      });
      
      return await handleApiResponse<FloorballPenaltyEventDto>(response);
    } catch (error) {
      console.error('Error updating penalty:', error);
      throw error;
    }
  },

  /**
   * Delete a match event (goal or penalty)
   */
  deleteEvent: async (eventId: string): Promise<ApiResponse<void>> => {
    try {
      console.log('Deleting event:', eventId);
      
      const response = await fetch(`${API_URL}/FloorballMatchEvent/${eventId}`, {
        method: 'DELETE',
      });
      
      return await handleApiResponse<void>(response);
    } catch (error) {
      console.error('Error deleting event:', error);
      throw error;
    }
  },

  /**
   * Start a period in a floorball match
   */
  startPeriod: async (matchId: string, periodNumber: number): Promise<ApiResponse<void>> => {
    try {
      console.log('Starting period:', periodNumber, 'for match:', matchId);
      // For period 1, use start-match; for overtime/shootout, handled by dedicated endpoints.
      if (periodNumber === 1) {
        const response = await fetch(`${API_URL}/FloorballMatch/start-match/${matchId}`, {
          method: 'PUT',
          headers: {
            'Content-Type': 'application/json',
          },
        });
        return await handleApiResponse<void>(response);
      }
      // No server call required for other periods
      return { success: true, data: undefined as unknown as void, message: 'Period start handled client-side', errors: [] };
    } catch (error) {
      console.error('Error starting period:', error);
      throw error;
    }
  },

  /**
   * End a period in a floorball match
   */
  endPeriod: async (matchId: string, periodNumber: number): Promise<ApiResponse<FloorballMatchDto>> => {
    try {
      console.log('Ending period:', periodNumber, 'for match:', matchId);
      const response = await fetch(`${API_URL}/FloorballMatch/${matchId}/period/${periodNumber}/end`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' }
        });
      return await handleApiResponse<FloorballMatchDto>(response);
    } catch (error) {
      console.error('Error ending period:', error);
      throw error;
    }
  },

  /**
   * Record overtime in a floorball match
   */
  recordOvertime: async (matchId: string): Promise<ApiResponse<FloorballMatchDto>> => {
    try {
      console.log('Recording overtime for match:', matchId);
      const response = await fetch(`${API_URL}/FloorballMatch/${matchId}/overtime`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
      });
      return await handleApiResponse<FloorballMatchDto>(response);
    } catch (error) {
      console.error('Error recording overtime:', error);
      throw error;
    }
  },

  /**
   * Record shootout in a floorball match
   */
  recordShootout: async (matchId: string): Promise<ApiResponse<FloorballMatchDto>> => {
    try {
      console.log('Recording shootout for match:', matchId);
      const response = await fetch(`${API_URL}/FloorballMatch/${matchId}/shootout`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
      });
      return await handleApiResponse<FloorballMatchDto>(response);
    } catch (error) {
      console.error('Error recording shootout:', error);
      throw error;
    }
  },

  /**
   * Cancel a floorball match
   */
  cancelMatch: async (matchId: string): Promise<ApiResponse<FloorballMatchDto>> => {
    try {
      console.log('Canceling match:', matchId);
      const response = await fetch(`${API_URL}/FloorballMatch/${matchId}/cancel`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
      });
      return await handleApiResponse<FloorballMatchDto>(response);
    } catch (error) {
      console.error('Error canceling match:', error);
      throw error;
    }
  }
}; 