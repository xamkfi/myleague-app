import React from 'react';
import './MatchRow.scss';
import ResultUnknown from '../MatchResultIcons/ResultUnknown';
import { formatMatchDateTime } from '../../utils/helpers';

export interface MatchRowProps {
  id: string;
  scheduledDateTime: string;
  homeTeamName: string;
  awayTeamName: string;
  homeTeamLogo?: string;
  awayTeamLogo?: string;
  homeScore?: number;
  awayScore?: number;
  periodCount?: number;
  statusComponent?: React.ReactNode;
  onClick?: () => void;
  className?: string;
}

export default function MatchRow({
  id,
  scheduledDateTime,
  homeTeamName,
  awayTeamName,
  homeTeamLogo,
  awayTeamLogo,
  homeScore = 0,
  awayScore = 0,
  periodCount = 3,
  statusComponent,
  onClick,
  className = ''
}: MatchRowProps) {
  const [formattedDate, formattedTime] = formatMatchDateTime(scheduledDateTime);
  const periods = Array.from({ length: periodCount }, (_, i) => i + 1);

  const handleClick = () => {
    if (onClick) {
      onClick();
    }
  };

  return (
    <div
      className={`match-row ${className}`}
      onClick={handleClick}
    >
      {/* Date */}
      <div className="match-row-date">
        <div className="match-row-date-day">{formattedDate}</div>
        <div className="match-row-date-time">{formattedTime}</div>
      </div>

      {/* Teams */}
      <div className="match-row-teams-container">
        <div className="match-row-home-team">
          {homeTeamLogo && (
            <img src={homeTeamLogo} alt={`${homeTeamName} logo`} />
          )}
          {homeTeamName}
        </div>

        <div className="match-row-away-team">
          {awayTeamLogo && (
            <img src={awayTeamLogo} alt={`${awayTeamName} logo`} />
          )}
          {awayTeamName}
        </div>
      </div>

      {/* Total score */}
      <div className="match-row-total-score-container">
        <div className="match-row-home-total-score">
          {homeScore > 0 ? homeScore : '-'}
        </div>
        <div className="match-row-away-total-score">
          {awayScore > 0 ? awayScore : '-'}
        </div>
      </div>

      {/* Period score */}
      <div className="match-row-period-score-container">
        {periods.map((period) => (
          <div key={period} className="match-row-period">
            <div className="match-row-home-period-score">
              {homeScore > 0 ? homeScore : ''}
            </div>
            <div className="match-row-away-period-score">
              {awayScore > 0 ? awayScore : ''}
            </div>
          </div>
        ))}
      </div>

      {/* Match status */}
      <div className="match-row-status">
        {statusComponent || <ResultUnknown />}
      </div>
    </div>
  );
} 