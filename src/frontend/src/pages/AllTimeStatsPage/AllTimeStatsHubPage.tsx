import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import CatalogPage from '../../components/CatalogPage/CatalogPage';
import SportIcon from '../../components/SportIcon/SportIcon';
import { getAllTimeStatsPath, type SportKind } from '../../utils/sportRoutes';
import '../SportsPage/SportsPage.scss';

interface HubSport {
  sport: SportKind;
  descriptionKey: string;
}

const SPORTS: HubSport[] = [
  { sport: 'floorball', descriptionKey: 'allTimeStats.cardDescription' },
  { sport: 'football', descriptionKey: 'allTimeStats.cardDescriptionFootball' },
  { sport: 'hockey', descriptionKey: 'allTimeStats.cardDescription' },
];

export default function AllTimeStatsHubPage() {
  const { t } = useTranslation();
  const title = t('allTimeStats.title');

  return (
    <PageTemplate title={title} fullBleed>
      <CatalogPage title={title} description={t('allTimeStats.hubDescription')}>
        <div className="sports-page__grid">
          {SPORTS.map((item) => (
            <Link key={item.sport} to={getAllTimeStatsPath(item.sport)} className="sport-card">
              <div className="sport-card__icon">
                <SportIcon sport={item.sport} size="lg" decorative />
              </div>
              <div className="sport-card__content">
                <h2 className="sport-card__title">{t(`allTimeStats.sports.${item.sport}`)}</h2>
                <p className="sport-card__description">{t(item.descriptionKey)}</p>
                <span className="sport-card__link">{t('allTimeStats.cta')} →</span>
              </div>
            </Link>
          ))}
        </div>
      </CatalogPage>
    </PageTemplate>
  );
}
