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
}

export const Timer = ({ matchId, periodNumber, onTimerUpdate, onGetCurrentTime }: TimerProps) => {
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
    onTimerUpdate,
  });

  // Provide a function to get current time to parent component
  const getCurrentTime = useCallback(() => timerState.elapsedTime, [timerState.elapsedTime]);

  // Notify parent component of the getCurrentTime function
  useEffect(() => {
    if (onGetCurrentTime) {
      onGetCurrentTime(getCurrentTime);
    }
  }, [onGetCurrentTime, getCurrentTime]);

  const handleStart = async () => {
    try {
      console.log('=== TIMER START BUTTON CLICKED ===');
      console.log('Match ID:', matchId);
      console.log('Period Number:', periodNumber);
      
      // Create timer first if it doesn't exist
      console.log('Step 1: Creating timer for match:', matchId);
      await createTimer();
      console.log('Step 1: Timer creation completed');
      
      // Start the period first if we have a period number
      if (periodNumber) {
        console.log(`Step 2: Starting period ${periodNumber} for match ${matchId}`);
        await floorballMatchEventService.startPeriod(matchId, periodNumber);
        console.log(`Step 2: Period ${periodNumber} started successfully`);
      }
      
      // Then start the timer
      console.log('Step 3: Starting timer...');
      startTimer(periodNumber);
      console.log('Step 3: Timer start initiated');
      console.log('=== TIMER START COMPLETED ===');
    } catch (error) {
      console.error('=== TIMER START FAILED ===');
      console.error('Error starting period or timer:', error);
    }
  };

  const handleStop = () => {
    console.log('=== TIMER STOP BUTTON CLICKED ===');
    console.log('Match ID:', matchId);
    stopTimer();
    console.log('=== TIMER STOP COMPLETED ===');
  };

  const handleReset = () => {
    console.log('=== TIMER RESET BUTTON CLICKED ===');
    console.log('Match ID:', matchId);
    resetTimer();
    console.log('=== TIMER RESET COMPLETED ===');
  };

  // Memoize button disabled states to prevent blinking during SignalR updates
  const buttonStates = useMemo(() => {
    const startDisabled = loading || timerState.isRunning;
    const stopDisabled = loading || !timerState.isRunning;
    const resetDisabled = loading;
    
    return {
      startDisabled,
      stopDisabled,
      resetDisabled
    };
  }, [loading, timerState.isRunning]);

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