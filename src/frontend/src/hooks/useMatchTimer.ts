import { useState, useEffect, useCallback, useRef } from 'react';
import { timerService, type TimerStatusResponse, type TimerUpdate } from '../api/common/timerService';

export interface UseMatchTimerOptions {
  matchId: string;
  autoConnect?: boolean;
  /**
   * Optional listener invoked whenever the hook receives an authoritative status from the
   * backend. The latest callback is always used, so callers do not need to memoize it.
   */
  onTimerUpdate?: (update: TimerUpdate) => void;
}

export interface UseMatchTimerReturn {
  displayTimeMs: number;
  displayTime: string;
  isRunning: boolean;
  periodNumber: number | null;
  loading: boolean;
  error: string | null;
  initialLoadComplete: boolean;
  startTimer: (periodNumber?: number) => Promise<void>;
  stopTimer: () => Promise<void>;
  toggleTimer: () => Promise<void>;
  resetTimer: () => Promise<void>;
  setTimer: (timeInSeconds: number) => Promise<void>;
  adjustTimer: (adjustmentInSeconds: number) => Promise<void>;
  createTimer: () => Promise<void>;
  loadStatus: () => Promise<void>;
  /** Live elapsed seconds, read from refs so the function identity never changes. */
  getCurrentElapsedSeconds: () => number;
}

const TICK_INTERVAL_MS: number = 100;

/**
 * Format milliseconds to display string (MM:SS or HH:MM:SS)
 */
export function formatElapsedMs(ms: number): string {
  const totalSeconds: number = Math.floor(Math.max(0, ms) / 1000);
  const hours: number = Math.floor(totalSeconds / 3600);
  const minutes: number = Math.floor((totalSeconds % 3600) / 60);
  const seconds: number = totalSeconds % 60;
  const mmss: string = `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
  return hours > 0 ? `${hours.toString().padStart(2, '0')}:${mmss}` : mmss;
}

/**
 * Parse an `hh:mm:ss` / `mm:ss` string to milliseconds. Unknown input yields 0.
 */
export function parseElapsedTime(elapsedTime: string | null | undefined): number {
  if (!elapsedTime || !elapsedTime.includes(':')) return 0;
  const parts: number[] = elapsedTime.split(':').map((p: string) => parseInt(p, 10) || 0);
  if (parts.length === 3) {
    const [hours, minutes, seconds] = parts;
    return (hours * 3600 + minutes * 60 + seconds) * 1000;
  }
  if (parts.length === 2) {
    const [minutes, seconds] = parts;
    return (minutes * 60 + seconds) * 1000;
  }
  return 0;
}

/**
 * REST-only match timer.
 *
 * - On mount the status is loaded once from the API.
 * - While running, the display is interpolated locally every 100ms from the last
 *   authoritative (base time, wall-clock) pair.
 * - Every mutation bumps a request sequence so a slow in-flight `GET /status` can never
 *   overwrite a newer state (e.g. re-start the clock after the operator pressed stop).
 */
export function useMatchTimer(options: UseMatchTimerOptions): UseMatchTimerReturn {
  const { matchId, autoConnect = true, onTimerUpdate } = options;

  const [timeMs, setTimeMs] = useState<number>(0);
  const [isRunning, setIsRunning] = useState<boolean>(false);
  const [periodNumber, setPeriodNumber] = useState<number | null>(null);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [initialLoadComplete, setInitialLoadComplete] = useState<boolean>(false);

  // Authoritative anchor: elapsed ms at `tickStartRef` wall-clock time.
  const baseTimeRef = useRef<number>(0);
  const tickStartRef = useRef<number>(Date.now());
  const isRunningRef = useRef<boolean>(false);
  // Monotonic token; responses carrying an older token are discarded.
  const requestSeqRef = useRef<number>(0);
  const onTimerUpdateRef = useRef<UseMatchTimerOptions['onTimerUpdate']>(onTimerUpdate);
  onTimerUpdateRef.current = onTimerUpdate;

  const currentElapsedMs = useCallback((): number => {
    if (!isRunningRef.current) return baseTimeRef.current;
    return baseTimeRef.current + (Date.now() - tickStartRef.current);
  }, []);

  const getCurrentElapsedSeconds = useCallback((): number => {
    return Math.floor(currentElapsedMs() / 1000);
  }, [currentElapsedMs]);

  /**
   * Applies an authoritative status from the backend to local state.
   */
  const applyStatus = useCallback((status: TimerStatusResponse, eventType: string): void => {
    const elapsedMs: number = parseElapsedTime(status.elapsedTime);
    const running: boolean = Boolean(status.exists && status.isRunning);
    const resolvedPeriod: number | null =
      typeof status.periodNumber === 'number' && Number.isFinite(status.periodNumber)
        ? status.periodNumber
        : null;

    baseTimeRef.current = elapsedMs;
    tickStartRef.current = Date.now();
    isRunningRef.current = running;
    setTimeMs(elapsedMs);
    setIsRunning(running);
    setPeriodNumber(resolvedPeriod);
    setInitialLoadComplete(true);

    onTimerUpdateRef.current?.({
      MatchId: matchId,
      ElapsedTime: status.elapsedTime ?? '00:00:00',
      ElapsedMilliseconds: elapsedMs,
      IsRunning: running,
      PeriodNumber: resolvedPeriod ?? undefined,
      LastUpdated: new Date().toISOString(),
      EventType: eventType,
    });
  }, [matchId]);

  /**
   * Freezes the local clock at its current value. Used as an optimistic update before a
   * stop request so the digits never keep ticking while the network round-trip is pending.
   */
  const freezeLocally = useCallback((): void => {
    baseTimeRef.current = currentElapsedMs();
    tickStartRef.current = Date.now();
    isRunningRef.current = false;
    setTimeMs(baseTimeRef.current);
    setIsRunning(false);
  }, [currentElapsedMs]);

  const loadStatus = useCallback(async (): Promise<void> => {
    const seq: number = ++requestSeqRef.current;
    try {
      const status: TimerStatusResponse = await timerService.getTimerStatus(matchId);
      if (seq !== requestSeqRef.current) return; // superseded by a newer request
      setError(null);
      applyStatus(status, 'TimerStatusLoaded');
    } catch (err) {
      if (seq !== requestSeqRef.current) return;
      console.error('Error loading timer status:', err);
      setInitialLoadComplete(true);
    }
  }, [matchId, applyStatus]);

  useEffect(() => {
    if (!autoConnect || !matchId) return;
    void loadStatus();
  }, [matchId, autoConnect, loadStatus]);

  useEffect(() => {
    if (!isRunning || !initialLoadComplete) return;
    const interval = setInterval(() => {
      const next: number = baseTimeRef.current + (Date.now() - tickStartRef.current);
      // The display has one-second resolution; skip renders that would not change it.
      setTimeMs(prev => (Math.floor(prev / 1000) === Math.floor(next / 1000) ? prev : next));
    }, TICK_INTERVAL_MS);
    return () => clearInterval(interval);
  }, [isRunning, initialLoadComplete]);

  /**
   * Runs a mutation with loading/error bookkeeping. `run` should return the authoritative
   * status when the endpoint provides one; otherwise the status is reloaded.
   */
  const runMutation = useCallback(async (
    fallbackMessage: string,
    eventType: string,
    run: () => Promise<TimerStatusResponse | void>,
  ): Promise<void> => {
    setLoading(true);
    setError(null);
    // Invalidate any in-flight GET so its (older) result cannot land after this mutation.
    requestSeqRef.current += 1;
    try {
      const status: TimerStatusResponse | void = await run();
      if (status) {
        requestSeqRef.current += 1;
        applyStatus(status, eventType);
      } else {
        await loadStatus();
      }
    } catch (err) {
      console.error(fallbackMessage, err);
      setError(err instanceof Error ? err.message : fallbackMessage);
      // Re-sync so the UI reflects whatever the server actually did.
      await loadStatus();
    } finally {
      setLoading(false);
    }
  }, [applyStatus, loadStatus]);

  const startTimer = useCallback(async (period?: number): Promise<void> => {
    await runMutation('Failed to start timer', 'TimerStarted', async () => {
      await timerService.startTimer(matchId, period);
    });
  }, [matchId, runMutation]);

  const stopTimer = useCallback(async (): Promise<void> => {
    freezeLocally();
    await runMutation('Failed to stop timer', 'TimerStopped', () => timerService.stopTimer(matchId));
  }, [matchId, freezeLocally, runMutation]);

  const toggleTimer = useCallback(async (): Promise<void> => {
    if (isRunningRef.current) {
      await stopTimer();
    } else {
      await startTimer(periodNumber ?? undefined);
    }
  }, [startTimer, stopTimer, periodNumber]);

  const resetTimer = useCallback(async (): Promise<void> => {
    await runMutation('Failed to reset timer', 'TimerReset', async () => {
      await timerService.resetTimer(matchId);
    });
  }, [matchId, runMutation]);

  const setTimer = useCallback(async (timeInSeconds: number): Promise<void> => {
    const target: number = Math.max(0, Math.floor(timeInSeconds));
    await runMutation('Failed to set timer', 'TimerSet', () => timerService.setTimer(matchId, target));
  }, [matchId, runMutation]);

  const adjustTimer = useCallback(async (adjustmentInSeconds: number): Promise<void> => {
    await setTimer(getCurrentElapsedSeconds() + adjustmentInSeconds);
  }, [setTimer, getCurrentElapsedSeconds]);

  const createTimer = useCallback(async (): Promise<void> => {
    try {
      setLoading(true);
      await timerService.createTimer(matchId);
    } catch (err) {
      // The timer may already exist; the start endpoint also creates it on demand.
      console.warn('Error creating timer:', err);
    } finally {
      setLoading(false);
    }
  }, [matchId]);

  return {
    displayTimeMs: timeMs,
    displayTime: formatElapsedMs(timeMs),
    isRunning,
    periodNumber,
    loading,
    error,
    initialLoadComplete,
    startTimer,
    stopTimer,
    toggleTimer,
    resetTimer,
    setTimer,
    adjustTimer,
    createTimer,
    loadStatus,
    getCurrentElapsedSeconds,
  };
}
