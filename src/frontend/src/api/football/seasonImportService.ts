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
import type { FootballSeasonImportPayload } from '../../types/football/seasonImportTypes';
import { FootballPosition, TeamCategory } from '../../types/football/footballTypes';
import type { CreateFootballMatchRequest, FootballTeamRequest } from '../../types/football/footballTypes';
import {
  buildPlayerNameKey,
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
import {
  FOOTBALL_HOBBY_MATCH_RULE_DEFAULTS,
  FOOTBALL_HOBBY_STANDING_RULE_DEFAULTS,
  footballSeasonService,
} from './footballSeasonService';
import { footballTeamNameSearchService } from './footballTeamNameSearchService';
import { footballTeamService } from './footballTeamService';
import { footballMatchService } from './footballMatchService';
import { footballPlayerService } from './footballPlayerService';
import { personApi } from '../admin/personApi';
import { clubService } from '../common/clubService';
import { divisionService } from '../common/divisionService';

const FOOTBALL_POSITIONS = new Set<string>(Object.values(FootballPosition));

export function getDryRunCounts(payload: FootballSeasonImportPayload): SeasonImportDryRunCounts {
  return getSeasonDryRunCounts(payload);
}

export function inferTeamCategory(payload: FootballSeasonImportPayload): SeasonTeamCategory {
  return inferSeasonTeamCategory(payload);
}

export function validatePayload(
  payload: unknown,
): { valid: true; payload: FootballSeasonImportPayload } | { valid: false; errors: string[] } {
  const result = validateSeasonPayload(payload);
  if (!result.valid) return result;
  return { valid: true, payload: result.payload as FootballSeasonImportPayload };
}

export async function importSeason(
  payload: FootballSeasonImportPayload,
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
    SportsCategory.Football,
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
      const result = await ensureFootballTeam(team, clubByName, divisionByName, defaultVenue, defaultCategory);
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
        footballPlayerAdapters,
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
    const created = await footballSeasonService.create({
      name: payload.season.name,
      startDate: payload.season.startDate,
      endDate: payload.season.endDate,
      divisionIds: payload.divisions
        .map((division) => divisionByName.get(division.name)?.id)
        .filter((id): id is string => typeof id === 'string'),
      numberOfHalves: payload.season.numberOfHalves ?? FOOTBALL_HOBBY_MATCH_RULE_DEFAULTS.numberOfHalves,
      halfDurationMinutes: payload.season.halfDurationMinutes ?? FOOTBALL_HOBBY_MATCH_RULE_DEFAULTS.halfDurationMinutes,
      playersOnField: payload.season.playersOnField ?? FOOTBALL_HOBBY_MATCH_RULE_DEFAULTS.playersOnField,
      requireGoalkeeper: payload.season.requireGoalkeeper ?? FOOTBALL_HOBBY_MATCH_RULE_DEFAULTS.requireGoalkeeper,
      maxSubstitutions: payload.season.maxSubstitutions ?? FOOTBALL_HOBBY_MATCH_RULE_DEFAULTS.maxSubstitutions,
      requireOfficialsToStart:
        payload.season.requireOfficialsToStart ?? FOOTBALL_HOBBY_MATCH_RULE_DEFAULTS.requireOfficialsToStart,
      allowExtraTime: payload.season.allowExtraTime ?? FOOTBALL_HOBBY_MATCH_RULE_DEFAULTS.allowExtraTime,
      extraTimeHalfCount: payload.season.extraTimeHalfCount ?? FOOTBALL_HOBBY_MATCH_RULE_DEFAULTS.extraTimeHalfCount,
      extraTimeHalfDurationMinutes:
        payload.season.extraTimeHalfDurationMinutes ?? FOOTBALL_HOBBY_MATCH_RULE_DEFAULTS.extraTimeHalfDurationMinutes,
      allowPenaltyShootout:
        payload.season.allowPenaltyShootout ?? FOOTBALL_HOBBY_MATCH_RULE_DEFAULTS.allowPenaltyShootout,
      winPoints: payload.season.winPoints ?? FOOTBALL_HOBBY_STANDING_RULE_DEFAULTS.winPoints,
      drawPoints: payload.season.drawPoints ?? FOOTBALL_HOBBY_STANDING_RULE_DEFAULTS.drawPoints,
      lossPoints: payload.season.lossPoints ?? FOOTBALL_HOBBY_STANDING_RULE_DEFAULTS.lossPoints,
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
      await footballSeasonService.addTeamToSeasonDivision(seasonId, divisionId, teamId, 'CopyLatest');
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
      const request: CreateFootballMatchRequest = {
        competitionId: seasonId,
        homeTeamId: homeId,
        awayTeamId: awayId,
        scheduledDateTime: match.scheduledDateTime,
        venue: composeVenue(defaultVenue, match.venue, match.field),
      };
      const created = await footballMatchService.create(request);
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
  return revertCreatedRecords(records, deleteFootballRecord, callbacks);
}

async function ensureFootballTeam(
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
  if (!division) {
    throw new Error(`Division "${team.divisionName}" was not resolved before creating team "${team.name}".`);
  }

  const request: FootballTeamRequest = {
    name: team.name,
    clubId: club.id,
    divisionId: division.id,
    homeArena: nonEmpty(team.homeArena) ?? venueFallback ?? undefined,
    primaryJerseyColor: nonEmpty(team.primaryJerseyColor) ?? undefined,
    secondaryJerseyColor: nonEmpty(team.secondaryJerseyColor) ?? undefined,
    category: (team.category as TeamCategory | undefined) ?? (defaultCategory as TeamCategory),
  };
  try {
    const created = await footballTeamService.create(request);
    return { id: created.id, created: true };
  } catch (err) {
    const fallback = await findExistingTeam(team.name);
    if (fallback) return { id: fallback.id, created: false };
    throw err;
  }
}

async function findExistingTeam(name: string): Promise<{ id: string; name: string } | null> {
  try {
    const lookup = await footballTeamNameSearchService.getTeamNames(name);
    return (lookup.data ?? []).find((item) => item.name.toLowerCase() === name.toLowerCase()) ?? null;
  } catch {
    return null;
  }
}

const footballPlayerAdapters: SeasonImportPlayerAdapters = {
  defaultPosition: FootballPosition.Forward,
  normalizePosition: (position) =>
    position && FOOTBALL_POSITIONS.has(position) ? position : FootballPosition.Forward,
  loadRoster: async (teamId) => {
    try {
      const list = await footballPlayerService.getAll({ teamId, pageSize: 50 });
      const snapshot = emptyRosterSnapshot();
      for (const player of list.data ?? []) {
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
        const list = await footballPlayerService.getAll({ searchTerm: personFullName, pageSize: 50 });
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
      const created = await footballPlayerService.create({ personId });
      return { id: created.id, created: true };
    } catch (err) {
      const retry = await lookup();
      if (retry) return retry;
      throw err;
    }
  },
  addPlayerToTeam: async (teamId, playerId, position, jerseyNumber, requestedJerseyNumber) => {
    await footballTeamService.addPlayerToTeam(
      teamId,
      playerId,
      position as FootballPosition,
      jerseyNumber,
      requestedJerseyNumber,
    );
  },
};

async function deleteFootballRecord(record: SeasonImportCreatedRecord): Promise<void> {
  switch (record.kind) {
    case 'match':
      await footballMatchService.delete(record.id);
      return;
    case 'season':
      await footballSeasonService.delete(record.id);
      return;
    case 'team-player':
      await footballTeamService.removePlayerFromTeam(record.teamId, record.playerId);
      return;
    case 'player':
      await footballPlayerService.delete(record.id);
      return;
    case 'person':
      await personApi.delete(record.id);
      return;
    case 'team':
      await footballTeamService.delete(record.id);
      return;
    case 'division':
      await divisionService.delete(record.id);
      return;
    case 'club':
      await clubService.remove(record.id);
      return;
  }
}
