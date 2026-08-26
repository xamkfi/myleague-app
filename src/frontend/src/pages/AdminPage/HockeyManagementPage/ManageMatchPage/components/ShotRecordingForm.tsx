import { useEffect, useMemo, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import {
  HOCKEY_SHOT_RESULTS,
  hockeyShotCreditsGoalieSave,
  type HockeyShotResult,
} from '../../../../../types/hockey/hockeyTypes';

const SHOT_RESULTS_FOR_FORM = HOCKEY_SHOT_RESULTS.filter((result) => result !== 'Goal');
import './ShotRecordingForm.scss';
import { formatPlayerOptionLabel, sortPlayersForSelect, type HockeyFormPlayer } from './eventFormHelpers';

interface ShotRecordingFormProps {
  showShotForm: boolean;
  teamName: string;
  players: HockeyFormPlayer[];
  playerId: string;
  shotResult: HockeyShotResult;
  loading: boolean;
  onPlayerChange: (playerId: string) => void;
  onResultChange: (result: HockeyShotResult) => void;
  onRecordShot: () => Promise<void>;
  onClose: () => void;
}

function ShotRecordingForm({
  showShotForm,
  teamName,
  players,
  playerId,
  shotResult,
  loading,
  onPlayerChange,
  onResultChange,
  onRecordShot,
  onClose,
}: ShotRecordingFormProps) {
  const { t } = useTranslation();
  const firstFieldRef = useRef<HTMLSelectElement | null>(null);
  const sortedPlayers = useMemo(() => sortPlayersForSelect(players), [players]);
  const canSubmit = !loading;

  useEffect(() => {
    if (showShotForm) {
      firstFieldRef.current?.focus();
    }
  }, [showShotForm]);

  useEffect(() => {
    if (showShotForm && shotResult === 'Goal') {
      onResultChange('Saved');
    }
  }, [showShotForm, shotResult, onResultChange]);

  const handleKeyDown = (event: React.KeyboardEvent<HTMLDivElement>): void => {
    if (event.key === 'Escape') {
      event.preventDefault();
      event.stopPropagation();
      onClose();
      return;
    }
    if (event.key === 'Enter' && canSubmit) {
      event.preventDefault();
      event.stopPropagation();
      void onRecordShot();
    }
  };

  if (!showShotForm) {
    return null;
  }

  return (
    <div className="goal-record-modal-overlay" onClick={onClose} role="presentation">
      <div
        className="goal-record-modal"
        onClick={(event) => event.stopPropagation()}
        onKeyDown={handleKeyDown}
        role="dialog"
        aria-modal="true"
        aria-labelledby="shot-record-modal-title"
      >
        <div className="goal-record-modal__header">
          <h3 id="shot-record-modal-title">
            {t('hockey.matches.recordShotFor', 'Record shot for {{team}}', { team: teamName || t('hockey.matches.team', 'team') })}
          </h3>
          <button className="goal-record-modal__close" onClick={onClose} disabled={loading} type="button" aria-label={t('common.close', 'Close')}>×</button>
        </div>
        <div className="goal-record-modal__body">
          <div className="event-form goal-form">
            <div className="form-grid">
              <div className="field">
                <label htmlFor="shooter">
                  {t('hockey.matches.shooter', 'Shooter')}{' '}
                  <span className="field-hint">({t('common.optional', 'optional')})</span>
                </label>
                <select
                  id="shooter"
                  ref={firstFieldRef}
                  className={`select-field${playerId ? '' : ' is-placeholder'}`}
                  value={playerId}
                  onChange={(event) => onPlayerChange(event.target.value)}
                >
                  <option value="">{t('hockey.matches.selectPlayer', 'Select player')}</option>
                  {sortedPlayers.map((player) => (
                    <option key={player.id} value={player.id}>{formatPlayerOptionLabel(player)}</option>
                  ))}
                </select>
              </div>
              <div className="field">
                <label htmlFor="shot-result">{t('hockey.matches.shotResult', 'Shot result')}</label>
                <select
                  id="shot-result"
                  className="select-field"
                  value={shotResult}
                  onChange={(event) => onResultChange(event.target.value as HockeyShotResult)}
                >
                  {SHOT_RESULTS_FOR_FORM.map((item) => (
                    <option key={item} value={item}>
                      {t(`hockey.matches.shotResults.${item}`, item)}
                    </option>
                  ))}
                </select>
                {hockeyShotCreditsGoalieSave(shotResult) && (
                  <p className="field-hint">
                    {t('hockey.matches.shotSaveHint', 'This shot on goal is also recorded as a save for the opposing goalie.')}
                  </p>
                )}
              </div>
            </div>
            <div className="form-actions">
              <button onClick={onClose} className="cancel-btn" type="button" disabled={loading}>
                {t('common.cancel', 'Cancel')}
              </button>
              <button onClick={() => void onRecordShot()} disabled={!canSubmit} className="submit-btn" type="button">
                {loading ? t('hockey.matches.recording', 'Recording…') : t('hockey.matches.shot', 'Record Shot')}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default ShotRecordingForm;
