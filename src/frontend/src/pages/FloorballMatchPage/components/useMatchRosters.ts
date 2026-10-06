import { useEffect, useState } from 'react';
import { floorballTeamService } from '../../../api/floorball/floorballTeamService';
import type { FloorballMatchDto, FloorballTeamPlayer } from '../../../types/floorball/floorballTypes';

export interface MatchRosters {
  home: FloorballTeamPlayer[];
  away: FloorballTeamPlayer[];
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
export function useMatchRosters(match: FloorballMatchDto, enabled: boolean): MatchRosters {
  const { homeTeamId, awayTeamId, competitionId } = match;
  const key: string | null = homeTeamId && awayTeamId ? `${homeTeamId}|${awayTeamId}|${competitionId}` : null;
  const [loaded, setLoaded] = useState<LoadedRosters | null>(null);
  const loadedKey: string | null = loaded?.key ?? null;

  useEffect(() => {
    if (!enabled || !key || !homeTeamId || !awayTeamId || loadedKey === key) return;

    let cancelled: boolean = false;
    Promise.all([
      floorballTeamService.getById(homeTeamId, competitionId),
      floorballTeamService.getById(awayTeamId, competitionId),
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
  }, [enabled, key, loadedKey, homeTeamId, awayTeamId, competitionId]);

  return loaded && loaded.key === key ? loaded.rosters : EMPTY_ROSTERS;
}
