import { useEffect, useState, useCallback, useRef, type CSSProperties } from 'react';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import './NewsPage.scss';
import NewsCard from './components/NewsCard';
import NewsCardSkeleton from './components/NewsCardSkeleton';
import NewsFilter from './components/NewsFilter';
import {
  newsService,
  type NewsArticleDto,
  type PaginatedNewsResponse,
} from '../../api/news/newsService';
import { useAudience } from '../../context/AudienceContext';
import { useNavigate, useSearchParams } from 'react-router-dom';
import Pagination from '../../components/Pagination';
import TeamCategoryBadge from '../../components/TeamCategoryBadge/TeamCategoryBadge';
import defaultNewsImage from '../../assets/defaultImage.jpg';
import {
  newsListFiltersFromSearchParams,
  newsListFiltersToSearchParams,
  type NewsListFilters,
} from './newsListFilters';
import { newsCategoryLabel, newsSportLabel } from '../AdminPage/NewsPage/Utils/newsTaxonomyLabels';

function NewsPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const { audience } = useAudience();
  const [newsList, setNewsList] = useState<NewsArticleDto[]>([]);
  const filters = newsListFiltersFromSearchParams(searchParams);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(0);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const filtersKey = `${audience.teamCategory}|${filters.category}|${filters.sportCategory}|${filters.tag}|${filters.searchTerm}`;
  const previousFiltersKeyRef = useRef(filtersKey);

  const retrieveNews = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await newsService({
        category: filters.category,
        sportCategory: filters.sportCategory,
        tag: filters.tag,
        searchTerm: filters.searchTerm,
        page: currentPage,
        pageSize,
        teamCategory: audience.teamCategory,
      });

      if (response && typeof response === 'object' && 'pagination' in response) {
        const paginatedResponse = response as PaginatedNewsResponse;
        setNewsList(paginatedResponse.data);
        setTotalCount(paginatedResponse.pagination.totalCount);
        setTotalPages(paginatedResponse.pagination.totalPages);
        setCurrentPage(paginatedResponse.pagination.currentPage);
        setPageSize(paginatedResponse.pagination.pageSize);
        return;
      }

      const oldResponse = response as NewsArticleDto[];
      setNewsList(oldResponse);
      setTotalCount(oldResponse.length);
      setTotalPages(Math.ceil(oldResponse.length / pageSize));
    } catch (fetchError: unknown) {
      console.error('Failed to fetch news:', fetchError);
      setNewsList([]);
      setTotalCount(0);
      setTotalPages(0);
      setError(t('newsPage.fetchError'));
    } finally {
      setIsLoading(false);
    }
  }, [
    filters.category,
    filters.sportCategory,
    filters.tag,
    filters.searchTerm,
    currentPage,
    pageSize,
    audience.teamCategory,
    t,
  ]);

  useEffect(() => {
    const filtersChanged = previousFiltersKeyRef.current !== filtersKey;
    previousFiltersKeyRef.current = filtersKey;

    if (filtersChanged && currentPage !== 1) {
      setCurrentPage(1);
      return;
    }

    void retrieveNews();
  }, [retrieveNews, filtersKey, currentPage]);

  const handleFilterChange = useCallback((updated: Partial<NewsListFilters>) => {
    setCurrentPage(1);
    setSearchParams((current) => {
      const currentFilters = newsListFiltersFromSearchParams(current);
      return newsListFiltersToSearchParams({
        ...currentFilters,
        ...updated,
      });
    }, { replace: true });
  }, [setSearchParams]);

  const hasActiveFilter = Boolean(
    filters.category || filters.sportCategory || filters.tag || filters.searchTerm,
  );

  const featuredNews = !hasActiveFilter && currentPage === 1 ? newsList[0] ?? null : null;
  const otherNews = featuredNews
    ? newsList.filter((item) => item.id !== featuredNews.id)
    : newsList;
  const featuredNewsBgStyle = featuredNews
    ? { '--main-news-image': `url('${featuredNews.mainImage || defaultNewsImage}')` } as CSSProperties
    : undefined;

  return (
    <div className="news-page">
      <PageTemplate title={t('nav.news')}>
        {featuredNews && (
          <div
            className="news-main-bg has-main-image"
            style={featuredNewsBgStyle}
          >
            <div className="news-main-section">
              <div className="main-news-card">
                <div className="main-news-image-container">
                  <img
                    src={featuredNews.mainImage || defaultNewsImage}
                    alt={featuredNews.title}
                    className="main-news-image"
                  />
                </div>
                <div className="main-news-content">
                  <div className="main-news-labels">
                    <TeamCategoryBadge category={featuredNews.teamCategory} showAll />
                    {featuredNews.sportCategory && (
                      <div className="main-news-category">
                        {newsSportLabel(t, featuredNews.sportCategory)}
                      </div>
                    )}
                  </div>
                  <h2 className="main-news-title">{featuredNews.title}</h2>
                  {featuredNews.summary && (
                    <div className="main-news-summary">{featuredNews.summary}</div>
                  )}
                  {featuredNews.category && (
                    <p className="main-news-meta">{newsCategoryLabel(t, featuredNews.category)}</p>
                  )}
                  <button className="main-news-button" onClick={() => navigate(`/uutiset/${featuredNews.id}`)}>
                    {t('newsPage.readMore')}
                  </button>
                </div>
              </div>
            </div>
          </div>
        )}

        <div className="news-list-section">
          <div className="news-list-heading">
            <h1 className="news-list-title">{t('newsPage.allNews')}</h1>
            <p className="news-list-audience">{t(audience.i18nKey)}</p>
          </div>

          <div className="news-filter-wrapper">
            <NewsFilter filters={filters} onFilterChange={handleFilterChange} />
          </div>

          {error && (
            <div className="no-news-message">
              <p>{error}</p>
              <button type="button" className="news-filter-clear" onClick={() => void retrieveNews()}>
                {t('newsPage.retry')}
              </button>
            </div>
          )}

          <div className="news-grid">
            {isLoading ? (
              Array.from({ length: pageSize }, (_, index) => (
                <NewsCardSkeleton key={`skeleton-${index}`} />
              ))
            ) : !error && otherNews.length === 0 && !featuredNews ? (
              <div className="no-news-message">
                <p>
                  {hasActiveFilter
                    ? t('newsPage.noNewsFound')
                    : t('newsPage.noNewsForAudience', { audience: t(audience.i18nKey) })}
                </p>
              </div>
            ) : (
              otherNews.map((item) => (
                <NewsCard key={item.id} news={item} />
              ))
            )}
          </div>

          {totalCount > 0 && (
            <div className="news-pagination-wrapper">
              <Pagination
                currentPage={currentPage}
                totalPages={totalPages}
                totalCount={totalCount}
                pageSize={pageSize}
                onPageChange={setCurrentPage}
                onPageSizeChange={setPageSize}
              />
            </div>
          )}
        </div>
      </PageTemplate>
    </div>
  );
}

export default NewsPage;
