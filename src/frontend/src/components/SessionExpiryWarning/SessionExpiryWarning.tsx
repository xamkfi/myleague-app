import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import {
  getRemainingSessionMs,
  getStoredTokens,
  isSessionNearingExpiry,
  recordActivity,
  refreshTokens,
  subscribe,
  type StoredTokens,
} from '../../api/utils/tokenManager';
import Button from '../Button/Button';
import './SessionExpiryWarning.scss';

const TICK_MS = 15_000;

type SessionExpiryWarningProps = {
  isAuthenticated: boolean;
};

function SessionExpiryWarning({ isAuthenticated }: SessionExpiryWarningProps) {
  const { t } = useTranslation();
  const [tokens, setTokens] = useState<StoredTokens | null>(getStoredTokens);
  const [remainingMs, setRemainingMs] = useState(0);
  const [dismissedExpiresAt, setDismissedExpiresAt] = useState<string | null>(null);
  const [isRefreshing, setIsRefreshing] = useState(false);
  const [refreshFailed, setRefreshFailed] = useState(false);

  useEffect(() => {
    return subscribe((next) => {
      setTokens(next);
      if (!next || next.expiresAt !== dismissedExpiresAt) {
        setRefreshFailed(false);
      }
    });
  }, [dismissedExpiresAt]);

  useEffect(() => {
    const tick = (): void => {
      const current = getStoredTokens();
      setTokens(current);
      setRemainingMs(current ? getRemainingSessionMs(current) : 0);
    };

    tick();
    const handle = window.setInterval(tick, TICK_MS);
    return () => window.clearInterval(handle);
  }, []);

  if (!isAuthenticated || !tokens) {
    return null;
  }

  if (dismissedExpiresAt === tokens.expiresAt) {
    return null;
  }

  if (!isSessionNearingExpiry(tokens)) {
    return null;
  }

  const remainingMinutes = Math.max(1, Math.ceil(remainingMs / 60_000));

  const handleStayLoggedIn = async (): Promise<void> => {
    setIsRefreshing(true);
    setRefreshFailed(false);
    recordActivity();
    const refreshed = await refreshTokens();
    setIsRefreshing(false);
    if (!refreshed) {
      setRefreshFailed(true);
    }
  };

  return (
    <div className="session-expiry-warning" role="status">
      <div className="session-expiry-warning__content">
        <p>
          {t('sessionExpiry.message', {
            minutes: remainingMinutes,
          })}
        </p>
        {refreshFailed && (
          <p className="session-expiry-warning__error">
            {t('sessionExpiry.refreshFailed', 'Session could not be extended. Please log in again.')}
          </p>
        )}
      </div>
      <div className="session-expiry-warning__actions">
        <Button
          size="sm"
          isLoading={isRefreshing}
          disabled={isRefreshing}
          onClick={() => {
            void handleStayLoggedIn();
          }}
        >
          {t('sessionExpiry.stayLoggedIn', 'Stay logged in')}
        </Button>
        <button
          type="button"
          className="session-expiry-warning__dismiss"
          aria-label={t('sessionExpiry.dismiss', 'Dismiss')}
          onClick={() => setDismissedExpiresAt(tokens.expiresAt)}
        >
          ×
        </button>
      </div>
    </div>
  );
}

export default SessionExpiryWarning;
