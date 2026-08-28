import {
  getCompetitionPathForSport,
  isTournamentCompetition,
  type CompetitionKind,
  type CompetitionRouteHints,
} from './sportRoutes';

export type FootballCompetitionKind = CompetitionKind;
export type FootballCompetitionRouteHints = CompetitionRouteHints;

export function isFootballTournamentCompetition(
  hints: FootballCompetitionRouteHints | null | undefined,
): boolean {
  return isTournamentCompetition(hints);
}

export function getFootballCompetitionPath(
  competitionId: string,
  kindOrHints?: FootballCompetitionKind | FootballCompetitionRouteHints | null,
  tab?: string,
): string {
  return getCompetitionPathForSport('football', competitionId, kindOrHints, tab);
}
