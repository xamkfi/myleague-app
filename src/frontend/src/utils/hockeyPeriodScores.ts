import type { HockeyMatchDto } from '../types/hockey/hockeyTypes';

export type PeriodScoreMap = Record<number, { homeScore: number; awayScore: number }>;

/**
 * Builds the period score map for MatchRow. Returns an empty map when the final score
 * has goals but no period row does, which means the score was entered without goal events.
 */
export function hockeyPeriodScoreMap(match: HockeyMatchDto): PeriodScoreMap {
  const scores: PeriodScoreMap = {};
  let periodGoals = 0;
  for (const period of match.periodScores) {
    scores[period.periodNumber] = { homeScore: period.homeGoals, awayScore: period.awayGoals };
    periodGoals += period.homeGoals + period.awayGoals;
  }
  if (periodGoals === 0 && match.homeScore + match.awayScore > 0) {
    return {};
  }
  return scores;
}
