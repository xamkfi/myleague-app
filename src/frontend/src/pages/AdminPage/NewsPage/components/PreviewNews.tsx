import { useTranslation } from 'react-i18next';
import { type NewsInputsData } from './NewsInputs';
import '../styles/PreviewNews.scss';
import defaultNewsImage from '../../../../assets/defaultImage.jpg';

interface PreviewNewsProps {
  value: string;
  newsData?: NewsInputsData;
}

export default function PreviewNews({ value, newsData }: PreviewNewsProps) {
  const { t } = useTranslation();

  return (
    <div className="single-news-page__container">
      <article className={`single-news-page`}>

        {/* Article header */}
        <header className="single-news-page__header">
        {/* Categories */}

        
        {/* Title */}
        <h1 className="single-news-page__title">
          {newsData?.title || t('admin.news.untitled', 'Untitled Article')}
        </h1>

        {/* Summary */}
        {newsData?.summary && (
          <p className="single-news-page__summary">
            {newsData.summary}
          </p>
        )}

        {/* Meta information */}
        <div className="single-news-page__meta">
          <div className="single-news-page__meta-left">
            {newsData?.author && (
              <div className="single-news-page__meta-item">
                <svg className="single-news-page__meta-icon" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                </svg>
                <span>{newsData.author}</span>
              </div>
            )}
            <div className="single-news-page__meta-item">
              <svg className="single-news-page__meta-icon" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
              </svg>
              <span>{new Date().toLocaleDateString()}</span>
            </div>
          </div>
          {(newsData?.sportCategory || newsData?.category) && (
            <div className="single-news-page__categories">
              {newsData.sportCategory && (
                <span className="single-news-page__category single-news-page__category--sport">
                  {newsData.sportCategory}
                </span>
              )}
              {newsData.category && (
                <span className="single-news-page__category single-news-page__category--general">
                  {newsData.category}
                </span>
              )}
            </div>
          )}
        </div>
        
        
        
      </header>

      {/* Main image */}
      <div className="single-news-page__image-section">
        <img
          src={newsData?.mainPicture || defaultNewsImage}
          alt={newsData?.title || t('admin.news.news_image', 'News image')}
          className="single-news-page__main-image"
          onError={(e) => {
            const target = e.target as HTMLImageElement;
            target.src = 'https://via.placeholder.com/800x400?text=Image+Not+Found';
          }}
        />
      </div>

      {/* Article content */}
      <div className="single-news-page__content">
        {value ? (
          <div 
            dangerouslySetInnerHTML={{ __html: value }}
            className="single-news-page__content-html"
          />
        ) : (
          <div className="single-news-page__no-content">
            <p>{t('admin.news.no_content', 'No content written yet...')}</p>
          </div>
        )}
      </div>

      {/* Tags */}
      {newsData?.tags && newsData.tags.length > 0 && (
        <footer className="single-news-page__footer">
          <h3 className="single-news-page__tags-title">
            {t('admin.news.tags', 'Tags')}
          </h3>
          <div className="single-news-page__tags">
            {newsData.tags.map((tag, index) => (
              <span key={index} className="single-news-page__tag">
                #{tag}
              </span>
            ))}
          </div>
        </footer>
      )}
    </article>
  </div>
  );
}