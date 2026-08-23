import { useEffect, useMemo, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import { HOCKEY_GOAL_STRENGTHS, type HockeyGoalStrength } from '../../../../../types/hockey/hockeyTypes';
import './GoalRecordingForm.scss';
import { formatPlayerOptionLabel, sortPlayersForSelect, type HockeyFormPlayer } from './eventFormHelpers';

interface GoalRecordingFormProps {
  showGoalForm: boolean;
  teamName: string;
  players: HockeyFormPlayer[];
  playerId: string;
  assistId: string;
  goalStrength: HockeyGoalStrength;
  loading: boolean;
  onPlayerChange: (playerId: string) => void;
  onAssistChange: (assistId: string) => void;
  onStrengthChange: (strength: HockeyGoalStrength) => void;
  onRecordGoal: () => Promise<void>;
  onClose: () => void;
}

function GoalRecordingForm({
  showGoalForm,
  teamName,
  players,
  playerId,
  assistId,
  goalStrength,
  loading,
  onPlayerChange,
  onAssistChange,
  onStrengthChange,
  onRecordGoal,
  onClose,
}: GoalRecordingFormProps) {
  const { t } = useTranslation();
  const firstFieldRef = useRef<HTMLSelectElement | null>(null);
  const sortedPlayers = useMemo(() => sortPlayersForSelect(players), [players]);
  const canSubmit = Boolean(playerId) && !loading;

  useEffect(() => {
    if (showGoalForm) {
      firstFieldRef.current?.focus();
    }
  }, [showGoalForm]);

  const handleKeyDown = (event: React.KeyboardEvent<HTMLDivElement>): void => {
    if (event.key === 'Escape') {
      event.preventDefault();
      event.stopPropagation();
      onClose();
      return;
    }
    if (event.key === 'Enter') {
      const target = event.target as HTMLElement;
      if (target?.tagName === 'TEXTAREA') {
        return;
      }
      if (canSubmit) {
        event.preventDefault();
        event.stopPropagation();
        void onRecordGoal();
      }
    }
  };

  if (!showGoalForm) {
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
        aria-labelledby="goal-record-modal-title"
      >
        <div className="goal-record-modal__header">
          <h3 id="goal-record-modal-title">
            {t('hockey.matches.recordGoalFor', 'Record goal for {{team}}', { team: teamName || t('hockey.matches.team', 'team') })}
          </h3>
          <button className="goal-record-modal__close" onClick={onClose} disabled={loading} type="button" aria-label={t('common.close', 'Close')}>×</button>
        </div>
        <div className="goal-record-modal__body">
          <div className="event-form goal-form">
            <div className="form-grid">
              <div className="field">
                <label htmlFor="scoring-player">{t('hockey.matches.scoringPlayer', 'Scoring player')}</label>
                <select
                  id="scoring-player"
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
                <label htmlFor="assisting-player">
                  {t('hockey.matches.assistingPlayer', 'Assisting player')}{' '}
                  <span className="field-hint">({t('hockey.matches.optional', 'optional')})</span>
                </label>
                <select
                  id="assisting-player"
                  className={`select-field${assistId ? '' : ' is-placeholder'}`}
                  value={assistId}
                  onChange={(event) => onAssistChange(event.target.value)}
                >
                  <option value="">{t('hockey.matches.noAssist', 'No assist')}</option>
                  {sortedPlayers.filter((player) => player.id !== playerId).map((player) => (
                    <option key={player.id} value={player.id}>{formatPlayerOptionLabel(player)}</option>
                  ))}
                </select>
              </div>
              <div className="field">
                <label htmlFor="goal-strength">{t('hockey.matches.strength', 'Strength')}</label>
                <select
                  id="goal-strength"
                  className="select-field"
                  value={goalStrength}
                  onChange={(event) => onStrengthChange(event.target.value as HockeyGoalStrength)}
                >
                  {HOCKEY_GOAL_STRENGTHS.map((item) => (
                    <option key={item} value={item}>{t(`hockey.matches.goalStrengths.${item}`, item)}</option>
                  ))}
                </select>
              </div>
            </div>
            <div className="form-actions">
              <button onClick={onClose} className="cancel-btn" type="button" disabled={loading}>{t('common.cancel', 'Cancel')}</button>
              <button onClick={() => void onRecordGoal()} disabled={!canSubmit} className="submit-btn" type="button">
                {loading ? t('hockey.matches.recording', 'Recording…') : t('hockey.matches.goal', 'Record Goal')}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default GoalRecordingForm;
