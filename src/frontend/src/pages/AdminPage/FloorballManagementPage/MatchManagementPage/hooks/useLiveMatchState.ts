import { useState, useEffect } from 'react';
import type { FloorballMatchDto } from '../../../../../types/floorball/floorballTypes';

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
    type: 'goal' | 'penalty';
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
    penaltyType?: string;
    penaltyMinutes?: number;
  }>;
}

export const useLiveMatchState = () => {
  const [liveMatches, setLiveMatches] = useState<Set<string>>(new Set());
  const [liveMatchStates, setLiveMatchStates] = useState<Map<string, LiveMatchState>>(new Map());

  // Background timer for running live match clocks
  useEffect(() => {
    const interval = window.setInterval(() => {
      setLiveMatchStates(prev => {
        const newMap = new Map(prev);
        let hasChanges = false;
        
        for (const [matchId, state] of newMap.entries()) {
          if (state.clock.isRunning) {
            const newSeconds = state.clock.seconds + 1;
            if (newSeconds >= 60) {
              newMap.set(matchId, {
                ...state,
                clock: {
                  ...state.clock,
                  minutes: state.clock.minutes + 1,
                  seconds: 0
                }
              });
            } else {
              newMap.set(matchId, {
                ...state,
                clock: {
                  ...state.clock,
                  seconds: newSeconds
                }
              });
            }
            hasChanges = true;
          }
        }
        
        return hasChanges ? newMap : prev;
      });
    }, 1000);

    return () => window.clearInterval(interval);
  }, []);

  const initializeLiveMatchState = (match: FloorballMatchDto): LiveMatchState => {
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

  const updateLiveMatchState = (matchId: string, updates: Partial<LiveMatchState>) => {
    setLiveMatchStates(prev => {
      const newMap = new Map(prev);
      const currentState = newMap.get(matchId);
      if (currentState) {
        newMap.set(matchId, { ...currentState, ...updates });
      }
      return newMap;
    });
  };

  const initializeLiveMatch = (match: FloorballMatchDto) => {
    setLiveMatches(prev => new Set([...prev, match.id]));
    
    setLiveMatchStates(prev => {
      const newMap = new Map(prev);
      newMap.set(match.id, initializeLiveMatchState(match));
      return newMap;
    });
  };

  const cancelLiveMatch = (matchId: string) => {
    // Remove from live matches
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
  };

  const getLiveMatchState = (matchId: string): LiveMatchState | undefined => {
    return liveMatchStates.get(matchId);
  };

  const isMatchLive = (matchId: string): boolean => {
    return liveMatches.has(matchId);
  };

  return {
    liveMatches,
    initializeLiveMatch,
    updateLiveMatchState,
    cancelLiveMatch,
    getLiveMatchState,
    isMatchLive
  };
}; 