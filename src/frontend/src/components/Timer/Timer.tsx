import { useEffect, useCallback, useMemo, useState } from 'react';
import { useTimer } from '../../hooks/useTimer';
import { TimeInputModal } from './TimeInputModal';
import './Timer.scss';
import type { TimerUpdate } from '../../api/common/timerService';
import EditIcon from '../../assets/basicIcons/edit.svg';

interface TimerProps {
  matchId: string;
  periodNumber?: number;
  onTimerUpdate?: (update: TimerUpdate) => void;
  onGetCurrentTime?: (getTime: () => string) => void;
  onGetCurrentElapsedSeconds?: (getSeconds: () => number) => void;
  onGetToggleFunction?: (toggleFunction: () => Promise<void>) => void;
  onGetResetFunction?: (resetFunction: () => void) => void;
  onGetStartFunction?: (startFunction: () => Promise<void>) => void;
  onGetStopFunction?: (stopFunction: () => void) => void;
  //disable timer controls when period is not active
  controlsEnabled?: boolean;
  isActive?: boolean; // New prop to control when timer should be active
  keybindsEnabled?: boolean; // New prop to show keybind indicator
  // Optional period control wiring (used by pages like ManageMatch)
  onPeriodControlClick?: () => void;
  canEndPeriod?: () => boolean;
  getPeriodControlButtonText?: () => string;
  periodLoading?: Record<number, boolean>;
  nextPeriodToStart?: number;
}

export const Timer = ({ matchId, periodNumber, onTimerUpdate, onGetCurrentTime, onGetCurrentElapsedSeconds, onGetToggleFunction, onGetResetFunction, onGetStartFunction, onGetStopFunction, controlsEnabled = true, isActive = true, keybindsEnabled = false, onPeriodControlClick, canEndPeriod, getPeriodControlButtonText, periodLoading, nextPeriodToStart }: TimerProps) => {
  // State for time input modal
  const [showTimeInputModal, setShowTimeInputModal] = useState(false);

  // Debug logging for component lifecycle - only log once per actual mount/unmount
  useEffect(() => {
    console.log('🔄 Timer component MOUNTED:', { matchId, periodNumber, isActive });
    return () => {
      console.log('🔄 Timer component UNMOUNTED:', { matchId, periodNumber, isActive });
    };
  }, [matchId, periodNumber, isActive]); // Include all dependencies

  const {
    timerState,
    currentElapsedSeconds,
    loading,
    error,
    startTimer,
    stopTimer,
    resetTimer,
    setTimer,
    adjustTimer,
    createTimer,
  } = useTimer({
    matchId,
    autoConnect: isActive, // Only connect when timer should be active
    onTimerUpdate,
  });

  // Provide a function to get current time to parent component
  const getCurrentTime = useCallback(() => timerState.elapsedTime, [timerState.elapsedTime]);

  // Provide a function to get current elapsed seconds to parent component
  const getCurrentElapsedSecondsFunc = useCallback(() => currentElapsedSeconds, [currentElapsedSeconds]);

  // Notify parent component of the getCurrentTime function
  useEffect(() => {
    if (onGetCurrentTime && isActive) {
      onGetCurrentTime(getCurrentTime);
    }
  }, [onGetCurrentTime, getCurrentTime, isActive]);

  // Notify parent component of the getCurrentElapsedSeconds function
  useEffect(() => {
    if (onGetCurrentElapsedSeconds && isActive) {
      onGetCurrentElapsedSeconds(getCurrentElapsedSecondsFunc);
    }
  }, [onGetCurrentElapsedSeconds, getCurrentElapsedSecondsFunc, isActive]);

  const handleStart = useCallback(async () => {
    try {
      console.log('=== TIMER START BUTTON CLICKED ===');
      console.log('Match ID:', matchId);
      console.log('Period Number:', periodNumber);
      console.log('Timer Active:', isActive);
      
      // Create timer first if it doesn't exist
      console.log('Step 1: Creating timer for match:', matchId);
      await createTimer();
      console.log('Step 1: Timer creation completed');
      
      // Start the timer (period management is handled by LiveMatchModal)
      console.log('Step 2: Starting timer...');
      startTimer(periodNumber);
      console.log('Step 2: Timer start initiated');
      
      console.log('=== TIMER START COMPLETED ===');
    } catch (error) {
      console.error('=== TIMER START FAILED ===');
      console.error('Error starting timer:', error);
    }
  }, [matchId, periodNumber, isActive, createTimer, startTimer]);

  const handleStop = useCallback(() => {
    console.log('=== TIMER STOP BUTTON CLICKED ===');
    console.log('Match ID:', matchId);
    console.log('Timer Active:', isActive);
    stopTimer();
    console.log('=== TIMER STOP COMPLETED ===');
  }, [matchId, isActive, stopTimer]);

  const handleReset = useCallback(() => {
    console.log('=== TIMER RESET BUTTON CLICKED ===');
    console.log('Match ID:', matchId);
    console.log('Timer Active:', isActive);
    resetTimer();
    console.log('=== TIMER RESET COMPLETED ===');
  }, [resetTimer, matchId, isActive]);

  const handleToggle = useCallback(async () => {
    if (timerState.isRunning) {
      handleStop();
    } else {
      await handleStart();
    }
  }, [timerState.isRunning, handleStop, handleStart]);

  const handleSetTime = async (timeInSeconds: number) => {
    try {
      console.log('=== TIMER SET TIME BUTTON CLICKED ===');
      console.log('Match ID:', matchId);
      console.log('Time in seconds:', timeInSeconds);
      console.log('Timer Active:', isActive);
      
      await setTimer(timeInSeconds);
      setShowTimeInputModal(false);
      
      console.log('=== TIMER SET TIME COMPLETED ===');
    } catch (error) {
      console.error('=== TIMER SET TIME FAILED ===');
      console.error('Error setting timer:', error);
    }
  };

  const handleAdjustTime = async (adjustmentInSeconds: number) => {
    try {
      console.log('=== TIMER ADJUST TIME BUTTON CLICKED ===');
      console.log('Match ID:', matchId);
      console.log('Adjustment in seconds:', adjustmentInSeconds);
      console.log('Timer Active:', isActive);
      
      await adjustTimer(adjustmentInSeconds);
      
      console.log('=== TIMER ADJUST TIME COMPLETED ===');
    } catch (error) {
      console.error('=== TIMER ADJUST TIME FAILED ===');
      console.error('Error adjusting timer:', error);
    }
  };

  const handleOpenTimeInput = () => {
    setShowTimeInputModal(true);
  };

  const handleCloseTimeInput = () => {
    setShowTimeInputModal(false);
  };

  // Notify parent component of the toggle function
  useEffect(() => {
    if (onGetToggleFunction && isActive) {
      onGetToggleFunction(handleToggle);
    }
  }, [onGetToggleFunction, handleToggle, isActive]);

  // Expose start/stop/reset handlers to parent when active
  useEffect(() => {
    if (!isActive) return;
    if (onGetStartFunction) onGetStartFunction(handleStart);
    if (onGetStopFunction) onGetStopFunction(handleStop);
    if (onGetResetFunction) onGetResetFunction(handleReset);
  }, [isActive, onGetStartFunction, onGetStopFunction, onGetResetFunction, handleStart, handleStop, handleReset]);

  // Memoize button disabled states to prevent blinking during SignalR updates
  const buttonStates = useMemo(() => {
    const controlsBlocked = !controlsEnabled;
    const toggleDisabled = loading || controlsBlocked;
    const resetDisabled = loading || controlsBlocked;
    const setTimeDisabled = loading || controlsBlocked;
    const adjustDisabled = loading || controlsBlocked;
    
    return {
      toggleDisabled,
      resetDisabled,
      setTimeDisabled,
      adjustDisabled
    };
  }, [loading, controlsEnabled]);

  // Derive end/start period control state if the parent provided handlers
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



  return (
    <div className="timer-component" data-keybinds-enabled={keybindsEnabled ? 'true' : undefined}>
      <div className="timer-display">
        <div className="timer-time">
          {timerState.elapsedTime}
        </div>
      </div>

      <div className="timer-controls">
        <button
          onClick={handleOpenTimeInput}
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
                onClick={handleToggle}
                disabled={buttonStates.toggleDisabled}
                className={`timer-button ${timerState.isRunning ? 'pause' : 'start'}`}
              >
                {timerState.isRunning ? 'Pause' : 'Play'}
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
        currentTime={timerState.elapsedTime}
        onSetTime={handleSetTime}
        onClose={handleCloseTimeInput}
        loading={loading}
      />
    </div>
  );
}; 