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
    elapsedTime: '00:00:00',
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
      
      const status = await timerService.getTimerStatus(matchId);
      
      setTimerState({
        isRunning: status.isRunning,
        elapsedTime: status.elapsedTime,
        lastUpdated: new Date().toISOString(),
      });
    } catch (err) {
      console.error('Error loading timer status:', err);
      setError(err instanceof Error ? err.message : 'Failed to load timer status');
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
      console.log('Timer started successfully');
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
      console.log('Timer stopped successfully');
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
      console.log('Timer reset successfully');
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
      console.log('Timer created successfully');
      await loadTimerStatus(); // Reload status after creation
    } catch (err) {
      console.error('Error creating timer:', err);
      setError(err instanceof Error ? err.message : 'Failed to create timer');
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
        elapsedTime: '00:00:00',
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
    if (event.eventType === 'DomainEvent' && event.data) {
      const timerUpdate = event.data as TimerUpdate;
      
      if (timerUpdate.matchId === matchId) {
        console.log('Received timer update:', timerUpdate);
        
        setTimerState({
          isRunning: timerUpdate.isRunning,
          elapsedTime: timerUpdate.elapsedTime,
          periodNumber: timerUpdate.periodNumber,
          lastUpdated: timerUpdate.lastUpdated,
        });
        
        if (onTimerUpdate) {
          onTimerUpdate(timerUpdate);
        }
      }
    }
  }, [matchId, onTimerUpdate]);

  // Setup SignalR connection and event handling
  useEffect(() => {
    if (!autoConnect) return;

    const setupSignalR = async () => {
      try {
        // Connect to SignalR
        await signalRService.connect();
        
        // Subscribe to match events
        await signalRService.subscribeToMatch(matchId);
        
        // Subscribe to timer event types
        await signalRService.subscribeToEventType('TimerStarted');
        await signalRService.subscribeToEventType('TimerStopped');
        await signalRService.subscribeToEventType('TimerReset');
        await signalRService.subscribeToEventType('TimerUpdate');
        
        // Listen for timer events
        unsubscribeRef.current = signalRService.onMatchEvent(handleTimerUpdate);
        
        // Load initial timer status
        await loadTimerStatus();
        
      } catch (err) {
        console.error('Error setting up SignalR for timer:', err);
        setError('Failed to connect to real-time updates');
      }
    };

    setupSignalR();

    // Cleanup on unmount
    return () => {
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