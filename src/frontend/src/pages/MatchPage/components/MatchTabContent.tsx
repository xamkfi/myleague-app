import type { FloorballMatchDto } from '../../../types/floorball/floorballTypes';
import { FloorballMatchStatus } from '../../../types/floorball/floorballTypes';
import type { MatchTabType } from '../../../components/match';
import MatchEvents from './MatchEvents';
import MatchLineups from './MatchLineups';
import MatchStats from './MatchStats';
import MatchStandings from './MatchStandings';
import { useTranslation } from 'react-i18next';

interface MatchTabContentProps {
  activeTab: MatchTabType;
  match: FloorballMatchDto;
}

export default function MatchTabContent({ activeTab, match }: MatchTabContentProps) {
  const { t } = useTranslation();
  const hasStarted: boolean =
    match.status === FloorballMatchStatus.InProgress ||
    match.status === FloorballMatchStatus.Completed;
  const hasEvents: boolean = match.goalEvents.length > 0 || match.penaltyEvents.length > 0;
  const renderTabContent = () => {
    switch (activeTab) {
      case 'summary':
        return (
          <div className="tab-content">
            <div className="summary-content">
              {(match.wentToOvertime || match.wentToShootout) && (
                <div className="match-notes">
                  {match.wentToOvertime && <span>{t('matchPage.matchInfo.overtime')}</span>}
                  {match.wentToShootout && <span>{t('matchPage.matchInfo.shootout')}</span>}
                </div>
              )}

              {!hasStarted && !hasEvents && (
                <p className="match-pending">{t('matchPage.matchInfo.notStarted')}</p>
              )}

              {hasStarted && (
                <div className="summary-stats-section">
                  <MatchStats match={match} />
                </div>
              )}

              {hasEvents && (
                <div className="summary-events-section">
                  <MatchEvents match={match} />
                </div>
              )}
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