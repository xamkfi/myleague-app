import { useState, useCallback, useEffect, useMemo } from 'react';
import { floorballMatchEventService } from '../../../../../api/floorball/floorballMatchEventService';
import type { FloorballMatchDto, FloorballMatchRules } from '../../../../../types/floorball/floorballTypes';
import {
  areNumberSetsEqual,
  deriveFloorballPeriodState,
  floorballPeriodNumbers,
  nextFloorballPeriodAfter,
  resolveFloorballRules,
  type FloorballPeriodState,
} from '../../../../../utils/floorballPeriod';
import type { PeriodEventData } from '../components/types';

interface UsePeriodManagementProps {
  currentMatch: FloorballMatchDto;
  currentPeriod: number;
  setCurrentPeriod: (period: number) => void;
  /** Period reported by the backend timer; drives the derived period state. */
  timerPeriodNumber: number | null;
  loadCurrentMatchStatus: () => Promise<void>;
}

export type PeriodKind = 'regular' | 'overtime' | 'shootout';

/**
 * Describes what the single period control button does right now. The component turns
 * this into a translated label.
 */
export interface PeriodControlDescriptor {
  /**
   * `end` closes the current period, `start` opens the next one and `finish` completes the
   * match because no further period can follow (decided in regulation/overtime, or the
   * shootout is over).
   */
  action: 'end' | 'start' | 'finish';
  period: number;
  kind: PeriodKind;
  loading: boolean;
}

export const usePeriodManagement = ({
  currentMatch,
  currentPeriod,
  setCurrentPeriod,
  timerPeriodNumber,
  loadCurrentMatchStatus,
}: UsePeriodManagementProps) => {
  const rules: FloorballMatchRules = useMemo(() => resolveFloorballRules(currentMatch.matchRules), [currentMatch.matchRules]);
  const { overtimePeriod: overtimePeriodNumber, shootoutPeriod: shootoutPeriodNumber, maxPeriod: maxPeriodNumber } =
    useMemo(() => floorballPeriodNumbers(rules), [rules]);

  const [startedPeriods, setStartedPeriods] = useState<Set<number>>(() => new Set());
  const [endedPeriods, setEndedPeriods] = useState<Set<number>>(() => new Set());
  const [nextPeriodToStart, setNextPeriodToStart] = useState<number>(1);
  const [periodLoading, setPeriodLoading] = useState<Record<number, boolean>>({});
  const [showEndPeriodConfirmation, setShowEndPeriodConfirmation] = useState<boolean>(false);

  // Re-derive the bookkeeping whenever the persisted match or the backend timer period
  // changes. Local optimistic updates made by the actions below converge to the same
  // values, so this is safe to run after every refresh.
  const { status, periodScores, wentToOvertime, wentToShootout, homeScore, awayScore } = currentMatch;
  const scoreTied: boolean = (homeScore ?? 0) === (awayScore ?? 0);
  useEffect(() => {
    const derived: FloorballPeriodState = deriveFloorballPeriodState(
      { status, periodScores, wentToOvertime, wentToShootout, matchRules: rules, homeScore, awayScore },
      timerPeriodNumber,
    );
    setStartedPeriods(prev => (areNumberSetsEqual(prev, derived.started) ? prev : derived.started));
    setEndedPeriods(prev => (areNumberSetsEqual(prev, derived.ended) ? prev : derived.ended));
    setNextPeriodToStart(derived.next);
    setCurrentPeriod(derived.current);
  }, [status, periodScores, wentToOvertime, wentToShootout, homeScore, awayScore, rules, timerPeriodNumber, setCurrentPeriod]);

  const setLoadingFor = useCallback((period: number, loading: boolean): void => {
    setPeriodLoading(prev => ({ ...prev, [period]: loading }));
  }, []);

  const periodKind = useCallback((period: number): PeriodKind => {
    if (period === overtimePeriodNumber) return 'overtime';
    if (period === shootoutPeriodNumber) return 'shootout';
    return 'regular';
  }, [overtimePeriodNumber, shootoutPeriodNumber]);

  /**
   * Handles real-time period started events from SignalR (another operator's browser).
   */
  const handlePeriodStarted = useCallback((eventData: PeriodEventData): void => {
    if (eventData.matchId !== currentMatch.id) return;
    setStartedPeriods(prev => (prev.has(eventData.periodNumber) ? prev : new Set([...prev, eventData.periodNumber])));
  }, [currentMatch.id]);

  /**
   * Ends the current period and moves the desk to the next one.
   */
  const endPeriod = useCallback(async (): Promise<void> => {
    const period: number = currentPeriod;
    setLoadingFor(period, true);
    try {
      await floorballMatchEventService.endPeriod(currentMatch.id, period);
      setEndedPeriods(prev => new Set([...prev, period]));
      const next: number = nextFloorballPeriodAfter(period, rules, scoreTied);
      setNextPeriodToStart(next);
      if (next > 0) setCurrentPeriod(next);
      await loadCurrentMatchStatus();
    } finally {
      setLoadingFor(period, false);
    }
  }, [currentPeriod, currentMatch.id, rules, scoreTied, setCurrentPeriod, loadCurrentMatchStatus, setLoadingFor]);

  /**
   * Starts the next period. Overtime and shootout are recorded on the match first.
   */
  const startPeriod = useCallback(async (): Promise<void> => {
    const period: number = nextPeriodToStart;
    if (period <= 0) return;
    setLoadingFor(period, true);
    try {
      if (period === overtimePeriodNumber) {
        await floorballMatchEventService.recordOvertime(currentMatch.id);
      } else if (period === shootoutPeriodNumber) {
        await floorballMatchEventService.recordShootout(currentMatch.id);
      }
      await floorballMatchEventService.startPeriod(currentMatch.id, period);

      setStartedPeriods(prev => new Set([...prev, period]));
      setCurrentPeriod(period);
      setNextPeriodToStart(nextFloorballPeriodAfter(period, rules, scoreTied));

      if (period === overtimePeriodNumber || period === shootoutPeriodNumber) {
        await loadCurrentMatchStatus();
      }
    } finally {
      setLoadingFor(period, false);
    }
  }, [nextPeriodToStart, currentMatch.id, rules, scoreTied, overtimePeriodNumber, shootoutPeriodNumber, setCurrentPeriod, loadCurrentMatchStatus, setLoadingFor]);

  /**
   * Goes straight to the shootout from the last regular period, ending that period first
   * when it is still open.
   */
  const skipToShootout = useCallback(async (): Promise<void> => {
    setLoadingFor(shootoutPeriodNumber, true);
    try {
      const lastRegularPeriod: number = rules.numberOfPeriods;
      const lastRegularStillOpen: boolean = currentPeriod === lastRegularPeriod
        && startedPeriods.has(lastRegularPeriod)
        && !endedPeriods.has(lastRegularPeriod);
      if (lastRegularStillOpen) {
        await floorballMatchEventService.endPeriod(currentMatch.id, lastRegularPeriod);
        setEndedPeriods(prev => new Set([...prev, lastRegularPeriod]));
      }

      await floorballMatchEventService.recordShootout(currentMatch.id);
      await floorballMatchEventService.startPeriod(currentMatch.id, shootoutPeriodNumber);

      setStartedPeriods(prev => new Set([...prev, shootoutPeriodNumber]));
      setCurrentPeriod(shootoutPeriodNumber);
      setNextPeriodToStart(0);
      await loadCurrentMatchStatus();
    } finally {
      setLoadingFor(shootoutPeriodNumber, false);
    }
  }, [currentMatch.id, currentPeriod, startedPeriods, endedPeriods, rules.numberOfPeriods, shootoutPeriodNumber, setCurrentPeriod, loadCurrentMatchStatus, setLoadingFor]);

  const isInShootout: boolean = currentPeriod === shootoutPeriodNumber;
  const isMatchInProgress: boolean = currentMatch.status === 'InProgress';
  const isCurrentPeriodOpen: boolean = startedPeriods.has(currentPeriod) && !endedPeriods.has(currentPeriod);

  const canEndPeriod: boolean = isMatchInProgress && !periodLoading[currentPeriod] && isCurrentPeriodOpen;

  /**
   * True when ending the current period also ends the match: nothing can follow it
   * (shootout, last allowed period, or the score is not level so no extra period is due).
   */
  const endingPeriodFinishesMatch: boolean =
    isInShootout
    || currentPeriod === maxPeriodNumber
    || nextFloorballPeriodAfter(currentPeriod, rules, scoreTied) === 0;

  const periodControl: PeriodControlDescriptor | null = useMemo(() => {
    const loading: boolean = Boolean(periodLoading[currentPeriod]);
    if (canEndPeriod) {
      return {
        action: endingPeriodFinishesMatch ? 'finish' : 'end',
        period: currentPeriod,
        kind: periodKind(currentPeriod),
        loading,
      };
    }
    if (nextPeriodToStart > 0) {
      return { action: 'start', period: nextPeriodToStart, kind: periodKind(nextPeriodToStart), loading: Boolean(periodLoading[nextPeriodToStart]) };
    }
    // Every playable period is over but the match is still open (e.g. the last period was
    // ended while the score was not level): offer to complete the match.
    if (isMatchInProgress && endedPeriods.has(currentPeriod)) {
      return { action: 'finish', period: currentPeriod, kind: periodKind(currentPeriod), loading };
    }
    return null;
  }, [canEndPeriod, endingPeriodFinishesMatch, isMatchInProgress, endedPeriods, currentPeriod, nextPeriodToStart, periodKind, periodLoading]);

  return {
    startedPeriods,
    endedPeriods,
    nextPeriodToStart,
    periodLoading,

    overtimePeriodNumber,
    shootoutPeriodNumber,
    maxPeriodNumber,
    matchRules: rules,

    showEndPeriodConfirmation,
    setShowEndPeriodConfirmation,

    handlePeriodStarted,
    endPeriod,
    startPeriod,
    skipToShootout,

    canEndPeriod,
    isInShootout,
    periodControl,
  };
};
