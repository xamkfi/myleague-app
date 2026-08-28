import type {
  CreateHockeyMatchRequest,
  GetPagedHockeyMatchesRequest,
  HockeyGoalStrength,
  HockeyMatchDto,
  HockeyMatchType,
  HockeyOfficialRole,
  HockeyPenaltyOffence,
  HockeyPenaltySeverity,
  HockeyShotResult,
  HockeyPeriodType,
  PaginatedApiResponse,
  RecordHockeyFaceoffRequest,
  RecordHockeyGoalRequest,
  RecordHockeyPenaltyRequest,
  RecordHockeyPeriodEventRequest,
  RecordHockeyShotRequest,
  RecordHockeyStoppageRequest,
} from '../../types/hockey/hockeyTypes';
import { hockeyPagedRequest, hockeyRequest, jsonBody, loadAllPaged, toQueryString } from './hockeyApi';

export const hockeyMatchService = {
  getPaged: (params: GetPagedHockeyMatchesRequest = {}): Promise<PaginatedApiResponse<HockeyMatchDto>> =>
    hockeyPagedRequest<HockeyMatchDto>(
      `/HockeyMatch/paged${toQueryString({
        page: params.page,
        pageSize: params.pageSize,
        competitionId: params.competitionId,
        teamId: params.teamId,
        startDate: params.startDate,
        endDate: params.endDate,
        status: params.status,
        sortOrder: params.sortOrder,
        searchQuery: params.searchQuery,
      })}`,
      'Failed to fetch hockey matches',
    ),

  getAllPages: (params: Omit<GetPagedHockeyMatchesRequest, 'page' | 'pageSize'> = {}): Promise<HockeyMatchDto[]> =>
    loadAllPaged((page, pageSize) => hockeyMatchService.getPaged({ ...params, page, pageSize })),

  getById: (matchId: string): Promise<HockeyMatchDto> =>
    hockeyRequest<HockeyMatchDto>(`/HockeyMatch/${matchId}`, 'Failed to fetch hockey match'),

  getByCompetition: (competitionId: string): Promise<HockeyMatchDto[]> =>
    hockeyRequest<HockeyMatchDto[]>(
      `/HockeyMatch/competition/${competitionId}`,
      'Failed to fetch hockey matches',
    ),

  getByTeam: (teamId: string): Promise<HockeyMatchDto[]> =>
    hockeyRequest<HockeyMatchDto[]>(
      `/HockeyMatch/team/${teamId}`,
      'Failed to fetch hockey matches for team',
    ),

  create: (data: CreateHockeyMatchRequest): Promise<HockeyMatchDto> =>
    hockeyRequest<HockeyMatchDto>('/HockeyMatch', 'Failed to create hockey match', {
      method: 'POST',
      ...jsonBody(data),
    }),

  assignTeams: (matchId: string, homeTeamId: string, awayTeamId: string): Promise<HockeyMatchDto> =>
    hockeyRequest<HockeyMatchDto>(`/HockeyMatch/${matchId}/teams`, 'Failed to assign match teams', {
      method: 'PUT',
      ...jsonBody({ homeTeamId, awayTeamId }),
    }),

  updateVenue: (matchId: string, venue: string): Promise<HockeyMatchDto> =>
    hockeyRequest<HockeyMatchDto>(`/HockeyMatch/${matchId}/venue`, 'Failed to update venue', {
      method: 'PUT',
      ...jsonBody({ venue }),
    }),

  updateSchedule: (matchId: string, scheduledStartTime: string): Promise<HockeyMatchDto> =>
    hockeyRequest<HockeyMatchDto>(
      `/HockeyMatch/${matchId}/schedule`,
      'Failed to update match schedule',
      { method: 'PUT', ...jsonBody({ scheduledStartTime }) },
    ),

  confirmRoster: (
    matchId: string,
    matchTeamId: string,
    teamPlayerIds: string[],
  ): Promise<HockeyMatchDto> =>
    hockeyRequest<HockeyMatchDto>(
      `/HockeyMatch/${matchId}/roster/confirm`,
      'Failed to confirm roster',
      {
        method: 'POST',
        ...jsonBody({ matchTeamId, teamPlayerIds, source: 'CopiedFromTeamRoster' }),
      },
    ),

  setActiveGoalie: (
    matchId: string,
    matchTeamId: string,
    matchActivePlayerId: string,
  ): Promise<HockeyMatchDto> =>
    hockeyRequest<HockeyMatchDto>(
      `/HockeyMatch/${matchId}/active-goalie`,
      'Failed to set active goalie',
      { method: 'PUT', ...jsonBody({ matchTeamId, matchActivePlayerId }) },
    ),

  addOfficial: (
    matchId: string,
    officialId: string,
    role: HockeyOfficialRole,
    isMainOfficial: boolean,
  ): Promise<HockeyMatchDto> =>
    hockeyRequest<HockeyMatchDto>(
      `/HockeyMatch/${matchId}/officials`,
      'Failed to attach official',
      { method: 'POST', ...jsonBody({ officialId, role, isMainOfficial }) },
    ),

  removeOfficial: (matchId: string, officialId: string): Promise<HockeyMatchDto> =>
    hockeyRequest<HockeyMatchDto>(
      `/HockeyMatch/${matchId}/officials/${officialId}`,
      'Failed to remove official',
      { method: 'DELETE' },
    ),

  start: (matchId: string): Promise<HockeyMatchDto> =>
    hockeyRequest<HockeyMatchDto>(`/HockeyMatch/${matchId}/start`, 'Failed to start match', {
      method: 'POST',
      ...jsonBody({}),
    }),

  finish: (matchId: string): Promise<HockeyMatchDto> =>
    hockeyRequest<HockeyMatchDto>(`/HockeyMatch/${matchId}/finish`, 'Failed to finish match', {
      method: 'POST',
      ...jsonBody({}),
    }),

  setStatus: (matchId: string, status: string): Promise<HockeyMatchDto> =>
    hockeyRequest<HockeyMatchDto>(`/HockeyMatch/${matchId}/status`, 'Failed to update match status', {
      method: 'PATCH',
      ...jsonBody({ status }),
    }),

  setPeriod: (matchId: string, periodNumber: number): Promise<HockeyMatchDto> =>
    hockeyRequest<HockeyMatchDto>(`/HockeyMatch/${matchId}/period`, 'Failed to set period', {
      method: 'PATCH',
      ...jsonBody({ periodNumber }),
    }),

  recordGoal: (matchId: string, data: RecordHockeyGoalRequest): Promise<HockeyMatchDto> =>
    hockeyRequest<HockeyMatchDto>(`/HockeyMatch/${matchId}/events/goals`, 'Failed to record goal', {
      method: 'POST',
      ...jsonBody(data),
    }),

  deleteGoal: (matchId: string, eventId: string): Promise<HockeyMatchDto> =>
    hockeyRequest<HockeyMatchDto>(
      `/HockeyMatch/${matchId}/events/goals/${eventId}`,
      'Failed to delete goal',
      { method: 'DELETE' },
    ),

  recordPenalty: (matchId: string, data: RecordHockeyPenaltyRequest): Promise<HockeyMatchDto> =>
    hockeyRequest<HockeyMatchDto>(
      `/HockeyMatch/${matchId}/events/penalties`,
      'Failed to record penalty',
      { method: 'POST', ...jsonBody(data) },
    ),

  deletePenalty: (matchId: string, eventId: string): Promise<HockeyMatchDto> =>
    hockeyRequest<HockeyMatchDto>(
      `/HockeyMatch/${matchId}/events/penalties/${eventId}`,
      'Failed to delete penalty',
      { method: 'DELETE' },
    ),

  recordShot: (matchId: string, data: RecordHockeyShotRequest): Promise<HockeyMatchDto> =>
    hockeyRequest<HockeyMatchDto>(`/HockeyMatch/${matchId}/events/shots`, 'Failed to record shot', {
      method: 'POST',
      ...jsonBody(data),
    }),

  deleteShot: (matchId: string, eventId: string): Promise<HockeyMatchDto> =>
    hockeyRequest<HockeyMatchDto>(
      `/HockeyMatch/${matchId}/events/shots/${eventId}`,
      'Failed to delete shot',
      { method: 'DELETE' },
    ),

  recordFaceoff: (matchId: string, data: RecordHockeyFaceoffRequest): Promise<HockeyMatchDto> =>
    hockeyRequest<HockeyMatchDto>(
      `/HockeyMatch/${matchId}/events/faceoffs`,
      'Failed to record faceoff',
      { method: 'POST', ...jsonBody(data) },
    ),

  recordStoppage: (matchId: string, data: RecordHockeyStoppageRequest): Promise<HockeyMatchDto> =>
    hockeyRequest<HockeyMatchDto>(
      `/HockeyMatch/${matchId}/events/stoppages`,
      'Failed to record stoppage',
      { method: 'POST', ...jsonBody(data) },
    ),

  recordPeriodEvent: (
    matchId: string,
    data: RecordHockeyPeriodEventRequest,
  ): Promise<HockeyMatchDto> =>
    hockeyRequest<HockeyMatchDto>(
      `/HockeyMatch/${matchId}/events/periods`,
      'Failed to record period event',
      { method: 'POST', ...jsonBody(data) },
    ),

  addPeriodScore: (
    matchId: string,
    periodNumber: number,
    periodType: HockeyPeriodType,
  ): Promise<HockeyMatchDto> =>
    hockeyRequest<HockeyMatchDto>(
      `/HockeyMatch/${matchId}/period-scores`,
      'Failed to add period score',
      { method: 'POST', ...jsonBody({ periodNumber, periodType }) },
    ),

  setWentToOvertime: (matchId: string, value: boolean): Promise<HockeyMatchDto> =>
    hockeyRequest<HockeyMatchDto>(
      `/HockeyMatch/${matchId}/went-to-overtime`,
      'Failed to update overtime flag',
      { method: 'PATCH', ...jsonBody({ value }) },
    ),

  setWentToShootout: (matchId: string, value: boolean): Promise<HockeyMatchDto> =>
    hockeyRequest<HockeyMatchDto>(
      `/HockeyMatch/${matchId}/went-to-shootout`,
      'Failed to update shootout flag',
      { method: 'PATCH', ...jsonBody({ value }) },
    ),
};

export type {
  CreateHockeyMatchRequest,
  HockeyGoalStrength,
  HockeyMatchType,
  HockeyPenaltyOffence,
  HockeyPenaltySeverity,
  HockeyShotResult,
};
