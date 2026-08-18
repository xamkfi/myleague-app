import { createContext, useContext, useState, useCallback, useEffect, useMemo, useRef, type ReactNode } from 'react';
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

  // Per-period elapsed-second offsets. The clock runs continuously across periods, so we
  // remember at what absolute elapsed-second mark each period started; the Reset button
  // and the "in-period elapsed" logic in the UI both derive from this map.
  periodStartTimes: Record<number, number>;
  /**
   * Records (and persists) the absolute elapsed-second value at which a period started.
   * Period 1 is implicitly anchored at 0 even when not explicitly set.
   */
  setPeriodStartTime: (period: number, seconds: number) => void;
  /** Absolute elapsed seconds at which the current period started (0 if not yet captured). */
  currentPeriodStartSeconds: number;

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

// eslint-disable-next-line react-refresh/only-export-components
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
  /**
   * Optional match identifier. When supplied, per-period start times are persisted to
   * localStorage under this key so the running clock survives page reloads / navigation
   * back into the match.
   */
  matchId?: string;
}

const buildPeriodStartTimesStorageKey = (matchId: string | undefined): string | null =>
  matchId ? `manage-match-period-starts:${matchId}` : null;

export const MatchTimerProvider = ({ children, initialPeriod = 1, matchId }: MatchTimerProviderProps) => {
  const [currentPeriod, setCurrentPeriod] = useState(initialPeriod);
  const [elapsedTimeSeconds, setElapsedTimeSeconds] = useState(0);
  const [isRunning, setIsRunning] = useState(false);

  // Period 1 is the implicit zero anchor; later periods are populated when the operator
  // actually starts them so the value reflects the real (possibly-early/late) handoff.
  const storageKey: string | null = useMemo(() => buildPeriodStartTimesStorageKey(matchId), [matchId]);
  const [periodStartTimes, setPeriodStartTimes] = useState<Record<number, number>>(() => {
    if (!storageKey) return { 1: 0 };
    try {
      const raw: string | null = localStorage.getItem(storageKey);
      if (!raw) return { 1: 0 };
      const parsed: unknown = JSON.parse(raw);
      if (parsed && typeof parsed === 'object') {
        const sanitized: Record<number, number> = { 1: 0 };
        for (const [key, value] of Object.entries(parsed as Record<string, unknown>)) {
          const periodNum: number = Number(key);
          const seconds: number = Number(value);
          if (Number.isFinite(periodNum) && Number.isFinite(seconds) && periodNum >= 1 && seconds >= 0) {
            sanitized[periodNum] = seconds;
          }
        }
        return sanitized;
      }
      return { 1: 0 };
    } catch {
      return { 1: 0 };
    }
  });
  useEffect(() => {
    if (!storageKey) return;
    try {
      localStorage.setItem(storageKey, JSON.stringify(periodStartTimes));
    } catch {
      /* noop – localStorage may be unavailable */
    }
  }, [storageKey, periodStartTimes]);

  const setPeriodStartTime = useCallback((period: number, seconds: number) => {
    if (!Number.isFinite(period) || period < 1 || !Number.isFinite(seconds) || seconds < 0) return;
    setPeriodStartTimes(prev => {
      // Avoid touching state if nothing actually changed; saves an extra render + write.
      if (prev[period] === seconds) return prev;
      return { ...prev, [period]: seconds };
    });
  }, []);

  const currentPeriodStartSeconds: number = periodStartTimes[currentPeriod] ?? 0;

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
    periodStartTimes,
    setPeriodStartTime,
    currentPeriodStartSeconds,
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

