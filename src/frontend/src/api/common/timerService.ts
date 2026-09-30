import type { ApiResponse } from '../../types/common/apiResponseType';
import { authFetch } from '../utils/authFetch';
import { API_URL } from '../../constants/config';

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
  /** Elapsed time formatted by the backend as `hh:mm:ss`. Empty when the timer does not exist. */
  elapsedTime: string;
  periodNumber?: number | null;
}

export interface TimerUpdate {
  MatchId: string; // Backend sends with PascalCase
  PeriodNumber?: number;
  ElapsedTime: string;
  ElapsedMilliseconds?: number; // optional: prefer for math when present
  IsRunning: boolean;
  LastUpdated: string;
  EventType: string;
  Sequence?: number; // optional: ordering token
}

const JSON_HEADERS: HeadersInit = { 'Content-Type': 'application/json' };

async function ensureOk(response: Response, fallbackMessage: string): Promise<void> {
  if (response.ok) return;
  const errorText: string = await response.text();
  throw new Error(`HTTP ${response.status}: ${errorText || fallbackMessage}`);
}

async function readStatus(response: Response): Promise<TimerStatusResponse> {
  const apiResponse: ApiResponse<TimerStatusResponse> = await response.json();
  return apiResponse.data;
}

export const timerService = {
  /**
   * Creates a timer for a match. The backend treats this as idempotent.
   */
  createTimer: async (matchId: string): Promise<void> => {
    const response = await authFetch(`${API_URL}/matches/${matchId}/timer/create`, {
      method: 'POST',
      headers: JSON_HEADERS,
    });
    await ensureOk(response, 'Failed to create timer');
  },

  /**
   * Starts the timer for a match. Creates the timer server-side if it does not exist yet.
   */
  startTimer: async (matchId: string, periodNumber?: number): Promise<void> => {
    const url = new URL(`${API_URL}/matches/${matchId}/timer/start`);
    if (typeof periodNumber === 'number' && Number.isFinite(periodNumber)) {
      url.searchParams.append('periodNumber', periodNumber.toString());
    }
    const response = await authFetch(url.toString(), {
      method: 'POST',
      headers: JSON_HEADERS,
    });
    await ensureOk(response, 'Failed to start timer');
  },

  /**
   * Stops the timer and returns the authoritative status after the stop.
   */
  stopTimer: async (matchId: string): Promise<TimerStatusResponse> => {
    const response = await authFetch(`${API_URL}/matches/${matchId}/timer/stop`, {
      method: 'POST',
      headers: JSON_HEADERS,
    });
    await ensureOk(response, 'Failed to stop timer');
    return readStatus(response);
  },

  /**
   * Resets the timer to zero.
   */
  resetTimer: async (matchId: string): Promise<void> => {
    const response = await authFetch(`${API_URL}/matches/${matchId}/timer/reset`, {
      method: 'POST',
      headers: JSON_HEADERS,
    });
    await ensureOk(response, 'Failed to reset timer');
  },

  /**
   * Gets the timer status for a match.
   */
  getTimerStatus: async (matchId: string): Promise<TimerStatusResponse> => {
    const response = await authFetch(`${API_URL}/matches/${matchId}/timer/status`, {
      method: 'GET',
      headers: JSON_HEADERS,
    });
    await ensureOk(response, 'Failed to get timer status');
    return readStatus(response);
  },

  /**
   * Sets the timer to an absolute elapsed time and returns the status after the change.
   */
  setTimer: async (matchId: string, timeInSeconds: number): Promise<TimerStatusResponse> => {
    const response = await authFetch(`${API_URL}/matches/${matchId}/timer/set-time`, {
      method: 'PUT',
      headers: JSON_HEADERS,
      body: JSON.stringify({ timeInSeconds }),
    });
    await ensureOk(response, 'Failed to set timer');
    return readStatus(response);
  },

  /**
   * Destroys the timer for a match.
   */
  destroyTimer: async (matchId: string): Promise<void> => {
    const response = await authFetch(`${API_URL}/matches/${matchId}/timer/destroy`, {
      method: 'DELETE',
      headers: JSON_HEADERS,
    });
    await ensureOk(response, 'Failed to destroy timer');
  },
};
