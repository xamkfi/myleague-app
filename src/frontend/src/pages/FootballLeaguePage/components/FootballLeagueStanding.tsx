import type { ReactNode } from 'react';
import type { FootballSeasonStatisticsSummaryDto } from '../../../api/football/footballStatistics';
import LeagueStanding from '../../../components/LeagueStanding/LeagueStanding';

interface FootballLeagueStandingProps {
  seasonSummary?: FootballSeasonStatisticsSummaryDto | null;
  loading?: boolean;
  error?: string | null;
  standingsOverride?: ReactNode;
  titleOverride?: string;
}

export default function FootballLeagueStanding({
  seasonSummary,
  loading,
  error,
  standingsOverride,
  titleOverride,
}: FootballLeagueStandingProps) {
  return (
    <LeagueStanding
      sport="football"
      seasonSummary={seasonSummary}
      loading={loading}
      error={error}
      standingsOverride={standingsOverride}
      titleOverride={titleOverride}
    />
  );
}
