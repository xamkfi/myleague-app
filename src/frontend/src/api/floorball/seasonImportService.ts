import type { Club } from '../common/clubService';
import type { DivisionType } from '../../types/common/divisionType';
import { SportsCategory } from '../../types/common/sports';
import type {
  SeasonImportCallbacks,
  SeasonImportCreatedRecord,
  SeasonImportDryRunCounts,
  SeasonImportOptions,
  SeasonImportPlayerAdapters,
  SeasonImportSummary,
  SeasonImportTeamBase,
  SeasonTeamCategory,
} from '../../types/common/seasonImportTypes';
import type { FloorballSeasonImportPayload } from '../../types/floorball/seasonImportTypes';
import { FloorballPosition, TeamCategory } from '../../types/floorball/floorballTypes';
import type { CreateFloorballMatchRequest, FloorballTeamRequest } from '../../types/floorball/floorballTypes';
import {
  composeVenue,
  createImportRuntime,
  emptyRosterSnapshot,
  emptySeasonImportSummary,
  ensureClub,
  getSeasonDryRunCounts,
  inferSeasonTeamCategory,
  importClubs,
  reportDuplicateSeasonName,
  importDivisions,
  importTeamPlayers,
  matchLabel,
  nonEmpty,
  revertCreatedRecords,
  validateSeasonPayload,
} from '../common/seasonImportShared';
import { buildPlayerNameKey } from '../common/seasonImportShared';
import { floorballTeamNameSearchService } from './floorballTeamNameSearchService';
import { floorballTeamService } from './floorballTeamService';
import { floorballSeasonService } from './floorballSeasonService';
import { floorballMatchService } from './floorballMatchService';
import { floorballPlayerService } from './floorballPlayerService';
import { personApi } from '../admin/personApi';
import { clubService } from '../common/clubService';
import { divisionService } from '../common/divisionService';

const FLOORBALL_POSITIONS = new Set<string>(Object.values(FloorballPosition));

export function getDryRunCounts(payload: FloorballSeasonImportPayload): SeasonImportDryRunCounts {
  return getSeasonDryRunCounts(payload);
}

export function inferTeamCategory(payload: FloorballSeasonImportPayload): SeasonTeamCategory {
  return inferSeasonTeamCategory(payload);
}

export function validatePayload(
  payload: unknown,
): { valid: true; payload: FloorballSeasonImportPayload } | { valid: false; errors: string[] } {
  const result = validateSeasonPayload(payload);
  if (!result.valid) return result;
  return { valid: true, payload: result.payload as FloorballSeasonImportPayload };
}

export async function importSeason(
  payload: FloorballSeasonImportPayload,
  callbacks: SeasonImportCallbacks,
  options: SeasonImportOptions = {},
): Promise<SeasonImportSummary> {
  const summary = emptySeasonImportSummary();
  const { reportFatal, checkAbort } = createImportRuntime(summary, callbacks);
  const defaultCategory = options.defaultTeamCategory ?? inferSeasonTeamCategory(payload);

  if (checkAbort()) return summary;
  try {
    const existing = await floorballSeasonService.getAll(true);
    const existingNames = existing.data.map((season) => season.name);
    if (
      reportDuplicateSeasonName(
        payload.season.name,
        existingNames,
        'floorball.seasons.import.duplicateName',
        reportFatal,
      )
    ) {
      return summary;
    }
  } catch (err) {
    reportFatal('season', `Season "${payload.season.name}"`, err);
    return summary;
  }

  const clubByName = await importClubs(payload.clubs, summary, callbacks, checkAbort, reportFatal);
  if (!clubByName || summary.fatal || summary.aborted) return summary;

  const divisionByName = await importDivisions(
    payload.divisions,
    SportsCategory.Floorball,
    summary,
    callbacks,
    checkAbort,
    reportFatal,
  );
  if (!divisionByName || summary.fatal || summary.aborted) return summary;

  const teamIdByName = new Map<string, string>();
  const defaultVenue = nonEmpty(payload.season.defaultVenue);
  for (let index = 0; index < payload.teams.length; index += 1) {
    if (checkAbort()) return summary;
    const team = payload.teams[index];
    try {
      const result = await ensureFloorballTeam(team, clubByName, divisionByName, defaultVenue, defaultCategory);
      teamIdByName.set(team.name, result.id);
      if (result.created) {
        summary.teamsCreated += 1;
        summary.created.push({ kind: 'team', id: result.id, label: team.name });
        callbacks.onStep({
          phase: 'teams',
          index,
          total: payload.teams.length,
          label: `Created team "${team.name}"`,
          status: 'created',
        });
      } else {
        summary.teamsExisting += 1;
        callbacks.onStep({
          phase: 'teams',
          index,
          total: payload.teams.length,
          label: `Found existing team "${team.name}"`,
          status: 'existing',
        });
      }
    } catch (err) {
      reportFatal('teams', `Team "${team.name}"`, err);
      return summary;
    }
  }

  const totalPlayerOps = payload.teams.reduce((sum, team) => sum + (team.players?.length ?? 0), 0);
  if (totalPlayerOps > 0) {
    let playerIndex = 0;
    for (const team of payload.teams) {
      const teamId = teamIdByName.get(team.name);
      if (!teamId || !team.players?.length) continue;
      playerIndex = await importTeamPlayers(
        team.name,
        teamId,
        team.players,
        floorballPlayerAdapters,
        summary,
        callbacks,
        playerIndex,
        totalPlayerOps,
        checkAbort,
      );
      if (summary.aborted) return summary;
    }
  }

  if (checkAbort()) return summary;
  let seasonId: string;
  try {
    const created = await floorballSeasonService.create({
      name: payload.season.name,
      startDate: payload.season.startDate,
      endDate: payload.season.endDate,
      divisionIds: payload.divisions
        .map((division) => divisionByName.get(division.name)?.id)
        .filter((id): id is string => typeof id === 'string'),
      numberOfPeriods: payload.season.numberOfPeriods ?? 2,
      periodDurationMinutes: payload.season.periodDurationMinutes ?? 15,
      allowOvertime: payload.season.allowOvertime ?? true,
      overtimeDurationMinutes: payload.season.overtimeDurationMinutes ?? 5,
      allowShootout: payload.season.allowShootout ?? true,
      teamCategory: defaultCategory,
    });
    seasonId = created.data.id;
    summary.seasonId = seasonId;
    summary.seasonName = created.data.name;
    summary.created.push({ kind: 'season', id: seasonId, label: created.data.name });
    callbacks.onStep({
      phase: 'season',
      index: 0,
      total: 1,
      label: `Created season "${created.data.name}"`,
      status: 'created',
    });
  } catch (err) {
    reportFatal('season', `Season "${payload.season.name}"`, err);
    return summary;
  }

  for (let index = 0; index < payload.teams.length; index += 1) {
    if (checkAbort()) return summary;
    const team = payload.teams[index];
    const teamId = teamIdByName.get(team.name);
    const divisionId = divisionByName.get(team.divisionName)?.id;
    if (!teamId || !divisionId) continue;
    try {
      await floorballSeasonService.addTeamToSeasonDivision(seasonId, divisionId, teamId, 'CopyLatest');
      summary.seasonAssignments += 1;
      callbacks.onStep({
        phase: 'season-teams',
        index,
        total: payload.teams.length,
        label: `Assigned "${team.name}" to ${team.divisionName}`,
        status: 'created',
      });
    } catch (err) {
      reportFatal('season-teams', `${team.name} → ${team.divisionName}`, err);
      return summary;
    }
  }

  for (let index = 0; index < payload.matches.length; index += 1) {
    if (checkAbort()) return summary;
    const match = payload.matches[index];
    const homeId = teamIdByName.get(match.homeTeamName);
    const awayId = teamIdByName.get(match.awayTeamName);
    if (!homeId || !awayId) {
      const error = {
        phase: 'matches' as const,
        label: matchLabel(match),
        message: `Cannot create match — missing team id (home=${match.homeTeamName}, away=${match.awayTeamName}).`,
        fatal: false,
      };
      summary.errors.push(error);
      callbacks.onError(error);
      continue;
    }
    try {
      const request: CreateFloorballMatchRequest = {
        competitionId: seasonId,
        homeTeamId: homeId,
        awayTeamId: awayId,
        scheduledDateTime: match.scheduledDateTime,
        venue: composeVenue(defaultVenue, match.venue, match.field),
      };
      const created = await floorballMatchService.create(request);
      summary.matchesCreated += 1;
      summary.created.push({ kind: 'match', id: created.data.id, label: matchLabel(match) });
      callbacks.onStep({
        phase: 'matches',
        index,
        total: payload.matches.length,
        label: `Created match ${matchLabel(match)}`,
        status: 'created',
      });
    } catch (err) {
      reportFatal('matches', matchLabel(match), err);
      return summary;
    }
  }

  callbacks.onStep({ phase: 'done', index: 1, total: 1, label: 'Import finished successfully', status: 'info' });
  return summary;
}

export async function revertImport(
  records: SeasonImportCreatedRecord[],
  callbacks: Pick<SeasonImportCallbacks, 'onStep' | 'onError'>,
): Promise<{ deleted: number; failed: number }> {
  return revertCreatedRecords(records, deleteFloorballRecord, callbacks);
}

async function ensureFloorballTeam(
  team: SeasonImportTeamBase,
  clubByName: Map<string, Club>,
  divisionByName: Map<string, DivisionType>,
  venueFallback: string | null,
  defaultCategory: SeasonTeamCategory,
): Promise<{ id: string; created: boolean }> {
  const existing = await findExistingTeam(team.name);
  if (existing) return { id: existing.id, created: false };

  let club = clubByName.get(team.clubName);
  if (!club) {
    const fallback = await ensureClub({ name: team.clubName });
    clubByName.set(team.clubName, fallback.club);
    club = fallback.club;
  }
  if (!club) {
    throw new Error(`Club "${team.clubName}" was not resolved before creating team "${team.name}".`);
  }

  const division = divisionByName.get(team.divisionName);
  if (!division) {
    throw new Error(`Division "${team.divisionName}" was not resolved before creating team "${team.name}".`);
  }

  const request: FloorballTeamRequest = {
    name: team.name,
    clubId: club.id,
    divisionId: division.id,
    homeArena: nonEmpty(team.homeArena) ?? venueFallback ?? undefined,
    primaryJerseyColor: nonEmpty(team.primaryJerseyColor) ?? undefined,
    secondaryJerseyColor: nonEmpty(team.secondaryJerseyColor) ?? undefined,
    category: (team.category as TeamCategory | undefined) ?? (defaultCategory as TeamCategory),
  };
  try {
    const created = await floorballTeamService.create(request);
    return { id: created.id, created: true };
  } catch (err) {
    const fallback = await findExistingTeam(team.name);
    if (fallback) return { id: fallback.id, created: false };
    throw err;
  }
}

async function findExistingTeam(name: string): Promise<{ id: string; name: string } | null> {
  try {
    const lookup = await floorballTeamNameSearchService.getTeamNames(name);
    return (lookup.data ?? []).find((item) => item.name.toLowerCase() === name.toLowerCase()) ?? null;
  } catch {
    return null;
  }
}

const floorballPlayerAdapters: SeasonImportPlayerAdapters = {
  defaultPosition: FloorballPosition.Forward,
  normalizePosition: (position) =>
    position && FLOORBALL_POSITIONS.has(position) ? position : FloorballPosition.Forward,
  loadRoster: async (teamId) => {
    try {
      const roster = await floorballPlayerService.getByTeamId(teamId);
      const snapshot = emptyRosterSnapshot();
      for (const player of roster) {
        snapshot.playerIds.add(player.id);
        if (player.personId) snapshot.personIds.add(player.personId);
        if (typeof player.jerseyNumber === 'number') snapshot.jerseyNumbers.add(player.jerseyNumber);
        const key = buildPlayerNameKey(player.person?.firstName, player.person?.lastName);
        if (key.length > 0) snapshot.nameKeys.add(key);
      }
      return snapshot;
    } catch {
      return emptyRosterSnapshot();
    }
  },
  ensureSportPlayer: async (personId, personFullName) => {
    const lookup = async (): Promise<{ id: string; created: false } | null> => {
      try {
        const list = await floorballPlayerService.getAll({ searchTerm: personFullName, pageSize: 50 });
        const match = (list.data ?? []).find((player) => player.personId === personId);
        if (match) return { id: match.id, created: false };
      } catch {
        // ignore
      }
      return null;
    };
    const existing = await lookup();
    if (existing) return existing;
    try {
      const created = await floorballPlayerService.create({ personId });
      return { id: created.id, created: true };
    } catch (err) {
      const retry = await lookup();
      if (retry) return retry;
      throw err;
    }
  },
  addPlayerToTeam: async (teamId, playerId, position, jerseyNumber, requestedJerseyNumber) => {
    await floorballTeamService.addPlayerToTeam(
      teamId,
      playerId,
      position as FloorballPosition,
      jerseyNumber,
      requestedJerseyNumber,
    );
  },
};

async function deleteFloorballRecord(record: SeasonImportCreatedRecord): Promise<void> {
  switch (record.kind) {
    case 'match':
      await floorballMatchService.delete(record.id);
      return;
    case 'season':
      await floorballSeasonService.delete(record.id);
      return;
    case 'team-player':
      await floorballTeamService.removePlayerFromTeam(record.teamId, record.playerId);
      return;
    case 'player':
      await floorballPlayerService.delete(record.id);
      return;
    case 'person':
      await personApi.delete(record.id);
      return;
    case 'team':
      await floorballTeamService.delete(record.id);
      return;
    case 'division':
      await divisionService.delete(record.id);
      return;
    case 'club':
      await clubService.remove(record.id);
      return;
  }
}
