import { useTranslation } from 'react-i18next';

export interface DeleteTournamentTarget {
  id: string;
  name: string;
  status: string;
  teamCount: number;
  matchCount: number;
  groups: Array<{ id: string; name: string }>;
}

interface ConfirmDeleteTournamentDialogProps {
  tournament: DeleteTournamentTarget;
  deleting: boolean;
  error: string | null;
  onConfirm: () => void;
  onCancel: () => void;
}

export default function ConfirmDeleteTournamentDialog({
  tournament,
  deleting,
  error,
  onConfirm,
  onCancel,
}: ConfirmDeleteTournamentDialogProps) {
  const { t } = useTranslation();
  const isDraft = tournament.status === 'Draft';
  const matchesBlockDelete = !isDraft && tournament.matchCount > 0;

  return (
    <div className="modal-overlay">
      <div className="modal-content" role="dialog" aria-modal="true" aria-labelledby="delete-tournament-title">
        <div className="modal-header">
          <h3 id="delete-tournament-title">{t('common.tournamentDelete.title', 'Delete tournament')}</h3>
          <button
            type="button"
            className="modal-close-btn"
            onClick={onCancel}
            disabled={deleting}
            aria-label={t('common.close', 'Close')}
          >
            ×
          </button>
        </div>

        <div className="modal-body">
          <p>
            {t('common.tournamentDelete.message', 'Delete the tournament "{{name}}"?', {
              name: tournament.name,
            })}
          </p>

          <div className="tournament-details">
            <strong>{tournament.name}</strong>
            <div className="tournament-meta">
              <div className="groups">
                {tournament.groups.length > 0 ? (
                  tournament.groups.map((group) => (
                    <span key={group.id} className="group">
                      {group.name}
                    </span>
                  ))
                ) : (
                  <span className="group">{t('common.tournamentDelete.noGroups', 'No groups')}</span>
                )}
              </div>
              {tournament.teamCount > 0 && (
                <span className="teams-warning">
                  {t('common.tournamentDelete.teamsWarning', 'This tournament has {{count}} teams.', {
                    count: tournament.teamCount,
                  })}
                </span>
              )}
            </div>
          </div>

          {isDraft ? (
            <p className="warning-text">
              {t(
                'common.tournamentDelete.draftDetail',
                'This draft is removed as a whole, including its groups, team links and {{matchCount}} matches.',
                { matchCount: tournament.matchCount },
              )}
            </p>
          ) : matchesBlockDelete ? (
            <p className="warning-text">
              {t(
                'common.tournamentDelete.matchesBlock',
                'This tournament has {{matchCount}} matches. Remove those matches before deleting the tournament.',
                { matchCount: tournament.matchCount },
              )}
            </p>
          ) : (
            <p className="warning-text">
              {t('common.tournamentDelete.warning', 'This cannot be undone.')}
            </p>
          )}

          {error && <p className="warning-text">{error}</p>}
        </div>

        <div className="modal-footer">
          <button type="button" className="btn btn-secondary" onClick={onCancel} disabled={deleting}>
            {t('common.cancel', 'Cancel')}
          </button>
          <button
            type="button"
            className="btn btn-danger"
            onClick={onConfirm}
            disabled={deleting || matchesBlockDelete}
          >
            {deleting
              ? t('common.deleting', 'Deleting...')
              : t('common.tournamentDelete.confirm', 'Yes, delete tournament')}
          </button>
        </div>
      </div>
    </div>
  );
}
