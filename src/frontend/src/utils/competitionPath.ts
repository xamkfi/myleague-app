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
 * Hints used to classify a competition's kind for routing purposes. The authoritative source is
 * `competitionType` (added to `FloorballMatchDto` so the backend emits the discriminator directly).
 * The legacy heuristic fields (`tournamentGroupId` / `tournamentStage`) remain as a fallback for
 * any payload that pre-dates the explicit discriminator.
 *
 * All fields are nullable so a bare `FloorballMatchDto` can be passed in directly.
 */
export interface CompetitionRouteHints {
  competitionType?: 'Season' | 'Tournament' | null;
  tournamentGroupId?: string | null;
  tournamentStage?: string | null;
}

/**
 * Returns true when the supplied hints indicate a tournament match. Prefers the explicit
 * `competitionType` discriminator emitted by the backend; falls back to inferring from
 * tournament-only fields when that field is missing (older clients/payloads).
 */
export function isTournamentCompetition(hints: CompetitionRouteHints | null | undefined): boolean {
  if (!hints) {
    return false;
  }
  if (hints.competitionType === 'Tournament') {
    return true;
  }
  if (hints.competitionType === 'Season') {
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
