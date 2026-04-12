import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import { floorballTournamentService } from '../../../../api/floorball/floorballTournamentService';
import type { FloorballTournamentDto } from '../../../../types/floorball/tournamentTypes';
import './FloorballTournamentsPage.scss';

const FloorballTournamentsPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const [tournaments, setTournaments] = useState<FloorballTournamentDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<FloorballTournamentDto | null>(null);
  const [deleting, setDeleting] = useState(false);

  const loadTournaments = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await floorballTournamentService.getAll();
      setTournaments(response.data);
    } catch (err) {
      const msg = err instanceof Error ? err.message : 'Failed to load tournaments';
      setError(msg);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadTournaments();
  }, [loadTournaments]);

  const handleDelete = async () => {
    if (!deleteTarget) return;
    try {
      setDeleting(true);
      await floorballTournamentService.delete(deleteTarget.id);
      setDeleteTarget(null);
      await loadTournaments();
    } catch (err) {
      const msg = err instanceof Error ? err.message : 'Failed to delete tournament';
      setError(msg);
    } finally {
      setDeleting(false);
    }
  };

  const formatDate = (dateStr: string): string => {
    try {
      return new Date(dateStr).toLocaleDateString();
    } catch {
      return dateStr;
    }
  };

  const getStatusBadgeClass = (status: string): string => {
    const normalized = status.toLowerCase().replace(/\s+/g, '');
    if (normalized.includes('draft')) return 'status-badge--draft';
    if (normalized.includes('registration')) return 'status-badge--registration';
    if (normalized.includes('group')) return 'status-badge--groupstage';
    if (normalized.includes('playoff')) return 'status-badge--playoff';
    if (normalized.includes('completed')) return 'status-badge--completed';
    return 'status-badge--active';
  };

  if (loading) {
    return (
      <PageTemplate title={t('floorball.tournaments.title', 'Manage Tournaments')}>
        <div className="tournaments-loading">
          <i className="fas fa-spinner fa-spin"></i>
          <p>{t('common.loading', 'Loading...')}</p>
        </div>
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={t('floorball.tournaments.title', 'Manage Tournaments')}>
      <div className="floorball-tournaments-container">
        <div className="tournaments-page-header">
          <div className="tournaments-page-header__info">
            <h2>{t('floorball.tournaments.title', 'Manage Tournaments')}</h2>
            <p>{t('floorball.tournaments.subtitle', '{{count}} tournament(s)', { count: tournaments.length })}</p>
          </div>
          <button
            className="btn-create"
            onClick={() => navigate('/admin/floorball/tournaments/create')}
          >
            <i className="fas fa-plus"></i>
            {t('floorball.tournaments.create', 'Create Tournament')}
          </button>
        </div>

        <ErrorPopup message={error} />

        {tournaments.length === 0 ? (
          <div className="tournaments-empty">
            <p>{t('floorball.tournaments.noTournaments', 'No tournaments found. Create one to get started.')}</p>
          </div>
        ) : (
          <div className="tournaments-table-wrapper">
            <table className="tournaments-table">
              <thead>
                <tr>
                  <th>{t('floorball.tournaments.fields.name', 'Name')}</th>
                  <th>{t('floorball.tournaments.fields.dates', 'Dates')}</th>
                  <th>{t('floorball.tournaments.fields.venue', 'Venue')}</th>
                  <th>{t('floorball.tournaments.fields.status', 'Status')}</th>
                  <th>{t('floorball.tournaments.fields.teams', 'Teams')}</th>
                  <th>{t('floorball.tournaments.fields.matches', 'Matches')}</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {tournaments.map((tournament) => (
                  <tr key={tournament.id}>
                    <td>
                      <strong>{tournament.name}</strong>
                    </td>
                    <td>
                      {formatDate(tournament.startDate)} – {formatDate(tournament.endDate)}
                    </td>
                    <td>{tournament.venue ?? '—'}</td>
                    <td>
                      <span className={`status-badge ${getStatusBadgeClass(tournament.tournamentStatus)}`}>
                        {tournament.tournamentStatus}
                      </span>
                    </td>
                    <td>{tournament.teamCount}</td>
                    <td>{tournament.matchCount}</td>
                    <td>
                      <div className="tournaments-table__actions">
                        <button
                          className="btn-action btn-action--edit"
                          onClick={() => navigate(`/admin/floorball/tournaments/${tournament.id}/edit`)}
                        >
                          <i className="fas fa-edit"></i>
                          {t('common.edit', 'Edit')}
                        </button>
                        <button
                          className="btn-action btn-action--delete"
                          onClick={() => setDeleteTarget(tournament)}
                        >
                          <i className="fas fa-trash-alt"></i>
                          {t('common.delete', 'Delete')}
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        {deleteTarget && (
          <div className="modal-overlay" onClick={() => !deleting && setDeleteTarget(null)}>
            <div className="modal-content" onClick={(e) => e.stopPropagation()}>
              <h3>{t('floorball.tournaments.deleteConfirm.title', 'Delete Tournament')}</h3>
              <p>
                {t('floorball.tournaments.deleteConfirm.message', 'Are you sure you want to delete "{{name}}"? This action cannot be undone.', {
                  name: deleteTarget.name,
                })}
              </p>
              <div className="modal-actions">
                <button
                  className="btn btn-secondary"
                  onClick={() => setDeleteTarget(null)}
                  disabled={deleting}
                >
                  {t('common.cancel', 'Cancel')}
                </button>
                <button
                  className="btn btn-danger"
                  onClick={handleDelete}
                  disabled={deleting}
                >
                  {deleting ? (
                    <><i className="fas fa-spinner fa-spin"></i> {t('common.deleting', 'Deleting...')}</>
                  ) : (
                    t('common.delete', 'Delete')
                  )}
                </button>
              </div>
            </div>
          </div>
        )}
      </div>
    </PageTemplate>
  );
};

export default FloorballTournamentsPage;
