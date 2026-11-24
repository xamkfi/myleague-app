import type { TFunction } from 'i18next';
import {
  FootballPosition,
  type FootballLineupPlayer,
  type FootballMatchRules,
} from '../../../../../types/football/footballTypes';

export const DEFAULT_FOOTBALL_MATCH_RULES: FootballMatchRules = {
  numberOfHalves: 2,
  halfDurationMinutes: 45,
  playersOnField: 11,
  requireGoalkeeper: true,
  maxSubstitutions: 0,
  requireOfficialsToStart: false,
  allowExtraTime: false,
  extraTimeHalfCount: 2,
  extraTimeHalfDurationMinutes: 15,
  allowPenaltyShootout: false,
};

export function resolveMatchRules(rules: FootballMatchRules | undefined | null): FootballMatchRules {
  return rules ?? DEFAULT_FOOTBALL_MATCH_RULES;
}

export function getSentOffPlayers(lineup: readonly FootballLineupPlayer[]): FootballLineupPlayer[] {
  return lineup.filter((player) => player.isSentOff);
}

/** Why a match cannot start with this lineup. The lineup can have any number of players. */
export type LineupProblem = 'needPlayers' | 'needGoalkeeper';

export function findLineupProblem(
  lineup: ReadonlyArray<Pick<FootballLineupPlayer, 'position'>>,
  rules: FootballMatchRules,
): LineupProblem | null {
  if (lineup.length === 0) {
    return 'needPlayers';
  }
  if (rules.requireGoalkeeper && lineup.every((player) => player.position !== FootballPosition.Goalkeeper)) {
    return 'needGoalkeeper';
  }
  return null;
}

export function isTeamLineupReady(
  lineup: readonly FootballLineupPlayer[] | undefined,
  rules: FootballMatchRules,
): boolean {
  return findLineupProblem(lineup ?? [], rules) === null;
}

export function areBothLineupsReady(
  homeLineup: readonly FootballLineupPlayer[] | undefined,
  awayLineup: readonly FootballLineupPlayer[] | undefined,
  rules: FootballMatchRules,
): boolean {
  return isTeamLineupReady(homeLineup, rules) && isTeamLineupReady(awayLineup, rules);
}

export function extraTimeStartPeriod(rules: FootballMatchRules): number {
  return rules.numberOfHalves + 1;
}

export function penaltyShootoutPeriod(rules: FootballMatchRules): number {
  return rules.numberOfHalves + (rules.allowExtraTime ? rules.extraTimeHalfCount : 0) + 1;
}

export function maxPeriodNumber(rules: FootballMatchRules): number {
  return (
    rules.numberOfHalves
    + (rules.allowExtraTime ? rules.extraTimeHalfCount : 0)
    + (rules.allowPenaltyShootout ? 1 : 0)
  );
}

/**
 * Period an event at `timeInSeconds` on the continuous match clock belongs to. With 2×45
 * minute halves, 44:59 is the first half and 45:00 the second. Time past regulation maps to
 * extra time only when extra time was played. The shootout has no clock, so it is never
 * derived from time.
 */
export function footballPeriodAtTime(
  timeInSeconds: number,
  rules: FootballMatchRules,
  extraTimePlayed: boolean,
): number {
  const halfSeconds: number = rules.halfDurationMinutes * 60;
  if (halfSeconds <= 0) return 1;
  const time: number = Math.max(0, timeInSeconds);
  const regulationSeconds: number = rules.numberOfHalves * halfSeconds;
  const extraHalfSeconds: number = rules.extraTimeHalfDurationMinutes * 60;
  if (extraTimePlayed && rules.allowExtraTime && time >= regulationSeconds && extraHalfSeconds > 0) {
    const extraIndex: number = Math.min(rules.extraTimeHalfCount, Math.floor((time - regulationSeconds) / extraHalfSeconds) + 1);
    return rules.numberOfHalves + extraIndex;
  }
  return Math.min(rules.numberOfHalves, Math.floor(time / halfSeconds) + 1);
}

export function isExtraTimePeriod(period: number, rules: FootballMatchRules): boolean {
  if (!rules.allowExtraTime) {
    return false;
  }
  const start = extraTimeStartPeriod(rules);
  return period >= start && period < penaltyShootoutPeriod(rules);
}

export function isPenaltyShootoutPeriod(period: number, rules: FootballMatchRules): boolean {
  return rules.allowPenaltyShootout && period === penaltyShootoutPeriod(rules);
}

export function getPeriodLabel(period: number, rules: FootballMatchRules, t: TFunction): string {
  if (isPenaltyShootoutPeriod(period, rules)) {
    return t('football.matches.manage.eventPeriod.shootout', 'Penalty shootout');
  }
  if (isExtraTimePeriod(period, rules)) {
    return t('football.matches.manage.eventPeriod.extraTime', 'Extra time {{number}}', {
      number: period - extraTimeStartPeriod(rules) + 1,
    });
  }
  return t('football.matches.manage.eventPeriod.half', 'Half {{number}}', { number: period });
}

export function getPeriodDurationSeconds(period: number, rules: FootballMatchRules): number {
  if (isPenaltyShootoutPeriod(period, rules)) {
    return 0;
  }
  if (isExtraTimePeriod(period, rules)) {
    return rules.extraTimeHalfDurationMinutes * 60;
  }
  return rules.halfDurationMinutes * 60;
}

/**
 * First period that has not started. Unplayed extra-time slots are skipped when the
 * match already went straight to a penalty shootout.
 */
export function nextOpenFootballPeriod(
  started: ReadonlySet<number>,
  rules: FootballMatchRules,
  wentToExtraTime: boolean,
  wentToPenaltyShootout: boolean,
): number {
  const maxPeriod: number = maxPeriodNumber(rules);
  const extraStart: number = extraTimeStartPeriod(rules);
  const shootout: number = penaltyShootoutPeriod(rules);
  for (let period = 1; period <= maxPeriod; period += 1) {
    const skippedExtraTime: boolean = wentToPenaltyShootout
      && !wentToExtraTime
      && period >= extraStart
      && period < shootout;
    if (skippedExtraTime) {
      continue;
    }
    if (!started.has(period)) {
      return period;
    }
  }
  return 0;
}

export function getTheoreticalPeriodStartSeconds(period: number, rules: FootballMatchRules): number {
  if (period <= 1) {
    return 0;
  }
  if (period <= rules.numberOfHalves) {
    return (period - 1) * rules.halfDurationMinutes * 60;
  }
  const regularTotal = rules.numberOfHalves * rules.halfDurationMinutes * 60;
  if (isExtraTimePeriod(period, rules)) {
    const extraTimeIndex = period - extraTimeStartPeriod(rules);
    return regularTotal + extraTimeIndex * rules.extraTimeHalfDurationMinutes * 60;
  }
  if (isPenaltyShootoutPeriod(period, rules)) {
    const extraTimeTotal = rules.allowExtraTime
      ? rules.extraTimeHalfCount * rules.extraTimeHalfDurationMinutes * 60
      : 0;
    return regularTotal + extraTimeTotal;
  }
  return regularTotal;
}

export function lineupPositionOrDefault(position: FootballPosition | undefined): FootballPosition {
  if (!position || position === FootballPosition.None) {
    return FootballPosition.Midfielder;
  }
  return position;
}
