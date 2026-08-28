import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import ActionsDropdown from '../ActionsDropdown/ActionsDropdown';
import LiveDot from '../LiveDot/LiveDot';
import TeamCategoryBadge from '../TeamCategoryBadge/TeamCategoryBadge';
import { getLeaguePath, type SportKind } from '../../utils/sportRoutes';
import AdminNameLink from './AdminNameLink';
import type { AdminSeasonRow, AdminSeasonTableLabels } from './adminTableTypes';
import '../../styles/AdminTable.scss';

interface AdminSeasonsTableProps {
  sport: SportKind;
  seasons: AdminSeasonRow[];
  labels: AdminSeasonTableLabels;
  liveCounts: Map<string, number>;
  onEdit: (seasonId: string) => void;
  onActivateToggle: (seasonId: string) => void;
  onComplete: (seasonId: string) => void;
  onDelete?: (seasonId: string) => void;
  operationLoading?: string | null;
  formatDate?: (value: string) => string;
}

function defaultFormatDate(dateString: string): string {
  try {
    return new Date(dateString).toLocaleDateString();
  } catch {
    return dateString;
  }
}

export default function AdminSeasonsTable({
  sport,
  seasons,
  labels,
  liveCounts,
  onEdit,
  onActivateToggle,
  onComplete,
  onDelete,
  operationLoading,
  formatDate = defaultFormatDate,
}: AdminSeasonsTableProps) {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const getStatusBadge = (season: AdminSeasonRow) => {
    if (season.isCompleted) {
      return <span className="admin-badge admin-badge--completed">{labels.completed}</span>;
    }
    if (season.isActive) {
      return <span className="admin-badge admin-badge--active">{labels.active}</span>;
    }
    return <span className="admin-badge admin-badge--inactive">{labels.inactive}</span>;
  };

  return (
    <table className="admin-table">
      <thead>
        <tr>
          <th>{labels.name}</th>
          <th>{labels.division}</th>
          <th>{labels.startDate}</th>
          <th>{labels.endDate}</th>
          <th>{labels.teams}</th>
          <th>{labels.status}</th>
          <th className="admin-table__actions-col">{t('common.actions')}</th>
        </tr>
      </thead>
      <tbody>
        {seasons.map((season) => {
          const liveCount = liveCounts.get(season.id) ?? 0;
          const publicPath = getLeaguePath(sport, season.id);
          const busy = operationLoading === season.id;

          return (
            <tr
              key={season.id}
              className="admin-table__row--clickable"
              onClick={() => onEdit(season.id)}
              role="button"
              tabIndex={0}
              title={labels.openEdit}
              onKeyDown={(event) => {
                if (event.key === 'Enter' || event.key === ' ') {
                  event.preventDefault();
                  onEdit(season.id);
                }
              }}
            >
              <td className="admin-table__name">
                <span className="admin-table__name-inner">
                  {liveCount > 0 && (
                    <LiveDot
                      tone="light"
                      count={liveCount}
                      ariaLabel={labels.matchesInProgress(liveCount)}
                    />
                  )}
                  <AdminNameLink to={publicPath}>{season.name}</AdminNameLink>
                  <TeamCategoryBadge category={season.teamCategory} />
                </span>
              </td>
              <td>
                <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.25rem' }}>
                  {season.divisions.length > 0 ? (
                    season.divisions.map((division) => (
                      <span key={division.id} className="admin-tag admin-tag--blue">
                        {division.name}
                      </span>
                    ))
                  ) : (
                    <span className="admin-table__muted">{labels.noDivisions}</span>
                  )}
                </div>
              </td>
              <td>{formatDate(season.startDate)}</td>
              <td>{formatDate(season.endDate)}</td>
              <td>
                <span className="admin-table__muted">
                  {season.teamCount} {labels.teamsCount}
                </span>
              </td>
              <td>{getStatusBadge(season)}</td>
              <td
                className="admin-table__actions-col"
                onClick={(event) => event.stopPropagation()}
                onKeyDown={(event) => event.stopPropagation()}
              >
                <ActionsDropdown
                  actions={[
                    {
                      label: t('common.edit'),
                      onClick: () => onEdit(season.id),
                      disabled: busy,
                    },
                    {
                      label: t('common.viewPublic'),
                      onClick: () => navigate(publicPath),
                      disabled: busy,
                    },
                    ...(!season.isCompleted
                      ? [{
                          label: season.isActive ? labels.deactivate : labels.activate,
                          onClick: () => onActivateToggle(season.id),
                          variant: 'status' as const,
                          disabled: busy,
                        }]
                      : []),
                    ...(season.isActive && !season.isCompleted
                      ? [{
                          label: labels.complete,
                          onClick: () => onComplete(season.id),
                          variant: 'status' as const,
                          disabled: busy,
                        }]
                      : []),
                    ...(onDelete
                      ? [{
                          label: t('common.delete'),
                          onClick: () => onDelete(season.id),
                          variant: 'danger' as const,
                          disabled: busy,
                        }]
                      : []),
                  ]}
                  ariaLabel={labels.actionsMenu}
                />
              </td>
            </tr>
          );
        })}
      </tbody>
    </table>
  );
}
