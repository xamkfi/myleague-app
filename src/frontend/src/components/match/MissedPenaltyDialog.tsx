import { useRef, useState, type FormEvent, type ReactElement } from 'react';
import { useTranslation } from 'react-i18next';
import { useDialogBehavior } from '../../hooks/useDialogBehavior';
import EventPeriodSelect, { type EventPeriodSelectLabels } from './EventPeriodSelect';
import EventTimeInput from './EventTimeInput';
import './BulkSaveDialog.scss';

export interface MissedPenaltyShooter {
  id: string;
  label: string;
}

export interface MissedPenaltyPayload {
  playerId: string;
  periodNumber: number;
  timeInSeconds: number;
}

interface MissedPenaltyDialogProps {
  teamName: string;
  shooters: readonly MissedPenaltyShooter[];
  /** Played periods the event can be put in. */
  periods: readonly number[];
  overtimePeriodNumber: number;
  shootoutPeriodNumber: number;
  periodLabels: EventPeriodSelectLabels;
  initialPeriod: number;
  /** Match clock when the dialog was opened. */
  initialTimeInSeconds: number;
  /** Rejects with a user-facing error when the event could not be recorded. */
  onSubmit: (payload: MissedPenaltyPayload) => Promise<void>;
  onClose: () => void;
}

/** Records a missed penalty shot. Shared by the floorball, football and hockey live desks. */
function MissedPenaltyDialog({
  teamName,
  shooters,
  periods,
  overtimePeriodNumber,
  shootoutPeriodNumber,
  periodLabels,
  initialPeriod,
  initialTimeInSeconds,
  onSubmit,
  onClose,
}: MissedPenaltyDialogProps): ReactElement {
  const { t } = useTranslation();
  const dialogRef = useRef<HTMLFormElement | null>(null);
  const shooterRef = useRef<HTMLSelectElement | null>(null);
  const [playerId, setPlayerId] = useState<string>('');
  const [periodNumber, setPeriodNumber] = useState<number>(initialPeriod);
  const [minutes, setMinutes] = useState<number>(Math.floor(initialTimeInSeconds / 60));
  const [seconds, setSeconds] = useState<number>(initialTimeInSeconds % 60);
  const [saving, setSaving] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  useDialogBehavior(true, dialogRef, onClose, shooterRef);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>): Promise<void> => {
    event.preventDefault();
    if (!playerId || saving) return;
    setSaving(true);
    setError(null);
    try {
      await onSubmit({ playerId, periodNumber, timeInSeconds: minutes * 60 + seconds });
      onClose();
    } catch (err) {
      setError(err instanceof Error && err.message ? err.message : t('matchManage.missedPenalty.failed'));
      setSaving(false);
    }
  };

  return (
    <div className="bulk-save-modal-overlay" onClick={onClose} role="presentation">
      <form
        ref={dialogRef}
        tabIndex={-1}
        className="bulk-save-modal"
        onClick={(event) => event.stopPropagation()}
        onSubmit={(event) => void handleSubmit(event)}
        role="dialog"
        aria-modal="true"
        aria-labelledby="missed-penalty-title"
      >
        <div className="bulk-save-modal__header">
          <div>
            <h3 id="missed-penalty-title">{t('matchManage.missedPenalty.title')}</h3>
            <p className="bulk-save-modal__subtitle">{teamName}</p>
          </div>
          <button
            className="bulk-save-modal__close"
            onClick={onClose}
            disabled={saving}
            type="button"
            aria-label={t('common.close')}
          >
            ×
          </button>
        </div>

        <div className="bulk-save-modal__body">
          <div className="field">
            <label htmlFor="missed-penalty-shooter">{t('matchManage.missedPenalty.shooter')}</label>
            <select
              id="missed-penalty-shooter"
              ref={shooterRef}
              className="select-field"
              value={playerId}
              onChange={(event) => setPlayerId(event.target.value)}
            >
              <option value="">{t('matchManage.missedPenalty.selectShooter')}</option>
              {shooters.map((shooter) => (
                <option key={shooter.id} value={shooter.id}>
                  {shooter.label}
                </option>
              ))}
            </select>
          </div>

          <div className="bulk-save-modal__row">
            <EventPeriodSelect
              id="missed-penalty-period"
              periodNumber={periodNumber}
              periods={periods}
              overtimePeriodNumber={overtimePeriodNumber}
              shootoutPeriodNumber={shootoutPeriodNumber}
              labels={periodLabels}
              onChange={setPeriodNumber}
            />
            <EventTimeInput
              idPrefix="missed-penalty-time"
              label={t('matchManage.missedPenalty.time')}
              minutes={minutes}
              seconds={seconds}
              onChange={(nextMinutes, nextSeconds) => {
                setMinutes(nextMinutes);
                setSeconds(nextSeconds);
              }}
            />
          </div>

          {error && (
            <div className="field-error" role="alert">
              {error}
            </div>
          )}
        </div>

        <div className="bulk-save-modal__footer">
          <button onClick={onClose} className="cancel-btn" type="button" disabled={saving}>
            {t('common.cancel')}
          </button>
          <button className="submit-btn" type="submit" disabled={!playerId || saving}>
            {saving ? t('common.saving') : t('matchManage.missedPenalty.submit')}
          </button>
        </div>
      </form>
    </div>
  );
}

export default MissedPenaltyDialog;
