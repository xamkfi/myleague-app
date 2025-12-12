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
  onGetCurrentElapsedSeconds: (getSeconds: () => number) => void;
  onGetToggleFunction: (toggleFunction: () => Promise<void>) => void;
  onGetResetFunction?: (resetFunction: () => void) => void;
  onGetStartFunction?: (startFunction: () => Promise<void>) => void;
  onGetStopFunction?: (stopFunction: () => void) => void;
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
  currentTimerElapsedTime,
  nextPeriodToStart,
  periodLoading,
  onStartMatch,
  onPeriodControlClick,
  onTimerUpdate,
  onGetCurrentTime,
  onGetCurrentElapsedSeconds,
  onGetToggleFunction,
  onGetResetFunction,
  onGetStartFunction,
  onGetStopFunction,
  canEndPeriod,
  getPeriodControlButtonText,
  startedPeriods,
  endedPeriods,
  isInShootout,
  keybindsEnabled,
  isStartMatchDisabled
}: LiveMatchTimerProps) => {
  const getChipStatus = (p: number) => {
    if (endedPeriods.has(p)) return 'completed';
    if (startedPeriods.has(p)) return 'started';
    return 'upcoming';
  };

  const periodLabels: Record<number, string> = {
    1: 'Period 1',
    2: 'Period 2',
    3: 'Overtime',
    4: 'Shootout'
  };

  // Turn digits red at 15:00 (900s) and after, except during shootout
  const shouldPeriodEnd = currentTimerElapsedTime >= 900 && !isInShootout();
  // Timer controls enabled only if current period has started and not ended, and not in shootout
  const controlsEnabled = startedPeriods.has(clock.period) && !endedPeriods.has(clock.period) && clock.period !== 4;

  // Determine which periods to show
  const periodsToShow = [1, 2];
  if (currentMatch.wentToOvertime) {
    periodsToShow.push(3);
  }
  if (currentMatch.wentToShootout) {
    periodsToShow.push(4);
  }

  return (
    <>
      <div className="clock-card">
        <div className="clock-inner">
          <div className="period-row">
            {periodsToShow.map((p) => (
              <div key={p} className={`period-chip ${getChipStatus(p)} ${p > 2 ? 'period-chip--extra' : ''}`}>
                {`${periodLabels[p]}: ${getChipStatus(p)}`}
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
            ) : currentMatch.status === 'Completed' ? (
              <div className="start-match-container">
                <div className="match-completed-message">
                  🏁 Match Completed
                </div>
              </div>
            ) : (
              <>
                <div className={`clock-digits${shouldPeriodEnd ? ' timer-digits--critical' : ''}`}>
                  <MemoizedTimer
                    key={`timer-${currentMatch.id}`}
                    matchId={currentMatch.id}
                    periodNumber={clock.period}
                    isActive={isOpen}
                    onTimerUpdate={onTimerUpdate}
                    onGetCurrentTime={onGetCurrentTime}
                    onGetCurrentElapsedSeconds={onGetCurrentElapsedSeconds}
                    onGetToggleFunction={onGetToggleFunction}
                    onGetResetFunction={onGetResetFunction}
                    onGetStartFunction={onGetStartFunction}
                    onGetStopFunction={onGetStopFunction}
                    controlsEnabled={controlsEnabled}
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
    </>
  );
};

export default LiveMatchTimer; 