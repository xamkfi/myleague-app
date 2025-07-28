import { useTimer } from '../../hooks/useTimer';
import './Timer.scss';
import type { TimerUpdate } from '../../api/common/timerService';

interface TimerProps {
  matchId: string;
  periodNumber?: number;
  onTimerUpdate?: (update: TimerUpdate) => void;
}

export const Timer = ({ matchId, periodNumber, onTimerUpdate }: TimerProps) => {
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

  const handleStart = () => {
    startTimer(periodNumber);
  };

  const handleStop = () => {
    stopTimer();
  };

  const handleReset = () => {
    resetTimer();
  };

  const handleCreate = () => {
    createTimer();
  };



  return (
    <div className="timer-component">
      <div className="timer-display">
        <div className="timer-time">{timerState.elapsedTime}</div>
        <div className="timer-status">
          {timerState.isRunning ? 'Running' : 'Stopped'}
          {typeof timerState.periodNumber === 'number' && ` - Period ${timerState.periodNumber}`}
        </div>
      </div>

      <div className="timer-controls">
        <button
          onClick={handleCreate}
          disabled={loading}
          className="timer-button create"
        >
          Create Timer
        </button>
        
        <button
          onClick={handleStart}
          disabled={loading || timerState.isRunning}
          className="timer-button start"
        >
          Start
        </button>
        
        <button
          onClick={handleStop}
          disabled={loading || !timerState.isRunning}
          className="timer-button stop"
        >
          Stop
        </button>
        
        <button
          onClick={handleReset}
          disabled={loading}
          className="timer-button reset"
        >
          Reset
        </button>
        

      </div>

      {loading && <div className="timer-loading">Loading...</div>}
      {error && <div className="timer-error">Error: {error}</div>}
      
      <div className="timer-info">
        <div>Match ID: {matchId}</div>
        <div>Last Updated: {new Date(timerState.lastUpdated).toLocaleTimeString()}</div>
      </div>
    </div>
  );
}; 