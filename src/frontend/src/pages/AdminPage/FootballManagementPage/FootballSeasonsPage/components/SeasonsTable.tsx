import { useTranslation } from 'react-i18next';
import { useMemo } from 'react';
import { useDivisions } from '../../../../../hooks/useDivisions';
import { SportsCategory } from '../../../../../types/common/sports';
// TODO: parent agent will add useInProgressFootballMatches
import { useInProgressFootballMatches } from '../../../../../hooks/useInProgressFootballMatches';
import type { FootballSeasonDto } from '../../../../../api/football/footballSeasonService';
import ActionsDropdown from '../../../../../components/ActionsDropdown/ActionsDropdown';
import LiveDot from '../../../../../components/LiveDot/LiveDot';
import TeamCategoryBadge from '../../../../../components/TeamCategoryBadge/TeamCategoryBadge';
import '../../../../../styles/AdminTable.scss';

interface SeasonsTableProps {
  seasons: FootballSeasonDto[];

  /**
   * Navigoi kauden edit-sivulle.
   * Tätä kutsutaan riviklikistä sekä Actions-valikon Edit-toiminnosta.
   */
  onEdit: (season: FootballSeasonDto) => void;

  /**
   * Avaa kauden poistovahvistuksen.
   */
  onDelete: (season: FootballSeasonDto) => void;

  /**
   * Aktivoi tai deaktivoi kauden.
   */
  onActivateToggle: (season: FootballSeasonDto) => void;

  /**
   * Merkitsee aktiivisen kauden valmiiksi.
   */
  onComplete: (season: FootballSeasonDto) => void;

  /**
   * Sen kauden id, jolla on operaatio käynnissä.
   * Tällä estetään saman rivin toimintojen tuplaklikkailu.
   */
  operationLoading?: string | null;
}

export const SeasonsTable = ({
  seasons,
  onEdit,
  onDelete,
  onActivateToggle,
  onComplete,
  operationLoading,
}: SeasonsTableProps) => {
  const { t } = useTranslation();
  const { divisions } = useDivisions();
  const footballDivisions = useMemo(
    () => divisions.filter((division) => division.sportType === SportsCategory.Football),
    [divisions]
  );
  const { countByCompetitionId } = useInProgressFootballMatches();

  const formatDate = (dateString: string): string => {
    try {
      return new Date(dateString).toLocaleDateString();
    } catch {
      return dateString;
    }
  };

  const getStatusBadge = (season: FootballSeasonDto) => {
    if (season.isCompleted) {
      return (
        <span className="admin-badge admin-badge--completed">
          {t('football.seasons.status.completed', 'Completed')}
        </span>
      );
    }

    if (season.isActive) {
      return (
        <span className="admin-badge admin-badge--active">
          {t('football.seasons.status.active', 'Active')}
        </span>
      );
    }

    return (
      <span className="admin-badge admin-badge--inactive">
        {t('football.seasons.status.inactive', 'Inactive')}
      </span>
    );
  };

  const getActions = (season: FootballSeasonDto) => {
    const actions: {
      label: string;
      onClick: () => void;
      variant?: 'default' | 'danger' | 'status';
      disabled: boolean;
    }[] = [
      {
        label: t('common.edit', 'Edit'),
        onClick: () => onEdit(season),
        disabled: operationLoading === season.id,
      },
    ];

    if (!season.isCompleted) {
      actions.push({
        label: season.isActive
          ? t('football.seasons.deactivate', 'Deactivate')
          : t('football.seasons.activate', 'Activate'),
        onClick: () => onActivateToggle(season),
        variant: 'status',
        disabled: operationLoading === season.id,
      });
    }

    if (season.isActive && !season.isCompleted) {
      actions.push({
        label: t('football.seasons.complete', 'Complete Season'),
        onClick: () => onComplete(season),
        variant: 'status',
        disabled: operationLoading === season.id,
      });
    }

    actions.push({
      label: t('common.delete', 'Delete'),
      onClick: () => onDelete(season),
      variant: 'danger',
      disabled: operationLoading === season.id,
    });

    return actions;
  };

  return (
    <table className="admin-table">
      <thead>
        <tr>
          {/* Multiselect-checkbox-sarake poistettu kokonaan. */}
          <th>{t('football.seasons.fields.name', 'Name')}</th>
          <th>{t('football.seasons.fields.division', 'Division')}</th>
          <th>{t('football.seasons.fields.startDate', 'Starts')}</th>
          <th>{t('football.seasons.fields.endDate', 'Ends')}</th>
          <th>{t('football.seasons.fields.teams', 'Teams')}</th>
          <th>{t('football.seasons.fields.status', 'Status')}</th>
          <th className="admin-table__actions-col">
            {t('common.actions', 'Actions')}
          </th>
        </tr>
      </thead>

      <tbody>
        {seasons.map((season) => {
          const liveCount: number = countByCompetitionId.get(season.id) ?? 0;

          return (
            <tr
              key={season.id}
              className="admin-table__row--clickable"
              onClick={() => onEdit(season)}
              role="button"
              tabIndex={0}
              title={t('football.seasons.actions.openEdit', 'Open and edit season')}
              /**
               * Näppäimistötuki:
               * Enter ja välilyönti avaavat edit-sivun samalla tavalla kuin hiiriklikki.
               */
              onKeyDown={(e) => {
                if (e.key === 'Enter' || e.key === ' ') {
                  e.preventDefault();
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
                      ariaLabel={t(
                        'football.seasons.matchesInProgress',
                        '{{count}} match(es) in progress',
                        { count: liveCount }
                      )}
                    />
                  )}

                  <span>{season.name}</span>
                  <TeamCategoryBadge category={season.teamCategory} />
                </span>
              </td>

              <td>
                <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.25rem' }}>
                  {season.seasonDivisions && season.seasonDivisions.length > 0 ? (
                    season.seasonDivisions.map((seasonDivision) => {
                      const division = footballDivisions.find(
                        (d) => d.id === seasonDivision.divisionId
                      );

                      return (
                        <span
                          key={seasonDivision.divisionId}
                          className="admin-tag admin-tag--blue"
                        >
                          {division?.name || seasonDivision.divisionId}
                        </span>
                      );
                    })
                  ) : (
                    <span className="admin-table__muted">
                      {t('football.seasons.noDivisions', 'No divisions')}
                    </span>
                  )}
                </div>
              </td>

              <td>{formatDate(season.startDate)}</td>

              <td>{formatDate(season.endDate)}</td>

              <td>
                <span className="admin-table__muted">
                  {season.teams?.length || 0}{' '}
                  {t('football.seasons.teamsCount', 'teams')}
                </span>
              </td>

              <td>{getStatusBadge(season)}</td>

              <td
                className="admin-table__actions-col"
                /**
                 * Estetään rivin onClick, kun käyttäjä käyttää Actions-valikkoa.
                 * Muuten esim. delete-napin klikkaus voisi samalla avata edit-sivun.
                 */
                onClick={(e) => e.stopPropagation()}
                onKeyDown={(e) => e.stopPropagation()}
              >
                <ActionsDropdown
                  actions={getActions(season)}
                  ariaLabel={t(
                    'football.seasons.actions.menu',
                    'Season actions menu'
                  )}
                />
              </td>
            </tr>
          );
        })}
      </tbody>
    </table>
  );
};