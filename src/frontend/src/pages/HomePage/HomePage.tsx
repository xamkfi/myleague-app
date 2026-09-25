import { useCallback, useEffect, useState } from 'react';
import HomeNewsSection from '../../components/HomeNewsSection/HomeNewsSection';
import NewsHeroCarousel from '../../components/NewsHeroCarousel/NewsHeroCarousel';
import MatchesPanel from '../../components/MatchesPanel/MatchesPanel';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import { newsService, type NewsArticleDto, type PaginatedNewsResponse } from '../../api/news/newsService';
import { useAudience } from '../../context/AudienceContext';
import { useTranslation } from 'react-i18next';
import './HomePage.scss';

function unwrapNewsList(response: PaginatedNewsResponse | NewsArticleDto[]): NewsArticleDto[] {
  if (response && typeof response === 'object' && 'pagination' in response) {
    return response.data;
  }

  return response.slice(0, 24);
}

function HomePage() {
  const { t } = useTranslation();
  const { audience } = useAudience();
  const [newsArticles, setNewsArticles] = useState<NewsArticleDto[]>([]);
  const [isLoadingNews, setIsLoadingNews] = useState(true);
  const [newsError, setNewsError] = useState<string | null>(null);

  const fetchNews = useCallback(async () => {
    try {
      setIsLoadingNews(true);
      setNewsError(null);
      const response = await newsService({
        page: 1,
        pageSize: 24,
        includeArchived: false,
      });
      setNewsArticles(unwrapNewsList(response));
    } catch (error) {
      console.error('Failed to fetch news:', error);
      setNewsError(t('homePage.newsSection.error', 'Uutisten lataaminen epäonnistui'));
      setNewsArticles([]);
    } finally {
      setIsLoadingNews(false);
    }
  }, [t]);

  useEffect(() => {
    void fetchNews();
  }, [fetchNews]);

  const audienceHeroNews = newsArticles.filter(
    (article) => !article.teamCategory || article.teamCategory === audience.teamCategory,
  );
  const heroNews = (audienceHeroNews.length > 0 ? audienceHeroNews : newsArticles).slice(0, 5);

  return (
    <div className="home-page-wrapper">
      <PageTemplate title="Home">
        <div className="home-page">
          {!isLoadingNews && heroNews.length > 0 && (
            <div className="hero-news-container">
              <NewsHeroCarousel newsArticles={heroNews} />
            </div>
          )}

          {isLoadingNews && (
            <div className="hero-news-container hero-news-container--loading">
              <div className="main-news-skeleton">
                <div className="skeleton-image" />
                <div className="skeleton-content">
                  <div className="skeleton-category" />
                  <div className="skeleton-title" />
                  <div className="skeleton-summary" />
                  <div className="skeleton-button" />
                </div>
              </div>
            </div>
          )}

          <div className="main-content">
            <div className="news-section-container">
              <HomeNewsSection
                articles={newsArticles}
                isLoading={isLoadingNews}
                error={newsError}
                onRetry={fetchNews}
              />
            </div>
            <div className="sidebar-container">
              <MatchesPanel />
            </div>
          </div>
        </div>
      </PageTemplate>
    </div>
  );
}

export default HomePage;
