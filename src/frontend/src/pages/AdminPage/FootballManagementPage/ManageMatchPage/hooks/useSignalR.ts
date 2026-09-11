import { useCallback } from 'react';
import { signalRService, type MatchEvent } from '../../../../../services/signalRService';
import type {
  PeriodEventData,
  GoalEventData,
  CardEventData,
  SubstitutionEventData,
  MatchLifecycleEventData,
} from '../components/types';

interface UseSignalRProps {
  matchId: string;
  onPeriodStarted: (eventData: PeriodEventData) => void;
  onGoalScored: (eventData: GoalEventData) => void;
  onCardAssigned: (eventData: CardEventData) => void;
  onSubstitutionRecorded: (eventData: SubstitutionEventData) => void;
  onMatchStarted: (eventData: MatchLifecycleEventData) => void;
  onMatchCompleted: (eventData: MatchLifecycleEventData) => void;
}

const matchIdFrom = (data: { MatchId?: string; matchId?: string } | undefined): string | undefined =>
  data?.MatchId ?? data?.matchId;

export const useSignalR = ({
  matchId,
  onPeriodStarted,
  onGoalScored,
  onCardAssigned,
  onSubstitutionRecorded,
  onMatchStarted,
  onMatchCompleted,
}: UseSignalRProps) => {
  const handleSignalREvent = useCallback((event: MatchEvent) => {
    const eventData = event.data as { MatchId?: string; matchId?: string };
    if (matchIdFrom(eventData) !== matchId) {
      return;
    }

    switch (event.eventType) {
      case 'FootballGoalScored':
        onGoalScored(event.data as GoalEventData);
        break;
      case 'FootballCardAssigned':
        onCardAssigned(event.data as CardEventData);
        break;
      case 'FootballSubstitutionRecorded':
        onSubstitutionRecorded(event.data as SubstitutionEventData);
        break;
      case 'FootballMatchStarted':
        onMatchStarted(event.data as MatchLifecycleEventData);
        break;
      case 'FootballMatchCompleted':
        onMatchCompleted(event.data as MatchLifecycleEventData);
        break;
      case 'FootballPeriodStartedEvent':
        onPeriodStarted(event.data as PeriodEventData);
        break;
    }
  }, [
    matchId,
    onGoalScored,
    onCardAssigned,
    onSubstitutionRecorded,
    onMatchStarted,
    onMatchCompleted,
    onPeriodStarted,
  ]);

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
    handleSignalREvent,
  };
};
