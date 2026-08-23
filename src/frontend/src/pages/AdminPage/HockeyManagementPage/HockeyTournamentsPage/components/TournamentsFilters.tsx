import { useTranslation } from 'react-i18next';
import './TournamentsFilters.scss';

export type TournamentStatusFilter = 'all' | string;

interface TournamentsFiltersProps {
  showOngoingOnly: boolean;
  onShowOngoingOnlyChange: (value: boolean) => void;
  statusFilter: TournamentStatusFilter;
  onStatusFilterChange: (value: TournamentStatusFilter) => void;
  uniqueStatuses: string[];
}

export function TournamentsFilters({
  showOngoingOnly,
  onShowOngoingOnlyChange,
  statusFilter,
  onStatusFilterChange,
  uniqueStatuses,
}: TournamentsFiltersProps) {
  const { t } = useTranslation();

  const getStatusLabel = (status: string): string => {
    switch (status) {
      case 'Draft':
        return t('hockey.tournaments.statusDraft', 'Draft');
      case 'Published':
        return t('hockey.tournaments.statusPublished', 'Published');
      case 'RegistrationOpen':
        return t('hockey.tournaments.statusRegistrationOpen', 'Registration open');
      case 'Active':
        return t('hockey.tournaments.statusActive', 'Active');
      case 'GroupStage':
        return t('hockey.tournaments.statusGroupStage', 'Group Stage');
      case 'PlayoffStage':
        return t('hockey.tournaments.statusPlayoffStage', 'Playoff Stage');
      case 'Completed':
        return t('hockey.tournaments.statusCompleted', 'Completed');
      case 'Cancelled':
        return t('hockey.tournaments.statusCancelled', 'Cancelled');
      default:
        return status;
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
              onChange={(event) => onShowOngoingOnlyChange(event.target.checked)}
            />
            {t('hockey.tournaments.showOngoingOnly', 'Show Ongoing Only')}
          </div>
        </div>
        <div className="filter-group">
          <label htmlFor="hockey-tournament-status-filter">{t('hockey.tournaments.fields.status', 'Status')}:</label>
          <select
            id="hockey-tournament-status-filter"
            value={statusFilter}
            onChange={(event) => onStatusFilterChange(event.target.value)}
          >
            <option value="all">{t('hockey.tournaments.allStatuses', 'All statuses')}</option>
            {uniqueStatuses.map((status) => (
              <option key={status} value={status}>
                {getStatusLabel(status)}
              </option>
            ))}
          </select>
        </div>
      </div>
    </div>
  );
}
