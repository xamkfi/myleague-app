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

/** Event flags follow the period being recorded, not sticky match-level overtime. */
export function floorballPeriodEventFlags(
  period: number,
  numberOfPeriods: number,
): { wasInOvertime: boolean; wasInShootout: boolean } {
  return {
    wasInOvertime: isFloorballOvertimePeriod(period, numberOfPeriods),
    wasInShootout: isFloorballShootoutPeriod(period, numberOfPeriods),
  };
}

/**
 * First period that has not started. An unplayed overtime slot is skipped when the
 * match already went straight to a shootout.
 */
export function nextOpenFloorballPeriod(
  maxPeriod: number,
  started: ReadonlySet<number>,
  numberOfPeriods: number,
  wentToOvertime: boolean,
  wentToShootout: boolean,
): number {
  const regularPeriods: number = numberOfPeriods > 0 ? numberOfPeriods : 2;
  const overtimePeriod: number = regularPeriods + 1;
  for (let period = 1; period <= maxPeriod; period += 1) {
    if (period === overtimePeriod && wentToShootout && !wentToOvertime) {
      continue;
    }
    if (!started.has(period)) {
      return period;
    }
  }
  return 0;
}
