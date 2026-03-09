import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import ActionsDropdown from '../../../../components/ActionsDropdown/ActionsDropdown';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import { floorballTournamentService } from '../../../../api/floorball/floorballTournamentService';
import type { FloorballTournamentSummaryDto } from '../../../../types/floorball/floorballTypes';
import '../../../../styles/AdminTable.scss';
import './FloorballTournamentsPage.scss';

const STATUS_FILTERS = ['', 'Draft', 'Active', 'InProgress', 'Completed', 'Cancelled'] as const;

const FloorballTournamentsPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const [tournaments, setTournaments] = useState<FloorballTournamentSummaryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [statusFilter, setStatusFilter] = useState('');
  const [operationLoading, setOperationLoading] = useState<string | null>(null);

  const loadTournaments = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await floorballTournamentService.getAll(statusFilter || undefined);
      setTournaments(response.data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load tournaments');
    } finally {
      setLoading(false);
    }
  }, [statusFilter]);

  useEffect(() => {
    loadTournaments();
  }, [loadTournaments]);

  const handleDelete = async (tournament: FloorballTournamentSummaryDto) => {
    if (!window.confirm(t('tournament.confirmDelete', 'Are you sure you want to delete this tournament?'))) {
      return;
    }
    try {
      setOperationLoading(tournament.id);
      await floorballTournamentService.delete(tournament.id);
      await loadTournaments();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to delete tournament');
    } finally {
      setOperationLoading(null);
    }
  };

  const handleChangeStatus = async (tournament: FloorballTournamentSummaryDto, action: string) => {
    try {
      setOperationLoading(tournament.id);
      await floorballTournamentService.changeStatus(tournament.id, action);
      await loadTournaments();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to change tournament status');
    } finally {
      setOperationLoading(null);
    }
  };

  const formatDate = (dateString: string) => {
    try {
      return new Date(dateString).toLocaleDateString();
    } catch {
      return dateString;
    }
  };

  const getStatusBadge = (status: string) => {
    const statusMap: Record<string, { className: string; label: string }> = {
      Draft: { className: 'admin-badge admin-badge--inactive', label: t('tournament.status.draft', 'Draft') },
      Active: { className: 'admin-badge admin-badge--active', label: t('tournament.status.active', 'Active') },
      InProgress: { className: 'admin-badge admin-badge--active', label: t('tournament.status.inProgress', 'In Progress') },
      Completed: { className: 'admin-badge admin-badge--completed', label: t('tournament.status.completed', 'Completed') },
      Cancelled: { className: 'admin-badge admin-badge--inactive', label: t('tournament.status.cancelled', 'Cancelled') },
    };
    const s = statusMap[status] || { className: 'admin-badge', label: status };
    return <span className={s.className}>{s.label}</span>;
  };

  const getActions = (tournament: FloorballTournamentSummaryDto) => {
    const actions: { label: string; onClick: () => void; variant?: 'default' | 'danger' | 'status'; disabled: boolean }[] = [
      {
        label: t('common.edit', 'Edit'),
        onClick: () => navigate(`/admin/floorball/tournaments/${tournament.id}/edit`),
        disabled: operationLoading === tournament.id,
      },
      {
        label: t('tournament.manageMatches', 'Matches'),
        onClick: () => navigate(`/admin/floorball/tournaments/${tournament.id}/matches`),
        disabled: operationLoading === tournament.id,
      },
    ];

    if (tournament.status === 'Draft') {
      actions.push({
        label: t('tournament.activate', 'Activate'),
        onClick: () => handleChangeStatus(tournament, 'activate'),
        variant: 'status',
        disabled: operationLoading === tournament.id,
      });
    }
    if (tournament.status === 'Active') {
      actions.push({
        label: t('tournament.start', 'Start'),
        onClick: () => handleChangeStatus(tournament, 'start'),
        variant: 'status',
        disabled: operationLoading === tournament.id,
      });
    }
    if (tournament.status === 'InProgress') {
      actions.push({
        label: t('tournament.complete', 'Complete'),
        onClick: () => handleChangeStatus(tournament, 'complete'),
        variant: 'status',
        disabled: operationLoading === tournament.id,
      });
    }
    if (tournament.status !== 'Completed' && tournament.status !== 'Cancelled') {
      actions.push({
        label: t('tournament.cancel', 'Cancel'),
        onClick: () => handleChangeStatus(tournament, 'cancel'),
        variant: 'status',
        disabled: operationLoading === tournament.id,
      });
    }

    actions.push({
      label: t('common.delete', 'Delete'),
      onClick: () => handleDelete(tournament),
      variant: 'danger',
      disabled: operationLoading === tournament.id,
    });

    return actions;
  };

  return (
    <PageTemplate title={t('tournament.adminTitle', 'Manage Tournaments')}>
      <div className="tournaments-container">

        <div className="tournaments-header">
          <div className="tournaments-header__left">
            <h2>{t('tournament.adminTitle', 'Manage Tournaments')}</h2>
            <span className="tournaments-header__count">
              {tournaments.length} {t('tournament.count', 'tournaments')}
            </span>
          </div>
          <button
            className="tournaments-header__create-btn"
            onClick={() => navigate('/admin/floorball/tournaments/create')}
          >
            + {t('tournament.create', 'New Tournament')}
          </button>
        </div>

        <ErrorPopup message={error} />

        <div className="tournaments-filters">
          <div className="tournaments-filters__tabs">
            {STATUS_FILTERS.map((s) => (
              <button
                key={s}
                className={`tournaments-filters__tab${statusFilter === s ? ' tournaments-filters__tab--active' : ''}`}
                onClick={() => setStatusFilter(s)}
              >
                {s === '' ? t('tournament.allStatuses', 'All') : t(`tournament.status.${s.charAt(0).toLowerCase() + s.slice(1)}`, s)}
              </button>
            ))}
          </div>
        </div>

        {loading ? (
          <div className="tournaments-loading">
            {t('common.loading', 'Loading...')}
          </div>
        ) : (
          <div className="admin-table__wrapper">
            {tournaments.length === 0 ? (
              <div className="admin-table__empty">
                <p>{t('tournament.noTournaments', 'No tournaments found')}</p>
              </div>
            ) : (
              <table className="admin-table">
                <thead>
                  <tr>
                    <th>{t('tournament.fields.name', 'Name')}</th>
                    <th>{t('tournament.fields.startDate', 'Start')}</th>
                    <th>{t('tournament.fields.endDate', 'End')}</th>
                    <th>{t('tournament.fields.location', 'Location')}</th>
                    <th>{t('tournament.fields.groups', 'Groups')}</th>
                    <th>{t('tournament.fields.teams', 'Teams')}</th>
                    <th>{t('tournament.fields.status', 'Status')}</th>
                    <th className="admin-table__actions-col">{t('common.actions', 'Actions')}</th>
                  </tr>
                </thead>
                <tbody>
                  {tournaments.map((tournament) => (
                    <tr
                      key={tournament.id}
                      className="admin-table__row--clickable"
                      onClick={() => navigate(`/admin/floorball/tournaments/${tournament.id}/edit`)}
                    >
                      <td className="admin-table__name">{tournament.name}</td>
                      <td>{formatDate(tournament.startDate)}</td>
                      <td>{formatDate(tournament.endDate)}</td>
                      <td>
                        {tournament.location || (
                          <span className="admin-table__muted">-</span>
                        )}
                      </td>
                      <td>{tournament.groupCount}</td>
                      <td>{tournament.teamCount}</td>
                      <td>{getStatusBadge(tournament.status)}</td>
                      <td
                        className="admin-table__actions-col"
                        onClick={(e) => e.stopPropagation()}
                      >
                        <ActionsDropdown
                          actions={getActions(tournament)}
                          ariaLabel={t('tournament.actions.menu', 'Tournament actions')}
                        />
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        )}
      </div>
    </PageTemplate>
  );
};

export default FloorballTournamentsPage;
