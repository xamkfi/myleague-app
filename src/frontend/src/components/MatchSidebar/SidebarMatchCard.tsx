import { useNavigate } from 'react-router-dom';
import type { FloorballMatchDto } from '../../types/floorball/floorballTypes';
import { FloorballMatchStatus } from '../../types/floorball/floorballTypes';

interface SidebarMatchCardProps {
  match: FloorballMatchDto;
}

function SidebarMatchCard({ match }: SidebarMatchCardProps) {
  const navigate = useNavigate();
  const isLive = match.status === FloorballMatchStatus.InProgress;

  const formatTime = (dateString: string): string => {
    const date = new Date(dateString);
    return date.toLocaleTimeString('fi-FI', {
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const formatDate = (dateString: string): string => {
    const date = new Date(dateString);
    const today = new Date();
    const tomorrow = new Date(today);
    tomorrow.setDate(tomorrow.getDate() + 1);

    if (date.toDateString() === today.toDateString()) {
      return 'Tänään';
    } else if (date.toDateString() === tomorrow.toDateString()) {
      return 'Huomenna';
    }
    
    return date.toLocaleDateString('fi-FI', {
      day: 'numeric',
      month: 'numeric'
    });
  };

  const handleClick = () => {
    navigate(`/match/${match.id}`);
  };

  return (
    <div className="sidebar-match-card" onClick={handleClick}>
      <div className="sidebar-match-card__season">
        {match.seasonName || 'No season'}
      </div>
      <div className="sidebar-match-card__header">
        {isLive && (
          <span className="sidebar-match-card__live-badge">
            <span className="live-dot" />
            LIVE
          </span>
        )}
        <span className="sidebar-match-card__time">
          {isLive ? formatTime(match.scheduledDateTime) : `${formatDate(match.scheduledDateTime)} ${formatTime(match.scheduledDateTime)}`}
        </span>
      </div>

      <div className="sidebar-match-card__teams">
        <div className="sidebar-match-card__team">
          <div className="sidebar-match-card__team-info">
            {match.homeTeamLogo ? (
              <img 
                src={match.homeTeamLogo} 
                alt={match.homeTeamName} 
                className="sidebar-match-card__logo"
                onError={(e) => {
                  const target = e.target as HTMLImageElement;
                  target.style.display = 'none';
                }}
              />
            ) : (
              <div className="sidebar-match-card__logo-placeholder" />
            )}
            <span className="sidebar-match-card__team-name">{match.homeTeamName}</span>
          </div>
          {(isLive || match.status === FloorballMatchStatus.Completed) && (
            <span className="sidebar-match-card__score">{match.homeScore}</span>
          )}
        </div>

        <div className="sidebar-match-card__team">
          <div className="sidebar-match-card__team-info">
            {match.awayTeamLogo ? (
              <img 
                src={match.awayTeamLogo} 
                alt={match.awayTeamName} 
                className="sidebar-match-card__logo"
                onError={(e) => {
                  const target = e.target as HTMLImageElement;
                  target.style.display = 'none';
                }}
              />
            ) : (
              <div className="sidebar-match-card__logo-placeholder" />
            )}
            <span className="sidebar-match-card__team-name">{match.awayTeamName}</span>
          </div>
          {(isLive || match.status === FloorballMatchStatus.Completed) && (
            <span className="sidebar-match-card__score">{match.awayScore}</span>
          )}
        </div>
      </div>

      {match.venue && (
        <div className="sidebar-match-card__venue">
          {match.venue}
        </div>
      )}

      <div className="sidebar-match-card__debug">
        <div className="sidebar-match-card__debug-title">DEBUG</div>
        <div className="sidebar-match-card__debug-row">
          <span>id:</span> <span>{match.id}</span>
        </div>
        <div className="sidebar-match-card__debug-row">
          <span>seasonId:</span> <span>{match.seasonId}</span>
        </div>
        <div className="sidebar-match-card__debug-row">
          <span>seasonName:</span> <span>"{match.seasonName}"</span>
        </div>
        <div className="sidebar-match-card__debug-row">
          <span>status:</span> <span>{match.status}</span>
        </div>
        <div className="sidebar-match-card__debug-row">
          <span>home:</span> <span>{match.homeTeamName} ({match.homeScore})</span>
        </div>
        <div className="sidebar-match-card__debug-row">
          <span>away:</span> <span>{match.awayTeamName} ({match.awayScore})</span>
        </div>
        <div className="sidebar-match-card__debug-row">
          <span>venue:</span> <span>{match.venue || 'null'}</span>
        </div>
        <div className="sidebar-match-card__debug-row">
          <span>scheduled:</span> <span>{match.scheduledDateTime}</span>
        </div>
        <div className="sidebar-match-card__debug-row">
          <span>rules:</span> <span>{match.matchRules ? `${match.matchRules.numberOfPeriods}x${match.matchRules.periodDurationMinutes}min` : 'null'}</span>
        </div>
      </div>
    </div>
  );
}

export default SidebarMatchCard;
