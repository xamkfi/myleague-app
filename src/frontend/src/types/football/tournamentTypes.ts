import type { FootballMatchRules } from './footballTypes';

export interface FootballTournamentDto {
  id: string;
  name: string;
  startDate: string;
  endDate: string;
  isActive: boolean;
  isCompleted: boolean;
  contentHtml: string | null;
  venue: string | null;
  tournamentStatus: string;
  tournamentRules: FootballTournamentRulesDto;
  groups: FootballTournamentGroupDto[];
  teamCount: number;
  matchCount: number;
  playoffSchedule: PlayoffScheduleSlotDto[];
  teamCategory?: string;
}

export interface PlayoffScheduleSlotDto {
  round: FootballPlayoffRoundKey;
  order: number;
  scheduledDateTime: string;
  venue: string | null;
}

export interface FootballTournamentRulesDto {
  groupStageMatchRules: FootballMatchRules;
  playoffMatchRules: FootballMatchRules;
  teamsAdvancingPerGroup: number;
  hasPlayoffStage: boolean;
  hasThirdPlaceMatch: boolean;
}

export interface FootballTournamentGroupDto {
  id: string;
  name: string;
  order: number;
  teams: FootballTournamentGroupTeamDto[];
}

export interface FootballTournamentGroupTeamDto {
  id: string;
  teamId: string;
  teamName: string;
}

export interface FootballTournamentGroupStandingDto {
  teamId: string;
  teamName: string;
  teamLogo: string | null;
  gamesPlayed: number;
  wins: number;
  draws: number;
  losses: number;
  goalsFor: number;
  goalsAgainst: number;
  goalDifference: number;
  points: number;
}

export type FootballPlayoffRoundKey =
  | 'QuarterFinal'
  | 'SemiFinal'
  | 'ThirdPlaceMatch'
  | 'Final';

export type FootballPlayoffSlotKey = 'Home' | 'Away';

export interface FootballPlayoffTeamDto {
  teamId: string;
  teamName: string;
  teamLogo: string | null;
}

export interface FootballPlayoffMatchDto {
  matchId: string;
  order: number;
  status: string;
  scheduledDateTime: string;
  venue: string | null;
  homeScore: number;
  awayScore: number;
  homeTeam: FootballPlayoffTeamDto | null;
  awayTeam: FootballPlayoffTeamDto | null;
  isHomeFeederResolved: boolean;
  isAwayFeederResolved: boolean;
  nextMatchId: string | null;
  nextMatchSlot: FootballPlayoffSlotKey | null;
}

export interface FootballPlayoffRoundDto {
  round: FootballPlayoffRoundKey;
  matches: FootballPlayoffMatchDto[];
}

export interface FootballPlayoffBracketDto {
  tournamentId: string;
  tournamentStatus: string;
  hasThirdPlaceMatch: boolean;
  champion: FootballPlayoffTeamDto | null;
  rounds: FootballPlayoffRoundDto[];
}

export interface CreateFootballTournamentRequest {
  name: string;
  startDate: string;
  endDate: string;
  venue?: string;
  contentHtml?: string;
  groupStageNumberOfHalves: number;
  groupStageHalfDurationMinutes: number;
  groupStagePlayersOnField: number;
  groupStageRequireGoalkeeper: boolean;
  groupStageMaxSubstitutions: number;
  groupStageRequireOfficialsToStart: boolean;
  groupStageAllowExtraTime: boolean;
  groupStageExtraTimeHalfCount: number;
  groupStageExtraTimeHalfDurationMinutes: number;
  groupStageAllowPenaltyShootout: boolean;
  playoffNumberOfHalves: number;
  playoffHalfDurationMinutes: number;
  playoffPlayersOnField: number;
  playoffRequireGoalkeeper: boolean;
  playoffMaxSubstitutions: number;
  playoffRequireOfficialsToStart: boolean;
  playoffAllowExtraTime: boolean;
  playoffExtraTimeHalfCount: number;
  playoffExtraTimeHalfDurationMinutes: number;
  playoffAllowPenaltyShootout: boolean;
  teamsAdvancingPerGroup: number;
  hasPlayoffStage: boolean;
  hasThirdPlaceMatch: boolean;
  playoffSchedule?: PlayoffScheduleSlotRequest[];
  teamCategory?: string;
}

export interface PlayoffScheduleSlotRequest {
  round: FootballPlayoffRoundKey;
  order: number;
  scheduledDateTime: string;
  venue?: string;
}

export type UpdateFootballTournamentRequest = CreateFootballTournamentRequest;

export const FOOTBALL_GROUP_STAGE_RULE_DEFAULTS = {
  groupStageNumberOfHalves: 2,
  groupStageHalfDurationMinutes: 20,
  groupStagePlayersOnField: 5,
  groupStageRequireGoalkeeper: true,
  groupStageMaxSubstitutions: 99,
  groupStageRequireOfficialsToStart: false,
  groupStageAllowExtraTime: false,
  groupStageExtraTimeHalfCount: 2,
  groupStageExtraTimeHalfDurationMinutes: 5,
  groupStageAllowPenaltyShootout: false,
};

export const FOOTBALL_PLAYOFF_RULE_DEFAULTS = {
  playoffNumberOfHalves: 2,
  playoffHalfDurationMinutes: 20,
  playoffPlayersOnField: 5,
  playoffRequireGoalkeeper: true,
  playoffMaxSubstitutions: 99,
  playoffRequireOfficialsToStart: false,
  playoffAllowExtraTime: true,
  playoffExtraTimeHalfCount: 2,
  playoffExtraTimeHalfDurationMinutes: 5,
  playoffAllowPenaltyShootout: true,
};
