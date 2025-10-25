import type { FloorballMatchDto } from '../../../../../types/floorball/floorballTypes';
import './LiveMatchQuickActions.scss';

interface LiveMatchQuickActionsProps {
  loading: boolean;
  currentMatch: FloorballMatchDto;
  homeTeamId?: string;
  awayTeamId?: string;
  onShowGoalForm: (teamId: string) => void;
  onShowPenaltyForm: (teamId: string) => void;
}

const LiveMatchQuickActions = ({
  loading,
  currentMatch,
  homeTeamId,
  awayTeamId,
  onShowGoalForm,
  onShowPenaltyForm
}: LiveMatchQuickActionsProps) => {
  const isMatchInProgress = currentMatch.status === 'InProgress';

  return (
    <div className="quick-actions-grid">
      <div className="team-actions">
        <button 
          onClick={() => homeTeamId && onShowGoalForm(homeTeamId)} 
          className="action-btn goal-btn"
          disabled={loading || !isMatchInProgress || !homeTeamId}
        >
          ⚽ Record Home Goal
        </button>
        <button 
          onClick={() => homeTeamId && onShowPenaltyForm(homeTeamId)} 
          className="action-btn penalty-btn"
          disabled={loading || !isMatchInProgress || !homeTeamId}
        >
          🟨 Record Home Penalty
        </button>
      </div>
      <div className="team-actions">
        <button 
          onClick={() => awayTeamId && onShowGoalForm(awayTeamId)} 
          className="action-btn goal-btn"
          disabled={loading || !isMatchInProgress || !awayTeamId}
        >
          ⚽ Record Away Goal
        </button>
        <button 
          onClick={() => awayTeamId && onShowPenaltyForm(awayTeamId)} 
          className="action-btn penalty-btn"
          disabled={loading || !isMatchInProgress || !awayTeamId}
        >
          🟨 Record Away Penalty
        </button>
      </div>
    </div>
  );
};

export default LiveMatchQuickActions; 