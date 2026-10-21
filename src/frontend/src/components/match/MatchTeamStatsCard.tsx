import type { ReactNode } from 'react';
import { getTeamInitials } from '../../pages/floorball/FloorballMatchPage/components/matchUtils';
import '../LeagueStanding/LeagueStanding.scss';
import './MatchTeamStatsCard.scss';

interface MatchTeamStatsCardProps {
  teamName: string;
  side: 'home' | 'away';
  children: ReactNode;
}

/** One team's stats table on a match page, headed by the team crest and name. */
export default function MatchTeamStatsCard({ teamName, side, children }: MatchTeamStatsCardProps) {
  return (
    <div className="standing-container match-team-stats">
      <div className="match-team-stats__header">
        <div className={`match-team-stats__crest match-team-stats__crest--${side}`} title={teamName}>
          {getTeamInitials(teamName)}
        </div>
        <span className="match-team-stats__name">{teamName}</span>
      </div>
      <div className="table-wrapper">{children}</div>
    </div>
  );
}
