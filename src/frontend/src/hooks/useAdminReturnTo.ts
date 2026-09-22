import { useCallback, useMemo } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import {
  isAdminListRoot,
  resolveAdminReturnTo,
} from '../utils/adminReturnTo';

interface AdminLocationState {
  from?: string;
}

export function useAdminReturnTo(): {
  returnTo: string;
  showBack: boolean;
  goBack: () => void;
} {
  const location = useLocation();
  const navigate = useNavigate();
  const state = location.state as AdminLocationState | null;
  const queryReturnTo = new URLSearchParams(location.search).get('returnTo');

  const returnTo = useMemo(
    () =>
      resolveAdminReturnTo(
        location.pathname,
        location.search,
        state?.from,
        queryReturnTo,
      ),
    [location.pathname, location.search, state?.from, queryReturnTo],
  );

  const showBack = !isAdminListRoot(location.pathname);

  const goBack = useCallback(() => {
    navigate(returnTo);
  }, [navigate, returnTo]);

  return { returnTo, showBack, goBack };
}
