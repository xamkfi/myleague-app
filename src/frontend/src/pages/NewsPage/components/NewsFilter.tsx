import { useTranslation } from 'react-i18next';
import { useEffect, useMemo, useState } from 'react';
import { getNewsTags } from '../../../api/news/newsService';
import {
  NEWS_CATEGORY_OPTIONS,
  NEWS_SPORT_CATEGORY_OPTIONS,
  SportsCategory,
} from '../../AdminPage/NewsPage/Utils/NewsFilterContstants';
import type { NewsListFilters } from '../newsListFilters';
import NewsTagCombobox from './NewsTagCombobox';

type NewsFilterProps = {
  filters: NewsListFilters;
  onFilterChange: (filters: Partial<NewsListFilters>) => void;
};

function sportLabelKey(sport: string): string {
  if (sport === SportsCategory.Floorball) return 'newsPage.sportCategory.floorball';
  if (sport === SportsCategory.Icehockey) return 'newsPage.sportCategory.hockey';
  if (sport === SportsCategory.Football) return 'newsPage.sportCategory.football';
  return sport;
}

export default function NewsFilter({ filters, onFilterChange }: NewsFilterProps) {
  const { t } = useTranslation();
  const [searchTerm, setSearchTerm] = useState(filters.searchTerm);
  const [availableTags, setAvailableTags] = useState<string[]>([]);

  useEffect(() => {
    setSearchTerm(filters.searchTerm);
  }, [filters.searchTerm]);

  useEffect(() => {
    let cancelled = false;
    getNewsTags().then((tags) => {
      if (!cancelled) {
        setAvailableTags(tags);
      }
    });
    return () => {
      cancelled = true;
    };
  }, []);

  useEffect(() => {
    const timer = window.setTimeout(() => {
      if (searchTerm !== filters.searchTerm) {
        onFilterChange({ searchTerm });
      }
    }, 300);

    return () => window.clearTimeout(timer);
  }, [searchTerm, filters.searchTerm, onFilterChange]);

  const hasActiveFilter = Boolean(
    filters.category || filters.sportCategory || filters.tag || filters.searchTerm
  );

  const tags = useMemo(() => {
    if (filters.tag && !availableTags.includes(filters.tag)) {
      return [filters.tag, ...availableTags];
    }
    return availableTags;
  }, [availableTags, filters.tag]);

  return (
    <div className="news-filter-panel">
      <select
        value={filters.sportCategory}
        onChange={(event) => onFilterChange({ sportCategory: event.target.value })}
        className={`category-dropdown${filters.sportCategory ? ' is-active' : ''}`}
        aria-label={t('newsPage.filters.sport')}
      >
        <option value="">{t('newsPage.filters.allSports')}</option>
        {NEWS_SPORT_CATEGORY_OPTIONS.map((sport) => (
          <option key={sport} value={sport}>
            {t(sportLabelKey(sport), sport)}
          </option>
        ))}
      </select>

      <select
        value={filters.category}
        onChange={(event) => onFilterChange({ category: event.target.value })}
        className={`category-dropdown${filters.category ? ' is-active' : ''}`}
        aria-label={t('newsPage.filters.category')}
      >
        <option value="">{t('newsPage.filters.allCategories')}</option>
        {NEWS_CATEGORY_OPTIONS.map((category) => (
          <option key={category} value={category}>
            {t(`newsPage.categoryValues.${category}`, category)}
          </option>
        ))}
      </select>

      <NewsTagCombobox
        tags={tags}
        selectedTag={filters.tag}
        onChange={(tag) => onFilterChange({ tag })}
      />

      <div className="search-input">
        <input
          type="search"
          placeholder={t('newsPage.searchPlaceholder')}
          value={searchTerm}
          onChange={(event) => setSearchTerm(event.target.value)}
          className="flex-grow px-4 py-2 outline-none"
        />
      </div>

      {hasActiveFilter && (
        <button
          type="button"
          className="news-filter-clear"
          onClick={() =>
            onFilterChange({ category: '', sportCategory: '', tag: '', searchTerm: '' })
          }
        >
          {t('newsPage.filters.clear')}
        </button>
      )}
    </div>
  );
}
