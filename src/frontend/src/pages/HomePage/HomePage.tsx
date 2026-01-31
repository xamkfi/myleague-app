import { useState, useEffect } from 'react';
import HomeNewsSection from '../../components/HomeNewsSection/HomeNewsSection';
import MainNewsCard from '../../components/MainNewsCard/MainNewsCard';
import MatchSidebar from '../../components/MatchSidebar/MatchSidebar';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import { getMainNewsArticle, type NewsArticleDto } from '../../api/news/newsService';
import './HomePage.scss';

function HomePage() {
  const [mainNews, setMainNews] = useState<NewsArticleDto | null>(null);
  const [isLoadingMainNews, setIsLoadingMainNews] = useState(true);

  useEffect(() => {
    const fetchMainNews = async () => {
      try {
        const news = await getMainNewsArticle();
        setMainNews(news);
      } catch (error) {
        console.error('Failed to fetch main news:', error);
      } finally {
        setIsLoadingMainNews(false);
      }
    };

    fetchMainNews();
  }, []);

  return (
    <PageTemplate title="Home">
      <div className="home-page">
        {/* Main News Hero Section */}
        {!isLoadingMainNews && mainNews && (
          <div className="hero-news-container">
            <MainNewsCard news={mainNews} />
          </div>
        )}

        {/* Loading skeleton for main news */}
        {isLoadingMainNews && (
          <div className="hero-news-container hero-news-container--loading">
            <div className="main-news-skeleton">
              <div className="skeleton-image" />
              <div className="skeleton-content">
                <div className="skeleton-category" />
                <div className="skeleton-title" />
                <div className="skeleton-summary" />
                <div className="skeleton-button" />
              </div>
            </div>
          </div>
        )}

        {/* Content Section: News List + Sidebar */}
        <div className="main-content">
          <div className="news-section-container">
            <HomeNewsSection />
          </div>
          <div className="sidebar-container">
            <MatchSidebar />
          </div>
        </div>
      </div>
    </PageTemplate>
  );
}

export default HomePage; 