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
import type { HockeySeasonImportPayload } from '../../types/hockey/seasonImportTypes';
import {
  HOCKEY_POSITIONS,
  type CreateHockeyTeamRequest,
  type HockeyCompetitionTeamDto,
  type HockeyPosition,
  type HockeySeasonDto,
  type HockeyTeamCategory,
} from '../../types/hockey/hockeyTypes';
import {
  composeVenue,
  createImportRuntime,
  emptyRosterSnapshot,
  emptySeasonImportSummary,
  ensureClub,
  getSeasonDryRunCounts,
  inferSeasonTeamCategory,
  importClubs,
  importDivisions,
  importTeamPlayers,
  matchLabel,
  nonEmpty,
  revertCreatedRecords,
  validateSeasonPayload,
} from '../common/seasonImportShared';
import { hockeySeasonService } from './hockeySeasonService';
import { hockeyTeamService } from './hockeyTeamService';
import { hockeyMatchService } from './hockeyMatchService';
import { hockeyPlayerService } from './hockeyPlayerService';
import { personApi } from '../admin/personApi';
import { clubService } from '../common/clubService';
import { divisionService } from '../common/divisionService';

const HOCKEY_POSITION_SET = new Set<string>(HOCKEY_POSITIONS);

export function getDryRunCounts(payload: HockeySeasonImportPayload): SeasonImportDryRunCounts {
  return getSeasonDryRunCounts(payload);
}

export function inferTeamCategory(payload: HockeySeasonImportPayload): SeasonTeamCategory {
  return inferSeasonTeamCategory(payload);
}

export function validatePayload(
  payload: unknown,
): { valid: true; payload: HockeySeasonImportPayload } | { valid: false; errors: string[] } {
  const result = validateSeasonPayload(payload);
  if (!result.valid) return result;
  return { valid: true, payload: result.payload as HockeySeasonImportPayload };
}

export async function importSeason(
  payload: HockeySeasonImportPayload,
  callbacks: SeasonImportCallbacks,
  options: SeasonImportOptions = {},
): Promise<SeasonImportSummary> {
  const summary = emptySeasonImportSummary();
  const { reportFatal, checkAbort } = createImportRuntime(summary, callbacks);
  const defaultCategory = options.defaultTeamCategory ?? inferSeasonTeamCategory(payload);

  const clubByName = await importClubs(payload.clubs, summary, callbacks, checkAbort, reportFatal);
  if (!clubByName || summary.fatal || summary.aborted) return summary;

  const divisionByName = await importDivisions(
    payload.divisions,
    SportsCategory.Icehockey,
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
      const result = await ensureHockeyTeam(team, clubByName, divisionByName, defaultVenue, defaultCategory);
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
        hockeyPlayerAdapters,
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
  let season: HockeySeasonDto;
  try {
    season = await hockeySeasonService.create({
      name: payload.season.name,
      startDate: toIsoDateTime(payload.season.startDate),
      endDate: toIsoDateTime(payload.season.endDate),
      seasonCode: nonEmpty(payload.season.seasonCode) ?? undefined,
      teamCategory: defaultCategory as HockeyTeamCategory,
    });
    summary.seasonId = season.id;
    summary.seasonName = season.name;
    summary.created.push({ kind: 'season', id: season.id, label: season.name });
    callbacks.onStep({
      phase: 'season',
      index: 0,
      total: 1,
      label: `Created season "${season.name}"`,
      status: 'created',
    });
  } catch (err) {
    reportFatal('season', `Season "${payload.season.name}"`, err);
    return summary;
  }

  const competitionDivisionIdByName = new Map<string, string>();
  for (let index = 0; index < payload.divisions.length; index += 1) {
    if (checkAbort()) return summary;
    const division = payload.divisions[index];
    const common = divisionByName.get(division.name);
    if (!common) continue;
    try {
      season = await hockeySeasonService.addDivision(season.id, common.id, division.name, index + 1);
      const added = (season.divisions ?? []).find(
        (item) => item.divisionId === common.id || item.name.toLowerCase() === division.name.toLowerCase(),
      );
      if (!added) {
        throw new Error(`Division "${division.name}" was not present on the season after addDivision.`);
      }
      competitionDivisionIdByName.set(division.name, added.id);
      callbacks.onStep({
        phase: 'season',
        index,
        total: payload.divisions.length,
        label: `Attached division "${division.name}"`,
        status: 'created',
      });
    } catch (err) {
      reportFatal('season', `Division "${division.name}"`, err);
      return summary;
    }
  }

  for (let index = 0; index < payload.teams.length; index += 1) {
    if (checkAbort()) return summary;
    const team = payload.teams[index];
    const teamId = teamIdByName.get(team.name);
    const competitionDivisionId = competitionDivisionIdByName.get(team.divisionName);
    if (!teamId || !competitionDivisionId) continue;
    try {
      const competitionTeam: HockeyCompetitionTeamDto = await hockeySeasonService.addTeam(season.id, teamId);
      await hockeySeasonService.addTeamToDivision(season.id, competitionDivisionId, competitionTeam.id);
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
    const homeTeam = payload.teams.find((team) => team.name === match.homeTeamName);
    const competitionDivisionId =
      (match.divisionName ? competitionDivisionIdByName.get(match.divisionName) : undefined) ??
      (homeTeam ? competitionDivisionIdByName.get(homeTeam.divisionName) : undefined);
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
      const created = await hockeyMatchService.create({
        scheduledStartTime: match.scheduledDateTime,
        matchType: 'League',
        competitionId: season.id,
        competitionDivisionId,
        venue: composeVenue(defaultVenue, match.venue, match.field),
      });
      await hockeyMatchService.assignTeams(created.id, homeId, awayId);
      summary.matchesCreated += 1;
      summary.created.push({ kind: 'match', id: created.id, label: matchLabel(match) });
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
  return revertCreatedRecords(records, deleteHockeyRecord, callbacks);
}

function toIsoDateTime(dateOnly: string): string {
  if (dateOnly.includes('T')) return dateOnly;
  return new Date(dateOnly).toISOString();
}

async function ensureHockeyTeam(
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

  const division = divisionByName.get(team.divisionName);
  const request: CreateHockeyTeamRequest = {
    name: team.name,
    clubId: club.id,
    teamCategory: (team.category as HockeyTeamCategory | undefined) ?? (defaultCategory as HockeyTeamCategory),
    divisionId: division?.id,
    homeArena: nonEmpty(team.homeArena) ?? venueFallback ?? undefined,
    primaryJerseyColor: nonEmpty(team.primaryJerseyColor) ?? undefined,
    secondaryJerseyColor: nonEmpty(team.secondaryJerseyColor) ?? undefined,
  };
  try {
    const created = await hockeyTeamService.create(request);
    return { id: created.id, created: true };
  } catch (err) {
    const fallback = await findExistingTeam(team.name);
    if (fallback) return { id: fallback.id, created: false };
    throw err;
  }
}

async function findExistingTeam(name: string): Promise<{ id: string; name: string } | null> {
  try {
    const page = await hockeyTeamService.getPaged({ searchTerm: name, page: 1, pageSize: 25 });
    return (page.data ?? []).find((item) => item.name.toLowerCase() === name.toLowerCase()) ?? null;
  } catch {
    return null;
  }
}

const hockeyPlayerAdapters: SeasonImportPlayerAdapters = {
  defaultPosition: 'Center',
  normalizePosition: (position) => {
    if (position && HOCKEY_POSITION_SET.has(position)) return position;
    if (position === 'Goalkeeper') return 'Goalie';
    if (position === 'Defender') return 'Defenseman';
    if (position === 'Forward') return 'RightWing';
    return 'Center';
  },
  loadRoster: async (teamId) => {
    try {
      const team = await hockeyTeamService.getById(teamId);
      const snapshot = emptyRosterSnapshot();
      for (const player of team.roster ?? []) {
        snapshot.playerIds.add(player.playerId);
        if (typeof player.jerseyNumber === 'number') snapshot.jerseyNumbers.add(player.jerseyNumber);
      }
      const players = await hockeyPlayerService.getAllPages({ teamId });
      for (const player of players) {
        if (player.personId) snapshot.personIds.add(player.personId);
      }
      return snapshot;
    } catch {
      return emptyRosterSnapshot();
    }
  },
  ensureSportPlayer: async (personId, _personFullName, position) => {
    const lookup = async (): Promise<{ id: string; created: false } | null> => {
      try {
        const page = await hockeyPlayerService.getPaged({ page: 1, pageSize: 50 });
        const match = (page.data ?? []).find((player) => player.personId === personId);
        if (match) return { id: match.id, created: false };
      } catch {
        // ignore
      }
      return null;
    };
    const existing = await lookup();
    if (existing) return existing;
    try {
      const created = await hockeyPlayerService.create({
        personId,
        primaryPosition: position as HockeyPosition,
      });
      return { id: created.id, created: true };
    } catch (err) {
      const retry = await lookup();
      if (retry) return retry;
      throw err;
    }
  },
  addPlayerToTeam: async (teamId, playerId, position, jerseyNumber) => {
    await hockeyTeamService.addPlayer(teamId, playerId, position as HockeyPosition, jerseyNumber);
  },
};

async function deleteHockeyRecord(record: SeasonImportCreatedRecord): Promise<void> {
  switch (record.kind) {
    case 'match':
      await hockeyMatchService.setStatus(record.id, 'Cancelled');
      return;
    case 'season':
      await hockeySeasonService.cancel(record.id);
      return;
    case 'team-player':
      await hockeyTeamService.removePlayer(record.teamId, record.playerId);
      return;
    case 'player':
      await hockeyPlayerService.delete(record.id);
      return;
    case 'person':
      await personApi.delete(record.id);
      return;
    case 'team':
      await hockeyTeamService.setActive(record.id, false);
      return;
    case 'division':
      await divisionService.delete(record.id);
      return;
    case 'club':
      await clubService.remove(record.id);
      return;
  }
}
