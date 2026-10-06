import type { HockeyMatchActivePlayerDto } from '../../../../../types/hockey/hockeyTypes';

export interface HockeyFormPlayer {
  id: string;
  jerseyNumber: number | undefined;
  name: string;
  isGoalie?: boolean;
  position?: string;
}

const isGoaliePlayer = (player: HockeyFormPlayer): boolean =>
  Boolean(player.isGoalie) || player.position === 'Goalie';

export const sortPlayersForSelect = (players: readonly HockeyFormPlayer[]): HockeyFormPlayer[] => {
  return [...players].sort((a, b) => {
    const aGoalie = isGoaliePlayer(a);
    const bGoalie = isGoaliePlayer(b);
    if (aGoalie !== bGoalie) {
      return aGoalie ? -1 : 1;
    }
    const aNumber = a.jerseyNumber ?? Number.POSITIVE_INFINITY;
    const bNumber = b.jerseyNumber ?? Number.POSITIVE_INFINITY;
    if (aNumber !== bNumber) {
      return aNumber - bNumber;
    }
    return a.name.localeCompare(b.name);
  });
};

export const formatPlayerOptionLabel = (player: HockeyFormPlayer): string => {
  const jersey = player.jerseyNumber !== undefined ? `#${player.jerseyNumber}` : '#??';
  return `${jersey} - ${player.name}`;
};

/**
 * Periods a scorekeeper can attach an event to: every period already started, up to and
 * including the live period. Overtime and the shootout stay out until they are the live
 * period or have been started. The live period is always included so the form default
 * matches a real option.
 */
export function hockeyRecordablePeriods(
  startedPeriods: ReadonlySet<number>,
  currentPeriod: number,
): number[] {
  const periods = new Set<number>();
  for (const period of startedPeriods) {
    if (period >= 1 && period <= currentPeriod) {
      periods.add(period);
    }
  }
  if (currentPeriod >= 1) {
    periods.add(currentPeriod);
  }
  return [...periods].sort((left, right) => left - right);
}

export const toFormPlayers = (
  players: HockeyMatchActivePlayerDto[],
  names: Map<string, string>,
): HockeyFormPlayer[] => {
  return players.map((player) => ({
    id: player.id,
    jerseyNumber: player.jerseyNumber,
    name: names.get(player.teamPlayerId) ?? `#${player.jerseyNumber}`,
    isGoalie: player.isGoalie || player.position === 'Goalie',
    position: player.position,
  }));
};
