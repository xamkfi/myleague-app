import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import './SportsPage.scss';

interface SportItem {
  id: string;
  nameKey: string;
  descriptionKey: string;
  icon: string;
  path: string;
  enabled: boolean;
}

const SPORTS: SportItem[] = [
  {
    id: 'floorball',
    nameKey: 'sports.floorball',
    descriptionKey: 'sportsPage.floorballDescription',
    icon: '🏑',
    path: '/sports/floorball',
    enabled: true,
  },
  {
    id: 'icehockey',
    nameKey: 'sports.iceHockey',
    descriptionKey: 'sportsPage.iceHockeyDescription',
    icon: '🏒',
    path: '/sports/icehockey',
    enabled: false,
  },
];

function SportsPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const handleSportClick = (sport: SportItem) => {
    if (sport.enabled) {
      navigate(sport.path);
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent, sport: SportItem) => {
    if ((e.key === 'Enter' || e.key === ' ') && sport.enabled) {
      e.preventDefault();
      navigate(sport.path);
    }
  };

  return (
    <PageTemplate title={t('nav.sports')}>
      <div className="sports-page">
        <div className="sports-page__header">
          <h1 className="sports-page__title">{t('sportsPage.title')}</h1>
          <p className="sports-page__description">
            {t('sportsPage.description')}
          </p>
        </div>

        <div className="sports-page__grid">
          {SPORTS.map((sport) => (
            <div
              key={sport.id}
              className={`sport-card ${!sport.enabled ? 'sport-card--disabled' : ''}`}
              onClick={() => handleSportClick(sport)}
              onKeyDown={(e) => handleKeyDown(e, sport)}
              role="button"
              tabIndex={sport.enabled ? 0 : -1}
              aria-disabled={!sport.enabled}
            >
              <div className="sport-card__icon">{sport.icon}</div>
              <div className="sport-card__content">
                <h2 className="sport-card__title">{t(sport.nameKey)}</h2>
                <p className="sport-card__description">{t(sport.descriptionKey)}</p>
                {sport.enabled ? (
                  <span className="sport-card__link">
                    {t('sportsPage.viewLeagues')} &rarr;
                  </span>
                ) : (
                  <span className="sport-card__coming-soon">
                    {t('sportsPage.comingSoon')}
                  </span>
                )}
              </div>
            </div>
          ))}
        </div>
      </div>
    </PageTemplate>
  );
}

export default SportsPage;
