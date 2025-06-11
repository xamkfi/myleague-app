import { useNavigate } from "react-router-dom";
import type { NewsArticleDto } from "../../../api/news/newsService";

export default function NewsCard({ news }: { news: NewsArticleDto }) {

  const navigate = useNavigate();
  
  return (
    <div className="rounded-2xl overflow-hidden shadow-md bg-white cursor-pointer transition-transform duration-300 hover:shadow-lg hover:scale-101" onClick={()=>navigate(`/uutiset/${news.id}`)}>
      <img
        src={news.mainImage}
        alt={news.title}
        className="w-full h-48 object-cover"
      />
      <div className="p-4 space-y-2">
        <div className="text-xs text-gray-500 uppercase">
          {news.sportCategory} • {news.category}
        </div>
        <h2 className="text-lg font-semibold">{news.title}</h2>
        <p className="text-sm text-gray-700">{news.summary}</p>
        <div className="flex items-center text-xs text-gray-500 mt-2 gap-4">
          <span className="flex items-center gap-1">{news.author}</span>
          <span className="flex items-center gap-1">
            {new Date(news.createdAt).toLocaleDateString()}
          </span>
        </div>
        <div className="flex flex-wrap gap-2 mt-2">
          {news.tags.map((tag) => (
            <span
              key={tag}
              className="bg-gray-100 text-xs text-gray-600 px-2 py-1 rounded-full"
            >
              {tag}
            </span>
          ))}
        </div>
      </div>
    </div>
  );
}
