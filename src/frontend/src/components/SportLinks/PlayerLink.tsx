import type { MouseEvent, ReactNode } from 'react';
import { Link } from 'react-router-dom';
import { getPlayerPath, type SportKind } from '../../utils/sportRoutes';
import './SportLinks.scss';

interface PlayerLinkProps {
  sport: SportKind;
  playerId: string;
  className?: string;
  children: ReactNode;
  onClick?: (event: MouseEvent<HTMLAnchorElement>) => void;
}

export default function PlayerLink({
  sport,
  playerId,
  className,
  children,
  onClick,
}: PlayerLinkProps) {
  if (!playerId) {
    return <span className={className}>{children}</span>;
  }

  return (
    <Link
      to={getPlayerPath(sport, playerId)}
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
