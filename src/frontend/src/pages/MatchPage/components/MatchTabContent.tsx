import type { FloorballMatchDto } from '../../../types/floorball/floorballTypes';
import type { TabType } from './MatchNavigation';
import MatchEvents from './MatchEvents';
import MatchLineups from './MatchLineups';

interface MatchTabContentProps {
  activeTab: TabType;
  match: FloorballMatchDto;
}

export default function MatchTabContent({ activeTab, match }: MatchTabContentProps) {
  const renderTabContent = () => {
    switch (activeTab) {
      case 'summary':
        return (
          <div className="tab-content">
            <div className="summary-content">
              <div className="match-info">
                {match.venue && (
                  <p>📍 Venue: {match.venue}</p>
                )}
                <p>Status: {match.status}</p>
                {match.wentToOvertime && <p>⏱️ Went to overtime</p>}
                {match.wentToShootout && <p>🥅 Went to shootout</p>}
              </div>
              
              <MatchEvents match={match} />
            </div>
          </div>
        );
      
      case 'stats':
        return (
          <div className="tab-content">
            <div className="stats-placeholder">
              <h3>Match Statistics</h3>
              <p>Detailed match statistics coming soon...</p>
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
            <div className="table-placeholder">
              <h3>League Table</h3>
              <p>League table and standings coming soon...</p>
            </div>
          </div>
        );
      
      default:
        return null;
    }
  };

  return renderTabContent();
} 