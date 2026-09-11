import { useTranslation } from 'react-i18next';
import type { TournamentStatusFilter } from '../hooks/useTournamentsManagement';
import TeamCategoryFilter from '../../../../../components/TeamCategoryFilter/TeamCategoryFilter';
import './TournamentsFilters.scss';

interface TournamentsFiltersProps {
  showOngoingOnly: boolean;
  onShowOngoingOnlyChange: (value: boolean) => void;
  statusFilter: TournamentStatusFilter;
  onStatusFilterChange: (value: TournamentStatusFilter) => void;
  uniqueStatuses: string[];
  categoryFilter: string[];
  onCategoryFilterChange: (categories: string[]) => void;
}

export const TournamentsFilters = ({
  showOngoingOnly,
  onShowOngoingOnlyChange,
  statusFilter,
  onStatusFilterChange,
  uniqueStatuses,
  categoryFilter,
  onCategoryFilterChange,
}: TournamentsFiltersProps) => {
  const { t } = useTranslation();

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
    <div className="filters-section">
      <div className="filters-row">
        <div className="filter-group">
          <div className="show-ongoing" onClick={() => onShowOngoingOnlyChange(!showOngoingOnly)}>
            <input
              type="checkbox"
              checked={showOngoingOnly}
              onChange={(e) => onShowOngoingOnlyChange(e.target.checked)}
            />
            {t('floorball.tournaments.showOngoingOnly', 'Show Ongoing Only')}
          </div>
        </div>

        <div className="filter-group">
          <label htmlFor="tournament-status-filter">
            {t('floorball.tournaments.fields.status', 'Status')}:
          </label>
          <select
            id="tournament-status-filter"
            value={statusFilter}
            onChange={(e) => onStatusFilterChange(e.target.value as TournamentStatusFilter)}
          >
            <option value="all">{t('floorball.tournaments.allStatuses', 'All statuses')}</option>
            {uniqueStatuses.map((status) => (
              <option key={status} value={status}>
                {getStatusLabel(status)}
              </option>
            ))}
          </select>
        </div>

        <div className="filter-group">
          <TeamCategoryFilter selected={categoryFilter} onChange={onCategoryFilterChange} />
        </div>
      </div>
    </div>
  );
};
