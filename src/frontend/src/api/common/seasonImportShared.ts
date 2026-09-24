import i18n from '../../i18n/i18n';
import type { Club } from './clubService';
import { clubService } from './clubService';
import { divisionService } from './divisionService';
import type { DivisionType } from '../../types/common/divisionType';
import type { SportsCategory } from '../../types/common/sports';
import { personApi } from '../admin/personApi';
import type { Person, PersonFormData } from '../../types/admin/personTypes';
import { PersonRole } from '../../types/admin/personTypes';
import type {
  SeasonImportCallbacks,
  SeasonImportClub,
  SeasonImportCreatedRecord,
  SeasonImportDryRunCounts,
  SeasonImportError,
  SeasonImportMatch,
  SeasonImportPayloadBase,
  SeasonImportPhase,
  SeasonImportPreview,
  SeasonImportPlayerAdapters,
  SeasonImportSummary,
  SeasonImportTeamPlayer,
  SeasonTeamCategory,
  TeamRosterSnapshot,
} from '../../types/common/seasonImportTypes';
import {
  SEASON_TEAM_CATEGORIES,
  SeasonImportAbortedError,
} from '../../types/common/seasonImportTypes';

const DIVISION_DEFAULT_DESCRIPTION = 'Auto-created by season JSON import.';
const DIVISION_DEFAULT_LEVEL = 1;
const JERSEY_NUMBER_MIN = 1;
const JERSEY_NUMBER_MAX = 99;

export function emptySeasonImportSummary(): SeasonImportSummary {
  return {
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
    seasonId: null,
    seasonName: null,
    seasonAssignments: 0,
    matchesCreated: 0,
    errors: [],
    created: [],
    fatal: false,
    aborted: false,
  };
}

export function getSeasonDryRunCounts(payload: SeasonImportPayloadBase): SeasonImportDryRunCounts {
  const players = payload.teams.reduce((sum, team) => sum + (team.players?.length ?? 0), 0);
  return {
    clubs: payload.clubs.length,
    divisions: payload.divisions.length,
    teams: payload.teams.length,
    players,
    assignments: payload.teams.length,
    matches: payload.matches.length,
  };
}

export function buildSeasonImportPreview(payload: SeasonImportPayloadBase): SeasonImportPreview {
  const matchCountByTeam = new Map<string, number>();
  for (const team of payload.teams) {
    matchCountByTeam.set(team.name, 0);
  }
  for (const match of payload.matches) {
    matchCountByTeam.set(match.homeTeamName, (matchCountByTeam.get(match.homeTeamName) ?? 0) + 1);
    matchCountByTeam.set(match.awayTeamName, (matchCountByTeam.get(match.awayTeamName) ?? 0) + 1);
  }

  const teamsByDivision = new Map<string, SeasonImportPreview['divisions'][number]['teams']>();
  for (const team of payload.teams) {
    const list = teamsByDivision.get(team.divisionName) ?? [];
    list.push({
      name: team.name,
      clubName: team.clubName,
      matchCount: matchCountByTeam.get(team.name) ?? 0,
      playerCount: team.players?.length ?? 0,
    });
    teamsByDivision.set(team.divisionName, list);
  }

  const divisions: SeasonImportPreview['divisions'] = payload.divisions.map((division) => ({
    name: division.name,
    level: typeof division.level === 'number' ? division.level : null,
    teams: teamsByDivision.get(division.name) ?? [],
  }));

  const knownDivisionNames = new Set(payload.divisions.map((division) => division.name));
  for (const [divisionName, teams] of teamsByDivision) {
    if (!knownDivisionNames.has(divisionName)) {
      divisions.push({ name: divisionName, level: null, teams });
    }
  }

  let firstMatchAt: string | null = null;
  let lastMatchAt: string | null = null;
  let firstMs = Number.POSITIVE_INFINITY;
  let lastMs = Number.NEGATIVE_INFINITY;
  const venues = new Set<string>();
  let matchesOutsideSeason = 0;
  let matchesWithoutOwnVenue = 0;
  const seasonStart = payload.season.startDate;
  const seasonEnd = payload.season.endDate;

  for (const match of payload.matches) {
    const parsed = Date.parse(match.scheduledDateTime);
    if (!Number.isNaN(parsed)) {
      if (parsed < firstMs) {
        firstMs = parsed;
        firstMatchAt = match.scheduledDateTime;
      }
      if (parsed > lastMs) {
        lastMs = parsed;
        lastMatchAt = match.scheduledDateTime;
      }
    }
    const matchDay = match.scheduledDateTime.slice(0, 10);
    if (matchDay < seasonStart || matchDay > seasonEnd) {
      matchesOutsideSeason += 1;
    }
    const venue = (match.venue ?? '').trim();
    if (venue.length > 0) {
      venues.add(venue);
    } else {
      matchesWithoutOwnVenue += 1;
    }
  }

  return {
    startDate: seasonStart,
    endDate: seasonEnd,
    divisions,
    matchCount: payload.matches.length,
    firstMatchAt,
    lastMatchAt,
    venues: [...venues].sort((left, right) => left.localeCompare(right, 'fi')),
    teamsWithoutMatches: payload.teams.filter((team) => (matchCountByTeam.get(team.name) ?? 0) === 0).map((team) => team.name),
    matchesOutsideSeason,
    matchesWithoutOwnVenue,
  };
}

export function inferSeasonTeamCategory(payload: SeasonImportPayloadBase): SeasonTeamCategory {
  const fromSeason = payload.season.teamCategory;
  if (fromSeason && isSeasonTeamCategory(fromSeason)) {
    return fromSeason;
  }
  const fromTeam = payload.teams.find((team) => team.category)?.category;
  if (fromTeam && isSeasonTeamCategory(fromTeam)) {
    return fromTeam;
  }
  return 'Adult';
}

export function reportDuplicateSeasonName(
  seasonName: string,
  existingNames: readonly string[],
  i18nKey: string,
  reportFatal: (phase: SeasonImportPhase, label: string, err: unknown) => void,
): boolean {
  if (!existingNames.some((existing) => existing === seasonName)) {
    return false;
  }

  reportFatal(
    'season',
    `Season "${seasonName}"`,
    new Error(
      i18n.t(i18nKey, {
        name: seasonName,
        defaultValue: `A season with the name '${seasonName}' already exists.`,
      }),
    ),
  );
  return true;
}

export function isSeasonTeamCategory(value: string | undefined): value is SeasonTeamCategory {
  return typeof value === 'string' && (SEASON_TEAM_CATEGORIES as readonly string[]).includes(value);
}

export function validateSeasonPayload(
  payload: unknown,
): { valid: true; payload: SeasonImportPayloadBase } | { valid: false; errors: string[] } {
  const errors: string[] = [];

  if (!payload || typeof payload !== 'object') {
    return { valid: false, errors: ['Uploaded file is not a JSON object.'] };
  }

  const parsed = payload as Partial<SeasonImportPayloadBase>;
  if (!parsed.season || typeof parsed.season !== 'object') {
    errors.push('Missing required "season" section.');
  } else {
    if (!parsed.season.name) errors.push('season.name is required.');
    if (!parsed.season.startDate) errors.push('season.startDate is required.');
    if (!parsed.season.endDate) errors.push('season.endDate is required.');
  }

  if (!Array.isArray(parsed.clubs)) errors.push('"clubs" must be an array.');
  if (!Array.isArray(parsed.divisions)) errors.push('"divisions" must be an array.');
  if (!Array.isArray(parsed.teams)) errors.push('"teams" must be an array.');
  if (!Array.isArray(parsed.matches)) errors.push('"matches" must be an array.');

  if (errors.length > 0) {
    return { valid: false, errors };
  }

  if ((parsed.divisions ?? []).length === 0) {
    errors.push('At least one division is required.');
  }

  const teamNames = new Set((parsed.teams ?? []).map((team) => team.name));
  const divisionNames = new Set((parsed.divisions ?? []).map((division) => division.name));

  for (const team of parsed.teams ?? []) {
    if (!team.name) errors.push('A team is missing its name.');
    if (!team.clubName) errors.push(`Team "${team.name ?? '?'}" is missing clubName.`);
    if (!team.divisionName) {
      errors.push(`Team "${team.name ?? '?'}" is missing divisionName.`);
    } else if (!divisionNames.has(team.divisionName)) {
      errors.push(`Team "${team.name}" references unknown division "${team.divisionName}".`);
    }

    if (team.players && team.players.length > 0) {
      for (let index = 0; index < team.players.length; index += 1) {
        const player = team.players[index];
        const hasName =
          typeof player.firstName === 'string' &&
          player.firstName.trim().length > 0 &&
          typeof player.lastName === 'string' &&
          player.lastName.trim().length > 0;
        const hasEmail = typeof player.personEmail === 'string' && player.personEmail.trim().length > 0;
        if (!hasName && !hasEmail) {
          errors.push(
            `Team "${team.name}" player #${index + 1} is missing both name and email — at least one is required.`,
          );
        }
      }
    }
  }

  for (const match of parsed.matches ?? []) {
    if (!match.homeTeamName || !teamNames.has(match.homeTeamName)) {
      errors.push(`Match #${match.matchNumber ?? '?'} references unknown home team "${match.homeTeamName}".`);
    }
    if (!match.awayTeamName || !teamNames.has(match.awayTeamName)) {
      errors.push(`Match #${match.matchNumber ?? '?'} references unknown away team "${match.awayTeamName}".`);
    }
    if (match.divisionName && !divisionNames.has(match.divisionName)) {
      errors.push(`Match #${match.matchNumber ?? '?'} references unknown division "${match.divisionName}".`);
    }
    if (!match.scheduledDateTime || Number.isNaN(Date.parse(match.scheduledDateTime))) {
      errors.push(`Match #${match.matchNumber ?? '?'} has an invalid scheduledDateTime: "${match.scheduledDateTime}".`);
    }
  }

  if (errors.length > 0) {
    return { valid: false, errors };
  }
  return { valid: true, payload: payload as SeasonImportPayloadBase };
}

export function createImportRuntime(summary: SeasonImportSummary, callbacks: SeasonImportCallbacks) {
  const reportFatal = (phase: SeasonImportPhase, label: string, err: unknown): void => {
    const message = err instanceof SeasonImportAbortedError ? err.message : prettifyError(err);
    if (err instanceof SeasonImportAbortedError) {
      summary.aborted = true;
    } else {
      summary.fatal = true;
    }
    const error: SeasonImportError = { phase, label, message, fatal: true };
    summary.errors.push(error);
    callbacks.onError(error);
  };

  const checkAbort = (): boolean => {
    if (callbacks.shouldAbort()) {
      reportFatal('validate', 'Aborted', new SeasonImportAbortedError());
      return true;
    }
    return false;
  };

  return { reportFatal, checkAbort };
}

export async function ensureClub(club: SeasonImportClub): Promise<{ club: Club; created: boolean }> {
  const matches = await clubService.searchByName(club.name);
  const exact = matches.find((item) => item.name.toLowerCase() === club.name.toLowerCase());
  if (exact) {
    return { club: exact, created: false };
  }

  try {
    const created = await clubService.create({
      name: club.name,
      city: club.city ?? undefined,
      country: club.country ?? undefined,
      websiteUrl: club.websiteUrl ?? undefined,
      logoUrl: club.logoUrl ?? undefined,
      contactEmail: club.contactEmail ?? undefined,
    });
    return { club: created, created: true };
  } catch (err) {
    const retry = await clubService.searchByName(club.name);
    const fallback = retry.find((item) => item.name.toLowerCase() === club.name.toLowerCase());
    if (fallback) {
      return { club: fallback, created: false };
    }
    throw err;
  }
}

export async function ensureDivision(
  name: string,
  sportType: SportsCategory,
  level: number | undefined,
): Promise<{ division: DivisionType; created: boolean }> {
  const existing = await divisionService.getBySportType(sportType, false);
  const match = (existing.data ?? []).find((item) => item.name.toLowerCase() === name.toLowerCase());
  if (match) {
    return { division: match, created: false };
  }

  const created = await divisionService.create({
    name,
    description: DIVISION_DEFAULT_DESCRIPTION,
    level: Number.isInteger(level) && (level ?? 0) >= 1 && (level ?? 0) <= 10 ? (level as number) : DIVISION_DEFAULT_LEVEL,
    sportType,
  });
  if (!created.data) {
    throw new Error(`Division "${name}" was created but the API returned no payload.`);
  }
  return { division: created.data, created: true };
}

export async function importClubs(
  clubs: SeasonImportClub[],
  summary: SeasonImportSummary,
  callbacks: SeasonImportCallbacks,
  checkAbort: () => boolean,
  reportFatal: (phase: SeasonImportPhase, label: string, err: unknown) => void,
): Promise<Map<string, Club> | null> {
  const clubByName = new Map<string, Club>();
  for (let index = 0; index < clubs.length; index += 1) {
    if (checkAbort()) return null;
    const club = clubs[index];
    try {
      const result = await ensureClub(club);
      clubByName.set(club.name, result.club);
      if (result.created) {
        summary.clubsCreated += 1;
        summary.created.push({ kind: 'club', id: result.club.id, label: club.name });
        callbacks.onStep({
          phase: 'clubs',
          index,
          total: clubs.length,
          label: `Created club "${club.name}"`,
          status: 'created',
        });
      } else {
        summary.clubsExisting += 1;
        callbacks.onStep({
          phase: 'clubs',
          index,
          total: clubs.length,
          label: `Found existing club "${club.name}"`,
          status: 'existing',
        });
      }
    } catch (err) {
      reportFatal('clubs', `Club "${club.name}"`, err);
      return null;
    }
  }
  return clubByName;
}

export async function importDivisions(
  namesAndLevels: Array<{ name: string; level?: number }>,
  sportType: SportsCategory,
  summary: SeasonImportSummary,
  callbacks: SeasonImportCallbacks,
  checkAbort: () => boolean,
  reportFatal: (phase: SeasonImportPhase, label: string, err: unknown) => void,
): Promise<Map<string, DivisionType> | null> {
  const divisionByName = new Map<string, DivisionType>();
  for (let index = 0; index < namesAndLevels.length; index += 1) {
    if (checkAbort()) return null;
    const item = namesAndLevels[index];
    try {
      const result = await ensureDivision(item.name, sportType, item.level);
      divisionByName.set(item.name, result.division);
      if (result.created) {
        summary.divisionsCreated += 1;
        summary.created.push({ kind: 'division', id: result.division.id, label: item.name });
        callbacks.onStep({
          phase: 'division',
          index,
          total: namesAndLevels.length,
          label: `Created division "${item.name}"`,
          status: 'created',
        });
      } else {
        summary.divisionsExisting += 1;
        callbacks.onStep({
          phase: 'division',
          index,
          total: namesAndLevels.length,
          label: `Found existing division "${item.name}"`,
          status: 'existing',
        });
      }
    } catch (err) {
      reportFatal('division', `Division "${item.name}"`, err);
      return null;
    }
  }
  return divisionByName;
}

export function buildPlayerNameKey(
  firstName: string | null | undefined,
  lastName: string | null | undefined,
): string {
  const first = (firstName ?? '').trim().toLowerCase().replace(/\s+/g, ' ');
  const last = (lastName ?? '').trim().toLowerCase().replace(/\s+/g, ' ');
  if (first.length === 0 && last.length === 0) return '';
  return `${first}|${last}`;
}

export function emptyRosterSnapshot(): TeamRosterSnapshot {
  return {
    playerIds: new Set<string>(),
    jerseyNumbers: new Set<number>(),
    nameKeys: new Set<string>(),
    personIds: new Set<string>(),
  };
}

export function collectReservedJerseyNumbers(remainingPlayers: SeasonImportTeamPlayer[]): Set<number> {
  const reserved = new Set<number>();
  for (const player of remainingPlayers) {
    if (typeof player.jerseyNumber === 'number' && Number.isFinite(player.jerseyNumber) && player.jerseyNumber > 0) {
      reserved.add(player.jerseyNumber);
    }
  }
  return reserved;
}

export function pickJerseyNumber(
  preferred: number | undefined,
  usedNumbers: Set<number>,
  reservedNumbers: Set<number>,
): { number: number | undefined; substituted: boolean } {
  if (preferred === undefined) {
    return { number: undefined, substituted: false };
  }
  if (!usedNumbers.has(preferred)) {
    return { number: preferred, substituted: false };
  }

  const tryRange = (start: number, endExclusive: number): number | undefined => {
    for (let value = start; value < endExclusive; value += 1) {
      if (value === preferred) continue;
      if (value < JERSEY_NUMBER_MIN || value > JERSEY_NUMBER_MAX) continue;
      if (usedNumbers.has(value) || reservedNumbers.has(value)) continue;
      return value;
    }
    return undefined;
  };

  const forward = tryRange(preferred + 1, JERSEY_NUMBER_MAX + 1);
  if (forward !== undefined) return { number: forward, substituted: true };
  const wrap = tryRange(JERSEY_NUMBER_MIN, preferred);
  if (wrap !== undefined) return { number: wrap, substituted: true };
  return { number: undefined, substituted: true };
}

export function isJerseyNumberConflict(err: unknown): boolean {
  const message = err instanceof Error ? err.message : String(err);
  return /jersey\s*number/i.test(message) && /assigned|in\s*use|conflict|already/i.test(message);
}

export async function ensurePerson(
  spec: SeasonImportTeamPlayer,
): Promise<{ person: Person; created: boolean }> {
  const email = spec.personEmail?.trim();
  if (email && email.length > 0) {
    try {
      const existing = await personApi.getByEmail(email);
      if (existing) return { person: existing, created: false };
    } catch {
      // Fall through to name search.
    }
  }

  const firstName = (spec.firstName ?? '').trim();
  const lastName = (spec.lastName ?? '').trim();
  if (firstName.length === 0 || lastName.length === 0) {
    throw new Error('Player is missing firstName/lastName and personEmail did not resolve.');
  }

  try {
    const matches = await personApi.search(`${firstName} ${lastName}`, 1, 25);
    const exact = (matches.data ?? []).find(
      (person) =>
        person.firstName.trim().toLowerCase() === firstName.toLowerCase() &&
        person.lastName.trim().toLowerCase() === lastName.toLowerCase() &&
        (!spec.birthDate || (person.birthDate ?? '').slice(0, 10) === spec.birthDate.slice(0, 10)),
    );
    if (exact) return { person: exact, created: false };
  } catch {
    // Treat search failures as not found.
  }

  const data: PersonFormData = {
    firstName,
    lastName,
    birthDate: spec.birthDate ?? null,
    isRegistered: false,
    role: PersonRole.User,
    contactInfo: email && email.length > 0 ? { email, phone: '', alternativePhone: null } : undefined,
  };
  const created = await personApi.create(data);
  return { person: created, created: true };
}

export async function importTeamPlayers(
  teamName: string,
  teamId: string,
  players: SeasonImportTeamPlayer[],
  adapters: SeasonImportPlayerAdapters,
  summary: SeasonImportSummary,
  callbacks: SeasonImportCallbacks,
  startIndex: number,
  totalPlayerOps: number,
  checkAbort: () => boolean,
): Promise<number> {
  let playerIndex = startIndex;
  const roster = await adapters.loadRoster(teamId);
  for (let index = 0; index < players.length; index += 1) {
    if (checkAbort()) return playerIndex;
    const spec = players[index];
    const reserved = collectReservedJerseyNumbers(players.slice(index + 1));
    const label = `${(spec.firstName ?? '').trim()} ${(spec.lastName ?? '').trim()}`.trim() || spec.personEmail || 'player';
    try {
      await importOnePlayer(spec, teamId, teamName, label, roster, reserved, adapters, summary, callbacks, {
        index: playerIndex,
        total: totalPlayerOps,
      });
    } catch (err) {
      const error: SeasonImportError = {
        phase: 'players',
        label: `${teamName} / ${label}`,
        message: prettifyError(err),
        fatal: false,
      };
      summary.errors.push(error);
      callbacks.onError(error);
    }
    playerIndex += 1;
  }
  return playerIndex;
}

async function importOnePlayer(
  spec: SeasonImportTeamPlayer,
  teamId: string,
  teamName: string,
  label: string,
  roster: TeamRosterSnapshot,
  reservedJerseyNumbers: Set<number>,
  adapters: SeasonImportPlayerAdapters,
  summary: SeasonImportSummary,
  callbacks: SeasonImportCallbacks,
  step: { index: number; total: number },
): Promise<void> {
  const candidateNameKey = buildPlayerNameKey(spec.firstName, spec.lastName);
  if (candidateNameKey.length > 0 && roster.nameKeys.has(candidateNameKey)) {
    summary.personsExisting += 1;
    summary.playersExisting += 1;
    callbacks.onStep({
      phase: 'players',
      index: step.index,
      total: step.total,
      label: `${teamName}: ${label} already on roster — skipped`,
      status: 'skipped',
    });
    return;
  }

  const personResult = await ensurePerson(spec);
  if (personResult.created) {
    summary.personsCreated += 1;
    summary.created.push({ kind: 'person', id: personResult.person.id, label });
  } else {
    summary.personsExisting += 1;
  }

  if (roster.personIds.has(personResult.person.id)) {
    summary.playersExisting += 1;
    if (candidateNameKey.length > 0) roster.nameKeys.add(candidateNameKey);
    callbacks.onStep({
      phase: 'players',
      index: step.index,
      total: step.total,
      label: `${teamName}: ${label} already on roster — skipped`,
      status: 'skipped',
    });
    return;
  }

  const position = adapters.normalizePosition(spec.position);
  const playerResult = await adapters.ensureSportPlayer(personResult.person.id, personResult.person.fullName, position);
  if (playerResult.created) {
    summary.playersCreated += 1;
    summary.created.push({ kind: 'player', id: playerResult.id, personId: personResult.person.id, label });
  } else {
    summary.playersExisting += 1;
  }

  if (roster.playerIds.has(playerResult.id)) {
    if (candidateNameKey.length > 0) roster.nameKeys.add(candidateNameKey);
    roster.personIds.add(personResult.person.id);
    callbacks.onStep({
      phase: 'players',
      index: step.index,
      total: step.total,
      label: `${teamName}: ${label} already on roster — skipped`,
      status: 'skipped',
    });
    return;
  }

  const preferred =
    typeof spec.jerseyNumber === 'number' && spec.jerseyNumber > 0 ? spec.jerseyNumber : undefined;
  let resolved = pickJerseyNumber(preferred, roster.jerseyNumbers, reservedJerseyNumbers);
  let added = false;
  let attempts = 0;
  while (!added && attempts < JERSEY_NUMBER_MAX + 2) {
    attempts += 1;
    try {
      const requestedForBackend =
        resolved.substituted && preferred !== undefined && preferred !== resolved.number ? preferred : undefined;
      await adapters.addPlayerToTeam(teamId, playerResult.id, position, resolved.number, requestedForBackend);
      added = true;
    } catch (err) {
      if (!isJerseyNumberConflict(err)) {
        const refreshed = await adapters.loadRoster(teamId);
        if (refreshed.playerIds.has(playerResult.id)) {
          roster.playerIds = refreshed.playerIds;
          roster.personIds = refreshed.personIds;
          roster.jerseyNumbers = refreshed.jerseyNumbers;
          roster.nameKeys = refreshed.nameKeys;
          added = true;
          break;
        }
        throw err;
      }
      const refreshed = await adapters.loadRoster(teamId);
      roster.playerIds = refreshed.playerIds;
      roster.personIds = refreshed.personIds;
      roster.jerseyNumbers = refreshed.jerseyNumbers;
      roster.nameKeys = refreshed.nameKeys;
      if (resolved.number !== undefined) {
        roster.jerseyNumbers.add(resolved.number);
      }
      const nextPick = pickJerseyNumber(preferred, roster.jerseyNumbers, reservedJerseyNumbers);
      if (nextPick.number === resolved.number || (nextPick.number === undefined && resolved.number === undefined)) {
        throw err;
      }
      resolved = nextPick;
    }
  }

  if (resolved.number !== undefined) roster.jerseyNumbers.add(resolved.number);
  roster.playerIds.add(playerResult.id);
  roster.personIds.add(personResult.person.id);
  if (candidateNameKey.length > 0) roster.nameKeys.add(candidateNameKey);
  summary.teamPlayerAssignments += 1;
  summary.created.push({ kind: 'team-player', teamId, playerId: playerResult.id, label: `${teamName} / ${label}` });

  let stepLabel = `Added ${label} to "${teamName}"`;
  if (resolved.substituted) {
    stepLabel =
      resolved.number !== undefined
        ? `Added ${label} to "${teamName}" — jersey #${preferred} taken, assigned #${resolved.number} instead`
        : `Added ${label} to "${teamName}" — no free jersey number, assigned without one`;
  }
  callbacks.onStep({
    phase: 'players',
    index: step.index,
    total: step.total,
    label: stepLabel,
    status: 'created',
  });
}

export function phaseForRecord(record: SeasonImportCreatedRecord): SeasonImportPhase {
  switch (record.kind) {
    case 'match':
      return 'matches';
    case 'season':
      return 'season';
    case 'team-player':
    case 'player':
    case 'person':
      return 'players';
    case 'team':
      return 'teams';
    case 'division':
      return 'division';
    case 'club':
      return 'clubs';
  }
}

export function prettifyError(err: unknown): string {
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
        for (const value of Object.values(obj.errors as Record<string, unknown>)) {
          if (Array.isArray(value)) {
            for (const item of value) if (typeof item === 'string') parts.push(item);
          } else if (typeof value === 'string') {
            parts.push(value);
          }
        }
      }
      if (parts.length > 0) return parts.join(' — ');
    }
  } catch {
    // keep raw
  }
  return raw;
}

export function composeVenue(
  defaultVenue: string | null | undefined,
  matchVenue: string | null | undefined,
  field: string | null | undefined,
): string | undefined {
  const base = nonEmpty(matchVenue) ?? nonEmpty(defaultVenue);
  const court = nonEmpty(field);
  if (base && court) return `${base} · ${court}`;
  if (base) return base;
  if (court) return court;
  return undefined;
}

export function matchLabel(match: SeasonImportMatch): string {
  const number = match.matchNumber ? `#${match.matchNumber} ` : '';
  return `${number}${match.homeTeamName} vs ${match.awayTeamName}`;
}

export function nonEmpty(value: string | null | undefined): string | null {
  const trimmed = (value ?? '').trim();
  return trimmed.length > 0 ? trimmed : null;
}

export async function revertCreatedRecords(
  records: SeasonImportCreatedRecord[],
  deleteRecord: (record: SeasonImportCreatedRecord) => Promise<void>,
  callbacks: Pick<SeasonImportCallbacks, 'onStep' | 'onError'>,
): Promise<{ deleted: number; failed: number }> {
  let deleted = 0;
  let failed = 0;
  const total = records.length;
  for (let index = records.length - 1; index >= 0; index -= 1) {
    const record = records[index];
    const reversedIndex = records.length - 1 - index;
    try {
      await deleteRecord(record);
      deleted += 1;
      callbacks.onStep({
        phase: phaseForRecord(record),
        index: reversedIndex,
        total,
        label: `Removed ${record.kind} "${record.label}"`,
        status: 'skipped',
      });
    } catch (err) {
      failed += 1;
      callbacks.onError({
        phase: phaseForRecord(record),
        label: `Revert ${record.kind} "${record.label}"`,
        message: prettifyError(err),
        fatal: false,
      });
    }
  }
  return { deleted, failed };
}
