import { useNavigate } from 'react-router-dom';
import { FloorballMatchStatus, type FloorballMatchDto } from '../../../types/floorball/floorballTypes';
import { formatDate, getTeamInitials } from './matchUtils';
import { slugify } from '../../../utils/slugUtils';
import './MatchHeader.scss';

interface MatchHeaderProps {
  match: FloorballMatchDto;
}

export default function MatchHeader({ match }: MatchHeaderProps) {
  const navigate = useNavigate();
  const scheduled = formatDate(match.scheduledDateTime);

  const handleTeamClick = (teamName: string) => {
    navigate(`/team/${slugify(teamName)}`);
  };

  return (
    <div className="match-header">
      <div className="teams-container">
        <div
          className="team-section home clickable"
          role="link"
          tabIndex={0}
          onClick={() => handleTeamClick(match.homeTeamName)}
          onKeyDown={(e) => { if (e.key === 'Enter') handleTeamClick(match.homeTeamName); }}
        >
          <div className="team-crest">
            {getTeamInitials(match.homeTeamName)}
            {match.homeTeamLogo && (
              <img 
                src={match.homeTeamLogo} 
                alt={`${match.homeTeamName} logo`}
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

        <div
          className="team-section away clickable"
          role="link"
          tabIndex={0}
          onClick={() => handleTeamClick(match.awayTeamName)}
          onKeyDown={(e) => { if (e.key === 'Enter') handleTeamClick(match.awayTeamName); }}
        >
          <div className="team-crest">
            {getTeamInitials(match.awayTeamName)}
            {match.awayTeamLogo && (
              <img 
                src={match.awayTeamLogo} 
                alt={`${match.awayTeamName} logo`}
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