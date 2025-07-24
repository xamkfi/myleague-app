import { useNavigate } from "react-router-dom";
import type { NewsArticleDto } from "../../../api/news/newsService";

export default function NewsCard({ news }: { news: NewsArticleDto }) {
  const navigate = useNavigate();

  return (
    <div className="news-card" onClick={() => navigate(`/uutiset/${news.id}`)}>
      <div className="news-card-image-container">
        <img
          src={news.mainImage}
          alt={news.title}
          className="news-card-image"
        />
      </div>
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
  );
}
