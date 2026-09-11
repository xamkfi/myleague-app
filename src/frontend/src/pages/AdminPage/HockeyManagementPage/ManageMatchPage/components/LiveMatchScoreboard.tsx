import './LiveMatchScoreboard.scss';

interface NamedTeam {
  name: string;
}

interface LiveMatchScoreboardProps {
  leftTeam: NamedTeam | null;
  rightTeam: NamedTeam | null;
  leftScore: number;
  rightScore: number;
}

function LiveMatchScoreboard({
  leftTeam,
  rightTeam,
  leftScore,
  rightScore,
}: LiveMatchScoreboardProps) {
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
}

export default LiveMatchScoreboard;
