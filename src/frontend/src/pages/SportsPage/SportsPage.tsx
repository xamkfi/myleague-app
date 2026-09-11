import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import SportIcon, { type SportIconSport } from '../../components/SportIcon/SportIcon';
import bannerImage from '../../assets/floorball-banner.png';
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
      <div className="sports-page">
        <header className="sports-page__banner">
          <img className="sports-page__banner-image" src={bannerImage} alt="" aria-hidden="true" />
          <div className="sports-page__banner-content">
            <h1 className="sports-page__title">{t('sportsPage.title')}</h1>
            <p className="sports-page__description">{t('sportsPage.description')}</p>
          </div>
        </header>

        <div className="sports-page__content">
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
        </div>
      </div>
    </PageTemplate>
  );
}

export default SportsPage;
