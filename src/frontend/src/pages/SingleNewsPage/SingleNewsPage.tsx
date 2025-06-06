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

interface SingleNewsPageProps {
  newsData?: NewsData;
  isPreview?: boolean;
  onBack?: () => void;
}

function SingleNewsPage({ newsData, isPreview = false, onBack }: SingleNewsPageProps) {
  const { t } = useTranslation();
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [news, setNews] = useState<NewsData | null>(newsData || null);

  useEffect(() => {
    if (!newsData && id) {
      // Find the news item by ID
      const foundNews = mockData.news.find((item) => item.id === id);
      if (foundNews) {
        setNews(foundNews);
      } else {
        // If news not found, redirect to news page
        navigate('/uutiset');
      }
    }
  }, [id, navigate, newsData]);

  if (!news) {
    return (
      <PageTemplate title={t('nav.news')}>
        <div className="single-news-page__loading">
          <div className="single-news-page__loading-text">Loading...</div>
        </div>
      </PageTemplate>
    );
  }

  const handleBack = () => {
    if (onBack) {
      onBack();
    } else {
      navigate('/uutiset');
    }
  };

  // Get the main image (first image if available)
  const mainImage = news.imageUrls.length > 0 ? news.imageUrls[0] : null;

  const content = (
    <article className={`single-news-page ${isPreview ? 'single-news-page--preview' : ''}`}>
      {/* Back button */}
      {!isPreview && (
        <button
          onClick={handleBack}
          className="single-news-page__back-button"
        >
          <svg className="single-news-page__back-icon" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
          </svg>
          {t('common.back', 'Back to News')}
        </button>
      )}

      {/* Article header */}
      <header className="single-news-page__header">
        {/* Categories */}
        {(news.sportCategory || news.category) && (
          <div className="single-news-page__categories">
            {news.sportCategory && (
              <span className="single-news-page__category single-news-page__category--sport">
                {news.sportCategory}
              </span>
            )}
            {news.category && (
              <span className="single-news-page__category single-news-page__category--general">
                {news.category}
              </span>
            )}
          </div>
        )}
        
        {/* Title */}
        <h1 className="single-news-page__title">
          {news.title}
        </h1>

        {/* Summary */}
        {news.summary && (
          <p className="single-news-page__summary">
            {news.summary}
          </p>
        )}

        {/* Meta information */}
        <div className="single-news-page__meta">
          {news.author && (
            <div className="single-news-page__meta-item">
              <svg className="single-news-page__meta-icon" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
              </svg>
              <span>{news.author}</span>
            </div>
          )}
          <div className="single-news-page__meta-item">
            <svg className="single-news-page__meta-icon" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
            <span>{new Date(news.createdAt).toLocaleDateString()}</span>
          </div>
          {news.updatedAt && (
            <div className="single-news-page__meta-item">
              <svg className="single-news-page__meta-icon" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
              </svg>
              <span>Updated: {new Date(news.updatedAt).toLocaleDateString()}</span>
            </div>
          )}
        </div>
      </header>

      {/* Main image */}
      {mainImage && (
        <div className="single-news-page__image-section">
          <img
            src={mainImage}
            alt={news.title}
            className="single-news-page__main-image"
            onError={(e) => {
              const target = e.target as HTMLImageElement;
              target.src = 'https://via.placeholder.com/800x400?text=Image+Not+Found';
            }}
          />
        </div>
      )}

      {/* Article content */}
      <div className="single-news-page__content">
        <div 
          dangerouslySetInnerHTML={{ __html: news.contentHtml }}
          className="single-news-page__content-html"
        />
      </div>

      {/* Tags */}
      {news.tags.length > 0 && (
        <footer className="single-news-page__footer">
          <h3 className="single-news-page__tags-title">Tags</h3>
          <div className="single-news-page__tags">
            {news.tags.map((tag) => (
              <span key={tag} className="single-news-page__tag">
                #{tag}
              </span>
            ))}
          </div>
        </footer>
      )}

      {/* Navigation button for non-preview mode */}
      {!isPreview && (
        <div className="single-news-page__navigation">
          <button
            onClick={handleBack}
            className="single-news-page__back-to-news"
          >
            {t('common.back_to_news', 'Back to All News')}
          </button>
        </div>
      )}
    </article>
  );

  // If it's a preview, don't wrap in PageTemplate
  if (isPreview) {
    return content;
  }

  return (
    <PageTemplate title={news.title}>
      {content}
    </PageTemplate>
  );
}

export default SingleNewsPage;
