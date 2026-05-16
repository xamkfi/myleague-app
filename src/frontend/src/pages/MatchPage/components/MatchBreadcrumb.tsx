import { Link } from 'react-router-dom';
import { getCompetitionPath, type CompetitionRouteHints } from '../../../utils/competitionPath';
import './MatchBreadcrumb.scss';

interface MatchBreadcrumbProps {
  /** Display label of the competition (season name or tournament name). */
  competitionName: string;
  /** Competition GUID (TPH-shared between seasons and tournaments). */
  competitionId: string;
  /**
   * Tournament-flavour hints from the match DTO; used to route tournament matches to
   * `/tournaments/{id}` instead of the default `/league/{id}` season route.
   */
  hints?: CompetitionRouteHints;
}

export default function MatchBreadcrumb({ competitionName, competitionId, hints }: MatchBreadcrumbProps) {
  const competitionPath: string = getCompetitionPath(competitionId, hints);
  return (
    <div className="match-breadcrumb">
      <Link to={competitionPath} className="season-link">
        {competitionName}
        <span className="arrow">›</span>
      </Link>
    </div>
  );
}
