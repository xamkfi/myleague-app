import { useCallback, useEffect, useMemo, useState } from 'react';
import HomeNewsSection from '../../components/HomeNewsSection/HomeNewsSection';
import NewsHeroCarousel from '../../components/NewsHeroCarousel/NewsHeroCarousel';
import MatchesPanel from '../../components/MatchesPanel/MatchesPanel';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import AdSlot from '../../components/AdSlot/AdSlot';
import { advertisementService } from '../../api/common/advertisementService';
import { useMediaQuery } from '../../hooks/useMediaQuery';
import {
  AD_SLOTS,
  SIDE_AD_SLOTS,
  type AdSlot as AdSlotName,
  type PublicAdvertisement,
} from '../../types/admin/advertisementTypes';
import { newsService, type NewsArticleDto, type PaginatedNewsResponse } from '../../api/news/newsService';
import { useAudience } from '../../context/AudienceContext';
import { useTranslation } from 'react-i18next';
import './HomePage.scss';

// Room for the 1200px content column, two 300px ad rails with gaps, and a scrollbar.
const SIDE_RAILS_QUERY = '(min-width: 1860px)';

function groupAdsBySlot(ads: PublicAdvertisement[]): Record<AdSlotName, PublicAdvertisement[]> {
  return Object.fromEntries(
    AD_SLOTS.map((slot) => [slot, ads.filter((ad) => ad.slot === slot)]),
  ) as Record<AdSlotName, PublicAdvertisement[]>;
}

function unwrapNewsList(response: PaginatedNewsResponse | NewsArticleDto[]): NewsArticleDto[] {
  if (response && typeof response === 'object' && 'pagination' in response) {
    return response.data;
  }

  return response.slice(0, 24);
}

function HomePage() {
  const { t } = useTranslation();
  const { audience } = useAudience();
  const [newsArticles, setNewsArticles] = useState<NewsArticleDto[]>([]);
  const [isLoadingNews, setIsLoadingNews] = useState(true);
  const [newsError, setNewsError] = useState<string | null>(null);
  const [ads, setAds] = useState<PublicAdvertisement[]>([]);
  const showSideRails = useMediaQuery(SIDE_RAILS_QUERY);

  const fetchNews = useCallback(async () => {
    try {
      setIsLoadingNews(true);
      setNewsError(null);
      const response = await newsService({
        page: 1,
        pageSize: 24,
        includeArchived: false,
      });
      setNewsArticles(unwrapNewsList(response));
    } catch (error) {
      console.error('Failed to fetch news:', error);
      setNewsError(t('homePage.newsSection.error', 'Uutisten lataaminen epäonnistui'));
      setNewsArticles([]);
    } finally {
      setIsLoadingNews(false);
    }
  }, [t]);

  useEffect(() => {
    void fetchNews();
  }, [fetchNews]);

  useEffect(() => {
    let isMounted = true;

    advertisementService
      .getVisible()
      .then((items) => {
        if (isMounted) {
          setAds(items);
        }
      })
      .catch((error: unknown) => {
        // Ads are optional; the page works without them.
        console.error('Failed to fetch advertisements:', error);
      });

    return () => {
      isMounted = false;
    };
  }, []);

  const adsBySlot = useMemo(() => groupAdsBySlot(ads), [ads]);
  const bottomAds = adsBySlot.BottomCenter;
  const stackedSideAds = showSideRails
    ? []
    : SIDE_AD_SLOTS.map((slot) => adsBySlot[slot]).filter((slotAds) => slotAds.length > 0);

  const audienceHeroNews = newsArticles.filter(
    (article) => !article.teamCategory || article.teamCategory === audience.teamCategory,
  );
  const heroNews = (audienceHeroNews.length > 0 ? audienceHeroNews : newsArticles).slice(0, 5);

  return (
    <div className="home-page-wrapper">
      <PageTemplate title="Home">
        <div className="home-page">
          {!isLoadingNews && heroNews.length > 0 && (
            <div className="hero-news-container">
              <NewsHeroCarousel newsArticles={heroNews} />
            </div>
          )}

          {isLoadingNews && (
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

          <div className="main-content">
            <div className="news-section-container">
              <HomeNewsSection
                articles={newsArticles}
                isLoading={isLoadingNews}
                error={newsError}
                onRetry={fetchNews}
              />
              {(bottomAds.length > 0 || stackedSideAds.length > 0) && (
                <div className="home-ads-stack">
                  {bottomAds.length > 0 && (
                    <AdSlot ads={bottomAds} className="home-ads-stack__wide" />
                  )}
                  {stackedSideAds.map((slotAds) => (
                    <AdSlot key={slotAds[0].slot} ads={slotAds} className="home-ads-stack__item" />
                  ))}
                </div>
              )}
            </div>
            <div className="sidebar-container">
              <MatchesPanel />
            </div>
            {showSideRails && (
              <>
                <div className="home-ads-rail home-ads-rail--left">
                  <AdSlot ads={adsBySlot.LeftTop} />
                  <AdSlot ads={adsBySlot.LeftBottom} />
                </div>
                <div className="home-ads-rail home-ads-rail--right">
                  <AdSlot ads={adsBySlot.RightTop} />
                  <AdSlot ads={adsBySlot.RightBottom} />
                </div>
              </>
            )}
          </div>
        </div>
      </PageTemplate>
    </div>
  );
}

export default HomePage;
