import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import type { CalendarFilters as FiltersType } from '../../../types/calendar';
import { CALENDAR_STATUSES } from '../../../types/calendar';
import './CalendarFilters.scss';

interface SeasonOption {
  id: string;
  name: string;
}

interface CalendarFiltersProps {
  filters: FiltersType;
  onFiltersChange: (filters: FiltersType) => void;
  seasons: SeasonOption[];
}

export default function CalendarFilters({ filters, onFiltersChange, seasons }: CalendarFiltersProps) {
  const { t } = useTranslation();
  const [mobileOpen, setMobileOpen] = useState(false);

  const activeCount = [
    filters.statuses.length > 0,
    filters.competitionId !== null,
    filters.teamSearch.length > 0,
  ].filter(Boolean).length;

  const toggleStatus = (status: string) => {
    const next = filters.statuses.includes(status)
      ? filters.statuses.filter((s) => s !== status)
      : [...filters.statuses, status];
    onFiltersChange({ ...filters, statuses: next });
  };

  const handleSeasonChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    onFiltersChange({ ...filters, competitionId: e.target.value || null });
  };

  const handleTeamSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    onFiltersChange({ ...filters, teamSearch: e.target.value });
  };

  const clearFilters = () => {
    onFiltersChange({
      ...filters,
      statuses: [],
      competitionId: null,
      teamSearch: '',
    });
  };

  const hasActiveFilters = activeCount > 0;

  const statusLabels: Record<string, string> = {
    scheduled: t('eventCalendarPage.status.scheduled'),
    live: t('eventCalendarPage.status.live'),
    completed: t('eventCalendarPage.status.completed'),
  };

  return (
    <div className="calendar-filters">
      <button
        type="button"
        className="calendar-filters__mobile-toggle"
        onClick={() => setMobileOpen(!mobileOpen)}
      >
        <span>{t('eventCalendarPage.filters.showFilters')}</span>
        {activeCount > 0 && (
          <span className="calendar-filters__badge">{activeCount}</span>
        )}
        <span className={`calendar-filters__chevron ${mobileOpen ? 'calendar-filters__chevron--open' : ''}`}>
          ›
        </span>
      </button>

      <div className={`calendar-filters__body ${mobileOpen ? 'calendar-filters__body--open' : ''}`}>
        <div className="calendar-filters__section">
          <label className="calendar-filters__label">
            {t('eventCalendarPage.filters.status')}
          </label>
          <div className="calendar-filters__chips">
            {CALENDAR_STATUSES.map((status) => (
              <button
                key={status}
                type="button"
                className={`calendar-filters__chip ${
                  filters.statuses.includes(status) ? 'calendar-filters__chip--active' : ''
                }`}
                onClick={() => toggleStatus(status)}
              >
                <span className={`calendar-filters__chip-dot calendar-filters__chip-dot--${status}`} />
                {statusLabels[status]}
              </button>
            ))}
          </div>
        </div>

        {seasons.length > 0 && (
          <div className="calendar-filters__section">
            <label className="calendar-filters__label" htmlFor="calendar-season-filter">
              {t('eventCalendarPage.filters.season')}
            </label>
            <select
              id="calendar-season-filter"
              className="calendar-filters__select"
              value={filters.competitionId ?? ''}
              onChange={handleSeasonChange}
            >
              <option value="">{t('eventCalendarPage.filters.allSeasons')}</option>
              {seasons.map((s) => (
                <option key={s.id} value={s.id}>{s.name}</option>
              ))}
            </select>
          </div>
        )}

        <div className="calendar-filters__section">
          <label className="calendar-filters__label" htmlFor="calendar-team-search">
            {t('eventCalendarPage.filters.team')}
          </label>
          <input
            id="calendar-team-search"
            type="text"
            className="calendar-filters__input"
            placeholder={t('eventCalendarPage.filters.teamPlaceholder')}
            value={filters.teamSearch}
            onChange={handleTeamSearchChange}
          />
        </div>

        {hasActiveFilters && (
          <button
            type="button"
            className="calendar-filters__clear"
            onClick={clearFilters}
          >
            {t('eventCalendarPage.filters.clear')}
          </button>
        )}
      </div>
    </div>
  );
}
