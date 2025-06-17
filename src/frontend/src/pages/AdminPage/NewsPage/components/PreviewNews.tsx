import { useTranslation } from 'react-i18next';
import { type NewsInputsData } from './NewsInputs';
import '../styles/PreviewNews.scss';
import '../styles/MatchResult.scss';

interface PreviewNewsProps {
  value: string;
  newsData?: NewsInputsData;
}

export default function PreviewNews({ value, newsData }: PreviewNewsProps) {
  const { t } = useTranslation();

  return (
    <article>
      {/* Article header */}
      <header>
        {/* Categories */}
        {newsData && (newsData.sportCategory || newsData.category) && (
          <div className="preview-news__categories">
            {newsData.sportCategory && (
              <span className="preview-news__category preview-news__category--sport">
                {newsData.sportCategory}
              </span>
            )}
            {newsData.category && (
              <span className="preview-news__category preview-news__category--general">
                {newsData.category}
              </span>
            )}
          </div>
        )}
        
        {/* Title */}
        <h1 className="preview-news__title">
          {newsData?.title || t('admin.news.untitled', 'Untitled Article')}
        </h1>

        {/* Summary */}
        {newsData?.summary && (
          <p className="preview-news__summary">
            {newsData.summary}
          </p>
        )}

        {/* Meta information */}
        <div className="preview-news__meta">
          {newsData?.author && (
            <div className="preview-news__meta-item">
              <svg className="preview-news__meta-icon" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
              </svg>
              <span>{newsData.author}</span>
            </div>
          )}
          <div className="preview-news__meta-item">
            <svg className="preview-news__meta-icon" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
            <span>{new Date().toLocaleDateString()}</span>
          </div>
        </div>
      </header>

      {/* Main image */}
      {newsData?.mainPicture && (
        <div className="preview-news__image-section">
          <img
            src={newsData.mainPicture}
            alt={newsData.title || 'News image'}
            className="preview-news__main-image"
            onError={(e) => {
              const target = e.target as HTMLImageElement;
              target.src = 'https://via.placeholder.com/800x400?text=Image+Not+Found';
            }}
          />
        </div>
      )}

      {/* Article content */}
      <div className="preview-news__content">
        {value ? (
          <div 
            dangerouslySetInnerHTML={{ __html: value }}
            className="preview-news__content-html"
          />
        ) : (
          <div className="preview-news__no-content">
            <p>{t('admin.news.no_content', 'No content written yet...')}</p>
          </div>
        )}
      </div>

      {/* Tags */}
      {newsData?.tags && newsData.tags.length > 0 && (
        <footer className="preview-news__footer">
          <h3 className="preview-news__tags-title">
            {t('admin.news.tags', 'Tags')}
          </h3>
          <div className="preview-news__tags">
            {newsData.tags.map((tag, index) => (
              <span key={index} className="preview-news__tag">
                #{tag}
              </span>
            ))}
          </div>
        </footer>
      )}
    </article>
  );
}