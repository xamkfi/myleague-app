import { useTranslation } from 'react-i18next';
import type { FloorballTournamentDto } from '../../../../../types/floorball/tournamentTypes';
import ActionsDropdown from '../../../../../components/ActionsDropdown/ActionsDropdown';
import '../../../../../styles/AdminTable.scss';

interface TournamentsTableProps {
  tournaments: FloorballTournamentDto[];
  onEdit: (tournament: FloorballTournamentDto) => void;
  onDelete: (tournament: FloorballTournamentDto) => void;
  onLifecycleAction: (tournament: FloorballTournamentDto, action: 'startGroupStage' | 'startPlayoffStage' | 'complete' | 'cancel') => void;
  /** Opens the tournament-matches management page with this tournament pre-selected in the filter. */
  onManageMatches: (tournament: FloorballTournamentDto) => void;
  operationLoading?: string | null;
  selectedIds: Set<string>;
  onToggleSelect: (id: string) => void;
  onSelectAll: () => void;
  onClearSelection: () => void;
}

const formatDate = (dateString: string): string => {
  try {
    return new Date(dateString).toLocaleDateString();
  } catch {
    return dateString;
  }
};

const getStatusBadgeClass = (status: string): string => {
  switch (status) {
    case 'Draft': return 'admin-badge--inactive';
    case 'GroupStage':
    case 'PlayoffStage': return 'admin-badge--active';
    case 'Completed': return 'admin-badge--completed';
    case 'Cancelled': return 'admin-badge--danger';
    default: return 'admin-badge--inactive';
  }
};

export const TournamentsTable = ({
  tournaments,
  onEdit,
  onDelete,
  onLifecycleAction,
  onManageMatches,
  operationLoading,
  selectedIds,
  onToggleSelect,
  onSelectAll,
  onClearSelection,
}: TournamentsTableProps) => {
  const { t } = useTranslation();

  const getActions = (tournament: FloorballTournamentDto) => {
    const actions: { label: string; onClick: () => void; variant?: 'default' | 'danger' | 'status'; disabled: boolean }[] = [
      {
        label: t('common.edit', 'Edit'),
        onClick: () => onEdit(tournament),
        disabled: operationLoading === tournament.id,
      },
      {
        label: t('floorball.tournaments.actions.manageMatches', 'Hallitse turnauksen otteluita'),
        onClick: () => onManageMatches(tournament),
        disabled: operationLoading === tournament.id,
      },
    ];

    const status = tournament.tournamentStatus;
    if (status === 'Draft') {
      actions.push({
        label: t('floorball.tournaments.lifecycle.startGroupStage', 'Start Group Stage'),
        onClick: () => onLifecycleAction(tournament, 'startGroupStage'),
        variant: 'status',
        disabled: operationLoading === tournament.id,
      });
    }
    if (status === 'GroupStage') {
      actions.push({
        label: t('floorball.tournaments.lifecycle.startPlayoffStage', 'Start Playoff'),
        onClick: () => onLifecycleAction(tournament, 'startPlayoffStage'),
        variant: 'status',
        disabled: operationLoading === tournament.id,
      });
    }
    if (status === 'GroupStage' || status === 'PlayoffStage') {
      actions.push({
        label: t('floorball.tournaments.lifecycle.complete', 'Complete Tournament'),
        onClick: () => onLifecycleAction(tournament, 'complete'),
        variant: 'status',
        disabled: operationLoading === tournament.id,
      });
    }
    if (status !== 'Completed' && status !== 'Cancelled') {
      actions.push({
        label: t('floorball.tournaments.lifecycle.cancel', 'Cancel Tournament'),
        onClick: () => onLifecycleAction(tournament, 'cancel'),
        variant: 'danger',
        disabled: operationLoading === tournament.id,
      });
    }

    actions.push({
      label: t('common.delete', 'Delete'),
      onClick: () => onDelete(tournament),
      variant: 'danger',
      disabled: operationLoading === tournament.id,
    });

    return actions;
  };

  const getStatusLabel = (status: string): string => {
    switch (status) {
      case 'Draft': return t('floorball.tournaments.status.draft', 'Draft');
      case 'GroupStage': return t('floorball.tournaments.status.groupStage', 'Group Stage');
      case 'PlayoffStage': return t('floorball.tournaments.status.playoffStage', 'Playoff Stage');
      case 'Completed': return t('floorball.tournaments.status.completed', 'Completed');
      case 'Cancelled': return t('floorball.tournaments.status.cancelled', 'Cancelled');
      default: return status;
    }
  };

  return (
    <table className="admin-table">
      <thead>
        <tr>
          <th className="admin-table__checkbox-col">
            <input
              type="checkbox"
              checked={tournaments.length > 0 && tournaments.every((tournament) => selectedIds.has(tournament.id))}
              onChange={(e) => {
                if (e.target.checked) {
                  onSelectAll();
                } else {
                  onClearSelection();
                }
              }}
              title={t('floorball.tournaments.selectAll', 'Select all tournaments')}
            />
          </th>
          <th>{t('floorball.tournaments.fields.name', 'Name')}</th>
          <th>{t('floorball.tournaments.fields.groups', 'Groups')}</th>
          <th>{t('floorball.tournaments.fields.startDate', 'Starts')}</th>
          <th>{t('floorball.tournaments.fields.endDate', 'Ends')}</th>
          <th>{t('floorball.tournaments.fields.teams', 'Teams')}</th>
          <th>{t('floorball.tournaments.fields.matches', 'Matches')}</th>
          <th>{t('floorball.tournaments.fields.status', 'Status')}</th>
          <th className="admin-table__actions-col">{t('common.actions', 'Actions')}</th>
        </tr>
      </thead>
      <tbody>
        {tournaments.map((tournament) => (
          <tr
            key={tournament.id}
            className={`admin-table__row--clickable${selectedIds.has(tournament.id) ? ' admin-table__row--selected' : ''}`}
            onClick={() => onToggleSelect(tournament.id)}
          >
            <td className="admin-table__checkbox-col">
              <input
                type="checkbox"
                checked={selectedIds.has(tournament.id)}
                onChange={() => onToggleSelect(tournament.id)}
                onClick={(e) => e.stopPropagation()}
              />
            </td>
            <td className="admin-table__name">{tournament.name}</td>
            <td>
              <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.25rem' }}>
                {tournament.groups && tournament.groups.length > 0 ? (
                  tournament.groups.map((group) => (
                    <span key={group.id} className="admin-tag admin-tag--blue">
                      {group.name}
                    </span>
                  ))
                ) : (
                  <span className="admin-table__muted">
                    {t('floorball.tournaments.noGroups', 'No groups')}
                  </span>
                )}
              </div>
            </td>
            <td>{formatDate(tournament.startDate)}</td>
            <td>{formatDate(tournament.endDate)}</td>
            <td>
              <span className="admin-table__muted">
                {t('floorball.tournaments.teamsCount', '{{count}} teams', { count: tournament.teamCount })}
              </span>
            </td>
            <td>
              <span className="admin-table__muted">
                {t('floorball.tournaments.matchesCount', '{{count}} matches', { count: tournament.matchCount })}
              </span>
            </td>
            <td>
              <span className={`admin-badge ${getStatusBadgeClass(tournament.tournamentStatus)}`}>
                {getStatusLabel(tournament.tournamentStatus)}
              </span>
            </td>
            <td className="admin-table__actions-col">
              <ActionsDropdown
                actions={getActions(tournament)}
                ariaLabel={t('floorball.tournaments.actions.menu', 'Tournament actions menu')}
              />
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
};
