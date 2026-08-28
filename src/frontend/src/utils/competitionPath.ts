import {
  getCompetitionPathForSport,
  isTournamentCompetition,
  type CompetitionKind,
  type CompetitionRouteHints,
} from './sportRoutes';

export type { CompetitionKind, CompetitionRouteHints };
export { isTournamentCompetition };

/**
 * Floorball public competition path. Prefer `getCompetitionPathForSport` for new code.
 */
export function getCompetitionPath(
  competitionId: string,
  kindOrHints?: CompetitionKind | CompetitionRouteHints | null,
  tab?: string,
): string {
  return getCompetitionPathForSport('floorball', competitionId, kindOrHints, tab);
}
