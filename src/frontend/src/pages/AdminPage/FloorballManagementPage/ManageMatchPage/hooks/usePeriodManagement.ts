import { useState, useCallback } from 'react';
import { floorballMatchEventService } from '../../../../../api/floorball/floorballMatchEventService';
import type { FloorballMatchDto } from '../../../../../types/floorball/floorballTypes';
import type { PeriodEventData } from '../components/types';

interface UsePeriodManagementProps {
  currentMatch: FloorballMatchDto;
  currentPeriod: number;
  setCurrentPeriod: (period: number) => void;
  loadCurrentMatchStatus?: () => Promise<void>;
}

export const usePeriodManagement = ({
  currentMatch,
  currentPeriod,
  setCurrentPeriod,
  loadCurrentMatchStatus,
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
    if (eventData.matchId !== currentMatch.id) {
      return;
    }
    setStartedPeriods(prev => new Set([...prev, eventData.periodNumber]));
  }, [currentMatch.id]);

  /**
   * Ends the current period by sending API call
   */
  const endPeriod = useCallback(async () => {
    try {
      setPeriodLoading(prev => ({ ...prev, [currentPeriod]: true }));
      
      await floorballMatchEventService.endPeriod(currentMatch.id, currentPeriod);

      if (loadCurrentMatchStatus) {
        await loadCurrentMatchStatus();
      }
      
      // Mark this period as ended
      setEndedPeriods(prev => new Set([...prev, currentPeriod]));
      
      // Calculate the next period to start
      let nextPeriod = currentPeriod + 1;
      
      if (currentPeriod === 2) {
        nextPeriod = 3; // Overtime
      } else if (currentPeriod === 3) {
        nextPeriod = 4; // Shootout
      } else if (currentPeriod === 4) {
        nextPeriod = 0; // No more periods
      }
      
      setNextPeriodToStart(nextPeriod);
      
      // Update the period number
      if (nextPeriod > 0) {
        setCurrentPeriod(nextPeriod);
      }
      
    } catch (error) {
      console.error('Error ending period:', error);
      throw error;
    } finally {
      setPeriodLoading(prev => ({ ...prev, [currentPeriod]: false }));
    }
  }, [currentPeriod, currentMatch.id, setCurrentPeriod, loadCurrentMatchStatus]);

  /**
   * Starts a new period
   */
  const startPeriod = useCallback(async () => {
    try {
      setPeriodLoading(prev => ({ ...prev, [nextPeriodToStart]: true }));
      
      if (nextPeriodToStart === 3) {
        await floorballMatchEventService.recordOvertime(currentMatch.id);
        await floorballMatchEventService.startPeriod(currentMatch.id, 3);
        if (loadCurrentMatchStatus) {
          await loadCurrentMatchStatus();
        }
      } else if (nextPeriodToStart === 4) {
        await floorballMatchEventService.recordShootout(currentMatch.id);
        await floorballMatchEventService.startPeriod(currentMatch.id, 4);
        if (loadCurrentMatchStatus) {
          await loadCurrentMatchStatus();
        }
      } else {
        await floorballMatchEventService.startPeriod(currentMatch.id, nextPeriodToStart);
      }
      
      // Mark this period as started
      setStartedPeriods(prev => new Set([...prev, nextPeriodToStart]));
      setCurrentPeriod(nextPeriodToStart);
      
      // Update next period to start
      const upcoming = nextPeriodToStart + 1;
      setNextPeriodToStart(upcoming <= 4 ? upcoming : 0);
      
    } catch (error) {
      console.error('Error starting period:', error);
      throw error;
    } finally {
      setPeriodLoading(prev => ({ ...prev, [nextPeriodToStart]: false }));
    }
  }, [nextPeriodToStart, currentMatch.id, setCurrentPeriod, loadCurrentMatchStatus]);

  /**
   * Records overtime for the current match
   */
  const recordOvertime = useCallback(async () => {
    try {
      await floorballMatchEventService.recordOvertime(currentMatch.id);
      await floorballMatchEventService.startPeriod(currentMatch.id, 3);
      
      if (loadCurrentMatchStatus) {
        await loadCurrentMatchStatus();
      }
      
      setStartedPeriods(prev => new Set([...prev, 3]));
      setCurrentPeriod(3);
      setShowOvertimeConfirmation(false);
      
    } catch (error) {
      console.error('Error recording overtime:', error);
      throw error;
    }
  }, [currentMatch.id, setCurrentPeriod, loadCurrentMatchStatus]);

  /**
   * Records shootout for the current match
   */
  const recordShootout = useCallback(async () => {
    try {
      await floorballMatchEventService.recordShootout(currentMatch.id);
      await floorballMatchEventService.startPeriod(currentMatch.id, 4);
      
      if (loadCurrentMatchStatus) {
        await loadCurrentMatchStatus();
      }
      
      setStartedPeriods(prev => new Set([...prev, 4]));
      setCurrentPeriod(4);
      setShowShootoutConfirmation(false);
      
    } catch (error) {
      console.error('Error recording shootout:', error);
      throw error;
    }
  }, [currentMatch.id, setCurrentPeriod, loadCurrentMatchStatus]);

  /**
   * Determines if we can end the current period
   */
  const canEndPeriod = useCallback(() => {
    const conditions = {
      matchInProgress: currentMatch.status === 'InProgress',
      notLoading: !periodLoading[currentPeriod],
      periodStarted: startedPeriods.has(currentPeriod),
      periodNotEnded: !endedPeriods.has(currentPeriod),
      hasNextPeriod: nextPeriodToStart > 0,
      isShootout: currentPeriod === 4,
    };
    
    return conditions.matchInProgress && 
           conditions.notLoading &&
           conditions.periodStarted &&
           conditions.periodNotEnded &&
           (conditions.hasNextPeriod || conditions.isShootout);
  }, [currentMatch.status, periodLoading, currentPeriod, startedPeriods, endedPeriods, nextPeriodToStart]);

  /**
   * Gets the current period status for display
   */
  const getPeriodStatus = useCallback(() => {
    if (periodLoading[currentPeriod]) {
      return 'Processing...';
    }
    
    if (currentMatch.status === 'Completed') {
      return '🔴 Completed';
    }
    
    if (currentMatch.status === 'InProgress') {
      if (endedPeriods.has(currentPeriod)) {
        if (currentPeriod === 3) return '🔴 Overtime Ended';
        if (currentPeriod === 4) return '🔴 Shootout Ended';
        return '🔴 Ended';
      } else if (startedPeriods.has(currentPeriod)) {
        if (currentPeriod === 3) return '🟢 Overtime Started';
        if (currentPeriod === 4) return '🟢 Shootout Started';
        return '🟢 Started';
      } else {
        if (currentPeriod === 3) return '⏸️ Overtime Not Started';
        if (currentPeriod === 4) return '⏸️ Shootout Not Started';
        return '⏸️ Not Started';
      }
    }
    
    return '⏸️ Not Started';
  }, [periodLoading, currentPeriod, currentMatch.status, endedPeriods, startedPeriods]);

  /**
   * Gets the text for the period control button
   */
  const getPeriodControlButtonText = useCallback(() => {
    if (canEndPeriod()) {
      if (periodLoading[currentPeriod]) return 'Ending...';
      if (currentPeriod === 3) return '🔴 End Overtime';
      if (currentPeriod === 4) return '🏁 End Shootout';
      return 'End period';
    } else {
      if (periodLoading[nextPeriodToStart]) return 'Starting...';
      if (nextPeriodToStart === 3) return '⏰ Start Overtime';
      if (nextPeriodToStart === 4) return '🎯 Start Shootout';
      return `Start period ${nextPeriodToStart}`;
    }
  }, [canEndPeriod, periodLoading, currentPeriod, nextPeriodToStart]);

  const isInOvertime = useCallback(() => currentPeriod === 3, [currentPeriod]);
  const isInShootout = useCallback(() => currentPeriod === 4, [currentPeriod]);

  const confirmEndPeriod = useCallback(() => {
    if (pendingEndPeriodAction) {
      pendingEndPeriodAction();
      setPendingEndPeriodAction(null);
    }
    setShowEndPeriodConfirmation(false);
  }, [pendingEndPeriodAction]);

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
    isInShootout,
  };
};
