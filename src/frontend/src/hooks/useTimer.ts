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
      
      console.log('Loading timer status for match:', matchId);
      const status = await timerService.getTimerStatus(matchId);
      console.log('Timer status received:', status);
      
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
      console.log('=== useTimer startTimer CALLED ===');
      console.log('Match ID:', matchId);
      console.log('Period Number:', periodNumber);
      console.log('Auto Connect:', autoConnect);
      
      setLoading(true);
      setError(null);
      
      console.log('Calling timerService.startTimer...');
      await timerService.startTimer(matchId, periodNumber);
      console.log('timerService.startTimer completed successfully');
      
      // Don't reload timer status - let SignalR handle the update
      console.log('Skipping loadTimerStatus - letting SignalR handle update');
      console.log('=== useTimer startTimer COMPLETED ===');
    } catch (err) {
      console.error('=== useTimer startTimer FAILED ===');
      console.error('Error starting timer:', err);
      setError(err instanceof Error ? err.message : 'Failed to start timer');
    } finally {
      setLoading(false);
    }
  }, [matchId, autoConnect]);

  // Stop timer
  const stopTimer = useCallback(async () => {
    try {
      console.log('=== useTimer stopTimer CALLED ===');
      console.log('Match ID:', matchId);
      console.log('Auto Connect:', autoConnect);
      
      setLoading(true);
      setError(null);
      
      console.log('Calling timerService.stopTimer...');
      await timerService.stopTimer(matchId);
      console.log('timerService.stopTimer completed successfully');
      
      // Don't reload timer status - let SignalR handle the update
      console.log('Skipping loadTimerStatus - letting SignalR handle update');
      console.log('=== useTimer stopTimer COMPLETED ===');
    } catch (err) {
      console.error('=== useTimer stopTimer FAILED ===');
      console.error('Error stopping timer:', err);
      setError(err instanceof Error ? err.message : 'Failed to stop timer');
    } finally {
      setLoading(false);
    }
  }, [matchId, autoConnect]);

  // Reset timer
  const resetTimer = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      
      console.log('Resetting timer for match:', matchId);
      await timerService.resetTimer(matchId);
      console.log('Timer reset successfully');
      
      // Don't reload timer status - let SignalR handle the update
      console.log('Skipping loadTimerStatus - letting SignalR handle update');
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
      
      console.log('Creating timer for match:', matchId);
      await timerService.createTimer(matchId);
      console.log('Timer created successfully');
      // Don't reload timer status - let SignalR handle the update
      console.log('Skipping loadTimerStatus - letting SignalR handle update');
    } catch (err) {
      console.error('Error creating timer:', err);
      // Don't set error for timer creation - it might already exist
      // Just log it and continue
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
      console.log('Timer destroyed successfully');
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
    // Only log very occasionally to avoid spam
    const shouldLog = Math.random() < 0.02; // Log ~2% of events
    
    if (shouldLog) {
      console.log('=== TIMER EVENT RECEIVED ===');
      console.log('Event type:', event.eventType);
      console.log('Event data:', event.data);
    }
    
    if (event.eventType === 'TimerUpdateEvent' && event.data) {
      const timerUpdate = event.data as TimerUpdate;
      
      if (timerUpdate.MatchId === matchId) {
        if (shouldLog) {
          console.log('✅ Processing timer update for match:', matchId);
          console.log('Timer state:', {
            isRunning: timerUpdate.IsRunning,
            elapsedTime: timerUpdate.ElapsedTime,
            periodNumber: timerUpdate.PeriodNumber
          });
        }
        
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
        
        setTimerState(prev => {
          return {
            isRunning: timerUpdate.IsRunning,
            elapsedTime: formattedTime,
            periodNumber: timerUpdate.PeriodNumber,
            lastUpdated: timerUpdate.LastUpdated,
          };
        });
        
        if (onTimerUpdate) {
          onTimerUpdate(timerUpdate);
        }
      } else {
        if (shouldLog) {
          console.log('❌ Match ID mismatch - ignoring event');
          console.log('Expected:', matchId);
          console.log('Received:', timerUpdate.MatchId);
        }
      }
    } else {
      if (shouldLog) {
        console.log('❌ Not a TimerUpdateEvent or no data');
      }
    }
    
    if (shouldLog) {
      console.log('=== END TIMER EVENT ===');
    }
  }, [matchId, onTimerUpdate, autoConnect]);

  // Update the ref with the latest callback
  handleTimerUpdateRef.current = handleTimerUpdate;

  // Setup SignalR connection and event handling
  useEffect(() => {
    let isActive = true; // Track if this effect is still active
    
    const setupSignalR = async () => {
      try {
        console.log('=== TIMER SIGNALR SETUP START ===');
        console.log('Setting up SignalR for timer with match ID:', matchId);
        console.log('Auto Connect:', autoConnect);
        
        // Only proceed if we should auto-connect and have a matchId
        if (!autoConnect || !matchId) {
          console.log('Skipping SignalR setup - autoConnect:', autoConnect, 'matchId:', matchId);
          return;
        }
        
        // Connect to SignalR
        console.log('Step 1: Connecting to SignalR...');
        await signalRService.connect();
        console.log('Step 1: SignalR connection completed');
        
        // Subscribe to match events (this is what we need for timer updates)
        console.log('Step 2: Subscribing to match events...');
        await signalRService.subscribeToMatch(matchId);
        console.log('Step 2: Match subscription completed');
        
        // Listen for timer events
        console.log('Step 3: Setting up event listener...');
        if (isActive) { // Only set up if still active
          unsubscribeRef.current = signalRService.onMatchEvent((event) => {
            if (handleTimerUpdateRef.current) {
              handleTimerUpdateRef.current(event);
            }
          });
          console.log('Step 3: Event listener setup completed');
        }
        
        console.log('SignalR setup completed for timer');
        
        // Load initial timer status to get current state immediately
        console.log('Step 4: Loading initial timer status...');
        await loadTimerStatus();
        console.log('Step 4: Initial timer status loaded');
        console.log('=== TIMER SIGNALR SETUP COMPLETE ===');
        
      } catch (err) {
        console.error('=== TIMER SIGNALR SETUP FAILED ===');
        console.error('Error setting up SignalR for timer:', err);
        // Don't set error - SignalR is not critical for basic functionality
      }
    };

    setupSignalR();

    // Cleanup on unmount
    return () => {
      isActive = false; // Mark as inactive
      console.log('=== TIMER SIGNALR CLEANUP ===');
      console.log('Cleaning up SignalR for timer');
      console.log('Match ID:', matchId);
      console.log('Auto Connect:', autoConnect);
      
      if (unsubscribeRef.current) {
        console.log('Calling unsubscribe function');
        unsubscribeRef.current();
        unsubscribeRef.current = null;
      }
      
      if (autoConnect && matchId) {
        console.log('Unsubscribing from match events');
        signalRService.unsubscribeFromMatch(matchId).catch(console.error);
      }
      
      console.log('=== TIMER SIGNALR CLEANUP COMPLETE ===');
    };
  }, [matchId, autoConnect]); // Removed handleTimerUpdate from dependencies

  return {
    timerState,
    loading,
    error,
    startTimer,
    stopTimer,
    resetTimer,
    createTimer,
    destroyTimer,
    loadTimerStatus,
  };
} 