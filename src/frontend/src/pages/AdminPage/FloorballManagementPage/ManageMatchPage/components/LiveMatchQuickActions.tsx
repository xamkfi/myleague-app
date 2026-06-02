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
  /**
   * Opens the bulk save dialog for the given side. Used for the "backfill missed saves"
   * recovery flow when the recorder forgot to mark individual saves during the period.
   * Optional so consumers that don't yet support bulk entry can omit it.
   */
  onShowBulkSave?: (team: 'home' | 'away', goalieId: string) => void;
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
  onShowBulkSave,
  keybindsEnabled,
  saveLoading
}: LiveMatchQuickActionsProps) => {
  const isMatchInProgress: boolean = currentMatch.status === 'InProgress';

  const renderTeamActions = (
    side: 'left' | 'right',
    teamId: string | undefined,
    goalieId: string | undefined,
    teamSide: 'home' | 'away',
    saveKeyLabel: string,
  ) => (
    <div className="team-actions">
      {onRecordSave && (
        <div className="save-action-group">
          <button
            onClick={() => goalieId && onRecordSave(teamSide, goalieId)}
            className="action-btn save-btn"
            disabled={Boolean(saveLoading) || !isMatchInProgress || !goalieId}
            title={!goalieId ? 'Select goalie to enable' : undefined}
            type="button"
          >
            <span className="btn-label">Record Save</span>
            <span className="btn-meta">
              <span className={`btn-key ${keybindsEnabled ? '' : 'disabled'}`}>({saveKeyLabel})</span>
              <span className="btn-icon" aria-hidden="true">🛡️</span>
            </span>
          </button>
          {onShowBulkSave && (
            <button
              onClick={() => goalieId && onShowBulkSave(teamSide, goalieId)}
              className="bulk-save-btn"
              disabled={Boolean(saveLoading) || !isMatchInProgress || !goalieId}
              title={!goalieId ? 'Select goalie to enable bulk save entry' : 'Bulk record saves'}
              aria-label={`Bulk record saves for ${side === 'left' ? leftTeamName ?? 'left team' : rightTeamName ?? 'right team'}`}
              type="button"
            >
              +N
            </button>
          )}
        </div>
      )}
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
        onClick={() => teamId && onShowPenaltyForm(teamId)}
        className="action-btn penalty-btn"
        disabled={loading || !isMatchInProgress || !teamId}
        type="button"
      >
        <span className="btn-label">Record Penalty</span>
        <span className="btn-icon" aria-hidden="true">🟧</span>
      </button>
    </div>
  );

  return (
    <div className="quick-actions-grid">
      <h3 className="qa-title">RECORD EVENT</h3>
      <h4 className="team-name left">{leftTeamName || 'Left Team'}</h4>
      <h4 className="team-name right">{rightTeamName || 'Right Team'}</h4>
      {renderTeamActions('left', leftTeamId, leftGoalieId, leftTeamSide, 'Q')}
      {renderTeamActions('right', rightTeamId, rightGoalieId, rightTeamSide, 'R')}
    </div>
  );
};

export default LiveMatchQuickActions; 