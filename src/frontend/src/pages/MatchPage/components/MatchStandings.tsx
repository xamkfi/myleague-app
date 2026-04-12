import { useEffect, useState } from 'react';
import { floorballStatisticsService, type FloorballSeasonStatisticsSummaryDto } from '../../../api/floorball/floorballStatistics';
import type { FloorballMatchDto } from '../../../types/floorball/floorballTypes';
import LeagueStanding from '../../../components/LeagueStanding/LeagueStanding';

interface MatchStandingsProps {
  match: FloorballMatchDto;
}

export default function MatchStandings({ match }: MatchStandingsProps) {
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [seasonStats, setSeasonStats] = useState<FloorballSeasonStatisticsSummaryDto | null>(null);

  useEffect(() => {
    const fetchSeasonStats = async () => {
      try {
        setIsLoading(true);
        setError(null);
        const data = await floorballStatisticsService.getSeasonStatistics(match.competitionId);
        setSeasonStats(data);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to load season statistics');
      } finally {
        setIsLoading(false);
      }
    };

    fetchSeasonStats();
  }, [match.competitionId]);

  return (
    <div className="match-standings">
      <LeagueStanding 
        seasonSummary={seasonStats}
        loading={isLoading}
        error={error}
      />
    </div>
  );
}
