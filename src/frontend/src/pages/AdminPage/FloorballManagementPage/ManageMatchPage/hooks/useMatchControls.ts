import { useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { floorballMatchService } from '../../../../../api/floorball/floorballMatchService';
import { timerService } from '../../../../../api/common/timerService';
import type { FloorballMatchDto } from '../../../../../types/floorball/floorballTypes';
import type { ApiResponse } from '../../../../../types/common/apiResponseType';
import { describeMatchError } from '../utils/describeMatchError';

interface UseMatchControlsProps {
  currentMatch: FloorballMatchDto;
  setCurrentMatch: (match: FloorballMatchDto) => void;
  setError: (error: string | null) => void;
  setLoading: (loading: boolean) => void;
  /** Receives the updated match after any successful lifecycle transition. */
  onMatchChanged?: (updatedMatch: FloorballMatchDto) => void;
}

/**
 * Match lifecycle transitions (start / complete / reopen / revert to not started). Every action resolves to
 * `true` on success and `false` after surfacing an error, so callers can chain follow-up
 * work (e.g. starting the clock) only when the transition really happened.
 */
export const useMatchControls = ({
  currentMatch,
  setCurrentMatch,
  setError,
  setLoading,
  onMatchChanged,
}: UseMatchControlsProps) => {
  const { t } = useTranslation();

  const runTransition = useCallback(async (
    request: () => Promise<ApiResponse<FloorballMatchDto>>,
    fallbackMessage: string,
    afterSuccess?: () => Promise<void>,
  ): Promise<boolean> => {
    setLoading(true);
    setError(null);
    try {
      const response: ApiResponse<FloorballMatchDto> = await request();
      if (!response.success || !response.data) {
        setError(response.message || fallbackMessage);
        return false;
      }
      if (afterSuccess) await afterSuccess();
      setCurrentMatch(response.data);
      onMatchChanged?.(response.data);
      return true;
    } catch (error) {
      setError(describeMatchError(error, fallbackMessage, t));
      return false;
    } finally {
      setLoading(false);
    }
  }, [setLoading, setError, setCurrentMatch, onMatchChanged, t]);

  const handleStartMatch = useCallback((): Promise<boolean> =>
    runTransition(
      () => floorballMatchService.start(currentMatch.id),
      t('floorball.matches.manage.errors.startMatch', 'Failed to start match'),
    ), [runTransition, currentMatch.id, t]);

  const handleCompleteLive = useCallback((): Promise<boolean> =>
    runTransition(
      () => floorballMatchService.complete(currentMatch.id),
      t('floorball.matches.manage.errors.completeMatch', 'Failed to complete match'),
      async () => {
        // Stop the background timer for this match; not fatal if it is already gone.
        try {
          await timerService.destroyTimer(currentMatch.id);
        } catch (timerError) {
          console.warn('Failed to destroy timer for match:', currentMatch.id, timerError);
        }
      },
    ), [runTransition, currentMatch.id, t]);

  /**
   * Reopens a completed match back to InProgress so the operator can correct results or
   * continue play. The backend reverses the per-match aggregates applied at completion.
   */
  const handleReopenMatch = useCallback((): Promise<boolean> =>
    runTransition(
      () => floorballMatchService.reopen(currentMatch.id),
      t('floorball.matches.manage.errors.reopenMatch', 'Failed to reopen match'),
    ), [runTransition, currentMatch.id, t]);

  /**
   * Puts a match that was started by mistake back to not started. The backend also removes
   * the match timer, and only allows this at 0-0 with no recorded events.
   */
  const handleRevertToScheduled = useCallback((): Promise<boolean> =>
    runTransition(
      () => floorballMatchService.revertToScheduled(currentMatch.id),
      t('floorball.matches.manage.errors.revertToScheduled', 'Failed to revert match to not started'),
    ), [runTransition, currentMatch.id, t]);

  return {
    handleStartMatch,
    handleCompleteLive,
    handleReopenMatch,
    handleRevertToScheduled,
  };
};
