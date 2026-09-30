import { useState, useCallback, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import { 
  floorballMatchEventService, 
  type RecordGoalEventRequest, 
  type RecordPenaltyEventRequest 
} from '../../../../../api/floorball/floorballMatchEventService';
import { FloorballGoalType, type FloorballMatchDto } from '../../../../../types/floorball/floorballTypes';
import {
  floorballPeriodEventFlags,
  isFloorballOvertimePeriod,
  isFloorballShootoutPeriod,
} from '../../../../../utils/floorballPeriod';
import type { GoalForm, PenaltyForm } from '../components/types';

interface UseFormStateProps {
  currentMatch: FloorballMatchDto;
  /** Period the desk is operating on; stamped onto recorded events. */
  currentPeriod: number;
  /** Live elapsed seconds of the match clock; used to prefill the time fields. */
  getCurrentElapsedSeconds: () => number;
  loadMatchEvents: () => Promise<void>;
  loadCurrentMatchStatus: () => Promise<void>;
  setError: (error: string | null) => void;
}

export const useFormState = ({
  currentMatch,
  currentPeriod,
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

  /**
   * Opens the goal form for a specific team
   * @param teamId The ID of the team to open the form for
   */
  const openGoalFormForTeam = useCallback((teamId: string) => {
    const elapsedSeconds: number = getCurrentElapsedSeconds();
    const timeMinutes: number = Math.floor(elapsedSeconds / 60);
    const timeSeconds: number = elapsedSeconds % 60;
    // Pre-select the goal type that matches the period: overtime goals are JA and
    // shootout goals VL. The scorer can still override it in the form.
    const regularPeriods: number = currentMatch.matchRules?.numberOfPeriods ?? 2;
    const goalType: FloorballGoalType | null = isFloorballShootoutPeriod(currentPeriod, regularPeriods)
      ? FloorballGoalType.Shootout
      : isFloorballOvertimePeriod(currentPeriod, regularPeriods)
        ? FloorballGoalType.Overtime
        : null;
    setGoalForm(prev => ({ ...prev, teamId, timeMinutes, timeSeconds, goalType }));
    setShowGoalForm(true);
  }, [getCurrentElapsedSeconds, currentPeriod, currentMatch.matchRules?.numberOfPeriods]);

  /**
   * Opens the penalty form for a specific team
   * @param teamId The ID of the team to open the form for
   */
  const openPenaltyFormForTeam = useCallback((teamId: string) => {
    const elapsedSeconds: number = getCurrentElapsedSeconds();
    const timeMinutes: number = Math.floor(elapsedSeconds / 60);
    const timeSeconds: number = elapsedSeconds % 60;
    setPenaltyForm(prev => ({ ...prev, teamId, timeMinutes, timeSeconds }));
    setShowPenaltyForm(true);
  }, [getCurrentElapsedSeconds]);

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
      
      const periodFlags = floorballPeriodEventFlags(currentPeriod, currentMatch.matchRules?.numberOfPeriods ?? 2);
      const goalData: RecordGoalEventRequest = {
        matchId: currentMatch.id,
        teamId: goalForm.teamId,
        playerId: goalForm.playerId,
        assisterId: goalForm.assisterId || undefined,
        periodNumber: currentPeriod,
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
      setGoalForm({ teamId: '', playerId: '', assisterId: '', timeMinutes: 0, timeSeconds: 0, goalType: null });
      setShowGoalForm(false);
      setError(null);
      
    } catch (error) {
      console.error('Error recording goal:', error);
      setError(error instanceof Error ? error.message : t('floorball.matches.manage.errors.recordGoal', 'Failed to record goal'));
    } finally {
      setLoading(false);
    }
  }, [goalForm, currentMatch, currentPeriod, loadMatchEvents, loadCurrentMatchStatus, setError, t]);

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
        periodNumber: currentPeriod,
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
  }, [penaltyForm, currentMatch, currentPeriod, loadMatchEvents, loadCurrentMatchStatus, setError, t]);

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
    
    // Loading state
    loading,
    
    // Actions
    recordGoal,
    recordPenalty
  };
}; 