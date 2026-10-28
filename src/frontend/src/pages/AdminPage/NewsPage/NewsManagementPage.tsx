import { useCallback, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useLocation, useNavigate } from 'react-router-dom';
import { createAdminFromState } from '../../../utils/adminReturnTo';
import PageTemplate from '../../../components/PageTemplate/AdminPageTemplate';
import Button from '../../../components/Button/Button';
import AddIcon from '../../../assets/basicIcons/add.svg';
import { useDebouncedValue } from '../../../hooks/useDebouncedValue';
import NewsFilter from './components/NewsFilter';
import NewsList from './components/NewsList';
import { EMPTY_NEWS_FILTERS, type NewsFilters } from './Utils/newsAdminFilters';
import './NewsManagementPage.scss';

const SEARCH_DEBOUNCE_MS = 400;

/** Admin list of news articles: search, filters, bulk actions and a link to the editor. */
export default function NewsManagementPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const location = useLocation();
  const [filters, setFilters] = useState<NewsFilters>(EMPTY_NEWS_FILTERS);
  const [totalCount, setTotalCount] = useState<number | null>(null);
  const debouncedSearch: string = useDebouncedValue(filters.searchTerm, SEARCH_DEBOUNCE_MS);

  // Only the search text is debounced; the other filters apply at once.
  const appliedFilters: NewsFilters = useMemo(
    () => ({ ...filters, searchTerm: debouncedSearch.trim() }),
    [filters, debouncedSearch],
  );

  const handleFiltersChange = (updated: Partial<NewsFilters>): void => {
    setFilters((previous) => ({ ...previous, ...updated }));
  };

  const handleClearFilters = (): void => {
    setFilters(EMPTY_NEWS_FILTERS);
  };

  const handleCreateNew = (): void => {
    navigate('/admin/news/create', { state: createAdminFromState(location.pathname, location.search) });
  };

  const handleTotalChange = useCallback((total: number): void => {
    setTotalCount(total);
  }, []);

  return (
    <PageTemplate title={t('admin.news.pageTitle')}>
      <div className="news-admin">
        <header className="news-admin__header">
          <div>
            <h1 className="news-admin__title font-title">{t('admin.news.pageTitle')}</h1>
            {totalCount !== null && (
              <p className="news-admin__count">{t('admin.news.totalCount', { count: totalCount })}</p>
            )}
          </div>
          <Button iconLeft={AddIcon} rounded="pill" onClick={handleCreateNew}>
            {t('admin.news.createNew')}
          </Button>
        </header>

        <NewsFilter filters={filters} onFiltersChange={handleFiltersChange} onClearFilters={handleClearFilters} />

        <NewsList
          filters={appliedFilters}
          onClearFilters={handleClearFilters}
          onCreate={handleCreateNew}
          onTotalChange={handleTotalChange}
        />
      </div>
    </PageTemplate>
  );
}
