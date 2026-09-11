import { API_URL } from '../../constants/config';
import { authFetch } from '../utils/authFetch';
import { parseErrorResponse } from '../utils/ParseErrorResponse';
import type { ApiResponse } from '../../types/common/apiResponseType';
import type { SiteSettings, SiteSettingsRequest } from '../../types/admin/siteSettingsTypes';

function getBaseUrl(): string {
  return `${API_URL}/site-settings`;
}

async function readResponse<T>(response: Response, fallback: string): Promise<T> {
  if (!response.ok) {
    throw new Error(await parseErrorResponse(response, fallback));
  }

  const result: ApiResponse<T> = await response.json();

  if (!result.success || result.data === undefined || result.data === null) {
    throw new Error(result.message || result.errors?.filter(Boolean).join(', ') || fallback);
  }

  return result.data;
}

export const siteSettingsService = {
  async get(): Promise<SiteSettings> {
    const response = await authFetch(getBaseUrl(), {
      method: 'GET',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
    });

    return readResponse<SiteSettings>(response, 'Failed to load site settings');
  },

  async update(payload: SiteSettingsRequest): Promise<SiteSettings> {
    const response = await authFetch(getBaseUrl(), {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify(payload),
    });

    return readResponse<SiteSettings>(response, 'Failed to save site settings');
  },
};
