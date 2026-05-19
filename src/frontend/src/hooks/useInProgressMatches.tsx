/**
 * useInProgressMatches
 * --------------------
 * Tracks the set of floorball matches currently in `InProgress` status so the admin UI can
 * surface "live now" indicators (red dots / count badges) on the sidebar and listing rows.
 *
 * Live updates strategy:
 *   Subscribes to the existing `FloorballMatchStatusChangedEvent` SignalR event (the same one
 *   that `MatchManagementPage` already uses) and refetches the in-progress list when the event
 *   fires. The fetch is debounced by ~250 ms so a burst of status changes coalesces into a
 *   single network request. If the SignalR connection cannot be established the hook silently
 *   falls back to a 30 s polling interval — the dots may be stale by up to 30 s in that case but
 *   the UI never crashes.
 *
 * Sharing strategy:
 *   `<InProgressMatchesProvider>` (in `./InProgressMatchesProvider.tsx`) exposes the same state
 *   to multiple consumers via React context, so the sidebar, seasons page, and tournaments page
 *   can all render off a single fetch when mounted under the provider (which they are, via
 *   `AdminPageTemplate`). Calling `useInProgressMatches()` outside the provider falls back to a
 *   self-contained fetch loop so the hook is still usable in isolation.
 *
 * Page size note:
 *   We request `pageSize: 100` which is generous for "matches currently in play". If the backend
 *   ever exceeds this in real life, the badge counts on individual rows still reflect the first
 *   100 active matches; extending pagination here can be done lazily.
 */

import { createContext, useCallback, useContext, useEffect, useRef, useState } from 'react';
import { floorballMatchService } from '../api/floorball/floorballMatchService';
import {
  FloorballMatchStatus,
  type FloorballMatchDto,
} from '../types/floorball/floorballTypes';
import { isTournamentCompetition } from '../utils/competitionPath';
import { signalRService, type MatchEvent } from '../services/signalRService';

const POLL_INTERVAL_MS = 30_000;
const REFRESH_DEBOUNCE_MS = 250;
const STATUS_CHANGED_EVENT = 'FloorballMatchStatusChangedEvent';

export interface InProgressMatchesState {
  matches: FloorballMatchDto[];
  totalCount: number;
  countByCompetitionId: Map<string, number>;
  countByCompetitionType: { season: number; tournament: number };
  loading: boolean;
  error: string | null;
}

const EMPTY_STATE: InProgressMatchesState = {
  matches: [],
  totalCount: 0,
  countByCompetitionId: new Map(),
  countByCompetitionType: { season: 0, tournament: 0 },
  loading: false,
  error: null,
};

export const InProgressMatchesContext = createContext<InProgressMatchesState | null>(null);

const buildState = (matches: FloorballMatchDto[]): InProgressMatchesState => {
  const countByCompetitionId: Map<string, number> = new Map();
  let seasonCount: number = 0;
  let tournamentCount: number = 0;

  for (const match of matches) {
    if (match.competitionId) {
      countByCompetitionId.set(
        match.competitionId,
        (countByCompetitionId.get(match.competitionId) ?? 0) + 1,
      );
    }

    if (isTournamentCompetition(match)) {
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

/**
 * Internal controller that performs the actual fetch / SignalR subscription / polling fallback.
 * `active` lets a consumer opt out of the side effects (used by the standalone hook when a
 * provider is already in scope so the hook still satisfies the rules-of-hooks but does no
 * duplicate work).
 *
 * Exported for `InProgressMatchesProvider` to consume; not intended for direct use elsewhere.
 */
export const useInProgressMatchesController = (active: boolean): InProgressMatchesState => {
  const [state, setState] = useState<InProgressMatchesState>(
    active ? { ...EMPTY_STATE, loading: true } : EMPTY_STATE,
  );

  const debounceTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const isMountedRef = useRef<boolean>(true);

  const fetchInProgress = useCallback(async (): Promise<void> => {
    try {
      const response = await floorballMatchService.getAll({
        status: FloorballMatchStatus.InProgress,
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

      const message: string = err instanceof Error ? err.message : 'Failed to load in-progress matches';
      console.error('useInProgressMatches: fetch failed', err);
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

    const scheduleRefresh = (): void => {
      if (debounceTimerRef.current) {
        clearTimeout(debounceTimerRef.current);
      }
      debounceTimerRef.current = setTimeout(() => {
        void fetchInProgress();
      }, REFRESH_DEBOUNCE_MS);
    };

    const handleSignalREvent = (event: MatchEvent): void => {
      if (event.eventType === STATUS_CHANGED_EVENT) {
        scheduleRefresh();
      }
    };

    const setupLiveUpdates = async (): Promise<void> => {
      try {
        await signalRService.connect();
        if (!signalRService.isConnected || !isMountedRef.current) {
          throw new Error('SignalR not connected');
        }

        await signalRService.subscribeToEventType(STATUS_CHANGED_EVENT);
        signalRUnsubscribe = signalRService.onMatchEvent(handleSignalREvent);
        signalRConnected = true;
      } catch (err) {
        console.warn('useInProgressMatches: SignalR unavailable, falling back to polling', err);
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
        void signalRService.unsubscribeFromEventType(STATUS_CHANGED_EVENT);
      }

      if (pollIntervalId !== null) {
        clearInterval(pollIntervalId);
      }
    };
  }, [active, fetchInProgress]);

  return state;
};

export const useInProgressMatches = (): InProgressMatchesState => {
  const fromContext: InProgressMatchesState | null = useContext(InProgressMatchesContext);
  const standalone: InProgressMatchesState = useInProgressMatchesController(fromContext === null);

  return fromContext ?? standalone;
};
