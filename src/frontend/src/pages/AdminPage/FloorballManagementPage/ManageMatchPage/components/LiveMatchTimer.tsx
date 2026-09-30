import { useMemo, type ReactElement } from 'react';
import { useTranslation } from 'react-i18next';
import './LiveMatchTimer.scss';
import { MatchTimerView, type PeriodControlDescriptor as ViewPeriodControl } from '../../../../../components/MatchTimer';
import { useMatchClock, useMatchTimerContext } from '../context';
import type { FloorballMatchDto } from '../../../../../types/floorball/floorballTypes';
import { formatEventTimeMmSs } from '../../../../../utils/matchEventFormat';
import { resolveFloorballRules } from '../../../../../utils/floorballPeriod';
import type { PeriodControlDescriptor, PeriodKind } from '../hooks/usePeriodManagement';

type ChipStatus = 'completed' | 'started' | 'upcoming';

interface LiveMatchTimerProps {
  currentMatch: FloorballMatchDto;
  loading: boolean;
  startedPeriods: Set<number>;
  endedPeriods: Set<number>;
  periodControl: PeriodControlDescriptor | null;
  onPeriodControlClick: () => void;
  onStartMatch: () => Promise<void>;
  keybindsEnabled: boolean;
  isStartMatchDisabled: boolean;
  /** Translated reason the Start Match button is disabled; used as label and tooltip. */
  startDisabledReason?: string;
  overtimePeriodNumber: number;
  shootoutPeriodNumber: number;
  showSkipToShootout?: boolean;
  skipToShootoutLoading?: boolean;
  onSkipToShootout?: () => void;
}

const LiveMatchTimer = ({
  currentMatch,
  loading,
  startedPeriods,
  endedPeriods,
  periodControl,
  onPeriodControlClick,
  onStartMatch,
  keybindsEnabled,
  isStartMatchDisabled,
  startDisabledReason,
  overtimePeriodNumber,
  shootoutPeriodNumber,
  showSkipToShootout = false,
  skipToShootoutLoading = false,
  onSkipToShootout,
}: LiveMatchTimerProps): ReactElement => {
  const { t } = useTranslation();
  const { currentPeriod, currentPeriodStartSeconds, timer } = useMatchTimerContext();
  const clock = useMatchClock();

  const rules = resolveFloorballRules(currentMatch.matchRules);
  const numberOfPeriods: number = rules.numberOfPeriods;
  const isInShootout: boolean = currentPeriod === shootoutPeriodNumber;
  const isInOvertime: boolean = currentPeriod === overtimePeriodNumber;
  const currentPeriodDurationSeconds: number = (isInOvertime ? rules.overtimeDurationMinutes : rules.periodDurationMinutes) * 60;

  const periodLabel = (period: number): string => {
    if (period === overtimePeriodNumber) return t('matchPage.events.overtimeName', 'Overtime');
    if (period === shootoutPeriodNumber) return t('matchPage.events.shootoutName', 'Shootout');
    return t('matchPage.events.periodName', 'Period {{number}}', { number: period });
  };

  const chipStatus = (period: number): ChipStatus => {
    if (endedPeriods.has(period)) return 'completed';
    if (startedPeriods.has(period)) return 'started';
    return 'upcoming';
  };

  const chipStatusLabel = (status: ChipStatus): string => {
    switch (status) {
      case 'completed': return t('floorball.matches.manage.periodStatus.completed', 'Completed');
      case 'started': return t('floorball.matches.manage.periodStatus.started', 'Live');
      default: return t('floorball.matches.manage.periodStatus.upcoming', 'Upcoming');
    }
  };

  const periodsToShow: number[] = useMemo(() => {
    const periods: number[] = Array.from({ length: numberOfPeriods }, (_, i) => i + 1);
    if (currentMatch.wentToOvertime) periods.push(overtimePeriodNumber);
    if (currentMatch.wentToShootout) periods.push(shootoutPeriodNumber);
    return periods;
  }, [numberOfPeriods, currentMatch.wentToOvertime, currentMatch.wentToShootout, overtimePeriodNumber, shootoutPeriodNumber]);

  // The clock is continuous across periods, so the "should this period end now" alert
  // compares the in-period elapsed time against the configured period duration.
  const inPeriodElapsedSeconds: number = Math.max(0, clock.elapsedTimeSeconds - currentPeriodStartSeconds);
  const shouldPeriodEnd: boolean = !isInShootout && inPeriodElapsedSeconds >= currentPeriodDurationSeconds;

  const controlsEnabled: boolean = startedPeriods.has(currentPeriod) && !endedPeriods.has(currentPeriod) && !isInShootout;

  const periodControlLabel = (descriptor: PeriodControlDescriptor): string => {
    const kindKey: Record<PeriodKind, string> = { regular: 'Period', overtime: 'Overtime', shootout: 'Shootout' };
    if (descriptor.loading) {
      return descriptor.action === 'start'
        ? t('floorball.matches.manage.periodControl.starting', 'Starting...')
        : t('floorball.matches.manage.periodControl.ending', 'Ending...');
    }
    if (descriptor.action === 'finish') {
      return t('floorball.matches.manage.periodControl.finishMatch', 'Finish match');
    }
    const key: string = `floorball.matches.manage.periodControl.${descriptor.action}${kindKey[descriptor.kind]}`;
    return t(key, { number: descriptor.period, defaultValue: `${descriptor.action} ${descriptor.kind} ${descriptor.period}` });
  };

  const periodControlTitle = (descriptor: PeriodControlDescriptor): string => {
    if (descriptor.action === 'finish') {
      return t('floorball.matches.manage.periodControl.finishMatchTitle', 'No further period can follow; complete the match');
    }
    return descriptor.action === 'end'
      ? t('matchTimer.endPeriodTitle', 'End the current period')
      : t('matchTimer.startPeriodTitle', 'Start the next period');
  };

  const viewPeriodControl: ViewPeriodControl | undefined = periodControl
    ? {
        label: periodControlLabel(periodControl),
        title: periodControlTitle(periodControl),
        disabled: periodControl.loading,
        action: periodControl.action,
        onClick: onPeriodControlClick,
      }
    : undefined;

  // Between periods `currentPeriod` already points at the period that starts next, so the
  // in-period time would be meaningless; show an intermission caption instead.
  const isIntermission: boolean = startedPeriods.size > 0 && !startedPeriods.has(currentPeriod);
  // The last playable period is over and nothing can follow it: the desk only waits for
  // the operator to complete the match.
  const isAwaitingFinish: boolean = endedPeriods.has(currentPeriod) && periodControl?.action === 'finish';
  const caption: string = isAwaitingFinish
    ? t('floorball.matches.manage.regulationDecided', 'Decided in regulation · finish the match')
    : isIntermission
    ? t('floorball.matches.manage.intermission', { next: periodLabel(currentPeriod), defaultValue: 'Intermission · {{next}} next' })
    : isInShootout
      ? periodLabel(currentPeriod)
      : `${periodLabel(currentPeriod)} · ${formatEventTimeMmSs(inPeriodElapsedSeconds)}`;

  return (
    <div className="clock-card">
      <div className="clock-inner">
        <div className="period-row" role="list">
          {periodsToShow.map((p) => {
            const status: ChipStatus = chipStatus(p);
            return (
              <div
                key={p}
                role="listitem"
                className={`period-chip period-chip--${status}${p > numberOfPeriods ? ' period-chip--extra' : ''}${p === currentPeriod ? ' period-chip--current' : ''}`}
              >
                <span className="period-chip__dot" aria-hidden="true" />
                <span className="period-chip__name">{periodLabel(p)}</span>
                <span className="period-chip__status">{chipStatusLabel(status)}</span>
              </div>
            );
          })}
        </div>

        {showSkipToShootout && onSkipToShootout && currentMatch.status === 'InProgress' && (
          <div className="period-row extra-actions">
            <button
              type="button"
              className="start-match-btn start-match-btn--secondary"
              onClick={onSkipToShootout}
              disabled={skipToShootoutLoading || loading}
            >
              {skipToShootoutLoading
                ? t('matchPage.skippingToShootout', 'Skipping to penalty shootout...')
                : t('matchPage.skipToShootout', 'Skip to penalty shootout')}
            </button>
          </div>
        )}

        <div className="clock-time">
          {currentMatch.status === 'Scheduled' ? (
            <div className="start-match-container">
              <button
                type="button"
                onClick={onStartMatch}
                disabled={loading || isStartMatchDisabled}
                className="start-match-btn"
                title={isStartMatchDisabled ? startDisabledReason : undefined}
              >
                {isStartMatchDisabled
                  ? (startDisabledReason ?? t('floorball.matches.manage.selectGoalies', 'Select goalies to start'))
                  : t('floorball.matches.manage.startMatch', 'Start match')}
              </button>
            </div>
          ) : currentMatch.status === 'Completed' ? (
            <div className="start-match-container">
              <div className="match-completed-message">
                <span aria-hidden="true">🏁</span> {t('floorball.matches.manage.matchCompleted', 'Match completed')}
                <span className="match-completed-time">{clock.displayTime}</span>
              </div>
            </div>
          ) : (
            <div className={`clock-digits${shouldPeriodEnd ? ' timer-digits--critical' : ''}`}>
              <MatchTimerView
                displayTime={clock.initialLoadComplete ? clock.displayTime : '--:--'}
                isRunning={clock.isRunning}
                loading={timer.loading}
                error={timer.error}
                controlsEnabled={controlsEnabled}
                keybindsEnabled={keybindsEnabled}
                caption={caption}
                onToggle={timer.toggle}
                onAdjust={timer.adjust}
                onReset={timer.reset}
                onSetTime={timer.setTime}
                periodControl={viewPeriodControl}
              />
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default LiveMatchTimer;
