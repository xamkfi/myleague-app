import { authService } from '../auth/authService';

const TOKEN_STORAGE_KEY = 'myleague_auth_tokens';

const MAX_REFRESH_BUFFER_MS = 3 * 60_000;
const MIN_REFRESH_BUFFER_MS = 15_000;
const MIN_SESSION_WARNING_MS = 30_000;

const REFRESH_CHECK_INTERVAL_MS = 30_000;
const INACTIVITY_THRESHOLD_MS = 30 * 60_000;

export const DEFAULT_SESSION_EXPIRY_WARNING_MINUTES = 5;

export interface StoredTokens {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  issuedAt?: string;
  sessionExpiryWarningMinutes?: number;
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

function normalizeTokens(tokens: StoredTokens): StoredTokens {
  return {
    ...tokens,
    issuedAt: tokens.issuedAt ?? new Date().toISOString(),
    sessionExpiryWarningMinutes:
      tokens.sessionExpiryWarningMinutes ?? DEFAULT_SESSION_EXPIRY_WARNING_MINUTES,
  };
}

export function storeTokens(tokens: StoredTokens): void {
  const normalized = normalizeTokens(tokens);
  localStorage.setItem(TOKEN_STORAGE_KEY, JSON.stringify(normalized));
  emit(normalized);
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

export function getTokenLifetimeMs(tokens: StoredTokens): number {
  const expiresAtMs = new Date(tokens.expiresAt).getTime();
  if (tokens.issuedAt) {
    const issuedAtMs = new Date(tokens.issuedAt).getTime();
    if (issuedAtMs < expiresAtMs) {
      return expiresAtMs - issuedAtMs;
    }
  }

  return Math.max(expiresAtMs - Date.now(), MIN_REFRESH_BUFFER_MS);
}

export function getRefreshBufferMs(tokens: StoredTokens): number {
  const lifetimeMs = getTokenLifetimeMs(tokens);
  return Math.min(MAX_REFRESH_BUFFER_MS, Math.max(MIN_REFRESH_BUFFER_MS, lifetimeMs * 0.25));
}

export function getSessionWarningLeadMs(tokens: StoredTokens): number {
  const configuredMs =
    (tokens.sessionExpiryWarningMinutes ?? DEFAULT_SESSION_EXPIRY_WARNING_MINUTES) * 60_000;
  const expiresAtMs = new Date(tokens.expiresAt).getTime();
  const issuedAtMs = tokens.issuedAt ? new Date(tokens.issuedAt).getTime() : Number.NaN;
  const lifetimeMs =
    Number.isFinite(issuedAtMs) && issuedAtMs < expiresAtMs
      ? expiresAtMs - issuedAtMs
      : configuredMs * 2;
  return Math.max(MIN_SESSION_WARNING_MS, Math.min(configuredMs, lifetimeMs * 0.5));
}

export function isTokenNearExpiry(tokens: StoredTokens): boolean {
  const remainingMs = new Date(tokens.expiresAt).getTime() - Date.now();
  return remainingMs <= getRefreshBufferMs(tokens);
}

export function isSessionNearingExpiry(tokens: StoredTokens): boolean {
  const remainingMs = new Date(tokens.expiresAt).getTime() - Date.now();
  return remainingMs > 0 && remainingMs <= getSessionWarningLeadMs(tokens);
}

export function getRemainingSessionMs(tokens: StoredTokens): number {
  return Math.max(0, new Date(tokens.expiresAt).getTime() - Date.now());
}

async function performRefresh(refreshToken: string): Promise<StoredTokens | null> {
  try {
    const newTokens = await authService.refreshTokens(refreshToken);
    const stored: StoredTokens = {
      accessToken: newTokens.accessToken,
      refreshToken: newTokens.refreshToken,
      expiresAt: newTokens.expiresAt,
      issuedAt: new Date().toISOString(),
      sessionExpiryWarningMinutes:
        newTokens.sessionExpiryWarningMinutes ?? DEFAULT_SESSION_EXPIRY_WARNING_MINUTES,
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

  if (isTokenExpired(tokens.expiresAt) || isTokenNearExpiry(tokens)) {
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
  if (isTokenExpired(tokens.expiresAt) || isTokenNearExpiry(tokens)) {
    void refreshTokens();
  }
}

function runPeriodicCheck(): void {
  const tokens = getStoredTokens();
  if (!tokens) return;
  if (!isUserActive()) return;
  if (isTokenExpired(tokens.expiresAt) || isTokenNearExpiry(tokens)) {
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
