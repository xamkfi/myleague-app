// Enums
export enum FloorballPosition {
  None = 'None',
  Goalkeeper = 'Goalkeeper',
  Defender = 'Defender',
  Forward = 'Forward'
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
  homeArena: string;
  primaryJerseyColor: string;
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
  homeTeamId: string;
  homeTeamName: string;
  homeTeamLogo: string | null;
  awayTeamId: string;
  awayTeamName: string;
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
  tournamentGroupId?: string | null;
  tournamentStage?: string | null;
}

export interface CreateFloorballMatchRequest {
  competitionId?: string;
  homeTeamId?: string;
  awayTeamId?: string;
  refereeId?: string;
  scheduledDateTime: string;
  venue?: string;
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