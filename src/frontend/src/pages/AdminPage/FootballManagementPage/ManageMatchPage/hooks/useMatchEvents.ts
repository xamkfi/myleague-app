import { useState, useCallback, useMemo } from 'react';
import { footballMatchEventService, type FootballDomainEventDto } from '../../../../../api/football/footballMatchEventService';
import type { FootballMatchDto, FootballTeam } from '../../../../../types/football/footballTypes';
import type {
  GoalEventData,
  CardEventData,
  SubstitutionEventData,
  MatchLifecycleEventData,
  ProcessedEvent,
} from '../components/types';

interface UseMatchEventsProps {
  match: FootballMatchDto;
  currentMatch: FootballMatchDto;
  homeTeam: FootballTeam | null;
  awayTeam: FootballTeam | null;
  getPlayerNameById: (playerId: string | undefined | null) => string;
  loadCurrentMatchStatus: () => Promise<void>;
}

interface GoalEventFields {
  eventId?: string;
  MatchId?: string;
  matchId?: string;
  TeamId?: string;
  teamId?: string;
  PlayerId?: string;
  playerId?: string;
  PeriodNumber?: number;
  periodNumber?: number;
  TimeInSeconds?: number;
  timeInSeconds?: number;
  EventTime?: string;
  eventTime?: string;
  AssisterId?: string;
  assisterId?: string;
  GoalType?: number | string | null;
  goalType?: number | string | null;
}

interface CardEventFields {
  eventId?: string;
  TeamId?: string;
  teamId?: string;
  PlayerId?: string;
  playerId?: string;
  PeriodNumber?: number;
  periodNumber?: number;
  TimeInSeconds?: number;
  timeInSeconds?: number;
  EventTime?: string;
  eventTime?: string;
  CardType?: number | string;
  cardType?: number | string;
  Description?: string;
  description?: string;
}

interface SubstitutionEventFields {
  eventId?: string;
  TeamId?: string;
  teamId?: string;
  PlayerOffId?: string;
  playerOffId?: string;
  PlayerOnId?: string;
  playerOnId?: string;
  PeriodNumber?: number;
  periodNumber?: number;
  TimeInSeconds?: number;
  timeInSeconds?: number;
  EventTime?: string;
  eventTime?: string;
  Description?: string;
  description?: string;
}

const parseEventTime = (eventTime: string | undefined, fallback: number): number => {
  if (fallback !== 0 || !eventTime) {
    return fallback;
  }
  const timeParts = eventTime.split(':');
  if (timeParts.length === 2) {
    const minutes = parseInt(timeParts[0], 10) || 0;
    const seconds = parseInt(timeParts[1], 10) || 0;
    return minutes * 60 + seconds;
  }
  if (timeParts.length === 3) {
    const hours = parseInt(timeParts[0], 10) || 0;
    const minutes = parseInt(timeParts[1], 10) || 0;
    const seconds = parseInt(timeParts[2], 10) || 0;
    return hours * 3600 + minutes * 60 + seconds;
  }
  return fallback;
};

export const useMatchEvents = ({
  match,
  currentMatch,
  homeTeam,
  awayTeam,
  getPlayerNameById,
  loadCurrentMatchStatus,
}: UseMatchEventsProps) => {
  const [matchEvents, setMatchEvents] = useState<FootballDomainEventDto[]>([]);

  const loadMatchEvents = useCallback(async () => {
    try {
      const response = await footballMatchEventService.getMatchEvents(match.id);
      if (response.success && response.data) {
        setMatchEvents(response.data);
      }
    } catch {
      // Events loading is not critical for operating the match
    }
  }, [match.id]);

  const refreshMatch = useCallback(() => {
    void loadCurrentMatchStatus();
    void loadMatchEvents();
  }, [loadCurrentMatchStatus, loadMatchEvents]);

  const isThisMatch = useCallback((eventMatchId: string | undefined): boolean => {
    return eventMatchId === match.id;
  }, [match.id]);

  const handleGoalScored = useCallback((eventData: GoalEventData) => {
    if (!isThisMatch(eventData.MatchId)) return;
    refreshMatch();
  }, [isThisMatch, refreshMatch]);

  const handleCardAssigned = useCallback((eventData: CardEventData) => {
    if (!isThisMatch(eventData.MatchId)) return;
    refreshMatch();
  }, [isThisMatch, refreshMatch]);

  const handleSubstitutionRecorded = useCallback((eventData: SubstitutionEventData) => {
    if (!isThisMatch(eventData.MatchId)) return;
    refreshMatch();
  }, [isThisMatch, refreshMatch]);

  const handleMatchStarted = useCallback((eventData: MatchLifecycleEventData) => {
    if (!isThisMatch(eventData.MatchId)) return;
    refreshMatch();
  }, [isThisMatch, refreshMatch]);

  const handleMatchCompleted = useCallback((eventData: MatchLifecycleEventData) => {
    if (!isThisMatch(eventData.MatchId)) return;
    refreshMatch();
  }, [isThisMatch, refreshMatch]);

  const allEvents = useMemo(() => {
    if (!matchEvents) {
      return [];
    }

    const events: ProcessedEvent[] = matchEvents
      .filter((event): event is FootballDomainEventDto => event !== null && event !== undefined)
      .map((event: FootballDomainEventDto): ProcessedEvent | null => {
        if (event.eventType === 'FootballGoalScoredEvent') {
          const goalData = event.data as GoalEventFields;
          const timeInSeconds = parseEventTime(
            goalData.EventTime ?? goalData.eventTime,
            goalData.TimeInSeconds ?? goalData.timeInSeconds ?? 0,
          );
          const periodNumber = goalData.PeriodNumber ?? goalData.periodNumber ?? 1;
          const teamId = goalData.TeamId ?? goalData.teamId ?? '';
          const playerId = goalData.PlayerId ?? goalData.playerId ?? '';
          const assisterId = goalData.AssisterId ?? goalData.assisterId;
          const goalType = goalData.GoalType ?? goalData.goalType ?? null;

          return {
            id: `goal-${teamId}-${playerId}-${periodNumber}-${timeInSeconds}`,
            type: 'goal',
            eventId: goalData.eventId ?? '',
            teamId,
            teamName: teamId === currentMatch.homeTeamId ? (homeTeam?.name || 'Home') : (awayTeam?.name || 'Away'),
            teamShortName: teamId === currentMatch.homeTeamId ? homeTeam?.shortName : awayTeam?.shortName,
            playerId,
            playerName: getPlayerNameById(playerId),
            assisterId,
            assisterName: assisterId ? getPlayerNameById(assisterId) : undefined,
            periodNumber,
            timeInSeconds,
            timestamp: new Date(event.occurredOn),
            goalType,
          };
        }

        if (event.eventType === 'FootballCardAssignedEvent') {
          const cardData = event.data as CardEventFields;
          const timeInSeconds = parseEventTime(
            cardData.EventTime ?? cardData.eventTime,
            cardData.TimeInSeconds ?? cardData.timeInSeconds ?? 0,
          );
          const periodNumber = cardData.PeriodNumber ?? cardData.periodNumber ?? 1;
          const teamId = cardData.TeamId ?? cardData.teamId ?? '';
          const playerId = cardData.PlayerId ?? cardData.playerId ?? '';
          const cardType = cardData.CardType ?? cardData.cardType ?? '';
          const description = cardData.Description ?? cardData.description ?? '';

          return {
            id: `card-${teamId}-${playerId}-${periodNumber}-${timeInSeconds}`,
            type: 'card',
            eventId: cardData.eventId ?? '',
            teamId,
            teamName: teamId === currentMatch.homeTeamId ? (homeTeam?.name || 'Home') : (awayTeam?.name || 'Away'),
            teamShortName: teamId === currentMatch.homeTeamId ? homeTeam?.shortName : awayTeam?.shortName,
            playerId,
            playerName: playerId ? getPlayerNameById(playerId) : 'Unknown player',
            periodNumber,
            timeInSeconds,
            timestamp: new Date(event.occurredOn),
            cardType,
            description,
          };
        }

        if (event.eventType === 'FootballSubstitutionRecordedEvent') {
          const subData = event.data as SubstitutionEventFields;
          const timeInSeconds = parseEventTime(
            subData.EventTime ?? subData.eventTime,
            subData.TimeInSeconds ?? subData.timeInSeconds ?? 0,
          );
          const periodNumber = subData.PeriodNumber ?? subData.periodNumber ?? 1;
          const teamId = subData.TeamId ?? subData.teamId ?? '';
          const playerOffId = subData.PlayerOffId ?? subData.playerOffId ?? '';
          const playerOnId = subData.PlayerOnId ?? subData.playerOnId ?? '';
          const description = subData.Description ?? subData.description ?? '';

          return {
            id: `sub-${teamId}-${playerOffId}-${playerOnId}-${periodNumber}-${timeInSeconds}`,
            type: 'substitution',
            eventId: subData.eventId ?? '',
            teamId,
            teamName: teamId === currentMatch.homeTeamId ? (homeTeam?.name || 'Home') : (awayTeam?.name || 'Away'),
            teamShortName: teamId === currentMatch.homeTeamId ? homeTeam?.shortName : awayTeam?.shortName,
            playerId: playerOnId,
            playerName: getPlayerNameById(playerOnId),
            playerOffId,
            playerOffName: getPlayerNameById(playerOffId),
            playerOnId,
            playerOnName: getPlayerNameById(playerOnId),
            periodNumber,
            timeInSeconds,
            timestamp: new Date(event.occurredOn),
            description,
          };
        }

        return null;
      })
      .filter((event): event is ProcessedEvent => event !== null);

    return events.sort((a, b) => {
      if (a.periodNumber !== b.periodNumber) {
        return b.periodNumber - a.periodNumber;
      }
      return b.timeInSeconds - a.timeInSeconds;
    });
  }, [
    matchEvents,
    currentMatch.homeTeamId,
    homeTeam?.name,
    homeTeam?.shortName,
    awayTeam?.name,
    awayTeam?.shortName,
    getPlayerNameById,
  ]);

  return {
    matchEvents,
    setMatchEvents,
    allEvents,
    loadMatchEvents,
    handleGoalScored,
    handleCardAssigned,
    handleSubstitutionRecorded,
    handleMatchStarted,
    handleMatchCompleted,
  };
};
