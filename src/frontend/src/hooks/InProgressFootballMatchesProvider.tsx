import { useMemo } from 'react';
import type { ReactNode } from 'react';
import {
  InProgressFootballMatchesContext,
  useInProgressFootballMatchesController,
  type InProgressFootballMatchesState,
} from './useInProgressFootballMatches';

interface InProgressFootballMatchesProviderProps {
  children: ReactNode;
}

export const InProgressFootballMatchesProvider = ({ children }: InProgressFootballMatchesProviderProps) => {
  const state: InProgressFootballMatchesState = useInProgressFootballMatchesController(true);

  const value: InProgressFootballMatchesState = useMemo(() => state, [state]);

  return (
    <InProgressFootballMatchesContext.Provider value={value}>
      {children}
    </InProgressFootballMatchesContext.Provider>
  );
};
