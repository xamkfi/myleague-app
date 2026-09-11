import type { FloorballPlayerDto } from '../../../../../api/floorball/floorballPlayerService';

/**
 * Sorts players for the goal/penalty/save selectors. Players are ordered by jersey number
 * (ascending, with missing jerseys pushed to the bottom) and ties are broken alphabetically
 * by full name. Returns a new array so callers can pass an immutable input safely.
 */
export const sortPlayersForSelect = (players: readonly FloorballPlayerDto[]): FloorballPlayerDto[] => {
  return [...players].sort((a, b) => {
    const aNumber: number = a.jerseyNumber ?? Number.POSITIVE_INFINITY;
    const bNumber: number = b.jerseyNumber ?? Number.POSITIVE_INFINITY;
    if (aNumber !== bNumber) {
      return aNumber - bNumber;
    }
    const aName: string = `${a.person.firstName} ${a.person.lastName}`.toLowerCase();
    const bName: string = `${b.person.firstName} ${b.person.lastName}`.toLowerCase();
    return aName.localeCompare(bName);
  });
};

/**
 * Formats a player as `"#NN - First Last"` for use as a dropdown option label. `??` is shown
 * when the player is missing a jersey number; this surfaces the data issue to the recorder
 * without preventing selection, while the parent forms still block submission until a jersey
 * is assigned (see the `missingJersey` check).
 */
export const formatPlayerOptionLabel = (player: FloorballPlayerDto): string => {
  const jersey: string = player.jerseyNumber !== undefined ? `#${player.jerseyNumber}` : '#??';
  return `${jersey} - ${player.person.firstName} ${player.person.lastName}`;
};
