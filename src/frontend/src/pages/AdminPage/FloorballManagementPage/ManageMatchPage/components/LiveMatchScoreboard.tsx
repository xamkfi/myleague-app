import type { FloorballTeam } from '../../../../../types/floorball/floorballTypes';

interface LiveMatchScoreboardProps {
  homeTeam: FloorballTeam | null;
  awayTeam: FloorballTeam | null;
  currentScore: { home: number; away: number };
}

const LiveMatchScoreboard = ({
  homeTeam,
  awayTeam,
  currentScore
}: LiveMatchScoreboardProps) => {
  return (
    <div className="scoreboard">
      <div className="team-score">
        <div className="team-name">{homeTeam?.name || 'Home'}</div>
        <div className="score">{currentScore.home}</div>
      </div>
      <div className="score-separator">-</div>
      <div className="team-score">
        <div className="team-name">{awayTeam?.name || 'Away'}</div>
        <div className="score">{currentScore.away}</div>
      </div>
    </div>
  );
};

export default LiveMatchScoreboard; 