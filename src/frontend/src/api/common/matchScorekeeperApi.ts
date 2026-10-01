import { API_URL } from '../../constants/config';
import { authFetch } from '../utils/authFetch';
import { parseErrorResponse } from '../utils/ParseErrorResponse';

interface ApiResponse<T> {
  success: boolean;
  data: T;
  message: string;
  errors: string[];
}

async function sendScorekeeperRequest<TMatch>(
  url: string,
  init: RequestInit,
  fallbackError: string,
): Promise<ApiResponse<TMatch>> {
  const response = await authFetch(url, init);
  const raw: string = await response.text();
  let apiResponse: ApiResponse<TMatch> | null = null;
  if (raw.length > 0) {
    try {
      apiResponse = JSON.parse(raw) as ApiResponse<TMatch>;
    } catch (error: unknown) {
      if (error instanceof SyntaxError) {
        throw new Error(fallbackError);
      }
      throw error;
    }
  }
  if (!response.ok) {
    throw new Error(apiResponse ? await parseErrorResponse(apiResponse, fallbackError) : fallbackError);
  }
  if (!apiResponse?.success) {
    throw new Error(apiResponse?.errors?.join(', ') || fallbackError);
  }
  return apiResponse;
}

/**
 * Adds a person as a scorekeeper (toimitsija) to a match under `api/{matchesPath}/{matchId}/scorekeepers`.
 */
export function addMatchScorekeeper<TMatch>(
  matchesPath: string,
  matchId: string,
  personId: string,
): Promise<ApiResponse<TMatch>> {
  return sendScorekeeperRequest<TMatch>(
    `${API_URL}/${matchesPath}/${matchId}/scorekeepers`,
    {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ personId }),
    },
    'Failed to add scorekeeper',
  );
}

/**
 * Removes a scorekeeper (toimitsija) from a match.
 */
export function removeMatchScorekeeper<TMatch>(
  matchesPath: string,
  matchId: string,
  personId: string,
): Promise<ApiResponse<TMatch>> {
  return sendScorekeeperRequest<TMatch>(
    `${API_URL}/${matchesPath}/${matchId}/scorekeepers/${personId}`,
    { method: 'DELETE' },
    'Failed to remove scorekeeper',
  );
}
