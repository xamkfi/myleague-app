import { useTranslation } from 'react-i18next';
import type { FloorballTeam } from '../../../../../types/floorball/floorballTypes';
import './LiveMatchScoreboard.scss';

interface LiveMatchScoreboardProps {
  leftTeam: FloorballTeam | null;
  rightTeam: FloorballTeam | null;
  leftScore: number;
  rightScore: number;
}

const LiveMatchScoreboard = ({
  leftTeam,
  rightTeam,
  leftScore,
  rightScore
}: LiveMatchScoreboardProps) => {
  const { t } = useTranslation();
  const leftLeads: boolean = leftScore > rightScore;
  const rightLeads: boolean = rightScore > leftScore;

  return (
    <div className="scoreboard" aria-live="polite">
      <div className={`team-score${leftLeads ? ' team-score--leading' : ''}`}>
        <div className="team-name">{leftTeam?.name || t('floorball.matches.manage.scoreboard.home', 'Home')}</div>
        <div className="score">{leftScore}</div>
      </div>
      <div className="score-separator" aria-hidden="true">–</div>
      <div className={`team-score${rightLeads ? ' team-score--leading' : ''}`}>
        <div className="team-name">{rightTeam?.name || t('floorball.matches.manage.scoreboard.away', 'Away')}</div>
        <div className="score">{rightScore}</div>
      </div>
    </div>
  );
};

export default LiveMatchScoreboard;
