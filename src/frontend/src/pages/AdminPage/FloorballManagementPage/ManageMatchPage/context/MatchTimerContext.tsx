import { createContext, useContext, useState, useCallback, useRef, type ReactNode } from 'react';
import type { TimerUpdate } from '../../../../../api/common/timerService';

interface TimerCallbacks {
  getCurrentTime: (() => string) | null;
  getCurrentElapsedSeconds: (() => number) | null;
  toggle: (() => Promise<void>) | null;
  start: (() => Promise<void>) | null;
  stop: (() => void) | null;
  reset: (() => void) | null;
}

interface MatchTimerContextValue {
  // Current period and time state
  currentPeriod: number;
  setCurrentPeriod: (period: number) => void;
  elapsedTimeSeconds: number;
  setElapsedTimeSeconds: (seconds: number) => void;
  isRunning: boolean;
  setIsRunning: (running: boolean) => void;
  
  // Timer callbacks (provided by MatchTimer component)
  callbacks: TimerCallbacks;
  registerCallback: <K extends keyof TimerCallbacks>(key: K, fn: TimerCallbacks[K]) => void;
  
  // Utility functions
  formatTime: (minutes: number, seconds: number) => string;
  formatEventTime: (timeInSeconds: number) => string;
  
  // Handle timer updates from SignalR
  handleTimerUpdate: (update: TimerUpdate) => void;
}

const MatchTimerContext = createContext<MatchTimerContextValue | null>(null);

export const useMatchTimerContext = () => {
  const context = useContext(MatchTimerContext);
  if (!context) {
    throw new Error('useMatchTimerContext must be used within MatchTimerProvider');
  }
  return context;
};

interface MatchTimerProviderProps {
  children: ReactNode;
  initialPeriod?: number;
}

export const MatchTimerProvider = ({ children, initialPeriod = 1 }: MatchTimerProviderProps) => {
  const [currentPeriod, setCurrentPeriod] = useState(initialPeriod);
  const [elapsedTimeSeconds, setElapsedTimeSeconds] = useState(0);
  const [isRunning, setIsRunning] = useState(false);
  
  // Use refs for callbacks to avoid re-renders
  const callbacksRef = useRef<TimerCallbacks>({
    getCurrentTime: null,
    getCurrentElapsedSeconds: null,
    toggle: null,
    start: null,
    stop: null,
    reset: null,
  });
  
  const [callbacks, setCallbacks] = useState<TimerCallbacks>(callbacksRef.current);
  
  const registerCallback = useCallback(<K extends keyof TimerCallbacks>(
    key: K,
    fn: TimerCallbacks[K]
  ) => {
    if (callbacksRef.current[key] !== fn) {
      callbacksRef.current[key] = fn;
      setCallbacks({ ...callbacksRef.current });
    }
  }, []);
  
  const formatTime = useCallback((minutes: number, seconds: number) => {
    return `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
  }, []);
  
  const formatEventTime = useCallback((timeInSeconds: number) => {
    if (timeInSeconds === undefined || timeInSeconds === null || isNaN(timeInSeconds)) {
      return '00:00';
    }
    const mins = Math.floor(timeInSeconds / 60);
    const secs = timeInSeconds % 60;
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  }, []);
  
  const handleTimerUpdate = useCallback((update: TimerUpdate) => {
    if (update.ElapsedTime) {
      const timeParts = update.ElapsedTime.split(':');
      let totalSeconds = 0;
      if (timeParts.length === 3) {
        const [h, m, s] = timeParts.map((part: string) => parseInt(part, 10) || 0);
        totalSeconds = h * 3600 + m * 60 + s;
      } else if (timeParts.length === 2) {
        const [m, s] = timeParts.map((part: string) => parseInt(part, 10) || 0);
        totalSeconds = m * 60 + s;
      }
      setElapsedTimeSeconds(totalSeconds);
    }
    if (update.PeriodNumber !== undefined) {
      setCurrentPeriod(update.PeriodNumber);
    }
    setIsRunning(update.IsRunning);
  }, []);
  
  const value: MatchTimerContextValue = {
    currentPeriod,
    setCurrentPeriod,
    elapsedTimeSeconds,
    setElapsedTimeSeconds,
    isRunning,
    setIsRunning,
    callbacks,
    registerCallback,
    formatTime,
    formatEventTime,
    handleTimerUpdate,
  };
  
  return (
    <MatchTimerContext.Provider value={value}>
      {children}
    </MatchTimerContext.Provider>
  );
};

export default MatchTimerContext;

