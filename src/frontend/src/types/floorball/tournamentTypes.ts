import type { FloorballMatchRules } from './floorballTypes';

export interface FloorballTournamentDto {
  id: string;
  name: string;
  startDate: string;
  endDate: string;
  isActive: boolean;
  isCompleted: boolean;
  contentHtml: string | null;
  venue: string | null;
  tournamentStatus: string;
  tournamentRules: FloorballTournamentRulesDto;
  groups: FloorballTournamentGroupDto[];
  teamCount: number;
  matchCount: number;
}

export interface FloorballTournamentRulesDto {
  groupStageMatchRules: FloorballMatchRules;
  playoffMatchRules: FloorballMatchRules;
  teamsAdvancingPerGroup: number;
  hasPlayoffStage: boolean;
  hasThirdPlaceMatch: boolean;
}

export interface FloorballTournamentGroupDto {
  id: string;
  name: string;
  order: number;
  teams: FloorballTournamentGroupTeamDto[];
}

export interface FloorballTournamentGroupTeamDto {
  id: string;
  teamId: string;
  teamName: string;
}

export interface FloorballTournamentGroupStandingDto {
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

export interface CreateFloorballTournamentRequest {
  name: string;
  startDate: string;
  endDate: string;
  venue?: string;
  contentHtml?: string;
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
