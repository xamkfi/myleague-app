/**
 * Tracks football matches currently in `InProgress` so admin UI can show live dots.
 * Separate from the floorball hook so floorball counts stay correct.
 */

import { createContext, useCallback, useContext, useEffect, useRef, useState } from 'react';
import { footballMatchService } from '../api/football/footballMatchService';
import {
  FootballMatchStatus,
  type FootballMatchDto,
} from '../types/football/footballTypes';
import { isFootballTournamentCompetition } from '../utils/footballCompetitionPath';
import { signalRService, type MatchEvent } from '../services/signalRService';
import { FOOTBALL_MATCH_NOTIFICATION_EVENTS } from '../constants/FootballMatchNotifications';

const POLL_INTERVAL_MS = 30_000;
const REFRESH_DEBOUNCE_MS = 250;

const STATUS_EVENTS: string[] = [
  FOOTBALL_MATCH_NOTIFICATION_EVENTS.MATCH_STARTED,
  FOOTBALL_MATCH_NOTIFICATION_EVENTS.MATCH_COMPLETED,
  FOOTBALL_MATCH_NOTIFICATION_EVENTS.MATCH_REOPENED,
  FOOTBALL_MATCH_NOTIFICATION_EVENTS.MATCH_CREATED,
];

export interface InProgressFootballMatchesState {
  matches: FootballMatchDto[];
  totalCount: number;
  countByCompetitionId: Map<string, number>;
  countByCompetitionType: { season: number; tournament: number };
  loading: boolean;
  error: string | null;
}

const EMPTY_STATE: InProgressFootballMatchesState = {
  matches: [],
  totalCount: 0,
  countByCompetitionId: new Map(),
  countByCompetitionType: { season: 0, tournament: 0 },
  loading: false,
  error: null,
};

export const InProgressFootballMatchesContext = createContext<InProgressFootballMatchesState | null>(null);

const buildState = (matches: FootballMatchDto[]): InProgressFootballMatchesState => {
  const countByCompetitionId: Map<string, number> = new Map();
  let seasonCount: number = 0;
  let tournamentCount: number = 0;

  for (const match of matches) {
    if (match.competitionId) {
      countByCompetitionId.set(
        match.competitionId,
        (countByCompetitionId.get(match.competitionId) ?? 0) + 1
      );
    }

    if (isFootballTournamentCompetition(match)) {
      tournamentCount += 1;
    } else {
      seasonCount += 1;
    }
  }

  return {
    matches,
    totalCount: matches.length,
    countByCompetitionId,
    countByCompetitionType: { season: seasonCount, tournament: tournamentCount },
    loading: false,
    error: null,
  };
};

export const useInProgressFootballMatchesController = (active: boolean): InProgressFootballMatchesState => {
  const [state, setState] = useState<InProgressFootballMatchesState>(
    active ? { ...EMPTY_STATE, loading: true } : EMPTY_STATE
  );

  const debounceTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const isMountedRef = useRef<boolean>(true);

  const fetchInProgress = useCallback(async (): Promise<void> => {
    try {
      const response = await footballMatchService.getAll({
        status: FootballMatchStatus.InProgress,
        page: 1,
        pageSize: 100,
      });

      if (!isMountedRef.current) {
        return;
      }

      setState(buildState(response.data ?? []));
    } catch (err) {
      if (!isMountedRef.current) {
        return;
      }

      const message: string = err instanceof Error ? err.message : 'Failed to load in-progress football matches';
      console.error('useInProgressFootballMatches: fetch failed', err);
      setState((prev) => ({
        ...prev,
        loading: false,
        error: message,
      }));
    }
  }, []);

  useEffect(() => {
    if (!active) {
      return;
    }

    isMountedRef.current = true;
    void fetchInProgress();

    let signalRUnsubscribe: (() => void) | undefined;
    let pollIntervalId: ReturnType<typeof setInterval> | null = null;
    let signalRConnected: boolean = false;
    const subscribedEvents: string[] = [];

    const scheduleRefresh = (): void => {
      if (debounceTimerRef.current) {
        clearTimeout(debounceTimerRef.current);
      }
      debounceTimerRef.current = setTimeout(() => {
        void fetchInProgress();
      }, REFRESH_DEBOUNCE_MS);
    };

    const handleSignalREvent = (event: MatchEvent): void => {
      if (STATUS_EVENTS.includes(event.eventType)) {
        scheduleRefresh();
      }
    };

    const setupLiveUpdates = async (): Promise<void> => {
      try {
        await signalRService.connect();
        if (!signalRService.isConnected || !isMountedRef.current) {
          throw new Error('SignalR not connected');
        }

        for (const eventName of STATUS_EVENTS) {
          await signalRService.subscribeToEventType(eventName);
          subscribedEvents.push(eventName);
        }
        signalRUnsubscribe = signalRService.onMatchEvent(handleSignalREvent);
        signalRConnected = true;
      } catch (err) {
        console.warn('useInProgressFootballMatches: SignalR unavailable, falling back to polling', err);
        pollIntervalId = setInterval(() => {
          void fetchInProgress();
        }, POLL_INTERVAL_MS);
      }
    };

    void setupLiveUpdates();

    return () => {
      isMountedRef.current = false;

      if (debounceTimerRef.current) {
        clearTimeout(debounceTimerRef.current);
        debounceTimerRef.current = null;
      }

      if (signalRUnsubscribe) {
        signalRUnsubscribe();
      }

      if (signalRConnected) {
        for (const eventName of subscribedEvents) {
          void signalRService.unsubscribeFromEventType(eventName);
        }
      }

      if (pollIntervalId !== null) {
        clearInterval(pollIntervalId);
      }
    };
  }, [active, fetchInProgress]);

  return state;
};

export const useInProgressFootballMatches = (): InProgressFootballMatchesState => {
  const fromContext: InProgressFootballMatchesState | null = useContext(InProgressFootballMatchesContext);
  const standalone: InProgressFootballMatchesState = useInProgressFootballMatchesController(fromContext === null);

  return fromContext ?? standalone;
};
