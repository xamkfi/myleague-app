import { useTranslation } from 'react-i18next';
import type { PersonPlayerLicence } from '../../api/common/personPlayerSportsService';

interface PlayerLicenceSummaryProps {
  licences: PersonPlayerLicence[];
}

export function PlayerLicenceSummary({ licences }: PlayerLicenceSummaryProps) {
  const { t } = useTranslation();

  return (
    <div className="player-licence-summary">
      <span className="stat-label">{t('playerPage.licencesTitle')}</span>
      {licences.length === 0 ? (
        <span className="stat-value">{t('playerPage.noLicences')}</span>
      ) : (
        <ul className="player-licence-summary__list">
          {licences.map((licence) => (
            <li
              key={`${licence.sport}-${licence.teamId}-${licence.competitionId ?? 'base'}`}
              className="player-licence-summary__item"
            >
              <span className="stat-value">{licence.teamName}</span>
              <span className="stat-label">
                {licence.competitionName ?? t('playerPage.licenceBaseRoster')}
              </span>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
