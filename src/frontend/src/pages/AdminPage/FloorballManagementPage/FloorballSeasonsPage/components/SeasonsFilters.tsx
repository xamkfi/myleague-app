import { useTranslation } from 'react-i18next';
import './SeasonsFilters.scss';

interface SeasonsFiltersProps {
  showActiveOnly: boolean;
  onShowActiveOnlyChange: (value: boolean) => void;
  divisionFilter: string;
  onDivisionFilterChange: (value: string) => void;
  uniqueDivisions: string[];
}

export const SeasonsFilters = ({
  showActiveOnly,
  onShowActiveOnlyChange,
  divisionFilter,
  onDivisionFilterChange,
  uniqueDivisions
}: SeasonsFiltersProps) => {
  const { t } = useTranslation();

  return (
    <div className="filters-section">
      <div className="filters-row">
        <div className="filter-group">
          <label>
            <input
              type="checkbox"
              checked={showActiveOnly}
              onChange={(e) => onShowActiveOnlyChange(e.target.checked)}
            />
            {t('floorball.seasons.showActiveOnly', 'Show Active Only')}
          </label>
        </div>
        
        <div className="filter-group">
          <label htmlFor="division-filter">{t('floorball.seasons.division', 'Division')}:</label>
          <select
            id="division-filter"
            value={divisionFilter}
            onChange={(e) => onDivisionFilterChange(e.target.value)}
          >
            <option value="all">{t('common.all', 'All')}</option>
            {uniqueDivisions.map(division => (
              <option key={division} value={division}>
                {division}
              </option>
            ))}
          </select>
        </div>
      </div>
    </div>
  );
}; 