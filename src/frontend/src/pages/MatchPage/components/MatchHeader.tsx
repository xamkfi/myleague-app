import { FloorballMatchStatus, type FloorballMatchDto } from '../../../types/floorball/floorballTypes';
import { formatDate, getTeamInitials } from './matchUtils';
import './MatchHeader.scss';

interface MatchHeaderProps {
  match: FloorballMatchDto;
}

export default function MatchHeader({ match }: MatchHeaderProps) {
  const scheduled = formatDate(match.scheduledDateTime);
  return (
    <div className="match-header">
      <div className="teams-container">
        <div className="team-section home">
          <div className="team-crest">
            {getTeamInitials(match.homeTeamName)}
            {match.homeTeamLogo && (
              <img 
                src={match.homeTeamLogo} 
                alt="Home Team Logo" 
                className="team-logo"
                loading="lazy"
                onError={(e) => {
                  const target = e.target as HTMLImageElement;
                  target.style.display = 'none';
                }}
              />
            )}
          </div>
          <div className="team-name">{match.homeTeamName}</div>
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
                loading="lazy"
                onError={(e) => {
                  const target = e.target as HTMLImageElement;
                  target.style.display = 'none';
                }}
              />
            )}
          </div>
          <div className="team-name">{match.awayTeamName}</div>
        </div>
      </div>

      <div className="match-date-time">
        <span className="weekday">{scheduled.weekday}</span>
        <span className="separator">·</span>
        <span className="date">{scheduled.date}</span>
        <span className="separator">·</span>
        <span className="time">{scheduled.time}</span>
      </div>

      {match.status === FloorballMatchStatus.InProgress && (
        <div className="match-status live">
          <span className="status-dot" aria-label="Live match" />
          <span>LIVE</span>
        </div>
      )}
      
      {match.status === FloorballMatchStatus.Completed && (
        <div className="match-status final">
          <span>FINAL</span>
        </div>
      )}
    </div>
  );
} 