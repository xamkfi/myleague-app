import { useState, useEffect, useCallback, useRef } from 'react';
import { signalRService, type MatchEvent } from '../services/signalRService';
import { timerService, type TimerUpdate } from '../api/common/timerService';

export interface TimerState {
  isRunning: boolean;
  elapsedTime: string;
  periodNumber?: number;
  lastUpdated: string;
}

export interface UseTimerOptions {
  matchId: string;
  autoConnect?: boolean;
  onTimerUpdate?: (update: TimerUpdate) => void;
}

export function useTimer(options: UseTimerOptions) {
  const { matchId, autoConnect = true, onTimerUpdate } = options;
  
  const [timerState, setTimerState] = useState<TimerState>({
    isRunning: false,
    elapsedTime: '00:00',
    lastUpdated: new Date().toISOString(),
  });
  
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const unsubscribeRef = useRef<(() => void) | null>(null);
  const handleTimerUpdateRef = useRef<((event: MatchEvent) => void) | null>(null);

  // Load initial timer status
  const loadTimerStatus = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      
      const status = await timerService.getTimerStatus(matchId);
      
      // Format the elapsed time to only show hours when needed
      let formattedTime = status.elapsedTime;
      if (status.elapsedTime && status.elapsedTime.includes(':')) {
        const parts = status.elapsedTime.split(':');
        if (parts.length === 3) {
          const [hours, minutes, seconds] = parts;
          const hoursNum = parseInt(hours) || 0;
          const minutesNum = parseInt(minutes) || 0;
          const secondsNum = parseInt(seconds) || 0;
          
          // If hours is 0, only show mm:ss
          if (hoursNum === 0) {
            formattedTime = `${minutesNum.toString().padStart(2, '0')}:${secondsNum.toString().padStart(2, '0')}`;
          } else {
            // If hours > 0, show hh:mm:ss
            formattedTime = `${hoursNum.toString().padStart(2, '0')}:${minutesNum.toString().padStart(2, '0')}:${secondsNum.toString().padStart(2, '0')}`;
          }
        }
      }
      
      setTimerState({
        isRunning: status.isRunning,
        elapsedTime: formattedTime,
        lastUpdated: new Date().toISOString(),
      });
    } catch (err) {
      console.error('Error loading timer status:', err);
      // Don't set error for status loading - it's not critical
      // Just log it and continue with default state
    } finally {
      setLoading(false);
    }
  }, [matchId]);

  // Start timer
  const startTimer = useCallback(async (periodNumber?: number) => {
    try {
      setLoading(true);
      setError(null);
      
      await timerService.startTimer(matchId, periodNumber);
      
      // Don't reload timer status - let SignalR handle the update
    } catch (err) {
      console.error('Error starting timer:', err);
      setError(err instanceof Error ? err.message : 'Failed to start timer');
    } finally {
      setLoading(false);
    }
  }, [matchId]);

  // Stop timer
  const stopTimer = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      
      await timerService.stopTimer(matchId);
      
      // Don't reload timer status - let SignalR handle the update
    } catch (err) {
      console.error('Error stopping timer:', err);
      setError(err instanceof Error ? err.message : 'Failed to stop timer');
    } finally {
      setLoading(false);
    }
  }, [matchId]);

  // Reset timer
  const resetTimer = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      
      await timerService.resetTimer(matchId);
      
      // Don't reload timer status - let SignalR handle the update
    } catch (err) {
      console.error('Error resetting timer:', err);
      setError(err instanceof Error ? err.message : 'Failed to reset timer');
    } finally {
      setLoading(false);
    }
  }, [matchId]);

  // Create timer
  const createTimer = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      
      await timerService.createTimer(matchId);
      // Don't reload timer status - let SignalR handle the update
    } catch (err) {
      console.error('Error creating timer:', err);
      // Don't set error for timer creation - it might already exist
      // Just log it and continue
    } finally {
      setLoading(false);
    }
  }, [matchId]);

  // Set timer to specific time
  const setTimer = useCallback(async (timeInSeconds: number) => {
    try {
      setLoading(true);
      setError(null);
      
      await timerService.setTimer(matchId, timeInSeconds);
      
      // Don't reload timer status - let SignalR handle the update
    } catch (err) {
      console.error('Error setting timer:', err);
      setError(err instanceof Error ? err.message : 'Failed to set timer');
    } finally {
      setLoading(false);
    }
  }, [matchId]);

  // Adjust timer by specific seconds (can be positive or negative)
  const adjustTimer = useCallback(async (adjustmentInSeconds: number) => {
    try {
      setLoading(true);
      setError(null);
      
      await timerService.adjustTimer(matchId, adjustmentInSeconds);
      
      // Don't reload timer status - let SignalR handle the update
    } catch (err) {
      console.error('Error adjusting timer:', err);
      setError(err instanceof Error ? err.message : 'Failed to adjust timer');
    } finally {
      setLoading(false);
    }
  }, [matchId]);

  // Destroy timer
  const destroyTimer = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      
      await timerService.destroyTimer(matchId);
      setTimerState({
        isRunning: false,
        elapsedTime: '00:00',
        lastUpdated: new Date().toISOString(),
      });
    } catch (err) {
      console.error('Error destroying timer:', err);
      setError(err instanceof Error ? err.message : 'Failed to destroy timer');
    } finally {
      setLoading(false);
    }
  }, [matchId]);

  // Handle timer updates from SignalR
  const handleTimerUpdate = useCallback((event: MatchEvent) => {
    if (event.eventType === 'TimerUpdateEvent' && event.data) {
      const timerUpdate = event.data as TimerUpdate;
      
      if (timerUpdate.MatchId === matchId) {
        // Format the elapsed time to only show hours when needed
        let formattedTime = timerUpdate.ElapsedTime;
        if (timerUpdate.ElapsedTime && timerUpdate.ElapsedTime.includes(':')) {
          const parts = timerUpdate.ElapsedTime.split(':');
          if (parts.length === 3) {
            const [hours, minutes, seconds] = parts;
            const hoursNum = parseInt(hours) || 0;
            const minutesNum = parseInt(minutes) || 0;
            const secondsNum = parseInt(seconds) || 0;
            
            // If hours is 0, only show mm:ss
            if (hoursNum === 0) {
              formattedTime = `${minutesNum.toString().padStart(2, '0')}:${secondsNum.toString().padStart(2, '0')}`;
            } else {
              // If hours > 0, show hh:mm:ss
              formattedTime = `${hoursNum.toString().padStart(2, '0')}:${minutesNum.toString().padStart(2, '0')}:${secondsNum.toString().padStart(2, '0')}`;
            }
          }
        }
        
        setTimerState({
          isRunning: timerUpdate.IsRunning,
          elapsedTime: formattedTime,
          periodNumber: timerUpdate.PeriodNumber,
          lastUpdated: timerUpdate.LastUpdated,
        });
        
        if (onTimerUpdate) {
          onTimerUpdate(timerUpdate);
        }
      }
    }
  }, [matchId, onTimerUpdate]);

  // Update the ref with the latest callback
  handleTimerUpdateRef.current = handleTimerUpdate;

  // Setup SignalR connection and event handling
  useEffect(() => {
    let isActive = true; // Track if this effect is still active
    
    const setupSignalR = async () => {
      try {
        // Only proceed if we should auto-connect and have a matchId
        if (!autoConnect || !matchId) {
          return;
        }
        
        // Connect to SignalR
        await signalRService.connect();
        
        // Subscribe to match events (this is what we need for timer updates)
        await signalRService.subscribeToMatch(matchId);
        
        // Listen for timer events
        if (isActive) { // Only set up if still active
          unsubscribeRef.current = signalRService.onMatchEvent((event) => {
            if (handleTimerUpdateRef.current) {
              handleTimerUpdateRef.current(event);
            }
          });
        }
        
        // Load initial timer status to get current state immediately
        await loadTimerStatus();
        
      } catch (err) {
        console.error('Error setting up SignalR for timer:', err);
        // Don't set error - SignalR is not critical for basic functionality
      }
    };

    setupSignalR();

    // Cleanup on unmount
    return () => {
      isActive = false; // Mark as inactive
      
      if (unsubscribeRef.current) {
        unsubscribeRef.current();
        unsubscribeRef.current = null;
      }
      
      if (autoConnect && matchId) {
        signalRService.unsubscribeFromMatch(matchId).catch(console.error);
      }
    };
  }, [matchId, autoConnect, loadTimerStatus]); // Include loadTimerStatus dependency

  return {
    timerState,
    loading,
    error,
    startTimer,
    stopTimer,
    resetTimer,
    setTimer,
    adjustTimer,
    createTimer,
    destroyTimer,
    loadTimerStatus,
  };
} 