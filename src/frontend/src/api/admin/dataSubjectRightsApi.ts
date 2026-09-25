import { API_URL } from '../../constants/config';
import { authFetch } from '../utils/authFetch';
import { parseErrorResponse } from '../utils/ParseErrorResponse';

export interface DataSubjectAddress {
  street1: string | null;
  street2: string | null;
  city: string | null;
  postalCode: string | null;
  country: string | null;
}

export interface DataSubjectContact {
  email: string | null;
  phone: string | null;
  alternativePhone: string | null;
}

export interface DataSubjectAccount {
  userId: string;
  email: string;
  role: string;
  isActive: boolean;
  lastLoginAt: string | null;
}

export interface DataSubjectPortableData {
  firstName: string;
  lastName: string;
  birthDate: string | null;
  address: DataSubjectAddress | null;
  contactInfo: DataSubjectContact | null;
  accountEmail: string | null;
}

export interface DataSubjectCopy {
  personId: string;
  isProcessed: boolean;
  firstName: string;
  lastName: string;
  fullName: string;
  birthDate: string | null;
  role: string;
  isRegistered: boolean;
  address: DataSubjectAddress | null;
  contactInfo: DataSubjectContact | null;
  isProcessingRestricted: boolean;
  hasObjectedToLegitimateInterest: boolean;
  anonymizedAt: string | null;
  retentionReason: string | null;
  account: DataSubjectAccount | null;
  portableData: DataSubjectPortableData;
}

export interface RectifyDataSubjectPayload {
  firstName: string;
  lastName: string;
  birthDate: string | null;
  address: DataSubjectAddress | null;
  contactInfo: DataSubjectContact | null;
  accountEmail: string | null;
}

interface ApiResponse<T> {
  success: boolean;
  data: T;
  message: string;
  errors: string[];
}

async function readCopy(response: Response, fallback: string): Promise<DataSubjectCopy> {
  const text = await response.text();
  if (!text) {
    throw new Error(response.ok ? fallback : `${fallback} (${response.status})`);
  }

  let apiResponse: ApiResponse<DataSubjectCopy>;
  try {
    apiResponse = JSON.parse(text) as ApiResponse<DataSubjectCopy>;
  } catch {
    throw new Error(response.ok ? fallback : `${fallback} (${response.status})`);
  }

  if (!response.ok || !apiResponse.success) {
    const errorMessage = await parseErrorResponse(apiResponse, fallback);
    throw new Error(errorMessage || fallback);
  }
  return apiResponse.data;
}

export const dataSubjectRightsApi = {
  getCopy: async (personId: string): Promise<DataSubjectCopy> => {
    const response = await authFetch(`${API_URL}/data-subject-rights/${personId}`);
    return readCopy(response, 'Failed to load data-subject copy');
  },

  rectify: async (personId: string, payload: RectifyDataSubjectPayload): Promise<DataSubjectCopy> => {
    const response = await authFetch(`${API_URL}/data-subject-rights/${personId}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
    });
    return readCopy(response, 'Failed to rectify data-subject data');
  },

  erase: async (personId: string): Promise<DataSubjectCopy> => {
    const response = await authFetch(`${API_URL}/data-subject-rights/${personId}/erasure`, {
      method: 'POST',
    });
    return readCopy(response, 'Failed to erase data-subject data');
  },

  setRestriction: async (personId: string, enabled: boolean): Promise<DataSubjectCopy> => {
    const response = await authFetch(`${API_URL}/data-subject-rights/${personId}/restriction`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ enabled }),
    });
    return readCopy(response, 'Failed to update processing restriction');
  },

  setObjection: async (personId: string, enabled: boolean): Promise<DataSubjectCopy> => {
    const response = await authFetch(`${API_URL}/data-subject-rights/${personId}/objection`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ enabled }),
    });
    return readCopy(response, 'Failed to update objection');
  },
};
