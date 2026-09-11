import { useState, useCallback, useMemo } from 'react';
import { footballMatchEventService } from '../../../../../api/football/footballMatchEventService';
import type { FootballMatchDto, FootballMatchRules } from '../../../../../types/football/footballTypes';
import type { PeriodEventData } from '../components/types';
import {
  extraTimeStartPeriod,
  getPeriodLabel,
  isExtraTimePeriod,
  isPenaltyShootoutPeriod,
  maxPeriodNumber as computeMaxPeriodNumber,
  penaltyShootoutPeriod,
  resolveMatchRules,
} from '../utils/lineupValidation';

interface UsePeriodManagementProps {
  currentMatch: FootballMatchDto;
  currentPeriod: number;
  setCurrentPeriod: (period: number) => void;
  loadCurrentMatchStatus?: () => Promise<void>;
}

const nextPeriodAfter = (endedPeriod: number, rules: FootballMatchRules): number => {
  if (endedPeriod < rules.numberOfHalves) {
    return endedPeriod + 1;
  }
  if (endedPeriod === rules.numberOfHalves && rules.allowExtraTime) {
    return extraTimeStartPeriod(rules);
  }
  if (endedPeriod === rules.numberOfHalves && !rules.allowExtraTime && rules.allowPenaltyShootout) {
    return penaltyShootoutPeriod(rules);
  }
  if (isExtraTimePeriod(endedPeriod, rules)) {
    const lastExtraTimeHalf = extraTimeStartPeriod(rules) + rules.extraTimeHalfCount - 1;
    if (endedPeriod < lastExtraTimeHalf) {
      return endedPeriod + 1;
    }
    if (rules.allowPenaltyShootout) {
      return penaltyShootoutPeriod(rules);
    }
  }
  return 0;
};

export const usePeriodManagement = ({
  currentMatch,
  currentPeriod,
  setCurrentPeriod,
  loadCurrentMatchStatus,
}: UsePeriodManagementProps) => {
  const rules = resolveMatchRules(currentMatch.matchRules);
  const extraTimePeriodNumber = useMemo(() => extraTimeStartPeriod(rules), [rules]);
  const shootoutPeriodNumber = useMemo(() => penaltyShootoutPeriod(rules), [rules]);
  const maxPeriodNumber = useMemo(() => computeMaxPeriodNumber(rules), [rules]);

  const [startedPeriods, setStartedPeriods] = useState<Set<number>>(new Set());
  const [endedPeriods, setEndedPeriods] = useState<Set<number>>(new Set());
  const [nextPeriodToStart, setNextPeriodToStart] = useState<number>(1);
  const [periodLoading, setPeriodLoading] = useState<Record<number, boolean>>({});

  const [showEndPeriodConfirmation, setShowEndPeriodConfirmation] = useState(false);
  const [pendingEndPeriodAction, setPendingEndPeriodAction] = useState<(() => void) | null>(null);
  const [showOvertimeConfirmation, setShowOvertimeConfirmation] = useState(false);
  const [showShootoutConfirmation, setShowShootoutConfirmation] = useState(false);

  const handlePeriodStarted = useCallback((eventData: PeriodEventData) => {
    if (eventData.matchId !== currentMatch.id) {
      return;
    }
    setStartedPeriods((prev) => new Set([...prev, eventData.periodNumber]));
  }, [currentMatch.id]);

  const endPeriod = useCallback(async () => {
    try {
      setPeriodLoading((prev) => ({ ...prev, [currentPeriod]: true }));

      await footballMatchEventService.endPeriod(currentMatch.id, currentPeriod);

      if (loadCurrentMatchStatus) {
        await loadCurrentMatchStatus();
      }

      setEndedPeriods((prev) => new Set([...prev, currentPeriod]));

      const nextPeriod = nextPeriodAfter(currentPeriod, rules);
      setNextPeriodToStart(nextPeriod);
      if (nextPeriod > 0) {
        setCurrentPeriod(nextPeriod);
      }
    } catch (error) {
      console.error('Error ending half:', error);
      throw error;
    } finally {
      setPeriodLoading((prev) => ({ ...prev, [currentPeriod]: false }));
    }
  }, [currentPeriod, currentMatch.id, setCurrentPeriod, rules, loadCurrentMatchStatus]);

  const startPeriod = useCallback(async () => {
    try {
      setPeriodLoading((prev) => ({ ...prev, [nextPeriodToStart]: true }));

      if (isExtraTimePeriod(nextPeriodToStart, rules) && !currentMatch.wentToExtraTime) {
        await footballMatchEventService.recordExtraTime(currentMatch.id);
        if (loadCurrentMatchStatus) {
          await loadCurrentMatchStatus();
        }
      } else if (isPenaltyShootoutPeriod(nextPeriodToStart, rules) && !currentMatch.wentToPenaltyShootout) {
        await footballMatchEventService.recordPenaltyShootout(currentMatch.id);
        if (loadCurrentMatchStatus) {
          await loadCurrentMatchStatus();
        }
      }

      await footballMatchEventService.startPeriod(currentMatch.id, nextPeriodToStart);

      setStartedPeriods((prev) => new Set([...prev, nextPeriodToStart]));
      setCurrentPeriod(nextPeriodToStart);
      setNextPeriodToStart(nextPeriodAfter(nextPeriodToStart, rules));
    } catch (error) {
      console.error('Error starting half:', error);
      throw error;
    } finally {
      setPeriodLoading((prev) => ({ ...prev, [nextPeriodToStart]: false }));
    }
  }, [
    nextPeriodToStart,
    currentMatch.id,
    currentMatch.wentToExtraTime,
    currentMatch.wentToPenaltyShootout,
    setCurrentPeriod,
    loadCurrentMatchStatus,
    rules,
  ]);

  const recordOvertime = useCallback(async () => {
    try {
      await footballMatchEventService.recordExtraTime(currentMatch.id);
      if (loadCurrentMatchStatus) {
        await loadCurrentMatchStatus();
      }
      setShowOvertimeConfirmation(false);
    } catch (error) {
      console.error('Error recording extra time:', error);
      throw error;
    }
  }, [currentMatch.id, loadCurrentMatchStatus]);

  const recordShootout = useCallback(async () => {
    try {
      await footballMatchEventService.recordPenaltyShootout(currentMatch.id);
      if (loadCurrentMatchStatus) {
        await loadCurrentMatchStatus();
      }
      setShowShootoutConfirmation(false);
    } catch (error) {
      console.error('Error recording penalty shootout:', error);
      throw error;
    }
  }, [currentMatch.id, loadCurrentMatchStatus]);

  const canEndPeriod = useCallback(() => {
    const isShootout = isPenaltyShootoutPeriod(currentPeriod, rules);
    const isLastAllowedPeriod = currentPeriod === maxPeriodNumber;

    return (
      currentMatch.status === 'InProgress'
      && !periodLoading[currentPeriod]
      && startedPeriods.has(currentPeriod)
      && !endedPeriods.has(currentPeriod)
      && (nextPeriodToStart > 0 || isShootout || isLastAllowedPeriod)
    );
  }, [
    currentMatch.status,
    periodLoading,
    currentPeriod,
    startedPeriods,
    endedPeriods,
    nextPeriodToStart,
    rules,
    maxPeriodNumber,
  ]);

  const getPeriodStatus = useCallback(() => {
    if (periodLoading[currentPeriod]) {
      return 'Processing...';
    }
    if (currentMatch.status === 'Completed') {
      return 'Completed';
    }

    const label = getPeriodLabel(currentPeriod, rules);
    if (currentMatch.status === 'InProgress') {
      if (endedPeriods.has(currentPeriod)) {
        return `${label} ended`;
      }
      if (startedPeriods.has(currentPeriod)) {
        return `${label} started`;
      }
      return `${label} not started`;
    }
    return 'Not started';
  }, [periodLoading, currentPeriod, currentMatch.status, endedPeriods, startedPeriods, rules]);

  const getPeriodControlButtonText = useCallback(() => {
    if (canEndPeriod()) {
      if (periodLoading[currentPeriod]) return 'Ending...';
      return `End ${getPeriodLabel(currentPeriod, rules).toLowerCase()}`;
    }
    if (periodLoading[nextPeriodToStart]) return 'Starting...';
    if (nextPeriodToStart <= 0) return 'Start half';
    return `Start ${getPeriodLabel(nextPeriodToStart, rules).toLowerCase()}`;
  }, [canEndPeriod, periodLoading, currentPeriod, nextPeriodToStart, rules]);

  const isInOvertime = useCallback(
    () => isExtraTimePeriod(currentPeriod, rules),
    [currentPeriod, rules],
  );
  const isInShootout = useCallback(
    () => isPenaltyShootoutPeriod(currentPeriod, rules),
    [currentPeriod, rules],
  );

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
    startedPeriods,
    setStartedPeriods,
    endedPeriods,
    setEndedPeriods,
    nextPeriodToStart,
    setNextPeriodToStart,
    periodLoading,
    overtimePeriodNumber: extraTimePeriodNumber,
    shootoutPeriodNumber,
    maxPeriodNumber,
    matchRules: rules,
    showEndPeriodConfirmation,
    setShowEndPeriodConfirmation,
    pendingEndPeriodAction,
    setPendingEndPeriodAction,
    showOvertimeConfirmation,
    setShowOvertimeConfirmation,
    showShootoutConfirmation,
    setShowShootoutConfirmation,
    handlePeriodStarted,
    endPeriod,
    startPeriod,
    recordOvertime,
    recordShootout,
    confirmEndPeriod,
    cancelEndPeriod,
    canEndPeriod,
    getPeriodStatus,
    getPeriodControlButtonText,
    isInOvertime,
    isInShootout,
  };
};
