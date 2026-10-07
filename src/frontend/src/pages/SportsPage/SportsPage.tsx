import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import CatalogPage from '../../components/CatalogPage/CatalogPage';
import SportIcon, { type SportIconSport } from '../../components/SportIcon/SportIcon';
import TrophyIcon from '../../components/TrophyIcon/TrophyIcon';
import { ALL_TIME_STATS_HUB_PATH } from '../../utils/sportRoutes';
import './SportsPage.scss';

interface SportItem {
  id: SportIconSport;
  nameKey: string;
  descriptionKey: string;
  path: string;
  enabled: boolean;
}

const SPORTS: SportItem[] = [
  {
    id: 'floorball',
    nameKey: 'sports.floorball',
    descriptionKey: 'sportsPage.floorballDescription',
    path: '/sports/floorball',
    enabled: true,
  },
  {
    id: 'football',
    nameKey: 'sports.football',
    descriptionKey: 'sportsPage.footballDescription',
    path: '/sports/football',
    enabled: true,
  },
  {
    id: 'icehockey',
    nameKey: 'sports.iceHockey',
    descriptionKey: 'sportsPage.iceHockeyDescription',
    path: '/sports/icehockey',
    enabled: true,
  },
];

function SportsPage() {
  const { t } = useTranslation();

  return (
    <PageTemplate title={t('nav.sports')} fullBleed>
      <CatalogPage title={t('sportsPage.title')} description={t('sportsPage.description')}>
        {/* One child: CatalogPage lays its children out in a row. */}
        <div className="sports-page">
          <div className="sports-page__grid">
            {SPORTS.map((sport) => {
              const cardClass = sport.enabled
                ? 'sport-card'
                : 'sport-card sport-card--disabled';
              const body = (
                <>
                  <div className="sport-card__icon">
                    <SportIcon sport={sport.id} size="lg" decorative />
                  </div>
                  <div className="sport-card__content">
                    <h2 className="sport-card__title">{t(sport.nameKey)}</h2>
                    <p className="sport-card__description">{t(sport.descriptionKey)}</p>
                    {sport.enabled ? (
                      <span className="sport-card__link">{t('sportsPage.viewLeagues')} →</span>
                    ) : (
                      <span className="sport-card__coming-soon">{t('sportsPage.comingSoon')}</span>
                    )}
                  </div>
                </>
              );

              if (!sport.enabled) {
                return (
                  <div key={sport.id} className={cardClass} aria-disabled="true">
                    {body}
                  </div>
                );
              }

              return (
                <Link key={sport.id} to={sport.path} className={cardClass}>
                  {body}
                </Link>
              );
            })}
          </div>
          <Link to={ALL_TIME_STATS_HUB_PATH} className="sports-page__all-time">
            <span className="sports-page__all-time-icon">
              <TrophyIcon size={24} />
            </span>
            <span className="sports-page__all-time-text">
              <span className="sports-page__all-time-title">{t('allTimeStats.title')}</span>
              <span className="sports-page__all-time-description">{t('allTimeStats.hubDescription')}</span>
            </span>
            <span className="sports-page__all-time-link">{t('allTimeStats.cta')} →</span>
          </Link>
        </div>
      </CatalogPage>
    </PageTemplate>
  );
}

export default SportsPage;
