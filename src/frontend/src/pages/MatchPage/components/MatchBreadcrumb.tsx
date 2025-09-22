import { Link } from 'react-router-dom';
import './MatchBreadcrumb.scss';

interface MatchBreadcrumbProps {
  seasonName: string;
  seasonId: string;
}

export default function MatchBreadcrumb({ seasonName, seasonId }: MatchBreadcrumbProps) {
  return (
    <div className="match-breadcrumb">
      <Link to={`/league/${seasonId}`} className="season-link">
        {seasonName}
        <span className="arrow">›</span>
      </Link>
    </div>
  );
}