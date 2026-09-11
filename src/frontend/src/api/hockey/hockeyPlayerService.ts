import type {
  CreateHockeyPlayerRequest,
  GetPagedHockeyPlayersRequest,
  HockeyPlayerDto,
  PaginatedApiResponse,
} from '../../types/hockey/hockeyTypes';
import { hockeyPagedRequest, hockeyRequest, jsonBody, loadAllPaged, toQueryString } from './hockeyApi';

export const hockeyPlayerService = {
  getPaged: (params: GetPagedHockeyPlayersRequest = {}): Promise<PaginatedApiResponse<HockeyPlayerDto>> =>
    hockeyPagedRequest<HockeyPlayerDto>(
      `/HockeyPlayer/paged${toQueryString({
        page: params.page,
        pageSize: params.pageSize,
        searchTerm: params.searchTerm,
        isActive: params.isActive,
        position: params.position,
        clubId: params.clubId,
        teamId: params.teamId,
        teamCategory: params.teamCategory,
      })}`,
      'Failed to fetch hockey players',
    ),

  getAllPages: (params: Omit<GetPagedHockeyPlayersRequest, 'page' | 'pageSize'> = {}): Promise<HockeyPlayerDto[]> =>
    loadAllPaged((page, pageSize) => hockeyPlayerService.getPaged({ ...params, page, pageSize })),

  getById: (id: string): Promise<HockeyPlayerDto> =>
    hockeyRequest<HockeyPlayerDto>(`/HockeyPlayer/${id}`, 'Failed to fetch hockey player'),

  create: (data: CreateHockeyPlayerRequest): Promise<HockeyPlayerDto> =>
    hockeyRequest<HockeyPlayerDto>('/HockeyPlayer', 'Failed to create hockey player', {
      method: 'POST',
      ...jsonBody(data),
    }),
};
