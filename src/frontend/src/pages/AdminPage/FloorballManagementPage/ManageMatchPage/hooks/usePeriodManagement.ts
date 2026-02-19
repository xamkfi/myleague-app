import { useState, useCallback, useMemo } from 'react';
import { floorballMatchEventService } from '../../../../../api/floorball/floorballMatchEventService';
import type { FloorballMatchDto } from '../../../../../types/floorball/floorballTypes';
import type { PeriodEventData } from '../components/types';

interface UsePeriodManagementProps {
  currentMatch: FloorballMatchDto;
  currentPeriod: number;
  setCurrentPeriod: (period: number) => void;
  loadCurrentMatchStatus?: () => Promise<void>;
}

/**
 * Default match rules used as fallback when matchRules is not present on the DTO
 * (e.g., for matches created before the match rules feature was added).
 */
const DEFAULT_MATCH_RULES = {
  numberOfPeriods: 2,
  periodDurationMinutes: 15,
  allowOvertime: true,
  overtimeDurationMinutes: 5,
  allowShootout: true,
};

export const usePeriodManagement = ({
  currentMatch,
  currentPeriod,
  setCurrentPeriod,
  loadCurrentMatchStatus,
}: UsePeriodManagementProps) => {
  // Derive dynamic period numbers from match rules
  const rules = currentMatch.matchRules ?? DEFAULT_MATCH_RULES;
  const overtimePeriodNumber = useMemo(() => rules.numberOfPeriods + 1, [rules.numberOfPeriods]);
  const shootoutPeriodNumber = useMemo(() => rules.numberOfPeriods + 2, [rules.numberOfPeriods]);
  const maxPeriodNumber = useMemo(() => {
    if (rules.allowShootout) return shootoutPeriodNumber;
    if (rules.allowOvertime) return overtimePeriodNumber;
    return rules.numberOfPeriods;
  }, [rules, overtimePeriodNumber, shootoutPeriodNumber]);

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
   * Ends the current period by sending API call.
   * Calculates the next period dynamically based on match rules.
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
      
      // Calculate the next period to start based on match rules
      let nextPeriod: number;
      
      if (currentPeriod < rules.numberOfPeriods) {
        // Still in regular periods, advance to next regular period
        nextPeriod = currentPeriod + 1;
      } else if (currentPeriod === rules.numberOfPeriods && rules.allowOvertime) {
        // Last regular period ended, overtime is available
        nextPeriod = overtimePeriodNumber;
      } else if (currentPeriod === rules.numberOfPeriods && !rules.allowOvertime && rules.allowShootout) {
        // Last regular period ended, no overtime but shootout is available
        nextPeriod = shootoutPeriodNumber;
      } else if (currentPeriod === overtimePeriodNumber && rules.allowShootout) {
        // Overtime ended, shootout is available
        nextPeriod = shootoutPeriodNumber;
      } else {
        // No more periods available (or shootout just ended)
        nextPeriod = 0;
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
  }, [currentPeriod, currentMatch.id, setCurrentPeriod, rules, overtimePeriodNumber, shootoutPeriodNumber]);

  /**
   * Starts a new period.
   * For overtime/shootout periods, also records the overtime/shootout on the match.
   */
  const startPeriod = useCallback(async () => {
    try {
      setPeriodLoading(prev => ({ ...prev, [nextPeriodToStart]: true }));
      
      if (nextPeriodToStart === overtimePeriodNumber) {
        await floorballMatchEventService.recordOvertime(currentMatch.id);
        await floorballMatchEventService.startPeriod(currentMatch.id, overtimePeriodNumber);
        if (loadCurrentMatchStatus) {
          await loadCurrentMatchStatus();
        }
      } else if (nextPeriodToStart === shootoutPeriodNumber) {
        await floorballMatchEventService.recordShootout(currentMatch.id);
        await floorballMatchEventService.startPeriod(currentMatch.id, shootoutPeriodNumber);
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
      let upcoming: number;
      if (nextPeriodToStart < rules.numberOfPeriods) {
        upcoming = nextPeriodToStart + 1;
      } else if (nextPeriodToStart === rules.numberOfPeriods && rules.allowOvertime) {
        upcoming = overtimePeriodNumber;
      } else if (nextPeriodToStart === rules.numberOfPeriods && !rules.allowOvertime && rules.allowShootout) {
        upcoming = shootoutPeriodNumber;
      } else if (nextPeriodToStart === overtimePeriodNumber && rules.allowShootout) {
        upcoming = shootoutPeriodNumber;
      } else {
        upcoming = 0;
      }
      setNextPeriodToStart(upcoming);
      
    } catch (error) {
      console.error('Error starting period:', error);
      throw error;
    } finally {
      setPeriodLoading(prev => ({ ...prev, [nextPeriodToStart]: false }));
    }
  }, [nextPeriodToStart, currentMatch.id, setCurrentPeriod, loadCurrentMatchStatus, rules, overtimePeriodNumber, shootoutPeriodNumber]);

  /**
   * Records overtime for the current match
   */
  const recordOvertime = useCallback(async () => {
    try {
      await floorballMatchEventService.recordOvertime(currentMatch.id);
      await floorballMatchEventService.startPeriod(currentMatch.id, overtimePeriodNumber);
      
      if (loadCurrentMatchStatus) {
        await loadCurrentMatchStatus();
      }
      
      setStartedPeriods(prev => new Set([...prev, overtimePeriodNumber]));
      setCurrentPeriod(overtimePeriodNumber);
      setShowOvertimeConfirmation(false);
      
    } catch (error) {
      console.error('Error recording overtime:', error);
      throw error;
    }
  }, [currentMatch.id, setCurrentPeriod, loadCurrentMatchStatus, overtimePeriodNumber]);

  /**
   * Records shootout for the current match
   */
  const recordShootout = useCallback(async () => {
    try {
      await floorballMatchEventService.recordShootout(currentMatch.id);
      await floorballMatchEventService.startPeriod(currentMatch.id, shootoutPeriodNumber);
      
      if (loadCurrentMatchStatus) {
        await loadCurrentMatchStatus();
      }
      
      setStartedPeriods(prev => new Set([...prev, shootoutPeriodNumber]));
      setCurrentPeriod(shootoutPeriodNumber);
      setShowShootoutConfirmation(false);
      
    } catch (error) {
      console.error('Error recording shootout:', error);
      throw error;
    }
  }, [currentMatch.id, setCurrentPeriod, loadCurrentMatchStatus, shootoutPeriodNumber]);

  /**
   * Determines if we can end the current period
   */
  const canEndPeriod = useCallback(() => {
    const isShootout = currentPeriod === shootoutPeriodNumber;
    const isLastAllowedPeriod = currentPeriod === maxPeriodNumber;
    
    const conditions = {
      matchInProgress: currentMatch.status === 'InProgress',
      notLoading: !periodLoading[currentPeriod],
      periodStarted: startedPeriods.has(currentPeriod),
      periodNotEnded: !endedPeriods.has(currentPeriod),
      hasNextPeriod: nextPeriodToStart > 0,
    };
    
    return conditions.matchInProgress && 
           conditions.notLoading &&
           conditions.periodStarted &&
           conditions.periodNotEnded &&
           (conditions.hasNextPeriod || isShootout || isLastAllowedPeriod);
  }, [currentMatch.status, periodLoading, currentPeriod, startedPeriods, endedPeriods, nextPeriodToStart, shootoutPeriodNumber, maxPeriodNumber]);

  /**
   * Gets the current period status for display
   */
  const getPeriodStatus = useCallback(() => {
    if (periodLoading[currentPeriod]) {
      return 'Processing...';
    }
    
    if (currentMatch.status === 'Completed') {
      return 'Completed';
    }
    
    const isOvertime = currentPeriod === overtimePeriodNumber;
    const isShootout = currentPeriod === shootoutPeriodNumber;
    
    if (currentMatch.status === 'InProgress') {
      if (endedPeriods.has(currentPeriod)) {
        if (isOvertime) return 'Overtime Ended';
        if (isShootout) return 'Shootout Ended';
        return 'Ended';
      } else if (startedPeriods.has(currentPeriod)) {
        if (isOvertime) return 'Overtime Started';
        if (isShootout) return 'Shootout Started';
        return 'Started';
      } else {
        if (isOvertime) return 'Overtime Not Started';
        if (isShootout) return 'Shootout Not Started';
        return 'Not Started';
      }
    }
    
    return 'Not Started';
  }, [periodLoading, currentPeriod, currentMatch.status, endedPeriods, startedPeriods, overtimePeriodNumber, shootoutPeriodNumber]);

  /**
   * Gets the text for the period control button
   */
  const getPeriodControlButtonText = useCallback(() => {
    if (canEndPeriod()) {
      if (periodLoading[currentPeriod]) return 'Ending...';
      if (currentPeriod === overtimePeriodNumber) return 'End Overtime';
      if (currentPeriod === shootoutPeriodNumber) return 'End Shootout';
      return 'End period';
    } else {
      if (periodLoading[nextPeriodToStart]) return 'Starting...';
      if (nextPeriodToStart === overtimePeriodNumber) return 'Start Overtime';
      if (nextPeriodToStart === shootoutPeriodNumber) return 'Start Shootout';
      return `Start period ${nextPeriodToStart}`;
    }
  }, [canEndPeriod, periodLoading, currentPeriod, nextPeriodToStart, overtimePeriodNumber, shootoutPeriodNumber]);

  const isInOvertime = useCallback(() => currentPeriod === overtimePeriodNumber, [currentPeriod, overtimePeriodNumber]);
  const isInShootout = useCallback(() => currentPeriod === shootoutPeriodNumber, [currentPeriod, shootoutPeriodNumber]);

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
    
    // Dynamic period numbers derived from match rules
    overtimePeriodNumber,
    shootoutPeriodNumber,
    maxPeriodNumber,
    matchRules: rules,
    
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
