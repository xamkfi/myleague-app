import { useState, useCallback, useMemo } from 'react';
import { floorballMatchEventService, type FloorballDomainEventDto } from '../../../../../api/floorball/floorballMatchEventService';
import type { FloorballMatchDto, FloorballTeam } from '../../../../../types/floorball/floorballTypes';
import type { GoalEventData, PenaltyEventData, SaveEventData } from '../components/types';

interface UseMatchEventsProps {
  match: FloorballMatchDto;
  currentMatch: FloorballMatchDto;
  homeTeam: FloorballTeam | null;
  awayTeam: FloorballTeam | null;
  getPlayerNameById: (playerId: string | undefined | null) => string;
  loadCurrentMatchStatus: () => Promise<void>;
}

export const useMatchEvents = ({
  match,
  currentMatch,
  homeTeam,
  awayTeam,
  getPlayerNameById,
  loadCurrentMatchStatus
}: UseMatchEventsProps) => {
  const [matchEvents, setMatchEvents] = useState<FloorballDomainEventDto[]>([]);

  /**
   * Loads all match events (goals and penalties) from the backend
   * The backend returns domain events in a flat array structure
   */
  const loadMatchEvents = useCallback(async () => {
    try {
      console.log('loadMatchEvents called for match:', match.id);
      const response = await floorballMatchEventService.getMatchEvents(match.id);
      
      console.log('loadMatchEvents response:', response);
      
      if (response.success && response.data) {
        console.log('Setting match events:', response.data);
        setMatchEvents(response.data as FloorballDomainEventDto[]);
      }
    } catch (error) {
      console.error('Error loading match events:', error);
      // Don't set error for events loading - it's not critical
    }
  }, [match.id]);

  /**
   * Handles real-time goal events from SignalR
   * Refreshes the match data from backend to get accurate score
   */
  const handleGoalScored = useCallback((eventData: GoalEventData) => {
    console.log('handleGoalScored called with eventData:', eventData);
    
    // Verify this event is for our match
    if (eventData.MatchId !== match.id) {
      console.log('Goal event is for different match, ignoring');
      return;
    }
    
    console.log('Goal scored for team:', eventData.TeamId);
    console.log('Player ID:', eventData.PlayerId);
    console.log('Period number:', eventData.PeriodNumber);
    console.log('Event time:', eventData.EventTime);
    console.log('Home team:', eventData.HomeTeam);
    console.log('Away team:', eventData.AwayTeam);
    
    // Refresh match data from backend to get accurate score
    loadCurrentMatchStatus();
    
    // Refresh events list
    loadMatchEvents();
  }, [match.id, loadCurrentMatchStatus, loadMatchEvents]);

  /**
   * Handles real-time penalty events from SignalR
   * Refreshes the events list to show the new penalty
   */
  const handlePenaltyAssigned = useCallback((eventData: PenaltyEventData) => {
    console.log('handlePenaltyAssigned called with eventData:', eventData);
    
    // Verify this event is for our match
    if (eventData.MatchId !== match.id) {
      console.log('Penalty event is for different match, ignoring');
      return;
    }
    
    console.log('Penalty assigned for team:', eventData.TeamId);
    console.log('Penalty type:', eventData.PenaltyType);
    console.log('Player ID:', eventData.PlayerId);
    
    // Refresh events list
    loadMatchEvents();
  }, [match.id, loadMatchEvents]);
  
  /**
   * Handles real-time save events from SignalR
   */
  const handleSaveRecorded = useCallback((eventData: SaveEventData) => {
    if (eventData.MatchId !== match.id) {
      return;
    }

    // Create a new event DTO from the SignalR data
    const newSaveEvent: FloorballDomainEventDto = {
      id: `save-${eventData.TeamId}-${eventData.GoalieId}-${eventData.PeriodNumber}-${eventData.TimeInSeconds}`,
      eventType: 'FloorballSaveEvent',
      occurredOn: new Date().toISOString(),
      data: {
        ...eventData,
        // Ensure camelCase properties are included for consistency if needed,
        // but the main thing is that GoalieName is now in eventData.
      }
    };
    
    // Prepend the new event to the existing events list
    setMatchEvents(prevEvents => [newSaveEvent, ...prevEvents]);
  }, [match.id, setMatchEvents]);

  /**
   * Processes and combines all match events (goals and penalties) from the backend
   * The backend returns domain events, so we need to extract and format the relevant data
   */
  const allEvents = useMemo(() => {
    console.log('allEvents useMemo called with matchEvents:', matchEvents);
    
    if (!matchEvents) {
      console.log('No matchEvents, returning empty array');
      return [];
    }

    // Shape is normalized later; we accept both PascalCase and camelCase
    const events = matchEvents
      .filter((event): event is FloorballDomainEventDto => event !== null && event !== undefined)
      .map((event: FloorballDomainEventDto) => {
        console.log('Processing event:', event);
        console.log('Event type:', event.eventType);
        console.log('Event data:', event.data);
        
        // Handle goal events
        if (event.eventType === 'FloorballGoalScoredEvent') {
          console.log('Processing FloorballGoalScoredEvent');
          console.log('Raw event data:', event.data);
          
          const goalData = event.data as {
            MatchId?: string;
            TeamId?: string;
            PlayerId?: string;
            PeriodNumber?: number;
            TimeInSeconds?: number;
            EventTime?: string;
            IsOvertime?: boolean;
            IsPenaltyShot?: boolean;
            IsShootout?: boolean;
            AssisterId?: string;
            SecondaryAssisterId?: string;
            // Handle camelCase field names from JSON serialization
            matchId?: string;
            teamId?: string;
            playerId?: string;
            periodNumber?: number;
            timeInSeconds?: number;
            eventTime?: string;
            isOvertime?: boolean;
            isPenaltyShot?: boolean;
            isShootout?: boolean;
            assisterId?: string;
            secondaryAssisterId?: string;
          };
          
          // Handle both PascalCase and camelCase field names
          let timeInSeconds = goalData.TimeInSeconds ?? goalData.timeInSeconds ?? 0;
          
          // If TimeInSeconds is not available, try to parse EventTime
          if (timeInSeconds === 0 && (goalData.EventTime || goalData.eventTime)) {
            const eventTime = goalData.EventTime ?? goalData.eventTime;
            if (typeof eventTime === 'string') {
              const timeParts = eventTime.split(':');
              if (timeParts.length === 2) {
                const minutes = parseInt(timeParts[0]) || 0;
                const seconds = parseInt(timeParts[1]) || 0;
                timeInSeconds = minutes * 60 + seconds;
              } else if (timeParts.length === 3) {
                const hours = parseInt(timeParts[0]) || 0;
                const minutes = parseInt(timeParts[1]) || 0;
                const seconds = parseInt(timeParts[2]) || 0;
                timeInSeconds = hours * 3600 + minutes * 60 + seconds;
              }
            }
          }
          
          const periodNumber = goalData.PeriodNumber ?? goalData.periodNumber ?? 1;
          const teamId = goalData.TeamId ?? goalData.teamId ?? '';
          const playerId = goalData.PlayerId ?? goalData.playerId ?? '';
          const isOvertime = goalData.IsOvertime ?? goalData.isOvertime ?? false;
          const isShootout = goalData.IsShootout ?? goalData.isShootout ?? false;
          const assisterId = goalData.AssisterId ?? goalData.assisterId;
          
          return {
            id: `goal-${teamId}-${playerId}-${periodNumber}-${timeInSeconds}`,
            type: 'goal' as const,
            teamId: teamId,
            teamName: teamId === currentMatch.homeTeamId ? (homeTeam?.name || 'Home') : (awayTeam?.name || 'Away'),
            playerId: playerId,
            playerName: getPlayerNameById(playerId),
            assisterId: assisterId,
            assisterName: assisterId ? getPlayerNameById(assisterId) : undefined,
            periodNumber: periodNumber,
            timeInSeconds: timeInSeconds,
            timestamp: new Date(event.occurredOn),
            wasInOvertime: isOvertime,
            wasInShootout: isShootout
          };
        } 
        // Handle penalty events
        else if (event.eventType === 'FloorballPenaltyAssignedEvent') {
          console.log('Processing FloorballPenaltyAssignedEvent');
          console.log('Raw penalty event data:', event.data);
          
          const penaltyData = event.data as {
            MatchId?: string;
            TeamId?: string;
            PlayerId?: string;
            PeriodNumber?: number;
            TimeInSeconds?: number;
            EventTime?: string;
            PenaltyType?: string;
            Minutes?: number;
            Description?: string;
            // Handle camelCase field names from JSON serialization
            matchId?: string;
            teamId?: string;
            playerId?: string;
            periodNumber?: number;
            timeInSeconds?: number;
            eventTime?: string;
            penaltyType?: string;
            minutes?: number;
            description?: string;
          };
          
          // Handle both PascalCase and camelCase field names for penalty
          let penaltyTimeInSeconds = penaltyData.TimeInSeconds ?? penaltyData.timeInSeconds ?? 0;
          
          // If TimeInSeconds is not available, try to parse EventTime
          if (penaltyTimeInSeconds === 0 && (penaltyData.EventTime || penaltyData.eventTime)) {
            const eventTime = penaltyData.EventTime ?? penaltyData.eventTime;
            if (typeof eventTime === 'string') {
              const timeParts = eventTime.split(':');
              if (timeParts.length === 2) {
                const minutes = parseInt(timeParts[0]) || 0;
                const seconds = parseInt(timeParts[1]) || 0;
                penaltyTimeInSeconds = minutes * 60 + seconds;
              } else if (timeParts.length === 3) {
                const hours = parseInt(timeParts[0]) || 0;
                const minutes = parseInt(timeParts[1]) || 0;
                const seconds = parseInt(timeParts[2]) || 0;
                penaltyTimeInSeconds = hours * 3600 + minutes * 60 + seconds;
              }
            }
          }
          
          const penaltyPeriodNumber = penaltyData.PeriodNumber ?? penaltyData.periodNumber ?? 1;
          const penaltyTeamId = penaltyData.TeamId ?? penaltyData.teamId ?? '';
          const penaltyPlayerId = penaltyData.PlayerId ?? penaltyData.playerId;
          const penaltyType = penaltyData.PenaltyType ?? penaltyData.penaltyType ?? '';
          const penaltyMinutes = penaltyData.Minutes ?? penaltyData.minutes ?? 0;
          const penaltyDescription = penaltyData.Description ?? penaltyData.description ?? '';
          
          return {
            id: `penalty-${penaltyTeamId}-${penaltyPlayerId || 'team'}-${penaltyPeriodNumber}-${penaltyTimeInSeconds}`,
            type: 'penalty' as const,
            teamId: penaltyTeamId,
            teamName: penaltyTeamId === currentMatch.homeTeamId ? (homeTeam?.name || 'Home') : (awayTeam?.name || 'Away'),
            playerId: penaltyPlayerId,
            playerName: penaltyPlayerId ? getPlayerNameById(penaltyPlayerId) : 'Team Penalty',
            periodNumber: penaltyPeriodNumber,
            timeInSeconds: penaltyTimeInSeconds,
            timestamp: new Date(event.occurredOn),
            penaltyType: penaltyType,
            penaltyMinutes: penaltyMinutes,
            description: penaltyDescription
          };
        }
        // Handle save events
        else if (event.eventType === 'FloorballSaveEvent') {
          console.log('Processing FloorballSaveEvent');
          interface AnySaveLike {
            MatchId?: string; matchId?: string;
            TeamId?: string; teamId?: string;
            GoalieId?: string; goalieId?: string;
            PeriodNumber?: number; periodNumber?: number;
            TimeInSeconds?: number; timeInSeconds?: number;
            IsOvertime?: boolean; wasInOvertime?: boolean;
            IsShootout?: boolean; wasInShootout?: boolean;
            GoalieName?: string; goalieName?: string;
          }
          const d = event.data as AnySaveLike;
          const saveData: SaveEventData = {
            MatchId: (d.MatchId ?? d.matchId) || '',
            TeamId: (d.TeamId ?? d.teamId) || '',
            GoalieId: (d.GoalieId ?? d.goalieId) || '',
            GoalieName: (d.GoalieName ?? d.goalieName) || '',
            PeriodNumber: (d.PeriodNumber ?? d.periodNumber) ?? 0,
            TimeInSeconds: (d.TimeInSeconds ?? d.timeInSeconds) ?? 0,
            IsOvertime: (d.IsOvertime ?? d.wasInOvertime) ?? false,
            IsShootout: (d.IsShootout ?? d.wasInShootout) ?? false
          };
          const { TeamId, GoalieId, GoalieName, PeriodNumber, TimeInSeconds, IsOvertime, IsShootout } = saveData;
          return {
            id: `save-${TeamId}-${GoalieId}-${PeriodNumber}-${TimeInSeconds}`,
            type: 'save' as const,
            teamId: TeamId,
            teamName: TeamId === currentMatch.homeTeamId ? homeTeam?.name || 'Home' : awayTeam?.name || 'Away',
            playerId: GoalieId,
            playerName: GoalieName || getPlayerNameById(GoalieId),
            periodNumber: PeriodNumber,
            timeInSeconds: TimeInSeconds,
            timestamp: new Date(event.occurredOn),
            wasInOvertime: IsOvertime,
            wasInShootout: IsShootout
          };
        }
        return null;
      })
      .filter((event): event is NonNullable<typeof event> => event !== null);

    console.log('Processed events array:', events);

    // Sort events by period number (descending), then by time in seconds (descending)
    // This shows the most recent events first
    const sortedEvents = events.sort((a, b) => {
      if (a.periodNumber !== b.periodNumber) {
        return b.periodNumber - a.periodNumber; // Most recent period first
      }
      return b.timeInSeconds - a.timeInSeconds; // Most recent time first
    });

    console.log('Final sorted events:', sortedEvents);
    return sortedEvents;
  }, [matchEvents, currentMatch.homeTeamId, homeTeam?.name, awayTeam?.name, getPlayerNameById]);

  return {
    matchEvents,
    setMatchEvents,
    allEvents,
    loadMatchEvents,
    handleGoalScored,
    handlePenaltyAssigned,
    handleSaveRecorded
  };
}; 