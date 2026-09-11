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
  onShowSubstitutionForm: (teamId: string) => void;
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
  onShowSubstitutionForm,
}: LiveMatchQuickActionsProps) => {
  const isMatchInProgress: boolean = currentMatch.status === 'InProgress';

  const renderTeamActions = (teamId: string | undefined) => (
    <div className="team-actions">
      <button
        onClick={() => teamId && onShowGoalForm(teamId)}
        className="action-btn goal-btn"
        disabled={loading || !isMatchInProgress || !teamId}
        type="button"
      >
        <span className="btn-label">Record Goal</span>
        <span className="btn-icon" aria-hidden="true">⚽</span>
      </button>
      <button
        onClick={() => teamId && onShowCardForm(teamId)}
        className="action-btn penalty-btn"
        disabled={loading || !isMatchInProgress || !teamId}
        type="button"
      >
        <span className="btn-label">Record Card</span>
        <span className="btn-icon" aria-hidden="true">🟨</span>
      </button>
      <button
        onClick={() => teamId && onShowSubstitutionForm(teamId)}
        className="action-btn sub-btn"
        disabled={loading || !isMatchInProgress || !teamId}
        type="button"
      >
        <span className="btn-label">Record Sub</span>
        <span className="btn-icon" aria-hidden="true">🔄</span>
      </button>
    </div>
  );

  return (
    <div className="quick-actions-grid">
      <h3 className="qa-title">RECORD EVENT</h3>
      <h4 className="team-name left">{leftTeamName || 'Left Team'}</h4>
      <h4 className="team-name right">{rightTeamName || 'Right Team'}</h4>
      {renderTeamActions(leftTeamId)}
      {renderTeamActions(rightTeamId)}
    </div>
  );
};

export default LiveMatchQuickActions;
