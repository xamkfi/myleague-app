import type { FloorballMatchDto } from '../../../../../types/floorball/floorballTypes';
import './LiveMatchQuickActions.scss';

interface LiveMatchQuickActionsProps {
  loading: boolean;
  currentMatch: FloorballMatchDto;
  homeTeamId?: string;
  awayTeamId?: string;
  homeTeamName?: string;
  awayTeamName?: string;
  onShowGoalForm: (teamId: string) => void;
  onShowPenaltyForm: (teamId: string) => void;
  // Save recording controls
  homeGoalieId?: string;
  awayGoalieId?: string;
  onRecordSave?: (team: 'home' | 'away', goalieId: string) => void;
  keybindsEnabled?: boolean;
  saveLoading?: boolean;
}

const LiveMatchQuickActions = ({
  loading,
  currentMatch,
  homeTeamId,
  awayTeamId,
  homeTeamName,
  awayTeamName,
  onShowGoalForm,
  onShowPenaltyForm,
  homeGoalieId,
  awayGoalieId,
  onRecordSave,
  keybindsEnabled,
  saveLoading
}: LiveMatchQuickActionsProps) => {
  const isMatchInProgress = currentMatch.status === 'InProgress';

  return (
    <div className="quick-actions-grid">
      <h3 className="qa-title">RECORD EVENT</h3>
      <h4 className="team-name home">{homeTeamName || 'Home Team'}</h4>
      <h4 className="team-name away">{awayTeamName || 'Away Team'}</h4>
      <div className="team-actions">
        {onRecordSave && (
          <button
            onClick={() => homeGoalieId && onRecordSave('home', homeGoalieId)}
            className="action-btn save-btn"
            disabled={Boolean(saveLoading) || !isMatchInProgress || !homeGoalieId}
            title={!homeGoalieId ? 'Select home goalie to enable' : undefined}
          >
            <span className="btn-label">Record Home Save</span>
            <span className="btn-meta">
              <span className={`btn-key ${keybindsEnabled ? '' : 'disabled'}`}>(Q)</span>
              <span className="btn-icon" aria-hidden="true">🛡️</span>
            </span>
          </button>
        )}
        <button 
          onClick={() => homeTeamId && onShowGoalForm(homeTeamId)} 
          className="action-btn goal-btn"
          disabled={loading || !isMatchInProgress || !homeTeamId}
        >
          <span className="btn-label">Record Home Goal</span>
          <span className="btn-icon" aria-hidden="true">⚽</span>
        </button>
        <button 
          onClick={() => homeTeamId && onShowPenaltyForm(homeTeamId)} 
          className="action-btn penalty-btn"
          disabled={loading || !isMatchInProgress || !homeTeamId}
        >
          <span className="btn-label">Record Home Penalty</span>
          <span className="btn-icon" aria-hidden="true">🟧</span>
        </button>
        
      </div>
      <div className="team-actions">
        {onRecordSave && (
          <button
            onClick={() => awayGoalieId && onRecordSave('away', awayGoalieId)}
            className="action-btn save-btn"
            disabled={Boolean(saveLoading) || !isMatchInProgress || !awayGoalieId}
            title={!awayGoalieId ? 'Select away goalie to enable' : undefined}
          >
            <span className="btn-label">Record Away Save</span>
            <span className="btn-meta">
              <span className={`btn-key ${keybindsEnabled ? '' : 'disabled'}`}>(P)</span>
              <span className="btn-icon" aria-hidden="true">🛡️</span>
            </span>
          </button>
        )}
        <button 
          onClick={() => awayTeamId && onShowGoalForm(awayTeamId)} 
          className="action-btn goal-btn"
          disabled={loading || !isMatchInProgress || !awayTeamId}
        >
          <span className="btn-label">Record Away Goal</span>
          <span className="btn-icon" aria-hidden="true">⚽</span>
        </button>
        <button 
          onClick={() => awayTeamId && onShowPenaltyForm(awayTeamId)} 
          className="action-btn penalty-btn"
          disabled={loading || !isMatchInProgress || !awayTeamId}
        >
          <span className="btn-label">Record Away Penalty</span>
          <span className="btn-icon" aria-hidden="true">🟧</span>
        </button>
        
      </div>
    </div>
  );
};

export default LiveMatchQuickActions; 