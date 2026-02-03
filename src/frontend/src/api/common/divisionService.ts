import { API_URL } from '../../constants/config';
import type {
  CreateDivisionInput,
  DivisionType,
  UpdateDivisionInput,
} from '../../types/common/divisionType';
import type { SportsCategory } from '../../types/common/sports';
import type { ApiResponse } from '../../types/common/apiResponseType';

const BASE_URL = `${API_URL}/Divisions`;

const parseApiResponse = async <T>(
  response: Response,
  defaultMessage: string,
): Promise<ApiResponse<T>> => {
  let payload: ApiResponse<T> | null = null;

  try {
    payload = await response.json();
  } catch (error) {
    console.error('Failed to parse division API response', error);
  }

  if (!payload) {
    throw new Error(defaultMessage);
  }

  if (!response.ok || !payload.success) {
    const errorMessage =
      payload.errors?.join(', ') || payload.message || defaultMessage;
    throw new Error(errorMessage);
  }

  return payload;
};

export const divisionService = {
  getAll: async (): Promise<ApiResponse<DivisionType[]>> => {
    const response = await fetch(BASE_URL);
    return parseApiResponse<DivisionType[]>(response, 'Failed to load divisions');
  },

  getById: async (id: string): Promise<ApiResponse<DivisionType>> => {
    const response = await fetch(`${BASE_URL}/${id}`);
    return parseApiResponse<DivisionType>(response, 'Failed to load division');
  },

  getBySportType: async (
    sportType: SportsCategory,
    activeOnly = false,
  ): Promise<ApiResponse<DivisionType[]>> => {
    const url = `${BASE_URL}/sport/${encodeURIComponent(
      sportType,
    )}?activeOnly=${activeOnly}`;
    const response = await fetch(url);
    return parseApiResponse<DivisionType[]>(
      response,
      'Failed to load divisions by sport type',
    );
  },

  create: async (
    payload: CreateDivisionInput,
  ): Promise<ApiResponse<DivisionType>> => {
    const response = await fetch(BASE_URL, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(payload),
    });

    return parseApiResponse<DivisionType>(response, 'Failed to create division');
  },

  update: async (
    id: string,
    payload: UpdateDivisionInput,
  ): Promise<ApiResponse<DivisionType>> => {
    const response = await fetch(`${BASE_URL}/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(payload),
    });

    return parseApiResponse<DivisionType>(response, 'Failed to update division');
  },

  activate: async (id: string): Promise<ApiResponse<void>> => {
    const response = await fetch(`${BASE_URL}/${id}/activate`, {
      method: 'PATCH',
    });

    return parseApiResponse<void>(response, 'Failed to activate division');
  },

  deactivate: async (id: string): Promise<ApiResponse<void>> => {
    const response = await fetch(`${BASE_URL}/${id}/deactivate`, {
      method: 'PATCH',
    });

    return parseApiResponse<void>(response, 'Failed to deactivate division');
  },

  delete: async (id: string): Promise<ApiResponse<void>> => {
    const response = await fetch(`${BASE_URL}/${id}`, {
      method: 'DELETE',
    });

    return parseApiResponse<void>(response, 'Failed to delete division');
  },
};