import { useCallback, useRef } from 'react';
import { signalRService, type MatchEvent } from '../../../../../services/signalRService';
import type { PeriodEventData, GoalEventData, PenaltyEventData, SaveEventData } from '../components/types';

interface UseSignalRProps {
  matchId: string;
  onPeriodStarted: (eventData: PeriodEventData) => void;
  onGoalScored: (eventData: GoalEventData) => void;
  onPenaltyAssigned: (eventData: PenaltyEventData) => void;
  onSaveRecorded: (eventData: SaveEventData) => void;
}

/**
 * Subscribes the manage page to the match's real-time events.
 *
 * The registered hub callback always forwards to the latest handlers (held in a ref), so the
 * subscription can be created once on mount without going stale. `cleanupSignalR` removes the
 * hub callback again; without that every mount (including React StrictMode's double mount)
 * would leave an extra handler behind and each event would trigger duplicate refreshes.
 */
export const useSignalR = ({
  matchId,
  onPeriodStarted,
  onGoalScored,
  onPenaltyAssigned,
  onSaveRecorded
}: UseSignalRProps) => {
  const handlersRef = useRef({ onPeriodStarted, onGoalScored, onPenaltyAssigned, onSaveRecorded });
  handlersRef.current = { onPeriodStarted, onGoalScored, onPenaltyAssigned, onSaveRecorded };

  const unsubscribeRef = useRef<(() => void) | null>(null);
  /** False once cleanup has run, so a setup still awaiting the connection does not register. */
  const activeRef = useRef<boolean>(false);

  const handleSignalREvent = useCallback((event: MatchEvent): void => {
    const eventData = event.data as { MatchId?: string };

    if (eventData?.MatchId !== matchId) {
      return;
    }

    const handlers = handlersRef.current;
    switch (event.eventType) {
      case 'FloorballGoalScored':
        handlers.onGoalScored(event.data as GoalEventData);
        break;
      case 'FloorballPenaltyAssigned':
        handlers.onPenaltyAssigned(event.data as PenaltyEventData);
        break;
      case 'FloorballSaveRecorded':
        handlers.onSaveRecorded(event.data as SaveEventData);
        break;
      case 'FloorballPeriodStartedEvent':
        handlers.onPeriodStarted(event.data as PeriodEventData);
        break;
    }
  }, [matchId]);

  const setupSignalR = useCallback(async (): Promise<void> => {
    activeRef.current = true;
    try {
      await signalRService.connect();

      if (activeRef.current && signalRService.isConnected) {
        await signalRService.subscribeToMatch(matchId);
        if (!activeRef.current) return;
        unsubscribeRef.current?.();
        unsubscribeRef.current = signalRService.onMatchEvent(handleSignalREvent);
      }
    } catch (error) {
      console.error('Error setting up SignalR:', error);
    }
  }, [matchId, handleSignalREvent]);

  const cleanupSignalR = useCallback(async (): Promise<void> => {
    activeRef.current = false;
    unsubscribeRef.current?.();
    unsubscribeRef.current = null;
    try {
      if (signalRService.isConnected) {
        await signalRService.unsubscribeFromMatch(matchId);
      }
    } catch (error) {
      console.error('Error cleaning up SignalR:', error);
    }
  }, [matchId]);

  return {
    setupSignalR,
    cleanupSignalR,
    handleSignalREvent
  };
};
