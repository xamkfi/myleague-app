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
  const apiResponse: ApiResponse<TMatch> = await response.json();
  if (!response.ok) {
    throw new Error(await parseErrorResponse(apiResponse, fallbackError));
  }
  if (!apiResponse.success) {
    throw new Error(apiResponse.errors?.join(', ') || fallbackError);
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
