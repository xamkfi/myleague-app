import { API_URL } from '../../constants/config';
import { authFetch } from '../utils/authFetch';
import { parseErrorResponse } from '../utils/ParseErrorResponse';
import type { ApiResponse } from '../../types/common/apiResponseType';
import type { PaginatedApiResponse } from '../../types/hockey/hockeyTypes';

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

export async function hockeyPagedRequest<T>(
  path: string,
  fallback: string,
  init?: RequestInit,
): Promise<PaginatedApiResponse<T>> {
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

  let payload: PaginatedApiResponse<T> | null = null;
  try {
    payload = (await response.json()) as PaginatedApiResponse<T>;
  } catch {
    payload = null;
  }

  if (!response.ok || !payload?.success) {
    const message = await parseErrorResponse(payload ?? response, fallback);
    throw new Error(message);
  }

  return {
    ...payload,
    data: payload.data ?? [],
    pagination: payload.pagination ?? {
      currentPage: 1,
      pageSize: 0,
      totalCount: 0,
      totalPages: 0,
      hasNextPage: false,
      hasPreviousPage: false,
      startItem: 0,
      endItem: 0,
    },
  };
}

export async function loadAllPaged<T>(
  fetchPage: (page: number, pageSize: number) => Promise<PaginatedApiResponse<T>>,
  pageSize: number = 100,
): Promise<T[]> {
  const first = await fetchPage(1, pageSize);
  const items: T[] = [...first.data];
  for (let page = 2; page <= first.pagination.totalPages; page += 1) {
    const next = await fetchPage(page, pageSize);
    items.push(...next.data);
  }
  return items;
}

export function toQueryString(params: Record<string, string | number | boolean | undefined | null>): string {
  const search = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value === undefined || value === null || value === '') {
      continue;
    }
    search.append(key, String(value));
  }
  const query = search.toString();
  return query ? `?${query}` : '';
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
