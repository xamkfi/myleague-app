import type { CreateHockeyPlayerRequest, HockeyPlayerDto } from '../../types/hockey/hockeyTypes';
import { hockeyRequest, jsonBody } from './hockeyApi';

export const hockeyPlayerService = {
  getById: (id: string): Promise<HockeyPlayerDto> =>
    hockeyRequest<HockeyPlayerDto>(`/HockeyPlayer/${id}`, 'Failed to fetch hockey player'),

  create: (data: CreateHockeyPlayerRequest): Promise<HockeyPlayerDto> =>
    hockeyRequest<HockeyPlayerDto>('/HockeyPlayer', 'Failed to create hockey player', {
      method: 'POST',
      ...jsonBody(data),
    }),
};
