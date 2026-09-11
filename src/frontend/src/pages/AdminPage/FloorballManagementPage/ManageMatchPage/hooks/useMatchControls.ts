import { useCallback } from 'react';
import { floorballMatchService } from '../../../../../api/floorball/floorballMatchService';
import { timerService } from '../../../../../api/common/timerService';
import type { FloorballMatchDto } from '../../../../../types/floorball/floorballTypes';

interface UseMatchControlsProps {
  currentMatch: FloorballMatchDto;
  setCurrentMatch: (match: FloorballMatchDto) => void;
  setError: (error: string | null) => void;
  setLoading: (loading: boolean) => void;
  onGoLive?: (matchId: string, updatedMatch?: FloorballMatchDto) => void;
  onCompleteLive?: (matchId: string, updatedMatch?: FloorballMatchDto) => void;
  onReopen?: (matchId: string, updatedMatch?: FloorballMatchDto) => void;
}

export const useMatchControls = ({
  currentMatch,
  setCurrentMatch,
  setError,
  setLoading,
  onGoLive,
  onCompleteLive,
  onReopen,
}: UseMatchControlsProps) => {

  /**
   * Simple function that only starts the match
   */
  const handleStartMatch = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      
      const response = await floorballMatchService.start(currentMatch.id);
      
      if (response.success && response.data) {
        setCurrentMatch(response.data);
        if (onGoLive) {
          onGoLive(currentMatch.id, response.data);
        }
      } else {
        throw new Error('Failed to start match');
      }
      
      setError(null);
    } catch (error) {
      console.error('Error starting match:', error);
      setError(error instanceof Error ? error.message : 'Failed to start match');
    } finally {
      setLoading(false);
    }
  }, [currentMatch.id, setCurrentMatch, setError, setLoading, onGoLive]);

  /**
   * Handles completing the live match
   */
  const handleCompleteLive = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      
      // Use the event sourced endpoint to complete the match
      const response = await floorballMatchService.complete(currentMatch.id);
      
      if (response.success && response.data) {
        
        // Destroy the timer for this match to stop background service queries
        try {
          await timerService.destroyTimer(currentMatch.id);
        } catch (timerError) {
          console.warn('Failed to destroy timer for match:', currentMatch.id, timerError);
          // Don't fail the match completion if timer destruction fails
        }
        
        // Update the current match with the response from the backend
        setCurrentMatch(response.data);
        
        // Update the match with the response from the backend
        // This will include the updated status from the event sourced system
        if (onCompleteLive) {
          onCompleteLive(currentMatch.id, response.data);
        }
        
      } else {
        setError('Failed to complete match');
      }
    } catch (error) {
      console.error('Error completing match:', error);
      setError(error instanceof Error ? error.message : 'Failed to complete match');
    } finally {
      setLoading(false);
    }
    // Don't close the modal - let it stay open with "Match Finished" status
  }, [currentMatch.id, setCurrentMatch, setError, setLoading, onCompleteLive]);

  /**
   * Reopens a previously completed match back to InProgress so the operator can correct
   * accidentally recorded results or continue play. The backend reverses the per-match
   * aggregates that were applied at completion time.
   */
  const handleReopenMatch = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);

      const response = await floorballMatchService.reopen(currentMatch.id);

      if (response.success && response.data) {
        setCurrentMatch(response.data);
        if (onReopen) {
          onReopen(currentMatch.id, response.data);
        }
      } else {
        throw new Error('Failed to reopen match');
      }
    } catch (error) {
      console.error('Error reopening match:', error);
      setError(error instanceof Error ? error.message : 'Failed to reopen match');
    } finally {
      setLoading(false);
    }
  }, [currentMatch.id, setCurrentMatch, setError, setLoading, onReopen]);

  return {
    handleStartMatch,
    handleCompleteLive,
    handleReopenMatch,
  };
}; 