import { useEffect, useCallback, useMemo, useRef, type ReactElement } from 'react';
import { useTranslation } from 'react-i18next';
import { formatElapsedMs, useMatchTimer } from '../../hooks/useMatchTimer';
import type { TimerUpdate } from '../../api/common/timerService';
import { MatchTimerView, type PeriodControlDescriptor } from './MatchTimerView';

interface MatchTimerProps {
  matchId: string;
  periodNumber?: number;
  onTimerUpdate?: (update: TimerUpdate) => void;
  onGetCurrentTime?: (getTime: () => string) => void;
  onGetCurrentElapsedSeconds?: (getSeconds: () => number) => void;
  onGetToggleFunction?: (toggleFunction: () => Promise<void>) => void;
  onGetResetFunction?: (resetFunction: () => Promise<void>) => void;
  onGetStartFunction?: (startFunction: () => Promise<void>) => void;
  onGetStopFunction?: (stopFunction: () => Promise<void>) => void;
  controlsEnabled?: boolean;
  isActive?: boolean;
  keybindsEnabled?: boolean;
  /**
   * Absolute elapsed-second mark at which the current period started. The Reset button
   * rewinds to this value instead of 0, because the match clock is continuous across
   * periods (e.g. period 2 starts at 20:00, so reset goes back to 20:00 there).
   */
  periodStartSeconds?: number;
  /**
   * When `period`, the digits show time inside the current period (0–20) and the
   * time editor writes period-relative values.
   */
  clockDisplayMode?: 'absolute' | 'period';
  // Period control props
  onPeriodControlClick?: () => void;
  canEndPeriod?: () => boolean;
  getPeriodControlButtonText?: () => string;
  /**
   * Optional resolver for the control's semantics. When omitted the action is derived
   * from `canEndPeriod` (`end` vs `start`). Returning `null` hides the button.
   */
  getPeriodControlAction?: () => 'end' | 'start' | 'finish' | null;
  periodLoading?: Record<number, boolean>;
  nextPeriodToStart?: number;
}

/**
 * Hook-owning wrapper around {@link MatchTimerView}. Used by the football and hockey live
 * desks, which register the timer's imperative API into their own contexts through the
 * `onGet*` props. All registered functions have stable identities so the parents can safely
 * store them in state without re-render loops.
 */
export const MatchTimer = ({
  matchId,
  periodNumber,
  onTimerUpdate,
  onGetCurrentTime,
  onGetCurrentElapsedSeconds,
  onGetToggleFunction,
  onGetResetFunction,
  onGetStartFunction,
  onGetStopFunction,
  controlsEnabled = true,
  isActive = true,
  keybindsEnabled = false,
  periodStartSeconds = 0,
  clockDisplayMode = 'absolute',
  onPeriodControlClick,
  canEndPeriod,
  getPeriodControlButtonText,
  getPeriodControlAction,
  periodLoading,
  nextPeriodToStart,
}: MatchTimerProps): ReactElement => {
  const { t } = useTranslation();
  const {
    displayTime,
    displayTimeMs,
    isRunning,
    loading,
    error,
    initialLoadComplete,
    startTimer,
    stopTimer,
    setTimer,
    adjustTimer,
    getCurrentElapsedSeconds,
  } = useMatchTimer({ matchId, autoConnect: isActive, onTimerUpdate });

  const periodOffsetSeconds: number = Math.max(0, Math.floor(periodStartSeconds));
  const visibleDisplayTime: string = clockDisplayMode === 'period'
    ? formatElapsedMs(Math.max(0, displayTimeMs - periodOffsetSeconds * 1000))
    : displayTime;

  // Expose the latest visible time through a stable getter.
  const visibleTimeRef = useRef<string>(visibleDisplayTime);
  visibleTimeRef.current = visibleDisplayTime;
  const getCurrentTime = useCallback((): string => visibleTimeRef.current, []);

  const handleStart = useCallback(async (): Promise<void> => {
    const resolvedPeriod: number | undefined =
      typeof periodNumber === 'number' && Number.isFinite(periodNumber) && periodNumber >= 1
        ? periodNumber
        : undefined;
    await startTimer(resolvedPeriod);
  }, [startTimer, periodNumber]);

  const handleStop = useCallback(async (): Promise<void> => {
    await stopTimer();
  }, [stopTimer]);

  // "Reset" rewinds to the start of the current period, not to 0.
  const handleReset = useCallback(async (): Promise<void> => {
    await setTimer(periodOffsetSeconds);
  }, [setTimer, periodOffsetSeconds]);

  const isRunningRef = useRef<boolean>(isRunning);
  isRunningRef.current = isRunning;
  const handleToggle = useCallback(async (): Promise<void> => {
    if (isRunningRef.current) {
      await handleStop();
    } else {
      await handleStart();
    }
  }, [handleStart, handleStop]);

  const handleSetTime = useCallback(async (seconds: number): Promise<void> => {
    const absoluteSeconds: number = clockDisplayMode === 'period'
      ? periodOffsetSeconds + seconds
      : seconds;
    await setTimer(absoluteSeconds);
  }, [setTimer, clockDisplayMode, periodOffsetSeconds]);

  useEffect(() => {
    if (!isActive) return;
    onGetCurrentTime?.(getCurrentTime);
    onGetCurrentElapsedSeconds?.(getCurrentElapsedSeconds);
    onGetToggleFunction?.(handleToggle);
    onGetStartFunction?.(handleStart);
    onGetStopFunction?.(handleStop);
    onGetResetFunction?.(handleReset);
  }, [
    isActive,
    onGetCurrentTime,
    onGetCurrentElapsedSeconds,
    onGetToggleFunction,
    onGetStartFunction,
    onGetStopFunction,
    onGetResetFunction,
    getCurrentTime,
    getCurrentElapsedSeconds,
    handleToggle,
    handleStart,
    handleStop,
    handleReset,
  ]);

  const periodControl: PeriodControlDescriptor | undefined = useMemo(() => {
    if (!onPeriodControlClick || !getPeriodControlButtonText) return undefined;
    const canEnd: boolean = canEndPeriod ? canEndPeriod() : false;
    const action: 'end' | 'start' | 'finish' | null = getPeriodControlAction
      ? getPeriodControlAction()
      : (canEnd ? 'end' : 'start');
    if (action === null) return undefined;
    const targetPeriod: number | undefined = action === 'start' ? nextPeriodToStart : periodNumber;
    const disabled: boolean = targetPeriod !== undefined && periodLoading
      ? Boolean(periodLoading[targetPeriod])
      : false;
    const title: string = action === 'finish'
      ? t('matchTimer.finishMatchTitle', 'No further period can follow; complete the match')
      : action === 'end'
        ? t('matchTimer.endPeriodTitle', 'End the current period')
        : t('matchTimer.startPeriodTitle', 'Start the next period');
    return {
      label: getPeriodControlButtonText(),
      title,
      disabled,
      action,
      onClick: onPeriodControlClick,
    };
  }, [onPeriodControlClick, getPeriodControlButtonText, getPeriodControlAction, canEndPeriod, periodNumber, nextPeriodToStart, periodLoading, t]);

  return (
    <MatchTimerView
      displayTime={initialLoadComplete ? visibleDisplayTime : '--:--'}
      isRunning={isRunning}
      loading={loading}
      error={error}
      controlsEnabled={controlsEnabled}
      keybindsEnabled={keybindsEnabled}
      onToggle={handleToggle}
      onAdjust={adjustTimer}
      onReset={handleReset}
      onSetTime={handleSetTime}
      periodControl={periodControl}
    />
  );
};
