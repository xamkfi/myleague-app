import { useTranslation } from 'react-i18next';
import type { FloorballMatchDto } from '../../../../../types/floorball/floorballTypes';
import './LiveMatchQuickActions.scss';

type TeamSide = 'home' | 'away';

interface LiveMatchQuickActionsProps {
  loading: boolean;
  currentMatch: FloorballMatchDto;
  leftTeamId?: string;
  rightTeamId?: string;
  leftTeamName?: string;
  rightTeamName?: string;
  leftTeamSide: TeamSide;
  rightTeamSide: TeamSide;
  onShowGoalForm: (teamId: string) => void;
  onShowPenaltyForm: (teamId: string) => void;
  // Save recording controls
  leftGoalieId?: string;
  rightGoalieId?: string;
  onRecordSave?: (team: TeamSide, goalieId: string) => void;
  /**
   * Opens the bulk save dialog for the given side. Used for the "backfill missed saves"
   * recovery flow when the recorder forgot to mark individual saves during the period.
   */
  onShowBulkSave?: (team: TeamSide, goalieId: string) => void;
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
  const { t } = useTranslation();
  const isMatchInProgress: boolean = currentMatch.status === 'InProgress';

  const renderTeamActions = (
    teamId: string | undefined,
    teamName: string | undefined,
    goalieId: string | undefined,
    teamSide: TeamSide,
    saveKeyLabel: string,
  ) => (
    <div className="team-actions">
      {onRecordSave && (
        <div className="save-action-group">
          <button
            onClick={() => goalieId && onRecordSave(teamSide, goalieId)}
            className="action-btn save-btn"
            disabled={Boolean(saveLoading) || !isMatchInProgress || !goalieId}
            title={!goalieId ? t('floorball.matches.manage.quickActions.selectGoalieToEnable', 'Select a goalie to enable') : undefined}
            type="button"
          >
            <span className="btn-label">{t('floorball.matches.manage.quickActions.recordSave', 'Record save')}</span>
            <span className="btn-meta">
              <span className={`btn-key ${keybindsEnabled ? '' : 'disabled'}`}>{saveKeyLabel}</span>
              <i className="fas fa-shield-alt btn-icon" aria-hidden="true"></i>
            </span>
          </button>
          {onShowBulkSave && (
            <button
              onClick={() => goalieId && onShowBulkSave(teamSide, goalieId)}
              className="bulk-save-btn"
              disabled={Boolean(saveLoading) || !isMatchInProgress || !goalieId}
              title={
                !goalieId
                  ? t('floorball.matches.manage.quickActions.selectGoalieForBulk', 'Select a goalie to enable bulk save entry')
                  : t('floorball.matches.manage.quickActions.bulkSaves', 'Record several saves')
              }
              aria-label={t('floorball.matches.manage.quickActions.bulkSavesFor', {
                team: teamName ?? t('floorball.matches.manage.quickActions.team', 'team'),
                defaultValue: 'Record several saves for {{team}}',
              })}
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
        <span className="btn-label">{t('floorball.matches.manage.quickActions.recordGoal', 'Record goal')}</span>
        <i className="fas fa-bullseye btn-icon" aria-hidden="true"></i>
      </button>
      <button
        onClick={() => teamId && onShowPenaltyForm(teamId)}
        className="action-btn penalty-btn"
        disabled={loading || !isMatchInProgress || !teamId}
        type="button"
      >
        <span className="btn-label">{t('floorball.matches.manage.quickActions.recordPenalty', 'Record penalty')}</span>
        <i className="fas fa-exclamation-triangle btn-icon" aria-hidden="true"></i>
      </button>
    </div>
  );

  return (
    <div className="quick-actions-grid">
      <h3 className="qa-title">{t('floorball.matches.manage.quickActions.title', 'Record event')}</h3>
      <h4 className="team-name left">{leftTeamName || t('floorball.matches.manage.scoreboard.home', 'Home')}</h4>
      <h4 className="team-name right">{rightTeamName || t('floorball.matches.manage.scoreboard.away', 'Away')}</h4>
      {renderTeamActions(leftTeamId, leftTeamName, leftGoalieId, leftTeamSide, 'Q')}
      {renderTeamActions(rightTeamId, rightTeamName, rightGoalieId, rightTeamSide, 'R')}
    </div>
  );
};

export default LiveMatchQuickActions;
