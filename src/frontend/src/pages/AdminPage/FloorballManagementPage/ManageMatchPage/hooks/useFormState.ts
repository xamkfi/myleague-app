import { useState, useCallback, useRef } from 'react';
import { 
  floorballMatchEventService, 
  type RecordGoalEventRequest, 
  type RecordPenaltyEventRequest 
} from '../../../../../api/floorball/floorballMatchEventService';
import type { FloorballMatchDto } from '../../../../../types/floorball/floorballTypes';
import type { GoalForm, PenaltyForm, LocalClock } from '../components/types';

interface UseFormStateProps {
  currentMatch: FloorballMatchDto;
  clock: LocalClock;
  currentTimerElapsedTime: number;
  loadMatchEvents: () => Promise<void>;
  loadCurrentMatchStatus: () => Promise<void>;
  setError: (error: string | null) => void;
}

export const useFormState = ({
  currentMatch,
  clock,
  currentTimerElapsedTime,
  loadMatchEvents,
  loadCurrentMatchStatus,
  setError
}: UseFormStateProps) => {
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
    // Initialize time from current timer
    const timeMinutes = Math.floor(currentTimerElapsedTime / 60);
    const timeSeconds = currentTimerElapsedTime % 60;
    setGoalForm(prev => ({ ...prev, teamId, timeMinutes, timeSeconds }));
    setShowGoalForm(true);
  }, [currentTimerElapsedTime]);

  /**
   * Opens the penalty form for a specific team
   * @param teamId The ID of the team to open the form for
   */
  const openPenaltyFormForTeam = useCallback((teamId: string) => {
    // Initialize time from current timer
    const timeMinutes = Math.floor(currentTimerElapsedTime / 60);
    const timeSeconds = currentTimerElapsedTime % 60;
    setPenaltyForm(prev => ({ ...prev, teamId, timeMinutes, timeSeconds }));
    setShowPenaltyForm(true);
  }, [currentTimerElapsedTime]);

  /**
   * Opens the penalty form
   */
  const openPenaltyForm = useCallback(() => {
    // Initialize time from current timer
    const timeMinutes = Math.floor(currentTimerElapsedTime / 60);
    const timeSeconds = currentTimerElapsedTime % 60;
    setPenaltyForm(prev => ({ ...prev, timeMinutes, timeSeconds }));
    setShowPenaltyForm(true);
  }, [currentTimerElapsedTime]);

  /**
   * Records a goal event
   */
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
      
      // Calculate time in seconds from the form time values (not the running clock)
      const timeInSeconds = goalForm.timeMinutes * 60 + goalForm.timeSeconds;
      
      const goalData: RecordGoalEventRequest = {
        matchId: currentMatch.id,
        teamId: goalForm.teamId,
        playerId: goalForm.playerId,
        assisterId: goalForm.assisterId || undefined,
        periodNumber: clock.period,
        timeInSeconds: timeInSeconds,
        wasInOvertime: currentMatch.wentToOvertime || clock.period > 2,
        wasInShootout: currentMatch.wentToShootout || clock.period > 3,
      };
      
      await floorballMatchEventService.recordGoal(goalData);
      
      // Refresh events from backend
      await loadMatchEvents();
      // Refresh match status to sync scores
      await loadCurrentMatchStatus();
      
      // Reset form
      setGoalForm({ teamId: '', playerId: '', assisterId: '', timeMinutes: 0, timeSeconds: 0 });
      setShowGoalForm(false);
      setError(null);
      
    } catch (error) {
      console.error('Error recording goal:', error);
      setError(error instanceof Error ? error.message : 'Failed to record goal');
    } finally {
      setLoading(false);
    }
  }, [goalForm, currentMatch, clock.period, loadMatchEvents, loadCurrentMatchStatus, setError]);

  /**
   * Records a penalty event
   */
  const recordPenalty = useCallback(async () => {
    if (!penaltyForm.teamId || !penaltyForm.penaltyType) {
      setError('Please select team and penalty type');
      return;
    }

    const key = `${currentMatch.id}:${penaltyForm.teamId}:${penaltyForm.playerId || 'team'}`;
    const now = Date.now();
    if (penaltyThrottleRef.current[key] && now - penaltyThrottleRef.current[key] < throttleMs) {
      setError('Please wait a moment before recording another penalty.');
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
        periodNumber: clock.period,
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
      setError(error instanceof Error ? error.message : 'Failed to record penalty');
    } finally {
      setLoading(false);
    }
  }, [penaltyForm, currentMatch, clock.period, loadMatchEvents, loadCurrentMatchStatus, setError]);

  return {
    // Form visibility
    showGoalForm,
    setShowGoalForm,
    showPenaltyForm,
    setShowPenaltyForm,
    openPenaltyForm,
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