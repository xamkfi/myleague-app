export const JERSEY_NUMBER_MIN = 1;
export const JERSEY_NUMBER_MAX = 99;

export const JERSEY_NUMBER_OPTIONS: readonly number[] = Array.from(
  { length: JERSEY_NUMBER_MAX - JERSEY_NUMBER_MIN + 1 },
  (_, index) => index + JERSEY_NUMBER_MIN,
);

export type JerseyNumberSport = 'floorball' | 'football' | 'hockey';

export function toJerseyNumberSport(sport: string | undefined | null): JerseyNumberSport | null {
  const key = sport?.trim().toLowerCase();
  if (key === 'floorball') {
    return 'floorball';
  }
  if (key === 'football') {
    return 'football';
  }
  if (key === 'hockey' || key === 'icehockey') {
    return 'hockey';
  }
  return null;
}

export function collectJerseyNumbers(
  roster: Array<{ jerseyNumber?: number | null }>,
): number[] {
  const numbers: number[] = [];
  for (const player of roster) {
    if (typeof player.jerseyNumber === 'number') {
      numbers.push(player.jerseyNumber);
    }
  }
  return numbers;
}
