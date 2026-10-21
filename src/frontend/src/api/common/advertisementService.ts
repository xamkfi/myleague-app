import { API_URL } from '../../constants/config';
import { authFetch } from '../utils/authFetch';
import { parseErrorResponse } from '../utils/ParseErrorResponse';
import type { ApiResponse } from '../../types/common/apiResponseType';
import type {
  Advertisement,
  AdvertisementRequest,
  PublicAdvertisement,
} from '../../types/admin/advertisementTypes';

function getBaseUrl(): string {
  return `${API_URL}/Advertisements`;
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

let visibleRequest: Promise<PublicAdvertisement[]> | null = null;

function invalidateVisibleAdvertisements(): void {
  visibleRequest = null;
}

export const advertisementService = {
  /** Ads that are active and scheduled right now. Shared by every caller until an admin write. */
  getVisible(): Promise<PublicAdvertisement[]> {
    if (!visibleRequest) {
      visibleRequest = fetch(`${getBaseUrl()}/visible`, {
        method: 'GET',
        headers: { 'Content-Type': 'application/json' },
      })
        .then((response) => readResponse<PublicAdvertisement[]>(response, 'Failed to load advertisements'))
        .catch((error: unknown) => {
          visibleRequest = null;
          throw error;
        });
    }

    return visibleRequest;
  },

  async getAll(): Promise<Advertisement[]> {
    const response = await authFetch(getBaseUrl(), {
      method: 'GET',
      credentials: 'include',
    });

    return readResponse<Advertisement[]>(response, 'Failed to load advertisements');
  },

  async create(payload: AdvertisementRequest): Promise<Advertisement> {
    const response = await authFetch(getBaseUrl(), {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify(payload),
    });

    const created = await readResponse<Advertisement>(response, 'Failed to create advertisement');
    invalidateVisibleAdvertisements();
    return created;
  },

  async update(id: string, payload: AdvertisementRequest): Promise<Advertisement> {
    const response = await authFetch(`${getBaseUrl()}/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify(payload),
    });

    const updated = await readResponse<Advertisement>(response, 'Failed to update advertisement');
    invalidateVisibleAdvertisements();
    return updated;
  },

  async remove(id: string): Promise<void> {
    const response = await authFetch(`${getBaseUrl()}/${id}`, {
      method: 'DELETE',
      credentials: 'include',
    });

    if (!response.ok) {
      throw new Error(await parseErrorResponse(response, 'Failed to delete advertisement'));
    }

    invalidateVisibleAdvertisements();
  },

  async uploadImage(file: File): Promise<string> {
    const formData = new FormData();
    formData.append('file', file);

    const response = await authFetch(`${getBaseUrl()}/upload-image`, {
      method: 'POST',
      credentials: 'include',
      body: formData,
    });

    return readResponse<string>(response, 'Image upload failed');
  },
};
