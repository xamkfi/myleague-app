import { useEffect, useState } from 'react';
import { footballTeamService } from '../../../../api/football/footballTeamService';
import type { FootballMatchDto, FootballTeamPlayer } from '../../../../types/football/footballTypes';

export interface MatchRosters {
  home: FootballTeamPlayer[];
  away: FootballTeamPlayer[];
}

interface LoadedRosters {
  key: string;
  rosters: MatchRosters;
}

const EMPTY_ROSTERS: MatchRosters = { home: [], away: [] };

/**
 * Loads both team rosters once per match and keeps them while the user switches tabs.
 * Nothing is fetched until `enabled` is true or when either team is still a placeholder.
 */
export function useMatchRosters(match: FootballMatchDto, enabled: boolean): MatchRosters {
  const { homeTeamId, awayTeamId } = match;
  const key: string | null = homeTeamId && awayTeamId ? `${homeTeamId}|${awayTeamId}` : null;
  const [loaded, setLoaded] = useState<LoadedRosters | null>(null);
  const loadedKey: string | null = loaded?.key ?? null;

  useEffect(() => {
    if (!enabled || !key || !homeTeamId || !awayTeamId || loadedKey === key) return;

    let cancelled: boolean = false;
    Promise.all([
      footballTeamService.getById(homeTeamId),
      footballTeamService.getById(awayTeamId),
    ])
      .then(([homeResponse, awayResponse]) => {
        if (cancelled) return;
        setLoaded({
          key,
          rosters: { home: homeResponse.roster ?? [], away: awayResponse.roster ?? [] },
        });
      })
      .catch((err: unknown) => {
        console.error('Failed to load team rosters for match:', err);
      });

    return () => {
      cancelled = true;
    };
  }, [enabled, key, loadedKey, homeTeamId, awayTeamId]);

  return loaded && loaded.key === key ? loaded.rosters : EMPTY_ROSTERS;
}
