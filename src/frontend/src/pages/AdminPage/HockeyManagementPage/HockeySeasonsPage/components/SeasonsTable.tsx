import { useTranslation } from 'react-i18next';
import type { HockeySeasonDto } from '../../../../../types/hockey/hockeyTypes';
import ActionsDropdown from '../../../../../components/ActionsDropdown/ActionsDropdown';
import LiveDot from '../../../../../components/LiveDot/LiveDot';
import TeamCategoryBadge from '../../../../../components/TeamCategoryBadge/TeamCategoryBadge';
import { useHockeyInProgressMatches } from '../../../../../hooks/useHockeyInProgressMatches';
import { formatHockeyDate } from '../../../../../utils/hockeyLookups';
import '../../../../../styles/AdminTable.scss';

interface SeasonsTableProps {
  seasons: HockeySeasonDto[];
  onEdit: (season: HockeySeasonDto) => void;
  onActivateToggle: (season: HockeySeasonDto) => void;
  onComplete: (season: HockeySeasonDto) => void;
  operationLoading?: string | null;
}

export function SeasonsTable({
  seasons,
  onEdit,
  onActivateToggle,
  onComplete,
  operationLoading,
}: SeasonsTableProps) {
  const { t } = useTranslation();
  const { countByCompetitionId } = useHockeyInProgressMatches();

  const getStatusBadge = (season: HockeySeasonDto) => {
    if (season.isCompleted) {
      return <span className="admin-badge admin-badge--completed">{t('hockey.seasons.statusCompleted', 'Completed')}</span>;
    }
    if (season.isActive) {
      return <span className="admin-badge admin-badge--active">{t('hockey.seasons.statusActive', 'Active')}</span>;
    }
    return <span className="admin-badge admin-badge--inactive">{t('hockey.seasons.statusInactive', 'Inactive')}</span>;
  };

  return (
    <table className="admin-table">
      <thead>
        <tr>
          <th>{t('hockey.seasons.fields.name', 'Name')}</th>
          <th>{t('hockey.seasons.fields.division', 'Division')}</th>
          <th>{t('hockey.seasons.fields.startDate', 'Starts')}</th>
          <th>{t('hockey.seasons.fields.endDate', 'Ends')}</th>
          <th>{t('hockey.seasons.fields.teams', 'Teams')}</th>
          <th>{t('hockey.seasons.fields.status', 'Status')}</th>
          <th className="admin-table__actions-col">{t('common.actions', 'Actions')}</th>
        </tr>
      </thead>
      <tbody>
        {seasons.map((season) => {
          const liveCount = countByCompetitionId.get(season.id) ?? 0;
          return (
            <tr
              key={season.id}
              className="admin-table__row--clickable"
              onClick={() => onEdit(season)}
              role="button"
              tabIndex={0}
              title={t('hockey.seasons.actions.openEdit', 'Open and edit season')}
              onKeyDown={(event) => {
                if (event.key === 'Enter' || event.key === ' ') {
                  event.preventDefault();
                  onEdit(season);
                }
              }}
            >
              <td className="admin-table__name">
                <span className="admin-table__name-inner">
                  {liveCount > 0 && (
                    <LiveDot
                      tone="light"
                      count={liveCount}
                      ariaLabel={t('hockey.seasons.matchesInProgress', '{{count}} match(es) in progress', { count: liveCount })}
                    />
                  )}
                  <span>{season.name}</span>
                  <TeamCategoryBadge category={season.teamCategory} />
                </span>
              </td>
              <td>
                <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.25rem' }}>
                  {(season.divisions ?? []).length > 0 ? (
                    season.divisions.map((division) => (
                      <span key={division.id} className="admin-tag admin-tag--blue">
                        {division.name}
                      </span>
                    ))
                  ) : (
                    <span className="admin-table__muted">{t('hockey.seasons.noDivisions', 'No divisions')}</span>
                  )}
                </div>
              </td>
              <td>{formatHockeyDate(season.startDate)}</td>
              <td>{formatHockeyDate(season.endDate)}</td>
              <td>
                <span className="admin-table__muted">
                  {season.teams?.length || 0} {t('hockey.seasons.teamsCountLabel', 'teams')}
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
                      label: t('common.edit', 'Edit'),
                      onClick: () => onEdit(season),
                      disabled: operationLoading === season.id,
                    },
                    ...(!season.isCompleted
                      ? [{
                          label: season.isActive
                            ? t('hockey.seasons.deactivate', 'Deactivate')
                            : t('hockey.seasons.activate', 'Activate'),
                          onClick: () => onActivateToggle(season),
                          variant: 'status' as const,
                          disabled: operationLoading === season.id,
                        }]
                      : []),
                    ...(season.isActive && !season.isCompleted
                      ? [{
                          label: t('hockey.seasons.complete', 'Complete Season'),
                          onClick: () => onComplete(season),
                          variant: 'status' as const,
                          disabled: operationLoading === season.id,
                        }]
                      : []),
                  ]}
                  ariaLabel={t('hockey.seasons.actions.menu', 'Season actions menu')}
                />
              </td>
            </tr>
          );
        })}
      </tbody>
    </table>
  );
}
