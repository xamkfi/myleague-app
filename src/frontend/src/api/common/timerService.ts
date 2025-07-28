import type { ApiResponse } from '../../types/common/apiResponseType';

const API_URL = import.meta.env.VITE_API_URL || '/api';

export interface TimerStatusResponse {
  exists: boolean;
  isRunning: boolean;
  elapsedTime: string; // TimeSpan as string
}

export interface TimerUpdate {
  matchId: string;
  periodNumber?: number;
  elapsedTime: string;
  isRunning: boolean;
  lastUpdated: string;
  eventType: string;
}

export const timerService = {
  /**
   * Creates a timer for a match
   */
  createTimer: async (matchId: string): Promise<void> => {
    const response = await fetch(`${API_URL}/matches/${matchId}/timer/create`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`HTTP ${response.status}: ${errorText || 'Failed to create timer'}`);
    }
  },

  /**
   * Starts the timer for a match
   */
  startTimer: async (matchId: string, periodNumber?: number): Promise<void> => {
    const url = new URL(`${API_URL}/matches/${matchId}/timer/start`);
    if (periodNumber !== undefined) {
      url.searchParams.append('periodNumber', periodNumber.toString());
    }

    const response = await fetch(url.toString(), {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`HTTP ${response.status}: ${errorText || 'Failed to start timer'}`);
    }
  },

  /**
   * Stops the timer for a match
   */
  stopTimer: async (matchId: string): Promise<void> => {
    const response = await fetch(`${API_URL}/matches/${matchId}/timer/stop`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`HTTP ${response.status}: ${errorText || 'Failed to stop timer'}`);
    }
  },

  /**
   * Resets the timer for a match
   */
  resetTimer: async (matchId: string): Promise<void> => {
    const response = await fetch(`${API_URL}/matches/${matchId}/timer/reset`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`HTTP ${response.status}: ${errorText || 'Failed to reset timer'}`);
    }
  },

  /**
   * Gets the elapsed time for a match
   */
  getElapsedTime: async (matchId: string): Promise<string> => {
    const response = await fetch(`${API_URL}/matches/${matchId}/timer/elapsed`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`HTTP ${response.status}: ${errorText || 'Failed to get elapsed time'}`);
    }

    const apiResponse: ApiResponse<string> = await response.json();
    return apiResponse.data;
  },

  /**
   * Gets the timer status for a match
   */
  getTimerStatus: async (matchId: string): Promise<TimerStatusResponse> => {
    const response = await fetch(`${API_URL}/matches/${matchId}/timer/status`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`HTTP ${response.status}: ${errorText || 'Failed to get timer status'}`);
    }

    const apiResponse: ApiResponse<TimerStatusResponse> = await response.json();
    return apiResponse.data;
  },

  /**
   * Destroys the timer for a match
   */
  destroyTimer: async (matchId: string): Promise<void> => {
    const response = await fetch(`${API_URL}/matches/${matchId}/timer/destroy`, {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`HTTP ${response.status}: ${errorText || 'Failed to destroy timer'}`);
    }
  },
}; 