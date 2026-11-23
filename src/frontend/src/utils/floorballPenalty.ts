/** Duration of a double minor (2+2 minutes), recorded as one penalty. */
export const FLOORBALL_DOUBLE_MINOR_MINUTES: number = 4;

/** Penalty length as scorekeepers write it: a four-minute penalty is "2+2". */
export function formatFloorballPenaltyMinutes(minutes: number): string {
  return minutes === FLOORBALL_DOUBLE_MINOR_MINUTES ? '2+2' : String(minutes);
}
