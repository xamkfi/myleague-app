import type { FloorballMatchDto } from '../../../../../types/floorball/floorballTypes';
import './LiveMatchQuickActions.scss';

interface LiveMatchQuickActionsProps {
  loading: boolean;
  currentMatch: FloorballMatchDto;
  leftTeamId?: string;
  rightTeamId?: string;
  leftTeamName?: string;
  rightTeamName?: string;
  leftTeamSide: 'home' | 'away';
  rightTeamSide: 'home' | 'away';
  onShowGoalForm: (teamId: string) => void;
  onShowPenaltyForm: (teamId: string) => void;
  // Save recording controls
  leftGoalieId?: string;
  rightGoalieId?: string;
  onRecordSave?: (team: 'home' | 'away', goalieId: string) => void;
  keybindsEnabled?: boolean;
  saveLoading?: boolean;
}

const LiveMatchQuickActions = ({
  loading,
  currentMatch,
  leftTeamId,
  rightTeamId,
  leftTeamName,
  rightTeamName,
  leftTeamSide,
  rightTeamSide,
  onShowGoalForm,
  onShowPenaltyForm,
  leftGoalieId,
  rightGoalieId,
  onRecordSave,
  keybindsEnabled,
  saveLoading
}: LiveMatchQuickActionsProps) => {
  const isMatchInProgress = currentMatch.status === 'InProgress';

  return (
    <div className="quick-actions-grid">
      <h3 className="qa-title">RECORD EVENT</h3>
      <h4 className="team-name left">{leftTeamName || 'Left Team'}</h4>
      <h4 className="team-name right">{rightTeamName || 'Right Team'}</h4>
      <div className="team-actions">
        {onRecordSave && (
          <button
            onClick={() => leftGoalieId && onRecordSave(leftTeamSide, leftGoalieId)}
            className="action-btn save-btn"
            disabled={Boolean(saveLoading) || !isMatchInProgress || !leftGoalieId}
            title={!leftGoalieId ? 'Select goalie to enable' : undefined}
          >
            <span className="btn-label">Record Save</span>
            <span className="btn-meta">
              <span className={`btn-key ${keybindsEnabled ? '' : 'disabled'}`}>(Q)</span>
              <span className="btn-icon" aria-hidden="true">🛡️</span>
            </span>
          </button>
        )}
        <button 
          onClick={() => leftTeamId && onShowGoalForm(leftTeamId)} 
          className="action-btn goal-btn"
          disabled={loading || !isMatchInProgress || !leftTeamId}
        >
          <span className="btn-label">Record Goal</span>
          <span className="btn-icon" aria-hidden="true">⚽</span>
        </button>
        <button 
          onClick={() => leftTeamId && onShowPenaltyForm(leftTeamId)} 
          className="action-btn penalty-btn"
          disabled={loading || !isMatchInProgress || !leftTeamId}
        >
          <span className="btn-label">Record Penalty</span>
          <span className="btn-icon" aria-hidden="true">🟧</span>
        </button>
        
      </div>
      <div className="team-actions">
        {onRecordSave && (
          <button
            onClick={() => rightGoalieId && onRecordSave(rightTeamSide, rightGoalieId)}
            className="action-btn save-btn"
            disabled={Boolean(saveLoading) || !isMatchInProgress || !rightGoalieId}
            title={!rightGoalieId ? 'Select goalie to enable' : undefined}
          >
            <span className="btn-label">Record Save</span>
            <span className="btn-meta">
              <span className={`btn-key ${keybindsEnabled ? '' : 'disabled'}`}>(R)</span>
              <span className="btn-icon" aria-hidden="true">🛡️</span>
            </span>
          </button>
        )}
        <button 
          onClick={() => rightTeamId && onShowGoalForm(rightTeamId)} 
          className="action-btn goal-btn"
          disabled={loading || !isMatchInProgress || !rightTeamId}
        >
          <span className="btn-label">Record Goal</span>
          <span className="btn-icon" aria-hidden="true">⚽</span>
        </button>
        <button 
          onClick={() => rightTeamId && onShowPenaltyForm(rightTeamId)} 
          className="action-btn penalty-btn"
          disabled={loading || !isMatchInProgress || !rightTeamId}
        >
          <span className="btn-label">Record Penalty</span>
          <span className="btn-icon" aria-hidden="true">🟧</span>
        </button>
        
      </div>
    </div>
  );
};

export default LiveMatchQuickActions; 