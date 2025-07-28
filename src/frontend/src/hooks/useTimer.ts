import { useState, useEffect, useCallback, useRef } from 'react';
import { signalRService } from '../services/signalRService';
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

  // Load initial timer status
  const loadTimerStatus = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      
      console.log('Loading timer status for match:', matchId);
      const status = await timerService.getTimerStatus(matchId);
      console.log('Timer status received:', status);
      
      // Format the elapsed time: show mm:ss for initial state, hh:mm:ss for running time
      let formattedTime = status.elapsedTime;
      if (status.elapsedTime === '00:00:00') {
        formattedTime = '00:00';
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
      
      setLoading(true);
      setError(null);
      
      console.log('Calling timerService.startTimer...');
      await timerService.startTimer(matchId, periodNumber);
      console.log('timerService.startTimer completed successfully');
      
      // Reload timer status after starting
      console.log('Reloading timer status...');
      await loadTimerStatus();
      console.log('Timer status reloaded');
      console.log('=== useTimer startTimer COMPLETED ===');
    } catch (err) {
      console.error('=== useTimer startTimer FAILED ===');
      console.error('Error starting timer:', err);
      setError(err instanceof Error ? err.message : 'Failed to start timer');
    } finally {
      setLoading(false);
    }
  }, [matchId, loadTimerStatus]);

  // Stop timer
  const stopTimer = useCallback(async () => {
    try {
      console.log('=== useTimer stopTimer CALLED ===');
      console.log('Match ID:', matchId);
      
      setLoading(true);
      setError(null);
      
      console.log('Calling timerService.stopTimer...');
      await timerService.stopTimer(matchId);
      console.log('timerService.stopTimer completed successfully');
      
      // Reload timer status after stopping
      console.log('Reloading timer status...');
      await loadTimerStatus();
      console.log('Timer status reloaded');
      console.log('=== useTimer stopTimer COMPLETED ===');
    } catch (err) {
      console.error('=== useTimer stopTimer FAILED ===');
      console.error('Error stopping timer:', err);
      setError(err instanceof Error ? err.message : 'Failed to stop timer');
    } finally {
      setLoading(false);
    }
  }, [matchId, loadTimerStatus]);

  // Reset timer
  const resetTimer = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      
      console.log('Resetting timer for match:', matchId);
      await timerService.resetTimer(matchId);
      console.log('Timer reset successfully');
      
      // Reload timer status after resetting
      await loadTimerStatus();
    } catch (err) {
      console.error('Error resetting timer:', err);
      setError(err instanceof Error ? err.message : 'Failed to reset timer');
    } finally {
      setLoading(false);
    }
  }, [matchId, loadTimerStatus]);

  // Create timer
  const createTimer = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      
      console.log('Creating timer for match:', matchId);
      await timerService.createTimer(matchId);
      console.log('Timer created successfully');
      await loadTimerStatus(); // Reload status after creation
    } catch (err) {
      console.error('Error creating timer:', err);
      // Don't set error for timer creation - it might already exist
      // Just log it and continue
    } finally {
      setLoading(false);
    }
  }, [matchId, loadTimerStatus]);

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
  const handleTimerUpdate = useCallback((event: { eventType: string; data: unknown }) => {
    console.log('=== TIMER EVENT RECEIVED ===');
    console.log('Received SignalR event:', event);
    console.log('Event type:', event.eventType);
    console.log('Event data:', event.data);
    console.log('Current match ID:', matchId);
    
    // Handle timer update events - check for TimerUpdateEvent and look at the EventType in data
    if (event.eventType === 'TimerUpdateEvent' && event.data) {
      const timerUpdate = event.data as TimerUpdate;
      console.log('Parsed timer update:', timerUpdate);
      console.log('Timer event type:', timerUpdate.EventType);
      console.log('Timer match ID:', timerUpdate.MatchId);
      console.log('Timer elapsed time:', timerUpdate.ElapsedTime);
      console.log('Timer is running:', timerUpdate.IsRunning);
      
      if (timerUpdate.MatchId === matchId) {
        console.log('✅ MATCH ID MATCHES - UPDATING TIMER STATE');
        
        // Format the elapsed time: show mm:ss for initial state, hh:mm:ss for running time
        let formattedTime = timerUpdate.ElapsedTime;
        if (timerUpdate.ElapsedTime === '00:00:00') {
          formattedTime = '00:00';
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
      } else {
        console.log('❌ Match ID mismatch - ignoring event');
        console.log('Expected:', matchId);
        console.log('Received:', timerUpdate.MatchId);
      }
    } else {
      console.log('❌ Not a TimerUpdateEvent or no data');
    }
    console.log('=== END TIMER EVENT ===');
  }, [matchId, onTimerUpdate]);

  // Setup SignalR connection and event handling
  useEffect(() => {
    if (!autoConnect || !matchId) return;

    const setupSignalR = async () => {
      try {
        console.log('=== TIMER SIGNALR SETUP START ===');
        console.log('Setting up SignalR for timer with match ID:', matchId);
        
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
        unsubscribeRef.current = signalRService.onMatchEvent(handleTimerUpdate);
        console.log('Step 3: Event listener setup completed');
        
        console.log('SignalR setup completed for timer');
        
        // Load initial timer status
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
      console.log('=== TIMER SIGNALR CLEANUP ===');
      console.log('Cleaning up SignalR for timer');
      if (unsubscribeRef.current) {
        unsubscribeRef.current();
      }
      signalRService.unsubscribeFromMatch(matchId).catch(console.error);
    };
  }, [matchId, autoConnect, handleTimerUpdate, loadTimerStatus]);

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