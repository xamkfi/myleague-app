import React from 'react';
import './MatchRow.scss';
import ResultUnknown from '../MatchResultIcons/ResultUnknown';
import { formatMatchDateTime } from '../../utils/helpers';
import { useNavigate } from 'react-router-dom';

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
  periodScores?: Record<number, { homeScore: number; awayScore: number }>;
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
  periodScores,
  statusComponent,
  className = ''
}: MatchRowProps) {
  const [formattedDate, formattedTime] = formatMatchDateTime(scheduledDateTime);
  const computedPeriodCount = periodScores ? Math.max(...Object.keys(periodScores).map(k => Number(k))) : periodCount;
  const periods = Array.from({ length: computedPeriodCount }, (_, i) => i + 1);
  const navigate = useNavigate();

  const handleClick = () => {
//    if (onClick) {
//      onClick();
//    } else {
      navigate(`/match/${id}`);
//    }
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
              {periodScores && periodScores[period] ? periodScores[period].homeScore : ''}
            </div>
            <div className="match-row-away-period-score">
              {periodScores && periodScores[period] ? periodScores[period].awayScore : ''}
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