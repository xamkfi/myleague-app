// Enums
export enum FloorballPosition {
  None = 'None',
  Goalkeeper = 'Goalkeeper',
  Defender = 'Defender',
  Forward = 'Forward',
  Center = 'Center'
}

/**
 * Single entry in a team's active field player lineup for a specific match. Mirrors the
 * backend `FloorballActiveLineupPlayerDto` and is used by the lineup UI to group players
 * by per-match role (defender / forward / center).
 */
export interface FloorballActiveLineupPlayer {
  playerId: string;
  position: FloorballPosition;
}

export enum TeamCategory {
  Adult = 'Adult',
  Youth = 'Youth',
  Women = 'Women'
}

export enum FloorballMatchStatus {
  Scheduled = 'Scheduled',
  InProgress = 'InProgress',
  Completed = 'Completed',
  Cancelled = 'Cancelled',
  Postponed = 'Postponed'
}

// Base interfaces
export interface Club {
  id: string;
  name: string;
  foundingDate: string | null;
  city: string | null;
  country: string | null;
  websiteUrl?: string | null;
  logoUrl?: string | null;
  contactEmail?: string | null;
}

export interface Person {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  dateOfBirth: string;
  email?: string;
  phoneNumber?: string;
  address?: string;
  isRegistered: boolean;
}

export interface FloorballTeamPlayer {
  teamId: string;
  playerId: string;
  playerName: string;
  position: FloorballPosition;
  jerseyNumber?: number;
  /**
   * The jersey number originally requested for this player when the assigned
   * {@link jerseyNumber} is a substitute (e.g. import had to pick the next free
   * number). `null` / `undefined` means the assigned number matches the requested
   * one and no admin review is needed. Drives the "needs review" highlight on the
   * roster page; cleared automatically when the admin changes the number.
   */
  requestedJerseyNumber?: number | null;
  isActive: boolean;
  age?: number;
  gamesPlayed: number;
  goals: number;
  assists: number;
  penaltyMinutes: number;
  yellowCards: number;
  redCards: number;
}

export interface FloorballReferee {
  refereeId: string;
  name: string;
}

export interface FloorballTeam {
  id: string;
  name: string;
  shortName: string;
  divisionId?: string | null;
  club: Club;
  homeArena: string;
  primaryJerseyColor: string;
  secondaryJerseyColor?: string;
  logoUrl?: string;
  hasActiveMembers: boolean;
  roster: FloorballTeamPlayer[];
  teamCategory?: TeamCategory;
}

export interface FloorballTeamNameResult {
  id: string;
  name: string;
}

// API Response types
export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message: string;
  errors: string[];
}

export interface PaginatedApiResponse<T> {
  success: boolean;
  data: T[];
  pagination: {
    currentPage: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
    hasNextPage: boolean;
    hasPreviousPage: boolean;
    startItem: number;
    endItem: number;
  };
  message: string;
  errors: string[];
}

// Request types
export interface GetFloorballTeamsRequest {
  page?: number;
  pageSize?: number;
  clubId?: string;
  division?: string;
}

export interface FloorballTeamRequest {
  name: string;
  divisionId?: string;
  clubId: string;
  /** Optional — tournament-only teams often have no permanent home arena. */
  homeArena?: string;
  /** Optional. */
  primaryJerseyColor?: string;
  category?: TeamCategory;
  secondaryJerseyColor?: string;
  logoUrl?: string;
  shortName?: string;
}

// Team Player Management types
export interface FloorballTeamPlayerDto {
  teamId: string;
  playerId: string;
  playerName: string;
  position: FloorballPosition;
  jerseyNumber?: number;
  /** See {@link FloorballTeamPlayer.requestedJerseyNumber}. */
  requestedJerseyNumber?: number | null;
  isActive: boolean;
  gamesPlayed: number;
  goals: number;
  assists: number;
  penaltyMinutes: number;
}

export interface UpdateFloorballTeamPlayerRequest {
  position: FloorballPosition;
  jerseyNumber?: number;
  isActive: boolean;
}

export interface AddPlayerToTeamRequest {
  position: FloorballPosition;
  jerseyNumber?: number;
}

/**
 * Floorball goal type. Mirrors the backend `Domain.Enums.Floorball.FloorballGoalType` enum.
 * Values are numeric so they can be sent over the wire in command payloads without ambiguity.
 */
export enum FloorballGoalType {
  Regular = 0,
  PowerPlay = 1,
  ShortHanded = 2,
  EmptyNet = 3,
  PenaltyShot = 4,
  OwnGoal = 5,
  Overtime = 6,
  Shootout = 7
}

// Match Event types
export interface FloorballGoalEventDto {
  id: string;
  teamId: string;
  playerId: string;
  assisterId?: string;
  secondaryAssisterId?: string;
  periodNumber: number;
  timeInSeconds: number;
  wasInOvertime: boolean;
  wasInShootout: boolean;
  playerName?: string;
  assisterName?: string;
  secondaryAssisterName?: string;
  /**
   * Type of goal scored. `null`/`undefined` means the goal was recorded without a
   * specific type (treated as a regular even-strength goal). Backend serializes
   * the enum as its string name (e.g. `"PenaltyShot"`) via
   * `JsonStringEnumConverter`, but command payloads use the numeric form, so we
   * accept either at the type level.
   */
  goalType?: FloorballGoalType | number | string | null;
}

export interface FloorballPenaltyEventDto {
  id: string;
  teamId: string;
  playerId?: string;
  penaltyType: string;
  minutes: number;
  periodNumber: number;
  timeInSeconds: number;
  description: string;
  playerName?: string;
}

// Save event DTO
export interface FloorballSaveEventDto {
  id: string;
  teamId: string;
  goalieId: string; // goalie id
  periodNumber: number;
  timeInSeconds: number;
  wasInOvertime: boolean;
  wasInShootout: boolean;
  playerName?: string;
}

export interface FloorballMatchRules {
  numberOfPeriods: number;
  periodDurationMinutes: number;
  allowOvertime: boolean;
  overtimeDurationMinutes: number;
  allowShootout: boolean;
}

export interface FloorballMatchDto {
  id: string;
  competitionId: string;
  /**
   * Display name of the competition (season or tournament) this match belongs to.
   * Backend renamed from "seasonName" to "competitionName" when FloorballSeason was
   * generalized to FloorballCompetition (TPH base for seasons + tournaments).
   */
  competitionName: string;
  /**
   * ID of the home team, or `null` when the participant is not yet known
   * (fixture scheduled in advance / playoff slot waiting on a feeder result).
   */
  homeTeamId: string | null;
  /** Name of the home team, or `null` when the slot is unassigned. */
  homeTeamName: string | null;
  homeTeamLogo: string | null;
  /** ID of the away team, or `null` when the participant is not yet known. */
  awayTeamId: string | null;
  /** Name of the away team, or `null` when the slot is unassigned. */
  awayTeamName: string | null;
  awayTeamLogo: string | null;
  scheduledDateTime: string;
  venue?: string;
  status: FloorballMatchStatus;
  homeScore: number;
  awayScore: number;
  wentToOvertime: boolean;
  wentToShootout: boolean;
  homeActiveGoalieId?: string;
  awayActiveGoalieId?: string;
  refereeId?: string;
  periodScores: Record<number, { homeScore: number; awayScore: number; isCompleted: boolean }>;
  officials: string[];
  goalEvents: FloorballGoalEventDto[];
  penaltyEvents: FloorballPenaltyEventDto[];
  saveEvents: FloorballSaveEventDto[];
  matchRules: FloorballMatchRules;
  /**
   * Active field players for the home team in this match, each with the per-match role
   * (Forward/Center/Defender). Goalies are tracked separately via
   * {@link FloorballMatchDto.homeActiveGoalieId}.
   */
  homeActivePlayers: FloorballActiveLineupPlayer[];
  /**
   * Active field players for the away team in this match. See {@link FloorballMatchDto.homeActivePlayers}.
   */
  awayActivePlayers: FloorballActiveLineupPlayer[];
  tournamentGroupId?: string | null;
  tournamentStage?: string | null;
  /**
   * Explicit competition discriminator. Backend sets this from the loaded Competition runtime type
   * (FloorballSeason vs FloorballTournament). Optional for backward compatibility with older clients
   * that consumed the DTO before the field existed.
   */
  competitionType?: FloorballCompetitionType;
}

export interface CreateFloorballMatchRequest {
  competitionId?: string;
  homeTeamId?: string;
  awayTeamId?: string;
  refereeId?: string;
  scheduledDateTime: string;
  venue?: string;
  /** Optional tournament group id (only for tournament group-stage matches). */
  tournamentGroupId?: string;
  /** Optional tournament stage label (e.g. "GroupStage"). Only for tournament matches. */
  tournamentStage?: string;
}

export interface UpdateFloorballMatchRequest {
  id: string;
  scheduledDateTime: string;
  venue?: string;
  refereeId?: string;
}

// New types for edit match functionality
export interface ChangeMatchSeasonRequest {
  competitionId: string;
}

export interface ChangeMatchTeamsRequest {
  homeTeamId: string;
  awayTeamId: string;
}

/**
 * Request body for `PUT /api/floorball-matches/{id}/teams`. Either side may be `null` to
 * clear that slot back to "to be determined". When both are present they must reference
 * different teams. Only allowed for matches in Scheduled or Postponed status.
 */
export interface AssignMatchTeamsRequest {
  homeTeamId: string | null;
  awayTeamId: string | null;
}

export interface ChangeMatchVenueRequest {
  venue: string;
}

export interface ChangeMatchDateTimeRequest {
  scheduledDateTime: string;
}

export type FloorballCompetitionType = 'Season' | 'Tournament';

export interface GetFloorballMatchesRequest {
  page?: number;
  pageSize?: number;
  competitionId?: string;
  teamId?: string;
  status?: FloorballMatchStatus;
  startDate?: string;
  endDate?: string;
  sortOrder?: string;
  searchQuery?: string;
  tournamentGroupId?: string;
  competitionType?: FloorballCompetitionType;
} 