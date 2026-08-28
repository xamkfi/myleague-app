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

export function SeasonsFilters({
  showActiveOnly,
  onShowActiveOnlyChange,
  divisionFilter,
  onDivisionFilterChange,
  uniqueDivisions,
  categoryFilter,
  onCategoryFilterChange,
}: SeasonsFiltersProps) {
  const { t } = useTranslation();

  return (
    <div className="filters-section">
      <div className="filters-row">
        <div className="filter-group">
          <div className="show-active" onClick={() => onShowActiveOnlyChange(!showActiveOnly)}>
            <input
              type="checkbox"
              checked={showActiveOnly}
              onChange={(event) => onShowActiveOnlyChange(event.target.checked)}
            />
            {t('hockey.seasons.showActiveOnly', 'Show Active Only')}
          </div>
        </div>
        <div className="filter-group">
          <label htmlFor="hockey-division-filter">{t('hockey.seasons.division', 'Division')}:</label>
          <select
            id="hockey-division-filter"
            value={divisionFilter}
            onChange={(event) => onDivisionFilterChange(event.target.value)}
          >
            <option value="all">{t('common.all', 'All')}</option>
            {uniqueDivisions.map((division) => (
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
}
