import type { ReactNode } from 'react';
import { resolveLogoUrl } from '../../utils/resolveLogoUrl';
import mahlLogo from '../../assets/logos/Mahl_primary_V3.svg';
import bannerImage from '../../assets/floorball-banner.png';
import './CompetitionHero.scss';

interface CompetitionHeroProps {
  title: string;
  logoUrl?: string | null;
  markLabel?: string;
  meta?: ReactNode;
  children?: ReactNode;
}

export default function CompetitionHero({ title, logoUrl, markLabel, meta, children }: CompetitionHeroProps) {
  const resolvedLogo = resolveLogoUrl(logoUrl);
  const label = markLabel?.trim() ? markLabel.trim() : null;
  const mark = resolvedLogo ?? (label ? null : mahlLogo);

  return (
    <header className="competition-hero">
      <div className="competition-hero__banner">
        <img
          className="competition-hero__photo"
          src={bannerImage}
          alt=""
          aria-hidden="true"
        />
        <div className="competition-hero__content">
          <div className="competition-hero__mark">
            {mark ? <img src={mark} alt="" /> : <span className="competition-hero__mark-label">{label}</span>}
          </div>
          <div className="competition-hero__heading">
            <h1 className="competition-hero__title">{title}</h1>
            {meta}
          </div>
        </div>
      </div>
      {children ? <div className="competition-hero__below">{children}</div> : null}
    </header>
  );
}
