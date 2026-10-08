import { useState, useCallback, useMemo, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import { 
  floorballMatchEventService, 
  type RecordGoalEventRequest, 
  type RecordPenaltyEventRequest 
} from '../../../../../api/floorball/floorballMatchEventService';
import { FloorballGoalType, type FloorballMatchDto } from '../../../../../types/floorball/floorballTypes';
import {
  floorballPeriodAtTime,
  floorballPeriodEventFlags,
  floorballPeriodNumbers,
  isFloorballOvertimePeriod,
  isFloorballShootoutPeriod,
  resolveFloorballRules,
} from '../../../../../utils/floorballPeriod';
import type { GoalForm, PenaltyForm } from '../components/types';

interface UseFormStateProps {
  currentMatch: FloorballMatchDto;
  /** Period the desk is operating on; the default period of a newly opened form. */
  currentPeriod: number;
  /** Periods that have been played; overtime and shootout appear here only once started. */
  startedPeriods: ReadonlySet<number>;
  /** Live elapsed seconds of the match clock; used to prefill the time fields. */
  getCurrentElapsedSeconds: () => number;
  loadMatchEvents: () => Promise<void>;
  loadCurrentMatchStatus: () => Promise<void>;
  setError: (error: string | null) => void;
}

export const useFormState = ({
  currentMatch,
  currentPeriod,
  startedPeriods,
  getCurrentElapsedSeconds,
  loadMatchEvents,
  loadCurrentMatchStatus,
  setError
}: UseFormStateProps) => {
  const { t } = useTranslation();
  // Form visibility states
  const [showGoalForm, setShowGoalForm] = useState(false);
  const [showPenaltyForm, setShowPenaltyForm] = useState(false);
  
  // Form data states
  const [goalForm, setGoalForm] = useState<GoalForm>({
    teamId: '',
    playerId: '',
    assisterId: '',
    timeMinutes: 0,
    timeSeconds: 0,
    periodNumber: 1,
    goalType: null,
  });
  
  const [penaltyForm, setPenaltyForm] = useState<PenaltyForm>({
    teamId: '',
    playerId: '',
    penaltyType: '',
    minutes: 2,
    description: '',
    periodNumber: 1,
    timeMinutes: 0,
    timeSeconds: 0,
  });
  
  // Loading state
  const [loading, setLoading] = useState(false);
  const goalThrottleRef = useRef<Record<string, number>>({});
  const penaltyThrottleRef = useRef<Record<string, number>>({});
  const throttleMs = 1000;

  const rules = useMemo(() => resolveFloorballRules(currentMatch.matchRules), [currentMatch.matchRules]);
  const { regularPeriods, overtimePeriod, shootoutPeriod } = floorballPeriodNumbers(rules);

  /** Regular periods are always selectable; overtime and shootout once they were played. */
  const recordablePeriods: number[] = useMemo(() => {
    const periods: number[] = Array.from({ length: regularPeriods }, (_, index) => index + 1);
    if (startedPeriods.has(overtimePeriod)) periods.push(overtimePeriod);
    if (startedPeriods.has(shootoutPeriod)) periods.push(shootoutPeriod);
    if (!periods.includes(currentPeriod) && currentPeriod >= 1 && currentPeriod <= shootoutPeriod) {
      periods.push(currentPeriod);
    }
    return periods.sort((a, b) => a - b);
  }, [regularPeriods, overtimePeriod, shootoutPeriod, startedPeriods, currentPeriod]);

  const periodForTime = useCallback(
    (timeInSeconds: number): number =>
      floorballPeriodAtTime(timeInSeconds, rules, recordablePeriods.includes(overtimePeriod)),
    [rules, recordablePeriods, overtimePeriod],
  );

  // Overtime goals are JA and shootout goals VL. The scorer can still override it in the form.
  const goalTypeForPeriod = useCallback((period: number): FloorballGoalType | null => {
    if (isFloorballShootoutPeriod(period, regularPeriods)) return FloorballGoalType.Shootout;
    if (isFloorballOvertimePeriod(period, regularPeriods)) return FloorballGoalType.Overtime;
    return null;
  }, [regularPeriods]);

  /** Keeps a chosen goal type, but follows the period when the type was period-derived. */
  const withPeriod = useCallback((form: GoalForm, periodNumber: number): GoalForm => {
    const periodDerivedType: boolean = form.goalType === null
      || form.goalType === FloorballGoalType.Overtime
      || form.goalType === FloorballGoalType.Shootout;
    return {
      ...form,
      periodNumber,
      goalType: periodDerivedType ? goalTypeForPeriod(periodNumber) : form.goalType,
    };
  }, [goalTypeForPeriod]);

  /** Editing the time moves the event to the period that time falls in (15:00 is period 2). */
  const changeGoalTime = useCallback((timeMinutes: number, timeSeconds: number) => {
    setGoalForm(prev => withPeriod({ ...prev, timeMinutes, timeSeconds }, periodForTime(timeMinutes * 60 + timeSeconds)));
  }, [withPeriod, periodForTime]);

  const changeGoalPeriod = useCallback((periodNumber: number) => {
    setGoalForm(prev => withPeriod(prev, periodNumber));
  }, [withPeriod]);

  const changePenaltyTime = useCallback((timeMinutes: number, timeSeconds: number) => {
    setPenaltyForm(prev => ({
      ...prev,
      timeMinutes,
      timeSeconds,
      periodNumber: periodForTime(timeMinutes * 60 + timeSeconds),
    }));
  }, [periodForTime]);

  const changePenaltyPeriod = useCallback((periodNumber: number) => {
    setPenaltyForm(prev => ({ ...prev, periodNumber }));
  }, []);

  /**
   * Opens the goal form for a specific team
   * @param teamId The ID of the team to open the form for
   */
  const openGoalFormForTeam = useCallback((teamId: string) => {
    const elapsedSeconds: number = getCurrentElapsedSeconds();
    const timeMinutes: number = Math.floor(elapsedSeconds / 60);
    const timeSeconds: number = elapsedSeconds % 60;
    setGoalForm(prev => ({
      ...prev,
      teamId,
      timeMinutes,
      timeSeconds,
      periodNumber: currentPeriod,
      goalType: goalTypeForPeriod(currentPeriod),
    }));
    setShowGoalForm(true);
  }, [getCurrentElapsedSeconds, currentPeriod, goalTypeForPeriod]);

  /**
   * Opens the penalty form for a specific team
   * @param teamId The ID of the team to open the form for
   */
  const openPenaltyFormForTeam = useCallback((teamId: string) => {
    const elapsedSeconds: number = getCurrentElapsedSeconds();
    const timeMinutes: number = Math.floor(elapsedSeconds / 60);
    const timeSeconds: number = elapsedSeconds % 60;
    setPenaltyForm(prev => ({ ...prev, teamId, timeMinutes, timeSeconds, periodNumber: currentPeriod }));
    setShowPenaltyForm(true);
  }, [getCurrentElapsedSeconds, currentPeriod]);

  /**
   * Records a goal event
   */
  const recordGoal = useCallback(async () => {
    if (!goalForm.teamId || !goalForm.playerId) {
      setError(t('floorball.matches.manage.errors.selectTeamAndPlayer', 'Select a team and a player'));
      return;
    }

    const key = `${currentMatch.id}:${goalForm.teamId}:${goalForm.playerId}`;
    const now = Date.now();
    if (goalThrottleRef.current[key] && now - goalThrottleRef.current[key] < throttleMs) {
      setError(t('floorball.matches.manage.errors.goalThrottle', 'Wait a moment before recording another goal.'));
      return;
    }
    
    try {
      setLoading(true);
      goalThrottleRef.current[key] = now;
      
      // Calculate time in seconds from the form time values (not the running clock)
      const timeInSeconds = goalForm.timeMinutes * 60 + goalForm.timeSeconds;
      
      const periodFlags = floorballPeriodEventFlags(goalForm.periodNumber, regularPeriods);
      const goalData: RecordGoalEventRequest = {
        matchId: currentMatch.id,
        teamId: goalForm.teamId,
        playerId: goalForm.playerId,
        assisterId: goalForm.assisterId || undefined,
        periodNumber: goalForm.periodNumber,
        timeInSeconds: timeInSeconds,
        wasInOvertime: periodFlags.wasInOvertime,
        wasInShootout: periodFlags.wasInShootout,
        goalType: goalForm.goalType ?? undefined,
      };
      
      await floorballMatchEventService.recordGoal(goalData);
      
      // Refresh events from backend
      await loadMatchEvents();
      // Refresh match status to sync scores
      await loadCurrentMatchStatus();
      
      // Reset form
      setGoalForm({ teamId: '', playerId: '', assisterId: '', timeMinutes: 0, timeSeconds: 0, periodNumber: 1, goalType: null });
      setShowGoalForm(false);
      setError(null);
      
    } catch (error) {
      console.error('Error recording goal:', error);
      setError(error instanceof Error ? error.message : t('floorball.matches.manage.errors.recordGoal', 'Failed to record goal'));
    } finally {
      setLoading(false);
    }
  }, [goalForm, currentMatch, regularPeriods, loadMatchEvents, loadCurrentMatchStatus, setError, t]);

  /**
   * Records a penalty event
   */
  const recordPenalty = useCallback(async () => {
    if (!penaltyForm.teamId || !penaltyForm.penaltyType) {
      setError(t('floorball.matches.manage.errors.selectTeamAndPenaltyType', 'Select a team and a penalty type'));
      return;
    }

    const key = `${currentMatch.id}:${penaltyForm.teamId}:${penaltyForm.playerId || 'team'}`;
    const now = Date.now();
    if (penaltyThrottleRef.current[key] && now - penaltyThrottleRef.current[key] < throttleMs) {
      setError(t('floorball.matches.manage.errors.penaltyThrottle', 'Wait a moment before recording another penalty.'));
      return;
    }
    
    try {
      setLoading(true);
      penaltyThrottleRef.current[key] = now;
      
      // Calculate time in seconds from the form time values (not the running clock)
      const timeInSeconds = penaltyForm.timeMinutes * 60 + penaltyForm.timeSeconds;
      
      const penaltyData: RecordPenaltyEventRequest = {
        matchId: currentMatch.id,
        teamId: penaltyForm.teamId,
        playerId: penaltyForm.playerId || undefined,
        penaltyType: penaltyForm.penaltyType,
        durationMinutes: penaltyForm.minutes,
        periodNumber: penaltyForm.periodNumber,
        timeInSeconds: timeInSeconds,
        description: penaltyForm.description,
      };
      
      await floorballMatchEventService.recordPenalty(penaltyData);
      
      // Refresh events from backend
      await loadMatchEvents();
      // Refresh match status to sync scores
      await loadCurrentMatchStatus();
      
      // Reset form
      setPenaltyForm({ 
        teamId: '', 
        playerId: '', 
        penaltyType: '', 
        minutes: 2, 
        description: '', 
        periodNumber: 1, 
        timeMinutes: 0, 
        timeSeconds: 0 
      });
      setShowPenaltyForm(false);
      setError(null);
      
    } catch (error) {
      console.error('Error recording penalty:', error);
      setError(error instanceof Error ? error.message : t('floorball.matches.manage.errors.recordPenalty', 'Failed to record penalty'));
    } finally {
      setLoading(false);
    }
  }, [penaltyForm, currentMatch, loadMatchEvents, loadCurrentMatchStatus, setError, t]);

  return {
    // Form visibility
    showGoalForm,
    setShowGoalForm,
    showPenaltyForm,
    setShowPenaltyForm,
    openGoalFormForTeam,
    openPenaltyFormForTeam,
    
    // Form data
    goalForm,
    setGoalForm,
    penaltyForm,
    setPenaltyForm,
    
    // Period picker
    recordablePeriods,
    overtimePeriod,
    shootoutPeriod,
    changeGoalTime,
    changeGoalPeriod,
    changePenaltyTime,
    changePenaltyPeriod,

    // Loading state
    loading,
    
    // Actions
    recordGoal,
    recordPenalty
  };
}; 