import type { 
  ApiResponse
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
  playerId: string;
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
      
      const response = await fetch(`${API_URL}/FloorballMatchEvent/${matchId}/history`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
      });
      
      return await handleApiResponse<FloorballDomainEventDto[]>(response);
    } catch (error) {
      console.error('Error fetching match events:', error);
      throw error;
    }
  },

  /**
   * Record a goal event in a floorball match
   */
  recordGoal: async (data: RecordGoalEventRequest): Promise<ApiResponse<FloorballGoalEventDto>> => {
    try {
      console.log('Recording goal:', data);
      
      const response = await fetch(`${API_URL}/FloorballMatchEvent/goal`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
      });
      
      return await handleApiResponse<FloorballGoalEventDto>(response);
    } catch (error) {
      console.error('Error recording goal:', error);
      throw error;
    }
  },

  /**
   * Record a penalty event in a floorball match
   */
  recordPenalty: async (data: RecordPenaltyEventRequest): Promise<ApiResponse<FloorballPenaltyEventDto>> => {
    try {
      console.log('Recording penalty:', data);
      
      const response = await fetch(`${API_URL}/FloorballMatchEvent/penalty`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
      });
      
      return await handleApiResponse<FloorballPenaltyEventDto>(response);
    } catch (error) {
      console.error('Error recording penalty:', error);
      throw error;
    }
  },

  /**
   * Add recordSave method
   */
  recordSave: async (data: RecordSaveEventRequest): Promise<ApiResponse<FloorballSaveEventDto>> => {
    try {
      console.log('Recording save:', data);
      const response = await fetch(`${API_URL}/FloorballMatchEvent/save`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data)
      });
      return await handleApiResponse<FloorballSaveEventDto>(response);
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
      
      const response = await fetch(`${API_URL}/FloorballMatchEvent/match/${matchId}/period/${periodNumber}/start`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
      });
      
      return await handleApiResponse<void>(response);
    } catch (error) {
      console.error('Error starting period:', error);
      throw error;
    }
  },

  /**
   * End a period in a floorball match
   */
  endPeriod: async (matchId: string, periodNumber: number): Promise<ApiResponse<void>> => {
    try {
      console.log('Ending period:', periodNumber, 'for match:', matchId);
      
      const response = await fetch(`${API_URL}/FloorballMatchEvent/match/${matchId}/period/${periodNumber}/end`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
      });
      
      return await handleApiResponse<void>(response);
    } catch (error) {
      console.error('Error ending period:', error);
      throw error;
    }
  },

  /**
   * Record overtime in a floorball match
   */
  recordOvertime: async (matchId: string): Promise<ApiResponse<Record<string, unknown>>> => {
    try {
      console.log('Recording overtime for match:', matchId);
      
      const response = await fetch(`${API_URL}/FloorballMatchEvent/match/${matchId}/overtime`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
      });
      
      return await handleApiResponse<Record<string, unknown>>(response);
    } catch (error) {
      console.error('Error recording overtime:', error);
      throw error;
    }
  },

  /**
   * Record shootout in a floorball match
   */
  recordShootout: async (matchId: string): Promise<ApiResponse<Record<string, unknown>>> => {
    try {
      console.log('Recording shootout for match:', matchId);
      
      const response = await fetch(`${API_URL}/FloorballMatchEvent/match/${matchId}/shootout`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
      });
      
      return await handleApiResponse<Record<string, unknown>>(response);
    } catch (error) {
      console.error('Error recording shootout:', error);
      throw error;
    }
  },

  /**
   * Cancel a floorball match
   */
  cancelMatch: async (matchId: string): Promise<ApiResponse<Record<string, unknown>>> => {
    try {
      console.log('Canceling match:', matchId);
      
      const response = await fetch(`${API_URL}/FloorballMatchEvent/match/${matchId}/cancel`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
      });
      
      return await handleApiResponse<Record<string, unknown>>(response);
    } catch (error) {
      console.error('Error canceling match:', error);
      throw error;
    }
  }
}; 