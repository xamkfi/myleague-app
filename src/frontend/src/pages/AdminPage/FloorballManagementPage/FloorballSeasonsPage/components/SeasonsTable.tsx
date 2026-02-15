import { useTranslation } from 'react-i18next';
import { useDivisions } from '../../../../../hooks/useDivisions';
import type { FloorballSeasonDto } from '../../../../../api/floorball/floorballSeasonService';
import ActionsDropdown from '../../../../../components/ActionsDropdown/ActionsDropdown';
import '../../../../../styles/AdminTable.scss';

interface SeasonsTableProps {
  seasons: FloorballSeasonDto[];
  onEdit: (season: FloorballSeasonDto) => void;
  onDelete: (season: FloorballSeasonDto) => void;
  onActivateToggle: (season: FloorballSeasonDto) => void;
  onComplete: (season: FloorballSeasonDto) => void;
  operationLoading?: string | null;
  selectedIds: Set<string>;
  onToggleSelect: (id: string) => void;
  onSelectAll: () => void;
  onClearSelection: () => void;
}

export const SeasonsTable = ({
  seasons,
  onEdit,
  onDelete,
  onActivateToggle,
  onComplete,
  operationLoading,
  selectedIds,
  onToggleSelect,
  onSelectAll,
  onClearSelection,
}: SeasonsTableProps) => {
  const { t } = useTranslation();
  const { divisions } = useDivisions();

  const formatDate = (dateString: string) => {
    try {
      return new Date(dateString).toLocaleDateString();
    } catch {
      return dateString;
    }
  };

  const getStatusBadge = (season: FloorballSeasonDto) => {
    if (season.isCompleted) {
      return (
        <span className="admin-badge admin-badge--completed">
          {t('floorball.seasons.status.completed', 'Completed')}
        </span>
      );
    }
    if (season.isActive) {
      return (
        <span className="admin-badge admin-badge--active">
          {t('floorball.seasons.status.active', 'Active')}
        </span>
      );
    }
    return (
      <span className="admin-badge admin-badge--inactive">
        {t('floorball.seasons.status.inactive', 'Inactive')}
      </span>
    );
  };

  const getActions = (season: FloorballSeasonDto) => {
    const actions: { label: string; onClick: () => void; variant?: 'default' | 'danger' | 'status'; disabled: boolean }[] = [
      {
        label: t('common.edit', 'Edit'),
        onClick: () => onEdit(season),
        disabled: operationLoading === season.id,
      },
    ];

    if (!season.isCompleted) {
      actions.push({
        label: season.isActive
          ? t('floorball.seasons.deactivate', 'Deactivate')
          : t('floorball.seasons.activate', 'Activate'),
        onClick: () => onActivateToggle(season),
        disabled: operationLoading === season.id,
      });
    }

    if (season.isActive && !season.isCompleted) {
      actions.push({
        label: t('floorball.seasons.complete', 'Complete Season'),
        onClick: () => onComplete(season),
        disabled: operationLoading === season.id,
      });
    }

    actions.push({
      label: t('common.delete', 'Delete'),
      onClick: () => onDelete(season),
      variant: 'danger' as const,
      disabled: operationLoading === season.id,
    });

    return actions;
  };

  return (
    <table className="admin-table">
      <thead>
        <tr>
          <th className="admin-table__checkbox-col">
            <input
              type="checkbox"
              checked={seasons.length > 0 && seasons.every((s) => selectedIds.has(s.id))}
              onChange={(e) => {
                if (e.target.checked) {
                  onSelectAll();
                } else {
                  onClearSelection();
                }
              }}
              title={t('floorball.seasons.selectAll', 'Select all seasons')}
            />
          </th>
          <th>{t('floorball.seasons.fields.name', 'Name')}</th>
          <th>{t('floorball.seasons.fields.division', 'Division')}</th>
          <th>{t('floorball.seasons.fields.startDate', 'Starts')}</th>
          <th>{t('floorball.seasons.fields.endDate', 'Ends')}</th>
          <th>{t('floorball.seasons.fields.teams', 'Teams')}</th>
          <th>{t('floorball.seasons.fields.status', 'Status')}</th>
          <th className="admin-table__actions-col">{t('common.actions', 'Actions')}</th>
        </tr>
      </thead>
      <tbody>
        {seasons.map((season) => (
          <tr
            key={season.id}
            className={`admin-table__row--clickable${selectedIds.has(season.id) ? ' admin-table__row--selected' : ''}`}
            onClick={() => onToggleSelect(season.id)}
          >
            <td className="admin-table__checkbox-col">
              <input
                type="checkbox"
                checked={selectedIds.has(season.id)}
                onChange={() => onToggleSelect(season.id)}
                onClick={(e) => e.stopPropagation()}
              />
            </td>
            <td className="admin-table__name">{season.name}</td>
            <td>
              <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.25rem' }}>
                {season.seasonDivisions && season.seasonDivisions.length > 0 ? (
                  season.seasonDivisions.map((seasonDivision) => {
                    const division = divisions.find((d) => d.id === seasonDivision.divisionId);
                    return (
                      <span key={seasonDivision.divisionId} className="admin-tag admin-tag--blue">
                        {division?.name || seasonDivision.divisionId}
                      </span>
                    );
                  })
                ) : (
                  <span className="admin-table__muted">
                    {t('floorball.seasons.noDivisions', 'No divisions')}
                  </span>
                )}
              </div>
            </td>
            <td>{formatDate(season.startDate)}</td>
            <td>{formatDate(season.endDate)}</td>
            <td>
              <span className="admin-table__muted">
                {season.teams?.length || 0} {t('floorball.seasons.teamsCount', 'teams')}
              </span>
            </td>
            <td>{getStatusBadge(season)}</td>
            <td className="admin-table__actions-col">
              <ActionsDropdown
                actions={getActions(season)}
                ariaLabel={t('floorball.seasons.actions.menu', 'Season actions menu')}
              />
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
};
