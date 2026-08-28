import SharedRosterSection from '../../../components/RosterSection/RosterSection';
import type { FloorballTeam } from '../../../types/floorball/floorballTypes';
import type { FloorballPlayerSeasonStatisticsDto } from '../../../api/floorball/floorballStatistics';

interface RosterSectionProps {
  team: FloorballTeam;
  playerStatistics?: FloorballPlayerSeasonStatisticsDto[] | null;
}

export default function RosterSection({ team, playerStatistics }: RosterSectionProps) {
  return (
    <SharedRosterSection
      sport="floorball"
      players={team.roster}
      playerStatistics={playerStatistics}
      positionOrder={['Goalkeeper', 'Defender', 'Center', 'Forward']}
    />
  );
}
