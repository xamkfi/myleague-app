import { useState, useCallback } from 'react';
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
  setError: (error: string | null) => void;
}

export const useFormState = ({
  currentMatch,
  clock,
  currentTimerElapsedTime,
  loadMatchEvents,
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

  /**
   * Opens the penalty form
   */
  const openPenaltyForm = useCallback(() => {
    setShowPenaltyForm(true);
  }, []);

  /**
   * Records a goal event
   */
  const recordGoal = useCallback(async () => {
    if (!goalForm.teamId || !goalForm.playerId) {
      setError('Please select team and player');
      return;
    }
    
    try {
      setLoading(true);
      
      const goalData: RecordGoalEventRequest = {
        matchId: currentMatch.id,
        teamId: goalForm.teamId,
        playerId: goalForm.playerId,
        assisterId: goalForm.assisterId || undefined,
        periodNumber: clock.period,
        timeInSeconds: currentTimerElapsedTime,
        wasInOvertime: currentMatch.wentToOvertime || clock.period > 3,
        wasInShootout: currentMatch.wentToShootout || clock.period > 4,
      };
      
      await floorballMatchEventService.recordGoal(goalData);
      
      // Refresh events from backend
      await loadMatchEvents();
      
      // Reset form
      setGoalForm({ teamId: '', playerId: '', assisterId: '' });
      setShowGoalForm(false);
      setError(null);
      
    } catch (error) {
      console.error('Error recording goal:', error);
      setError(error instanceof Error ? error.message : 'Failed to record goal');
    } finally {
      setLoading(false);
    }
  }, [goalForm, currentMatch, clock.period, currentTimerElapsedTime, loadMatchEvents, setError]);

  /**
   * Records a penalty event
   */
  const recordPenalty = useCallback(async () => {
    if (!penaltyForm.teamId || !penaltyForm.penaltyType) {
      setError('Please select team and penalty type');
      return;
    }
    
    try {
      setLoading(true);
      
      const penaltyData: RecordPenaltyEventRequest = {
        matchId: currentMatch.id,
        teamId: penaltyForm.teamId,
        playerId: penaltyForm.playerId || undefined,
        penaltyType: penaltyForm.penaltyType,
        durationMinutes: penaltyForm.minutes,
        periodNumber: clock.period,
        timeInSeconds: currentTimerElapsedTime,
        description: penaltyForm.description,
      };
      
      await floorballMatchEventService.recordPenalty(penaltyData);
      
      // Refresh events from backend
      await loadMatchEvents();
      
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
  }, [penaltyForm, currentMatch, clock.period, currentTimerElapsedTime, loadMatchEvents, setError]);

  return {
    // Form visibility
    showGoalForm,
    setShowGoalForm,
    showPenaltyForm,
    setShowPenaltyForm,
    openPenaltyForm,
    
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