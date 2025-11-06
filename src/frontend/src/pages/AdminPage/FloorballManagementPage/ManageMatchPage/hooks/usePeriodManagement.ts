import { useState, useCallback } from 'react';
import { floorballMatchEventService } from '../../../../../api/floorball/floorballMatchEventService';
import type { FloorballMatchDto } from '../../../../../types/floorball/floorballTypes';
import type { LocalClock, PeriodEventData, StateUpdate } from '../components/types';

interface UsePeriodManagementProps {
  currentMatch: FloorballMatchDto;
  clock: LocalClock;
  setLocalClock: (clock: LocalClock | ((prev: LocalClock) => LocalClock)) => void;
  currentTimerElapsedTime: number;
  isOpen: boolean;
  onStateUpdate?: (updates: StateUpdate) => void;
}

export const usePeriodManagement = ({
  currentMatch,
  clock,
  setLocalClock,
  currentTimerElapsedTime,
  onStateUpdate
}: UsePeriodManagementProps) => {
  // State for tracking which periods have been started and ended
  const [startedPeriods, setStartedPeriods] = useState<Set<number>>(new Set());
  const [endedPeriods, setEndedPeriods] = useState<Set<number>>(new Set());
  const [nextPeriodToStart, setNextPeriodToStart] = useState<number>(1);
  const [periodLoading, setPeriodLoading] = useState<Record<number, boolean>>({});
  
  // Confirmation dialog states
  const [showEndPeriodConfirmation, setShowEndPeriodConfirmation] = useState(false);
  const [pendingEndPeriodAction, setPendingEndPeriodAction] = useState<(() => void) | null>(null);
  const [showOvertimeConfirmation, setShowOvertimeConfirmation] = useState(false);
  const [showShootoutConfirmation, setShowShootoutConfirmation] = useState(false);

  /**
   * Handles real-time period started events from SignalR
   */
  const handlePeriodStarted = useCallback((eventData: PeriodEventData) => {
    console.log('handlePeriodStarted called with eventData:', eventData);
    
    if (eventData.matchId !== currentMatch.id) {
      console.log('Period started event is for different match, ignoring');
      return;
    }
    
    // Mark this period as started in our local state
    setStartedPeriods(prev => new Set([...prev, eventData.periodNumber]));
    
    console.log('Period started event handled');
  }, [currentMatch.id]);

  /**
   * Ends the current period by sending API call
   */
  const endPeriod = useCallback(async () => {
    try {
      setPeriodLoading(prev => ({ ...prev, [clock.period]: true }));
      
      console.log(`Ending period ${clock.period} at ${currentTimerElapsedTime} seconds for match ${currentMatch.id}`);
      
      await floorballMatchEventService.endPeriod(currentMatch.id, clock.period);
      console.log(`Ended period ${clock.period} for match ${currentMatch.id}`);
      
      // Mark this period as ended
      setEndedPeriods(prev => new Set([...prev, clock.period]));
      
      // Calculate the next period to start
      let nextPeriod = clock.period + 1;
      
      // Allow progression to overtime and shootout regardless of score
      if (clock.period === 3) {
        nextPeriod = 4; // Overtime
        console.log('Regular periods ended, transitioning to overtime');
      } else if (clock.period === 4) {
        nextPeriod = 5; // Shootout
        console.log('Overtime ended, transitioning to shootout');
      } else if (clock.period === 5) {
        // After shootout, no more periods
        nextPeriod = 0;
        console.log('Shootout ended, match is complete');
      }
      
      // Set the next period to start
      setNextPeriodToStart(nextPeriod);
      
      // Update the clock to the next period
      if (nextPeriod > 0) {
        const newClock = { 
          period: nextPeriod, 
          minutes: 0, 
          seconds: 0, 
          isRunning: false 
        };
        setLocalClock(newClock);
        if (onStateUpdate) {
          onStateUpdate({
            clock: newClock
          });
        }
      }
      
    } catch (error) {
      console.error('Error ending period:', error);
      throw error;
    } finally {
      setPeriodLoading(prev => ({ ...prev, [clock.period]: false }));
    }
  }, [clock.period, currentMatch.id, currentTimerElapsedTime, setLocalClock, onStateUpdate]);

  /**
   * Starts a new period
   */
  const startPeriod = useCallback(async () => {
    try {
      setPeriodLoading(prev => ({ ...prev, [nextPeriodToStart]: true }));
      
      console.log(`Starting period ${nextPeriodToStart} for match ${currentMatch.id}`);
      
      // Start the period via API (server-managed for 1; record phase for 4/5)
      if (nextPeriodToStart === 4) {
        await floorballMatchEventService.recordOvertime(currentMatch.id);
        console.log(`Recorded overtime for match ${currentMatch.id}`);
      } else if (nextPeriodToStart === 5) {
        await floorballMatchEventService.recordShootout(currentMatch.id);
        console.log(`Recorded shootout for match ${currentMatch.id}`);
      } else {
        await floorballMatchEventService.startPeriod(currentMatch.id, nextPeriodToStart);
        console.log(`Started period ${nextPeriodToStart} for match ${currentMatch.id}`);
      }
      
      // Mark this period as started
      setStartedPeriods(prev => new Set([...prev, nextPeriodToStart]));
      
      // Update the clock to the new period
      const newClock = { 
        period: nextPeriodToStart, 
        minutes: 0, 
        seconds: 0, 
        isRunning: false 
      };
      setLocalClock(newClock);
      if (onStateUpdate) {
        onStateUpdate({
          clock: newClock
        });
      }
      
      // Update next period to start
      setNextPeriodToStart(nextPeriodToStart + 1);
      
    } catch (error) {
      console.error('Error starting period:', error);
      throw error;
    } finally {
      setPeriodLoading(prev => ({ ...prev, [nextPeriodToStart]: false }));
    }
  }, [nextPeriodToStart, currentMatch.id, setLocalClock, onStateUpdate]);

  /**
   * Records overtime for the current match
   */
  const recordOvertime = useCallback(async () => {
    try {
      await floorballMatchEventService.recordOvertime(currentMatch.id);
      
      // Start the overtime period (period 4)
      await floorballMatchEventService.startPeriod(currentMatch.id, 4);
      
      // Update the clock to period 4
      const newClock = { 
        period: 4, 
        minutes: 0, 
        seconds: 0, 
        isRunning: false 
      };
      setLocalClock(newClock);
      // Ensure local state reflects that OT has started
      setStartedPeriods(prev => new Set([...prev, 4]));
      if (onStateUpdate) {
        onStateUpdate({
          clock: newClock
        });
      }
      
      setShowOvertimeConfirmation(false);
      
    } catch (error) {
      console.error('Error recording overtime:', error);
      throw error;
    }
  }, [currentMatch.id, setLocalClock, onStateUpdate]);

  /**
   * Records shootout for the current match
   */
  const recordShootout = useCallback(async () => {
    try {
      await floorballMatchEventService.recordShootout(currentMatch.id);
      
      // Start the shootout period (period 5)
      await floorballMatchEventService.startPeriod(currentMatch.id, 5);
      
      // Update the clock to period 5
      const newClock = { 
        period: 5, 
        minutes: 0, 
        seconds: 0, 
        isRunning: false 
      };
      setLocalClock(newClock);
      // Ensure local state reflects that shootout has started
      setStartedPeriods(prev => new Set([...prev, 5]));
      if (onStateUpdate) {
        onStateUpdate({
          clock: newClock
        });
      }
      
      setShowShootoutConfirmation(false);
      
    } catch (error) {
      console.error('Error recording shootout:', error);
      throw error;
    }
  }, [currentMatch.id, setLocalClock, onStateUpdate]);

  /**
   * Determines if we can end the current period
   */
  const canEndPeriod = useCallback(() => {
    const conditions = {
      matchInProgress: currentMatch.status === 'InProgress',
      notLoading: !periodLoading[clock.period],
      periodStarted: startedPeriods.has(clock.period),
      periodNotEnded: !endedPeriods.has(clock.period),
      hasNextPeriod: nextPeriodToStart > 0
    };
    
    const canEnd = conditions.matchInProgress && 
                   conditions.notLoading &&
                   conditions.periodStarted &&
                   conditions.periodNotEnded &&
                   conditions.hasNextPeriod;
        
    return canEnd;
  }, [currentMatch.status, periodLoading, clock.period, startedPeriods, endedPeriods, nextPeriodToStart]);

  /**
   * Gets the current period status for display
   */
  const getPeriodStatus = useCallback(() => {
    if (periodLoading[clock.period]) {
      return 'Processing...';
    }
    
    if (currentMatch.status === 'Completed') {
      return '🔴 Completed';
    }
    
    if (currentMatch.status === 'InProgress') {
      if (endedPeriods.has(clock.period)) {
        if (clock.period === 4) {
          return '🔴 Overtime Ended';
        } else if (clock.period === 5) {
          return '🔴 Shootout Ended';
        } else {
          return '🔴 Ended';
        }
      } else if (startedPeriods.has(clock.period)) {
        if (clock.period === 4) {
          return '🟢 Overtime Started';
        } else if (clock.period === 5) {
          return '🟢 Shootout Started';
        } else {
          return '🟢 Started';
        }
      } else {
        if (clock.period === 4) {
          return '⏸️ Overtime Not Started';
        } else if (clock.period === 5) {
          return '⏸️ Shootout Not Started';
        } else {
          return '⏸️ Not Started';
        }
      }
    }
    
    return '⏸️ Not Started';
  }, [periodLoading, clock.period, currentMatch.status, endedPeriods, startedPeriods]);

  /**
   * Gets the text for the period control button
   */
  const getPeriodControlButtonText = useCallback(() => {
    // If we can end a period, show end period text
    if (canEndPeriod()) {
      if (periodLoading[clock.period]) {
        return 'Ending...';
      }
      
      if (clock.period === 4) {
        return '🔴 End Overtime';
      }
      
      if (clock.period === 5) {
        return '🔴 End Shootout';
      }
      
      return '🔴 End Period';
    } else {
      // Show start period text
      if (periodLoading[nextPeriodToStart]) {
        return 'Starting...';
      }
      
      if (nextPeriodToStart === 4) {
        return '⏰ Start Overtime';
      }
      
      if (nextPeriodToStart === 5) {
        return '🎯 Start Shootout';
      }
      
      return `🟢 Start Period ${nextPeriodToStart}`;
    }
  }, [canEndPeriod, periodLoading, clock.period, nextPeriodToStart]);

  /**
   * Determines if we're currently in overtime
   */
  const isInOvertime = useCallback(() => clock.period === 4, [clock.period]);

  /**
   * Determines if we're currently in shootout
   */
  const isInShootout = useCallback(() => clock.period === 5, [clock.period]);

  /**
   * Confirms the end period action
   */
  const confirmEndPeriod = useCallback(() => {
    if (pendingEndPeriodAction) {
      pendingEndPeriodAction();
      setPendingEndPeriodAction(null);
    }
    setShowEndPeriodConfirmation(false);
  }, [pendingEndPeriodAction]);

  /**
   * Cancels the end period action
   */
  const cancelEndPeriod = useCallback(() => {
    setPendingEndPeriodAction(null);
    setShowEndPeriodConfirmation(false);
  }, []);

  return {
    // State
    startedPeriods,
    setStartedPeriods,
    endedPeriods,
    setEndedPeriods,
    nextPeriodToStart,
    setNextPeriodToStart,
    periodLoading,
    
    // Confirmation dialogs
    showEndPeriodConfirmation,
    setShowEndPeriodConfirmation,
    pendingEndPeriodAction,
    setPendingEndPeriodAction,
    showOvertimeConfirmation,
    setShowOvertimeConfirmation,
    showShootoutConfirmation,
    setShowShootoutConfirmation,
    
    // Actions
    handlePeriodStarted,
    endPeriod,
    startPeriod,
    recordOvertime,
    recordShootout,
    confirmEndPeriod,
    cancelEndPeriod,
    
    // Utility functions
    canEndPeriod,
    getPeriodStatus,
    getPeriodControlButtonText,
    isInOvertime,
    isInShootout
  };
}; 