import { useState, useEffect, useCallback, useRef, useMemo } from 'react';
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
  const lastSeqRef = useRef<number>(-1);
  
  // Local interpolated time for smooth display
  const [localElapsedMs, setLocalElapsedMs] = useState(0);
  const lastSyncTimeRef = useRef<number>(Date.now());
  const lastServerElapsedMsRef = useRef(0);
  const isRunningRef = useRef(false);
  
  // Track optimistic updates to prevent stale SignalR updates from overwriting
  const lastOptimisticUpdateRef = useRef<{ timeMs: number; timestamp: number } | null>(null);

  // Load initial timer status
  const loadTimerStatus = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      
      const status = await timerService.getTimerStatus(matchId);
      
      // Calculate milliseconds from the status for interpolation
      let elapsedMs = 0;
      
      // Format the elapsed time to only show hours when needed
      let formattedTime = status.elapsedTime;
      if (status.elapsedTime && status.elapsedTime.includes(':')) {
        const parts = status.elapsedTime.split(':');
        if (parts.length === 3) {
          const [hours, minutes, seconds] = parts;
          const hoursNum = parseInt(hours) || 0;
          const minutesNum = parseInt(minutes) || 0;
          const secondsNum = parseInt(seconds) || 0;
          
          // Calculate total milliseconds
          elapsedMs = (hoursNum * 3600 + minutesNum * 60 + secondsNum) * 1000;
          
          // If hours is 0, only show mm:ss
          if (hoursNum === 0) {
            formattedTime = `${minutesNum.toString().padStart(2, '0')}:${secondsNum.toString().padStart(2, '0')}`;
          } else {
            // If hours > 0, show hh:mm:ss
            formattedTime = `${hoursNum.toString().padStart(2, '0')}:${minutesNum.toString().padStart(2, '0')}:${secondsNum.toString().padStart(2, '0')}`;
          }
        } else if (parts.length === 2) {
          const [minutes, seconds] = parts;
          const minutesNum = parseInt(minutes) || 0;
          const secondsNum = parseInt(seconds) || 0;
          
          // Calculate total milliseconds
          elapsedMs = (minutesNum * 60 + secondsNum) * 1000;
        }
      }
      
      // Initialize local interpolation state with current server time
      lastServerElapsedMsRef.current = elapsedMs;
      lastSyncTimeRef.current = Date.now();
      setLocalElapsedMs(elapsedMs);
      
      setTimerState({
        isRunning: status.isRunning,
        elapsedTime: formattedTime,
        periodNumber: status.periodNumber,
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

  // Local clock that interpolates between server updates for smooth display
  useEffect(() => {
    if (!timerState.isRunning) {
      isRunningRef.current = false;
      return;
    }
    
    isRunningRef.current = true;
    
    const interval = setInterval(() => {
      if (!isRunningRef.current) return;
      
      const now = Date.now();
      const clientDelta = now - lastSyncTimeRef.current;
      const interpolatedMs = lastServerElapsedMsRef.current + clientDelta;
      
      setLocalElapsedMs(interpolatedMs);
    }, 50); // Update every 50ms for smooth display
    
    return () => clearInterval(interval);
  }, [timerState.isRunning]);

  // Start timer
  const startTimer = useCallback(async (periodNumber?: number) => {
    try {
      setLoading(true);
      setError(null);
      
      await timerService.startTimer(matchId, periodNumber);
      
      // Optimistically update local state; SignalR will reconcile
      setTimerState(prev => ({
        ...prev,
        isRunning: true,
        lastUpdated: new Date().toISOString(),
      }));
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
      
      // Optimistically update local state; SignalR will reconcile
      setTimerState(prev => ({
        ...prev,
        isRunning: false,
        lastUpdated: new Date().toISOString(),
      }));
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
      
      // Optimistically update local state; SignalR will reconcile
      setTimerState({
        isRunning: false,
        elapsedTime: '00:00',
        lastUpdated: new Date().toISOString(),
      });
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

  // Set timer to specific time (NOT optimistic - waits for backend confirmation)
  const setTimer = useCallback(async (timeInSeconds: number) => {
    try {
      setLoading(true);
      setError(null);
      
      // Call backend to set the time
      await timerService.setTimer(matchId, timeInSeconds);
      
      // Wait for backend confirmation and load the updated timer status
      await loadTimerStatus();
      
      // Clear any pending optimistic updates
      lastOptimisticUpdateRef.current = null;
      
      console.log('Timer set successfully to', timeInSeconds, 'seconds');
    } catch (err) {
      console.error('Error setting timer:', err);
      setError(err instanceof Error ? err.message : 'Failed to set timer');
    } finally {
      setLoading(false);
    }
  }, [matchId, loadTimerStatus]);

  // Adjust timer by specific seconds (can be positive or negative)
  const adjustTimer = useCallback(async (adjustmentInSeconds: number) => {
    try {
      setLoading(true);
      setError(null);
      
      // Calculate current time from refs (accounts for running time since last sync)
      const now = Date.now();
      const clientDelta = isRunningRef.current ? (now - lastSyncTimeRef.current) : 0;
      const currentMs = lastServerElapsedMsRef.current + clientDelta;
      
      // Optimistically update local interpolation state to prevent visual reset
      const newElapsedMs = Math.max(0, currentMs + (adjustmentInSeconds * 1000));
      lastServerElapsedMsRef.current = newElapsedMs;
      lastSyncTimeRef.current = now;
      setLocalElapsedMs(newElapsedMs);
      
      // Track this optimistic update to ignore stale SignalR updates
      lastOptimisticUpdateRef.current = { timeMs: newElapsedMs, timestamp: now };
      
      await timerService.adjustTimer(matchId, adjustmentInSeconds);
      
      // SignalR will reconcile with actual server time
    } catch (err) {
      console.error('Error adjusting timer:', err);
      setError(err instanceof Error ? err.message : 'Failed to adjust timer');
      // Clear the optimistic update on error
      lastOptimisticUpdateRef.current = null;
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
        // Lenient ordering: accept updates even if Sequence repeats; only drop if clearly older
        if (typeof timerUpdate.Sequence === 'number') {
          if (lastSeqRef.current !== -1 && timerUpdate.Sequence < lastSeqRef.current) {
            return;
          }
          lastSeqRef.current = Math.max(lastSeqRef.current, timerUpdate.Sequence);
        }
        
        const serverMs = timerUpdate.ElapsedMilliseconds || 0;
        const now = Date.now();
        
        // ALWAYS accept state changes that indicate major events (period reset, pause)
        const isPeriodReset = serverMs < 5000; // Less than 5 seconds indicates period start/reset
        const isPauseEvent = !timerUpdate.IsRunning && isRunningRef.current; // Transition to paused
        const isResumeEvent = timerUpdate.IsRunning && !isRunningRef.current; // Transition to running
        
        // Check if we have a recent optimistic update that should take precedence
        if (lastOptimisticUpdateRef.current && !isPeriodReset && !isPauseEvent && !isResumeEvent) {
          const { timeMs: optimisticMs, timestamp: optimisticTimestamp } = lastOptimisticUpdateRef.current;
          const timeSinceOptimistic = now - optimisticTimestamp;
          
          // Only ignore the update if:
          // 1. The optimistic update is recent (< 2 seconds old)
          // 2. The server time is BEHIND the optimistic time (server is stale)
          // 3. The difference is significant (> 500ms)
          if (timeSinceOptimistic < 2000 && serverMs < optimisticMs && (optimisticMs - serverMs) > 500) {
            console.log('Ignoring stale SignalR update:', serverMs, 'vs optimistic:', optimisticMs);
            return;
          }
          
          // If the server update is close to our optimistic update (within 500ms) or
          // enough time has passed, accept it and clear the optimistic update
          if (Math.abs(serverMs - optimisticMs) <= 500 || timeSinceOptimistic >= 2000) {
            lastOptimisticUpdateRef.current = null;
          }
        } else if (isPeriodReset || isPauseEvent || isResumeEvent) {
          // Clear optimistic updates on major state changes
          lastOptimisticUpdateRef.current = null;
        }
        
        // Sync local clock with server time
        lastServerElapsedMsRef.current = serverMs;
        lastSyncTimeRef.current = now;
        isRunningRef.current = timerUpdate.IsRunning; // CRITICAL: Update isRunning ref!
        setLocalElapsedMs(serverMs);
        
        // Format the elapsed time to only show hours when needed
        let formattedTime = timerUpdate.ElapsedTime;

        // Prefer ElapsedMilliseconds if provided
        if (typeof timerUpdate.ElapsedMilliseconds === 'number') {
          const totalMs = Math.max(0, timerUpdate.ElapsedMilliseconds);
          const totalSeconds = Math.floor(totalMs / 1000);
          const hours = Math.floor(totalSeconds / 3600);
          const minutes = Math.floor((totalSeconds % 3600) / 60);
          const seconds = totalSeconds % 60;
          formattedTime = hours > 0
            ? `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`
            : `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
        } else if (timerUpdate.ElapsedTime && timerUpdate.ElapsedTime.includes(':')) {
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

  // Format milliseconds to display time
  const formatMilliseconds = useCallback((ms: number): string => {
    const totalSeconds = Math.floor(ms / 1000);
    const hours = Math.floor(totalSeconds / 3600);
    const minutes = Math.floor((totalSeconds % 3600) / 60);
    const seconds = totalSeconds % 60;
    
    return hours > 0
      ? `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`
      : `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
  }, []);

  // Use local interpolated time for smooth display
  const displayTime = useMemo(() => 
    formatMilliseconds(localElapsedMs), 
    [localElapsedMs, formatMilliseconds]
  );

  const currentElapsedSeconds = useMemo(() => 
    Math.floor(localElapsedMs / 1000),
    [localElapsedMs]
  );

  return {
    timerState: { ...timerState, elapsedTime: displayTime },
    currentElapsedSeconds,
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