import { createContext, useContext, useState, useEffect, useCallback, useRef, type ReactNode } from 'react';
import { authService } from '../api/auth/authService';
import type { AuthTokenResponse, AuthUser } from '../types/auth/authTypes';

const TOKEN_STORAGE_KEY = 'myleague_auth_tokens';
const REFRESH_BUFFER_MS = 60_000; // Refresh 1 minute before expiry

interface StoredTokens {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
}

interface AuthContextValue {
  isAuthenticated: boolean;
  isLoading: boolean;
  user: AuthUser | null;
  accessToken: string | null;
  login: (tokens: AuthTokenResponse) => Promise<void>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

function getStoredTokens(): StoredTokens | null {
  try {
    const raw = localStorage.getItem(TOKEN_STORAGE_KEY);
    if (!raw) return null;
    return JSON.parse(raw) as StoredTokens;
  } catch {
    localStorage.removeItem(TOKEN_STORAGE_KEY);
    return null;
  }
}

function storeTokens(tokens: StoredTokens): void {
  localStorage.setItem(TOKEN_STORAGE_KEY, JSON.stringify(tokens));
}

function clearStoredTokens(): void {
  localStorage.removeItem(TOKEN_STORAGE_KEY);
}

function isTokenExpired(expiresAt: string): boolean {
  return new Date(expiresAt).getTime() <= Date.now();
}

function getTimeUntilExpiry(expiresAt: string): number {
  return new Date(expiresAt).getTime() - Date.now();
}

interface AuthProviderProps {
  children: ReactNode;
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [accessToken, setAccessToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const refreshTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  const clearAuth = useCallback(() => {
    setUser(null);
    setAccessToken(null);
    clearStoredTokens();
    if (refreshTimerRef.current) {
      clearTimeout(refreshTimerRef.current);
      refreshTimerRef.current = null;
    }
  }, []);

  const scheduleRefresh = useCallback((tokens: StoredTokens) => {
    if (refreshTimerRef.current) {
      clearTimeout(refreshTimerRef.current);
    }

    const timeUntilExpiry = getTimeUntilExpiry(tokens.expiresAt);
    const refreshIn = Math.max(timeUntilExpiry - REFRESH_BUFFER_MS, 0);

    refreshTimerRef.current = setTimeout(async () => {
      try {
        const newTokens = await authService.refreshTokens(tokens.refreshToken);
        const stored: StoredTokens = {
          accessToken: newTokens.accessToken,
          refreshToken: newTokens.refreshToken,
          expiresAt: newTokens.expiresAt,
        };
        storeTokens(stored);
        setAccessToken(newTokens.accessToken);
        scheduleRefresh(stored);
      } catch {
        clearAuth();
      }
    }, refreshIn);
  }, [clearAuth]);

  const login = useCallback(async (tokens: AuthTokenResponse) => {
    const stored: StoredTokens = {
      accessToken: tokens.accessToken,
      refreshToken: tokens.refreshToken,
      expiresAt: tokens.expiresAt,
    };
    storeTokens(stored);
    setAccessToken(tokens.accessToken);

    try {
      const userInfo = await authService.getMe(tokens.accessToken);
      setUser(userInfo);
    } catch {
      // Token is valid but /me failed -- clear auth
      clearAuth();
      throw new Error('Failed to get user info after login');
    }

    scheduleRefresh(stored);
  }, [clearAuth, scheduleRefresh]);

  const logout = useCallback(async () => {
    const tokens = getStoredTokens();
    if (tokens) {
      try {
        await authService.logout(tokens.refreshToken);
      } catch {
        // Ignore logout API errors -- clear local state regardless
      }
    }
    clearAuth();
  }, [clearAuth]);

  // Initialize auth state on mount
  useEffect(() => {
    const initAuth = async () => {
      const tokens = getStoredTokens();
      if (!tokens) {
        setIsLoading(false);
        return;
      }

      // If access token is expired, try to refresh
      if (isTokenExpired(tokens.expiresAt)) {
        try {
          const newTokens = await authService.refreshTokens(tokens.refreshToken);
          const stored: StoredTokens = {
            accessToken: newTokens.accessToken,
            refreshToken: newTokens.refreshToken,
            expiresAt: newTokens.expiresAt,
          };
          storeTokens(stored);
          setAccessToken(newTokens.accessToken);

          const userInfo = await authService.getMe(newTokens.accessToken);
          setUser(userInfo);
          scheduleRefresh(stored);
        } catch {
          clearAuth();
        }
      } else {
        // Access token still valid -- validate with /me
        try {
          const userInfo = await authService.getMe(tokens.accessToken);
          setUser(userInfo);
          setAccessToken(tokens.accessToken);
          scheduleRefresh(tokens);
        } catch {
          // Token invalid or /me failed -- try refresh
          try {
            const newTokens = await authService.refreshTokens(tokens.refreshToken);
            const stored: StoredTokens = {
              accessToken: newTokens.accessToken,
              refreshToken: newTokens.refreshToken,
              expiresAt: newTokens.expiresAt,
            };
            storeTokens(stored);
            setAccessToken(newTokens.accessToken);

            const userInfo = await authService.getMe(newTokens.accessToken);
            setUser(userInfo);
            scheduleRefresh(stored);
          } catch {
            clearAuth();
          }
        }
      }

      setIsLoading(false);
    };

    initAuth();

    return () => {
      if (refreshTimerRef.current) {
        clearTimeout(refreshTimerRef.current);
      }
    };
  }, [clearAuth, scheduleRefresh]);

  const isAuthenticated = user !== null && accessToken !== null;

  return (
    <AuthContext.Provider value={{ isAuthenticated, isLoading, user, accessToken, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
