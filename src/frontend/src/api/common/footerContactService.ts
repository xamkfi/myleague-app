import { API_URL } from '../../constants/config';
import { authFetch } from '../utils/authFetch';
import { parseErrorResponse } from '../utils/ParseErrorResponse';
import type { ApiResponse } from '../../types/common/apiResponseType';
import type { FooterContact, FooterContactRequest, FooterSection } from '../../types/admin/footerContactTypes';

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

let cachedPublicContacts: FooterContact[] | null = null;
let publicContactsRequest: Promise<FooterContact[]> | null = null;

function invalidatePublicFooterContacts(): void {
  cachedPublicContacts = null;
  publicContactsRequest = null;
}

/** Already loaded public footer rows, or null before the first successful fetch. */
export function peekFooterContacts(): FooterContact[] | null {
  return cachedPublicContacts;
}

async function fetchContacts(section?: FooterSection): Promise<FooterContact[]> {
  const url = section
    ? `${getBaseUrl()}?section=${encodeURIComponent(section)}`
    : getBaseUrl();
  const response = await fetch(url, {
    method: 'GET',
    headers: { 'Content-Type': 'application/json' },
  });

  return readResponse<FooterContact[]>(response, 'Failed to load footer contacts');
}

function loadPublicContacts(): Promise<FooterContact[]> {
  if (cachedPublicContacts) {
    return Promise.resolve(cachedPublicContacts);
  }

  if (!publicContactsRequest) {
    publicContactsRequest = fetchContacts()
      .then((items) => {
        cachedPublicContacts = items;
        return items;
      })
      .catch((error: unknown) => {
        publicContactsRequest = null;
        throw error;
      });
  }

  return publicContactsRequest;
}

export const footerContactService = {
  async getAll(section?: FooterSection): Promise<FooterContact[]> {
    if (section) {
      return fetchContacts(section);
    }

    return loadPublicContacts();
  },

  async create(payload: FooterContactRequest): Promise<FooterContact> {
    const response = await authFetch(getBaseUrl(), {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify(payload),
    });

    const created = await readResponse<FooterContact>(response, 'Failed to create footer contact');
    invalidatePublicFooterContacts();
    return created;
  },

  async update(id: string, payload: FooterContactRequest): Promise<FooterContact> {
    const response = await authFetch(`${getBaseUrl()}/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify(payload),
    });

    const updated = await readResponse<FooterContact>(response, 'Failed to update footer contact');
    invalidatePublicFooterContacts();
    return updated;
  },

  async remove(id: string): Promise<void> {
    const response = await authFetch(`${getBaseUrl()}/${id}`, {
      method: 'DELETE',
      credentials: 'include',
    });

    if (!response.ok) {
      throw new Error(await parseErrorResponse(response, 'Failed to delete footer contact'));
    }

    invalidatePublicFooterContacts();
  },
};
