export enum FootballPosition {
  None = 'None',
  Goalkeeper = 'Goalkeeper',
  Defender = 'Defender',
  Midfielder = 'Midfielder',
  Forward = 'Forward'
}

export enum TeamCategory {
  Adult = 'Adult',
  Youth = 'Youth',
  Women = 'Women'
}

export enum FootballMatchStatus {
  None = 'None',
  Scheduled = 'Scheduled',
  InProgress = 'InProgress',
  Completed = 'Completed',
  Cancelled = 'Cancelled',
  Postponed = 'Postponed'
}

export enum FootballGoalType {
  Regular = 0,
  PenaltyKick = 1,
  OwnGoal = 2,
  ExtraTime = 3,
  PenaltyShootout = 4
}

export enum FootballCardType {
  Yellow = 0,
  SecondYellow = 1,
  DirectRed = 2
}

export type FootballCompetitionType = 'Season' | 'Tournament';

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

export interface FootballLineupPlayer {
  playerId: string;
  position: FootballPosition;
  isOnField: boolean;
  isSentOff: boolean;
}

export interface FootballTeamPlayer {
  teamId: string;
  playerId: string;
  playerName: string;
  position: FootballPosition;
  jerseyNumber?: number;
  requestedJerseyNumber?: number | null;
  isActive: boolean;
  gamesPlayed: number;
  goals: number;
  assists: number;
  yellowCards: number;
  redCards: number;
}

export interface FootballReferee {
  refereeId: string;
  name: string;
}

export interface FootballTeam {
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
  roster: FootballTeamPlayer[];
  teamCategory?: TeamCategory;
}

export interface FootballTeamNameResult {
  id: string;
  name: string;
}

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

export interface GetFootballTeamsRequest {
  page?: number;
  pageSize?: number;
  clubId?: string;
  division?: string;
  teamCategories?: TeamCategory[];
  searchTerm?: string;
}

export interface FootballTeamRequest {
  name: string;
  divisionId?: string;
  clubId: string;
  homeArena?: string;
  primaryJerseyColor?: string;
  category?: TeamCategory;
  secondaryJerseyColor?: string;
  logoUrl?: string;
  shortName?: string;
}

export interface FootballTeamPlayerDto {
  teamId: string;
  playerId: string;
  playerName: string;
  position: FootballPosition;
  jerseyNumber?: number;
  requestedJerseyNumber?: number | null;
  isActive: boolean;
  gamesPlayed: number;
  goals: number;
  assists: number;
  yellowCards: number;
  redCards: number;
}

export interface UpdateFootballTeamPlayerRequest {
  position: FootballPosition;
  jerseyNumber?: number;
  isActive: boolean;
}

export interface AddPlayerToTeamRequest {
  position: FootballPosition;
  jerseyNumber?: number;
}

export interface FootballGoalEventDto {
  id: string;
  teamId: string;
  scoringPlayerId?: string;
  assistingPlayerId?: string;
  periodNumber: number;
  timeInSeconds: number;
  playerName?: string;
  assisterName?: string;
  goalType?: FootballGoalType | number | string | null;
  description?: string;
}

export interface FootballCardEventDto {
  id: string;
  teamId: string;
  playerId: string;
  cardType: FootballCardType | string;
  periodNumber: number;
  timeInSeconds: number;
  playerName?: string;
  description?: string;
}

export interface FootballSubstitutionEventDto {
  id: string;
  teamId: string;
  playerOffId: string;
  playerOnId: string;
  periodNumber: number;
  timeInSeconds: number;
  playerOffName?: string;
  playerOnName?: string;
  description?: string;
}

export interface FootballMatchRules {
  numberOfHalves: number;
  halfDurationMinutes: number;
  playersOnField: number;
  requireGoalkeeper: boolean;
  maxSubstitutions: number;
  requireOfficialsToStart: boolean;
  allowExtraTime: boolean;
  extraTimeHalfCount: number;
  extraTimeHalfDurationMinutes: number;
  allowPenaltyShootout: boolean;
}

export interface FootballMatchDto {
  id: string;
  competitionId: string;
  competitionName: string;
  homeTeamId: string | null;
  homeTeamName: string | null;
  homeTeamLogo: string | null;
  awayTeamId: string | null;
  awayTeamName: string | null;
  awayTeamLogo: string | null;
  scheduledDateTime: string;
  venue?: string;
  status: FootballMatchStatus;
  homeScore: number;
  awayScore: number;
  wentToExtraTime: boolean;
  wentToPenaltyShootout: boolean;
  periodScores: Record<number, { homeScore: number; awayScore: number; isCompleted: boolean }>;
  officials: string[];
  refereeId?: string;
  goalEvents: FootballGoalEventDto[];
  cardEvents: FootballCardEventDto[];
  substitutionEvents: FootballSubstitutionEventDto[];
  matchRules: FootballMatchRules;
  homeLineup: FootballLineupPlayer[];
  awayLineup: FootballLineupPlayer[];
  tournamentGroupId?: string | null;
  tournamentStage?: string | null;
  competitionType?: FootballCompetitionType;
  playoffRound?: string | null;
  playoffMatchOrder?: number | null;
  nextMatchId?: string | null;
  nextMatchSlot?: string | null;
}

export interface CreateFootballMatchRequest {
  competitionId?: string;
  homeTeamId?: string;
  awayTeamId?: string;
  refereeId?: string;
  scheduledDateTime: string;
  venue?: string;
  tournamentGroupId?: string;
  tournamentStage?: string;
}

export interface UpdateFootballMatchRequest {
  id: string;
  scheduledDateTime: string;
  venue?: string;
  refereeId?: string;
}

export interface AssignMatchTeamsRequest {
  homeTeamId: string | null;
  awayTeamId: string | null;
}

export interface GetFootballMatchesRequest {
  page?: number;
  pageSize?: number;
  competitionId?: string;
  teamId?: string;
  status?: FootballMatchStatus;
  startDate?: string;
  endDate?: string;
  sortOrder?: string;
  searchQuery?: string;
  tournamentGroupId?: string;
  competitionType?: FootballCompetitionType;
  teamCategory?: TeamCategory;
}

export interface LineupPlayerRequest {
  playerId: string;
  position: FootballPosition;
  isOnField: boolean;
}

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
