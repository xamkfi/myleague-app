/**
 * Helpers for building public-facing competition URLs from match/competition data.
 *
 * Background: in the TPH (Table-Per-Hierarchy) competition model both seasons and tournaments
 * share the same `competitionId` GUID space, but they have different public routes:
 *   - Seasons live at      `/league/{id}`
 *   - Tournaments live at  `/tournaments/{id}`
 *
 * The match DTO does not (yet) carry an explicit `competitionType` discriminator, so we infer
 * it from tournament-only fields (`tournamentGroupId` / `tournamentStage`). When an explicit
 * hint is available callers should prefer passing it via the `kind` parameter.
 */

export type CompetitionKind = 'season' | 'tournament';

/**
 * Heuristic-friendly shape: anything carrying tournament-only fields is treated as a tournament.
 * Both fields are nullable so that bare `FloorballMatchDto`s can be passed in directly.
 */
export interface CompetitionRouteHints {
  tournamentGroupId?: string | null;
  tournamentStage?: string | null;
}

/**
 * Returns true when the supplied hints indicate a tournament match. A match is considered a
 * tournament match if it has either a tournament group association or a non-empty / non-"None"
 * stage label (group stage matches set `tournamentGroupId`; playoff matches set `tournamentStage`).
 */
export function isTournamentCompetition(hints: CompetitionRouteHints | null | undefined): boolean {
  if (!hints) {
    return false;
  }
  if (hints.tournamentGroupId) {
    return true;
  }
  const stage = hints.tournamentStage;
  return Boolean(stage && stage !== 'None');
}

/**
 * Build the public competition path for a given competition id.
 *
 * @param competitionId - The competition GUID (works for both seasons and tournaments)
 * @param kindOrHints  - Either an explicit kind (`'season' | 'tournament'`) or a hints object.
 *                       When omitted the function falls back to `'season'` to preserve the
 *                       legacy default routing.
 * @param tab          - Optional `?tab=...` query value to append.
 */
export function getCompetitionPath(
  competitionId: string,
  kindOrHints?: CompetitionKind | CompetitionRouteHints | null,
  tab?: string,
): string {
  const kind: CompetitionKind = typeof kindOrHints === 'string'
    ? kindOrHints
    : isTournamentCompetition(kindOrHints ?? undefined)
      ? 'tournament'
      : 'season';

  const basePath: string = kind === 'tournament'
    ? `/tournaments/${competitionId}`
    : `/league/${competitionId}`;

  return tab ? `${basePath}?tab=${tab}` : basePath;
}
