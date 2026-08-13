import { useTranslation } from 'react-i18next';
import TeamCategoryFilter from '../../../../../components/TeamCategoryFilter/TeamCategoryFilter';
import './SeasonsFilters.scss';

interface SeasonsFiltersProps {
  showActiveOnly: boolean;
  onShowActiveOnlyChange: (value: boolean) => void;
  divisionFilter: string;
  onDivisionFilterChange: (value: string) => void;
  uniqueDivisions: string[];
  categoryFilter: string[];
  onCategoryFilterChange: (categories: string[]) => void;
}

export const SeasonsFilters = ({
  showActiveOnly,
  onShowActiveOnlyChange,
  divisionFilter,
  onDivisionFilterChange,
  uniqueDivisions,
  categoryFilter,
  onCategoryFilterChange
}: SeasonsFiltersProps) => {
  const { t } = useTranslation();

  return (
    <div className="filters-section">
      <div className="filters-row">
        <div className="filter-group">
          <div className="show-active" onClick={() => onShowActiveOnlyChange(!showActiveOnly)}>
            <input
              type="checkbox"
              checked={showActiveOnly}
              onChange={(e) => onShowActiveOnlyChange(e.target.checked)}
            />
            {t('football.seasons.showActiveOnly', 'Show Active Only')}
          </div>
        </div>
        
        <div className="filter-group">
          <label htmlFor="division-filter">{t('football.seasons.division', 'Division')}:</label>
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

        <div className="filter-group">
          <TeamCategoryFilter selected={categoryFilter} onChange={onCategoryFilterChange} />
        </div>
      </div>
    </div>
  );
}; 