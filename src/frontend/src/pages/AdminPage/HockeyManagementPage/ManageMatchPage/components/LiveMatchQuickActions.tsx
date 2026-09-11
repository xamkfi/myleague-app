import { useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import type { HockeyMatchDto } from '../../../../../types/hockey/hockeyTypes';
import { isHockeyMatchLive } from '../../../../../types/hockey/hockeyTypes';
import './LiveMatchQuickActions.scss';

interface LiveMatchQuickActionsProps {
  loading: boolean;
  currentMatch: HockeyMatchDto;
  leftTeamId?: string;
  rightTeamId?: string;
  leftTeamName?: string;
  rightTeamName?: string;
  onShowGoalForm: (teamId: string) => void;
  onShowPenaltyForm: (teamId: string) => void;
  onShowShotForm: (teamId: string) => void;
  onShowFaceoffForm: () => void;
  onRecordOffside: () => void;
  keybindsEnabled?: boolean;
}

function LiveMatchQuickActions({
  loading,
  currentMatch,
  leftTeamId,
  rightTeamId,
  leftTeamName,
  rightTeamName,
  onShowGoalForm,
  onShowPenaltyForm,
  onShowShotForm,
  onShowFaceoffForm,
  onRecordOffside,
  keybindsEnabled,
}: LiveMatchQuickActionsProps) {
  const { t } = useTranslation();
  const canRecord = isHockeyMatchLive(currentMatch.status);

  useEffect(() => {
    if (!keybindsEnabled || !canRecord) {
      return undefined;
    }
    const handleKey = (event: KeyboardEvent): void => {
      const target = event.target as HTMLElement | null;
      if (target && (target.tagName === 'INPUT' || target.tagName === 'TEXTAREA' || target.tagName === 'SELECT' || target.isContentEditable)) {
        return;
      }
      if (event.key === 'q' || event.key === 'Q') {
        if (leftTeamId) {
          event.preventDefault();
          onShowShotForm(leftTeamId);
        }
      }
      if (event.key === 'r' || event.key === 'R') {
        if (rightTeamId) {
          event.preventDefault();
          onShowShotForm(rightTeamId);
        }
      }
      if (event.key === 'o' || event.key === 'O') {
        event.preventDefault();
        onRecordOffside();
      }
      if (event.key === 'f' || event.key === 'F') {
        event.preventDefault();
        onShowFaceoffForm();
      }
    };
    window.addEventListener('keydown', handleKey);
    return () => window.removeEventListener('keydown', handleKey);
  }, [keybindsEnabled, canRecord, leftTeamId, rightTeamId, onShowShotForm, onRecordOffside, onShowFaceoffForm]);

  const renderTeamActions = (teamId: string | undefined, shotKeyLabel: string) => (
    <div className="team-actions">
      <button
        type="button"
        onClick={() => teamId && onShowShotForm(teamId)}
        className="action-btn save-btn"
        disabled={loading || !canRecord || !teamId}
      >
        <span className="btn-label">{t('hockey.matches.shot', 'Record Shot')}</span>
        <span className="btn-meta">
          <span className={`btn-key ${keybindsEnabled ? '' : 'disabled'}`}>({shotKeyLabel})</span>
          <span className="btn-icon" aria-hidden="true">🏒</span>
        </span>
      </button>
      <button
        type="button"
        onClick={() => teamId && onShowGoalForm(teamId)}
        className="action-btn goal-btn"
        disabled={loading || !canRecord || !teamId}
      >
        <span className="btn-label">{t('hockey.matches.goal', 'Record Goal')}</span>
        <span className="btn-icon" aria-hidden="true">🥅</span>
      </button>
      <button
        type="button"
        onClick={() => teamId && onShowPenaltyForm(teamId)}
        className="action-btn penalty-btn"
        disabled={loading || !canRecord || !teamId}
      >
        <span className="btn-label">{t('hockey.matches.penalty', 'Record Penalty')}</span>
        <span className="btn-icon" aria-hidden="true">🟧</span>
      </button>
    </div>
  );

  return (
    <div className="quick-actions-grid">
      <h3 className="qa-title">{t('hockey.matches.recordEvent', 'RECORD EVENT')}</h3>
      <h4 className="team-name left">{leftTeamName || t('hockey.matches.home', 'Home')}</h4>
      <h4 className="team-name right">{rightTeamName || t('hockey.matches.away', 'Away')}</h4>
      {renderTeamActions(leftTeamId, 'Q')}
      {renderTeamActions(rightTeamId, 'R')}
      <div className="shared-actions">
        <button
          type="button"
          onClick={onShowFaceoffForm}
          className="action-btn faceoff-btn"
          disabled={loading || !canRecord}
        >
          <span className="btn-label">{t('hockey.matches.faceoff', 'Face-off')}</span>
          <span className="btn-meta">
            <span className={`btn-key ${keybindsEnabled ? '' : 'disabled'}`}>(F)</span>
            <span className="btn-icon" aria-hidden="true">🏒</span>
          </span>
        </button>
        <button
          type="button"
          onClick={onRecordOffside}
          className="action-btn offside-btn"
          disabled={loading || !canRecord}
        >
          <span className="btn-label">{t('hockey.matches.offside', 'Offside')}</span>
          <span className="btn-meta">
            <span className={`btn-key ${keybindsEnabled ? '' : 'disabled'}`}>(O)</span>
            <span className="btn-icon" aria-hidden="true">🛑</span>
          </span>
        </button>
      </div>
    </div>
  );
}

export default LiveMatchQuickActions;
