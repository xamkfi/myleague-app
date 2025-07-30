import React from 'react';
import { Timer } from '../../../../../../components/Timer/Timer';
import type { FloorballMatchDto } from '../../../../../../types/floorball/floorballTypes';
import type { LocalClock } from './types';

// Create a memoized Timer component to prevent unnecessary re-renders
const MemoizedTimer = React.memo(Timer);

interface LiveMatchTimerProps {
  currentMatch: FloorballMatchDto;
  clock: LocalClock;
  isOpen: boolean;
  loading: boolean;
  startedPeriods: Set<number>;
  endedPeriods: Set<number>;
  nextPeriodToStart: number;
  periodLoading: Record<number, boolean>;
  currentTimerElapsedTime: number;
  onStartMatch: () => Promise<void>;
  onPeriodControlClick: () => void;
  onTimerUpdate: (update: any) => void;
  onGetCurrentTime: (getTime: () => string) => void;
  canEndPeriod: () => boolean;
  getPeriodStatus: () => string;
  getPeriodControlButtonText: () => string;
  isInOvertime: () => boolean;
  isInShootout: () => boolean;
  formatTime: (minutes: number, seconds: number) => string;
}

const LiveMatchTimer: React.FC<LiveMatchTimerProps> = ({
  currentMatch,
  clock,
  isOpen,
  loading,
  nextPeriodToStart,
  periodLoading,
  onStartMatch,
  onPeriodControlClick,
  onTimerUpdate,
  onGetCurrentTime,
  canEndPeriod,
  getPeriodStatus,
  getPeriodControlButtonText,
  isInOvertime,
  isInShootout
}) => {
  return (
    <div className={`clock-score-section ${isInOvertime() ? 'overtime' : ''} ${isInShootout() ? 'shootout' : ''}`}>
      {currentMatch.status === 'Completed' && (
        <div className="match-finished-notice">
          <span className="notice-icon">🏁</span>
          <span className="notice-text">Match has been finished. Live tracking has been stopped.</span>
        </div>
      )}
      {currentMatch.status !== 'InProgress' && currentMatch.status !== 'Completed' && (
        <div className="not-live-notice">
          <span className="notice-icon">⏸️</span>
          <span className="notice-text">Match is not live yet. Use the clock button to start the match and first period.</span>
        </div>
      )}
      
      {/* Period Management - Simplified */}
      <div className="period-management">
        <div className="period-status">
          Period {clock.period}: {getPeriodStatus()}
        </div>
      </div>
      
      {/* Timer Component */}
      <div className="timer-container">
        {currentMatch.status === 'Scheduled' ? (
          <div className="start-match-container">
            <button 
              onClick={onStartMatch}
              disabled={loading}
              className="start-match-btn"
            >
              🏁 Start Match
            </button>
            <div className="start-match-hint">
              Click to start the match. After starting, you can use the timer controls below.
            </div>
          </div>
        ) : currentMatch.status === 'InProgress' ? (
          <MemoizedTimer 
            key={`timer-${currentMatch.id}`} // Remove status from key to prevent re-mounting
            matchId={currentMatch.id} 
            periodNumber={clock.period}
            isActive={isOpen} // Only activate timer when modal is open
            onTimerUpdate={onTimerUpdate}
            onGetCurrentTime={onGetCurrentTime}
          />
        ) : (
          <div className="timer-loading">
            <div>00:00</div>
          </div>
        )}
      </div>
      
      {/* Period Control Button - End Period or Start Period */}
      <div className="clock-start-reset">
        <button 
          onClick={onPeriodControlClick} 
          className="period-control-btn"
          title={canEndPeriod() ? "End the current period" : "Start the next period"}
          disabled={periodLoading[canEndPeriod() ? clock.period : nextPeriodToStart]}
        >
          {getPeriodControlButtonText()}
        </button>
      </div>
    </div>
  );
};

export default LiveMatchTimer; 