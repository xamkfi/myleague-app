/** Phase of play that decided a match. Matches the backend `MatchDecision` enum names. */
export type MatchDecision = 'Regulation' | 'Overtime' | 'Shootout';

export function matchDecisionOf(wentToOvertime: boolean, wentToShootout: boolean): MatchDecision {
  if (wentToShootout) return 'Shootout';
  return wentToOvertime ? 'Overtime' : 'Regulation';
}
