import type { ApiResponse } from '../../types/common/apiResponseType';

const API_URL = import.meta.env.VITE_API_URL || '/api';

export interface TimeSpan {
  days: number;
  hours: number;
  minutes: number;
  seconds: number;
  milliseconds: number;
}

export interface TimerStatusResponse {
  exists: boolean;
  isRunning: boolean;
  elapsedTime: string; // TimeSpan as string
  periodNumber?: number; // Current period number
}

export interface TimerUpdate {
  MatchId: string; // Backend sends with PascalCase
  PeriodNumber?: number;
  ElapsedTime: string;
  IsRunning: boolean;
  LastUpdated: string;
  EventType: string;
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
    console.log('=== timerService.startTimer CALLED ===');
    console.log('Match ID:', matchId);
    console.log('Period Number:', periodNumber);
    
    const url = new URL(`${API_URL}/matches/${matchId}/timer/start`);
    if (periodNumber !== undefined) {
      url.searchParams.append('periodNumber', periodNumber.toString());
    }
    
    console.log('Start timer API URL:', url.toString());

    const response = await fetch(url.toString(), {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    console.log('Start timer response status:', response.status);
    console.log('Start timer response ok:', response.ok);

    if (!response.ok) {
      const errorText = await response.text();
      console.error('Start timer error response:', errorText);
      throw new Error(`HTTP ${response.status}: ${errorText || 'Failed to start timer'}`);
    }

    console.log('=== timerService.startTimer SUCCESS ===');
  },

  /**
   * Stops the timer for a match
   */
  stopTimer: async (matchId: string): Promise<void> => {
    console.log('=== timerService.stopTimer CALLED ===');
    console.log('Match ID:', matchId);
    console.log('API URL:', `${API_URL}/matches/${matchId}/timer/stop`);
    
    const response = await fetch(`${API_URL}/matches/${matchId}/timer/stop`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    console.log('Stop timer response status:', response.status);
    console.log('Stop timer response ok:', response.ok);

    if (!response.ok) {
      const errorText = await response.text();
      console.error('Stop timer error response:', errorText);
      throw new Error(`HTTP ${response.status}: ${errorText || 'Failed to stop timer'}`);
    }

    console.log('=== timerService.stopTimer SUCCESS ===');
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

    const apiResponse: ApiResponse<TimeSpan> = await response.json();
    console.log('getElapsedTime API response:', apiResponse);
    
    // The backend returns a TimeSpan object, we need to convert it to a string
    // TimeSpan is serialized as an object with properties like { days: 0, hours: 0, minutes: 0, seconds: 0, milliseconds: 0 }
    const timeSpan = apiResponse.data;
    const totalSeconds = (timeSpan.days * 24 * 3600) + (timeSpan.hours * 3600) + (timeSpan.minutes * 60) + timeSpan.seconds;
    const hours = Math.floor(totalSeconds / 3600);
    const minutes = Math.floor((totalSeconds % 3600) / 60);
    const seconds = totalSeconds % 60;
    
    // Format as "hh:mm:ss" or "mm:ss" depending on hours
    const formattedTime = hours > 0 
      ? `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`
      : `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
    
    console.log('Formatted elapsed time:', formattedTime);
    return formattedTime;
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
   * Sets the timer to a specific time for a match
   */
  setTimer: async (matchId: string, timeInSeconds: number): Promise<void> => {
    console.log('=== timerService.setTimer CALLED ===');
    console.log('Match ID:', matchId);
    console.log('Time in seconds:', timeInSeconds);
    
    const response = await fetch(`${API_URL}/matches/${matchId}/timer/set-time`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ timeInSeconds }),
    });

    console.log('Set timer response status:', response.status);
    console.log('Set timer response ok:', response.ok);

    if (!response.ok) {
      const errorText = await response.text();
      console.error('Set timer error response:', errorText);
      throw new Error(`HTTP ${response.status}: ${errorText || 'Failed to set timer'}`);
    }

    console.log('=== timerService.setTimer SUCCESS ===');
  },

  /**
   * Adjusts the timer by a specific number of seconds (can be positive or negative)
   */
  adjustTimer: async (matchId: string, adjustmentInSeconds: number): Promise<void> => {
    console.log('=== timerService.adjustTimer CALLED ===');
    console.log('Match ID:', matchId);
    console.log('Adjustment in seconds:', adjustmentInSeconds);
    
    try {
      // First, get the current timer status to get the elapsed time
      const status = await timerService.getTimerStatus(matchId);
      
      if (!status.exists) {
        throw new Error('Timer does not exist for this match');
      }
      
      // Parse the current elapsed time to seconds
      let currentSeconds = 0;
      if (status.elapsedTime && status.elapsedTime.includes(':')) {
        const parts = status.elapsedTime.split(':');
        if (parts.length === 3) {
          const [hours, minutes, seconds] = parts.map(p => parseInt(p, 10) || 0);
          currentSeconds = hours * 3600 + minutes * 60 + seconds;
        } else if (parts.length === 2) {
          const [minutes, seconds] = parts.map(p => parseInt(p, 10) || 0);
          currentSeconds = minutes * 60 + seconds;
        }
      }
      
      // Calculate new time (ensure it doesn't go below 0)
      const newTimeInSeconds = Math.max(0, currentSeconds + adjustmentInSeconds);
      
      console.log('Current time in seconds:', currentSeconds);
      console.log('New time in seconds:', newTimeInSeconds);
      
      // Use the existing setTimer method to set the new time
      await timerService.setTimer(matchId, newTimeInSeconds);
      
      console.log('=== timerService.adjustTimer SUCCESS ===');
    } catch (error) {
      console.error('=== timerService.adjustTimer FAILED ===');
      console.error('Error adjusting timer:', error);
      throw error;
    }
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