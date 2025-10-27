import React from 'react';
import './LiveMatchTimer.scss';
import { Timer } from '../../../../../components/Timer/Timer';
import type { FloorballMatchDto } from '../../../../../types/floorball/floorballTypes';
import type { TimerUpdate } from '../../../../../api/common/timerService';
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
  onTimerUpdate: (update: TimerUpdate) => void;
  onGetCurrentTime: (getTime: () => string) => void;
  onGetToggleFunction: (toggleFunction: () => Promise<void>) => void;
  canEndPeriod: () => boolean;
  getPeriodStatus: () => string;
  getPeriodControlButtonText: () => string;
  isInOvertime: () => boolean;
  isInShootout: () => boolean;
  formatTime: (minutes: number, seconds: number) => string;
  keybindsEnabled: boolean;
  isStartMatchDisabled: boolean;
}

const LiveMatchTimer = ({
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
  onGetToggleFunction,
  canEndPeriod,
  getPeriodControlButtonText,
  startedPeriods,
  endedPeriods,
  keybindsEnabled,
  isStartMatchDisabled
}: LiveMatchTimerProps) => {
  const getChipStatus = (p: number) => {
    if (endedPeriods.has(p)) return 'completed';
    if (startedPeriods.has(p)) return 'started';
    return 'upcoming';
  };

  return (
    <>
      <div className="clock-card">
        <div className="clock-inner">
          <div className="period-row">
            {[1, 2, 3].map((p) => (
              <div key={p} className={`period-chip ${getChipStatus(p)}`}>
                {`Period ${p}: ${getChipStatus(p)}`}
              </div>
            ))}
          </div>
          <div className="clock-time">
            {currentMatch.status === 'Scheduled' ? (
              <div className="start-match-container">
                <button
                  onClick={onStartMatch}
                  disabled={loading || isStartMatchDisabled}
                  className="start-match-btn"
                >
                  {isStartMatchDisabled ? 'Select goalies to start' : 'Start Match'}
                </button>
              </div>
            ) : (
              <>
                <div className="clock-digits">
                  <MemoizedTimer
                    key={`timer-${currentMatch.id}`}
                    matchId={currentMatch.id}
                    periodNumber={clock.period}
                    isActive={isOpen}
                    onTimerUpdate={onTimerUpdate}
                    onGetCurrentTime={onGetCurrentTime}
                    onGetToggleFunction={onGetToggleFunction}
                    keybindsEnabled={keybindsEnabled}
                    onPeriodControlClick={onPeriodControlClick}
                    canEndPeriod={canEndPeriod}
                    getPeriodControlButtonText={getPeriodControlButtonText}
                    periodLoading={periodLoading}
                    nextPeriodToStart={nextPeriodToStart}
                  />
                </div>

              </>
            )}
          </div>
        </div>
      </div>

      {/* Keep toolbar wrapper for future additions if needed */}
      <div className="clock-toolbar" />
    </>
  );
};

export default LiveMatchTimer; 