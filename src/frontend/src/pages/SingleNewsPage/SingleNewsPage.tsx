import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import './SingleNewsPage.scss';
import type { NewsArticleDto } from '../../api/news/newsService';
import { singleNewsService } from '../../api/news/singleNewsService';


interface SingleNewsPageProps {
  newsData?: NewsArticleDto;
  onBack?: () => void;
}

function SingleNewsPage({ newsData }: SingleNewsPageProps) {
  const { t } = useTranslation();
  const { id } = useParams<{ id: string }>();
  const [news, setNews] = useState<NewsArticleDto | null>(newsData || null);

  async function RetrieveNews(articleId: string) {
    const response = await singleNewsService(articleId);
    setNews(response);
  }
  useEffect(()=>{
    if(id){
      RetrieveNews(id);
          console.log(news?.mainImage, news?.contentHtml);
    }

  },[news?.contentHtml, news?.mainImage, id])

  if (!news) {
    return (
      <PageTemplate title={t('nav.news')}>
        <div className="single-news-page__loading">
          <div className="single-news-page__loading-text">Loading...</div>
        </div>
      </PageTemplate>
    );
  }

  const content = (
    <article className={`single-news-page`}>

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
      {news.mainImage && (
        <div className="single-news-page__image-section">
          <img
            src={news.mainImage}
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

    </article>
  );

  return (
    <PageTemplate title={news.title}>
      {content}
    </PageTemplate>
  );
}

export default SingleNewsPage;
