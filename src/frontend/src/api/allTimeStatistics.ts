import type { PaginatedApiResponse, TeamCategory } from '../types/floorball/floorballTypes';
import type { ApiResponse } from '../types/common/apiResponseType';
import { API_URL } from '../constants/config';
import type { SportKind } from '../utils/sportRoutes';
import { authFetch } from './utils/authFetch';
import { parseErrorResponse } from './utils/ParseErrorResponse';

export type AllTimeStatSort =
  | 'Games'
  | 'Goals'
  | 'Assists'
  | 'Points'
  | 'Penalties'
  | 'YellowCards'
  | 'RedCards';

export type AllTimeSortDirection = 'Asc' | 'Desc';

export type AllTimeCompetitionFilter = 'Season' | 'Tournament' | 'All';

export interface AllTimePlayerStatisticsDto {
  rank: number;
  playerId: string;
  playerName: string;
  teamName: string;
  gamesPlayed: number;
  goals: number;
  assists: number;
  points: number;
  penaltyMinutes?: number;
  yellowCards?: number;
  redCards?: number;
}

export interface AllTimePlayerStatisticsQuery {
  page: number;
  pageSize: number;
  teamCategory: TeamCategory;
  competitionType: AllTimeCompetitionFilter;
  sort: AllTimeStatSort;
  direction: AllTimeSortDirection;
  search?: string;
  teamId?: string;
}

export interface AllTimeTeamOptionDto {
  teamId: string;
  teamName: string;
}

function statisticsPath(sport: SportKind): string {
  if (sport === 'football') {
    return `${API_URL}/football/statistics/all-time`;
  }
  if (sport === 'hockey') {
    return `${API_URL}/HockeyStatistics/all-time`;
  }
  return `${API_URL}/floorball/statistics/all-time`;
}

export async function getAllTimePlayerStatistics(
  sport: SportKind,
  query: AllTimePlayerStatisticsQuery,
): Promise<PaginatedApiResponse<AllTimePlayerStatisticsDto>> {
  const params = new URLSearchParams({
    page: String(query.page),
    pageSize: String(query.pageSize),
    teamCategory: query.teamCategory,
    competitionType: query.competitionType,
    sort: query.sort,
    direction: query.direction,
  });
  const search = query.search?.trim();
  if (search) {
    params.set('search', search);
  }
  if (query.teamId) {
    params.set('teamId', query.teamId);
  }

  const response = await authFetch(`${statisticsPath(sport)}?${params.toString()}`);
  if (!response.ok) {
    const errorMessage = await parseErrorResponse(response, 'Failed to fetch all-time statistics');
    throw new Error(errorMessage);
  }

  const apiResponse: PaginatedApiResponse<AllTimePlayerStatisticsDto> = await response.json();
  if (!apiResponse.success) {
    throw new Error(await parseErrorResponse(apiResponse, 'Failed to fetch all-time statistics'));
  }

  return apiResponse;
}

export async function getAllTimeTeams(
  sport: SportKind,
  teamCategory: TeamCategory,
  competitionType: AllTimeCompetitionFilter,
): Promise<AllTimeTeamOptionDto[]> {
  const params = new URLSearchParams({ teamCategory, competitionType });

  const response = await authFetch(`${statisticsPath(sport)}/teams?${params.toString()}`);
  if (!response.ok) {
    const errorMessage = await parseErrorResponse(response, 'Failed to fetch all-time teams');
    throw new Error(errorMessage);
  }

  const apiResponse: ApiResponse<AllTimeTeamOptionDto[]> = await response.json();
  if (!apiResponse.success) {
    throw new Error(await parseErrorResponse(apiResponse, 'Failed to fetch all-time teams'));
  }

  return apiResponse.data ?? [];
}
