import { useState, useCallback, useMemo, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import {
  footballMatchEventService,
  type RecordGoalEventRequest,
  type RecordCardEventRequest,
} from '../../../../../api/football/footballMatchEventService';
import { FootballGoalType, type FootballMatchDto } from '../../../../../types/football/footballTypes';
import type { GoalForm, CardForm, LocalClock } from '../components/types';
import {
  extraTimeStartPeriod,
  footballPeriodAtTime,
  penaltyShootoutPeriod,
  resolveMatchRules,
} from '../utils/lineupValidation';

interface UseFormStateProps {
  currentMatch: FootballMatchDto;
  /** Live clock; its period is the default period of a newly opened form. */
  clock: LocalClock;
  /** Periods that have been played; extra time and the shootout appear here only once started. */
  startedPeriods: ReadonlySet<number>;
  currentTimerElapsedTime: number;
  getCurrentElapsedSeconds: (() => number) | null;
  loadMatchEvents: () => Promise<void>;
  loadCurrentMatchStatus: () => Promise<void>;
  setError: (error: string | null) => void;
}

const EMPTY_GOAL_FORM: GoalForm = {
  teamId: '',
  playerId: '',
  assisterId: '',
  timeMinutes: 0,
  timeSeconds: 0,
  periodNumber: 1,
  goalType: FootballGoalType.Regular,
};

const EMPTY_CARD_FORM: CardForm = {
  teamId: '',
  playerId: '',
  cardType: null,
  description: '',
  timeMinutes: 0,
  timeSeconds: 0,
  periodNumber: 1,
};

/** Extra-time and shootout goal types belong to their period, so they reset when the period changes. */
function withGoalPeriod(form: GoalForm, periodNumber: number): GoalForm {
  const isPeriodType: boolean = form.goalType === FootballGoalType.ExtraTime
    || form.goalType === FootballGoalType.PenaltyShootout;
  return {
    ...form,
    periodNumber,
    goalType: isPeriodType && form.periodNumber !== periodNumber ? FootballGoalType.Regular : form.goalType,
  };
}

export const useFormState = ({
  currentMatch,
  clock,
  startedPeriods,
  currentTimerElapsedTime,
  getCurrentElapsedSeconds,
  loadMatchEvents,
  loadCurrentMatchStatus,
  setError,
}: UseFormStateProps) => {
  const { t } = useTranslation();
  const [showGoalForm, setShowGoalForm] = useState(false);
  const [showCardForm, setShowCardForm] = useState(false);

  const [goalForm, setGoalForm] = useState<GoalForm>(EMPTY_GOAL_FORM);
  const [cardForm, setCardForm] = useState<CardForm>(EMPTY_CARD_FORM);

  const [loading, setLoading] = useState(false);
  const goalThrottleRef = useRef<Record<string, number>>({});
  const cardThrottleRef = useRef<Record<string, number>>({});
  const throttleMs = 1000;

  const elapsedClock = useCallback(() => {
    const elapsedSeconds = getCurrentElapsedSeconds
      ? getCurrentElapsedSeconds()
      : currentTimerElapsedTime;
    return {
      timeMinutes: Math.floor(elapsedSeconds / 60),
      timeSeconds: elapsedSeconds % 60,
    };
  }, [getCurrentElapsedSeconds, currentTimerElapsedTime]);

  const rules = useMemo(() => resolveMatchRules(currentMatch.matchRules), [currentMatch.matchRules]);
  const firstExtraTimePeriod: number = extraTimeStartPeriod(rules);
  const shootoutPeriod: number = penaltyShootoutPeriod(rules);

  /** Halves are always selectable; extra time and the shootout once they were played. */
  const recordablePeriods: number[] = useMemo(() => {
    const periods: number[] = Array.from({ length: rules.numberOfHalves }, (_, index) => index + 1);
    for (const period of startedPeriods) {
      if (period > rules.numberOfHalves && period <= shootoutPeriod) periods.push(period);
    }
    if (!periods.includes(clock.period) && clock.period >= 1 && clock.period <= shootoutPeriod) {
      periods.push(clock.period);
    }
    return [...new Set(periods)].sort((a, b) => a - b);
  }, [rules.numberOfHalves, startedPeriods, shootoutPeriod, clock.period]);

  /**
   * Period for an edited time (45:00 is the second half). The shootout has no clock, so an event
   * that is in the shootout stays there whatever the time is.
   */
  const periodAfterTimeEdit = useCallback(
    (formPeriod: number, timeMinutes: number, timeSeconds: number): number =>
      formPeriod === shootoutPeriod && recordablePeriods.includes(shootoutPeriod)
        ? formPeriod
        : footballPeriodAtTime(timeMinutes * 60 + timeSeconds, rules, recordablePeriods.includes(firstExtraTimePeriod)),
    [rules, recordablePeriods, firstExtraTimePeriod, shootoutPeriod],
  );

  const changeGoalTime = useCallback((timeMinutes: number, timeSeconds: number) => {
    setGoalForm((prev) => withGoalPeriod(
      { ...prev, timeMinutes, timeSeconds },
      periodAfterTimeEdit(prev.periodNumber, timeMinutes, timeSeconds),
    ));
  }, [periodAfterTimeEdit]);
  const changeCardTime = useCallback((timeMinutes: number, timeSeconds: number) => {
    setCardForm((prev) => ({
      ...prev,
      timeMinutes,
      timeSeconds,
      periodNumber: periodAfterTimeEdit(prev.periodNumber, timeMinutes, timeSeconds),
    }));
  }, [periodAfterTimeEdit]);

  const changeGoalPeriod = useCallback((periodNumber: number) => {
    setGoalForm((prev) => withGoalPeriod(prev, periodNumber));
  }, []);
  const changeCardPeriod = useCallback((periodNumber: number) => {
    setCardForm((prev) => ({ ...prev, periodNumber }));
  }, []);

  const openGoalFormForTeam = useCallback((teamId: string) => {
    const clockTime = elapsedClock();
    setGoalForm((prev) => ({ ...prev, teamId, ...clockTime, periodNumber: clock.period }));
    setShowGoalForm(true);
  }, [elapsedClock, clock.period]);

  const openCardFormForTeam = useCallback((teamId: string) => {
    const clockTime = elapsedClock();
    setCardForm((prev) => ({ ...prev, teamId, ...clockTime, periodNumber: clock.period }));
    setShowCardForm(true);
  }, [elapsedClock, clock.period]);

  const recordGoal = useCallback(async () => {
    if (!goalForm.teamId || !goalForm.playerId) {
      setError(t('football.matches.manage.errors.selectTeamAndPlayer'));
      return;
    }

    const key = `${currentMatch.id}:${goalForm.teamId}:${goalForm.playerId}`;
    const now = Date.now();
    if (goalThrottleRef.current[key] && now - goalThrottleRef.current[key] < throttleMs) {
      setError(t('football.matches.manage.errors.throttle'));
      return;
    }

    try {
      setLoading(true);
      goalThrottleRef.current[key] = now;

      const timeInSeconds = goalForm.timeMinutes * 60 + goalForm.timeSeconds;
      const isOwnGoal = goalForm.goalType === FootballGoalType.OwnGoal;

      const goalData: RecordGoalEventRequest = {
        matchId: currentMatch.id,
        teamId: goalForm.teamId,
        playerId: goalForm.playerId,
        assisterId: isOwnGoal ? undefined : (goalForm.assisterId || undefined),
        periodNumber: goalForm.periodNumber,
        timeInSeconds,
        goalType: goalForm.goalType ?? FootballGoalType.Regular,
      };

      await footballMatchEventService.recordGoal(goalData);
      await loadMatchEvents();
      await loadCurrentMatchStatus();

      setGoalForm(EMPTY_GOAL_FORM);
      setShowGoalForm(false);
      setError(null);
    } catch (error) {
      setError(error instanceof Error ? error.message : t('football.matches.manage.errors.recordGoal'));
    } finally {
      setLoading(false);
    }
  }, [goalForm, currentMatch, loadMatchEvents, loadCurrentMatchStatus, setError, t]);

  const recordCard = useCallback(async () => {
    if (!cardForm.teamId || !cardForm.playerId || cardForm.cardType === null) {
      setError(t('football.matches.manage.errors.selectCard'));
      return;
    }

    const key = `${currentMatch.id}:${cardForm.teamId}:${cardForm.playerId}`;
    const now = Date.now();
    if (cardThrottleRef.current[key] && now - cardThrottleRef.current[key] < throttleMs) {
      setError(t('football.matches.manage.errors.throttle'));
      return;
    }

    try {
      setLoading(true);
      cardThrottleRef.current[key] = now;

      const timeInSeconds = cardForm.timeMinutes * 60 + cardForm.timeSeconds;
      const cardData: RecordCardEventRequest = {
        matchId: currentMatch.id,
        teamId: cardForm.teamId,
        playerId: cardForm.playerId,
        cardType: cardForm.cardType,
        periodNumber: cardForm.periodNumber,
        timeInSeconds,
        description: cardForm.description,
      };

      await footballMatchEventService.recordCard(cardData);
      await loadMatchEvents();
      await loadCurrentMatchStatus();

      setCardForm(EMPTY_CARD_FORM);
      setShowCardForm(false);
      setError(null);
    } catch (error) {
      setError(error instanceof Error ? error.message : t('football.matches.manage.errors.recordCard'));
    } finally {
      setLoading(false);
    }
  }, [cardForm, currentMatch, loadMatchEvents, loadCurrentMatchStatus, setError, t]);

  return {
    showGoalForm,
    setShowGoalForm,
    showCardForm,
    setShowCardForm,
    openGoalFormForTeam,
    openCardFormForTeam,
    goalForm,
    setGoalForm,
    cardForm,
    setCardForm,
    recordablePeriods,
    shootoutPeriod,
    changeGoalTime,
    changeCardTime,
    changeGoalPeriod,
    changeCardPeriod,
    loading,
    recordGoal,
    recordCard,
  };
};
