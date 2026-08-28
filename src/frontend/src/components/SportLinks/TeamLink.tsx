import type { MouseEvent, ReactNode } from 'react';
import { Link } from 'react-router-dom';
import { getTeamSlug, slugify } from '../../utils/slugUtils';
import { getTeamPath, type SportKind } from '../../utils/sportRoutes';
import './SportLinks.scss';

export interface NamedTeam {
  id: string;
  name: string;
}

interface TeamLinkProps {
  sport: SportKind;
  teamName: string;
  teamId?: string;
  teams?: NamedTeam[];
  slug?: string;
  className?: string;
  children?: ReactNode;
  onClick?: (event: MouseEvent<HTMLAnchorElement>) => void;
}

export default function TeamLink({
  sport,
  teamName,
  teamId,
  teams,
  slug,
  className,
  children,
  onClick,
}: TeamLinkProps) {
  if (!teamName && !slug) {
    return <span className={className}>{children}</span>;
  }

  const resolvedSlug =
    slug ??
    (teamId && teams
      ? getTeamSlug({ id: teamId, name: teamName }, teams)
      : slugify(teamName));

  return (
    <Link
      to={getTeamPath(sport, resolvedSlug)}
      className={['sport-link', className].filter(Boolean).join(' ')}
      onClick={(event) => {
        event.stopPropagation();
        onClick?.(event);
      }}
    >
      {children ?? teamName}
    </Link>
  );
}
