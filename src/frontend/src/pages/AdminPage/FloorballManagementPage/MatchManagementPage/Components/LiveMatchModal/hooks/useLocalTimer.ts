import { useState, useEffect, useRef } from 'react';
import type { LocalClock, StateUpdate } from '../types';

interface UseLocalTimerProps {
  isOpen: boolean;
  onStateUpdate?: (updates: StateUpdate) => void;
}

export const useLocalTimer = ({ isOpen, onStateUpdate }: UseLocalTimerProps) => {
  // LOCAL TIMER STATE - Only runs when modal is open
  const [localClock, setLocalClock] = useState<LocalClock>({
    period: 1,
    minutes: 0,
    seconds: 0,
    isRunning: false
  });
  
  // Timer state tracking for accurate time calculations
  const [currentTimerElapsedTime, setCurrentTimerElapsedTime] = useState<number>(0);
  const [getCurrentTimeFromTimer, setGetCurrentTimeFromTimer] = useState<(() => string) | null>(null);
  
  // Timer interval ref for cleanup
  const timerIntervalRef = useRef<number | null>(null);

  // LOCAL TIMER MANAGEMENT - Only runs when modal is open
  useEffect(() => {
    if (!isOpen) {
      // Clean up timer when modal closes
      if (timerIntervalRef.current) {
        console.log('LiveMatchModal: Cleaning up timer interval');
        clearInterval(timerIntervalRef.current);
        timerIntervalRef.current = null;
      }
      return;
    }

    // Only start timer if modal is open and clock is running
    if (isOpen && localClock.isRunning) {
      console.log('LiveMatchModal: Starting local timer interval');
      timerIntervalRef.current = setInterval(() => {
        setLocalClock(prev => {
          const newSeconds = prev.seconds + 1;
          if (newSeconds >= 60) {
            return {
              ...prev,
              minutes: prev.minutes + 1,
              seconds: 0
            };
          } else {
            return {
              ...prev,
              seconds: newSeconds
            };
          }
        });
      }, 1000);
    }

    return () => {
      if (timerIntervalRef.current) {
        console.log('LiveMatchModal: Cleaning up timer interval on unmount');
        clearInterval(timerIntervalRef.current);
        timerIntervalRef.current = null;
      }
    };
  }, [isOpen, localClock.isRunning]);

  // Initialize clock state
  useEffect(() => {
    if (isOpen && onStateUpdate && localClock.period === 1 && localClock.minutes === 0 && localClock.seconds === 0) {
      // Only initialize if we don't already have a clock state
      const initialClock = {
        period: 1,
        minutes: 0,
        seconds: 0,
        isRunning: false
      };
      setLocalClock(initialClock);
      onStateUpdate({
        clock: initialClock
      });
    }
  }, [isOpen, onStateUpdate, localClock.period, localClock.minutes, localClock.seconds]);

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
    // Handle invalid inputs
    if (timeInSeconds === undefined || timeInSeconds === null || isNaN(timeInSeconds)) {
      console.warn('formatEventTime received invalid timeInSeconds:', timeInSeconds);
      return '00:00';
    }
    
    const mins = Math.floor(timeInSeconds / 60);
    const secs = timeInSeconds % 60;
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  };

  return {
    // Timer state
    localClock,
    setLocalClock,
    currentTimerElapsedTime,
    setCurrentTimerElapsedTime,
    getCurrentTimeFromTimer,
    setGetCurrentTimeFromTimer,
    
    // Utility functions
    formatTime,
    formatEventTime
  };
}; 