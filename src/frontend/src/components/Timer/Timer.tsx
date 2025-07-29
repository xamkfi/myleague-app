import { useEffect, useCallback, useMemo } from 'react';
import { useTimer } from '../../hooks/useTimer';
import { floorballMatchEventService } from '../../api/floorball/floorballMatchEventService';
import './Timer.scss';
import type { TimerUpdate } from '../../api/common/timerService';

interface TimerProps {
  matchId: string;
  periodNumber?: number;
  onTimerUpdate?: (update: TimerUpdate) => void;
  onGetCurrentTime?: (getTime: () => string) => void;
  isActive?: boolean; // New prop to control when timer should be active
}

export const Timer = ({ matchId, periodNumber, onTimerUpdate, onGetCurrentTime, isActive = true }: TimerProps) => {
  // Debug logging for component lifecycle - only log once per actual mount/unmount
  useEffect(() => {
    console.log('🔄 Timer component MOUNTED:', { matchId, periodNumber, isActive });
    return () => {
      console.log('🔄 Timer component UNMOUNTED:', { matchId, periodNumber, isActive });
    };
  }, []); // Empty dependency array to only run once on mount/unmount

  const {
    timerState,
    loading,
    error,
    startTimer,
    stopTimer,
    resetTimer,
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

  // Memoize button disabled states to prevent blinking during SignalR updates
  const buttonStates = useMemo(() => {
    const startDisabled = loading || timerState.isRunning || !isActive;
    const stopDisabled = loading || !timerState.isRunning || !isActive;
    const resetDisabled = loading || !isActive;
    
    return {
      startDisabled,
      stopDisabled,
      resetDisabled
    };
  }, [loading, timerState.isRunning, isActive]);

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
          onClick={handleStart}
          disabled={buttonStates.startDisabled}
          className="timer-button start"
        >
          Start
        </button>
        
        <button
          onClick={handleStop}
          disabled={buttonStates.stopDisabled}
          className="timer-button stop"
        >
          Stop
        </button>
        
        <button
          onClick={handleReset}
          disabled={buttonStates.resetDisabled}
          className="timer-button reset"
        >
          Reset
        </button>
      </div>

      {error && <div className="timer-error">Error: {error}</div>}
    </div>
  );
}; 