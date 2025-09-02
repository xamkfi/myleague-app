import { useEffect, useCallback, useMemo, useState } from 'react';
import { useTimer } from '../../hooks/useTimer';
import { TimeInputModal } from './TimeInputModal';
import './Timer.scss';
import type { TimerUpdate } from '../../api/common/timerService';

interface TimerProps {
  matchId: string;
  periodNumber?: number;
  onTimerUpdate?: (update: TimerUpdate) => void;
  onGetCurrentTime?: (getTime: () => string) => void;
  onGetToggleFunction?: (toggleFunction: () => Promise<void>) => void;
  isActive?: boolean; // New prop to control when timer should be active
  keybindsEnabled?: boolean; // New prop to show keybind indicator
}

export const Timer = ({ matchId, periodNumber, onTimerUpdate, onGetCurrentTime, onGetToggleFunction, isActive = true, keybindsEnabled = false }: TimerProps) => {
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

  // Notify parent component of the getCurrentTime function
  useEffect(() => {
    if (onGetCurrentTime && isActive) {
      onGetCurrentTime(getCurrentTime);
    }
  }, [onGetCurrentTime, getCurrentTime, isActive]);

  const handleStart = async () => {
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
  };

  const handleStop = () => {
    console.log('=== TIMER STOP BUTTON CLICKED ===');
    console.log('Match ID:', matchId);
    console.log('Timer Active:', isActive);
    stopTimer();
    console.log('=== TIMER STOP COMPLETED ===');
  };

  const handleReset = () => {
    console.log('=== TIMER RESET BUTTON CLICKED ===');
    console.log('Match ID:', matchId);
    console.log('Timer Active:', isActive);
    resetTimer();
    console.log('=== TIMER RESET COMPLETED ===');
  };

  const handleToggle = async () => {
    if (timerState.isRunning) {
      handleStop();
    } else {
      await handleStart();
    }
  };

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

  // Memoize button disabled states to prevent blinking during SignalR updates
  const buttonStates = useMemo(() => {
    const toggleDisabled = loading || !isActive;
    const resetDisabled = loading || !isActive;
    const setTimeDisabled = loading || !isActive;
    const adjustDisabled = loading || !isActive;
    
    return {
      toggleDisabled,
      resetDisabled,
      setTimeDisabled,
      adjustDisabled
    };
  }, [loading, isActive]);

  // Don't render timer controls if not active
  if (!isActive) {
    return (
      <div className="timer-component">
        <div className="timer-display">
          <div className="timer-time">
            {timerState.elapsedTime}
          </div>
        </div>
        <div className="timer-inactive">
          Timer inactive
        </div>
      </div>
    );
  }

  return (
    <div className="timer-component">
      <div className="timer-display">
        <div className="timer-time">
          {timerState.elapsedTime}
        </div>
      </div>

      <div className="timer-controls">
        <button
          onClick={handleToggle}
          disabled={buttonStates.toggleDisabled}
          className={`timer-button ${timerState.isRunning ? 'pause' : 'start'}`}
        >
          <span className={`key-label ${keybindsEnabled ? '' : 'disabled'}`}>(Space) </span>
          {timerState.isRunning ? 'Pause' : 'Start'}
        </button>
        
        <button
          onClick={handleReset}
          disabled={buttonStates.resetDisabled}
          className="timer-button reset"
        >
          Reset
        </button>

        <button
          onClick={handleOpenTimeInput}
          disabled={buttonStates.setTimeDisabled}
          className="timer-button set-time"
          title="Set specific time"
        >
          Set Time
        </button>
      </div>

      {/* Time Adjustment Controls */}
      <div className="timer-adjustments">
        <div className="adjustment-group">
          <span className="adjustment-label">Quick Adjust</span>
          <div className="adjustment-buttons">
            <button
              onClick={() => handleAdjustTime(-60)}
              disabled={buttonStates.adjustDisabled}
              className="timer-button adjust-time decrease minute-back"
              title="Go back 1 minute"
            >
              ⏪ 1min
            </button>
            <button
              onClick={() => handleAdjustTime(-10)}
              disabled={buttonStates.adjustDisabled}
              className="timer-button adjust-time decrease seconds-back"
              title="Go back 10 seconds"
            >
              ◀ 10s
            </button>
            <button
              onClick={() => handleAdjustTime(10)}
              disabled={buttonStates.adjustDisabled}
              className="timer-button adjust-time increase seconds-forward"
              title="Advance 10 seconds"
            >
              ▶ 10s
            </button>
            <button
              onClick={() => handleAdjustTime(60)}
              disabled={buttonStates.adjustDisabled}
              className="timer-button adjust-time increase minute-forward"
              title="Advance 1 minute"
            >
              ⏩ 1min
            </button>
          </div>
        </div>
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