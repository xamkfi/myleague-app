import {
  createContext,
  useContext,
  useState,
  useEffect,
  useCallback,
  type ReactNode,
} from 'react';
import { authService } from '../api/auth/authService';
import {
  clearStoredTokens,
  getStoredTokens,
  isTokenExpired,
  recordActivity,
  refreshTokens as refreshStoredTokens,
  startTokenManager,
  storeTokens,
  subscribe as subscribeToTokens,
  type StoredTokens,
} from '../api/utils/tokenManager';
import type { AuthTokenResponse, AuthUser } from '../types/auth/authTypes';

interface AuthContextValue {
  isAuthenticated: boolean;
  isLoading: boolean;
  user: AuthUser | null;
  accessToken: string | null;
  login: (tokens: AuthTokenResponse) => Promise<void>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

interface AuthProviderProps {
  children: ReactNode;
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [accessToken, setAccessToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const clearAuth = useCallback(() => {
    setUser(null);
    setAccessToken(null);
    clearStoredTokens();
  }, []);

  const login = useCallback(
    async (tokens: AuthTokenResponse) => {
      const stored: StoredTokens = {
        accessToken: tokens.accessToken,
        refreshToken: tokens.refreshToken,
        expiresAt: tokens.expiresAt,
      };
      storeTokens(stored);
      setAccessToken(tokens.accessToken);
      recordActivity();

      try {
        const userInfo = await authService.getMe(tokens.accessToken);
        setUser(userInfo);
      } catch {
        clearAuth();
        throw new Error('Failed to get user info after login');
      }
    },
    [clearAuth],
  );

  const logout = useCallback(async () => {
    const tokens = getStoredTokens();
    if (tokens) {
      try {
        await authService.logout(tokens.refreshToken);
      } catch {
        // Ignore logout API errors -- clear local state regardless.
      }
    }
    clearAuth();
  }, [clearAuth]);

  // Start the global token-manager loop (activity tracking, periodic refresh, visibility
  // handling) and subscribe to token changes so this context stays in sync with refreshes
  // performed elsewhere (background timer, authFetch retry, other tabs).
  useEffect(() => {
    startTokenManager();

    const unsubscribe = subscribeToTokens((tokens) => {
      if (tokens) {
        setAccessToken(tokens.accessToken);
      } else {
        setAccessToken(null);
        setUser(null);
      }
    });

    return unsubscribe;
  }, []);

  useEffect(() => {
    let cancelled = false;

    const initAuth = async () => {
      const tokens = getStoredTokens();
      if (!tokens) {
        if (!cancelled) setIsLoading(false);
        return;
      }

      const tryGetMeWith = async (token: string): Promise<boolean> => {
        try {
          const userInfo = await authService.getMe(token);
          if (cancelled) return true;
          setUser(userInfo);
          setAccessToken(token);
          return true;
        } catch {
          return false;
        }
      };

      let success = false;

      if (isTokenExpired(tokens.expiresAt)) {
        const refreshed = await refreshStoredTokens();
        if (refreshed) {
          success = await tryGetMeWith(refreshed.accessToken);
        }
      } else {
        success = await tryGetMeWith(tokens.accessToken);
        if (!success) {
          const refreshed = await refreshStoredTokens();
          if (refreshed) {
            success = await tryGetMeWith(refreshed.accessToken);
          }
        }
      }

      if (!cancelled && !success) {
        clearAuth();
      }

      if (!cancelled) setIsLoading(false);
    };

    void initAuth();

    return () => {
      cancelled = true;
    };
  }, [clearAuth]);

  const isAuthenticated = user !== null && accessToken !== null;

  return (
    <AuthContext.Provider
      value={{ isAuthenticated, isLoading, user, accessToken, login, logout }}
    >
      {children}
    </AuthContext.Provider>
  );
}

// eslint-disable-next-line react-refresh/only-export-components
export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
