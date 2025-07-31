import { useNavigate } from "react-router-dom";
import type { NewsArticleDto } from "../../../api/news/newsService";
import defaultNewsImage from '../../../assets/defaultImage.jpg';

export default function NewsCard({ news }: { news: NewsArticleDto }) {
  const navigate = useNavigate();

  const getSportIcon = (sportCategory: string) => {
    switch (sportCategory?.toLowerCase()) {
      case 'floorball':
        return '🏒';
      case 'icehockey':
        return '🏒';
      case 'football':
        return '⚽';
      default:
        return '🏆';
    }
  };

  const getSportColor = (sportCategory: string) => {
    switch (sportCategory?.toLowerCase()) {
      case 'floorball':
        return 'floorball';
      case 'icehockey':
        return 'icehockey';
      case 'football':
        return 'football';
      default:
        return 'default';
    }
  };

  return (
    <div className="news-card" onClick={() => navigate(`/uutiset/${news.id}`)}>
      <div 
        className="news-card-background"
        style={{
          backgroundImage: `linear-gradient(0deg, rgba(28, 28, 30, 0.90) 0%, rgba(49, 63, 90, 0.48) 61.06%, rgba(74, 103, 159, 0.00) 100%), url(${news.mainImage || defaultNewsImage})`
        }}
      >
        <div className="news-card-overlay">
          {news.sportCategory && (
            <div className={`sport-category-badge ${getSportColor(news.sportCategory)}`}>
              <span className="sport-icon">{getSportIcon(news.sportCategory)}</span>
              <span className="sport-name">{news.sportCategory}</span>
            </div>
          )}
          <div className="news-card-content">
            <div className="news-card-date">
              {new Date(news.createdAt).toLocaleDateString()}
            </div>
            <div className="news-card-title">{news.title}</div>
            <div className="news-card-tags">
              {news.tags.map((tag) => (
                <span key={tag}>#{tag}</span>
              ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
