import type {
  CreateHockeySeasonRequest,
  HockeySeasonDto,
  UpdateHockeySeasonRequest,
} from '../../types/hockey/hockeyTypes';
import { hockeyRequest, jsonBody, withTeamCategory } from './hockeyApi';

const action = (seasonId: string, verb: string, fallback: string): Promise<HockeySeasonDto> =>
  hockeyRequest<HockeySeasonDto>(`/HockeySeason/${seasonId}/${verb}`, fallback, { method: 'POST' });

export const hockeySeasonService = {
  getAll: (teamCategory?: string): Promise<HockeySeasonDto[]> =>
    hockeyRequest<HockeySeasonDto[]>(
      withTeamCategory('/HockeySeason', teamCategory),
      'Failed to fetch hockey seasons',
    ),

  getActive: (teamCategory?: string): Promise<HockeySeasonDto[]> =>
    hockeyRequest<HockeySeasonDto[]>(
      withTeamCategory('/HockeySeason/active', teamCategory),
      'Failed to fetch active hockey seasons',
    ),

  getById: (id: string): Promise<HockeySeasonDto> =>
    hockeyRequest<HockeySeasonDto>(`/HockeySeason/${id}`, 'Failed to fetch hockey season'),

  create: (data: CreateHockeySeasonRequest): Promise<HockeySeasonDto> =>
    hockeyRequest<HockeySeasonDto>('/HockeySeason', 'Failed to create hockey season', {
      method: 'POST',
      ...jsonBody(data),
    }),

  update: (id: string, data: UpdateHockeySeasonRequest): Promise<HockeySeasonDto> =>
    hockeyRequest<HockeySeasonDto>(`/HockeySeason/${id}`, 'Failed to update hockey season', {
      method: 'PUT',
      ...jsonBody(data),
    }),

  publish: (id: string): Promise<HockeySeasonDto> => action(id, 'publish', 'Failed to publish season'),
  openRegistration: (id: string): Promise<HockeySeasonDto> =>
    action(id, 'open-registration', 'Failed to open registration'),
  activate: (id: string): Promise<HockeySeasonDto> => action(id, 'activate', 'Failed to activate season'),
  deactivate: (id: string): Promise<HockeySeasonDto> =>
    action(id, 'deactivate', 'Failed to deactivate season'),
  complete: (id: string): Promise<HockeySeasonDto> => action(id, 'complete', 'Failed to complete season'),
  cancel: (id: string): Promise<HockeySeasonDto> => action(id, 'cancel', 'Failed to cancel season'),

  addTeam: (seasonId: string, teamId: string, seed?: number): Promise<unknown> =>
    hockeyRequest(`/HockeySeason/${seasonId}/teams`, 'Failed to add team to season', {
      method: 'POST',
      ...jsonBody({ teamId, seed }),
    }),

  removeTeam: (seasonId: string, teamId: string): Promise<HockeySeasonDto> =>
    hockeyRequest<HockeySeasonDto>(
      `/HockeySeason/${seasonId}/teams/${teamId}`,
      'Failed to remove team from season',
      { method: 'DELETE' },
    ),

  addDivision: (
    seasonId: string,
    divisionId: string,
    name: string,
    sortOrder: number,
  ): Promise<HockeySeasonDto> =>
    hockeyRequest<HockeySeasonDto>(`/HockeySeason/${seasonId}/divisions`, 'Failed to add division', {
      method: 'POST',
      ...jsonBody({ divisionId, name, sortOrder }),
    }),

  removeDivision: (seasonId: string, competitionDivisionId: string): Promise<HockeySeasonDto> =>
    hockeyRequest<HockeySeasonDto>(
      `/HockeySeason/${seasonId}/divisions/${competitionDivisionId}`,
      'Failed to remove division',
      { method: 'DELETE' },
    ),

  addTeamToDivision: (
    seasonId: string,
    competitionDivisionId: string,
    competitionTeamId: string,
    seed?: number,
  ): Promise<HockeySeasonDto> =>
    hockeyRequest<HockeySeasonDto>(
      `/HockeySeason/${seasonId}/divisions/${competitionDivisionId}/teams`,
      'Failed to place team in division',
      { method: 'POST', ...jsonBody({ competitionTeamId, seed }) },
    ),
};
