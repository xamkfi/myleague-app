import { API_URL } from '../../constants/config';
import type { ApiResponse } from '../../types/common/apiResponseType';
import type { AuthTokenResponse, AuthUser } from '../../types/auth/authTypes';
import { parseErrorResponse } from '../utils/ParseErrorResponse';

const BASE_URL = `${API_URL}/Auth`;

export const authService = {
  /**
   * Request a login code to be sent to the specified email.
   * Always returns 200 to prevent email enumeration.
   * When the backend `LoginCode:AutoFillLoginCode` flag is enabled, the response includes
   * `autoFillCode` so the login page can pre-fill the verification input. The flag is intended
   * for local development / trusted internal environments only.
   */
  requestLoginCode: async (email: string): Promise<{ message: string; autoFillCode?: string }> => {
    const response = await fetch(`${BASE_URL}/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email }),
    });
    const data = await response.json();

    if (!response.ok || !data?.success) {
      const errorMessage = await parseErrorResponse(data, 'Failed to request login code');
      throw new Error(errorMessage || 'Failed to request login code');
    }

    return {
      message: data.message as string,
      autoFillCode: data.data?.autoFillCode as string | undefined,
    };
  },

  /**
   * Verify the login code and receive authentication tokens.
   */
  verifyLoginCode: async (email: string, code: string): Promise<AuthTokenResponse> => {
    const response = await fetch(`${BASE_URL}/verify`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, code }),
    });
    const data: ApiResponse<AuthTokenResponse> = await response.json();

    if (!response.ok || !data?.success) {
      const errorMessage = await parseErrorResponse(data, 'Invalid login code');
      throw new Error(errorMessage || 'Invalid login code');
    }

    return data.data;
  },

  /**
   * Refresh authentication tokens using a valid refresh token.
   */
  refreshTokens: async (refreshToken: string): Promise<AuthTokenResponse> => {
    const response = await fetch(`${BASE_URL}/refresh`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ refreshToken }),
    });
    const data: ApiResponse<AuthTokenResponse> = await response.json();

    if (!response.ok || !data?.success) {
      const errorMessage = await parseErrorResponse(data, 'Failed to refresh tokens');
      throw new Error(errorMessage || 'Failed to refresh tokens');
    }

    return data.data;
  },

  /**
   * Logout by revoking the refresh token.
   */
  logout: async (refreshToken: string): Promise<void> => {
    const response = await fetch(`${BASE_URL}/logout`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ refreshToken }),
    });
    const data = await response.json();

    if (!response.ok || !data?.success) {
      const errorMessage = await parseErrorResponse(data, 'Failed to logout');
      throw new Error(errorMessage || 'Failed to logout');
    }
  },

  /**
   * Verify a new admin's email address using the token from the invitation email.
   * On success the account is activated and a welcome email with login instructions is sent.
   */
  verifyAdminEmail: async (token: string): Promise<void> => {
    const response = await fetch(`${BASE_URL}/verify-admin-email`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ token }),
    });
    const data = await response.json();

    if (!response.ok || !data?.success) {
      const errorMessage = await parseErrorResponse(data, 'Email verification failed');
      throw new Error(errorMessage || 'Email verification failed');
    }
  },

  /**
   * Get the current authenticated user's information.
   */
  getMe: async (accessToken: string): Promise<AuthUser> => {
    const response = await fetch(`${BASE_URL}/me`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${accessToken}`,
      },
    });
    const data: ApiResponse<AuthUser> = await response.json();

    if (!response.ok || !data?.success) {
      const errorMessage = await parseErrorResponse(data, 'Failed to get user info');
      throw new Error(errorMessage || 'Failed to get user info');
    }

    return data.data;
  },
};
