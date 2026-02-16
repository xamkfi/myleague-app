import { useState, useEffect } from 'react';
import HomeNewsSection from '../../components/HomeNewsSection/HomeNewsSection';
import NewsHeroCarousel from '../../components/NewsHeroCarousel/NewsHeroCarousel';
import MatchSidebar from '../../components/MatchSidebar/MatchSidebar';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import { newsService, type NewsArticleDto, type PaginatedNewsResponse } from '../../api/news/newsService';
import './HomePage.scss';

function HomePage() {
  const [heroNews, setHeroNews] = useState<NewsArticleDto[]>([]);
  const [isLoadingHeroNews, setIsLoadingHeroNews] = useState(true);

  useEffect(() => {
    const fetchHeroNews = async () => {
      try {
        const response = await newsService({
          page: 1,
          pageSize: 5,
          includeArchived: false,
        });

        if (response && typeof response === 'object' && 'pagination' in response) {
          const paginatedResponse = response as PaginatedNewsResponse;
          setHeroNews(paginatedResponse.data);
        } else {
          const oldResponse = response as NewsArticleDto[];
          setHeroNews(oldResponse.slice(0, 5));
        }
      } catch (error) {
        console.error('Failed to fetch hero news:', error);
      } finally {
        setIsLoadingHeroNews(false);
      }
    };

    fetchHeroNews();
  }, []);

  return (
    <div className="home-page-wrapper">
      <PageTemplate title="Home">
        <div className="home-page">
          {/* Main News Hero Carousel */}
          {!isLoadingHeroNews && heroNews.length > 0 && (
            <div className="hero-news-container">
              <NewsHeroCarousel newsArticles={heroNews} />
            </div>
          )}

          {/* Loading skeleton for hero news */}
          {isLoadingHeroNews && (
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
    </div>
  );
}

export default HomePage; 