import { useEffect, useState, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import './NewsPage.scss';
import NewsCard from './components/NewsCard';
import NewsCardSkeleton from './components/NewsCardSkeleton';
import NewsFilter from './components/NewsFilter';
import { newsService, type NewsArticleDto, type NewsParameters, type PaginatedNewsResponse, getMainNewsArticle } from '../../api/news/newsService';
import { useNavigate } from 'react-router-dom';
import Pagination from '../../components/Pagination';
import defaultNewsImage from '../../assets/defaultImage.jpg';

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
      const response = await newsService({
        ...filters,
        page: currentPage,
        pageSize: pageSize
      });
      
      console.log('API Response:', response);
      
      // Handle the paginated response structure
      if (response && typeof response === 'object' && 'pagination' in response) {
        // New paginated response format with pagination object
        const paginatedResponse = response as PaginatedNewsResponse;
        console.log('Pagination data:', paginatedResponse.pagination);
        
        setNewsList(paginatedResponse.data);
        setTotalCount(paginatedResponse.pagination.totalCount);
        setTotalPages(paginatedResponse.pagination.totalPages);
        setCurrentPage(paginatedResponse.pagination.currentPage);
        setPageSize(paginatedResponse.pagination.pageSize);
        
        console.log('Set totalCount to:', paginatedResponse.pagination.totalCount);
        console.log('Set totalPages to:', paginatedResponse.pagination.totalPages);
      } else {
        // Fallback for old format
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
  }, [filters, currentPage, pageSize]);

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

    const mainNewsBgStyle = mainNews
    ? { '--main-news-image': `url('${mainNews.mainImage || defaultNewsImage}')` } as React.CSSProperties
    : undefined;

  return (
    <div className="news-page">
      <PageTemplate title={t('nav.news')} >
        <div
          className={`news-main-bg${mainNews ? ' has-main-image' : ''}`}
          style={mainNewsBgStyle}
        >
        <div className="news-main-section">
          {mainNews && (
            <div className="main-news-card">
              <div className="main-news-image-container">
                {mainNews.mainImage ? (
                  <img src={mainNews.mainImage} alt={mainNews.title} className="main-news-image" />
                ) : (
                  <img src={defaultNewsImage} alt="Default News Image" className="main-news-image" />
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

      <div className="news-list-section">
        <h1 className="news-list-title">KAIKKI UUTISET</h1>
        
        {/* News Filter */}
        <div className="news-filter-wrapper">
          <NewsFilter onFilterChange={setFilters} />
        </div>
        
        {/* News Grid */}
        <div className="news-grid">
          {isLoading ? (
            Array.from({ length: pageSize }, (_, index) => (
              <NewsCardSkeleton key={`skeleton-${index}`} />
            ))
          ) : otherNews.length === 0 ? (
            <div className="no-news-message">
              <p>{t('newsPage.noNewsFound', 'Ei uutisia vastaaville hakuehdoille.')}</p>
            </div>
          ) : (
            otherNews.map((item) => (
              <NewsCard key={item.id} news={item} />
            ))
          )}
        </div>
        
        {/* Pagination */}
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