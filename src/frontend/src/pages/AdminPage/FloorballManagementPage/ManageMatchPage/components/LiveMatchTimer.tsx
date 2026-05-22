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
  overtimePeriodNumber: number;
  shootoutPeriodNumber: number;
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
  overtimePeriodNumber,
  shootoutPeriodNumber,
}: LiveMatchTimerProps) => {
  const {
    currentPeriod,
    elapsedTimeSeconds,
    currentPeriodStartSeconds,
    registerCallback,
    handleTimerUpdate,
  } = useMatchTimerContext();

  const getChipStatus = (p: number) => {
    if (endedPeriods.has(p)) return 'completed';
    if (startedPeriods.has(p)) return 'started';
    return 'upcoming';
  };

  const rules = currentMatch.matchRules;
  const numberOfPeriods = rules?.numberOfPeriods ?? 2;
  const periodDurationSeconds = (rules?.periodDurationMinutes ?? 15) * 60;

  // Build dynamic period labels
  const periodLabels: Record<number, string> = {};
  for (let i = 1; i <= numberOfPeriods; i++) {
    periodLabels[i] = `Period ${i}`;
  }
  periodLabels[overtimePeriodNumber] = 'Overtime';
  periodLabels[shootoutPeriodNumber] = 'Shootout';

  // The clock display is continuous across periods, so the "should this period end now"
  // alert has to compare the in-period elapsed time (total elapsed minus the current
  // period's recorded start) against the configured period duration. Without this, the
  // digits would turn red as soon as elapsed >= 15min in period 2 even though only a few
  // seconds had been played in that period.
  const isInShootout = currentPeriod === shootoutPeriodNumber;
  const inPeriodElapsedSeconds: number = Math.max(0, elapsedTimeSeconds - currentPeriodStartSeconds);
  const shouldPeriodEnd = inPeriodElapsedSeconds >= periodDurationSeconds && !isInShootout;
  
  // Timer controls enabled only if current period has started and not ended, and not in shootout
  const controlsEnabled = startedPeriods.has(currentPeriod) && !endedPeriods.has(currentPeriod) && currentPeriod !== shootoutPeriodNumber;

  // Determine which periods to show
  const periodsToShow: number[] = [];
  for (let i = 1; i <= numberOfPeriods; i++) {
    periodsToShow.push(i);
  }
  if (currentMatch.wentToOvertime) {
    periodsToShow.push(overtimePeriodNumber);
  }
  if (currentMatch.wentToShootout) {
    periodsToShow.push(shootoutPeriodNumber);
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
            <div key={p} className={`period-chip ${getChipStatus(p)} ${p > numberOfPeriods ? 'period-chip--extra' : ''}`}>
                {`${periodLabels[p] ?? `Period ${p}`}: ${getChipStatus(p)}`}
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
                    periodStartSeconds={currentPeriodStartSeconds}
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
