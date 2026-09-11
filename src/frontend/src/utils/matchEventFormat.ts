/**
 * Formats the elapsed seconds within a period as MM:SS.
 *
 * Returns "00:00" for missing/invalid values so consumers always render a
 * stable string.
 */
export function formatEventTimeMmSs(timeInSeconds: number | null | undefined): string {
  if (timeInSeconds === undefined || timeInSeconds === null || Number.isNaN(timeInSeconds)) {
    return '00:00';
  }
  const safe: number = Math.max(0, Math.floor(timeInSeconds));
  const mins: number = Math.floor(safe / 60);
  const secs: number = safe % 60;
  return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
}

/**
 * Formats a match event time as "P{period} - {mm:ss}" — the canonical
 * representation used across the admin live-match panel and the public
 * match summary so that both surfaces stay visually identical.
 */
export function formatMatchEventTime(
  periodNumber: number | null | undefined,
  timeInSeconds: number | null | undefined
): string {
  const period: number =
    periodNumber === undefined || periodNumber === null || Number.isNaN(periodNumber)
      ? 0
      : Math.max(0, Math.floor(periodNumber));
  return `P${period} - ${formatEventTimeMmSs(timeInSeconds)}`;
}
