import { createContext, useContext, useState, useCallback, useEffect, useMemo, type ReactNode } from 'react';
import { useMatchTimer, type UseMatchTimerReturn } from '../../../../../hooks/useMatchTimer';

/**
 * Imperative timer API plus slow-changing status. This is the single owner of the REST
 * timer hook for the floorball manage-match page; child components call actions from here
 * instead of registering callbacks upwards.
 */
export interface LiveMatchTimerApi {
  isRunning: boolean;
  /** Period number reported by the backend timer, or null before the first load / without a timer. */
  periodNumber: number | null;
  loading: boolean;
  error: string | null;
  initialLoadComplete: boolean;
  /** Live elapsed seconds; stable identity, safe to call from event handlers. */
  getCurrentElapsedSeconds: () => number;
  loadStatus: () => Promise<void>;
  start: (periodNumber?: number) => Promise<void>;
  stop: () => Promise<void>;
  toggle: () => Promise<void>;
  setTime: (seconds: number) => Promise<void>;
  adjust: (seconds: number) => Promise<void>;
  /** Rewinds to the start of the current period (the match clock is continuous). */
  reset: () => Promise<void>;
}

interface MatchTimerContextValue {
  /** Period the live desk is operating on. Derived from match + timer state by usePeriodManagement. */
  currentPeriod: number;
  setCurrentPeriod: (period: number) => void;

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

  timer: LiveMatchTimerApi;
}

/**
 * Fast-changing clock values, kept in a separate context so only the components that
 * actually render the digits re-render once per second.
 */
export interface MatchClockValue {
  displayTime: string;
  displayTimeMs: number;
  /** Whole elapsed seconds of the continuous match clock. */
  elapsedTimeSeconds: number;
  isRunning: boolean;
  initialLoadComplete: boolean;
}

const MatchTimerContext = createContext<MatchTimerContextValue | null>(null);
const MatchClockContext = createContext<MatchClockValue | null>(null);

// eslint-disable-next-line react-refresh/only-export-components
export const useMatchTimerContext = (): MatchTimerContextValue => {
  const context = useContext(MatchTimerContext);
  if (!context) {
    throw new Error('useMatchTimerContext must be used within MatchTimerProvider');
  }
  return context;
};

// eslint-disable-next-line react-refresh/only-export-components
export const useMatchClock = (): MatchClockValue => {
  const context = useContext(MatchClockContext);
  if (!context) {
    throw new Error('useMatchClock must be used within MatchTimerProvider');
  }
  return context;
};

interface MatchTimerProviderProps {
  children: ReactNode;
  matchId: string;
  initialPeriod?: number;
}

const buildPeriodStartTimesStorageKey = (matchId: string): string =>
  `manage-match-period-starts:${matchId}`;

function readPeriodStartTimes(storageKey: string): Record<number, number> {
  try {
    const raw: string | null = localStorage.getItem(storageKey);
    if (!raw) return { 1: 0 };
    const parsed: unknown = JSON.parse(raw);
    if (!parsed || typeof parsed !== 'object') return { 1: 0 };
    const sanitized: Record<number, number> = { 1: 0 };
    for (const [key, value] of Object.entries(parsed as Record<string, unknown>)) {
      const periodNum: number = Number(key);
      const seconds: number = Number(value);
      if (Number.isFinite(periodNum) && Number.isFinite(seconds) && periodNum >= 1 && seconds >= 0) {
        sanitized[periodNum] = seconds;
      }
    }
    return sanitized;
  } catch {
    return { 1: 0 };
  }
}

export const MatchTimerProvider = ({ children, matchId, initialPeriod = 1 }: MatchTimerProviderProps) => {
  const [currentPeriod, setCurrentPeriod] = useState<number>(initialPeriod);
  const hook: UseMatchTimerReturn = useMatchTimer({ matchId });

  const storageKey: string = useMemo(() => buildPeriodStartTimesStorageKey(matchId), [matchId]);
  const [periodStartTimes, setPeriodStartTimes] = useState<Record<number, number>>(() => readPeriodStartTimes(storageKey));

  useEffect(() => {
    try {
      localStorage.setItem(storageKey, JSON.stringify(periodStartTimes));
    } catch {
      /* noop – localStorage may be unavailable */
    }
  }, [storageKey, periodStartTimes]);

  const setPeriodStartTime = useCallback((period: number, seconds: number): void => {
    if (!Number.isFinite(period) || period < 1 || !Number.isFinite(seconds) || seconds < 0) return;
    setPeriodStartTimes(prev => (prev[period] === seconds ? prev : { ...prev, [period]: seconds }));
  }, []);

  const currentPeriodStartSeconds: number = periodStartTimes[currentPeriod] ?? 0;

  const { setTimer, startTimer, stopTimer, isRunning } = hook;

  const reset = useCallback(async (): Promise<void> => {
    await setTimer(currentPeriodStartSeconds);
  }, [setTimer, currentPeriodStartSeconds]);

  // Starting always tags the backend timer with the period the desk is operating on.
  const start = useCallback(async (period?: number): Promise<void> => {
    await startTimer(period ?? currentPeriod);
  }, [startTimer, currentPeriod]);

  const toggle = useCallback(async (): Promise<void> => {
    if (isRunning) {
      await stopTimer();
    } else {
      await start();
    }
  }, [isRunning, stopTimer, start]);

  const timer: LiveMatchTimerApi = useMemo(() => ({
    isRunning: hook.isRunning,
    periodNumber: hook.periodNumber,
    loading: hook.loading,
    error: hook.error,
    initialLoadComplete: hook.initialLoadComplete,
    getCurrentElapsedSeconds: hook.getCurrentElapsedSeconds,
    loadStatus: hook.loadStatus,
    start,
    stop: hook.stopTimer,
    toggle,
    setTime: hook.setTimer,
    adjust: hook.adjustTimer,
    reset,
  }), [
    hook.isRunning,
    hook.periodNumber,
    hook.loading,
    hook.error,
    hook.initialLoadComplete,
    hook.getCurrentElapsedSeconds,
    hook.loadStatus,
    hook.stopTimer,
    hook.setTimer,
    hook.adjustTimer,
    start,
    toggle,
    reset,
  ]);

  const value: MatchTimerContextValue = useMemo(() => ({
    currentPeriod,
    setCurrentPeriod,
    periodStartTimes,
    setPeriodStartTime,
    currentPeriodStartSeconds,
    timer,
  }), [currentPeriod, periodStartTimes, setPeriodStartTime, currentPeriodStartSeconds, timer]);

  const clock: MatchClockValue = useMemo(() => ({
    displayTime: hook.displayTime,
    displayTimeMs: hook.displayTimeMs,
    elapsedTimeSeconds: Math.floor(hook.displayTimeMs / 1000),
    isRunning: hook.isRunning,
    initialLoadComplete: hook.initialLoadComplete,
  }), [hook.displayTime, hook.displayTimeMs, hook.isRunning, hook.initialLoadComplete]);

  return (
    <MatchTimerContext.Provider value={value}>
      <MatchClockContext.Provider value={clock}>
        {children}
      </MatchClockContext.Provider>
    </MatchTimerContext.Provider>
  );
};

export default MatchTimerContext;
