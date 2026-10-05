export interface LoanGoalkeeperDto {
  playerId: string;
  rosterEntryId: string;
  firstName: string;
  lastName: string;
  displayName: string;
  position: string;
  isLoanGoalkeeper: boolean;
}

const STORED_LOAN_GOALKEEPER_NAMES = new Set(['Lainavahti -', 'Lainavahti']);

export function isStoredLoanGoalkeeperName(fullName: string): boolean {
  return STORED_LOAN_GOALKEEPER_NAMES.has(fullName.trim());
}

export function rosterDisplayName(fullName: string): { firstName: string; lastName: string } {
  if (isStoredLoanGoalkeeperName(fullName)) {
    return { firstName: 'Lainavahti', lastName: '' };
  }
  const trimmed = fullName.trim();
  const spaceIndex = trimmed.indexOf(' ');
  if (spaceIndex === -1) {
    return { firstName: trimmed, lastName: '' };
  }
  return {
    firstName: trimmed.slice(0, spaceIndex),
    lastName: trimmed.slice(spaceIndex + 1),
  };
}

export function formatPersonName(firstName: string, lastName: string): string {
  return [firstName, lastName].filter((part) => part.trim().length > 0).join(' ');
}
