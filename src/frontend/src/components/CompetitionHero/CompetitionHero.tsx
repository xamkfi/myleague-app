import type { ReactNode } from 'react';
import { resolveLogoUrl } from '../../utils/resolveLogoUrl';
import mahlLogo from '../../assets/logos/Mahl_primary_V3.svg';
import './CompetitionHero.scss';

interface CompetitionHeroProps {
  title: string;
  logoUrl?: string | null;
  meta?: ReactNode;
  children?: ReactNode;
}

export default function CompetitionHero({ title, logoUrl, meta, children }: CompetitionHeroProps) {
  const mark = resolveLogoUrl(logoUrl) ?? mahlLogo;

  return (
    <header className="competition-hero">
      <div className="competition-hero__banner">
        <div className="competition-hero__mark">
          <img src={mark} alt="" />
        </div>
        <div className="competition-hero__heading">
          <h1 className="competition-hero__title">{title}</h1>
          {meta}
        </div>
      </div>
      {children ? <div className="competition-hero__below">{children}</div> : null}
    </header>
  );
}
