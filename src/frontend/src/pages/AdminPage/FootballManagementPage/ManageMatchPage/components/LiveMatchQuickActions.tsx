import { useTranslation } from 'react-i18next';
import type { FootballMatchDto } from '../../../../../types/football/footballTypes';
import './LiveMatchQuickActions.scss';

interface LiveMatchQuickActionsProps {
  loading: boolean;
  currentMatch: FootballMatchDto;
  leftTeamId?: string;
  rightTeamId?: string;
  leftTeamName?: string;
  rightTeamName?: string;
  onShowGoalForm: (teamId: string) => void;
  onShowCardForm: (teamId: string) => void;
  onShowMissedPenaltyForm: (teamId: string) => void;
}

const LiveMatchQuickActions = ({
  loading,
  currentMatch,
  leftTeamId,
  rightTeamId,
  leftTeamName,
  rightTeamName,
  onShowGoalForm,
  onShowCardForm,
  onShowMissedPenaltyForm,
}: LiveMatchQuickActionsProps) => {
  const { t } = useTranslation();
  const isMatchInProgress: boolean = currentMatch.status === 'InProgress';

  const renderTeamActions = (teamId: string | undefined) => (
    <div className="team-actions">
      <button
        onClick={() => teamId && onShowGoalForm(teamId)}
        className="action-btn goal-btn"
        disabled={loading || !isMatchInProgress || !teamId}
        type="button"
      >
        <span className="btn-label">{t('football.matches.manage.quickActions.recordGoal')}</span>
        <span className="btn-icon" aria-hidden="true">⚽</span>
      </button>
      <button
        onClick={() => teamId && onShowCardForm(teamId)}
        className="action-btn penalty-btn"
        disabled={loading || !isMatchInProgress || !teamId}
        type="button"
      >
        <span className="btn-label">{t('football.matches.manage.quickActions.recordCard')}</span>
        <span className="btn-icon" aria-hidden="true">🟨</span>
      </button>
      <button
        onClick={() => teamId && onShowMissedPenaltyForm(teamId)}
        className="action-btn sub-btn"
        disabled={loading || !isMatchInProgress || !teamId}
        type="button"
      >
        <span className="btn-label">{t('matchManage.missedPenalty.quickAction')}</span>
        <span className="btn-icon" aria-hidden="true">❌</span>
      </button>
    </div>
  );

  return (
    <div className="quick-actions-grid">
      <h3 className="qa-title">{t('football.matches.manage.quickActions.title')}</h3>
      <h4 className="team-name left">{leftTeamName || t('matchManage.scoreboard.home')}</h4>
      <h4 className="team-name right">{rightTeamName || t('matchManage.scoreboard.away')}</h4>
      {renderTeamActions(leftTeamId)}
      {renderTeamActions(rightTeamId)}
    </div>
  );
};

export default LiveMatchQuickActions;
