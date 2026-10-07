import type { FloorballMatchDto } from '../../../../types/floorball/floorballTypes';
import { FloorballMatchStatus } from '../../../../types/floorball/floorballTypes';
import { MatchInfoCard, type MatchTabType } from '../../../../components/match';
import MatchEvents from './MatchEvents';
import MatchLineups from './MatchLineups';
import MatchStats from './MatchStats';
import MatchStandings from './MatchStandings';
import { useTranslation } from 'react-i18next';
import { useMatchRosters, type MatchRosters } from './useMatchRosters';

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
  const rosters: MatchRosters = useMatchRosters(
    match,
    activeTab === 'lineups' || (activeTab === 'summary' && hasEvents)
  );
  const renderTabContent = () => {
    switch (activeTab) {
      case 'summary':
        return (
          <div className="tab-content">
            <div className="summary-content">
              <MatchInfoCard
                decision={match.wentToShootout ? 'shootout' : match.wentToOvertime ? 'overtime' : null}
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
                  <MatchEvents match={match} homeRoster={rosters.home} awayRoster={rosters.away} />
                </div>
              )}
            </div>
          </div>
        );
      
      case 'lineups':
        return (
          <div className="tab-content">
            <MatchLineups match={match} homeRoster={rosters.home} awayRoster={rosters.away} />
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