export interface LoanPlayerDto {
  playerId: string;
  rosterEntryId: string;
  firstName: string;
  lastName: string;
  displayName: string;
  jerseyNumber: number;
  loanPlayerNumber: number;
  position: string;
  isLoanPlayer: boolean;
}

export const LOAN_PLAYER_COUNT_MIN = 1;
export const LOAN_PLAYER_COUNT_MAX = 20;

export function parseLoanPlayerCount(value: string): number | null {
  const parsed = Number.parseInt(value, 10);
  if (!Number.isInteger(parsed) || parsed < LOAN_PLAYER_COUNT_MIN || parsed > LOAN_PLAYER_COUNT_MAX) {
    return null;
  }
  return parsed;
}
