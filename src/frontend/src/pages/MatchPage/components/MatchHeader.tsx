import { FloorballMatchStatus, type FloorballMatchDto } from '../../../types/floorball/floorballTypes';
import { formatDate, getTeamInitials } from './matchUtils';

interface MatchHeaderProps {
  match: FloorballMatchDto;
}

export default function MatchHeader({ match }: MatchHeaderProps) {
  return (
    <div className="match-header">
      <div className="match-date-time">{formatDate(match.scheduledDateTime)}</div>
      
      <div className="teams-container">
        <div className="team-section home">
          <div className="team-crest">
            {getTeamInitials(match.homeTeamName)}
            {match.homeClub && match.homeClub.logoUrl && (
              <img 
                src={match.homeClub.logoUrl} 
                alt="Home Team Logo" 
                className="team-logo"
              />
            )}
          </div>
          <div className="team-info">
            <div className="team-name">{match.homeTeamName}</div>
          </div>
        </div>

        <div className="score-container">
          {match.status === FloorballMatchStatus.Scheduled ? (
            <div className="vs-separator">VS</div>
          ) : (
            <div className="match-score">
              <span className="home-score">{match.homeScore}</span>
              <span className="score-separator">—</span>
              <span className="away-score">{match.awayScore}</span>
            </div>
          )}
        </div>

        <div className="team-section away">
          <div className="team-crest">
            {getTeamInitials(match.awayTeamName)}
            {match.awayClub && match.awayClub.logoUrl && (
              <img 
                src={match.awayClub.logoUrl} 
                alt="Away Team Logo" 
                className="team-logo"
              />
            )}
          </div>
          <div className="team-info">
            <div className="team-name">{match.awayTeamName}</div>
          </div>
        </div>
      </div>

      {match.status === FloorballMatchStatus.InProgress && (
        <div className="match-status">
          <span className="status-indicator">🔴</span>
          <span>LIVE</span>
        </div>
      )}
      
      {match.status === FloorballMatchStatus.Completed && (
        <div className="match-status">
          <span className="status-indicator">✅</span>
          <span>FINAL</span>
        </div>
      )}
    </div>
  );
} 