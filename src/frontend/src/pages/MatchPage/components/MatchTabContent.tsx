import type { FloorballMatchDto } from '../../../types/floorball/floorballTypes';
import type { TabType } from './MatchNavigation';
import MatchEvents from './MatchEvents';
import MatchLineups from './MatchLineups';
import MatchStats from './MatchStats';
import MatchStandings from './MatchStandings';
import { useTranslation } from 'react-i18next';

interface MatchTabContentProps {
  activeTab: TabType;
  match: FloorballMatchDto;
}

export default function MatchTabContent({ activeTab, match }: MatchTabContentProps) {
  const { t } = useTranslation();
  const renderTabContent = () => {
    switch (activeTab) {
      case 'summary':
        return (
          <div className="tab-content">
            <div className="summary-content">
              <div className="match-info">
                {match.venue && (
                  <p>📍 {t('matchPage.matchInfo.venue')}: {match.venue}</p>
                )}
                <p>{t('matchPage.matchInfo.status')}: {t(`floorball.matches.status.${match.status}`)}</p>
                {match.wentToOvertime && <p>⏱️ {t('matchPage.matchInfo.overtime')}</p>}
                {match.wentToShootout && <p>🥅 {t('matchPage.matchInfo.shootout')}</p>}
              </div>
              
              <MatchEvents match={match} />
              
              {/* Add stats section to summary */}
              <div className="summary-stats-section">
                <MatchStats match={match} />
              </div>
            </div>
          </div>
        );
      
      case 'stats':
        return (
          <div className="tab-content">
            <MatchStats match={match} />
          </div>
        );
      
      case 'lineups':
        return (
          <div className="tab-content">
            <MatchLineups match={match} />
          </div>
        );
      
      case 'table':
        return (
          <div className="tab-content">
            <MatchStandings match={match} />
          </div>
        );
      
      default:
        return null;
    }
  };

  return renderTabContent();
} 