import { useCallback, useEffect, useMemo, useState } from 'react';
import { loadAllHockeyMatches } from '../api/hockey/loadAllHockeyMatches';
import { isHockeyMatchLive, type HockeyMatchDto } from '../types/hockey/hockeyTypes';

interface HockeyInProgressState {
  matches: HockeyMatchDto[];
  totalCount: number;
  countByCompetitionId: Map<string, number>;
}

export function useHockeyInProgressMatches(): HockeyInProgressState {
  const [matches, setMatches] = useState<HockeyMatchDto[]>([]);

  const refresh = useCallback(async (): Promise<void> => {
    try {
      setMatches((await loadAllHockeyMatches()).filter((match) => isHockeyMatchLive(match.status)));
    } catch {
      setMatches([]);
    }
  }, []);

  useEffect(() => {
    void refresh();
    const timer = window.setInterval(() => {
      void refresh();
    }, 5000);
    return () => window.clearInterval(timer);
  }, [refresh]);

  const countByCompetitionId = useMemo(() => {
    const counts = new Map<string, number>();
    for (const match of matches) {
      if (!match.competitionId) {
        continue;
      }
      counts.set(match.competitionId, (counts.get(match.competitionId) ?? 0) + 1);
    }
    return counts;
  }, [matches]);

  return {
    matches,
    totalCount: matches.length,
    countByCompetitionId,
  };
}
