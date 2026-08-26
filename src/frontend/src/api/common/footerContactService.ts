import { API_URL } from '../../constants/config';
import { authFetch } from '../utils/authFetch';
import { parseErrorResponse } from '../utils/ParseErrorResponse';
import type { ApiResponse } from '../../types/common/apiResponseType';
import type { FooterContact, FooterContactRequest } from '../../types/admin/footerContactTypes';

function getBaseUrl(): string {
  return `${API_URL}/FooterContact`;
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

export const footerContactService = {
  async getAll(): Promise<FooterContact[]> {
    const response = await fetch(getBaseUrl(), {
      method: 'GET',
      headers: { 'Content-Type': 'application/json' },
    });

    return readResponse<FooterContact[]>(response, 'Failed to load footer contacts');
  },

  async create(payload: FooterContactRequest): Promise<FooterContact> {
    const response = await authFetch(getBaseUrl(), {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify(payload),
    });

    return readResponse<FooterContact>(response, 'Failed to create footer contact');
  },

  async update(id: string, payload: FooterContactRequest): Promise<FooterContact> {
    const response = await authFetch(`${getBaseUrl()}/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify(payload),
    });

    return readResponse<FooterContact>(response, 'Failed to update footer contact');
  },

  async remove(id: string): Promise<void> {
    const response = await authFetch(`${getBaseUrl()}/${id}`, {
      method: 'DELETE',
      credentials: 'include',
    });

    if (!response.ok) {
      throw new Error(await parseErrorResponse(response, 'Failed to delete footer contact'));
    }
  },
};
