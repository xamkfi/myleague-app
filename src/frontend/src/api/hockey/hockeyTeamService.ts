import type {
  CreateHockeyTeamRequest,
  GetPagedHockeyTeamsRequest,
  HockeyLineType,
  HockeyPosition,
  HockeyTeamDto,
  HockeyTeamPlayerDto,
  HockeyRosterStatus,
  PaginatedApiResponse,
  UpdateHockeyTeamRequest,
} from '../../types/hockey/hockeyTypes';
import { hockeyPagedRequest, hockeyRequest, jsonBody, toQueryString, withTeamCategory } from './hockeyApi';

function uniqueById<T extends { id: string }>(items: T[]): T[] {
  const seen = new Set<string>();
  const unique: T[] = [];
  for (const item of items) {
    if (seen.has(item.id)) {
      continue;
    }
    seen.add(item.id);
    unique.push(item);
  }
  return unique;
}

export const hockeyTeamService = {
  getPaged: (params: GetPagedHockeyTeamsRequest = {}): Promise<PaginatedApiResponse<HockeyTeamDto>> =>
    hockeyPagedRequest<HockeyTeamDto>(
      `/HockeyTeam/paged${toQueryString({
        page: params.page,
        pageSize: params.pageSize,
        searchTerm: params.searchTerm,
        clubId: params.clubId,
        teamCategory: params.teamCategory,
      })}`,
      'Failed to fetch hockey teams',
    ),

  getAll: async (teamCategory?: string): Promise<HockeyTeamDto[]> =>
    uniqueById(
      await hockeyRequest<HockeyTeamDto[]>(
        withTeamCategory('/HockeyTeam', teamCategory),
        'Failed to fetch hockey teams',
      ),
    ),

  getById: (id: string): Promise<HockeyTeamDto> =>
    hockeyRequest<HockeyTeamDto>(`/HockeyTeam/${id}`, 'Failed to fetch hockey team'),

  getByClubId: async (clubId: string, teamCategory?: string): Promise<HockeyTeamDto[]> =>
    uniqueById(
      await hockeyRequest<HockeyTeamDto[]>(
        withTeamCategory(`/HockeyTeam/club/${clubId}`, teamCategory),
        'Failed to fetch hockey teams for club',
      ),
    ),

  create: (data: CreateHockeyTeamRequest): Promise<HockeyTeamDto> =>
    hockeyRequest<HockeyTeamDto>('/HockeyTeam', 'Failed to create hockey team', {
      method: 'POST',
      ...jsonBody(data),
    }),

  update: (id: string, data: UpdateHockeyTeamRequest): Promise<HockeyTeamDto> =>
    hockeyRequest<HockeyTeamDto>(`/HockeyTeam/${id}`, 'Failed to update hockey team', {
      method: 'PUT',
      ...jsonBody(data),
    }),

  setActive: (id: string, isActive: boolean): Promise<HockeyTeamDto> =>
    hockeyRequest<HockeyTeamDto>(`/HockeyTeam/${id}/active`, 'Failed to update team status', {
      method: 'PUT',
      ...jsonBody({ isActive }),
    }),

  updateLogo: (teamId: string, logoUrl: string | null): Promise<HockeyTeamDto> =>
    hockeyRequest<HockeyTeamDto>(`/HockeyTeam/${teamId}/logo`, 'Failed to update team logo', {
      method: 'PUT',
      ...jsonBody({ logoUrl }),
    }),

  addPlayer: (
    teamId: string,
    playerId: string,
    position: HockeyPosition,
    jerseyNumber?: number,
    rosterStatus: HockeyRosterStatus = 'Active',
  ): Promise<HockeyTeamDto> =>
    hockeyRequest<HockeyTeamDto>(`/HockeyTeam/${teamId}/players`, 'Failed to add player to team', {
      method: 'POST',
      ...jsonBody({
        playerId,
        position,
        jerseyNumber,
        rosterStatus,
      }),
    }),

  updatePlayer: (
    teamId: string,
    playerId: string,
    data: {
      position: HockeyPosition;
      jerseyNumber?: number | null;
      rosterStatus: string;
      captainRole: string;
    },
  ): Promise<HockeyTeamPlayerDto> =>
    hockeyRequest<HockeyTeamPlayerDto>(
      `/HockeyTeam/${teamId}/players/${playerId}`,
      'Failed to update roster player',
      { method: 'PUT', ...jsonBody(data) },
    ),

  removePlayer: (teamId: string, playerId: string): Promise<HockeyTeamDto> =>
    hockeyRequest<HockeyTeamDto>(
      `/HockeyTeam/${teamId}/players/${playerId}`,
      'Failed to remove player from team',
      { method: 'DELETE' },
    ),

  addLine: (
    teamId: string,
    data: { name: string; lineNumber: number; lineType: HockeyLineType; competitionId?: string | null },
  ): Promise<HockeyTeamDto> =>
    hockeyRequest<HockeyTeamDto>(`/HockeyTeam/${teamId}/lines`, 'Failed to create line', {
      method: 'POST',
      ...jsonBody(data),
    }),

  removeLine: (teamId: string, lineId: string): Promise<HockeyTeamDto> =>
    hockeyRequest<HockeyTeamDto>(`/HockeyTeam/${teamId}/lines/${lineId}`, 'Failed to remove line', {
      method: 'DELETE',
    }),

  addPlayerToLine: (
    teamId: string,
    lineId: string,
    data: { teamPlayerId: string; slot: string; order: number },
  ): Promise<HockeyTeamDto> =>
    hockeyRequest<HockeyTeamDto>(
      `/HockeyTeam/${teamId}/lines/${lineId}/players`,
      'Failed to add player to line',
      {
        method: 'POST',
        ...jsonBody(data),
      },
    ),

  removePlayerFromLine: (teamId: string, lineId: string, teamPlayerId: string): Promise<HockeyTeamDto> =>
    hockeyRequest<HockeyTeamDto>(
      `/HockeyTeam/${teamId}/lines/${lineId}/players/${teamPlayerId}`,
      'Failed to remove player from line',
      { method: 'DELETE' },
    ),
};
