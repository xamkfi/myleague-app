import { useState, useCallback, useRef } from 'react';
import {
  footballMatchEventService,
  type RecordGoalEventRequest,
  type RecordCardEventRequest,
  type RecordSubstitutionEventRequest,
} from '../../../../../api/football/footballMatchEventService';
import { FootballGoalType, type FootballMatchDto } from '../../../../../types/football/footballTypes';
import type { GoalForm, CardForm, SubstitutionForm, LocalClock } from '../components/types';

interface UseFormStateProps {
  currentMatch: FootballMatchDto;
  clock: LocalClock;
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
  goalType: FootballGoalType.Regular,
};

const EMPTY_CARD_FORM: CardForm = {
  teamId: '',
  playerId: '',
  cardType: null,
  description: '',
  timeMinutes: 0,
  timeSeconds: 0,
};

const EMPTY_SUBSTITUTION_FORM: SubstitutionForm = {
  teamId: '',
  playerOffId: '',
  playerOnId: '',
  description: '',
  timeMinutes: 0,
  timeSeconds: 0,
};

export const useFormState = ({
  currentMatch,
  clock,
  currentTimerElapsedTime,
  getCurrentElapsedSeconds,
  loadMatchEvents,
  loadCurrentMatchStatus,
  setError,
}: UseFormStateProps) => {
  const [showGoalForm, setShowGoalForm] = useState(false);
  const [showCardForm, setShowCardForm] = useState(false);
  const [showSubstitutionForm, setShowSubstitutionForm] = useState(false);

  const [goalForm, setGoalForm] = useState<GoalForm>(EMPTY_GOAL_FORM);
  const [cardForm, setCardForm] = useState<CardForm>(EMPTY_CARD_FORM);
  const [substitutionForm, setSubstitutionForm] = useState<SubstitutionForm>(EMPTY_SUBSTITUTION_FORM);

  const [loading, setLoading] = useState(false);
  const goalThrottleRef = useRef<Record<string, number>>({});
  const cardThrottleRef = useRef<Record<string, number>>({});
  const substitutionThrottleRef = useRef<Record<string, number>>({});
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

  const openGoalFormForTeam = useCallback((teamId: string) => {
    const clockTime = elapsedClock();
    setGoalForm((prev) => ({ ...prev, teamId, ...clockTime }));
    setShowGoalForm(true);
  }, [elapsedClock]);

  const openCardFormForTeam = useCallback((teamId: string) => {
    const clockTime = elapsedClock();
    setCardForm((prev) => ({ ...prev, teamId, ...clockTime }));
    setShowCardForm(true);
  }, [elapsedClock]);

  const openSubstitutionFormForTeam = useCallback((teamId: string) => {
    const clockTime = elapsedClock();
    setSubstitutionForm((prev) => ({ ...prev, teamId, ...clockTime }));
    setShowSubstitutionForm(true);
  }, [elapsedClock]);

  const recordGoal = useCallback(async () => {
    if (!goalForm.teamId || !goalForm.playerId) {
      setError('Please select team and player');
      return;
    }

    const key = `${currentMatch.id}:${goalForm.teamId}:${goalForm.playerId}`;
    const now = Date.now();
    if (goalThrottleRef.current[key] && now - goalThrottleRef.current[key] < throttleMs) {
      setError('Please wait a moment before recording another goal.');
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
        periodNumber: clock.period,
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
      setError(error instanceof Error ? error.message : 'Failed to record goal');
    } finally {
      setLoading(false);
    }
  }, [goalForm, currentMatch, clock.period, loadMatchEvents, loadCurrentMatchStatus, setError]);

  const recordCard = useCallback(async () => {
    if (!cardForm.teamId || !cardForm.playerId || cardForm.cardType === null) {
      setError('Please select team, player and card type');
      return;
    }

    const key = `${currentMatch.id}:${cardForm.teamId}:${cardForm.playerId}`;
    const now = Date.now();
    if (cardThrottleRef.current[key] && now - cardThrottleRef.current[key] < throttleMs) {
      setError('Please wait a moment before recording another card.');
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
        periodNumber: clock.period,
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
      setError(error instanceof Error ? error.message : 'Failed to record card');
    } finally {
      setLoading(false);
    }
  }, [cardForm, currentMatch, clock.period, loadMatchEvents, loadCurrentMatchStatus, setError]);

  const recordSubstitution = useCallback(async () => {
    if (!substitutionForm.teamId || !substitutionForm.playerOffId || !substitutionForm.playerOnId) {
      setError('Please select team, player going off and player coming on');
      return;
    }

    const key = `${currentMatch.id}:${substitutionForm.teamId}:${substitutionForm.playerOffId}:${substitutionForm.playerOnId}`;
    const now = Date.now();
    if (substitutionThrottleRef.current[key] && now - substitutionThrottleRef.current[key] < throttleMs) {
      setError('Please wait a moment before recording another substitution.');
      return;
    }

    try {
      setLoading(true);
      substitutionThrottleRef.current[key] = now;

      const timeInSeconds = substitutionForm.timeMinutes * 60 + substitutionForm.timeSeconds;
      const substitutionData: RecordSubstitutionEventRequest = {
        matchId: currentMatch.id,
        teamId: substitutionForm.teamId,
        playerOffId: substitutionForm.playerOffId,
        playerOnId: substitutionForm.playerOnId,
        periodNumber: clock.period,
        timeInSeconds,
        description: substitutionForm.description,
      };

      await footballMatchEventService.recordSubstitution(substitutionData);
      await loadMatchEvents();
      await loadCurrentMatchStatus();

      setSubstitutionForm(EMPTY_SUBSTITUTION_FORM);
      setShowSubstitutionForm(false);
      setError(null);
    } catch (error) {
      setError(error instanceof Error ? error.message : 'Failed to record substitution');
    } finally {
      setLoading(false);
    }
  }, [substitutionForm, currentMatch, clock.period, loadMatchEvents, loadCurrentMatchStatus, setError]);

  return {
    showGoalForm,
    setShowGoalForm,
    showCardForm,
    setShowCardForm,
    showSubstitutionForm,
    setShowSubstitutionForm,
    openGoalFormForTeam,
    openCardFormForTeam,
    openSubstitutionFormForTeam,
    goalForm,
    setGoalForm,
    cardForm,
    setCardForm,
    substitutionForm,
    setSubstitutionForm,
    loading,
    recordGoal,
    recordCard,
    recordSubstitution,
  };
};
