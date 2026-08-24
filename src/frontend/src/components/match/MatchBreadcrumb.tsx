import { Link } from 'react-router-dom';
import './MatchBreadcrumb.scss';

interface MatchBreadcrumbProps {
  competitionName: string;
  competitionPath: string;
}

export default function MatchBreadcrumb({
  competitionName,
  competitionPath,
}: MatchBreadcrumbProps) {
  return (
    <div className="match-breadcrumb">
      <Link to={competitionPath} className="season-link">
        {competitionName}
        <span className="arrow">›</span>
      </Link>
    </div>
  );
}
