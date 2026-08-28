import TournamentGroupStandingsTable from '../../../components/TournamentGroupStandingsTable/TournamentGroupStandingsTable';
import { footballStatisticsService } from '../../../api/football/footballStatistics';

interface FootballTournamentGroupStandingsTableProps {
  groupId: string;
  groupName: string;
  teamsAdvancingPerGroup?: number;
  hideHeader?: boolean;
}

export default function FootballTournamentGroupStandingsTable(props: FootballTournamentGroupStandingsTableProps) {
  return (
    <TournamentGroupStandingsTable
      {...props}
      sport="football"
      loadStandings={(groupId) => footballStatisticsService.getTournamentGroupStandings(groupId)}
    />
  );
}
