import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import type { NewsArticleDto } from '../../api/news/newsService';
import defaultNewsImage from '../../assets/defaultImage.jpg';
import './MainNewsCard.scss';

interface MainNewsCardProps {
  news: NewsArticleDto;
}

function MainNewsCard({ news }: MainNewsCardProps) {
  const navigate = useNavigate();
  const { t } = useTranslation();

  const mainNewsBgStyle = {
    '--main-news-image': `url('${news.mainImage || defaultNewsImage}')`
  } as React.CSSProperties;

  return (
    <div className="main-news-bg has-main-image" style={mainNewsBgStyle}>
      <div className="main-news-section">
        <div className="main-news-card">
          <div className="main-news-image-container">
            <img
              src={news.mainImage || defaultNewsImage}
              alt={news.title}
              className="main-news-image"
              onError={(e) => {
                const target = e.target as HTMLImageElement;
                target.src = defaultNewsImage;
              }}
            />
          </div>
          <div className="main-news-content">
            {news.sportCategory && (
              <div className="main-news-category">{news.sportCategory}</div>
            )}
            <h2 className="main-news-title">{news.title}</h2>
            {news.summary && (
              <div className="main-news-summary">{news.summary}</div>
            )}
            <button
              className="main-news-button"
              onClick={() => navigate(`/uutiset/${news.id}`)}
            >
              {t('newsPage.readMore', 'Lue lisää')}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}

export default MainNewsCard;
