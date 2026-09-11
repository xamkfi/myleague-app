import type { MouseEvent, ReactNode } from 'react';
import { Link } from 'react-router-dom';
import { getMatchPath, type SportKind } from '../../utils/sportRoutes';
import './SportLinks.scss';

interface MatchLinkProps {
  sport: SportKind;
  matchId: string;
  className?: string;
  children: ReactNode;
  onClick?: (event: MouseEvent<HTMLAnchorElement>) => void;
}

export default function MatchLink({
  sport,
  matchId,
  className,
  children,
  onClick,
}: MatchLinkProps) {
  if (!matchId) {
    return <span className={className}>{children}</span>;
  }

  return (
    <Link
      to={getMatchPath(sport, matchId)}
      className={['sport-link', className].filter(Boolean).join(' ')}
      onClick={(event) => {
        event.stopPropagation();
        onClick?.(event);
      }}
    >
      {children}
    </Link>
  );
}
