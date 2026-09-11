import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { newsService, type NewsArticleDto, type PaginatedNewsResponse } from '../../api/news/newsService';
import { useAudience } from '../../context/AudienceContext';
import HomeNewsCard from './HomeNewsCard';
import './HomeNewsSection.scss';

function HomeNewsSection() {
  const { t } = useTranslation();
  const { audience } = useAudience();
  const [newsArticles, setNewsArticles] = useState<NewsArticleDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchNews = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      
      const response = await newsService({
        page: 1,
        pageSize: 10,
        includeArchived: false,
        teamCategory: audience.teamCategory,
      });

      if (response && typeof response === 'object' && 'pagination' in response) {
        const paginatedResponse = response as PaginatedNewsResponse;
        setNewsArticles(paginatedResponse.data);
      } else {
        const oldResponse = response as NewsArticleDto[];
        setNewsArticles(oldResponse.slice(0, 10));
      }
    } catch (err) {
      console.error('Failed to fetch news:', err);
      setError(t('homePage.newsSection.error', 'Uutisten lataaminen epäonnistui'));
    } finally {
      setIsLoading(false);
    }
  }, [t, audience.teamCategory]);

  useEffect(() => {
    fetchNews();
  }, [fetchNews]);

  if (isLoading) {
    return (
      <div className="home-news-section">
        <h2 className="home-news-section__title">
          {t('homePage.newsSection.title', 'Ajankohtaista')}
        </h2>
        <div className="home-news-section__list">
          {Array.from({ length: 5 }, (_, index) => (
            <div key={`skeleton-${index}`} className="home-news-card home-news-card--skeleton">
              <div className="home-news-card__image-container skeleton-image" />
              <div className="home-news-card__content">
                <div className="skeleton-meta" />
                <div className="skeleton-title" />
                <div className="skeleton-summary" />
                <div className="skeleton-summary skeleton-summary--short" />
              </div>
            </div>
          ))}
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="home-news-section">
        <h2 className="home-news-section__title">
          {t('homePage.newsSection.title', 'Ajankohtaista')}
        </h2>
        <div className="home-news-section__error">
          <p>{error}</p>
          <button onClick={fetchNews} className="home-news-section__retry-btn">
            {t('homePage.newsSection.retry', 'Yritä uudelleen')}
          </button>
        </div>
      </div>
    );
  }

  if (newsArticles.length === 0) {
    return (
      <div className="home-news-section">
        <h2 className="home-news-section__title">
          {t('homePage.newsSection.title', 'Ajankohtaista')}
        </h2>
        <div className="home-news-section__empty">
          <p>{t('homePage.newsSection.noNews', 'Ei uutisia saatavilla')}</p>
        </div>
      </div>
    );
  }

  return (
    <div className="home-news-section">
      <h2 className="home-news-section__title">
        {t('homePage.newsSection.title', 'Ajankohtaista')}
      </h2>
      <div className="home-news-section__list">
        {newsArticles.map((news) => (
          <HomeNewsCard key={news.id} news={news} />
        ))}
      </div>
    </div>
  );
}

export default HomeNewsSection;
