import SharedRosterSection from '../../../components/RosterSection/RosterSection';
import type { FootballTeam } from '../../../types/football/footballTypes';
import type { FootballPlayerSeasonStatisticsDto } from '../../../api/football/footballStatistics';

interface RosterSectionProps {
  team: FootballTeam;
  playerStatistics?: FootballPlayerSeasonStatisticsDto[] | null;
}

export default function RosterSection({ team, playerStatistics }: RosterSectionProps) {
  return (
    <SharedRosterSection
      sport="football"
      players={team.roster}
      playerStatistics={playerStatistics}
      positionOrder={['Goalkeeper', 'Defender', 'Midfielder', 'Forward']}
    />
  );
}
