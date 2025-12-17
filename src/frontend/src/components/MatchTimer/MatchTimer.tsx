import { useEffect, useCallback, useMemo, useState } from 'react';
import { useMatchTimer } from '../../hooks/useMatchTimer';
import { TimeInputModal } from '../Timer/TimeInputModal';
import './MatchTimer.scss';
import type { TimerUpdate } from '../../api/common/timerService';
import EditIcon from '../../assets/basicIcons/edit.svg';

interface MatchTimerProps {
  matchId: string;
  periodNumber?: number;
  onTimerUpdate?: (update: TimerUpdate) => void;
  onGetCurrentTime?: (getTime: () => string) => void;
  onGetCurrentElapsedSeconds?: (getSeconds: () => number) => void;
  onGetToggleFunction?: (toggleFunction: () => Promise<void>) => void;
  onGetResetFunction?: (resetFunction: () => void) => void;
  onGetStartFunction?: (startFunction: () => Promise<void>) => void;
  onGetStopFunction?: (stopFunction: () => void) => void;
  controlsEnabled?: boolean;
  isActive?: boolean;
  keybindsEnabled?: boolean;
  // Period control props
  onPeriodControlClick?: () => void;
  canEndPeriod?: () => boolean;
  getPeriodControlButtonText?: () => string;
  periodLoading?: Record<number, boolean>;
  nextPeriodToStart?: number;
}

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
  onPeriodControlClick,
  canEndPeriod,
  getPeriodControlButtonText,
  periodLoading,
  nextPeriodToStart,
}: MatchTimerProps) => {
  const [showTimeInputModal, setShowTimeInputModal] = useState(false);
  
  const {
    displayTime,
    isRunning,
    loading,
    error,
    initialLoadComplete,
    startTimer,
    stopTimer,
    resetTimer,
    setTimer,
    adjustTimer,
    createTimer,
    getCurrentElapsedSeconds,
  } = useMatchTimer({
    matchId,
    autoConnect: isActive,
    onTimerUpdate,
  });
  
  // Provide getCurrentTime function to parent
  const getCurrentTime = useCallback(() => displayTime, [displayTime]);
  
  useEffect(() => {
    if (onGetCurrentTime && isActive) {
      onGetCurrentTime(getCurrentTime);
    }
  }, [onGetCurrentTime, getCurrentTime, isActive]);
  
  // Provide getCurrentElapsedSeconds function to parent
  useEffect(() => {
    if (onGetCurrentElapsedSeconds && isActive) {
      onGetCurrentElapsedSeconds(getCurrentElapsedSeconds);
    }
  }, [onGetCurrentElapsedSeconds, getCurrentElapsedSeconds, isActive]);
  
  // Handle start button
  const handleStart = useCallback(async () => {
    try {
      await createTimer();
      await startTimer(periodNumber);
    } catch (err) {
      console.error('Error starting timer:', err);
    }
  }, [createTimer, startTimer, periodNumber]);
  
  // Handle stop button
  const handleStop = useCallback(async () => {
    try {
      await stopTimer();
    } catch (err) {
      console.error('Error stopping timer:', err);
    }
  }, [stopTimer]);
  
  // Handle reset button
  const handleReset = useCallback(async () => {
    try {
      await resetTimer();
    } catch (err) {
      console.error('Error resetting timer:', err);
    }
  }, [resetTimer]);
  
  // Handle toggle (play/pause)
  const handleToggle = useCallback(async () => {
    if (isRunning) {
      await handleStop();
    } else {
      await handleStart();
    }
  }, [isRunning, handleStop, handleStart]);
  
  // Handle set time from modal
  const handleSetTime = useCallback(async (timeInSeconds: number) => {
    try {
      await setTimer(timeInSeconds);
      setShowTimeInputModal(false);
    } catch (err) {
      console.error('Error setting timer:', err);
    }
  }, [setTimer]);
  
  // Handle time adjustment
  const handleAdjustTime = useCallback(async (adjustmentInSeconds: number) => {
    try {
      await adjustTimer(adjustmentInSeconds);
    } catch (err) {
      console.error('Error adjusting timer:', err);
    }
  }, [adjustTimer]);
  
  // Expose toggle function to parent
  useEffect(() => {
    if (onGetToggleFunction && isActive) {
      onGetToggleFunction(handleToggle);
    }
  }, [onGetToggleFunction, handleToggle, isActive]);
  
  // Expose start/stop/reset handlers to parent
  useEffect(() => {
    if (!isActive) return;
    if (onGetStartFunction) onGetStartFunction(handleStart);
    if (onGetStopFunction) onGetStopFunction(handleStop);
    if (onGetResetFunction) onGetResetFunction(handleReset);
  }, [isActive, onGetStartFunction, onGetStopFunction, onGetResetFunction, handleStart, handleStop, handleReset]);
  
  // Memoize button disabled states
  const buttonStates = useMemo(() => {
    const controlsBlocked = !controlsEnabled;
    return {
      toggleDisabled: loading || controlsBlocked,
      resetDisabled: loading || controlsBlocked,
      setTimeDisabled: loading || controlsBlocked,
      adjustDisabled: loading || controlsBlocked,
    };
  }, [loading, controlsEnabled]);
  
  // Period control button state
  const endPeriod = useMemo(() => {
    if (!getPeriodControlButtonText) {
      return { disabled: false, title: '', label: '' };
    }
    const canEnd = canEndPeriod ? canEndPeriod() : false;
    const targetPeriod = canEnd ? periodNumber : nextPeriodToStart;
    const disabled = (targetPeriod !== undefined && periodLoading)
      ? Boolean(periodLoading[targetPeriod])
      : false;
    const title = canEnd ? 'End the current period' : 'Start the next period';
    const label = getPeriodControlButtonText();
    return { disabled, title, label };
  }, [canEndPeriod, periodNumber, nextPeriodToStart, periodLoading, getPeriodControlButtonText]);
  
  // Show loading indicator until initial load is complete
  const timeDisplay = initialLoadComplete ? displayTime : '--:--';
  
  return (
    <div className="timer-component" data-keybinds-enabled={keybindsEnabled ? 'true' : undefined}>
      <div className="timer-display">
        <div className="timer-time">
          {timeDisplay}
        </div>
      </div>

      <div className="timer-controls">
        <button
          onClick={() => setShowTimeInputModal(true)}
          disabled={buttonStates.setTimeDisabled}
          className="timer-button set-time"
          title="Edit time"
        >
          <img src={EditIcon} alt="" aria-hidden="true" />
        </button>

        <div className="timer-adjustments">
          <div className="adjustment-group">
            <div className="adjustment-buttons">
              <button
                onClick={() => handleAdjustTime(-60)}
                disabled={buttonStates.adjustDisabled}
                className="timer-button adjust-time decrease minute-back"
                title="Go back 1 minute"
              >
                1 min
              </button>
              <button
                onClick={() => handleAdjustTime(-10)}
                disabled={buttonStates.adjustDisabled}
                className="timer-button adjust-time decrease seconds-back"
                title="Go back 10 seconds"
              >
                10s
              </button>
              <button
                onClick={() => handleAdjustTime(-1)}
                disabled={buttonStates.adjustDisabled}
                className="timer-button adjust-time decrease one-second-back"
                title="Go back 1 second"
              >
                1s
              </button>
              <button
                onClick={handleToggle}
                disabled={buttonStates.toggleDisabled}
                className={`timer-button ${isRunning ? 'pause' : 'start'}`}
              >
                {isRunning ? 'Pause' : 'Play'}
              </button>
              <button
                onClick={() => handleAdjustTime(1)}
                disabled={buttonStates.adjustDisabled}
                className="timer-button adjust-time increase one-second-forward"
                title="Advance 1 second"
              >
                1s
              </button>
              <button
                onClick={() => handleAdjustTime(10)}
                disabled={buttonStates.adjustDisabled}
                className="timer-button adjust-time increase seconds-forward"
                title="Advance 10 seconds"
              >
                10s
              </button>
              <button
                onClick={() => handleAdjustTime(60)}
                disabled={buttonStates.adjustDisabled}
                className="timer-button adjust-time increase minute-forward"
                title="Advance 1 minute"
              >
                1 min
              </button>
            </div>
          </div>
        </div>

        <button
          onClick={handleReset}
          disabled={buttonStates.resetDisabled}
          className="timer-button reset"
          title="Reset clock"
        >
          R
        </button>

        {onPeriodControlClick && getPeriodControlButtonText && (
          <button
            onClick={onPeriodControlClick}
            className="timer-button end-period-inline"
            title={endPeriod.title}
            disabled={endPeriod.disabled}
          >
            {endPeriod.label}
          </button>
        )}
      </div>

      {error && <div className="timer-error">Error: {error}</div>}

      <TimeInputModal
        isOpen={showTimeInputModal}
        currentTime={displayTime}
        onSetTime={handleSetTime}
        onClose={() => setShowTimeInputModal(false)}
        loading={loading}
      />
    </div>
  );
};
