import type { FloorballMatchDto } from '../../../../../../types/floorball/floorballTypes';
import { formatDateTime, getStatusBadge } from '../../../ManageMatchPage/utils/matchFormatters';
import './CollapsibleMatchSection.scss';

interface CollapsibleMatchSectionProps {
  title: string;
  matches: FloorballMatchDto[];
  isCollapsed: boolean;
  onToggleCollapse: () => void;
  onLiveMatch: (match: FloorballMatchDto) => void;
  onEditMatch: (match: FloorballMatchDto) => void;
  sectionType?: 'ongoing' | 'scheduled' | 'completed' | 'cancelled';
}

const CollapsibleMatchSection = ({
  title,
  matches,
  isCollapsed,
  onToggleCollapse,
  onLiveMatch,
  onEditMatch,
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
                    <td className="match-cell clickable-cell" onClick={() => onLiveMatch(match)}>
                      <div className="match-teams">
                        <div className="team-names">
                          <div className="team-name team-name--home">{match.homeTeamName}</div>
                          <div className="team-name team-name--away">{match.awayTeamName}</div>
                        </div>
                        <div className="vs-badge">VS</div>
                      </div>
                    </td>
                    <td className="date-cell clickable-cell" onClick={() => onLiveMatch(match)}>
                      {formatDateTime(match.scheduledDateTime)}
                    </td>
                    <td className="venue-cell clickable-cell" onClick={() => onLiveMatch(match)}>
                      {match.venue || <span className="tbd">TBD</span>}
                    </td>
                    <td className="score-cell clickable-cell" onClick={() => onLiveMatch(match)}>
                      {match.status === 'Scheduled' ? (
                        <span className="no-score">-</span>
                      ) : (
                        <span className="score">{match.homeScore} - {match.awayScore}</span>
                      )}
                    </td>
                    <td className="status-cell clickable-cell" onClick={() => onLiveMatch(match)}>
                      <span className={getStatusBadge(match.status)}>
                        {match.status}
                      </span>
                    </td>
                    <td className="actions-cell">
                      <div className="action-buttons">
                        <button
                          onClick={() => onLiveMatch(match)}
                          className={match.status === 'InProgress' ? "live-button" : "go-live-button"}
                        >
                          {match.status === 'InProgress' ? "🔴 Live" : "📊 Manage"}
                        </button>
                        <button
                          onClick={() => onEditMatch(match)}
                          className="edit-button"
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