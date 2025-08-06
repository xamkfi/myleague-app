import type { FloorballMatchDto } from '../../../../../../types/floorball/floorballTypes';
import { formatDateTime, getStatusBadge } from '../../utils/matchFormatters';
import './CollapsibleMatchSection.scss';

interface CollapsibleMatchSectionProps {
  title: string;
  matches: FloorballMatchDto[];
  isCollapsed: boolean;
  onToggleCollapse: () => void;
  onLiveMatch: (match: FloorballMatchDto) => void;
  onEditMatch: (match: FloorballMatchDto) => void;
  actionLoading: string | null;
  sectionType?: 'ongoing' | 'scheduled' | 'completed' | 'cancelled';
}

const CollapsibleMatchSection = ({
  title,
  matches,
  isCollapsed,
  onToggleCollapse,
  onLiveMatch,
  onEditMatch,
  actionLoading,
  sectionType
}: CollapsibleMatchSectionProps) => {
  if (matches.length === 0) {
    return null; // Don't render empty sections
  }

  return (
    <div className={`collapsible-match-section ${sectionType ? `${sectionType}-section` : ''}`}>
      <div 
        className="section-header"
        onClick={onToggleCollapse}
      >
        <div className="section-title">
          <span className="collapse-icon">
            {isCollapsed ? '▶' : '▼'}
          </span>
          {title}
        </div>
        <div className="section-count">
          {matches.length} {matches.length === 1 ? 'match' : 'matches'}
        </div>
      </div>
      
      {!isCollapsed && (
        <div className="section-content">
          <div className="matches-table-container">
            <table className="matches-table">
              <thead>
                <tr>
                  <th>Match</th>
                  <th>Date & Time</th>
                  <th>Venue</th>
                  <th>Score</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {matches.map((match: FloorballMatchDto) => (
                  <tr key={match.id}>
                    <td className="match-cell">
                      <div className="match-teams">
                        {match.homeTeamName} vs {match.awayTeamName}
                      </div>
                    </td>
                    <td className="date-cell">
                      {formatDateTime(match.scheduledDateTime)}
                    </td>
                    <td className="venue-cell">
                      {match.venue || <span className="tbd">TBD</span>}
                    </td>
                    <td className="score-cell">
                      {match.status === 'Scheduled' ? (
                        <span className="no-score">-</span>
                      ) : (
                        <span className="score">{match.homeScore} - {match.awayScore}</span>
                      )}
                    </td>
                    <td className="status-cell">
                      <span className={getStatusBadge(match.status)}>
                        {match.status}
                      </span>
                    </td>
                    <td className="actions-cell">
                      <div className="action-buttons">
                        <button
                          onClick={() => onLiveMatch(match)}
                          className={match.status === 'InProgress' ? "live-button" : "go-live-button"}
                          disabled={actionLoading !== null}
                        >
                          {match.status === 'InProgress' ? "🔴 Live" : "📊 Manage"}
                        </button>
                        <button
                          onClick={() => onEditMatch(match)}
                          className="edit-button"
                          disabled={actionLoading !== null}
                        >
                          ✏️ Edit
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
};

export default CollapsibleMatchSection; 