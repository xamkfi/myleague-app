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
  foundingDate: string;
  city: string;
  country: string;
  websiteUrl?: string;
  logoUrl?: string;
  contactEmail?: string;
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
  gamesPlayed: number;
  goals: number;
  assists: number;
  penaltyMinutes: number;
}

export interface FloorballReferee {
  refereeId: string;
  name: string;
}

export interface FloorballTeam {
  id: string;
  name: string;
  divisionId: string;
  club: Club;
  homeArena: string;
  primaryJerseyColor: string;
  secondaryJerseyColor?: string;
  logoUrl?: string;
  hasActiveMembers: boolean;
  roster: FloorballTeamPlayer[];
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
  divisionId: string;
  clubId: string;
  homeArena: string;
  primaryJerseyColor: string;
  category?: TeamCategory;
  secondaryJerseyColor?: string;
  logoUrl?: string;
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
  teamId: string;
  playerId?: string;
  penaltyType: string;
  minutes: number;
  periodNumber: number;
  timeInSeconds: number;
  description: string;
  playerName?: string;
}

// Match-related interfaces
export interface FloorballMatchDto {
  id: string;
  seasonId: string;
  homeTeamId: string;
  homeTeamName: string;
  awayTeamId: string;
  awayTeamName: string;
  scheduledDateTime: string;
  venue?: string;
  status: FloorballMatchStatus;
  homeScore: number;
  awayScore: number;
  wentToOvertime: boolean;
  wentToShootout: boolean;
  periodScores: Record<number, { homeScore: number; awayScore: number }>;
  officials: string[];
  goalEvents: FloorballGoalEventDto[];
  penaltyEvents: FloorballPenaltyEventDto[];
  homeClub?: Club;
  awayClub?: Club;
}

export interface CreateFloorballMatchRequest {
  seasonId: string;
  homeTeamId: string;
  awayTeamId: string;
  refereeId?: string;
  scheduledDateTime: string;
  venue?: string;
}

export interface UpdateFloorballMatchRequest {
  id: string;
  scheduledDateTime: string;
  venue?: string;
}

export interface GetFloorballMatchesRequest {
  page?: number;
  pageSize?: number;
  seasonId?: string;
  teamId?: string;
  startDate?: string;
  endDate?: string;
} 