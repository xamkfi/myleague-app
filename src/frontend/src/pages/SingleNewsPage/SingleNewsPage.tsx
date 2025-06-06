import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import mockData from '../NewsPage/mockNews.json';
import './SingleNewsPage.scss';

interface NewsData {
  id: string;
  title: string;
  contentHtml: string;
  summary?: string;
  imageUrls: string[];
  author?: string;
  createdAt: string;
  updatedAt?: string | null;
  category?: string;
  sportCategory?: string;
  tags: string[];
  isArchived: boolean;
}

function SingleNewsPage() {
  const { t } = useTranslation();
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [news, setNews] = useState<NewsData | null>(null);
  const [currentImageIndex, setCurrentImageIndex] = useState(0);

  useEffect(() => {
    // Find the news item by ID
    const foundNews = mockData.news.find((item) => item.id === id);
    if (foundNews) {
      setNews(foundNews);
    } else {
      // If news not found, redirect to news page
      navigate('/uutiset');
    }
  }, [id, navigate]);

  if (!news) {
    return (
      <PageTemplate title={t('nav.news')}>
        <div className="flex justify-center items-center h-64">
          <div className="text-lg text-gray-500">Loading...</div>
        </div>
      </PageTemplate>
    );
  }

  const handleImageNavigation = (direction: 'prev' | 'next') => {
    if (direction === 'prev') {
      setCurrentImageIndex((prev) =>
        prev === 0 ? news.imageUrls.length - 1 : prev - 1
      );
    } else {
      setCurrentImageIndex((prev) =>
        prev === news.imageUrls.length - 1 ? 0 : prev + 1
      );
    }
  };

  return (
    <PageTemplate title={news.title}>
      <div className="single-news-page max-w-4xl mx-auto">
        {/* Back button */}
        <button
          onClick={() => navigate('/uutiset')}
          className="mb-6 flex items-center gap-2 text-blue-600 hover:text-blue-800 transition-colors"
        >
          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
          </svg>
          {t('common.back', 'Back to News')}
        </button>

        {/* Article header */}
        <div className="mb-8">
          <div className="flex flex-wrap gap-2 mb-4">
            {news.sportCategory && (
              <span className="bg-blue-100 text-blue-800 text-sm font-medium px-3 py-1 rounded-full">
                {news.sportCategory}
              </span>
            )}
            {news.category && (
              <span className="bg-green-100 text-green-800 text-sm font-medium px-3 py-1 rounded-full">
                {news.category}
              </span>
            )}
          </div>
          
          <h1 className="text-3xl md:text-4xl font-bold text-gray-900 mb-4 leading-tight">
            {news.title}
          </h1>

          {news.summary && (
            <p className="text-xl text-gray-600 mb-6 leading-relaxed">
              {news.summary}
            </p>
          )}

          <div className="flex flex-wrap items-center gap-4 text-sm text-gray-500 mb-6">
            {news.author && (
              <div className="flex items-center gap-2">
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                </svg>
                <span>{news.author}</span>
              </div>
            )}
            <div className="flex items-center gap-2">
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
              </svg>
              <span>{new Date(news.createdAt).toLocaleDateString()}</span>
            </div>
            {news.updatedAt && (
              <div className="flex items-center gap-2">
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
                </svg>
                <span>Updated: {new Date(news.updatedAt).toLocaleDateString()}</span>
              </div>
            )}
          </div>
        </div>

        {/* Image gallery */}
        {news.imageUrls.length > 0 && (
          <div className="mb-8">
            <div className="relative rounded-lg overflow-hidden bg-gray-100">
              <img
                src={news.imageUrls[currentImageIndex]}
                alt={news.title}
                className="w-full h-64 md:h-96 object-cover"
                onError={(e) => {
                  const target = e.target as HTMLImageElement;
                  target.src = 'https://via.placeholder.com/800x400?text=Image+Not+Found';
                }}
              />
              
              {news.imageUrls.length > 1 && (
                <>
                  <button
                    onClick={() => handleImageNavigation('prev')}
                    className="absolute left-2 top-1/2 transform -translate-y-1/2 bg-black bg-opacity-50 text-white p-2 rounded-full hover:bg-opacity-70 transition-opacity"
                  >
                    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
                    </svg>
                  </button>
                  <button
                    onClick={() => handleImageNavigation('next')}
                    className="absolute right-2 top-1/2 transform -translate-y-1/2 bg-black bg-opacity-50 text-white p-2 rounded-full hover:bg-opacity-70 transition-opacity"
                  >
                    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                    </svg>
                  </button>
                  
                  {/* Image indicators */}
                  <div className="absolute bottom-4 left-1/2 transform -translate-x-1/2 flex gap-2">
                    {news.imageUrls.map((_, index) => (
                      <button
                        key={index}
                        onClick={() => setCurrentImageIndex(index)}
                        className={`w-2 h-2 rounded-full transition-colors ${
                          index === currentImageIndex ? 'bg-white' : 'bg-white bg-opacity-50'
                        }`}
                      />
                    ))}
                  </div>
                </>
              )}
            </div>
          </div>
        )}

        {/* Article content */}
        <div className="prose prose-lg max-w-none mb-8">
          <div 
            dangerouslySetInnerHTML={{ __html: news.contentHtml }}
            className="text-gray-800 leading-relaxed"
          />
        </div>

        {/* Tags */}
        {news.tags.length > 0 && (
          <div className="mb-8">
            <h3 className="text-lg font-semibold text-gray-900 mb-3">Tags</h3>
            <div className="flex flex-wrap gap-2">
              {news.tags.map((tag) => (
                <span
                  key={tag}
                  className="bg-gray-100 text-gray-700 text-sm px-3 py-1 rounded-full hover:bg-gray-200 transition-colors"
                >
                  #{tag}
                </span>
              ))}
            </div>
          </div>
        )}

        {/* Navigation */}
        <div className="border-t border-gray-200 pt-6">
          <button
            onClick={() => navigate('/uutiset')}
            className="bg-blue-600 text-white px-6 py-3 rounded-lg hover:bg-blue-700 transition-colors font-medium"
          >
            {t('common.back_to_news', 'Back to All News')}
          </button>
        </div>
      </div>
    </PageTemplate>
  );
}

export default SingleNewsPage;
