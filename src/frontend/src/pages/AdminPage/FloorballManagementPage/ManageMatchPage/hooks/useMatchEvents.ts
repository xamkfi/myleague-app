import { useState, useCallback, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { floorballMatchEventService, type FloorballDomainEventDto } from '../../../../../api/floorball/floorballMatchEventService';
import type { FloorballMatchDto, FloorballTeam } from '../../../../../types/floorball/floorballTypes';
import type { GoalEventData, PenaltyEventData, ProcessedEvent, SaveEventData } from '../components/types';
import { mapFloorballDomainEvents } from '../utils/mapMatchEvents';

interface UseMatchEventsProps {
  matchId: string;
  currentMatch: FloorballMatchDto;
  homeTeam: FloorballTeam | null;
  awayTeam: FloorballTeam | null;
  getPlayerNameById: (playerId: string | undefined | null) => string;
  loadCurrentMatchStatus: () => Promise<void>;
}

/**
 * Loads the match's domain events and reacts to real-time SignalR notifications by
 * refreshing from the backend (the backend is the source of truth for scores and order).
 */
export const useMatchEvents = ({
  matchId,
  currentMatch,
  homeTeam,
  awayTeam,
  getPlayerNameById,
  loadCurrentMatchStatus,
}: UseMatchEventsProps) => {
  const { t } = useTranslation();
  const [matchEvents, setMatchEvents] = useState<FloorballDomainEventDto[]>([]);

  const loadMatchEvents = useCallback(async (): Promise<void> => {
    try {
      const response = await floorballMatchEventService.getMatchEvents(matchId);
      if (response.success && response.data) {
        setMatchEvents(response.data as FloorballDomainEventDto[]);
      }
    } catch (error) {
      // Not critical: the list simply stays as it was until the next refresh.
      console.error('Error loading match events:', error);
    }
  }, [matchId]);

  const handleGoalScored = useCallback((eventData: GoalEventData): void => {
    if (eventData.MatchId !== matchId) return;
    void loadCurrentMatchStatus();
    void loadMatchEvents();
  }, [matchId, loadCurrentMatchStatus, loadMatchEvents]);

  const handlePenaltyAssigned = useCallback((eventData: PenaltyEventData): void => {
    if (eventData.MatchId !== matchId) return;
    void loadMatchEvents();
  }, [matchId, loadMatchEvents]);

  const handleSaveRecorded = useCallback((eventData: SaveEventData): void => {
    if (eventData.MatchId !== matchId) return;
    void loadMatchEvents();
  }, [matchId, loadMatchEvents]);

  const allEvents: ProcessedEvent[] = useMemo(() => mapFloorballDomainEvents(matchEvents, {
    homeTeamId: currentMatch.homeTeamId,
    homeTeamName: homeTeam?.name,
    awayTeamName: awayTeam?.name,
    homeTeamShortName: homeTeam?.shortName,
    awayTeamShortName: awayTeam?.shortName,
    getPlayerNameById,
    labels: {
      home: t('floorball.matches.manage.scoreboard.home', 'Home'),
      away: t('floorball.matches.manage.scoreboard.away', 'Away'),
      teamPenalty: t('floorball.matches.manage.events.teamPenalty', 'Team penalty'),
    },
  }), [
    matchEvents,
    currentMatch.homeTeamId,
    homeTeam?.name,
    homeTeam?.shortName,
    awayTeam?.name,
    awayTeam?.shortName,
    getPlayerNameById,
    t,
  ]);

  return {
    matchEvents,
    allEvents,
    loadMatchEvents,
    handleGoalScored,
    handlePenaltyAssigned,
    handleSaveRecorded,
  };
};
