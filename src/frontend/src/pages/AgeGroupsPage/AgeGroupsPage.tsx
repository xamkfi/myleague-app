import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import CatalogPage from '../../components/CatalogPage/CatalogPage';
import { AUDIENCE_REGISTRY } from '../../audience/audienceRegistry';
import { useAudience } from '../../context/AudienceContext';
import './AgeGroupsPage.scss';

function AgeGroupsPage() {
  const { t } = useTranslation();
  const { selectedAudienceId, setAudience } = useAudience();

  return (
    <PageTemplate title={t('nav.ageGroups')} fullBleed>
      <CatalogPage title={t('nav.ageGroups')} description={t('audience.intro')}>
        <div className="age-groups-grid">
          {AUDIENCE_REGISTRY.map((group) => {
            const isSelected = selectedAudienceId === group.id;

            return (
              <article
                key={group.id}
                className={`age-group-card${isSelected ? ' age-group-card--selected' : ''}`}
                data-audience-card={group.themeId}
              >
                <div className="age-group-card__band" aria-hidden="true" />
                <div className="age-group-card__content">
                  <h2 className="age-group-card__title">{t(group.i18nKey)}</h2>
                  <p className="age-group-card__description">{t(`audience.descriptions.${group.id}`)}</p>
                  <button
                    type="button"
                    className="age-group-card__button"
                    onClick={() => setAudience(group.id)}
                    disabled={isSelected}
                  >
                    {isSelected ? t('audience.selected') : t('audience.select')}
                  </button>
                </div>
              </article>
            );
          })}
        </div>
      </CatalogPage>
    </PageTemplate>
  );
}

export default AgeGroupsPage;
