import type { FootballMatchDto } from '../../../types/football/footballTypes';
import type { MatchTabType } from '../../../components/match';
import MatchEvents from './MatchEvents';
import MatchLineups from './MatchLineups';
import MatchStats from './MatchStats';
import MatchStandings from './MatchStandings';
import { useTranslation } from 'react-i18next';

interface MatchTabContentProps {
  activeTab: MatchTabType;
  match: FootballMatchDto;
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
                  <p>{t('matchPage.matchInfo.venue')}: {match.venue}</p>
                )}
                <p>{t('matchPage.matchInfo.status')}: {t(`football.matches.status.${match.status}`, match.status)}</p>
                {match.wentToExtraTime && <p>{t('football.match.extraTime', 'Extra time')}</p>}
                {match.wentToPenaltyShootout && <p>{t('football.match.penaltyShootout', 'Penalty shootout')}</p>}
              </div>
              
              <MatchEvents match={match} />
              
              {/* Add stats section to summary */}
              <div className="summary-stats-section">
                <MatchStats match={match} />
              </div>
            </div>
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