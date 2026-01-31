import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import type { NewsArticleDto } from '../../api/news/newsService';
import defaultNewsImage from '../../assets/defaultImage.jpg';

interface HomeNewsCardProps {
  news: NewsArticleDto;
}

function HomeNewsCard({ news }: HomeNewsCardProps) {
  const navigate = useNavigate();
  const { t } = useTranslation();

  const formatDate = (dateString: string): string => {
    return new Date(dateString).toLocaleDateString('fi-FI', {
      day: 'numeric',
      month: 'numeric',
      year: 'numeric'
    });
  };

  const handleClick = () => {
    navigate(`/uutiset/${news.id}`);
  };

  return (
    <article className="home-news-card" onClick={handleClick}>
      <div className="home-news-card__image-container">
        <img
          src={news.mainImage || defaultNewsImage}
          alt={news.title}
          className="home-news-card__image"
          onError={(e) => {
            const target = e.target as HTMLImageElement;
            target.src = defaultNewsImage;
          }}
        />
      </div>
      <div className="home-news-card__content">
        <div className="home-news-card__meta">
          {news.sportCategory && (
            <span className="home-news-card__category">{news.sportCategory}</span>
          )}
          <span className="home-news-card__date">{formatDate(news.createdAt)}</span>
        </div>
        <h3 className="home-news-card__title">{news.title}</h3>
        {news.summary && (
          <p className="home-news-card__summary">{news.summary}</p>
        )}
        <span className="home-news-card__read-more">
          {t('homePage.newsSection.readMore', 'Lue lisää')} →
        </span>
      </div>
    </article>
  );
}

export default HomeNewsCard;
