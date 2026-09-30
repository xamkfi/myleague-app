import { memo, useCallback, useState, type ReactElement } from 'react';
import { useTranslation } from 'react-i18next';
import { TimeInputModal } from '../Timer/TimeInputModal';
import EditIcon from '../../assets/basicIcons/edit.svg';
import './MatchTimer.scss';

export interface PeriodControlDescriptor {
  /** Button label, already translated by the caller. */
  label: string;
  /** Tooltip, already translated by the caller. */
  title: string;
  disabled: boolean;
  /** `end` and `finish` render the danger style; `start` the primary style. */
  action: 'end' | 'start' | 'finish';
  onClick: () => void;
}

export interface MatchTimerViewProps {
  /** Formatted clock digits. Use `--:--` style placeholders while loading. */
  displayTime: string;
  isRunning: boolean;
  loading: boolean;
  error: string | null;
  controlsEnabled: boolean;
  keybindsEnabled?: boolean;
  /** Optional caption rendered under the digits (e.g. active period and in-period time). */
  caption?: string;
  onToggle: () => void | Promise<void>;
  onAdjust: (seconds: number) => void | Promise<void>;
  onReset: () => void | Promise<void>;
  /** Receives the seconds entered in the editor, in the same frame of reference as `displayTime`. */
  onSetTime: (seconds: number) => Promise<void>;
  periodControl?: PeriodControlDescriptor;
}

interface AdjustButtonSpec {
  seconds: number;
  className: string;
  labelKey: string;
  titleKey: string;
  fallbackLabel: string;
  fallbackTitle: string;
}

const REWIND_BUTTONS: AdjustButtonSpec[] = [
  { seconds: -60, className: 'decrease minute-back', labelKey: 'matchTimer.oneMinute', titleKey: 'matchTimer.back1min', fallbackLabel: '1 min', fallbackTitle: 'Go back 1 minute' },
  { seconds: -10, className: 'decrease seconds-back', labelKey: 'matchTimer.tenSeconds', titleKey: 'matchTimer.back10s', fallbackLabel: '10s', fallbackTitle: 'Go back 10 seconds' },
  { seconds: -1, className: 'decrease one-second-back', labelKey: 'matchTimer.oneSecond', titleKey: 'matchTimer.back1s', fallbackLabel: '1s', fallbackTitle: 'Go back 1 second' },
];

const FORWARD_BUTTONS: AdjustButtonSpec[] = [
  { seconds: 1, className: 'increase one-second-forward', labelKey: 'matchTimer.oneSecond', titleKey: 'matchTimer.forward1s', fallbackLabel: '1s', fallbackTitle: 'Advance 1 second' },
  { seconds: 10, className: 'increase seconds-forward', labelKey: 'matchTimer.tenSeconds', titleKey: 'matchTimer.forward10s', fallbackLabel: '10s', fallbackTitle: 'Advance 10 seconds' },
  { seconds: 60, className: 'increase minute-forward', labelKey: 'matchTimer.oneMinute', titleKey: 'matchTimer.forward1min', fallbackLabel: '1 min', fallbackTitle: 'Advance 1 minute' },
];

/**
 * Presentational match clock. Owns nothing but the "edit time" modal visibility; all timer
 * state and actions come from the parent so the same view can be driven by a context
 * (floorball) or by the hook-owning `MatchTimer` wrapper (football / hockey).
 */
const MatchTimerViewComponent = ({
  displayTime,
  isRunning,
  loading,
  error,
  controlsEnabled,
  keybindsEnabled = false,
  caption,
  onToggle,
  onAdjust,
  onReset,
  onSetTime,
  periodControl,
}: MatchTimerViewProps): ReactElement => {
  const { t } = useTranslation();
  const [showTimeInputModal, setShowTimeInputModal] = useState<boolean>(false);

  const disabled: boolean = loading || !controlsEnabled;

  const handleSetTime = useCallback(async (seconds: number): Promise<void> => {
    await onSetTime(seconds);
    setShowTimeInputModal(false);
  }, [onSetTime]);

  const renderAdjust = (spec: AdjustButtonSpec): ReactElement => (
    <button
      key={spec.seconds}
      type="button"
      onClick={() => onAdjust(spec.seconds)}
      disabled={disabled}
      className={`timer-button adjust-time ${spec.className}`}
      title={t(spec.titleKey, spec.fallbackTitle)}
    >
      {t(spec.labelKey, spec.fallbackLabel)}
    </button>
  );

  return (
    <div className="timer-component" data-keybinds-enabled={keybindsEnabled ? 'true' : undefined}>
      <div className="timer-display">
        <div className={`timer-time${isRunning ? ' timer-time--running' : ''}`}>{displayTime}</div>
        {caption && <div className="timer-caption">{caption}</div>}
      </div>

      <div className="timer-controls">
        <button
          type="button"
          onClick={() => setShowTimeInputModal(true)}
          disabled={disabled}
          className="timer-button set-time"
          title={t('matchTimer.editTime', 'Edit time')}
          aria-label={t('matchTimer.editTime', 'Edit time')}
        >
          <img src={EditIcon} alt="" aria-hidden="true" />
        </button>

        <div className="timer-adjustments">
          <div className="adjustment-buttons">
            <div className="adjustment-cluster">{REWIND_BUTTONS.map(renderAdjust)}</div>
            <button
              type="button"
              onClick={() => onToggle()}
              disabled={disabled}
              className={`timer-button ${isRunning ? 'pause' : 'start'}`}
              title={isRunning ? t('matchTimer.pause', 'Pause') : t('matchTimer.play', 'Play')}
              aria-label={isRunning ? t('matchTimer.pause', 'Pause') : t('matchTimer.play', 'Play')}
            >
              {isRunning ? t('matchTimer.pause', 'Pause') : t('matchTimer.play', 'Play')}
            </button>
            <div className="adjustment-cluster">{FORWARD_BUTTONS.map(renderAdjust)}</div>
          </div>
        </div>

        <button
          type="button"
          onClick={() => onReset()}
          disabled={disabled}
          className="timer-button reset"
          title={t('matchTimer.reset', 'Reset clock to period start')}
          aria-label={t('matchTimer.reset', 'Reset clock to period start')}
        >
          R
        </button>
      </div>

      {periodControl && (
        <button
          type="button"
          onClick={periodControl.onClick}
          className={`timer-button period-control period-control--${periodControl.action}`}
          title={periodControl.title}
          disabled={periodControl.disabled}
        >
          {periodControl.label}
        </button>
      )}

      {error && (
        <div className="timer-error" role="alert">
          {t('matchTimer.error', 'Timer error: {{message}}', { message: error })}
        </div>
      )}

      <TimeInputModal
        isOpen={showTimeInputModal}
        currentTime={displayTime}
        onSetTime={handleSetTime}
        onClose={() => setShowTimeInputModal(false)}
        loading={loading}
      />
    </div>
  );
};

export const MatchTimerView = memo(MatchTimerViewComponent);
