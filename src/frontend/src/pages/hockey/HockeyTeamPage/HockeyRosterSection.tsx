import { useTranslation } from 'react-i18next';
import SharedRosterSection, { type RosterStatRow } from '../../../components/RosterSection/RosterSection';
import {
  HOCKEY_POSITIONS,
  hockeyLicenceActive,
  type HockeyGoalieCompetitionStatisticsDto,
  type HockeyPlayerCompetitionStatisticsDto,
  type HockeyTeamDto,
} from '../../../types/hockey/hockeyTypes';

interface HockeyRosterSectionProps {
  team: HockeyTeamDto;
  playerNames: Map<string, string>;
  playerStats: HockeyPlayerCompetitionStatisticsDto[];
  goalieStats: HockeyGoalieCompetitionStatisticsDto[];
}

/** Skater rows come from the player statistics; goalies only have goalie statistics. */
function buildRosterStats(
  playerStats: HockeyPlayerCompetitionStatisticsDto[],
  goalieStats: HockeyGoalieCompetitionStatisticsDto[],
): RosterStatRow[] {
  const rows = new Map<string, RosterStatRow>(
    playerStats.map((row) => [
      row.playerId,
      { playerId: row.playerId, gamesPlayed: row.gamesPlayed, goals: row.goals, assists: row.assists },
    ]),
  );
  for (const row of goalieStats) {
    const existing = rows.get(row.playerId);
    rows.set(row.playerId, {
      playerId: row.playerId,
      gamesPlayed: existing?.gamesPlayed ?? row.gamesPlayed,
      goals: existing?.goals ?? 0,
      assists: existing?.assists ?? 0,
      savePercentage: row.savePercentage,
      goalsAgainstAverage: row.goalsAgainstAverage,
    });
  }
  return [...rows.values()];
}

function HockeyRosterSection({ team, playerNames, playerStats, goalieStats }: HockeyRosterSectionProps) {
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
        licenceActive: hockeyLicenceActive(row),
        nameSuffix:
          row.captainRole === 'Captain'
            ? ' (C)'
            : row.captainRole === 'AlternateCaptain'
              ? ' (A)'
              : undefined,
      }))}
      playerStatistics={buildRosterStats(playerStats, goalieStats)}
      positionOrder={[...HOCKEY_POSITIONS]}
      goaliePositions={['Goalie']}
      positionLabel={(position) => t(`hockey.positions.${position}`, position)}
    />
  );
}

export default HockeyRosterSection;
