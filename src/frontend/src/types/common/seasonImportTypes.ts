/**
 * Shared types for the admin "Import season from JSON" feature.
 * Sport-specific payloads extend these shapes (match rules, positions, hockey seasonCode).
 */

export const SEASON_IMPORT_SCHEMA_VERSION = 'myleague-season-import/v1' as const;

export type SeasonTeamCategory = 'Adult' | 'Youth' | 'Women';

export const SEASON_TEAM_CATEGORIES: readonly SeasonTeamCategory[] = ['Adult', 'Youth', 'Women'];

export interface SeasonImportSeasonBase {
  name: string;
  /** ISO date `YYYY-MM-DD` (no time). */
  startDate: string;
  /** ISO date `YYYY-MM-DD` (no time). */
  endDate: string;
  teamCategory?: SeasonTeamCategory;
  /** Default hall used when a match has no venue of its own. */
  defaultVenue?: string | null;
}

export interface SeasonImportClub {
  name: string;
  city?: string | null;
  country?: string | null;
  websiteUrl?: string | null;
  logoUrl?: string | null;
  contactEmail?: string | null;
}

export interface SeasonImportDivision {
  name: string;
  /** 1..10. Defaults to 1 when omitted. */
  level?: number;
}

export interface SeasonImportTeamPlayer {
  firstName: string;
  lastName: string;
  personEmail?: string;
  birthDate?: string;
  /** Sport-specific position string. Invalid values fall back to the sport default. */
  position?: string;
  jerseyNumber?: number;
}

export interface SeasonImportTeamBase {
  name: string;
  clubName: string;
  divisionName: string;
  homeArena?: string | null;
  primaryJerseyColor?: string | null;
  secondaryJerseyColor?: string | null;
  category?: SeasonTeamCategory;
  players?: SeasonImportTeamPlayer[];
}

export interface SeasonImportMatch {
  matchNumber?: number | null;
  scheduledDateTime: string;
  field?: string | null;
  venue?: string | null;
  homeTeamName: string;
  awayTeamName: string;
  divisionName?: string | null;
}

export interface SeasonImportPayloadBase {
  $schema?: string;
  season: SeasonImportSeasonBase;
  clubs: SeasonImportClub[];
  divisions: SeasonImportDivision[];
  teams: SeasonImportTeamBase[];
  matches: SeasonImportMatch[];
}

export type SeasonImportPhase =
  | 'validate'
  | 'clubs'
  | 'division'
  | 'teams'
  | 'players'
  | 'season'
  | 'season-teams'
  | 'matches'
  | 'done';

export interface SeasonImportStep {
  phase: SeasonImportPhase;
  index: number;
  total: number;
  label: string;
  status: 'created' | 'existing' | 'skipped' | 'info';
}

export interface SeasonImportError {
  phase: SeasonImportPhase;
  label: string;
  message: string;
  fatal: boolean;
}

export type SeasonImportCreatedRecord =
  | { kind: 'match'; id: string; label: string }
  | { kind: 'season'; id: string; label: string }
  | { kind: 'team-player'; teamId: string; playerId: string; label: string }
  | { kind: 'player'; id: string; personId: string; label: string }
  | { kind: 'person'; id: string; label: string }
  | { kind: 'team'; id: string; label: string }
  | { kind: 'division'; id: string; label: string }
  | { kind: 'club'; id: string; label: string };

export interface SeasonImportSummary {
  clubsCreated: number;
  clubsExisting: number;
  divisionsCreated: number;
  divisionsExisting: number;
  teamsCreated: number;
  teamsExisting: number;
  personsCreated: number;
  personsExisting: number;
  playersCreated: number;
  playersExisting: number;
  teamPlayerAssignments: number;
  seasonId: string | null;
  seasonName: string | null;
  seasonAssignments: number;
  matchesCreated: number;
  errors: SeasonImportError[];
  created: SeasonImportCreatedRecord[];
  fatal: boolean;
  aborted: boolean;
}

export interface SeasonImportCallbacks {
  onStep: (step: SeasonImportStep) => void;
  onError: (err: SeasonImportError) => void;
  shouldAbort: () => boolean;
}

export interface SeasonImportOptions {
  defaultTeamCategory?: SeasonTeamCategory;
}

export interface SeasonImportDryRunCounts {
  clubs: number;
  divisions: number;
  teams: number;
  players: number;
  assignments: number;
  matches: number;
}

export class SeasonImportAbortedError extends Error {
  constructor() {
    super('Import was aborted by the user.');
    this.name = 'SeasonImportAbortedError';
  }
}

export interface TeamRosterSnapshot {
  playerIds: Set<string>;
  jerseyNumbers: Set<number>;
  nameKeys: Set<string>;
  personIds: Set<string>;
}

export interface SeasonImportPlayerAdapters {
  loadRoster: (teamId: string) => Promise<TeamRosterSnapshot>;
  ensureSportPlayer: (
    personId: string,
    personFullName: string,
    position: string,
  ) => Promise<{ id: string; created: boolean }>;
  addPlayerToTeam: (
    teamId: string,
    playerId: string,
    position: string,
    jerseyNumber: number | undefined,
    requestedJerseyNumber: number | undefined,
  ) => Promise<void>;
  defaultPosition: string;
  normalizePosition: (position: string | undefined) => string;
}
