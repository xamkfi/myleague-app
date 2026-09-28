export type FloorballPeriodKind = 'regular' | 'overtime' | 'shootout';

/**
 * Regular periods stay numbered. The next period is overtime, and the one after that
 * is the penalty shootout. A shootout is never presented as another regular period.
 */
export function getFloorballPeriodKind(
  period: number,
  numberOfPeriods: number,
  wentToOvertime: boolean,
  wentToShootout: boolean
): FloorballPeriodKind {
  const regularPeriods: number = numberOfPeriods > 0 ? numberOfPeriods : 2;
  if (period <= regularPeriods) {
    return 'regular';
  }

  const overtimePeriod: number = regularPeriods + 1;
  const shootoutPeriod: number = regularPeriods + 2;
  if (period === shootoutPeriod || (wentToShootout && !wentToOvertime)) {
    return 'shootout';
  }

  if (period === overtimePeriod) {
    return 'overtime';
  }

  return wentToShootout ? 'shootout' : 'overtime';
}

export function isFloorballOvertimePeriod(period: number, numberOfPeriods: number): boolean {
  const regularPeriods: number = numberOfPeriods > 0 ? numberOfPeriods : 2;
  return period === regularPeriods + 1;
}

export function isFloorballShootoutPeriod(period: number, numberOfPeriods: number): boolean {
  const regularPeriods: number = numberOfPeriods > 0 ? numberOfPeriods : 2;
  return period === regularPeriods + 2;
}
