import FootballLeagueStanding from './FootballLeagueStanding';
import type { FootballSeasonStatisticsSummaryDto } from '../../../api/football/footballStatistics';

interface StandingsSectionProps {
  seasonSummary?: FootballSeasonStatisticsSummaryDto | null;
  loading?: boolean;
  error?: string | null;
}

export default function StandingsSection({ seasonSummary, loading, error }: StandingsSectionProps) {
  return (
    <FootballLeagueStanding 
      seasonSummary={seasonSummary}
      loading={loading}
      error={error}
    />
  );
}
