import type { FootballTeam } from '../../../../../types/football/footballTypes';
import './LiveMatchScoreboard.scss';

interface LiveMatchScoreboardProps {
  leftTeam: FootballTeam | null;
  rightTeam: FootballTeam | null;
  leftScore: number;
  rightScore: number;
}

const LiveMatchScoreboard = ({
  leftTeam,
  rightTeam,
  leftScore,
  rightScore
}: LiveMatchScoreboardProps) => {
  return (
    <div className="scoreboard">
      <div className="team-score">
        <div className="team-name">{leftTeam?.name || 'Home'}</div>
        <div className="score">{leftScore}</div>
      </div>
      <div className="score-separator">-</div>
      <div className="team-score">
        <div className="team-name">{rightTeam?.name || 'Away'}</div>
        <div className="score">{rightScore}</div>
      </div>
    </div>
  );
};

export default LiveMatchScoreboard; 