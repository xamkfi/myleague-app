import SharedRosterSection, { type RosterStatRow } from '../../../../components/RosterSection/RosterSection';
import type { FloorballTeam } from '../../../../types/floorball/floorballTypes';
import type {
  FloorballGoalieSeasonStatisticsDto,
  FloorballPlayerSeasonStatisticsDto,
} from '../../../../api/floorball/floorballStatistics';

interface RosterSectionProps {
  team: FloorballTeam;
  playerStatistics?: FloorballPlayerSeasonStatisticsDto[] | null;
  goalieStatistics?: FloorballGoalieSeasonStatisticsDto[] | null;
}

/** Player rows carry games, goals and assists; goalie rows add save percentage and GAA on top. */
function buildRosterStats(
  playerStatistics: FloorballPlayerSeasonStatisticsDto[] | null | undefined,
  goalieStatistics: FloorballGoalieSeasonStatisticsDto[] | null | undefined,
): RosterStatRow[] | null {
  if (!playerStatistics && !goalieStatistics) {
    return null;
  }
  const rows = new Map<string, RosterStatRow>(
    (playerStatistics ?? []).map((row) => [
      row.playerId,
      { playerId: row.playerId, gamesPlayed: row.gamesPlayed, goals: row.goals, assists: row.assists },
    ]),
  );
  for (const row of goalieStatistics ?? []) {
    const existing = rows.get(row.playerId);
    rows.set(row.playerId, {
      playerId: row.playerId,
      gamesPlayed: row.gamesPlayed > 0 ? row.gamesPlayed : (existing?.gamesPlayed ?? 0),
      goals: existing?.goals ?? 0,
      assists: existing?.assists ?? 0,
      savePercentage: row.savePercentage,
      goalsAgainstAverage: row.goalsAgainstAverage,
    });
  }
  return [...rows.values()];
}

export default function RosterSection({ team, playerStatistics, goalieStatistics }: RosterSectionProps) {
  return (
    <SharedRosterSection
      sport="floorball"
      players={team.roster.map((player) => ({ ...player, licenceActive: player.isActive }))}
      playerStatistics={buildRosterStats(playerStatistics, goalieStatistics)}
      positionOrder={['Goalkeeper', 'Defender', 'Center', 'Forward']}
      goaliePositions={['Goalkeeper']}
    />
  );
}
