import type { FootballMatchDto } from '../../../types/football/footballTypes';
import { FootballMatchStatus } from '../../../types/football/footballTypes';
import { MatchInfoCard, type MatchTabType } from '../../../components/match';
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
              <MatchInfoCard
                decision={match.wentToPenaltyShootout ? 'shootout' : match.wentToExtraTime ? 'overtime' : null}
                decisionLabel={match.wentToPenaltyShootout
                  ? t('matchPage.matchInfo.decidedInPenaltyShootout', 'Decided by penalty shootout')
                  : match.wentToExtraTime
                    ? t('matchPage.matchInfo.decidedInExtraTime', 'Decided in extra time')
                    : undefined}
                referees={match.refereeDetails}
                scorekeepers={match.scorekeepers}
              />

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