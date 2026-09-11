import { useCallback, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { hockeyMatchService } from '../../../../../api/hockey/hockeyMatchService';
import {
  isHockeyMatchLive,
  type HockeyMatchDto,
  type HockeyPeriodAction,
  type HockeyPeriodType,
} from '../../../../../types/hockey/hockeyTypes';

/**
 * Default hockey match rules used until matchRules is exposed on the match DTO.
 * Standard ice hockey: 3 × 20 minutes, overtime and shootout available.
 */
export const DEFAULT_HOCKEY_MATCH_RULES = {
  numberOfPeriods: 3,
  periodDurationMinutes: 20,
  allowOvertime: true,
  overtimeDurationMinutes: 5,
  allowShootout: true,
};

interface UseHockeyPeriodManagementProps {
  currentMatch: HockeyMatchDto | null;
  currentPeriod: number;
  setCurrentPeriod: (period: number) => void;
  getElapsedSeconds: () => number;
  onMatchUpdated: (match: HockeyMatchDto) => void;
}

function periodTypeFor(period: number, overtimePeriodNumber: number, shootoutPeriodNumber: number): HockeyPeriodType {
  if (period === shootoutPeriodNumber) return 'Shootout';
  if (period === overtimePeriodNumber) return 'Overtime';
  return 'RegularPeriod';
}

function startActionFor(period: number, overtimePeriodNumber: number, shootoutPeriodNumber: number): HockeyPeriodAction {
  if (period === shootoutPeriodNumber) return 'ShootoutStarted';
  if (period === overtimePeriodNumber) return 'OvertimeStarted';
  return 'PeriodStarted';
}

export function useHockeyPeriodManagement({
  currentMatch,
  currentPeriod,
  setCurrentPeriod,
  getElapsedSeconds,
  onMatchUpdated,
}: UseHockeyPeriodManagementProps) {
  const { t } = useTranslation();
  const rules = DEFAULT_HOCKEY_MATCH_RULES;
  const overtimePeriodNumber = useMemo(() => rules.numberOfPeriods + 1, [rules.numberOfPeriods]);
  const shootoutPeriodNumber = useMemo(() => rules.numberOfPeriods + 2, [rules.numberOfPeriods]);
  const maxPeriodNumber = useMemo(() => {
    if (rules.allowShootout) return shootoutPeriodNumber;
    if (rules.allowOvertime) return overtimePeriodNumber;
    return rules.numberOfPeriods;
  }, [rules, overtimePeriodNumber, shootoutPeriodNumber]);

  const [startedPeriods, setStartedPeriods] = useState<Set<number>>(new Set());
  const [endedPeriods, setEndedPeriods] = useState<Set<number>>(new Set());
  const [nextPeriodToStart, setNextPeriodToStart] = useState(1);
  const [periodLoading, setPeriodLoading] = useState<Record<number, boolean>>({});
  const [showEndPeriodConfirmation, setShowEndPeriodConfirmation] = useState(false);

  const computeNextAfter = useCallback((period: number): number => {
    if (period < rules.numberOfPeriods) return period + 1;
    if (period === rules.numberOfPeriods && rules.allowOvertime) return overtimePeriodNumber;
    if (period === rules.numberOfPeriods && !rules.allowOvertime && rules.allowShootout) {
      return shootoutPeriodNumber;
    }
    if (period === overtimePeriodNumber && rules.allowShootout) return shootoutPeriodNumber;
    return 0;
  }, [rules, overtimePeriodNumber, shootoutPeriodNumber]);

  const applyMatch = useCallback((match: HockeyMatchDto) => {
    onMatchUpdated(match);
  }, [onMatchUpdated]);

  const ensurePeriodScore = useCallback(async (period: number): Promise<HockeyMatchDto | null> => {
    if (!currentMatch) return null;
    const exists = currentMatch.periodScores.some((score) => score.periodNumber === period);
    if (exists) return null;
    try {
      return await hockeyMatchService.addPeriodScore(
        currentMatch.id,
        period,
        periodTypeFor(period, overtimePeriodNumber, shootoutPeriodNumber),
      );
    } catch {
      return null;
    }
  }, [currentMatch, overtimePeriodNumber, shootoutPeriodNumber]);

  const endPeriod = useCallback(async () => {
    if (!currentMatch) return;
    try {
      setPeriodLoading((prev) => ({ ...prev, [currentPeriod]: true }));
      const timeInSeconds = getElapsedSeconds();
      let updated = await hockeyMatchService.recordPeriodEvent(currentMatch.id, {
        periodNumber: currentPeriod,
        timeInSeconds,
        action: 'PeriodEnded',
        description: 'PeriodEnded',
      });
      const scoreRow = await ensurePeriodScore(currentPeriod);
      if (scoreRow) updated = scoreRow;
      applyMatch(updated);

      setEndedPeriods((prev) => new Set([...prev, currentPeriod]));
      const nextPeriod = computeNextAfter(currentPeriod);
      setNextPeriodToStart(nextPeriod);
      if (nextPeriod > 0) {
        setCurrentPeriod(nextPeriod);
      }
    } finally {
      setPeriodLoading((prev) => ({ ...prev, [currentPeriod]: false }));
    }
  }, [
    currentPeriod,
    currentMatch,
    getElapsedSeconds,
    ensurePeriodScore,
    applyMatch,
    computeNextAfter,
    setCurrentPeriod,
  ]);

  const startPeriod = useCallback(async () => {
    const starting = nextPeriodToStart;
    if (!currentMatch || starting < 1) return;
    try {
      setPeriodLoading((prev) => ({ ...prev, [starting]: true }));
      if (starting === overtimePeriodNumber) {
        await hockeyMatchService.setWentToOvertime(currentMatch.id, true);
      } else if (starting === shootoutPeriodNumber) {
        await hockeyMatchService.setWentToShootout(currentMatch.id, true);
      }

      await hockeyMatchService.setPeriod(currentMatch.id, starting);
      const scoreRow = await ensurePeriodScore(starting);
      const timeInSeconds = getElapsedSeconds();
      const updated = await hockeyMatchService.recordPeriodEvent(currentMatch.id, {
        periodNumber: starting,
        timeInSeconds,
        action: startActionFor(starting, overtimePeriodNumber, shootoutPeriodNumber),
        description: startActionFor(starting, overtimePeriodNumber, shootoutPeriodNumber),
      });
      applyMatch(scoreRow ?? updated);

      setStartedPeriods((prev) => new Set([...prev, starting]));
      setCurrentPeriod(starting);
      setNextPeriodToStart(computeNextAfter(starting));
    } finally {
      setPeriodLoading((prev) => ({ ...prev, [starting]: false }));
    }
  }, [
    nextPeriodToStart,
    overtimePeriodNumber,
    shootoutPeriodNumber,
    currentMatch,
    ensurePeriodScore,
    getElapsedSeconds,
    applyMatch,
    setCurrentPeriod,
    computeNextAfter,
  ]);

  const canEndPeriod = useCallback(() => {
    const isShootout = currentPeriod === shootoutPeriodNumber;
    const isLastAllowedPeriod = currentPeriod === maxPeriodNumber;
    return (
      Boolean(currentMatch && isHockeyMatchLive(currentMatch.status)) &&
      !periodLoading[currentPeriod] &&
      startedPeriods.has(currentPeriod) &&
      !endedPeriods.has(currentPeriod) &&
      (nextPeriodToStart > 0 || isShootout || isLastAllowedPeriod)
    );
  }, [
    currentMatch,
    periodLoading,
    currentPeriod,
    startedPeriods,
    endedPeriods,
    nextPeriodToStart,
    shootoutPeriodNumber,
    maxPeriodNumber,
  ]);

  const getPeriodControlButtonText = useCallback(() => {
    if (canEndPeriod()) {
      if (periodLoading[currentPeriod]) return t('hockey.matches.endingPeriod', 'Ending...');
      if (currentPeriod === overtimePeriodNumber) return t('hockey.matches.endOvertime', 'End Overtime');
      if (currentPeriod === shootoutPeriodNumber) return t('hockey.matches.endShootout', 'End Shootout');
      return t('hockey.matches.endPeriod', 'End period');
    }
    if (periodLoading[nextPeriodToStart]) return t('hockey.matches.startingPeriod', 'Starting...');
    if (nextPeriodToStart === overtimePeriodNumber) return t('hockey.matches.startOvertime', 'Start Overtime');
    if (nextPeriodToStart === shootoutPeriodNumber) return t('hockey.matches.startShootout', 'Start Shootout');
    return t('hockey.matches.startPeriodN', 'Start period {{number}}', { number: nextPeriodToStart });
  }, [
    canEndPeriod,
    periodLoading,
    currentPeriod,
    nextPeriodToStart,
    overtimePeriodNumber,
    shootoutPeriodNumber,
    t,
  ]);

  const isInOvertime = useCallback(
    () => currentPeriod === overtimePeriodNumber,
    [currentPeriod, overtimePeriodNumber],
  );
  const isInShootout = useCallback(
    () => currentPeriod === shootoutPeriodNumber,
    [currentPeriod, shootoutPeriodNumber],
  );

  const restoreFromMatch = useCallback((match: HockeyMatchDto) => {
    const started = new Set<number>();
    const ended = new Set<number>();

    for (const eventItem of match.events) {
      if (eventItem.eventType !== 'Period') continue;
      const description = eventItem.description ?? '';
      if (
        description === 'PeriodStarted'
        || description === 'OvertimeStarted'
        || description === 'ShootoutStarted'
        || description === 'IntermissionStarted'
      ) {
        started.add(eventItem.periodNumber);
      }
      // Only an explicit PeriodEnded event closes a period. Stoppages (offside,
      // icing) and period-score rows must not force the next period.
      if (description === 'PeriodEnded') {
        started.add(eventItem.periodNumber);
        ended.add(eventItem.periodNumber);
      }
    }

    for (const score of match.periodScores) {
      started.add(score.periodNumber);
    }

    const current = match.currentPeriodNumber || 1;
    if (isHockeyMatchLive(match.status)) {
      started.add(current);
    }

    let next = 0;
    for (let period = 1; period <= maxPeriodNumber; period += 1) {
      if (!started.has(period)) {
        next = period;
        break;
      }
    }

    let restoredCurrent = current;
    if (ended.has(current) && next > 0) {
      restoredCurrent = next;
    }

    setStartedPeriods(started);
    setEndedPeriods(ended);
    setNextPeriodToStart(next);
    setCurrentPeriod(restoredCurrent);
  }, [maxPeriodNumber, setCurrentPeriod]);

  return {
    rules,
    overtimePeriodNumber,
    shootoutPeriodNumber,
    maxPeriodNumber,
    startedPeriods,
    setStartedPeriods,
    endedPeriods,
    setEndedPeriods,
    nextPeriodToStart,
    setNextPeriodToStart,
    periodLoading,
    showEndPeriodConfirmation,
    setShowEndPeriodConfirmation,
    endPeriod,
    startPeriod,
    canEndPeriod,
    getPeriodControlButtonText,
    isInOvertime,
    isInShootout,
    restoreFromMatch,
  };
}
