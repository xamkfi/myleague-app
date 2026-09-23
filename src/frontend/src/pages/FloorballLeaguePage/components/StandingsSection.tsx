import LeagueStanding from '../../../components/LeagueStanding/LeagueStanding';
import type { FloorballSeasonStatisticsSummaryDto } from '../../../api/floorball/floorballStatistics';

interface StandingsSectionProps {
  seasonSummary?: FloorballSeasonStatisticsSummaryDto | null;
  loading?: boolean;
  error?: string | null;
}

export default function StandingsSection({ seasonSummary, loading, error }: StandingsSectionProps) {
  return (
    <LeagueStanding 
      seasonSummary={seasonSummary}
      loading={loading}
      error={error}
    />
  );
}
