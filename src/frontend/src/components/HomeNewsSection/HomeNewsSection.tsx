import { useTranslation } from 'react-i18next';
import type { NewsArticleDto } from '../../api/news/newsService';
import HomeNewsCard from './HomeNewsCard';
import './HomeNewsSection.scss';

type HomeNewsSectionProps = {
  articles: NewsArticleDto[];
  isLoading: boolean;
  error: string | null;
  onRetry: () => void;
};

function HomeNewsSection({ articles, isLoading, error, onRetry }: HomeNewsSectionProps) {
  const { t } = useTranslation();

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
          <button type="button" onClick={onRetry} className="home-news-section__retry-btn">
            {t('homePage.newsSection.retry', 'Yritä uudelleen')}
          </button>
        </div>
      </div>
    );
  }

  if (articles.length === 0) {
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
        {articles.map((news) => (
          <HomeNewsCard key={news.id} news={news} />
        ))}
      </div>
    </div>
  );
}

export default HomeNewsSection;
