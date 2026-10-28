import { useTranslation } from 'react-i18next';
import SegmentedControl, { type SegmentedOption } from '../../../../components/SegmentedControl/SegmentedControl';
import SportIcon from '../../../../components/SportIcon/SportIcon';
import TeamCategoryFilter from '../../../../components/TeamCategoryFilter/TeamCategoryFilter';
import type { SportKind } from '../../../../utils/sportRoutes';
import { NEWS_CATEGORY_OPTIONS } from '../Utils/NewsFilterContstants';
import { hasActiveNewsFilters, type NewsFilters } from '../Utils/newsAdminFilters';
import { newsCategoryLabel, newsSportLabel } from '../Utils/newsTaxonomyLabels';
import { sportCategoryForKind, sportKindForCategory } from './newsForm';
import './NewsFilter.scss';

type SportChoice = SportKind | 'all';
type ArchiveChoice = 'all' | 'published';

const SPORT_CHOICES: SportChoice[] = ['all', 'floorball', 'football', 'hockey'];

type NewsFilterProps = {
  filters: NewsFilters;
  onFiltersChange: (updatedFilters: Partial<NewsFilters>) => void;
  onClearFilters: () => void;
};

/** Search and filters above the admin news list. */
export default function NewsFilter({ filters, onFiltersChange, onClearFilters }: NewsFilterProps) {
  const { t } = useTranslation();
  const selectedSport: SportChoice = sportKindForCategory(filters.sportCategory) ?? 'all';

  const sportOptions: SegmentedOption<SportChoice>[] = SPORT_CHOICES.map((choice) =>
    choice === 'all'
      ? { value: choice, label: t('admin.news.filter.allSports') }
      : {
          value: choice,
          label: newsSportLabel(t, sportCategoryForKind(choice)),
          icon: (active: boolean) => <SportIcon sport={choice} size="sm" inverted={active} decorative />,
        },
  );

  const archiveOptions: SegmentedOption<ArchiveChoice>[] = [
    { value: 'all', label: t('admin.news.filter.statusAll') },
    { value: 'published', label: t('admin.news.filter.statusPublished') },
  ];

  return (
    <div className="news-toolbar">
      <div className="news-toolbar__row">
        <label className="news-toolbar__search">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" aria-hidden="true">
            <circle cx="11" cy="11" r="7" />
            <path d="m20 20-3.5-3.5" />
          </svg>
          <input
            type="search"
            className="news-toolbar__search-input"
            value={filters.searchTerm}
            onChange={(event) => onFiltersChange({ searchTerm: event.target.value })}
            placeholder={t('admin.news.filter.searchPlaceholder')}
            aria-label={t('admin.news.filter.search')}
          />
        </label>
        <SegmentedControl
          options={sportOptions}
          value={selectedSport}
          onChange={(choice) =>
            onFiltersChange({ sportCategory: choice === 'all' ? '' : sportCategoryForKind(choice) })
          }
          ariaLabel={t('admin.news.filter.sport')}
          tone="sport"
          className="news-toolbar__sports"
        />
      </div>

      <div className="news-toolbar__row news-toolbar__row--secondary">
        <select
          className="news-toolbar__select"
          value={filters.category}
          onChange={(event) => onFiltersChange({ category: event.target.value })}
          aria-label={t('admin.news.filter.category')}
        >
          <option value="">{t('admin.news.filter.allCategories')}</option>
          {NEWS_CATEGORY_OPTIONS.map((category) => (
            <option key={category} value={category}>{newsCategoryLabel(t, category)}</option>
          ))}
        </select>

        <SegmentedControl
          options={archiveOptions}
          value={filters.includeArchived ? 'all' : 'published'}
          onChange={(choice) => onFiltersChange({ includeArchived: choice === 'all' })}
          ariaLabel={t('admin.news.filter.status')}
          size="sm"
        />

        <TeamCategoryFilter
          selected={filters.teamCategories}
          onChange={(categories) => onFiltersChange({ teamCategories: categories })}
        />

        {hasActiveNewsFilters(filters) && (
          <button type="button" className="news-toolbar__clear" onClick={onClearFilters}>
            {t('admin.news.filter.clear')}
          </button>
        )}
      </div>
    </div>
  );
}
