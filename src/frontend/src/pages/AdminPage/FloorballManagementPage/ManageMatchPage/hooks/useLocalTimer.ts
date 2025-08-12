import { useState, useEffect } from 'react';
import { timerService } from '../../../../../api/common/timerService';
import type { LocalClock, StateUpdate } from '../components/types';

interface UseLocalTimerProps {
  isOpen: boolean;
  matchId: string;
  onStateUpdate?: (updates: StateUpdate) => void;
}

export const useLocalTimer = ({ isOpen, matchId, onStateUpdate }: UseLocalTimerProps) => {
  const [localClock, setLocalClock] = useState<LocalClock>({
    period: 1,
    minutes: 0,
    seconds: 0,
    isRunning: false
  });
  
  const [currentTimerElapsedTime, setCurrentTimerElapsedTime] = useState<number>(0);
  const [getCurrentTimeFromTimer, setGetCurrentTimeFromTimer] = useState<(() => string) | null>(null);
  const [getToggleFromTimer, setGetToggleFromTimer] = useState<(() => Promise<void>) | null>(null);
  
  useEffect(() => {
    if (!isOpen || !matchId) return;

    const loadTimerState = async () => {
      try {
        const timerStatus = await timerService.getTimerStatus(matchId);
        
        if (timerStatus.exists && timerStatus.periodNumber) {
          const restoredClock = {
            period: timerStatus.periodNumber,
            minutes: 0,
            seconds: 0,
            isRunning: false
          };
          
          console.log('Restoring period state to:', timerStatus.periodNumber);
          setLocalClock(restoredClock);
          
          if (onStateUpdate) {
            onStateUpdate({
              clock: restoredClock
            });
          }
        } else {
          const initialClock = {
            period: 1,
            minutes: 0,
            seconds: 0,
            isRunning: false
          };
          setLocalClock(initialClock);
          
          if (onStateUpdate) {
            onStateUpdate({
              clock: initialClock
            });
          }
        }
      } catch (error) {
        console.warn('Failed to load timer state, using default period 1:', error);
        const fallbackClock = {
          period: 1,
          minutes: 0,
          seconds: 0,
          isRunning: false
        };
        setLocalClock(fallbackClock);
        
        if (onStateUpdate) {
          onStateUpdate({
            clock: fallbackClock
          });
        }
      }
    };

    loadTimerState();
  }, [isOpen, matchId, onStateUpdate]);

  /**
   * Formats time as MM:SS
   */
  const formatTime = (minutes: number, seconds: number) => {
    return `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
  };

  /**
   * Formats event time from seconds to MM:SS
   */
  const formatEventTime = (timeInSeconds: number) => {
    if (timeInSeconds === undefined || timeInSeconds === null || isNaN(timeInSeconds)) {
      console.warn('formatEventTime received invalid timeInSeconds:', timeInSeconds);
      return '00:00';
    }
    
    const mins = Math.floor(timeInSeconds / 60);
    const secs = timeInSeconds % 60;
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  };

  return {
    localClock,
    setLocalClock,
    currentTimerElapsedTime,
    setCurrentTimerElapsedTime,
    getCurrentTimeFromTimer,
    setGetCurrentTimeFromTimer,
    getToggleFromTimer,
    setGetToggleFromTimer,
    formatTime,
    formatEventTime
  };
}; 