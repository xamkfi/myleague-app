import { API_URL } from '../../constants/config';
import { authFetch } from '../utils/authFetch';
import type { ApiResponse } from '../../types/common/apiResponseType';
import type { SportsCategory } from '../../types/common/sports';
import type {
  CreateSeasonContentBlockRequest,
  SeasonContentBlockDto,
  UpdateSeasonContentBlockRequest,
} from '../../types/admin/seasonContentBlockTypes';

const BASE_URL = `${API_URL}/SeasonContentBlock`;

const parsePayload = async <T>(response: Response, defaultMessage: string): Promise<T> => {
  let payload: ApiResponse<T> | null = null;

  try {
    const text = await response.text();
    if (text.trim().length > 0) {
      payload = JSON.parse(text) as ApiResponse<T>;
    }
  } catch (error) {
    console.error('Failed to parse season content block API response', error);
  }

  if (!response.ok || !payload?.success || payload.data === undefined || payload.data === null) {
    throw new Error(
      payload?.message ||
        payload?.errors?.filter(Boolean).join(', ') ||
        defaultMessage,
    );
  }

  return payload.data;
};

export const seasonContentBlockService = {
  getByCompetition: async (competitionId: string): Promise<SeasonContentBlockDto[]> => {
    const response = await authFetch(
      `${BASE_URL}?competitionId=${encodeURIComponent(competitionId)}`,
      { method: 'GET' },
    );
    return parsePayload<SeasonContentBlockDto[]>(response, 'Failed to load season content blocks');
  },

  getBySportAndYear: async (
    sport: SportsCategory,
    seasonYear: string,
  ): Promise<SeasonContentBlockDto[]> => {
    const params = new URLSearchParams({ sport, seasonYear });
    const response = await authFetch(`${BASE_URL}?${params.toString()}`, { method: 'GET' });
    return parsePayload<SeasonContentBlockDto[]>(response, 'Failed to load season content blocks');
  },

  create: async (data: CreateSeasonContentBlockRequest): Promise<SeasonContentBlockDto> => {
    const response = await authFetch(BASE_URL, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    });
    return parsePayload<SeasonContentBlockDto>(response, 'Failed to create season content block');
  },

  update: async (
    id: string,
    data: UpdateSeasonContentBlockRequest,
  ): Promise<SeasonContentBlockDto> => {
    const response = await authFetch(`${BASE_URL}/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    });
    return parsePayload<SeasonContentBlockDto>(response, 'Failed to update season content block');
  },

  reorder: async (orderedIds: string[]): Promise<SeasonContentBlockDto[]> => {
    const response = await authFetch(`${BASE_URL}/reorder`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ orderedIds }),
    });
    return parsePayload<SeasonContentBlockDto[]>(response, 'Failed to reorder season content blocks');
  },

  delete: async (id: string): Promise<void> => {
    const response = await authFetch(`${BASE_URL}/${id}`, { method: 'DELETE' });
    await parsePayload<boolean>(response, 'Failed to delete season content block');
  },
};
