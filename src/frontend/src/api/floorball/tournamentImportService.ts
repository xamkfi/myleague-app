import type { Club } from '../common/clubService';
import type { DivisionType } from '../../types/common/divisionType';
import { SportsCategory } from '../../types/common/sports';
import type {
  CreatedRecord,
  ImportCallbacks,
  ImportDryRunCounts,
  ImportError,
  ImportOptions,
  ImportPhase,
  ImportSummary,
  TournamentImportClub,
  TournamentImportMatch,
  TournamentImportPayload,
  TournamentImportTeam,
  TournamentImportTeamPlayer,
} from '../../types/floorball/tournamentImportTypes';
import { ImportAbortedError } from '../../types/floorball/tournamentImportTypes';
import { FloorballPosition, TeamCategory } from '../../types/floorball/floorballTypes';
import type {
  CreateFloorballMatchRequest,
  FloorballTeamRequest,
} from '../../types/floorball/floorballTypes';
import type { CreateFloorballTournamentRequest, FloorballTournamentDto } from '../../types/floorball/tournamentTypes';
import { clubService } from '../common/clubService';
import { divisionService } from '../common/divisionService';
import { floorballTeamNameSearchService } from './floorballTeamNameSearchService';
import { floorballTeamService } from './floorballTeamService';
import { floorballTournamentService } from './floorballTournamentService';
import { floorballMatchService } from './floorballMatchService';
import { floorballPlayerService } from './floorballPlayerService';
import { personApi } from '../admin/personApi';
import type { Person, PersonFormData } from '../../types/admin/personTypes';
import { PersonRole } from '../../types/admin/personTypes';

// ---------------------------------------------------------------------------
// Public helpers
// ---------------------------------------------------------------------------

/**
 * Pure function — counts what an import would create, without calling any APIs.
 * Used by the modal to show a dry-run summary before the user hits "Import".
 */
export function getDryRunCounts(payload: TournamentImportPayload): ImportDryRunCounts {
  const groupAssignments = payload.groups.reduce((sum, g) => sum + g.teamNames.length, 0);
  const players = payload.teams.reduce((sum, t) => sum + (t.players?.length ?? 0), 0);
  return {
    clubs: payload.clubs.length,
    teams: payload.teams.length,
    players,
    groups: payload.groups.length,
    groupAssignments,
    matches: payload.matches.length,
    playoffSlots: payload.playoffSchedule?.length ?? 0,
  };
}

/**
 * Best-effort structural validation. Catches the obvious mistakes (missing required fields,
 * dangling team/club/group references) before any network calls are made.
 */
export function validatePayload(payload: unknown): { valid: true; payload: TournamentImportPayload } | { valid: false; errors: string[] } {
  const errors: string[] = [];

  if (!payload || typeof payload !== 'object') {
    return { valid: false, errors: ['Uploaded file is not a JSON object.'] };
  }

  const p = payload as Partial<TournamentImportPayload>;
  if (!p.tournament || typeof p.tournament !== 'object') {
    errors.push('Missing required "tournament" section.');
  } else {
    if (!p.tournament.name) errors.push('tournament.name is required.');
    if (!p.tournament.startDate) errors.push('tournament.startDate is required.');
    if (!p.tournament.endDate) errors.push('tournament.endDate is required.');
  }

  if (!Array.isArray(p.clubs)) errors.push('"clubs" must be an array.');
  if (!Array.isArray(p.teams)) errors.push('"teams" must be an array.');
  if (!Array.isArray(p.groups)) errors.push('"groups" must be an array.');
  if (!Array.isArray(p.matches)) errors.push('"matches" must be an array.');

  if (errors.length > 0) return { valid: false, errors };

  const teamNames = new Set((p.teams ?? []).map((t) => t.name));
  const clubNames = new Set((p.clubs ?? []).map((c) => c.name));
  const groupNames = new Set((p.groups ?? []).map((g) => g.name));

  for (const team of p.teams ?? []) {
    if (!team.clubName) errors.push(`Team "${team.name}" is missing clubName.`);
    else if (!clubNames.has(team.clubName)) {
      // Don't fail — the orchestrator will auto-create a club with that name. Warn only.
    }
    // divisionName is intentionally optional — tournaments do not require divisions.

    // Player rosters are optional. When present, every entry must at least name a person —
    // either explicitly via firstName + lastName, or implicitly via personEmail.
    if (team.players && team.players.length > 0) {
      for (let i = 0; i < team.players.length; i++) {
        const pl = team.players[i];
        const hasName = typeof pl.firstName === 'string' && pl.firstName.trim().length > 0
          && typeof pl.lastName === 'string' && pl.lastName.trim().length > 0;
        const hasEmail = typeof pl.personEmail === 'string' && pl.personEmail.trim().length > 0;
        if (!hasName && !hasEmail) {
          errors.push(`Team "${team.name}" player #${i + 1} is missing both name and email — at least one is required.`);
        }
      }
    }
  }

  for (const group of p.groups ?? []) {
    for (const teamName of group.teamNames) {
      if (!teamNames.has(teamName)) {
        errors.push(`Group "${group.name}" references unknown team "${teamName}".`);
      }
    }
  }

  for (const m of p.matches ?? []) {
    if (!m.homeTeamName || !teamNames.has(m.homeTeamName)) {
      errors.push(`Match #${m.matchNumber ?? '?'} references unknown home team "${m.homeTeamName}".`);
    }
    if (!m.awayTeamName || !teamNames.has(m.awayTeamName)) {
      errors.push(`Match #${m.matchNumber ?? '?'} references unknown away team "${m.awayTeamName}".`);
    }
    if (m.groupName && !groupNames.has(m.groupName)) {
      errors.push(`Match #${m.matchNumber ?? '?'} references unknown group "${m.groupName}".`);
    }
    if (!m.scheduledDateTime || isNaN(Date.parse(m.scheduledDateTime))) {
      errors.push(`Match #${m.matchNumber ?? '?'} has an invalid scheduledDateTime: "${m.scheduledDateTime}".`);
    }
  }

  // Playoff schedule is optional. When present it must reference real round keys, contain a
  // valid date and avoid duplicate (round, order) entries.
  const validRounds = new Set<string>(['QuarterFinal', 'SemiFinal', 'ThirdPlaceMatch', 'Final']);
  if (Array.isArray(p.playoffSchedule)) {
    const seen = new Set<string>();
    for (let i = 0; i < p.playoffSchedule.length; i++) {
      const slot = p.playoffSchedule[i];
      if (!slot || typeof slot !== 'object') {
        errors.push(`playoffSchedule[${i}] is not an object.`);
        continue;
      }
      if (!validRounds.has(slot.round as string)) {
        errors.push(`playoffSchedule[${i}].round must be one of QuarterFinal | SemiFinal | ThirdPlaceMatch | Final.`);
      }
      if (!Number.isInteger(slot.order) || slot.order < 0) {
        errors.push(`playoffSchedule[${i}].order must be a non-negative integer.`);
      }
      if (!slot.scheduledDateTime || isNaN(Date.parse(slot.scheduledDateTime))) {
        errors.push(`playoffSchedule[${i}] has an invalid scheduledDateTime: "${slot.scheduledDateTime}".`);
      }
      const key = `${slot.round}|${slot.order}`;
      if (seen.has(key)) {
        errors.push(`playoffSchedule[${i}] duplicates an earlier slot for ${slot.round} #${slot.order}.`);
      }
      seen.add(key);
    }
  }

  if (errors.length > 0) return { valid: false, errors };
  return { valid: true, payload: payload as TournamentImportPayload };
}

// ---------------------------------------------------------------------------
// Orchestration
// ---------------------------------------------------------------------------

const DIVISION_DEFAULT_DESCRIPTION = 'Auto-created by tournament JSON import.';
// The Division entity validates level to be 1..10. Tournament-created divisions don't really
// belong to any league hierarchy, so we tuck them at the bottom level 10.
const DIVISION_DEFAULT_LEVEL = 10;

/**
 * Runs the import pipeline. All steps are idempotent (find-or-create); only freshly created
 * entities are added to the returned summary's `created` log so the revert flow only touches
 * what this import actually produced.
 *
 * Always returns an ImportSummary — even on fatal failure. Check `summary.fatal` / `summary.aborted`
 * and offer the user a revert using `summary.created`.
 */
export async function importTournament(
  payload: TournamentImportPayload,
  callbacks: ImportCallbacks,
  options: ImportOptions = {},
): Promise<ImportSummary> {
  const summary: ImportSummary = {
    clubsCreated: 0,
    clubsExisting: 0,
    divisionsCreated: 0,
    divisionsExisting: 0,
    teamsCreated: 0,
    teamsExisting: 0,
    personsCreated: 0,
    personsExisting: 0,
    playersCreated: 0,
    playersExisting: 0,
    teamPlayerAssignments: 0,
    tournamentId: null,
    tournamentName: null,
    groupsCreated: 0,
    groupAssignments: 0,
    matchesCreated: 0,
    errors: [],
    created: [],
    fatal: false,
    aborted: false,
  };

  const reportFatal = (phase: ImportPhase, label: string, err: unknown): void => {
    const message = err instanceof ImportAbortedError
      ? err.message
      : prettifyError(err);
    if (err instanceof ImportAbortedError) {
      summary.aborted = true;
    } else {
      summary.fatal = true;
    }
    const e: ImportError = { phase, label, message, fatal: true };
    summary.errors.push(e);
    callbacks.onError(e);
  };

  const checkAbort = (): boolean => {
    if (callbacks.shouldAbort()) {
      reportFatal('validate', 'Aborted', new ImportAbortedError());
      return true;
    }
    return false;
  };

  // 1) Clubs ---------------------------------------------------------------
  const clubByName = new Map<string, Club>();
  for (let i = 0; i < payload.clubs.length; i++) {
    if (checkAbort()) return summary;
    const c = payload.clubs[i];
    try {
      const club = await ensureClub(c);
      clubByName.set(c.name, club.club);
      if (club.created) {
        summary.clubsCreated++;
        summary.created.push({ kind: 'club', id: club.club.id, label: c.name });
        callbacks.onStep({ phase: 'clubs', index: i, total: payload.clubs.length, label: `Created club "${c.name}"`, status: 'created' });
      } else {
        summary.clubsExisting++;
        callbacks.onStep({ phase: 'clubs', index: i, total: payload.clubs.length, label: `Found existing club "${c.name}"`, status: 'existing' });
      }
    } catch (err) {
      reportFatal('clubs', `Club "${c.name}"`, err);
      return summary;
    }
  }

  // 2) Divisions -----------------------------------------------------------
  // Tournaments don't require divisions. We only resolve them when at least one team in the
  // import asks for one via `divisionName`. If none do, the whole phase is skipped and the
  // teams are created without a division (DivisionId is nullable on the backend).
  const distinctDivisionNames = Array.from(
    new Set(
      payload.teams
        .map((t) => (t.divisionName ?? '').trim())
        .filter((n) => n.length > 0),
    ),
  );
  const divisionByName = new Map<string, DivisionType>();
  if (distinctDivisionNames.length === 0) {
    callbacks.onStep({
      phase: 'division',
      index: 0,
      total: 1,
      label: 'No divisions specified — skipping division phase',
      status: 'skipped',
    });
  } else {
    if (checkAbort()) return summary;
    let existingDivisions: DivisionType[] = [];
    try {
      const all = await divisionService.getAll();
      existingDivisions = all.data ?? [];
    } catch (err) {
      reportFatal('division', 'Load divisions', err);
      return summary;
    }

    for (let i = 0; i < distinctDivisionNames.length; i++) {
      if (checkAbort()) return summary;
      const name = distinctDivisionNames[i];
      const existing = existingDivisions.find((d) => d.name.toLowerCase() === name.toLowerCase());
      if (existing) {
        divisionByName.set(name, existing);
        summary.divisionsExisting++;
        callbacks.onStep({
          phase: 'division',
          index: i,
          total: distinctDivisionNames.length,
          label: `Found existing division "${name}"`,
          status: 'existing',
        });
        continue;
      }
      try {
        const created = await divisionService.create({
          name,
          description: DIVISION_DEFAULT_DESCRIPTION,
          level: DIVISION_DEFAULT_LEVEL,
          sportType: SportsCategory.Floorball,
        });
        divisionByName.set(name, created.data);
        summary.divisionsCreated++;
        summary.created.push({ kind: 'division', id: created.data.id, label: name });
        callbacks.onStep({
          phase: 'division',
          index: i,
          total: distinctDivisionNames.length,
          label: `Created division "${name}"`,
          status: 'created',
        });
      } catch (err) {
        reportFatal('division', `Division "${name}"`, err);
        return summary;
      }
    }
  }

  // 3) Teams ---------------------------------------------------------------
  const teamIdByName = new Map<string, string>();
  const tournamentVenue = (payload.tournament.venue ?? '').trim() || null;
  const defaultCategory = options.defaultTeamCategory ?? TeamCategory.Adult;
  for (let i = 0; i < payload.teams.length; i++) {
    if (checkAbort()) return summary;
    const t = payload.teams[i];
    try {
      const result = await ensureTeam(t, clubByName, divisionByName, tournamentVenue, defaultCategory);
      teamIdByName.set(t.name, result.id);
      if (result.created) {
        summary.teamsCreated++;
        summary.created.push({ kind: 'team', id: result.id, label: t.name });
        callbacks.onStep({ phase: 'teams', index: i, total: payload.teams.length, label: `Created team "${t.name}"`, status: 'created' });
      } else {
        summary.teamsExisting++;
        callbacks.onStep({ phase: 'teams', index: i, total: payload.teams.length, label: `Found existing team "${t.name}"`, status: 'existing' });
      }
    } catch (err) {
      reportFatal('teams', `Team "${t.name}"`, err);
      return summary;
    }
  }

  // 3b) Players ------------------------------------------------------------
  // Walk every team's optional roster and find-or-create Person + FloorballPlayer,
  // then add to the team. Non-fatal: a per-player failure is logged but the import
  // continues so the tournament still gets the schedule even if a single roster row
  // is bad.
  const totalPlayerOps = payload.teams.reduce((sum, t) => sum + (t.players?.length ?? 0), 0);
  if (totalPlayerOps > 0) {
    let playerIdx = 0;
    for (const t of payload.teams) {
      const teamId = teamIdByName.get(t.name);
      if (!teamId || !t.players || t.players.length === 0) continue;
      // Snapshot the team's current roster once so we can detect already-on-the-team
      // entries without N round-trips per player.
      const existingTeamPlayerIds = await loadExistingTeamPlayerIds(teamId);
      for (const playerSpec of t.players) {
        if (checkAbort()) return summary;
        const label = `${playerSpec.firstName ?? ''} ${playerSpec.lastName ?? ''}`.trim()
          || playerSpec.personEmail
          || 'Unknown player';
        try {
          await importPlayer(playerSpec, teamId, t.name, label, existingTeamPlayerIds, summary, callbacks, {
            phase: 'players',
            index: playerIdx,
            total: totalPlayerOps,
          });
        } catch (err) {
          // Treat per-player failures as non-fatal so one bad row doesn't kill the import.
          const e: ImportError = {
            phase: 'players',
            label: `${t.name} / ${label}`,
            message: prettifyError(err),
            fatal: false,
          };
          summary.errors.push(e);
          callbacks.onError(e);
        }
        playerIdx++;
      }
    }
  }

  // 4) Tournament ----------------------------------------------------------
  if (checkAbort()) return summary;
  let tournament: FloorballTournamentDto;
  try {
    // Forward the optional playoff schedule with the tournament create. The backend stores it
    // on the tournament so the public schedule renders placeholder rows for QF/SF/F slots
    // even before the bracket is generated. Slots with bad data are dropped here (a single
    // malformed date shouldn't block the entire tournament from being created).
    const playoffSchedule = (payload.playoffSchedule ?? [])
      .filter((s) => s.round && Number.isFinite(s.order) && s.scheduledDateTime && !isNaN(Date.parse(s.scheduledDateTime)))
      .map((s) => ({
        round: s.round,
        order: s.order,
        scheduledDateTime: s.scheduledDateTime,
        venue: s.venue ?? undefined,
      }));

    const req: CreateFloorballTournamentRequest = {
      name: payload.tournament.name,
      startDate: payload.tournament.startDate,
      endDate: payload.tournament.endDate,
      venue: payload.tournament.venue ?? undefined,
      contentHtml: payload.tournament.contentHtml ?? undefined,
      groupStageNumberOfPeriods: payload.tournament.groupStageNumberOfPeriods,
      groupStagePeriodDurationMinutes: payload.tournament.groupStagePeriodDurationMinutes,
      groupStageAllowOvertime: payload.tournament.groupStageAllowOvertime,
      groupStageOvertimeDurationMinutes: payload.tournament.groupStageOvertimeDurationMinutes,
      groupStageAllowShootout: payload.tournament.groupStageAllowShootout,
      playoffNumberOfPeriods: payload.tournament.playoffNumberOfPeriods,
      playoffPeriodDurationMinutes: payload.tournament.playoffPeriodDurationMinutes,
      playoffAllowOvertime: payload.tournament.playoffAllowOvertime,
      playoffOvertimeDurationMinutes: payload.tournament.playoffOvertimeDurationMinutes,
      playoffAllowShootout: payload.tournament.playoffAllowShootout,
      teamsAdvancingPerGroup: payload.tournament.teamsAdvancingPerGroup,
      hasPlayoffStage: payload.tournament.hasPlayoffStage,
      hasThirdPlaceMatch: payload.tournament.hasThirdPlaceMatch,
      playoffSchedule: playoffSchedule.length > 0 ? playoffSchedule : undefined,
    };
    const resp = await floorballTournamentService.create(req);
    tournament = resp.data;
    summary.tournamentId = tournament.id;
    summary.tournamentName = tournament.name;
    summary.created.push({ kind: 'tournament', id: tournament.id, label: tournament.name });
    callbacks.onStep({ phase: 'tournament', index: 0, total: 1, label: `Created tournament "${tournament.name}"`, status: 'created' });
  } catch (err) {
    reportFatal('tournament', `Tournament "${payload.tournament.name}"`, err);
    return summary;
  }

  // 5) Groups --------------------------------------------------------------
  const groupIdByName = new Map<string, string>();
  for (let i = 0; i < payload.groups.length; i++) {
    if (checkAbort()) return summary;
    const g = payload.groups[i];
    try {
      const resp = await floorballTournamentService.addGroup(tournament.id, g.name);
      tournament = resp.data;
      const created = tournament.groups.find((tg) => tg.name.toLowerCase() === g.name.toLowerCase());
      if (!created) {
        throw new Error(`Group "${g.name}" was not present in tournament response after creation.`);
      }
      groupIdByName.set(g.name, created.id);
      summary.groupsCreated++;
      summary.created.push({ kind: 'group', tournamentId: tournament.id, groupId: created.id, label: g.name });
      callbacks.onStep({ phase: 'groups', index: i, total: payload.groups.length, label: `Created group "${g.name}"`, status: 'created' });
    } catch (err) {
      reportFatal('groups', `Group "${g.name}"`, err);
      return summary;
    }
  }

  // 6) Assign teams to groups ---------------------------------------------
  const totalAssignments = payload.groups.reduce((sum, g) => sum + g.teamNames.length, 0);
  let assignIdx = 0;
  for (const g of payload.groups) {
    const groupId = groupIdByName.get(g.name);
    if (!groupId) continue; // already reported as fatal above
    for (const teamName of g.teamNames) {
      if (checkAbort()) return summary;
      const teamId = teamIdByName.get(teamName);
      if (!teamId) {
        const e: ImportError = {
          phase: 'group-teams',
          label: `${teamName} → ${g.name}`,
          message: `Team "${teamName}" was not created and cannot be added to group "${g.name}".`,
          fatal: false,
        };
        summary.errors.push(e);
        callbacks.onError(e);
        assignIdx++;
        continue;
      }
      try {
        const resp = await floorballTournamentService.addTeamToGroup(tournament.id, groupId, teamId);
        tournament = resp.data;
        summary.groupAssignments++;
        callbacks.onStep({
          phase: 'group-teams',
          index: assignIdx,
          total: totalAssignments,
          label: `Assigned "${teamName}" to group "${g.name}"`,
          status: 'created',
        });
      } catch (err) {
        reportFatal('group-teams', `${teamName} → ${g.name}`, err);
        return summary;
      }
      assignIdx++;
    }
  }

  // 7) Matches -------------------------------------------------------------
  for (let i = 0; i < payload.matches.length; i++) {
    if (checkAbort()) return summary;
    const m = payload.matches[i];
    const homeId = teamIdByName.get(m.homeTeamName);
    const awayId = teamIdByName.get(m.awayTeamName);
    const groupId = m.groupName ? groupIdByName.get(m.groupName) : undefined;
    if (!homeId || !awayId) {
      const e: ImportError = {
        phase: 'matches',
        label: matchLabel(m),
        message: `Cannot create match — missing team id (home=${m.homeTeamName}, away=${m.awayTeamName}).`,
        fatal: false,
      };
      summary.errors.push(e);
      callbacks.onError(e);
      continue;
    }
    try {
      const req: CreateFloorballMatchRequest = {
        competitionId: tournament.id,
        homeTeamId: homeId,
        awayTeamId: awayId,
        scheduledDateTime: m.scheduledDateTime,
        venue: composeVenue(payload.tournament.venue ?? null, m.field ?? null),
        tournamentGroupId: groupId,
        tournamentStage: 'GroupStage',
      };
      const resp = await floorballMatchService.create(req);
      summary.matchesCreated++;
      summary.created.push({ kind: 'match', id: resp.data.id, label: matchLabel(m) });
      callbacks.onStep({
        phase: 'matches',
        index: i,
        total: payload.matches.length,
        label: `Created match ${matchLabel(m)}`,
        status: 'created',
      });
    } catch (err) {
      reportFatal('matches', matchLabel(m), err);
      return summary;
    }
  }

  callbacks.onStep({ phase: 'done', index: 1, total: 1, label: 'Import finished successfully', status: 'info' });
  return summary;
}

/**
 * Reverts everything `importTournament` created, walking `created` in reverse order so that
 * children (matches, groups) come down before their parents (tournament, teams, division, clubs).
 *
 * Continues on errors — every failure is reported via the callback but cleanup keeps going so
 * the user is left with the smallest possible mess.
 */
export async function revertImport(
  records: CreatedRecord[],
  callbacks: Pick<ImportCallbacks, 'onStep' | 'onError'>,
): Promise<{ deleted: number; failed: number }> {
  let deleted = 0;
  let failed = 0;
  const total = records.length;
  // Walk in reverse so dependencies disappear in the safe order:
  // match → group → tournament → team → division → club
  for (let i = records.length - 1; i >= 0; i--) {
    const r = records[i];
    const reversedIndex = records.length - 1 - i;
    try {
      await deleteRecord(r);
      deleted++;
      callbacks.onStep({
        phase: phaseForRecord(r),
        index: reversedIndex,
        total,
        label: `Removed ${r.kind} "${r.label}"`,
        status: 'skipped',
      });
    } catch (err) {
      failed++;
      callbacks.onError({
        phase: phaseForRecord(r),
        label: `Revert ${r.kind} "${r.label}"`,
        message: prettifyError(err),
        fatal: false,
      });
    }
  }
  return { deleted, failed };
}

// ---------------------------------------------------------------------------
// Internals
// ---------------------------------------------------------------------------

async function ensureClub(c: TournamentImportClub): Promise<{ club: Club; created: boolean }> {
  // searchByName is case-sensitive on the server; try exact match first, fall back to a
  // contains-style scan of the page-1 results.
  const matches = await clubService.searchByName(c.name);
  const exact = matches.find((m) => m.name.toLowerCase() === c.name.toLowerCase());
  if (exact) return { club: exact, created: false };

  try {
    const created = await clubService.create({
      name: c.name,
      city: c.city ?? undefined,
      country: c.country ?? undefined,
      websiteUrl: c.websiteUrl ?? undefined,
      logoUrl: c.logoUrl ?? undefined,
      contactEmail: c.contactEmail ?? undefined,
    });
    return { club: created, created: true };
  } catch (err) {
    // Handle benign duplicates: if the server rejected the create (likely "Club already exists"),
    // re-query and reuse it instead of aborting the import.
    const retry = await clubService.searchByName(c.name);
    const fallback = retry.find((m) => m.name.toLowerCase() === c.name.toLowerCase());
    if (fallback) return { club: fallback, created: false };
    throw err;
  }
}

async function ensureTeam(
  t: TournamentImportTeam,
  clubByName: Map<string, Club>,
  divisionByName: Map<string, DivisionType>,
  tournamentVenueFallback: string | null,
  defaultCategory: TeamCategory,
): Promise<{ id: string; created: boolean }> {
  const existing = await findExistingTeam(t.name);
  if (existing) return { id: existing.id, created: false };

  let club = clubByName.get(t.clubName);
  if (!club) {
    // Club wasn't in the import's clubs[] list — auto-create it on the fly so the team can be made.
    const fallback = await ensureClub({ name: t.clubName });
    clubByName.set(t.clubName, fallback.club);
    club = fallback.club;
  }
  // Division is optional. Tournaments don't require one; the backend accepts null DivisionId.
  const divisionName = (t.divisionName ?? '').trim();
  let divisionId: string | undefined;
  if (divisionName.length > 0) {
    const division = divisionByName.get(divisionName);
    if (!division) {
      throw new Error(`Division "${divisionName}" was not resolved before creating team "${t.name}".`);
    }
    divisionId = division.id;
  }

  // All of homeArena, primaryJerseyColor and category are optional on the backend now. We
  // still prefer the tournament's venue as the home arena when the team JSON didn't specify
  // one — it's the most useful default for tournament-only teams.
  const req: FloorballTeamRequest = {
    name: t.name,
    clubId: club.id,
    divisionId,
    homeArena: nonEmpty(t.homeArena) ?? tournamentVenueFallback ?? undefined,
    primaryJerseyColor: nonEmpty(t.primaryJerseyColor) ?? undefined,
    secondaryJerseyColor: nonEmpty(t.secondaryJerseyColor) ?? undefined,
    // Per-team category in JSON wins; otherwise use the import-wide default picked in the UI.
    category: t.category ?? defaultCategory,
  };
  try {
    const created = await floorballTeamService.create(req);
    return { id: created.id, created: true };
  } catch (err) {
    // Some servers return 400/409 for "already exists" conditions. Re-check the name index
    // after a failed create so a benign race / pre-existing duplicate doesn't abort the import.
    const fallback = await findExistingTeam(t.name);
    if (fallback) return { id: fallback.id, created: false };
    throw err;
  }
}

function nonEmpty(value: string | null | undefined): string | null {
  const trimmed = (value ?? '').trim();
  return trimmed.length > 0 ? trimmed : null;
}

// ---------------------------------------------------------------------------
// Player import helpers
// ---------------------------------------------------------------------------

/** Returns a Set of playerIds already on the given team, used to skip duplicates. */
async function loadExistingTeamPlayerIds(teamId: string): Promise<Set<string>> {
  try {
    const roster = await floorballPlayerService.getByTeamId(teamId);
    return new Set(roster.map((p) => p.id));
  } catch {
    // If we can't fetch the roster (e.g. team has no players), treat it as empty.
    return new Set();
  }
}

interface PlayerStepContext {
  phase: ImportPhase;
  index: number;
  total: number;
}

async function importPlayer(
  spec: TournamentImportTeamPlayer,
  teamId: string,
  teamName: string,
  label: string,
  existingTeamPlayerIds: Set<string>,
  summary: ImportSummary,
  callbacks: ImportCallbacks,
  step: PlayerStepContext,
): Promise<void> {
  // 1) Find-or-create the Person.
  const personResult = await ensurePerson(spec);
  if (personResult.created) {
    summary.personsCreated++;
    summary.created.push({ kind: 'person', id: personResult.person.id, label });
  } else {
    summary.personsExisting++;
  }

  // 2) Find-or-create the FloorballPlayer for that Person.
  const playerResult = await ensureFloorballPlayer(personResult.person.id, personResult.person.fullName);
  if (playerResult.created) {
    summary.playersCreated++;
    summary.created.push({ kind: 'player', id: playerResult.id, personId: personResult.person.id, label });
  } else {
    summary.playersExisting++;
  }

  // 3) Add to the team unless already on the roster.
  if (existingTeamPlayerIds.has(playerResult.id)) {
    callbacks.onStep({
      phase: step.phase,
      index: step.index,
      total: step.total,
      label: `${teamName}: ${label} already on roster — skipped`,
      status: 'skipped',
    });
    return;
  }

  const position = spec.position ?? FloorballPosition.Forward;
  const jerseyNumber = typeof spec.jerseyNumber === 'number' && spec.jerseyNumber > 0
    ? spec.jerseyNumber
    : undefined;
  try {
    await floorballTeamService.addPlayerToTeam(teamId, playerResult.id, position, jerseyNumber);
  } catch (err) {
    // The backend rejects duplicate roster entries. If the player got assigned by another
    // race, swallow the error so the import still counts the row as resolved.
    const refreshed = await loadExistingTeamPlayerIds(teamId);
    if (!refreshed.has(playerResult.id)) {
      throw err;
    }
  }
  existingTeamPlayerIds.add(playerResult.id);
  summary.teamPlayerAssignments++;
  summary.created.push({ kind: 'team-player', teamId, playerId: playerResult.id, label: `${teamName} / ${label}` });
  callbacks.onStep({
    phase: step.phase,
    index: step.index,
    total: step.total,
    label: `Added ${label} to "${teamName}"`,
    status: 'created',
  });
}

async function ensurePerson(spec: TournamentImportTeamPlayer): Promise<{ person: Person; created: boolean }> {
  // Prefer email-based identity when provided.
  const email = spec.personEmail?.trim();
  if (email && email.length > 0) {
    try {
      const existing = await personApi.getByEmail(email);
      if (existing) return { person: existing, created: false };
    } catch {
      // Fall through to name-based search and creation.
    }
  }

  const firstName = (spec.firstName ?? '').trim();
  const lastName = (spec.lastName ?? '').trim();
  if (firstName.length === 0 || lastName.length === 0) {
    throw new Error('Player is missing firstName/lastName and personEmail did not resolve.');
  }

  // Name-based search: pull a page of matches and pick a case-insensitive exact match
  // on first + last name. Birthdate disambiguates if provided.
  const searchTerm = `${firstName} ${lastName}`;
  try {
    const matches = await personApi.search(searchTerm, 1, 25);
    const list = matches.data ?? [];
    const exact = list.find((p) =>
      p.firstName.trim().toLowerCase() === firstName.toLowerCase()
      && p.lastName.trim().toLowerCase() === lastName.toLowerCase()
      && (!spec.birthDate || (p.birthDate ?? '').slice(0, 10) === spec.birthDate.slice(0, 10)),
    );
    if (exact) return { person: exact, created: false };
  } catch {
    // Treat search failures as "not found" so the create below surfaces the real error.
  }

  // Tournament-imported players rarely have contact or address info, so we omit both blocks
  // entirely when there's nothing to send. The backend treats them as optional and skips
  // persisting the owned types when null, which avoids the historical "Email is required" and
  // "Country is required" validation failures triggered by sending empty-string fields.
  const data: PersonFormData = {
    firstName,
    lastName,
    birthDate: spec.birthDate ?? null,
    isRegistered: false,
    role: PersonRole.User,
    contactInfo: email && email.length > 0
      ? { email, phone: '', alternativePhone: null }
      : undefined,
  };
  const created = await personApi.create(data);
  return { person: created, created: true };
}

async function ensureFloorballPlayer(personId: string, personFullName: string): Promise<{ id: string; created: boolean }> {
  // The /api/FloorballPlayer endpoint has no "by personId" lookup, so narrow the search
  // by the person's name and then match on personId server-side.
  const lookup = async (): Promise<{ id: string; created: false } | null> => {
    try {
      const list = await floorballPlayerService.getAll({ searchTerm: personFullName, pageSize: 50 });
      const match = (list.data ?? []).find((p) => p.personId === personId);
      if (match) return { id: match.id, created: false };
    } catch {
      // ignore — let create attempt surface the real error
    }
    return null;
  };

  const existing = await lookup();
  if (existing) return existing;

  try {
    const created = await floorballPlayerService.create({ personId });
    return { id: created.id, created: true };
  } catch (err) {
    // Re-check in case of race condition / "already exists" rejection.
    const retry = await lookup();
    if (retry) return retry;
    throw err;
  }
}

async function findExistingTeam(name: string): Promise<{ id: string; name: string } | null> {
  try {
    const lookup = await floorballTeamNameSearchService.getTeamNames(name);
    return (lookup.data ?? []).find((x) => x.name.toLowerCase() === name.toLowerCase()) ?? null;
  } catch {
    // Treat search errors as "not found" — the create call will surface the real problem.
    return null;
  }
}

/**
 * Many of the underlying services throw an `Error` whose message is the raw response body —
 * which for ASP.NET validation failures is a JSON string like
 * `{"title":"One or more validation errors occurred.","errors":["Division level must be ..."]}`.
 * Pull out the useful bits so the log/modal stays readable.
 */
function prettifyError(err: unknown): string {
  const raw = err instanceof Error ? err.message : String(err);
  const trimmed = raw.trim();
  if (!trimmed.startsWith('{')) return raw;
  try {
    const parsed: unknown = JSON.parse(trimmed);
    if (parsed && typeof parsed === 'object') {
      const obj = parsed as { title?: unknown; errors?: unknown; message?: unknown; detail?: unknown };
      const parts: string[] = [];
      if (typeof obj.title === 'string') parts.push(obj.title);
      else if (typeof obj.message === 'string') parts.push(obj.message);
      else if (typeof obj.detail === 'string') parts.push(obj.detail);
      if (Array.isArray(obj.errors)) {
        for (const item of obj.errors) {
          if (typeof item === 'string') parts.push(item);
        }
      } else if (obj.errors && typeof obj.errors === 'object') {
        // ProblemDetails: { errors: { Field: ["msg1", "msg2"] } }
        for (const value of Object.values(obj.errors as Record<string, unknown>)) {
          if (Array.isArray(value)) {
            for (const v of value) if (typeof v === 'string') parts.push(v);
          } else if (typeof value === 'string') {
            parts.push(value);
          }
        }
      }
      if (parts.length > 0) return parts.join(' — ');
    }
  } catch {
    // fall through
  }
  return raw;
}

function composeVenue(baseVenue: string | null, field: string | null): string | undefined {
  if (baseVenue && field) return `${baseVenue} - Kenttä ${field}`;
  if (baseVenue) return baseVenue;
  if (field) return `Kenttä ${field}`;
  return undefined;
}

function matchLabel(m: TournamentImportMatch): string {
  const num = m.matchNumber ? `#${m.matchNumber} ` : '';
  return `${num}${m.homeTeamName} vs ${m.awayTeamName}`;
}

function phaseForRecord(r: CreatedRecord): ImportPhase {
  switch (r.kind) {
    case 'match': return 'matches';
    case 'group': return 'groups';
    case 'tournament': return 'tournament';
    case 'team-player': return 'players';
    case 'player': return 'players';
    case 'person': return 'players';
    case 'team': return 'teams';
    case 'division': return 'division';
    case 'club': return 'clubs';
  }
}

async function deleteRecord(r: CreatedRecord): Promise<void> {
  switch (r.kind) {
    case 'match':
      await floorballMatchService.delete(r.id);
      return;
    case 'group':
      await floorballTournamentService.removeGroup(r.tournamentId, r.groupId);
      return;
    case 'tournament':
      await floorballTournamentService.delete(r.id);
      return;
    case 'team-player':
      // Undo only the roster assignment; the player entity itself is removed by the
      // 'player' record if we created it.
      await floorballTeamService.removePlayerFromTeam(r.teamId, r.playerId);
      return;
    case 'player':
      await floorballPlayerService.delete(r.id);
      return;
    case 'person':
      await personApi.delete(r.id);
      return;
    case 'team':
      await floorballTeamService.delete(r.id);
      return;
    case 'division':
      await divisionService.delete(r.id);
      return;
    case 'club':
      await clubService.remove(r.id);
      return;
  }
}
