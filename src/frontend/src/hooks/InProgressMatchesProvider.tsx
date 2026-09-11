import { useMemo } from 'react';
import type { ReactNode } from 'react';
import {
  InProgressMatchesContext,
  useInProgressMatchesController,
  type InProgressMatchesState,
} from './useInProgressMatches';

interface InProgressMatchesProviderProps {
  children: ReactNode;
}

/**
 * Provider that owns the single SignalR + polling subscription for in-progress
 * floorball matches. Mounted high in the admin layout so the navbar, seasons
 * grid, and tournaments grid can all render off the same fetched state.
 *
 * Lives in its own file (separate from the hook) to satisfy
 * `react-refresh/only-export-components`: this file exports a component, the
 * sibling `useInProgressMatches.tsx` exports only hooks/context/types.
 */
export const InProgressMatchesProvider = ({ children }: InProgressMatchesProviderProps) => {
  const state: InProgressMatchesState = useInProgressMatchesController(true);

  const value: InProgressMatchesState = useMemo(() => state, [state]);

  return (
    <InProgressMatchesContext.Provider value={value}>
      {children}
    </InProgressMatchesContext.Provider>
  );
};
