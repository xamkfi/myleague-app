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
  seasonId?: string | null;
  seasonName?: string | null;
  tournamentId?: string | null;
  tournamentName?: string | null;
  tournamentGroupId?: string | null;
  tournamentRound?: string | null;
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
}

export interface CreateFloorballMatchRequest {
  seasonId?: string;
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
  seasonId: string;
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

export interface GetFloorballMatchesRequest {
  page?: number;
  pageSize?: number;
  seasonId?: string;
  teamId?: string;
  status?: FloorballMatchStatus;
  startDate?: string;
  endDate?: string;
  sortOrder?: string;
  searchQuery?: string;
}

// Tournament types
export enum FloorballTournamentStatus {
  Draft = 'Draft',
  Active = 'Active',
  InProgress = 'InProgress',
  Completed = 'Completed',
  Cancelled = 'Cancelled'
}

export enum FloorballTournamentPlayoffFormat {
  None = 'None',
  SingleElimination = 'SingleElimination',
  FinalGroup = 'FinalGroup'
}

export enum FloorballTournamentGroupPhase {
  GroupStage = 'GroupStage',
  Playoff = 'Playoff'
}

export interface FloorballTournamentGroupTeamDto {
  id: string;
  groupId: string;
  teamId: string;
  teamName: string;
  tournamentId: string;
}

export interface FloorballTournamentGroupDto {
  id: string;
  tournamentId: string;
  name: string;
  phase: string;
  sortOrder: number;
  teams: FloorballTournamentGroupTeamDto[];
}

export interface FloorballTournamentDto {
  id: string;
  name: string;
  descriptionHtml?: string | null;
  startDate: string;
  endDate: string;
  location?: string | null;
  status: string;
  playoffFormat: string;
  groupStageAdvancingCount: number;
  imageUrls: string[];
  matchRules: FloorballMatchRules;
  groups: FloorballTournamentGroupDto[];
  matches: FloorballMatchDto[];
}

export interface FloorballTournamentSummaryDto {
  id: string;
  name: string;
  startDate: string;
  endDate: string;
  location?: string | null;
  status: string;
  playoffFormat: string;
  groupCount: number;
  teamCount: number;
}

export interface FloorballTournamentGroupStandingEntryDto {
  rank: number;
  teamId: string;
  teamName: string;
  gamesPlayed: number;
  wins: number;
  draws: number;
  losses: number;
  goalsFor: number;
  goalsAgainst: number;
  goalDifference: number;
  points: number;
}

export interface FloorballTournamentGroupStandingsDto {
  groupId: string;
  groupName: string;
  entries: FloorballTournamentGroupStandingEntryDto[];
}

export interface CreateFloorballTournamentRequest {
  name: string;
  startDate: string;
  endDate: string;
  location?: string;
  descriptionHtml?: string;
  numberOfPeriods?: number;
  periodDurationMinutes?: number;
  allowOvertime?: boolean;
  overtimeDurationMinutes?: number;
  allowShootout?: boolean;
  playoffFormat?: string;
  groupStageAdvancingCount?: number;
}

export interface UpdateFloorballTournamentRequest {
  name: string;
  startDate: string;
  endDate: string;
  location?: string;
  descriptionHtml?: string;
  numberOfPeriods?: number;
  periodDurationMinutes?: number;
  allowOvertime?: boolean;
  overtimeDurationMinutes?: number;
  allowShootout?: boolean;
  playoffFormat?: string;
  groupStageAdvancingCount?: number;
}

export interface AddGroupToTournamentRequest {
  name: string;
  phase?: string;
  sortOrder?: number;
}

export interface CreateTournamentMatchRequest {
  homeTeamId: string;
  awayTeamId: string;
  scheduledDateTime: string;
  venue?: string;
  groupId?: string;
  tournamentRound?: string;
  refereeId?: string;
}