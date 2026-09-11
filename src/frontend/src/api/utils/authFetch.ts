import {
  getValidAccessToken,
  recordActivity,
  refreshTokens,
} from './tokenManager';

/**
 * Authenticated fetch wrapper that automatically injects the JWT access token in the
 * Authorization header. Provides two layers of protection against expired tokens:
 *
 *  1. Before sending, calls `getValidAccessToken()` which refreshes proactively when the
 *     access token is expired or close to expiry.
 *  2. If the server still answers `401 Unauthorized` (for example because the token was
 *     revoked server-side, or clock skew made the proactive check miss), it attempts a
 *     single refresh and replays the original request once.
 *
 * Safe to use for all requests – if no token is stored the request proceeds without an
 * Authorization header (behaves identically to plain `fetch`).
 */
export async function authFetch(
  input: RequestInfo | URL,
  init?: RequestInit,
): Promise<Response> {
  // Each outgoing API call counts as user activity so the proactive refresh loop knows
  // the session is in use.
  recordActivity();

  const token = await getValidAccessToken();

  if (!token) {
    return fetch(input, init);
  }

  const response = await sendWithToken(input, init, token);

  if (response.status !== 401) {
    return response;
  }

  // Server rejected the (presumably valid-looking) token. Try a single refresh and replay.
  const refreshed = await refreshTokens();
  if (!refreshed) {
    return response;
  }

  return sendWithToken(input, init, refreshed.accessToken);
}

function sendWithToken(
  input: RequestInfo | URL,
  init: RequestInit | undefined,
  token: string,
): Promise<Response> {
  const mergedHeaders = new Headers(init?.headers);
  mergedHeaders.set('Authorization', `Bearer ${token}`);

  return fetch(input, {
    ...init,
    headers: mergedHeaders,
  });
}
