import { FloorballMatchStatus, type FloorballMatchDto } from '../../../types/floorball/floorballTypes';
import { formatDate, getTeamInitials } from './matchUtils';

interface MatchHeaderProps {
  match: FloorballMatchDto;
}

export default function MatchHeader({ match }: MatchHeaderProps) {
  return (
    <div className="match-header">
      <div className="match-date-time">
        <div className="weekday">{formatDate(match.scheduledDateTime).weekday}</div>
        <div className="date-time">
          <span className="date">{formatDate(match.scheduledDateTime).date}</span>
          <span className="time">{formatDate(match.scheduledDateTime).time}</span>
        </div>
      </div>
      <div className="teams-container">
        <div className="team-section home">
          <div className="team-crest">
            {getTeamInitials(match.homeTeamName)}
            {match.homeTeamLogo && (
              <img 
                src={match.homeTeamLogo} 
                alt="Home Team Logo" 
                className="team-logo"
                onError={(e) => {
                  const target = e.target as HTMLImageElement;
                  target.style.display = 'none';
                }}
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
            {match.awayTeamLogo && (
              <img 
                src={match.awayTeamLogo} 
                alt="Away Team Logo" 
                className="team-logo"
                onError={(e) => {
                  const target = e.target as HTMLImageElement;
                  target.style.display = 'none';
                }}
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