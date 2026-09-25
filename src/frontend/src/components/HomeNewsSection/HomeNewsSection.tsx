import { useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import type { NewsArticleDto } from '../../api/news/newsService';
import { AUDIENCE_REGISTRY } from '../../audience/audienceRegistry';
import { useAudience } from '../../context/AudienceContext';
import { SportsCategory } from '../../types/common/sports';
import SportIcon from '../SportIcon/SportIcon';
import HomeNewsCard from './HomeNewsCard';
import './HomeNewsSection.scss';

type HomeNewsSectionProps = {
  articles: NewsArticleDto[];
  isLoading: boolean;
  error: string | null;
  onRetry: () => void;
};

type AudienceFilter = 'all' | string;
type SportFilter = 'all' | SportsCategory;

const SPORT_FILTERS: { id: SportsCategory; labelKey: string }[] = [
  { id: SportsCategory.Floorball, labelKey: 'sports.floorball' },
  { id: SportsCategory.Football, labelKey: 'sports.football' },
  { id: SportsCategory.Icehockey, labelKey: 'sports.iceHockey' },
];

function matchesAudience(article: NewsArticleDto, filter: AudienceFilter): boolean {
  if (filter === 'all') {
    return true;
  }

  if (!article.teamCategory) {
    return true;
  }

  return article.teamCategory === filter;
}

function matchesSport(article: NewsArticleDto, filter: SportFilter): boolean {
  if (filter === 'all') {
    return true;
  }

  return article.sportCategory?.toLowerCase() === filter.toLowerCase();
}

interface NewsFilterChipProps {
  label: string;
  pressed: boolean;
  tone: string;
  onSelect: () => void;
  icon?: SportsCategory;
}

function NewsFilterChip({ label, pressed, tone, onSelect, icon }: NewsFilterChipProps) {
  return (
    <button
      type="button"
      className={`home-news-filter__chip home-news-filter__chip--${tone}${pressed ? ' home-news-filter__chip--active' : ''}`}
      aria-pressed={pressed}
      onClick={onSelect}
    >
      {icon ? (
        <SportIcon sport={icon} size="sm" className="home-news-filter__sport-icon" inverted={pressed} decorative />
      ) : (
        <span className="home-news-filter__dot" aria-hidden="true" />
      )}
      {label}
    </button>
  );
}

function HomeNewsSection({ articles, isLoading, error, onRetry }: HomeNewsSectionProps) {
  const { t } = useTranslation();
  const { audience } = useAudience();
  const [audienceFilter, setAudienceFilter] = useState<AudienceFilter>('all');
  const [sportFilter, setSportFilter] = useState<SportFilter>('all');
  const [filtersOpen, setFiltersOpen] = useState(false);
  const filtersActive = audienceFilter !== 'all' || sportFilter !== 'all';

  const visibleArticles = useMemo(
    () => articles.filter((article) => matchesAudience(article, audienceFilter) && matchesSport(article, sportFilter)),
    [articles, audienceFilter, sportFilter],
  );

  const filters = filtersOpen ? (
    <div className="home-news-filter">
      <div className="home-news-filter__group" role="group" aria-label={t('homePage.newsSection.filters.audience', 'Kohderyhmä')}>
        <NewsFilterChip
          label={t('homePage.newsSection.filters.allAudiences', 'Kaikki')}
          pressed={audienceFilter === 'all'}
          tone="all"
          onSelect={() => setAudienceFilter('all')}
        />
        {AUDIENCE_REGISTRY.map((entry) => (
          <NewsFilterChip
            key={entry.id}
            label={t(entry.i18nKey)}
            pressed={audienceFilter === entry.teamCategory}
            tone={entry.id}
            onSelect={() => setAudienceFilter(entry.teamCategory)}
          />
        ))}
      </div>
      <div className="home-news-filter__group" role="group" aria-label={t('homePage.newsSection.filters.sport', 'Laji')}>
        <NewsFilterChip
          label={t('homePage.newsSection.filters.allSports', 'Kaikki lajit')}
          pressed={sportFilter === 'all'}
          tone="sport"
          onSelect={() => setSportFilter('all')}
        />
        {SPORT_FILTERS.map((sport) => (
          <NewsFilterChip
            key={sport.id}
            label={t(sport.labelKey)}
            pressed={sportFilter === sport.id}
            tone="sport"
            icon={sport.id}
            onSelect={() => setSportFilter(sport.id)}
          />
        ))}
      </div>
    </div>
  ) : null;

  const header = (
    <div className="home-news-section__header">
      <h2 className="home-news-section__title">
        {t('homePage.newsSection.title', 'Ajankohtaista')}
      </h2>
      <button
        type="button"
        className={`home-news-section__filter-toggle${filtersOpen ? ' home-news-section__filter-toggle--open' : ''}${filtersActive ? ' home-news-section__filter-toggle--active' : ''}`}
        aria-expanded={filtersOpen}
        onClick={() => setFiltersOpen((open) => !open)}
      >
        {t('homePage.newsSection.filters.toggle', 'Suodata')}
      </button>
    </div>
  );

  if (isLoading) {
    return (
      <div className="home-news-section">
        {header}
        <div className="home-news-section__list">
          {Array.from({ length: 5 }, (_, index) => (
            <div key={`skeleton-${index}`} className="home-news-card home-news-card--skeleton">
              <div className="home-news-card__image-container skeleton-image" />
              <div className="home-news-card__content">
                <div className="skeleton-meta" />
                <div className="skeleton-title" />
                <div className="skeleton-summary" />
                <div className="skeleton-summary skeleton-summary--short" />
              </div>
            </div>
          ))}
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="home-news-section">
        {header}
        <div className="home-news-section__error">
          <p>{error}</p>
          <button type="button" onClick={onRetry} className="home-news-section__retry-btn">
            {t('homePage.newsSection.retry', 'Yritä uudelleen')}
          </button>
        </div>
      </div>
    );
  }

  if (articles.length === 0) {
    return (
      <div className="home-news-section">
        {header}
        <div className="home-news-section__empty">
          <p>{t('homePage.newsSection.noNewsForAudience', { audience: t(audience.i18nKey) })}</p>
        </div>
      </div>
    );
  }

  return (
    <div className="home-news-section">
      {header}
      {filters}
      {visibleArticles.length === 0 ? (
        <div className="home-news-section__empty">
          <p>{t('homePage.newsSection.noFilterMatches', 'Ei uutisia valituilla suodattimilla.')}</p>
        </div>
      ) : (
        <div className="home-news-section__list">
          {visibleArticles.map((news) => (
            <HomeNewsCard key={news.id} news={news} />
          ))}
        </div>
      )}
    </div>
  );
}

export default HomeNewsSection;
