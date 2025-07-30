import { useTranslation } from 'react-i18next';
import './NewsFilter.scss';

export const NewsCategory = {
  None: 'None',
  General: 'General',
  MatchReports: 'MatchReports',
  LeagueNews: 'LeagueNews',
  PlayerUpdates: 'PlayerUpdates',
  TeamNews: 'TeamNews',
  Announcements: 'Announcements',
  Events: 'Events',
  Transfers: 'Transfers',
  Injuries: 'Injuries',
  Awards: 'Awards',
};

export const SportsCategory = {
  None: 'None',
  Floorball: 'Floorball',
  Icehockey: 'Icehockey',
  Football: 'Football',
};

export default function NewsFilter() {
  const { t } = useTranslation();

  return (
    <div className="admin-news-filter">
      
      <div className="filter-controls">
        {/* Category dropdown */}
        <div className="filter-group">
          <label htmlFor="category-filter">{t('admin.news.filter.category', 'Category')}</label>
          <select
            id="category-filter"
            className="filter-select"
            defaultValue=""
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
            defaultValue=""
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
          />
        </div>

        {/* Filter actions */}
        <div className="filter-actions">
          <button className="filter-button filter-apply">
            {t('admin.news.filter.apply', 'Apply Filters')}
          </button>
          <button className="filter-button filter-clear">
            {t('admin.news.filter.clear', 'Clear')}
          </button>
        </div>
      </div>
    </div>
  );
} 