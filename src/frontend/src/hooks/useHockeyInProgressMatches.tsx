/**
 * Tracks live hockey matches so admin UI can show live dots.
 *
 * Live updates strategy:
 *   Hockey has no SignalR status event yet. The hook polls GET /HockeyMatch/live
 *   every 30s and keeps the rows with a live status (Warmup, InProgress, Intermission,
 *   Overtime, Shootout) — the same fallback interval floorball/football use when SignalR is down.
 *
 * Sharing strategy:
 *   `<InProgressHockeyMatchesProvider>` exposes one fetch to the navbar, seasons
 *   table, and tournaments table via AdminPageTemplate. Calling the hook outside
 *   the provider falls back to a self-contained poll.
 */

import { createContext, useCallback, useContext, useEffect, useRef, useState } from 'react';
import { hockeyMatchService } from '../api/hockey/hockeyMatchService';
import {
  isHockeyMatchLive,
  type HockeyLiveMatchDto,
} from '../types/hockey/hockeyTypes';

const POLL_INTERVAL_MS = 30_000;

export interface HockeyInProgressState {
  matches: HockeyLiveMatchDto[];
  totalCount: number;
  countByCompetitionId: Map<string, number>;
  countByCompetitionType: { season: number; tournament: number };
  loading: boolean;
  error: string | null;
}

const EMPTY_STATE: HockeyInProgressState = {
  matches: [],
  totalCount: 0,
  countByCompetitionId: new Map(),
  countByCompetitionType: { season: 0, tournament: 0 },
  loading: false,
  error: null,
};

export const InProgressHockeyMatchesContext = createContext<HockeyInProgressState | null>(null);

function isHockeyTournamentMatch(match: HockeyLiveMatchDto): boolean {
  return match.matchType === 'TournamentGroup' || match.matchType === 'TournamentPlayoff';
}

const buildState = (matches: HockeyLiveMatchDto[]): HockeyInProgressState => {
  const countByCompetitionId: Map<string, number> = new Map();
  let seasonCount = 0;
  let tournamentCount = 0;

  for (const match of matches) {
    if (match.competitionId) {
      countByCompetitionId.set(
        match.competitionId,
        (countByCompetitionId.get(match.competitionId) ?? 0) + 1,
      );
    }

    if (isHockeyTournamentMatch(match)) {
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

export const useHockeyInProgressMatchesController = (active: boolean): HockeyInProgressState => {
  const [state, setState] = useState<HockeyInProgressState>(
    active ? { ...EMPTY_STATE, loading: true } : EMPTY_STATE,
  );
  const isMountedRef = useRef(true);

  const fetchLive = useCallback(async (): Promise<void> => {
    let rows: HockeyLiveMatchDto[];
    try {
      rows = await hockeyMatchService.getLive();
    } catch {
      if (isMountedRef.current) {
        setState((prev) => ({
          ...prev,
          loading: false,
          error: prev.error ?? 'Failed to load live hockey matches',
        }));
      }
      return;
    }

    if (!isMountedRef.current) {
      return;
    }

    setState(buildState(rows.filter((row) => isHockeyMatchLive(row.status))));
  }, []);

  useEffect(() => {
    if (!active) {
      return;
    }

    isMountedRef.current = true;
    void fetchLive();
    const timer = window.setInterval(() => {
      void fetchLive();
    }, POLL_INTERVAL_MS);

    return () => {
      isMountedRef.current = false;
      window.clearInterval(timer);
    };
  }, [active, fetchLive]);

  return state;
};

export function useHockeyInProgressMatches(): HockeyInProgressState {
  const fromContext = useContext(InProgressHockeyMatchesContext);
  const standalone = useHockeyInProgressMatchesController(fromContext === null);
  return fromContext ?? standalone;
}
