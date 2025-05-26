import { useState, useEffect } from 'react';
import { getNextMatchApi, getStandingsApi, getTeamStatsApi } from '../api/matchService';
import type { Match, StandingsRow, TeamStat } from '../types/league.types';

interface MatchData {
  match: Match | null;
  standings: StandingsRow[];
  teamStats: TeamStat[];
  loading: boolean;
  error: string | null;
}

export function useMatchData(): MatchData {
  const [match, setMatch] = useState<Match | null>(null);
  const [standings, setStandings] = useState<StandingsRow[]>([]);
  const [teamStats, setTeamStats] = useState<TeamStat[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        // Fetch all data in parallel
        const [matchData, standingsData, statsData] = await Promise.all([
          getNextMatchApi(),
          getStandingsApi(),
          getTeamStatsApi()
        ]);

        setMatch(matchData);
        setStandings(standingsData);
        setTeamStats(statsData);
        setError(null);
      } catch (err) {
        setError('Failed to fetch match data');
        console.error('Error fetching match data:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, []);

  return { match, standings, teamStats, loading, error };
} 