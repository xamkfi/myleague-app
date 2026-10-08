import type { FloorballMatchDto, FloorballMatchRules } from '../types/floorball/floorballTypes';

export type FloorballPeriodKind = 'regular' | 'overtime' | 'shootout';

/**
 * Rules used when a match predates the match-rules feature and the DTO carries none.
 */
export const DEFAULT_FLOORBALL_MATCH_RULES: FloorballMatchRules = {
  numberOfPeriods: 2,
  periodDurationMinutes: 15,
  allowOvertime: true,
  overtimeDurationMinutes: 5,
  allowShootout: true,
};

export interface FloorballPeriodNumbers {
  regularPeriods: number;
  overtimePeriod: number;
  shootoutPeriod: number;
  /** Highest period the rules allow (shootout, overtime, or the last regular period). */
  maxPeriod: number;
}

export function resolveFloorballRules(rules: FloorballMatchRules | null | undefined): FloorballMatchRules {
  return rules ?? DEFAULT_FLOORBALL_MATCH_RULES;
}

export function floorballPeriodNumbers(rules: FloorballMatchRules): FloorballPeriodNumbers {
  const regularPeriods: number = rules.numberOfPeriods > 0 ? rules.numberOfPeriods : 2;
  const overtimePeriod: number = regularPeriods + 1;
  const shootoutPeriod: number = regularPeriods + 2;
  const maxPeriod: number = rules.allowShootout
    ? shootoutPeriod
    : rules.allowOvertime
      ? overtimePeriod
      : regularPeriods;
  return { regularPeriods, overtimePeriod, shootoutPeriod, maxPeriod };
}

/**
 * The period that follows `period` under the given rules, or 0 when no further period is
 * possible (the shootout just ended, overtime/shootout are disallowed, or the score is
 * not level so no extra period is needed).
 *
 * @param scoreTied Whether the match is level. Overtime and the shootout are only offered
 *   for a tie; a decided match ends with the current period.
 */
export function nextFloorballPeriodAfter(period: number, rules: FloorballMatchRules, scoreTied: boolean = true): number {
  const { regularPeriods, overtimePeriod, shootoutPeriod } = floorballPeriodNumbers(rules);
  if (period < regularPeriods) return period + 1;
  if (!scoreTied) return 0;
  if (period === regularPeriods) {
    if (rules.allowOvertime) return overtimePeriod;
    if (rules.allowShootout) return shootoutPeriod;
    return 0;
  }
  if (period === overtimePeriod && rules.allowShootout) return shootoutPeriod;
  return 0;
}

/**
 * Absolute clock mark (seconds) at which a period nominally begins. The match clock is
 * continuous, so period 2 of a 15-minute game starts at 15:00 regardless of when period 1
 * was actually ended. Shootouts do not tick, so their mark is cosmetic but keeps the clock
 * monotone.
 */
export function floorballPeriodStartSeconds(
  period: number,
  rules: FloorballMatchRules,
  wentToOvertime: boolean,
): number {
  const { regularPeriods, overtimePeriod, shootoutPeriod } = floorballPeriodNumbers(rules);
  const periodSeconds: number = rules.periodDurationMinutes * 60;
  const overtimeSeconds: number = rules.overtimeDurationMinutes * 60;
  const regulationSeconds: number = regularPeriods * periodSeconds;

  if (period === overtimePeriod) return regulationSeconds;
  if (period === shootoutPeriod) return wentToOvertime ? regulationSeconds + overtimeSeconds : regulationSeconds;
  return Math.max(0, (period - 1) * periodSeconds);
}

/**
 * Period an event at `timeInSeconds` on the continuous match clock belongs to. With 2×15
 * minute periods, 14:59 is period 1 and 15:00 is period 2. Time past regulation maps to
 * overtime only when overtime was played; otherwise it stays in the last regular period.
 * The shootout has no clock, so it is never derived from time.
 */
export function floorballPeriodAtTime(
  timeInSeconds: number,
  rules: FloorballMatchRules,
  overtimePlayed: boolean,
): number {
  const { regularPeriods, overtimePeriod } = floorballPeriodNumbers(rules);
  const periodSeconds: number = rules.periodDurationMinutes * 60;
  if (periodSeconds <= 0) return 1;
  if (overtimePlayed && timeInSeconds >= regularPeriods * periodSeconds) return overtimePeriod;
  return Math.min(regularPeriods, Math.floor(Math.max(0, timeInSeconds) / periodSeconds) + 1);
}

export interface FloorballPeriodState {
  started: Set<number>;
  ended: Set<number>;
  /** Period the live desk should operate on. */
  current: number;
  /** Next period that can be started, or 0 when none remains. */
  next: number;
}

/**
 * Derives the full period bookkeeping from the persisted match and the backend timer's
 * period. Pure, so the live desk can re-run it after every match refresh without keeping
 * mirror refs around.
 */
export function deriveFloorballPeriodState(
  match: Pick<FloorballMatchDto, 'status' | 'periodScores' | 'wentToOvertime' | 'wentToShootout' | 'matchRules' | 'homeScore' | 'awayScore'>,
  timerPeriod: number | null,
): FloorballPeriodState {
  const rules: FloorballMatchRules = resolveFloorballRules(match.matchRules);
  const { regularPeriods, overtimePeriod, shootoutPeriod, maxPeriod } = floorballPeriodNumbers(rules);
  const started: Set<number> = new Set<number>();
  const ended: Set<number> = new Set<number>();

  if (match.status === 'Scheduled' || match.status === 'Postponed' || match.status === 'Cancelled') {
    return { started, ended, current: 1, next: 1 };
  }

  // Regular periods are pre-populated on the match, so a mere entry says nothing about
  // whether play has begun; only `isCompleted` is authoritative.
  for (const [key, score] of Object.entries(match.periodScores ?? {})) {
    const period: number = Number(key);
    if (!Number.isFinite(period) || period < 1 || !score.isCompleted) continue;
    started.add(period);
    ended.add(period);
  }

  const activeTimerPeriod: number = timerPeriod && timerPeriod >= 1 ? timerPeriod : 1;
  started.add(activeTimerPeriod);
  if (match.wentToOvertime) started.add(overtimePeriod);
  if (match.wentToShootout) started.add(shootoutPeriod);

  if (match.status === 'Completed') {
    // Everything that was played is over; nothing can be started or ended any more.
    for (const period of started) ended.add(period);
    const last: number = Math.max(1, ...started);
    return { started, ended, current: last, next: 0 };
  }

  let next: number = nextOpenFloorballPeriod(maxPeriod, started, regularPeriods, match.wentToOvertime, match.wentToShootout);
  if (next > maxPeriod || started.has(maxPeriod)) next = 0;
  // Extra periods exist only to break a tie. Evaluated live, so an overtime goal
  // immediately turns "start shootout" into "finish the match".
  if (next > regularPeriods && (match.homeScore ?? 0) !== (match.awayScore ?? 0)) next = 0;

  let current: number;
  if (match.wentToShootout && !ended.has(shootoutPeriod)) {
    current = shootoutPeriod;
  } else if (ended.has(activeTimerPeriod) && next > 0) {
    // Intermission: the desk already points at the period that will be started next.
    current = next;
  } else {
    current = activeTimerPeriod;
  }

  return { started, ended, current, next };
}

export function areNumberSetsEqual(a: ReadonlySet<number>, b: ReadonlySet<number>): boolean {
  if (a === b) return true;
  if (a.size !== b.size) return false;
  for (const value of a) {
    if (!b.has(value)) return false;
  }
  return true;
}

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
