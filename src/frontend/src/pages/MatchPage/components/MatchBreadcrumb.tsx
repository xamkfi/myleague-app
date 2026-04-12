import { Link } from 'react-router-dom';
import './MatchBreadcrumb.scss';

interface MatchBreadcrumbProps {
  seasonName: string;
  competitionId: string;
}

export default function MatchBreadcrumb({ seasonName, competitionId }: MatchBreadcrumbProps) {
  return (
    <div className="match-breadcrumb">
      <Link to={`/league/${competitionId}`} className="season-link">
        {seasonName}
        <span className="arrow">›</span>
      </Link>
    </div>
  );
}