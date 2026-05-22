import { authService } from '../auth/authService';

const TOKEN_STORAGE_KEY = 'myleague_auth_tokens';

// Refresh the access token when this much time (or less) is left before it expires.
// Access tokens currently live for 15 minutes (see backend Jwt:AccessTokenExpirationMinutes),
// so refreshing with 3 min of headroom guarantees we never serve a stale token even if the
// periodic check is slightly delayed by background-tab throttling or short sleep periods.
const REFRESH_BUFFER_MS = 3 * 60_000;

// How often we re-check whether the access token needs refreshing while the app is open.
// Using an interval (rather than a single setTimeout) means we self-heal after the browser
// returns from sleep / suspends timers — the next tick simply notices the token is near
// expiry and refreshes it.
const REFRESH_CHECK_INTERVAL_MS = 30_000;

// After this much inactivity the user is considered idle and we stop proactively refreshing.
// The refresh token still lives in storage so a subsequent activity event (or API call) can
// recover the session, but we don't keep extending the session indefinitely for an idle tab.
const INACTIVITY_THRESHOLD_MS = 30 * 60_000;

export interface StoredTokens {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
}

type AuthListener = (tokens: StoredTokens | null) => void;

const listeners: Set<AuthListener> = new Set();
let inFlightRefresh: Promise<StoredTokens | null> | null = null;
let lastActivityAt: number = Date.now();
let periodicCheckHandle: ReturnType<typeof setInterval> | null = null;
let listenersAttached = false;

export function getStoredTokens(): StoredTokens | null {
  try {
    const raw = localStorage.getItem(TOKEN_STORAGE_KEY);
    if (!raw) return null;
    return JSON.parse(raw) as StoredTokens;
  } catch {
    localStorage.removeItem(TOKEN_STORAGE_KEY);
    return null;
  }
}

export function storeTokens(tokens: StoredTokens): void {
  localStorage.setItem(TOKEN_STORAGE_KEY, JSON.stringify(tokens));
  emit(tokens);
}

export function clearStoredTokens(): void {
  localStorage.removeItem(TOKEN_STORAGE_KEY);
  emit(null);
}

export function subscribe(listener: AuthListener): () => void {
  listeners.add(listener);
  return () => {
    listeners.delete(listener);
  };
}

function emit(tokens: StoredTokens | null): void {
  for (const listener of listeners) {
    try {
      listener(tokens);
    } catch (err) {
      console.error('Auth listener threw:', err);
    }
  }
}

export function recordActivity(): void {
  lastActivityAt = Date.now();
}

export function isUserActive(): boolean {
  return Date.now() - lastActivityAt < INACTIVITY_THRESHOLD_MS;
}

export function isTokenExpired(expiresAt: string): boolean {
  return new Date(expiresAt).getTime() <= Date.now();
}

export function isTokenNearExpiry(expiresAt: string): boolean {
  const remainingMs = new Date(expiresAt).getTime() - Date.now();
  return remainingMs <= REFRESH_BUFFER_MS;
}

async function performRefresh(refreshToken: string): Promise<StoredTokens | null> {
  try {
    const newTokens = await authService.refreshTokens(refreshToken);
    const stored: StoredTokens = {
      accessToken: newTokens.accessToken,
      refreshToken: newTokens.refreshToken,
      expiresAt: newTokens.expiresAt,
    };
    storeTokens(stored);
    return stored;
  } catch (err) {
    console.warn('Token refresh failed, clearing session:', err);
    clearStoredTokens();
    return null;
  }
}

/**
 * Refresh the tokens using the stored refresh token. Concurrent callers share the same
 * in-flight promise so we never issue parallel refresh requests (which would invalidate
 * each other due to refresh-token rotation on the backend).
 */
export function refreshTokens(): Promise<StoredTokens | null> {
  if (inFlightRefresh) return inFlightRefresh;

  const tokens = getStoredTokens();
  if (!tokens) return Promise.resolve(null);

  inFlightRefresh = performRefresh(tokens.refreshToken).finally(() => {
    inFlightRefresh = null;
  });
  return inFlightRefresh;
}

/**
 * Return a valid access token, refreshing first if the current one is expired or near expiry.
 * Returns null when there are no tokens or the refresh failed.
 */
export async function getValidAccessToken(): Promise<string | null> {
  const tokens = getStoredTokens();
  if (!tokens) return null;

  if (isTokenExpired(tokens.expiresAt) || isTokenNearExpiry(tokens.expiresAt)) {
    const refreshed = await refreshTokens();
    return refreshed?.accessToken ?? null;
  }

  return tokens.accessToken;
}

function handleStorageEvent(event: StorageEvent): void {
  if (event.key !== TOKEN_STORAGE_KEY) return;
  emit(getStoredTokens());
}

function handleVisibilityChange(): void {
  if (document.visibilityState !== 'visible') return;
  if (!isUserActive()) return;
  const tokens = getStoredTokens();
  if (!tokens) return;
  if (isTokenExpired(tokens.expiresAt) || isTokenNearExpiry(tokens.expiresAt)) {
    void refreshTokens();
  }
}

function runPeriodicCheck(): void {
  const tokens = getStoredTokens();
  if (!tokens) return;
  if (!isUserActive()) return;
  if (isTokenExpired(tokens.expiresAt) || isTokenNearExpiry(tokens.expiresAt)) {
    void refreshTokens();
  }
}

/**
 * Wire up activity tracking, visibility handling, cross-tab sync and the periodic
 * refresh check. Safe to call multiple times — only attaches listeners once.
 */
export function startTokenManager(): void {
  if (listenersAttached || typeof window === 'undefined') return;
  listenersAttached = true;

  const activityEvents: Array<keyof WindowEventMap> = [
    'mousedown',
    'keydown',
    'touchstart',
    'scroll',
  ];
  for (const event of activityEvents) {
    window.addEventListener(event, recordActivity, { passive: true, capture: true });
  }

  window.addEventListener('storage', handleStorageEvent);
  document.addEventListener('visibilitychange', handleVisibilityChange);

  periodicCheckHandle = setInterval(runPeriodicCheck, REFRESH_CHECK_INTERVAL_MS);
}

/**
 * Tear down listeners and timers. Intended for tests / hot-reload cleanup.
 */
export function stopTokenManager(): void {
  if (!listenersAttached) return;
  listenersAttached = false;

  const activityEvents: Array<keyof WindowEventMap> = [
    'mousedown',
    'keydown',
    'touchstart',
    'scroll',
  ];
  for (const event of activityEvents) {
    window.removeEventListener(event, recordActivity, { capture: true });
  }

  window.removeEventListener('storage', handleStorageEvent);
  document.removeEventListener('visibilitychange', handleVisibilityChange);

  if (periodicCheckHandle !== null) {
    clearInterval(periodicCheckHandle);
    periodicCheckHandle = null;
  }
}
