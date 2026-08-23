import { useState, useEffect, useCallback, useRef } from 'react';
import { timerService, type TimerUpdate } from '../api/common/timerService';

export interface UseMatchTimerOptions {
  matchId: string;
  autoConnect?: boolean;
  onTimerUpdate?: (update: TimerUpdate) => void;
}

export interface UseMatchTimerReturn {
  displayTimeMs: number;
  displayTime: string;
  isRunning: boolean;
  periodNumber: number | null;
  loading: boolean;
  error: string | null;
  initialLoadComplete: boolean;
  startTimer: (periodNumber?: number) => Promise<void>;
  stopTimer: () => Promise<void>;
  resetTimer: () => Promise<void>;
  setTimer: (timeInSeconds: number) => Promise<void>;
  adjustTimer: (adjustmentInSeconds: number) => Promise<void>;
  createTimer: () => Promise<void>;
  loadStatus: () => Promise<void>;
  getCurrentElapsedSeconds: () => number;
}

/**
 * Format milliseconds to display string (MM:SS or HH:MM:SS)
 */
function formatMs(ms: number): string {
  const totalSeconds = Math.floor(Math.max(0, ms) / 1000);
  const hours = Math.floor(totalSeconds / 3600);
  const minutes = Math.floor((totalSeconds % 3600) / 60);
  const seconds = totalSeconds % 60;
  
  if (hours > 0) {
    return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
  }
  return `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
}

/**
 * Parse elapsed time string to milliseconds
 */
function parseElapsedTime(elapsedTime: string): number {
  if (!elapsedTime || !elapsedTime.includes(':')) return 0;
  
  const parts = elapsedTime.split(':');
  if (parts.length === 3) {
    const [hours, minutes, seconds] = parts.map(p => parseInt(p, 10) || 0);
    return (hours * 3600 + minutes * 60 + seconds) * 1000;
  } else if (parts.length === 2) {
    const [minutes, seconds] = parts.map(p => parseInt(p, 10) || 0);
    return (minutes * 60 + seconds) * 1000;
  }
  return 0;
}

/**
 * Simple match timer hook - REST only, no SignalR.
 * 
 * How it works:
 * 1. On mount: load status from REST API
 * 2. When running: tick locally every 100ms
 * 3. On any action: call API, wait for response, update state from response
 */
export function useMatchTimer(options: UseMatchTimerOptions): UseMatchTimerReturn {
  const { matchId, autoConnect = true, onTimerUpdate } = options;
  
  // Simple state
  const [timeMs, setTimeMs] = useState(0);
  const [isRunning, setIsRunning] = useState(false);
  const [periodNumber, setPeriodNumber] = useState<number | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [initialLoadComplete, setInitialLoadComplete] = useState(false);
  
  // Track the base time and when we started ticking (for accurate interpolation)
  const baseTimeRef = useRef(0);
  const tickStartRef = useRef(Date.now());
  
  /**
   * Load timer status from REST API
   */
  const loadStatus = useCallback(async () => {
    try {
      setError(null);
      
      const status = await timerService.getTimerStatus(matchId);
      const elapsedMs = parseElapsedTime(status.elapsedTime);
      
      // Update state
      baseTimeRef.current = elapsedMs;
      tickStartRef.current = Date.now();
      setTimeMs(elapsedMs);
      setIsRunning(status.isRunning);
      setPeriodNumber(status.periodNumber ?? null);
      setInitialLoadComplete(true);
      
      // Notify parent if callback provided
      if (onTimerUpdate) {
        const resolvedPeriod = typeof status.periodNumber === 'number' && Number.isFinite(status.periodNumber)
          ? status.periodNumber
          : undefined;
        onTimerUpdate({
          MatchId: matchId,
          ElapsedTime: status.elapsedTime,
          ElapsedMilliseconds: elapsedMs,
          IsRunning: status.isRunning,
          PeriodNumber: resolvedPeriod,
          LastUpdated: new Date().toISOString(),
          EventType: 'TimerStatusLoaded',
        });
      }
      
    } catch (err) {
      console.error('Error loading timer status:', err);
      setInitialLoadComplete(true);
    }
  }, [matchId, onTimerUpdate]);
  
  /**
   * Initial load on mount
   */
  useEffect(() => {
    if (!autoConnect || !matchId) return;
    loadStatus();
  }, [matchId, autoConnect, loadStatus]);
  
  /**
   * Local tick when running - simple interval that calculates elapsed time
   */
  useEffect(() => {
    if (!isRunning || !initialLoadComplete) return;
    
    const interval = setInterval(() => {
      const now = Date.now();
      const elapsed = now - tickStartRef.current;
      setTimeMs(baseTimeRef.current + elapsed);
    }, 100);
    
    return () => clearInterval(interval);
  }, [isRunning, initialLoadComplete]);
  
  /**
   * Start the timer
   */
  const startTimer = useCallback(async (period?: number) => {
    try {
      setLoading(true);
      setError(null);
      
      await timerService.startTimer(matchId, period);
      await loadStatus();
      
    } catch (err) {
      console.error('Error starting timer:', err);
      setError(err instanceof Error ? err.message : 'Failed to start timer');
    } finally {
      setLoading(false);
    }
  }, [matchId, loadStatus]);
  
  /**
   * Stop the timer
   */
  const stopTimer = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      
      await timerService.stopTimer(matchId);
      await loadStatus();
      
    } catch (err) {
      console.error('Error stopping timer:', err);
      setError(err instanceof Error ? err.message : 'Failed to stop timer');
    } finally {
      setLoading(false);
    }
  }, [matchId, loadStatus]);
  
  /**
   * Reset the timer
   */
  const resetTimer = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      
      await timerService.resetTimer(matchId);
      await loadStatus();
      
    } catch (err) {
      console.error('Error resetting timer:', err);
      setError(err instanceof Error ? err.message : 'Failed to reset timer');
    } finally {
      setLoading(false);
    }
  }, [matchId, loadStatus]);
  
  /**
   * Set the timer to a specific time
   */
  const setTimer = useCallback(async (timeInSeconds: number) => {
    try {
      setLoading(true);
      setError(null);
      
      await timerService.setTimer(matchId, timeInSeconds);
      await loadStatus();
      
    } catch (err) {
      console.error('Error setting timer:', err);
      setError(err instanceof Error ? err.message : 'Failed to set timer');
    } finally {
      setLoading(false);
    }
  }, [matchId, loadStatus]);
  
  /**
   * Adjust the timer by a number of seconds
   */
  const adjustTimer = useCallback(async (adjustmentInSeconds: number) => {
    try {
      setLoading(true);
      setError(null);
      
      await timerService.adjustTimer(matchId, adjustmentInSeconds);
      await loadStatus();
      
    } catch (err) {
      console.error('Error adjusting timer:', err);
      setError(err instanceof Error ? err.message : 'Failed to adjust timer');
    } finally {
      setLoading(false);
    }
  }, [matchId, loadStatus]);
  
  /**
   * Create a new timer
   */
  const createTimer = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      
      await timerService.createTimer(matchId);
      
    } catch (err) {
      console.error('Error creating timer:', err);
      // Don't set error - timer might already exist
    } finally {
      setLoading(false);
    }
  }, [matchId]);
  
  /**
   * Get current elapsed seconds
   */
  const getCurrentElapsedSeconds = useCallback(() => {
    return Math.floor(timeMs / 1000);
  }, [timeMs]);
  
  return {
    displayTimeMs: timeMs,
    displayTime: formatMs(timeMs),
    isRunning,
    periodNumber,
    loading,
    error,
    initialLoadComplete,
    startTimer,
    stopTimer,
    resetTimer,
    setTimer,
    adjustTimer,
    createTimer,
    loadStatus,
    getCurrentElapsedSeconds,
  };
}
