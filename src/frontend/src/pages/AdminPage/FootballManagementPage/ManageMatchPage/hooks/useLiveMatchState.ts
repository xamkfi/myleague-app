import { useState, useCallback } from 'react';
import type { FootballMatchDto } from '../../../../../types/football/footballTypes';

export interface LiveMatchState {
  clock: {
    period: number;
    minutes: number;
    seconds: number;
    isRunning: boolean;
  };
  currentScore: {
    home: number;
    away: number;
  };
  events: Array<{
    id: string;
    type: 'goal' | 'card' | 'substitution';
    teamId: string;
    teamName: string;
    playerId?: string;
    playerName?: string;
    assisterId?: string;
    assisterName?: string;
    periodNumber: number;
    timeInSeconds: number;
    timestamp: Date;
    description?: string;
  }>;
}

export const useLiveMatchState = () => {
  const [liveMatches, setLiveMatches] = useState<Set<string>>(new Set());
  const [liveMatchStates, setLiveMatchStates] = useState<Map<string, LiveMatchState>>(new Map());

  const initializeLiveMatchState = (match: FootballMatchDto): LiveMatchState => {
    return {
      clock: {
        period: 1,
        minutes: 0,
        seconds: 0,
        isRunning: false
      },
      currentScore: {
        home: match.homeScore,
        away: match.awayScore
      },
      events: []
    };
  };

  const updateLiveMatchState = useCallback((matchId: string, updates: Partial<LiveMatchState>) => {
    setLiveMatchStates(prev => {
      const newMap = new Map(prev);
      const currentState = newMap.get(matchId);
      if (currentState) {
        newMap.set(matchId, { ...currentState, ...updates });
      }
      return newMap;
    });
  }, []);

  const initializeLiveMatch = useCallback((match: FootballMatchDto) => {
    setLiveMatches(prev => new Set([...prev, match.id]));
    
    setLiveMatchStates(prev => {
      const newMap = new Map(prev);
      newMap.set(match.id, initializeLiveMatchState(match));
      return newMap;
    });
  }, []);

  const cancelLiveMatch = useCallback((matchId: string) => {
    setLiveMatches(prev => {
      const newSet = new Set(prev);
      newSet.delete(matchId);
      return newSet;
    });
    
    // Remove persistent state for this match
    setLiveMatchStates(prev => {
      const newMap = new Map(prev);
      newMap.delete(matchId);
      return newMap;
    });
  }, []);

  const getLiveMatchState = useCallback((matchId: string): LiveMatchState | undefined => {
    return liveMatchStates.get(matchId);
  }, [liveMatchStates]);

  const isMatchLive = useCallback((matchId: string): boolean => {
    return liveMatches.has(matchId);
  }, [liveMatches]);

  return {
    liveMatches,
    initializeLiveMatch,
    updateLiveMatchState,
    cancelLiveMatch,
    getLiveMatchState,
    isMatchLive
  };
}; 