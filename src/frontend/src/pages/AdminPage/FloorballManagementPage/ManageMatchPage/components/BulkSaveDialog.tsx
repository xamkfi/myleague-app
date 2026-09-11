import { useEffect, useRef, useState } from 'react';
import './BulkSaveDialog.scss';

export interface BulkSavePayload {
  count: number;
  periodNumber: number;
  timeInSeconds: number;
}

interface BulkSaveDialogProps {
  isOpen: boolean;
  goalieName: string;
  teamName: string;
  /** Currently active period (1..N). Used as the default value for the period selector. */
  currentPeriod: number;
  /** Total number of regular periods (typically 3). Drives the period dropdown options. */
  numberOfPeriods: number;
  /** Length of a regular period in minutes. Used to compute the "last minute" preset. */
  periodDurationMinutes: number;
  /** Current elapsed time (seconds) of the live timer. Pre-fills the time fields. */
  currentElapsedSeconds: number;
  /** Submit handler. Receives the count + period + time the user chose. */
  onSubmit: (payload: BulkSavePayload) => Promise<void>;
  onClose: () => void;
  loading: boolean;
  /** Optional async error from the parent (e.g. server validation failure). */
  errorMessage?: string | null;
}

const MAX_BULK_SAVES: number = 99;

const clampInt = (raw: string, min: number, max: number, fallback: number): number => {
  const parsed: number = parseInt(raw, 10);
  if (Number.isNaN(parsed)) return fallback;
  return Math.max(min, Math.min(max, parsed));
};

/**
 * Modal dialog for bulk-recording saves for a single goalie. Designed for the common
 * scorekeeper recovery case where the live recorder forgot to mark individual saves during
 * the period and wants to backfill an aggregate count (e.g. "20 saves in P1") in one go.
 *
 * All saves are recorded at the same `timeInSeconds` mark, since the original ordering
 * is no longer recoverable; the recorder picks a sensible bucket via the time field
 * (default: current elapsed time on the live timer, with a "Last minute" preset for the
 * period-end backfill case the user asked for).
 */
const BulkSaveDialog = ({
  isOpen,
  goalieName,
  teamName,
  currentPeriod,
  numberOfPeriods,
  periodDurationMinutes,
  currentElapsedSeconds,
  onSubmit,
  onClose,
  loading,
  errorMessage,
}: BulkSaveDialogProps) => {
  const countInputRef = useRef<HTMLInputElement | null>(null);
  // Tracks the previous `isOpen` so we can detect the closed→open transition. Without this
  // the reset effect would re-run every time `currentElapsedSeconds` (or any other live
  // prop) ticks while the dialog is open, clobbering whatever the user has typed into the
  // count / time fields — a 1Hz timer tick would reset count back to 1 once per second.
  const wasOpenRef = useRef<boolean>(false);

  const [count, setCount] = useState<number>(1);
  const [periodNumber, setPeriodNumber] = useState<number>(currentPeriod);
  const initialMinutes: number = Math.floor(currentElapsedSeconds / 60);
  const initialSeconds: number = currentElapsedSeconds % 60;
  const [timeMinutes, setTimeMinutes] = useState<number>(initialMinutes);
  const [timeSeconds, setTimeSeconds] = useState<number>(initialSeconds);

  // Re-sync the form only on the closed→open transition so a stale period/time from a
  // previous open session doesn't leak in, but once the user is editing we leave the
  // fields alone even as live props (current period / elapsed seconds) keep updating.
  useEffect(() => {
    const justOpened: boolean = isOpen && !wasOpenRef.current;
    wasOpenRef.current = isOpen;
    if (!justOpened) return;

    setCount(1);
    setPeriodNumber(currentPeriod);
    setTimeMinutes(Math.floor(currentElapsedSeconds / 60));
    setTimeSeconds(currentElapsedSeconds % 60);
    countInputRef.current?.focus();
    countInputRef.current?.select();
  }, [isOpen, currentPeriod, currentElapsedSeconds]);

  const handleKeyDown = (e: React.KeyboardEvent<HTMLDivElement>): void => {
    if (e.key === 'Escape') {
      e.preventDefault();
      e.stopPropagation();
      onClose();
      return;
    }
    if (e.key === 'Enter' && canSubmit) {
      e.preventDefault();
      e.stopPropagation();
      void submit();
    }
  };

  const setLastMinutePreset = (): void => {
    // The "last minute" preset places the time pointer one minute before the period ends so
    // any subsequently-recorded events still slot in after these bulk saves. For atypical
    // matches with a sub-2-minute period length we floor at 0 to avoid negative times.
    const lastMinuteStart: number = Math.max(0, periodDurationMinutes - 1);
    setTimeMinutes(lastMinuteStart);
    setTimeSeconds(0);
  };

  const setCurrentTimePreset = (): void => {
    setTimeMinutes(Math.floor(currentElapsedSeconds / 60));
    setTimeSeconds(currentElapsedSeconds % 60);
  };

  const submit = async (): Promise<void> => {
    const timeInSeconds: number = timeMinutes * 60 + timeSeconds;
    await onSubmit({ count, periodNumber, timeInSeconds });
  };

  const canSubmit: boolean = count > 0 && !loading;
  const periodOptions: number[] = Array.from({ length: Math.max(1, numberOfPeriods) }, (_, i) => i + 1);

  if (!isOpen) return null;

  return (
    <div className="bulk-save-modal-overlay" onClick={onClose} role="presentation">
      <div
        className="bulk-save-modal"
        onClick={(e) => e.stopPropagation()}
        onKeyDown={handleKeyDown}
        role="dialog"
        aria-modal="true"
        aria-labelledby="bulk-save-modal-title"
      >
        <div className="bulk-save-modal__header">
          <div>
            <h3 id="bulk-save-modal-title">Bulk record saves</h3>
            <p className="bulk-save-modal__subtitle">
              {goalieName ? `${goalieName} · ${teamName}` : teamName}
            </p>
          </div>
          <button
            className="bulk-save-modal__close"
            onClick={onClose}
            disabled={loading}
            type="button"
            aria-label="Close"
          >
            ×
          </button>
        </div>

        <div className="bulk-save-modal__body">
          <div className="field">
            <label htmlFor="bulk-save-count">Number of saves</label>
            <input
              id="bulk-save-count"
              ref={countInputRef}
              type="number"
              className="number-input"
              min={1}
              max={MAX_BULK_SAVES}
              value={count}
              onChange={(e) => setCount(clampInt(e.target.value, 1, MAX_BULK_SAVES, 1))}
              aria-describedby="bulk-save-count-help"
            />
            <small id="bulk-save-count-help" className="field-help">
              Each save is recorded as an individual event so existing per-event tooling
              (deletion, period stats, etc.) keeps working.
            </small>
          </div>

          <div className="bulk-save-modal__row">
            <div className="field">
              <label htmlFor="bulk-save-period">Period</label>
              <select
                id="bulk-save-period"
                className="select-field"
                value={periodNumber}
                onChange={(e) => setPeriodNumber(parseInt(e.target.value, 10) || 1)}
              >
                {periodOptions.map((p) => (
                  <option key={p} value={p}>
                    Period {p}
                  </option>
                ))}
              </select>
            </div>

            <div className="field">
              <label htmlFor="bulk-save-time-minutes">Time within period</label>
              <div className="time-input-group">
                <input
                  id="bulk-save-time-minutes"
                  type="number"
                  className="time-input"
                  value={timeMinutes}
                  onChange={(e) => setTimeMinutes(clampInt(e.target.value, 0, 99, 0))}
                  min={0}
                  max={99}
                  placeholder="MM"
                  aria-label="Minutes"
                />
                <span className="time-separator" aria-hidden="true">
                  :
                </span>
                <input
                  id="bulk-save-time-seconds"
                  type="number"
                  className="time-input"
                  value={timeSeconds}
                  onChange={(e) => setTimeSeconds(clampInt(e.target.value, 0, 59, 0))}
                  min={0}
                  max={59}
                  placeholder="SS"
                  aria-label="Seconds"
                />
              </div>
            </div>
          </div>

          <div className="bulk-save-modal__presets">
            <button type="button" className="preset-btn" onClick={setCurrentTimePreset} disabled={loading}>
              Use current timer
            </button>
            <button type="button" className="preset-btn" onClick={setLastMinutePreset} disabled={loading}>
              Place at last minute
            </button>
          </div>

          {errorMessage && (
            <div className="field-error" role="alert">
              {errorMessage}
            </div>
          )}
        </div>

        <div className="bulk-save-modal__footer">
          <button onClick={onClose} className="cancel-btn" type="button" disabled={loading}>
            Cancel
          </button>
          <button onClick={() => void submit()} disabled={!canSubmit} className="submit-btn" type="button">
            {loading ? 'Recording…' : `Record ${count} save${count === 1 ? '' : 's'}`}
          </button>
        </div>
      </div>
    </div>
  );
};

export default BulkSaveDialog;
