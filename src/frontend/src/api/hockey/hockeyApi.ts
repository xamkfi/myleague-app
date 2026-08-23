import { API_URL } from '../../constants/config';
import { authFetch } from '../utils/authFetch';
import { parseErrorResponse } from '../utils/ParseErrorResponse';
import type { ApiResponse } from '../../types/common/apiResponseType';

export function withTeamCategory(path: string, teamCategory?: string): string {
  if (!teamCategory) {
    return path;
  }
  const separator = path.includes('?') ? '&' : '?';
  return `${path}${separator}teamCategory=${encodeURIComponent(teamCategory)}`;
}

export async function hockeyRequest<T>(
  path: string,
  fallback: string,
  init?: RequestInit,
): Promise<T> {
  const headers: Record<string, string> = {
    ...(init?.body ? { 'Content-Type': 'application/json' } : {}),
  };
  if (init?.headers) {
    Object.assign(headers, init.headers);
  }

  const response = await authFetch(`${API_URL}${path}`, {
    ...init,
    headers,
  });

  let payload: ApiResponse<T> | null = null;
  try {
    payload = (await response.json()) as ApiResponse<T>;
  } catch {
    payload = null;
  }

  if (!response.ok || !payload?.success) {
    const message = await parseErrorResponse(payload ?? response, fallback);
    throw new Error(message);
  }

  return payload.data;
}

export async function hockeyRequestVoid(
  path: string,
  fallback: string,
  init?: RequestInit,
): Promise<void> {
  await hockeyRequest<unknown>(path, fallback, init);
}

export function jsonBody(body: unknown): RequestInit {
  return {
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  };
}
