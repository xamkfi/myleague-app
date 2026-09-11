import React from 'react';
import './LiveMatchTimer.scss';
import { MatchTimer } from '../../../../../components/MatchTimer';
import { useMatchTimerContext } from '../context';
import type { FootballMatchDto } from '../../../../../types/football/footballTypes';
import {
  extraTimeStartPeriod,
  getPeriodDurationSeconds,
  getPeriodLabel,
  isPenaltyShootoutPeriod,
  penaltyShootoutPeriod,
  resolveMatchRules,
} from '../utils/lineupValidation';

const MemoizedTimer = React.memo(MatchTimer);

interface LiveMatchTimerProps {
  currentMatch: FootballMatchDto;
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
  startDisabledReason?: string;
  showExtraTimeButton?: boolean;
  showPenaltyShootoutButton?: boolean;
  extraTimeLoading?: boolean;
  penaltyShootoutLoading?: boolean;
  onRecordExtraTime?: () => void;
  onRecordPenaltyShootout?: () => void;
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
  startDisabledReason,
  showExtraTimeButton = false,
  showPenaltyShootoutButton = false,
  extraTimeLoading = false,
  penaltyShootoutLoading = false,
  onRecordExtraTime,
  onRecordPenaltyShootout,
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

  const rules = resolveMatchRules(currentMatch.matchRules);
  const numberOfHalves = rules.numberOfHalves;
  const extraTimeStart = extraTimeStartPeriod(rules);
  const psoPeriod = penaltyShootoutPeriod(rules);
  const periodDurationSeconds = getPeriodDurationSeconds(currentPeriod, rules);

  const isInShootout = isPenaltyShootoutPeriod(currentPeriod, rules);
  const inPeriodElapsedSeconds: number = Math.max(0, elapsedTimeSeconds - currentPeriodStartSeconds);
  const shouldPeriodEnd = periodDurationSeconds > 0
    && inPeriodElapsedSeconds >= periodDurationSeconds
    && !isInShootout;

  const controlsEnabled = startedPeriods.has(currentPeriod)
    && !endedPeriods.has(currentPeriod)
    && !isInShootout;

  const periodsToShow: number[] = [];
  for (let i = 1; i <= numberOfHalves; i++) {
    periodsToShow.push(i);
  }
  if (currentMatch.wentToExtraTime) {
    for (let i = 0; i < rules.extraTimeHalfCount; i++) {
      periodsToShow.push(extraTimeStart + i);
    }
  }
  if (currentMatch.wentToPenaltyShootout) {
    periodsToShow.push(psoPeriod);
  }

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
            <div key={p} className={`period-chip ${getChipStatus(p)} ${p > numberOfHalves ? 'period-chip--extra' : ''}`}>
              {`${getPeriodLabel(p, rules)}: ${getChipStatus(p)}`}
            </div>
          ))}
        </div>
        {(showExtraTimeButton || showPenaltyShootoutButton) && currentMatch.status === 'InProgress' && (
          <div className="period-row extra-actions">
            {showExtraTimeButton && onRecordExtraTime && (
              <button
                type="button"
                className="start-match-btn"
                onClick={onRecordExtraTime}
                disabled={extraTimeLoading || loading}
              >
                {extraTimeLoading ? 'Starting extra time…' : 'Start extra time'}
              </button>
            )}
            {showPenaltyShootoutButton && onRecordPenaltyShootout && (
              <button
                type="button"
                className="start-match-btn"
                onClick={onRecordPenaltyShootout}
                disabled={penaltyShootoutLoading || loading}
              >
                {penaltyShootoutLoading ? 'Starting PSO…' : 'Start penalty shootout'}
              </button>
            )}
          </div>
        )}
        <div className="clock-time">
          {currentMatch.status === 'Scheduled' ? (
            <div className="start-match-container">
              <button
                onClick={onStartMatch}
                disabled={loading || isStartMatchDisabled}
                className="start-match-btn"
                title={isStartMatchDisabled ? (startDisabledReason ?? 'Set lineup to start') : undefined}
              >
                {isStartMatchDisabled
                  ? (startDisabledReason ?? 'Set lineup to start')
                  : 'Start Match'}
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
