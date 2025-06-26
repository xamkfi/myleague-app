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

export interface FloorballTeam {
  id: string;
  name: string;
  divisionId: string;
  club: Club;
  homeArena: string;
  primaryJerseyColor: string;
  secondaryJerseyColor?: string;
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