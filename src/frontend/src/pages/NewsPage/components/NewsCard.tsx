import { useNavigate } from 'react-router-dom';
import type { NewsArticleDto } from '../../../api/news/newsService';
import defaultNewsImage from '../../../assets/defaultImage.jpg';
import { newsListUrl } from '../newsListFilters';
import { SportsCategory } from '../../../types/common/sports';
import { useTranslation } from 'react-i18next';

function sportLabelKey(sport: string): string {
  if (sport === SportsCategory.Floorball) return 'newsPage.sportCategory.floorball';
  if (sport === SportsCategory.Icehockey) return 'newsPage.sportCategory.hockey';
  if (sport === SportsCategory.Football) return 'newsPage.sportCategory.football';
  return sport;
}

export default function NewsCard({ news }: { news: NewsArticleDto }) {
  const navigate = useNavigate();
  const { t } = useTranslation();

  const openArticle = () => navigate(`/uutiset/${news.id}`);

  return (
    <div className="news-card" onClick={openArticle} onKeyDown={(event) => {
      if (event.key === 'Enter' || event.key === ' ') {
        event.preventDefault();
        openArticle();
      }
    }} role="link" tabIndex={0}>
      <div
        className="news-card-background"
        style={{
          backgroundImage: `linear-gradient(0deg, rgba(28, 28, 30, 0.90) 0%, rgba(49, 63, 90, 0.48) 61.06%, rgba(74, 103, 159, 0.00) 100%), url(${news.mainImage || defaultNewsImage})`
        }}
      >
        <div className="news-card-overlay">
          <div className="news-card-content">
            <div className="news-card-date">
              {new Date(news.createdAt).toLocaleDateString()}
            </div>
            <div className="news-card-title">{news.title}</div>
            <div className="news-card-tags">
              {news.sportCategory && (
                <button
                  type="button"
                  className="news-card-tag news-card-tag--sport"
                  onClick={(event) => {
                    event.stopPropagation();
                    navigate(newsListUrl({ sportCategory: news.sportCategory }));
                  }}
                >
                  {t(sportLabelKey(news.sportCategory), news.sportCategory)}
                </button>
              )}
              {news.category && (
                <button
                  type="button"
                  className="news-card-tag news-card-tag--category"
                  onClick={(event) => {
                    event.stopPropagation();
                    navigate(newsListUrl({ category: news.category }));
                  }}
                >
                  {t(`newsPage.categoryValues.${news.category}`, news.category)}
                </button>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
