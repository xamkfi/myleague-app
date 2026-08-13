import { type ChangeEvent } from 'react';
import { useTranslation } from 'react-i18next';
import TeamCategoryFilter from '../../../../components/TeamCategoryFilter/TeamCategoryFilter';
import './NewsFilter.scss';
import { NewsCategory, SportsCategory } from '../Utils/NewsFilterContstants';

export type NewsFilters = {
  category: string;
  sportCategory: string;
  searchTerm: string;
  includeArchived: boolean;
  teamCategories: string[];
};

type NewsFilterProps = {
  filters: NewsFilters;
  onFiltersChange: (updatedFilters: Partial<NewsFilters>) => void;
  onClearFilters: () => void;
};

export default function NewsFilter({ filters, onFiltersChange, onClearFilters }: NewsFilterProps) {
  const { t } = useTranslation();

  const handleSearchChange = (event: ChangeEvent<HTMLInputElement>) => {
    onFiltersChange({ searchTerm: event.target.value });
  };

  const handleClearClick = () => {
    onClearFilters();
  };

  return (
    <div className="admin-news-filter">
      
      <div className="filter-controls">
        {/* Category dropdown */}
        <div className="filter-group">
          <label htmlFor="category-filter">{t('admin.news.filter.category', 'Category')}</label>
          <select
            id="category-filter"
            className="filter-select"
            value={filters.category}
            onChange={(event) => onFiltersChange({ category: event.target.value })}
          >
            <option value="">{t('admin.news.filter.allCategories', 'All Categories')}</option>
            {Object.values(NewsCategory)
              .filter(value => value !== NewsCategory.None)
              .map((category) => (
                <option key={category} value={category}>
                  {category}
                </option>
              ))}
          </select>
        </div>

        {/* Sport category dropdown */}
        <div className="filter-group">
          <label htmlFor="sport-filter">{t('admin.news.filter.sport', 'Sport')}</label>
          <select
            id="sport-filter"
            className="filter-select"
            value={filters.sportCategory}
            onChange={(event) => onFiltersChange({ sportCategory: event.target.value })}
          >
            <option value="">{t('admin.news.filter.allSports', 'All Sports')}</option>
            {Object.values(SportsCategory)
              .filter(value => value !== SportsCategory.None)
              .map((sport) => (
                <option key={sport} value={sport}>
                  {sport}
                </option>
              ))}
          </select>
        </div>
      
        {/* Search input */}
        <div className="filter-group">
          <label htmlFor="search-filter">{t('admin.news.filter.search', 'Search')}</label>
          <input
            id="search-filter"
            type="search"
            className="filter-input"
            placeholder={t('admin.news.filter.searchPlaceholder', 'Search news articles...')}
            value={filters.searchTerm}
            onChange={handleSearchChange}
          />
        </div>

        <div className="filter-group">
          <TeamCategoryFilter
            selected={filters.teamCategories}
            onChange={(categories) => onFiltersChange({ teamCategories: categories })}
          />
        </div>

        <div className="filter-group filter-checkbox">
          <label htmlFor="archived-filter">
            <input
              id="archived-filter"
              type="checkbox"
              checked={filters.includeArchived}
              onChange={(event) =>
                onFiltersChange({ includeArchived: event.target.checked })
              }
            />
            {t('admin.news.filter.includeArchived', 'Include archived')}
          </label>
        </div>

        {/* Filter actions */}
        <div className="filter-actions single-button">
          <button
            className="filter-button filter-clear"
            type="button"
            onClick={handleClearClick}
          >
            {t('admin.news.filter.clear', 'Clear')}
          </button>
        </div>
      </div>
    </div>
  );
}