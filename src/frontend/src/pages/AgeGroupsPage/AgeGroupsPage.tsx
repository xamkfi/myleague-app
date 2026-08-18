import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import { AUDIENCE_REGISTRY } from '../../audience/audienceRegistry';
import { useAudience } from '../../context/AudienceContext';
import './AgeGroupsPage.css';

function AgeGroupsPage() {
  const { t } = useTranslation();
  const { selectedAudienceId, setAudience } = useAudience();

  return (
    <PageTemplate title={t('nav.ageGroups')}>
      <div className="age-groups-container">
        <p className="intro-text">{t('audience.intro')}</p>

        <div className="age-groups-grid">
          {AUDIENCE_REGISTRY.map((group) => {
            const isSelected = selectedAudienceId === group.id;

            return (
              <div
                key={group.id}
                className={`age-group-card${isSelected ? ' selected' : ''}`}
                data-audience-card={group.themeId}
              >
                <h2 className="age-group-title">{t(group.i18nKey)}</h2>
                <p className="age-group-description">{t(`audience.descriptions.${group.id}`)}</p>
                <button
                  type="button"
                  className="age-group-button"
                  onClick={() => setAudience(group.id)}
                  disabled={isSelected}
                >
                  {isSelected ? t('audience.selected') : t('audience.select')}
                </button>
              </div>
            );
          })}
        </div>
      </div>
    </PageTemplate>
  );
}

export default AgeGroupsPage;
