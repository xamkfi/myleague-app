import { useEffect, useMemo, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import {
  HOCKEY_PENALTY_OFFENCES,
  HOCKEY_PENALTY_SEVERITIES,
  type HockeyPenaltyOffence,
  type HockeyPenaltySeverity,
} from '../../../../../types/hockey/hockeyTypes';
import './PenaltyRecordingForm.scss';
import { formatPlayerOptionLabel, sortPlayersForSelect, type HockeyFormPlayer } from './eventFormHelpers';

interface PenaltyRecordingFormProps {
  showPenaltyForm: boolean;
  teamName: string;
  players: HockeyFormPlayer[];
  playerId: string;
  penaltyOffence: HockeyPenaltyOffence;
  penaltySeverity: HockeyPenaltySeverity;
  penaltyMinutes: number;
  loading: boolean;
  onPlayerChange: (playerId: string) => void;
  onOffenceChange: (offence: HockeyPenaltyOffence) => void;
  onSeverityChange: (severity: HockeyPenaltySeverity) => void;
  onMinutesChange: (minutes: number) => void;
  onRecordPenalty: () => Promise<void>;
  onClose: () => void;
}

function PenaltyRecordingForm({
  showPenaltyForm,
  teamName,
  players,
  playerId,
  penaltyOffence,
  penaltySeverity,
  penaltyMinutes,
  loading,
  onPlayerChange,
  onOffenceChange,
  onSeverityChange,
  onMinutesChange,
  onRecordPenalty,
  onClose,
}: PenaltyRecordingFormProps) {
  const { t } = useTranslation();
  const firstFieldRef = useRef<HTMLSelectElement | null>(null);
  const sortedPlayers = useMemo(() => sortPlayersForSelect(players), [players]);
  const canSubmit = Boolean(playerId) && penaltyMinutes > 0 && !loading;

  useEffect(() => {
    if (showPenaltyForm) {
      firstFieldRef.current?.focus();
    }
  }, [showPenaltyForm]);

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
      void onRecordPenalty();
    }
  };

  if (!showPenaltyForm) {
    return null;
  }

  return (
    <div className="penalty-record-modal-overlay" onClick={onClose} role="presentation">
      <div
        className="penalty-record-modal"
        onClick={(event) => event.stopPropagation()}
        onKeyDown={handleKeyDown}
        role="dialog"
        aria-modal="true"
        aria-labelledby="penalty-record-modal-title"
      >
        <div className="penalty-record-modal__header">
          <h3 id="penalty-record-modal-title">
            {t('hockey.matches.recordPenaltyFor', 'Record penalty for {{team}}', { team: teamName || t('hockey.matches.team', 'team') })}
          </h3>
          <button className="penalty-record-modal__close" onClick={onClose} disabled={loading} type="button" aria-label={t('common.close', 'Close')}>×</button>
        </div>
        <div className="penalty-record-modal__body">
          <div className="event-form penalty-form">
            <div className="form-grid">
              <div className="field">
                <label htmlFor="penalized-player">{t('hockey.matches.penalizedPlayer', 'Penalized player')}</label>
                <select
                  id="penalized-player"
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
                <label htmlFor="penalty-offence">{t('hockey.matches.offence', 'Offence')}</label>
                <select id="penalty-offence" className="select-field" value={penaltyOffence} onChange={(event) => onOffenceChange(event.target.value as HockeyPenaltyOffence)}>
                  {HOCKEY_PENALTY_OFFENCES.map((item) => (
                    <option key={item} value={item}>{t(`hockey.matches.penaltyOffences.${item}`, item)}</option>
                  ))}
                </select>
              </div>
              <div className="field">
                <label htmlFor="penalty-severity">{t('hockey.matches.severity', 'Severity')}</label>
                <select id="penalty-severity" className="select-field" value={penaltySeverity} onChange={(event) => onSeverityChange(event.target.value as HockeyPenaltySeverity)}>
                  {HOCKEY_PENALTY_SEVERITIES.map((item) => (
                    <option key={item} value={item}>{t(`hockey.matches.penaltySeverities.${item}`, item)}</option>
                  ))}
                </select>
              </div>
              <div className="field">
                <label htmlFor="penalty-minutes">{t('hockey.matches.minutes', 'Minutes')}</label>
                <select id="penalty-minutes" className="select-field" value={penaltyMinutes} onChange={(event) => onMinutesChange(Number(event.target.value))}>
                  {[2, 4, 5, 10].map((minutes) => <option key={minutes} value={minutes}>{minutes}</option>)}
                </select>
              </div>
            </div>
            <div className="form-actions">
              <button onClick={onClose} className="cancel-btn" type="button" disabled={loading}>{t('common.cancel', 'Cancel')}</button>
              <button onClick={() => void onRecordPenalty()} disabled={!canSubmit} className="submit-btn" type="button">
                {loading ? t('hockey.matches.recording', 'Recording…') : t('hockey.matches.penalty', 'Record Penalty')}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default PenaltyRecordingForm;
