import type { HockeyMatchActivePlayerDto } from '../../../../../types/hockey/hockeyTypes';

export interface HockeyFormPlayer {
  id: string;
  jerseyNumber: number | undefined;
  name: string;
}

export const sortPlayersForSelect = (players: readonly HockeyFormPlayer[]): HockeyFormPlayer[] => {
  return [...players].sort((a, b) => {
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

export const toFormPlayers = (
  players: HockeyMatchActivePlayerDto[],
  names: Map<string, string>,
): HockeyFormPlayer[] => {
  return players.map((player) => ({
    id: player.id,
    jerseyNumber: player.jerseyNumber,
    name: names.get(player.teamPlayerId) ?? `#${player.jerseyNumber}`,
  }));
};
