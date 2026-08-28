import { useEffect, useState, useCallback, useRef } from 'react';
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
  getMainNewsArticle,
} from '../../api/news/newsService';
import { useAudience } from '../../context/AudienceContext';
import { useNavigate, useSearchParams } from 'react-router-dom';
import Pagination from '../../components/Pagination';
import defaultNewsImage from '../../assets/defaultImage.jpg';
import {
  newsListFiltersFromSearchParams,
  newsListFiltersToSearchParams,
  type NewsListFilters,
} from './newsListFilters';

function NewsPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const { audience } = useAudience();
  const [newsList, setNewsList] = useState<NewsArticleDto[]>([]);
  const [mainNews, setMainNews] = useState<NewsArticleDto | null>(null);
  const filters = newsListFiltersFromSearchParams(searchParams);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(0);
  const [isLoading, setIsLoading] = useState(false);

  const filtersKey = `${filters.category}|${filters.sportCategory}|${filters.tag}|${filters.searchTerm}`;
  const previousFiltersKeyRef = useRef(filtersKey);

  const RetrieveNews = useCallback(async () => {
    setIsLoading(true);
    try {
      const response = await newsService({
        category: filters.category,
        sportCategory: filters.sportCategory,
        tag: filters.tag,
        searchTerm: filters.searchTerm,
        page: currentPage,
        pageSize: pageSize,
        teamCategory: audience.teamCategory,
      });

      if (response && typeof response === 'object' && 'pagination' in response) {
        const paginatedResponse = response as PaginatedNewsResponse;
        setNewsList(paginatedResponse.data);
        setTotalCount(paginatedResponse.pagination.totalCount);
        setTotalPages(paginatedResponse.pagination.totalPages);
        setCurrentPage(paginatedResponse.pagination.currentPage);
        setPageSize(paginatedResponse.pagination.pageSize);
      } else {
        const oldResponse = response as NewsArticleDto[];
        setNewsList(oldResponse);
        setTotalCount(oldResponse.length);
        setTotalPages(Math.ceil(oldResponse.length / pageSize));
      }
    } catch (error) {
      console.error('Failed to fetch news:', error);
    } finally {
      setIsLoading(false);
    }
  }, [filters.category, filters.sportCategory, filters.tag, filters.searchTerm, currentPage, pageSize, audience.teamCategory]);

  useEffect(() => {
    const filtersChanged = previousFiltersKeyRef.current !== filtersKey;
    previousFiltersKeyRef.current = filtersKey;

    if (filtersChanged && currentPage !== 1) {
      setCurrentPage(1);
      return;
    }

    void RetrieveNews();
  }, [RetrieveNews, filtersKey, currentPage]);

  useEffect(() => {
    getMainNewsArticle().then(setMainNews);
  }, []);

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
    filters.category || filters.sportCategory || filters.tag || filters.searchTerm
  );

  const otherNews = mainNews && !hasActiveFilter
    ? newsList.filter((item) => item.id !== mainNews.id)
    : newsList;

  const showMainNews = Boolean(mainNews) && !hasActiveFilter;
  const mainNewsBgStyle = showMainNews
    ? { '--main-news-image': `url('${mainNews?.mainImage || defaultNewsImage}')` } as React.CSSProperties
    : undefined;

  return (
    <div className="news-page">
      <PageTemplate title={t('nav.news')}>
        {showMainNews && mainNews && (
          <div
            className="news-main-bg has-main-image"
            style={mainNewsBgStyle}
          >
            <div className="news-main-section">
              <div className="main-news-card">
                <div className="main-news-image-container">
                  <img
                    src={mainNews.mainImage || defaultNewsImage}
                    alt={mainNews.title}
                    className="main-news-image"
                  />
                </div>
                <div className="main-news-content">
                  <div className="main-news-category">{mainNews.sportCategory}</div>
                  <h2 className="main-news-title">{mainNews.title}</h2>
                  <div className="main-news-summary">{mainNews.summary}</div>
                  <button className="main-news-button" onClick={() => navigate(`/uutiset/${mainNews.id}`)}>
                    {t('newsPage.readMore')}
                  </button>
                </div>
              </div>
            </div>
          </div>
        )}

        <div className="news-list-section">
          <h1 className="news-list-title">{t('newsPage.allNews')}</h1>

          <div className="news-filter-wrapper">
            <NewsFilter filters={filters} onFilterChange={handleFilterChange} />
          </div>

          <div className="news-grid">
            {isLoading ? (
              Array.from({ length: pageSize }, (_, index) => (
                <NewsCardSkeleton key={`skeleton-${index}`} />
              ))
            ) : otherNews.length === 0 ? (
              <div className="no-news-message">
                <p>{t('newsPage.noNewsFound')}</p>
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
