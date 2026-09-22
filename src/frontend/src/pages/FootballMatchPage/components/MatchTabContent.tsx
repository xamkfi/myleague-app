import type { FootballMatchDto } from '../../../types/football/footballTypes';
import { FootballMatchStatus } from '../../../types/football/footballTypes';
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
  const hasStarted: boolean =
    match.status === FootballMatchStatus.InProgress ||
    match.status === FootballMatchStatus.Completed;
  const hasEvents: boolean =
    match.goalEvents.length > 0 ||
    match.cardEvents.length > 0 ||
    (match.substitutionEvents?.length ?? 0) > 0;
  const renderTabContent = () => {
    switch (activeTab) {
      case 'summary':
        return (
          <div className="tab-content">
            <div className="summary-content">
              {(match.wentToExtraTime || match.wentToPenaltyShootout) && (
                <div className="match-notes">
                  {match.wentToExtraTime && <span>{t('football.match.extraTime', 'Extra time')}</span>}
                  {match.wentToPenaltyShootout && <span>{t('football.match.penaltyShootout', 'Penalty shootout')}</span>}
                </div>
              )}

              {!hasStarted && !hasEvents && (
                <p className="match-pending">{t('matchPage.matchInfo.notStarted')}</p>
              )}

              <MatchEvents match={match} />

              {hasStarted && (
                <div className="summary-stats-section">
                  <MatchStats match={match} />
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