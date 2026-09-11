import type { ReactNode, MouseEvent } from 'react';
import { Link } from 'react-router-dom';
import '../SportLinks/SportLinks.scss';

interface AdminNameLinkProps {
  to: string;
  children: ReactNode;
  className?: string;
}

export default function AdminNameLink({ to, children, className }: AdminNameLinkProps) {
  return (
    <Link
      to={to}
      className={['sport-link', className].filter(Boolean).join(' ')}
      onClick={(event: MouseEvent<HTMLAnchorElement>) => {
        event.stopPropagation();
      }}
    >
      {children}
    </Link>
  );
}
