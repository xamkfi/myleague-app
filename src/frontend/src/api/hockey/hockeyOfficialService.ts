import type {
  CreateHockeyOfficialRequest,
  GetPagedHockeyOfficialsRequest,
  HockeyOfficialDto,
  PaginatedApiResponse,
  UpdateHockeyOfficialRequest,
} from '../../types/hockey/hockeyTypes';
import { hockeyPagedRequest, hockeyRequest, jsonBody, toQueryString } from './hockeyApi';

export const hockeyOfficialService = {
  getPaged: (params: GetPagedHockeyOfficialsRequest = {}): Promise<PaginatedApiResponse<HockeyOfficialDto>> =>
    hockeyPagedRequest<HockeyOfficialDto>(
      `/HockeyOfficial/paged${toQueryString({
        page: params.page,
        pageSize: params.pageSize,
        isActive: params.isActive,
        searchTerm: params.searchTerm,
        licenseExpiringWithinDays: params.licenseExpiringWithinDays,
      })}`,
      'Failed to fetch hockey officials',
    ),

  getAll: (isActive?: boolean): Promise<HockeyOfficialDto[]> => {
    const query = isActive === undefined ? '' : `?isActive=${String(isActive)}`;
    return hockeyRequest<HockeyOfficialDto[]>(
      `/HockeyOfficial${query}`,
      'Failed to fetch hockey officials',
    );
  },

  getById: (id: string): Promise<HockeyOfficialDto> =>
    hockeyRequest<HockeyOfficialDto>(`/HockeyOfficial/${id}`, 'Failed to fetch hockey official'),

  create: (data: CreateHockeyOfficialRequest): Promise<HockeyOfficialDto> =>
    hockeyRequest<HockeyOfficialDto>('/HockeyOfficial', 'Failed to create hockey official', {
      method: 'POST',
      ...jsonBody(data),
    }),

  update: (id: string, data: UpdateHockeyOfficialRequest): Promise<HockeyOfficialDto> =>
    hockeyRequest<HockeyOfficialDto>(`/HockeyOfficial/${id}`, 'Failed to update hockey official', {
      method: 'PUT',
      ...jsonBody(data),
    }),
};
