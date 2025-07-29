import { useEffect, useState, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import './NewsPage.scss';
import NewsCard from './components/NewsCard';
import NewsCardSkeleton from './components/NewsCardSkeleton';
import NewsFilter from './components/NewsFilter';
import { newsService, type NewsArticleDto, type NewsParameters, getMainNewsArticle } from '../../api/news/newsService';
import { useNavigate } from 'react-router-dom';
import Pagination from '../../components/Pagination';

function NewsPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [newsList, setNewsList] = useState<NewsArticleDto[]>([]);
  const [mainNews, setMainNews] = useState<NewsArticleDto | null>(null);
  const [filters, setFilters] = useState<NewsParameters>({
    category: '',
    sportCategory: '',
    searchTerm: ''
  });
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(0);
  const [isLoading, setIsLoading] = useState(false);

  const RetrieveNews = useCallback(async () => {
    setIsLoading(true);
    try {
      const response = await newsService(filters);
      setNewsList(response);
      setTotalCount(response.length);
      setTotalPages(Math.ceil(response.length / pageSize));
    } catch (error) {
      console.error('Failed to fetch news:', error);
    } finally {
      setIsLoading(false);
    }
  }, [filters, pageSize]);

  useEffect(() => {
    RetrieveNews();
  }, [RetrieveNews]);

  useEffect(() => {
    getMainNewsArticle().then(setMainNews);
  }, []);

  // Reset to first page when filters change
  useEffect(() => {
    setCurrentPage(1);
  }, [filters]);

  // Exclude mainNews from otherNews if present
  const otherNews = mainNews
    ? newsList.filter((item) => item.id !== mainNews.id)
    : newsList;

  // Apply pagination to the news list
  const startIndex = (currentPage - 1) * pageSize;
  const endIndex = startIndex + pageSize;
  const paginatedNews = otherNews.slice(startIndex, endIndex);

  const mainNewsBgStyle = mainNews && mainNews.mainImage
    ? { ['--main-news-image' as any]: `url('${mainNews.mainImage}')` } as React.CSSProperties
    : undefined;

  return (
    <PageTemplate title={t('nav.news')} >
      <div
        className={`news-main-bg${mainNews && mainNews.mainImage ? ' has-main-image' : ''}`}
        style={mainNewsBgStyle}
      >
        <div className="news-main-section">
          {mainNews && (
            <div className="main-news-card">
              <div className="main-news-image-container">
                {mainNews.mainImage && (
                  <img src={mainNews.mainImage} alt={mainNews.title} className="main-news-image" />
                )}
              </div>
              <div className="main-news-content">
                <div className="main-news-category">{mainNews.sportCategory}</div>
                <h2 className="main-news-title">{mainNews.title}</h2>
                <div className="main-news-summary">{mainNews.summary}</div>
                <button className="main-news-button" onClick={() => navigate(`/uutiset/${mainNews.id}`)}>
                  {t('newsPage.readMore', 'Lue lisää')}
                </button>
              </div>
            </div>
          )}
        </div>
      </div>

      <div className="news-list-section container">
        <NewsFilter onFilterChange={setFilters} />
        <div className="news-grid">
          {isLoading ? (
            Array.from({ length: pageSize }, (_, index) => (
              <NewsCardSkeleton key={`skeleton-${index}`} />
            ))
          ) : paginatedNews.length === 0 ? (
            <p>{t('newsPage.noNewsFound', 'Ei uutisia vastaaville hakuehdoille.')}</p>
          ) : (
            paginatedNews.map((item) => (
              <NewsCard key={item.id} news={item} />
            ))
          )}
        </div>
        <Pagination
          currentPage={currentPage}
          totalPages={totalPages}
          totalCount={totalCount}
          pageSize={pageSize}
          onPageChange={setCurrentPage}
          onPageSizeChange={setPageSize}
        />
      </div>
    </PageTemplate>
  );
}

export default NewsPage; 