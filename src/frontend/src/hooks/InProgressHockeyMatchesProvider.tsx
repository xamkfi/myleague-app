import { useMemo } from 'react';
import type { ReactNode } from 'react';
import {
  InProgressHockeyMatchesContext,
  useHockeyInProgressMatchesController,
  type HockeyInProgressState,
} from './useHockeyInProgressMatches';

interface InProgressHockeyMatchesProviderProps {
  children: ReactNode;
}

/**
 * Owns the single polling subscription for live hockey matches. Mounted in
 * AdminPageTemplate so the navbar, seasons grid, and tournaments grid share
 * one fetch.
 */
export const InProgressHockeyMatchesProvider = ({ children }: InProgressHockeyMatchesProviderProps) => {
  const state: HockeyInProgressState = useHockeyInProgressMatchesController(true);
  const value: HockeyInProgressState = useMemo(() => state, [state]);

  return (
    <InProgressHockeyMatchesContext.Provider value={value}>
      {children}
    </InProgressHockeyMatchesContext.Provider>
  );
};
