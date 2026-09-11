import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import './SingleNewsPage.scss';
import '../NewsPage/NewsPage.scss';
import type { NewsArticleDto } from '../../api/news/newsService';
import { getRecentNewsArticles } from '../../api/news/newsService';
import { singleNewsService } from '../../api/news/singleNewsService';
import defaultNewsImage from '../../assets/defaultImage.jpg';
import NewsTaxonomyBar from './NewsTaxonomyBar';
import NewsArticleHtml, { useHydratedNewsHtml } from './NewsArticleHtml';
import NewsCard from '../NewsPage/components/NewsCard';

type SingleNewsPageProps = {
  newsData?: NewsArticleDto;
};

function SingleNewsPage({ newsData }: SingleNewsPageProps) {
  const { t } = useTranslation();
  const { id } = useParams<{ id: string }>();
  const [news, setNews] = useState<NewsArticleDto | null>(newsData || null);
  const [relatedNews, setRelatedNews] = useState<NewsArticleDto[]>([]);
  const { displayHtml, relatedTeams } = useHydratedNewsHtml(news?.contentHtml ?? '');

  useEffect(() => {
    if (!id) {
      return;
    }

    let cancelled = false;
    singleNewsService(id).then((article) => {
      if (!cancelled) {
        setNews(article);
      }
    });

    return () => {
      cancelled = true;
    };
  }, [id]);

  useEffect(() => {
    let cancelled = false;
    getRecentNewsArticles(4).then((articles) => {
      if (!cancelled) {
        setRelatedNews(articles.filter((article) => article.id !== id).slice(0, 3));
      }
    });
    return () => {
      cancelled = true;
    };
  }, [id]);

  if (!news) {
    return (
      <div className="single-news-layout">
        <PageTemplate title={t('nav.news')}>
          <div className="single-news-page__loading">
            <div className="single-news-page__loading-text">{t('newsPage.loading')}</div>
          </div>
        </PageTemplate>
      </div>
    );
  }

  const content = (
    <div className="single-news-page-container">
      <article className="single-news-page">
        <header className="single-news-page__header">
          <Link to="/uutiset" className="single-news-page__back-button">
            <svg className="single-news-page__back-icon" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
            </svg>
            {t('newsPage.backToNews')}
          </Link>

          <h1 className="single-news-page__title">{news.title}</h1>

          {news.summary && (
            <p className="single-news-page__summary">{news.summary}</p>
          )}

          <NewsTaxonomyBar
            sportCategory={news.sportCategory}
            category={news.category}
            tags={news.tags}
            teams={relatedTeams}
            clickable
          />

          <div className="single-news-page__meta">
            <div className="single-news-page__meta-left">
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
                  <span>{t('newsPage.updated', { date: new Date(news.updatedAt).toLocaleDateString() })}</span>
                </div>
              )}
            </div>
          </div>
        </header>

        <div className="single-news-page__image-section">
          <img
            src={news.mainImage || defaultNewsImage}
            alt={news.title}
            className="single-news-page__main-image"
            onError={(event) => {
              const target = event.target as HTMLImageElement;
              target.src = defaultNewsImage;
            }}
          />
        </div>

        <div className="single-news-page__content">
          <NewsArticleHtml html={displayHtml} />
        </div>
      </article>

      {relatedNews.length > 0 && (
        <section className="single-news-page__related">
          <h2 className="single-news-page__related-title">{t('newsPage.similarNews')}</h2>
          <div className="single-news-page__related-grid">
            {relatedNews.map((article) => (
              <NewsCard key={article.id} news={article} />
            ))}
          </div>
        </section>
      )}
    </div>
  );

  return (
    <div className="single-news-layout">
      <PageTemplate title={news.title}>
        {content}
      </PageTemplate>
    </div>
  );
}

export default SingleNewsPage;
