import { useEffect } from 'react';

/** Runs `callback` on an interval while `enabled` is true. */
export function useIntervalWhen(enabled: boolean, callback: () => void, intervalMs: number): void {
  useEffect(() => {
    if (!enabled) {
      return;
    }
    const timer = window.setInterval(callback, intervalMs);
    return () => window.clearInterval(timer);
  }, [enabled, callback, intervalMs]);
}
