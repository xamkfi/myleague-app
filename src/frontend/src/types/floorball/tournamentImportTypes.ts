import type { FloorballPosition, TeamCategory } from './floorballTypes';
import type { FloorballPlayoffRoundKey } from './tournamentTypes';

/**
 * JSON schema for the "Import tournament from JSON" feature.
 *
 * One file = one tournament. Mirrors the seeder format but adds optional player rosters and a
 * group-stage match schedule. An optional `playoffSchedule` section lets the JSON pin specific
 * kickoff times to bracket slots (QF1, SF1, FIN, …) — these are surfaced as placeholder
 * "TBD vs TBD" rows in the public schedule so end-users see the full programme up-front, and
 * the backend honors them when generating the real playoff matches.
 *
 * See also: the AI prompt for generating this JSON from a schedule image (documented in the
 * implementation plan and the modal's "Help" section).
 */

export const TOURNAMENT_IMPORT_SCHEMA_VERSION = 'myleague-tournament-import/v1' as const;

/** Top-level shape uploaded via the import modal. */
export interface TournamentImportPayload {
  /** Optional schema marker. The orchestrator accepts files without it for forward compatibility. */
  $schema?: string;
  tournament: TournamentImportTournamentSection;
  clubs: TournamentImportClub[];
  teams: TournamentImportTeam[];
  groups: TournamentImportGroup[];
  matches: TournamentImportMatch[];
  /**
   * Optional pre-defined playoff bracket schedule. Each entry pins a (round, order) bracket
   * slot to a kickoff time / venue. When omitted, the backend auto-schedules playoff matches
   * on the days following the tournament end date.
   */
  playoffSchedule?: TournamentImportPlayoffSlot[];
}

/**
 * One slot in the pre-defined playoff schedule. The (round, order) pair identifies a position
 * in the bracket — the backend pairs the slot to the corresponding generated match.
 *
 * Order is 0-based within the round (QF1 = 0, QF2 = 1, …). For a 2-group-of-4 tournament with
 * 2 teams advancing per group, the bracket is: 2 semifinals (order 0, 1) → 1 final (order 0)
 * + optional 1 third-place match (order 0 under ThirdPlaceMatch round).
 */
export interface TournamentImportPlayoffSlot {
  round: FloorballPlayoffRoundKey;
  order: number;
  /** ISO-8601 with explicit offset, e.g. "2026-05-23T14:00:00+03:00". */
  scheduledDateTime: string;
  /** Optional venue / court label. Falls back to the tournament venue when omitted. */
  venue?: string | null;
}

/** Tournament-level fields. Maps 1:1 to CreateFloorballTournamentRequest on the backend. */
export interface TournamentImportTournamentSection {
  name: string;
  /** ISO date `YYYY-MM-DD` (no time). */
  startDate: string;
  /** ISO date `YYYY-MM-DD` (no time). */
  endDate: string;
  venue?: string | null;
  contentHtml?: string | null;
  groupStageNumberOfPeriods: number;
  groupStagePeriodDurationMinutes: number;
  groupStageAllowOvertime: boolean;
  groupStageOvertimeDurationMinutes: number;
  groupStageAllowShootout: boolean;
  playoffNumberOfPeriods: number;
  playoffPeriodDurationMinutes: number;
  playoffAllowOvertime: boolean;
  playoffOvertimeDurationMinutes: number;
  playoffAllowShootout: boolean;
  teamsAdvancingPerGroup: number;
  hasPlayoffStage: boolean;
  hasThirdPlaceMatch: boolean;
}

export interface TournamentImportClub {
  name: string;
  city?: string | null;
  country?: string | null;
  websiteUrl?: string | null;
  logoUrl?: string | null;
  contactEmail?: string | null;
}

export interface TournamentImportTeam {
  name: string;
  /** Club name — must match an entry in `clubs` (or an already-existing club). */
  clubName: string;
  /**
   * Division name — OPTIONAL. Tournaments don't require a division and the orchestrator
   * skips division resolution entirely when none of the imported teams specify one. Set
   * this only if you want the team to be assigned to a specific league division.
   */
  divisionName?: string | null;
  /**
   * Home arena — OPTIONAL. The backend requires a non-empty value, so the orchestrator
   * falls back to the tournament's `venue` and finally to `"TBD"` if neither is set.
   * Teams imported just for a tournament rarely have a meaningful home arena.
   */
  homeArena?: string | null;
  /** Primary jersey color — OPTIONAL. Defaults to `"TBD"` when not specified. */
  primaryJerseyColor?: string | null;
  secondaryJerseyColor?: string | null;
  /** Team category — OPTIONAL. Defaults to `Adult`. */
  category?: TeamCategory;
  /** Optional roster. If omitted, the team is created without players. */
  players?: TournamentImportTeamPlayer[];
}

export interface TournamentImportTeamPlayer {
  /**
   * First name — REQUIRED. Combined with `lastName` it is used as the find-or-create
   * key when `personEmail` is not provided. Player rosters from schedule sheets rarely
   * include emails, so name-based matching is the primary path.
   */
  firstName: string;
  /** Last name — REQUIRED. See `firstName` for the matching strategy. */
  lastName: string;
  /**
   * Optional contact email. When present it can be used as a stable unique key, but
   * we still fall back to firstName + lastName when missing.
   */
  personEmail?: string;
  /** Optional ISO date `YYYY-MM-DD`. Helps disambiguate same-named persons when present. */
  birthDate?: string;
  /** Position on the floorball team. Defaults to `Forward` when omitted. */
  position?: FloorballPosition;
  /** Jersey number — OPTIONAL. Some Excel rosters don't include jersey numbers. */
  jerseyNumber?: number;
}

export interface TournamentImportGroup {
  /** Group label as it appears on the schedule, e.g. "A", "B". */
  name: string;
  /** Team names referencing the `teams` array. */
  teamNames: string[];
}

export interface TournamentImportMatch {
  /** Optional human-readable match # from the schedule (the "#" column). */
  matchNumber?: number | null;
  /** ISO-8601 with explicit offset, e.g. "2026-05-22T18:00:00+03:00". */
  scheduledDateTime: string;
  /** Optional field/court label (e.g. "1", "2"). Appended to the venue at import time. */
  field?: string | null;
  homeTeamName: string;
  awayTeamName: string;
  /** Optional — required when the tournament has groups. */
  groupName?: string | null;
}

// ---------------------------------------------------------------------------
// Progress + record-of-changes types used by the orchestrator
// ---------------------------------------------------------------------------

export type ImportPhase =
  | 'validate'
  | 'clubs'
  | 'division'
  | 'teams'
  | 'players'
  | 'tournament'
  | 'groups'
  | 'group-teams'
  | 'matches'
  | 'done';

export interface ImportStep {
  phase: ImportPhase;
  /** 0-based progress index inside the current phase (or the overall step counter). */
  index: number;
  /** Total items in the current phase. */
  total: number;
  /** Human-readable progress line (already localized by the caller). */
  label: string;
  /** Status flag for the line so the UI can render check / refresh / skip glyphs. */
  status: 'created' | 'existing' | 'skipped' | 'info';
}

export interface ImportError {
  phase: ImportPhase;
  label: string;
  message: string;
  /** True when this error stopped the import. Non-fatal errors are reported but the run continues. */
  fatal: boolean;
}

/** Discriminated record of things the orchestrator created. Walked in reverse during revert. */
export type CreatedRecord =
  | { kind: 'match'; id: string; label: string }
  | { kind: 'group'; tournamentId: string; groupId: string; label: string }
  | { kind: 'tournament'; id: string; label: string }
  /**
   * A player→team assignment we made by calling AddPlayerToTeam. Revert calls
   * RemovePlayerFromTeam without touching the player entity itself — useful when
   * the player already existed before this import.
   */
  | { kind: 'team-player'; teamId: string; playerId: string; label: string }
  | { kind: 'player'; id: string; personId: string; label: string }
  | { kind: 'person'; id: string; label: string }
  | { kind: 'team'; id: string; label: string }
  | { kind: 'division'; id: string; label: string }
  | { kind: 'club'; id: string; label: string };

export interface ImportSummary {
  clubsCreated: number;
  clubsExisting: number;
  divisionsCreated: number;
  divisionsExisting: number;
  teamsCreated: number;
  teamsExisting: number;
  /** Persons newly created (vs found by name/email lookup). */
  personsCreated: number;
  personsExisting: number;
  /** FloorballPlayer entities newly created (one per Person who wasn't yet a player). */
  playersCreated: number;
  playersExisting: number;
  /** Player→team assignments freshly recorded (vs already on the roster). */
  teamPlayerAssignments: number;
  tournamentId: string | null;
  tournamentName: string | null;
  groupsCreated: number;
  groupAssignments: number;
  matchesCreated: number;
  errors: ImportError[];
  /** Ordered list of created records (used for revert). */
  created: CreatedRecord[];
  /**
   * True when the import was stopped before completion. `errors` will contain the fatal error
   * (or, if `aborted` is also true, it was a user cancel). `created` still lists everything
   * the orchestrator made before stopping so the caller can offer a revert.
   */
  fatal: boolean;
  /** True when the user cancelled via `shouldAbort`. Mutually exclusive with `fatal` only when `false`. */
  aborted: boolean;
}

export interface ImportCallbacks {
  /** Streamed once per progress step. */
  onStep: (step: ImportStep) => void;
  /** Streamed for every non-fatal warning. The fatal error is also thrown. */
  onError: (err: ImportError) => void;
  /** Returns true if the user pressed Cancel. Checked between every API call. */
  shouldAbort: () => boolean;
}

/** Runtime options chosen by the user in the import modal (not part of the JSON). */
export interface ImportOptions {
  /**
   * Default category applied to newly created teams that don't specify a `category` in the JSON.
   * Tournaments are usually all-Adult, all-Youth or all-Women, so this lets the admin pick once
   * for the whole import instead of editing every team in the file. Existing teams are NOT
   * re-categorized — only freshly created ones.
   */
  defaultTeamCategory?: TeamCategory;
  /**
   * Whether the tournament should be created with a playoff stage. When `false`:
   *  - The backend skips the 1..8 range check on `teamsAdvancingPerGroup` (irrelevant without a
   *    playoff stage), so non-standard team counts no longer block the import.
   *  - Any `playoffSchedule` slots in the JSON are ignored.
   *  - `hasThirdPlaceMatch` is forced off.
   *
   * Defaults to "honor the JSON" when omitted. The import modal pre-fills this based on the
   * JSON's `hasPlayoffStage` flag + presence of `playoffSchedule` entries, and lets the admin
   * override it before kicking off the import.
   */
  hasPlayoffStageOverride?: boolean;
}

export class ImportAbortedError extends Error {
  constructor() {
    super('Import was aborted by the user.');
    this.name = 'ImportAbortedError';
  }
}

/** Counts a payload would produce, for the dry-run preview shown before any API call. */
export interface ImportDryRunCounts {
  clubs: number;
  teams: number;
  /** Total player rows in the payload across all teams. */
  players: number;
  groups: number;
  groupAssignments: number;
  matches: number;
  /** Pre-defined playoff bracket slots (rendered as placeholder rows in the schedule). */
  playoffSlots: number;
}
