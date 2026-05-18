import { useCallback } from 'react';
import { signalRService, type MatchEvent } from '../../../../../services/signalRService';
import type { PeriodEventData, GoalEventData, PenaltyEventData, SaveEventData } from '../components/types';

interface UseSignalRProps {
  matchId: string;
  onPeriodStarted: (eventData: PeriodEventData) => void;
  onGoalScored: (eventData: GoalEventData) => void;
  onPenaltyAssigned: (eventData: PenaltyEventData) => void;
  onSaveRecorded: (eventData: SaveEventData) => void;
}

export const useSignalR = ({
  matchId,
  onPeriodStarted,
  onGoalScored,
  onPenaltyAssigned,
  onSaveRecorded
}: UseSignalRProps) => {

  const handleSignalREvent = useCallback((event: MatchEvent) => {
    const eventData = event.data as { MatchId?: string };

    if (eventData?.MatchId !== matchId) {
      return;
    }

    switch (event.eventType) {
      case 'FloorballGoalScored':
        onGoalScored(event.data as GoalEventData);
        break;
      case 'FloorballPenaltyAssigned':
        onPenaltyAssigned(event.data as PenaltyEventData);
        break;
      case 'FloorballSaveRecorded':
        onSaveRecorded(event.data as SaveEventData);
        break;
      case 'FloorballPeriodStartedEvent':
        onPeriodStarted(event.data as PeriodEventData);
        break;
    }
  }, [matchId, onGoalScored, onPenaltyAssigned, onPeriodStarted, onSaveRecorded]);

  const setupSignalR = useCallback(async () => {
    try {
      await signalRService.connect();

      if (signalRService.isConnected) {
        await signalRService.subscribeToMatch(matchId);
        return signalRService.onMatchEvent(handleSignalREvent);
      }
    } catch (error) {
      console.error('Error setting up SignalR:', error);
    }
  }, [matchId, handleSignalREvent]);

  const cleanupSignalR = useCallback(async () => {
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