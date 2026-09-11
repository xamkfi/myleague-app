import { useTranslation } from 'react-i18next';
import type { FootballTournamentDto } from '../../../../../types/football/tournamentTypes';

interface ConfirmDeleteTournamentModalProps {
  tournament: FootballTournamentDto;
  onConfirm: () => void;
  onCancel: () => void;
}

export const ConfirmDeleteTournamentModal = ({
  tournament,
  onConfirm,
  onCancel,
}: ConfirmDeleteTournamentModalProps) => {
  const { t } = useTranslation();

  return (
    <div className="modal-overlay">
      <div className="modal-content">
        <div className="modal-header">
          <h3>{t('football.tournaments.deleteConfirm.title', 'Delete Tournament')}</h3>
          <button
            className="modal-close-btn"
            onClick={onCancel}
            aria-label={t('common.close', 'Close')}
          >
            ×
          </button>
        </div>

        <div className="modal-body">
          <div className="warning-icon">
            <i className="fas fa-exclamation-triangle"></i>
          </div>

          <p>{t('football.tournaments.deleteConfirm.message', 'Are you sure you want to delete this tournament?')}</p>

          <div className="tournament-details">
            <strong>{tournament.name}</strong>
            <div className="tournament-meta">
              <div className="groups">
                {tournament.groups && tournament.groups.length > 0 ? (
                  tournament.groups.map((group) => (
                    <span key={group.id} className="group">
                      {group.name}
                    </span>
                  ))
                ) : (
                  <span className="group">{t('football.tournaments.noGroups', 'No groups')}</span>
                )}
              </div>
              {tournament.teamCount > 0 && (
                <span className="teams-warning">
                  {t('football.tournaments.deleteConfirm.teamsWarning', 'This tournament has {{count}} teams', {
                    count: tournament.teamCount,
                  })}
                </span>
              )}
            </div>
          </div>

          <p className="warning-text">
            {t('football.tournaments.deleteConfirm.warning', 'This action cannot be undone.')}
          </p>
        </div>

        <div className="modal-footer">
          <button className="btn btn-secondary" onClick={onCancel}>
            {t('common.cancel', 'Cancel')}
          </button>
          <button className="btn btn-danger" onClick={onConfirm}>
            {t('common.delete', 'Delete')}
          </button>
        </div>
      </div>
    </div>
  );
};
