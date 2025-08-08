import { useState, useEffect, useRef } from 'react';
import { timerService } from '../../../../../../../api/common/timerService';
import type { LocalClock, StateUpdate } from '../components/types';

interface UseLocalTimerProps {
  isOpen: boolean;
  matchId: string;
  onStateUpdate?: (updates: StateUpdate) => void;
}

export const useLocalTimer = ({ isOpen, matchId, onStateUpdate }: UseLocalTimerProps) => {
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
  const [getToggleFromTimer, setGetToggleFromTimer] = useState<(() => Promise<void>) | null>(null);
  
  // Timer interval ref for cleanup
  const timerIntervalRef = useRef<number | null>(null);

  // LOCAL TIMER MANAGEMENT - Only runs when modal is open
  useEffect(() => {
    if (!isOpen) {
      // Clean up timer when modal closes
      if (timerIntervalRef.current) {
        clearInterval(timerIntervalRef.current);
        timerIntervalRef.current = null;
      }
      return;
    }

    // Only start timer if modal is open and clock is running
    if (isOpen && localClock.isRunning) {
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
        clearInterval(timerIntervalRef.current);
        timerIntervalRef.current = null;
      }
    };
  }, [isOpen, localClock.isRunning]);

  // Load and restore period state when modal opens
  useEffect(() => {
    if (!isOpen || !matchId) return;

    const loadTimerState = async () => {
      try {
        const timerStatus = await timerService.getTimerStatus(matchId);
        
        if (timerStatus.exists && timerStatus.periodNumber) {
          // Restore the clock to the current period
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
          // Initialize with default state if no timer exists or no period set
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
        // Fallback to period 1 if loading fails
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
    getToggleFromTimer,
    setGetToggleFromTimer,
    
    // Utility functions
    formatTime,
    formatEventTime
  };
}; 