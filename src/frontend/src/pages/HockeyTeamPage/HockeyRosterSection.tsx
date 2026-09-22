import { useTranslation } from 'react-i18next';
import SharedRosterSection from '../../components/RosterSection/RosterSection';
import {
  HOCKEY_POSITIONS,
  type HockeyPlayerCompetitionStatisticsDto,
  type HockeyTeamDto,
} from '../../types/hockey/hockeyTypes';

interface HockeyRosterSectionProps {
  team: HockeyTeamDto;
  playerNames: Map<string, string>;
  playerStats: HockeyPlayerCompetitionStatisticsDto[];
}

function HockeyRosterSection({ team, playerNames, playerStats }: HockeyRosterSectionProps) {
  const { t } = useTranslation();
  const rosterByPlayer = new Map<string, (typeof team.roster)[number]>();
  for (const row of team.roster) {
    const current = rosterByPlayer.get(row.playerId);
    if (!current || (row.isActive && !current.isActive)) {
      rosterByPlayer.set(row.playerId, row);
    }
  }

  return (
    <SharedRosterSection
      sport="hockey"
      players={[...rosterByPlayer.values()].map((row) => ({
        playerId: row.playerId,
        playerName: playerNames.get(row.playerId) ?? row.playerId.slice(0, 8),
        position: row.position,
        jerseyNumber: row.jerseyNumber,
        nameSuffix:
          row.captainRole === 'Captain'
            ? ' (C)'
            : row.captainRole === 'AlternateCaptain'
              ? ' (A)'
              : undefined,
      }))}
      playerStatistics={playerStats}
      positionOrder={[...HOCKEY_POSITIONS]}
      positionLabel={(position) => t(`hockey.positions.${position}`, position)}
    />
  );
}

export default HockeyRosterSection;
