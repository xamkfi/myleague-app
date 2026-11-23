import { useTranslation } from 'react-i18next';
import SharedRosterSection from '../../../../components/RosterSection/RosterSection';
import type { FootballTeam } from '../../../../types/football/footballTypes';
import type { FootballPlayerSeasonStatisticsDto } from '../../../../api/football/footballStatistics';

interface RosterSectionProps {
  team: FootballTeam;
  playerStatistics?: FootballPlayerSeasonStatisticsDto[] | null;
}

export default function RosterSection({ team, playerStatistics }: RosterSectionProps) {
  const { t } = useTranslation();

  return (
    <SharedRosterSection
      sport="football"
      players={team.roster.map((player) => ({ ...player, licenceActive: player.isActive }))}
      playerStatistics={playerStatistics}
      positionOrder={['Goalkeeper', 'Defender', 'Midfielder', 'Forward']}
      goaliePositions={['Goalkeeper']}
      fieldColumns={['goals', 'yellowCards']}
      goalieColumns={['goals', 'cleanSheets']}
      positionLabel={(position) => t(`football.positions.${position}`, position)}
    />
  );
}
