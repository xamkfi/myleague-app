import { useState, useEffect, type FormEvent, type ReactElement } from 'react';
import { useTranslation } from 'react-i18next';
import './TimeInputModal.scss';

interface TimeInputModalProps {
  isOpen: boolean;
  currentTime: string;
  onSetTime: (timeInSeconds: number) => void | Promise<void>;
  onClose: () => void;
  loading?: boolean;
}

export const TimeInputModal = ({ isOpen, currentTime, onSetTime, onClose, loading = false }: TimeInputModalProps): ReactElement | null => {
  const { t } = useTranslation();
  const [minutes, setMinutes] = useState<string>('');
  const [seconds, setSeconds] = useState<string>('');
  const [error, setError] = useState<string>('');
  const [staticTime, setStaticTime] = useState<string>(''); // Store static snapshot of time

  // Capture a static snapshot of the current time ONCE when modal opens
  // Note: currentTime is intentionally excluded from deps to prevent continuous updates
  useEffect(() => {
    if (isOpen) {
      setStaticTime(currentTime);

      const parts: string[] = currentTime.split(':');
      if (parts.length >= 2) {
        // Handle both MM:SS and HH:MM:SS formats
        const mins: string = parts.length === 3 ? parts[1] : parts[0];
        const secs: string = parts.length === 3 ? parts[2] : parts[1];
        setMinutes(mins);
        setSeconds(secs);
      }
    } else {
      setError('');
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isOpen]); // Only depend on isOpen - currentTime intentionally excluded

  const handleSubmit = (e: FormEvent): void => {
    e.preventDefault();
    setError('');

    const minutesNum: number = parseInt(minutes, 10);
    const secondsNum: number = parseInt(seconds, 10);

    if (Number.isNaN(minutesNum) || Number.isNaN(secondsNum)) {
      setError(t('matchTimer.setTime.errors.invalid', 'Please enter valid numbers'));
      return;
    }
    if (minutesNum < 0 || secondsNum < 0) {
      setError(t('matchTimer.setTime.errors.negative', 'Time cannot be negative'));
      return;
    }
    if (secondsNum >= 60) {
      setError(t('matchTimer.setTime.errors.seconds', 'Seconds must be less than 60'));
      return;
    }
    if (minutesNum > 999) {
      setError(t('matchTimer.setTime.errors.minutes', 'Minutes cannot exceed 999'));
      return;
    }

    void onSetTime(minutesNum * 60 + secondsNum);
  };

  const handleClose = (): void => {
    setError('');
    onClose();
  };

  const handleMinutesChange = (value: string): void => {
    if (value === '' || /^\d+$/.test(value)) {
      setMinutes(value);
    }
  };

  const handleSecondsChange = (value: string): void => {
    if (value === '' || (/^\d{1,2}$/.test(value) && parseInt(value, 10) < 60)) {
      setSeconds(value);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="time-input-modal-overlay" onClick={handleClose}>
      <div className="time-input-modal" role="dialog" aria-modal="true" onClick={e => e.stopPropagation()}>
        <div className="time-input-modal-header">
          <h3>{t('matchTimer.setTime.title', 'Set timer')}</h3>
          <button
            type="button"
            className="close-button"
            onClick={handleClose}
            disabled={loading}
            aria-label={t('common.close', 'Close')}
          >
            ×
          </button>
        </div>

        <form onSubmit={handleSubmit} className="time-input-form">
          <div className="time-input-section">
            <label>
              {t('matchTimer.setTime.currentTime', 'Current time')}: <span className="current-time">{staticTime}</span>
            </label>
          </div>

          <div className="time-input-fields">
            <div className="time-field">
              <label htmlFor="minutes">{t('matchTimer.setTime.minutes', 'Minutes')}</label>
              <input
                id="minutes"
                type="text"
                inputMode="numeric"
                value={minutes}
                onChange={(e) => handleMinutesChange(e.target.value)}
                placeholder="00"
                disabled={loading}
                autoFocus
                autoComplete="off"
              />
            </div>

            <div className="time-separator">:</div>

            <div className="time-field">
              <label htmlFor="seconds">{t('matchTimer.setTime.seconds', 'Seconds')}</label>
              <input
                id="seconds"
                type="text"
                inputMode="numeric"
                value={seconds}
                onChange={(e) => handleSecondsChange(e.target.value)}
                placeholder="00"
                disabled={loading}
                maxLength={2}
                autoComplete="off"
              />
            </div>
          </div>

          {error && (
            <div className="error-message" role="alert">
              {error}
            </div>
          )}

          <div className="time-input-actions">
            <button
              type="button"
              onClick={handleClose}
              className="cancel-button"
              disabled={loading}
            >
              {t('common.cancel', 'Cancel')}
            </button>
            <button
              type="submit"
              className="set-time-button"
              disabled={loading}
            >
              {loading
                ? t('matchTimer.setTime.saving', 'Setting...')
                : t('matchTimer.setTime.submit', 'Set time')}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
