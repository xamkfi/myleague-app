import { useTranslation } from 'react-i18next';
import type { SportKind } from '../../utils/sportRoutes';
import './LiveMatchScoreboard.scss';

interface ScoreboardTeam {
  name: string;
}

interface LiveMatchScoreboardProps {
  sport: SportKind;
  leftTeam: ScoreboardTeam | null;
  rightTeam: ScoreboardTeam | null;
  leftScore: number;
  rightScore: number;
}

const LiveMatchScoreboard = ({
  sport,
  leftTeam,
  rightTeam,
  leftScore,
  rightScore
}: LiveMatchScoreboardProps) => {
  const { t } = useTranslation();
  const leftLeads: boolean = leftScore > rightScore;
  const rightLeads: boolean = rightScore > leftScore;

  return (
    <div className={`scoreboard scoreboard--${sport}`} aria-live="polite">
      <div className={`team-score${leftLeads ? ' team-score--leading' : ''}`}>
        <div className="team-name">{leftTeam?.name || t('matchManage.scoreboard.home', 'Home')}</div>
        <div className="score">{leftScore}</div>
      </div>
      <div className="score-separator" aria-hidden="true">–</div>
      <div className={`team-score${rightLeads ? ' team-score--leading' : ''}`}>
        <div className="team-name">{rightTeam?.name || t('matchManage.scoreboard.away', 'Away')}</div>
        <div className="score">{rightScore}</div>
      </div>
    </div>
  );
};

export default LiveMatchScoreboard;
