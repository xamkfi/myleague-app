const TOKEN_STORAGE_KEY = 'myleague_auth_tokens';

interface StoredTokens {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
}

/**
 * Authenticated fetch wrapper that automatically includes the JWT access token
 * from localStorage in the Authorization header.
 *
 * Safe to use for all requests – if no token is stored, the request proceeds
 * without an Authorization header (behaves identically to plain `fetch`).
 */
export async function authFetch(
  input: RequestInfo | URL,
  init?: RequestInit,
): Promise<Response> {
  const token = getAccessToken();

  if (!token) {
    return fetch(input, init);
  }

  const mergedHeaders = new Headers(init?.headers);
  mergedHeaders.set('Authorization', `Bearer ${token}`);

  return fetch(input, {
    ...init,
    headers: mergedHeaders,
  });
}

function getAccessToken(): string | null {
  try {
    const raw = localStorage.getItem(TOKEN_STORAGE_KEY);
    if (!raw) return null;
    const tokens: StoredTokens = JSON.parse(raw);
    return tokens.accessToken ?? null;
  } catch {
    return null;
  }
}
