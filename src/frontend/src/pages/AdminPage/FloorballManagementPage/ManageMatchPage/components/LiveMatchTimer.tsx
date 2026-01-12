import React from 'react';
import './LiveMatchTimer.scss';
import { MatchTimer } from '../../../../../components/MatchTimer';
import { useMatchTimerContext } from '../context';
import type { FloorballMatchDto } from '../../../../../types/floorball/floorballTypes';

// Create a memoized Timer component to prevent unnecessary re-renders
const MemoizedTimer = React.memo(MatchTimer);

interface LiveMatchTimerProps {
  currentMatch: FloorballMatchDto;
  isOpen: boolean;
  loading: boolean;
  startedPeriods: Set<number>;
  endedPeriods: Set<number>;
  nextPeriodToStart: number;
  periodLoading: Record<number, boolean>;
  onStartMatch: () => Promise<void>;
  onPeriodControlClick: () => void;
  canEndPeriod: () => boolean;
  getPeriodControlButtonText: () => string;
  keybindsEnabled: boolean;
  isStartMatchDisabled: boolean;
}

const LiveMatchTimer = ({
  currentMatch,
  isOpen,
  loading,
  startedPeriods,
  endedPeriods,
  nextPeriodToStart,
  periodLoading,
  onStartMatch,
  onPeriodControlClick,
  canEndPeriod,
  getPeriodControlButtonText,
  keybindsEnabled,
  isStartMatchDisabled,
}: LiveMatchTimerProps) => {
  const {
    currentPeriod,
    elapsedTimeSeconds,
    registerCallback,
    handleTimerUpdate,
  } = useMatchTimerContext();

  const getChipStatus = (p: number) => {
    if (endedPeriods.has(p)) return 'completed';
    if (startedPeriods.has(p)) return 'started';
    return 'upcoming';
  };

  const periodLabels: Record<number, string> = {
    1: 'Period 1',
    2: 'Period 2',
    3: 'Overtime',
    4: 'Shootout',
  };

  // Turn digits red at 15:00 (900s) and after, except during shootout
  const isInShootout = currentPeriod === 4;
  const shouldPeriodEnd = elapsedTimeSeconds >= 900 && !isInShootout;
  
  // Timer controls enabled only if current period has started and not ended, and not in shootout
  const controlsEnabled = startedPeriods.has(currentPeriod) && !endedPeriods.has(currentPeriod) && currentPeriod !== 4;

  // Determine which periods to show
  const periodsToShow = [1, 2];
  if (currentMatch.wentToOvertime) {
    periodsToShow.push(3);
  }
  if (currentMatch.wentToShootout) {
    periodsToShow.push(4);
  }

  // Register timer callbacks when they're provided
  const handleGetCurrentTime = (getTime: () => string) => {
    registerCallback('getCurrentTime', getTime);
  };

  const handleGetCurrentElapsedSeconds = (getSeconds: () => number) => {
    registerCallback('getCurrentElapsedSeconds', getSeconds);
  };

  const handleGetToggleFunction = (toggleFn: () => Promise<void>) => {
    registerCallback('toggle', toggleFn);
  };

  const handleGetResetFunction = (resetFn: () => void) => {
    registerCallback('reset', resetFn);
  };

  const handleGetStartFunction = (startFn: () => Promise<void>) => {
    registerCallback('start', startFn);
  };

  const handleGetStopFunction = (stopFn: () => void) => {
    registerCallback('stop', stopFn);
  };

  return (
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
                <div className={`clock-digits${shouldPeriodEnd ? ' timer-digits--critical' : ''}`}>
                  <MemoizedTimer
                    key={`timer-${currentMatch.id}`}
                    matchId={currentMatch.id}
                periodNumber={currentPeriod}
                    isActive={isOpen}
                onTimerUpdate={handleTimerUpdate}
                onGetCurrentTime={handleGetCurrentTime}
                onGetCurrentElapsedSeconds={handleGetCurrentElapsedSeconds}
                onGetToggleFunction={handleGetToggleFunction}
                onGetResetFunction={handleGetResetFunction}
                onGetStartFunction={handleGetStartFunction}
                onGetStopFunction={handleGetStopFunction}
                    controlsEnabled={controlsEnabled}
                    keybindsEnabled={keybindsEnabled}
                    onPeriodControlClick={onPeriodControlClick}
                    canEndPeriod={canEndPeriod}
                    getPeriodControlButtonText={getPeriodControlButtonText}
                    periodLoading={periodLoading}
                    nextPeriodToStart={nextPeriodToStart}
                  />
                </div>
            )}
          </div>
        </div>
      </div>
  );
};

export default LiveMatchTimer; 
