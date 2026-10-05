import { useEffect, useMemo, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import './PenaltyRecordingForm.scss';
import type { FloorballMatchDto, FloorballTeam } from '../../../../../types/floorball/floorballTypes';
import type { FloorballPlayerDto } from '../../../../../api/floorball/floorballPlayerService';
import type { PenaltyForm } from './types';
import { formatPlayerOptionLabel, sortPlayersForSelect } from './eventFormHelpers';
import EventTimeInput from '../../../../../components/match/EventTimeInput';

interface PenaltyRecordingFormProps {
  showPenaltyForm: boolean;
  penaltyForm: PenaltyForm;
  setPenaltyForm: React.Dispatch<React.SetStateAction<PenaltyForm>>;
  currentMatch: FloorballMatchDto;
  homeTeam: FloorballTeam | null;
  awayTeam: FloorballTeam | null;
  loading: boolean;
  getPlayersForTeam: (teamId: string) => FloorballPlayerDto[];
  onRecordPenalty: () => Promise<void>;
  onClose: () => void;
}

/** Duration options; the label is resolved via i18n key `penaltyForm.duration{value}`. */
const PENALTY_DURATION_OPTIONS: ReadonlyArray<{ value: number; labelKey: string; fallback: string }> = [
  { value: 2, labelKey: 'floorball.matches.manage.penaltyForm.duration2', fallback: '2 minutes (minor)' },
  { value: 5, labelKey: 'floorball.matches.manage.penaltyForm.duration5', fallback: '5 minutes (major)' },
  { value: 10, labelKey: 'floorball.matches.manage.penaltyForm.duration10', fallback: '10 minutes (misconduct)' },
  { value: 20, labelKey: 'floorball.matches.manage.penaltyForm.duration20', fallback: '20 minutes (game misconduct)' },
];

const PENALTY_TYPE_OPTIONS: ReadonlyArray<{ value: string; labelKey: string; fallback: string }> = [
  { value: 'Minor', labelKey: 'floorball.matches.manage.penaltyForm.severityMinor', fallback: 'Minor' },
  { value: 'Major', labelKey: 'floorball.matches.manage.penaltyForm.severityMajor', fallback: 'Major' },
];

const DESCRIPTION_MAX_LENGTH: number = 280;

const PenaltyRecordingForm = ({
  showPenaltyForm,
  penaltyForm,
  setPenaltyForm,
  currentMatch,
  homeTeam,
  awayTeam,
  loading,
  getPlayersForTeam,
  onRecordPenalty,
  onClose,
}: PenaltyRecordingFormProps) => {
  const { t } = useTranslation();
  const firstFieldRef = useRef<HTMLSelectElement | null>(null);

  const sortedPlayers: FloorballPlayerDto[] = useMemo(
    () => (penaltyForm.teamId ? sortPlayersForSelect(getPlayersForTeam(penaltyForm.teamId)) : []),
    [penaltyForm.teamId, getPlayersForTeam],
  );

  const selectedPlayer: FloorballPlayerDto | undefined = sortedPlayers.find((p) => p.id === penaltyForm.playerId);
  const missingJersey: boolean = !!(penaltyForm.playerId && selectedPlayer?.jerseyNumber === undefined);
  const canSubmit: boolean =
    !!penaltyForm.playerId && !!penaltyForm.penaltyType && penaltyForm.minutes > 0 && !missingJersey && !loading;

  const selectedTeamName: string | undefined =
    penaltyForm.teamId === currentMatch.homeTeamId ? homeTeam?.name : awayTeam?.name;

  useEffect(() => {
    if (showPenaltyForm) {
      firstFieldRef.current?.focus();
    }
  }, [showPenaltyForm]);

  const handleKeyDown = (e: React.KeyboardEvent<HTMLDivElement>): void => {
    if (e.key === 'Escape') {
      e.preventDefault();
      e.stopPropagation();
      onClose();
      return;
    }
    if (e.key === 'Enter') {
      const target = e.target as HTMLElement;
      if (target?.tagName === 'TEXTAREA') return;
      if (canSubmit) {
        e.preventDefault();
        e.stopPropagation();
        void onRecordPenalty();
      }
    }
  };

  if (!showPenaltyForm) return null;

  return (
    <div className="penalty-record-modal-overlay" onClick={onClose} role="presentation">
      <div
        className="penalty-record-modal"
        onClick={(e) => e.stopPropagation()}
        onKeyDown={handleKeyDown}
        role="dialog"
        aria-modal="true"
        aria-labelledby="penalty-record-modal-title"
      >
        <div className="penalty-record-modal__header">
          <h3 id="penalty-record-modal-title">
            {t('floorball.matches.manage.penaltyForm.title', { team: selectedTeamName ?? t('common.team') })}
          </h3>
          <button
            className="penalty-record-modal__close"
            onClick={onClose}
            disabled={loading}
            type="button"
            aria-label={t('common.close')}
          >
            ×
          </button>
        </div>

        <div className="penalty-record-modal__body">
          <div className="event-form penalty-form">
            <div className="form-grid">
              <div className="field">
                <label htmlFor="penalty-player">{t('floorball.matches.manage.penaltyForm.receivingPlayer')}</label>
                <select
                  id="penalty-player"
                  ref={firstFieldRef}
                  className={`select-field${penaltyForm.playerId ? '' : ' is-placeholder'}`}
                  value={penaltyForm.playerId}
                  onChange={(e) => setPenaltyForm((prev) => ({ ...prev, playerId: e.target.value }))}
                >
                  <option value="">{t('floorball.matches.manage.penaltyForm.selectPlayer')}</option>
                  {sortedPlayers.map((player) => (
                    <option key={player.id} value={player.id}>
                      {formatPlayerOptionLabel(player)}
                    </option>
                  ))}
                </select>
              </div>

              <div className="field">
                <label htmlFor="penalty-type">{t('floorball.matches.manage.penaltyForm.severity')}</label>
                <select
                  id="penalty-type"
                  className={`select-field${penaltyForm.penaltyType ? '' : ' is-placeholder'}`}
                  value={penaltyForm.penaltyType}
                  onChange={(e) => setPenaltyForm((prev) => ({ ...prev, penaltyType: e.target.value }))}
                >
                  <option value="">{t('floorball.matches.manage.penaltyForm.selectSeverity')}</option>
                  {PENALTY_TYPE_OPTIONS.map((option) => (
                    <option key={option.value} value={option.value}>
                      {t(option.labelKey, option.fallback)}
                    </option>
                  ))}
                </select>
              </div>

              <div className="field">
                <label htmlFor="penalty-duration">{t('floorball.matches.manage.penaltyForm.duration')}</label>
                <select
                  id="penalty-duration"
                  className={`select-field${penaltyForm.minutes ? '' : ' is-placeholder'}`}
                  value={penaltyForm.minutes || ''}
                  onChange={(e) => setPenaltyForm((prev) => ({ ...prev, minutes: parseInt(e.target.value, 10) || 0 }))}
                >
                  <option value="">{t('floorball.matches.manage.penaltyForm.selectDuration')}</option>
                  {PENALTY_DURATION_OPTIONS.map((option) => (
                    <option key={option.value} value={option.value}>
                      {t(option.labelKey, option.fallback)}
                    </option>
                  ))}
                </select>
              </div>

              <EventTimeInput
                idPrefix="penalty-time"
                label={t('floorball.matches.manage.penaltyForm.time')}
                minutes={penaltyForm.timeMinutes}
                seconds={penaltyForm.timeSeconds}
                onChange={(timeMinutes, timeSeconds) =>
                  setPenaltyForm((prev) => ({ ...prev, timeMinutes, timeSeconds }))
                }
              />
            </div>

            <div className="field field--description">
              <label htmlFor="penalty-description">
                {t('floorball.matches.manage.penaltyForm.description')}{' '}
                <span className="field-hint">{t('floorball.matches.manage.goalForm.optional')}</span>
              </label>
              <textarea
                id="penalty-description"
                value={penaltyForm.description}
                onChange={(e) => setPenaltyForm((prev) => ({ ...prev, description: e.target.value }))}
                placeholder={t('floorball.matches.manage.penaltyForm.descriptionPlaceholder')}
                className="description-input"
                maxLength={DESCRIPTION_MAX_LENGTH}
                rows={3}
              />
              <div className="description-counter" aria-live="polite">
                {penaltyForm.description.length}/{DESCRIPTION_MAX_LENGTH}
              </div>
            </div>

            {missingJersey && (
              <div className="field-error" role="alert">
                {t('floorball.matches.manage.penaltyForm.missingJersey')}
              </div>
            )}

            <div className="form-actions">
              <button onClick={onClose} className="cancel-btn" type="button" disabled={loading}>
                {t('common.cancel')}
              </button>
              <button
                onClick={onRecordPenalty}
                disabled={!canSubmit}
                className="submit-btn"
                type="button"
              >
                {loading
                  ? t('floorball.matches.manage.penaltyForm.recording')
                  : missingJersey
                    ? t('floorball.matches.manage.penaltyForm.missingJerseyShort')
                    : t('floorball.matches.manage.penaltyForm.submit')}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default PenaltyRecordingForm;
