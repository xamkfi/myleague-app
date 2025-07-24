import { useEffect, useState, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import './NewsPage.scss';
import NewsCard from './components/NewsCard';
import NewsFilter from './components/NewsFilter';
import { newsService, type NewsArticleDto, type NewsParameters, getMainNewsArticle } from '../../api/news/newsService';
import { useNavigate } from 'react-router-dom';

function NewsPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [newsList, setNewsList] = useState<NewsArticleDto[]>([]);
  const [mainNews, setMainNews] = useState<NewsArticleDto | null>(null);
  const [filters, setFilters] = useState<NewsParameters>({
    category: '',
    sportCategory: '',
    searchTerm: ''
  });

  const RetrieveNews = useCallback(async () => {
    const response = await newsService(filters);
    setNewsList(response);
  }, [filters]);

  useEffect(() => {
    RetrieveNews();
  }, [RetrieveNews]);

  useEffect(() => {
    getMainNewsArticle().then(setMainNews);
  }, []);

  // Exclude mainNews from otherNews if present
  const otherNews = mainNews
    ? newsList.filter((item) => item.id !== mainNews.id)
    : newsList;

  const mainNewsBgStyle = mainNews && mainNews.mainImage
    ? { ['--main-news-image' as any]: `url('${mainNews.mainImage}')` } as React.CSSProperties
    : undefined;

  return (
    <PageTemplate title={t('nav.news')} >
      <div
        className={`news-main-bg${mainNews && mainNews.mainImage ? ' has-main-image' : ''}`}
        style={mainNewsBgStyle}
      >
        <div className="news-main-section">
          {mainNews && (
            <div className="main-news-card">
              <div className="main-news-image-container">
                {mainNews.mainImage && (
                  <img src={mainNews.mainImage} alt={mainNews.title} className="main-news-image" />
                )}
              </div>
              <div className="main-news-content">
                <div className="main-news-category">{mainNews.sportCategory}</div>
                <h2 className="main-news-title">{mainNews.title}</h2>
                <div className="main-news-summary">{mainNews.summary}</div>
                <button className="main-news-button" onClick={() => navigate(`/uutiset/${mainNews.id}`)}>
                  {t('newsPage.readMore', 'Lue lisää')}
                </button>
              </div>
            </div>
          )}
        </div>
      </div>

      <div className="news-list-section">
        <NewsFilter onFilterChange={setFilters} />
        <div className="news-grid">
          {otherNews.map((item) => (
            <NewsCard key={item.id} news={item} />
          ))}
        </div>
      </div>
    </PageTemplate>
  );
}

export default NewsPage; 