import { useNavigate } from "react-router-dom";
import type { NewsArticleDto } from "../../../api/news/newsService";
import defaultNewsImage from '../../../assets/defaultImage.jpg';

export default function NewsCard({ news }: { news: NewsArticleDto }) {
  const navigate = useNavigate();

  return (
    <div className="news-card" onClick={() => navigate(`/uutiset/${news.id}`)}>
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
            {news.sportCategory && (
              <div className={`sport-category-badge`}>
                <span className="sport-icon"></span>
                <span className="sport-name">{news.sportCategory}</span>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
