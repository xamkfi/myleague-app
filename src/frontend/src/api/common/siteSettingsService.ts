import { API_URL } from '../../constants/config';
import { authFetch } from '../utils/authFetch';
import { parseErrorResponse } from '../utils/ParseErrorResponse';

interface ApiResponse<T> {
  success: boolean;
  data: T;
  message: string;
  errors: string[];
}

export interface FooterContactPerson {
  nameOrRole: string;
  email: string;
  phone: string;
}

export interface FooterContactSettings {
  organizationName: string;
  organizationAddress: string;
  lastModifiedBy: string | null;
  updatedAt: string | null;
  contactPersons: FooterContactPerson[];
}

export interface UpdateFooterContactSettingsRequest {
  organizationName: string;
  organizationAddress: string;
  contactPersons: FooterContactPerson[];
}

const BASE_URL = `${API_URL}/site-settings/footer-contact`;

export const siteSettingsService = {
  getFooterContact: async (): Promise<FooterContactSettings> => {
    const response = await fetch(BASE_URL, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    const data: ApiResponse<FooterContactSettings> = await response.json();

    if (!response.ok || !data?.success) {
      const errorMessage = await parseErrorResponse(data, 'Failed to fetch footer contact settings');
      throw new Error(errorMessage || 'Failed to fetch footer contact settings');
    }

    return data.data;
  },

  updateFooterContact: async (payload: UpdateFooterContactSettingsRequest): Promise<FooterContactSettings> => {
    const response = await authFetch(BASE_URL, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(payload),
    });

    const data: ApiResponse<FooterContactSettings> = await response.json();

    if (!response.ok || !data?.success) {
      const errorMessage = await parseErrorResponse(data, 'Failed to update footer contact settings');
      throw new Error(errorMessage || 'Failed to update footer contact settings');
    }

    return data.data;
  },
};
