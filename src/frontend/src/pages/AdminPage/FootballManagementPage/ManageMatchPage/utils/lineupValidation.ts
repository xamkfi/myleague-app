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

export interface LineupDraftPlayer {
  playerId: string;
  position: FootballPosition;
  isOnField: boolean;
  isSentOff: boolean;
}

export function resolveMatchRules(rules: FootballMatchRules | undefined | null): FootballMatchRules {
  return rules ?? DEFAULT_FOOTBALL_MATCH_RULES;
}

export function getOnFieldPlayers(lineup: readonly FootballLineupPlayer[]): FootballLineupPlayer[] {
  return lineup.filter((player) => player.isOnField && !player.isSentOff);
}

export function getBenchPlayers(lineup: readonly FootballLineupPlayer[]): FootballLineupPlayer[] {
  return lineup.filter((player) => !player.isOnField && !player.isSentOff);
}

export function getSentOffPlayers(lineup: readonly FootballLineupPlayer[]): FootballLineupPlayer[] {
  return lineup.filter((player) => player.isSentOff);
}

export function validateTeamLineup(
  lineup: readonly LineupDraftPlayer[],
  rules: FootballMatchRules,
): string | null {
  const onField = lineup.filter((player) => player.isOnField);
  if (onField.some((player) => player.isSentOff)) {
    return 'Sent-off players cannot be on the field.';
  }
  if (onField.length !== rules.playersOnField) {
    return `On-field count must equal ${rules.playersOnField} players.`;
  }
  if (rules.requireGoalkeeper) {
    const goalkeeperCount = onField.filter(
      (player) => player.position === FootballPosition.Goalkeeper,
    ).length;
    if (goalkeeperCount !== 1) {
      return 'Exactly one on-field player must be a goalkeeper.';
    }
  }
  return null;
}

export function isTeamLineupReady(
  lineup: readonly FootballLineupPlayer[] | undefined,
  rules: FootballMatchRules,
): boolean {
  if (!lineup || lineup.length === 0) {
    return false;
  }
  return validateTeamLineup(lineup, rules) === null;
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

export function getPeriodLabel(period: number, rules: FootballMatchRules): string {
  if (isPenaltyShootoutPeriod(period, rules)) {
    return 'Penalty shootout';
  }
  if (isExtraTimePeriod(period, rules)) {
    const extraTimeIndex = period - extraTimeStartPeriod(rules) + 1;
    if (rules.extraTimeHalfCount > 1) {
      return `Extra time half ${extraTimeIndex}`;
    }
    return 'Extra time half';
  }
  return `Half ${period}`;
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
